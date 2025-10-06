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

    TaskCompletionSource<bool> impactoTcs;
    bool configurado;

    void Awake()
    {
        impactoTcs = new TaskCompletionSource<bool>();
    }
    
    public void Configure(Transform inicio, Transform destino, float alturaParabola, float velocidadVuelo)
    {
        startMarker = ResolvePuntoSalida(inicio);
        endMarker = ResolvePuntoEntrada(destino);
        parabola = alturaParabola;
        velocidad = velocidadVuelo;

        startTime = Time.time;
        configurado = true;

        journeyLength = Vector3.Distance(GetStartPosition(), GetEndPosition());
        if (journeyLength <= 0.0001f)
        {
            journeyLength = 0.0001f;
        }

        transform.position = GetStartPosition();
        if (endMarker != null)
        {
            transform.LookAt(GetEndPosition());
        }
    }

    void Start()
    {
        startMarker = ResolvePuntoSalida(startMarker);
        endMarker = ResolvePuntoEntrada(endMarker);

        if (!configurado && startMarker != null && endMarker != null)
        {
            Configure(startMarker, endMarker, parabola, velocidad);
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
    {   Debug.DrawLine(GetStartPosition(), GetStartPosition() + Vector3.up * 0.5f, Color.yellow);
        startMarker = ResolvePuntoSalida(startMarker);
        endMarker = ResolvePuntoEntrada(endMarker);

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

        Vector3 origen = GetStartPosition();
        Vector3 destino = GetEndPosition();

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

    Vector3 GetStartPosition()
    {
        if (startMarker == null)
        {
            return transform.position;
        }

        return startMarker.position;
    }

    Vector3 GetEndPosition()
    {
        if (endMarker == null)
        {
            return transform.position;
        }

        return endMarker.position;
    }

    Transform ResolvePuntoSalida(Transform posibleInicio)
    {
        if (posibleInicio == null)
        {
            return null;
        }

        Unidad unidad = posibleInicio.GetComponentInParent<Unidad>();
        if (unidad != null && unidad.puntoSaliente != null)
        {
            return unidad.puntoSaliente;
        }

        return posibleInicio;
    }

    Transform ResolvePuntoEntrada(Transform posibleDestino)
    {
        if (posibleDestino == null)
        {
            return null;
        }

        Unidad unidad = posibleDestino.GetComponentInParent<Unidad>();
        if (unidad != null && unidad.puntoEntrante != null)
        {
            return unidad.puntoEntrante;
        }

        return posibleDestino;
    }

    Vector3 CalculateParabolicPath(Vector3 start, Vector3 end, float height, float t)
    {
        // interpolación lineal entre inicio y fin
        Vector3 pos = Vector3.Lerp(start, end, t);

        // parábola: 4 * h * t * (1 - t) asegura que:
        //   t=0  → 0
        //   t=0.5 → altura máxima
        //   t=1  → 0
        float parabola = 4 * height * t * (1 - t);

        pos.y += parabola;
        return pos;
    }

}