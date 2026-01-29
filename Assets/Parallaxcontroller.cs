using System;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxcontroller : MonoBehaviour
{
    [Serializable]
    public class Layer
    {
        public RectTransform rect;

        [Tooltip("Multiplicador de movimiento para esta capa. Ej: 0.02 lejos, 0.08 cerca")]
        public float strength = 0.05f;

        [Tooltip("Límite máximo de desplazamiento en píxeles para esta capa (por eje)")]
        public Vector2 maxOffset = new Vector2(30f, 20f);

        [Tooltip("Si querés que una capa solo se mueva en X o en Y")]
        public Vector2 axisMask = new Vector2(1f, 1f);

        [Tooltip("Offset manual adicional (por si querés compensar composición)")]
        public Vector2 manualOffset = Vector2.zero;

        [HideInInspector] public Vector2 baseAnchoredPos;
        [HideInInspector] public Vector2 velocity; // para SmoothDamp
    }

    [Header("Layers (orden sugerido: cielo -> lejano -> medio -> ruta -> foreground -> nubes)")]
    public List<Layer> layers = new List<Layer>();

    [Header("Input")]
    [Tooltip("Si está ON, usa mouse. Si está OFF, podés alimentar targetNormalized desde otro lado")]
    public bool useMouse = true;

    [Tooltip("Normalizado -1..1. Si useMouse es true, esto se calcula solo")]
    public Vector2 targetNormalized = Vector2.zero;

    [Tooltip("Inverte X/Y si lo sentís raro")]
    public bool invertX = false;
    public bool invertY = false;

    [Header("Feel")]
    [Tooltip("Tiempo de suavizado. Menor = más reactivo, mayor = más flotante")]
    [Range(0.01f, 0.5f)]
    public float smoothTime = 0.12f;

    [Tooltip("Curva para que el centro sea más tranquilo y los bordes aceleren")]
    public AnimationCurve responseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Auto Drift")]
    public bool enableAutoDrift = true;

    [Tooltip("Fuerza del drift (normalizado). Ej: 0.03")]
    [Range(0f, 0.2f)]
    public float driftStrength = 0.03f;

    [Tooltip("Velocidad del drift")]
    public float driftSpeed = 0.25f;

    private Vector2 drift;

    void Awake()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].rect != null)
                layers[i].baseAnchoredPos = layers[i].rect.anchoredPosition;
        }
    }

    void OnEnable()
    {
        // Re-captura por si moviste algo en editor
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].rect != null)
                layers[i].baseAnchoredPos = layers[i].rect.anchoredPosition;
        }
    }

    void Update()
    {
        Vector2 input = targetNormalized;

        if (useMouse)
        {
            Vector2 mp = Input.mousePosition;
            float nx = (mp.x / Screen.width) * 2f - 1f;
            float ny = (mp.y / Screen.height) * 2f - 1f;
            input = new Vector2(nx, ny);
        }

        if (invertX) input.x *= -1f;
        if (invertY) input.y *= -1f;

        input = Vector2.ClampMagnitude(input, 1f);

        if (enableAutoDrift)
        {
            float t = Time.unscaledTime * driftSpeed;
            drift = new Vector2(Mathf.Sin(t * 1.13f), Mathf.Cos(t * 0.97f)) * driftStrength;
        }
        else
        {
            drift = Vector2.zero;
        }

        Vector2 final = input + drift;
        final = Vector2.ClampMagnitude(final, 1f);

        // Curva de respuesta (centro más suave)
        float mag = Mathf.Clamp01(final.magnitude);
        float curvedMag = responseCurve.Evaluate(mag);
        if (mag > 0.0001f) final = final.normalized * curvedMag;

        for (int i = 0; i < layers.Count; i++)
        {
            var L = layers[i];
            if (L.rect == null) continue;

            Vector2 desiredOffset = final * L.strength;
            desiredOffset = new Vector2(
                Mathf.Clamp(desiredOffset.x, -1f, 1f),
                Mathf.Clamp(desiredOffset.y, -1f, 1f)
            );

            // Convertimos a píxeles usando maxOffset
            Vector2 pxOffset = new Vector2(desiredOffset.x * L.maxOffset.x, desiredOffset.y * L.maxOffset.y);

            // Máscaras de eje
            pxOffset = Vector2.Scale(pxOffset, L.axisMask);

            Vector2 targetPos = L.baseAnchoredPos + pxOffset + L.manualOffset;

            // Suavizado
            L.rect.anchoredPosition = Vector2.SmoothDamp(
                L.rect.anchoredPosition,
                targetPos,
                ref L.velocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
        }
    }
}
