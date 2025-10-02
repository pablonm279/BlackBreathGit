using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAuraSagrada : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_Aurasagrada");
      ActualizarDescripcion();
      IDenClase = 1;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
     
        if(NIVEL<2)
       {
         txtDescripcion = "<color=#5dade2><b>Aura Sagrada I</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 1 de Barrera y 1 Bonus de daño divino a Aliados.</i>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Barrera</color>\n\n";
            }
          }
        }
      
       }
       if(NIVEL==2)
       {
         txtDescripcion = "<color=#5dade2><b>Aura Sagrada II</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 2 de Barrera y 1 Bonus de daño Divino a Aliados.</i>\n\n";
           if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Bonus Daño divino</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada III</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";
      
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: Remueve la necesidad de Fervor.</color>\n\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Si tiene 3 o más Fervor, duplica las bonificaciones.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
         txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV a</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";
       }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV b</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva)  Mientras la Purificadora tenga Fervor, da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";
         txtDescripcion += "<i>Si tiene 3 o más Fervor, duplica las bonificaciones.</i>\n\n";
       }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
