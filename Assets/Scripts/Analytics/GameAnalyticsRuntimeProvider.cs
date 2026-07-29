using System;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;
using GameAnalyticsSDK.Setup;
using UnityEngine;

internal static class GameAnalyticsRuntimeProvider
{
    private const string TrackerObjectName = "[Metrics] GameAnalytics";

    private static bool initializationAttempted;
    private static bool ready;
    private static bool failureLogged;

    public static void Initialize(RuntimeAnalyticsSettings runtimeSettings)
    {
        if (initializationAttempted)
        {
            return;
        }

        initializationAttempted = true;

        if (runtimeSettings == null || !runtimeSettings.SendEventsToGameAnalytics || Application.isEditor)
        {
            return;
        }

        try
        {
            Settings settings = GameAnalytics.SettingsGA;
            int platformIndex = settings == null ? -1 : settings.Platforms.IndexOf(Application.platform);
            if (platformIndex < 0 ||
                string.IsNullOrWhiteSpace(settings.GetGameKey(platformIndex)) ||
                string.IsNullOrWhiteSpace(settings.GetSecretKey(platformIndex)))
            {
                LogFailureOnce("GameAnalytics no tiene credenciales para " + Application.platform + "; se mantiene el registro local.");
                return;
            }

            GameAnalytics tracker = UnityEngine.Object.FindFirstObjectByType<GameAnalytics>();
            if (tracker == null)
            {
                GameObject trackerObject = new GameObject(TrackerObjectName);
                trackerObject.AddComponent<GameAnalytics>();
            }

            GameAnalytics.Initialize();
            if (GameAnalytics.Initialized)
            {
                GameAnalytics.SetEnabledEventSubmission(true);
            }
            ready = GameAnalytics.Initialized;
        }
        catch (Exception exception)
        {
            LogFailureOnce("GameAnalytics no pudo inicializarse; se mantiene el registro local. " + exception.Message);
        }
    }

    public static void Disable()
    {
        ready = false;
        if (!GameAnalytics.Initialized)
        {
            return;
        }

        try
        {
            GameAnalytics.SetEnabledEventSubmission(false, false);
        }
        catch (Exception exception)
        {
            LogFailureOnce("GameAnalytics no pudo detener el envio de eventos. " + exception.Message);
        }
    }

    public static void TrackDesign(string eventName)
    {
        if (!ready || string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        TrySend(() => GameAnalytics.NewDesignEvent(eventName, 1f));
    }

    public static void TrackDesign(string eventName, float value)
    {
        if (!ready || string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        TrySend(() => GameAnalytics.NewDesignEvent(eventName, value));
    }

    public static void TrackProgression(
        string status,
        string progression01,
        string progression02,
        string progression03)
    {
        if (!ready || string.IsNullOrWhiteSpace(progression01))
        {
            return;
        }

        GAProgressionStatus progressionStatus;
        switch (status)
        {
            case "start":
                progressionStatus = GAProgressionStatus.Start;
                break;
            case "complete":
                progressionStatus = GAProgressionStatus.Complete;
                break;
            case "fail":
                progressionStatus = GAProgressionStatus.Fail;
                break;
            default:
                return;
        }

        if (!string.IsNullOrWhiteSpace(progression03))
        {
            TrySend(() => GameAnalytics.NewProgressionEvent(
                progressionStatus,
                progression01,
                progression02,
                progression03));
        }
        else if (!string.IsNullOrWhiteSpace(progression02))
        {
            TrySend(() => GameAnalytics.NewProgressionEvent(
                progressionStatus,
                progression01,
                progression02));
        }
        else
        {
            TrySend(() => GameAnalytics.NewProgressionEvent(progressionStatus, progression01));
        }
    }

    public static void TrackResource(
        string flowType,
        string currency,
        float amount,
        string itemType,
        string itemId)
    {
        if (!ready || amount <= 0f)
        {
            return;
        }

        GAResourceFlowType resourceFlowType;
        switch (flowType)
        {
            case "source":
                resourceFlowType = GAResourceFlowType.Source;
                break;
            case "sink":
                resourceFlowType = GAResourceFlowType.Sink;
                break;
            default:
                return;
        }

        TrySend(() => GameAnalytics.NewResourceEvent(
            resourceFlowType,
            currency,
            amount,
            itemType,
            itemId));
    }

    private static void TrySend(Action sendEvent)
    {
        try
        {
            sendEvent();
        }
        catch (Exception exception)
        {
            LogFailureOnce("GameAnalytics rechazo un evento; el registro local sigue activo. " + exception.Message);
        }
    }

    private static void LogFailureOnce(string message)
    {
        if (failureLogged)
        {
            return;
        }

        failureLogged = true;
        Debug.LogWarning("[Metrics] " + message);
    }
}
