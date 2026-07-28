using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialTooltipSize
{
  Chico,
  Grande
}

public enum TutorialTooltipSide
{
  Centro,
  Arriba,
  Abajo,
  Izquierda,
  Derecha,
  ArribaIzquierda,
  ArribaDerecha,
  AbajoIzquierda,
  AbajoDerecha
}

public enum TutorialTooltipPointerDirection
{
  Derecha,
  Izquierda,
  Arriba,
  Abajo
}

[Serializable]
public class TutorialTooltipDefinition
{
  public string id;
  public TutorialTooltipSize size = TutorialTooltipSize.Chico;
  public TutorialTooltipSide side = TutorialTooltipSide.Centro;
  public Vector2 panelOffset = Vector2.zero;
  [TextArea(2, 8)] public string textoEs;
  [TextArea(2, 8)] public string textoEn;
  [TextArea(2, 8)] public string textoPt;
  public bool showPointer;
  public Vector2 pointerOffset = Vector2.zero;
  public TutorialTooltipPointerDirection pointerDirection = TutorialTooltipPointerDirection.Derecha;
  public float pointerScale = 1f;

  public string GetText()
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
    if (idioma == TRADU.IdiomaIngles && !string.IsNullOrEmpty(textoEn))
    {
      return textoEn;
    }

    if (idioma == TRADU.IdiomaPortugues && !string.IsNullOrEmpty(textoPt))
    {
      return textoPt;
    }

    return !string.IsNullOrEmpty(textoEs) ? textoEs : id;
  }
}

[CreateAssetMenu(menuName = "GDD/Tutorial/Tooltip Catalog", fileName = "TutorialTooltipCatalog")]
public class TutorialTooltipCatalog : ScriptableObject
{
  public List<TutorialTooltipDefinition> tooltips = new List<TutorialTooltipDefinition>();

  public TutorialTooltipDefinition Find(string id)
  {
    if (string.IsNullOrWhiteSpace(id))
    {
      return null;
    }

    for (int i = 0; i < tooltips.Count; i++)
    {
      TutorialTooltipDefinition tooltip = tooltips[i];
      if (tooltip != null && tooltip.id == id)
      {
        return tooltip;
      }
    }

    return null;
  }
}

public static class TutorialTooltipProgress
{
  public const string PrefMostrarAyudas = "tutorial_tooltips_mostrar_ayudas";
  private const string CooldownSavePrefix = "__tooltip_cooldown_dia__:";

  private static readonly HashSet<string> vistos = new HashSet<string>();
  private static bool silenciados;
  private static bool playerPrefsCargados;
  private static int ultimoDiaMostrado;

  public static bool Silenciados
  {
    get
    {
      CargarPlayerPrefsSiHaceFalta();
      return silenciados;
    }
    private set
    {
      silenciados = value;
    }
  }

  public static bool MostrarAyudas
  {
    get { return !Silenciados; }
  }

  public static bool FueVisto(string id)
  {
    return !string.IsNullOrWhiteSpace(id) && vistos.Contains(id);
  }

  public static void MarcarVisto(string id)
  {
    if (!string.IsNullOrWhiteSpace(id))
    {
      vistos.Add(id);
    }
  }

  public static bool PuedeMostrarPorCooldown(int diaActual, int cooldownDias)
  {
    return ultimoDiaMostrado <= 0
      || Mathf.Max(1, diaActual) - ultimoDiaMostrado >= Mathf.Max(0, cooldownDias);
  }

  public static void RegistrarDiaMostrado(int diaActual)
  {
    ultimoDiaMostrado = Mathf.Max(1, diaActual);
  }

  public static void Silenciar()
  {
    SetMostrarAyudas(false);
  }

  public static void SetMostrarAyudas(bool mostrar)
  {
    playerPrefsCargados = true;
    Silenciados = !mostrar;
    PlayerPrefs.SetInt(PrefMostrarAyudas, mostrar ? 1 : 0);
    PlayerPrefs.Save();
  }

  public static void ResetearParaNuevaCampania()
  {
    CargarPlayerPrefsSiHaceFalta();
    vistos.Clear();
    ultimoDiaMostrado = 0;
  }

  public static void RestaurarDesdeSave(CampaignSaveData data)
  {
    vistos.Clear();
    ultimoDiaMostrado = 0;
    CargarPlayerPrefsSiHaceFalta();
    if (data != null && data.tutorialTooltipsSilenciados)
    {
      SetMostrarAyudas(false);
    }

    if (data == null || data.tutorialTooltipsVistos == null)
    {
      return;
    }

    for (int i = 0; i < data.tutorialTooltipsVistos.Count; i++)
    {
      string id = data.tutorialTooltipsVistos[i];
      if (TryLeerDiaCooldown(id, out int diaCooldown))
      {
        ultimoDiaMostrado = Mathf.Max(ultimoDiaMostrado, diaCooldown);
        continue;
      }

      if (!string.IsNullOrWhiteSpace(id))
      {
        vistos.Add(id);
      }
    }
  }

  public static void CopiarASave(CampaignSaveData data)
  {
    if (data == null)
    {
      return;
    }

    data.tutorialTooltipsSilenciados = Silenciados;
    data.tutorialTooltipsVistos = new List<string>(vistos);
    if (ultimoDiaMostrado > 0)
    {
      data.tutorialTooltipsVistos.Add(CooldownSavePrefix + ultimoDiaMostrado);
    }
  }

  private static bool TryLeerDiaCooldown(string valor, out int dia)
  {
    dia = 0;
    return !string.IsNullOrEmpty(valor)
      && valor.StartsWith(CooldownSavePrefix, StringComparison.Ordinal)
      && int.TryParse(valor.Substring(CooldownSavePrefix.Length), out dia)
      && dia > 0;
  }

  private static void CargarPlayerPrefsSiHaceFalta()
  {
    if (playerPrefsCargados)
    {
      return;
    }

    playerPrefsCargados = true;
    silenciados = PlayerPrefs.GetInt(PrefMostrarAyudas, 1) != 1;
  }
}
