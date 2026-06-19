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
    int ataque = NIVEL == 5 ? 2 : 1;
    int critico = NIVEL == 4 ? 10 : NIVEL > 1 ? 5 : 0;
    int danio = NIVEL >= 4 ? 4 : 2;
    bool reduceAp = NIVEL >= 3;
    string sufijo = SufijoNivel();
    string titulo = $"Maestría con Espada Corta {sufijo}";
    string bajada = "Mejora los ataques hechos con espada corta.";
    string etiquetaTipo = "Tipo";
    string etiquetaAplica = "Aplica a";
    string etiquetaBonus = "Bonus";
    string etiquetaExtra = "Extra";
    string tipo = "Pasiva";
    string aplica = "Ataques con espada corta";
    string bonificador = $"+{ataque} Ataque, +{danio} daño Cortante";
    string extra = reduceAp ? "-1 costo AP" : "";
    string proximo = ProximoNivel();

    if (critico > 0)
    {
      bonificador += $", +{critico}% Crítico";
    }

    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      titulo = $"Short Sword Mastery {sufijo}";
      bajada = "Improves attacks made with a short sword.";
      etiquetaTipo = "Type";
      etiquetaAplica = "Applies to";
      etiquetaBonus = "Bonus";
      etiquetaExtra = "Extra";
      tipo = "Passive";
      aplica = "Short sword attacks";
      bonificador = $"+{ataque} Attack, +{danio} Slashing damage";
      if (critico > 0) bonificador += $", +{critico}% Critical";
      extra = reduceAp ? "-1 AP cost" : "";
      proximo = ProximoNivelIngles();
    }
    if (TRADU.i.nIdioma == 3)
    {
      titulo = $"Maestria com Espada Curta {sufijo}";
      bajada = "Melhora ataques feitos com espada curta.";
      etiquetaTipo = "Tipo";
      etiquetaAplica = "Aplica a";
      etiquetaBonus = "Bonus";
      etiquetaExtra = "Extra";
      tipo = "Passiva";
      aplica = "Ataques com espada curta";
      bonificador = $"+{ataque} Ataque, +{danio} de dano Cortante";
      if (critico > 0) bonificador += $", +{critico}% Crítico";
      extra = reduceAp ? "-1 custo AP" : "";
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
    if (NIVEL == 3) return "- Opción A: +5% Crítico, +2 daño\n- Opción B: +1 Ataque, +2 daño";
    return "";
  }

  string ProximoNivelIngles()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Next Level: +5% Critical";
    if (NIVEL == 2) return "- Next Level: -1 AP cost";
    if (NIVEL == 3) return "- Option A: +5% Critical, +2 damage\n- Option B: +1 Attack, +2 damage";
    return "";
  }

  string ProximoNivelPortugues()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Próximo Nivel: +5% Crítico";
    if (NIVEL == 2) return "- Proximo Nivel: -1 custo AP";
    if (NIVEL == 3) return "- Opcao A: +5% Critico, +2 dano\n- Opcao B: +1 Ataque, +2 dano";
    return "";
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
    }
}
