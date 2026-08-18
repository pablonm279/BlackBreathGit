using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONVistaLejana : Habilidad
{
    public override void Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_VistaLejana");
      ActualizarDescripcion();

      IDenClase = 1;
    }

    public bool seusoEsteTurno = false;

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      string colorTitulo = "#5dade2";
      string colorEncabezado = "#44d3ec";

      int ataque = NIVEL > 2 ? 2 : 1;
      int danioPorcentaje = NIVEL > 1 ? 15 : 10;
      bool critico = NIVEL == 4;
      bool defensa = NIVEL == 5;

      if (esIngles)
      {
        string criticoTermino = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
        string defensaTermino = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "+5% Damage."; }
          else if (NIVEL == 2) { proximaMejora = "+1 Attack."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: +5% Crit.\nOption B: +1 Defense."; }
        }

        string buff = $"+{ataque} Attack, +{danioPorcentaje}% Damage";
        if (critico) { buff += $", +5% {criticoTermino}"; }
        if (defensa) { buff += $", +1 {defensaTermino}"; }
        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          $"Long Sight {SufijoNivel()}",
          "Passive: Improves bow attacks from the rear column.",
          new[]
          {
            LineaDescripcion("Trigger", "Starts the turn in the last column."),
            LineaDescripcion("Buff", buff)
          },
          proximaMejora);
        return;
      }

      if (esPortugues)
      {
        string criticoTermino = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
        string defensaTermino = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nível: +5% de Dano."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nível: +1 Ataque."; }
          else if (NIVEL == 3) { proximaMejora = "Opção A: +5% de Crítico.\nOpção B: +1 Defesa."; }
        }
        string buff = $"+{ataque} Ataque, +{danioPorcentaje}% de Dano";
        if (critico) { buff += $", +5% de {criticoTermino}"; }
        if (defensa) { buff += $", +1 {defensaTermino}"; }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          $"Visão Longa {SufijoNivel()}", "Passiva: melhora os ataques com arco a partir da coluna traseira.",
          new[] { LineaDescripcion("Ativação", "Começa o turno na última coluna."), LineaDescripcion("Bônus", buff) }, proximaMejora, costoSuperior: string.Empty);
        return;
      }

      {
        string criticoTermino = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
        string defensaTermino = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nivel: +5% de Daño."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nivel: +1 Ataque."; }
          else if (NIVEL == 3) { proximaMejora = "Opción A: +5% de Crítico.\nOpción B: +1 Defensa."; }
        }
        string buff = $"+{ataque} Ataque, +{danioPorcentaje}% de Daño";
        if (critico) { buff += $", +5% de {criticoTermino}"; }
        if (defensa) { buff += $", +1 {defensaTermino}"; }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          $"Vista Lejana {SufijoNivel()}", "Pasiva: mejora los ataques con arco desde la columna trasera.",
          new[] { LineaDescripcion("Activación", "Comienza el turno en la última columna."), LineaDescripcion("Bonificación", buff) }, proximaMejora, costoSuperior: string.Empty);
        return;
      }

      string titulo = $"Vista Lejana {SufijoNivel()}";
      string subtitulo = "Proporciona bonificaciones al ataque si el Explorador comienza su turno en la última columna.";
      string cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n" +
                      $"<color={colorEncabezado}><b>Activacion:</b></color> Empieza su turno en la ultima columna\n" +
                      $"<color={colorEncabezado}><b>Efecto:</b></color> +{ataque} Ataque, +{danioPorcentaje}% Daño";
      if (critico) { cuerpo += ", +5% Crítico"; }
      if (defensa) { cuerpo += ", +1 Defensa"; }

      if (esIngles)
      {
        titulo = $"Long Sight {SufijoNivel()}";
        subtitulo = "Grants attack bonuses if the Explorer starts their turn in the last column.";
        cuerpo = $"<color={colorEncabezado}><b>Type:</b></color> Passive\n" +
                 $"<color={colorEncabezado}><b>Trigger:</b></color> Starts turn in the last column\n" +
                 $"<color={colorEncabezado}><b>Effect:</b></color> +{ataque} Attack, +{danioPorcentaje}% Damage";
        if (critico) { cuerpo += ", +5% Crit"; }
        if (defensa) { cuerpo += ", +1 Defense"; }
      }
      else if (esPortugues)
      {
        titulo = $"Visao Distante {SufijoNivel()}";
        subtitulo = "Concede bonus de ataque se o Explorador começar seu turno na última coluna.";
        cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n" +
                 $"<color={colorEncabezado}><b>Ativacao:</b></color> Comeca o turno na ultima coluna\n" +
                 $"<color={colorEncabezado}><b>Efeito:</b></color> +{ataque} Ataque, +{danioPorcentaje}% Dano";
        if (critico) { cuerpo += ", +5% Crítico"; }
        if (defensa) { cuerpo += ", +1 Defesa"; }
      }

      string proximo = TextoProximoNivel(esIngles, esPortugues);
      if (!string.IsNullOrEmpty(proximo)) { cuerpo += "\n\n" + proximo; }

      txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size>\n\n";
      txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
      txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
      txtDescripcion += cuerpo;
    }

    private string SufijoNivel()
    {
      if (NIVEL < 2) { return "I"; }
      if (NIVEL == 2) { return "II"; }
      if (NIVEL == 3) { return "III"; }
      if (NIVEL == 4) { return "IV a"; }
      return "IV b";
    }

    private string TextoProximoNivel(bool esIngles, bool esPortugues)
    {
      if (!EsEscenaCampaña() || CampaignManager.Instance.scMenuPersonajes.pSel == null || CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad <= 0)
      {
        return "";
      }

      if (esIngles)
      {
        if (NIVEL < 2) { return "<color=#dfea02>Next Level: +5% Damage.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Next Level: +1 Attack.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Option A: +5% Crit.\nOption B: +1 Defense.</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { return "<color=#dfea02>Proximo Nivel: +5% Dano.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +1 Ataque.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcao A: +5% Critico.\nOpcao B: +1 Defesa.</color>"; }
      }
      else
      {
        if (NIVEL < 2) { return "<color=#dfea02>Próximo Nivel: +5% Daño.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +1 Ataque.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opción A: +5% Crítico.\nOpción B: +1 Defensa.</color>"; }
      }

      return "";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar(){}
}
