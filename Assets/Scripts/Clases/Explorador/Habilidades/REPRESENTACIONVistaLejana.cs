using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONVistaLejana : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_VistaLejana");
      ActualizarDescripcion();

      IDenClase = 1;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {

       if(NIVEL<2)
       {
         txtDescripcion = "<color=#5dade2><b>Vista Lejana I</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Si el Explorador arranca su turno en la última columna, recibe +1 Ataque y +10% Daño.</i>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +5% Daño</color>\n\n";
            }
          }
        }
      
       }
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Vista Lejana II</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Si el Explorador arranca su turno en la última columna, recibe +1 Ataque y +15% Daño.</i>\n\n";
           if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Ataque</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Vista Lejana III</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Si el Explorador arranca su turno en la última columna, recibe +2 Ataque y +15% Daño.</i>\n\n";
      
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Rango de Dado Crítico</color>\n\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +1 Defensa</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Vista Lejana IV a</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Si el Explorador arranca su turno en la última columna, recibe +2 Ataque, +1 Rango de Dado Crítico y +15% Daño.</i>\n\n";
       }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Vista Lejana IV b</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Si el Explorador arranca su turno en la última columna, recibe +2 Ataque, +1 Defensa y +15% Daño.</i>\n\n";
       }

    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
