using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialLocalizedText
{
  public string key;
  [TextArea(1, 8)] public string es;
  [TextArea(1, 8)] public string en;
  [TextArea(1, 8)] public string pt;
}

[CreateAssetMenu(menuName = "GDD/Tutorial/Localization Table", fileName = "TutorialLocalizationTable")]
public class TutorialLocalizationTable : ScriptableObject
{
  public List<TutorialLocalizedText> texts = new List<TutorialLocalizedText>();

  public string Get(string key)
  {
    if (string.IsNullOrEmpty(key))
    {
      return string.Empty;
    }

    TutorialLocalizedText entry = Find(key);
    if (entry == null)
    {
      return key;
    }

    int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
    if (idioma == TRADU.IdiomaIngles && !string.IsNullOrEmpty(entry.en))
    {
      return entry.en;
    }

    if (idioma == TRADU.IdiomaPortugues && !string.IsNullOrEmpty(entry.pt))
    {
      return entry.pt;
    }

    return !string.IsNullOrEmpty(entry.es) ? entry.es : key;
  }

  private TutorialLocalizedText Find(string key)
  {
    for (int i = 0; i < texts.Count; i++)
    {
      TutorialLocalizedText entry = texts[i];
      if (entry != null && entry.key == key)
      {
        return entry;
      }
    }

    return null;
  }
}
