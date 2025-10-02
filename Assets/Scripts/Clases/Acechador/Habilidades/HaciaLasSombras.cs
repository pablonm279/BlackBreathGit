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
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Hacia Las Sombras I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Acechador se desvanece en las sombras para buscar un nuevo enfoque y recuperarse.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Se mueve a la casilla seleccionada, gana 2 Evasión, Escondido II, remueve debuffs y termina el turno.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 Enfriamiento</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Hacia Las Sombras II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Acechador se desvanece en las sombras para buscar un nuevo enfoque y recuperarse.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Se mueve a la casilla seleccionada, gana 2 Evasión, Escondido II, remueve debuffs y termina el turno.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
           if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
         {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Evasión</color>\n\n";
          }
         }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Hacia Las Sombras III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Acechador se desvanece en las sombras para buscar un nuevo enfoque y recuperarse.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Se mueve a la casilla seleccionada, gana 3 Evasión, Escondido II, remueve debuffs y termina el turno.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";
        
         if (EsEscenaCampaña())
        {
           if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
           {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: No termina turno.</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: No cuesta Valentía.</color>\n";
          }
           }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Hacia Las Sombras IVa</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Acechador se desvanece en las sombras para buscar un nuevo enfoque y recuperarse.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Se mueve a la casilla seleccionada, gana 2 Evasión, Escondido II, remueve debuffs.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM} </color>\n\n";
        }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Hacia Las Sombras IVb</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Acechador se desvanece en las sombras para buscar un nuevo enfoque y recuperarse.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Se mueve a la casilla seleccionada, gana 2 Evasión, Escondido II, remueve debuffs y termina el turno.</color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP}- Termina turno \n- Costo Val: {costoPM-1} </color>\n\n";
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
