using UnityEngine;
using UnityEngine.Rendering;

// Relieve exclusivamente escénico detrás del mapa: sin colliders, nodos ni RNG de campaña.
[DisallowMultipleComponent]
public sealed class FondoPasoVientoHelado : MonoBehaviour
{
    Mesh relieve, bancos;
    Material roca, bruma;
    MeshFilter filtroExtension;
    Mesh extensionOriginal, extensionVisual;
    ParticleSystemRenderer cintaViento;
    bool cintaVientoOriginal;
    Transform aldea;
    Vector4 areaAldea;
    float alturaAldea;
    AtributosZona zonaActual;

    public static void Crear(AtributosZona zona, Transform padre)
    {
        if (zona == null || zona.ID != 2 || zona.TexturaTerreno == null) return;
        var existente = padre.Find("Valle y contrafuertes del Paso");
        if (existente != null)
        {
            return;
        }
        var shader = Resources.Load<Shader>("PasoVientoHeladoRelieveFondo");
        var niebla = Resources.Load<Shader>("PasoVientoHeladoBrumaFondo");
        if (shader == null || niebla == null) return;
        var go = new GameObject("Valle y contrafuertes del Paso") { layer = zona.TexturaTerreno.gameObject.layer, hideFlags = HideFlags.DontSave };
        go.transform.SetParent(padre, true);
        var fondo = go.AddComponent<FondoPasoVientoHelado>();
        fondo.Construir(zona, shader, niebla);
    }

    void Construir(AtributosZona zona, Shader shader, Shader niebla)
    {
        zonaActual = zona;
        var limites = zona.TexturaTerrenoExtension != null ? zona.TexturaTerrenoExtension.bounds : zona.TexturaTerreno.bounds;
        transform.position = new Vector3(limites.center.x, 0, zona.TexturaTerreno.bounds.max.z - 1f);
        aldea = zona.pasovientoheladoContenedorGameObjects.transform.Find("ALDEAFONDO");
        if (aldea != null)
        {
            areaAldea = new Vector4(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            alturaAldea = aldea.position.y + 1.5f;
            foreach (Transform objeto in aldea)
            {
                if (Mathf.Abs(objeto.position.x - aldea.position.x) > 25f) continue;
                var p = objeto.position;
                areaAldea.x = Mathf.Min(areaAldea.x, p.x - 2f); areaAldea.y = Mathf.Max(areaAldea.y, p.x + 2f);
                areaAldea.z = Mathf.Min(areaAldea.z, p.z - 2f); areaAldea.w = Mathf.Max(areaAldea.w, p.z + 2f);
            }
        }
        // La cinta blanca anterior atravesaba las cumbres; el viento ahora lo aportan la bruma y la nieve rasante.
        foreach (var renderer in zona.pasovientoheladoContenedorGameObjects.GetComponentsInChildren<ParticleSystemRenderer>(true))
            if (renderer.name == "Viento")
            {
                cintaViento = renderer; cintaVientoOriginal = renderer.enabled; renderer.enabled = false;
                break;
            }
        float semilla = zona.FASE * 13.71f + 47.3f;
        const int columnas = 100, filas = 60;
        const float ancho = 280f, fondo = 170f;
        var vertices = new Vector3[(columnas + 1) * (filas + 1)];
        var indices = new int[columnas * filas * 6];
        var principal = zona.TexturaTerreno.GetComponent<MeshCollider>();
        var jugable = zona.TexturaTerreno.bounds;
        for (int z = 0; z <= filas; z++)
        for (int x = 0; x <= columnas; x++)
        {
            float px = (x / (float)columnas - .5f) * ancho;
            float pz = z / (float)filas * fondo;
            float valle = Mathf.Exp(-Mathf.Pow((px + Mathf.Sin(pz * .022f) * 16f) / 38f, 2f));
            float entrada = Mathf.SmoothStep(0, 1, pz / 24f);
            float cresta = Mathf.PerlinNoise(px * .023f + semilla, pz * .025f + 17f);
            float fractura = 1f - Mathf.Abs(Mathf.PerlinNoise(px * .072f + 13f, pz * .064f + semilla) * 2f - 1f);
            // La cámara mira hacia abajo: el valle desciende para que las cumbres lejanas sigan visibles.
            float altura = 17f - pz * .64f + entrada * ((1f - valle) * (6f + cresta * 14f) + pz * .025f);
            altura += entrada * fractura * fractura * 3f;
            altura += 5f * Mathf.Exp(-Mathf.Pow((pz - 15f) / 10f, 2f));
            // El primer tramo nace bajo el borde real, con descenso continuo hacia el valle.
            var rayo = new Ray(new Vector3(Mathf.Clamp(transform.position.x + px, jugable.min.x + .05f, jugable.max.x - .05f),
                jugable.max.y + 20f, jugable.max.z - .1f), Vector3.down);
            if (pz < 14f && principal != null && principal.Raycast(rayo, out var apoyo, 100f))
                altura = Mathf.Lerp(apoyo.point.y - .3f - pz * .4f, altura, Mathf.SmoothStep(0,1,pz/14f));
            altura = AlturaSueloAldea(new Vector3(px + transform.position.x, altura, pz + transform.position.z), altura);
            // La boca del valle permanece baja; los espolones laterales rompen el borde recto del lago.
            vertices[z * (columnas + 1) + x] = new Vector3(px, altura, pz);
            if (x == columnas || z == filas) continue;
            int i = z * (columnas + 1) + x, k = (z * columnas + x) * 6;
            indices[k] = i; indices[k + 1] = i + columnas + 1; indices[k + 2] = i + 1;
            indices[k + 3] = i + 1; indices[k + 4] = i + columnas + 1; indices[k + 5] = i + columnas + 2;
        }
        relieve = new Mesh { name = "Contrafuertes nevados (Runtime)", hideFlags = HideFlags.DontSave };
        relieve.vertices = vertices; relieve.triangles = indices;
        relieve.RecalculateNormals(); relieve.RecalculateBounds();
        roca = new Material(shader) { name = "Pizarra y nieve del fondo (Runtime)", hideFlags = HideFlags.DontSave };
        roca.SetFloat("_InicioBruma", limites.max.z + 10f);
        roca.SetFloat("_FinBruma", limites.max.z + fondo - 15f);
        AgregarRenderer(gameObject, relieve, roca);

        // Tres bancos con profundidad real y bordes suaves, no una franja uniforme pegada al horizonte.
        var nieblaGO = new GameObject("Bancos de bruma del valle") { layer = gameObject.layer };
        nieblaGO.transform.SetParent(transform, false);
        var puntos = new Vector3[12]; var uv = new Vector2[12]; var caras = new int[18];
        for (int i = 0; i < 3; i++)
        {
            float distancia = 12f + i * 33f, alto = 15f + i * 4f;
            int v = i * 4, t = i * 6;
            puntos[v] = new Vector3(-125, 12, distancia); puntos[v + 1] = new Vector3(125, 12, distancia);
            puntos[v + 2] = new Vector3(-125, 12 + alto, distancia); puntos[v + 3] = new Vector3(125, 12 + alto, distancia);
            uv[v] = Vector2.zero; uv[v + 1] = Vector2.right; uv[v + 2] = Vector2.up; uv[v + 3] = Vector2.one;
            caras[t] = v; caras[t + 1] = v + 2; caras[t + 2] = v + 1;
            caras[t + 3] = v + 1; caras[t + 4] = v + 2; caras[t + 5] = v + 3;
        }
        bancos = new Mesh { name = "Bruma estratificada (Runtime)", hideFlags = HideFlags.DontSave };
        bancos.vertices = puntos; bancos.uv = uv; bancos.triangles = caras; bancos.RecalculateBounds();
        bruma = new Material(niebla) { name = "Bruma suave del valle (Runtime)", hideFlags = HideFlags.DontSave };
        AgregarRenderer(nieblaGO, bancos, bruma);
    }

    public static void ActualizarSueloAldea(AtributosZona zona, Mesh fuente)
    {
        if (zona == null || zona.ID != 2 || zona.pasovientoheladoContenedorGameObjects == null) return;
        var fondo = zona.pasovientoheladoContenedorGameObjects.GetComponentInChildren<FondoPasoVientoHelado>(true);
        if (fondo == null) return;
        fondo.AdaptarExtension(zona, fuente);
        fondo.ApoyarObjetos();
    }

    void LateUpdate()
    {
        if (zonaActual == null) zonaActual = GetComponentInParent<AtributosZona>();
        if (zonaActual == null || zonaActual.ID != 2 || zonaActual.TexturaTerrenoExtension == null) return;
        var filtro = zonaActual.TexturaTerrenoExtension.GetComponent<MeshFilter>();
        if (filtro != null && filtro.sharedMesh != null && filtro.sharedMesh != extensionVisual)
        {
            AdaptarExtension(zonaActual, filtro.sharedMesh);
            ApoyarObjetos();
        }
    }

    float AlturaSueloAldea(Vector3 p, float original)
    {
        if (aldea == null) return original;
        // Mascara local: el precipicio posterior y el resto del mapa quedan intactos.
        float z = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(areaAldea.z - 4f, areaAldea.z, p.z))
            * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(areaAldea.w, areaAldea.w + 4f, p.z)));
        float x = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(areaAldea.x - 4f, areaAldea.x, p.x))
            * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(areaAldea.y, areaAldea.y + 5f, p.x)));
        float ondulacion = (Mathf.PerlinNoise(p.x * .24f, p.z * .24f) - .5f) * .36f;
        float altura = Mathf.Lerp(original, alturaAldea + ondulacion, x * z);
        float valle = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(areaAldea.y, areaAldea.y + 6f, p.x))
            * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(areaAldea.y + 12f, areaAldea.y + 24f, p.x)));
        return Mathf.Lerp(altura, alturaAldea - 2f + ondulacion, valle * z);
    }

    void AdaptarExtension(AtributosZona zona, Mesh fuente)
    {
        if (zona.TexturaTerrenoExtension == null) return;
        filtroExtension = zona.TexturaTerrenoExtension.GetComponent<MeshFilter>();
        if (filtroExtension == null || filtroExtension.sharedMesh == null) return;
        if (fuente == null) return;
        Liberar(extensionVisual);
        extensionOriginal = fuente;
        extensionVisual = Instantiate(extensionOriginal);
        extensionVisual.name = "Descenso visual hacia el valle (Runtime)";
        extensionVisual.hideFlags = HideFlags.DontSave;
        // Densifica por interpolacion: conserva el relieve existente fuera de la aldea.
        for (int nivel = 0; nivel < 3; nivel++) Subdividir(extensionVisual);
        var vertices = extensionVisual.vertices;
        float inicio = zona.TexturaTerreno.bounds.max.z + 1f;
        var principal = zona.TexturaTerreno.GetComponent<MeshCollider>();
        var limites = zona.TexturaTerreno.bounds;
        for (int i = 0; i < vertices.Length; i++)
        {
            var p = filtroExtension.transform.TransformPoint(vertices[i]);
            // Entierra el solape visual bajo el relieve jugable y lo une suavemente por fuera.
            // Muestrea solo el collider principal; no cambia colliders ni el apoyo de los nodos.
            var cercano = new Vector3(Mathf.Clamp(p.x, limites.min.x + .05f, limites.max.x - .05f),
                limites.max.y + 20, Mathf.Clamp(p.z, limites.min.z + .05f, limites.max.z - .05f));
            float distancia = Vector2.Distance(new Vector2(p.x,p.z), new Vector2(cercano.x,cercano.z));
            if (principal != null && distancia < 3f
                && principal.Raycast(new Ray(cercano, Vector3.down), out var apoyo, 100f)
                && Mathf.Abs(p.y - apoyo.point.y) < 4f)
                p.y = Mathf.Lerp(Mathf.Min(p.y, apoyo.point.y - .18f), p.y, Mathf.SmoothStep(0,1,distancia/3f));
            p.y -= Mathf.Max(0, p.z - inicio) * 1.1f;
            p.y = AlturaSueloAldea(p, p.y);
            vertices[i] = filtroExtension.transform.InverseTransformPoint(p);
        }
        extensionVisual.vertices = vertices;
        extensionVisual.RecalculateNormals(); extensionVisual.RecalculateTangents(); extensionVisual.RecalculateBounds();
        // El collider sigue apuntando a la malla original y el terreno jugable no se altera.
        filtroExtension.sharedMesh = extensionVisual;
    }

    static void Subdividir(Mesh mesh)
    {
        var v = new System.Collections.Generic.List<Vector3>(mesh.vertices);
        var uv = new System.Collections.Generic.List<Vector2>(mesh.uv);
        bool tieneUV = uv.Count == v.Count;
        var indices = new System.Collections.Generic.List<int>();
        var medios = new System.Collections.Generic.Dictionary<ulong, int>();
        int Medio(int a, int b)
        {
            ulong clave = ((ulong)(uint)Mathf.Min(a, b) << 32) | (uint)Mathf.Max(a, b);
            if (medios.TryGetValue(clave, out int indice)) return indice;
            indice = v.Count; v.Add((v[a] + v[b]) * .5f);
            if (tieneUV) uv.Add((uv[a] + uv[b]) * .5f);
            medios.Add(clave, indice); return indice;
        }
        var t = mesh.triangles;
        for (int i = 0; i < t.Length; i += 3)
        {
            int a = t[i], b = t[i + 1], c = t[i + 2], ab = Medio(a, b), bc = Medio(b, c), ca = Medio(c, a);
            indices.AddRange(new[] { a, ab, ca, ab, b, bc, ca, bc, c, ab, bc, ca });
        }
        mesh.SetVertices(v); if (tieneUV) mesh.SetUVs(0, uv); mesh.SetTriangles(indices, 0);
    }

    void ApoyarObjetos()
    {
        if (aldea == null || extensionVisual == null) return;
        // Solo se mueven las raices decorativas: luces, sombras y efectos siguen a cada objeto.
        foreach (Transform objeto in aldea)
        {
            float suelo = float.NegativeInfinity;
            Muestrear(extensionVisual, filtroExtension.transform, objeto.position, ref suelo, false);
            Muestrear(relieve, transform, objeto.position, ref suelo, true);
            if (float.IsNegativeInfinity(suelo)) continue;
            float pie = float.PositiveInfinity;
            foreach (var renderer in objeto.GetComponentsInChildren<MeshRenderer>(true))
                pie = Mathf.Min(pie, renderer.bounds.min.y);
            if (float.IsPositiveInfinity(pie)) continue;
            objeto.position += Vector3.up * (suelo - pie - .04f);
        }
    }

    void Muestrear(Mesh mesh, Transform marco, Vector3 p, ref float altura, bool fondo)
    {
        if (mesh == null) return;
        var vertices = mesh.vertices; var caras = mesh.triangles;
        for (int i = 0; i < caras.Length; i += 3)
        {
            Vector3 a = marco.TransformPoint(vertices[caras[i]]), b = marco.TransformPoint(vertices[caras[i + 1]]), c = marco.TransformPoint(vertices[caras[i + 2]]);
            float den = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
            if (Mathf.Abs(den) < .00001f) continue;
            float u = ((b.z - c.z) * (p.x - c.x) + (c.x - b.x) * (p.z - c.z)) / den;
            float v = ((c.z - a.z) * (p.x - c.x) + (a.x - c.x) * (p.z - c.z)) / den;
            if (u < 0 || v < 0 || u + v > 1) continue;
            // Misma correccion del borde delantero que aplica el shader del fondo.
            if (fondo)
            {
                float inicio = roca.GetFloat("_InicioBruma");
                a.y -= 12f * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(inicio - 22f, inicio - 16f, a.z)));
                b.y -= 12f * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(inicio - 22f, inicio - 16f, b.z)));
                c.y -= 12f * (1f - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(inicio - 22f, inicio - 16f, c.z)));
            }
            altura = Mathf.Max(altura, a.y * u + b.y * v + c.y * (1f - u - v));
        }
    }

    static void AgregarRenderer(GameObject go, Mesh mesh, Material material)
    {
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
    }

    static void Liberar(Object objeto) { if (objeto == null) return; if (Application.isPlaying) Destroy(objeto); else DestroyImmediate(objeto); }
    public void RestaurarExtension()
    {
        if (filtroExtension != null && filtroExtension.sharedMesh == extensionVisual) filtroExtension.sharedMesh = extensionOriginal;
    }
    void OnDisable()
    {
        RestaurarExtension();
        if (cintaViento != null) cintaViento.enabled = cintaVientoOriginal;
    }
    void OnEnable()
    {
        if (cintaViento != null) cintaViento.enabled = false;
        if (filtroExtension != null && extensionVisual != null && filtroExtension.sharedMesh == extensionOriginal)
            filtroExtension.sharedMesh = extensionVisual;
    }
    void OnDestroy()
    {
        RestaurarExtension();
        if (cintaViento != null) cintaViento.enabled = cintaVientoOriginal;
        Liberar(extensionVisual); Liberar(relieve); Liberar(bancos); Liberar(roca); Liberar(bruma);
    }
}
