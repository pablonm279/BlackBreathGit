using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAcrobatico : Habilidad
{
    public override void Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_Acrobatico");
      ActualizarDescripcion();

      IDenClase = 2;
    }

    public bool seusoEsteTurno = false;

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      string colorTitulo = "#5dade2";
      string colorEncabezado = "#44d3ec";

      int evasion = NIVEL < 3 ? 1 : NIVEL < 4 ? 2 : 3;
      int reflejos = NIVEL > 1 ? 1 : 0;

      if (esIngles)
      {
        string evasionTermino = TerminoDescripcion(TerminoDescripcionId.Evasion, "Evasion", "Estado_evasion");
        string reflejosTermino = TerminoDescripcion(TerminoDescripcionId.SalvacionReflejos, "Reflex Save", "ic_Reflejos");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "+1 Reflex Save."; }
          else if (NIVEL == 2) { proximaMejora = "+1 Evasion."; }
          else if (NIVEL == 3) { proximaMejora = "Options A/B: +1 Evasion."; }
        }

        string efecto = $"+{evasion} {evasionTermino}";
        if (reflejos > 0)
        {
          efecto += $", +{reflejos} {reflejosTermino}";
        }
        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          $"Acrobatic {SufijoNivel()}",
          "Passive: Begins each combat with Evasion.",
          new[]
          {
            LineaDescripcion("Trigger", "Start of combat"),
            LineaDescripcion("Effect", efecto),
            LineaDescripcion("Evasion", $"Adds to {defensa}; lost after taking damage.")
          },
          proximaMejora,
          costoSuperior: string.Empty);
        return;
      }

      if (esPortugues)
      {
        string evasaoTermino = TerminoDescripcion(TerminoDescripcionId.Evasion, "Evasão", "Estado_evasion");
        string reflexosTermino = TerminoDescripcion(TerminoDescripcionId.SalvacionReflejos, "Resistência de Reflexos", "ic_Reflejos");
        string defesa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nível: +1 Resistência de Reflexos."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nível: +1 Evasão."; }
          else if (NIVEL == 3) { proximaMejora = "Próximo nível: Opções A/B: +1 Evasão."; }
        }
        string efeito = $"+{evasion} {evasaoTermino}";
        if (reflejos > 0) { efeito += $", +{reflejos} {reflexosTermino}"; }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          $"Acrobático {SufijoNivel()}",
          "Passiva: começa cada combate com Evasão.",
          new[]
          {
            LineaDescripcion("Ativação", "Início do combate"),
            LineaDescripcion("Efeito", efeito),
            LineaDescripcion("Evasão", $"É adicionada à {defesa}; é perdida ao sofrer dano.")
          },
          proximaMejora,
          costoSuperior: string.Empty);
        return;
      }

      {
        string evasionTermino = TerminoDescripcion(TerminoDescripcionId.Evasion, "Evasión", "Estado_evasion");
        string reflejosTermino = TerminoDescripcion(TerminoDescripcionId.SalvacionReflejos, "TS de Reflejos", "ic_Reflejos");
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nivel: +1 TS de Reflejos."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nivel: +1 Evasión."; }
          else if (NIVEL == 3) { proximaMejora = "Próximo nivel: Opciones A/B: +1 Evasión."; }
        }
        string efecto = $"+{evasion} {evasionTermino}";
        if (reflejos > 0) { efecto += $", +{reflejos} {reflejosTermino}"; }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
          $"Acrobático {SufijoNivel()}",
          "Pasiva: comienza cada combate con Evasión.",
          new[]
          {
            LineaDescripcion("Activación", "Inicio del combate"),
            LineaDescripcion("Efecto", efecto),
            LineaDescripcion("Evasión", $"Se suma a {defensa}; se pierde al recibir daño.")
          },
          proximaMejora,
          costoSuperior: string.Empty);
        return;
      }

      string titulo = $"Acrobatico {SufijoNivel()}";
      string subtitulo = "Gana Evasion al comienzo de cada combate.";
      string cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n" +
                      $"<color={colorEncabezado}><b>Activacion:</b></color> Comienzo de combate\n" +
                      $"<color={colorEncabezado}><b>Efecto:</b></color> +{evasion} Evasion" +
                      (reflejos > 0 ? $", +{reflejos} Reflejos" : "") + "\n" +
                      $"<color={colorEncabezado}><b>Evasion:</b></color> Se suma a Defensa y se pierde al recibir daño";

      if (esIngles)
      {
        titulo = $"Acrobatic {SufijoNivel()}";
        subtitulo = "Gains Evasion at the start of each combat.";
        cuerpo = $"<color={colorEncabezado}><b>Type:</b></color> Passive\n" +
                 $"<color={colorEncabezado}><b>Trigger:</b></color> Start of combat\n" +
                 $"<color={colorEncabezado}><b>Effect:</b></color> +{evasion} Evasion" +
                 (reflejos > 0 ? $", +{reflejos} Reflex" : "") + "\n" +
                 $"<color={colorEncabezado}><b>Evasion:</b></color> Adds to Defense and is lost when taking damage";
      }
      else if (esPortugues)
      {
        titulo = $"Acrobatico {SufijoNivel()}";
        subtitulo = "Recebe Evasao no inicio de cada combate.";
        cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n" +
                 $"<color={colorEncabezado}><b>Ativacao:</b></color> Inicio de combate\n" +
                 $"<color={colorEncabezado}><b>Efeito:</b></color> +{evasion} Evasao" +
                 (reflejos > 0 ? $", +{reflejos} Reflexos" : "") + "\n" +
                 $"<color={colorEncabezado}><b>Evasao:</b></color> Soma na Defesa e e perdida ao receber dano";
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
        if (NIVEL < 2) { return "<color=#dfea02>Next Level: +1 Reflex Save.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Next Level: +1 Evasion.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Option A/B: +1 Evasion.</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { return "<color=#dfea02>Próximo Nivel: +1 TS Reflexos.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +1 Evasao.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcao A/B: +1 Evasao.</color>"; }
      }
      else
      {
        if (NIVEL < 2) { return "<color=#dfea02>Próximo Nivel: +1 TS Reflejos.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +1 Evasion.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opción A/B: +1 Evasion.</color>"; }
      }

      return "";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar(){}
}
