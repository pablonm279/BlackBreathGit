using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EncounterUnitSlot
{
   public GameObject prefab;
   public int tierCost;
   public bool spawnAsReinforcement;
}

public class EncounterDefinition
{
   public EncounterZoneType zoneType;
   public BattleEncounterType battleType;
   public string factionId;
   public string factionName;
   public int fase;
   public int baseBudget;
   public int randomBonus;
   public int totalBudget;
   public int initialCap;
   public int reinforcementDelay;
   public List<EncounterUnitSlot> units = new List<EncounterUnitSlot>();

   public IEnumerable<GameObject> InitialUnits()
   {
      foreach (var slot in units)
      {
         if (!slot.spawnAsReinforcement)
         {
            yield return slot.prefab;
         }
      }
   }

   public IEnumerable<GameObject> ReinforcementUnits()
   {
      foreach (var slot in units)
      {
         if (slot.spawnAsReinforcement)
         {
            yield return slot.prefab;
         }
      }
   }
}

public static class EncounterGenerator
{
   const int MaxInitialUnits = 6;

   public static bool TryGenerateEncounter(
      AtributosZona atributosZona,
      EncounterZoneType zoneType,
      BattleEncounterType battleType,
      int fase,
      out EncounterDefinition encounter,
      Predicate<EnemyFactionConfig> factionFilter = null)
   {
      encounter = null;
      if (atributosZona == null)
      {
         return false;
      }

      if (TryGetDebugEncounter(atributosZona, zoneType, battleType, fase, out encounter))
      {
         return true;
      }

      var zoneConfig = atributosZona.GetEncounterConfig(zoneType);
      if (zoneConfig == null)
      {
         return false;
      }

      var pool = zoneConfig.GetPool(battleType);
      if (pool == null || pool.factions == null || pool.factions.Count == 0)
      {
         return false;
      }

      var candidates = new List<EnemyFactionConfig>();
      foreach (var faction in pool.factions)
      {
         if (!FactionHasPrefabs(faction))
         {
            continue;
         }

         if (factionFilter != null && !factionFilter(faction))
         {
            continue;
         }

         candidates.Add(faction);
      }

      if (candidates.Count == 0)
      {
         return false;
      }

      // Selección uniforme entre facciones habilitadas
      var chosenFaction = candidates[UnityEngine.Random.Range(0, candidates.Count)];

      int faseClamped = Mathf.Max(1, fase);
      int maxTierAllowed = Mathf.Clamp(faseClamped + 2, 1, 5);
      int minUnits = 2;
      int cheapestTier = GetCheapestTierAvailable(chosenFaction, maxTierAllowed);
      if (cheapestTier == 0)
      {
         return false;
      }
      int baseBudget = GetBaseBudget(battleType);
      int randomBonus = UnityEngine.Random.Range(0, 4); // 0 a 3 inclusive
      int totalBudget = baseBudget * faseClamped + randomBonus;
      totalBudget += Sistema.HandicapDificultad.AjustePuntosEnemigos;
      totalBudget = Mathf.Max(cheapestTier * minUnits, totalBudget);
      int initialCap = Mathf.Min(MaxInitialUnits, Mathf.Max(1, faseClamped) * 5);
      int reinforcementDelay = Mathf.Clamp(2 + faseClamped, 3, 6);
      int maxUnitsPossible = Mathf.Max(minUnits, totalBudget / Mathf.Max(1, cheapestTier));
      int targetUnits = ElegirCantidadObjetivo(minUnits, maxUnitsPossible);
      float highTierBias = (maxUnitsPossible > minUnits)
         ? 1f - Mathf.Clamp01((float)(targetUnits - minUnits) / (float)(maxUnitsPossible - minUnits))
         : 0.5f;

      var generated = new EncounterDefinition
      {
         zoneType = zoneType,
         battleType = battleType,
         factionId = chosenFaction.factionId,
         factionName = string.IsNullOrWhiteSpace(chosenFaction.displayName) ? chosenFaction.factionId : chosenFaction.displayName,
         fase = faseClamped,
         baseBudget = baseBudget,
         randomBonus = randomBonus,
         totalBudget = totalBudget,
         initialCap = initialCap,
         reinforcementDelay = reinforcementDelay,
      };

      FillUnits(generated, chosenFaction, totalBudget, initialCap, maxTierAllowed, minUnits, targetUnits, maxUnitsPossible, highTierBias);
      generated.totalBudget = Mathf.Max(generated.totalBudget, CalcularCostoTotal(generated));

      // Garantizar al menos una unidad inicial si es posible
      if (generated.units.Count == 0)
      {
         var fallback = GetFirstAvailablePrefab(chosenFaction);
         if (fallback != null)
         {
            generated.units.Add(new EncounterUnitSlot
            {
               prefab = fallback,
               tierCost = 1,
               spawnAsReinforcement = false
            });
         }
      }

      encounter = generated.units.Count > 0 ? generated : null;
      return encounter != null;
   }

   static int ElegirCantidadObjetivo(int minUnits, int maxUnitsPossible)
   {
      if (maxUnitsPossible <= minUnits)
      {
         return minUnits;
      }

      int modo = UnityEngine.Random.Range(0, 3); // 0 pocas, 1 muchas, 2 intermedio
      if (modo == 0) return minUnits;
      if (modo == 1) return maxUnitsPossible;
      return UnityEngine.Random.Range(minUnits, maxUnitsPossible + 1);
   }

   static void FillUnits(EncounterDefinition definition, EnemyFactionConfig faction, int budget, int initialCap, int maxTierAllowedParam, int minUnitsParam, int targetUnits = -1, int maxUnitsPossible = -1, float highTierBias = 0.5f)
   {
      if (faction == null)
      {
         return;
      }

      int remaining = budget;
      int initialCount = 0;
      int safetyCounter = 0;
      int maxTierAllowed = Mathf.Max(1, maxTierAllowedParam);
      int minUnits = Mathf.Max(1, minUnitsParam);
      int maxUnits = maxUnitsPossible > 0 ? maxUnitsPossible : minUnits;
      int targetCount = targetUnits > 0 ? targetUnits : minUnits;
      int allowedInitial = Mathf.Min(initialCap, MaxInitialUnits);
      int cheapestTier = GetCheapestTierAvailable(faction, maxTierAllowed);
      if (cheapestTier == 0)
      {
         return;
      }
      targetCount = Mathf.Clamp(targetCount, minUnits, Mathf.Max(minUnits, maxUnits));

      while (remaining > 0 && safetyCounter < 200 && targetCount > 0)
      {
         safetyCounter++;
         var availableTiers = GetAvailableTiers(faction, remaining, maxTierAllowed);
         if (availableTiers.Count == 0)
         {
            break;
         }

         int slotsRestantes = targetCount - definition.units.Count;
         var viables = new List<int>();
         foreach (var tier in availableTiers)
         {
            if (remaining - tier >= (slotsRestantes - 1) * cheapestTier)
            {
               viables.Add(tier);
            }
         }
         if (viables.Count == 0)
         {
            viables = availableTiers;
         }

         // Favorecer tiers altos cuando el objetivo es "pocas unidades fuertes"
         viables.Sort();
         int selectedTier = cheapestTier;
         if (viables.Count > 0)
         {
            bool usarAltos = UnityEngine.Random.value < Mathf.Clamp01(highTierBias);
            if (usarAltos)
            {
               int start = Mathf.Max(0, viables.Count - Mathf.Max(1, viables.Count / 2));
               selectedTier = viables[UnityEngine.Random.Range(start, viables.Count)];
            }
            else
            {
               selectedTier = viables[UnityEngine.Random.Range(0, viables.Count)];
            }
         }

         var tierList = GetTierList(faction, selectedTier);
         if (tierList == null || tierList.Count == 0)
         {
            continue;
         }

         var prefab = tierList[UnityEngine.Random.Range(0, tierList.Count)];
         bool spawnAsReinforcement = initialCount >= allowedInitial;

         definition.units.Add(new EncounterUnitSlot
         {
            prefab = prefab,
            tierCost = selectedTier,
            spawnAsReinforcement = spawnAsReinforcement
         });

         if (!spawnAsReinforcement)
         {
            initialCount++;
         }

         remaining -= selectedTier;
         targetCount--;
      }

      // Guardia final: si quedaron por debajo del piso de unidades, sumar del tier mШs barato
      while (definition.units.Count < minUnits && safetyCounter < 400)
      {
         safetyCounter++;
         var prefab = GetPrefabFromTier(faction, cheapestTier);
         if (prefab == null)
         {
            break;
         }

         bool spawnAsReinforcement = initialCount >= allowedInitial;
         definition.units.Add(new EncounterUnitSlot
         {
            prefab = prefab,
            tierCost = cheapestTier,
            spawnAsReinforcement = spawnAsReinforcement
         });

         if (!spawnAsReinforcement)
         {
            initialCount++;
         }
      }
   }

   static bool TryGetDebugEncounter(
      AtributosZona atributosZona,
      EncounterZoneType zoneType,
      BattleEncounterType battleType,
      int fase,
      out EncounterDefinition definition)
   {
      definition = null;
      if (atributosZona == null || atributosZona.debugEncounterUnits == null)
      {
         return false;
      }

      var forcedUnits = new List<GameObject>();
      foreach (var prefab in atributosZona.debugEncounterUnits)
      {
         if (prefab != null)
         {
            forcedUnits.Add(prefab);
         }
      }

      if (forcedUnits.Count == 0)
      {
         return false;
      }

      int faseClamped = Mathf.Max(1, fase);
      int baseBudget = GetBaseBudget(battleType);
      int randomBonus = 0;
      int totalBudget = baseBudget * faseClamped + randomBonus;
      totalBudget += Sistema.HandicapDificultad.AjustePuntosEnemigos;
      totalBudget = Mathf.Max(1, totalBudget);
      int initialCap = Mathf.Min(MaxInitialUnits, Mathf.Max(1, forcedUnits.Count));
      int reinforcementDelay = 0;

      definition = new EncounterDefinition
      {
         zoneType = zoneType,
         battleType = battleType,
         factionId = "DebugEncounter",
         factionName = string.IsNullOrWhiteSpace(atributosZona.Nombre) ? "DebugEncounter" : atributosZona.Nombre,
         fase = faseClamped,
         baseBudget = baseBudget,
         randomBonus = randomBonus,
         totalBudget = totalBudget,
         initialCap = initialCap,
         reinforcementDelay = reinforcementDelay
      };

      int index = 0;
      foreach (var prefab in forcedUnits)
      {
         definition.units.Add(new EncounterUnitSlot
         {
            prefab = prefab,
            tierCost = 0,
            spawnAsReinforcement = index >= MaxInitialUnits
         });

         index++;
      }

      return true;
   }

   static List<int> GetAvailableTiers(EnemyFactionConfig faction, int remainingBudget, int maxTierAllowed)
   {
      var tiers = new List<int>();

      if (HasTier(faction, 1) && remainingBudget >= 1)
      {
         tiers.Add(1);
      }
      if (maxTierAllowed >= 2 && HasTier(faction, 2) && remainingBudget >= 2)
      {
         tiers.Add(2);
      }
      if (maxTierAllowed >= 3 && HasTier(faction, 3) && remainingBudget >= 3)
      {
         tiers.Add(3);
      }
      if (maxTierAllowed >= 4 && HasTier(faction, 4) && remainingBudget >= 4)
      {
         tiers.Add(4);
      }
      if (maxTierAllowed >= 5 && HasTier(faction, 5) && remainingBudget >= 5)
      {
         tiers.Add(5);
      }

      return tiers;
   }

   static bool HasTier(EnemyFactionConfig faction, int tier)
   {
      var list = GetTierList(faction, tier);
      return list != null && list.Count > 0;
   }

   static int GetCheapestTierAvailable(EnemyFactionConfig faction, int maxTierAllowed)
   {
      if (faction == null)
      {
         return 0;
      }

      for (int tier = 1; tier <= Mathf.Max(1, maxTierAllowed); tier++)
      {
         if (HasTier(faction, tier))
         {
            return tier;
         }
      }

      return 0;
   }

   static List<GameObject> GetTierList(EnemyFactionConfig faction, int tier)
   {
      if (faction == null || faction.tiers == null)
      {
         return null;
      }

      switch (tier)
      {
         case 1:
            return faction.tiers.tier1;
         case 2:
            return faction.tiers.tier2;
         case 3:
            return faction.tiers.tier3;
         case 4:
            return faction.tiers.tier4;
         case 5:
            return faction.tiers.tier5;
         default:
            return null;
      }
   }

   static bool FactionHasPrefabs(EnemyFactionConfig faction)
   {
      if (faction == null || faction.tiers == null)
      {
         return false;
      }

      return (faction.tiers.tier1 != null && faction.tiers.tier1.Count > 0)
         || (faction.tiers.tier2 != null && faction.tiers.tier2.Count > 0)
         || (faction.tiers.tier3 != null && faction.tiers.tier3.Count > 0)
         || (faction.tiers.tier4 != null && faction.tiers.tier4.Count > 0)
         || (faction.tiers.tier5 != null && faction.tiers.tier5.Count > 0);
   }

   static GameObject GetFirstAvailablePrefab(EnemyFactionConfig faction)
   {
      if (faction == null || faction.tiers == null)
      {
         return null;
      }

      if (faction.tiers.tier1 != null && faction.tiers.tier1.Count > 0)
      {
         return faction.tiers.tier1[0];
      }
      if (faction.tiers.tier2 != null && faction.tiers.tier2.Count > 0)
      {
         return faction.tiers.tier2[0];
      }
      if (faction.tiers.tier3 != null && faction.tiers.tier3.Count > 0)
      {
         return faction.tiers.tier3[0];
      }
      if (faction.tiers.tier4 != null && faction.tiers.tier4.Count > 0)
      {
         return faction.tiers.tier4[0];
      }
      if (faction.tiers.tier5 != null && faction.tiers.tier5.Count > 0)
      {
         return faction.tiers.tier5[0];
      }

      return null;
   }

   static GameObject GetPrefabFromTier(EnemyFactionConfig faction, int tier)
   {
      var tierList = GetTierList(faction, tier);
      if (tierList == null || tierList.Count == 0)
      {
         return null;
      }

      return tierList[UnityEngine.Random.Range(0, tierList.Count)];
   }

   static int GetBaseBudget(BattleEncounterType battleType)
   {
      switch (battleType)
      {
         case BattleEncounterType.Elite:
            return 7;
         case BattleEncounterType.AtaqueCaravana:
            return 12;
         case BattleEncounterType.Subterraneo:
            return 6;
         case BattleEncounterType.Normal:
         default:
            return 5;
      }
   }

   static int CalcularCostoTotal(EncounterDefinition definition)
   {
      if (definition == null || definition.units == null)
      {
         return 0;
      }

      int total = 0;
      foreach (var slot in definition.units)
      {
         if (slot != null)
         {
            total += Mathf.Max(0, slot.tierCost);
         }
      }

      return total;
   }
}
