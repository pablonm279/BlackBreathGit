using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using System;
using System.Text;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TipoHighlightNodoCampania
{
  Asentamiento,
  Ritual,
  Incendio,
  AtajoSuperficie,
  AtajoSubterraneo,
  MisionSalvamento
}

public enum TipoAvanceTiempoCampania
{
  Viaje,
  Descanso,
  Posada,
  Asentamiento,
  Exploradores
}

public class CampaignManager : MonoBehaviour
{
  public class ResultadoExploradoresCampania
  {
    public Nodo nodoObjetivo;
    public string titulo;
    public string descripcion;
    public int tirada;
    public int chance;
    public bool exito;
    public bool critico;
    public int civilesMuertos;
    public int civilesDevueltos;
    public int oroGanado;
    public int materialesGanados;
    public int esperanzaCambio;
    public string faccionReveladaNombre;
  }

  private class SlotHighlightNodoCampania
  {
    public GameObject root;
    public RectTransform rect;
    public Image image;
    public TMP_Text texto;
    public RectTransform textoRect;
    public Animator animator;
    public Coroutine rutina;
    public bool ocupado;
    public Vector3 offsetTextoPantalla;
  }

  public static CampaignManager Instance { get; private set; }
  private const int MinTierMejoraCaravana = 1;
  private const int MaxTierMejoraCaravana = 5;
  private const string TooltipPersonajeTiendasId = "campania_personaje_tiendas";
  private const string TooltipPersonajePesoId = "campania_personaje_peso";
  private const string TooltipEmboscadaNormalId = "campania_emboscada_normal";
  private const string TooltipPersonajeDescansandoId = "campania_personaje_descansando";
  private const bool DEBUG_FORZAR_OLA_DE_CALOR_AL_PLAY = false;
  private const bool DEBUG_FORZAR_MASACRE_NEDUKAZAL = false;
  private const bool DEBUG_ABRIR_MENU_SERRIA_AL_INICIAR = false;
  private const int DistanciaVisionBase = 1;
  private const int DistanciaVisionMinima = 1;
  private const float AlcanceVisionMinimoPasos = 1.14f;
  private const float MultiplicadorAlcanceVisionCatalejos = 0.85f;
  private const float MultiplicadorAlcanceVisionAntorchas = 0.80f;
  private const float MultiplicadorVisionNiebla = 0.85f;
  private static readonly float[] AlcanceVisionCatalejosPorTier =
  {
    1.5f,
    1.9f,
    2.5f,
    2.9f,
    3.5f
  };
  private const float DuracionEnvioExploradoresHoras = 3f;
  private const float DuracionAnimacionEnvioExploradoresSegundos = 5f;
  private const float DuracionResultadoExploradoresSegundos = 1.2f;
  private const float RetrasoInicioEnvioExploradoresSegundos = 0.25f;
  private const float RetrasoEntreTextosExploradoresSegundos = 0.9f;
  private const float DuracionHighlightNodoSegundos = 10f;
  private const float DuracionFadeOutHighlightNodoSegundos = 1f;
  private bool enviandoExploradores;
  private readonly Dictionary<GameObject, bool> estadosCanvasCampaniaDuranteExploradores = new Dictionary<GameObject, bool>();
  private readonly Dictionary<GameObject, bool> estadosCanvasCampaniaDuranteIntro = new Dictionary<GameObject, bool>();
  private readonly Dictionary<int, int> ultimasAparienciasAlternativasPorClase = new Dictionary<int, int>();
  private readonly List<System.Action> accionesAlFinalizarIntroCampania = new List<System.Action>();
  private readonly List<string> logsPresagiosInicioPendientes = new List<string>();
  private bool introCampaniaPendiente;
  private bool introCampaniaActiva;
  private bool interfazCampaniaOcultaPorIntro;
  private bool faderIntroCampaniaTomado;
  private bool startCampaniaEjecutado;
  private bool eventoInicioCampaniaEmitido;
  private Coroutine rutinaIntroCampaniaTrasCarga;
  private Coroutine rutinaLogsPresagiosInicio;
  private bool logsPresagiosInicioSolicitados;
  private const float AlphaFaderListoIntroCampania = 0.02f;
  private const float TiempoMaximoEsperaFaderIntroCampania = 6f;
  private const float RetrasoLogsPresagiosInicioSegundos = 1f;
  private const string ColorEtiquetaLogPresagio = "#C16070";
  [Header("Debug Demo")]
  [SerializeField] private bool debugSaltarTutorialAlIniciar = false;
  [SerializeField] private bool debugForzarMapaLinealTutorialAlIniciar = false;
  [SerializeField] private bool debugPermitirZonaBosque = false;
  [SerializeField] private bool debugPermitirZonaPasoVientoHelado = false;
  [SerializeField] private bool debugPermitirZonaNedukazal = false;
  [SerializeField] private bool debugIniciarConEstadosCaravana = false;
  [SerializeField] private bool debugForzarCombateFinalBosqueAlIniciar = false;
  [SerializeField] private bool debugForzarCombateJefeGulekGulAlIniciar = false;
  [SerializeField] private bool debugForzarAuroraPasoVientoHelado = false;
  [SerializeField, InspectorName("Debug Ignorar Peleas")] private bool debugIgnorarCombates = false;
  [SerializeField] private bool debugMostrarTodosLosCaminosMapa = false;
  [SerializeField] private bool debugIniciarConCaballeroCompleto = false;
  [SerializeField] private bool debugIniciarConExploradorCompleto = false;
  [SerializeField] private bool debugIniciarConPurificadoraCompleta = false;
  [SerializeField] private bool debugIniciarConAcechadorCompleto = false;
  [SerializeField] private bool debugIniciarConCanalizadorCompleto = false;
  [SerializeField] private bool debugIniciarConDuelistaCompleta = false;
  [SerializeField] private bool debugAbrirPantallaEventosAlIniciar = false;
  private bool debugMostrarTodosLosCaminosMapaAplicado;
  [Header("Debug mouse")]
  [SerializeField] private KeyCode teclaDebugBajoMouse = KeyCode.F10;
  [SerializeField] private int maxHitsDebugBajoMouse = 12;
  private const int CapacidadRaycastNodosCampania = 64;
  private Nodo nodoHoverCampaniaActual;
  private readonly List<RaycastResult> resultadosRaycastUICampania = new List<RaycastResult>();
  private readonly RaycastHit[] hitsRaycastNodosCampania = new RaycastHit[CapacidadRaycastNodosCampania];
  private PointerEventData pointerRaycastUICampania;
  private EventSystem eventSystemRaycastUICampania;
  private Camera camaraRaycastNodosCampania;

  public GameObject prefabTextoRecursos;
  [SerializeField] private float recursoTextoStackOffsetY = 16f;
  [SerializeField] private float recursoTextoStackOffsetX = 10f;
  [SerializeField] private int recursoTextoMaxStackVisual = 4;
  [SerializeField] private float recursoTextoDuracionExtra = 5.35f;
  [SerializeField] private float recursoTextoAnimatorSpeed = 0.85f;
  [SerializeField] private float recursoTextoIntervaloSpawn = 0.25f;
  [SerializeField] private float recursoTextoVentanaStackGlobal = 0.45f;
  private const float RecursoTextoDeltaAnimacionY = 110f;
  private const float RecursoTextoDuracionMovimiento = 2.3833334f;
  private const float RecursoTextoDuracionFade = 3.45f;
  private const float RecursoTextoAlphaIntermedio = 0.9843137f;
  private readonly Queue<SolicitudTextoRecurso> colaTextosRecursos = new Queue<SolicitudTextoRecurso>();
  private readonly Queue<SolicitudTextoRecurso> colaTextosRecursosSuspendidos = new Queue<SolicitudTextoRecurso>();
  private readonly List<RegistroTextoRecurso> textosRecursosRecientes = new List<RegistroTextoRecurso>();
  private Coroutine rutinaTextosRecursos;
  private bool pausandoTextoDistanciaAliento;
  public Animator animCaravana;
  public GameObject goCanvas;
  public GameObject highlightNodos;
  private readonly List<SlotHighlightNodoCampania> slotsHighlightNodos = new List<SlotHighlightNodoCampania>();
  private GameObject origenSlotsHighlightNodos;
  public MapaManager scMapaManager;
  [SerializeField, Min(0f)] private float escalaNodos = 0f;
  public AtributosZona scAtributosZona;
  public TutorialManager scTutorialManager;
  public AlientoNegroVFX scAlientoNegroVFX;
  public MenuSequitos scMenuSequito;
  public MenuPersonajes scMenuPersonajes;
  public GameObject goMenuPuerto;
  public int mejoraCaravanaAntorchas;
  public int mejoraCaravanaAlforjas;
  public int mejoraCaravanaTiendas;
  public int mejoraCaravanaCatalejos;
  public int mejoraCaravanaAlmacen;
  public int mejoraCaravanaDefensas;
  private float estadisticaHorasViajadas;
  private int estadisticaBatallasLibradas;
  private int estadisticaCivilesPerdidos;
  private int estadisticaAsentamientosVisitados;

  // Sonido de la caravana al moverse (asignar desde Inspector)
  public AudioClip sfxMovimientoCaravana;
  [Range(0f, 1f)] public float sfxMovimientoVolumen = 0.8f;
  [Range(0.5f, 1.5f)] public float sfxMovimientoPitch = 0.85f;
  [SerializeField] private float sfxMovimientoFadeIn = 0.35f;
  [SerializeField] private float sfxMovimientoFadeOut = 0.4f;
  private AudioSource sfxMovimientoSource;
  private Coroutine rutinaDesvanecerSfxMovimiento;

  public float sequitoHerrerosMantArmasHoras;
  public float sequitoHerrerosMantArmadurasHoras;
  public int sequitoMercaderesTier;
  public TextMeshProUGUI distanciaAlientotxt;
  public SequitoMercaderes scSequitoMercaderes;
  public SequitoArtistas scSequitoArtistas;
  public SequitoHerboristas scSequitoHerboristas;
  public SequitoDesertores scSequitoDesertores;
  public SequitoCronistas scSequitoCronistas;
  public SequitoRefugiados scSequitoRefugiados;
  public SequitoNobles scSequitoNobles;
  public SequitoClerigos scSequitoClerigos;
  public SequitoEsclavos scSequitoEsclavos;

  public float sequitoCuranderosMejoraCuracion;
  public EstadosCaravana estadosCaravana = new EstadosCaravana();

  private const double HoraInicioCampania = 9d;
  [SerializeField] private double horasTotales = HoraInicioCampania;
  public int numeroTurno => ObtenerDiaCalendario();
  [SerializeField] private bool antorchasEncendidas = true;
  private CaravanTorchLight[] lucesAntorchasCaravana;
  [SerializeField] private float progresoFatigaHoras;
  [SerializeField] private float acumuladorEfectosSequitosHoras;
  [SerializeField] private bool combateHoraCapturada;
  [SerializeField] private bool combateNocturno;
  [SerializeField] private bool descansoInterrumpidoPendiente;
  [SerializeField] private bool descansoResultadosPendientes;
  [SerializeField] private bool descansoTuvoEmboscada;
  [SerializeField] private bool descansoRitualElegible;
  [SerializeField] private string descansoRitualPersonajeId;
  [SerializeField] private int descansoClimaInicial;
  [SerializeField] private float descansoValorTarea;
  [SerializeField] private int descansoChanceExploracion;
  [SerializeField] private int descansoChanceEmboscada;
  [SerializeField] private bool descansoEmboscadaPendiente;
  [SerializeField] private float descansoHorasHastaEmboscada;
  [SerializeField] private int descansoTiradaEmboscada;
  [SerializeField] private float descansoHorasRestantes;
  [SerializeField] private int descansoTareaCivil;
  [SerializeField] private bool descansoEnClaro;
  [SerializeField] private float descansoHoraCombate;
  [SerializeField] private float creditoPrevencionAlientoHoras;
  private bool viajeActualIncluyoNoche;
  [SerializeField] private int viajeClimaInicial;
  [SerializeField] private float horasViajeActual;
  private bool continuacionDescansoGestionadaPorMenu;
  public const float HorasPorPasoMapa = 5f;
  private const float HoraInicioNoche = 21f;
  private const float HoraFinNoche = 6f;
  private const string TEXTO_LOG_INICIO_CAMPANIA = "El viaje de la caravana ha comenzado.";
  private bool logInicioCampaniaEscrito;
  public bool MoviendoCaravana = false;
  private bool transicionZonaEnCurso = false;
  private bool bloquearOlaDeCalorEnSiguienteTiradaClima;
  public Nodo nodoDestinoActual;
  private readonly List<int> eventosAleatoriosUsadosMapa = new List<int>();
  private const float MultiplicadorVelocidadSobrecarga = 0.70f;
  private const float MultiplicadorDuracionAtajoSubterraneo = 0.70f;
  private float multiplicadorVelocidadVisualViajeActual = 1f;
  private bool estadoSobrecargaInicializado;
  private bool sobrecargaAnterior;

  public GameObject prefabGOPersonaje;

  public ContenedorPrefabsCamp scContprefab;

  public int BATALLA_EnCurso;
  public int EMBOSCADA_EnCurso;
  private bool emboscadaViajeCalculada;
  private string logEmboscadaViajePendiente;

  public GameObject goBotonViajando;
  public GameObject goBotonResolverCombate;
  public GameObject goBotonAcampar;
  public GameObject goSequitos;
  public GameObject goLogCampania;
  public GameObject goDerrota;

  public AdministradorEscenas scAdministradorEscenas;

  // Cola y reacomodo de textos flotantes para evitar solapamientos.
  [SerializeField] private float gapEntreMensajes = 0.5f;
  [SerializeField] private bool usarTextoFlotanteManager = false;
  [SerializeField] private float yStackOffset = 28f;            // separación vertical mínima entre mensajes visibles
  [SerializeField, Min(0f)] private float duracionEntradaTextoFlotante = 0.35f;
  private readonly Queue<(string, Color)> colaTextos = new Queue<(string, Color)>();
  private readonly Queue<(string, Color)> colaTextosSuspendidosCampania = new Queue<(string, Color)>();
  private readonly List<(RectTransform contenedor, RectTransform rectTexto, float altura)> textosFlotantesCampaniaActivos = new List<(RectTransform, RectTransform, float)>();
  private bool procesandoCola;
  private int bloqueoTextosFlotantesCampania;
  private int bloqueoTextosRecursosCampania;
  private float tiempoUltimoSpawnTiempoReal = float.NegativeInfinity;
  private Coroutine rutinaTextoFlotanteCampania;
  private readonly Dictionary<TextMeshProUGUI, string> textosOriginalesDerrotaTMP = new Dictionary<TextMeshProUGUI, string>();
  private readonly Dictionary<Text, string> textosOriginalesDerrotaLegacy = new Dictionary<Text, string>();
  private bool textosDerrotaCacheados;
  private bool inicializandoNuevaCampania;
  private bool creandoLiderInicial;
  private int presagiosRegionActivaId;
  private readonly List<int> presagiosActivosRegion = new List<int>();
  private bool primeraBatallaPresagioEnemigosConsumida;
  // Demo: la Alerta Regional de metaprogresion no modifica dificultad ni aumenta hasta habilitar este interruptor.
  private static readonly bool MecanicaAlertaRegionHabilitada = false;
  private bool resolviendoJefeZona;
  private bool abriendoCiudadPuerto;
  private bool campaniaInicializada;
  private bool debeEscribirLogInicioEnStart;
  private bool cursorCampaniaMostrandoAlerta;
  [SerializeField] private AsentamientoManager asentamientoManager;
  private const string NombreTextoCargaCampania = "Cargando";
  private const string TextoCargaCampania = "Cargando...";
  private const float IntervaloAnimacionTextoCargaCampania = 0.35f;
  private readonly List<TextMeshProUGUI> textosCargaCampania = new List<TextMeshProUGUI>();
  private Coroutine rutinaAnimacionTextoCargaCampania;
  private string prefijoTextoCargaCampania = "Cargando";

#if UNITY_EDITOR
  private static string ObtenerClaveEditorDebugZona(int zonaId)
  {
    return $"{Application.dataPath}:CampaignManager:DebugPermitirZona:{zonaId}";
  }

  public static bool EsZonaPermitidaPorDebug(int zonaId)
  {
    return EditorPrefs.GetBool(ObtenerClaveEditorDebugZona(zonaId), false);
  }

  private void OnValidate()
  {
    EditorPrefs.SetBool(ObtenerClaveEditorDebugZona(1), debugPermitirZonaBosque);
    EditorPrefs.SetBool(ObtenerClaveEditorDebugZona(2), debugPermitirZonaPasoVientoHelado);
    EditorPrefs.SetBool(ObtenerClaveEditorDebugZona(3), debugPermitirZonaNedukazal);

    if (Application.isPlaying)
    {
      return;
    }

    AsentamientoManager encontrado = BuscarAsentamientoManagerEnEscena();
    if (encontrado == null || asentamientoManager == encontrado)
    {
      return;
    }

    asentamientoManager = encontrado;
    EditorUtility.SetDirty(this);
  }
#endif

  private void Awake()
  {
    if (Instance != null)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    if (highlightNodos != null)
    {
      PrepararSlotsHighlightNodosCampania();
      OcultarTodosSlotsHighlightNodosCampania();
    }

    PrepararEscenaCampania();

    if (SaveGameService.TryConsumePendingLoad(out SaveFileData savePendiente))
    {
      if (!CargarCampaniaPendiente(savePendiente, out string error, true))
      {
        SaveGameService.ReportPendingLoadFailure(error);
      }
      return;
    }

    InicializarNuevaCampania();

}

  public void MarcarNodoCampaniaTemporal(Nodo nodo, TipoHighlightNodoCampania tipo, float retrasoSegundos = 0f)
  {
    if (nodo == null || highlightNodos == null || !DebeMostrarHighlightNodoCampania(nodo, tipo))
    {
      return;
    }

    PrepararSlotsHighlightNodosCampania();
    SlotHighlightNodoCampania slot = ObtenerSlotHighlightNodoCampaniaDisponible();
    if (slot == null)
    {
      return;
    }

    if (slot.rutina != null)
    {
      StopCoroutine(slot.rutina);
      OcultarVisualSlotHighlightNodoCampania(slot);
      slot.rutina = null;
    }

    slot.ocupado = true;
    slot.rutina = StartCoroutine(RutinaMarcarNodoCampaniaTemporal(slot, nodo, tipo, retrasoSegundos));
  }

  public void CancelarHighlightsNodosCampania()
  {
    PrepararSlotsHighlightNodosCampania();

    for (int i = 0; i < slotsHighlightNodos.Count; i++)
    {
      SlotHighlightNodoCampania slot = slotsHighlightNodos[i];
      if (slot == null)
      {
        continue;
      }

      if (slot.rutina != null)
      {
        StopCoroutine(slot.rutina);
      }
    }

    OcultarTodosSlotsHighlightNodosCampania();
  }

  private IEnumerator RutinaMarcarNodoCampaniaTemporal(SlotHighlightNodoCampania slot, Nodo nodo, TipoHighlightNodoCampania tipo, float retrasoSegundos)
  {
    if (retrasoSegundos > 0f)
    {
      yield return new WaitForSecondsRealtime(retrasoSegundos);
    }

    if (slot == null || nodo == null || highlightNodos == null || !DebeMostrarHighlightNodoCampania(nodo, tipo))
    {
      LiberarSlotHighlightNodoCampania(slot);
      yield break;
    }

    TutorialTarget target = nodo != null ? nodo.GetComponent<TutorialTarget>() : null;
    Color colorBase = ObtenerColorHighlightNodoCampania(tipo);
    float tiempo = 0f;
    float duracionVisible = Mathf.Max(0f, DuracionHighlightNodoSegundos - DuracionFadeOutHighlightNodoSegundos);

    highlightNodos.SetActive(true);
    slot.root.SetActive(true);
    if (slot.texto != null)
    {
      slot.texto.gameObject.SetActive(true);
    }

    if (slot.root.transform.localScale.sqrMagnitude <= 0.0001f)
    {
      slot.root.transform.localScale = Vector3.one;
    }

    slot.root.transform.SetAsLastSibling();
    if (slot.texto != null && !slot.texto.transform.IsChildOf(slot.root.transform))
    {
      slot.texto.transform.SetAsLastSibling();
    }

    slot.offsetTextoPantalla = slot.textoRect != null && slot.rect != null
      ? (Vector3)(slot.textoRect.anchoredPosition - slot.rect.anchoredPosition)
      : Vector3.zero;

    ConfigurarContenidoHighlightNodoCampania(slot.image, slot.texto, tipo, colorBase, 1f);

    if (slot.animator != null)
    {
      slot.animator.Play(0, 0, 0f);
      slot.animator.Update(0f);
    }

    while (tiempo < DuracionHighlightNodoSegundos
      && nodo != null
      && DebeMostrarHighlightNodoCampania(nodo, tipo)
      && nodo.gameObject.activeInHierarchy
      && slot.root != null
      && slot.root.activeInHierarchy)
    {
      PosicionarHighlightNodoCampania(nodo, target, slot);
      float alpha = tiempo <= duracionVisible
        ? 1f
        : Mathf.Clamp01(1f - ((tiempo - duracionVisible) / Mathf.Max(0.01f, DuracionFadeOutHighlightNodoSegundos)));
      AplicarAlphaHighlightNodoCampania(slot.image, slot.texto, colorBase, alpha);
      tiempo += Time.unscaledDeltaTime;
      yield return null;
    }

    LiberarSlotHighlightNodoCampania(slot);
  }

  bool DebeMostrarHighlightNodoCampania(Nodo nodo, TipoHighlightNodoCampania tipo)
  {
    if (nodo == null)
    {
      return false;
    }

    if (tipo == TipoHighlightNodoCampania.Incendio || tipo == TipoHighlightNodoCampania.Ritual)
    {
      return nodo.revelado;
    }

    return true;
  }

  private void PrepararSlotsHighlightNodosCampania()
  {
    if (highlightNodos == null)
    {
      slotsHighlightNodos.Clear();
      origenSlotsHighlightNodos = null;
      return;
    }

    if (origenSlotsHighlightNodos == highlightNodos && slotsHighlightNodos.Count > 0)
    {
      return;
    }

    slotsHighlightNodos.Clear();
    origenSlotsHighlightNodos = highlightNodos;

    Image imagenRaiz = highlightNodos.GetComponent<Image>();
    if (imagenRaiz != null)
    {
      AgregarSlotHighlightNodoCampania(imagenRaiz, highlightNodos.GetComponentInChildren<TMP_Text>(true));
      return;
    }

    Image[] imagenes = highlightNodos.GetComponentsInChildren<Image>(true);
    TMP_Text[] textos = highlightNodos.GetComponentsInChildren<TMP_Text>(true);
    for (int i = 0; i < imagenes.Length; i++)
    {
      TMP_Text texto = i < textos.Length ? textos[i] : imagenes[i].GetComponentInChildren<TMP_Text>(true);
      AgregarSlotHighlightNodoCampania(imagenes[i], texto);
    }
  }

  private void AgregarSlotHighlightNodoCampania(Image image, TMP_Text texto)
  {
    if (image == null)
    {
      return;
    }

    slotsHighlightNodos.Add(new SlotHighlightNodoCampania
    {
      root = image.gameObject,
      rect = image.GetComponent<RectTransform>(),
      image = image,
      texto = texto,
      textoRect = texto != null ? texto.GetComponent<RectTransform>() : null,
      animator = image.GetComponent<Animator>()
    });
  }

  private SlotHighlightNodoCampania ObtenerSlotHighlightNodoCampaniaDisponible()
  {
    for (int i = 0; i < slotsHighlightNodos.Count; i++)
    {
      SlotHighlightNodoCampania slot = slotsHighlightNodos[i];
      if (slot != null && !slot.ocupado)
      {
        return slot;
      }
    }

    return slotsHighlightNodos.Count > 0 ? slotsHighlightNodos[0] : null;
  }

  private void OcultarTodosSlotsHighlightNodosCampania()
  {
    for (int i = 0; i < slotsHighlightNodos.Count; i++)
    {
      OcultarVisualSlotHighlightNodoCampania(slotsHighlightNodos[i]);
      slotsHighlightNodos[i].ocupado = false;
      slotsHighlightNodos[i].rutina = null;
    }

    if (highlightNodos != null)
    {
      highlightNodos.SetActive(false);
    }
  }

  private void LiberarSlotHighlightNodoCampania(SlotHighlightNodoCampania slot)
  {
    if (slot == null)
    {
      return;
    }

    OcultarVisualSlotHighlightNodoCampania(slot);
    slot.ocupado = false;
    slot.rutina = null;

    if (highlightNodos != null && !HaySlotsHighlightNodosOcupados())
    {
      highlightNodos.SetActive(false);
    }
  }

  private bool HaySlotsHighlightNodosOcupados()
  {
    for (int i = 0; i < slotsHighlightNodos.Count; i++)
    {
      if (slotsHighlightNodos[i] != null && slotsHighlightNodos[i].ocupado)
      {
        return true;
      }
    }

    return false;
  }

  private void OcultarVisualSlotHighlightNodoCampania(SlotHighlightNodoCampania slot)
  {
    if (slot == null)
    {
      return;
    }

    if (slot.texto != null)
    {
      slot.texto.gameObject.SetActive(false);
    }

    if (slot.root != null)
    {
      slot.root.SetActive(false);
    }
  }

  private void ConfigurarContenidoHighlightNodoCampania(Image highlightImage, TMP_Text highlightTexto, TipoHighlightNodoCampania tipo, Color colorBase, float alpha)
  {
    AplicarAlphaHighlightNodoCampania(highlightImage, highlightTexto, colorBase, alpha);

    if (highlightTexto != null)
    {
      highlightTexto.text = ObtenerTextoHighlightNodoCampania(tipo);
      highlightTexto.gameObject.SetActive(true);
    }
  }

  private void AplicarAlphaHighlightNodoCampania(Image highlightImage, TMP_Text highlightTexto, Color colorBase, float alpha)
  {
    alpha = Mathf.Clamp01(alpha);
    Color color = colorBase;
    color.a = alpha;

    if (highlightImage != null)
    {
      highlightImage.color = color;
    }

    if (highlightTexto != null)
    {
      highlightTexto.color = color;
    }
  }

  private Color ObtenerColorHighlightNodoCampania(TipoHighlightNodoCampania tipo)
  {
    switch (tipo)
    {
      case TipoHighlightNodoCampania.Asentamiento:
        return new Color32(126, 214, 247, 255);
      case TipoHighlightNodoCampania.Ritual:
        return new Color32(180, 92, 255, 255);
      case TipoHighlightNodoCampania.Incendio:
        return new Color32(255, 138, 42, 255);
      case TipoHighlightNodoCampania.AtajoSuperficie:
        return new Color32(196, 154, 108, 255);
      case TipoHighlightNodoCampania.AtajoSubterraneo:
        return new Color32(12, 202, 116, 255);
      case TipoHighlightNodoCampania.MisionSalvamento:
        return new Color32(224, 184, 48, 255);
      default:
        return Color.white;
    }
  }

  private string ObtenerTextoHighlightNodoCampania(TipoHighlightNodoCampania tipo)
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;

    switch (tipo)
    {
      case TipoHighlightNodoCampania.Asentamiento:
        if (idioma == TRADU.IdiomaIngles) { return "Settlement discovered"; }
        if (idioma == TRADU.IdiomaPortugues) { return "Assentamento descoberto"; }
        return "Asentamiento descubierto";
      case TipoHighlightNodoCampania.Ritual:
        if (idioma == TRADU.IdiomaIngles) { return "Ritual detected"; }
        if (idioma == TRADU.IdiomaPortugues) { return "Ritual detectado"; }
        return "Ritual detectado";
      case TipoHighlightNodoCampania.Incendio:
        if (idioma == TRADU.IdiomaIngles) { return "The flames spread!"; }
        if (idioma == TRADU.IdiomaPortugues) { return "As chamas se espalham!"; }
        return "¡Las llamas se esparcen!";
      case TipoHighlightNodoCampania.AtajoSuperficie:
        if (idioma == TRADU.IdiomaIngles) { return "Shortcut found!"; }
        if (idioma == TRADU.IdiomaPortugues) { return "Atalho encontrado!"; }
        return "¡Atajo encontrado!";
      case TipoHighlightNodoCampania.AtajoSubterraneo:
        if (idioma == TRADU.IdiomaIngles) { return "Underground shortcut"; }
        if (idioma == TRADU.IdiomaPortugues) { return "Atalho subterrâneo"; }
        return "Atajo subterráneo";
      case TipoHighlightNodoCampania.MisionSalvamento:
        if (idioma == TRADU.IdiomaIngles) { return "Rescue Mission"; }
        if (idioma == TRADU.IdiomaPortugues) { return "Missão de Salvamento"; }
        return "Misión Salvamento";
      default:
        return string.Empty;
    }
  }

  private void PosicionarHighlightNodoCampania(Nodo nodo, TutorialTarget target, SlotHighlightNodoCampania slot)
  {
    if (nodo == null || highlightNodos == null || slot == null)
    {
      return;
    }

    Vector3 posicionPantalla = Vector3.zero;
    bool tienePosicion = false;

    if (target != null && target.rectTransform != null)
    {
      posicionPantalla = target.rectTransform.position;
      tienePosicion = true;
    }

    Camera camara = ObtenerCamaraHighlightNodoCampania(target);
    if (!tienePosicion && camara != null)
    {
      Transform referenciaMundo = target != null && target.worldTransform != null
        ? target.worldTransform
        : nodo.transform;
      posicionPantalla = camara.WorldToScreenPoint(referenciaMundo.position);
      tienePosicion = posicionPantalla.z >= 0f;
    }

    if (!tienePosicion && target != null)
    {
      tienePosicion = target.TryGetScreenPosition(out posicionPantalla);
    }

    if (!tienePosicion)
    {
      OcultarVisualSlotHighlightNodoCampania(slot);
      return;
    }

    if (!highlightNodos.activeSelf)
    {
      highlightNodos.SetActive(true);
    }

    if (slot.root != null && !slot.root.activeSelf)
    {
      slot.root.SetActive(true);
    }

    bool pudoConvertirPosicion = false;
    Vector2 posicionLocal = Vector2.zero;
    if (slot.rect != null)
    {
      pudoConvertirPosicion = TryConvertirPantallaALocalHighlightNodoCampania(slot.rect, posicionPantalla, out posicionLocal);
      if (pudoConvertirPosicion)
      {
        slot.rect.anchoredPosition = posicionLocal;
      }
      else
      {
        slot.rect.position = posicionPantalla;
      }

      if (target != null)
      {
        Vector2 tamano = target.GetHighlightSize();
        if (tamano.sqrMagnitude > 0f)
        {
          slot.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tamano.x);
          slot.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tamano.y);
        }
      }
    }

    if (slot.textoRect != null && slot.root != null && !slot.textoRect.transform.IsChildOf(slot.root.transform))
    {
      if (pudoConvertirPosicion)
      {
        slot.textoRect.anchoredPosition = posicionLocal + (Vector2)slot.offsetTextoPantalla;
      }
      else
      {
        slot.textoRect.position = posicionPantalla + slot.offsetTextoPantalla;
      }

      slot.texto.gameObject.SetActive(true);
    }
    else if (slot.root != null)
    {
      if (pudoConvertirPosicion && slot.root.transform is RectTransform rectRoot)
      {
        rectRoot.anchoredPosition = posicionLocal;
      }
      else
      {
        slot.root.transform.position = posicionPantalla;
      }
    }
  }

  private Camera ObtenerCamaraHighlightNodoCampania(TutorialTarget target)
  {
    if (EsCamaraActivaValida(target != null ? target.worldCamera : null))
    {
      return target.worldCamera;
    }

    Camera camaraMain = Camera.main;
    if (EsCamaraCampaniaPreferida(camaraMain))
    {
      return camaraMain;
    }

    Camera[] camaras = Camera.allCameras;
    for (int i = 0; i < camaras.Length; i++)
    {
      if (EsCamaraCampaniaPreferida(camaras[i]))
      {
        return camaras[i];
      }
    }

    if (EsCamaraActivaValida(camaraMain))
    {
      return camaraMain;
    }

    for (int i = 0; i < camaras.Length; i++)
    {
      if (EsCamaraActivaValida(camaras[i]) && !NombreIndicaCamaraBatalla(camaras[i]))
      {
        return camaras[i];
      }
    }

    for (int i = 0; i < camaras.Length; i++)
    {
      if (EsCamaraActivaValida(camaras[i]))
      {
        return camaras[i];
      }
    }

    return null;
  }

  private static bool EsCamaraCampaniaPreferida(Camera camara)
  {
    return EsCamaraActivaValida(camara) && camara.GetComponent("EdgePanCameraZ") != null;
  }

  private static bool EsCamaraActivaValida(Camera camara)
  {
    return camara != null && camara.enabled && camara.gameObject.activeInHierarchy;
  }

  private static bool NombreIndicaCamaraBatalla(Camera camara)
  {
    return camara != null
      && !string.IsNullOrEmpty(camara.name)
      && camara.name.IndexOf("Batalla", StringComparison.OrdinalIgnoreCase) >= 0;
  }

  private bool TryConvertirPantallaALocalHighlightNodoCampania(RectTransform rectObjetivo, Vector2 posicionPantalla, out Vector2 posicionLocal)
  {
    posicionLocal = Vector2.zero;
    if (rectObjetivo == null)
    {
      return false;
    }

    RectTransform rectPadre = rectObjetivo.parent as RectTransform;
    if (rectPadre == null)
    {
      return false;
    }

    Canvas canvasPadre = rectPadre.GetComponentInParent<Canvas>();
    Camera camaraCanvas = canvasPadre != null && canvasPadre.renderMode != RenderMode.ScreenSpaceOverlay
      ? canvasPadre.worldCamera
      : null;

    return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectPadre, posicionPantalla, camaraCanvas, out posicionLocal);
  }

public class AnimacionTextoRecursoManual : MonoBehaviour
{
  private RectTransform rectTransform;
  private TextMeshProUGUI texto;
  private Vector2 posicionInicial;
  private float tiempoTranscurrido;
  private float desplazamientoY;
  private float duracionMovimiento;
  private float duracionFade;
  private float alphaIntermedio;

  private void Awake()
  {
    enabled = false;
  }

  public void Configurar(float desplazamientoYConfigurado, float duracionMovimientoConfigurada, float duracionFadeConfigurada, float alphaIntermedioConfigurado)
  {
    rectTransform = GetComponent<RectTransform>();
    texto = GetComponent<TextMeshProUGUI>();
    posicionInicial = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
    tiempoTranscurrido = 0f;
    desplazamientoY = desplazamientoYConfigurado;
    duracionMovimiento = Mathf.Max(0.01f, duracionMovimientoConfigurada);
    duracionFade = Mathf.Max(duracionMovimiento, duracionFadeConfigurada);
    alphaIntermedio = Mathf.Clamp01(alphaIntermedioConfigurado);
    enabled = rectTransform != null;
  }

  private void Update()
  {
    if (rectTransform == null)
    {
      enabled = false;
      return;
    }

    tiempoTranscurrido += Time.unscaledDeltaTime;

    float progresoMovimiento = Mathf.Clamp01(tiempoTranscurrido / duracionMovimiento);
    rectTransform.anchoredPosition = posicionInicial + Vector2.up * (desplazamientoY * progresoMovimiento);

    if (texto != null)
    {
      Color colorActual = texto.color;
      if (tiempoTranscurrido <= duracionMovimiento)
      {
        colorActual.a = Mathf.Lerp(1f, alphaIntermedio, progresoMovimiento);
      }
      else
      {
        float progresoFade = Mathf.InverseLerp(duracionMovimiento, duracionFade, tiempoTranscurrido);
        colorActual.a = Mathf.Lerp(alphaIntermedio, 0f, progresoFade);
      }

      texto.color = colorActual;
    }

    if (tiempoTranscurrido >= duracionFade)
    {
      enabled = false;
    }
  }
}

  private void PrepararEscenaCampania()
  {
    AplicarTraduccionTextoCargaInicial();

    if (goDerrota != null)
    {
      goDerrota.SetActive(false);
      CachearTextosOriginalesDerrota();
    }

    AsegurarAudioMovimientoCaravana();
    AjustesAudio.AplicarVolumenSfx(sfxMovimientoSource, sfxMovimientoVolumen);
    sfxMovimientoSource.pitch = sfxMovimientoPitch * Mathf.Max(0.5f, multiplicadorVelocidadVisualViajeActual);
    AsegurarAsentamientoManager();
    ActualizarCursorCampania(true);
  }

  private void AplicarTraduccionTextoCargaInicial()
  {
    string textoCargaTraducido = ObtenerTextoCargaInicialSegunIdioma();
    prefijoTextoCargaCampania = ObtenerPrefijoTextoCarga(textoCargaTraducido);
    textosCargaCampania.Clear();
    TextMeshProUGUI[] textos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
    for (int i = 0; i < textos.Length; i++)
    {
      TextMeshProUGUI texto = textos[i];
      if (texto == null || texto.gameObject == null || texto.gameObject.name != NombreTextoCargaCampania)
      {
        continue;
      }

      if (!texto.gameObject.scene.IsValid())
      {
        continue;
      }

      texto.text = ConstruirTextoCargaAnimado(1);
      textosCargaCampania.Add(texto);
    }

    if (rutinaAnimacionTextoCargaCampania != null)
    {
      StopCoroutine(rutinaAnimacionTextoCargaCampania);
      rutinaAnimacionTextoCargaCampania = null;
    }

    if (textosCargaCampania.Count > 0)
    {
      rutinaAnimacionTextoCargaCampania = StartCoroutine(AnimarTextoCargaCampania());
    }
  }

  private string ObtenerPrefijoTextoCarga(string textoCargaTraducido)
  {
    string prefijo = string.IsNullOrEmpty(textoCargaTraducido)
      ? TextoCargaCampania
      : textoCargaTraducido;

    prefijo = prefijo.TrimEnd('.');
    return string.IsNullOrEmpty(prefijo) ? NombreTextoCargaCampania : prefijo;
  }

  private string ConstruirTextoCargaAnimado(int cantidadPuntos)
  {
    return prefijoTextoCargaCampania + new string('.', Mathf.Clamp(cantidadPuntos, 1, 3));
  }

  private IEnumerator AnimarTextoCargaCampania()
  {
    int cantidadPuntos = 1;
    WaitForSecondsRealtime espera = new WaitForSecondsRealtime(IntervaloAnimacionTextoCargaCampania);

    while (true)
    {
      string textoAnimado = ConstruirTextoCargaAnimado(cantidadPuntos);
      for (int i = textosCargaCampania.Count - 1; i >= 0; i--)
      {
        TextMeshProUGUI texto = textosCargaCampania[i];
        if (texto == null || texto.gameObject == null)
        {
          textosCargaCampania.RemoveAt(i);
          continue;
        }

        texto.text = textoAnimado;
      }

      if (textosCargaCampania.Count == 0)
      {
        rutinaAnimacionTextoCargaCampania = null;
        yield break;
      }

      cantidadPuntos++;
      if (cantidadPuntos > 3)
      {
        cantidadPuntos = 1;
      }

      yield return espera;
    }
  }

  public float ObtenerMultiplicadorEscalaNodos()
  {
    return 1f + (escalaNodos / 100f);
  }

  public bool DebeForzarMapaLinealTutorial()
  {
    return debugForzarMapaLinealTutorialAlIniciar || HayTutorialNuevoEnEscena();
  }

  public bool DebeForzarPrimerCombateTutorial()
  {
    return DebeUsarConfiguracionTutorial()
      && scMapaManager != null
      && scTutorialManager != null
      && (scMapaManager.nodoActual == scTutorialManager.NodoPelea1
        || scMapaManager.nodoActual == scTutorialManager.Nodotut2);
  }

  public bool DebeUsarConfiguracionTutorial()
  {
    return debugForzarMapaLinealTutorialAlIniciar
      || (scTutorialManager != null && scTutorialManager.tutorialActivo)
      || HayTutorialNuevoEnEscena();
  }

  public bool IntroCampaniaActivaOPendiente => introCampaniaPendiente || introCampaniaActiva;

  public bool IntroCampaniaPuedeIniciarTrasCarga()
  {
    return !DebeEsperarFaderInicialIntroCampania();
  }

  public bool ConsumirIntroCampaniaPendiente()
  {
    if (!introCampaniaPendiente)
    {
      return false;
    }

    introCampaniaPendiente = false;
    introCampaniaActiva = true;
    OcultarInterfazCampaniaParaIntro();
    return true;
  }

  public void FinalizarIntroCampania()
  {
    bool habiaIntro = IntroCampaniaActivaOPendiente;
    introCampaniaPendiente = false;
    introCampaniaActiva = false;

    if (habiaIntro)
    {
      RestaurarInterfazCampaniaTrasIntro();
      LiberarFaderIntroCampaniaSiCorresponde(true);
    }

    EjecutarAccionesAlFinalizarIntroCampania();
    TutorialDirector.ReintentarAutoarranqueTrasIntroSiCorresponde();
    CompletarInicioCampaniaSiCorresponde();
  }

  public void EjecutarTrasIntroCampania(System.Action accion)
  {
    if (accion == null)
    {
      return;
    }

    if (IntroCampaniaActivaOPendiente)
    {
      accionesAlFinalizarIntroCampania.Add(accion);
      return;
    }

    accion.Invoke();
  }

  public void SolicitarInicioIntroCampaniaTrasCarga(bool ignorarFaderInicial = false)
  {
    if (!IntroCampaniaActivaOPendiente)
    {
      return;
    }

    if (rutinaIntroCampaniaTrasCarga != null)
    {
      if (!ignorarFaderInicial)
      {
        return;
      }

      StopCoroutine(rutinaIntroCampaniaTrasCarga);
      rutinaIntroCampaniaTrasCarga = null;
    }

    rutinaIntroCampaniaTrasCarga = StartCoroutine(IniciarIntroCampaniaTrasCarga(ignorarFaderInicial));
  }

  private bool HayTutorialNuevoEnEscena()
  {
    return TutorialDirector.HayTutorialActivoOPendiente();
  }

  private string ObtenerTextoCargaInicialSegunIdioma()
  {
    if (TRADU.i != null)
    {
      return TRADU.i.Traducir(TextoCargaCampania);
    }

    int idiomaGuardado = PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaIngles);
    switch (idiomaGuardado)
    {
      case TRADU.IdiomaEspanol:
        return TextoCargaCampania;
      case TRADU.IdiomaPortugues:
        return "Carregando...";
      default:
        return "Loading...";
    }
  }

  private void AsegurarAsentamientoManager()
  {
    if (asentamientoManager == null)
    {
      asentamientoManager = GetComponent<AsentamientoManager>();
    }

    if (asentamientoManager == null)
    {
      asentamientoManager = BuscarAsentamientoManagerEnEscena();
    }

    if (asentamientoManager == null)
    {
      asentamientoManager = gameObject.AddComponent<AsentamientoManager>();
    }
  }

  private AsentamientoManager BuscarAsentamientoManagerEnEscena()
  {
    AsentamientoManager encontradoEnEscena = null;
    AsentamientoManager[] managers = Resources.FindObjectsOfTypeAll<AsentamientoManager>();
    for (int i = 0; i < managers.Length; i++)
    {
      AsentamientoManager candidato = managers[i];
      if (candidato == null || !candidato.gameObject.scene.IsValid())
      {
        continue;
      }

      if (candidato.gameObject.name == "UIAsentamiento")
      {
        return candidato;
      }

      if (encontradoEnEscena == null)
      {
        encontradoEnEscena = candidato;
      }
    }

    return encontradoEnEscena;
  }

  public AsentamientoManager ObtenerAsentamientoManager()
  {
    AsegurarAsentamientoManager();
    return asentamientoManager;
  }

  public void InicializarNuevaCampania()
  {
    if (campaniaInicializada)
    {
      return;
    }

    inicializandoNuevaCampania = true;
    try
    {
      TutorialTooltipProgress.ResetearParaNuevaCampania();
      ResetearEstadoTransitorioCampania();
      ConfigurarEstadoTutorialNuevaCampania();
      PrepararIntroCampaniaNueva();
      InicializarRecursosNuevaCampania();
      InicializarZonaNuevaCampania();
      InicializarSequitosNuevaCampania();
      InicializarProgresoNuevaCampania();
      AplicarPresagioEsperanzaInicial();
      InicializarClimaAlIniciar();
      InicializarPersonajesNuevaCampania();
      AplicarTraitsInicioNuevaZona();
      AplicarPresagiosPersonajesAlComenzar();
      EncolarResumenesPresagiosInicio();
      AjustarDificultad();

      debeEscribirLogInicioEnStart = true;
      campaniaInicializada = true;
    }
    finally
    {
      inicializandoNuevaCampania = false;
    }
  }

  private bool CargarCampaniaPendiente(SaveFileData savePendiente, out string error, bool iniciarNuevaCampaniaSiFalla)
  {
    error = string.Empty;

    if (savePendiente == null)
    {
      TutorialDirector.CancelarRestauracionPendiente();
      error = "El save pendiente es invalido.";
      Debug.LogWarning("[CampaignManager] " + error);
      if (iniciarNuevaCampaniaSiFalla)
      {
        InicializarNuevaCampania();
      }
      return false;
    }
    if (savePendiente.version < SaveFileData.MinimumCompatibleVersion)
    {
      error = "Esta partida usa un formato anterior a v26 y no puede cargarse. Inicia una campaña nueva.";
      Debug.LogWarning("[CampaignManager] " + error);
      if (iniciarNuevaCampaniaSiFalla)
      {
        InicializarNuevaCampania();
      }
      return false;
    }
    if (savePendiente.version > SaveFileData.CurrentVersion)
    {
      error = "El save pertenece a una version posterior incompatible.";
      Debug.LogWarning("[CampaignManager] " + error);
      if (iniciarNuevaCampaniaSiFalla)
      {
        InicializarNuevaCampania();
      }
      return false;
    }

    try
    {
      if (savePendiente.campaign == null || savePendiente.map == null || savePendiente.party == null || savePendiente.sequitos == null)
      {
        throw new InvalidOperationException("El save no contiene todos los bloques necesarios.");
      }

      TutorialDirector.PrepararRestauracionDesdeSave(savePendiente.campaign, savePendiente.version);

      if (scMapaManager != null)
      {
        scMapaManager.OmitirAutoGeneracionEnStart();
      }

      ResetearEstadoTransitorioCampania();
      LimpiarEstadoActualParaCarga();
      RestaurarMetaprogresionDesdeSave(savePendiente);
      RestaurarTutorialDesdeSave(savePendiente.campaign);
      AplicarEstadoBaseCampaniaDesdeSave(savePendiente.campaign);
      RestaurarZonaDesdeSave(savePendiente);
      RestaurarMapaDesdeSave(savePendiente);
      RestaurarSequitosDesdeSave(savePendiente.sequitos);
      RestaurarPartyDesdeSave(savePendiente.party, savePendiente.version);
      RestaurarRecursosDesdeSave(savePendiente.campaign);
      RestaurarSeleccionBatallaDesdeSave(savePendiente.party);
      FinalizarCargaCampania(savePendiente);
      TutorialDirector.AplicarRestauracionPendienteSiCorresponde();

      debeEscribirLogInicioEnStart = false;
      campaniaInicializada = true;
      return true;
    }
    catch (Exception ex)
    {
      TutorialDirector.CancelarRestauracionPendiente();
      error = ex.Message;
      Debug.LogError("[CampaignManager] Fallo la carga del save. " + error);
      Debug.LogException(ex, this);
      if (iniciarNuevaCampaniaSiFalla)
      {
        InicializarNuevaCampania();
      }
      return false;
    }
  }

  private void ResetearEstadoTransitorioCampania()
  {
    logInicioCampaniaEscrito = false;
    debeEscribirLogInicioEnStart = false;
    introCampaniaPendiente = false;
    introCampaniaActiva = false;
    RestaurarInterfazCampaniaTrasIntro();
    accionesAlFinalizarIntroCampania.Clear();
    logsPresagiosInicioPendientes.Clear();
    logsPresagiosInicioSolicitados = false;
    if (rutinaLogsPresagiosInicio != null)
    {
      StopCoroutine(rutinaLogsPresagiosInicio);
      rutinaLogsPresagiosInicio = null;
    }
    eventoInicioCampaniaEmitido = false;
    MoviendoCaravana = false;
    transicionZonaEnCurso = false;
    bloquearOlaDeCalorEnSiguienteTiradaClima = false;
    nodoDestinoActual = null;
    BATALLA_EnCurso = 0;
    EMBOSCADA_EnCurso = 0;
    emboscadaViajeCalculada = false;
    logEmboscadaViajePendiente = null;
    horasViajeActual = 0f;
    viajeActualIncluyoNoche = false;
    viajeClimaInicial = 0;
    multiplicadorVelocidadVisualViajeActual = 1f;
    pausandoTextoDistanciaAliento = false;
    resolviendoJefeZona = false;
    abriendoCiudadPuerto = false;
  }

  private void ConfigurarEstadoTutorialNuevaCampania()
  {
    int yaPasotuto = PlayerPrefs.GetInt("Tutorial_Terminado");

    if (debugSaltarTutorialAlIniciar || debugForzarMapaLinealTutorialAlIniciar || HayTutorialNuevoEnEscena())
    {
      yaPasotuto = 1;
    }

    scTutorialManager.tutorialActivo = yaPasotuto != 1;
  }

  private void PrepararIntroCampaniaNueva()
  {
    introCampaniaPendiente = true;
  }

  public void PrepararIntroCampaniaNuevaZona()
  {
    if (introCampaniaActiva)
    {
      return;
    }

    introCampaniaPendiente = true;
  }

  private void InicializarRecursosNuevaCampania()
  {
    CambiarCivilesActuales(110);
    CambiarEsperanzaActual(60);
    CambiarSuministrosActuales(300);
    CambiarMaterialesActuales(50);
    CambiarBueyesActuales(22);

    if (scTutorialManager.tutorialActivo)
    {
      CambiarMaterialesActuales(-10);
      CambiarBueyesActuales(-4);
      CambiarSuministrosActuales(-100);
      CambiarCivilesActuales(-25);
    }

    CambiarOroActual(400);
    CambiarValorAlientoNegroHoras(DebeUsarConfiguracionTutorial() ? 10f : 5f);
  }

  private void InicializarZonaNuevaCampania()
  {
    int zonaInicial = PrePartidaManager.ConsumirZonaInicialPendiente();
    if (zonaInicial <= 0)
    {
      zonaInicial = ObtenerZonaInicialDebug();
    }

    if (zonaInicial == 0 && DebeUsarConfiguracionTutorial())
    {
      zonaInicial = 1;
    }

    InicializarPresagiosNuevaCampania(zonaInicial);
    scAtributosZona.GenerarZona(zonaInicial);
    RegistrarZonaVisitada(scAtributosZona != null ? scAtributosZona.ID : zonaInicial);
    if (scAtributosZona != null)
    {
      RuntimeAnalytics.TrackProgressionStart(
        "zone",
        RuntimeAnalytics.ZoneToken(scAtributosZona.ID),
        RuntimeAnalytics.PhaseToken(scAtributosZona.FASE));
    }
    AplicarPresagioAlientoNegroInicial();
  }

  private void RegistrarZonaVisitada(int zonaId)
  {
    if (zonaId <= 0 || MetaprogresionManager.Instance == null)
    {
      return;
    }

    MetaprogresionManager.Instance.MarcarZonaVisitada(zonaId);
  }

  private bool MecanicasZonaConocidas(int zonaId)
  {
    if (zonaId <= 0)
    {
      return false;
    }

    MetaprogresionManager meta = MetaprogresionManager.Instance;
    return meta == null || meta.ZonaVisitada(zonaId);
  }

  private void InicializarPresagiosNuevaCampania(int regionId)
  {
    presagiosActivosRegion.Clear();
    presagiosRegionActivaId = Mathf.Max(0, regionId);
    primeraBatallaPresagioEnemigosConsumida = false;

    if (PrePartidaManager.TryConsumirPresagiosInicialesPendientes(out List<int> presagios))
    {
      presagiosActivosRegion.AddRange(presagios);
    }
  }

  public bool TienePresagioActivo(int presagioId)
  {
    return presagioId > 0
      && scAtributosZona != null
      && scAtributosZona.ID == presagiosRegionActivaId
      && presagiosActivosRegion.Contains(presagioId);
  }

  public bool DebeGarantizarPrimeraBatallaPresagioEnemigos()
  {
    return !primeraBatallaPresagioEnemigosConsumida
      && (TienePresagioActivo(PresagioCatalog.LeyDelMasFuerte)
        || TienePresagioActivo(PresagioCatalog.CorrompidosAlAcecho)
        || TienePresagioActivo(PresagioCatalog.VenganadoresCazando)
        || TienePresagioActivo(PresagioCatalog.CentinelasLocales));
  }

  public void RegistrarInicioBatallaPresagioEnemigos()
  {
    if (DebeGarantizarPrimeraBatallaPresagioEnemigos())
    {
      primeraBatallaPresagioEnemigosConsumida = true;
    }
  }

  private void AplicarPresagioEsperanzaInicial()
  {
    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    if (TienePresagioActivo(PresagioCatalog.SensacionPositiva))
    {
      CambiarEsperanzaActual(15);
      EncolarLogPresagioInicio(idioma switch
      {
        TRADU.IdiomaIngles => "-Positive Feeling: the Caravan starts with +15 Hope.",
        TRADU.IdiomaPortugues => "-Sensação Positiva: a Caravana começa com +15 de Esperança.",
        _ => "-Sensación Positiva: la Caravana comienza con +15 Esperanza."
      });
    }
    else if (TienePresagioActivo(PresagioCatalog.SensacionNegativa))
    {
      CambiarEsperanzaActual(-10);
      EncolarLogPresagioInicio(idioma switch
      {
        TRADU.IdiomaIngles => "-Negative Feeling: the Caravan starts with -10 Hope.",
        TRADU.IdiomaPortugues => "-Sensação Negativa: a Caravana começa com -10 de Esperança.",
        _ => "-Sensación Negativa: la Caravana comienza con -10 Esperanza."
      });
    }
  }

  private void AplicarPresagioAlientoNegroInicial()
  {
    if (DebeUsarConfiguracionTutorial())
    {
      return;
    }

    if (TienePresagioActivo(PresagioCatalog.VientoAFavor))
    {
      CambiarValorAlientoNegroHoras(5f);
      int idioma = PresagioCatalog.ObtenerIdiomaActual();
      EncolarLogPresagioInicio(idioma switch
      {
        TRADU.IdiomaIngles => "-Tailwind: the Black Breath starts 5 h farther ahead.",
        TRADU.IdiomaPortugues => "-Vento a Favor: o Hálito Negro começa 5 h mais adiante.",
        _ => "-Viento a favor: el Aliento Negro comienza 5 h más adelante."
      });
    }
    else if (TienePresagioActivo(PresagioCatalog.VientoEnContra))
    {
      CambiarValorAlientoNegroHoras(-10f);
      int idioma = PresagioCatalog.ObtenerIdiomaActual();
      EncolarLogPresagioInicio(idioma switch
      {
        TRADU.IdiomaIngles => "-Headwind: the Black Breath starts 10 h farther behind.",
        TRADU.IdiomaPortugues => "-Vento Contrário: o Hálito Negro começa 10 h mais atrás.",
        _ => "-Viento en contra: el Aliento Negro comienza 10 h más atrás."
      });
    }
  }

  private int AjustarPerdidaPorAlientoNegroPresagios(int perdidaBase)
  {
    if (perdidaBase <= 0)
    {
      return 0;
    }

    float multiplicador = 1f;
    if (TienePresagioActivo(PresagioCatalog.AireLimpio))
    {
      multiplicador = 0.7f;
    }
    else if (TienePresagioActivo(PresagioCatalog.AirePutrido))
    {
      multiplicador = 1.3f;
    }

    return Mathf.Max(1, Mathf.RoundToInt(perdidaBase * multiplicador));
  }

  private string ObtenerTextoLogEfectoAlientoNegroViaje(int tier, int perdidaEsperanza, int perdidaCiviles = 0)
  {
    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    if (tier == 2)
    {
      return idioma switch
      {
        TRADU.IdiomaIngles => $"-The noticeable presence of the Black Breath unsettles the Caravan. -{perdidaEsperanza} Hope",
        TRADU.IdiomaPortugues => $"-A presença perceptível do Hálito Negro provoca incerteza na Caravana. -{perdidaEsperanza} Esperança",
        _ => $"-La presencia notable del Aliento Negro al viajar provoca incertidumbre en la Caravana. -{perdidaEsperanza} Esperanza"
      };
    }

    if (tier == 3)
    {
      return idioma switch
      {
        TRADU.IdiomaIngles => $"-The heavy presence of the Black Breath frightens the Caravan. -{perdidaEsperanza} Hope",
        TRADU.IdiomaPortugues => $"-A forte presença do Hálito Negro provoca medo na Caravana. -{perdidaEsperanza} Esperança",
        _ => $"-La gran presencia del Aliento Negro provoca temor en la Caravana. -{perdidaEsperanza} Esperanza"
      };
    }

    return idioma switch
    {
      TRADU.IdiomaIngles => $"-The Black Breath is fatal to the civilians. -{perdidaEsperanza} Hope, -{perdidaCiviles} Civilians",
      TRADU.IdiomaPortugues => $"-O Hálito Negro é fatal para os Civis. -{perdidaEsperanza} Esperança, -{perdidaCiviles} Civis",
      _ => $"-El Aliento Negro resulta fatal para los Civiles. -{perdidaEsperanza} Esperanza, -{perdidaCiviles} Civiles"
    };
  }

  public void AplicarPresagiosDescanso()
  {
    AplicarPresagioEsperanzaDescanso();
    AplicarPresagioPlagaDescanso();
    AplicarPresagioEspejismosDescanso();
  }

  private void AplicarPresagioEsperanzaDescanso()
  {
    int cambio = 0;
    if (TienePresagioActivo(PresagioCatalog.NochesPacificas))
    {
      cambio = 5;
    }
    else if (TienePresagioActivo(PresagioCatalog.NochesTurbulentas))
    {
      cambio = -10;
    }

    if (cambio == 0)
    {
      return;
    }

    CambiarEsperanzaActual(cambio);
    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    if (cambio > 0)
    {
      EscribirLog(idioma switch
      {
        TRADU.IdiomaIngles => "-A peaceful night renews the Caravan's spirits. +5 Hope.",
        TRADU.IdiomaPortugues => "-Uma noite pacífica renova o ânimo da Caravana. +5 Esperança.",
        _ => "-Una noche pacífica renueva el ánimo de la Caravana. +5 Esperanza."
      });
      return;
    }

    EscribirLog(idioma switch
    {
      TRADU.IdiomaIngles => "-A turbulent night unsettles the Caravan. -10 Hope.",
      TRADU.IdiomaPortugues => "-Uma noite turbulenta abala a Caravana. -10 Esperança.",
      _ => "-Una noche turbulenta inquieta a la Caravana. -10 Esperanza."
    });
  }

  private void AplicarPresagioPlagaDescanso()
  {
    if (!TienePresagioActivo(PresagioCatalog.PlagaEnLaRegion)
      || scMenuPersonajes == null
      || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null || personaje.Camp_Muerto || !personaje.FalloTiradaSalvacionFortalezaCampania(10))
      {
        continue;
      }

      int[] duracionesEnfermoHoras = { 72, 96, 120 };
      int horasEnfermo = duracionesEnfermoHoras[UnityEngine.Random.Range(0, duracionesEnfermoHoras.Length)];
      personaje.AplicarEnfermoHoras(horasEnfermo);
      EscribirLog(idioma switch
      {
        TRADU.IdiomaIngles => $"-{personaje.sNombre} fails Fortitude DC 10 and becomes Sick for {horasEnfermo} h.",
        TRADU.IdiomaPortugues => $"-{personaje.sNombre} falha em Fortitude CD 10 e fica Doente por {horasEnfermo} h.",
        _ => $"-{personaje.sNombre} falla Fortaleza DC 10 y obtiene Enfermo por {horasEnfermo} h."
      });
    }
  }

  private void AplicarPresagioEspejismosDescanso()
  {
    if (!TienePresagioActivo(PresagioCatalog.Espejismos)
      || DebeUsarConfiguracionTutorial()
      || scMapaManager == null
      || scMapaManager.AplicarEspejismosEnNodosConocidosAlcanzables() <= 0)
    {
      return;
    }

    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    EscribirLog(idioma switch
    {
      TRADU.IdiomaIngles => "-As the Caravan rests, mirages engulf the roads. Familiar places no longer seem to be what they were.",
      TRADU.IdiomaPortugues => "-Enquanto a Caravana descansa, miragens cobrem os caminhos. Lugares conhecidos já não parecem ser o que eram.",
      _ => "-Mientras la Caravana descansa, espejismos cubren los caminos. Los lugares conocidos ya no parecen ser lo que eran."
    });
  }

  private void AplicarPresagiosPersonajesAlComenzar()
  {
    if (DebeUsarConfiguracionTutorial()
      || scMenuPersonajes == null
      || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    bool corrupcionInsoportable = TienePresagioActivo(PresagioCatalog.CorrupcionInsoportable);
    bool regionBendecida = TienePresagioActivo(PresagioCatalog.RegionBendecida);
    if (!corrupcionInsoportable && !regionBendecida)
    {
      return;
    }

    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    List<string> nombresCorrompidos = corrupcionInsoportable ? new List<string>() : null;
    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null || personaje.Camp_Muerto)
      {
        continue;
      }

      if (corrupcionInsoportable)
      {
        if (!personaje.FalloTiradaSalvacionFortalezaCampania(8))
        {
          continue;
        }

        bool yaEstabaCorrupto = personaje.Camp_Corrupto;
        personaje.Camp_Corrupto = true;
        if (!yaEstabaCorrupto)
        {
          RuntimeAnalytics.TrackCharacterState(personaje, "corrupted", "omen");
        }
        nombresCorrompidos.Add(personaje.sNombre);
        continue;
      }

      int tiradaMental = UnityEngine.Random.Range(1, 21) + ObtenerTSMentalTotalCampania(personaje);
      if (tiradaMental < 13)
      {
        continue;
      }

      personaje.AplicarBendecidoHoras(96f);
      EncolarLogPresagioInicio(idioma switch
      {
        TRADU.IdiomaIngles => $"-{personaje.sNombre} passes Mental DC 13 and becomes Blessed for 96 h.",
        TRADU.IdiomaPortugues => $"-{personaje.sNombre} passa em Mental CD 13 e fica Abençoado por 96 h.",
        _ => $"-{personaje.sNombre} supera Mental DC 13 y obtiene Bendecido por 96 h."
      });
    }

    if (nombresCorrompidos != null && nombresCorrompidos.Count > 0)
    {
      string nombres = UnirNombresLocalizados(nombresCorrompidos, idioma);
      bool singular = nombresCorrompidos.Count == 1;
      EncolarLogPresagioInicio(idioma switch
      {
        TRADU.IdiomaIngles => singular
          ? $"-Facing the region's Unbearable Corruption, {nombres} fails a DC 8 Fortitude check and becomes Corrupted."
          : $"-Facing the region's Unbearable Corruption, {nombres} fail a DC 8 Fortitude check and become Corrupted.",
        TRADU.IdiomaPortugues => singular
          ? $"-Diante da Corrupção Insuportável da região, {nombres} falha em um teste de Fortitude CD 8 e fica Corrompido."
          : $"-Diante da Corrupção Insuportável da região, {nombres} falham em um teste de Fortitude CD 8 e ficam Corrompidos.",
        _ => singular
          ? $"-Ante la Corrupción Insoportable de la región, {nombres} falla una prueba de Fortaleza DC 8 y queda Corrupto."
          : $"-Ante la Corrupción Insoportable de la región, {nombres} fallan una prueba de Fortaleza DC 8 y quedan Corruptos."
      });
    }

  }

  private void EncolarResumenesPresagiosInicio()
  {
    for (int i = 0; i < presagiosActivosRegion.Count; i++)
    {
      int presagioId = presagiosActivosRegion[i];
      if (presagioId == PresagioCatalog.SensacionPositiva
        || presagioId == PresagioCatalog.SensacionNegativa
        || presagioId == PresagioCatalog.VientoAFavor
        || presagioId == PresagioCatalog.VientoEnContra
        || presagioId == PresagioCatalog.CorrupcionInsoportable
        || presagioId == PresagioCatalog.RegionBendecida
        || presagioId == PresagioCatalog.AmenazasVigilantes
        || presagioId == PresagioCatalog.SinVigilancia)
      {
        continue;
      }

      string resumen = PresagioCatalog.ObtenerTextoLocalizado(presagioId);
      if (!string.IsNullOrWhiteSpace(resumen))
      {
        EncolarLogPresagioInicio("-" + resumen);
      }
    }
  }

  private void EncolarLogPresagioInicio(string mensaje)
  {
    if (!string.IsNullOrWhiteSpace(mensaje))
    {
      logsPresagiosInicioPendientes.Add(mensaje);
    }
  }

  public void MostrarLogsPresagiosInicioTrasContinuarDescripcionZona()
  {
    if (logsPresagiosInicioSolicitados || logsPresagiosInicioPendientes.Count == 0)
    {
      return;
    }

    logsPresagiosInicioSolicitados = true;
    if (rutinaLogsPresagiosInicio != null)
    {
      StopCoroutine(rutinaLogsPresagiosInicio);
    }

    rutinaLogsPresagiosInicio = StartCoroutine(EscribirLogsPresagiosInicioDiferidos());
  }

  private IEnumerator EscribirLogsPresagiosInicioDiferidos()
  {
    yield return new WaitForSecondsRealtime(RetrasoLogsPresagiosInicioSegundos);

    List<string> mensajes = new List<string>(logsPresagiosInicioPendientes);
    logsPresagiosInicioPendientes.Clear();
    for (int i = 0; i < mensajes.Count; i++)
    {
      EscribirLogSinBitacora(FormatearLogPresagioInicio(mensajes[i]), true);
    }

    rutinaLogsPresagiosInicio = null;
  }

  private static string FormatearLogPresagioInicio(string mensaje)
  {
    string contenido = string.IsNullOrWhiteSpace(mensaje)
      ? string.Empty
      : mensaje.Trim().TrimStart('-').TrimStart();
    int idioma = PresagioCatalog.ObtenerIdiomaActual();
    string etiqueta = idioma switch
    {
      TRADU.IdiomaIngles => "OMEN",
      TRADU.IdiomaPortugues => "PRESSÁGIO",
      _ => "PRESAGIO"
    };

    return "-<color=" + ColorEtiquetaLogPresagio + "><b>" + etiqueta + ":</b></color> " + contenido;
  }

  private static string UnirNombresLocalizados(List<string> nombres, int idioma)
  {
    if (nombres == null || nombres.Count == 0)
    {
      return string.Empty;
    }

    if (nombres.Count == 1)
    {
      return nombres[0];
    }

    string conjuncion = idioma switch
    {
      TRADU.IdiomaIngles => " and ",
      TRADU.IdiomaPortugues => " e ",
      _ => " y "
    };
    return string.Join(", ", nombres.GetRange(0, nombres.Count - 1))
      + conjuncion
      + nombres[nombres.Count - 1];
  }

  public float AplicarMultiplicadorExperienciaPresagios(float experienciaBase)
  {
    if (TienePresagioActivo(PresagioCatalog.AventuraMemorable))
    {
      return experienciaBase * 1.15f;
    }

    if (TienePresagioActivo(PresagioCatalog.AventuraOlvidable))
    {
      return experienciaBase * 0.85f;
    }

    return experienciaBase;
  }

  public float ObtenerBonusCuracionPasivaViajePresagios()
  {
    return TienePresagioActivo(PresagioCatalog.PlantasCurativas) ? 0.1f : 0f;
  }

  public int AjustarChanceEmboscadaEnemigaPresagios(int chanceBase)
  {
    if (!DebeUsarConfiguracionTutorial() && TienePresagioActivo(PresagioCatalog.CaminosPeligrosos))
    {
      chanceBase += 10;
    }

    return Mathf.Clamp(chanceBase, 0, 100);
  }

  public int AjustarChanceEmboscadaAliadaPresagios(int chanceBase)
  {
    if (!DebeUsarConfiguracionTutorial() && TienePresagioActivo(PresagioCatalog.EnemigosDesprevenidos))
    {
      chanceBase += 10;
    }

    return Mathf.Clamp(chanceBase, 0, 100);
  }

  public int ObtenerAumentoAlertaRegionalAlComenzarViaje()
  {
    if (!MecanicaAlertaRegionHabilitada)
    {
      return 0;
    }

    if (TienePresagioActivo(PresagioCatalog.SinVigilancia))
    {
      return 0;
    }

    return TienePresagioActivo(PresagioCatalog.AmenazasVigilantes) ? 2 : 1;
  }

  public bool EstaMecanicaAlertaRegionHabilitada()
  {
    return MecanicaAlertaRegionHabilitada;
  }

  public List<int> ObtenerPresagiosActivosRegion()
  {
    return new List<int>(presagiosActivosRegion);
  }

  public int ObtenerRegionPresagiosActivos()
  {
    return presagiosRegionActivaId;
  }

  public float ObtenerMultiplicadorBifurcacionesPresagios()
  {
    if (TienePresagioActivo(PresagioCatalog.RutasQuebradas))
    {
      return 0.8f;
    }

    if (TienePresagioActivo(PresagioCatalog.RutasAbiertas))
    {
      return 1.2f;
    }

    return 1f;
  }

  public int AjustarChanceAtajoSubterraneoPresagios(int chanceBase)
  {
    if (TienePresagioActivo(PresagioCatalog.Derrumbado))
    {
      return 0;
    }

    if (TienePresagioActivo(PresagioCatalog.Subsuelo))
    {
      return Mathf.Clamp(chanceBase + 15, 0, 100);
    }

    return Mathf.Clamp(chanceBase, 0, 100);
  }

  public int AjustarChanceCaminoSinuosoPresagios(int chanceBase)
  {
    if (TienePresagioActivo(PresagioCatalog.CaminosIntrincados))
    {
      return Mathf.Clamp(chanceBase + 15, 0, 100);
    }

    if (TienePresagioActivo(PresagioCatalog.CaminosCuidados))
    {
      return Mathf.Clamp(Mathf.RoundToInt(chanceBase * 0.5f), 0, 100);
    }

    return Mathf.Clamp(chanceBase, 0, 100);
  }

  public int AjustarChanceAtajoSuperficiePresagios(int chanceBase)
  {
    if (TienePresagioActivo(PresagioCatalog.CaminosBorrados))
    {
      return 0;
    }

    if (TienePresagioActivo(PresagioCatalog.ViejosSenderos))
    {
      return Mathf.Clamp(chanceBase + 15, 0, 100);
    }

    return Mathf.Clamp(chanceBase, 0, 100);
  }

  public float AplicarRecoleccionMaterialesPresagios(float cantidadBase)
  {
    float multiplicador = 1f;
    if (TienePresagioActivo(PresagioCatalog.MaterialesAbundantes))
    {
      multiplicador = 1.15f;
    }
    else if (TienePresagioActivo(PresagioCatalog.MaterialesEscasos))
    {
      multiplicador = 0.85f;
    }

    return Mathf.Max(0f, cantidadBase * multiplicador);
  }

  public float AplicarRecoleccionSuministrosPresagios(float cantidadBase)
  {
    float multiplicador = 1f;
    if (TienePresagioActivo(PresagioCatalog.PresasFaciles))
    {
      multiplicador = 1.15f;
    }
    else if (TienePresagioActivo(PresagioCatalog.FaunaReducida))
    {
      multiplicador = 0.85f;
    }

    return Mathf.Max(0f, cantidadBase * multiplicador);
  }

  public int ObtenerCostoCompraConPresagios(float costoBase)
  {
    if (costoBase <= 0f)
    {
      return 0;
    }

    if (TienePresagioActivo(PresagioCatalog.ComercioActivo))
    {
      return Mathf.Max(1, Mathf.RoundToInt(costoBase * 0.9f));
    }

    if (TienePresagioActivo(PresagioCatalog.ComercioMenguado))
    {
      return Mathf.Max(1, Mathf.RoundToInt(costoBase * 1.1f));
    }

    return Mathf.Max(1, (int)costoBase);
  }

  public int ObtenerCostoPuestoComercialConPresagios(float costoBase)
  {
    int costo = ObtenerCostoCompraConPresagios(costoBase);
    if (TienePresagioActivo(PresagioCatalog.PobladosVividos))
    {
      costo = Mathf.Max(1, Mathf.RoundToInt(costo * 0.85f));
    }

    return costo;
  }

  public int ObtenerCostoServicioAsentamientoConPresagios(int costoBase)
  {
    if (costoBase <= 0)
    {
      return 0;
    }

    return TienePresagioActivo(PresagioCatalog.PobladosVividos)
      ? Mathf.Max(1, Mathf.RoundToInt(costoBase * 0.85f))
      : costoBase;
  }

  public int ObtenerMaxAccionesAsentamiento()
  {
    return TienePresagioActivo(PresagioCatalog.PobladosEscasos) ? 2 : 3;
  }

  public bool DebeEliminarAltaresPorPresagio()
  {
    return TienePresagioActivo(PresagioCatalog.TierraProfana);
  }

  public bool DebePrepararTiposNodoPorPresagios()
  {
    return TienePresagioActivo(PresagioCatalog.SenalesClaras)
      || TienePresagioActivo(PresagioCatalog.SenalesConfusas)
      || TienePresagioActivo(PresagioCatalog.PilasDeRecursos)
      || TienePresagioActivo(PresagioCatalog.RecursosEscondidos)
      || TienePresagioActivo(PresagioCatalog.RumoresComeciales)
      || TienePresagioActivo(PresagioCatalog.SenalesSagradas)
      || TienePresagioActivo(PresagioCatalog.TierraProfana)
      || TienePresagioActivo(PresagioCatalog.AdvertenciasDeAmenazas)
      || TienePresagioActivo(PresagioCatalog.AmenazasEscondidas);
  }

  public bool DebeRevelarTipoNodoPorPresagio(int tipoNodo)
  {
    return (tipoNodo == 2 && TienePresagioActivo(PresagioCatalog.SenalesClaras))
      || (tipoNodo == 5 && TienePresagioActivo(PresagioCatalog.PilasDeRecursos))
      || (tipoNodo == 6 && TienePresagioActivo(PresagioCatalog.RumoresComeciales))
      || (tipoNodo == 14 && TienePresagioActivo(PresagioCatalog.SenalesSagradas))
      || (tipoNodo == 8 && TienePresagioActivo(PresagioCatalog.AdvertenciasDeAmenazas));
  }

  public bool DebeForzarTipoNodoMisteriosoPorPresagio(int tipoNodo)
  {
    return (tipoNodo == 2 && TienePresagioActivo(PresagioCatalog.SenalesConfusas))
      || (tipoNodo == 5 && TienePresagioActivo(PresagioCatalog.RecursosEscondidos))
      || (tipoNodo == 8 && TienePresagioActivo(PresagioCatalog.AmenazasEscondidas));
  }

  public float AjustarChanceEventoBuenoPresagios(float chanceBase)
  {
    if (TienePresagioActivo(PresagioCatalog.AuraPositiva))
    {
      chanceBase += 15f;
    }
    else if (TienePresagioActivo(PresagioCatalog.AuraNegativa))
    {
      chanceBase -= 15f;
    }

    return Mathf.Clamp(chanceBase, 0f, 100f);
  }

  private int ObtenerZonaInicialDebug()
  {
    if (EstaActivoDebugForzarAuroraPasoVientoHelado())
    {
      return 2;
    }

    List<int> zonasHabilitadas = new List<int>();

    if (debugPermitirZonaBosque)
    {
      zonasHabilitadas.Add(1);
    }

    if (debugPermitirZonaPasoVientoHelado)
    {
      zonasHabilitadas.Add(2);
    }

    if (debugPermitirZonaNedukazal)
    {
      zonasHabilitadas.Add(3);
    }

    if (zonasHabilitadas.Count == 0)
    {
      return 0;
    }

    return zonasHabilitadas[UnityEngine.Random.Range(0, zonasHabilitadas.Count)];
  }

  private void InicializarSequitosNuevaCampania()
  {
    scMenuSequito.AgregarSequito(1);
    scMenuSequito.AgregarSequito(2);
    scMenuSequito.AgregarSequito(3);
  }

  private void InicializarProgresoNuevaCampania()
  {
    InicializarMejorasCaravanaNuevaCampania();
    horasTotales = HoraInicioCampania;
    antorchasEncendidas = true;
    progresoFatigaHoras = 0f;
    creditoPrevencionAlientoHoras = 0f;
    acumuladorEfectosSequitosHoras = 0f;
    combateHoraCapturada = false;
    combateNocturno = false;
    descansoInterrumpidoPendiente = false;
    descansoResultadosPendientes = false;
    descansoTuvoEmboscada = false;
    descansoRitualElegible = false;
    descansoRitualPersonajeId = string.Empty;
    descansoClimaInicial = 0;
    descansoValorTarea = 0f;
    descansoChanceExploracion = 0;
    descansoChanceEmboscada = 0;
    descansoEmboscadaPendiente = false;
    descansoHorasHastaEmboscada = 0f;
    descansoTiradaEmboscada = 0;
    descansoHorasRestantes = 0f;
    descansoTareaCivil = 0;
    descansoEnClaro = false;
    descansoHoraCombate = 0f;
    horasViajeActual = 0f;
    viajeClimaInicial = 0;
    posicionCaravana = 1;
    ResetearEstadisticasCampania();
    if (estadosCaravana == null)
    {
      estadosCaravana = new EstadosCaravana();
    }
    else
    {
      estadosCaravana.RestaurarDesdeSave(null);
    }

    if (debugIniciarConEstadosCaravana)
    {
      AgregarEstadosCaravanaDebugIniciales();
    }
  }

  private void InicializarMejorasCaravanaNuevaCampania()
  {
    mejoraCaravanaAntorchas = MinTierMejoraCaravana;
    mejoraCaravanaAlforjas = MinTierMejoraCaravana;
    mejoraCaravanaTiendas = MinTierMejoraCaravana;
    mejoraCaravanaCatalejos = MinTierMejoraCaravana;
    mejoraCaravanaAlmacen = MinTierMejoraCaravana;
    mejoraCaravanaDefensas = MinTierMejoraCaravana;
    sequitoCuranderosMejoraCuracion = 0.10f;
  }

  private void ResetearEstadisticasCampania()
  {
    estadisticaHorasViajadas = 0f;
    estadisticaBatallasLibradas = 0;
    estadisticaCivilesPerdidos = 0;
    estadisticaAsentamientosVisitados = 0;
  }

  private int NormalizarTierMejoraCaravana(int tier)
  {
    return Mathf.Clamp(tier, MinTierMejoraCaravana, MaxTierMejoraCaravana);
  }

  private void AgregarEstadosCaravanaDebugIniciales()
  {
    AgregarEstadoCaravana(TipoEstadoCaravana.Inspiracion, 3);
    AgregarEstadoCaravana(TipoEstadoCaravana.Presteza, 3);
    AgregarEstadoCaravana(TipoEstadoCaravana.Compromiso, 3);
    AgregarEstadoCaravana(TipoEstadoCaravana.Vigilante, 3);
    AgregarEstadoCaravana(TipoEstadoCaravana.Acobardados, 3);
    AgregarEstadoCaravana(TipoEstadoCaravana.Aletargados, 3);
    AgregarEstadoCaravana(TipoEstadoCaravana.Desmotivacion, 3);
    AgregarEstadoCaravana(TipoEstadoCaravana.Descuidados, 3);
  }
 
  private void InicializarPersonajesNuevaCampania()
  {
    AgregarHeroesDebugIniciales();

    if (!DebeUsarConfiguracionTutorial())
    {
      int claseLider = PrePartidaManager.ConsumirClaseLiderPendiente();
      if (claseLider < PrePartidaManager.ClaseCaballero || claseLider > PrePartidaManager.ClaseDuelista)
      {
        claseLider = UnityEngine.Random.Range(
          PrePartidaManager.ClaseCaballero,
          PrePartidaManager.ClaseDuelista + 1);
      }

      int heroesInicialesAgregados = AgregarLiderInicial(claseLider) != null ? 1 : 0;

      // Si el lider no ocupa uno de estos roles, se mantiene la garantia anterior.
      if (claseLider != 3 && claseLider != 5 && heroesInicialesAgregados < 4)
      {
        int claseApoyo = UnityEngine.Random.value < 0.5f ? 5 : 3;
        if (AgregarHeroe(claseApoyo))
        {
          heroesInicialesAgregados++;
        }
      }

      int intentos = 0;
      while (heroesInicialesAgregados < 4 && intentos < 8)
      {
        if (AgregarHeroe(0))
        {
          heroesInicialesAgregados++;
        }

        intentos++;
      }

      RefrescarRetratosPersonajesCampania();
      return;
    }

    CrearAcechador(true);
    RefrescarRetratosPersonajesCampania();
  }

  private Personaje AgregarLiderInicial(int claseLider)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return null;
    }

    int cantidadAntes = scMenuPersonajes.listaPersonajes.Count;
    creandoLiderInicial = true;
    try
    {
      if (!AgregarHeroe(claseLider) || scMenuPersonajes.listaPersonajes.Count <= cantidadAntes)
      {
        return null;
      }
    }
    finally
    {
      creandoLiderInicial = false;
    }

    Personaje lider = scMenuPersonajes.listaPersonajes[scMenuPersonajes.listaPersonajes.Count - 1];
    OtorgarExperienciaHastaNivel(lider, 2);
    ResolverSubidaNivelLiderInicial(lider);
    return lider;
  }

  private void ResolverSubidaNivelLiderInicial(Personaje lider)
  {
    if (lider == null || lider.fNivelActual < 2f)
    {
      return;
    }

    while (lider.NivelPuntoAtributo > 0)
    {
      bool puedeElegirPoder = lider.IDClase == 3 // Purificadora
        || lider.IDClase == 5; // Canalizador
      int atributoElegido = UnityEngine.Random.Range(0, puedeElegirPoder ? 3 : 2);

      if (atributoElegido == 0)
      {
        lider.iFuerza++;
      }
      else if (atributoElegido == 1)
      {
        lider.iAgi++;
      }
      else
      {
        lider.iPoder++;
      }

      lider.NivelPuntoAtributo--;
    }

    while (lider.NivelPuntoHabilidad > 0)
    {
      List<Habilidad> habilidadesDisponibles = new List<Habilidad>();
      foreach (Habilidad habilidad in lider.GetComponents<Habilidad>())
      {
        if (habilidad == null
          || habilidad.NIVEL != 1
          || habilidad is AtaqueBasico
          || habilidad is RetrasarTurno
          || habilidad.agregaDesdeArmaUI != null)
        {
          continue;
        }

        habilidadesDisponibles.Add(habilidad);
      }

      if (habilidadesDisponibles.Count == 0)
      {
        Debug.LogWarning("[CampaignManager] El líder no tiene una habilidad de clase válida para resolver automáticamente su subida a nivel 2.");
        break;
      }

      Habilidad habilidadElegida = habilidadesDisponibles[UnityEngine.Random.Range(0, habilidadesDisponibles.Count)];
      habilidadElegida.NIVEL = 2;
      lider.NivelPuntoHabilidad--;
    }
  }

  private void AgregarHeroesDebugIniciales()
  {
    if (debugIniciarConCaballeroCompleto)
    {
      AgregarHeroeDebugCompleto(1);
    }

    if (debugIniciarConExploradorCompleto)
    {
      AgregarHeroeDebugCompleto(2);
    }

    if (debugIniciarConPurificadoraCompleta)
    {
      AgregarHeroeDebugCompleto(3);
    }

    if (debugIniciarConAcechadorCompleto)
    {
      AgregarHeroeDebugCompleto(4);
    }

    if (debugIniciarConCanalizadorCompleto)
    {
      AgregarHeroeDebugCompleto(5);
    }

    if (debugIniciarConDuelistaCompleta)
    {
      AgregarHeroeDebugCompleto(6);
    }
  }

  private void AgregarHeroeDebugCompleto(int idClase)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    int cantidadAntes = scMenuPersonajes.listaPersonajes.Count;
    if (!AgregarHeroe(idClase))
    {
      return;
    }

    if (scMenuPersonajes.listaPersonajes.Count <= cantidadAntes)
    {
      return;
    }

    Personaje personaje = scMenuPersonajes.listaPersonajes[scMenuPersonajes.listaPersonajes.Count - 1];
    CompletarHeroeDebugConTodasLasHabilidades(personaje);
    personaje.iIniciativa += 100;
    personaje.iApMax += 20;
    personaje.sinCooldownDebug = true;
  }

  private void RefrescarRetratosPersonajesCampania(bool actualizarInfoSiMenuAbierto = false)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    if (scMenuPersonajes.listaPersonajes.Count > 0)
    {
      Personaje personajeBase = scMenuPersonajes.pSel != null && !scMenuPersonajes.pSel.Camp_Muerto
        ? scMenuPersonajes.pSel
        : scMenuPersonajes.listaPersonajes.Find(p => p != null && !p.Camp_Muerto);

      scMenuPersonajes.pSel = personajeBase != null ? personajeBase : scMenuPersonajes.listaPersonajes[0];
    }
    else
    {
      scMenuPersonajes.pSel = null;
    }

    scMenuPersonajes.ActualizarLista();

    if (actualizarInfoSiMenuAbierto && scMenuPersonajes.gameObject.activeInHierarchy)
    {
      scMenuPersonajes.ActualizarInfo();
    }
  }

  public void RefrescarBarraPersonajesCampania(bool actualizarInfoSiMenuAbierto = true)
  {
    RefrescarRetratosPersonajesCampania(actualizarInfoSiMenuAbierto);
  }


  private void OnEnable()
  {
    AjustesAudio.VolumenSfxCambiado += ActualizarVolumenSfxMovimientoCaravana;
    TryProcesarColaTextoFlotante();
    ActualizarCursorCampania(true);

    if (startCampaniaEjecutado && !IntroCampaniaActivaOPendiente)
    {
      BanterBattleUI.InstalarCampania(this);
    }
  }

  private void OnDisable()
  {
    AjustesAudio.VolumenSfxCambiado -= ActualizarVolumenSfxMovimientoCaravana;
    if (rutinaAnimacionTextoCargaCampania != null)
    {
      StopCoroutine(rutinaAnimacionTextoCargaCampania);
      rutinaAnimacionTextoCargaCampania = null;
    }

    textosCargaCampania.Clear();
    if (rutinaTextoFlotanteCampania != null)
    {
      StopCoroutine(rutinaTextoFlotanteCampania);
      rutinaTextoFlotanteCampania = null;
    }
    if (rutinaTextosRecursos != null)
    {
      StopCoroutine(rutinaTextosRecursos);
      rutinaTextosRecursos = null;
    }

    procesandoCola = false;
    bloqueoTextosFlotantesCampania = 0;
    bloqueoTextosRecursosCampania = 0;
    colaTextos.Clear();
    colaTextosSuspendidosCampania.Clear();
    colaTextosRecursos.Clear();
    colaTextosRecursosSuspendidos.Clear();
    textosRecursosRecientes.Clear();
    LimpiarTextosFlotantesCampaniaActivos();
    tiempoUltimoSpawnTiempoReal = float.NegativeInfinity;
    RestaurarCursorCampaniaPredeterminado();
    BanterBattleUI.FinalizarCampania();
  }
  [Header("Reloj horario")]
  [Tooltip("Texto que muestra Día N · HH:MM.")]
  [SerializeField] private TextMeshProUGUI txtDia;
  [Tooltip("Arrastrar aquí el objeto con el componente Image del círculo (actualmente RelojRota).")]
  [SerializeField] private Image relojHoraFill;
  private float relojHoraFillObjetivo;
  private Color relojHoraColorObjetivo;
  private bool relojHoraInicializado;

  private void ActualizarTextoDia()
  {
    AsegurarReferenciasRelojHora();

    string textoDia = TRADU.i != null ? TRADU.i.Traducir("Día") : "Día";
    if (txtDia != null)
    {
      txtDia.text = textoDia + " " + ObtenerDiaCalendario() + " · " + FormatearHoraCampania(ObtenerHoraActual());
    }

    float hora = ObtenerHoraActual();
    relojHoraFillObjetivo = Mathf.Clamp01(hora / 24f);
    relojHoraColorObjetivo = ObtenerColorRelojHora(hora);
    if (!relojHoraInicializado && relojHoraFill != null)
    {
      relojHoraFill.fillAmount = relojHoraFillObjetivo;
      relojHoraFill.color = relojHoraColorObjetivo;
      relojHoraInicializado = true;
    }
  }

  private void AsegurarReferenciasRelojHora()
  {
    if (relojHoraFill != null && txtDia != null)
    {
      return;
    }

    GameObject relojHora = GameObject.Find("RelojHora");
    if (relojHora == null)
    {
      relojHora = GameObject.Find("RelojRota");
    }
    if (relojHora == null)
    {
      return;
    }

    if (relojHoraFill == null)
    {
      relojHoraFill = relojHora.GetComponent<Image>();
      if (relojHoraFill == null)
      {
        relojHoraFill = relojHora.GetComponentInChildren<Image>(true);
      }
      if (relojHoraFill != null)
      {
        relojHoraFill.type = Image.Type.Filled;
        relojHoraFill.fillMethod = Image.FillMethod.Radial360;
        relojHoraFill.fillOrigin = (int)Image.Origin360.Top;
        relojHoraFill.fillClockwise = true;
        relojHoraFill.raycastTarget = false;
      }
    }

    if (txtDia == null || txtDia.transform.parent != relojHora.transform.parent)
    {
      Transform textoDiaNuevo = relojHora.transform.parent != null
        ? relojHora.transform.parent.Find("Dia")
        : null;
      if (textoDiaNuevo != null)
      {
        TextMeshProUGUI texto = textoDiaNuevo.GetComponent<TextMeshProUGUI>();
        if (texto != null)
        {
          txtDia = texto;
        }
      }
    }
  }

  private static Color ObtenerColorRelojHora(float hora)
  {
    float h = Mathf.Repeat(hora, 24f);
    Color violeta = new Color(0.30f, 0.20f, 0.43f, 1f);
    Color verde = new Color(0.28f, 0.48f, 0.31f, 1f);
    Color amarillo = new Color(0.62f, 0.52f, 0.24f, 1f);
    Color rojo = new Color(0.55f, 0.25f, 0.20f, 1f);

    if (h < 6f || h >= 21f) return violeta;
    if (h < 8f) return Color.Lerp(violeta, verde, Mathf.SmoothStep(0f, 1f, (h - 6f) / 2f));
    if (h < 12f) return Color.Lerp(verde, amarillo, Mathf.SmoothStep(0f, 1f, (h - 8f) / 4f));
    if (h < 19f) return Color.Lerp(amarillo, rojo, Mathf.SmoothStep(0f, 1f, (h - 12f) / 7f));
    if (h < 21f) return Color.Lerp(rojo, violeta, Mathf.SmoothStep(0f, 1f, (h - 19f) / 2f));
    return violeta;
  }

  private void AnimarRelojHora()
  {
    if (relojHoraFill == null)
    {
      return;
    }

    if (relojHoraFill.fillAmount - relojHoraFillObjetivo > 0.5f)
    {
      relojHoraFill.fillAmount = relojHoraFillObjetivo;
    }
    float suavizado = 1f - Mathf.Exp(-7f * Time.unscaledDeltaTime);
    relojHoraFill.fillAmount = Mathf.Lerp(relojHoraFill.fillAmount, relojHoraFillObjetivo, suavizado);
    relojHoraFill.color = Color.Lerp(relojHoraFill.color, relojHoraColorObjetivo, suavizado);
  }

  public int ObtenerDiaCalendario()
  {
    return Mathf.Max(1, Mathf.FloorToInt((float)(horasTotales / 24d)) + 1);
  }

  public float ObtenerHoraActual()
  {
    return Mathf.Repeat((float)horasTotales, 24f);
  }

  public double ObtenerHorasTotales()
  {
    return Math.Max(0d, horasTotales);
  }

  public bool EsNocheActual()
  {
    return EsHoraNocturna(ObtenerHoraActual());
  }

  public static bool EsHoraNocturna(float hora)
  {
    float horaNormalizada = Mathf.Repeat(hora, 24f);
    return horaNormalizada >= HoraInicioNoche || horaNormalizada < HoraFinNoche;
  }

  public string FormatearHoraCampania(float hora)
  {
    int minutosTotales = Mathf.RoundToInt(Mathf.Repeat(hora, 24f) * 60f) % (24 * 60);
    int horas = minutosTotales / 60;
    int minutos = minutosTotales % 60;
    return horas.ToString("00") + ":" + minutos.ToString("00");
  }

  public string FormatearDuracionHoras(float horas, bool redondearHaciaArriba = false)
  {
    int minutosTotales = redondearHaciaArriba
      ? Mathf.CeilToInt(Mathf.Max(0f, horas)) * 60
      : Mathf.RoundToInt(Mathf.Max(0f, horas) * 60f);
    int dias = minutosTotales / (24 * 60);
    int horasEnteras = minutosTotales / 60 % 24;
    int minutos = minutosTotales % 60;
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    string etiquetaDia = idioma == TRADU.IdiomaIngles
      ? (dias == 1 ? "day" : "days")
      : idioma == TRADU.IdiomaPortugues
        ? (dias == 1 ? "dia" : "dias")
        : (dias == 1 ? "día" : "días");
    if (dias > 0)
    {
      string duracion = dias + " " + etiquetaDia;
      if (horasEnteras > 0)
      {
        duracion += " " + horasEnteras + " h";
      }
      if (minutos > 0 && !redondearHaciaArriba)
      {
        duracion += " " + minutos + " min";
      }
      return duracion;
    }
    if (minutos > 0 && !redondearHaciaArriba)
    {
      return horasEnteras + " h " + minutos + " min";
    }
    return horasEnteras + " h";
  }

  public bool PuedeCambiarAntorchas()
  {
    return !MoviendoCaravana && !HayBatallaPendiente();
  }

  public bool AntorchasEncendidas => antorchasEncendidas;

  public bool SetAntorchasEncendidas(bool encendidas)
  {
    if (antorchasEncendidas == encendidas)
    {
      return true;
    }
    if (!PuedeCambiarAntorchas())
    {
      return false;
    }

    antorchasEncendidas = encendidas;
    EscribirLog(ObtenerTextoLogEstadoAntorchas(encendidas));
    if (scMapaManager != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }
    return true;
  }

  private string ObtenerTextoLogEstadoAntorchas(bool encendidas)
  {
    int idioma = TRADU.i != null
      ? TRADU.i.nIdioma
      : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaIngles);

    return idioma switch
    {
      TRADU.IdiomaPortugues => encendidas
        ? "As tochas foram acesas."
        : "As tochas foram apagadas.",
      TRADU.IdiomaIngles => encendidas
        ? "The torches have been lit."
        : "The torches have been extinguished.",
      _ => encendidas
        ? "Las antorchas han sido prendidas."
        : "Las antorchas han sido apagadas."
    };
  }

  private void AsegurarLuzAntorchasCaravana()
  {
    if (scMapaManager == null || scMapaManager.goCaravana == null)
    {
      return;
    }

    if (lucesAntorchasCaravana == null || lucesAntorchasCaravana.Length == 0)
    {
      lucesAntorchasCaravana = scMapaManager.goCaravana.GetComponentsInChildren<CaravanTorchLight>(true);
    }

    for (int i = 0; i < lucesAntorchasCaravana.Length; i++)
    {
      CaravanTorchLight luzAntorcha = lucesAntorchasCaravana[i];
      if (luzAntorcha != null)
      {
        luzAntorcha.ActualizarEstado(this, scMapaManager);
      }
    }
  }

  public void AvanzarTiempoCampania(
    float horas,
    TipoAvanceTiempoCampania tipo,
    float multiplicadorAliento = 1f,
    float multiplicadorCuracion = 1f,
    bool progresarActividades = true)
  {
    if (horas <= 0f)
    {
      return;
    }

    float horasRestantes = horas;
    while (horasRestantes > 0.0001f)
    {
      double siguienteAmanecer = ObtenerSiguienteHoraAbsoluta(horasTotales, HoraFinNoche);
      float horasHastaAmanecer = Mathf.Max(0f, (float)(siguienteAmanecer - horasTotales));
      bool alcanzaAmanecer = horasHastaAmanecer <= horasRestantes + 0.0001f;
      float horasTramo = alcanzaAmanecer
        ? Mathf.Min(horasRestantes, horasHastaAmanecer)
        : horasRestantes;

      if (horasTramo > 0.000001f)
      {
        AvanzarTramoTiempoCampania(
          horasTramo,
          tipo,
          multiplicadorAliento,
          multiplicadorCuracion,
          progresarActividades);
        horasRestantes -= horasTramo;
      }

      if (alcanzaAmanecer)
      {
        horasTotales = siguienteAmanecer;
        ForzarTiradaClima();
        ActualizarTextoDia();
      }
      else
      {
        break;
      }
    }
  }

  private void AvanzarTramoTiempoCampania(
    float horas,
    TipoAvanceTiempoCampania tipo,
    float multiplicadorAliento,
    float multiplicadorCuracion,
    bool progresarActividades)
  {
    int diaAnterior = ObtenerDiaCalendario();
    bool eraNoche = EsNocheActual();
    horasTotales = Math.Max(0d, horasTotales + horas);

    bool esNoche = EsNocheActual();
    if (eraNoche != esNoche && scMapaManager != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }

    if (sunController != null)
    {
      sunController.SetCampaignHour(ObtenerHoraActual());
    }

    if (multiplicadorAliento > 0f)
    {
      float avanceAliento = horas * multiplicadorAliento;
      float prevenido = Mathf.Min(avanceAliento, creditoPrevencionAlientoHoras);
      creditoPrevencionAlientoHoras = Mathf.Max(0f, creditoPrevencionAlientoHoras - prevenido);
      avanceAliento -= prevenido;
      if (avanceAliento > 0f)
      {
        CambiarValorAlientoNegroHoras(avanceAliento);
      }
    }

    ProcesarHorasPersonajes(horas, tipo, multiplicadorCuracion, progresarActividades);
    ProcesarAcumuladoresGlobales(horas);

    if (numeroTurno != diaAnterior && logDeCampania != null)
    {
      logDeCampania.RegistrarInicioDia(
        numeroTurno,
        GetEsperanzaActual(),
        GetOroActuales(),
        GetMaterialesActuales(),
        GetSuministrosActuales(),
        intTipoClima);
    }

    ActualizarTextoDia();
  }

  public IEnumerator TranscurrirAccionCampania(
    float horas,
    TipoAvanceTiempoCampania tipo,
    float multiplicadorAliento = 1f,
    float multiplicadorCuracion = 1f,
    bool progresarActividades = true)
  {
    float restantes = Mathf.Max(0f, horas);
    while (restantes > 0.0001f)
    {
      float delta = Mathf.Min(restantes, Mathf.Max(0f, Time.deltaTime));
      if (delta <= 0f)
      {
        yield return null;
        continue;
      }
      AvanzarTiempoCampania(delta, tipo, multiplicadorAliento, multiplicadorCuracion, progresarActividades);
      restantes -= delta;
      yield return null;
    }
  }

  public void FinalizarAccionTemporal()
  {
    if (scAlientoNegroVFX != null)
    {
      scAlientoNegroVFX.AvanzarAlientoNegro(0);
    }
    creditoPrevencionAlientoHoras = 0f;
    RefrescarBarraPersonajesCampania(true);
  }

  public void PrepararPrevencionAlientoAccion(float horas)
  {
    creditoPrevencionAlientoHoras = Mathf.Max(creditoPrevencionAlientoHoras, Mathf.Max(0f, horas));
  }

  private static double ObtenerSiguienteHoraAbsoluta(double inicio, float horaObjetivo)
  {
    double siguiente = Math.Floor((inicio - horaObjetivo) / 24d + 1d) * 24d + horaObjetivo;
    if (siguiente <= inicio + 0.000001d)
    {
      siguiente += 24d;
    }
    return siguiente;
  }

  private void ProcesarHorasPersonajes(
    float horas,
    TipoAvanceTiempoCampania tipo,
    float multiplicadorCuracion,
    bool progresarActividades)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    bool esPosada = tipo == TipoAvanceTiempoCampania.Posada;
    bool esViaje = tipo == TipoAvanceTiempoCampania.Viaje;

    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null || personaje.Camp_Muerto)
      {
        continue;
      }

      personaje.ProcesarHorasEstadosCampania(horas);
      bool descansa = esPosada || personaje.ActividadSeleccionada == 1 || !personaje.PuedeRealizarActividades();
      float porcentajePorHora = ObtenerPorcentajeCuracionPasivaPorHora(
        personaje,
        descansa,
        esViaje,
        multiplicadorCuracion) / 100f;
      float curacion = personaje.fVidaMaxima
        * porcentajePorHora
        * horas;
      personaje.RecibirCuracion(curacion);

      if (!esPosada && progresarActividades && personaje.PuedeRealizarActividades() && EsActividadPermitidaPorClimaCampania(personaje.ActividadSeleccionada))
      {
        int ciclos = personaje.AgregarHorasActividad(personaje.ActividadSeleccionada, horas);
        for (int i = 0; i < ciclos; i++)
        {
          ResolverCicloActividad(personaje);
        }
      }
    }
  }

  public float ObtenerPorcentajeCuracionPasivaPorHora(
    Personaje personaje,
    bool descansando = true,
    bool esViaje = false,
    float multiplicadorCuracion = 1f)
  {
    float porcentajeBase = descansando ? 0.04f : 0.02f;
    float porcentajeFinal = porcentajeBase
      * ObtenerMultiplicadorMejorasGlobalesCuracion(esViaje)
      * Mathf.Max(0f, multiplicadorCuracion);

    if (personaje != null)
    {
      porcentajeFinal = personaje.AplicarMultiplicadorCuracionCampaniaTraits(porcentajeFinal);
    }

    return Mathf.Max(0f, porcentajeFinal * 100f);
  }

  private float ObtenerMultiplicadorMejorasGlobalesCuracion(bool esViaje)
  {
    float colaboradores = CuantosPersonajesHacenTalActividad(12);
    float bonusHerboristas = scMenuSequito != null && scMenuSequito.TieneSequito(5) && scSequitoHerboristas != null
      ? 0.03f + 0.03f * scSequitoHerboristas.vecesEnClaro
      : 0f;

    return 1f
      + Mathf.Max(0f, sequitoCuranderosMejoraCuracion)
      + Mathf.Max(0f, colaboradores) * 0.05f
      + bonusHerboristas
      + (esViaje ? ObtenerBonusCuracionPasivaViajePresagios() : 0f);
  }

  private void ProcesarAcumuladoresGlobales(float horas)
  {
    sequitoHerrerosMantArmadurasHoras = Mathf.Max(0f, sequitoHerrerosMantArmadurasHoras - horas);
    sequitoHerrerosMantArmasHoras = Mathf.Max(0f, sequitoHerrerosMantArmasHoras - horas);

    acumuladorEfectosSequitosHoras += horas;
    while (acumuladorEfectosSequitosHoras >= 24f)
    {
      acumuladorEfectosSequitosHoras -= 24f;
      EfectosdeSequitos();
    }
  }

  private void Start()
  {
    startCampaniaEjecutado = true;
    sobrecargaAnterior = SeLlevaDemasiadaCarga();
    estadoSobrecargaInicializado = true;
    PrepararBitacoraDiaActualSiCorresponde();

    TRADU.i.ActualizarIdioma();
    logDeCampania?.SetDiaActual(numeroTurno);
    RefrescarRetratosPersonajesCampania();
    StartCoroutine(RefrescarRetratosPersonajesCampaniaDiferido());
    ActualizarTextoDia();
    if (sunController != null)
    {
      sunController.SetCampaignHour(ObtenerHoraActual());
    }

    MenuOpciones.GetComponent<OpcionesCargarPlayerPrefsUI>().AplicarEfectosEnUI();
    RefrescarVfxClimaCalor();

    if (SaveGameService.TryConsumePendingLoadFailure(out string loadFailureMessage))
    {
      NotificarResultadoGuardado("-No se pudo cargar la campania. " + loadFailureMessage, Color.red);
    }

    if (DEBUG_ABRIR_MENU_SERRIA_AL_INICIAR)
    {
      AbrirCiudadPuertoDirectoDebug();
    }

    if (debugForzarCombateFinalBosqueAlIniciar)
    {
      ForzarCombateFinalBosqueDebug();
    }

    if (debugAbrirPantallaEventosAlIniciar)
    {
      AbrirPantallaEventosDebug();
    }

    if (debugForzarCombateJefeGulekGulAlIniciar)
    {
      ForzarCombateJefeGulekGulDebug();
    }

    if (introCampaniaActiva)
    {
      OcultarInterfazCampaniaParaIntro();
    }
    else if (introCampaniaPendiente)
    {
      SolicitarInicioIntroCampaniaTrasCarga();
    }
    else if (!introCampaniaPendiente)
    {
      CompletarInicioCampaniaSiCorresponde();
    }
  }

  IEnumerator IniciarIntroCampaniaTrasCarga(bool ignorarFaderInicial = false)
  {
    yield return null;
    yield return new WaitForEndOfFrame();
    bool faderCubriendoIntro = AsegurarFaderNegroIntroCampania("inicio intro tras carga");
    if (!ignorarFaderInicial && !faderCubriendoIntro)
    {
      yield return EsperarFaderInicialIntroCampania();
    }

    bool ignorarEsperaFaderMapa = ignorarFaderInicial || faderCubriendoIntro || DebeEsperarFaderInicialIntroCampania();
    if (ignorarFaderInicial && !faderCubriendoIntro)
    {
      ForzarPantallaVisibleParaIntroCampania(ignorarFaderInicial ? "inicio forzado" : "timeout fader inicial");
      yield return null;
      yield return new WaitForEndOfFrame();
    }

    if (!IntroCampaniaActivaOPendiente)
    {
      rutinaIntroCampaniaTrasCarga = null;
      yield break;
    }

    MapaManager mapaManager = scMapaManager != null ? scMapaManager : BuscarMapaManagerEnEscena();
    if (mapaManager != null)
    {
      mapaManager.IniciarIntroCampaniaPendienteTrasCarga(ignorarEsperaFaderMapa);
      rutinaIntroCampaniaTrasCarga = null;
      yield break;
    }

    rutinaIntroCampaniaTrasCarga = null;
    LiberarFaderIntroCampaniaSiCorresponde(true);
    FinalizarIntroCampania();
  }

  IEnumerator EsperarFaderInicialIntroCampania()
  {
    float tiempoEsperando = 0f;
    while (DebeEsperarFaderInicialIntroCampania() && tiempoEsperando < TiempoMaximoEsperaFaderIntroCampania)
    {
      tiempoEsperando += Time.unscaledDeltaTime;
      yield return null;
    }

    yield return null;
  }

  bool DebeEsperarFaderInicialIntroCampania()
  {
    CanvasGroup fader = scAdministradorEscenas != null ? scAdministradorEscenas.fader : null;
    return fader != null && fader.gameObject.activeInHierarchy && fader.alpha > AlphaFaderListoIntroCampania;
  }

  void ForzarPantallaVisibleParaIntroCampania(string motivo)
  {
    CanvasGroup fader = scAdministradorEscenas != null ? scAdministradorEscenas.fader : null;
    if (fader == null)
    {
      return;
    }

    bool estabaTapando = fader.gameObject.activeInHierarchy && (fader.alpha > AlphaFaderListoIntroCampania || fader.blocksRaycasts || fader.interactable);
    scAdministradorEscenas.SetFaderHold(false);
    fader.alpha = 0f;
    fader.blocksRaycasts = false;
    fader.interactable = false;
  }

  bool AsegurarFaderNegroIntroCampania(string motivo)
  {
    if (scAdministradorEscenas == null || scAdministradorEscenas.fader == null)
    {
      return false;
    }

    scAdministradorEscenas.SetFaderHold(true);
    faderIntroCampaniaTomado = true;
    return true;
  }

  void LiberarFaderIntroCampaniaSiCorresponde(bool forzarTransparente)
  {
    if (!faderIntroCampaniaTomado || scAdministradorEscenas == null || scAdministradorEscenas.fader == null)
    {
      return;
    }

    faderIntroCampaniaTomado = false;
    scAdministradorEscenas.SetFaderHold(false);
    if (forzarTransparente)
    {
      scAdministradorEscenas.fader.alpha = 0f;
      scAdministradorEscenas.fader.blocksRaycasts = false;
      scAdministradorEscenas.fader.interactable = false;
    }
  }

  MapaManager BuscarMapaManagerEnEscena()
  {
    MapaManager[] mapas = Resources.FindObjectsOfTypeAll<MapaManager>();
    for (int i = 0; i < mapas.Length; i++)
    {
      if (mapas[i] != null && mapas[i].gameObject.scene.IsValid())
      {
        return mapas[i];
      }
    }

    return null;
  }

  private IEnumerator RefrescarRetratosPersonajesCampaniaDiferido()
  {
    yield return null;
    RefrescarRetratosPersonajesCampania();
  }

  [ContextMenu("Debug/Forzar Combate Final Bosque")]
  public void ForzarCombateFinalBosqueDebug()
  {
    if (scTutorialManager != null)
    {
      scTutorialManager.tutorialActivo = false;
    }

    if (scAtributosZona != null)
    {
      scAtributosZona.GenerarZona(IdsZonaCampania.BosqueAngustiante);
      scAtributosZona.FASE = 1;
    }

    if (goMenuBatallas != null)
    {
      goMenuBatallas.SetActive(true);
    }

    if (scMenuBatallas != null)
    {
      scMenuBatallas.EventoBatallaFinal(11);
    }
  }

  [ContextMenu("Debug/Forzar Combate Jefe Gulek-Gul")]
  public void ForzarCombateJefeGulekGulDebug()
  {
    if (scTutorialManager != null)
    {
      scTutorialManager.tutorialActivo = false;
    }

    if (scAtributosZona != null)
    {
      scAtributosZona.GenerarZona(IdsZonaCampania.PasoVientoHelado);
      scAtributosZona.FASE = 1;
    }

    if (goMenuBatallas != null)
    {
      goMenuBatallas.SetActive(true);
    }

    if (scMenuBatallas != null)
    {
      scMenuBatallas.EventoBatallaFinal(60);
    }
  }

  [ContextMenu("Guardar Campania En Slot Default")]
  private void GuardarCampaniaDesdeContextMenu()
  {
    if (TryGuardarCampania(out string error))
    {
      Debug.Log("[SaveGame] Campania guardada en slot default.");
      return;
    }

    Debug.LogError("[SaveGame] No se pudo guardar la campania. " + error);
  }

  [ContextMenu("Cargar Campania Desde Slot Default")]
  private void CargarCampaniaDesdeContextMenu()
  {
    if (TryCargarCampaniaDesdeArchivo(out string error))
    {
      Debug.Log("[SaveGame] Campania cargada desde slot default.");
      return;
    }

    Debug.LogError("[SaveGame] No se pudo cargar la campania. " + error);
  }

  public bool PuedeGuardarCampania(out string motivo)
  {
    if (!campaniaInicializada)
    {
      motivo = "La campania aun no termino de inicializar.";
      return false;
    }

    if (scAdministradorEscenas != null && scAdministradorEscenas.escenaActual != 0)
    {
      motivo = "Solo se puede guardar en campania.";
      return false;
    }

    if (enviandoExploradores)
    {
      motivo = "No se puede guardar mientras los exploradores estan fuera.";
      return false;
    }

    if (transicionZonaEnCurso)
    {
      motivo = "No se puede guardar durante una transición de región.";
      return false;
    }

    if (scMapaManager == null || scMapaManager.nodoActual == null)
    {
      motivo = "No hay un nodo actual valido para guardar la campania.";
      return false;
    }

    if (HayInteraccionTransitoriaActiva())
    {
      motivo = "No se puede guardar mientras hay una interaccion de nodo abierta.";
      return false;
    }

    motivo = string.Empty;
    return true;
  }

  private bool HayInteraccionTransitoriaActiva()
  {
    return EstaInteraccionActiva(menuDescanso)
      || EstaInteraccionActiva(goMenuBatallas)
      || EstaInteraccionActiva(UIEvetos)
      || (scMenuCaravana != null && scMenuCaravana.TieneMenuAbierto)
      || (asentamientoManager != null && asentamientoManager.TieneInteraccionActiva)
      || EstaInteraccionActiva(goUIComercioNodo)
      || EstaInteraccionActiva(goUIPersonajeSequito)
      || EstaInteraccionActiva(goUISantuario)
      || EstaInteraccionActiva(goUIVictoriaZona)
      || EstaInteraccionActiva(goMenuPuerto)
      || EstaInteraccionActiva(goDerrota);
  }

  private bool HayInteraccionTransitoriaActivaParaExploradores()
  {
    bool ignorarEventoNodoActual = EsNodoActualDeEvento();
    return EstaInteraccionActiva(menuDescanso)
      || EstaInteraccionActiva(goMenuBatallas)
      || (!ignorarEventoNodoActual && EstaInteraccionActiva(UIEvetos))
      || (scMenuCaravana != null && scMenuCaravana.TieneMenuAbierto)
      || (asentamientoManager != null && asentamientoManager.TieneInteraccionActiva)
      || EstaInteraccionActiva(goUIComercioNodo)
      || EstaInteraccionActiva(goUIPersonajeSequito)
      || EstaInteraccionActiva(goUISantuario)
      || EstaInteraccionActiva(goUIVictoriaZona)
      || EstaInteraccionActiva(goMenuPuerto)
      || EstaInteraccionActiva(goDerrota);
  }

  private bool EsNodoActualDeEvento()
  {
    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    return nodoActual != null && nodoActual.tipoNodo == 2;
  }

  public bool DebeBloquearPaneoCamaraCampania()
  {
    return IntroCampaniaActivaOPendiente
      || MoviendoCaravana
      || HayInteraccionTransitoriaActiva()
      || EstaInteraccionActiva(MenuOpciones);
  }

  private bool EstaInteraccionActiva(GameObject go)
  {
    return go != null && go.activeInHierarchy;
  }

  public int ObtenerDistanciaVisionEfectiva()
  {
    return Mathf.Max(ObtenerDistanciaVisionMinima(), Mathf.FloorToInt(ObtenerAlcanceVisionEnPasos()));
  }

  public int ObtenerDistanciaVisionCalculada()
  {
    return Mathf.Max(
      ObtenerDistanciaVisionMinima(),
      ObtenerDistanciaVisionBase() + ObtenerBonusDistanciaVisionCatalejos());
  }

  public float ObtenerAlcanceVisionEnPasos()
  {
    float alcance;
    if (intTipoClima == 5)
    {
      alcance = AlcanceVisionMinimoPasos * MultiplicadorVisionNiebla;
    }
    else if (EsNocheActual())
    {
      float alcanceNocturno = Mathf.Max(
        AlcanceVisionMinimoPasos,
        ObtenerAlcanceVisionNormalEnPasos() * ObtenerMultiplicadorClimaVision());

      if (!antorchasEncendidas)
      {
        alcance = alcanceNocturno * MultiplicadorAlcanceVisionAntorchas;
      }
      else
      {
        alcance = Mathf.Max(alcanceNocturno, ObtenerAlcanceVisionAntorchasEnPasos());
      }
    }
    else
    {
      float alcanceNormal = ObtenerAlcanceVisionNormalEnPasos();
      alcance = Mathf.Max(
        AlcanceVisionMinimoPasos,
        alcanceNormal * ObtenerMultiplicadorClimaVision());
    }

    return alcance * ObtenerMultiplicadorAlcanceVisionActividades();
  }

  private float ObtenerMultiplicadorAlcanceVisionActividades()
  {
    if (DebeUsarConfiguracionTutorial())
    {
      return 1f;
    }

    float bonoClimaDespejado = intTipoClima == 1 && !EsNocheActual() ? 0.10f : 0f;
    float bonoExploracion = EsNocheActual() ? 0f : CuantosPersonajesHacenTalActividad(9) * 0.10f;
    return 1f + bonoClimaDespejado + bonoExploracion;
  }

  public float ObtenerAlcanceVisionAntorchasEnPasos()
  {
    int tierAntorchas = Mathf.Clamp(mejoraCaravanaAntorchas, MinTierMejoraCaravana, MaxTierMejoraCaravana);
    float alcanceAntorchas = AlcanceVisionCatalejosPorTier[tierAntorchas - MinTierMejoraCaravana]
      * 0.90f
      * MultiplicadorAlcanceVisionAntorchas;
    return Mathf.Max(AlcanceVisionMinimoPasos, alcanceAntorchas * ObtenerMultiplicadorClimaVision());
  }

  public float ObtenerAlcanceVisionNormalEnPasos()
  {
    int tier = Mathf.Clamp(
      mejoraCaravanaCatalejos,
      MinTierMejoraCaravana,
      MaxTierMejoraCaravana);
    return AlcanceVisionCatalejosPorTier[tier - MinTierMejoraCaravana]
      * MultiplicadorAlcanceVisionCatalejos;
  }

  public int ObtenerDistanciaVisionBase()
  {
    return DistanciaVisionBase;
  }

  public int ObtenerDistanciaVisionMinima()
  {
    return DistanciaVisionMinima;
  }

  public int ObtenerBonusDistanciaVisionCatalejos()
  {
    return Mathf.Max(0, mejoraCaravanaCatalejos - 1);
  }

  public int ObtenerPenalizacionClimaVision()
  {
    if (intTipoClima == 5) return 15;
    return 0;
  }

  public float ObtenerMultiplicadorClimaVision()
  {
    return intTipoClima == 5 ? MultiplicadorVisionNiebla : 1f;
  }

  public int ObtenerBonusObjetosPostBatallaAntorchas()
  {
    return 3 + Mathf.Max(0, mejoraCaravanaAntorchas - 1) * 2;
  }

  public int ObtenerChanceExploracionPasiva(int modificadorContextual = 0)
  {
    int chance = 75;
    chance += scAtributosZona != null ? scAtributosZona.modChanceExploracion : 0;
    chance += MetaprogresionManager.Instance != null ? MetaprogresionManager.Instance.SerriaTierAlmenaras * 3 : 0;
    chance += ExploracionSumadaPorActividades();
    chance += ObtenerModificadorChanceExploracionTraits();
    chance += estadosCaravana != null ? estadosCaravana.ObtenerModificadorExploracionPendiente() : 0;
    chance += modificadorContextual;
    if (TienePresagioActivo(PresagioCatalog.ZonaDesconocida))
    {
      chance -= 10;
    }

    if (intTipoClima == 5)
    {
      chance -= 10;
    }

    if (EsNocheActual())
    {
      chance -= 15;
    }

    if (scTutorialManager != null && scTutorialManager.tutorialActivo)
    {
      chance = 100;
    }

    return Mathf.Clamp(chance, 0, 100);
  }

  public int ObtenerChanceExploracionViaje()
  {
    return ObtenerChanceExploracionPasiva();
  }

  public int ObtenerChanceExploracionDescanso(int modificadorDescanso = 0)
  {
    return ObtenerChanceExploracionPasiva(modificadorDescanso);
  }

  public int ObtenerChanceScout()
  {
    int chance = 80;
    chance += scAtributosZona != null ? scAtributosZona.modChanceExploracion : 0;
    chance -= GetTierAlientoNegro() >= 2 ? 10 : 0;
    return Mathf.Clamp(chance, 0, 100);
  }

  public bool IntentarEnviarExploradores(Nodo destino)
  {
    if (!PuedeEnviarExploradores(destino, out string motivo))
    {
      if (!string.IsNullOrEmpty(motivo))
      {
        EscribirAdvertenciaLog(motivo);
      }
      return false;
    }

    EnviarExploradores(destino);
    return true;
  }

  public bool PuedeEnviarExploradoresATooltip(Nodo destino)
  {
    return PuedeEnviarExploradores(destino, out _);
  }

  bool PuedeEnviarExploradores(Nodo destino, out string motivo)
  {
    motivo = "";

    if (enviandoExploradores || MoviendoCaravana)
    {
      motivo = LocExploradores(
        "-No se pueden enviar exploradores mientras la caravana esta ocupada.",
        "-Scouts cannot be sent while the caravan is busy.",
        "-Nao e possivel enviar exploradores enquanto a caravana esta ocupada.");
      return false;
    }

    if (HayInteraccionTransitoriaActivaParaExploradores() || ObtenerTipoCombatePendiente() > 0)
    {
      motivo = LocExploradores(
        "-Resuelve la interaccion actual antes de enviar exploradores.",
        "-Resolve the current interaction before sending scouts.",
        "-Resolva a interacao atual antes de enviar exploradores.");
      return false;
    }

    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    if (nodoActual == null || destino == null || !nodoActual.DestinosPosibles.Contains(destino))
    {
      motivo = LocExploradores(
        "-Solo puedes enviar exploradores a un nodo adyacente.",
        "-Scouts can only be sent to an adjacent node.",
        "-Exploradores so podem ser enviados a um nodo adjacente.");
      return false;
    }

    if (destino.posXNodo - nodoActual.posXNodo != 1)
    {
      motivo = LocExploradores(
        "-Los exploradores solo pueden avanzar por caminos continuos.",
        "-Scouts can only advance through continuous paths.",
        "-Exploradores so podem avancar por caminhos continuos.");
      return false;
    }

    bool ignorarAlcanceTutorial = scTutorialManager != null
      && scTutorialManager.tutorialActivo
      && scTutorialManager.EsClaroMisteriosoTutorial(destino);
    bool ignorarAlcanceMisteriosoAdyacente = destino.ObtenerEstadoMisterioso();
    if (!ignorarAlcanceTutorial && !ignorarAlcanceMisteriosoAdyacente
      && (scMapaManager == null || !scMapaManager.NodoDentroDeVision(destino)))
    {
      motivo = LocExploradores(
        "-Ese destino esta fuera de la distancia de vision.",
        "-That destination is outside vision range.",
        "-Esse destino esta fora da distancia de visao.");
      return false;
    }

    if (destino.EstaReveladoParaExploradores())
    {
      motivo = LocExploradores(
        "-Ese destino ya fue revelado. Solo puedes enviar exploradores a nodos desconocidos.",
        "-That destination has already been revealed. Scouts can only be sent to unknown nodes.",
        "-Esse destino ja foi revelado. Exploradores so podem ser enviados a nodos desconhecidos.");
      return false;
    }

    if (GetOroActuales() < 50 || GetCivilesActual() < 30)
    {
      motivo = LocExploradores(
        "-Enviar exploradores requiere al menos 30 Civiles disponibles y 50 Oro.",
        "-Sending scouts requires at least 30 available Civilians and 50 Gold.",
        "-Enviar exploradores requer pelo menos 30 Civis disponiveis e 50 de Ouro.");
      return false;
    }

    return true;
  }

  void EnviarExploradores(Nodo destino)
  {
    if (destino == null)
    {
      return;
    }

    StartCoroutine(EnviarExploradoresCoroutine(destino));
  }

  IEnumerator EnviarExploradoresCoroutine(Nodo destino)
  {
    if (destino == null)
    {
      yield break;
    }

    enviandoExploradores = true;
    EscribirLog(LocExploradores(
      "-Los exploradores se dirigen al destino.",
      "-The scouts head toward the destination.",
      "-Os exploradores seguem para o destino."), true);
    yield return new WaitForSecondsRealtime(RetrasoInicioEnvioExploradoresSegundos);

    OcultarInterfazCampaniaParaExploradores();
    ComenzarBufferTextosFlotantesCampania();

    CambiarOroActual(-50);
    CambiarCivilesActuales(-5);

    Nodo origenExploradores = scMapaManager != null ? scMapaManager.nodoActual : null;
    Coroutine tiempoExploradores = StartCoroutine(TranscurrirAccionCampania(
      DuracionEnvioExploradoresHoras,
      TipoAvanceTiempoCampania.Exploradores,
      1f,
      1f,
      true));
    yield return ScoutExplorationSequenceFx.ReproducirTrayecto(origenExploradores, destino, DuracionAnimacionEnvioExploradoresSegundos);
    yield return tiempoExploradores;
    FinalizarAccionTemporal();

    ResultadoExploradoresCampania resultado = ResolverResultadoExploradores(destino);
    List<(string texto, Color color)> textosBufferizados = FinalizarBufferTextosFlotantesCampania();
    yield return ScoutExplorationSequenceFx.ReproducirResultado(resultado, DuracionResultadoExploradoresSegundos);
    enviandoExploradores = false;
    RestaurarInterfazCampaniaTrasExploradores();
    yield return null;

    if (scMenuCaravana != null)
    {
      scMenuCaravana.MostrarResultadoExploradores(resultado);
    }

    string logResultado = ObtenerLogResultadoExploradores(resultado);
    if (!string.IsNullOrEmpty(logResultado))
    {
      EscribirLogEnBitacoraSinTextoFlotante(logResultado, true);
      textosBufferizados.Insert(0, (logResultado, Color.cyan));
    }

    EmitirTutorialExploradoresCompletado(resultado);

    if (textosBufferizados.Count > 0)
    {
      yield return ReproducirTextosExploradores(textosBufferizados);
    }

    LiberarTextosRecursosSuspendidos();
  }

  ResultadoExploradoresCampania ResolverResultadoExploradores(Nodo destino)
  {
    ResultadoExploradoresCampania resultado = new ResultadoExploradoresCampania();
    resultado.nodoObjetivo = destino;
    resultado.chance = ObtenerChanceScout();
    resultado.tirada = UnityEngine.Random.Range(1, 101);
    if (DebeForzarExitoExploradoresTutorial(destino))
    {
      resultado.chance = 100;
      resultado.tirada = 79;
    }

    int umbralExito = Mathf.Clamp(101 - resultado.chance, 1, 100);
    bool falloCritico = resultado.tirada <= 20;
    bool exito = resultado.chance > 0 && !falloCritico && resultado.tirada >= umbralExito;
    bool exitoCritico = exito && resultado.tirada >= 80;

    if (exitoCritico)
    {
      resultado.exito = true;
      resultado.critico = true;
      resultado.civilesDevueltos = 5;
      resultado.materialesGanados = UnityEngine.Random.Range(10, 21);
      resultado.oroGanado = UnityEngine.Random.Range(5, 16);
      resultado.esperanzaCambio = 5;

      CambiarCivilesActuales(resultado.civilesDevueltos);
      CambiarMaterialesActuales(resultado.materialesGanados);
      CambiarOroActual(resultado.oroGanado);
      CambiarEsperanzaActual(resultado.esperanzaCambio);
      RevelarNodoPorExploradores(destino, true, resultado);

      resultado.titulo = LocExploradores(
        "Exito critico de exploradores",
        "Critical scout success",
        "Sucesso critico dos exploradores");
      resultado.descripcion = LocExploradores(
        "Revelaron el destino, volvieron todos y trajeron recursos.",
        "They revealed the destination, all returned, and they brought supplies back.",
        "Eles revelaram o destino, todos retornaram e trouxeram recursos.");
      return resultado;
    }

    if (exito)
    {
      resultado.exito = true;
      resultado.civilesDevueltos = 5;
      CambiarCivilesActuales(resultado.civilesDevueltos);
      RevelarNodoPorExploradores(destino, false, resultado);

      resultado.titulo = LocExploradores(
        "Exito de exploradores",
        "Scout success",
        "Sucesso dos exploradores");
      resultado.descripcion = LocExploradores(
        "Revelaron el destino y volvieron todos.",
        "They revealed the destination and all returned.",
        "Eles revelaram o destino e todos retornaram.");
      return resultado;
    }

    if (falloCritico)
    {
      resultado.critico = true;
      resultado.esperanzaCambio = -7;
      CambiarEsperanzaActual(resultado.esperanzaCambio);

      resultado.titulo = LocExploradores(
        "Fallo critico de exploradores",
        "Critical scout failure",
        "Falha critica dos exploradores");
      resultado.descripcion = LocExploradores(
        "Los exploradores se perdieron y no regresaron.",
        "The scouts were lost and did not return.",
        "Os exploradores se perderam e nao retornaram.");
      return resultado;
    }

    resultado.civilesMuertos = UnityEngine.Random.Range(1, 4);
    resultado.civilesDevueltos = 5 - resultado.civilesMuertos;
    resultado.esperanzaCambio = -5;
    CambiarCivilesActuales(resultado.civilesDevueltos);
    CambiarEsperanzaActual(resultado.esperanzaCambio);

    resultado.titulo = LocExploradores(
      "Fallo de exploradores",
      "Scout failure",
      "Falha dos exploradores");
    resultado.descripcion = LocExploradores(
      "No llegaron al destino y volvieron con bajas.",
      "They failed to reach the destination and returned with casualties.",
      "Eles nao chegaram ao destino e retornaram com baixas.");
    return resultado;
  }

  bool DebeForzarExitoExploradoresTutorial(Nodo destino)
  {
    return DebeUsarConfiguracionTutorial()
      && scTutorialManager != null
      && scTutorialManager.EsClaroMisteriosoTutorial(destino);
  }

  void EmitirTutorialExploradoresCompletado(ResultadoExploradoresCampania resultado)
  {
    if (resultado == null || resultado.nodoObjetivo == null)
    {
      return;
    }

    Nodo nodo = resultado.nodoObjetivo;
    TutorialDirector director = TutorialDirector.Instance;
    TutorialStep pasoAntes = director != null ? director.CurrentStep : null;
    TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignScoutsExplorationCompleted, nodo.gameObject)
      .Add("nodeId", nodo.ObtenerTutorialTargetId())
      .Add("type", nodo.tipoNodo)
      .Add("x", nodo.posXNodo)
      .Add("y", nodo.posYNodo)
      .Add("success", resultado.exito ? 1 : 0)
      .Add("critical", resultado.critico ? 1 : 0));

    if (DebeAvanzarTutorialNuevoPorExploradores(resultado, nodo, director, pasoAntes))
    {
      director.NextStep();
    }

    if (resultado.exito
        && scTutorialManager != null
        && scTutorialManager.tutorialActivo
        && scTutorialManager.pasoActual == 20
        && scTutorialManager.EsClaroMisteriosoTutorial(nodo))
    {
      scTutorialManager.SiguientePaso();
    }
  }

  bool DebeAvanzarTutorialNuevoPorExploradores(ResultadoExploradoresCampania resultado, Nodo nodo, TutorialDirector director, TutorialStep pasoAntes)
  {
    if (resultado == null || !resultado.exito || nodo == null || director == null || !director.IsRunning || pasoAntes == null)
    {
      return false;
    }

    if (director.CurrentStep != pasoAntes || scTutorialManager == null || !scTutorialManager.EsClaroMisteriosoTutorial(nodo))
    {
      return false;
    }

    return pasoAntes.id == "Exploracion2"
      || pasoAntes.id == "exploracion2"
      || (!string.IsNullOrEmpty(pasoAntes.targetId) && pasoAntes.targetId.StartsWith("tuto_nodoclaromist"));
  }

  void RevelarNodoPorExploradores(Nodo nodo, bool revelarFaccionCombate, ResultadoExploradoresCampania resultado)
  {
    if (nodo == null)
    {
      return;
    }

    nodo.RevelarPorExploradores();

    if (revelarFaccionCombate && EsTipoNodoDeCombateConScout(nodo.tipoNodo) && scMenuBatallas != null)
    {
      if (scMenuBatallas.TryGenerarFaccionScout(nodo, out string factionId, out string factionName))
      {
        nodo.RegistrarFaccionScoutRevelada(factionId, factionName);
        resultado.faccionReveladaNombre = factionName;
      }
    }

    if (scMapaManager != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }
  }

  bool EsTipoNodoDeCombateConScout(int tipoNodo)
  {
    return tipoNodo == 1 || tipoNodo == 8 || tipoNodo == 11 || tipoNodo == 15;
  }

  void EscribirLogResultadoExploradores(ResultadoExploradoresCampania resultado)
  {
    string logResultado = ObtenerLogResultadoExploradores(resultado);
    if (string.IsNullOrEmpty(logResultado))
    {
      return;
    }

    EscribirLog(logResultado, true);
  }

  string ObtenerLogResultadoExploradores(ResultadoExploradoresCampania resultado)
  {
    if (resultado == null)
    {
      return string.Empty;
    }

    if (resultado.exito && resultado.critico)
    {
      string logExitoCritico = LocExploradores(
        "-Los exploradores regresan tras revelar el destino. Todos volvieron y trajeron recursos.",
        "-The scouts return after revealing the destination. Everyone made it back and they brought resources.",
        "-Os exploradores retornam apos revelar o destino. Todos voltaram e trouxeram recursos.");

      if (!string.IsNullOrEmpty(resultado.faccionReveladaNombre))
      {
        logExitoCritico += " " + LocExploradores(
          "Enemigos avistados: ",
          "Enemies sighted: ",
          "Inimigos avistados: ") + resultado.faccionReveladaNombre + ".";
      }

      return logExitoCritico;
    }

    if (resultado.exito)
    {
      return LocExploradores(
        "-Los exploradores regresan tras revelar el destino. Todos volvieron sanos y salvos.",
        "-The scouts return after revealing the destination. Everyone made it back safely.",
        "-Os exploradores retornam apos revelar o destino. Todos voltaram em seguranca.");
    }

    if (resultado.critico)
    {
      return LocExploradores(
        "-Los exploradores se perdieron y no regresaron.",
        "-The scouts were lost and did not return.",
        "-Os exploradores se perderam e nao retornaram.");
    }

    return LocExploradores(
      "-Los exploradores no llegaron al destino y regresaron con bajas.",
      "-The scouts failed to reach the destination and returned with casualties.",
      "-Os exploradores nao chegaram ao destino e retornaram com baixas.");
  }

  void OcultarInterfazCampaniaParaExploradores()
  {
    estadosCanvasCampaniaDuranteExploradores.Clear();
    if (goCanvas == null)
    {
      return;
    }

    Transform canvasTransform = goCanvas.transform;
    for (int i = 0; i < canvasTransform.childCount; i++)
    {
      Transform hijo = canvasTransform.GetChild(i);
      if (hijo == null)
      {
        continue;
      }

      estadosCanvasCampaniaDuranteExploradores[hijo.gameObject] = hijo.gameObject.activeSelf;
      hijo.gameObject.SetActive(false);
    }
  }

  void RestaurarInterfazCampaniaTrasExploradores()
  {
    if (goCanvas == null || estadosCanvasCampaniaDuranteExploradores.Count == 0)
    {
      estadosCanvasCampaniaDuranteExploradores.Clear();
      return;
    }

    foreach (KeyValuePair<GameObject, bool> estado in estadosCanvasCampaniaDuranteExploradores)
    {
      if (estado.Key != null)
      {
        estado.Key.SetActive(estado.Value);
      }
    }

    estadosCanvasCampaniaDuranteExploradores.Clear();
  }

  void OcultarInterfazCampaniaParaIntro()
  {
    if (goCanvas == null)
    {
      return;
    }

    Transform canvasTransform = goCanvas.transform;
    for (int i = 0; i < canvasTransform.childCount; i++)
    {
      Transform hijo = canvasTransform.GetChild(i);
      if (hijo == null)
      {
        continue;
      }

      if (!estadosCanvasCampaniaDuranteIntro.ContainsKey(hijo.gameObject))
      {
        estadosCanvasCampaniaDuranteIntro[hijo.gameObject] = hijo.gameObject.activeSelf;
      }
      hijo.gameObject.SetActive(false);
    }

    interfazCampaniaOcultaPorIntro = true;
  }

  void RestaurarInterfazCampaniaTrasIntro()
  {
    if (!interfazCampaniaOcultaPorIntro)
    {
      estadosCanvasCampaniaDuranteIntro.Clear();
      return;
    }

    if (goCanvas != null)
    {
      foreach (KeyValuePair<GameObject, bool> estado in estadosCanvasCampaniaDuranteIntro)
      {
        if (estado.Key != null)
        {
          estado.Key.SetActive(estado.Value);
        }
      }
    }

    estadosCanvasCampaniaDuranteIntro.Clear();
    interfazCampaniaOcultaPorIntro = false;
  }

  void CompletarInicioCampaniaSiCorresponde()
  {
    if (!startCampaniaEjecutado || IntroCampaniaActivaOPendiente)
    {
      return;
    }

    if (debeEscribirLogInicioEnStart)
    {
      EscribirLogInicioCampania();
      debeEscribirLogInicioEnStart = false;
    }

    if (!eventoInicioCampaniaEmitido)
    {
      TutorialEvents.Emit(TutorialEventNames.CampaignStarted, gameObject);
      eventoInicioCampaniaEmitido = true;
    }

    BanterBattleUI.InstalarCampania(this);
  }

  void EjecutarAccionesAlFinalizarIntroCampania()
  {
    if (accionesAlFinalizarIntroCampania.Count == 0)
    {
      return;
    }

    List<System.Action> acciones = new List<System.Action>(accionesAlFinalizarIntroCampania);
    accionesAlFinalizarIntroCampania.Clear();

    for (int i = 0; i < acciones.Count; i++)
    {
      acciones[i]?.Invoke();
    }
  }

  IEnumerator ReproducirTextosExploradores(List<(string texto, Color color)> textosBufferizados)
  {
    if (textosBufferizados == null || textosBufferizados.Count == 0)
    {
      yield break;
    }

    for (int i = 0; i < textosBufferizados.Count; i++)
    {
      (string texto, Color color) = textosBufferizados[i];
      GenerarTextoFlotanteCampaña(texto, color);

      if (i < textosBufferizados.Count - 1)
      {
        yield return new WaitForSecondsRealtime(RetrasoEntreTextosExploradoresSegundos);
      }
    }
  }

  string LocExploradores(string es, string en, string pt)
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
      case TRADU.IdiomaIngles:
        return en;
      case TRADU.IdiomaPortugues:
        return pt;
      default:
        return es;
    }
  }

  public bool TryGuardarCampania(out string error, string path = null)
  {
    error = string.Empty;

    if (!PuedeGuardarCampania(out string motivo))
    {
      error = motivo;
      return false;
    }

    SaveFileData save = ConstruirSaveActual();
    if (save == null)
    {
      error = "No se pudo construir el estado de guardado.";
      return false;
    }

    if (!SaveGameService.TryWriteSaveFile(save, out error, path))
    {
      return false;
    }

    return true;
  }

  public void GuardarCampaniaManual()
  {
    if (TryGuardarCampania(out string error))
    {
      NotificarResultadoGuardado("-Campaña guardada.", Color.cyan);
      return;
    }

    NotificarResultadoGuardado("-No se pudo guardar la campaña. " + error, Color.red);
  }

  public void CargarCampaniaManual()
  {
    if (TryCargarCampaniaDesdeArchivo(out string error))
    {
      NotificarResultadoGuardado("-Campaña cargada.", Color.cyan);
      return;
    }

    NotificarResultadoGuardado("-No se pudo cargar la campaña. " + error, Color.red);
  }

  public bool TryAutosaveCampania(string origen, out string error)
  {
    if (TryGuardarCampania(out error))
    {
      Debug.Log("[SaveGame] Autosave de campania completado" + FormatearOrigenAutosave(origen) + ".");
      return true;
    }

    Debug.LogWarning("[SaveGame] No se pudo completar el autosave" + FormatearOrigenAutosave(origen) + ". " + error);
    return false;
  }

  public bool TryCargarCampaniaDesdeArchivo(out string error, string path = null)
  {
    error = string.Empty;
    if (!SaveGameService.TryReadSaveFile(out SaveFileData save, out error, path))
    {
      return false;
    }

    return CargarCampaniaPendiente(save, out error, true);
  }

  public SaveFileData ConstruirSaveActual()
  {
    ItemDatabase itemDatabase = ItemSaveCatalog.GetRuntimeItemDatabase(this);
    if (itemDatabase == null)
    {
      Debug.LogWarning("[SaveGame] No se encontro ItemDatabase runtime. El guardado de items podría quedar incompleto.");
    }

    SaveFileData save = new SaveFileData();
    save.campaign = ConstruirCampaignSaveData();
    save.map = ConstruirMapSaveData();
    save.party = ConstruirPartySaveData(itemDatabase);
    save.sequitos = ConstruirSequitosSaveData(itemDatabase);
    save.metaprogresion = ConstruirMetaprogresionSaveData();
    save.MarcarGuardadoAhora();
    return save;
  }

  private CampaignSaveData ConstruirCampaignSaveData()
  {
    CampaignSaveData data = new CampaignSaveData();
    data.zonaId = scAtributosZona != null ? scAtributosZona.ID : 0;
    data.zonaFase = scAtributosZona != null ? scAtributosZona.FASE : 0;
    MapDecorator mapDecorator = scAtributosZona != null ? scAtributosZona.GetComponent<MapDecorator>() : null;
    data.reliefSeed = mapDecorator != null ? mapDecorator.GetReliefSeed() : 0;
    data.pasoVientoHeladoFuerzaKaleTav = scAtributosZona != null ? scAtributosZona.PasoVientoHelado_FuerzaKaleTav : 0;
    data.presagiosRegionId = presagiosRegionActivaId;
    data.presagiosActivos = new List<int>(presagiosActivosRegion);
    data.primeraBatallaPresagioEnemigosConsumida = primeraBatallaPresagioEnemigosConsumida;
    data.horasTotales = horasTotales;
    data.posicionCaravana = posicionCaravana;
    data.tipoClima = intTipoClima;
    data.alientoNegroHoras = GetValorAlientoNegro();
    data.antorchasEncendidas = antorchasEncendidas;
    data.progresoFatigaHoras = progresoFatigaHoras;
    data.creditoPrevencionAlientoHoras = creditoPrevencionAlientoHoras;
    data.horasViajadas = estadisticaHorasViajadas;
    data.viajeEnCurso = MoviendoCaravana && nodoDestinoActual != null;
    data.viajeProgresoNormalizado = data.viajeEnCurso ? ObtenerProgresoViajeActual() : 0f;
    data.viajeHorasTranscurridas = data.viajeEnCurso ? horasViajeActual : 0f;
    data.viajeActualIncluyoNoche = data.viajeEnCurso && viajeActualIncluyoNoche;
    data.viajeClimaInicial = data.viajeEnCurso ? viajeClimaInicial : 0;
    data.viajeMultiplicadorVelocidad = data.viajeEnCurso ? multiplicadorVelocidadVisualViajeActual : 1f;
    data.emboscadaViajeCalculada = data.viajeEnCurso && emboscadaViajeCalculada;
    data.logEmboscadaViajePendiente = data.viajeEnCurso ? logEmboscadaViajePendiente : null;
    data.acumuladorEfectosSequitosHoras = acumuladorEfectosSequitosHoras;
    data.combateHoraCapturada = combateHoraCapturada;
    data.combateNocturno = combateNocturno;
    data.descansoInterrumpidoPendiente = descansoInterrumpidoPendiente;
    data.descansoResultadosPendientes = descansoResultadosPendientes;
    data.descansoTuvoEmboscada = descansoTuvoEmboscada;
    data.descansoRitualElegible = descansoRitualElegible;
    data.descansoRitualPersonajeId = descansoRitualPersonajeId;
    data.descansoClimaInicial = descansoClimaInicial;
    data.descansoValorTarea = descansoValorTarea;
    data.descansoChanceExploracion = descansoChanceExploracion;
    data.descansoChanceEmboscada = descansoChanceEmboscada;
    data.descansoEmboscadaPendiente = descansoEmboscadaPendiente;
    data.descansoHorasHastaEmboscada = descansoHorasHastaEmboscada;
    data.descansoTiradaEmboscada = descansoTiradaEmboscada;
    data.descansoHorasRestantes = descansoHorasRestantes;
    data.descansoTareaCivil = descansoTareaCivil;
    data.descansoEnClaro = descansoEnClaro;
    data.descansoHoraCombate = descansoHoraCombate;
    data.fatiga = GetFatigaActual();
    data.esperanza = GetEsperanzaActual();
    data.civiles = Mathf.RoundToInt(GetCivilesActual());
    data.bueyes = GetBueyesActual();
    data.suministros = GetSuministrosActuales();
    data.materiales = GetMaterialesActuales();
    data.oro = GetOroActuales();
    data.mejoraCaravanaAntorchas = mejoraCaravanaAntorchas;
    data.mejoraCaravanaAlforjas = mejoraCaravanaAlforjas;
    data.mejoraCaravanaTiendas = mejoraCaravanaTiendas;
    data.mejoraCaravanaCatalejos = mejoraCaravanaCatalejos;
    data.mejoraCaravanaAlmacen = mejoraCaravanaAlmacen;
    data.mejoraCaravanaDefensas = mejoraCaravanaDefensas;
    data.sequitoHerrerosMantArmasHoras = sequitoHerrerosMantArmasHoras;
    data.sequitoHerrerosMantArmadurasHoras = sequitoHerrerosMantArmadurasHoras;
    data.sequitoMercaderesTier = sequitoMercaderesTier;
    data.sequitoCuranderosMejoraCuracion = sequitoCuranderosMejoraCuracion;
    data.miliciasMejoras = miliciasMejoras;
    data.peligroZonaAnterior = peligrozonaanterior;
    data.puestoComercialSuministrosDisp = pComercialSuministrosDisp;
    data.puestoComercialMaterialesDisp = pComercialMaterialesDisp;
    data.puestoComercialBueyesDisp = pComercialBueyesDisp;
    data.tutorialActivo = scTutorialManager != null && scTutorialManager.tutorialActivo;
    data.tutorialPasoActual = scTutorialManager != null ? scTutorialManager.pasoActual : 0;
    TutorialDirector tutorialNuevo = TutorialDirector.Instance;
    data.tutorialNuevoActivo = tutorialNuevo != null && tutorialNuevo.IsRunning;
    data.tutorialNuevoId = tutorialNuevo != null && tutorialNuevo.ActiveDefinition != null
      ? tutorialNuevo.ActiveDefinition.tutorialId
      : string.Empty;
    data.tutorialNuevoPasoId = tutorialNuevo != null ? tutorialNuevo.CurrentStepId : string.Empty;
    data.tutorialNuevoPendienteTrasDescripcionZona = PlayerPrefs.GetInt(TutorialDirector.PendingStartAfterZoneDescriptionKey, 0) == 1;
    TutorialTooltipProgress.CopiarASave(data);
    data.estadisticaBatallasLibradas = estadisticaBatallasLibradas;
    data.estadisticaCivilesPerdidos = estadisticaCivilesPerdidos;
    data.estadisticaAsentamientosVisitados = estadisticaAsentamientosVisitados;
    if (scAtributosZona != null && scAtributosZona.ZonasEstado != null)
    {
      data.zonasEstado = new List<int>(scAtributosZona.ZonasEstado);
    }
    data.nodoActual = CrearReferenciaNodo(scMapaManager != null ? scMapaManager.nodoActual : null);
    data.nodoDestinoActual = CrearReferenciaNodo(MoviendoCaravana ? nodoDestinoActual : null);
    data.batallaEnCurso = BATALLA_EnCurso;
    data.emboscadaEnCurso = EMBOSCADA_EnCurso;
    data.eventosAleatoriosUsadosMapa = new List<int>(eventosAleatoriosUsadosMapa);
    data.estadosCaravana = estadosCaravana != null ? estadosCaravana.ConstruirSaveData() : new EstadosCaravanaSaveData();
    data.bitacora = logDeCampania != null ? logDeCampania.ExportarSaveData() : new BitacoraSaveData();
    GuardarUltimasAparienciasPorClaseEnSave(data);
    data.settlementOpen = asentamientoManager != null && asentamientoManager.DebeGuardarseComoAbierto;
    data.settlementActionsRemaining = asentamientoManager != null ? asentamientoManager.AccionesRestantes : 3;
    return data;
  }

  private MapSaveData ConstruirMapSaveData()
  {
    MapSaveData data = new MapSaveData();
    if (scMapaManager == null || scMapaManager.scContenedordeNodos == null)
    {
      return data;
    }

    foreach (Nodo nodo in scMapaManager.scContenedordeNodos.GetComponentsInChildren<Nodo>(true))
    {
      if (nodo == null)
      {
        continue;
      }

      NodeSaveData nodoData = new NodeSaveData();
      nodoData.x = nodo.posXNodo;
      nodoData.y = nodo.posYNodo;
      nodoData.activo = nodo.gameObject.activeSelf;
      nodoData.tipoNodo = nodo.tipoNodo;
      nodoData.nodoDespejado = nodo.nodoDespejado;
      nodoData.revelado = nodo.revelado;
      nodoData.yatiroConexiones = nodo.yatiroConexiones;
      nodoData.nodoIncendiado = nodo.nodoIncendiado;
      nodoData.nodoRitual = nodo.nodoRitual;
      nodoData.tipoNodoOriginalRitual = nodo.tipoNodoOriginalRitual;
      nodoData.visualCode = nodo.ObtenerVisualCodeActual();
      nodoData.esMisterioso = nodo.ObtenerEstadoMisterioso();
      nodoData.atajoSubterraneoPendiente = nodo.ObtenerAtajoSubterraneoPendiente();
      nodoData.faccionScoutReveladaId = nodo.ObtenerFaccionScoutReveladaId();
      nodoData.faccionScoutReveladaNombre = nodo.ObtenerFaccionScoutReveladaNombre();
      nodoData.visibilidadForzadaEspecial = nodo.TieneVisibilidadForzadaPorReveladoEspecial();
      nodoData.reveladoPorZonaCartografiada = nodo.FueReveladoPorZonaCartografiada();
      nodoData.descubiertoPorMecanicaEspecial = nodo.FueDescubiertoPorMecanicaEspecial();

      foreach (CaminoConexion conexion in nodo.ConexionesSalientes)
      {
        if (conexion == null || conexion.destino == null)
        {
          continue;
        }

        nodoData.conexiones.Add(new CaminoConexionSaveData
        {
          destinoX = conexion.destino.posXNodo,
          destinoY = conexion.destino.posYNodo,
          tipo = conexion.tipo,
          costoMovimiento = conexion.costoMovimiento,
          rutaHaciaAldea = conexion.rutaHaciaAldea,
          recorridoPorCaravana = conexion.recorridoPorCaravana,
          reveladoPorVision = false
        });
      }

      data.nodes.Add(nodoData);
    }

    if (scMapaManager != null)
    {
      data.emboscadasSubterraneasZona = scMapaManager.ObtenerEmboscadasSubterraneasZona();
      data.viajesDesdeUltimaEmboscadaSubterranea = scMapaManager.ObtenerViajesDesdeUltimaEmboscadaSubterranea();
      foreach (Nodo settlement in scMapaManager.ObtenerSettlementsForzados())
      {
        data.settlementsForzados.Add(CrearReferenciaNodo(settlement));
      }
    }

    return data;
  }

  private PartySaveData ConstruirPartySaveData(ItemDatabase itemDatabase)
  {
    PartySaveData data = new PartySaveData();
    if (scMenuPersonajes == null)
    {
      return data;
    }

    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null)
      {
        continue;
      }

      data.characters.Add(ConstruirCharacterSaveData(personaje, itemDatabase));
    }

    if (scMenuPersonajes.scEquipo != null)
    {
      foreach (GameObject itemGo in scMenuPersonajes.scEquipo.listInventario)
      {
        if (itemGo == null)
        {
          continue;
        }

        Item item = itemGo.GetComponent<Item>();
        if (item == null)
        {
          continue;
        }

        string itemId = ItemSaveCatalog.ResolveItemId(item, itemDatabase);
        if (!string.IsNullOrWhiteSpace(itemId))
        {
          data.inventoryItemIds.Add(itemId);
        }
      }
    }

    AgregarSeleccionParticipante(scAdministradorEscenas != null ? scAdministradorEscenas.Personaje1 : null, data.selectedBattleCharacterIds);
    AgregarSeleccionParticipante(scAdministradorEscenas != null ? scAdministradorEscenas.Personaje2 : null, data.selectedBattleCharacterIds);
    AgregarSeleccionParticipante(scAdministradorEscenas != null ? scAdministradorEscenas.Personaje3 : null, data.selectedBattleCharacterIds);
    AgregarSeleccionParticipante(scAdministradorEscenas != null ? scAdministradorEscenas.Personaje4 : null, data.selectedBattleCharacterIds);

    return data;
  }

  private CharacterSaveData ConstruirCharacterSaveData(Personaje personaje, ItemDatabase itemDatabase)
  {
    CharacterSaveData data = new CharacterSaveData();
    data.id = personaje.EnsurePersistentId();
    data.nombre = personaje.sNombre;
    data.idClase = personaje.IDClase;
    data.idRetrato = personaje.idRetrato;
    data.indiceAparienciaAlternativa = personaje.indiceAparienciaAlternativa;
    data.aparienciaAlternativaResuelta = personaje.aparienciaAlternativaResuelta;
    data.puestoDeseado = personaje.iPuestoDeseado;
    data.vidaActual = personaje.fVidaActual;
    data.vidaMaxima = personaje.fVidaMaxima;
    data.experienciaActual = personaje.fExperienciaActual;
    data.nivelActual = personaje.fNivelActual;
    data.fuerza = personaje.iFuerza;
    data.agi = personaje.iAgi;
    data.poder = personaje.iPoder;
    data.iniciativa = personaje.iIniciativa;
    data.apMax = personaje.iApMax;
    data.valMax = personaje.iValMax;
    data.armadura = personaje.iArmadura;
    data.defensa = personaje.iDefensa;
    data.tsReflejo = personaje.iTSReflejo;
    data.tsFortaleza = personaje.iTSFortaleza;
    data.tsMental = personaje.iTSMental;
    data.resFuego = personaje.iResFuego;
    data.resRayo = personaje.iResRayo;
    data.resHielo = personaje.iResHielo;
    data.resArcano = personaje.iResArcano;
    data.resAcido = personaje.iResAcido;
    data.resNecro = personaje.iResNecro;
    data.resDivino = personaje.iResDivino;
    data.critRango = personaje.fCritRango;
    data.critDanio = personaje.fCritDanio;
    data.bonusAtaque = personaje.fBonusAtaque;
    data.sinCooldownDebug = personaje.sinCooldownDebug;
    data.habilidades = CopiarHabilidadesPersonaje(personaje);
    data.actividades = CopiarActividadesPersonaje(personaje);
    data.actividadSeleccionada = personaje.PuedeRealizarActividades() ? personaje.ActividadSeleccionada : 0;
    data.actividadFijada = personaje.ActividadFijada;
    data.nivelPuntoAtributo = personaje.NivelPuntoAtributo;
    data.nivelPuntoTS = personaje.NivelPuntoTS;
    data.nivelPuntoHabilidad = personaje.NivelPuntoHabilidad;
    data.nivelNuevaHabilidadBase = personaje.NivelNuevaHabilidadBase;
    data.campFatigado = personaje.Camp_Fatigado;
    data.campBendecidoHoras = personaje.ObtenerHorasRestantesBendecido();
    data.campHerido = personaje.Camp_Herido;
    data.campEnfermoHoras = personaje.ObtenerHorasRestantesEnfermo();
    data.campMoralEstado = (int)personaje.ObtenerEstadoMoralCampania();
    data.campMoralHoras = personaje.ObtenerHorasRestantesMoral();
    data.campAvergonzado = personaje.Camp_Avergonzado;
    data.campMuerto = personaje.Camp_Muerto;
    data.campCorrupto = personaje.Camp_Corrupto;
    data.traitHeroeLocalCivilesOtorgados = personaje.TraitHeroeLocalCivilesOtorgados;
    data.traitHeroeLocalPenalidadMuerteAplicada = personaje.TraitHeroeLocalPenalidadMuerteAplicada;
    data.traitLiderCaravanaPenalidadMuerteAplicada = personaje.TraitLiderCaravanaPenalidadMuerteAplicada;
    data.traitEjemploASeguirAplicado = personaje.TraitEjemploASeguirAplicado;
    data.traitHerenciaItemOtorgado = personaje.TraitHerenciaItemOtorgado;
    data.horasViajadas = personaje.HorasViajadas;
    data.progresoActividades = personaje.ExportarProgresoActividades();
    data.enemigosEliminados = personaje.EnemigosEliminados;
    data.danioHecho = personaje.DanioHecho;
    data.danioRecibido = personaje.DanioRecibido;
    data.vecesDerribado = personaje.VecesDerribado;
    data.rasgos = CopiarRasgosPersonaje(personaje);
    data.equipment = ConstruirEquipmentSaveData(personaje, itemDatabase);
    return data;
  }

  private EquipmentSaveData ConstruirEquipmentSaveData(Personaje personaje, ItemDatabase itemDatabase)
  {
    EquipmentSaveData data = new EquipmentSaveData();
    data.armaItemId = ItemSaveCatalog.ResolveItemId(personaje.itemArma, itemDatabase);
    data.armaduraItemId = ItemSaveCatalog.ResolveItemId(personaje.itemArmadura, itemDatabase);
    data.accesorio1ItemId = ItemSaveCatalog.ResolveItemId(personaje.Accesorio1, itemDatabase);
    data.accesorio2ItemId = ItemSaveCatalog.ResolveItemId(personaje.Accesorio2, itemDatabase);
    data.consumible1ItemId = ItemSaveCatalog.ResolveItemId(personaje.Consumible1, itemDatabase);
    data.consumible2ItemId = ItemSaveCatalog.ResolveItemId(personaje.Consumible2, itemDatabase);
    return data;
  }

  private SequitosSaveData ConstruirSequitosSaveData(ItemDatabase itemDatabase)
  {
    SequitosSaveData data = new SequitosSaveData();

    if (scMenuSequito != null && scMenuSequito.lstSequitos != null)
    {
      data.sequitosActivos.AddRange(scMenuSequito.lstSequitos);
    }

    if (scSequitoHerboristas != null)
    {
      data.herboristasVecesEnClaro = scSequitoHerboristas.vecesEnClaro;
      data.herboristasCantBalsamoFort = scSequitoHerboristas.cantBalsamoFort;
      data.herboristasCantBalsamoReflej = scSequitoHerboristas.cantBalsamoReflej;
      data.herboristasCantBalsamoMental = scSequitoHerboristas.cantBalsamoMental;
    }

    if (scSequitoCronistas != null)
    {
      data.cronistasValorCambios = scSequitoCronistas.valorCambiosCronicas;
      data.cronistasYaVendio = scSequitoCronistas.yaVendioCronica;
    }

    if (scSequitoMercaderes != null && scSequitoMercaderes.ItemsVendidos != null)
    {
      foreach (Item item in scSequitoMercaderes.ItemsVendidos)
      {
        string itemId = ItemSaveCatalog.ResolveItemId(item, itemDatabase);
        if (!string.IsNullOrWhiteSpace(itemId))
        {
          data.mercaderesItemsVendidosIds.Add(itemId);
        }
      }
    }

    if (scSequitoClerigos != null)
    {
      data.clerigosZonaIdUltimaPlegaria = scSequitoClerigos.ObtenerZonaIdUltimaPlegaria();
    }

    return data;
  }

  private MetaprogresionSaveData ConstruirMetaprogresionSaveData()
  {
    MetaprogresionSaveData data = new MetaprogresionSaveData();
    data.presagiosRegionesPendientes = PresagioRegionPendienteStore.Exportar();
    MetaprogresionManager meta = MetaprogresionManager.Instance;
    if (meta != null)
    {
      data.zonasVisitadas = meta.ObtenerZonasVisitadas();
      data.climasExclusivosDescubiertos = meta.ObtenerClimasExclusivosDescubiertos();
    }
    AgregarZonaVisitadaASave(data, scAtributosZona != null ? scAtributosZona.ID : 0);
    if (meta == null)
    {
      return data;
    }

    data.corrupcionGlobal = meta.CorrupcionGlobal;
    data.cantidadCiviles = meta.CantidadCiviles;
    data.valorTrabajoDisponible = meta.ValordeTrabajoDisponible;
    data.misionesSalvamento = meta.MisionesSalvamento;
    data.nivelAlertaBosqueArdiente = meta.NivelAlertaBosqueArdiente;
    data.nivelAlertaPasoVientohelado = meta.NivelAlertaPasoVientohelado;
    data.nivelAlertaNedukazal = meta.NivelAlertaNedukazal;
    data.serriaTierBarcos = meta.SerriaTierBarcos;
    data.serriaTierAlmenaras = meta.SerriaTierAlmenaras;
    data.serriaTierPalacio = meta.SerriaTierPalacio;
    data.serriaTierCuartel = meta.SerriaTierCuartel;
    data.serriaTierGranjas = meta.SerriaTierGranjas;
    data.serriaTierBarricadas = meta.SerriaTierBarricadas;
    data.serriaTierTemplo = meta.SerriaTierTemplo;
    data.serriaPuntosAlmacenadosBarcos = meta.SerriaPuntosAlmacenadosBarcos;
    data.serriaPuntosAlmacenadosAlmenaras = meta.SerriaPuntosAlmacenadosAlmenaras;
    data.serriaPuntosAlmacenadosPalacio = meta.SerriaPuntosAlmacenadosPalacio;
    data.serriaPuntosAlmacenadosCuartel = meta.SerriaPuntosAlmacenadosCuartel;
    data.serriaPuntosAlmacenadosGranjas = meta.SerriaPuntosAlmacenadosGranjas;
    data.serriaPuntosAlmacenadosBarricadas = meta.SerriaPuntosAlmacenadosBarricadas;
    data.serriaPuntosAlmacenadosTemplo = meta.SerriaPuntosAlmacenadosTemplo;
    return data;
  }

  private static void AgregarZonaVisitadaASave(MetaprogresionSaveData data, int zonaId)
  {
    if (data == null || zonaId <= 0)
    {
      return;
    }

    if (data.zonasVisitadas == null)
    {
      data.zonasVisitadas = new List<int>();
    }

    if (!data.zonasVisitadas.Contains(zonaId))
    {
      data.zonasVisitadas.Add(zonaId);
    }
  }

  private NodeReferenceSaveData CrearReferenciaNodo(Nodo nodo)
  {
    NodeReferenceSaveData data = new NodeReferenceSaveData();
    if (nodo == null)
    {
      return data;
    }

    data.x = nodo.posXNodo;
    data.y = nodo.posYNodo;
    return data;
  }

  private int[] CopiarHabilidadesPersonaje(Personaje personaje)
  {
    return new int[]
    {
      personaje.Habilidad_1,
      personaje.Habilidad_2,
      personaje.Habilidad_3,
      personaje.Habilidad_4,
      personaje.Habilidad_5,
      personaje.Habilidad_6,
      personaje.Habilidad_7,
      personaje.Habilidad_8,
      personaje.Habilidad_9,
      personaje.Habilidad_10
    };
  }

  private int[] CopiarActividadesPersonaje(Personaje personaje)
  {
    return new int[]
    {
      personaje.Actividad_1,
      personaje.Actividad_2,
      personaje.Actividad_3
    };
  }

  private int[] CopiarRasgosPersonaje(Personaje personaje)
  {
    if (personaje.aRasgos == null)
    {
      return Array.Empty<int>();
    }

    int[] copia = new int[personaje.aRasgos.Length];
    Array.Copy(personaje.aRasgos, copia, personaje.aRasgos.Length);
    return copia;
  }

  private void AgregarSeleccionParticipante(Personaje personaje, List<string> seleccion)
  {
    if (personaje == null || seleccion == null)
    {
      return;
    }

    string id = personaje.EnsurePersistentId();
    if (!seleccion.Contains(id))
    {
      seleccion.Add(id);
    }
  }

  private void LimpiarEstadoActualParaCarga()
  {
    LimpiarSeleccionBatallaParaCarga();
    LimpiarInventarioParaCarga();
    LimpiarPersonajesParaCarga();
    LimpiarSequitosParaCarga();
  }

  private void LimpiarSeleccionBatallaParaCarga()
  {
    if (scAdministradorEscenas == null)
    {
      return;
    }

    scAdministradorEscenas.Personaje1 = null;
    scAdministradorEscenas.Personaje2 = null;
    scAdministradorEscenas.Personaje3 = null;
    scAdministradorEscenas.Personaje4 = null;
  }

  private void LimpiarPersonajesParaCarga()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje != null)
      {
        if (personaje.itemArma != null) Destroy(personaje.itemArma.gameObject);
        if (personaje.itemArmadura != null) Destroy(personaje.itemArmadura.gameObject);
        if (personaje.Accesorio1 != null) Destroy(personaje.Accesorio1.gameObject);
        if (personaje.Accesorio2 != null) Destroy(personaje.Accesorio2.gameObject);
        if (personaje.Consumible1 != null) Destroy(personaje.Consumible1.gameObject);
        if (personaje.Consumible2 != null) Destroy(personaje.Consumible2.gameObject);
        Destroy(personaje.gameObject);
      }
    }

    scMenuPersonajes.listaPersonajes.Clear();
    scMenuPersonajes.pSel = null;
  }

  private void LimpiarInventarioParaCarga()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.scEquipo == null || scMenuPersonajes.scEquipo.listInventario == null)
    {
      return;
    }

    foreach (GameObject itemGo in scMenuPersonajes.scEquipo.listInventario)
    {
      if (itemGo != null)
      {
        Destroy(itemGo);
      }
    }

    scMenuPersonajes.scEquipo.listInventario.Clear();
  }

  private void LimpiarSequitosParaCarga()
  {
    if (scMenuSequito != null)
    {
      scMenuSequito.LimpiarInstanciasParaCarga();
    }

    scSequitoMercaderes = null;
    scSequitoArtistas = null;
    scSequitoHerboristas = null;
    scSequitoDesertores = null;
    scSequitoCronistas = null;
    scSequitoRefugiados = null;
    scSequitoNobles = null;
    scSequitoClerigos = null;
    scSequitoEsclavos = null;
  }

  private void RestaurarTutorialDesdeSave(CampaignSaveData data)
  {
    if (data == null)
    {
      return;
    }

    if (scTutorialManager != null)
    {
      scTutorialManager.tutorialActivo = data.tutorialActivo;
      scTutorialManager.pasoActual = data.tutorialPasoActual;
    }

    TutorialTooltipProgress.RestaurarDesdeSave(data);
  }

  private void RestaurarMetaprogresionDesdeSave(SaveFileData saveFileData)
  {
    if (saveFileData != null && saveFileData.metaprogresion != null)
    {
      PresagioRegionPendienteStore.ImportarSiNoHayEstadoGlobal(saveFileData.metaprogresion.presagiosRegionesPendientes);
    }

    if (saveFileData == null || MetaprogresionManager.Instance == null)
    {
      return;
    }

    MetaprogresionSaveData data = saveFileData.metaprogresion;
    MetaprogresionManager meta = MetaprogresionManager.Instance;
    if (data != null)
    {
      meta.RestaurarZonasVisitadas(data.zonasVisitadas);
      meta.RestaurarClimasExclusivosDescubiertos(data.climasExclusivosDescubiertos);
    }
    if (saveFileData.campaign != null)
    {
      meta.MarcarZonaVisitada(saveFileData.campaign.zonaId);
    }

    if (saveFileData.version < 3 || data == null)
    {
      return;
    }

    meta.CorrupcionGlobal = Mathf.Max(0, data.corrupcionGlobal);
    meta.CantidadCiviles = Mathf.Max(0, data.cantidadCiviles);
    meta.ValordeTrabajoDisponible = Mathf.Max(0, data.valorTrabajoDisponible);

    if (data.misionesSalvamento >= 0) meta.MisionesSalvamento = data.misionesSalvamento;
    if (data.nivelAlertaBosqueArdiente >= 0) meta.NivelAlertaBosqueArdiente = data.nivelAlertaBosqueArdiente;
    if (data.nivelAlertaPasoVientohelado >= 0) meta.NivelAlertaPasoVientohelado = data.nivelAlertaPasoVientohelado;
    if (data.nivelAlertaNedukazal >= 0) meta.NivelAlertaNedukazal = data.nivelAlertaNedukazal;
    if (data.serriaTierBarcos >= 0) meta.SerriaTierBarcos = data.serriaTierBarcos;
    if (data.serriaTierAlmenaras >= 0) meta.SerriaTierAlmenaras = data.serriaTierAlmenaras;
    if (data.serriaTierPalacio >= 0) meta.SerriaTierPalacio = data.serriaTierPalacio;
    if (data.serriaTierCuartel >= 0) meta.SerriaTierCuartel = data.serriaTierCuartel;
    if (data.serriaTierGranjas >= 0) meta.SerriaTierGranjas = data.serriaTierGranjas;
    if (data.serriaTierBarricadas >= 0) meta.SerriaTierBarricadas = data.serriaTierBarricadas;
    if (data.serriaTierTemplo >= 0) meta.SerriaTierTemplo = data.serriaTierTemplo;

    meta.SerriaPuntosAlmacenadosBarcos = Mathf.Max(0, data.serriaPuntosAlmacenadosBarcos);
    meta.SerriaPuntosAlmacenadosAlmenaras = Mathf.Max(0, data.serriaPuntosAlmacenadosAlmenaras);
    meta.SerriaPuntosAlmacenadosPalacio = Mathf.Max(0, data.serriaPuntosAlmacenadosPalacio);
    meta.SerriaPuntosAlmacenadosCuartel = Mathf.Max(0, data.serriaPuntosAlmacenadosCuartel);
    meta.SerriaPuntosAlmacenadosGranjas = Mathf.Max(0, data.serriaPuntosAlmacenadosGranjas);
    meta.SerriaPuntosAlmacenadosBarricadas = Mathf.Max(0, data.serriaPuntosAlmacenadosBarricadas);
    meta.SerriaPuntosAlmacenadosTemplo = Mathf.Max(0, data.serriaPuntosAlmacenadosTemplo);
  }

  private void AplicarEstadoBaseCampaniaDesdeSave(CampaignSaveData data)
  {
    ultimasAparienciasAlternativasPorClase.Clear();
    if (data == null)
    {
      return;
    }

    horasTotales = Math.Max(0d, data.horasTotales);
    antorchasEncendidas = data.antorchasEncendidas;
    progresoFatigaHoras = Mathf.Max(0f, data.progresoFatigaHoras);
    creditoPrevencionAlientoHoras = Mathf.Max(0f, data.creditoPrevencionAlientoHoras);
    estadisticaHorasViajadas = Mathf.Max(0f, data.horasViajadas);
    horasViajeActual = data.viajeEnCurso ? Mathf.Max(0f, data.viajeHorasTranscurridas) : 0f;
    viajeActualIncluyoNoche = data.viajeEnCurso && data.viajeActualIncluyoNoche;
    viajeClimaInicial = data.viajeEnCurso ? data.viajeClimaInicial : 0;
    multiplicadorVelocidadVisualViajeActual = data.viajeEnCurso
      ? Mathf.Max(0.01f, data.viajeMultiplicadorVelocidad)
      : 1f;
    emboscadaViajeCalculada = data.viajeEnCurso && data.emboscadaViajeCalculada;
    logEmboscadaViajePendiente = data.viajeEnCurso ? data.logEmboscadaViajePendiente : null;
    acumuladorEfectosSequitosHoras = Mathf.Repeat(Mathf.Max(0f, data.acumuladorEfectosSequitosHoras), 24f);
    combateHoraCapturada = data.combateHoraCapturada;
    combateNocturno = data.combateNocturno;
    descansoInterrumpidoPendiente = data.descansoInterrumpidoPendiente;
    descansoResultadosPendientes = data.descansoResultadosPendientes;
    descansoTuvoEmboscada = data.descansoTuvoEmboscada;
    descansoRitualElegible = data.descansoRitualElegible;
    descansoRitualPersonajeId = data.descansoRitualPersonajeId ?? string.Empty;
    descansoClimaInicial = data.descansoClimaInicial;
    descansoValorTarea = data.descansoValorTarea;
    descansoChanceExploracion = data.descansoChanceExploracion;
    descansoChanceEmboscada = data.descansoChanceEmboscada;
    descansoEmboscadaPendiente = data.descansoEmboscadaPendiente;
    descansoHorasHastaEmboscada = Mathf.Max(0f, data.descansoHorasHastaEmboscada);
    descansoTiradaEmboscada = data.descansoTiradaEmboscada;
    descansoHorasRestantes = Mathf.Max(0f, data.descansoHorasRestantes);
    descansoTareaCivil = data.descansoTareaCivil;
    descansoEnClaro = data.descansoEnClaro;
    descansoHoraCombate = data.descansoHoraCombate;
    continuacionDescansoGestionadaPorMenu = false;
    posicionCaravana = Mathf.Max(0, data.posicionCaravana);
    presagiosRegionActivaId = Mathf.Max(0, data.presagiosRegionId);
    presagiosActivosRegion.Clear();
    if (data.presagiosActivos != null)
    {
      presagiosActivosRegion.AddRange(data.presagiosActivos);
    }
    primeraBatallaPresagioEnemigosConsumida = data.primeraBatallaPresagioEnemigosConsumida;
    mejoraCaravanaAntorchas = NormalizarTierMejoraCaravana(data.mejoraCaravanaAntorchas);
    mejoraCaravanaAlforjas = NormalizarTierMejoraCaravana(data.mejoraCaravanaAlforjas);
    mejoraCaravanaTiendas = NormalizarTierMejoraCaravana(data.mejoraCaravanaTiendas);
    mejoraCaravanaCatalejos = NormalizarTierMejoraCaravana(data.mejoraCaravanaCatalejos);
    mejoraCaravanaAlmacen = NormalizarTierMejoraCaravana(data.mejoraCaravanaAlmacen);
    mejoraCaravanaDefensas = NormalizarTierMejoraCaravana(data.mejoraCaravanaDefensas);
    sequitoHerrerosMantArmasHoras = Mathf.Max(0f, data.sequitoHerrerosMantArmasHoras);
    sequitoHerrerosMantArmadurasHoras = Mathf.Max(0f, data.sequitoHerrerosMantArmadurasHoras);
    sequitoMercaderesTier = data.sequitoMercaderesTier;
    sequitoCuranderosMejoraCuracion = Mathf.Clamp(data.sequitoCuranderosMejoraCuracion, 0.10f, 0.30f);
    miliciasMejoras = data.miliciasMejoras;
    peligrozonaanterior = data.peligroZonaAnterior;
    pComercialSuministrosDisp = data.puestoComercialSuministrosDisp;
    pComercialMaterialesDisp = data.puestoComercialMaterialesDisp;
    pComercialBueyesDisp = data.puestoComercialBueyesDisp;
    estadisticaBatallasLibradas = Mathf.Max(0, data.estadisticaBatallasLibradas);
    estadisticaCivilesPerdidos = Mathf.Max(0, data.estadisticaCivilesPerdidos);
    estadisticaAsentamientosVisitados = Mathf.Max(0, data.estadisticaAsentamientosVisitados);
    MoviendoCaravana = false;
    nodoDestinoActual = null;
    if (estadosCaravana == null)
    {
      estadosCaravana = new EstadosCaravana();
    }
    estadosCaravana.RestaurarDesdeSave(data.estadosCaravana);
    transicionZonaEnCurso = false;
    BATALLA_EnCurso = data.batallaEnCurso;
    EMBOSCADA_EnCurso = data.emboscadaEnCurso;
    RestaurarUltimasAparienciasPorClaseDesdeSave(data);
  }

  private void RestaurarZonaDesdeSave(SaveFileData saveFileData)
  {
    CampaignSaveData data = saveFileData != null ? saveFileData.campaign : null;
    if (data == null || scAtributosZona == null)
    {
      return;
    }

    RestaurarEventosAleatoriosUsadosMapaDesdeSave(data);

    int? reliefSeedGuardado = saveFileData != null && saveFileData.version >= 4 ? data.reliefSeed : null;
    scAtributosZona.RestaurarZonaDesdeSave(data.zonaId, data.zonaFase, data.zonasEstado, reliefSeedGuardado);
    scAtributosZona.PasoVientoHelado_FuerzaKaleTav = data.pasoVientoHeladoFuerzaKaleTav;
    scAtributosZona.ActualizarLuzNedukazal();
  }

  void RestaurarEventosAleatoriosUsadosMapaDesdeSave(CampaignSaveData data)
  {
    eventosAleatoriosUsadosMapa.Clear();
    if (data == null || data.eventosAleatoriosUsadosMapa == null)
    {
      return;
    }

    foreach (int idEvento in data.eventosAleatoriosUsadosMapa)
    {
      if (idEvento <= 0 || eventosAleatoriosUsadosMapa.Contains(idEvento))
      {
        continue;
      }

      eventosAleatoriosUsadosMapa.Add(idEvento);
    }
  }

  public List<int> ObtenerEventosAleatoriosUsadosMapa()
  {
    return new List<int>(eventosAleatoriosUsadosMapa);
  }

  public void RegistrarEventoAleatorioUsadoEnMapa(int idEvento)
  {
    if (idEvento <= 0 || eventosAleatoriosUsadosMapa.Contains(idEvento))
    {
      return;
    }

    eventosAleatoriosUsadosMapa.Add(idEvento);
  }

  public void ResetearEventosAleatoriosUsadosMapa()
  {
    eventosAleatoriosUsadosMapa.Clear();
  }

  private void RestaurarMapaDesdeSave(SaveFileData saveFileData)
  {
    if (saveFileData == null || saveFileData.map == null || scMapaManager == null || scMapaManager.scContenedordeNodos == null)
    {
      return;
    }

    scMapaManager.ResetearYGenerarSiguienteZona();
    scMapaManager.AlinearNodosAlSueloActual();
    scMapaManager.scContenedordeNodos.RecolectarNodos();

    Dictionary<string, Nodo> nodosPorClave = new Dictionary<string, Nodo>();
    foreach (Nodo nodo in scMapaManager.scContenedordeNodos.listTodosNodos)
    {
      if (nodo == null)
      {
        continue;
      }

      nodosPorClave[BuildNodeKey(nodo.posXNodo, nodo.posYNodo)] = nodo;
    }

    Dictionary<string, NodeSaveData> savePorClave = new Dictionary<string, NodeSaveData>();
    if (saveFileData.map != null && saveFileData.map.nodes != null)
    {
      foreach (NodeSaveData nodeData in saveFileData.map.nodes)
      {
        if (nodeData == null)
        {
          continue;
        }

        savePorClave[BuildNodeKey(nodeData.x, nodeData.y)] = nodeData;
      }
    }

    foreach (KeyValuePair<string, Nodo> entry in nodosPorClave)
    {
      if (savePorClave.TryGetValue(entry.Key, out NodeSaveData nodeData))
      {
        entry.Value.RestaurarDesdeSave(nodeData, saveFileData.version);
      }
      else
      {
        entry.Value.gameObject.SetActive(false);
      }
    }

    if (saveFileData.map.nodes != null)
    {
      foreach (NodeSaveData nodeData in saveFileData.map.nodes)
      {
        if (nodeData == null || !nodeData.activo)
        {
          continue;
        }

        Nodo origen = BuscarNodoDesdeReferencia(new NodeReferenceSaveData { x = nodeData.x, y = nodeData.y }, nodosPorClave);
        if (origen == null || nodeData.conexiones == null)
        {
          continue;
        }

        foreach (CaminoConexionSaveData conexionData in nodeData.conexiones)
        {
          if (conexionData == null)
          {
            continue;
          }

          Nodo destino = BuscarNodoDesdeReferencia(
            new NodeReferenceSaveData { x = conexionData.destinoX, y = conexionData.destinoY },
            nodosPorClave);
          if (destino == null)
          {
            continue;
          }

          origen.ConectarConNodo(
            destino,
            false,
            false,
            true,
            false,
            conexionData.tipo,
            conexionData.costoMovimiento,
            conexionData.rutaHaciaAldea);

          CaminoConexion conexionRestaurada = origen.ObtenerConexionHacia(destino);
          if (conexionRestaurada != null)
          {
            conexionRestaurada.recorridoPorCaravana = conexionData.recorridoPorCaravana;
            if (saveFileData.version < 24
                && (conexionRestaurada.EsAtajoSubterraneo || conexionRestaurada.EsAtajoSuperficie))
            {
              destino.MarcarDescubiertoPorMecanicaEspecial();
            }
          }
        }
      }
    }

    scMapaManager.scContenedordeNodos.RecolectarNodos();
    scMapaManager.RestaurarEstadoVariedadDesdeSave(saveFileData.map);
    List<Nodo> settlementsForzados = new List<Nodo>();
    if (saveFileData.map.settlementsForzados != null)
    {
      foreach (NodeReferenceSaveData settlementRef in saveFileData.map.settlementsForzados)
      {
        Nodo settlement = BuscarNodoDesdeReferencia(settlementRef, nodosPorClave);
        if (settlement != null)
        {
          settlementsForzados.Add(settlement);
        }
      }
    }
    scMapaManager.RestaurarSettlementsForzados(settlementsForzados);
    scMapaManager.nodoActual = BuscarNodoDesdeReferencia(saveFileData.campaign != null ? saveFileData.campaign.nodoActual : null, nodosPorClave);
    if (scMapaManager.nodoActual == null)
    {
      scMapaManager.nodoActual = BuscarNodoDesdeReferencia(new NodeReferenceSaveData { x = 0, y = 0 }, nodosPorClave);
    }

    scMapaManager.PosicionarCaravanaEnNodoActual();
    if (scMapaManager.nodoActual != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }
  }

  private string BuildNodeKey(int x, int y)
  {
    return x + "_" + y;
  }

  private Nodo BuscarNodoDesdeReferencia(NodeReferenceSaveData referencia, Dictionary<string, Nodo> nodosPorClave)
  {
    if (referencia == null || referencia.x < 0 || nodosPorClave == null)
    {
      return null;
    }

    nodosPorClave.TryGetValue(BuildNodeKey(referencia.x, referencia.y), out Nodo nodo);
    return nodo;
  }

  private void RestaurarSequitosDesdeSave(SequitosSaveData data)
  {
    if (data == null || scMenuSequito == null)
    {
      return;
    }

    if (data.sequitosActivos != null)
    {
      foreach (int sequitoId in data.sequitosActivos)
      {
        scMenuSequito.AgregarSequito(sequitoId, false);
      }
    }

    if (scSequitoHerboristas != null)
    {
      scSequitoHerboristas.vecesEnClaro = data.herboristasVecesEnClaro;
      scSequitoHerboristas.cantBalsamoFort = data.herboristasCantBalsamoFort;
      scSequitoHerboristas.cantBalsamoReflej = data.herboristasCantBalsamoReflej;
      scSequitoHerboristas.cantBalsamoMental = data.herboristasCantBalsamoMental;
      scSequitoHerboristas.Actualizar();
    }

    if (scSequitoCronistas != null)
    {
      scSequitoCronistas.valorCambiosCronicas = data.cronistasValorCambios;
      scSequitoCronistas.yaVendioCronica = data.cronistasYaVendio;
      scSequitoCronistas.Actualizar();
    }

    if (scSequitoClerigos != null)
    {
      scSequitoClerigos.RestaurarZonaIdUltimaPlegaria(data.clerigosZonaIdUltimaPlegaria);
    }

    RestaurarMercaderesDesdeSave(data);

    SequitoCuranderos curanderos = scMenuSequito.ObtenerSequitoCuranderosActivo();
    if (curanderos != null)
    {
      curanderos.Actualizar();
    }
  }

  private void RestaurarMercaderesDesdeSave(SequitosSaveData data)
  {
    if (scSequitoMercaderes == null || data == null)
    {
      return;
    }

    scSequitoMercaderes.MarcarRestauradoDesdeSave();

    ItemDatabase itemDatabase = ItemSaveCatalog.GetRuntimeItemDatabase(this);
    foreach (Item itemExistente in scSequitoMercaderes.ItemsVendidos)
    {
      if (itemExistente != null)
      {
        Destroy(itemExistente.gameObject);
      }
    }

    scSequitoMercaderes.ItemsVendidos.Clear();

    if (data.mercaderesItemsVendidosIds != null)
    {
      foreach (string itemId in data.mercaderesItemsVendidosIds)
      {
        Item item = ItemSaveCatalog.InstantiateItemById(itemId, itemDatabase);
        if (item != null)
        {
          scSequitoMercaderes.ItemsVendidos.Add(item);
        }
      }
    }

    scSequitoMercaderes.Actualizar();
    scSequitoMercaderes.MostrarInventarioVenta();
  }

  private void RestaurarPartyDesdeSave(PartySaveData data, int saveVersion)
  {
    if (data == null || scMenuPersonajes == null)
    {
      return;
    }

    ItemDatabase itemDatabase = ItemSaveCatalog.GetRuntimeItemDatabase(this);
    if (data.characters != null)
    {
      foreach (CharacterSaveData characterData in data.characters)
      {
        if (characterData == null)
        {
          continue;
        }

        Personaje personaje = CrearPersonajeDesdeSave(characterData, itemDatabase, saveVersion);
        if (personaje != null)
        {
          scMenuPersonajes.listaPersonajes.Add(personaje);
        }
      }
    }

    RestaurarInventarioDesdeSave(data.inventoryItemIds, itemDatabase);

    RefrescarRetratosPersonajesCampania();
  }

  private void RestaurarInventarioDesdeSave(List<string> inventoryItemIds, ItemDatabase itemDatabase)
  {
    if (inventoryItemIds == null || scMenuPersonajes == null || scMenuPersonajes.scEquipo == null)
    {
      return;
    }

    foreach (string itemId in inventoryItemIds)
    {
      Item item = ItemSaveCatalog.InstantiateItemById(itemId, itemDatabase);
      if (item != null)
      {
        scMenuPersonajes.scEquipo.listInventario.Add(item.gameObject);
      }
    }
  }

  private Personaje CrearPersonajeDesdeSave(CharacterSaveData data, ItemDatabase itemDatabase, int saveVersion)
  {
    if (data == null || prefabGOPersonaje == null)
    {
      return null;
    }

    GameObject personajeGo = Instantiate(prefabGOPersonaje);
    Personaje personaje = personajeGo.GetComponent<Personaje>();
    if (personaje == null)
    {
      Destroy(personajeGo);
      return null;
    }

    int[] habilidadesGuardadas = data.habilidades ?? Array.Empty<int>();
    int[] actividadesGuardadas = data.actividades ?? Array.Empty<int>();

    AplicarDatosBasePersonajeDesdeSave(personaje, data, habilidadesGuardadas, actividadesGuardadas, saveVersion);
    AgregarHabilidadesIntrinsecasDeClase(personaje);
    AgregarActividadesBase(personaje);

    for (int i = 0; i < habilidadesGuardadas.Length; i++)
    {
      if (habilidadesGuardadas[i] > 0)
      {
        AgregarHabilidadDeClaseSegunSlot(personaje, i + 1, habilidadesGuardadas[i]);
      }
    }

    for (int i = 0; i < actividadesGuardadas.Length; i++)
    {
      if (actividadesGuardadas[i] > 0)
      {
        AgregarActividadDeClaseSegunSlot(personaje, i + 1);
      }
    }

    RestaurarEquipmentDesdeSave(personaje, data.equipment, itemDatabase, saveVersion);
    if (scMenuPersonajes != null && scMenuPersonajes.scEquipo != null)
    {
      scMenuPersonajes.scEquipo.ActualizarEquipo(personaje);
    }

    return personaje;
  }

  private void AplicarDatosBasePersonajeDesdeSave(Personaje personaje, CharacterSaveData data, int[] habilidadesGuardadas, int[] actividadesGuardadas, int saveVersion)
  {
    personaje.SetPersistentId(string.IsNullOrWhiteSpace(data.id) ? Guid.NewGuid().ToString("N") : data.id);
    personaje.sNombre = data.nombre;
    personaje.IDClase = data.idClase;
    personaje.idRetrato = data.idRetrato;
    personaje.indiceAparienciaAlternativa = data.indiceAparienciaAlternativa;
    personaje.aparienciaAlternativaResuelta = data.aparienciaAlternativaResuelta;
    if (saveVersion < 21)
    {
      personaje.aparienciaAlternativaResuelta = false;
      personaje.indiceAparienciaAlternativa = Personaje.IndiceAparienciaBase;
    }
    personaje.iPuestoDeseado = data.puestoDeseado;
    personaje.fVidaActual = data.vidaActual;
    personaje.fVidaMaxima = data.vidaMaxima;
    personaje.fExperienciaActual = data.experienciaActual;
    personaje.fNivelActual = data.nivelActual;
    personaje.iFuerza = data.fuerza;
    personaje.iAgi = data.agi;
    personaje.iPoder = data.poder;
    personaje.iIniciativa = data.iniciativa;
    personaje.iApMax = data.apMax;
    personaje.iValMax = data.valMax;
    personaje.iArmadura = data.armadura;
    personaje.iDefensa = data.defensa;
    personaje.iTSReflejo = data.tsReflejo;
    personaje.iTSFortaleza = data.tsFortaleza;
    personaje.iTSMental = data.tsMental;
    personaje.iResFuego = data.resFuego;
    personaje.iResRayo = data.resRayo;
    personaje.iResHielo = data.resHielo;
    personaje.iResArcano = data.resArcano;
    personaje.iResAcido = data.resAcido;
    personaje.iResNecro = data.resNecro;
    personaje.iResDivino = data.resDivino;
    personaje.fCritRango = data.critRango;
    personaje.fCritDanio = data.critDanio;
    personaje.fBonusAtaque = data.bonusAtaque;
    personaje.sinCooldownDebug = data.sinCooldownDebug;
    personaje.Habilidad_1 = habilidadesGuardadas.Length > 0 ? habilidadesGuardadas[0] : 0;
    personaje.Habilidad_2 = habilidadesGuardadas.Length > 1 ? habilidadesGuardadas[1] : 0;
    personaje.Habilidad_3 = habilidadesGuardadas.Length > 2 ? habilidadesGuardadas[2] : 0;
    personaje.Habilidad_4 = habilidadesGuardadas.Length > 3 ? habilidadesGuardadas[3] : 0;
    personaje.Habilidad_5 = habilidadesGuardadas.Length > 4 ? habilidadesGuardadas[4] : 0;
    personaje.Habilidad_6 = habilidadesGuardadas.Length > 5 ? habilidadesGuardadas[5] : 0;
    personaje.Habilidad_7 = habilidadesGuardadas.Length > 6 ? habilidadesGuardadas[6] : 0;
    personaje.Habilidad_8 = habilidadesGuardadas.Length > 7 ? habilidadesGuardadas[7] : 0;
    personaje.Habilidad_9 = habilidadesGuardadas.Length > 8 ? habilidadesGuardadas[8] : 0;
    personaje.Habilidad_10 = habilidadesGuardadas.Length > 9 ? habilidadesGuardadas[9] : 0;
    personaje.Actividad_1 = actividadesGuardadas.Length > 0 ? actividadesGuardadas[0] : 0;
    personaje.Actividad_2 = actividadesGuardadas.Length > 1 ? actividadesGuardadas[1] : 0;
    personaje.Actividad_3 = actividadesGuardadas.Length > 2 ? actividadesGuardadas[2] : 0;
    personaje.ActividadSeleccionada = data.actividadSeleccionada;
    personaje.ActividadFijada = data.actividadFijada;
    personaje.NivelPuntoAtributo = data.nivelPuntoAtributo;
    personaje.NivelPuntoTS = data.nivelPuntoTS;
    personaje.NivelPuntoHabilidad = data.nivelPuntoHabilidad;
    personaje.NivelNuevaHabilidadBase = data.nivelNuevaHabilidadBase;
    personaje.NormalizarPuntosPendientesPorNivelActual();
    personaje.SetCampFatigado(data.campFatigado);
    personaje.Camp_Herido = data.campHerido;
    personaje.RestaurarEstadosCampaniaHorarios(
      data.campEnfermoHoras,
      data.campBendecidoHoras,
      (EstadoMoralCampania)Mathf.Clamp(data.campMoralEstado, -1, 1),
      data.campMoralHoras);
    personaje.RestaurarProgresoActividades(data.progresoActividades);
    personaje.Camp_Avergonzado = data.campAvergonzado;
    personaje.Camp_Muerto = data.campMuerto;
    personaje.Camp_Corrupto = data.campCorrupto;
    personaje.TraitHeroeLocalCivilesOtorgados = data.traitHeroeLocalCivilesOtorgados;
    personaje.TraitHeroeLocalPenalidadMuerteAplicada = data.traitHeroeLocalPenalidadMuerteAplicada;
    personaje.TraitLiderCaravanaPenalidadMuerteAplicada = data.traitLiderCaravanaPenalidadMuerteAplicada;
    personaje.TraitEjemploASeguirAplicado = data.traitEjemploASeguirAplicado;
    personaje.TraitHerenciaItemOtorgado = data.traitHerenciaItemOtorgado;
    personaje.HorasViajadas = Mathf.Max(0f, data.horasViajadas);
    personaje.EnemigosEliminados = data.enemigosEliminados;
    personaje.DanioHecho = data.danioHecho;
    personaje.DanioRecibido = data.danioRecibido;
    personaje.VecesDerribado = data.vecesDerribado;
    personaje.aRasgos = new int[Mathf.Max(300, data.rasgos != null ? data.rasgos.Length : 300)];
    if (data.rasgos != null)
    {
      Array.Copy(data.rasgos, personaje.aRasgos, Mathf.Min(personaje.aRasgos.Length, data.rasgos.Length));
    }

    if (!personaje.PuedeRealizarActividades())
    {
      personaje.ActividadSeleccionada = 1; //descansa
    }

    personaje.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    personaje.InicializarEscaladoResElementalPorPoderSiHaceFalta();
    SincronizarAparienciaVisualPersonaje(personaje);
  }

  private void RestaurarEquipmentDesdeSave(Personaje personaje, EquipmentSaveData equipment, ItemDatabase itemDatabase, int saveVersion)
  {
    if (personaje == null)
    {
      return;
    }

    if (equipment != null)
    {
      personaje.itemArma = ItemSaveCatalog.InstantiateItemById(equipment.armaItemId, itemDatabase) as Arma;
      personaje.itemArmadura = ItemSaveCatalog.InstantiateItemById(equipment.armaduraItemId, itemDatabase) as Armadura;
      personaje.Accesorio1 = ItemSaveCatalog.InstantiateItemById(equipment.accesorio1ItemId, itemDatabase) as Accesorio;
      personaje.Accesorio2 = ItemSaveCatalog.InstantiateItemById(equipment.accesorio2ItemId, itemDatabase) as Accesorio;
      personaje.Consumible1 = ItemSaveCatalog.InstantiateItemById(equipment.consumible1ItemId, itemDatabase) as Consumible;
      personaje.Consumible2 = ItemSaveCatalog.InstantiateItemById(equipment.consumible2ItemId, itemDatabase) as Consumible;
    }

    if (saveVersion < 5)
    {
      RestaurarEquipoInicialLegacySiFalta(personaje, equipment, itemDatabase);
    }
  }

  private void RestaurarEquipoInicialLegacySiFalta(Personaje personaje, EquipmentSaveData equipment, ItemDatabase itemDatabase)
  {
    if (personaje == null || scContprefab == null)
    {
      return;
    }

    bool faltaArma = personaje.itemArma == null && (equipment == null || string.IsNullOrWhiteSpace(equipment.armaItemId));
    bool faltaArmadura = personaje.itemArmadura == null && (equipment == null || string.IsNullOrWhiteSpace(equipment.armaduraItemId));
    if (!faltaArma && !faltaArmadura)
    {
      return;
    }

    switch (personaje.IDClase)
    {
      case 1:
        if (faltaArma && scContprefab.armaMandoble != null)
        {
          personaje.itemArma = Instantiate(scContprefab.armaMandoble);
        }

        if (faltaArmadura && scContprefab.Coraza != null)
        {
          personaje.itemArmadura = Instantiate(scContprefab.Coraza);
        }
        break;
      case 2:
        if (faltaArma && scContprefab.armaArcoLargo != null)
        {
          personaje.itemArma = Instantiate(scContprefab.armaArcoLargo);
        }

        if (faltaArmadura && scContprefab.ArmaduraCuero != null)
        {
          personaje.itemArmadura = Instantiate(scContprefab.ArmaduraCuero);
        }
        break;
      case 3:
        if (faltaArma && scContprefab.armaBaculoPurificador != null)
        {
          personaje.itemArma = Instantiate(scContprefab.armaBaculoPurificador);
        }
        break;
      case 4:
        if (faltaArma && scContprefab.armaEspadaCorta != null)
        {
          personaje.itemArma = Instantiate(scContprefab.armaEspadaCorta);
        }

        if (faltaArmadura && scContprefab.ArmaduraCueroReforzado != null)
        {
          personaje.itemArmadura = Instantiate(scContprefab.ArmaduraCueroReforzado);
        }
        break;
      case 6:
        if (faltaArma && scContprefab.armaEstoque != null)
        {
          personaje.itemArma = Instantiate(scContprefab.armaEstoque);
        }

        if (faltaArmadura && scContprefab.ArmaduraGambeson != null)
        {
          personaje.itemArmadura = Instantiate(scContprefab.ArmaduraGambeson);
        }
        break;
    }

    AsignarPersistentItemIdSiExiste(personaje.itemArma, itemDatabase);
    AsignarPersistentItemIdSiExiste(personaje.itemArmadura, itemDatabase);
  }

  private void AsignarPersistentItemIdSiExiste(Item item, ItemDatabase itemDatabase)
  {
    if (item == null)
    {
      return;
    }

    ItemSaveCatalog.ResolveItemId(item, itemDatabase);
  }

  private void RestaurarSeleccionBatallaDesdeSave(PartySaveData data)
  {
    LimpiarSeleccionBatallaParaCarga();

    if (data == null || scAdministradorEscenas == null || data.selectedBattleCharacterIds == null || scMenuPersonajes == null)
    {
      return;
    }

    List<Personaje> seleccion = new List<Personaje>();
    foreach (string id in data.selectedBattleCharacterIds)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        continue;
      }

      Personaje personaje = scMenuPersonajes.listaPersonajes.Find(p => p != null && p.GetPersistentId() == id);
      if (personaje != null && !seleccion.Contains(personaje))
      {
        seleccion.Add(personaje);
      }
    }

    scAdministradorEscenas.Personaje1 = seleccion.Count > 0 ? seleccion[0] : null;
    scAdministradorEscenas.Personaje2 = seleccion.Count > 1 ? seleccion[1] : null;
    scAdministradorEscenas.Personaje3 = seleccion.Count > 2 ? seleccion[2] : null;
    scAdministradorEscenas.Personaje4 = seleccion.Count > 3 ? seleccion[3] : null;
  }

  private void FinalizarCargaCampania(SaveFileData saveFileData)
  {
    if (saveFileData == null)
    {
      return;
    }

    AplicarSpriteClimaDesdeEstadoActual();
    SincronizarVisualesMenuDescansoClima();
    RecalcularPreciosPuestoComercialDesdeEstadoActual();
    ActualizarDescripcionesPuestoComercialDesdeEstadoActual();
    ActualizarPuestoComercial();

    if (scMenuPersonajes != null && scMenuPersonajes.listaPersonajes.Count > 0)
    {
      scMenuPersonajes.pSel = scMenuPersonajes.pSel != null ? scMenuPersonajes.pSel : scMenuPersonajes.listaPersonajes[0];
      scMenuPersonajes.ActualizarLista();
      scMenuPersonajes.ActualizarInfo();
    }

    if (scMapaManager != null && scMapaManager.nodoActual != null)
    {
      scMapaManager.PosicionarCaravanaEnNodoActual();
      scMapaManager.RefrescarVisibilidadExploracion();
    }

    RestaurarBitacoraDesdeSave(saveFileData != null ? saveFileData.campaign : null);

    ActualizarTextoDia();

    if (scAtributosZona != null)
    {
      scAtributosZona.ActualizarLuzNedukazal();
    }

    RefrescarVfxClimaCalor();
    RefrescarUiSequitosTrasCarga();
    AjustarDificultad();
    AsegurarAsentamientoManager();

    if (asentamientoManager != null)
    {
      asentamientoManager.RestaurarEstadoDesdeSave(saveFileData.campaign);
    }

    if (saveFileData.campaign != null && saveFileData.campaign.viajeEnCurso)
    {
      StartCoroutine(ReanudarViajeTrasCarga(saveFileData.campaign));
    }
    else if ((descansoInterrumpidoPendiente || descansoResultadosPendientes) && BATALLA_EnCurso <= 0)
    {
      StartCoroutine(CompletarContinuacionDescansoTrasCarga());
    }
  }

  private IEnumerator ReanudarViajeTrasCarga(CampaignSaveData data)
  {
    yield return null;
    if (data == null || scMapaManager == null || scMapaManager.nodoActual == null || scMapaManager.scContenedordeNodos == null)
    {
      yield break;
    }

    Nodo destino = null;
    NodeReferenceSaveData referenciaDestino = data.nodoDestinoActual;
    foreach (Nodo nodo in scMapaManager.scContenedordeNodos.listTodosNodos)
    {
      if (nodo != null
          && referenciaDestino != null
          && nodo.posXNodo == referenciaDestino.x
          && nodo.posYNodo == referenciaDestino.y)
      {
        destino = nodo;
        break;
      }
    }

    if (destino == null || scMapaManager.nodoActual.ObtenerConexionHacia(destino) == null)
    {
      horasViajeActual = 0f;
      multiplicadorVelocidadVisualViajeActual = 1f;
      creditoPrevencionAlientoHoras = 0f;
      estadosCaravana?.FinalizarViajeActual();
      Debug.LogWarning("[CampaignManager] No se pudo reanudar el viaje guardado porque su conexión ya no existe.");
      yield break;
    }

    nodoDestinoActual = destino;
    MoviendoCaravana = true;
    pausandoTextoDistanciaAliento = true;
    if (menuDescanso != null)
    {
      menuDescanso.SetActive(false);
    }
    if (sunController != null)
    {
      sunController.OnTravelStart();
    }
    if (animCaravana != null)
    {
      animCaravana.SetBool("IsWalking", true);
      animCaravana.speed = multiplicadorVelocidadVisualViajeActual;
    }
    IniciarSonidoMovimientoCaravana(Mathf.Max(0.05f, sfxMovimientoFadeIn));

    if (!destino.ReanudarViajeDesdeGuardado(scMapaManager.nodoActual, data.viajeProgresoNormalizado))
    {
      MoviendoCaravana = false;
      nodoDestinoActual = null;
      horasViajeActual = 0f;
      pausandoTextoDistanciaAliento = false;
      DetenerSonidoMovimientoCaravanaIntro();
      Debug.LogWarning("[CampaignManager] No se pudo iniciar la rutina del viaje guardado.");
    }
    ActualizarBotonesAccionNodoActual();
  }

  private void RestaurarBitacoraDesdeSave(CampaignSaveData data)
  {
    if (logDeCampania == null)
    {
      return;
    }

    logDeCampania.ImportarSaveData(
      data != null ? data.bitacora : null,
      numeroTurno,
      GetEsperanzaActual(),
      GetOroActuales(),
      GetMaterialesActuales(),
      GetSuministrosActuales(),
      intTipoClima);
    logDeCampania.SetDiaActual(numeroTurno);
  }

  private void PrepararBitacoraDiaActualSiCorresponde()
  {
    if (logDeCampania == null)
    {
      return;
    }

    logDeCampania.AsegurarDiaActualConSnapshotSiFalta(
      numeroTurno,
      GetEsperanzaActual(),
      GetOroActuales(),
      GetMaterialesActuales(),
      GetSuministrosActuales(),
      intTipoClima);
    logDeCampania.SetDiaActual(numeroTurno);
  }

  private void RefrescarUiSequitosTrasCarga()
  {
    if (scSequitoHerboristas != null)
    {
      scSequitoHerboristas.Actualizar();
    }

    if (scSequitoCronistas != null)
    {
      scSequitoCronistas.Actualizar();
    }

    if (scSequitoMercaderes != null)
    {
      scSequitoMercaderes.Actualizar();
      scSequitoMercaderes.MostrarInventarioVenta();
    }

    if (scMenuSequito != null)
    {
      SequitoCuranderos curanderos = scMenuSequito.ObtenerSequitoCuranderosActivo();
      if (curanderos != null)
      {
        curanderos.Actualizar();
      }
    }
  }

  private Sprite ObtenerRetratoCampaniaPorId(int idRetrato, int idClase)
  {
    if (scMenuPersonajes == null)
    {
      return null;
    }

    switch (idRetrato)
    {
      case 1: return scMenuPersonajes.Male001;
      case 5: return scMenuPersonajes.Male003;
      case 6: return scMenuPersonajes.Female001;
      case 7: return scMenuPersonajes.Male004;
      case 8: return scMenuPersonajes.Male005;
      case 9: return scMenuPersonajes.Female002;
    }

    switch (idClase)
    {
      case 1: return scMenuPersonajes.Male001;
      case 2: return scMenuPersonajes.Male003;
      case 3: return scMenuPersonajes.Female001;
      case 4: return scMenuPersonajes.Male004;
      case 5: return scMenuPersonajes.Male005;
      case 6: return scMenuPersonajes.Female002;
      default: return scMenuPersonajes.Male001;
    }
  }

  BattleManager ObtenerBattleManagerParaResolverApariencias()
  {
    if (scAdministradorEscenas != null && scAdministradorEscenas.EscenaBatalla != null)
    {
      BattleManager battleManagerEscena = scAdministradorEscenas.EscenaBatalla.GetComponentInChildren<BattleManager>(true);
      if (battleManagerEscena != null)
      {
        return battleManagerEscena;
      }
    }

    return BattleManager.Instance;
  }

  Unidad ObtenerPrefabClaseParaResolverApariencia(int idClase)
  {
    BattleManager battleManager = ObtenerBattleManagerParaResolverApariencias();
    if (battleManager == null)
    {
      return null;
    }

    GameObject prefabClase = null;
    switch (idClase)
    {
      case 1: prefabClase = battleManager.prefabUnidadCaballero; break;
      case 2: prefabClase = battleManager.prefabUnidadExplorador; break;
      case 3: prefabClase = battleManager.prefabUnidadPurificadora; break;
      case 4: prefabClase = battleManager.prefabUnidadAcechador; break;
      case 5: prefabClase = battleManager.prefabUnidadCanalizador; break;
      case 6: prefabClase = battleManager.prefabUnidadDuelista; break;
    }

    return prefabClase != null ? prefabClase.GetComponent<Unidad>() : null;
  }

  public void SincronizarAparienciaVisualPersonaje(Personaje personaje)
  {
    if (personaje == null)
    {
      return;
    }

    Unidad prefabClase = ObtenerPrefabClaseParaResolverApariencia(personaje.IDClase);
    if (prefabClase == null)
    {
      if (personaje.spRetrato == null)
      {
        personaje.spRetrato = ObtenerRetratoCampaniaPorId(personaje.idRetrato, personaje.IDClase);
      }
      return;
    }

    int cantidadApariencias = prefabClase.ObtenerCantidadAparienciasAlternativas();
    if (cantidadApariencias <= 0)
    {
      personaje.indiceAparienciaAlternativa = Personaje.IndiceAparienciaBase;
      personaje.aparienciaAlternativaResuelta = true;
      personaje.spRetrato = ObtenerRetratoCampaniaPorId(personaje.idRetrato, personaje.IDClase);
      return;
    }

    if (!personaje.aparienciaAlternativaResuelta || !prefabClase.EsIndiceAparienciaAlternativaValido(personaje.indiceAparienciaAlternativa))
    {
      personaje.indiceAparienciaAlternativa = ElegirIndiceAparienciaAlternativaSinRepetirConsecutiva(personaje.IDClase, prefabClase);
      personaje.aparienciaAlternativaResuelta = true;
      RegistrarUltimaAparienciaAlternativaClase(personaje.IDClase, personaje.indiceAparienciaAlternativa);
    }

    Sprite retratoApariencia = prefabClase.ObtenerRetratoAparienciaAlternativa(personaje.indiceAparienciaAlternativa);
    personaje.spRetrato = retratoApariencia != null
      ? retratoApariencia
      : ObtenerRetratoCampaniaPorId(personaje.idRetrato, personaje.IDClase);
  }

  void GuardarUltimasAparienciasPorClaseEnSave(CampaignSaveData data)
  {
    if (data == null)
    {
      return;
    }

    data.ultimasAparienciasPorClase.Clear();
    foreach (KeyValuePair<int, int> entry in ultimasAparienciasAlternativasPorClase)
    {
      if (entry.Key <= 0)
      {
        continue;
      }

      data.ultimasAparienciasPorClase.Add(new UltimaAparienciaClaseSaveData
      {
        idClase = entry.Key,
        indiceAparienciaAlternativa = entry.Value
      });
    }
  }

  void RestaurarUltimasAparienciasPorClaseDesdeSave(CampaignSaveData data)
  {
    if (data == null || data.ultimasAparienciasPorClase == null)
    {
      return;
    }

    foreach (UltimaAparienciaClaseSaveData entry in data.ultimasAparienciasPorClase)
    {
      if (entry == null || entry.idClase <= 0)
      {
        continue;
      }

      ultimasAparienciasAlternativasPorClase[entry.idClase] = entry.indiceAparienciaAlternativa;
    }
  }

  void RegistrarUltimaAparienciaAlternativaClase(int idClase, int indiceApariencia)
  {
    if (idClase <= 0)
    {
      return;
    }

    ultimasAparienciasAlternativasPorClase[idClase] = indiceApariencia;
  }

  int ElegirIndiceAparienciaAlternativaSinRepetirConsecutiva(int idClase, Unidad prefabClase)
  {
    if (prefabClase == null)
    {
      return Personaje.IndiceAparienciaBase;
    }

    List<int> indicesDisponibles = prefabClase.ObtenerIndicesAparienciasAlternativasDisponibles();
    if (indicesDisponibles == null || indicesDisponibles.Count == 0)
    {
      return Personaje.IndiceAparienciaBase;
    }

    if (indicesDisponibles.Count == 1)
    {
      return indicesDisponibles[0];
    }

    if (!ultimasAparienciasAlternativasPorClase.TryGetValue(idClase, out int ultimoIndiceUsado) || !indicesDisponibles.Contains(ultimoIndiceUsado))
    {
      return indicesDisponibles[UnityEngine.Random.Range(0, indicesDisponibles.Count)];
    }

    List<int> indicesFiltrados = new List<int>();
    for (int i = 0; i < indicesDisponibles.Count; i++)
    {
      if (indicesDisponibles[i] != ultimoIndiceUsado)
      {
        indicesFiltrados.Add(indicesDisponibles[i]);
      }
    }

    if (indicesFiltrados.Count == 0)
    {
      return ultimoIndiceUsado;
    }

    return indicesFiltrados[UnityEngine.Random.Range(0, indicesFiltrados.Count)];
  }

  private void AgregarHabilidadesIntrinsecasDeClase(Personaje personaje)
  {
    if (personaje == null)
    {
      return;
    }

    switch (personaje.IDClase)
    {
      case 1:
        AsignarNivelHabilidad(AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONCorajeInquebrantable)) as Habilidad, -1);
        break;
      case 2:
        AsignarNivelHabilidad(AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONPasoCauteloso)) as Habilidad, -1);
        AsignarNivelHabilidad(AgregarComponenteSiFalta(personaje.gameObject, typeof(ImprovisarFlechas)) as Habilidad, 1);
        AgregarComponenteSiFalta(personaje.gameObject, typeof(CorteDaga));
        break;
      case 3:
        AsignarNivelHabilidad(AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONAlmaEndeble)) as Habilidad, -1);
        AsignarNivelHabilidad(AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONFervorConjunto)) as Habilidad, -1);
        break;
      case 4:
        AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONSueldo));
        AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONSigiloso));
        AgregarComponenteSiFalta(personaje.gameObject, typeof(TiroBallestaDeMano));
        break;
      case 5:
        AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONSobrecarga));
        AgregarComponenteSiFalta(personaje.gameObject, typeof(AcumularEnergia));
        AgregarComponenteSiFalta(personaje.gameObject, typeof(DescargaArcana));
        break;
      case 6:
        AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONPasoLigero));
        AgregarComponenteSiFalta(personaje.gameObject, typeof(REPRESENTACIONPosturaDemandante));
        break;
    }
  }

  private void AgregarHabilidadDeClaseSegunSlot(Personaje personaje, int slot, int nivel)
  {
    Type tipo = ResolverTipoHabilidadDeClase(personaje != null ? personaje.IDClase : 0, slot);
    Habilidad habilidad = AgregarComponenteSiFalta(personaje != null ? personaje.gameObject : null, tipo) as Habilidad;
    AsignarNivelHabilidad(habilidad, nivel);
  }

  private void CompletarHeroeDebugConTodasLasHabilidades(Personaje personaje)
  {
    if (personaje == null)
    {
      return;
    }

    AgregarHabilidadesIntrinsecasDeClase(personaje);
    AsignarNivelesHabilidadesIntrinsecasDebug(personaje);

    switch (personaje.IDClase)
    {
      case 1:
        AgregarHabilidadDebug(personaje, 1, typeof(REPRESENTACIONAcorazado));
        AgregarHabilidadDebug(personaje, 2, typeof(GritoMotivador));
        AgregarHabilidadDebug(personaje, 3, typeof(CorteHorizontal));
        AgregarHabilidadDebug(personaje, 4, typeof(PrimerosAuxilios));
        AgregarHabilidadDebug(personaje, 5, typeof(REPRESENTACIONDeterminacion));
        AgregarHabilidadDebug(personaje, 6, typeof(Partir));
        AgregarHabilidadDebug(personaje, 7, typeof(PosturaDefensiva));
        AgregarHabilidadDebug(personaje, 8, typeof(SiguesTu));
        break;
      case 2:
        AgregarHabilidadDebug(personaje, 1, typeof(REPRESENTACIONVistaLejana));
        AgregarHabilidadDebug(personaje, 2, typeof(REPRESENTACIONAcrobatico));
        AgregarHabilidadDebug(personaje, 3, typeof(MarcarPresa));
        AgregarHabilidadDebug(personaje, 4, typeof(DisparoPotente));
        AgregarHabilidadDebug(personaje, 5, typeof(Vigilancia));
        AgregarHabilidadDebug(personaje, 6, typeof(Acechar));
        AgregarHabilidadDebug(personaje, 7, typeof(Fogata));
        break;
      case 3:
        AgregarHabilidadDebug(personaje, 1, typeof(REPRESENTACIONAuraSagrada));
        AgregarHabilidadDebug(personaje, 2, typeof(REPRESENTACIONEcosDivinos));
        AgregarHabilidadDebug(personaje, 3, typeof(Enmendar));
        AgregarHabilidadDebug(personaje, 4, typeof(LuzCegadora));
        AgregarHabilidadDebug(personaje, 5, typeof(PilaresDeLuz));
        AgregarHabilidadDebug(personaje, 6, typeof(SalmoPurificador));
        AgregarHabilidadDebug(personaje, 7, typeof(LlamaDivina));
        AgregarHabilidadDebug(personaje, 8, typeof(CastigaraLosMalvados));
        break;
      case 4:
        AgregarHabilidadDebug(personaje, 1, typeof(REPRESENTACIONMaestriaBallesta));
        AgregarHabilidadDebug(personaje, 2, typeof(REPRESENTACIONMaestriaEspadaCorta));
        AgregarHabilidadDebug(personaje, 3, typeof(DisparoEnvenenado));
        AgregarHabilidadDebug(personaje, 4, typeof(CorteIncapacitante));
        AgregarHabilidadDebug(personaje, 5, typeof(BombaDeHumo));
        AgregarHabilidadDebug(personaje, 6, typeof(Asesinar));
        AgregarHabilidadDebug(personaje, 7, typeof(Distraer));
        AgregarHabilidadDebug(personaje, 8, typeof(ArrojarAbrojos));
        break;
      case 5:
        AgregarHabilidadDebug(personaje, 1, typeof(REPRESENTACIONAcumulacionProtegida));
        AgregarHabilidadDebug(personaje, 2, typeof(DescargaDePoder));
        AgregarHabilidadDebug(personaje, 3, typeof(Instatransporte));
        AgregarHabilidadDebug(personaje, 4, typeof(AcumulacionInestable));
        AgregarHabilidadDebug(personaje, 5, typeof(HojaDeEnergia));
        AgregarHabilidadDebug(personaje, 6, typeof(EscudoEnergetico));
        AgregarHabilidadDebug(personaje, 7, typeof(SifonArcano));
        AgregarHabilidadDebug(personaje, 8, typeof(REPRESENTACIONExcesoDePoder));
        break;
      case 6:
        AgregarHabilidadDebug(personaje, 1, typeof(REPRESENTACIONAtaquesReveladores));
        AgregarHabilidadDebug(personaje, 2, typeof(REPRESENTACIONEvasionMaestra));
        AgregarHabilidadDebug(personaje, 3, typeof(CargaDeEstoque));
        AgregarHabilidadDebug(personaje, 4, typeof(Riposte));
        AgregarHabilidadDebug(personaje, 5, typeof(AFondo));
        AgregarHabilidadDebug(personaje, 6, typeof(EnGarde));
        AgregarHabilidadDebug(personaje, 7, typeof(PuntaHiriente));
        AgregarHabilidadDebug(personaje, 8, typeof(RecuperarAire));
        AgregarHabilidadDebug(personaje, 9, typeof(PresenciaProvocadora));
        AgregarHabilidadDebug(personaje, 10, typeof(REPRESENTACIONDanzaDelEstoque));
        break;
    }
  }

  private void AsignarNivelesHabilidadesIntrinsecasDebug(Personaje personaje)
  {
    if (personaje == null)
    {
      return;
    }

    switch (personaje.IDClase)
    {
      case 1:
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONCorajeInquebrantable>(), -1);
        break;
      case 2:
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONPasoCauteloso>(), -1);
        AsignarNivelHabilidad(personaje.GetComponent<ImprovisarFlechas>(), 1);
        break;
      case 3:
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONAlmaEndeble>(), -1);
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONFervorConjunto>(), -1);
        break;
      case 4:
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONSueldo>(), -1);
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONSigiloso>(), -1);
        AsignarNivelHabilidad(personaje.GetComponent<TiroBallestaDeMano>(), -1);
        break;
      case 5:
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONSobrecarga>(), -1);
        AsignarNivelHabilidad(personaje.GetComponent<AcumularEnergia>(), -1);
        AsignarNivelHabilidad(personaje.GetComponent<DescargaArcana>(), -1);
        break;
      case 6:
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONPasoLigero>(), -1);
        AsignarNivelHabilidad(personaje.GetComponent<REPRESENTACIONPosturaDemandante>(), -1);
        break;
    }
  }

  private void AgregarHabilidadDebug(Personaje personaje, int slot, Type tipo, int nivel = 1)
  {
    if (personaje == null || tipo == null)
    {
      return;
    }

    AsignarSlotHabilidadPersonaje(personaje, slot, 1);
    AsignarNivelHabilidad(AgregarComponenteSiFalta(personaje.gameObject, tipo) as Habilidad, nivel);
  }

  private void AsignarSlotHabilidadPersonaje(Personaje personaje, int slot, int valor)
  {
    if (personaje == null)
    {
      return;
    }

    switch (slot)
    {
      case 1: personaje.Habilidad_1 = valor; break;
      case 2: personaje.Habilidad_2 = valor; break;
      case 3: personaje.Habilidad_3 = valor; break;
      case 4: personaje.Habilidad_4 = valor; break;
      case 5: personaje.Habilidad_5 = valor; break;
      case 6: personaje.Habilidad_6 = valor; break;
      case 7: personaje.Habilidad_7 = valor; break;
      case 8: personaje.Habilidad_8 = valor; break;
      case 9: personaje.Habilidad_9 = valor; break;
      case 10: personaje.Habilidad_10 = valor; break;
    }
  }

  private Type ResolverTipoHabilidadDeClase(int idClase, int slot)
  {
    switch (idClase)
    {
      case 1:
        switch (slot)
        {
          case 1: return typeof(REPRESENTACIONAcorazado);
          case 2: return typeof(GritoMotivador);
          case 3: return typeof(CorteHorizontal);
          case 4: return typeof(PrimerosAuxilios);
          case 5: return typeof(REPRESENTACIONDeterminacion);
          case 6: return typeof(Partir);
          case 7: return typeof(PosturaDefensiva);
          case 8: return typeof(SiguesTu);
        }
        break;
      case 2:
        switch (slot)
        {
          case 1: return typeof(REPRESENTACIONVistaLejana);
          case 2: return typeof(DisparoPotente);
          case 3: return typeof(REPRESENTACIONAcrobatico);
          case 4: return typeof(MarcarPresa);
          case 5: return typeof(Acechar);
          case 6: return typeof(Vigilancia);
          case 7: return typeof(Fogata);
        }
        break;
      case 3:
        switch (slot)
        {
          case 1: return typeof(REPRESENTACIONAuraSagrada);
          case 2: return typeof(REPRESENTACIONEcosDivinos);
          case 3: return typeof(SalmoPurificador);
          case 4: return typeof(LlamaDivina);
          case 5: return typeof(Enmendar);
          case 6: return typeof(LuzCegadora);
          case 7: return typeof(PilaresDeLuz);
          case 8: return typeof(CastigaraLosMalvados);
        }
        break;
      case 4:
        switch (slot)
        {
          case 1: return typeof(REPRESENTACIONMaestriaBallesta);
          case 2: return typeof(REPRESENTACIONMaestriaEspadaCorta);
          case 3: return typeof(DisparoEnvenenado);
          case 4: return typeof(CorteIncapacitante);
          case 5: return typeof(BombaDeHumo);
          case 6: return typeof(Asesinar);
          case 7: return typeof(Distraer);
          case 8: return typeof(ArrojarAbrojos);
        }
        break;
      case 5:
        switch (slot)
        {
          case 1: return typeof(REPRESENTACIONAcumulacionProtegida);
          case 2: return typeof(REPRESENTACIONExcesoDePoder);
          case 3: return typeof(DescargaDePoder);
          case 4: return typeof(Instatransporte);
          case 5: return typeof(AcumulacionInestable);
          case 6: return typeof(HojaDeEnergia);
          case 7: return typeof(EscudoEnergetico);
          case 8: return typeof(SifonArcano);
        }
        break;
      case 6:
        switch (slot)
        {
          case 1: return typeof(REPRESENTACIONAtaquesReveladores);
          case 2: return typeof(REPRESENTACIONEvasionMaestra);
          case 3: return typeof(CargaDeEstoque);
          case 4: return typeof(Riposte);
          case 5: return typeof(AFondo);
          case 6: return typeof(EnGarde);
          case 7: return typeof(PuntaHiriente);
          case 8: return typeof(RecuperarAire);
          case 9: return typeof(PresenciaProvocadora);
          case 10: return typeof(REPRESENTACIONDanzaDelEstoque);
        }
        break;
    }

    return null;
  }

  private void AgregarActividadesBase(Personaje personaje)
  {
    if (personaje == null)
    {
      return;
    }

    AgregarComponenteSiFalta(personaje.gameObject, typeof(Actividad_Descansar));
    AgregarComponenteSiFalta(personaje.gameObject, typeof(Actividad_Entrenar));
    AgregarComponenteSiFalta(personaje.gameObject, typeof(Actividad_Guardia));
  }

  private void AgregarActividadDeClaseSegunSlot(Personaje personaje, int slot)
  {
    Type tipo = ResolverTipoActividadDeClase(personaje != null ? personaje.IDClase : 0, slot);
    AgregarComponenteSiFalta(personaje != null ? personaje.gameObject : null, tipo);
  }

  private Type ResolverTipoActividadDeClase(int idClase, int slot)
  {
    switch (idClase)
    {
      case 1:
        switch (slot)
        {
          case 1: return typeof(Actividad_RelatosDeBatalla);
          case 2: return typeof(Actividad_MantenerArmadura);
          case 3: return typeof(Actividad_Vigilar);
        }
        break;
      case 2:
        switch (slot)
        {
          case 1: return typeof(Actividad_CazaNocturna);
          case 2: return typeof(Actividad_Exploracion);
          case 3: return typeof(Actividad_PrepararFlechas);
        }
        break;
      case 3:
        switch (slot)
        {
          case 1: return typeof(Actividad_RitualDeLimpieza);
          case 2: return typeof(Actividad_ColaborarConLosCuranderos);
          case 3: return typeof(Actividad_AyudarDesamparados);
        }
        break;
      case 4:
        switch (slot)
        {
          case 1: return typeof(Actividad_AfilarArmas);
          case 2: return typeof(Actividad_VigilarDesdeLasSombras);
          case 3: return typeof(Actividad_Coercion);
        }
        break;
      case 5:
        switch (slot)
        {
          case 1: return typeof(Actividad_ConcentracionArcana);
          case 2: return typeof(Actividad_Telekinesis);
          case 3: return typeof(Actividad_CrearSimboloArcanoProteccion);
        }
        break;
      case 6:
        switch (slot)
        {
          case 1: return typeof(Actividad_SiempreAlerta);
          case 2: return typeof(Actividad_Socializar);
          case 3: return typeof(Actividad_Consuelo);
        }
        break;
    }

    return null;
  }

  private Component AgregarComponenteSiFalta(GameObject gameObjectObjetivo, Type componentType)
  {
    if (gameObjectObjetivo == null || componentType == null)
    {
      return null;
    }

    Component componenteExistente = gameObjectObjetivo.GetComponent(componentType);
    if (componenteExistente != null)
    {
      return componenteExistente;
    }

    return gameObjectObjetivo.AddComponent(componentType);
  }

  private void AsignarNivelHabilidad(Habilidad habilidad, int nivel)
  {
    if (habilidad != null)
    {
      habilidad.NIVEL = nivel;
    }
  }

  private void RestaurarRecursosDesdeSave(CampaignSaveData data)
  {
    if (data == null)
    {
      return;
    }

    intTipoClima = data.tipoClima;
    RegistrarClimaExclusivoDescubierto(intTipoClima);
    EstadoAlientoNegro = Mathf.Max(0f, data.alientoNegroHoras);
    FatigaActual = data.fatiga;
    EsperanzaActual = Mathf.Clamp(data.esperanza, 0, 100);
    civilesActuales = Mathf.Max(0, data.civiles);
    BueyesActuales = Mathf.Max(0, data.bueyes);
    SuministrosActuales = Mathf.Max(0, data.suministros);
    MaterialesActuales = Mathf.Max(0, data.materiales);
    OroActuales = Mathf.Max(0, data.oro);

    ActualizarUIEsperanzaDesdeEstadoActual();
    ActualizarUICivilesDesdeEstadoActual();
    ActualizarUISuministrosDesdeEstadoActual();
    ActualizarUIMaterialesDesdeEstadoActual();
    ActualizarUIBueyesDesdeEstadoActual();
    ActualizarUIFatigaDesdeEstadoActual();
    ActualizarUIOroDesdeEstadoActual();
    ActualizarUIAlientoNegroDesdeEstadoActual();
    AplicarSpriteClimaDesdeEstadoActual();
    EvaluarDerrotaPorEstadoCaravana();
  }

  private void ActualizarUIEsperanzaDesdeEstadoActual()
  {
    if (valueEsperanza == null)
    {
      return;
    }

    valueEsperanza.text = "" + EsperanzaActual;

    if (EsperanzaActual <= 10)
    {
      valueEsperanza.color = new Color(0.8f, 0.1f, 0.4f);
    }
    else if (EsperanzaActual <= 20)
    {
      valueEsperanza.color = new Color(0.6f, 0.2f, 0.4f);
    }
    else if (EsperanzaActual <= 40)
    {
      valueEsperanza.color = new Color(0.25f, 0.5f, 0.3f);
    }
    else if (EsperanzaActual <= 60)
    {
      valueEsperanza.color = new Color(0.45f, 0.55f, 0.3f);
    }
    else if (EsperanzaActual <= 80)
    {
      valueEsperanza.color = new Color(0.25f, 0.75f, 0.3f);
    }
    else if (EsperanzaActual <= 90)
    {
      valueEsperanza.color = new Color(0.15f, 0.75f, 0.45f);
    }
    else
    {
      valueEsperanza.color = new Color(0.05f, 0.85f, 0.55f);
    }

    if (alertaEsperanza != null)
    {
      alertaEsperanza.SetActive(EsperanzaActual < 20);
    }
  }

  private void ActualizarUICivilesDesdeEstadoActual()
  {
    if (valueCiviles != null)
    {
      valueCiviles.text = "" + civilesActuales;
    }

    GetMiliciasActual();

    if (scMapaManager != null)
    {
      if (scMapaManager.goCaravanafollower1 != null) scMapaManager.goCaravanafollower1.SetActive(civilesActuales > 40);
      if (scMapaManager.goCaravanafollower2 != null) scMapaManager.goCaravanafollower2.SetActive(civilesActuales > 60);
      if (scMapaManager.goCaravanafollower3 != null) scMapaManager.goCaravanafollower3.SetActive(civilesActuales > 95);
      if (scMapaManager.goCaravanafollower4 != null) scMapaManager.goCaravanafollower4.SetActive(civilesActuales > 120);
      if (scMapaManager.goCaravanafollower5 != null) scMapaManager.goCaravanafollower5.SetActive(civilesActuales > 140);
      if (scMapaManager.goCaravanafollower6 != null) scMapaManager.goCaravanafollower6.SetActive(civilesActuales > 180);
    }
  }

  private void ActualizarUISuministrosDesdeEstadoActual()
  {
    if (valueSuministros != null)
    {
      valueSuministros.text = "" + SuministrosActuales;
    }

    float consumo = GetCivilesActual() + GetBueyesActual() * 2;
    int diasSuministros = consumo > 0f ? Mathf.FloorToInt(SuministrosActuales / consumo) : 0;
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
    if (valueCantdescansos != null)
    {
      switch (idioma)
      {
        case 2:
          valueCantdescansos.text = diasSuministros == 1 ? "<i>1 rest</i>" : $"<i>{diasSuministros} rests</i>";
          break;
        default:
          valueCantdescansos.text = diasSuministros == 1 ? "<i>1 descanso</i>" : $"<i>{diasSuministros} descansos</i>";
          break;
      }
    }

    GetCargaLlevadaActual();

    if (alertaSuministros != null)
    {
      alertaSuministros.SetActive(SuministrosActuales < GetCivilesActual());
    }
  }

  private void ActualizarUIMaterialesDesdeEstadoActual()
  {
    if (valueMateriales != null)
    {
      valueMateriales.text = "" + MaterialesActuales;
    }

    GetCargaLlevadaActual();
    EvaluarTooltipSobrepesoMateriales();
  }

  private void ActualizarUIBueyesDesdeEstadoActual()
  {
    CargaMaxActual = GetCapacidadDeCargaActual();
    if (valueCargaMax != null)
    {
      valueCargaMax.text = "/" + CargaMaxActual + ")";
    }
    if (valueCargaLlevada != null)
    {
      valueCargaLlevada.text = "(" + GetCargaLlevadaActual();
    }
    EvaluarTooltipSobrepesoMateriales();
    if (valueBueyes != null)
    {
      valueBueyes.text = "" + BueyesActuales;
    }
  }

  private void ActualizarUIFatigaDesdeEstadoActual()
  {
    if (valueFatiga == null)
    {
      return;
    }

    switch (FatigaActual)
    {
      case < 0:
        valueFatiga.text = TraducirDuranteCarga("Enérgicos(0)");
        valueFatiga.color = new Color(0.1f, 0.95f, 0.2f);
        break;
      case 0:
        valueFatiga.text = TraducirDuranteCarga("Descansados(1)");
        valueFatiga.color = new Color(0.1f, 0.9f, 0.3f);
        break;
      case 1:
        valueFatiga.text = TraducirDuranteCarga("Frescos(2)");
        valueFatiga.color = new Color(0.1f, 0.7f, 0.3f);
        break;
      case 2:
        valueFatiga.text = TraducirDuranteCarga("En Marcha(3)");
        valueFatiga.color = new Color(0.25f, 0.6f, 0.3f);
        break;
      case 3:
        valueFatiga.text = TraducirDuranteCarga("Agitados(4)");
        valueFatiga.color = new Color(0.55f, 0.5f, 0.2f);
        break;
      case 4:
        valueFatiga.text = TraducirDuranteCarga("Cansados(5)");
        valueFatiga.color = new Color(0.75f, 0.3f, 0.25f);
        break;
      default:
        valueFatiga.text = TraducirDuranteCarga("Exhaustos(6)");
        valueFatiga.color = new Color(0.8f, 0.15f, 0.45f);
        break;
    }

    if (alertaFatiga != null)
    {
      alertaFatiga.SetActive(FatigaActual > 2);
    }
  }

  private void ActualizarUIOroDesdeEstadoActual()
  {
    if (valueOro != null)
    {
      valueOro.text = "" + OroActuales;
    }
  }

  private void ActualizarUIAlientoNegroDesdeEstadoActual()
  {
    if (sliderAlientoNegro != null)
    {
      sliderAlientoNegro.value = Mathf.InverseLerp(0f, 100f, EstadoAlientoNegro);
    }

    if (scAlientoNegroVFX != null)
    {
      scAlientoNegroVFX.AvanzarAlientoNegro(0);
    }

    ActualizarTierAlientoNegro();
  }

  public float GetPosicionAlientoNegro()
  {
    return EstadoAlientoNegro;
  }

  public int GetPosicionCaravana()
  {
    return posicionCaravana;
  }

  public float GetDistanciaAlientoACaravana()
  {
    return 35f - (EstadoAlientoNegro - posicionCaravana * HorasPorPasoMapa);
  }

  private void RefrescarTextoDistanciaAliento(bool forzar = false)
  {
    if (distanciaAlientotxt == null)
    {
      return;
    }

    if (!forzar && (MoviendoCaravana || pausandoTextoDistanciaAliento))
    {
      return;
    }

    if (scAtributosZona != null && scAtributosZona.ID == 3)
    {
      distanciaAlientotxt.text = ObtenerTextoDistanciaAliento("--");
      distanciaAlientotxt.color = new Color(0.65f, 0.65f, 0.65f);
      return;
    }

    int distancia = Mathf.RoundToInt(GetDistanciaAlientoACaravana());
    distanciaAlientotxt.text = ObtenerTextoDistanciaAliento(distancia.ToString());
    distanciaAlientotxt.color = ObtenerColorDistanciaAliento(distancia);
  }

  private string ObtenerTextoDistanciaAliento(string distancia)
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    string distanciaFormateada = distancia;
    if (int.TryParse(distancia, out int horasDistancia))
    {
      string signo = horasDistancia < 0 ? "-" : string.Empty;
      distanciaFormateada = signo + FormatearDuracionHoras(Mathf.Abs(horasDistancia));
    }

    return idioma switch
    {
      TRADU.IdiomaIngles => "Distance: " + distanciaFormateada,
      TRADU.IdiomaPortugues => "Distância: " + distanciaFormateada,
      _ => "Distancia: " + distanciaFormateada
    };
  }

  private Color ObtenerColorDistanciaAliento(int distancia)
  {
    Color rojo = new Color(0.9f, 0.14f, 0.12f);
    Color rojoProfundo = new Color(0.55f, 0.02f, 0.08f);
    Color amarillo = new Color(0.95f, 0.78f, 0.16f);
    Color verde = new Color(0.22f, 0.85f, 0.28f);

    if (distancia <= 0)
    {
      return Color.Lerp(rojo, rojoProfundo, Mathf.InverseLerp(0f, -10f, distancia));
    }

    if (distancia < 15)
    {
      return Color.Lerp(rojo, amarillo, Mathf.InverseLerp(0f, 15f, distancia));
    }

    return Color.Lerp(amarillo, verde, Mathf.InverseLerp(15f, 35f, distancia));
  }

  private void AplicarSpriteClimaDesdeEstadoActual()
  {
    if (widgetClima == null)
    {
      return;
    }

    switch (intTipoClima)
    {
      case 1: widgetClima.sprite = clima_sol; break;
      case 2: widgetClima.sprite = clima_calor; break;
      case 3: widgetClima.sprite = clima_lluvia; break;
      case 4: widgetClima.sprite = clima_nieve; break;
      case 5: widgetClima.sprite = clima_niebla; break;
      case 6: widgetClima.sprite = clima_almasDanzantes; break;
      case 7: widgetClima.sprite = clima_auroraboreal; break;
      case 8: widgetClima.sprite = clima_NedukazalNormal; break;
      case 9: widgetClima.sprite = clima_NedukazalMasacre; break;
      default: widgetClima.sprite = clima_sol; break;
    }
  }

  private void RecalcularPreciosPuestoComercialDesdeEstadoActual()
  {
    precio10Suministros = 15 - sequitoMercaderesTier;
    precio1Material = 18 - sequitoMercaderesTier;
    precio1Buey = 20 - sequitoMercaderesTier;

    int tierPalacio = MetaprogresionManager.Instance != null ? MetaprogresionManager.Instance.SerriaTierPalacio : 0;
    float descuento = Mathf.Max(0f, 1f - 0.1f * tierPalacio) * MULTIPLICADOR_PRECIO_PUESTO_COMERCIAL_ASENTAMIENTO;
    precio10Suministros *= descuento;
    precio1Material *= descuento;
    precio1Buey *= descuento;
  }

  private void ActualizarDescripcionesPuestoComercialDesdeEstadoActual()
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
    int costoCompraSuministros = ObtenerCostoPuestoComercialConPresagios(precio10Suministros);
    int costoCompraMaterial = ObtenerCostoPuestoComercialConPresagios(precio1Material);
    int costoCompraBuey = ObtenerCostoPuestoComercialConPresagios(precio1Buey);

    if (idioma == 2)
    {
      if (txtDescSum != null) txtDescSum.text = $"<Color=#F26B70>Sell: {(int)precio10Suministros / 2} Gold</color>    x10   <Color=#5ABD46>Buy: {costoCompraSuministros} Gold</color>";
      if (txtDescMat != null) txtDescMat.text = $"<Color=#F26B70>Sell: {(int)precio1Material / 2} Gold</color>    x1   <Color=#5ABD46>Buy: {costoCompraMaterial} Gold</color>";
      if (txtDescBuey != null) txtDescBuey.text = $"<Color=#F26B70>Sell: {(int)precio1Buey / 2}  Gold</color>    x1   <Color=#5ABD46>Buy: {costoCompraBuey} Gold</color>";
      return;
    }

    if (txtDescSum != null) txtDescSum.text = $"<Color=#F26B70>Venta: {(int)precio10Suministros / 2} Oro</color>    x10   <Color=#5ABD46>Compra: {costoCompraSuministros} Oro</color>";
    if (txtDescMat != null) txtDescMat.text = $"<Color=#F26B70>Venta: {(int)precio1Material / 2} Oro</color>    x1   <Color=#5ABD46>Compra: {costoCompraMaterial} Oro</color>";
    if (txtDescBuey != null) txtDescBuey.text = $"<Color=#F26B70>Venta: {(int)precio1Buey / 2}  Oro</color>    x1   <Color=#5ABD46>Compra: {costoCompraBuey} Oro</color>";
  }

  private string TraducirDuranteCarga(string texto)
  {
    return TRADU.i != null ? TRADU.i.Traducir(texto) : texto;
  }

  private async void EjecutarTareaSegura(Task tarea, string contexto)
  {
    try
    {
      await tarea;
    }
    catch (Exception ex)
    {
      Debug.LogError($"[CampaignManager] Error en {contexto}.");
      Debug.LogException(ex, this);
    }
  }

  public void ForzarTiradaClima()
  {
    if (IntentarAplicarClimaAuroraPasoVientoHeladoDebug(true, true))
    {
      return;
    }

    menuDescanso.GetComponent<MenuDescanso>().TiradaClima();
  }

  public bool EstaActivoDebugForzarMasacreNedukazal()
  {
#if UNITY_EDITOR
    return DEBUG_FORZAR_MASACRE_NEDUKAZAL;
#else
    return false;
#endif
  }

  public bool EstaActivoDebugForzarAuroraPasoVientoHelado()
  {
#if UNITY_EDITOR
    return debugForzarAuroraPasoVientoHelado;
#else
    return false;
#endif
  }

  public bool IntentarAplicarClimaAuroraPasoVientoHeladoDebug(bool escribirLogDebug = false, bool aplicarBeneficioEsperanza = false)
  {
    if (!EstaActivoDebugForzarAuroraPasoVientoHelado()
      || scAtributosZona == null
      || scAtributosZona.ID != 2)
    {
      return false;
    }

    AplicarClimaAuroraPasoVientoHeladoForzada(escribirLogDebug);

    if (aplicarBeneficioEsperanza)
    {
      EscribirLog(TRADU.i.Traducir("-La Aurora Boreal maravilla a toda la caravana. +10 Esperanza"));
      CambiarEsperanzaActual(10);
    }

    return true;
  }

  public void AplicarClimaAuroraPasoVientoHeladoForzada(bool escribirLogDebug = false)
  {
    intTipoClima = 7;
    RegistrarClimaExclusivoDescubierto(intTipoClima);
    if (widgetClima != null)
    {
      widgetClima.sprite = clima_auroraboreal;
    }

    RefrescarVfxClimaCalor();
    if (scMapaManager != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }

    if (menuDescanso != null)
    {
      MenuDescanso menuDescansoComponent = menuDescanso.GetComponent<MenuDescanso>();
      if (menuDescansoComponent != null)
      {
        menuDescansoComponent.SincronizarVisualesClimaDesdeEstadoActual();
      }
    }

    if (escribirLogDebug)
    {
      Debug.Log("[CampaignManager] Debug de clima activo: Aurora Boreal forzada en Paso Viento Helado.");
    }
  }

  public void AplicarClimaMasacreNedukazalForzada(bool escribirLogDebug = false)
  {
    intTipoClima = 9;
    RegistrarClimaExclusivoDescubierto(intTipoClima);
    if (widgetClima != null)
    {
      widgetClima.sprite = clima_NedukazalMasacre;
    }

    RefrescarVfxClimaCalor();
    if (scMapaManager != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }

    if (escribirLogDebug)
    {
      Debug.Log("[CampaignManager] Debug de clima activo: Masacre de Nedukazal forzada.");
    }
  }

  public void RegistrarClimaExclusivoDescubierto(int tipoClima)
  {
    if (MetaprogresionManager.Instance != null)
    {
      MetaprogresionManager.Instance.RegistrarClimaExclusivoDescubierto(tipoClima);
    }
  }

  public void BloquearOlaDeCalorEnSiguienteTiradaClima()
  {
    bloquearOlaDeCalorEnSiguienteTiradaClima = true;
  }

  public bool ConsumirBloqueoOlaDeCalorEnSiguienteTiradaClima()
  {
    bool bloquear = bloquearOlaDeCalorEnSiguienteTiradaClima;
    bloquearOlaDeCalorEnSiguienteTiradaClima = false;
    return bloquear;
  }

  private void InicializarClimaAlIniciar()
  {
    if (DebeUsarConfiguracionTutorial())
    {
      intTipoClima = 1;
      AplicarSpriteClimaDesdeEstadoActual();
      RefrescarVfxClimaCalor();
      SincronizarVisualesMenuDescansoClima();
      return;
    }

#if UNITY_EDITOR
    if (IntentarAplicarClimaAuroraPasoVientoHeladoDebug(true, false))
    {
      return;
    }

    if (EstaActivoDebugForzarMasacreNedukazal() && scAtributosZona != null && scAtributosZona.ID == 3)
    {
      AplicarClimaMasacreNedukazalForzada(true);
      return;
    }

    if (DEBUG_FORZAR_OLA_DE_CALOR_AL_PLAY)
    {
      intTipoClima = 2;
      if (widgetClima != null)
      {
        widgetClima.sprite = clima_calor;
      }

      RefrescarVfxClimaCalor();
      Debug.Log("[CampaignManager] Debug de clima activo: Ola de calor forzada al entrar en Play.");
      return;
    }
#endif

    RefrescarVfxClimaCalor();
    SincronizarVisualesMenuDescansoClima();
  }

  private void SincronizarVisualesMenuDescansoClima()
  {
    if (menuDescanso == null)
    {
      return;
    }

    MenuDescanso menuDescansoComponent = menuDescanso.GetComponent<MenuDescanso>();
    if (menuDescansoComponent != null)
    {
      menuDescansoComponent.SincronizarVisualesClimaDesdeEstadoActual();
    }
  }

  public void RefrescarVfxClimaCalor()
  {
    RefrescarTextoDistanciaAliento();

    Canvas canvasObjetivo = null;
    if (widgetClima != null && widgetClima.canvas != null)
    {
      canvasObjetivo = widgetClima.canvas.rootCanvas != null ? widgetClima.canvas.rootCanvas : widgetClima.canvas;
    }
    else if (goCanvas != null)
    {
      canvasObjetivo = goCanvas.GetComponentInChildren<Canvas>(true);
    }

    HeatWaveScreenEffect heatWaveEffect = HeatWaveScreenEffect.Ensure(canvasObjetivo);
    if (heatWaveEffect != null)
    {
      heatWaveEffect.SetEffectActive(false);
    }

    BurningForestScreenEffect burningForestEffect = BurningForestScreenEffect.Ensure(canvasObjetivo);
    if (burningForestEffect != null)
    {
      bool zonaBosqueArdienteActiva = scAtributosZona != null && scAtributosZona.ID == 1;
      burningForestEffect.SetEffectActive(zonaBosqueArdienteActiva);
    }

    BlackBreathInsideScreenEffect blackBreathEffect = BlackBreathInsideScreenEffect.Ensure(canvasObjetivo);
    if (blackBreathEffect != null)
    {
      blackBreathEffect.SetEffectActive(EstaDentroDelAlientoNegro());
    }

    VisualPolishRuntime.ApplyCampaignParticleQualityScaleNow();
  }

  private bool EstaDentroDelAlientoNegro()
  {
    if (scAtributosZona == null || scAtributosZona.ID == 3)
    {
      return false;
    }

    // Reutiliza el mismo umbral que el estado "Dentro" del tooltip (tier 3+).
    float cercaniaAliento = EstadoAlientoNegro - posicionCaravana * HorasPorPasoMapa;
    return cercaniaAliento >= 35f;
  }

  public bool EstaDentroOpeorDelAlientoNegro()
  {
    return EstaDentroDelAlientoNegro();
  }
  #region Nodos
  public SunController sunController;

  bool EsTipoNodoDeCombate(int tipoNodo)
  {
    switch (tipoNodo)
    {
      case 1:
      case 8:
      case 10:
      case 11:
      case 12:
      case 15:
        return true;
      default:
        return false;
    }
  }

  void IgnorarCombatePendienteDebug(int tipoCombate)
  {
    if (tipoCombate <= 0 || !EsTipoNodoDeCombate(tipoCombate))
    {
      return;
    }

    BATALLA_EnCurso = 0;
    EMBOSCADA_EnCurso = 0;
    FinalizarSnapshotCombate();

    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    if (nodoActual != null && nodoActual.tipoNodo == tipoCombate)
    {
      nodoActual.nodoDespejado = true;
    }

    Debug.Log("[CampaignManager] Debug activo: combate ignorado en nodo tipo " + tipoCombate + ".");
    ActualizarBotonesAccionNodoActual();
  }

  int ObtenerTipoCombatePendiente()
  {
    if (debugIgnorarCombates)
    {
      return 0;
    }

    if (BATALLA_EnCurso > 0)
    {
      return BATALLA_EnCurso;
    }

    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    if (nodoActual != null && !nodoActual.nodoDespejado && EsTipoNodoDeCombate(nodoActual.tipoNodo))
    {
      return nodoActual.tipoNodo;
    }

    return 0;
  }

  public bool HayBatallaPendiente()
  {
    return ObtenerTipoCombatePendiente() > 0;
  }

  public void CapturarHoraCombatePendiente()
  {
    if (combateHoraCapturada)
    {
      return;
    }
    combateHoraCapturada = true;
    combateNocturno = EsNocheActual();
  }

  public void CapturarHoraCombatePendiente(float hora)
  {
    combateHoraCapturada = true;
    combateNocturno = EsHoraNocturna(hora);
  }

  public bool EsCombatePendienteNocturno()
  {
    return combateHoraCapturada ? combateNocturno : EsNocheActual();
  }

  public void FinalizarSnapshotCombate()
  {
    combateHoraCapturada = false;
    combateNocturno = false;
    if ((descansoInterrumpidoPendiente || descansoResultadosPendientes) && !continuacionDescansoGestionadaPorMenu)
    {
      StartCoroutine(CompletarContinuacionDescansoTrasCarga());
    }
  }

  public void GuardarContinuacionDescanso(
    float horasRestantes,
    int tareaCivil,
    bool enClaro,
    float horaCombate,
    Personaje purificadoraRitual,
    int climaInicial,
    float valorTarea,
    int chanceExploracion,
    int chanceEmboscada,
    bool emboscadaPendiente,
    float horasHastaEmboscada,
    int tiradaEmboscada)
  {
    descansoInterrumpidoPendiente = true;
    descansoResultadosPendientes = true;
    descansoTuvoEmboscada = false;
    descansoHorasRestantes = Mathf.Max(0f, horasRestantes);
    descansoTareaCivil = tareaCivil;
    descansoEnClaro = enClaro;
    descansoHoraCombate = Mathf.Repeat(horaCombate, 24f);
    descansoRitualElegible = purificadoraRitual != null;
    descansoRitualPersonajeId = purificadoraRitual != null
      ? purificadoraRitual.EnsurePersistentId()
      : string.Empty;
    descansoClimaInicial = climaInicial;
    descansoValorTarea = valorTarea;
    descansoChanceExploracion = chanceExploracion;
    descansoChanceEmboscada = chanceEmboscada;
    descansoEmboscadaPendiente = emboscadaPendiente;
    descansoHorasHastaEmboscada = emboscadaPendiente ? Mathf.Max(0f, horasHastaEmboscada) : 0f;
    descansoTiradaEmboscada = emboscadaPendiente ? tiradaEmboscada : 0;
    continuacionDescansoGestionadaPorMenu = true;
  }

  public void ObtenerSnapshotResultadosDescanso(
    out int climaInicial,
    out float valorTarea,
    out int chanceExploracion,
    out int chanceEmboscada)
  {
    climaInicial = descansoClimaInicial;
    valorTarea = descansoValorTarea;
    chanceExploracion = descansoChanceExploracion;
    chanceEmboscada = descansoChanceEmboscada;
  }

  public Personaje ObtenerPurificadoraRitualDescansoPendiente()
  {
    if (!descansoRitualElegible
      || string.IsNullOrWhiteSpace(descansoRitualPersonajeId)
      || scMenuPersonajes == null
      || scMenuPersonajes.listaPersonajes == null)
    {
      return null;
    }

    return scMenuPersonajes.listaPersonajes.Find(personaje =>
      personaje != null && personaje.GetPersistentId() == descansoRitualPersonajeId);
  }

  public void AvanzarProgresoDescansoPendiente(float horas)
  {
    float horasValidas = Mathf.Max(0f, horas);
    descansoHorasRestantes = Mathf.Max(0f, descansoHorasRestantes - horasValidas);
    if (descansoEmboscadaPendiente)
    {
      descansoHorasHastaEmboscada = Mathf.Max(0f, descansoHorasHastaEmboscada - horasValidas);
    }
  }

  public void MarcarEmboscadaDescansoConsumida(float horaCombate)
  {
    descansoTuvoEmboscada = true;
    descansoEmboscadaPendiente = false;
    descansoHorasHastaEmboscada = 0f;
    descansoTiradaEmboscada = 0;
    descansoHoraCombate = Mathf.Repeat(horaCombate, 24f);
  }

  public bool DescansoTuvoEmboscadaPendienteResultados()
  {
    return descansoResultadosPendientes && descansoTuvoEmboscada;
  }

  public void FinalizarContinuacionDescanso()
  {
    descansoInterrumpidoPendiente = false;
    descansoHorasRestantes = 0f;
    descansoEmboscadaPendiente = false;
    descansoHorasHastaEmboscada = 0f;
    descansoTiradaEmboscada = 0;
    continuacionDescansoGestionadaPorMenu = false;
  }

  public void MarcarResultadosDescansoCompletados()
  {
    descansoResultadosPendientes = false;
    descansoTuvoEmboscada = false;
    descansoRitualElegible = false;
    descansoRitualPersonajeId = string.Empty;
    descansoClimaInicial = 0;
    descansoValorTarea = 0f;
    descansoChanceExploracion = 0;
    descansoChanceEmboscada = 0;
    descansoTareaCivil = 0;
    descansoEnClaro = false;
    descansoHoraCombate = 0f;
    descansoEmboscadaPendiente = false;
    descansoHorasHastaEmboscada = 0f;
    descansoTiradaEmboscada = 0;
  }

  private IEnumerator CompletarContinuacionDescansoTrasCarga()
  {
    continuacionDescansoGestionadaPorMenu = true;
    yield return null;
    int tareaCivilPendiente = descansoTareaCivil;
    float multiplicadorAliento = descansoEnClaro ? 0.5f : 1f;
    float multiplicadorCuracion = descansoEnClaro ? 1.1f : 1f;
    Nodo nodoDescanso = scMapaManager != null ? scMapaManager.nodoActual : null;
    if (nodoDescanso != null && nodoDescanso.tipoNodo == 5) multiplicadorCuracion *= 1.2f;
    if (descansoTareaCivil == 4) multiplicadorCuracion *= 1.1f;
    if (descansoInterrumpidoPendiente && descansoHorasRestantes > 0.0001f)
    {
      if (descansoEmboscadaPendiente)
      {
        float horasHastaEmboscada = Mathf.Min(descansoHorasRestantes, descansoHorasHastaEmboscada);
        yield return TranscurrirDescansoPendienteTrasCarga(
          horasHastaEmboscada,
          multiplicadorAliento,
          multiplicadorCuracion);

        int tiradaEmboscada = descansoTiradaEmboscada;
        int chanceEmboscada = descansoChanceEmboscada;
        MarcarEmboscadaDescansoConsumida(ObtenerHoraActual());
        CapturarHoraCombatePendiente(ObtenerHoraActual());
        BATALLA_EnCurso = 11;
        EMBOSCADA_EnCurso = 3;
        EscribirLog(TRADU.i.Traducir("-La caravana ha sufrido un Ataque durante el descanso. Probabilidades ")
          + chanceEmboscada
          + TRADU.i.Traducir("% - Tirada: 1d100 = ")
          + tiradaEmboscada);
        TryAutosaveCampania("descanso_interrumpido_reanudado", out _);
        scMenuBatallas.EventoBatallaCaravana(0, 3);

        while (BATALLA_EnCurso > 0)
        {
          yield return null;
        }
      }

      if (descansoHorasRestantes > 0.0001f)
      {
        yield return TranscurrirDescansoPendienteTrasCarga(
          descansoHorasRestantes,
          multiplicadorAliento,
          multiplicadorCuracion);
      }
    }
    FinalizarContinuacionDescanso();

    MenuDescanso controladorDescanso = menuDescanso != null
      ? menuDescanso.GetComponent<MenuDescanso>()
      : null;
    if (controladorDescanso == null)
    {
      MarcarResultadosDescansoCompletados();
      FinalizarAccionTemporal();
      yield break;
    }

    Task resultadosDescanso = controladorDescanso.CompletarResultadosDescansoTrasCarga(tareaCivilPendiente);
    while (!resultadosDescanso.IsCompleted)
    {
      yield return null;
    }

    if (resultadosDescanso.IsFaulted)
    {
      Debug.LogException(resultadosDescanso.Exception, controladorDescanso);
      FinalizarAccionTemporal();
    }

    MarcarResultadosDescansoCompletados();
  }

  private IEnumerator TranscurrirDescansoPendienteTrasCarga(
    float horas,
    float multiplicadorAliento,
    float multiplicadorCuracion)
  {
    float restantes = Mathf.Max(0f, horas);
    while (restantes > 0.0001f)
    {
      float delta = Mathf.Min(restantes, Mathf.Max(0f, Time.deltaTime));
      if (delta <= 0f)
      {
        yield return null;
        continue;
      }

      AvanzarTiempoCampania(
        delta,
        TipoAvanceTiempoCampania.Descanso,
        multiplicadorAliento,
        multiplicadorCuracion,
        true);
      AvanzarProgresoDescansoPendiente(delta);
      restantes -= delta;
      yield return null;
    }
  }

  bool PuedeAcamparEnNodo(Nodo nodo)
  {
    return nodo != null
      && nodo.tipoNodo != 4
      && nodo.nodoDespejado
      && !nodo.nodoIncendiado;
  }

  void ActualizarBotonesAccionNodoActual()
  {
    bool mostrandoViajando = MoviendoCaravana || enviandoExploradores;
    int combatePendiente = mostrandoViajando ? 0 : ObtenerTipoCombatePendiente();
    bool mostrandoResolverCombate = !mostrandoViajando && combatePendiente > 0;
    bool mostrandoAcampar = !mostrandoViajando
      && !mostrandoResolverCombate
      && PuedeAcamparEnNodo(scMapaManager != null ? scMapaManager.nodoActual : null);

    if (goBotonViajando != null && goBotonViajando.activeSelf != mostrandoViajando)
    {
      goBotonViajando.SetActive(mostrandoViajando);
    }

    if (goBotonResolverCombate != null && goBotonResolverCombate.activeSelf != mostrandoResolverCombate)
    {
      goBotonResolverCombate.SetActive(mostrandoResolverCombate);
    }

    if (goBotonAcampar != null && goBotonAcampar.activeSelf != mostrandoAcampar)
    {
      goBotonAcampar.SetActive(mostrandoAcampar);
    }

  }

  bool AbrirMenuBatallas()
  {
    GameObject menuBatallas = goMenuBatallas != null
      ? goMenuBatallas
      : (scMenuBatallas != null ? scMenuBatallas.gameObject : null);

    if (menuBatallas == null || scMenuBatallas == null)
    {
      return false;
    }

    if (menuBatallas.activeInHierarchy)
    {
      return false;
    }

    CapturarHoraCombatePendiente();
    menuBatallas.SetActive(true);
    return true;
  }

  int CalcularResultadoEmboscadaViajeActual(bool anunciarEmboscadaAliada = true)
  {
    if (debugIgnorarCombates)
    {
      return 0;
    }

    if (scTutorialManager.pasoActual == 2)
    {
      scTutorialManager.establecerPasoEspecifico(3);
    }

    int modEmboscadaViajeActual = estadosCaravana != null ? estadosCaravana.ObtenerModificadorEmboscadaDuranteViajeActual() : 0;
    int randomEmboscada = UnityEngine.Random.Range(1, 101);

    int chancesemboscada = scAtributosZona.modChanceEmboscada + modEmboscadaViajeActual;
    chancesemboscada += CuantosPersonajesHacenTalActividad(7) * 3;
    chancesemboscada -= CuantosPersonajesHacenTalActividad(14) * 5;
    chancesemboscada += ObtenerModificadorChanceEmboscadaTraits();
    if (viajeActualIncluyoNoche && antorchasEncendidas)
    {
      chancesemboscada += 15;
    }

    if (scMenuSequito.TieneSequito(9))
    {
      chancesemboscada += 4;
    }
    if (viajeClimaInicial == 6)
    {
      chancesemboscada -= 100;
    }
    if (DebeUsarConfiguracionTutorial())
    {
      chancesemboscada = 0;
    }

    int chanceEmboscadaNormalizada = Mathf.Clamp(chancesemboscada, 0, 100);
    int chanceEmboscadaEnemiga = ReducirFrecuenciaEmboscada(chanceEmboscadaNormalizada);
    int chanceEmboscadaAliada = ReducirFrecuenciaEmboscada(Mathf.Max(0, 51 - chanceEmboscadaNormalizada));
    chanceEmboscadaEnemiga = AjustarChanceEmboscadaEnemigaPresagios(chanceEmboscadaEnemiga);
    chanceEmboscadaAliada = AjustarChanceEmboscadaAliadaPresagios(chanceEmboscadaAliada);
    if (viajeClimaInicial == 5)
    {
      chanceEmboscadaEnemiga = Mathf.Max(0, chanceEmboscadaEnemiga - 20);
      chanceEmboscadaAliada = Mathf.Max(0, chanceEmboscadaAliada - 20);
    }
    bool emboscadaEnemiga = randomEmboscada <= chanceEmboscadaEnemiga;
    bool emboscadaAliada = !emboscadaEnemiga && chanceEmboscadaAliada > 0 && randomEmboscada > 100 - chanceEmboscadaAliada;

    if (emboscadaEnemiga)
    {
      EscribirLog(FormatearLogTiradaEmboscada(true, chanceEmboscadaEnemiga, chanceEmboscadaAliada, randomEmboscada));
      return 1;
    }

    if (emboscadaAliada)
    {
      string logEmboscadaAliada = FormatearLogTiradaEmboscada(
        false,
        chanceEmboscadaEnemiga,
        chanceEmboscadaAliada,
        randomEmboscada);
      if (anunciarEmboscadaAliada)
      {
        EscribirLog(logEmboscadaAliada);
      }
      else
      {
        logEmboscadaViajePendiente = logEmboscadaAliada;
      }

      return 2;
    }

    return 0;
  }

  public int PrepararEmboscadaViajeAnticipada(Nodo destino)
  {
    if (emboscadaViajeCalculada)
    {
      return EMBOSCADA_EnCurso;
    }

    if (destino == null
        || destino != nodoDestinoActual
        || (destino.tipoNodo != 1 && destino.tipoNodo != 8))
    {
      return 0;
    }

    EMBOSCADA_EnCurso = CalcularResultadoEmboscadaViajeActual(false);
    emboscadaViajeCalculada = true;
    return EMBOSCADA_EnCurso;
  }

  int ObtenerResultadoEmboscadaAlLlegar()
  {
    if (!emboscadaViajeCalculada)
    {
      EMBOSCADA_EnCurso = CalcularResultadoEmboscadaViajeActual();
      emboscadaViajeCalculada = true;
    }
    else if (!string.IsNullOrEmpty(logEmboscadaViajePendiente))
    {
      EscribirLog(logEmboscadaViajePendiente);
      logEmboscadaViajePendiente = null;
    }

    return EMBOSCADA_EnCurso;
  }

  int ReducirFrecuenciaEmboscada(int chanceBase)
  {
    return Mathf.Clamp(Mathf.RoundToInt(chanceBase * 0.9f), 0, 100);
  }

  void ResolverCombateNormal(int emboscada)
  {
    scMenuBatallas.EventoBatallaNormal(0, emboscada);
  }

  void ResolverCombateElite(int emboscada, bool forzarRitualKaleTav = false)
  {
    scMenuBatallas.EventoBatallaElite(0, emboscada, forzarRitualKaleTav);
  }

  public bool IntentarIniciarViajeDesdeNodoActual()
  {
    if (MoviendoCaravana || enviandoExploradores)
    {
      return false;
    }

    if (ObtenerTipoCombatePendiente() <= 0)
    {
      return true;
    }

    ResolverCombate();
    return false;
  }

  public void ResolverCombate()
  {
    if (MoviendoCaravana || enviandoExploradores)
    {
      return;
    }

    int tipoCombatePendiente = ObtenerTipoCombatePendiente();
    if (tipoCombatePendiente <= 0)
    {
      return;
    }

    if (debugIgnorarCombates)
    {
      IgnorarCombatePendienteDebug(tipoCombatePendiente);
      return;
    }

    if (!AbrirMenuBatallas())
    {
      return;
    }
    TutorialEvents.Emit("ui.batprimeratuto_presionado", gameObject);
    switch (tipoCombatePendiente)
    {
      case 1:
        ResolverCombateNormal(EMBOSCADA_EnCurso);
        break;
      case 8:
        ResolverCombateElite(EMBOSCADA_EnCurso);
        break;
      case 10:
        scMenuBatallas.EventoBatallaFinal(0);
        break;
      case 11:
        scMenuBatallas.EventoBatallaCaravana(0, EMBOSCADA_EnCurso > 0 ? EMBOSCADA_EnCurso : 3);
        break;
      case 12:
        scMenuBatallas.EventoBatallaSubterranea(scAtributosZona.FASE);
        break;
      case 15:
        bool esRitualActivo = scMapaManager != null
          && scMapaManager.nodoActual != null
          && scMapaManager.nodoActual.nodoRitual;
        ResolverCombateElite(0, esRitualActivo);
        break;
    }
  }

  private string ObtenerTextoLogViajeAtajoSuperficie()
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
      case TRADU.IdiomaIngles:
        return "-The uncomfortable trip through the shortcut has caused unease among the civilians.";
      case TRADU.IdiomaPortugues:
        return "-A viagem desconfortável pelo atalho provocou mal-estar nos civis.";
      default:
        return "-El viaje incómodo por el atajo ha provocado malestar en los civiles.";
    }
  }

  public void ViajeIniciado(CaminoConexion conexion)
  {
    if (conexion == null || conexion.destino == null)
    {
      Debug.LogError("[CampaignManager] No se puede iniciar un viaje sin una conexion valida.");
      return;
    }

    if (menuDescanso != null)
    {
      menuDescanso.SetActive(false);
    }

    pausandoTextoDistanciaAliento = true;
    Nodo destino = conexion.destino;
    bool viajeAtajoSuperficie = conexion.EsAtajoSuperficie;
    ActivarLog(0);

    float duracionViajePrevista = CalcularDuracionViajeHoras(conexion);
    viajeActualIncluyoNoche = IntervaloContieneNoche(ObtenerHoraActual(), duracionViajePrevista);
    viajeClimaInicial = intTipoClima;

    EfectosViajeCaravana efectosViajeCaravana = estadosCaravana != null
      ? estadosCaravana.IniciarViajeActual()
      : default;

    creditoPrevencionAlientoHoras = 0f;
    horasViajeActual = 0f;
    BATALLA_EnCurso = 0;
    EMBOSCADA_EnCurso = 0;
    emboscadaViajeCalculada = false;
    logEmboscadaViajePendiente = null;
    nodoDestinoActual = destino;
    if (sunController != null)
    {
      sunController.OnTravelStart(); // Amanecer, día, atardecer y noche durante el viaje.
    }
    animCaravana.SetBool("IsWalking", true);
    multiplicadorVelocidadVisualViajeActual = efectosViajeCaravana.multiplicadorVelocidadVisual <= 0f
      ? 1f
      : efectosViajeCaravana.multiplicadorVelocidadVisual;
    if (SeLlevaDemasiadaCarga())
    {
      multiplicadorVelocidadVisualViajeActual *= MultiplicadorVelocidadSobrecarga;
    }
    animCaravana.speed = multiplicadorVelocidadVisualViajeActual;

    // Inicia sonido de caravana en movimiento
    IniciarSonidoMovimientoCaravana(Mathf.Max(0.05f, sfxMovimientoFadeIn));

    if (viajeAtajoSuperficie)
    {
      CambiarEsperanzaActual(-3);
      EscribirLog(ObtenerTextoLogViajeAtajoSuperficie());
    }

    //Sequito de Clerigos 20% de prevenirlo
    int random2 = UnityEngine.Random.Range(0, 100);
    if (scMenuSequito.TieneSequito(10) && random2 < 21) //Clérigos !!! 21
    {
      PrepararPrevencionAlientoAccion(HorasPorPasoMapa);
      EscribirLog(TRADU.i.Traducir("-Los rezos constantes del Séquito de Clérigos previenen 5 h de Aliento Negro."));

    }

    //Si nieva, viajar desgasta la esperanza de la caravana.
    if (intTipoClima == 4)
    {
      int perdidaEsperanzaNieve = 3 * Mathf.Max(1, conexion.costoMovimiento);
      CambiarEsperanzaActual(-perdidaEsperanzaNieve);
      EscribirLog(ObtenerTextoLogViajeNieve(perdidaEsperanzaNieve));
    }

    if (intTipoClima == 6)
    {
      if (scAtributosZona.ID == 1) // Bosque Angustiante
      {
        EscribirLog(TRADU.i.Traducir("-Las Almas Danzantes guían a la caravana. +3 Esperanza."));
        CambiarEsperanzaActual(3);
      }
      if (scAtributosZona.ID == 2) // Paso del Viento Helado
      {
        EscribirLog(TRADU.i.Traducir("-La Aurora Boreal maravilla a toda la caravana. +10 Esperanza"));
        CambiarEsperanzaActual(10);
      }
    }

    //Efectos en Civiles segun Tier de Aliento
    if (GetTierAlientoNegro() == 1)
    {
      EscribirLogSinBitacora(TRADU.i.Traducir("-La ausencia de Aliento Negro al viajar, inspira a la Caravana. +2 Esperanza"));
      CambiarEsperanzaActual(2);
    }
    if (GetTierAlientoNegro() == 2)
    {
      int perdidaEsperanza = AjustarPerdidaPorAlientoNegroPresagios(3);
      EscribirLogSinBitacora(ObtenerTextoLogEfectoAlientoNegroViaje(2, perdidaEsperanza));
      CambiarEsperanzaActual(-perdidaEsperanza);
    }
    if (GetTierAlientoNegro() == 3)
    {
      int perdidaEsperanza = AjustarPerdidaPorAlientoNegroPresagios(7);
      EscribirLogSinBitacora(ObtenerTextoLogEfectoAlientoNegroViaje(3, perdidaEsperanza));
      CambiarEsperanzaActual(-perdidaEsperanza);
    }
    if (GetTierAlientoNegro() == 4)
    {
      int perdidaEsperanza = AjustarPerdidaPorAlientoNegroPresagios(10);
      int civilesPerdidos = AjustarPerdidaPorAlientoNegroPresagios(UnityEngine.Random.Range(1, 5));
      CambiarEsperanzaActual(-perdidaEsperanza);
      CambiarCivilesActuales(-civilesPerdidos);
      EscribirLogSinBitacora(ObtenerTextoLogEfectoAlientoNegroViaje(4, perdidaEsperanza, civilesPerdidos));
    }

    AplicarTraitsMoraleAmbientales();

    RefrescarBarraPersonajesCampania(true);
    ActualizarBotonesAccionNodoActual();
  }

  private string ObtenerTextoLogViajeNieve(int perdidaEsperanza)
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
      case TRADU.IdiomaIngles:
        return "-Traveling through snow wears down the caravan. -" + perdidaEsperanza + " Hope";
      case TRADU.IdiomaPortugues:
        return "-Viajar pela neve desgasta a caravana. -" + perdidaEsperanza + " Esperança";
      default:
        return "-Viajar con nieve desgasta a la caravana. -" + perdidaEsperanza + " Esperanza";
    }
  }

  public MenuBatallas scMenuBatallas;
  public GameObject goMenuBatallas;
  private void AsegurarAudioMovimientoCaravana()
  {
    if (sfxMovimientoSource == null)
    {
      sfxMovimientoSource = gameObject.AddComponent<AudioSource>();
    }

    sfxMovimientoSource.playOnAwake = false;
    sfxMovimientoSource.loop = true;
    sfxMovimientoSource.pitch = sfxMovimientoPitch;
    AjustesAudio.AplicarVolumenSfx(sfxMovimientoSource, sfxMovimientoVolumen);
  }

  private void ActualizarVolumenSfxMovimientoCaravana(float _ = -1f)
  {
    if (sfxMovimientoSource == null || rutinaDesvanecerSfxMovimiento != null)
    {
      return;
    }

    AjustesAudio.AplicarVolumenSfx(sfxMovimientoSource, sfxMovimientoVolumen);
  }

  private void IniciarSonidoMovimientoCaravana(float duracionFade)
  {
    if (sfxMovimientoCaravana == null)
    {
      return;
    }

    AsegurarAudioMovimientoCaravana();

    if (rutinaDesvanecerSfxMovimiento != null)
    {
      StopCoroutine(rutinaDesvanecerSfxMovimiento);
      rutinaDesvanecerSfxMovimiento = null;
    }

    if (sfxMovimientoSource.clip != sfxMovimientoCaravana)
    {
      sfxMovimientoSource.clip = sfxMovimientoCaravana;
    }

    rutinaDesvanecerSfxMovimiento = StartCoroutine(FadeInSonidoMovimientoCaravanaCoroutine(Mathf.Max(0.05f, duracionFade)));
  }

  public void IniciarSonidoMovimientoCaravanaIntro()
  {
    IniciarSonidoMovimientoCaravana(Mathf.Max(0.05f, sfxMovimientoFadeIn));
  }

  public void DetenerSonidoMovimientoCaravanaIntro()
  {
    DesvanecerSonidoMovimientoCaravana(Mathf.Max(0.05f, sfxMovimientoFadeOut));
  }

  private IEnumerator FadeInSonidoMovimientoCaravanaCoroutine(float duracion)
  {
    if (sfxMovimientoSource == null)
    {
      rutinaDesvanecerSfxMovimiento = null;
      yield break;
    }

    sfxMovimientoSource.pitch = sfxMovimientoPitch;
    sfxMovimientoSource.loop = true;
    float volumenObjetivo = Mathf.Clamp01(AjustesAudio.EscalarVolumenSfx(sfxMovimientoVolumen));
    float volumenInicial = sfxMovimientoSource.isPlaying ? sfxMovimientoSource.volume : 0f;
    sfxMovimientoSource.volume = volumenInicial;

    if (!sfxMovimientoSource.isPlaying)
    {
      sfxMovimientoSource.Play();
    }

    float tiempo = 0f;
    while (tiempo < duracion && sfxMovimientoSource != null)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      volumenObjetivo = Mathf.Clamp01(AjustesAudio.EscalarVolumenSfx(sfxMovimientoVolumen));
      sfxMovimientoSource.volume = Mathf.Lerp(volumenInicial, volumenObjetivo, t);
      yield return null;
    }

    if (sfxMovimientoSource != null)
    {
      AjustesAudio.AplicarVolumenSfx(sfxMovimientoSource, sfxMovimientoVolumen);
    }

    rutinaDesvanecerSfxMovimiento = null;
  }

  public void DesvanecerSonidoMovimientoCaravana(float duracion)
  {
    if (sfxMovimientoSource == null || !sfxMovimientoSource.isPlaying)
    {
      return;
    }

    if (rutinaDesvanecerSfxMovimiento != null)
    {
      StopCoroutine(rutinaDesvanecerSfxMovimiento);
    }

    rutinaDesvanecerSfxMovimiento = StartCoroutine(DesvanecerSonidoMovimientoCaravanaCoroutine(Mathf.Max(0.05f, duracion)));
  }

  private IEnumerator DesvanecerSonidoMovimientoCaravanaCoroutine(float duracion)
  {
    if (sfxMovimientoSource == null)
    {
      rutinaDesvanecerSfxMovimiento = null;
      yield break;
    }

    float volumenInicial = sfxMovimientoSource.volume;
    float tiempo = 0f;

    while (tiempo < duracion && sfxMovimientoSource != null && sfxMovimientoSource.isPlaying)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      sfxMovimientoSource.volume = Mathf.Lerp(volumenInicial, 0f, t);
      yield return null;
    }

    if (sfxMovimientoSource != null)
    {
      sfxMovimientoSource.Stop();
      AjustesAudio.AplicarVolumenSfx(sfxMovimientoSource, sfxMovimientoVolumen);
      sfxMovimientoSource.pitch = sfxMovimientoPitch;
    }

    rutinaDesvanecerSfxMovimiento = null;
  }

  public void LlegarANodo(int ID, int posX, Nodo nodo)
  {
    pausandoTextoDistanciaAliento = true;
    
    // Detiene el sonido de movimiento al llegar al nodo
    if (sfxMovimientoSource != null && sfxMovimientoSource.isPlaying && rutinaDesvanecerSfxMovimiento == null)
    {
      DesvanecerSonidoMovimientoCaravana(Mathf.Max(0.05f, sfxMovimientoFadeOut));
    }

    animCaravana.SetBool("IsWalking", false);
    animCaravana.speed = 1f;
    if (estadosCaravana != null)
    {
      estadosCaravana.FinalizarViajeActual();
    }
    
    if (scTutorialManager.pasoActual == 18) { scTutorialManager.SiguientePaso(); CambiarValorAlientoNegroHoras(10f); }
    if (scTutorialManager.pasoActual == 31) { scTutorialManager.SiguientePaso();  }


    posicionCaravana = posX + 1;
    pausandoTextoDistanciaAliento = false;
    RefrescarTextoDistanciaAliento(true);
    ActualizarTierAlientoNegro();
    logDeCampania?.RegistrarLlegadaNodo(ID);
    if (debugIgnorarCombates && EsTipoNodoDeCombate(ID))
    {
      IgnorarCombatePendienteDebug(ID);
      return;
    }

    if (ID == 1) //Batalla
    {

      BATALLA_EnCurso = ID;
      EMBOSCADA_EnCurso = ObtenerResultadoEmboscadaAlLlegar();
      if (EMBOSCADA_EnCurso == 1 && AbrirMenuBatallas())
      {
        TutorialTooltipManager.TryShow(TooltipEmboscadaNormalId);
        ResolverCombateNormal(EMBOSCADA_EnCurso);
      }

#if false
      if (scTutorialManager.pasoActual == 2) {scTutorialManager.establecerPasoEspecifico(3); }
     
      

      //Probabilidad emboscada
      int randomEmboscada = UnityEngine.Random.Range(1, 101);

      int chancesemboscada = scAtributosZona.modChanceEmboscada + modEmboscadaViajeActual;
      chancesemboscada -= CuantosPersonajesHacenTalActividad(14) * 5; //-5% por cada Acechador Actividad Vigilar Desde Sombras
      chancesemboscada += ObtenerModificadorChanceEmboscadaTraits();

      if (scMenuSequito.TieneSequito(9))
      {
        chancesemboscada += 4; // +4% si hay un Séquito de Nobles
      }
      if (intTipoClima == 6) //Almas Danzantes del bosque ardiente
      {
        chancesemboscada -= 100; // -100% si hay Almas Danzantes
      }
      if (scTutorialManager.tutorialActivo)
      { chancesemboscada = 0; } //No emboscadas en tutorial

      int chanceEmboscadaNormalizada = Mathf.Clamp(chancesemboscada, 0, 100);
      int chanceEmboscadaEnemiga = ReducirFrecuenciaEmboscada(chanceEmboscadaNormalizada);
      int chanceEmboscadaAliada = ReducirFrecuenciaEmboscada(Mathf.Max(0, 61 - chanceEmboscadaNormalizada));
      bool emboscadaEnemiga = randomEmboscada <= chanceEmboscadaEnemiga;
      bool emboscadaAliada = !emboscadaEnemiga && chanceEmboscadaAliada > 0 && randomEmboscada > 100 - chanceEmboscadaAliada;

      if (emboscadaEnemiga)
      {
        EscribirLog(FormatearLogTiradaEmboscada(true, chanceEmboscadaEnemiga, chanceEmboscadaAliada, randomEmboscada));
        scMenuBatallas.EventoBatallaNormal(0, 1); //Emboscada

      }
      else
      {
        if (emboscadaAliada)
        {
          EscribirLog(FormatearLogTiradaEmboscada(false, chanceEmboscadaEnemiga, chanceEmboscadaAliada, randomEmboscada));
          scMenuBatallas.EventoBatallaNormal(0, 2); //Emboscada a favor de la caravana
        }
        else
        {
          scMenuBatallas.EventoBatallaNormal(0, 0); //No emboscada
        }

      }
#endif
    }
    if (ID == 8) //Batalla Elite
    {

      BATALLA_EnCurso = ID;
      EMBOSCADA_EnCurso = ObtenerResultadoEmboscadaAlLlegar();
      if (EMBOSCADA_EnCurso == 1 && AbrirMenuBatallas())
      {
        ResolverCombateElite(EMBOSCADA_EnCurso);
      }
    }
    if (ID == 2) //Evento
    {
      if (scTutorialManager != null && scTutorialManager.DebeForzarEventoDesaparicionesMisteriosas(nodo))
      {
        EmpezarEvento(IdsEventoCampania.DesaparicionesMisteriosas);
        if (scMapaManager != null && scMapaManager.nodoActual != null)
        {
          scMapaManager.nodoActual.nodoDespejado = true;
        }
        else if (nodo != null)
        {
          nodo.nodoDespejado = true;
        }
        return;
      }

      float factorEventoBuenoMalo = 40 + Instance.GetEsperanzaActual() / 5 + ObtenerModificadorChanceEventoTraits();
      factorEventoBuenoMalo = AjustarChanceEventoBuenoPresagios(factorEventoBuenoMalo);
      float randomEvento = UnityEngine.Random.Range(0, 100);

      if (randomEvento < factorEventoBuenoMalo)
      {
        EmpezarEventoBueno(TipoOrigenEventoCampania.Nodo);

      }
      else { EmpezarEventoMalo(TipoOrigenEventoCampania.Nodo); }


      scMapaManager.nodoActual.nodoDespejado = true;

    }
    if (ID == 3) //Claro
    {
      EmpezarEvento(IdsEventoCampania.Claro);
      scMapaManager.nodoActual.nodoDespejado = true;
      if (scMenuSequito.TieneSequito(5))
      {
        scSequitoHerboristas.vecesEnClaro++;
        EscribirLog(TRADU.i.Traducir("-El Séquito de Herboristas ha visitado un Claro y recolectado hierbas curativas."));
      }
    }
    if (ID == 4) //Asentamiento
    {
      scMapaManager?.RevelarAdyacentesDesdeAsentamiento();

      if (DebeUsarConfiguracionTutorial())
      {
        if (scMapaManager != null && scMapaManager.nodoActual != null)
        {
          scMapaManager.nodoActual.nodoDespejado = true;
        }
        return;
      }

      estadisticaAsentamientosVisitados++;
      BanterCampaignDirector.NotificarLlegadaAsentamiento();
      if (ObtenerAsentamientoManager() != null)
      {
        asentamientoManager.AbrirAlLlegar();
      }
      else
      {
        EmpezarEvento(IdsEventoCampania.Asentamiento);
        scMapaManager.nodoActual.nodoDespejado = true;
      }
    }
    if (ID == 5) //Recursos
    {
      EmpezarEvento(IdsEventoCampania.Recursos);
      scMapaManager.nodoActual.nodoDespejado = true;
    }
    if (ID == 6) //Puesto Comercial
    {
      AbrirPuestoComercial();
      scMapaManager.nodoActual.nodoDespejado = true;
    }
    if (ID == 7) //Personaje / Séquito
    {
      goUIPersonajeSequito.SetActive(true);
      scMapaManager.nodoActual.nodoDespejado = true;
    }
    if (ID == 10) //Batalla Final
    {

      BATALLA_EnCurso = ID;
      EMBOSCADA_EnCurso = 0;
    }
    if (ID == 11) //Ataque Caravana
    {

      BATALLA_EnCurso = ID;
      EMBOSCADA_EnCurso = 3;
      if (AbrirMenuBatallas())
      {
        scMenuBatallas.EventoBatallaCaravana(0, EMBOSCADA_EnCurso);
      }
    }
    if (ID == 12) //Ataque Subterráneo
    {
      EscribirLog(TRADU.i.Traducir("-La Caravana ha sido emboscada por un ataque subterráneo."));

      BATALLA_EnCurso = ID;

    }
    if (ID == 12)
    {
      EMBOSCADA_EnCurso = 1;
    }
    if (ID == 14) //Santuario
    {

      goUISantuario.SetActive(true);
      txtdescripcionSantuario.text = TRADU.i.Traducir("Has llegado a un Santuario de Purificadores, varios se han construido en la región para dar apoyo y plegarias a los valientes que combatieron al Liche.\nHoy, si bien está abandonado, mantiene su aura de tranquilidad y puedes depositar ofrendas para realizar una plegaria de purificación.\n\n<i>Descansar en este lugar bendecirá a tus personajes por 96 h.</i>");

      CambiarEsperanzaActual(10);
      EscribirLog(TRADU.i.Traducir("-La caravana ha llegado a un Santuario de Purificadores. Los personajes se han curado un 15%. +10 Esperanza."));

      foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
      {
        if (pers.IDClase == 3) // Purificadora
        {
          pers.RecibirExperiencia(60);
          EscribirLog(TRADU.i.Traducir("-Como Purificadora,") + pers.sNombre + TRADU.i.Traducir(" gana 60 Experiencia por la visita al santuario."));
        }
        float curacion = pers.fVidaMaxima * 0.15f;
        curacion = pers.AplicarMultiplicadorCuracionCampaniaTraits(curacion);
        pers.RecibirCuracion(curacion);

      }

      AplicarTraitsVisitaSantuario();

      scMapaManager.nodoActual.nodoDespejado = true;

    }
    if (ID == 15) //Batalla Ritual PasoVientohelado
    {

      BATALLA_EnCurso = ID;
    }
    if (ID == 16) //Mision Salvamento
    {
      EmpezarEvento(IdsEventoCampania.EncuentroEsperado);
      scMapaManager.nodoActual.nodoDespejado = true;
    }

    //Cronistas
    if (scMenuSequito.TieneSequito(7))
    {
      if (!scSequitoCronistas.yaVendioCronica)
      {
        scSequitoCronistas.valorCambiosCronicas += 20;
        EscribirLog(TRADU.i.Traducir("-El Séquito de Cronistas ha registrado el viaje. +20 Valor Crónica."));
      }
    }
    //Nobles
    if (scMenuSequito.TieneSequito(9))
    {
      int oro = GetEsperanzaActual() / 3;
      CambiarOroActual(oro);
      EscribirLog(TRADU.i.Traducir("-El Séquito de Nobles ha hecho una donación. Oro: ") + oro);
    }
    //Esclavos
    if (scMenuSequito.TieneSequito(11))
    {
      CambiarEsperanzaActual(-2);
      EscribirLog(TRADU.i.Traducir("-Los Civiles se sienten culpables por la presencia de los Esclavos. -2 Esperanza."));
    }


    BosqueArdienteMecanicaIncendio(25);
    PasoVientoHeladoMecanicaRituales(30);


    if (nodo.nodoIncendiado)
    {
      CambiarEsperanzaActual(-10);
      int nmuertos = UnityEngine.Random.Range(8, 16);
      CambiarCivilesActuales(-nmuertos);
      EscribirLog(TRADU.i.Traducir("-La caravana ha llegado a un nodo incendiado. -10 Esperanza.  ") + nmuertos + TRADU.i.Traducir(" Civiles Muertos."));
    }

    ActualizarBotonesAccionNodoActual();
  }



  public GameObject goUIVictoriaZona;

  public void EvaluarDerrotaPorResultadoBatalla(bool fueDefensaCaravana, bool fueBatallaFinal)
  {
    bool tutorialActivo = scTutorialManager != null && scTutorialManager.tutorialActivo;
    bool fueZonaExpuesta = false;
    if (scMapaManager != null && scMapaManager.nodoActual != null)
    {
      fueZonaExpuesta = scMapaManager.nodoActual.tipoNodo == 11;
    }

    if (!tutorialActivo && !fueDefensaCaravana && !fueBatallaFinal && !fueZonaExpuesta)
    {
      return;
    }

    ActivarDerrota();
  }

  public void ActivarDerrota()
  {
    if (goDerrota == null)
    {
      return;
    }

    if (!goDerrota.activeSelf && !DebeUsarConfiguracionTutorial())
    {
      RuntimeAnalytics.TrackProgressionFail("campaign", "new_game");
    }
    AplicarTraduccionPanelDerrota();
    goDerrota.SetActive(true);
  }

  private void EvaluarDerrotaPorEstadoCaravana()
  {
    if (inicializandoNuevaCampania)
    {
      return;
    }

    if (EsperanzaActual <= 0 || civilesActuales <= 0)
    {
      ActivarDerrota();
    }
  }

  string FormatearLogTiradaEmboscada(bool emboscadaEnemiga, int chanceEmboscadaEnemiga, int chanceEmboscadaAliada, int tirada)
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    string color = emboscadaEnemiga ? "#ff9a9a" : "#8ad8ff";
    int inicioEmboscadaAliada = chanceEmboscadaAliada > 0 ? 101 - chanceEmboscadaAliada : 0;
    string detalleEmboscadaEnemiga = chanceEmboscadaEnemiga > 0
      ? $"{chanceEmboscadaEnemiga}% (1-{chanceEmboscadaEnemiga})"
      : "0%";
    string detalleEmboscadaAliada = chanceEmboscadaAliada > 0
      ? $"{chanceEmboscadaAliada}% ({inicioEmboscadaAliada}-100)"
      : "0%";

    if (idioma == TRADU.IdiomaIngles)
    {
      string textoEn = emboscadaEnemiga
        ? "-The caravan was ambushed."
        : "-You ambushed the enemies.";
      string detalleResultadoEn = emboscadaEnemiga
        ? $"Enemy ambush: {detalleEmboscadaEnemiga}"
        : $"Ally ambush: {detalleEmboscadaAliada}";
      return $"<color={color}>{textoEn} {detalleResultadoEn} | 1d100: {tirada}.</color>";
    }

    if (idioma == TRADU.IdiomaPortugues)
    {
      string textoPt = emboscadaEnemiga
        ? "-A caravana sofreu uma emboscada."
        : "-Você emboscou os inimigos.";
      string detalleResultadoPt = emboscadaEnemiga
        ? $"Emboscada inimiga: {detalleEmboscadaEnemiga}"
        : $"Emboscada aliada: {detalleEmboscadaAliada}";
      return $"<color={color}>{textoPt} {detalleResultadoPt} | 1d100: {tirada}.</color>";
    }

    string textoEs = emboscadaEnemiga
      ? "-La caravana ha sido emboscada."
      : "-Has emboscado a los enemigos.";
    string detalleResultadoEs = emboscadaEnemiga
      ? $"Emboscada enemiga: {detalleEmboscadaEnemiga}"
      : $"Emboscada aliada: {detalleEmboscadaAliada}";
    return $"<color={color}>{textoEs} {detalleResultadoEs} | 1d100: {tirada}.</color>";
  }

  private void CachearTextosOriginalesDerrota()
  {
    if (goDerrota == null || textosDerrotaCacheados)
    {
      return;
    }

    textosOriginalesDerrotaTMP.Clear();
    TextMeshProUGUI[] tmps = goDerrota.GetComponentsInChildren<TextMeshProUGUI>(true);
    foreach (TextMeshProUGUI tmp in tmps)
    {
      if (tmp == null || string.IsNullOrWhiteSpace(tmp.text))
      {
        continue;
      }

      textosOriginalesDerrotaTMP[tmp] = tmp.text;
    }

    textosOriginalesDerrotaLegacy.Clear();
    Text[] textos = goDerrota.GetComponentsInChildren<Text>(true);
    foreach (Text txt in textos)
    {
      if (txt == null || string.IsNullOrWhiteSpace(txt.text))
      {
        continue;
      }

      textosOriginalesDerrotaLegacy[txt] = txt.text;
    }

    textosDerrotaCacheados = true;
  }

  private void AplicarTraduccionPanelDerrota()
  {
    if (goDerrota == null || TRADU.i == null)
    {
      return;
    }

    CachearTextosOriginalesDerrota();

    foreach (KeyValuePair<TextMeshProUGUI, string> entrada in textosOriginalesDerrotaTMP)
    {
      if (entrada.Key == null)
      {
        continue;
      }

      entrada.Key.text = TRADU.i.Traducir(entrada.Value);
    }

    foreach (KeyValuePair<Text, string> entrada in textosOriginalesDerrotaLegacy)
    {
      if (entrada.Key == null)
      {
        continue;
      }

      entrada.Key.text = TRADU.i.Traducir(entrada.Value);
    }
  }

  // Llamar desde el resultado de la Batalla Final (jefe derrotado)
  public void OnDerrotadoJefeZona()
  {
    EjecutarTareaSegura(OnDerrotadoJefeZonaAsync(), nameof(OnDerrotadoJefeZona));
  }

  private async Task OnDerrotadoJefeZonaAsync()
  {
    if (resolviendoJefeZona)
    {
      return;
    }

    resolviendoJefeZona = true;
    try
    {
    RuntimeAnalytics.TrackProgressionComplete(
      "zone",
      RuntimeAnalytics.ZoneToken(scAtributosZona.ID),
      RuntimeAnalytics.PhaseToken(scAtributosZona.FASE));
    if (scAtributosZona.FASE >= 3 && !DebeUsarConfiguracionTutorial())
    {
      RuntimeAnalytics.TrackProgressionComplete("campaign", "new_game");
    }
    if (scAtributosZona.FASE < 3) //Zona completada pero no es la final
    {



      goUIVictoriaZona.SetActive(true);

      CambiarSuministrosActuales(120);
      CambiarMaterialesActuales(40);
    }
    else
    {
      await AbrirCiudadPuertoAsync();
    }
    }
    finally
    {
      resolviendoJefeZona = false;
    }
  }

  public void AbrirCiudadPuerto()
  {
    EjecutarTareaSegura(AbrirCiudadPuertoAsync(), nameof(AbrirCiudadPuerto));
  }

  void AbrirCiudadPuertoDirectoDebug()
  {
    if (goLogCampania != null)
    {
      goLogCampania.SetActive(false);
    }

    AplicarTraitsLlegadaPuertoSerria();

    if (goMenuPuerto != null)
    {
      goMenuPuerto.SetActive(true);
    }

    Debug.Log("[CampaignManager] Debug activo: menu de Serria abierto al iniciar.");
  }

  private async Task AbrirCiudadPuertoAsync()
  {
      if (abriendoCiudadPuerto)
      {
        return;
      }

      abriendoCiudadPuerto = true;
      try
      {
        goLogCampania.SetActive(false);
        scAdministradorEscenas.PlayFadeInOut(1.2f, 2.0f);
        await BattleManager.DelayCombateAsync(2200);
        AplicarTraitsLlegadaPuertoSerria();
        goMenuPuerto.SetActive(true);
      }
      finally
      {
        abriendoCiudadPuerto = false;
      }
  }

  public void ContinuarASiguienteZona()
  {
    if (transicionZonaEnCurso) return;
    StartCoroutine(ContinuarASiguienteZonaCR());
  }

  IEnumerator ContinuarASiguienteZonaCR()
  {
    transicionZonaEnCurso = true;
    goLogCampania.SetActive(true);
    if (goUIVictoriaZona != null)
    {
      goUIVictoriaZona.SetActive(false);
    }

    if (scAdministradorEscenas != null)
    {
      // Tapar inmediatamente para que no se vea el reseteo/regeneracion del mapa.
      scAdministradorEscenas.SetFaderHold(true);
    }

    // Asegura que el frame negro se pinte antes de mutar nodos/escenario.
    yield return null;

    posicionCaravana = 1;
    scAtributosZona.ActualizarEstadoZona(scAtributosZona.ID, 1); //Zona completada
    LimpiarPersonajesPorCambioZona();
    FatigaActual = 0;
    ActualizarUIFatigaDesdeEstadoActual();
    scMapaManager.ResetearYGenerarSiguienteZona();
    ResetearAlientoNegro();
    PrepararIntroCampaniaNuevaZona();
    scAtributosZona.GenerarZona(0); //0 es aleatorio
    RegistrarZonaVisitada(scAtributosZona != null ? scAtributosZona.ID : 0);
    if (scAtributosZona != null)
    {
      RuntimeAnalytics.TrackProgressionStart(
        "zone",
        RuntimeAnalytics.ZoneToken(scAtributosZona.ID),
        RuntimeAnalytics.PhaseToken(scAtributosZona.FASE));
    }
    SolicitarInicioIntroCampaniaTrasCarga(true);
    AplicarTraitsInicioNuevaZona();
    transicionZonaEnCurso = false;
  }

  void LimpiarPersonajesPorCambioZona()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null)
      {
        continue;
      }

      pers.ResetearEstadoTraitsCombate();
      pers.SetCampFatigado(false);
      pers.LimpiarBendecido();
      pers.Camp_Herido = false;
      pers.LimpiarEnfermo();
      pers.LimpiarMoral();
      pers.Camp_Avergonzado = false;

      if (pers.Camp_Muerto)
      {
        continue;
      }

      pers.fVidaActual = pers.fVidaMaxima;
    }
  }

  public bool DesactivarIncendiosPorLluvia(bool escribirAvisoLog)
  {
    if (scMapaManager == null || scMapaManager.scContenedordeNodos == null || scMapaManager.scContenedordeNodos.listTodosNodos == null)
    {
      return false;
    }

    bool habiaIncendiosActivos = false;
    foreach (Nodo nodo in scMapaManager.scContenedordeNodos.listTodosNodos)
    {
      if (nodo == null || !nodo.nodoIncendiado)
      {
        continue;
      }

      nodo.DesactivarIncendio();
      habiaIncendiosActivos = true;
    }

    if (habiaIncendiosActivos && escribirAvisoLog)
    {
      EscribirLog("<color=#00c8ff>" + TRADU.i.Traducir("La lluvia apaga los focos de incendio actuales.") + "</color>");
    }

    return habiaIncendiosActivos;
  }

  public void BosqueArdienteMecanicaIncendio(int probabilidad)
  {
    if (DebeUsarConfiguracionTutorial()) { return; }
    if (scAtributosZona == null || scAtributosZona.ID != 1) { return; } // Solo en Bosque Ardiente

    if (intTipoClima == 3) // Lluvia: desactiva la mecanica y apaga incendios existentes
    {
      DesactivarIncendiosPorLluvia(false);
      return;
    }

    int randomIncendio = UnityEngine.Random.Range(1, 101);
    if (posicionCaravana < 9 && randomIncendio <= probabilidad)
    {
      const int codigoAsentamiento = 4;
      Nodo nodoAIncendiar = null;

      // Evita incendiar asentamientos por la mecanica del Bosque Ardiente.
      for (int intento = 0; intento < 6; intento++)
      {
        // El nodo inmediatamente contiguo a la caravana queda protegido.
        Nodo candidato = ObtenerNodoFuturoAleatorio(2);
        if (candidato == null) { continue; }
        if (candidato.nodoIncendiado) { continue; }
        if (candidato.tipoNodo == codigoAsentamiento) { continue; }
        if (candidato.tipoNodo == 16) { continue; }
        nodoAIncendiar = candidato;
        break;
      }

      if (nodoAIncendiar != null)
      {
        nodoAIncendiar.ActivarIncendio();
        MarcarNodoCampaniaTemporal(nodoAIncendiar, TipoHighlightNodoCampania.Incendio);
        EscribirLog(TRADU.i.Traducir("<color=#FF3D00>-El incendio ha envuelto un nodo cercano al camino de la caravana.</color>"));
      }
    }
  }

  public void PasoVientoHeladoMecanicaRituales(int probabilidad)
  {

    if (scAtributosZona.ID == 2) //Solo en Paso Viento Helado
    {
      List<Nodo> todosNodos = scMapaManager.scContenedordeNodos.listTodosNodos;
      bool yaHayRitual = false;
      foreach (Nodo n in todosNodos)
      {
        if (n.nodoRitual)
        {
          yaHayRitual = true;
          break;
        }
      }
      int randomritual = UnityEngine.Random.Range(1, 101);
      if (posicionCaravana < 11 && randomritual <= probabilidad && !yaHayRitual)
      {
        Nodo nodoRitual = null;
        for (int intento = 0; intento < 6; intento++)
        {
          int random = UnityEngine.Random.Range(1, 3);
          Nodo candidato = ObtenerNodoFuturoAleatorio(random);
          if (!EsNodoValidoParaRitualKaleTav(candidato))
          {
            continue;
          }

          nodoRitual = candidato;
          break;
        }

        if (nodoRitual != null)
        {
          nodoRitual.tipoNodoOriginalRitual = nodoRitual.tipoNodo;
          nodoRitual.tipoNodo = 15; // Nodo Ritual
          nodoRitual.ActivarNodoVisual(15, false, true);
          nodoRitual.ActivarRitual();
          MarcarNodoCampaniaTemporal(nodoRitual, TipoHighlightNodoCampania.Ritual);
          CambiarEsperanzaActual(-5);
          EscribirLog(TRADU.i.Traducir("<color=#6A0DAD>-Un ritual Kale'Tav ha comenzado en un nodo cercano. La másica profana desalienta a la caravana. -5 Esperanza.</color>"));




        }
      }

      // Buscar nodos activos con ritual detrás de la caravana, aumentar fuerza Kale'Tav, registrar y desactivar ritual

      foreach (Nodo nodoCandidato in todosNodos)
      {
        if (!nodoCandidato.gameObject.activeInHierarchy) { continue; }


        if (nodoCandidato.nodoRitual && nodoCandidato.posXNodo < posicionCaravana - 1)
        {
          scAtributosZona.PasoVientoHelado_FuerzaKaleTav++;
          EscribirLog(TRADU.i.Traducir("<color=#FF3D00>-Un ritual Kale'Tav ha sido completado. La fuerza de Kale'Tav aumenta en 1.</color>"));
          nodoCandidato.DesactivarRitual();
        }




      }

    }
  }

  bool EsNodoValidoParaRitualKaleTav(Nodo nodo)
  {
    if (nodo == null || !nodo.gameObject.activeInHierarchy)
    {
      return false;
    }

    if (nodo.nodoRitual || nodo.nodoIncendiado)
    {
      return false;
    }

    switch (nodo.tipoNodo)
    {
      case 4:
      case 10:
      case 14:
      case 15:
      case 16:
        return false;
      default:
        return true;
    }
  }

  bool EsNodoValidoParaMisionSalvamento(Nodo nodo)
  {
    if (nodo == null || !nodo.gameObject.activeInHierarchy)
    {
      return false;
    }

    if (nodo.nodoRitual || nodo.nodoIncendiado)
    {
      return false;
    }

    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    if (nodoActual != null && nodo.posXNodo <= nodoActual.posXNodo)
    {
      return false;
    }

    if (nodo.DestinosPosibles == null || !nodo.DestinosPosibles.Exists(destino => destino != null && destino.gameObject.activeInHierarchy))
    {
      return false;
    }

    switch (nodo.tipoNodo)
    {
      case 4:
      case 10:
      case 15:
      case 16:
        return false;
      default:
        return true;
    }
  }

  public Nodo ObtenerNodoFuturoAleatorio(int distancia = 0)
  {
    return ObtenerNodoFuturoAleatorio(distancia, null);
  }

  Nodo ObtenerNodoFuturoAleatorio(int distancia, Predicate<Nodo> filtroCandidato)
  {
    if (scMapaManager == null || scMapaManager.nodoActual == null)
    {
      return null;
    }

    if (distancia < 0)
    {
      distancia = 0;
    }

    var nodosPorDistancia = new Dictionary<int, List<Nodo>>();
    var visitados = new HashSet<Nodo>();
    var colaNodos = new Queue<Nodo>();
    var colaDistancias = new Queue<int>();

    var nodoOrigen = scMapaManager.nodoActual;
    visitados.Add(nodoOrigen);
    colaNodos.Enqueue(nodoOrigen);
    colaDistancias.Enqueue(0);

    while (colaNodos.Count > 0)
    {
      var nodoActual = colaNodos.Dequeue();
      int distanciaActual = colaDistancias.Dequeue();

      if (nodoActual.DestinosPosibles == null || nodoActual.DestinosPosibles.Count == 0)
      {
        continue;
      }

      foreach (var destino in nodoActual.DestinosPosibles)
      {
        if (destino == null) continue;
        if (!destino.gameObject.activeInHierarchy) continue;
        if (!visitados.Add(destino)) continue;

        int distanciaDestino = distanciaActual + 1;

        if (filtroCandidato == null || filtroCandidato(destino))
        {
          if (!nodosPorDistancia.TryGetValue(distanciaDestino, out var lista))
          {
            lista = new List<Nodo>();
            nodosPorDistancia[distanciaDestino] = lista;
          }

          lista.Add(destino);
        }

        colaNodos.Enqueue(destino);
        colaDistancias.Enqueue(distanciaDestino);
      }
    }

    if (nodosPorDistancia.Count == 0)
    {
      return null;
    }

    if (distancia == 0)
    {
      var distanciasDisponibles = new List<int>(nodosPorDistancia.Keys);
      distanciasDisponibles.Sort();

      var distanciasElegibles = new List<int>();
      foreach (var dist in distanciasDisponibles)
      {
        if (nodosPorDistancia[dist].Exists(n => n.DestinosPosibles != null && n.DestinosPosibles.Count > 0))
        {
          distanciasElegibles.Add(dist);
        }
      }

      var opciones = distanciasElegibles.Count > 0 ? distanciasElegibles : distanciasDisponibles;
      if (opciones.Count == 0)
      {
        return null;
      }

      int distanciaSeleccionada = opciones[UnityEngine.Random.Range(0, opciones.Count)];
      var candidatos = nodosPorDistancia[distanciaSeleccionada];
      return candidatos[UnityEngine.Random.Range(0, candidatos.Count)];
    }
    else
    {
      if (!nodosPorDistancia.TryGetValue(distancia, out var candidatos) || candidatos.Count == 0)
      {
        return null;
      }

      return candidatos[UnityEngine.Random.Range(0, candidatos.Count)];
    }
  }
  #region Santuario
  public GameObject goUIPersonajeSequito;


  public TextMeshProUGUI txtOro;
  public TextMeshProUGUI txtBueyes;

  public void RealizarRitualSantuario()
  {
    if (GetOroActuales() < 200)
    {
      txtOro.color = Color.red;
      return;
    }

    CambiarOroActual(-200);
    CambiarValorAlientoNegroHoras(-15f);
    EscribirLog(TRADU.i.Traducir("-Has realizado un ritual en el santuario. El Aliento Negro retrocede 15 h y se han gastado 200 de oro."));

    // Buscar personajes corruptos
    var corruptos = scMenuPersonajes.listaPersonajes.FindAll(p => p.Camp_Corrupto);
    if (corruptos.Count > 0)
    {
      var personajeCurado = corruptos[UnityEngine.Random.Range(0, corruptos.Count)];
      personajeCurado.Camp_Corrupto = false;
      EscribirLog(personajeCurado.sNombre + TRADU.i.Traducir(" ha sido purificado de la corrupción."));
    }
    else
    {
      EscribirAdvertenciaLog(TRADU.i.Traducir("-No hay personajes corruptos para purificar."));
    }

    goUISantuario.SetActive(false);

    if (scTutorialManager.pasoActual == 20) { scTutorialManager.SiguientePaso(); }
  }

  public void abandonarsantuario() { goUISantuario.SetActive(false);    if (scTutorialManager.pasoActual == 20) { scTutorialManager.SiguientePaso(); }
 }
  public void RealizarRitualSantuarioPorBueyes()
  {
    if (GetBueyesActual() < 3)
    {
      txtBueyes.color = Color.red;
      EscribirAdvertenciaLog(TRADU.i.Traducir("-No tienes suficientes bueyes para realizar el ritual en el santuario."));
      return;
    }

    CambiarBueyesActuales(-3);
    CambiarValorAlientoNegroHoras(-15f);
    EscribirLog(TRADU.i.Traducir("-Has realizado un ritual en el santuario. El Aliento Negro retrocede 15 h y se han sacrificado 3 bueyes."));

    // Buscar personajes corruptos
    var corruptos = scMenuPersonajes.listaPersonajes.FindAll(p => p.Camp_Corrupto);
    if (corruptos.Count > 0)
    {
      var personajeCurado = corruptos[UnityEngine.Random.Range(0, corruptos.Count)];
      personajeCurado.Camp_Corrupto = false;
      EscribirLog("-" + personajeCurado.sNombre + TRADU.i.Traducir(" ha sido purificado de la corrupción."));
    }
    else
    {
      EscribirAdvertenciaLog(TRADU.i.Traducir("-No hay personajes corruptos para purificar."));
    }
    goUISantuario.SetActive(false);

    if (scTutorialManager.pasoActual == 20) { scTutorialManager.SiguientePaso(); }


  }
  #endregion

  public void BendecirPersonajesSantuario(float horas)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null || personaje.Camp_Muerto)
      {
        continue;
      }

      personaje.AplicarBendecidoHoras(horas);
    }
  }

  #region Puesto Comercial
  public GameObject goUIComercioNodo;
  public GameObject goUISantuario;
  public TextMeshProUGUI txtdescripcionSantuario;

  public TextMeshProUGUI txtDescripcionPuestoComercial;

  public TextMeshProUGUI txtComnercialSumDisp;
  public TextMeshProUGUI txtComnercialMatDisp;
  public TextMeshProUGUI txtComnercialBueyesDisp;
  [SerializeField] private TextMeshProUGUI txtOroCaravana;

  public TextMeshProUGUI txtDescSum;
  public TextMeshProUGUI txtDescMat;
  public TextMeshProUGUI txtDescBuey;

  public GameObject btnCompraSum;
  public GameObject btnVentaSum;
  public GameObject btnCompraMat;
  public GameObject btnVentaMat;
  public GameObject btnCompraBuey;
  public GameObject btnVentaBuey;

  public int pComercialSuministrosDisp;
  public int pComercialMaterialesDisp;
  public int pComercialBueyesDisp;

  const float MULTIPLICADOR_STOCK_PUESTO_COMERCIAL_ASENTAMIENTO = 1.2f;
  const float MULTIPLICADOR_PRECIO_PUESTO_COMERCIAL_ASENTAMIENTO = 0.9f;

  float precio10Suministros;
  float precio1Material;
  float precio1Buey;

  public void ResetearPuestoComercial()
  {
    pComercialSuministrosDisp = Mathf.RoundToInt(UnityEngine.Random.Range(15, 30) * MULTIPLICADOR_STOCK_PUESTO_COMERCIAL_ASENTAMIENTO);
    pComercialSuministrosDisp *= 10;
    pComercialMaterialesDisp = Mathf.RoundToInt(UnityEngine.Random.Range(15, 30) * MULTIPLICADOR_STOCK_PUESTO_COMERCIAL_ASENTAMIENTO);
    pComercialBueyesDisp = Mathf.RoundToInt(UnityEngine.Random.Range(5, 15) * MULTIPLICADOR_STOCK_PUESTO_COMERCIAL_ASENTAMIENTO);

    precio10Suministros = 15 - sequitoMercaderesTier;
    precio1Material = 18 - sequitoMercaderesTier;
    precio1Buey = 20 - sequitoMercaderesTier;

    int tierPalacio = MetaprogresionManager.Instance.SerriaTierPalacio;
    float descuento = Mathf.Max(0f, 1f - 0.1f * tierPalacio) * MULTIPLICADOR_PRECIO_PUESTO_COMERCIAL_ASENTAMIENTO; // cada tier baja 10% los precios
    precio10Suministros *= descuento;
    precio1Material *= descuento;
    precio1Buey *= descuento;

    int costoCompraSuministros = ObtenerCostoPuestoComercialConPresagios(precio10Suministros);
    int costoCompraMaterial = ObtenerCostoPuestoComercialConPresagios(precio1Material);
    int costoCompraBuey = ObtenerCostoPuestoComercialConPresagios(precio1Buey);


    if (TRADU.i.nIdioma == 1) //Español
    {
      txtDescSum.text = $"<Color=#F26B70>Venta: {(int)precio10Suministros / 2} Oro</color>    x10   <Color=#5ABD46>Compra: {costoCompraSuministros} Oro</color>";
      txtDescMat.text = $"<Color=#F26B70>Venta: {(int)precio1Material / 2} Oro</color>    x1   <Color=#5ABD46>Compra: {costoCompraMaterial} Oro</color>";
      txtDescBuey.text = $"<Color=#F26B70>Venta: {(int)precio1Buey / 2}  Oro</color>    x1   <Color=#5ABD46>Compra: {costoCompraBuey} Oro</color>";
    }
    else if (TRADU.i.nIdioma == 2) //Inglés
    {
      txtDescSum.text = $"<Color=#F26B70>Sell: {(int)precio10Suministros / 2} Gold</color>    x10   <Color=#5ABD46>Buy: {costoCompraSuministros} Gold</color>";
      txtDescMat.text = $"<Color=#F26B70>Sell: {(int)precio1Material / 2} Gold</color>    x1   <Color=#5ABD46>Buy: {costoCompraMaterial} Gold</color>";
      txtDescBuey.text = $"<Color=#F26B70>Sell: {(int)precio1Buey / 2}  Gold</color>    x1   <Color=#5ABD46>Buy: {costoCompraBuey} Gold</color>";
    }
    ActualizarPuestoComercial();
  }

  public void ActualizarPuestoComercial()
  {
    ActualizarOroPuestoComercial();
    txtComnercialSumDisp.text = "" + pComercialSuministrosDisp;
    txtComnercialMatDisp.text = "" + pComercialMaterialesDisp;
    txtComnercialBueyesDisp.text = "" + pComercialBueyesDisp;

    if ((pComercialSuministrosDisp > 0) && (GetOroActuales() >= ObtenerCostoPuestoComercialConPresagios(precio10Suministros)))
    {
      btnCompraSum.SetActive(true);
    }
    else { btnCompraSum.SetActive(false); }

    if (GetSuministrosActuales() > 0)
    {
      btnVentaSum.SetActive(true);
    }
    else { btnVentaSum.SetActive(false); }



    if ((pComercialMaterialesDisp > 0) && (GetOroActuales() >= ObtenerCostoPuestoComercialConPresagios(precio1Material)))
    {
      btnCompraMat.SetActive(true);
    }
    else { btnCompraMat.SetActive(false); }

    if (GetMaterialesActuales() > 0)
    {
      btnVentaMat.SetActive(true);
    }
    else { btnVentaMat.SetActive(false); }


    if ((pComercialBueyesDisp > 0) && (GetOroActuales() >= ObtenerCostoPuestoComercialConPresagios(precio1Buey)))
    {
      btnCompraBuey.SetActive(true);
    }
    else { btnCompraBuey.SetActive(false); }

    if (GetBueyesActual() > 0)
    {
      btnVentaBuey.SetActive(true);
    }
    else { btnVentaBuey.SetActive(false); }

  }

  private void ActualizarOroPuestoComercial()
  {
    if (txtOroCaravana == null && goUIComercioNodo != null)
    {
      TextMeshProUGUI[] textosPuestoComercial = goUIComercioNodo.GetComponentsInChildren<TextMeshProUGUI>(true);
      foreach (TextMeshProUGUI texto in textosPuestoComercial)
      {
        if (texto != null && texto.gameObject.name == "txtOrocaravana")
        {
          txtOroCaravana = texto;
          break;
        }
      }
    }

    if (txtOroCaravana != null)
    {
      txtOroCaravana.text = OroActuales.ToString();
    }
  }

  public void ComprarSum()
  {
    CambiarOroActual(-ObtenerCostoPuestoComercialConPresagios(precio10Suministros));
    CambiarSuministrosActuales(10);
    pComercialSuministrosDisp -= 10;

    ActualizarPuestoComercial();

  }

  public void VenderSum()
  {
    CambiarOroActual((int)precio10Suministros / 2);
    CambiarSuministrosActuales(-10);
    pComercialSuministrosDisp += 10;

    ActualizarPuestoComercial();

  }

  public void ComprarMat()
  {
    CambiarOroActual(-ObtenerCostoPuestoComercialConPresagios(precio1Material));
    CambiarMaterialesActuales(1);
    pComercialMaterialesDisp--;

    ActualizarPuestoComercial();

  }

  public void VenderMat()
  {
    CambiarOroActual((int)precio1Material / 2);
    CambiarMaterialesActuales(-1);
    pComercialMaterialesDisp++;

    ActualizarPuestoComercial();

  }

  public void ComprarBuey()
  {
    CambiarOroActual(-ObtenerCostoPuestoComercialConPresagios(precio1Buey));
    CambiarBueyesActuales(1);
    pComercialBueyesDisp--;

    ActualizarPuestoComercial();

  }

  public void VenderBuey()
  {
    CambiarOroActual((int)precio1Buey / 2);
    CambiarBueyesActuales(-1);
    pComercialBueyesDisp++;

    ActualizarPuestoComercial();
  }

  public void cerrarPuestoComercial()
  {
    goUIComercioNodo.SetActive(false);

    if (asentamientoManager != null)
    {
      asentamientoManager.OnPuestoComercialCerrado();
    }
  }

  public void AbrirPuestoComercial(bool resetearPuesto = true)
  {
    if (goUIComercioNodo == null)
    {
      return;
    }

    goUIComercioNodo.SetActive(true);
    txtDescripcionPuestoComercial.text = TRADU.i.Traducir("Has llegado a un improvisado <b>Puesto Comercial</b>, ofrecen Suministros básicos de supervivencia a los viajeros.\nEl Tier de tu Séquito de Mercaderes ayudará a bajar los precios.\n\nTu Séquito de Mercaderes ha actualizado su Inventario.\n\nSi descansas aquí, los civiles entablarán relaciones comerciales, generando 2 Oro cada uno.");

    if (resetearPuesto)
    {
      ResetearPuestoComercial();
      AplicarTraitsVisitaPuestoComercial();
      if (scSequitoMercaderes != null)
      {
        scSequitoMercaderes.GenerarItemsVendidos();
      }
    }
    else
    {
      ActualizarPuestoComercial();
    }

    //EscribirLog(TRADU.i.Traducir("El Séquito de Mercaderes ha actualizado su inventario en el Puesto Comercial."));
  }



  #endregion


  #endregion

  public int CuantosPersonajesHacenTalActividad(int IDActividad)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int cant = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers != null
        && !pers.Camp_Muerto
        && pers.PuedeRealizarActividades()
        && pers.ActividadSeleccionada == IDActividad
        && EsActividadPermitidaPorClimaCampania(IDActividad))
      {
        cant++;
      }
    }

    return cant;
  }

  public bool EsActividadPermitidaPorClimaCampania(int idActividad)
  {
    if (intTipoClima != 4)
    {
      return true;
    }

    return idActividad == 1
      || idActividad == 3
      || idActividad == 6
      || idActividad == 14;
  }

  public int NormalizarActividadesPorClimaCampania()
  {
    if (intTipoClima != 4 || scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int cambios = 0;
    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null
        || personaje.Camp_Muerto
        || !personaje.PuedeRealizarActividades()
        || EsActividadPermitidaPorClimaCampania(personaje.ActividadSeleccionada))
      {
        continue;
      }

      personaje.ActividadSeleccionada = 1;
      cambios++;
    }

    if (cambios > 0)
    {
      RefrescarRetratosPersonajesCampania(true);
      if (scMenuPersonajes.scActividades != null)
      {
        scMenuPersonajes.scActividades.ActualizarRecuadros();
      }
      EvaluarTooltipPersonajeDescansando();
    }

    return cambios;
  }

  private int ObtenerTSMentalTotalCampania(Personaje personaje)
  {
    if (personaje == null)
    {
      return 0;
    }

    int total = personaje.iTSMental;

    if (personaje.itemArma != null) total += personaje.itemArma.buffTSMental;
    if (personaje.itemArmadura != null) total += personaje.itemArmadura.buffTSMental;
    if (personaje.Accesorio1 != null) total += personaje.Accesorio1.buffTSMental;
    if (personaje.Accesorio2 != null) total += personaje.Accesorio2.buffTSMental;
    if (personaje.EstaBendecido()) total += 3;

    return total;
  }

  public int CuantosPersonajesActivos()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int cant = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers != null && !pers.Camp_Muerto)
      {
        cant++;
      }
    }

    return cant;
  }

  public void TodosDescansar()
  {
    CambiarActividadDeTodosLosPersonajes(1, "Descansar");
  }

  public void TodosGuardia()
  {
    CambiarActividadDeTodosLosPersonajes(3, "Guardia");
  }

  private void CambiarActividadDeTodosLosPersonajes(int idActividad, string nombreActividad)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null || personaje.Camp_Muerto || !personaje.PuedeRealizarActividades() || personaje.ActividadFijada)
      {
        continue;
      }

      int actividadAnterior = personaje.ActividadSeleccionada;
      personaje.ActividadSeleccionada = idActividad;

      if (actividadAnterior != idActividad
        && personaje.TieneRasgo(PersonajeTraitCatalog.TraitDesganado))
      {
        personaje.AplicarMoralBajaHoras(48f);
      }
    }

    RefrescarRetratosPersonajesCampania(true);

    if (scMenuPersonajes.scActividades != null)
    {
      scMenuPersonajes.scActividades.ActualizarRecuadros();
    }

    string mensaje = TRADU.i != null
      ? TRADU.i.Traducir("-La actividad de todos los personajes ahora es: ") + TRADU.i.Traducir(nombreActividad)
      : "-La actividad de todos los personajes ahora es: " + nombreActividad;
    EscribirLog(mensaje, true);
    EvaluarTooltipPersonajeDescansando();
  }

  public int CuantosPersonajesSonDeTalClase(int IdClase)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int cant = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers != null && !pers.Camp_Muerto && pers.IDClase == IdClase)
      {
        cant++;
      }
    }

    return cant;
  }

  public int ObtenerModificadorChanceEventoTraits()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int modificador = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitOptimista))
      {
        modificador += 5;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitPesimista))
      {
        modificador -= 5;
      }
    }

    return modificador;
  }

  public int ObtenerModificadorChanceEmboscadaTraits()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int modificador = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitCuidadoso))
      {
        modificador -= 3;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitRudioso))
      {
        modificador += 3;
      }

      if (TieneTraitConocimientoZonaActual(pers))
      {
        modificador -= 3;
      }
    }

    return modificador;
  }

  public int ObtenerModificadorChanceExploracionTraits()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int modificador = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (TieneTraitConocimientoZonaActual(pers))
      {
        modificador += 5;
      }
    }

    return modificador;
  }

  bool TieneTraitConocimientoZonaActual(Personaje pers)
  {
    if (pers == null || scAtributosZona == null)
    {
      return false;
    }

    return scAtributosZona.ID switch
    {
      1 => pers.TieneRasgo(PersonajeTraitCatalog.TraitConoceBosqueArdiente),
      2 => pers.TieneRasgo(PersonajeTraitCatalog.TraitConocePasoVientohelado),
      3 => pers.TieneRasgo(PersonajeTraitCatalog.TraitConoceNedukazal),
      _ => false
    };
  }

  bool TieneTraitDetestaZonaActual(Personaje pers)
  {
    if (pers == null || scAtributosZona == null)
    {
      return false;
    }

    return scAtributosZona.ID switch
    {
      1 => pers.TieneRasgo(PersonajeTraitCatalog.TraitDetestaBosqueArdiente),
      2 => pers.TieneRasgo(PersonajeTraitCatalog.TraitDetestaPasoVientohelado),
      3 => pers.TieneRasgo(PersonajeTraitCatalog.TraitDetestaNedukazal),
      _ => false
    };
  }

  float ObtenerPesoParticipacionEventoTraits(Personaje pers)
  {
    if (pers == null || pers.Camp_Muerto)
    {
      return 0f;
    }

    float peso = 1f;
    if (pers.TieneRasgo(PersonajeTraitCatalog.TraitProtagonista))
    {
      peso *= 2f;
    }

    if (pers.TieneRasgo(PersonajeTraitCatalog.TraitPerfilBajo))
    {
      peso *= 0.5f;
    }

    return Mathf.Max(0.01f, peso);
  }

  public void AplicarTraitsVisitaPuestoComercial()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitAhorrista))
      {
        int oroGanado = UnityEngine.Random.Range(50, 101);
        CambiarOroActual(oroGanado);
        string mensajeAhorrista = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " finds a good deal at the Trading Post. +" + oroGanado + " Gold.",
          TRADU.IdiomaPortugues => pers.sNombre + " encontra um bom negócio no Posto Comercial. +" + oroGanado + " de Ouro.",
          _ => pers.sNombre + " consigue un gran trato en el Puesto Comercial. +" + oroGanado + " Oro."
        };
        EscribirLog("-" + mensajeAhorrista);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitDespilfarrador))
      {
        int oroPerdido = UnityEngine.Random.Range(20, 51);
        CambiarOroActual(-oroPerdido);
        string mensajeDespilfarrador = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " wastes coin at the Trading Post. -" + oroPerdido + " Gold.",
          TRADU.IdiomaPortugues => pers.sNombre + " desperdiça moedas no Posto Comercial. -" + oroPerdido + " de Ouro.",
          _ => pers.sNombre + " despilfarra en el Puesto Comercial. -" + oroPerdido + " Oro."
        };
        EscribirLog("-" + mensajeDespilfarrador);
      }
    }
  }

  public void AplicarTraitsVisitaSantuario()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitFeInquebrantable))
      {
        pers.AplicarMoralAltaHoras(96f);
        string mensajeFe = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " is uplifted by the Sanctuary. Gains High Morale for 4 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " se fortalece no Santuário. Recebe Moral Alta por 4 dias.",
          _ => pers.sNombre + " se fortalece en el Santuario. Obtiene Alta Moral por 4 días."
        };
        EscribirLog("-" + mensajeFe);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitPagano))
      {
        pers.AplicarMoralBajaHoras(72f);
        string mensajePagano = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " rejects the Sanctuary. Gains Low Morale for 3 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " rejeita o Santuário. Recebe Moral Baixa por 3 dias.",
          _ => pers.sNombre + " rechaza el Santuario. Obtiene Baja Moral por 3 días."
        };
        EscribirLog("-" + mensajePagano);
      }
    }
  }

  public void AplicarTraitsInicioNuevaZona()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null || scAtributosZona == null)
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    string nombreZona = scAtributosZona.ID switch
    {
      1 => idiomaTrait switch
      {
        TRADU.IdiomaIngles => "the Burning Forest",
        TRADU.IdiomaPortugues => "a Floresta Ardente",
        _ => "Bosque Ardiente"
      },
      2 => idiomaTrait switch
      {
        TRADU.IdiomaIngles => "the Windfrost Pass",
        TRADU.IdiomaPortugues => "o Paso de Vientohelado",
        _ => "Paso de Vientohelado"
      },
      3 => "Nedukazal",
      _ => scAtributosZona.Nombre
    };

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitAventurero))
      {
        pers.AplicarMoralAltaHoras(96f);
        string mensajeAventurero = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " embraces the new region. Gains High Morale for 4 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " abraça a nova região. Recebe Moral Alta por 4 dias.",
          _ => pers.sNombre + " abraza la nueva región. Obtiene Alta Moral por 4 días."
        };
        EscribirLog("-" + mensajeAventurero);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitArrastrado))
      {
        pers.AplicarMoralBajaHoras(72f);
        string mensajeArrastrado = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " dreads the new region. Gains Low Morale for 3 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " teme a nova região. Recebe Moral Baixa por 3 dias.",
          _ => pers.sNombre + " teme la nueva región. Obtiene Baja Moral por 3 días."
        };
        EscribirLog("-" + mensajeArrastrado);
      }

      if (TieneTraitDetestaZonaActual(pers))
      {
        pers.AplicarMoralBajaHoras(144f);
        string mensajeDetesta = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " dreads " + nombreZona + ". Gains Low Morale for 6 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " detesta " + nombreZona + ". Recebe Moral Baixa por 6 dias.",
          _ => pers.sNombre + " detesta " + nombreZona + ". Obtiene Baja Moral por 6 días."
        };
        EscribirLog("-" + mensajeDetesta);
      }
    }
  }

  public void AplicarTraitsLlegadaAsentamiento()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitTrabajador))
      {
        int oroGanado = UnityEngine.Random.Range(150, 201);
        CambiarOroActual(oroGanado);
        string mensajeTrabajador = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " profits from the settlement. +" + oroGanado + " Gold.",
          TRADU.IdiomaPortugues => pers.sNombre + " aproveita o assentamento. +" + oroGanado + " de Ouro.",
          _ => pers.sNombre + " saca provecho del asentamiento. +" + oroGanado + " Oro."
        };
        EscribirLog("-" + mensajeTrabajador);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitBeodo))
      {
        CambiarFatigaActual(1);
        CambiarOroActual(-10);
        string mensajeBeodo = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " drinks away the stop. Gains Fatigue and spends 10 Gold.",
          TRADU.IdiomaPortugues => pers.sNombre + " bebe demais na parada. Recebe Fadiga e gasta 10 de Ouro.",
          _ => pers.sNombre + " se entrega a la bebida en la parada. Obtiene Fatiga y gasta 10 Oro."
        };
        EscribirLog("-" + mensajeBeodo);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitConvincente))
      {
        CambiarCivilesActuales(5);
        string mensajeConvincente = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " wins over the locals. +5 Civilians.",
          TRADU.IdiomaPortugues => pers.sNombre + " convence os moradores. +5 Civis.",
          _ => pers.sNombre + " convence a los lugareños. +5 Civiles."
        };
        EscribirLog("-" + mensajeConvincente);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitHermitano))
      {
        pers.AplicarMoralBajaHoras(48f);
        string mensajeHermitano = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " withdraws from settlement life. Gains Low Morale for 2 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " se retrai da vida no assentamento. Recebe Moral Baixa por 2 dias.",
          _ => pers.sNombre + " se incomoda con la vida del asentamiento. Obtiene Baja Moral por 2 días."
        };
        EscribirLog("-" + mensajeHermitano);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitCitadino))
      {
        pers.AplicarMoralAltaHoras(72f);
        string mensajeCitadino = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " thrives in the settlement. Gains High Morale for 3 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " se anima no assentamento. Recebe Moral Alta por 3 dias.",
          _ => pers.sNombre + " se siente en casa en el asentamiento. Obtiene Alta Moral por 3 días."
        };
        EscribirLog("-" + mensajeCitadino);
      }
    }
  }

  public int ObtenerModificadorCivilesPlazaAsentamiento()
  {
    int modificador = 0;
    modificador += CuantosPersonajesTienenTraitActivo(PersonajeTraitCatalog.TraitBuenaReputacion) * 2;
    modificador -= CuantosPersonajesTienenTraitActivo(PersonajeTraitCatalog.TraitMalaReputacion) * 2;
    if (TienePresagioActivo(PresagioCatalog.PobladosVividos))
    {
      modificador += 5;
    }
    return modificador;
  }

  public void AplicarTraitHeroeLocalInicialSiCorresponde(Personaje pers)
  {
    if (pers == null || !pers.TieneRasgo(PersonajeTraitCatalog.TraitHeroeLocal) || pers.TraitHeroeLocalCivilesOtorgados)
    {
      return;
    }

    pers.TraitHeroeLocalCivilesOtorgados = true;
    CambiarCivilesActuales(15);

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    string mensajeHeroeLocal = idiomaTrait switch
    {
      TRADU.IdiomaIngles => pers.sNombre + " is already known by the people. +15 Civilians join the Caravan.",
      TRADU.IdiomaPortugues => pers.sNombre + " ja e conhecido pelo povo. +15 Civis se juntam à Caravana.",
      _ => pers.sNombre + " ya es conocido por la gente. +15 Civiles se suman a la Caravana."
    };
    EscribirLog("-" + mensajeHeroeLocal);
  }

  private Item ObtenerItemHerenciaBase(Personaje pers)
  {
    if (pers == null)
    {
      return null;
    }

    List<Item> candidatos = new List<Item>(2);
    if (pers.itemArma != null)
    {
      candidatos.Add(pers.itemArma);
    }

    if (pers.itemArmadura != null)
    {
      candidatos.Add(pers.itemArmadura);
    }

    if (candidatos.Count == 0)
    {
      return null;
    }

    return candidatos[UnityEngine.Random.Range(0, candidatos.Count)];
  }

  private Consumible CrearConsumibleAleatorioHerencia(ItemDatabase itemDatabase)
  {
    List<ItemDatabaseEntry> candidatos = new List<ItemDatabaseEntry>();
    if (itemDatabase != null && itemDatabase.items != null)
    {
      for (int i = 0; i < itemDatabase.items.Count; i++)
      {
        ItemDatabaseEntry entry = itemDatabase.items[i];
        if (entry == null || !entry.activo || entry.prefab == null || !(entry.prefab is Consumible))
        {
          continue;
        }

        candidatos.Add(entry);
      }
    }

    if (candidatos.Count > 0)
    {
      ItemDatabaseEntry entry = candidatos[UnityEngine.Random.Range(0, candidatos.Count)];
      return ItemSaveCatalog.InstantiateItemById(entry.id, itemDatabase) as Consumible;
    }

    if (scContprefab != null && scContprefab.pocioncuracion != null)
    {
      Consumible consumible = Instantiate(scContprefab.pocioncuracion);
      return consumible;
    }

    return null;
  }

  private void AplicarNivelMejoraHerencia(Item item, int nivelMejora)
  {
    if (item == null || nivelMejora <= 0)
    {
      return;
    }

    item.nivelMejora = nivelMejora;
    if (string.IsNullOrWhiteSpace(item.sNombreItem))
    {
      return;
    }

    string sufijo = " +" + nivelMejora;
    if (item.sNombreItem.EndsWith(sufijo, StringComparison.Ordinal))
    {
      return;
    }

    int indiceSufijoAnterior = item.sNombreItem.LastIndexOf(" +", StringComparison.Ordinal);
    if (indiceSufijoAnterior >= 0)
    {
      string posibleNivel = item.sNombreItem.Substring(indiceSufijoAnterior + 2);
      if (int.TryParse(posibleNivel, out _))
      {
        item.sNombreItem = item.sNombreItem.Substring(0, indiceSufijoAnterior);
      }
    }

    item.sNombreItem += sufijo;
  }

  public void AplicarTraitHerenciaInicialSiCorresponde(Personaje pers)
  {
    if (pers == null || !pers.TieneRasgo(PersonajeTraitCatalog.TraitHerencia) || pers.TraitHerenciaItemOtorgado)
    {
      return;
    }

    ItemDatabase itemDatabase = ItemSaveCatalog.GetRuntimeItemDatabase(this);
    Item itemBase = ObtenerItemHerenciaBase(pers);
    string nombreItem = string.Empty;
    bool entregoConsumible = false;

    if (itemBase != null)
    {
      AplicarNivelMejoraHerencia(itemBase, UnityEngine.Random.Range(1, 3));
      if (itemDatabase != null)
      {
        ItemSaveCatalog.ResolveItemId(itemBase, itemDatabase);
      }

      nombreItem = itemBase.sNombreItem;
    }
    else
    {
      Consumible consumibleAleatorio = CrearConsumibleAleatorioHerencia(itemDatabase);
      if (consumibleAleatorio == null)
      {
        return;
      }

      if (pers.Consumible1 == null)
      {
        pers.Consumible1 = consumibleAleatorio;
      }
      else if (pers.Consumible2 == null)
      {
        pers.Consumible2 = consumibleAleatorio;
      }
      else
      {
        pers.Consumible1 = consumibleAleatorio;
      }

      nombreItem = consumibleAleatorio.sNombreItem;
      entregoConsumible = true;
    }

    pers.TraitHerenciaItemOtorgado = true;

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    string mensajeHerencia = entregoConsumible
      ? idiomaTrait switch
      {
        TRADU.IdiomaIngles => pers.sNombre + " receives a random consumable: " + nombreItem + ".",
        TRADU.IdiomaPortugues => pers.sNombre + " recebe um consumível aleatório: " + nombreItem + ".",
        _ => pers.sNombre + " recibe un consumible aleatorio: " + nombreItem + "."
      }
      : idiomaTrait switch
      {
        TRADU.IdiomaIngles => pers.sNombre + " inherits " + nombreItem + ".",
        TRADU.IdiomaPortugues => pers.sNombre + " herda " + nombreItem + ".",
        _ => pers.sNombre + " hereda " + nombreItem + "."
      };
    EscribirLog("-" + mensajeHerencia);
  }

  public void AplicarTraitHeroeLocalMuerteSiCorresponde(Personaje pers)
  {
    AplicarTraitLiderCaravanaMuerteSiCorresponde(pers);

    if (pers == null || !pers.Camp_Muerto || !pers.TieneRasgo(PersonajeTraitCatalog.TraitHeroeLocal) || pers.TraitHeroeLocalPenalidadMuerteAplicada)
    {
      return;
    }

    pers.TraitHeroeLocalPenalidadMuerteAplicada = true;
    CambiarEsperanzaActual(-20);

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    string mensajeHeroeLocal = idiomaTrait switch
    {
      TRADU.IdiomaIngles => pers.sNombre + " dies for good. -20 Hope.",
      TRADU.IdiomaPortugues => pers.sNombre + " morre de vez. -20 de Esperança.",
      _ => pers.sNombre + " muere para siempre. -20 Esperanza."
    };
    EscribirLog("-" + mensajeHeroeLocal);
  }

  private void AplicarTraitLiderCaravanaMuerteSiCorresponde(Personaje pers)
  {
    if (pers == null
      || !pers.Camp_Muerto
      || !pers.TieneRasgo(PersonajeTraitCatalog.TraitLiderCaravana)
      || pers.TraitLiderCaravanaPenalidadMuerteAplicada)
    {
      return;
    }

    pers.TraitLiderCaravanaPenalidadMuerteAplicada = true;
    CambiarEsperanzaActual(-25);

    if (scMenuPersonajes != null && scMenuPersonajes.listaPersonajes != null)
    {
      foreach (Personaje aliado in scMenuPersonajes.listaPersonajes)
      {
        if (aliado == null || aliado.Camp_Muerto)
        {
          continue;
        }

        aliado.AplicarMoralBajaHoras(96f);
      }
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    string mensajeLider = idiomaTrait switch
    {
      TRADU.IdiomaIngles => pers.sNombre + ", the Caravan Protector, dies. -25 Hope.",
      TRADU.IdiomaPortugues => pers.sNombre + ", Protetor da Caravana, morre. -25 de Esperança.",
      _ => pers.sNombre + ", Protector de la Caravana, muere. -25 Esperanza."
    };
    EscribirLog("-" + mensajeLider);

    string mensajeMoral = idiomaTrait switch
    {
      TRADU.IdiomaIngles => "The Protector's death demoralizes the caravan. Everyone gains Low Morale for 4 days.",
      TRADU.IdiomaPortugues => "A morte do Protetor desmoraliza a caravana. Todos recebem Moral Baixa por 4 dias.",
      _ => "La muerte del Protector desmoraliza a la caravana. Todos obtienen Baja Moral por 4 días."
    };
    EscribirLog("-" + mensajeMoral);
  }

  public void ProcesarTraitContratoSiCorresponde(Personaje pers)
  {
    if (pers == null || pers.Camp_Muerto || !pers.TieneRasgo(PersonajeTraitCatalog.TraitContrato))
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    if (GetOroActuales() >= 50)
    {
      CambiarOroActual(-50);
      string mensajeContrato = idiomaTrait switch
      {
        TRADU.IdiomaIngles => pers.sNombre + " collects 50 Gold from the caravan contract.",
        TRADU.IdiomaPortugues => pers.sNombre + " recebe 50 de Ouro pelo contrato com a caravana.",
        _ => pers.sNombre + " cobra 50 de Oro por su contrato con la caravana."
      };
      EscribirLog("-" + mensajeContrato);
      return;
    }

    pers.AplicarMoralBajaHoras(72f);
    string mensajeSinOro = idiomaTrait switch
    {
      TRADU.IdiomaIngles => pers.sNombre + " cannot be paid. Gains Low Morale for 3 days.",
      TRADU.IdiomaPortugues => pers.sNombre + " não pode ser pago. Recebe Moral Baixa por 3 dias.",
      _ => pers.sNombre + " no puede cobrar. Obtiene Baja Moral por 3 días."
    };
    EscribirLog("-" + mensajeSinOro);
  }

  public void AplicarTraitsLlegadaPuertoSerria()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null || MetaprogresionManager.Instance == null)
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto || !pers.TieneRasgo(PersonajeTraitCatalog.TraitEjemploASeguir) || pers.TraitEjemploASeguirAplicado)
      {
        continue;
      }

      pers.TraitEjemploASeguirAplicado = true;
      MetaprogresionManager.Instance.ValordeTrabajoDisponible += 25;

      string mensajeEjemplo = idiomaTrait switch
      {
        TRADU.IdiomaIngles => pers.sNombre + " inspires Serria. +25 Work Value.",
        TRADU.IdiomaPortugues => pers.sNombre + " inspira Serria. +25 de Valor de Trabalho.",
        _ => pers.sNombre + " inspira a Serria. +25 Valor de Trabajo."
      };
      EscribirLog("-" + mensajeEjemplo);
    }
  }

  void AplicarTraitFlojoSiCorresponde(int fatigaAnterior, int fatigaAhora)
  {
    if (fatigaAnterior >= 4 || fatigaAhora < 4 || scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto || !pers.TieneRasgo(PersonajeTraitCatalog.TraitFlojo))
      {
        continue;
      }

      pers.SetCampFatigado(true);

      string mensajeFlojo = idiomaTrait switch
      {
        TRADU.IdiomaIngles => pers.sNombre + " grows sluggish as caravan fatigue reaches 4. Gains Fatigue.",
        TRADU.IdiomaPortugues => pers.sNombre + " fica abatido quando a fadiga da caravana chega a 4. Recebe Fadiga.",
        _ => pers.sNombre + " se viene abajo cuando la fatiga de la caravana llega a 4. Obtiene Fatiga."
      };
      EscribirLog("-" + mensajeFlojo);
    }
  }

  public int CuantosPersonajesTienenTraitActivo(int traitId)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int cant = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers != null && !pers.Camp_Muerto && pers.TieneRasgo(traitId))
      {
        cant++;
      }
    }

    return cant;
  }

  public int AplicarCostoMejoraCaravanaTraits(int costoBase)
  {
    int cantidadOrganizados = CuantosPersonajesTienenTraitActivo(PersonajeTraitCatalog.TraitOrganizado);
    float multiplicador = Mathf.Max(0f, 1f - (cantidadOrganizados * 0.05f));
    return Mathf.Max(1, Mathf.CeilToInt(costoBase * multiplicador));
  }

  public bool HayClimaLluviosoONevado()
  {
    return intTipoClima == 3 || intTipoClima == 4;
  }

  public bool HayAlientoNegroIntenso()
  {
    return GetTierAlientoNegro() >= 3f;
  }

  public void AplicarTraitsMoraleAmbientales()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    bool climaHostil = HayClimaLluviosoONevado();
    bool alientoIntenso = HayAlientoNegroIntenso();

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (climaHostil && pers.TieneRasgo(PersonajeTraitCatalog.TraitOdiaLaLluvia))
      {
        pers.AplicarMoralBajaHoras(24f);
      }

      if (alientoIntenso && pers.TieneRasgo(PersonajeTraitCatalog.TraitAlmaDebil))
      {
        pers.AplicarMoralBajaHoras(24f);
      }
    }
  }

  public int ObtenerCapacidadMaximaPersonajes()
  {
    return 4 + Mathf.Max(0, mejoraCaravanaTiendas - 1) + ObtenerCantidadHeroesDebugInicialesExtra();
  }

  private int ObtenerCantidadHeroesDebugInicialesExtra()
  {
    int cantidad = 0;

    if (debugIniciarConCaballeroCompleto) cantidad++;
    if (debugIniciarConExploradorCompleto) cantidad++;
    if (debugIniciarConPurificadoraCompleta) cantidad++;
    if (debugIniciarConAcechadorCompleto) cantidad++;
    if (debugIniciarConCanalizadorCompleto) cantidad++;
    if (debugIniciarConDuelistaCompleta) cantidad++;

    return cantidad;
  }

  public bool TieneCapacidadDisponibleParaHeroe()
  {
    return CuantosPersonajesActivos() < ObtenerCapacidadMaximaPersonajes();
  }

  public bool HayMisionSalvamentoActivaEnMapa()
  {
    if (scMapaManager == null || scMapaManager.scContenedordeNodos == null)
    {
      return false;
    }

    scMapaManager.scContenedordeNodos.RecolectarNodos();
    foreach (Nodo nodo in scMapaManager.scContenedordeNodos.listTodosNodos)
    {
      if (nodo != null && nodo.gameObject.activeInHierarchy && nodo.tipoNodo == 16)
      {
        return true;
      }
    }

    return false;
  }

  public bool IntentarCrearMisionSalvamentoEnMapa()
  {
    if (HayMisionSalvamentoActivaEnMapa())
    {
      return false;
    }

    Nodo nodoFuturo = ObtenerNodoFuturoAleatorio(2, EsNodoValidoParaMisionSalvamento);
    if (nodoFuturo == null)
    {
      nodoFuturo = ObtenerNodoFuturoAleatorio(1, EsNodoValidoParaMisionSalvamento);
    }

    if (nodoFuturo == null)
    {
      return false;
    }

    nodoFuturo.nodoIncendiado = false;
    nodoFuturo.DesactivarRitual();
    if (nodoFuturo.transform.childCount > 14)
    {
      nodoFuturo.transform.GetChild(14).gameObject.SetActive(false);
    }

    if (nodoFuturo.transform.childCount > 15)
    {
      nodoFuturo.transform.GetChild(15).gameObject.SetActive(false);
    }

    nodoFuturo.DesactivarGraficosNodo();
    nodoFuturo.tipoNodo = 16;
    nodoFuturo.revelado = true;
    nodoFuturo.MarcarDescubiertoPorMecanicaEspecial();
    nodoFuturo.ActivarNodoVisual(16, false, true);
    scMapaManager.RefrescarVisibilidadExploracion();
    MarcarNodoCampaniaTemporal(nodoFuturo, TipoHighlightNodoCampania.MisionSalvamento);
    return true;
  }

  void EfectosdeSequitos()
  {


    if (scMenuSequito.TieneSequito(4)) //Artistas
    {
      int rand = UnityEngine.Random.Range(1, 4);
      if (rand == 1)
      {
        int rand2 = UnityEngine.Random.Range(1, 6);

        CambiarSuministrosActuales(-rand2);
        EscribirLog(TRADU.i.Traducir("-El Séquito de Artistas ha tenido un festán y despilfarrado suministros: ") + rand2);
      }
    }






  }
  private float ObtenerProgresoViajeActual()
  {
    Nodo origen = scMapaManager != null ? scMapaManager.nodoActual : null;
    CaminoConexion conexion = origen != null && nodoDestinoActual != null
      ? origen.ObtenerConexionHacia(nodoDestinoActual)
      : null;
    LineRenderer linea = conexion != null && conexion.linea != null
      ? conexion.linea.GetComponent<LineRenderer>()
      : null;
    if (linea == null || linea.positionCount < 2 || scMapaManager == null || scMapaManager.goCaravana == null)
    {
      return 0f;
    }

    Vector3 posicion = scMapaManager.goCaravana.transform.position;
    posicion.y = 0f;
    float longitudTotal = 0f;
    float mejorDistancia = 0f;
    float mejorError = float.PositiveInfinity;
    Vector3 anterior = linea.useWorldSpace ? linea.GetPosition(0) : linea.transform.TransformPoint(linea.GetPosition(0));
    anterior.y = 0f;

    for (int i = 1; i < linea.positionCount; i++)
    {
      Vector3 actual = linea.useWorldSpace ? linea.GetPosition(i) : linea.transform.TransformPoint(linea.GetPosition(i));
      actual.y = 0f;
      Vector3 segmento = actual - anterior;
      float longitudSegmento = segmento.magnitude;
      if (longitudSegmento > 0.0001f)
      {
        float t = Mathf.Clamp01(Vector3.Dot(posicion - anterior, segmento) / (longitudSegmento * longitudSegmento));
        float error = (posicion - (anterior + segmento * t)).sqrMagnitude;
        if (error < mejorError)
        {
          mejorError = error;
          mejorDistancia = longitudTotal + longitudSegmento * t;
        }
      }
      longitudTotal += longitudSegmento;
      anterior = actual;
    }

    return longitudTotal > 0.0001f ? Mathf.Clamp01(mejorDistancia / longitudTotal) : 0f;
  }

  public float CalcularLongitudRealCamino(CaminoConexion conexion)
  {
    LineRenderer linea = conexion != null && conexion.linea != null
      ? conexion.linea.GetComponent<LineRenderer>()
      : null;
    if (linea == null || linea.positionCount < 2)
    {
      return scMapaManager != null ? scMapaManager.ObtenerPasoMapa() : 1f;
    }

    float longitud = 0f;
    Vector3 anterior = linea.useWorldSpace ? linea.GetPosition(0) : linea.transform.TransformPoint(linea.GetPosition(0));
    for (int i = 1; i < linea.positionCount; i++)
    {
      Vector3 actual = linea.useWorldSpace ? linea.GetPosition(i) : linea.transform.TransformPoint(linea.GetPosition(i));
      longitud += Vector3.Distance(anterior, actual);
      anterior = actual;
    }
    return Mathf.Max(0.001f, longitud);
  }

  public float ObtenerVelocidadViajeUnidadesPorHora(CaminoConexion conexion, float hora, bool usarEstadoPendiente)
  {
    float pasoMapa = scMapaManager != null ? Mathf.Max(0.001f, scMapaManager.ObtenerPasoMapa()) : 1f;
    float velocidad = pasoMapa / HorasPorPasoMapa;
    float multiplicadorFatiga = Mathf.Max(0.60f, 1f - GetFatigaActual() * (0.07f / 1.35f));
    velocidad *= multiplicadorFatiga;
    if (conexion != null && conexion.tipo == TipoCaminoCampania.Dificil)
    {
      velocidad *= 0.90f;
    }
    if (conexion != null && conexion.EsAtajoSubterraneo)
    {
      velocidad /= MultiplicadorDuracionAtajoSubterraneo;
    }

    float multiplicadorEstado = usarEstadoPendiente
      ? (estadosCaravana != null ? estadosCaravana.ObtenerMultiplicadorVelocidadViajePendiente() : 1f)
      : Mathf.Max(0.01f, multiplicadorVelocidadVisualViajeActual);
    if (usarEstadoPendiente && SeLlevaDemasiadaCarga())
    {
      multiplicadorEstado *= MultiplicadorVelocidadSobrecarga;
    }
    velocidad *= multiplicadorEstado;

    if (intTipoClima == 4)
    {
      velocidad *= 0.85f;
    }
    if (EsHoraNocturna(hora) && !antorchasEncendidas)
    {
      velocidad *= 0.80f;
    }
    return Mathf.Max(0.001f, velocidad);
  }

  public float CalcularDuracionViajeHoras(CaminoConexion conexion)
  {
    float restante = CalcularLongitudRealCamino(conexion);
    float hora = ObtenerHoraActual();
    float duracion = 0f;
    const float pasoIntegracion = 1f / 60f;
    while (restante > 0.0001f && duracion < 240f)
    {
      float velocidad = ObtenerVelocidadViajeUnidadesPorHora(conexion, hora + duracion, true);
      float delta = Mathf.Min(pasoIntegracion, restante / velocidad);
      restante -= velocidad * delta;
      duracion += delta;
    }
    return Mathf.Max(0.01f, duracion);
  }

  public string ObtenerTextoDuracionViaje(CaminoConexion conexion)
  {
    float duracion = CalcularDuracionViajeHoras(conexion);
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    bool cruzaAmanecer = horasTotales + duracion >= ObtenerSiguienteHoraAbsoluta(horasTotales, HoraFinNoche) - 0.0001d;
    string marcaEstimacion = cruzaAmanecer
      ? idioma switch
      {
        TRADU.IdiomaIngles => " (estimated)",
        TRADU.IdiomaPortugues => " (estimada)",
        _ => " (estimada)"
      }
      : string.Empty;
    string texto = idioma switch
    {
      TRADU.IdiomaIngles => "Travel Time: " + FormatearDuracionHoras(duracion, true) + marcaEstimacion,
      TRADU.IdiomaPortugues => "Duração: " + FormatearDuracionHoras(duracion, true) + marcaEstimacion,
      _ => "Duración: " + FormatearDuracionHoras(duracion, true) + marcaEstimacion
    };
    return "<size=95%><color=#C8C8C8>" + texto + "</color></size>";
  }

  private bool IntervaloContieneNoche(float horaInicial, float duracion)
  {
    if (duracion <= 0f)
    {
      return EsHoraNocturna(horaInicial);
    }
    if (duracion >= 15f)
    {
      return true;
    }
    const float paso = 0.25f;
    for (float t = 0f; t <= duracion; t += paso)
    {
      if (EsHoraNocturna(horaInicial + t))
      {
        return true;
      }
    }
    return EsHoraNocturna(horaInicial + duracion);
  }

  public bool AccionIncluyeNoche(float duracionHoras)
  {
    return IntervaloContieneNoche(ObtenerHoraActual(), Mathf.Max(0f, duracionHoras));
  }

  public void AvanzarTiempoDuranteViaje(float horas)
  {
    if (EsNocheActual() || EsHoraNocturna(ObtenerHoraActual() + horas))
    {
      viajeActualIncluyoNoche = true;
    }
    horasViajeActual += Mathf.Max(0f, horas);
    AvanzarTiempoCampania(horas, TipoAvanceTiempoCampania.Viaje);
  }

  public void RegistrarFinViaje(float horas)
  {
    float horasValidas = Mathf.Max(Mathf.Max(0f, horas), horasViajeActual);
    horasViajeActual = 0f;
    estadisticaHorasViajadas += horasValidas;
    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null || personaje.Camp_Muerto)
      {
        continue;
      }
      personaje.HorasViajadas += horasValidas;
    }

    progresoFatigaHoras += horasValidas;
    int fatigaGanada = Mathf.FloorToInt(progresoFatigaHoras / HorasPorPasoMapa);
    if (fatigaGanada > 0)
    {
      progresoFatigaHoras -= fatigaGanada * HorasPorPasoMapa;
      CambiarFatigaActual(fatigaGanada);
    }
    FinalizarAccionTemporal();
  }

  public static bool ActividadTieneResultadoCada24Horas(int actividadId)
  {
    return actividadId == 2
      || actividadId == 4
      || actividadId == 7
      || actividadId == 10
      || actividadId == 11
      || actividadId == 15
      || actividadId == 18
      || actividadId == 20;
  }

  private void ResolverCicloActividad(Personaje pers)
  {
    if (pers == null || pers.Camp_Muerto || !pers.PuedeRealizarActividades()
      || !EsActividadPermitidaPorClimaCampania(pers.ActividadSeleccionada))
    {
      return;
    }

    if (pers.ActividadSeleccionada == 2)
    {
      int exp = scMenuSequito.TieneSequito(6) ? 55 : 45;
      pers.RecibirExperiencia(exp);
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" gana ") + exp + TRADU.i.Traducir(" Experiencia por su Actividad de <b>Entrenamiento</b>."));
    }
    else if (pers.ActividadSeleccionada == 4)
    {
      foreach (Personaje pers2 in scMenuPersonajes.listaPersonajes)
      {
        if (pers2 != null && pers2.fNivelActual < pers.fNivelActual)
        {
          pers2.RecibirExperiencia(10);
        }
      }
      CambiarEsperanzaActual(4);
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" brinda 10 Experiencia a sus compañeros de menor nivel por su Actividad de <b>Relatos de Batalla</b>."));
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" comparte sus historias de batalla con los civiles. +4 Esperanza"));
    }
    else if (pers.ActividadSeleccionada == 7)
    {
      int cantidad = UnityEngine.Random.Range(10, 16);
      CambiarSuministrosActuales(cantidad);
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" consigue ") + cantidad + TRADU.i.Traducir(" suministros por su Actividad de <b>Caza</b>."));
    }
    else if (pers.ActividadSeleccionada == 10)
    {
      CambiarValorAlientoNegroHoras(-4f);
      string mensajeRitual = TRADU.i.nIdioma switch
      {
        TRADU.IdiomaIngles => ", after a full day of rites and prayers, weakens the march of corruption. The Black Breath recedes by 4 h.",
        TRADU.IdiomaPortugues => ", após um dia inteiro de ritos e orações, enfraquece o avanço da corrupção. O Sopro Negro recua 4 h.",
        _ => ", tras una jornada de ritos y oraciones, debilita la marcha de la corrupción. El Aliento Negro retrocede 4 h."
      };
      EscribirLog("-" + pers.sNombre + mensajeRitual);
    }
    else if (pers.ActividadSeleccionada == 11)
    {
      int cantidad = UnityEngine.Random.Range(5, 11);
      CambiarEsperanzaActual(cantidad);
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" realiza su actividad <b>Ayudar a los Desamparados</b> y la esperanza aumenta en ") + cantidad + ".");
    }
    else if (pers.ActividadSeleccionada == 15)
    {
      int cantidad = UnityEngine.Random.Range(40, 81);
      CambiarEsperanzaActual(-5);
      CambiarOroActual(cantidad);
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" obtiene ") + cantidad + TRADU.i.Traducir(" de Oro de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -5 Esperanza"));
    }
    else if (pers.ActividadSeleccionada == 18)
    {
      GameObject consumible = Instantiate(scContprefab.SimboloProtArcano.gameObject);
      scMenuPersonajes.scEquipo.listInventario.Add(consumible);
      RuntimeAnalytics.TrackItemAcquired(consumible.GetComponent<Item>(), "character_activity");
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" ha creado un Símbolo de Protección Arcano."));
    }
    else if (pers.ActividadSeleccionada == 20)
    {
      const int dificultad = 16;
      List<string> beneficiados = new List<string>();
      foreach (Personaje pers2 in scMenuPersonajes.listaPersonajes)
      {
        if (pers2 == null || pers2 == pers || pers2.Camp_Muerto)
        {
          continue;
        }
        int resultado = UnityEngine.Random.Range(1, 21) + ObtenerTSMentalTotalCampania(pers2);
        if (resultado >= dificultad)
        {
          pers2.AplicarMoralAltaHoras(48f);
          beneficiados.Add(pers2.sNombre);
        }
      }
      string resumenBeneficiados = beneficiados.Count > 0 ? string.Join(", ", beneficiados) : TRADU.i.Traducir("nadie");
      EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" socializa con la caravana. Beneficiados: ") + resumenBeneficiados + ".");
    }
  }

  public int ExploracionSumadaPorActividades()
  {
    int valor = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto || !pers.PuedeRealizarActividades())
      {
        continue;
      }

      if (pers.ActividadSeleccionada == 3) //Guardia
      {
        valor += 3;
      }
      //Agregar las actividades que sumen exploracion aca.

    }
    return valor;
  }





  #region Aliento Negro
  [SerializeField] private Slider sliderAlientoNegro;
  [SerializeField] private GameObject tooltipAlientoNegro;
  [SerializeField] private TextMeshProUGUI txtTooltipAlientoNegro;
  public void ActivarTooltipAlientoNegro(int n)
  {

    if (n == 1)
    {

      tooltipAlientoNegro.SetActive(true);
      int tierAliento = (int)GetTierAlientoNegro();

      txtTooltipAlientoNegro.text = TRADU.i.Traducir("<color=#8708a4><b>                  El Aliento Negro</b></color>\n\n\n");
      txtTooltipAlientoNegro.text += TRADU.i.Traducir("<color=#ebdeef>Al morir el Liche, liberó un último estertor de muerte y putrefacción que se expande por cientos de kilómetros alrededor.</color>");
      txtTooltipAlientoNegro.text += TRADU.i.Traducir("\n\nLlamado el Aliento Negro, esta ola de peste y podredumbre lentamente está envolviendo a los seres vivos que no logran escapar, provocándoles la muerte, o peor. </color>\n\n\n\n");
      if (tierAliento == 1)
      {
        txtTooltipAlientoNegro.text += "<color=#bae895><b>" + ObtenerTextoDistanciaAliento(Mathf.RoundToInt(GetDistanciaAlientoACaravana()).ToString()) + "</b> - " + TRADU.i.Traducir("La Caravana viaja con tranquilidad.</color>");
      }
      if (tierAliento == 2)
      {
        txtTooltipAlientoNegro.text += "<color=#c8a6e8><b>" + ObtenerTextoDistanciaAliento(Mathf.RoundToInt(GetDistanciaAlientoACaravana()).ToString()) + "</b> - " + TRADU.i.Traducir("La Caravana comienza a preocuparse y la podredumbre se siente en el aire. Los Corrompidos acechan en las sombras.</color>");
      }
      if (tierAliento == 3)
      {
        txtTooltipAlientoNegro.text += "<color=#aa66ea><b>" + ObtenerTextoDistanciaAliento(Mathf.RoundToInt(GetDistanciaAlientoACaravana()).ToString()) + "</b> - " + TRADU.i.Traducir("La Caravana ya es directamente afectada por el hedor. Los Corrompidos se dejan ver.</color>");
      }
      if (tierAliento == 4)
      {
        txtTooltipAlientoNegro.text += "<color=#7a1dd1><b>" + ObtenerTextoDistanciaAliento(Mathf.RoundToInt(GetDistanciaAlientoACaravana()).ToString()) + "</b> - " + TRADU.i.Traducir("La peste comienza a tomar vidas civiles. Los Corrompidos son implacables.</color>");
      }

    }
    else
    {
      tooltipAlientoNegro.SetActive(false);
    }

  }
  private float EstadoAlientoNegro; //Horas acumuladas del Aliento Negro.
  private float TierAlientoNegro; //
  public float GetValorAlientoNegro()
  {
    return EstadoAlientoNegro;
  }
  public float GetTierAlientoNegro()
  {
    ActualizarTierAlientoNegro();


    return TierAlientoNegro;
  }
  public void CambiarValorAlientoNegroHoras(float horas)
  {
    if (Mathf.Approximately(horas, 0f) || (scAtributosZona != null && scAtributosZona.ID == 3))
    {
      return;
    }

    EstadoAlientoNegro = Mathf.Max(0f, EstadoAlientoNegro + horas);
    ActualizarTierAlientoNegro();
    RefrescarTextoDistanciaAliento();
    if (Mathf.Abs(horas) >= HorasPorPasoMapa)
    {
      AvanzarAlientoNegro(Mathf.RoundToInt(horas / HorasPorPasoMapa));
    }

    if (GetDistanciaAlientoACaravana() < 0f && scMenuSequito != null && scMenuSequito.TieneSequito(10))
    {
      scMenuSequito.RemoverSequito(10);
      EscribirLog(TRADU.i.Traducir("-El Séquito de Clérigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crítico. -20 Esperanza"));
    }
  }

  public void ResetearAlientoNegro()
  { 
    EstadoAlientoNegro = 15f;
    ActualizarTierAlientoNegro();
  }
  public void AvanzarAlientoNegro(int n)
  {
    if (scAlientoNegroVFX != null)
    {
      scAlientoNegroVFX.AvanzarAlientoNegro(n);
    }

  }

  public int posicionCaravana; //1-12 la posicion de la caravana en los nodos

  public void AgregarEstadoCaravana(TipoEstadoCaravana tipo, int stacks = 1)
  {
    if (estadosCaravana == null)
    {
      estadosCaravana = new EstadosCaravana();
    }

    estadosCaravana.AgregarEstado(tipo, stacks);
  }

  public TipoEstadoCaravana AgregarEstadoCaravanaPositivoAleatorio(int stacks = 1)
  {
    TipoEstadoCaravana estado = EstadosCaravana.ObtenerEstadoPositivoAleatorio();
    AgregarEstadoCaravana(estado, stacks);
    return estado;
  }

  private string ObtenerLogAlmenarasSerria(int tierAlmenaras, TipoEstadoCaravana estado)
  {
    int esperanza = tierAlmenaras * 5;
    string nombreEstadoEs = EstadosCaravana.ObtenerNombreVisible(estado);
    string stackEs = tierAlmenaras == 1 ? "stack" : "stacks";
    string stackPt = tierAlmenaras == 1 ? "acúmulo" : "acúmulos";

    if (TRADU.i == null || TRADU.i.nIdioma == TRADU.IdiomaEspanol)
    {
      return "-Las almenaras de Serria se divisan a lo lejos sobre las montañas, brillando con fuerza y marcando el destino de la caravana: "
        + esperanza + " Esperanza, +" + tierAlmenaras + " " + stackEs + " de " + nombreEstadoEs + ".";
    }

    if (TRADU.i.nIdioma == TRADU.IdiomaIngles)
    {
      string nombreEstadoEn = estado switch
      {
        TipoEstadoCaravana.Inspiracion => "Inspiration",
        TipoEstadoCaravana.Presteza => "Swiftness",
        TipoEstadoCaravana.Compromiso => "Commitment",
        _ => "Vigilant"
      };

      return "-The beacons of Serria can be seen in the distance over the mountains, shining brightly and marking the caravan's destination: "
        + esperanza + " Hope, +" + tierAlmenaras + " " + stackEs + " of " + nombreEstadoEn + ".";
    }

    string nombreEstadoPt = estado switch
    {
      TipoEstadoCaravana.Inspiracion => "Inspiração",
      TipoEstadoCaravana.Presteza => "Presteza",
      TipoEstadoCaravana.Compromiso => "Compromisso",
      _ => "Vigilante"
    };

    return "-As almenaras de Serria podem ser vistas ao longe sobre as montanhas, brilhando com força e marcando o destino da caravana: "
      + esperanza + " Esperança, +" + tierAlmenaras + " " + stackPt + " de " + nombreEstadoPt + ".";
  }

  public int ObtenerModificadorValentiaEstadosCaravanaCombateActual()
  {
    return estadosCaravana != null ? estadosCaravana.ObtenerModificadorValentiaCombateActual() : 0;
  }

  public float AplicarMultiplicadorExperienciaEstadosCaravana(float experienciaBase)
  {
    return estadosCaravana != null
      ? estadosCaravana.AplicarMultiplicadorExperienciaCombateActual(experienciaBase)
      : experienciaBase;
  }

  void ActualizarTierAlientoNegro()
  {
    float cercaniaAliento = EstadoAlientoNegro - posicionCaravana * HorasPorPasoMapa;

    if (cercaniaAliento < 20f)
    {
      TierAlientoNegro = 1;

      //  handleSliderCalavera.color = new Color(0.15f, 0.15f, 0.15f);

    }
    else if (cercaniaAliento < 35f)
    {
      TierAlientoNegro = 2;

      //  handleSliderCalavera.color = new Color(0.15f, 0.12f, 0.12f);
    }
    else if (cercaniaAliento < 50f)
    {
      TierAlientoNegro = 3;

      //  handleSliderCalavera.color = new Color(0.18f, 0.3f, 0.3f);
    }
    else
    {
      TierAlientoNegro = 4;

      //  handleSliderCalavera.color = new Color(0.75f, 0.2f, 0.6f);

    }

    RefrescarVfxClimaCalor();

  }
  #endregion






  #region Fatiga
  [SerializeField] private TextMeshProUGUI valueFatiga;

  private int FatigaActual;
  public int GetFatigaActual()
  {
    return FatigaActual;
  }
  public void CambiarFatigaActual(int fatiga)
  {
    int fatigaAnterior = FatigaActual;
    FatigaActual += fatiga;
    int fatigaNueva = FatigaActual;
    switch (FatigaActual)
    {
      case < 0: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = TRADU.i.Traducir("Enérgicos(0)"); valueFatiga.color = new Color(0.1f, 0.95f, 0.2f); break;
      case 0: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = TRADU.i.Traducir("Descansados(1)"); valueFatiga.color = new Color(0.1f, 0.9f, 0.3f); break;
      case 1: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = TRADU.i.Traducir("Frescos(2)"); valueFatiga.color = new Color(0.1f, 0.7f, 0.3f); break;
      case 2: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = TRADU.i.Traducir("En Marcha(3)"); valueFatiga.color = new Color(0.25f, 0.6f, 0.3f); break;
      case 3: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = TRADU.i.Traducir("Agitados(4)"); valueFatiga.color = new Color(0.55f, 0.5f, 0.2f); break;
      case 4: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = TRADU.i.Traducir("Cansados(5)"); valueFatiga.color = new Color(0.75f, 0.3f, 0.25f); break;
      case > 4: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = TRADU.i.Traducir("Exhaustos(6)"); valueFatiga.color = new Color(0.8f, 0.15f, 0.45f); break;
    }

    if (FatigaActual > 2) { alertaFatiga.SetActive(true); }
    else { alertaFatiga.SetActive(false); }
  }
  void EventoFatiga(int fatigaAnterior, int fatigaAhora)
  {
    if (fatigaAnterior < fatigaAhora) //Solamente si se gano fatiga
    {
      AplicarTraitFlojoSiCorresponde(fatigaAnterior, fatigaAhora);

      switch (fatigaAhora)
      {
        case 4: CambiarEsperanzaActual(-10); int rand = UnityEngine.Random.Range(-2, 1); CambiarBueyesActuales(rand); if (rand < 0) { EscribirLog(TRADU.i.Traducir("-La fatiga ha provocado la muerte de algunos Bueyes.") + " " + rand + TRADU.i.Traducir(" Bueyes")); } break;    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        case 5: CambiarEsperanzaActual(-15); int rand2 = UnityEngine.Random.Range(-2, 1); CambiarBueyesActuales(rand2); if (rand2 < 0) { EscribirLog(TRADU.i.Traducir("-La fatiga ha provocado la muerte de algunos Bueyes.") + " " + rand2 + TRADU.i.Traducir(" Bueyes")); } break;
        case > 5: CambiarEsperanzaActual(-20); int rand3 = UnityEngine.Random.Range(-4, 1); CambiarBueyesActuales(rand3); int rand4 = UnityEngine.Random.Range(-10, 1); CambiarCivilesActuales(rand4); if (rand3 < 0 || rand4 < 0) { EscribirLog(TRADU.i.Traducir("-La fatiga extrema ha provocado la muerte de algunos Bueyes y Civiles.") + " " + rand3 + TRADU.i.Traducir(" Bueyes ") + rand4 + TRADU.i.Traducir(" Civiles")); } break;
      }
      if (scMenuSequito.TieneSequito(9) && fatigaAhora >= 4) //Si hay un Séquito de Nobles y la fatiga es 4 o más
      {
        CambiarEsperanzaActual(-2);
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El Séquito de Nobles se queja por la falta de descanso. -2 Esperanza"));
      }

      if (fatigaAhora == 5)
      {
        foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
        {
          pers.SetCampFatigado(true);
        }
        EscribirLog(TRADU.i.Traducir("-Tus personajes están fatigados. Afectará su rendimiento en batalla."));

      }
    }
  }
  #endregion
  #region Esperanza
  [SerializeField] private TextMeshProUGUI valueEsperanza;
  private int EsperanzaActual;
  public int GetEsperanzaActual()
  {
    return EsperanzaActual;
  }
  public void CambiarEsperanzaActual(int esperanza)
  {
    EjecutarTareaSegura(CambiarEsperanzaActualAsync(esperanza), nameof(CambiarEsperanzaActual));
  }

  private async Task CambiarEsperanzaActualAsync(int esperanza)
  {
    int mitigacionConsuelo = 0;
    if (esperanza < 0)
    {
      int cantidadConsuelos = CuantosPersonajesHacenTalActividad(21);
      mitigacionConsuelo = Mathf.Min(-esperanza, Mathf.Min(2, cantidadConsuelos * 2));
      esperanza += mitigacionConsuelo;
    }

    int esperanzaAnterior = EsperanzaActual;
    EsperanzaActual = Mathf.Clamp(EsperanzaActual + esperanza, 0, 100);
    int esperanzaAplicada = EsperanzaActual - esperanzaAnterior;

    if (EsperanzaActual >= 0 && EsperanzaActual <= 10)
    {
      valueEsperanza.text = "" + EsperanzaActual;
      valueEsperanza.color = new Color(0.8f, 0.1f, 0.4f);
    }
    else if (EsperanzaActual >= 11 && EsperanzaActual <= 20)
    {
      valueEsperanza.text = "" + EsperanzaActual;
      valueEsperanza.color = new Color(0.6f, 0.2f, 0.4f);
    }
    else if (EsperanzaActual >= 21 && EsperanzaActual <= 40)
    {
      valueEsperanza.text = "" + EsperanzaActual;
      valueEsperanza.color = new Color(0.25f, 0.5f, 0.3f);
    }
    else if (EsperanzaActual >= 41 && EsperanzaActual <= 60)
    {
      valueEsperanza.text = "" + EsperanzaActual;
      valueEsperanza.color = new Color(0.45f, 0.55f, 0.3f);
    }
    else if (EsperanzaActual >= 61 && EsperanzaActual <= 80)
    {
      valueEsperanza.text = "" + EsperanzaActual;
      valueEsperanza.color = new Color(0.25f, 0.75f, 0.3f);
    }
    else if (EsperanzaActual >= 81 && EsperanzaActual <= 90)
    {
      valueEsperanza.text = "" + EsperanzaActual;
      valueEsperanza.color = new Color(0.15f, 0.75f, 0.45f);
    }
    else if (EsperanzaActual >= 91 && EsperanzaActual <= 100)
    {
      valueEsperanza.text = "" + EsperanzaActual;
      valueEsperanza.color = new Color(0.05f, 0.85f, 0.55f);
    }

    if (EsperanzaActual < 20) { alertaEsperanza.SetActive(true); }
    else { alertaEsperanza.SetActive(false); }

    GameObject textoOrigen = valueEsperanza != null ? valueEsperanza.gameObject : null;
    await GenerarTextoRecursos(esperanzaAplicada, textoOrigen, false);
    EvaluarDerrotaPorEstadoCaravana();

    if (mitigacionConsuelo > 0)
    {
      EscribirLog(TRADU.i.Traducir("-Consuelo reduce la pérdida de Esperanza en ") + mitigacionConsuelo + ".");
    }
  }


  #endregion
  #region Civiles
  [SerializeField] private TextMeshProUGUI valueCiviles;
  private int civilesActuales;
  public float GetCivilesActual()
  {
    return civilesActuales;
  }

  public void RegistrarBatallaLibrada()
  {
    estadisticaBatallasLibradas++;
  }

  public int ObtenerEstadisticaDiasViajados()
  {
    return Mathf.FloorToInt(Mathf.Max(0f, estadisticaHorasViajadas) / 24f);
  }

  public float ObtenerEstadisticaHorasViajadas()
  {
    return estadisticaHorasViajadas;
  }

  public int ObtenerEstadisticaBatallasLibradas()
  {
    return estadisticaBatallasLibradas;
  }

  public int ObtenerEstadisticaCivilesPerdidos()
  {
    return estadisticaCivilesPerdidos;
  }

  public int ObtenerEstadisticaAsentamientosVisitados()
  {
    return estadisticaAsentamientosVisitados;
  }

  public int ObtenerEstadisticaPersonajesMuertos()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int total = 0;
    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje != null && personaje.Camp_Muerto)
      {
        total++;
      }
    }

    return total;
  }

  public int ObtenerEstadisticaEnemigosAsesinados()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    int total = 0;
    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje != null)
      {
        total += Mathf.Max(0, personaje.EnemigosEliminados);
      }
    }

    return total;
  }

  public void CambiarCivilesActuales(int civiles)
  {
    EjecutarTareaSegura(CambiarCivilesActualesAsync(civiles), nameof(CambiarCivilesActuales));
  }

  private async Task CambiarCivilesActualesAsync(int civiles)
  {
    int civilesAnteriores = civilesActuales;
    civilesActuales = Mathf.Max(0, civilesActuales + civiles);
    int civilesAplicados = civilesActuales - civilesAnteriores;
    if (civilesAplicados < 0)
    {
      estadisticaCivilesPerdidos += -civilesAplicados;
    }
    valueCiviles.text = "" + civilesActuales;

    GetMiliciasActual();


    scMapaManager.goCaravanafollower1.SetActive(civilesActuales > 40);
    scMapaManager.goCaravanafollower2.SetActive(civilesActuales > 60);
    scMapaManager.goCaravanafollower3.SetActive(civilesActuales > 95);
    scMapaManager.goCaravanafollower4.SetActive(civilesActuales > 120);
    scMapaManager.goCaravanafollower5.SetActive(civilesActuales > 140);
    scMapaManager.goCaravanafollower6.SetActive(civilesActuales > 180);

    GameObject textoOrigen = valueCiviles != null ? valueCiviles.gameObject : null;
    await GenerarTextoRecursos(civilesAplicados, textoOrigen, true);
    EvaluarDerrotaPorEstadoCaravana();
  }

  //Milicias
  [SerializeField] private TextMeshProUGUI valueMilicias;

  public int miliciasMejoras;
  public float GetMiliciasActual()
  {
    float milicias = (GetCivilesActual() / 100) * (25 + miliciasMejoras); //25% civiles son aptos para luchar
    valueMilicias.text = "" + (int)milicias;
    return milicias;
  }
  #endregion
  #region Bueyes, Capacidad de Carga y Carga llevada
  [SerializeField] private TextMeshProUGUI valueBueyes;
  [SerializeField] private TextMeshProUGUI valueCargaMax;
  [SerializeField] private TextMeshProUGUI valueCargaLlevada;
  private int BueyesActuales;
  public int GetBueyesActual()
  {
    return BueyesActuales;
  }
  private int CargaMaxActual;
  public int GetCapacidadDeCargaActual()
  {
    int cargaPorBuey = 25 + Mathf.Max(0, mejoraCaravanaAlforjas - 1);
    CargaMaxActual = BueyesActuales * cargaPorBuey;
    CargaMaxActual += mejoraCaravanaAlmacen * 5;
    CargaMaxActual += CuantosPersonajesHacenTalActividad(17) * 20; //Canalizador: Telekinesis
    CargaMaxActual += CuantosPersonajesTienenTraitActivo(PersonajeTraitCatalog.TraitHombrosFirmes) * 15;
    if (scMenuSequito.TieneSequito(11)) //Esclavos
    {
      CargaMaxActual += 50; //Bonus de carga por los esclavos
    }



    return CargaMaxActual;


  }

  public void CambiarBueyesActuales(int bueyes)
  {
    EjecutarTareaSegura(CambiarBueyesActualesAsync(bueyes), nameof(CambiarBueyesActuales));
  }

  private async Task CambiarBueyesActualesAsync(int bueyes)
  {
    int bueyesAnteriores = BueyesActuales;
    BueyesActuales = Mathf.Max(0, BueyesActuales + bueyes);
    int bueyesAplicados = BueyesActuales - bueyesAnteriores;

    CargaMaxActual = GetCapacidadDeCargaActual();
    valueCargaMax.text = "/" + CargaMaxActual + ")";
    valueCargaLlevada.text = "(" + GetCargaLlevadaActual() + "";
    valueBueyes.text = "" + BueyesActuales;
    EvaluarTooltipSobrepesoMateriales();

    GameObject textoOrigen = valueBueyes != null ? valueBueyes.gameObject : null;
    await GenerarTextoRecursos(bueyesAplicados, textoOrigen, false);
  }

  public int GetCargaLlevadaActual()
  {
    int cargaActual = (GetMaterialesActuales() * 3) + GetSuministrosActuales();

    if (cargaActual > GetCapacidadDeCargaActual()) { valueCargaLlevada.color = new Color(0.8f, 0.2f, 0.2f); }
    else { valueCargaLlevada.color = new Color(0.35f, 0.7f, 0.3f); }
    valueCargaMax.text = "/" + CargaMaxActual + ")";
    valueCargaLlevada.text = "(" + cargaActual + "";

    if (cargaActual > CargaMaxActual) { alertaCarga.SetActive(true); }
    else { alertaCarga.SetActive(false); }


    return cargaActual;
  }

  public bool SeLlevaDemasiadaCarga()
  {
    if (GetCargaLlevadaActual() > GetCapacidadDeCargaActual()) { return true; }
    return false;
  }

  private void ActualizarAdvertenciaSobrecarga()
  {
    bool sobrecargaAhora = SeLlevaDemasiadaCarga();
    if (estadoSobrecargaInicializado && sobrecargaAhora && !sobrecargaAnterior)
    {
      EscribirAdvertenciaLog(ObtenerTextoAdvertenciaSobrecarga());
    }

    sobrecargaAnterior = sobrecargaAhora;
    estadoSobrecargaInicializado = true;
  }

  private static string ObtenerTextoAdvertenciaSobrecarga()
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    string mensaje = idioma switch
    {
      TRADU.IdiomaIngles => "The caravan is overloaded.",
      TRADU.IdiomaPortugues => "A caravana está sobrecarregada.",
      _ => "La caravana está sobrecargada."
    };

    return "<color=#FF3333>" + mensaje + "</color>";
  }

  private void EvaluarTooltipSobrepesoMateriales()
  {
    if (GetMaterialesActuales() > 50 && GetCargaLlevadaActual() > GetCapacidadDeCargaActual())
    {
      TutorialTooltipManager.TryShow(TooltipPersonajePesoId);
    }
  }

  public void EvaluarTooltipPersonajeDescansando()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null
        || pers.Camp_Muerto
        || pers.ActividadSeleccionada != 1
        || pers.TieneRasgo(PersonajeTraitCatalog.TraitHolgazan))
      {
        continue;
      }

      if (pers.fVidaMaxima > 0f && pers.fVidaActual >= pers.fVidaMaxima)
      {
        TutorialTooltipManager.TryShow(TooltipPersonajeDescansandoId);
        return;
      }
    }
  }

  public void SacrificarBuey()
  {
    if (GetBueyesActual() > 0)
    {
      CambiarBueyesActuales(-1);
      CambiarSuministrosActuales(20);
      CambiarEsperanzaActual(-2);
      EscribirLog(TRADU.i.Traducir("-El sacrificio de Bueyes para obtener Suministros ha provocado preocupación. -2 Esperanza"));
    }
  }

  #endregion
  #region Suministros
  [SerializeField] private TextMeshProUGUI valueSuministros;
  [SerializeField] private TextMeshProUGUI valueCantdescansos;
  private int SuministrosActuales;
  public int GetSuministrosActuales()
  {
    return SuministrosActuales;
  }
  public void CambiarSuministrosActuales(int suministros)
  {
    EjecutarTareaSegura(CambiarSuministrosActualesAsync(suministros), nameof(CambiarSuministrosActuales));
  }

  private async Task CambiarSuministrosActualesAsync(int suministros)
  {
    int suministrosAnteriores = SuministrosActuales;
    SuministrosActuales = Mathf.Max(0, SuministrosActuales + suministros);
    int suministrosAplicados = SuministrosActuales - suministrosAnteriores;
    float consumo = GetCivilesActual() + GetBueyesActual() * 2;
    valueSuministros.text = "" + SuministrosActuales;

    // Calcula cuántos días alcanzan los suministros actuales
    int diasSuministros = consumo > 0f ? Mathf.FloorToInt(SuministrosActuales / consumo) : 0;


    switch (TRADU.i.nIdioma)
    {
      case 1: //Español
        {
          if (diasSuministros != 1)
          { valueCantdescansos.text = $"<i>{(int)diasSuministros} descansos</i>"; }
          else { valueCantdescansos.text = $"<i>{(int)diasSuministros} descanso</i>"; }
          break;
        }
      case 2: //Ingles
        {
          if (diasSuministros != 1)
          { valueCantdescansos.text = $"<i>{(int)diasSuministros} rests</i>"; }
          else { valueCantdescansos.text = $"<i>{(int)diasSuministros} rest</i>"; }
          break;
        }



    }


    GetCargaLlevadaActual();
    EvaluarTooltipSobrepesoMateriales();

    if (SuministrosActuales < GetCivilesActual()) { alertaSuministros.SetActive(true); }
    else { alertaSuministros.SetActive(false); }

    GameObject textoOrigen = valueSuministros != null ? valueSuministros.gameObject : null;
    await GenerarTextoRecursos(suministrosAplicados, textoOrigen, false);
  }

  public GameObject alertaSuministros;
  public GameObject alertaFatiga;
  public GameObject alertaEsperanza;
  public GameObject alertaCarga;

  public void AbandonarSuministros()
  {
    if (GetSuministrosActuales() > 4)
    {
      CambiarSuministrosActuales(-5);
      CambiarEsperanzaActual(-1);
    }
  }

  #endregion
  #region Materiales
  [SerializeField] private TextMeshProUGUI valueMateriales;
  private int MaterialesActuales;
  public int GetMaterialesActuales()
  {
    return MaterialesActuales;
  }
  public void CambiarMaterialesActuales(int Materiales)
  {
    EjecutarTareaSegura(CambiarMaterialesActualesAsync(Materiales), nameof(CambiarMaterialesActuales));
  }

  private async Task CambiarMaterialesActualesAsync(int Materiales)
  {
    int materialesAnteriores = MaterialesActuales;
    MaterialesActuales = Mathf.Max(0, MaterialesActuales + Materiales);
    int materialesAplicados = MaterialesActuales - materialesAnteriores;
    valueMateriales.text = "" + MaterialesActuales;
    GetCargaLlevadaActual();
    EvaluarTooltipSobrepesoMateriales();

    GameObject textoOrigen = valueMateriales != null ? valueMateriales.gameObject : null;
    await GenerarTextoRecursos(materialesAplicados, textoOrigen, false);
  }
  public void AbandonarMateriales()
  {
    if (GetMaterialesActuales() > 1)
    {
      CambiarMaterialesActuales(-2);
      // CambiarEsperanzaActual(-1);
    }
  }



  #endregion
  #region Oro
  [SerializeField] private TextMeshProUGUI valueOro;
  private int OroActuales;
  public int GetOroActuales()
  {
    return OroActuales;
  }
  public void CambiarOroActual(int Oro)
  {
    EjecutarTareaSegura(CambiarOroActualAsync(Oro), nameof(CambiarOroActual));
  }

  private async Task CambiarOroActualAsync(int Oro)
  {
    int oroAnterior = OroActuales;
    OroActuales = Mathf.Max(0, OroActuales + Oro);
    int oroAplicado = OroActuales - oroAnterior;
    valueOro.text = "" + OroActuales;
    ActualizarOroPuestoComercial();

    GameObject textoOrigen = valueOro != null ? valueOro.gameObject : null;
    await GenerarTextoRecursos(oroAplicado, textoOrigen, true);
  }
  #endregion


  Task GenerarTextoRecursos(int cantidad, GameObject textoOrigen, bool efectoRetraso)
  {
    if (cantidad == 0 || prefabTextoRecursos == null || textoOrigen == null)
    {
      return Task.CompletedTask;
    }

    if (!isActiveAndEnabled)
    {
      return Task.CompletedTask;
    }

    SolicitudTextoRecurso solicitud = new SolicitudTextoRecurso(cantidad, textoOrigen, efectoRetraso);
    if (bloqueoTextosRecursosCampania > 0)
    {
      colaTextosRecursosSuspendidos.Enqueue(solicitud);
      return Task.CompletedTask;
    }

    EncolarTextoRecurso(solicitud);

    return Task.CompletedTask;
  }

  private void EncolarTextoRecurso(SolicitudTextoRecurso solicitud)
  {
    colaTextosRecursos.Enqueue(solicitud);
    if (rutinaTextosRecursos == null)
    {
      rutinaTextosRecursos = StartCoroutine(ProcesarColaTextosRecursos());
    }
  }

  public void LiberarTextosRecursosSuspendidos()
  {
    if (bloqueoTextosRecursosCampania > 0 || colaTextosRecursosSuspendidos.Count == 0)
    {
      return;
    }

    while (colaTextosRecursosSuspendidos.Count > 0)
    {
      EncolarTextoRecurso(colaTextosRecursosSuspendidos.Dequeue());
    }
  }

  private IEnumerator ProcesarColaTextosRecursos()
  {
    yield return null;

    while (colaTextosRecursos.Count > 0)
    {
      SolicitudTextoRecurso solicitud = colaTextosRecursos.Dequeue();
      CrearTextoRecursos(solicitud.Cantidad, solicitud.TextoOrigen);

      if (colaTextosRecursos.Count > 0)
      {
        float intervalo = solicitud.EfectoRetraso
          ? Mathf.Max(0.12f, recursoTextoIntervaloSpawn)
          : recursoTextoIntervaloSpawn;
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, intervalo));
      }
    }

    rutinaTextosRecursos = null;
  }

  private void CrearTextoRecursos(int cantidad, GameObject textoOrigen)
  {
    if (cantidad == 0 || prefabTextoRecursos == null || textoOrigen == null)
    {
      return;
    }

    Transform origen = textoOrigen.transform;
    int stackIndex = Mathf.Max(
      ObtenerIndiceStackTextoRecurso(origen),
      ObtenerIndiceStackGlobalTextoRecurso(origen));

    GameObject goTextoFlotante = Instantiate(prefabTextoRecursos, textoOrigen.transform, false);

    Animator animator = goTextoFlotante.GetComponent<Animator>();
    float velocidadAnimador = Mathf.Max(0.01f, recursoTextoAnimatorSpeed);
    if (animator != null)
    {
      animator.speed = velocidadAnimador;
    }

    AutodestruirDelay autodestruirDelay = goTextoFlotante.GetComponent<AutodestruirDelay>();
    if (autodestruirDelay != null)
    {
      autodestruirDelay.SetDelay(recursoTextoDuracionExtra);
    }

    RectTransform rt = goTextoFlotante.GetComponent<RectTransform>();
    if (rt != null && stackIndex > 0)
    {
      float side = (stackIndex % 2 == 0) ? 1f : -1f;
      float lateralStep = Mathf.Ceil(stackIndex * 0.5f);
      rt.anchoredPosition += new Vector2(recursoTextoStackOffsetX * side * lateralStep, -recursoTextoStackOffsetY * stackIndex);
    }


    TextMeshProUGUI txtMesh = goTextoFlotante.GetComponent<TextMeshProUGUI>();
    if (txtMesh == null)
    {
      Destroy(goTextoFlotante);
      return;
    }

    RegistrarTextoRecursoReciente(origen);

    // Configura el texto y el color

    TMP_SpriteAsset spriteAssetRecursos = logDeCampania != null ? logDeCampania.SpriteAssetRecursos : null;
    if (spriteAssetRecursos != null)
    {
      TextoIconosCombate.NormalizarSpriteAsset(spriteAssetRecursos);
      txtMesh.spriteAsset = spriteAssetRecursos;
    }

    if (cantidad >= 1)
    {
      txtMesh.color = new Color(0.1f, 0.7f, 0.2f); ;
      txtMesh.text = FormatearTextoFlotanteRecurso("+" + cantidad, textoOrigen);

    }
    else if (cantidad < 0)
    {
      txtMesh.text = FormatearTextoFlotanteRecurso("" + cantidad, textoOrigen);
      txtMesh.color = new Color(0.7f, 0.1f, 0.2f); ;
    }

    if (EsTextoFlotanteBueyes(textoOrigen))
    {
      AplicarAnimacionTextoRecursoHaciaArriba(goTextoFlotante, velocidadAnimador);
    }
  }

  private void AplicarAnimacionTextoRecursoHaciaArriba(GameObject goTextoFlotante, float velocidadAnimador)
  {
    if (goTextoFlotante == null)
    {
      return;
    }

    Animator animator = goTextoFlotante.GetComponent<Animator>();
    if (animator != null)
    {
      animator.enabled = false;
    }

    AnimacionTextoRecursoManual animacionManual = goTextoFlotante.GetComponent<AnimacionTextoRecursoManual>();
    if (animacionManual == null)
    {
      animacionManual = goTextoFlotante.AddComponent<AnimacionTextoRecursoManual>();
    }

    animacionManual.Configurar(
      RecursoTextoDeltaAnimacionY,
      RecursoTextoDuracionMovimiento / velocidadAnimador,
      RecursoTextoDuracionFade / velocidadAnimador,
      RecursoTextoAlphaIntermedio);
  }

  private string FormatearTextoFlotanteRecurso(string textoBase, GameObject textoOrigen)
  {
    if (string.IsNullOrEmpty(textoBase))
    {
      return textoBase;
    }

    if (logDeCampania == null || logDeCampania.SpriteAssetRecursos == null)
    {
      return textoBase;
    }

    string spriteName = ObtenerSpriteTextoRecurso(textoOrigen);
    if (string.IsNullOrEmpty(spriteName))
    {
      return textoBase;
    }

    return textoBase + " " + TextoIconosCombate.NormalizarIconosInline("<sprite name=\"" + spriteName + "\">");
  }

  private string ObtenerSpriteTextoRecurso(GameObject textoOrigen)
  {
    if (textoOrigen == null)
    {
      return null;
    }

    if (valueEsperanza != null && textoOrigen == valueEsperanza.gameObject) { return "esperanza"; }
    if (valueCiviles != null && textoOrigen == valueCiviles.gameObject) { return "civiles"; }
    if (valueBueyes != null && textoOrigen == valueBueyes.gameObject) { return "bueyes"; }
    if (valueSuministros != null && textoOrigen == valueSuministros.gameObject) { return "suministros"; }
    if (valueMateriales != null && textoOrigen == valueMateriales.gameObject) { return "materiales"; }
    if (valueOro != null && textoOrigen == valueOro.gameObject) { return "oro"; }
    return null;
  }

  private bool EsTextoFlotanteBueyes(GameObject textoOrigen)
  {
    return valueBueyes != null && textoOrigen == valueBueyes.gameObject;
  }

  private int ObtenerIndiceStackGlobalTextoRecurso(Transform origen)
  {
    if (origen == null)
    {
      return 0;
    }

    float tiempoActual = Time.unscaledTime;
    float ventanaStack = Mathf.Max(0f, recursoTextoVentanaStackGlobal);
    textosRecursosRecientes.RemoveAll(registro => registro.Origen == null || tiempoActual - registro.Tiempo > ventanaStack);

    int maxStackVisual = Mathf.Max(1, recursoTextoMaxStackVisual);
    return Mathf.Clamp(textosRecursosRecientes.Count, 0, maxStackVisual - 1);
  }

  private void RegistrarTextoRecursoReciente(Transform origen)
  {
    if (origen == null)
    {
      return;
    }

    textosRecursosRecientes.Add(new RegistroTextoRecurso(origen, Time.unscaledTime));
  }

  private int ObtenerIndiceStackTextoRecurso(Transform origen)
  {
    if (origen == null || prefabTextoRecursos == null)
    {
      return 0;
    }

    int textosActivos = 0;
    string tagTexto = prefabTextoRecursos.tag;
    int maxStackVisual = Mathf.Max(1, recursoTextoMaxStackVisual);
    for (int i = 0; i < origen.childCount; i++)
    {
      Transform child = origen.GetChild(i);
      if (child == null)
      {
        continue;
      }

      GameObject childObject = child.gameObject;
      if (childObject.activeInHierarchy && childObject.CompareTag(tagTexto))
      {
        textosActivos++;
      }
    }

    return Mathf.Clamp(textosActivos, 0, maxStackVisual - 1);
  }

  private readonly struct SolicitudTextoRecurso
  {
    public readonly int Cantidad;
    public readonly GameObject TextoOrigen;
    public readonly bool EfectoRetraso;

    public SolicitudTextoRecurso(int cantidad, GameObject textoOrigen, bool efectoRetraso)
    {
      Cantidad = cantidad;
      TextoOrigen = textoOrigen;
      EfectoRetraso = efectoRetraso;
    }
  }

  private readonly struct RegistroTextoRecurso
  {
    public readonly Transform Origen;
    public readonly float Tiempo;

    public RegistroTextoRecurso(Transform origen, float tiempo)
    {
      Origen = origen;
      Tiempo = tiempo;
    }
  }

  #region Tooltips
  public GameObject tooltipGOEsperanza;
  public GameObject tooltipGOCiviles;
  public GameObject tooltipGOSuministros;
  public GameObject tooltipGOMateriales;
  public GameObject tooltipGOBueyes;
  public GameObject tooltipGOOro;
  public GameObject tooltipGOFatiga;
  public GameObject tooltipGOMecanicaZona;
  public GameObject tooltipGOPresagios;
  public GameObject tooltipGOAntorcha;


  public void TooltipRecursoEntrar(int n)
  {

    if (n == 1) //Esperanza
    {
      tooltipGOEsperanza.SetActive(true);
      String text = "";

      int num = GetEsperanzaActual();
      text = TRADU.i.Traducir("La <color=#a0e812>Esperanza</color> determina el optimismo de la Caravana en general sobre la posibilidad de cumplir la misión y llegar al puerto.\n\n");
      text += num + TRADU.i.Traducir("/100 de <color=#a0e812>Esperanza</color>\n");

      if (num < 11) { text += TRADU.i.Traducir(" <color=#982a1b>1-20 Civiles abandonarán la Caravana cada descanso.</color>\n"); }
      if (num < 20 && num >= 11) { text += TRADU.i.Traducir(" <color=#982a1b>1-10 Civiles abandonarán la Caravana cada descanso.</color>\n"); }
      if (num >= 20 && num < 80) { text += ""; }
      if (num >= 80 && num < 90) { text += TRADU.i.Traducir(" <color=#39a91b>Los Civiles donarán algo de Oro cada descanso.</color>\n"); }
      if (num >= 90) { text += TRADU.i.Traducir(" <color=#39a91b>Los Civiles donarán buena cantidad de Oro cada descanso.</color>\n"); }

      tooltipGOEsperanza.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 2) //Civiles
    {
      tooltipGOCiviles.SetActive(true);
      String text = "";

      float num = GetCivilesActual();
      text = TRADU.i.Traducir("Los <color=#c918bb>Civiles</color> que lleva la caravana hacia el Puerto. Salvar la mayor cantidad es el objetivo principal de esta misión.\n\nCada uno consume 1 de <color=#b7972c>Suministros</color> cada Descanso, y la cantidad de Civiles determina la eficiencia de las Tareas Civiles.\n");
      text += TRADU.i.Traducir("\nLlevas ") + (int)num + TRADU.i.Traducir(" <color=#c918bb>Civiles</color> en la caravana.\n\n");
      text += TRADU.i.Traducir("\nLas fuerzas de la Milicia de la caravana son de <color=#a8a29c>") + (int)GetMiliciasActual() + " </color>" + TRADU.i.Traducir(", que equivalen a ") + "<color=#a8a29c>" + (int)GetMiliciasActual() / 10 + TRADU.i.Traducir("</color> Milicianos que ayudarán a defenderla de ataques directos.\n\n");



      tooltipGOCiviles.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 3) //Suministros
    {
      tooltipGOSuministros.SetActive(true);
      String text = "";

      text = TRADU.i.Traducir("<color=#ffdda5>---<b>Haz click para abandonar <color=#b7972c>5 Suministros</color> y alivianar la Carga. -1 Esperanza</b>---</color>\n\n");
      int num = GetSuministrosActuales();
      text += TRADU.i.Traducir($"Los <color=#b7972c>Suministros</color> constituyen las reservas de comida y elementos de supervivencia de la caravana.\n\nCada <color=#c918bb>Civil</color> consume 1 en cada Descanso. Los Bueyes consumen 2.\n");
      text += TRADU.i.Traducir("\nLlevas ") + num + TRADU.i.Traducir(" <color=#b7972c>Suministros</color>, por un total de peso de ") + num + ".\n\n";



      tooltipGOSuministros.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 4) //Materiales
    {
      tooltipGOMateriales.SetActive(true);
      String text = "";

      text = TRADU.i.Traducir("<color=#ffdda5>---<b>Haz click para abandonar <color=#b34f09>2 Materiales</color> y alivianar la Carga.</b>---</color>\n\n");
      int num = GetMaterialesActuales();
      text += TRADU.i.Traducir("Los <color=#b34f09>Materiales</color> son elementos básicos de construcción utilizados para mantenimiento y expansión de la caravana.\nCada uno pesa 3.\n");
      text += TRADU.i.Traducir("\nLlevas ") + num + TRADU.i.Traducir(" <color=#b34f09>Materiales</color>, por un total de peso de ") + (num * 3) + ".\n\n";



      tooltipGOMateriales.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 5) //Carga
    {
      tooltipGOBueyes.SetActive(true);
      String text = "";
      text = TRADU.i.Traducir("<color=#ffdda5>---<b>Haz click para sacrificar <color=#9e2a1c>1 Buey</color> para obtener <color=#b7972c>20 Suministros</color>. -2 Esperanza</b>---</color>\n\n");
      int num = GetBueyesActual();
      int num2 = GetSuministrosActuales();
      int num3 = GetMaterialesActuales();
      int num4 = GetCargaLlevadaActual();
      int capacidadActual = GetCapacidadDeCargaActual();

      int cargaPorBuey = 25 + Mathf.Max(0, mejoraCaravanaAlforjas - 1);
      text += TRADU.i.Traducir("Los <color=#9e2a1c>Bueyes</color> son utilizados para llevar la carga de la caravana.\nCada uno da ") + cargaPorBuey + TRADU.i.Traducir(" de Capacidad de Carga.\n");
      text += TRADU.i.Traducir("\nLlevas ") + num + TRADU.i.Traducir(" <color=#9e2a1c>Bueyes</color>, por un total de Capacidad de Carga de ") + capacidadActual + ".\n\n";
      text += TRADU.i.Traducir("\nLlevas ") + num2 + TRADU.i.Traducir(" <color=#b7972c>Suministros</color> y ") + num3 + TRADU.i.Traducir(" <color=#b34f09>Materiales</color> por un total de peso de ") + num4 + "/" + capacidadActual + ".\n\n";

      if (num4 > GetCapacidadDeCargaActual())
      {
        text += TRADU.i.Traducir("<color=#cc0d0d>La Caravana lleva Sobrecarga. Cada tramo reduce 10 la <color=#a0e812>Esperanza</color> y la velocidad de viaje se reduce un 30%.</color>\n\n");

      }

      tooltipGOBueyes.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 6) //Oro
    {
      tooltipGOOro.SetActive(true);
      String text = "";


      text = TRADU.i.Traducir("El <color=#d8a205>Oro</color> que lleva la Caravana, utilizado para comprar bienes y contratar servicios.");

      tooltipGOOro.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 7) //Fatiga
    {
      tooltipGOFatiga.SetActive(true);
      String text = "";

      int num = GetFatigaActual();
      text = TRADU.i.Traducir("Indica que tanta <color=#06c297>Fatiga</color> tiene la Caravana en general.\n");
      text += TRADU.i.Traducir("Cada tramo de viaje la aumenta en 1.\n");
      text += TRADU.i.Traducir("Si descansas volverá a 0 y arrancarán el nuevo día Descansados(1).\n\n");



      switch (FatigaActual)
      {
        case 0: text += TRADU.i.Traducir("Actualmente estan Descansados(1), no habrá penalizaciones por viajar.\n\n"); break;
        case 1: text += TRADU.i.Traducir("Actualmente estan Frescos(2), no habrá penalizaciones por viajar."); break;
        case 2: text += TRADU.i.Traducir("Actualmente estan En Marcha(3), no habrá penalizaciones por viajar."); break;
        case 3: text += TRADU.i.Traducir("Actualmente estan Agitados(4), -10 Esperanza, pocos Bueyes podrán morir si viajas."); break;
        case 4: text += TRADU.i.Traducir("Actualmente estan Cansados(5), -15 Esperanza y algunos Bueyes podrán morir si viajas."); break;
        case > 4: text += TRADU.i.Traducir("Actualmente estan Exhaustos(6), -20 Esperanza y varios Bueyes podrán morir si viajas."); break;
      }


      tooltipGOFatiga.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;

    }
    else if (n == 8) //Mecánica Especial del mapa
    {

      tooltipGOMecanicaZona.SetActive(true);
      TextMeshProUGUI text = tooltipGOMecanicaZona.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
      text.text = scAtributosZona != null
        ? ZonaMecanicaTextos.ObtenerDescripcion(
          scAtributosZona.ID,
          scAtributosZona.PasoVientoHelado_FuerzaKaleTav,
          MecanicasZonaConocidas(scAtributosZona.ID))
        : string.Empty;



    }
    else if (n == 9) //Presagios activos
    {
      if (tooltipGOPresagios == null)
      {
        return;
      }

      tooltipGOPresagios.SetActive(true);
      TMP_Text textObject = tooltipGOPresagios.GetComponentInChildren<TMP_Text>(true);
      if (textObject != null)
      {
        List<string> textosPresagios = new List<string>();
        for (int i = 0; i < presagiosActivosRegion.Count; i++)
        {
          string textoPresagio = PresagioCatalog.ObtenerTextoLocalizado(presagiosActivosRegion[i]);
          if (!string.IsNullOrWhiteSpace(textoPresagio))
          {
            textosPresagios.Add(textoPresagio);
          }
        }

        textObject.text = textosPresagios.Count > 0
          ? string.Join("\n\n", textosPresagios)
          : PresagioCatalog.ObtenerTextoSinPresagios();
      }
    }
    else if (n == 10) //tooltipantorcha
    {
      tooltipGOAntorcha.SetActive(true);
    }



  }

  public void TooltipRecursoSalir()
  {
    tooltipGOEsperanza.SetActive(false);
    tooltipGOCiviles.SetActive(false);
    tooltipGOSuministros.SetActive(false);
    tooltipGOMateriales.SetActive(false);
    tooltipGOBueyes.SetActive(false);
    tooltipGOOro.SetActive(false);
    tooltipGOFatiga.SetActive(false);
    tooltipGOMecanicaZona.SetActive(false);
    tooltipGOAntorcha.SetActive(false);

    if (tooltipGOPresagios != null)
    {
      tooltipGOPresagios.SetActive(false);
    }
  }

  #endregion


  #region Descanso
  [SerializeField] Image botonDescansar;
  public Sprite campSi;
  public Sprite campNo;

  [SerializeField] GameObject menuDescanso;

  public void AbrirMenuDescanso()
  {
    if (MoviendoCaravana) { return; }
    if (scTutorialManager.tutorialActivo && scTutorialManager.pasoActual < 24) { return; }
    if (scTutorialManager.tutorialActivo && scTutorialManager.pasoActual == 24) { scTutorialManager.SiguientePaso(); }
    if (asentamientoManager != null && asentamientoManager.TieneInteraccionActiva) { return; }
    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    if (nodoActual != null && nodoActual.tipoNodo == 4)
    {
      EscribirAdvertenciaLog(TRADU.i.Traducir("<color=#FF6666>En los Asentamientos debes usar las acciones propias del asentamiento.</color>"));
      return;
    }
        TutorialEvents.Emit("ui.descanso_presionado", gameObject);

    bool puedeDescansar = PuedeAcamparEnNodo(nodoActual);

    if (!menuDescanso.activeInHierarchy && puedeDescansar)
    {
      menuDescanso.GetComponent<MenuDescanso>().SeleccionarActividadCivil(1);
      menuDescanso.SetActive(true);
    }
    else
    {
      menuDescanso.SetActive(false);

    }
    if (nodoActual != null && !puedeDescansar)
    { EscribirAdvertenciaLog(TRADU.i.Traducir("<color=#FF6666>No puedes descansar aquí.</color>")); }


  }

  public int intTipoClima; //1 - Sol, 2 - Calor, 3 - Lluvia, 4 - Nieve, 5 - Niebla
                           // Especiales:
                           // 6 - Bosque Ardiente Almas Danzantes
                           // 7 - Paso Vientohelado Aurora Boreal
                           // 8 - Nedukazal Oscuridad
                           // 9 - Nedukazal - Masacre
  public Image widgetClima;
  public Sprite clima_lluvia;
  public Sprite clima_nieve;
  public Sprite clima_sol;
  public Sprite clima_calor;
  public Sprite clima_niebla;
  public Sprite clima_almasDanzantes;
  public Sprite clima_auroraboreal;
  public Sprite clima_NedukazalNormal;
  public Sprite clima_NedukazalMasacre;
  public GameObject climaTooltip;
  public TextMeshProUGUI textClimaTooltip;

  public void ActivartooltipClima(int n)
  {
    if (n == 1)
    {
      climaTooltip.SetActive(true);

      if (intTipoClima == 4)
      {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
        string descripcionNieve = idioma switch
        {
          TRADU.IdiomaIngles => "Snow: only Rest or Guard activities are allowed. -15% Gatherings, -20% Ambush, -15% travel speed. Traveling costs -3 Hope per map section.",
          TRADU.IdiomaPortugues => "Neve: permite apenas Descansar ou Guarda. -15% Coletas, -20% Emboscada, -15% velocidade de viagem. Viajar custa -3 Esperança por trecho do mapa.",
          _ => "Nieve: solo permite Descansar o Guardia. -15% Recolecciones, -20% Emboscada, -15% velocidad de viaje. Viajar da -3 Esperanza por tramo del mapa."
        };
        textClimaTooltip.text = descripcionNieve;
        return;
      }

      switch (intTipoClima)
      {
        case 1: textClimaTooltip.text = ClimaZonaCatalog.ObtenerTooltipCampania(ClimaZonaCatalog.ClimaSol); break;
        case 2: textClimaTooltip.text = TRADU.i.Traducir("Ola de Calor: +1 Fatiga. Día Libre da +5 Esperanza, otras Tareas Civiles dan -3."); break;
        case 3: textClimaTooltip.text = TRADU.i.Traducir("Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% chances de Emboscada."); break;
        case 4: break;
        case 5:
        {
          int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
          textClimaTooltip.text = idioma switch
          {
            TRADU.IdiomaIngles => "Fog: -15% Vision, -20% Gatherings, -10% Exploration, -20% Ambush.",
            TRADU.IdiomaPortugues => "Névoa: -15% Visão, -20% Coletas, -10% Exploração, -20% Emboscada.",
            _ => "Niebla: -15% Visión, -20% Recolecciones, -10% Exploración, -20% Emboscada."
          };
          break;
        }
        case 6: textClimaTooltip.text = TRADU.i.Traducir("Almas Danzantes: +3 Esperanza por viaje, -100% de probabilidad de Emboscada."); break;
        case 7: textClimaTooltip.text = TRADU.i.Traducir("Aurora Boreal: +10 Esperanza."); break;
        case 8: textClimaTooltip.text = TRADU.i.Traducir("Nedukazal está a oscuras."); break;
        case 9: textClimaTooltip.text = TRADU.i.Traducir("Masacre: Nedukazal está siendo atacada. -10 Esperanza. +10% Emboscada. Los Zúrkil están potenciados."); break;


      }


    }
    else
    {
      climaTooltip.SetActive(false);

    }

  }


  #endregion

  #region Eventos

  public GameObject UIEvetos;
  public void EmpezarEvento(int ID)
  {
    UIEvetos.SetActive(true);
    UIEvetos.GetComponent<EventosAdmin>().EmpezarEvento(ID);
  }

  // Debug: abre la pantalla de eventos al iniciar la campania y la deja en modo
  // ciclado, con botones para recorrer absolutamente todos los eventos del juego
  // (ver debugAbrirPantallaEventosAlIniciar).
  void AbrirPantallaEventosDebug()
  {
    if (UIEvetos == null)
    {
      return;
    }

    EventosAdmin eventosAdmin = UIEvetos.GetComponent<EventosAdmin>();
    if (eventosAdmin == null)
    {
      return;
    }

    UIEvetos.SetActive(true);
    eventosAdmin.AbrirEnModoDebugCicladoEventos();
  }

  public bool EmpezarEventoMalo(TipoOrigenEventoCampania origen = TipoOrigenEventoCampania.Nodo)
  {
    return EmpezarEventoAleatorio(origen, TipoResultadoEventoCampania.Malo);
  }

  public bool EmpezarEventoBueno(TipoOrigenEventoCampania origen = TipoOrigenEventoCampania.Nodo)
  {
    return EmpezarEventoAleatorio(origen, TipoResultadoEventoCampania.Bueno);
  }

  bool EmpezarEventoAleatorio(TipoOrigenEventoCampania origen, TipoResultadoEventoCampania resultado)
  {
    if (UIEvetos == null)
    {
      return false;
    }

    EventosAdmin eventosAdmin = UIEvetos.GetComponent<EventosAdmin>();
    if (eventosAdmin == null)
    {
      return false;
    }

    UIEvetos.SetActive(true);

    bool eventoIniciado = resultado == TipoResultadoEventoCampania.Bueno
      ? eventosAdmin.TirarEventoBueno(origen)
      : eventosAdmin.TirarEventoMalo(origen);

    if (!eventoIniciado)
    {
      UIEvetos.SetActive(false);
    }

    return eventoIniciado;
  }

  #endregion



  public MenuCaravana scMenuCaravana;
  public GameObject MenuOpciones;

  public void abrirOpciones()
  {
    MenuOpciones.SetActive(!MenuOpciones.activeInHierarchy);
  }

  private bool CerrarMenuOpcionesSiEstaAbierto()
  {
    if (MenuOpciones != null && MenuOpciones.activeInHierarchy)
    {
      MenuOpciones.SetActive(false);
      return true;
    }

    AdministradorEscenas administradorEscenas = scAdministradorEscenas;
    if (administradorEscenas != null && administradorEscenas.MenuOpciones != null && administradorEscenas.MenuOpciones.activeInHierarchy)
    {
      administradorEscenas.MenuOpciones.SetActive(false);
      return true;
    }

    return false;
  }

  private void NotificarResultadoGuardado(string mensaje, Color color)
  {
    if (logDeCampania != null && numeroTurno > 1)
    {
      logDeCampania.SetDiaActual(numeroTurno);
      logDeCampania.Escribir(mensaje);
    }

    GenerarTextoFlotanteCampaña(TRADU.i.Traducir(mensaje), color);
    Debug.Log("[SaveGame] " + mensaje);
  }

  private string FormatearOrigenAutosave(string origen)
  {
    if (string.IsNullOrWhiteSpace(origen))
    {
      return string.Empty;
    }

    return " (" + origen + ")";
  }

  public bool DebeMostrarTodosLosCaminosMapaDebug()
  {
    return debugMostrarTodosLosCaminosMapa;
  }

  private void RefrescarDebugMostrarTodosLosCaminosSiCambio()
  {
    if (debugMostrarTodosLosCaminosMapaAplicado == debugMostrarTodosLosCaminosMapa)
    {
      return;
    }

    debugMostrarTodosLosCaminosMapaAplicado = debugMostrarTodosLosCaminosMapa;
    if (scMapaManager != null)
    {
      scMapaManager.RefrescarVisibilidadExploracion();
    }
  }

  void Update()
  {
    AsegurarLuzAntorchasCaravana();
    AnimarRelojHora();

    if (BattleManager.Instance != null && BattleManager.Instance.EntradaBatallaBloqueadaPorUI)
    {
      return;
    }

    ActualizarCursorCampania();
    ActualizarBotonesAccionNodoActual();
    RefrescarDebugMostrarTodosLosCaminosSiCambio();

    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    bool puedeDescansar = PuedeAcamparEnNodo(nodoActual);
    bool asentamientoActivo = asentamientoManager != null && asentamientoManager.TieneInteraccionActiva;

    if (botonDescansar != null)
    {
      botonDescansar.sprite = puedeDescansar ? campSi : campNo;
    }

    if (IntroCampaniaActivaOPendiente)
    {
      LimpiarHoverNodoCampaniaActual();
      return;
    }

    ActualizarAdvertenciaSobrecarga();

    bool tutorialConfiguracionActiva = DebeUsarConfiguracionTutorial();

    if (!tutorialConfiguracionActiva && !asentamientoActivo)
    {
      int posicionPersonaje = ObtenerPosicionAtajoPersonaje();
      if (posicionPersonaje > 0)
      {
        AbrirMenuPersonajePorAtajo(posicionPersonaje);
      }
    }

    if (!tutorialConfiguracionActiva && Input.GetKeyDown(teclaDebugBajoMouse))
    {
      DebugObjetosBajoMouseCampania();
    }

    ProcesarInputMouseNodosCampania();


    //HOTKEYS
    // Detecta cuando se presiona la tecla H una sola vez
    if (Input.GetKeyDown(KeyCode.R))
    {
      if (tutorialConfiguracionActiva) { EscribirAdvertenciaLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados.")); return; }
      if (asentamientoActivo) { return; }
      AbrirMenuDescanso();
    }
    if (Input.GetKeyDown(KeyCode.C))
    {
       if (tutorialConfiguracionActiva)
       {
          if (scMenuCaravana != null
              && scMenuCaravana.MenuPersonajesEstaAbierto()
              && TutorialSolicitaCierrePorHotkey(TutorialEventNames.CampaignCharacterMenuClosed))
          {
            scMenuCaravana.AbrirMenuPersonajesDesdeHotkey();
          }
         else
         {
           EscribirAdvertenciaLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados."));
         }
         return;
       }
      if (asentamientoActivo) { return; }
      scMenuCaravana.AbrirMenuPersonajesDesdeHotkey();
    }
    if (Input.GetKeyDown(KeyCode.I))
    {
       if (tutorialConfiguracionActiva)
       {
          if (scMenuCaravana != null
              && scMenuCaravana.MenuMejorasEstaAbierto()
              && TutorialSolicitaCierrePorHotkey(TutorialEventNames.CampaignUpgradeMenuClosed))
          {
            scMenuCaravana.AbrirMenuMejorasDesdeHotkey();
          }
         else
         {
           EscribirAdvertenciaLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados."));
         }
         return;
      }
      if (asentamientoActivo) { return; }
      scMenuCaravana.AbrirMenuMejorasDesdeHotkey();
    }
    if (Input.GetKeyDown(KeyCode.M))
    {
       if (tutorialConfiguracionActiva)
       {
          if (scMenuCaravana != null
              && scMenuCaravana.MenuSequitosEstaAbierto()
              && TutorialSolicitaCierrePorHotkey(TutorialEventNames.CampaignFollowersMenuClosed))
          {
            scMenuCaravana.AbrirMenuSequitosDesdeHotkey();
          }
         else
         {
           EscribirAdvertenciaLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados."));
         }
         return;
       }
      if (asentamientoActivo) { return; }
      scMenuCaravana.AbrirMenuSequitosDesdeHotkey();
    }
    if (Input.GetKeyDown(KeyCode.F5))
    {
      if (tutorialConfiguracionActiva) { EscribirAdvertenciaLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados.")); return; }
      GuardarCampaniaManual();
    }
    if (Input.GetKeyDown(KeyCode.F9))
    {
      if (tutorialConfiguracionActiva) { EscribirAdvertenciaLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados.")); return; }
      CargarCampaniaManual();
    }
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (tutorialConfiguracionActiva)
      {
        EscribirAdvertenciaLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados."));
        return;
      }

      if (CerrarMenuOpcionesSiEstaAbierto())
      {
        return;
      }

      if (asentamientoActivo) { return; }

      if (scMenuCaravana != null && scMenuCaravana.SeApretoESC()) //Si se apreto escape se cierran menus, si no habia ningun abierto abre opciones
      {
        return;
      }

      if (menuDescanso != null && menuDescanso.activeInHierarchy)
      {
        menuDescanso.SetActive(false);
      }
      else if (MenuOpciones != null)
      {
        MenuOpciones.SetActive(true);
      }

    }


  }

  private bool TutorialSolicitaCierrePorHotkey(string eventId)
  {
    TutorialDirector director = TutorialDirector.Instance;
    TutorialStep paso = director != null && director.IsRunning ? director.CurrentStep : null;
    if (paso != null && paso.advanceConditions != null)
    {
      for (int i = 0; i < paso.advanceConditions.Count; i++)
      {
        TutorialCondition condicion = paso.advanceConditions[i];
        if (condicion == null
            || !string.Equals(condicion.eventId, eventId, StringComparison.Ordinal)
            || condicion.requiredValues == null)
        {
          continue;
        }

        for (int j = 0; j < condicion.requiredValues.Count; j++)
        {
          TutorialConditionValue valor = condicion.requiredValues[j];
          if (valor != null
              && string.Equals(valor.key, "closedByHotkey", StringComparison.OrdinalIgnoreCase)
              && string.Equals(valor.value, "1", StringComparison.OrdinalIgnoreCase))
          {
            return true;
          }
        }
      }

      return false;
    }

    return eventId == TutorialEventNames.CampaignUpgradeMenuClosed
      && scTutorialManager != null
      && scTutorialManager.tutorialActivo
      && scTutorialManager.pasoActual == 17;
  }

  public bool UsaRaycastManualNodosCampania => true;

  void ProcesarInputMouseNodosCampania()
  {
    Nodo nodoBajoMouse = ObtenerNodoBajoMouseCampania();
    if (nodoHoverCampaniaActual != nodoBajoMouse)
    {
      if (nodoHoverCampaniaActual != null)
      {
        nodoHoverCampaniaActual.ProcesarMouseExitDesdeRaycast();
      }

      nodoHoverCampaniaActual = nodoBajoMouse;

      if (nodoHoverCampaniaActual != null)
      {
        nodoHoverCampaniaActual.ProcesarMouseEnterDesdeRaycast();
      }
    }

    if (nodoHoverCampaniaActual == null)
    {
      return;
    }

    if (Input.GetMouseButtonDown(0))
    {
      nodoHoverCampaniaActual.ProcesarClickIzquierdoDesdeRaycast();
    }

    if (Input.GetMouseButtonDown(1))
    {
      nodoHoverCampaniaActual.ProcesarClickDerechoDesdeRaycast();
    }
  }

  private static int ObtenerPosicionAtajoPersonaje()
  {
    if (Input.GetKeyDown(KeyCode.Alpha1)) { return 1; }
    if (Input.GetKeyDown(KeyCode.Alpha2)) { return 2; }
    if (Input.GetKeyDown(KeyCode.Alpha3)) { return 3; }
    if (Input.GetKeyDown(KeyCode.Alpha4)) { return 4; }
    if (Input.GetKeyDown(KeyCode.Alpha5)) { return 5; }
    if (Input.GetKeyDown(KeyCode.Alpha6)) { return 6; }
    if (Input.GetKeyDown(KeyCode.Alpha7)) { return 7; }
    return 0;
  }

  private void AbrirMenuPersonajePorAtajo(int posicion)
  {
    if (scMenuCaravana == null || scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    int posicionActual = 0;
    foreach (Personaje personaje in scMenuPersonajes.listaPersonajes)
    {
      if (personaje == null || personaje.Camp_Muerto)
      {
        continue;
      }

      posicionActual++;
      if (posicionActual == posicion)
      {
        scMenuCaravana.AbrirMenuPersonajesDesdeHotkey(personaje);
        return;
      }
    }
  }

  Nodo ObtenerNodoBajoMouseCampania()
  {
    if (!Application.isFocused)
    {
      return null;
    }

    if (EstaPunteroSobreUICampania()
        && !DebePermitirRaycastNodosBajoUITutorial())
    {
      return null;
    }

    Camera cam = ObtenerCamaraRaycastNodosCampania();

    if (cam == null)
    {
      return null;
    }

    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
    int cantidadHits = Physics.RaycastNonAlloc(
      ray,
      hitsRaycastNodosCampania,
      500f,
      Physics.DefaultRaycastLayers,
      QueryTriggerInteraction.Collide);

    if (cantidadHits >= hitsRaycastNodosCampania.Length)
    {
      return ObtenerNodoBajoMouseCampaniaFallback(ray);
    }

    return ObtenerNodoMasCercano(hitsRaycastNodosCampania, cantidadHits);
  }

  Camera ObtenerCamaraRaycastNodosCampania()
  {
    if (camaraRaycastNodosCampania != null
        && camaraRaycastNodosCampania.enabled
        && camaraRaycastNodosCampania.gameObject.activeInHierarchy)
    {
      return camaraRaycastNodosCampania;
    }

    camaraRaycastNodosCampania = Camera.main;
    if (camaraRaycastNodosCampania != null)
    {
      return camaraRaycastNodosCampania;
    }

    foreach (Camera cameraActiva in Camera.allCameras)
    {
      if (cameraActiva != null && cameraActiva.enabled)
      {
        camaraRaycastNodosCampania = cameraActiva;
        break;
      }
    }

    return camaraRaycastNodosCampania;
  }

  static Nodo ObtenerNodoMasCercano(RaycastHit[] hits, int cantidadHits)
  {
    Nodo nodoMasCercano = null;
    float distanciaMasCercana = float.PositiveInfinity;

    for (int i = 0; i < cantidadHits; i++)
    {
      RaycastHit hit = hits[i];
      if (hit.distance >= distanciaMasCercana || hit.collider == null)
      {
        continue;
      }

      Nodo nodo = hit.collider.GetComponentInParent<Nodo>();
      if (nodo == null || !nodo.gameObject.activeInHierarchy)
      {
        continue;
      }

      nodoMasCercano = nodo;
      distanciaMasCercana = hit.distance;
    }

    return nodoMasCercano;
  }

  static Nodo ObtenerNodoBajoMouseCampaniaFallback(Ray ray)
  {
    RaycastHit[] hits = Physics.RaycastAll(
      ray,
      500f,
      Physics.DefaultRaycastLayers,
      QueryTriggerInteraction.Collide);
    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

    for (int i = 0; i < hits.Length; i++)
    {
      Collider collider = hits[i].collider;
      if (collider == null)
      {
        continue;
      }

      Nodo nodo = collider.GetComponentInParent<Nodo>();
      if (nodo != null && nodo.gameObject.activeInHierarchy)
      {
        return nodo;
      }
    }

    return null;
  }

  bool EstaPunteroSobreUICampania()
  {
    EventSystem eventSystemActual = EventSystem.current;
    if (eventSystemActual == null)
    {
      return false;
    }

    if (pointerRaycastUICampania == null || eventSystemRaycastUICampania != eventSystemActual)
    {
      eventSystemRaycastUICampania = eventSystemActual;
      pointerRaycastUICampania = new PointerEventData(eventSystemActual);
    }

    pointerRaycastUICampania.Reset();
    pointerRaycastUICampania.position = Input.mousePosition;
    resultadosRaycastUICampania.Clear();
    eventSystemActual.RaycastAll(pointerRaycastUICampania, resultadosRaycastUICampania);
    return resultadosRaycastUICampania.Count > 0;
  }

  bool DebePermitirRaycastNodosBajoUITutorial()
  {
    TutorialDirector tutorial = TutorialDirector.Instance;
    TutorialStep pasoTutorial = tutorial != null ? tutorial.CurrentStep : null;
    return tutorial != null
      && tutorial.IsRunning
      && pasoTutorial != null
      && EsPasoInteraccionNodoTutorial(pasoTutorial);
  }

  private void LateUpdate()
  {
    LimpiarRegistrosTextosFlotantesCampaniaDestruidos();
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
      && (targetId.StartsWith("tut_nodo", StringComparison.OrdinalIgnoreCase)
        || targetId.StartsWith("tuto_nodo", StringComparison.OrdinalIgnoreCase));
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

  void LimpiarHoverNodoCampaniaActual()
  {
    if (nodoHoverCampaniaActual == null)
    {
      return;
    }

    nodoHoverCampaniaActual.ProcesarMouseExitDesdeRaycast();
    nodoHoverCampaniaActual = null;
  }

  private void DebugObjetosBajoMouseCampania()
  {
    List<string> lineasDebug = new List<string>();
    Vector3 mousePos = Input.mousePosition;
    lineasDebug.Add("[MouseDebugCampania] Frame: " + Time.frameCount);
    lineasDebug.Add("[MouseDebugCampania] Posicion mouse: " + mousePos);

    if (EventSystem.current != null)
    {
      PointerEventData pointerData = new PointerEventData(EventSystem.current)
      {
        position = mousePos
      };

      List<RaycastResult> resultadosUI = new List<RaycastResult>();
      EventSystem.current.RaycastAll(pointerData, resultadosUI);
      lineasDebug.Add("[MouseDebugCampania] UI hits: " + resultadosUI.Count + " | IsPointerOverGameObject: " + EventSystem.current.IsPointerOverGameObject());

      for (int i = 0; i < resultadosUI.Count && i < maxHitsDebugBajoMouse; i++)
      {
        RaycastResult hit = resultadosUI[i];
        lineasDebug.Add("  UI[" + i + "] " + DescribirJerarquiaDebug(hit.gameObject) + " | dist=" + hit.distance.ToString("0.###") + " | sort=" + hit.sortingOrder + " | layer=" + LayerMask.LayerToName(hit.gameObject.layer));
      }
    }
    else
    {
      lineasDebug.Add("[MouseDebugCampania] Sin EventSystem activo.");
    }

    Camera cam = Camera.main;
    if (cam == null)
    {
      foreach (Camera cameraActiva in Camera.allCameras)
      {
        if (cameraActiva != null && cameraActiva.enabled)
        {
          cam = cameraActiva;
          break;
        }
      }
    }

    if (cam == null)
    {
      lineasDebug.Add("[MouseDebugCampania] Sin camara activa para raycast 3D.");
      Debug.Log(string.Join("\n", lineasDebug));
      return;
    }

    Ray ray = cam.ScreenPointToRay(mousePos);
    Debug.DrawRay(ray.origin, ray.direction * 500f, Color.cyan, 2f);
    RaycastHit[] hits3D = Physics.RaycastAll(ray, 500f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
    System.Array.Sort(hits3D, (a, b) => a.distance.CompareTo(b.distance));

    lineasDebug.Add("[MouseDebugCampania] 3D hits: " + hits3D.Length + " | camara: " + cam.name);
    for (int i = 0; i < hits3D.Length && i < maxHitsDebugBajoMouse; i++)
    {
      RaycastHit hit = hits3D[i];
      GameObject go = hit.collider != null ? hit.collider.gameObject : null;
      if (go == null)
      {
        continue;
      }

      Debug.DrawLine(ray.origin, hit.point, i == 0 ? Color.green : Color.yellow, 2f);
      Nodo nodo = go.GetComponentInParent<Nodo>();
      lineasDebug.Add(
        "  3D[" + i + "] " + DescribirJerarquiaDebug(go)
        + " | collider=" + hit.collider.GetType().Name
        + " | dist=" + hit.distance.ToString("0.###")
        + " | layer=" + LayerMask.LayerToName(go.layer)
        + " | tag=" + go.tag
        + " | nodo=" + (nodo != null ? nodo.posXNodo + "," + nodo.posYNodo + " tipo=" + nodo.tipoNodo : "none"));
    }

    Debug.Log(string.Join("\n", lineasDebug));
  }

  private static string DescribirJerarquiaDebug(GameObject go)
  {
    if (go == null)
    {
      return "(null)";
    }

    List<string> nombres = new List<string>();
    Transform actual = go.transform;
    while (actual != null)
    {
      nombres.Add(actual.name);
      actual = actual.parent;
    }

    nombres.Reverse();
    return string.Join("/", nombres);
  }

  private void ActualizarCursorCampania(bool forzar = false)
  {
    bool debeMostrarCursorAlerta = DebeMostrarCursorAlertaCampania();
    if (!forzar && debeMostrarCursorAlerta == cursorCampaniaMostrandoAlerta)
    {
      return;
    }

    cursorCampaniaMostrandoAlerta = debeMostrarCursorAlerta;
    CursorVisualManager.EstablecerAlerta(this, debeMostrarCursorAlerta);
  }

  private bool DebeMostrarCursorAlertaCampania()
  {
    if (!isActiveAndEnabled)
    {
      return false;
    }

    if (scAdministradorEscenas != null && scAdministradorEscenas.escenaActual != 0)
    {
      return false;
    }

    if (HayInteraccionTransitoriaActiva())
    {
      return false;
    }

    return EstaActiva(alertaFatiga)
      || EstaActiva(alertaCarga);
  }

  private static bool EstaActiva(GameObject alerta)
  {
    return alerta != null && alerta.activeInHierarchy;
  }

  private void RestaurarCursorCampaniaPredeterminado()
  {
    cursorCampaniaMostrandoAlerta = false;
    CursorVisualManager.EstablecerAlerta(this, false);
  }



  #region Log
  [SerializeField] TextMeshProUGUI txtLog;
  [SerializeField] GameObject goLog;

  public bool LogEstaAbierto()
  {
    return goLog != null && goLog.activeInHierarchy;
  }

  public void ActivarLog(int n)
  {
    if (scAdministradorEscenas != null
      && scAdministradorEscenas.escenaActual == 1
      && BattleManager.Instance != null)
    {
      BattleManager.Instance.ActivarLog(n);
      return;
    }

    if (n == 1)
    {
      if (MoviendoCaravana)
      {
        return;
      }

      scMenuCaravana?.CerrarMenusExclusivos();
      goLog.SetActive(true);
      logDeCampania?.ForzarReacomodoLibro();
    }
    else
    {
      goLog.SetActive(false);
    }

  }

  [SerializeField] public LogDeCampania logDeCampania;
  private void EscribirLogInicioCampania()
  {
    if (logInicioCampaniaEscrito || logDeCampania == null)
      return;

    string logInicio = TRADU.i != null ? TRADU.i.Traducir(TEXTO_LOG_INICIO_CAMPANIA) : TEXTO_LOG_INICIO_CAMPANIA;
    logDeCampania.SetDiaActual(numeroTurno);
    logDeCampania.Escribir(AgregarHoraABitacora(logInicio));
    logInicioCampaniaEscrito = true;
  }
  public void EscribirLog(string log, bool forzarAunqueNumeroTurno1 = false)
  {
    if (logDeCampania == null) return;

    // Asegura que el logger sabe el día actual
    logDeCampania.SetDiaActual(numeroTurno);
    logDeCampania.Escribir(AgregarHoraABitacora(log));

    // Durante la intro se conserva la bitácora, pero no se crean mensajes
    // flotantes sobre el mapa. Al terminar la intro se habilitan también en el Día 1.
    if (!forzarAunqueNumeroTurno1 && IntroCampaniaActivaOPendiente) return;

    if (bloqueoTextosFlotantesCampania > 0)
    {
      colaTextosSuspendidosCampania.Enqueue((log, Color.cyan));
      return;
    }

    GenerarTextoFlotanteCampaña(log, Color.cyan);


  }

  private void EscribirLogEnBitacoraSinTextoFlotante(string log, bool forzarAunqueNumeroTurno1 = false)
  {
    if (logDeCampania == null) return;

    logDeCampania.SetDiaActual(numeroTurno);
    logDeCampania.Escribir(AgregarHoraABitacora(log));
  }

  private string AgregarHoraABitacora(string texto)
  {
    return "[" + FormatearHoraCampania(ObtenerHoraActual()) + "] " + texto;
  }

  public void EscribirAdvertenciaLog(string log, bool forzarAunqueNumeroTurno1 = false)
  {
    EscribirLogSinBitacora(log, forzarAunqueNumeroTurno1);
  }

  private void EscribirLogSinBitacora(string log, bool forzarAunqueNumeroTurno1 = false)
  {
    if (logDeCampania == null) return;

    if (!forzarAunqueNumeroTurno1 && IntroCampaniaActivaOPendiente) return;

    if (bloqueoTextosFlotantesCampania > 0)
    {
      colaTextosSuspendidosCampania.Enqueue((log, Color.cyan));
      return;
    }

    GenerarTextoFlotanteCampaña(log, Color.cyan);
  }

  public void ComenzarBufferTextosFlotantesCampania()
  {
    bloqueoTextosFlotantesCampania++;
    bloqueoTextosRecursosCampania++;
  }

  public List<(string texto, Color color)> FinalizarBufferTextosFlotantesCampania()
  {
    if (bloqueoTextosFlotantesCampania > 0)
    {
      bloqueoTextosFlotantesCampania--;
    }

    if (bloqueoTextosRecursosCampania > 0)
    {
      bloqueoTextosRecursosCampania--;
    }

    List<(string texto, Color color)> textosBufferizados = new List<(string texto, Color color)>();
    if (bloqueoTextosFlotantesCampania > 0)
    {
      return textosBufferizados;
    }

    while (colaTextosSuspendidosCampania.Count > 0)
    {
      var (texto, color) = colaTextosSuspendidosCampania.Dequeue();
      textosBufferizados.Add((texto, color));
    }

    return textosBufferizados;
  }

  public GameObject prefabTextoCampaña;
  [SerializeField] GameObject puntoPantalla;

  private void TryProcesarColaTextoFlotante()
  {
    if (!isActiveAndEnabled || procesandoCola || colaTextos.Count == 0)
      return;

    procesandoCola = true;
    rutinaTextoFlotanteCampania = StartCoroutine(ProcesarColaTextoFlotante());
  }

  // Serializa los textos a través de una cola para que no se pisen.
  // Mantiene la firma Task para compatibilidad; retorna completado inmediatamente.
  public Task GenerarTextoFlotanteCampaña(string txString, Color color)
  {
    if (puntoPantalla == null || prefabTextoCampaña == null)
      return Task.CompletedTask;

    if (string.IsNullOrWhiteSpace(txString))
      return Task.CompletedTask;

    txString = FloatingTextAnimator.NormalizarTextoRichText(txString);
    TMP_SpriteAsset spriteAssetRecursos = logDeCampania != null ? logDeCampania.SpriteAssetRecursos : null;
    TextoIconosCombate.NormalizarSpriteAsset(spriteAssetRecursos);
    txString = TextoRecursosCampania.FormatearRecursos(txString, spriteAssetRecursos != null);
    colaTextos.Enqueue((txString, color));
    TryProcesarColaTextoFlotante();

    return Task.CompletedTask;
  }

  private IEnumerator ProcesarColaTextoFlotante()
  {
    while (colaTextos.Count > 0)
    {
      // Respeta separación mínima desde el último spawn
      float pausaEntreMensajes = Mathf.Max(0.5f, gapEntreMensajes);
      float elapsed = Time.unscaledTime - tiempoUltimoSpawnTiempoReal;
      if (elapsed < pausaEntreMensajes)
        yield return new WaitForSecondsRealtime(pausaEntreMensajes - elapsed);

      var (tx, col) = colaTextos.Dequeue();

      if (usarTextoFlotanteManager && TextoFlotanteManager.Instance != null)
      {
        // Delega al manager externo la creación del texto
        TextoFlotanteManager.Instance.GenerarTextoFlotanteConFondo(tx, col);
      }
      else
      {
        // El contenedor conserva el desplazamiento correctivo aunque el Animator
        // del prefab escriba su propia anchoredPosition en cada frame.
        RectTransform contenedor = CrearContenedorTextoFlotanteCampania();
        Transform padre = contenedor != null ? contenedor : puntoPantalla.transform;
        GameObject goTextoFlotante = Instantiate(prefabTextoCampaña, padre, false);
        RectTransform rt = goTextoFlotante.GetComponent<RectTransform>();
        if (rt != null)
        {
          Vector2 posicionTexto = rt.anchoredPosition;
          posicionTexto.x = -Mathf.Abs(posicionTexto.x);
          rt.anchoredPosition = posicionTexto;
        }

        TextMeshProUGUI txtMesh = goTextoFlotante.GetComponentInChildren<TextMeshProUGUI>();
        FloatingTextBackground fondoTexto = null;
        if (txtMesh != null)
        {
          TMP_SpriteAsset spriteAssetRecursos = logDeCampania != null ? logDeCampania.SpriteAssetRecursos : null;
          if (spriteAssetRecursos != null)
          {
            TextoIconosCombate.NormalizarSpriteAsset(spriteAssetRecursos);
            txtMesh.spriteAsset = spriteAssetRecursos;
          }

          txtMesh.horizontalAlignment = HorizontalAlignmentOptions.Right;
          Vector4 margenTexto = txtMesh.margin;
          txtMesh.margin = new Vector4(margenTexto.z, margenTexto.y, margenTexto.x, margenTexto.w);
          txtMesh.text = tx;
          txtMesh.color = col;
          fondoTexto = FloatingTextBackground.Attach(txtMesh, true);
          txtMesh.ForceMeshUpdate();
        }

        CanvasGroup grupoEntrada = contenedor != null ? contenedor.GetComponent<CanvasGroup>() : null;
        if (grupoEntrada != null)
        {
          fondoTexto?.SetExternalAlpha(0f);
          StartCoroutine(MostrarTextoFlotanteCampaniaSuavemente(grupoEntrada, fondoTexto));
        }

        if (contenedor != null && rt != null)
        {
          float alturaTexto = txtMesh != null
            ? Mathf.Max(1f, txtMesh.textBounds.size.y)
            : Mathf.Max(1f, rt.rect.height);
          LimpiarRegistrosTextosFlotantesCampaniaDestruidos();
          textosFlotantesCampaniaActivos.Add((contenedor, rt, alturaTexto));
          PosicionarNuevoTextoFlotanteCampania();
        }
      }

      tiempoUltimoSpawnTiempoReal = Time.unscaledTime;
    }
    procesandoCola = false;
    rutinaTextoFlotanteCampania = null;
  }

  private RectTransform CrearContenedorTextoFlotanteCampania()
  {
    if (puntoPantalla == null)
    {
      return null;
    }

    GameObject goContenedor = new GameObject("ContenedorTextoFlotanteCampania", typeof(RectTransform), typeof(CanvasGroup));
    RectTransform contenedor = goContenedor.GetComponent<RectTransform>();
    contenedor.SetParent(puntoPantalla.transform, false);
    contenedor.anchorMin = Vector2.zero;
    contenedor.anchorMax = Vector2.one;
    contenedor.offsetMin = Vector2.zero;
    contenedor.offsetMax = Vector2.zero;
    contenedor.localScale = Vector3.one;
    goContenedor.GetComponent<CanvasGroup>().alpha = duracionEntradaTextoFlotante > 0f ? 0f : 1f;
    return contenedor;
  }

  private IEnumerator MostrarTextoFlotanteCampaniaSuavemente(CanvasGroup grupo, FloatingTextBackground fondo)
  {
    float duracion = Mathf.Max(0f, duracionEntradaTextoFlotante);
    if (duracion <= 0f)
    {
      grupo.alpha = 1f;
      fondo?.SetExternalAlpha(1f);
      yield break;
    }

    float transcurrido = 0f;
    while (transcurrido < duracion && grupo != null)
    {
      transcurrido += Time.unscaledDeltaTime;
      float progreso = Mathf.Clamp01(transcurrido / duracion);
      float alpha = Mathf.SmoothStep(0f, 1f, progreso);
      grupo.alpha = alpha;
      fondo?.SetExternalAlpha(alpha);
      yield return null;
    }

    if (grupo != null)
    {
      grupo.alpha = 1f;
    }
    fondo?.SetExternalAlpha(1f);
  }

  private void LimpiarRegistrosTextosFlotantesCampaniaDestruidos()
  {
    for (int i = textosFlotantesCampaniaActivos.Count - 1; i >= 0; i--)
    {
      var registro = textosFlotantesCampaniaActivos[i];
      if (registro.contenedor != null && registro.rectTexto != null)
      {
        continue;
      }

      if (registro.contenedor != null)
      {
        Destroy(registro.contenedor.gameObject);
      }
      textosFlotantesCampaniaActivos.RemoveAt(i);
    }
  }

  private void PosicionarNuevoTextoFlotanteCampania()
  {
    int indiceNuevo = textosFlotantesCampaniaActivos.Count - 1;
    if (indiceNuevo < 0)
    {
      return;
    }

    var registroNuevo = textosFlotantesCampaniaActivos[indiceNuevo];
    float desplazamientoFijo = 0f;
    for (int anterior = 0; anterior < indiceNuevo; anterior++)
    {
      var registroAnterior = textosFlotantesCampaniaActivos[anterior];
      float separacionPorAltura = (registroAnterior.altura + registroNuevo.altura) * 0.5f + 6f;
      float separacionMinima = Mathf.Max(yStackOffset * 0.6f, separacionPorAltura);
      float desplazamientoDebajo = registroAnterior.contenedor.anchoredPosition.y - separacionMinima;
      desplazamientoFijo = Mathf.Min(desplazamientoFijo, desplazamientoDebajo);
    }

    Vector2 posicionContenedor = registroNuevo.contenedor.anchoredPosition;
    posicionContenedor.y = desplazamientoFijo;
    registroNuevo.contenedor.anchoredPosition = posicionContenedor;
  }

  private void LimpiarTextosFlotantesCampaniaActivos()
  {
    for (int i = 0; i < textosFlotantesCampaniaActivos.Count; i++)
    {
      RectTransform contenedor = textosFlotantesCampaniaActivos[i].contenedor;
      if (contenedor != null)
      {
        Destroy(contenedor.gameObject);
      }
    }
    textosFlotantesCampaniaActivos.Clear();
  }

  public void BorrarLog()
  {
    txtLog.text = "Log vacío.";
  }



  #endregion


  #region Crear Personajes


  public void CrearCaballero()
  {

    GameObject caballero = Instantiate(prefabGOPersonaje);

    Personaje pers1 = caballero.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreHombreAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 1;
    pers1.idRetrato = 1;
    pers1.iPuestoDeseado = 3;

    pers1.fVidaMaxima = 45 + UnityEngine.Random.Range(1, 7);
    pers1.fVidaActual = pers1.fVidaMaxima;

    pers1.iFuerza = 3 + UnityEngine.Random.Range(0, 2);
    pers1.iAgi = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 0 + UnityEngine.Random.Range(0, 1);
    pers1.InicializarEscaladoResElementalPorPoderSiHaceFalta();

    pers1.iDefensa = 13;
    pers1.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    pers1.iArmadura = 0; //lo da la armadura
    pers1.iApMax = 4;
    pers1.iValMax = 5;
    pers1.iIniciativa = 2 + UnityEngine.Random.Range(0, 2); ;

    pers1.iTSFortaleza = 4 + UnityEngine.Random.Range(0, 2);
    pers1.iTSReflejo = 1 + UnityEngine.Random.Range(0, 2);
    pers1.iTSMental = 3 + UnityEngine.Random.Range(0, 2);

    pers1.iResAcido = 0;
    pers1.iResArcano = 0;
    pers1.iResFuego = 0;
    pers1.iResHielo = 0;
    pers1.iResNecro = 0;
    pers1.iResRayo = 0;
    pers1.iResDivino = 0;


    SortearRasgos(pers1); //Método vacío!!

    //Habilidades Intrinsecas
    //pers1.AddComponent<REPRESENTACIONArmaduraLimitante>(); //AGREGADA POR ARMADURA
    pers1.AddComponent<REPRESENTACIONCorajeInquebrantable>();
    pers1.GetComponent<REPRESENTACIONCorajeInquebrantable>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel
                                                                         //pers1.AddComponent<Cortevertical>(); AGREGADO POR MANDOBLE
                                                                         //Habilidades Base



    
     


    if (!scTutorialManager.tutorialActivo)
    {
      int randHabPot1 = UnityEngine.Random.Range(1, 5);
      switch (randHabPot1)
      {
        case 1: pers1.Habilidad_1 = 1; pers1.AddComponent<REPRESENTACIONAcorazado>(); pers1.GetComponent<REPRESENTACIONAcorazado>().NIVEL = 1; break;  //Acorazado
        case 2: pers1.Habilidad_2 = 1; pers1.AddComponent<GritoMotivador>(); pers1.GetComponent<GritoMotivador>().NIVEL = 1; break; //Grito Motivador
        case 3: pers1.Habilidad_3 = 1; pers1.AddComponent<CorteHorizontal>(); pers1.GetComponent<CorteHorizontal>().NIVEL = 1; break; //Corte Horizontal
        case 4: pers1.Habilidad_4 = 1; pers1.AddComponent<PrimerosAuxilios>(); pers1.GetComponent<PrimerosAuxilios>().NIVEL = 1; break; //Primeros Auxilios
      }
      int randHabPot2 = UnityEngine.Random.Range(1, 5);
      switch (randHabPot2)
      {
        case 1: pers1.Habilidad_5 = 1; pers1.AddComponent<REPRESENTACIONDeterminacion>(); pers1.GetComponent<REPRESENTACIONDeterminacion>().NIVEL = 1; break;  //Acorazado
        case 2: pers1.Habilidad_6 = 1; pers1.AddComponent<Partir>(); pers1.GetComponent<Partir>().NIVEL = 1; break;//Grito Motivador
        case 3: pers1.Habilidad_7 = 1; pers1.AddComponent<PosturaDefensiva>(); pers1.GetComponent<PosturaDefensiva>().NIVEL = 1; break; //Corte Horizontal
        case 4: pers1.Habilidad_8 = 1; pers1.AddComponent<SiguesTu>(); pers1.GetComponent<SiguesTu>().NIVEL = 1; break; //Primeros Auxilios
      }
    }
    else
    {
      pers1.AddComponent<GritoMotivador>(); pers1.GetComponent<GritoMotivador>().NIVEL = 1;

    }

    pers1.AplicarTraitExpertoInicial();
    pers1.ActividadSeleccionada = 3;
    pers1.AddComponent<Actividad_Descansar>();
    pers1.AddComponent<Actividad_Entrenar>();
    pers1.AddComponent<Actividad_Guardia>();

    int randAct = UnityEngine.Random.Range(1, 4); //de las 3 de clase, nacen con 2
    switch (randAct)
    {
      case 1: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_RelatosDeBatalla>(); pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_MantenerArmadura>(); break;
      case 2: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_RelatosDeBatalla>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_Vigilar>(); break;
      case 3: pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_MantenerArmadura>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_Vigilar>(); break;

    }


    pers1.itemArma = Instantiate(scContprefab.armaMandoble);
    pers1.itemArmadura = Instantiate(scContprefab.Coraza);
    AplicarTraitHerenciaInicialSiCorresponde(pers1);

    SincronizarAparienciaVisualPersonaje(pers1);

    scMenuPersonajes.listaPersonajes.Add(pers1);

    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);




  }
  public void CrearExplorador()
  {

    GameObject explorador = Instantiate(prefabGOPersonaje);

    Personaje pers1 = explorador.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreHombreAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 2;
    pers1.idRetrato = 5;
    pers1.iPuestoDeseado = 1;

    pers1.fVidaMaxima = 38 + UnityEngine.Random.Range(1, 5);
    pers1.fVidaActual = pers1.fVidaMaxima;

    pers1.iFuerza = 3 + UnityEngine.Random.Range(0, 2);
    pers1.iAgi = 5 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 1 + UnityEngine.Random.Range(0, 1);
    pers1.InicializarEscaladoResElementalPorPoderSiHaceFalta();

    pers1.iDefensa = 13;
    pers1.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    pers1.iArmadura = 0; //lo da la armadura
    pers1.iApMax = 4; //
    pers1.iValMax = 3;
    pers1.iIniciativa = 5 + UnityEngine.Random.Range(0, 2); ;

    pers1.iTSFortaleza = 3 + UnityEngine.Random.Range(0, 2);
    pers1.iTSReflejo = 5 + UnityEngine.Random.Range(0, 2);
    pers1.iTSMental = 2 + UnityEngine.Random.Range(0, 2);

    pers1.iResAcido = 0;
    pers1.iResArcano = 0;
    pers1.iResFuego = 0;
    pers1.iResHielo = 0;
    pers1.iResNecro = 0;
    pers1.iResRayo = 0;
    pers1.iResDivino = 0;




    SortearRasgos(pers1); //Método vacío!!


    //Habilidades Intrinsecas
    pers1.AddComponent<REPRESENTACIONPasoCauteloso>();
    pers1.GetComponent<REPRESENTACIONPasoCauteloso>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel
    pers1.AddComponent<ImprovisarFlechas>(); //Esta es intrinseca
    pers1.GetComponent<ImprovisarFlechas>().NIVEL = 1;
    pers1.AddComponent<CorteDaga>(); //La daga no es item



    //Habilidades Base
    if (!scTutorialManager.tutorialActivo)
    {
      int randHabPot1 = UnityEngine.Random.Range(1, 5);

      switch (randHabPot1)
      {
        case 1: pers1.Habilidad_1 = 1; pers1.AddComponent<REPRESENTACIONVistaLejana>(); pers1.GetComponent<REPRESENTACIONVistaLejana>().NIVEL = 1; break;
        case 2: pers1.Habilidad_2 = 1; pers1.AddComponent<DisparoPotente>(); pers1.GetComponent<DisparoPotente>().NIVEL = 1; break;
        case 3: pers1.Habilidad_3 = 1; pers1.AddComponent<REPRESENTACIONAcrobatico>(); pers1.GetComponent<REPRESENTACIONAcrobatico>().NIVEL = 1; break;
        case 4: pers1.Habilidad_4 = 1; pers1.AddComponent<MarcarPresa>(); pers1.GetComponent<MarcarPresa>().NIVEL = 1; break;
      }
      int randHabPot2 = UnityEngine.Random.Range(1, 4);
      switch (randHabPot2)
      {
        case 1: pers1.Habilidad_5 = 1; pers1.AddComponent<Acechar>(); pers1.GetComponent<Acechar>().NIVEL = 1; break;
        case 2: pers1.Habilidad_6 = 1; pers1.AddComponent<Vigilancia>(); pers1.GetComponent<Vigilancia>().NIVEL = 1; break;
        case 3: pers1.Habilidad_7 = 1; pers1.AddComponent<Fogata>(); pers1.GetComponent<Fogata>().NIVEL = 1; break;
      }
    }
    else
    { 

      pers1.AddComponent<MarcarPresa>(); pers1.GetComponent<MarcarPresa>().NIVEL = 1;
      
    }
    
    
    pers1.AplicarTraitExpertoInicial();
    pers1.ActividadSeleccionada = 3;
    pers1.AddComponent<Actividad_Descansar>();
    pers1.AddComponent<Actividad_Entrenar>();
    pers1.AddComponent<Actividad_Guardia>();


    int randAct = UnityEngine.Random.Range(1, 4); //de las 3 de clase, nacen con 2
    switch (randAct)
    {
      case 1: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_CazaNocturna>(); pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_Exploracion>(); break;
      case 2: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_CazaNocturna>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_PrepararFlechas>(); break;
      case 3: pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_Exploracion>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_PrepararFlechas>(); break;

    }






    pers1.itemArma = Instantiate(scContprefab.armaArcoLargo);
    pers1.itemArmadura = Instantiate(scContprefab.ArmaduraCuero);
    AplicarTraitHerenciaInicialSiCorresponde(pers1);
    SincronizarAparienciaVisualPersonaje(pers1);
    scMenuPersonajes.listaPersonajes.Add(pers1);
    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);


  }
  public void CrearPurificadora()
  {

    GameObject purificadora = Instantiate(prefabGOPersonaje);

    Personaje pers1 = purificadora.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreMujerAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 3;
    pers1.idRetrato = 6;
    pers1.iPuestoDeseado = 1;

    pers1.fVidaMaxima = 29 + UnityEngine.Random.Range(1, 5);
    pers1.fVidaActual = pers1.fVidaMaxima;

    pers1.iFuerza = 1 + UnityEngine.Random.Range(0, 2);
    pers1.iAgi = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 4 + UnityEngine.Random.Range(0, 2);
    pers1.InicializarEscaladoResElementalPorPoderSiHaceFalta();

    pers1.iDefensa = 11;
    pers1.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    pers1.iArmadura = 0; //lo da la armadura
    pers1.iApMax = 4; //
    pers1.iValMax = 5;
    pers1.iIniciativa = 2 + UnityEngine.Random.Range(0, 2); ;

    pers1.iTSFortaleza = 3 + UnityEngine.Random.Range(0, 2);
    pers1.iTSReflejo = 1 + UnityEngine.Random.Range(0, 2);
    pers1.iTSMental = 6 + UnityEngine.Random.Range(0, 2);

    pers1.iResAcido = 0;
    pers1.iResArcano = 0;
    pers1.iResFuego = 0;
    pers1.iResHielo = 0;
    pers1.iResNecro = -5;
    pers1.iResRayo = 0;
    pers1.iResDivino = 5;




    SortearRasgos(pers1); //Método vacío!!

 

    //Habilidades Intrinsecas
    pers1.AddComponent<REPRESENTACIONAlmaEndeble>();
    pers1.GetComponent<REPRESENTACIONAlmaEndeble>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel
    pers1.AddComponent<REPRESENTACIONFervorConjunto>();
    pers1.GetComponent<REPRESENTACIONFervorConjunto>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel

    //Habilidades Base
    int randHabPot1 = UnityEngine.Random.Range(1, 5);
    switch (randHabPot1)
    {
      case 1: pers1.Habilidad_1 = 1; pers1.AddComponent<REPRESENTACIONAuraSagrada>(); pers1.GetComponent<REPRESENTACIONAuraSagrada>().NIVEL = 1; break;
      case 2: pers1.Habilidad_2 = 1; pers1.AddComponent<REPRESENTACIONEcosDivinos>(); pers1.GetComponent<REPRESENTACIONEcosDivinos>().NIVEL = 1; break;
      case 3: pers1.Habilidad_3 = 1; pers1.AddComponent<SalmoPurificador>(); pers1.GetComponent<SalmoPurificador>().NIVEL = 1; break;
      case 4: pers1.Habilidad_4 = 1; pers1.AddComponent<LlamaDivina>(); pers1.GetComponent<LlamaDivina>().NIVEL = 1; break;
    }

    int randHabPot2 = UnityEngine.Random.Range(1, 5);
    switch (randHabPot2)
    {
      case 1: pers1.Habilidad_5 = 1; pers1.AddComponent<Enmendar>(); pers1.GetComponent<Enmendar>().NIVEL = 1; break;
      case 2: pers1.Habilidad_6 = 1; pers1.AddComponent<LuzCegadora>(); pers1.GetComponent<LuzCegadora>().NIVEL = 1; break;
      case 3: pers1.Habilidad_7 = 1; pers1.AddComponent<PilaresDeLuz>(); pers1.GetComponent<PilaresDeLuz>().NIVEL = 1; break;
      case 4: pers1.Habilidad_8 = 1; pers1.AddComponent<CastigaraLosMalvados>(); pers1.GetComponent<CastigaraLosMalvados>().NIVEL = 1; break;
    }


    pers1.AplicarTraitExpertoInicial();

    pers1.ActividadSeleccionada = 3;
    pers1.AddComponent<Actividad_Descansar>();
    pers1.AddComponent<Actividad_Entrenar>();
    pers1.AddComponent<Actividad_Guardia>();



    int randAct = UnityEngine.Random.Range(1, 4); //de las 3 de clase, nacen con 2
    switch (randAct)
    {
      case 1: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_RitualDeLimpieza>(); pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_ColaborarConLosCuranderos>(); break;
      case 2: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_RitualDeLimpieza>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_AyudarDesamparados>(); break;
      case 3: pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_ColaborarConLosCuranderos>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_AyudarDesamparados>(); break;

    }






    pers1.itemArma = Instantiate(scContprefab.armaBaculoPurificador);
    AplicarTraitHerenciaInicialSiCorresponde(pers1);
    SincronizarAparienciaVisualPersonaje(pers1);
    scMenuPersonajes.listaPersonajes.Add(pers1);
    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);



  }
  public void CrearAcechador(bool configuracionTutorial = false)
  {

    GameObject acechador = Instantiate(prefabGOPersonaje);

    Personaje pers1 = acechador.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreHombreAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 4;
    pers1.idRetrato = 7;
    pers1.iPuestoDeseado = 2;

    pers1.fVidaMaxima = 41 + UnityEngine.Random.Range(1, 6);
    pers1.fVidaActual = pers1.fVidaMaxima;

    pers1.iFuerza = 4 + UnityEngine.Random.Range(0, 2);
    pers1.iAgi = 5 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 2 + UnityEngine.Random.Range(0, 2);
    pers1.InicializarEscaladoResElementalPorPoderSiHaceFalta();

    pers1.iDefensa = 13;
    pers1.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    pers1.iArmadura = 0; //lo da la armadura
    pers1.iApMax = 4;
    pers1.iValMax = 2;
    pers1.iIniciativa = 6 + UnityEngine.Random.Range(0, 3); ;

    pers1.iTSFortaleza = 3 + UnityEngine.Random.Range(0, 2);
    pers1.iTSReflejo = 5 + UnityEngine.Random.Range(0, 2);
    pers1.iTSMental = 1 + UnityEngine.Random.Range(0, 2);

    pers1.iResAcido = 0;
    pers1.iResArcano = 0;
    pers1.iResFuego = 0;
    pers1.iResHielo = 0;
    pers1.iResNecro = 0;
    pers1.iResRayo = 0;
    pers1.iResDivino = 0;




    SortearRasgos(pers1); //Método vacío!!



   
    //Habilidades Intrinsecas
    pers1.AddComponent<REPRESENTACIONSueldo>(); pers1.GetComponent<REPRESENTACIONSueldo>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel
    pers1.AddComponent<REPRESENTACIONSigiloso>(); pers1.GetComponent<REPRESENTACIONSigiloso>().NIVEL = -1;
    pers1.AddComponent<TiroBallestaDeMano>(); pers1.GetComponent<TiroBallestaDeMano>().NIVEL = -1;


    //Habilidades Base
    bool usarHabilidadesTutorial = configuracionTutorial || (scTutorialManager != null && scTutorialManager.tutorialActivo);
    if (!usarHabilidadesTutorial)
    {
      int randHabPot1 = UnityEngine.Random.Range(1, 3);
      switch (randHabPot1)
      {
        case 1: pers1.Habilidad_1 = 1; pers1.AddComponent<REPRESENTACIONMaestriaBallesta>(); pers1.GetComponent<REPRESENTACIONMaestriaBallesta>().NIVEL = 1; break;
        case 2: pers1.Habilidad_2 = 1; pers1.AddComponent<REPRESENTACIONMaestriaEspadaCorta>(); pers1.GetComponent<REPRESENTACIONMaestriaEspadaCorta>().NIVEL = 1; break;
      }



      int randHabPot2 = UnityEngine.Random.Range(1, 7);
      switch (randHabPot2)
      {
        case 1: pers1.Habilidad_3 = 1; pers1.AddComponent<DisparoEnvenenado>(); pers1.GetComponent<DisparoEnvenenado>().NIVEL = 1; break;
        case 2: pers1.Habilidad_4 = 1; pers1.AddComponent<CorteIncapacitante>(); pers1.GetComponent<CorteIncapacitante>().NIVEL = 1; break;
        case 3: pers1.Habilidad_5 = 1; pers1.AddComponent<BombaDeHumo>(); pers1.GetComponent<BombaDeHumo>().NIVEL = 1; break;
        case 4: pers1.Habilidad_6 = 1; pers1.AddComponent<Asesinar>(); pers1.GetComponent<Asesinar>().NIVEL = 1; break;
        case 5: pers1.Habilidad_7 = 1; pers1.AddComponent<Distraer>(); pers1.GetComponent<Distraer>().NIVEL = 1; break;
        case 6: pers1.Habilidad_8 = 1; pers1.AddComponent<ArrojarAbrojos>(); pers1.GetComponent<ArrojarAbrojos>().NIVEL = 1; break;

      }
    }
    else
    {
      pers1.Habilidad_2 = 1;
      pers1.AddComponent<REPRESENTACIONMaestriaEspadaCorta>();
      pers1.GetComponent<REPRESENTACIONMaestriaEspadaCorta>().NIVEL = 1;

      pers1.Habilidad_7 = 1;
      pers1.AddComponent<Distraer>();
      pers1.GetComponent<Distraer>().NIVEL = 1;
    }

    pers1.AplicarTraitExpertoInicial();
    //Habilidades de Actividad
    pers1.ActividadSeleccionada = 3;
    pers1.AddComponent<Actividad_Descansar>();
    pers1.AddComponent<Actividad_Entrenar>();
    pers1.AddComponent<Actividad_Guardia>();


    int randAct = UnityEngine.Random.Range(1, 4); //de las 3 de clase, nacen con 2
    switch (randAct)
    {
      case 1: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_AfilarArmas>(); pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_VigilarDesdeLasSombras>(); break;
      case 2: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_AfilarArmas>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_Coercion>(); break;
      case 3: pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_VigilarDesdeLasSombras>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_Coercion>(); break;

    }


    pers1.itemArma = Instantiate(scContprefab.armaEspadaCorta);
    pers1.itemArmadura = Instantiate(scContprefab.ArmaduraCueroReforzado);
    AplicarTraitHerenciaInicialSiCorresponde(pers1);
    SincronizarAparienciaVisualPersonaje(pers1);
    scMenuPersonajes.listaPersonajes.Add(pers1);
    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);




  }
  public void CrearCanalizador()
  {

    GameObject canalizador = Instantiate(prefabGOPersonaje);

    Personaje pers1 = canalizador.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreHombreAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 5;
    pers1.idRetrato = 8;
    pers1.iPuestoDeseado = 2;

    pers1.fVidaMaxima = 33 + UnityEngine.Random.Range(1, 4);
    pers1.fVidaActual = pers1.fVidaMaxima;

    pers1.iFuerza = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iAgi = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 4 + UnityEngine.Random.Range(0, 2);
    pers1.InicializarEscaladoResElementalPorPoderSiHaceFalta();

    pers1.iDefensa = 11;
    pers1.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    pers1.iArmadura = 0; //lo da la armadura
    pers1.iApMax = 4;
    pers1.iValMax = 3;
    pers1.iIniciativa = 3 + UnityEngine.Random.Range(0, 2);

    pers1.iTSFortaleza = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iTSReflejo = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iTSMental = 4 + UnityEngine.Random.Range(0, 2);

    pers1.iResAcido = 0;
    pers1.iResArcano = 0;
    pers1.iResFuego = 0;
    pers1.iResHielo = 0;
    pers1.iResNecro = 0;
    pers1.iResRayo = 0;
    pers1.iResDivino = 0;




    SortearRasgos(pers1); //Método vacío!!

    //Intrinsecas 
    pers1.AddComponent<REPRESENTACIONSobrecarga>();  pers1.GetComponent<REPRESENTACIONSobrecarga>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel
    pers1.AddComponent<AcumularEnergia>();  pers1.GetComponent<AcumularEnergia>().NIVEL = -1;
    pers1.AddComponent<DescargaArcana>();  pers1.GetComponent<DescargaArcana>().NIVEL = -1;



    //Habilidades Base
    int randHabPot1 = UnityEngine.Random.Range(1, 3);
    switch (randHabPot1)
    {
      case 1: pers1.Habilidad_1 = 1; pers1.AddComponent<REPRESENTACIONAcumulacionProtegida>(); pers1.GetComponent<REPRESENTACIONAcumulacionProtegida>().NIVEL = 1; break;
      case 2: pers1.Habilidad_2 = 1; pers1.AddComponent<REPRESENTACIONExcesoDePoder>(); pers1.GetComponent<REPRESENTACIONExcesoDePoder>().NIVEL = 1; break;
    }

    int randHabPot2 = UnityEngine.Random.Range(1, 7);
    switch (randHabPot2)
    {
      case 1: pers1.Habilidad_3 = 1; pers1.AddComponent<DescargaDePoder>(); pers1.GetComponent<DescargaDePoder>().NIVEL = 1; break;
      case 2: pers1.Habilidad_4 = 1; pers1.AddComponent<Instatransporte>(); pers1.GetComponent<Instatransporte>().NIVEL = 1; break;
      case 3: pers1.Habilidad_5 = 1; pers1.AddComponent<AcumulacionInestable>(); pers1.GetComponent<AcumulacionInestable>().NIVEL = 1; break;
      case 4: pers1.Habilidad_6 = 1; pers1.AddComponent<HojaDeEnergia>(); pers1.GetComponent<HojaDeEnergia>().NIVEL = 1; break;
      case 5: pers1.Habilidad_7 = 1; pers1.AddComponent<EscudoEnergetico>(); pers1.GetComponent<EscudoEnergetico>().NIVEL = 1; break;
      case 6: pers1.Habilidad_8 = 1; pers1.AddComponent<SifonArcano>(); pers1.GetComponent<SifonArcano>().NIVEL = 1; break;

    }

    pers1.AplicarTraitExpertoInicial();
    //Habilidades de Actividad
    pers1.ActividadSeleccionada = 3;
    pers1.AddComponent<Actividad_Descansar>();
    pers1.AddComponent<Actividad_Entrenar>();
    pers1.AddComponent<Actividad_Guardia>();


    int randAct = UnityEngine.Random.Range(1, 4); //de las 3 de clase, nacen con 2
    switch (randAct)
    {
      case 1: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_ConcentracionArcana>(); pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_Telekinesis>(); break;
      case 2: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_ConcentracionArcana>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_CrearSimboloArcanoProteccion>(); break;
      case 3: pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_Telekinesis>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_CrearSimboloArcanoProteccion>(); break;

    }
    AplicarTraitHerenciaInicialSiCorresponde(pers1);
    SincronizarAparienciaVisualPersonaje(pers1);
    scMenuPersonajes.listaPersonajes.Add(pers1);
    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);

  }

  public void CrearDuelista()
  {

    GameObject caballero = Instantiate(prefabGOPersonaje);

    Personaje pers1 = caballero.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreMujerAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 6;
    pers1.idRetrato = 9;
    pers1.iPuestoDeseado = 3;

    pers1.fVidaMaxima = 41 + UnityEngine.Random.Range(1, 6);
    pers1.fVidaActual = pers1.fVidaMaxima;

    pers1.iFuerza = 3 + UnityEngine.Random.Range(0, 2);
    pers1.iAgi = 4 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 0 + UnityEngine.Random.Range(0, 1);
    pers1.InicializarEscaladoResElementalPorPoderSiHaceFalta();

    pers1.iDefensa = 14;
    pers1.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    pers1.iArmadura = 0; //lo da la armadura
    pers1.iApMax = 4;
    pers1.iValMax = 3;
    pers1.iIniciativa = 3 + UnityEngine.Random.Range(0, 2); ;

    pers1.iTSFortaleza = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iTSReflejo = 5 + UnityEngine.Random.Range(0, 2);
    pers1.iTSMental = 4 + UnityEngine.Random.Range(0, 2);

    pers1.iResAcido = 0;
    pers1.iResArcano = 0;
    pers1.iResFuego = 0;
    pers1.iResHielo = 0;
    pers1.iResNecro = 0;
    pers1.iResRayo = 0;
    pers1.iResDivino = 0;


    SortearRasgos(pers1); //Método vacío!!

   

     //Habilidades Intrinsecas
    pers1.AddComponent<REPRESENTACIONPasoLigero>();
    pers1.GetComponent<REPRESENTACIONPasoLigero>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel
    pers1.AddComponent<REPRESENTACIONPosturaDemandante>();
    pers1.GetComponent<REPRESENTACIONPosturaDemandante>().NIVEL = -1; //Pasiva   -1 porque es intrinseca, no sube de nivel
   
    //Habilidades Base
    int randHabPot1 = UnityEngine.Random.Range(1, 5);
    switch (randHabPot1)
    {
      case 1: pers1.Habilidad_2 = 1; pers1.AddComponent<REPRESENTACIONEvasionMaestra>(); pers1.GetComponent<REPRESENTACIONEvasionMaestra>().NIVEL = 1; break;
      case 2: pers1.Habilidad_3 = 1; pers1.AddComponent<CargaDeEstoque>(); pers1.GetComponent<CargaDeEstoque>().NIVEL = 1; break;
      case 3: pers1.Habilidad_4 = 1; pers1.AddComponent<Riposte>(); pers1.GetComponent<Riposte>().NIVEL = 1; break;
      case 4: pers1.Habilidad_5 = 1; pers1.AddComponent<AFondo>(); pers1.GetComponent<AFondo>().NIVEL = 1; break;
     
     
    }

    int randHabPot2 = UnityEngine.Random.Range(1, 5);
    switch (randHabPot2)
    {
      case 1: pers1.Habilidad_1 = 1; pers1.AddComponent<REPRESENTACIONAtaquesReveladores>(); pers1.GetComponent<REPRESENTACIONAtaquesReveladores>().NIVEL = 1; break;
      case 2: pers1.Habilidad_7 = 1; pers1.AddComponent<PuntaHiriente>(); pers1.GetComponent<PuntaHiriente>().NIVEL = 1; break;
      case 3: pers1.Habilidad_6 = 1; pers1.AddComponent<EnGarde>(); pers1.GetComponent<EnGarde>().NIVEL = 1; break;
      case 4: pers1.Habilidad_8 = 1; pers1.AddComponent<RecuperarAire>(); pers1.GetComponent<RecuperarAire>().NIVEL = 1; break;
     
    }


    pers1.AplicarTraitExpertoInicial();
    //Habilidades de Actividad
    pers1.ActividadSeleccionada = 3;
    pers1.AddComponent<Actividad_Descansar>();
    pers1.AddComponent<Actividad_Entrenar>();
    pers1.AddComponent<Actividad_Guardia>();

    int randAct = UnityEngine.Random.Range(1, 4); //de las 3 de clase, nacen con 2
    switch (randAct)
    {
      case 1: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_SiempreAlerta>(); pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_Socializar>(); break;
      case 2: pers1.Actividad_1 = 1; pers1.AddComponent<Actividad_SiempreAlerta>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_Consuelo>(); break;
      case 3: pers1.Actividad_2 = 1; pers1.AddComponent<Actividad_Socializar>(); pers1.Actividad_3 = 1; pers1.AddComponent<Actividad_Consuelo>(); break;

    }


    pers1.itemArma = Instantiate(scContprefab.armaEstoque);
    pers1.itemArmadura = Instantiate(scContprefab.ArmaduraGambeson);
    AplicarTraitHerenciaInicialSiCorresponde(pers1);
    SincronizarAparienciaVisualPersonaje(pers1);

    scMenuPersonajes.listaPersonajes.Add(pers1);

    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);




  }























































  private List<string> nombresHombreDisponibles = new List<string>
  {
    "Jonan", "Claude", "Riller", "Castallion", "Mark", "Pirrik", "Mance", "Avain", "Segrin", "Ballag",
    "Eldric", "Tharion", "Lucan", "Darian", "Garrick", "Bram", "Cedric", "Ulric", "Leoric", "Torin", "Aldric",
    "Bastian", "Cyrus", "Dorian", "Eamon", "Finnian", "Gideon", "Hector", "Isidore", "Jareth", "Kieran",
    "Varric", "Roland", "Baldric", "Edric", "Galen", "Harlan", "Jorund", "Kael", "Luther", "Magnus","Basel", "Nolan", "Orin", "Perrin", "Quentin", "Roderic", "Soren", "Tobias", "Uther", "Viktor",
  };

  string CrearNombreHombreAzar()
  {
    if (nombresHombreDisponibles.Count == 0)
    {
      // Si se acaban los nombres, puedes lanzar una excepción o regenerar la lista si lo prefieres
      throw new InvalidOperationException("No hay más nombres de hombre disponibles.");
    }
    int index = UnityEngine.Random.Range(0, nombresHombreDisponibles.Count);
    string name = nombresHombreDisponibles[index];
    nombresHombreDisponibles.RemoveAt(index);
    return name;
  }

  private List<string> nombresMujerDisponibles = new List<string>
  {
    "Maguie", "Bellezia", "Ava", "Lira", "Joakia", "Sanna", "Robin", "Prisia", "Gillia", "Cadia","Zafira", "Elara", "Fiorella", "Lyra", "Nerina", "Selene", "Thalia", "Vespera", "Ysolde", "Zinnia",
    "Althea", "Briony", "Cressida", "Dahlia", "Elysia", "Fiora", "Ginevra", "Helena", "Isolde", "Jasmine","Kassandra", "Lysandra", "Mirabel", "Nerissa", "Ophelia", "Persephone", "Quintessa", "Rowena", "Seraphina", "Tamsin", "Ursula",
    "Vespera", "Wisteria", "Xanthe", "Yara", "Zara", "Ariadne", "Brielle", "Celestia", "Daphne", "Evangeline",
  };
  string CrearNombreMujerAzar()
  {
    if (nombresMujerDisponibles.Count == 0)
    {
      throw new InvalidOperationException("No hay más nombres de mujer disponibles.");
    }
    int index = UnityEngine.Random.Range(0, nombresMujerDisponibles.Count);
    string name = nombresMujerDisponibles[index];
    nombresMujerDisponibles.RemoveAt(index);
    return name;
  }

  void SortearRasgos(Personaje pers)
  {
    if (pers == null)
    {
      return;
    }

    pers.LimpiarRasgos();
    bool esLiderInicial = creandoLiderInicial;
    if (esLiderInicial)
    {
      pers.AgregarRasgo(PersonajeTraitCatalog.TraitLiderCaravana);
    }

    List<PersonajeTraitDefinition> disponibles = PersonajeTraitCatalog.ObtenerTraitsDisponiblesAlCrear();
    if (disponibles.Count == 0)
    {
      return;
    }

    int cantidadMinima = esLiderInicial ? 0 : 1;
    int cantidadMaxima = Mathf.Min(esLiderInicial ? 2 : 3, disponibles.Count);
    int cantidadARollear = UnityEngine.Random.Range(cantidadMinima, cantidadMaxima + 1);
    for (int i = 0; i < cantidadARollear; i++)
    {
      List<PersonajeTraitDefinition> compatibles = new List<PersonajeTraitDefinition>();
      for (int j = 0; j < disponibles.Count; j++)
      {
        PersonajeTraitDefinition candidata = disponibles[j];
        if (candidata == null)
        {
          continue;
        }

        bool compatible = true;
        foreach (int rasgoActivo in pers.EnumerarRasgosActivos())
        {
          if (!PersonajeTraitCatalog.SonCompatibles(candidata.Id, rasgoActivo))
          {
            compatible = false;
            break;
          }
        }

        if (compatible)
        {
          compatibles.Add(candidata);
        }
      }

      if (compatibles.Count == 0)
      {
        break;
      }

      int index = UnityEngine.Random.Range(0, compatibles.Count);
      PersonajeTraitDefinition definicion = compatibles[index];
      disponibles.Remove(definicion);

      if (definicion != null)
      {
        pers.AgregarRasgo(definicion.Id);
      }
    }

    if (!pers.PuedeRealizarActividades())
    {
      pers.ActividadSeleccionada = 1;
    }
    AplicarTraitHeroeLocalInicialSiCorresponde(pers);
  }




  #endregion

  public void AplicarRecompensaPrimerCombateTutorial()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    Personaje acechador = scMenuPersonajes.listaPersonajes.Find(p => p != null && !p.Camp_Muerto && p.IDClase == 4);
    OtorgarExperienciaHastaNivel(acechador, 2);

    if (CuantosPersonajesSonDeTalClase(1) == 0)
    {
      AgregarHeroe(1);
    }

    if (CuantosPersonajesSonDeTalClase(3) == 0)
    {
      AgregarHeroe(3);
    }

    RefrescarRetratosPersonajesCampania(true);
  }

  private void OtorgarExperienciaHastaNivel(Personaje personaje, int nivelObjetivo)
  {
    if (personaje == null)
    {
      return;
    }

    int intentos = 0;
    while (personaje.fNivelActual < nivelObjetivo && intentos < 5)
    {
      float experienciaFaltante = Mathf.Max(1f, personaje.ObtenerExperienciaNecesariaParaProximoNivel() - personaje.fExperienciaActual);
      personaje.RecibirExperiencia(experienciaFaltante + 1f, false);
      intentos++;
    }
  }

  public bool AgregarHeroe(int n)
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      Debug.LogWarning("[CampaignManager] No se pudo agregar un personaje porque la lista de personajes no esta disponible.");
      return false;
    }

    if (CuantosPersonajesActivos() >= ObtenerCapacidadMaximaPersonajes())
    {
      string mensajeSinCupo = TRADU.i != null
        ? TRADU.i.Traducir("La caravana no tiene más tiendas para otro personaje.")
        : "La caravana no tiene más tiendas para otro personaje.";
      EscribirAdvertenciaLog("<color=#ff9e9e>" + mensajeSinCupo + "</color>", true);
      TutorialTooltipManager.TryShow(TooltipPersonajeTiendasId);
      return false;
    }

    int cantidadAntes = scMenuPersonajes.listaPersonajes.Count;
    int claseElegida;
    if (n == 0) //Es heroe al azar
    {
      // Pool aleatorio actual: 1-Caballero, 2-Explorador, 3-Purificadora, 4-Acechador, 5-Canalizador, 6-Duelista.
      List<int> clasesFaltantes = new List<int>();
      for (int i = 1; i <= 6; i++)
      {
        if (CuantosPersonajesSonDeTalClase(i) == 0)
          clasesFaltantes.Add(i);
      }

      if (clasesFaltantes.Count > 0)
      {
        claseElegida = clasesFaltantes[UnityEngine.Random.Range(0, clasesFaltantes.Count)];
      }
      else
      {
        claseElegida = UnityEngine.Random.Range(1, 7);
      }
    }
    else //Es heroe de clase elegida
    {
      claseElegida = n;
    }

    bool claseValida = true;
    switch (claseElegida)
    {
      case 1: CrearCaballero(); break;
      case 2: CrearExplorador(); break;
      case 3: CrearPurificadora(); break;
      case 4: CrearAcechador(); break;
      case 5: CrearCanalizador(); break;
      case 6: CrearDuelista(); break;
      default:
        claseValida = false;
        break;
    }

    if (!claseValida)
    {
      return false;
    }

    if (scMenuPersonajes.listaPersonajes.Count <= cantidadAntes)
    {
      Debug.LogWarning("[CampaignManager] No se pudo agregar el personaje solicitado.");
      return false;
    }
    string mensajeNuevoHeroe = TRADU.i != null
      ? TRADU.i.Traducir("-Un nuevo personaje se ha unido a la caravana: ")
      : "-Un nuevo personaje se ha unido a la caravana: ";
    EscribirLog(mensajeNuevoHeroe + scMenuPersonajes.listaPersonajes[scMenuPersonajes.listaPersonajes.Count - 1].sNombre + ".");
    if (!inicializandoNuevaCampania)
    {
      RefrescarRetratosPersonajesCampania(true);
    }
    return true;
  }


  public int ObtenerCantidadPersonajesDisponiblesParaEvento()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return 0;
    }

    return scMenuPersonajes.listaPersonajes.FindAll(p => p != null && !p.Camp_Muerto).Count;
  }

  public Personaje ObtenerPersonajeAleatorio(List<Personaje> excluidos = null, int IDClasePrioritaria = -1)
  {
    if (scMenuPersonajes.listaPersonajes.Count == 0)
    {
      throw new InvalidOperationException("No hay personajes disponibles.");
    }

    List<Personaje> personajesDisponibles = scMenuPersonajes.listaPersonajes.FindAll(p => p != null && !p.Camp_Muerto);

    if (excluidos != null && excluidos.Count > 0)
    {
      personajesDisponibles = personajesDisponibles.FindAll(p => !excluidos.Contains(p));
    }

    // Si hay clase prioritaria, intenta filtrar primero por esa clase
    if (IDClasePrioritaria > 0)
    {
      var prioritarios = personajesDisponibles.FindAll(p => p.IDClase == IDClasePrioritaria);
      if (prioritarios.Count > 0)
      {
        personajesDisponibles = prioritarios;
      }
    }

    if (personajesDisponibles.Count == 0)
    {
      throw new InvalidOperationException("No hay personajes disponibles.");
    }

    float pesoTotal = 0f;
    foreach (Personaje personaje in personajesDisponibles)
    {
      pesoTotal += ObtenerPesoParticipacionEventoTraits(personaje);
    }

    if (pesoTotal <= 0f)
    {
      int indexFallback = UnityEngine.Random.Range(0, personajesDisponibles.Count);
      return personajesDisponibles[indexFallback];
    }

    float tirada = UnityEngine.Random.Range(0f, pesoTotal);
    float acumulado = 0f;
    foreach (Personaje personaje in personajesDisponibles)
    {
      acumulado += ObtenerPesoParticipacionEventoTraits(personaje);
      if (tirada <= acumulado)
      {
        return personaje;
      }
    }

    return personajesDisponibles[personajesDisponibles.Count - 1];
  }

  public void AjustarDificultad()
  {

    int difPlayerPrefs = PlayerPrefs.GetInt("dificultad_index", 2);
    int presetNivel = Sistema.HandicapDificultad.ConvertirIndexPrefsAPresetNivel(difPlayerPrefs);

    var handicap = Sistema.HandicapDificultad.Instance;
    if (handicap != null)
    {
      handicap.AplicarDificultadDesdePlayerPrefs();
    }
    else if (BattleManager.Instance != null)
    {
      BattleManager.Instance.EstablecerDificultadCombate(presetNivel);
    }


  }

  public int peligrozonaanterior;
  public void IncrementarDificultadSegunAlertaRegion(int nivelAlerta)
  {

    var hd = Sistema.HandicapDificultad.Instance;
    if (hd != null)
    {
      hd.puntosExtraEnemigos -= 2 * peligrozonaanterior;

      hd.puntosExtraEnemigos += 2 * nivelAlerta;
    }


    peligrozonaanterior = nivelAlerta;
  }


  public void AplicarEfectosMejorasPuerto()
  {
    //Templo
    int templo = MetaprogresionManager.Instance.SerriaTierTemplo;
    CambiarValorAlientoNegroHoras(-templo * 5f);
    List<Personaje> personajesDisponibles = scMenuPersonajes.listaPersonajes;
    foreach (Personaje personaje in personajesDisponibles)
    {
      if (personaje.IDClase == 3) //Purificadora
      {
        personaje.RecibirExperiencia(100 * templo);
      }
    }
    if (templo > 0)
    { EscribirLog(TRADU.i.Traducir("-Las oraciones de los Purificadores del Templo de Serria reducen el Aliento Negro en: ") + (templo * 5) + " h"); }
    //---
    //Almenaras
    int almenaras = MetaprogresionManager.Instance.SerriaTierAlmenaras;
    if (almenaras > 0)
    {
      TipoEstadoCaravana estadoAlmenaras = AgregarEstadoCaravanaPositivoAleatorio(almenaras);

      if (scAtributosZona.ID != 3) //no en Nedukazal
      {
        string logAlmenaras = ObtenerLogAlmenarasSerria(almenaras, estadoAlmenaras);
        EscribirLog(logAlmenaras);
      }

      CambiarEsperanzaActual(almenaras * 5);
    }
    //---

  }
}


