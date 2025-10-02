using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONReconocimiento : Habilidad
{
   

    
     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_Reconocimiento");
      ActualizarDescripcion();

      IDenClase = 9;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {

       if(NIVEL<2)
       {
         txtDescripcion = "<color=#5dade2><b>Reconocimiento I</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) El Explorador otorga ventajas de terreno a sus aliados.</i>\n\n";
         txtDescripcion += "<i>+1 AP en el primer turno.</i>\n";
         txtDescripcion += "<i>Retrasa 1 turno refuerzos enemigos</i>\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Buff a aliados: +1 Iniciativa</color>\n\n";
            }
          }
        }
      
       }
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Reconocimiento II</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) El Explorador otorga ventajas de terreno a sus aliados.</i>\n\n";
         txtDescripcion += "<i>+1 AP y +1 Iniciativa en el primer turno.</i>\n";
         txtDescripcion += "<i>Retrasa 1 turno refuerzos enemigos</i>\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Buff a aliados: +1 Iniciativa</color>\n\n";
            }
          }
        }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Reconocimiento III</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) El Explorador otorga ventajas de terreno a sus aliados.</i>\n\n";
         txtDescripcion += "<i>+1 AP y +2 Iniciativa en el primer turno.</i>\n";
         txtDescripcion += "<i>Retrasa 1 turno refuerzos enemigos</i>\n";

       
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: Buff a aliados: +1 AP extra</color>\n\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 Retraso extra a refuerzos enemigos</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Reconocimiento IV a</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) El Explorador otorga ventajas de terreno a sus aliados.</i>\n\n";
         txtDescripcion += "<i>+2 AP y +2 Iniciativa en el primer turno.</i>\n";
         txtDescripcion += "<i>Retrasa 1 turno refuerzos enemigos</i>\n";
     }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Reconocimiento IV b</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) El Explorador otorga ventajas de terreno a sus aliados.</i>\n\n";
         txtDescripcion += "<i>+1 AP y +2 Iniciativa en el primer turno.</i>\n";
         txtDescripcion += "<i>Retrasa 2 turno refuerzos enemigos</i>\n";       }

    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
