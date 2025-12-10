using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.Rendering;

public class BattleManager : MonoBehaviour
{
  public TutorialCombate scTutorialCombate;
  public GameObject prefabUnidad;
  public GameObject prefabUnidadCaballero;
  public GameObject prefabUnidadExplorador;
  public GameObject prefabUnidadPurificadora;
  public GameObject prefabUnidadAcechador;
  public GameObject prefabUnidadCanalizador;
  public GameObject prefabOstaculo;
  public GameObject prefabUnidadEnemiga;

  public ContenedorPrefabs contenedorPrefabs;

  public static BattleManager Instance { get; private set; }
  public int RondaNro;
  public Unidad unidadActiva;
  // Silencia logs de combate durante preparación (buffs/estados iniciales)
  public bool silenciarLogCombate = false;

  public GameObject PantallaNegraAcciones;

  public LadoManager ladoA; //Enemigo
  public LadoManager ladoB; //Jugador

  public List<Unidad> lUnidadesTotal = new List<Unidad>();
  public List<Casilla> lCasillasTotal = new List<Casilla>();

  public List<Unidad> lUnidadesPosiblesHabilidadActiva = new List<Unidad>();
  public List<Obstaculo> lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>();
  public event EventHandler OnRondaNueva;
  public event EventHandler OnTurnoNuevo;


  public UIBotonesHabilidades scUIBotonesHab;
  public UIContadorAP scUIContadorAP;
  public UIBarraOrdenTurno scUIBarraOrdenTurno;
  public UIInfoChar scUIInfoChar;


  public GameObject botonConsumibleA;
  public GameObject botonConsumibleB;

  public GameObject UICanvasTurnoJugador;
  public GameObject UICanvasTurnoAI;

  public GameObject UIGOPasarTurno;


  public Image widgetClima;
  public GameObject climaTooltip;
  public TextMeshProUGUI textClimaTooltip;
  public TextMeshProUGUI rondaText;
  public bool bOcupado; //Variable de control de flujo de batalla

  public GameObject nocheLienzo;
  private void Awake()
  {
    if (Instance != null)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
  }
  public int indexTurno = 0;

  private void Start()
  {
    ArmarListadeCasillastotales();
    var handicapDificultad = GetComponent<Sistema.HandicapDificultad>();
    if (handicapDificultad != null)
    {
      handicapDificultad.AplicarDificultadDesdePlayerPrefs();
    }

    RondaNro = 1;

    if (logDeCampania != null)
      logDeCampania.SetDiaActual(RondaNro);

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



    ActualizarAliadosRefUI();
    //  RondaNueva();

  }
  public void ArrancarTurno() //Arranca el turno de la unidad activa
  {
    if (unidadActiva != null)
    {
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
      ActualizarlistaHabilidades();//dejar aca y abajo, se llama 2 veces

      OnTurnoNuevo?.Invoke(this, EventArgs.Empty);

      unidadActiva.ArrancaTurnoEstaUnidad();
      scUIInfoChar.ActualizarInfoChar(unidadActiva);
      /*---*/
      SincronizarHabilidadDestruirObstaculo(unidadActiva);
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





  public void TerminarTurno() //Termina el turno de la unidad activa
  {
    scUIBotonesHab.UIDesactivarBotones();

    unidadActiva.TerminaTurnoEstaUnidad();

    if (indexTurno >= 0 && indexTurno < lUnidadesTotal.Count)
    {
      unidadActiva = lUnidadesTotal[indexTurno];



      ArrancarTurno();
    }
    else
    {
      // Si no quedan enemigos en el campo y hay refuerzos pendientes,
      // forzar que aparezcan en la próxima ronda sin importar el delay actual.
      // Ajustamos el umbral para que en la próxima Ronda (RondaNro+1) se cumpla (RondaNro+1 > delayRefuerzo).
      if (enemigosRefuerzos != null && enemigosRefuerzos.Count > 0 && ladoA != null && ladoA.unidadesLado.Count == 0)
      {
        if (delayRefuerzo > RondaNro)
        {
          delayRefuerzo = RondaNro;
          ActualizarRefuerzosUI();
        }
      }

      RondaNueva();
    }





  }
  public void RondaNueva() //Finaliza la ronda y se reordenan las unidades según iniciativa
  {

    RondaNro++;
    silenciarLogCombate = false;
    if (TRADU.i.nIdioma == 1)
    { EscribirLog("==== Ronda " + RondaNro + " comienza ===="); }
    else if (TRADU.i.nIdioma == 2)
    { EscribirLog("==== Round " + RondaNro + " begins ===="); }

    OnRondaNueva?.Invoke(this, EventArgs.Empty);

    AdministrarListas();
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

    if (RondaNro > delayRefuerzo)
    {
      AdministrarRefuerzosEnemigos();
    }

    if (RondaNro > delayAliados)
    {
      AdministrarRefuerzosAliados();
    }

    rondaText.text = TRADU.i.Traducir("Ronda") + " " + RondaNro;
    //  BorrarLog();
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
  public void ActualizarRefuerzosUI()
  {
    int tiempoRestante = delayRefuerzo - RondaNro + 1;
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
      return; // No mandar refuerzos si hay más de 6 enemigos
    }

    // Si hay más de 3 enemigos en la lista de refuerzos
    if (enemigosRefuerzos.Count > 3)
    {
      // Mandar refuerzos en los dos primeros y quitarlos de la lista
      MandarRefuerzoEnemigo(enemigosRefuerzos[0]);
      MandarRefuerzoEnemigo(enemigosRefuerzos[1]);

      // Eliminar los dos primeros de la lista
      enemigosRefuerzos.RemoveAt(0); // Elimina el primer enemigo
      enemigosRefuerzos.RemoveAt(0); // Elimina el nuevo primer enemigo, ya que la lista se ha reordenado
    }
    else if (enemigosRefuerzos.Count > 0) // Si hay 3 o menos
    {
      // Mandar un solo refuerzo y quitarlo de la lista
      MandarRefuerzoEnemigo(enemigosRefuerzos[0]);
      enemigosRefuerzos.RemoveAt(0);
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
      return; // No mandar refuerzos si hay más de 5 aliados
    }

    if (aliadosRefuerzos.Count > 0) // Si hay 3 o menos
    {
      // Mandar un solo refuerzo y quitarlo de la lista
      MandarRefuerzoAliado(aliadosRefuerzos[0]);
      aliadosRefuerzos.RemoveAt(0);
      // Hacer que los aliados solo lleguen cada 2 turnos, desde el 2do turno
      delayAliados += 2;

    }


    ActualizarAliadosRefUI();
  }

  void MandarRefuerzoEnemigo(GameObject enemigo)
  {
    if (ladoA.c1x3.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x3.PonerObjetoEnCasillaAnimado(enemigo, 2);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoA.c1x2.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x2.PonerObjetoEnCasillaAnimado(enemigo, 2);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoA.c1x4.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x4.PonerObjetoEnCasillaAnimado(enemigo, 2);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoA.c1x5.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x5.PonerObjetoEnCasillaAnimado(enemigo, 2);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoA.c1x1.Presente == null)
    {
      enemigo.SetActive(true);
      ladoA.c1x1.PonerObjetoEnCasillaAnimado(enemigo, 2);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }

    EscribirLog("<color=#d92b08>" + enemigo.GetComponent<Unidad>().uNombre + TRADU.i.Traducir(" se ha unido a la batalla. Quedan ") + (enemigosRefuerzos.Count() - 1) + TRADU.i.Traducir(" refuerzos.</color> "));
    AplicarEfectosInicioCombate(enemigo.GetComponent<Unidad>());
    scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
  }
  void MandarRefuerzoAliado(GameObject enemigo)
  {
    enemigo.GetComponent<Unidad>().entroComoAliado = true;

    if (ladoB.c1x3.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x3.PonerObjetoEnCasillaAnimado(enemigo, 1);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoB.c1x2.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x2.PonerObjetoEnCasillaAnimado(enemigo, 1);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoB.c1x4.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x4.PonerObjetoEnCasillaAnimado(enemigo, 1);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoB.c1x5.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x5.PonerObjetoEnCasillaAnimado(enemigo, 1);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    else if (ladoB.c1x1.Presente == null)
    {
      enemigo.SetActive(true);
      ladoB.c1x1.PonerObjetoEnCasillaAnimado(enemigo, 1);
      scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
      enemigo.GetComponent<Unidad>().EstablecerAPActualA(0);
    }
    AplicarEfectosInicioCombate(enemigo.GetComponent<Unidad>());
    EscribirLog("<color=#d92b08>" + enemigo.GetComponent<Unidad>().uNombre + TRADU.i.Traducir(" se ha unido a la batalla. Quedan ") + (enemigosRefuerzos.Count() - 1) + TRADU.i.Traducir(" refuerzos.</color> "));
    scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();


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
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Fuerza Kale'Tav";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = -1;
        buff.cantDanioPorcentaje += 10;
        buff.cantHPMax += 10;
        buff.cantTsMental += 1;
        buff.cantTsFortaleza += 1;
        buff.AplicarBuff(u);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }
    }

    //Zarkil Masacre
    if (u.TieneTag("Zarkil") && CampaignManager.Instance.scAtributosZona.ID == 3 && CampaignManager.Instance.intTipoClima == 9)
    {
      // BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Masacre Zarkil";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = -1;
      buff.cantCritDado += 2;
      buff.percCritDaño += 20;
      buff.AplicarBuff(u);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
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
      return;
    }

    scUIBotonesHab?.DeseleccionarTodas();
    HabilidadActiva = null;
    SeleccionandoObjetivo = false;
    LimpiarCapasCasillas();
    scUIContadorAP?.ResetearCirculos();
  }

  private bool _requiereActualizarBotones;
  // Cache para restaurar estado de render de obstáculos tras sombrear
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

      _habilidadActiva?.LimpiarMarcasUnidadesPosibles();
      _habilidadActiva = value;
    }
  }
  // Casilla clickeada para resolver la habilidad (para VFX con referencia de clic)
  public Casilla casillaClickHabilidad;
  private static readonly KeyCode[] _habilidadHotkeys = new[]
  {
    KeyCode.Alpha1,
    KeyCode.Alpha2,
    KeyCode.Alpha3
  };

  private void Update()
  {
    // Si el jugador hace clic derecho o ESC mientras hay una habilidad activa, cancelarla
    if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)) && (unidadActiva != null && HabilidadActiva != null))
    {
      CancelarHabilidadActiva();
    }
    else if (Input.GetKeyDown(KeyCode.Escape))
    {
      //ACA QUE ABRA MENU OPCIONES DE BATALLA CUANDO ESTEN
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
    if (Input.GetKeyDown(KeyCode.Space))
    {
      if (unidadActiva != null)
      {
        if (unidadActiva.GetComponent<IAUnidad>() == null)
        {
          TerminarTurno();
        }
      }
    }

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
  }

  private void LateUpdate()
  {
    SincronizarMarcasHabilidadActiva();
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
  public void TiltearCamaraLadoEnemigo(bool cBool)
  {
    if (seTilteo != cBool)
    {

      seTilteo = cBool;
      float targetAngle = cBool ? 3.5f : -3.5f;
      StartCoroutine(RotateCameraSmoothly(targetAngle));
    }
  }

  private IEnumerator RotateCameraSmoothly(float targetAngle)
  {
    float duration = 0.12f; // Duración del efecto en segundos
    float delay = targetAngle < 0 ? 0.35f : 0f; // Retardo adicional si el ángulo es negativo!!
    float elapsedTime = 0f;

    // Pausa inicial si el ángulo es negativo
    if (delay > 0)
    {
      yield return new WaitForSeconds(delay);
    }

    Quaternion initialRotation = goCamara.transform.localRotation;
    Quaternion targetRotation = initialRotation * Quaternion.Euler(0, targetAngle, 0);

    while (elapsedTime < duration)
    {
      goCamara.transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / duration);
      elapsedTime += Time.deltaTime;
      yield return null;
    }

    goCamara.transform.localRotation = targetRotation; // Asegúrate de que la rotación final sea exacta
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
    // y aún no se actualizó la lista de unidades del lado.
    ladoA.ActualizarListaDeUnidadesEnLado();
    ladoB.ActualizarListaDeUnidadesEnLado();

    //Lado Enemigos
    if (ladoA.unidadesLado.Count < 1 && enemigosRefuerzos.Count < 1)
    {
      transform.parent.parent.gameObject.GetComponent<AdministradorEscenas>().FinDeBatalla(1); //Ganó jugador
    }
    else if (ladoB.unidadesLado.Count < 1)
    {
      //Lado Jugador
      transform.parent.parent.gameObject.GetComponent<AdministradorEscenas>().FinDeBatalla(0); //Perdió jugador
    }


  }

  [SerializeField] TextMeshProUGUI txtLog;
  [SerializeField] GameObject goLog;
  /* public void EscribirLog(string log)
  {
     // Divide el texto existente en líneas
     List<string> lineas = new List<string>(txtLog.text.Split('\n'));

     // Si la cantidad de líneas es mayor que 20, elimina las primeras
     while (lineas.Count > 70)
     {
         lineas.RemoveAt(0); // Elimina la primera línea
     }

     // Reinicia txtLog.text para construir el nuevo texto
     txtLog.text = "";

     foreach (string linea in lineas)
     {
         // Si la línea contiene "Día {numeroTurno}", no la modificamos
         if (linea.Contains($"Ronda {RondaNro}"))
         {
             txtLog.text += linea + "\n";
         }
         else
         {
             // Si no contiene "Día {numeroTurno}", le cambiamos el color y el tamaño
             txtLog.text += $"<size=70%>{linea}</size></color>\n";
         }
     }

     // Agrega el nuevo log y el número de turno

     txtLog.text += $"\n<size=120%><color=#cdcdcd>-Ronda {RondaNro}: </size></color>";
     txtLog.text += $"<size=100%>{log}</size>";

     // Si después de agregar las nuevas líneas, el total de líneas es mayor que 20, eliminar las más antiguas
     List<string> nuevasLineas = new List<string>(txtLog.text.Split('\n'));
     while (nuevasLineas.Count > 70)
     {
         nuevasLineas.RemoveAt(0); // Elimina la primera línea
     }

     // Reconstruye el txtLog.text con las líneas restantes
     txtLog.text = string.Join("\n", nuevasLineas);
 }*/

  public List<string> lineas;
  [SerializeField] private LogDeCampania logDeCampania;

  public void EscribirLog(string log)
  {
    if (logDeCampania == null) return;

    // Asegura que el logger sabe el día actual
    logDeCampania.SetDiaActual(RondaNro);
    logDeCampania.Escribir(log, true);

  }

  public void BorrarLog()
  {
    txtLog.text = "";
  }

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



  public void ActivartooltipClima(int n)
  {
    if (n == 1)
    {
      climaTooltip.SetActive(true);

      switch (CampaignManager.Instance.intTipoClima)
      {
        case 1: textClimaTooltip.text = TRADU.i.Traducir("Clima normal."); break;
        case 2: textClimaTooltip.text = TRADU.i.Traducir("Calor: todas las unidades obtienen 'Acalorado'."); break;
        case 3: textClimaTooltip.text = TRADU.i.Traducir("Lluvia: todas las unidades obtienen 'Mojado'. -1 Ataque a habilidades de rango."); break;
        case 4: textClimaTooltip.text = TRADU.i.Traducir("Nieve: todas las unidades obtienen 'Frío'."); break;
        case 5: textClimaTooltip.text = TRADU.i.Traducir("Niebla: -2 Ataque a habilidades de rango."); break;
      }


    }
    else
    {
      climaTooltip.SetActive(false);

    }

  }

  public void SombrearANoParticipantesHabilidad(List<object> unidades)
  {
    oscurecedor.SetActive(true);

    // Primero, asegúrate que el oscurecedor esté en el lugar correcto en la jerarquía
    Transform oscurecedorTransform = oscurecedor.transform;
    HashSet<object> unidadesSet = (unidades != null) ? new HashSet<object>(unidades) : new HashSet<object>();

    foreach (Unidad uni in lUnidadesTotal)
    {
      Transform unidadTransform = uni.transform;
      if (ListaContieneObjeto(unidadesSet, uni, uni.gameObject))
      {
        // Sombrear y poner por encima del oscurecedor
        uni.gameObject.transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(true); // Sombrear
        if (uni.gameObject.transform.GetChild(3).GetChild(1).GetChild(1).GetChild(0) != null)
        {
          uni.gameObject.transform.GetChild(3).GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(false); // ojo
        }
        uni.gameObject.transform.GetChild(3).GetChild(0).gameObject.SetActive(false); // barra

        // Poner por encima del oscurecedor en la jerarquía
        if (unidadTransform.parent == oscurecedorTransform.parent)
        {
          int oscurecedorIndex = oscurecedorTransform.GetSiblingIndex();
          int maxIndex = unidadTransform.parent.childCount - 1;
          int newIndex = Mathf.Min(oscurecedorIndex + 1, maxIndex);

          unidadTransform.GetChild(3).gameObject.GetComponent<Canvas>().overrideSorting = true; // Asegurarse que el canvas de la unidad esté por encima
          unidadTransform.GetChild(3).gameObject.GetComponent<Canvas>().sortingOrder = 0; // Asegurarse que el canvas de la unidad esté por encima

          unidadTransform.SetSiblingIndex(newIndex);

        }
      }
      else
      {
        // Poner por debajo del oscurecedor en la jerarquía
        if (unidadTransform.parent == oscurecedorTransform.parent)
        {
          int oscurecedorIndex = oscurecedorTransform.GetSiblingIndex();
          int newIndex = Mathf.Max(oscurecedorIndex - 1, 0);

          unidadTransform.GetChild(3).gameObject.GetComponent<Canvas>().overrideSorting = true; // Asegurarse que el canvas de la unidad esté por encima
          unidadTransform.GetChild(3).gameObject.GetComponent<Canvas>().sortingOrder = 5; // Asegurarse que el canvas de la unidad esté por encima


          unidadTransform.SetSiblingIndex(newIndex);
        }
      }

      uni.AcomodarSortingLayerDelay(); // Acomodar sorting layer despues de un pequeño delay
    }

    foreach (GameObject obstaculoGO in GameObject.FindGameObjectsWithTag("Obstaculo"))
    {
      Obstaculo obstaculo = obstaculoGO.GetComponent<Obstaculo>();
      if (obstaculo == null)
      {
        continue;
      }

      bool sombrearObstaculo = ListaContieneObjeto(unidadesSet, obstaculo, obstaculoGO);
      AjustarObstaculoDuranteSeleccion(obstaculo, sombrearObstaculo, oscurecedorTransform);
    }
  }

  public GameObject oscurecedor;
  public void DesombrearANoParticipantesHabilidad(List<object> unidades)
  {

    oscurecedor.SetActive(false);


    foreach (var unidad in unidades)
    {
      if (unidad is Unidad)
      {
        ((Unidad)unidad).gameObject.transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(false); //Desombrear
        if (((Unidad)unidad).gameObject.transform.GetChild(3).GetChild(1).GetChild(1).GetChild(0) != null)
        { ((Unidad)unidad).gameObject.transform.GetChild(3).GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(true); } //ojo 
      ((Unidad)unidad).gameObject.transform.GetChild(3).GetChild(0).gameObject.SetActive(true); //barra

        //
        ((Unidad)unidad).gameObject.transform.GetChild(3).gameObject.GetComponent<Canvas>().overrideSorting = false;
        ((Unidad)unidad).gameObject.transform.GetChild(3).gameObject.GetComponent<Canvas>().sortingOrder = 0;
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


    // Poner el oscurecedor como primer hijo en la jerarquía
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

  private void AjustarObstaculoDuranteSeleccion(Obstaculo obstaculo, bool sombrear, Transform oscurecedorTransform)
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

    int ordenOscurecedor = ObtenerOrdenOscurecedor(oscurecedorTransform);
    int ordenObstaculo = ordenOscurecedor - 50; // suficientemente bajo para quedar siempre detrǭs

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

    RestaurarOrdenObstaculo(obstaculo.transform);

    Canvas obstaculoCanvas = obstaculo.transform.GetComponentInChildren<Canvas>(true);
    if (obstaculoCanvas != null)
    {
      if (_canvasOriginalObstaculos.TryGetValue(obstaculoCanvas, out var original))
      {
        obstaculoCanvas.overrideSorting = original.overrideSorting;
        obstaculoCanvas.sortingOrder = original.sortingOrder;
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
  }

  private void RestaurarOrdenObstaculo(Transform obstaculoTransform)
  {
    foreach (SortingGroup sg in obstaculoTransform.GetComponentsInChildren<SortingGroup>(true))
    {
      if (_sortingGroupOriginalObstaculos.TryGetValue(sg, out var original))
      {
        sg.sortingLayerID = original.sortingLayerId;
        sg.sortingOrder = original.sortingOrder;
      }
    }

    foreach (Renderer renderer in obstaculoTransform.GetComponentsInChildren<Renderer>(true))
    {
      if (_renderOriginalObstaculos.TryGetValue(renderer, out var original))
      {
        renderer.sortingLayerID = original.sortingLayerId;
        renderer.sortingOrder = original.sortingOrder;
      }
    }
  }

  public bool modoRapidoActivado = false;
  public GameObject btnModoRapido;

  public void btnCambiarEstadoModoRapido()
  {
    ActivarModoRapido(!modoRapidoActivado);
  }
  public void ActivarModoRapido(bool activar)
  {
    if (activar)
    {
      modoRapidoActivado = true;
      Time.timeScale = 1.35f;
      PlayerPrefs.SetInt("modoRapido", modoRapidoActivado ? 1 : 0);
      PlayerPrefs.Save();

      btnModoRapido.transform.GetChild(0).gameObject.SetActive(false);
      btnModoRapido.transform.GetChild(1).gameObject.SetActive(true);

    }
    else
    {
      modoRapidoActivado = false;
      Time.timeScale = 1f;
      PlayerPrefs.SetInt("modoRapido", modoRapidoActivado ? 1 : 0);
      PlayerPrefs.Save();

      btnModoRapido.transform.GetChild(0).gameObject.SetActive(true);
      btnModoRapido.transform.GetChild(1).gameObject.SetActive(false);
    }


  }

  public void ActualizarCasillasMelee()
  {
   

    foreach (Casilla casillas in lCasillasTotal)
    {
      casillas.activarCapaMelee(false);
    } //Resetear todas las casillas
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
    {return; }

    
    // Marcar solo casillas vacías que sean atacables en melee.
    // Regla: columna 3 siempre atacable; columna 2 atacable solo si la casilla delante contiene
    // un obstáculo que permite atacar por detrás o una unidad
    foreach (Casilla casilla in ladoB.casillasLado)
    {
      if (casilla == null || casilla.Presente != null)
      continue;

      int x = casilla.posX;
      int y = casilla.posY;

      if (x == 3)
      {
      casilla.activarCapaMelee(true);
      continue;
      }

      if (x == 2)
      {
      Casilla frente = ladoB.ObtenerCasillaPorIndex(x + 1, y);
      if (frente?.Presente != null)
      {
        Obstaculo obst = frente.Presente.GetComponent<Obstaculo>();
        Unidad unidadFrente = frente.Presente.GetComponent<Unidad>();
        if(unidadFrente == unidadActiva)
        {
          unidadFrente = null; // No cuenta si es la misma unidad
        }
        if ((obst != null && obst.bPermiteAtacarDetras) || (unidadFrente != null))
          {
            casilla.activarCapaMelee(true);
          }
      }
      }
    }

    


  }
  


}
