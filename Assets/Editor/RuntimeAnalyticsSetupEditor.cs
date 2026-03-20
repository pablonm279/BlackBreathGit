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

    static RuntimeAnalyticsSetupEditor()
    {
        EditorApplication.delayCall += EnsureAnalyticsSetup;
    }

    [MenuItem("Tools/Analytics/Open Runtime Settings")]
    [MenuItem("Window/GameAnalytics/Open Runtime Analytics Settings", false, 210)]
    private static void OpenRuntimeSettings()
    {
        EnsureAnalyticsSetup();
        SelectAndFocusAsset(
            AssetDatabase.LoadAssetAtPath<RuntimeAnalyticsSettings>(RuntimeSettingsPath),
            "Runtime Analytics settings");
    }

    [MenuItem("Tools/Analytics/Open GameAnalytics Settings")]
    [MenuItem("Window/GameAnalytics/Select Settings", false, 200)]
    private static void OpenGameAnalyticsSettings()
    {
        EnsureAnalyticsSetup();
        SelectAndFocusAsset(GameAnalytics.SettingsGA, "GameAnalytics settings");
    }

    private static void EnsureAnalyticsSetup()
    {
        EnsureRuntimeSettingsAsset();
        EnsureGameAnalyticsSettingsAsset();
    }

    private static void EnsureRuntimeSettingsAsset()
    {
        if (!Directory.Exists(RuntimeSettingsDirectory))
        {
            Directory.CreateDirectory(RuntimeSettingsDirectory);
        }

        RuntimeAnalyticsSettings settings = AssetDatabase.LoadAssetAtPath<RuntimeAnalyticsSettings>(RuntimeSettingsPath);
        if (settings != null)
        {
            return;
        }

        settings = ScriptableObject.CreateInstance<RuntimeAnalyticsSettings>();
        AssetDatabase.CreateAsset(settings, RuntimeSettingsPath);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureGameAnalyticsSettingsAsset()
    {
        Settings settings = GameAnalytics.SettingsGA;
        if (settings == null)
        {
            return;
        }

        bool dirty = false;
        dirty |= EnsurePlatform(settings, RuntimePlatform.WindowsPlayer);
        dirty |= EnsurePlatform(settings, RuntimePlatform.OSXPlayer);
        dirty |= EnsurePlatform(settings, RuntimePlatform.LinuxPlayer);

        dirty |= EnsureString(settings.ResourceCurrencies, "gold");
        dirty |= EnsureString(settings.ResourceCurrencies, "materials");

        dirty |= EnsureString(settings.ResourceItemTypes, "caravan_upgrade");
        dirty |= EnsureString(settings.ResourceItemTypes, "merchant_item");
        dirty |= EnsureString(settings.ResourceItemTypes, "battle_reward");

        string buildVersion = string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion) ? "0.1.0" : PlayerSettings.bundleVersion;
        for (int i = 0; i < settings.Build.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(settings.Build[i]))
            {
                settings.Build[i] = buildVersion;
                dirty = true;
            }
        }

        if (settings.IntroScreen)
        {
            settings.IntroScreen = false;
            dirty = true;
        }

        if (settings.SubmitErrors)
        {
            settings.SubmitErrors = false;
            dirty = true;
        }

        if (settings.EnableSDKInitEvent)
        {
            settings.EnableSDKInitEvent = false;
            dirty = true;
        }

        if (settings.EnableFPSHistogram)
        {
            settings.EnableFPSHistogram = false;
            dirty = true;
        }

        if (settings.EnableMemoryHistogram)
        {
            settings.EnableMemoryHistogram = false;
            dirty = true;
        }

        if (settings.EnableHardwareTracking)
        {
            settings.EnableHardwareTracking = false;
            dirty = true;
        }

        if (dirty)
        {
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }

    private static bool EnsurePlatform(Settings settings, RuntimePlatform platform)
    {
        if (settings.Platforms.Contains(platform))
        {
            return false;
        }

        settings.AddPlatform(platform);
        return true;
    }

    private static bool EnsureString(System.Collections.Generic.List<string> values, string value)
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
            Debug.LogWarning("Analytics setup could not find the requested asset.");
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
