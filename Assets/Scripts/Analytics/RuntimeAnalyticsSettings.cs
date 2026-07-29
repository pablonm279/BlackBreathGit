using UnityEngine;

[CreateAssetMenu(fileName = "RuntimeAnalyticsSettings", menuName = "Analytics/Runtime Analytics Settings")]
public sealed class RuntimeAnalyticsSettings : ScriptableObject
{
    [SerializeField] private bool enableAnalytics = true;
    [SerializeField] private bool disableInEditor = false;
    [SerializeField] private bool disableInDevelopmentBuild = false;
    [SerializeField] private bool logInitializationMessages = true;
    [SerializeField] private bool writeEventsToFile = true;
    [SerializeField] private bool logEventsToConsole = false;
    [SerializeField] private bool sendEventsToGameAnalytics = true;
    [SerializeField] private string eventLogFolderName = "Metrics";

    public bool EnableAnalytics => enableAnalytics;
    public bool DisableInEditor => disableInEditor;
    public bool DisableInDevelopmentBuild => disableInDevelopmentBuild;
    public bool LogInitializationMessages => logInitializationMessages;
    public bool WriteEventsToFile => writeEventsToFile;
    public bool LogEventsToConsole => logEventsToConsole;
    public bool SendEventsToGameAnalytics => sendEventsToGameAnalytics;
    public string EventLogFolderName => eventLogFolderName;
}
