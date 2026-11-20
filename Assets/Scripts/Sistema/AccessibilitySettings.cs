using System;
using UnityEngine;

// Opciones de accesibilidad del jugador. Cada efecto del juego consulta aca si debe atenuarse:
//   * ReduceShake        -> sacudida de camara y "hit stop" (BattleVisualJuice)
//   * ReduceFlashes      -> destellos de pantalla (ScreenFlash) y pulso de vida baja (UnidadLowHealthFx)
//   * ReduceCameraSway   -> vaiven ambiental de la camara (EdgePanCameraZ)
//   * EdgeScrolling      -> desplazamiento de la camara al acercar el mouse al borde (EdgePanCameraZ)
//   * ColorBlind         -> filtro de color del mundo del juego (mezclador de canales del ColorGrading)
//   * Contrast           -> contraste (comparte el valor "gfx_contrast" que ya leia VisualPolishRuntime)
//   * TextScale          -> escala global de texto (EscaladoFuentesGlobal)
public static class AccessibilitySettings
{
  public enum ColorBlindMode { Off = 0, Protanopia = 1, Deuteranopia = 2, Tritanopia = 3 }

  public const float ContrastMin = 0.5f;
  public const float ContrastMax = 1.5f;
  public const float ContrastDefault = 1f;

  private const string PrefColorBlind = "acc_colorblind";
  private const string PrefReduceShake = "acc_reduce_shake";
  private const string PrefReduceFlashes = "acc_reduce_flashes";
  private const string PrefReduceSway = "acc_reduce_sway";
  private const string PrefEdgeScrolling = "acc_edge_scrolling";
  private const string PrefContrast = "gfx_contrast"; // mismo valor que lee VisualPolishRuntime

  private static bool loaded;
  private static ColorBlindMode colorBlind;
  private static bool reduceShake;
  private static bool reduceFlashes;
  private static bool reduceSway;
  private static bool edgeScrolling = true;

  public static event Action Changed;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void ResetStaticState()
  {
    loaded = false;
  }

  public static ColorBlindMode ColorBlind
  {
    get { Load(); return colorBlind; }
    set { Load(); colorBlind = value; PlayerPrefs.SetInt(PrefColorBlind, (int)value); Commit(); }
  }

  public static bool ReduceShake
  {
    get { Load(); return reduceShake; }
    set { Load(); reduceShake = value; PlayerPrefs.SetInt(PrefReduceShake, value ? 1 : 0); Commit(); }
  }

  public static bool ReduceFlashes
  {
    get { Load(); return reduceFlashes; }
    set { Load(); reduceFlashes = value; PlayerPrefs.SetInt(PrefReduceFlashes, value ? 1 : 0); Commit(); }
  }

  public static bool ReduceCameraSway
  {
    get { Load(); return reduceSway; }
    set { Load(); reduceSway = value; PlayerPrefs.SetInt(PrefReduceSway, value ? 1 : 0); Commit(); }
  }

  public static bool EdgeScrolling
  {
    get { Load(); return edgeScrolling; }
    set { Load(); edgeScrolling = value; PlayerPrefs.SetInt(PrefEdgeScrolling, value ? 1 : 0); Commit(); }
  }

  public static float Contrast
  {
    get { return Mathf.Clamp(PlayerPrefs.GetFloat(PrefContrast, ContrastDefault), ContrastMin, ContrastMax); }
    set { PlayerPrefs.SetFloat(PrefContrast, Mathf.Clamp(value, ContrastMin, ContrastMax)); Commit(); }
  }

  public static float TextScale
  {
    get { return EscaladoFuentesGlobal.EscalaTextoActual; }
    set { EscaladoFuentesGlobal.EstablecerEscalaTexto(value); RaiseChanged(); }
  }

  public static void ResetAll()
  {
    Load();
    colorBlind = ColorBlindMode.Off;
    reduceShake = false;
    reduceFlashes = false;
    reduceSway = false;
    edgeScrolling = true;
    PlayerPrefs.DeleteKey(PrefColorBlind);
    PlayerPrefs.DeleteKey(PrefReduceShake);
    PlayerPrefs.DeleteKey(PrefReduceFlashes);
    PlayerPrefs.DeleteKey(PrefReduceSway);
    PlayerPrefs.DeleteKey(PrefEdgeScrolling);
    PlayerPrefs.DeleteKey(PrefContrast);
    EscaladoFuentesGlobal.EstablecerEscalaTexto(EscaladoFuentesGlobal.EscalaPorDefecto);
    Commit();
  }

  // Mezclador de canales (porcentajes de -200 a 200) para el ColorGrading, en el orden:
  // RedOut(R,G,B), GreenOut(R,G,B), BlueOut(R,G,B). Con el modo apagado es la identidad.
  //
  // Es una daltonizacion: M = I + S * (I - Sim), donde Sim simula la deficiencia (Vienot et al.) y S
  // reparte el error que el ojo no distingue hacia los canales que si distingue. Cada fila suma 1, por lo que
  // el blanco y los grises no cambian.
  public static float[] GetColorMixer(ColorBlindMode mode)
  {
    float[,] sim;
    float[,] shift;
    switch (mode)
    {
      case ColorBlindMode.Protanopia:
        sim = new float[,] { { 0.567f, 0.433f, 0f }, { 0.558f, 0.442f, 0f }, { 0f, 0.242f, 0.758f } };
        shift = new float[,] { { 0f, 0f, 0f }, { 0.7f, 1f, 0f }, { 0.7f, 0f, 1f } };
        break;
      case ColorBlindMode.Deuteranopia:
        sim = new float[,] { { 0.625f, 0.375f, 0f }, { 0.7f, 0.3f, 0f }, { 0f, 0.3f, 0.7f } };
        shift = new float[,] { { 0f, 0f, 0f }, { 0.7f, 1f, 0f }, { 0.7f, 0f, 1f } };
        break;
      case ColorBlindMode.Tritanopia:
        sim = new float[,] { { 0.95f, 0.05f, 0f }, { 0f, 0.433f, 0.567f }, { 0f, 0.475f, 0.525f } };
        shift = new float[,] { { 1f, 0f, 0.7f }, { 0f, 1f, 0.7f }, { 0f, 0f, 0f } };
        break;
      default:
        return new float[] { 100f, 0f, 0f, 0f, 100f, 0f, 0f, 0f, 100f };
    }

    float[] result = new float[9];
    for (int row = 0; row < 3; row++)
    {
      for (int col = 0; col < 3; col++)
      {
        float error = 0f;
        for (int k = 0; k < 3; k++)
        {
          float identityMinusSim = (k == col ? 1f : 0f) - sim[k, col];
          error += shift[row, k] * identityMinusSim;
        }

        float value = (row == col ? 1f : 0f) + error;
        result[row * 3 + col] = Mathf.Clamp(value * 100f, -200f, 200f);
      }
    }

    return result;
  }

  public static float[] GetColorMixer()
  {
    return GetColorMixer(ColorBlind);
  }

  // ---------------------------------------------------------------- internos

  private static void Load()
  {
    if (loaded) { return; }
    loaded = true;
    colorBlind = (ColorBlindMode)Mathf.Clamp(PlayerPrefs.GetInt(PrefColorBlind, 0), 0, 3);
    reduceShake = PlayerPrefs.GetInt(PrefReduceShake, 0) == 1;
    reduceFlashes = PlayerPrefs.GetInt(PrefReduceFlashes, 0) == 1;
    reduceSway = PlayerPrefs.GetInt(PrefReduceSway, 0) == 1;
    edgeScrolling = PlayerPrefs.GetInt(PrefEdgeScrolling, 1) == 1;
  }

  private static void Commit()
  {
    PlayerPrefs.Save();
    RaiseChanged();
  }

  private static void RaiseChanged()
  {
    Action handler = Changed;
    if (handler != null) { handler(); }
  }
}
