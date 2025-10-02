using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class CastigaraLosMalvados : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
      public override void  Awake()
    {
      nombre = "Castigar a los Malvados";
      IDenClase = 8;
      costoAP = 2;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Purificadora_CastigarMalvados");
     
    }

   public override void ActualizarDescripcion()
   {
       if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Castigar a los Malvados I</b></color>\n\n"; 
       
        txtDescripcion += "<i>Si el objetivo daña a un personaje aliado, debe superar una tirada Mental o sufrir daño divino y perder sus AP restantes.</i>\n";
        txtDescripcion += $"<i>Tirada Salvación: DC 10+ Poder. Dura 2 usos o hasta salvarse. </i>\n";
        txtDescripcion += "<i>Daño: 1d6+ 1/3 daño hecho al personaje aliado.</i>\n";
      
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";


         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 DC</color>\n\n";
          }
          }
        }
   
       }
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Castigar a los Malvados II</b></color>\n\n"; 
       
        txtDescripcion += "<i>Si el objetivo daña a un personaje aliado, debe superar una tirada Mental o sufrir daño divino y perder sus AP restantes.</i>\n";
        txtDescripcion += $"<i>Tirada Salvación: DC 11+ Poder. Dura 2 usos o hasta salvarse. </i>\n";
        txtDescripcion += "<i>Daño: 1d6+ 1/3 daño hecho al personaje aliado.</i>\n";
      
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";
 if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 Cooldown</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Castigar a los Malvados I</b></color>\n\n"; 
       
        txtDescripcion += "<i>Si el objetivo daña a un personaje aliado, debe superar una tirada Mental o sufrir daño divino y perder sus AP restantes.</i>\n";
        txtDescripcion += $"<i>Tirada Salvación: DC 11+ Poder. Dura 2 usos o hasta salvarse. </i>\n";
        txtDescripcion += "<i>Daño: 1d6+ 1/3 daño hecho al personaje aliado.</i>\n";
      
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";

          if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Uso.</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: aumenta a +1/2 daño hecho al personaje aliado.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
       txtDescripcion = "<color=#5dade2><b>Castigar a los Malvados IVa</b></color>\n\n"; 
       
        txtDescripcion += "<i>Si el objetivo daña a un personaje aliado, debe superar una tirada Mental o sufrir daño divino y perder sus AP restantes.</i>\n";
        txtDescripcion += $"<i>Tirada Salvación: DC 11+ Poder. Dura 3 usos o hasta salvarse. </i>\n";
        txtDescripcion += "<i>Daño: 1d6+ 1/3 daño hecho al personaje aliado.</i>\n";
      
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";
    }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Castigar a los Malvados IVb</b></color>\n\n"; 
       
        txtDescripcion += "<i>Si el objetivo daña a un personaje aliado, debe superar una tirada Mental o sufrir daño divino y perder sus AP restantes.</i>\n";
        txtDescripcion += $"<i>Tirada Salvación: DC 11+ Poder. Dura 2 usos o hasta salvarse. </i>\n";
        txtDescripcion += "<i>Daño: 1d6+ 1/2 daño hecho al personaje aliado.</i>\n";
      
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP}\n- Costo Val: {costoPM} \n</color>\n\n";
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

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {

      Unidad objetivo = (Unidad)obj;

      BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre} en {objetivo.uNombre}");
      VFXAplicar(objetivo.gameObject);
      //Agrega la reacción 
      ReaccionCastigarMalvados reaccion = new ReaccionCastigarMalvados();
      reaccion.variableUnidad = scEstaUnidad;
      reaccion.NIVEL = NIVEL;
      reaccion.nombre = "Castigar a los Malvados";
      ReaccionCastigarMalvados reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

      objetivo.GenerarTextoFlotante("Castigar a los Malvados", Color.yellow);
      }
     
    }
    
  
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CastigarMalvados");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
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
           
           if(c.Presente.GetComponent<Unidad>() != null)
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
