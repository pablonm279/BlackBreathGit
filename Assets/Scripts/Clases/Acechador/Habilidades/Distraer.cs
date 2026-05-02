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
    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

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

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Ranged control (5 range)\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> 1 enemy\n";
      cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> no attack roll\n";
      cuerpo += $"<color={colorEncabezado}><b>Save:</b></color> Mental vs DC {dcBase}\n";
      cuerpo += $"<color={colorEncabezado}><b>On failed save:</b></color> Distracted (2 turns): {apPenalty} max AP, {defPenalty} Defense\n";
      cuerpo += $"<color={colorEncabezado}><b>If isolated:</b></color> gain Hidden ({escondidoGanado})\n";
      cuerpo += $"<color={colorEncabezado}><b>Stealth:</b></color> Discreet; does not reveal the caster";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Controle a distancia (5 de alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> 1 inimigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> sem rolagem de ataque\n";
      cuerpo += $"<color={colorEncabezado}><b>Resistencia:</b></color> Mental vs CD {dcBase}\n";
      cuerpo += $"<color={colorEncabezado}><b>Se falhar:</b></color> Distraido (2 turnos): {apPenalty} AP max, {defPenalty} Defesa\n";
      cuerpo += $"<color={colorEncabezado}><b>Se estiver isolado:</b></color> ganha Escondido ({escondidoGanado})\n";
      cuerpo += $"<color={colorEncabezado}><b>Furtividade:</b></color> Discreta; nao revela o lancador";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Control a distancia (5 alcance)\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> 1 enemigo\n";
      cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> no tiene tirada de ataque\n";
      cuerpo += $"<color={colorEncabezado}><b>TS:</b></color> Mental vs DC {dcBase}\n";
      cuerpo += $"<color={colorEncabezado}><b>Si falla TS:</b></color> Distraido (2 turnos): {apPenalty} AP max, {defPenalty} Defensa\n";
      cuerpo += $"<color={colorEncabezado}><b>Si esta aislado:</b></color> ganas Escondido ({escondidoGanado})\n";
      cuerpo += $"<color={colorEncabezado}><b>Sigilo:</b></color> Discreta; no revela al lanzador";
    }

    string subtitulo = esIngles
      ? "Distracts one enemy and can restore Hidden."
      : esPortugues
        ? "Distrai um inimigo e pode recuperar Escondido."
        : "Distrae a un enemigo y puede recuperar Escondido.";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

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





