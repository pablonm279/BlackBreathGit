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

  public override void ActualizarDescripcion()
  {

    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Acrobatico I</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Recibe 1 Evasión al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel:+1 TS Reflejos</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Acrobatico II</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Recibe 1 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";
      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Evasión</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Acrobatico III</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Recibe 2 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: +1 Evasión</color>\n\n";
            txtDescripcion += "<i>(Pasiva) Recibe 1 Evasión al comienzo de cada combate.</i>\n\n";
          }
        }
      }
    }
    if (NIVEL == 4)
    {
      txtDescripcion += $"<color=#dfea02>-Opción A: +1 Evasión</color>\n\n";
      txtDescripcion += "<i>(Pasiva) Recibe 3 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion += $"<color=#dfea02>-Opción B: +1 Evasión</color>\n\n";
      txtDescripcion += "<i>(Pasiva) Recibe 3 Evasión y 1 Reflejos al comienzo de cada combate. La evasión se suma a la Defensa y se va al recibir daño.</i>\n\n";
    }
       
      if (TRADU.i.nIdioma == 2) // agrega la traducción a inglés
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Acrobatic I</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Gains 1 Evasion at the start of each combat. Evasion is added to Defense and is lost when taking damage.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Reflex Save</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Acrobatic II</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Gains 1 Evasion and 1 Reflex at the start of each combat. Evasion is added to Defense and is lost when taking damage.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Evasion</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Acrobatic III</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Gains 2 Evasion and 1 Reflex at the start of each combat. Evasion is added to Defense and is lost when taking damage.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: +1 Evasion</color>\n\n";
                txtDescripcion += "<i>(Passive) Gains 1 Evasion at the start of each combat.</i>\n\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion += $"<color=#dfea02>-Option A: +1 Evasion</color>\n\n";
          txtDescripcion += "<i>(Passive) Gains 3 Evasion and 1 Reflex at the start of each combat. Evasion is added to Defense and is lost when taking damage.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion += $"<color=#dfea02>-Option B: +1 Evasion</color>\n\n";
          txtDescripcion += "<i>(Passive) Gains 3 Evasion and 1 Reflex at the start of each combat. Evasion is added to Defense and is lost when taking damage.</i>\n\n";
        }
      }

    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
