using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

// Pruebas de integración visual en el proyecto aislado, sin cargar partidas ni ejecutar el tutorial.
public static class BosquePulidoRender
{
    static readonly string Salida = Path.GetFullPath("../../output/bosque-pulido-v1");
    static readonly BindingFlags Privado = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly StringBuilder log = new StringBuilder();
    static void Check(bool condicion, string descripcion)
    {
        if (!condicion) throw new Exception(descripcion);
        log.AppendLine("OK " + descripcion);
    }
    static void Ejecutar(IEnumerator rutina)
    {
        try { while (rutina.MoveNext()) if (rutina.Current is IEnumerator hija) Ejecutar(hija); }
        finally { (rutina as IDisposable)?.Dispose(); }
    }

    public static void Run()
    {
        Directory.CreateDirectory(Salida);
        try { Verificar(); }
        finally { File.WriteAllText(Path.Combine(Salida, "validacion-pulido.txt"), log.ToString()); }
        Debug.Log(log.ToString());
    }

    static void Verificar()
    {
        BosqueRender.Run(); // Conservación de las llamas, RNG, humo original, pausa y LOD.
        foreach (string nombre in new[] { "BosqueArdienteSuelo", "BosqueArdienteAmbiente", "BosqueArdienteMadera", "BosqueArdienteFondo" })
        {
            var shader = Resources.Load<Shader>(nombre);
            Check(shader != null && shader.isSupported && !ShaderUtil.ShaderHasError(shader), "Shader " + nombre);
        }
        var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
        var bosque = zona.bosqueardienteContenedorGameObjects;
        bosque.SetActive(true);
        var ambiente = bosque.GetComponent<AmbienteBosqueArdiente>();
        var decorador = zona.GetComponent<MapDecorator>();
        var cm = new GameObject("Reloj aislado").AddComponent<CampaignManager>();
        CampaignManager.Instance = cm;
        var caminos = UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(l => l.name.StartsWith("Camino prueba")).ToArray();
        foreach (var camino in caminos) camino.gameObject.AddComponent<CaminoMesh>();
        var nodo = GameObject.Find("Nodo prueba").AddComponent<Nodo>();
        var estadoRng = UnityEngine.Random.state;
        var mallaSuelo = zona.TexturaTerreno.GetComponent<MeshFilter>().sharedMesh;
        var verticesSuelo = mallaSuelo.vertices;
        PlayerPrefs.SetInt("graficos_index", 2);
        AmbienteBosqueArdiente.Preparar(zona, false);
        var fondo = bosque.GetComponentInChildren<FondoBosqueArdiente>();
        Check(fondo != null && fondo.GetComponentsInChildren<MeshRenderer>().Length == 4, "Fondo con tres distancias y humo, en cuatro renderers");
        Ejecutar(AdornosBosqueArdiente.Generar(zona));
        var adornos = bosque.GetComponent<AdornosBosqueArdiente>();
        Check(adornos != null && adornos.CantidadAdornos >= 12 && adornos.CantidadAdornos <= 72, "Cantidad acotada de adornos: " + adornos.CantidadAdornos);
        var raiz = bosque.transform.Find("Troncos, tocones y ramas quemadas (Runtime)");
        Check(raiz.GetComponentsInChildren<Collider>().Length == 0, "Adornos sin colliders ni interacción");
        Check(fondo.GetComponentsInChildren<Collider>().Length == 0, "Fondo sin colliders");
        Check(UnityEngine.Random.state.Equals(estadoRng), "RNG de campaña intacto al construir fondo y adornos");
        Check(mallaSuelo == zona.TexturaTerreno.GetComponent<MeshFilter>().sharedMesh && verticesSuelo.SequenceEqual(mallaSuelo.vertices), "Malla jugable intacta");
        foreach (var renderer in raiz.GetComponentsInChildren<MeshRenderer>())
        {
            foreach (var camino in caminos)
            {
                bool cercaTramo = camino.name.EndsWith("0")
                    ? renderer.bounds.max.x >= -25 && renderer.bounds.min.x <= 25
                    : renderer.bounds.max.z >= -25 && renderer.bounds.min.z <= 25;
                float limite = camino.name.EndsWith("0") ? Mathf.Abs(renderer.bounds.center.z) - renderer.bounds.extents.z
                    : Mathf.Abs(renderer.bounds.center.x - 10) - renderer.bounds.extents.x;
                Check(!cercaTramo || limite > .45f, "Huella libre de camino " + (camino.gameObject.activeSelf ? "visible" : "oculto"));
            }
            Check(Vector2.Distance(new Vector2(renderer.bounds.center.x, renderer.bounds.center.z), new Vector2(nodo.transform.position.x, nodo.transform.position.z))
                > renderer.bounds.extents.magnitude + 1, "Huella libre de nodo");
        }
        var posiciones = raiz.Cast<Transform>().Select(t => t.position).ToArray();
        Ejecutar(AdornosBosqueArdiente.Generar(zona));
        raiz = bosque.transform.Find("Troncos, tocones y ramas quemadas (Runtime)");
        Check(posiciones.SequenceEqual(raiz.Cast<Transform>().Select(t => t.position)), "Regeneración determinista sin duplicar adornos");
        Check(bosque.GetComponentsInChildren<FondoBosqueArdiente>().Length == 1, "Fondo único al reaplicar");
        var relleno = bosque.transform.Find("Relleno cálido de incendios (Runtime)");
        Check(relleno.GetComponentsInChildren<Light>().Length <= 4, "Presupuesto máximo de cuatro luces sin sombras");
        Check(relleno.GetComponentsInChildren<Light>().All(l => l.shadows == LightShadows.None), "Luces nuevas sin coste de sombras");

        var camara = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).First();
        camara.transform.position = new Vector3(10, 12, -17); camara.transform.LookAt(Vector3.zero);
        var sol = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).First(l => l.type == LightType.Directional);
        sol.intensity = 3.65f * 1.665f; sol.color = new Color(.8301887f, .78991514f, .5991456f);
        RenderSettings.ambientLight = new Color(.24f, .22f, .2f);
        cm.hora = 13; ambiente.Avanzar(1f / 60);
        var nuevo = zona.TexturaTerreno.sharedMaterial;
        var antes = new Material(Resources.Load<Shader>("BosqueArdienteSueloAntes"));
        antes.CopyPropertiesFromMaterial(nuevo);
        var luces = relleno.GetComponentsInChildren<Light>();
        zona.TexturaTerreno.sharedMaterial = antes; raiz.gameObject.SetActive(false); fondo.gameObject.SetActive(false);
        foreach (var luz in luces) luz.enabled = false;
        Capturar(camara, "dia-antes");
        zona.TexturaTerreno.sharedMaterial = nuevo; raiz.gameObject.SetActive(true); fondo.gameObject.SetActive(true);
        foreach (var luz in luces) luz.enabled = true;
        Capturar(camara, "dia-despues");
        camara.transform.position = new Vector3(24, 28, -38); camara.transform.LookAt(new Vector3(0, 0, 7));
        Capturar(camara, "panorama-dia");
        cm.hora = 23; ambiente.Avanzar(1f / 60);
        sol.intensity = 3.65f * 1.1f; sol.color = Color.Lerp(new Color(.5943396f, .378471f, .41854542f), new Color(.49f, .53f, .59f), .18f);
        RenderSettings.ambientLight = Color.Lerp(new Color(.7264151f, .5036935f, .50853527f), new Color(.48f, .46f, .43f), .18f);
        Capturar(camara, "panorama-noche");
        var detalle = raiz.GetChild(0);
        camara.transform.position = detalle.position + new Vector3(2, 3, -4); camara.transform.LookAt(detalle.position);
        Capturar(camara, "detalle-madera");
        UnityEngine.Object.DestroyImmediate(antes);

        var objetivo = raiz.GetChild(0); var rutaNueva = new GameObject("Camino revelado de control").AddComponent<LineRenderer>();
        rutaNueva.positionCount = 2; rutaNueva.useWorldSpace = true;
        rutaNueva.SetPositions(new[] { objetivo.position + Vector3.left * 4, objetivo.position + Vector3.right * 4 });
        decorador.OcultarDecoracionRemovibleSobreCamino(rutaNueva, true);
        Check(!objetivo.gameObject.activeSelf, "Un camino nuevo retira el adorno que invade su huella");
        PlayerPrefs.SetInt("graficos_index", 0);
        Ejecutar(AdornosBosqueArdiente.Generar(zona));
        Check(adornos.CantidadAdornos < posiciones.Length, "Calidad baja reduce la decoración");
        AmbienteBosqueArdiente.Preparar(zona, false);
        Check(relleno.GetComponentsInChildren<Light>().Count(l => l.enabled) <= 1, "Calidad baja limita el relleno a una luz");
        int cantidadAntes = adornos.CantidadAdornos;
        zona.ID = 2;
        Ejecutar(AdornosBosqueArdiente.Generar(zona));
        Check(adornos.CantidadAdornos == cantidadAntes, "El generador no modifica otras zonas");
        zona.ID = 1;
        bosque.SetActive(false);
        Check(!fondo.gameObject.activeInHierarchy && bosque.GetComponentsInChildren<Light>(true).All(l => !l.isActiveAndEnabled), "Fondo y luces se apagan al salir del bosque");
        Check(UnityEngine.Random.state.Equals(estadoRng), "RNG intacto después de animar y regenerar");
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
