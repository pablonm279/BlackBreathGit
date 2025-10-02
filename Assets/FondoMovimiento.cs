using UnityEngine;
using UnityEngine.UI;

public class BackgroundBreathing : MonoBehaviour
{
    public enum FondoMovimiento { AutoDetect, RectTransform, RawImageUV }

    [Header("Modo de paneo")]
    public FondoMovimiento panMode = FondoMovimiento.AutoDetect;

    [Header("Vaivén (paneo)")]
    [Tooltip("Amplitud horizontal en píxeles si es RectTransform; en UV si es RawImage (0–1).")]
    public float amplitudeX = 20f;
    [Tooltip("Amplitud vertical en píxeles si es RectTransform; en UV si es RawImage (0–1).")]
    public float amplitudeY = 0f;
    [Tooltip("Velocidad del vaivén.")]
    public float panSpeed = 0.2f;
    [Tooltip("Desfase opcional para no iniciar en el centro.")]
    public float panPhase = 0f;

    [Header("Zoom respirante (opcional)")]
    public bool enableBreathing = true;
    [Tooltip("Intensidad del zoom (0.01 = 1%).")]
    public float zoomAmplitude = 0.02f;
    [Tooltip("Velocidad del zoom respirante.")]
    public float zoomSpeed = 0.15f;
    [Tooltip("Desfase del zoom.")]
    public float zoomPhase = 0.5f;

    [Header("Suavizado")]
    [Tooltip("Usar tiempo independiente de Time.timeScale (menú suele estar pausado).")]
    public bool useUnscaledTime = true;

    // Cache
    RectTransform rect;
    RawImage raw;
    Vector2 startAnchoredPos;
    Vector3 startScale;
    Rect startUV;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        raw  = GetComponent<RawImage>();

        if (rect != null) startAnchoredPos = rect.anchoredPosition;
        if (rect != null) startScale       = rect.localScale;
        if (raw  != null) startUV          = raw.uvRect;

        if (panMode == FondoMovimiento.AutoDetect)
            panMode = (raw != null) ? FondoMovimiento.RawImageUV : FondoMovimiento.RectTransform;
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        // --- Paneo / Vaivén ---
        float ox = Mathf.Sin((t + panPhase) * panSpeed);
        float oy = Mathf.Sin((t + panPhase) * panSpeed * 0.85f); // leve desfase para Y

        if (panMode == FondoMovimiento.RectTransform && rect != null)
        {
            rect.anchoredPosition = startAnchoredPos + new Vector2(ox * amplitudeX, oy * amplitudeY);
        }
        else if (panMode == FondoMovimiento.RawImageUV && raw != null)
        {
            // Paneo UV sutil (no corta la imagen)
            Rect uv = startUV;
            uv.x += ox * amplitudeX;
            uv.y += oy * amplitudeY;
            raw.uvRect = uv;
        }

        // --- Zoom “respirante” ---
        if (enableBreathing && rect != null)
        {
            float z = Mathf.Sin((t + zoomPhase) * zoomSpeed) * zoomAmplitude;
            rect.localScale = startScale * (1f + z);
        }
    }

    // Utilidad para resetear desde el Inspector si hiciste pruebas
    [ContextMenu("Reset Offsets")]
    void ResetOffsets()
    {
        if (rect != null)
        {
            rect.anchoredPosition = startAnchoredPos;
            rect.localScale = startScale;
        }
        if (raw != null)
            raw.uvRect = startUV;
    }
}
