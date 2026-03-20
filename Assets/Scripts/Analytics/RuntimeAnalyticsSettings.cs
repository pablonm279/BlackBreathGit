using UnityEngine;

[CreateAssetMenu(fileName = "RuntimeAnalyticsSettings", menuName = "Analytics/Runtime Analytics Settings")]
public sealed class RuntimeAnalyticsSettings : ScriptableObject
{
    [SerializeField] private bool enableAnalytics = true;
    [SerializeField] private bool disableInEditor = true;
    [SerializeField] private bool disableInDevelopmentBuild = false;
    [SerializeField] private bool logInitializationMessages = true;

    public bool EnableAnalytics => enableAnalytics;
    public bool DisableInEditor => disableInEditor;
    public bool DisableInDevelopmentBuild => disableInDevelopmentBuild;
    public bool LogInitializationMessages => logInitializationMessages;
}
