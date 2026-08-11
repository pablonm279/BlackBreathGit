using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class SunController : MonoBehaviour
{
    [Header("Trayectoria")]
    public float elevStart = 12f;      // altura al inicio del viaje
    public float elevEnd   = 55f;      // altura al final del viaje
    public float duracion  = 1.2f;     // seg por viaje

    [Header("Orientación inicial (Y)")]
    public float azimuthDerecha = 90f; // Y que corresponde a "derecha" en tu mapa

    [Header("Paso por viaje (Y)")]
    [FormerlySerializedAs("pasoX")] public float pasoY = -4f; // grados que rota en Y por cada avance

    [Header("Comportamiento")]
    public bool Fijo = false; // si esta activo, no rota al bajar la intensidad

    [Header("Atardecer")]
    [Range(0.5f, 1f)] public float intensityStepFactor = 0.95f; // 5% menos por step
    [Range(0f, 1f)] public float colorStep = 0.05f;             // cuánto se acerca al color atardecer por step
    public Color sunsetColor = new Color(1f, 0.45f, 0.1f);       // naranja/rojizo
    [Min(0f)] public float minIntensity = 0f;                    // intensidad mínima (0 = sin límite)

    [Header("ILUMINACIÓN POR HORA (editar aquí)")]
    [Tooltip("Comienza la transición desde la iluminación nocturna.")]
    [Range(0f, 24f)] public float horaAmanecer = 6f;
    [Tooltip("Termina el amanecer y comienza la iluminación diurna.")]
    [Range(0f, 24f)] public float horaManana = 9f;
    [Tooltip("Hora en la que alcanza la intensidad máxima del día.")]
    [Range(0f, 24f)] public float horaMediodia = 13f;
    [Tooltip("Comienza la transición de atardecer.")]
    [Range(0f, 24f)] public float horaAtardecer = 17f;
    [Tooltip("Comienza la iluminación nocturna.")]
    [Range(0f, 24f)] public float horaNoche = 21f;

    [Header("Colores por franja")]
    public Color dawnColor = new Color(1f, 0.72f, 0.48f);
    [Tooltip("Color de la Directional Light durante la noche.")]
    public Color nightColor = new Color(0.28f, 0.38f, 0.62f);
    [Tooltip("Multiplicador de la intensidad original al mediodía.")]
    [Range(0f, 2f)] public float middayIntensityFactor = 1.08f;
    [Tooltip("Multiplicador de la intensidad original durante el atardecer.")]
    [Range(0f, 1f)] public float sunsetIntensityFactor = 0.62f;
    [HideInInspector] public float nightIntensityFactor = 0.015f;
    [Tooltip("Intensidad nocturna respecto de la intensidad original del sol.")]
    [Range(0f, 0.2f)] public float intensidadLuzNocturna = 0.06f;

    [Header("Ambiente")]
    public bool controlarLuzAmbiente = true;
    public Color nightAmbientColor = new Color(0.055f, 0.075f, 0.13f);

    [Header("Reset")]
    public float resetDuration = 6f;  // segundos para volver a la posición inicial
    public bool resetIntensity = true; // también restaurar intensidad del sol

    float azActual;
    Quaternion _initialLocalRotation;
    float _initialIntensity;
    Color _initialColor;
    float _initialPasoY;
    Color _initialAmbientColor;
    bool _initialAmbientCaptured;

    void Awake()
    {
        azActual = transform.eulerAngles.y;
        _initialLocalRotation = transform.localRotation;
        var ownLight = sun ? sun : GetComponent<Light>();
        _initialIntensity = ownLight ? ownLight.intensity : 0f;
        _initialColor = ownLight ? ownLight.color : Color.white;
        _initialPasoY = pasoY;
    }
     [Header("Referencias")]
    public Light sun;             // arrastrá aquí tu Directional Light
    public Renderer mapaRenderer; // arrastrá el MeshRenderer del plane
    void Start()
    {
        var l = sun ? sun : GetComponent<Light>();
        if (l && mapaRenderer)
            l.cookieSize = Mathf.Max(mapaRenderer.bounds.size.x, mapaRenderer.bounds.size.z) * 1.2f;
    }

    public void SetCampaignHour(float hour)
    {
        CapturarAmbienteInicialSiHaceFalta();
        float hora = Mathf.Repeat(hour, 24f);
        Light luz = sun ? sun : GetComponent<Light>();

        if (!Fijo)
        {
            // Conserva la trayectoria visual anterior al reloj horario: un cambio
            // leve únicamente sobre Y. Inclinar la Directional Light sobre X hacía
            // que barriera la escena y quemara materiales a determinadas horas.
            float progresoDia = hora < 8f
                ? 0f
                : Mathf.InverseLerp(8f, 21f, hora);
            transform.localRotation = _initialLocalRotation * Quaternion.Euler(0f, pasoY * progresoDia, 0f);
        }

        EvaluarHoraAbsoluta(hora, out float intensidad, out Color color, out Color ambiente);
        if (luz != null)
        {
            luz.intensity = intensidad;
            luz.color = color;
        }
        if (controlarLuzAmbiente)
        {
            RenderSettings.ambientLight = ambiente;
        }
    }

    void EvaluarHoraAbsoluta(float hora, out float intensidad, out Color color, out Color ambiente)
    {
        float amanecer = Mathf.Clamp(horaAmanecer, 0f, 23.96f);
        float manana = Mathf.Clamp(horaManana, amanecer + 0.01f, 23.97f);
        float mediodia = Mathf.Clamp(horaMediodia, manana + 0.01f, 23.98f);
        float atardecer = Mathf.Clamp(horaAtardecer, mediodia + 0.01f, 23.99f);
        float noche = Mathf.Clamp(horaNoche, atardecer + 0.01f, 24f);
        float intensidadNoche = Mathf.Max(minIntensity, _initialIntensity * Mathf.Clamp(intensidadLuzNocturna, 0f, 0.2f));
        float intensidadAmanecer = Mathf.Max(minIntensity, _initialIntensity * 0.75f);
        float intensidadMediodia = Mathf.Max(minIntensity, _initialIntensity * middayIntensityFactor);
        float intensidadAtardecer = Mathf.Max(minIntensity, _initialIntensity * sunsetIntensityFactor);

        if (hora >= amanecer && hora < manana)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(amanecer, manana, hora));
            intensidad = Mathf.Lerp(intensidadNoche, intensidadAmanecer, t);
            color = Color.Lerp(nightColor, dawnColor, t);
            ambiente = Color.Lerp(nightAmbientColor, _initialAmbientColor, t);
        }
        else if (hora >= manana && hora < atardecer)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(manana, mediodia, hora));
            intensidad = Mathf.Lerp(intensidadAmanecer, intensidadMediodia, t);
            color = Color.Lerp(dawnColor, _initialColor, t);
            ambiente = _initialAmbientColor;
        }
        else if (hora >= atardecer && hora < noche)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(atardecer, noche, hora));
            if (t < 0.5f)
            {
                float tramo = t / 0.5f;
                intensidad = Mathf.Lerp(intensidadMediodia, intensidadAtardecer, tramo);
                color = Color.Lerp(_initialColor, sunsetColor, tramo);
            }
            else
            {
                float tramo = (t - 0.5f) / 0.5f;
                intensidad = Mathf.Lerp(intensidadAtardecer, intensidadNoche, tramo);
                color = Color.Lerp(sunsetColor, nightColor, tramo);
            }
            ambiente = Color.Lerp(_initialAmbientColor, nightAmbientColor, t);
        }
        else
        {
            intensidad = intensidadNoche;
            color = nightColor;
            ambiente = nightAmbientColor;
        }
    }

    // Llamá esto UNA VEZ cuando empieza cada viaje
    public void OnTravelStart()
    {
        StopAllCoroutines();
        SetCampaignHour(CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerHoraActual() : 8f);
    }

    // Para descansar, usar la posada o realizar una actividad que consume un día.
    // Recorre el día completo, muestra la noche y devuelve el control al amanecer.
    public void OnDayActionStart(float duracionTotal)
    {
        StopAllCoroutines();
        SetCampaignHour(CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerHoraActual() : 8f);
    }

    IEnumerator ReproducirAccionDiaria(float duracionTotal)
    {
        yield return StartCoroutine(ReproducirCicloDiario(duracionTotal * 0.78f));
        yield return StartCoroutine(ResetRoutine(duracionTotal * 0.22f));
    }

    IEnumerator ReproducirCicloDiario(float duracionOverride = -1f)
    {
        Quaternion startRot = transform.localRotation;
        bool bloquearRotacion = Fijo;
        Quaternion dawnRot = _initialLocalRotation;
        Quaternion targetRot = bloquearRotacion ? dawnRot : dawnRot * Quaternion.Euler(0f, pasoY, 0f);

        var ownLight = GetComponent<Light>();
        float startInt = ownLight ? ownLight.intensity : 0f;
        Color startCol = ownLight ? ownLight.color : Color.white;
        Color startAmbient = RenderSettings.ambientLight;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duracionOverride > 0f ? duracionOverride : duracion);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float progreso = Mathf.Clamp01(t);

            if (!bloquearRotacion)
            {
                transform.localRotation = progreso < 0.15f
                    ? Quaternion.Slerp(startRot, dawnRot, Mathf.SmoothStep(0f, 1f, progreso / 0.15f))
                    : Quaternion.Slerp(dawnRot, targetRot, Mathf.SmoothStep(0f, 1f, (progreso - 0.15f) / 0.85f));
            }

            if (ownLight)
            {
                EvaluarLuzDelDia(progreso, startInt, startCol, out float intensidad, out Color color);
                ownLight.intensity = intensidad;
                ownLight.color = color;
            }

            if (controlarLuzAmbiente)
            {
                Color ambienteDia = Color.Lerp(startAmbient, _initialAmbientColor, Mathf.Clamp01(progreso / 0.15f));
                RenderSettings.ambientLight = Color.Lerp(
                    ambienteDia,
                    nightAmbientColor,
                    Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 1f, progreso)));
            }
            yield return null;
        }

        if (!bloquearRotacion)
            transform.localRotation = targetRot;
        if (ownLight)
        {
            ownLight.intensity = Mathf.Max(minIntensity, _initialIntensity * intensidadLuzNocturna);
            ownLight.color = nightColor;
        }
        if (controlarLuzAmbiente)
            RenderSettings.ambientLight = nightAmbientColor;
    }

    void EvaluarLuzDelDia(float t, float startIntensity, Color startColor, out float intensity, out Color color)
    {
        float dawnIntensity = Mathf.Max(minIntensity, _initialIntensity * 0.75f);
        float middayIntensity = Mathf.Max(minIntensity, _initialIntensity * middayIntensityFactor);
        float sunsetIntensity = Mathf.Max(minIntensity, _initialIntensity * sunsetIntensityFactor);
        float finalNightIntensity = Mathf.Max(minIntensity, _initialIntensity * intensidadLuzNocturna);

        if (t < 0.15f)
        {
            float p = Mathf.SmoothStep(0f, 1f, t / 0.15f);
            intensity = Mathf.Lerp(startIntensity, dawnIntensity, p);
            color = Color.Lerp(startColor, dawnColor, p);
        }
        else if (t < 0.5f)
        {
            float p = Mathf.SmoothStep(0f, 1f, (t - 0.15f) / 0.35f);
            intensity = Mathf.Lerp(dawnIntensity, middayIntensity, p);
            color = Color.Lerp(dawnColor, _initialColor, p);
        }
        else if (t < 0.78f)
        {
            float p = Mathf.SmoothStep(0f, 1f, (t - 0.5f) / 0.28f);
            intensity = Mathf.Lerp(middayIntensity, sunsetIntensity, p);
            color = Color.Lerp(_initialColor, sunsetColor, p);
        }
        else
        {
            float p = Mathf.SmoothStep(0f, 1f, (t - 0.78f) / 0.22f);
            intensity = Mathf.Lerp(sunsetIntensity, finalNightIntensity, p);
            color = Color.Lerp(sunsetColor, nightColor, p);
        }
    }

    void CapturarAmbienteInicialSiHaceFalta()
    {
        if (_initialAmbientCaptured)
            return;

        _initialAmbientColor = RenderSettings.ambientLight;
        _initialAmbientCaptured = true;
    }

    // Resetea gradualmente a la rotación e intensidad iniciales
    public void ResetSun()
    {
        ResetSun(resetDuration);
    }

    public void ResetSun(float duracionOverride)
    {
        StopAllCoroutines();
        CapturarAmbienteInicialSiHaceFalta();
        StartCoroutine(ResetRoutine(duracionOverride));
    }

    IEnumerator ResetRoutine(float duracionOverride)
    {
        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = _initialLocalRotation;

        var ownLight = GetComponent<Light>();
        float startInt = ownLight ? ownLight.intensity : 0f;
        float targetInt = ownLight ? _initialIntensity : startInt;
        Color startCol = ownLight ? ownLight.color : Color.white;
        Color targetCol = ownLight ? _initialColor : Color.white;
        Color startAmbient = RenderSettings.ambientLight;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duracionOverride);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float progreso = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, progreso);
            if (ownLight)
            {
                ownLight.intensity = Mathf.Lerp(startInt, targetInt, progreso);
                ownLight.color = Color.Lerp(startCol, targetCol, progreso);
            }
            if (controlarLuzAmbiente)
                RenderSettings.ambientLight = Color.Lerp(startAmbient, _initialAmbientColor, progreso);
            yield return null;
        }

        transform.localRotation = targetRot;
        if (ownLight)
        {
            ownLight.intensity = targetInt;
            ownLight.color = targetCol;
        }
        if (controlarLuzAmbiente)
            RenderSettings.ambientLight = _initialAmbientColor;
        // restaurar también el paso de rotación en Y
        pasoY = _initialPasoY;
    }

    void Aplicar(float elev) => transform.rotation = Quaternion.Euler(elev, azActual, 0f);

#if UNITY_EDITOR
    // Paso instantáneo para usar desde el Editor (fuera de Play Mode)
    public void EditorStepInstant()
    {
        if (!Fijo)
            transform.localRotation = transform.localRotation * Quaternion.Euler(0f, pasoY, 0f);
        var ownLight = GetComponent<Light>();
        if (ownLight)
        {
            ownLight.intensity = Mathf.Max(minIntensity, ownLight.intensity * intensityStepFactor);
            ownLight.color = Color.Lerp(ownLight.color, sunsetColor, colorStep);
            UnityEditor.EditorUtility.SetDirty(ownLight);
        }
        UnityEditor.EditorUtility.SetDirty(transform);
    }
#endif
}



