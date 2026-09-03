using System;
using UnityEngine;

public static class PlataformaRuntime
{
  private static bool? esSteamDeckCache;

  public static bool EsSteamDeck
  {
    get
    {
      if (!esSteamDeckCache.HasValue)
      {
        esSteamDeckCache = DetectarSteamDeck();
      }

      return esSteamDeckCache.Value;
    }
  }

  public static bool EsLinuxPlayer => Application.platform == RuntimePlatform.LinuxPlayer;

  public static float FrecuenciaPantallaHz
  {
    get
    {
      double frecuencia = Screen.currentResolution.refreshRateRatio.value;
      return frecuencia > 0d ? (float)frecuencia : 60f;
    }
  }

  public static bool UsaPantallaDeckDeAltaFrecuencia
  {
    get
    {
      Resolution resolucion = Screen.currentResolution;
      return EsSteamDeck
        && resolucion.width == 1280
        && resolucion.height == 800
        && FrecuenciaPantallaHz >= 80f;
    }
  }

  private static bool DetectarSteamDeck()
  {
    string variableSteamDeck = Environment.GetEnvironmentVariable("SteamDeck");
    if (string.Equals(variableSteamDeck, "1", StringComparison.Ordinal))
    {
      return true;
    }

    if (!EsLinuxPlayer)
    {
      return false;
    }

    string modelo = SystemInfo.deviceModel;
    return !string.IsNullOrEmpty(modelo)
      && (modelo.IndexOf("Steam Deck", StringComparison.OrdinalIgnoreCase) >= 0
        || modelo.IndexOf("Jupiter", StringComparison.OrdinalIgnoreCase) >= 0
        || modelo.IndexOf("Galileo", StringComparison.OrdinalIgnoreCase) >= 0);
  }
}
