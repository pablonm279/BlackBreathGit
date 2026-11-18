using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Comparación sobre Camera.main y los límites reales de EdgePanCameraZ. No guarda partidas ni escenas.
[InitializeOnLoad]
public static class CapturasArmonizacionPaso
{
    const string Clave = "Paso.CapturasArmonizacion";
    const string Salida = "output/paso-armonizacion-integral/";
    static readonly int[] Climas = { 1, 3, 4, 5, 7 };
    static double siguiente;
    static int indice;
    static bool lista;
    static Camera camara;
    static EdgePanCameraZ control;
    static AmbientePasoVientoHelado ambiente;
    static ClimasPasoVientoHelado clima;
    static Vector3 posicion;
    static Quaternion rotacion;
    static float fov;
    static bool oscilacion, auroraActiva;
    static GameObject aurora;
    static Vector3 posicionOriginal;
    static Quaternion rotacionOriginal;
    static float zoomOriginal, panXOriginal, panZOriginal;
    static readonly BindingFlags Privado = BindingFlags.Instance | BindingFlags.NonPublic;

    static CapturasArmonizacionPaso()
    {
        EditorApplication.playModeStateChanged += Cambio;
        EditorApplication.update += Actualizar;
    }

    public static void Iniciar(string fase)
    {
        if (Application.isPlaying) throw new InvalidOperationException("Iniciar desde edición.");
        SessionState.SetString(Clave, fase);
        SessionState.SetInt(Clave + ".indice", 0);
        EditorApplication.isPlaying = true;
    }

    static void Cambio(PlayModeStateChange estado)
    {
        if (SessionState.GetString(Clave, "") == "") return;
        if (estado == PlayModeStateChange.EnteredPlayMode)
        {
            UnityEngine.Random.InitState(17112026);
            indice = 0; lista = false; siguiente = EditorApplication.timeSinceStartup + 12;
        }
    }

    static void Actualizar()
    {
        string fase = SessionState.GetString(Clave, "");
        if (fase == "" || !Application.isPlaying || EditorApplication.isCompiling
            || EditorApplication.timeSinceStartup < siguiente) return;
        try
        {
            indice = SessionState.GetInt(Clave + ".indice", 0);
            if (!lista)
            {
                var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
                if (zona == null || zona.ID != 2) { siguiente += 2; return; }
                var cm = UnityEngine.Object.FindFirstObjectByType<CampaignManager>();
                if (cm == null) throw new Exception("Sin CampaignManager");
                // La compilación de comandos del editor puede reiniciar el singleton durante una inspección.
                if (CampaignManager.Instance == null) typeof(CampaignManager).GetProperty("Instance").SetValue(null, cm);
                bool cerroIntroduccion = false;
                foreach (var boton in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                {
                    var texto = boton.GetComponentInChildren<TMPro.TMP_Text>();
                    if (boton.name == "Continuar" && texto != null)
                    {
                        boton.onClick.Invoke(); cerroIntroduccion = true;
                    }
                }
                if (cerroIntroduccion || cm.IntroCampaniaActivaOPendiente || zona.DecoracionZonaEnCurso)
                {
                    siguiente = EditorApplication.timeSinceStartup + 2; return;
                }
                ambiente = zona.pasovientoheladoContenedorGameObjects.GetComponent<AmbientePasoVientoHelado>();
                clima = ambiente.GetComponent<ClimasPasoVientoHelado>();
                camara = Camera.main; control = camara.GetComponent<EdgePanCameraZ>();
                posicion = (Vector3)Campo("baseLocalPos").GetValue(control);
                rotacion = (Quaternion)Campo("baseLocalRotation").GetValue(control);
                fov = camara.fieldOfView;
                posicionOriginal = camara.transform.localPosition; rotacionOriginal = camara.transform.localRotation;
                oscilacion = (bool)Campo("oscillationEnabled").GetValue(control);
                zoomOriginal = (float)Campo("zoomActual").GetValue(control);
                panXOriginal = (float)Campo("panXActual").GetValue(control);
                panZOriginal = (float)Campo("panZActual").GetValue(control);
                var menu = UnityEngine.Object.FindFirstObjectByType<MenuDescanso>(FindObjectsInactive.Include);
                aurora = menu != null ? menu.climaAuroraBoreal : null;
                if (aurora != null) auroraActiva = aurora.activeSelf;
                control.enabled = false;
                Directory.CreateDirectory(Salida + fase);
                var nodos = UnityEngine.Object.FindObjectsByType<Nodo>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Select(n => n.transform.position.ToString("F4")).OrderBy(n => n);
                File.WriteAllLines(Salida + fase + "/nodos.txt", nodos);
                File.WriteAllText(Salida + fase + "/contexto.txt", "Semilla relieve: " + zona.GetComponent<MapDecorator>().GetReliefSeed()
                    + "\nCámara: " + posicion.ToString("F4") + " / " + rotacion.eulerAngles + " / FOV " + fov
                    + "\nTerreno: " + zona.TexturaTerreno.bounds + " / " + zona.TexturaTerreno.sharedMaterial
                    + "\nExtension: " + zona.TexturaTerrenoExtension.bounds + " / " + zona.TexturaTerrenoExtension.sharedMaterial
                    + "\nVertices: " + zona.TexturaTerreno.GetComponent<MeshFilter>().sharedMesh.vertexCount);
                lista = true; siguiente = EditorApplication.timeSinceStartup + 2;
                PrepararCaso(); return;
            }
            string nombre = Nombre();
            Capturar(Salida + fase + "/" + nombre + ".png");
            // La captura de pantalla incluye el HUD y los canvas superpuestos que Camera.Render no incluye.
            if (indice == 0 || indice == 15) ScreenCapture.CaptureScreenshot(Salida + fase + "/hud-" + nombre + ".png");
            indice++;
            SessionState.SetInt(Clave + ".indice", indice);
            if (indice == 30)
            {
                ambiente.horaVistaPrevia = -1; clima.climaVistaPrevia = -1;
                Campo("oscillationEnabled").SetValue(control, oscilacion);
                Campo("zoomActual").SetValue(control, zoomOriginal);
                Campo("panXActual").SetValue(control, panXOriginal); Campo("panZActual").SetValue(control, panZOriginal);
                control.enabled = true; camara.transform.localPosition = posicionOriginal; camara.transform.localRotation = rotacionOriginal;
                if (aurora != null) aurora.SetActive(auroraActiva);
                var errores = new System.Collections.Generic.List<string>();
                foreach (string nombreShader in new[] { "PasoVientoHeladoVegetacion", "PasoVientoHeladoTerrenoExtension", "CaminoSueloRework", "PasoVientoHeladoClimaParticulas" })
                {
                    var shader = Resources.Load<Shader>(nombreShader);
                    errores.Add(nombreShader + ": " + (shader != null && shader.isSupported && !ShaderUtil.ShaderHasError(shader) ? "OK" : "ERROR"));
                }
                File.WriteAllLines(Salida + fase + "/shaders.txt", errores);
                File.WriteAllText(Salida + fase + "/completo.txt", "30 vistas: 3 cámaras x 2 horas x 5 climas.\n");
                SessionState.SetString(Clave, "");
                Debug.Log("PASO_CAPTURAS_COMPLETAS " + fase); return;
            }
            PrepararCaso(); siguiente = EditorApplication.timeSinceStartup + .8;
        }
        catch (Exception e)
        {
            Directory.CreateDirectory(Salida + fase);
            File.WriteAllText(Salida + fase + "/error.txt", e.ToString());
            SessionState.SetString(Clave, ""); Debug.LogException(e);
            if (control != null) control.enabled = true;
        }
    }

    static FieldInfo Campo(string nombre) => typeof(EdgePanCameraZ).GetField(nombre, Privado);
    static string Nombre() => (indice / 15 == 0 ? "dia" : "noche") + "-clima" + Climas[(indice % 15) / 3] + "-cam" + indice % 3;

    static void PrepararCaso()
    {
        int vista = indice % 3;
        ambiente.horaVistaPrevia = indice / 15 == 0 ? 13 : 23;
        clima.PrepararVistaPrevia(Climas[(indice % 15) / 3], camara);
        if (aurora != null) aurora.SetActive(Climas[(indice % 15) / 3] == 7);
        Campo("zoomActual").SetValue(control, vista == 1 ? -1f : vista == 2 ? .18f : 0f);
        Campo("oscillationEnabled").SetValue(control, false);
        Campo("panXActual").SetValue(control, posicion.x + (vista == 2 ? (float)Campo("maxRightOffsetX").GetValue(control) * 1.1f : 0));
        Campo("panZActual").SetValue(control, posicion.z + (vista == 1 ? 1.2f : 0));
        typeof(EdgePanCameraZ).GetMethod("AplicarZoomVisual", Privado).Invoke(control, null);
        typeof(EdgePanCameraZ).GetMethod("AplicarFinalPosicion", Privado).Invoke(control, null);
        camara.fieldOfView = fov;
    }

    static void Capturar(string ruta)
    {
        var anterior = camara.targetTexture; var activo = RenderTexture.active;
        var rt = new RenderTexture(1280, 720, 24); var textura = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            camara.targetTexture = rt; camara.Render(); RenderTexture.active = rt;
            textura.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); textura.Apply();
            File.WriteAllBytes(ruta, textura.EncodeToPNG());
        }
        finally
        {
            camara.targetTexture = anterior; RenderTexture.active = activo;
            UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(textura);
        }
    }
}
