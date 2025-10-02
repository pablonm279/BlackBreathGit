using UnityEngine;

[DisallowMultipleComponent]
public class RandomizeScaleAnchorBottom : MonoBehaviour
{
    [Header("Rango de escala aleatoria (uniforme)")]
    [SerializeField, Range(0.5f, 2f)] float minFactor = 0.85f;
    [SerializeField, Range(0.5f, 2f)] float maxFactor = 1.15f;

    [Header("Anclaje")]
    [Tooltip("Anclar al eje Y del mundo. (Si prefieres al 'up' local, desactívalo)")]
    [SerializeField] bool anchorWorldUp = true;

    Renderer[] rends;
    Vector3 initialScale;
    float initialBottomY;
    bool done;

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();
        initialScale = transform.localScale;
        initialBottomY = GetBottomY();
    }

    void Start() => Apply();

#if UNITY_EDITOR
    // útil si duplicas en editor
    [ContextMenu("Reaplicar")]
    void EditorReapply() { done = false; Apply(); }
#endif

    void Apply()
    {
        if (done) return;
        done = true;

        if (rends == null || rends.Length == 0)
        {
            Debug.LogWarning($"[{name}] No hay Renderer para calcular el pie.");
            return;
        }

        // Escala aleatoria
        float factor =UnityEngine.Random.Range(minFactor, maxFactor);
        transform.localScale = initialScale * factor;

        // Recalcular fondo (pie) y ajustar posición para mantenerlo fijo
        float newBottomY = GetBottomY();
        float deltaY = initialBottomY - newBottomY;

        if (Mathf.Abs(deltaY) > 0.0001f)
        {
            Vector3 dir = anchorWorldUp ? Vector3.up : transform.up;
            transform.position += dir * deltaY;
        }
    }

    float GetBottomY()
    {
        Bounds b = default;
        bool has = false;
        foreach (var r in rends)
        {
            if (r == null) continue;
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        return has ? b.min.y : transform.position.y;
    }
}
