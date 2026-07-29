using UnityEngine;

internal static class InitialLanguageBootstrap
{
    private const string LanguagePlayerPrefsKey = "nIdioma";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void DetectWindowsLanguage()
    {
        if (PlayerPrefs.HasKey(LanguagePlayerPrefsKey))
        {
            return;
        }

        int language = Application.systemLanguage switch
        {
            SystemLanguage.Spanish => TRADU.IdiomaEspanol,
            SystemLanguage.Portuguese => TRADU.IdiomaPortugues,
            _ => TRADU.IdiomaIngles
        };

        PlayerPrefs.SetInt(LanguagePlayerPrefsKey, language);
        PlayerPrefs.Save();
    }
}
