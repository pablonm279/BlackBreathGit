using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Fogata : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
      public override void  Awake()
    {
      nombre = "Fogata";
      IDenClase = 8;
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
      poneTrampas = true;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Explorador_Fogata");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Fogata I</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador prende una fogata en el campo de batalla que usará para encender sus flechas y obtener daño fuego.</i>\n";
        txtDescripcion += "<i>Siempre que el explorador este adyacente (arriba, abajo y a los lados) de la fogata obtiene el buff.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff: +1d6 daño fuego. Dura 3 turnos.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Turno duración</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Fogata II</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador prende una fogata en el campo de batalla que usará para encender sus flechas y obtener daño fuego.</i>\n";
        txtDescripcion += "<i>Siempre que el explorador este adyacente (arriba, abajo y a los lados) de la fogata obtiene el buff.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff: +1d6 daño fuego. Dura 4 turnos.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 costo AP</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Fogata III</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador prende una fogata en el campo de batalla que usará para encender sus flechas y obtener daño fuego.</i>\n";
        txtDescripcion += "<i>Siempre que el explorador este adyacente (arriba, abajo y a los lados) de la fogata obtiene el buff.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff: +1d6 daño fuego. Dura 4 turnos.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP-1} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +0d3 daño fuego. </color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: -1 costo AP. </color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
          txtDescripcion = "<color=#5dade2><b>Fogata IVa</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador prende una fogata en el campo de batalla que usará para encender sus flechas y obtener daño fuego.</i>\n";
        txtDescripcion += "<i>Siempre que el explorador este adyacente (arriba, abajo y a los lados) de la fogata obtiene el buff.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff: +1d9 daño fuego. Dura 4 turnos.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

       }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Fogata IVb</b></color>\n\n"; 
       
        txtDescripcion += "<i>El Explorador prende una fogata en el campo de batalla que usará para encender sus flechas y obtener daño fuego.</i>\n";
        txtDescripcion += "<i>Siempre que el explorador este adyacente (arriba, abajo y a los lados) de la fogata obtiene el buff.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8><b>Buff: +1d9 daño fuego. Dura 4 turnos.</b>  </color>\n\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP-2} \n- Costo Val: {costoPM} \n</color>\n\n";
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
        ClaseExplorador clas = (ClaseExplorador)scEstaUnidad;
        clas.ChequeartieneFogataCerca();
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
    {
       cas.AddComponent<TrampaFogata>();
       cas.GetComponent<TrampaFogata>().Inicializar(NIVEL);
     
   
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
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(1);
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
