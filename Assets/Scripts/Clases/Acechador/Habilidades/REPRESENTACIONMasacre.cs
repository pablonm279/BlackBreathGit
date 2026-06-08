using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONMasacre : Habilidad
{
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_Masacre");
      ActualizarDescripcion();
      IDenClase = 10;
    }

  public override void ActualizarDescripcion()
  {
    string colorAgilidad = "#7fa35a";
    int apGanado = NIVEL == 5 ? 3 : 2;
    int danio = NIVEL > 2 ? 15 : 10;
    int dcBase = NIVEL > 1 ? 6 : 5;
    int apMaxAterrorizado = NIVEL == 4 ? -2 : -1;
    string sufijo = SufijoNivel();
    string titulo = $"Masacre {sufijo}";
    string bajada = "Al matar, gana AP y daño; los enemigos cercanos pueden quedar aterrorizados.";
    string etiquetaTipo = "Tipo";
    string etiquetaDisparador = "Disparador";
    string etiquetaEfecto = "Efecto";
    string etiquetaTiradaEnemiga = "Tirada enemiga";
    string etiquetaFallo = "Fallo";
    string tipo = "Pasiva";
    string disparador = "Al matar a un enemigo";
    string tirada = $"TS Mental vs DC {dcBase} + <color={colorAgilidad}>Agilidad</color>";
    string efectoPropio = $"+{apGanado} AP, +{danio}% daño este turno";
    string efectoEnemigo = $"Aterrorizado: -2 Ataque, {apMaxAterrorizado} AP Max, -2 TS Mental";
    string proximo = ProximoNivel();

    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      titulo = $"Massacre {sufijo}";
      bajada = "On kill, gains AP and damage; nearby enemies may become terrified.";
      etiquetaTipo = "Type";
      etiquetaDisparador = "Trigger";
      etiquetaEfecto = "Effect";
      etiquetaTiradaEnemiga = "Enemy save";
      etiquetaFallo = "On fail";
      tipo = "Passive";
      disparador = "When killing an enemy";
      tirada = $"Mental Save vs DC {dcBase} + <color={colorAgilidad}>Agility</color>";
      efectoPropio = $"+{apGanado} AP, +{danio}% damage this turn";
      efectoEnemigo = $"Terrified: -2 Attack, {apMaxAterrorizado} Max AP, -2 Mental Save";
      proximo = ProximoNivelIngles();
    }
    if (TRADU.i.nIdioma == 3)
    {
      titulo = $"Massacre {sufijo}";
      bajada = "Ao matar, ganha AP e dano; inimigos proximos podem ficar aterrorizados.";
      etiquetaTipo = "Tipo";
      etiquetaDisparador = "Ativacao";
      etiquetaEfecto = "Efeito";
      etiquetaTiradaEnemiga = "Teste inimigo";
      etiquetaFallo = "Se falhar";
      tipo = "Passiva";
      disparador = "Ao matar um inimigo";
      tirada = $"Teste Mental vs CD {dcBase} + <color={colorAgilidad}>Agilidade</color>";
      efectoPropio = $"+{apGanado} AP, +{danio}% de dano neste turno";
      efectoEnemigo = $"Aterrorizado: -2 Ataque, {apMaxAterrorizado} AP Max, -2 Teste Mental";
      proximo = ProximoNivelPortugues();
    }

    txtDescripcion = $"<size=115%><color=#5dade2><b>{titulo}</b></color></size>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{bajada}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaTipo}:</b></color> <color=#ffffff>{tipo}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaDisparador}:</b></color> <color=#ffffff>{disparador}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaEfecto}:</b></color> <color=#ffffff>{efectoPropio}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaTiradaEnemiga}:</b></color> <color=#ffffff>{tirada}</color>\n";
    txtDescripcion += $"<color=#44d3ec><b>{etiquetaFallo}:</b></color> <color=#ffffff>{efectoEnemigo}</color>";

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
    if (NIVEL < 2) return "- Próximo Nivel: +1 DC";
    if (NIVEL == 2) return "- Próximo Nivel: +5% daño este turno";
    if (NIVEL == 3) return "- Opción A: Aterrorizado aplica -2 AP Max\n- Opción B: +1 AP al matar";
    return "";
  }

  string ProximoNivelIngles()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Next Level: +1 DC";
    if (NIVEL == 2) return "- Next Level: +5% damage this turn";
    if (NIVEL == 3) return "- Option A: Terrified applies -2 Max AP\n- Option B: +1 AP on kill";
    return "";
  }

  string ProximoNivelPortugues()
  {
    if (!PuedeMostrarProximoNivel()) return "";
    if (NIVEL < 2) return "- Proximo Nivel: +1 CD";
    if (NIVEL == 2) return "- Proximo Nivel: +5% de dano neste turno";
    if (NIVEL == 3) return "- Opcao A: Aterrorizado aplica -2 AP Max\n- Opcao B: +1 AP ao matar";
    return "";
  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
    }
}
