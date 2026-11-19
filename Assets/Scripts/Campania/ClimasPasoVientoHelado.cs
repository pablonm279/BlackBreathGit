using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Presentacion del clima del Paso. Solo lee el estado de campaña: no tira clima ni aplica beneficios.
[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public sealed class ClimasPasoVientoHelado : MonoBehaviour
{
    AtributosZona zona;
    AmbientePasoVientoHelado ambiente;
    MapDecorator decorador;
    Camera camara;
    SunController sol;
    GameObject raiz;
    Texture2D ruido;
    Material matCopos, matGrano, matLluvia, matBruma, matImpactos, matDestellos;
    ParticleSystem copos, grano, lluvia, bruma, impactos, destellos;
    readonly Dictionary<Renderer, bool> renderersOriginales = new Dictionary<Renderer, bool>();
    readonly Dictionary<ParticleSystem, bool> emisionesOriginales = new Dictionary<ParticleSystem, bool>();
    readonly Dictionary<ParticleSystemRenderer, Material> materialesAuroraOriginales = new Dictionary<ParticleSystemRenderer, Material>();
    readonly Dictionary<ParticleSystemRenderer, Material> materialesAuroraRefinados = new Dictionary<ParticleSystemRenderer, Material>();
    readonly Dictionary<Light, float> lucesAuroraOriginales = new Dictionary<Light, float>();
    readonly List<Material> materiales = new List<Material>();
    System.Random rng;
    Vector3 centro;
    Vector4 mezcla; // lluvia, nieve, niebla, aurora
    float cobertura, factorCalidad, acumuladorImpactos, acumuladorDestellos;
    bool preparado;
    static float coberturaLuz;

    public static float CoberturaLuz => coberturaLuz;
    public int ClimaVisualActual => ObtenerClima();
    public Texture2D RuidoVisual => ruido;
    public int CantidadParticulas => Contar(copos) + Contar(grano) + Contar(lluvia)
        + Contar(bruma) + Contar(impactos) + Contar(destellos);

#if UNITY_EDITOR
    // Vista visual aislada para comparar perfiles sin tocar clima, reloj ni RNG de campaña.
    [System.NonSerialized] public int climaVistaPrevia = -1;
    public void PrepararVistaPrevia(int clima, Camera vista = null)
    {
        climaVistaPrevia = clima;
        if (vista != null) camara = vista;
        mezcla = Perfil(clima, out cobertura);
        foreach (var ps in new[] { copos, grano, lluvia, bruma, impactos, destellos })
            if (ps != null) ps.Clear(false);
    }
#endif

    public static void Preparar(AtributosZona zona, AmbientePasoVientoHelado ambiente)
    {
        if (zona == null || zona.ID != 2 || zona.pasovientoheladoContenedorGameObjects == null) return;
        var go = zona.pasovientoheladoContenedorGameObjects;
        var componente = go.GetComponent<ClimasPasoVientoHelado>();
        if (componente == null) componente = go.AddComponent<ClimasPasoVientoHelado>();
        componente.zona = zona;
        componente.ambiente = ambiente;
        componente.Construir();
    }

    void Construir()
    {
        if (preparado || zona.TexturaTerreno == null) return;
        var shader = Resources.Load<Shader>("PasoVientoHeladoClimaParticulas");
        if (shader == null) return;
        decorador = zona.GetComponent<MapDecorator>();
        sol = zona.pasovientoheladoContenedorGameObjects.GetComponentInChildren<SunController>(true);
        rng = new System.Random(unchecked((decorador != null ? decorador.GetReliefSeed() : 19) ^ 0x31a925));
        int calidad = Mathf.Clamp(PlayerPrefs.GetInt("graficos_index", QualitySettings.GetQualityLevel()), 0, 2);
        factorCalidad = calidad == 0 ? .45f : calidad == 1 ? .72f : 1f;
        ruido = CrearRuido();
        raiz = new GameObject("Climas del Paso - visuales (Runtime)") { hideFlags = HideFlags.DontSave };
        raiz.transform.SetParent(transform, true); // escala mundial unitaria
        matCopos = MaterialParticulas(shader, "Copos de nevada", 0, new Color(.78f,.86f,.94f,.42f), .7f);
        matGrano = MaterialParticulas(shader, "Cristales de ventisca", 0, new Color(.72f,.84f,.95f,.30f), .5f);
        matLluvia = MaterialParticulas(shader, "Lluvia fria", 1, new Color(.58f,.70f,.82f,.38f), .4f);
        matBruma = MaterialParticulas(shader, "Niebla en volumen", 3, new Color(.44f,.54f,.64f,.28f), 4f);
        matBruma.renderQueue = 2950;
        matImpactos = MaterialParticulas(shader, "Impactos de lluvia", 2, new Color(.60f,.73f,.82f,.24f), .12f);
        matDestellos = MaterialParticulas(shader, "Cristales iluminados", 4, new Color(.72f,.90f,1,.34f), .1f);
        copos = CrearParticulas("Nevada - copos", matCopos, 700, .035f, .085f, 6f,
            new Vector3(-.75f,-.90f,.18f), new Vector3(24,8,18), 101);
        grano = CrearParticulas("Nevada - grano fino", matGrano, 400, .014f, .035f, 5f,
            new Vector3(-1.25f,-.48f,.27f), new Vector3(42,4,26), 211);
        lluvia = CrearParticulas("Lluvia - gotas inclinadas", matLluvia, 900, .025f, .055f, 1.8f,
            new Vector3(-.60f,-7.4f,.15f), new Vector3(36,.8f,24), 307);
        var rendererLluvia = lluvia.GetComponent<ParticleSystemRenderer>();
        rendererLluvia.renderMode = ParticleSystemRenderMode.Stretch;
        rendererLluvia.velocityScale = .045f;
        rendererLluvia.lengthScale = 2.1f;
        var mainLluvia = lluvia.main; mainLluvia.startRotation = 0;
        bruma = CrearParticulas("Niebla - bancos moviles", matBruma, 64, 7f, 12f, 13f,
            new Vector3(-.12f,.008f,.035f), new Vector3(46,2.5f,30), 419);
        impactos = CrearParticulas("Lluvia - contacto con suelo", matImpactos, 80, .09f, .18f, .42f,
            Vector3.zero, Vector3.one, 523);
        destellos = CrearParticulas("Despejado - cristales aislados", matDestellos, 32, .035f, .075f, 1.1f,
            Vector3.zero, Vector3.one, 631);
        foreach (var ps in new[] { impactos, destellos })
        {
            var shape = ps.shape; shape.enabled = false;
            ps.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;
        }
        foreach (var ps in new[] { copos, grano })
        {
            var n = ps.noise; n.enabled = true; n.strength = ps == copos ? .22f : .12f;
            n.frequency = .24f; n.scrollSpeed = .15f; n.quality = ParticleSystemNoiseQuality.Medium;
        }
        var tamBruma = bruma.sizeOverLifetime; tamBruma.enabled = true;
        tamBruma.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0,.8f,1,1.25f));
        var tamImpactos = impactos.sizeOverLifetime; tamImpactos.enabled = true;
        tamImpactos.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0,.25f,1,1.2f));
        preparado = true;
        CapturarYDesactivarHeredados();
        mezcla = Perfil(ObtenerClima(), out cobertura);
        foreach (var ps in new[] { copos, grano, lluvia, bruma, impactos, destellos }) ps.Play(false);
    }

    Material MaterialParticulas(Shader shader, string nombre, int modo, Color color, float suavidad)
    {
        var m = new Material(shader) { name = nombre + " del Paso (Runtime)", hideFlags = HideFlags.DontSave };
        m.SetTexture("_RuidoTex", ruido); m.SetFloat("_Modo", modo); m.SetColor("_Color", color);
        m.SetFloat("_SuavidadProfundidad", suavidad); materiales.Add(m); return m;
    }

    ParticleSystem CrearParticulas(string nombre, Material material, int maximo, float min, float max,
        float vida, Vector3 velocidad, Vector3 volumen, uint semilla)
    {
        var go = new GameObject(nombre) { layer = zona.TexturaTerreno.gameObject.layer, hideFlags = HideFlags.DontSave };
        go.SetActive(false); go.transform.SetParent(raiz.transform, false);
        var ps = go.AddComponent<ParticleSystem>(); ps.useAutoRandomSeed = false; ps.randomSeed = semilla;
        var main = ps.main; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.maxParticles = Mathf.CeilToInt(maximo * factorCalidad);
        main.startSize = new ParticleSystem.MinMaxCurve(min, max);
        main.startLifetime = new ParticleSystem.MinMaxCurve(vida*.8f, vida*1.2f);
        main.startSpeed = 0; main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
        var em = ps.emission; em.rateOverTime = 0;
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = volumen;
        var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
        vel.x = velocidad.x; vel.y = velocidad.y; vel.z = velocidad.z;
        var col = ps.colorOverLifetime; col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(new[] { new GradientColorKey(Color.white,0), new GradientColorKey(Color.white,1) },
            new[] { new GradientAlphaKey(0,0), new GradientAlphaKey(1,.15f), new GradientAlphaKey(.7f,.7f), new GradientAlphaKey(0,1) });
        col.color = grad;
        var renderer = ps.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
        renderer.maxParticleSize = material == matBruma ? .38f : .06f;
        go.SetActive(true); return ps;
    }

    Texture2D CrearRuido()
    {
        var t = new Texture2D(64,64,TextureFormat.RGBA32,false,true)
        { name = "Ruido de clima del Paso (Runtime)", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
        var colores = new Color32[64*64]; var random = new System.Random(11963);
        for (int i = 0; i < colores.Length; i++) { byte v = (byte)random.Next(256); colores[i] = new Color32(v,v,v,255); }
        t.SetPixels32(colores); t.Apply(false,true); return t;
    }

    void PrepararAuroraAnterior(GameObject aurora)
    {
        if (aurora == null) return;
        foreach (var renderer in aurora.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            if (!materialesAuroraRefinados.TryGetValue(renderer, out var material))
            {
                var original = renderer.sharedMaterial;
                if (original == null) continue;
                if (!renderersOriginales.ContainsKey(renderer)) renderersOriginales.Add(renderer, renderer.enabled);
                materialesAuroraOriginales.Add(renderer,original);
                material = new Material(original) { name = original.name + " Aurora suave (Runtime)", hideFlags = HideFlags.DontSave };
                var shaderAurora = Resources.Load<Shader>("PasoVientoHeladoAuroraParticulas");
                if (shaderAurora != null)
                {
                    material.shader = shaderAurora;
                    material.SetFloat("_Fase", original.HasProperty("_Color") ? original.GetColor("_Color").b * .73f : 0);
                }
                if (material.HasProperty("_Color"))
                {
                    Color color = original.GetColor("_Color");
                    material.SetColor("_Color",new Color(color.r*.9f,color.g*.9f,color.b*.9f,Mathf.Min(1,color.a*1.08f)));
                }
                if (material.HasProperty("_SoftParticlesEnabled"))
                {
                    material.SetFloat("_SoftParticlesEnabled",1);
                    material.SetFloat("_SoftParticlesNearFadeDistance",0);
                    material.SetFloat("_SoftParticlesFarFadeDistance",1.5f);
                    material.SetVector("_SoftParticleFadeParams",new Vector4(0,1f/1.5f,0,0));
                    material.EnableKeyword("_FADING_ON");
                }
                materialesAuroraRefinados.Add(renderer,material); materiales.Add(material);
            }
            renderer.sharedMaterial = material;
        }
        foreach (var luz in aurora.GetComponentsInChildren<Light>(true))
            if (!lucesAuroraOriginales.ContainsKey(luz)) lucesAuroraOriginales.Add(luz,luz.intensity);
    }

    void CapturarYDesactivarHeredados()
    {
        if (!Application.isPlaying) return;
        var menu = Object.FindFirstObjectByType<MenuDescanso>(FindObjectsInactive.Include);
        if (menu == null) return;
        foreach (var go in new[] { menu.climaLluvia, menu.climaNieve, menu.climaNiebla })
        {
            if (go == null) continue;
            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderersOriginales.ContainsKey(renderer)) renderersOriginales.Add(renderer,renderer.enabled);
                renderer.enabled = false;
            }
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (!emisionesOriginales.ContainsKey(ps)) emisionesOriginales.Add(ps,ps.emission.enabled);
                var em = ps.emission; em.enabled = false;
                ps.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
        PrepararAuroraAnterior(menu.climaAuroraBoreal);
    }

    int ObtenerClima()
    {
#if UNITY_EDITOR
        if (climaVistaPrevia >= 0) return climaVistaPrevia;
#endif
        return CampaignManager.Instance != null ? CampaignManager.Instance.intTipoClima : 1;
    }

    static Vector4 Perfil(int clima, out float nubes)
    {
        switch (clima)
        {
            case 3: nubes=.85f; return new Vector4(1,0,.10f,0);
            case 4: nubes=.72f; return new Vector4(0,1,.18f,0);
            case 5: nubes=.55f; return new Vector4(0,.04f,1,0);
            case 7: nubes=0; return new Vector4(0,0,0,1);
            default: nubes=0; return Vector4.zero;
        }
    }

    void Update()
    {
        if (!preparado || zona == null || zona.ID != 2) return;
        if (camara == null || !camara.isActiveAndEnabled) camara = Camera.main;
        if (camara == null) return;
        camara.depthTextureMode |= DepthTextureMode.Depth;
        Vector4 objetivo = Perfil(ObtenerClima(), out float nubes);
        float t = 1-Mathf.Exp(-Time.unscaledDeltaTime*.8f);
        mezcla = Vector4.Lerp(mezcla,objetivo,t); cobertura = Mathf.Lerp(cobertura,nubes,t);
        coberturaLuz = cobertura;
        Shader.SetGlobalVector("_PasoClimaVisual",new Vector4(cobertura,mezcla.z,mezcla.y,mezcla.w));
        float hora = CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerHoraActual() : 12;
#if UNITY_EDITOR
        if (ambiente != null && ambiente.horaVistaPrevia >= 0) hora = ambiente.horaVistaPrevia;
#endif
        AmbientePasoVientoHelado.EvaluarHora(hora,out float intensidad,out _,out _);
        float dia = Mathf.InverseLerp(.18f,.86f,intensidad);
        float luz = Mathf.Lerp(.20f,1f,dia);
        Color tinte = Shader.GetGlobalColor("_PasoTinteCielo");
        foreach (var m in materiales)
            if (m.HasProperty("_Luz")) { m.SetFloat("_Luz",luz); m.SetColor("_Tinte",tinte); }
        // Reflejo verde/celeste de la aurora: suave, con una respiracion lenta por color.
        foreach (var par in lucesAuroraOriginales)
            if (par.Key != null)
            {
                float fase = par.Key.color.b * 4.1f;
                float pulso = .94f + .06f * Mathf.Sin(Time.unscaledTime * .17f + fase);
                par.Key.intensity = par.Value*Mathf.Lerp(.065f,.028f,dia)*mezcla.w*pulso;
            }
        foreach (var par in materialesAuroraRefinados)
        {
            if (par.Key == null) continue;
            // La aurora acompaña el cielo: tenue de día, visible de noche y solo bajo su clima.
            par.Key.enabled = mezcla.w > .01f && renderersOriginales[par.Key];
            if (par.Value.HasProperty("_Color") && materialesAuroraOriginales[par.Key].HasProperty("_Color"))
            {
                Color color = materialesAuroraOriginales[par.Key].GetColor("_Color");
                bool azul = color.b > color.g;
                float brillo = 1.12f * Mathf.Lerp(1, azul ? 1.90f : 1.25f, dia);
                color.r *= brillo; color.g *= brillo; color.b *= brillo;
                color.a = Mathf.Min(1, color.a * mezcla.w * Mathf.Lerp(.92f, .78f, dia)
                    * Mathf.Lerp(1, azul ? 1.35f : 1, dia));
                par.Value.SetColor("_Color", color);
            }
        }
        PosicionarEmisores();
        float rafaga = VientoPasoVientoHelado.Activo
            ? Mathf.Lerp(.72f,1.27f,Mathf.Clamp01(VientoPasoVientoHelado.FuerzaEn(centro)))
            : .72f + Mathf.Pow(.5f+.5f*Mathf.Sin(Time.unscaledTime*.31f),2)*.55f;
        Tasa(copos,75*mezcla.y*rafaga); Tasa(grano,53*mezcla.y*rafaga);
        Tasa(lluvia,230*mezcla.x); Tasa(bruma,3.2f*mezcla.z);
        if (VientoPasoVientoHelado.Activo)
        {
            var dir = VientoPasoVientoHelado.Direccion;
            float fuerza = VientoPasoVientoHelado.FuerzaEn(centro);
            VientoHorizontal(copos,dir*(.4f+fuerza));
            VientoHorizontal(grano,dir*(.7f+fuerza*1.3f));
            VientoHorizontal(lluvia,dir*(.45f+fuerza*.35f));
            VientoHorizontal(bruma,dir*(.06f+fuerza*.10f));
        }
        EmitirSuelo(impactos,ref acumuladorImpactos,12*mezcla.x);
        float despejado = Mathf.Clamp01(1-mezcla.x-mezcla.y-mezcla.z);
        EmitirSuelo(destellos,ref acumuladorDestellos,1.8f*despejado*Mathf.Lerp(.2f,1,dia));
        if (sol != null) sol.SetCampaignHour(hora);
    }

    void PosicionarEmisores()
    {
        var bounds = zona.TexturaTerreno.bounds; centro = bounds.center;
        var plano = new Plane(Vector3.up,centro); var ray = camara.ViewportPointToRay(new Vector3(.5f,.45f,0));
        if (plano.Raycast(ray,out float distancia)) centro = ray.GetPoint(distancia);
        centro.x = Mathf.Clamp(centro.x,bounds.min.x+2,bounds.max.x-2);
        centro.z = Mathf.Clamp(centro.z,bounds.min.z+2,bounds.max.z-2);
        if (decorador != null && decorador.TrySampleSurface(centro,out var suelo,out _)) centro.y = suelo.y;
        copos.transform.position = centro+Vector3.up*4.5f;
        grano.transform.position = centro+Vector3.up*2;
        lluvia.transform.position = centro+Vector3.up*10;
        bruma.transform.position = centro+Vector3.up*1.6f;
    }

    void Tasa(ParticleSystem ps, float tasa)
    {
        var em = ps.emission; em.rateOverTimeMultiplier = tasa*factorCalidad;
    }

    static void VientoHorizontal(ParticleSystem ps, Vector3 viento)
    {
        var v = ps.velocityOverLifetime; v.x = viento.x; v.z = viento.z;
    }

    void EmitirSuelo(ParticleSystem ps, ref float acumulador, float tasa)
    {
        if (decorador == null) return;
        acumulador = Mathf.Min(8,acumulador + Mathf.Min(.1f,Time.unscaledDeltaTime)*tasa*factorCalidad);
        while (acumulador >= 1)
        {
            acumulador--;
            var p = centro+new Vector3(((float)rng.NextDouble()-.5f)*28,0,((float)rng.NextDouble()-.5f)*18);
            if (!decorador.PasaMargenCornisaFrontal(p,.2f) || !decorador.TrySampleSurface(p,out var suelo,out var normal) || normal.y < .85f) continue;
            var e = new ParticleSystem.EmitParams { position=suelo+Vector3.up*.025f, velocity=Vector3.zero };
            ps.Emit(e,1);
        }
    }

    void OnEnable()
    {
        if (!preparado) return;
        if (raiz != null) raiz.SetActive(true);
        CapturarYDesactivarHeredados();
        foreach (var ps in new[] { copos, grano, lluvia, bruma, impactos, destellos })
            if (ps != null) ps.Play(false);
    }
    void OnDisable()
    {
        if (raiz != null) raiz.SetActive(false);
        coberturaLuz = 0; Shader.SetGlobalVector("_PasoClimaVisual",Vector4.zero);
        if (sol != null && CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona == zona)
            sol.SetCampaignHour(CampaignManager.Instance.ObtenerHoraActual());
        foreach (var par in renderersOriginales) if (par.Key != null) par.Key.enabled = par.Value;
        foreach (var par in materialesAuroraOriginales) if (par.Key != null) par.Key.sharedMaterial = par.Value;
        foreach (var par in lucesAuroraOriginales) if (par.Key != null) par.Key.intensity = par.Value;
        foreach (var par in emisionesOriginales)
        {
            if (par.Key == null) continue;
            var em = par.Key.emission; em.enabled = par.Value;
            if (par.Value && par.Key.gameObject.activeInHierarchy) par.Key.Play(false);
        }
    }
    void OnDestroy()
    {
        foreach (var m in materiales) Liberar(m);
        Liberar(ruido); Liberar(raiz);
    }
    static int Contar(ParticleSystem ps) => ps != null ? ps.particleCount : 0;
    static void Liberar(Object obj) { if (obj == null) return; if (Application.isPlaying) Destroy(obj); else DestroyImmediate(obj); }
}
