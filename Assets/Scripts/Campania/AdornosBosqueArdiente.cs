using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Detalles exclusivamente visuales. Usa su propia semilla y respeta incluso las rutas ocultas.
[DisallowMultipleComponent]
public sealed class AdornosBosqueArdiente : MonoBehaviour
{
    struct Tramo { public Vector3 a, b; public float margen; }
    struct Adorno { public GameObject objeto; public Vector3 centro; public float radio; }
    readonly List<Tramo> caminos = new List<Tramo>();
    readonly List<Vector3> nodos = new List<Vector3>();
    readonly List<Vector4> ocupados = new List<Vector4>();
    readonly List<Adorno> adornos = new List<Adorno>();
    readonly List<Mesh> mallas = new List<Mesh>();
    AtributosZona zona;
    MapDecorator decorador;
    GameObject raiz;
    Material material;
    System.Random rng;
    Bounds terreno;
    float paso;

    public int CantidadAdornos => adornos.Count;

    public static IEnumerator Generar(AtributosZona atributos)
    {
        if (atributos == null || atributos.ID != 1 || atributos.TexturaTerreno == null
            || atributos.bosqueardienteContenedorGameObjects == null) yield break;
        var contenedor = atributos.bosqueardienteContenedorGameObjects;
        var generador = contenedor.GetComponent<AdornosBosqueArdiente>();
        if (generador == null) generador = contenedor.AddComponent<AdornosBosqueArdiente>();
        yield return generador.Construir(atributos);
    }

    IEnumerator Construir(AtributosZona atributos)
    {
        Limpiar();
        zona = atributos;
        decorador = zona.GetComponent<MapDecorator>();
        var shader = Resources.Load<Shader>("BosqueArdienteMadera");
        if (decorador == null || shader == null) yield break;
        material = new Material(shader) { name = "Madera carbonizada del bosque (Runtime)", hideFlags = HideFlags.DontSave };
        raiz = new GameObject("Troncos, tocones y ramas quemadas (Runtime)") { layer = zona.TexturaTerreno.gameObject.layer };
        raiz.transform.SetParent(transform, true); // Tamaños de mundo, sin heredar escala no uniforme.
        terreno = zona.TexturaTerreno.bounds;
        paso = CampaignManager.Instance != null && CampaignManager.Instance.scMapaManager != null
            ? Mathf.Clamp(CampaignManager.Instance.scMapaManager.ObtenerPasoMapa(), 3, 12) : 5;
        rng = new System.Random(unchecked(decorador.GetReliefSeed() ^ (zona.FASE * 19349663) ^ 0x4b057e));
        RecolectarExclusiones();
        int calidad = Mathf.Clamp(PlayerPrefs.GetInt("graficos_index", QualitySettings.GetQualityLevel()), 0, 2);
        int cantidad = Mathf.Clamp(Mathf.RoundToInt(terreno.size.x * terreno.size.z / (paso * paso * 3.5f)), 24, 72);
        cantidad = Mathf.RoundToInt(cantidad * (calidad == 0 ? .55f : calidad == 1 ? .8f : 1));
        for (int i = 0; i < cantidad; i++)
        {
            if (zona == null || zona.ID != 1) yield break;
            int tipo = i % 5;
            float radio = paso * (tipo == 0 ? Rango(.20f, .28f) : Rango(.10f, .18f));
            for (int intento = 0; intento < 24; intento++)
            {
                var p = new Vector3(Rango(terreno.min.x + radio, terreno.max.x - radio), terreno.center.y,
                    Rango(terreno.min.z + radio, terreno.max.z - radio));
                if (!LugarLibre(p, radio, out var centro)) continue;
                CrearDetalle(centro, radio, tipo);
                break;
            }
            if (i % 4 == 3) yield return null;
        }
    }

    void RecolectarExclusiones()
    {
        caminos.Clear(); nodos.Clear(); ocupados.Clear();
        foreach (var nodo in Object.FindObjectsByType<Nodo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (nodo.gameObject.scene == zona.gameObject.scene) nodos.Add(nodo.transform.position);
        foreach (var camino in Object.FindObjectsByType<CaminoMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (camino.gameObject.scene != zona.gameObject.scene) continue;
            var linea = camino.GetComponent<LineRenderer>();
            if (linea == null) continue;
            float escala = Mathf.Max(Mathf.Abs(linea.transform.lossyScale.x), Mathf.Abs(linea.transform.lossyScale.z));
            float margen = camino.GetWidth() * escala * 1.1f + paso * .14f;
            for (int i = 1; i < linea.positionCount; i++)
                caminos.Add(new Tramo { a = Punto(linea, i - 1), b = Punto(linea, i), margen = margen });
        }
        foreach (Transform hijo in decorador.transform)
        {
            if (!hijo.name.EndsWith("(Clone)")) continue;
            var renderer = hijo.GetComponentInChildren<Renderer>();
            if (renderer == null || renderer is ParticleSystemRenderer) continue;
            float radio = Mathf.Clamp(Mathf.Min(renderer.bounds.size.x, renderer.bounds.size.z) * .3f, .15f, paso * .35f);
            ocupados.Add(new Vector4(hijo.position.x, 0, hijo.position.z, radio));
        }
    }

    bool LugarLibre(Vector3 p, float radio, out Vector3 centro)
    {
        centro = p;
        if (!decorador.PasaMargenCornisaFrontal(p, radio)
            || !decorador.TrySampleSurface(p, out centro, out var normal) || normal.y < .92f) return false;
        foreach (var nodo in nodos)
            if (DistanciaXZ(centro, nodo) < radio + paso * .32f) return false;
        foreach (var tramo in caminos)
            if (DistanciaSegmento(centro, tramo.a, tramo.b) < radio + tramo.margen) return false;
        foreach (var objeto in ocupados)
            if (DistanciaXZ(centro, objeto) < radio + objeto.w) return false;
        foreach (var adorno in adornos)
            if (DistanciaXZ(centro, adorno.centro) < radio + adorno.radio + paso * .2f) return false;
        foreach (var eje in Ejes)
            if (!decorador.TrySampleSurface(centro + eje * radio, out var borde, out _)
                || Mathf.Abs(borde.y - centro.y) > radio * .4f) return false;
        return true;
    }

    static readonly Vector3[] Ejes = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

    void CrearDetalle(Vector3 centro, float radio, int tipo)
    {
        var vertices = new List<Vector3>(); var colores = new List<Color>(); var indices = new List<int>();
        var giro = Quaternion.Euler(0, Rango(0, 360), 0);
        Color carbon = new Color(.047f, .039f, .033f), corte = new Color(.105f, .077f, .052f);
        if (tipo == 0)
        {
            Vector3 a = Apoyo(centro + giro * Vector3.left * radio * .74f) - centro;
            Vector3 b = Apoyo(centro + giro * Vector3.right * radio * .74f) - centro;
            float grueso = radio * .16f;
            AgregarRama(vertices, colores, indices, a + Vector3.up * grueso * .5f, b + Vector3.up * grueso * .6f, grueso, carbon, corte);
            AgregarRama(vertices, colores, indices, Vector3.up * grueso, giro * new Vector3(radio * .17f, grueso * 1.5f, radio * .5f), grueso * .38f, carbon, carbon);
        }
        else if (tipo == 1)
        {
            AgregarRama(vertices, colores, indices, Vector3.down * .04f, Vector3.up * radio * Rango(.6f, .9f), radio * .35f, carbon, corte);
            for (int j = 0; j < 4; j++)
            {
                var fin = Quaternion.Euler(0, j * 91 + Rango(-15, 15), 0) * Vector3.right * radio * .8f;
                fin = Apoyo(centro + fin) - centro;
                AgregarRama(vertices, colores, indices, Vector3.up * radio * .2f, fin, radio * .12f, carbon, carbon);
            }
        }
        else
        {
            for (int j = 0; j < (tipo == 2 ? 5 : 3); j++)
            {
                var a = giro * new Vector3(Rango(-.65f, .1f), 0, Rango(-.55f, .55f)) * radio;
                var b = a + giro * new Vector3(Rango(.35f, .75f), 0, Rango(-.35f, .35f)) * radio;
                AgregarRama(vertices, colores, indices, Apoyo(centro + a) - centro, Apoyo(centro + b) - centro,
                    radio * Rango(.035f, .065f), Color.Lerp(carbon, corte, .2f), corte);
            }
        }
        var mesh = new Mesh { name = "Restos de madera quemada (Runtime)", hideFlags = HideFlags.DontSave };
        mesh.SetVertices(vertices); mesh.SetColors(colores); mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        var go = new GameObject(tipo == 0 ? "Tronco caído carbonizado" : tipo == 1 ? "Tocón quemado con raíces" : "Ramas y astillas carbonizadas") { layer = raiz.layer };
        go.transform.SetParent(raiz.transform, false); go.transform.position = centro;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = true;
        mallas.Add(mesh); adornos.Add(new Adorno { objeto = go, centro = centro, radio = radio });
    }

    void AgregarRama(List<Vector3> v, List<Color> c, List<int> t, Vector3 a, Vector3 b, float radio, Color carbon, Color corte)
    {
        const int lados = 7, anillos = 4;
        int inicio = v.Count;
        var eje = (b - a).normalized;
        var u = Vector3.Cross(eje, Mathf.Abs(eje.y) > .9f ? Vector3.forward : Vector3.up).normalized;
        var w = Vector3.Cross(eje, u);
        float fase = Rango(0, 6.28f);
        for (int anillo = 0; anillo < anillos; anillo++)
        for (int j = 0; j < lados; j++)
        {
            float f = anillo / (float)(anillos - 1), angulo = j * Mathf.PI * 2 / lados;
            float r = radio * Mathf.Lerp(1, .57f, f) * (1 + Mathf.Sin(j * 13.7f + fase) * .17f);
            var punto = Vector3.Lerp(a, b, f) + (u * Mathf.Cos(angulo) + w * Mathf.Sin(angulo)) * r;
            punto += eje * (anillo == anillos - 1 ? Rango(-radio * .4f, radio * .35f) : 0);
            v.Add(punto); c.Add(Color.Lerp(carbon * Rango(.72f, 1.12f), corte, anillo == anillos - 1 ? .27f : .06f));
            if (anillo == 0) continue;
            int actual = inicio + anillo * lados + j, siguiente = inicio + anillo * lados + (j + 1) % lados;
            Triangulo(t, actual - lados, siguiente, actual); Triangulo(t, actual - lados, siguiente - lados, siguiente);
        }
        int tapa = v.Count; v.Add(b); c.Add(corte);
        for (int j = 0; j < lados; j++) Triangulo(t, tapa, inicio + (anillos - 1) * lados + j, inicio + (anillos - 1) * lados + (j + 1) % lados);
    }

    public void RetirarSobreCamino(LineRenderer linea, float margen)
    {
        if (zona == null || zona.ID != 1 || linea == null) return;
        foreach (var adorno in adornos)
        {
            if (adorno.objeto == null || !adorno.objeto.activeSelf) continue;
            for (int i = 1; i < linea.positionCount; i++)
                if (DistanciaSegmento(adorno.centro, Punto(linea, i - 1), Punto(linea, i)) < adorno.radio + margen)
                { adorno.objeto.SetActive(false); break; }
        }
    }

    Vector3 Apoyo(Vector3 p) => decorador.TrySampleSurface(p, out var suelo, out _) ? suelo + Vector3.up * .025f : p;
    float Rango(float a, float b) => Mathf.Lerp(a, b, (float)rng.NextDouble());
    static float DistanciaXZ(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    static Vector3 Punto(LineRenderer l, int i) => l.useWorldSpace ? l.GetPosition(i) : l.transform.TransformPoint(l.GetPosition(i));
    static void Triangulo(List<int> t, int a, int b, int c) { t.Add(a); t.Add(b); t.Add(c); }
    static float DistanciaSegmento(Vector3 p, Vector3 a, Vector3 b)
    {
        var inicio = new Vector2(a.x, a.z); var eje = new Vector2(b.x - a.x, b.z - a.z);
        float f = eje.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector2.Dot(new Vector2(p.x, p.z) - inicio, eje) / eje.sqrMagnitude) : 0;
        return Vector2.Distance(new Vector2(p.x, p.z), inicio + eje * f);
    }
    void Limpiar()
    {
        if (raiz != null) { raiz.SetActive(false); Liberar(raiz); }
        foreach (var mesh in mallas) Liberar(mesh);
        Liberar(material); adornos.Clear(); mallas.Clear();
    }
    static void Liberar(Object o) { if (o == null) return; if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }
    void OnDestroy() => Limpiar();
}
