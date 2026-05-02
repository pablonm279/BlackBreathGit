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

      string titulo = $"Vista Lejana {SufijoNivel()}";
      string subtitulo = "Mejora al Explorador si empieza el turno en la ultima columna.";
      string cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n" +
                      $"<color={colorEncabezado}><b>Activacion:</b></color> Empieza su turno en la ultima columna\n" +
                      $"<color={colorEncabezado}><b>Efecto:</b></color> +{ataque} Ataque, +{danioPorcentaje}% Danio";
      if (critico) { cuerpo += ", +5% Critico"; }
      if (defensa) { cuerpo += ", +1 Defensa"; }

      if (esIngles)
      {
        titulo = $"Long Sight {SufijoNivel()}";
        subtitulo = "Improves the Explorer when starting the turn in the last column.";
        cuerpo = $"<color={colorEncabezado}><b>Type:</b></color> Passive\n" +
                 $"<color={colorEncabezado}><b>Trigger:</b></color> Starts turn in the last column\n" +
                 $"<color={colorEncabezado}><b>Effect:</b></color> +{ataque} Attack, +{danioPorcentaje}% Damage";
        if (critico) { cuerpo += ", +5% Crit"; }
        if (defensa) { cuerpo += ", +1 Defense"; }
      }
      else if (esPortugues)
      {
        titulo = $"Visao Distante {SufijoNivel()}";
        subtitulo = "Melhora o Explorador se comecar o turno na ultima coluna.";
        cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n" +
                 $"<color={colorEncabezado}><b>Ativacao:</b></color> Comeca o turno na ultima coluna\n" +
                 $"<color={colorEncabezado}><b>Efeito:</b></color> +{ataque} Ataque, +{danioPorcentaje}% Dano";
        if (critico) { cuerpo += ", +5% Critico"; }
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
        if (NIVEL == 2) { return "<color=#dfea02>Proximo Nivel: +1 Ataque.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcao A: +5% Critico.\nOpcao B: +1 Defesa.</color>"; }
      }
      else
      {
        if (NIVEL < 2) { return "<color=#dfea02>Proximo Nivel: +5% Danio.</color>"; }
        if (NIVEL == 2) { return "<color=#dfea02>Proximo Nivel: +1 Ataque.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcion A: +5% Critico.\nOpcion B: +1 Defensa.</color>"; }
      }

      return "";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar(){}
}
