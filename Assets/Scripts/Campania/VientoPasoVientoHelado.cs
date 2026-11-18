using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Una corriente visual compartida por polvo, precipitación, vegetación y luces del Paso.
[DisallowMultipleComponent]
[DefaultExecutionOrder(50)]
public sealed class VientoPasoVientoHelado : MonoBehaviour
{
    AtributosZona zona;
    AmbientePasoVientoHelado ambiente;
    ClimasPasoVientoHelado clima;
    MapDecorator decorador;
    Camera camara;
    GameObject raiz;
    ParticleSystem rasante, cristales;
    Material matRasante, matCristales;
    readonly Dictionary<ParticleSystem, bool> emisiones = new Dictionary<ParticleSystem, bool>();
    readonly Dictionary<ParticleSystemRenderer, bool> renderers = new Dictionary<ParticleSystemRenderer, bool>();
    readonly Dictionary<Light, float> luces = new Dictionary<Light, float>();
    System.Random rng;
    float calidad, fuerza = .72f, pendientes;
    bool preparado;
    static Vector4 estado;
    static Shader shaderArbol;

    public static bool Activo => estado.z > 0;
    public static Vector3 Direccion => Activo ? new Vector3(estado.x,0,estado.y) : new Vector3(-1,0,.22f).normalized;
    public static Vector4 Estado => estado;
    public int ParticulasActivas => (rasante != null ? rasante.particleCount : 0) + (cristales != null ? cristales.particleCount : 0);

    public static float FuerzaEn(Vector3 p)
    {
        if (!Activo) return .6f;
        float fase = estado.w*.42f-(p.x*estado.x+p.z*estado.y)*.09f;
        float pulso = Mathf.Pow(.5f+.5f*Mathf.Sin(fase),3);
        return estado.z*(.20f+.80f*pulso);
    }

    public static void Preparar(AtributosZona zona, AmbientePasoVientoHelado ambiente)
    {
        if (zona == null || zona.ID != 2 || zona.pasovientoheladoContenedorGameObjects == null) return;
        var go = zona.pasovientoheladoContenedorGameObjects;
        var viento = go.GetComponent<VientoPasoVientoHelado>();
        if (viento == null) viento = go.AddComponent<VientoPasoVientoHelado>();
        viento.zona = zona; viento.ambiente = ambiente; viento.Construir();
    }

    public static void PrepararMaterialArbol(Material material)
    {
        if (material == null || !material.HasProperty("_MainTex")) return;
        var textura = material.GetTexture("_MainTex");
        if (textura == null) return;
        string nombre = textura.name.ToLowerInvariant();
        if (!nombre.Contains("arbol") && !nombre.Contains("tree")) return;
        if (shaderArbol == null) shaderArbol = Resources.Load<Shader>("PasoVientoHeladoVegetacion");
        if (shaderArbol == null) return;
        material.shader = shaderArbol;
        material.SetFloat("_RespuestaViento",1f);
        material.renderQueue = (int)RenderQueue.AlphaTest;
    }

    void Construir()
    {
        if (preparado || zona.TexturaTerreno == null) return;
        var shader = Resources.Load<Shader>("PasoVientoHeladoClimaParticulas");
        if (shader == null) return;
        clima = GetComponent<ClimasPasoVientoHelado>(); decorador = zona.GetComponent<MapDecorator>();
        rng = new System.Random(unchecked((decorador != null ? decorador.GetReliefSeed() : 17)^0x42b17));
        int nivel = Mathf.Clamp(PlayerPrefs.GetInt("graficos_index",QualitySettings.GetQualityLevel()),0,2);
        calidad = nivel == 0 ? .45f : nivel == 1 ? .72f : 1f;
        raiz = new GameObject("Corrientes permanentes del Paso (Runtime)") { hideFlags = HideFlags.DontSave };
        raiz.transform.SetParent(transform,true);
        matRasante = CrearMaterial(shader,"Nieve rasante en rafagas",3,new Color(.66f,.77f,.87f,.20f),.065f);
        matCristales = CrearMaterial(shader,"Escarcha arrastrada",0,new Color(.70f,.84f,.95f,.30f),.45f);
        rasante = CrearSistema("Rafagas - polvo sobre nieve",matRasante,110,2.1f,.6f,1.5f,571);
        var m = rasante.main; m.startSize3D = true;
        m.startSizeX = new ParticleSystem.MinMaxCurve(.7f,1.8f);
        m.startSizeY = new ParticleSystem.MinMaxCurve(.18f,.40f); m.startSizeZ = 1;
        m.startRotation = Mathf.Atan2(.22f,-1f);
        var s = rasante.shape; s.enabled = false;
        var e = rasante.emission; e.enabled = false;
        rasante.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        cristales = CrearSistema("Rafagas - cristales suspendidos",matCristales,160,3.7f,.018f,.045f,683);
        s = cristales.shape; s.shapeType = ParticleSystemShapeType.Box; s.scale = new Vector3(24,1.8f,18);
        var v = cristales.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;
        v.x = -1; v.y = -.10f; v.z = .22f;
        var n = cristales.noise; n.enabled = true; n.strength = .12f; n.frequency = .4f; n.scrollSpeed = .2f;
        foreach (var l in GetComponentsInChildren<Light>(true))
            if (l.type == LightType.Point && l.transform.parent != null && l.transform.parent.name.Contains("Fuego"))
                luces[l] = l.intensity;
        preparado = true;
        SustituirHeredados();
        rasante.Play(false); cristales.Play(false);
    }

    Material CrearMaterial(Shader shader, string nombre, int modo, Color color, float suavidad)
    {
        var material = new Material(shader) { name = nombre+" (Runtime)", hideFlags = HideFlags.DontSave };
        material.SetFloat("_Modo",modo); material.SetColor("_Color",color);
        material.SetFloat("_SuavidadProfundidad",suavidad);
        if (clima != null && clima.RuidoVisual != null) material.SetTexture("_RuidoTex",clima.RuidoVisual);
        return material;
    }

    ParticleSystem CrearSistema(string nombre, Material material, int maximo, float vida, float min, float max, uint semilla)
    {
        var go = new GameObject(nombre) { layer = zona.TexturaTerreno.gameObject.layer, hideFlags = HideFlags.DontSave };
        go.SetActive(false); go.transform.SetParent(raiz.transform,false);
        var ps = go.AddComponent<ParticleSystem>(); ps.useAutoRandomSeed = false; ps.randomSeed = semilla;
        var main = ps.main; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.maxParticles = Mathf.CeilToInt(maximo*calidad);
        main.startLifetime = new ParticleSystem.MinMaxCurve(vida*.8f,vida*1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(min,max); main.startSpeed = 0;
        var em = ps.emission; em.rateOverTime = 0;
        var color = ps.colorOverLifetime; color.enabled = true;
        var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white,0), new GradientColorKey(Color.white,1) },
            new[] { new GradientAlphaKey(0,0), new GradientAlphaKey(.85f,.2f), new GradientAlphaKey(.5f,.65f), new GradientAlphaKey(0,1) });
        color.color = g;
        var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false; renderer.maxParticleSize = .12f;
        go.SetActive(true); return ps;
    }

    void SustituirHeredados()
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            string nombre = ps.name;
            if (nombre != "Viento" && nombre != "Viento (1)" && nombre != "Vientodelmapa"
                && nombre != "Nieveambiental" && nombre != "NieblaBase" && nombre != "NieblaFondo"
                && nombre != "Copos al viento" && nombre != "Nieve barrida sobre el suelo") continue;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (!renderers.ContainsKey(renderer)) renderers.Add(renderer,renderer.enabled);
            if (!emisiones.ContainsKey(ps)) emisiones.Add(ps,ps.emission.enabled);
            renderer.enabled = false; var em = ps.emission; em.enabled = false;
            ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        if (!preparado || zona == null || zona.ID != 2) return;
        float objetivo = clima != null && clima.ClimaVisualActual == 4 ? 1.3f
            : clima != null && clima.ClimaVisualActual == 3 ? 1f
            : clima != null && clima.ClimaVisualActual == 5 ? .5f : .75f;
        fuerza = Mathf.Lerp(fuerza,objetivo,1-Mathf.Exp(-Time.unscaledDeltaTime*.7f));
        Vector3 dir = new Vector3(-1,0,.22f).normalized;
        estado = new Vector4(dir.x,dir.z,fuerza,Time.unscaledTime);
        Shader.SetGlobalVector("_PasoVientoGlobal",estado);
        if (camara == null || !camara.isActiveAndEnabled) camara = Camera.main;
        if (camara == null) return;
        camara.depthTextureMode |= DepthTextureMode.Depth;
        var bounds = zona.TexturaTerreno.bounds; Vector3 centro = bounds.center;
        var plano = new Plane(Vector3.up,centro); var rayo = camara.ViewportPointToRay(new Vector3(.5f,.45f,0));
        if (plano.Raycast(rayo,out float distancia)) centro = rayo.GetPoint(distancia);
        centro.x = Mathf.Clamp(centro.x,bounds.min.x+2,bounds.max.x-2);
        centro.z = Mathf.Clamp(centro.z,bounds.min.z+2,bounds.max.z-2);
        if (decorador != null && decorador.TrySampleSurface(centro,out var superficie,out _)) centro.y = superficie.y;
        float racha = FuerzaEn(centro);
        cristales.transform.position = centro+Vector3.up*.9f;
        var vel = cristales.velocityOverLifetime; vel.x = dir.x*(.65f+racha*1.6f); vel.z = dir.z*(.65f+racha*1.6f);
        var emision = cristales.emission; emision.rateOverTimeMultiplier = calidad*(2.5f+racha*11);
        pendientes = Mathf.Min(6,pendientes+Mathf.Min(.1f,Time.unscaledDeltaTime)*calidad*(3+racha*racha*18));
        while (pendientes >= 1)
        {
            pendientes--;
            Vector3 p = centro+new Vector3(((float)rng.NextDouble()-.5f)*24,0,((float)rng.NextDouble()-.5f)*16);
            if (decorador == null || !decorador.PasaMargenCornisaFrontal(p,.3f)
                || !decorador.TrySampleSurface(p,out var suelo,out var normal) || normal.y < .88f) continue;
            Vector3 avance = Vector3.ProjectOnPlane(dir,normal).normalized*(1.4f+FuerzaEn(p)*2);
            rasante.Emit(new ParticleSystem.EmitParams { position=suelo+Vector3.up*.045f, velocity=avance },1);
        }
        float hora = CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerHoraActual() : 12;
#if UNITY_EDITOR
        if (ambiente != null && ambiente.horaVistaPrevia >= 0) hora = ambiente.horaVistaPrevia;
#endif
        AmbientePasoVientoHelado.EvaluarHora(hora,out float intensidad,out _,out _);
        float dia = Mathf.InverseLerp(.18f,.86f,intensidad);
        float luz = Mathf.Lerp(.24f,1,dia); Color tinte = Shader.GetGlobalColor("_PasoTinteCielo");
        matRasante.SetFloat("_Luz",luz); matRasante.SetColor("_Tinte",tinte);
        matCristales.SetFloat("_Luz",luz); matCristales.SetColor("_Tinte",tinte);
        foreach (var par in luces)
        {
            if (par.Key == null) continue;
            float fase = par.Key.transform.position.x*.71f+par.Key.transform.position.z*.33f;
            float titilar = .95f+Mathf.Sin(Time.unscaledTime*6.2f+fase)*.035f
                +Mathf.Sin(Time.unscaledTime*11.3f-fase)*FuerzaEn(par.Key.transform.position)*.045f;
            par.Key.intensity = par.Value*Mathf.Lerp(1,.4f,dia)*titilar;
        }
    }

    void OnEnable()
    {
        if (!preparado) return;
        raiz.SetActive(true); SustituirHeredados(); rasante.Play(false); cristales.Play(false);
    }
    void OnDisable()
    {
        estado = Vector4.zero; Shader.SetGlobalVector("_PasoVientoGlobal",Vector4.zero);
        if (raiz != null) raiz.SetActive(false);
        foreach (var par in renderers) if (par.Key != null) par.Key.enabled = par.Value;
        foreach (var par in emisiones)
        {
            if (par.Key == null) continue;
            var em = par.Key.emission; em.enabled = par.Value;
            if (par.Value && par.Key.gameObject.activeInHierarchy) par.Key.Play(false);
        }
        foreach (var par in luces) if (par.Key != null) par.Key.intensity = par.Value;
    }
    void OnDestroy() { Liberar(matRasante); Liberar(matCristales); Liberar(raiz); }
    static void Liberar(Object obj) { if (obj == null) return; if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj); }
}
