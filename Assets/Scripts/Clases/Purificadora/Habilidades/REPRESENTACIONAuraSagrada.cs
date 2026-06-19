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

  public override void ActualizarDescripcion()
  {

    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada I</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 1 de Barrera y 1 Bonus de daño divino a Aliados.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Barrera</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada II</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 2 de Barrera y 1 Bonus de daño Divino a Aliados.</i>\n\n";
      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Bonus Daño divino</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada III</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Mientras la Purificadora tenga Fervor, da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: Remueve la necesidad de Fervor.</color>\n\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: Si tiene 3 o más Fervor, duplica las bonificaciones.</color>\n";
          }
        }
      }
    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV a</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV b</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva)  Mientras la Purificadora tenga Fervor, da 2 de Barrera y 2 Bonus de daño Divino a Aliados.</i>\n\n";
      txtDescripcion += "<i>Si tiene 3 o más Fervor, duplica las bonificaciones.</i>\n\n";
    }
       
      if (TRADU.i.nIdioma == 2) // English translation
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura I</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 1 Barrier and 1 Divine damage bonus to Allies.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Barrier</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura II</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 2 Barrier and 1 Divine damage bonus to Allies.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Next Level: +1 Divine damage bonus</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura III</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 2 Barrier and 2 Divine damage bonus to Allies.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Option A: Removes the need for Fervor.</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Option B: If has 3 or more Fervor, doubles the bonuses.</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passive) Gives 2 Barrier and 2 Divine damage bonus to Allies.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Sacred Aura IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passive) While the Purifier has Fervor, gives 2 Barrier and 2 Divine damage bonus to Allies.</i>\n\n";
          txtDescripcion += "<i>If has 3 or more Fervor, doubles the bonuses.</i>\n\n";
        }
      }
      if (TRADU.i.nIdioma == 3) // Portuguese translation
      {
        if (NIVEL < 2)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada I</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 1 Barreira e 1 bonus de dano Divino aos Aliados.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: +1 Barreira</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 2)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada II</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 2 Barreira e 1 bonus de dano Divino aos Aliados.</i>\n\n";
          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Proximo Nivel: +1 bonus de dano Divino</color>\n\n";
              }
            }
          }
        }
        if (NIVEL == 3)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada III</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 2 Barreira e 2 bonus de dano Divino aos Aliados.</i>\n\n";

          if (EsEscenaCampaña())
          {
            if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
              {
                txtDescripcion += $"<color=#dfea02>-Opcao A: Remove a necessidade de Fervor.</color>\n\n";
                txtDescripcion += $"<color=#dfea02>-Opcao B: Se tiver 3 ou mais Fervor, duplica os bonus.</color>\n";
              }
            }
          }
        }
        if (NIVEL == 4)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV a</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Concede 2 Barreira e 2 bonus de dano Divino aos Aliados.</i>\n\n";
        }
        if (NIVEL == 5)
        {
          txtDescripcion = "<color=#5dade2><b>Aura Sagrada IV b</b></color>\n\n";
          txtDescripcion += "<i>(Passiva) Enquanto a Purificadora tiver Fervor, concede 2 Barreira e 2 bonus de dano Divino aos Aliados.</i>\n\n";
          txtDescripcion += "<i>Se tiver 3 ou mais Fervor, duplica os bonus.</i>\n\n";
        }
      }
      AplicarDescripcionEstandar();
    }

    private void AplicarDescripcionEstandar()
    {
      int barrera = NIVEL < 2 ? 1 : 2;
      int bonusDivino = NIVEL < 3 ? 1 : 2;
      bool requiereFervor = NIVEL != 4;

      string titulo = $"Aura Sagrada {SufijoNivel()}";
      string subtitulo = requiereFervor
        ? "<color=#4f5552>Pasiva: con Fervor, mejora a los aliados.</color>"
        : "<color=#4f5552>Pasiva: mejora a los aliados sin requerir Fervor.</color>";
      string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Pasiva</color>\n" +
                      "<color=#44d3ec><b>Objetivo:</b></color> <color=#ffffff>Aliados</color>\n" +
                      "<color=#44d3ec><b>Requisito:</b></color> <color=#ffffff>" + (requiereFervor ? "Purificadora con 1+ Fervor." : "No requiere Fervor.") + "</color>\n" +
                      $"<color=#44d3ec><b>Efecto:</b></color> <color=#ffffff>+{barrera} Barrera, +{bonusDivino} daño Divino.</color>";

      if (NIVEL == 5)
      {
        cuerpo += "\n<color=#44d3ec><b>Con 3+ Fervor:</b></color> <color=#ffffff>Duplica las bonificaciones.</color>";
      }

      string proximo = TextoProximoNivel();
      if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }

      if (TRADU.i.nIdioma == 2)
      {
        titulo = $"Sacred Aura {SufijoNivel()}";
        subtitulo = requiereFervor
          ? "<color=#4f5552>Passive: with Fervor, buffs allies.</color>"
          : "<color=#4f5552>Passive: buffs allies without requiring Fervor.</color>";
        cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Passive</color>\n" +
                 "<color=#44d3ec><b>Target:</b></color> <color=#ffffff>Allies</color>\n" +
                 "<color=#44d3ec><b>Requirement:</b></color> <color=#ffffff>" + (requiereFervor ? "Purifier has 1+ Fervor." : "No Fervor required.") + "</color>\n" +
                 $"<color=#44d3ec><b>Effect:</b></color> <color=#ffffff>+{barrera} Barrier, +{bonusDivino} Divine damage.</color>";
        if (NIVEL == 5)
        {
          cuerpo += "\n<color=#44d3ec><b>At 3+ Fervor:</b></color> <color=#ffffff>Doubles the bonuses.</color>";
        }
        proximo = TextoProximoNivel();
        if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }
      }
      else if (TRADU.i.nIdioma == 3)
      {
        titulo = $"Aura Sagrada {SufijoNivel()}";
        subtitulo = requiereFervor
          ? "<color=#4f5552>Passiva: com Fervor, melhora aliados.</color>"
          : "<color=#4f5552>Passiva: melhora aliados sem exigir Fervor.</color>";
        cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Passiva</color>\n" +
                 "<color=#44d3ec><b>Alvo:</b></color> <color=#ffffff>Aliados</color>\n" +
                 "<color=#44d3ec><b>Requisito:</b></color> <color=#ffffff>" + (requiereFervor ? "Purificadora com 1+ Fervor." : "Nao exige Fervor.") + "</color>\n" +
                 $"<color=#44d3ec><b>Efeito:</b></color> <color=#ffffff>+{barrera} Barreira, +{bonusDivino} dano Divino.</color>";
        if (NIVEL == 5)
        {
          cuerpo += "\n<color=#44d3ec><b>Com 3+ Fervor:</b></color> <color=#ffffff>Duplica os bonus.</color>";
        }
        proximo = TextoProximoNivel();
        if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }
      }

      txtDescripcion = ConstruirDescripcionEstandar($"<size=115%>{titulo}</size>", subtitulo, cuerpo, "", "#5dade2");
    }

    private string SufijoNivel()
    {
      if (NIVEL < 2) { return "I"; }
      if (NIVEL == 2) { return "II"; }
      if (NIVEL == 3) { return "III"; }
      if (NIVEL == 4) { return "IV a"; }
      return "IV b";
    }

    private string TextoProximoNivel()
    {
      if (!EsEscenaCampaña() || CampaignManager.Instance.scMenuPersonajes.pSel == null || CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad <= 0)
      {
        return "";
      }

      if (TRADU.i.nIdioma == 2)
      {
        if (NIVEL < 2) { return "<color=#dfea02>Next Level: +1 Barrier.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Next Level: +1 Divine damage.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Option A: removes Fervor requirement.\nOption B: 3+ Fervor doubles bonuses.</color>"; }
      }
      else if (TRADU.i.nIdioma == 3)
      {
        if (NIVEL < 2) { return "<color=#dfea02>Proximo Nivel: +1 Barreira.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Proximo Nivel: +1 dano Divino.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcao A: remove requisito de Fervor.\nOpcao B: 3+ Fervor duplica bonus.</color>"; }
      }
      else
      {
        if (NIVEL < 2) { return "<color=#dfea02>Próximo Nivel: +1 Barrera.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +1 daño Divino.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opción A: remueve requisito de Fervor.\nOpción B: 3+ Fervor duplica bonificaciones.</color>"; }
      }

      return "";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}



