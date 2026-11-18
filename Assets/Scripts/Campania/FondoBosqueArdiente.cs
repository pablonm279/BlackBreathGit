using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Capas escénicas detrás del terreno jugable; no modifica el fondo pintado ni sus materiales.
[DisallowMultipleComponent]
public sealed class FondoBosqueArdiente : MonoBehaviour
{
    readonly List<Mesh> mallas = new List<Mesh>();
    readonly List<Material> materiales = new List<Material>();
    Material bruma;

    public static FondoBosqueArdiente Preparar(AtributosZona zona, Transform padre)
    {
        if (zona == null || zona.ID != 1 || zona.TexturaTerreno == null) return null;
        var existente = padre.GetComponentInChildren<FondoBosqueArdiente>(true);
        if (existente != null) return existente;
        var shader = Resources.Load<Shader>("BosqueArdienteFondo");
        var arbol = Resources.Load<GameObject>("ObjetosMapa/BosqueArdiente - Arbol1");
        if (shader == null || arbol == null) return null;
        Texture textura = null;
        foreach (var renderer in arbol.GetComponentsInChildren<MeshRenderer>(true))
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
            { textura = renderer.sharedMaterial.mainTexture; break; }
        if (textura == null) return null;
        var go = new GameObject("Profundidad del Bosque Ardiente (Runtime)") { layer = zona.TexturaTerreno.gameObject.layer };
        go.transform.SetParent(padre, true);
        var fondo = go.AddComponent<FondoBosqueArdiente>();
        fondo.Construir(zona, shader, textura);
        return fondo;
    }

    void Construir(AtributosZona zona, Shader shader, Texture textura)
    {
        var bounds = zona.TexturaTerreno.bounds;
        var decorador = zona.GetComponent<MapDecorator>();
        var rng = new System.Random(6911 + zona.FASE * 71);
        float ancho = Mathf.Max(bounds.size.x * 1.65f, 100);
        transform.position = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z + 6);
        for (int capa = 0; capa < 3; capa++)
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>(); var indices = new List<int>();
            int cantidad = 40 + capa * 8;
            float distancia = capa * 9;
            for (int i = 0; i < cantidad; i++)
            {
                float x = ((i + (float)rng.NextDouble() * .75f) / cantidad - .5f) * ancho;
                float alto = Mathf.Lerp(2.8f, 6.5f, (float)rng.NextDouble()) * (1 + capa * .12f);
                float z = distancia + (float)rng.NextDouble() * 4;
                var baseMundo = transform.position + new Vector3(x, 0, z);
                float y = decorador != null && decorador.TrySampleSurface(baseMundo, out var suelo, out _)
                    ? suelo.y - transform.position.y - .15f : -.5f;
                // La cámara mira hacia abajo: el bosque distante desciende para conservar sus copas en cuadro.
                y = Mathf.Min(y, -.4f) - 1.2f - z * .32f;
                float semiancho = alto * .38f;
                AgregarQuad(vertices, uv, indices, new Vector3(x, y, z), semiancho, alto,
                    rng.Next(2) == 0);
            }
            var material = new Material(shader) { name = "Siluetas de bosque lejano " + capa, hideFlags = HideFlags.DontSave };
            material.mainTexture = textura;
            material.SetColor("_Color", Color.Lerp(new Color(.045f, .040f, .038f, .48f), new Color(.16f, .14f, .12f, .26f), capa / 2f));
            materiales.Add(material);
            CrearRenderer("Arbolado distante " + capa, vertices, uv, indices, material);
        }
        bruma = new Material(shader) { name = "Humo lejano entre el arbolado (Runtime)", hideFlags = HideFlags.DontSave };
        bruma.SetFloat("_Modo", 1);
        materiales.Add(bruma);
        var v = new List<Vector3>(); var u = new List<Vector2>(); var t = new List<int>();
        for (int i = 0; i < 3; i++)
            AgregarQuad(v, u, t, new Vector3(0, -1.6f - i * 3.2f, 3 + i * 10), ancho * .56f, 4 + i * 1.5f, false);
        CrearRenderer("Velos de humo del fondo", v, u, t, bruma);
        Actualizar(0, .7f);
    }

    static void AgregarQuad(List<Vector3> v, List<Vector2> uv, List<int> t, Vector3 pie, float ancho, float alto, bool invertir)
    {
        int n = v.Count;
        v.Add(pie + Vector3.left * ancho); v.Add(pie + Vector3.right * ancho);
        v.Add(pie + new Vector3(-ancho, alto, 0)); v.Add(pie + new Vector3(ancho, alto, 0));
        uv.Add(new Vector2(invertir ? 1 : 0, 0)); uv.Add(new Vector2(invertir ? 0 : 1, 0));
        uv.Add(new Vector2(invertir ? 1 : 0, 1)); uv.Add(new Vector2(invertir ? 0 : 1, 1));
        t.Add(n); t.Add(n + 2); t.Add(n + 1); t.Add(n + 1); t.Add(n + 2); t.Add(n + 3);
    }

    void CrearRenderer(string nombre, List<Vector3> vertices, List<Vector2> uv, List<int> indices, Material material)
    {
        var mesh = new Mesh { name = nombre + " (Runtime)", hideFlags = HideFlags.DontSave };
        mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(indices, 0); mesh.RecalculateBounds();
        mallas.Add(mesh);
        var go = new GameObject(nombre) { layer = gameObject.layer };
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
    }

    public void Actualizar(float reloj, float dia)
    {
        foreach (var material in materiales)
        {
            material.SetFloat("_Reloj", reloj);
            material.SetFloat("_Dia", dia);
        }
        if (bruma != null) bruma.SetColor("_Color", Color.Lerp(new Color(.14f, .12f, .12f, .12f), new Color(.30f, .265f, .22f, .13f), dia));
    }

    void OnDestroy()
    {
        foreach (var mesh in mallas) Liberar(mesh);
        foreach (var material in materiales) Liberar(material);
    }
    static void Liberar(Object o) { if (o == null) return; if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }
}
