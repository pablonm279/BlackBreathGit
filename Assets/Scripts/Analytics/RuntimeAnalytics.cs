using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GameAnalyticsSDK;
using UnityEngine;

public static class RuntimeAnalytics
{
    private const string RuntimeSettingsResourcePath = "Analytics/RuntimeAnalyticsSettings";
    private static RuntimeAnalyticsSettings cachedSettings;
    private static bool initializeAttempted;
    private static bool providerReady;
    private static bool stateLogged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (initializeAttempted)
        {
            return;
        }

        initializeAttempted = true;

        if (!CanSendEvents(out string reason))
        {
            LogState(reason);
            return;
        }

        if (!HasCurrentPlatformKeys(out reason))
        {
            LogState(reason);
            return;
        }

        EnsureGameAnalyticsObject();
        GameAnalytics.Initialize();
        providerReady = true;
        LogState("GameAnalytics inicializado.");
    }

    public static void TrackDesign(params string[] parts)
    {
        if (!EnsureProviderReady())
        {
            return;
        }

        string eventName = BuildEventName(parts);
        if (string.IsNullOrEmpty(eventName))
        {
            return;
        }

        GameAnalytics.NewDesignEvent(eventName);
    }

    public static void TrackProgressionStart(string progression01, string progression02 = null, string progression03 = null)
    {
        TrackProgression(GAProgressionStatus.Start, progression01, progression02, progression03);
    }

    public static void TrackProgressionComplete(string progression01, string progression02 = null, string progression03 = null)
    {
        TrackProgression(GAProgressionStatus.Complete, progression01, progression02, progression03);
    }

    public static void TrackProgressionFail(string progression01, string progression02 = null, string progression03 = null)
    {
        TrackProgression(GAProgressionStatus.Fail, progression01, progression02, progression03);
    }

    public static void TrackResourceSource(string currency, float amount, string itemType, string itemId)
    {
        TrackResource(GAResourceFlowType.Source, currency, amount, itemType, itemId);
    }

    public static void TrackResourceSink(string currency, float amount, string itemType, string itemId)
    {
        TrackResource(GAResourceFlowType.Sink, currency, amount, itemType, itemId);
    }

    public static string SanitizeToken(string value, int maxLength = 32)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        string normalized = value.Trim().Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder(normalized.Length);
        bool underscorePending = false;

        for (int i = 0; i < normalized.Length; i++)
        {
            char c = normalized[i];
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                if (underscorePending && builder.Length > 0)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
                underscorePending = false;
            }
            else
            {
                underscorePending = builder.Length > 0;
            }

            if (builder.Length >= maxLength)
            {
                break;
            }
        }

        string result = builder.ToString().Trim('_');
        return string.IsNullOrEmpty(result) ? "unknown" : result;
    }

    public static string BoolToken(bool value)
    {
        return value ? "on" : "off";
    }

    public static string ClassToken(Personaje personaje)
    {
        return personaje == null ? "class_0" : "class_" + Mathf.Max(0, personaje.IDClase);
    }

    public static string AbilityToken(Habilidad habilidad)
    {
        return habilidad == null ? "none" : SanitizeToken(habilidad.GetType().Name);
    }

    public static string ActivityToken(Actividad actividad)
    {
        return actividad == null ? "none" : SanitizeToken(actividad.GetType().Name);
    }

    public static string ItemToken(Item item)
    {
        if (item == null)
        {
            return "none";
        }

        string detail = !string.IsNullOrWhiteSpace(item.GetPersistentItemId())
            ? item.GetPersistentItemId()
            : item.sNombreItem;
        string kind = ItemKind(item);
        string token = SanitizeToken(detail, 24);
        return string.IsNullOrEmpty(token) ? kind : kind + "_" + token;
    }

    public static string ItemKind(Item item)
    {
        if (item is Arma) { return "weapon"; }
        if (item is Armadura) { return "armor"; }
        if (item is Accesorio) { return "accessory"; }
        if (item is Consumible) { return "consumable"; }
        return "item";
    }

    private static bool EnsureProviderReady()
    {
        if (!providerReady)
        {
            Initialize();
        }

        return providerReady;
    }

    private static bool CanSendEvents(out string reason)
    {
        RuntimeAnalyticsSettings settings = GetSettings();
        if (settings != null && !settings.EnableAnalytics)
        {
            reason = "Analytics deshabilitado en RuntimeAnalyticsSettings.";
            return false;
        }

        if (Application.isEditor && (settings == null || settings.DisableInEditor))
        {
            reason = "Analytics deshabilitado dentro del editor.";
            return false;
        }

#if DEVELOPMENT_BUILD
        if (!Application.isEditor && settings != null && settings.DisableInDevelopmentBuild)
        {
            reason = "Analytics deshabilitado en development build.";
            return false;
        }
#endif

        reason = string.Empty;
        return true;
    }

    private static RuntimeAnalyticsSettings GetSettings()
    {
        if (cachedSettings == null)
        {
            cachedSettings = Resources.Load<RuntimeAnalyticsSettings>(RuntimeSettingsResourcePath);
        }

        return cachedSettings;
    }

    private static bool HasCurrentPlatformKeys(out string reason)
    {
        var settings = GameAnalytics.SettingsGA;
        if (settings == null)
        {
            reason = "No existe Assets/Resources/GameAnalytics/Settings.asset.";
            return false;
        }

        int platformIndex = settings.Platforms.IndexOf(Application.platform);
        if (platformIndex < 0)
        {
            reason = "La plataforma actual no esta configurada en GameAnalytics Settings.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.GetGameKey(platformIndex)) || string.IsNullOrWhiteSpace(settings.GetSecretKey(platformIndex)))
        {
            reason = "Faltan Game Key y Secret Key para la plataforma actual en GameAnalytics Settings.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static void EnsureGameAnalyticsObject()
    {
#if UNITY_2023_1_OR_NEWER
        GameAnalytics ga = Object.FindFirstObjectByType<GameAnalytics>();
#else
        GameAnalytics ga = Object.FindObjectOfType<GameAnalytics>();
#endif
        if (ga != null)
        {
            return;
        }

        GameObject analyticsObject = new GameObject("GameAnalytics");
        Object.DontDestroyOnLoad(analyticsObject);
        analyticsObject.AddComponent<GameAnalytics>();
    }

    private static void TrackProgression(GAProgressionStatus status, string progression01, string progression02, string progression03)
    {
        if (!EnsureProviderReady())
        {
            return;
        }

        string p1 = SanitizeToken(progression01);
        string p2 = string.IsNullOrWhiteSpace(progression02) ? null : SanitizeToken(progression02);
        string p3 = string.IsNullOrWhiteSpace(progression03) ? null : SanitizeToken(progression03);

        if (string.IsNullOrEmpty(p2))
        {
            GameAnalytics.NewProgressionEvent(status, p1);
        }
        else if (string.IsNullOrEmpty(p3))
        {
            GameAnalytics.NewProgressionEvent(status, p1, p2);
        }
        else
        {
            GameAnalytics.NewProgressionEvent(status, p1, p2, p3);
        }
    }

    private static void TrackResource(GAResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
    {
        if (amount <= 0f || !EnsureProviderReady())
        {
            return;
        }

        string safeCurrency = SanitizeToken(currency, 24);
        string safeItemType = SanitizeToken(itemType, 32);
        string safeItemId = SanitizeToken(itemId, 32);
        GameAnalytics.NewResourceEvent(flowType, safeCurrency, amount, safeItemType, safeItemId);
    }

    private static string BuildEventName(params string[] parts)
    {
        List<string> tokens = new List<string>();
        if (parts != null)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(parts[i]))
                {
                    continue;
                }

                tokens.Add(SanitizeToken(parts[i]));
            }
        }

        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        const int maxSegments = 5;
        if (tokens.Count > maxSegments)
        {
            List<string> trimmed = new List<string>(maxSegments);
            for (int i = 0; i < maxSegments - 1; i++)
            {
                trimmed.Add(tokens[i]);
            }

            StringBuilder overflow = new StringBuilder();
            for (int i = maxSegments - 1; i < tokens.Count; i++)
            {
                if (overflow.Length > 0)
                {
                    overflow.Append('_');
                }

                overflow.Append(tokens[i]);
            }

            trimmed.Add(SanitizeToken(overflow.ToString()));
            tokens = trimmed;
        }

        return string.Join(":", tokens);
    }

    private static void LogState(string message)
    {
        RuntimeAnalyticsSettings settings = GetSettings();
        if (stateLogged || (settings != null && !settings.LogInitializationMessages))
        {
            return;
        }

        stateLogged = true;
        Debug.Log("[Analytics] " + message);
    }
}
