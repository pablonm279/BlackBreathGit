using System.Threading.Tasks;
using UnityEngine;

public class ArrowFlight : MonoBehaviour
{
    public Transform startMarker; // Punto de inicio (personaje)
    public Transform endMarker;   // Punto final (enemigo)
 

    private float startTime;
    private float journeyLength;

    public float parabola;
    public float velocidad;
    public float offsetStartY; //para que salga mas arriba o abajo
    public float offsetEndY; //para que pegue mas arriba o abajo

    TaskCompletionSource<bool> impactoTcs;
    bool configurado;

    void Awake()
    {
        impactoTcs = new TaskCompletionSource<bool>();
    }

    public void Configure(Transform inicio, Transform destino, float alturaParabola, float velocidadVuelo, float offsetInicio = 0f, float offsetDestino = 0f)
    {
        startMarker = inicio;
        endMarker = destino;
        parabola = alturaParabola;
        velocidad = velocidadVuelo;
        offsetStartY = offsetInicio;
        offsetEndY = offsetDestino;

        startTime = Time.time;
        configurado = true;
        journeyLength = Vector3.Distance(startMarker.position + new Vector3(0, offsetStartY, 0), endMarker.position + new Vector3(0, offsetEndY, 0));
        transform.position = startMarker.position + new Vector3(0, offsetStartY, 0);
        if (endMarker != null)
        {
            transform.LookAt(endMarker.position);
        }
    }

    void Start()
    {
        if (!configurado && startMarker != null && endMarker != null)
        {
            Configure(startMarker, endMarker, parabola, velocidad, offsetStartY, offsetEndY);
        }
    }

    void Destruir()
    {
        impactoTcs.TrySetResult(true);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        impactoTcs?.TrySetResult(true);
    }

    public Task EsperarImpactoAsync()
    {
        return impactoTcs.Task;
    }

    void Update()
    {
        if (startMarker == null || endMarker == null)
        {
            Destruir();
            return;
        }

        float distCovered = (Time.time - startTime) * velocidad; // Velocidad
        float distanciaTotal = journeyLength;
        if (distanciaTotal <= 0.0001f)
        {
            distanciaTotal = 0.0001f;
        }

        float fracJourney = Mathf.Clamp01(distCovered / distanciaTotal);

        Vector3 origen = startMarker.position + new Vector3(0, offsetStartY, 0);
        Vector3 destino = endMarker.position + new Vector3(0, offsetEndY, 0);

        Vector3 nextPosition = CalculateParabolicPath(origen, destino, parabola, fracJourney);
        transform.position = nextPosition;

        if (fracJourney < 1f)
        {
            float previewT = Mathf.Clamp01(fracJourney + (Time.deltaTime * velocidad / distanciaTotal));
            Vector3 preview = CalculateParabolicPath(origen, destino, parabola, previewT);
            transform.LookAt(preview);
        }

        float remainingDistance = Vector3.Distance(nextPosition, destino);

        if (remainingDistance <= 0.12f || fracJourney >= 0.999f)
        {
            Destruir();
        }
    }

    Vector3 CalculateParabolicPath(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = Mathf.Sin(t * Mathf.PI);

        return Vector3.Lerp(start, end, t) + Vector3.up * parabolicT * height;
    }
}
