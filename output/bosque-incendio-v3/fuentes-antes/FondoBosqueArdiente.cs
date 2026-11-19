using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Fondo cubierto por humo de incendio, sin terreno ni colisiones adicionales.
[DisallowMultipleComponent]
public sealed class FondoBosqueArdiente : MonoBehaviour
{
    readonly List<Mesh> mallas = new List<Mesh>();
    readonly List<Material> materiales = new List<Material>();
    readonly List<Vector3> frentes = new List<Vector3>();
    ParticleSystem incendio;
    ParticleSystemRenderer humoProvisional;
    bool humoProvisionalVisible;
    System.Random rng;
    float relojAnterior, acumulador, factorCalidad;
    int siguienteFrente;

    public static FondoBosqueArdiente Preparar(AtributosZona zona, Transform padre)
    {
        if (zona == null || zona.ID != 1 || zona.TexturaTerreno == null) return null;
        var humo = Resources.Load<Shader>("BosqueArdienteFondo");
        var fuego = Resources.Load<Shader>("BosqueArdienteFuego");
        if (humo == null || fuego == null) return null;
        var fondo = padre.GetComponentInChildren<FondoBosqueArdiente>(true);
        if (fondo == null)
        {
            var go = new GameObject("Muro de humo e incendio del bosque (Runtime)") { layer = zona.TexturaTerreno.gameObject.layer };
            go.transform.SetParent(padre, true);
            fondo = go.AddComponent<FondoBosqueArdiente>();
        }
        fondo.Construir(zona, humo, fuego);
        return fondo;
    }

    void Construir(AtributosZona zona, Shader shaderHumo, Shader shaderFuego)
    {
        Limpiar();
        if (humoProvisional == null)
        {
            foreach (var renderer in zona.bosqueardienteContenedorGameObjects.GetComponentsInChildren<ParticleSystemRenderer>(true))
                if (renderer.name == "NieblaFondo")
                {
                    humoProvisional = renderer; humoProvisionalVisible = renderer.enabled;
                    break;
                }
        }
        if (humoProvisional != null) humoProvisional.enabled = false;
        var bounds = zona.TexturaTerreno.bounds;
        transform.position = new Vector3(bounds.center.x, bounds.min.y, bounds.max.z + 1.5f);
        rng = new System.Random(6911 + zona.FASE * 71);
        int calidad = Mathf.Clamp(PlayerPrefs.GetInt("graficos_index", QualitySettings.GetQualityLevel()), 0, 2);
        factorCalidad = calidad == 0 ? .45f : calidad == 1 ? .7f : 1;
        float ancho = Mathf.Max(bounds.size.x * 1.7f, 90);
        for (int capa = 0; capa < 3; capa++)
        {
            var material = new Material(shaderHumo) { name = "Pared de humo de incendio " + capa, hideFlags = HideFlags.DontSave };
            material.SetFloat("_Capa", capa);
            material.SetColor("_Color", new Color(.037f + capa * .010f, .033f + capa * .008f, .030f + capa * .006f, .96f));
            materiales.Add(material);
            var mesh = new Mesh { name = "Cortina de humo " + capa, hideFlags = HideFlags.DontSave };
            float z = 3 + capa * 7, pie = -6 - capa * 2, alto = 70 + capa * 8;
            mesh.vertices = new[] { new Vector3(-ancho, pie, z), new Vector3(ancho, pie, z),
                new Vector3(-ancho, pie + alto, z), new Vector3(ancho, pie + alto, z) };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
            mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 }; mesh.RecalculateBounds();
            mallas.Add(mesh);
            var go = new GameObject("Humo denso del frente " + capa) { layer = gameObject.layer };
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
        }
        var fuego = new Material(shaderFuego) { name = "Llamas del incendio lejano (Runtime)", hideFlags = HideFlags.DontSave };
        fuego.SetColor("_Color", Color.white); fuego.SetFloat("_Intensidad", 1.05f); materiales.Add(fuego);
        var llamas = new GameObject("Lenguas de fuego del fondo") { layer = gameObject.layer };
        llamas.transform.SetParent(transform, false);
        incendio = llamas.AddComponent<ParticleSystem>();
        incendio.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        incendio.useAutoRandomSeed = false; incendio.randomSeed = 11931;
        var main = incendio.main; main.playOnAwake = false; main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World; main.scalingMode = ParticleSystemScalingMode.Shape;
        main.maxParticles = Mathf.RoundToInt(420 * factorCalidad);
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.1f);
        main.startSpeed = 0; main.startSize3D = true;
        main.startSizeX = new ParticleSystem.MinMaxCurve(.75f, 1.5f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(1.25f, 2.6f);
        main.startSizeZ = 1;
        main.startRotation = new ParticleSystem.MinMaxCurve(-.07f, .07f);
        var emission = incendio.emission; emission.enabled = false;
        var shape = incendio.shape; shape.enabled = false;
        var velocity = incendio.velocityOverLifetime; velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-.14f, -.04f);
        velocity.y = new ParticleSystem.MinMaxCurve(.25f, .65f);
        velocity.z = new ParticleSystem.MinMaxCurve(0, .06f);
        var color = incendio.colorOverLifetime; color.enabled = true; color.color = GradienteFuego();
        var size = incendio.sizeOverLifetime; size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .65f), new Keyframe(.25f, 1), new Keyframe(1, .12f)));
        var r = incendio.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = fuego;
        r.renderMode = ParticleSystemRenderMode.Billboard; r.alignment = ParticleSystemRenderSpace.View;
        r.shadowCastingMode = ShadowCastingMode.Off; r.receiveShadows = false;
        var decorador = zona.GetComponent<MapDecorator>();
        int cantidad = Mathf.Clamp(Mathf.RoundToInt(bounds.size.x / 1.7f), 28, 80);
        for (int i = 0; i < cantidad; i++)
        {
            float x = Mathf.Lerp(bounds.min.x - 6, bounds.max.x + 6, (i + .5f) / cantidad);
            var p = new Vector3(x, bounds.center.y, bounds.max.z + 1.2f + (float)rng.NextDouble() * 1.2f);
            if (decorador != null && decorador.TrySampleSurface(p, out var suelo, out _)) p.y = suelo.y;
            frentes.Add(p + Vector3.up * .15f);
        }
        foreach (var p in frentes)
            incendio.Emit(new ParticleSystem.EmitParams { position = p, startLifetime = 1.2f + (float)rng.NextDouble() }, 1);
        incendio.Play(false);
        relojAnterior = acumulador = 0; siguienteFrente = 0;
    }

    public static Gradient GradienteFuego()
    {
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(.88f, .12f), new GradientAlphaKey(.65f, .58f), new GradientAlphaKey(0, 1) });
        return g;
    }

    public void Actualizar(float reloj, float dia)
    {
        foreach (var material in materiales)
        {
            material.SetFloat("_Reloj", reloj);
            if (material.HasProperty("_Dia")) material.SetFloat("_Dia", dia);
        }
        float dt = Mathf.Max(0, reloj - relojAnterior); relojAnterior = reloj;
        if (incendio == null || frentes.Count == 0 || dt <= 0) return;
        acumulador += Mathf.Min(dt, .1f) * 95 * factorCalidad;
        int cantidad = Mathf.Min(12, Mathf.FloorToInt(acumulador)); acumulador -= cantidad;
        for (int i = 0; i < cantidad; i++)
        {
            var p = frentes[siguienteFrente++ % frentes.Count];
            p.x += ((float)rng.NextDouble() - .5f) * .8f;
            incendio.Emit(new ParticleSystem.EmitParams { position = p }, 1);
        }
    }

    void OnEnable() { if (incendio != null) incendio.Play(false); }
    void OnDisable()
    {
        if (incendio != null) incendio.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        acumulador = 0;
    }
    void Limpiar()
    {
        var recursos = new HashSet<Object>();
        foreach (var mesh in mallas) recursos.Add(mesh);
        foreach (var material in materiales) recursos.Add(material);
        // También recoge los recursos de la versión anterior después de una recarga de scripts.
        var hijos = new List<Transform>();
        foreach (Transform hijo in transform) hijos.Add(hijo);
        foreach (var hijo in hijos)
        {
            var filtro = hijo.GetComponent<MeshFilter>();
            if (filtro != null && filtro.sharedMesh != null && (filtro.sharedMesh.hideFlags & HideFlags.DontSave) != 0) recursos.Add(filtro.sharedMesh);
            var renderer = hijo.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null && (renderer.sharedMaterial.hideFlags & HideFlags.DontSave) != 0) recursos.Add(renderer.sharedMaterial);
            hijo.gameObject.SetActive(false); Liberar(hijo.gameObject);
        }
        foreach (var recurso in recursos) Liberar(recurso);
        mallas.Clear(); materiales.Clear(); frentes.Clear(); incendio = null;
    }
    void OnDestroy()
    {
        if (humoProvisional != null) humoProvisional.enabled = humoProvisionalVisible;
        Limpiar();
    }
    static void Liberar(Object o) { if (o == null) return; if (Application.isPlaying) Destroy(o); else DestroyImmediate(o); }
}
