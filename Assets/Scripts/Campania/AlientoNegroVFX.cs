using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// Controla el avance del Aliento Negro en escena usando un sistema de partículas.
public class AlientoNegroVFX : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Slider sliderAlientoNegro;
    [SerializeField] ParticleSystem alientoNegroParticulas;
    [SerializeField] Transform objetivoMovimiento;
    [SerializeField] GameObject goPuntosPosAliento;


    [Header("Rango de valores")]
    [SerializeField] float valorMinimo = 0f;
    [SerializeField] float valorMaximo = 20f;

    [Header("Rango de posición")]
    [SerializeField] bool usarEspacioLocal = true;
    [SerializeField] Vector3 posicionMinima = new Vector3(-5f, 0f, 0f);
    [SerializeField] Vector3 posicionMaxima = new Vector3(5f, 0f, 0f);
    [SerializeField] Transform puntoMinimoReferencia;
    [SerializeField] Transform puntoMaximoReferencia;

    [Header("Animación")]
    [SerializeField] bool escalarDuracionPorPaso = true;
    [SerializeField] float duracionBase = 1.5f;
    [SerializeField] float duracionPorPaso = 0.35f;
    [SerializeField] float duracionMinima = 0.25f;
    [SerializeField] float duracionMaxima = 4f;
    [SerializeField] AnimationCurve curvaMovimiento = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Partículas")]
    [SerializeField] bool reproducirParticulasDuranteMovimiento = true;
    [SerializeField] float detenerParticulasDelay = 0.5f;
    [SerializeField] bool ajustarEmisionSegunProgreso = false;
    [SerializeField] float emissionRateMin = 10f;
    [SerializeField] float emissionRateMax = 60f;

    Coroutine moverCoroutine;
    Coroutine detenerParticulasCoroutine;
    float progresoActual;
    float valorActualVisual;
    bool emissionDefaultEnabled = true;
    readonly Dictionary<int, Transform> puntosPosicionPorValor = new Dictionary<int, Transform>();

    void Awake()
    {
        InicializarObjetivo();
        ReconstruirPuntosPosicion();
        ActualizarPosicionesDesdeAnclas();
        if (alientoNegroParticulas != null)
        {
            emissionDefaultEnabled = alientoNegroParticulas.emission.enabled;
        }
    }

    void Start()
    {
        SincronizarConEstadoActual();
    }

    void OnValidate()
    {
        InicializarObjetivo();
        ReconstruirPuntosPosicion();
        valorMaximo = Mathf.Max(valorMinimo, valorMaximo);
        duracionMinima = Mathf.Max(0f, duracionMinima);
        duracionMaxima = Mathf.Max(duracionMinima, duracionMaxima);
        duracionBase = Mathf.Clamp(duracionBase, duracionMinima, duracionMaxima);
        duracionPorPaso = Mathf.Max(0f, duracionPorPaso);
        detenerParticulasDelay = Mathf.Max(0f, detenerParticulasDelay);
        ActualizarPosicionesDesdeAnclas();

        if (!Application.isPlaying && objetivoMovimiento)
        {
            AplicarEstadoInstantaneo(CalcularDestinoParaValor(valorActualVisual, progresoActual), progresoActual, valorActualVisual);
        }
    }

    public void AvanzarAlientoNegro(int cant)
    {
        float valorActual = ObtenerValorAlientoNegro();
        float nuevoProgreso = CalcularProgreso(valorActual);
        Vector3 destino = CalcularDestinoParaValor(valorActual, nuevoProgreso);
        float duracion = CalcularDuracionMovimiento(Mathf.Abs(cant));

        if (Mathf.Approximately(valorActual, valorActualVisual))
        {
            AplicarEstadoInstantaneo(destino, nuevoProgreso, valorActual);
            return;
        }

        if (!isActiveAndEnabled)
        {
            AplicarEstadoInstantaneo(destino, nuevoProgreso, valorActual);
            return;
        }

        if (moverCoroutine != null)
        {
            StopCoroutine(moverCoroutine);
        }
        moverCoroutine = StartCoroutine(Mover(valorActual, destino, nuevoProgreso, duracion));
    }

    IEnumerator Mover(float valorDestino, Vector3 destinoFinal, float destinoProgreso, float duracionTotal)
    {
        ActivarParticulas();

        List<Vector3> ruta = ConstruirRutaHastaValor(valorDestino);
        if (ruta.Count == 0)
        {
            ruta.Add(destinoFinal);
        }

        float progresoInicialTotal = progresoActual;
        float valorInicialTotal = valorActualVisual;
        Vector3 origenSegmento = ObtenerPosicionActual();
        float progresoSegmentoInicial = progresoInicialTotal;
        float valorSegmentoInicial = valorInicialTotal;
        int totalSegmentos = ruta.Count;

        for (int i = 0; i < totalSegmentos; i++)
        {
            Vector3 destinoSegmento = ruta[i];
            float progresoSegmentoFinal = Mathf.Lerp(progresoInicialTotal, destinoProgreso, (i + 1f) / totalSegmentos);
            float valorSegmentoFinal = Mathf.Lerp(valorInicialTotal, valorDestino, (i + 1f) / totalSegmentos);
            float duracionSegmento = totalSegmentos > 0 ? duracionTotal / totalSegmentos : 0f;
            float tiempo = 0f;

            while (tiempo < duracionSegmento)
            {
                tiempo += Time.deltaTime;
                float t = duracionSegmento > 0f ? Mathf.Clamp01(tiempo / duracionSegmento) : 1f;
                float curvaT = curvaMovimiento.Evaluate(t);

                EstablecerPosicion(Vector3.LerpUnclamped(origenSegmento, destinoSegmento, curvaT));
                float progresoInterpolado = Mathf.Lerp(progresoSegmentoInicial, progresoSegmentoFinal, curvaT);
                float valorInterpolado = Mathf.Lerp(valorSegmentoInicial, valorSegmentoFinal, curvaT);
                ActualizarUIYParticulas(progresoInterpolado, valorInterpolado);

                yield return null;
            }

            EstablecerPosicion(destinoSegmento);
            ActualizarUIYParticulas(progresoSegmentoFinal, valorSegmentoFinal);
            origenSegmento = destinoSegmento;
            progresoSegmentoInicial = progresoSegmentoFinal;
            valorSegmentoInicial = valorSegmentoFinal;
        }

        AplicarEstadoInstantaneo(destinoFinal, destinoProgreso, valorDestino);
        moverCoroutine = null;
        //ProgramarDetencionParticulas();
    }

    void AplicarEstadoInstantaneo(Vector3 destino, float progreso, float valorVisual)
    {
        EstablecerPosicion(destino);
        ActualizarUIYParticulas(progreso, valorVisual);
        progresoActual = progreso;
        valorActualVisual = valorVisual;
    }

    void ActualizarUIYParticulas(float progreso, float valorVisual)
    {
        progreso = Mathf.Clamp01(progreso);
        progresoActual = progreso;
        valorActualVisual = valorVisual;

        if (sliderAlientoNegro != null)
        {
            sliderAlientoNegro.value = progreso;
        }

        if (alientoNegroParticulas != null && ajustarEmisionSegunProgreso)
        {
            var emission = alientoNegroParticulas.emission;
            emission.rateOverTime = Mathf.Lerp(emissionRateMin, emissionRateMax, progreso);
        }
    }

    void ActivarParticulas()
    {
        if (!reproducirParticulasDuranteMovimiento || alientoNegroParticulas == null)
            return;

        if (detenerParticulasCoroutine != null)
        {
            StopCoroutine(detenerParticulasCoroutine);
            detenerParticulasCoroutine = null;
        }

        SetEmissionEnabled(true);

        if (!alientoNegroParticulas.isPlaying)
            alientoNegroParticulas.Play(true);
    }

    void SetEmissionEnabled(bool enabled)
    {
        if (alientoNegroParticulas == null)
            return;

        var emission = alientoNegroParticulas.emission;
        emission.enabled = enabled;
    }

    void ProgramarDetencionParticulas()
    {
        if (!reproducirParticulasDuranteMovimiento || alientoNegroParticulas == null)
            return;

        if (detenerParticulasCoroutine != null)
        {
            StopCoroutine(detenerParticulasCoroutine);
        }
        detenerParticulasCoroutine = StartCoroutine(DetenerParticulasTrasDelay());
    }

    IEnumerator DetenerParticulasTrasDelay()
    {
        if (detenerParticulasDelay > 0f)
        {
            yield return new WaitForSeconds(detenerParticulasDelay);
        }

        SetEmissionEnabled(false);
        detenerParticulasCoroutine = null;
    }

    float ObtenerValorAlientoNegro()
    {
        return CampaignManager.Instance != null
            ? CampaignManager.Instance.GetValorAlientoNegro()
            : valorMinimo;
    }

    float CalcularProgreso(float valor)
    {
        if (Mathf.Approximately(valorMaximo, valorMinimo))
            return 0f;

        return Mathf.Clamp01((valor - valorMinimo) / (valorMaximo - valorMinimo));
    }

    float CalcularDuracionMovimiento(int pasos)
    {
        float duracion = duracionBase;
        if (escalarDuracionPorPaso)
        {
            duracion += pasos * duracionPorPaso;
        }

        return Mathf.Clamp(duracion, duracionMinima, duracionMaxima);
    }

    Vector3 CalcularDestino(float progreso)
    {
        return Vector3.Lerp(posicionMinima, posicionMaxima, progreso);
    }

    Vector3 CalcularDestinoParaValor(float valor, float progresoFallback)
    {
        if (TryObtenerPosicionPunto(Mathf.RoundToInt(valor), out Vector3 posicionPunto))
        {
            return posicionPunto;
        }

        return CalcularDestino(progresoFallback);
    }

    List<Vector3> ConstruirRutaHastaValor(float valorDestino)
    {
        var ruta = new List<Vector3>();
        int valorObjetivo = Mathf.RoundToInt(valorDestino);
        int valorVisualActualRedondeado = Mathf.RoundToInt(valorActualVisual);
        int direccion = valorObjetivo.CompareTo(valorVisualActualRedondeado);

        if (direccion == 0)
        {
            return ruta;
        }

        int valorBase = direccion > 0
            ? Mathf.FloorToInt(valorActualVisual)
            : Mathf.CeilToInt(valorActualVisual);

        for (int valor = valorBase + direccion; direccion > 0 ? valor <= valorObjetivo : valor >= valorObjetivo; valor += direccion)
        {
            ruta.Add(CalcularDestinoParaValor(valor, CalcularProgreso(valor)));
        }

        return ruta;
    }

    Vector3 ObtenerPosicionActual()
    {
        return usarEspacioLocal ? objetivoMovimiento.localPosition : objetivoMovimiento.position;
    }

    void EstablecerPosicion(Vector3 posicion)
    {
        if (usarEspacioLocal)
            objetivoMovimiento.localPosition = posicion;
        else
            objetivoMovimiento.position = posicion;
    }

    void InicializarObjetivo()
    {
        if (objetivoMovimiento == null)
            objetivoMovimiento = transform;
    }

    void ReconstruirPuntosPosicion()
    {
        puntosPosicionPorValor.Clear();

        if (goPuntosPosAliento == null)
            return;

        Transform raizPuntos = goPuntosPosAliento.transform;
        for (int i = 0; i < raizPuntos.childCount; i++)
        {
            Transform punto = raizPuntos.GetChild(i);
            if (!int.TryParse(punto.name, out int valor))
                continue;

            puntosPosicionPorValor[valor] = punto;
        }
    }

    bool TryObtenerPosicionPunto(int valor, out Vector3 posicion)
    {
        if (puntosPosicionPorValor.Count == 0)
        {
            ReconstruirPuntosPosicion();
        }

        if (puntosPosicionPorValor.TryGetValue(valor, out Transform punto) && punto != null)
        {
            if (usarEspacioLocal)
            {
                Transform parentObjetivo = objetivoMovimiento != null ? objetivoMovimiento.parent : null;
                posicion = parentObjetivo != null
                    ? parentObjetivo.InverseTransformPoint(punto.position)
                    : punto.position;
            }
            else
            {
                posicion = punto.position;
            }
            return true;
        }

        posicion = default;
        return false;
    }

    void ActualizarPosicionesDesdeAnclas()
    {
        if (puntoMinimoReferencia != null)
        {
            posicionMinima = usarEspacioLocal ? puntoMinimoReferencia.localPosition : puntoMinimoReferencia.position;
        }

        if (puntoMaximoReferencia != null)
        {
            posicionMaxima = usarEspacioLocal ? puntoMaximoReferencia.localPosition : puntoMaximoReferencia.position;
        }
    }

    void SincronizarConEstadoActual()
    {
        float valor = ObtenerValorAlientoNegro();
        float progreso = CalcularProgreso(valor);
        AplicarEstadoInstantaneo(CalcularDestinoParaValor(valor, progreso), progreso, valor);
    }

#if UNITY_EDITOR
    public float DebugValorActual
    {
        get
        {
            if (Application.isPlaying && CampaignManager.Instance != null)
                return CampaignManager.Instance.GetValorAlientoNegro();
            return valorActualVisual;
        }
    }

    public float DebugValorMinimo => valorMinimo;
    public float DebugValorMaximo => valorMaximo;

    public void DebugAjustarAliento(int delta)
    {
        if (Application.isPlaying && CampaignManager.Instance != null)
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(delta);
            return;
        }

        float nuevo = Mathf.Clamp(DebugValorActual + delta, valorMinimo, valorMaximo);
        float progreso = CalcularProgreso(nuevo);
        AplicarEstadoInstantaneo(CalcularDestinoParaValor(nuevo, progreso), progreso, nuevo);
    }

    public void DebugSetValor(int valor)
    {
        float clamped = Mathf.Clamp(valor, valorMinimo, valorMaximo);

        if (Application.isPlaying && CampaignManager.Instance != null)
        {
            float actual = CampaignManager.Instance.GetValorAlientoNegro();
            int delta = Mathf.RoundToInt(clamped - actual);
            if (delta != 0)
                CampaignManager.Instance.CambiarValorAlientoNegro(delta);
            else
                AvanzarAlientoNegro(0);
            return;
        }

        float progreso = CalcularProgreso(clamped);
        AplicarEstadoInstantaneo(CalcularDestinoParaValor(clamped, progreso), progreso, clamped);
    }
#endif

    void OnDisable()
    {
        if (moverCoroutine != null)
        {
            StopCoroutine(moverCoroutine);
            moverCoroutine = null;
        }

        if (detenerParticulasCoroutine != null)
        {
            StopCoroutine(detenerParticulasCoroutine);
            detenerParticulasCoroutine = null;
        }

        if (alientoNegroParticulas != null)
        {
            SetEmissionEnabled(emissionDefaultEnabled);
            alientoNegroParticulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AlientoNegroVFX))]
class AlientoNegroVFXEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var vfx = (AlientoNegroVFX)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Aliento Negro", EditorStyles.boldLabel);

        int valorActual = Mathf.RoundToInt(vfx.DebugValorActual);
        EditorGUILayout.LabelField("Valor actual", valorActual.ToString("F0"));

        int minimo = Mathf.RoundToInt(vfx.DebugValorMinimo);
        int maximo = Mathf.RoundToInt(vfx.DebugValorMaximo);

        int nuevoValor = EditorGUILayout.IntSlider("Valor debug", valorActual, minimo, maximo);
        if (nuevoValor != valorActual)
        {
            vfx.DebugSetValor(nuevoValor);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("- Paso"))
            vfx.DebugAjustarAliento(-1);
        if (GUILayout.Button("+ Paso"))
            vfx.DebugAjustarAliento(1);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Min"))
            vfx.DebugSetValor(minimo);
        if (GUILayout.Button("Max"))
            vfx.DebugSetValor(maximo);
        EditorGUILayout.EndHorizontal();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Fuera de Play, los controles sólo mueven el VFX para vista previa.", MessageType.Info);
        }
    }
}
#endif



