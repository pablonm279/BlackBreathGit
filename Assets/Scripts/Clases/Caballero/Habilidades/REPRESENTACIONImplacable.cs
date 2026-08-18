using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONImplacable : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_Implacable");
      IDenClase = 9;
      ActualizarDescripcion();
       
 

    }

  public override void ActualizarDescripcion()
  {
    if (TRADU.i.nIdioma == 1) // Español
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Implacable I</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +3 Fue, +20% Daño, +3 TS Mental. Dura 2 Turnos. 1 vez.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Fue +2 Tirada Salvación Fortaleza al efecto</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==2)
      {
        txtDescripcion = "<color=#5dade2><b>Implacable II</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fue, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura 2 Turnos. 1 vez.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +5% Crítico</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==3)
      {
        txtDescripcion = "<color=#5dade2><b>Implacable III</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fue, +5% Crítico, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura 2 Turnos. 1 vez.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Opción A: -1 Valentía Máxima +1 Uso de la Habilidad</color>\n";
              txtDescripcion += $"<color=#dfea02>-Opción B: +1 Turno de duración</color>\n";
            }
          }
        }
      }
      if(NIVEL==4)
      {
        txtDescripcion = "<color=#5dade2><b>Implacable IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +1 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fue, +5% Crítico, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura 2 Turnos. 2 veces.</i>\n\n";
      }
      if(NIVEL==5)
      {
        txtDescripcion = "<color=#5dade2><b>Implacable IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Si posee la Valentía al Máximo se obtiene: +2 AP, +4 Fue, +5% Crítico, +20% Daño, +3 TS Mental, +3 TS Fortaleza. Dura  Turnos. 1 vez.</i>\n\n";
      }
    }
    if (TRADU.i.nIdioma == 2) // Inglés
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Relentless I</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive) +2 Maximum Valour.</i>\n\n";
        txtDescripcion += "<i>If Valour is at Maximum: +2 AP, +3 Strength, +20% Damage, +3 Mental Save. Lasts 2 Turns. 1 time.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: +1 Strength +2 Fortitude Save against effect</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==2)
      {
        txtDescripcion = "<color=#5dade2><b>Relentless II</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive) +2 Maximum Valour.</i>\n\n";
        txtDescripcion += "<i>If Valour is at Maximum: +2 AP, +4 Strength, +20% Damage, +3 Mental Save, +3 Fortitude Save. Lasts 2 Turns. 1 time.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Next Level: +5% Critical</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==3)
      {
        txtDescripcion = "<color=#5dade2><b>Relentless III</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive) +2 Maximum Valour.</i>\n\n";
        txtDescripcion += "<i>If Valour is at Maximum: +2 AP, +4 Strength, +5% Critical, +20% Damage, +3 Mental Save, +3 Fortitude Save. Lasts 2 Turns. 1 time.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Option A: -1 Maximum Valour +1 Skill Use</color>\n";
              txtDescripcion += $"<color=#dfea02>-Option B: +1 Turn duration</color>\n";
            }
          }
        }
      }
      if(NIVEL==4)
      {
        txtDescripcion = "<color=#5dade2><b>Relentless IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive) +1 Maximum Valour.</i>\n\n";
        txtDescripcion += "<i>If Valour is at Maximum: +2 AP, +4 Strength, +5% Critical, +20% Damage, +3 Mental Save, +3 Fortitude Save. Lasts 2 Turns. 2 times.</i>\n\n";
      }
      if(NIVEL==5)
      {
        txtDescripcion = "<color=#5dade2><b>Relentless IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Passive) +2 Maximum Valour.</i>\n\n";
        txtDescripcion += "<i>If Valour is at Maximum: +2 AP, +4 Strength, +5% Critical, +20% Damage, +3 Mental Save, +3 Fortitude Save. Lasts  Turns. 1 time.</i>\n\n";
      }
    }
    if (TRADU.i.nIdioma == 3) // Portugues
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Implacavel I</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Se estiver com Valentia Maxima: +2 AP, +3 Forca, +20% Dano, +3 Resistencia Mental. Dura 2 turnos. 1 vez.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Proximo Nivel: +1 Forca e +2 Resistencia de Fortitude no efeito</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==2)
      {
        txtDescripcion = "<color=#5dade2><b>Implacavel II</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Se estiver com Valentia Maxima: +2 AP, +4 Forca, +20% Dano, +3 Resistencia Mental, +3 Resistencia de Fortitude. Dura 2 turnos. 1 vez.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +5% Crítico</color>\n\n";
            }
          }
        }
      }
      if(NIVEL==3)
      {
        txtDescripcion = "<color=#5dade2><b>Implacavel III</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Se estiver com Valentia Maxima: +2 AP, +4 Forca, +5% Critico, +20% Dano, +3 Resistencia Mental, +3 Resistencia de Fortitude. Dura 2 turnos. 1 vez.</i>\n\n";
        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
            if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
            {
              txtDescripcion += $"<color=#dfea02>-Opcao A: -1 Valentia Maxima e +1 Uso da habilidade</color>\n";
              txtDescripcion += $"<color=#dfea02>-Opcao B: +1 turno de duracao</color>\n";
            }
          }
        }
      }
      if(NIVEL==4)
      {
        txtDescripcion = "<color=#5dade2><b>Implacavel IV a</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva) +1 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Se estiver com Valentia Maxima: +2 AP, +4 Forca, +5% Critico, +20% Dano, +3 Resistencia Mental, +3 Resistencia de Fortitude. Dura 2 turnos. 2 vezes.</i>\n\n";
      }
      if(NIVEL==5)
      {
        txtDescripcion = "<color=#5dade2><b>Implacavel IV b</b></color>\n\n"; 
        txtDescripcion += "<i>(Passiva) +2 Valentía Máxima.</i>\n\n";
        txtDescripcion += "<i>Se estiver com Valentia Maxima: +2 AP, +4 Forca, +5% Critico, +20% Dano, +3 Resistencia Mental, +3 Resistencia de Fortitude. Dura  turnos. 1 vez.</i>\n\n";
      }
    }
    AplicarDescripcionEstandar();
  }

  private void AplicarDescripcionEstandar()
  {
    int valentiaMaxima = NIVEL == 4 ? 1 : 2;
    int fuerza = NIVEL > 1 ? 4 : 3;
    int duracion = NIVEL == 5 ? 3 : 2;
    int usos = NIVEL == 4 ? 2 : 1;

    if (TRADU.i != null && TRADU.i.nIdioma == 2)
    {
      string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valour", "Valentía");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "max AP", "ap");
      string fuerzaTermino = TerminoDescripcion(TerminoDescripcionId.Fuerza, "Strength");
      string mental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Mental Save", "ic_mental");
      string fortaleza = TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza, "Fortitude Save", "ic_fortaleza");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "+1 Strength and +2 Fortitude Save in the buff."; }
        else if (NIVEL == 2) { proximaMejora = "+5% Crit in the buff."; }
        else if (NIVEL == 3) { proximaMejora = "Option A: -1 Maximum Valour and +1 use.\nOption B: +1 turn duration."; }
      }

      string efecto = $"Gains +2 {ap}, +{fuerza} {fuerzaTermino}, +20% Damage, +3 {mental}";
      if (NIVEL > 1) { efecto += $", +2 {fortaleza}"; }
      if (NIVEL > 2) { efecto += $", +5% {critico}"; }
      efecto += ".";

      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        $"Relentless {SufijoNivel()}",
        "Passive: Reaching maximum Valour triggers a powerful temporary buff.",
        new[]
        {
          LineaDescripcion("Base", $"+{valentiaMaxima} Maximum {valentia}."),
          LineaDescripcion("Trigger", "Reaches maximum Valour."),
          LineaDescripcion("Effect", efecto, 1),
          LineaDescripcion("Duration", $"{duracion} turns", 1),
          LineaDescripcion("Uses", $"{usos} per battle", 1)
        },
        proximaMejora);
      return;
    }

    if (TRADU.i != null && TRADU.i.nIdioma == 3)
    {
      string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentia", "Valentía");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP máximo", "ap");
      string forca = TerminoDescripcion(TerminoDescripcionId.Fuerza, "Força");
      string mental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Salvaguarda Mental", "ic_mental");
      string fortitude = TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza, "Salvaguarda de Fortitude", "ic_fortaleza");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "Próximo nível: +1 Força e +2 Salvaguarda de Fortitude no bônus."; }
        else if (NIVEL == 2) { proximaMejora = "Próximo nível: +5% de Crítico no bônus."; }
        else if (NIVEL == 3) { proximaMejora = "Opção A: -1 Valentia Máxima e +1 uso.\nOpção B: +1 turno de duração."; }
      }
      string efeito = $"Recebe +2 {ap}, +{fuerza} {forca}, +20% de Dano, +3 {mental}";
      if (NIVEL > 1) { efeito += $", +2 {fortitude}"; }
      if (NIVEL > 2) { efeito += $", +5% de {critico}"; }
      efeito += ".";
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada($"Implacável {SufijoNivel()}", "Passiva: alcançar a Valentia máxima ativa um poderoso bônus temporário.", new[]
      {
        LineaDescripcion("Base", $"+{valentiaMaxima} de {valentia} Máxima."), LineaDescripcion("Ativação", "Alcança a Valentia máxima."),
        LineaDescripcion("Efeito", efeito, 1), LineaDescripcion("Duração", $"{duracion} turnos", 1), LineaDescripcion("Usos", $"{usos} por batalha", 1)
      }, proximaMejora, costoSuperior: string.Empty);
      return;
    }

    {
      string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentía", "Valentía");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP máximo", "ap");
      string fuerzaTermino = TerminoDescripcion(TerminoDescripcionId.Fuerza, "Fuerza");
      string mental = TerminoDescripcion(TerminoDescripcionId.SalvacionMental, "Salvación Mental", "ic_mental");
      string fortaleza = TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza, "Salvación de Fortaleza", "ic_fortaleza");
      string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) { proximaMejora = "Próximo nivel: +1 Fuerza y +2 Salvación de Fortaleza en la bonificación."; }
        else if (NIVEL == 2) { proximaMejora = "Próximo nivel: +5% de Crítico en la bonificación."; }
        else if (NIVEL == 3) { proximaMejora = "Opción A: -1 Valentía Máxima y +1 uso.\nOpción B: +1 turno de duración."; }
      }
      string efecto = $"Obtiene +2 {ap}, +{fuerza} {fuerzaTermino}, +20% de Daño, +3 {mental}";
      if (NIVEL > 1) { efecto += $", +2 {fortaleza}"; }
      if (NIVEL > 2) { efecto += $", +5% de {critico}"; }
      efecto += ".";
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada($"Implacable {SufijoNivel()}", "Pasiva: alcanzar la Valentía máxima activa una potente bonificación temporal.", new[]
      {
        LineaDescripcion("Base", $"+{valentiaMaxima} de {valentia} Máxima."), LineaDescripcion("Activación", "Alcanza la Valentía máxima."),
        LineaDescripcion("Efecto", efecto, 1), LineaDescripcion("Duración", $"{duracion} turnos", 1), LineaDescripcion("Usos", $"{usos} por batalla", 1)
      }, proximaMejora, costoSuperior: string.Empty);
      return;
    }

    string titulo = $"Implacable {SufijoNivel()}";
    string subtitulo = $"<color=#4f5552>Pasiva: +{valentiaMaxima} Valentía Máxima; con Valentía al máximo gana un buff.</color>";
    string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Pasiva</color>\n" +
                    $"<color=#44d3ec><b>Base:</b></color> <color=#ffffff>+{valentiaMaxima} Valentía Máxima.</color>\n" +
                    $"<color=#44d3ec><b>Al maximo:</b></color> <color=#ffffff>+2 AP Max, <color=#d9822b>+{fuerza} Fuerza</color>, +20% Dano, +3 TS Mental";
    if (NIVEL > 1) { cuerpo += ", +2 TS Fortaleza"; }
    if (NIVEL > 2) { cuerpo += ", +5% Crítico"; }
    cuerpo += $". {duracion} turnos, {usos} vez";
    if (usos > 1) { cuerpo += "es"; }
    cuerpo += ".</color>";

    string proximo = TextoProximoNivel();
    if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }

    if (TRADU.i.nIdioma == 2)
    {
      titulo = $"Relentless {SufijoNivel()}";
      subtitulo = $"<color=#4f5552>Passive: +{valentiaMaxima} Maximum Valour; at maximum Valour gains a buff.</color>";
      cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Passive</color>\n" +
               $"<color=#44d3ec><b>Base:</b></color> <color=#ffffff>+{valentiaMaxima} Maximum Valour.</color>\n" +
               $"<color=#44d3ec><b>At maximum:</b></color> <color=#ffffff>+2 Max AP, <color=#d9822b>+{fuerza} Strength</color>, +20% Damage, +3 Mental Save";
      if (NIVEL > 1) { cuerpo += ", +2 Fortitude Save"; }
      if (NIVEL > 2) { cuerpo += ", +5% Critical"; }
      cuerpo += $". {duracion} turns, {usos} use";
      if (usos > 1) { cuerpo += "s"; }
      cuerpo += ".</color>";
      proximo = TextoProximoNivel();
      if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }
    }
    else if (TRADU.i.nIdioma == 3)
    {
      titulo = $"Implacavel {SufijoNivel()}";
      subtitulo = $"<color=#4f5552>Passiva: +{valentiaMaxima} Valentia Maxima; com Valentia no maximo ganha um buff.</color>";
      cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Passiva</color>\n" +
               $"<color=#44d3ec><b>Base:</b></color> <color=#ffffff>+{valentiaMaxima} Valentía Máxima.</color>\n" +
               $"<color=#44d3ec><b>No maximo:</b></color> <color=#ffffff>+2 AP Max, <color=#d9822b>+{fuerza} Forca</color>, +20% Dano, +3 Resistencia Mental";
      if (NIVEL > 1) { cuerpo += ", +2 Resistencia de Fortitude"; }
      if (NIVEL > 2) { cuerpo += ", +5% Crítico"; }
      cuerpo += $". {duracion} turnos, {usos} uso";
      if (usos > 1) { cuerpo += "s"; }
      cuerpo += ".</color>";
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
      if (NIVEL < 2) { return "<color=#dfea02>Next Level: +1 Strength and +2 Fortitude Save in the buff.</color>"; }
      if (NIVEL == 2) { return "<color=#dfea02>Next Level: +5% Critical in the buff.</color>"; }
      if (NIVEL == 3) { return "<color=#dfea02>Option A: -1 Maximum Valour, +1 use.\nOption B: +1 turn duration.</color>"; }
    }
    else if (TRADU.i.nIdioma == 3)
    {
      if (NIVEL < 2) { return "<color=#dfea02>Proximo Nivel: +1 Forca e +2 Resistencia de Fortitude no buff.</color>"; }
      if (NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +5% Crítico no buff.</color>"; }
      if (NIVEL == 3) { return "<color=#dfea02>Opcao A: -1 Valentia Maxima, +1 uso.\nOpcao B: +1 turno de duracao.</color>"; }
    }
    else
    {
      if (NIVEL < 2) { return "<color=#dfea02>Próximo Nivel: +1 Fuerza y +2 TS Fortaleza en el buff.</color>"; }
      if (NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +5% Crítico en el buff.</color>"; }
      if (NIVEL == 3) { return "<color=#dfea02>Opción A: -1 Valentía Máxima, +1 uso.\nOpción B: +1 turno de duración.</color>"; }
    }

    return "";
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}




