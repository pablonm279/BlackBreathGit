using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAcumulacionProtegida : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Canalizador_AcumulacionProtegida");
      ActualizarDescripcion();
      IDenClase = 1;
      
    }

  
    public override void  ActualizarDescripcion()
    {

      if(NIVEL<2)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida I</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Poder + 3 x Energía</b> de Barrera y <b>+1 a TS Mental</b> hasta su siguiente turno.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +2 Barrera al acumular Energía.</color>\n\n";
            }
        }
    }
}
if(NIVEL==2)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida II</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Poder + 3 x Energía +2</b> de Barrera y <b>+1 a TS Mental</b> hasta su siguiente turno.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Próximo Nivel: +1 a TS Mental adicional.</color>\n\n";
            }
        }
    }
}
if(NIVEL==3)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida III</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Poder + 3 x Energía +2</b> de Barrera y <b>+2 a TS Mental</b> hasta su siguiente turno.</color>\n\n";

    if (EsEscenaCampaña())
    {
        if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
        {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
                txtDescripcion += $"<color=#dfea02>- Opción A: Si supera la TS Mental de concentración, el atacante recibe 1d10 daño Arcano.</color>\n";
                txtDescripcion += $"<color=#dfea02>- Opción B: Si completa con éxito la acumulación, obtiene +1 AP ese turno.</color>\n";
            }
        }
    }
}
if(NIVEL==4)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida IV a</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Poder + 3 x Energía +6</b> de Barrera y <b>+2 a TS Mental</b> hasta su siguiente turno.</color>\n\n";
}
if(NIVEL==5)
{
    txtDescripcion = "<color=#5dade2><b>Acumulación Protegida IV b</b></color>\n\n"; 
    txtDescripcion += "<i>(Pasiva) El Canalizador recubre su cuerpo con energía protectora al acumular poder.</i>\n\n";
    txtDescripcion += "<color=#c8c8c8>Al Acumular Energía, obtiene <b>1 + Poder + 3 x Energía +2</b> de Barrera y <b>+2 a TS Mental</b> hasta su siguiente turno.</color>\n\n";
    txtDescripcion += "<color=#c8c8c8>Si completa con éxito la acumulación, obtiene <b>+1 AP</b> ese turno.</color>\n";
}

      

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
