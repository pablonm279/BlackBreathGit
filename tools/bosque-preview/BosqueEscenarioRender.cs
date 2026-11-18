using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Cámara, escala, terrenos, luces y tandas de adornos extraídos de la escena fuente.
// Sustituye los gestores por el contexto aislado; no ejecuta ni modifica el tutorial.
public static class BosqueEscenarioRender
{
    static readonly string Salida = Path.GetFullPath("../../output/bosque-pulido-v1/escenario");
    public static void RunTodo() { BosquePulidoRender.Run(); Run(); }
    static void Ejecutar(IEnumerator rutina)
    {
        try { while (rutina.MoveNext()) if (rutina.Current is IEnumerator hija) Ejecutar(hija); }
        finally { (rutina as IDisposable)?.Dispose(); }
    }
    public static void Run()
    {
        Directory.CreateDirectory(Salida);
        EditorSceneManager.OpenScene("Assets/BosqueEscenarioReal.unity");
        var decorador = UnityEngine.Object.FindFirstObjectByType<MapDecorator>();
        var zona = decorador.gameObject.AddComponent<AtributosZona>();
        zona.ID = zona.FASE = 1;
        zona.bosqueardienteContenedorGameObjects = GameObject.Find("ContenedorGameObjectsBosqueArdiente");
        zona.TexturaTerreno = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
            .First(r => r.name.StartsWith("Terreno (0)"));
        zona.TexturaTerrenoExtension = GameObject.Find("Terrenoextenso").GetComponent<MeshRenderer>();
        var cm = new GameObject("Reloj aislado").AddComponent<CampaignManager>();
        CampaignManager.Instance = cm;
        var sol = zona.bosqueardienteContenedorGameObjects.GetComponentInChildren<SunController>();
        typeof(SunController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(sol, null);
        typeof(MapDecorator).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(decorador, null);
        decorador.RegenerarRelieveParaZona(1, 1, 93217);
        PlayerPrefs.SetInt("graficos_index", 2);
        Ejecutar(BosqueDecoracionReal.Generar(decorador));
        var camara = Camera.main;
        if (camara == null) throw new Exception("Falta cámara principal del escenario.");
        // Los controladores de cámara no se ejecutan en esta copia; se conserva su transform serializado.
        camara.enabled = true;
        var contexto = new StringBuilder();
        contexto.AppendLine("Cámara fuente: " + camara.transform.position + " / " + camara.transform.eulerAngles + " / FOV " + camara.fieldOfView);
        contexto.AppendLine("Terreno: " + zona.TexturaTerreno.bounds);
        contexto.AppendLine("Extensión: " + zona.TexturaTerrenoExtension.bounds);
        contexto.AppendLine("Decoraciones originales: " + decorador.transform.childCount);
        File.WriteAllText(Path.Combine(Salida, "contexto.txt"), contexto.ToString());
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var originales = renderers.ToDictionary(r => r, r => r.sharedMaterials);
        cm.hora = 13; sol.SetCampaignHour(13);
        Simular(6);
        AmbienteBosqueArdienteAntes.Preparar(zona, false);
        var viejo = zona.bosqueardienteContenedorGameObjects.GetComponent<AmbienteBosqueArdienteAntes>();
        for (int i = 0; i < 240; i++) { viejo.Avanzar(1f / 60); Simular(1f / 60, false); }
        Capturar(camara, "dia-antes");
        cm.hora = 23; sol.SetCampaignHour(23);
        Capturar(camara, "noche-antes");
        foreach (var par in originales) if (par.Key != null) par.Key.sharedMaterials = par.Value;
        var particulasViejas = zona.bosqueardienteContenedorGameObjects.transform.Find("Atmosfera Bosque Ardiente (Runtime)");
        if (particulasViejas != null) UnityEngine.Object.DestroyImmediate(particulasViejas.gameObject);
        UnityEngine.Object.DestroyImmediate(viejo);
        var estadoRng = UnityEngine.Random.state;
        AmbienteBosqueArdiente.Preparar(zona, false);
        Ejecutar(AdornosBosqueArdiente.Generar(zona));
        var ambiente = zona.bosqueardienteContenedorGameObjects.GetComponent<AmbienteBosqueArdiente>();
        for (int i = 0; i < 240; i++) { ambiente.Avanzar(1f / 60); Simular(1f / 60, false); }
        cm.hora = 13; sol.SetCampaignHour(13); ambiente.Avanzar(1f / 60);
        Capturar(camara, "dia-despues");
        cm.hora = 23; sol.SetCampaignHour(23); ambiente.Avanzar(1f / 60);
        Capturar(camara, "noche-despues");
        var posicion = camara.transform.position; var rotacion = camara.transform.rotation; float fov = camara.fieldOfView;
        foreach (float offset in new[] { -10f, 10f })
        {
            camara.transform.position = posicion + Vector3.right * offset;
            Capturar(camara, offset < 0 ? "paneo-izquierda" : "paneo-derecha");
        }
        camara.transform.position = posicion;
        camara.fieldOfView = fov * 1.18f;
        Capturar(camara, "zoom-lejano");
        camara.fieldOfView = fov * .78f;
        Capturar(camara, "zoom-cercano");
        camara.fieldOfView = fov; camara.transform.rotation = rotacion;
        var generador = zona.bosqueardienteContenedorGameObjects.GetComponent<AdornosBosqueArdiente>();
        contexto.AppendLine("Adornos nuevos: " + generador.CantidadAdornos);
        contexto.AppendLine("RNG de campaña intacto: " + estadoRng.Equals(UnityEngine.Random.state));
        File.WriteAllText(Path.Combine(Salida, "contexto.txt"), contexto.ToString());
        foreach (string nombre in new[] { "BosqueArdienteSuelo", "BosqueArdienteAmbiente", "BosqueArdienteLlamas", "BosqueArdienteMadera", "BosqueArdienteFondo" })
            if (ShaderUtil.ShaderHasError(Resources.Load<Shader>(nombre))) throw new Exception("Shader inválido: " + nombre);
        Debug.Log("BOSQUE_ESCENARIO_COMPLETO\n" + contexto);
    }
    static void Simular(float dt, bool reiniciar = true)
    {
        foreach (var ps in UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            ps.Simulate(dt, false, reiniciar);
    }
    static void Capturar(Camera camara, string nombre)
    {
        var anterior = camara.targetTexture; var activo = RenderTexture.active;
        var rt = new RenderTexture(1600, 900, 24); var textura = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        try
        {
            camara.targetTexture = rt; camara.Render(); RenderTexture.active = rt;
            textura.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); textura.Apply();
            File.WriteAllBytes(Path.Combine(Salida, nombre + ".png"), textura.EncodeToPNG());
        }
        finally
        {
            camara.targetTexture = anterior; RenderTexture.active = activo;
            UnityEngine.Object.DestroyImmediate(textura); UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
