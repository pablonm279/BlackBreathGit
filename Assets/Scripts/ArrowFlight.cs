using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ArrowFlight : MonoBehaviour
{
    public Transform startMarker; // Punto de inicio (personaje)
    public Transform endMarker;   // Punto final (enemigo)

    private float startTime;
    private float journeyLength;

    public float parabola;
    private float parabolaBase;
    public float velocidad;
    [SerializeField] private float alturaMinimaParabola = 0.05f;
    [SerializeField] private float duracionContinuacionFallo = 0.07f;
    [SerializeField] private float esperaResultadoMaxima = 0.12f;

    TaskCompletionSource<bool> impactoTcs;
    bool configurado;
    bool impactoCompletado;
    bool llegoAlDestino;
    bool resultadoRecibido;
    bool resultadoAplicado;
    bool falloRecibido;
    bool continuandoTrasFallo;
    bool registrado;
    float tiempoLlegada;
    float tiempoInicioContinuacion;
    Vector3 origenContinuacion;
    Vector3 destinoContinuacion;
    float distanciaContinuacion;
    Unidad unidadOrigen;
    Unidad unidadDestino;
    Obstaculo obstaculoDestino;

    static readonly List<ArrowFlight> vuelosActivos = new List<ArrowFlight>();

    void Awake()
    {
        impactoTcs = new TaskCompletionSource<bool>();
        RenderOrderHelper.ForzarProyectilAlFrente(gameObject);
    }
    
    public void Configure(Transform inicio, Transform destino, float alturaParabola, float velocidadVuelo)
    {
        startMarker = ResolvePuntoSalida(inicio);
        endMarker = ResolvePuntoEntrada(destino);
        SincronizarParticipantes();
        parabolaBase = alturaParabola;
        parabola = AjustarParabolaPorDistancia(parabolaBase, startMarker, endMarker);
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

        RegistrarVueloActivo();
    }

    void Start()
    {
        startMarker = ResolvePuntoSalida(startMarker);
        endMarker = ResolvePuntoEntrada(endMarker);

        //Desescalar
        transform.localScale = new Vector3(transform.localScale.x * 0.9f, transform.localScale.y * 0.9f, transform.localScale.z * 0.9f);

        if (!configurado && startMarker != null && endMarker != null)
        {
            parabolaBase = parabola;
            Configure(startMarker, endMarker, parabolaBase, velocidad);
        }
    }

    void Destruir()
    {
        CompletarImpacto();
        Destroy(gameObject);
    }

    void CompletarImpacto()
    {
        if (impactoCompletado)
        {
            return;
        }

        impactoCompletado = true;
        impactoTcs.TrySetResult(true);
    }

    void OnDestroy()
    {
        DesregistrarVueloActivo();
        impactoTcs?.TrySetResult(true);
    }

    public Task EsperarImpactoAsync()
    {
        return impactoTcs.Task;
    }

    void Update()
    {
        if (continuandoTrasFallo)
        {
            ActualizarContinuacionFallo();
            return;
        }

        if (llegoAlDestino)
        {
            if (!resultadoRecibido && Time.time - tiempoLlegada >= esperaResultadoMaxima)
            {
                Destruir();
            }
            return;
        }

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

        if (remainingDistance <= 0.03f || fracJourney >= 0.999f)
        {
            transform.position = destino;
            origenContinuacion = origen;
            destinoContinuacion = destino;
            distanciaContinuacion = distanciaTotal;
            tiempoLlegada = Time.time;
            llegoAlDestino = true;
            CompletarImpacto();

            if (resultadoRecibido)
            {
                ResolverResultadoRecibido();
            }
        }
    }

    void ActualizarContinuacionFallo()
    {
        float tiempoTranscurrido = Time.time - tiempoInicioContinuacion;
        float distanciaTotal = Mathf.Max(distanciaContinuacion, 0.0001f);
        float t = 1f + (tiempoTranscurrido * velocidad / distanciaTotal);

        transform.position = CalculateParabolicPath(origenContinuacion, destinoContinuacion, parabola, t);

        float previewT = t + (Time.deltaTime * velocidad / distanciaTotal);
        Vector3 preview = CalculateParabolicPath(origenContinuacion, destinoContinuacion, parabola, previewT);
        transform.LookAt(preview);

        if (tiempoTranscurrido >= duracionContinuacionFallo)
        {
            Destruir();
        }
    }

    void NotificarResultado(bool fallo)
    {
        if (resultadoRecibido)
        {
            return;
        }

        resultadoRecibido = true;
        falloRecibido = fallo;
        if (llegoAlDestino)
        {
            ResolverResultadoRecibido();
        }
    }

    void ResolverResultadoRecibido()
    {
        if (resultadoAplicado)
        {
            return;
        }

        resultadoAplicado = true;
        if (!falloRecibido)
        {
            Destruir();
            return;
        }

        continuandoTrasFallo = true;
        tiempoInicioContinuacion = Time.time;
    }

    public static void NotificarResultadoAtaque(Unidad origen, Unidad destino, int resultado)
    {
        if (origen == null || destino == null)
        {
            return;
        }

        ArrowFlight vueloPendiente = null;
        for (int i = vuelosActivos.Count - 1; i >= 0; i--)
        {
            ArrowFlight vuelo = vuelosActivos[i];
            if (vuelo == null)
            {
                vuelosActivos.RemoveAt(i);
                continue;
            }

            vuelo.SincronizarParticipantes();
            if (vuelo.unidadOrigen == origen && vuelo.unidadDestino == destino && !vuelo.resultadoRecibido)
            {
                if (vueloPendiente == null || vuelo.startTime < vueloPendiente.startTime)
                {
                    vueloPendiente = vuelo;
                }
            }
        }

        vueloPendiente?.NotificarResultado(resultado <= 0);
    }

    public static Task ObtenerImpactoPendienteAsync(Unidad origen, Unidad destino)
    {
        ArrowFlight vuelo = BuscarVueloActivo(origen, destino, null);
        return vuelo != null ? vuelo.EsperarImpactoAsync() : null;
    }

    public static Task ObtenerImpactoPendienteAsync(Unidad origen, Obstaculo destino)
    {
        ArrowFlight vuelo = BuscarVueloActivo(origen, null, destino);
        return vuelo != null ? vuelo.EsperarImpactoAsync() : null;
    }

    static ArrowFlight BuscarVueloActivo(Unidad origen, Unidad unidadObjetivo, Obstaculo obstaculoObjetivo)
    {
        if (origen == null || (unidadObjetivo == null && obstaculoObjetivo == null))
        {
            return null;
        }

        ArrowFlight mejor = null;
        for (int i = vuelosActivos.Count - 1; i >= 0; i--)
        {
            ArrowFlight vuelo = vuelosActivos[i];
            if (vuelo == null)
            {
                vuelosActivos.RemoveAt(i);
                continue;
            }

            vuelo.SincronizarParticipantes();
            if (vuelo.impactoCompletado || vuelo.unidadOrigen != origen)
            {
                continue;
            }

            bool coincideDestino = unidadObjetivo != null
                ? vuelo.unidadDestino == unidadObjetivo
                : vuelo.obstaculoDestino == obstaculoObjetivo;
            if (!coincideDestino)
            {
                continue;
            }

            if (mejor == null || vuelo.startTime < mejor.startTime)
            {
                mejor = vuelo;
            }
        }

        return mejor;
    }

    void RegistrarVueloActivo()
    {
        if (registrado)
        {
            return;
        }

        registrado = true;
        if (!vuelosActivos.Contains(this))
        {
            vuelosActivos.Add(this);
        }
    }

    void DesregistrarVueloActivo()
    {
        if (!registrado)
        {
            return;
        }

        registrado = false;
        vuelosActivos.Remove(this);
    }

    void SincronizarParticipantes()
    {
        unidadOrigen = startMarker != null ? startMarker.GetComponentInParent<Unidad>() : null;
        unidadDestino = endMarker != null ? endMarker.GetComponentInParent<Unidad>() : null;
        obstaculoDestino = unidadDestino == null && endMarker != null ? endMarker.GetComponentInParent<Obstaculo>() : null;
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

        Obstaculo obstaculo = posibleDestino.GetComponentInParent<Obstaculo>();
        if (obstaculo != null && obstaculo.puntoEntrante != null)
        {
            return obstaculo.puntoEntrante;
        }

        return posibleDestino;
    }

    float AjustarParabolaPorDistancia(float baseParabola, Transform inicio, Transform destino)
    {
        if (inicio == null || destino == null)
        {
            return Mathf.Max(baseParabola, alturaMinimaParabola);
        }

        float bParabola = baseParabola;
        
        Unidad unidadInicio = inicio.GetComponentInParent<Unidad>();
        float posXinicio = unidadInicio.CasillaPosicion.posX * 0.07f;


        float posXdestino = 0;
        if (destino.GetComponentInParent<Unidad>() != null)
        {
            Unidad unidadDestino = destino.GetComponentInParent<Unidad>();
            posXdestino = -(unidadDestino.CasillaPosicion.posX * 0.04f);
        }

        float restaFinal = Mathf.Abs(posXdestino - posXinicio);
        bParabola -= restaFinal;
        
        // Evita alturas negativas o planas que invierten la curva
        bParabola = Mathf.Max(bParabola, alturaMinimaParabola);

        return bParabola;
    }

   
   
    Vector3 CalculateParabolicPath(Vector3 start, Vector3 end, float height, float t)
    {
        float h = Mathf.Max(height, alturaMinimaParabola);
        // interpolacion lineal entre inicio y fin
        Vector3 pos = Vector3.LerpUnclamped(start, end, t);

        // parabola: 4 * h * t * (1 - t) asegura que:
        //   t=0  -> 0
        //   t=0.5 -> altura maxima
        //   t=1  -> 0
        float parabola = 4 * h * t * (1 - t);

        pos.y += parabola;
        return pos;
    }

}
