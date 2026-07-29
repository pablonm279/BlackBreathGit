using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

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
    [SerializeField] MeshFilter[] sectoresTerreno = new MeshFilter[5];
    [SerializeField] bool autoBuscarSectoresTerreno = true;

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
    [SerializeField] bool preferirPrimerPuntoCentrico = true;

    [Header("Altura / terreno")]
    [SerializeField] bool ajustarAlturaConRaycast = true;
    [SerializeField] bool raycastSoloContraPlane = true;
    [SerializeField] LayerMask capaSuelo = ~0;
    [SerializeField] bool excluirDecoracionBajoAlturaSuperficie = false;
    [SerializeField] float alturaMinimaSuperficieDecoracion = -0.7f;

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

    [Header("Rendimiento (sombras de decoracion)")]
    [SerializeField] bool convertirSombrasBlobCanvas = true;

    [Header("Relieve procedural")]
    [SerializeField] bool usarRelieveProcedural = true;
    [SerializeField] MeshFilter planeMeshExtension;
    [SerializeField] MeshCollider planeColliderExtension;
    [SerializeField] float alturaRelieveBosque = 0.34f;
    [SerializeField] float alturaRelievePasoHelado = 0.46f;
    [SerializeField] float alturaRelieveNedukazal = 0.24f;
    [SerializeField] float frecuenciaRelievePrincipal = 0.065f;
    [SerializeField] float frecuenciaRelieveSecundaria = 0.16f;
    [SerializeField] float frecuenciaWarpRelieve = 0.035f;
    [SerializeField] float intensidadWarpRelieve = 1.1f;
    [SerializeField] float distanciaMuestreoNormal = 0.32f;

    [Header("Precipicio - Paso Viento Helado")]
    [SerializeField] bool usarPrecipicioTerrenoSurPasoHelado = true;
    [SerializeField] float caidaPrecipicioTerrenoSur = 9.5f;
    [SerializeField] float margenLateralPrecipicioTerrenoSur = 2f;
    [SerializeField] float offsetBordePrecipicioTerrenoSur = 0.18f;
    [SerializeField] float anchoSuavizadoSubidaPrecipicioTerrenoSur = 7f;
    [SerializeField, Range(0f, 1f)] float factorSubidaBordePrecipicioTerrenoSur = 0.18f;
    [SerializeField] bool usarPrecipicioTerrenoNortePasoHelado = true;
    [SerializeField] float caidaPrecipicioTerrenoNorte = 9.5f;
    [SerializeField] float margenLateralPrecipicioTerrenoNorte = 2f;
    [SerializeField] float offsetBordePrecipicioTerrenoNorte = 0.18f;
    [SerializeField] float irregularidadBordePrecipicioTerrenoNorte = 4f;
    [SerializeField] float frecuenciaBordePrecipicioTerrenoNorte = 0.055f;
    [SerializeField] float irregularidadLateralPrecipicioTerrenoNorte = 4f;
    [SerializeField] float frecuenciaLateralPrecipicioTerrenoNorte = 0.045f;
    [SerializeField] float anchoTransicionLateralPrecipicioTerrenoNorte = 1.25f;
    [SerializeField] float ruidoAlturaPrecipicioTerrenoNorte = 0.55f;
    [SerializeField] float frecuenciaRuidoAlturaPrecipicioTerrenoNorte = 0.085f;
    [SerializeField] float anchoSuavizadoSubidaPrecipicioTerrenoNorte = 7f;
    [SerializeField, Range(0f, 1f)] float factorSubidaBordePrecipicioTerrenoNorte = 0.18f;
    [SerializeField] bool permitirDecoracionSobrePrecipicioTerrenoNorte = true;

    [Header("Grieta - Paso Viento Helado")]
    [SerializeField] bool usarGrietaMarcadorPasoHelado = true;
    [SerializeField] string nombreMarcadorGrietaPasoHelado = "MarcadorGrieta";
    [SerializeField] float profundidadGrietaPasoHelado = 8.5f;
    [SerializeField] float anchoMinimoGrietaPasoHelado = 3.5f;
    [SerializeField] float irregularidadBordeGrietaPasoHelado = 1.8f;
    [SerializeField] float frecuenciaBordeGrietaPasoHelado = 0.14f;
    [SerializeField] float ruidoFondoGrietaPasoHelado = 0.45f;
    [SerializeField] float frecuenciaRuidoFondoGrietaPasoHelado = 0.18f;
    [SerializeField] float anchoTransicionGrietaPasoHelado = 0.65f;
    [SerializeField] float margenDecoracionGrietaPasoHelado = 2f;

    [Header("Materiales Aliento Negro")]
    [SerializeField] Material materialDefaultAlientoNegro;
    [SerializeField] Material materialDefaultAlientoNegro1;
    [SerializeField] Material materialPasoVientoHeladoAlientoNegro;
    [SerializeField] Material materialPasoVientoHeladoAlientoNegro1;
    [SerializeField] Material materialDefaultClimaNiebla;
    [SerializeField] Material materialPasoVientoHeladoClimaNiebla;

    // ---- internos ----
    const string PrefGraficosIndex = "graficos_index";
    const float AumentoRadioDecoracionesPorNivelCalidad = 0.10f;
    const float UmbralDistCaminoRellenoRemovible = 0.15f;
    const float TamanoCeldaDecoracionRemovible = 2f;
    struct Segmento { public Vector3 a, b; public float halfWidth; }
    readonly List<Segmento> segmentos = new List<Segmento>();
    readonly List<Transform> nodos = new List<Transform>();
    readonly List<GameObject> decoracionesRemoviblesSobreCaminos = new List<GameObject>();
    readonly Dictionary<CellKey, List<GameObject>> decoracionesRemoviblesPorCelda = new Dictionary<CellKey, List<GameObject>>();
    readonly Dictionary<LineRenderer, int> versionLimpiezaDecoracionPorCamino = new Dictionary<LineRenderer, int>();
    readonly List<Canvas> canvasesSombraCandidatos = new List<Canvas>(2);
    readonly List<Graphic> graficosSombraCandidatos = new List<Graphic>(2);
    readonly List<MeshRenderer> renderersDecoracionConSombraBlob = new List<MeshRenderer>(2);

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
    MeshCollider planeCollider;
    Mesh runtimePlaneMesh;
    Mesh runtimePlaneMeshExtension;
    Vector3[] basePlaneVertices;
    Vector3[] basePlaneExtensionVertices;
    MeshFilter meshAreaActual;
    Collider colliderSueloAreaActual;
    MeshFilter sectorExcluidoActual;
    int relieveSeed;
    int zonaRelieveActual;
    float alturaRelieveActual;
    bool precipicioTerrenoSurDisponible;
    float precipicioSurMinX;
    float precipicioSurMaxX;
    float precipicioSurMinZ;
    float precipicioSurMaxZ;
    float precipicioSurBordeInteriorZ;
    float precipicioSurDireccionExteriorZ = -1f;
    bool precipicioTerrenoNorteDisponible;
    float precipicioNorteMinX;
    float precipicioNorteMaxX;
    float precipicioNorteMinZ;
    float precipicioNorteMaxZ;
    float precipicioNorteBordeInteriorZ;
    float precipicioNorteDireccionExteriorZ = 1f;
    bool grietaPasoHeladoDisponible;
    Vector3 grietaPasoHeladoCentro;
    Vector3 grietaPasoHeladoEjeLargo = Vector3.forward;
    Vector3 grietaPasoHeladoEjeAncho = Vector3.right;
    float grietaPasoHeladoMitadLargo;
    float grietaPasoHeladoMitadAncho;
    int decorBatchCounter;
    int versionDecoracionesRemovibles;
    bool batchActualEsRellenoRemovible;

    void Awake()
    {
        if (!planeMesh || !planeMesh.sharedMesh)
        {
            Debug.LogError("[MapDecorator] Asigná un MeshFilter vélido.");
            enabled = false; return;
        }

        EnsureReliefTargets();
        EnsureRuntimeTerrainMeshes();

        ConfigurarAreaTrabajo(planeMesh);
        EnsureSectoresTerreno();

        if (transform.lossyScale != Vector3.one)
            Debug.LogWarning("[MapDecorator] Este GameObject deberáa estar en escala (1,1,1) para no heredar escalas raras.");
    }

    bool ConfigurarAreaTrabajo(MeshFilter areaMesh)
    {
        if (!areaMesh || !areaMesh.sharedMesh)
        {
            return false;
        }

        meshAreaActual = areaMesh;
        colliderSueloAreaActual = areaMesh.GetComponent<Collider>();

        tPlane      = areaMesh.transform;
        localBounds = areaMesh.sharedMesh.bounds;
        minL        = localBounds.min;
        maxL        = localBounds.max;
        sizeL       = localBounds.size;
        yLocalPlano = localBounds.center.y;
        safeMinX    = minL.x; safeMaxX = maxL.x;
        safeMinZ    = minL.z; safeMaxZ = maxL.z;

        sx = Mathf.Abs(tPlane.lossyScale.x);
        sz = Mathf.Abs(tPlane.lossyScale.z);

        sizeWorldX = sizeL.x * sx;
        sizeWorldZ = sizeL.z * sz;

        var b = areaMesh.sharedMesh.bounds;
        Vector3 aW = tPlane.TransformPoint(new Vector3(b.min.x, b.center.y, b.min.z));
        Vector3 bW = tPlane.TransformPoint(new Vector3(b.max.x, b.center.y, b.min.z));
        Vector3 cW = tPlane.TransformPoint(new Vector3(b.max.x, b.center.y, b.max.z));
        Vector3 dW = tPlane.TransformPoint(new Vector3(b.min.x, b.center.y, b.max.z));
        rectMinX = Mathf.Min(Mathf.Min(aW.x, bW.x), Mathf.Min(cW.x, dW.x));
        rectMaxX = Mathf.Max(Mathf.Max(aW.x, bW.x), Mathf.Max(cW.x, dW.x));
        rectMinZ = Mathf.Min(Mathf.Min(aW.z, bW.z), Mathf.Min(cW.z, dW.z));
        rectMaxZ = Mathf.Max(Mathf.Max(aW.z, bW.z), Mathf.Max(cW.z, dW.z));

        ActualizarZonaSegura();
        return true;
    }

    void EnsureSectoresTerreno()
    {
        if (sectoresTerreno == null || sectoresTerreno.Length < 5)
        {
            MeshFilter[] anteriores = sectoresTerreno;
            sectoresTerreno = new MeshFilter[5];
            if (anteriores != null)
            {
                for (int i = 0; i < anteriores.Length && i < sectoresTerreno.Length; i++)
                {
                    sectoresTerreno[i] = anteriores[i];
                }
            }
        }

        if (!autoBuscarSectoresTerreno || planeMesh == null)
        {
            return;
        }

        Transform raiz = planeMesh.transform.parent != null ? planeMesh.transform.parent : transform;
        MeshFilter[] filtros = raiz.GetComponentsInChildren<MeshFilter>(true);
        List<MeshFilter> candidatos = new List<MeshFilter>();

        for (int i = 0; i < filtros.Length; i++)
        {
            MeshFilter filtro = filtros[i];
            if (filtro == null || filtro == planeMesh || filtro == planeMeshExtension || filtro.sharedMesh == null)
            {
                continue;
            }

            if (filtro.GetComponent<MeshCollider>() == null)
            {
                continue;
            }

            candidatos.Add(filtro);
        }

        candidatos.Sort((a, b) =>
        {
            int indiceA = ExtraerIndiceSector(a.name);
            int indiceB = ExtraerIndiceSector(b.name);
            if (indiceA != indiceB)
            {
                if (indiceA <= 0) return 1;
                if (indiceB <= 0) return -1;
                return indiceA.CompareTo(indiceB);
            }
            return string.CompareOrdinal(a.name, b.name);
        });

        for (int i = 0; i < candidatos.Count; i++)
        {
            int indiceSector = ExtraerIndiceSector(candidatos[i].name);
            if (indiceSector >= 1 && indiceSector <= sectoresTerreno.Length)
            {
                sectoresTerreno[indiceSector - 1] = candidatos[i];
            }
        }

        int proximoLibre = 0;
        for (int i = 0; i < candidatos.Count; i++)
        {
            if (ContieneSector(candidatos[i]))
            {
                continue;
            }

            while (proximoLibre < sectoresTerreno.Length && sectoresTerreno[proximoLibre] != null)
            {
                proximoLibre++;
            }

            if (proximoLibre >= sectoresTerreno.Length)
            {
                break;
            }

            sectoresTerreno[proximoLibre] = candidatos[i];
        }
    }

    bool ContieneSector(MeshFilter filtro)
    {
        for (int i = 0; i < sectoresTerreno.Length; i++)
        {
            if (sectoresTerreno[i] == filtro)
            {
                return true;
            }
        }
        return false;
    }

    int ExtraerIndiceSector(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
        {
            return 0;
        }

        for (int i = 0; i < nombre.Length; i++)
        {
            if (nombre[i] != '(')
            {
                continue;
            }

            int cierre = nombre.IndexOf(')', i + 1);
            if (cierre <= i + 1)
            {
                continue;
            }

            string textoIndice = nombre.Substring(i + 1, cierre - i - 1);
            if (int.TryParse(textoIndice, out int indice))
            {
                return indice;
            }
        }

        char ultimo = nombre[nombre.Length - 1];
        if (ultimo >= '1' && ultimo <= '9')
        {
            return ultimo - '0';
        }

        return 0;
    }

    MeshFilter ResolverAreaPorSector(int sector)
    {
        if (sector <= 0)
        {
            return planeMesh;
        }

        EnsureSectoresTerreno();
        int indice = sector - 1;
        if (sectoresTerreno != null
            && indice >= 0
            && indice < sectoresTerreno.Length
            && sectoresTerreno[indice] != null)
        {
            return sectoresTerreno[indice];
        }

        Debug.LogWarning($"[MapDecorator] Sector {sector} no asignado. Se usa el terreno principal.");
        return planeMesh;
    }

    public void RegenerarRelieveParaZona(int zonaId, int fase)
    {
        RegenerarRelieveParaZona(zonaId, fase, null);
    }

    public void RegenerarRelieveParaZona(int zonaId, int fase, int? reliefSeedOverride)
    {
        if (!usarRelieveProcedural || planeMesh == null)
            return;

        EnsureReliefTargets();
        EnsureRuntimeTerrainMeshes();

        if (reliefSeedOverride.HasValue)
        {
            relieveSeed = reliefSeedOverride.Value;
        }
        else
        {
            int variante = zonaId == 2 ? fase * 19349663 : System.Guid.NewGuid().GetHashCode();
            relieveSeed = (zonaId * 73856093) ^ variante ^ 0x2f6e2b1;
        }

        ReiniciarSesionDecoracion();
        zonaRelieveActual = zonaId;
        AplicarMaterialesAlientoNegro(zonaId);
        AplicarMaterialesClimaNiebla(zonaId);
        alturaRelieveActual = ObtenerAlturaRelieveParaZona(zonaId);
        PrepararPrecipicioTerrenoSur();
        PrepararPrecipicioTerrenoNorte();
        PrepararGrietaPasoHelado();

        AplicarRelieveAPlano(planeMesh, runtimePlaneMesh, basePlaneVertices, planeCollider);
        AplicarRelieveAPlano(planeMeshExtension, runtimePlaneMeshExtension, basePlaneExtensionVertices, planeColliderExtension);
        LimpiarParedVisualPrecipicioTerrenoSur();
    }

    public int GetReliefSeed()
    {
        return relieveSeed;
    }

    public bool TrySampleSurface(Vector3 worldPos, out Vector3 surfacePos, out Vector3 surfaceNormal, float normalOffset = 0f)
    {
        surfacePos = worldPos;
        surfaceNormal = Vector3.up;

        if (!usarRelieveProcedural || planeMesh == null)
            return false;

        EnsureReliefTargets();

        Ray ray = new Ray(worldPos + Vector3.up * 200f, Vector3.down);
        int mask = 1 << planeMesh.gameObject.layer;
        RaycastHit[] hits = Physics.RaycastAll(ray, 500f, mask, QueryTriggerInteraction.Ignore);

        bool found = false;
        float nearestDistance = float.PositiveInfinity;
        RaycastHit nearestHit = default;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null)
                continue;

            if (hitCollider != planeCollider && hitCollider != planeColliderExtension)
                continue;

            if (hits[i].distance < nearestDistance)
            {
                nearestDistance = hits[i].distance;
                nearestHit = hits[i];
                found = true;
            }
        }

        if (found)
        {
            surfaceNormal = nearestHit.normal.sqrMagnitude > 0.0001f ? nearestHit.normal.normalized : Vector3.up;
            surfacePos = nearestHit.point + surfaceNormal * normalOffset;
            return true;
        }

        float surfaceY = planeMesh.transform.position.y + EvaluateReliefHeightOffsetWorld(worldPos);
        surfaceNormal = CalculateReliefNormalWorld(worldPos);
        surfacePos = new Vector3(worldPos.x, surfaceY, worldPos.z) + surfaceNormal * normalOffset;
        return true;
    }

    public void AlinearTransformASuelo(Transform target, float normalOffset = 0f)
    {
        if (target == null)
            return;

        if (TrySampleSurface(target.position, out var surfacePos, out _, normalOffset))
            target.position = surfacePos;
    }

    public void ConfigurarExclusionDecoracionPorAltura(bool activa, float alturaMinimaSuperficie)
    {
        excluirDecoracionBajoAlturaSuperficie = activa;
        alturaMinimaSuperficieDecoracion = alturaMinimaSuperficie;
    }

    public void ConfigurarPrimerPuntoCentrico(bool usarCentro)
    {
        preferirPrimerPuntoCentrico = usarCentro;
    }

    // ===================== API =====================
    public void Limpiar()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject hijo = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(hijo);
            }
            else
            {
                DestroyImmediate(hijo);
            }
        }

        decoracionesRemoviblesSobreCaminos.Clear();
        decoracionesRemoviblesPorCelda.Clear();
        versionLimpiezaDecoracionPorCamino.Clear();
        versionDecoracionesRemovibles = 0;
        ReiniciarSesionDecoracion();
    }

    // Firma compatible con tu llamada previa
    public int Generar(GameObject prefab, int cantidad, float distCamino, float distNodo, float r, int k = 100, int sector = 0, int sectorno = 0)
        => GenerarSync(prefab, cantidad, distCamino, distNodo, r, k, sector, sectorno);
    public int Generar(GameObject prefab, int cantidad, int sector = 0, int sectorno = 0)
        => GenerarSync(prefab, cantidad, this.distCamino, this.distNodo, this.radioPoisson, this.intentosPorPunto, sector, sectorno);

    public int GenerarSync(GameObject prefab, int cantidad,
                           float distCaminoOverride, float distNodoOverride,
                           float rOverride, int kOverride, int sector = 0, int sectorno = 0)
    {
        Preparar(prefab, distCaminoOverride, distNodoOverride, rOverride, kOverride, sector, sectorno);
        int col = ColocarPoisson(prefab, cantidad);
//        Debug.Log($"[MapDecorator] (Sync) Colocados {col}/{cantidad} '{prefab?.name}'.");
        return col;
    }

    public void GenerarAsync(MonoBehaviour runner, GameObject prefab, int cantidad,
                             float distCaminoOverride, float distNodoOverride,
                             float rOverride, int kOverride, int sector = 0, int sectorno = 0)
    {
        Preparar(prefab, distCaminoOverride, distNodoOverride, rOverride, kOverride, sector, sectorno);
        runner.StartCoroutine(ColocarPoissonCR(prefab, cantidad));
    }

    // Variante que permite al llamador hacer yield hasta terminar
    public IEnumerator GenerarAsyncCR(GameObject prefab, int cantidad,
                                      float distCaminoOverride, float distNodoOverride,
                                      float rOverride, int kOverride, int sector = 0, int sectorno = 0)
    {
        Preparar(prefab, distCaminoOverride, distNodoOverride, rOverride, kOverride, sector, sectorno);
        yield return ColocarPoissonCR(prefab, cantidad);
    }

    public IEnumerator GenerarAsyncCR(GameObject prefab, int cantidad, int sector = 0, int sectorno = 0)
    {
        Preparar(prefab, this.distCamino, this.distNodo, this.radioPoisson, this.intentosPorPunto, sector, sectorno);
        yield return ColocarPoissonCR(prefab, cantidad);
    }

    // ===================== Preparación =====================
    float EscalarRadioDecoracionesPorCalidad(float radio)
    {
        if (radio <= 0f)
        {
            return radio;
        }

        int calidadMaxima = Mathf.Max(0, QualitySettings.names.Length - 1);
        if (calidadMaxima <= 0)
        {
            return radio;
        }

        int calidadActual = PlayerPrefs.GetInt(PrefGraficosIndex, QualitySettings.GetQualityLevel());
        calidadActual = Mathf.Clamp(calidadActual, 0, calidadMaxima);

        int nivelesDebajoDeUltra = calidadMaxima - calidadActual;
        if (nivelesDebajoDeUltra <= 0)
        {
            return radio;
        }

        float multiplicador = 1f + (AumentoRadioDecoracionesPorNivelCalidad * nivelesDebajoDeUltra);
        return radio * multiplicador;
    }

    void Preparar(GameObject prefab, float distCaminoOverride, float distNodoOverride, float rOverride, int kOverride, int sector = 0, int sectorno = 0)
    {
        if (prefab == null) { Debug.LogError("[MapDecorator] Prefab nulo."); return; }

        ConfigurarAreaTrabajo(ResolverAreaPorSector(sector));
        sectorExcluidoActual = sectorno > 0 ? ResolverAreaPorSector(sectorno) : null;

        int batchSeedBase = relieveSeed != 0
            ? relieveSeed
            : (usarSemillaAleatoria ? System.Guid.NewGuid().GetHashCode() : semilla);
        int prefabSeed = prefab.name != null ? prefab.name.GetHashCode() : 0;
        int batchIndex = decorBatchCounter++;
        int batchSeed = unchecked((batchSeedBase * 397) ^ prefabSeed ^ (batchIndex * 486187739) ^ semilla);
        rng = new System.Random(batchSeed);

        this.distCamino = distCaminoOverride;
        this.distNodo   = distNodoOverride;
        this.radioPoisson = EscalarRadioDecoracionesPorCalidad(rOverride);
        this.intentosPorPunto = kOverride;
        batchActualEsRellenoRemovible = distCaminoOverride < UmbralDistCaminoRellenoRemovible;

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

        OcultarDecoracionRemovibleSobreCaminosActivos();
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

        OcultarDecoracionRemovibleSobreCaminosActivos();
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
        if (preferirPrimerPuntoCentrico)
        {
            Vector3 centro = CentroTerrenoMundo();
            float radioCentro = Mathf.Max(0.6f, radioPoisson * 0.35f);

            for (int i = 0; i < 6; i++)
            {
                float distancia = radioCentro * (0.25f + (float)rng.NextDouble() * 0.75f);
                Vector3 candidatoCentrico = MoverEnPlano(centro, RandomUnit(), distancia);
                if (!DentroDelRect(candidatoCentrico))
                    continue;

                if (PasaExclusiones(candidatoCentrico) && PasaPoisson(candidatoCentrico))
                {
                    p0 = candidatoCentrico;
                    return true;
                }
            }

            if (PasaExclusiones(centro) && PasaPoisson(centro))
            {
                p0 = centro;
                return true;
            }
        }

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
        bool incluirCaminosInactivos = !batchActualEsRellenoRemovible;

        if (soloLineRenderersConTag)
        {
            var todos = FindObjectsOfType<LineRenderer>(incluirCaminosInactivos);
            for (int i = 0; i < todos.Length; i++)
            {
                LineRenderer lr = todos[i];
                if (!lr || !TieneTagCamino(lr.gameObject) || (!incluirCaminosInactivos && !lr.gameObject.activeInHierarchy))
                {
                    continue;
                }

                AgregarSiIntersecta(lr, fuentes);
            }
        }
        else
        {
            var todos = FindObjectsOfType<LineRenderer>(incluirCaminosInactivos);
            for (int i = 0; i < todos.Length; i++)
            {
                LineRenderer lr = todos[i];
                if (!lr || (!incluirCaminosInactivos && !lr.gameObject.activeInHierarchy))
                {
                    continue;
                }

                AgregarSiIntersecta(lr, fuentes);
            }
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

    public void OcultarDecoracionRemovibleSobreCamino(LineRenderer lr)
    {
        if (lr == null || lr.positionCount < 2 || decoracionesRemoviblesSobreCaminos.Count == 0)
        {
            return;
        }

        if (versionLimpiezaDecoracionPorCamino.TryGetValue(lr, out int versionLimpieza)
            && versionLimpieza == versionDecoracionesRemovibles)
        {
            return;
        }

        float radio = CalcularRadioLimpiezaCamino(lr);
        float radioSqr = radio * radio;

        for (int i = 0; i < lr.positionCount - 1; i++)
        {
            Vector3 a = LRPointWorld(lr, i);
            Vector3 b = LRPointWorld(lr, i + 1);
            OcultarDecoracionRemovibleCercanaASegmento(a, b, radio, radioSqr);
        }

        versionLimpiezaDecoracionPorCamino[lr] = versionDecoracionesRemovibles;
    }

    void OcultarDecoracionRemovibleSobreCaminosActivos()
    {
        if (!batchActualEsRellenoRemovible || decoracionesRemoviblesSobreCaminos.Count == 0)
        {
            return;
        }

        var todos = FindObjectsOfType<LineRenderer>();
        for (int i = 0; i < todos.Length; i++)
        {
            LineRenderer lr = todos[i];
            if (!lr || !lr.gameObject.activeInHierarchy || (soloLineRenderersConTag && !TieneTagCamino(lr.gameObject)))
            {
                continue;
            }

            OcultarDecoracionRemovibleSobreCamino(lr);
        }
    }

    void OcultarDecoracionRemovibleCercanaASegmento(Vector3 a, Vector3 b, float radio, float radioSqr)
    {
        CellKey cmin = CellDecoracionRemovible(Mathf.Min(a.x, b.x) - radio, Mathf.Min(a.z, b.z) - radio);
        CellKey cmax = CellDecoracionRemovible(Mathf.Max(a.x, b.x) + radio, Mathf.Max(a.z, b.z) + radio);

        for (int x = cmin.x; x <= cmax.x; x++)
        for (int z = cmin.z; z <= cmax.z; z++)
        {
            if (!decoracionesRemoviblesPorCelda.TryGetValue(new CellKey(x, z), out var candidatas))
            {
                continue;
            }

            for (int i = 0; i < candidatas.Count; i++)
            {
                GameObject go = candidatas[i];
                if (!go || !go.activeSelf)
                {
                    continue;
                }

                if (DistSegXZSqr(go.transform.position, a, b) <= radioSqr)
                {
                    go.SetActive(false);
                }
            }
        }
    }

    float CalcularRadioLimpiezaCamino(LineRenderer lr)
    {
        float worldWidthLR = MaxWidthWorld(lr);
        float worldWidth = Mathf.Max(worldWidthLR, anchoCaminoMinWorld);
        return 0.5f * worldWidth + margenCamino;
    }

    static CellKey CellDecoracionRemovible(float worldX, float worldZ)
    {
        return new CellKey(
            Mathf.FloorToInt(worldX / TamanoCeldaDecoracionRemovible),
            Mathf.FloorToInt(worldZ / TamanoCeldaDecoracionRemovible));
    }

    bool TieneTagCamino(GameObject go)
    {
        if (go == null)
        {
            return false;
        }

        if (TieneTagSeguro(go, tagCaminos))
        {
            return true;
        }

        if (tagsCaminosAlternativos == null)
        {
            return false;
        }

        for (int i = 0; i < tagsCaminosAlternativos.Length; i++)
        {
            if (TieneTagSeguro(go, tagsCaminosAlternativos[i]))
            {
                return true;
            }
        }

        return false;
    }

    static bool TieneTagSeguro(GameObject go, string tag)
    {
        if (go == null || string.IsNullOrEmpty(tag))
        {
            return false;
        }

        try
        {
            return go.CompareTag(tag);
        }
        catch
        {
            return false;
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

        CaminoMesh caminoMesh = lr.GetComponent<CaminoMesh>();
        if (caminoMesh != null)
        {
            Vector3 escala = lr.transform.lossyScale;
            float escalaXZ = Mathf.Max(Mathf.Abs(escala.x), Mathf.Abs(escala.z));
            w = Mathf.Max(w, caminoMesh.GetWidth() * escalaXZ);
        }

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
        if (EstaDentroSectorExcluido(p))
        {
            return false;
        }

        if (EstaSobreGrietaPasoHelado(p, margenDecoracionGrietaPasoHelado))
        {
            return false;
        }

        if (!PasaAlturaMinimaDecoracion(p))
        {
            return false;
        }

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

    bool PasaAlturaMinimaDecoracion(Vector3 p)
    {
        if (!excluirDecoracionBajoAlturaSuperficie)
        {
            return true;
        }

        if (permitirDecoracionSobrePrecipicioTerrenoNorte && EstaSobrePrecipicioTerrenoNorte(p))
        {
            return true;
        }

        float alturaSuperficie = ObtenerAlturaSuperficieEstimada(p);
        return alturaSuperficie > alturaMinimaSuperficieDecoracion;
    }

    bool EstaSobrePrecipicioTerrenoNorte(Vector3 worldPos)
    {
        if (!precipicioTerrenoNorteDisponible || zonaRelieveActual != 2)
        {
            return false;
        }

        float seedA = relieveSeed * 0.00071f;
        float seedB = relieveSeed * 0.00037f;
        float mascaraLateral = CalcularMascaraLateralPrecipicioTerrenoNorte(worldPos, seedA, seedB);
        if (mascaraLateral <= 0.5f)
        {
            return false;
        }

        float borde = CalcularBordePrecipicioTerrenoNorte(worldPos.x, seedA, seedB);
        float distanciaExterior = (worldPos.z - borde) * precipicioNorteDireccionExteriorZ;
        return distanciaExterior >= 0f;
    }

    float ObtenerAlturaSuperficieEstimada(Vector3 worldPos)
    {
        if (!usarRelieveProcedural)
        {
            return worldPos.y;
        }

        MeshFilter area = meshAreaActual != null ? meshAreaActual : planeMesh;
        if (area == null)
        {
            return worldPos.y + EvaluateReliefHeightOffsetWorld(worldPos);
        }

        Vector3 local = area.transform.InverseTransformPoint(worldPos);
        local.y = ObtenerYLocalBaseAreaActual();
        float baseY = area.transform.TransformPoint(local).y;
        return baseY + EvaluateReliefHeightOffsetWorld(worldPos);
    }

    float ObtenerYLocalBaseAreaActual()
    {
        if (meshAreaActual == planeMesh && basePlaneVertices != null && basePlaneVertices.Length > 0)
        {
            return basePlaneVertices[0].y;
        }

        if (meshAreaActual == planeMeshExtension && basePlaneExtensionVertices != null && basePlaneExtensionVertices.Length > 0)
        {
            return basePlaneExtensionVertices[0].y;
        }

        return yLocalPlano;
    }

    bool EstaDentroSectorExcluido(Vector3 p)
    {
        if (sectorExcluidoActual == null || sectorExcluidoActual.sharedMesh == null)
        {
            return false;
        }

        Bounds bounds = sectorExcluidoActual.sharedMesh.bounds;
        Vector3 local = sectorExcluidoActual.transform.InverseTransformPoint(p);
        return local.x >= bounds.min.x
            && local.x <= bounds.max.x
            && local.z >= bounds.min.z
            && local.z <= bounds.max.z;
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
    void ReiniciarSesionDecoracion()
    {
        decorBatchCounter = 0;
    }

    Vector3 RandomPointInsideRect()
    {
        float rx = Mathf.Lerp(safeMinX, safeMaxX, (float)rng.NextDouble());
        float rz = Mathf.Lerp(safeMinZ, safeMaxZ, (float)rng.NextDouble());
        return tPlane.TransformPoint(new Vector3(rx, yLocalPlano, rz));
    }

    Vector3 CentroTerrenoMundo()
    {
        float cx = 0.5f * (safeMinX + safeMaxX);
        float cz = 0.5f * (safeMinZ + safeMaxZ);
        return tPlane.TransformPoint(new Vector3(cx, yLocalPlano, cz));
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
    void EnsureReliefTargets()
    {
        if (planeMesh != null && planeCollider == null)
            planeCollider = planeMesh.GetComponent<MeshCollider>();

        if (planeMeshExtension == null)
        {
            var atributosZona = GetComponent<AtributosZona>();
            if (atributosZona != null && atributosZona.TexturaTerrenoExtension != null)
                planeMeshExtension = atributosZona.TexturaTerrenoExtension.GetComponent<MeshFilter>();
        }

        if (planeMeshExtension != null && planeColliderExtension == null)
            planeColliderExtension = planeMeshExtension.GetComponent<MeshCollider>();
    }

    void EnsureRuntimeTerrainMeshes()
    {
        if (!usarRelieveProcedural)
            return;

        EnsureRuntimeTerrainMesh(planeMesh, ref runtimePlaneMesh, ref basePlaneVertices, planeCollider);
        EnsureRuntimeTerrainMesh(planeMeshExtension, ref runtimePlaneMeshExtension, ref basePlaneExtensionVertices, planeColliderExtension);
    }

    void EnsureRuntimeTerrainMesh(MeshFilter filter, ref Mesh runtimeMesh, ref Vector3[] baseVertices, MeshCollider collider)
    {
        if (filter == null || filter.sharedMesh == null || runtimeMesh != null)
            return;

        runtimeMesh = Instantiate(filter.sharedMesh);
        runtimeMesh.name = filter.sharedMesh.name + "_RuntimeRelief";
        runtimeMesh.MarkDynamic();
        filter.sharedMesh = runtimeMesh;
        baseVertices = runtimeMesh.vertices;

        if (collider != null)
        {
            collider.sharedMesh = null;
            collider.sharedMesh = runtimeMesh;
        }
    }

    void AplicarRelieveAPlano(MeshFilter filter, Mesh runtimeMesh, Vector3[] baseVertices, MeshCollider collider)
    {
        if (filter == null || runtimeMesh == null || baseVertices == null || baseVertices.Length == 0)
            return;

        Vector3[] verts = new Vector3[baseVertices.Length];
        Transform tf = filter.transform;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 world = tf.TransformPoint(baseVertices[i]);
            world.y += EvaluateReliefHeightOffsetWorld(world);
            verts[i] = tf.InverseTransformPoint(world);
        }

        runtimeMesh.vertices = verts;
        runtimeMesh.RecalculateNormals();
        runtimeMesh.RecalculateBounds();

        if (collider != null)
        {
            collider.sharedMesh = null;
            collider.sharedMesh = runtimeMesh;
        }
    }

    float ObtenerAlturaRelieveParaZona(int zonaId)
    {
        switch (zonaId)
        {
            case 2:
                return alturaRelievePasoHelado;
            case 3:
                return alturaRelieveNedukazal;
            default:
                return alturaRelieveBosque;
        }
    }

    void AplicarMaterialesAlientoNegro(int zonaId)
    {
        bool esPasoVientoHelado = zonaId == 2;
        Material materialAlientoNegro = esPasoVientoHelado && materialPasoVientoHeladoAlientoNegro != null
            ? materialPasoVientoHeladoAlientoNegro
            : materialDefaultAlientoNegro;
        Material materialAlientoNegro1 = esPasoVientoHelado && materialPasoVientoHeladoAlientoNegro1 != null
            ? materialPasoVientoHeladoAlientoNegro1
            : materialDefaultAlientoNegro1;

        AsignarMaterialAlientoNegro("ALIENTONEGRO", materialAlientoNegro);
        AsignarMaterialAlientoNegro("ALIENTONEGRO (1)", materialAlientoNegro1);
    }

    void AplicarMaterialesClimaNiebla(int zonaId)
    {
        Material materialNiebla = zonaId == 2 && materialPasoVientoHeladoClimaNiebla != null
            ? materialPasoVientoHeladoClimaNiebla
            : materialDefaultClimaNiebla;

        AsignarMaterialATodosLosObjetos("Clima_Niebla", materialNiebla);
    }

    static void AsignarMaterialAlientoNegro(string nombreObjeto, Material material)
    {
        if (material == null)
            return;

        Transform objetoAliento = BuscarTransformEscena(nombreObjeto);
        if (objetoAliento == null)
            return;

        Renderer renderer = objetoAliento.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;
    }

    static void AsignarMaterialATodosLosObjetos(string nombreObjeto, Material material)
    {
        if (material == null || string.IsNullOrWhiteSpace(nombreObjeto))
            return;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidato = transforms[i];
            if (candidato == null || !candidato.gameObject.scene.IsValid() || candidato.name != nombreObjeto)
                continue;

            Renderer renderer = candidato.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }
    }

    void PrepararPrecipicioTerrenoSur()
    {
        precipicioTerrenoSurDisponible = false;

        if (!usarPrecipicioTerrenoSurPasoHelado || zonaRelieveActual != 2)
            return;

        MeshFilter sectorSur = ObtenerSectorTerreno(3);
        if (sectorSur == null || sectorSur.sharedMesh == null)
            return;

        if (!TryObtenerBoundsXZ(sectorSur, out precipicioSurMinX, out precipicioSurMaxX, out precipicioSurMinZ, out precipicioSurMaxZ))
            return;

        float centroSectorZ = (precipicioSurMinZ + precipicioSurMaxZ) * 0.5f;
        float centroTerrenoZ = ObtenerCentroTerrenoPrincipalZ();
        precipicioSurDireccionExteriorZ = centroSectorZ < centroTerrenoZ ? -1f : 1f;
        float bordeSector = precipicioSurDireccionExteriorZ < 0f ? precipicioSurMaxZ : precipicioSurMinZ;
        precipicioSurBordeInteriorZ = bordeSector + precipicioSurDireccionExteriorZ * offsetBordePrecipicioTerrenoSur;
        precipicioTerrenoSurDisponible = true;
    }

    void PrepararPrecipicioTerrenoNorte()
    {
        precipicioTerrenoNorteDisponible = false;

        if (!usarPrecipicioTerrenoNortePasoHelado || zonaRelieveActual != 2)
            return;

        MeshFilter sectorNorte = ObtenerSectorTerreno(1);
        if (sectorNorte == null || sectorNorte.sharedMesh == null)
            return;

        if (!TryObtenerBoundsXZ(sectorNorte, out precipicioNorteMinX, out precipicioNorteMaxX, out precipicioNorteMinZ, out precipicioNorteMaxZ))
            return;

        float centroSectorZ = (precipicioNorteMinZ + precipicioNorteMaxZ) * 0.5f;
        float centroTerrenoZ = ObtenerCentroTerrenoPrincipalZ();
        precipicioNorteDireccionExteriorZ = centroSectorZ < centroTerrenoZ ? -1f : 1f;
        float bordeSector = precipicioNorteDireccionExteriorZ < 0f ? precipicioNorteMaxZ : precipicioNorteMinZ;
        precipicioNorteBordeInteriorZ = bordeSector + precipicioNorteDireccionExteriorZ * offsetBordePrecipicioTerrenoNorte;
        precipicioTerrenoNorteDisponible = true;
    }

    void PrepararGrietaPasoHelado()
    {
        grietaPasoHeladoDisponible = false;

        if (!usarGrietaMarcadorPasoHelado || zonaRelieveActual != 2)
            return;

        Transform marcador = BuscarTransformEscena(nombreMarcadorGrietaPasoHelado);
        if (marcador == null)
            return;

        Vector3 escala = marcador.lossyScale;
        bool largoEnX = Mathf.Abs(escala.x) >= Mathf.Abs(escala.z);
        Vector3 ejeLargo = largoEnX ? marcador.right : marcador.forward;
        Vector3 ejeAncho = largoEnX ? marcador.forward : marcador.right;

        ejeLargo.y = 0f;
        ejeAncho.y = 0f;
        if (ejeLargo.sqrMagnitude < 0.0001f || ejeAncho.sqrMagnitude < 0.0001f)
            return;

        grietaPasoHeladoCentro = marcador.position;
        grietaPasoHeladoEjeLargo = ejeLargo.normalized;
        grietaPasoHeladoEjeAncho = ejeAncho.normalized;
        grietaPasoHeladoMitadLargo = Mathf.Max(1f, (largoEnX ? Mathf.Abs(escala.x) : Mathf.Abs(escala.z)) * 0.5f);
        grietaPasoHeladoMitadAncho = Mathf.Max(anchoMinimoGrietaPasoHelado * 0.5f, (largoEnX ? Mathf.Abs(escala.z) : Mathf.Abs(escala.x)) * 0.5f);
        grietaPasoHeladoDisponible = true;
    }

    static Transform BuscarTransformEscena(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return null;

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidato = transforms[i];
            if (candidato == null || !candidato.gameObject.scene.IsValid())
                continue;

            if (candidato.name == nombre)
                return candidato;
        }

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidato = transforms[i];
            if (candidato == null || !candidato.gameObject.scene.IsValid())
                continue;

            if (candidato.name.IndexOf(nombre, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return candidato;
        }

        return null;
    }

    MeshFilter ObtenerSectorTerreno(int sector)
    {
        if (sector <= 0)
            return null;

        EnsureSectoresTerreno();
        int indice = sector - 1;
        if (sectoresTerreno == null || indice < 0 || indice >= sectoresTerreno.Length)
            return null;

        return sectoresTerreno[indice];
    }

    float ObtenerCentroTerrenoPrincipalZ()
    {
        if (planeMesh == null || planeMesh.sharedMesh == null)
            return (precipicioSurMinZ + precipicioSurMaxZ) * 0.5f;

        return planeMesh.transform.TransformPoint(planeMesh.sharedMesh.bounds.center).z;
    }

    static bool TryObtenerBoundsXZ(MeshFilter filter, out float minX, out float maxX, out float minZ, out float maxZ)
    {
        minX = maxX = minZ = maxZ = 0f;
        if (filter == null || filter.sharedMesh == null)
            return false;

        Bounds bounds = filter.sharedMesh.bounds;
        Transform tf = filter.transform;
        Vector3[] corners =
        {
            new Vector3(bounds.min.x, bounds.center.y, bounds.min.z),
            new Vector3(bounds.min.x, bounds.center.y, bounds.max.z),
            new Vector3(bounds.max.x, bounds.center.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.center.y, bounds.max.z)
        };

        Vector3 first = tf.TransformPoint(corners[0]);
        minX = maxX = first.x;
        minZ = maxZ = first.z;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 world = tf.TransformPoint(corners[i]);
            minX = Mathf.Min(minX, world.x);
            maxX = Mathf.Max(maxX, world.x);
            minZ = Mathf.Min(minZ, world.z);
            maxZ = Mathf.Max(maxZ, world.z);
        }

        return maxX > minX && maxZ > minZ;
    }

    float EvaluateReliefHeightOffsetWorld(Vector3 worldPos)
    {
        if (!usarRelieveProcedural)
            return 0f;

        float seedA = relieveSeed * 0.00071f;
        float seedB = relieveSeed * 0.00037f;

        float warpX = SampleSignedPerlin(worldPos.x * frecuenciaWarpRelieve + seedA + 17.3f, worldPos.z * frecuenciaWarpRelieve - seedB + 91.7f) * intensidadWarpRelieve;
        float warpZ = SampleSignedPerlin(worldPos.x * frecuenciaWarpRelieve - seedB + 47.1f, worldPos.z * frecuenciaWarpRelieve + seedA + 13.9f) * intensidadWarpRelieve;

        float nx = worldPos.x + warpX;
        float nz = worldPos.z + warpZ;

        float principal = SampleSignedPerlin(nx * frecuenciaRelievePrincipal + seedA * 3.1f, nz * frecuenciaRelievePrincipal - seedB * 2.7f);
        float secundario = SampleSignedPerlin(nx * frecuenciaRelieveSecundaria - 31.7f + seedB * 1.3f, nz * frecuenciaRelieveSecundaria + 18.8f - seedA * 1.7f);
        float cresta = 1f - Mathf.Abs((Mathf.PerlinNoise(nx * (frecuenciaRelieveSecundaria * 0.72f) + 11.4f + seedA, nz * (frecuenciaRelieveSecundaria * 0.72f) - 23.6f + seedB) * 2f) - 1f);
        cresta = (cresta - 0.5f) * 0.18f;

        float combinado = principal * 0.74f + secundario * 0.26f + cresta;
        float relieve = combinado * alturaRelieveActual;
        relieve = SuavizarSubidaCercaPrecipicioTerrenoSur(worldPos, relieve);
        relieve = SuavizarSubidaCercaPrecipicioTerrenoNorte(worldPos, relieve, seedA, seedB);
        return relieve
            + EvaluatePrecipicioTerrenoSur(worldPos, seedA, seedB)
            + EvaluatePrecipicioTerrenoNorte(worldPos, seedA, seedB)
            + EvaluateGrietaPasoHelado(worldPos, seedA, seedB);
    }

    float SuavizarSubidaCercaPrecipicioTerrenoSur(Vector3 worldPos, float relieve)
    {
        if (!precipicioTerrenoSurDisponible || zonaRelieveActual != 2 || relieve <= 0f)
            return relieve;

        float margenLateral = Mathf.Max(0f, margenLateralPrecipicioTerrenoSur);
        if (worldPos.x < precipicioSurMinX - margenLateral || worldPos.x > precipicioSurMaxX + margenLateral)
            return relieve;

        float distanciaExterior = (worldPos.z - precipicioSurBordeInteriorZ) * precipicioSurDireccionExteriorZ;
        if (distanciaExterior >= 0f)
            return relieve;

        float ancho = Mathf.Max(0.1f, anchoSuavizadoSubidaPrecipicioTerrenoSur);
        float distanciaInterior = -distanciaExterior;
        if (distanciaInterior >= ancho)
            return relieve;

        float t = Mathf.Clamp01(distanciaInterior / ancho);
        t = t * t * (3f - 2f * t);
        float factorBorde = Mathf.Clamp01(factorSubidaBordePrecipicioTerrenoSur);
        return relieve * Mathf.Lerp(factorBorde, 1f, t);
    }

    float SuavizarSubidaCercaPrecipicioTerrenoNorte(Vector3 worldPos, float relieve, float seedA, float seedB)
    {
        if (!precipicioTerrenoNorteDisponible || zonaRelieveActual != 2 || relieve <= 0f)
            return relieve;

        float mascaraLateral = CalcularMascaraLateralPrecipicioTerrenoNorte(worldPos, seedA, seedB);
        if (mascaraLateral <= 0.001f)
            return relieve;

        float borde = CalcularBordePrecipicioTerrenoNorte(worldPos.x, seedA, seedB);
        float distanciaExterior = (worldPos.z - borde) * precipicioNorteDireccionExteriorZ;
        if (distanciaExterior >= 0f)
            return relieve;

        float ancho = Mathf.Max(0.1f, anchoSuavizadoSubidaPrecipicioTerrenoNorte);
        float distanciaInterior = -distanciaExterior;
        if (distanciaInterior >= ancho)
            return relieve;

        float t = Mathf.Clamp01(distanciaInterior / ancho);
        t = t * t * (3f - 2f * t);
        float factorBorde = Mathf.Clamp01(factorSubidaBordePrecipicioTerrenoNorte);
        return relieve * Mathf.Lerp(factorBorde, 1f, t);
    }

    float EvaluatePrecipicioTerrenoSur(Vector3 worldPos, float seedA, float seedB)
    {
        if (!precipicioTerrenoSurDisponible || zonaRelieveActual != 2)
            return 0f;

        float margenLateral = Mathf.Max(0f, margenLateralPrecipicioTerrenoSur);
        if (worldPos.x < precipicioSurMinX - margenLateral || worldPos.x > precipicioSurMaxX + margenLateral)
            return 0f;

        float distanciaExterior = (worldPos.z - precipicioSurBordeInteriorZ) * precipicioSurDireccionExteriorZ;
        float mascaraCaida = distanciaExterior >= 0f ? 1f : 0f;
        float caida = Mathf.Max(8.5f, Mathf.Abs(caidaPrecipicioTerrenoSur));

        return -caida * mascaraCaida;
    }

    float CalcularBordePrecipicioTerrenoSur(float worldX, float seedA, float seedB)
    {
        return precipicioSurBordeInteriorZ;
    }

    float EvaluatePrecipicioTerrenoNorte(Vector3 worldPos, float seedA, float seedB)
    {
        if (!precipicioTerrenoNorteDisponible || zonaRelieveActual != 2)
            return 0f;

        float mascaraLateral = CalcularMascaraLateralPrecipicioTerrenoNorte(worldPos, seedA, seedB);
        if (mascaraLateral <= 0.001f)
            return 0f;

        float borde = CalcularBordePrecipicioTerrenoNorte(worldPos.x, seedA, seedB);
        float distanciaExterior = (worldPos.z - borde) * precipicioNorteDireccionExteriorZ;
        float mascaraCaida = distanciaExterior >= 0f ? 1f : 0f;
        float caida = Mathf.Max(8.5f, Mathf.Abs(caidaPrecipicioTerrenoNorte));
        float ruidoAltura = CalcularRuidoAlturaPrecipicioTerrenoNorte(worldPos, seedA, seedB);

        return (-caida + ruidoAltura) * mascaraCaida * mascaraLateral;
    }

    float CalcularBordePrecipicioTerrenoNorte(float worldX, float seedA, float seedB)
    {
        float frecuencia = Mathf.Max(0.001f, frecuenciaBordePrecipicioTerrenoNorte);
        float ruidoAmplio = SampleSignedPerlin(worldX * frecuencia + seedA * 2.1f + 37.4f, seedB * 1.7f - 19.8f);
        float ruidoDetalle = SampleSignedPerlin(worldX * frecuencia * 2.35f - seedB * 1.4f + 81.2f, seedA * 1.3f + 11.6f);
        float ruidoCorto = SampleSignedPerlin(worldX * frecuencia * 4.2f + seedA * 0.9f - 12.8f, seedB * 2.4f + 53.1f);
        float ruido = ruidoAmplio * 0.62f + ruidoDetalle * 0.28f + ruidoCorto * 0.10f;
        return precipicioNorteBordeInteriorZ + ruido * Mathf.Max(0f, irregularidadBordePrecipicioTerrenoNorte);
    }

    float CalcularMascaraLateralPrecipicioTerrenoNorte(Vector3 worldPos, float seedA, float seedB)
    {
        float margen = Mathf.Max(0f, margenLateralPrecipicioTerrenoNorte);
        float irregularidad = Mathf.Max(0f, irregularidadLateralPrecipicioTerrenoNorte);
        float transicion = Mathf.Max(0.1f, anchoTransicionLateralPrecipicioTerrenoNorte);
        float minX = precipicioNorteMinX - margen + CalcularRuidoLateralPrecipicioTerrenoNorte(worldPos.z, seedA, seedB, 0) * irregularidad;
        float maxX = precipicioNorteMaxX + margen + CalcularRuidoLateralPrecipicioTerrenoNorte(worldPos.z, seedA, seedB, 1) * irregularidad;

        if (maxX < minX + transicion)
        {
            float centro = (minX + maxX) * 0.5f;
            minX = centro - transicion * 0.5f;
            maxX = centro + transicion * 0.5f;
        }

        float izquierda = Mathf.Clamp01((worldPos.x - minX) / transicion);
        float derecha = Mathf.Clamp01((maxX - worldPos.x) / transicion);
        float mascara = Mathf.Min(izquierda, derecha);
        return mascara * mascara * (3f - 2f * mascara);
    }

    float CalcularRuidoLateralPrecipicioTerrenoNorte(float worldZ, float seedA, float seedB, int lado)
    {
        float frecuencia = Mathf.Max(0.001f, frecuenciaLateralPrecipicioTerrenoNorte);
        float offset = lado == 0 ? -31.7f : 46.2f;
        float amplio = SampleSignedPerlin(worldZ * frecuencia + seedA * 1.5f + offset, seedB * 1.2f - 9.4f + offset);
        float detalle = SampleSignedPerlin(worldZ * frecuencia * 2.6f - seedB * 0.9f + offset * 1.7f, seedA * 1.1f + 62.5f - offset);
        return amplio * 0.72f + detalle * 0.28f;
    }

    float CalcularRuidoAlturaPrecipicioTerrenoNorte(Vector3 worldPos, float seedA, float seedB)
    {
        float intensidad = Mathf.Max(0f, ruidoAlturaPrecipicioTerrenoNorte);
        if (intensidad <= 0f)
            return 0f;

        float frecuencia = Mathf.Max(0.001f, frecuenciaRuidoAlturaPrecipicioTerrenoNorte);
        float amplio = SampleSignedPerlin(worldPos.x * frecuencia + seedA * 1.8f + 25.3f, worldPos.z * frecuencia - seedB * 1.1f - 44.6f);
        float detalle = SampleSignedPerlin(worldPos.x * frecuencia * 2.7f - seedB * 0.8f + 73.9f, worldPos.z * frecuencia * 2.7f + seedA * 1.2f + 18.4f);
        return (amplio * 0.7f + detalle * 0.3f) * intensidad;
    }

    float EvaluateGrietaPasoHelado(Vector3 worldPos, float seedA, float seedB)
    {
        if (!grietaPasoHeladoDisponible || zonaRelieveActual != 2)
            return 0f;

        float mascara = CalcularMascaraGrietaPasoHelado(worldPos, seedA, seedB, 0f);
        if (mascara <= 0f)
            return 0f;

        float profundidad = Mathf.Max(1f, Mathf.Abs(profundidadGrietaPasoHelado));
        float ruidoFondo = CalcularRuidoFondoGrietaPasoHelado(worldPos, seedA, seedB);
        return (-profundidad + ruidoFondo) * mascara;
    }

    bool EstaSobreGrietaPasoHelado(Vector3 worldPos, float margenExtra)
    {
        if (!grietaPasoHeladoDisponible || zonaRelieveActual != 2)
            return false;

        float seedA = relieveSeed * 0.00071f;
        float seedB = relieveSeed * 0.00037f;
        return CalcularMascaraGrietaPasoHelado(worldPos, seedA, seedB, margenExtra) > 0.02f;
    }

    float CalcularMascaraGrietaPasoHelado(Vector3 worldPos, float seedA, float seedB, float margenExtra)
    {
        Vector3 delta = worldPos - grietaPasoHeladoCentro;
        float largo = Vector3.Dot(delta, grietaPasoHeladoEjeLargo);
        float ancho = Vector3.Dot(delta, grietaPasoHeladoEjeAncho);
        float frecuencia = Mathf.Max(0.001f, frecuenciaBordeGrietaPasoHelado);
        float irregularidad = Mathf.Max(0f, irregularidadBordeGrietaPasoHelado);
        float transicion = Mathf.Max(0.05f, anchoTransicionGrietaPasoHelado);

        float bordeIzq = -grietaPasoHeladoMitadAncho - margenExtra
            + SampleSignedPerlin(largo * frecuencia + seedA * 1.6f + 12.7f, seedB * 1.1f - 44.9f) * irregularidad;
        float bordeDer = grietaPasoHeladoMitadAncho + margenExtra
            + SampleSignedPerlin(largo * frecuencia - seedB * 1.3f - 73.2f, seedA * 1.4f + 19.6f) * irregularidad;
        float bordeInicio = -grietaPasoHeladoMitadLargo - margenExtra
            + SampleSignedPerlin(ancho * frecuencia * 1.4f + seedB * 0.9f - 11.3f, seedA * 1.2f + 63.5f) * irregularidad;
        float bordeFin = grietaPasoHeladoMitadLargo + margenExtra
            + SampleSignedPerlin(ancho * frecuencia * 1.4f - seedA * 1.1f + 39.8f, seedB * 1.5f - 27.4f) * irregularidad;

        if (bordeDer < bordeIzq + transicion)
        {
            float centro = (bordeIzq + bordeDer) * 0.5f;
            bordeIzq = centro - transicion * 0.5f;
            bordeDer = centro + transicion * 0.5f;
        }

        if (bordeFin < bordeInicio + transicion)
        {
            float centro = (bordeInicio + bordeFin) * 0.5f;
            bordeInicio = centro - transicion * 0.5f;
            bordeFin = centro + transicion * 0.5f;
        }

        float mascaraIzq = Mathf.Clamp01((ancho - (bordeIzq - transicion)) / transicion);
        float mascaraDer = Mathf.Clamp01(((bordeDer + transicion) - ancho) / transicion);
        float mascaraInicio = Mathf.Clamp01((largo - (bordeInicio - transicion)) / transicion);
        float mascaraFin = Mathf.Clamp01(((bordeFin + transicion) - largo) / transicion);
        float mascara = Mathf.Min(Mathf.Min(mascaraIzq, mascaraDer), Mathf.Min(mascaraInicio, mascaraFin));
        return mascara * mascara * (3f - 2f * mascara);
    }

    float CalcularRuidoFondoGrietaPasoHelado(Vector3 worldPos, float seedA, float seedB)
    {
        float intensidad = Mathf.Max(0f, ruidoFondoGrietaPasoHelado);
        if (intensidad <= 0f)
            return 0f;

        float frecuencia = Mathf.Max(0.001f, frecuenciaRuidoFondoGrietaPasoHelado);
        float amplio = SampleSignedPerlin(worldPos.x * frecuencia + seedA * 1.7f + 91.4f, worldPos.z * frecuencia - seedB * 1.2f + 8.6f);
        float detalle = SampleSignedPerlin(worldPos.x * frecuencia * 2.9f - seedB * 0.8f - 38.1f, worldPos.z * frecuencia * 2.9f + seedA * 1.1f + 54.2f);
        return (amplio * 0.65f + detalle * 0.35f) * intensidad;
    }

    void LimpiarParedVisualPrecipicioTerrenoSur()
    {
        Transform existente = transform.Find("ParedPrecipicioTerrenoSur");
        if (existente == null)
            return;

        if (Application.isPlaying)
            Destroy(existente.gameObject);
        else
            DestroyImmediate(existente.gameObject);
    }

    Vector3 CalculateReliefNormalWorld(Vector3 worldPos)
    {
        float d = Mathf.Max(0.05f, distanciaMuestreoNormal);
        Vector3 px = new Vector3(worldPos.x + d, worldPos.y, worldPos.z);
        Vector3 nx = new Vector3(worldPos.x - d, worldPos.y, worldPos.z);
        Vector3 pz = new Vector3(worldPos.x, worldPos.y, worldPos.z + d);
        Vector3 nz = new Vector3(worldPos.x, worldPos.y, worldPos.z - d);

        float hx = EvaluateReliefHeightOffsetWorld(px) - EvaluateReliefHeightOffsetWorld(nx);
        float hz = EvaluateReliefHeightOffsetWorld(pz) - EvaluateReliefHeightOffsetWorld(nz);

        Vector3 normal = new Vector3(-hx, d * 2f, -hz).normalized;
        return normal.sqrMagnitude > 0.0001f ? normal : Vector3.up;
    }

    static float SampleSignedPerlin(float x, float y)
    {
        return (Mathf.PerlinNoise(x, y) - 0.5f) * 2f;
    }

    void Instanciar(GameObject prefab, Vector3 posMundo)
    {
        Vector3 pos = posMundo;

        if (usarRelieveProcedural && TrySampleSurface(posMundo, out var relievePos, out _, 0f))
        {
            pos = relievePos;
        }

        if (ajustarAlturaConRaycast)
        {
            if (usarColliderSueloDirecto && sueloCollider != null)
            {
                if (TryRaycastCollider(sueloCollider, posMundo, out var puntoSuelo))
                {
                    pos.y = puntoSuelo.y;
                }
            }
            else if (colliderSueloAreaActual != null)
            {
                if (TryRaycastCollider(colliderSueloAreaActual, posMundo, out var puntoSuelo))
                {
                    pos.y = puntoSuelo.y;
                }
            }
            else
            {
                MeshFilter areaRaycast = meshAreaActual != null ? meshAreaActual : planeMesh;
                int mask = raycastSoloContraPlane ? (1 << areaRaycast.gameObject.layer) : (int)capaSuelo;
                if (Physics.Raycast(new Ray(posMundo + Vector3.up * 200f, Vector3.down), out var hit, 500f, mask))
                    pos.y = hit.point.y;
            }
        }

        var go = Instantiate(prefab, pos, Quaternion.identity, transform);
        if (ConvertirSombrasBlobCanvas(go))
        {
            DesactivarSombrasDinamicasRedundantes(go);
        }
        if (go.GetComponentInChildren<ParticleSystem>(true) != null)
        {
            VisualPolishRuntime.ApplyGeneratedCampaignVfxQualityScale(go);
        }
        if (rotarYRandom) go.transform.rotation = Quaternion.Euler(0f,UnityEngine.Random.Range(0f, 360f), 0f);
        if (batchActualEsRellenoRemovible)
        {
            decoracionesRemoviblesSobreCaminos.Add(go);
            CellKey key = CellDecoracionRemovible(go.transform.position.x, go.transform.position.z);
            if (!decoracionesRemoviblesPorCelda.TryGetValue(key, out var decoracionesCelda))
            {
                decoracionesCelda = new List<GameObject>(4);
                decoracionesRemoviblesPorCelda.Add(key, decoracionesCelda);
            }
            decoracionesCelda.Add(go);
            versionDecoracionesRemovibles++;
        }
        // Escala del prefab NO se toca (asegurate que este GameObject padre está en 1,1,1).
    }

    bool ConvertirSombrasBlobCanvas(GameObject decoracion)
    {
        if (!convertirSombrasBlobCanvas || decoracion == null)
        {
            return false;
        }

        bool convirtioAlgunaSombra = false;
        canvasesSombraCandidatos.Clear();
        decoracion.GetComponentsInChildren(true, canvasesSombraCandidatos);

        for (int i = 0; i < canvasesSombraCandidatos.Count; i++)
        {
            Canvas canvas = canvasesSombraCandidatos[i];
            if (canvas == null || canvas.renderMode != RenderMode.WorldSpace)
            {
                continue;
            }

            graficosSombraCandidatos.Clear();
            canvas.gameObject.GetComponentsInChildren(true, graficosSombraCandidatos);
            if (graficosSombraCandidatos.Count != 1
                || !(graficosSombraCandidatos[0] is Image image)
                || !EsImagenSombraBlobCompatible(image))
            {
                continue;
            }

            RectTransform rect = image.rectTransform;
            SpriteRenderer spriteRenderer = image.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = image.gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = image.sprite;
            spriteRenderer.color = image.color;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = rect.rect.size;
            spriteRenderer.sortingLayerID = canvas.sortingLayerID;
            spriteRenderer.sortingOrder = canvas.sortingOrder;
            spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
            spriteRenderer.shadowCastingMode = ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;
            spriteRenderer.lightProbeUsage = LightProbeUsage.Off;
            spriteRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            spriteRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            spriteRenderer.enabled = canvas.enabled && image.enabled;

            image.raycastTarget = false;
            image.enabled = false;

            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = false;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.enabled = false;
            }

            canvas.enabled = false;
            convirtioAlgunaSombra = true;
        }

        return convirtioAlgunaSombra;
    }

    void DesactivarSombrasDinamicasRedundantes(GameObject decoracion)
    {
        renderersDecoracionConSombraBlob.Clear();
        decoracion.GetComponentsInChildren(true, renderersDecoracionConSombraBlob);

        for (int i = 0; i < renderersDecoracionConSombraBlob.Count; i++)
        {
            MeshRenderer renderer = renderersDecoracionConSombraBlob[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }
    }

    static bool EsImagenSombraBlobCompatible(Image image)
    {
        if (image == null
            || image.sprite == null
            || image.type != Image.Type.Simple
            || image.fillAmount < 0.999f)
        {
            return false;
        }

        RectTransform rect = image.rectTransform;
        if (rect == null
            || Mathf.Abs(rect.pivot.x - 0.5f) > 0.001f
            || Mathf.Abs(rect.pivot.y - 0.5f) > 0.001f)
        {
            return false;
        }

        Sprite sprite = image.sprite;
        string textureName = sprite.texture != null ? sprite.texture.name : string.Empty;
        return sprite.name == "Shadow_Blob_1024" || textureName == "Shadow_Blob_1024";
    }

    bool TryRaycastCollider(Collider colliderObjetivo, Vector3 posMundo, out Vector3 punto)
    {
        punto = posMundo;
        if (colliderObjetivo == null)
        {
            return false;
        }

        Ray ray = new Ray(posMundo + Vector3.up * 200f, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 500f, ~0, QueryTriggerInteraction.Ignore);
        bool encontrado = false;
        float menorDistancia = float.PositiveInfinity;
        Vector3 mejorPunto = posMundo;

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != colliderObjetivo || hits[i].distance >= menorDistancia)
            {
                continue;
            }

            menorDistancia = hits[i].distance;
            mejorPunto = hits[i].point;
            encontrado = true;
        }

        if (!encontrado)
        {
            return false;
        }

        punto = mejorPunto;
        return true;
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



