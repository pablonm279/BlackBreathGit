using UnityEngine;

public class materialbrilloDescargaPoder : MonoBehaviour
{
    [Header("Renderer (opcional)")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Pulso de brillo (Emision)")]
    [SerializeField] private Color pulseColor = new Color(0.4f, 0.7f, 1f, 1f);
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 2.5f;
    [SerializeField] private float pulseSpeed = 2.0f;

    [Header("Respiración de escala")]
    [SerializeField] private float scaleAmount = 0.08f; // 8% de cambio
    [SerializeField] private float scaleSpeed = 1.2f;

    [Header("Opciones")]
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private bool randomizePhase = true;

    private Material _matInstance;
    private Vector3 _baseScale;
    private float _phase;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void Start()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer != null)
        {
            // Crea una instancia del material para no afectar materiales compartidos.
            _matInstance = targetRenderer.material;

            // Asegura que la Emision esté activa en el shader Standard.
            _matInstance.EnableKeyword("_EMISSION");

            // Si no tiene emisión definida, usa el color de pulso por defecto.
            if (_matInstance.HasProperty(EmissionColorID))
            {
                var current = _matInstance.GetColor(EmissionColorID);
                if (current.maxColorComponent <= 0.001f)
                {
                    _matInstance.SetColor(EmissionColorID, pulseColor.linear * minIntensity);
                }
            }
        }

        _phase = randomizePhase ? Random.value * Mathf.PI * 2f : 0f;
    }

    private void Update()
    {
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;

        // Pulso 0..1 con seno
        float t = time * pulseSpeed + _phase;
        float pulse01 = (Mathf.Sin(t) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse01);

        if (_matInstance != null && _matInstance.HasProperty(EmissionColorID))
        {
            // Standard espera color en espacio lineal para _EmissionColor
            _matInstance.SetColor(EmissionColorID, pulseColor.linear * intensity);
        }

        // "Respiración" de escala, sutil y uniforme
        float ts = time * scaleSpeed + _phase;
        float scaleFactor = 1f + Mathf.Sin(ts) * scaleAmount;
        transform.localScale = _baseScale * scaleFactor;
    }

    private void OnDisable()
    {
        // Restablece la escala al salir para evitar quedar deformado en editor.
        transform.localScale = _baseScale;
    }

    private void OnDestroy()
    {
        transform.localScale = _baseScale;
    }
}
