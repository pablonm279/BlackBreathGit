using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[DisallowMultipleComponent]
public class CaminoMesh : MonoBehaviour
{
    [Header("Ajustes visuales")]
    private float width = 0.45f;           // Ancho del camino
    private float yOffset = 0.02f;        // Altura para evitar z-fighting
    private float uvTilesPerUnit = 0.42f; // Tiling a lo largo
    private const float WidthEndScale = 0.52f;
    private const float WidthCenterBoost = 0.035f;
    private const float WidthTaperSpan = 0.2f;
    private const float EdgeIrregularity = 0.055f;

    Mesh _mesh;
    MeshFilter _mf;
    MeshRenderer _mr;
    LineRenderer _lr;
    bool _visible = true;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _mf = GetComponent<MeshFilter>();   if (_mf == null) _mf = gameObject.AddComponent<MeshFilter>();
        _mr = GetComponent<MeshRenderer>(); if (_mr == null) _mr = gameObject.AddComponent<MeshRenderer>();
        if (_mesh == null) { _mesh = new Mesh(); _mesh.name = "CaminoMesh"; _mesh.MarkDynamic(); }
    }

    public void RebuildFromLine()
    {
        if (_lr == null) _lr = GetComponent<LineRenderer>();
        int n = _lr.positionCount;
        if (n < 2) return;

        // 1) Tomo puntos desde el LR
        var tmp = new Vector3[n];
        _lr.GetPositions(tmp);

        // 2) Los convierto a LOCAL del objeto que tiene el MeshFilter
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

        BuildStrip(ptsLocal);
    }

    void BuildStrip(IList<Vector3> ptsLocal)
    {
        int n = ptsLocal.Count;
        int vCount = n * 2;
        int tCount = (n - 1) * 6;

        var verts = new Vector3[vCount];
        var norms = new Vector3[vCount];
        var uvs   = new Vector2[vCount];
        var tris  = new int[tCount];

        float accDist = 0f;
        float uvOffset = CalculateStablePhase(ptsLocal, 0.37f);
        float leftPhase = CalculateStablePhase(ptsLocal, 1.73f);
        float rightPhase = CalculateStablePhase(ptsLocal, 3.11f);
        for (int i = 0; i < n; i++)
        {
            float t = n > 1 ? i / (float)(n - 1) : 0f;

            // Direcciones en LOCAL
            Vector3 forward;
            if (i == 0) forward = (ptsLocal[1] - ptsLocal[0]).normalized;
            else if (i == n - 1) forward = (ptsLocal[n - 1] - ptsLocal[n - 2]).normalized;
            else forward = (ptsLocal[i + 1] - ptsLocal[i - 1]).normalized;

            // Perpendicular en el plano XZ local
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
            if (side.sqrMagnitude <= 0.0001f)
                side = Vector3.right;
            float widthScale = EvaluateWidthScale(t);
            float irregularityEnvelope = Mathf.Sin(t * Mathf.PI);
            float leftWidthScale = 1f + EvaluateEdgeNoise(t, leftPhase) * EdgeIrregularity * irregularityEnvelope;
            float rightWidthScale = 1f + EvaluateEdgeNoise(t, rightPhase) * EdgeIrregularity * irregularityEnvelope;
            float halfWidth = width * widthScale * 0.5f;

            Vector3 left  = ptsLocal[i] - side * (halfWidth * leftWidthScale);
            Vector3 right = ptsLocal[i] + side * (halfWidth * rightWidthScale);
            Vector3 normal = Vector3.Cross(forward, side).normalized;
            if (normal.sqrMagnitude <= 0.0001f || Vector3.Dot(normal, Vector3.up) < 0f)
                normal = normal.sqrMagnitude <= 0.0001f ? Vector3.up : -normal;

            int vi = i * 2;
            verts[vi] = left;
            verts[vi + 1] = right;
            norms[vi] = norms[vi + 1] = normal;

            if (i > 0) accDist += Vector3.Distance(ptsLocal[i], ptsLocal[i - 1]);

            float u = uvOffset + accDist * uvTilesPerUnit;
            uvs[vi]     = new Vector2(u, 0f);
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

        if (_mesh != null)
        {
            _mesh.Clear();
            _mesh.vertices = verts;
            _mesh.normals = norms;
            _mesh.uv = uvs;
            _mesh.triangles = tris;
            _mesh.RecalculateTangents();
            _mesh.RecalculateBounds();
            _mf.sharedMesh = _mesh;
        }

        if (_mr != null) _mr.enabled = _visible;
    }

    float EvaluateWidthScale(float t)
    {
        float taperIn = Mathf.SmoothStep(WidthEndScale, 1f, Mathf.Clamp01(t / WidthTaperSpan));
        float taperOut = Mathf.SmoothStep(WidthEndScale, 1f, Mathf.Clamp01((1f - t) / WidthTaperSpan));
        float centerBoost = 1f + Mathf.Sin(t * Mathf.PI) * WidthCenterBoost;
        return Mathf.Min(taperIn, taperOut) * centerBoost;
    }

    static float EvaluateEdgeNoise(float t, float phase)
    {
        float waveA = Mathf.Sin((t * 4.1f + phase) * Mathf.PI * 2f);
        float waveB = Mathf.Sin((t * 7.3f + phase * 1.61f) * Mathf.PI * 2f) * 0.42f;
        return (waveA + waveB) / 1.42f;
    }

    static float CalculateStablePhase(IList<Vector3> ptsLocal, float salt)
    {
        if (ptsLocal == null || ptsLocal.Count == 0)
            return 0f;

        Vector3 first = ptsLocal[0];
        Vector3 last = ptsLocal[ptsLocal.Count - 1];
        float value = Mathf.Sin(
            first.x * 12.9898f +
            first.z * 78.233f +
            last.x * 37.719f +
            last.z * 19.913f +
            salt * 53.539f) * 43758.5453f;
        return value - Mathf.Floor(value);
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
    }

    // Opcional para setear material por código
    public void SetMaterial(Material m)
    {
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        _mr.sharedMaterial = m;
    }

    public MeshRenderer GetMeshRenderer() => _mr;
}


