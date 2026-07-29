using UnityEngine;

/// <summary>
/// Reequilibra las capas existentes del Aliento Negro sin modificar la escena.
/// Conserva el aspecto original y agrega un centro denso y un velo frontal
/// progresivo mucho más tenue.
/// </summary>
[DisallowMultipleComponent]
public sealed class AlientoNegroVolumenRuntime : MonoBehaviour
{
    const string NombreCapaCuerpo = "ALIENTONEGRO (1)";
    const string NombreRetaguardia = "AlientoNegroRetaguardiaInfinitaRuntime";
    const string NombreVolumenCentral = "AlientoNegroVolumenCentralRuntime";
    const string NombreNucleosDensos = "AlientoNegroNucleosTurbiosRuntime";
    const string NombreVeloFrontal = "AlientoNegroVeloFrontalRuntime";
    const string RutaShader = "AlientoNegroVolumetrico";
    const string NombreShader = "GDD/Aliento Negro Volumetrico";
    const float AlphaReferenciaMaterial = 0.007843138f;

    static Shader shaderVolumetrico;

    ParticleSystem sistemaNucleo;
    ParticleSystem sistemaCuerpo;
    ParticleSystem sistemaRetaguardia;
    ParticleSystem sistemaVolumenCentral;
    ParticleSystem sistemaNucleosDensos;
    ParticleSystem sistemaVelo;
    ParticleSystemRenderer rendererCuerpo;
    ParticleSystemRenderer rendererRetaguardia;
    ParticleSystemRenderer rendererVolumenCentral;
    ParticleSystemRenderer rendererNucleosDensos;
    ParticleSystemRenderer rendererVelo;
    Material materialFuenteVolumenCentral;
    Material materialFuenteNucleosDensos;
    Material materialFuenteRetaguardia;
    Material materialFuenteVelo;
    Material materialRetaguardiaRuntime;
    Material materialVolumenCentralRuntime;
    Material materialNucleosDensosRuntime;
    Material materialVeloRuntime;
    Texture2D texturaRuidoRuntime;
    bool configuracionAplicada;
    float proximaRevisionMateriales;

    public static AlientoNegroVolumenRuntime Ensure(GameObject root, ParticleSystem sistemaPrincipal)
    {
        if (root == null)
        {
            return null;
        }

        AlientoNegroVolumenRuntime mejora = root.GetComponent<AlientoNegroVolumenRuntime>();
        if (mejora == null)
        {
            mejora = root.AddComponent<AlientoNegroVolumenRuntime>();
        }

        mejora.Inicializar(sistemaPrincipal);
        return mejora;
    }

    void Awake()
    {
        Inicializar(GetComponent<ParticleSystem>());
    }

    void LateUpdate()
    {
        if (!configuracionAplicada)
        {
            Inicializar(sistemaNucleo != null ? sistemaNucleo : GetComponent<ParticleSystem>());
        }

        if (Time.unscaledTime < proximaRevisionMateriales)
        {
            return;
        }

        proximaRevisionMateriales = Time.unscaledTime + 0.25f;
        ActualizarMaterialesSiCambioLaRegion();
    }

    public void Inicializar(ParticleSystem sistemaPrincipal)
    {
        if (sistemaPrincipal != null)
        {
            sistemaNucleo = sistemaPrincipal;
        }

        if (sistemaNucleo == null)
        {
            return;
        }

        Transform transformCuerpo = transform.Find(NombreCapaCuerpo);
        sistemaCuerpo = transformCuerpo != null
            ? transformCuerpo.GetComponent<ParticleSystem>()
            : null;

        rendererCuerpo = sistemaCuerpo != null
            ? sistemaCuerpo.GetComponent<ParticleSystemRenderer>()
            : null;

        if (!configuracionAplicada)
        {
            texturaRuidoRuntime = CrearTexturaRuido();
            ConfigurarNucleo(sistemaNucleo);
            if (sistemaCuerpo != null)
            {
                ConfigurarCuerpo(sistemaCuerpo);
                sistemaRetaguardia = ObtenerOCrearCopia(
                    sistemaCuerpo,
                    NombreRetaguardia);
                if (sistemaRetaguardia != null)
                {
                    ConfigurarRetaguardia(sistemaRetaguardia, sistemaCuerpo);
                    rendererRetaguardia = sistemaRetaguardia.GetComponent<ParticleSystemRenderer>();
                }

                sistemaVolumenCentral = ObtenerOCrearCopia(
                    sistemaCuerpo,
                    NombreVolumenCentral);
                if (sistemaVolumenCentral != null)
                {
                    ConfigurarVolumenCentral(sistemaVolumenCentral, sistemaCuerpo);
                    rendererVolumenCentral = sistemaVolumenCentral.GetComponent<ParticleSystemRenderer>();
                }

                sistemaNucleosDensos = ObtenerOCrearCopia(
                    sistemaCuerpo,
                    NombreNucleosDensos);
                if (sistemaNucleosDensos != null)
                {
                    ConfigurarNucleosDensos(sistemaNucleosDensos, sistemaCuerpo);
                    rendererNucleosDensos = sistemaNucleosDensos.GetComponent<ParticleSystemRenderer>();
                }

                sistemaVelo = ObtenerOCrearCopia(sistemaCuerpo, NombreVeloFrontal);
                if (sistemaVelo != null)
                {
                    ConfigurarVeloFrontal(sistemaVelo, sistemaCuerpo);
                    rendererVelo = sistemaVelo.GetComponent<ParticleSystemRenderer>();
                }
            }

            configuracionAplicada = true;
        }
        else if (sistemaRetaguardia == null
            || sistemaVolumenCentral == null
            || sistemaNucleosDensos == null
            || sistemaVelo == null)
        {
            Transform transformRetaguardia = transform.Find(NombreRetaguardia);
            sistemaRetaguardia = transformRetaguardia != null
                ? transformRetaguardia.GetComponent<ParticleSystem>()
                : null;
            rendererRetaguardia = sistemaRetaguardia != null
                ? sistemaRetaguardia.GetComponent<ParticleSystemRenderer>()
                : null;

            Transform transformVolumenCentral = transform.Find(NombreVolumenCentral);
            sistemaVolumenCentral = transformVolumenCentral != null
                ? transformVolumenCentral.GetComponent<ParticleSystem>()
                : null;
            rendererVolumenCentral = sistemaVolumenCentral != null
                ? sistemaVolumenCentral.GetComponent<ParticleSystemRenderer>()
                : null;

            Transform transformNucleosDensos = transform.Find(NombreNucleosDensos);
            sistemaNucleosDensos = transformNucleosDensos != null
                ? transformNucleosDensos.GetComponent<ParticleSystem>()
                : null;
            rendererNucleosDensos = sistemaNucleosDensos != null
                ? sistemaNucleosDensos.GetComponent<ParticleSystemRenderer>()
                : null;

            Transform transformVelo = transform.Find(NombreVeloFrontal);
            sistemaVelo = transformVelo != null
                ? transformVelo.GetComponent<ParticleSystem>()
                : null;
            rendererVelo = sistemaVelo != null
                ? sistemaVelo.GetComponent<ParticleSystemRenderer>()
                : null;
        }

        ActualizarMaterialesSiCambioLaRegion();
    }

    static void ConfigurarNucleo(ParticleSystem sistema)
    {
        var main = sistema.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(8f, 10f);
        main.maxParticles = 6000;

        var shape = sistema.shape;
        Vector3 escala = shape.scale;
        escala.y = 0.7f;
        shape.scale = escala;

        var noise = sistema.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = new ParticleSystem.MinMaxCurve(0.5f);
        noise.strengthY = new ParticleSystem.MinMaxCurve(0.18f);
        noise.strengthZ = new ParticleSystem.MinMaxCurve(0.7f);
        noise.frequency = 0.45f;
        noise.damping = true;
        noise.octaveCount = 2;
        noise.octaveMultiplier = 0.48f;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.045f);
        noise.positionAmount = 0.45f;
    }

    static void ConfigurarCuerpo(ParticleSystem sistema)
    {
        var main = sistema.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 15f);
        main.startSize = new ParticleSystem.MinMaxCurve(18f, 24f);
        main.maxParticles = 7000;

        var noise = sistema.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = new ParticleSystem.MinMaxCurve(0.3f);
        noise.strengthY = new ParticleSystem.MinMaxCurve(0.1f);
        noise.strengthZ = new ParticleSystem.MinMaxCurve(0.5f);
        noise.frequency = 0.3f;
        noise.damping = true;
        noise.octaveCount = 1;
        noise.octaveMultiplier = 0.5f;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.015f);
        noise.positionAmount = 1f;
    }

    ParticleSystem ObtenerOCrearCopia(ParticleSystem fuente, string nombre)
    {
        Transform existente = transform.Find(nombre);
        if (existente != null)
        {
            return existente.GetComponent<ParticleSystem>();
        }

        GameObject copia = Instantiate(fuente.gameObject, transform);
        copia.name = nombre;
        copia.transform.localPosition = fuente.transform.localPosition;
        copia.transform.localRotation = fuente.transform.localRotation;
        copia.transform.localScale = fuente.transform.localScale;
        return copia.GetComponent<ParticleSystem>();
    }

    void ConfigurarRetaguardia(ParticleSystem retaguardia, ParticleSystem cuerpo)
    {
        var main = retaguardia.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(13f, 20f);
        main.startSize = new ParticleSystem.MinMaxCurve(22f, 30f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpeed = 0.42f;
        main.maxParticles = 1000;

        var emission = retaguardia.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(165f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeRetaguardia = retaguardia.shape;
        Vector3 escala = shapeCuerpo.scale;
        escala.x *= 1.12f;
        escala.z *= 1.08f;
        shapeRetaguardia.scale = escala;
        shapeRetaguardia.position = shapeCuerpo.position;

        var noise = retaguardia.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = new ParticleSystem.MinMaxCurve(0.25f);
        noise.strengthY = new ParticleSystem.MinMaxCurve(0.08f);
        noise.strengthZ = new ParticleSystem.MinMaxCurve(0.35f);
        noise.frequency = 0.22f;
        noise.damping = true;
        noise.octaveCount = 1;
        noise.quality = ParticleSystemNoiseQuality.Low;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.01f);
        noise.positionAmount = 0.55f;

        AplicarFadeDeVida(retaguardia, 0.3f, 0.22f);
        DesplazarRetaguardiaHaciaIzquierda(retaguardia.transform, cuerpo.transform);
        ConfigurarRendererCopia(retaguardia, cuerpo, 1);
    }

    void DesplazarRetaguardiaHaciaIzquierda(
        Transform retaguardia,
        Transform cuerpo)
    {
        Camera camara = Camera.main;
        Vector3 izquierdaMundo = camara != null
            ? -camara.transform.right
            : -transform.right;
        izquierdaMundo = Vector3.ProjectOnPlane(izquierdaMundo, Vector3.up);
        if (izquierdaMundo.sqrMagnitude < 0.001f)
        {
            izquierdaMundo = -transform.right;
        }

        Vector3 izquierdaLocal = transform.InverseTransformDirection(
            izquierdaMundo.normalized);
        izquierdaLocal.y = 0f;
        izquierdaLocal.Normalize();

        retaguardia.localPosition = cuerpo.localPosition + izquierdaLocal * 22f;
    }

    static void ConfigurarVolumenCentral(ParticleSystem volumen, ParticleSystem cuerpo)
    {
        var main = volumen.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 13f);
        main.startSize = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 1800;

        var emission = volumen.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(620f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeVolumen = volumen.shape;
        Vector3 escala = shapeVolumen.scale;
        escala.x = 8f;
        escala.y = 1.8f;
        shapeVolumen.scale = escala;
        shapeVolumen.position = shapeCuerpo.position;

        var noise = volumen.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = new ParticleSystem.MinMaxCurve(0.75f);
        noise.strengthY = new ParticleSystem.MinMaxCurve(0.38f);
        noise.strengthZ = new ParticleSystem.MinMaxCurve(1f);
        noise.frequency = 0.55f;
        noise.damping = true;
        noise.octaveCount = 2;
        noise.octaveMultiplier = 0.48f;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.09f);
        noise.positionAmount = 1f;

        AplicarFadeDeVida(volumen, 0.74f, 0.52f);
        ConfigurarRendererCopia(volumen, cuerpo, 2);
    }

    static void ConfigurarVeloFrontal(ParticleSystem velo, ParticleSystem cuerpo)
    {
        var main = velo.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 11f);
        main.startSize = new ParticleSystem.MinMaxCurve(7f, 13f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 900;

        var emission = velo.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(260f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeVelo = velo.shape;
        Vector3 escala = shapeVelo.scale;
        escala.x = 5.5f;
        escala.y = 1f;
        shapeVelo.scale = escala;

        Vector3 posicion = shapeVelo.position;
        posicion.x = shapeCuerpo.position.x - Mathf.Abs(shapeCuerpo.scale.x) * 0.5f + 0.6f;
        shapeVelo.position = posicion;

        var noise = velo.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = new ParticleSystem.MinMaxCurve(0.5f);
        noise.strengthY = new ParticleSystem.MinMaxCurve(0.22f);
        noise.strengthZ = new ParticleSystem.MinMaxCurve(0.75f);
        noise.frequency = 0.45f;
        noise.damping = true;
        noise.octaveCount = 2;
        noise.octaveMultiplier = 0.45f;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.06f);
        noise.positionAmount = 1f;

        AplicarFadeDeVida(velo, 0.32f, 0.18f);
        ConfigurarRendererCopia(velo, cuerpo, 3);

    }

    static void ConfigurarNucleosDensos(ParticleSystem nucleos, ParticleSystem cuerpo)
    {
        var main = nucleos.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(9f, 16f);
        main.startSize = new ParticleSystem.MinMaxCurve(4.5f, 9.5f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 800;

        var emission = nucleos.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(145f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeNucleos = nucleos.shape;
        Vector3 escala = shapeNucleos.scale;
        escala.x = 6.5f;
        escala.y = 2.8f;
        shapeNucleos.scale = escala;
        shapeNucleos.position = shapeCuerpo.position + new Vector3(0.35f, 0f, 0f);

        var velocity = nucleos.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.12f, -0.04f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.035f, 0.065f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.025f, 0.025f);

        var noise = nucleos.noise;
        noise.enabled = true;
        noise.separateAxes = true;
        noise.strengthX = new ParticleSystem.MinMaxCurve(0.95f);
        noise.strengthY = new ParticleSystem.MinMaxCurve(0.6f);
        noise.strengthZ = new ParticleSystem.MinMaxCurve(1.35f);
        noise.frequency = 0.62f;
        noise.damping = true;
        noise.octaveCount = 2;
        noise.octaveMultiplier = 0.52f;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(0.07f);
        noise.positionAmount = 1.15f;

        AplicarFadeDeVida(nucleos, 0.88f, 0.62f);
        ConfigurarRendererCopia(nucleos, cuerpo, 4);
    }

    static void AplicarFadeDeVida(ParticleSystem sistema, float alphaCentro, float alphaFinal)
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(alphaCentro, 0.2f),
                new GradientAlphaKey(alphaFinal, 0.78f),
                new GradientAlphaKey(0f, 1f)
            });

        var colorOverLifetime = sistema.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    static void ConfigurarRendererCopia(
        ParticleSystem copia,
        ParticleSystem cuerpo,
        int offsetOrden)
    {
        ParticleSystemRenderer rendererCuerpoLocal = cuerpo.GetComponent<ParticleSystemRenderer>();
        ParticleSystemRenderer rendererCopia = copia.GetComponent<ParticleSystemRenderer>();
        if (rendererCuerpoLocal != null && rendererCopia != null)
        {
            rendererCopia.renderMode = ParticleSystemRenderMode.Billboard;
            rendererCopia.alignment = ParticleSystemRenderSpace.View;
            rendererCopia.sortingOrder = rendererCuerpoLocal.sortingOrder + offsetOrden;
            rendererCopia.sortingFudge = rendererCuerpoLocal.sortingFudge + offsetOrden;
        }

        if (copia.gameObject.activeInHierarchy)
        {
            copia.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            copia.Play(true);
        }
    }

    void ActualizarMaterialesSiCambioLaRegion()
    {
        if (shaderVolumetrico == null)
        {
            shaderVolumetrico = Resources.Load<Shader>(RutaShader);
            if (shaderVolumetrico == null)
            {
                shaderVolumetrico = Shader.Find(NombreShader);
            }
        }

        if (shaderVolumetrico == null)
        {
            return;
        }

        Material fuente = rendererCuerpo != null
            ? rendererCuerpo.sharedMaterial
            : null;

        ActualizarMaterialCapa(
            sistemaRetaguardia,
            rendererRetaguardia,
            fuente,
            shaderVolumetrico,
            PerfilCapa.Retaguardia,
            ref materialFuenteRetaguardia,
            ref materialRetaguardiaRuntime);
        ActualizarMaterialCapa(
            sistemaVolumenCentral,
            rendererVolumenCentral,
            fuente,
            shaderVolumetrico,
            PerfilCapa.Centro,
            ref materialFuenteVolumenCentral,
            ref materialVolumenCentralRuntime);
        ActualizarMaterialCapa(
            sistemaNucleosDensos,
            rendererNucleosDensos,
            fuente,
            shaderVolumetrico,
            PerfilCapa.NucleosDensos,
            ref materialFuenteNucleosDensos,
            ref materialNucleosDensosRuntime);
        ActualizarMaterialCapa(
            sistemaVelo,
            rendererVelo,
            fuente,
            shaderVolumetrico,
            PerfilCapa.Velo,
            ref materialFuenteVelo,
            ref materialVeloRuntime);
    }

    enum PerfilCapa
    {
        Retaguardia,
        Centro,
        NucleosDensos,
        Velo
    }

    void ActualizarMaterialCapa(
        ParticleSystem sistema,
        ParticleSystemRenderer rendererParticulas,
        Material fuente,
        Shader shader,
        PerfilCapa perfil,
        ref Material materialFuente,
        ref Material materialRuntime)
    {
        if (sistema == null || rendererParticulas == null || fuente == null)
        {
            return;
        }

        if (materialFuente == fuente && rendererParticulas.sharedMaterial == materialRuntime)
        {
            return;
        }

        if (materialRuntime != null)
        {
            Destroy(materialRuntime);
        }

        materialFuente = fuente;
        materialRuntime = new Material(shader)
        {
            name = fuente.name + " (Volumetrico Runtime)",
            hideFlags = HideFlags.DontSave
        };

        if (fuente.HasProperty("_MainTex"))
        {
            materialRuntime.SetTexture("_MainTex", fuente.GetTexture("_MainTex"));
            materialRuntime.SetTextureScale("_MainTex", fuente.GetTextureScale("_MainTex"));
            materialRuntime.SetTextureOffset("_MainTex", fuente.GetTextureOffset("_MainTex"));
        }

        Color colorFuente = fuente.HasProperty("_Color")
            ? fuente.GetColor("_Color")
            : new Color(0.03f, 0.25f, 0.18f, AlphaReferenciaMaterial);
        Color colorEmision = fuente.HasProperty("_EmissionColor")
            ? fuente.GetColor("_EmissionColor")
            : colorFuente;
        if (colorEmision.maxColorComponent <= 0.001f)
        {
            colorEmision = colorFuente;
        }

        colorEmision.a = 1f;
        float escalaAlphaRegional = Mathf.Clamp(
            colorFuente.a / AlphaReferenciaMaterial,
            0.65f,
            2.35f);

        materialRuntime.SetColor("_TintColor", colorEmision);
        materialRuntime.SetTexture("_NoiseTex", texturaRuidoRuntime);
        if (perfil == PerfilCapa.Velo)
        {
            ConfigurarMaterial(
                materialRuntime,
                0.01f * escalaAlphaRegional,
                2.25f,
                0.68f,
                0.47f,
                0.25f,
                0.035f,
                0.055f,
                0.14f,
                0.06f,
                0.02f,
                0f,
                0f);
        }
        else if (perfil == PerfilCapa.NucleosDensos)
        {
            ConfigurarMaterial(
                materialRuntime,
                0.046f * escalaAlphaRegional,
                1.65f,
                0.38f,
                0.39f,
                0.24f,
                0.025f,
                0.075f,
                0.95f,
                0.48f,
                0.28f,
                0.68f,
                0.42f);
        }
        else if (perfil == PerfilCapa.Retaguardia)
        {
            ConfigurarMaterial(
                materialRuntime,
                0.016f * escalaAlphaRegional,
                1.35f,
                0.24f,
                0.36f,
                0.3f,
                0.012f,
                0.025f,
                0.18f,
                0.08f,
                0.03f,
                0.08f,
                0f);
        }
        else
        {
            ConfigurarMaterial(
                materialRuntime,
                0.032f * escalaAlphaRegional,
                2.8f,
                0.54f,
                0.43f,
                0.22f,
                0.055f,
                0.045f,
                0.58f,
                0.24f,
                0.12f,
                0.2f,
                0.08f);
        }

        rendererParticulas.sharedMaterial = materialRuntime;
    }

    static void ConfigurarMaterial(
        Material material,
        float densidad,
        float escalaRuido,
        float ruptura,
        float umbralRuido,
        float suavidadRuido,
        float velocidadRuido,
        float distorsion,
        float relieve,
        float autosombra,
        float bordeFrio,
        float nucleoOscuro,
        float pulsoNecrotico)
    {
        material.SetFloat("_Density", densidad);
        material.SetFloat("_NoiseScale", escalaRuido);
        material.SetFloat("_NoiseStrength", ruptura);
        material.SetFloat("_NoiseCutoff", umbralRuido);
        material.SetFloat("_NoiseSoftness", suavidadRuido);
        material.SetFloat("_NoiseSpeed", velocidadRuido);
        material.SetFloat("_Distortion", distorsion);
        material.SetFloat("_VolumeLight", relieve);
        material.SetFloat("_CoreShadow", autosombra);
        material.SetFloat("_RimStrength", bordeFrio);
        material.SetFloat("_DarkCore", nucleoOscuro);
        material.SetFloat("_NecroPulse", pulsoNecrotico);
    }

    void OnDestroy()
    {
        if (materialVolumenCentralRuntime != null)
        {
            Destroy(materialVolumenCentralRuntime);
        }

        if (materialVeloRuntime != null)
        {
            Destroy(materialVeloRuntime);
        }

        if (materialNucleosDensosRuntime != null)
        {
            Destroy(materialNucleosDensosRuntime);
        }

        if (materialRetaguardiaRuntime != null)
        {
            Destroy(materialRetaguardiaRuntime);
        }

        if (texturaRuidoRuntime != null)
        {
            Destroy(texturaRuidoRuntime);
        }
    }

    static Texture2D CrearTexturaRuido()
    {
        const int tamano = 64;
        var textura = new Texture2D(tamano, tamano, TextureFormat.RGBA32, false, true)
        {
            name = "Aliento Negro - Ruido Volumetrico Runtime",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Bilinear,
            anisoLevel = 0,
            hideFlags = HideFlags.DontSave
        };

        var colores = new Color[tamano * tamano];
        for (int y = 0; y < tamano; y++)
        {
            float v = y / (float)tamano;
            for (int x = 0; x < tamano; x++)
            {
                float u = x / (float)tamano;
                float bajo = RuidoTileable(u, v, 3.2f, 1.7f);
                float medio = RuidoTileable(u, v, 6.4f, 13.1f);
                float detalle = RuidoTileable(u, v, 10.8f, 27.4f);
                colores[y * tamano + x] = new Color(bajo, medio, detalle, 1f);
            }
        }

        textura.SetPixels(colores);
        textura.Apply(false, true);
        return textura;
    }

    static float RuidoTileable(float u, float v, float escala, float offset)
    {
        float x = u * escala;
        float y = v * escala;
        float a = Mathf.PerlinNoise(x + offset, y + offset);
        float b = Mathf.PerlinNoise(x - escala + offset, y + offset);
        float c = Mathf.PerlinNoise(x + offset, y - escala + offset);
        float d = Mathf.PerlinNoise(x - escala + offset, y - escala + offset);
        return Mathf.Lerp(
            Mathf.Lerp(a, b, u),
            Mathf.Lerp(c, d, u),
            v);
    }
}
