using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class EscudoEnergetico : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
     public override void  Awake()
    {
      nombre = "Escudo Energético";
      IDenClase = 6;
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

      imHab = Resources.Load<Sprite>("imHab/Canalizador_EscudoEnergetico");
    }


        public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

        string tituloEs = "Escudo Energetico I";
        string tituloEn = "Energy Shield I";
        if (NIVEL == 2) { tituloEs = "Escudo Energetico II"; tituloEn = "Energy Shield II"; }
        if (NIVEL == 3) { tituloEs = "Escudo Energetico III"; tituloEn = "Energy Shield III"; }
        if (NIVEL == 4) { tituloEs = "Escudo Energetico IV a"; tituloEn = "Energy Shield IV a"; }
        if (NIVEL == 5) { tituloEs = "Escudo Energetico IV b"; tituloEn = "Energy Shield IV b"; }

        int defensaBase = NIVEL > 1 ? 2 : 1;
        int bonusAtaqueReaccion = NIVEL > 2 ? 1 : 0;
        int usosReaccion = NIVEL == 5 ? 3 : 2;
        bool seCancelaConDanio = NIVEL != 4;

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Self\n";
            cuerpo += "<b>Target:</b> Self\n";
            cuerpo += $"<b>Defense Buff:</b> {defensaBase} + current Energy Tier (2 rounds)\n";
            cuerpo += $"<b>Reaction:</b> On failed enemy projectile, counters with Arcane Discharge";
            if (bonusAtaqueReaccion > 0)
            {
                cuerpo += $" (+{bonusAtaqueReaccion} attack roll)";
            }
            cuerpo += " and creates 1 Energy Residue nearby\n";
            cuerpo += $"<b>Reaction Uses per cast:</b> {usosReaccion}\n";
            cuerpo += seCancelaConDanio
                ? "<b>Condition:</b> Shield is removed if user takes damage"
                : "<b>Condition:</b> Shield is not removed by incoming damage";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Propia\n";
            cuerpo += "<b>Objetivo:</b> Propio usuario\n";
            cuerpo += $"<b>Buff de Defensa:</b> {defensaBase} + Nivel de Energia actual (2 rondas)\n";
            cuerpo += $"<b>Reaccion:</b> Ante proyectil enemigo fallido, contraataca con Descarga Arcana";
            if (bonusAtaqueReaccion > 0)
            {
                cuerpo += $" (+{bonusAtaqueReaccion} a la tirada de ataque)";
            }
            cuerpo += " y genera 1 Residuo Energetico cercano\n";
            cuerpo += $"<b>Usos de la reaccion por casteo:</b> {usosReaccion}\n";
            cuerpo += seCancelaConDanio
                ? "<b>Condicion:</b> El escudo se cancela si recibe danio"
                : "<b>Condicion:</b> El escudo no se cancela al recibir danio";
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
            : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentía: {costoPM}";

        txtDescripcion = ConstruirDescripcionEstandar(
            esIngles ? tituloEn : tituloEs,
            esIngles
                ? "The Channeler forms a reactive barrier that reinforces defense and punishes ranged pressure."
                : "El Canalizador forma una barrera reactiva que refuerza defensa y castiga la presion a distancia.",
            cuerpo,
            costos,
            "#5dade2");

        bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel)
        {
            return;
        }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 defense base.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack roll on counter discharge.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no cancel on damage) or Option B (+1 reaction use).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 defensa base.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 a la tirada de ataque de la descarga de reaccion.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (no se cancela por danio) u Opcion B (+1 uso de reaccion).</color>"; }
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

    if(obj is Unidad) //Acá van los efectos a Unidades.
     {

       Unidad objetivo = (Unidad)obj;
         VFXAplicar(objetivo.gameObject);
      ClaseCanalizador scCana = (ClaseCanalizador)objetivo;
      float defensa = 10*scCana.ObtenerEnergia(); 
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Escudo Energético";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantDefensa += 1+defensa;
       if (NIVEL > 1) { buff.cantDefensa += 1; }
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       objetivo.Marcar(0);

       //Agrega la reacción 
       ReaccionEscudoEnergetico reaccion = new ReaccionEscudoEnergetico();
       reaccion.NIVEL = NIVEL;
       reaccion.permanente = false;
       reaccion.nombre = "Escudo Energético";
       ReaccionEscudoEnergetico reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

       //Usarla termina el turno
      // BattleManager.Instance.TerminarTurno();
     }
    
    
    }
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_EscudoEnergetico");

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





