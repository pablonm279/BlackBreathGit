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
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Pilares de Luz I</b></color>\n\n"; 
       
        txtDescripcion += "<i>Convoca dos pilares de luz que obstaculizan enemigos y hacen daño divino a quienes los ataquen.</i>\n";
        txtDescripcion += "<i>Pilar: Vida: 20 -- Daño: 1d8 Divino + Poder, doble a No-Muertos y Etéreos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Convoca un pilar en la casilla seleccionada y uno en una casilla abajo, si está ocupada, entonces en la de arriba.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +5 Vida</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Pilares de Luz II</b></color>\n\n"; 
       
        txtDescripcion += "<i>Convoca dos pilares de luz que obstaculizan enemigos y hacen daño divino a quienes los ataquen.</i>\n";
        txtDescripcion += "<i>Pilar: Vida: 25 -- Daño: 1d8 Divino + Poder, doble a No-Muertos y Etéreos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Convoca un pilar en la casilla seleccionada y uno en una casilla abajo, si está ocupada, entonces en la de arriba.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +3 Daño</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Pilares de Luz III</b></color>\n\n"; 
       
        txtDescripcion += "<i>Convoca dos pilares de luz que obstaculizan enemigos y hacen daño divino a quienes los ataquen.</i>\n";
        txtDescripcion += "<i>Pilar: Vida: 25 -- Daño: 1d8+3 Divino + Poder, doble a No-Muertos y Etéreos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Convoca un pilar en la casilla seleccionada y uno en una casilla abajo, si está ocupada, entonces en la de arriba.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: Obtienen 3 de Resistencia al Daño. </color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 Pilar. </color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Pilares de Luz IV a</b></color>\n\n"; 
       
        txtDescripcion += "<i>Convoca dos pilares de luz que obstaculizan enemigos y hacen daño divino a quienes los ataquen.</i>\n";
        txtDescripcion += "<i>Pilar: Vida: 25 y 3 de Resistencia al daño.-- Daño: 1d8+3 Divino + Poder, doble a No-Muertos y Etéreos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Convoca un pilar en la casilla seleccionada y uno en una casilla abajo, si está ocupada, entonces en la de arriba.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Pilares de Luz IV b</b></color>\n\n"; 
       
        txtDescripcion += "<i>Convoca tres pilares de luz que obstaculizan enemigos y hacen daño divino a quienes los ataquen.</i>\n";
        txtDescripcion += "<i>Pilar: Vida: 25.-- Daño: 1d8+3 Divino + Poder, doble a No-Muertos y Etéreos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Convoca un pilar en la casilla seleccionada y uno en las casillas de arriba y abajo (si están desocupadas).</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";
       }
    if (TRADU.i.nIdioma == 2) // English translation
    {
      if (NIVEL < 2)
      {
        txtDescripcion = "<color=#5dade2><b>Pillars of Light I</b></color>\n\n";
        txtDescripcion += "<i>Summons two pillars of light that block enemies and deal divine damage to those who attack them.</i>\n";
        txtDescripcion += "<i>Pillar: HP: 20 -- Damage: 1d8 Divine + Power, double to Undead and Ethereal.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Summons a pillar on the selected tile and one on the tile below, if occupied then on the tile above.</b></color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n</color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: +5 HP</color>\n\n";
            }
          }
        }
      }
      if (NIVEL == 2)
      {
        txtDescripcion = "<color=#5dade2><b>Pillars of Light II</b></color>\n\n";
        txtDescripcion += "<i>Summons two pillars of light that block enemies and deal divine damage to those who attack them.</i>\n";
        txtDescripcion += "<i>Pillar: HP: 25 -- Damage: 1d8 Divine + Power, double to Undead and Ethereal.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Summons a pillar on the selected tile and one on the tile below, if occupied then on the tile above.</b></color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n</color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: +3 Damage</color>\n\n";
            }
          }
        }
      }
      if (NIVEL == 3)
      {
        txtDescripcion = "<color=#5dade2><b>Pillars of Light III</b></color>\n\n";
        txtDescripcion += "<i>Summons two pillars of light that block enemies and deal divine damage to those who attack them.</i>\n";
        txtDescripcion += "<i>Pillar: HP: 25 -- Damage: 1d8+3 Divine + Power, double to Undead and Ethereal.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Summons a pillar on the selected tile and one on the tile below, if occupied then on the tile above.</b></color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n</color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Option A: Gain 3 Damage Resistance.</color>\n";
              txtDescripcion += $"<color=#dfea02>-Option B: +1 Pillar.</color>\n";
            }
          }
        }
      }
      if (NIVEL == 4)
      {
        txtDescripcion = "<color=#5dade2><b>Pillars of Light IV a</b></color>\n\n";
        txtDescripcion += "<i>Summons two pillars of light that block enemies and deal divine damage to those who attack them.</i>\n";
        txtDescripcion += "<i>Pillar: HP: 25 and 3 Damage Resistance.-- Damage: 1d8+3 Divine + Power, double to Undead and Ethereal.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Summons a pillar on the selected tile and one on the tile below, if occupied then on the tile above.</b></color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n</color>\n\n";
      }
      if (NIVEL == 5)
      {
        txtDescripcion = "<color=#5dade2><b>Pillars of Light IV b</b></color>\n\n";
        txtDescripcion += "<i>Summons three pillars of light that block enemies and deal divine damage to those who attack them.</i>\n";
        txtDescripcion += "<i>Pillar: HP: 25.-- Damage: 1d8+3 Divine + Power, double to Undead and Ethereal.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Summons a pillar on the selected tile and one on the tiles above and below (if unoccupied).</b></color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM} \n</color>\n\n";
      }
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
