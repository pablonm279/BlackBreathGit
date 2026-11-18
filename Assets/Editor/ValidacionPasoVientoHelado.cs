using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Vista de control sobre la escena real, sin iniciar campaña ni guardar la escena.
public static class ValidacionPasoVientoHelado
{
    const string Salida = "outputs/paso-viento-helado";

    [MenuItem("Tools/Campaña/Paso Viento Helado/Reaplicar armonía visual (Play)")]
    public static void ReaplicarArmonia()
    {
        var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
        if (!Application.isPlaying || zona == null || zona.ID != 2) return;
        AmbientePasoVientoHelado.Preparar(zona);
        var ambiente = zona.pasovientoheladoContenedorGameObjects.GetComponent<AmbientePasoVientoHelado>();
        var decorador = zona.GetComponent<MapDecorator>();
        if (decorador != null)
        {
            var nombres = new System.Collections.Generic.HashSet<string>();
            foreach (var campo in typeof(AtributosZona).GetFields())
            {
                if (campo.FieldType != typeof(GameObject) || !(campo.Name.StartsWith("PasoVientoHelado_")
                    || campo.Name == "BosqueAngustiante_ArbolQuemado1" || campo.Name == "BosqueAngustiante_ManchaCeniza1")) continue;
                var prefab = campo.GetValue(zona) as GameObject;
                if (prefab != null) nombres.Add(prefab.name + "(Clone)");
            }
            foreach (Transform hijo in decorador.transform)
                if (nombres.Contains(hijo.name)) ambiente.ArmonizarDecoracion(hijo.gameObject);
        }
        ambiente.AjustarApoyoSombras();
        foreach (var camino in UnityEngine.Object.FindObjectsByType<CaminoMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (camino.gameObject.scene == zona.gameObject.scene) camino.ReaplicarVisual();
        zona.StartCoroutine(AdornosPasoVientoHelado.Generar(zona));
    }

    [MenuItem("Tools/Campaña/Paso Viento Helado/Recrear adornos (Play)")]
    public static void RecrearAdornos()
    {
        var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
        if (Application.isPlaying && zona != null && zona.ID == 2)
            zona.StartCoroutine(AdornosPasoVientoHelado.Generar(zona));
    }

    [MenuItem("Tools/Campaña/Paso Viento Helado/Capturar ambiente actual (Play)")]
    public static void CapturarActual()
    {
        var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
        if (!Application.isPlaying || zona == null || zona.ID != 2) return;
        Directory.CreateDirectory(Salida);
        var log = new StringBuilder();
        foreach (string nombre in new[] { "CaminoSueloRework", "PasoVientoHeladoAmbiente", "PasoVientoHeladoDetalles" })
        {
            var shader = Resources.Load<Shader>(nombre);
            if (shader == null || !shader.isSupported || ShaderUtil.ShaderHasError(shader))
                throw new Exception("Shader del Paso no disponible: " + nombre);
        }
        float hora = CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerHoraActual() : 8f;
        log.AppendLine("Hora: " + hora);
        log.AppendLine("Suelo: " + zona.TexturaTerreno.sharedMaterial);
        var adornos = zona.pasovientoheladoContenedorGameObjects.GetComponent<AdornosPasoVientoHelado>();
        log.AppendLine("Adornos nuevos: " + (adornos != null ? adornos.CantidadAdornos : 0));
        if (adornos != null && adornos.GetComponentsInChildren<Collider>(true).Any(c => c.transform.IsChildOf(
            adornos.transform.Find("Ventisqueros, hielo y hitos del Paso (Runtime)"))))
            throw new Exception("La decoración nueva no debe agregar colisiones al mapa.");
        foreach (var camino in UnityEngine.Object.FindObjectsByType<CaminoMesh>(FindObjectsSortMode.None))
        {
            var renderer = camino.GetMeshRenderer();
            if (renderer != null && renderer.enabled)
                log.AppendLine("Camino visible: " + camino.name + " / " + renderer.sharedMaterial);
        }
        foreach (var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (!cam.isActiveAndEnabled || cam.cameraType != CameraType.Game) continue;
            var anterior = cam.targetTexture;
            var activo = RenderTexture.active;
            var rt = new RenderTexture(1600, 900, 24);
            var imagen = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                imagen.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                imagen.Apply();
                File.WriteAllBytes(Salida + "/actual-" + cam.GetInstanceID() + ".png", imagen.EncodeToPNG());
            }
            finally
            {
                cam.targetTexture = anterior;
                RenderTexture.active = activo;
                UnityEngine.Object.DestroyImmediate(imagen);
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        File.WriteAllText(Salida + "/validacion-actual.txt", log.ToString());
    }

    // Prueba aislada de materiales y apoyo con los prefabs reales. Solo para ejecución batch.
    public static void ValidarArmonia()
    {
        Directory.CreateDirectory(Salida);
        var log = new StringBuilder();
        var anterior = CampaignManager.Instance;
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            var contexto = new GameObject("Contexto de validación del Paso");
            contexto.transform.localScale = new Vector3(2.068544f,1f,5.7770762f);
            contexto.transform.rotation = Quaternion.Euler(0,90,0);
            var zona = contexto.AddComponent<AtributosZona>(); zona.ID = 2; zona.FASE = 1;
            var decorador = contexto.AddComponent<MapDecorator>();
            var terreno = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terreno.transform.localScale = new Vector3(4,1,4);
            zona.TexturaTerreno = terreno.GetComponent<MeshRenderer>();
            zona.TexturaTerreno.sharedMaterial = UnityEngine.Object.Instantiate(Resources.Load<Material>("ObjetosMapa/MatPasoVientoHeladoSuelo"));
            var textura = TexturaProceduralSueloZona.CrearTexturaSuelo(EncounterZoneType.PasoVientoHelado, 137,
                new TexturaProceduralPasoVientoHeladoConfig { tamanoTextura = 256 });
            TexturaProceduralSueloZona.AplicarTexturaSueloZona(zona.TexturaTerreno, textura, Color.white);
            var campos = new SerializedObject(decorador);
            campos.FindProperty("planeMesh").objectReferenceValue = terreno.GetComponent<MeshFilter>();
            campos.FindProperty("autoBuscarSectoresTerreno").boolValue = false;
            campos.ApplyModifiedPropertiesWithoutUndo();
            decorador.RegenerarRelieveParaZona(2,1,137);
            zona.pasovientoheladoContenedorGameObjects = new GameObject("Paso de prueba");
            var campaña = new GameObject("Reloj de prueba").AddComponent<CampaignManager>(); campaña.scAtributosZona = zona;
            typeof(CampaignManager).GetProperty("Instance").SetValue(null, campaña);
            var luz = new GameObject("Sol de prueba").AddComponent<Light>(); luz.type = LightType.Directional;
            luz.transform.rotation = Quaternion.Euler(50,35,0); luz.intensity = 3;
            luz.transform.SetParent(zona.pasovientoheladoContenedorGameObjects.transform);
            var sol = luz.gameObject.AddComponent<SunController>(); sol.sun = luz; sol.Fijo = true;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            var fijo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fijo.name = "Elemento fijo de prueba"; fijo.transform.position = new Vector3(6,.6f,2);
            fijo.transform.SetParent(zona.pasovientoheladoContenedorGameObjects.transform,true);
            var rendererFijo = fijo.GetComponent<MeshRenderer>();
            rendererFijo.sharedMaterial = Resources.Load<Material>("ObjetosMapa/MatPasoVientoHeladoSuelo");
            var fuenteFija = rendererFijo.sharedMaterial;
            string originalFijo = EditorJsonUtility.ToJson(fuenteFija);
            var fuente = zona.TexturaTerreno.sharedMaterial;
            string original = EditorJsonUtility.ToJson(fuente);
            var estadoRng = UnityEngine.Random.state;
            AmbientePasoVientoHelado.Preparar(zona);
            var ambiente = zona.pasovientoheladoContenedorGameObjects.GetComponent<AmbientePasoVientoHelado>();
            Comprobar(original == EditorJsonUtility.ToJson(fuente), "Material del suelo original intacto", log);
            Comprobar(rendererFijo.sharedMaterial != fuenteFija, "Elemento fijo armonizado", log);
            Comprobar(EditorJsonUtility.ToJson(fuenteFija) == originalFijo, "Material original del elemento fijo intacto", log);
            Comprobar(UnityEngine.Random.state.Equals(estadoRng), "RNG de campaña intacto al preparar", log);
            Comprobar(zona.TexturaTerreno.sharedMaterial.GetFloat("_Metallic") == 0, "Nieve sin brillo metálico", log);
            string[] prefabs = { "PasoVientoHelado - Arbol1", "PasoVientoHelado - Piedra 3", "PasoVientoHelado - Arbol2" };
            for (int i = 0; i < prefabs.Length; i++)
            {
                var prefab = Resources.Load<GameObject>("ObjetosMapa/" + prefabs[i]);
                Comprobar(prefab != null, "Prefab disponible: " + prefabs[i], log);
                var objeto = UnityEngine.Object.Instantiate(prefab, new Vector3(i*4-4,0,i%2*3), Quaternion.identity, contexto.transform);
                typeof(MapDecorator).GetMethod("ConvertirSombrasBlobCanvas", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(decorador, new object[] { objeto });
                var originales = objeto.GetComponentsInChildren<MeshRenderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToDictionary(m => m, m => EditorJsonUtility.ToJson(m));
                int cantidad = objeto.GetComponentsInChildren<Renderer>(true).Length;
                estadoRng = UnityEngine.Random.state;
                ambiente.ArmonizarDecoracion(objeto);
                Comprobar(originales.All(par => EditorJsonUtility.ToJson(par.Key) == par.Value), "Materiales del prefab intactos: " + prefabs[i], log);
                Comprobar(cantidad == objeto.GetComponentsInChildren<Renderer>(true).Length, "Sin nuevos renderers: " + prefabs[i], log);
                var primera = objeto.GetComponentsInChildren<MeshRenderer>(true).SelectMany(r => r.sharedMaterials).ToArray();
                ambiente.ArmonizarDecoracion(objeto);
                Comprobar(primera.SequenceEqual(objeto.GetComponentsInChildren<MeshRenderer>(true).SelectMany(r => r.sharedMaterials)), "Reaplicar no duplica materiales: " + prefabs[i], log);
                Comprobar(UnityEngine.Random.state.Equals(estadoRng), "RNG intacto al armonizar: " + prefabs[i], log);
            }
            ambiente.AjustarApoyoSombras();
            foreach (var sprite in contexto.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sprite.sprite == null || sprite.sprite.name != "Shadow_Blob_1024") continue;
                decorador.TrySampleSurface(sprite.transform.position, out var suelo, out var normal);
                Comprobar(Vector3.Distance(sprite.transform.position, suelo) < .025f, "Sombra apoyada en relieve", log);
                Comprobar(Mathf.Abs(Vector3.Dot(sprite.transform.forward, normal)) > .99f, "Sombra alineada con el suelo", log);
                Comprobar(sprite.color.a <= .25f, "Sombra de apoyo moderada", log);
            }
            var linea = new GameObject("Camino de prueba").AddComponent<LineRenderer>();
            linea.positionCount = 17; linea.useWorldSpace = true;
            for (int i = 0; i < 17; i++)
            {
                var punto = new Vector3(i-8,0,-3 + Mathf.Sin(i*.3f)*.6f);
                decorador.TrySampleSurface(punto, out punto, out _); linea.SetPosition(i,punto);
            }
            var camino = linea.gameObject.AddComponent<CaminoMesh>();
            typeof(CaminoMesh).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(camino,null);
            camino.SetMaterial(Resources.Load<Material>("Imagenes/Materials/CaminoBase_Paso"),true); camino.RebuildFromLine();
            Comprobar(camino.GetMeshRenderer().sharedMaterial.GetFloat("_SnowAmount") > .8f, "Camino integrado en nieve", log);
            estadoRng = UnityEngine.Random.state;
            EjecutarRutinaPrueba(AdornosPasoVientoHelado.Generar(zona));
            var adornosPrueba = zona.pasovientoheladoContenedorGameObjects.GetComponent<AdornosPasoVientoHelado>();
            Comprobar(adornosPrueba != null && adornosPrueba.CantidadAdornos > 0, "Adornos existentes disponibles para comparar paleta", log);
            Comprobar(adornosPrueba.transform.Find("Ventisqueros, hielo y hitos del Paso (Runtime)").GetComponentsInChildren<Collider>(true).Length == 0, "Adornos sin colisiones", log);
            Comprobar(UnityEngine.Random.state.Equals(estadoRng), "RNG intacto al generar adornos", log);
            Shader.SetGlobalFloat("_CaminoVisionActiva",0);
            var cam = new GameObject("Camara de prueba").AddComponent<Camera>(); cam.tag = "MainCamera";
            cam.transform.position = new Vector3(11,14,-18); cam.transform.LookAt(Vector3.zero); cam.fieldOfView = 43;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.07f,.10f,.14f);
            cam.depthTextureMode = DepthTextureMode.Depth;
            foreach (float hora in new[] { 8f,13f,23f })
            {
                sol.SetCampaignHour(hora);
                CapturarVistaPrueba(cam, "armonia-" + hora);
            }
            zona.ID = 1;
            var materialPaso = zona.TexturaTerreno.sharedMaterial;
            AmbientePasoVientoHelado.Preparar(zona);
            Comprobar(zona.TexturaTerreno.sharedMaterial == materialPaso, "Zona de tutorial fuera del ajuste", log);
            foreach (string nombre in new[] { "CaminoSueloRework", "PasoVientoHeladoDetalles", "PasoVientoHeladoAmbiente", "NieblaGuerraCaravanaPasoVientoHelado" })
            {
                var shader = Resources.Load<Shader>(nombre);
                Comprobar(shader != null && !ShaderUtil.ShaderHasError(shader), "Shader sin errores: " + nombre, log);
            }
            File.WriteAllText(Salida + "/validacion-armonia.txt", log.ToString());
            Debug.Log("PASO_ARMONIA_OK");
        }
        finally { typeof(CampaignManager).GetProperty("Instance").SetValue(null, anterior); }
    }

    static void EjecutarRutinaPrueba(System.Collections.IEnumerator rutina)
    {
        try { while (rutina.MoveNext()) if (rutina.Current is System.Collections.IEnumerator hija) EjecutarRutinaPrueba(hija); }
        finally { (rutina as IDisposable)?.Dispose(); }
    }

    static void Comprobar(bool condicion, string texto, StringBuilder log)
    {
        if (!condicion) throw new Exception(texto);
        log.AppendLine("OK " + texto);
    }

    static void CapturarVistaPrueba(Camera cam, string nombre)
    {
        var anterior = cam.targetTexture; var activo = RenderTexture.active;
        var rt = new RenderTexture(1200,750,24); var imagen = new Texture2D(1200,750,TextureFormat.RGB24,false);
        try
        {
            cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
            imagen.ReadPixels(new Rect(0,0,1200,750),0,0); imagen.Apply();
            File.WriteAllBytes(Salida + "/" + nombre + ".png",imagen.EncodeToPNG());
        }
        finally
        {
            cam.targetTexture = anterior; RenderTexture.active = activo;
            UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(imagen);
        }
    }

    public static void Inspeccionar()
    {
        Directory.CreateDirectory(Salida);
        EditorSceneManager.OpenScene("Assets/Scenes/ES-Campaña.unity");
        var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
        var log = new StringBuilder();
        log.AppendLine("Terreno " + zona.TexturaTerreno.bounds);
        log.AppendLine("Extension " + zona.TexturaTerrenoExtension.bounds);
        foreach (var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            log.AppendLine("Camara " + cam.name + " " + cam.transform.position + " rot " + cam.transform.eulerAngles + " fov " + cam.fieldOfView);
        foreach (var luz in zona.pasovientoheladoContenedorGameObjects.GetComponentsInChildren<Light>(true))
            log.AppendLine("Luz " + luz.name + " " + luz.type + " intensidad " + luz.intensity + " color " + luz.color + " rot " + luz.transform.eulerAngles);
        foreach (var ps in zona.pasovientoheladoContenedorGameObjects.GetComponentsInChildren<ParticleSystem>(true))
            log.AppendLine("Particulas " + ps.name + " posicion " + ps.transform.position + " escala " + ps.transform.lossyScale + " tasa " + ps.emission.rateOverTimeMultiplier + " tam " + ps.main.startSizeMultiplier + " vida " + ps.main.startLifetimeMultiplier);
        var caminos = zona.pasovientoheladoContenedorGameObjects.GetComponent<MaterialesCaminosZona>();
        if (caminos != null) log.AppendLine(EditorJsonUtility.ToJson(caminos, true));
        File.WriteAllText(Salida + "/inspeccion.txt", log.ToString());
        Debug.Log("PASO_INSPECCION_OK");
    }
}
