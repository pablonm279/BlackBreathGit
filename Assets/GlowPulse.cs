using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GlowPulse : MonoBehaviour
{
    private Material mat;
    private Color baseColor;

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow; // Color del glow
    public float minIntensity = 0.3f;      // Intensidad mínima
    public float maxIntensity = 2.0f;      // Intensidad máxima
    public float pulseSpeed = 0.7f;          // Velocidad del pulso

    void Start()
    {
        // Clonar material para no modificar el original
        mat = GetComponent<Renderer>().material;
        baseColor = glowColor.linear;
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // Oscilar intensidad
        float emission = Mathf.Lerp(minIntensity, maxIntensity,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);

        // Aplicar color * intensidad
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
        mat.SetColor("_EmissionColor", finalColor);
    }
}
