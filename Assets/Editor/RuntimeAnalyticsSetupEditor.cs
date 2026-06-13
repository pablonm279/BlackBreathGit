#if UNITY_EDITOR
using System.IO;
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

    [MenuItem("Tools/Metrics/Open Runtime Settings")]
    [MenuItem("Tools/Analytics/Open Runtime Settings")]
    private static void OpenRuntimeSettings()
    {
        EnsureAnalyticsSetup();
        SelectAndFocusAsset(
            AssetDatabase.LoadAssetAtPath<RuntimeAnalyticsSettings>(RuntimeSettingsPath),
            "Runtime metrics settings");
    }

    private static void EnsureAnalyticsSetup()
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
