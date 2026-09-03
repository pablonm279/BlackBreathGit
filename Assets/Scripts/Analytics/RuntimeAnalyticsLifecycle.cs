using UnityEngine;
using UnityEngine.Profiling;

internal sealed class RuntimeAnalyticsLifecycle : MonoBehaviour
{
    private const float PerformanceSampleSeconds = 300f;

    private float elapsed;
    private int frames;

    public static void EnsureExists()
    {
        RuntimeAnalyticsLifecycle existing = Object.FindFirstObjectByType<RuntimeAnalyticsLifecycle>();
        if (existing != null)
        {
            return;
        }

        GameObject lifecycleObject = new GameObject("[Metrics] Runtime");
        Object.DontDestroyOnLoad(lifecycleObject);
        lifecycleObject.AddComponent<RuntimeAnalyticsLifecycle>();
    }

    public static void DestroyExisting()
    {
        RuntimeAnalyticsLifecycle existing = Object.FindFirstObjectByType<RuntimeAnalyticsLifecycle>();
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }
    }

    private void Update()
    {
        float delta = Time.unscaledDeltaTime;
        if (delta <= 0f)
        {
            return;
        }

        elapsed += delta;
        frames++;
        if (elapsed < PerformanceSampleSeconds)
        {
            return;
        }

        float averageFps = frames / elapsed;
        float allocatedMemoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
        float reservedMemoryMb = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
        RuntimeAnalytics.TrackPerformance(averageFps, allocatedMemoryMb, reservedMemoryMb);
        elapsed = 0f;
        frames = 0;
    }

    private void OnApplicationQuit()
    {
        RuntimeAnalytics.TrackBattleAbandoned("application_quit");
    }
}
