using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Instatransporte : Habilidad
{


  [SerializeField] private GameObject VFXenObjetivo;

    public override void  Awake()
    {
    nombre = "Instatransporte";
    IDenClase = 3;
    costoAP = 1;
    costoPM = 1;
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    esZonal = false;
    enArea = 0;
    esforzable = 0;
    esCargable = false;
    esMelee = false;
    esHostil = false;
    cooldownMax = 5;
    if (NIVEL > 1) { cooldownMax--; }
    bAfectaObstaculos = false;
    poneTrampas = true;

    imHab = Resources.Load<Sprite>("imHab/Canalizador_Instatransporte");




  }

  public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Instatransporte I</b></color>\n\n";
      txtDescripcion += "<i>Desapareciendo en un destello de energía, se teletransporta a otra casilla, dejando un residuo energético en las casillas adyacentes.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Movimiento instantáneo.\n- Deja <b>Residuos Energéticos</b> en cruz adyacente.\n- Ignora y destruye trampas en la casilla de destino.\n- Gana <color=#44d3ec>+1 Evasión</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Alcance: 3 \n- Cooldown: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>- Próximo Nivel: -1 Cooldown</color>\n\n";
          }
        }
      }
    }

    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Instatransporte II</b></color>\n\n";
      txtDescripcion += "<i>Desapareciendo en un destello de energía, se teletransporta a otra casilla, dejando un residuo energético en las casillas adyacentes.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Movimiento instantáneo.\n- Deja <b>Residuos Energéticos</b> en cruz adyacente.\n- Ignora y destruye trampas en la casilla de destino.\n- Gana <color=#44d3ec>+1 Evasión</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Alcance: 3 \n- Cooldown: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 Alcance</color>\n\n";
          }
        }
      }
    }

    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Instatransporte III</b></color>\n\n";
      txtDescripcion += "<i>Desapareciendo en un destello de energía, se teletransporta a otra casilla, dejando un residuo energético en las casillas adyacentes.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Movimiento instantáneo.\n- Deja <b>Residuos Energéticos</b> en cruz adyacente.\n- Ignora y destruye trampas en la casilla de destino.\n- Gana <color=#44d3ec>+1 Evasión</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Alcance: 4 \n- Cooldown: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>- Opción A: Deja residuos en todo alrededor</color>\n";
            txtDescripcion += $"<color=#dfea02>- Opción B: +1 Evasión</color>\n\n";
          }
        }
      }
    }

    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Instatransporte IV a</b></color>\n\n";
      txtDescripcion += "<i>Desapareciendo en un destello de energía, se teletransporta a otra casilla, dejando un residuo energético en las casillas alrededor.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Movimiento instantáneo.\n- Deja <b>Residuos Energéticos</b> en todo alrededor.\n- Ignora y destruye trampas en la casilla de destino.\n- Gana <color=#44d3ec>+1 Evasión</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Alcance: 4 \n- Cooldown: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";
    }

    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Instatransporte IV b</b></color>\n\n";
      txtDescripcion += "<i>Desapareciendo en un destello de energía, se teletransporta a otra casilla, dejando un residuo energético en las casillas adyacentes.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>- Movimiento instantáneo.\n- Deja <b>Residuos Energéticos</b> en cruz adyacente.\n- Ignora y destruye trampas en la casilla de destino.\n- Gana <color=#44d3ec>+2 Evasión</color> al usarla.</color>\n\n";
      txtDescripcion += $"<color=#44d3ec>- Alcance: 4 \n- Cooldown: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM}</color>\n\n";
    }

    if (TRADU.i.nIdioma == 2) // English translation
    {
      if (NIVEL < 2)
      {
        txtDescripcion = "<color=#5dade2><b>Instatransport I</b></color>\n\n";
        txtDescripcion += "<i>Disappearing in a flash of energy, instantly teleports to another tile, leaving an energy residue on adjacent tiles.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>- Instant movement.\n- Leaves <b>Energy Residue</b> on adjacent cross.\n- Ignores and destroys traps on the destination tile.\n- Gains <color=#44d3ec>+1 Evasion</color> when used.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Range: 3 \n- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM}</color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>- Next Level: -1 Cooldown</color>\n\n";
            }
          }
        }
      }

      if (NIVEL == 2)
      {
        txtDescripcion = "<color=#5dade2><b>Instatransport II</b></color>\n\n";
        txtDescripcion += "<i>Disappearing in a flash of energy, instantly teleports to another tile, leaving an energy residue on adjacent tiles.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>- Instant movement.\n- Leaves <b>Energy Residue</b> on adjacent cross.\n- Ignores and destroys traps on the destination tile.\n- Gains <color=#44d3ec>+1 Evasion</color> when used.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Range: 3 \n- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM}</color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>- Next Level: +1 Range</color>\n\n";
            }
          }
        }
      }

      if (NIVEL == 3)
      {
        txtDescripcion = "<color=#5dade2><b>Instatransport III</b></color>\n\n";
        txtDescripcion += "<i>Disappearing in a flash of energy, instantly teleports to another tile, leaving an energy residue on adjacent tiles.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>- Instant movement.\n- Leaves <b>Energy Residue</b> on adjacent cross.\n- Ignores and destroys traps on the destination tile.\n- Gains <color=#44d3ec>+1 Evasion</color> when used.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Range: 4 \n- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM}</color>\n\n";

        if (EsEscenaCampaña())
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>- Option A: Leaves residue all around</color>\n";
              txtDescripcion += $"<color=#dfea02>- Option B: +1 Evasion</color>\n\n";
            }
          }
        }
      }

      if (NIVEL == 4)
      {
        txtDescripcion = "<color=#5dade2><b>Instatransport IV a</b></color>\n\n";
        txtDescripcion += "<i>Disappearing in a flash of energy, instantly teleports to another tile, leaving an energy residue on surrounding tiles.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>- Instant movement.\n- Leaves <b>Energy Residue</b> all around.\n- Ignores and destroys traps on the destination tile.\n- Gains <color=#44d3ec>+1 Evasion</color> when used.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Range: 4 \n- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM}</color>\n\n";
      }

      if (NIVEL == 5)
      {
        txtDescripcion = "<color=#5dade2><b>Instatransport IV b</b></color>\n\n";
        txtDescripcion += "<i>Disappearing in a flash of energy, instantly teleports to another tile, leaving an energy residue on adjacent tiles.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>- Instant movement.\n- Leaves <b>Energy Residue</b> on adjacent cross.\n- Ignores and destroys traps on the destination tile.\n- Gains <color=#44d3ec>+2 Evasion</color> when used.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Range: 4 \n- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Val Cost: {costoPM}</color>\n\n";
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

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());



  }



  public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
  {
    scEstaUnidad.estado_evasion = 1;
    if (NIVEL == 5) { scEstaUnidad.estado_evasion += 1; }
  
    VFXAplicar(scEstaUnidad.gameObject);
    

    Trampa[] trampas = cas.transform.GetComponentsInChildren<Trampa>();
    foreach (Trampa trmp in trampas)
    {
      trmp.DestruirTrampa();

    }

    scEstaUnidad.TeletransportarACasilla(cas);
    
    int alre = 1;
    if (NIVEL == 4) { alre = 2; }
    foreach (Casilla ady in cas.ObtenerCasillasAlrededor(alre))
    {
      ady.AddComponent<ResiduoEnergetico>();
      ady.GetComponent<ResiduoEnergetico>().InicializarCreador(scEstaUnidad, NIVEL);

    }




  }
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Instatransporte");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }

  //Provisorio
  private List<Unidad> lObjetivosPosibles = new List<Unidad>();
  private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();


  private void ObtenerObjetivos()
  {

    lObjetivosPosibles.Clear();
    lCasillasafectadas.Clear();

    List<Casilla> alCasillasafectadas = new List<Casilla>();
    //Casillas Alrededor al origen
    int alre = 3;
    if (NIVEL > 2) { alre++; }
    alCasillasafectadas = Origen.ObtenerCasillasAlrededor(alre);
    alCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear

    foreach (Casilla c in alCasillasafectadas)
    {
      c.ActivarCapaColorAzul();
      if (c.Presente != null)
      {
        continue;
      }

      lCasillasafectadas.Add(c);


    }


  }

}
    

 

   /*  private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();

     int alcance = 3;
     if (NIVEL > 2) { alcance++; }
      //Casillas Alrededor al origen
     List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(alcance);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
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


   
    

 
}*/
