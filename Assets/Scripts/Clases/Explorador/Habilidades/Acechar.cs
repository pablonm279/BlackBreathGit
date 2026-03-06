using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Acechar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Acechar";
      costoAP = 1;
      costoPM = 0;
      IDenClase = 7;

      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Explorador_Acechar");

       
      ActualizarDescripcion();
    
    }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    int buffAtaque = 2 + (NIVEL > 1 ? 1 : 0);
    int buffCrit = NIVEL > 2 ? 1 : 0;
    if (NIVEL == 4) { buffCrit += 2; }
    int duracionTurnos = 2;
    bool seRemueveAlDanar = NIVEL != 5;

    string tituloEs = "Acechar I";
    string tituloEn = "Hide I";
    if (NIVEL == 2) { tituloEs = "Acechar II"; tituloEn = "Hide II"; }
    if (NIVEL == 3) { tituloEs = "Acechar III"; tituloEn = "Hide III"; }
    if (NIVEL == 4) { tituloEs = "Acechar IV a"; tituloEn = "Hide IV a"; }
    if (NIVEL == 5) { tituloEs = "Acechar IV b"; tituloEn = "Hide IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Self Buff\n";
      cuerpo += "<b>Target:</b> Self\n";
      cuerpo += "<b>On cast:</b> gains Hidden (1)\n";
      cuerpo += $"<b>Buff ({duracionTurnos} turns):</b> +15% Damage, +{buffAtaque} Attack";
      if (buffCrit > 0)
      {
        cuerpo += $", +{buffCrit} crit range";
      }
      cuerpo += "\n";
      cuerpo += seRemueveAlDanar
        ? "<b>Buff removal:</b> removed after dealing damage"
        : "<b>Buff removal:</b> does not get removed after dealing damage";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Auto Buff\n";
      cuerpo += "<b>Objetivo:</b> Uno mismo\n";
      cuerpo += "<b>Al lanzarla:</b> gana Escondido (1)\n";
      cuerpo += $"<b>Buff ({duracionTurnos} turnos):</b> +15% Danio, +{buffAtaque} Ataque";
      if (buffCrit > 0)
      {
        cuerpo += $", +{buffCrit} rango critico";
      }
      cuerpo += "\n";
      cuerpo += seRemueveAlDanar
        ? "<b>Remocion del buff:</b> se elimina al hacer danio"
        : "<b>Remocion del buff:</b> no se elimina al hacer danio";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "The Explorer vanishes from sight and primes a short offensive spike."
        : "El Explorador se oculta y prepara una subida ofensiva breve.",
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack buff.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 crit range buff.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 crit range) or Option B (buff persists after damage).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al buff de Ataque.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al buff de rango critico.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+2 rango critico) u Opcion B (el buff persiste al danar).</color>"; }
    }
  }
  public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
    {
        // El log de uso ahora está centralizado en Habilidad.Resolver
      await  base.Resolver(Objetivos);
    }



    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

    
      if(obj is Unidad) //Acá van los efectos a Unidades.
      {

        Unidad objetivo = (Unidad)obj;
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");
        VFXAplicar(objetivo.gameObject);
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acechando";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque += 2;
        buff.cantDanioPorcentaje += 15;
        if(NIVEL > 1){ buff.cantAtaque += 1;}
        if(NIVEL > 2){ buff.cantCritDado += 1;}
        if(NIVEL == 4){ buff.cantCritDado += 2;}
        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
        objetivo.Marcar(0);

        
        //Usarla termina el turno
        BattleManager.Instance.TerminarTurno();

        //Agrega acechar
        objetivo.GanarEscondido(1);
      }
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Acechar");

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
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
        if(c.Presente == null)
        {
            continue;
        }
        
       
        if(c.Presente.GetComponent<Unidad>() == null)
        {
            continue;
        }
           
        if(c.Presente.GetComponent<Unidad>() != null)
        {
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
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





