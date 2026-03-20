using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Purificacion : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
     public override void  Awake()
    {
      nombre = "Purificación";
      IDenClase = 9;
      costoAP = 6;
      costoPM = 0;
      
      
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 10;
      esforzable = 3;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5; 
      bAfectaObstaculos = false;

      targetEspecial = 0; 

      bonusAtaque +=0; //0
      XdDanio = 1;
      daniodX = 5; 
      tipoDanio = 11; //Divino
     

      imHab = Resources.Load<Sprite>("imHab/Purificadora_Purificacion");
      
     

      
    }

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      ClasePurificadora scPurificadora = Usuario != null ? Usuario.GetComponent<ClasePurificadora>() : null;
      int fervorActual = scPurificadora != null ? scPurificadora.ObtenerFervor() : 0;
      int multiplicadorFervor = 1 + fervorActual;
      int baseMin = 3 + Mathf.FloorToInt(poderActual / 2f);
      int baseMax = 7 + Mathf.FloorToInt(poderActual / 2f);
      int danioMinConFervor = baseMin * multiplicadorFervor;
      int danioMaxConFervor = baseMax * multiplicadorFervor;

      string tituloEs = "Purificacion I";
      string tituloEn = "Purification I";
      string tituloPt = "Purificacao I";
      if (NIVEL == 2) { tituloEs = "Purificacion II"; tituloEn = "Purification II"; }
      if (NIVEL == 3) { tituloEs = "Purificacion III"; tituloEn = "Purification III"; }
      if (NIVEL == 4) { tituloEs = "Purificacion IV a"; tituloEn = "Purification IV a"; }
      if (NIVEL == 5) { tituloEs = "Purificacion IV b"; tituloEn = "Purification IV b"; }
      if (NIVEL == 2) { tituloPt = "Purificacao II"; }
      if (NIVEL == 3) { tituloPt = "Purificacao III"; }
      if (NIVEL == 4) { tituloPt = "Purificacao IV a"; }
      if (NIVEL == 5) { tituloPt = "Purificacao IV b"; }

      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Reflejos, 9, "Poder", "Power", poderActual);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Reflejos, 9, "Poder", "Power", poderActual);

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Area (10 range)\n";
        cuerpo += "<b>Target:</b> All enemies in the selected large area\n";
        cuerpo += lineaSalvacionEn + "\n";
        cuerpo += $"<b>Damage:</b> (2 + 1d5 + Power ({poderActual}) / 2) x (1 + Fervor ({fervorActual})) | <b>Type:</b> Divine\n";
        cuerpo += $"<b>Current range with Fervor:</b> {danioMinConFervor}-{danioMaxConFervor} (save success), {danioMinConFervor * 2}-{danioMaxConFervor * 2} (failed save)\n";
        cuerpo += "<b>On failed save:</b> Burning 2 and double damage\n";
        cuerpo += "<b>On cast:</b> Fervor is set to 0";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Area (10 de alcance)\n";
        cuerpo += "<b>Alvo:</b> Todos os inimigos da area selecionada\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += $"<b>Dano:</b> (2 + 1d5 + Poder ({poderActual}) / 2) x (1 + Fervor ({fervorActual})) | <b>Tipo:</b> Divino\n";
        cuerpo += $"<b>Faixa atual com Fervor:</b> {danioMinConFervor}-{danioMaxConFervor} (se passar na resistencia), {danioMinConFervor * 2}-{danioMaxConFervor * 2} (se falhar na resistencia)\n";
        cuerpo += "<b>Se falhar na resistencia:</b> Queimando 2 e dano dobrado\n";
        cuerpo += "<b>Ao usar:</b> Fervor vai para 0";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Area (10 alcance)\n";
        cuerpo += "<b>Objetivo:</b> Todos los enemigos del area seleccionada\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += $"<b>Danio:</b> (2 + 1d5 + Poder ({poderActual}) / 2) x (1 + Fervor ({fervorActual})) | <b>Tipo:</b> Divino\n";
        cuerpo += $"<b>Rango actual con Fervor:</b> {danioMinConFervor}-{danioMaxConFervor} (si supera TS), {danioMinConFervor * 2}-{danioMaxConFervor * 2} (si falla TS)\n";
        cuerpo += "<b>Si falla TS:</b> Ardiendo 2 y danio duplicado\n";
        cuerpo += "<b>Al lanzar:</b> Fervor queda en 0";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A wide divine purge fueled by current Fervor."
          : esPortugues
            ? "Uma purga divina ampla alimentada pelo Fervor atual."
          : "Una purga divina masiva alimentada por el Fervor actual.",
        cuerpo,
        costos,
        "#5dade2");
    }

    Casilla Origen;
    public override void Activar()
    { 
        seTiroFlechaVFX = false;
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();
        
      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    bool seTiroFlechaVFX = false;
  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {

      Unidad objetivo = (Unidad)obj;
      float dificultadAtributo = 9 + scEstaUnidad.mod_CarPoder;

      VFXAplicar(objetivo.gameObject);


      float damDivino =  2 + UnityEngine.Random.Range(1, 6) + (scEstaUnidad.mod_CarPoder / 2); //Se pone la mitad, si no se salva, se duplica el daño
      ClasePurificadora pur = (ClasePurificadora)scEstaUnidad;
      int fervor = pur.ObtenerFervor();
      print("Se uso fervor: " + pur.ObtenerFervor());
      damDivino = damDivino* (1 + fervor);


      if (objetivo.TiradaSalvacion(objetivo.mod_TSReflejos, dificultadAtributo))
      {
        //Si falla la tirada de salvación, se aplica el daño completo (en vez de la mitad) y arde
        objetivo.estado_ardiendo += 2;
        damDivino = damDivino*2;


      }

      objetivo.RecibirDanio(damDivino, 11, false, scEstaUnidad);

      Invoke("BorrarFervor", 1.5f);

    }
     
  }
  void BorrarFervor()
  {
    ClasePurificadora pur = (ClasePurificadora)scEstaUnidad;
    if(NIVEL == 5)
    {
      pur.CambiarFervor(1);
    }
    else
    {
      pur.CambiarFervor(-100);
    }
    pur.CambiarFervor(-100);

  }

  public override Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
  {



    return base.Resolver(Objetivos, casillaOrigenTrampas);

  }
  
  
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Purificacion");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

   private void ObtenerObjetivos()
    {
      //Cualquier objetivo
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(10,10);
    
       foreach(Casilla c in lCasillasafectadas)
      {
       
       c.ActivarCapaColorRojo();
        if(c.Presente == null)
        {
            continue;
        }
        
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
           }

        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

    
    
}




