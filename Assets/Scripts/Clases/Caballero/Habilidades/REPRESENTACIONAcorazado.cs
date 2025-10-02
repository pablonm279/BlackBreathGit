using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAcorazado : Habilidad
{
     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_Acorazado");
       ActualizarDescripcion();
      IDenClase = 1;
      
    }

    public override void  ActualizarDescripcion()
    {
      if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Acorazado I</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 4 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 5</color>\n\n";
          }
          }
        }
      
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Acorazado II</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 5 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";
           if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 6</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Acorazado III</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 6 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";
      
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: Solo se pierde 1 de Armadura si el daño físico recibido es mayor a 8</color>\n\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: La Armadura no puede bajar a menos de la mitad del valor de inicio</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Acorazado IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 8 o + daño para que su armadura se reduzca al ser golpeado.</i>\n\n";
       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Acorazado IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva)Su armadura pesada resiste los golpes enemigos más de lo habitual.\nDebe recibir 6 o + daño para que su armadura se reduzca al ser golpeado y no puede bajar a menos de la mitad del valor inicial.</i>\n\n";
       }
 

    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
