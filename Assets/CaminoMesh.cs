using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[DisallowMultipleComponent]
public class CaminoMesh : MonoBehaviour
{
    [Header("Ajustes visuales")]
    private float width = 0.45f;           // Ancho del camino
    private float yOffset = 0.02f;        // Altura para evitar z-fighting
    private float uvTilesPerUnit = 0.35f; // Tiling a lo largo

    Mesh _mesh;
    MeshFilter _mf;
    MeshRenderer _mr;
    LineRenderer _lr;

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
        for (int i = 0; i < n; i++)
        {
            // Direcciones en LOCAL
            Vector3 forward;
            if (i == 0) forward = (ptsLocal[1] - ptsLocal[0]).normalized;
            else if (i == n - 1) forward = (ptsLocal[n - 1] - ptsLocal[n - 2]).normalized;
            else forward = (ptsLocal[i + 1] - ptsLocal[i - 1]).normalized;

            // Perpendicular en el plano XZ local
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 left  = ptsLocal[i] - side * (width * 0.5f);
            Vector3 right = ptsLocal[i] + side * (width * 0.5f);

            int vi = i * 2;
            verts[vi] = left;
            verts[vi + 1] = right;
            norms[vi] = norms[vi + 1] = Vector3.up;

            if (i > 0) accDist += Vector3.Distance(ptsLocal[i], ptsLocal[i - 1]);

            float u = accDist * uvTilesPerUnit;
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

        _mesh.Clear();
        _mesh.vertices  = verts;
        _mesh.normals   = norms;
        _mesh.uv        = uvs;
        _mesh.triangles = tris;
        _mesh.RecalculateBounds();
        _mf.sharedMesh = _mesh;

        if (_mr != null) _mr.enabled = true;
    }

    // Opcional para setear material por código
    public void SetMaterial(Material m)
    {
        if (_mr == null) _mr = GetComponent<MeshRenderer>();
        _mr.sharedMaterial = m;
    }

    public MeshRenderer GetMeshRenderer() => _mr;
}
