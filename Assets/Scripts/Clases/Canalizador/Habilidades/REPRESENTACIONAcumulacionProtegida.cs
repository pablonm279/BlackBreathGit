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

    if (TRADU.i.nIdioma == 2) // English translation
    {
        if (NIVEL < 2)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging I</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy</b> Barrier and <b>+1 to Mental Save</b> until their next turn.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Next Level: +2 Barrier when accumulating Energy.</color>\n\n";
                    }
                }
            }
        }
        if (NIVEL == 2)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging II</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +2</b> Barrier and <b>+1 to Mental Save</b> until their next turn.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Next Level: +1 additional Mental Save.</color>\n\n";
                    }
                }
            }
        }
        if (NIVEL == 3)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging III</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +2</b> Barrier and <b>+2 to Mental Save</b> until their next turn.</color>\n\n";

            if (EsEscenaCampaña())
            {
                if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
                {
                    if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                    {
                        txtDescripcion += $"<color=#dfea02>- Option A: If the Mental Save for concentration is passed, the attacker takes 1d10 Arcane damage.</color>\n";
                        txtDescripcion += $"<color=#dfea02>- Option B: If Charging is successfully completed, gains +1 AP that turn.</color>\n";
                    }
                }
            }
        }
        if (NIVEL == 4)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging IV a</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +6</b> Barrier and <b>+2 to Mental Save</b> until their next turn.</color>\n\n";
        }
        if (NIVEL == 5)
        {
            txtDescripcion = "<color=#5dade2><b>Protected Charging IV b</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Channeler covers their body with protective energy when Charging power.</i>\n\n";
            txtDescripcion += "<color=#c8c8c8>When Charging Energy, gains <b>1 + Power + 3 x Energy +2</b> Barrier and <b>+2 to Mental Save</b> until their next turn.</color>\n\n";
            txtDescripcion += "<color=#c8c8c8>If Charging is successfully completed, gains <b>+1 AP</b> that turn.</color>\n";
        }
    }

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
