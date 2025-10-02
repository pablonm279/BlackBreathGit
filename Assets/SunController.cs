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
    public float azimuthDerecha = 90f; // Y que corresponde a “derecha” en tu mapa

    [Header("Paso por viaje (Y)")]
    [FormerlySerializedAs("pasoX")] public float pasoY = -4f; // grados que rota en Y por cada avance

    [Header("Atardecer")]
    [Range(0.5f, 1f)] public float intensityStepFactor = 0.95f; // 5% menos por step
    [Range(0f, 1f)] public float colorStep = 0.05f;             // cuánto se acerca al color atardecer por step
    public Color sunsetColor = new Color(1f, 0.45f, 0.1f);       // naranja/rojizo
    [Min(0f)] public float minIntensity = 0f;                    // intensidad mínima (0 = sin límite)

    [Header("Reset")]
    public float resetDuration = 6f;  // segundos para volver a la posición inicial
    public bool resetIntensity = true; // también restaurar intensidad del sol

    float azActual;
    Quaternion _initialLocalRotation;
    float _initialIntensity;
    Color _initialColor;
    float _initialPasoY;

    void Awake()
    {
        azActual = transform.eulerAngles.y;
        _initialLocalRotation = transform.localRotation;
        var ownLight = GetComponent<Light>();
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

    // Llamá esto UNA VEZ cuando empieza cada viaje
    public void OnTravelStart()
    {
        StopAllCoroutines();
        StartCoroutine(Elevar());
    }

    IEnumerator Elevar()
    {
        // Rotación SOLO en eje Y, acumulativa y robusta (espacio local)
        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, pasoY, 0f);

        // Usar el Light de este mismo GameObject
        var ownLight = GetComponent<Light>();

        // Atenuar intensidad ~5% y acercar color hacia atardecer gradualmente
        float startInt = ownLight ? ownLight.intensity : 0f;
        float targetInt = ownLight ? Mathf.Max(minIntensity, startInt * intensityStepFactor) : 0f;
        Color startCol = ownLight ? ownLight.color : Color.white;
        Color targetCol = ownLight ? Color.Lerp(startCol, sunsetColor, colorStep) : Color.white;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duracion);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            if (ownLight)
            {
                ownLight.intensity = Mathf.Lerp(startInt, targetInt, t);
                ownLight.color = Color.Lerp(startCol, targetCol, t);
            }
            yield return null;
        }

        // Fijar el estado final exacto
        transform.localRotation = targetRot;
        if (ownLight)
        {
            ownLight.intensity = targetInt;
            ownLight.color = targetCol;
        }
    }

    // Resetea gradualmente a la rotación e intensidad iniciales
    public void ResetSun()
    {
        StopAllCoroutines();
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = _initialLocalRotation;

        var ownLight = GetComponent<Light>();
        float startInt = ownLight ? ownLight.intensity : 0f;
        float targetInt = ownLight ? _initialIntensity : startInt;
        Color startCol = ownLight ? ownLight.color : Color.white;
        Color targetCol = ownLight ? _initialColor : Color.white;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, resetDuration);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            if (ownLight)
            {
                ownLight.intensity = Mathf.Lerp(startInt, targetInt, t);
                ownLight.color = Color.Lerp(startCol, targetCol, t);
            }
            yield return null;
        }

        transform.localRotation = targetRot;
        if (ownLight)
        {
            ownLight.intensity = targetInt;
            ownLight.color = targetCol;
        }
        // restaurar también el paso de rotación en Y
        pasoY = _initialPasoY;
    }

    void Aplicar(float elev) => transform.rotation = Quaternion.Euler(elev, azActual, 0f);

#if UNITY_EDITOR
    // Paso instantáneo para usar desde el Editor (fuera de Play Mode)
    public void EditorStepInstant()
    {
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
