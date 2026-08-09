using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class NodoVisualRework : MonoBehaviour
{
  const string NombreMarcador = "MarcadorCampaniaRework";
  const int SegmentosAro = 64;
  const float VelocidadAparicion = 7.5f;
  const float DuracionRevelado = 1.55f;
  const float EscalaGeneralMarcador = 0.85f;
  const float BrilloGeneralMarcador = 0.90f;
  const float OpacidadRevelado = 0.85f;

  static readonly int ShaderColorId = Shader.PropertyToID("_Color");
  static readonly int ShaderBaseColorId = Shader.PropertyToID("_BaseColor");
  static readonly int ShaderEmissionColorId = Shader.PropertyToID("_EmissionColor");
  static readonly int ShaderTintColorId = Shader.PropertyToID("_TintColor");
  static readonly int ShaderMainTexId = Shader.PropertyToID("_MainTex");

  static Mesh mallaAroCompleto;
  static Mesh mallaAroSegmentado;
  static Mesh mallaHaloRevelado;
  static Material materialPedestal;
  static Material materialAro;
  static Material materialHaz;
  static Material materialHaloRevelado;
  static Material materialNiebla;
  static Texture2D texturaVfxSuave;
  static Camera camaraEscalaPantalla;
  static float profundidadReferenciaPantalla;

  Nodo nodo;
  Transform raizMarcador;
  Transform contenidoMarcador;
  Transform aroTipo;
  Transform aroEstado;
  Renderer rendererPedestal;
  Renderer rendererAroTipo;
  Renderer rendererAroEstado;
  LineRenderer hazVertical;
  LineRenderer hazReveladoExterior;
  Transform haloRevelado;
  Renderer rendererHaloRevelado;
  ParticleSystem particulasRevelado;
  ParticleSystem nieblaMisterio;
  readonly List<Transform> iconosOriginales = new List<Transform>();
  readonly List<Vector3> escalasBaseIconosOriginales = new List<Vector3>();
  MaterialPropertyBlock bloquePropiedades;
  bool construido;
  bool seguimientoReveladoInicializado;
  bool reveladoRealAnterior;
  bool misteriosoAnterior;
  int codigoVisualAnterior;
  float aparicion;
  float tiempoReveladoRestante;
  Color colorRevelado;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void RegistrarEscenas()
  {
    camaraEscalaPantalla = null;
    profundidadReferenciaPantalla = 0f;
    SceneManager.sceneLoaded -= AlCargarEscena;
    SceneManager.sceneLoaded += AlCargarEscena;
  }

  static void AlCargarEscena(Scene escena, LoadSceneMode modo)
  {
    if (!escena.IsValid() || !escena.isLoaded)
    {
      return;
    }

    GameObject[] raices = escena.GetRootGameObjects();
    for (int i = 0; i < raices.Length; i++)
    {
      Nodo[] nodos = raices[i].GetComponentsInChildren<Nodo>(true);
      for (int j = 0; j < nodos.Length; j++)
      {
        Asegurar(nodos[j]);
      }
    }
  }

  public static NodoVisualRework Asegurar(Nodo nodoObjetivo)
  {
    if (nodoObjetivo == null)
    {
      return null;
    }

    NodoVisualRework visual = nodoObjetivo.GetComponent<NodoVisualRework>();
    if (visual == null)
    {
      visual = nodoObjetivo.gameObject.AddComponent<NodoVisualRework>();
    }

    return visual;
  }

  void Awake()
  {
    nodo = GetComponent<Nodo>();
    bloquePropiedades = new MaterialPropertyBlock();
    ConstruirSiHaceFalta();
  }

  void OnEnable()
  {
    ConstruirSiHaceFalta();
  }

  void LateUpdate()
  {
    if (!construido)
    {
      ConstruirSiHaceFalta();
    }

    ActualizarVisual();
  }

  void ConstruirSiHaceFalta()
  {
    if (construido || nodo == null)
    {
      return;
    }

    PrepararRecursosCompartidos();
    OcultarBasesOriginalesYAmpliarIconos();

    GameObject raizGo = new GameObject(NombreMarcador);
    raizGo.layer = gameObject.layer;
    raizGo.transform.SetParent(transform, false);
    raizGo.transform.localPosition = new Vector3(0f, -1.35f, 0f);
    raizMarcador = raizGo.transform;

    GameObject contenidoGo = new GameObject("Contenido");
    contenidoGo.layer = gameObject.layer;
    contenidoGo.transform.SetParent(raizMarcador, false);
    contenidoMarcador = contenidoGo.transform;

    CrearPedestal();
    aroTipo = CrearAro("AroTipo", mallaAroCompleto, 0.075f, out rendererAroTipo);
    aroEstado = CrearAro("AroEstado", mallaAroSegmentado, 0.095f, out rendererAroEstado);
    CrearHazVertical();
    InicializarSeguimientoRevelado();
    ActualizarCompensacionVertical();

    construido = true;
  }

  void CrearPedestal()
  {
    GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    pedestal.name = "Pedestal";
    pedestal.layer = gameObject.layer;
    pedestal.transform.SetParent(contenidoMarcador, false);
    pedestal.transform.localPosition = Vector3.zero;
    pedestal.transform.localScale = new Vector3(1.989f, 0.042f, 1.989f);

    rendererPedestal = pedestal.GetComponent<Renderer>();
    if (rendererPedestal != null)
    {
      rendererPedestal.sharedMaterial = materialPedestal;
      ConfigurarRenderer(rendererPedestal);
    }

    Collider collider = pedestal.GetComponent<Collider>();
    if (collider != null)
    {
      collider.isTrigger = true;
    }
  }

  Transform CrearAro(string nombre, Mesh malla, float altura, out Renderer rendererAro)
  {
    GameObject aro = new GameObject(nombre);
    aro.layer = gameObject.layer;
    aro.transform.SetParent(contenidoMarcador, false);
    aro.transform.localPosition = new Vector3(0f, altura, 0f);

    MeshFilter filtro = aro.AddComponent<MeshFilter>();
    filtro.sharedMesh = malla;
    MeshRenderer renderer = aro.AddComponent<MeshRenderer>();
    renderer.sharedMaterial = materialAro;
    ConfigurarRenderer(renderer);
    rendererAro = renderer;
    return aro.transform;
  }

  void CrearHazVertical()
  {
    GameObject haz = new GameObject("HazVertical");
    haz.layer = gameObject.layer;
    haz.transform.SetParent(contenidoMarcador, false);
    haz.transform.localPosition = Vector3.zero;

    hazVertical = haz.AddComponent<LineRenderer>();
    hazVertical.sharedMaterial = materialHaz;
    hazVertical.useWorldSpace = false;
    hazVertical.positionCount = 2;
    hazVertical.SetPosition(0, new Vector3(0f, 0.117f, 0f));
    hazVertical.SetPosition(1, new Vector3(0f, 1.08f, 0f));
    hazVertical.startWidth = 0.117f;
    hazVertical.endWidth = 0.0108f;
    hazVertical.numCapVertices = 4;
    hazVertical.shadowCastingMode = ShadowCastingMode.Off;
    hazVertical.receiveShadows = false;
    hazVertical.lightProbeUsage = LightProbeUsage.Off;
    hazVertical.reflectionProbeUsage = ReflectionProbeUsage.Off;

    GameObject hazExterior = new GameObject("HazReveladoExterior");
    hazExterior.layer = gameObject.layer;
    hazExterior.transform.SetParent(contenidoMarcador, false);
    hazExterior.transform.localPosition = Vector3.zero;

    hazReveladoExterior = hazExterior.AddComponent<LineRenderer>();
    hazReveladoExterior.sharedMaterial = materialHaz;
    hazReveladoExterior.useWorldSpace = false;
    hazReveladoExterior.positionCount = 2;
    hazReveladoExterior.SetPosition(0, new Vector3(0f, 0.10f, 0f));
    hazReveladoExterior.SetPosition(1, new Vector3(0f, 3.75f, 0f));
    hazReveladoExterior.startWidth = 0.68f;
    hazReveladoExterior.endWidth = 0.075f;
    hazReveladoExterior.numCapVertices = 6;
    hazReveladoExterior.shadowCastingMode = ShadowCastingMode.Off;
    hazReveladoExterior.receiveShadows = false;
    hazReveladoExterior.lightProbeUsage = LightProbeUsage.Off;
    hazReveladoExterior.reflectionProbeUsage = ReflectionProbeUsage.Off;
    hazReveladoExterior.enabled = false;
  }

  void CrearHaloReveladoSiHaceFalta()
  {
    if (haloRevelado != null)
    {
      return;
    }

    GameObject halo = new GameObject("HaloRevelado");
    halo.layer = gameObject.layer;
    halo.transform.SetParent(contenidoMarcador, false);
    halo.transform.localPosition = new Vector3(0f, 0.115f, 0f);
    haloRevelado = halo.transform;

    MeshFilter filtro = halo.AddComponent<MeshFilter>();
    filtro.sharedMesh = mallaHaloRevelado;
    MeshRenderer renderer = halo.AddComponent<MeshRenderer>();
    renderer.sharedMaterial = materialHaloRevelado;
    ConfigurarRenderer(renderer);
    renderer.enabled = false;
    rendererHaloRevelado = renderer;
  }

  void CrearParticulasReveladoSiHaceFalta()
  {
    if (particulasRevelado != null)
    {
      return;
    }

    GameObject particulas = new GameObject("ParticulasRevelado");
    particulas.layer = gameObject.layer;
    particulas.transform.SetParent(contenidoMarcador, false);
    particulas.transform.localPosition = new Vector3(0f, 0.22f, 0f);

    particulasRevelado = particulas.AddComponent<ParticleSystem>();
    ParticleSystem.MainModule main = particulasRevelado.main;
    main.loop = false;
    main.playOnAwake = false;
    main.maxParticles = 18;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.90f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.198f);
    main.simulationSpace = ParticleSystemSimulationSpace.Local;
    main.scalingMode = ParticleSystemScalingMode.Hierarchy;

    ParticleSystem.EmissionModule emision = particulasRevelado.emission;
    emision.enabled = false;

    ParticleSystem.ShapeModule forma = particulasRevelado.shape;
    forma.enabled = true;
    forma.shapeType = ParticleSystemShapeType.Circle;
    forma.radius = 0.702f;
    forma.radiusThickness = 1f;
    forma.rotation = new Vector3(90f, 0f, 0f);

    ParticleSystem.VelocityOverLifetimeModule velocidad = particulasRevelado.velocityOverLifetime;
    velocidad.enabled = false;
    velocidad.x = new ParticleSystem.MinMaxCurve(0f, 0f);
    velocidad.y = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
    velocidad.z = new ParticleSystem.MinMaxCurve(0f, 0f);
    velocidad.enabled = true;

    ParticleSystem.ColorOverLifetimeModule colorVida = particulasRevelado.colorOverLifetime;
    colorVida.enabled = true;
    colorVida.color = CrearGradienteAlpha(0f, 0.72f, 0f);

    ParticleSystemRenderer renderer = particulas.GetComponent<ParticleSystemRenderer>();
    renderer.sharedMaterial = materialHaloRevelado;
    renderer.renderMode = ParticleSystemRenderMode.Billboard;
    renderer.sortingFudge = 1f;
    ConfigurarRenderer(renderer);
  }

  void CrearNieblaMisterioSiHaceFalta()
  {
    if (nieblaMisterio != null)
    {
      return;
    }

    GameObject niebla = new GameObject("NieblaMisterio");
    niebla.layer = gameObject.layer;
    niebla.transform.SetParent(contenidoMarcador, false);
    niebla.transform.localPosition = new Vector3(0f, 0.43f, 0f);

    nieblaMisterio = niebla.AddComponent<ParticleSystem>();
    ParticleSystem.MainModule main = nieblaMisterio.main;
    main.loop = true;
    main.playOnAwake = false;
    main.maxParticles = 7;
    main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 3.8f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.015f, 0.05f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.432f, 0.702f);
    main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
    main.startColor = new ParticleSystem.MinMaxGradient(
      new Color(0.30f, 0.34f, 0.40f, 0.027f),
      new Color(0.48f, 0.52f, 0.58f, 0.054f));
    main.simulationSpace = ParticleSystemSimulationSpace.Local;
    main.scalingMode = ParticleSystemScalingMode.Hierarchy;

    ParticleSystem.EmissionModule emision = nieblaMisterio.emission;
    emision.enabled = true;
    emision.rateOverTime = 1.25f;

    ParticleSystem.ShapeModule forma = nieblaMisterio.shape;
    forma.enabled = true;
    forma.shapeType = ParticleSystemShapeType.Circle;
    forma.radius = 0.414f;
    forma.radiusThickness = 1f;
    forma.rotation = new Vector3(90f, 0f, 0f);

    ParticleSystem.VelocityOverLifetimeModule velocidad = nieblaMisterio.velocityOverLifetime;
    velocidad.enabled = false;
    velocidad.x = new ParticleSystem.MinMaxCurve(0f, 0f);
    velocidad.y = new ParticleSystem.MinMaxCurve(0.018f, 0.045f);
    velocidad.z = new ParticleSystem.MinMaxCurve(0f, 0f);
    velocidad.enabled = true;

    ParticleSystem.NoiseModule ruido = nieblaMisterio.noise;
    ruido.enabled = true;
    ruido.quality = ParticleSystemNoiseQuality.Low;
    ruido.strength = 0.075f;
    ruido.frequency = 0.24f;
    ruido.scrollSpeed = 0.045f;
    ruido.damping = true;

    ParticleSystem.ColorOverLifetimeModule colorVida = nieblaMisterio.colorOverLifetime;
    colorVida.enabled = true;
    colorVida.color = CrearGradienteAlpha(0f, 0.82f, 0f);

    ParticleSystemRenderer renderer = niebla.GetComponent<ParticleSystemRenderer>();
    renderer.sharedMaterial = materialNiebla;
    renderer.renderMode = ParticleSystemRenderMode.Billboard;
    renderer.sortingFudge = 1.5f;
    renderer.minParticleSize = 0.002f;
    renderer.maxParticleSize = 0.12f;
    ConfigurarRenderer(renderer);
  }

  void ActualizarVisual()
  {
    if (nodo == null || raizMarcador == null)
    {
      return;
    }

    bool visible = nodo.EstaVisiblePorVision()
      && gameObject.activeInHierarchy
      && HayVisualOriginalActivo();

    if (!visible)
    {
      aparicion = 0f;
      raizMarcador.gameObject.SetActive(false);
      return;
    }

    if (!raizMarcador.gameObject.activeSelf)
    {
      raizMarcador.gameObject.SetActive(true);
      aparicion = 0f;
    }

    aparicion = Mathf.MoveTowards(aparicion, 1f, Time.deltaTime * VelocidadAparicion);
    ActualizarCompensacionVertical();

    MapaManager mapa = nodo.scMapaManager;
    Nodo nodoActual = mapa != null ? mapa.nodoActual : null;
    CaminoConexion conexion = nodoActual != null ? nodoActual.ObtenerConexionHacia(nodo) : null;
    bool disponible = conexion != null && conexion.estadoVisual == EstadoVisualCamino.Disponible;
    bool actual = nodoActual == nodo;
    bool remoto = nodo.EsDescubiertoRemoto();
    bool hover = nodo.EstaCursorSobreNodo() && !remoto;
    bool urgente = nodo.nodoIncendiado || nodo.nodoRitual || nodo.tipoNodo == 16;
    bool inaccesible = nodo.EstaVisualmenteInaccesible();

    int codigoVisual = ObtenerCodigoVisual();
    bool misterioso = nodo.ObtenerEstadoMisterioso() || codigoVisual == 12;
    Color colorTipo = ResolverColorTipo(codigoVisual);
    if (inaccesible)
    {
      float gris = colorTipo.grayscale;
      colorTipo = Color.Lerp(colorTipo, new Color(gris, gris, gris, 1f), 0.90f) * 0.22f;
      colorTipo.a = 1f;
    }

    ActualizarSeguimientoRevelado(codigoVisual, misterioso, inaccesible, colorTipo);
    ActualizarNieblaMisterio((misterioso && !inaccesible) || remoto);
    bool revelandoTipo = ActualizarHaloRevelado(inaccesible);
    ActualizarHazReveladoExterior(revelandoTipo, colorTipo);

    float pulso = 0.5f + 0.5f * Mathf.Sin(
      Time.unscaledTime * (hover ? 6.4f : 3.8f)
      + nodo.posXNodo * 0.43f
      + nodo.posYNodo * 0.21f);

    float escalaAparicion = Mathf.SmoothStep(0.72f, 1f, aparicion);
    float escalaHover = hover ? 1.11f : 1f;
    float escalaMisterio = misterioso ? 0.90f : 1f;
    float compensacionPerspectiva = CalcularCompensacionPerspectiva();
    contenidoMarcador.localScale = Vector3.one
      * escalaAparicion
      * escalaHover
      * escalaMisterio
      * EscalaGeneralMarcador
      * compensacionPerspectiva;
    AplicarCompensacionPerspectivaIconos(compensacionPerspectiva);
    aroTipo.localScale = Vector3.one * (!inaccesible && disponible ? Mathf.Lerp(1f, 1.025f, pulso) : 1f);

    bool mostrarEstado = !inaccesible && (disponible || actual || hover || urgente);
    rendererAroEstado.enabled = mostrarEstado;
    hazVertical.enabled = !inaccesible && (disponible || hover || urgente || revelandoTipo);

    if (mostrarEstado)
    {
      float escalaEstado = hover
        ? 1.12f
        : disponible
          ? Mathf.Lerp(1f, 1.07f, pulso)
          : 1f;
      aroEstado.localScale = Vector3.one * escalaEstado;
      aroEstado.Rotate(0f, Time.unscaledDeltaTime * (hover ? 72f : 28f), 0f, Space.Self);
    }

    Color colorEstado = actual
      ? new Color(1f, 0.76f, 0.28f, 1f)
      : disponible || hover
        ? new Color(1f, 0.92f, 0.72f, 1f)
        : colorTipo;

    float brilloTipo = inaccesible ? 0.0068f : hover ? 0.918f : disponible ? Mathf.Lerp(0.425f, 0.68f, pulso) : 0.221f;
    float brilloEstado = hover ? 1.088f : disponible ? Mathf.Lerp(0.493f, 0.85f, pulso) : 0.425f;

    Color colorPedestal = inaccesible
      ? new Color(0.009f, 0.010f, 0.012f, 1f)
      : new Color(0.028f, 0.030f, 0.036f, 1f);
    AplicarColor(rendererPedestal, colorPedestal, colorTipo * (inaccesible ? 0f : 0.017f * BrilloGeneralMarcador));
    AplicarColor(
      rendererAroTipo,
      colorTipo * (0.73f * BrilloGeneralMarcador),
      colorTipo * (brilloTipo * BrilloGeneralMarcador));
    if (mostrarEstado)
    {
      AplicarColor(
        rendererAroEstado,
        colorEstado * (0.75f * BrilloGeneralMarcador),
        colorEstado * (brilloEstado * BrilloGeneralMarcador));
    }

    if (hazVertical.enabled)
    {
      if (revelandoTipo)
      {
        hazVertical.SetPosition(0, new Vector3(0f, 0.10f, 0f));
        hazVertical.SetPosition(1, new Vector3(0f, 3.75f, 0f));
        hazVertical.startWidth = 0.24f;
        hazVertical.endWidth = 0.022f;
      }
      else
      {
        hazVertical.SetPosition(0, new Vector3(0f, 0.117f, 0f));
        hazVertical.SetPosition(1, new Vector3(0f, 1.08f, 0f));
        hazVertical.startWidth = 0.117f;
        hazVertical.endWidth = 0.0108f;
      }

      float alphaInicio = hover
        ? 0.17f
        : disponible
          ? Mathf.Lerp(0.051f, 0.102f, pulso)
          : urgente
            ? 0.068f
            : 0f;
      if (revelandoTipo)
      {
        alphaInicio = Mathf.Max(
          alphaInicio,
          ObtenerIntensidadRevelado() * 0.48f * OpacidadRevelado);
      }
      Color inicio = revelandoTipo ? Color.Lerp(colorTipo, Color.white, 0.30f) : colorTipo;
      inicio.a = alphaInicio * aparicion * BrilloGeneralMarcador;
      Color fin = inicio;
      fin.a = revelandoTipo
        ? ObtenerIntensidadRevelado()
          * 0.035f
          * aparicion
          * BrilloGeneralMarcador
          * OpacidadRevelado
        : 0f;
      hazVertical.startColor = inicio;
      hazVertical.endColor = fin;
    }
  }

  void InicializarSeguimientoRevelado()
  {
    codigoVisualAnterior = ObtenerCodigoVisual();
    misteriosoAnterior = nodo.ObtenerEstadoMisterioso() || codigoVisualAnterior == 12;
    reveladoRealAnterior = nodo.revelado && !misteriosoAnterior && codigoVisualAnterior > 0;
    seguimientoReveladoInicializado = true;
  }

  void ActualizarSeguimientoRevelado(int codigoVisual, bool misterioso, bool inaccesible, Color colorTipo)
  {
    bool reveladoReal = nodo.revelado && !misterioso && codigoVisual > 0;
    if (!seguimientoReveladoInicializado)
    {
      codigoVisualAnterior = codigoVisual;
      misteriosoAnterior = misterioso;
      reveladoRealAnterior = reveladoReal;
      seguimientoReveladoInicializado = true;
      return;
    }

    bool resolvioMisterio = misteriosoAnterior && !misterioso && reveladoReal;
    bool pasoDeOcultoATipo = (codigoVisualAnterior <= 0 || codigoVisualAnterior == 12)
      && codigoVisual != 12
      && reveladoReal;
    bool acabaDeRevelarTipo = reveladoReal
      && (!reveladoRealAnterior || resolvioMisterio || pasoDeOcultoATipo);
    if (acabaDeRevelarTipo && !inaccesible && !EsNodoInicial())
    {
      IniciarAnimacionRevelado(colorTipo);
    }

    codigoVisualAnterior = codigoVisual;
    misteriosoAnterior = misterioso;
    reveladoRealAnterior = reveladoReal;
  }

  void IniciarAnimacionRevelado(Color colorTipo)
  {
    CrearHaloReveladoSiHaceFalta();
    CrearParticulasReveladoSiHaceFalta();

    colorRevelado = colorTipo;
    colorRevelado.a = 1f;
    tiempoReveladoRestante = DuracionRevelado;
    haloRevelado.localRotation = Quaternion.identity;
    haloRevelado.localScale = Vector3.one * 0.72f;
    rendererHaloRevelado.enabled = true;

    ParticleSystem.MainModule main = particulasRevelado.main;
    Color colorA = colorRevelado;
    colorA.a = 0.092f * OpacidadRevelado;
    Color colorB = Color.Lerp(colorRevelado, Color.white, 0.34f);
    colorB.a = 0.184f * OpacidadRevelado;
    main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);

    ParticleSystem.EmitParams destelloCentral = new ParticleSystem.EmitParams
    {
      position = new Vector3(0f, 0.18f, 0f),
      velocity = Vector3.zero,
      startLifetime = 0.72f,
      startSize = 0.74f,
      startColor = new Color(
        Mathf.Lerp(colorRevelado.r, 1f, 0.24f),
        Mathf.Lerp(colorRevelado.g, 1f, 0.24f),
        Mathf.Lerp(colorRevelado.b, 1f, 0.24f),
        0.288f * OpacidadRevelado),
      applyShapeToPosition = false
    };
    particulasRevelado.Emit(destelloCentral, 1);
    particulasRevelado.Emit(8);
  }

  public void ReproducirAnimacionRevelado()
  {
    ConstruirSiHaceFalta();
    if (!construido
      || nodo == null
      || !nodo.EstaVisiblePorVision()
      || nodo.EstaVisualmenteInaccesible()
      || EsNodoInicial())
    {
      return;
    }

    int codigoVisual = ObtenerCodigoVisual();
    bool misterioso = nodo.ObtenerEstadoMisterioso() || codigoVisual == 12;
    if (!nodo.revelado || misterioso || codigoVisual <= 0)
    {
      return;
    }

    codigoVisualAnterior = codigoVisual;
    misteriosoAnterior = false;
    reveladoRealAnterior = true;
    IniciarAnimacionRevelado(ResolverColorTipo(codigoVisual));
  }

  bool ActualizarHaloRevelado(bool inaccesible)
  {
    if (tiempoReveladoRestante <= 0f || haloRevelado == null || rendererHaloRevelado == null)
    {
      return false;
    }

    if (inaccesible)
    {
      tiempoReveladoRestante = 0f;
      rendererHaloRevelado.enabled = false;
      return false;
    }

    tiempoReveladoRestante = Mathf.Max(0f, tiempoReveladoRestante - Time.unscaledDeltaTime);
    float progreso = 1f - tiempoReveladoRestante / DuracionRevelado;
    float suavizado = Mathf.SmoothStep(0f, 1f, progreso);
    float intensidad = Mathf.Sin(Mathf.Clamp01(progreso) * Mathf.PI);
    haloRevelado.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.42f, suavizado);
    haloRevelado.Rotate(0f, Time.unscaledDeltaTime * 46f, 0f, Space.Self);

    Color halo = colorRevelado;
    halo.a = intensidad * 0.135f * OpacidadRevelado;
    AplicarColorVfx(rendererHaloRevelado, halo);

    if (tiempoReveladoRestante <= 0f)
    {
      rendererHaloRevelado.enabled = false;
      return false;
    }

    return true;
  }

  float ObtenerIntensidadRevelado()
  {
    if (tiempoReveladoRestante <= 0f)
    {
      return 0f;
    }

    float progreso = 1f - tiempoReveladoRestante / DuracionRevelado;
    return Mathf.Sin(Mathf.Clamp01(progreso) * Mathf.PI);
  }

  void ActualizarHazReveladoExterior(bool mostrar, Color colorTipo)
  {
    if (hazReveladoExterior == null)
    {
      return;
    }

    hazReveladoExterior.enabled = mostrar;
    if (!mostrar)
    {
      return;
    }

    float intensidad = ObtenerIntensidadRevelado();
    float apertura = Mathf.Lerp(0.82f, 1f, intensidad);
    hazReveladoExterior.startWidth = 0.68f * apertura;
    hazReveladoExterior.endWidth = 0.075f * apertura;

    Color inicio = Color.Lerp(colorTipo, Color.white, 0.42f);
    inicio.a = intensidad * 0.162f * aparicion * OpacidadRevelado;
    Color fin = inicio;
    fin.a = intensidad * 0.0162f * aparicion * OpacidadRevelado;
    hazReveladoExterior.startColor = inicio;
    hazReveladoExterior.endColor = fin;
  }

  void ActualizarNieblaMisterio(bool mostrar)
  {
    if (mostrar)
    {
      CrearNieblaMisterioSiHaceFalta();
      if (!nieblaMisterio.isPlaying)
      {
        nieblaMisterio.Play(true);
      }
      return;
    }

    if (nieblaMisterio != null && (nieblaMisterio.isPlaying || nieblaMisterio.particleCount > 0))
    {
      nieblaMisterio.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
  }

  bool EsNodoInicial()
  {
    return nodo != null && nodo.posXNodo == 0 && nodo.posYNodo == 0;
  }

  int ObtenerCodigoVisual()
  {
    if (!nodo.revelado)
    {
      return 0;
    }

    int codigo = nodo.ObtenerVisualCodeActual();
    return codigo > 0 ? codigo : nodo.tipoNodo;
  }

  bool HayVisualOriginalActivo()
  {
    int limite = Mathf.Min(transform.childCount, 17);
    for (int i = 0; i < limite; i++)
    {
      if (i == 13 || i == 14)
      {
        continue;
      }

      Transform visual = transform.GetChild(i);
      if (visual != null && visual.gameObject.activeInHierarchy)
      {
        return true;
      }
    }

    return false;
  }

  void ActualizarCompensacionVertical()
  {
    if (raizMarcador == null)
    {
      return;
    }

    Vector3 escalaPadre = transform.lossyScale;
    float x = Mathf.Max(0.0001f, Mathf.Abs(escalaPadre.x));
    float y = Mathf.Max(0.0001f, Mathf.Abs(escalaPadre.y));
    float z = Mathf.Max(0.0001f, Mathf.Abs(escalaPadre.z));
    float escalaHorizontal = (x + z) * 0.5f;
    raizMarcador.localScale = new Vector3(1f, escalaHorizontal / y, 1f);
  }

  float CalcularCompensacionPerspectiva()
  {
    Camera camara = camaraEscalaPantalla;
    if (camara == null || !camara.enabled || !camara.gameObject.activeInHierarchy)
    {
      camara = Camera.main;
    }
    if (camara == null || camara.orthographic)
    {
      return 1f;
    }

    if (camaraEscalaPantalla != camara)
    {
      camaraEscalaPantalla = camara;
      profundidadReferenciaPantalla = 0f;
    }

    if (profundidadReferenciaPantalla <= 0.01f)
    {
      Nodo referencia = nodo != null && nodo.scMapaManager != null
        ? nodo.scMapaManager.nodoActual
        : null;
      if (referencia == null)
      {
        return 1f;
      }

      profundidadReferenciaPantalla = Vector3.Dot(
        camara.transform.forward,
        referencia.transform.position - camara.transform.position);
      if (profundidadReferenciaPantalla <= 0.01f)
      {
        profundidadReferenciaPantalla = 0f;
        return 1f;
      }
    }

    float profundidadNodo = Vector3.Dot(
      camara.transform.forward,
      transform.position - camara.transform.position);
    if (profundidadNodo <= 0.01f)
    {
      return 1f;
    }

    float compensacionCompleta = profundidadNodo / profundidadReferenciaPantalla;
    float compensacionSuave = Mathf.Lerp(1f, compensacionCompleta, 0.25f);
    return Mathf.Clamp(compensacionSuave, 0.92f, 1.20f);
  }

  void AplicarCompensacionPerspectivaIconos(float compensacion)
  {
    int cantidad = Mathf.Min(iconosOriginales.Count, escalasBaseIconosOriginales.Count);
    for (int i = 0; i < cantidad; i++)
    {
      Transform icono = iconosOriginales[i];
      if (icono != null)
      {
        icono.localScale = escalasBaseIconosOriginales[i] * compensacion;
      }
    }
  }

  void OcultarBasesOriginalesYAmpliarIconos()
  {
    int limite = Mathf.Min(transform.childCount, 17);
    for (int i = 0; i < limite; i++)
    {
      if (i == 13 || i == 14)
      {
        continue;
      }

      Transform visual = transform.GetChild(i);
      if (visual == null)
      {
        continue;
      }

      Renderer baseOriginal = visual.GetComponent<Renderer>();
      if (baseOriginal != null && visual.childCount > 0)
      {
        baseOriginal.enabled = false;
      }

      for (int j = 0; j < visual.childCount; j++)
      {
        Transform hijo = visual.GetChild(j);
        if (hijo != null
          && hijo.name.StartsWith("Quad", System.StringComparison.OrdinalIgnoreCase)
          && hijo.GetComponent<Renderer>() != null)
        {
          hijo.localScale *= 0.872f;
          iconosOriginales.Add(hijo);
          escalasBaseIconosOriginales.Add(hijo.localScale);
        }
      }
    }
  }

  void AplicarColor(Renderer rendererObjetivo, Color color, Color emision)
  {
    if (rendererObjetivo == null || bloquePropiedades == null)
    {
      return;
    }

    bloquePropiedades.Clear();
    bloquePropiedades.SetColor(ShaderColorId, color);
    bloquePropiedades.SetColor(ShaderBaseColorId, color);
    bloquePropiedades.SetColor(ShaderEmissionColorId, emision);
    rendererObjetivo.SetPropertyBlock(bloquePropiedades);
  }

  void AplicarColorVfx(Renderer rendererObjetivo, Color color)
  {
    if (rendererObjetivo == null || bloquePropiedades == null)
    {
      return;
    }

    bloquePropiedades.Clear();
    bloquePropiedades.SetColor(ShaderTintColorId, color);
    bloquePropiedades.SetColor(ShaderColorId, color);
    bloquePropiedades.SetColor(ShaderBaseColorId, color);
    rendererObjetivo.SetPropertyBlock(bloquePropiedades);
  }

  static ParticleSystem.MinMaxGradient CrearGradienteAlpha(float inicio, float medio, float fin)
  {
    Gradient gradiente = new Gradient();
    gradiente.SetKeys(
      new[]
      {
        new GradientColorKey(Color.white, 0f),
        new GradientColorKey(Color.white, 1f)
      },
      new[]
      {
        new GradientAlphaKey(inicio, 0f),
        new GradientAlphaKey(medio, 0.42f),
        new GradientAlphaKey(fin, 1f)
      });
    return new ParticleSystem.MinMaxGradient(gradiente);
  }

  static Color ResolverColorTipo(int codigo)
  {
    switch (codigo)
    {
      case 1: return new Color(0.96f, 0.13f, 0.09f, 1f);  // Batalla
      case 2: return new Color(0.20f, 0.55f, 1f, 1f);     // Evento
      case 3: return new Color(0.12f, 0.88f, 0.78f, 1f);  // Claro
      case 4: return new Color(1f, 0.68f, 0.18f, 1f);     // Asentamiento
      case 5: return new Color(0.82f, 0.48f, 0.16f, 1f);  // Recursos
      case 6: return new Color(1f, 0.78f, 0.24f, 1f);     // Comercio
      case 7: return new Color(0.20f, 0.82f, 0.58f, 1f);  // Personajes
      case 8:
      case 10: return new Color(0.78f, 0.24f, 1f, 1f);    // Élite / final
      case 11: return new Color(1f, 0.25f, 0.08f, 1f);    // Emboscada
      case 12: return new Color(1f, 0.62f, 0.10f, 1f);    // Misterioso
      case 13: return new Color(0.10f, 0.78f, 0.42f, 1f); // Atajo
      case 14: return new Color(0.55f, 0.90f, 1f, 1f);    // Santuario
      case 15: return new Color(0.83f, 0.18f, 1f, 1f);    // Ritual
      case 16: return new Color(1f, 0.84f, 0.28f, 1f);    // Salvamento
      default: return new Color(0.92f, 0.58f, 0.12f, 1f); // Oculto
    }
  }

  static void PrepararRecursosCompartidos()
  {
    if (mallaAroCompleto == null)
    {
      mallaAroCompleto = CrearMallaAro(0.916f, 0.981f, false);
      mallaAroCompleto.name = "AroNodoCampaniaCompleto";
      mallaAroCompleto.hideFlags = HideFlags.HideAndDontSave;
    }

    if (mallaAroSegmentado == null)
    {
      mallaAroSegmentado = CrearMallaAro(1.109f, 1.179f, true);
      mallaAroSegmentado.name = "AroNodoCampaniaSegmentado";
      mallaAroSegmentado.hideFlags = HideFlags.HideAndDontSave;
    }

    if (mallaHaloRevelado == null)
    {
      mallaHaloRevelado = CrearMallaAro(0.612f, 0.758f, false);
      mallaHaloRevelado.name = "HaloReveladoNodoCampania";
      mallaHaloRevelado.hideFlags = HideFlags.HideAndDontSave;
    }

    if (materialPedestal == null)
    {
      materialPedestal = CrearMaterialStandard("Pedestal Nodo Campaña", new Color(0.035f, 0.038f, 0.045f, 1f), 0.72f, 0.58f);
    }

    if (materialAro == null)
    {
      materialAro = CrearMaterialStandard("Aros Nodo Campaña", Color.white, 0.58f, 0.72f);
      materialAro.EnableKeyword("_EMISSION");
      if (materialAro.HasProperty(ShaderEmissionColorId))
      {
        materialAro.SetColor(ShaderEmissionColorId, Color.white);
      }
    }

    if (materialHaz == null)
    {
      Shader shaderHaz = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shaderHaz == null)
      {
        shaderHaz = Shader.Find("Sprites/Default");
      }

      materialHaz = new Material(shaderHaz)
      {
        name = "Haz Nodo Campaña (Runtime)",
        hideFlags = HideFlags.HideAndDontSave
      };
    }


    if (texturaVfxSuave == null)
    {
      texturaVfxSuave = CrearTexturaVfxSuave();
    }

    if (materialHaloRevelado == null)
    {
      materialHaloRevelado = CrearMaterialParticulas(
        "Halo Revelado Nodo Campaña",
        "Legacy Shaders/Particles/Additive");
    }

    if (materialNiebla == null)
    {
      materialNiebla = CrearMaterialParticulas(
        "Niebla Misterio Nodo Campaña",
        "Legacy Shaders/Particles/Alpha Blended");
    }
  }

  static Material CrearMaterialParticulas(string nombre, string nombreShader)
  {
    Shader shader = Shader.Find(nombreShader);
    if (shader == null)
    {
      shader = Shader.Find("Sprites/Default");
    }

    Material material = new Material(shader)
    {
      name = nombre + " (Runtime)",
      hideFlags = HideFlags.HideAndDontSave
    };
    if (material.HasProperty(ShaderMainTexId))
    {
      material.SetTexture(ShaderMainTexId, texturaVfxSuave);
    }
    if (material.HasProperty(ShaderTintColorId))
    {
      material.SetColor(ShaderTintColorId, Color.white);
    }
    return material;
  }

  static Texture2D CrearTexturaVfxSuave()
  {
    const int resolucion = 32;
    Texture2D textura = new Texture2D(resolucion, resolucion, TextureFormat.RGBA32, false, true)
    {
      name = "Textura VFX Suave Nodo Campaña (Runtime)",
      filterMode = FilterMode.Bilinear,
      wrapMode = TextureWrapMode.Clamp,
      hideFlags = HideFlags.HideAndDontSave
    };

    Color[] pixeles = new Color[resolucion * resolucion];
    for (int y = 0; y < resolucion; y++)
    {
      for (int x = 0; x < resolucion; x++)
      {
        float nx = (x + 0.5f) / resolucion * 2f - 1f;
        float ny = (y + 0.5f) / resolucion * 2f - 1f;
        float distancia = Mathf.Sqrt(nx * nx + ny * ny);
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 1.65f);
        pixeles[y * resolucion + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    textura.SetPixels(pixeles);
    textura.Apply(false, true);
    return textura;
  }

  static Material CrearMaterialStandard(string nombre, Color color, float metalico, float suavidad)
  {
    Shader shader = Shader.Find("Standard");
    if (shader == null)
    {
      shader = Shader.Find("Unlit/Color");
    }

    Material material = new Material(shader)
    {
      name = nombre + " (Runtime)",
      color = color,
      hideFlags = HideFlags.HideAndDontSave
    };

    if (material.HasProperty("_Metallic"))
    {
      material.SetFloat("_Metallic", metalico);
    }
    if (material.HasProperty("_Glossiness"))
    {
      material.SetFloat("_Glossiness", suavidad);
    }
    return material;
  }

  static Mesh CrearMallaAro(float radioInterior, float radioExterior, bool segmentado)
  {
    Vector3[] vertices = new Vector3[SegmentosAro * 4];
    Vector3[] normales = new Vector3[vertices.Length];
    Vector2[] uv = new Vector2[vertices.Length];
    int[] triangulos = new int[SegmentosAro * 6];

    for (int i = 0; i < SegmentosAro; i++)
    {
      float anguloA = (i / (float)SegmentosAro) * Mathf.PI * 2f;
      float anguloB = ((i + 1) / (float)SegmentosAro) * Mathf.PI * 2f;
      bool mostrarSegmento = !segmentado || i % 8 < 5;
      float interior = mostrarSegmento ? radioInterior : radioExterior;

      int vi = i * 4;
      vertices[vi] = new Vector3(Mathf.Cos(anguloA) * interior, 0f, Mathf.Sin(anguloA) * interior);
      vertices[vi + 1] = new Vector3(Mathf.Cos(anguloA) * radioExterior, 0f, Mathf.Sin(anguloA) * radioExterior);
      vertices[vi + 2] = new Vector3(Mathf.Cos(anguloB) * interior, 0f, Mathf.Sin(anguloB) * interior);
      vertices[vi + 3] = new Vector3(Mathf.Cos(anguloB) * radioExterior, 0f, Mathf.Sin(anguloB) * radioExterior);
      normales[vi] = normales[vi + 1] = normales[vi + 2] = normales[vi + 3] = Vector3.up;
      uv[vi] = new Vector2(0f, 0f);
      uv[vi + 1] = new Vector2(0f, 1f);
      uv[vi + 2] = new Vector2(1f, 0f);
      uv[vi + 3] = new Vector2(1f, 1f);

      int ti = i * 6;
      triangulos[ti] = vi;
      triangulos[ti + 1] = vi + 2;
      triangulos[ti + 2] = vi + 1;
      triangulos[ti + 3] = vi + 1;
      triangulos[ti + 4] = vi + 2;
      triangulos[ti + 5] = vi + 3;
    }

    Mesh mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.normals = normales;
    mesh.uv = uv;
    mesh.triangles = triangulos;
    mesh.RecalculateBounds();
    return mesh;
  }

  static void ConfigurarRenderer(Renderer renderer)
  {
    renderer.shadowCastingMode = ShadowCastingMode.Off;
    renderer.receiveShadows = false;
    renderer.lightProbeUsage = LightProbeUsage.Off;
    renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
  }
}
