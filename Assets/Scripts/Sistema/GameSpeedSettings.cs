using System;
using UnityEngine;

// Ajustes de ritmo de juego: velocidad de combate y velocidad de la caravana en el mapa.
// Los consulta BattleManager / IAUnidad (combate) y Nodo (viaje). El tutorial ignora estos ajustes.
public static class GameSpeedSettings
{
  public const string PrefModoRapido = "modoRapido";             // 0/1: el boton de batalla y las opciones lo comparten
  public const string PrefNivelCombate = "velocidadCombateNivel"; // 1 = rapida, 2 = muy rapida
  public const string PrefSoloEnemigos = "velocidadSoloEnemigos"; // 0/1: acelerar solo los turnos enemigos
  public const string PrefCaravana = "velocidadCaravana";         // 0 = normal, 1 = rapida, 2 = muy rapida

  public const int NivelCombateRapido = 1;
  public const int NivelCombateMuyRapido = 2;
  public const int NivelCaravanaMax = 2;

  private const float EscalaTiempoRapida = 1.35f;
  private const float EscalaTiempoMuyRapida = 1.9f;
  private const float FactorIARapida = 0.65f;
  private const float FactorIAMuyRapida = 0.45f;
  private static readonly float[] MultiplicadorCaravana = { 1f, 1.75f, 2.5f };

  public static event Action Changed;

  private static int nivelCombate = -1;
  private static int soloEnemigos = -1;
  private static int caravana = -1;

  // Nivel de velocidad cuando el modo rapido esta activo (1 o 2).
  public static int CombatLevel
  {
    get
    {
      if (nivelCombate < 0)
      {
        nivelCombate = Mathf.Clamp(PlayerPrefs.GetInt(PrefNivelCombate, NivelCombateRapido), NivelCombateRapido, NivelCombateMuyRapido);
      }
      return nivelCombate;
    }
    set
    {
      nivelCombate = Mathf.Clamp(value, NivelCombateRapido, NivelCombateMuyRapido);
      PlayerPrefs.SetInt(PrefNivelCombate, nivelCombate);
      PlayerPrefs.Save();
      Changed?.Invoke();
    }
  }

  public static bool CombatOnlyEnemies
  {
    get
    {
      if (soloEnemigos < 0)
      {
        soloEnemigos = PlayerPrefs.GetInt(PrefSoloEnemigos, 0) == 1 ? 1 : 0;
      }
      return soloEnemigos == 1;
    }
    set
    {
      soloEnemigos = value ? 1 : 0;
      PlayerPrefs.SetInt(PrefSoloEnemigos, soloEnemigos);
      PlayerPrefs.Save();
      Changed?.Invoke();
    }
  }

  public static bool CombatFastEnabled
  {
    get { return PlayerPrefs.GetInt(PrefModoRapido, 0) == 1; }
  }

  // Indice para el desplegable: 0 = normal, 1 = rapida, 2 = muy rapida.
  public static int CombatSpeedIndex
  {
    get { return CombatFastEnabled ? CombatLevel : 0; }
  }

  public static void SetCombatSpeedIndex(int indice)
  {
    indice = Mathf.Clamp(indice, 0, NivelCombateMuyRapido);
    if (indice > 0)
    {
      nivelCombate = indice;
      PlayerPrefs.SetInt(PrefNivelCombate, nivelCombate);
    }
    PlayerPrefs.SetInt(PrefModoRapido, indice > 0 ? 1 : 0);
    PlayerPrefs.Save();

    // Si hay una batalla en curso, se actualiza el boton y la escala de tiempo al instante.
    BattleManager batalla = BattleManager.Instance;
    if (batalla != null)
    {
      batalla.ActivarModoRapido(indice > 0);
    }

    Changed?.Invoke();
  }

  // El boton de la batalla puede cambiar el modo rapido por su cuenta: las opciones se resincronizan.
  public static void NotificarCambioModoRapido()
  {
    Changed?.Invoke();
  }

  public static float CombatTimeScale(int nivel)
  {
    return nivel >= NivelCombateMuyRapido ? EscalaTiempoMuyRapida : EscalaTiempoRapida;
  }

  public static float CombatAiTimingFactor(int nivel)
  {
    return nivel >= NivelCombateMuyRapido ? FactorIAMuyRapida : FactorIARapida;
  }

  public static int CaravanLevel
  {
    get
    {
      if (caravana < 0)
      {
        caravana = Mathf.Clamp(PlayerPrefs.GetInt(PrefCaravana, 0), 0, NivelCaravanaMax);
      }
      return caravana;
    }
    set
    {
      caravana = Mathf.Clamp(value, 0, NivelCaravanaMax);
      PlayerPrefs.SetInt(PrefCaravana, caravana);
      PlayerPrefs.Save();
      Changed?.Invoke();
    }
  }

  // Multiplicador de la velocidad de viaje en el mapa. El tiempo de juego que transcurre se escala igual
  // (ver Nodo.MoverConvoyEnLinea), asi que el viaje consume las mismas horas en menos tiempo real.
  public static float CaravanMultiplier
  {
    get { return MultiplicadorCaravana[CaravanLevel]; }
  }
}
