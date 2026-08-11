using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light))]
public class intensidadluzcaminos : MonoBehaviour
{
    [Header("Intensidad por horario")]
    [SerializeField, Min(0f)] private float intensidadDia = 1.1f;
    [SerializeField, Min(0f)] private float intensidadNoche = 0.25f;
    [SerializeField, Range(0f, 6f)] private float anticipacionNocheHoras = 1f;
    [SerializeField, Min(0f)] private float duracionTransicion = 0.6f;

    private Light luz;
    private float velocidadTransicion;

    private void Awake()
    {
        luz = GetComponent<Light>();
    }

    private void OnEnable()
    {
        if (luz == null)
        {
            luz = GetComponent<Light>();
        }

        AplicarIntensidad(true);
    }

    private void Update()
    {
        AplicarIntensidad(false);
    }

    private void AplicarIntensidad(bool inmediata)
    {
        if (luz == null)
        {
            return;
        }

        CampaignManager campaignManager = CampaignManager.Instance;
        bool aplicarNoche = campaignManager != null
            && (campaignManager.EsNocheActual()
                || CampaignManager.EsHoraNocturna(
                    campaignManager.ObtenerHoraActual() + anticipacionNocheHoras));
        float intensidadObjetivo = aplicarNoche
            ? intensidadNoche
            : intensidadDia;

        if (inmediata || duracionTransicion <= 0f)
        {
            luz.intensity = intensidadObjetivo;
            velocidadTransicion = 0f;
            return;
        }

        luz.intensity = Mathf.SmoothDamp(
            luz.intensity,
            intensidadObjetivo,
            ref velocidadTransicion,
            duracionTransicion,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
    }
}
