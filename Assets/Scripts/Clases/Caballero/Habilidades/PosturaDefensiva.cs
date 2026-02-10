using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PosturaDefensiva : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
     public override void  Awake()
    {
      nombre = "Postura Defensiva";
      IDenClase = 7;
      costoAP = 2; //Termina turno
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Caballero_PosturaDefensiva");
      ActualizarDescripcion();
    }

   
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

      int bonoDefensa = 1 + (NIVEL > 1 ? 1 : 0);
      int bonoAtaque = NIVEL > 2 ? 1 : 0;
      int usosReaccion = NIVEL == 5 ? 2 : 1;
      bool seCancelaAlRecibirDanio = NIVEL != 4;

      string tituloEs = "Postura Defensiva I";
      string tituloEn = "Defensive Stance I";
      if (NIVEL == 2) { tituloEs = "Postura Defensiva II"; tituloEn = "Defensive Stance II"; }
      if (NIVEL == 3) { tituloEs = "Postura Defensiva III"; tituloEn = "Defensive Stance III"; }
      if (NIVEL == 4) { tituloEs = "Postura Defensiva IV a"; tituloEn = "Defensive Stance IV a"; }
      if (NIVEL == 5) { tituloEs = "Postura Defensiva IV b"; tituloEn = "Defensive Stance IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Self Buff + Reaction\n";
        cuerpo += "<b>Target:</b> Self\n";
        cuerpo += $"<b>Buff (2 turns):</b> +{bonoDefensa} Defense";
        if (bonoAtaque > 0)
        {
          cuerpo += $", +{bonoAtaque} Attack";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Reaction:</b> Counterattack with Vertical Cut when an enemy misses a melee attack ({usosReaccion} use/s)\n";
        cuerpo += seCancelaAlRecibirDanio
          ? "<b>Reaction cancel:</b> removed when taking damage"
          : "<b>Reaction cancel:</b> does not get removed when taking damage";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Auto Buff + Reaccion\n";
        cuerpo += "<b>Objetivo:</b> Uno mismo\n";
        cuerpo += $"<b>Buff (2 turnos):</b> +{bonoDefensa} Defensa";
        if (bonoAtaque > 0)
        {
          cuerpo += $", +{bonoAtaque} Ataque";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Reaccion:</b> contraataca con Corte Vertical cuando un enemigo falla un ataque melee ({usosReaccion} uso/s)\n";
        cuerpo += seCancelaAlRecibirDanio
          ? "<b>Cancelacion de reaccion:</b> se elimina al recibir danio"
          : "<b>Cancelacion de reaccion:</b> no se elimina al recibir danio";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Val Cost: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Val: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "The Knight braces for incoming melee and answers with punishing counters."
          : "El Caballero se planta para recibir melee y responder con contraataques.",
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense buff.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Attack buff during stance.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no cancel on hit) or Option B (+1 reaction use).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al buff de Defensa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al buff de Ataque durante la postura.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (no se cancela al recibir golpe) u Opcion B (+1 uso de reaccion).</color>"; }
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

    if(obj is Unidad) //AcÃ¡ van los efectos a Unidades.
     {

       Unidad objetivo = (Unidad)obj;
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");

      VFXAplicar(objetivo.gameObject);
       /////////////////////////////////////////////
       //BUFF ---- AsÃ­ se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Postura Defensiva";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantDefensa += 1;
       if(NIVEL > 1){ buff.cantDefensa += 1;}
       if(NIVEL > 2){ buff.cantAtaque += 1;}
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuraciÃ³n del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       objetivo.Marcar(0);

       //Agrega la reacciÃ³n 
       ReaccionPosturaDefensiva reaccion = new ReaccionPosturaDefensiva();
       reaccion.NIVEL = NIVEL;
       reaccion.permanente = false;
       reaccion.nombre = "Postura Defensiva";
       ReaccionPosturaDefensiva reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

       //Usarla termina el turno
       BattleManager.Instance.TerminarTurno();
     }
    
    
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PosturaDefensiva");

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



