using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONMaestriaEspadaCorta : Habilidad
{
   

    
     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_MaestriaEspadaCorta");
      ActualizarDescripcion();
      IDenClase = 2;
      
    }


  public override void ActualizarDescripcion()
  {

    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Maestría con Espada Corta I</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque y +2 Daño Cortante a los ataques con la Espada Corta.</i>\n\n";

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
      txtDescripcion = "<color=#5dade2><b>Maestría con Espada Corta II</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +1 Rango crítico y +2 Daño Cortante a los ataques con la Espada Corta.</i>\n\n";

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
      txtDescripcion = "<color=#5dade2><b>Maestría con Espada Corta III</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +1 Rango crítico y +2 Daño Cortante a los ataques con la Espada Corta. Además cuestan 1 AP menos.</i>\n\n";



      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: +1 Rango Crítico +2 Daño</color>\n\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: +1 Ataque +2 Daño</color>\n";
          }
        }
      }
    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Maestría con Espada Corta IVa</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +1 Ataque, +2 Rango crítico y +4 Daño Cortante a los ataques con la Espada Corta. Además cuestan 1 AP menos.</i>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Maestría con Espada Corta IVb</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Agrega +2 Ataque, +1 Rango crítico y +4 Daño Cortante a los ataques con la Espada Corta. Además cuestan 1 AP menos.</i>\n\n";
    }
       

      if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Short Sword Mastery I</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack and +2 Slashing Damage to Short Sword attacks.</i>\n\n";

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
          txtDescripcion = "<color=#5dade2><b>Short Sword Mastery II</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack, +1 Critical Range and +2 Slashing Damage to Short Sword attacks.</i>\n\n";

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
          txtDescripcion = "<color=#5dade2><b>Short Sword Mastery III</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack, +1 Critical Range and +2 Slashing Damage to Short Sword attacks. Also costs 1 less AP.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: +1 Critical Range +2 Damage</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Option B: +1 Attack +2 Damage</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Short Sword Mastery IVa</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +1 Attack, +2 Critical Range and +4 Slashing Damage to Short Sword attacks. Also costs 1 less AP.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Short Sword Mastery IVb</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Adds +2 Attack, +1 Critical Range and +4 Slashing Damage to Short Sword attacks. Also costs 1 less AP.</i>\n\n";
        }
      }

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
