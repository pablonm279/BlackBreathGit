using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class SifonArcano : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
      public override void  Awake()
    {
      nombre = "Sifón Arcano";
      IDenClase = 7;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Canalizador_SifonArcano");
      ActualizarDescripcion();
     
    }
  public override void ActualizarDescripcion()
  {
    var statsUI = ObtenerStatsDescripcionUI();
    int poderActual = statsUI.Poder;
    int bonusDCNivel = NIVEL > 2 ? 1 : 0;
    int dcBase = 8 + bonusDCNivel;
    int duracionTurnos = NIVEL == 5 ? 4 : 3;
    int bonusDanioBase = NIVEL > 1 ? 1 : 0;
    string lineaSalvacionEs = ConstruirLineaSalvacion(false, TipoSalvacionDescripcion.Fortaleza, dcBase, "Poder", "Power", poderActual);
    string lineaSalvacionEn = ConstruirLineaSalvacion(true, TipoSalvacionDescripcion.Fortaleza, dcBase, "Poder", "Power", poderActual);

    string tituloEs = "Sifon Arcano I";
    string tituloEn = "Arcane Siphon I";
    if (NIVEL == 2) { tituloEs = "Sifon Arcano II"; tituloEn = "Arcane Siphon II"; }
    if (NIVEL == 3) { tituloEs = "Sifon Arcano III"; tituloEn = "Arcane Siphon III"; }
    if (NIVEL == 4) { tituloEs = "Sifon Arcano IV a"; tituloEn = "Arcane Siphon IV a"; }
    if (NIVEL == 5) { tituloEs = "Sifon Arcano IV b"; tituloEn = "Arcane Siphon IV b"; }

    string lineaDanioEs = bonusDanioBase > 0
      ? $"<b>Danio por turno:</b> (1d10 + {bonusDanioBase}) x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano"
      : "<b>Danio por turno:</b> 1d10 x (1 + Residuos Energeticos) | <b>Tipo:</b> Arcano";
    string lineaDanioEn = bonusDanioBase > 0
      ? $"<b>Turn Damage:</b> (1d10 + {bonusDanioBase}) x (1 + Energy Residues) | <b>Type:</b> Arcane"
      : "<b>Turn Damage:</b> 1d10 x (1 + Energy Residues) | <b>Type:</b> Arcane";

    if (TRADU.i != null && TRADU.i.nIdioma == 2)
    {
      string cuerpo = "";
      cuerpo += "<b>Type:</b> Ranged (5 range)\n";
      cuerpo += "<b>Target:</b> 1 enemy unit on the opposite side\n";
      cuerpo += lineaSalvacionEn + "\n";
      cuerpo += $"<b>On failed save:</b> Applies Arcane Siphon for {duracionTurnos} turns\n";
      cuerpo += lineaDanioEn + "\n";
      cuerpo += "<b>On kill by this effect:</b> +1 permanent AP max, +10% Damage and +1 Energy";

      string costos = $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Val Cost: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        tituloEn,
        "Marks the target with an unstable link that drains vitality over time, amplified by Energy Residues.",
        cuerpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 base turn damage.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 DC.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A or B (IV upgrades).</color>"; }
      }
      return;
    }

    {
      string cuerpo = "";
      cuerpo += "<b>Tipo:</b> Rango (5 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 unidad enemiga del lado opuesto\n";
      cuerpo += lineaSalvacionEs + "\n";
      cuerpo += $"<b>Si falla TS:</b> aplica Sifon Arcano por {duracionTurnos} turnos\n";
      cuerpo += lineaDanioEs + "\n";
      cuerpo += "<b>Si mata con este efecto:</b> +1 AP max permanente, +10% Danio y +1 Energia";

      string costos = $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Val: {costoPM} ";

      txtDescripcion = ConstruirDescripcionEstandar(
        tituloEs,
        "Marca al objetivo con un vinculo inestable que drena vitalidad por turnos, amplificado por Residuos Energeticos.",
        cuerpo,
        costos,
        "#5dade2");

      if (EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al danio base por turno.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 DC.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A u Opcion B (mejoras IV).</color>"; }
      }
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

    if (obj is Unidad uni) //Acá van los efectos a Unidades.
    {
      float dc = 8 + scEstaUnidad.mod_CarPoder; 
      VFXAplicar(uni.gameObject);
      if (NIVEL > 2) { dc += 1; }

      if (uni.TiradaSalvacion(uni.mod_TSFortaleza, dc))
      {

        //Agrega la reacción 
        ReaccionSifonArcano reaccion = new ReaccionSifonArcano();
        reaccion.variableUnidad = scEstaUnidad;
        reaccion.NIVEL = NIVEL;
        reaccion.nombre = "Sifón Arcano";
        reaccion.variableUnidad = scEstaUnidad;
        ReaccionSifonArcano reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, uni.gameObject);


      }
                        
      }
     
    }
    
       void VFXAplicar(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_SifonArcano");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---

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

        if (c.Presente.GetComponent<Unidad>() != null)
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
