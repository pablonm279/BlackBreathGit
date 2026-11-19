using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Solo decoracion de campaña. Todos los recursos son propios de esta instancia.
[DisallowMultipleComponent]
public sealed class AmbienteBosqueArdiente : MonoBehaviour
{
    readonly List<Material> materiales = new List<Material>(7);
    readonly List<Vector3> focos = new List<Vector3>();
    readonly List<ParticleSystem> sistemasAmbiente = new List<ParticleSystem>(3);
    readonly Dictionary<Material, Material> materialesLlamas = new Dictionary<Material, Material>();
    struct MedidasFuego { public ParticleSystem.MinMaxCurve x, y, z, velocidad; }
    readonly Dictionary<ParticleSystem, MedidasFuego> medidasFuego = new Dictionary<ParticleSystem, MedidasFuego>();
    readonly Dictionary<ParticleSystemRenderer, bool> visibilidadFuego = new Dictionary<ParticleSystemRenderer, bool>();
    Bounds areaFuego;
    AtributosZona zona;
    Material humo, brasa, ceniza;
    GameObject raizParticulas;
    readonly List<Light> lucesIncendio = new List<Light>(4);
    GameObject raizLuces;
    FondoBosqueArdiente fondo;
    SunController solBosque;
    bool iluminacionPreparada;
    float reloj;
    static readonly int RelojId = Shader.PropertyToID("_Reloj");

    public static void Preparar(AtributosZona atributos, bool conservarTextura)
    {
        if (atributos == null || atributos.ID != 1 || atributos.bosqueardienteContenedorGameObjects == null) return;
        var raiz = atributos.bosqueardienteContenedorGameObjects;
        var ambiente = raiz.GetComponent<AmbienteBosqueArdiente>();
        if (ambiente == null) ambiente = raiz.AddComponent<AmbienteBosqueArdiente>();
        ambiente.zona = atributos;
        ambiente.DelimitarFuego();
        var camara = Camera.main;
        if (camara != null) camara.depthTextureMode |= DepthTextureMode.Depth;
        foreach (var emisor in new List<ParticleSystem>(ambiente.medidasFuego.Keys))
            if (emisor == null) ambiente.medidasFuego.Remove(emisor);
        ambiente.PrepararMateriales();
        ambiente.PrepararTerreno(atributos.TexturaTerreno, conservarTextura);
        ambiente.PrepararTerreno(atributos.TexturaTerrenoExtension, conservarTextura);
        ambiente.focos.Clear();
        ambiente.RecolectarFocos(raiz);
        ambiente.RecolectarFocos(atributos.gameObject);
        ambiente.RefinarMaterialLlamas(raiz);
        ambiente.RefinarMaterialLlamas(atributos.gameObject);
        ambiente.AjustarFocosAlSuelo();
        ambiente.PrepararAtmosfera();
        ambiente.PrepararIluminacion();
        ambiente.fondo = FondoBosqueArdiente.Preparar(atributos, ambiente.transform);
        ambiente.ActualizarLuzAmbiente();
    }

    void PrepararMateriales()
    {
        if (humo != null) return;
        var shader = Resources.Load<Shader>("BosqueArdienteAmbiente");
        if (shader == null) return;
        humo = CrearMaterial(shader, "Humo de incendio", 1);
        brasa = CrearMaterial(shader, "Brasas al viento", 2);
        ceniza = CrearMaterial(shader, "Ceniza suspendida", 3);
    }

    Material CrearMaterial(Shader shader, string nombre, float modo)
    {
        var material = new Material(shader) { name = nombre + " (campaña runtime)", hideFlags = HideFlags.DontSave };
        if (material.HasProperty("_Modo")) material.SetFloat("_Modo", modo);
        materiales.Add(material);
        return material;
    }

    void PrepararTerreno(MeshRenderer renderer, bool conservarTextura)
    {
        if (renderer == null || renderer.sharedMaterial == null) return;
        var shader = Resources.Load<Shader>("BosqueArdienteSuelo");
        if (shader == null) return;
        var anterior = renderer.sharedMaterial;
        if (materiales.Contains(anterior))
        {
            anterior.SetFloat("_UsarTextura", conservarTextura ? 1 : 0);
            anterior.SetVector("_OrigenTerreno", zona.TexturaTerreno.bounds.center);
            return;
        }
        var material = CrearMaterial(shader, "Suelo carbonizado", 0);
        material.mainTexture = anterior.mainTexture;
        material.mainTextureScale = anterior.mainTextureScale;
        material.mainTextureOffset = anterior.mainTextureOffset;
        if (anterior.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", anterior.GetTexture("_BumpMap"));
        if (anterior.HasProperty("_Color")) material.color = anterior.color;
        material.SetFloat("_UsarTextura", conservarTextura ? 1 : 0);
        material.SetVector("_OrigenTerreno", zona.TexturaTerreno.bounds.center);
        renderer.sharedMaterial = material;
    }

    void RecolectarFocos(GameObject raiz)
    {
        if (raiz == null) return;
        // Solo se leen las posiciones para distribuir las particulas ambientales.
        foreach (var ps in raiz.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (OmitirEmisor(ps.transform)) continue;
            if (!DentroDelBosque(ps.transform.position)) continue;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var original = renderer != null ? renderer.sharedMaterial : null;
            if (original == null || original.name.IndexOf("Llama", System.StringComparison.OrdinalIgnoreCase) < 0
                || original.name.IndexOf("Espectral", System.StringComparison.OrdinalIgnoreCase) >= 0
                || ps.name.IndexOf("Humo", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
            AgregarFoco(ps.transform.position);
        }
    }

    void RefinarMaterialLlamas(GameObject raiz)
    {
        if (raiz == null) return;
        var shader = Resources.Load<Shader>("BosqueArdienteFuego");
        if (shader == null) return;
        foreach (var ps in raiz.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (OmitirEmisor(ps.transform)) continue;
            if (ps.name.IndexOf("Humo", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var original = renderer != null ? renderer.sharedMaterial : null;
            if (original == null || original.name.IndexOf("Llama", System.StringComparison.OrdinalIgnoreCase) < 0
                || original.name.IndexOf("Espectral", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
            if (!visibilidadFuego.TryGetValue(renderer, out var visible))
            { visible = renderer.enabled; visibilidadFuego.Add(renderer, visible); }
            renderer.enabled = visible && DentroDelBosque(ps.transform.position);
            ProfesionalizarEmisor(ps, renderer);
            if (materiales.Contains(original))
            {
                original.shader = shader;
                original.SetColor("_Color", Color.white);
                original.SetFloat("_Intensidad", .9f);
                continue;
            }
            if (!materialesLlamas.TryGetValue(original, out var refinado))
            {
                refinado = CrearMaterial(shader, original.name + " refinada", 0);
                refinado.SetColor("_Color", Color.white);
                refinado.SetFloat("_Intensidad", .9f);
                materialesLlamas.Add(original, refinado);
            }
            renderer.sharedMaterial = refinado;
        }
    }

    void DelimitarFuego()
    {
        if (zona.TexturaTerreno == null) return;
        var min = zona.TexturaTerreno.bounds.min; var max = zona.TexturaTerreno.bounds.max;
        min.x += .8f; max.x -= .8f; min.z += .8f; max.z -= .8f;
        float izquierda = float.PositiveInfinity, derecha = float.NegativeInfinity;
        foreach (var nodo in Object.FindObjectsByType<Nodo>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (nodo.gameObject.scene != zona.gameObject.scene) continue;
            var p = nodo.transform.position;
            if (p.x < min.x || p.x > max.x || p.z < min.z || p.z > max.z) continue;
            izquierda = Mathf.Min(izquierda, p.x); derecha = Mathf.Max(derecha, p.x);
        }
        if (derecha - izquierda > 1)
        { min.x = Mathf.Max(min.x, izquierda - .5f); max.x = Mathf.Min(max.x, derecha + .5f); }
        areaFuego.SetMinMax(min, max);
    }

    bool DentroDelBosque(Vector3 p) => p.x >= areaFuego.min.x && p.x <= areaFuego.max.x
        && p.z >= areaFuego.min.z && p.z <= areaFuego.max.z;

    bool OmitirEmisor(Transform objeto)
    {
        return objeto.GetComponentInParent<FondoBosqueArdiente>() != null
            || zona.pasovientoheladoContenedorGameObjects != null && objeto.IsChildOf(zona.pasovientoheladoContenedorGameObjects.transform)
            || zona.nedukazalContenedorGameObjects != null && objeto.IsChildOf(zona.nedukazalContenedorGameObjects.transform);
    }

    void ProfesionalizarEmisor(ParticleSystem ps, ParticleSystemRenderer renderer)
    {
        var main = ps.main;
        if (!medidasFuego.TryGetValue(ps, out var medidas))
        {
            medidas = new MedidasFuego { x = main.startSizeX,
                y = main.startSize3D ? main.startSizeY : main.startSizeX,
                z = main.startSize3D ? main.startSizeZ : main.startSizeX, velocidad = main.startSpeed };
            medidasFuego.Add(ps, medidas);
        }
        main.startSize3D = true;
        main.startSizeX = EscalarCurva(medidas.x, .82f);
        main.startSizeY = EscalarCurva(medidas.y, 1.12f);
        main.startSizeZ = EscalarCurva(medidas.z, .82f);
        main.startSpeed = EscalarCurva(medidas.velocidad, .55f);
        main.startRotation3D = false;
        main.startRotation = new ParticleSystem.MinMaxCurve(-.10f, .10f);
        main.startColor = Color.white;
        var rotacion = ps.rotationOverLifetime; rotacion.enabled = false;
        var color = ps.colorOverLifetime; color.enabled = true; color.color = FondoBosqueArdiente.GradienteFuego();
        var size = ps.sizeOverLifetime; size.enabled = true; size.separateAxes = false;
        size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .55f), new Keyframe(.22f, 1), new Keyframe(1, .15f)));
        var noise = ps.noise; noise.enabled = true; noise.strength = .055f;
        noise.frequency = .8f; noise.scrollSpeed = .32f; noise.quality = ParticleSystemNoiseQuality.Low;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.pivot = new Vector3(0, -.35f, 0);
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
    }

    static ParticleSystem.MinMaxCurve EscalarCurva(ParticleSystem.MinMaxCurve curva, float factor)
    {
        if (curva.mode == ParticleSystemCurveMode.Constant) curva.constant *= factor;
        else if (curva.mode == ParticleSystemCurveMode.TwoConstants)
        { curva.constantMin *= factor; curva.constantMax *= factor; }
        else curva.curveMultiplier *= factor;
        return curva;
    }

    void AgregarFoco(Vector3 posicion)
    {
        // Un emisor por grupo, no por cada lengua de fuego del prefab.
        foreach (var foco in focos)
            if ((foco - posicion).sqrMagnitude < 2.25f) return;
        if (focos.Count < 48) focos.Add(posicion);
    }

    void AjustarFocosAlSuelo()
    {
        var decorador = zona.GetComponent<MapDecorator>();
        for (int i = 0; i < focos.Count; i++)
        {
            Vector3 posicion = focos[i];
            if (decorador != null && decorador.TrySampleSurface(posicion, out var suelo, out _)) posicion = suelo;
            focos[i] = posicion + Vector3.up * .12f;
        }
        siguienteFoco = 0;
    }

    void PrepararAtmosfera()
    {
        if (raizParticulas != null || humo == null || zona.TexturaTerreno == null) return;
        raizParticulas = new GameObject("Atmosfera Bosque Ardiente (Runtime)");
        // La raiz del mapa tiene escala no uniforme; los tamaños de atmosfera son de mundo.
        raizParticulas.transform.SetParent(transform, true);
        var bounds = zona.TexturaTerreno.bounds;
        int calidad = Mathf.Clamp(PlayerPrefs.GetInt("graficos_index", QualitySettings.GetQualityLevel()), 0, 2);
        float factor = calidad == 0 ? .45f : calidad == 1 ? .7f : 1;
        var chispas = CrearSistema("Brasas de los incendios", brasa, 180, .045f, .09f, 2.5f, 4.8f);
        var humoLocal = CrearSistema("Humo que deriva entre arboles", humo, 50, .55f, 1.15f, 3.5f, 6);
        var cenizas = CrearSistema("Ceniza tenue del bosque", ceniza, 95, .025f, .055f, 9, 15);
        ConfigurarMovimiento(chispas, new Vector3(-.22f,.34f,.055f), new Vector3(-.10f,.8f,.11f), .12f);
        ConfigurarMovimiento(humoLocal, new Vector3(-.16f,.055f,.04f), new Vector3(-.08f,.12f,.08f), .085f);
        ConfigurarMovimiento(cenizas, new Vector3(-.14f,-.045f,.035f), new Vector3(-.06f,-.015f,.07f), .075f);
        ConfigurarColor(chispas, new Color(1,.56f,.10f), new Color(.8f,.11f,.01f), .8f);
        ConfigurarColor(humoLocal, new Color(.29f,.265f,.235f), new Color(.24f,.245f,.25f), .045f);
        ConfigurarColor(cenizas, new Color(.62f,.58f,.51f), new Color(.38f,.35f,.30f), .30f);
        // Emision explicita distribuida sobre focos reales; semilla propia para no alterar encuentros.
        var aleatorio = new System.Random(9317);
        ConfigurarCaja(cenizas, bounds.center + Vector3.up * 1.8f, new Vector3(bounds.size.x*.88f, 2.5f, bounds.size.z*.88f), 5*factor);
        // Dos sistemas compartidos bastan para todo el mapa, sin luces adicionales por particula.
        var emision = chispas.emission; emision.enabled = false;
        emision = humoLocal.emission; emision.enabled = false;
        rng = aleatorio;
        factorCalidad = factor;
        foreach (var ps in sistemasAmbiente) ps.Play(false);
    }

    void PrepararIluminacion()
    {
        if (!iluminacionPreparada)
        {
            iluminacionPreparada = true;
            solBosque = GetComponentInChildren<SunController>(true);
            if (solBosque != null)
            {
                // Conserva intensidades y horarios del inspector; reduce apenas el rosa de noche.
                solBosque.nightColor = Color.Lerp(solBosque.nightColor, new Color(.49f, .53f, .59f), .18f);
                solBosque.nightAmbientColor = Color.Lerp(solBosque.nightAmbientColor, new Color(.48f, .46f, .43f), .18f);
                var luz = solBosque.sun != null ? solBosque.sun : solBosque.GetComponent<Light>();
                if (luz != null) luz.shadowStrength = Mathf.Min(luz.shadowStrength, .78f);
                solBosque.SetCampaignHour(CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerHoraActual() : 8);
            }
        }
        int calidad = Mathf.Clamp(PlayerPrefs.GetInt("graficos_index", QualitySettings.GetQualityLevel()), 0, 2);
        int cantidad = Mathf.Min(focos.Count, calidad == 0 ? 1 : calidad == 1 ? 2 : 4);
        for (int i = 0; i < cantidad; i++)
        {
            if (i == lucesIncendio.Count)
            {
                if (raizLuces == null)
                {
                    raizLuces = new GameObject("Relleno cálido de incendios (Runtime)");
                    raizLuces.transform.SetParent(transform, true);
                }
                var go = new GameObject("Luz cálida de incendio " + (i + 1));
                go.transform.SetParent(raizLuces.transform, true);
                var luz = go.AddComponent<Light>();
                luz.type = LightType.Point; luz.color = new Color(1, .62f, .27f);
                luz.range = 2.6f; luz.shadows = LightShadows.None;
                luz.renderMode = LightRenderMode.ForceVertex;
                luz.cullingMask = 1 << zona.TexturaTerreno.gameObject.layer;
                lucesIncendio.Add(luz);
            }
            lucesIncendio[i].transform.position = focos[i * focos.Count / cantidad] + Vector3.up * .3f;
        }
        for (int i = 0; i < lucesIncendio.Count; i++) lucesIncendio[i].enabled = i < cantidad;
    }

    void ActualizarLuzAmbiente()
    {
        float hora = CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerHoraActual() : 13;
        float dia = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(5, 9, hora))
            * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(17, 21, hora)));
        for (int i = 0; i < lucesIncendio.Count; i++)
            if (lucesIncendio[i] != null)
                lucesIncendio[i].intensity = Mathf.Lerp(.40f, .25f, dia)
                    * (1 + Mathf.Sin(reloj * .73f + i * 2.1f) * .075f + Mathf.Sin(reloj * 1.13f + i) * .025f);
        if (humo != null) humo.SetColor("_Color", Color.Lerp(new Color(.72f, .67f, .65f), Color.white, dia));
        if (ceniza != null) ceniza.SetColor("_Color", Color.Lerp(new Color(.63f, .61f, .60f), Color.white, dia));
        if (fondo != null) fondo.Actualizar(reloj, dia);
    }

    System.Random rng;
    float factorCalidad = 1, acumuladorBrasas, acumuladorHumo;
    int siguienteFoco;

    ParticleSystem CrearSistema(string nombre, Material material, int maximo, float minTam, float maxTam, float minVida, float maxVida)
    {
        var go = new GameObject(nombre) { layer = zona.TexturaTerreno.gameObject.layer };
        go.transform.SetParent(raizParticulas.transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.useAutoRandomSeed = false;
        ps.randomSeed = (uint)(317 + sistemasAmbiente.Count * 83);
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.maxParticles = maximo;
        main.startSpeed = 0;
        main.startSize = new ParticleSystem.MinMaxCurve(minTam, maxTam);
        main.startLifetime = new ParticleSystem.MinMaxCurve(minVida, maxVida);
        main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI*2);
        var shape = ps.shape; shape.enabled = false;
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .6f, 1, material == humo ? 2.2f : .3f));
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = material;
        r.shadowCastingMode = ShadowCastingMode.Off;
        r.receiveShadows = false;
        r.sortMode = ParticleSystemSortMode.Distance;
        sistemasAmbiente.Add(ps);
        return ps;
    }

    static void ConfigurarCaja(ParticleSystem ps, Vector3 centro, Vector3 escala, float cantidad)
    {
        ps.transform.position = centro;
        var shape = ps.shape; shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = escala;
        var emission = ps.emission; emission.rateOverTime = cantidad;
    }

    static void ConfigurarMovimiento(ParticleSystem ps, Vector3 minimo, Vector3 maximo, float turbulencia)
    {
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true; velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(minimo.x,maximo.x);
        velocity.y = new ParticleSystem.MinMaxCurve(minimo.y,maximo.y);
        velocity.z = new ParticleSystem.MinMaxCurve(minimo.z,maximo.z);
        var noise = ps.noise;
        noise.enabled = true; noise.strength = turbulencia; noise.frequency = .28f; noise.scrollSpeed = .065f;
        noise.quality = ParticleSystemNoiseQuality.Low;
    }

    static Gradient Gradiente(Color inicio, Color fin, float alpha)
    {
        var gradiente = new Gradient();
        gradiente.SetKeys(new[] { new GradientColorKey(inicio,0), new GradientColorKey(fin,1) },
            new[] { new GradientAlphaKey(0,0), new GradientAlphaKey(alpha,.16f), new GradientAlphaKey(alpha*.7f,.55f), new GradientAlphaKey(0,1) });
        return gradiente;
    }

    static void ConfigurarColor(ParticleSystem ps, Color inicio, Color fin, float alpha)
    {
        var color = ps.colorOverLifetime; color.enabled = true; color.color = Gradiente(inicio,fin,alpha);
    }

    void Update()
    {
        if (zona == null || zona.ID != 1) return;
        Avanzar(Time.deltaTime);
    }

    public void Avanzar(float dt)
    {
        if (dt <= 0 || raizParticulas == null) return;
        reloj += dt;
        foreach (var material in materiales) if (material != null) material.SetFloat(RelojId,reloj);
        ActualizarLuzAmbiente();
        if (focos.Count == 0) return;
        float rafaga = .84f + .16f * Mathf.Sin(reloj * .19f);
        acumuladorBrasas += dt * Mathf.Min(24, focos.Count*1.5f) * factorCalidad * rafaga;
        acumuladorHumo += dt * Mathf.Min(4, focos.Count*.18f) * factorCalidad * (.9f + .1f * Mathf.Sin(reloj * .11f));
        EmitirDesdeFocos(sistemasAmbiente[0], ref acumuladorBrasas, true);
        EmitirDesdeFocos(sistemasAmbiente[1], ref acumuladorHumo, false);
    }

    void EmitirDesdeFocos(ParticleSystem ps, ref float acumulador, bool esBrasa)
    {
        // Limita la recuperacion tras un frame lento; no crea rafagas al volver al mapa.
        int cantidad = Mathf.Min(6, Mathf.FloorToInt(acumulador));
        acumulador = Mathf.Min(1, acumulador-cantidad);
        for (int i=0; i<cantidad; i++)
        {
            Vector3 posicion = focos[siguienteFoco++ % focos.Count];
            posicion += new Vector3((float)rng.NextDouble()-.5f, esBrasa ? .08f : .35f, (float)rng.NextDouble()-.5f)*.45f;
            ps.Emit(new ParticleSystem.EmitParams { position = posicion },1);
        }
    }

    void OnDisable()
    {
        if (raizLuces != null) raizLuces.SetActive(false);
        if (fondo != null) fondo.gameObject.SetActive(false);
        if (raizParticulas == null) return;
        foreach (var ps in sistemasAmbiente)
            if (ps != null) ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        acumuladorBrasas = acumuladorHumo = 0;
    }

    void OnEnable()
    {
        if (raizLuces != null) raizLuces.SetActive(true);
        if (fondo != null) fondo.gameObject.SetActive(true);
        foreach (var ps in sistemasAmbiente) if (ps != null) ps.Play(false);
    }

    void OnDestroy()
    {
        foreach (var par in visibilidadFuego) if (par.Key != null) par.Key.enabled = par.Value;
        if (raizLuces != null) { if (Application.isPlaying) Destroy(raizLuces); else DestroyImmediate(raizLuces); }
        if (fondo != null) { if (Application.isPlaying) Destroy(fondo.gameObject); else DestroyImmediate(fondo.gameObject); }
        foreach (var material in materiales)
            if (material != null) { if (Application.isPlaying) Destroy(material); else DestroyImmediate(material); }
    }
}
