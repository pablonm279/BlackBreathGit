using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Stopwatch = System.Diagnostics.Stopwatch;



public class BattleManager : MonoBehaviour
{
  [Header("Ajustes visuales de batalla")]
  [SerializeField] public float TAMANIO_UNIDADES = 1f;
  [Header("Compensacion de perspectiva")]
  [SerializeField] private bool compensarPerspectivaPorPosY = true;
  [SerializeField] private float escalaUnidadBaseEnY1 = 1f;
  [SerializeField] private float escalaUnidadPorPasoY = 0.12f;
  [SerializeField] private float escalaUnidadMaximaPorPosY = 1.35f;
  [Header("Debug mouse")]
  [SerializeField] private KeyCode teclaDebugBajoMouse = KeyCode.F10;
  [SerializeField] private int maxHitsDebugBajoMouse = 12;

  public TutorialCombate scTutorialCombate;

  [SerializeField] public Image retratoPers;
  public GameObject prefabUnidad;
  public GameObject prefabUnidadCaballero;
  public GameObject prefabUnidadExplorador;
  public GameObject prefabUnidadPurificadora;
  public GameObject prefabUnidadAcechador;
  public GameObject prefabUnidadCanalizador;
  public GameObject prefabUnidadDuelista;
  public GameObject prefabOstaculo;
  public GameObject prefabUnidadEnemiga;

  public ContenedorPrefabs contenedorPrefabs;

  public static BattleManager Instance { get; private set; }
  public int RondaNro;
  public Unidad unidadActiva;
  // Silencia logs de combate durante preparaciÃ³n (buffs/estados iniciales)
  public bool silenciarLogCombate = false;

  public GameObject PantallaNegraAcciones;

  public LadoManager ladoA; //Enemigo
  public LadoManager ladoB; //Jugador

  public List<Unidad> lUnidadesTotal = new List<Unidad>();
  public List<Casilla> lCasillasTotal = new List<Casilla>();

  public List<Unidad> lUnidadesPosiblesHabilidadActiva = new List<Unidad>();
  public List<Obstaculo> lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>();
  private readonly HashSet<Unidad> unidadesConFadeHoverObjetivoHabilidad = new HashSet<Unidad>();
  public event EventHandler OnRondaNueva;
  public event EventHandler OnTurnoNuevo;
  public event Action<float> OnValourGlobalAliadosCambiado;

  private const float UmbralValourMuyAlto = 90f;
  private const float UmbralValourAlto = 70f;
  private const float UmbralValourBajo = 40f;
  private const float UmbralValourMuyBajo = 15f;
  private const float ValourGlobalBasePct = 50f;
  private const float ValourGlobalPctPorPuntoPromedio = 8f;
  private const float TimeScaleNormal = 1f;
  private const float TimeScaleModoRapido = 1.35f;
  private const int DcValourBase = 15;
  private const int DcValourMin = 8;
  private const int DcValourMax = 22;
  private const int MaxHuidasMoralPorRonda = 1;
  private int huidasMoralEstaRonda = 0;
  private float ultimoValourGlobalAliadosPct = -1f;
  [SerializeField, Range(0f, 1f)] private float alphaHoverObjetivoNoAfectado = 0.35f;


  public UIBotonesHabilidades scUIBotonesHab;
  public UIContadorAP scUIContadorAP;
  public UIBarraOrdenTurno scUIBarraOrdenTurno;
  public UIInfoChar scUIInfoChar;


  public GameObject botonConsumibleA;
  public GameObject botonConsumibleB;

  public GameObject UICanvasTurnoJugador;
  public GameObject UICanvasTurnoAI;

  public GameObject UIGOPasarTurno;
  public GameObject UIGOEsforzar;
  public GameObject SeparadorAliados;
  public GameObject SeparadorEnemigos;
  public Image widgetClima;
  public GameObject climaTooltip;
  public TextMeshProUGUI textClimaTooltip;
  [Header("Tooltip Valour Global")]
  public GameObject tooltipValorES;
  public GameObject tooltipValorEN;
  public GameObject tooltipValorPO;
  [SerializeField] private float tooltipValorHoverDelay = 0.25f;
  public TextMeshProUGUI rondaText;
  public TextMeshProUGUI apDisponible;

  public GameObject txtSeleccionaobj;
  public bool bOcupado; //Variable de control de flujo de batalla

  public GameObject nocheLienzo;
  private Coroutine coroutineTooltipValorDelay;
  private bool tooltipValorHoverActivo;
  private readonly Dictionary<int, Vector3> escalaBaseUnidadesBatalla = new Dictionary<int, Vector3>();
  private readonly List<RaycastResult> resultadosRaycastUnidadBajoMouse = new List<RaycastResult>();

  private void OnValidate()
  {
    if (!Application.isPlaying)
    {
      return;
    }

    AplicarTamanioUnidadesEnBatalla();
  }

  public float ObtenerMultiplicadorEscalaPerspectivaUnidad(int posY)
  {
    if (!compensarPerspectivaPorPosY)
    {
      return 1f;
    }

    float multiplicador = escalaUnidadBaseEnY1 + (Mathf.Max(0, posY - 1) * escalaUnidadPorPasoY);
    return Mathf.Min(multiplicador, escalaUnidadMaximaPorPosY);
  }

  public void RefrescarVfxClimaCalor(bool activo)
  {
    Canvas canvasObjetivo = null;
    if (widgetClima != null && widgetClima.canvas != null)
    {
      canvasObjetivo = widgetClima.canvas.rootCanvas != null ? widgetClima.canvas.rootCanvas : widgetClima.canvas;
    }
    else
    {
      canvasObjetivo = GetComponentInChildren<Canvas>(true);
    }

    HeatWaveScreenEffect heatWaveEffect = HeatWaveScreenEffect.Ensure(canvasObjetivo);
    if (heatWaveEffect != null)
    {
      heatWaveEffect.SetEffectActive(activo);
    }
  }

  public void SetClimaUIVisible(bool visible)
  {
    if (widgetClima != null)
    {
      widgetClima.gameObject.SetActive(visible);
    }

    if (!visible && climaTooltip != null)
    {
      UIFadeSlideUtility.Hide(climaTooltip);
    }
  }

  private void Awake()
  {
    if (Instance != null)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    tiltCamaraSeleccionHabilidadActivo = true;
  }

  private void OnDestroy()
  {
    if (Instance != this)
    {
      return;
    }

    pausaPorLogActiva = false;
    pausaManualCombateActiva = false;
    ultimaVisibilidadLogActiva = false;
    CancelarCambioEstadoPausaDelay();
    AplicarEscalaTiempoCombate();
    Instance = null;
  }
  public int indexTurno = 0;

  private void Start()
  {
    ConfigurarMicroAnimacionesUI();

    ArmarListadeCasillastotales();
    var handicapDificultad = GetComponent<Sistema.HandicapDificultad>();
    if (handicapDificultad != null)
    {
      handicapDificultad.AplicarDificultadDesdePlayerPrefs();
    }

    RondaNro = 1;

    if (logDeCampania != null)
      logDeCampania.SetDiaActual(RondaNro, true);

    //OBSTACULOS---------
    #region 

    /* GameObject obst1 = Instantiate(prefabOstaculo);
     obst1.GetComponent<Obstaculo>().oName = "Roca";
     obst1.GetComponent<Obstaculo>().hpMax = 40.0f;
     obst1.GetComponent<Obstaculo>().iDureza = 4.0f;
     obst1.GetComponent<Obstaculo>().bPermiteAtacarDetras = true;
     ladoA.c3x5.PonerObjetoEnCasilla(obst1); //Asi se posicionan

     GameObject obst2 = Instantiate(prefabOstaculo);
     obst2.GetComponent<Obstaculo>().oName = "Roca";
     obst2.GetComponent<Obstaculo>().hpMax = 40.0f;
     obst2.GetComponent<Obstaculo>().iDureza = 4.0f;
     obst2.GetComponent<Obstaculo>().bPermiteAtacarDetras = true;
     ladoA.c3x4.PonerObjetoEnCasilla(obst2); //Asi se posicionan

     GameObject obst3 = Instantiate(prefabOstaculo);
     obst3.GetComponent<Obstaculo>().oName = "Roca";
     obst3.GetComponent<Obstaculo>().hpMax = 40.0f;
     obst3.GetComponent<Obstaculo>().iDureza = 4.0f;
     obst3.GetComponent<Obstaculo>().bPermiteAtacarDetras = true;
     ladoA.c3x3.PonerObjetoEnCasilla(obst3); //Asi se posicionan

    /* GameObject obst4 = Instantiate(prefabOstaculo);
     obst4.GetComponent<Obstaculo>().oName = "Roca";
     obst4.GetComponent<Obstaculo>().hpMax = 40.0f;
     obst4.GetComponent<Obstaculo>().iDureza = 4.0f;
     obst4.GetComponent<Obstaculo>().bPermiteAtacarDetras = true;
     ladoA.c3x1.PonerObjetoEnCasilla(obst4); //Asi se posicionan

     GameObject obst5 = Instantiate(prefabOstaculo);
     obst5.GetComponent<Obstaculo>().oName = "Roca";
     obst5.GetComponent<Obstaculo>().hpMax = 40.0f;
     obst5.GetComponent<Obstaculo>().iDureza = 4.0f;
     obst5.GetComponent<Obstaculo>().bPermiteAtacarDetras = true;
     ladoA.c3x2.PonerObjetoEnCasilla(obst5); //Asi se posicionan*/
    #endregion

    //UNIDADES
    /* #region 
     GameObject unidad2 = Instantiate(prefabUnidadCaballero);
     unidad2.GetComponent<ClaseCaballero>().CrearUnidad(2,"Claude",32,30,4,5,7,3,3,6,3,2,0,1,1,19,0,0,0,4,5,2); //Asi se determinan atributos
     unidad2.GetComponent<ClaseCaballero>().uImage.sprite = contenedorPrefabs.Heroe1;
     unidad2.GetComponent<ClaseCaballero>().ValentiaP_actual = 3;
     unidad2.GetComponent<ClaseCaballero>().PASIVA_Implacable = 5;
     Estados.Aplicar_Sangrado(unidad2.GetComponent<ClaseCaballero>(), 4);
     unidad2.AddComponent<Cortevertical>();
     unidad2.AddComponent<SiguesTu>(); 
     unidad2.AddComponent<HombroConHombro>();
     ladoB.c2x4.PonerObjetoEnCasilla(unidad2); //Asi se posicionan
     unidad2.GetComponent<ClaseCaballero>().ConsumibleA = Instantiate(BattleManager.Instance.contenedorPrefabs.consPocionCuracion); //Asi se ponen consumibles
     unidad2.GetComponent<Unidad>().ComienzoBatallaClase();

     GameObject unidad1 = Instantiate(prefabUnidadCaballero);
     unidad1.GetComponent<ClaseCaballero>().CrearUnidad(1,"Jonan",32,30,4,5,7,3,3,6,3,2,0,1,1,19,0,0,0,4,5,2); //Asi se determinan atributos
     unidad1.GetComponent<ClaseCaballero>().uImage.sprite = contenedorPrefabs.Heroe1;
     unidad1.GetComponent<ClaseCaballero>().ValentiaP_actual = 3;
     unidad1.GetComponent<ClaseCaballero>().PASIVA_Implacable = 4;
     unidad1.AddComponent<SiguesTu>(); 
     unidad1.AddComponent<HombroConHombro>();
     unidad1.GetComponent<HombroConHombro>().NIVEL = 5;
     unidad1.AddComponent<Cortevertical>(); 
     unidad1.AddComponent<PosturaDefensiva>();
     ladoB.c2x5.PonerObjetoEnCasilla(unidad1); //Asi se posicionan
     unidad1.GetComponent<ClaseCaballero>().ConsumibleA = Instantiate(BattleManager.Instance.contenedorPrefabs.consPocionCuracion); //Asi se ponen consumibles
     unidad1.GetComponent<Unidad>().ComienzoBatallaClase();

     GameObject unidad3 = Instantiate(prefabUnidadCaballero);
     unidad3.GetComponent<ClaseCaballero>().CrearUnidad(2,"Alan",32,30,4,5,7,3,3,6,3,2,0,1,1,19,0,0,0,4,5,2); //Asi se determinan atributos
     unidad3.GetComponent<ClaseCaballero>().uImage.sprite = contenedorPrefabs.Heroe1;
     unidad3.GetComponent<ClaseCaballero>().ValentiaP_actual = 3;
     unidad3.GetComponent<ClaseCaballero>().PASIVA_Implacable = 5;
     unidad3.AddComponent<Cortevertical>();
     unidad3.AddComponent<SiguesTu>(); 
     unidad3.AddComponent<HombroConHombro>();
     ladoB.c2x1.PonerObjetoEnCasilla(unidad3); //Asi se posicionan
     unidad3.GetComponent<ClaseCaballero>().ConsumibleA = Instantiate(BattleManager.Instance.contenedorPrefabs.consPocionCuracion); //Asi se ponen consumibles
     unidad3.GetComponent<Unidad>().ComienzoBatallaClase();

  /*   GameObject aliado = Instantiate(prefabUnidad);
     aliado.GetComponent<Unidad>().CrearUnidad(2,"Peter",62,4,4,5,2,2,5,2,5,3,2,0,1,16,0,10,1,8,2,2);
     aliado.GetComponent<Unidad>().uImage.sprite = contenedorPrefabs.Heroe1;
     aliado.GetComponent<Unidad>().estado_veneno = 3;
     aliado.GetComponent<Unidad>().estado_sangrado = 3;
     aliado.AddComponent<AtaqueBasico>();
     aliado.AddComponent<BoladeFuego>();
     ladoB.c3x1.PonerObjetoEnCasilla(aliado);*/


    /*

        GameObject unidad4 = Instantiate(prefabUnidadEnemiga);
        unidad4.GetComponent<Unidad>().CrearUnidad(4,"Goblin 1",20,2,2,3,2,2,5,1,2,4,2,2,4,10,0,0,2,3,2,1);
        unidad4.GetComponent<Unidad>().uImage.sprite = contenedorPrefabs.Goblin1;
        unidad4.AddComponent<IAPatada>();
        unidad4.GetComponent<IAPatada>().pPrioridad = 1;
        ladoA.c2x4.PonerObjetoEnCasilla(unidad4);

        GameObject unidad5 = Instantiate(prefabUnidadEnemiga);
        unidad5.GetComponent<Unidad>().CrearUnidad(4,"Goblin 2",20,2,2,3,2,2,5,1,2,4,2,2,4,10,0,0,2,3,2,1);
        unidad5.GetComponent<Unidad>().uImage.sprite = contenedorPrefabs.Goblin1;
        unidad5.AddComponent<IAPatada>();
        unidad5.GetComponent<IAPatada>().pPrioridad = 1;
        ladoA.c1x4.PonerObjetoEnCasilla(unidad5);

        GameObject unidad6 = Instantiate(prefabUnidadEnemiga);
        unidad6.GetComponent<Unidad>().CrearUnidad(4,"Goblin 3",20,2,2,3,2,2,5,1,2,4,2,2,4,10,0,0,2,3,2,1);
        unidad6.GetComponent<Unidad>().uImage.sprite = contenedorPrefabs.Goblin1;
        unidad6.AddComponent<IAPatada>();
        unidad6.GetComponent<IAPatada>().pPrioridad = 1;
        ladoA.c3x4.PonerObjetoEnCasilla(unidad6);

        #endregion*/

    //TRAMPAS
    /* #region 
     TrampaFuego trmp = ladoB.c2x3.gameObject.AddComponent<TrampaFuego>();
     trmp.Inicializar();
      TrampaFuego trmp1 = ladoA.c2x5.gameObject.AddComponent<TrampaFuego>();
     trmp1.Inicializar();
      TrampaFuego trmp2 = ladoA.c2x3.gameObject.AddComponent<TrampaFuego>();
     trmp2.Inicializar();
      TrampaFuego trmp3 = ladoA.c2x4.gameObject.AddComponent<TrampaFuego>();
     trmp3.Inicializar();
     #endregion*/

    ReordenarTodoPorY();
    AplicarTamanioUnidadesEnBatalla();
    ActualizarAliadosRefUI();
    //  RondaNueva();

  }

  public void AplicarTamanioUnidadBatalla(Unidad unidad)
  {
    if (unidad == null)
    {
      return;
    }

    int id = unidad.GetInstanceID();
    if (!escalaBaseUnidadesBatalla.ContainsKey(id))
    {
      escalaBaseUnidadesBatalla[id] = unidad.transform.localScale;
    }

    float multiplicador = Mathf.Max(0.01f, TAMANIO_UNIDADES);
    unidad.transform.localScale = escalaBaseUnidadesBatalla[id] * multiplicador;
    unidad.CasillaPosicion?.AplicarEscalaPerspectivaUnidad(unidad.gameObject);
  }

  private void AplicarTamanioUnidadesEnBatalla()
  {
    if (lUnidadesTotal != null)
    {
      foreach (Unidad unidad in lUnidadesTotal)
      {
        AplicarTamanioUnidadBatalla(unidad);
      }
    }

    if (lCasillasTotal == null)
    {
      return;
    }

    foreach (Casilla casilla in lCasillasTotal)
    {
      if (casilla == null || casilla.Presente == null)
      {
        continue;
      }

      Unidad unidad = casilla.Presente.GetComponent<Unidad>();
      AplicarTamanioUnidadBatalla(unidad);
    }
  }

  private void ConfigurarMicroAnimacionesUI()
  {
    ultimaVisibilidadLogActiva = goLog != null && goLog.activeInHierarchy;

    if (goLog != null)
    {
      UIFadeSlide animLog = UIFadeSlideUtility.Ensure(goLog);
      if (animLog != null)
      {
        animLog.SetDurations(0.16f, 0.14f);
        animLog.SetOffsets(new Vector2(0f, -16f), new Vector2(0f, -8f));
        animLog.SetFollowMouse(false, Vector2.zero);
      }
    }

    if (climaTooltip != null)
    {
      UIFadeSlide animClima = UIFadeSlideUtility.Ensure(climaTooltip);
      if (animClima != null)
      {
        animClima.SetDurations(0.14f, 0.12f);
        animClima.SetOffsets(new Vector2(0f, -10f), new Vector2(0f, -6f));
        animClima.SetFollowMouse(false, Vector2.zero);
      }
    }

    ConfigurarAnimTooltipValour(tooltipValorES);
    ConfigurarAnimTooltipValour(tooltipValorEN);
    ConfigurarAnimTooltipValour(tooltipValorPO);
  }

  private void ConfigurarAnimTooltipValour(GameObject tooltip)
  {
    if (tooltip == null) { return; }

    UIFadeSlide anim = UIFadeSlideUtility.Ensure(tooltip);
    if (anim != null)
    {
      anim.SetDurations(0.2f, 0.16f);
      anim.SetOffsets(new Vector2(0f, -6f), new Vector2(0f, -4f));
      anim.SetFollowMouse(false, Vector2.zero);
    }
  }
  public void ArrancarTurno() //Arranca el turno de la unidad activa
  {
    if (unidadActiva != null)
    {
      if (retratoPers != null)
      {
        Sprite retratoActual = unidadActiva.uRetrato;
        if (retratoActual == null && unidadActiva.uImage != null)
        {
          retratoActual = unidadActiva.uImage.sprite;
        }

        retratoPers.sprite = retratoActual;
        retratoPers.enabled = retratoActual != null;
      }

      //Control si corresponde a IA o Jugador para activar UI correspondiente
      if (unidadActiva.GetComponent<IAUnidad>() != null)
      {
        if (unidadActiva.GetComponent<Unidad>().CasillaPosicion.lado == 1)
        {
          UIActivarCanvas0Jugadoro1AI(1);//Enemigo
        }
        else
        {
          UIActivarCanvas0Jugadoro1AI(2);//Aliado

        }
      }
      else
      {
        UIActivarCanvas0Jugadoro1AI(0);
      }

      CalcularCasillasAMovimiento();


      /*---*/
      SincronizarHabilidadDestruirObstaculo(unidadActiva);
      SincronizarHabilidadEscapar(unidadActiva);
      ActualizarlistaHabilidades();//dejar aca y abajo, se llama 2 veces

      OnTurnoNuevo?.Invoke(this, EventArgs.Empty);

      unidadActiva.ArrancaTurnoEstaUnidad();
      scUIInfoChar.RefrescarSegunEstadoActual();
      /*---*/
      SincronizarHabilidadDestruirObstaculo(unidadActiva);
      SincronizarHabilidadEscapar(unidadActiva);
      ActualizarlistaHabilidades();//dejar aca y arriba, se llama 2 veces

      indexTurno++;

    }
  }

  [SerializeField] private TextMeshProUGUI textoTurno;
  private bool _isFlashingTurnText;


  public void RevisarAPUnidadActiva()
  {
    if (unidadActiva != null && unidadActiva.GetComponent<IAUnidad>() == null && unidadActiva.ObtenerAPActual() <= 0)
    {
      if (!_isFlashingTurnText && textoTurno != null)
      {
        StartCoroutine(FlashTextoTurnoAlpha());
      }
    }
  }

  private IEnumerator FlashTextoTurnoAlpha()
  {
    if (textoTurno == null)
    {
      _isFlashingTurnText = false;
      yield break;
    }
    _isFlashingTurnText = true;
    Color originalColor = textoTurno.color;
    float flashDuration = 0.3f;
    int flashCount = 8;

    for (int i = 0; i < flashCount; i++)
    {
      // bajar alpha
      Color c = textoTurno.color;
      c.a = 0.25f;
      textoTurno.color = c;
      yield return new WaitForSeconds(flashDuration);
      // restaurar alpha original
      c.a = originalColor.a;
      textoTurno.color = c;
      yield return new WaitForSeconds(flashDuration);
    }
    textoTurno.color = originalColor;
    _isFlashingTurnText = false;
  }





  public void TerminarTurnoManual()
  {
    TerminarTurno(true);
  }

  public void TerminarTurno(bool fueManualConBoton = false) //Termina el turno de la unidad activa
  {
    TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.BattleTurnEnded, gameObject)
      .Add("manual", fueManualConBoton ? 1 : 0)
      .Add("unit", unidadActiva != null ? unidadActiva.uNombre : string.Empty));

    scUIBotonesHab.UIDesactivarBotones();

    unidadActiva.TerminaTurnoEstaUnidad(fueManualConBoton);

    if (indexTurno >= 0 && indexTurno < lUnidadesTotal.Count)
    {
      unidadActiva = lUnidadesTotal[indexTurno];



      ArrancarTurno();
    }
    else
    {
      AcelerarRefuerzosSiLadoSinUnidades();

      RondaNueva();
    }


    if (scTutorialCombate.tutorialCombateActivo && scTutorialCombate.ObtenerPasoActual() == 7)
    {
      scTutorialCombate.SiguientePasoCombate();
    }

  }
  public void RondaNueva() //Finaliza la ronda y se reordenan las unidades segÃºn iniciativa
  {

    RondaNro++;
    huidasMoralEstaRonda = 0;
    silenciarLogCombate = false;
    string rondaInicio = (TRADU.i != null)
      ? TRADU.i.Traducir("==== Ronda ") + RondaNro + TRADU.i.Traducir(" comienza ====")
      : "==== Ronda " + RondaNro + " comienza ====";
    EscribirLog(rondaInicio, false);

    OnRondaNueva?.Invoke(this, EventArgs.Empty);
    ObtenerAdministradorEscenasActual()?.ProcesarTraitsInicioRonda();
    GenerarViasEscapeRondaTres();

    AdministrarListas();
    AcelerarRefuerzosSiLadoSinUnidades();

    AplicarReglasValourGlobalInicioRonda();
    AdministrarListas();

    bool derrotaSinRefuerzos = ladoB != null && ladoB.unidadesLado.Count < 1 && aliadosRefuerzos.Count < 1;
    bool victoriaSinRefuerzos = ladoA != null && ladoA.unidadesLado.Count < 1 && enemigosRefuerzos.Count < 1;
    if (derrotaSinRefuerzos || victoriaSinRefuerzos)
    {
      ChequearFinBatalla();
      return;
    }
    EstablecerOrdenPorIniciativa();

    ActualizarRefuerzosUI();
    ActualizarAliadosRefUI();
    DisminuirDuracionObstaculos(); //Se disminuye la duracion de los obstaculos al final de la ronda

    indexTurno = 0;
    if (lUnidadesTotal.Count > 0)
    { unidadActiva = lUnidadesTotal[indexTurno]; }

    //         print("turno de  "+ unidadActiva.uNombre);

    scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();

    // scUIBarraOrdenTurno.gameObject.transform.GetChild(0).GetComponent<Image>().color = Color.red; //

    // EfectosTrampasenCasillas();//Obsoleto, las trampas persistentes ahora aplican efectos al comenzar turno unidad en su casilla

    ArrancarTurno();

    if (RondaNro > ObtenerDelayRefuerzosEnemigosConTraits())
    {
      AdministrarRefuerzosEnemigos();
    }

    if (RondaNro > delayAliados)
    {
      AdministrarRefuerzosAliados();
    }

    rondaText.text = TRADU.i.Traducir("Ronda") + " " + RondaNro;
    //  BorrarLog();

     if (scTutorialCombate.tutorialCombateActivo && scTutorialCombate.ObtenerPasoActual() == 8)
    {
      scTutorialCombate.SiguientePasoCombate();
    }

  }

  void DisminuirDuracionObstaculos()
  {
    foreach (Casilla cas in ladoA.casillasLado)
    {
      if (cas.Presente != null)
      {
        if (cas.Presente.GetComponent<Obstaculo>() != null)
        {
          cas.Presente.GetComponent<Obstaculo>().ReducirDuracion(1);
        }
      }
    }
    //---
    foreach (Casilla cas in ladoB.casillasLado)
    {
      if (cas.Presente != null)
      {
        if (cas.Presente.GetComponent<Obstaculo>() != null)
        {
          cas.Presente.GetComponent<Obstaculo>().ReducirDuracion(1);
        }
      }
    }
  }

  void GenerarViasEscapeRondaTres()
  {
    if (RondaNro != ObtenerRondaAparicionViasEscape() || ladoB == null || ladoB.casillasLado == null)
    {
      return;
    }

    List<Casilla> casillasLibres = new List<Casilla>();
    List<Casilla> casillasConPersonaje = new List<Casilla>();
    int viasGeneradas = 0;

    foreach (Casilla casilla in ladoB.casillasLado)
    {
      if (!EsCasillaValidaParaViaEscape(casilla, out bool tienePersonaje))
      {
        continue;
      }

      if (tienePersonaje)
      {
        casillasConPersonaje.Add(casilla);
      }
      else
      {
        casillasLibres.Add(casilla);
      }
    }

    for (int i = 0; i < 2; i++)
    {
      Casilla destino = ExtraerCasillaAleatoria(casillasLibres);
      if (destino == null)
      {
        destino = ExtraerCasillaAleatoria(casillasConPersonaje);
      }

      if (destino == null)
      {
        continue;
      }

      TrampaEscape trampa = destino.gameObject.AddComponent<TrampaEscape>();
      trampa.Inicializar();
      viasGeneradas++;
    }

    if (viasGeneradas > 0)
    {
      string mensaje = TRADU.i != null
        ? TRADU.i.Traducir(viasGeneradas == 1
          ? "Se abre 1 vÃ­a de escape en la retaguardia aliada."
          : "Se abren 2 vÃ­as de escape en la retaguardia aliada.")
        : (viasGeneradas == 1
          ? "Se abre 1 vÃ­a de escape en la retaguardia aliada."
          : "Se abren 2 vÃ­as de escape en la retaguardia aliada.");
      EscribirLog(mensaje, false);
    }
  }

  bool EsCasillaValidaParaViaEscape(Casilla casilla, out bool tienePersonaje)
  {
    tienePersonaje = false;
    if (casilla == null || casilla.posX != 1)
    {
      return false;
    }

    if (casilla.GetComponent<Trampa>() != null)
    {
      return false;
    }

    if (casilla.Presente == null)
    {
      return true;
    }

    if (casilla.Presente.GetComponent<Obstaculo>() != null)
    {
      return false;
    }

    if (casilla.Presente.GetComponent<Unidad>() != null)
    {
      tienePersonaje = true;
      return true;
    }

    return false;
  }

  Casilla ExtraerCasillaAleatoria(List<Casilla> casillas)
  {
    if (casillas == null || casillas.Count < 1)
    {
      return null;
    }

    int indice = UnityEngine.Random.Range(0, casillas.Count);
    Casilla seleccionada = casillas[indice];
    casillas.RemoveAt(indice);
    return seleccionada;
  }

  public void ActualizarRefuerzosUI()
  {
    int tiempoRestante = ObtenerDelayRefuerzosEnemigosConTraits() - RondaNro + 1;
    if (tiempoRestante < 0) { tiempoRestante = 0; }
    txtRefuerzosContador.text = "" + enemigosRefuerzos.Count();

    txtRefuerzosTiempo.text = "" + tiempoRestante;
    if (enemigosRefuerzos.Count < 1)
    { goRefuerzos.SetActive(false); }
    else { goRefuerzos.SetActive(true); }
  }

  public List<GameObject> enemigosRefuerzos = new List<GameObject>();
  public int delayRefuerzo = 0; //La cantidad de turnos para que empiecen a aparecer los refuerzos.
  public TextMeshProUGUI txtRefuerzosContador;
  public TextMeshProUGUI txtRefuerzosTiempo;
  public GameObject goRefuerzos;


  public TextMeshProUGUI txtAliadosContador;
  public TextMeshProUGUI txtAliadosRefTiempo;
  public List<GameObject> aliadosRefuerzos = new List<GameObject>();
  public GameObject goAliadosRefuerzos;
  int delayAliados = 1;

  public void ReiniciarEstadoRefuerzos()
  {
    DestruirRefuerzosPendientes(enemigosRefuerzos);
    DestruirRefuerzosPendientes(aliadosRefuerzos);
    enemigosRefuerzos.Clear();
    aliadosRefuerzos.Clear();
    delayRefuerzo = 0;
    delayAliados = 1;
    ActualizarRefuerzosUI();
    ActualizarAliadosRefUI();
  }

  void DestruirRefuerzosPendientes(List<GameObject> listaRefuerzos)
  {
    if (listaRefuerzos == null)
    {
      return;
    }

    foreach (GameObject refuerzo in listaRefuerzos)
    {
      if (refuerzo != null)
      {
        Destroy(refuerzo);
      }
    }
  }

  public void ActualizarAliadosRefUI()
  {
    int tiempoRestante = delayAliados - RondaNro + 1;
    if (tiempoRestante < 0) { tiempoRestante = 0; }
    txtAliadosContador.text = "" + aliadosRefuerzos.Count();
    txtAliadosRefTiempo.text = "" + tiempoRestante;

    if (aliadosRefuerzos.Count < 1)
    { goAliadosRefuerzos.SetActive(false); }
    else { goAliadosRefuerzos.SetActive(true); }
  }
  void AdministrarRefuerzosEnemigos()
  {
    if (enemigosRefuerzos.Count < 1)
    { goRefuerzos.SetActive(false); }
    else
    { goRefuerzos.SetActive(true); }

    int enemigosEnCampo = ladoA.unidadesLado.Count;
    if (enemigosEnCampo > 6)
    {
      delayRefuerzo += 1;
      ActualizarRefuerzosUI();
      return; // No mandar refuerzos si hay mÃ¡s de 6 enemigos
    }

    bool noHayEnemigosVivos = enemigosEnCampo < 1;
    // Si el campo enemigo quedo vacio y hay mas de un refuerzo pendiente, entran 2 juntos.
    // Se mantiene tambien la regla existente para listas largas de refuerzos.
    int cantidadAEnviar = (noHayEnemigosVivos && enemigosRefuerzos.Count > 1) || enemigosRefuerzos.Count > 3 ? 2 : 1;
    for (int i = 0; i < cantidadAEnviar && enemigosRefuerzos.Count > 0; i++)
    {
      bool seEnvio = MandarRefuerzoEnemigo(enemigosRefuerzos[0]);
      if (seEnvio)
      {
        enemigosRefuerzos.RemoveAt(0);
      }
      else
      {
        break;
      }
    }

    ActualizarRefuerzosUI();

  }
  void AdministrarRefuerzosAliados()
  {

    if (aliadosRefuerzos.Count < 1)
    { goAliadosRefuerzos.SetActive(false); }
    else
    { goAliadosRefuerzos.SetActive(true); }

    // Contar aliados en el campo de batalla
    int aliadosEnCampo = ladoB.unidadesLado.Count;
    if (aliadosEnCampo > 5)
    {
      delayAliados += 1;
      ActualizarAliadosRefUI();
      return; // No mandar refuerzos si hay mÃ¡s de 5 aliados
    }

    if (aliadosRefuerzos.Count > 0) // Si hay 3 o menos
    {
      // Mandar un solo refuerzo y quitarlo de la lista
      bool seEnvio = MandarRefuerzoAliado(aliadosRefuerzos[0]);
      if (seEnvio)
      {
        aliadosRefuerzos.RemoveAt(0);
        // Hacer que los aliados solo lleguen cada 2 turnos, desde el 2do turno
        delayAliados += 2;
      }

    }


    ActualizarAliadosRefUI();
  }

  bool MandarRefuerzoEnemigo(GameObject enemigo)
  {
    bool seColoco = false;
    Unidad unidadRefuerzo = enemigo != null ? enemigo.GetComponent<Unidad>() : null;
    if (unidadRefuerzo == null)
    {
      return false;
    }

    if (ladoA.c1x3.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x3.PonerObjetoEnCasillaAnimado(enemigo, 2);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoA.c1x2.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x2.PonerObjetoEnCasillaAnimado(enemigo, 2);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoA.c1x4.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x4.PonerObjetoEnCasillaAnimado(enemigo, 2);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoA.c1x5.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x5.PonerObjetoEnCasillaAnimado(enemigo, 2);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoA.c1x1.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x1.PonerObjetoEnCasillaAnimado(enemigo, 2);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }

    if (!seColoco)
    {
      return false;
    }

    AplicarTamanioUnidadBatalla(unidadRefuerzo);
    string nombreRefuerzoEnemigo = ObtenerNombreTraducidoParaLog(unidadRefuerzo.uNombre);
    string txtSeUnio = TRADU.i != null ? TRADU.i.Traducir(" se ha unido a la batalla. Quedan ") : " se ha unido a la batalla. Quedan ";
    string txtRefuerzosRestantes = TRADU.i != null ? TRADU.i.Traducir(" refuerzos.</color> ") : " refuerzos.</color> ";
    EscribirLog("<color=#d92b08>" + nombreRefuerzoEnemigo + txtSeUnio + (enemigosRefuerzos.Count() - 1) + txtRefuerzosRestantes, false);
    AplicarImpactoValentiaPorRefuerzo(true, unidadRefuerzo);
    AplicarEfectosInicioCombate(unidadRefuerzo);
    return true;
  }
  bool MandarRefuerzoAliado(GameObject enemigo)
  {
    bool seColoco = false;
    Unidad unidadRefuerzo = enemigo != null ? enemigo.GetComponent<Unidad>() : null;
    if (unidadRefuerzo == null)
    {
      return false;
    }

    unidadRefuerzo.entroComoAliado = true;

    if (ladoB.c1x3.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x3.PonerObjetoEnCasillaAnimado(enemigo, 1);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoB.c1x2.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x2.PonerObjetoEnCasillaAnimado(enemigo, 1);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoB.c1x4.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x4.PonerObjetoEnCasillaAnimado(enemigo, 1);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoB.c1x5.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x5.PonerObjetoEnCasillaAnimado(enemigo, 1);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }
    else if (ladoB.c1x1.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x1.PonerObjetoEnCasillaAnimado(enemigo, 1);
      unidadRefuerzo.EstablecerAPActualA(0);
      seColoco = true;
    }

    if (!seColoco)
    {
      return false;
    }

    AplicarTamanioUnidadBatalla(unidadRefuerzo);
    AplicarEfectosInicioCombate(unidadRefuerzo);
    string nombreRefuerzoAliado = ObtenerNombreTraducidoParaLog(unidadRefuerzo.uNombre);
    string txtSeUnio = TRADU.i != null ? TRADU.i.Traducir(" se ha unido a la batalla. Quedan ") : " se ha unido a la batalla. Quedan ";
    string txtRefuerzosRestantes = TRADU.i != null ? TRADU.i.Traducir(" refuerzos.</color> ") : " refuerzos.</color> ";
    EscribirLog("<color=#d92b08>" + nombreRefuerzoAliado + txtSeUnio + (aliadosRefuerzos.Count() - 1) + txtRefuerzosRestantes, false);
    AplicarImpactoValentiaPorRefuerzo(false, unidadRefuerzo);
    return true;
  }

  void AplicarImpactoValentiaPorRefuerzo(bool esRefuerzoEnemigo, Unidad refuerzo)
  {
    if (ladoB == null || ladoB.unidadesLado == null || ladoB.unidadesLado.Count < 1)
    {
      return;
    }

    string nombreRefuerzo = refuerzo != null ? ObtenerNombreTraducidoParaLog(refuerzo.uNombre) : "";
    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    int cambioValentia = esRefuerzoEnemigo ? -1 : 1;
    bool huboImpacto = false;

    foreach (Unidad aliado in ladoB.unidadesLado)
    {
      if (aliado == null || aliado.HP_actual <= 0)
      {
        continue;
      }

      aliado.AjustarValentiaInicialSinLog(cambioValentia, false);
      huboImpacto = true;
    }

    if (!huboImpacto)
    {
      return;
    }

    string mensajeValentia = esRefuerzoEnemigo
      ? (enIngles
        ? "Enemy reinforcement " + nombreRefuerzo + " lowers allied valour (-1 VAL)."
        : "El refuerzo enemigo " + nombreRefuerzo + " reduce la ValentÃ­a aliada (-1 VAL).")
      : (enIngles
        ? "Allied reinforcement " + nombreRefuerzo + " boosts allied valour (+1 VAL)."
        : "El refuerzo aliado " + nombreRefuerzo + " aumenta la ValentÃ­a aliada (+1 VAL).");
    EscribirLog(CombatLogFormatter.EventoValour(mensajeValentia), false);

    if (scUIInfoChar != null)
    {
      scUIInfoChar.RefrescarSegunEstadoActual();
    }

    NotificarCambioValourGlobal();
  }

  public float ObtenerValourGlobalAliadosPctActual()
  {
    List<Unidad> aliadosJugador = ObtenerAliadosJugadorParaValourGlobal();
    if (aliadosJugador.Count < 1)
    {
      return 50f;
    }

    return CalcularValourGlobalAliadosPct(aliadosJugador);
  }

  public void NotificarCambioValourGlobal(bool forzar = false)
  {
    float pctActual = ObtenerValourGlobalAliadosPctActual();
    if (!forzar && Mathf.Abs(pctActual - ultimoValourGlobalAliadosPct) < 0.01f)
    {
      return;
    }

    ultimoValourGlobalAliadosPct = pctActual;
    OnValourGlobalAliadosCambiado?.Invoke(pctActual);
  }

  void AplicarReglasValourGlobalInicioRonda()
  {
    List<Unidad> aliadosJugador = ObtenerAliadosJugadorParaValourGlobal();
    if (aliadosJugador.Count < 1)
    {
      return;
    }

    float valourGlobalPct = CalcularValourGlobalAliadosPct(aliadosJugador);
    int valourMostrado = Mathf.RoundToInt(valourGlobalPct);
    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

    if (valourGlobalPct >= UmbralValourMuyAlto)
    {
      EscribirLog(CombatLogFormatter.EventoValour(enIngles
        ? "Global Valour Very High (" + valourMostrado + "%): allies gain +15% damage and +1 AP this round."
        : "ValentÃ­a global Muy Alta (" + valourMostrado + "%): los aliados ganan +15% daÃ±o y +1 AP esta ronda."));
      AplicarBuffGlobalValourDanio(aliadosJugador);
      AplicarBuffGlobalValourAP(aliadosJugador);
      return;
    }

    if (valourGlobalPct >= UmbralValourAlto)
    {
      EscribirLog(CombatLogFormatter.EventoValour(enIngles
        ? "Global Valour High (" + valourMostrado + "%): allies gain +1 AP this round."
        : "ValentÃ­a global Alta (" + valourMostrado + "%): los aliados ganan +1 AP esta ronda."));
      AplicarBuffGlobalValourAP(aliadosJugador);
      return;
    }

    if (valourGlobalPct < UmbralValourMuyBajo)
    {
      EscribirLog(CombatLogFormatter.EventoValour(enIngles
        ? "Global Valour Very Low (" + valourMostrado + "%): all allies roll Mental Save (DC 15 - current VAL, min 8 max 22). On fail: flee in shame (max 1 per round)."
        : "ValentÃ­a global Muy Baja (" + valourMostrado + "%): todos los aliados tiran TS Mental (DC 15 - VAL actual, mÃ­n 8 mÃ¡x 22). Si fallan: huyen avergonzados (mÃ¡x 1 por ronda)."));
      AplicarChequeoMoralGlobal(aliadosJugador, true);
      return;
    }

    if (valourGlobalPct < UmbralValourBajo)
    {
      EscribirLog(CombatLogFormatter.EventoValour(enIngles
        ? "Global Valour Low (" + valourMostrado + "%): all allies roll Mental Save (DC 15 - current VAL, min 8 max 22). On fail: Doubting 1 round (-10% damage, -1 Defense)."
        : "ValentÃ­a global Baja (" + valourMostrado + "%): todos los aliados tiran TS Mental (DC 15 - VAL actual, mÃ­n 8 mÃ¡x 22). Si fallan: Dudando 1 ronda (-10% daÃ±o, -1 Defensa)."));
      AplicarChequeoMoralGlobal(aliadosJugador, false);
    }
  }

  List<Unidad> ObtenerAliadosJugadorParaValourGlobal()
  {
    List<Unidad> aliados = new List<Unidad>();
    if (ladoB == null || ladoB.unidadesLado == null)
    {
      return aliados;
    }

    foreach (Unidad unidad in ladoB.unidadesLado)
    {
      if (unidad == null || unidad.HP_actual <= 0 || unidad.GetComponent<IAUnidad>() != null || !unidad.gameObject.activeInHierarchy)
      {
        continue;
      }

      aliados.Add(unidad);
    }

    return aliados;
  }

  float CalcularValourGlobalAliadosPct(List<Unidad> aliadosJugador)
  {
    if (aliadosJugador == null || aliadosJugador.Count < 1)
    {
      return 0f;
    }

    float suma = 0f;
    int cantidadValidos = 0;
    foreach (Unidad unidad in aliadosJugador)
    {
      if (unidad == null)
      {
        continue;
      }

      suma += unidad.ValentiaP_actual;
      cantidadValidos++;
    }

    if (cantidadValidos < 1)
    {
      return 50f;
    }

    float promedioValentia = suma / cantidadValidos;
    float pct = ValourGlobalBasePct + (promedioValentia * ValourGlobalPctPorPuntoPromedio);
    return Mathf.Clamp(pct, 0f, 100f);
  }

  bool DebeIgnorarValorGrupalPorTraits(Unidad unidad)
  {
    if (unidad == null || CampaignManager.Instance == null || CampaignManager.Instance.scAdministradorEscenas == null)
    {
      return false;
    }

    return CampaignManager.Instance.scAdministradorEscenas.DebeIgnorarValorGrupalPorLoboSolitario(unidad);
  }

  void AplicarBuffGlobalValourDanio(List<Unidad> aliadosJugador)
  {
    foreach (Unidad aliado in aliadosJugador)
    {
      if (aliado == null || DebeIgnorarValorGrupalPorTraits(aliado)) { continue; }

      RefrescarBuffTemporalValour(aliado, "ValentÃ­a Global Muy Alta", buff =>
      {
        buff.boolfDebufftBuff = true;
        buff.buffDescr = "La moral colectiva desborda. +15% daÃ±o y +1 PA mÃ¡ximo esta ronda.";
        buff.DuracionBuffRondas = 1;
        buff.cantDanioPorcentaje += 15;
      });
    }
  }

  void AplicarBuffGlobalValourAP(List<Unidad> aliadosJugador)
  {
    foreach (Unidad aliado in aliadosJugador)
    {
      if (aliado == null || DebeIgnorarValorGrupalPorTraits(aliado)) { continue; }

      RefrescarBuffTemporalValour(aliado, "ValentÃ­a Global Alta", buff =>
      {
        buff.boolfDebufftBuff = true;
        buff.buffDescr = "La moral colectiva impulsa al grupo. +1 PA mÃ¡ximo esta ronda.";
        buff.DuracionBuffRondas = 1;
        buff.cantAPMax += 1;
      });
    }
  }

  void RefrescarBuffTemporalValour(Unidad unidad, string nombreBuff, Action<Buff> configurarBuff)
  {
    if (unidad == null || string.IsNullOrWhiteSpace(nombreBuff) || configurarBuff == null)
    {
      return;
    }

    // Compat: limpia alias viejos/nuevos para evitar duplicados por cambio de nombre.
    if (nombreBuff == "ValentÃ­a Global Alta" || nombreBuff == "Valentia Global Alta" || nombreBuff == "Valour Global Alto")
    {
      unidad.RemoverBuffNombre("Valour Global Alto");
      unidad.RemoverBuffNombre("ValentÃ­a Global Alta");
      unidad.RemoverBuffNombre("Valentia Global Alta");
    }
    else if (nombreBuff == "ValentÃ­a Global Muy Alta" || nombreBuff == "Valentia Global Muy Alta" || nombreBuff == "Valour Global Muy Alto")
    {
      unidad.RemoverBuffNombre("Valour Global Muy Alto");
      unidad.RemoverBuffNombre("ValentÃ­a Global Muy Alta");
      unidad.RemoverBuffNombre("Valentia Global Muy Alta");
    }

    unidad.RemoverBuffNombre(nombreBuff);

    Buff buff = new Buff();
    buff.buffNombre = nombreBuff;
    // Los efectos de Valentia Global se explican en la UI de la barra global;
    // no deben aparecer como buffs individuales en la UI de unidad.
    buff.esBuffVisibleUI = false;
    buff.suprimeTextoFlotante = true;
    buff.suprimeLogCombate = true;
    buff.ocultarEnBarraVida = true;
    configurarBuff(buff);
    buff.AplicarBuff(unidad);
    Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);
  }

  void AplicarChequeoMoralGlobal(List<Unidad> aliadosJugador, bool modoMuyBajo)
  {
    if (aliadosJugador == null || aliadosJugador.Count < 1)
    {
      return;
    }

    List<Unidad> aliadosSnapshot = new List<Unidad>(aliadosJugador);
    foreach (Unidad aliado in aliadosSnapshot)
    {
      if (aliado == null || aliado.HP_actual <= 0 || !aliado.gameObject.activeInHierarchy || DebeIgnorarValorGrupalPorTraits(aliado))
      {
        continue;
      }

      int dc = Mathf.Clamp(DcValourBase - Mathf.RoundToInt(aliado.ValentiaP_actual), DcValourMin, DcValourMax);
      bool noSeSalva = aliado.TiradaSalvacion(aliado.mod_TSMental, dc, true);
      if (!noSeSalva)
      {
        continue;
      }

      if (modoMuyBajo && huidasMoralEstaRonda < MaxHuidasMoralPorRonda && ProcesarHuidaPorMoral(aliado, dc))
      {
        continue;
      }

      AplicarDebuffDudando(aliado, dc, modoMuyBajo && huidasMoralEstaRonda >= MaxHuidasMoralPorRonda);
    }
  }

  bool ProcesarHuidaPorMoral(Unidad aliado, int dc)
  {
    if (aliado == null)
    {
      return false;
    }

    if (UnidadTieneTrait(aliado, PersonajeTraitCatalog.TraitTenaz))
    {
      bool enInglesTenaz = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool enPortuguesTenaz = TRADU.i != null && TRADU.i.nIdioma == 3;
      string nombreAliadoTenaz = TRADU.i != null ? TRADU.i.Traducir(aliado.uNombre) : aliado.uNombre;
      EscribirLog(CombatLogFormatter.EventoValour(enInglesTenaz
        ? nombreAliadoTenaz + " fails Mental Save (DC " + dc + ") but refuses to flee."
        : enPortuguesTenaz
          ? nombreAliadoTenaz + " falha no TS Mental (DC " + dc + "), mas se recusa a fugir."
          : nombreAliadoTenaz + " falla la TS Mental (DC " + dc + "), pero se rehusa a huir."));
      return false;
    }

    if (!aliado.RetirarseDeBatallaPorMoral())
    {
      return false;
    }

    huidasMoralEstaRonda++;

    AdministradorEscenas admin = ObtenerAdministradorEscenasActual();
    if (admin != null)
    {
      admin.MarcarAvergonzadoDesdeUnidad(aliado);
    }

    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    string nombreAliado = TRADU.i != null ? TRADU.i.Traducir(aliado.uNombre) : aliado.uNombre;
    EscribirLog(CombatLogFormatter.EventoValour(enIngles
      ? nombreAliado + " fails Mental Save (DC " + dc + ") and flees the battle in shame."
      : nombreAliado + " falla la TS Mental (DC " + dc + ") y huye avergonzado de la batalla."));
    return true;
  }

  void AplicarDebuffDudando(Unidad aliado, int dc, bool porLimiteHuida)
  {
    if (aliado == null)
    {
      return;
    }

    aliado.RemoverBuffNombre("Dudando");

    Buff dudando = new Buff();
    dudando.buffNombre = "Dudando";
    dudando.buffDescr = "La moral flaquea por la presiÃ³n del combate.";
    dudando.esBuffVisibleUI = true;
    dudando.suprimeTextoFlotante = true;
    dudando.ocultarEnBarraVida = true;
    dudando.boolfDebufftBuff = false;
    dudando.DuracionBuffRondas = 1;
    dudando.cantDanioPorcentaje -= 10;
    dudando.cantDefensa -= 1;
    dudando.AplicarBuff(aliado);
    Buff buffComponent = ComponentCopier.CopyComponent(dudando, aliado.gameObject);

    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    string nombreAliado = TRADU.i != null ? TRADU.i.Traducir(aliado.uNombre) : aliado.uNombre;
    if (porLimiteHuida)
    {
      EscribirLog(CombatLogFormatter.EventoValour(enIngles
        ? nombreAliado + " fails Mental Save (DC " + dc + "). Doubting applied for 1 round (-10% damage, -1 Defense). Flee limit reached this round."
        : nombreAliado + " falla la TS Mental (DC " + dc + "). Se aplica Dudando por 1 ronda (-10% daÃ±o, -1 Defensa). Se alcanzÃ³ el lÃ­mite de huida en esta ronda."));
    }
    else
    {
      EscribirLog(CombatLogFormatter.EventoValour(enIngles
        ? nombreAliado + " fails Mental Save (DC " + dc + ") and becomes Doubting for 1 round (-10% damage, -1 Defense)."
        : nombreAliado + " falla la TS Mental (DC " + dc + ") y entra en Dudando por 1 ronda (-10% daÃ±o, -1 Defensa)."));
    }
  }

  AdministradorEscenas ObtenerAdministradorEscenasActual()
  {
    if (transform == null || transform.parent == null || transform.parent.parent == null)
    {
      return null;
    }

    return transform.parent.parent.gameObject.GetComponent<AdministradorEscenas>();
  }

  void AplicarEfectosInicioCombate(Unidad u)
  {
    //Aca van los efectos y controles a aplicar que deben recibir los refuerzos.
    //Kale'Tav fuerza
    int repeticiones = CampaignManager.Instance.scAtributosZona.PasoVientoHelado_FuerzaKaleTav;
    for (int i = 0; i < repeticiones; i++)
    {
      if (u.TieneTag("Kale'Tav") && CampaignManager.Instance.scAtributosZona.ID == 2)
      {
        // BUFF ---- AsÃ­ se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Fuerza Kale'Tav";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = -1;
        buff.cantDanioPorcentaje += 10;
        buff.cantHPMax += 10;
        buff.cantTsMental += 1;
        buff.cantTsFortaleza += 1;
        buff.AplicarBuff(u);
        // Agrega el componente Buff al objeto objetivo y asigna la configuraciÃ³n del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }
    }

    //Zarkil Masacre
    if (u.TieneTag("Zarkil") && CampaignManager.Instance.scAtributosZona.ID == 3 && CampaignManager.Instance.intTipoClima == 9)
    {
      // BUFF ---- AsÃ­ se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Masacre Zarkil";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = -1;
      buff.cantCritDado += 2;
      buff.percCritDaño += 20;
      buff.AplicarBuff(u);
      // Agrega el componente Buff al objeto objetivo y asigna la configuraciÃ³n del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
    }

  }

  public List<Casilla> lCasillasMovimiento = new List<Casilla>();
  public List<Casilla> CalcularCasillasAMovimiento()
  {
    lCasillasMovimiento.Clear();

    lCasillasMovimiento = unidadActiva.CasillaPosicion.ObtenerCasillasAlrededorParaMovimiento();


    return lCasillasMovimiento;
  }


  private void ArmarListadeCasillastotales()
  {
    //Arma lista con todas las casillas
    ladoA.ActualizarListaDeCasillasEnLado();
    ladoB.ActualizarListaDeCasillasEnLado();
    lCasillasTotal.AddRange(ladoA.casillasLado);
    lCasillasTotal.AddRange(ladoB.casillasLado);
  }
  public void AdministrarListas()
  {
    ladoA.ActualizarListaDeUnidadesEnLado();
    ladoB.ActualizarListaDeUnidadesEnLado();

    lUnidadesTotal.Clear();
    lUnidadesTotal.AddRange(ladoA.GetComponent<LadoManager>().unidadesLado);
    lUnidadesTotal.AddRange(ladoB.GetComponent<LadoManager>().unidadesLado);
    AplicarTamanioUnidadesEnBatalla();
    NotificarCambioValourGlobal();
  }

  public bool RemoverUnidadDeOrdenTurno(Unidad unidad)
  {
    if (unidad == null || lUnidadesTotal == null)
    {
      return false;
    }

    int indiceUnidad = lUnidadesTotal.IndexOf(unidad);
    if (indiceUnidad < 0)
    {
      return false;
    }

    lUnidadesTotal.RemoveAt(indiceUnidad);

    // Si se elimina una unidad ubicada antes del prÃ³ximo turno, el Ã­ndice debe retroceder
    // para no saltear al siguiente combatiente.
    if (indiceUnidad < indexTurno)
    {
      indexTurno--;
    }

    if (indexTurno < 0)
    {
      indexTurno = 0;
    }
    else if (indexTurno > lUnidadesTotal.Count)
    {
      indexTurno = lUnidadesTotal.Count;
    }

    return true;
  }

  void AcelerarRefuerzosSiLadoSinUnidades()
  {
    if (ladoA != null && ladoA.unidadesLado.Count < 1 && enemigosRefuerzos != null && enemigosRefuerzos.Count > 0)
    {
      if (delayRefuerzo > RondaNro)
      {
        delayRefuerzo = RondaNro;
        ActualizarRefuerzosUI();
      }
    }

    if (ladoB != null && ladoB.unidadesLado.Count < 1 && aliadosRefuerzos != null && aliadosRefuerzos.Count > 0)
    {
      if (delayAliados > RondaNro)
      {
        delayAliados = RondaNro;
        ActualizarAliadosRefUI();
      }
    }
  }
  public void EstablecerOrdenPorIniciativa()
  {

    if (RondaNro < 2)
    {
      foreach (Unidad u in lUnidadesTotal)
      {

        u.TirarIniciativa();

      }
    }
    // Ordena la lista de unidades por iniciativa de mayor a menor
    lUnidadesTotal = lUnidadesTotal.OrderByDescending(u => u.iniciativa_actual).ToList();

  }



  private void ActualizarlistaHabilidades()
  {
    scUIBotonesHab.ActualizarBotonesHabilidad();
    _requiereActualizarBotones = false;

  }

  private void CancelarHabilidadActiva()
  {
    if (HabilidadActiva == null)
    {
      ActualizarVisibilidadIndicadorEsfuerzo();
      return;
    }

    scUIBotonesHab?.DeseleccionarTodas();
    HabilidadActiva = null;
    SeleccionandoObjetivo = false;
    LimpiarCapasCasillas();
    scUIContadorAP?.ResetearCirculos();
    ActualizarVisibilidadIndicadorEsfuerzo();
  }

  private bool _requiereActualizarBotones;
  // Cache para restaurar estado de render de obstÃ¡culos tras sombrear
  private readonly Dictionary<Renderer, (int sortingLayerId, int sortingOrder)> _renderOriginalObstaculos = new Dictionary<Renderer, (int sortingLayerId, int sortingOrder)>();
  private readonly Dictionary<SortingGroup, (int sortingLayerId, int sortingOrder)> _sortingGroupOriginalObstaculos = new Dictionary<SortingGroup, (int sortingLayerId, int sortingOrder)>();
  private readonly Dictionary<Canvas, (bool overrideSorting, int sortingOrder)> _canvasOriginalObstaculos = new Dictionary<Canvas, (bool overrideSorting, int sortingOrder)>();
  private readonly Dictionary<Transform, int> _siblingOriginalObstaculos = new Dictionary<Transform, int>();

  private void SolicitarActualizarBotones()
  {
    _requiereActualizarBotones = true;
  }

  private bool DebeTenerHabilidadDestruirObstaculo(Unidad unidad)
  {
    if (unidad == null)
    {
      return false;
    }

    if (unidad.GetComponent<IAUnidad>() != null)
    {
      return false;
    }

    if (unidad.CasillaPosicion == null)
    {
      return false;
    }

    List<Casilla> casillas = unidad.CasillaPosicion.ObtenerCasillasAlrededor(1);
    foreach (Casilla casilla in casillas)
    {
      if (casilla == null || casilla.Presente == null)
      {
        continue;
      }

      if (casilla.lado != unidad.CasillaPosicion.lado)
      {
        continue;
      }

      Obstaculo obstaculo = casilla.Presente.GetComponent<Obstaculo>();
      if (obstaculo != null && obstaculo.destruiblePorMismoLado)
      {
        return true;
      }
    }

    return false;
  }

  private bool DebeTenerHabilidadEscapar(Unidad unidad)
  {
    if (unidad == null || unidad.CasillaPosicion == null)
    {
      return false;
    }

    if (unidad.CasillaPosicion.lado != 2)
    {
      return false;
    }

    if (UnidadTieneTrait(unidad, PersonajeTraitCatalog.TraitTenaz))
    {
      return false;
    }

    return unidad.CasillaPosicion.GetComponent<TrampaEscape>() != null;
  }

  bool UnidadTieneTrait(Unidad unidad, int traitId)
  {
    if (unidad == null)
    {
      return false;
    }

    AdministradorEscenas admin = ObtenerAdministradorEscenasActual();
    if (admin == null)
    {
      return false;
    }

    Personaje personaje = admin.ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidad);
    return personaje != null && personaje.TieneRasgo(traitId);
  }

  int ObtenerRondaAparicionViasEscape()
  {
    int rondaBase = 3;
    if (AliadosTienenTrait(PersonajeTraitCatalog.TraitTactico))
    {
      return Mathf.Max(1, rondaBase - 1);
    }

    return rondaBase;
  }

  int ObtenerDelayRefuerzosEnemigosConTraits()
  {
    int delayAjustado = delayRefuerzo;
    if (AliadosTienenTrait(PersonajeTraitCatalog.TraitTactico))
    {
      delayAjustado += 1;
    }

    return delayAjustado;
  }

  bool AliadosTienenTrait(int traitId)
  {
    AdministradorEscenas admin = ObtenerAdministradorEscenasActual();
    if (admin == null)
    {
      return false;
    }

    return PersonajeTieneTraitAliadoEnCombate(admin.Personaje1, traitId)
      || PersonajeTieneTraitAliadoEnCombate(admin.Personaje2, traitId)
      || PersonajeTieneTraitAliadoEnCombate(admin.Personaje3, traitId)
      || PersonajeTieneTraitAliadoEnCombate(admin.Personaje4, traitId);
  }

  bool PersonajeTieneTraitAliadoEnCombate(Personaje personaje, int traitId)
  {
    return personaje != null && !personaje.Camp_Muerto && personaje.TieneRasgo(traitId);
  }

  public void SincronizarHabilidadDestruirObstaculo(Unidad unidad)
  {
    if (unidad == null)
    {
      return;
    }

    DestruirObstaculo habilidad = unidad.GetComponent<DestruirObstaculo>();
    bool requiere = DebeTenerHabilidadDestruirObstaculo(unidad);

    if (requiere && habilidad == null)
    {
      habilidad = unidad.gameObject.AddComponent<DestruirObstaculo>();
      habilidad.NIVEL = Mathf.Max(1, habilidad.NIVEL);
      SolicitarActualizarBotones();
    }
    else if (!requiere && habilidad != null)
    {
      if (HabilidadActiva == habilidad)
      {
        CancelarHabilidadActiva();
      }
      Destroy(habilidad);
      SolicitarActualizarBotones();
    }
  }

  public void SincronizarHabilidadEscapar(Unidad unidad)
  {
    if (unidad == null)
    {
      return;
    }

    Escapar habilidad = unidad.GetComponent<Escapar>();
    bool requiere = DebeTenerHabilidadEscapar(unidad);

    if (requiere && habilidad == null)
    {
      habilidad = unidad.gameObject.AddComponent<Escapar>();
      habilidad.NIVEL = Mathf.Max(1, habilidad.NIVEL);
      if (unidad == unidadActiva && scUIBotonesHab != null)
      {
        scUIBotonesHab.ActualizarBotonesHabilidad();
        _requiereActualizarBotones = false;
      }
      else
      {
        SolicitarActualizarBotones();
      }
    }
    else if (!requiere && habilidad != null)
    {
      if (HabilidadActiva == habilidad)
      {
        CancelarHabilidadActiva();
      }
      Destroy(habilidad);
      if (unidad == unidadActiva && scUIBotonesHab != null)
      {
        scUIBotonesHab.ActualizarBotonesHabilidad();
        _requiereActualizarBotones = false;
      }
      else
      {
        SolicitarActualizarBotones();
      }
    }
  }
  public void LimpiarCapasCasillas()
  {

    foreach (Casilla cas in lCasillasTotal)
    {
      cas.DesactivarCapas();
    }

  }
  public bool SeleccionandoObjetivo;
  private Habilidad _habilidadActiva;
  public Habilidad HabilidadActiva
  {
    get => _habilidadActiva;
    set
    {
      if (_habilidadActiva == value)
      {
        return;
      }

      LimpiarFadeHoverObjetivoHabilidad();
      _habilidadActiva?.LimpiarMarcasUnidadesPosibles();
      _habilidadActiva = value;
      ActualizarVisibilidadIndicadorEsfuerzo();
    }
  }

  public void AplicarFadeHoverObjetivoHabilidad(IEnumerable<Unidad> unidadesMantenerVisibles)
  {
    if (!SeleccionandoObjetivo || HabilidadActiva == null)
    {
      LimpiarFadeHoverObjetivoHabilidad();
      return;
    }

    HashSet<Unidad> visibles = new HashSet<Unidad>();
    if (unidadesMantenerVisibles != null)
    {
      foreach (Unidad unidad in unidadesMantenerVisibles)
      {
        if (unidad != null)
        {
          visibles.Add(unidad);
        }
      }
    }

    HashSet<Unidad> nuevasConFade = new HashSet<Unidad>();
    if (lUnidadesPosiblesHabilidadActiva != null)
    {
      foreach (Unidad unidad in lUnidadesPosiblesHabilidadActiva)
      {
        if (unidad == null)
        {
          continue;
        }

        if (visibles.Contains(unidad))
        {
          unidad.AplicarFadeImagenUnidad(0f);
          continue;
        }

        unidad.EstablecerMultiplicadorAlphaVisual(alphaHoverObjetivoNoAfectado);
        nuevasConFade.Add(unidad);
      }
    }

    foreach (Unidad unidad in unidadesConFadeHoverObjetivoHabilidad)
    {
      if (unidad != null && !nuevasConFade.Contains(unidad))
      {
        unidad.AplicarFadeImagenUnidad(0f);
      }
    }

    unidadesConFadeHoverObjetivoHabilidad.Clear();
    foreach (Unidad unidad in nuevasConFade)
    {
      unidadesConFadeHoverObjetivoHabilidad.Add(unidad);
    }
  }

  public void LimpiarFadeHoverObjetivoHabilidad()
  {
    if (unidadesConFadeHoverObjetivoHabilidad.Count == 0)
    {
      return;
    }

    foreach (Unidad unidad in unidadesConFadeHoverObjetivoHabilidad)
    {
      if (unidad != null)
      {
        unidad.AplicarFadeImagenUnidad(0f);
      }
    }

    unidadesConFadeHoverObjetivoHabilidad.Clear();
  }

  private void ActualizarVisibilidadIndicadorEsfuerzo()
  {
    if (UIGOEsforzar == null)
    {
      return;
    }

    bool mostrarEsfuerzo = SeleccionandoObjetivo
      && !bOcupado
      && HabilidadActiva != null
      && HabilidadActiva.seEsforzaria > 0;

    UIGOEsforzar.SetActive(mostrarEsfuerzo);
  }

  public bool DebeRestringirMeleePorInmovilizacion(Unidad unidad, bool habilidadEsMelee)
  {
    return unidad != null && habilidadEsMelee && unidad.estado_inmovil > 0;
  }

  public bool EsObjetivoMeleeAdyacentePermitido(Unidad atacante, object objetivo)
  {
    if (atacante == null || atacante.CasillaPosicion == null || objetivo == null)
    {
      return false;
    }

    Casilla casillaObjetivo = null;
    if (objetivo is Unidad unidadObjetivo)
    {
      casillaObjetivo = unidadObjetivo.CasillaPosicion;
    }
    else if (objetivo is Obstaculo obstaculoObjetivo)
    {
      casillaObjetivo = obstaculoObjetivo.CasillaPosicion;
    }

    if (casillaObjetivo == null)
    {
      return false;
    }

    Casilla casillaAtacante = atacante.CasillaPosicion;
    if (casillaAtacante.posX != 3)
    {
      return false;
    }

    if (casillaObjetivo.lado == casillaAtacante.lado)
    {
      return false;
    }

    if (casillaObjetivo.posX != 3)
    {
      return false;
    }

    return Mathf.Abs(casillaObjetivo.posY - casillaAtacante.posY) <= 1;
  }

  public bool TryFiltrarObjetivosMeleePorInmovilizacion(Unidad atacante, bool habilidadEsMelee, List<object> objetivosOriginales, out List<object> objetivosFiltrados)
  {
    if (!DebeRestringirMeleePorInmovilizacion(atacante, habilidadEsMelee))
    {
      objetivosFiltrados = objetivosOriginales;
      return true;
    }

    if (objetivosOriginales == null || objetivosOriginales.Count == 0)
    {
      objetivosFiltrados = objetivosOriginales;
      return false;
    }

    objetivosFiltrados = new List<object>(objetivosOriginales.Count);
    foreach (object objetivo in objetivosOriginales)
    {
      if (EsObjetivoMeleeAdyacentePermitido(atacante, objetivo))
      {
        objetivosFiltrados.Add(objetivo);
      }
    }

    return objetivosFiltrados.Count > 0;
  }

  public bool TryFiltrarObjetivosHostilesPorProvocacion(Unidad atacante, bool habilidadEsHostil, List<object> objetivosOriginales, out List<object> objetivosFiltrados)
  {
    if (!habilidadEsHostil || atacante == null)
    {
      objetivosFiltrados = objetivosOriginales;
      return true;
    }

    Unidad provocador = atacante.ObtenerProvocadorVigente();
    if (provocador == null)
    {
      objetivosFiltrados = objetivosOriginales;
      return true;
    }

    if (objetivosOriginales == null || objetivosOriginales.Count == 0)
    {
      objetivosFiltrados = objetivosOriginales;
      return false;
    }

    objetivosFiltrados = new List<object>(1);
    foreach (object objetivo in objetivosOriginales)
    {
      if (objetivo is Unidad unidadObjetivo && unidadObjetivo == provocador)
      {
        objetivosFiltrados.Add(unidadObjetivo);
        break;
      }
    }

    return objetivosFiltrados.Count > 0;
  }
  // Casilla clickeada para resolver la habilidad (para VFX con referencia de clic)
  public Casilla casillaClickHabilidad;
  private static readonly KeyCode[] _habilidadHotkeys = new[]
  {
    KeyCode.Alpha1,
    KeyCode.Alpha2,
    KeyCode.Alpha3,
    KeyCode.Alpha4,
    KeyCode.Alpha5,
    KeyCode.Alpha6,
    KeyCode.Alpha7,
    KeyCode.Alpha8,
    KeyCode.Alpha9,
    KeyCode.Alpha0
  };

  private void Update()
  {
    bool tutorialActivo = scTutorialCombate != null && scTutorialCombate.tutorialCombateActivo;
    SincronizarPausaConVisibilidadLog();
    ActualizarFadeHoverObjetivoHabilidadPorMouse();

    if (Input.GetKeyDown(teclaDebugBajoMouse))
    {
      DebugObjetosBajoMouse();
    }

    if (pausaManualCombateActiva && (unidadActiva == null || unidadActiva.GetComponent<IAUnidad>() == null))
    {
      SetPausaManualCombate(false);
    }

    if (Input.GetKeyDown(KeyCode.Space))
    {
      if (!tutorialActivo && !pausaPorLogActiva && unidadActiva != null && unidadActiva.GetComponent<IAUnidad>() != null)
      {
        TogglePausaManualCombate();
        return;
      }

      if (unidadActiva != null && unidadActiva.GetComponent<IAUnidad>() == null && tutorialActivo == false)
      {
        TerminarTurno();
        return;
      }
    }

    if (PausaCombateActiva)
    {
      return;
    }

    // Si el jugador hace clic derecho o ESC mientras hay una habilidad activa, cancelarla
    if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) && (unidadActiva != null && HabilidadActiva != null))
    {
      if (scTutorialCombate.tutorialCombateActivo) { return; }
      CancelarHabilidadActiva();
    }
    else if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (scTutorialCombate.tutorialCombateActivo) { return; }

      AdministradorEscenas administradorEscenas = CampaignManager.Instance != null
        ? CampaignManager.Instance.scAdministradorEscenas
        : null;

      if (administradorEscenas != null)
      {
        administradorEscenas.RefrescarUICompartidaSegunEscena();
        administradorEscenas.abrirOpciones();
      }
    }

    if (Input.GetKeyDown(KeyCode.I))
    {
      scUIInfoChar.BotonInfoenemigos();
    }
    if (Input.GetKeyDown(KeyCode.F))
    {
      ActivarModoRapido(!modoRapidoActivado);
    }
    ManejarHotkeysHabilidadesJugador();

    if (unidadActiva != null && scUIBotonesHab != null && unidadActiva.GetComponent<IAUnidad>() == null)
    {
      bool requiereHabilidad = DebeTenerHabilidadDestruirObstaculo(unidadActiva);
      bool tieneHabilidad = unidadActiva.GetComponent<DestruirObstaculo>() != null;
      if (requiereHabilidad != tieneHabilidad)
      {
        SincronizarHabilidadDestruirObstaculo(unidadActiva);
        scUIBotonesHab.ActualizarBotonesHabilidad();
      }
    }

    if (unidadActiva != null && scUIBotonesHab != null)
    {
      bool requiereEscape = DebeTenerHabilidadEscapar(unidadActiva);
      bool tieneEscape = unidadActiva.GetComponent<Escapar>() != null;
      if (!requiereEscape && tieneEscape)
      {
        SincronizarHabilidadEscapar(unidadActiva);
        if (unidadActiva.GetComponent<IAUnidad>() == null)
        {
          scUIBotonesHab.ActualizarBotonesHabilidad();
        }
      }
    }

    ActualizarVisibilidadIndicadorEsfuerzo();

    // Mostrar AP (sin texto adicional de esfuerzo).
    if (apDisponible != null && unidadActiva != null)
    {
      apDisponible.text = $"{(int)unidadActiva.ObtenerAPActual()}/{(int)unidadActiva.mod_maxAccionP}";
    }
    else if (apDisponible != null)
    {
      apDisponible.text = string.Empty;
    }
  }

  private void LateUpdate()
  {
    FiltrarObjetivosActivosPorProvocacion();

    FiltrarObjetivosActivosMeleePorInmovilizacion();

    SincronizarMarcasHabilidadActiva();

    ActualizarTextoSeleccionObjetivo();
  }

  private void DebugObjetosBajoMouse()
  {
    List<string> lineasDebug = new List<string>();
    Vector3 mousePos = Input.mousePosition;
    lineasDebug.Add("[MouseDebug] Posicion mouse: " + mousePos);

    if (EventSystem.current != null)
    {
      PointerEventData pointerData = new PointerEventData(EventSystem.current)
      {
        position = mousePos
      };

      List<RaycastResult> resultadosUI = new List<RaycastResult>();
      EventSystem.current.RaycastAll(pointerData, resultadosUI);
      lineasDebug.Add("[MouseDebug] UI hits: " + resultadosUI.Count + " | IsPointerOverGameObject: " + EventSystem.current.IsPointerOverGameObject());

      for (int i = 0; i < resultadosUI.Count && i < maxHitsDebugBajoMouse; i++)
      {
        RaycastResult hit = resultadosUI[i];
        lineasDebug.Add("  UI[" + i + "] " + DescribirJerarquia(hit.gameObject) + " | dist=" + hit.distance.ToString("0.###") + " | sort=" + hit.sortingOrder + " | layer=" + LayerMask.LayerToName(hit.gameObject.layer));
      }
    }
    else
    {
      lineasDebug.Add("[MouseDebug] Sin EventSystem activo.");
    }

    Camera cam = Camera.main;
    if (cam == null)
    {
      cam = Camera.allCameras.FirstOrDefault(c => c != null && c.enabled);
    }

    if (cam == null)
    {
      lineasDebug.Add("[MouseDebug] Sin camara activa para raycast 3D.");
      Debug.Log(string.Join("\n", lineasDebug));
      return;
    }

    Ray ray = cam.ScreenPointToRay(mousePos);
    RaycastHit[] hits3D = Physics.RaycastAll(ray, 500f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
      .OrderBy(h => h.distance)
      .ToArray();

    lineasDebug.Add("[MouseDebug] 3D hits: " + hits3D.Length + " | camara: " + cam.name);
    for (int i = 0; i < hits3D.Length && i < maxHitsDebugBajoMouse; i++)
    {
      RaycastHit hit = hits3D[i];
      GameObject go = hit.collider != null ? hit.collider.gameObject : null;
      if (go == null)
      {
        continue;
      }

      lineasDebug.Add(
        "  3D[" + i + "] " + DescribirJerarquia(go)
        + " | collider=" + hit.collider.GetType().Name
        + " | dist=" + hit.distance.ToString("0.###")
        + " | layer=" + LayerMask.LayerToName(go.layer)
        + " | tag=" + go.tag);
    }

    Debug.Log(string.Join("\n", lineasDebug));
  }

  private void ActualizarFadeHoverObjetivoHabilidadPorMouse()
  {
    if (!SeleccionandoObjetivo || HabilidadActiva == null)
    {
      LimpiarFadeHoverObjetivoHabilidad();
      return;
    }

    Unidad unidadBajoMouse = ObtenerUnidadBajoMouse();
    Casilla casillaBajoMouse = unidadBajoMouse != null ? unidadBajoMouse.CasillaPosicion : ObtenerCasillaBajoMouse();
    if (casillaBajoMouse == null)
    {
      LimpiarFadeHoverObjetivoHabilidad();
      return;
    }

    casillaBajoMouse.OnMouseOver();
  }

  private Unidad ObtenerUnidadBajoMouse()
  {
    Vector3 mousePos = Input.mousePosition;
    Unidad unidadPorRect = ObtenerUnidadBajoMousePorRectImagen(mousePos);
    if (unidadPorRect != null)
    {
      return unidadPorRect;
    }

    if (EventSystem.current != null)
    {
      PointerEventData pointerData = new PointerEventData(EventSystem.current)
      {
        position = mousePos
      };

      resultadosRaycastUnidadBajoMouse.Clear();
      EventSystem.current.RaycastAll(pointerData, resultadosRaycastUnidadBajoMouse);
      for (int i = 0; i < resultadosRaycastUnidadBajoMouse.Count; i++)
      {
        GameObject go = resultadosRaycastUnidadBajoMouse[i].gameObject;
        if (go == null)
        {
          continue;
        }

        Unidad unidadUI = go.GetComponentInParent<Unidad>();
        if (unidadUI != null)
        {
          return unidadUI;
        }
      }
    }

    Camera cam = Camera.main;
    if (cam == null)
    {
      cam = Camera.allCameras.FirstOrDefault(c => c != null && c.enabled);
    }

    if (cam == null)
    {
      return null;
    }

    Ray ray = cam.ScreenPointToRay(mousePos);
    RaycastHit[] hits3D = Physics.RaycastAll(ray, 500f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
      .OrderBy(h => h.distance)
      .ToArray();

    for (int i = 0; i < hits3D.Length; i++)
    {
      Collider collider = hits3D[i].collider;
      if (collider == null)
      {
        continue;
      }

      Unidad unidad3D = collider.GetComponentInParent<Unidad>();
      if (unidad3D != null)
      {
        return unidad3D;
      }
    }

    return null;
  }

  private Unidad ObtenerUnidadBajoMousePorRectImagen(Vector3 mousePos)
  {
    if (lUnidadesTotal == null || lUnidadesTotal.Count == 0)
    {
      return null;
    }

    Unidad mejorUnidad = null;
    float mejorDistancia = float.MaxValue;

    foreach (Unidad unidad in lUnidadesTotal)
    {
      if (unidad == null
        || unidad.uImage == null
        || !unidad.gameObject.activeInHierarchy
        || !unidad.uImage.gameObject.activeInHierarchy
        || !unidad.uImage.enabled
        || unidad.EstaOcultoVisualmenteParaJugador())
      {
        continue;
      }

      RectTransform rect = unidad.uImage.rectTransform;
      Camera camaraUI = ObtenerCamaraParaRectTransform(unidad.uImage.canvas);
      if (!RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, camaraUI))
      {
        continue;
      }

      Vector3 centroPantalla = RectTransformUtility.WorldToScreenPoint(camaraUI, rect.TransformPoint(rect.rect.center));
      float distancia = (centroPantalla - mousePos).sqrMagnitude;
      if (distancia < mejorDistancia)
      {
        mejorDistancia = distancia;
        mejorUnidad = unidad;
      }
    }

    return mejorUnidad;
  }

  private Camera ObtenerCamaraParaRectTransform(Canvas canvas)
  {
    if (canvas != null)
    {
      if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
      {
        return null;
      }

      if (canvas.worldCamera != null)
      {
        return canvas.worldCamera;
      }
    }

    Camera cam = Camera.main;
    if (cam == null)
    {
      cam = Camera.allCameras.FirstOrDefault(c => c != null && c.enabled);
    }

    return cam;
  }

  private Casilla ObtenerCasillaBajoMouse()
  {
    Vector3 mousePos = Input.mousePosition;
    Camera cam = Camera.main;
    if (cam == null)
    {
      cam = Camera.allCameras.FirstOrDefault(c => c != null && c.enabled);
    }

    if (cam == null)
    {
      return null;
    }

    Ray ray = cam.ScreenPointToRay(mousePos);
    RaycastHit[] hits3D = Physics.RaycastAll(ray, 500f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
      .OrderBy(h => h.distance)
      .ToArray();

    for (int i = 0; i < hits3D.Length; i++)
    {
      Collider collider = hits3D[i].collider;
      if (collider == null)
      {
        continue;
      }

      Casilla casilla = collider.GetComponentInParent<Casilla>();
      if (casilla != null)
      {
        return casilla;
      }
    }

    return null;
  }

  private static string DescribirJerarquia(GameObject go)
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

  private void FiltrarObjetivosActivosPorProvocacion()
  {
    if (!SeleccionandoObjetivo || HabilidadActiva == null || unidadActiva == null)
    {
      return;
    }

    if (!HabilidadActiva.esHostil)
    {
      return;
    }

    Unidad provocador = unidadActiva.ObtenerProvocadorVigente();
    if (provocador == null)
    {
      return;
    }

    if (lUnidadesPosiblesHabilidadActiva != null)
    {
      lUnidadesPosiblesHabilidadActiva.RemoveAll(unidadObjetivo => unidadObjetivo != provocador);
    }

    if (lObstaculosPosiblesHabilidadActiva != null)
    {
      lObstaculosPosiblesHabilidadActiva.Clear();
    }
  }

  private void FiltrarObjetivosActivosMeleePorInmovilizacion()
  {
    if (!SeleccionandoObjetivo || HabilidadActiva == null || unidadActiva == null)
    {
      return;
    }

    if (HabilidadActiva is CargaDeEstoque)
    {
      return;
    }

    if (!DebeRestringirMeleePorInmovilizacion(unidadActiva, HabilidadActiva.esMelee))
    {
      return;
    }

    if (lUnidadesPosiblesHabilidadActiva != null)
    {
      lUnidadesPosiblesHabilidadActiva.RemoveAll(unidadObjetivo => !EsObjetivoMeleeAdyacentePermitido(unidadActiva, unidadObjetivo));
    }

    if (lObstaculosPosiblesHabilidadActiva != null)
    {
      lObstaculosPosiblesHabilidadActiva.RemoveAll(obstaculoObjetivo => !EsObjetivoMeleeAdyacentePermitido(unidadActiva, obstaculoObjetivo));
    }
  }

  private void ActualizarTextoSeleccionObjetivo()
  {
    if (txtSeleccionaobj == null)
    {
      return;
    }

    TextMeshProUGUI tmp = txtSeleccionaobj.GetComponentInChildren<TextMeshProUGUI>(true);

    if (!SeleccionandoObjetivo || HabilidadActiva == null)
    {
      txtSeleccionaobj.SetActive(false);
      return;
    }

    bool hayObjetivos = (lUnidadesPosiblesHabilidadActiva != null && lUnidadesPosiblesHabilidadActiva.Count > 0) ||
                        (lObstaculosPosiblesHabilidadActiva != null && lObstaculosPosiblesHabilidadActiva.Count > 0);

    txtSeleccionaobj.SetActive(true);

    if (!hayObjetivos)
    {
      if (HabilidadActiva.esHostil)
      {
        if (tmp != null)
        {
          Unidad provocador = unidadActiva != null ? unidadActiva.ObtenerProvocadorVigente() : null;
          if (provocador != null)
          {
            string nombreProvocador = TRADU.i != null ? TRADU.i.Traducir(provocador.uNombre) : provocador.uNombre;
            tmp.text = TRADU.i.Traducir("Provocado: solo puedes atacar a ") + nombreProvocador + ".";
          }
          else
          {
            tmp.text = TRADU.i.Traducir("No hay objetivos al alcance.");
          }
          tmp.color = Color.red;
        }
      }
      else
      {
        txtSeleccionaobj.SetActive(false);
      }
      return;
    }

    if (tmp != null)
    {
      tmp.text = TRADU.i.Traducir("Selecciona un objetivo.");
      tmp.color = HabilidadActiva.esHostil ? Color.red : new Color(0.55f, 0.75f, 1f);
    }
  }

  private void SincronizarMarcasHabilidadActiva()
  {
    if (HabilidadActiva == null)
    {
      return;
    }

    if (SeleccionandoObjetivo)
    {
      HabilidadActiva.SincronizarMarcasUnidadesPosibles();
    }
    else
    {
      HabilidadActiva.LimpiarMarcasUnidadesPosibles();
    }
  }

  private void ManejarHotkeysHabilidadesJugador()
  {
    if (unidadActiva == null || scUIBotonesHab == null)
    {
      return;
    }

    if (unidadActiva.GetComponent<IAUnidad>() != null)
    {
      return;
    }

    for (int i = 0; i < _habilidadHotkeys.Length; i++)
    {
      if (Input.GetKeyDown(_habilidadHotkeys[i]))
      {
        scUIBotonesHab.ActivarHabilidadPorHotkeyIndex(i);
        break;
      }
    }

    if (_requiereActualizarBotones && (HabilidadActiva == null || !SeleccionandoObjetivo))
    {
      _requiereActualizarBotones = false;
      scUIBotonesHab?.ActualizarBotonesHabilidad();
    }
  }


  /* public void OpacarCasillasMelee()
  {
     if (HabilidadActiva.esMelee && unidadActiva.GetComponent<Unidad>().CasillaPosicion.posX == 3)
     {
         LadoManager ladoOpuesto = unidadActiva.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();

         if (!HayUnidadEnColumna(ladoOpuesto, 3))
         {
             OscurecerCasillasEnColumna(ladoOpuesto, 3);
         }
         if (!HayUnidadEnColumna(ladoOpuesto, 2) && !HayUnidadEnColumna(ladoOpuesto, 3))
         {
             OscurecerCasillasEnColumna(ladoOpuesto, 2);
         }
     }
  }
 */
  bool HayUnidadEnColumna(LadoManager lado, int columna)
  {
    foreach (Transform cas in lado.transform)
    {
      Casilla scCas = cas.GetComponent<Casilla>();
      if (scCas.posX == columna && scCas.Presente != null && scCas.Presente.GetComponent<Unidad>())
      {
        return true;
      }
    }
    return false;
  }
  /*
  void OscurecerCasillasEnColumna(LadoManager lado, int columna)
  {
      foreach (Transform cas in lado.transform)
      {
          Casilla scCas = cas.GetComponent<Casilla>();
          if (scCas.posX == columna)
          {
              scCas.ActivarCapaColorNegro();
          }
      }
  }   */

  public void UIActivarCanvas0Jugadoro1AI(int value)
  {
    if (value == 0 && pausaManualCombateActiva)
    {
      SetPausaManualCombate(false);
    }

    bool mostrarSeparadorAliados = value == 0 || value == 2;
    if (SeparadorAliados != null)
    {
      SeparadorAliados.SetActive(mostrarSeparadorAliados);
    }
    if (SeparadorEnemigos != null)
    {
      SeparadorEnemigos.SetActive(value == 1);
    }

    if (value == 0) //Jugador
    {
      UICanvasTurnoJugador.SetActive(true);
      UICanvasTurnoAI.transform.GetChild(0).gameObject.SetActive(false);
      UICanvasTurnoAI.transform.GetChild(1).gameObject.SetActive(false);
      UIGOPasarTurno.SetActive(true);

    }
    else if (value == 1)//Enemigo
    {
      UICanvasTurnoJugador.SetActive(false);
      UICanvasTurnoAI.transform.GetChild(0).gameObject.SetActive(true);
      UICanvasTurnoAI.transform.GetChild(1).gameObject.SetActive(false);
      UIGOPasarTurno.SetActive(false);

    }
    else if (value == 2)//Aliado
    {
      UICanvasTurnoJugador.SetActive(false);
      UICanvasTurnoAI.transform.GetChild(0).gameObject.SetActive(false);
      UICanvasTurnoAI.transform.GetChild(1).gameObject.SetActive(true);
      UIGOPasarTurno.SetActive(false);

    }

  }

  public void ReducirDuracionTrampasDeUnidad(Unidad creador)
  {
    if (creador == null) { return; }

    foreach (Transform trans in ladoA.transform)
    {
      ReduceDuracionTrampasCasillas(trans, creador);
    }
    foreach (Transform trans in ladoB.transform)
    {
      ReduceDuracionTrampasCasillas(trans, creador);
    }
  }

  void ReduceDuracionTrampasCasillas(Transform trans, Unidad creador)
  {
    // Obtener todos los componentes Trampa en el objeto y sus hijos
    Trampa[] trampas = trans.GetComponentsInChildren<Trampa>();

    foreach (Trampa trmp in trampas)
    {
      // Reduce la duracion solo para las trampas del creador
      if (trmp.unidadCreadora == creador)
      {
        trmp.ReducirDuracion(1);
      }
    }
  }

  public GameObject goCamara;

  public bool seTilteo = false;
  [Header("Camara - seleccion de habilidades")]
  [SerializeField, Tooltip("Activa el giro sutil de la camara hacia el lado enemigo al seleccionar un boton de habilidad hostil.")]
  private bool tiltCamaraSeleccionHabilidadActivo = true;
  [SerializeField, Tooltip("Angulo en grados del giro lateral al seleccionar una habilidad hostil. Valores chicos evitan bamboleo.")]
  private float tiltCamaraSeleccionAngulo = 2.4f;
  [SerializeField, Tooltip("Duracion en segundos para entrar al tilt de seleccion.")]
  private float tiltCamaraSeleccionEntradaDuracion = 0.22f;
  [SerializeField, Tooltip("Duracion en segundos para volver del tilt cuando no hay zoom de habilidad integrando la salida.")]
  private float tiltCamaraSeleccionSalidaDuracion = 0.34f;
  [Header("Camara - foco de habilidades")]
  [SerializeField, Tooltip("Activa el zoom/paneo sutil al resolver habilidades.")]
  private bool focoCamaraHabilidadesActivo = true;
  [SerializeField, Tooltip("Si esta activo, las habilidades de IA tambien usan el foco de camara.")]
  private bool focoCamaraIncluyeIA = true;
  [SerializeField, Tooltip("Si esta activo, solo las habilidades hostiles disparan el foco de camara.")]
  private bool focoCamaraSoloHostiles = false;
  [SerializeField, Tooltip("Cambio de Field of View durante el foco. Negativo acerca la camara; positivo la aleja.")]
  private float focoCamaraFovDelta = -2f;
  [SerializeField, Tooltip("Desplazamiento hacia adelante en direccion de la camara durante el foco. Mantener bajo para que sea sutil.")]
  private float focoCamaraAvanceLocal = 0.08f;
  [SerializeField, Tooltip("Cuanto acompania la camara el centro entre usuario y objetivos. 0 no panea; valores altos siguen mas al objetivo.")]
  private float focoCamaraSeguimientoObjetivo = 0.16f;
  [SerializeField, Tooltip("Limite maximo del desplazamiento lateral/vertical del foco, para que la escena no se pierda.")]
  private float focoCamaraDesplazamientoMax = 0.18f;
  [SerializeField, Tooltip("Limite maximo del desplazamiento vertical del foco. Baja este valor si la camara cae demasiado en eje Y.")]
  private float focoCamaraDesplazamientoVerticalMax = 0.1f;
  [SerializeField, Tooltip("Zoom extra por cada columna de distancia en Y entre caster y foco. Valor positivo: suma zoom haciendo el FOV mas negativo.")]
  private float focoCamaraFovDeltaPorDistanciaY = 0.3f;
  [SerializeField, Range(0.2f, 1.5f), Tooltip("Multiplicador de intensidad para habilidades melee. 1 usa el foco base.")]
  private float focoCamaraMeleeMultiplicador = 1f;
  [SerializeField, Range(0.2f, 0.8f), Tooltip("En habilidades de rango, peso del objetivo en el punto de foco. 0.5 enfoca entre caster y objetivo; menor mantiene mas visible al caster.")]
  private float focoCamaraRangoPesoObjetivo = 0.45f;
  [SerializeField, Range(0.2f, 1f), Tooltip("Multiplicador de intensidad para habilidades de rango. Reduce zoom y paneo respecto de melee.")]
  private float focoCamaraRangoMultiplicador = 0.65f;
  [SerializeField, Range(0.2f, 1f), Tooltip("Multiplicador de intensidad para habilidades de area. Valores menores abren mas el encuadre.")]
  private float focoCamaraAreaMultiplicador = 0.55f;
  [SerializeField, Range(0.1f, 1f), Tooltip("Multiplicador de intensidad para habilidades de soporte, buffs o curaciones.")]
  private float focoCamaraSoporteMultiplicador = 0.45f;
  [SerializeField, Range(0f, 0.25f), Tooltip("Reduccion extra de intensidad por cada objetivo adicional. Ayuda a que multiples objetivos no queden fuera de pantalla.")]
  private float focoCamaraReduccionPorObjetivoExtra = 0.08f;
  [SerializeField, Range(0.2f, 1f), Tooltip("Limite minimo del multiplicador aplicado por cantidad de objetivos.")]
  private float focoCamaraMultiplesObjetivosMin = 0.55f;
  [SerializeField, Range(0f, 20f), Tooltip("Giro lateral en Y aplicado durante el foco de habilidad hacia el lado del receptor.")]
  private float focoCamaraGiroYAngulo = 4f;
  [SerializeField, Tooltip("Duracion en segundos de la entrada al foco de habilidad.")]
  private float focoCamaraEntradaDuracion = 0.34f;
  [SerializeField, Tooltip("Duracion en segundos de la vuelta al estado original. Tambien integra la vuelta del tilt si corresponde.")]
  private float focoCamaraSalidaDuracion = 0.42f;
  [SerializeField, Tooltip("Espera en segundos antes de iniciar la vuelta del foco al terminar la habilidad.")]
  private float focoCamaraDelayRetorno = 0f;
  [SerializeField, Tooltip("Activa un refuerzo breve del zoom cuando un impacto critico o muerte ocurre durante el foco.")]
  private bool focoCamaraImpactosImportantesActivo = true;
  [SerializeField, Tooltip("Zoom extra por impacto critico durante el foco. Negativo acerca la camara.")]
  private float focoCamaraCriticoFovDelta = -0.7f;
  [SerializeField, Tooltip("Zoom extra por muerte durante el foco. Negativo acerca la camara.")]
  private float focoCamaraMuerteFovDelta = -1f;
  [SerializeField, Tooltip("Avance extra breve de la camara en criticos o muertes.")]
  private float focoCamaraImpactoAvanceLocal = 0.035f;
  [SerializeField, Tooltip("Duracion en segundos del refuerzo de zoom por critico o muerte.")]
  private float focoCamaraImpactoDuracion = 0.12f;
  [SerializeField, Tooltip("Tiempo minimo entre refuerzos de zoom por impactos importantes.")]
  private float focoCamaraImpactoCooldown = 0.08f;
  private Quaternion rotacionOrigenCamaraLocal = Quaternion.identity;
  private bool rotacionOrigenCamaraInicializada;
  private Coroutine corrutinaTiltCamara;
  private bool retornoTiltCamaraIntegradoEnCurso;
  private Camera componenteCamaraBatalla;
  private Oscilacioncamara oscilacionCamaraBatalla;
  private Vector3 posicionOrigenFocoCamara;
  private float fovOrigenFocoCamara;
  private bool focoCamaraInicializado;
  private bool focoCamaraEnUso;
  private bool retornoFocoCamaraEnCurso;
  private float ultimoRefuerzoImpactoCamara = -999f;
  private int versionFocoCamara;
  private Coroutine corrutinaFocoCamara;
  private Quaternion rotacionObjetivoFocoCamaraLocal = Quaternion.identity;
  public void TiltearCamaraLadoEnemigo(bool cBool)
  {
    if (goCamara == null)
    {
      return;
    }

    InicializarRotacionOrigenCamaraSiHaceFalta();

    if (!tiltCamaraSeleccionHabilidadActivo)
    {
      if (corrutinaTiltCamara != null)
      {
        StopCoroutine(corrutinaTiltCamara);
        corrutinaTiltCamara = null;
      }

      seTilteo = false;
      if (!cBool && !EstaCamaraEnRotacionOrigen())
      {
        goCamara.transform.localRotation = rotacionOrigenCamaraLocal;
      }
      return;
    }

    if (cBool)
    {
      retornoTiltCamaraIntegradoEnCurso = false;
      if (seTilteo)
      {
        return;
      }

      seTilteo = true;
      Quaternion rotacionObjetivo = rotacionOrigenCamaraLocal * Quaternion.Euler(0f, tiltCamaraSeleccionAngulo, 0f);
      IniciarTiltCamara(rotacionObjetivo, 0f, tiltCamaraSeleccionEntradaDuracion);
      return;
    }

    if (retornoTiltCamaraIntegradoEnCurso)
    {
      return;
    }

    if (!seTilteo && EstaCamaraEnRotacionOrigen())
    {
      return;
    }

    seTilteo = false;
    IniciarTiltCamara(rotacionOrigenCamaraLocal, 0f, tiltCamaraSeleccionSalidaDuracion);
  }

  private void InicializarRotacionOrigenCamaraSiHaceFalta()
  {
    if (rotacionOrigenCamaraInicializada || goCamara == null)
    {
      return;
    }

    rotacionOrigenCamaraLocal = goCamara.transform.localRotation;
    rotacionOrigenCamaraInicializada = true;
  }

  private bool EstaCamaraEnRotacionOrigen()
  {
    if (!rotacionOrigenCamaraInicializada || goCamara == null)
    {
      return false;
    }

    return Quaternion.Angle(goCamara.transform.localRotation, rotacionOrigenCamaraLocal) <= 0.08f;
  }

  private void IniciarTiltCamara(Quaternion rotacionObjetivo, float delay, float duracion)
  {
    if (corrutinaTiltCamara != null)
    {
      StopCoroutine(corrutinaTiltCamara);
    }

    corrutinaTiltCamara = StartCoroutine(RotateCameraSmoothly(rotacionObjetivo, delay, duracion));
  }

  private IEnumerator RotateCameraSmoothly(Quaternion targetRotation, float delay, float duration)
  {
    float elapsedTime = 0f;
    duration = Mathf.Max(0.01f, duration);

    if (delay > 0)
    {
      yield return new WaitForSeconds(delay);
    }

    Quaternion initialRotation = goCamara.transform.localRotation;

    while (elapsedTime < duration)
    {
      float t = SuavizadoFocoCamara(elapsedTime / duration);
      goCamara.transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
      elapsedTime += Time.deltaTime;
      yield return null;
    }

    goCamara.transform.localRotation = targetRotation; // Asegura que la rotacion final sea exacta
    corrutinaTiltCamara = null;
  }

  public void EnfocarCamaraHabilidad(Unidad usuario, List<object> objetivos, bool esHostil, bool desdeIA, float intensidad = 1f, bool esMelee = false, bool esArea = false)
  {
    if (!focoCamaraHabilidadesActivo || goCamara == null)
    {
      return;
    }

    if (desdeIA && !focoCamaraIncluyeIA)
    {
      return;
    }

    if (focoCamaraSoloHostiles && !esHostil)
    {
      return;
    }

    InicializarRotacionOrigenCamaraSiHaceFalta();
    InicializarFocoCamaraSiHaceFalta();
    if (componenteCamaraBatalla == null)
    {
      return;
    }

    int cantidadObjetivos = ContarObjetivosFoco(objetivos);
    TipoFocoCamaraHabilidad tipoFoco = DeterminarTipoFocoCamara(esHostil, esMelee, esArea, cantidadObjetivos);
    intensidad = Mathf.Clamp(intensidad, 0.15f, 2f) * ObtenerMultiplicadorTipoFoco(tipoFoco);
    if (cantidadObjetivos > 1)
    {
      float multiplicadorMultiples = Mathf.Max(focoCamaraMultiplesObjetivosMin, 1f - ((cantidadObjetivos - 1) * focoCamaraReduccionPorObjetivoExtra));
      intensidad *= multiplicadorMultiples;
    }

    Vector3 centro = CalcularCentroFocoHabilidad(usuario, objetivos, tipoFoco);
    float distanciaYFoco = CalcularDistanciaYFoco(usuario, objetivos);
    Transform camaraTransform = goCamara.transform;
    Vector3 direccionCentro = centro - posicionOrigenFocoCamara;
    Vector3 paneo = Vector3.ProjectOnPlane(direccionCentro, camaraTransform.forward);
    float paneoMagnitud = Mathf.Min(paneo.magnitude * focoCamaraSeguimientoObjetivo * intensidad, focoCamaraDesplazamientoMax * intensidad);
    Vector3 offsetPaneo = paneo.sqrMagnitude > 0.0001f ? paneo.normalized * paneoMagnitud : Vector3.zero;
    Vector3 offsetAvance = camaraTransform.forward * focoCamaraAvanceLocal * intensidad;
    Vector3 offsetObjetivo = offsetPaneo + offsetAvance;
    float limiteVertical = focoCamaraDesplazamientoVerticalMax * intensidad;
    offsetObjetivo.y = Mathf.Clamp(offsetObjetivo.y, -limiteVertical, limiteVertical);
    float fovDeltaAjustado = focoCamaraFovDelta - (distanciaYFoco * focoCamaraFovDeltaPorDistanciaY);
    float fovObjetivo = Mathf.Clamp(fovOrigenFocoCamara + (fovDeltaAjustado * intensidad), 25f, 80f);
    Quaternion rotacionObjetivo = CalcularRotacionFocoHabilidad(objetivos);

    if (corrutinaTiltCamara != null)
    {
      StopCoroutine(corrutinaTiltCamara);
      corrutinaTiltCamara = null;
    }

    focoCamaraEnUso = true;
    retornoFocoCamaraEnCurso = false;
    IniciarFocoCamara(offsetObjetivo, fovObjetivo, rotacionObjetivo, focoCamaraEntradaDuracion, 0f, false);
  }

  public void ReforzarFocoCamaraImpacto(Unidad causante, Unidad objetivo, bool esCritico, bool causaMuerte)
  {
    if (!focoCamaraImpactosImportantesActivo || !focoCamaraInicializado || !focoCamaraEnUso || goCamara == null || componenteCamaraBatalla == null)
    {
      return;
    }

    if (!esCritico && !causaMuerte)
    {
      return;
    }

    if (Time.time - ultimoRefuerzoImpactoCamara < focoCamaraImpactoCooldown)
    {
      return;
    }

    ultimoRefuerzoImpactoCamara = Time.time;
    float fovDelta = 0f;
    if (esCritico)
    {
      fovDelta += focoCamaraCriticoFovDelta;
    }
    if (causaMuerte)
    {
      fovDelta += focoCamaraMuerteFovDelta;
    }

    float intensidadAvance = (esCritico ? 1f : 0f) + (causaMuerte ? 1.25f : 0f);
    Vector3 offsetObjetivo = ObtenerOffsetFocoCamaraActual() + (goCamara.transform.forward * focoCamaraImpactoAvanceLocal * intensidadAvance);
    float fovMinimo = Mathf.Max(25f, fovOrigenFocoCamara + focoCamaraFovDelta - 2.5f);
    float fovObjetivo = Mathf.Clamp(componenteCamaraBatalla.fieldOfView + fovDelta, fovMinimo, 80f);

    retornoFocoCamaraEnCurso = false;
    IniciarFocoCamara(offsetObjetivo, fovObjetivo, rotacionObjetivoFocoCamaraLocal, focoCamaraImpactoDuracion, 0f, false);
  }

  public void RestaurarCamaraHabilidad()
  {
    if (!focoCamaraInicializado || goCamara == null)
    {
      return;
    }

    if (!focoCamaraEnUso && EstaFocoCamaraEnOrigen())
    {
      return;
    }

    if (retornoFocoCamaraEnCurso)
    {
      return;
    }

    bool integrarRetornoTilt = tiltCamaraSeleccionHabilidadActivo && rotacionOrigenCamaraInicializada && (seTilteo || !EstaCamaraEnRotacionOrigen());
    if (integrarRetornoTilt)
    {
      if (corrutinaTiltCamara != null)
      {
        StopCoroutine(corrutinaTiltCamara);
        corrutinaTiltCamara = null;
      }

      seTilteo = false;
    }

    focoCamaraEnUso = false;
    retornoFocoCamaraEnCurso = true;
    IniciarFocoCamara(Vector3.zero, fovOrigenFocoCamara, rotacionOrigenCamaraLocal, focoCamaraSalidaDuracion, focoCamaraDelayRetorno, integrarRetornoTilt);
  }

  private void InicializarFocoCamaraSiHaceFalta()
  {
    if (focoCamaraInicializado || goCamara == null)
    {
      return;
    }

    componenteCamaraBatalla = goCamara.GetComponent<Camera>();
    oscilacionCamaraBatalla = goCamara.GetComponent<Oscilacioncamara>();
    posicionOrigenFocoCamara = oscilacionCamaraBatalla != null ? oscilacionCamaraBatalla.PosicionBase : goCamara.transform.position;
    fovOrigenFocoCamara = componenteCamaraBatalla != null ? componenteCamaraBatalla.fieldOfView : 52f;
    rotacionObjetivoFocoCamaraLocal = rotacionOrigenCamaraLocal;
    focoCamaraInicializado = true;
  }

  private enum TipoFocoCamaraHabilidad
  {
    Melee,
    Rango,
    Area,
    Soporte
  }

  private TipoFocoCamaraHabilidad DeterminarTipoFocoCamara(bool esHostil, bool esMelee, bool esArea, int cantidadObjetivos)
  {
    if (!esHostil)
    {
      return TipoFocoCamaraHabilidad.Soporte;
    }

    if (esArea || cantidadObjetivos > 1)
    {
      return TipoFocoCamaraHabilidad.Area;
    }

    return esMelee ? TipoFocoCamaraHabilidad.Melee : TipoFocoCamaraHabilidad.Rango;
  }

  private float ObtenerMultiplicadorTipoFoco(TipoFocoCamaraHabilidad tipoFoco)
  {
    switch (tipoFoco)
    {
      case TipoFocoCamaraHabilidad.Melee:
        return focoCamaraMeleeMultiplicador;
      case TipoFocoCamaraHabilidad.Area:
        return focoCamaraAreaMultiplicador;
      case TipoFocoCamaraHabilidad.Soporte:
        return focoCamaraSoporteMultiplicador;
      default:
        return focoCamaraRangoMultiplicador;
    }
  }

  private int ContarObjetivosFoco(List<object> objetivos)
  {
    if (objetivos == null)
    {
      return 0;
    }

    int cantidad = 0;
    foreach (object objetivo in objetivos)
    {
      if (TryObtenerPosicionFoco(objetivo, out _))
      {
        cantidad++;
      }
    }

    return cantidad;
  }

  private float CalcularDistanciaYFoco(Unidad usuario, List<object> objetivos)
  {
    if (usuario == null || usuario.CasillaPosicion == null || objetivos == null)
    {
      return 0f;
    }

    int usuarioY = usuario.CasillaPosicion.posY;
    int cantidad = 0;
    float distanciaAcumulada = 0f;

    foreach (object objetivo in objetivos)
    {
      if (TryObtenerPosYFoco(objetivo, out int objetivoY))
      {
        distanciaAcumulada += Mathf.Abs(objetivoY - usuarioY);
        cantidad++;
      }
    }

    return cantidad > 0 ? distanciaAcumulada / cantidad : 0f;
  }

  private Vector3 CalcularCentroFocoHabilidad(Unidad usuario, List<object> objetivos, TipoFocoCamaraHabilidad tipoFoco)
  {
    Vector3 acumulado = Vector3.zero;
    int cantidad = 0;
    Vector3 objetivosAcumulado = Vector3.zero;
    int cantidadObjetivos = 0;

    if (usuario != null)
    {
      acumulado += usuario.transform.position;
      cantidad++;
    }

    if (objetivos != null)
    {
      foreach (object objetivo in objetivos)
      {
        if (TryObtenerPosicionFoco(objetivo, out Vector3 posicion))
        {
          acumulado += posicion;
          cantidad++;
          objetivosAcumulado += posicion;
          cantidadObjetivos++;
        }
      }
    }

    if (tipoFoco == TipoFocoCamaraHabilidad.Rango && usuario != null && cantidadObjetivos > 0)
    {
      Vector3 centroObjetivos = objetivosAcumulado / cantidadObjetivos;
      return Vector3.Lerp(usuario.transform.position, centroObjetivos, focoCamaraRangoPesoObjetivo);
    }

    if (tipoFoco == TipoFocoCamaraHabilidad.Area && cantidadObjetivos > 0)
    {
      return objetivosAcumulado / cantidadObjetivos;
    }

    return cantidad > 0 ? acumulado / cantidad : posicionOrigenFocoCamara;
  }

  private Quaternion CalcularRotacionFocoHabilidad(List<object> objetivos)
  {
    if (!rotacionOrigenCamaraInicializada || goCamara == null || Mathf.Abs(focoCamaraGiroYAngulo) <= 0.01f)
    {
      return rotacionOrigenCamaraLocal;
    }

    if (!TryCalcularCentroObjetivosFoco(objetivos, out Vector3 centroObjetivos))
    {
      return rotacionOrigenCamaraLocal;
    }

    Transform padreCamara = goCamara.transform.parent;
    Vector3 origenLocal = padreCamara != null ? padreCamara.InverseTransformPoint(posicionOrigenFocoCamara) : posicionOrigenFocoCamara;
    Vector3 objetivoLocal = padreCamara != null ? padreCamara.InverseTransformPoint(centroObjetivos) : centroObjetivos;
    Vector3 deltaLocalOrigen = Quaternion.Inverse(rotacionOrigenCamaraLocal) * (objetivoLocal - origenLocal);
    float direccionLateral = deltaLocalOrigen.x;
    if (Mathf.Abs(direccionLateral) <= 0.01f)
    {
      return rotacionOrigenCamaraLocal;
    }

    float angulo = Mathf.Sign(direccionLateral) * focoCamaraGiroYAngulo;
    return rotacionOrigenCamaraLocal * Quaternion.Euler(0f, angulo, 0f);
  }

  private bool TryCalcularCentroObjetivosFoco(List<object> objetivos, out Vector3 centroObjetivos)
  {
    if (objetivos != null)
    {
      Vector3 acumulado = Vector3.zero;
      int cantidad = 0;
      foreach (object objetivo in objetivos)
      {
        if (TryObtenerPosicionFoco(objetivo, out Vector3 posicion))
        {
          acumulado += posicion;
          cantidad++;
        }
      }

      if (cantidad > 0)
      {
        centroObjetivos = acumulado / cantidad;
        return true;
      }
    }

    centroObjetivos = Vector3.zero;
    return false;
  }

  private bool TryObtenerPosicionFoco(object objetivo, out Vector3 posicion)
  {
    if (objetivo is Unidad unidad)
    {
      posicion = unidad.transform.position;
      return true;
    }

    if (objetivo is Obstaculo obstaculo)
    {
      posicion = obstaculo.transform.position;
      return true;
    }

    if (objetivo is Casilla casilla)
    {
      posicion = casilla.transform.position;
      return true;
    }

    if (objetivo is UnityEngine.Component componente)
    {
      posicion = componente.transform.position;
      return true;
    }

    if (objetivo is GameObject gameObjectObjetivo)
    {
      posicion = gameObjectObjetivo.transform.position;
      return true;
    }

    posicion = Vector3.zero;
    return false;
  }

  private bool TryObtenerPosYFoco(object objetivo, out int posY)
  {
    if (objetivo is Unidad unidad && unidad.CasillaPosicion != null)
    {
      posY = unidad.CasillaPosicion.posY;
      return true;
    }

    if (objetivo is Casilla casilla)
    {
      posY = casilla.posY;
      return true;
    }

    if (objetivo is UnityEngine.Component componente)
    {
      Unidad unidadComponente = componente.GetComponent<Unidad>();
      if (unidadComponente != null && unidadComponente.CasillaPosicion != null)
      {
        posY = unidadComponente.CasillaPosicion.posY;
        return true;
      }

      Casilla casillaComponente = componente.GetComponent<Casilla>();
      if (casillaComponente != null)
      {
        posY = casillaComponente.posY;
        return true;
      }
    }

    if (objetivo is GameObject gameObjectObjetivo)
    {
      Unidad unidadGameObject = gameObjectObjetivo.GetComponent<Unidad>();
      if (unidadGameObject != null && unidadGameObject.CasillaPosicion != null)
      {
        posY = unidadGameObject.CasillaPosicion.posY;
        return true;
      }

      Casilla casillaGameObject = gameObjectObjetivo.GetComponent<Casilla>();
      if (casillaGameObject != null)
      {
        posY = casillaGameObject.posY;
        return true;
      }
    }

    posY = 0;
    return false;
  }

  private void IniciarFocoCamara(Vector3 offsetObjetivo, float fovObjetivo, Quaternion rotacionObjetivo, float duracion, float delay, bool integrarRetornoTilt)
  {
    int version = ++versionFocoCamara;
    retornoTiltCamaraIntegradoEnCurso = integrarRetornoTilt;
    rotacionObjetivoFocoCamaraLocal = rotacionObjetivo;

    if (corrutinaFocoCamara != null)
    {
      StopCoroutine(corrutinaFocoCamara);
    }

    corrutinaFocoCamara = StartCoroutine(AnimarFocoCamara(offsetObjetivo, fovObjetivo, rotacionObjetivo, duracion, delay, version, integrarRetornoTilt));
  }

  private IEnumerator AnimarFocoCamara(Vector3 offsetObjetivo, float fovObjetivo, Quaternion rotacionObjetivo, float duracion, float delay, int version, bool integrarRetornoTilt)
  {
    if (delay > 0f)
    {
      yield return new WaitForSeconds(delay);
    }

    if (version != versionFocoCamara)
    {
      yield break;
    }

    Vector3 offsetInicial = ObtenerOffsetFocoCamaraActual();
    float fovInicial = componenteCamaraBatalla != null ? componenteCamaraBatalla.fieldOfView : fovObjetivo;
    Quaternion rotacionInicial = goCamara != null ? goCamara.transform.localRotation : Quaternion.identity;
    float elapsedTime = 0f;
    duracion = Mathf.Max(0.01f, duracion);

    while (elapsedTime < duracion)
    {
      if (version != versionFocoCamara)
      {
        yield break;
      }

      float t = SuavizadoFocoCamara(elapsedTime / duracion);
      AplicarOffsetFocoCamara(Vector3.Lerp(offsetInicial, offsetObjetivo, t));
      if (componenteCamaraBatalla != null)
      {
        componenteCamaraBatalla.fieldOfView = Mathf.Lerp(fovInicial, fovObjetivo, t);
      }
      if (goCamara != null)
      {
        goCamara.transform.localRotation = Quaternion.Slerp(rotacionInicial, rotacionObjetivo, t);
      }

      elapsedTime += Time.deltaTime;
      yield return null;
    }

    if (version != versionFocoCamara)
    {
      yield break;
    }

    AplicarOffsetFocoCamara(offsetObjetivo);
    if (componenteCamaraBatalla != null)
    {
      componenteCamaraBatalla.fieldOfView = fovObjetivo;
    }
    if (goCamara != null)
    {
      goCamara.transform.localRotation = rotacionObjetivo;
    }
    if (integrarRetornoTilt)
    {
      retornoTiltCamaraIntegradoEnCurso = false;
    }

    if (offsetObjetivo == Vector3.zero && componenteCamaraBatalla != null && Mathf.Abs(fovObjetivo - fovOrigenFocoCamara) <= 0.01f)
    {
      retornoFocoCamaraEnCurso = false;
    }

    corrutinaFocoCamara = null;
  }

  private static float SuavizadoFocoCamara(float t)
  {
    t = Mathf.Clamp01(t);
    return t * t * t * (t * (6f * t - 15f) + 10f);
  }

  private bool EstaFocoCamaraEnOrigen()
  {
    bool offsetEnOrigen = ObtenerOffsetFocoCamaraActual().sqrMagnitude <= 0.0004f;
    bool fovEnOrigen = componenteCamaraBatalla == null || Mathf.Abs(componenteCamaraBatalla.fieldOfView - fovOrigenFocoCamara) <= 0.05f;
    return offsetEnOrigen && fovEnOrigen;
  }

  private Vector3 ObtenerOffsetFocoCamaraActual()
  {
    if (oscilacionCamaraBatalla != null)
    {
      return oscilacionCamaraBatalla.OffsetExterno;
    }

    return goCamara != null ? goCamara.transform.position - posicionOrigenFocoCamara : Vector3.zero;
  }

  private void AplicarOffsetFocoCamara(Vector3 offset)
  {
    if (oscilacionCamaraBatalla != null)
    {
      oscilacionCamaraBatalla.EstablecerOffsetExterno(offset);
      return;
    }

    if (goCamara != null)
    {
      goCamara.transform.position = posicionOrigenFocoCamara + offset;
    }
  }

  public void DesmarcarTodasLasUnidades()
  {
    foreach (Unidad uni in lUnidadesTotal)
    {
      uni.Marcar(0);
      uni.OcultarProbabilidad();
    }


  }

  public void ChequearFinBatalla()
  {

    // Asegurar listas actualizadas antes de chequear victoria/derrota
    // Esto cubre el caso donde entran refuerzos en la misma ronda
    // y aÃºn no se actualizÃ³ la lista de unidades del lado.
    ladoA.ActualizarListaDeUnidadesEnLado();
    ladoB.ActualizarListaDeUnidadesEnLado();
    AcelerarRefuerzosSiLadoSinUnidades();

    bool enemigosSinUnidades = ladoA.unidadesLado.Count < 1 && enemigosRefuerzos.Count < 1;
    bool aliadosSinUnidades = ladoB.unidadesLado.Count < 1 && aliadosRefuerzos.Count < 1;

    //Lado Enemigos
    if (aliadosSinUnidades)
    {
      transform.parent.parent.gameObject.GetComponent<AdministradorEscenas>().FinDeBatalla(0); //PerdiÃ³ jugador
    }
    else if (enemigosSinUnidades)
    {
      transform.parent.parent.gameObject.GetComponent<AdministradorEscenas>().FinDeBatalla(1); //GanÃ³ jugador
    }


  }

  [SerializeField] TextMeshProUGUI txtLog;
  [SerializeField] GameObject goLog;
  public List<string> lineas;
  [SerializeField] private LogDeCampania logDeCampania;
  public TMP_SpriteAsset SpriteAssetCombate => logDeCampania != null ? logDeCampania.SpriteAssetCombate : null;
  private bool pausaPorLogActiva;
  private bool pausaManualCombateActiva;
  private bool ultimaVisibilidadLogActiva;
  private CancellationTokenSource cambioEstadoPausaDelayCts = new CancellationTokenSource();
  private GameObject goTextoPausaCombate;
  private TextMeshProUGUI txtPausaCombate;
  private readonly Dictionary<string, string> cacheNombresTraducidosLog = new Dictionary<string, string>();
  private int idiomaCacheNombresTraducidosLog = int.MinValue;

  public void EscribirLog(string log, bool normalizarNombresActores = true)
  {
    if (logDeCampania == null) return;

    string logNormalizado = normalizarNombresActores
      ? TraducirNombresActoresEnLog(log)
      : log;

    // Asegura que el logger sabe el dia actual
    logDeCampania.SetDiaActual(RondaNro, true);
    logDeCampania.Escribir(logNormalizado, true);
  }

  private string ObtenerNombreTraducidoParaLog(string nombreOriginal)
  {
    if (string.IsNullOrWhiteSpace(nombreOriginal) || TRADU.i == null)
    {
      return nombreOriginal;
    }

    int idiomaActual = TRADU.i.nIdioma;
    if (idiomaActual == TRADU.IdiomaEspanol)
    {
      return nombreOriginal;
    }

    if (idiomaCacheNombresTraducidosLog != idiomaActual)
    {
      cacheNombresTraducidosLog.Clear();
      idiomaCacheNombresTraducidosLog = idiomaActual;
    }

    if (cacheNombresTraducidosLog.TryGetValue(nombreOriginal, out string nombreTraducido))
    {
      return nombreTraducido;
    }

    nombreTraducido = TRADU.i.Traducir(nombreOriginal);
    if (string.IsNullOrWhiteSpace(nombreTraducido))
    {
      nombreTraducido = nombreOriginal;
    }

    cacheNombresTraducidosLog[nombreOriginal] = nombreTraducido;
    return nombreTraducido;
  }

  private string TraducirNombresActoresEnLog(string log)
  {
    if (string.IsNullOrEmpty(log) || TRADU.i == null || TRADU.i.nIdioma == TRADU.IdiomaEspanol)
    {
      return log;
    }

    Dictionary<string, string> reemplazos = new Dictionary<string, string>();

    void RegistrarNombre(string nombreOriginal)
    {
      if (string.IsNullOrWhiteSpace(nombreOriginal))
      {
        return;
      }

      string nombreTraducido = ObtenerNombreTraducidoParaLog(nombreOriginal);
      if (string.IsNullOrWhiteSpace(nombreTraducido) || nombreTraducido == nombreOriginal)
      {
        return;
      }

      if (!reemplazos.ContainsKey(nombreOriginal))
      {
        reemplazos.Add(nombreOriginal, nombreTraducido);
      }
    }

    if (lUnidadesTotal != null)
    {
      foreach (Unidad unidad in lUnidadesTotal)
      {
        if (unidad == null) continue;
        RegistrarNombre(unidad.uNombre);
      }
    }

    if (enemigosRefuerzos != null)
    {
      foreach (GameObject refuerzo in enemigosRefuerzos)
      {
        if (refuerzo == null) continue;
        Unidad unidadRefuerzo = refuerzo.GetComponent<Unidad>();
        if (unidadRefuerzo == null) continue;
        RegistrarNombre(unidadRefuerzo.uNombre);
      }
    }

    if (aliadosRefuerzos != null)
    {
      foreach (GameObject refuerzo in aliadosRefuerzos)
      {
        if (refuerzo == null) continue;
        Unidad unidadRefuerzo = refuerzo.GetComponent<Unidad>();
        if (unidadRefuerzo == null) continue;
        RegistrarNombre(unidadRefuerzo.uNombre);
      }
    }

    if (unidadActiva != null)
    {
      RegistrarNombre(unidadActiva.uNombre);
    }

    if (reemplazos.Count == 0)
    {
      return log;
    }

    string logNormalizado = log;
    foreach (string nombreOriginal in reemplazos.Keys.OrderByDescending(n => n.Length))
    {
      string patron = $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(nombreOriginal)}(?![\p{{L}}\p{{N}}])";
      logNormalizado = Regex.Replace(logNormalizado, patron, reemplazos[nombreOriginal]);
    }

    return logNormalizado;
  }

  public void BorrarLog()
  {
    txtLog.text = "";
  }

  public void SetLogCombateActivoPorEscena(bool activo)
  {
    if (goLog == null)
    {
      return;
    }

    GameObject root = ObtenerRootLogCombate();
    Transform logInteractivo = goLog.transform.parent;

    if (activo)
    {
      if (root != null)
      {
        root.SetActive(true);
        if (root.transform.localScale == Vector3.zero)
        {
          root.transform.localScale = Vector3.one;
        }
      }

      if (logInteractivo != null)
      {
        logInteractivo.gameObject.SetActive(true);
      }

      UIFadeSlideUtility.HideImmediate(goLog);
      ultimaVisibilidadLogActiva = false;
    }
    else
    {
      UIFadeSlideUtility.HideImmediate(goLog);
      if (logInteractivo != null)
      {
        logInteractivo.gameObject.SetActive(false);
      }

      if (root != null)
      {
        root.SetActive(false);
      }

      ultimaVisibilidadLogActiva = false;
    }

    SetPausaPorLog(false);
  }

  private GameObject ObtenerRootLogCombate()
  {
    Transform actual = goLog.transform;
    while (actual.parent != null)
    {
      actual = actual.parent;
      if (actual.name.Contains("CanvasLog") || actual.name.Contains("Handbook"))
      {
        return actual.gameObject;
      }
    }

    return goLog.transform.parent != null ? goLog.transform.parent.gameObject : goLog;
  }

  public void ActivarLog(int n)
  {
    if (n == 1)
    {
      UIFadeSlideUtility.Show(goLog);
      ultimaVisibilidadLogActiva = goLog != null && goLog.activeInHierarchy;
      SetPausaPorLog(true);
    }
    else
    {
      UIFadeSlideUtility.Hide(goLog);
      SetPausaPorLog(false);
    }

  }



  public void ActivartooltipClima(int n)
  {
    if (widgetClima == null || !widgetClima.gameObject.activeInHierarchy)
    {
      if (climaTooltip != null)
      {
        UIFadeSlideUtility.Hide(climaTooltip);
      }
      return;
    }

    if (n == 1)
    {
      UIFadeSlideUtility.Show(climaTooltip);

      switch (CampaignManager.Instance.intTipoClima)
      {
        case 1: textClimaTooltip.text = TRADU.i.Traducir("Clima normal."); break;
        case 2: textClimaTooltip.text = TRADU.i.Traducir("Calor: todas las unidades obtienen 'Acalorado'."); break;
        case 3: textClimaTooltip.text = TRADU.i.Traducir("Lluvia: todas las unidades obtienen 'Mojado'."); break;
        case 4: textClimaTooltip.text = TRADU.i.Traducir("Nieve: todas las unidades obtienen 'FrÃ­o'."); break;
        case 5: textClimaTooltip.text = TRADU.i.Traducir("Niebla: -1 Ataque a habilidades de rango."); break;
      }


    }
    else
    {
      UIFadeSlideUtility.Hide(climaTooltip);

    }

  }

  public void AbrirTooltipValor()
  {
    tooltipValorHoverActivo = true;
    if (coroutineTooltipValorDelay != null)
    {
      StopCoroutine(coroutineTooltipValorDelay);
    }

    coroutineTooltipValorDelay = StartCoroutine(AbrirTooltipValorConDelay());
  }

  public void CerrarTooltipValor()
  {
    tooltipValorHoverActivo = false;
    if (coroutineTooltipValorDelay != null)
    {
      StopCoroutine(coroutineTooltipValorDelay);
      coroutineTooltipValorDelay = null;
    }

    if (tooltipValorES != null)
    {
      UIFadeSlideUtility.Hide(tooltipValorES);
    }

    if (tooltipValorEN != null)
    {
      UIFadeSlideUtility.Hide(tooltipValorEN);
    }

    if (tooltipValorPO != null)
    {
      UIFadeSlideUtility.Hide(tooltipValorPO);
    }
  }

  private IEnumerator AbrirTooltipValorConDelay()
  {
    float delay = Mathf.Max(0f, tooltipValorHoverDelay);
    if (delay > 0f)
    {
      yield return new WaitForSecondsRealtime(delay);
    }

    coroutineTooltipValorDelay = null;
    if (!tooltipValorHoverActivo)
    {
      yield break;
    }

    if (tooltipValorES != null)
    {
      UIFadeSlideUtility.Hide(tooltipValorES);
    }

    if (tooltipValorEN != null)
    {
      UIFadeSlideUtility.Hide(tooltipValorEN);
    }

    if (tooltipValorPO != null)
    {
      UIFadeSlideUtility.Hide(tooltipValorPO);
    }

    int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
    GameObject tooltipSeleccionado = null;
    switch (idioma)
    {
      case 1: // EspaÃ±ol
        tooltipSeleccionado = tooltipValorES;
        break;
      case 2: // InglÃ©s
        tooltipSeleccionado = tooltipValorEN;
        break;
      case 3: // PortuguÃ©s
        tooltipSeleccionado = tooltipValorPO;
        break;
      default:
        tooltipSeleccionado = tooltipValorES;
        break;
    }

    if (tooltipSeleccionado != null)
    {
      UIFadeSlideUtility.Show(tooltipSeleccionado);
    }
  }

  public void SombrearANoParticipantesHabilidad(List<object> unidades)
  {
    if (oscurecedor == null)
    {
      return;
    }

    AjustarOscurecedorPantalla();
    oscurecedor.SetActive(true);

    // Primero, asegÃºrate que el oscurecedor estÃ¡ en el lugar correcto en la jerarquÃ­a
    Transform oscurecedorTransform = oscurecedor.transform;
    HashSet<object> unidadesSet = (unidades != null) ? new HashSet<object>(unidades) : new HashSet<object>();
    int ordenOscurecedor = ObtenerOrdenOscurecedor(oscurecedorTransform);
    foreach (Unidad uni in lUnidadesTotal)
    {
      Transform unidadTransform = uni.transform;
      bool esNoParticipante = ListaContieneObjeto(unidadesSet, uni, uni.gameObject);
      int posY = (uni.CasillaPosicion != null) ? uni.CasillaPosicion.posY : 0;
      AplicarOrdenDuranteOscurecedor(uni.gameObject, posY, !esNoParticipante, ordenOscurecedor);
      if (esNoParticipante)
      {
        // Sombrear y poner por encima del oscurecedor
        ConfigurarOscurecidoVisualUnidad(uni, true);

        // Poner por encima del oscurecedor en la jerarquÃ­a
        if (unidadTransform.parent == oscurecedorTransform.parent)
        {
          int oscurecedorIndex = oscurecedorTransform.GetSiblingIndex();
          int maxIndex = unidadTransform.parent.childCount - 1;
          int newIndex = Mathf.Min(oscurecedorIndex + 1, maxIndex);

          unidadTransform.SetSiblingIndex(newIndex);

        }
      }
      else
      {
        // Poner por debajo del oscurecedor en la jerarquÃ­a
        if (unidadTransform.parent == oscurecedorTransform.parent)
        {
          int oscurecedorIndex = oscurecedorTransform.GetSiblingIndex();
          int newIndex = Mathf.Max(oscurecedorIndex - 1, 0);

          unidadTransform.SetSiblingIndex(newIndex);
        }
      }
    }

    foreach (GameObject obstaculoGO in GameObject.FindGameObjectsWithTag("Obstaculo"))
    {
      Obstaculo obstaculo = obstaculoGO.GetComponent<Obstaculo>();
      if (obstaculo == null)
      {
        continue;
      }

      bool sombrearObstaculo = ListaContieneObjeto(unidadesSet, obstaculo, obstaculoGO);
      AjustarObstaculoDuranteSeleccion(obstaculo, sombrearObstaculo, oscurecedorTransform, ordenOscurecedor);
    }
  }

  public GameObject oscurecedor;
  [SerializeField] private float margenExtraOscurecedor = 360f;
  private const int OffsetOscurecedorParticipante = 200;
  private const int OffsetOscurecedorNoParticipante = -200;

  private void AjustarOscurecedorPantalla()
  {
    RectTransform rect = oscurecedor != null ? oscurecedor.GetComponent<RectTransform>() : null;
    if (rect == null || rect.parent == null)
    {
      return;
    }

    float margen = Mathf.Max(0f, margenExtraOscurecedor);
    float zLocal = rect.localPosition.z;

    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.offsetMin = new Vector2(-margen, -margen);
    rect.offsetMax = new Vector2(margen, margen);
    rect.localScale = Vector3.one;
    rect.localRotation = Quaternion.identity;

    Vector3 posicionLocal = rect.localPosition;
    posicionLocal.z = zLocal;
    rect.localPosition = posicionLocal;
  }

  private int CalcularOrdenDuranteOscurecedor(int posY, bool esParticipante, int ordenOscurecedor)
  {
    int baseOrden = RenderOrderHelper.CalcularOrdenPorY(posY);
    int offset = esParticipante ? OffsetOscurecedorParticipante : OffsetOscurecedorNoParticipante;
    return ordenOscurecedor + offset + baseOrden;
  }

  private void AplicarOrdenDuranteOscurecedor(GameObject objetivo, int posY, bool esParticipante, int ordenOscurecedor)
  {
    if (objetivo == null)
    {
      return;
    }

    int ordenFinal = CalcularOrdenDuranteOscurecedor(posY, esParticipante, ordenOscurecedor);
    RenderOrderHelper.AplicarOrdenBase(objetivo, ordenFinal, "UI3D");
  }

  private static void ConfigurarOscurecidoVisualUnidad(Unidad unidad, bool sombrear)
  {
    if (unidad == null)
    {
      return;
    }

    GameObject overlayEscondido = unidad.ObtenerOverlayEscondidoGO();
    if (overlayEscondido != null)
    {
      overlayEscondido.SetActive(sombrear);
    }

    GameObject ojoEscondido = unidad.ObtenerOjoEscondidoGO();
    if (ojoEscondido != null)
    {
      ojoEscondido.SetActive(!sombrear);
    }

    GameObject barraVida = unidad.ObtenerBarraVidaGO();
    if (barraVida != null)
    {
      barraVida.SetActive(!sombrear);
    }
  }

  public void MarcarUnidadComoParticipanteDuranteOscurecedor(Unidad unidad)
  {
    if (unidad == null || oscurecedor == null || !oscurecedor.activeInHierarchy)
    {
      return;
    }

    Transform oscurecedorTransform = oscurecedor.transform;
    int posY = unidad.CasillaPosicion != null ? unidad.CasillaPosicion.posY : 0;
    int ordenOscurecedor = ObtenerOrdenOscurecedor(oscurecedorTransform);
    AplicarOrdenDuranteOscurecedor(unidad.gameObject, posY, true, ordenOscurecedor);
    ConfigurarOscurecidoVisualUnidad(unidad, false);
    AjustarOrdenRespectoOscurecedor(unidad.transform, oscurecedorTransform, false);
  }

  private void ReordenarTodoPorY()
  {
    foreach (Unidad uni in lUnidadesTotal)
    {
      if (uni == null)
      {
        continue;
      }

      int posY = uni.CasillaPosicion != null ? uni.CasillaPosicion.posY : 0;
      RenderOrderHelper.AplicarOrdenPorY(uni.gameObject, posY, "UI3D");
    }

    foreach (GameObject obstaculoGO in GameObject.FindGameObjectsWithTag("Obstaculo"))
    {
      Obstaculo obstaculo = obstaculoGO.GetComponent<Obstaculo>();
      if (obstaculo == null)
      {
        continue;
      }

      int posY = obstaculo.CasillaPosicion != null ? obstaculo.CasillaPosicion.posY : 0;
      RenderOrderHelper.AplicarOrdenPorY(obstaculo.gameObject, posY);
    }
  }
  public void DesombrearANoParticipantesHabilidad(List<object> unidades)
  {
    if (oscurecedor == null)
    {
      return;
    }

    oscurecedor.SetActive(false);


    if (unidades != null)
    {
      foreach (var unidad in unidades)
      {
        if (unidad is Unidad)
        {
          ConfigurarOscurecidoVisualUnidad((Unidad)unidad, false);
        }
        else if (unidad is Obstaculo)
        {
          RestaurarObstaculoTrasDesombrear((Obstaculo)unidad);
        }
        else if (unidad is GameObject && ((GameObject)unidad).CompareTag("Obstaculo"))
        {
          Obstaculo obstaculo = ((GameObject)unidad).GetComponent<Obstaculo>();
          if (obstaculo != null)
          {
            RestaurarObstaculoTrasDesombrear(obstaculo);
          }
        }
      }
    }

    RestaurarTodosLosObstaculosTrasDesombrear();
    ReordenarTodoPorY();

    // Poner el oscurecedor como primer hijo en la jerarquÃ­a
    oscurecedor.transform.SetAsFirstSibling();
  }

  // Pasarela para configurar dificultad desde BattleManager
  public void EstablecerDificultadCombate(int nivel)
  {
    var hd = GetComponent<Sistema.HandicapDificultad>();
    if (hd != null)
    {
      hd.EstablecerDificultadCombate(nivel);
    }
  }

  private static bool ListaContieneObjeto(HashSet<object> objetos, params object[] candidatos)
  {
    if (objetos == null || objetos.Count == 0)
    {
      return false;
    }

    foreach (object candidato in candidatos)
    {
      if (candidato != null && objetos.Contains(candidato))
      {
        return true;
      }
    }

    return false;
  }

  private static void AjustarOrdenRespectoOscurecedor(Transform objetivo, Transform oscurecedorTransform, bool colocarEncima)
  {
    if (objetivo == null || oscurecedorTransform == null)
    {
      return;
    }

    // Si no comparten padre, al menos mueve el objetivo al extremo de su propio padre
    // para que no quede siempre sobre el resto.
    if (objetivo.parent != oscurecedorTransform.parent)
    {
      if (objetivo.parent != null)
      {
        int maxIndex2 = objetivo.parent.childCount - 1;
        int newIndex2 = colocarEncima ? maxIndex2 : 0;
        newIndex2 = Mathf.Clamp(newIndex2, 0, maxIndex2);
        objetivo.SetSiblingIndex(newIndex2);
      }
      return;
    }

    int oscurecedorIndex = oscurecedorTransform.GetSiblingIndex();
    int maxIndex = objetivo.parent.childCount - 1;
    int newIndex = colocarEncima ? Mathf.Min(oscurecedorIndex + 1, maxIndex) : Mathf.Max(oscurecedorIndex - 1, 0);

    objetivo.SetSiblingIndex(newIndex);
  }

  private void AjustarObstaculoDuranteSeleccion(Obstaculo obstaculo, bool sombrear, Transform oscurecedorTransform, int ordenOscurecedor)
  {
    if (obstaculo == null)
    {
      return;
    }

    Transform obstaculoTransform = obstaculo.transform;
    if (!_siblingOriginalObstaculos.ContainsKey(obstaculoTransform) && obstaculoTransform.parent != null)
    {
      _siblingOriginalObstaculos[obstaculoTransform] = obstaculoTransform.GetSiblingIndex();
    }
    // Empuja los obstaculos al fondo de la jerarquia para que no tapen a las unidades ni al oscurecedor
    if (obstaculoTransform.parent != null)
    {
      obstaculoTransform.SetSiblingIndex(0);
    }
    else
    {
      AjustarOrdenRespectoOscurecedor(obstaculoTransform, oscurecedorTransform, false);
    }

    int posY = obstaculo.CasillaPosicion != null ? obstaculo.CasillaPosicion.posY : 0;
    int ordenObstaculo = CalcularOrdenDuranteOscurecedor(posY, !sombrear, ordenOscurecedor);

    AjustarRenderersObstaculo(obstaculoTransform, sombrear, ordenObstaculo, ordenObstaculo);

    Canvas obstaculoCanvas = obstaculoTransform.GetComponentInChildren<Canvas>(true);
    if (obstaculoCanvas != null)
    {
      if (!_canvasOriginalObstaculos.ContainsKey(obstaculoCanvas))
      {
        _canvasOriginalObstaculos[obstaculoCanvas] = (obstaculoCanvas.overrideSorting, obstaculoCanvas.sortingOrder);
      }

      obstaculoCanvas.overrideSorting = true;
      obstaculoCanvas.sortingOrder = ordenObstaculo;

      Transform barraVida = obstaculoCanvas.transform.Find("BarraVida");
      if (barraVida != null)
      {
        barraVida.gameObject.SetActive(!sombrear);
      }
    }
  }

  private int ObtenerOrdenOscurecedor(Transform oscurecedorTransform)
  {
    if (oscurecedorTransform == null)
    {
      return 0;
    }

    SortingGroup sg = oscurecedorTransform.GetComponentInChildren<SortingGroup>(true);
    if (sg != null)
    {
      return sg.sortingOrder;
    }

    Canvas canvas = oscurecedorTransform.GetComponentInChildren<Canvas>(true);
    if (canvas != null)
    {
      return canvas.sortingOrder;
    }

    Renderer rend = oscurecedorTransform.GetComponentInChildren<Renderer>(true);
    if (rend != null)
    {
      return rend.sortingOrder;
    }

    return 0;
  }

  private void AjustarRenderersObstaculo(Transform obstaculoTransform, bool sombrear, int ordenSombreado, int ordenDestacado)
  {
    foreach (SortingGroup sg in obstaculoTransform.GetComponentsInChildren<SortingGroup>(true))
    {
      if (!_sortingGroupOriginalObstaculos.ContainsKey(sg))
      {
        _sortingGroupOriginalObstaculos[sg] = (sg.sortingLayerID, sg.sortingOrder);
      }
      sg.sortingOrder = sombrear ? ordenSombreado : ordenDestacado;
    }

    foreach (Renderer renderer in obstaculoTransform.GetComponentsInChildren<Renderer>(true))
    {
      if (!_renderOriginalObstaculos.ContainsKey(renderer))
      {
        _renderOriginalObstaculos[renderer] = (renderer.sortingLayerID, renderer.sortingOrder);
      }

      renderer.sortingOrder = sombrear ? ordenSombreado : ordenDestacado;
    }
  }

  private void RestaurarTodosLosObstaculosTrasDesombrear()
  {
    foreach (GameObject obstaculoGO in GameObject.FindGameObjectsWithTag("Obstaculo"))
    {
      Obstaculo obstaculo = obstaculoGO.GetComponent<Obstaculo>();
      if (obstaculo == null)
      {
        continue;
      }

      RestaurarObstaculoTrasDesombrear(obstaculo);
    }
  }

  private void RestaurarObstaculoTrasDesombrear(Obstaculo obstaculo)
  {
    if (obstaculo == null)
    {
      return;
    }

    if (_siblingOriginalObstaculos.TryGetValue(obstaculo.transform, out int siblingIndex) && obstaculo.transform.parent != null)
    {
      int maxIndex = Mathf.Max(obstaculo.transform.parent.childCount - 1, 0);
      obstaculo.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, maxIndex));
    }

    bool restauroOrden = RestaurarOrdenObstaculo(obstaculo.transform);

    Canvas obstaculoCanvas = obstaculo.transform.GetComponentInChildren<Canvas>(true);
    if (obstaculoCanvas != null)
    {
      if (_canvasOriginalObstaculos.TryGetValue(obstaculoCanvas, out var original))
      {
        obstaculoCanvas.overrideSorting = original.overrideSorting;
        obstaculoCanvas.sortingOrder = original.sortingOrder;
        restauroOrden = true;
      }
      else
      {
        obstaculoCanvas.overrideSorting = false;
        obstaculoCanvas.sortingOrder = 0;
      }

      Transform barraVida = obstaculoCanvas.transform.Find("BarraVida");
      if (barraVida != null)
      {
        barraVida.gameObject.SetActive(true);
      }
    }

    if (!restauroOrden)
    {
      int posY = obstaculo.CasillaPosicion != null
        ? obstaculo.CasillaPosicion.posY
        : 0;
      RenderOrderHelper.AplicarOrdenPorY(obstaculo.gameObject, posY);
    }
  }

  private bool RestaurarOrdenObstaculo(Transform obstaculoTransform)
  {
    bool restauro = false;
    foreach (SortingGroup sg in obstaculoTransform.GetComponentsInChildren<SortingGroup>(true))
    {
      if (_sortingGroupOriginalObstaculos.TryGetValue(sg, out var original))
      {
        sg.sortingLayerID = original.sortingLayerId;
        sg.sortingOrder = original.sortingOrder;
        restauro = true;
      }
    }

    foreach (Renderer renderer in obstaculoTransform.GetComponentsInChildren<Renderer>(true))
    {
      if (_renderOriginalObstaculos.TryGetValue(renderer, out var original))
      {
        renderer.sortingLayerID = original.sortingLayerId;
        renderer.sortingOrder = original.sortingOrder;
        restauro = true;
      }
    }

    return restauro;
  }

  public bool modoRapidoActivado = false;
  public GameObject btnModoRapido;

  public bool PausaCombateActiva
  {
    get { return pausaPorLogActiva || pausaManualCombateActiva; }
  }

  public static async Task DelayCombateAsync(int milliseconds)
  {
    await DelayCombateAsync(TimeSpan.FromMilliseconds(Mathf.Max(0, milliseconds)));
  }

  public static async Task DelayCombateAsync(TimeSpan duration)
  {
    if (duration <= TimeSpan.Zero)
    {
      return;
    }

    BattleManager battleManager = Instance;
    if (battleManager == null)
    {
      await Task.Delay(duration);
      return;
    }

    double restanteMs = duration.TotalMilliseconds;

    while (restanteMs > 0d)
    {
      battleManager = Instance;
      if (battleManager == null)
      {
        await Task.Delay(TimeSpan.FromMilliseconds(restanteMs));
        return;
      }

      while (battleManager.PausaCombateActiva)
      {
        CancellationToken tokenCambioEstadoPausa = battleManager.ObtenerTokenCambioEstadoPausa();
        try
        {
          await Task.Delay(Timeout.InfiniteTimeSpan, tokenCambioEstadoPausa);
        }
        catch (TaskCanceledException)
        {
        }

        battleManager = Instance;
        if (battleManager == null)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(restanteMs));
          return;
        }
      }

      CancellationToken tokenDelayActiva = battleManager.ObtenerTokenCambioEstadoPausa();
      Stopwatch cronometro = Stopwatch.StartNew();
      try
      {
        await Task.Delay(TimeSpan.FromMilliseconds(restanteMs), tokenDelayActiva);
        return;
      }
      catch (TaskCanceledException)
      {
        restanteMs = Math.Max(0d, restanteMs - cronometro.Elapsed.TotalMilliseconds);
      }
    }
  }

  private void TogglePausaManualCombate()
  {
    SetPausaManualCombate(!pausaManualCombateActiva);
  }

  private void SetPausaPorLog(bool activa)
  {
    if (pausaPorLogActiva == activa)
    {
      return;
    }

    pausaPorLogActiva = activa;
    NotificarCambioEstadoPausaCombate();
    AplicarEscalaTiempoCombate();
  }

  private void SetPausaManualCombate(bool activa)
  {
    if (pausaManualCombateActiva == activa)
    {
      ActualizarIndicadorPausaCombate();
      return;
    }

    pausaManualCombateActiva = activa;
    NotificarCambioEstadoPausaCombate();
    ActualizarIndicadorPausaCombate();
    AplicarEscalaTiempoCombate();
  }

  private void SincronizarPausaConVisibilidadLog()
  {
    bool logActivo = goLog != null && goLog.activeInHierarchy;
    if (logActivo == ultimaVisibilidadLogActiva)
    {
      return;
    }

    ultimaVisibilidadLogActiva = logActivo;

    if (logActivo && !pausaPorLogActiva)
    {
      SetPausaPorLog(true);
    }
    else if (!logActivo && pausaPorLogActiva)
    {
      SetPausaPorLog(false);
    }
  }

  private CancellationToken ObtenerTokenCambioEstadoPausa()
  {
    if (cambioEstadoPausaDelayCts == null)
    {
      cambioEstadoPausaDelayCts = new CancellationTokenSource();
    }

    return cambioEstadoPausaDelayCts.Token;
  }

  private void NotificarCambioEstadoPausaCombate()
  {
    CancellationTokenSource ctsAnterior = cambioEstadoPausaDelayCts;
    cambioEstadoPausaDelayCts = new CancellationTokenSource();

    if (ctsAnterior == null)
    {
      return;
    }

    try
    {
      ctsAnterior.Cancel();
    }
    catch (ObjectDisposedException)
    {
    }
    finally
    {
      ctsAnterior.Dispose();
    }
  }

  private void CancelarCambioEstadoPausaDelay()
  {
    if (cambioEstadoPausaDelayCts == null)
    {
      return;
    }

    try
    {
      cambioEstadoPausaDelayCts.Cancel();
    }
    catch (ObjectDisposedException)
    {
    }
    finally
    {
      cambioEstadoPausaDelayCts.Dispose();
      cambioEstadoPausaDelayCts = null;
    }
  }

  private void ActualizarIndicadorPausaCombate()
  {
    EnsureTextoPausaCombate();
    if (goTextoPausaCombate == null || txtPausaCombate == null)
    {
      return;
    }

    txtPausaCombate.text = ObtenerTextoPausa();

    if (pausaManualCombateActiva)
    {
      UIFadeSlideUtility.Show(goTextoPausaCombate);
    }
    else
    {
      UIFadeSlideUtility.Hide(goTextoPausaCombate);
    }
  }

  private void EnsureTextoPausaCombate()
  {
    if (goTextoPausaCombate != null)
    {
      return;
    }

    Canvas canvasObjetivo = null;
    if (txtLog != null && txtLog.canvas != null)
    {
      canvasObjetivo = txtLog.canvas.rootCanvas != null ? txtLog.canvas.rootCanvas : txtLog.canvas;
    }
    else if (UICanvasTurnoJugador != null)
    {
      Canvas canvasJugador = UICanvasTurnoJugador.GetComponentInParent<Canvas>(true);
      if (canvasJugador != null)
      {
        canvasObjetivo = canvasJugador.rootCanvas != null ? canvasJugador.rootCanvas : canvasJugador;
      }
    }
    else
    {
      canvasObjetivo = GetComponentInChildren<Canvas>(true);
    }

    if (canvasObjetivo == null)
    {
      return;
    }

    goTextoPausaCombate = new GameObject("TextoPausaCombate", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI));
    goTextoPausaCombate.transform.SetParent(canvasObjetivo.transform, false);

    RectTransform rect = (RectTransform)goTextoPausaCombate.transform;
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = new Vector2(0f, 28f);
    rect.sizeDelta = new Vector2(420f, 90f);

    txtPausaCombate = goTextoPausaCombate.GetComponent<TextMeshProUGUI>();
    txtPausaCombate.alignment = TextAlignmentOptions.Center;
    txtPausaCombate.fontSize = 42f;
    txtPausaCombate.fontStyle = FontStyles.SmallCaps;
    txtPausaCombate.enableWordWrapping = false;
    txtPausaCombate.raycastTarget = false;
    txtPausaCombate.color = new Color(1f, 1f, 1f, 0.72f);
    txtPausaCombate.text = "Pausa";

    if (txtLog != null && txtLog.font != null)
    {
      txtPausaCombate.font = txtLog.font;
    }
    else if (rondaText != null && rondaText.font != null)
    {
      txtPausaCombate.font = rondaText.font;
    }

    UIFadeSlide anim = UIFadeSlideUtility.Ensure(goTextoPausaCombate);
    if (anim != null)
    {
      anim.SetDurations(0.12f, 0.1f);
      anim.SetOffsets(new Vector2(0f, 12f), new Vector2(0f, -12f));
      anim.SetFollowMouse(false, Vector2.zero);
    }

    UIFadeSlideUtility.HideImmediate(goTextoPausaCombate);
  }

  private string ObtenerTextoPausa()
  {
    if (TRADU.i == null)
    {
      return "Pausa";
    }

    string traducido = TRADU.i.Traducir("Pausa");
    if (TRADU.i.nIdioma == 2)
    {
      return string.IsNullOrWhiteSpace(traducido) || string.Equals(traducido, "Pausa", StringComparison.OrdinalIgnoreCase)
        ? "Pause"
        : traducido;
    }

    return string.IsNullOrWhiteSpace(traducido) ? "Pausa" : traducido;
  }

  private void AplicarEscalaTiempoCombate()
  {
    Time.timeScale = PausaCombateActiva
      ? 0f
      : (modoRapidoActivado ? TimeScaleModoRapido : TimeScaleNormal);
  }

  public void btnCambiarEstadoModoRapido()
  {
    ActivarModoRapido(!modoRapidoActivado);
  }
  public void ActivarModoRapido(bool activar)
  {
    if (activar)
    {
      modoRapidoActivado = true;
      PlayerPrefs.SetInt("modoRapido", modoRapidoActivado ? 1 : 0);
      PlayerPrefs.Save();

      btnModoRapido.transform.GetChild(0).gameObject.SetActive(false);
      btnModoRapido.transform.GetChild(1).gameObject.SetActive(true);

    }
    else
    {
      modoRapidoActivado = false;
      PlayerPrefs.SetInt("modoRapido", modoRapidoActivado ? 1 : 0);
      PlayerPrefs.Save();

      btnModoRapido.transform.GetChild(0).gameObject.SetActive(true);
      btnModoRapido.transform.GetChild(1).gameObject.SetActive(false);
    }

    AplicarEscalaTiempoCombate();

  }

  public void ActualizarCasillasMelee()
  {
    if (lCasillasTotal == null)
    {
      return;
    }


    foreach (Casilla casillas in lCasillasTotal)
    {
      if (casillas != null)
      {
        casillas.activarCapaMelee(false);
      }
    } //Resetear todas las casillas
    if (unidadActiva == null)
    {
      return;
    }

    if (unidadActiva.GetComponent<IAUnidad>() != null) //IA descartados
    {
      return;
    }

    if (unidadActiva.ObtenerAPActual() < 1)
    {
      return;
    }

    bool tieneAlgunahabilidadMelee = false;
    foreach (Habilidad habilidadActiva in unidadActiva.GetComponents<Habilidad>())
    {
      if (habilidadActiva.esMelee)
      {
        tieneAlgunahabilidadMelee = true;
        break;
      }
      else
      {
        tieneAlgunahabilidadMelee = false;
      }
    }

    if (!tieneAlgunahabilidadMelee)
    { return; }


    // Marcar solo casillas vacÃ­as que sean atacables en melee.
    // Regla: columna 3 siempre atacable; columna 2 atacable solo si la casilla delante contiene
    // un obstÃ¡culo que permite atacar por detrÃ¡s o una unidad

    Casilla casillaActual = unidadActiva.CasillaPosicion;
    if (casillaActual == null || casillaActual.Presente != unidadActiva.gameObject)
    {
      casillaActual = lCasillasTotal.FirstOrDefault(c => c.Presente == unidadActiva.gameObject);
      if (casillaActual != null)
      {
        unidadActiva.CasillaPosicion = casillaActual;
      }
    }

    if (EstaEnPosMelee(casillaActual, true))
    { return; } //Si ya estÃ¡ en melee no marcar nada

    if (ladoB == null || ladoB.casillasLado == null)
    {
      return;
    }

    foreach (Casilla casilla in ladoB.casillasLado)
    {
      if (casilla == null)
      {
        continue;
      }

      if (EstaEnPosMelee(casilla))
      {
        casilla.activarCapaMelee(true);

      }
      else
      {
        casilla.activarCapaMelee(false);
      }
    }

  }

  bool EstaEnPosMelee(Casilla casilla, bool ignorarPresente = false)
  {
    if (casilla == null)
    { return false; }
    if (!ignorarPresente && casilla.Presente != null)
    {
      return false;
    }

    int x = casilla.posX;
    int y = casilla.posY;

    if (x == 3)
    {

      return true;
    }

    if (x == 2)
    {
      Casilla frente = ladoB.ObtenerCasillaPorIndex(x + 1, y);
      if (frente?.Presente != null)
      {
        Obstaculo obst = frente.Presente.GetComponent<Obstaculo>();
        Unidad unidadFrente = frente.Presente.GetComponent<Unidad>();
        if (unidadFrente == unidadActiva)
        {
          unidadFrente = null; // No cuenta si es la misma unidad
        }
        if ((obst != null && obst.bPermiteAtacarDetras) || (unidadFrente != null))
        {

          return true;
        }
      }
    }

    return false;

  }

 

}
