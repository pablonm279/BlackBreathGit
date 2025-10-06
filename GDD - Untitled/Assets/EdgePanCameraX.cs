using UnityEngine;

/// Paneo por bordes SOLO en eje Z local (derecha = Z subiendo).
/// Origen y tope son RELATIVOS AL PADRE (caravana).
/// - Borde derecho: mueve +Z mientras el mouse esté en ese borde (tope = % del ancho visible).
/// - Borde izquierdo: mueve hacia el origen (−Z) mientras esté en ese borde.
/// - Fuera de bordes: retorno suave hacia el origen.
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

    private Camera cam;
    private float startLocalZ;          // origen relativo al padre
    private float forwardLimitLocalZ;   // tope +Z relativo al padre

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam) Debug.LogWarning("[EdgePanCameraZ] Debe estar en un objeto con Camera.");
    }

    void Start()
    {
        startLocalZ = transform.localPosition.z;
        RecalcularTopeAdelanteLocal();
    }

    void Update()
    {
        // Por si cambian FOV/aspect/orthoSize/escala en runtime
        RecalcularTopeAdelanteLocal();

        float mouseX = Input.mousePosition.x;
        bool enBordeIzq = mouseX <= edgeThickness;
        bool enBordeDer = mouseX >= (Screen.width - edgeThickness);

        Vector3 lp = transform.localPosition;

        if (enBordeDer && !CampaignManager.Instance.MoviendoCaravana)
        {
            // Factor por proximidad al borde derecho
            float t = Mathf.InverseLerp(Screen.width - edgeThickness, Screen.width, mouseX); // 0..1
            float factor = Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(0.1f, edgeEasePower));
            float step = panSpeed * factor * Time.deltaTime; // +Z

            lp.z = Mathf.Clamp(lp.z + step, startLocalZ, forwardLimitLocalZ);
            transform.localPosition = lp;
        }
        else if (enBordeIzq)
        {
            // Factor por proximidad al borde izquierdo
            float t = Mathf.InverseLerp(edgeThickness, 0f, mouseX); // 0..1 (1 pegado a 0 px)
            float factor = Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(0.1f, edgeEasePower));
            float step = panSpeed * factor * Time.deltaTime; // −Z (volver al origen)

            lp.z = Mathf.Clamp(lp.z - step, startLocalZ, forwardLimitLocalZ);
            transform.localPosition = lp;
        }
        else
        {
            // Auto-retorno suave al origen cuando NO está en bordes
            lp.z = Mathf.MoveTowards(lp.z, startLocalZ, returnSpeed * Time.deltaTime);
            transform.localPosition = lp;
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

    /// Recentrado manual al origen relativo al padre.
    public void RecentrarLocal()
    {
        Vector3 lp = transform.localPosition;
        lp.z = startLocalZ;
        transform.localPosition = lp;
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
