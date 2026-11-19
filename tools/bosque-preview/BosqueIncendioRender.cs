using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Validación de la revisión de caminos, llamas y pared de humo en una copia aislada.
public static class BosqueIncendioRender
{
    static readonly string Salida = Path.GetFullPath("../../output/bosque-incendio-v3");
    static readonly StringBuilder log = new StringBuilder();
    static void Check(bool ok, string mensaje)
    { if (!ok) throw new Exception(mensaje); log.AppendLine("OK " + mensaje); }
    static void Ejecutar(IEnumerator rutina)
    {
        try { while (rutina.MoveNext()) if (rutina.Current is IEnumerator hija) Ejecutar(hija); }
        finally { (rutina as IDisposable)?.Dispose(); }
    }
    public static void Run()
    {
        Directory.CreateDirectory(Salida);
        try { Verificar(); }
        finally { File.WriteAllText(Path.Combine(Salida, "validacion.txt"), log.ToString()); }
        Debug.Log("BOSQUE_INCENDIO_VALIDADO\n" + log);
    }
    static void Verificar()
    {
        EditorSceneManager.OpenScene("Assets/BosqueEscenarioReal.unity");
        var decorador = UnityEngine.Object.FindFirstObjectByType<MapDecorator>();
        var zona = decorador.gameObject.AddComponent<AtributosZona>(); zona.ID = zona.FASE = 1;
        zona.bosqueardienteContenedorGameObjects = GameObject.Find("ContenedorGameObjectsBosqueArdiente");
        zona.TexturaTerreno = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).First(r => r.name.StartsWith("Terreno (0)"));
        zona.TexturaTerrenoExtension = GameObject.Find("Terrenoextenso").GetComponent<MeshRenderer>();
        var perfilPaso = CaminosBosqueFixture.Crear(); zona.pasovientoheladoContenedorGameObjects = perfilPaso.gameObject;
        var cm = new GameObject("Reloj aislado").AddComponent<CampaignManager>();
        CampaignManager.Instance = cm; cm.scAtributosZona = zona;
        var sol = zona.bosqueardienteContenedorGameObjects.GetComponentInChildren<SunController>();
        typeof(SunController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(sol, null);
        typeof(MapDecorator).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(decorador, null);
        decorador.RegenerarRelieveParaZona(1, 1, 93217);
        PlayerPrefs.SetInt("graficos_index", 2);
        Ejecutar(BosqueDecoracionReal.Generar(decorador));
        var limitesNodos = zona.TexturaTerreno.bounds;
        foreach (float x in new[] { limitesNodos.min.x + 4, limitesNodos.max.x - 4 })
        {
            var nodo = new GameObject("Límite de nodos aislado").AddComponent<Nodo>();
            nodo.transform.position = new Vector3(x, limitesNodos.center.y, limitesNodos.center.z);
        }
        var camara = Camera.main;
        var bosque = zona.bosqueardienteContenedorGameObjects;
        var original = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var humoFuente = original.FirstOrDefault(p => p.name == "NieblaFondo");
        bool humoFuenteVisible = humoFuente != null && humoFuente.GetComponent<ParticleSystemRenderer>().enabled;
        var posiciones = original.ToDictionary(p => p, p => p.transform.position);
        var emisiones = original.ToDictionary(p => p, p => p.emission.rateOverTimeMultiplier);
        var espectrales = original.Where(p => p.GetComponent<ParticleSystemRenderer>().sharedMaterial != null
            && p.GetComponent<ParticleSystemRenderer>().sharedMaterial.name.Contains("Espectral")).ToDictionary(p => p, p => EditorJsonUtility.ToJson(p));
        var fuentes = original.Select(p => p.GetComponent<ParticleSystemRenderer>().sharedMaterial).Where(m => m != null).Distinct()
            .ToDictionary(m => m, m => EditorJsonUtility.ToJson(m));
        var materialPrueba = fuentes.Keys.First(m => m.name.Contains("Llama") && !m.name.Contains("Espectral"));
        zona.nedukazalContenedorGameObjects = new GameObject("Otra zona aislada");
        var fuegosAjenos = new[] { perfilPaso.gameObject, zona.nedukazalContenedorGameObjects }.Select(raiz =>
        {
            var go = new GameObject("Llama de otra zona"); go.transform.SetParent(raiz.transform);
            var p = go.AddComponent<ParticleSystem>(); var main = p.main; main.startSize = .3f;
            p.GetComponent<ParticleSystemRenderer>().sharedMaterial = materialPrueba;
            return p;
        }).ToArray();
        cm.hora = 13; sol.SetCampaignHour(13); Simular(6, true);
        Capturar(camara, "dia-antes");
        var rng = UnityEngine.Random.state;
        var malla = zona.TexturaTerreno.GetComponent<MeshFilter>().sharedMesh;
        var vertices = malla.vertices;
        AmbienteBosqueArdiente.Preparar(zona, false);
        Ejecutar(AdornosBosqueArdiente.Generar(zona));
        var ambiente = bosque.GetComponent<AmbienteBosqueArdiente>();
        Check(UnityEngine.Random.state.Equals(rng), "RNG de campaña intacto al preparar");
        Check(original.All(p => p.transform.position == posiciones[p]), "Posiciones de emisores originales intactas");
        Check(original.All(p => p.emission.rateOverTimeMultiplier == emisiones[p]), "Presupuesto de emisión original conservado");
        Check(espectrales.All(p => EditorJsonUtility.ToJson(p.Key) == p.Value), "Emisores espectrales sin cambios");
        Check(fuentes.All(p => EditorJsonUtility.ToJson(p.Key) == p.Value), "Materiales fuente sin modificar");
        Check(fuegosAjenos.All(p => !p.main.startSize3D && p.main.startSize.constant == .3f
            && p.GetComponent<ParticleSystemRenderer>().sharedMaterial == materialPrueba), "No modifica llamas del Paso ni de Nedukazal");
        Check(vertices.SequenceEqual(malla.vertices), "Malla jugable intacta");
        var fuegos = original.Where(p => p.GetComponent<ParticleSystemRenderer>().sharedMaterial != null
            && p.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.name == "Campania/Bosque Ardiente/Fuego").ToArray();
        Check(fuegos.Length > 10, "Llamas regulares profesionalizadas: " + fuegos.Length);
        Check(fuegos.All(p => p.main.startSize3D && p.main.startSizeY.constantMax > p.main.startSizeX.constantMax), "Llamas alargadas, con punta y oscilación");
        Check(fuegos.All(p => p.GetComponent<ParticleSystemRenderer>().shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off), "Llamas sin sombras adicionales");
        var visibles = fuegos.Where(p => p.GetComponent<ParticleSystemRenderer>().enabled).ToArray();
        Check(visibles.Length > 5 && visibles.All(p => p.transform.position.x >= limitesNodos.min.x + 3.5f
            && p.transform.position.x <= limitesNodos.max.x - 3.5f && p.transform.position.z >= limitesNodos.min.z + .8f
            && p.transform.position.z <= limitesNodos.max.z - .8f), "Llamas visibles dentro del terreno y del rango X de los nodos");
        var medidas = fuegos.ToDictionary(p => p, p => p.main.startSizeY.constantMax);
        int cantidad = bosque.GetComponentsInChildren<ParticleSystem>(true).Length;
        AmbienteBosqueArdiente.Preparar(zona, false);
        Check(cantidad == bosque.GetComponentsInChildren<ParticleSystem>(true).Length, "Reaplicar no duplica sistemas");
        Check(medidas.All(p => p.Key.main.startSizeY.constantMax == p.Value), "Reaplicar no agranda las llamas acumulativamente");
        var cache = (IDictionary)typeof(AmbienteBosqueArdiente).GetField("medidasFuego", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ambiente);
        Check(cache.Count == fuegos.Length, "El banco de fondo no se reprocesa como una llama original");
        var fondo = bosque.GetComponentInChildren<FondoBosqueArdiente>();
        Check(fondo.GetComponentsInChildren<MeshRenderer>().Length == 3, "Tres cortinas de humo, sin arbolado lejano");
        Check(fondo.GetComponentsInChildren<Collider>().Length == 0, "Fondo sin colisiones");
        Check(fondo.GetComponentsInChildren<ParticleSystem>().Length == 0, "Sin fila ni emisores de llamas fuera del bosque");
        foreach (string nombre in new[] { "BosqueArdienteFuego", "BosqueArdienteFondo", "BosqueArdienteSuelo", "CaminoSueloRework" })
        {
            var shader = Resources.Load<Shader>(nombre);
            Check(shader != null && shader.isSupported && !ShaderUtil.ShaderHasError(shader), "Shader " + nombre);
        }
        for (int i = 0; i < 360; i++) { ambiente.Avanzar(1f / 60); Simular(1f / 60, false); }
        cm.hora = 13; sol.SetCampaignHour(13); ambiente.Avanzar(1f / 60); Capturar(camara, "dia-despues");
        var posicionDetalle = camara.transform.position; var rotacionDetalle = camara.transform.rotation; float fovDetalle = camara.fieldOfView;
        var fuegoDetalle = visibles.OrderBy(p => Vector3.Distance(p.transform.position, camara.transform.position)).First();
        camara.transform.position = fuegoDetalle.transform.position + new Vector3(2, 2.5f, -4);
        camara.transform.LookAt(fuegoDetalle.transform.position + Vector3.up * .4f); camara.fieldOfView = 46;
        Capturar(camara, "detalle-llamas");
        Directory.CreateDirectory(Path.Combine(Salida, "animacion"));
        for (int i = 0; i < 12; i++)
        {
            ambiente.Avanzar(.1f); Simular(.1f, false);
            Capturar(camara, "animacion/detalle-" + i.ToString("D2"));
        }
        camara.transform.position = posicionDetalle; camara.transform.rotation = rotacionDetalle; camara.fieldOfView = fovDetalle;
        cm.hora = 23; sol.SetCampaignHour(23); ambiente.Avanzar(1f / 60); Capturar(camara, "noche-despues");
        var posicion = camara.transform.position; float fov = camara.fieldOfView;
        camara.transform.position = posicion + Vector3.left * 12; Capturar(camara, "paneo-izquierda");
        camara.transform.position = posicion + Vector3.right * 12; Capturar(camara, "paneo-derecha");
        camara.transform.position = posicion; camara.fieldOfView = fov * 1.18f; Capturar(camara, "zoom-lejano");
        camara.fieldOfView = fov;
        int particulas = fuegos.Sum(p => p.particleCount);
        ambiente.Avanzar(0); Check(fuegos.Sum(p => p.particleCount) == particulas, "La pausa no emite llamas adicionales");
        VerificarCaminos(zona, perfilPaso);
        PlayerPrefs.SetInt("graficos_index", 0); AmbienteBosqueArdiente.Preparar(zona, false);
        var relleno = bosque.transform.Find("Relleno cálido de incendios (Runtime)");
        Check(relleno == null || relleno.GetComponentsInChildren<Light>().Count(l => l.enabled) <= 1,
            "Calidad baja limita el resplandor a una luz");
        bosque.SetActive(false);
        Check(!fondo.gameObject.activeInHierarchy, "Humo e incendio se apagan al salir del bosque");
        Check(UnityEngine.Random.state.Equals(rng), "RNG intacto después de animar y reaplicar");
        var provisional = bosque.GetComponentsInChildren<ParticleSystemRenderer>(true).FirstOrDefault(r => r.name == "NieblaFondo");
        Check(provisional == null || !provisional.enabled, "El humo provisional no tapa las cortinas nuevas");
        // Los callbacks de un MonoBehaviour normal no se ejecutan como en Play dentro de este fixture de editor.
        typeof(FondoBosqueArdiente).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fondo, null);
        UnityEngine.Object.DestroyImmediate(fondo.gameObject);
        Check(provisional == null || provisional.enabled == humoFuenteVisible, "Al retirar el fondo nuevo se restaura el renderer anterior");
    }

    static void VerificarCaminos(AtributosZona zona, MaterialesCaminosZona paso)
    {
        var bosque = zona.ObtenerMaterialesCaminosZona();
        Check(bosque.caminoOriginal == paso.caminoOriginal && bosque.caminoMarcado == paso.caminoMarcado
            && bosque.caminoUsado == paso.caminoUsado && bosque.caminoHint == paso.caminoHint && bosque.caminoAAldea == paso.caminoAAldea,
            "Mismos materiales regionales del Paso para caminos y asentamientos");
        Check(bosque.multiplicadorAnchoBase == paso.multiplicadorAnchoBase, "Mismo multiplicador de ancho del Paso");
        var go = new GameObject("Comparación de camino aislado");
        var linea = go.AddComponent<LineRenderer>(); linea.positionCount = 4; linea.useWorldSpace = false;
        linea.SetPositions(new[] { Vector3.zero, new Vector3(4, 0, 1), new Vector3(8, 0, 1.2f), new Vector3(12, 0, 0) });
        var camino = go.AddComponent<CaminoMesh>();
        typeof(CaminoMesh).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(camino, null);
        foreach (var material in new[] { paso.caminoOriginal, paso.caminoMarcado, paso.caminoUsado, paso.caminoHint, paso.caminoAAldea })
        {
            zona.ID = 2; camino.SetWidth(.6f * paso.multiplicadorAnchoBase); camino.SetMaterial(material, true); camino.RebuildFromLine();
            var renderer = camino.GetMeshRenderer(); var textura = renderer.sharedMaterial.mainTexture;
            var uvEscala = renderer.sharedMaterial.mainTextureScale;
            var malla = go.GetComponent<MeshFilter>().sharedMesh; var puntos = malla.vertices; var uv = malla.uv;
            zona.ID = 1; camino.SetMaterial(material, true); camino.RebuildFromLine();
            Check(renderer.sharedMaterial.mainTexture == textura && renderer.sharedMaterial.mainTextureScale == uvEscala,
                "Misma textura y escala UV: " + material.name);
            Check(puntos.SequenceEqual(malla.vertices) && uv.SequenceEqual(malla.uv), "Misma geometría y repetición: " + material.name);
            Check(renderer.sharedMaterial.GetFloat("_SnowAmount") == 0, "Caminos del bosque sin nieve añadida");
        }
        UnityEngine.Object.DestroyImmediate(go);
    }

    static void Simular(float dt, bool reiniciar)
    {
        foreach (var ps in UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)) ps.Simulate(dt, false, reiniciar);
    }
    static void Capturar(Camera camara, string nombre)
    {
        var anterior = camara.targetTexture; var activo = RenderTexture.active;
        var rt = new RenderTexture(1600, 900, 24); var t = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        try
        {
            camara.targetTexture = rt; camara.Render(); RenderTexture.active = rt;
            t.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); t.Apply(); File.WriteAllBytes(Path.Combine(Salida, nombre + ".png"), t.EncodeToPNG());
        }
        finally
        {
            camara.targetTexture = anterior; RenderTexture.active = activo;
            UnityEngine.Object.DestroyImmediate(t); UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
