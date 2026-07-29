#if UNITY_EDITOR
using System.IO;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Setup;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class RuntimeAnalyticsSetupEditor
{
    private const string RuntimeSettingsDirectory = "Assets/Resources/Analytics";
    private const string RuntimeSettingsPath = RuntimeSettingsDirectory + "/RuntimeAnalyticsSettings.asset";
    private const string GameAnalyticsSettingsDirectory = "Assets/Resources/GameAnalytics";
    private const string GameAnalyticsSettingsPath = GameAnalyticsSettingsDirectory + "/Settings.asset";

    static RuntimeAnalyticsSetupEditor()
    {
        EditorApplication.delayCall += EnsureAnalyticsSetup;
    }

    public static void EnsureConfigured()
    {
        EnsureAnalyticsSetup();
    }

    [MenuItem("Tools/Metrics/Open Runtime Settings")]
    [MenuItem("Tools/Analytics/Open Runtime Settings")]
    private static void OpenRuntimeSettings()
    {
        EnsureAnalyticsSetup();
        SelectAndFocusAsset(
            AssetDatabase.LoadAssetAtPath<RuntimeAnalyticsSettings>(RuntimeSettingsPath),
            "Runtime metrics settings");
    }

    [MenuItem("Tools/Metrics/Open GameAnalytics Settings")]
    [MenuItem("Tools/Analytics/Open GameAnalytics Settings")]
    private static void OpenGameAnalyticsSettings()
    {
        EnsureAnalyticsSetup();
        SelectAndFocusAsset(
            AssetDatabase.LoadAssetAtPath<Settings>(GameAnalyticsSettingsPath),
            "GameAnalytics settings");
    }

    [MenuItem("Tools/Metrics/Open GameAnalytics Dashboard")]
    [MenuItem("Tools/Analytics/Open GameAnalytics Dashboard")]
    private static void OpenGameAnalyticsDashboard()
    {
        Application.OpenURL("https://tool.gameanalytics.com/");
    }

    [MenuItem("Tools/Metrics/Validate Setup")]
    [MenuItem("Tools/Analytics/Validate Setup")]
    public static void ValidateAnalyticsSetup()
    {
        EnsureAnalyticsSetup();
        Settings settings = AssetDatabase.LoadAssetAtPath<Settings>(GameAnalyticsSettingsPath);
        int platformIndex = settings == null ? -1 : settings.Platforms.IndexOf(RuntimePlatform.WindowsPlayer);
        bool hasCredentials = platformIndex >= 0 &&
                              !string.IsNullOrWhiteSpace(settings.GetGameKey(platformIndex)) &&
                              !string.IsNullOrWhiteSpace(settings.GetSecretKey(platformIndex));

        Debug.Log(hasCredentials
            ? "[Metrics] Configuracion lista para enviar eventos desde Windows."
            : "[Metrics] Falta vincular Game Key y Secret Key para Windows.");
    }

    private static void EnsureAnalyticsSetup()
    {
        if (!Directory.Exists(RuntimeSettingsDirectory))
        {
            Directory.CreateDirectory(RuntimeSettingsDirectory);
        }

        RuntimeAnalyticsSettings settings = AssetDatabase.LoadAssetAtPath<RuntimeAnalyticsSettings>(RuntimeSettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<RuntimeAnalyticsSettings>();
            AssetDatabase.CreateAsset(settings, RuntimeSettingsPath);
        }

        EnsureGameAnalyticsSettings();
    }

    private static void EnsureGameAnalyticsSettings()
    {
        if (!Directory.Exists(GameAnalyticsSettingsDirectory))
        {
            Directory.CreateDirectory(GameAnalyticsSettingsDirectory);
        }

        Settings settings = AssetDatabase.LoadAssetAtPath<Settings>(GameAnalyticsSettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<Settings>();
            AssetDatabase.CreateAsset(settings, GameAnalyticsSettingsPath);
        }

        bool changed = false;
        int platformIndex = settings.Platforms.IndexOf(RuntimePlatform.WindowsPlayer);
        if (platformIndex < 0)
        {
            settings.AddPlatform(RuntimePlatform.WindowsPlayer);
            platformIndex = settings.Platforms.Count - 1;
            changed = true;
        }

        string buildVersion = string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion)
            ? "0.1"
            : PlayerSettings.bundleVersion;
        if (settings.Build[platformIndex] != buildVersion)
        {
            settings.Build[platformIndex] = buildVersion;
            changed = true;
        }

        changed |= AddUnique(settings.ResourceCurrencies, "gold");
        changed |= AddUnique(settings.ResourceCurrencies, "materials");
        changed |= AddUnique(settings.ResourceItemTypes, "merchant_item");
        changed |= AddUnique(settings.ResourceItemTypes, "caravan_upgrade");
        changed |= AddUnique(settings.ResourceItemTypes, "battle_reward");

        if (settings.InfoLogBuild)
        {
            settings.InfoLogBuild = false;
            changed = true;
        }

        if (settings.VerboseLogBuild)
        {
            settings.VerboseLogBuild = false;
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(settings);
        }

        AssetDatabase.SaveAssets();
    }

    private static bool AddUnique(System.Collections.Generic.List<string> values, string value)
    {
        if (values.Contains(value))
        {
            return false;
        }

        values.Add(value);
        return true;
    }

    private static void SelectAndFocusAsset(UnityEngine.Object asset, string label)
    {
        if (asset == null)
        {
            Debug.LogWarning("Metrics setup could not find the requested asset.");
            return;
        }

        EditorApplication.ExecuteMenuItem("Window/General/Inspector");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);

        string assetPath = AssetDatabase.GetAssetPath(asset);
        if (!string.IsNullOrWhiteSpace(assetPath))
        {
            Debug.Log($"Opened {label}: {assetPath}");
        }
    }
}
#endif
