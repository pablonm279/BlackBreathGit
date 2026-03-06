using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Decorador con Poisson blue-noise + exclusiones.
/// - Grilla en MUNDO (respeta escala del plane).
/// - No pisa caminos: LineRenderer (ancho real o mínimo) + margen + (opcional) colliders por capa.
/// - No sale del plane; ajusta solo Y contra el suelo correcto.
/// - No toca escala de prefabs; rotación Y aleatoria opcional.
[DisallowMultipleComponent]
public class MapDecorator : MonoBehaviour
{
    [Header("área de trabajo (OBLIGATORIO)")]
    [SerializeField] MeshFilter planeMesh;

    [Header("Caminos y Nodos (exclusiones)")]
    [Tooltip("Usar SOLO LineRenderers con este tag (recomendado).")]
    [SerializeField] bool soloLineRenderersConTag = true;
    [SerializeField] string tagCaminos = "Camino";
    [SerializeField] string[] tagsCaminosAlternativos = new[] { "camino", "CAMINO" };
    [SerializeField] string tagNodos = "Nodo";

    [Tooltip("Margen extra al ancho del camino (m).")]
    [SerializeField] float margenCamino = 1.5f;

    [Tooltip("Inflado extra del rectángulo del plane al filtrar caminos (m).")]
    [SerializeField] float filtroCaminosInflar = 3f;

    [Tooltip("Piso mínimo del ancho del camino en MUNDO (m) por si el LR usa width muy chico.")]
    [SerializeField] float anchoCaminoMinWorld = 6f;

    [Header("Parámetros de colocación")]
    [SerializeField] float distCamino = 7f;   // se suma al halfWidth del camino
    [SerializeField] float distNodo   = 5f;
    [SerializeField] float radioPoisson = 3.5f;
    [SerializeField] int   intentosPorPunto = 30;

    [Header("Altura / terreno")]
    [SerializeField] bool ajustarAlturaConRaycast = true;
    [SerializeField] bool raycastSoloContraPlane = true;
    [SerializeField] LayerMask capaSuelo = ~0;

    [Header("Altura precisa (opcional)")]
    [SerializeField] bool usarColliderSueloDirecto = false;
    [SerializeField] Collider sueloCollider; // arrastrá el collider del plane si usás esta opción

    [Header("Exclusión por capa (colliders de camino)")]
    [SerializeField] bool usarExclusionPorCapaCaminos = false;
    [SerializeField] LayerMask capasCaminos;
    [SerializeField] float radioCheckCamino = 1.0f; // acolchado extra del checkSphere

    [Header("Aleatoriedad / estática")]
    [SerializeField] bool rotarYRandom = false;
    [SerializeField] int  semilla = 12345;
    [SerializeField] bool usarSemillaAleatoria = false;

    [Header("Rendimiento (async)")]
    [SerializeField] int porFrame = 60;

    // ---- internos ----
    struct Segmento { public Vector3 a, b; public float halfWidth; }
    readonly List<Segmento> segmentos = new List<Segmento>();
    readonly List<Transform> nodos = new List<Transform>();

    Transform tPlane;
    Bounds   localBounds;
    Vector3  minL, maxL, sizeL;
    float    yLocalPlano;
    float    safeMinX, safeMaxX, safeMinZ, safeMaxZ;

    // Tamaño en MUNDO
    float sx, sz;             // |lossyScale| X y Z
    float sizeWorldX, sizeWorldZ;

    // Rect del plane en MUNDO (AABB)
    float rectMinX, rectMaxX, rectMinZ, rectMaxZ;

    System.Random rng;

    // Hash espacial (grilla en MUNDO)
    struct CellKey { public int x, z; public CellKey(int X, int Z){ x=X; z=Z; } }
    Dictionary<CellKey, List<Vector3>> grid;
    float cell; // r / sqrt(2)

    // índices de exclusión (grilla en mundo) para acelerar PasaExclusiones
    Dictionary<CellKey, List<int>> segmentGrid;
    Dictionary<CellKey, List<int>> nodeGrid;
    List<int> segmentCandidates = new List<int>(64);
    List<int> nodeCandidates = new List<int>(32);
    int[] segmentVisitStamp = System.Array.Empty<int>();
    int[] nodeVisitStamp = System.Array.Empty<int>();
    int currentSegmentStamp = 1;
    int currentNodeStamp = 1;
    float exclusionCellSize = 1f;
    float maxSegmentHalfWidth = 0f;

    void Awake()
    {
        if (!planeMesh || !planeMesh.sharedMesh)
        {
            Debug.LogError("[MapDecorator] Asigná un MeshFilter vélido.");
            enabled = false; return;
        }

        tPlane      = planeMesh.transform;
        localBounds = planeMesh.sharedMesh.bounds;
        minL        = localBounds.min;
        maxL        = localBounds.max;
        sizeL       = localBounds.size;
        yLocalPlano = localBounds.center.y;
        safeMinX    = minL.x; safeMaxX = maxL.x;
        safeMinZ    = minL.z; safeMaxZ = maxL.z;

        // Escalas absolutas en mundo
        sx = Mathf.Abs(tPlane.lossyScale.x);
        sz = Mathf.Abs(tPlane.lossyScale.z);

        sizeWorldX = sizeL.x * sx;
        sizeWorldZ = sizeL.z * sz;

        // Rect mundo del plane (AABB del bounds transformado)
        var b = planeMesh.sharedMesh.bounds;
        Vector3 aW = tPlane.TransformPoint(new Vector3(b.min.x, b.center.y, b.min.z));
        Vector3 bW = tPlane.TransformPoint(new Vector3(b.max.x, b.center.y, b.min.z));
        Vector3 cW = tPlane.TransformPoint(new Vector3(b.max.x, b.center.y, b.max.z));
        Vector3 dW = tPlane.TransformPoint(new Vector3(b.min.x, b.center.y, b.max.z));
        rectMinX = Mathf.Min(Mathf.Min(aW.x, bW.x), Mathf.Min(cW.x, dW.x));
        rectMaxX = Mathf.Max(Mathf.Max(aW.x, bW.x), Mathf.Max(cW.x, dW.x));
        rectMinZ = Mathf.Min(Mathf.Min(aW.z, bW.z), Mathf.Min(cW.z, dW.z));
        rectMaxZ = Mathf.Max(Mathf.Max(aW.z, bW.z), Mathf.Max(cW.z, dW.z));

        ActualizarZonaSegura();

        if (transform.lossyScale != Vector3.one)
            Debug.LogWarning("[MapDecorator] Este GameObject deberáa estar en escala (1,1,1) para no heredar escalas raras.");
    }

    // ===================== API =====================
    public void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    // Firma compatible con tu llamada previa
    public int Generar(GameObject prefab, int cantidad, float distCamino, float distNodo, float r, int k = 100)
        => GenerarSync(prefab, cantidad, distCamino, distNodo, r, k);
    public int Generar(GameObject prefab, int cantidad)
        => GenerarSync(prefab, cantidad, this.distCamino, this.distNodo, this.radioPoisson, this.intentosPorPunto);

    public int GenerarSync(GameObject prefab, int cantidad,
                           float distCaminoOverride, float distNodoOverride,
                           float rOverride, int kOverride)
    {
        Preparar(prefab, distCaminoOverride, distNodoOverride, rOverride, kOverride);
        int col = ColocarPoisson(prefab, cantidad);
//        Debug.Log($"[MapDecorator] (Sync) Colocados {col}/{cantidad} '{prefab?.name}'.");
        return col;
    }

    public void GenerarAsync(MonoBehaviour runner, GameObject prefab, int cantidad,
                             float distCaminoOverride, float distNodoOverride,
                             float rOverride, int kOverride)
    {
        Preparar(prefab, distCaminoOverride, distNodoOverride, rOverride, kOverride);
        runner.StartCoroutine(ColocarPoissonCR(prefab, cantidad));
    }

    // Variante que permite al llamador hacer yield hasta terminar
    public IEnumerator GenerarAsyncCR(GameObject prefab, int cantidad,
                                      float distCaminoOverride, float distNodoOverride,
                                      float rOverride, int kOverride)
    {
        Preparar(prefab, distCaminoOverride, distNodoOverride, rOverride, kOverride);
        yield return ColocarPoissonCR(prefab, cantidad);
    }

    public IEnumerator GenerarAsyncCR(GameObject prefab, int cantidad)
    {
        Preparar(prefab, this.distCamino, this.distNodo, this.radioPoisson, this.intentosPorPunto);
        yield return ColocarPoissonCR(prefab, cantidad);
    }

    // ===================== Preparación =====================
    void Preparar(GameObject prefab, float distCaminoOverride, float distNodoOverride, float rOverride, int kOverride)
    {
        if (prefab == null) { Debug.LogError("[MapDecorator] Prefab nulo."); return; }

        if (usarSemillaAleatoria) semilla = System.Guid.NewGuid().GetHashCode();
        rng = new System.Random(semilla);

        this.distCamino = distCaminoOverride;
        this.distNodo   = distNodoOverride;
        this.radioPoisson = rOverride;
        this.intentosPorPunto = kOverride;

        ActualizarZonaSegura();

        cell = radioPoisson / Mathf.Sqrt(2f);
        grid = new Dictionary<CellKey, List<Vector3>>(1024);

        RecolectarCaminosFiltrados();
        RecolectarNodos();
        ConstruirIndicesExclusion();

       // Debug.Log($"[MapDecorator] área={tPlane.name}  SizeWorld=({sizeWorldX:F1},{sizeWorldZ:F1})  r={radioPoisson}  k={intentosPorPunto}  distCamino={distCamino}  distNodo={distNodo}  Caminos={segmentos.Count} tramos  Nodos={nodos.Count}");
    }

    // ===================== Núcleo =====================
    int ColocarPoisson(GameObject prefab, int cantidad)
    {
        int colocados = 0;

        if (!TryPrimerPunto(out var p0))
        {
            Debug.LogWarning("[MapDecorator] No hay espacio inicial libre (revisá r/distancias/área).");
            return 0;
        }
        Registrar(p0); Instanciar(prefab, p0); colocados++;

        List<Vector3> activos = new List<Vector3> { p0 };

        while (activos.Count > 0 && colocados < cantidad)
        {
            int idx = rng.Next(activos.Count);
            var baseP = activos[idx];
            bool ok = false;

            for (int i = 0; i < intentosPorPunto; i++)
            {
                var dir = RandomUnit();
                float rad = radioPoisson * (1f + (float)rng.NextDouble()); // [r,2r)
                Vector3 q = MoverEnPlano(baseP, dir, rad);
                if (!DentroDelRect(q)) continue;
                if (!PasaExclusiones(q)) continue;
                if (!PasaPoisson(q)) continue;

                Registrar(q); activos.Add(q);
                Instanciar(prefab, q);
                colocados++;
                ok = true;
                break;
            }

            if (!ok) activos.RemoveAt(idx);
        }

        return colocados;
    }

    IEnumerator ColocarPoissonCR(GameObject prefab, int cantidad)
    {
        int colocados = 0;

        if (!TryPrimerPunto(out var p0))
        {
            Debug.LogWarning("[MapDecorator] No hay espacio inicial libre (revisá r/distancias/área).");
            yield break;
        }
        Registrar(p0); Instanciar(prefab, p0); colocados++;

        List<Vector3> activos = new List<Vector3> { p0 };
        int hechosEsteFrame = 0;

        while (activos.Count > 0 && colocados < cantidad)
        {
            int idx = rng.Next(activos.Count);
            var baseP = activos[idx];
            bool ok = false;

            for (int i = 0; i < intentosPorPunto; i++)
            {
                var dir = RandomUnit();
                float rad = radioPoisson * (1f + (float)rng.NextDouble());
                Vector3 q = MoverEnPlano(baseP, dir, rad);
                if (!DentroDelRect(q)) continue;
                if (!PasaExclusiones(q)) continue;
                if (!PasaPoisson(q)) continue;

                Registrar(q); activos.Add(q);
                Instanciar(prefab, q);
                colocados++; hechosEsteFrame++;
                ok = true;
                break;
            }

            if (!ok) activos.RemoveAt(idx);

            if (hechosEsteFrame >= porFrame)
            {
                hechosEsteFrame = 0;
                yield return null;
            }
        }

//        Debug.Log($"[MapDecorator] (Async) Colocados {colocados}/{cantidad} '{prefab?.name}'.");
    }

    void ActualizarZonaSegura()
    {
        float margen = Mathf.Max(0f, distCamino);
        float margenLocalX = margen / Mathf.Max(1e-6f, sx);
        float margenLocalZ = margen / Mathf.Max(1e-6f, sz);

        float propuestoMinX = minL.x + margenLocalX;
        float propuestoMaxX = maxL.x - margenLocalX;
        if (propuestoMinX > propuestoMaxX)
        {
            float centro = 0.5f * (minL.x + maxL.x);
            safeMinX = safeMaxX = centro;
            Debug.LogWarning("[MapDecorator] distCamino demasiado grande para el ancho del plane (eje X).");
        }
        else
        {
            safeMinX = propuestoMinX;
            safeMaxX = propuestoMaxX;
        }

        float propuestoMinZ = minL.z + margenLocalZ;
        float propuestoMaxZ = maxL.z - margenLocalZ;
        if (propuestoMinZ > propuestoMaxZ)
        {
            float centro = 0.5f * (minL.z + maxL.z);
            safeMinZ = safeMaxZ = centro;
            Debug.LogWarning("[MapDecorator] distCamino demasiado grande para el largo del plane (eje Z).");
        }
        else
        {
            safeMinZ = propuestoMinZ;
            safeMaxZ = propuestoMaxZ;
        }
    }

    // ===================== Primer punto =====================
    bool TryPrimerPunto(out Vector3 p0)
    {
        for (int i = 0; i < 400; i++)
        {
            var cand = RandomPointInsideRect();
            if (PasaExclusiones(cand) && PasaPoisson(cand)) { p0 = cand; return true; }
        }
        p0 = Vector3.zero;
        return false;
    }

    // ===================== Exclusiones y recolección =====================
    void RecolectarCaminosFiltrados()
    {
        segmentos.Clear();

        List<LineRenderer> fuentes = new List<LineRenderer>();

        if (soloLineRenderersConTag)
        {
            // Tag principal
            foreach (var go in FindByTagSafe(tagCaminos))
            {
                var lr = go.GetComponent<LineRenderer>();
                if (lr) AgregarSiIntersecta(lr, fuentes);
            }
            // Tags alternativos
            for (int i = 0; i < tagsCaminosAlternativos.Length; i++)
            {
                foreach (var go in FindByTagSafe(tagsCaminosAlternativos[i]))
                {
                    var lr = go.GetComponent<LineRenderer>();
                    if (lr) AgregarSiIntersecta(lr, fuentes);
                }
            }
        }
        else
        {
            var todos = FindObjectsOfType<LineRenderer>(true);
            for (int i = 0; i < todos.Length; i++) AgregarSiIntersecta(todos[i], fuentes);
        }

        float expand = distCamino + margenCamino + filtroCaminosInflar;

        foreach (var lr in fuentes)
        {
            if (!lr || lr.positionCount < 2) continue;

            float worldWidthLR = MaxWidthWorld(lr);                         // ancho real del LR
            float worldWidth   = Mathf.Max(worldWidthLR, anchoCaminoMinWorld); // piso mínimo
            float half         = 0.5f * worldWidth + margenCamino;

            for (int i = 0; i < lr.positionCount - 1; i++)
            {
                Vector3 a = LRPointWorld(lr, i);         // FIX: respeta useWorldSpace
                Vector3 b = LRPointWorld(lr, i + 1);     // FIX: respeta useWorldSpace
                if (!SegmentoIntersecaRectExpandido(a, b, expand)) continue;

                segmentos.Add(new Segmento { a = a, b = b, halfWidth = half });
            }
        }
    }

    void AgregarSiIntersecta(LineRenderer lr, List<LineRenderer> lista)
    {
        if (!lr) return;
        var bb = CalcularAABBLineRenderer(lr);
        if (AABBOverlap(bb.minX, bb.maxX, bb.minZ, bb.maxZ, rectMinX, rectMaxX, rectMinZ, rectMaxZ))
            lista.Add(lr);
    }

    (float minX, float maxX, float minZ, float maxZ) CalcularAABBLineRenderer(LineRenderer lr)
    {
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;

        for (int i = 0; i < lr.positionCount; i++)
        {
            Vector3 p = LRPointWorld(lr, i);   // FIX: respeta useWorldSpace
            if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x;
            if (p.z < minZ) minZ = p.z; if (p.z > maxZ) maxZ = p.z;
        }
        return (minX, maxX, minZ, maxZ);
    }

    bool SegmentoIntersecaRectExpandido(Vector3 a, Vector3 b, float expand)
    {
        float minX = Mathf.Min(a.x, b.x), maxX = Mathf.Max(a.x, b.x);
        float minZ = Mathf.Min(a.z, b.z), maxZ = Mathf.Max(a.z, b.z);

        return AABBOverlap(minX, maxX, minZ, maxZ,
                           rectMinX - expand, rectMaxX + expand,
                           rectMinZ - expand, rectMaxZ + expand);
    }

    static bool AABBOverlap(float minAx, float maxAx, float minAz, float maxAz,
                            float minBx, float maxBx, float minBz, float maxBz)
    {
        return (minAx <= maxBx) && (maxAx >= minBx) && (minAz <= maxBz) && (maxAz >= minBz);
    }

    static float MaxWidthWorld(LineRenderer lr)
    {
        float curveMax = 0f;
        var curve = lr.widthCurve;
        if (curve != null && curve.keys != null && curve.keys.Length > 0)
            for (int k = 0; k < curve.keys.Length; k++)
                curveMax = Mathf.Max(curveMax, curve.keys[k].value);

        float w = curveMax > 0f ? curveMax * lr.widthMultiplier
                                : Mathf.Max(lr.startWidth, lr.endWidth);

        return Mathf.Max(0.0001f, w);
    }

    void RecolectarNodos()
    {
        nodos.Clear();
        foreach (var go in FindByTagSafe(tagNodos)) nodos.Add(go.transform);
    }

    void ConstruirIndicesExclusion()
    {
        maxSegmentHalfWidth = 0f;
        for (int i = 0; i < segmentos.Count; i++)
            if (segmentos[i].halfWidth > maxSegmentHalfWidth) maxSegmentHalfWidth = segmentos[i].halfWidth;

        float maxSegmentRange = distCamino + maxSegmentHalfWidth;
        exclusionCellSize = Mathf.Max(0.5f, Mathf.Max(radioPoisson, Mathf.Max(distNodo, maxSegmentRange)));

        segmentGrid = new Dictionary<CellKey, List<int>>(Mathf.Max(64, segmentos.Count * 2));
        nodeGrid = new Dictionary<CellKey, List<int>>(Mathf.Max(32, nodos.Count * 2));

        if (segmentVisitStamp.Length < segmentos.Count) segmentVisitStamp = new int[segmentos.Count];
        if (nodeVisitStamp.Length < nodos.Count) nodeVisitStamp = new int[nodos.Count];
        currentSegmentStamp = 1;
        currentNodeStamp = 1;

        for (int i = 0; i < segmentos.Count; i++)
        {
            var s = segmentos[i];
            float range = distCamino + s.halfWidth;
            float minX = Mathf.Min(s.a.x, s.b.x) - range;
            float maxX = Mathf.Max(s.a.x, s.b.x) + range;
            float minZ = Mathf.Min(s.a.z, s.b.z) - range;
            float maxZ = Mathf.Max(s.a.z, s.b.z) + range;
            IndexarRect(segmentGrid, minX, maxX, minZ, maxZ, i);
        }

        float nodoRange = distNodo;
        for (int i = 0; i < nodos.Count; i++)
        {
            Vector3 p = nodos[i].position;
            IndexarRect(nodeGrid, p.x - nodoRange, p.x + nodoRange, p.z - nodoRange, p.z + nodoRange, i);
        }
    }

    void IndexarRect(Dictionary<CellKey, List<int>> indice, float minX, float maxX, float minZ, float maxZ, int item)
    {
        var cmin = CellFromWorldForExclusion(minX, minZ);
        var cmax = CellFromWorldForExclusion(maxX, maxZ);
        for (int x = cmin.x; x <= cmax.x; x++)
        for (int z = cmin.z; z <= cmax.z; z++)
        {
            var key = new CellKey(x, z);
            if (!indice.TryGetValue(key, out var list))
            {
                list = new List<int>(4);
                indice.Add(key, list);
            }
            list.Add(item);
        }
    }

    static GameObject[] FindByTagSafe(string tag)
    {
        try { return GameObject.FindGameObjectsWithTag(tag); }
        catch { return System.Array.Empty<GameObject>(); }
    }

    bool PasaExclusiones(Vector3 p)
    {
        // 1) por segmentos de LR (con ancho real + piso mínimo) usando índice espacial
        if (segmentos.Count > 0)
        {
            currentSegmentStamp++;
            if (currentSegmentStamp == int.MaxValue)
            {
                System.Array.Clear(segmentVisitStamp, 0, segmentVisitStamp.Length);
                currentSegmentStamp = 1;
            }

            segmentCandidates.Clear();
            float queryRange = distCamino + maxSegmentHalfWidth;
            RecolectarCandidatos(segmentGrid, p.x, p.z, queryRange, segmentCandidates, segmentVisitStamp, currentSegmentStamp);

            for (int i = 0; i < segmentCandidates.Count; i++)
            {
                var s = segmentos[segmentCandidates[i]];
                float permitido = distCamino + s.halfWidth;
                if (DistSegXZSqr(p, s.a, s.b) < permitido * permitido) return false;
            }
        }

        // 2) por colisión de capa (malla de camino)
        if (usarExclusionPorCapaCaminos && TocaCaminoPorCapa(p)) return false;

        // 3) nodos usando índice espacial
        if (nodos.Count > 0)
        {
            currentNodeStamp++;
            if (currentNodeStamp == int.MaxValue)
            {
                System.Array.Clear(nodeVisitStamp, 0, nodeVisitStamp.Length);
                currentNodeStamp = 1;
            }

            nodeCandidates.Clear();
            RecolectarCandidatos(nodeGrid, p.x, p.z, distNodo, nodeCandidates, nodeVisitStamp, currentNodeStamp);

            float distNodoSqr = distNodo * distNodo;
            for (int i = 0; i < nodeCandidates.Count; i++)
                if (SqrDistXZ(p, nodos[nodeCandidates[i]].position) < distNodoSqr) return false;
        }

        return true;
    }

    bool TocaCaminoPorCapa(Vector3 p)
    {
        float r = distCamino + radioCheckCamino;
        return Physics.CheckSphere(p + Vector3.up * 0.5f, r, capasCaminos, QueryTriggerInteraction.Collide);
    }

    // ===================== Blue-noise (hash en MUNDO) =====================
    bool PasaPoisson(Vector3 q)
    {
        var key = CellFromWorld(q);
        for (int ix = key.x - 1; ix <= key.x + 1; ix++)
        for (int iz = key.z - 1; iz <= key.z + 1; iz++)
        {
            var k = new CellKey(ix, iz);
            if (grid.TryGetValue(k, out var lista))
                for (int i = 0; i < lista.Count; i++)
                    if (SqrDistXZ(lista[i], q) < radioPoisson * radioPoisson) return false;
        }
        return true;
    }

    void Registrar(Vector3 p)
    {
        var key = CellFromWorld(p);
        if (!grid.TryGetValue(key, out var lista))
        {
            lista = new List<Vector3>(4);
            grid.Add(key, lista);
        }
        lista.Add(p);
    }

    void RecolectarCandidatos(
        Dictionary<CellKey, List<int>> indice,
        float worldX,
        float worldZ,
        float rango,
        List<int> salida,
        int[] visitStamp,
        int stamp)
    {
        var cmin = CellFromWorldForExclusion(worldX - rango, worldZ - rango);
        var cmax = CellFromWorldForExclusion(worldX + rango, worldZ + rango);

        for (int x = cmin.x; x <= cmax.x; x++)
        for (int z = cmin.z; z <= cmax.z; z++)
        {
            var key = new CellKey(x, z);
            if (!indice.TryGetValue(key, out var list)) continue;
            for (int i = 0; i < list.Count; i++)
            {
                int idx = list[i];
                if (idx < 0 || idx >= visitStamp.Length) continue;
                if (visitStamp[idx] == stamp) continue;
                visitStamp[idx] = stamp;
                salida.Add(idx);
            }
        }
    }

    // *** indexar usando DISTANCIAS EN MUNDO ***
    CellKey CellFromWorld(Vector3 w)
    {
        Vector3 local = tPlane.InverseTransformPoint(w);
        float wx = (local.x - minL.x) * sx; // pasar a unidades de mundo
        float wz = (local.z - minL.z) * sz;
        int gx = Mathf.FloorToInt(wx / cell);
        int gz = Mathf.FloorToInt(wz / cell);
        return new CellKey(gx, gz);
    }

    CellKey CellFromWorldForExclusion(float worldX, float worldZ)
    {
        float gx = (worldX - rectMinX) / Mathf.Max(1e-6f, exclusionCellSize);
        float gz = (worldZ - rectMinZ) / Mathf.Max(1e-6f, exclusionCellSize);
        return new CellKey(Mathf.FloorToInt(gx), Mathf.FloorToInt(gz));
    }

    // ===================== Util geométricas =====================
    Vector3 RandomPointInsideRect()
    {
        float rx = Mathf.Lerp(safeMinX, safeMaxX, (float)rng.NextDouble());
        float rz = Mathf.Lerp(safeMinZ, safeMaxZ, (float)rng.NextDouble());
        return tPlane.TransformPoint(new Vector3(rx, yLocalPlano, rz));
    }

    Vector3 MoverEnPlano(Vector3 baseWorld, Vector2 dir, float dist)
    {
        Vector3 local = tPlane.InverseTransformPoint(baseWorld);
        local.x += dir.x * dist / Mathf.Max(1e-6f, sx); // compensar escala para mover 'dist' en mundo
        local.z += dir.y * dist / Mathf.Max(1e-6f, sz);
        local.y  = yLocalPlano;
        return tPlane.TransformPoint(local);
    }

    bool DentroDelRect(Vector3 world)
    {
        Vector3 local = tPlane.InverseTransformPoint(world);
        return (local.x >= safeMinX && local.x <= safeMaxX &&
                local.z >= safeMinZ && local.z <= safeMaxZ);
    }

    static Vector3 LRPointWorld(LineRenderer lr, int i)
        => lr.useWorldSpace ? lr.GetPosition(i) : lr.transform.TransformPoint(lr.GetPosition(i));

    static float DistSegXZ(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector2 P = new Vector2(p.x, p.z), A = new Vector2(a.x, a.z), B = new Vector2(b.x, b.z);
        Vector2 AB = B - A;
        float den = Vector2.Dot(AB, AB);
        if (den < 1e-6f) return Vector2.Distance(P, A);
        float t = Mathf.Clamp01(Vector2.Dot(P - A, AB) / den);
        return Vector2.Distance(P, A + t * AB);
    }

    static float DistSegXZSqr(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector2 P = new Vector2(p.x, p.z), A = new Vector2(a.x, a.z), B = new Vector2(b.x, b.z);
        Vector2 AB = B - A;
        float den = Vector2.Dot(AB, AB);
        if (den < 1e-6f) return (P - A).sqrMagnitude;
        float t = Mathf.Clamp01(Vector2.Dot(P - A, AB) / den);
        Vector2 q = A + t * AB;
        return (P - q).sqrMagnitude;
    }

    static float DistXZ(Vector3 a, Vector3 b){ float dx=a.x-b.x, dz=a.z-b.z; return Mathf.Sqrt(dx*dx + dz*dz); }
    static float SqrDistXZ(Vector3 a, Vector3 b){ float dx=a.x-b.x, dz=a.z-b.z; return dx*dx + dz*dz; }

    Vector2 RandomUnit()
    {
        float ang = (float)(rng.NextDouble() * Mathf.PI * 2.0);
        return new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
    }

    // ===================== Instanciación =====================
    void Instanciar(GameObject prefab, Vector3 posMundo)
    {
        Vector3 pos = posMundo;

        if (ajustarAlturaConRaycast)
        {
            if (usarColliderSueloDirecto && sueloCollider != null)
            {
                Ray ray = new Ray(posMundo + Vector3.up * 200f, Vector3.down);
                var hits = Physics.RaycastAll(ray, 500f);
                for (int i = 0; i < hits.Length; i++)
                    if (hits[i].collider == sueloCollider) { pos.y = hits[i].point.y; break; }
            }
            else
            {
                int mask = raycastSoloContraPlane ? (1 << planeMesh.gameObject.layer) : (int)capaSuelo;
                if (Physics.Raycast(new Ray(posMundo + Vector3.up * 200f, Vector3.down), out var hit, 500f, mask))
                    pos.y = hit.point.y;
            }
        }

        var go = Instantiate(prefab, pos, Quaternion.identity, transform);
        if (rotarYRandom) go.transform.rotation = Quaternion.Euler(0f,UnityEngine.Random.Range(0f, 360f), 0f);
        // Escala del prefab NO se toca (asegurate que este GameObject padre está en 1,1,1).
    }

    // ===================== Gizmos =====================
    void OnDrawGizmosSelected()
    {
        if (!planeMesh || !planeMesh.sharedMesh) return;
        var t = planeMesh.transform;
        var b = planeMesh.sharedMesh.bounds;

        Vector3 a = t.TransformPoint(new Vector3(b.min.x, b.center.y, b.min.z));
        Vector3 b0= t.TransformPoint(new Vector3(b.max.x, b.center.y, b.min.z));
        Vector3 c = t.TransformPoint(new Vector3(b.max.x, b.center.y, b.max.z));
        Vector3 d = t.TransformPoint(new Vector3(b.min.x, b.center.y, b.max.z));

        Gizmos.color = new Color(0,1,0,0.35f);
        Gizmos.DrawLine(a,b0); Gizmos.DrawLine(b0,c); Gizmos.DrawLine(c,d); Gizmos.DrawLine(d,a);

        Gizmos.color = new Color(1,0,0,0.35f);
        for (int i = 0; i < segmentos.Count; i++)
            Gizmos.DrawLine(segmentos[i].a, segmentos[i].b);
    }
}



