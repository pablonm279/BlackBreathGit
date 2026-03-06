using System.Collections;
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
    bool emissionDefaultEnabled = true;

    void Awake()
    {
        InicializarObjetivo();
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
        valorMaximo = Mathf.Max(valorMinimo, valorMaximo);
        duracionMinima = Mathf.Max(0f, duracionMinima);
        duracionMaxima = Mathf.Max(duracionMinima, duracionMaxima);
        duracionBase = Mathf.Clamp(duracionBase, duracionMinima, duracionMaxima);
        duracionPorPaso = Mathf.Max(0f, duracionPorPaso);
        detenerParticulasDelay = Mathf.Max(0f, detenerParticulasDelay);
        ActualizarPosicionesDesdeAnclas();

        if (!Application.isPlaying && objetivoMovimiento)
        {
            AplicarEstadoInstantaneo(CalcularDestino(progresoActual), progresoActual);
        }
    }

    public void AvanzarAlientoNegro(int cant)
    {
        float valorActual = ObtenerValorAlientoNegro();
        float nuevoProgreso = CalcularProgreso(valorActual);
        Vector3 destino = CalcularDestino(nuevoProgreso);
        float duracion = CalcularDuracionMovimiento(Mathf.Abs(cant));

        if (Mathf.Approximately(nuevoProgreso, progresoActual))
        {
            AplicarEstadoInstantaneo(destino, nuevoProgreso);
            return;
        }

        if (!isActiveAndEnabled)
        {
            AplicarEstadoInstantaneo(destino, nuevoProgreso);
            return;
        }

        if (moverCoroutine != null)
        {
            StopCoroutine(moverCoroutine);
        }
        moverCoroutine = StartCoroutine(Mover(destino, nuevoProgreso, duracion));
    }

    IEnumerator Mover(Vector3 destino, float destinoProgreso, float duracion)
    {
        Vector3 origen = ObtenerPosicionActual();
        float progresoInicial = progresoActual;
        float tiempo = 0f;

        ActivarParticulas();

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = duracion > 0f ? Mathf.Clamp01(tiempo / duracion) : 1f;
            float curvaT = curvaMovimiento.Evaluate(t);

            EstablecerPosicion(Vector3.LerpUnclamped(origen, destino, curvaT));
            float progresoInterpolado = Mathf.Lerp(progresoInicial, destinoProgreso, curvaT);
            ActualizarUIYParticulas(progresoInterpolado);

            yield return null;
        }

        AplicarEstadoInstantaneo(destino, destinoProgreso);
        moverCoroutine = null;
        //ProgramarDetencionParticulas();
    }

    void AplicarEstadoInstantaneo(Vector3 destino, float progreso)
    {
        EstablecerPosicion(destino);
        ActualizarUIYParticulas(progreso);
        progresoActual = progreso;
    }

    void ActualizarUIYParticulas(float progreso)
    {
        progreso = Mathf.Clamp01(progreso);
        progresoActual = progreso;

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
        AplicarEstadoInstantaneo(CalcularDestino(progreso), progreso);
    }

#if UNITY_EDITOR
    public float DebugValorActual
    {
        get
        {
            if (Application.isPlaying && CampaignManager.Instance != null)
                return CampaignManager.Instance.GetValorAlientoNegro();
            return Mathf.Lerp(valorMinimo, valorMaximo, progresoActual);
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
        AplicarEstadoInstantaneo(CalcularDestino(progreso), progreso);
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
        AplicarEstadoInstantaneo(CalcularDestino(progreso), progreso);
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



