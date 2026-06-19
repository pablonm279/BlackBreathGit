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
      bajada = "Improves attacks made with a hand crossbow.";
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
