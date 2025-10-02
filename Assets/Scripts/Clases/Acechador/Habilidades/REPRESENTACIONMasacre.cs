using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONMasacre : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_Masacre");
      ActualizarDescripcion();
      IDenClase = 10;
      
    }

  
    public override void  ActualizarDescripcion()
    {

       if(NIVEL<2)
       {
         txtDescripcion = "<color=#5dade2><b>Masacre I</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Al matar a un enemigo el personaje obtiene +2 AP y +10% daño por el turno. Además los enemigos deben superar una TS Mental vs DC 5+Agilidad o ser Aterrorizados.</i>\n\n";

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
         txtDescripcion = "<color=#5dade2><b>Masacre II</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Al matar a un enemigo el personaje obtiene +2 AP y +10% daño por el turno. Además los enemigos deben superar una TS Mental vs DC 6+Agilidad o ser Aterrorizados.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +5% Daño por el Turno</color>\n\n";
          }
        }
      }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Masacre III</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Al matar a un enemigo el personaje obtiene +2 AP y +15% daño por el turno. Además los enemigos deben superar una TS Mental vs DC 6+Agilidad o ser Aterrorizados.</i>\n\n";

     
     
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: -2 AP a Enemigos por Aterrorizado.</color>\n\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Gana +1 AP al matar a un enemigo.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Masacre IVa</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Al matar a un enemigo el personaje obtiene +2 AP y +15% daño por el turno. Además los enemigos deben superar una TS Mental vs DC 6+Agilidad o ser Aterrorizados II.</i>\n\n";
       }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Masacre IVb</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Al matar a un enemigo el personaje obtiene +3 AP y +15% daño por el turno. Además los enemigos deben superar una TS Mental vs DC 6+Agilidad o ser Aterrorizados.</i>\n\n";
       }

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
