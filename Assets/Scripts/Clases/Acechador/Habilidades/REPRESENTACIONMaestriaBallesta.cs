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


  public override void ActualizarDescripcion()
  {

    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano I</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque y +2 Daño Perforante a los ataques con la Ballesta de Mano.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Rango Crítico</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
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
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano III</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +1 Rango Crítico y +2 Daño Perforante a los ataques con la Ballesta de Mano. Además cuestan 1 AP menos.</i>\n\n";


      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: +1 Alcance</color>\n\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: Remueve Cooldown</color>\n";
          }
        }
      }
    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano IVa</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Alcance, +1 Ataque, +1 Rango Crítico y +2 Daño Perforante a los ataques con la Ballesta de Mano. Además cuestan 1 AP menos.</i>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Maestría con Ballesta de Mano IVb</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +1 Rango Crítico y +2 Daño Perforante a los ataques con la Ballesta de Mano. Además cuestan 1 AP menos y no tiene Cooldown.</i>\n\n";
    }
      

      if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Hand Crossbow Mastery I</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack and +2 Piercing Damage to Hand Crossbow attacks.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Critical Range</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Hand Crossbow Mastery II</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack, +1 Critical Range and +2 Piercing Damage to Hand Crossbow attacks.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: -1 AP Cost</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Hand Crossbow Mastery III</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack, +1 Critical Range and +2 Piercing Damage to Hand Crossbow attacks. Also costs 1 less AP.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: +1 Range</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Option B: Removes Cooldown</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Hand Crossbow Mastery IVa</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Range, +1 Attack, +1 Critical Range and +2 Piercing Damage to Hand Crossbow attacks. Also costs 1 less AP.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Hand Crossbow Mastery IVb</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack, +1 Critical Range and +2 Piercing Damage to Hand Crossbow attacks. Also costs 1 less AP and has no Cooldown.</i>\n\n";
        }
      }

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
