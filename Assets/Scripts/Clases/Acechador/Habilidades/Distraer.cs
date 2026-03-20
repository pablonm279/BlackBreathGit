using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class Distraer : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     ClaseAcechador claseAcechador;
      public override void  Awake()
    {


      nombre = "Distraer";
      costoAP = 1; 
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      IDenClase = 7;
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 2;  
      bAfectaObstaculos = false;

      esDiscreta = true; //No quita sigilo

      bonusAtaque = 0;
    
      XdDanio = 0;
      daniodX = 0; 
      tipoDanio = 0; 
      criticoRangoHab = 0;


      imHab = Resources.Load<Sprite>("imHab/Acechador_Distraer");
      ActualizarDescripcion();
    }
    
   void Start()
   {
  
   }

   public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    int dcBase = NIVEL > 1 ? 13 : 12;
    int apPenalty = -2;
    int defPenalty = -2;
    if (NIVEL > 2) { apPenalty -= 1; }
    if (NIVEL == 4) { apPenalty -= 1; defPenalty -= 1; }
    int escondidoGanado = NIVEL == 5 ? 2 : 1;

    string tituloEs = "Distraer I";
    string tituloEn = "Distract I";
    string tituloPt = "Distrair I";
    if (NIVEL == 2) { tituloEs = "Distraer II"; tituloEn = "Distract II"; }
    if (NIVEL == 3) { tituloEs = "Distraer III"; tituloEn = "Distract III"; }
    if (NIVEL == 4) { tituloEs = "Distraer IV a"; tituloEn = "Distract IV a"; }
    if (NIVEL == 5) { tituloEs = "Distraer IV b"; tituloEn = "Distract IV b"; }
    if (NIVEL == 2) { tituloPt = "Distrair II"; }
    if (NIVEL == 3) { tituloPt = "Distrair III"; }
    if (NIVEL == 4) { tituloPt = "Distrair IV a"; }
    if (NIVEL == 5) { tituloPt = "Distrair IV b"; }

    string lineaSalvacion = ConstruirLineaSalvacion(esIngles, TipoSalvacionDescripcion.Mental, dcBase);

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Ranged (5 range)\n";
      cuerpo += "<b>Target:</b> 1 enemy\n";
      cuerpo += "<b>Roll/Save:</b> no attack roll\n";
      cuerpo += lineaSalvacion + "\n";
      cuerpo += $"<b>On failed save:</b> Distracted (2 turns): {apPenalty} max AP, {defPenalty} Defense\n";
      cuerpo += $"<b>If target is isolated:</b> gain Hidden ({escondidoGanado})\n";
      cuerpo += "<b>Stealth interaction:</b> Discreet (does not reveal the caster)";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Alcance (5 de alcance)\n";
      cuerpo += "<b>Alvo:</b> 1 inimigo\n";
      cuerpo += "<b>Rolagem/Resistencia:</b> sem rolagem de ataque\n";
      cuerpo += lineaSalvacion + "\n";
      cuerpo += $"<b>Se falhar na resistencia:</b> Distraido (2 turnos): {apPenalty} AP max, {defPenalty} Defesa\n";
      cuerpo += $"<b>Se o alvo estiver isolado:</b> ganha Escondido ({escondidoGanado})\n";
      cuerpo += "<b>Interacao com furtividade:</b> Discreta (nao revela o lancador)";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 enemigo\n";
      cuerpo += "<b>Tirada/TS:</b> no tiene tirada de ataque\n";
      cuerpo += lineaSalvacion + "\n";
      cuerpo += $"<b>Si falla TS:</b> Distraido (2 turnos): {apPenalty} AP max, {defPenalty} Defensa\n";
      cuerpo += $"<b>Si el objetivo esta aislado:</b> ganas Escondido ({escondidoGanado})\n";
      cuerpo += "<b>Interaccion con sigilo:</b> Discreta (no revela al lanzador)";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "The Stalker uses a trick to distract the enemy and leave them vulnerable."
        : esPortugues
          ? "O Espreitador usa um truque para distrair o inimigo e deixa-lo vulneravel."
        : "El Acechador utiliza un truco para distraer al enemigo y dejarlo vulnerable.",
      cuerpo,
      costos,
      "#5dade2");

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC base.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 max AP in Distracted debuff.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 max AP reduction and +1 Defense reduction) or Option B (gain Hidden II if isolated).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 na CD base da resistencia.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 AP max no debuff Distraido.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 reducao de AP max e +1 reducao de Defesa) ou Opcao B (ganhar Escondido II se estiver isolado).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al DC base de TS.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 AP max en el debuff Distraido.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 reduccion de AP max y +1 reduccion de Defensa) u Opcion B (ganar Escondido II si esta aislado).</color>"; }
    }
  }

  int damExtra;
      Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      int DC = 12;
      if (NIVEL > 1) { DC++; }
      Unidad objetivo = (Unidad)obj;
      VFXAplicar(objetivo.gameObject);
      if (objetivo.TiradaSalvacion(objetivo.mod_TSMental, DC))
      {

        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Distraído";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAPMax -= 2;
        buff.cantDefensa -= 2;
        if (NIVEL > 2) { buff.cantAPMax--; }
        if (NIVEL == 4) { buff.cantAPMax--; buff.cantDefensa--; }
        

        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        if (objetivo.ChequearEstaAislado(2))
        {
          if (NIVEL < 5) { scEstaUnidad.GanarEscondido(1); }
          if (NIVEL == 5) {  scEstaUnidad.GanarEscondido(2);}
         
        }


      }


    }
  }

  protected override float? CalcularProbabilidadEspecialSobreObjetivo(Unidad objetivo)
  {
    if (objetivo == null)
    {
      return null;
    }

    int dc = 12;
    if (NIVEL > 1)
    {
      dc++;
    }

    return CalcularProbabilidadFallarTS(objetivo.mod_TSMental, dc);
  }

  protected override string ObtenerTextoProbabilidadSobreObjetivo(Unidad objetivo, float probabilidad)
  {
    return FormatearTextoProbabilidadExito(probabilidad);
  }
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Distraer");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
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
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      //Melee - Si está en columna 3 de su lado, aumenta el rango ignorando cada columna vacia del lado opuesto
      int rangoPlus = 0;
   
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(5,1);
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
       
       c.ActivarCapaColorRojo();
       if(esMelee)//Si hab es melee, activa capa roja, de columna al alcance final, no de las otras también
       {
         if(c.transform.GetChild(2).gameObject.activeInHierarchy){ c.DesactivarCapaColorRojo();}
       } 



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
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
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





