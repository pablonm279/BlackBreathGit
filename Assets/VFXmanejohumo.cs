using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class UIFogBreath : MonoBehaviour
{
    public float amplitude = 12f;      // píxeles de vaivén horizontal
    public float speed = 0.2f;         // velocidad del vaivén
    public float emissionBase = 12f;   // emisión media
    public float emissionPulse = 6f;   // cuánto sube y baja
    public float alphaBase = 0.28f;    // alpha base del material
    public float alphaPulse = 0.06f;   // pulso de alpha

    RectTransform rt;
    ParticleSystem ps;
    ParticleSystem.EmissionModule em;
    Material mat;
    Vector2 startPos;
    float seed;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt != null) startPos = rt.anchoredPosition;
        ps = GetComponent<ParticleSystem>();
        em = ps.emission;
        var rend = GetComponent<ParticleSystemRenderer>();
        if (rend != null) mat = rend.material;
        seed =UnityEngine.Random.Range(0f, 100f);
    }

    void Update()
    {
        float t = Time.unscaledTime + seed;

        // Paneo sutil (si está bajo Canvas)
        if (rt != null)
            rt.anchoredPosition = startPos + new Vector2(Mathf.Sin(t * speed) * amplitude, 0f);

        // Emission pulsing
        var rate = new ParticleSystem.MinMaxCurve(emissionBase + Mathf.Sin(t * (speed * 0.7f)) * emissionPulse);
        em.rateOverTime = rate;

        // Alpha pulsing (requiere material instanciado)
        if (mat != null)
        {
            Color c = mat.color;
            c.a = Mathf.Clamp01(alphaBase + Mathf.Sin(t * (speed * 0.9f)) * alphaPulse);
            mat.color = c;
        }
    }
}
