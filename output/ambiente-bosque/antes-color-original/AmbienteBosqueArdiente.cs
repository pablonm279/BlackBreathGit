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
    AtributosZona zona;
    Material humo, brasa, ceniza;
    GameObject raizParticulas;
    float reloj;
    static readonly int RelojId = Shader.PropertyToID("_Reloj");

    public static void Preparar(AtributosZona atributos, bool conservarTextura)
    {
        if (atributos == null || atributos.ID != 1 || atributos.bosqueardienteContenedorGameObjects == null) return;
        var raiz = atributos.bosqueardienteContenedorGameObjects;
        var ambiente = raiz.GetComponent<AmbienteBosqueArdiente>();
        if (ambiente == null) ambiente = raiz.AddComponent<AmbienteBosqueArdiente>();
        ambiente.zona = atributos;
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
        var shader = Resources.Load<Shader>("BosqueArdienteLlamas");
        if (shader == null) return;
        foreach (var ps in raiz.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps.name.IndexOf("Humo", System.StringComparison.OrdinalIgnoreCase) >= 0) continue;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            var original = renderer != null ? renderer.sharedMaterial : null;
            if (original == null || materiales.Contains(original)
                || original.name.IndexOf("Llama", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
            if (!materialesLlamas.TryGetValue(original, out var refinado))
            {
                refinado = CrearMaterial(shader, original.name + " refinada", 0);
                refinado.mainTexture = original.mainTexture;
                refinado.mainTextureScale = original.mainTextureScale;
                refinado.mainTextureOffset = original.mainTextureOffset;
                refinado.SetFloat("_Espectral", original.name.IndexOf("Espectral", System.StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0);
                materialesLlamas.Add(original, refinado);
            }
            // Solo cambia el sombreado: no se escriben modulos del emisor, transform ni LOD.
            renderer.sharedMaterial = refinado;
        }
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
        ConfigurarMovimiento(chispas, new Vector3(.13f,.34f,.04f), new Vector3(.38f,.8f,.18f), .18f);
        ConfigurarMovimiento(humoLocal, new Vector3(.11f,.055f,.015f), new Vector3(.22f,.12f,.055f), .08f);
        ConfigurarMovimiento(cenizas, new Vector3(.08f,-.045f,.02f), new Vector3(.20f,-.015f,.08f), .09f);
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
        noise.enabled = true; noise.strength = turbulencia; noise.frequency = .35f; noise.scrollSpeed = .12f;
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
        if (focos.Count == 0) return;
        acumuladorBrasas += dt * Mathf.Min(24, focos.Count*1.5f) * factorCalidad;
        acumuladorHumo += dt * Mathf.Min(4, focos.Count*.18f) * factorCalidad;
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
        if (raizParticulas == null) return;
        foreach (var ps in sistemasAmbiente)
            if (ps != null) ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        acumuladorBrasas = acumuladorHumo = 0;
    }

    void OnEnable()
    {
        foreach (var ps in sistemasAmbiente) if (ps != null) ps.Play(false);
    }

    void OnDestroy()
    {
        foreach (var material in materiales)
            if (material != null) { if (Application.isPlaying) Destroy(material); else DestroyImmediate(material); }
    }
}
