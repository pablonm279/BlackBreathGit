using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAcrobatico : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_Acrobatico");
      ActualizarDescripcion();

      IDenClase = 2;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {

       if(NIVEL<2)
       {
         txtDescripcion = "<color=#5dade2><b>Acrobatico I</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Recibe 1 Evasión al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel:+1 TS Reflejos</color>\n\n";
            }
          }
        }
      
       }
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Acrobatico II</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Recibe 1 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";
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
         txtDescripcion = "<color=#5dade2><b>Acrobatico III</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Recibe 2 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";
      
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Evasión</color>\n\n";
         txtDescripcion += "<i>(Pasiva) Recibe 1 Evasión al comienzo de cada combate.</i>\n\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Evasión</color>\n\n";
         txtDescripcion += "<i>(Pasiva) Recibe 3 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";
       }
       if(NIVEL==5)
       {
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 Evasión</color>\n\n";
         txtDescripcion += "<i>(Pasiva) Recibe 3 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";
       }

    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
