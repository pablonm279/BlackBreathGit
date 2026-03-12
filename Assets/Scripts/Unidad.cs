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

  private static readonly Color ColorDanioFuego = new Color(1f, 0.65f, 0.25f, 1f);
  private static readonly Color ColorValentiaGanada = new Color(0.27f, 0.94f, 0.58f, 1f);
  private static readonly Color ColorValentiaPerdida = new Color(1f, 0.36f, 0.36f, 1f);

   [Header ("Lógica")]
   public Casilla CasillaPosicion;
   public Casilla CasillaDeseadaMov;

   public Casilla CasillaForzadoaMover;
   // Marca si hay un desplazamiento forzado en curso para evitar reordenarlo cada frame
  private bool movimientoForzadoPendiente;
  

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
  private const float PorcentajeVidaPorPuntoFuerza = 0.05f;
  private float bonusVidaPorFuerzaAplicado;
  private float agilidadReferenciaDefensa;
  private float bonusDefensaPorAgilidadAplicado;
  private float poderReferenciaResElemental;
  private float bonusResElementalPorPoderAplicado;
  private bool statsInicializados;
  private float ultimaFuerzaSincronizada;
  private float ultimaAgilidadSincronizada;
  private float ultimoPoderSincronizado;

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
  

   public Alcambiarvalorflash scTextoArmaduraFlash; 

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
 public int estado_Condenado; //En (stacks) turnos. Al llegar a 0 inflige 5% de HP max por cada turno consecutivo que estuvo activo.
 [HideInInspector] public int estado_CondenadoTurnosSeguidos; //Cuenta turnos seguidos con la condena activa para el danio acumulado.
 public int estado_Escudado; //10% por stack de prevenir un ataque de daño fisico. Pierde 1 stack.
 public bool estado_Corrupto;
 public bool unidadVoladora;
 public bool estado_Volando;


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
  [HideInInspector] public List<DebuffImpactoArmaData> debuffsImpactoArma = new List<DebuffImpactoArmaData>();

 [Header("Defensa de equipo")]
 public int reduccionDanioRecibidoPorcentaje;
 public int reduccionDanioCriticoRecibidoPorcentaje;
 public int resistenciaEstadosPorcentaje;
 public int espinasDanioPlano;
 public int espinasDanioPorcentaje;

 [Header("Ataque de equipo")]
 public int penetracionArmaduraPlano;
 [HideInInspector] public int penetracionArmaduraHabilidadActual;

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
  private bool suprimirAnimacionIA;
  private float ultimoAtaqueAnimTime = -999f;
  private float ultimoHabilidadAnimTime = -999f;
  private Vector2 uImagePosVuelo;
  private Vector2 uImagePosSuelo;
  private Coroutine animacionVueloCoroutine;
  private bool uImagePosInicializada;
  [SerializeField] private float offsetVueloY = 13f;
  [SerializeField] private float duracionAnimacionVuelo = 0.4f;
  

  UnidadCanvas scUnidadCanvas;
  [SerializeField] private bool apilarTextosFlotantes = false;
  [SerializeField] private float floatingTextSlotSpacing = 9f;
  [SerializeField] private float floatingTextSlotLifetimeFallback = 1.1f;
  [SerializeField] private int floatingTextMaxSlots = 4;
  [SerializeField] private float floatingTextMinInterval = 0.05f;
  [SerializeField] private int floatingTextCrowdThreshold = 3;
  [SerializeField] private float floatingTextCrowdIntervalStep = 0.01f;
  [SerializeField] private float floatingTextCrowdExtraIntervalMax = 0.03f;
  private float nextFloatingTextTime = -999f;
  private readonly List<float> floatingTextSlotExpiries = new List<float>();
  private readonly List<Renderer> renderersOcultosPorEscondido = new List<Renderer>();
  private bool unidadCanvasOcultadoPorEscondido;
  private bool imagenOcultadaPorEscondido;
  private int ultimoEstadoVisualEscondido = int.MinValue;
  private bool ultimoOcultamientoTotalPorEscondido;
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
    if (GetComponent<UnidadIdleMotion>() == null)
    {
      gameObject.AddComponent<UnidadIdleMotion>();
    }
    if (GetComponent<UnidadStatusVfxController>() == null)
    {
      gameObject.AddComponent<UnidadStatusVfxController>();
    }
    if (GetComponent<UnidadHiddenVisualController>() == null)
    {
      gameObject.AddComponent<UnidadHiddenVisualController>();
    }
    InicializarVueloVisual();

    ValentiaP_actual = 0;
   
  
  Transform child4 = transform.childCount > 4 ? transform.GetChild(4) : null;

  if (child4 != null && child4.childCount >= 2)
  {
    puntoSaliente = child4.GetChild(0);
    puntoEntrante = child4.GetChild(1);
  }
  SincronizarVisualEscondido();
}

  void InicializarVueloVisual()
  {
    if (uImagePosInicializada || uImage == null) { return; }

    RectTransform rectTransform = uImage.rectTransform;
    uImagePosVuelo = rectTransform.anchoredPosition;
    uImagePosSuelo = uImagePosVuelo + Vector2.down * offsetVueloY;
    uImagePosInicializada = true;
  }

 bool evitarRepeticion = false;
  public void ReproducirAnimacionAtaque(bool forzar = false)
  {
    if (suprimirAnimacionIA && !forzar)
    {
      return;
    }
    ultimoAtaqueAnimTime = Time.time;
    if (poseController != null) { poseController.PlayAttackPose(); }

    
    if (evitarRepeticion)
    {
      return;

    }
    if (animator != null)
    {
      animator.SetTrigger("Trigger_Ataque");

    }
    

    evitarRepeticion = true;
    Invoke("ResetearEvitarRepeticionAnimacion", 2.5f);
  }

   public void ReproducirAnimacionMiss(bool forzar = false)
  {
    if (suprimirAnimacionIA && !forzar)
    {
      return;
    }
    if (!gameObject.activeInHierarchy || HP_actual <= 0)
    {
      return;
    }

    if (animator != null)
    {
      animator.SetTrigger("Trigger_Miss");

    }
  
  }
  
  void ResetearEvitarRepeticionAnimacion()
  {
    evitarRepeticion = false;
  }
  public void ReproducirAnimacionTurnoNuevo()
  {
    if (animator != null)
    {
      animator.SetTrigger("Trigger_TurnoNuevo");

    }
  }
  
  public GameObject goSANGRE;


  public void ReproducirAnimacionRecibirDanio()
  {
    if (animator != null)
    {
      animator.SetTrigger("Trigger_Recibedanio");

    }

    if (goSANGRE != null)
    {
      Instantiate(goSANGRE, puntoEntrante.position, Quaternion.identity);
    }
  }

  public void ReproducirAnimacionHabilidadNoHostil(bool forzar = false)
  {
    if (suprimirAnimacionIA && !forzar)
    {
      return;
    }

    ultimoHabilidadAnimTime = Time.time;
    if(poseController != null){ poseController.PlaySkillPose(); }
  }

  public void SetSuprimirAnimacionIA(bool estado)
  {
    suprimirAnimacionIA = estado;
  }

  public bool ConsumirAnimacionAtaqueReciente(float ventanaSeg)
  {
    if (Time.time - ultimoAtaqueAnimTime <= ventanaSeg)
    {
      ultimoAtaqueAnimTime = -999f;
      return true;
    }
    return false;
  }

  public bool ConsumirAnimacionHabilidadReciente(float ventanaSeg)
  {
    if (Time.time - ultimoHabilidadAnimTime <= ventanaSeg)
    {
      ultimoHabilidadAnimTime = -999f;
      return true;
    }
    return false;
  }

  public bool EsAnimacionAtaqueRecienteDesde(float desdeTiempo, float ventanaSeg)
  {
    return ultimoAtaqueAnimTime >= desdeTiempo && (Time.time - ultimoAtaqueAnimTime) <= ventanaSeg;
  }

  public bool EsAnimacionHabilidadRecienteDesde(float desdeTiempo, float ventanaSeg)
  {
    return ultimoHabilidadAnimTime >= desdeTiempo && (Time.time - ultimoHabilidadAnimTime) <= ventanaSeg;
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
  SincronizarEscaladosPorAtributos();

 if(BattleManager.Instance.unidadActiva == this)
 {
    transform.GetChild(3).GetChild(0).GetChild(0).gameObject.SetActive(true);
 }
  else
 { transform.GetChild(3).GetChild(0).GetChild(0).gameObject.SetActive(false); }

    if (animator != null && unidadVoladora)
    {
      animator.enabled = estado_Volando;
       
      
    }
}

  void ActualizarAnimacionVuelo(Vector2 destino, bool instantaneo)
  {
    if (!uImagePosInicializada || uImage == null) { return; }

    if (animacionVueloCoroutine != null)
    {
      StopCoroutine(animacionVueloCoroutine);
    }

    RectTransform rectTransform = uImage.rectTransform;
    if (instantaneo || duracionAnimacionVuelo <= 0f)
    {
      rectTransform.anchoredPosition = destino;
      animacionVueloCoroutine = null;
      return;
    }

    animacionVueloCoroutine = StartCoroutine(AnimarVuelo(destino));
  }

  IEnumerator AnimarVuelo(Vector2 destino)
  {
    if (uImage == null) { animacionVueloCoroutine = null; yield break; }

    RectTransform rectTransform = uImage.rectTransform;
    Vector2 origen = rectTransform.anchoredPosition;
    float tiempo = 0f;

    while (tiempo < duracionAnimacionVuelo)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracionAnimacionVuelo);
      rectTransform.anchoredPosition = Vector2.Lerp(origen, destino, t);
      yield return null;
    }

    rectTransform.anchoredPosition = destino;
    animacionVueloCoroutine = null;
  }

  public void LevantarVuelo(bool instantaneo = false)
  {
    if (!unidadVoladora) { return; }

    estado_Volando = true;
    InicializarVueloVisual();
    ActualizarAnimacionVuelo(uImagePosVuelo, instantaneo);
  }

  public void BajarVuelo(bool instantaneo = false)
  {
    estado_Volando = false;
    InicializarVueloVisual();
    if (!unidadVoladora) { return; }

    // BUFF ---- Así se aplica un buff/debuff
    Buff buff = new Buff();
    buff.buffNombre = "Derribado";
    buff.boolfDebufftBuff = false;
    buff.DuracionBuffRondas = 1;
    buff.cantAPMax -= 2;
    buff.cantDefensa -= 3;
    buff.AplicarBuff(this);
    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
    Buff buffComponent = ComponentCopier.CopyComponent(buff, this.gameObject);

    ActualizarAnimacionVuelo(uImagePosSuelo, instantaneo);
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
  agilidadReferenciaDefensa = mod_CarAgilidad;
  bonusDefensaPorAgilidadAplicado = 0f;
  at_Ataque = Ataque;
  mod_Ataque = Ataque;

  at_TSFortaleza = TSFortaleza;
  mod_TSFortaleza = TSFortaleza;
  at_TSReflejos = TSReflejos;
  mod_TSReflejos = TSReflejos;
  at_TSMental = TSMental;
  mod_TSMental = TSMental;

  // La Fuerza escala vida maxima para todas las unidades (PJ e IA).
  RecalcularVidaPorFuerza(true);
  // La Agilidad escala defensa sobre una referencia inicial para mantener el valor de arranque.
  RecalcularDefensaPorAgilidad();
  // El Poder escala resistencias elementales sobre una referencia inicial para mantener el valor de arranque.
  poderReferenciaResElemental = mod_CarPoder;
  bonusResElementalPorPoderAplicado = 0f;
  RecalcularResElementalesPorPoder();

  statsInicializados = true;
  ultimaFuerzaSincronizada = mod_CarFuerza;
  ultimaAgilidadSincronizada = mod_CarAgilidad;
  ultimoPoderSincronizado = mod_CarPoder;
 
   
}

  private void SincronizarEscaladosPorAtributos()
  {
    if (!statsInicializados)
    {
      return;
    }

    if (!Mathf.Approximately(ultimaFuerzaSincronizada, mod_CarFuerza))
    {
      RecalcularVidaPorFuerza(true);
      ultimaFuerzaSincronizada = mod_CarFuerza;
    }

    if (!Mathf.Approximately(ultimaAgilidadSincronizada, mod_CarAgilidad))
    {
      RecalcularDefensaPorAgilidad();
      ultimaAgilidadSincronizada = mod_CarAgilidad;
    }

    if (!Mathf.Approximately(ultimoPoderSincronizado, mod_CarPoder))
    {
      RecalcularResElementalesPorPoder();
      ultimoPoderSincronizado = mod_CarPoder;
    }
  }

  public void RecalcularVidaPorFuerza(bool ajustarHPActual)
  {
    float bonusNuevo = at_maxHP * PorcentajeVidaPorPuntoFuerza * mod_CarFuerza;
    float delta = bonusNuevo - bonusVidaPorFuerzaAplicado;
    if (Mathf.Approximately(delta, 0f))
    {
      return;
    }

    mod_maxHP = Mathf.Max(1f, mod_maxHP + delta);
    if (ajustarHPActual)
    {
      HP_actual = Mathf.Clamp(HP_actual + delta, 0f, mod_maxHP);
    }
    else
    {
      HP_actual = Mathf.Clamp(HP_actual, 0f, mod_maxHP);
    }

    bonusVidaPorFuerzaAplicado = bonusNuevo;
    ActualizarBarraVidaPropia();

    // Si el ajuste de fuerza deja a la unidad sin vida durante combate, muere.
    if (HP_actual < 1f && BattleManager.Instance != null)
    {
      UnidadMuere();
    }
  }

  public float ObtenerBonusVidaPorFuerzaActual()
  {
    return bonusVidaPorFuerzaAplicado;
  }

  public void RecalcularDefensaPorAgilidad()
  {
    float bonusNuevo = mod_CarAgilidad - agilidadReferenciaDefensa;
    float delta = bonusNuevo - bonusDefensaPorAgilidadAplicado;
    if (Mathf.Approximately(delta, 0f))
    {
      return;
    }

    mod_Defensa += delta;
    bonusDefensaPorAgilidadAplicado = bonusNuevo;
  }

  public void RecalcularResElementalesPorPoder()
  {
    float bonusNuevo = mod_CarPoder - poderReferenciaResElemental;
    float delta = bonusNuevo - bonusResElementalPorPoderAplicado;
    if (Mathf.Approximately(delta, 0f))
    {
      return;
    }

    mod_ResFuego += delta;
    mod_ResRayo += delta;
    mod_ResHielo += delta;
    mod_ResAcido += delta;
    mod_ResArcano += delta;

    bonusResElementalPorPoderAplicado = bonusNuevo;
  }

  public void EstablecerResistenciaAcidoBase(float resistenciaAcido)
  {
    at_ResAcido = resistenciaAcido;
    mod_ResAcido = resistenciaAcido;
  }

public void ConfigurarDebuffsImpactoArma(Arma arma)
{
  debuffsImpactoArma = new List<DebuffImpactoArmaData>();
  if (arma == null || arma.debuffsImpactoArma == null)
  {
    return;
  }

  for (int i = 0; i < arma.debuffsImpactoArma.Count; i++)
  {
    DebuffImpactoArmaData origen = arma.debuffsImpactoArma[i];
    if (origen == null)
    {
      continue;
    }

    DebuffImpactoArmaData copia = new DebuffImpactoArmaData();
    copia.activo = origen.activo;
    copia.nombreDebuff = origen.nombreDebuff;
    copia.probabilidadAplicar = origen.probabilidadAplicar;
    copia.duracionRondas = origen.duracionRondas;
    copia.requiereTiradaSalvacion = origen.requiereTiradaSalvacion;
    copia.tipoTiradaSalvacion = origen.tipoTiradaSalvacion;
    copia.dificultadSalvacion = origen.dificultadSalvacion;
    copia.modFuerza = origen.modFuerza;
    copia.modAgilidad = origen.modAgilidad;
    copia.modPoder = origen.modPoder;
    copia.modIniciativa = origen.modIniciativa;
    copia.modAtaque = origen.modAtaque;
    copia.modDefensa = origen.modDefensa;
    copia.modArmadura = origen.modArmadura;
    copia.modDanioPorcentaje = origen.modDanioPorcentaje;
    copia.modTSReflejos = origen.modTSReflejos;
    copia.modTSFortaleza = origen.modTSFortaleza;
    copia.modTSMental = origen.modTSMental;
    copia.modResFuego = origen.modResFuego;
    copia.modResHielo = origen.modResHielo;
    copia.modResRayo = origen.modResRayo;
    copia.modResAcido = origen.modResAcido;
    copia.modResArcano = origen.modResArcano;
    copia.modResNecro = origen.modResNecro;
    copia.modResDivino = origen.modResDivino;
    copia.modCritDado = origen.modCritDado;
    copia.modCritDanioPorcentaje = origen.modCritDanioPorcentaje;
    copia.stacksSangrado = origen.stacksSangrado;
    copia.stacksArdiendo = origen.stacksArdiendo;
    copia.stacksCongelado = origen.stacksCongelado;
    copia.stacksAcido = origen.stacksAcido;
    copia.stacksAturdido = origen.stacksAturdido;
    copia.reduccionAPPorTurno = origen.reduccionAPPorTurno;
    copia.reduccionResistencias = origen.reduccionResistencias;
    copia.stacksCondenado = origen.stacksCondenado;
    copia.ignorarArmaduraPlano = origen.ignorarArmaduraPlano;
    copia.roboVidaPorcentaje = origen.roboVidaPorcentaje;
    copia.empujeCasillas = origen.empujeCasillas;
    copia.jalonCasillas = origen.jalonCasillas;
    debuffsImpactoArma.Add(copia);
  }
}

public float velocidadMovimiento = 2.8f; 
public bool movimientoEnCurso = false;
  // Casilla origen al comenzar el movimiento; se limpia Presente solo una vez
  private Casilla casillaOrigenEnMovimiento;
  private bool bonusAcercamientoValentiaAplicadoTurno;

  void Start()
  {
    Invoke("AcomodarSortingLayer", 1.15f); //Para que el sprite quede bien en el orden de sorting layer
    if (unidadVoladora)
    {
      LevantarVuelo(true);
    }
       
    if(goSANGRE == null && !esEtereo && (TieneTag("Humanoide")|| TieneTag("Bestia") || TieneTag("Criatura")|| TieneTag("Animal")))
    {
      goSANGRE = BattleManager.Instance.contenedorPrefabs.SangrePrefab;
    }
      scTextoArmaduraFlash = scUnidadCanvas.barraVida.GetChild(4).GetChild(0).gameObject.GetComponent<Alcambiarvalorflash>();
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
        movimientoForzadoPendiente = true;
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
          CasillaForzadoaMover = null;
          CasillaDeseadaMov = null;
          movimientoForzadoPendiente = false;
          movimientoEnCurso = false;
          casillaOrigenEnMovimiento = null;
          if (poseController != null) { poseController.OnStopMove(); }
        }
      }

    }
    else if (movimientoForzadoPendiente)
    {
      movimientoForzadoPendiente = false;
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
      transform.position = cas.transform.position;
    }

    BattleManager.Instance.ActualizarCasillasMelee();
    cas.ActualizarSenialadores();
}

  public void ArrancaTurnoEstaUnidad()
  {
    Invoke("AcomodarSortingLayer", 1.15f); //Para que el sprite quede bien en el orden de sorting layer
    bonusAcercamientoValentiaAplicadoTurno = false;

    if (gameObject.GetComponent<RetrasarTurno>() != null)
    {
      EscribirCabeceraTurnoEnLog();

      bool unidadEncarnadaEnTurno = TieneBuffNombre("Encarnado");

      if (gameObject.GetComponent<RetrasarTurno>().yaRetraso == false)
      {//Aplica los efectos de turno nuevo solo cuando no retrasí el turno, ya que si retrasa su turno y le vuelve a tocar despues, se aplicaria todo 2 veces.

        if (GetComponent<IAUnidad>() == null) { ReproducirAnimacionTurnoNuevo(); } //Las unidades no IA, tienen esa pequeña animación

        BattleManager.Instance.ReducirDuracionTrampasDeUnidad(this);

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

        //Volando
        if (unidadVoladora)
        {
          LevantarVuelo();
        }

        //Invulnerable
        if (estado_invulnerable > 0)
        {
          estado_invulnerable--;
        }

        //Regenera vida
        if (estado_regeneravida > 0) { Estados.Efecto_RegeneraVida(this); }

        //Condenado
        if (estado_Condenado > 0)
        {
          Estados.Efecto_Condenado(this);
        }
        else
        {
          estado_CondenadoTurnosSeguidos = 0;
        }

        //Regenera Armadura
        if (estado_regeneraarmadura > 0) { Estados.Efecto_RegeneraArmadura(this); }

        //Ejecuta efectos custom de Buffs al inicio del turno
        ActivarEfectosCustomBuffsInicioTurno();
        unidadEncarnadaEnTurno = TieneBuffNombre("Encarnado");
        if (unidadEncarnadaEnTurno)
        {
          NotificarUnidadEncarnadaEnTurno();
        }

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

        //Activar resaltar lado
        Invoke("ResaltarLado", 0.1f);

        //Señaladores direcciones
        CasillaPosicion.ActualizarSenialadores();

      }
      else if (unidadEncarnadaEnTurno)
      {
        NotificarUnidadEncarnadaEnTurno();
      }

      //Si es IA, se manda a comenzar su turno.
      if (GetComponent<IAUnidad>() != null)
      {
        GetComponent<IAUnidad>().RealizarTurnoIA();

        if (!unidadEncarnadaEnTurno)
        {
          GenerarTextoFlotante(TRADU.i.Traducir("Activa!"), Color.red);
        }
      }
      else 
      {
        if (!unidadEncarnadaEnTurno)
        {
          GenerarTextoFlotante(TRADU.i.Traducir("Activa!"), Color.cyan);
        }
      }
    }

  }

  private void EscribirCabeceraTurnoEnLog()
  {
    if (scBattleManager == null || CasillaPosicion == null)
    {
      return;
    }

    string nombreTurno = TRADU.i != null ? TRADU.i.Traducir(uNombre) : uNombre;
    bool esEnemigo = CasillaPosicion.lado == 1;
    string etiquetaTurno = TRADU.i != null
      ? TRADU.i.Traducir(esEnemigo ? "Turno Enemigo" : "Turno Aliado")
      : (esEnemigo ? "Turno Enemigo" : "Turno Aliado");
    string color = esEnemigo ? "#db3315" : "#3b75e0";

    scBattleManager.EscribirLog($"<size=130%><color={color}>=== {etiquetaTurno}: {nombreTurno} ===</color></size>");
  }

  private void NotificarUnidadEncarnadaEnTurno()
  {
    string estadoEncarnado = TRADU.i != null ? TRADU.i.Traducir("Encarnado") : "Encarnado";
    GenerarTextoFlotante(estadoEncarnado, new Color(0.85f, 0.45f, 1f));

    if (scBattleManager == null)
    {
      return;
    }

    string nombreUnidad = TRADU.i != null ? TRADU.i.Traducir(uNombre) : uNombre;
    string mensaje = TRADU.i != null
      ? TRADU.i.Traducir(" está encarnado y no puede actuar este turno.")
      : " está encarnado y no puede actuar este turno.";
    scBattleManager.EscribirLog(CombatLogFormatter.EventoEstado(nombreUnidad + mensaje));
  }

  void ResaltarLado()
  {
        CasillaPosicion.ladoGO.GetComponent<LadoManager>().ResaltarLadoActivo();
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
              if (!(inmunidad_Trampas && !trmp.esTrampaFavorable))
              {
                trmp.AplicarEfectosTrampa(this);
              }
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
    if (AccionP_actual > 0 && gameObject.GetComponent<RetrasarTurno>().yaRetraso == false && gameObject.GetComponent<IAUnidad>() == null)
    {
      if (AccionP_actual > 2) { AccionP_actual = 2; }

      Defensa_BonusPASinUsar += (int)AccionP_actual;
    }
    //---

    LlamarReacciones(5, this, false); //Reacciones al terminar turno

    ReducirDuracionBuffs(); //Se reduce duracion de buffs/debuffs al terminar el turno

    ControlarSiEsDescanso();
    
    CasillaPosicion.DesactivarSenialadores();

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

     string nombreUnidad = NombreUnidadParaLog(this);
     bool enIngles = IdiomaInglesActivo();
     string motivoDescanso = enIngles
       ? nombreUnidad + " ends turn without spending AP"
       : nombreUnidad + " termina el turno sin gastar AP";
     SumarValentia(2, motivoDescanso);
  }

}
private void ResolverCargarHabilidades()
{
  if(estaCargando != null)
  {
   
    if( AccionP_actual < valorCargando) //Si todavía no le alcanzan los AP carga 1 turno mas
    {
       valorCargando -= (int)AccionP_actual;


       scBattleManager.EscribirLog(CombatLogFormatter.EventoEstado(uNombre+TRADU.i.Traducir(" sigue canalizando.")));

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
  scBattleManager.EscribirLog(CombatLogFormatter.EventoEstado(uNombre + TRADU.i.Traducir(" ya no está escondido.")));
  gameObject.transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(false);
  SincronizarVisualEscondido();
  //aca agregar tratamientos de vfx de revelar etc.
}
public virtual void GanarEscondido(int n) // n es Tier de Escondido, 1 se va al recibir daño u atacar, 2 no se va al recibir daño ni atacar
{
  estaEscondido = n;
  scBattleManager.EscribirLog(CombatLogFormatter.EventoEstado(uNombre + TRADU.i.Traducir(" está escondido.")));
  gameObject.transform.GetChild(3).GetChild(1).GetChild(1).gameObject.SetActive(true);
  SincronizarVisualEscondido();
  //aca agregar tratamientos de vfx de esconderse etc.
}
public int ObtenerEstaEscondido() // nes Tier de Escondido, 1 se va al recibir daño u atacar, 2 no se va al recibir daño ni atacar
{
  return estaEscondido;
  //aca agregar tratamientos de vfx de esconderse etc.
}

public bool EsEnemigoParaJugador()
{
  return CasillaPosicion != null && CasillaPosicion.lado == 1;
}

public bool EstaOcultoVisualmenteParaJugador()
{
  return estaEscondido > 0 && EsEnemigoParaJugador();
}

public void SincronizarVisualEscondido()
{
  bool debeOcultarTotal = EstaOcultoVisualmenteParaJugador();
  if (ultimoEstadoVisualEscondido == estaEscondido
    && ultimoOcultamientoTotalPorEscondido == debeOcultarTotal)
  {
    return;
  }

  ultimoEstadoVisualEscondido = estaEscondido;
  ultimoOcultamientoTotalPorEscondido = debeOcultarTotal;

  if (debeOcultarTotal)
  {
    AplicarOcultamientoVisualTotalPorEscondido();
    return;
  }

  RestaurarOcultamientoVisualTotalPorEscondido();
}

private void AplicarOcultamientoVisualTotalPorEscondido()
{
  if (scUnidadCanvas != null && scUnidadCanvas.unidadCanvas != null && scUnidadCanvas.unidadCanvas.activeSelf)
  {
    scUnidadCanvas.unidadCanvas.SetActive(false);
    unidadCanvasOcultadoPorEscondido = true;
  }

  if ((scUnidadCanvas == null || scUnidadCanvas.unidadCanvas == null) && uImage != null && uImage.enabled)
  {
    uImage.enabled = false;
    imagenOcultadaPorEscondido = true;
  }

  if (renderersOcultosPorEscondido.Count == 0)
  {
    foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
    {
      if (renderer == null || !renderer.enabled)
      {
        continue;
      }

      renderer.enabled = false;
      renderersOcultosPorEscondido.Add(renderer);
    }
  }

  Marcar(0);
  anterior = null;

  BattleManager battleManager = BattleManager.Instance;
  if (battleManager != null
    && battleManager.scUIInfoChar != null
    && battleManager.scUIInfoChar.unidadMostrada == this)
  {
    battleManager.scUIInfoChar.hayUnidadSeleccionadaParaInfo = false;
    if (battleManager.unidadActiva != null)
    {
      battleManager.scUIInfoChar.ActualizarInfoChar(battleManager.unidadActiva);
    }
  }
}

private void RestaurarOcultamientoVisualTotalPorEscondido()
{
  if (unidadCanvasOcultadoPorEscondido && scUnidadCanvas != null && scUnidadCanvas.unidadCanvas != null)
  {
    scUnidadCanvas.unidadCanvas.SetActive(true);
  }
  unidadCanvasOcultadoPorEscondido = false;

  if (imagenOcultadaPorEscondido && uImage != null)
  {
    uImage.enabled = true;
  }
  imagenOcultadaPorEscondido = false;

  if (renderersOcultosPorEscondido.Count > 0)
  {
    foreach (Renderer renderer in renderersOcultosPorEscondido)
    {
      if (renderer != null)
      {
        renderer.enabled = true;
      }
    }

    renderersOcultosPorEscondido.Clear();
  }
}

public virtual void OcasionoDanioaEnemigo(Unidad victima, int tipoDanio, bool esCritico, float danio)
{
  //---
  LlamarReacciones(4, victima, false, tipoDanio, danio);

  if (esCritico && danio > 0f)
  {
    string nombreAtacante = NombreUnidadParaLog(this);
    string nombreVictima = NombreUnidadParaLog(victima);
    bool enIngles = IdiomaInglesActivo();
    string motivoCritico = enIngles
      ? nombreAtacante + " lands a critical hit on " + nombreVictima
      : nombreAtacante + " asesta un golpe crítico a " + nombreVictima;
    SumarValentia(1, motivoCritico);
  }

  
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
  private const float MultiplicadorVolumenSonidoArmadura = 0.75f;
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

    await Task.Delay(450);
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

  private void ReproducirSonidoArmadura()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.contenedorPrefabs == null)
    {
      return;
    }

    AudioClip clip = BattleManager.Instance.contenedorPrefabs.sonidoArmadura;
    if (clip == null)
    {
      return;
    }

    if (audioSource == null)
    {
      audioSource = GetComponent<AudioSource>();
      if (audioSource == null)
      {
        audioSource = gameObject.AddComponent<AudioSource>();
      }
    }

    audioSource.PlayOneShot(clip, MultiplicadorVolumenSonidoArmadura);
  }

  private int CalcularDanioBonusElemental(float Xddanio, int tipoDanio, out Color color)
  {
    color = Color.white;
    if (Xddanio <= 0)
    {
      return 0;
    }

    int danio = UnityEngine.Random.Range(1, (int)Xddanio + 1);
    float danioFinal = 0f;

    switch (tipoDanio)
    {
      case 1: danioFinal = danio - mod_Armadura; color = Color.red; break; //Cortante
      case 2: danioFinal = danio - mod_Armadura; color = Color.red; break; //Perforante
      case 3: danioFinal = danio - mod_Armadura; color = Color.red; break; //Contundente
      case 4: danioFinal = danio - ObtenerResistenciaA(1); color = ColorDanioFuego; break; //Fuego
      case 5: danioFinal = danio - ObtenerResistenciaA(2); color = Color.cyan; break;//Hielo
      case 6: danioFinal = danio - ObtenerResistenciaA(3); color = Color.yellow; break; //Rayo
      case 7: danioFinal = danio - ObtenerResistenciaA(4); color = Color.green; break; //Acido
      case 8: danioFinal = danio - ObtenerResistenciaA(5); color = Color.blue; break; //Arcano
      case 9: danioFinal = danio - ObtenerResistenciaA(6); color = new Color(0.8f, 1f, 0.6f); break; //Necro
      case 10: danioFinal = danio; color = Color.white; break; //Verdadero
      case 11: danioFinal = danio - ObtenerResistenciaA(7); color = Color.yellow; break; //Divino
    }

    if (esEtereo && tipoDanio < 4)
    {
      danioFinal = danioFinal / 2;
    }

    if (danioFinal < 0) { danioFinal = 0; }
    return (int)danioFinal;
  }

  private string ObtenerTextoTipoDanioLog(int tipoDanio)
  {
    string colorHex;
    string tipoDanioBase;

    switch (tipoDanio)
    {
      case 1: colorHex = "#c5c5c5"; tipoDanioBase = "cortante"; break;
      case 2: colorHex = "#8a5b32"; tipoDanioBase = "perforante"; break;
      case 3: colorHex = "#c67f60"; tipoDanioBase = "contundente"; break;
      case 4: colorHex = "#FFA64D"; tipoDanioBase = "fuego"; break;
      case 5: colorHex = "#63c4b7"; tipoDanioBase = "hielo"; break;
      case 6: colorHex = "#7758df"; tipoDanioBase = "rayo"; break;
      case 7: colorHex = "#28b717"; tipoDanioBase = "\u00E1cido"; break;
      case 8: colorHex = "#1760b7"; tipoDanioBase = "arcano"; break;
      case 9: colorHex = "#8038b2"; tipoDanioBase = "necr\u00F3tico"; break;
      case 10: colorHex = "#d6c304"; tipoDanioBase = "verdadero"; break;
      case 11: colorHex = "#d6c304"; tipoDanioBase = "divino"; break;
      default: colorHex = "#ffffff"; tipoDanioBase = "da\u00F1o"; break;
    }

    string etiquetaBase = $"<color={colorHex}>{tipoDanioBase}</color>";
    if (TRADU.i == null)
    {
      return etiquetaBase;
    }

    // 1) Traduccion exacta de la etiqueta (incluye color), que es como estan varias claves en TRADU.
    string etiquetaTraducida = TRADU.i.Traducir(etiquetaBase);
    if (!string.Equals(etiquetaTraducida, etiquetaBase, StringComparison.Ordinal))
    {
      return etiquetaTraducida;
    }

    // 2) Fallback: traducir solo la palabra.
    string tipoDanioTraducido = TRADU.i.Traducir(tipoDanioBase);
    if (!string.Equals(tipoDanioTraducido, tipoDanioBase, StringComparison.Ordinal))
    {
      return $"<color={colorHex}>{tipoDanioTraducido}</color>";
    }

    // 3) Fallback final para EN si faltan claves de TRADU.
    if (TRADU.i.nIdioma == 2)
    {
      switch (tipoDanio)
      {
        case 1: tipoDanioTraducido = "slashing"; break;
        case 2: tipoDanioTraducido = "piercing"; break;
        case 3: tipoDanioTraducido = "bludgeoning"; break;
        case 4: tipoDanioTraducido = "fire"; break;
        case 5: tipoDanioTraducido = "ice"; break;
        case 6: tipoDanioTraducido = "lightning"; break;
        case 7: tipoDanioTraducido = "acid"; break;
        case 8: tipoDanioTraducido = "arcane"; break;
        case 9: tipoDanioTraducido = "necrotic"; break;
        case 10: tipoDanioTraducido = "true"; break;
        case 11: tipoDanioTraducido = "divine"; break;
        default: tipoDanioTraducido = "damage"; break;
      }

      return $"<color={colorHex}>{tipoDanioTraducido}</color>";
    }

    return etiquetaBase;
  }
  private static string FormatearNumeroLogDanio(float valor)
  {
    return Mathf.Abs(valor % 1f) < 0.01f ? valor.ToString("0") : valor.ToString("0.##");
  }

  private string ConstruirDetalleCalculoDanio(
    float danioBase,
    bool esCritico,
    bool usoArmadura,
    float armaduraEfectiva,
    int penetracionTotal,
    bool usoResistencia,
    float resistenciaAplicada,
    bool aplicoMitadEtereo,
    float danioBloqueadoPorBarrera,
    int bonusElemental,
    int reduccionCritAplicada,
    int reduccionGlobalAplicada,
    int danioFinal)
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    List<string> partes = new List<string>(10)
    {
      (esIngles ? "base " : "base ") + FormatearNumeroLogDanio(danioBase)
    };

    if (esCritico)
    {
      partes.Add(esIngles ? "crit" : "crit");
    }

    if (usoArmadura && (armaduraEfectiva > 0f || penetracionTotal > 0))
    {
      if (penetracionTotal > 0)
      {
        partes.Add("arm " + FormatearNumeroLogDanio(armaduraEfectiva) + " (pen " + penetracionTotal + ")");
      }
      else
      {
        partes.Add("arm " + FormatearNumeroLogDanio(armaduraEfectiva));
      }
    }

    if (usoResistencia && Mathf.Abs(resistenciaAplicada) > 0.01f)
    {
      partes.Add("res " + FormatearNumeroLogDanio(resistenciaAplicada));
    }

    if (aplicoMitadEtereo)
    {
      partes.Add(esIngles ? "ethereal x0.5" : "etereo x0.5");
    }

    if (danioBloqueadoPorBarrera > 0.01f)
    {
      partes.Add((esIngles ? "bar " : "bar ") + FormatearNumeroLogDanio(danioBloqueadoPorBarrera));
    }

    if (reduccionCritAplicada > 0)
    {
      partes.Add("redCrit " + reduccionCritAplicada);
    }

    if (reduccionGlobalAplicada > 0)
    {
      partes.Add("red " + reduccionGlobalAplicada);
    }

    if (bonusElemental > 0)
    {
      partes.Add("+elem " + bonusElemental);
    }

    partes.Add((esIngles ? "final " : "final ") + danioFinal);
    return "[" + string.Join(" | ", partes) + "]";
  }

  private void EscribirLogDanioRecibido(string nombreLog, int danio, int tipoDanio, int bonusElemental = 0, bool esDanioElementalExtra = false, string detalleCalculo = null)
  {
    if (scBattleManager == null || danio <= 0)
    {
      return;
    }

    string tipoDanioTexto = ObtenerTextoTipoDanioLog(tipoDanio);
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    string mensaje;

    if (esDanioElementalExtra)
    {
      mensaje = esIngles
        ? $"<color=#d92b08>{nombreLog} takes {danio} {tipoDanioTexto} extra damage.</color>"
        : $"<color=#d92b08>{nombreLog} recibe {danio} de daño elemental extra {tipoDanioTexto}.</color>";
    }
    else if (esIngles)
    {
      string bonusLogEn = bonusElemental > 0 ? $" (+{bonusElemental} elemental bonus)" : string.Empty;
      mensaje = $"<color=#d92b08>{nombreLog} takes {danio} {tipoDanioTexto} damage{bonusLogEn}.</color>";
    }
    else
    {
      string bonusLogEs = bonusElemental > 0 ? $" (+{bonusElemental} bonus elemental)" : string.Empty;
      mensaje = $"<color=#d92b08>{nombreLog} recibe {danio} de daño {tipoDanioTexto}{bonusLogEs}.</color>";
    }

    if (!string.IsNullOrWhiteSpace(detalleCalculo))
    {
      mensaje += $" <size=82%><color=#c9c9c9>{detalleCalculo}</color></size>";
    }

    scBattleManager.EscribirLog(CombatLogFormatter.EventoDanio(mensaje));
  }

  public async virtual void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
  {
    await Task.Delay(delayEfectos); //Delay para que se vea el efecto de daño en la unidad antes de aplicar el daño
    float danioFinal = 0;
    bool textoDanioMostrado = false;
    bool absorbioPorArmadura = false;
    string nombreLog = TRADU.i != null ? TRADU.i.Traducir(uNombre) : uNombre;
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


      Color colorDanio = Color.white;
      float armaduraBaseContraAtaque = ObtenerArmaduraActual();
      int penetracionTotalContraAtaque = 0;
      if (uCausante != null)
      {
        int penetracionEquipo = Mathf.Max(0, uCausante.penetracionArmaduraPlano);
        int penetracionHabilidad = Mathf.Max(0, uCausante.penetracionArmaduraHabilidadActual);
        penetracionTotalContraAtaque = penetracionEquipo + penetracionHabilidad;
      }
      float armaduraEfectivaContraAtaque = armaduraBaseContraAtaque - penetracionTotalContraAtaque;
      if (armaduraEfectivaContraAtaque < 0f)
      {
        armaduraEfectivaContraAtaque = 0f;
      }

      bool usoArmaduraMitigacion = false;
      bool usoResistenciaMitigacion = false;
      float resistenciaAplicada = 0f;
      bool aplicoMitadEtereo = false;
      int reduccionCritAplicada = 0;
      int reduccionGlobalAplicada = 0;

      int stacksSangradoCrit = 0;
      int stacksArdiendoCrit = 0;
      int stacksCongeladoCrit = 0;
      int stacksAcidoCrit = 0;
      bool aplicarAturdidoCrit = false;
      bool aplicarResReducidasCrit = false;
      bool aplicarAPModCrit = false;
      bool intentarCondenadoCrit = false;

      if (esCritico) //Al ser crítico el daño se aplican los efectos segun tipo de daño
      {

        switch (tipoDanio)
        {
          case 1:
            usoArmaduraMitigacion = true;
            danioFinal = danio - armaduraEfectivaContraAtaque;
            colorDanio = Color.red;
            stacksSangradoCrit = (int)(danio / 4);
            ReducirArmaduraPorGolpe(danioFinal);
            break; //Cortante
          case 2:
            danioFinal = danio;
            colorDanio = Color.red;
            ReducirArmaduraPorGolpe(danioFinal);
            break; //Perforante
          case 3:
            usoArmaduraMitigacion = true;
            danioFinal = danio - armaduraEfectivaContraAtaque;
            colorDanio = Color.red;
            aplicarAPModCrit = true;
            ReducirArmaduraPorGolpe(danioFinal);
            break; //Contundente
          case 4:
            usoResistenciaMitigacion = true;
            resistenciaAplicada = ObtenerResistenciaA(1);
            danioFinal = danio - resistenciaAplicada;
            colorDanio = ColorDanioFuego;
            stacksArdiendoCrit = (int)(danioFinal / 3);
            break; //Fuego
          case 5:
            usoResistenciaMitigacion = true;
            resistenciaAplicada = ObtenerResistenciaA(2);
            danioFinal = danio - resistenciaAplicada;
            colorDanio = Color.cyan;
            stacksCongeladoCrit = (int)(danioFinal / 4);
            break; //Hielo
          case 6:
            usoResistenciaMitigacion = true;
            resistenciaAplicada = ObtenerResistenciaA(3);
            danioFinal = danio - resistenciaAplicada;
            colorDanio = Color.yellow;
            aplicarAturdidoCrit = true;
            break; //Rayo
          case 7:
            usoResistenciaMitigacion = true;
            resistenciaAplicada = ObtenerResistenciaA(4);
            danioFinal = danio - resistenciaAplicada;
            colorDanio = Color.green;
            stacksAcidoCrit = (int)(danioFinal / 5);
            break; //Acido
          case 8:
            usoResistenciaMitigacion = true;
            resistenciaAplicada = ObtenerResistenciaA(5);
            danioFinal = danio - resistenciaAplicada;
            colorDanio = Color.blue;
            aplicarResReducidasCrit = true;
            break; //Arcano
          case 9:
            usoResistenciaMitigacion = true;
            resistenciaAplicada = ObtenerResistenciaA(6);
            danioFinal = danio - resistenciaAplicada;
            colorDanio = new Color(0.8f, 1f, 0.6f);
            if (UnityEngine.Random.Range(0, 100) < (danioFinal / 2))
            {
              danioFinal = mod_maxHP + 1;
            }
            break; //Necro
          case 10: danioFinal = danio; colorDanio = Color.white; break; //Verdadero
          case 11:
            usoResistenciaMitigacion = true;
            resistenciaAplicada = ObtenerResistenciaA(7);
            danioFinal = danio - resistenciaAplicada;
            colorDanio = Color.yellow;
            intentarCondenadoCrit = true;
            break; //Divino
        }

      }
      else
      {
        switch (tipoDanio)
        {
          case 1: usoArmaduraMitigacion = true; danioFinal = danio - armaduraEfectivaContraAtaque; colorDanio = Color.red; ReducirArmaduraPorGolpe(danioFinal);  break; //Cortante
          case 2: usoArmaduraMitigacion = true; danioFinal = danio - armaduraEfectivaContraAtaque; colorDanio = Color.red; ReducirArmaduraPorGolpe(danioFinal);  break; //Perforante
          case 3: usoArmaduraMitigacion = true; danioFinal = danio - armaduraEfectivaContraAtaque; colorDanio = Color.red; ReducirArmaduraPorGolpe(danioFinal);  break; //Contundente
                                                                                                      //Los 3 primeros tipos de daño (físico) al ser golpeado y dañado, reduce en 1 la armadura.
          case 4: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(1); danioFinal = danio - resistenciaAplicada; colorDanio = ColorDanioFuego; break; //Fuego
          case 5: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(2); danioFinal = danio - resistenciaAplicada; colorDanio = Color.cyan; break;//Hielo
          case 6: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(3); danioFinal = danio - resistenciaAplicada; colorDanio = Color.yellow; break; //Rayo
          case 7: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(4); danioFinal = danio - resistenciaAplicada; colorDanio = Color.green; break; //Acido
          case 8: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(5); danioFinal = danio - resistenciaAplicada; colorDanio = Color.blue; break; //Arcano
          case 9: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(6); danioFinal = danio - resistenciaAplicada; colorDanio = new Color(0.8f, 1f, 0.6f); break; //Necro
          case 10: danioFinal = danio; colorDanio = Color.white; break; //Verdadero
          case 11: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(7); danioFinal = danio - resistenciaAplicada; colorDanio = Color.yellow; break; //Divino
        }


      }

      if (esEtereo && tipoDanio < 4)
      {
        danioFinal = danioFinal / 2;
        aplicoMitadEtereo = true;
      }

      float barreraAntes = barreraDeDanio;
      float danioBloqueado = Mathf.Min(danioFinal, barreraDeDanio); // Daño que la barrera absorbe
      if (danioBloqueado > 0)
      {
        barreraDeDanio -= danioBloqueado; // Reducimos la barrera
        danioFinal -= danioBloqueado; // Reducimos el daño que pasará a la unidad

        // Aseguramos que la barrera no quede en valores negativos
        if (barreraDeDanio < 0) barreraDeDanio = 0;

        if (danioBloqueado > 0)
        {
          BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(TRADU.i.Traducir("La Barrera de ") + uNombre + TRADU.i.Traducir(" absorbió ") + danioBloqueado + TRADU.i.Traducir(" de daño.")));

          int barreraMostrada = Mathf.RoundToInt(danioBloqueado);
          if (barreraMostrada > 0)
          {
            if (barreraAntes > 0 && barreraDeDanio <= 0)
            {
              GenerarTextoFlotante("(<s>" + TRADU.i.Traducir("Barrera") + "</s>)", Color.cyan, FloatingTextContext.Damage);
            }
            else
            {
              GenerarTextoFlotante("(-" + barreraMostrada + " " + TRADU.i.Traducir("Barrera") + ")", Color.cyan, FloatingTextContext.Damage);
            }
          }
        }
      }




      if (danioFinal < 0) { danioFinal = 0; }

      //ESTADO ESCUDADO: 10% de bloquear todo el daño por cada stack de escudo
      if (estado_Escudado > 0 && uCausante != null)
      {
        int chances = 10 * estado_Escudado;
        int random = UnityEngine.Random.Range(1, 101);
        if (random <= chances)
        {
          danioFinal = 0;
          scBattleManager.EscribirLog(CombatLogFormatter.EventoEstado(uNombre + TRADU.i.Traducir(" bloquea el daño con su escudo.")));
          GenerarTextoFlotante(TRADU.i.Traducir("Bloqueado"), Color.cyan, FloatingTextContext.Block);
          estado_Escudado--;
          textoDanioMostrado = true;
        }
      }

      if (!textoDanioMostrado && danioFinal <= 0 && danioBloqueado <= 0 && tipoDanio <= 3)
      {
        absorbioPorArmadura = true;
      }
      //----
      if (danioFinal > 0)
      {
        ReproducirAnimacionRecibirDanio();
        ChequearCorrompidoVsCorrupto(uCausante, danioFinal);
        BajarVuelo();
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

      int bonusTotal = 0;
      string bonusTexto = "";
      bool bonusColorAsignado = false;
      Color bonusColorPrimario = colorDanio;

      if (uCausante != null)
      {
        if (uCausante.bonusdam_acido > 0)
        {
          int bonus = CalcularDanioBonusElemental(uCausante.bonusdam_acido, 7, out Color col);
          if (bonus > 0)
          {
            bool bonusMismoTipo = tipoDanio == 7;
            bonusTotal += bonus;
            if (!bonusMismoTipo)
            {
              string bonusHex = ColorUtility.ToHtmlStringRGB(col);
              bonusTexto += "<size=80%><color=#" + bonusHex + ">(+" + bonus + ")</color></size>";
              if (!bonusColorAsignado) { bonusColorPrimario = col; bonusColorAsignado = true; }
            }
          }
        }
        if (uCausante.bonusdam_arcano > 0)
        {
          int bonus = CalcularDanioBonusElemental(uCausante.bonusdam_arcano, 8, out Color col);
          if (bonus > 0)
          {
            bool bonusMismoTipo = tipoDanio == 8;
            bonusTotal += bonus;
            if (!bonusMismoTipo)
            {
              string bonusHex = ColorUtility.ToHtmlStringRGB(col);
              bonusTexto += "<size=80%><color=#" + bonusHex + ">(+" + bonus + ")</color></size>";
              if (!bonusColorAsignado) { bonusColorPrimario = col; bonusColorAsignado = true; }
            }
          }
        }
        if (uCausante.bonusdam_fuego > 0)
        {
          int bonus = CalcularDanioBonusElemental(uCausante.bonusdam_fuego, 4, out Color col);
          if (bonus > 0)
          {
            bool bonusMismoTipo = tipoDanio == 4;
            bonusTotal += bonus;
            if (!bonusMismoTipo)
            {
              string bonusHex = ColorUtility.ToHtmlStringRGB(col);
              bonusTexto += "<size=80%><color=#" + bonusHex + ">(+" + bonus + ")</color></size>";
              if (!bonusColorAsignado) { bonusColorPrimario = col; bonusColorAsignado = true; }
            }
          }
        }
        if (uCausante.bonusdam_hielo > 0)
        {
          int bonus = CalcularDanioBonusElemental(uCausante.bonusdam_hielo, 5, out Color col);
          if (bonus > 0)
          {
            bool bonusMismoTipo = tipoDanio == 5;
            bonusTotal += bonus;
            if (!bonusMismoTipo)
            {
              string bonusHex = ColorUtility.ToHtmlStringRGB(col);
              bonusTexto += "<size=80%><color=#" + bonusHex + ">(+" + bonus + ")</color></size>";
              if (!bonusColorAsignado) { bonusColorPrimario = col; bonusColorAsignado = true; }
            }
          }
        }
        if (uCausante.bonusdam_necro > 0)
        {
          int bonus = CalcularDanioBonusElemental(uCausante.bonusdam_necro, 9, out Color col);
          if (bonus > 0)
          {
            bool bonusMismoTipo = tipoDanio == 9;
            bonusTotal += bonus;
            if (!bonusMismoTipo)
            {
              string bonusHex = ColorUtility.ToHtmlStringRGB(col);
              bonusTexto += "<size=80%><color=#" + bonusHex + ">(+" + bonus + ")</color></size>";
              if (!bonusColorAsignado) { bonusColorPrimario = col; bonusColorAsignado = true; }
            }
          }
        }
        if (uCausante.bonusdam_rayo > 0)
        {
          int bonus = CalcularDanioBonusElemental(uCausante.bonusdam_rayo, 6, out Color col);
          if (bonus > 0)
          {
            bool bonusMismoTipo = tipoDanio == 6;
            bonusTotal += bonus;
            if (!bonusMismoTipo)
            {
              string bonusHex = ColorUtility.ToHtmlStringRGB(col);
              bonusTexto += "<size=80%><color=#" + bonusHex + ">(+" + bonus + ")</color></size>";
              if (!bonusColorAsignado) { bonusColorPrimario = col; bonusColorAsignado = true; }
            }
          }
        }
        if (uCausante.bonusdam_divino > 0)
        {
          int bonus = CalcularDanioBonusElemental(uCausante.bonusdam_divino, 11, out Color col);
          if (bonus > 0)
          {
            bool bonusMismoTipo = tipoDanio == 11;
            bonusTotal += bonus;
            if (!bonusMismoTipo)
            {
              string bonusHex = ColorUtility.ToHtmlStringRGB(col);
              bonusTexto += "<size=80%><color=#" + bonusHex + ">(+" + bonus + ")</color></size>";
              if (!bonusColorAsignado) { bonusColorPrimario = col; bonusColorAsignado = true; }
            }
          }
        }
      }

      int danioTotal = (int)danioFinal + bonusTotal;
      Color colorDanioFinal = (danioFinal <= 0 && bonusTotal > 0 && bonusColorAsignado) ? bonusColorPrimario : colorDanio;
      IntentarAplicarDebuffsImpactoArma(uCausante, tipoDanio, ref danioTotal);
      AplicarMitigacionDefensivaAlDanioRecibido(tipoDanio, esCritico, ref danioTotal, out reduccionCritAplicada, out reduccionGlobalAplicada);

      if (danioTotal > 0 && scUnidadCanvas.unidadCanvas != null)
      {
        GameObject goDanioRecibido = Instantiate(scUnidadCanvas.PrefabtxtDaño, scUnidadCanvas.unidadCanvas.transform);
        scUnidadCanvas.txtDaño = goDanioRecibido.GetComponent<TextMeshProUGUI>();
        if (scUnidadCanvas.txtDaño != null)
        {
          scUnidadCanvas.txtDaño.color = colorDanioFinal;
          scUnidadCanvas.txtDaño.text = "";
        }
      }
   
      if (absorbioPorArmadura)
      {
        if (scTextoArmaduraFlash != null)
        {
          scTextoArmaduraFlash.Flash(new Color(1f, 0.9f, 0.2f, 1f));
        }
        ReproducirSonidoArmadura();
        if (danioTotal <= 0)
        {
          GenerarTextoFlotante(TRADU.i.Traducir("Armadura"), Color.yellow, FloatingTextContext.Resist);
        }
      }

      bool muereConDanio = HP_actual - danioTotal < 1;
      HP_actual -= danioTotal;
      AplicarEspinasAlAtacanteSiCorresponde(uCausante, tipoDanio, danioTotal);
      if (esCritico && !muereConDanio)
      {
        switch (tipoDanio)
        {
          case 1:
            Estados.Aplicar_Sangrado(this, stacksSangradoCrit, uCausante);
            break;
          case 3:
            if (aplicarAPModCrit && !IntentarResistenciaEstado("Reduccion AP", uCausante)) { estado_APModificador -= 1; }
            break;
          case 4:
            Estados.Aplicar_Ardiendo(this, stacksArdiendoCrit, uCausante);
            break;
          case 5:
            Estados.Aplicar_Congelado(this, stacksCongeladoCrit, uCausante);
            break;
          case 6:
            if (aplicarAturdidoCrit) { Estados.Aplicar_Aturdido(this, 1, uCausante); }
            break;
          case 7:
            Estados.Aplicar_Acido(this, stacksAcidoCrit, uCausante);
            break;
          case 8:
            if (aplicarResReducidasCrit && !IntentarResistenciaEstado("Resistencias reducidas", uCausante)) { estado_ResistenciasReducidas += 1; }
            break;
          case 11:
            if (intentarCondenadoCrit && danioFinal > 0 && TiradaSalvacion(mod_TSMental, 17) && !IntentarResistenciaEstado("Condenado", uCausante))
            {
              Buff buff = new Buff();
              buff.buffNombre = "Condenado";
              buff.boolfDebufftBuff = false;
              buff.DuracionBuffRondas = -1;
              buff.cantAtaque -= 1;
              buff.cantResDiv -= 5;
              buff.AplicarBuff(this);
              Buff buffComponent = ComponentCopier.CopyComponent(buff, gameObject);
            }
            break;
        }
      }
      string textoDanio = "-" + danioTotal;
      if (!(danioFinal <= 0 && bonusTotal > 0))
      {
        textoDanio += bonusTexto;
      }
      if (esCritico) { textoDanio += "!"; }

      if (danioTotal > 0)
      {
        await GenerarTextoFlotante(textoDanio, colorDanioFinal, esCritico ? FloatingTextContext.CriticalDamage : FloatingTextContext.Damage, scUnidadCanvas.txtDaño);
      }
      else if (scUnidadCanvas.txtDaño != null)
      {
        // No mostrar texto si el daño es 0
        scUnidadCanvas.txtDaño.text = "";
      }

      if (esCritico && danioTotal > 0)
      {
        string nombreUnidad = NombreUnidadParaLog(this);
        bool enIngles = IdiomaInglesActivo();
        string motivoCritRecibido = enIngles
          ? nombreUnidad + " receives a critical hit"
          : nombreUnidad + " recibe un golpe crítico";
        SumarValentia(-1, motivoCritRecibido);
      }
      
      await Task.Delay(100);
      if (danioTotal > 0)
      {
        string detalleCalculoDanio = ConstruirDetalleCalculoDanio(
          danio,
          esCritico,
          usoArmaduraMitigacion,
          armaduraEfectivaContraAtaque,
          penetracionTotalContraAtaque,
          usoResistenciaMitigacion,
          resistenciaAplicada,
          aplicoMitadEtereo,
          danioBloqueado,
          bonusTotal,
          reduccionCritAplicada,
          reduccionGlobalAplicada,
          danioTotal);
        EscribirLogDanioRecibido(nombreLog, danioTotal, tipoDanio, bonusTotal, false, detalleCalculoDanio);
      }


      
    


   
      ActualizarBarraVidaPropia();
      

      //Chequear si queda Herido (30% menos de vida, recibe herida)
      if (HP_actual < (mod_maxHP * 0.3))
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

      BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();

    }
    else
    {
      if (TieneBuffNombre("Invulnerable"))
      { GenerarTextoFlotante( TRADU.i.Traducir("Invulnerable"), Color.gray, FloatingTextContext.Resist); }

    }
  }

  void IntentarAplicarDebuffsImpactoArma(Unidad uCausante, int tipoDanio, ref int danioTotal)
  {
    if (uCausante == null || uCausante == this)
    {
      return;
    }

    // Se aplica por impactos de arma fisicos para evitar procs por dano residual.
    if (tipoDanio < 1 || tipoDanio > 3)
    {
      return;
    }

    if (uCausante.debuffsImpactoArma == null || uCausante.debuffsImpactoArma.Count == 0)
    {
      return;
    }

    for (int i = 0; i < uCausante.debuffsImpactoArma.Count; i++)
    {
      DebuffImpactoArmaData efecto = uCausante.debuffsImpactoArma[i];
      if (efecto == null || !efecto.activo || !efecto.TieneEfectos())
      {
        continue;
      }

      // Si el golpe quedo en 0, solo permitimos intentar efectos que puedan abrir dano
      // (hoy: ignorar armadura en ataques fisicos).
      if (danioTotal <= 0)
      {
        bool puedeAbrirDanio = efecto.ignorarArmaduraPlano > 0 && tipoDanio >= 1 && tipoDanio <= 3;
        if (!puedeAbrirDanio)
        {
          continue;
        }
      }

      int chance = Mathf.Clamp(efecto.probabilidadAplicar, 0, 100);
      if (chance <= 0)
      {
        continue;
      }

      int rollChance = UnityEngine.Random.Range(1, 101);
      if (rollChance > chance)
      {
        continue;
      }

      if (efecto.requiereTiradaSalvacion && efecto.tipoTiradaSalvacion > 0)
      {
        float atributoDefensaTS = ObtenerAtributoTSParaDebuff(efecto.tipoTiradaSalvacion);
        bool noSeSalva = TiradaSalvacion(atributoDefensaTS, efecto.dificultadSalvacion);
        if (!noSeSalva)
        {
          continue;
        }
      }

      AplicarEfectosDirectosDebuffImpactoArma(efecto, uCausante, tipoDanio, ref danioTotal);
      if (danioTotal <= 0)
      {
        continue;
      }

      if (efecto.TieneModificadores())
      {
        string nombreDebuff = string.IsNullOrWhiteSpace(efecto.nombreDebuff) ? "Debuff de impacto" : efecto.nombreDebuff;
        if (!IntentarResistenciaEstado(nombreDebuff, uCausante))
        {
          Buff buff = new Buff();
          buff.buffNombre = nombreDebuff;
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = Mathf.Max(1, efecto.duracionRondas);
          buff.cantAtFue += efecto.modFuerza;
          buff.cantAtAgi += efecto.modAgilidad;
          buff.cantAtPod += efecto.modPoder;
          buff.cantIniciativa += efecto.modIniciativa;
          buff.cantAtaque += efecto.modAtaque;
          buff.cantDefensa += efecto.modDefensa;
          buff.cantArmadura += efecto.modArmadura;
          buff.cantDanioPorcentaje += efecto.modDanioPorcentaje;
          buff.cantTsReflejos += efecto.modTSReflejos;
          buff.cantTsFortaleza += efecto.modTSFortaleza;
          buff.cantTsMental += efecto.modTSMental;
          buff.cantResFue += efecto.modResFuego;
          buff.cantResHie += efecto.modResHielo;
          buff.cantResRay += efecto.modResRayo;
          buff.cantResAci += efecto.modResAcido;
          buff.cantResArc += efecto.modResArcano;
          buff.cantResNec += efecto.modResNecro;
          buff.cantResDiv += efecto.modResDivino;
          buff.cantCritDado += efecto.modCritDado;
          buff.cantCritDaño += efecto.modCritDanioPorcentaje;
          buff.AplicarBuff(this, uCausante);
          Buff buffComponent = ComponentCopier.CopyComponent(buff, gameObject);
        }
      }

      AplicarEstadosDebuffImpactoArma(efecto, uCausante, danioTotal);
    }
  }

  void AplicarEfectosDirectosDebuffImpactoArma(
    DebuffImpactoArmaData efecto,
    Unidad uCausante,
    int tipoDanio,
    ref int danioTotal)
  {
    if (efecto == null)
    {
      return;
    }

    if (efecto.ignorarArmaduraPlano > 0 && tipoDanio >= 1 && tipoDanio <= 3)
    {
      // Respeta penetracion previa (equipo/habilidad) del atacante.
      int armaduraActual = Mathf.RoundToInt(ObtenerArmaduraEfectivaContraAtaque(uCausante));
      if (armaduraActual > 0)
      {
        int danioExtra = Mathf.Min(efecto.ignorarArmaduraPlano, armaduraActual);
        if (danioExtra > 0)
        {
          danioTotal += danioExtra;

          if (BattleManager.Instance != null)
          {
            bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
            string nombreAtacanteLog = uCausante != null
              ? (TRADU.i != null ? TRADU.i.Traducir(uCausante.uNombre) : uCausante.uNombre)
              : (TRADU.i != null ? TRADU.i.Traducir("Atacante") : "Atacante");
            string nombreObjetivoLog = TRADU.i != null ? TRADU.i.Traducir(uNombre) : uNombre;
            string mensaje = esIngles
              ? $"{nombreAtacanteLog} ignores {danioExtra} armor from {nombreObjetivoLog}."
              : $"{nombreAtacanteLog} ignora {danioExtra} de armadura de {nombreObjetivoLog}.";
            BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(mensaje));
          }
        }
      }
    }

    if (efecto.roboVidaPorcentaje > 0 && uCausante != null && danioTotal > 0)
    {
      float curacion = danioTotal * (efecto.roboVidaPorcentaje / 100f);
      if (curacion > 0)
      {
        uCausante.RecibirCuracion(curacion, false);
      }
    }
  }

  void AplicarEstadosDebuffImpactoArma(DebuffImpactoArmaData efecto, Unidad uCausante, int danioTotal)
  {
    if (efecto == null)
    {
      return;
    }

    if (efecto.stacksSangrado != 0)
    {
      Estados.Aplicar_Sangrado(this, efecto.stacksSangrado, uCausante);
    }

    if (efecto.stacksArdiendo != 0)
    {
      Estados.Aplicar_Ardiendo(this, efecto.stacksArdiendo, uCausante);
    }

    if (efecto.stacksCongelado != 0)
    {
      Estados.Aplicar_Congelado(this, efecto.stacksCongelado, uCausante);
    }

    if (efecto.stacksAcido != 0)
    {
      Estados.Aplicar_Acido(this, efecto.stacksAcido, uCausante);
    }

    if (efecto.stacksAturdido != 0)
    {
      Estados.Aplicar_Aturdido(this, efecto.stacksAturdido, uCausante);
    }

    if (efecto.reduccionAPPorTurno != 0)
    {
      if (!IntentarResistenciaEstado("Reduccion AP", uCausante))
      {
        estado_APModificador -= efecto.reduccionAPPorTurno;
      }
    }

    if (efecto.reduccionResistencias != 0)
    {
      if (!IntentarResistenciaEstado("Resistencias reducidas", uCausante))
      {
        estado_ResistenciasReducidas += efecto.reduccionResistencias;
      }
    }

    if (efecto.stacksCondenado != 0)
    {
      if (!IntentarResistenciaEstado("Condenado", uCausante))
      {
        for (int i = 0; i < efecto.stacksCondenado; i++)
        {
          Buff buff = new Buff();
          buff.buffNombre = "Condenado";
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = -1;
          buff.cantAtaque -= 1;
          buff.cantResDiv -= 5;
          buff.AplicarBuff(this);
          Buff buffComponent = ComponentCopier.CopyComponent(buff, gameObject);
        }
      }
    }

    if (danioTotal > 0)
    {
      if (efecto.empujeCasillas > 0)
      {
        EmpujarUnidad(efecto.empujeCasillas);
      }
      else if (efecto.jalonCasillas > 0)
      {
        JalarUnidad(efecto.jalonCasillas);
      }
    }
  }

  void AplicarMitigacionDefensivaAlDanioRecibido(
    int tipoDanio,
    bool esCritico,
    ref int danioTotal,
    out int reduccionCritAplicada,
    out int reduccionGlobalAplicada)
  {
    reduccionCritAplicada = 0;
    reduccionGlobalAplicada = 0;

    if (danioTotal <= 0)
    {
      return;
    }

    if (tipoDanio == 10)
    {
      return;
    }

    int reduccionCrit = Mathf.Clamp(reduccionDanioCriticoRecibidoPorcentaje, 0, 95);
    if (esCritico && reduccionCrit > 0)
    {
      int danioReducidoCrit = Mathf.RoundToInt(danioTotal * (reduccionCrit / 100f));
      if (danioReducidoCrit > 0)
      {
        danioTotal -= danioReducidoCrit;
        reduccionCritAplicada = danioReducidoCrit;
      }
    }

    if (danioTotal <= 0)
    {
      danioTotal = 0;
      return;
    }

    int reduccionGlobal = Mathf.Clamp(reduccionDanioRecibidoPorcentaje, 0, 95);
    if (reduccionGlobal > 0)
    {
      int danioReducido = Mathf.RoundToInt(danioTotal * (reduccionGlobal / 100f));
      if (danioReducido > 0)
      {
        danioTotal -= danioReducido;
        reduccionGlobalAplicada = danioReducido;
      }
    }

    if (danioTotal < 0)
    {
      danioTotal = 0;
    }
  }

  void AplicarEspinasAlAtacanteSiCorresponde(Unidad uCausante, int tipoDanio, int danioRecibidoFinal)
  {
    if (uCausante == null || uCausante == this || danioRecibidoFinal <= 0)
    {
      return;
    }

    if (tipoDanio < 1 || tipoDanio > 3)
    {
      return;
    }

    int danioEspinas = 0;
    if (espinasDanioPlano > 0)
    {
      danioEspinas += espinasDanioPlano;
    }

    int porcentajeEspinas = Mathf.Max(0, espinasDanioPorcentaje);
    if (porcentajeEspinas > 0)
    {
      danioEspinas += Mathf.RoundToInt(danioRecibidoFinal * (porcentajeEspinas / 100f));
    }

    if (danioEspinas <= 0)
    {
      return;
    }

    if (BattleManager.Instance != null)
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      string nombreDefensor = TRADU.i != null ? TRADU.i.Traducir(uNombre) : uNombre;
      string nombreAtacante = TRADU.i != null ? TRADU.i.Traducir(uCausante.uNombre) : uCausante.uNombre;
      string mensaje = esIngles
        ? $"{nombreDefensor} reflects {danioEspinas} damage to {nombreAtacante}."
        : $"{nombreDefensor} refleja {danioEspinas} de dano a {nombreAtacante}.";
      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(mensaje));
    }

    uCausante.RecibirDanio(danioEspinas, 10, false, this);
  }

  public bool IntentarResistenciaEstado(string nombreEstado, Unidad origen = null)
  {
    int chance = Mathf.Clamp(resistenciaEstadosPorcentaje, 0, 100);
    if (chance <= 0)
    {
      return false;
    }

    int roll = UnityEngine.Random.Range(1, 101);
    if (roll > chance)
    {
      return false;
    }

    Color colorResist = (CasillaPosicion != null && CasillaPosicion.lado == 1)
      ? new Color(0.75f, 0f, 0f)
      : new Color(0f, 0.75f, 0f);
    string textoResiste = TRADU.i != null ? TRADU.i.Traducir("Resiste") : "Resiste";
    GenerarTextoFlotante(textoResiste, colorResist, FloatingTextContext.Resist);

    if (BattleManager.Instance != null)
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      string nombreObjetivo = TRADU.i != null ? TRADU.i.Traducir(uNombre) : uNombre;
      string estado = string.IsNullOrWhiteSpace(nombreEstado) ? (TRADU.i != null ? TRADU.i.Traducir("estado") : "estado") : (TRADU.i != null ? TRADU.i.Traducir(nombreEstado) : nombreEstado);

      string mensaje;
      if (origen != null)
      {
        string nombreOrigen = TRADU.i != null ? TRADU.i.Traducir(origen.uNombre) : origen.uNombre;
        mensaje = esIngles
          ? $"{nombreObjetivo} resists {estado} from {nombreOrigen}."
          : $"{nombreObjetivo} resiste {estado} de {nombreOrigen}.";
      }
      else
      {
        mensaje = esIngles
          ? $"{nombreObjetivo} resists {estado}."
          : $"{nombreObjetivo} resiste {estado}.";
      }

      BattleManager.Instance.EscribirLog(CombatLogFormatter.EventoEstado(mensaje));
    }

    return true;
  }

  float ObtenerAtributoTSParaDebuff(int tipoTiradaSalvacion)
  {
    switch (tipoTiradaSalvacion)
    {
      case 2: return mod_TSReflejos;
      case 3: return mod_TSMental;
      case 1:
      default:
        return mod_TSFortaleza;
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
    string nombreLog = TRADU.i != null ? TRADU.i.Traducir(uNombre) : uNombre;

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


      Color colorDanioElemental = Color.white;
      float armaduraBaseContraAtaque = ObtenerArmaduraActual();
      int penetracionTotalContraAtaque = 0;
      if (uCausante != null)
      {
        int penetracionEquipo = Mathf.Max(0, uCausante.penetracionArmaduraPlano);
        int penetracionHabilidad = Mathf.Max(0, uCausante.penetracionArmaduraHabilidadActual);
        penetracionTotalContraAtaque = penetracionEquipo + penetracionHabilidad;
      }
      float armaduraEfectivaContraAtaque = armaduraBaseContraAtaque - penetracionTotalContraAtaque;
      if (armaduraEfectivaContraAtaque < 0f)
      {
        armaduraEfectivaContraAtaque = 0f;
      }
      bool usoArmaduraMitigacion = false;
      bool usoResistenciaMitigacion = false;
      float resistenciaAplicada = 0f;
      bool aplicoMitadEtereo = false;
      int reduccionCritAplicada = 0;
      int reduccionGlobalAplicada = 0;




      switch (tipoDanio)
      {
        case 1: usoArmaduraMitigacion = true; danioFinal = danio - armaduraEfectivaContraAtaque; colorDanioElemental = Color.red; break; //Cortante
        case 2: usoArmaduraMitigacion = true; danioFinal = danio - armaduraEfectivaContraAtaque; colorDanioElemental = Color.red; break; //Perforante
        case 3: usoArmaduraMitigacion = true; danioFinal = danio - armaduraEfectivaContraAtaque; colorDanioElemental = Color.red; break; //Contundente
        //Los 3 primeros tipos de daño (físico) al ser golpeado y dañado, reduce en 1 la armadura.
        case 4: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(1); danioFinal = danio - resistenciaAplicada; colorDanioElemental = ColorDanioFuego; break; //Fuego
        case 5: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(2); danioFinal = danio - resistenciaAplicada; colorDanioElemental = Color.cyan; break;//Hielo
        case 6: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(3); danioFinal = danio - resistenciaAplicada; colorDanioElemental = Color.yellow; break; //Rayo
        case 7: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(4); danioFinal = danio - resistenciaAplicada; colorDanioElemental = Color.green; break; //Acido
        case 8: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(5); danioFinal = danio - resistenciaAplicada; colorDanioElemental = Color.blue; break; //Arcano
        case 9: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(6); danioFinal = danio - resistenciaAplicada; colorDanioElemental =  new Color(0.8f, 1f, 0.6f); break; //Necro
        case 10: danioFinal = danio; colorDanioElemental = Color.white; break; //Verdadero
        case 11: usoResistenciaMitigacion = true; resistenciaAplicada = ObtenerResistenciaA(7); danioFinal = danio - resistenciaAplicada; colorDanioElemental = Color.yellow; break; //Divino
      }




      if (esEtereo && tipoDanio < 4)
      {
        danioFinal = danioFinal / 2;
        aplicoMitadEtereo = true;
      }








      if (danioFinal < 0) { danioFinal = 0; }
      int danioFinalInt = Mathf.RoundToInt(danioFinal);
      AplicarMitigacionDefensivaAlDanioRecibido(tipoDanio, false, ref danioFinalInt, out reduccionCritAplicada, out reduccionGlobalAplicada);
      danioFinal = danioFinalInt;
      if (danioFinalInt > 0)
      {
       estado_evasion = 0;
      }

      if (danioFinalInt > 0 && scUnidadCanvas.unidadCanvas != null)
      {
        GameObject goDanioRecibido = Instantiate(scUnidadCanvas.PrefabtxtDaño, scUnidadCanvas.unidadCanvas.transform);
        scUnidadCanvas.txtDaño = goDanioRecibido.GetComponent<TextMeshProUGUI>();
        if (scUnidadCanvas.txtDaño != null)
        {
          scUnidadCanvas.txtDaño.color = colorDanioElemental;
          scUnidadCanvas.txtDaño.text = "";
        }
      }
      await Task.Delay(150);
      HP_actual -= danioFinalInt;
      string textoDanioElemental = "-" + danioFinalInt;
      Color colorDanioElementalFinal = colorDanioElemental;

      if (danioFinalInt > 0)
      {
        await GenerarTextoFlotante(textoDanioElemental, colorDanioElementalFinal, FloatingTextContext.Damage, scUnidadCanvas.txtDaño);
      }
      else if (scUnidadCanvas.txtDaño != null)
      {
        // No mostrar texto si el daño es 0
        scUnidadCanvas.txtDaño.text = "";
      }
      if (danioFinalInt > 0)
      {
        string detalleCalculoDanio = ConstruirDetalleCalculoDanio(
          danio,
          false,
          usoArmaduraMitigacion,
          armaduraEfectivaContraAtaque,
          penetracionTotalContraAtaque,
          usoResistenciaMitigacion,
          resistenciaAplicada,
          aplicoMitadEtereo,
          0f,
          0,
          reduccionCritAplicada,
          reduccionGlobalAplicada,
          danioFinalInt);
        EscribirLogDanioRecibido(nombreLog, danioFinalInt, tipoDanio, 0, true, detalleCalculoDanio);
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
      { GenerarTextoFlotante( TRADU.i.Traducir("Invulnerable"), Color.gray, FloatingTextContext.Resist); }


    }






  }
  public virtual void ReducirArmaduraPorGolpe(float danioFinal)
  {
   if (scTextoArmaduraFlash == null)
    {
      return;
    }

    if (danioFinal > 0) { estado_armaduraModificador++; scTextoArmaduraFlash.Flash(); }
     
}
 public virtual void  RecibirHerida()
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
       Herida.cantAtFue -= 1;
       Herida.cantAtAgi -= 1;
       Herida.cantAtPod -= 1;
       Herida.AplicarBuff(this);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(Herida, gameObject);
       //--------------------------------------

    }

}
public virtual void AcabaDeMatarUnidad(Unidad uVictima)
{
  if (uVictima == null)
  {
    SumarValentia(2);
    return;
  }

  bool sonEnemigos = CasillaPosicion != null && uVictima.CasillaPosicion != null && CasillaPosicion.lado != uVictima.CasillaPosicion.lado;
  if (!sonEnemigos)
  {
    return;
  }

  string nombreAsesino = NombreUnidadParaLog(this);
  string nombreVictima = NombreUnidadParaLog(uVictima);
  bool enIngles = IdiomaInglesActivo();

  string motivoAsesino = enIngles
    ? nombreAsesino + " defeats " + nombreVictima
    : nombreAsesino + " derrota a " + nombreVictima;
  SumarValentia(2, motivoAsesino);

  LadoManager ladoAliado = null;
  if (BattleManager.Instance != null && CasillaPosicion != null)
  {
    ladoAliado = CasillaPosicion.lado == 1 ? BattleManager.Instance.ladoA : BattleManager.Instance.ladoB;
  }

  if (ladoAliado == null && CasillaPosicion != null && CasillaPosicion.ladoGO != null)
  {
    ladoAliado = CasillaPosicion.ladoGO.GetComponent<LadoManager>();
  }

  if (ladoAliado == null)
  {
    return;
  }

  ladoAliado.ActualizarListaDeUnidadesEnLado();
  List<Unidad> aliadosInspirados = new List<Unidad>(ladoAliado.unidadesLado);
  foreach (Unidad aliado in aliadosInspirados)
  {
    if (aliado == null || aliado == this || aliado.HP_actual <= 0 || !aliado.gameObject.activeInHierarchy)
    {
      continue;
    }

    string nombreAliado = NombreUnidadParaLog(aliado);
    string motivoAliado = enIngles
      ? nombreAliado + " is inspired by " + nombreAsesino + "'s kill"
      : nombreAliado + " se inspira con la baja de " + nombreVictima;
    aliado.SumarValentia(1, motivoAliado);
  }
}
  public virtual void AcabaDeHacerDañoA(Unidad uVictima)
  {
     RemoverBuffNombre("Escondido Por Humo");
 
  }

public virtual void FalloAtaqueRecibido(Unidad uOrigen, bool melee)
{
 ReproducirAnimacionMiss();
 LlamarReacciones(1, uOrigen, melee);
}
private bool IdiomaInglesActivo()
{
  return TRADU.i != null && TRADU.i.nIdioma == 2;
}

private string NombreUnidadParaLog(Unidad unidad)
{
  if (unidad == null)
  {
    return string.Empty;
  }

  return TRADU.i != null ? TRADU.i.Traducir(unidad.uNombre) : unidad.uNombre;
}

public virtual void SumarValentia(int cant, string motivo = null)
{
  if (GetComponent<IAUnidad>() != null || cant == 0)
  {
    return;
  }

  float valentiaInicial = ValentiaP_actual;
  float valentiaFinal = valentiaInicial + cant;
  if (valentiaFinal > mod_maxValentiaP)
  {
    valentiaFinal = mod_maxValentiaP;
  }

  int cambioReal = Mathf.RoundToInt(valentiaFinal - valentiaInicial);
  if (cambioReal == 0)
  {
    return;
  }

  ChequearBuffsDeValentia(valentiaInicial, cambioReal);
  ValentiaP_actual = valentiaFinal;

  bool esGanancia = cambioReal > 0;
  int cambioAbs = Mathf.Abs(cambioReal);
  string etiquetaValentia = " VAL";
  string textoCambioValentia = (esGanancia ? "+" : "-") + cambioAbs + etiquetaValentia;
  Color colorTextoValentia = esGanancia ? ColorValentiaGanada : ColorValentiaPerdida;
  FloatingTextContext contextoTextoValentia = esGanancia ? FloatingTextContext.ValourGain : FloatingTextContext.ValourLoss;

  _ = GenerarTextoFlotante("<b>" + textoCambioValentia + "</b>", colorTextoValentia, contextoTextoValentia);

  if (scBattleManager != null)
  {
    if (!string.IsNullOrWhiteSpace(motivo))
    {
      string motivoLimpio = motivo.Trim();
      if (motivoLimpio.EndsWith("."))
      {
        motivoLimpio = motivoLimpio.Substring(0, motivoLimpio.Length - 1);
      }

      scBattleManager.EscribirLog(
        CombatLogFormatter.EventoValour(
          motivoLimpio + " (" + (esGanancia ? "+" : "-") + cambioAbs + " VAL)."));
    }
    else
    {
      string nombreUnidad = NombreUnidadParaLog(this);
      string verbo = TRADU.i != null
        ? TRADU.i.Traducir(esGanancia ? " gana " : " pierde ")
        : (esGanancia ? " gana " : " pierde ");
      string sufijoValentia = TRADU.i != null ? TRADU.i.Traducir(" de Valentía.") : " de Valentía.";
      scBattleManager.EscribirLog(
        CombatLogFormatter.EventoValour(nombreUnidad + verbo + cambioAbs + sufijoValentia));
    }
  }

  if (BattleManager.Instance != null)
  {
    UIInfoChar infoChar = BattleManager.Instance.scUIInfoChar;
    if (infoChar != null)
    {
      bool mostrandoEstaUnidad = infoChar.unidadMostrada == this;
      bool mostrandoActivoAutomatico = !infoChar.hayUnidadSeleccionadaParaInfo && BattleManager.Instance.unidadActiva == this;
      if (mostrandoEstaUnidad || mostrandoActivoAutomatico)
      {
        infoChar.ActualizarInfoChar(this);
      }
    }

    BattleManager.Instance.NotificarCambioValourGlobal();
  }
}

public virtual void AjustarValentiaInicialSinLog(int cant)
{
  if (GetComponent<IAUnidad>() != null || cant == 0)
  {
    return;
  }

  float valentiaInicial = ValentiaP_actual;
  float valentiaFinal = valentiaInicial + cant;
  if (valentiaFinal > mod_maxValentiaP)
  {
    valentiaFinal = mod_maxValentiaP;
  }

  int cambioReal = Mathf.RoundToInt(valentiaFinal - valentiaInicial);
  if (cambioReal == 0)
  {
    return;
  }

  ChequearBuffsDeValentia(valentiaInicial, cambioReal);
  ValentiaP_actual = valentiaFinal;

  if (BattleManager.Instance != null)
  {
    BattleManager.Instance.NotificarCambioValourGlobal();
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
       motivado.suprimeTextoFlotante = true;
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
       motivado.suprimeTextoFlotante = true;
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
       motivado.suprimeTextoFlotante = true;
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
       motivado.suprimeTextoFlotante = true;
       motivado.boolfDebufftBuff = false;
       motivado.DuracionBuffRondas = -1;
       motivado.cantAtFue -= 1;
       motivado.cantAtPod -= 1;
       motivado.cantAtAgi -= 1;
       motivado.AplicarBuff(this);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(motivado, gameObject);
}

 
  private int ReservarSlotTextoFlotante(float lifetime)
  {
    float now = Time.time;
    for (int i = 0; i < floatingTextSlotExpiries.Count; i++)
    {
      if (floatingTextSlotExpiries[i] <= now)
      {
        floatingTextSlotExpiries[i] = now + lifetime;
        return i;
      }
    }

    int maxSlots = Mathf.Max(1, floatingTextMaxSlots);
    if (floatingTextSlotExpiries.Count < maxSlots)
    {
      floatingTextSlotExpiries.Add(now + lifetime);
      return floatingTextSlotExpiries.Count - 1;
    }

    int reuseIndex = 0;
    float earliestExpiry = floatingTextSlotExpiries[0];
    for (int i = 1; i < floatingTextSlotExpiries.Count; i++)
    {
      if (floatingTextSlotExpiries[i] < earliestExpiry)
      {
        earliestExpiry = floatingTextSlotExpiries[i];
        reuseIndex = i;
      }
    }

    floatingTextSlotExpiries[reuseIndex] = now + lifetime;
    return reuseIndex;
  }

  private float ObtenerDuracionTextoFlotante(FloatingTextAnimator animator, FloatingTextContext contexto)
  {
    if (animator != null)
    {
      float lifetime = animator.GetLifetime(contexto);
      if (lifetime > 0f)
      {
        return lifetime;
      }
    }

    return floatingTextSlotLifetimeFallback;
  }

  private float CalcularIntervaloTextoFlotante(float now)
  {
    float baseInterval = Mathf.Max(0f, floatingTextMinInterval);
    if (baseInterval <= 0f)
    {
      return 0f;
    }

    float tiempoPendiente = Mathf.Max(0f, nextFloatingTextTime - now);
    int textosPendientes = Mathf.CeilToInt(tiempoPendiente / baseInterval);
    if (textosPendientes < Mathf.Max(1, floatingTextCrowdThreshold))
    {
      return baseInterval;
    }

    float extra = (textosPendientes - floatingTextCrowdThreshold + 1) * Mathf.Max(0f, floatingTextCrowdIntervalStep);
    extra = Mathf.Clamp(extra, 0f, Mathf.Max(0f, floatingTextCrowdExtraIntervalMax));
    return baseInterval + extra;
  }

  public async Task GenerarTextoFlotante(string txString, Color color, FloatingTextContext contexto = FloatingTextContext.Generic, TextMeshProUGUI overrideText = null)
  {

    // Pequena espera para escalonar textos y evitar solapamientos
    float delaySeconds = 0f;
    if (floatingTextMinInterval > 0f)
    {
      float now = Time.time;
      float intervaloActual = CalcularIntervaloTextoFlotante(now);
      if (now < nextFloatingTextTime)
      {
        delaySeconds = nextFloatingTextTime - now;
      }
      nextFloatingTextTime = Mathf.Max(nextFloatingTextTime, now) + intervaloActual;
    }

    if (delaySeconds > 0f)
    {
      int delayMs = Mathf.CeilToInt(delaySeconds * 1000f);
      await Task.Delay(delayMs);
    }

    // Instancia el nuevo objeto
    TextMeshProUGUI txtMesh = overrideText;
    GameObject goTextoFlotante = null;

    if (txtMesh == null || txtMesh.gameObject == null)
    {
      if (scUnidadCanvas != null && scUnidadCanvas.PrefabtxtDaño != null && scUnidadCanvas.unidadCanvas != null)
      {
        goTextoFlotante = Instantiate(scUnidadCanvas.PrefabtxtDaño, scUnidadCanvas.unidadCanvas.transform, false);
        txtMesh = goTextoFlotante.GetComponent<TextMeshProUGUI>();
      }
    }
    else
    {
      goTextoFlotante = txtMesh.gameObject;
      if (!goTextoFlotante.activeSelf)
      {
        goTextoFlotante.SetActive(true);
      }
    }

    if (txtMesh != null)
    {
      txtMesh.richText = true;
      txtMesh.text = txString;
      txtMesh.color = color;
    }

    FloatingTextAnimator animator = goTextoFlotante != null ? goTextoFlotante.GetComponent<FloatingTextAnimator>() : null;
    RectTransform rectTransform = goTextoFlotante != null ? goTextoFlotante.GetComponent<RectTransform>() : null;
    if (rectTransform != null)
    {
      Vector2 basePosition = rectTransform.anchoredPosition;
      if (animator != null)
      {
        basePosition = animator.GetInitialBasePosition();
      }

      if (apilarTextosFlotantes)
      {
        float lifetime = ObtenerDuracionTextoFlotante(animator, contexto);
        int slotIndex = ReservarSlotTextoFlotante(lifetime);
        basePosition += Vector2.up * (floatingTextSlotSpacing * slotIndex);
      }

      if (animator != null)
      {
        animator.SetBasePosition(basePosition);
      }
      else
      {
        rectTransform.anchoredPosition = basePosition;
      }
    }

    if (animator != null)
    {
      animator.Play(txString, color, contexto);
    }
    else if (TextoFlotanteManager.Instance != null && overrideText == null)
    {
      TextoFlotanteManager.Instance.GenerarTextoFlotante(txString, color, contexto);
    }
  }

  public void MostrarRotuloHabilidadIA(string txString, Color color, float duracion = 4f)
  {
    if (string.IsNullOrWhiteSpace(txString))
    {
      return;
    }

    string textoFormateado = txString.Trim().ToUpperInvariant();

    if (scUnidadCanvas != null)
    {
      scUnidadCanvas.MostrarRotuloHabilidadIA(textoFormateado, color, duracion);
      return;
    }

    _ = GenerarTextoFlotante("<b>" + textoFormateado + "</b>", color, FloatingTextContext.Generic);
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
  GenerarTextoFlotante(TRADU.i.Traducir("Cura ")+(int)curaFinal, Color.green, FloatingTextContext.Heal);
  scBattleManager.EscribirLog(CombatLogFormatter.EventoCuracion(uNombre+TRADU.i.Traducir(" recibe <color=#11c66b>") +curaFinal+TRADU.i.Traducir("</color> de curación.")));

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
private bool yaSeRetiro = false;
public void UnidadMuere()
{  
  if(!yaMurio)
  {
   yaMurio = true;
   GenerarTextoFlotante(TRADU.i.Traducir("Muerto"), new Color(0.35f, 0.35f, 0.35f), FloatingTextContext.Resist);

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
   
   if (CasillaPosicion != null && CasillaPosicion.lado == 2) // Penaliza valentía de aliados del jugador al caer un aliado.
   {
     bool caidoEraIAAliada = GetComponent<IAUnidad>() != null;
     int perdidaValentia = caidoEraIAAliada ? -1 : -3;
     bool enIngles = IdiomaInglesActivo();
     string nombreCaido = NombreUnidadParaLog(this);

     foreach (Unidad aliado in BattleManager.Instance.ladoB.GetComponent<LadoManager>().unidadesLado)
     {
       if (aliado == null || aliado.gameObject == gameObject || aliado.HP_actual <= 0)
       {
         continue;
       }

       string nombreAliado = NombreUnidadParaLog(aliado);
       string motivo = enIngles
         ? nombreAliado + " loses morale after " + nombreCaido + " falls"
         : nombreAliado + " pierde ánimo al caer " + nombreCaido;
       aliado.SumarValentia(perdidaValentia, motivo);
     }
   }
   
   CasillaPosicion.Presente = null;
   Invoke("DesactivarGOconDelay", 1.4f); 


    if(CasillaPosicion.lado == 1)
    {
          scBattleManager.EscribirLog(CombatLogFormatter.EventoMuerte($"<color=#d92b08>"+uNombre+TRADU.i.Traducir(" muere.")+"</color>"));

    }
    else
    {
          scBattleManager.EscribirLog(CombatLogFormatter.EventoMuerte($""+uNombre+TRADU.i.Traducir(" muere.")+""));
    }


    ChequearEsteror();

  }
}

void ChequearEsteror()
{






}

public bool RetirarseDeBatallaPorMoral()
{
  if (yaMurio || yaSeRetiro)
  {
    return false;
  }

  yaSeRetiro = true;

  if (scBattleManager != null)
  {
    scBattleManager.OnRondaNueva -= BattleManager_OnRondaNueva;
    scBattleManager.lUnidadesTotal.Remove(this);
  }

  if (CasillaPosicion != null && CasillaPosicion.ladoGO != null)
  {
    LadoManager lado = CasillaPosicion.ladoGO.GetComponent<LadoManager>();
    if (lado != null)
    {
      lado.unidadesLado.Remove(this);
    }

    CasillaPosicion.Presente = null;
  }

  AccionP_actual = 0;
  gameObject.SetActive(false);

  if (BattleManager.Instance != null)
  {
    if (BattleManager.Instance.scUIBarraOrdenTurno != null)
    {
      BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
    }
    BattleManager.Instance.ChequearFinBatalla();
  }

  return true;
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
  SincronizarEscaladosPorAtributos();
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

public float ObtenerArmaduraEfectivaContraAtaque(Unidad uCausante)
{
    float armaduraBase = ObtenerArmaduraActual();
    if (uCausante == null)
    {
      return armaduraBase;
    }

    int penetracionEquipo = Mathf.Max(0, uCausante.penetracionArmaduraPlano);
    int penetracionHabilidad = Mathf.Max(0, uCausante.penetracionArmaduraHabilidadActual);
    int penetracionTotal = penetracionEquipo + penetracionHabilidad;
    if (penetracionTotal <= 0)
    {
      return armaduraBase;
    }

    float armaduraEfectiva = armaduraBase - penetracionTotal;
    if (armaduraEfectiva < 0)
    {
      armaduraEfectiva = 0;
    }

    return armaduraEfectiva;
}

public float ObtenerResistenciaA(int tipo)
{
   SincronizarEscaladosPorAtributos();
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

  public void MostrarProbabilidad(float? probabilidad, string textoPersonalizado = null)
  {
    if (scUnidadCanvas == null)
    {
      return;
    }

    if (!probabilidad.HasValue)
    {
      OcultarProbabilidad();
      return;
    }

    if (scUnidadCanvas.txtProbabilidad == null)
    {
      scUnidadCanvas.CrearTextoProbabilidad();
    }

    if (scUnidadCanvas.txtProbabilidad != null)
    {
      float valor = Mathf.Clamp01(probabilidad.Value);
      string texto = string.IsNullOrEmpty(textoPersonalizado)
        ? Mathf.RoundToInt(valor * 100f) + TRADU.i.Traducir(" % Chances")
        : textoPersonalizado;
      if (scUnidadCanvas.txtProbabilidad.text != texto)
      {
        scUnidadCanvas.txtProbabilidad.text = texto;
      }
      // Color: rojo (<50%), amarillo (~50%) a verde (>=75%)
      Color col;
      if (valor < 0.6f)
      {
        col = Color.Lerp(new Color(0.8f, 0f, 0f), new Color(1f, 0.9f, 0f), valor / 0.6f);
      }
      else
      {
        col = Color.Lerp(new Color(1f, 0.9f, 0f), new Color(0.1f, 0.8f, 0.2f), (valor - 0.6f) / 0.4f);
      }
      if (scUnidadCanvas.txtProbabilidad.color != col)
      {
        scUnidadCanvas.txtProbabilidad.color = col;
      }
      if (!scUnidadCanvas.txtProbabilidad.gameObject.activeSelf)
      {
        scUnidadCanvas.txtProbabilidad.gameObject.SetActive(true);
      }
    }
  }

  public void OcultarProbabilidad()
  {
    if (scUnidadCanvas != null && scUnidadCanvas.txtProbabilidad != null)
    {
      scUnidadCanvas.txtProbabilidad.gameObject.SetActive(false);
    }
  }
Unidad anterior = null;
public void OnMouseEnter() 
{
    if (EstaOcultoVisualmenteParaJugador())
    {
      return;
    }
 
    anterior = null;
    Marcar(1);

    if(BattleManager.Instance.scUIInfoChar.unidadMostrada != BattleManager.Instance.unidadActiva)
    {anterior = BattleManager.Instance.scUIInfoChar.unidadMostrada; }
    
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);

     if(scBattleManager.SeleccionandoObjetivo)
     {
        CasillaPosicion.OnMouseOver();

     }

}
public void OnMouseExit() 
{
    if (EstaOcultoVisualmenteParaJugador())
    {
      return;
    }

   
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
    if (EstaOcultoVisualmenteParaJugador())
    {
      return;
    }

    if (scBattleManager.lUnidadesPosiblesHabilidadActiva.Contains(this) && scBattleManager.SeleccionandoObjetivo)
    {
      if (scBattleManager.HabilidadActiva.esMelee && estado_Volando)
      {
        //Si se quiere hacer una habilidad melee a una unidad voladora, no hace nada.
        GenerarTextoFlotante(TRADU.i.Traducir("Inalcanzable: unidad volando"), Color.gray, FloatingTextContext.Resist);
      }
      else if (scBattleManager.HabilidadActiva.esHostil && ObtenerEstaEscondido() > 0)
      {
        //Si se quiere hacer una habilidad hostil a una unidad escondida, no hace nada.
        GenerarTextoFlotante(TRADU.i.Traducir("Inalcanzable: unidad escondida"), Color.gray, FloatingTextContext.Resist);
      }
      else
      {


        string sss = "Se resuelve la habilidad " + scBattleManager.HabilidadActiva.nombre + " hecha por " + scBattleManager.HabilidadActiva.gameObject + " a " + this;




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
        else if (!BattleManager.Instance.bOcupado)
        {
          List<object> listaUno = new List<object> { this };
          await scBattleManager.HabilidadActiva.Resolver(listaUno);
          BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(anterior);
        }
      }
    }
    else
    {
      if (anterior == this)
      {
        BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo = false;
        BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(anterior);
        Marcar(0); anterior = null;
      }
      else if (BattleManager.Instance.scUIInfoChar.unidadMostrada != this || BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo == false || anterior != null)
      {
        BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);
        BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo = true;
        Marcar(1); anterior = this;
      }
      else
      {
        BattleManager.Instance.scUIInfoChar.hayUnidadSeleccionadaParaInfo = false;
        Marcar(0); anterior = null;
      }

      if (CasillaPosicion.lado == 2 && scBattleManager.unidadActiva!= this && !scBattleManager.SeleccionandoObjetivo)
      { 
        CasillaPosicion.OnMouseDown();

      }

  }

 
}

  public virtual bool TiradaSalvacion(float atributoDefiende, float dificultadHabilidada, bool porValourGlobal = false) //TRUE no se salva FALSE se salva (xd)
  {
    float iTiradaDefensa = UnityEngine.Random.Range(1,21);
    float iResultadoAtaque =  dificultadHabilidada;
    float iResultadoDefensa = iTiradaDefensa + atributoDefiende;

    bool noSeSalva = iResultadoAtaque > iResultadoDefensa;

    string tipoTS = InferirTipoSalvacion(atributoDefiende);
    string textoResultado = noSeSalva ? TRADU.i.Traducir("No se salva") : TRADU.i.Traducir("Se salva");
    CombatLogFormatter.CombatOutcome outcome = noSeSalva ? CombatLogFormatter.CombatOutcome.Fallo : CombatLogFormatter.CombatOutcome.Exito;

    BattleManager.Instance.EscribirLog(
      CombatLogFormatter.FormatearSalvacion(
        uNombre,
        tipoTS,
        (int)iTiradaDefensa,
        atributoDefiende,
        iResultadoAtaque,
        textoResultado,
        outcome,
        !porValourGlobal));

    if (!noSeSalva) //NegativoSeSalva
    {
      Color colorResist = (CasillaPosicion != null && CasillaPosicion.lado == 1) ? new Color(0.75f, 0f, 0f) : new Color(0f, 0.75f, 0f);
      GenerarTextoFlotante(TRADU.i.Traducir("Resiste"), colorResist, FloatingTextContext.Resist);
    }

    return noSeSalva;
  }

  private string InferirTipoSalvacion(float atributoDefiende)
  {
    if (Mathf.Approximately(atributoDefiende, mod_TSFortaleza)) { return TRADU.i.Traducir("Fortaleza"); }
    if (Mathf.Approximately(atributoDefiende, mod_TSReflejos)) { return TRADU.i.Traducir("Reflejos"); }
    if (Mathf.Approximately(atributoDefiende, mod_TSMental)) { return TRADU.i.Traducir("Mental"); }
    return TRADU.i.Traducir("TS");
  }

  
  public void ForzarMoverAPrimeraFila()
  {
    Casilla casillaOrigen = movimientoForzadoPendiente && CasillaForzadoaMover != null
      ? CasillaForzadoaMover
      : CasillaPosicion;

    if (casillaOrigen == null || casillaOrigen.ladoGO == null)
    {
      return;
    }

    if (casillaOrigen.posX >= 3)
    {
      return;
    }

    LadoManager lado = casillaOrigen.ladoGO.GetComponent<LadoManager>();
    if (lado == null)
    {
      return;
    }

    Casilla destino = null;
    for (int targetX = 3; targetX > casillaOrigen.posX; targetX--)
    {
      Casilla candidata = lado.ObtenerCasillaPorIndex(targetX, casillaOrigen.posY);
      if (candidata != null && candidata.Presente == null)
      {
        destino = candidata;
        break;
      }
    }

    if (destino == null || destino == CasillaPosicion || destino == CasillaForzadoaMover)
    {
      return;
    }

    CasillaForzadoaMover = destino;
    CasillaDeseadaMov = null;
    movimientoForzadoPendiente = true;
  }

  public void EmpujarUnidad(int cantidad)
  {
    if (cantidad <= 0)
    {
      return;
    }

    Casilla casillaOrigen = movimientoForzadoPendiente && CasillaForzadoaMover != null
      ? CasillaForzadoaMover
      : CasillaPosicion;

    if (casillaOrigen == null || casillaOrigen.ladoGO == null)
    {
      return;
    }

    LadoManager lado = casillaOrigen.ladoGO.GetComponent<LadoManager>();
    if (lado == null)
    {
      return;
    }

    int posXBase = casillaOrigen.posX;
    int posY = casillaOrigen.posY;
    Casilla destino = null;

    for (int paso = 1; paso <= cantidad; paso++)
    {
      Casilla casillaAtras = lado.ObtenerCasillaPorIndex(posXBase - paso, posY);
      if (casillaAtras == null)
      {
        break;
      }

      if (casillaAtras.Presente != null && casillaAtras.Presente != gameObject)
      {
        destino = null;
        break;
      }

      destino = casillaAtras;
    }

    if (destino == null || destino == CasillaPosicion || destino == CasillaForzadoaMover)
    {
      return;
    }

    CasillaForzadoaMover = destino;
    CasillaDeseadaMov = null;
    movimientoForzadoPendiente = true;
  }

  public void JalarUnidad(int cantidad)
  {
    if (cantidad <= 0)
    {
      return;
    }

    Casilla casillaOrigen = movimientoForzadoPendiente && CasillaForzadoaMover != null
      ? CasillaForzadoaMover
      : CasillaPosicion;

    if (casillaOrigen == null || casillaOrigen.ladoGO == null)
    {
      return;
    }

    LadoManager lado = casillaOrigen.ladoGO.GetComponent<LadoManager>();
    if (lado == null)
    {
      return;
    }

    int posXBase = casillaOrigen.posX;
    int posY = casillaOrigen.posY;
    Casilla destino = null;

    for (int paso = 1; paso <= cantidad; paso++)
    {
      int destinoX = posXBase + paso;
      if (destinoX > 3)
      {
        break;
      }

      Casilla casillaAdelante = lado.ObtenerCasillaPorIndex(destinoX, posY);
      if (casillaAdelante == null)
      {
        break;
      }

      if (casillaAdelante.Presente != null && casillaAdelante.Presente != gameObject)
      {
        destino = null;
        break;
      }

      destino = casillaAdelante;
    }

    if (destino == null || destino == CasillaPosicion || destino == CasillaForzadoaMover)
    {
      return;
    }

    CasillaForzadoaMover = destino;
    CasillaDeseadaMov = null;
    movimientoForzadoPendiente = true;
  }

  public virtual void ActualizarClaseComienzoTurno() //Método vacío que se llama cada vez que arranca turno de la unidad
  {
    //---
    //VACIO
    //Cada clase lo usará para determinar ciertos efectos en cada turno
  }
  public virtual void ComienzoBatallaClase() //Método vacío que se llama al comenzar la batalla
  {
    // Seguridad: recalcula por si la unidad llega con stats seteados por otra via.
    RecalcularVidaPorFuerza(true);
    RecalcularDefensaPorAgilidad();
    RecalcularResElementalesPorPoder();

    TirarIniciativa();
   
  }

  public void AcomodarSortingLayerDelay()
  { 
     Invoke("AcomodarSortingLayer", 2.0f);

  }
void AcomodarSortingLayer()
{
    
    // 1) Encontrar el/los canvas aunque están desactivados
    var canvases = GetComponentsInChildren<Canvas>(true);
    if (canvases == null || canvases.Length == 0)
    {
        print($"{name}: no encontró Canvas en hijos");
    }

    // 2) Orden por Y de la casilla (fallback por posición mundial)
    int y = (CasillaPosicion != null) ? CasillaPosicion.posY
                                      : 0;
    int orden = RenderOrderHelper.CalcularOrdenPorY(y);

    if (canvases != null)
    {
      foreach (var c in canvases)
      {
          // Recomendado: World Space (o Screen Space - Camera con tu cámara de batalla)
          if (c.renderMode == RenderMode.ScreenSpaceOverlay)
              c.renderMode = RenderMode.WorldSpace;

          // (Opcional) si usas ScreenSpace-Camera:
          // c.worldCamera = Camera.main; // o tu cámara de batalla
      }
    }

    RenderOrderHelper.AplicarOrdenBase(gameObject, orden, "UI3D");

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
   IntentarValentiaPorAcercarseAliado();

  }

  private void IntentarValentiaPorAcercarseAliado()
  {
    if (bonusAcercamientoValentiaAplicadoTurno)
    {
      return;
    }

    if (GetComponent<IAUnidad>() != null || scBattleManager == null || scBattleManager.unidadActiva != this)
    {
      return;
    }

    if (casillaOrigenEnMovimiento == null || CasillaPosicion == null || casillaOrigenEnMovimiento == CasillaPosicion)
    {
      return;
    }

    List<Unidad> aliadosAntes = ObtenerAliadosAdyacentesNoDiagonal(casillaOrigenEnMovimiento);
    List<Unidad> aliadosAhora = ObtenerAliadosAdyacentesNoDiagonal(CasillaPosicion);
    if (aliadosAhora.Count == 0)
    {
      return;
    }

    List<Unidad> aliadosNuevos = new List<Unidad>();
    foreach (Unidad aliado in aliadosAhora)
    {
      if (aliado == null || aliadosAntes.Contains(aliado))
      {
        continue;
      }

      aliadosNuevos.Add(aliado);
    }

    if (aliadosNuevos.Count == 0)
    {
      return;
    }

    bool enIngles = IdiomaInglesActivo();
    Unidad aliadoBajaVal = aliadosNuevos.FirstOrDefault(a => a.ValentiaP_actual < -1f);
    if (aliadoBajaVal != null)
    {
      string nombreUnidad = NombreUnidadParaLog(this);
      string nombreAliado = NombreUnidadParaLog(aliadoBajaVal);
      string motivo = enIngles
        ? nombreUnidad + " moves next to low-valour ally " + nombreAliado
        : nombreUnidad + " se acerca al aliado de valentía baja " + nombreAliado;
      SumarValentia(-1, motivo);
      bonusAcercamientoValentiaAplicadoTurno = true;
      return;
    }

    Unidad aliadoAltaVal = aliadosNuevos.FirstOrDefault(a => a.ValentiaP_actual > 1f);
    if (aliadoAltaVal != null)
    {
      string nombreUnidad = NombreUnidadParaLog(this);
      string nombreAliado = NombreUnidadParaLog(aliadoAltaVal);
      string motivo = enIngles
        ? nombreUnidad + " moves next to high-valour ally " + nombreAliado
        : nombreUnidad + " se acerca al aliado de valentía alta " + nombreAliado;
      SumarValentia(1, motivo);
      bonusAcercamientoValentiaAplicadoTurno = true;
    }
  }

  private List<Unidad> ObtenerAliadosAdyacentesNoDiagonal(Casilla centro)
  {
    List<Unidad> aliados = new List<Unidad>();
    if (centro == null || centro.ladoGO == null)
    {
      return aliados;
    }

    LadoManager lado = centro.ladoGO.GetComponent<LadoManager>();
    if (lado == null)
    {
      return aliados;
    }

    int[,] offsets = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
    for (int i = 0; i < offsets.GetLength(0); i++)
    {
      Casilla casilla = lado.ObtenerCasillaPorIndex(centro.posX + offsets[i, 0], centro.posY + offsets[i, 1]);
      if (casilla == null || casilla.Presente == null)
      {
        continue;
      }

      Unidad aliado = casilla.Presente.GetComponent<Unidad>();
      if (aliado == null || aliado == this || aliado.CasillaPosicion == null)
      {
        continue;
      }

      if (CasillaPosicion == null || aliado.CasillaPosicion.lado != CasillaPosicion.lado)
      {
        continue;
      }

      aliados.Add(aliado);
    }

    return aliados;
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
    movimientoForzadoPendiente = false;
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











