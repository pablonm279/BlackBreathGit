using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
[DisallowMultipleComponent]
public class CaminoMesh : MonoBehaviour
{
    [Header("Ajustes visuales")]
    private float width = 0.56925f;       // Ancho del camino
    private float yOffset = 0.02f;        // Altura para evitar z-fighting
    private float uvTilesPerUnit = 0.18f; // Tiling a lo largo: detalle legible desde la cámara de campaña
    private const float RoadVisualWidthScale = 1.215f;
    private const float WidthEndScale = 0.52f;
    private const float WidthCenterBoost = 0.035f;
    private const float WidthTaperSpan = 0.2f;
    private const float WidthVariation = 0.17f;
    private const float MicroWidthVariation = 0.055f;
    private const float EdgeIrregularity = 0.18f;
    private const float MicroEdgeIrregularity = 0.05f;
    private const float CenterWander = 0.045f;
    private const float MicroCenterWander = 0.012f;
    private const float UnderlayWidthScale = 1.512f;
    private const float UnderlayYOffset = -0.012f;
    private const float UnderlayEdgeIrregularityScale = 1.25f;
    private const float RutYOffset = 0.008f;
    private const float RutCenterOffsetScale = 0.43f;
    private const float RutHalfWidthScale = 0.18f;
    private const float PathOpacity = 0.92f;
    private const float PathOpacityRecorrido = 0.92f;
    private const float UnderlayOpacity = 0.16f;
    private const float UnderlayOpacityRecorrido = 0.24f;
    private const float RutOpacityRecorrido = 0.34f;
    private const float FootprintSpacing = 0.34f;
    private const float PathEmissionScale = 0.05f;
    private const float GoldenTintBlend = 0.10f;
    private const string UnderlayName = "BaseTierraCamino";
    private const string RutsName = "HuellasCarretaCamino";
    private const string FootprintsName = "PisadasConvoyCamino";

    Mesh _mesh;
    Mesh _underlayMesh;
    Mesh _rutsMesh;
    Mesh _footprintsMesh;
    MeshFilter _mf;
    MeshRenderer _mr;
    MeshFilter _underlayMf;
    MeshRenderer _underlayMr;
    MeshFilter _rutsMf;
    MeshRenderer _rutsMr;
    MeshFilter _footprintsMf;
    MeshRenderer _footprintsMr;
    Material _displayMaterial;
    Material _displaySourceMaterial;
    Material _underlayMaterial;
    Material _underlaySourceMaterial;
    Material _rutsMaterial;
    Material _rutsSourceMaterial;
    LineRenderer _lr;
    bool _visible = true;
    bool _culledByVision;
    bool _cullingVisionInicializado;
    bool _visibleParaDecoracion;
    bool _caminoRecorrido;

    public bool VisibleParaDecoracion => _visibleParaDecoracion;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _mf = GetComponent<MeshFilter>();
        if (_mf == null) _mf = gameObject.AddComponent<MeshFilter>();
        _mr = GetComponent<MeshRenderer>();
        if (_mr == null) _mr = gameObject.AddComponent<MeshRenderer>();
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "CaminoMesh";
            _mesh.MarkDynamic();
        }

        EnsureUnderlay();
        EnsureRuts();
    }

    public void RebuildFromLine()
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        int n = _lr.positionCount;
        if (n < 2) return;

        // La línea conserva los puntos para movimiento y lógica, pero la visual la dibuja la malla.
        _lr.enabled = false;

        // El LineRenderer define la curva; ambas mallas comparten esos puntos.
        var tmp = new Vector3[n];
        _lr.GetPositions(tmp);

        var ptsLocal = new Vector3[n];
        if (_lr.useWorldSpace)
        {
            for (int i = 0; i < n; i++)
                ptsLocal[i] = transform.InverseTransformPoint(tmp[i]) + Vector3.up * yOffset;
        }
        else
        {
            for (int i = 0; i < n; i++)
                ptsLocal[i] = tmp[i] + Vector3.up * yOffset;
        }

        BuildStrip(ptsLocal, _mesh, RoadVisualWidthScale, 0f, 1f);

        EnsureUnderlay();
        BuildStrip(
            ptsLocal,
            _underlayMesh,
            UnderlayWidthScale,
            UnderlayYOffset,
            UnderlayEdgeIrregularityScale);
        EnsureRuts();
        BuildRuts(ptsLocal, _rutsMesh);
        if (_caminoRecorrido)
        {
            EnsureFootprints();
            BuildFootprints(ptsLocal, _footprintsMesh);
        }

        if (_mf != null) _mf.sharedMesh = _mesh;
        if (_underlayMf != null) _underlayMf.sharedMesh = _underlayMesh;
        if (_rutsMf != null) _rutsMf.sharedMesh = _rutsMesh;
        if (_mr != null) _mr.enabled = _visible;
        if (_underlayMr != null) _underlayMr.enabled = _visible;
        if (_rutsMr != null) _rutsMr.enabled = _visible && _caminoRecorrido;
        if (_footprintsMr != null) _footprintsMr.enabled = _visible && _caminoRecorrido;
    }

    void BuildStrip(
        IList<Vector3> ptsLocal,
        Mesh targetMesh,
        float widthScaleGlobal,
        float heightOffset,
        float irregularityScale)
    {
        if (targetMesh == null)
            return;

        int n = ptsLocal.Count;
        int vCount = n * 2;
        int tCount = (n - 1) * 6;

        var verts = new Vector3[vCount];
        var norms = new Vector3[vCount];
        var uvs = new Vector2[vCount];
        var tris = new int[tCount];
        var distances = new float[n];

        float totalDistance = 0f;
        for (int i = 1; i < n; i++)
        {
            totalDistance += Vector3.Distance(ptsLocal[i], ptsLocal[i - 1]);
            distances[i] = totalDistance;
        }

        float uvOffset = CalculateStablePhase(ptsLocal, 0.37f);
        float leftPhase = CalculateStablePhase(ptsLocal, 1.73f);
        float rightPhase = CalculateStablePhase(ptsLocal, 3.11f);
        float widthPhase = CalculateStablePhase(ptsLocal, 5.27f);
        float centerPhase = CalculateStablePhase(ptsLocal, 7.91f);
        float uvScale = uvTilesPerUnit * Mathf.Lerp(0.9f, 1.1f, CalculateStablePhase(ptsLocal, 9.31f));

        for (int i = 0; i < n; i++)
        {
            float distance = distances[i];

            Vector3 forward;
            if (i == 0) forward = (ptsLocal[1] - ptsLocal[0]).normalized;
            else if (i == n - 1) forward = (ptsLocal[n - 1] - ptsLocal[n - 2]).normalized;
            else forward = (ptsLocal[i + 1] - ptsLocal[i - 1]).normalized;

            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
            if (side.sqrMagnitude <= 0.0001f)
                side = Vector3.right;

            float widthScale = EvaluateWidthScale(distance, totalDistance);
            float irregularityEnvelope = EvaluateIrregularityEnvelope(distance, totalDistance);
            float widthNoise = (
                EvaluateWidthNoise(distance, widthPhase) * WidthVariation
                + EvaluateMicroNoise(distance, widthPhase) * MicroWidthVariation)
                * irregularityEnvelope;
            float leftWidthScale = 1f + EvaluateEdgeNoise(distance, leftPhase)
                * EdgeIrregularity * irregularityScale * irregularityEnvelope
                + EvaluateMicroNoise(distance, leftPhase) * MicroEdgeIrregularity * irregularityScale * irregularityEnvelope;
            float rightWidthScale = 1f + EvaluateEdgeNoise(distance, rightPhase)
                * EdgeIrregularity * irregularityScale * irregularityEnvelope
                + EvaluateMicroNoise(distance, rightPhase) * MicroEdgeIrregularity * irregularityScale * irregularityEnvelope;
            float halfWidth = width * widthScaleGlobal * widthScale * (1f + widthNoise) * 0.5f;
            float centerOffset = (
                EvaluateCenterNoise(distance, centerPhase) * CenterWander
                + EvaluateMicroNoise(distance, centerPhase) * MicroCenterWander)
                * irregularityEnvelope;
            Vector3 center = ptsLocal[i] + Vector3.up * heightOffset + side * centerOffset;

            Vector3 left = center - side * (halfWidth * leftWidthScale);
            Vector3 right = center + side * (halfWidth * rightWidthScale);
            Vector3 normal = Vector3.Cross(forward, side).normalized;
            if (normal.sqrMagnitude <= 0.0001f || Vector3.Dot(normal, Vector3.up) < 0f)
                normal = normal.sqrMagnitude <= 0.0001f ? Vector3.up : -normal;

            int vi = i * 2;
            verts[vi] = left;
            verts[vi + 1] = right;
            norms[vi] = norms[vi + 1] = normal;

            float u = uvOffset + distance * uvScale;
            uvs[vi] = new Vector2(u, 0f);
            uvs[vi + 1] = new Vector2(u, 1f);

            if (i < n - 1)
            {
                int ti = i * 6;
                tris[ti + 0] = vi;
                tris[ti + 1] = vi + 2;
                tris[ti + 2] = vi + 1;
                tris[ti + 3] = vi + 1;
                tris[ti + 4] = vi + 2;
                tris[ti + 5] = vi + 3;
            }
        }

        targetMesh.Clear();
        targetMesh.vertices = verts;
        targetMesh.normals = norms;
        targetMesh.uv = uvs;
        targetMesh.triangles = tris;
        targetMesh.RecalculateTangents();
        targetMesh.RecalculateBounds();
    }

    void BuildRuts(IList<Vector3> ptsLocal, Mesh targetMesh)
    {
        if (targetMesh == null)
            return;

        int n = ptsLocal.Count;
        var verts = new Vector3[n * 4];
        var norms = new Vector3[n * 4];
        var uvs = new Vector2[n * 4];
        var tris = new int[(n - 1) * 12];
        var distances = new float[n];

        float totalDistance = 0f;
        for (int i = 1; i < n; i++)
        {
            totalDistance += Vector3.Distance(ptsLocal[i], ptsLocal[i - 1]);
            distances[i] = totalDistance;
        }

        float widthPhase = CalculateStablePhase(ptsLocal, 5.27f);
        float centerPhase = CalculateStablePhase(ptsLocal, 7.91f);
        float uvOffset = CalculateStablePhase(ptsLocal, 0.37f);
        float uvScale = uvTilesPerUnit * Mathf.Lerp(0.9f, 1.1f, CalculateStablePhase(ptsLocal, 9.31f));

        for (int i = 0; i < n; i++)
        {
            float distance = distances[i];
            Vector3 forward;
            if (i == 0) forward = (ptsLocal[1] - ptsLocal[0]).normalized;
            else if (i == n - 1) forward = (ptsLocal[n - 1] - ptsLocal[n - 2]).normalized;
            else forward = (ptsLocal[i + 1] - ptsLocal[i - 1]).normalized;

            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
            if (side.sqrMagnitude <= 0.0001f)
                side = Vector3.right;

            float envelope = EvaluateIrregularityEnvelope(distance, totalDistance);
            float widthNoise = (
                EvaluateWidthNoise(distance, widthPhase) * WidthVariation
                + EvaluateMicroNoise(distance, widthPhase) * MicroWidthVariation)
                * envelope;
            float halfRoadWidth = width * RoadVisualWidthScale * EvaluateWidthScale(distance, totalDistance) * (1f + widthNoise) * 0.5f;
            float centerOffset = (
                EvaluateCenterNoise(distance, centerPhase) * CenterWander
                + EvaluateMicroNoise(distance, centerPhase) * MicroCenterWander)
                * envelope;
            Vector3 roadCenter = ptsLocal[i] + Vector3.up * RutYOffset + side * centerOffset;
            float rutCenterOffset = halfRoadWidth * RutCenterOffsetScale;
            float rutHalfWidth = halfRoadWidth * RutHalfWidthScale;
            Vector3 leftRutCenter = roadCenter - side * rutCenterOffset;
            Vector3 rightRutCenter = roadCenter + side * rutCenterOffset;
            Vector3 normal = Vector3.Cross(forward, side).normalized;
            if (normal.sqrMagnitude <= 0.0001f || Vector3.Dot(normal, Vector3.up) < 0f)
                normal = normal.sqrMagnitude <= 0.0001f ? Vector3.up : -normal;

            int vi = i * 4;
            verts[vi] = leftRutCenter - side * rutHalfWidth;
            verts[vi + 1] = leftRutCenter + side * rutHalfWidth;
            verts[vi + 2] = rightRutCenter - side * rutHalfWidth;
            verts[vi + 3] = rightRutCenter + side * rutHalfWidth;
            norms[vi] = norms[vi + 1] = norms[vi + 2] = norms[vi + 3] = normal;

            float u = uvOffset + distance * uvScale;
            uvs[vi] = new Vector2(u, 0f);
            uvs[vi + 1] = new Vector2(u, 1f);
            uvs[vi + 2] = new Vector2(u, 0f);
            uvs[vi + 3] = new Vector2(u, 1f);

            if (i < n - 1)
            {
                int ti = i * 12;
                int next = vi + 4;
                tris[ti] = vi;
                tris[ti + 1] = next;
                tris[ti + 2] = vi + 1;
                tris[ti + 3] = vi + 1;
                tris[ti + 4] = next;
                tris[ti + 5] = next + 1;
                tris[ti + 6] = vi + 2;
                tris[ti + 7] = next + 2;
                tris[ti + 8] = vi + 3;
                tris[ti + 9] = vi + 3;
                tris[ti + 10] = next + 2;
                tris[ti + 11] = next + 3;
            }
        }

        targetMesh.Clear();
        targetMesh.vertices = verts;
        targetMesh.normals = norms;
        targetMesh.uv = uvs;
        targetMesh.triangles = tris;
        targetMesh.RecalculateTangents();
        targetMesh.RecalculateBounds();
    }

    void BuildFootprints(IList<Vector3> ptsLocal, Mesh targetMesh)
    {
        if (targetMesh == null || ptsLocal == null || ptsLocal.Count < 2)
            return;

        int segmentCount = ptsLocal.Count - 1;
        float[] cumulative = new float[ptsLocal.Count];
        float totalDistance = 0f;
        for (int i = 0; i < segmentCount; i++)
        {
            totalDistance += Vector3.Distance(ptsLocal[i], ptsLocal[i + 1]);
            cumulative[i + 1] = totalDistance;
        }

        int footprintCount = Mathf.Max(0, Mathf.FloorToInt((totalDistance - 0.30f) / FootprintSpacing));
        if (footprintCount == 0)
        {
            targetMesh.Clear();
            return;
        }

        var verts = new Vector3[footprintCount * 4];
        var norms = new Vector3[footprintCount * 4];
        var uvs = new Vector2[footprintCount * 4];
        var tris = new int[footprintCount * 6];
        int segmentIndex = 0;

        for (int i = 0; i < footprintCount; i++)
        {
            float distance = 0.20f + i * FootprintSpacing;
            while (segmentIndex < segmentCount - 1 && cumulative[segmentIndex + 1] < distance)
                segmentIndex++;

            float segmentLength = Mathf.Max(0.0001f, cumulative[segmentIndex + 1] - cumulative[segmentIndex]);
            float t = Mathf.Clamp01((distance - cumulative[segmentIndex]) / segmentLength);
            Vector3 forward = (ptsLocal[segmentIndex + 1] - ptsLocal[segmentIndex]).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
            if (side.sqrMagnitude <= 0.0001f) side = Vector3.right;

            float alternate = (i & 1) == 0 ? -1f : 1f;
            Vector3 center = Vector3.Lerp(ptsLocal[segmentIndex], ptsLocal[segmentIndex + 1], t)
                + Vector3.up * (RutYOffset + 0.004f)
                + side * (width * 0.10f * alternate);
            float halfLength = width * 0.14f;
            float halfWidth = width * 0.047f;
            int vi = i * 4;
            verts[vi] = center - forward * halfLength - side * halfWidth;
            verts[vi + 1] = center + forward * halfLength - side * halfWidth;
            verts[vi + 2] = center + forward * halfLength + side * halfWidth;
            verts[vi + 3] = center - forward * halfLength + side * halfWidth;
            norms[vi] = norms[vi + 1] = norms[vi + 2] = norms[vi + 3] = Vector3.up;
            uvs[vi] = new Vector2(0f, 0f);
            uvs[vi + 1] = new Vector2(1f, 0f);
            uvs[vi + 2] = new Vector2(1f, 1f);
            uvs[vi + 3] = new Vector2(0f, 1f);

            int ti = i * 6;
            tris[ti] = vi;
            tris[ti + 1] = vi + 1;
            tris[ti + 2] = vi + 2;
            tris[ti + 3] = vi;
            tris[ti + 4] = vi + 2;
            tris[ti + 5] = vi + 3;
        }

        targetMesh.Clear();
        targetMesh.vertices = verts;
        targetMesh.normals = norms;
        targetMesh.uv = uvs;
        targetMesh.triangles = tris;
        targetMesh.RecalculateBounds();
    }

    static float EvaluateWidthScale(float distance, float totalDistance)
    {
        if (totalDistance <= 0.0001f)
            return WidthEndScale;

        float t = distance / totalDistance;
        float taperDistance = Mathf.Min(0.65f, totalDistance * WidthTaperSpan);
        float taperIn = Mathf.SmoothStep(WidthEndScale, 1f, Mathf.Clamp01(distance / taperDistance));
        float taperOut = Mathf.SmoothStep(WidthEndScale, 1f, Mathf.Clamp01((totalDistance - distance) / taperDistance));
        float centerBoost = 1f + Mathf.Sin(t * Mathf.PI) * WidthCenterBoost;
        return Mathf.Min(taperIn, taperOut) * centerBoost;
    }

    static float EvaluateIrregularityEnvelope(float distance, float totalDistance)
    {
        if (totalDistance <= 0.0001f)
            return 0f;

        float blendDistance = Mathf.Min(0.8f, totalDistance * 0.25f);
        float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(distance / blendDistance));
        float fadeOut = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((totalDistance - distance) / blendDistance));
        return Mathf.Min(fadeIn, fadeOut);
    }

    static float EvaluateWidthNoise(float distance, float phase)
    {
        float phaseRadians = phase * Mathf.PI * 2f;
        float waveA = Mathf.Sin(distance * 0.58f + phaseRadians);
        float waveB = Mathf.Sin(distance * 1.27f + phaseRadians * 1.43f) * 0.38f;
        return (waveA + waveB) / 1.38f;
    }

    static float EvaluateCenterNoise(float distance, float phase)
    {
        float phaseRadians = phase * Mathf.PI * 2f;
        return Mathf.Sin(distance * 0.43f + phaseRadians) * 0.72f
            + Mathf.Sin(distance * 0.91f + phaseRadians * 1.79f) * 0.28f;
    }

    static float EvaluateEdgeNoise(float distance, float phase)
    {
        float phaseRadians = phase * Mathf.PI * 2f;
        float waveA = Mathf.Sin(distance * 1.7f + phaseRadians);
        float waveB = Mathf.Sin(distance * 4.15f + phaseRadians * 1.61f) * 0.42f;
        return (waveA + waveB) / 1.42f;
    }

    static float EvaluateMicroNoise(float distance, float phase)
    {
        float noiseA = Mathf.PerlinNoise(distance * 0.82f, phase * 11.7f) * 2f - 1f;
        float noiseB = Mathf.PerlinNoise(distance * 2.35f + 17.3f, phase * 23.9f) * 2f - 1f;
        return noiseA * 0.7f + noiseB * 0.3f;
    }

    static float CalculateStablePhase(IList<Vector3> ptsLocal, float salt)
    {
        if (ptsLocal == null || ptsLocal.Count == 0)
            return 0f;

        Vector3 first = ptsLocal[0];
        Vector3 last = ptsLocal[ptsLocal.Count - 1];
        float value = Mathf.Sin(
            first.x * 12.9898f
            + first.z * 78.233f
            + last.x * 37.719f
            + last.z * 19.913f
            + salt * 53.539f) * 43758.5453f;
        return value - Mathf.Floor(value);
    }

    void EnsureUnderlay()
    {
        if (_underlayMf == null || _underlayMr == null)
        {
            Transform underlay = transform.Find(UnderlayName);
            if (underlay == null)
            {
                GameObject underlayObject = new GameObject(UnderlayName);
                underlayObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                underlayObject.layer = gameObject.layer;
                underlayObject.transform.SetParent(transform, false);
                underlay = underlayObject.transform;
            }

            _underlayMf = underlay.GetComponent<MeshFilter>();
            if (_underlayMf == null) _underlayMf = underlay.gameObject.AddComponent<MeshFilter>();
            _underlayMr = underlay.GetComponent<MeshRenderer>();
            if (_underlayMr == null) _underlayMr = underlay.gameObject.AddComponent<MeshRenderer>();
            _underlayMr.shadowCastingMode = ShadowCastingMode.Off;
            _underlayMr.receiveShadows = false;
            if (_mr != null)
            {
                _underlayMr.sortingLayerID = _mr.sortingLayerID;
                _underlayMr.sortingOrder = _mr.sortingOrder - 1;
            }
        }

        if (_underlayMesh == null)
        {
            _underlayMesh = new Mesh();
            _underlayMesh.name = "CaminoBaseTierraMesh";
            _underlayMesh.MarkDynamic();
        }
    }

    void EnsureRuts()
    {
        if (_rutsMf == null || _rutsMr == null)
        {
            Transform ruts = transform.Find(RutsName);
            if (ruts == null)
            {
                GameObject rutsObject = new GameObject(RutsName);
                rutsObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                rutsObject.layer = gameObject.layer;
                rutsObject.transform.SetParent(transform, false);
                ruts = rutsObject.transform;
            }

            _rutsMf = ruts.GetComponent<MeshFilter>();
            if (_rutsMf == null) _rutsMf = ruts.gameObject.AddComponent<MeshFilter>();
            _rutsMr = ruts.GetComponent<MeshRenderer>();
            if (_rutsMr == null) _rutsMr = ruts.gameObject.AddComponent<MeshRenderer>();
            _rutsMr.shadowCastingMode = ShadowCastingMode.Off;
            _rutsMr.receiveShadows = false;
            if (_mr != null)
            {
                _rutsMr.sortingLayerID = _mr.sortingLayerID;
                _rutsMr.sortingOrder = _mr.sortingOrder + 1;
            }
        }

        if (_rutsMesh == null)
        {
            _rutsMesh = new Mesh();
            _rutsMesh.name = "CaminoHuellasCarretaMesh";
            _rutsMesh.MarkDynamic();
        }
    }

    void EnsureFootprints()
    {
        if (_footprintsMf == null || _footprintsMr == null)
        {
            Transform footprints = transform.Find(FootprintsName);
            if (footprints == null)
            {
                GameObject footprintsObject = new GameObject(FootprintsName);
                footprintsObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                footprintsObject.layer = gameObject.layer;
                footprintsObject.transform.SetParent(transform, false);
                footprints = footprintsObject.transform;
            }

            _footprintsMf = footprints.GetComponent<MeshFilter>();
            if (_footprintsMf == null) _footprintsMf = footprints.gameObject.AddComponent<MeshFilter>();
            _footprintsMr = footprints.GetComponent<MeshRenderer>();
            if (_footprintsMr == null) _footprintsMr = footprints.gameObject.AddComponent<MeshRenderer>();
            _footprintsMr.shadowCastingMode = ShadowCastingMode.Off;
            _footprintsMr.receiveShadows = false;
            if (_mr != null)
            {
                _footprintsMr.sortingLayerID = _mr.sortingLayerID;
                _footprintsMr.sortingOrder = _mr.sortingOrder + 2;
            }
        }

        if (_footprintsMesh == null)
        {
            _footprintsMesh = new Mesh();
            _footprintsMesh.name = "CaminoPisadasConvoyMesh";
            _footprintsMesh.MarkDynamic();
        }

        _footprintsMf.sharedMesh = _footprintsMesh;
        _footprintsMr.sharedMaterial = _rutsMaterial;
    }

    void UpdateDisplayMaterial(Material source)
    {
        if (_displayMaterial != null && _displaySourceMaterial == source)
            return;

        ReleaseDisplayMaterial();
        _displaySourceMaterial = source;
        if (source == null)
        {
            _mr.sharedMaterial = null;
            return;
        }

        _displayMaterial = new Material(source)
        {
            name = source.name + " Camino Mate (Runtime)"
        };
        ClampSurfaceShine(_displayMaterial, 0.08f, 0.06f);
        GradeDisplayMaterial(_displayMaterial, _caminoRecorrido);
        ConfigureTransparentSurface(_displayMaterial);
        _mr.sharedMaterial = _displayMaterial;
    }

    void UpdateUnderlayMaterial(Material source)
    {
        EnsureUnderlay();
        if (_underlayMaterial != null && _underlaySourceMaterial == source)
            return;

        ReleaseUnderlayMaterial();
        _underlaySourceMaterial = source;

        if (source == null)
        {
            _underlayMr.sharedMaterial = null;
            return;
        }

        _underlayMaterial = new Material(source)
        {
            name = source.name + " Base Tierra (Runtime)"
        };

        Color dirt = ObtenerColorTierraDesgastada(_caminoRecorrido ? UnderlayOpacityRecorrido : UnderlayOpacity);
        ApplyUnderlayColor("_Color", dirt);
        ApplyUnderlayColor("_BaseColor", dirt);
        if (_underlayMaterial.HasProperty("_Metallic")) _underlayMaterial.SetFloat("_Metallic", 0f);
        if (_underlayMaterial.HasProperty("_Glossiness")) _underlayMaterial.SetFloat("_Glossiness", 0f);
        if (_underlayMaterial.HasProperty("_Smoothness")) _underlayMaterial.SetFloat("_Smoothness", 0f);
        if (_underlayMaterial.HasProperty("_BumpScale")) _underlayMaterial.SetFloat("_BumpScale", 0.35f);
        if (_underlayMaterial.HasProperty("_EmissionColor"))
        {
            _underlayMaterial.SetColor("_EmissionColor", Color.black);
            _underlayMaterial.DisableKeyword("_EMISSION");
        }

        ConfigureTransparentSurface(_underlayMaterial);
        _underlayMaterial.renderQueue = (int)RenderQueue.Transparent - 1;

        _underlayMr.sharedMaterial = _underlayMaterial;
    }

    void UpdateRutsMaterial(Material source)
    {
        EnsureRuts();
        if (_rutsMaterial != null && _rutsSourceMaterial == source)
            return;

        ReleaseRutsMaterial();
        _rutsSourceMaterial = source;
        if (source == null)
        {
            _rutsMr.sharedMaterial = null;
            return;
        }

        _rutsMaterial = new Material(source)
        {
            name = source.name + " Huellas Carreta (Runtime)"
        };

        Color wornDirt = ObtenerColorHuellas(RutOpacityRecorrido);
        ApplyDerivedColor(_rutsMaterial, "_Color", wornDirt, 0.68f);
        ApplyDerivedColor(_rutsMaterial, "_BaseColor", wornDirt, 0.68f);
        ClampSurfaceShine(_rutsMaterial, 0f, 0f);
        if (_rutsMaterial.HasProperty("_BumpScale")) _rutsMaterial.SetFloat("_BumpScale", 0.22f);
        DisableEmission(_rutsMaterial);
        ConfigureTransparentSurface(_rutsMaterial);
        _rutsMaterial.renderQueue = (int)RenderQueue.Transparent + 1;

        _rutsMr.sharedMaterial = _rutsMaterial;
        if (_footprintsMr != null) _footprintsMr.sharedMaterial = _rutsMaterial;
    }

    void ApplyUnderlayColor(string propertyName, Color dirt)
    {
        ApplyDerivedColor(_underlayMaterial, propertyName, dirt, 0.72f);
    }

    static void ApplyDerivedColor(Material material, string propertyName, Color tint, float blend)
    {
        if (material == null || !material.HasProperty(propertyName))
            return;

        Color sourceColor = material.GetColor(propertyName);
        Color derivedColor = Color.Lerp(sourceColor, tint, blend);
        derivedColor.a = Mathf.Min(sourceColor.a, tint.a);
        material.SetColor(propertyName, derivedColor);
    }

    static void GradeDisplayMaterial(Material material, bool recorrido)
    {
        if (material == null)
            return;

        Color sourceEmission = material.HasProperty("_EmissionColor")
            ? material.GetColor("_EmissionColor")
            : Color.black;
        bool hasGoldenSignal = sourceEmission.maxColorComponent > 0.02f
            && sourceEmission.r > sourceEmission.g
            && sourceEmission.g > sourceEmission.b * 1.5f;

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", GradePathColor(material.GetColor("_Color"), hasGoldenSignal, recorrido));
        }
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", GradePathColor(material.GetColor("_BaseColor"), hasGoldenSignal, recorrido));
        }
        if (material.HasProperty("_BumpScale"))
            material.SetFloat("_BumpScale", Mathf.Max(material.GetFloat("_BumpScale"), 0.8f));
        if (material.HasProperty("_EmissionColor"))
        {
            Color emission = sourceEmission * PathEmissionScale;
            material.SetColor("_EmissionColor", emission);
            if (emission.maxColorComponent > 0.001f)
                material.EnableKeyword("_EMISSION");
            else
                material.DisableKeyword("_EMISSION");
        }
    }

    static Color GradePathColor(Color color, bool hasGoldenSignal, bool recorrido)
    {
        float multiplicador = recorrido ? 0.80f : 0.86f;
        Color graded = new Color(
            color.r * multiplicador,
            color.g * multiplicador,
            color.b * multiplicador,
            Mathf.Max(color.a, recorrido ? PathOpacityRecorrido : PathOpacity));

        if (hasGoldenSignal)
        {
            Color subtleGold = new Color(0.58f, 0.39f, 0.18f, graded.a);
            graded = Color.Lerp(graded, subtleGold, GoldenTintBlend);
        }

        return graded;
    }

    static Color ObtenerColorTierraDesgastada(float alpha)
    {
        int zona = CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null
            ? CampaignManager.Instance.scAtributosZona.ID
            : 0;
        if (zona == 2) return new Color(0.18f, 0.23f, 0.25f, alpha);
        if (zona == 1) return new Color(0.24f, 0.15f, 0.075f, alpha);
        if (zona == 3) return new Color(0.16f, 0.14f, 0.12f, alpha);
        return new Color(0.28f, 0.18f, 0.09f, alpha);
    }

    static Color ObtenerColorHuellas(float alpha)
    {
        int zona = CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null
            ? CampaignManager.Instance.scAtributosZona.ID
            : 0;
        if (zona == 2) return new Color(0.10f, 0.16f, 0.18f, alpha);
        if (zona == 1) return new Color(0.075f, 0.042f, 0.018f, alpha);
        if (zona == 3) return new Color(0.075f, 0.065f, 0.055f, alpha);
        return new Color(0.10f, 0.06f, 0.025f, alpha);
    }

    static void ConfigureTransparentSurface(Material material)
    {
        if (material == null)
            return;

        if (material.HasProperty("_Mode"))
            material.SetFloat("_Mode", 2f);
        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 1f);

        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)RenderQueue.Transparent;
    }

    static void ClampSurfaceShine(Material material, float maxMetallic, float maxSmoothness)
    {
        if (material == null)
            return;

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", Mathf.Min(material.GetFloat("_Metallic"), maxMetallic));
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", Mathf.Min(material.GetFloat("_Glossiness"), maxSmoothness));
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", Mathf.Min(material.GetFloat("_Smoothness"), maxSmoothness));
    }

    static void DisableEmission(Material material)
    {
        if (material == null || !material.HasProperty("_EmissionColor"))
            return;

        material.SetColor("_EmissionColor", Color.black);
        material.DisableKeyword("_EMISSION");
    }

    void ReleaseUnderlayMaterial()
    {
        if (_underlayMaterial == null)
            return;

        if (Application.isPlaying)
            Destroy(_underlayMaterial);
        else
            DestroyImmediate(_underlayMaterial);
        _underlayMaterial = null;
        _underlaySourceMaterial = null;
    }

    void ReleaseDisplayMaterial()
    {
        ReleaseMaterial(_displayMaterial);
        _displayMaterial = null;
        _displaySourceMaterial = null;
    }

    void ReleaseRutsMaterial()
    {
        ReleaseMaterial(_rutsMaterial);
        _rutsMaterial = null;
        _rutsSourceMaterial = null;
    }

    static void ReleaseMaterial(Material material)
    {
        if (material == null)
            return;

        if (Application.isPlaying)
            Destroy(material);
        else
            DestroyImmediate(material);
    }

    public void SetWidth(float newWidth)
    {
        width = Mathf.Max(0.05f, newWidth);
    }

    public float GetWidth()
    {
        return width;
    }

    public void SetYOffset(float newYOffset)
    {
        yOffset = Mathf.Max(0f, newYOffset);
    }

    public void SetVisible(bool visible)
    {
        _visible = visible;
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        if (_mr != null) _mr.enabled = _visible;
        if (_underlayMr != null) _underlayMr.enabled = _visible;
        if (_rutsMr != null) _rutsMr.enabled = _visible && _caminoRecorrido;
        if (_footprintsMr != null) _footprintsMr.enabled = _visible && _caminoRecorrido;
        if (_lr != null) _lr.enabled = false;
    }

    public void SetCulledByVision(bool culled)
    {
        if (_cullingVisionInicializado && _culledByVision == culled)
            return;

        _culledByVision = culled;
        _cullingVisionInicializado = true;
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        EnsureUnderlay();
        EnsureRuts();

        if (_lr != null) _lr.forceRenderingOff = culled;
        if (_mr != null) _mr.forceRenderingOff = culled;
        if (_underlayMr != null) _underlayMr.forceRenderingOff = culled;
        if (_rutsMr != null) _rutsMr.forceRenderingOff = culled;
        if (_footprintsMr != null) _footprintsMr.forceRenderingOff = culled;
    }

    public void SetEstadoRecorrido(bool recorrido)
    {
        if (_caminoRecorrido == recorrido)
            return;

        _caminoRecorrido = recorrido;
        if (_caminoRecorrido)
        {
            EnsureFootprints();
            RebuildFromLine();
        }

        if (_rutsMr != null) _rutsMr.enabled = _visible && _caminoRecorrido;
        if (_footprintsMr != null) _footprintsMr.enabled = _visible && _caminoRecorrido;
    }

    public void SetVisibleParaDecoracion(bool visible)
    {
        _visibleParaDecoracion = visible;
    }

    public void SetMaterial(Material material)
    {
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        UpdateDisplayMaterial(material);
        UpdateUnderlayMaterial(material);
        UpdateRutsMaterial(material);
        ClearMaterialPropertyOverrides();
    }

    public MeshRenderer GetMeshRenderer() => _mr;

    void ClearMaterialPropertyOverrides()
    {
        if (_mr != null) _mr.SetPropertyBlock(null);
        if (_underlayMr != null) _underlayMr.SetPropertyBlock(null);
        if (_rutsMr != null) _rutsMr.SetPropertyBlock(null);
        if (_footprintsMr != null) _footprintsMr.SetPropertyBlock(null);
    }

    void OnDestroy()
    {
        ReleaseDisplayMaterial();
        ReleaseUnderlayMaterial();
        ReleaseRutsMaterial();
        ReleaseMesh(_mesh);
        ReleaseMesh(_underlayMesh);
        ReleaseMesh(_rutsMesh);
        ReleaseMesh(_footprintsMesh);
    }

    static void ReleaseMesh(Mesh mesh)
    {
        if (mesh == null)
            return;

        if (Application.isPlaying)
            Destroy(mesh);
        else
            DestroyImmediate(mesh);
    }
}
