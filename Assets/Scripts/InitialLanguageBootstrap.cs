using UnityEngine;

internal static class InitialLanguageBootstrap
{
    private const string LanguagePlayerPrefsKey = "nIdioma";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void DetectWindowsLanguage()
    {
        if (PlayerPrefs.HasKey(LanguagePlayerPrefsKey))
        {
            int savedLanguage = PlayerPrefs.GetInt(LanguagePlayerPrefsKey);
            int normalizedLanguage = TRADU.NormalizarIdioma(savedLanguage);
            if (savedLanguage != normalizedLanguage)
            {
                PlayerPrefs.SetInt(LanguagePlayerPrefsKey, normalizedLanguage);
                PlayerPrefs.Save();
            }
            return;
        }

        int language = Application.systemLanguage switch
        {
            SystemLanguage.Spanish => TRADU.IdiomaEspanol,
            SystemLanguage.Portuguese => TRADU.IdiomaPortugues,
            SystemLanguage.ChineseSimplified when ChineseLocalization.IsAvailable => TRADU.IdiomaChinoSimplificado,
            _ => TRADU.IdiomaIngles
        };

        PlayerPrefs.SetInt(LanguagePlayerPrefsKey, language);
        PlayerPrefs.Save();
    }
}
