using UnityEngine;

[DisallowMultipleComponent]
public class EdgePanCameraZ : MonoBehaviour
{
    [Header("Bordes de pantalla")]
    [SerializeField] private int edgeThickness = 16;             // píxeles

    [Header("Movimiento")]
    [SerializeField] private float panSpeed = 4f;                 // u/s pegado al borde
    [SerializeField] private float returnSpeed = 1.2f;            // u/s retorno fuera de bordes
    [Range(0f, 1f)]
    [SerializeField] private float maxForwardOffsetPercent = 0.30f; // 30% del ancho visible
    [Tooltip("1 = lineal; 2+ = más suave al entrar, más fuerte pegado al borde.")]
    [SerializeField] private float edgeEasePower = 2f;

    [Header("Perspectiva (si NO es ortográfica)")]
    [Tooltip("Plano/objeto de referencia para medir el ancho visible a esa profundidad (opcional).")]
    [SerializeField] private Transform focusAtDepth;
    [Tooltip("Si no hay foco, distancia (m) delante de la cámara para calcular el ancho visible.")]
    [SerializeField] private float fallbackFocusDistance = 10f;

    [Header("Oscilación sutil (no interfiere con paneo)")]
    [SerializeField] private bool oscillationEnabled = true;
    [Tooltip("Oscilación en X/Y (m).")]
    [SerializeField] private float oscAmplitudeXY = 0.05f;
    [Tooltip("Oscilación en Z (m). Mantener bajo o en 0 para no empujar los límites.")]
    [SerializeField] private float oscAmplitudeZ = 0.00f;
    [Tooltip("Velocidad de la oscilación.")]
    [SerializeField] private float oscSpeed = 0.5f;
    [Tooltip("Reduce la oscilación Z mientras se panea (0 = anula Z al panear, 1 = sin reducción).")]
    [Range(0f, 1f)] [SerializeField] private float oscZWhilePanningFactor = 0f;

    private Camera cam;

    // Base/origen:
    private Vector3 baseLocalPos;       // Posición local inicial (X/Y/Z)
    private float startLocalZ;          // Origen Z relativo al padre (para límites)
    private float forwardLimitLocalZ;   // Tope +Z relativo al padre

    // Estado de paneo independiente de la oscilación:
    private float panZActual;           // Z local controlada por paneo/retorno
    private bool estaPaneando;          // true si mouse está en cualquiera de los bordes

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) Debug.LogWarning("[EdgePanCameraZ] Debe estar en un objeto con Camera.");
    }

    void Start()
    {
        baseLocalPos = transform.localPosition;
        startLocalZ = baseLocalPos.z;
        panZActual = startLocalZ;
        RecalcularTopeAdelanteLocal();
    }

    void Update()
    {
        // Por si cambian FOV/aspect/orthoSize/escala en runtime
        RecalcularTopeAdelanteLocal();

        // --- Bloquea durante tutorial temprano o movimiento de caravana
        if (CampaignManager.Instance != null)
        {
            if (CampaignManager.Instance.scTutorialManager.tutorialActivo &&
                CampaignManager.Instance.scTutorialManager.pasoActual < 7)
            {
                AplicarFinalPosicion(); // solo actualiza oscilación visual sin cambiar paneo
                return;
            }
            if (CampaignManager.Instance.MoviendoCaravana)
            {
                AplicarFinalPosicion();
                return;
            }
        }

        float mouseX = Input.mousePosition.x;
        bool enBordeIzq = mouseX <= edgeThickness;
        bool enBordeDer = mouseX >= (Screen.width - edgeThickness);
        estaPaneando = enBordeIzq || enBordeDer;

        // ----- Paneo SOLO modifica panZActual (no toca X/Y ni la base)
        if (enBordeDer)
        {
            float t = Mathf.InverseLerp(Screen.width - edgeThickness, Screen.width, mouseX); // 0..1
            float factor = Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(0.1f, edgeEasePower));
            float step = panSpeed * factor * Time.deltaTime; // +Z
            panZActual = Mathf.Clamp(panZActual + step, startLocalZ, forwardLimitLocalZ);
        }
        else if (enBordeIzq)
        {
            float t = Mathf.InverseLerp(edgeThickness, 0f, mouseX); // 0..1 (1 pegado a 0 px)
            float factor = Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(0.1f, edgeEasePower));
            float step = panSpeed * factor * Time.deltaTime; // −Z
            panZActual = Mathf.Clamp(panZActual - step, startLocalZ, forwardLimitLocalZ);
        }
        else
        {
            // Auto-retorno suave al origen cuando NO está en bordes
            panZActual = Mathf.MoveTowards(panZActual, startLocalZ, returnSpeed * Time.deltaTime);
        }

        // Aplica resultado (paneo + oscilación desacoplada)
        AplicarFinalPosicion();
    }

    // Compone paneo (Z) + oscilación (X/Y y opcional Z) sin violar límites
    private void AplicarFinalPosicion()
    {
        Vector3 osc = Vector3.zero;

        if (oscillationEnabled)
        {
            float t = Time.time * oscSpeed;

            // X/Y siempre sutiles y no restringidos (no afectan límites de Z):
            float ox = Mathf.Sin(t) * oscAmplitudeXY;
            float oy = Mathf.Cos(t * 0.5f) * oscAmplitudeXY;

            // Z opcional y atenuada mientras se panea:
            float zFactor = estaPaneando ? oscZWhilePanningFactor : 1f;
            float oz = Mathf.Sin(t * 0.8f) * oscAmplitudeZ * zFactor;

            // Ojo: la suma en Z se clampa para no cruzar límites
            float zFinal = Mathf.Clamp(panZActual + oz, startLocalZ, forwardLimitLocalZ);

            // Componer final
            transform.localPosition = new Vector3(
                baseLocalPos.x + ox,
                baseLocalPos.y + oy,
                zFinal
            );
        }
        else
        {
            transform.localPosition = new Vector3(
                baseLocalPos.x,
                baseLocalPos.y,
                panZActual
            );
        }
    }

    /// Calcula el tope +Z en LOCAL a partir del ancho visible (mundo) y la escala del padre.
    private void RecalcularTopeAdelanteLocal()
    {
        float worldWidth = CalcularAnchoVisibleMundo(); // unidades de mundo
        float offsetForwardWorld = worldWidth * Mathf.Clamp01(maxForwardOffsetPercent);

        float parentScaleZ = 1f;
        if (transform.parent != null)
            parentScaleZ = Mathf.Abs(transform.parent.lossyScale.z);

        float offsetForwardLocal = offsetForwardWorld / Mathf.Max(0.0001f, parentScaleZ);
        forwardLimitLocalZ = startLocalZ + offsetForwardLocal;

        // Por si el límite cambió y el paneo quedó fuera:
        panZActual = Mathf.Clamp(panZActual, startLocalZ, forwardLimitLocalZ);
    }

    /// Ancho visible en unidades de mundo al plano elegido.
    private float CalcularAnchoVisibleMundo()
    {
        if (!cam) return 0f;

        if (cam.orthographic)
        {
            return 2f * cam.orthographicSize * cam.aspect;
        }
        else
        {
            float distance = fallbackFocusDistance;
            if (focusAtDepth)
            {
                Vector3 camToFocus = focusAtDepth.position - cam.transform.position;
                distance = Mathf.Max(0.01f, Vector3.Dot(camToFocus, cam.transform.forward));
            }

            float heightAtDist = 2f * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
            return heightAtDist * cam.aspect;
        }
    }

    /// Recentrado manual al origen relativo al padre (resetea paneo, no la oscilación).
    public void RecentrarLocal()
    {
        panZActual = startLocalZ;
        AplicarFinalPosicion();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Transform p = transform.parent;
        Vector3 lp = transform.localPosition;

        Vector3 localStart  = new Vector3(lp.x, lp.y, startLocalZ);
        Vector3 localTope   = new Vector3(lp.x, lp.y, forwardLimitLocalZ);

        Vector3 worldStart  = p ? p.TransformPoint(localStart) : transform.TransformPoint(Vector3.zero);
        Vector3 worldTope   = p ? p.TransformPoint(localTope)  : transform.TransformPoint(Vector3.zero);

        Gizmos.color = Color.cyan;   Gizmos.DrawSphere(worldStart, 0.05f);
        Gizmos.color = Color.yellow; Gizmos.DrawSphere(worldTope,  0.05f);
    }
#endif
}
