using UnityEngine;

[DisallowMultipleComponent]
public class CaravanTorchLight : MonoBehaviour
{
    [Header("Aspecto")]
    [SerializeField] private Color colorAntorcha = new Color(1f, 0.48f, 0.18f, 1f);
    [SerializeField, Min(0f)] private float intensidadBase = 1.8f;
    [SerializeField, Range(0f, 0.35f)] private float variacionTitileo = 0.12f;
    [SerializeField, Min(0.1f)] private float velocidadTitileo = 2.6f;
    [HideInInspector, SerializeField] private float alturaSobreCaravana = 1.15f;
    [SerializeField, Range(0.5f, 1.5f)] private float multiplicadorRango = 1f;
    [SerializeField, Min(0.05f)] private float suavizado = 24f;

    private Light luz;
    private float anguloSpotBase;
    private float factorVisual;
    private bool configuracionCapturada;
    private bool luzHabilitadaOriginal;
    private bool estadoSecuenciadoInicializado;
    private bool encendidaPorSecuencia;
    private bool cambioEstadoProgramado;
    private bool estadoProgramado;
    private float momentoCambioEstado;

    public void ProgramarEstado(bool encendida, float retardo)
    {
        if (!estadoSecuenciadoInicializado)
        {
            encendidaPorSecuencia = !encendida;
            estadoSecuenciadoInicializado = true;
        }

        estadoProgramado = encendida;
        momentoCambioEstado = Time.unscaledTime + Mathf.Max(0f, retardo);
        cambioEstadoProgramado = retardo > 0f;
        if (!cambioEstadoProgramado)
        {
            encendidaPorSecuencia = estadoProgramado;
        }
    }

    public void ActualizarEstado(CampaignManager campaignManager, MapaManager mapaManager)
    {
        if (campaignManager == null || mapaManager == null)
        {
            ApagarCompletamente();
            return;
        }

        CapturarConfiguracionSiHaceFalta();
        if (luz == null)
        {
            return;
        }

        if (!estadoSecuenciadoInicializado)
        {
            encendidaPorSecuencia = campaignManager.AntorchasEncendidas;
            estadoSecuenciadoInicializado = true;
        }
        if (cambioEstadoProgramado && Time.unscaledTime >= momentoCambioEstado)
        {
            encendidaPorSecuencia = estadoProgramado;
            cambioEstadoProgramado = false;
        }
        else if (!cambioEstadoProgramado && encendidaPorSecuencia != campaignManager.AntorchasEncendidas)
        {
            encendidaPorSecuencia = campaignManager.AntorchasEncendidas;
        }

        float hora = campaignManager.ObtenerHoraActual();
        float factorHorario = ObtenerFactorHorario(hora);
        bool carroActivo = transform.parent != null && transform.parent.gameObject.activeInHierarchy;
        bool debeEncender = carroActivo
            && encendidaPorSecuencia
            && factorHorario > 0.0001f;

        if (!debeEncender)
        {
            ApagarCompletamente();
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        float t = 1f - Mathf.Exp(-suavizado * Time.unscaledDeltaTime);
        factorVisual = Mathf.Lerp(factorVisual, factorHorario, t);
        float desfase = Mathf.Abs(transform.GetInstanceID() % 997) / 997f;
        float ruido = Mathf.PerlinNoise(Time.unscaledTime * velocidadTitileo, desfase) * 2f - 1f;

        luz.color = colorAntorcha;
        luz.spotAngle = Mathf.Clamp(
            anguloSpotBase * (1f + campaignManager.mejoraCaravanaAntorchas * 0.10f),
            1f,
            179f);
        luz.range = mapaManager.ObtenerPasoMapa()
            * campaignManager.ObtenerAlcanceVisionAntorchasEnPasos()
            * multiplicadorRango;
        luz.intensity = Mathf.Max(0f, intensidadBase * factorVisual * (1f + ruido * variacionTitileo));
        luz.enabled = luzHabilitadaOriginal && luz.intensity > 0.005f;
    }

    private void CapturarConfiguracionSiHaceFalta()
    {
        if (configuracionCapturada)
        {
            return;
        }

        luz = GetComponent<Light>();
        if (luz == null)
        {
            return;
        }

        luzHabilitadaOriginal = luz.enabled;
        anguloSpotBase = luz.spotAngle;
        configuracionCapturada = true;
    }

    private void ApagarCompletamente()
    {
        if (!configuracionCapturada)
        {
            CapturarConfiguracionSiHaceFalta();
        }

        factorVisual = 0f;
        if (luz != null)
        {
            luz.intensity = 0f;
            luz.enabled = false;
        }
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private static float ObtenerFactorHorario(float hora)
    {
        float h = Mathf.Repeat(hora, 24f);
        if (h >= 19f && h < 19.15f)
        {
            return Mathf.Lerp(0.04f, 1f, Mathf.SmoothStep(0f, 1f, (h - 19f) / 0.15f));
        }
        if (h >= 19.15f || h < 6f)
        {
            return 1f;
        }
        if (h < 7f)
        {
            return Mathf.SmoothStep(0f, 1f, 7f - h);
        }
        return 0f;
    }
}
