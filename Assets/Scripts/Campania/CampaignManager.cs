using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using System;
using System.Threading;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CampaignManager : MonoBehaviour
{
  public static CampaignManager Instance { get; private set; }
  private const bool DEBUG_FORZAR_OLA_DE_CALOR_AL_PLAY = false;
  private const bool DEBUG_FORZAR_MASACRE_NEDUKAZAL = false;
  private const bool DEBUG_ABRIR_MENU_SERRIA_AL_INICIAR = false;
  [Header("Debug Demo")]
  [SerializeField] private bool debugSaltarTutorialAlIniciar = false;
  [SerializeField] private bool debugPermitirZonaBosque = false;
  [SerializeField] private bool debugPermitirZonaPasoVientoHelado = false;
  [SerializeField] private bool debugPermitirZonaNedukazal = false;
  [SerializeField] private bool debugIniciarConEstadosCaravana = false;

  public GameObject prefabTextoRecursos;
  public Animator animCaravana;
  public GameObject goCanvas;
  public MapaManager scMapaManager;
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

  // Sonido de la caravana al moverse (asignar desde Inspector)
  public AudioClip sfxMovimientoCaravana;
  [Range(0f, 1f)] public float sfxMovimientoVolumen = 0.8f;
  [Range(0.5f, 1.5f)] public float sfxMovimientoPitch = 0.85f;
  [SerializeField] private float sfxMovimientoFadeIn = 0.35f;
  [SerializeField] private float sfxMovimientoFadeOut = 0.4f;
  private AudioSource sfxMovimientoSource;
  private Coroutine rutinaDesvanecerSfxMovimiento;

  public int sequitoHerrerosMantArmas;
  public int sequitoHerrerosMantArmaduras;
  public int sequitoMercaderesTier;
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

  public int numeroTurno;
  private const string TEXTO_LOG_INICIO_CAMPANIA = "El viaje de la caravana ha comenzado.";
  private bool logInicioCampaniaEscrito;
  public bool MoviendoCaravana = false;
  private bool transicionZonaEnCurso = false;
  private bool bloquearOlaDeCalorEnSiguienteTiradaClima;
  public Nodo nodoDestinoActual;
  private readonly List<int> eventosAleatoriosUsadosMapa = new List<int>();
  private float multiplicadorVelocidadVisualViajeActual = 1f;

  public GameObject prefabGOPersonaje;

  public ContenedorPrefabsCamp scContprefab;

  public int BATALLA_EnCurso;

  public GameObject goSequitos;
  public GameObject goLogCampania;
  public GameObject goDerrota;

  public AdministradorEscenas scAdministradorEscenas;

  // Cola de textos flotantes para evitar solapamientos (mínimo 0.5s entre spawns)
  [SerializeField] private float gapEntreMensajes = 0.5f;
  [SerializeField] private bool usarTextoFlotanteManager = false;
  [SerializeField] private float yStackOffset = 28f;            // desplazamiento vertical entre mensajes simultáneos
  [SerializeField] private float stackWindowSeconds = 1.2f;     // ventana donde consideramos mensajes "cercanos" al origen
  private readonly Queue<(string, Color)> colaTextos = new Queue<(string, Color)>();
  private readonly SemaphoreSlim serializacionTextosRecursos = new SemaphoreSlim(1, 1);
  private bool procesandoCola;
  private float tiempoUltimoSpawnTiempoReal = float.NegativeInfinity;
  private readonly List<float> recentSpawnTimes = new List<float>();
  private Coroutine rutinaTextoFlotanteCampania;
  private readonly Dictionary<TextMeshProUGUI, string> textosOriginalesDerrotaTMP = new Dictionary<TextMeshProUGUI, string>();
  private readonly Dictionary<Text, string> textosOriginalesDerrotaLegacy = new Dictionary<Text, string>();
  private bool textosDerrotaCacheados;
  private bool resolviendoJefeZona;
  private bool abriendoCiudadPuerto;
  private bool campaniaInicializada;
  private bool debeEscribirLogInicioEnStart;
  [SerializeField] private AsentamientoManager asentamientoManager;

#if UNITY_EDITOR
  private void OnValidate()
  {
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

  private void PrepararEscenaCampania()
  {
    if (goDerrota != null)
    {
      goDerrota.SetActive(false);
      CachearTextosOriginalesDerrota();
    }

    AsegurarAudioMovimientoCaravana();
    sfxMovimientoSource.volume = sfxMovimientoVolumen;
    sfxMovimientoSource.pitch = sfxMovimientoPitch * Mathf.Max(0.5f, multiplicadorVelocidadVisualViajeActual);
    AsegurarAsentamientoManager();
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

    ResetearEstadoTransitorioCampania();
    ConfigurarEstadoTutorialNuevaCampania();
    InicializarRecursosNuevaCampania();
    InicializarZonaNuevaCampania();
    InicializarSequitosNuevaCampania();
    InicializarProgresoNuevaCampania();
    InicializarClimaAlIniciar();
    InicializarPersonajesNuevaCampania();
    AjustarDificultad();

    debeEscribirLogInicioEnStart = true;
    campaniaInicializada = true;
  }

  private bool CargarCampaniaPendiente(SaveFileData savePendiente, out string error, bool iniciarNuevaCampaniaSiFalla)
  {
    error = string.Empty;

    if (savePendiente == null)
    {
      error = "El save pendiente es invalido.";
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

      debeEscribirLogInicioEnStart = false;
      campaniaInicializada = true;
      return true;
    }
    catch (Exception ex)
    {
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
    MoviendoCaravana = false;
    transicionZonaEnCurso = false;
    bloquearOlaDeCalorEnSiguienteTiradaClima = false;
    nodoDestinoActual = null;
    BATALLA_EnCurso = 0;
    resolviendoJefeZona = false;
    abriendoCiudadPuerto = false;
  }

  private void ConfigurarEstadoTutorialNuevaCampania()
  {
    int yaPasotuto = PlayerPrefs.GetInt("Tutorial_Terminado");

    if (debugSaltarTutorialAlIniciar)
    {
      yaPasotuto = 1;
    }

    scTutorialManager.tutorialActivo = yaPasotuto != 1;
  }

  private void InicializarRecursosNuevaCampania()
  {
    CambiarCivilesActuales(110);
    CambiarEsperanzaActual(75);
    CambiarSuministrosActuales(300);
    CambiarMaterialesActuales(45);
    CambiarBueyesActuales(22);

    if (scTutorialManager.tutorialActivo)
    {
      CambiarMaterialesActuales(-10);
      CambiarBueyesActuales(-4);
      CambiarSuministrosActuales(-100);
      CambiarCivilesActuales(-25);
    }

    CambiarOroActual(400);
    CambiarValorAlientoNegro(2);
  }

  private void InicializarZonaNuevaCampania()
  {
    int zonaInicial = ObtenerZonaInicialDebug();
    if (zonaInicial == 0 && scTutorialManager.tutorialActivo)
    {
      zonaInicial = 1;
    }

    scAtributosZona.GenerarZona(zonaInicial);
  }

  private int ObtenerZonaInicialDebug()
  {
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
    numeroTurno = 1;
    posicionCaravana = 1;
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
    if (!scTutorialManager.tutorialActivo)
    {
      AgregarHeroe(0);
      AgregarHeroe(0);
      AgregarHeroe(0);
      AgregarHeroe(0);
      return;
    }

    CrearAcechador();
  }


  private void OnEnable()
  {
    TryProcesarColaTextoFlotante();
  }

  private void OnDisable()
  {
    if (rutinaTextoFlotanteCampania != null)
    {
      StopCoroutine(rutinaTextoFlotanteCampania);
      rutinaTextoFlotanteCampania = null;
    }

    procesandoCola = false;
    colaTextos.Clear();
    recentSpawnTimes.Clear();
    tiempoUltimoSpawnTiempoReal = float.NegativeInfinity;
  }

  private void Start()
  {
    if (logDeCampania != null)
      logDeCampania.SetDiaActual(numeroTurno);

    TRADU.i.ActualizarIdioma();
    if (debeEscribirLogInicioEnStart)
    {
      EscribirLogInicioCampania();
      debeEscribirLogInicioEnStart = false;
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

    if (MoviendoCaravana)
    {
      motivo = "No se puede guardar mientras la caravana esta viajando.";
      return false;
    }

    if (transicionZonaEnCurso)
    {
      motivo = "No se puede guardar durante una transicion de zona.";
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
      || (asentamientoManager != null && asentamientoManager.TieneInteraccionActiva)
      || EstaInteraccionActiva(goUIComercioNodo)
      || EstaInteraccionActiva(goUIPersonajeSequito)
      || EstaInteraccionActiva(goUISantuario)
      || EstaInteraccionActiva(goUIVictoriaZona)
      || EstaInteraccionActiva(goMenuPuerto)
      || EstaInteraccionActiva(goDerrota);
  }

  private bool EstaInteraccionActiva(GameObject go)
  {
    return go != null && go.activeInHierarchy;
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
    data.numeroTurno = numeroTurno;
    data.posicionCaravana = posicionCaravana;
    data.tipoClima = intTipoClima;
    data.alientoNegro = GetValorAlientoNegro();
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
    data.sequitoHerrerosMantArmas = sequitoHerrerosMantArmas;
    data.sequitoHerrerosMantArmaduras = sequitoHerrerosMantArmaduras;
    data.sequitoMercaderesTier = sequitoMercaderesTier;
    data.sequitoCuranderosMejoraCuracion = sequitoCuranderosMejoraCuracion;
    data.miliciasMejoras = miliciasMejoras;
    data.peligroZonaAnterior = peligrozonaanterior;
    data.puestoComercialSuministrosDisp = pComercialSuministrosDisp;
    data.puestoComercialMaterialesDisp = pComercialMaterialesDisp;
    data.puestoComercialBueyesDisp = pComercialBueyesDisp;
    data.tutorialActivo = scTutorialManager != null && scTutorialManager.tutorialActivo;
    data.tutorialPasoActual = scTutorialManager != null ? scTutorialManager.pasoActual : 0;
    if (scAtributosZona != null && scAtributosZona.ZonasEstado != null)
    {
      data.zonasEstado = new List<int>(scAtributosZona.ZonasEstado);
    }
    data.nodoActual = CrearReferenciaNodo(scMapaManager != null ? scMapaManager.nodoActual : null);
    data.nodoDestinoActual = CrearReferenciaNodo(MoviendoCaravana ? nodoDestinoActual : null);
    data.eventosAleatoriosUsadosMapa = new List<int>(eventosAleatoriosUsadosMapa);
    data.estadosCaravana = estadosCaravana != null ? estadosCaravana.ConstruirSaveData() : new EstadosCaravanaSaveData();
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
      nodoData.costoMovimiento = nodo.costoMovimiento;
      nodoData.revelado = nodo.revelado;
      nodoData.yatiroConexiones = nodo.yatiroConexiones;
      nodoData.nodoIncendiado = nodo.nodoIncendiado;
      nodoData.nodoRitual = nodo.nodoRitual;
      nodoData.visualCode = nodo.ObtenerVisualCodeActual();
      nodoData.esMisterioso = nodo.ObtenerEstadoMisterioso();
      nodoData.atajoSubterraneoPendiente = nodo.ObtenerAtajoSubterraneoPendiente();

      if (nodo.DestinosPosibles != null)
      {
        foreach (Nodo destino in nodo.DestinosPosibles)
        {
          nodoData.destinos.Add(CrearReferenciaNodo(destino));
        }
      }

      data.nodes.Add(nodoData);
    }

    if (scMapaManager != null)
    {
      data.emboscadasSubterraneasZona = scMapaManager.ObtenerEmboscadasSubterraneasZona();
      data.viajesDesdeUltimaEmboscadaSubterranea = scMapaManager.ObtenerViajesDesdeUltimaEmboscadaSubterranea();
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
    data.habilidades = CopiarHabilidadesPersonaje(personaje);
    data.actividades = CopiarActividadesPersonaje(personaje);
    data.actividadSeleccionada = personaje.ActividadSeleccionada;
    data.nivelPuntoAtributo = personaje.NivelPuntoAtributo;
    data.nivelPuntoTS = personaje.NivelPuntoTS;
    data.nivelPuntoHabilidad = personaje.NivelPuntoHabilidad;
    data.nivelNuevaHabilidadBase = personaje.NivelNuevaHabilidadBase;
    data.campFatigado = personaje.Camp_Fatigado;
    data.campBendecidoSequitoClerigos = personaje.Camp_Bendecido_SequitoClerigos;
    data.campHerido = personaje.Camp_Herido;
    data.campEnfermo = personaje.Camp_Enfermo;
    data.campMoral = personaje.Camp_Moral;
    data.campAvergonzado = personaje.Camp_Avergonzado;
    data.campMuerto = personaje.Camp_Muerto;
    data.campCorrupto = personaje.Camp_Corrupto;
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

    return data;
  }

  private MetaprogresionSaveData ConstruirMetaprogresionSaveData()
  {
    MetaprogresionSaveData data = new MetaprogresionSaveData();
    MetaprogresionManager meta = MetaprogresionManager.Instance;
    if (meta == null)
    {
      return data;
    }

    data.corrupcionGlobal = meta.CorrupcionGlobal;
    data.cantidadCiviles = meta.CantidadCiviles;
    data.valorTrabajoDisponible = meta.ValordeTrabajoDisponible;
    data.misionesSalvamento = meta.MisionesSalvamento;
    data.nivelPeligroBosqueArdiente = meta.NivelPeligroBosqueArdiente;
    data.nivelPeligroPasoVientohelado = meta.NivelPeligroPasoVientohelado;
    data.nivelPeligroNedukazal = meta.NivelPeligroNedukazal;
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
      Transform contenedorInstancias = scMenuSequito.transform.childCount > 2 ? scMenuSequito.transform.GetChild(2) : null;
      if (contenedorInstancias != null)
      {
        foreach (Transform child in contenedorInstancias)
        {
          Destroy(child.gameObject);
        }
      }

      if (scMenuSequito.lstSequitos != null)
      {
        scMenuSequito.lstSequitos.Clear();
      }
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
    if (data == null || scTutorialManager == null)
    {
      return;
    }

    scTutorialManager.tutorialActivo = data.tutorialActivo;
    scTutorialManager.pasoActual = data.tutorialPasoActual;
  }

  private void RestaurarMetaprogresionDesdeSave(SaveFileData saveFileData)
  {
    if (saveFileData == null || saveFileData.version < 3 || saveFileData.metaprogresion == null || MetaprogresionManager.Instance == null)
    {
      return;
    }

    MetaprogresionSaveData data = saveFileData.metaprogresion;
    MetaprogresionManager meta = MetaprogresionManager.Instance;
    meta.CorrupcionGlobal = Mathf.Max(0, data.corrupcionGlobal);
    meta.CantidadCiviles = Mathf.Max(0, data.cantidadCiviles);
    meta.ValordeTrabajoDisponible = Mathf.Max(0, data.valorTrabajoDisponible);

    if (data.misionesSalvamento >= 0) meta.MisionesSalvamento = data.misionesSalvamento;
    if (data.nivelPeligroBosqueArdiente >= 0) meta.NivelPeligroBosqueArdiente = data.nivelPeligroBosqueArdiente;
    if (data.nivelPeligroPasoVientohelado >= 0) meta.NivelPeligroPasoVientohelado = data.nivelPeligroPasoVientohelado;
    if (data.nivelPeligroNedukazal >= 0) meta.NivelPeligroNedukazal = data.nivelPeligroNedukazal;
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
    if (data == null)
    {
      return;
    }

    numeroTurno = Mathf.Max(1, data.numeroTurno);
    posicionCaravana = Mathf.Max(0, data.posicionCaravana);
    mejoraCaravanaAntorchas = data.mejoraCaravanaAntorchas;
    mejoraCaravanaAlforjas = data.mejoraCaravanaAlforjas;
    mejoraCaravanaTiendas = data.mejoraCaravanaTiendas;
    mejoraCaravanaCatalejos = data.mejoraCaravanaCatalejos;
    mejoraCaravanaAlmacen = data.mejoraCaravanaAlmacen;
    mejoraCaravanaDefensas = data.mejoraCaravanaDefensas;
    sequitoHerrerosMantArmas = data.sequitoHerrerosMantArmas;
    sequitoHerrerosMantArmaduras = data.sequitoHerrerosMantArmaduras;
    sequitoMercaderesTier = data.sequitoMercaderesTier;
    sequitoCuranderosMejoraCuracion = data.sequitoCuranderosMejoraCuracion;
    miliciasMejoras = data.miliciasMejoras;
    peligrozonaanterior = data.peligroZonaAnterior;
    pComercialSuministrosDisp = data.puestoComercialSuministrosDisp;
    pComercialMaterialesDisp = data.puestoComercialMaterialesDisp;
    pComercialBueyesDisp = data.puestoComercialBueyesDisp;
    MoviendoCaravana = false;
    nodoDestinoActual = null;
    multiplicadorVelocidadVisualViajeActual = 1f;
    if (estadosCaravana == null)
    {
      estadosCaravana = new EstadosCaravana();
    }
    estadosCaravana.RestaurarDesdeSave(data.estadosCaravana);
    transicionZonaEnCurso = false;
    BATALLA_EnCurso = 0;
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
        entry.Value.RestaurarDesdeSave(nodeData);
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
        if (origen == null || nodeData.destinos == null)
        {
          continue;
        }

        foreach (NodeReferenceSaveData destinoRef in nodeData.destinos)
        {
          Nodo destino = BuscarNodoDesdeReferencia(destinoRef, nodosPorClave);
          if (destino == null)
          {
            continue;
          }

          bool esAtajo = destino.posXNodo - origen.posXNodo > 1;
          origen.ConectarConNodo(destino, esAtajo, false, true);
        }
      }
    }

    scMapaManager.scContenedordeNodos.RecolectarNodos();
    scMapaManager.RestaurarEstadoVariedadDesdeSave(saveFileData.map);
    scMapaManager.nodoActual = BuscarNodoDesdeReferencia(saveFileData.campaign != null ? saveFileData.campaign.nodoActual : null, nodosPorClave);
    if (scMapaManager.nodoActual == null)
    {
      scMapaManager.nodoActual = BuscarNodoDesdeReferencia(new NodeReferenceSaveData { x = 0, y = 0 }, nodosPorClave);
    }

    scMapaManager.PosicionarCaravanaEnNodoActual();
    if (scMapaManager.nodoActual != null)
    {
      scMapaManager.nodoActual.RefrescarCaminosMarcadosDesdeEstadoActual();
    }
  }

  private string BuildNodeKey(int x, int y)
  {
    return x + "_" + y;
  }

  private Nodo BuscarNodoDesdeReferencia(NodeReferenceSaveData referencia, Dictionary<string, Nodo> nodosPorClave)
  {
    if (referencia == null || referencia.x < 0 || referencia.y < 0 || nodosPorClave == null)
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

    RestaurarMercaderesDesdeSave(data);

    SequitoCuranderos curanderos = scMenuSequito.GetComponentInChildren<SequitoCuranderos>(true);
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

    if (scMenuPersonajes.listaPersonajes.Count > 0)
    {
      Personaje personajeInicial = scMenuPersonajes.listaPersonajes.Find(p => p != null && !p.Camp_Muerto);
      scMenuPersonajes.pSel = personajeInicial != null ? personajeInicial : scMenuPersonajes.listaPersonajes[0];
      scMenuPersonajes.ActualizarLista();
    }
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

    AplicarDatosBasePersonajeDesdeSave(personaje, data, habilidadesGuardadas, actividadesGuardadas);
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

  private void AplicarDatosBasePersonajeDesdeSave(Personaje personaje, CharacterSaveData data, int[] habilidadesGuardadas, int[] actividadesGuardadas)
  {
    personaje.SetPersistentId(string.IsNullOrWhiteSpace(data.id) ? Guid.NewGuid().ToString("N") : data.id);
    personaje.sNombre = data.nombre;
    personaje.IDClase = data.idClase;
    personaje.idRetrato = data.idRetrato;
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
    personaje.NivelPuntoAtributo = data.nivelPuntoAtributo;
    personaje.NivelPuntoTS = data.nivelPuntoTS;
    personaje.NivelPuntoHabilidad = data.nivelPuntoHabilidad;
    personaje.NivelNuevaHabilidadBase = data.nivelNuevaHabilidadBase;
    personaje.NormalizarPuntosPendientesPorNivelActual();
    personaje.Camp_Fatigado = data.campFatigado;
    personaje.Camp_Bendecido_SequitoClerigos = data.campBendecidoSequitoClerigos;
    personaje.Camp_Herido = data.campHerido;
    personaje.Camp_Enfermo = data.campEnfermo;
    personaje.Camp_Moral = data.campMoral;
    personaje.Camp_Avergonzado = data.campAvergonzado;
    personaje.Camp_Muerto = data.campMuerto;
    personaje.Camp_Corrupto = data.campCorrupto;
    personaje.aRasgos = new int[Mathf.Max(300, data.rasgos != null ? data.rasgos.Length : 300)];
    if (data.rasgos != null)
    {
      Array.Copy(data.rasgos, personaje.aRasgos, Mathf.Min(personaje.aRasgos.Length, data.rasgos.Length));
    }

    personaje.spRetrato = ObtenerRetratoCampaniaPorId(data.idRetrato, data.idClase);
    personaje.InicializarEscaladoDefensaPorAgilidadSiHaceFalta();
    personaje.InicializarEscaladoResElementalPorPoderSiHaceFalta();
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
      scMapaManager.nodoActual.RefrescarCaminosMarcadosDesdeEstadoActual();
    }

    if (logDeCampania != null)
    {
      logDeCampania.SetDiaActual(numeroTurno);
    }

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
      SequitoCuranderos curanderos = scMenuSequito.GetComponentInChildren<SequitoCuranderos>(true);
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
    }

    switch (idClase)
    {
      case 1: return scMenuPersonajes.Male001;
      case 2: return scMenuPersonajes.Male003;
      case 3: return scMenuPersonajes.Female001;
      case 4: return scMenuPersonajes.Male004;
      case 5: return scMenuPersonajes.Male005;
      default: return scMenuPersonajes.Male001;
    }
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
    }
  }

  private void AgregarHabilidadDeClaseSegunSlot(Personaje personaje, int slot, int nivel)
  {
    Type tipo = ResolverTipoHabilidadDeClase(personaje != null ? personaje.IDClase : 0, slot);
    Habilidad habilidad = AgregarComponenteSiFalta(personaje != null ? personaje.gameObject : null, tipo) as Habilidad;
    AsignarNivelHabilidad(habilidad, nivel);
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
    EstadoAlientoNegro = Mathf.Max(0f, data.alientoNegro);
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
      sliderAlientoNegro.value = Mathf.InverseLerp(0f, 20f, EstadoAlientoNegro);
    }

    ActualizarTierAlientoNegro();
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

    if (idioma == 2)
    {
      if (txtDescSum != null) txtDescSum.text = $"<Color=#F26B70>Sell: {(int)precio10Suministros / 2} Gold</color>    x10   <Color=#5ABD46>Buy: {(int)precio10Suministros} Gold</color>";
      if (txtDescMat != null) txtDescMat.text = $"<Color=#F26B70>Sell: {(int)precio1Material / 2} Gold</color>    x1   <Color=#5ABD46>Buy: {(int)precio1Material} Gold</color>";
      if (txtDescBuey != null) txtDescBuey.text = $"<Color=#F26B70>Sell: {(int)precio1Buey / 2}  Gold</color>    x1   <Color=#5ABD46>Buy: {(int)precio1Buey} Gold</color>";
      return;
    }

    if (txtDescSum != null) txtDescSum.text = $"<Color=#F26B70>Venta: {(int)precio10Suministros / 2} Oro</color>    x10   <Color=#5ABD46>Compra: {(int)precio10Suministros} Oro</color>";
    if (txtDescMat != null) txtDescMat.text = $"<Color=#F26B70>Venta: {(int)precio1Material / 2} Oro</color>    x1   <Color=#5ABD46>Compra: {(int)precio1Material} Oro</color>";
    if (txtDescBuey != null) txtDescBuey.text = $"<Color=#F26B70>Venta: {(int)precio1Buey / 2}  Oro</color>    x1   <Color=#5ABD46>Compra: {(int)precio1Buey} Oro</color>";
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

  public void AplicarClimaMasacreNedukazalForzada(bool escribirLogDebug = false)
  {
    intTipoClima = 9;
    if (widgetClima != null)
    {
      widgetClima.sprite = clima_NedukazalMasacre;
    }

    RefrescarVfxClimaCalor();

    if (escribirLogDebug)
    {
      Debug.Log("[CampaignManager] Debug de clima activo: Masacre de Nedukazal forzada.");
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
#if UNITY_EDITOR
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
  }

  public void RefrescarVfxClimaCalor()
  {
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
      heatWaveEffect.SetEffectActive(intTipoClima == 2);
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
  }

  private bool EstaDentroDelAlientoNegro()
  {
    if (scAtributosZona == null || scAtributosZona.ID == 3)
    {
      return false;
    }

    // Reutiliza el mismo umbral que el estado "Dentro" del tooltip (tier 3+).
    float cercaniaAliento = -(posicionCaravana - EstadoAlientoNegro);
    return cercaniaAliento >= 7f;
  }

  public bool EstaDentroOpeorDelAlientoNegro()
  {
    return EstaDentroDelAlientoNegro();
  }
  #region Nodos
  public SunController sunController;

  public void ViajeIniciado(Nodo destino, bool viajeSubterraneo = false)
  {
    EfectosViajeCaravana efectosViajeCaravana = estadosCaravana != null
      ? estadosCaravana.IniciarViajeActual()
      : default;

    bool sePrevieneAvanceAliento = false;
    nodoDestinoActual = destino;
    sunController.OnTravelStart(); // duración en segundos
    animCaravana.SetBool("IsWalking", true);
    multiplicadorVelocidadVisualViajeActual = efectosViajeCaravana.multiplicadorVelocidadVisual <= 0f
      ? 1f
      : efectosViajeCaravana.multiplicadorVelocidadVisual;
    animCaravana.speed = multiplicadorVelocidadVisualViajeActual;

    // Inicia sonido de caravana en movimiento
    IniciarSonidoMovimientoCaravana(Mathf.Max(0.05f, sfxMovimientoFadeIn));

    //Sequito de Clerigos 20% de prevenirlo
    int random2 = UnityEngine.Random.Range(0, 100);
    if (scMenuSequito.TieneSequito(10) && random2 < 21) //Clérigos !!! 21
    {
      sePrevieneAvanceAliento = true;
      EscribirLog(TRADU.i.Traducir("-Los rezos constantes del Séquito de Clérigos han logrado frenar el avance del Aliento Negro."));

    }

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      int random = UnityEngine.Random.Range(0, 100);
      if (pers.ActividadSeleccionada == 10 && random < 15 && !sePrevieneAvanceAliento) //Ritual de Limpieza
      {
        sePrevieneAvanceAliento = true;
        EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" ha realizado con Éxito un Ritual de Limpieza, previniendo el avance del Aliento Negro."));
        break;
      }

      //Aca solamente va esa actividad! las demas van mas abajo
    }

    if (efectosViajeCaravana.previeneAvanceAliento)
    {
      sePrevieneAvanceAliento = true;
      EscribirLog(TRADU.i.Traducir("-La Presteza de la Caravana ha evitado el avance del Aliento Negro durante el viaje."));
    }

    if (!sePrevieneAvanceAliento)
    {
      CambiarValorAlientoNegro(destino.costoMovimiento); //Avance Aliento Negro por día, si no es prevenido por Purificadora o Clérigos
    }


    numeroTurno++;
    if (destino.costoMovimiento > 1 && !sePrevieneAvanceAliento)
    {
      EscribirLog(TRADU.i.Traducir("-El viaje por el camino sinuoso ha retrasado la caravana. +") + destino.costoMovimiento + TRADU.i.Traducir(" Avance del Aliento Negro"));
    }

    //Si Nieva, avanza 1 mas el élito
    if (intTipoClima == 4)
    {
      if (!sePrevieneAvanceAliento)
      {
        EscribirLog(TRADU.i.Traducir("-La nieve a retrasado el viaje. +1 Avance del Aliento Negro"));
        CambiarValorAlientoNegro(1);
      }
    }

    if (efectosViajeCaravana.avanceAlientoExtra > 0)
    {
      EscribirLog(TRADU.i.Traducir("-La Caravana se mueve con Aletargamiento. +1 Avance del Aliento Negro."));
      CambiarValorAlientoNegro(efectosViajeCaravana.avanceAlientoExtra);
    }

    if (intTipoClima == 6)
    {
      if (scAtributosZona.ID == 1) // Bosque Angustiante
      {
        EscribirLog(TRADU.i.Traducir("-Las Almas Danzantes guían a la caravana. +5 Esperanza"));
        CambiarEsperanzaActual(5);
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
      EscribirLog(TRADU.i.Traducir("-La ausencia de Aliento Negro al viajar, inspira a la Caravana. +2 Esperanza"));
      CambiarEsperanzaActual(2);
    }
    if (GetTierAlientoNegro() == 2)
    {
      EscribirLog(TRADU.i.Traducir("-La presencia notable del Aliento Negro al viajar, provoca incertidumbre en la Caravana. -3 Esperanza"));
      CambiarEsperanzaActual(-3);
    }
    if (GetTierAlientoNegro() == 3)
    {
      EscribirLog(TRADU.i.Traducir("-La gran presencia de Aliento Negro en el aire, provoca temor en la Caravana. -5 Esperanza"));
      CambiarEsperanzaActual(-5);
    }
    if (GetTierAlientoNegro() == 4)
    {
      CambiarEsperanzaActual(-7);
      int random = UnityEngine.Random.Range(1, 5);
      CambiarCivilesActuales(-random);
      EscribirLog(TRADU.i.Traducir("-La presencia de Aliento Negro en el aire es fatal para los Civiles. -7 Esperanza -") + random + TRADU.i.Traducir(" Civiles"));
    }

    if (sequitoHerrerosMantArmaduras > 0) { sequitoHerrerosMantArmaduras--; }
    if (sequitoHerrerosMantArmas > 0) { sequitoHerrerosMantArmas--; }

    EfectosdeActividades();
    EfectosdeSequitos();



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

  private IEnumerator FadeInSonidoMovimientoCaravanaCoroutine(float duracion)
  {
    if (sfxMovimientoSource == null)
    {
      rutinaDesvanecerSfxMovimiento = null;
      yield break;
    }

    sfxMovimientoSource.pitch = sfxMovimientoPitch;
    sfxMovimientoSource.loop = true;
    float volumenObjetivo = Mathf.Clamp01(sfxMovimientoVolumen);
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
      sfxMovimientoSource.volume = Mathf.Lerp(volumenInicial, volumenObjetivo, t);
      yield return null;
    }

    if (sfxMovimientoSource != null)
    {
      sfxMovimientoSource.volume = volumenObjetivo;
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
      sfxMovimientoSource.volume = sfxMovimientoVolumen;
      sfxMovimientoSource.pitch = sfxMovimientoPitch;
    }

    rutinaDesvanecerSfxMovimiento = null;
  }

  public void LlegarANodo(int ID, int posX, Nodo nodo)
  {
    
    // Detiene el sonido de movimiento al llegar al nodo
    if (sfxMovimientoSource != null && sfxMovimientoSource.isPlaying && rutinaDesvanecerSfxMovimiento == null)
    {
      DesvanecerSonidoMovimientoCaravana(Mathf.Max(0.05f, sfxMovimientoFadeOut));
    }

    animCaravana.SetBool("IsWalking", false);
    animCaravana.speed = 1f;
    int modEmboscadaViajeActual = estadosCaravana != null ? estadosCaravana.ObtenerModificadorEmboscadaDuranteViajeActual() : 0;
    if (estadosCaravana != null)
    {
      estadosCaravana.FinalizarViajeActual();
    }
    
    if (scTutorialManager.pasoActual == 18) { scTutorialManager.SiguientePaso(); CambiarValorAlientoNegro(2); }
    if (scTutorialManager.pasoActual == 31) { scTutorialManager.SiguientePaso();  }


    posicionCaravana = posX + 1;
    if (ID == 1) //Batalla
    {

      goMenuBatallas.SetActive(true);

      if (scTutorialManager.pasoActual == 2) {scTutorialManager.establecerPasoEspecifico(3); }
     
      

      //Probabilidad emboscada
      int randomEmboscada = UnityEngine.Random.Range(1, 101);

      int chancesemboscada = scAtributosZona.modChanceEmboscada + modEmboscadaViajeActual;
      chancesemboscada -= CuantosPersonajesHacenTalActividad(14) * 5; //-5% por cada Acechador Actividad Vigilar Desde Sombras

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


      if (randomEmboscada <= chancesemboscada)
      {
        scMenuBatallas.EventoBatallaNormal(0, 1); //Emboscada

      }
      else
      {
        scMenuBatallas.EventoBatallaNormal(0, 0); //No emboscada

      }


    }
    if (ID == 8) //Batalla Elite
    {

      goMenuBatallas.SetActive(true);
      scMenuBatallas.EventoBatallaElite(0); //0 es Random

    }
    if (ID == 2) //Evento
    {

      float factorEventoBuenoMalo = 40 + Instance.GetEsperanzaActual() / 3;
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

      goMenuBatallas.SetActive(true);
      scMenuBatallas.EventoBatallaFinal(0); //0 es Random

    }
    if (ID == 11) //Ataque Caravana
    {

      goMenuBatallas.SetActive(true);
      scMenuBatallas.EventoBatallaCaravana(0); //0 es Random

    }
    if (ID == 12) //Ataque Subterráneo
    {
      EscribirLog(TRADU.i.Traducir("-La Caravana ha sido emboscada por un ataque subterráneo."));

      goMenuBatallas.SetActive(true);
      scMenuBatallas.EventoBatallaSubterranea(scAtributosZona.FASE); //0 es Random

    }
    if (ID == 14) //Santuario
    {

      goUISantuario.SetActive(true);
      txtdescripcionSantuario.text = TRADU.i.Traducir("Has llegado a un Santuario de Purificadores, varios se han construido en la zona para dar apoyo y plegarias a los valientes que combatieron al Liche.\nHoy, si bien está abandonado, mantiene su aura de tranquilidad y puedes depositar ofrendas para realizar una plegaria de purificación.\n\n\n. ");

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
        pers.RecibirCuracion(curacion);

      }

      scMapaManager.nodoActual.nodoDespejado = true;

    }
    if (ID == 15) //Batalla Ritual PasoVientohelado
    {

      goMenuBatallas.SetActive(true);
      scMenuBatallas.EventoBatallaElite(0, 0, true); //0 es Random

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
      EscribirLog(TRADU.i.Traducir("-El Séquito de Nobles ha hecho una donación. Oro: " + oro));
    }
    //Esclavos
    if (scMenuSequito.TieneSequito(11))
    {
      CambiarEsperanzaActual(-2);
      EscribirLog(TRADU.i.Traducir("-Los Civiles se sienten culpables por la presencia de los Esclavos. -2 Esperanza."));
    }


    BosqueArdienteMecanicaIncendio(35);
    PasoVientoHeladoMecanicaRituales(30);


    if (nodo.nodoIncendiado)
    {
      CambiarEsperanzaActual(-10);
      int nmuertos = UnityEngine.Random.Range(8, 16);
      CambiarCivilesActuales(-nmuertos);
      EscribirLog(TRADU.i.Traducir("-La caravana ha llegado a un nodo incendiado. -10 Esperanza.  ") + nmuertos + TRADU.i.Traducir(" Civiles Muertos."));
    }

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

    AplicarTraduccionPanelDerrota();
    goDerrota.SetActive(true);
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
    if (scAtributosZona.FASE < 3) //Zona completada pero no es la final
    {



      goUIVictoriaZona.SetActive(true);

      foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
      {
        pers.RecibirCuracion(2000);
        pers.Camp_Herido = false;
      }
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
    if (scAdministradorEscenas != null)
    {
      scAdministradorEscenas.LimpiarAvergonzadoPorCambioZona();
    }
    scMapaManager.ResetearYGenerarSiguienteZona();
    ResetearAlientoNegro();
    scAtributosZona.GenerarZona(0); //0 es aleatorio
    transicionZonaEnCurso = false;
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
    if (scTutorialManager != null && scTutorialManager.tutorialActivo) { return; }
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
        int random = UnityEngine.Random.Range(1, 3);
        Nodo candidato = ObtenerNodoFuturoAleatorio(random);
        if (candidato == null) { continue; }
        if (candidato.nodoIncendiado) { continue; }
        if (candidato.tipoNodo == codigoAsentamiento) { continue; }
        nodoAIncendiar = candidato;
        break;
      }

      if (nodoAIncendiar != null)
      {
        nodoAIncendiar.ActivarIncendio();
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
        int random = UnityEngine.Random.Range(1, 3);
        Nodo nodoRitual = ObtenerNodoFuturoAleatorio(random);
        if (nodoRitual != null && !nodoRitual.nodoRitual)
        {
          nodoRitual.tipoNodo = 15; // Nodo Ritual
          nodoRitual.ActivarNodoVisual(15, false, true);
          nodoRitual.ActivarRitual();
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
  public Nodo ObtenerNodoFuturoAleatorio(int distancia = 0)
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

        if (!nodosPorDistancia.TryGetValue(distanciaDestino, out var lista))
        {
          lista = new List<Nodo>();
          nodosPorDistancia[distanciaDestino] = lista;
        }
        lista.Add(destino);

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
    CambiarValorAlientoNegro(-3);
    EscribirLog(TRADU.i.Traducir("-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se ha gastado 200 de oro."));

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
      EscribirLog(TRADU.i.Traducir("-No hay personajes corruptos para purificar."));
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
      EscribirLog(TRADU.i.Traducir("-No tienes suficientes bueyes para realizar el ritual en el santuario."));
      return;
    }

    CambiarBueyesActuales(-3);
    CambiarValorAlientoNegro(-3);
    EscribirLog(TRADU.i.Traducir("-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se han sacrificado 3 bueyes."));

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
      EscribirLog(TRADU.i.Traducir("-No hay personajes corruptos para purificar."));
    }
    goUISantuario.SetActive(false);

    if (scTutorialManager.pasoActual == 20) { scTutorialManager.SiguientePaso(); }


  }
  #endregion
  #region Puesto Comercial
  public GameObject goUIComercioNodo;
  public GameObject goUISantuario;
  public TextMeshProUGUI txtdescripcionSantuario;

  public TextMeshProUGUI txtDescripcionPuestoComercial;

  public TextMeshProUGUI txtComnercialSumDisp;
  public TextMeshProUGUI txtComnercialMatDisp;
  public TextMeshProUGUI txtComnercialBueyesDisp;

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



    if (TRADU.i.nIdioma == 1) //Español
    {
      txtDescSum.text = $"<Color=#F26B70>Venta: {(int)precio10Suministros / 2} Oro</color>    x10   <Color=#5ABD46>Compra: {(int)precio10Suministros} Oro</color>";
      txtDescMat.text = $"<Color=#F26B70>Venta: {(int)precio1Material / 2} Oro</color>    x1   <Color=#5ABD46>Compra: {(int)precio1Material} Oro</color>";
      txtDescBuey.text = $"<Color=#F26B70>Venta: {(int)precio1Buey / 2}  Oro</color>    x1   <Color=#5ABD46>Compra: {(int)precio1Buey} Oro</color>";
    }
    else if (TRADU.i.nIdioma == 2) //Inglés
    {
      txtDescSum.text = $"<Color=#F26B70>Sell: {(int)precio10Suministros / 2} Gold</color>    x10   <Color=#5ABD46>Buy: {(int)precio10Suministros} Gold</color>";
      txtDescMat.text = $"<Color=#F26B70>Sell: {(int)precio1Material / 2} Gold</color>    x1   <Color=#5ABD46>Buy: {(int)precio1Material} Gold</color>";
      txtDescBuey.text = $"<Color=#F26B70>Sell: {(int)precio1Buey / 2}  Gold</color>    x1   <Color=#5ABD46>Buy: {(int)precio1Buey} Gold</color>";
    }
    ActualizarPuestoComercial();
  }

  public void ActualizarPuestoComercial()
  {
    txtComnercialSumDisp.text = "" + pComercialSuministrosDisp;
    txtComnercialMatDisp.text = "" + pComercialMaterialesDisp;
    txtComnercialBueyesDisp.text = "" + pComercialBueyesDisp;

    if ((pComercialSuministrosDisp > 0) && (GetOroActuales() >= precio10Suministros))
    {
      btnCompraSum.SetActive(true);
    }
    else { btnCompraSum.SetActive(false); }

    if (GetSuministrosActuales() > 0)
    {
      btnVentaSum.SetActive(true);
    }
    else { btnVentaSum.SetActive(false); }



    if ((pComercialMaterialesDisp > 0) && (GetOroActuales() >= precio1Material))
    {
      btnCompraMat.SetActive(true);
    }
    else { btnCompraMat.SetActive(false); }

    if (GetMaterialesActuales() > 0)
    {
      btnVentaMat.SetActive(true);
    }
    else { btnVentaMat.SetActive(false); }


    if ((pComercialBueyesDisp > 0) && (GetOroActuales() >= precio1Buey))
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

  public void ComprarSum()
  {
    CambiarOroActual(-(int)precio10Suministros);
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
    CambiarOroActual(-(int)precio1Material);
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
    CambiarOroActual(-(int)precio1Buey);
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

  public void AbrirPuestoComercial()
  {
    if (goUIComercioNodo == null)
    {
      return;
    }

    goUIComercioNodo.SetActive(true);
    txtDescripcionPuestoComercial.text = TRADU.i.Traducir("Has llegado a un improvisado Puesto Comercial, ofrecen Suministros básicos de supervivencia a los viajeros.\nEl Tier de tu Séquito de Mercaderes ayudará a bajar los precios.\n\n\nTu Séquito de Mercaderes ha actualizado su Inventario.");

    ResetearPuestoComercial();
    if (scSequitoMercaderes != null)
    {
      scSequitoMercaderes.GenerarItemsVendidos();
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
      if (pers != null && !pers.Camp_Muerto && pers.ActividadSeleccionada == IDActividad)
      {
        cant++;
      }
    }

    return cant;
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

  public int ObtenerCapacidadMaximaPersonajes()
  {
    return 4 + Mathf.Max(0, mejoraCaravanaTiendas);
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

    Nodo nodoFuturo = ObtenerNodoFuturoAleatorio(2);
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
    nodoFuturo.ActivarNodoVisual(16, false, true);
    return true;
  }

  private void ActualizarEstadoPersonajesTrasPasoDeDia()
  {
    if (scMenuPersonajes == null || scMenuPersonajes.listaPersonajes == null)
    {
      return;
    }

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || pers.Camp_Muerto)
      {
        continue;
      }

      if (pers.Camp_Enfermo > 0)
      {
        pers.Camp_Enfermo -= 1;
      }

      if (pers.Camp_Moral > 0)
      {
        pers.Camp_Moral -= 1;
      }
      else if (pers.Camp_Moral < 0)
      {
        pers.Camp_Moral += 1;
      }
    }
  }

  public void ProcesarPasoDeDiaEnAsentamiento(float multiplicadorCuracionDescanso = 1.1f, bool aplicarActividades = true, bool aplicarEfectosSequitos = true)
  {
    ActualizarEstadoPersonajesTrasPasoDeDia();

    if (aplicarActividades)
    {
      EfectosdeActividades(multiplicadorCuracionDescanso);
    }

    if (aplicarEfectosSequitos)
    {
      EfectosdeSequitos();
    }
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
  void EfectosdeActividades(float multiplicadorCuracionDescanso = 1f)
  {

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers.ActividadSeleccionada == 1) //Descanso
      {

        float cantPurificadorasColaborando = CuantosPersonajesHacenTalActividad(12); //Colaborar con los Curanderos

        float cantHerboristasVecesClaro = 0;
        if (scMenuSequito.TieneSequito(5)) //Herboristas
        {
          cantHerboristasVecesClaro = 0.03f + 0.03f * scSequitoHerboristas.vecesEnClaro; //3+3% por cada Claro visitado
        }


        float curacionFinalSequito = sequitoCuranderosMejoraCuracion + (cantPurificadorasColaborando * 0.05f) + cantHerboristasVecesClaro; //5% por cada Purificadora colaborando

        float porcentajeVidaMax = pers.fVidaMaxima * curacionFinalSequito * Mathf.Max(0f, multiplicadorCuracionDescanso);
        if (pers.fVidaMaxima > pers.fVidaActual)
        {
          EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" se cura ") + (int)porcentajeVidaMax + TRADU.i.Traducir(" PV por su Actividad de <b>Descanso</b>."));
        }
        pers.RecibirCuracion(porcentajeVidaMax);

      }
      if (pers.ActividadSeleccionada == 2) //Entrenar
      {
        int exp = 20;
        if (scMenuSequito.TieneSequito(6)) //Desertores
        {
          exp += 10;
        }
        pers.RecibirExperiencia(exp);

        EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" gana ") + exp + TRADU.i.Traducir(" Experiencia por su Actividad de <b>Entrenamiento</b>."));

      }
      if (pers.ActividadSeleccionada == 4) //Caballero: Relatos de Batalla
      {

        foreach (Personaje pers2 in scMenuPersonajes.listaPersonajes)
        {
          if (pers2.fNivelActual < pers.fNivelActual)
          {
            pers2.RecibirExperiencia(10);
          }
        }


        EscribirLog($"-" + pers.sNombre + TRADU.i.Traducir(" brinda 10 Experiencia a sus compañeros de menor nivel por su Actividad de <b>Relatos de Batalla</b>."));

      }
      if (pers.ActividadSeleccionada == 7) //Explorador: Caza Nocturna
      {
        int rand = UnityEngine.Random.Range(1, 5);
        CambiarSuministrosActuales(rand);
        EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" consigue ") + rand + TRADU.i.Traducir(" suministros por su Actividad de <b>Caza Nocturna</b>."));
      }
      if (pers.ActividadSeleccionada == 11) //Purificadora: Ayudar a los Desamparados
      {
        int rand = UnityEngine.Random.Range(1, 4);
        CambiarEsperanzaActual(rand);
        EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" realiza su actividad <b>Ayudar a los Desamparados</b> y la esperanza aumenta en ") + rand + ".");
      }
      if (pers.ActividadSeleccionada == 15) //Acechador: Coerción
      {
        int rand = UnityEngine.Random.Range(1, 10);
        CambiarEsperanzaActual(-1);
        CambiarOroActual(rand);
        EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" obtiene ") + rand + TRADU.i.Traducir(" de Oro de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -1 Esperanza"));
      }
      if (pers.ActividadSeleccionada == 18) //Canalizador: Simbolo de Proteccion Arcano
      {

        GameObject consumible = Instantiate(scContprefab.SimboloProtArcano.gameObject);
        scMenuPersonajes.scEquipo.listInventario.Add(consumible);
        EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" ha creado un Símbolo de Protección Arcano."));

      }
    }



  }

  public int ExploracionSumadaPorActividades()
  {
    int valor = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
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
        txtTooltipAlientoNegro.text += TRADU.i.Traducir("<color=#bae895><b>Estado: Distante</b> (") + EstadoAlientoNegro + TRADU.i.Traducir("/20) - La Caravana viaja con tranquilidad.</color>");
      }
      if (tierAliento == 2)
      {
        txtTooltipAlientoNegro.text += TRADU.i.Traducir("<color=#c8a6e8><b>Estado: Cerca</b> (") + EstadoAlientoNegro + TRADU.i.Traducir("/20) - La Caravana comienza a preocuparse y la podredumbre se siente en el aire. Los Corrompidos acechan en las sombras.</color>");
      }
      if (tierAliento == 3)
      {
        txtTooltipAlientoNegro.text += TRADU.i.Traducir("<color=#aa66ea><b>Estado: Dentro</b> (") + EstadoAlientoNegro + TRADU.i.Traducir("/20) - La Caravana ya es directamente afectada por el hedor. Los Corrompidos se dejan ver.</color>");
      }
      if (tierAliento == 4)
      {
        txtTooltipAlientoNegro.text += TRADU.i.Traducir("<color=#7a1dd1><b>Estado: Nocivo</b> (") + EstadoAlientoNegro + TRADU.i.Traducir("/20) - La peste comienza a tomar vidas civiles. Los Corrompidos son implacables.</color>");
      }

    }
    else
    {
      tooltipAlientoNegro.SetActive(false);
    }

  }
  private float EstadoAlientoNegro; //Va de 1 a 20, arranca en 3. Tier I 0-5 - Tier II 6-10 - Tier III 11-15 - Tier IV 16-20
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
  public void CambiarValorAlientoNegro(int aliento)
  {
    if (scAtributosZona.ID == 3) { return; }  //Nedukazal no tiene Aliento Negro 


    EstadoAlientoNegro += aliento;

    if (EstadoAlientoNegro < 0) { EstadoAlientoNegro = 0; }

    ActualizarTierAlientoNegro();

    AvanzarAlientoNegro(aliento);

    if (EstadoAlientoNegro > 16 && scMenuSequito.TieneSequito(10)) //Sequito de Clérigos
    {
      scMenuSequito.RemoverSequito(10);
      EscribirLog(TRADU.i.Traducir("-El Séquito de Clérigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crítico. -20 Esperanza"));
    }

    

  }

  public void ResetearAlientoNegro()
  { 
    EstadoAlientoNegro = 3;
    ActualizarTierAlientoNegro();
  }
  public void AvanzarAlientoNegro(int n)
  {
    scAlientoNegroVFX.AvanzarAlientoNegro(n);

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
    Image handleSliderCalavera = sliderAlientoNegro.gameObject.transform.GetChild(2).GetChild(0).gameObject.GetComponent<Image>();

    float cercaniaAliento = -(posicionCaravana - EstadoAlientoNegro);

    if (cercaniaAliento < 3)
    {
      TierAlientoNegro = 1;

      //  handleSliderCalavera.color = new Color(0.15f, 0.15f, 0.15f);

    }
    else if (cercaniaAliento >= 4 && cercaniaAliento < 6)
    {
      TierAlientoNegro = 2;

      //  handleSliderCalavera.color = new Color(0.15f, 0.12f, 0.12f);
    }
    else if (cercaniaAliento >= 7 && cercaniaAliento < 9)
    {
      TierAlientoNegro = 3;

      //  handleSliderCalavera.color = new Color(0.18f, 0.3f, 0.3f);
    }
    else if (cercaniaAliento >= 10)
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
      switch (fatigaAhora)
      {
        case 4: CambiarEsperanzaActual(-10); int rand = UnityEngine.Random.Range(-2, 1); CambiarBueyesActuales(rand); if (rand < 0) { EscribirLog(TRADU.i.Traducir("-La fatiga ha provocado la muerte de algunos Bueyes.") + " -" + rand + TRADU.i.Traducir(" Bueyes")); } break;    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        case 5: CambiarEsperanzaActual(-15); int rand2 = UnityEngine.Random.Range(-2, 1); CambiarBueyesActuales(rand2); if (rand2 < 0) { EscribirLog(TRADU.i.Traducir("-La fatiga ha provocado la muerte de algunos Bueyes.") + " -" + rand2 + TRADU.i.Traducir(" Bueyes")); } break;
        case > 5: CambiarEsperanzaActual(-20); int rand3 = UnityEngine.Random.Range(-4, 1); CambiarBueyesActuales(rand3); int rand4 = UnityEngine.Random.Range(-10, 1); CambiarCivilesActuales(rand4); if (rand3 < 0 || rand4 < 0) { EscribirLog(TRADU.i.Traducir("-La fatiga extrema ha provocado la muerte de algunos Bueyes y Civiles.") + " -" + rand3 + TRADU.i.Traducir(" Bueyes -") + rand4 + TRADU.i.Traducir(" Civiles")); } break;
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
          pers.Camp_Fatigado = true;
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
  }


  #endregion
  #region Civiles
  [SerializeField] private TextMeshProUGUI valueCiviles;
  private int civilesActuales;
  public float GetCivilesActual()
  {
    return civilesActuales;
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
    int cargaPorBuey = 25 + mejoraCaravanaAlforjas;
    CargaMaxActual = BueyesActuales * cargaPorBuey;
    CargaMaxActual += CuantosPersonajesHacenTalActividad(17) * 20; //Canalizador: Telekinesis
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

    GameObject textoOrigen = valueOro != null ? valueOro.gameObject : null;
    await GenerarTextoRecursos(oroAplicado, textoOrigen, true);
  }
  #endregion


  async Task GenerarTextoRecursos(int cantidad, GameObject textoOrigen, bool efectoRetraso)
  {
    if (cantidad == 0 || prefabTextoRecursos == null || textoOrigen == null)
    {
      return;
    }

    await serializacionTextosRecursos.WaitAsync();
    try
    {
    // Encuentra todos los objetos existentes del prefab scUnidadCanvas.PrefabtxtDaño
    GameObject[] existingTextObjects = GameObject.FindGameObjectsWithTag(prefabTextoRecursos.tag);

    // Calcula el retraso total en milisegundos
    int delayPerObject = 100;
    if (!efectoRetraso) { delayPerObject = 80; }
    int totalDelay = delayPerObject * existingTextObjects.Length;

    // Espera el tiempo calculado
    await BattleManager.DelayCombateAsync(totalDelay);

    if (this == null || prefabTextoRecursos == null || textoOrigen == null)
    {
      return;
    }

    // Instancia el nuevo objeto
    GameObject goTextoFlotante = Instantiate(prefabTextoRecursos, textoOrigen.transform, false);


    TextMeshProUGUI txtMesh = goTextoFlotante.GetComponent<TextMeshProUGUI>();


    // Configura el texto y el color



    if (cantidad >= 1)
    {
      txtMesh.color = new Color(0.1f, 0.7f, 0.2f); ;
      txtMesh.text = "+" + cantidad;

    }
    else if (cantidad < 0)
    {
      txtMesh.text = "" + cantidad;
      txtMesh.color = new Color(0.7f, 0.1f, 0.2f); ;
    }
    }
    finally
    {
      serializacionTextosRecursos.Release();
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

      int cargaPorBuey = 25 + mejoraCaravanaAlforjas;
      text += TRADU.i.Traducir("Los <color=#9e2a1c>Bueyes</color> son utilizados para llevar la carga de la caravana.\nCada uno da ") + cargaPorBuey + TRADU.i.Traducir(" de Capacidad de Carga.\n");
      text += TRADU.i.Traducir("\nLlevas ") + num + TRADU.i.Traducir(" <color=#9e2a1c>Bueyes</color>, por un total de Capacidad de Carga de ") + (num * cargaPorBuey) + ".\n\n";
      text += TRADU.i.Traducir("\nLlevas ") + num2 + TRADU.i.Traducir(" <color=#b7972c>Suministros</color> y ") + num3 + TRADU.i.Traducir(" <color=#b34f09>Materiales</color> por un total de peso de ") + num4 + "/" + (num * 25) + ".\n\n";

      if (num4 > GetCapacidadDeCargaActual())
      {
        text += TRADU.i.Traducir("<color=#cc0d0d>La Caravana lleva Sobrecarga. Cada tramo que se haga duplica la Fatiga obtenida y reduce 10 la <color=#a0e812>Esperanza</color></color>.\n\n");

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

      if (scAtributosZona.ID == 1) //Bosque Ardiente
      {

        text.text = TRADU.i.Traducir("A medida que viajas por el bosque, las llamas envolverán regiones del mapa de forma inesperada.\n\nSi intentas atravesar un Nodo prendido fuego, perderás 10 de Esperanza y 8-15 Civiles.\nNo se podrá descansar en nodos incendiados.\n\nAdemás, las batallas que tengan lugar en un Nodo incendiado, tendrán llamas en el campo de batalla.");
      }
      if (scAtributosZona.ID == 2) //Paso Vientohelado
      {
        text.text = TRADU.i.Traducir("La tribu Kale'Tav está realizando rituales en el área, preparándose para el Aliento Negro.\n\nAl escuchar sus tambores a lo lejos sabrás dónde se encuentran.\nPor cada Ritual completado, sus combatientes recibirán bonificaciones en batalla.\n\nPara interrumpir un ritual debes aproximarte a los nodos marcados y derrotarlos.\n\nFuerza Kale'Tav: ") + scAtributosZona.PasoVientoHelado_FuerzaKaleTav;
      }
      if (scAtributosZona.ID == 3) //Nedukazal
      {
        text.text = TRADU.i.Traducir("Debido a la invasión, Nedukazal está envuelta en caos y oscuridad, por lo tanto la caravana no podrá ver claramente el camino adelante.\n\nAl depender de la luz propia, será más propensa a sufrir emboscadas (+20%).\n\nMejora las <b>Antorchas de Pie</b> para aumentar el rango de visión.\n\nEl Aliento Negro no será una preocupación en esta zona.");
      }



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
  }

  #endregion


  #region Descanso
  [SerializeField] Image botonDescansar;
  public Sprite campSi;
  public Sprite campNo;

  [SerializeField] GameObject menuDescanso;

  public void AbrirMenuDescanso()
  {
    if (scTutorialManager.tutorialActivo && scTutorialManager.pasoActual < 24) { return; }
    if (scTutorialManager.tutorialActivo && scTutorialManager.pasoActual == 24) { scTutorialManager.SiguientePaso(); }
    if (asentamientoManager != null && asentamientoManager.TieneInteraccionActiva) { return; }
    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    if (nodoActual != null && nodoActual.tipoNodo == 4)
    {
      EscribirLog(TRADU.i.Traducir("<color=#FF6666>En los Asentamientos debes usar las acciones propias del asentamiento.</color>"));
      return;
    }

    bool puedeDescansar = nodoActual != null && nodoActual.nodoDespejado && !nodoActual.nodoIncendiado;

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
    { EscribirLog(TRADU.i.Traducir("<color=#FF6666>No puedes descansar aquí.</color>")); }


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

      switch (intTipoClima)
      {
        case 1: textClimaTooltip.text = TRADU.i.Traducir("Día ") + numeroTurno + ":\n" + TRADU.i.Traducir("Soleado: +5 Esperanza."); break;
        case 2: textClimaTooltip.text = TRADU.i.Traducir("Día ") + numeroTurno + ":\n" + TRADU.i.Traducir("Ola de Calor: +1 Fatiga. Jornada Libre da +5 Esperanza, otras Tareas Civiles dan -3."); break;
        case 3: textClimaTooltip.text = TRADU.i.Traducir("Día ")   + numeroTurno + ":\n" + TRADU.i.Traducir("Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% chances de Emboscada."); break;
        case 4: textClimaTooltip.text = TRADU.i.Traducir("Día ")   + numeroTurno + ":\n" + TRADU.i.Traducir("Nieve: +3 Esperanza. -15% Recolecciones, -20% Emboscada. Viajar lleva el doble de tiempo."); break;
        case 5: textClimaTooltip.text = TRADU.i.Traducir("Día ")   + numeroTurno + ":\n" + TRADU.i.Traducir("Niebla: -20% Recolecciones, -20% Emboscada, -20% Exploración, +10% Nodos Misteriosos."); break;
        case 6: textClimaTooltip.text = TRADU.i.Traducir("Día ")   + numeroTurno + ":\n" + TRADU.i.Traducir("Almas Danzantes: +5 Esperanza, -100% chances de Emboscada."); break;
        case 7: textClimaTooltip.text = TRADU.i.Traducir("Día ")   + numeroTurno + ":\n" + TRADU.i.Traducir("Aurora Boreal: +10 Esperanza."); break;
        case 8: textClimaTooltip.text = TRADU.i.Traducir("Día ")   + numeroTurno + ":\n" + TRADU.i.Traducir("Nedukazal está a oscuras."); break;
        case 9: textClimaTooltip.text = TRADU.i.Traducir("Día ")   + numeroTurno + ":\n" + TRADU.i.Traducir("Masacre: Nedukazal está siendo atacada. -10 Esperanza. +10% Emboscada. Los Zúrkil están potenciados."); break;


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

  void Update()
  {
    Nodo nodoActual = scMapaManager != null ? scMapaManager.nodoActual : null;
    bool puedeDescansar = nodoActual != null && nodoActual.nodoDespejado;
    bool asentamientoActivo = asentamientoManager != null && asentamientoManager.TieneInteraccionActiva;

    if (botonDescansar != null)
    {
      botonDescansar.sprite = puedeDescansar ? campSi : campNo;
    }


    //HOTKEYS
    // Detecta cuando se presiona la tecla H una sola vez
    if (Input.GetKeyDown(KeyCode.R))
    {
      if(scTutorialManager.tutorialActivo) {EscribirLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados.")); return; }
      if (asentamientoActivo) { return; }
      AbrirMenuDescanso();
    }
    if (Input.GetKeyDown(KeyCode.C))
    {
       if(scTutorialManager.tutorialActivo) {EscribirLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados.")); return; }
      if (asentamientoActivo) { return; }
      scMenuCaravana.AbrirMenuPersonajes();
    }
    if (Input.GetKeyDown(KeyCode.I))
    {
       if(scTutorialManager.tutorialActivo) {EscribirLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados.")); return; }
      if (asentamientoActivo) { return; }
      scMenuCaravana.AbrirMenuMejoras();
    }
    if (Input.GetKeyDown(KeyCode.M))
    {
       if(scTutorialManager.tutorialActivo) {EscribirLog(TRADU.i.Traducir("Tutorial activo, atajos deshabilitados.")); return; }
      if (asentamientoActivo) { return; }
      scMenuCaravana.AbrirMenuSequitos();
    }
    if (Input.GetKeyDown(KeyCode.F5))
    {
      GuardarCampaniaManual();
    }
    if (Input.GetKeyDown(KeyCode.F9))
    {
      CargarCampaniaManual();
    }
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (asentamientoActivo) { return; }
      if (!scMenuCaravana.SeApretoESC()) //Si se apreto escape se cierran menus, si no habia ningun abierto abre opciones
      {
        if (menuDescanso.activeInHierarchy)
        {
          menuDescanso.SetActive(false);
        }
        else
        {
          MenuOpciones.SetActive(true);
        }

      }



    }


  }



  #region Log
  [SerializeField] TextMeshProUGUI txtLog;
  [SerializeField] GameObject goLog;

  public void ActivarLog(int n)
  {
    if (n == 1)
    {
      goLog.SetActive(true);
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
    logDeCampania.Escribir(logInicio);
    logInicioCampaniaEscrito = true;
  }
  public void EscribirLog(string log, bool forzarAunqueNumeroTurno1 = false)
  {
    if (logDeCampania == null) return;
    if (!forzarAunqueNumeroTurno1 && numeroTurno <= 1) return;

    // Asegura que el logger sabe el día actual
    logDeCampania.SetDiaActual(numeroTurno);
    logDeCampania.Escribir(log);

    GenerarTextoFlotanteCampaña(log, Color.cyan);


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
        TextoFlotanteManager.Instance.GenerarTextoFlotante(tx, col);
      }
      else
      {
        // Manejo local: instancia el prefab directamente
        GameObject goTextoFlotante = Instantiate(prefabTextoCampaña, puntoPantalla.transform, false);

        // Calcula desplazamiento vertical según cuántos spawns recientes hay aún cerca del origen
        float tiempoActual = Time.unscaledTime;
        recentSpawnTimes.RemoveAll(t => tiempoActual - t > stackWindowSeconds);
        int stackIndex = recentSpawnTimes.Count; // 0 para el primero, 1 para el segundo, etc.
        var rt = goTextoFlotante.GetComponent<RectTransform>();
        if (rt != null && stackIndex > 0)
        {
          rt.anchoredPosition += new Vector2(0f, -yStackOffset * stackIndex);
        }

        TextMeshProUGUI txtMesh = goTextoFlotante.GetComponentInChildren<TextMeshProUGUI>();
        if (txtMesh != null)
        {
          txtMesh.text = tx;
          txtMesh.color = col;
        }
      }

      tiempoUltimoSpawnTiempoReal = Time.unscaledTime;
      recentSpawnTimes.Add(tiempoUltimoSpawnTiempoReal);
    }
    procesandoCola = false;
    rutinaTextoFlotanteCampania = null;
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

    pers1.fVidaMaxima = 60 + UnityEngine.Random.Range(1, 7);
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

    pers1.spRetrato = scMenuPersonajes.Male001;

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

    pers1.fVidaMaxima = 48 + UnityEngine.Random.Range(1, 5);
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
    pers1.spRetrato = scMenuPersonajes.Male003;
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

    pers1.fVidaMaxima = 35 + UnityEngine.Random.Range(1, 5);
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
    pers1.spRetrato = scMenuPersonajes.Female001;
    scMenuPersonajes.listaPersonajes.Add(pers1);
    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);



  }
  public void CrearAcechador()
  {

    GameObject acechador = Instantiate(prefabGOPersonaje);

    Personaje pers1 = acechador.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreHombreAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 4;
    pers1.idRetrato = 7;
    pers1.iPuestoDeseado = 2;

    pers1.fVidaMaxima = 52 + UnityEngine.Random.Range(1, 6);
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
    pers1.AddComponent<REPRESENTACIONSueldo>();
    pers1.AddComponent<REPRESENTACIONSigiloso>();
    pers1.AddComponent<TiroBallestaDeMano>();


    //Habilidades Base
    if (!scTutorialManager.tutorialActivo)
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
       pers1.AddComponent<Distraer>(); pers1.GetComponent<Distraer>().NIVEL = 1;
    }

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
    pers1.spRetrato = scMenuPersonajes.Male004;
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

    pers1.fVidaMaxima = 42 + UnityEngine.Random.Range(1, 4);
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
    pers1.AddComponent<REPRESENTACIONSobrecarga>();
    pers1.AddComponent<AcumularEnergia>();
    pers1.AddComponent<DescargaArcana>();


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
    pers1.spRetrato = scMenuPersonajes.Male005;
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

    pers1.fVidaMaxima = 51 + UnityEngine.Random.Range(1, 6);
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

   

    pers1.spRetrato = scMenuPersonajes.Female002;

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
    "Maguie", "Bellezia", "Ava", "Lira", "Joakia", "Sanna", "Robin", "Prisia", "Gillia", "Cadia","Zafira", "Elara", "Fiora", "Lyra", "Nerina", "Selene", "Thalia", "Vespera", "Ysolde", "Zinnia",
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

  }




  #endregion

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
      EscribirLog("<color=#ff9e9e>" + mensajeSinCupo + "</color>", true);
      return false;
    }

    int cantidadAntes = scMenuPersonajes.listaPersonajes.Count;
    int claseElegida;
    if (n == 0) //Es heroe al azar
    {
      // IDs de clase: 1-Caballero, 2-Explorador, 3-Purificadora, 4-Acechador, 5-Canalizador
      List<int> clasesFaltantes = new List<int>();
      for (int i = 1; i <= 5; i++)
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
        claseElegida = UnityEngine.Random.Range(1, 6);
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
    return true;
  }


  public Personaje ObtenerPersonajeAleatorio(List<Personaje> excluidos = null, int IDClasePrioritaria = -1)
  {
    if (scMenuPersonajes.listaPersonajes.Count == 0)
    {
      throw new InvalidOperationException("No hay personajes disponibles.");
    }

    List<Personaje> personajesDisponibles = scMenuPersonajes.listaPersonajes;

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

    int index = UnityEngine.Random.Range(0, personajesDisponibles.Count);
    return personajesDisponibles[index];
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
  public void IncrementarDificultadSegunPeligroRegion(int peligro)
  {

    var hd = Sistema.HandicapDificultad.Instance;
    if (hd != null)
    {
      hd.puntosExtraEnemigos -= 2 * peligrozonaanterior;

      hd.puntosExtraEnemigos += 2 * peligro;
    }


    peligrozonaanterior = peligro;
  }


  public void AplicarEfectosMejorasPuerto()
  {
    //Templo
    int templo = MetaprogresionManager.Instance.SerriaTierTemplo;
    CambiarValorAlientoNegro(-templo);
    List<Personaje> personajesDisponibles = scMenuPersonajes.listaPersonajes;
    foreach (Personaje personaje in personajesDisponibles)
    {
      if (personaje.IDClase == 3) //Purificadora
      {
        personaje.RecibirExperiencia(100 * templo);
      }
    }
    if (templo > 0)
    { EscribirLog(TRADU.i.Traducir("-Las oraciones de los Purificadores del Templo de Serria merman el avance del Aliento Negro en: " + templo + "")); }
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

