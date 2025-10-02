using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using TMPro;

public class BattleManager : MonoBehaviour
{

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
      ActualizarlistaHabilidades();//dejar aca y abajo, se llama 2 veces

      OnTurnoNuevo?.Invoke(this, EventArgs.Empty);

      unidadActiva.ArrancaTurnoEstaUnidad();
      scUIInfoChar.ActualizarInfoChar(unidadActiva);
      /*---*/
      ActualizarlistaHabilidades();//dejar aca y arria, se llama 2 veces

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
      RondaNueva();
    }





  }
  public void RondaNueva() //Finaliza la ronda y se reordenan las unidades según iniciativa
  {

    RondaNro++;
    silenciarLogCombate = false;
    EscribirLog($"==== Ronda {RondaNro} comienza ====");

    OnRondaNueva?.Invoke(this, EventArgs.Empty);

    AdministrarListas();
    EstablecerOrdenPorIniciativa();

    ActualizarRefuerzosUI();
    ActualizarAliadosRefUI();
    DisminuirDuracionObstaculos(); //Se disminuye la duracion de los obstaculos al final de la ronda
    DuracionTrampasenCasillas(); //Se disminuye la duracion de las trampas al final de la ronda

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
    txtRefuerzosContador.text = "" + enemigosRefuerzos.Count() + "/" + (delayRefuerzo - RondaNro + 1);
    if (enemigosRefuerzos.Count < 1)
    { goRefuerzos.SetActive(false); }
    else { goRefuerzos.SetActive(true); }
  }

  public List<GameObject> enemigosRefuerzos = new List<GameObject>();
  public int delayRefuerzo = 0; //La cantidad de turnos para que empiecen a aparecer los refuerzos.
  public TextMeshProUGUI txtRefuerzosContador;
  public GameObject goRefuerzos;


  public TextMeshProUGUI txtAliadosContador;
  public List<GameObject> aliadosRefuerzos = new List<GameObject>();
  public GameObject goAliadosRefuerzos;
  int delayAliados = 1;

  public void ActualizarAliadosRefUI()
  {
    txtAliadosContador.text = "" + aliadosRefuerzos.Count() + "/" + (delayAliados - RondaNro + 1);
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

    EscribirLog($"<color=#d92b08>{enemigo.GetComponent<Unidad>().uNombre} se ha unido a la batalla. Quedan {enemigosRefuerzos.Count() - 1} refuerzos.</color> ");


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

    EscribirLog($"<color=#d92b08>{enemigo.GetComponent<Unidad>().uNombre} se ha unido a la batalla. Quedan {enemigosRefuerzos.Count() - 1} refuerzos.</color> ");



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

  }
  public void LimpiarCapasCasillas()
  {

    foreach (Casilla cas in lCasillasTotal)
    {
      cas.DesactivarCapas();
    }

  }
  public bool SeleccionandoObjetivo;
  public Habilidad HabilidadActiva;
  // Casilla clickeada para resolver la habilidad (para VFX con referencia de clic)
  public Casilla casillaClickHabilidad;

  /* void Update()
   {
     if(HabilidadActiva != null)
     {
       OpacarCasillasMelee();

     }

   }*/

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

  void DuracionTrampasenCasillas()
  {
    foreach (Transform trans in ladoA.transform)
    {
      ReduceDuracionTrampasCasillas(trans);
    }
    foreach (Transform trans in ladoB.transform)
    {
      ReduceDuracionTrampasCasillas(trans);
    }
  }

  void ReduceDuracionTrampasCasillas(Transform trans)
  {
    // Obtener todos los componentes Trampa en el objeto y sus hijos
    Trampa[] trampas = trans.GetComponentsInChildren<Trampa>();

    foreach (Trampa trmp in trampas)
    {
      // Reduce la duración y la destruye si es 0
      trmp.ReducirDuracion(1);
    }
  }

  public GameObject goCamara;
  public void TiltearCamaraLadoEnemigo(bool cBool)
  {
    float targetAngle = cBool ? 3.5f : -3.5f;
    StartCoroutine(RotateCameraSmoothly(targetAngle));
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
        case 1: textClimaTooltip.text = "Clima normal."; break;
        case 2: textClimaTooltip.text = "Calor: Todas las unidades obtienen 'Acalorado'."; break;
        case 3: textClimaTooltip.text = "Lluvia: Todas las unidades obtienen 'Mojado'. -1 Ataque a habilidades de rango."; break;
        case 4: textClimaTooltip.text = "Nieve: Todas las unidades obtienen 'Frío'."; break;
        case 5: textClimaTooltip.text = "Niebla: -2 Ataque a habilidades de rango."; break;
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

    foreach (Unidad uni in lUnidadesTotal)
    {
      Transform unidadTransform = uni.transform;
      if (unidades.Contains(uni))
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

}
