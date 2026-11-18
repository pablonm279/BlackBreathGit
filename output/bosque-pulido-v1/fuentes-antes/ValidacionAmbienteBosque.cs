using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ValidacionAmbienteBosque
{
    [MenuItem("Tools/Campaña/Bosque Ardiente/Reaplicar ambiente (Play)")]
    public static void Reaplicar()
    {
        var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
        if (!Application.isPlaying || zona == null || zona.ID != 1) return;
        bool conservar = new SerializedObject(zona).FindProperty("usarTexturaProceduralBosqueAngustiante").boolValue;
        if (conservar) zona.ReaplicarTexturaBosqueArdiente();
        AmbienteBosqueArdiente.Preparar(zona, conservar);
    }

    [MenuItem("Tools/Campaña/Bosque Ardiente/Capturar ambiente actual")]
    public static void Inspeccionar()
    {
        var zona = UnityEngine.Object.FindFirstObjectByType<AtributosZona>();
        if (zona == null || zona.ID != 1 || zona.TexturaTerreno == null) return;
        Directory.CreateDirectory("output/ambiente-bosque");
        var texto = new StringBuilder();
        texto.AppendLine("Play: " + Application.isPlaying);
        if (zona != null)
        {
            texto.AppendLine("Zona: " + zona.ID + " / " + zona.gameObject.activeInHierarchy);
            texto.AppendLine("Terreno: " + zona.TexturaTerreno.bounds);
            var origenes = new[] { zona.bosqueardienteContenedorGameObjects, zona.gameObject };
            foreach (var raiz in origenes.Where(g => g != null))
            foreach (var ps in raiz.GetComponentsInChildren<ParticleSystem>(true).Take(35))
            {
                var r = ps.GetComponent<ParticleSystemRenderer>();
                texto.AppendLine(ps.name + " pos=" + ps.transform.position + " escala=" + ps.transform.lossyScale
                    + " material=" + r.sharedMaterial + " size=" + ps.main.startSizeMultiplier
                    + " velocidad=" + ps.main.startSpeedMultiplier + " vida=" + ps.main.startLifetimeMultiplier
                    + " rate=" + ps.emission.rateOverTimeMultiplier + " color=" + ps.main.startColor.color);
            }
        }
        foreach (var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            texto.AppendLine("Camara: " + cam.name + " activa=" + cam.isActiveAndEnabled + " pos=" + cam.transform.position
                + " rot=" + cam.transform.eulerAngles + " fov=" + cam.fieldOfView + " mask=" + cam.cullingMask);
            if (cam.isActiveAndEnabled && cam.cameraType == CameraType.Game)
                Capturar(cam, "actual-" + cam.GetInstanceID());
        }
        File.WriteAllText("output/ambiente-bosque/inspeccion.txt", texto.ToString());
    }

    public static void Capturar(Camera cam, string nombre)
    {
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
            File.WriteAllBytes("output/ambiente-bosque/" + nombre + ".png", imagen.EncodeToPNG());
        }
        finally
        {
            cam.targetTexture = anterior;
            RenderTexture.active = activo;
            UnityEngine.Object.DestroyImmediate(imagen);
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
