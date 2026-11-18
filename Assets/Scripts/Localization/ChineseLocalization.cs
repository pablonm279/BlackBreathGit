using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>Catálogo de chino simplificado. Las claves conservan el texto español o el ID del tutorial.</summary>
public static class ChineseLocalization
{
    public const string Locale = "zh-Hans";
    public const string ResourcePath = "Localization/zh-Hans";

    // Habilitar en distribución solamente después de completar traducción y LQA.
    public const bool ReleaseReady = true;
    public static bool IsAvailable => ReleaseReady || Application.isEditor || Debug.isDebugBuild;

    [Serializable]
    public sealed class Entry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public sealed class Catalog
    {
        public string locale;
        public Entry[] entries;
    }

    private static Dictionary<string, string> translations;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        translations = null;
    }

    public static bool TryTranslate(string source, out string translated)
    {
        translated = null;
        if (string.IsNullOrEmpty(source))
        {
            return false;
        }

        EnsureLoaded();
        return translations.TryGetValue(source.Normalize(NormalizationForm.FormC), out translated);
    }

    public static string Get(string key, string fallback)
    {
        return TryTranslate(key, out string translated) ? translated : fallback;
    }

    // Los campos de texto del Inspector pueden contener sangría o saltos de línea
    // distintos de los extraídos del YAML. Mantener la búsqueda exacta como prioridad.
    public static string GetDisplayText(string source, string fallback)
    {
        if (TryTranslate(source, out string translated)) return translated;
        if (string.IsNullOrWhiteSpace(source)) return fallback;

        string key = NormalizeDisplayKey(source);
        string candidate = null;
        foreach (var entry in translations)
        {
            if (NormalizeDisplayKey(entry.Key) != key) continue;
            // No elegir arbitrariamente entre dos textos con traducciones distintas.
            if (candidate != null && candidate != entry.Value) return fallback;
            candidate = entry.Value;
        }
        return candidate ?? fallback;
    }

    private static string NormalizeDisplayKey(string text)
    {
        return text.Normalize(NormalizationForm.FormC).Replace("\r\n", "\n").Replace('\r', '\n').Trim();
    }

    private static void EnsureLoaded()
    {
        if (translations != null)
        {
            return;
        }

        translations = new Dictionary<string, string>(StringComparer.Ordinal);
        TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
        if (asset == null)
        {
            Debug.LogError("[Localization] Falta el catálogo " + ResourcePath);
            return;
        }

        try
        {
            Catalog catalog = JsonUtility.FromJson<Catalog>(asset.text);
            if (catalog == null || catalog.locale != Locale || catalog.entries == null)
            {
                Debug.LogError("[Localization] Catálogo zh-Hans inválido.");
                return;
            }

            foreach (Entry entry in catalog.entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.key) || string.IsNullOrWhiteSpace(entry.value))
                {
                    continue;
                }

                string key = entry.key.Normalize(NormalizationForm.FormC);
                if (translations.ContainsKey(key))
                {
                    Debug.LogError("[Localization] Clave zh-Hans duplicada: " + key);
                    continue;
                }

                translations.Add(key, entry.value);
            }
        }
        catch (ArgumentException exception)
        {
            Debug.LogError("[Localization] No se pudo leer zh-Hans: " + exception.Message);
        }
    }
}
