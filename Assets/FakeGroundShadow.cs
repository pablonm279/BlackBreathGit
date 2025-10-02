using UnityEngine;

[DisallowMultipleComponent]
public class FakeGroundShadow : MonoBehaviour
{
    [Header("Opcional: asignar explícitamente el objeto a sombrear")]
    public Transform target;              // si es null usa el padre
    [Tooltip("Capas consideradas como suelo para el Raycast")]
    public LayerMask groundMask = ~0;

    [Header("Ajustes de tamaño/altura")]
    [Range(0.6f, 1.6f)] public float scaleMultiplier = 1.0f;
    [Tooltip("Separación del suelo para evitar z-fighting")]
    public float yOffset = 0.015f;

    [Header("Actualización")]
    [Tooltip("Si el objeto se mueve/escala, mantener la sombra actualizada")]
    public bool updateEveryFrame = false;

    Renderer[] targetRenderers;
    Transform tf;

    void Awake()
    {
        tf = transform;
        if (!target) target = tf.parent;
        if (target) targetRenderers = target.GetComponentsInChildren<Renderer>();
        PrepareMaterialIfNeeded();
        AlignAndScale();
    }

    void LateUpdate()
    {
        if (updateEveryFrame) AlignAndScale();
    }

    void PrepareMaterialIfNeeded()
    {
        // Si no tiene material, crea uno Unlit/Transparent automáticamente
        var mr = GetComponent<MeshRenderer>();
        if (!mr) return;
        if (!mr.sharedMaterial)
        {
            var sh = Shader.Find("Unlit/Transparent");
            if (!sh) sh = Shader.Find("Particles/Standard Unlit");
            if (sh)
            {
                var mat = new Material(sh);
                mat.color = new Color(0f, 0f, 0f, 1f); // la textura maneja el alpha suave
                mr.sharedMaterial = mat;
            }
        }
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    void AlignAndScale()
    {
        if (!target) return;

        // 1) Calcular footprint del target (bounds en XZ)
        Bounds b = default;
        bool has = false;
        foreach (var r in targetRenderers)
        {
            if (!r) continue;
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!has) b = new Bounds(target.position, Vector3.one * 0.5f);

        float sizeX = Mathf.Max(0.1f, b.size.x);
        float sizeZ = Mathf.Max(0.1f, b.size.z);
        float diameter = Mathf.Max(sizeX, sizeZ) * scaleMultiplier;

        // Los Quads en Unity están en el plano XY y su normal mira +Z.
        // Alineamos la normal del Quad a la normal del suelo.
        Vector3 origin = b.center + Vector3.up * 2f;
        if (Physics.Raycast(origin, Vector3.down, out var hit, 10f, groundMask, QueryTriggerInteraction.Ignore))
        {
            // Orientar: local +Z (normal del quad) => normal del suelo
            tf.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            // Posicionar: sobre el punto de contacto + pequeño offset
            tf.position = hit.point + hit.normal * yOffset;
        }
        else
        {
            // Fallback si no golpea nada
            tf.position = new Vector3(b.center.x, b.min.y + yOffset, b.center.z);
            tf.rotation = Quaternion.identity;
        }

        // 2) Escalar el quad para abarcar el pie del objeto
        // (Y=1 porque el quad es plano; sólo necesitamos XZ)
        tf.localScale = new Vector3(diameter, 1f, diameter);
    }
}
