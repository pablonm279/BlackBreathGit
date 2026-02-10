using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PilaresDeLuz : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
    public override void  Awake()
    {
      nombre = "Pilares De Luz";
      IDenClase = 5;
      costoAP = 5;
      if(NIVEL > 2){costoAP--;}
      if(NIVEL == 5){costoAP--;}
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 2;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 7;
      bAfectaObstaculos = false;
      poneTrampas = false;
      poneObstaculo = true;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_PilaresDeLuz");
      ActualizarDescripcion();
    }
        public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int vidaPilar = NIVEL > 1 ? 25 : 20;
      int resistenciaDanio = NIVEL == 4 ? 3 : 0;
      int cantidadPilares = NIVEL == 5 ? 3 : 2;
      int duracionTurnos = NIVEL == 5 ? 4 : 3;
      int bonusDanio = NIVEL > 2 ? 3 : 0;

      string tituloEs = "Pilares de Luz I";
      string tituloEn = "Pillars of Light I";
      if (NIVEL == 2) { tituloEs = "Pilares de Luz II"; tituloEn = "Pillars of Light II"; }
      if (NIVEL == 3) { tituloEs = "Pilares de Luz III"; tituloEn = "Pillars of Light III"; }
      if (NIVEL == 4) { tituloEs = "Pilares de Luz IV a"; tituloEn = "Pillars of Light IV a"; }
      if (NIVEL == 5) { tituloEs = "Pilares de Luz IV b"; tituloEn = "Pillars of Light IV b"; }

      string danioPilarEs = bonusDanio > 0
        ? $"1d6 + {bonusDanio} + <color=#ea0606>Poder ({poderActual})</color>"
        : $"1d6 + <color=#ea0606>Poder ({poderActual})</color>";
      string danioPilarEn = bonusDanio > 0
        ? $"1d6 + {bonusDanio} + <color=#ea0606>Power ({poderActual})</color>"
        : $"1d6 + <color=#ea0606>Power ({poderActual})</color>";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (3 range)\n";
        cuerpo += "<b>Target:</b> 1 tile in range\n";
        cuerpo += $"<b>Summon:</b> {cantidadPilares} pillars (selected tile and adjacent tiles in the same column if free)\n";
        cuerpo += $"<b>Pillar Stats:</b> HP {vidaPilar}";
        if (resistenciaDanio > 0)
        {
          cuerpo += $", Damage Resistance {resistenciaDanio}";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Retaliation:</b> {danioPilarEn} | <b>Type:</b> Divine (x2 vs Undead/Ethereal)\n";
        cuerpo += $"<b>Duration:</b> {duracionTurnos} turns";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (3 alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 casilla en rango\n";
        cuerpo += $"<b>Invocacion:</b> {cantidadPilares} pilares (casilla seleccionada y casillas adyacentes en la misma columna si estan libres)\n";
        cuerpo += $"<b>Stats del pilar:</b> Vida {vidaPilar}";
        if (resistenciaDanio > 0)
        {
          cuerpo += $", Resistencia al dano {resistenciaDanio}";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Contraataque:</b> {danioPilarEs} | <b>Tipo:</b> Divino (x2 vs Nomuerto/Etereo)\n";
        cuerpo += $"<b>Duracion:</b> {duracionTurnos} turnos";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Val Cost: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Val: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "Creates holy obstacles that punish attackers with divine retaliation."
          : "Crea obstaculos sagrados que castigan a quienes los ataquen con represalia divina.",
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5 pillar HP.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +3 pillar retaliation damage.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+3 Damage Resistance) or Option B (+1 pillar).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5 Vida de pilar.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +3 danio de contraataque del pilar.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+3 Resistencia al dano) u Opcion B (+1 pilar).</color>"; }
      }
    }
    void Start()
    {
      
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
      
       
     GameObject obst1 = Instantiate(BattleManager.Instance.contenedorPrefabs.PurificadoraPilarDeLuz);
     obst1.GetComponent<PilarDeLuz>().oName = "Pilar De Luz";
     obst1.GetComponent<PilarDeLuz>().hpMax = 20.0f;
     if(NIVEL > 1){ obst1.GetComponent<PilarDeLuz>().hpMax += 5;}
     obst1.GetComponent<PilarDeLuz>().iDureza = 0.0f;
     if(NIVEL == 4){ obst1.GetComponent<PilarDeLuz>().iDureza += 3;}
     obst1.GetComponent<PilarDeLuz>().hpCurr =  obst1.GetComponent<PilarDeLuz>().hpMax;
     obst1.GetComponent<PilarDeLuz>().bPermiteAtacarDetras = true;
     obst1.GetComponent<PilarDeLuz>().NIVEL = NIVEL;
     obst1.GetComponent<PilarDeLuz>().scCreador = Usuario.GetComponent<ClasePurificadora>();
     obst1.GetComponent<PilarDeLuz>().intDuracionTurnos = 3;
     if(NIVEL == 5){ obst1.GetComponent<PilarDeLuz>().intDuracionTurnos += 1;}


     cas.PonerObjetoEnCasilla(obst1);
     int cantidadQuedan = 1;
     if(NIVEL == 5){ cantidadQuedan += 1;}
     foreach(Casilla ady in cas.ObtenerCasillasAdyacentesEnColumna())
     {
      if(ady.Presente == null && cantidadQuedan > 0)
      { 
        cantidadQuedan--;
        GameObject obst2 = Instantiate(BattleManager.Instance.contenedorPrefabs.PurificadoraPilarDeLuz);
        obst2.GetComponent<PilarDeLuz>().oName = "Pilar De Luz";
        obst2.GetComponent<PilarDeLuz>().hpMax = 20.0f;
        if(NIVEL > 1){ obst2.GetComponent<PilarDeLuz>().hpMax += 5;}
        obst2.GetComponent<PilarDeLuz>().iDureza = 0.0f;
        obst2.GetComponent<PilarDeLuz>().hpCurr =  obst2.GetComponent<PilarDeLuz>().hpMax;
        if(NIVEL == 4){ obst2.GetComponent<PilarDeLuz>().iDureza += 3;}
        obst2.GetComponent<PilarDeLuz>().bPermiteAtacarDetras = true;
        obst2.GetComponent<PilarDeLuz>().NIVEL = NIVEL;
        obst2.GetComponent<PilarDeLuz>().scCreador = Usuario.GetComponent<ClasePurificadora>();
        obst2.GetComponent<PilarDeLuz>().intDuracionTurnos = 3;
        if(NIVEL == 5){ obst2.GetComponent<PilarDeLuz>().intDuracionTurnos += 1;}
        ady.PonerObjetoEnCasilla(obst2);
      }
     }
     

     
       BattleManager.Instance.HabilidadActiva = null;// desactiva la habilidad activa, para que no se pueda usar de nuevo
    }
    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
     
     
      
      //Casillas Alrededor al origen
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(3);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
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
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
        }

      }
    
         
    }

   
    

 
}
