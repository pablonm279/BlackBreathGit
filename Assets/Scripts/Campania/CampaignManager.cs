using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class CampaignManager : MonoBehaviour
{
  public static CampaignManager Instance { get; private set; }

  public GameObject prefabTextoRecursos;
  public GameObject goCanvas;
  public MapaManager scMapaManager;
  public AtributosZona scAtributosZona;

  public AlientoNegroVFX scAlientoNegroVFX;
  public MenuSequitos scMenuSequito;
  public MenuPersonajes scMenuPersonajes;
  public int mejoraCaravanaAntorchas;
  public int mejoraCaravanaAlforjas;
  public int mejoraCaravanaTiendas;
  public int mejoraCaravanaCatalejos;
  public int mejoraCaravanaAlmacen;
  public int mejoraCaravanaDefensas;

  // Sonido de la caravana al moverse (asignar desde Inspector)
  public AudioClip sfxMovimientoCaravana;
  [Range(0f, 1f)] public float sfxMovimientoVolumen = 0.8f;
  private AudioSource sfxMovimientoSource;

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

  public int numeroTurno;
  public bool MoviendoCaravana = false;

  public GameObject prefabGOPersonaje;

  public ContenedorPrefabsCamp scContprefab;

  public int BATALLA_EnCurso;

  public GameObject goSequitos;

  public AdministradorEscenas scAdministradorEscenas;

  // Cola de textos flotantes para evitar solapamientos (mínimo 0.5s entre spawns)
  [SerializeField] private float gapEntreMensajes = 0.5f;
  [SerializeField] private bool usarTextoFlotanteManager = false;
  [SerializeField] private float yStackOffset = 28f;            // desplazamiento vertical entre mensajes simultáneos
  [SerializeField] private float stackWindowSeconds = 1.2f;     // ventana donde consideramos mensajes "cercanos" al origen
  private readonly Queue<(string, Color)> colaTextos = new Queue<(string, Color)>();
  private bool procesandoCola;
  private float tiempoUltimoSpawn = -9999f;
  private readonly List<float> recentSpawnTimes = new List<float>();
  private void Awake()
  {
    if (Instance != null)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;


    // Configurar el AudioSource para el SFX de movimiento de la caravana
    if (sfxMovimientoSource == null)
    {
      sfxMovimientoSource = gameObject.AddComponent<AudioSource>();
      sfxMovimientoSource.playOnAwake = false;
      sfxMovimientoSource.loop = true;
    }
    sfxMovimientoSource.volume = sfxMovimientoVolumen;

    CambiarCivilesActuales(120);
    CambiarEsperanzaActual(75);
    CambiarSuministrosActuales(350);
    CambiarMaterialesActuales(55);
    CambiarBueyesActuales(25);
    CambiarOroActual(500);
    CambiarValorAlientoNegro(1);

    scAtributosZona.ConstruirZonaBosqueAngustiante(1);


    scMenuSequito.AgregarSequito(1);
    scMenuSequito.AgregarSequito(2);
    scMenuSequito.AgregarSequito(3);




    numeroTurno = 1;

    menuDescanso.GetComponent<MenuDescanso>().TiradaClima();



    CrearCaballero();
    CrearPurificadora();
    CrearAcechador();
    CrearExplorador();
    CrearCanalizador();

    
  
  }
  private void Start()
  {
    if (logDeCampania != null)
      logDeCampania.SetDiaActual(numeroTurno);
  }


  #region Nodos
  public SunController sunController;
  
  public void ViajeIniciado(Nodo destino)
  {
    
    bool sePrevieneAvanceAliento = false;
    sunController.OnTravelStart(); // duración en segundos

    // Inicia sonido de caravana en movimiento
    if (sfxMovimientoCaravana != null)
    {
      if (sfxMovimientoSource == null)
      {
        sfxMovimientoSource = gameObject.AddComponent<AudioSource>();
        sfxMovimientoSource.playOnAwake = false;
        sfxMovimientoSource.loop = true;
      }
      sfxMovimientoSource.volume = sfxMovimientoVolumen;
      if (sfxMovimientoSource.clip != sfxMovimientoCaravana)
        sfxMovimientoSource.clip = sfxMovimientoCaravana;
      if (!sfxMovimientoSource.isPlaying)
        sfxMovimientoSource.Play();
    }

    //Sequito de Clerigos 20% de prevenirlo
    int random2 = UnityEngine.Random.Range(0, 100);
    if (scMenuSequito.TieneSequito(10) && random2 < 21) //Clérigos !!! 21
    {
      sePrevieneAvanceAliento = true;
      EscribirLog($"-Los rezos constantes del Séquito de Clérigos han logrado frenar el avance del Aliento Negro.");

    }

    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      int random = UnityEngine.Random.Range(0, 100);
      if (pers.ActividadSeleccionada == 10 && random < 15 && !sePrevieneAvanceAliento) //Ritual de Limpieza
      {
        sePrevieneAvanceAliento = true;
        EscribirLog($"-{pers.sNombre} ha realizado con éxito un Ritual de Limpieza, previniendo el avance del Aliento Negro.");
        break;
      }

      //Aca solamente va esa actividad! las demas van mas abajo
    }

    if (!sePrevieneAvanceAliento)
    {
      CambiarValorAlientoNegro(destino.costoMovimiento); //Avance Aliento Negro por día, si no es prevenido por Purificadora o Clérigos
    }


    numeroTurno++;
    if (destino.costoMovimiento > 1)
    {
      EscribirLog($"-El viaje por el camino escarpado ha demorado la caravana. +{destino.costoMovimiento} Avance del Aliento Negro");
    }

    //Si Nieva, avanza 1 mas el Álito
    if (intTipoClima == 4)
    {
      EscribirLog("-La nieve a retrasado el viaje. +1 Avance del Aliento Negro");
      CambiarValorAlientoNegro(1);
    }

    //Efectos en Civiles segun Tier de Aliento
    if (GetTierAlientoNegro() == 1)
    {
      EscribirLog("-La ausencia de Aliento Negro al viajar, inspira a la Caravana. +2 Esperanza");
      CambiarEsperanzaActual(2);
    }
    if (GetTierAlientoNegro() == 2)
    {
      EscribirLog("-La presencia notable del Aliento Negro al viajar, provoca incertidumbre en la Caravana. -3 Esperanza");
      CambiarEsperanzaActual(-3);
    }
    if (GetTierAlientoNegro() == 3)
    {
      EscribirLog("-La gran presencia de Aliento Negro en el aire, provoca temor en la Caravana. -5 Esperanza");
      CambiarEsperanzaActual(-5);
    }
    if (GetTierAlientoNegro() == 4)
    {
      CambiarEsperanzaActual(-7);
      int random = UnityEngine.Random.Range(0, 5);
      CambiarCivilesActuales(-random);
      EscribirLog($"-La presencia de Aliento Negro en el aire es fatal para los Civiles. -7 Esperanza -{random} Civiles");
    }

    if (sequitoHerrerosMantArmaduras > 0) { sequitoHerrerosMantArmaduras--; }
    if (sequitoHerrerosMantArmas > 0) { sequitoHerrerosMantArmas--; }

    EfectosdeActividades();
    EfectosdeSequitos();



  }

  public MenuBatallas scMenuBatallas;
  public GameObject goMenuBatallas;
  public void LlegarANodo(int ID)
  {
    // Detiene el sonido de movimiento al llegar al nodo
    if (sfxMovimientoSource != null && sfxMovimientoSource.isPlaying)
      sfxMovimientoSource.Stop();

   
    
    if (ID == 1) //Batalla
    {

      goMenuBatallas.SetActive(true);




      //Probabilidad emboscada
      int randomEmboscada = UnityEngine.Random.Range(1, 101);

      int chancesemboscada = scAtributosZona.modChanceEmboscada;
      chancesemboscada -= CuantosPersonajesHacenTalActividad(14) * 5; //-5% por cada Acechador Actividad Vigilar Desde Sombras

      if (scMenuSequito.TieneSequito(9))
      {
        chancesemboscada += 4; // +4% si hay un Séquito de Nobles
      }

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
        EmpezarEventoBueno();

      }
      else { EmpezarEventoMalo();  }


      scMapaManager.nodoActual.nodoDespejado = true;

    }
    if (ID == 3) //Claro
    {
      EmpezarEvento(401);
      scMapaManager.nodoActual.nodoDespejado = true;
      if (scMenuSequito.TieneSequito(5))
      {
        scSequitoHerboristas.vecesEnClaro++;
        EscribirLog($"-El Séquito de Herboristas ha visitado un Claro y recolectado hierbas curativas.");
      }
    }
    if (ID == 4) //Posada
    {
      EmpezarEvento(402);
      scMapaManager.nodoActual.nodoDespejado = true;
    }
    if (ID == 5) //Recursos
    {
      EmpezarEvento(403);
      scMapaManager.nodoActual.nodoDespejado = true;
    }
    if (ID == 6) //Puesto Comercial
    {
      goUIComercioNodo.SetActive(true);
      txtDescripcionPuestoComercial.text = "Has llegado a un improvisado Puesto Comercial, ofrecen Suministros básicos de supervivencia a los viajeros.\nEl Tier de tu Séquito de Mercaderes ayudará a bajar los precios.\n\n\nTu Séquito de Mercaderes ha actualizado su Inventario. ";

      ResetearPuestoComercial();
      scSequitoMercaderes.GenerarItemsVendidos();
      EscribirLog("El Séquito de Mercaderes ha actualizado su inventario en el Puesto Comercial.");
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
      EscribirLog("-La Caravana ha sido emboscada por un ataque subterráneo.");

      goMenuBatallas.SetActive(true);
      scMenuBatallas.EventoBatallaSubterranea(scAtributosZona.FASE); //0 es Random

    }
    if (ID == 14) //Santuario
    {

      goUISantuario.SetActive(true);
     txtdescripcionSantuario.text = "Has llegado a un Santuario de Purificadores, varios se han construido en la zona para dar apoyo y plegarias a los valientes que combatieron al Liche.\nHoy, si bien está abandonado, mantiene su aura de tranquilidad y puedes depositar ofrendas para realizar una plegaria de purificación.\n\n\n. ";

      CambiarEsperanzaActual(10);
      EscribirLog("-La caravana ha llegado a un Santuario de Purificadores. Los personajes se han curado un 15%. +10 Esperanza.");

      foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
      {
        if (pers.IDClase == 3) // Purificadora
        {
          pers.RecibirExperiencia(60);
          EscribirLog($"-Como Purificadora, {pers.sNombre} gana 60 Experiencia por la visita al santuario.");
        }
        float curacion = pers.fVidaMaxima * 0.15f;
        pers.RecibirCuracion(curacion);
       
      }

      scMapaManager.nodoActual.nodoDespejado = true;

    }
    //Cronistas
    if (scMenuSequito.TieneSequito(7))
    {
      if (!scSequitoCronistas.yaVendioCronica)
      {
        scSequitoCronistas.valorCambiosCronicas += 20;
        EscribirLog($"-El Séquito de Cronistas ha registrado el viaje. +20 Valor Crónica.");
      }
    }
    //Nobles
    if (scMenuSequito.TieneSequito(9))
    {
      int oro = GetEsperanzaActual() / 3;
      CambiarOroActual(oro);
      EscribirLog($"-El Séquito de Nobles ha hecho una donación. +{oro} Oro.");
    }
    //Esclavos
    if (scMenuSequito.TieneSequito(11))
    {
      CambiarEsperanzaActual(-2);
      EscribirLog($"-Los Civiles se sienten culpables por la presencia de los Esclavos. -2 Esperanza.");
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
    EscribirLog("-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se ha gastado 200 de oro.");

    // Buscar personajes corruptos
    var corruptos = scMenuPersonajes.listaPersonajes.FindAll(p => p.Camp_Corrupto);
    if (corruptos.Count > 0)
    {
      var personajeCurado = corruptos[UnityEngine.Random.Range(0, corruptos.Count)];
      personajeCurado.Camp_Corrupto = false;
      EscribirLog($"-{personajeCurado.sNombre} ha sido purificado de la corrupción.");
    }
    else
    {
      EscribirLog("-No hay personajes corruptos para purificar.");
    }

    goUISantuario.SetActive(false);
  }

  public void abandonarsantuario() {  goUIPersonajeSequito.SetActive(false);}
  public void RealizarRitualSantuarioPorBueyes()
  {
    if (GetBueyesActual() < 3)
    {
      txtBueyes.color = Color.red;
      EscribirLog("-No tienes suficientes bueyes para realizar el ritual en el santuario.");
      return;
    }

    CambiarBueyesActuales(-3);
    CambiarValorAlientoNegro(-3);
    EscribirLog("-Has realizado un ritual en el santuario. El Aliento Negro retrocede en 3 y se han sacrificado 3 bueyes.");

    // Buscar personajes corruptos
    var corruptos = scMenuPersonajes.listaPersonajes.FindAll(p => p.Camp_Corrupto);
    if (corruptos.Count > 0)
    {
      var personajeCurado = corruptos[UnityEngine.Random.Range(0, corruptos.Count)];
      personajeCurado.Camp_Corrupto = false;
      EscribirLog($"-{personajeCurado.sNombre} ha sido purificado de la corrupción.");
    }
    else
    {
      EscribirLog("-No hay personajes corruptos para purificar.");
    }
    goUISantuario.SetActive(false);

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

  float precio10Suministros;
  float precio1Material;
  float precio1Buey;

  public void ResetearPuestoComercial()
  {
    pComercialSuministrosDisp = UnityEngine.Random.Range(15, 30);
    pComercialSuministrosDisp *= 10;
    pComercialMaterialesDisp = UnityEngine.Random.Range(15, 30);
    pComercialBueyesDisp = UnityEngine.Random.Range(5, 15);

    precio10Suministros = 15 - sequitoMercaderesTier;
    precio1Material = 18 - sequitoMercaderesTier;
    precio1Buey = 20 - sequitoMercaderesTier;

    txtDescSum.text = $"<Color=#F26B70>Venta: {(int)precio10Suministros / 2} Oro</color>    x10   <Color=#5ABD46>Compra: {(int)precio10Suministros} Oro</color>";
    txtDescMat.text = $"<Color=#F26B70>Venta: {(int)precio1Material / 2} Oro</color>    x1   <Color=#5ABD46>Compra: {(int)precio1Material} Oro</color>";
    txtDescBuey.text = $"<Color=#F26B70>Venta: {(int)precio1Buey / 2}  Oro</color>    x1   <Color=#5ABD46>Compra: {(int)precio1Buey} Oro</color>";
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
  }



  #endregion


  #endregion

  public int CuantosPersonajesHacenTalActividad(int IDActividad)
  {
    int cant = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers.ActividadSeleccionada == IDActividad)
      {
        cant++;
      }
    }

    return cant;
  }

  public int CuantosPersonajesSonDeTalClase(int IdClase)
  {
    int cant = 0;
    foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
    {
      if (pers.IDClase == IdClase)
      {
        cant++;
      }
    }

    return cant;
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
        EscribirLog($"-El Séquito de Artistas ha tenido un festín y despilfarrado {rand2} suministros.");
      }
    }






  }
  void EfectosdeActividades()
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

        float porcentajeVidaMax = pers.fVidaMaxima * curacionFinalSequito;
        if (pers.fVidaMaxima > pers.fVidaActual)
        {
          EscribirLog($"-{pers.sNombre} se cura {(int)porcentajeVidaMax} PV por su Actividad de <b>Descanso</b>.");
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

        EscribirLog($"-{pers.sNombre} gana {exp} Experiencia por su Actividad de <b>Entrenamiento</b>.");

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


        EscribirLog($"-{pers.sNombre} brinda 10 Experiencia a sus compañeros de menor nivel por su Actividad de <b>Relatos de Batalla</b>.");

      }
      if (pers.ActividadSeleccionada == 7) //Explorador: Caza Nocturna
      {
        int rand = UnityEngine.Random.Range(1, 5);
        CambiarSuministrosActuales(rand);
        EscribirLog($"-{pers.sNombre} consigue {rand} suministros por su Actividad de <b>Caza Nocturna</b>.");
      }
      if (pers.ActividadSeleccionada == 11) //Purificadora: Ayudar a los Desamparados
      {
        int rand = UnityEngine.Random.Range(1, 4);
        CambiarEsperanzaActual(rand);
        EscribirLog($"-{pers.sNombre} realiza su actividad <b>Ayudar a los Desamparados</b> y la esperanza aumenta en {rand}.");
      }
      if (pers.ActividadSeleccionada == 15) //Acechador: Coerción
      {
        int rand = UnityEngine.Random.Range(1, 10);
        CambiarEsperanzaActual(-1);
        CambiarOroActual(rand);
        EscribirLog($"-{pers.sNombre} obtiene {rand} de Oro de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -1 Esperanza");
      }
      if (pers.ActividadSeleccionada == 18) //Canalizador: Simbolo de Proteccion Arcano
      {

        GameObject consumible = Instantiate(scContprefab.SimboloProtArcano.gameObject);
        scMenuPersonajes.scEquipo.listInventario.Add(consumible);
        EscribirLog($"-{pers.sNombre} ha creado un Símbolo de Protección Arcano.");

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

      txtTooltipAlientoNegro.text = "<color=#8708a4><b>                  El Aliento Negro</b></color>\n\n\n";
      txtTooltipAlientoNegro.text += "<color=#ebdeef>Al morir el Liche, liberó un último estertor de muerte y putrefacción que se expande por cientos de kilómetros alrededor.";
      txtTooltipAlientoNegro.text += "\n\nLlamado el Aliento Negro, esta ola de peste y podredumbre lentamente está envolviendo a los seres vivos que no logran escapar, provocándoles la muerte, o peor. </color>\n\n\n";
      if (tierAliento == 1)
      {
        txtTooltipAlientoNegro.text += "<color=#bae895><b>Estado: Distante</b> - La Caravana viaja con tranquilidad.</color>";
      }
      if (tierAliento == 2)
      {
        txtTooltipAlientoNegro.text += "<color=#c8a6e8><b>Estado: Cerca</b> - La Caravana comienza a preocuparse y la podredumbre se siente en el aire. Los muertos acechan en las sombras.</color>";
      }
      if (tierAliento == 3)
      {
        txtTooltipAlientoNegro.text += "<color=#aa66ea><b>Estado: Dentro</b> - La Caravana ya es directamente afectada por el hedor. Los muertos se dejan ver.</color>";
      }
      if (tierAliento == 4)
      {
        txtTooltipAlientoNegro.text += "<color=#7a1dd1><b>Estado: Nocivo</b> - La peste comienza a tomar vidas civiles. Los muertos son implacables.</color>";
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
    EstadoAlientoNegro += aliento;

    if (EstadoAlientoNegro < 0) { EstadoAlientoNegro = 0; }

    ActualizarTierAlientoNegro();

    AvanzarAlientoNegro(aliento);

    if (EstadoAlientoNegro > 16 && scMenuSequito.TieneSequito(10)) //Sequito de Clérigos
    {
      scMenuSequito.RemoverSequito(10);
      EscribirLog("-El Séquito de Clérigos ha perecido, ya que el Aliento Negro ha alcanzado un nivel crítico. -20 Esperanza");
    }

  }
  public void AvanzarAlientoNegro(int n)
  {
    scAlientoNegroVFX.AvanzarAlientoNegro(n);

  }

  void ActualizarTierAlientoNegro()
  {
    Image handleSliderCalavera = sliderAlientoNegro.gameObject.transform.GetChild(2).GetChild(0).gameObject.GetComponent<Image>();

    if (EstadoAlientoNegro < 5)
    {
      TierAlientoNegro = 1;

    //  handleSliderCalavera.color = new Color(0.15f, 0.15f, 0.15f);

    }
    else if (EstadoAlientoNegro >= 5 && EstadoAlientoNegro < 11)
    {
      TierAlientoNegro = 2;

    //  handleSliderCalavera.color = new Color(0.15f, 0.12f, 0.12f);
    }
    else if (EstadoAlientoNegro > 10 && EstadoAlientoNegro < 16)
    {
      TierAlientoNegro = 3;

    //  handleSliderCalavera.color = new Color(0.18f, 0.3f, 0.3f);
    }
    else if (EstadoAlientoNegro > 15)
    {
      TierAlientoNegro = 4;

    //  handleSliderCalavera.color = new Color(0.75f, 0.2f, 0.6f);

    }


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
      case < 0: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = "Enérgicos(0)"; valueFatiga.color = new Color(0.1f, 0.95f, 0.2f); break;
      case 0: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = "Descansados(1)"; valueFatiga.color = new Color(0.1f, 0.9f, 0.3f); break;
      case 1: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = "Frescos(2)"; valueFatiga.color = new Color(0.1f, 0.7f, 0.3f); break;
      case 2: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = "En Marcha(3)"; valueFatiga.color = new Color(0.25f, 0.6f, 0.3f); break;
      case 3: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = "Agitados(4)"; valueFatiga.color = new Color(0.55f, 0.5f, 0.2f); break;
      case 4: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = "Cansados(5)"; valueFatiga.color = new Color(0.75f, 0.3f, 0.25f); break;
      case > 4: EventoFatiga(fatigaAnterior, fatigaNueva); valueFatiga.text = "Exhaustos(6)"; valueFatiga.color = new Color(0.8f, 0.15f, 0.45f); break;
    }
  }
  void EventoFatiga(int fatigaAnterior, int fatigaAhora)
  {
    if (fatigaAnterior < fatigaAhora) //Solamente si se gano fatiga
    {
      switch (fatigaAhora)
      {
        case 4: CambiarEsperanzaActual(-10); int rand = UnityEngine.Random.Range(-2, 1); CambiarBueyesActuales(rand); EscribirLog($"-La fatiga ha provocado la muerte de algunos Bueyes. {rand} Bueyes"); break;    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        case 5: CambiarEsperanzaActual(-15); int rand2 = UnityEngine.Random.Range(-2, 1); CambiarBueyesActuales(rand2); EscribirLog($"-La fatiga ha provocado la muerte de algunos Bueyes. {rand2} Bueyes"); break;
        case > 5: CambiarEsperanzaActual(-20); int rand3 = UnityEngine.Random.Range(-4, 1); CambiarBueyesActuales(rand3); int rand4 = UnityEngine.Random.Range(-10, 1); CambiarCivilesActuales(rand4); EscribirLog($"-La fatiga extrema ha provocado la muerte de algunos Bueyes y Civiles. {rand3} Bueyes {rand4} Civiles"); break;
      }
      if (scMenuSequito.TieneSequito(9) && fatigaAhora >= 4) //Si hay un Séquito de Nobles y la fatiga es 4 o más
      {
        CambiarEsperanzaActual(-2);
        CampaignManager.Instance.EscribirLog($"-El Séquito de Nobles se queja por la falta de descanso. -2 Esperanza");
      }

      if (fatigaAhora == 5)
      {
        foreach (Personaje pers in scMenuPersonajes.listaPersonajes)
        {
          pers.Camp_Fatigado = true;
        }
        EscribirLog($"-Tus héroes están fatigados. Afectará su rendimiento en batalla.");

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
  public async void CambiarEsperanzaActual(int esperanza)
  {
    int esperanzaAnterior = EsperanzaActual;
    EsperanzaActual += esperanza;
    int esperanzaNueva = EsperanzaActual;
    await GenerarTextoRecursos(esperanza, valueEsperanza.gameObject, false);



    if (EsperanzaActual < 0) { EsperanzaActual = 0; }
    if (EsperanzaActual > 100) { EsperanzaActual = 100; }

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

  }


  #endregion
  #region Civiles
  [SerializeField] private TextMeshProUGUI valueCiviles;
  private int civilesActuales;
  public float GetCivilesActual()
  {
    return civilesActuales;
  }
  public async void CambiarCivilesActuales(int civiles)
  {
    civilesActuales += civiles;
    await GenerarTextoRecursos(civiles, valueCiviles.gameObject, true);


    if (civilesActuales < 0) { civilesActuales = 0; }

    if (civilesActuales < 100)
    {
      valueCiviles.text = "" + civilesActuales;
      valueCiviles.color = new Color(0.8f, 0.1f, 0.3f);
    }
    else if (civilesActuales >= 100)
    {
      valueCiviles.text = "" + civilesActuales;
      valueCiviles.color = new Color(0.4f, 0.7f, 0.4f);
    }

    GetMiliciasActual();

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

  public async void CambiarBueyesActuales(int bueyes)
  {
    BueyesActuales += bueyes;
    await GenerarTextoRecursos(bueyes, valueBueyes.gameObject, false);

    CargaMaxActual = GetCapacidadDeCargaActual();
    valueCargaMax.text = "/" + CargaMaxActual + ")";
    valueCargaLlevada.text = "(" + GetCargaLlevadaActual() + "";
    valueBueyes.text = "" + BueyesActuales;
  }

  public int GetCargaLlevadaActual()
  {
    int cargaActual = (GetMaterialesActuales() * 3) + GetSuministrosActuales();

    if (cargaActual > GetCapacidadDeCargaActual()) { valueCargaLlevada.color = new Color(0.8f, 0.2f, 0.2f); }
    else { valueCargaLlevada.color = new Color(0.35f, 0.7f, 0.3f); }
    valueCargaMax.text = "/" + CargaMaxActual + ")";
    valueCargaLlevada.text = "(" + cargaActual + "";
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
      EscribirLog("-El sacrificio de Bueyes para obtener Suministros ha provocado preocupación. -2 Esperanza");
    }
  }

  #endregion
  #region Suministros
  [SerializeField] private TextMeshProUGUI valueSuministros;
  private int SuministrosActuales;
  public int GetSuministrosActuales()
  {
    return SuministrosActuales;
  }
  public async void CambiarSuministrosActuales(int suministros)
  {
    SuministrosActuales += suministros;
    await GenerarTextoRecursos(suministros, valueSuministros.gameObject, false);

    valueSuministros.text = "" + SuministrosActuales;
    GetCargaLlevadaActual();

  }

  public void AbandonarSuministros()
  {
    CambiarSuministrosActuales(-5);
    CambiarEsperanzaActual(-1);
  }

  #endregion
  #region Materiales
  [SerializeField] private TextMeshProUGUI valueMateriales;
  private int MaterialesActuales;
  public int GetMaterialesActuales()
  {
    return MaterialesActuales;
  }
  public async void CambiarMaterialesActuales(int Materiales)
  {
    MaterialesActuales += Materiales;
    await GenerarTextoRecursos(Materiales, valueMateriales.gameObject, false);
    valueMateriales.text = "" + MaterialesActuales;
    GetCargaLlevadaActual();
  }
  public void AbandonarMateriales()
  {
    CambiarMaterialesActuales(-2);
    CambiarEsperanzaActual(-1);
  }



  #endregion
  #region Oro
  [SerializeField] private TextMeshProUGUI valueOro;
  private int OroActuales;
  public int GetOroActuales()
  {
    return OroActuales;
  }
  public async void CambiarOroActual(int Oro)
  {
    OroActuales += Oro;
    await GenerarTextoRecursos(Oro, valueOro.gameObject, true);

    valueOro.text = "" + OroActuales;

  }
  #endregion


  async Task GenerarTextoRecursos(int cantidad, GameObject textoOrigen, bool efectoRetraso)
  {
    // Encuentra todos los objetos existentes del prefab scUnidadCanvas.PrefabtxtDaño
    GameObject[] existingTextObjects = GameObject.FindGameObjectsWithTag(prefabTextoRecursos.tag);

    // Calcula el retraso total en milisegundos
    int delayPerObject = 100;
    if (!efectoRetraso) { delayPerObject = 80; }
    int totalDelay = delayPerObject * existingTextObjects.Length;

    // Espera el tiempo calculado
    await Task.Delay(totalDelay);

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

  #region Tooltips
  public GameObject tooltipGOEsperanza;
  public GameObject tooltipGOCiviles;
  public GameObject tooltipGOSuministros;
  public GameObject tooltipGOMateriales;
  public GameObject tooltipGOBueyes;
  public GameObject tooltipGOOro;
  public GameObject tooltipGOFatiga;


  public void TooltipRecursoEntrar(int n)
  {

    if (n == 1) //Esperanza
    {
      tooltipGOEsperanza.SetActive(true);
      String text = "";

      int num = GetEsperanzaActual();
      text = $"La <color=#a0e812>Esperanza</color> determina el optimismo de la Caravana en general sobre la posibilidad de cumplir la misión y llegar al puerto.\n\n";
      text += $"{num}/100 de <color=#a0e812>Esperanza</color>\n\n";

      if (num < 11) { text += $" <color=#982a1b>1-20 Civiles abandonarán la Caravana cada descanso.</color>\n"; }
      if (num < 20 && num >= 11) { text += $" <color=#982a1b>1-10 Civiles abandonarán la Caravana cada descanso.</color>\n"; }
      if (num >= 20 && num < 80) { text += $""; }
      if (num >= 80 && num < 90) { text += $" <color=#39a91b>Los Civiles donarán algo de Oro cada descanso.</color>\n"; }
      if (num >= 90) { text += $" <color=#39a91b>Los Civiles donarán buena cantidad de Oro cada descanso.</color>\n"; }

      tooltipGOEsperanza.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 2) //Civiles
    {
      tooltipGOCiviles.SetActive(true);
      String text = "";

      float num = GetCivilesActual();
      text = $"Los <color=#c918bb>Civiles</color> que lleva la caravana hacia el Puerto. Salvar la mayor cantidad es el objetivo principal de esta misión.\n\nCada uno consume 1 de <color=#b7972c>Suministros</color> cada Descanso, y la cantidad de Civiles determina la eficiencia de las Tareas Civiles.\n";
      text += $"\nLlevas {(int)num} <color=#c918bb>Civiles</color>, deben ser al menos 100 para que la misión se considere exitosa.\n\n";
      text += $"\nLas fuerzas de la Milicia de la caravana son de <color=#a8a29c>{(int)GetMiliciasActual()}, que equivalen a {(int)GetMiliciasActual() / 10}</color> Milicianos que ayudarán a defenderla de ataques directos.\n\n";



      tooltipGOCiviles.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 3) //Suministros
    {
      tooltipGOSuministros.SetActive(true);
      String text = "";

      text = $"<color=#ffdda5>---<b>Haz click para abandonar <color=#b7972c>5 Suministros</color> y alivianar la Carga. -1 Esperanza</b>---</color>\n\n\n";
      int num = GetSuministrosActuales();
      text += $"Los <color=#b7972c>Suministros</color> constituyen las reservas de comida y elementos de supervivencia de la caravana.\n\nCada <color=#c918bb>Civil</color> consume 1 en cada Descanso.\n";
      text += $"\nLlevas {num} <color=#b7972c>Suministros</color>, por un total de peso de {num}.\n\n\n";



      tooltipGOSuministros.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 4) //Materiales
    {
      tooltipGOMateriales.SetActive(true);
      String text = "";

      text = $"<color=#ffdda5>---<b>Haz click para abandonar <color=#b34f09>2 Materiales</color> y alivianar la Carga. -1 Esperanza</b>---</color>\n\n\n";
      int num = GetMaterialesActuales();
      text += $"Los <color=#b34f09>Materiales</color> son elementos básicos de construcción utilizados para mantenimiento y expansión de la caravana.\nCada uno pesa 3.\n";
      text += $"\nLlevas {num} <color=#b34f09>Materiales</color>, por un total de peso de {num * 3}.\n\n\n";



      tooltipGOMateriales.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 5) //Carga
    {
      tooltipGOBueyes.SetActive(true);
      String text = "";
      text = $"<color=#ffdda5>---<b>Haz click para sacrificar <color=#9e2a1c>1 Buey</color> para obtener <color=#b7972c>20 Suministros</color>. -2 Esperanza</b>---</color>\n\n\n";
      int num = GetBueyesActual();
      int num2 = GetSuministrosActuales();
      int num3 = GetMaterialesActuales();
      int num4 = GetCargaLlevadaActual();

      int cargaPorBuey = 25 + mejoraCaravanaAlforjas;
      text += $"Los <color=#9e2a1c>Bueyes</color> son utilizados para llevar la carga de la caravana.\nCada uno da {cargaPorBuey} de Capacidad de Carga.\n";
      text += $"\nLlevas {num} <color=#9e2a1c>Bueyes</color>, por un total de Capacidad de Carga de {num * cargaPorBuey}.\n\n\n";
      text += $"\nLlevas {num2} <color=#b7972c>Suministros</color> y {num3} <color=#b34f09>Materiales</color> por un total de peso de {num4}/{num * 25}.\n\n";

      if (num4 > GetCapacidadDeCargaActual())
      {
        text += $"<color=#cc0d0d>La Caravana lleva Sobrecarga. Cada tramo que se haga duplica la Fatiga obtenida y reduce 10 la <color=#a0e812>Esperanza</color></color>.\n\n\n";

      }

      tooltipGOBueyes.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 6) //Oro
    {
      tooltipGOOro.SetActive(true);
      String text = "";

     
      text = $"El <color=#d8a205>Oro</color> que lleva la Caravana, utilizado para comprar bienes y contratar servicios.";

      tooltipGOOro.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
    }
    else if (n == 7) //Fatiga
    {
      tooltipGOFatiga.SetActive(true);
      String text = "";

      int num = GetFatigaActual();
      text = $"Indica que tanta <color=#06c297>Fatiga</color> tiene la Caravana en general.\n\n\n";
      text += $"Cada tramo de viaje la aumenta en 1.\n";
      text += $"Si descansas volverá a 0 y arrancarán el nuevo día Descansados(1).\n\n";



      switch (FatigaActual)
      {
        case 0: text += $"Actualmente estan Descansados(1), no habrá penalizaciones por viajar.\n\n"; ; break;
        case 1: text += $"Actualmente estan Frescos(2), no habrá penalizaciones por viajar."; break;
        case 2: text += $"Actualmente estan En Marcha(3), no habrá penalizaciones por viajar."; break;
        case 3: text += $"Actualmente estan Agitados(4), -10 Esperanza y pocos Bueyes podrían morir y los héroes se fatigarán si marchas."; break;
        case 4: text += $"Actualmente estan Cansados(5), -15 Esperanza y algunos Bueyes podrán morir si viajas."; break;
        case > 4: text += $"Actualmente estan Exhaustos(6), -20 Esperanza y varios Bueyes podrán morir si viajas."; break;
      }


      tooltipGOFatiga.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;

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
  }

  #endregion


  #region Descanso
  [SerializeField] Image botonDescansar;
  public Sprite campSi;
  public Sprite campNo;

  [SerializeField] GameObject menuDescanso;

  public void AbrirMenuDescanso()
  {
    if (!menuDescanso.activeInHierarchy && scMapaManager.nodoActual.nodoDespejado)
    {
      menuDescanso.GetComponent<MenuDescanso>().SeleccionarActividadCivil(1);
      menuDescanso.SetActive(true);
    }
    else
    {
      menuDescanso.SetActive(false);

    }


  }

  public int intTipoClima; //1 Sol, 2 Calor, 3 Lluvia, 4 Nieve, 5 Niebla
  public Image widgetClima;
  public Sprite clima_lluvia;
  public Sprite clima_nieve;
  public Sprite clima_sol;
  public Sprite clima_calor;
  public Sprite clima_niebla;

  public GameObject climaTooltip;
  public TextMeshProUGUI textClimaTooltip;

  public void ActivartooltipClima(int n)
  {
    if (n == 1)
    {
      climaTooltip.SetActive(true);

      switch (intTipoClima)
      {
        case 1: textClimaTooltip.text = $"Día {numeroTurno}: -Soleado: +5 Esperanza."; break;
        case 2: textClimaTooltip.text = $"Día {numeroTurno}: -Ola de Calor: +1 Fatiga. Jornada Libre da +5 Esperanza, otras Tareas Civiles dan -3."; break;
        case 3: textClimaTooltip.text = $"Día {numeroTurno}: -Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% Emboscada."; break;
        case 4: textClimaTooltip.text = $"Día {numeroTurno}: -Nieve: +3 Esperanza. -15% Recolecciónes, -20% Emboscada. Viajar lleva el doble de tiempo."; break;
        case 5: textClimaTooltip.text = $"Día {numeroTurno}: -Niebla: -20% Recolecciónes, -20% Emboscada, -20% Exploración, +10% Nodos Misteriosos."; break;





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
  public void EmpezarEventoMalo()
  {
    UIEvetos.SetActive(true);
    UIEvetos.GetComponent<EventosAdmin>().TirarEventoMalo();
  }
  public void EmpezarEventoBueno()
  {
    UIEvetos.SetActive(true);
    UIEvetos.GetComponent<EventosAdmin>().TirarEventoBueno();
  }

  #endregion




  void Update()
  {
    if (scMapaManager.nodoActual.nodoDespejado)
    {
      botonDescansar.sprite = campSi;
    }
    else { botonDescansar.sprite = campNo; }

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
  public void EscribirLog(string log)
  {
    if (logDeCampania == null) return;

    // Asegura que el logger sabe el día actual
    logDeCampania.SetDiaActual(numeroTurno);
    logDeCampania.Escribir(log);

    GenerarTextoFlotanteCampaña(log, Color.cyan);

   
}

  public GameObject prefabTextoCampaña;
  [SerializeField] GameObject puntoPantalla;
  private static int delayAcumulado = 0;

  // Serializa los textos a través de una cola para que no se pisen.
  // Mantiene la firma Task para compatibilidad; retorna completado inmediatamente.
  public Task GenerarTextoFlotanteCampaña(string txString, Color color)
  {
    if (puntoPantalla == null || prefabTextoCampaña == null)
      return Task.CompletedTask;

    colaTextos.Enqueue((txString, color));
    if (!procesandoCola)
    {
      // Evita iniciar múltiples coroutines si entran varias llamadas en el mismo frame
      procesandoCola = true;
      StartCoroutine(ProcesarColaTextoFlotante());
    }

    return Task.CompletedTask;
  }

  private IEnumerator ProcesarColaTextoFlotante()
  {
    while (colaTextos.Count > 0)
    {
      // Respeta separación mínima desde el último spawn
      float elapsed = Time.time - tiempoUltimoSpawn;
      if (elapsed < gapEntreMensajes)
        yield return new WaitForSeconds(gapEntreMensajes - elapsed);

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
        recentSpawnTimes.RemoveAll(t => Time.time - t > stackWindowSeconds);
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

      tiempoUltimoSpawn = Time.time;
      recentSpawnTimes.Add(tiempoUltimoSpawn);
    }
    procesandoCola = false;
  }


  public void BorrarLog()
  {
    txtLog.text = "Log vacío.";
  }



  #endregion


  #region Crear Personajes


  void CrearCaballero()
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

    pers1.iFuerza = 3 + UnityEngine.Random.Range(0, 3);
    pers1.iAgi = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 0 + UnityEngine.Random.Range(0, 1);

    pers1.iDefensa = 13;
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

    pers1.ActividadSeleccionada = 1;
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
  void CrearExplorador()
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
    pers1.iAgi = 5 + UnityEngine.Random.Range(0, 3);
    pers1.iPoder = 1 + UnityEngine.Random.Range(0, 1);

    pers1.iDefensa = 13;
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

    pers1.ActividadSeleccionada = 1;
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
  void CrearPurificadora()
  {

    GameObject purificadora = Instantiate(prefabGOPersonaje);

    Personaje pers1 = purificadora.GetComponent<Personaje>();
    pers1.sNombre = CrearNombreMujerAzar();
    pers1.fNivelActual = 1;
    pers1.fExperienciaActual = 0;
    pers1.IDClase = 3;
    pers1.idRetrato = 6;
    pers1.iPuestoDeseado = 1;

    pers1.fVidaMaxima = 4 + UnityEngine.Random.Range(1, 5);
    pers1.fVidaActual = pers1.fVidaMaxima;

    pers1.iFuerza = 1 + UnityEngine.Random.Range(0, 2);
    pers1.iAgi = 2 + UnityEngine.Random.Range(0, 2);
    pers1.iPoder = 4 + UnityEngine.Random.Range(0, 3);

    pers1.iDefensa = 11;
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



    pers1.ActividadSeleccionada = 1;
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






    pers1.itemArma = Instantiate(scContprefab.armaBaculoPurificador3);
    pers1.spRetrato = scMenuPersonajes.Female001;
    scMenuPersonajes.listaPersonajes.Add(pers1);
    scMenuPersonajes.scEquipo.ActualizarEquipo(pers1);



  }
  void CrearAcechador()
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
    pers1.iAgi = 5 + UnityEngine.Random.Range(0, 3);
    pers1.iPoder = 4 + UnityEngine.Random.Range(0, 1);

    pers1.iDefensa = 13;
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

    //Habilidades de Actividad
    pers1.ActividadSeleccionada = 1;
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
  void CrearCanalizador()
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
    pers1.iPoder = 4 + UnityEngine.Random.Range(0, 3);

    pers1.iDefensa = 11;
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
    pers1.ActividadSeleccionada = 1;
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

  public void AgregarHeroe(int n)
  {
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

    switch (claseElegida)
    {
      case 1: CrearCaballero(); break;
      case 2: CrearExplorador(); break;
      case 3: CrearPurificadora(); break;
      case 4: CrearAcechador(); break;
      case 5: CrearCanalizador(); break;
    }
    EscribirLog($"-Un nuevo héroe se ha unido a la caravana: {scMenuPersonajes.listaPersonajes[scMenuPersonajes.listaPersonajes.Count - 1].sNombre}.");
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
}
