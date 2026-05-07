using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class EdgePanCameraZ : MonoBehaviour
{
    private const float ZoomWheelStepMultiplier = 0.9f;
    private const float ZoomSmoothMultiplier = 1.35f;
    private const float ZoomOutRange = 0.15f;
    private const float ZoomInOffsetMultiplier = 1.10f;

    [Header("Bordes de pantalla")]
    [SerializeField] private int edgeThickness = 16;

    [Header("Movimiento")]
    [SerializeField] private float panSpeed = 4f;
    [SerializeField] private float returnSpeed = 1.2f;
    [Range(0f, 1f)]
    [SerializeField] private float maxForwardOffsetPercent = 0.30f;
    [SerializeField] private float edgeEasePower = 2f;

    [Header("Perspectiva (si NO es ortografica)")]
    [SerializeField] private Transform focusAtDepth;
    [SerializeField] private float fallbackFocusDistance = 10f;

    [Header("Oscilacion sutil (no interfiere con paneo)")]
    [SerializeField] private bool oscillationEnabled = true;
    [SerializeField] private float oscAmplitudeXY = 0.05f;
    [SerializeField] private float oscAmplitudeZ = 0.00f;
    [SerializeField] private float oscSpeed = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float oscZWhilePanningFactor = 0f;

    [Header("Zoom con rueda")]
    [SerializeField] private bool zoomEnabled = true;
    [SerializeField] private float zoomWheelStep = 0.18f;
    [SerializeField] private float zoomSmoothTime = 0.22f;
    [SerializeField] private float zoomInFovDelta = -10f;
    [SerializeField] private float zoomInPitchUp = 6.5f;
    [SerializeField] private float zoomInForwardOffset = 7f;
    [SerializeField] private float zoomInDownOffset = 2f;

    private Camera cam;
    private Vector3 baseLocalPos;
    private Quaternion baseLocalRotation;
    private float startLocalZ;
    private float forwardLimitLocalZ;
    private float panZActual;
    private bool estaPaneando;
    private float zoomObjetivo;
    private float zoomActual;
    private float zoomVelocidadActual;
    private float zoomVisualActual;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam)
        {
            Debug.LogWarning("[EdgePanCameraZ] Debe estar en un objeto con Camera.");
        }
    }

    void Start()
    {
        baseLocalPos = transform.localPosition;
        baseLocalRotation = transform.localRotation;
        startLocalZ = baseLocalPos.z;
        panZActual = startLocalZ;
        RecalcularTopeAdelanteLocal();
        AplicarZoomVisual();
    }

    void Update()
    {
        bool bloquearMovimiento = false;

        if (CampaignManager.Instance != null)
        {
            if (CampaignManager.Instance.scTutorialManager.tutorialActivo &&
                CampaignManager.Instance.scTutorialManager.pasoActual < 7)
            {
                bloquearMovimiento = true;
            }
            else if (CampaignManager.Instance.MoviendoCaravana)
            {
                bloquearMovimiento = true;
            }
        }

        ActualizarZoom(bloquearMovimiento);
        RecalcularTopeAdelanteLocal();

        if (bloquearMovimiento)
        {
            AplicarFinalPosicion();
            return;
        }

        float mouseX = Input.mousePosition.x;
        bool enBordeIzq = mouseX <= edgeThickness;
        bool enBordeDer = mouseX >= (Screen.width - edgeThickness);
        estaPaneando = enBordeIzq || enBordeDer;

        if (enBordeDer)
        {
            float t = Mathf.InverseLerp(Screen.width - edgeThickness, Screen.width, mouseX);
            float factor = Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(0.1f, edgeEasePower));
            float step = panSpeed * factor * Time.deltaTime;
            panZActual = Mathf.Clamp(panZActual + step, startLocalZ, forwardLimitLocalZ);
        }
        else if (enBordeIzq)
        {
            float t = Mathf.InverseLerp(edgeThickness, 0f, mouseX);
            float factor = Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(0.1f, edgeEasePower));
            float step = panSpeed * factor * Time.deltaTime;
            panZActual = Mathf.Clamp(panZActual - step, startLocalZ, forwardLimitLocalZ);
        }
        else
        {
            panZActual = Mathf.MoveTowards(panZActual, startLocalZ, returnSpeed * Time.deltaTime);
        }

        AplicarFinalPosicion();
    }

    private void ActualizarZoom(bool bloquearMovimiento)
    {
        if (!zoomEnabled)
        {
            zoomObjetivo = 0f;
        }
        else if (!bloquearMovimiento && !EstaMouseSobreUI())
        {
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.001f)
            {
                float pasoZoom = zoomWheelStep * ZoomWheelStepMultiplier;
                zoomObjetivo = Mathf.Clamp(zoomObjetivo - (wheel * pasoZoom), -1f, ZoomOutRange);
            }
        }

        zoomActual = Mathf.SmoothDamp(
            zoomActual,
            zoomObjetivo,
            ref zoomVelocidadActual,
            Mathf.Max(0.01f, zoomSmoothTime * ZoomSmoothMultiplier));
        AplicarZoomVisual();
    }

    private void AplicarZoomVisual()
    {
        if (zoomActual < 0f)
        {
            zoomVisualActual = -Mathf.SmoothStep(0f, 1f, -zoomActual);
        }
        else
        {
            float zoomOutNormalizado = ZoomOutRange <= 0.001f ? 0f : Mathf.Clamp01(zoomActual / ZoomOutRange);
            zoomVisualActual = Mathf.SmoothStep(0f, 1f, zoomOutNormalizado) * ZoomOutRange;
        }

        // El zoom se resuelve solo con offset/rotacion para no alterar el FOV.
        float pitchDelta = 0f;
        if (zoomVisualActual < 0f)
        {
            pitchDelta = Mathf.Max(0f, -zoomVisualActual) * zoomInPitchUp;
        }
        else if (zoomVisualActual > 0f)
        {
            float zoomOutNormalizado = ZoomOutRange <= 0.001f ? 0f : Mathf.Clamp01(zoomVisualActual / ZoomOutRange);
            pitchDelta = -zoomOutNormalizado * (zoomInPitchUp * ZoomOutRange);
        }

        transform.localRotation = baseLocalRotation * Quaternion.Euler(-pitchDelta, 0f, 0f);
    }

    private void AplicarFinalPosicion()
    {
        if (oscillationEnabled)
        {
            float t = Time.time * oscSpeed;
            float ox = Mathf.Sin(t) * oscAmplitudeXY;
            float oy = Mathf.Cos(t * 0.5f) * oscAmplitudeXY;
            float zFactor = estaPaneando ? oscZWhilePanningFactor : 1f;
            float oz = Mathf.Sin(t * 0.8f) * oscAmplitudeZ * zFactor;
            float zFinal = Mathf.Clamp(panZActual + oz, startLocalZ, forwardLimitLocalZ);

            Vector3 zoomOffset = CalcularOffsetZoomLocal();
            transform.localPosition = new Vector3(
                baseLocalPos.x + ox,
                baseLocalPos.y + oy,
                zFinal
            ) + zoomOffset;
        }
        else
        {
            transform.localPosition = new Vector3(
                baseLocalPos.x,
                baseLocalPos.y,
                panZActual
            ) + CalcularOffsetZoomLocal();
        }
    }

    private Vector3 CalcularOffsetZoomLocal()
    {
        if (zoomVisualActual < 0f)
        {
            float t = -zoomVisualActual;
            float avanceT = Mathf.SmoothStep(0f, 1f, t);
            float bajadaT = 1f - ((1f - t) * (1f - t));
            Vector3 avance = baseLocalRotation * Vector3.forward * ((zoomInForwardOffset * ZoomInOffsetMultiplier) * avanceT);
            Vector3 bajada = Vector3.down * ((zoomInDownOffset * ZoomInOffsetMultiplier) * bajadaT);
            return avance + bajada;
        }

        if (zoomVisualActual > 0f)
        {
            float t = ZoomOutRange <= 0.001f ? 0f : Mathf.Clamp01(zoomVisualActual / ZoomOutRange);
            float retrocesoT = Mathf.SmoothStep(0f, 1f, t);
            float subidaT = 1f - ((1f - t) * (1f - t));
            Vector3 retroceso = baseLocalRotation * Vector3.back * ((zoomInForwardOffset * ZoomOutRange) * retrocesoT);
            Vector3 subida = Vector3.up * ((zoomInDownOffset * ZoomOutRange) * subidaT);
            return retroceso + subida;
        }

        return Vector3.zero;
    }

    private void RecalcularTopeAdelanteLocal()
    {
        float worldWidth = CalcularAnchoVisibleMundo();
        float offsetForwardWorld = worldWidth * Mathf.Clamp01(maxForwardOffsetPercent);

        float parentScaleZ = 1f;
        if (transform.parent != null)
        {
            parentScaleZ = Mathf.Abs(transform.parent.lossyScale.z);
        }

        float offsetForwardLocal = offsetForwardWorld / Mathf.Max(0.0001f, parentScaleZ);
        forwardLimitLocalZ = startLocalZ + offsetForwardLocal;
        panZActual = Mathf.Clamp(panZActual, startLocalZ, forwardLimitLocalZ);
    }

    private float CalcularAnchoVisibleMundo()
    {
        if (!cam)
        {
            return 0f;
        }

        if (cam.orthographic)
        {
            return 2f * cam.orthographicSize * cam.aspect;
        }

        float distance = fallbackFocusDistance;
        if (focusAtDepth)
        {
            Vector3 camToFocus = focusAtDepth.position - cam.transform.position;
            distance = Mathf.Max(0.01f, Vector3.Dot(camToFocus, cam.transform.forward));
        }

        float heightAtDist = 2f * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
        return heightAtDist * cam.aspect;
    }

    private static bool EstaMouseSobreUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void RecentrarLocal()
    {
        panZActual = startLocalZ;
        AplicarFinalPosicion();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Transform p = transform.parent;
        Vector3 lp = transform.localPosition;

        Vector3 localStart = new Vector3(lp.x, lp.y, startLocalZ);
        Vector3 localTope = new Vector3(lp.x, lp.y, forwardLimitLocalZ);

        Vector3 worldStart = p ? p.TransformPoint(localStart) : transform.TransformPoint(Vector3.zero);
        Vector3 worldTope = p ? p.TransformPoint(localTope) : transform.TransformPoint(Vector3.zero);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(worldStart, 0.05f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(worldTope, 0.05f);
    }
#endif
}
