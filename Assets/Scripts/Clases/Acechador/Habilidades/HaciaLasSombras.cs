using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class HaciaLasSombras : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
      public override void  Awake()
    {
      nombre = "Hacia Las Sombras";
      IDenClase = 9; // Termina turno
      costoAP = 1;
      costoPM = 1;
      if(NIVEL == 5){costoPM--;}
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 7;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;
      poneTrampas = true;

      imHab = Resources.Load<Sprite>("imHab/Acechador_HaciaLasSombras");

       
      ActualizarDescripcion();
    
    }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

    int evasionGanada = NIVEL > 2 ? 3 : 2;
    bool terminaTurno = NIVEL != 4;

    string tituloEs = "Hacia Las Sombras I";
    string tituloEn = "Into the Shadows I";
    if (NIVEL == 2) { tituloEs = "Hacia Las Sombras II"; tituloEn = "Into the Shadows II"; }
    if (NIVEL == 3) { tituloEs = "Hacia Las Sombras III"; tituloEn = "Into the Shadows III"; }
    if (NIVEL == 4) { tituloEs = "Hacia Las Sombras IV a"; tituloEn = "Into the Shadows IV a"; }
    if (NIVEL == 5) { tituloEs = "Hacia Las Sombras IV b"; tituloEn = "Into the Shadows IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Mobility Utility\n";
      cuerpo += "<b>Target:</b> Any empty tile on own side\n";
      cuerpo += "<b>Roll/Save:</b> none\n";
      cuerpo += "<b>On cast:</b> teleports to target tile\n";
      cuerpo += $"<b>Self effects:</b> gains Hidden (2), Evasion ({evasionGanada}), removes debuffs\n";
      cuerpo += terminaTurno
        ? "<b>Turn flow:</b> ends turn"
        : "<b>Turn flow:</b> does not end turn";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Utilidad de Movilidad\n";
      cuerpo += "<b>Objetivo:</b> Cualquier casilla vacia de tu lado\n";
      cuerpo += "<b>Tirada/TS:</b> no tiene\n";
      cuerpo += "<b>Al lanzarla:</b> se teletransporta a la casilla objetivo\n";
      cuerpo += $"<b>Efectos propios:</b> gana Escondido (2), Evasion ({evasionGanada}), remueve debuffs\n";
      cuerpo += terminaTurno
        ? "<b>Flujo de turno:</b> termina turno"
        : "<b>Flujo de turno:</b> no termina turno";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "A defensive reset that repositions, cleanses and re-enters stealth."
        : "Un reset defensivo que reposiciona, limpia estados y vuelve al sigilo.",
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Evasion gained.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (does not end turn) or Option B (-1 Valour Cost).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Evasion ganada.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (no termina turno) u Opcion B (-1 costo de Valentía).</color>"; }
    }
  }

     Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());
      
        
    }



  public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
  {
    scEstaUnidad.GanarEscondido(2);
    scEstaUnidad.estado_evasion = 2;
    if (NIVEL > 2) { scEstaUnidad.estado_evasion++; }
    scEstaUnidad.RemoverfDebuffstBuffs(false); //Remueve todos los debuffs, si los tuviera.

    VFXAplicar(scEstaUnidad.gameObject);    
    scEstaUnidad.TeletransportarACasilla(cas);
    if (NIVEL != 4) { BattleManager.Instance.TerminarTurno(); }
    
   
  }
        void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_HaciaLasSombras");

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
      
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
     
      List<Casilla> alCasillasafectadas2 = new List<Casilla>();
      //Casillas Alrededor al origen
      alCasillasafectadas2 = Origen.ObtenerCasillasMismoLado();
      alCasillasafectadas2.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in alCasillasafectadas2)
      {
         c.ActivarCapaColorAzul();
        if(c.Presente != null)
        {
            continue;
        }
        
        lCasillasafectadas.Add(c);
       

      }
    
         
    }

   
    

 
}




