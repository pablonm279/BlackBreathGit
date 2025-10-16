using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
//using Unity.Android.Types;
using System.Diagnostics;
using TMPro;
using System.Threading.Tasks;
using System.Transactions;
using UnityEngine.Analytics;
using System.Linq;

public class Unidad : MonoBehaviour
{

   [Header ("Lógica")]
   public Casilla CasillaPosicion;
   public Casilla CasillaDeseadaMov;

   public Casilla CasillaForzadoaMover;

   public Habilidad estaCargando; //la habilidad que está cargando el personaje para lanzar en un turno próximo
   public int valorCargando; //la cantidad de AP que le falta para terminar la habilidad


   [Header ("Info base")]

   public Sprite uRetrato;
   public Image uImage;
   public String uNombre;
   public List<string> tags = new List<string>();
   public bool TieneTag(string tag)
   {
        return tags.Contains(tag);
   }

  #region //------- ATRIBUTOS -------
  //Atributo - Iniciativa --------
  
  [Header ("Atributos")]
  [SerializeField] private float at_iniciativa; //Atributo base de Iniciativa
  public float mod_iniciativa; //Atributo variable de iniciativa
  public float iniciativa_actual; //Resultado de iniciativa para la ronda
  public bool esEtereo;
  //Atributo - HP --------  Vida
  [SerializeField] private float at_maxHP; 
  public float mod_maxHP; 
  public float HP_actual; 

  //Atributo - AP -------- Puntos de Acción
  [SerializeField] private float at_maxAccionP; 
  public bool esInmobil;
  public float mod_maxAccionP; 
  
 [SerializeField] float AccionP_actual;
  public float ObtenerAPActual()
  {
    return AccionP_actual;
  }
  public void CambiarAPActual(int n)
  {
    AccionP_actual += n;
    BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();

  }
   public void EstablecerAPActualA(int n)
  {
    AccionP_actual = n;
    BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();

  }
  public float AccionP_SeEsforzo;
  



  //Atributo - PM -------- Puntos de Mérito
  [SerializeField] private float at_maxMeritoP; 
  public float mod_maxValentiaP; 
  public float ValentiaP_actual; 

  //Atributo - Car_Fuerza --------- Característica: Fuerza
  [SerializeField] private float at_CarFuerza; 
  public float mod_CarFuerza; 

  //Atributo - Car_Agilidad --------- Característica: Agilidad
  [SerializeField] private float at_CarAgilidad; 
  public float mod_CarAgilidad; 
  
  //Atributo - Car_Fuerza --------- Característica: Poder
  [SerializeField] private float at_CarPoder; 
  public float mod_CarPoder; 

  //Atributo - ARM --------- Armadura
  [SerializeField] private float at_Armadura; 
  public float mod_Armadura; 
 
  //Atributo - ResFuego --------- Res: Fuego
  [SerializeField] private float at_ResFuego; 
  public float mod_ResFuego; 

  //Atributo - ResHielo --------- Res: Hielo
  [SerializeField] private float at_ResHielo; 
  public float mod_ResHielo; 

  //Atributo - ResRayo --------- Res: Rayo
  [SerializeField] private float at_ResRayo; 
  public float mod_ResRayo; 

  //Atributo - ResArcano--------- Res: Ácido
  [SerializeField] private float at_ResAcido; 
  public float mod_ResAcido; 
  //Atributo - ResArcano--------- Res: Arcano
  [SerializeField] private float at_ResArcano; 
  public float mod_ResArcano; 

  //Atributo - ResNecro --------- Res: Necro
  [SerializeField] private float at_ResNecro; 
  public float mod_ResNecro; 

  //Atributo - ResDivino --------- Res: Divino
  [SerializeField] private float at_ResDivino; 
  public float mod_ResDivino; 

  //Atributo - Defensa
  [SerializeField] private float at_Defensa; 
  public float mod_Defensa; 
  public int Defensa_AtaquesRepetidosRonda; //Cada vez que ataquen a un mismo objetivo en el mismo turno, su defensa baja -1, (máximo -3). 
  public int Defensa_BonusPASinUsar; //Si se terminó el turno con PA sin usar, se suman a la defensa hasta el próximo turno (Máximo 2).
  //Atributo - Ataque
  [SerializeField] private float at_Ataque; 
  public float mod_Ataque; 

  public float mod_DanioPorcentaje; 

  //Atributo - Critico Dado Tirada
  [SerializeField] private float at_CriticoRangoDado; 
  public float mod_CriticoRangoDado; 

  //Atributo - Critico daño bonus
  [SerializeField] private float at_CriticoDañoBonus; 
  public float mod_CriticoDañoBonus; 

  //Atributo - Suministrosación: Reflejos
  [SerializeField] private float at_TSReflejos; 
  public float mod_TSReflejos; 

  //Atributo - Tirada salvación: Fortaleza
  [SerializeField] private float at_TSFortaleza; 
  public float mod_TSFortaleza; 

  //Atributo - Tirada salvación: Mental
  [SerializeField] private float at_TSMental; 
  public float mod_TSMental; 




  #endregion
  #region //------- ESTADOS ---------
   
  [Header ("Consumibles")]
  public Consumible ConsumibleA;
  public Consumible ConsumibleB;

 [Header ("Estados")]
 public int estado_ardiendo;
 public int estado_congelado;
 public int estado_aturdido;
 public int estado_evasion; //Cada uno aumenta en 1 la Defensa, se va al recibir daño.
 public int estado_inmovil;
 public int estado_armaduraModificador; //Es lo que le resta a la armadura por golpes que va recibiendo, debe ser psoitivo
 public int estado_acido;
 public int estado_sangrado;
 public int estado_veneno;
 public int estado_invulnerable;
 public int estado_regeneravida;
 public int estado_regeneraarmadura;
 public int estado_APModificador;
 public int estado_ResistenciasReducidas;
 public bool estado_Corrupto;

 [Header("Inmunidades Extras")]
 public bool inmunidad_Ceguera = false;
 public bool inmunidad_Oscuridad = false;
 public bool inmunidad_Trampas = false; //Flotadoras



 public int bonusdam_fuego; //Esto funciona como 1dX 
 public int bonusdam_hielo;
 public int bonusdam_rayo;
 public int bonusdam_acido;
 public int bonusdam_arcano;
 public int bonusdam_necro;
 public int bonusdam_divino;

 public float barreraDeDanio;
 public int tejidoCuracMagica;
 public bool loMatoCorrompido = false; //Si la unidad fue muerta por un enemigo corrupto, queda registrado aca

//Escondido
  int estaEscondido; //0 no, 1 si y sale al recibir daño o atacar, 2 si y no sale al recibir daño ni atacar
  public bool entroComoAliado = false;
  
 #endregion


  //Animaciones
  private Animator animator;
  private UnidadPoseController poseController;
  

  UnidadCanvas scUnidadCanvas;
  public



 BattleManager scBattleManager;
  public Transform puntoSaliente;
  public Transform puntoEntrante;
  private void Awake()
  {
    scBattleManager = BattleManager.Instance;

    scUnidadCanvas = GetComponentInChildren<UnidadCanvas>();

    scBattleManager.OnRondaNueva += BattleManager_OnRondaNueva;

    animator = GetComponent<Animator>();
    poseController = GetComponent<UnidadPoseController>();

    ValentiaP_actual = 0;
   
  
  Transform child4 = transform.childCount > 4 ? transform.GetChild(4) : null;

  if (child4 != null && child4.childCount >= 2)
  {
    puntoSaliente = child4.GetChild(0);
    puntoEntrante = child4.GetChild(1);
  }
}

  public void ReproducirAnimacionAtaque()
  {
    if(animator != null)
    {
        animator.SetTrigger( "Trigger_Ataque");
        
    }
    if(poseController != null){ poseController.PlayAttackPose(); }
  }
  public void ReproducirAnimacionTurnoNuevo()
  {
    if(animator != null)
    {
        animator.SetTrigger( "Trigger_TurnoNuevo");
       
    }
  }

  public void ReproducirAnimacionRecibirDanio()
  {
    if(animator != null)
    {
        animator.SetTrigger( "Trigger_Recibedanio");
        
    }
  }

  public void ReproducirAnimacionHabilidadNoHostil()
  {
    if(poseController != null){ poseController.PlaySkillPose(); }
  }

  public void ReproducirAnimacionMorir()
  {
    if(animator != null)
    {
        animator.SetTrigger( "Trigger_Morir");
       
    }
  }


private void Update()
{

 if(BattleManager.Instance.unidadActiva == this)
 {
    transform.GetChild(1).gameObject.SetActive(true);
 }else{  transform.GetChild(1).gameObject.SetActive(false);}


}


public void CrearUnidad(
int indexRetrato, string Nombre, float HP, float Iniciativa, float AP, float Merito, 
float cFuerza, float cAgilidad, float cPoder, float Arm, float ResNecro, 
float ResArcano, float ResFuego, float ResHielo, float ResRayo, float Defensa, 
float critRangoDado, float critDañoBonus, float Ataque, float TSReflejos, float TSFortaleza, float TSMental, float ResDivino
)

{

    //Pone Retratos
    switch (indexRetrato)
    {
      case 1: uRetrato = scBattleManager.contenedorPrefabs.char1; break;
      case 2: uRetrato = scBattleManager.contenedorPrefabs.char2; break;
      case 3: uRetrato = scBattleManager.contenedorPrefabs.char3; break;
      case 4: uRetrato = scBattleManager.contenedorPrefabs.char4; break;
      case 5: uRetrato = scBattleManager.contenedorPrefabs.explorador1; break;
      case 6: uRetrato = scBattleManager.contenedorPrefabs.purificadora1; break;
      case 7: uRetrato = scBattleManager.contenedorPrefabs.acechador1;break;
      case 8: uRetrato = scBattleManager.contenedorPrefabs.canalizador1;break;
  }
 
  //Pone Atributos
  gameObject.name = "Unidad: "+Nombre;
  uNombre = Nombre;
  at_maxHP = HP;
  mod_maxHP = HP;
  HP_actual = at_maxHP; 

  at_iniciativa = Iniciativa;
  mod_iniciativa = Iniciativa;
  iniciativa_actual = Iniciativa;

  at_maxAccionP = AP;
  mod_maxAccionP = AP;
  AccionP_actual = AP;

  at_maxMeritoP = Merito;
  mod_maxValentiaP = Merito;
  ValentiaP_actual = 0; 


  at_CarFuerza = cFuerza;
  mod_CarFuerza = cFuerza;

  at_CarAgilidad = cAgilidad;
  mod_CarAgilidad = cAgilidad;

   at_CarPoder = cPoder;
  mod_CarPoder = cPoder;

   at_Armadura = Arm;
  mod_Armadura = Arm;

  at_ResArcano = ResArcano;
  mod_ResArcano = ResArcano;
  at_ResFuego = ResFuego;
  mod_ResFuego = ResFuego;
  at_ResHielo = ResHielo;
  mod_ResHielo = ResHielo;
  at_ResRayo = ResRayo;
  mod_ResRayo= ResRayo;
  at_ResNecro = ResNecro;
  mod_ResNecro = ResNecro;
  at_ResDivino = ResDivino;
  mod_ResDivino = ResDivino;

  at_CriticoRangoDado = critRangoDado;
  mod_CriticoRangoDado = critRangoDado;
  at_CriticoDañoBonus = critDañoBonus;
  mod_CriticoDañoBonus = critDañoBonus;

  at_Defensa = Defensa;
  mod_Defensa = Defensa;
  at_Ataque = Ataque;
  mod_Ataque = Ataque;

  at_TSFortaleza = TSFortaleza;
  mod_TSFortaleza = TSFortaleza;
  at_TSReflejos = TSReflejos;
  mod_TSReflejos = TSReflejos;
  at_TSMental = TSMental;
  mod_TSMental = TSMental;

 
   
}

public float velocidadMovimiento = 2.8f; 
public bool movimientoEnCurso = false;
  // Casilla origen al comenzar el movimiento; se limpia Presente solo una vez
  private Casilla casillaOrigenEnMovimiento;

    void Start()
    {
       Invoke("AcomodarSortingLayer", 1.15f); //Para que el sprite quede bien en el orden de sorting layer

    }
  public bool NoSonidoAlMover;
    private void FixedUpdate()
  {

    if (CasillaDeseadaMov != null)
    {
      if ((CasillaPosicion != CasillaDeseadaMov) && (scBattleManager.unidadActiva == this))
      {
        if (!movimientoEnCurso)
        {
          if (poseController != null) { poseController.OnStartMove(); }

          if (!NoSonidoAlMover) { AudioSource.PlayClipAtPoint(BattleManager.Instance.contenedorPrefabs.sonidoMovimientoLigero, transform.position); }

          // Guardar casilla origen y limpiar Presente una sola vez
          casillaOrigenEnMovimiento = CasillaPosicion;
          if (casillaOrigenEnMovimiento != null)
          {
            casillaOrigenEnMovimiento.Presente = null;
          }
        }
        movimientoEnCurso = true;
        // Calcula la dirección hacia la casilla deseada
        Vector3 direccion = CasillaDeseadaMov.transform.position - transform.position;

        // Calcula la nueva posición interpolando suavemente
        Vector3 nuevaPosicion = transform.position + direccion.normalized * velocidadMovimiento * Time.fixedDeltaTime;

        // Establece la nueva posición
        transform.position = nuevaPosicion;

        // Comprueba si el objeto ha llegado a la casilla deseada
        if (Vector3.Distance(transform.position, CasillaDeseadaMov.transform.position) < 0.045f)
        {
          LlegoACasilla(CasillaDeseadaMov);
          CasillaPosicion = CasillaDeseadaMov;
          CasillaPosicion.NuevoObjetoPresenteEnCasilla(gameObject);
          scBattleManager.CalcularCasillasAMovimiento();
          ChequearSeMovio();
          movimientoEnCurso = false;
          casillaOrigenEnMovimiento = null;
          if (poseController != null) { poseController.OnStopMove(); }

        }
      }
    }

    if (CasillaForzadoaMover != null)
    {
      if ((CasillaPosicion != CasillaForzadoaMover) && (scBattleManager.unidadActiva != this))
      {
        if (!movimientoEnCurso)
        {
          if (poseController != null) { poseController.OnStartMove(); }
          // Guardar casilla origen y limpiar Presente una sola vez
          casillaOrigenEnMovimiento = CasillaPosicion;
          if (casillaOrigenEnMovimiento != null)
          {
            casillaOrigenEnMovimiento.Presente = null;
          }
        }
        movimientoEnCurso = true;
        // Calcula la dirección hacia la casilla deseada
        Vector3 direccion = CasillaForzadoaMover.transform.position - transform.position;

        // Calcula la nueva posición interpolando suavemente
        Vector3 nuevaPosicion = transform.position + direccion.normalized * velocidadMovimiento * Time.fixedDeltaTime;

        // Establece la nueva posición
        transform.position = nuevaPosicion;

        // Comprueba si el objeto ha llegado a la casilla deseada
        if (Vector3.Distance(transform.position, CasillaForzadoaMover.transform.position) < 0.035f)
        {
          // Llegó a casilla forzada
          LlegoACasilla(CasillaForzadoaMover);
          CasillaPosicion = CasillaForzadoaMover;
          CasillaPosicion.NuevoObjetoPresenteEnCasilla(gameObject);
          scBattleManager.CalcularCasillasAMovimiento();
          ChequearSeMovio();
          CasillaPosicion.Presente = this.gameObject;
          print(CasillaPosicion.Presente);
          CasillaForzadoaMover = null;
          CasillaDeseadaMov = null;
          movimientoEnCurso = false;
          casillaOrigenEnMovimiento = null;
          if (poseController != null) { poseController.OnStopMove(); }
        }
      }

    }


  }

public void TirarIniciativa()
{
  iniciativa_actual = UnityEngine.Random.Range(1,21) + mod_iniciativa;

}
private void BattleManager_OnRondaNueva(object sender, EventArgs empty)
{
 
   if(gameObject.GetComponent<RetrasarTurno>() != null)
   {
      gameObject.GetComponent<RetrasarTurno>().yaRetraso = false;
   }
   
}
  public virtual void ComienzoBatallaEnemigo()
  {
   


    //--Cada enemigo que lo necesite deberá heredar de Unidad    

  }
void LlegoACasilla(Casilla cas) //Método que se llama cada vez que una unidad llega a una casilla, se puede sobreescribir en las subclases
{
    if (cas != null)
    {

      //Si tiene el buff Escudo de Fe y se mueve a casilla sin la "trampa" escudo de fe, remueve el buff
      if (cas.GetComponent<TrampaEscudoFe>() == null)
      {
        foreach (Buff buff in gameObject.GetComponents<Buff>())
        {
          if (buff.buffNombre == "Escudado por Fe")
          {
            buff.RemoverBuff(this);
          }
        }
      }

      cas.Presente = gameObject;
    }
}

  public void ArrancaTurnoEstaUnidad()
  {
    Invoke("AcomodarSortingLayer", 1.15f); //Para que el sprite quede bien en el orden de sorting layer

    if (gameObject.GetComponent<RetrasarTurno>() != null)
    {


      if (TRADU.i.nIdioma == 1)
      {
        if (CasillaPosicion.lado == 1)
        {
          scBattleManager.EscribirLog($"<size=130%><color=#ae1b00>---Turno de {uNombre}---</color></size>");
        }
        else
        {
          scBattleManager.EscribirLog($"<size=130%><color=#003cab>---Turno de {uNombre}---</color></size>");
        }
      }
       if (TRADU.i.nIdioma == 2)
      {
        if (CasillaPosicion.lado == 1)
        {
          scBattleManager.EscribirLog($"<size=130%><color=#ae1b00>---{uNombre}'s Turn---</color></size>");
        }
        else
        {
          scBattleManager.EscribirLog($"<size=130%><color=#003cab>---{uNombre}'s Turn---</color></size>");
        }
      }

      if (gameObject.GetComponent<RetrasarTurno>().yaRetraso == false)
      {//Aplica los efectos de turno nuevo solo cuando no retrasó el turno, ya que si retrasa su turno y le vuelve a tocar despues, se aplicaria todo 2 veces.

        if (GetComponent<IAUnidad>() == null) { ReproducirAnimacionTurnoNuevo(); } //Las unidades no IA, tienen esa pequeña animación

        //-------------------------------Defensa_AtaquesRepetidosRonda
        Defensa_AtaquesRepetidosRonda = 0;
        //---
        //-------------------------------Defensa_BonusPASinUsar
        Defensa_BonusPASinUsar = 0;
        //---
        //-------------------------------Descuenta Esfuerzo a AP y restaura AP
        AccionP_actual = mod_maxAccionP;
        AccionP_actual -= AccionP_SeEsforzo;
        AccionP_actual += estado_APModificador;

        BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();

        AccionP_SeEsforzo = 0;


        //Estados -----------------
        estado_APModificador = 0;


        //Ardiendo
        if (estado_ardiendo > 0 && !TieneTag("Etereo")) { Estados.Efecto_Ardiendo(this); }

        //Congelado
        if (estado_congelado > 0) { Estados.Efecto_Congelado(this); }

        //Aturdido
        if (estado_aturdido > 0) { Estados.Efecto_Aturdido(this); }

        //Inmovil
        if (estado_inmovil > 0) { Estados.Efecto_Inmovil(this); }

        //Sangrado
        if (estado_sangrado > 0 && !TieneTag("Etereo") && !TieneTag("Nomuerto")) { Estados.Efecto_Sangrado(this); }

        //Veneno
        if (estado_veneno > 0 && !TieneTag("Etereo") && !TieneTag("Nomuerto")) { Estados.Efecto_Veneno(this); }

        //Invulnerable
        if (estado_invulnerable > 0)
        {
          estado_invulnerable--;
        }

        //Regenera vida
        if (estado_regeneravida > 0) { Estados.Efecto_RegeneraVida(this); }

        //Regenera Armadura
        if (estado_regeneraarmadura > 0) { Estados.Efecto_RegeneraArmadura(this); }

        //Ejecuta efectos custom de Buffs al inicio del turno
        ActivarEfectosCustomBuffsInicioTurno();

        //Exclusivo Clase
        ActualizarClaseComienzoTurno();//va antes

        //Buff / Debuff
        //ReducirDuracionBuffs();


        //Consumibles
        ChequearTieneConsumibles();

        //Cooldowns
        ReducirCooldowns();

        //Cargar habilidades
        ResolverCargarHabilidades();

        //Remover Reacciones no permanentes
        RemoverReacciones();

        //Reduce duracion marcas
        ReducirDuracionMarcas();

        //Chequear Trampas Persistentes en su casilla
        Invoke("ChequearTrampaPersistenteenCasilla", 0.1f); //Se invoca con un delay para que se aplique después de los efectos de inicio de turno




      }

      //Si es IA, se manda a comenzar su turno.
      if (GetComponent<IAUnidad>() != null)
      {
        GetComponent<IAUnidad>().RealizarTurnoIA();
      }


    }

  }

  void ChequearTrampaPersistenteenCasilla()  //0 referencias porque se Invoka--Chequear si hay trampas persistentes en la casilla, y aplicar sus efectos al arrancar el turno de la unidad
  {
       // Obtener todos los componentes Trampa en la casilla actual 
        Trampa[] trampas = CasillaPosicion.transform.GetComponentsInChildren<Trampa>();
        foreach (Trampa trmp in trampas)
        {
            // Si la trampa es persistente, aplicar sus efectos
            if (trmp.esPersistente)
            {
              print($"aplicando efectos{ trmp.nombre } trampa persistente a "+ uNombre);
              trmp.AplicarEfectosTrampa(this);
            }

        }

  }

 void RemoverReacciones()
{
  foreach(Reaccion reaccion in gameObject.GetComponents<Reaccion>())
  {
    if(!reaccion.permanente)
    {
      Destroy(reaccion);
    }

  }
}
void ReducirDuracionMarcas()
{
   Marca[] marcas = gameObject.GetComponents<Marca>();

        foreach (Marca marca in marcas)
        {
            marca.duracion--;

            if (marca.duracion == 0) // no se chequea < 1, porque los buffs eternos arrancan en -1 duracion
            {
               Destroy(marca);
            }
        }

}
void ReducirDuracionBuffs()
{
  Buff[] buffs = gameObject.GetComponents<Buff>();

        foreach (Buff buff in buffs)
        { 
            buff.DuracionBuffRondas--;

            if (buff.DuracionBuffRondas == 0) // no se chequea < 1, porque los buffs eternos arrancan en -1 duracion
            {
                buff.RemoverBuff(this); 
            }
        }

     ChequearHayBarricadaAdelante();
}

// Llama los efectos custom de buffs al inicio del turno
void ActivarEfectosCustomBuffsInicioTurno()
{
  Buff[] buffs = gameObject.GetComponents<Buff>();
  foreach (Buff buff in buffs)
  {
    buff.activarCustomEffectInicioTurno();
  }
}
  void ReducirCooldowns()
  {
    foreach (Habilidad hab in gameObject.GetComponents<Habilidad>())
    {
      if (hab.cooldownActual > 0) { hab.cooldownActual--; }


    }
    foreach (IAHabilidad hab in gameObject.GetComponents<IAHabilidad>())
    {
      if (hab.hActualCooldown > 0) { hab.hActualCooldown--; }


    }

  }

  public List<Unidad> ObtenerTodosEnemigos()
  { 
    List<Unidad> lEnemigos = new List<Unidad>();
    foreach (Unidad u in scBattleManager.lUnidadesTotal)
    {
      if (u.CasillaPosicion.lado != CasillaPosicion.lado)
      {
        lEnemigos.Add(u);
      }
    }
    return lEnemigos;

  }
 void ChequearTieneConsumibles()
{
 
  if(ConsumibleA != null)
  {
    BattleManager.Instance.botonConsumibleA.SetActive(true);
    BattleManager.Instance.botonConsumibleA.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ConsumibleA.sNombreItem;
    BattleManager.Instance.botonConsumibleA.GetComponent<Image>().sprite = ConsumibleA.imItem;
  }
  else
  {
    BattleManager.Instance.botonConsumibleA.SetActive(false);
  }

   if(ConsumibleB != null)
  {
    BattleManager.Instance.botonConsumibleB.SetActive(true);
    BattleManager.Instance.botonConsumibleB.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ConsumibleB.sNombreItem;
    BattleManager.Instance.botonConsumibleB.GetComponent<Image>().sprite = ConsumibleB.imItem;
  }
  else
  {
    BattleManager.Instance.botonConsumibleB.SetActive(false);
  }

}
  public void TerminaTurnoEstaUnidad()
  {

    //-------------------------------Defensa_BonusPASinUsar
    if (AccionP_actual > 0 && gameObject.GetComponent<RetrasarTurno>().yaRetraso == false)
    {
      if (AccionP_actual > 2) { AccionP_actual = 2; }

      Defensa_BonusPASinUsar += (int)AccionP_actual;
    }
    //---

    LlamarReacciones(5, this, false); //Reacciones al terminar turno

    ReducirDuracionBuffs(); //Se reduce duracion de buffs/debuffs al terminar el turno
    
    ControlarSiEsDescanso();

}

void ControlarSiEsDescanso()
{
  //Controlar si es descanso
  //Descanso: si termina el turno con todos los AP, el próximo turno gana el buff Descansado (+1AP +3 Iniciativa)
  if(AccionP_actual >= mod_maxAccionP && gameObject.GetComponent<RetrasarTurno>().yaRetraso == false && estaCargando == null && !ChequearTieneReaccionesTipo(-1))
  {
    /////////////////////////////////////////////
    //BUFF ---- Así se aplica un buff/debuff
     Buff Descansado = new Buff();
     Descansado.buffNombre = "Descansado";
     Descansado.boolfDebufftBuff = true;
     Descansado.DuracionBuffRondas = 1;
     Descansado.cantIniciativa += 3;
     Descansado.cantAPMax += 1;
     Descansado.AplicarBuff(this);
     // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
     Buff buffComponent = ComponentCopier.CopyComponent(Descansado, gameObject);
  }

}
private void ResolverCargarHabilidades()
{
  if(estaCargando != null)
  {
   
    if( AccionP_actual < valorCargando) //Si todavía no le alcanzan los AP carga 1 turno mas
    {
       valorCargando -= (int)AccionP_actual;


       scBattleManager.EscribirLog(uNombre+TRADU.i.Traducir(" sigue canalizando."));

       BattleManager.Instance.TerminarTurno();

       
       
    }
    else //Si le alcanzan los AP
    {
      ActivarHabilidadCargada();
      
      //estaCargando = null;
    
       
     

    }


  }
  else
  {
  

  }


}
private void ActivarHabilidadCargada()
{
  Transform BotoneraHabilidades = BattleManager.Instance.scUIBotonesHab.transform;

 
  foreach (Transform botonHab in BotoneraHabilidades)
  {
   
      if(botonHab.GetComponent<BotonHabilidad>().HabilidadRepresentada == estaCargando)
      {
        botonHab.GetComponent<BotonHabilidad>().HabilidadRepresentada.cooldownActual = 0;
        botonHab.GetComponent<BotonHabilidad>().ActivarHabilidad(true);
        
      }   
  }

 


}


private void OnDestroy() 
{
    scBattleManager.OnRondaNueva  -= BattleManager_OnRondaNueva;
    scBattleManager.lUnidadesTotal.Remove(this); 
}
public virtual void PerderEscondido()
{
  estaEscondido = 0;
  scBattleManager.EscribirLog(uNombre + TRADU.i.Traducir(" ya no está escondido."));
  gameObject.transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(false);
  //aca agregar tratamientos de vfx de revelar etc.
}
public virtual void GanarEscondido(int n) // n es Tier de Escondido, 1 se va al recibir daño u atacar, 2 no se va al recibir daño ni atacar
{
  estaEscondido = n;
  scBattleManager.EscribirLog(uNombre + TRADU.i.Traducir(" está escondido."));
  gameObject.transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(true);
  //aca agregar tratamientos de vfx de esconderse etc.
}
public int ObtenerEstaEscondido() // nes Tier de Escondido, 1 se va al recibir daño u atacar, 2 no se va al recibir daño ni atacar
{
  return estaEscondido;
  //aca agregar tratamientos de vfx de esconderse etc.
}

public virtual void OcasionoDanioaEnemigo(Unidad victima, int tipoDanio, bool esCritico, float danio)
{
  //---
  LlamarReacciones(4, victima, false, tipoDanio, danio);

  
}

  public virtual void RemoverBuffNombre(string nombreBuff)
  {
    Buff[] buffs = this.GetComponents<Buff>();
    // Recorre cada buff y realiza una acción
    foreach (Buff buff in buffs)
    {
      if (buff.buffNombre == nombreBuff)
      {
        buff.RemoverBuff(this);
      }
    }

  }

  public bool TieneBuffNombre(string nombreBuff)
  {
    Buff[] buffs = this.GetComponents<Buff>();
    // Recorre cada buff y realiza una acción
    foreach (Buff buff in buffs)
    {
      if (buff.buffNombre == nombreBuff)
      {
        return true;
      }
    }
    return false;
  }

  [Header("Sonidos")]
  public List<AudioClip> sonidosRecibirDanio = new List<AudioClip>();
  private AudioSource audioSource;

  /// <summary>
  /// Reproduce un sonido aleatorio de la lista de sonidos de recibir daño.
  /// </summary>
  private int ultimoSonidoDanioIndex = -1;
  public async Task ReproducirSonidoRecibirDanio(int tipodanio)
  {
   
  // Reproduce un sonido específico según el tipo de daño antes del await
  if (BattleManager.Instance != null && BattleManager.Instance.contenedorPrefabs != null)
  {
     await Task.Delay(20);
    AudioClip clip = null;
    switch (tipodanio)
    {
      case 1: // Cortante
        clip = BattleManager.Instance.contenedorPrefabs.sonidoCortante;
        break;
      case 2: // Perforante
        clip = BattleManager.Instance.contenedorPrefabs.sonidoPerforante;
        break;
      case 3: // Contundente
        clip = BattleManager.Instance.contenedorPrefabs.sonidoContundente;
        break;
      case 4: // Fuego
        clip = BattleManager.Instance.contenedorPrefabs.sonidoFuego;
        break;
      case 5: // Hielo
        clip = BattleManager.Instance.contenedorPrefabs.sonidoHielo;
        break;
      case 6: // Rayo
        clip = BattleManager.Instance.contenedorPrefabs.sonidoElectrico;
        break;
      case 7: // Ácido
        clip = BattleManager.Instance.contenedorPrefabs.sonidoAcido;
        break;
      case 8: // Arcano
        clip = BattleManager.Instance.contenedorPrefabs.sonidoArcano;
        break;
      case 9: // Necro
        clip = BattleManager.Instance.contenedorPrefabs.sonidoNecro;
        break;
      case 10: // Verdadero
        clip = BattleManager.Instance.contenedorPrefabs.sonidoVerdadero;
        break;
      case 11: // Divino
        clip = BattleManager.Instance.contenedorPrefabs.sonidoDivino;
        break;
    }
    if (clip != null)
    {
      if (audioSource == null)
      {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
          audioSource = gameObject.AddComponent<AudioSource>();
        }
      }
      audioSource.PlayOneShot(clip);
    }
  }
   
   
   
    await Task.Delay(300);
    if (sonidosRecibirDanio != null && sonidosRecibirDanio.Count > 0)
    {
      if (audioSource == null)
      {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
          audioSource = gameObject.AddComponent<AudioSource>();
        }
      }
      int index;
      do
      {
        index = UnityEngine.Random.Range(0, sonidosRecibirDanio.Count);
      } while (sonidosRecibirDanio.Count > 1 && index == ultimoSonidoDanioIndex);

      ultimoSonidoDanioIndex = index;
      audioSource.PlayOneShot(sonidosRecibirDanio[index]);
    }
  }
  public async virtual void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
  {
    await Task.Delay(delayEfectos); //Delay para que se vea el efecto de daño en la unidad antes de aplicar el daño
    float danioFinal = 0;
    if (estado_invulnerable == 0)
    {
      if (estaEscondido == 1) //Si esta escondido "1" (el escondido 2 perdura igual) y recibe daño, pierde el "escondido"
      { PerderEscondido(); }

      if (estado_congelado > 0 && tipoDanio != 5) //Reduce estado congelado cada vez que es golpeado por daño no frío
      {
        estado_congelado--;
        if (tipoDanio == 4) { estado_congelado = 0; } //Si recibe daño fuego, remueve congelado.
      }

      if (estado_ardiendo > 0 && tipoDanio == 5) { estado_ardiendo = 0; } //Si recibe daño hielo y está ardiendo, remueve fuego.


      if (scUnidadCanvas.unidadCanvas != null)
      {
        GameObject goDanioRecibido = Instantiate(scUnidadCanvas.PrefabtxtDaño, scUnidadCanvas.unidadCanvas.transform);
        scUnidadCanvas.txtDaño = goDanioRecibido.GetComponent<TextMeshProUGUI>();
      }



      if (esCritico) //Al ser crítico el daño se aplican los efectos segun tipo de daño
      {

        switch (tipoDanio)
        {
          case 1: danioFinal = EfectosDanios.CriticoCortante(danio, this); scUnidadCanvas.txtDaño.color = Color.red; ReducirArmaduraPorGolpe(danioFinal);break; //Cortante
          case 2: danioFinal = EfectosDanios.CriticoPerforante(danio, this); scUnidadCanvas.txtDaño.color = Color.red; ReducirArmaduraPorGolpe(danioFinal);break; //Perforante
          case 3: danioFinal = EfectosDanios.CriticoContundente(danio, this); scUnidadCanvas.txtDaño.color = Color.red; ReducirArmaduraPorGolpe(danioFinal);break; //Contundente
          case 4: danioFinal = EfectosDanios.CriticoFuego(danio, this); scUnidadCanvas.txtDaño.color = Color.red; break; //Fuego
          case 5: danioFinal = EfectosDanios.CriticoHielo(danio, this); scUnidadCanvas.txtDaño.color = Color.cyan; break; //Hielo
          case 6: danioFinal = EfectosDanios.CriticoRayo(danio, this); scUnidadCanvas.txtDaño.color = Color.yellow; break; //Rayo
          case 7: danioFinal = EfectosDanios.CriticoAcido(danio, this); scUnidadCanvas.txtDaño.color = Color.green; break; //Acido
          case 8: danioFinal = EfectosDanios.CriticoArcano(danio, this); scUnidadCanvas.txtDaño.color = Color.blue; break; //Arcano
          case 9: danioFinal = EfectosDanios.CriticoNecro(danio, this); scUnidadCanvas.txtDaño.color =  new Color(0.8f, 1f, 0.6f); break; //Necro
          case 10: danioFinal = danio; scUnidadCanvas.txtDaño.color = Color.white; break; //Verdadero
          case 11: danioFinal = EfectosDanios.CriticoDivino(danio, this); scUnidadCanvas.txtDaño.color = Color.yellow; break; //Divino
        }

      }
      else
      {
        switch (tipoDanio)
        {
          case 1: danioFinal = danio - mod_Armadura; scUnidadCanvas.txtDaño.color = Color.red; ReducirArmaduraPorGolpe(danioFinal);  break; //Cortante
          case 2: danioFinal = danio - mod_Armadura; scUnidadCanvas.txtDaño.color = Color.red; ReducirArmaduraPorGolpe(danioFinal);  break; //Perforante
          case 3: danioFinal = danio - mod_Armadura; scUnidadCanvas.txtDaño.color = Color.red; ReducirArmaduraPorGolpe(danioFinal);  break; //Contundente
                                                                                                      //Los 3 primeros tipos de daño (físico) al ser golpeado y dañado, reduce en 1 la armadura.
          case 4: danioFinal = danio - ObtenerResistenciaA(1); scUnidadCanvas.txtDaño.color = Color.red; break; //Fuego
          case 5: danioFinal = danio - ObtenerResistenciaA(2); scUnidadCanvas.txtDaño.color = Color.cyan; break;//Hielo
          case 6: danioFinal = danio - ObtenerResistenciaA(3); scUnidadCanvas.txtDaño.color = Color.yellow; break; //Rayo
          case 7: danioFinal = danio - ObtenerResistenciaA(4); scUnidadCanvas.txtDaño.color = Color.green; break; //Acido
          case 8: danioFinal = danio - ObtenerResistenciaA(5); scUnidadCanvas.txtDaño.color = Color.blue; break; //Arcano
          case 9: danioFinal = danio - ObtenerResistenciaA(6); scUnidadCanvas.txtDaño.color = new Color(0.8f, 1f, 0.6f); break; //Necro
          case 10: danioFinal = danio; scUnidadCanvas.txtDaño.color = Color.white; break; //Verdadero
          case 11: danioFinal = danio - ObtenerResistenciaA(7); scUnidadCanvas.txtDaño.color = Color.yellow; break; //Divino
        }


      }

      if (esEtereo && tipoDanio < 4)
      {
        danioFinal = danioFinal / 2;
      }

      float danioBloqueado = Mathf.Min(danioFinal, barreraDeDanio); // Daño que la barrera absorbe
      if (danioBloqueado > 0)
      {
        barreraDeDanio -= danioBloqueado; // Reducimos la barrera
        danioFinal -= danioBloqueado; // Reducimos el daño que pasará a la unidad

        // Aseguramos que la barrera no quede en valores negativos
        if (barreraDeDanio < 0) barreraDeDanio = 0;

        if (danioBloqueado > 0)
        { BattleManager.Instance.EscribirLog(TRADU.i.Traducir("La Barrera de ") + uNombre + TRADU.i.Traducir(" absorbió ") + danioBloqueado + TRADU.i.Traducir(" de daño.")); }
      }




      if (danioFinal < 0) { danioFinal = 0; }
      if (danioFinal > 0)
      {
        ReproducirAnimacionRecibirDanio();
        ChequearCorrompidoVsCorrupto(uCausante, danioFinal);
        estado_evasion = 0;
        if (uCausante != null)
        {
          uCausante.OcasionoDanioaEnemigo(this, tipoDanio, esCritico, danioFinal); //se le avisa al causante que le hizo daño a la unidad, para lo que sea.
          LlamarReacciones(2, uCausante, false, danio, tipoDanio); //Llama a las reacciones de la unidad que recibe el daño.
        }
        if (danioFinal > 2)
        {
          await ReproducirSonidoRecibirDanio(tipoDanio);
        }
      }

      scUnidadCanvas.txtDaño.text = "";
      await Task.Delay(700);
      HP_actual -= (int)danioFinal;
      scUnidadCanvas.txtDaño.text = "-"+(int)danioFinal;
      if (esCritico) { scUnidadCanvas.txtDaño.text += "!"; }


      string stDaniotipo = "";
      switch (tipoDanio)
      {
        case 1: stDaniotipo = "<color=#c5c5c5>cortante</color>"; break; //Cortante
        case 2: stDaniotipo = "<color=#c69360>perforante</color>"; break; //Perforante
        case 3: stDaniotipo = "<color=#c67f60>contundente</color>"; break; //Contundente
        case 4: stDaniotipo = "<color=#ce3715>fuego</color>"; break; //Fuego
        case 5: stDaniotipo = "<color=#63c4b7>hielo</color>"; break; //Hielo
        case 6: stDaniotipo = "<color=#7758df>rayo</color>"; break; //Rayo
        case 7: stDaniotipo = "<color=#28b717>ácido</color>"; break; //Acido
        case 8: stDaniotipo = "<color=#1760b7>arcano</color>"; break; //Arcano
        case 9: stDaniotipo = "<color=#8038b2>necrótico</color>"; break; //Necro
        case 10: stDaniotipo = "<color=#d6c304>verdadero</color>"; break; //Verdadero
        case 11: stDaniotipo = "<color=#d6c304>divino</color>"; break; //Divino
      }


    
        if (TRADU.i.nIdioma == 1)
        {
          scBattleManager.EscribirLog($"<color=#d92b08>{uNombre} recibe {danioFinal} de daño {stDaniotipo}.</color>");
        }
        else if (TRADU.i.nIdioma == 2)
        {
          scBattleManager.EscribirLog($"<color=#d92b08>{uNombre} takes {danioFinal} {TRADU.i.Traducir(stDaniotipo)} damage.</color>");
        }


      
    


   
      ActualizarBarraVidaPropia();
      

      //Chequear si queda Herido (25% menos de vida, recibe herida)
      if (HP_actual < (mod_maxHP * 0.25))
      {
        RecibirHerida();

      }
      if (uCausante != null)
      {
        uCausante.AcabaDeHacerDañoA(this);
      }
      //Chequear si muere
      if (HP_actual < 1)
      {
        if (uCausante != null)
        {

          uCausante.AcabaDeMatarUnidad(this);

          if (uCausante.TieneTag("Corrupto"))
          {
            
            loMatoCorrompido = true; //Si el causante es corrupto, se marca que la unidad fue muerta por un corrupto.
          }

        }


        UnidadMuere();

      }

      if (uCausante != null)
      {
        if (uCausante.bonusdam_acido > 0)
        {
          RecibirDanioBonusElemental(uCausante.bonusdam_acido, 7, uCausante);
        }
        if (uCausante.bonusdam_arcano > 0)
        {
          RecibirDanioBonusElemental(uCausante.bonusdam_arcano, 8, uCausante);
        }
        if (uCausante.bonusdam_fuego > 0)
        {
          RecibirDanioBonusElemental(uCausante.bonusdam_fuego, 4, uCausante);
        }
        if (uCausante.bonusdam_hielo > 0)
        {
          RecibirDanioBonusElemental(uCausante.bonusdam_hielo, 5, uCausante);
        }
        if (uCausante.bonusdam_necro > 0)
        {
          RecibirDanioBonusElemental(uCausante.bonusdam_necro, 9, uCausante);
        }
        if (uCausante.bonusdam_rayo > 0)
        {
          RecibirDanioBonusElemental(uCausante.bonusdam_rayo, 6, uCausante);
        }
        if (uCausante.bonusdam_divino > 0)
        {
          RecibirDanioBonusElemental(uCausante.bonusdam_divino, 11, uCausante);
        }
      }
      BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();

    }
    else
    {
      if (TieneBuffNombre("Invulnerable"))
      { GenerarTextoFlotante( TRADU.i.Traducir("Invulnerable"), Color.gray); }

    }
  }

  void ChequearCorrompidoVsCorrupto(Unidad causante, float danio)
  { 
    if (causante != null && causante.TieneTag("Corrupto"))
    {
      if (estado_Corrupto)
      {
        // Aplicar efectos de corrupción
        float danioCorrupto = danio * 0.15f; // Ejemplo: 15% de daño adicional

        RecibirDanioBonusElemental(danioCorrupto, 9, this);
        causante.RecibirCuracion(danioCorrupto, false);
      }
      
    }
  }
public async virtual void RecibirDanioBonusElemental(float Xddanio, int tipoDanio, Unidad uCausante)
  {
    float danioFinal = 0;
    int danio = UnityEngine.Random.Range(1, (int)Xddanio + 1);
    //el daño del buff elemental es 1d(buff elemental), o sea si tiene 3 de buff, el daño es 1d3.

    if (estado_invulnerable == 0 && HP_actual > 0)
    {
      if (estaEscondido == 1) //Si esta escondido "1" (el escondido 2 perdura igual) y recibe daño, pierde el "escondido"
      { PerderEscondido(); }

      if (estado_congelado > 0 && tipoDanio != 5) //Reduce estado congelado cada vez que es golpeado por daño no frío
      {
        estado_congelado--;
        if (tipoDanio == 4) { estado_congelado = 0; } //Si recibe daño fuego, remueve congelado.
      }

      if (estado_ardiendo > 0 && tipoDanio == 5) { estado_ardiendo = 0; } //Si recibe daño hielo y está ardiendo, remueve fuego.


      if (scUnidadCanvas.unidadCanvas != null)
      {
        GameObject goDanioRecibido = Instantiate(scUnidadCanvas.PrefabtxtDaño, scUnidadCanvas.unidadCanvas.transform);
        scUnidadCanvas.txtDaño = goDanioRecibido.GetComponent<TextMeshProUGUI>();
      }




      switch (tipoDanio)
      {
        case 1: danioFinal = danio - mod_Armadura; scUnidadCanvas.txtDaño.color = Color.red; break; //Cortante
        case 2: danioFinal = danio - mod_Armadura; scUnidadCanvas.txtDaño.color = Color.red; break; //Perforante
        case 3: danioFinal = danio - mod_Armadura; scUnidadCanvas.txtDaño.color = Color.red; break; //Contundente
        //Los 3 primeros tipos de daño (físico) al ser golpeado y dañado, reduce en 1 la armadura.
        case 4: danioFinal = danio - ObtenerResistenciaA(1); scUnidadCanvas.txtDaño.color = Color.red; break; //Fuego
        case 5: danioFinal = danio - ObtenerResistenciaA(2); scUnidadCanvas.txtDaño.color = Color.cyan; break;//Hielo
        case 6: danioFinal = danio - ObtenerResistenciaA(3); scUnidadCanvas.txtDaño.color = Color.yellow; break; //Rayo
        case 7: danioFinal = danio - ObtenerResistenciaA(4); scUnidadCanvas.txtDaño.color = Color.green; break; //Acido
        case 8: danioFinal = danio - ObtenerResistenciaA(5); scUnidadCanvas.txtDaño.color = Color.blue; break; //Arcano
        case 9: danioFinal = danio - ObtenerResistenciaA(6); scUnidadCanvas.txtDaño.color =  new Color(0.8f, 1f, 0.6f); break; //Necro
        case 10: danioFinal = danio; scUnidadCanvas.txtDaño.color = Color.white; break; //Verdadero
        case 11: danioFinal = danio - ObtenerResistenciaA(7); scUnidadCanvas.txtDaño.color = Color.yellow; break; //Divino
      }




      if (esEtereo && tipoDanio < 4)
      {
        danioFinal = danioFinal / 2;
      }








      if (danioFinal < 0) { danioFinal = 0; }
      if (danioFinal > 0)
      {
       /* ReproducirAnimacionRecibirDanio();*/ estado_evasion = 0;
      }

      scUnidadCanvas.txtDaño.text = "";
      await Task.Delay(700);
      HP_actual -= (int)danioFinal;
      scUnidadCanvas.txtDaño.text = "-"+(int)danioFinal;

      string stDaniotipo = "";
      switch (tipoDanio)
      {
        case 1: stDaniotipo = "<color=#c5c5c5>cortante</color>"; break; //Cortante
        case 2: stDaniotipo = "<color=#c69360>perforante</color>"; break; //Perforante
        case 3: stDaniotipo = "<color=#c67f60>contundente</color>"; break; //Contundente
        case 4: stDaniotipo = "<color=#ce3715>fuego</color>"; break; //Fuego
        case 5: stDaniotipo = "<color=#63c4b7>hielo</color>"; break; //Hielo
        case 6: stDaniotipo = "<color=#7758df>rayo</color>"; break; //Rayo
        case 7: stDaniotipo = "<color=#28b717>ácido</color>"; break; //Acido
        case 8: stDaniotipo = "<color=#1760b7>arcano</color>"; break; //Arcano
        case 9: stDaniotipo = "<color=#8038b2>necrótico</color>"; break; //Necro
        case 10: stDaniotipo = "<color=#d6c304>verdadero</color>"; break; //Verdadero
        case 11: stDaniotipo = "<color=#d6c304>divino</color>"; break; //Divino
      }


       if (TRADU.i.nIdioma == 1)
        {
          scBattleManager.EscribirLog($"<color=#d92b08>{uNombre} recibe {danioFinal} de daño elemental extra {stDaniotipo}.</color>");
        }
        else if (TRADU.i.nIdioma == 2)
        {
          scBattleManager.EscribirLog($"<color=#d92b08>{uNombre} takes {danioFinal} {TRADU.i.Traducir(stDaniotipo)} extra damage.</color>");
        }



      ActualizarBarraVidaPropia();

      //Chequear si queda Herido (25% menos de vida, recibe herida)
      if (HP_actual < (mod_maxHP * 0.25))
      {
        RecibirHerida();
      }

      uCausante.AcabaDeHacerDañoA(this);
      //Chequear si muere
      if (HP_actual < 1)
      {
        if (uCausante != null)
        {

          uCausante.AcabaDeMatarUnidad(this);

        }


        UnidadMuere();

      }

    }
    else
    {
       if (TieneBuffNombre("Invulnerable"))
      { GenerarTextoFlotante( TRADU.i.Traducir("Invulnerable"), Color.gray); }


    }






  }
public virtual void ReducirArmaduraPorGolpe(float danioFinal)
{
  if(danioFinal > 0){estado_armaduraModificador++;}
}
 void RecibirHerida()
{
   bool yaEstaHerido = false;
   Buff[] buffs = gameObject.GetComponents<Buff>();
   foreach(Buff buff in buffs)
   {
      if(buff.buffNombre == "Herida"){ yaEstaHerido = true; }
   
   }

   if(!yaEstaHerido && !TieneTag("Etereo") && !TieneTag("Nomuerto"))
    {
    
      //BUFF ---- Así se aplica un buff/debuff
       Buff Herida = new Buff();
       Herida.buffNombre = "Herida";
       Herida.boolfDebufftBuff = false;
       Herida.DuracionBuffRondas = -1;
       Herida.cantAtFue = -1;
       Herida.cantAtAgi = -1;
       Herida.cantAtPod = -1;
       Herida.cantAPMax -= 1;
       Herida.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(Herida, gameObject);
       //--------------------------------------

    }

}
public virtual void AcabaDeMatarUnidad(Unidad uVictima)
{
 SumarValentia(1);
 //Aca poner efectos que sean al "matar enemigo". Extender en las clases hijas.
}
  public virtual void AcabaDeHacerDañoA(Unidad uVictima)
  {
     RemoverBuffNombre("Escondido Por Humo");
 
  }

public virtual void FalloAtaqueRecibido(Unidad uOrigen, bool melee)
{
 LlamarReacciones(1, uOrigen, melee);
}
public virtual void SumarValentia(int cant)
{
  if(GetComponent<IAUnidad>() == null)
  {
      ChequearBuffsDeValentia(ValentiaP_actual, cant);
       ValentiaP_actual += cant; 
      
      if(ValentiaP_actual > mod_maxValentiaP)
      {
        ValentiaP_actual = mod_maxValentiaP;
      }
       
      Instantiate(scUnidadCanvas.PrefabtxtValentia, scUnidadCanvas.unidadCanvas.transform, false);
     
      TextMeshProUGUI txtVAl = scUnidadCanvas.PrefabtxtValentia.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
      if(cant > 0)
      {txtVAl.text = "+ "+cant; } else {txtVAl.text = " "+cant; }

     switch (TRADU.i.nIdioma)
     {
      case 1:
        scBattleManager.EscribirLog($"{uNombre} gana {cant} de Valentía.");
        break;
      case 2:
        scBattleManager.EscribirLog($"{uNombre} obtains {cant} Valor.");
        break;
     } 

      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);
  }
}
public virtual void ChequearBuffsDeValentia(float inicial, float cambio)
{ 
  float valorInicial = inicial;
  float valorFinal = inicial + cambio;

     if(valorFinal > mod_maxValentiaP)
      {
        valorFinal = mod_maxValentiaP;
      }
  //Positivos
  #region
  //Motivado--------
  if(valorInicial < 3 && valorFinal >= 3) //Chequea si tiene 3 o mas valentia al haber tenido menos antes
  {
     AplicarMotivado();
  }
  if(valorInicial >= 3 && valorFinal < 3) 
  {
      
       Buff[] buffs = gameObject.GetComponents<Buff>();
       foreach(Buff buff in buffs)
       {
        if(buff.buffNombre == "Motivado"){ buff.RemoverBuff(this); }
   
       }
  }
  //Motivado fin--------

  //Eufórico--------
  if(valorInicial < 5 && valorFinal >= 5) //Chequea si tiene 5 o mas valentia al haber tenido menos antes
  {
    AplicarEuforico();
  }
  if(valorInicial >= 5 && valorFinal < 5) 
  {
      
       Buff[] buffs = gameObject.GetComponents<Buff>();
       foreach(Buff buff in buffs)
       {
        if(buff.buffNombre == "Euforia"){ buff.RemoverBuff(this); }
   
       }
  }
  //Euforia fin--------
  #endregion

  //Negativos
   #region
  //Desmotivado--------
  if(valorInicial > -3 && valorFinal <= -3) 
  {
    AplicarDesmotivado();
  }
  if(valorInicial <= -3 && valorFinal > -3) 
  {
      
       Buff[] buffs = gameObject.GetComponents<Buff>();
       foreach(Buff buff in buffs)
       {
        if(buff.buffNombre == "Desmotivado"){ buff.RemoverBuff(this); }
   
       }
  }
  //Motivado fin--------

  //Desesperanzado--------
  if(valorInicial > -5 && valorFinal <= -5) //Chequea si tiene 5 o mas valentia al haber tenido menos antes
  {
     AplicarDesesperanzado();
  }
  if(valorInicial <= -5 && valorFinal > -5) 
  {
      
       Buff[] buffs = gameObject.GetComponents<Buff>();
       foreach(Buff buff in buffs)
       {
        if(buff.buffNombre == "Desesperanzado"){ buff.RemoverBuff(this); }
   
       }
  }
  //Euforia fin--------
  #endregion

}


public virtual void AplicarMotivado()
{
 /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Motivado";
       motivado.boolfDebufftBuff = true;
       motivado.DuracionBuffRondas = -1;
       motivado.cantTsMental += 2;
       motivado.cantTsFortaleza += 1;
       motivado.cantTsReflejos += 1;
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
}
public virtual void AplicarDesmotivado()
{
   /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Desmotivado";
       motivado.boolfDebufftBuff = false;
       motivado.DuracionBuffRondas = -1;
       motivado.cantTsMental -= 2;
       motivado.cantTsFortaleza -= 1;
       motivado.cantTsReflejos -= 1;
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
}
public virtual void AplicarEuforico()
{
   /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Euforia";
       motivado.boolfDebufftBuff = true;
       motivado.DuracionBuffRondas = -1;
       motivado.cantAtFue += 1;
       motivado.cantAtPod += 1;
       motivado.cantAtAgi += 1;
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
}
public virtual void AplicarDesesperanzado()
{
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
       Buff motivado = new Buff();
       motivado.buffNombre = "Desesperanzado";
       motivado.boolfDebufftBuff = false;
       motivado.DuracionBuffRondas = -1;
       motivado.cantAtFue -= 1;
       motivado.cantAtPod -= 1;
       motivado.cantAtAgi -= 1;
       motivado.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
}

 
  public async Task GenerarTextoFlotante(string txString, Color color)
  {

  // Encuentra todos los objetos existentes del prefab scUnidadCanvas.PrefabtxtDaño

  // Calcula el retraso total en milisegundos
   int delayPerObject = UnityEngine.Random.Range(0, 800); // Retraso entre 100 y 500 ms por objeto
   GameObject[] existingTextObjects = GameObject.FindGameObjectsWithTag(scUnidadCanvas.PrefabtxtDaño.tag);

   int totalDelay = delayPerObject * existingTextObjects.Length;

     // Espera el tiempo calculado
     await Task.Delay(totalDelay);

       // Instancia el nuevo objeto
      GameObject goTextoFlotante = Instantiate(scUnidadCanvas.PrefabtxtDaño, scUnidadCanvas.unidadCanvas.transform, false);
      TextMeshProUGUI txtMesh = goTextoFlotante.GetComponent<TextMeshProUGUI>();

      // Configura el texto y el color
       txtMesh.text = txString;
      txtMesh.color = color;
     

        // Genera el texto flotante
    TextoFlotanteManager.Instance.GenerarTextoFlotante(txString, color);
    }


public void RecibirCuracion(float curacion, bool magica)
{
  
 //Cada stack de sangrado previene 2 de curación y se elimina
 float curaFinal = curacion;
 curaFinal -= estado_sangrado*2;
 curaFinal -= tejidoCuracMagica; //Resta el tejido curativo mágico
 if(curaFinal < 0 ){ curaFinal = 0;}

 estado_sangrado -= (int)(curacion/2);
 if(estado_sangrado < 0){estado_sangrado = 0;}

 
 if(curaFinal > 0 && HP_actual < mod_maxHP)
 { 
   HP_actual += (int)curaFinal;
  if(HP_actual > mod_maxHP){HP_actual = mod_maxHP; }
  GenerarTextoFlotante(TRADU.i.Traducir("Cura ")+(int)curaFinal, Color.green);
  scBattleManager.EscribirLog(uNombre+TRADU.i.Traducir(" recibe <color=#11c66b>") +curaFinal+TRADU.i.Traducir("</color> de curación."));

  if(magica){ tejidoCuracMagica += (int)curaFinal/5;} //Cada 5 curación mágica se suma 1 de residuo tejido curativo que previene 1 de futuras curaciones.
 }

 ActualizarBarraVidaPropia();
 
 

}

  public void ActualizarBarraVidaPropia()
  {
    if (scUnidadCanvas != null && scUnidadCanvas.barraVida != null && mod_maxHP > 0f)
    {
      // float ratio = Mathf.Clamp01(HP_actual / mod_maxHP);
      //scUnidadCanvas.barraVida.value = ratio;
    }

    if (BattleManager.Instance != null && BattleManager.Instance.scUIInfoChar != null)
    {
      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);
    }
  
    
}

private bool yaMurio = false;
public void UnidadMuere()
{  
  if(!yaMurio)
  {
   yaMurio = true;

   scBattleManager.OnRondaNueva  -= BattleManager_OnRondaNueva;
   int posicionUnidad =  BattleManager.Instance.lUnidadesTotal.IndexOf(this)+1;
   scBattleManager.lUnidadesTotal.Remove(this); 
   CasillaPosicion.ladoGO.GetComponent<LadoManager>().unidadesLado.Remove(this);
   AccionP_actual = 0;
   
   LlamarReacciones(3,this, false); //Reacciones al morir
   

   BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
  
   if(posicionUnidad >= BattleManager.Instance.indexTurno && BattleManager.Instance.unidadActiva == this)
   {
     BattleManager.Instance.indexTurno--;
   }
   if(BattleManager.Instance.unidadActiva == this && gameObject.GetComponent<IAUnidad>() == null)
   {
    BattleManager.Instance.TerminarTurno(); //termina el turno del personaje no IA que muera
   }
   
   ReproducirAnimacionMorir();
   


   if (GetComponent<IAUnidad>() == null)  //Resta 3 valentía a cada aliado al morir una unidad no IA del lado del jugador
   {
     foreach(Unidad aliado in BattleManager.Instance.ladoB.GetComponent<LadoManager>().unidadesLado)
     {
        if(aliado.gameObject != gameObject) //si no es esta misma unidad
        {
         aliado.SumarValentia(-3);
        }
     }
   }
   
   CasillaPosicion.Presente = null;
   Invoke("DesactivarGOconDelay", 1.4f); 


    if(CasillaPosicion.lado == 1)
    {
          scBattleManager.EscribirLog($"<color=#d92b08>"+uNombre+TRADU.i.Traducir(" muere.")+"</color>");

    }
    else
    {
          scBattleManager.EscribirLog($""+uNombre+TRADU.i.Traducir(" muere.")+"");
    }


    ChequearEsteror();

  }
}

void ChequearEsteror()
{






}

void DesactivarGOconDelay()
{
   scBattleManager.lUnidadesTotal.Remove(this); 
   
  BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
  gameObject.SetActive(false);

  
  BattleManager.Instance.ChequearFinBatalla();
}

public void AplicarDebuffPorAtaquesreiterados(int cant)
{
     if(Defensa_AtaquesRepetidosRonda < 3) //Aplica el debuff al objetivo, que al ser atacado pierde 1 defensa por la ronda 
     {
       Defensa_AtaquesRepetidosRonda += cant;
     }
}

public float ObtenerdefensaActual()
{
  float defensa = mod_Defensa - Defensa_AtaquesRepetidosRonda + Defensa_BonusPASinUsar - AccionP_SeEsforzo + estado_evasion;
  
  if(estado_aturdido  > 0){ defensa -=5; defensa -= estado_evasion;}
  if(estado_congelado > 0){ defensa -=2; defensa -= estado_evasion;}


  return defensa;
}
public float ObtenerArmaduraActual()
{
   
    float res = mod_Armadura - estado_armaduraModificador - estado_acido + estado_congelado*2;
    
    if(res < 0){res = 0;}

    return res;
}

public float ObtenerResistenciaA(int tipo)
{
   float res = 0;
   switch(tipo)
   {
     case 1: res = mod_ResFuego; break; //Fuego
     case 2: res = mod_ResHielo; break; //Hielo
     case 3: res = mod_ResRayo; break; //Rayo
     case 4: res = mod_ResAcido; break; //Ácido
     case 5: res = mod_ResArcano; break; //Arcano
     case 6: res = mod_ResNecro; break; //Necro
     case 7: res = mod_ResDivino; break; //Divino
   }
     
    res -= estado_ResistenciasReducidas;
    

    return res ;
}




public void Marcar(int n)
{
    if (scUnidadCanvas != null)
    {
      if (n == 1)
      {
        scUnidadCanvas.imMarcador.SetActive(true);
      }
      else
      {
        scUnidadCanvas.imMarcador.SetActive(false);
      }
    }

}
Unidad anterior = null;
public void OnMouseEnter() 
{
 
    anterior = null;
    Marcar(1);

    if(BattleManager.Instance.scUIInfoChar.unidadMostrada != BattleManager.Instance.unidadActiva)
    {anterior = BattleManager.Instance.scUIInfoChar.unidadMostrada; }
    
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);

     if(scBattleManager.SeleccionandoObjetivo)
     {
        CasillaPosicion.OnMouseEnter();

     }

}
public void OnMouseExit() 
{
   
    if(anterior != null)
    {
        BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(anterior);
       if(anterior != BattleManager.Instance.unidadActiva)
        {anterior.Marcar(1);}

     
    }
    else
    { 
      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(BattleManager.Instance.unidadActiva);
     
    }

      if(scBattleManager.SeleccionandoObjetivo)
     {
        CasillaPosicion.OnMouseExit();

     }
}
public async void OnMouseDown() 
{
  
  if(scBattleManager.lUnidadesPosiblesHabilidadActiva.Contains(this) && scBattleManager.SeleccionandoObjetivo)
  {
    
    if(scBattleManager.HabilidadActiva.esHostil && ObtenerEstaEscondido() > 0)
    {
      //Si se quiere hacer una habilidad hostil a una unidad escondida, no hace nada.
    }
    else
    {

    
    string sss = "Se resuelve la habilidad "+scBattleManager.HabilidadActiva.nombre+" hecha por "+scBattleManager.HabilidadActiva.gameObject+ " a "+ this;




        if (scBattleManager.HabilidadActiva.esZonal && !BattleManager.Instance.bOcupado)
        { /* print(scBattleManager.HabilidadActiva+"");
      List<object> listResolver = new List<object>();
      listResolver.AddRange(scBattleManager.lObstaculosPosiblesHabilidadActiva);
      listResolver.AddRange(scBattleManager.lUnidadesPosiblesHabilidadActiva);

      await scBattleManager.HabilidadActiva.Resolver(listResolver);
      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(anterior);*///  VIEJO
          print(000);
          CasillaPosicion.OnMouseDown();
        }
        else if (scBattleManager.HabilidadActiva.targetEspecial > 0 && !BattleManager.Instance.bOcupado)
        { 
           CasillaPosicion.OnMouseDown();

        }
        else if(!BattleManager.Instance.bOcupado)
        {
          List<object> listaUno = new List<object> { this };
          await scBattleManager.HabilidadActiva.Resolver(listaUno);
          BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(anterior);
        }
   }
  }
  else
  {
    if(anterior == this)
    {
      BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo = false;
      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(anterior);
      Marcar(0);  anterior = null;
    }
    else if( BattleManager.Instance.scUIInfoChar.unidadMostrada != this || BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo == false || anterior != null)
    {
      BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);
      BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo = true;
      if(gameObject.GetComponent<IAUnidad>())
      {BattleManager.Instance.scUIInfoChar.infoEnemigos.SetActive(true); BattleManager.Instance.scUIInfoChar.mostrardesc = true;}
      Marcar(1);anterior = this;
    }
    else 
    {
       BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo = false;
       Marcar(0); anterior = null;
    }

  }

 
}

  public bool TiradaSalvacion(float atributoDefiende, float dificultadHabilidada) //TRUE no se salva FALSE se salva (xd)
    {
      bool resultado = false;

      
      float iTiradaDefensa = UnityEngine.Random.Range(1,21);

      float iResultadoAtaque =  dificultadHabilidada;
      float iResultadoDefensa = iTiradaDefensa + atributoDefiende;

      resultado = iResultadoAtaque > iResultadoDefensa;

    if (resultado) //positivo NO se salva
    {
      BattleManager.Instance.EscribirLog(uNombre+TRADU.i.Traducir(" realiza Tirada de Salvación: 1d20 = ") + iTiradaDefensa + " +" + atributoDefiende + " vs Tirada Dificultad: " + iResultadoAtaque + ". Resultado: No se salva.");
    }
    else //NegativoSeSalva
    {
      BattleManager.Instance.EscribirLog(uNombre+TRADU.i.Traducir(" realiza Tirada de Salvación: 1d20 = ") + iTiradaDefensa + " +" + atributoDefiende + " vs Tirada Dificultad: " + iResultadoAtaque + ". Resultado: Se salva.");
      GenerarTextoFlotante(TRADU.i.Traducir("Resiste"), Color.green);
   
    }

    return resultado;
    }

  
  public void ForzarMoverAPrimeraFila()
  {
    if(CasillaPosicion.posX == 3){ /*Nada*/ }
    if(CasillaPosicion.posX == 2)
    { 
       Casilla casillaFrontal = CasillaPosicion.ladoGO.GetComponent<LadoManager>().ObtenerCasillaPorIndex(CasillaPosicion.posX+1, CasillaPosicion.posY);
       if(casillaFrontal.Presente == null)
       {   
            CasillaForzadoaMover = casillaFrontal;
       } 
    }
     if(CasillaPosicion.posX == 1)
    { 
       Casilla casillaFrontal = CasillaPosicion.ladoGO.GetComponent<LadoManager>().ObtenerCasillaPorIndex(CasillaPosicion.posX+2, CasillaPosicion.posY);
       if(casillaFrontal.Presente == null)
       {
            CasillaForzadoaMover = casillaFrontal;
       }
       else
       {
        Casilla casillaFrontal2 = CasillaPosicion.ladoGO.GetComponent<LadoManager>().ObtenerCasillaPorIndex(CasillaPosicion.posX+1, CasillaPosicion.posY);
         if(casillaFrontal2.Presente == null)
         {
            CasillaForzadoaMover = casillaFrontal2;
         } 

       } 
    }


  }

  public void EmpujarUnidad(int cantidad)
  {

    print("EMPUJAR");
    if (cantidad == 1)
    {
      //Casilla atras
      Casilla casillaAtras1 = CasillaPosicion.ladoGO.GetComponent<LadoManager>().ObtenerCasillaPorIndex(CasillaPosicion.posX - 1, CasillaPosicion.posY);
      if (casillaAtras1 != null)
      {
        if (casillaAtras1.Presente == null)
        {
          CasillaForzadoaMover = casillaAtras1;
        }
      }
    
    }

    if (cantidad == 2)
    {
      Casilla casillaAMover;
      bool sepuedeaCas1 = false;
      bool sepuedeaCas2 = false;
      //Casilla atras
      Casilla casillaAtras1 = CasillaPosicion.ladoGO.GetComponent<LadoManager>().ObtenerCasillaPorIndex(CasillaPosicion.posX - 1, CasillaPosicion.posY);
      //Casilla atras
      Casilla casillaAtras2 = CasillaPosicion.ladoGO.GetComponent<LadoManager>().ObtenerCasillaPorIndex(CasillaPosicion.posX - 2, CasillaPosicion.posY);
      if (casillaAtras1 != null)
      {
        if (casillaAtras1.Presente == null)
        {
         sepuedeaCas1 = true;  
        }
      }
       if (sepuedeaCas1 && casillaAtras2 != null)
      { 
        if (casillaAtras2.Presente == null)
        {
         sepuedeaCas2 = true;
        }
      }

      //Si las dos casillas de atrás están libres, elige la más atrás
      if (sepuedeaCas1 && sepuedeaCas2)
      {
        //Si las dos casillas de atrás están libres, elige la más atrás
        casillaAMover = casillaAtras2;
      }
      else if (sepuedeaCas1 && !sepuedeaCas2)
      {
        //Si solo la primera está libre, mueve a esa
        casillaAMover = casillaAtras1;
      }
      else
      {
        //Si ninguna está libre, no se mueve
        return;
      }
     
      CasillaForzadoaMover = casillaAMover;
    }
  

  
  

  }

  public virtual void ActualizarClaseComienzoTurno() //Método vacío que se llama cada vez que arranca turno de la unidad
  {
    //---
    //VACIO
    //Cada clase lo usará para determinar ciertos efectos en cada turno
  }
  public virtual void ComienzoBatallaClase() //Método vacío que se llama al comenzar la batalla
  {

    TirarIniciativa();
   
  }

  public void AcomodarSortingLayerDelay()
  { 
     Invoke("AcomodarSortingLayer", 2.0f);

  }
void AcomodarSortingLayer()
{
    
    // 1) Encontrar el/los canvas aunque estén desactivados
    var canvases = GetComponentsInChildren<Canvas>(true);
    if (canvases == null || canvases.Length == 0)
    {
        print($"{name}: no encontré Canvas en hijos");
        return;
    }

    // 2) Orden por Y de la casilla (fallback por posición mundial)
    int y = (CasillaPosicion != null) ? CasillaPosicion.posY
                                      : Mathf.RoundToInt(transform.position.y);
    int orden = 60 - (y * 10);

    foreach (var c in canvases)
    {
        // Recomendado: World Space (o Screen Space - Camera con tu cámara de batalla)
        if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            c.renderMode = RenderMode.WorldSpace;

        c.overrideSorting = true;
        // Ponelo en la misma Sorting Layer que tus unidades, o una por encima (p.e. "UI3D")
        c.sortingLayerID = SortingLayer.NameToID("UI3D"); // ajusta el nombre a tu proyecto
        c.sortingOrder   = orden;
        // (Opcional) si usas ScreenSpace-Camera:
        // c.worldCamera = Camera.main; // o tu cámara de batalla
    }

    Canvas.ForceUpdateCanvases();

}
  public void LlamarReacciones(int tipo, Unidad unidadtercero, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)  //tipo de Trigger de la reaccion en cuestión
  {
    foreach(Reaccion reaccion in gameObject.GetComponents<Reaccion>())
    {
      if(reaccion.TipoTrigger == tipo)
      {
        reaccion.AplicarEfectos(unidadtercero, melee, variableFlexible1, variableFlexible2);
      }
    }
  }

  public bool ChequearTieneReaccionesTipo(int tipo)  //Para la IA - Si tipo -1, chequea simplemente si tiene reaciiones
  {
    foreach(Reaccion reaccion in gameObject.GetComponents<Reaccion>())
    {
      if(tipo == -1){return true;} //Si encuentra alguna reacción y el tipo buscado es -1 (cualquiera) devuelve true
      if(reaccion.TipoTrigger == tipo)
      {
        return true;
      }
    }
    return false;
  }
  
  public List<Unidad> ObtenerListaAliados(bool incluirEsta)
  { 
    CasillaPosicion.ladoGO.GetComponent<LadoManager>().ActualizarListaDeUnidadesEnLado();
    List<Unidad> aliados = new List<Unidad>();

    List<Unidad> unidadesLado = CasillaPosicion.ladoGO.GetComponent<LadoManager>().unidadesLado;
   

    foreach(Unidad unidad in unidadesLado)
    { 
      if(unidad == this && incluirEsta)
      {
        aliados.Add(unidad);
      }


      if(unidad != this)
      {
        aliados.Add(unidad);
      }
      
    }
    return aliados;
  }
 
   public List<Unidad> ObtenerListaEnemigos()
  {  CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>().ActualizarListaDeUnidadesEnLado();
    List<Unidad> enemigos = new List<Unidad>();
    foreach(Unidad unidad in CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>().unidadesLado)
    {
      enemigos.Add(unidad);
    }
    return enemigos;
  }

  public virtual void ChequearSeMovio()
  {

    if (TieneBuffNombre("Escondido Por Humo") && !(this is ClaseAcechador)) //Si no es Acechador, pierde el escondido por salir del humo
    {
      PerderEscondido();
      RemoverBuffNombre("Escondido Por Humo");
      
    }
    

    if (TieneBuffNombre("Nido Defensivo") && !CasillaPosicion.GetComponent<TrampaNidoDefensivo>())
    {

      RemoverBuffNombre("Nido Defensivo");
    }


   ChequearHayBarricadaAdelante();
   ChequearHombroConHombroSePierde(); //Habilidad clase caballero

  }

  void ChequearHayBarricadaAdelante()
  { 
     // Si la casilla de adelante en el eje X tiene un obstáculo llamado "Barricada", gana un buff
    if (CasillaPosicion != null)
    {
      // Obtener la casilla de adelante (eje X +1)
      Casilla casillaAdelante = CasillaPosicion.ladoGO.GetComponent<LadoManager>().ObtenerCasillaPorIndex(CasillaPosicion.posX + 1, CasillaPosicion.posY);
      if (casillaAdelante != null && casillaAdelante.Presente != null)
      {
        GameObject obstaculo = casillaAdelante.Presente;
        if (obstaculo.GetComponent<Obstaculo>() == null) { return; } // Si no es un obstáculo, no hacemos nada
        if (obstaculo.GetComponent<Obstaculo>().oName.Contains("Barricada"))
        {
          // Aplica el buff "Cobertura de Barricada" si no lo tiene
          if (!TieneBuffNombre("Cobertura de Barricada"))
          {
            Buff cobertura = new Buff();
            cobertura.buffNombre = "Cobertura de Barricada";
            cobertura.boolfDebufftBuff = true;
            cobertura.DuracionBuffRondas = -1;
            cobertura.cantDefensa += 1;
            cobertura.AplicarBuff(this);
            Buff buffComponent = ComponentCopier.CopyComponent(cobertura, gameObject);
          }
        }
        else
        {
          // Si ya no hay barricada, remueve el buff si lo tiene
          RemoverBuffNombre("Cobertura de Barricada");
        }
      }
      else
      {
        // Si no hay casilla adelante o no hay obstáculo, remueve el buff si lo tiene
        RemoverBuffNombre("Cobertura de Barricada");
      }
    }
  }

  public bool ChequearEstaAislado(int xAlre) //Si no tiene aliados en xAlre ver "ObtenerCasillasAlrededor"
  {
    bool estaAislado = true;

    List<Casilla> casillasAlrededor = CasillaPosicion.ObtenerCasillasAlrededor(xAlre);

    foreach (Casilla casilla in casillasAlrededor)
    {
      if (casilla.Presente != null && casilla.Presente.GetComponent<Unidad>() != null)
      {
        estaAislado = false;
        break;
      }
    }
    return estaAislado;
  }

  public void RemoverfDebuffstBuffs(bool n) //Remueve false = Debuffs, true = Buffs
  { 
    Buff[] buffs = gameObject.GetComponents<Buff>();

    foreach(Buff buff in buffs)
    {
      if(buff.boolfDebufftBuff == n && buff.esRemovible) 
      {
        buff.RemoverBuff(this);
        
      }
    }

  }

  public void TeletransportarACasilla(Casilla cas)
  {

    CasillaPosicion.Presente = null; //En habilidades teletransporte importante sacarlo de la casilla origen
    CasillaForzadoaMover = null;
    CasillaDeseadaMov = null;
    cas.PonerObjetoEnCasilla(gameObject);/**/
    LlegoACasilla(cas);
    scBattleManager.CalcularCasillasAMovimiento();
    ChequearSeMovio();
              

  }



  public int PorcentajeVidaActual()
  { 
    if(HP_actual <= 0){ return 0; }
    int porcentaje = (int)(HP_actual * 100 / mod_maxHP);
    return porcentaje;


  }

  void ChequearHombroConHombroSePierde()
  {
       Buff[] buffs = gameObject.GetComponents<Buff>();
       List<Casilla> casillasLado = BattleManager.Instance.ladoB.casillasLado;

       foreach(Buff buff in buffs)
       {
        if(buff.buffNombre == "Hombro Con Hombro")
        { 
          //buff.RemoverBuff(this);
          
          //----
           foreach(Casilla cas in casillasLado)
           {
             if(cas.Presente != null)
            {
             if(cas.Presente.GetComponent<Unidad>() != null)
             {
              Unidad presente = cas.Presente.GetComponent<Unidad>();
              Buff[] bbuffs = presente.GetComponents<Buff>();
              foreach(Buff abuff in bbuffs)
              {
                if(abuff.buffNombre == "Hombro Con Hombro"){ abuff.RemoverBuff(presente); }
          
              }

              }
            }

           }
        }
   
       }

      

       
  }
}
