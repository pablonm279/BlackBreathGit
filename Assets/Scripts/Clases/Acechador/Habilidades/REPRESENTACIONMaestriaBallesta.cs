using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONMaestriaBallesta : Habilidad
{
   

    
  public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_MaestriaBallesta");
      ActualizarDescripcion();
      IDenClase = 1;
      
    }

  
    public override void  ActualizarDescripcion()
    {

       if(NIVEL<2)
       {
         txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano I</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque y +2 Daño Perforante a los ataques con la Ballesta de Mano.</i>\n\n";

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
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano II</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +1 Rango Crítico y +2 Daño Perforante a los ataques con la Ballesta de Mano.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 Costo AP/color>\n\n";
          }
        }
      }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano III</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +1 Rango Crítico y +2 Daño Perforante a los ataques con la Ballesta de Mano. Además cuestan 1 AP menos.</i>\n\n";

     
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Alcance</color>\n\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Remueve Cooldown</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano IVa</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Agrega +1 Alcance, +1 Ataque, +1 Rango Crítico y +2 Daño Perforante a los ataques con la Ballesta de Mano. Además cuestan 1 AP menos.</i>\n\n";
       }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano IVb</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +1 Rango Crítico y +2 Daño Perforante a los ataques con la Ballesta de Mano. Además cuestan 1 AP menos y no tiene Cooldown.</i>\n\n";
      }

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
