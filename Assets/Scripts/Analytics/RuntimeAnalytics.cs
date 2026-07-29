using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class RuntimeAnalytics
{
    public const string TelemetryEnabledPlayerPrefsKey = "Telemetry_Enabled";

    private const string RuntimeSettingsResourcePath = "Analytics/RuntimeAnalyticsSettings";
    private const string DefaultLogFolderName = "Metrics";

    private static readonly string sessionId = Guid.NewGuid().ToString("N");
    private static RuntimeAnalyticsSettings cachedSettings;
    private static bool initializeAttempted;
    private static bool metricsReady;
    private static bool stateLogged;
    private static bool fileWriteFailed;
    private static string logFilePath;
    private static bool battleActive;
    private static string battleType = "unknown";
    private static string battleEncounter = "unknown";
    private static double battleStartedAt;
    private static int battleTurns;
    private static int battleMaxRound;
    private static float battleDamageDealt;
    private static float battleDamageReceived;
    private static float battleHealingReceived;
    private static int battleEnemiesDown;
    private static int battleAlliesDown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (!IsTelemetryEnabled)
        {
            return;
        }

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

        RuntimeAnalyticsSettings settings = GetSettings();
        if (settings == null || settings.WriteEventsToFile)
        {
            PrepareLogFile(settings);
        }

        GameAnalyticsRuntimeProvider.Initialize(settings);
        metricsReady = true;
        RuntimeAnalyticsLifecycle.EnsureExists();
        LogState("Metricas runtime inicializadas.");
    }

    public static bool IsTelemetryEnabled =>
        PlayerPrefs.GetInt(TelemetryEnabledPlayerPrefsKey, 0) == 1;

    public static void SetTelemetryEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(TelemetryEnabledPlayerPrefsKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (enabled)
        {
            Initialize();
            return;
        }

        metricsReady = false;
        ResetBattle();
        GameAnalyticsRuntimeProvider.Disable();
        RuntimeAnalyticsLifecycle.DestroyExisting();
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

        Emit(new MetricEvent
        {
            type = "design",
            name = eventName,
            hasValue = true,
            value = 1f
        });
        GameAnalyticsRuntimeProvider.TrackDesign(eventName);
    }

    public static void TrackDesignValue(float value, params string[] parts)
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

        Emit(new MetricEvent
        {
            type = "design",
            name = eventName,
            hasValue = true,
            value = value
        });
        GameAnalyticsRuntimeProvider.TrackDesign(eventName, value);
    }

    public static void TrackProgressionStart(string progression01, string progression02 = null, string progression03 = null)
    {
        TrackProgression("start", progression01, progression02, progression03);
    }

    public static void TrackProgressionComplete(string progression01, string progression02 = null, string progression03 = null)
    {
        TrackProgression("complete", progression01, progression02, progression03);
    }

    public static void TrackProgressionFail(string progression01, string progression02 = null, string progression03 = null)
    {
        TrackProgression("fail", progression01, progression02, progression03);
    }

    public static void TrackResourceSource(string currency, float amount, string itemType, string itemId)
    {
        TrackResource("source", currency, amount, itemType, itemId);
    }

    public static void TrackResourceSink(string currency, float amount, string itemType, string itemId)
    {
        TrackResource("sink", currency, amount, itemType, itemId);
    }

    public static void BeginBattle(string type, string encounter)
    {
        if (!EnsureProviderReady())
        {
            return;
        }

        if (battleActive)
        {
            TrackBattleAbandoned("replaced");
        }

        battleActive = true;
        battleType = SanitizeToken(type);
        battleEncounter = SanitizeToken(encounter);
        battleStartedAt = Time.realtimeSinceStartupAsDouble;
        battleTurns = 0;
        battleMaxRound = 0;
        battleDamageDealt = 0f;
        battleDamageReceived = 0f;
        battleHealingReceived = 0f;
        battleEnemiesDown = 0;
        battleAlliesDown = 0;
    }

    public static void RecordBattleTurn(int round)
    {
        if (!battleActive)
        {
            return;
        }

        battleTurns++;
        battleMaxRound = Mathf.Max(battleMaxRound, round);
    }

    public static void TrackCombatDamage(float amount, int damageType, bool critical, Unidad source, Unidad target)
    {
        if (!battleActive || amount <= 0f || target == null)
        {
            return;
        }

        bool targetIsEnemy = IsEnemy(target);
        string direction = targetIsEnemy ? "dealt" : "received";
        if (targetIsEnemy)
        {
            battleDamageDealt += amount;
        }
        else
        {
            battleDamageReceived += amount;
        }

        TrackDesignValue(
            amount,
            "combat",
            "damage",
            direction,
            DamageTypeToken(damageType),
            critical ? "critical" : "normal");
    }

    public static void TrackCombatHealing(float amount, Unidad target)
    {
        if (!battleActive || amount <= 0f || target == null)
        {
            return;
        }

        string side = SideToken(target);
        if (!IsEnemy(target))
        {
            battleHealingReceived += amount;
        }

        TrackDesignValue(amount, "combat", "healing", "received", side);
    }

    public static void TrackCombatState(string action, string state, float amount, Unidad target)
    {
        if (!battleActive || amount <= 0f || target == null)
        {
            return;
        }

        TrackDesignValue(amount, "combat", "state", action, state, SideToken(target));
    }

    public static void TrackCombatStateResisted(string state, Unidad target)
    {
        if (!battleActive || target == null)
        {
            return;
        }

        TrackDesign("combat", "state", "resisted", state, SideToken(target));
    }

    public static void TrackCombatBuff(string action, string buffName, bool beneficial, Unidad target)
    {
        if (!battleActive || target == null || string.IsNullOrWhiteSpace(buffName))
        {
            return;
        }

        TrackDesign(
            "combat",
            beneficial ? "buff" : "debuff",
            action,
            buffName,
            SideToken(target));
    }

    public static void TrackAbilityUsed(Habilidad ability, Unidad user)
    {
        if (!battleActive || ability == null || user == null)
        {
            return;
        }

        TrackDesign("combat", "ability_used", SideToken(user), AbilityToken(ability));
    }

    public static void TrackUnitDown(Unidad unit)
    {
        if (!battleActive || unit == null)
        {
            return;
        }

        bool enemy = IsEnemy(unit);
        if (enemy)
        {
            battleEnemiesDown++;
        }
        else
        {
            battleAlliesDown++;
        }

        TrackDesign("combat", "unit_down", enemy ? "enemy" : "ally", UnitTypeToken(unit));
    }

    public static void TrackEnemyPartyComposition(IEnumerable<Unidad> initialEnemies, IEnumerable<GameObject> reinforcements)
    {
        if (!battleActive)
        {
            return;
        }

        Dictionary<string, int> initial = CountUnits(initialEnemies);
        Dictionary<string, int> reserve = CountUnits(reinforcements);
        TrackDesignValue(SumCounts(initial), "battle", "enemy_party", "size", "initial", battleType);
        TrackDesignValue(SumCounts(reserve), "battle", "enemy_party", "size", "reinforcement", battleType);
        TrackComposition(initial, "initial");
        TrackComposition(reserve, "reinforcement");
    }

    public static void EndBattle(bool victory, int rounds)
    {
        if (!battleActive)
        {
            return;
        }

        battleMaxRound = Mathf.Max(battleMaxRound, rounds);
        EmitBattleSummary(victory ? "victory" : "defeat");
        ResetBattle();
    }

    public static void TrackBattleAbandoned(string reason)
    {
        if (!battleActive)
        {
            return;
        }

        TrackDesign("battle", "abandoned", battleType, battleEncounter, reason);
        TrackProgressionFail("battle", battleType, battleEncounter);
        EmitBattleSummary("abandoned");
        ResetBattle();
    }

    public static void TrackItemAcquired(Item item, string source)
    {
        if (item == null)
        {
            return;
        }

        TrackDesign("item", "acquired", source, ItemKind(item), ItemToken(item));
    }

    public static void TrackItemUsed(Item item, Unidad user)
    {
        if (item == null)
        {
            return;
        }

        TrackDesign("item", "used", ItemKind(item), ItemToken(item), SideToken(user));
    }

    public static void TrackCharacterState(Personaje character, string state, string source)
    {
        if (character == null)
        {
            return;
        }

        TrackDesign("character", "state", state, source, ClassToken(character));
    }

    public static string ZoneToken(int zoneId)
    {
        return "zone_" + Mathf.Max(0, zoneId);
    }

    public static string PhaseToken(int phase)
    {
        return "phase_" + Mathf.Max(0, phase);
    }

    public static void TrackPerformance(float averageFps, float allocatedMemoryMb, float reservedMemoryMb)
    {
        TrackDesignValue(averageFps, "performance", "fps", "average");
        TrackDesignValue(allocatedMemoryMb, "performance", "memory", "allocated_mb");
        TrackDesignValue(reservedMemoryMb, "performance", "memory", "reserved_mb");
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

    private static bool IsEnemy(Unidad unit)
    {
        return unit != null && unit.CasillaPosicion != null && unit.CasillaPosicion.lado == 1;
    }

    private static string SideToken(Unidad unit)
    {
        if (unit == null)
        {
            return "unknown";
        }

        return IsEnemy(unit) ? "enemy" : "ally";
    }

    private static string UnitTypeToken(Unidad unit)
    {
        if (unit == null)
        {
            return "unknown";
        }

        string typeName = unit.GetType().Name;
        string identity = string.Equals(typeName, nameof(Unidad), StringComparison.Ordinal)
            ? unit.uNombre
            : typeName;
        return SanitizeToken(identity, 32);
    }

    private static string DamageTypeToken(int damageType)
    {
        switch (damageType)
        {
            case 1: return "slashing";
            case 2: return "piercing";
            case 3: return "blunt";
            case 4: return "fire";
            case 5: return "cold";
            case 6: return "lightning";
            case 7: return "acid";
            case 8: return "arcane";
            case 9: return "necrotic";
            case 10: return "true";
            case 11: return "divine";
            default: return "unknown";
        }
    }

    private static Dictionary<string, int> CountUnits(IEnumerable<Unidad> units)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        if (units == null)
        {
            return counts;
        }

        foreach (Unidad unit in units)
        {
            AddUnitCount(counts, unit);
        }

        return counts;
    }

    private static Dictionary<string, int> CountUnits(IEnumerable<GameObject> units)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        if (units == null)
        {
            return counts;
        }

        foreach (GameObject unitObject in units)
        {
            AddUnitCount(counts, unitObject != null ? unitObject.GetComponent<Unidad>() : null);
        }

        return counts;
    }

    private static void AddUnitCount(Dictionary<string, int> counts, Unidad unit)
    {
        if (unit == null)
        {
            return;
        }

        string token = UnitTypeToken(unit);
        counts.TryGetValue(token, out int current);
        counts[token] = current + 1;
    }

    private static int SumCounts(Dictionary<string, int> counts)
    {
        int total = 0;
        foreach (KeyValuePair<string, int> entry in counts)
        {
            total += entry.Value;
        }

        return total;
    }

    private static void TrackComposition(Dictionary<string, int> counts, string group)
    {
        foreach (KeyValuePair<string, int> entry in counts)
        {
            TrackDesignValue(entry.Value, "battle", "enemy_party", group, battleType, entry.Key);
        }
    }

    private static void EmitBattleSummary(string result)
    {
        float durationSeconds = Mathf.Max(0f, (float)(Time.realtimeSinceStartupAsDouble - battleStartedAt));
        TrackDesignValue(durationSeconds, "battle", "summary", result, "duration_seconds", battleType);
        TrackDesignValue(battleTurns, "battle", "summary", result, "turns", battleType);
        TrackDesignValue(battleMaxRound, "battle", "summary", result, "rounds", battleType);
        TrackDesignValue(battleDamageDealt, "battle", "summary", result, "damage_dealt", battleType);
        TrackDesignValue(battleDamageReceived, "battle", "summary", result, "damage_received", battleType);
        TrackDesignValue(battleHealingReceived, "battle", "summary", result, "healing_received", battleType);
        TrackDesignValue(battleEnemiesDown, "battle", "summary", result, "enemies_down", battleType);
        TrackDesignValue(battleAlliesDown, "battle", "summary", result, "allies_down", battleType);
    }

    private static void ResetBattle()
    {
        battleActive = false;
        battleType = "unknown";
        battleEncounter = "unknown";
        battleStartedAt = 0d;
        battleTurns = 0;
        battleMaxRound = 0;
        battleDamageDealt = 0f;
        battleDamageReceived = 0f;
        battleHealingReceived = 0f;
        battleEnemiesDown = 0;
        battleAlliesDown = 0;
    }

    private static bool EnsureProviderReady()
    {
        if (!metricsReady)
        {
            Initialize();
        }

        return metricsReady;
    }

    private static bool CanSendEvents(out string reason)
    {
        RuntimeAnalyticsSettings settings = GetSettings();
        if (settings != null && !settings.EnableAnalytics)
        {
            reason = "Metricas deshabilitadas en RuntimeAnalyticsSettings.";
            return false;
        }

        if (Application.isEditor && (settings == null || settings.DisableInEditor))
        {
            reason = "Metricas deshabilitadas dentro del editor.";
            return false;
        }

#if DEVELOPMENT_BUILD
        if (!Application.isEditor && settings != null && settings.DisableInDevelopmentBuild)
        {
            reason = "Metricas deshabilitadas en development build.";
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

    private static void PrepareLogFile(RuntimeAnalyticsSettings settings)
    {
        string folderName = settings == null || string.IsNullOrWhiteSpace(settings.EventLogFolderName)
            ? DefaultLogFolderName
            : settings.EventLogFolderName.Trim();

        try
        {
            string directory = Path.Combine(Application.persistentDataPath, folderName);
            Directory.CreateDirectory(directory);
            logFilePath = Path.Combine(directory, "events-" + DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".jsonl");
        }
        catch (Exception exception)
        {
            fileWriteFailed = true;
            Debug.LogWarning("[Metrics] No se pudo preparar el archivo de metricas: " + exception.Message);
        }
    }

    private static void TrackProgression(string status, string progression01, string progression02, string progression03)
    {
        if (!EnsureProviderReady())
        {
            return;
        }

        string sanitizedProgression01 = SanitizeToken(progression01);
        string sanitizedProgression02 = string.IsNullOrWhiteSpace(progression02) ? null : SanitizeToken(progression02);
        string sanitizedProgression03 = string.IsNullOrWhiteSpace(progression03) ? null : SanitizeToken(progression03);

        Emit(new MetricEvent
        {
            type = "progression",
            status = status,
            progression01 = sanitizedProgression01,
            progression02 = sanitizedProgression02,
            progression03 = sanitizedProgression03
        });
        GameAnalyticsRuntimeProvider.TrackProgression(
            status,
            sanitizedProgression01,
            sanitizedProgression02,
            sanitizedProgression03);
    }

    private static void TrackResource(string flowType, string currency, float amount, string itemType, string itemId)
    {
        if (amount <= 0f || !EnsureProviderReady())
        {
            return;
        }

        string sanitizedCurrency = SanitizeToken(currency, 24);
        string sanitizedItemType = SanitizeToken(itemType, 32);
        string sanitizedItemId = SanitizeToken(itemId, 32);

        Emit(new MetricEvent
        {
            type = "resource",
            flow = flowType,
            currency = sanitizedCurrency,
            amount = amount,
            itemType = sanitizedItemType,
            itemId = sanitizedItemId
        });
        GameAnalyticsRuntimeProvider.TrackResource(
            flowType,
            sanitizedCurrency,
            amount,
            sanitizedItemType,
            sanitizedItemId);
    }

    private static void Emit(MetricEvent metricEvent)
    {
        RuntimeAnalyticsSettings settings = GetSettings();
        metricEvent.utc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
        metricEvent.session = sessionId;
        metricEvent.scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(metricEvent);
        if (settings != null && settings.LogEventsToConsole)
        {
            Debug.Log("[Metrics] " + json);
        }

        if ((settings == null || settings.WriteEventsToFile) && !fileWriteFailed && !string.IsNullOrWhiteSpace(logFilePath))
        {
            try
            {
                File.AppendAllText(logFilePath, json + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception exception)
            {
                fileWriteFailed = true;
                Debug.LogWarning("[Metrics] No se pudo escribir el evento: " + exception.Message);
            }
        }
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
        Debug.Log("[Metrics] " + message);
    }

    [Serializable]
    private sealed class MetricEvent
    {
        public string utc;
        public string session;
        public string scene;
        public string type;
        public string name;
        public bool hasValue;
        public float value;
        public string status;
        public string progression01;
        public string progression02;
        public string progression03;
        public string flow;
        public string currency;
        public float amount;
        public string itemType;
        public string itemId;
    }
}
