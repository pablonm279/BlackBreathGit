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
    int critico = NIVEL > 1 ? 5 : 0;
    bool reduceAp = NIVEL >= 3;
    bool alcance = NIVEL == 4;
    bool sinCooldown = NIVEL == 5;
    string sufijo = SufijoNivel();
    string titulo = $"Maestría con Ballesta de Mano {sufijo}";
    string bajada = "Mejora los ataques hechos con ballesta de mano.";
    string etiquetaTipo = "Tipo";
    string etiquetaAplica = "Aplica a";
    string etiquetaBonus = "Bonus";
    string etiquetaExtra = "Extra";
    string tipo = "Pasiva";
    string aplica = "Ataques con ballesta de mano";
    string bonificador = "+1 Ataque, +2 daño Perforante";
    string extra = "";
    string proximo = ProximoNivel();

    if (TRADU.i != null && TRADU.i.nIdioma == 2)
    {
      string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Attack");
      string danioPerforante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "Piercing damage", "dano_perforante");
      string criticoTermino = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string bonus = $"+1 {ataque}, +2 {danioPerforante}" + (critico > 0 ? $", +{critico}% {criticoTermino}" : "") + (alcance ? ", +1 range" : ".");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) proximaMejora = "+5% Crit.";
        else if (NIVEL == 2) proximaMejora = "-1 AP cost.";
        else if (NIVEL == 3) proximaMejora = "Option A: +1 range. Option B: removes cooldown.";
      }
      var lineas = new List<LineaDescripcionNormalizada>
      {
        LineaDescripcion("Applies to", "Hand crossbow attacks."),
        LineaDescripcion("Bonus", bonus)
      };
      if (reduceAp) lineas.Add(LineaDescripcion("Cost", $"-1 {ap}."));
      if (sinCooldown) lineas.Add(LineaDescripcion("Cooldown", "None."));
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        $"Hand Crossbow Mastery {sufijo}",
        "Passive: Improves attacks made with the hand crossbow.",
        lineas,
        proximaMejora);
      return;
    }

    if (TRADU.i != null && TRADU.i.nIdioma == 3)
    {
      string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Ataque"); string dano = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "dano Perfurante", "dano_perforante"); string crit = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico"); string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string bonus = $"+1 {ataque}, +2 {dano}" + (critico > 0 ? $", +{critico}% {crit}" : "") + (alcance ? ", +1 alcance" : "."); string prox = !DebeMostrarProximaMejoraDescripcion() ? null : NIVEL < 2 ? "Próximo nível: +5% de Crítico." : NIVEL == 2 ? "Próximo nível: -1 de custo de AP." : NIVEL == 3 ? "Opção A: +1 alcance. Opção B: remove a recarga." : null;
      var l = new List<LineaDescripcionNormalizada> { LineaDescripcion("Aplica-se a", "Ataques com besta de mão."), LineaDescripcion("Bônus", bonus) }; if (reduceAp) l.Add(LineaDescripcion("Custo", $"-1 {ap}.")); if (sinCooldown) l.Add(LineaDescripcion("Recarga", "Nenhuma."));
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada($"Maestria com Besta de Mão {sufijo}", "Passiva: melhora os ataques feitos com a besta de mão.", l, prox, costoSuperior: string.Empty); return;
    }
    {
      string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Ataque"); string dano = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "daño Perforante", "dano_perforante"); string crit = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico"); string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
      string bonus = $"+1 {ataque}, +2 {dano}" + (critico > 0 ? $", +{critico}% {crit}" : "") + (alcance ? ", +1 alcance" : "."); string prox = !DebeMostrarProximaMejoraDescripcion() ? null : NIVEL < 2 ? "Próximo nivel: +5% de Crítico." : NIVEL == 2 ? "Próximo nivel: -1 de costo de AP." : NIVEL == 3 ? "Opción A: +1 alcance. Opción B: elimina el enfriamiento." : null;
      var l = new List<LineaDescripcionNormalizada> { LineaDescripcion("Se aplica a", "Ataques con ballesta de mano."), LineaDescripcion("Bonificación", bonus) }; if (reduceAp) l.Add(LineaDescripcion("Costo", $"-1 {ap}.")); if (sinCooldown) l.Add(LineaDescripcion("Enfriamiento", "Ninguno."));
      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada($"Maestría con Ballesta de Mano {sufijo}", "Pasiva: mejora los ataques realizados con la ballesta de mano.", l, prox, costoSuperior: string.Empty); return;
    }

    if (critico > 0)
    {
      bonificador += $", +{critico}% Crítico";
    }
    if (alcance)
    {
      bonificador += ", +1 alcance";
    }
    if (reduceAp)
    {
      extra = "-1 costo AP";
    }
    if (sinCooldown)
    {
      extra += string.IsNullOrEmpty(extra) ? "Sin cooldown" : ", sin cooldown";
    }

    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      titulo = $"Hand Crossbow Mastery {sufijo}";
      bajada = "Improves attacks made with the hand crossbow.";
      etiquetaTipo = "Type";
      etiquetaAplica = "Applies to";
      etiquetaBonus = "Bonus";
      etiquetaExtra = "Extra";
      tipo = "Passive";
      aplica = "Hand crossbow attacks";
      bonificador = "+1 Attack, +2 Piercing damage";
      if (critico > 0) bonificador += $", +{critico}% Critical";
      if (alcance) bonificador += ", +1 range";
      extra = reduceAp ? "-1 AP cost" : "";
      if (sinCooldown) extra += string.IsNullOrEmpty(extra) ? "No cooldown" : ", no cooldown";
      proximo = ProximoNivelIngles();
    }
    if (TRADU.i.nIdioma == 3)
    {
      titulo = $"Maestria com Besta de Mao {sufijo}";
      bajada = "Melhora ataques feitos com besta de mao.";
      etiquetaTipo = "Tipo";
      etiquetaAplica = "Aplica a";
      etiquetaBonus = "Bonus";
      etiquetaExtra = "Extra";
      tipo = "Passiva";
      aplica = "Ataques com besta de mao";
      bonificador = "+1 Ataque, +2 de dano Perfurante";
      if (critico > 0) bonificador += $", +{critico}% Crítico";
      if (alcance) bonificador += ", +1 alcance";
      extra = reduceAp ? "-1 custo AP" : "";
      if (sinCooldown) extra += string.IsNullOrEmpty(extra) ? "Sem recarga" : ", sem recarga";
      proximo = ProximoNivelPortugues();
    }

    txtDescripcion = $"<size=115%><color=#5dade2><b>{titulo}</b></color></size>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{bajada}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaTipo}:</b></color> <color=#ffffff>{tipo}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaAplica}:</b></color> <color=#ffffff>{aplica}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaBonus}:</b></color> <color=#ffffff>{bonificador}</color>";
    if (!string.IsNullOrEmpty(extra))
    {
      txtDescripcion += $"\n<color=#44d3ec><b>{etiquetaExtra}:</b></color> <color=#ffffff>{extra}</color>";
    }
    if (!string.IsNullOrEmpty(proximo))
    {
      txtDescripcion += $"\n\n<color=#dfea02>{proximo}</color>";
    }
  }

  string SufijoNivel()
  {
    if (NIVEL < 2) return "I";
    if (NIVEL == 2) return "II";
    if (NIVEL == 3) return "III";
    if (NIVEL == 4) return "IVa";
    return "IVb";
  }

  bool PuedeMostrarProximoNivel()
  {
    return EsEscenaCampaña()
      && CampaignManager.Instance != null
      && CampaignManager.Instance.scMenuPersonajes != null
      && CampaignManager.Instance.scMenuPersonajes.pSel != null
      && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
  }

  string ProximoNivel()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Próximo Nivel: +5% Crítico";
    if (NIVEL == 2) return "- Próximo Nivel: -1 costo AP";
    if (NIVEL == 3) return "- Opción A: +1 alcance\n- Opción B: remueve cooldown";
    return "";
  }

  string ProximoNivelIngles()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Next Level: +5% Critical";
    if (NIVEL == 2) return "- Next Level: -1 AP cost";
    if (NIVEL == 3) return "- Option A: +1 range\n- Option B: removes cooldown";
    return "";
  }

  string ProximoNivelPortugues()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Próximo Nivel: +5% Crítico";
    if (NIVEL == 2) return "- Proximo Nivel: -1 custo AP";
    if (NIVEL == 3) return "- Opcao A: +1 alcance\n- Opcao B: remove recarga";
    return "";
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
    }
}
