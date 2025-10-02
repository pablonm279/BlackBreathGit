using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIGlowPulse : MonoBehaviour
{
    private Image img;
    private Color baseColor;

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow; // Color del glow
    public float minAlpha = 0.2f;          // Transparencia mínima
    public float maxAlpha = 1.2f;            // Transparencia máxima
    public float pulseSpeed = 0.7f;          // Velocidad de pulso
    public bool syncWithOthers = false;    // Si querés sincronizar varios glows

    void Awake()
    {
        img = GetComponent<Image>();
        baseColor = glowColor;
    }

    void Update()
    {
        // Tiempo base: si sync está activo, todos comparten el mismo valor
        float t = syncWithOthers ? Time.time : (Time.time + GetInstanceID() * 0.1f);

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(t * pulseSpeed) + 1f) / 2f);
        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
