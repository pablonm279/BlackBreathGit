using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CastigaraLosMalvados : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
      public override void  Awake()
    {
      nombre = "Castigar a los Malvados";
      IDenClase = 8;
      costoAP = 2;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Purificadora_CastigarMalvados");
     
    }

   public override void ActualizarDescripcion()
   {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int dcBase = NIVEL > 1 ? 11 : 10;
      int usos = NIVEL == 4 ? 3 : 2;
      string fraccionDanio = NIVEL == 5 ? "1/2" : "1/3";

      string tituloEs = "Castigar a los Malvados I";
      string tituloEn = "Punish the Wicked I";
      string tituloPt = "Castigar os Malvados I";
      if (NIVEL == 2) { tituloEs = "Castigar a los Malvados II"; tituloEn = "Punish the Wicked II"; }
      if (NIVEL == 3) { tituloEs = "Castigar a los Malvados III"; tituloEn = "Punish the Wicked III"; }
      if (NIVEL == 4) { tituloEs = "Castigar a los Malvados IV a"; tituloEn = "Punish the Wicked IV a"; }
      if (NIVEL == 5) { tituloEs = "Castigar a los Malvados IV b"; tituloEn = "Punish the Wicked IV b"; }
      if (NIVEL == 2) { tituloPt = "Castigar os Malvados II"; }
      if (NIVEL == 3) { tituloPt = "Castigar os Malvados III"; }
      if (NIVEL == 4) { tituloPt = "Castigar os Malvados IV a"; }
      if (NIVEL == 5) { tituloPt = "Castigar os Malvados IV b"; }

      string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Mental, dcBase, "Poder", "Power", poderActual);
      string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Mental, dcBase, "Poder", "Power", poderActual);

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Reactive mark\n";
        cuerpo += "<b>Target:</b> 1 enemy unit on the opposite side\n";
        cuerpo += "<b>Trigger:</b> Each time the marked unit damages one of your allies\n";
        cuerpo += lineaSalvacionEn + "\n";
        cuerpo += $"<b>On failed save:</b> Lose all remaining AP and take 1d6 + <color=#ea0606>Power ({poderActual})</color> + {fraccionDanio} of damage dealt | <b>Type:</b> Divine\n";
        cuerpo += $"<b>Duration:</b> Up to {usos} failed saves, or ends early if the target succeeds the save";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Marca reativa\n";
        cuerpo += "<b>Alvo:</b> 1 unidade inimiga do lado oposto\n";
        cuerpo += "<b>Gatilho:</b> Cada vez que a unidade marcada causar dano a um aliado seu\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += $"<b>Se falhar na resistencia:</b> Perde todo AP restante e sofre 1d6 + <color=#ea0606>Poder ({poderActual})</color> + {fraccionDanio} do dano causado | <b>Tipo:</b> Divino\n";
        cuerpo += $"<b>Duracao:</b> Ate {usos} falhas na resistencia, ou termina antes se o alvo passar na resistencia";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Marca reactiva\n";
        cuerpo += "<b>Objetivo:</b> 1 unidad enemiga del lado opuesto\n";
        cuerpo += "<b>Disparo:</b> Cada vez que la unidad marcada dana a un aliado tuyo\n";
        cuerpo += lineaSalvacionEs + "\n";
        cuerpo += $"<b>Si falla TS:</b> Pierde todo su AP restante y recibe 1d6 + <color=#ea0606>Poder ({poderActual})</color> + {fraccionDanio} del dano infligido | <b>Tipo:</b> Divino\n";
        cuerpo += $"<b>Duracion:</b> Hasta {usos} fallos de TS, o termina antes si el objetivo supera la TS";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "Marks a target with divine retribution that punishes aggression."
          : esPortugues
            ? "Marca um alvo com uma represalia divina que pune a agressao."
          : "Marca un objetivo con una represalia divina que castiga la agresion.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = EsEscenaCampaña()
        && CampaignManager.Instance != null
        && CampaignManager.Instance.scMenuPersonajes != null
        && CampaignManager.Instance.scMenuPersonajes.pSel != null
        && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 use) or Option B (damage share to 1/2).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 CD de resistencia.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 uso) ou Opcao B (proporcao de dano para 1/2).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC de salvacion.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 uso) u Opcion B (proporcion de dano a 1/2).</color>"; }
      }
   }

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

      Unidad objetivo = (Unidad)obj;

      BattleManager.Instance.EscribirLog(
        TRADU.i.Traducir(scEstaUnidad.uNombre) + " " +
        TRADU.i.Traducir("usa ") +
        TRADU.i.Traducir(nombre) + " -> " +
        TRADU.i.Traducir(objetivo.uNombre) + ".");
      VFXAplicar(objetivo.gameObject);
      //Agrega la reacción 
      ReaccionCastigarMalvados reaccion = new ReaccionCastigarMalvados();
      reaccion.variableUnidad = scEstaUnidad;
      reaccion.NIVEL = NIVEL;
      reaccion.nombre = "Castigar a los Malvados";
      ReaccionCastigarMalvados reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

      objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Castigar a los Malvados"), Color.yellow);
      }
     
    }
    
  
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CastigarMalvados");

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
     
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasLadoOpuesto();
    
    
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
             c.ActivarCapaColorRojo();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
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

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}




