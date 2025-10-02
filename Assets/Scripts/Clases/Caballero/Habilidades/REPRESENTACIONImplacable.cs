using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONImplacable : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_Implacable");
      IDenClase = 9;
      ActualizarDescripcion();
       
 

    }

   public override void ActualizarDescripcion()
   {
    if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Implacable I</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +3 Fuerza, +20% Daño, +3 TS Mental. Dura 2 Turnos. 1 vez.</i>\n\n";

        
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Fuerza +2 Tirada Salvación Fortaleza al efecto</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Implacable II</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fuerza, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura 2 Turnos. 1 vez.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Rango Crítico</color>\n\n";
          }
          }
        }
       
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Implacable III</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fuerza, +1 Rango Crítico, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura 2 Turnos. 1 vez.</i>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: -1 Valentía Máxima +1 Uso de la Habilidad</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 Turno de duración</color>\n";
          }
          }
        }
      
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Implacable IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +1 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fuerza, +1 Rango Crítico, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura 2 Turnos. 2 veces.</i>\n\n";
       }
       if(NIVEL==5)
       {
        txtDescripcion = "<color=#5dade2><b>Implacable IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fuerza, +1 Rango Crítico, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura  Turnos. 1 vez.</i>\n\n";
       
       }

   }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
