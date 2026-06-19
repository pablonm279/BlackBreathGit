using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONReconocimiento : Habilidad
{
    public override void Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_Reconocimiento");
      ActualizarDescripcion();

      IDenClase = 9;
    }

    public bool seusoEsteTurno = false;

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      string colorTitulo = "#5dade2";
      string colorEncabezado = "#44d3ec";

      int apPrimerTurno = NIVEL == 4 ? 2 : 1;
      int iniciativa = NIVEL < 2 ? 0 : NIVEL == 2 ? 1 : 2;
      int retrasoRefuerzos = NIVEL == 5 ? 2 : 1;

      string titulo = $"Reconocimiento {SufijoNivel()}";
      string subtitulo = "Otorga ventaja inicial al grupo.";
      string cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n" +
                      $"<color={colorEncabezado}><b>Aliados:</b></color> +{apPrimerTurno} AP" +
                      (iniciativa > 0 ? $", +{iniciativa} Iniciativa" : "") + " en el primer turno\n" +
                      $"<color={colorEncabezado}><b>Enemigos:</b></color> Retrasa refuerzos {retrasoRefuerzos} turno" + (retrasoRefuerzos > 1 ? "s" : "");

      if (esIngles)
      {
        titulo = $"Reconnaissance {SufijoNivel()}";
        subtitulo = "Grants an opening advantage to the party.";
        cuerpo = $"<color={colorEncabezado}><b>Type:</b></color> Passive\n" +
                 $"<color={colorEncabezado}><b>Allies:</b></color> +{apPrimerTurno} AP" +
                 (iniciativa > 0 ? $", +{iniciativa} Initiative" : "") + " on the first turn\n" +
                 $"<color={colorEncabezado}><b>Enemies:</b></color> Delays reinforcements by {retrasoRefuerzos} turn" + (retrasoRefuerzos > 1 ? "s" : "");
      }
      else if (esPortugues)
      {
        titulo = $"Reconhecimento {SufijoNivel()}";
        subtitulo = "Concede vantagem inicial ao grupo.";
        cuerpo = $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n" +
                 $"<color={colorEncabezado}><b>Aliados:</b></color> +{apPrimerTurno} AP" +
                 (iniciativa > 0 ? $", +{iniciativa} Iniciativa" : "") + " no primeiro turno\n" +
                 $"<color={colorEncabezado}><b>Inimigos:</b></color> Atrasa reforcos em {retrasoRefuerzos} turno" + (retrasoRefuerzos > 1 ? "s" : "");
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
        if (NIVEL < 2 || NIVEL == 2) { return "<color=#dfea02>Next Level: +1 Initiative.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Option A: +1 AP.\nOption B: +1 reinforcement delay.</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2 || NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +1 Iniciativa.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opcao A: +1 AP.\nOpcao B: +1 atraso de reforcos.</color>"; }
      }
      else
      {
        if (NIVEL < 2 || NIVEL == 2) { return "<color=#dfea02>Próximo Nivel: +1 Iniciativa.</color>"; }
        if (NIVEL == 3) { return "<color=#dfea02>Opción A: +1 AP.\nOpción B: +1 retraso de refuerzos.</color>"; }
      }

      return "";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar(){}
}
