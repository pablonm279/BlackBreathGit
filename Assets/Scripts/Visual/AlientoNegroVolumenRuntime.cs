using UnityEngine;

/// <summary>
/// Unifica las capas del Aliento Negro en un frente de nubes con volumen,
/// autosombra y turbulencia compartida, conservando los tonos de cada región.
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

    // IDs cacheados: se escriben cada frame en LateUpdate y evitan hashear strings.
    static readonly int IdWorldToAliento = Shader.PropertyToID("_WorldToAliento");
    static readonly int IdDensityGradient = Shader.PropertyToID("_DensityGradient");
    static readonly int IdFootprint = Shader.PropertyToID("_Footprint");
    static readonly int IdStrength = Shader.PropertyToID("_Strength");

    static Shader shaderVolumetrico;

    ParticleSystem sistemaNucleo;
    ParticleSystem sistemaCuerpo;
    ParticleSystem sistemaRetaguardia;
    ParticleSystem sistemaVolumenCentral;
    ParticleSystem sistemaNucleosDensos;
    ParticleSystem sistemaVelo;
    ParticleSystemRenderer rendererNucleo;
    ParticleSystemRenderer rendererCuerpo;
    ParticleSystemRenderer rendererRetaguardia;
    ParticleSystemRenderer rendererVolumenCentral;
    ParticleSystemRenderer rendererNucleosDensos;
    ParticleSystemRenderer rendererVelo;
    Material materialFuenteVolumenCentral;
    Material materialFuenteNucleosDensos;
    Material materialFuenteRetaguardia;
    Material materialFuenteVelo;
    Material materialFuenteNucleo;
    Material materialFuenteCuerpo;
    Material materialNucleoRuntime;
    Material materialCuerpoRuntime;
    Material materialRetaguardiaRuntime;
    Material materialVolumenCentralRuntime;
    Material materialNucleosDensosRuntime;
    Material materialVeloRuntime;
    readonly Material[] materialesVolumen = new Material[6];
    Texture2D texturaRuidoRuntime;
    Material materialContactoSuelo;
    readonly MeshRenderer[] suelosContacto = new MeshRenderer[2];
    readonly MeshRenderer[] capasContacto = new MeshRenderer[2];
    float fuerzaContacto;
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

        // El campo de turbulencia sigue al frente incluso durante su avance.
        Matrix4x4 mundoAAliento = transform.worldToLocalMatrix;
        Vector4 gradiente = new Vector4(0f, 1f, 0f, 0f);
        if (sistemaCuerpo != null)
        {
            var forma = sistemaCuerpo.shape;
            Vector3 frente = transform.InverseTransformPoint(sistemaCuerpo.transform.TransformPoint(
                forma.position - Vector3.right * Mathf.Abs(forma.scale.x) * 0.5f));
            Vector3 fondo = transform.InverseTransformPoint(sistemaCuerpo.transform.TransformPoint(
                forma.position + Vector3.right * Mathf.Abs(forma.scale.x) * 0.5f));
            gradiente = new Vector4(frente.x, Mathf.Max(0.01f, fondo.x - frente.x), 0f, 0f);
        }
        foreach (Material material in materialesVolumen)
        {
            if (material != null)
            {
                material.SetMatrix(IdWorldToAliento, mundoAAliento);
                material.SetVector(IdDensityGradient, gradiente);
            }
        }
        ActualizarContactoSuelo(mundoAAliento);

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

        rendererNucleo = sistemaNucleo.GetComponent<ParticleSystemRenderer>();

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
        main.simulationSpeed = 0.56f;
        main.startSize = new ParticleSystem.MinMaxCurve(8f, 10f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 1000;

        var emission = sistema.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(160f);

        var shape = sistema.shape;
        Vector3 escala = shape.scale;
        escala.y = 2.6f;
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
        AplicarFadeDeVida(sistema, 0.85f, 0.5f);
        ConfigurarRendererCopia(sistema, sistema, 0);
    }

    static void ConfigurarCuerpo(ParticleSystem sistema)
    {
        var main = sistema.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 15f);
        main.simulationSpeed = 0.48f;
        main.startSize = new ParticleSystem.MinMaxCurve(18f, 24f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 900;

        var emission = sistema.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(110f);

        var shape = sistema.shape;
        Vector3 escala = shape.scale;
        escala.y = 5f;
        shape.scale = escala;

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
        AplicarFadeDeVida(sistema, 0.65f, 0.4f);
        ConfigurarRendererCopia(sistema, sistema, 1);
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
        main.simulationSpeed = 0.336f;
        main.maxParticles = 450;

        var emission = retaguardia.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(70f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeRetaguardia = retaguardia.shape;
        Vector3 escala = shapeCuerpo.scale;
        escala.x *= 1.28f;
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
        ConfigurarRendererCopia(retaguardia, cuerpo, -2);
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

        retaguardia.localPosition = cuerpo.localPosition + izquierdaLocal * 26f;
    }

    static void ConfigurarVolumenCentral(ParticleSystem volumen, ParticleSystem cuerpo)
    {
        var main = volumen.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 13f);
        main.startSize = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 700;

        var emission = volumen.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(120f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeVolumen = volumen.shape;
        Vector3 escala = shapeVolumen.scale;
        escala.x = 8f;
        escala.y = 5.6f;
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
        ConfigurarRendererCopia(volumen, cuerpo, 1);
    }

    static void ConfigurarVeloFrontal(ParticleSystem velo, ParticleSystem cuerpo)
    {
        var main = velo.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 11f);
        main.startSize = new ParticleSystem.MinMaxCurve(7f, 13f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.maxParticles = 350;

        var emission = velo.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(75f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeVelo = velo.shape;
        Vector3 escala = shapeVelo.scale;
        escala.x = 5.5f;
        escala.y = 2.4f;
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
        main.maxParticles = 450;

        var emission = nucleos.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(65f);
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(0f);

        var shapeCuerpo = cuerpo.shape;
        var shapeNucleos = nucleos.shape;
        Vector3 escala = shapeNucleos.scale;
        escala.x = 6.5f;
        escala.y = 6f;
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
        ConfigurarRendererCopia(nucleos, cuerpo, 2);
    }

    static void AplicarFadeDeVida(ParticleSystem sistema, float alphaCentro, float alphaFinal)
    {
        var main = sistema.main;
        // El avance pertenece al controlador del frente. Las partículas sólo
        // aportan deriva suave, sin velocidades heredadas que escapen de la nube.
        main.startSpeed = 0f;
        var velocity = sistema.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.06f, -0.025f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.015f, 0.025f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocity.orbitalX = velocity.orbitalY = velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
        velocity.radial = new ParticleSystem.MinMaxCurve(0f);
        velocity.speedModifier = new ParticleSystem.MinMaxCurve(1f);
        var force = sistema.forceOverLifetime;
        force.enabled = false;
        var vida = main.startLifetime;
        main.startLifetime = new ParticleSystem.MinMaxCurve(
            vida.constantMin * 1.5f, vida.constantMax * 1.5f);
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

        // Dejar margen incluso con la vida máxima evita recambios por tandas
        // cuando el emisor llena todos sus espacios disponibles.
        var emission = sistema.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Min(
            emission.rateOverTime.constant,
            main.maxParticles * 0.9f / main.startLifetime.constantMax));

        // La nube se disuelve por transparencia; no colapsa al final de su vida.
        var size = sistema.sizeOverLifetime;
        size.enabled = true;
        size.separateAxes = false;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.75f, 0f, 0f),
            new Keyframe(0.35f, 0.9f, 0.2f, 0.2f),
            new Keyframe(1f, 1f, 0f, 0f)));

        var noise = sistema.noise;
        noise.positionAmount = new ParticleSystem.MinMaxCurve(noise.positionAmount.constant * 0.65f);

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
                new GradientAlphaKey(alphaCentro * 0.25f, 0.14f),
                new GradientAlphaKey(alphaCentro, 0.35f),
                new GradientAlphaKey(alphaFinal, 0.58f),
                new GradientAlphaKey(alphaFinal * 0.5f, 0.74f),
                new GradientAlphaKey(alphaFinal * 0.15f, 0.88f),
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
            // Orden estable: al cruzarse dos nubes no se intercambia de golpe
            // cuál tapa a la otra. Las nuevas entran transparentes y salen igual.
            rendererCopia.sortMode = ParticleSystemSortMode.OldestInFront;
            rendererCopia.sortingOrder = rendererCuerpoLocal.sortingOrder + offsetOrden;
            rendererCopia.sortingFudge = rendererCuerpoLocal.sortingFudge;
        }

        if (copia.gameObject.activeInHierarchy)
        {
            copia.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            copia.Play(false);
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
            ? (rendererCuerpo.sharedMaterial == materialCuerpoRuntime
                ? materialFuenteCuerpo : rendererCuerpo.sharedMaterial)
            : null;

        Material fuenteNucleo = rendererNucleo != null
            ? (rendererNucleo.sharedMaterial == materialNucleoRuntime
                ? materialFuenteNucleo : rendererNucleo.sharedMaterial)
            : null;

        ActualizarMaterialCapa(
            sistemaNucleo, rendererNucleo, fuenteNucleo, shaderVolumetrico,
            PerfilCapa.Nucleo, ref materialFuenteNucleo, ref materialNucleoRuntime);
        ActualizarMaterialCapa(
            sistemaCuerpo, rendererCuerpo, fuente, shaderVolumetrico,
            PerfilCapa.Cuerpo, ref materialFuenteCuerpo, ref materialCuerpoRuntime);

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

        materialesVolumen[0] = materialNucleoRuntime;
        materialesVolumen[1] = materialCuerpoRuntime;
        materialesVolumen[2] = materialRetaguardiaRuntime;
        materialesVolumen[3] = materialVolumenCentralRuntime;
        materialesVolumen[4] = materialNucleosDensosRuntime;
        materialesVolumen[5] = materialVeloRuntime;

        // Los materiales originales tienen brillos de emisión muy distintos.
        // Todas las capas toman la paleta del cuerpo para formar una sola masa.
        Material paleta = fuente != null ? fuente : fuenteNucleo;
        if (paleta != null)
        {
            Color baseRegional = paleta.HasProperty("_Color")
                ? paleta.GetColor("_Color") : new Color(0.03f, 0.25f, 0.18f);
            Color emisionRegional = paleta.HasProperty("_EmissionColor")
                ? paleta.GetColor("_EmissionColor") : baseRegional;
            Color tinteRegional = Color.Lerp(baseRegional, emisionRegional, 0.35f);
            tinteRegional *= 0.3f / Mathf.Max(0.3f, tinteRegional.maxColorComponent);
            tinteRegional.b = Mathf.Lerp(tinteRegional.b, tinteRegional.g * 1.08f, 0.4f);
            // Un mínimo cian verdoso mantiene el relieve sobre mapas oscuros;
            // la autosombra sigue dando contraste sobre nieve y terrenos claros.
            tinteRegional = Color.Lerp(new Color(0.065f, 0.48f, 0.40f), tinteRegional, 0.25f);
            tinteRegional.a = 1f;
            foreach (Material material in materialesVolumen)
            {
                if (material != null) material.SetColor("_TintColor", tinteRegional);
            }
        }
        PrepararContactoSuelo();
    }

    void PrepararContactoSuelo()
    {
        AtributosZona zona = GetComponentInParent<AtributosZona>();
        if (zona == null || sistemaCuerpo == null) return;
        if (materialContactoSuelo == null)
        {
            Shader shader = Resources.Load<Shader>("AlientoNegroContactoSuelo");
            if (shader == null) return;
            materialContactoSuelo = new Material(shader)
            {
                name = "Contacto tenue del Aliento (Runtime)",
                hideFlags = HideFlags.DontSave
            };
            materialContactoSuelo.SetTexture("_NoiseTex", texturaRuidoRuntime);
        }
        suelosContacto[0] = zona.TexturaTerreno;
        suelosContacto[1] = zona.TexturaTerrenoExtension;
        for (int i = 0; i < suelosContacto.Length; i++)
        {
            MeshRenderer suelo = suelosContacto[i];
            MeshFilter malla = suelo != null ? suelo.GetComponent<MeshFilter>() : null;
            if (malla == null || malla.sharedMesh == null) continue;
            if (capasContacto[i] == null)
            {
                var capa = new GameObject("Contacto Aliento Negro (Runtime)")
                {
                    layer = suelo.gameObject.layer,
                    hideFlags = HideFlags.DontSave
                };
                capa.transform.SetParent(suelo.transform, false);
                capa.AddComponent<MeshFilter>();
                capasContacto[i] = capa.AddComponent<MeshRenderer>();
                capasContacto[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                capasContacto[i].receiveShadows = false;
                capasContacto[i].sharedMaterial = materialContactoSuelo;
            }
            capasContacto[i].transform.SetParent(suelo.transform, false);
            capasContacto[i].gameObject.layer = suelo.gameObject.layer;
            capasContacto[i].GetComponent<MeshFilter>().sharedMesh = malla.sharedMesh;
        }
        if (materialCuerpoRuntime != null)
            materialContactoSuelo.SetColor("_TintColor", materialCuerpoRuntime.GetColor("_TintColor"));
    }

    void ActualizarContactoSuelo(Matrix4x4 mundoAAliento)
    {
        if (materialContactoSuelo == null || sistemaCuerpo == null) return;
        var shape = sistemaCuerpo.shape;
        Vector3 centro = transform.InverseTransformPoint(sistemaCuerpo.transform.TransformPoint(shape.position));
        materialContactoSuelo.SetMatrix(IdWorldToAliento, mundoAAliento);
        materialContactoSuelo.SetVector(IdFootprint, new Vector4(
            centro.x, centro.z, Mathf.Abs(shape.scale.x) * 0.5f + 1f,
            Mathf.Abs(shape.scale.z) * 0.5f + 1f));
        bool hayNube = sistemaNucleo.particleCount > 0 || sistemaCuerpo.particleCount > 0;
        fuerzaContacto = Mathf.Lerp(fuerzaContacto, hayNube ? 0.16f : 0f,
            1f - Mathf.Exp(-3f * Time.deltaTime));
        materialContactoSuelo.SetFloat(IdStrength, fuerzaContacto);
        for (int i = 0; i < capasContacto.Length; i++)
            if (capasContacto[i] != null)
                capasContacto[i].enabled = isActiveAndEnabled && suelosContacto[i] != null
                    && suelosContacto[i].enabled && suelosContacto[i].gameObject.activeInHierarchy;
    }

    void OnDisable()
    {
        fuerzaContacto = 0f;
        if (materialContactoSuelo != null) materialContactoSuelo.SetFloat("_Strength", 0f);
        foreach (MeshRenderer capa in capasContacto)
            if (capa != null) capa.enabled = false;
    }

    enum PerfilCapa
    {
        Nucleo,
        Cuerpo,
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
            1.15f);

        materialRuntime.SetColor("_TintColor", colorEmision);
        materialRuntime.SetTexture("_NoiseTex", texturaRuidoRuntime);
        materialRuntime.SetMatrix("_WorldToAliento", transform.worldToLocalMatrix);
        float densidad = perfil switch
        {
            PerfilCapa.Nucleo => 0.075f,
            PerfilCapa.Cuerpo => 0.045f,
            PerfilCapa.Retaguardia => 0.035f,
            PerfilCapa.Centro => 0.10f,
            PerfilCapa.NucleosDensos => 0.16f,
            _ => 0.03f
        };
        // Una misma forma, iluminación y velocidad para todas las capas.
        // La densidad determina su función dentro de la masa de nubes.
        ConfigurarMaterial(
            materialRuntime, densidad * escalaAlphaRegional * 1.2f,
            2.3f, 0.58f, 0.43f, 0.22f, 0.028f, 0.06f,
            0.95f, 0.58f, 0.30f, 0.48f,
            perfil == PerfilCapa.NucleosDensos ? 0.32f : 0.12f);

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
        foreach (MeshRenderer capa in capasContacto)
            if (capa != null) Destroy(capa.gameObject);
        if (materialContactoSuelo != null) Destroy(materialContactoSuelo);
        if (rendererNucleo != null && rendererNucleo.sharedMaterial == materialNucleoRuntime)
            rendererNucleo.sharedMaterial = materialFuenteNucleo;
        if (rendererCuerpo != null && rendererCuerpo.sharedMaterial == materialCuerpoRuntime)
            rendererCuerpo.sharedMaterial = materialFuenteCuerpo;
        if (materialNucleoRuntime != null) Destroy(materialNucleoRuntime);
        if (materialCuerpoRuntime != null) Destroy(materialCuerpoRuntime);

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
