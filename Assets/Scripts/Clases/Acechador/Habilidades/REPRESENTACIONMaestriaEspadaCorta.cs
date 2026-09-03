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
    int danioCritico = NIVEL >= 3 ? 10 : 0;
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
    if (critico > 0) bonificador += $", +{critico}% Crítico";
    if (danioCritico > 0) bonificador += $", +{danioCritico}% daño crítico";
    string extra = "";
    string proximo = ProximoNivel();

    if (TRADU.i != null && TRADU.i.nIdioma == 2)
    {
      string ataqueTermino = TerminoDescripcion(TerminoDescripcionId.Ataque, "Attack");
      string danioCortante = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "Slashing damage", "dano_cortante");
      string criticoTermino = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
      string bonus = $"+{ataque} {ataqueTermino}, +{danio} {danioCortante}" + (critico > 0 ? $", +{critico}% {criticoTermino}" : "") + (danioCritico > 0 ? $", +{danioCritico}% critical damage" : ".");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) proximaMejora = "+5% Crit.";
        else if (NIVEL == 2) proximaMejora = "+10% critical damage.";
        else if (NIVEL == 3) proximaMejora = "Option A: +5% Crit and +2 damage. Option B: +1 Attack and +2 damage.";
      }
      var lineas = new List<LineaDescripcionNormalizada>
      {
        LineaDescripcion("Applies to", "Short sword attacks."),
        LineaDescripcion("Bonus", bonus)
      };
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        $"Short Sword Mastery {sufijo}",
        "Passive: Improves attacks made with the short sword.",
        lineas,
        proximaMejora);
      return;
    }

    if (TRADU.i != null && TRADU.i.nIdioma == 3)
    {
      string atk = TerminoDescripcion(TerminoDescripcionId.Ataque, "Ataque"); string danoT = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "dano Cortante", "dano_cortante"); string critT = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico"); string bonus = $"+{ataque} {atk}, +{danio} {danoT}" + (critico > 0 ? $", +{critico}% {critT}" : "") + (danioCritico > 0 ? $", +{danioCritico}% de dano crítico" : "."); string prox = !DebeMostrarProximaMejoraDescripcion() ? null : NIVEL < 2 ? "Próximo nível: +5% de Crítico." : NIVEL == 2 ? "Próximo nível: +10% de dano crítico." : NIVEL == 3 ? "Opção A: +5% de Crítico e +2 de dano. Opção B: +1 Ataque e +2 de dano." : null; var l = new List<LineaDescripcionNormalizada>{LineaDescripcion("Aplica-se a", "Ataques com espada curta."), LineaDescripcion("Bônus", bonus)}; txtDescripcion=ConstruirDescripcionNormalizadaLocalizada($"Maestria com Espada Curta {sufijo}","Passiva: melhora os ataques feitos com a espada curta.",l,prox,costoSuperior:string.Empty); return;
    }
    {
      string atk = TerminoDescripcion(TerminoDescripcionId.Ataque, "Ataque"); string danoT = TerminoDescripcion(TerminoDescripcionId.DanioCortante, "daño Cortante", "dano_cortante"); string critT = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico"); string bonus = $"+{ataque} {atk}, +{danio} {danoT}" + (critico > 0 ? $", +{critico}% {critT}" : "") + (danioCritico > 0 ? $", +{danioCritico}% de daño crítico" : "."); string prox = !DebeMostrarProximaMejoraDescripcion() ? null : NIVEL < 2 ? "Próximo nivel: +5% de Crítico." : NIVEL == 2 ? "Próximo nivel: +10% de daño crítico." : NIVEL == 3 ? "Opción A: +5% de Crítico y +2 de daño. Opción B: +1 Ataque y +2 de daño." : null; var l = new List<LineaDescripcionNormalizada>{LineaDescripcion("Se aplica a", "Ataques con espada corta."), LineaDescripcion("Bonificación", bonus)}; txtDescripcion=ConstruirDescripcionNormalizadaLocalizada($"Maestría con Espada Corta {sufijo}","Pasiva: mejora los ataques realizados con la espada corta.",l,prox,costoSuperior:string.Empty); return;
    }

    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      titulo = $"Short Sword Mastery {sufijo}";
      bajada = "Improves attacks made with the short sword.";
      etiquetaTipo = "Type";
      etiquetaAplica = "Applies to";
      etiquetaBonus = "Bonus";
      etiquetaExtra = "Extra";
      tipo = "Passive";
      aplica = "Short sword attacks";
      bonificador = $"+{ataque} Attack, +{danio} Slashing damage";
      if (critico > 0) bonificador += $", +{critico}% Critical";
      if (danioCritico > 0) bonificador += $", +{danioCritico}% critical damage";
      extra = "";
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
      if (danioCritico > 0) bonificador += $", +{danioCritico}% de dano crítico";
      extra = "";
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
    if (NIVEL == 2) return "- Próximo Nivel: +10% de daño crítico";
    if (NIVEL == 3) return "- Opción A: +5% Crítico, +2 daño\n- Opción B: +1 Ataque, +2 daño";
    return "";
  }

  string ProximoNivelIngles()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Next Level: +5% Critical";
    if (NIVEL == 2) return "- Next Level: +10% critical damage";
    if (NIVEL == 3) return "- Option A: +5% Critical, +2 damage\n- Option B: +1 Attack, +2 damage";
    return "";
  }

  string ProximoNivelPortugues()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Próximo Nivel: +5% Crítico";
    if (NIVEL == 2) return "- Proximo Nivel: +10% de dano crítico";
    if (NIVEL == 3) return "- Opcao A: +5% Critico, +2 dano\n- Opcao B: +1 Ataque, +2 dano";
    return "";
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
    }
}
