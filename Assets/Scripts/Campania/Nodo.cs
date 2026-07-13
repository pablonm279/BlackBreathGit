using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TipoCaminoCampania
{
  Normal,
  Dificil,
  AtajoSubterraneo,
  AtajoSuperficie
}

public enum EstadoVisualCamino
{
  Neutral,
  Disponible,
  Inactivo,
  Hint
}

[System.Serializable]
public class CaminoConexion
{
  [System.NonSerialized] public Nodo origen;
  [System.NonSerialized] public Nodo destino;
  [System.NonSerialized] public Transform linea;

  public TipoCaminoCampania tipo;
  public int costoMovimiento = 1;
  public bool rutaHaciaAldea;
  [System.NonSerialized] public EstadoVisualCamino estadoVisual = EstadoVisualCamino.Neutral;
  [System.NonSerialized] public bool hoverActivo;

  public bool EsAtajoSubterraneo => tipo == TipoCaminoCampania.AtajoSubterraneo;
  public bool EsAtajoSuperficie => tipo == TipoCaminoCampania.AtajoSuperficie;
}

public class Nodo : MonoBehaviour
{
  private ContenedorDeNodos scContenedorNodos2;

  // --- Datos del nodo ---
  public int tipoNodo;     //1-Batalla  2-Elie  3-Evento  4-Claro
  public int posXNodo;     // 1..11
  public int posYNodo;     // 1..5 (A..E)
  public bool nodoDespejado;
  public int cantidadConexiones = 0;
  [System.NonSerialized] public readonly List<Nodo> DestinosPosibles = new List<Nodo>();
  public bool revelado = false;
  public List<int> ObligatorioEnZona = new List<int>();
  public List<int> ProhibidoEnZona = new List<int>();

  public MapaManager scMapaManager;

  // --- Visual caminos ---
  [Header("Caminos")]
  public GameObject linePrefab;          // Debe traer LineRenderer
  public float lineWidth = 0.6f;         // Ancho de la cinta (CaminoMesh)
  public float lineHeightOffset = 0.02f; // Evitar z-fighting
  const float CaminoAnchoBaseMultiplicador = 0.8f;
  const float CaminoAnchoDificilMultiplicador = 0.62f;
  const float CaminoAAldeaAnchoMultiplicador = 1.15f;
  const float CaminoAtajoSuperficieAnchoMultiplicador = 0.85f;
  const float CaminoSubterraneoAnchoMultiplicador = 0.8f;
  const float CaminoAlturaMinimaSobreRelieve = 0.055f;
  const float CaminoYOffsetMallaMinimo = 0.025f;
  const float CaminoAnchoGlobalMultiplicador = 0.75f;
  const float ToleranciaCoincidenciaCaminoXZ = 0.18f;
  const float TramoContinuacionVisionVisible = 0.28f;
  const float MultiplicadorAnchoContinuacionVision = 0.58f;
  const string NombreLineaContinuacionVision = "LineaVisionCorta";
  const float PulsoNodoMovibleVelocidad = 3.7f;
  const float PulsoNodoMovibleEscalaMax = 1.26f;
  const float PulsoNodoMovibleEscalaMaxHover = 1.31f;
  const float PulsoNodoMovibleGlowColorBlend = 0.16f;
  const float PulsoNodoMovibleGlowEmission = 0.55f;
  const float DuracionFadeVisionNodo = 0.18f;
  const float DuracionFadeVisionCamino = 0.14f;
  const float RetrasoPasoXFadeVision = 0.028f;
  const float RetrasoPasoYFadeVision = 0.006f;
  const float RetrasoMaximoFadeVision = 0.11f;
  const string NombreLineaIntroCampania = "LineaIntroCampania";
  const string NombreLineaOutroCampania = "LineaOutroCampania";
  const int SegmentosLineaIntroCampania = 5;
  const int SegmentosLineaOutroCampania = 6;
  const int ResolucionLineaIntroCampania = 12;
  const int ResolucionLineaOutroCampania = 16;
  const float AlphaIntroCampaniaMinimo = 0.16f;
  const float AlphaIntroCampaniaMaximo = 0.82f;
  const float MultiplicadorLongitudOutroCampania = 1.18f;
  const float LongitudMinimaOutroCampania = 5.4f;
  const float LongitudMaximaOutroCampania = 8.35f;
  const float CurvaturaLateralOutroCampania = 0.34f;
  const float SinuosidadOutroCampania = 0.12f;
  const float VelocidadIntroCampania = 1.0f;
  const float SeparacionIntroCampania = 0.62f;
  const float LateralMaximoIntroCampania = 0.38f;
  const float RotacionIntroCampania = 5.5f;
  const float LookAheadIntroCampania = 0.45f;
  const float DeltaMaximoIntroCampania = 1f / 30f;
  const float OffsetConvoyIntroSobreRelieve = 0.03f;
  const string TooltipZonaExpuestaId = "campania_personaje_zonaExpuesta";
  const string TooltipAsentamientoId = "campania_asentamiento";
  const string TooltipAtajoSubterraneoId = "campania_atajo_sup";
  static readonly int ShaderColorId = Shader.PropertyToID("_Color");
  static readonly int ShaderBaseColorId = Shader.PropertyToID("_BaseColor");
  static readonly int ShaderEmissionColorId = Shader.PropertyToID("_EmissionColor");

  // Materiales
  public Material MaterialCaminoOriginal;
  public Material MaterialCaminoMarcado;
  public Material MaterialCaminoUsado;
  public Material MaterialAtajo;
  public Material MaterialCaminoHint;
  public Material caminoLento;
  public Material caminoAAldea;
  private Material materialAtajoSubterraneoVisual;
  private Material materialCaminoHintVisual;
  private Material materialCaminoHintPasoVientoHeladoVisual;
  public Material materialAtajoSuperficieVisual;

  // Lógica movimiento
  public float velocidadMovimiento = 4f;

  // Internos
  public bool yatiroConexiones = false;
  Nodo vieneDeNodo;
  bool esMisterioso = false; // Nodo no revelado visualmente
  bool misterioForzadoTutorial = false;
  bool revelandoPorExpedicionTutorial = false;
  bool reveladoPorExpedicionTutorial = false;
  public bool nodoIncendiado = false;
  public bool nodoRitual = false;
  public int tipoNodoOriginalRitual = 0;
  int numVisualActual = -1;
  const int CodigoSettlement = 4;
  const int IndiceVisualSettlement = 4;
  const float MultiplicadorEscalaSettlement = 1.18f;
  bool escalaSettlementInicializada = false;
  Vector3 escalaSettlementOriginal = Vector3.one;
  bool atajoSubterraneoPendiente = false;
  private static GameObject undergroundTravelMarker;
  bool pulsoMovimientoActivo;
  Transform visualPulsoMovimientoActual;
  Vector3 escalaBaseVisualPulsoMovimiento = Vector3.one;
  readonly List<Renderer> renderersPulsoMovimientoActuales = new List<Renderer>();
  static readonly HashSet<Nodo> nodosConPulsoMovimientoActivos = new HashSet<Nodo>();
  readonly List<CaminoConexion> conexionesSalientes = new List<CaminoConexion>();
  readonly Dictionary<string, Material> variantesMaterialCamino = new Dictionary<string, Material>();
  CaminoConexion conexionHoverActiva;
  Transform lineaHoverTemporalActiva;
  bool lineaHoverTemporalEstabaActiva;
  bool lineaHoverTemporalMeshEstabaVisible;
  CaminoConexion conexionLlegada;
  readonly Dictionary<Nodo, Transform> lineasContinuacionVisionPorDestino = new Dictionary<Nodo, Transform>();
  readonly HashSet<Transform> lineasReveladas = new HashSet<Transform>();
  readonly HashSet<Transform> lineasPendientesVision = new HashSet<Transform>();
  readonly HashSet<Transform> lineasConFadeVisionAplicado = new HashSet<Transform>();
  readonly HashSet<Nodo> continuacionesVisionConfiguradas = new HashSet<Nodo>();
  readonly Dictionary<Transform, Coroutine> rutinasFadeVisionLineas = new Dictionary<Transform, Coroutine>();
  readonly Dictionary<Transform, List<FadeRendererState>> estadosFadeVisionLineas = new Dictionary<Transform, List<FadeRendererState>>();
  readonly Dictionary<Transform, List<FadeCaminoMeshState>> estadosFadeVisionMeshesLineas = new Dictionary<Transform, List<FadeCaminoMeshState>>();
  List<FadeRendererState> estadosFadeVisionNodo;
  List<FadeTransformState> estadosFadeVisionTransformNodo;
  MaterialPropertyBlock bloqueFadeVision;
  MaterialPropertyBlock bloquePulsoMovimiento;
  bool cursorSobreNodo;
  bool visibleForzadaPorReveladoEspecial;
  bool visiblePorVision = true;
  string faccionScoutReveladaId = "";
  string faccionScoutReveladaNombre = "";
  Coroutine rutinaFadeVisionNodo;
  bool nodoConFadeVisionAplicado;

  class FadeRendererState
  {
    public Renderer renderer;
    public bool usaColor;
    public Color colorOriginal;
    public bool usaBaseColor;
    public Color baseColorOriginal;
  }

  class FadeTransformState
  {
    public Transform transform;
    public Vector3 escalaOriginal;
  }

  class FadeCaminoMeshState
  {
    public CaminoMesh caminoMesh;
    public float anchoOriginal;
  }

  bool EsSettlement()
  {
    return tipoNodo == CodigoSettlement;
  }

  bool PuedeTenerIncendioPersistente()
  {
    return !EsSettlement();
  }

  bool PuedeTenerRitualPersistente()
  {
    return !EsSettlement();
  }

  bool UsaConfiguracionTutorial()
  {
    return CampaignManager.Instance != null && CampaignManager.Instance.DebeUsarConfiguracionTutorial();
  }

  void LimpiarEstadosPersistentesNoValidos()
  {
    bool tutorialActivo = UsaConfiguracionTutorial();

    if (tutorialActivo || !PuedeTenerIncendioPersistente())
    {
      nodoIncendiado = false;
    }

    if (tutorialActivo || !PuedeTenerRitualPersistente())
    {
      nodoRitual = false;
    }
  }

  public void LimpiarEstadosEspecialesTutorial()
  {
    esMisterioso = false;
    misterioForzadoTutorial = false;
    revelandoPorExpedicionTutorial = false;
    reveladoPorExpedicionTutorial = false;
    atajoSubterraneoPendiente = false;
    nodoIncendiado = false;
    if (numVisualActual == 12 || numVisualActual == 13)
    {
      numVisualActual = tipoNodo;
    }

    SincronizarVFXPersistentes();
  }

  private class UndergroundAudioFxState
  {
    public AudioReverbFilter reverb;
    public AudioLowPassFilter lowPass;
    public AudioEchoFilter echo;

    public bool createdReverb;
    public bool createdLowPass;
    public bool createdEcho;

    public bool reverbWasEnabled;
    public bool lowPassWasEnabled;
    public bool echoWasEnabled;

    public AudioReverbPreset reverbPresetBefore;
    public float lowPassCutoffBefore;
    public float lowPassResonanceBefore;
    public float echoWetBefore;
    public float echoDryBefore;
    public float echoDelayBefore;
    public float echoDecayBefore;
  }

  void Awake()
  {
    bloqueFadeVision = new MaterialPropertyBlock();
    bloquePulsoMovimiento = new MaterialPropertyBlock();
  }

  void Start()
  {
    scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;
    PrepararMaterialAtajoSubterraneo();

    EsconderSiNedukazal();
  }

  void Update()
  {
    ActualizarPulsoMovimientoNodo();
  }

  public void LlegoCaravana()
  {
    CampaignManager.Instance.MoviendoCaravana = false;
    scMapaManager.nodoActual = this;
    if (scMapaManager != null)
    {
      scMapaManager.NotificarFinViajeCaravana();
    }
    DeterminarConexiones();

    // Apagar animaciones con un retraso aleatorio hasta 0.25s por cada follower
    if (scMapaManager != null)
    {
      IEnumerator SetWalkingFalseAfterRandomDelay(GameObject follower)
      {
      if (follower == null) yield break;
      float delay = UnityEngine.Random.Range(0f, 0.25f);
      yield return new WaitForSeconds(delay);
      if (follower.transform.childCount > 0)
      {
        var animator = follower.transform.GetChild(0).GetComponent<Animator>();
        if (animator != null) animator.SetBool("IsWalking", false);
      }
      }

      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower1));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower2));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower3));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower4));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower5));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower6));
    }

    string hayExploracionExplorador = "";
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers.PuedeRealizarActividades() && pers.ActividadSeleccionada == 9) hayExploracionExplorador = pers.sNombre;
      if (pers.Camp_Enfermo > 0) pers.Camp_Enfermo -= 1;
      pers.ReducirCampBendecido();
      if (pers.Camp_Moral > 0) pers.Camp_Moral -= 1;
      if (pers.Camp_Moral < 0) pers.Camp_Moral += 1;
    }

    int chanceExploracion = CampaignManager.Instance.ObtenerChanceExploracionViaje();
    int alcanceExploracion = Mathf.Max(1, CampaignManager.Instance.ObtenerDistanciaVisionEfectiva());
    TiradaExploracion(chanceExploracion, true, hayExploracionExplorador, string.IsNullOrEmpty(hayExploracionExplorador), alcanceExploracion);

    int fatigaSuma = 1;
    int esperanzaSuma = 0;

    if (CampaignManager.Instance.SeLlevaDemasiadaCarga())
    {
      fatigaSuma += 1;
      esperanzaSuma -= 10;
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La Caravana ha viajado con exceso de Carga. -10 Esperanza +1 Fatiga"));
    }

    int chancesAtajo = 15;
    chancesAtajo += 5 * CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9);
    if (CampaignManager.Instance.scAtributosZona.ID == 3) { chancesAtajo = 0; } // En Nedukazal no hay atajos
    if (CampaignManager.Instance.DebeUsarConfiguracionTutorial()) { chancesAtajo = 0; } // En Tutorial no hay atajos
    
    Nodo nodoAtajoSubterraneoEncontrado = null;
    if (UnityEngine.Random.Range(0, 100) < chancesAtajo && posXNodo < 9)
    {
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Se ha encontrado un atajo subterráneo."));
      nodoAtajoSubterraneoEncontrado = EncontrarAtajo(2, 0);
    }

    CampaignManager.Instance.CambiarFatigaActual(fatigaSuma);
    CampaignManager.Instance.CambiarEsperanzaActual(esperanzaSuma);
    CampaignManager.Instance.LlegarANodo(ObtenerTipoNodoAlLlegar(), posXNodo, this);
    TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignNodeArrived, gameObject)
      .Add("nodeId", ObtenerTutorialTargetId())
      .Add("type", tipoNodo)
      .Add("x", posXNodo)
      .Add("y", posYNodo));

    CampaignManager.Instance?.scTutorialManager?.RevelarEmboscadaMov3AlLlegar(this);
    scMapaManager.RefrescarVisibilidadExploracion();
    CampaignManager.Instance?.MarcarNodoCampaniaTemporal(nodoAtajoSubterraneoEncontrado, TipoHighlightNodoCampania.AtajoSubterraneo);
  }

  public Nodo EncontrarAtajo(int X, int Y)
  {
    if (UsaConfiguracionTutorial())
    {
      return null;
    }

    if (scContenedorNodos2 == null)
      scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;
    int zonaId = -1;
    if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
      zonaId = CampaignManager.Instance.scAtributosZona.ID;

    int nextX = posXNodo + X;
    List<Nodo> posiblesAtajos = new List<Nodo>();

    for (int dy = -Y; dy <= Y; dy++)
    {
      int y = posYNodo + dy;
      if (y < 1 || y > 5) continue;

      Nodo c = scContenedorNodos2.ObtenerNodoSegunXY(nextX, y);
      if (c == null) continue;
      if (!EstaPermitidoEnZona(c, zonaId)) continue;
      if (DestinosPosibles.Contains(c)) continue;

      bool hayRutaIntermedia = false;
      foreach (var b in DestinosPosibles)
      {
        if (b == null) continue;
        if (b.posXNodo != posXNodo + 1) continue;
        if (b.DestinosPosibles != null && b.DestinosPosibles.Contains(c))
        {
          hayRutaIntermedia = true;
          break;
        }
      }

      if (!hayRutaIntermedia) posiblesAtajos.Add(c);
    }

    if (posiblesAtajos.Count == 0)
    {
      if (Y < 2)
      {
        return EncontrarAtajo(X, Y + 1);
      }

      for (int dy = -2; dy <= 2; dy++)
      {
        int y = posYNodo + dy;
        if (y < 1 || y > 5) continue;

        Nodo c = scContenedorNodos2.ObtenerNodoSegunXY(nextX, y);
        if (c != null && !DestinosPosibles.Contains(c) && EstaPermitidoEnZona(c, zonaId))
          posiblesAtajos.Add(c);
      }
    }

    if (posiblesAtajos.Count > 0)
    {
      Nodo elegido = posiblesAtajos[UnityEngine.Random.Range(0, posiblesAtajos.Count)];
      ConectarConNodo(elegido, true, false);
      elegido.Revelar(true);
      if (scMapaManager != null)
      {
        scMapaManager.RefrescarVisibilidadExploracion();
      }
      TutorialTooltipManager.TryShow(TooltipAtajoSubterraneoId);
      return elegido;
    }

    return null;
  }

  public bool IntentarEncontrarAtajoSuperficie()
  {
    if (CampaignManager.Instance == null ||
        CampaignManager.Instance.scMapaManager == null ||
        CampaignManager.Instance.DebeUsarConfiguracionTutorial())
    {
      return false;
    }

    if (CampaignManager.Instance.scAtributosZona != null && CampaignManager.Instance.scAtributosZona.ID == 3)
    {
      return false;
    }

    if (posXNodo >= 10)
    {
      return false;
    }

    if (scContenedorNodos2 == null)
      scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;

    if (scContenedorNodos2 == null)
    {
      return false;
    }

    int zonaId = CampaignManager.Instance.scAtributosZona != null
      ? CampaignManager.Instance.scAtributosZona.ID
      : -1;

    int nextX = posXNodo + 1;
    List<Nodo> posiblesAtajos = new List<Nodo>();
    int primero = UnityEngine.Random.value < 0.5f ? -1 : 1;

    for (int i = 0; i < 2; i++)
    {
      int dy = i == 0 ? primero : -primero;
      int y = posYNodo + dy;
      if (y < 1 || y > 5) continue;

      Nodo destino = scContenedorNodos2.ObtenerNodoSegunXY(nextX, y);
      if (destino == null || !destino.gameObject.activeSelf) continue;
      if (!EstaPermitidoEnZona(destino, zonaId)) continue;
      if (DestinosPosibles.Contains(destino)) continue;

      posiblesAtajos.Add(destino);
    }

    if (posiblesAtajos.Count == 0)
    {
      return false;
    }

    Nodo elegido = posiblesAtajos[UnityEngine.Random.Range(0, posiblesAtajos.Count)];
    ConectarConNodo(elegido, false, true, false, true);
    if (!DestinosPosibles.Contains(elegido))
    {
      return false;
    }

    elegido.RevelarComoMisterioso();

    if (scMapaManager != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }

    elegido.ActivarVfxDescubrimiento();
    CampaignManager.Instance?.MarcarNodoCampaniaTemporal(elegido, TipoHighlightNodoCampania.AtajoSuperficie);
    return true;
  }

  #region LOGICA CAMINOS
  public IReadOnlyList<CaminoConexion> ConexionesSalientes => conexionesSalientes;

  public CaminoConexion ObtenerConexionHacia(Nodo destino)
  {
    return destino == null ? null : conexionesSalientes.Find(conexion => conexion != null && conexion.destino == destino);
  }

  public bool TieneConexionHacia(Nodo destino)
  {
    return ObtenerConexionHacia(destino) != null;
  }

  public void LimpiarConexiones()
  {
    conexionHoverActiva = null;
    conexionLlegada = null;
    conexionesSalientes.Clear();
    DestinosPosibles.Clear();
  }

  public void DeterminarConexiones()
  {
    int xadelante = posXNodo + 1;
    scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;
    int zonaId = -1;
    if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
      zonaId = CampaignManager.Instance.scAtributosZona.ID;

    if (yatiroConexiones) return;
    yatiroConexiones = true;

    if ((posXNodo == 0) && (posYNodo == 0)) // Nodo origen
    {
      IntentarConectar(1, 1, zonaId);
      IntentarConectar(1, 3, zonaId);
      IntentarConectar(1, 5, zonaId);

      RevelarNodosFuturosIniciales(3);

      if (CampaignManager.Instance != null)
      {
        int chanceExploracionInicial = CampaignManager.Instance.ObtenerChanceExploracionViaje();
        TiradaExploracion(chanceExploracionInicial, true, "", true, 1);
      }
    }
    else if (posXNodo == 1)
    {
      if (posYNodo == 1)
        IntentarConectar(xadelante, 2, zonaId);
      else
        IntentarConectar(xadelante, posYNodo - 1, zonaId);

      IntentarConectar(xadelante, posYNodo, zonaId);
    }
    else if (posYNodo == 1 && posXNodo < 10)
    {
      int random1 = UnityEngine.Random.Range(1, 11);
      if (random1 <= 2) IntentarConectar(xadelante, 1, zonaId);
      else if (random1 <= 4) IntentarConectar(xadelante, 2, zonaId);
      else { IntentarConectar(xadelante, 1, zonaId); IntentarConectar(xadelante, 2, zonaId); }
    }
    else if (posYNodo == 2 && posXNodo < 10)
    {
      int random2 = UnityEngine.Random.Range(1, 6);
      if (random2 == 1) IntentarConectar(xadelante, 1, zonaId);
      else if (random2 == 2) { IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 3, zonaId); }
      else if (random2 == 3) IntentarConectar(xadelante, 2, zonaId);
      else if (random2 == 4) { IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 3, zonaId); }
      else if (random2 == 5) IntentarConectar(xadelante, 3, zonaId);
    }
    else if (posYNodo == 3 && posXNodo < 10)
    {
      int random3 = UnityEngine.Random.Range(1, 6);
      if (random3 == 1) IntentarConectar(xadelante, 2, zonaId);
      else if (random3 == 2) IntentarConectar(xadelante, 3, zonaId);
      else if (random3 == 3) { IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 4, zonaId); }
      else if (random3 == 4) { IntentarConectar(xadelante, 3, zonaId); IntentarConectar(xadelante, 4, zonaId); }
      else if (random3 == 5) { IntentarConectar(xadelante, 3, zonaId); IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 4, zonaId); }
    }
    else if (posYNodo == 4 && posXNodo < 10)
    {
      int random4 = UnityEngine.Random.Range(1, 6);
      if (random4 == 1) IntentarConectar(xadelante, 4, zonaId);
      else if (random4 == 2) { IntentarConectar(xadelante, 4, zonaId); IntentarConectar(xadelante, 5, zonaId); }
      else if (random4 == 3) IntentarConectar(xadelante, 3, zonaId);
      else if (random4 == 4) IntentarConectar(xadelante, 4, zonaId);
      else if (random4 == 5) { IntentarConectar(xadelante, 4, zonaId); IntentarConectar(xadelante, 3, zonaId); }
    }
    else if (posYNodo == 5 && posXNodo < 10)
    {
      int random5 = UnityEngine.Random.Range(1, 5);
      if (random5 == 1) IntentarConectar(xadelante, 5, zonaId);
      else if (random5 == 2) { IntentarConectar(xadelante, 5, zonaId); IntentarConectar(xadelante, 4, zonaId); }
      else if (random5 == 3) IntentarConectar(xadelante, 4, zonaId);
      else if (random5 == 4) { IntentarConectar(xadelante, 4, zonaId); IntentarConectar(xadelante, 5, zonaId); }
    }
    else if (posXNodo == 10)
    {
      IntentarConectar(11, 10, zonaId);
    }

    if (DestinosPosibles.Count == 0 && posXNodo < 10)
      ConectarFallbackSiguienteColumna(xadelante, zonaId);
  }

  public void ConectarConNodo(
    Nodo nodoB,
    bool esPorAbajo = false,
    bool propagar = true,
    bool ignorarRestricciones = false,
    bool esAtajoSuperficie = false,
    TipoCaminoCampania? tipoForzado = null,
    int costoMovimientoForzado = 0,
    bool rutaHaciaAldea = false)
  {
    if (nodoB == null) return;
    if (UsaConfiguracionTutorial() && (esPorAbajo || esAtajoSuperficie))
    {
      return;
    }

    int zonaId = -1;
    if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
      zonaId = CampaignManager.Instance.scAtributosZona.ID;
    if (!ignorarRestricciones && !EstaPermitidoEnZona(nodoB, zonaId)) return;
    if (DestinosPosibles.Contains(nodoB)) return;

    Nodo nodoA = this;
    TipoCaminoCampania tipoCamino = tipoForzado ?? (
      esPorAbajo
        ? TipoCaminoCampania.AtajoSubterraneo
        : esAtajoSuperficie
          ? TipoCaminoCampania.AtajoSuperficie
          : nodoB.posXNodo > 1 && UnityEngine.Random.Range(0, 100) < 20
            ? TipoCaminoCampania.Dificil
            : TipoCaminoCampania.Normal);
    int costoMovimientoCamino = costoMovimientoForzado > 0
      ? costoMovimientoForzado
      : tipoCamino == TipoCaminoCampania.Normal || tipoCamino == TipoCaminoCampania.AtajoSuperficie ? 1 : 2;
    bool esCaminoDificil = tipoCamino == TipoCaminoCampania.Dificil;
    esPorAbajo = tipoCamino == TipoCaminoCampania.AtajoSubterraneo;
    esAtajoSuperficie = tipoCamino == TipoCaminoCampania.AtajoSuperficie;
    CaminoConexion conexion = new CaminoConexion
    {
      origen = nodoA,
      destino = nodoB,
      tipo = tipoCamino,
      costoMovimiento = costoMovimientoCamino,
      rutaHaciaAldea = rutaHaciaAldea
    };
    conexionesSalientes.Add(conexion);
    nodoA.DestinosPosibles.Add(nodoB);
    cantidadConexiones++;

    // Crear línea
    GameObject lineObject = Instantiate(linePrefab, this.transform);
    lineObject.name = esPorAbajo ? "LineaCaminosSubterraneo" : esAtajoSuperficie ? "LineaCaminosAtajoSuperficie" : "LineaCaminos";
    conexion.linea = lineObject.transform;

    LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
    if (lineRenderer == null)
    {
      Debug.LogError("El prefab de línea no tiene LineRenderer.");
      return;
    }

    // Aseguramos world space para que CaminoMesh convierta bien a local
    lineRenderer.useWorldSpace = true;

    Vector3 p0 = nodoA.transform.position;
    Vector3 p3 = nodoB.transform.position;
    MapDecorator mapDecorator = ObtenerDecoradorMapa();

    // Dirección y perpendicular para "empujar" la curva
    Vector3 dir = (p3 - p0);
    dir.y = 0f;
    float dist = dir.magnitude;
    if (dist < 0.001f) dist = 0.001f;
    dir /= dist;

    Vector3 perp = Vector3.Cross(Vector3.up, dir);
    if (perp.sqrMagnitude < 0.0001f) perp = Vector3.Cross(Vector3.forward, dir);
    perp.Normalize();

    // Curvatura: más marcada si es atajo, pero SIN tocar Y
    float outward;
    if (esPorAbajo)
    {
      // atajo subterraneo: curva amplia y mas abierta
      outward = UnityEngine.Random.Range(3.1f, 4.35f);
    }
    else if (esCaminoDificil)
    {
      outward = UnityEngine.Random.Range(0.36f, 0.67f);
    }
    else
    {
      // 30% de probabilidad de una curvatura más pronunciada también para no-atajo
      if (UnityEngine.Random.value < 0.24f && cantidadConexiones < 2 && dist > 7.5f)
        outward = UnityEngine.Random.Range(0.9f, 1.35f); // curva visible pero controlada
      else
        outward = UnityEngine.Random.Range(0.19f, 0.52f); // normal: leve
    }

    // Evitar que los primeros 2 ramos salgan muy curvos
    if (!esPorAbajo && cantidadConexiones < 2)
      outward *= esCaminoDificil ? 0.88f : 0.72f;

    float outwardMaximo = esPorAbajo
      ? Mathf.Max(3f, dist * 0.3f)
      : esCaminoDificil
        ? Mathf.Clamp(dist * 0.125f, 0.3f, 0.67f)
        : Mathf.Clamp(dist * 0.085f, 0.2f, 0.7f);
    outward = Mathf.Min(outward, outwardMaximo);

    float sideSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;

    // Dónde colocar puntos de control
    float t1 = esPorAbajo ? UnityEngine.Random.Range(0.1f, 0.17f) : UnityEngine.Random.Range(0.16f, 0.24f);
    float t2 = esPorAbajo ? UnityEngine.Random.Range(0.79f, 0.9f) : UnityEngine.Random.Range(0.60f, 0.76f);

    // Pequeña variación lateral
    float jitter1 = UnityEngine.Random.Range(-0.5f, 0.5f);
    float jitter2 = UnityEngine.Random.Range(-0.5f, 0.5f);

    float intensidadCurva1 = esPorAbajo ? (0.78f + 0.4f * Mathf.Abs(jitter1)) : (0.35f + 0.65f * Mathf.Abs(jitter1));
    float intensidadCurva2 = esPorAbajo ? (0.78f + 0.4f * Mathf.Abs(jitter2)) : (0.35f + 0.65f * Mathf.Abs(jitter2));
    Vector3 p1 = p0 + dir * (dist * t1) + perp * (sideSign * outward * intensidadCurva1);
    Vector3 p2 = p3 - dir * (dist * (1f - t2)) + perp * (sideSign * outward * intensidadCurva2);

    // Curva Bézier: SIEMPRE PLANA en Y (evita hundirse bajo el suelo)
    int resolutionMinima = esCaminoDificil ? 30 : 22;
    int resolutionMaxima = esCaminoDificil ? 54 : 42;
    float densidadMuestreo = esCaminoDificil ? 10.5f : 8.5f;
    int resolution = Mathf.Clamp(Mathf.RoundToInt(dist * densidadMuestreo), resolutionMinima, resolutionMaxima);
    float frecuenciaCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(1.14f, 1.34f) : 0f;
    float amplitudCaminoSinuoso = esCaminoDificil ? Mathf.Min(dist * 0.045f, UnityEngine.Random.Range(0.125f, 0.21f)) : 0f;
    float caosCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(0.011f, 0.025f) : 0f;
    float faseCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(0f, Mathf.PI * 2f) : 0f;
    float semillaCaosCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(0f, 100f) : 0f;
    float offsetCaminoSobreRelieve = Mathf.Max(CaminoAlturaMinimaSobreRelieve, lineHeightOffset * 3f);
    lineRenderer.positionCount = resolution;
    for (int i = 0; i < resolution; i++)
    {
      float t = i / (float)(resolution - 1);
      Vector3 point = BezierCurve.GetPoint(p0, p1, p2, p3, t);

      // Forzamos Y a la interpolación del tramo (plano) + leve offset si querés
      if (esCaminoDificil && i > 0 && i < resolution - 1)
      {
        point += perp * CalcularDesvioCaminoSinuoso(t, frecuenciaCaminoSinuoso, amplitudCaminoSinuoso, caosCaminoSinuoso, faseCaminoSinuoso, semillaCaosCaminoSinuoso);
      }

      if (mapDecorator != null && mapDecorator.TrySampleSurface(point, out var surfacePoint, out _, offsetCaminoSobreRelieve))
      {
        point.y = surfacePoint.y;
      }
      else
      {
        point.y = Mathf.Lerp(p0.y, p3.y, t);
      }

      lineRenderer.SetPosition(i, point);
    }

    // Construir malla plana del camino
    var caminoMesh = lineObject.GetComponent<CaminoMesh>();
    if (caminoMesh == null) caminoMesh = lineObject.AddComponent<CaminoMesh>();
    float anchoCamino = ObtenerAnchoVisualCamino(esCaminoDificil);
    if (esAtajoSuperficie)
    {
      anchoCamino *= CaminoAtajoSuperficieAnchoMultiplicador;
    }
    if (esPorAbajo)
    {
      anchoCamino *= CaminoSubterraneoAnchoMultiplicador;
    }
    caminoMesh.SetWidth(anchoCamino);
    caminoMesh.SetYOffset(Mathf.Max(CaminoYOffsetMallaMinimo, lineHeightOffset));
    caminoMesh.RebuildFromLine();

    // Material según tipo (normal vs atajo)
    AplicarEstadoVisualCamino(conexion, EstadoVisualCamino.Neutral);
    caminoMesh.SetVisible(!esPorAbajo);

    // Continuar tirando conexiones
    if (propagar)
      nodoB.DeterminarConexiones();
  }

  public LineRenderer CrearLineaIntroCampaniaDesdeIzquierda(Vector3 direccionIzquierda)
  {
    if (linePrefab == null)
    {
      return null;
    }

    Transform lineaAnterior = transform.Find(NombreLineaIntroCampania);
    if (lineaAnterior != null)
    {
      Destroy(lineaAnterior.gameObject);
    }

    direccionIzquierda.y = 0f;
    if (direccionIzquierda.sqrMagnitude <= 0.0001f)
    {
      direccionIzquierda = Vector3.left;
    }
    direccionIzquierda.Normalize();

    float longitud = CalcularLongitudIntroCampania();
    Vector3 fin = transform.position;
    Vector3 inicio = fin + direccionIzquierda * longitud;
    GameObject lineaGO = Instantiate(linePrefab, transform);
    lineaGO.name = NombreLineaIntroCampania;

    LineRenderer lr = lineaGO.GetComponent<LineRenderer>();
    if (lr == null)
    {
      Destroy(lineaGO);
      return null;
    }

    lr.useWorldSpace = true;
    ConfigurarPuntosLineaIntro(lr, inicio, fin, ResolucionLineaIntroCampania);
    OcultarRenderersLineaIntro(lineaGO);
    CrearSegmentosVisualesIntro(lineaGO.transform, lr);
    return lr;
  }

  float CalcularLongitudIntroCampania()
  {
    float distanciaPrimerTramo = 0f;
    for (int i = 0; i < DestinosPosibles.Count; i++)
    {
      Nodo destino = DestinosPosibles[i];
      if (destino == null || !destino.gameObject.activeSelf)
      {
        continue;
      }

      distanciaPrimerTramo = Vector3.Distance(transform.position, destino.transform.position);
      break;
    }

    if (distanciaPrimerTramo <= 0.001f)
    {
      distanciaPrimerTramo = 6.7f;
    }

    return Mathf.Clamp(distanciaPrimerTramo * 0.663f, 4.42f, 7.1825f);
  }

  void ConfigurarPuntosLineaIntro(LineRenderer lr, Vector3 inicio, Vector3 fin, int resolucion)
  {
    if (lr == null)
    {
      return;
    }

    MapDecorator mapDecorator = ObtenerDecoradorMapa();
    int cantidadPuntos = Mathf.Max(2, resolucion);
    lr.positionCount = cantidadPuntos;

    for (int i = 0; i < cantidadPuntos; i++)
    {
      float t = i / (float)(cantidadPuntos - 1);
      Vector3 punto = Vector3.Lerp(inicio, fin, t);
      punto = AjustarPuntoIntroASuelo(punto, mapDecorator);
      lr.SetPosition(i, punto);
    }
  }

  Vector3 AjustarPuntoIntroASuelo(Vector3 punto, MapDecorator mapDecorator)
  {
    float offsetCaminoSobreRelieve = Mathf.Max(CaminoAlturaMinimaSobreRelieve, lineHeightOffset * 3f);
    if (mapDecorator != null && mapDecorator.TrySampleSurface(punto, out var surfacePoint, out _, offsetCaminoSobreRelieve))
    {
      punto.y = surfacePoint.y;
    }

    return punto;
  }

  void OcultarRenderersLineaIntro(GameObject lineaGO)
  {
    if (lineaGO == null)
    {
      return;
    }

    LineRenderer lr = lineaGO.GetComponent<LineRenderer>();
    if (lr != null)
    {
      lr.enabled = false;
    }

    MeshRenderer meshRenderer = lineaGO.GetComponent<MeshRenderer>();
    if (meshRenderer != null)
    {
      meshRenderer.enabled = false;
    }
  }

  void CrearSegmentosVisualesDecorativos(Transform raizLinea, LineRenderer lineaBase, string nombreBase, int cantidadSegmentos)
  {
    if (raizLinea == null || lineaBase == null || lineaBase.positionCount < 2)
    {
      return;
    }

    for (int i = 0; i < cantidadSegmentos; i++)
    {
      float t0 = i / (float)cantidadSegmentos;
      float t1 = (i + 1) / (float)cantidadSegmentos;
      float alpha = Mathf.Lerp(AlphaIntroCampaniaMinimo, AlphaIntroCampaniaMaximo, t1);

      GameObject segmentoGO = Instantiate(linePrefab, raizLinea);
      segmentoGO.name = nombreBase + "_Segmento_" + (i + 1);
      ConfigurarSegmentoVisualIntro(segmentoGO, lineaBase, t0, t1, alpha);
    }
  }

  void CrearSegmentosVisualesOutro(Transform raizOutro, LineRenderer lineaBase)
  {
    CrearSegmentosVisualesDecorativos(raizOutro, lineaBase, NombreLineaOutroCampania, SegmentosLineaOutroCampania);
  }

  public LineRenderer CrearLineaDecorativaFinalHaciaDerecha(Vector3 direccionDerecha)
  {
    if (linePrefab == null)
    {
      return null;
    }

    Transform lineaAnterior = transform.Find(NombreLineaOutroCampania);
    if (lineaAnterior != null)
    {
      Destroy(lineaAnterior.gameObject);
    }

    direccionDerecha.y = 0f;
    if (direccionDerecha.sqrMagnitude <= 0.0001f)
    {
      direccionDerecha = Vector3.right;
    }
    direccionDerecha.Normalize();

    float longitud = CalcularLongitudOutroCampania();
    Vector3 inicio = transform.position;
    Vector3 fin = inicio + direccionDerecha * longitud;
    GameObject lineaGO = Instantiate(linePrefab, transform);
    lineaGO.name = NombreLineaOutroCampania;

    LineRenderer lr = lineaGO.GetComponent<LineRenderer>();
    if (lr == null)
    {
      Destroy(lineaGO);
      return null;
    }

    lr.useWorldSpace = true;
    ConfigurarPuntosLineaOutro(lr, inicio, fin, ResolucionLineaOutroCampania);
    OcultarRenderersLineaIntro(lineaGO);
    CrearSegmentosVisualesOutro(lineaGO.transform, lr);
    return lr;
  }

  float CalcularLongitudOutroCampania()
  {
    float distanciaUltimoTramo = 0f;
    if (scContenedorNodos2 != null && scContenedorNodos2.listTodosNodos != null)
    {
      for (int i = 0; i < scContenedorNodos2.listTodosNodos.Count; i++)
      {
        Nodo origen = scContenedorNodos2.listTodosNodos[i];
        if (origen == null || !origen.TieneConexionHacia(this))
        {
          continue;
        }

        distanciaUltimoTramo = Vector3.Distance(origen.transform.position, transform.position);
        break;
      }
    }

    if (distanciaUltimoTramo <= 0.001f)
    {
      distanciaUltimoTramo = 6.2f;
    }

    return Mathf.Clamp(distanciaUltimoTramo * MultiplicadorLongitudOutroCampania, LongitudMinimaOutroCampania, LongitudMaximaOutroCampania);
  }

  void ConfigurarPuntosLineaOutro(LineRenderer lr, Vector3 inicio, Vector3 fin, int resolucion)
  {
    if (lr == null)
    {
      return;
    }

    MapDecorator mapDecorator = ObtenerDecoradorMapa();
    int cantidadPuntos = Mathf.Max(2, resolucion);
    lr.positionCount = cantidadPuntos;

    Vector3 direccion = (fin - inicio).normalized;
    Vector3 perpendicular = Vector3.Cross(Vector3.up, direccion);
    if (perpendicular.sqrMagnitude <= 0.0001f)
    {
      perpendicular = Vector3.forward;
    }
    perpendicular.Normalize();

    for (int i = 0; i < cantidadPuntos; i++)
    {
      float t = i / (float)(cantidadPuntos - 1);
      Vector3 punto = Vector3.Lerp(inicio, fin, t);
      float envolvente = Mathf.Sin(t * Mathf.PI);
      float desplazamiento = Mathf.Sin(t * Mathf.PI * 1.65f) * CurvaturaLateralOutroCampania;
      desplazamiento += Mathf.Sin(t * Mathf.PI * 3.1f) * SinuosidadOutroCampania;
      punto += perpendicular * (desplazamiento * envolvente);
      punto = AjustarPuntoIntroASuelo(punto, mapDecorator);
      lr.SetPosition(i, punto);
    }
  }

  bool EsNodoFinalZona()
  {
    return posXNodo == 11;
  }

  void AsegurarNodoFinalSiempreRevelado()
  {
    if (!EsNodoFinalZona())
    {
      return;
    }

    if (tipoNodo <= 0)
    {
      tipoNodo = 10;
    }

    revelado = true;
    esMisterioso = false;
    numVisualActual = tipoNodo;
  }

  void SincronizarLineaDecorativaNodoFinal()
  {
    Transform lineaOutro = transform.Find(NombreLineaOutroCampania);
    if (!EsNodoFinalZona())
    {
      if (lineaOutro != null)
      {
        Destroy(lineaOutro.gameObject);
      }
      return;
    }

    bool mostrar = visiblePorVision && revelado && gameObject.activeInHierarchy;
    if (!mostrar)
    {
      if (lineaOutro != null)
      {
        lineaOutro.gameObject.SetActive(false);
      }
      return;
    }

    if (lineaOutro == null)
    {
      LineRenderer lr = CrearLineaDecorativaFinalHaciaDerecha(Vector3.right);
      lineaOutro = lr != null ? lr.transform : null;
    }

    if (lineaOutro != null)
    {
      lineaOutro.gameObject.SetActive(true);
    }
  }

  void CrearSegmentosVisualesIntro(Transform raizIntro, LineRenderer lineaBase)
  {
    if (raizIntro == null || lineaBase == null || lineaBase.positionCount < 2)
    {
      return;
    }
    CrearSegmentosVisualesDecorativos(raizIntro, lineaBase, NombreLineaIntroCampania, SegmentosLineaIntroCampania);
  }

  void ConfigurarSegmentoVisualIntro(GameObject segmentoGO, LineRenderer lineaBase, float t0, float t1, float alpha)
  {
    if (segmentoGO == null)
    {
      return;
    }

    LineRenderer lrSegmento = segmentoGO.GetComponent<LineRenderer>();
    if (lrSegmento == null)
    {
      Destroy(segmentoGO);
      return;
    }

    const int puntosPorSegmento = 4;
    lrSegmento.useWorldSpace = true;
    lrSegmento.positionCount = puntosPorSegmento;
    lrSegmento.startWidth = 0f;
    lrSegmento.endWidth = 0f;

    for (int i = 0; i < puntosPorSegmento; i++)
    {
      float t = Mathf.Lerp(t0, t1, i / (float)(puntosPorSegmento - 1));
      lrSegmento.SetPosition(i, CalcularPosicionEnCurva(lineaBase, t));
    }

    CaminoMesh caminoMesh = segmentoGO.GetComponent<CaminoMesh>();
    if (caminoMesh == null)
    {
      caminoMesh = segmentoGO.AddComponent<CaminoMesh>();
    }

    caminoMesh.SetWidth(ObtenerAnchoVisualCamino(false));
    caminoMesh.SetYOffset(Mathf.Max(CaminoYOffsetMallaMinimo, lineHeightOffset));
    caminoMesh.RebuildFromLine();
    AplicarMaterialIntroCampania(segmentoGO.transform, CrearMaterialIntroCampania(alpha));
  }

  Material CrearMaterialIntroCampania(float alpha)
  {
    Material materialBase = MaterialCaminoOriginal != null ? MaterialCaminoOriginal : MaterialCaminoMarcado;
    if (materialBase == null)
    {
      return null;
    }

    Material material = new Material(materialBase);
    material.name = materialBase.name + "_IntroCampania_" + Mathf.RoundToInt(alpha * 100f);
    Color color = Color.white;
    if (material.HasProperty(ShaderBaseColorId))
    {
      color = material.GetColor(ShaderBaseColorId);
    }
    else if (material.HasProperty(ShaderColorId))
    {
      color = material.GetColor(ShaderColorId);
    }

    color.a = Mathf.Clamp01(alpha);
    if (material.HasProperty(ShaderColorId))
    {
      material.SetColor(ShaderColorId, color);
    }
    if (material.HasProperty(ShaderBaseColorId))
    {
      material.SetColor(ShaderBaseColorId, color);
    }

    ConfigurarMaterialTransparenteIntro(material);
    return material;
  }

  void ConfigurarMaterialTransparenteIntro(Material material)
  {
    if (material == null)
    {
      return;
    }

    if (material.HasProperty("_Surface"))
    {
      material.SetFloat("_Surface", 1f);
    }
    if (material.HasProperty("_Mode"))
    {
      material.SetFloat("_Mode", 3f);
    }

    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    material.SetInt("_ZWrite", 0);
    material.DisableKeyword("_ALPHATEST_ON");
    material.EnableKeyword("_ALPHABLEND_ON");
    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
  }

  void AplicarMaterialIntroCampania(Transform linea, Material material)
  {
    if (linea == null || material == null)
    {
      return;
    }

    LineRenderer lr = linea.GetComponent<LineRenderer>();
    if (lr != null)
    {
      lr.sharedMaterial = material;
    }

    MeshRenderer mr = linea.GetComponent<MeshRenderer>();
    if (mr != null)
    {
      mr.sharedMaterial = material;
    }
  }
  // Resetea este nodo para reutilizarlo en una nueva zona
  public void ResetearParaNuevaZona()
  {
    tipoNodo = 0;
    tipoNodoOriginalRitual = 0;
    nodoDespejado = false;
    cantidadConexiones = 0;
    revelado = false;
    yatiroConexiones = false;
    LimpiarConexiones();
    nodoIncendiado = false;
    nodoRitual = false;
    esMisterioso = false;
    numVisualActual = -1;
    atajoSubterraneoPendiente = false;
    visiblePorVision = true;
    visibleForzadaPorReveladoEspecial = false;
    faccionScoutReveladaId = "";
    faccionScoutReveladaNombre = "";
    vieneDeNodo = null;
    lineasContinuacionVisionPorDestino.Clear();
    lineasReveladas.Clear();
    lineasPendientesVision.Clear();
    lineasConFadeVisionAplicado.Clear();
    continuacionesVisionConfiguradas.Clear();
    CancelarTodosLosFadesVision();
    nodoConFadeVisionAplicado = false;

    var destruir = new List<GameObject>();
    foreach (Transform child in transform)
    {
      if (child.name.Contains("LineaCaminos")) destruir.Add(child.gameObject);
      if (child.name.Contains(NombreLineaContinuacionVision)) destruir.Add(child.gameObject);
      if (child.name.Contains(NombreLineaOutroCampania)) destruir.Add(child.gameObject);
      if (child.name.Contains("Nodo")) child.gameObject.SetActive(false);
    }
    foreach (var go in destruir) Destroy(go);

    AplicarEstiloVisualSettlement(false);
    transform.GetChild(0).gameObject.SetActive(true);
    transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
    SincronizarVFXPersistentes();
    Invoke("EsconderSiNedukazal", 0.15f);
    gameObject.SetActive(true);
  }

  public void RestaurarDesdeSave(NodeSaveData data)
  {
    if (data == null)
    {
      return;
    }

    gameObject.SetActive(true);
    tipoNodo = data.tipoNodo;
    tipoNodoOriginalRitual = data.tipoNodoOriginalRitual;
    nodoDespejado = data.nodoDespejado;
    cantidadConexiones = 0;
    revelado = data.revelado;
    yatiroConexiones = data.yatiroConexiones;
    nodoIncendiado = data.nodoIncendiado;
    nodoRitual = data.nodoRitual;
    atajoSubterraneoPendiente = data.atajoSubterraneoPendiente;
    visiblePorVision = true;
    visibleForzadaPorReveladoEspecial = data.visibilidadForzadaEspecial;
    faccionScoutReveladaId = data.faccionScoutReveladaId ?? "";
    faccionScoutReveladaNombre = data.faccionScoutReveladaNombre ?? "";
    LimpiarConexiones();
    vieneDeNodo = null;
    lineasContinuacionVisionPorDestino.Clear();
    lineasReveladas.Clear();
    lineasPendientesVision.Clear();
    lineasConFadeVisionAplicado.Clear();
    continuacionesVisionConfiguradas.Clear();
    CancelarTodosLosFadesVision();
    nodoConFadeVisionAplicado = false;
    DestruirContinuacionesCortasPorVision();
    if (tipoNodo == 15 && !nodoRitual && tipoNodoOriginalRitual > 0)
    {
      tipoNodo = tipoNodoOriginalRitual;
      tipoNodoOriginalRitual = 0;
    }
    LimpiarEstadosPersistentesNoValidos();
    AplicarVisualGuardado(data.visualCode, data.esMisterioso);
    SincronizarVFXPersistentes();
    SincronizarLineaDecorativaNodoFinal();
    gameObject.SetActive(data.activo);
  }

  public void AplicarVisualGuardado(int visualCode, bool estadoMisterioso)
  {
    DesactivarGraficosNodo();
    bool tutorialActivo = UsaConfiguracionTutorial();
    bool permiteMisterioTutorial = tutorialActivo && misterioForzadoTutorial && visualCode == 12 && estadoMisterioso;
    esMisterioso = tutorialActivo && !permiteMisterioTutorial ? false : estadoMisterioso;
    numVisualActual = visualCode;

    if (EsNodoFinalZona())
    {
      AsegurarNodoFinalSiempreRevelado();
    }

    if (tutorialActivo && !permiteMisterioTutorial && (numVisualActual == 12 || numVisualActual == 13))
    {
      numVisualActual = tipoNodo;
    }

    int codigoAAplicar = numVisualActual > 0 ? numVisualActual : tipoNodo;
    if (codigoAAplicar <= 0)
    {
      ActivarVisualBaseNoRevelado();
      return;
    }

    if (!ActivarVisualPorCodigo(codigoAAplicar))
    {
      numVisualActual = -1;
      ActivarVisualBaseNoRevelado();
    }
  }

  public void ForzarSettlement(bool mostrarVisualDesdeInicio)
  {
    if (UsaConfiguracionTutorial())
    {
      return;
    }

    tipoNodo = CodigoSettlement;
    tipoNodoOriginalRitual = 0;
    esMisterioso = false;
    atajoSubterraneoPendiente = false;
    nodoIncendiado = false;
    nodoRitual = false;
    SincronizarVFXPersistentes();

    if (mostrarVisualDesdeInicio)
    {
      revelado = true;
      ActivarNodoVisual(CodigoSettlement, false, true);
      CampaignManager.Instance?.MarcarNodoCampaniaTemporal(this, TipoHighlightNodoCampania.Asentamiento);
      return;
    }

    revelado = false;
    numVisualActual = -1;
    DesactivarGraficosNodo();
    ActivarVisualBaseNoRevelado();
  }

  int ObtenerTipoNodoAlLlegar()
  {
    if (!revelado || tipoNodo <= 0)
    {
      Revelar(false);
    }

    bool llegoPorAtajo = conexionLlegada != null && conexionLlegada.EsAtajoSubterraneo;
    int tipoNodoLlegada = tipoNodo;

    if (llegoPorAtajo && atajoSubterraneoPendiente)
    {
      atajoSubterraneoPendiente = false;
      tipoNodoLlegada = 12;
    }

    return tipoNodoLlegada;
  }

  void ConfigurarResultadoAtajoSubterraneo()
  {
    atajoSubterraneoPendiente = false;

    if (!revelado || scMapaManager == null)
    {
      return;
    }

    if (scMapaManager.TirarEmboscadaSubterraneaAtajo(this))
    {
      atajoSubterraneoPendiente = true;
    }
  }

  public void RefrescarCaminosMarcadosDesdeEstadoActual()
  {
    MarcarCaminosPosibles();
  }

  public void AplicarMaterialCaminosSegunAlcance(HashSet<Nodo> nodosAlcanzables)
  {
    if (nodosAlcanzables == null)
    {
      return;
    }

    bool origenAlcanzable = nodosAlcanzables.Contains(this);
    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    foreach (CaminoConexion conexion in conexionesSalientes)
    {
      if (conexion == null || conexion.destino == null)
      {
        continue;
      }

      bool caminoAlcanzable = origenAlcanzable && nodosAlcanzables.Contains(conexion.destino);
      EstadoVisualCamino estado = this == nodoActual
        ? EstadoVisualCamino.Disponible
        : caminoAlcanzable
          ? EstadoVisualCamino.Neutral
          : EstadoVisualCamino.Inactivo;
      AplicarEstadoVisualCamino(conexion, estado);
    }
  }

  public bool EstaVisiblePorVision()
  {
    return visiblePorVision;
  }

  public bool TieneVisibilidadForzadaPorReveladoEspecial()
  {
    return visibleForzadaPorReveladoEspecial;
  }

  public void ForzarVisibleSinRevelarEspecial()
  {
    visibleForzadaPorReveladoEspecial = true;
  }

  public void ForzarVisiblePorReveladoEspecial()
  {
    visibleForzadaPorReveladoEspecial = true;
    Revelar(false);
  }

  public bool EstaReveladoParaExploradores()
  {
    return revelado && !esMisterioso;
  }

  public void AplicarVisibilidadPorVision(bool visible)
  {
    visiblePorVision = visible;
    RestaurarPreviewHoverCaminosPosibles();

    if (!visible)
    {
      CancelarFadeVisionNodo();
      DesactivarPulsoMovimientoNodo();
      DesactivarTodosGraficosNodo();
      SincronizarLineaDecorativaNodoFinal();
      return;
    }

    if (DebeAplicarFadeVisionNodo())
    {
      ProgramarFadeVisionNodo();
      SincronizarLineaDecorativaNodoFinal();
      return;
    }

    RefrescarVisualSegunRevelado();
    RestaurarAlphaGraficosNodoActivos();
    SincronizarLineaDecorativaNodoFinal();
  }

  public void OcultarCaminosPorVision()
  {
    OcultarContinuacionesCortasPorVision();

    foreach (Transform child in transform)
    {
      if (!child.name.Contains("LineaCaminos"))
      {
        continue;
      }

      CancelarFadeVisionLinea(child);
      if (lineasReveladas.Contains(child))
      {
        child.gameObject.SetActive(true);
        RestaurarAlphaTransform(child);
        OcultarDecoracionSobreCamino(child);
      }
      else
      {
        child.gameObject.SetActive(false);
      }
    }
  }

  public void MostrarTodosLosCaminosDebug()
  {
    foreach (CaminoConexion conexion in conexionesSalientes)
    {
      Transform linea = conexion?.linea;
      if (linea == null)
      {
        continue;
      }

      CancelarFadeVisionLinea(linea);
      linea.gameObject.SetActive(true);
      RestaurarAlphaTransform(linea);
      OcultarDecoracionSobreCamino(linea);
    }
  }

  void OcultarContinuacionesCortasPorVision()
  {
    foreach (KeyValuePair<Nodo, Transform> kvp in lineasContinuacionVisionPorDestino)
    {
      if (kvp.Value != null)
      {
        CancelarFadeVisionLinea(kvp.Value);
        kvp.Value.gameObject.SetActive(false);
      }
    }
  }

  void DestruirContinuacionesCortasPorVision()
  {
    var destruir = new List<GameObject>();
    foreach (Transform child in transform)
    {
      if (child.name.Contains(NombreLineaContinuacionVision))
      {
        destruir.Add(child.gameObject);
      }
    }

    for (int i = 0; i < destruir.Count; i++)
    {
      Destroy(destruir[i]);
    }
  }

  void RevelarNodosFuturosIniciales(int profundidad)
  {
    if (profundidad <= 0 || DestinosPosibles == null || DestinosPosibles.Count == 0)
    {
      return;
    }

    HashSet<Nodo> visitados = new HashSet<Nodo>();
    RevelarNodosFuturosInicialesRecursivo(this, profundidad, visitados);
  }

  static void RevelarNodosFuturosInicialesRecursivo(Nodo origen, int profundidadRestante, HashSet<Nodo> visitados)
  {
    if (origen == null || profundidadRestante <= 0)
    {
      return;
    }

    foreach (Nodo destino in origen.DestinosPosibles)
    {
      if (destino == null || !visitados.Add(destino))
      {
        continue;
      }

      origen.MostrarCaminoPorVisionHacia(destino);
      if (origen.posXNodo == 0 && origen.posYNodo == 0 && destino.posXNodo == 1)
      {
        destino.Revelar(false, false);
      }
      else
      {
        destino.ForzarVisibleSinRevelarEspecial();
      }
      RevelarNodosFuturosInicialesRecursivo(destino, profundidadRestante - 1, visitados);
    }
  }

  public void MostrarCaminoPorVisionHacia(Nodo destino)
  {
    if (destino == null)
    {
      return;
    }

    Transform linea = ObtenerConexionHacia(destino)?.linea;
    if (linea != null)
    {
      MostrarLineaPorVision(linea, destino);
      return;
    }

    foreach (Transform child in transform)
    {
      if (!child.name.Contains("LineaCaminos")) continue;

      Nodo nodoDestino = ObtenerDestinoSegunTransformLinea(child);
      if (nodoDestino == destino)
      {
        MostrarLineaPorVision(child, destino);
        return;
      }
    }
  }

  public bool TieneCaminoVisiblePorVisionHacia(Nodo destino)
  {
    if (destino == null)
    {
      return false;
    }

    Transform linea = ObtenerConexionHacia(destino)?.linea;
    if (linea != null)
    {
      return linea.gameObject.activeSelf || lineasPendientesVision.Contains(linea);
    }

    foreach (Transform child in transform)
    {
      if (!child.name.Contains("LineaCaminos"))
      {
        continue;
      }

      if (ObtenerDestinoSegunTransformLinea(child) == destino)
      {
        return child.gameObject.activeSelf || lineasPendientesVision.Contains(child);
      }
    }

    return false;
  }

  public bool EstaCaminoReveladoPorVision(CaminoConexion conexion)
  {
    Transform linea = conexion?.linea;
    return linea != null && (lineasReveladas.Contains(linea) || lineasPendientesVision.Contains(linea));
  }

  public void RestaurarCaminoReveladoPorVisionHacia(Nodo destino)
  {
    if (destino == null)
    {
      return;
    }

    Transform linea = ObtenerConexionHacia(destino)?.linea;
    if (linea == null)
    {
      return;
    }

    CancelarFadeVisionLinea(linea);
    lineasReveladas.Add(linea);
    lineasConFadeVisionAplicado.Add(linea);
    linea.gameObject.SetActive(true);
    RestaurarAlphaTransform(linea);
    OcultarDecoracionSobreCamino(linea);
  }

  public void MostrarContinuacionCortaPorVisionHacia(Nodo destino)
  {
    if (destino == null || linePrefab == null)
    {
      return;
    }

    CaminoConexion conexion = ObtenerConexionHacia(destino);
    if (conexion != null && conexion.EsAtajoSubterraneo)
    {
      return;
    }

    Transform lineaOriginal = conexion?.linea;
    if (lineaOriginal == null)
    {
      foreach (Transform child in transform)
      {
        if (!child.name.Contains("LineaCaminos"))
        {
          continue;
        }

        if (ObtenerDestinoSegunTransformLinea(child) == destino)
        {
          lineaOriginal = child;
          break;
        }
      }
    }

    if (lineaOriginal == null)
    {
      return;
    }

    LineRenderer lrOriginal = lineaOriginal.GetComponent<LineRenderer>();
    if (lrOriginal == null || lrOriginal.positionCount < 2)
    {
      return;
    }

    if (!lineasContinuacionVisionPorDestino.TryGetValue(destino, out Transform lineaContinuacion) || lineaContinuacion == null)
    {
      GameObject lineaContinuacionGO = Instantiate(linePrefab, transform);
      lineaContinuacionGO.name = NombreLineaContinuacionVision;
      lineaContinuacion = lineaContinuacionGO.transform;
      lineasContinuacionVisionPorDestino[destino] = lineaContinuacion;
      continuacionesVisionConfiguradas.Remove(destino);
    }

    if (!continuacionesVisionConfiguradas.Contains(destino))
    {
      ConfigurarContinuacionCortaPorVision(lineaContinuacion, lrOriginal);
      continuacionesVisionConfiguradas.Add(destino);
    }

    MostrarLineaPorVision(lineaContinuacion, destino);
  }

  void ConfigurarContinuacionCortaPorVision(Transform lineaContinuacion, LineRenderer lineaOriginal)
  {
    if (lineaContinuacion == null || lineaOriginal == null)
    {
      return;
    }

    LineRenderer lrContinuacion = lineaContinuacion.GetComponent<LineRenderer>();
    if (lrContinuacion == null)
    {
      return;
    }

    int cantidadOriginal = lineaOriginal.positionCount;
    if (cantidadOriginal < 2)
    {
      lineaContinuacion.gameObject.SetActive(false);
      return;
    }

    float indiceFinalF = Mathf.Clamp((cantidadOriginal - 1) * TramoContinuacionVisionVisible, 1f, cantidadOriginal - 1);
    int indiceFinalEntero = Mathf.Clamp(Mathf.FloorToInt(indiceFinalF), 1, cantidadOriginal - 1);
    float tSegmentoFinal = Mathf.Clamp01(indiceFinalF - indiceFinalEntero);

    List<Vector3> puntos = new List<Vector3>(indiceFinalEntero + 2);
    for (int i = 0; i <= indiceFinalEntero; i++)
    {
      puntos.Add(lineaOriginal.GetPosition(i));
    }

    if (indiceFinalEntero < cantidadOriginal - 1)
    {
      Vector3 puntoInterpolado = Vector3.Lerp(lineaOriginal.GetPosition(indiceFinalEntero), lineaOriginal.GetPosition(indiceFinalEntero + 1), tSegmentoFinal);
      if (!CoincidePosicionCamino(puntos[puntos.Count - 1], puntoInterpolado))
      {
        puntos.Add(puntoInterpolado);
      }
    }

    if (puntos.Count < 2)
    {
      lineaContinuacion.gameObject.SetActive(false);
      return;
    }

    lrContinuacion.useWorldSpace = true;
    lrContinuacion.positionCount = puntos.Count;
    lrContinuacion.SetPositions(puntos.ToArray());

    SetMaterialCamino(lineaContinuacion, ObtenerMaterialCaminoHintVisual());
    AplicarAnchoContinuacionVision(lineaContinuacion, lineaOriginal.transform);

    OcultarDecoracionSobreCamino(lineaContinuacion);
  }

  void AplicarAnchoContinuacionVision(Transform lineaContinuacion, Transform lineaOriginal = null)
  {
    if (lineaContinuacion == null)
    {
      return;
    }

    LineRenderer lr = lineaContinuacion.GetComponent<LineRenderer>();
    if (lr != null)
    {
      lr.startWidth = 0f;
      lr.endWidth = 0f;
    }

    CaminoMesh caminoMesh = lineaContinuacion.GetComponent<CaminoMesh>();
    if (caminoMesh == null)
    {
      caminoMesh = lineaContinuacion.gameObject.AddComponent<CaminoMesh>();
    }

    float anchoBase = ObtenerAnchoVisualCamino(false);
    CaminoMesh caminoOriginal = lineaOriginal != null ? lineaOriginal.GetComponent<CaminoMesh>() : null;
    if (caminoOriginal != null)
    {
      anchoBase = caminoOriginal.GetWidth();
    }

    caminoMesh.SetWidth(anchoBase * MultiplicadorAnchoContinuacionVision);
    caminoMesh.SetYOffset(Mathf.Max(CaminoYOffsetMallaMinimo, lineHeightOffset));
    caminoMesh.RebuildFromLine();
  }

  void MostrarLineaPorVision(Transform linea, Nodo destino = null)
  {
    if (linea == null)
    {
      return;
    }

    bool esContinuacionVision = linea.name.Contains(NombreLineaContinuacionVision);
    if (!esContinuacionVision)
    {
      lineasReveladas.Add(linea);
    }

    bool aplicarFade = DebeAplicarFadeVisionLinea(linea, destino);
    lineasPendientesVision.Add(linea);

    if (!aplicarFade)
    {
      linea.gameObject.SetActive(true);
      RestaurarAlphaTransform(linea);
      lineasConFadeVisionAplicado.Add(linea);
      lineasPendientesVision.Remove(linea);
    }
    else if (!rutinasFadeVisionLineas.ContainsKey(linea))
    {
      linea.gameObject.SetActive(false);
      rutinasFadeVisionLineas[linea] = StartCoroutine(FadeInLineaVision(linea, CalcularRetrasoFadeVision(destino), DuracionFadeVisionCamino));
    }

    OcultarDecoracionSobreCamino(linea);
  }

  void OcultarDecoracionSobreCamino(Transform linea)
  {
    if (linea == null)
    {
      return;
    }

    MapDecorator mapDecorator = ObtenerDecoradorMapa();
    if (mapDecorator == null)
    {
      return;
    }

    LineRenderer lr = linea.GetComponent<LineRenderer>();
    if (lr == null)
    {
      return;
    }

    mapDecorator.OcultarDecoracionRemovibleSobreCamino(lr);
  }

  public void RegistrarFaccionScoutRevelada(string factionId, string factionName)
  {
    faccionScoutReveladaId = factionId ?? "";
    faccionScoutReveladaNombre = factionName ?? "";
  }

  public bool TieneFaccionScoutRevelada()
  {
    return !string.IsNullOrEmpty(faccionScoutReveladaId);
  }

  public string ObtenerFaccionScoutReveladaId()
  {
    return faccionScoutReveladaId;
  }

  public string ObtenerFaccionScoutReveladaNombre()
  {
    return faccionScoutReveladaNombre;
  }

  public void PosicionarObjetoEnNodo(GameObject go)
  {
    if (go == null)
    {
      return;
    }

    go.transform.position = transform.position;
  }

  private void OnMouseDown()
  {
    if (DebeIgnorarEventosOnMousePorRaycastManual())
    {
      return;
    }

    ProcesarClickIzquierdoDesdeRaycast();
  }

  public void ProcesarClickIzquierdoDesdeRaycast()
  {
    if (DebeIgnorarInputMouseNodo())
      return;

    TutorialDirector tutorialNuevo = TutorialDirector.Instance;
    bool tutorialNuevoActivo = tutorialNuevo != null && tutorialNuevo.IsRunning;
    var tm = CampaignManager.Instance != null ? CampaignManager.Instance.scTutorialManager : null;
    if (!tutorialNuevoActivo && tm != null && tm.tutorialActivo)
    {
      // permitir interacción solo en los pasos 2, 11, 17, 21 durante el tutorial
      if (!(tm.pasoActual == 2 || tm.pasoActual == 11 || tm.pasoActual == 17 || tm.pasoActual == 21|| tm.pasoActual == 30))
        return;
    }

    if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButton(0))
    {
      return;
    }

    CaminoConexion conexionSeleccionada = scMapaManager.nodoActual.ObtenerConexionHacia(this);
    if (conexionSeleccionada != null)
    {
      if (CampaignManager.Instance != null && !CampaignManager.Instance.IntentarIniciarViajeDesdeNodoActual())
      {
        return;
      }

      if (!tutorialNuevoActivo && tm != null && tm.tutorialActivo)
      {
        if (tm.pasoActual == 2)
          tm.cerrarPasoEspecifico(2);

        if (tm.pasoActual == 11 || tm.pasoActual == 17 || tm.pasoActual == 21 || tm.pasoActual == 30)
          tm.SiguientePaso();
      }

      CampaignManager.Instance.MoviendoCaravana = true;
      LimpiarPulsosMovimientoNodos();
      RuntimeAnalytics.TrackDesign(
        "campaign",
        "node_selected",
        RuntimeAnalytics.SanitizeToken("type_" + tipoNodo + "_x" + posXNodo + "_y" + posYNodo));
      TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignNodeSelected, gameObject)
        .Add("nodeId", ObtenerTutorialTargetId())
        .Add("type", tipoNodo)
        .Add("x", posXNodo)
        .Add("y", posYNodo));
      MoverJugadorANodo(scMapaManager.nodoActual, this);

      if (!tutorialNuevoActivo && CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 7)
        CampaignManager.Instance.scTutorialManager.SiguientePaso();

      if (conexionSeleccionada.EsAtajoSubterraneo)
      {
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Al viajar por el atajo subterráneo, la moral de la caravana disminuye. -5 Esperanza"));
        CampaignManager.Instance.CambiarEsperanzaActual(-5);
      }
    }
  }

  private void OnMouseOver()
  {
    if (DebeIgnorarEventosOnMousePorRaycastManual())
    {
      return;
    }

    ProcesarClickDerechoDesdeRaycast();
  }

  public void ProcesarClickDerechoDesdeRaycast()
  {
    if (!Input.GetMouseButtonDown(1))
    {
      return;
    }

    if (DebeIgnorarInputMouseNodo(true))
    {
      return;
    }

    CampaignManager.Instance.IntentarEnviarExploradores(this);
  }

  bool DebeIgnorarEventosOnMousePorRaycastManual()
  {
    return CampaignManager.Instance != null && CampaignManager.Instance.UsaRaycastManualNodosCampania;
  }

  bool DebeIgnorarInputMouseNodo(bool esEnvioExploradores = false)
  {
    if (!visiblePorVision || CampaignManager.Instance == null)
    {
      return true;
    }

    AsentamientoManager asentamientoManager = CampaignManager.Instance.ObtenerAsentamientoManager();
    if (asentamientoManager != null && asentamientoManager.TieneInteraccionActiva)
    {
      return true;
    }

    TutorialDirector tutorialNuevo = TutorialDirector.Instance;
    TutorialStep pasoTutorialNuevo = tutorialNuevo != null ? tutorialNuevo.CurrentStep : null;
    if (tutorialNuevo != null && tutorialNuevo.IsRunning && pasoTutorialNuevo != null && pasoTutorialNuevo.id == "postbatalla2")
    {
      return true;
    }

    if (tutorialNuevo != null && tutorialNuevo.IsRunning && !esEnvioExploradores && DebeBloquearViajeEnPasoTutorial(pasoTutorialNuevo))
    {
      return true;
    }

    if (tutorialNuevo != null && tutorialNuevo.IsRunning && !tutorialNuevo.AllowsInput(ObtenerTutorialTargetId()))
    {
      return !(esEnvioExploradores && DebePermitirEnvioExploradoresTutorial(pasoTutorialNuevo));
    }

    // TutorialManager legacy deshabilitado: no debe seguir filtrando input de nodos.
    // var tm = CampaignManager.Instance.scTutorialManager;
    // if (tm != null && tm.tutorialActivo)
    // {
    //   // Permitir interaccion solo en los pasos de mapa durante el tutorial.
    //   bool permitirExploradoresTutorial = esEnvioExploradores && tm.pasoActual == 20 && EsClaroMisteriosoTutorial();
    //   if (!(permitirExploradoresTutorial || tm.pasoActual == 2 || tm.pasoActual == 11 || tm.pasoActual == 17 || tm.pasoActual == 21 || tm.pasoActual == 30))
    //     return true;
    // }

    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
    {
      bool tooltipActivo = TooltipNodos.Instance != null &&
                           TooltipNodos.Instance.tooltipObject != null &&
                           TooltipNodos.Instance.tooltipObject.activeInHierarchy;
      bool permitirExploradoresSobreUI = esEnvioExploradores && EsClaroMisteriosoTutorial();
      bool permitirNodoTutorialSobreUI = !esEnvioExploradores && DebePermitirClickNodoTutorialSobreUI(pasoTutorialNuevo);
      if (!tooltipActivo && !permitirExploradoresSobreUI && !permitirNodoTutorialSobreUI)
        return true;
    }

    return false;
  }

  bool DebePermitirClickNodoTutorialSobreUI(TutorialStep pasoTutorial)
  {
    if (pasoTutorial == null || !EsPasoInteraccionNodoTutorial(pasoTutorial))
    {
      return false;
    }

    return string.Equals(pasoTutorial.targetId, ObtenerTutorialTargetId(), System.StringComparison.OrdinalIgnoreCase);
  }

  static bool EsPasoInteraccionNodoTutorial(TutorialStep pasoTutorial)
  {
    if (pasoTutorial == null || !EsTargetNodoTutorialCampania(pasoTutorial.targetId))
    {
      return false;
    }

    if (pasoTutorial.id == "Exploracion2" || pasoTutorial.id == "exploracion2")
    {
      return true;
    }

    if (pasoTutorial.advanceMode != TutorialAdvanceMode.Event || pasoTutorial.advanceConditions == null)
    {
      return false;
    }

    for (int i = 0; i < pasoTutorial.advanceConditions.Count; i++)
    {
      TutorialCondition condition = pasoTutorial.advanceConditions[i];
      if (condition != null && EsEventoInteraccionNodoTutorial(condition.eventId))
      {
        return true;
      }
    }

    return false;
  }

  static bool EsTargetNodoTutorialCampania(string targetId)
  {
    return !string.IsNullOrEmpty(targetId)
      && (targetId.StartsWith("tut_nodo", System.StringComparison.OrdinalIgnoreCase)
        || targetId.StartsWith("tuto_nodo", System.StringComparison.OrdinalIgnoreCase));
  }

  static bool EsEventoInteraccionNodoTutorial(string eventId)
  {
    return eventId == TutorialEventNames.CampaignNodeSelected
      || eventId == TutorialEventNames.CampaignNodeArrived
      || eventId == TutorialEventNames.CampaignResourceNodeContinued
      || eventId == TutorialEventNames.CampaignMissingPeopleEventContinued
      || eventId == TutorialEventNames.CampaignRestNodeContinued
      || eventId == TutorialEventNames.CampaignRestRandomEventContinued
      || eventId == TutorialEventNames.CampaignScoutsExplorationCompleted;
  }

  bool DebeBloquearViajeEnPasoTutorial(TutorialStep pasoTutorial)
  {
    if (pasoTutorial == null)
    {
      return false;
    }

    if (pasoTutorial.id == "Exploracion2" || pasoTutorial.id == "exploracion2")
    {
      return EsClaroMisteriosoTutorial();
    }

    return pasoTutorial.id == "Fatiga2"
      || pasoTutorial.id == "fatiga2"
      || pasoTutorial.id == "Descanso1"
      || pasoTutorial.id == "descanso1"
      || pasoTutorial.id == "Clima1"
      || pasoTutorial.id == "clima1";
  }

  public string ObtenerTutorialTargetId()
  {
    TutorialTarget target = GetComponent<TutorialTarget>();
    if (target != null && !string.IsNullOrEmpty(target.targetId))
    {
      return target.targetId;
    }

    return "nodo_" + posXNodo + "_" + posYNodo;
  }

  bool DebePermitirEnvioExploradoresTutorial(TutorialStep pasoTutorial)
  {
    if (pasoTutorial == null || !EsClaroMisteriosoTutorial())
    {
      return false;
    }

    if (pasoTutorial.id == "Exploracion2" || pasoTutorial.id == "exploracion2")
    {
      return true;
    }

    if (!string.IsNullOrEmpty(pasoTutorial.targetId) && pasoTutorial.targetId.StartsWith("tuto_nodoclaromist"))
    {
      return true;
    }

    if (pasoTutorial.advanceConditions == null)
    {
      return false;
    }

    for (int i = 0; i < pasoTutorial.advanceConditions.Count; i++)
    {
      TutorialCondition condition = pasoTutorial.advanceConditions[i];
      if (condition != null && condition.eventId == TutorialEventNames.CampaignScoutsExplorationCompleted)
      {
        return true;
      }
    }

    return false;
  }

  bool EsClaroMisteriosoTutorial()
  {
    return CampaignManager.Instance != null
      && CampaignManager.Instance.scTutorialManager != null
      && CampaignManager.Instance.scTutorialManager.EsClaroMisteriosoTutorial(this);
  }

  bool DebeMantenerMisteriosoHastaExpedicion()
  {
    return EsClaroMisteriosoTutorial()
      && !revelandoPorExpedicionTutorial
      && !reveladoPorExpedicionTutorial;
  }

  bool EsNodoInicialSinMisterio()
  {
    return posXNodo == 1;
  }


  public void MoverJugadorANodo(Nodo nodoOrigen, Nodo nodoDestino)
  {
    CaminoConexion conexion = nodoOrigen != null ? nodoOrigen.ObtenerConexionHacia(nodoDestino) : null;
    if (nodoDestino == null || conexion == null)
    {
      Debug.LogWarning("Nodo destino no vélido o no está en la lista de destinos posibles.");
      return;
    }

    // Buscar línea
    const float multiplicadorVelocidadMinimaFatiga = 0.6f;
    float velocidadBase = 0.75f + 0.6f / conexion.costoMovimiento;
    int cansancio = CampaignManager.Instance.GetFatigaActual();
    float velocidadReducidaPorFatiga = velocidadBase - cansancio * 0.07f;
    velocidadMovimiento = Mathf.Max(velocidadBase * multiplicadorVelocidadMinimaFatiga, velocidadReducidaPorFatiga);

    Transform lineaTransform = conexion.linea;

    if (lineaTransform == null)
    {
      Debug.LogWarning("No se encontró la línea correspondiente entre los nodos.");
      return;
    }

    bool viajeSubterraneo = conexion.EsAtajoSubterraneo;

    vieneDeNodo = nodoOrigen;
    conexionLlegada = conexion;
    CampaignManager.Instance.ViajeIniciado(conexion);

    if (scMapaManager != null && scMapaManager.goCaravana != null)
    {
      var girarCaravana = scMapaManager.goCaravana.GetComponent<GirarCaravana>();
      if (girarCaravana != null)
        girarCaravana.CambiarSpriteSegunRuta(nodoOrigen, nodoDestino);
    }

   /* StartCoroutine(MoverAloLargoDeLaCurva(0.0f, true, scMapaManager.goCaravana, lineaTransform.GetComponent<LineRenderer>(), 1.5f));
    StartCoroutine(MoverAloLargoDeLaCurva(0.35f, false, scMapaManager.goCaravanafollower1, lineaTransform.GetComponent<LineRenderer>(), 1.3f));
    StartCoroutine(MoverAloLargoDeLaCurva(0.5f, false, scMapaManager.goCaravanafollower2, lineaTransform.GetComponent<LineRenderer>(), 1.15f));
    StartCoroutine(MoverAloLargoDeLaCurva(0.75f, false, scMapaManager.goCaravanafollower3, lineaTransform.GetComponent<LineRenderer>(), 1.0f));
    StartCoroutine(MoverAloLargoDeLaCurva(1.0f, false, scMapaManager.goCaravanafollower4, lineaTransform.GetComponent<LineRenderer>(), 0.95f));
    StartCoroutine(MoverAloLargoDeLaCurva(1.15f, false, scMapaManager.goCaravanafollower5, lineaTransform.GetComponent<LineRenderer>(), 0.85f));
    StartCoroutine(MoverAloLargoDeLaCurva(1.25f, false, scMapaManager.goCaravanafollower6, lineaTransform.GetComponent<LineRenderer>(), 0.8f));
*/
    StartCoroutine(MoverConvoyEnLinea(lineaTransform.GetComponent<LineRenderer>(), viajeSubterraneo));
  }

 /* private IEnumerator MoverAloLargoDeLaCurva(float delay, bool esLaLider, GameObject caravana, LineRenderer lineRenderer, float velRotacion)
  {
    if (caravana == null) yield break;

    GameObject caravanarotacion = esLaLider ? caravana.transform.GetChild(4).gameObject : caravana.transform.GetChild(0).gameObject;

    int resolution = lineRenderer.positionCount;

    Vector3 inicio = lineRenderer.GetPosition(0);
    Vector3 fin = lineRenderer.GetPosition(resolution - 1);
    Vector3 dirAvance = (fin - inicio).normalized;

    // Ajustes
    float velocidadRotacion = velRotacion;                 // más alto = gira más rápido
    float lookAhead = Mathf.Max(0.01f, 2f / Mathf.Max(2, resolution)); // cuánto "mira" hacia adelante en la curva

    // --- Rotación durante el delay (no mover aún) ---
    float elapsed = 0f;
    while (elapsed < delay)
    {
      float tFuture = Mathf.Clamp01(0f + lookAhead);
      Vector3 posFutura = CalcularPosicionEnCurva(lineRenderer, tFuture);

      Vector3 forward = (posFutura - caravanarotacion.transform.position);
      forward.y = 0f;
      if (forward.sqrMagnitude > 0.000001f)
      {
        Quaternion rotObjetivo = Quaternion.LookRotation(forward.normalized, Vector3.up);
        float k = 1f - Mathf.Exp(-velocidadRotacion * Time.deltaTime);
        caravanarotacion.transform.rotation = Quaternion.Slerp(caravanarotacion.transform.rotation, rotObjetivo, k);
      }

      elapsed += Time.deltaTime;
      yield return null;
    }

  // Comienza el movimiento
float t = 0f;
Vector3 ultima = caravana.transform.position;

// Cuánto del recorrido usás para acelerar y frenar (0.15 = 15%)
const float ramp = 0.35f;

while (t < 1f)
{
  float tNorm = Mathf.Clamp01(t);

  // Factor de velocidad: acelera al inicio y frena al final
  float inF  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tNorm / ramp));
  float outF = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - tNorm) / ramp));

  float speedFactor = inF * outF;

  // Piso para que no se quede "cerquita" eternamente
  speedFactor = Mathf.Max(0.03f, speedFactor);

  // Avance de t (acá va el suavizado)
  t += Time.deltaTime * velocidadMovimiento / resolution * speedFactor;

  // Snap final
  if (t >= 0.999f) t = 1f;

  float tClamped = Mathf.Clamp01(t);

  // Posición SIEMPRE con tClamped (monótono)
  Vector3 nuevaPosicion = CalcularPosicionEnCurva(lineRenderer, tClamped);

  Vector3 delta = nuevaPosicion - ultima;
  if (Vector3.Dot(delta, dirAvance) < 0f)
    nuevaPosicion = ultima;

  // Rotación siguiendo la curva (lookahead también con tClamped)
  float tFuturo = Mathf.Clamp01(tClamped + lookAhead);
  Vector3 posFutura = CalcularPosicionEnCurva(lineRenderer, tFuturo);

  Vector3 forward = (posFutura - nuevaPosicion);
  forward.y = 0f;

  if (forward.sqrMagnitude > 0.000001f)
  {
    Quaternion rotObjetivo = Quaternion.LookRotation(forward.normalized, Vector3.up);
    float k = 1f - Mathf.Exp(-velocidadRotacion * Time.deltaTime);
    caravanarotacion.transform.rotation =
      Quaternion.Slerp(caravanarotacion.transform.rotation, rotObjetivo, k);
  }

  caravana.transform.position = nuevaPosicion;
  ultima = nuevaPosicion;

  yield return null;
}

caravana.transform.position = fin;
if (esLaLider)
{
  LlegoCaravana();
}
  }*/



  private Vector3 CalcularPosicionEnCurva(LineRenderer lineRenderer, float t)
  {
    t = Mathf.Clamp01(t);
    int resolution = lineRenderer.positionCount;

    int indexA = Mathf.FloorToInt(t * (resolution - 1));
    int indexB = Mathf.Clamp(indexA + 1, 0, resolution - 1);

    Vector3 posicionA = lineRenderer.GetPosition(indexA);
    Vector3 posicionB = lineRenderer.GetPosition(indexB);

    float tLocal = t * (resolution - 1) - indexA;

    return Vector3.Lerp(posicionA, posicionB, tLocal);
  }

  private MapDecorator ObtenerDecoradorMapa()
  {
    if (CampaignManager.Instance == null || CampaignManager.Instance.scAtributosZona == null)
    {
      return null;
    }

    return CampaignManager.Instance.scAtributosZona.GetComponent<MapDecorator>();
  }

  private Quaternion CalcularRotacionConvoyPorRelieve(Vector3 posActual, Vector3 posFutura, bool viajeSubterraneo)
  {
    Vector3 forward = posFutura - posActual;
    if (forward.sqrMagnitude <= 0.000001f)
    {
      return Quaternion.identity;
    }

    if (viajeSubterraneo)
    {
      forward.y = 0f;
      return forward.sqrMagnitude > 0.000001f
        ? Quaternion.LookRotation(forward.normalized, Vector3.up)
        : Quaternion.identity;
    }

    MapDecorator mapDecorator = ObtenerDecoradorMapa();
    Vector3 up = Vector3.up;
    if (mapDecorator != null && mapDecorator.TrySampleSurface(posActual, out _, out var terrainNormal, 0f))
    {
      up = Vector3.RotateTowards(Vector3.up, terrainNormal.normalized, Mathf.Deg2Rad * 10f, 0f);
    }

    Vector3 forwardPlano = Vector3.ProjectOnPlane(forward, up);
    if (forwardPlano.sqrMagnitude <= 0.000001f)
    {
      forwardPlano = new Vector3(forward.x, 0f, forward.z);
    }

    return forwardPlano.sqrMagnitude > 0.000001f
      ? Quaternion.LookRotation(forwardPlano.normalized, up)
      : Quaternion.identity;
  }

  private float ObtenerAnchoVisualCamino(bool esCaminoDificil)
  {
    return lineWidth * (esCaminoDificil ? CaminoAnchoDificilMultiplicador : CaminoAnchoBaseMultiplicador) * CaminoAnchoGlobalMultiplicador;
  }

  private static float CalcularDesvioCaminoSinuoso(float t, float frecuencia, float amplitud, float caos, float fase, float semilla)
  {
    const float inicioSinuosidad = 0.22f;
    const float finSinuosidad = 0.78f;
    const float suavizadoBorde = 0.11f;

    if (t <= inicioSinuosidad || t >= finSinuosidad)
    {
      return 0f;
    }

    float tNormalizado = Mathf.InverseLerp(inicioSinuosidad, finSinuosidad, t);
    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - inicioSinuosidad) / suavizadoBorde));
    float salida = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((finSinuosidad - t) / suavizadoBorde));
    float envolvente = entrada * salida;
    float curvaPrincipal = Mathf.Sin(tNormalizado * Mathf.PI) * Mathf.Sin(tNormalizado * Mathf.PI * 2f) * amplitud;
    float zigzagSuave = Mathf.Sin((tNormalizado * frecuencia * Mathf.PI * 2f) + fase) * (amplitud * 0.3f);
    float ruido = (Mathf.PerlinNoise(semilla, tNormalizado * 3.3f) - 0.5f) * 2f * caos;
    return (curvaPrincipal + zigzagSuave + ruido) * envolvente;
  }

  // --- Helper materiales: aplica al LR y a la malla ---
  private void SetMaterialCamino(Transform linea, Material mat)
  {
    if (linea == null)
    {
      return;
    }

    var lr = linea.GetComponent<LineRenderer>();
    if (lr != null) lr.sharedMaterial = mat;

    var mr = linea.GetComponent<MeshRenderer>();
    if (mr != null) mr.sharedMaterial = mat;

    AjustarAnchoCaminoSegunEstado(linea);
  }

  void AjustarAnchoCaminoSegunEstado(Transform linea)
  {
    if (linea == null)
    {
      return;
    }

    Nodo nodoOrigen = linea.parent != null ? linea.parent.GetComponent<Nodo>() : null;
    if (nodoOrigen == null)
    {
      return;
    }

    CaminoConexion conexion = nodoOrigen.ObtenerConexionSegunTransformLinea(linea);
    bool esAtajoSuperficie = conexion != null && conexion.EsAtajoSuperficie;
    bool esCaminoSubterraneo = conexion != null && conexion.EsAtajoSubterraneo;
    bool esCaminoDificil = conexion != null && conexion.tipo == TipoCaminoCampania.Dificil;
    bool esCaminoAAldea = conexion != null && conexion.rutaHaciaAldea && !esAtajoSuperficie && !esCaminoSubterraneo;

    float ancho = nodoOrigen.ObtenerAnchoVisualCamino(esCaminoDificil);
    if (esCaminoAAldea)
    {
      ancho *= CaminoAAldeaAnchoMultiplicador;
    }
    if (esAtajoSuperficie)
    {
      ancho *= CaminoAtajoSuperficieAnchoMultiplicador;
    }
    if (esCaminoSubterraneo)
    {
      ancho *= CaminoSubterraneoAnchoMultiplicador;
    }

    LineRenderer lr = linea.GetComponent<LineRenderer>();
    if (lr != null)
    {
      // El LineRenderer solo define la curva; el ancho visible lo controla CaminoMesh.
      // Mantenerlo en 0 evita inflar las exclusiones de MapDecorator.
      lr.startWidth = 0f;
      lr.endWidth = 0f;
    }

    CaminoMesh caminoMesh = linea.GetComponent<CaminoMesh>();
    if (caminoMesh != null)
    {
      caminoMesh.SetWidth(ancho);
      caminoMesh.RebuildFromLine();
    }
  }

  private void PrepararMaterialAtajoSubterraneo()
  {
    if (MaterialAtajo == null || materialAtajoSubterraneoVisual != null)
    {
      return;
    }

    materialAtajoSubterraneoVisual = new Material(MaterialAtajo);

    if (materialAtajoSubterraneoVisual.HasProperty("_Color"))
    {
      Color colorBase = MaterialAtajo.color;
      materialAtajoSubterraneoVisual.color = Color.Lerp(colorBase, Color.black, 0.3f);
    }
  }

  private Material ObtenerMaterialAtajoSubterraneoVisual()
  {
    PrepararMaterialAtajoSubterraneo();
    return materialAtajoSubterraneoVisual != null ? materialAtajoSubterraneoVisual : MaterialAtajo;
  }

  private Material ObtenerMaterialAtajoSuperficie()
  {
    if (materialAtajoSuperficieVisual != null)
    {
      return materialAtajoSuperficieVisual;
    }

    materialAtajoSuperficieVisual = Resources.Load<Material>("Imagenes/Materials/CaminoAtajoSup");
    return materialAtajoSuperficieVisual != null ? materialAtajoSuperficieVisual : MaterialAtajo != null ? MaterialAtajo : MaterialCaminoMarcado;
  }

  private Material ObtenerMaterialCaminoHintVisual()
  {
    if (EsZonaPasoVientoHelado())
    {
      return ObtenerMaterialCaminoHintPasoVientoHeladoVisual();
    }

    if (MaterialCaminoHint != null)
    {
      return MaterialCaminoHint;
    }

    return ObtenerMaterialCaminoHintBaseRuntime();
  }

  private Material ObtenerMaterialCaminoHintPasoVientoHeladoVisual()
  {
    if (materialCaminoHintPasoVientoHeladoVisual != null)
    {
      return materialCaminoHintPasoVientoHeladoVisual;
    }

    Material materialBase = MaterialCaminoHint != null ? MaterialCaminoHint : ObtenerMaterialCaminoHintBaseRuntime();
    if (materialBase == null)
    {
      return MaterialCaminoOriginal;
    }

    materialCaminoHintPasoVientoHeladoVisual = new Material(materialBase);
    materialCaminoHintPasoVientoHeladoVisual.name = "Caminos Hint Paso Viento Helado (Runtime)";
    AplicarColorHintCamino(
      materialCaminoHintPasoVientoHeladoVisual,
      new Color(0.55f, 0.03f, 0.42f, 0.98f),
      new Color(1.15f, 0.08f, 0.78f, 1f));

    return materialCaminoHintPasoVientoHeladoVisual;
  }

  private Material ObtenerMaterialCaminoHintBaseRuntime()
  {
    if (MaterialCaminoHint != null)
    {
      return MaterialCaminoHint;
    }

    if (materialCaminoHintVisual != null)
    {
      return materialCaminoHintVisual;
    }

    Shader shader = MaterialCaminoOriginal != null ? MaterialCaminoOriginal.shader : Shader.Find("Standard");
    if (shader == null)
    {
      return MaterialCaminoOriginal;
    }

    materialCaminoHintVisual = new Material(shader);
    materialCaminoHintVisual.name = "Caminos Hint (Runtime)";

    Texture texturaPrincipal = Resources.Load<Texture>("Imagenes/Materials/textura camino normal");
    Texture texturaNormal = Resources.Load<Texture>("ObjetosMapa/Normal_BosqueArdiente");

    if (materialCaminoHintVisual.HasProperty("_MainTex") && texturaPrincipal != null)
    {
      materialCaminoHintVisual.SetTexture("_MainTex", texturaPrincipal);
      materialCaminoHintVisual.SetTextureScale("_MainTex", new Vector2(2.5f, 1.05f));
      materialCaminoHintVisual.SetTextureOffset("_MainTex", new Vector2(0f, 0.95f));
    }

    if (materialCaminoHintVisual.HasProperty("_BumpMap") && texturaNormal != null)
    {
      materialCaminoHintVisual.SetTexture("_BumpMap", texturaNormal);
    }

    if (materialCaminoHintVisual.HasProperty("_Color"))
    {
      materialCaminoHintVisual.color = new Color(0.5f, 0.124193646f, 0.073113196f, 0.9372549f);
    }

    if (materialCaminoHintVisual.HasProperty("_EmissionColor"))
    {
      materialCaminoHintVisual.SetColor("_EmissionColor", new Color(0.254717f, 0.16321342f, 0f, 1f));
      materialCaminoHintVisual.EnableKeyword("_EMISSION");
    }

    if (materialCaminoHintVisual.HasProperty("_BumpScale")) materialCaminoHintVisual.SetFloat("_BumpScale", 1f);
    if (materialCaminoHintVisual.HasProperty("_GlossMapScale")) materialCaminoHintVisual.SetFloat("_GlossMapScale", 0.583f);
    if (materialCaminoHintVisual.HasProperty("_Glossiness")) materialCaminoHintVisual.SetFloat("_Glossiness", 0f);
    if (materialCaminoHintVisual.HasProperty("_Metallic")) materialCaminoHintVisual.SetFloat("_Metallic", 1f);
    if (materialCaminoHintVisual.HasProperty("_Mode")) materialCaminoHintVisual.SetFloat("_Mode", 2f);
    if (materialCaminoHintVisual.HasProperty("_SrcBlend")) materialCaminoHintVisual.SetFloat("_SrcBlend", 5f);
    if (materialCaminoHintVisual.HasProperty("_DstBlend")) materialCaminoHintVisual.SetFloat("_DstBlend", 10f);
    if (materialCaminoHintVisual.HasProperty("_ZWrite")) materialCaminoHintVisual.SetFloat("_ZWrite", 0f);
    if (materialCaminoHintVisual.HasProperty("_Cull")) materialCaminoHintVisual.SetFloat("_Cull", 2f);
    if (materialCaminoHintVisual.HasProperty("_Cutoff")) materialCaminoHintVisual.SetFloat("_Cutoff", 0.5f);

    materialCaminoHintVisual.SetOverrideTag("RenderType", "Transparent");
    materialCaminoHintVisual.renderQueue = 3000;
    materialCaminoHintVisual.EnableKeyword("_ALPHABLEND_ON");
    if (texturaNormal != null)
    {
      materialCaminoHintVisual.EnableKeyword("_NORMALMAP");
    }
    materialCaminoHintVisual.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
    materialCaminoHintVisual.DisableKeyword("_ALPHATEST_ON");
    materialCaminoHintVisual.DisableKeyword("_ALPHAPREMULTIPLY_ON");

    return materialCaminoHintVisual;
  }

  static void AplicarColorHintCamino(Material material, Color color, Color emision)
  {
    if (material == null)
    {
      return;
    }

    if (material.HasProperty(ShaderColorId))
    {
      material.SetColor(ShaderColorId, color);
    }

    if (material.HasProperty(ShaderBaseColorId))
    {
      material.SetColor(ShaderBaseColorId, color);
    }

    if (material.HasProperty(ShaderEmissionColorId))
    {
      material.SetColor(ShaderEmissionColorId, emision);
      material.EnableKeyword("_EMISSION");
    }
  }

  static bool EsZonaPasoVientoHelado()
  {
    return CampaignManager.Instance != null
      && CampaignManager.Instance.scAtributosZona != null
      && CampaignManager.Instance.scAtributosZona.ID == 2;
  }

  private bool CoincideExtremoLineaConPosicion(LineRenderer lineRenderer, Vector3 posicionObjetivo)
  {
    if (lineRenderer == null || lineRenderer.positionCount <= 0)
    {
      return false;
    }

    return CoincidePosicionCamino(lineRenderer.GetPosition(lineRenderer.positionCount - 1), posicionObjetivo);
  }

  private bool CoincidePosicionCamino(Vector3 posicionA, Vector3 posicionB)
  {
    Vector2 aXZ = new Vector2(posicionA.x, posicionA.z);
    Vector2 bXZ = new Vector2(posicionB.x, posicionB.z);
    return (aXZ - bXZ).sqrMagnitude <= ToleranciaCoincidenciaCaminoXZ * ToleranciaCoincidenciaCaminoXZ;
  }

  private Nodo ObtenerDestinoSegunLinea(LineRenderer lineRenderer)
  {
    if (lineRenderer == null)
    {
      return null;
    }

    return DestinosPosibles.Find(n => n != null && CoincideExtremoLineaConPosicion(lineRenderer, n.transform.position));
  }

  Nodo ObtenerDestinoSegunTransformLinea(Transform linea)
  {
    return ObtenerConexionSegunTransformLinea(linea)?.destino;
  }

  CaminoConexion ObtenerConexionSegunTransformLinea(Transform linea)
  {
    if (linea == null)
    {
      return null;
    }

    foreach (CaminoConexion conexion in conexionesSalientes)
    {
      if (conexion != null && conexion.linea == linea)
      {
        return conexion;
      }
    }

    LineRenderer lr = linea.GetComponent<LineRenderer>();
    Nodo destino = ObtenerDestinoSegunLinea(lr);
    return ObtenerConexionHacia(destino);
  }

  private void ActivarPulsoMovimientoNodo()
  {
    pulsoMovimientoActivo = true;
    nodosConPulsoMovimientoActivos.Add(this);
  }

  private void DesactivarPulsoMovimientoNodo()
  {
    pulsoMovimientoActivo = false;
    nodosConPulsoMovimientoActivos.Remove(this);
    RestablecerPulsoMovimientoNodo();
  }

  void AplicarEstadoVisualCamino(CaminoConexion conexion, EstadoVisualCamino estado)
  {
    if (conexion == null)
    {
      return;
    }

    conexion.estadoVisual = estado;
    SetMaterialCamino(conexion.linea, ResolverMaterialCamino(conexion, estado, conexion.hoverActivo));
  }

  Material ResolverMaterialCamino(CaminoConexion conexion, EstadoVisualCamino estado, bool hover)
  {
    if (estado == EstadoVisualCamino.Hint)
    {
      return ObtenerMaterialCaminoHintVisual();
    }

    Material materialBase = ObtenerMaterialFisicoCamino(conexion);
    if (materialBase == null)
    {
      return MaterialCaminoOriginal;
    }

    bool esNormalSinRuta = conexion != null
      && conexion.tipo == TipoCaminoCampania.Normal
      && !conexion.rutaHaciaAldea;
    if (esNormalSinRuta)
    {
      materialBase = estado switch
      {
        EstadoVisualCamino.Disponible => MaterialCaminoMarcado != null ? MaterialCaminoMarcado : materialBase,
        EstadoVisualCamino.Inactivo => MaterialCaminoUsado != null ? MaterialCaminoUsado : materialBase,
        _ => materialBase
      };
      if (!hover)
      {
        return materialBase;
      }
    }

    if (estado == EstadoVisualCamino.Neutral && !hover)
    {
      return materialBase;
    }

    string clave = materialBase.GetInstanceID() + "_" + estado + "_" + hover;
    if (variantesMaterialCamino.TryGetValue(clave, out Material variante) && variante != null)
    {
      return variante;
    }

    variante = new Material(materialBase)
    {
      name = materialBase.name + " " + estado + (hover ? " Hover" : "") + " (Runtime)"
    };
    AplicarTinteEstadoCamino(variante, materialBase, estado, hover);
    variantesMaterialCamino[clave] = variante;
    return variante;
  }

  Material ObtenerMaterialFisicoCamino(CaminoConexion conexion)
  {
    if (conexion == null)
    {
      return MaterialCaminoOriginal;
    }

    if (conexion.EsAtajoSubterraneo)
    {
      return ObtenerMaterialAtajoSubterraneoVisual();
    }

    if (conexion.EsAtajoSuperficie)
    {
      return ObtenerMaterialAtajoSuperficie();
    }

    if (conexion.rutaHaciaAldea && caminoAAldea != null)
    {
      return caminoAAldea;
    }

    return MaterialCaminoOriginal;
  }

  static void AplicarTinteEstadoCamino(Material variante, Material materialBase, EstadoVisualCamino estado, bool hover)
  {
    if (variante == null || materialBase == null)
    {
      return;
    }

    if (variante.HasProperty("_Color") && materialBase.HasProperty("_Color"))
    {
      Color color = materialBase.GetColor("_Color");
      if (estado == EstadoVisualCamino.Disponible)
      {
        color = Color.Lerp(color, new Color(1f, 1f, 1f, color.a), 0.12f);
      }
      else if (estado == EstadoVisualCamino.Inactivo)
      {
        float gris = color.grayscale;
        color = Color.Lerp(color, new Color(gris, gris, gris, color.a), 0.45f) * 0.68f;
        color.a = materialBase.GetColor("_Color").a;
      }

      if (hover)
      {
        color = Color.Lerp(color, new Color(1f, 1f, 1f, color.a), 0.10f);
      }

      variante.SetColor("_Color", color);
    }

    if (variante.HasProperty("_EmissionColor"))
    {
      Color emisionBase = materialBase.HasProperty("_EmissionColor")
        ? materialBase.GetColor("_EmissionColor")
        : Color.black;
      Color colorBase = materialBase.HasProperty("_Color") ? materialBase.GetColor("_Color") : Color.white;
      Color emision = estado == EstadoVisualCamino.Inactivo
        ? emisionBase * 0.25f
        : estado == EstadoVisualCamino.Disponible
          ? emisionBase + colorBase * 0.12f
          : emisionBase;
      if (hover)
      {
        emision += colorBase * 0.08f;
      }

      variante.SetColor("_EmissionColor", emision);
      variante.EnableKeyword("_EMISSION");
    }
  }

  public bool EsAtajoSuperficieHacia(Nodo nodoDestino)
  {
    return ObtenerConexionHacia(nodoDestino)?.EsAtajoSuperficie ?? false;
  }

  public void MarcarCaminoAAldeaHacia(Nodo nodoDestino)
  {
    if (nodoDestino == null)
    {
      return;
    }

    CaminoConexion conexion = ObtenerConexionHacia(nodoDestino);
    if (conexion == null || conexion.EsAtajoSubterraneo || conexion.EsAtajoSuperficie)
    {
      return;
    }

    conexion.rutaHaciaAldea = true;
    ActualizarMaterialCaminoHaciaDestino(nodoDestino);
  }

  public void LimpiarCaminosAAldea()
  {
    foreach (CaminoConexion conexion in conexionesSalientes)
    {
      if (conexion != null)
      {
        conexion.rutaHaciaAldea = false;
      }
    }
  }

  void ActualizarMaterialCaminoHaciaDestino(Nodo nodoDestino)
  {
    if (nodoDestino == null)
    {
      return;
    }

    CaminoConexion conexion = ObtenerConexionHacia(nodoDestino);
    Transform linea = conexion?.linea;
    if (linea != null)
    {
      AplicarEstadoVisualCamino(conexion, conexion.estadoVisual);
      return;
    }

    foreach (Transform child in transform)
    {
      if (!child.name.Contains("LineaCaminos")) continue;

      LineRenderer lr = child.GetComponent<LineRenderer>();
      if (lr == null || !CoincideExtremoLineaConPosicion(lr, nodoDestino.transform.position))
      {
        continue;
      }

      if (conexion != null)
      {
        AplicarEstadoVisualCamino(conexion, conexion.estadoVisual);
      }
      return;
    }
  }

  private static void LimpiarPulsosMovimientoNodos()
  {
    if (nodosConPulsoMovimientoActivos.Count == 0)
    {
      return;
    }

    List<Nodo> nodosActivos = new List<Nodo>(nodosConPulsoMovimientoActivos);
    foreach (Nodo nodo in nodosActivos)
    {
      if (nodo != null)
      {
        nodo.DesactivarPulsoMovimientoNodo();
      }
    }

    nodosConPulsoMovimientoActivos.Clear();
  }

  private void ActualizarPulsoMovimientoNodo()
  {
    if (!pulsoMovimientoActivo || !gameObject.activeInHierarchy)
    {
      if (visualPulsoMovimientoActual != null)
      {
        RestablecerPulsoMovimientoNodo();
      }

      return;
    }

    Transform visualObjetivo = ObtenerVisualPrincipalNodoParaPulso();
    if (visualObjetivo == null)
    {
      RestablecerPulsoMovimientoNodo();
      return;
    }

    if (visualPulsoMovimientoActual != visualObjetivo)
    {
      RestablecerPulsoMovimientoNodo();
      visualPulsoMovimientoActual = visualObjetivo;
      escalaBaseVisualPulsoMovimiento = visualObjetivo.localScale;
      CachearRenderersPulsoMovimiento(visualObjetivo);
    }

    float desfase = (posXNodo * 0.37f) + (posYNodo * 0.19f);
    float pulso = 0.5f + (0.5f * Mathf.Sin((Time.time * PulsoNodoMovibleVelocidad) + desfase));
    float escalaMaxima = cursorSobreNodo ? PulsoNodoMovibleEscalaMaxHover : PulsoNodoMovibleEscalaMax;
    float multiplicadorEscala = Mathf.Lerp(1f, escalaMaxima, pulso);
    visualPulsoMovimientoActual.localScale = escalaBaseVisualPulsoMovimiento * multiplicadorEscala;
    ActualizarGlowPulsoMovimientoNodo(pulso);
  }

  private void RestablecerPulsoMovimientoNodo()
  {
    RestablecerGlowPulsoMovimientoNodo();

    if (visualPulsoMovimientoActual != null)
    {
      visualPulsoMovimientoActual.localScale = escalaBaseVisualPulsoMovimiento;
    }

    visualPulsoMovimientoActual = null;
    escalaBaseVisualPulsoMovimiento = Vector3.one;
    renderersPulsoMovimientoActuales.Clear();
  }

  void CachearRenderersPulsoMovimiento(Transform visualObjetivo)
  {
    renderersPulsoMovimientoActuales.Clear();
    if (visualObjetivo == null)
    {
      return;
    }

    Renderer[] renderers = visualObjetivo.GetComponentsInChildren<Renderer>(true);
    for (int i = 0; i < renderers.Length; i++)
    {
      if (renderers[i] != null)
      {
        renderersPulsoMovimientoActuales.Add(renderers[i]);
      }
    }
  }

  void ActualizarGlowPulsoMovimientoNodo(float pulso)
  {
    if (bloquePulsoMovimiento == null || renderersPulsoMovimientoActuales.Count == 0)
    {
      return;
    }

    float intensidadGlow = Mathf.Lerp(0.2f, 1f, pulso);
    for (int i = 0; i < renderersPulsoMovimientoActuales.Count; i++)
    {
      Renderer renderer = renderersPulsoMovimientoActuales[i];
      if (renderer == null)
      {
        continue;
      }

      Material materialBase = renderer.sharedMaterial;
      if (materialBase == null)
      {
        continue;
      }

      renderer.GetPropertyBlock(bloquePulsoMovimiento);
      Color colorBase = ObtenerColorBasePulsoMovimiento(materialBase);
      Color colorPulso = Color.Lerp(colorBase, Color.white, PulsoNodoMovibleGlowColorBlend * intensidadGlow);

      if (materialBase.HasProperty("_Color"))
      {
        bloquePulsoMovimiento.SetColor(ShaderColorId, colorPulso);
      }

      if (materialBase.HasProperty("_BaseColor"))
      {
        bloquePulsoMovimiento.SetColor(ShaderBaseColorId, colorPulso);
      }

      if (materialBase.HasProperty("_EmissionColor"))
      {
        Color emissionBase = materialBase.GetColor("_EmissionColor");
        Color emissionPulso = emissionBase + (colorPulso * (PulsoNodoMovibleGlowEmission * intensidadGlow));
        bloquePulsoMovimiento.SetColor(ShaderEmissionColorId, emissionPulso);
      }

      renderer.SetPropertyBlock(bloquePulsoMovimiento);
    }
  }

  void RestablecerGlowPulsoMovimientoNodo()
  {
    if (bloquePulsoMovimiento == null)
    {
      return;
    }

    bloquePulsoMovimiento.Clear();
    for (int i = 0; i < renderersPulsoMovimientoActuales.Count; i++)
    {
      Renderer renderer = renderersPulsoMovimientoActuales[i];
      if (renderer != null)
      {
        renderer.SetPropertyBlock(bloquePulsoMovimiento);
      }
    }
  }

  static Color ObtenerColorBasePulsoMovimiento(Material materialBase)
  {
    if (materialBase == null)
    {
      return Color.white;
    }

    if (materialBase.HasProperty("_BaseColor"))
    {
      return materialBase.GetColor("_BaseColor");
    }

    if (materialBase.HasProperty("_Color"))
    {
      return materialBase.GetColor("_Color");
    }

    return Color.white;
  }

  private Transform ObtenerVisualPrincipalNodoParaPulso()
  {
    if (transform.childCount <= 0)
    {
      return null;
    }

    int indiceVisual = ObtenerIndiceVisualPorCodigo(numVisualActual > 0 ? numVisualActual : tipoNodo);
    if (indiceVisual >= 0 && indiceVisual < transform.childCount)
    {
      Transform visual = transform.GetChild(indiceVisual);
      if (visual != null && visual.gameObject.activeInHierarchy)
      {
        return visual;
      }
    }

    Transform visualBase = transform.GetChild(0);
    return visualBase != null && visualBase.gameObject.activeInHierarchy ? visualBase : null;
  }

  void MarcarCaminosPosibles()
  {
    LimpiarPulsosMovimientoNodos();

    // Salientes del nodo actual
    foreach (Transform child in transform)
    {
      if (!child.name.Contains("LineaCaminos")) continue;
      var lr = child.GetComponent<LineRenderer>();
      if (lr == null) continue;

      Nodo nodoDestino = ObtenerDestinoSegunLinea(lr);
      if (nodoDestino == null)
      {
        SetMaterialCamino(child, MaterialCaminoOriginal);
        continue;
      }

      // Camino normal, lento o atajo
      AplicarEstadoVisualCamino(ObtenerConexionHacia(nodoDestino), EstadoVisualCamino.Disponible);
      nodoDestino.ActivarPulsoMovimientoNodo();
    }
  }
  #endregion

  public void Revelar(bool esAtajo)
  {
    Revelar(esAtajo, true);
  }

  void Revelar(bool esAtajo, bool permitirNodoMisterioso)
  {
    if (DebeMantenerMisteriosoHastaExpedicion())
    {
      ForzarMisteriosoTutorial();
      return;
    }

    bool estabaRevelado = revelado;
    revelado = true;

    if (estabaRevelado && !esMisterioso && !esAtajo && tipoNodo > 0)
    {
      if (visiblePorVision)
      {
        RefrescarVisualSegunRevelado();
        RestaurarAlphaGraficosNodoActivos();
      }

      return;
    }

    //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
   
    if (tipoNodo == 0)
    {
      int rand = UnityEngine.Random.Range(1, 8);

      if (posXNodo == 1)
      {
        switch (rand)
        {
          
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 5; break;
          case 5: tipoNodo = 8; break;
          case 6: tipoNodo = 5; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 2)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 7; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 8; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 3)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 11; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 1; break;
          case 6: tipoNodo = 11; break;
          case 7: tipoNodo = 3; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 4)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 11; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 1; break;
          case 7: tipoNodo = 6; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 5)
      {
        switch (rand)
        {
          case 1: tipoNodo = 3; break;
          case 2: tipoNodo = 3; break;
          case 3: tipoNodo = 3; break;
          case 4: tipoNodo = 14; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 7; break;
          case 7: tipoNodo = 5; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 6)
      {
        switch (rand)
        {
          case 1: tipoNodo = 11; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 8; break;
          case 6: tipoNodo = 11; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 7)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 3; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 1; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 3; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 8)
      {
        switch (rand)
        {
          case 1: tipoNodo = 14; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 14; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 7; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 9)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 8; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 8; break;
          case 5: tipoNodo = 11; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 10)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 3; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 14; break;
          case 5: tipoNodo = 3; break;
          case 6: tipoNodo = 3; break;
          case 7: tipoNodo = 3; break;
        }
      }
      if (posXNodo == 11) { tipoNodo = 10; }
    }

    //Correctores por zona
    //Nedukazal no tiene Santuarios
    if (CampaignManager.Instance.scAtributosZona.ID == 3) //Nedukazal
    {
      if (tipoNodo == 14) tipoNodo = 1; //Santuario a Batalla normal
    }

    ActivarNodoVisual(tipoNodo, esAtajo, estabaRevelado, permitirNodoMisterioso);

    if (esAtajo)
    {
      ConfigurarResultadoAtajoSubterraneo();
    }

    if (!visiblePorVision)
    {
      DesactivarTodosGraficosNodo();
    }

    EvaluarTooltipZonaExpuestaRevelada(estabaRevelado);
    EvaluarTooltipAsentamientoRevelado(estabaRevelado);
  }

  void EvaluarTooltipZonaExpuestaRevelada(bool estabaRevelado)
  {
    if (estabaRevelado || tipoNodo != 11 || esMisterioso)
    {
      return;
    }

    if (scMapaManager == null || scMapaManager.nodoActual == null)
    {
      return;
    }

    if (!scMapaManager.nodoActual.DestinosPosibles.Contains(this))
    {
      return;
    }

    TutorialTooltipManager.TryShow(TooltipZonaExpuestaId);
  }

  void EvaluarTooltipAsentamientoRevelado(bool estabaRevelado)
  {
    if (estabaRevelado || tipoNodo != 4 || esMisterioso)
    {
      return;
    }

    TutorialTooltipManager.TryShow(TooltipAsentamientoId);
    CampaignManager.Instance?.MarcarNodoCampaniaTemporal(this, TipoHighlightNodoCampania.Asentamiento);
  }

  public void RevelarPorExploradores()
  {
    revelandoPorExpedicionTutorial = EsClaroMisteriosoTutorial();
    Revelar(false);
    if (revelandoPorExpedicionTutorial)
    {
      reveladoPorExpedicionTutorial = true;
      revelandoPorExpedicionTutorial = false;
      misterioForzadoTutorial = false;
    }

    if (!esMisterioso)
    {
      ActivarVfxDescubrimiento();
      return;
    }

    esMisterioso = false;
    ActivarNodoVisual(tipoNodo, false, true);
    ActivarVfxDescubrimiento();
    if (!visiblePorVision)
    {
      DesactivarTodosGraficosNodo();
    }
  }

  void MarcarComoMisteriosoPorExploracionFallida()
  {
    if (EsNodoFinalZona())
    {
      Revelar(false, false);
      return;
    }

    if (revelado || tipoNodo == 16 || UsaConfiguracionTutorial() || EsNodoInicialSinMisterio())
    {
      return;
    }

    revelado = true;
    esMisterioso = true;
    numVisualActual = 12;
    AplicarVisualGuardado(numVisualActual, esMisterioso);
    SincronizarVFXPersistentes();

    if (!visiblePorVision)
    {
      DesactivarTodosGraficosNodo();
    }
  }

  public void RevelarComoMisterioso()
  {
    if (EsNodoFinalZona())
    {
      Revelar(false, false);
      return;
    }

    if (UsaConfiguracionTutorial())
    {
      Revelar(false, false);
      return;
    }

    if (EsNodoInicialSinMisterio())
    {
      Revelar(false, false);
      return;
    }

    if (!revelado || tipoNodo <= 0)
    {
      Revelar(false, false);
    }

    revelado = true;
    esMisterioso = true;
    numVisualActual = 12;
    AplicarVisualGuardado(numVisualActual, esMisterioso);
    SincronizarVFXPersistentes();

    if (!visiblePorVision)
    {
      DesactivarTodosGraficosNodo();
    }
  }

  public void TiradaExploracion(int chances, bool continua, string actividadExploradorON = "", bool sinLog = false, int distanciaRestante = -1, bool marcarFallosComoMisterioso = true)
  {
    if (UsaConfiguracionTutorial())
    {
      marcarFallosComoMisterioso = false;
    }

    if (distanciaRestante < 0 && CampaignManager.Instance != null)
    {
      distanciaRestante = Mathf.Max(1, CampaignManager.Instance.ObtenerDistanciaVisionEfectiva());
    }

    if (distanciaRestante == 0)
    {
      return;
    }

    bool yaAvisoLog = sinLog;
    int cappedChance = Mathf.Clamp(chances, 0, 100);

    foreach (Nodo nodo in DestinosPosibles)
    {
      int tirada = UnityEngine.Random.Range(0, 100);
      if (tirada >= cappedChance)
      {
        if (marcarFallosComoMisterioso)
        {
          nodo.MarcarComoMisteriosoPorExploracionFallida();
        }
        continue;
      }

      if (nodo.DebeMantenerMisteriosoHastaExpedicion())
      {
        nodo.ForzarMisteriosoTutorial();
        continue;
      }

      nodo.Revelar(false, false);
      int logChance = Mathf.Min(cappedChance, 90);

      if ((continua || !string.IsNullOrEmpty(actividadExploradorON)) && logChance > 36)
      {
        if (!yaAvisoLog)
        {
          string textoTirada = TRADU.i.Traducir("Tirada: ");
          if (!string.IsNullOrEmpty(actividadExploradorON))
            CampaignManager.Instance.EscribirLog("<color=#7ED6F7>-" + actividadExploradorON + TRADU.i.Traducir(" ha Explorado con Éxito el camino adelante.</color>") + $"({textoTirada}{tirada} < {cappedChance})");
          else
            CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("<color=#7ED6F7>-Durante el Descanso, se ha Explorado con Éxito el camino adelante.</color>") + $"({textoTirada}{tirada} < {cappedChance})");

          yaAvisoLog = true;
        }

        if (continua)
        {
          int nextChance = Mathf.Clamp(logChance - 15, 0, 90);
          int siguienteDistancia = distanciaRestante > 0 ? distanciaRestante - 1 : -1;
          if (nextChance > 0) nodo.TiradaExploracion(nextChance, true, "", true, siguienteDistancia, false);
        }
      }
    }
  }

  public void DesactivarGraficosNodo()
  {
    foreach (Transform child in transform)
    {
      if (!child.name.Contains("Nodo")) continue;
      int idx = child.GetSiblingIndex();
      if (idx == 14 && nodoIncendiado) continue; 
      if (idx == 15 && nodoRitual) continue;  

      child.gameObject.SetActive(false);
    }
  }

  void DesactivarTodosGraficosNodo()
  {
    foreach (Transform child in transform)
    {
      if (child.name.Contains("Nodo"))
      {
        child.gameObject.SetActive(false);
      }
    }
  }

  void RefrescarVisualSegunRevelado()
  {
    if (!visiblePorVision)
    {
      DesactivarTodosGraficosNodo();
      SincronizarLineaDecorativaNodoFinal();
      return;
    }

    if (!revelado)
    {
      DesactivarTodosGraficosNodo();
      ActivarVisualBaseNoRevelado();
      SincronizarLineaDecorativaNodoFinal();
      return;
    }

    AplicarVisualGuardado(numVisualActual, esMisterioso);
    SincronizarVFXPersistentes();
    SincronizarLineaDecorativaNodoFinal();
  }

  bool DebeAplicarFadeVisionNodo()
  {
    return Application.isPlaying
      && !nodoConFadeVisionAplicado
      && !EsNodoFinalZona()
      && scMapaManager != null
      && scMapaManager.nodoActual != null
      && posXNodo > scMapaManager.nodoActual.posXNodo;
  }

  bool DebeAplicarFadeVisionLinea(Transform linea, Nodo destino)
  {
    if (!Application.isPlaying || linea == null || lineasConFadeVisionAplicado.Contains(linea))
    {
      return false;
    }

    if (destino == null)
    {
      destino = ObtenerDestinoSegunTransformLinea(linea);
    }

    return destino != null
      && scMapaManager != null
      && scMapaManager.nodoActual != null
      && destino.posXNodo > scMapaManager.nodoActual.posXNodo;
  }

  float CalcularRetrasoFadeVision(Nodo destino = null)
  {
    if (scMapaManager == null || scMapaManager.nodoActual == null)
    {
      return 0f;
    }

    int posXObjetivo = destino != null ? destino.posXNodo : posXNodo;
    int posYObjetivo = destino != null ? destino.posYNodo : posYNodo;
    int distanciaX = Mathf.Max(0, posXObjetivo - scMapaManager.nodoActual.posXNodo);
    int distanciaY = Mathf.Abs(posYObjetivo - scMapaManager.nodoActual.posYNodo);
    return Mathf.Min(RetrasoMaximoFadeVision, (distanciaX * RetrasoPasoXFadeVision) + (distanciaY * RetrasoPasoYFadeVision));
  }

  void ProgramarFadeVisionNodo()
  {
    CancelarFadeVisionNodo();
    DesactivarTodosGraficosNodo();
    rutinaFadeVisionNodo = StartCoroutine(FadeInNodoVision(CalcularRetrasoFadeVision(), DuracionFadeVisionNodo));
  }

  IEnumerator FadeInNodoVision(float retraso, float duracion)
  {
    if (retraso > 0f)
    {
      yield return new WaitForSeconds(retraso);
    }

    if (!visiblePorVision || !gameObject.activeInHierarchy)
    {
      rutinaFadeVisionNodo = null;
      yield break;
    }

    RefrescarVisualSegunRevelado();
    List<FadeRendererState> renderers = CapturarRenderersGraficosNodoActivos();
    List<FadeTransformState> transformStates = CapturarTransformsGraficosNodoActivos();
    if (renderers.Count == 0 && transformStates.Count == 0)
    {
      nodoConFadeVisionAplicado = true;
      rutinaFadeVisionNodo = null;
      yield break;
    }

    estadosFadeVisionNodo = renderers;
    estadosFadeVisionTransformNodo = transformStates;
    yield return EjecutarFadeVision(renderers, null, transformStates, duracion);
    estadosFadeVisionNodo = null;
    estadosFadeVisionTransformNodo = null;
    nodoConFadeVisionAplicado = true;
    rutinaFadeVisionNodo = null;
  }

  IEnumerator FadeInLineaVision(Transform linea, float retraso, float duracion)
  {
    if (retraso > 0f)
    {
      yield return new WaitForSeconds(retraso);
    }

    if (linea == null || !lineasPendientesVision.Contains(linea) || !gameObject.activeInHierarchy)
    {
      if (linea != null)
      {
        rutinasFadeVisionLineas.Remove(linea);
      }

      yield break;
    }

    linea.gameObject.SetActive(true);
    OcultarDecoracionSobreCamino(linea);
    List<FadeRendererState> renderers = CapturarRenderersActivos(linea);
    List<FadeCaminoMeshState> caminoMeshes = CapturarCaminoMeshesActivos(linea);
    estadosFadeVisionLineas[linea] = renderers;
    estadosFadeVisionMeshesLineas[linea] = caminoMeshes;

    if (renderers.Count > 0 || caminoMeshes.Count > 0)
    {
      yield return EjecutarFadeVision(renderers, caminoMeshes, null, duracion);
    }
    else
    {
      RestaurarAlphaTransform(linea);
    }

    estadosFadeVisionLineas.Remove(linea);
    estadosFadeVisionMeshesLineas.Remove(linea);
    lineasConFadeVisionAplicado.Add(linea);
    lineasPendientesVision.Remove(linea);
    rutinasFadeVisionLineas.Remove(linea);
  }

  IEnumerator EjecutarFadeVision(List<FadeRendererState> renderers, List<FadeCaminoMeshState> caminoMeshes, List<FadeTransformState> transforms, float duracion)
  {
    AplicarAlphaRenderers(renderers, 0f);
    AplicarAnchoCaminoMeshes(caminoMeshes, 0f);
    AplicarEscalaTransforms(transforms, 0f);
    float tiempo = 0f;
    duracion = Mathf.Max(0.01f, duracion);

    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float progreso = Mathf.Clamp01(tiempo / duracion);
      AplicarAlphaRenderers(renderers, progreso);
      AplicarAnchoCaminoMeshes(caminoMeshes, progreso);
      AplicarEscalaTransforms(transforms, progreso);
      yield return null;
    }

    AplicarAlphaRenderers(renderers, 1f);
    AplicarAnchoCaminoMeshes(caminoMeshes, 1f);
    AplicarEscalaTransforms(transforms, 1f);
  }

  List<FadeRendererState> CapturarRenderersGraficosNodoActivos()
  {
    List<FadeRendererState> renderers = new List<FadeRendererState>();

    foreach (Transform child in transform)
    {
      if (!child.name.Contains("Nodo") || !child.gameObject.activeInHierarchy)
      {
        continue;
      }

      AgregarRenderersActivos(child, renderers);
    }

    return renderers;
  }

  List<FadeTransformState> CapturarTransformsGraficosNodoActivos()
  {
    List<FadeTransformState> transforms = new List<FadeTransformState>();

    foreach (Transform child in transform)
    {
      if (!child.name.Contains("Nodo") || !child.gameObject.activeInHierarchy)
      {
        continue;
      }

      transforms.Add(new FadeTransformState
      {
        transform = child,
        escalaOriginal = child.localScale
      });
    }

    return transforms;
  }

  List<FadeRendererState> CapturarRenderersActivos(Transform raiz)
  {
    List<FadeRendererState> renderers = new List<FadeRendererState>();
    AgregarRenderersActivos(raiz, renderers);
    return renderers;
  }

  List<FadeCaminoMeshState> CapturarCaminoMeshesActivos(Transform raiz)
  {
    List<FadeCaminoMeshState> caminoMeshes = new List<FadeCaminoMeshState>();
    if (raiz == null || !raiz.gameObject.activeInHierarchy)
    {
      return caminoMeshes;
    }

    CaminoMesh[] encontrados = raiz.GetComponentsInChildren<CaminoMesh>(true);
    for (int i = 0; i < encontrados.Length; i++)
    {
      CaminoMesh caminoMesh = encontrados[i];
      if (caminoMesh == null || !caminoMesh.gameObject.activeInHierarchy)
      {
        continue;
      }

      caminoMeshes.Add(new FadeCaminoMeshState
      {
        caminoMesh = caminoMesh,
        anchoOriginal = caminoMesh.GetWidth()
      });
    }

    return caminoMeshes;
  }

  void AgregarRenderersActivos(Transform raiz, List<FadeRendererState> renderers)
  {
    if (raiz == null)
    {
      return;
    }

    Renderer[] encontrados = raiz.GetComponentsInChildren<Renderer>(true);
    for (int i = 0; i < encontrados.Length; i++)
    {
      Renderer renderer = encontrados[i];
      if (renderer == null || !renderer.gameObject.activeInHierarchy)
      {
        continue;
      }

      Material material = renderer.sharedMaterial;
      if (material == null)
      {
        continue;
      }

      bool usaColor = material.HasProperty(ShaderColorId);
      bool usaBaseColor = material.HasProperty(ShaderBaseColorId);
      if (!usaColor && !usaBaseColor)
      {
        continue;
      }

      renderers.Add(new FadeRendererState
      {
        renderer = renderer,
        usaColor = usaColor,
        colorOriginal = usaColor ? material.GetColor(ShaderColorId) : Color.white,
        usaBaseColor = usaBaseColor,
        baseColorOriginal = usaBaseColor ? material.GetColor(ShaderBaseColorId) : Color.white
      });
    }
  }

  public void ForzarMisteriosoTutorial()
  {
    if (EsNodoFinalZona())
    {
      Revelar(false, false);
      return;
    }

    misterioForzadoTutorial = true;
    reveladoPorExpedicionTutorial = false;
    revelandoPorExpedicionTutorial = false;
    revelado = true;
    esMisterioso = true;
    numVisualActual = 12;
    RefrescarVisualSegunRevelado();
  }

  void AplicarAlphaRenderers(List<FadeRendererState> renderers, float alphaNormalizado)
  {
    if (renderers == null)
    {
      return;
    }

    float alpha = Mathf.Clamp01(alphaNormalizado);

    for (int i = 0; i < renderers.Count; i++)
    {
      FadeRendererState estado = renderers[i];
      if (estado == null || estado.renderer == null)
      {
        continue;
      }

      bloqueFadeVision.Clear();

      if (estado.usaColor)
      {
        Color color = estado.colorOriginal;
        color.a *= alpha;
        bloqueFadeVision.SetColor(ShaderColorId, color);
      }

      if (estado.usaBaseColor)
      {
        Color colorBase = estado.baseColorOriginal;
        colorBase.a *= alpha;
        bloqueFadeVision.SetColor(ShaderBaseColorId, colorBase);
      }

      estado.renderer.SetPropertyBlock(bloqueFadeVision);
    }
  }

  void AplicarAnchoCaminoMeshes(List<FadeCaminoMeshState> caminoMeshes, float alphaNormalizado)
  {
    if (caminoMeshes == null)
    {
      return;
    }

    float escala = Mathf.SmoothStep(0.08f, 1f, Mathf.Clamp01(alphaNormalizado));
    for (int i = 0; i < caminoMeshes.Count; i++)
    {
      FadeCaminoMeshState estado = caminoMeshes[i];
      if (estado == null || estado.caminoMesh == null)
      {
        continue;
      }

      estado.caminoMesh.SetWidth(estado.anchoOriginal * escala);
      estado.caminoMesh.RebuildFromLine();
    }
  }

  void AplicarEscalaTransforms(List<FadeTransformState> transforms, float alphaNormalizado)
  {
    if (transforms == null)
    {
      return;
    }

    float escala = Mathf.SmoothStep(0.78f, 1f, Mathf.Clamp01(alphaNormalizado));
    for (int i = 0; i < transforms.Count; i++)
    {
      FadeTransformState estado = transforms[i];
      if (estado == null || estado.transform == null)
      {
        continue;
      }

      estado.transform.localScale = estado.escalaOriginal * escala;
    }
  }

  void RestaurarAlphaGraficosNodoActivos()
  {
    AplicarAlphaRenderers(CapturarRenderersGraficosNodoActivos(), 1f);
  }

  void RestaurarAlphaTransform(Transform raiz)
  {
    AplicarAlphaRenderers(CapturarRenderersActivos(raiz), 1f);
  }

  void CancelarFadeVisionNodo()
  {
    if (rutinaFadeVisionNodo != null)
    {
      StopCoroutine(rutinaFadeVisionNodo);
      rutinaFadeVisionNodo = null;
    }

    AplicarAlphaRenderers(estadosFadeVisionNodo, 1f);
    AplicarEscalaTransforms(estadosFadeVisionTransformNodo, 1f);
    estadosFadeVisionNodo = null;
    estadosFadeVisionTransformNodo = null;
  }

  void CancelarFadeVisionLinea(Transform linea)
  {
    if (linea == null)
    {
      return;
    }

    if (rutinasFadeVisionLineas.TryGetValue(linea, out Coroutine rutina) && rutina != null)
    {
      StopCoroutine(rutina);
      rutinasFadeVisionLineas.Remove(linea);
    }

    if (estadosFadeVisionLineas.TryGetValue(linea, out List<FadeRendererState> renderers))
    {
      AplicarAlphaRenderers(renderers, 1f);
      estadosFadeVisionLineas.Remove(linea);
    }

    if (estadosFadeVisionMeshesLineas.TryGetValue(linea, out List<FadeCaminoMeshState> caminoMeshes))
    {
      AplicarAnchoCaminoMeshes(caminoMeshes, 1f);
      estadosFadeVisionMeshesLineas.Remove(linea);
    }

    lineasPendientesVision.Remove(linea);
  }

  void CancelarTodosLosFadesVision()
  {
    CancelarFadeVisionNodo();

    if (rutinasFadeVisionLineas.Count > 0)
    {
      List<Coroutine> rutinas = new List<Coroutine>(rutinasFadeVisionLineas.Values);
      for (int i = 0; i < rutinas.Count; i++)
      {
        if (rutinas[i] != null)
        {
          StopCoroutine(rutinas[i]);
        }
      }

      rutinasFadeVisionLineas.Clear();
    }

    foreach (List<FadeRendererState> renderers in estadosFadeVisionLineas.Values)
    {
      AplicarAlphaRenderers(renderers, 1f);
    }

    foreach (List<FadeCaminoMeshState> caminoMeshes in estadosFadeVisionMeshesLineas.Values)
    {
      AplicarAnchoCaminoMeshes(caminoMeshes, 1f);
    }

    estadosFadeVisionLineas.Clear();
    estadosFadeVisionMeshesLineas.Clear();
    lineasPendientesVision.Clear();
  }

  bool ActivarVisualPorCodigo(int codigo)
  {

    int indice = -1;
    AplicarEstiloVisualSettlement(codigo == CodigoSettlement);
    switch (codigo)
    {
      case 1: indice = 1; break;  // 1: Combate directo (batalla normal)
      case 2: indice = 2; break;  // 2: Evento aleatorio
      case 3: indice = 3; break;  // 3: Claro tranquilo (posible descanso / efecto benigno)
      case 4: indice = 4; break;  // 4: Asentamiento
      case 5: indice = 5; break;  // 5: Recolección de recursos
      case 6: indice = 6; break;  // 6: Puesto de comercio
      case 7: indice = 7; break;  // 7: Adquisición de personajes (reclutamiento)
      case 8: indice = 8; break;  // 8: Combate contra enemigos de élite
      case 10: indice = 8; break; // 10: Batalla final de la zona (visual similar a élite)
      case 11: indice = 9; break; // 11: Zona expuesta (emboscada)
      case 12: indice = 10; break; // 12: Nodo misterioso / posible batalla subterránea
      case 13: indice = 11; break; // 13: Salida del atajo subterráneo
      case 14: indice = 12; break; //14: Santuario
      case 15: indice = 15; break; //15: Ritual Kale'Tav
      case 16: indice = 16; break; //16: Misión de Salvamento
    }


    if (indice < 0 || indice >= transform.childCount) return false;
    transform.GetChild(indice).gameObject.SetActive(true);
    return true;
  }

  int ObtenerIndiceVisualPorCodigo(int codigo)
  {
    switch (codigo)
    {
      case 1: return 1;  // 1: Combate directo (batalla normal)
      case 2: return 2;  // 2: Evento aleatorio
      case 3: return 3;  // 3: Claro tranquilo (posible descanso / efecto benigno)
      case 4: return 4;  // 4: Asentamiento
      case 5: return 5;  // 5: Recoleccion de recursos
      case 6: return 6;  // 6: Puesto de comercio
      case 7: return 7;  // 7: Adquisicion de personajes (reclutamiento)
      case 8: return 8;  // 8: Combate contra enemigos de elite
      case 10: return 8; // 10: Batalla final de la zona (visual similar a elite)
      case 11: return 9; // 11: Zona expuesta (emboscada)
      case 12: return 10; // 12: Nodo misterioso / posible batalla subterranea
      case 13: return 11; // 13: Salida del atajo subterraneo
      case 14: return 12; //14: Santuario
      case 15: return 15; //15: Ritual Kale'Tav
      case 16: return 16; //16: Mision de Salvamento
      default: return -1;
    }
  }

  void AplicarEstiloVisualSettlement(bool destacar)
  {
    if (IndiceVisualSettlement < 0 || IndiceVisualSettlement >= transform.childCount)
    {
      return;
    }

    Transform visualSettlement = transform.GetChild(IndiceVisualSettlement);
    if (visualSettlement == null)
    {
      return;
    }

    if (!escalaSettlementInicializada)
    {
      escalaSettlementOriginal = visualSettlement.localScale;
      escalaSettlementInicializada = true;
    }

    float multiplicador = destacar ? MultiplicadorEscalaSettlement : 1f;
    visualSettlement.localScale = escalaSettlementOriginal * multiplicador;
  }

  public void ActivarNodoVisual(int num, bool esAtajo, bool estabaRevelado, bool permitirNodoMisterioso = true)
  {
    DesactivarGraficosNodo();

    if (!revelandoPorExpedicionTutorial)
    {
      misterioForzadoTutorial = false;
    }
    esMisterioso = false;

    bool esTutorial = UsaConfiguracionTutorial();

    int chancesMisterioso = 15;
    if (CampaignManager.Instance.intTipoClima == 5) chancesMisterioso += 10; // Niebla
    if (CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) > 0)
      chancesMisterioso -= CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) * 5;

    if (esTutorial)
    {
      chancesMisterioso = 0;
      esAtajo = false;
      num = tipoNodo; // en tutorial no variamos el visual
    }

    if (posXNodo == 10 || EsNodoInicialSinMisterio()) chancesMisterioso = 0;
    if (estabaRevelado) chancesMisterioso = 0;
    if (nodoRitual) chancesMisterioso = 0;
    if (nodoIncendiado) chancesMisterioso = 0;

    if (permitirNodoMisterioso && UnityEngine.Random.Range(0, 100) < chancesMisterioso && tipoNodo != 16)
    {
      num = 12; // misterioso
      esMisterioso = true;
    }
    if (esAtajo) num = 13; // salida atajo

    if (EsNodoFinalZona())
    {
      num = tipoNodo > 0 ? tipoNodo : 10;
      esAtajo = false;
      esMisterioso = false;
      revelado = true;
    }

    numVisualActual = num;
    if (!ActivarVisualPorCodigo(numVisualActual))
      numVisualActual = -1;

    if (CampaignManager.Instance.scMapaManager.nodoActual != null && transform.childCount > 13)
    {
      int nodoenXactual = CampaignManager.Instance.scMapaManager.nodoActual.posXNodo;
      if (nodoenXactual >= posXNodo) { return; } //No activa VFx de revelado en nodos de la misma altura en X

      if (!CampaignManager.Instance.scMapaManager.nodoActual.DestinosPosibles.Contains(this) || esAtajo)
      {
        if (tipoNodo != 16) //no vfx en misión de salvamento
        { transform.GetChild(13).gameObject.SetActive(true); }
      } // vfx de revelado (no inmediatos)
    }

  }

  public void ActivarVfxDescubrimiento()
  {
    if (!gameObject.activeInHierarchy || tipoNodo == 16 || transform.childCount <= 13)
    {
      return;
    }

    transform.GetChild(13).gameObject.SetActive(true);
  }

  public string descripcion;

  void OnEnable()
  {
    if (!Application.isPlaying)
    {
      return;
    }

    // Sincronizar VFX persistentes con estado lógico al reactivar (antes de cualquier early-return)
    //SincronizarVFXPersistentes();
    SincronizarVFXPersistentes();
    Invoke(nameof(SincronizarVFXPersistentes), 0.1f);

    if (tipoNodo == 15 && nodoRitual && numVisualActual == 16)
    {
      numVisualActual = 15;
    }

    if (!visiblePorVision)
    {
      DesactivarTodosGraficosNodo();
      SincronizarLineaDecorativaNodoFinal();
      return;
    }

    if (!revelado)
    {
      DesactivarTodosGraficosNodo();
      ActivarVisualBaseNoRevelado();
      SincronizarLineaDecorativaNodoFinal();
      return;
    }

    int codigoAAplicar = numVisualActual > 0 ? numVisualActual : tipoNodo;
    if (codigoAAplicar <= 0)
    {
      ActivarVisualBaseNoRevelado();
      return;
    }
    DesactivarGraficosNodo();

    bool visualActivado = ActivarVisualPorCodigo(codigoAAplicar);
    if (visualActivado)
    {
      numVisualActual = codigoAAplicar;
    }
    else if (codigoAAplicar != tipoNodo && tipoNodo > 0)
    {
      visualActivado = ActivarVisualPorCodigo(tipoNodo);
      if (visualActivado)
        numVisualActual = tipoNodo;
    }

    if (!visualActivado)
    {
      ActivarVisualBaseNoRevelado();
      SincronizarLineaDecorativaNodoFinal();
      return;
    }

    SincronizarLineaDecorativaNodoFinal();

    if (CampaignManager.Instance != null &&
        CampaignManager.Instance.scMapaManager != null &&
        CampaignManager.Instance.scMapaManager.nodoActual != null &&
        transform.childCount > 13)
    {
      bool esAtajoActivo = numVisualActual == 13;
      int nodoenXactual = CampaignManager.Instance.scMapaManager.nodoActual.posXNodo;
      if (nodoenXactual == posXNodo) { return; } //No activa VFx de revelado en nodos de la misma altura en X
      if (!CampaignManager.Instance.scMapaManager.nodoActual.DestinosPosibles.Contains(this) || esAtajoActivo)
        transform.GetChild(13).gameObject.SetActive(true);
    }
  }

  void OnDisable()
  {
    if (!Application.isPlaying)
    {
      return;
    }

    CancelarTodosLosFadesVision();
    RestaurarPreviewHoverCaminosPosibles();
    DesactivarPulsoMovimientoNodo();
  }

  void OnMouseEnter()
  {
    if (DebeIgnorarEventosOnMousePorRaycastManual())
    {
      return;
    }

    ProcesarMouseEnterDesdeRaycast();
  }

  public void ProcesarMouseEnterDesdeRaycast()
  {
    if (!visiblePorVision) return;
    if (EventSystem.current.IsPointerOverGameObject()) return;
    cursorSobreNodo = true;
    AplicarPreviewHoverCaminosPosibles();

    if (!revelado)
    {
      descripcion = TRADU.i.Traducir("Nodo Desconocido.");
      Vector3 posicionTooltip = Input.mousePosition;
      TooltipNodos.Instance.ShowTooltip(descripcion, posicionTooltip, this);
      return;
    }

    switch (tipoNodo)
    {
      case 1: descripcion = TRADU.i.Traducir("Combate directo."); break;
      case 2: descripcion = TRADU.i.Traducir("Evento aleatorio."); break;
      case 3: descripcion = TRADU.i.Traducir("Claro tranquilo."); break;
      case 4: descripcion = TRADU.i.Traducir("Asentamiento."); break;
      case 5: descripcion = TRADU.i.Traducir("Recolección de Recursos."); break;
      case 6: descripcion = TRADU.i.Traducir("Puesto de Comercio."); break;
      case 7: descripcion = TRADU.i.Traducir("Adquisición de Personajes."); break;
      case 8: descripcion = TRADU.i.Traducir("Combate directo contra enemigos de élite."); break;
      case 10: descripcion = TRADU.i.Traducir("Batalla final de la Zona actual."); break;
      case 11: descripcion = TRADU.i.Traducir("<b>(!)</b> Zona Expuesta, la caravana será emboscada."); break;
      case 15: descripcion = TRADU.i.Traducir("Batalla Kale'Tav"); break;
      case 16: descripcion = TRADU.i.Traducir("Ubicación de la Misión de Salvamento"); break;

      default: descripcion = TRADU.i.Traducir("Nodo Desconocido."); break;
    }
    if (esMisterioso)
    {
      descripcion = CampaignManager.Instance != null && CampaignManager.Instance.PuedeEnviarExploradoresATooltip(this)
        ? ObtenerTextoTooltipNodoMisteriosoConExploradores()
        : TRADU.i.Traducir("Nodo Misterioso, no se ha logrado revelar.");
    }
    if (transform.GetChild(11).gameObject.activeInHierarchy) descripcion = TRADU.i.Traducir("Salida del atajo subterraneo, no sabemos que hay del otro lado.");
    if (transform.GetChild(12).gameObject.activeInHierarchy) descripcion = TRADU.i.Traducir("Santuario de Purificadores.");
    if (TieneFaccionScoutRevelada())
      descripcion += "\n--" + ObtenerFaccionScoutReveladaNombre() + "--";

    Vector3 pos = Input.mousePosition;
    TooltipNodos.Instance.ShowTooltip(descripcion, pos, this);
  }

  string ObtenerTextoTooltipNodoMisteriosoConExploradores()
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
      case TRADU.IdiomaIngles:
        return "Mysterious Node: right mouse button to send 5 Scouts";
      case TRADU.IdiomaPortugues:
        return "Nó Misterioso: botão direito do mouse para enviar 5 Exploradores";
      default:
        return "Nodo Misterioso: boton derecho del mouse para enviar 5 Exploradores";
    }
  }

  void OnMouseExit()
  {
    if (DebeIgnorarEventosOnMousePorRaycastManual())
    {
      return;
    }

    ProcesarMouseExitDesdeRaycast();
  }

  public void ProcesarMouseExitDesdeRaycast()
  {
    cursorSobreNodo = false;
    RestaurarPreviewHoverCaminosPosibles();
    TooltipNodos.Instance.HideTooltip();
  }

  void AplicarPreviewHoverCaminosPosibles()
  {
    if (!gameObject.activeInHierarchy ||
        CampaignManager.Instance == null ||
        CampaignManager.Instance.scMapaManager == null ||
        CampaignManager.Instance.scMapaManager.nodoActual == null)
    {
      return;
    }

    CaminoConexion conexion = CampaignManager.Instance.scMapaManager.nodoActual.ObtenerConexionHacia(this);
    if (conexion == null || conexion.estadoVisual != EstadoVisualCamino.Disponible)
    {
      return;
    }

    RestaurarPreviewHoverCaminosPosibles();
    conexionHoverActiva = conexion;
    conexion.hoverActivo = true;
    AplicarEstadoVisualCamino(conexion, conexion.estadoVisual);
    MostrarCaminoSubterraneoHoverTemporal(conexion);
  }

  void RestaurarPreviewHoverCaminosPosibles()
  {
    if (conexionHoverActiva == null)
    {
      return;
    }

    conexionHoverActiva.hoverActivo = false;
    AplicarEstadoVisualCamino(conexionHoverActiva, conexionHoverActiva.estadoVisual);
    RestaurarCaminoSubterraneoHoverTemporal();
    conexionHoverActiva = null;
  }

  void MostrarCaminoSubterraneoHoverTemporal(CaminoConexion conexion)
  {
    if (conexion == null || !conexion.EsAtajoSubterraneo || conexion.linea == null)
    {
      return;
    }

    lineaHoverTemporalActiva = conexion.linea;
    lineaHoverTemporalEstabaActiva = lineaHoverTemporalActiva.gameObject.activeSelf;
    lineaHoverTemporalMeshEstabaVisible = EstaMeshCaminoVisible(lineaHoverTemporalActiva);

    lineaHoverTemporalActiva.gameObject.SetActive(true);
    SetMeshCaminoVisible(lineaHoverTemporalActiva, true);
    RestaurarAlphaTransform(lineaHoverTemporalActiva);
    OcultarDecoracionSobreCamino(lineaHoverTemporalActiva);
  }

  void RestaurarCaminoSubterraneoHoverTemporal()
  {
    if (lineaHoverTemporalActiva == null)
    {
      return;
    }

    RestaurarAlphaTransform(lineaHoverTemporalActiva);
    SetMeshCaminoVisible(lineaHoverTemporalActiva, lineaHoverTemporalMeshEstabaVisible);
    lineaHoverTemporalActiva.gameObject.SetActive(lineaHoverTemporalEstabaActiva);
    lineaHoverTemporalActiva = null;
    lineaHoverTemporalEstabaActiva = false;
    lineaHoverTemporalMeshEstabaVisible = false;
  }

  bool EstaMeshCaminoVisible(Transform linea)
  {
    if (linea == null)
    {
      return false;
    }

    CaminoMesh caminoMesh = linea.GetComponent<CaminoMesh>();
    MeshRenderer meshRenderer = caminoMesh != null ? caminoMesh.GetMeshRenderer() : linea.GetComponent<MeshRenderer>();
    return meshRenderer != null && meshRenderer.enabled;
  }

  void SetMeshCaminoVisible(Transform linea, bool visible)
  {
    if (linea == null)
    {
      return;
    }

    CaminoMesh caminoMesh = linea.GetComponent<CaminoMesh>();
    if (caminoMesh != null)
    {
      caminoMesh.SetVisible(visible);
      return;
    }

    MeshRenderer meshRenderer = linea.GetComponent<MeshRenderer>();
    if (meshRenderer != null)
    {
      meshRenderer.enabled = visible;
    }
  }


  public void ActivarIncendio()
  {
    if (UsaConfiguracionTutorial() || !PuedeTenerIncendioPersistente())
    {
      nodoIncendiado = false;
      if (transform.childCount > 14)
        transform.GetChild(14).gameObject.SetActive(false);
      return;
    }

    print("ActivarIncendio called");
    nodoIncendiado = true;
    if (transform.childCount > 14)
      transform.GetChild(14).gameObject.SetActive(true);

  }

  public void DesactivarIncendio()
  {
    print("DesactivarIncendio called");
    nodoIncendiado = false;
    if (transform.childCount > 14)
      transform.GetChild(14).gameObject.SetActive(false);
  }

  public void ActivarRitual()
  {
    if (!PuedeTenerRitualPersistente())
    {
      nodoRitual = false;
      if (transform.childCount > 15)
        transform.GetChild(15).gameObject.SetActive(false);
      return;
    }

    print("ActivarRitual called");
    nodoRitual = true;
    if (transform.childCount > 15)
      transform.GetChild(15).gameObject.SetActive(true);
  }

  public void DesactivarRitual()
  {
    print("DesactivarRitual called");
    nodoRitual = false;
    if (transform.childCount > 15)
      transform.GetChild(15).gameObject.SetActive(false);
    RestaurarTipoOriginalTrasRitual();
  }

  void RestaurarTipoOriginalTrasRitual()
  {
    if (tipoNodo != 15 || tipoNodoOriginalRitual <= 0)
    {
      return;
    }

    tipoNodo = tipoNodoOriginalRitual;
    tipoNodoOriginalRitual = 0;
    AplicarVisualGuardado(tipoNodo, false);
  }

  public void SincronizarVFXPersistentes()
  {
    // En tutorial nunca deberáan quedar incendios/rituales activos
    LimpiarEstadosPersistentesNoValidos();

    if (!visiblePorVision || !revelado)
    {
      if (transform.childCount > 14)
        transform.GetChild(14).gameObject.SetActive(false);
      if (transform.childCount > 15)
        transform.GetChild(15).gameObject.SetActive(false);

      for (int i = 0; i < transform.childCount; i++)
      {
        var child = transform.GetChild(i);
        string name = child.name.ToLowerInvariant();
        if (name.Contains("nodoincendiado") || name.Contains("nodoritual"))
        {
          child.gameObject.SetActive(false);
        }
      }
      return;
    }

    // Fallback por índice (prefab original)
    if (transform.childCount > 14)
      transform.GetChild(14).gameObject.SetActive(nodoIncendiado);
    if (transform.childCount > 15)
      transform.GetChild(15).gameObject.SetActive(nodoRitual);

    // Refuerzo por nombre (por si cambia el orden de hijos)
    for (int i = 0; i < transform.childCount; i++)
    {
      var child = transform.GetChild(i);
      string name = child.name.ToLowerInvariant();
      if (name.Contains("nodoincendiado"))
      {
        child.gameObject.SetActive(nodoIncendiado);
      }
      else if (name.Contains("nodoritual"))
      {
        child.gameObject.SetActive(nodoRitual);
      }
    }
  }

  void ActivarVisualBaseNoRevelado()
  {
    AplicarEstiloVisualSettlement(false);

    if (transform.childCount == 0)
    {
      return;
    }

    Transform visualBase = transform.GetChild(0);
    if (visualBase == null)
    {
      return;
    }

    visualBase.gameObject.SetActive(true);

    if (visualBase.childCount == 0)
    {
      return;
    }

    bool mostrarSubVisual = CampaignManager.Instance == null
      || CampaignManager.Instance.scAtributosZona == null
      || CampaignManager.Instance.scAtributosZona.ID != 3;

    visualBase.GetChild(0).gameObject.SetActive(mostrarSubVisual);
  }

  bool EstaPermitidoEnZona(Nodo nodo, int zonaId)
  {
    if (nodo == null) return false;
    if (zonaId <= 0) return true;
    return nodo.ProhibidoEnZona == null || !nodo.ProhibidoEnZona.Contains(zonaId);
  }

  Nodo ObtenerNodoPermitido(int x, int y, int zonaId)
  {
    if (scContenedorNodos2 == null) return null;
    Nodo destino = scContenedorNodos2.ObtenerNodoSegunXY(x, y);
    if (!EstaPermitidoEnZona(destino, zonaId)) return null;
    return destino;
  }

  bool IntentarConectar(int x, int y, int zonaId, bool esPorAbajo = false)
  {
    Nodo destino = ObtenerNodoPermitido(x, y, zonaId);
    if (destino == null) return false;
    if (!esPorAbajo && CruzariaConexionExistente(destino)) return false;
    ConectarConNodo(destino, esPorAbajo);
    return true;
  }

  bool CruzariaConexionExistente(Nodo destino)
  {
    if (destino == null || scContenedorNodos2 == null || destino.posXNodo != posXNodo + 1)
      return false;

    foreach (Nodo otroOrigen in scContenedorNodos2.listTodosNodos)
    {
      if (otroOrigen == null || otroOrigen == this || otroOrigen.posXNodo != posXNodo)
        continue;

      foreach (Nodo otroDestino in otroOrigen.DestinosPosibles)
      {
        if (otroDestino != null &&
            otroDestino.posXNodo == destino.posXNodo &&
            (posYNodo - otroOrigen.posYNodo) * (destino.posYNodo - otroDestino.posYNodo) < 0)
          return true;
      }
    }

    return false;
  }

  void ConectarFallbackSiguienteColumna(int nextX, int zonaId)
  {
    if (scContenedorNodos2 == null) return;

    Nodo mejor = null;
    int mejorDistY = int.MaxValue;

    foreach (Nodo candidato in scContenedorNodos2.listTodosNodos)
    {
      if (candidato == null) continue;
      if (candidato.posXNodo != nextX) continue;
      if (!EstaPermitidoEnZona(candidato, zonaId)) continue;
      if (CruzariaConexionExistente(candidato)) continue;

      int distY = Mathf.Abs(candidato.posYNodo - posYNodo);
      if (distY < mejorDistY)
      {
        mejorDistY = distY;
        mejor = candidato;
        if (mejorDistY == 0) break;
      }
    }

    if (mejor != null)
      ConectarConNodo(mejor);
  }

  public void EsconderSiNedukazal()
  {
    if (CampaignManager.Instance.scAtributosZona.ID == 3)
    {
      transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }
    else
    { transform.GetChild(0).GetChild(0).gameObject.SetActive(true); }
  }
  
  [Header("Convoy")]
public float rotSpeed = 10f;            // suavizado de rotación
public float lookAheadDist = 0.5f;    // "mira" hacia adelante para orientar
float gapDist = 0.66f;              // distancia entre vehículos
public void MoverConvoyIntroEnLinea(LineRenderer lr, System.Action alFinalizar, Transform rotacionLiderOverride = null, float progresoInicial = 0f)
{
    AsegurarMapaManagerRuntime();
    if (lr == null || scMapaManager == null || scMapaManager.goCaravana == null)
    {
      alFinalizar?.Invoke();
      return;
    }

    StartCoroutine(MoverConvoyIntroSuave(lr, alFinalizar, rotacionLiderOverride, progresoInicial));
}

public void PrepararConvoyIntroEnLinea(LineRenderer lr, Transform rotacionLiderOverride = null, float progresoInicial = 0f)
{
    AsegurarMapaManagerRuntime();
    if (lr == null || lr.positionCount < 2 || scMapaManager == null || scMapaManager.goCaravana == null)
    {
      return;
    }

    var convoy = new List<(GameObject go, Transform rot)>();
    ConstruirConvoyIntro(convoy, rotacionLiderOverride);
    if (convoy.Count == 0)
    {
      return;
    }

    Vector3[] pts = ObtenerPuntosLineaIntro(lr);
    float[] segLen;
    float[] cumLen;
    CalcularLongitudesLineaIntro(pts, out segLen, out cumLen);
    Vector3[] offsetsIniciales = CalcularOffsetsFormacionIntro(convoy, pts);
    MapDecorator mapDecorator = ObtenerDecoradorMapa();
    float totalLen = cumLen[cumLen.Length - 1];
    float distanciaInicial = totalLen * Mathf.Clamp01(progresoInicial);
    AplicarFormacionIntro(convoy, PointAtDistance(pts, segLen, cumLen, distanciaInicial), PointAtDistance(pts, segLen, cumLen, Mathf.Min(totalLen, distanciaInicial + LookAheadIntroCampania)), offsetsIniciales, mapDecorator, 1f);
}

private IEnumerator MoverConvoyIntroSuave(LineRenderer lr, System.Action alFinalizar, Transform rotacionLiderOverride, float progresoInicial)
{
    var convoy = new List<(GameObject go, Transform rot)>();
    ConstruirConvoyIntro(convoy, rotacionLiderOverride);

    if (lr == null || lr.positionCount < 2 || convoy.Count == 0)
    {
      alFinalizar?.Invoke();
      yield break;
    }

    int n = lr.positionCount;
    Vector3[] pts = ObtenerPuntosLineaIntro(lr);
    float[] segLen;
    float[] cumLen;
    CalcularLongitudesLineaIntro(pts, out segLen, out cumLen);

    float totalLen = cumLen[n - 1];
    if (totalLen <= 0.0001f)
    {
      alFinalizar?.Invoke();
      yield break;
    }

    Vector3[] offsetsIniciales = CalcularOffsetsFormacionIntro(convoy, pts);
    SetAnimacionCaravanaIntro(true);
    SetWalkingFollowersIntro(true);
    if (CampaignManager.Instance != null)
    {
      CampaignManager.Instance.IniciarSonidoMovimientoCaravanaIntro();
    }

    MapDecorator mapDecorator = ObtenerDecoradorMapa();
    float distanciaInicial = totalLen * Mathf.Clamp01(progresoInicial);
    float longitudRestante = Mathf.Max(0.01f, totalLen - distanciaInicial);
    float duracion = Mathf.Max(1.2f, longitudRestante / Mathf.Max(0.1f, VelocidadIntroCampania));
    float tiempo = 0f;

    while (tiempo < duracion)
    {
      float dt = Mathf.Min(Time.deltaTime, DeltaMaximoIntroCampania);
      tiempo += dt;
      float t = Mathf.Clamp01(tiempo / duracion);
      float progreso = t * t * (3f - 2f * t);
      float leaderS = Mathf.Lerp(distanciaInicial, totalLen, progreso);
      Vector3 posicionLider = PointAtDistance(pts, segLen, cumLen, leaderS);
      Vector3 posicionLiderFutura = PointAtDistance(pts, segLen, cumLen, Mathf.Min(totalLen, leaderS + LookAheadIntroCampania));
      AplicarFormacionIntro(convoy, posicionLider, posicionLiderFutura, offsetsIniciales, mapDecorator, dt);

      yield return null;
    }

    AplicarFormacionIntro(convoy, pts[n - 1], pts[n - 1], offsetsIniciales, mapDecorator, 1f);

    SetAnimacionCaravanaIntro(false);
    SetWalkingFollowersIntro(false);
    if (CampaignManager.Instance != null)
    {
      CampaignManager.Instance.DetenerSonidoMovimientoCaravanaIntro();
    }
    alFinalizar?.Invoke();
}

void ConstruirConvoyIntro(List<(GameObject go, Transform rot)> convoy, Transform rotacionLiderOverride)
{
    if (convoy == null || scMapaManager == null)
    {
      return;
    }

    void AddIf(GameObject go, bool esLider)
    {
        if (go == null) return;
        int idx = esLider ? 4 : 0;
        Transform rotT = (esLider && rotacionLiderOverride != null) ? rotacionLiderOverride : (go.transform.childCount > idx) ? go.transform.GetChild(idx) : null;
        convoy.Add((go, rotT));
    }

    AddIf(scMapaManager.goCaravana, true);
    AddIf(scMapaManager.goCaravanafollower1, false);
    AddIf(scMapaManager.goCaravanafollower2, false);
    AddIf(scMapaManager.goCaravanafollower3, false);
    AddIf(scMapaManager.goCaravanafollower4, false);
    AddIf(scMapaManager.goCaravanafollower5, false);
    AddIf(scMapaManager.goCaravanafollower6, false);
}

static Vector3[] ObtenerPuntosLineaIntro(LineRenderer lr)
{
    int n = lr.positionCount;
    Vector3[] pts = new Vector3[n];
    for (int i = 0; i < n; i++)
    {
      Vector3 p = lr.GetPosition(i);
      pts[i] = lr.useWorldSpace ? p : lr.transform.TransformPoint(p);
    }

    return pts;
}

static void CalcularLongitudesLineaIntro(Vector3[] pts, out float[] segLen, out float[] cumLen)
{
    int n = pts.Length;
    segLen = new float[n - 1];
    cumLen = new float[n];
    cumLen[0] = 0f;
    for (int i = 0; i < n - 1; i++)
    {
      segLen[i] = Vector3.Distance(pts[i], pts[i + 1]);
      cumLen[i + 1] = cumLen[i] + segLen[i];
    }
}

static Vector3[] CalcularOffsetsFormacionIntro(List<(GameObject go, Transform rot)> convoy, Vector3[] pts)
{
    Vector3 direccionIntro = ObtenerDireccionIntroCampania(pts);
    Vector3 lateralIntro = Vector3.Cross(Vector3.up, direccionIntro).normalized;
    Vector3 posicionLiderInicial = convoy[0].go.transform.position;
    Vector3[] offsetsIniciales = new Vector3[convoy.Count];

    for (int i = 0; i < convoy.Count; i++)
    {
      offsetsIniciales[i] = CalcularOffsetFormacionIntro(convoy[i].go.transform.position - posicionLiderInicial, i, direccionIntro, lateralIntro);
    }

    return offsetsIniciales;
}

void AplicarFormacionIntro(List<(GameObject go, Transform rot)> convoy, Vector3 posicionLider, Vector3 posicionLiderFutura, Vector3[] offsetsIniciales, MapDecorator mapDecorator, float dt)
{
    for (int i = 0; i < convoy.Count; i++)
    {
      Vector3 posicion = AplicarAlturaConvoyIntro(posicionLider + offsetsIniciales[i], mapDecorator);
      convoy[i].go.transform.position = posicion;

      if (convoy[i].rot == null)
      {
        continue;
      }

      Vector3 futuro = AplicarAlturaConvoyIntro(posicionLiderFutura + offsetsIniciales[i], mapDecorator);
      Quaternion target = CalcularRotacionConvoyPorRelieve(posicion, futuro, false);
      if (target == Quaternion.identity)
      {
        continue;
      }

      float k = 1f - Mathf.Exp(-RotacionIntroCampania * dt);
      convoy[i].rot.rotation = Quaternion.Slerp(convoy[i].rot.rotation, target, k);
    }
}

static Vector3 ObtenerDireccionIntroCampania(Vector3[] pts)
{
    for (int i = 0; i < pts.Length - 1; i++)
    {
      Vector3 dir = pts[i + 1] - pts[i];
      dir.y = 0f;
      if (dir.sqrMagnitude > 0.000001f)
      {
        return dir.normalized;
      }
    }

    return Vector3.right;
}

static Vector3 CalcularOffsetFormacionIntro(Vector3 offsetOriginal, int indice, Vector3 direccionIntro, Vector3 lateralIntro)
{
    if (indice == 0)
    {
      return Vector3.zero;
    }

    offsetOriginal.y = 0f;
    float lateral = Mathf.Clamp(Vector3.Dot(offsetOriginal, lateralIntro), -LateralMaximoIntroCampania, LateralMaximoIntroCampania);
    return -direccionIntro * (SeparacionIntroCampania * indice) + lateralIntro * lateral;
}

Vector3 AplicarAlturaConvoyIntro(Vector3 posicion, MapDecorator mapDecorator)
{
    if (mapDecorator != null && mapDecorator.TrySampleSurface(posicion, out var surfacePoint, out _, OffsetConvoyIntroSobreRelieve))
    {
      posicion.y = surfacePoint.y;
    }

    return posicion;
}

void SetAnimacionCaravanaIntro(bool activa)
{
    if (CampaignManager.Instance == null || CampaignManager.Instance.animCaravana == null)
    {
      return;
    }

    CampaignManager.Instance.animCaravana.SetBool("IsWalking", activa);
    CampaignManager.Instance.animCaravana.speed = 1f;
}

void SetWalkingFollowersIntro(bool walking)
{
    if (scMapaManager == null)
    {
      return;
    }

    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower1, walking);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower2, walking);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower3, walking);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower4, walking);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower5, walking);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower6, walking);
}

private IEnumerator MoverConvoyEnLinea(LineRenderer lr, bool viajeSubterraneo = false, bool ejecutarLlegadaCaravana = true, System.Action alFinalizar = null, float velocidadOverride = -1f, Transform rotacionLiderOverride = null)
{
    AsegurarMapaManagerRuntime();
    // Ajustes de suavizado (si querés tunear, subilos a fields públicos)
    const float tramoSuavizadoExtremos = 0.18f;
    float easeInTime = 0.28f;       // segundos para acelerar al inicio
    float easeOutDist = 0.6f;       // metros antes del final para frenar líder
    float easeOutTailDist = 0.08f;   // "error" de cola para frenar al final
    float minSpeedFactor = 0.10f;   // piso para que no se quede "cerquita"
    float snapEps = 0.03f;          // snap final del líder (en metros aprox)

    if (lr == null)
    {
      if (!ejecutarLlegadaCaravana) alFinalizar?.Invoke();
      yield break;
    }

    // Convoy ordenado
    var convoy = new List<(GameObject go, Transform rot)>();
    void AddIf(GameObject go, bool esLider)
    {
        if (go == null) return;
        int idx = esLider ? 4 : 0; // tu setup
        Transform rotT = (esLider && rotacionLiderOverride != null) ? rotacionLiderOverride : (go.transform.childCount > idx) ? go.transform.GetChild(idx) : null;
        convoy.Add((go, rotT));
    }

    AddIf(scMapaManager.goCaravana, true);
    AddIf(scMapaManager.goCaravanafollower1, false);
    AddIf(scMapaManager.goCaravanafollower2, false);
    AddIf(scMapaManager.goCaravanafollower3, false);
    AddIf(scMapaManager.goCaravanafollower4, false);
    AddIf(scMapaManager.goCaravanafollower5, false);
    AddIf(scMapaManager.goCaravanafollower6, false);

    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower1, true);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower2, true);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower3, true);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower4, true);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower5, true);
    SetWalkingConvoyIfPresent(scMapaManager.goCaravanafollower6, true);

    int m = convoy.Count;
    float velocidadBaseMovimiento = velocidadOverride > 0f ? velocidadOverride : velocidadMovimiento;
    if (m == 0)
    {
      if (!ejecutarLlegadaCaravana) alFinalizar?.Invoke();
      yield break;
    }

    var undergroundRenderers = new List<Renderer>();
    var undergroundRendererInitialStates = new List<bool>();
    bool convoyOculto = false;
    GameObject markerGO = null;
    Vector3 markerBaseScale = new Vector3(0.24f, 0.24f, 0.24f);
    const float markerYOffset = 0.2f;
    const float markerPulseSpeed = 8f;
    const float markerPulseAmp = 0.1f;
    const float tramoEntradaSalidaVisible = 0.18f;
    const float profundidadSubterraneaY = 1.2f;
    const float pitchSubterraneoMax = 7f;
    const float overlayAlphaMax = 0.322f; // +15% más oscuro
    GameObject overlayGO = null;
    Image overlayImage = null;
    UndergroundAudioFxState undergroundAudioFx = null;

    // Puntos del camino en WORLD
    int n = lr.positionCount;
    if (n < 2)
    {
      if (!ejecutarLlegadaCaravana) alFinalizar?.Invoke();
      yield break;
    }

    Vector3[] pts = new Vector3[n];
    for (int i = 0; i < n; i++)
    {
        Vector3 p = lr.GetPosition(i);
        pts[i] = lr.useWorldSpace ? p : lr.transform.TransformPoint(p);
    }

    AjustarExtremosTrayectoConvoy(pts, convoy[0].go.transform.position, transform.position, tramoSuavizadoExtremos);

    // Longitudes acumuladas del tramo
    float[] segLen = new float[n - 1];
    float[] cumLen = new float[n];
    cumLen[0] = 0f;
    for (int i = 0; i < n - 1; i++)
    {
        float L = Vector3.Distance(pts[i], pts[i + 1]);
        segLen[i] = L;
        cumLen[i + 1] = cumLen[i] + L;
    }

    float totalLen = cumLen[n - 1];
    if (totalLen <= 0.0001f)
    {
      if (!ejecutarLlegadaCaravana) alFinalizar?.Invoke();
      yield break;
    }
    float duracionDesvanecidoSonido = Mathf.Lerp(0.08f, 0.2f, Mathf.InverseLerp(2.5f, 9f, totalLen));

    // Gap desde el Inspector (no lo pises con un local)
    float gap = Mathf.Max(0.0001f, this.gapDist);
    easeInTime = Mathf.Lerp(0.24f, 0.4f, Mathf.InverseLerp(2.5f, 9f, totalLen));
    easeOutDist = Mathf.Clamp(totalLen * 0.07f, 0.2f, 0.45f);
    easeOutTailDist = Mathf.Clamp(gap * 0.28f, 0.04f, 0.12f);
    minSpeedFactor = 0.18f;
    snapEps = 0.05f;

    if (viajeSubterraneo)
    {
      for (int i = 0; i < convoy.Count; i++)
      {
        RegistrarRenderersSubterraneos(convoy[i].rot, undergroundRenderers, undergroundRendererInitialStates);
      }
      markerGO = GetOrCreateUndergroundTravelMarker();
      overlayGO = CrearOverlayViajeSubterraneo(out overlayImage);
      undergroundAudioFx = PrepararAudioSubterraneo();
      ActualizarAudioSubterraneo(undergroundAudioFx, 0f);
    }


    // Trail del líder: seed con estado actual (cola -> ... -> líder) para cero teleports
    var trailPos = new List<Vector3>(256);
    var trailRot = new List<Quaternion>(256);
    var trailS   = new List<float>(256);

    for (int i = m - 1; i >= 0; i--)
    {
        Vector3 p = convoy[i].go.transform.position;
        Quaternion r = (convoy[i].rot != null) ? convoy[i].rot.rotation : Quaternion.identity;

        if (trailPos.Count == 0)
        {
            trailPos.Add(p);
            trailRot.Add(r);
            trailS.Add(0f);
        }
        else
        {
            float add = Vector3.Distance(trailPos[trailPos.Count - 1], p);
            trailPos.Add(p);
            trailRot.Add(r);
            trailS.Add(trailS[trailS.Count - 1] + add);
        }
    }

    // "odómetro" de cada follower sobre el TRAIL (esto es lo que permite que sigan moviéndose al final)
    float[] followerS = new float[m];
    for (int i = 0; i < m; i++)
        followerS[i] = ProjectDistanceOnTrail(trailPos, trailS, convoy[i].go.transform.position);

    // Arrancamos líder en su proyección al tramo nuevo
    float leaderS = ProjectDistanceOnPolyline(pts, cumLen, convoy[0].go.transform.position);

    const float minSampleDist = 0.02f;
    float maxTrailBack = gap * (m - 1) + 2.0f;

    float elapsed = 0f;
    bool leaderArrived = false;
    bool sonidoMovimientoDesvanecido = false;
    float speedFactorSmoothed = 0f;
    const float speedSmooth = 10f; // subí a 12-16 si aún hay tirán
    
    try
    {
      while (true)
      {
      float dt = Time.deltaTime;
      elapsed += dt;
      bool leaderArrivedPrevio = leaderArrived;

      // Factor de aceleración inicial
      float easeIn = (easeInTime <= 0.0001f) ? 1f : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / easeInTime));

      // Factor de frenado (líder) o drenado (cola)
      float easeOutFactor;

      if (!leaderArrived)
      {
        float remaining = totalLen - leaderS;

        // Snap para que no quede "cerquita"
        if (remaining <= snapEps)
        {
          leaderS = totalLen;
          leaderArrived = true;
          remaining = 0f;
        }

        // 1 lejos, 0 cerca del final
        easeOutFactor = (easeOutDist <= 0.0001f)
            ? 1f
            : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(remaining / easeOutDist));
      }
      else
      {
        // Drenado: si la cola todavía tiene error grande, seguimos a buena velocidad;
        // si el error es chico, frenamos suave.
        float headTrailS2 = trailS[trailS.Count - 1];
        float maxErr = 0f;

        for (int i = 1; i < m; i++)
        {
          float targetS = headTrailS2 - gap * i;
          if (targetS < trailS[0]) targetS = trailS[0];
          float err = Mathf.Abs(followerS[i] - targetS);
          if (err > maxErr) maxErr = err;
        }

        easeOutFactor = (easeOutTailDist <= 0.0001f)
            ? 1f
            : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(maxErr / easeOutTailDist));
      }

      float speedFactorTarget = Mathf.Max(minSpeedFactor, easeIn * easeOutFactor);
      float speedFloor = Mathf.Lerp(0f, minSpeedFactor, easeIn);
      speedFactorTarget = Mathf.Max(speedFloor, easeIn * easeOutFactor);

// suavizado exponencial independiente del FPS
float kSpeed = 1f - Mathf.Exp(-speedSmooth * dt);
speedFactorSmoothed = Mathf.Lerp(speedFactorSmoothed, speedFactorTarget, kSpeed);

float step = Mathf.Max(0f, velocidadBaseMovimiento) * dt * speedFactorSmoothed;


      // Mover líder hasta el final (luego se queda)
      if (!leaderArrived)
      {
        leaderS = Mathf.MoveTowards(leaderS, totalLen, step);
        if (totalLen - leaderS <= snapEps)
        {
          leaderS = totalLen;
          leaderArrived = true;
        }
      }

      if (ejecutarLlegadaCaravana && leaderArrived && !leaderArrivedPrevio && !sonidoMovimientoDesvanecido && CampaignManager.Instance != null)
      {
        CampaignManager.Instance.DesvanecerSonidoMovimientoCaravana(duracionDesvanecidoSonido);
        sonidoMovimientoDesvanecido = true;
      }

      Vector3 leaderPosCamino = PointAtDistance(pts, segLen, cumLen, leaderS);
      float offsetYSubterraneo = 0f;

      if (viajeSubterraneo)
      {
        float progreso = totalLen <= 0.0001f ? 1f : Mathf.Clamp01(leaderS / totalLen);
        bool ocultarAhora = progreso > tramoEntradaSalidaVisible && progreso < (1f - tramoEntradaSalidaVisible);

        if (ocultarAhora != convoyOculto)
        {
          SetRenderersVisible(undergroundRenderers, !ocultarAhora);
          convoyOculto = ocultarAhora;
        }

        if (markerGO != null)
        {
          markerGO.SetActive(ocultarAhora);
          if (ocultarAhora)
          {
            float pulse = 1f + markerPulseAmp * Mathf.Sin(elapsed * markerPulseSpeed);
            markerGO.transform.localScale = markerBaseScale * pulse;
            markerGO.transform.position = leaderPosCamino + Vector3.up * markerYOffset;
          }
        }

        float intensidadSubterranea = CalcularIntensidadSubterranea(progreso, tramoEntradaSalidaVisible);
        AplicarTinteSubterraneo(overlayImage, intensidadSubterranea, overlayAlphaMax);
        ActualizarAudioSubterraneo(undergroundAudioFx, intensidadSubterranea);
        offsetYSubterraneo = CalcularOffsetYSubterraneo(progreso, tramoEntradaSalidaVisible, profundidadSubterraneaY);
      }

      Vector3 leaderPos = leaderPosCamino;
      leaderPos.y += offsetYSubterraneo;
      convoy[0].go.transform.position = leaderPos;

      // Rotación del líder por tangente
      Quaternion leaderRot = Quaternion.identity;
      if (convoy[0].rot != null)
      {
        float sAtras = Mathf.Max(leaderS - lookAheadDist, 0f);
        float sF = Mathf.Min(leaderS + lookAheadDist, totalLen);
        Vector3 posAtras = PointAtDistance(pts, segLen, cumLen, sAtras);
        Vector3 posF = PointAtDistance(pts, segLen, cumLen, sF);
        float pitchSubterraneo = 0f;

        if (viajeSubterraneo)
        {
          float progresoActual = totalLen <= 0.0001f ? 1f : Mathf.Clamp01(leaderS / totalLen);
          float progresoAtras = totalLen <= 0.0001f ? 0f : Mathf.Clamp01(sAtras / totalLen);
          float progresoFuturo = totalLen <= 0.0001f ? 1f : Mathf.Clamp01(sF / totalLen);

          posAtras.y += CalcularOffsetYSubterraneo(progresoAtras, tramoEntradaSalidaVisible, profundidadSubterraneaY);
          posF.y += CalcularOffsetYSubterraneo(progresoFuturo, tramoEntradaSalidaVisible, profundidadSubterraneaY);
          pitchSubterraneo = CalcularPitchSubterraneo(progresoActual, tramoEntradaSalidaVisible, pitchSubterraneoMax);
        }

        Vector3 origenRotacion = leaderPos;
        Vector3 destinoRotacion = posF;

        if ((destinoRotacion - origenRotacion).sqrMagnitude <= 0.000001f)
        {
          origenRotacion = posAtras;
          destinoRotacion = leaderPos;
        }

        Quaternion target = CalcularRotacionConvoyPorRelieve(origenRotacion, destinoRotacion, viajeSubterraneo);
        if (viajeSubterraneo && target != Quaternion.identity)
        {
          target *= Quaternion.Euler(pitchSubterraneo, 0f, 0f);
        }

        if (target != Quaternion.identity)
        {
          float k = 1f - Mathf.Exp(-rotSpeed * dt);
          convoy[0].rot.rotation = Quaternion.Slerp(convoy[0].rot.rotation, target, k);
        }
        leaderRot = convoy[0].rot.rotation;
      }

      // Guardar sample del líder en el trail (siempre consistente)
Vector3 lastPos = trailPos[trailPos.Count - 1];
float moved = Vector3.Distance(lastPos, leaderPos);

// si el líder realmente se movió, agregamos un sample nuevo
if (moved > 0.00001f)
{
    trailPos.Add(leaderPos);
    trailRot.Add(leaderRot);
    trailS.Add(trailS[trailS.Count - 1] + moved);
}
else
{
    // si no se movió, actualizamos rot/pos por prolijidad
    trailPos[trailPos.Count - 1] = leaderPos;
    trailRot[trailRot.Count - 1] = leaderRot;
}


      float headTrailS = trailS[trailS.Count - 1];

      // Followers: avanzan por el trail con su propio odómetro (esto permite "terminar" después)
      bool allFollowersAtTarget = true;

      for (int i = 1; i < m; i++)
      {
        float targetS = headTrailS - gap * i;
        if (targetS < trailS[0]) targetS = trailS[0];

        followerS[i] = Mathf.MoveTowards(followerS[i], targetS, step);

        SampleTrail(trailPos, trailRot, trailS, followerS[i], out var p, out var r);
        p.y += offsetYSubterraneo;
        convoy[i].go.transform.position = p;

        if (convoy[i].rot != null)
        {
          float k = 1f - Mathf.Exp(-rotSpeed * dt);
          convoy[i].rot.rotation = Quaternion.Slerp(convoy[i].rot.rotation, r, k);
        }

        if (Mathf.Abs(followerS[i] - targetS) > 0.01f)
          allFollowersAtTarget = false;
      }

      // Recorte del trail
      while (trailS.Count > 2 && (headTrailS - trailS[0]) > maxTrailBack)
      {
        trailS.RemoveAt(0);
        trailPos.RemoveAt(0);
        trailRot.RemoveAt(0);

        // Clamp por si justo recortaste "debajo" de algún follower
        for (int i = 1; i < m; i++)
          if (followerS[i] < trailS[0]) followerS[i] = trailS[0];
      }

      // Salida: líder llegó y cola drenó
      if (leaderArrived && allFollowersAtTarget)
        break;

      yield return null;
      }

      // Snap final líder al último punto exacto del LR
      convoy[0].go.transform.position = pts[n - 1];
    }
    finally
    {
      if (viajeSubterraneo)
      {
        RestaurarRenderers(undergroundRenderers, undergroundRendererInitialStates);
        if (markerGO != null) markerGO.SetActive(false);
        AplicarTinteSubterraneo(overlayImage, 0f, overlayAlphaMax);
        RestaurarAudioSubterraneo(undergroundAudioFx);
        if (overlayGO != null) Destroy(overlayGO);
      }
    }

    if (ejecutarLlegadaCaravana)
    {
      LlegoCaravana();
    }
    else
    {
      alFinalizar?.Invoke();
    }
}

void AsegurarMapaManagerRuntime()
{
    if (scMapaManager != null)
    {
      return;
    }

    if (CampaignManager.Instance != null)
    {
      scMapaManager = CampaignManager.Instance.scMapaManager;
    }
}

// Helper: proyecta posición al TRAIL (polilínea) y devuelve "s"
private static void SetWalkingConvoyIfPresent(GameObject go, bool walking)
{
    if (go == null || go.transform.childCount == 0)
    {
      return;
    }

    Animator animator = go.transform.GetChild(0).GetComponent<Animator>();
    if (animator != null)
    {
      animator.SetBool("IsWalking", walking);
    }
}

private static void AjustarExtremosTrayectoConvoy(Vector3[] pts, Vector3 origenReal, Vector3 destinoReal, float tramoSuavizado)
{
    if (pts == null || pts.Length == 0)
    {
        return;
    }

    if (pts.Length == 1)
    {
        pts[0] = destinoReal;
        return;
    }

    Vector3 offsetInicio = origenReal - pts[0];
    Vector3 offsetFin = destinoReal - pts[pts.Length - 1];

    if (offsetInicio.sqrMagnitude <= 0.000001f && offsetFin.sqrMagnitude <= 0.000001f)
    {
        return;
    }

    float blend = Mathf.Clamp01(tramoSuavizado);
    float blendSeguro = Mathf.Max(0.0001f, blend);
    int ultimo = pts.Length - 1;

    for (int i = 0; i <= ultimo; i++)
    {
        float t = i / (float)ultimo;
        float pesoInicio = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / blendSeguro));
        float pesoFin = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - t) / blendSeguro));
        pts[i] += offsetInicio * pesoInicio + offsetFin * pesoFin;
    }

    pts[0] = origenReal;
    pts[ultimo] = destinoReal;
}

private static float ProjectDistanceOnTrail(List<Vector3> trailPos, List<float> trailS, Vector3 worldPos)
{
    Vector3 p = worldPos; p.y = 0f;
    float bestS = trailS[0];
    float bestD2 = float.PositiveInfinity;

    for (int i = 0; i < trailPos.Count - 1; i++)
    {
        Vector3 a = trailPos[i]; a.y = 0f;
        Vector3 b = trailPos[i + 1]; b.y = 0f;

        Vector3 ab = b - a;
        float ab2 = Vector3.Dot(ab, ab);
        if (ab2 <= 0.000001f) continue;

        float t = Vector3.Dot(p - a, ab) / ab2;
        t = Mathf.Clamp01(t);

        Vector3 proj = a + ab * t;
        float d2 = (p - proj).sqrMagnitude;

        if (d2 < bestD2)
        {
            bestD2 = d2;
            float segL = Mathf.Sqrt(ab2);
            bestS = trailS[i] + segL * t;
        }
    }

    return bestS;
}


// Helpers (los mismos de antes)
private static float ProjectDistanceOnPolyline(Vector3[] pts, float[] cumLen, Vector3 worldPos)
{
    Vector3 p = worldPos; p.y = 0f;

    float bestS = 0f;
    float bestD2 = float.PositiveInfinity;

    for (int i = 0; i < pts.Length - 1; i++)
    {
        Vector3 a = pts[i]; a.y = 0f;
        Vector3 b = pts[i + 1]; b.y = 0f;

        Vector3 ab = b - a;
        float ab2 = Vector3.Dot(ab, ab);
        if (ab2 <= 0.000001f) continue;

        float t = Vector3.Dot(p - a, ab) / ab2;
        t = Mathf.Clamp01(t);

        Vector3 proj = a + ab * t;
        float d2 = (p - proj).sqrMagnitude;

        if (d2 < bestD2)
        {
            bestD2 = d2;
            float segL = Mathf.Sqrt(ab2);
            bestS = cumLen[i] + segL * t;
        }
    }

    return bestS;
}

private static Vector3 PointAtDistance(Vector3[] pts, float[] segLen, float[] cumLen, float s)
{
    if (s <= 0f) return pts[0];
    float total = cumLen[cumLen.Length - 1];
    if (s >= total) return pts[pts.Length - 1];

    for (int i = 0; i < segLen.Length; i++)
    {
        float a = cumLen[i];
        float b = cumLen[i + 1];
        if (s > b) continue;

        float L = segLen[i];
        if (L <= 0.000001f) return pts[i];

        float t = (s - a) / L;
        return Vector3.Lerp(pts[i], pts[i + 1], t);
    }

    return pts[pts.Length - 1];
}

private static void SampleTrail(
    List<Vector3> pos,
    List<Quaternion> rot,
    List<float> s,
    float targetS,
    out Vector3 outPos,
    out Quaternion outRot)
{
    int last = s.Count - 1;

    if (targetS <= s[0]) { outPos = pos[0]; outRot = rot[0]; return; }
    if (targetS >= s[last]) { outPos = pos[last]; outRot = rot[last]; return; }

    int j = 1;
    while (j < s.Count && s[j] < targetS) j++;

    int a = j - 1;
    int b = j;

    float t = Mathf.InverseLerp(s[a], s[b], targetS);
    outPos = Vector3.Lerp(pos[a], pos[b], t);
    outRot = Quaternion.Slerp(rot[a], rot[b], t);
}

private static void RegistrarRenderersSubterraneos(Transform root, List<Renderer> renderers, List<bool> enabledInicial)
{
  if (root == null) return;

  Renderer[] encontrados = root.GetComponentsInChildren<Renderer>(true);
  for (int i = 0; i < encontrados.Length; i++)
  {
    Renderer r = encontrados[i];
    if (r == null) continue;

    renderers.Add(r);
    enabledInicial.Add(r.enabled);
  }
}

private static void SetRenderersVisible(List<Renderer> renderers, bool visible)
{
  if (renderers == null) return;

  for (int i = 0; i < renderers.Count; i++)
  {
    Renderer r = renderers[i];
    if (r == null) continue;
    r.enabled = visible;
  }
}

private static void RestaurarRenderers(List<Renderer> renderers, List<bool> enabledInicial)
{
  if (renderers == null || enabledInicial == null) return;
  int n = Mathf.Min(renderers.Count, enabledInicial.Count);

  for (int i = 0; i < n; i++)
  {
    Renderer r = renderers[i];
    if (r == null) continue;
    r.enabled = enabledInicial[i];
  }
}

private static float CalcularOffsetYSubterraneo(float progreso, float tramoEntradaSalida, float profundidadY)
{
  if (tramoEntradaSalida <= 0.0001f) return -Mathf.Abs(profundidadY);

  float depth = Mathf.Abs(profundidadY);
  float p = Mathf.Clamp01(progreso);

  if (p <= tramoEntradaSalida)
  {
    float t = Mathf.Clamp01(p / tramoEntradaSalida);
    return -Mathf.SmoothStep(0f, depth, t);
  }

  if (p >= 1f - tramoEntradaSalida)
  {
    float t = Mathf.Clamp01((p - (1f - tramoEntradaSalida)) / tramoEntradaSalida);
    return -Mathf.SmoothStep(depth, 0f, t);
  }

  return -depth;
}

private static float CalcularPitchSubterraneo(float progreso, float tramoEntradaSalida, float pitchMax)
{
  if (tramoEntradaSalida <= 0.0001f || Mathf.Abs(pitchMax) <= 0.0001f) return 0f;

  float maxPitch = Mathf.Abs(pitchMax);
  float p = Mathf.Clamp01(progreso);

  if (p <= tramoEntradaSalida)
  {
    float t = Mathf.Clamp01(p / tramoEntradaSalida);
    return Mathf.Lerp(0f, maxPitch, t);
  }

  if (p >= 1f - tramoEntradaSalida)
  {
    float t = Mathf.Clamp01((p - (1f - tramoEntradaSalida)) / tramoEntradaSalida);
    return Mathf.Lerp(-maxPitch, 0f, t);
  }

  return 0f;
}

public int ObtenerVisualCodeActual()
{
  return numVisualActual;
}

public bool ObtenerEstadoMisterioso()
{
  return esMisterioso;
}

public bool ObtenerAtajoSubterraneoPendiente()
{
  return atajoSubterraneoPendiente;
}

private static float CalcularIntensidadSubterranea(float progreso, float tramoEntradaSalida)
{
  if (tramoEntradaSalida <= 0.0001f) return 1f;

  float p = Mathf.Clamp01(progreso);
  if (p <= tramoEntradaSalida)
  {
    float t = Mathf.Clamp01(p / tramoEntradaSalida);
    return Mathf.SmoothStep(0f, 1f, t);
  }

  if (p >= 1f - tramoEntradaSalida)
  {
    float t = Mathf.Clamp01((p - (1f - tramoEntradaSalida)) / tramoEntradaSalida);
    return Mathf.SmoothStep(1f, 0f, t);
  }

  return 1f;
}

private static GameObject CrearOverlayViajeSubterraneo(out Image overlayImage)
{
  overlayImage = null;

  GameObject canvasGO = new GameObject("UndergroundTravelOverlay", typeof(Canvas));
  Canvas canvas = canvasGO.GetComponent<Canvas>();
  canvas.renderMode = RenderMode.ScreenSpaceOverlay;
  canvas.sortingOrder = 8000;

  GameObject tintGO = new GameObject("Tint", typeof(RectTransform), typeof(Image));
  tintGO.transform.SetParent(canvasGO.transform, false);

  RectTransform rt = tintGO.GetComponent<RectTransform>();
  rt.anchorMin = Vector2.zero;
  rt.anchorMax = Vector2.one;
  rt.offsetMin = Vector2.zero;
  rt.offsetMax = Vector2.zero;

  overlayImage = tintGO.GetComponent<Image>();
  overlayImage.raycastTarget = false;
  overlayImage.color = new Color(0.2f, 0.12f, 0.06f, 0f);

  return canvasGO;
}

private static void AplicarTinteSubterraneo(Image overlayImage, float intensidad, float alphaMax)
{
  if (overlayImage == null) return;

  Color c = overlayImage.color;
  c.a = Mathf.Clamp01(intensidad) * Mathf.Clamp01(alphaMax);
  overlayImage.color = c;
}

private static AudioListener ObtenerAudioListenerActivo()
{
  if (Camera.main != null)
  {
    AudioListener mainListener = Camera.main.GetComponent<AudioListener>();
    if (mainListener != null) return mainListener;
  }

  return Object.FindObjectOfType<AudioListener>();
}

private static UndergroundAudioFxState PrepararAudioSubterraneo()
{
  AudioListener listener = ObtenerAudioListenerActivo();
  if (listener == null) return null;

  GameObject go = listener.gameObject;
  var estado = new UndergroundAudioFxState();

  estado.reverb = go.GetComponent<AudioReverbFilter>();
  if (estado.reverb == null)
  {
    estado.reverb = go.AddComponent<AudioReverbFilter>();
    estado.createdReverb = true;
  }
  estado.reverbWasEnabled = estado.reverb.enabled;
  estado.reverbPresetBefore = estado.reverb.reverbPreset;
  estado.reverb.enabled = true;

  estado.lowPass = go.GetComponent<AudioLowPassFilter>();
  if (estado.lowPass == null)
  {
    estado.lowPass = go.AddComponent<AudioLowPassFilter>();
    estado.createdLowPass = true;
  }
  estado.lowPassWasEnabled = estado.lowPass.enabled;
  estado.lowPassCutoffBefore = estado.lowPass.cutoffFrequency;
  estado.lowPassResonanceBefore = estado.lowPass.lowpassResonanceQ;
  estado.lowPass.enabled = true;

  estado.echo = go.GetComponent<AudioEchoFilter>();
  if (estado.echo == null)
  {
    estado.echo = go.AddComponent<AudioEchoFilter>();
    estado.createdEcho = true;
  }
  estado.echoWasEnabled = estado.echo.enabled;
  estado.echoWetBefore = estado.echo.wetMix;
  estado.echoDryBefore = estado.echo.dryMix;
  estado.echoDelayBefore = estado.echo.delay;
  estado.echoDecayBefore = estado.echo.decayRatio;
  estado.echo.enabled = true;

  return estado;
}

private static void ActualizarAudioSubterraneo(UndergroundAudioFxState estado, float intensidad)
{
  if (estado == null) return;

  float blend = Mathf.Clamp01(intensidad);

  if (estado.reverb != null)
  {
    estado.reverb.enabled = blend > 0.001f;
    estado.reverb.reverbPreset = AudioReverbPreset.Cave;
  }

  if (estado.lowPass != null)
  {
    estado.lowPass.enabled = blend > 0.001f;
    estado.lowPass.cutoffFrequency = Mathf.Lerp(22000f, 1200f, blend);
    estado.lowPass.lowpassResonanceQ = Mathf.Lerp(1f, 1.15f, blend);
  }

  if (estado.echo != null)
  {
    estado.echo.enabled = blend > 0.001f;
    estado.echo.wetMix = Mathf.Lerp(0f, 0.25f, blend);
    estado.echo.dryMix = 1f;
    estado.echo.delay = Mathf.Lerp(30f, 140f, blend);
    estado.echo.decayRatio = Mathf.Lerp(0f, 0.18f, blend);
  }
}

private static void RestaurarAudioSubterraneo(UndergroundAudioFxState estado)
{
  if (estado == null) return;

  if (estado.reverb != null)
  {
    if (estado.createdReverb)
      Object.Destroy(estado.reverb);
    else
    {
      estado.reverb.reverbPreset = estado.reverbPresetBefore;
      estado.reverb.enabled = estado.reverbWasEnabled;
    }
  }

  if (estado.lowPass != null)
  {
    if (estado.createdLowPass)
      Object.Destroy(estado.lowPass);
    else
    {
      estado.lowPass.cutoffFrequency = estado.lowPassCutoffBefore;
      estado.lowPass.lowpassResonanceQ = estado.lowPassResonanceBefore;
      estado.lowPass.enabled = estado.lowPassWasEnabled;
    }
  }

  if (estado.echo != null)
  {
    if (estado.createdEcho)
      Object.Destroy(estado.echo);
    else
    {
      estado.echo.wetMix = estado.echoWetBefore;
      estado.echo.dryMix = estado.echoDryBefore;
      estado.echo.delay = estado.echoDelayBefore;
      estado.echo.decayRatio = estado.echoDecayBefore;
      estado.echo.enabled = estado.echoWasEnabled;
    }
  }
}

private static GameObject GetOrCreateUndergroundTravelMarker()
{
  if (undergroundTravelMarker != null) return undergroundTravelMarker;

  undergroundTravelMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
  undergroundTravelMarker.name = "UndergroundTravelMarker";
  undergroundTravelMarker.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);

  Collider col = undergroundTravelMarker.GetComponent<Collider>();
  if (col != null) Object.Destroy(col);

  Renderer renderer = undergroundTravelMarker.GetComponent<Renderer>();
  if (renderer != null)
  {
    Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
    if (shader == null) shader = Shader.Find("Unlit/Color");
    if (shader == null) shader = Shader.Find("Standard");

    Material markerMaterial = new Material(shader);
    markerMaterial.color = new Color(0.62f, 0.98f, 0.76f, 0.85f);
    renderer.sharedMaterial = markerMaterial;
    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    renderer.receiveShadows = false;
  }

  undergroundTravelMarker.SetActive(false);
  return undergroundTravelMarker;
}




}



