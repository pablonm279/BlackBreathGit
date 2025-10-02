using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONEcosDivinos : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_EcosDivinos");
      ActualizarDescripcion();
      IDenClase = 2;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
        if(NIVEL<2)
       {
         txtDescripcion = "<color=#5dade2><b>Ecos Divinos I</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
         txtDescripcion += "<i>A aliados: Cura 1d10 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
         txtDescripcion += "<i>A enemigos: Causa 1d10 daño Divino.</i>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño y Curación</color>\n\n";
            }
          }
        }
      
       }
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Ecos Divinos II</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
         txtDescripcion += "<i>A aliados: Cura 1d10+2 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
         txtDescripcion += "<i>A enemigos: Causa 1d10+2 daño Divino.</i>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Daño y Curación</color>\n\n";
            }
          }
        }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Ecos Divinos III</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
         txtDescripcion += "<i>A aliados: Cura 1d10+4 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
         txtDescripcion += "<i>A enemigos: Causa 1d10+4 daño Divino.</i>\n\n";
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +5 Curación.</color>\n\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +5 Daño.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Ecos Divinos IV a</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
         txtDescripcion += "<i>A aliados: Cura 1d10+9 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
         txtDescripcion += "<i>A enemigos: Causa 1d10+4 daño Divino.</i>\n\n";
       }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Ecos Divinos IV b</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Cada turno genera un Eco divino en cualquier lado de la batalla, dañando a los enemigos o curando a aliados que los tocan.</i>\n\n";
         txtDescripcion += "<i>A aliados: Cura 1d10+4 y suma +1 Valentía. Si es la Purificadora, gana 1 Fervor. Curación mágica.</i>\n\n";
         txtDescripcion += "<i>A enemigos: Causa 1d10+9 daño Divino.</i>\n\n";
       }
      }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
