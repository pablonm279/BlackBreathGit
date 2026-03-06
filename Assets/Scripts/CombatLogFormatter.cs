using System;
using System.Text;
using UnityEngine;

/// <summary>
/// Utilidades para dar formato consistente y legible al log de combate.
/// </summary>
public static class CombatLogFormatter
{
  public enum CombatOutcome
  {
    Pifia,
    Fallo,
    Roce,
    Golpe,
    Critico,
    Exito
  }

  private const string ColorEtiquetaAtaque = "#b88a4aff";
  private const string ColorEtiquetaSalvacion = "#5f9f9bff";
  private const string ColorExito = "#12673cff";
  private const string ColorFallo = "#822121ff";
  private const string ColorRoce = "#7a6421ff";
  private const string ColorCritico = "#e03409ff";
  private const string ColorEstado = "#563795ff";
  private const string ColorValour = "#5f9f9bff";
  private const string ColorBuff = "#2c766dff";
  private const string ColorDebuff = "#89354eff";
  private const string ColorTrampa = "#5c5049ff";
  private const string ColorDanio = "#9d2121ff";
  private const string ColorMuerte = "#3b1311ff";
  private const string ColorCuracion = "#2f7452ff";
  private static string T(string s) => TRADU.i != null ? TRADU.i.Traducir(s) : s;

  public static string FormatearAtaque(
    string atacante,
    string objetivo,
    float tiradaBase,
    float tiradaFinal,
    float modAtributo,
    float modHabilidad,
    float modAtaque,
    float defensaObjetivo,
    string textoResultado,
    CombatOutcome outcome,
    int umbralPifia,
    int umbralCritico,
    float deltaSituacional = 0f,
    string notaExtra = null)
  {
    float total = tiradaFinal + modAtributo + modHabilidad + modAtaque;

    string atacanteTx = string.IsNullOrEmpty(atacante) ? string.Empty : T(atacante);
    string objetivoTx = string.IsNullOrEmpty(objetivo) ? string.Empty : T(objetivo);

    var sb = new StringBuilder(160);
    sb.Append(Etiqueta(T("ATQ"), ColorEtiquetaAtaque)).Append(' ');
    if (!string.IsNullOrEmpty(atacanteTx))
    {
      sb.Append(atacanteTx);
      if (!string.IsNullOrEmpty(objetivoTx))
      {
        sb.Append(" -> ").Append(objetivoTx);
      }
    }
    else if (!string.IsNullOrEmpty(objetivoTx))
    {
      sb.Append(objetivoTx);
    }

    sb.Append(" | d20: ").Append(FormatearNumero(tiradaFinal));

    float deltaClima = tiradaFinal - tiradaBase;
    if (Mathf.Abs(deltaClima) > 0.01f)
    {
      sb.Append(" (").Append(FormatearNumeroConSigno(deltaClima)).Append(' ').Append(T("clima")).Append(')');
    }

    if (Mathf.Abs(deltaSituacional) > 0.01f)
    {
      sb.Append(" (").Append(FormatearNumeroConSigno(deltaSituacional)).Append(' ').Append(T("situacional")).Append(')');
    }

    sb.Append(" | ").Append(T("mods")).Append(": ")
      .Append(FormatearNumeroConSigno(modAtributo)).Append(' ').Append(T("atr")).Append(' ')
      .Append(FormatearNumeroConSigno(modHabilidad)).Append(' ').Append(T("hab")).Append(' ')
      .Append(FormatearNumeroConSigno(modAtaque)).Append(' ').Append(T("atq")).Append(' ');

    sb.Append("| = ").Append(FormatearNumero(total))
      .Append(' ').Append(T("vs")).Append(' ').Append(T("DEF")).Append(' ').Append(FormatearNumero(defensaObjetivo))
      .Append(" | ").Append(T("crit")).Append(" >= ").Append(umbralCritico)
      .Append(" -> ").Append(ResaltarResultado(textoResultado, outcome));

    if (!string.IsNullOrWhiteSpace(notaExtra))
    {
      sb.Append(" | ").Append(notaExtra.Trim());
    }

    return sb.ToString();
  }

  public static string FormatearSalvacion(
    string unidad,
    string tipo,
    int d20,
    float atributo,
    float dificultad,
    string textoResultado,
    CombatOutcome outcome,
    bool colorearResultado = true)
  {
    float total = d20 + atributo;

    string unidadTx = string.IsNullOrEmpty(unidad) ? string.Empty : T(unidad);
    string tipoTx = string.IsNullOrEmpty(tipo) ? string.Empty : T(tipo);

    var sb = new StringBuilder(140);
    sb.Append(Etiqueta(T("TS"), ColorEtiquetaSalvacion)).Append(' ');
    if (!string.IsNullOrEmpty(unidadTx))
    {
      sb.Append(unidadTx);
    }
    if (!string.IsNullOrEmpty(tipoTx))
    {
      sb.Append(" (").Append(tipoTx).Append(')');
    }

    sb.Append(" | d20: ").Append(FormatearNumero(d20))
      .Append(' ').Append(FormatearNumeroConSigno(atributo)).Append(' ').Append(T("atr"))
      .Append(" = ").Append(FormatearNumero(total))
      .Append(" | ").Append(T("vs")).Append(' ').Append(T("DC")).Append(' ').Append(FormatearNumero(dificultad))
      .Append(" -> ").Append(ResaltarResultado(textoResultado, outcome, colorearResultado));

    return sb.ToString();
  }

  public static string EventoEstado(string mensaje)
  {
    return Evento(T("ESTADO"), ColorEstado, mensaje);
  }

  public static string EventoBuff(string mensaje, bool esBuff)
  {
    return Evento(esBuff ? T("BUFF") : T("DEBUFF"), esBuff ? ColorBuff : ColorDebuff, mensaje);
  }

  public static string EventoValour(string mensaje)
  {
    return Evento("VAL", ColorValour, mensaje);
  }

  public static string EventoTrampa(string mensaje)
  {
    return Evento(T("TRAMPA"), ColorTrampa, mensaje);
  }

  public static string EventoDanio(string mensaje)
  {
    return Evento(T("DAÑO"), ColorDanio, mensaje);
  }

  public static string EventoMuerte(string mensaje)
  {
    return Evento(T("MUERTE"), ColorMuerte, mensaje);
  }

  public static string EventoCuracion(string mensaje)
  {
    return Evento(T("CURACION"), ColorCuracion, mensaje);
  }

  public static string Etiqueta(string texto, string colorHex)
  {
    return $"<color={colorHex}>[{texto}]</color>";
  }

  private static string Evento(string etiqueta, string colorHex, string mensaje)
  {
    return $"{Etiqueta(etiqueta, colorHex)} {mensaje}";
  }

  private static string ResaltarResultado(string textoResultado, CombatOutcome outcome, bool colorearResultado = true)
  {
    if (!colorearResultado)
    {
      return $"<b>{T(textoResultado)}</b>";
    }

    string color = outcome switch
    {
      CombatOutcome.Pifia => ColorFallo,
      CombatOutcome.Fallo => ColorFallo,
      CombatOutcome.Roce => ColorRoce,
      CombatOutcome.Critico => ColorCritico,
      CombatOutcome.Golpe => ColorExito,
      CombatOutcome.Exito => ColorExito,
      _ => ColorExito
    };

    return $"<b><color={color}>{T(textoResultado)}</color></b>";
  }

  private static string FormatearNumero(float valor)
  {
    return Math.Abs(valor % 1) < 0.01f ? valor.ToString("0") : valor.ToString("0.##");
  }

  private static string FormatearNumeroConSigno(float valor)
  {
    return Math.Abs(valor % 1) < 0.01f ? valor.ToString("+0;-0;+0") : valor.ToString("+0.##;-0.##;+0.##");
  }
}

