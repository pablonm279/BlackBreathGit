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

   enum CompositionArchetype
   {
      Balanced,
      Swarm,
      FewStrong,
      Reinforced
   }

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
      int minUnits = GetMinUnitsFloor(battleType, faseClamped);
      int cheapestTier = GetCheapestTierAvailable(chosenFaction, maxTierAllowed);
      if (cheapestTier == 0)
      {
         return false;
      }
      int highestTierAvailable = GetHighestTierAvailable(chosenFaction, maxTierAllowed);
      int baseBudget = GetBaseBudget(battleType);
      int randomBonus = RollBudgetBonus(battleType, faseClamped);
      int totalBudget = baseBudget * faseClamped + randomBonus;
      totalBudget += Sistema.HandicapDificultad.AjustePuntosEnemigos;
      int minBudgetFloor = GetMinBudgetFloor(battleType, faseClamped, cheapestTier, minUnits, highestTierAvailable);
      totalBudget = Mathf.Max(Mathf.Max(cheapestTier * minUnits, minBudgetFloor), totalBudget);
      int initialCap = GetInitialUnitCap(battleType, faseClamped);
      int reinforcementDelay = 0;

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

      FillUnits(generated, chosenFaction, totalBudget, initialCap, maxTierAllowed, minUnits);
      EnforceMinimumComposition(generated, chosenFaction, battleType, maxTierAllowed, minUnits);
      EnsureMandatoryReinforcements(generated, chosenFaction, maxTierAllowed);
      EnforceUniqueUnits(generated, chosenFaction, battleType, maxTierAllowed, minUnits);
      MarkReinforcements(generated, initialCap);
      generated.totalBudget = CalcularCostoTotal(generated);
      generated.reinforcementDelay = HasReinforcements(generated)
         ? GetReinforcementDelay(battleType, faseClamped)
         : 0;

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

   static void FillUnits(EncounterDefinition definition, EnemyFactionConfig faction, int budget, int initialCap, int maxTierAllowedParam, int minUnitsParam)
   {
      if (definition == null || faction == null)
      {
         return;
      }

      int maxTierAllowed = Mathf.Max(1, maxTierAllowedParam);
      int minUnits = Mathf.Max(1, minUnitsParam);
      int cheapestTier = GetCheapestTierAvailable(faction, maxTierAllowed);
      if (cheapestTier == 0)
      {
         return;
      }
      int highestTier = GetHighestTierAvailable(faction, maxTierAllowed);
      int maxUnitsPossible = Mathf.Max(minUnits, budget / Mathf.Max(1, cheapestTier));
      int maxUnits = GetSoftMaxUnits(definition.battleType, definition.fase, budget, cheapestTier, minUnits, maxUnitsPossible);
      CompositionArchetype archetype = ChooseCompositionArchetype(definition.battleType, budget, cheapestTier, highestTier, minUnits, maxUnits);
      int targetCount = ChooseTargetUnitCount(definition.battleType, archetype, budget, cheapestTier, highestTier, minUnits, maxUnits);
      int remaining = budget;
      int safetyCounter = 0;
      Dictionary<GameObject, int> prefabUsage = new Dictionary<GameObject, int>();
      Dictionary<string, int> uniqueUsage = new Dictionary<string, int>();

      while (definition.units.Count < targetCount && safetyCounter < 200)
      {
         safetyCounter++;
         int unitsLeft = Mathf.Max(1, targetCount - definition.units.Count);
         int maxCostNow = remaining - cheapestTier * Mathf.Max(0, unitsLeft - 1);
         int selectedTier = ChooseTierForSlot(
            faction,
            definition.battleType,
            archetype,
            remaining,
            unitsLeft,
            Mathf.Max(cheapestTier, maxCostNow),
            maxTierAllowed,
            uniqueUsage);
         if (selectedTier == 0)
         {
            break;
         }

         GameObject prefab = PickPrefabFromTierWithVariety(faction, selectedTier, prefabUsage, uniqueUsage);
         if (prefab == null)
         {
            continue;
         }

         definition.units.Add(new EncounterUnitSlot
         {
            prefab = prefab,
            tierCost = selectedTier,
            spawnAsReinforcement = false
         });
         RegistrarUsoPrefab(prefabUsage, prefab, 1);
         RegistrarUsoUnico(uniqueUsage, prefab, 1);
         remaining -= selectedTier;
      }

      SpendRemainingBudget(definition, faction, definition.battleType, archetype, maxTierAllowed, maxUnits, ref remaining, prefabUsage, uniqueUsage);
   }

   static CompositionArchetype ChooseCompositionArchetype(BattleEncounterType battleType, int budget, int cheapestTier, int highestTier, int minUnits, int maxUnits)
   {
      bool canFewStrong = highestTier >= 3 && budget >= Mathf.Max(highestTier + cheapestTier * Mathf.Max(1, minUnits - 1), minUnits * 2);
      bool canSwarm = cheapestTier == 1 && maxUnits >= minUnits + 2;

      float balancedWeight = 4f;
      float swarmWeight = canSwarm ? 1f : 0f;
      float fewStrongWeight = canFewStrong ? 1f : 0f;
      float reinforcedWeight = 0.4f;

      switch (battleType)
      {
         case BattleEncounterType.Elite:
            balancedWeight = 2.5f;
            swarmWeight *= 0.3f;
            fewStrongWeight *= 2.8f;
            reinforcedWeight = 0.2f;
            break;
         case BattleEncounterType.AtaqueCaravana:
            balancedWeight = 2.8f;
            swarmWeight += canSwarm ? 1.2f : 0f;
            fewStrongWeight *= 0.9f;
            reinforcedWeight = 3.1f;
            break;
         case BattleEncounterType.Subterraneo:
            balancedWeight = 3.4f;
            swarmWeight += canSwarm ? 0.5f : 0f;
            fewStrongWeight *= 1.4f;
            reinforcedWeight = 0.8f;
            break;
         case BattleEncounterType.Normal:
         default:
            balancedWeight = 4.4f;
            swarmWeight += canSwarm ? 0.9f : 0f;
            fewStrongWeight *= 0.12f;
            reinforcedWeight = 0.3f;
            break;
      }

      float total = balancedWeight + swarmWeight + fewStrongWeight + reinforcedWeight;
      float roll = UnityEngine.Random.Range(0f, total);

      if (roll < balancedWeight)
      {
         return CompositionArchetype.Balanced;
      }

      roll -= balancedWeight;
      if (roll < swarmWeight)
      {
         return CompositionArchetype.Swarm;
      }

      roll -= swarmWeight;
      if (roll < fewStrongWeight)
      {
         return CompositionArchetype.FewStrong;
      }

      return CompositionArchetype.Reinforced;
   }

   static int ChooseTargetUnitCount(BattleEncounterType battleType, CompositionArchetype archetype, int budget, int cheapestTier, int highestTier, int minUnits, int maxUnits)
   {
      int center;
      switch (archetype)
      {
         case CompositionArchetype.FewStrong:
            center = Mathf.RoundToInt(budget / Mathf.Clamp(highestTier - 0.15f, 2f, 3.75f));
            center = Mathf.Min(center, Mathf.Max(minUnits, maxUnits - 1));
            break;
         case CompositionArchetype.Swarm:
            center = Mathf.RoundToInt(budget / 1.35f);
            break;
         case CompositionArchetype.Reinforced:
            center = Mathf.RoundToInt(budget / 1.7f);
            break;
         case CompositionArchetype.Balanced:
         default:
            center = Mathf.RoundToInt(budget / 1.9f);
            break;
      }

      if (battleType == BattleEncounterType.Elite)
      {
         center = Mathf.Min(center, Mathf.Max(minUnits, maxUnits - 1));
      }

      center = Mathf.Clamp(center, minUnits, maxUnits);
      int jitter = maxUnits > minUnits ? 1 : 0;
      if (archetype == CompositionArchetype.FewStrong && highestTier >= 4)
      {
         jitter = 0;
      }

      int minBand = Mathf.Clamp(center - jitter, minUnits, maxUnits);
      int maxBand = Mathf.Clamp(center + jitter, minUnits, maxUnits);
      return UnityEngine.Random.Range(minBand, maxBand + 1);
   }

   static int ChooseTierForSlot(
      EnemyFactionConfig faction,
      BattleEncounterType battleType,
      CompositionArchetype archetype,
      int remainingBudget,
      int unitsLeft,
      int maxAffordableTierBudget,
      int maxTierAllowed,
      Dictionary<string, int> uniqueUsage)
   {
      int capBudget = Mathf.Max(1, maxAffordableTierBudget);
      var availableTiers = GetAvailableTiers(faction, capBudget, maxTierAllowed, uniqueUsage);
      if (availableTiers.Count == 0)
      {
         return 0;
      }

      int cheapestTier = availableTiers[0];
      int highestTier = availableTiers[availableTiers.Count - 1];
      float budgetPerUnit = unitsLeft > 0 ? (float)remainingBudget / unitsLeft : remainingBudget;
      int preferredTier = Mathf.Clamp(Mathf.RoundToInt(budgetPerUnit), cheapestTier, highestTier);

      switch (archetype)
      {
         case CompositionArchetype.FewStrong:
            preferredTier = Mathf.Clamp(preferredTier + 1, Mathf.Min(2, highestTier), highestTier);
            break;
         case CompositionArchetype.Swarm:
            preferredTier = Mathf.Clamp(preferredTier - 1, cheapestTier, highestTier);
            break;
         case CompositionArchetype.Reinforced:
            preferredTier = Mathf.Clamp(preferredTier, cheapestTier, highestTier);
            break;
      }

      if (battleType == BattleEncounterType.Elite)
      {
         preferredTier = Mathf.Clamp(preferredTier + 1, Mathf.Min(2, highestTier), highestTier);
      }

      float totalWeight = 0f;
      List<float> weights = new List<float>(availableTiers.Count);
      foreach (int tier in availableTiers)
      {
         float weight = 1f / (1f + Mathf.Abs(tier - preferredTier));

         switch (archetype)
         {
            case CompositionArchetype.FewStrong:
               weight *= 0.8f + tier * 0.85f;
               break;
            case CompositionArchetype.Swarm:
               weight *= 0.9f + (highestTier - tier + 1) * 0.7f;
               break;
            case CompositionArchetype.Reinforced:
               weight *= 1f + Mathf.Max(0, tier - cheapestTier) * 0.35f;
               break;
            case CompositionArchetype.Balanced:
            default:
               weight *= tier == preferredTier ? 1.6f : 1f;
               break;
         }

         if (battleType == BattleEncounterType.Elite)
         {
            weight *= tier >= 2 ? 1.8f : 0.25f;
         }

         if (unitsLeft == 1)
         {
            weight *= 1f + tier * 0.35f;
         }

         totalWeight += weight;
         weights.Add(weight);
      }

      float roll = UnityEngine.Random.Range(0f, totalWeight);
      for (int i = 0; i < availableTiers.Count; i++)
      {
         roll -= weights[i];
         if (roll <= 0f)
         {
            return availableTiers[i];
         }
      }

      return availableTiers[availableTiers.Count - 1];
   }

   static GameObject PickPrefabFromTierWithVariety(
      EnemyFactionConfig faction,
      int tier,
      Dictionary<GameObject, int> prefabUsage,
      Dictionary<string, int> uniqueUsage,
      string ignoreUniqueKey = null)
   {
      var tierList = GetEligibleTierList(faction, tier, uniqueUsage, ignoreUniqueKey);
      if (tierList == null || tierList.Count == 0)
      {
         return null;
      }

      int minUsage = int.MaxValue;
      List<GameObject> candidates = new List<GameObject>();
      foreach (GameObject prefab in tierList)
      {
         if (prefab == null)
         {
            continue;
         }

         int usage = 0;
         prefabUsage.TryGetValue(prefab, out usage);
         if (usage < minUsage)
         {
            minUsage = usage;
            candidates.Clear();
            candidates.Add(prefab);
         }
         else if (usage == minUsage)
         {
            candidates.Add(prefab);
         }
      }

      if (candidates.Count == 0)
      {
         return null;
      }

      return candidates[UnityEngine.Random.Range(0, candidates.Count)];
   }

   static void RegistrarUsoPrefab(Dictionary<GameObject, int> prefabUsage, GameObject prefab, int delta)
   {
      if (prefab == null || prefabUsage == null || delta == 0)
      {
         return;
      }

      int current = 0;
      prefabUsage.TryGetValue(prefab, out current);
      current = Mathf.Max(0, current + delta);
      prefabUsage[prefab] = current;
   }

   static void RegistrarUsoUnico(Dictionary<string, int> uniqueUsage, GameObject prefab, int delta)
   {
      if (uniqueUsage == null || prefab == null || delta == 0)
      {
         return;
      }

      string uniqueKey = GetUniqueEncounterKey(prefab);
      if (string.IsNullOrEmpty(uniqueKey))
      {
         return;
      }

      int current = 0;
      uniqueUsage.TryGetValue(uniqueKey, out current);
      current = Mathf.Max(0, current + delta);
      if (current == 0)
      {
         uniqueUsage.Remove(uniqueKey);
      }
      else
      {
         uniqueUsage[uniqueKey] = current;
      }
   }

   static void SpendRemainingBudget(
      EncounterDefinition definition,
      EnemyFactionConfig faction,
      BattleEncounterType battleType,
      CompositionArchetype archetype,
      int maxTierAllowed,
      int maxUnits,
      ref int remainingBudget,
      Dictionary<GameObject, int> prefabUsage,
      Dictionary<string, int> uniqueUsage)
   {
      int cheapestTier = GetCheapestTierAvailable(faction, maxTierAllowed);
      if (cheapestTier == 0)
      {
         return;
      }

      int safetyCounter = 0;
      while (remainingBudget >= cheapestTier && safetyCounter < 80)
      {
         safetyCounter++;
         bool preferUpgrade = archetype == CompositionArchetype.FewStrong
            || definition.units.Count >= maxUnits
            || remainingBudget < cheapestTier * 2;

         bool changed = preferUpgrade
            ? TryUpgradeExistingUnit(definition, faction, archetype, maxTierAllowed, ref remainingBudget, prefabUsage, uniqueUsage)
            : TryAddExtraUnit(definition, faction, battleType, archetype, maxTierAllowed, maxUnits, ref remainingBudget, prefabUsage, uniqueUsage);

         if (!changed)
         {
            changed = preferUpgrade
               ? TryAddExtraUnit(definition, faction, battleType, archetype, maxTierAllowed, maxUnits, ref remainingBudget, prefabUsage, uniqueUsage)
               : TryUpgradeExistingUnit(definition, faction, archetype, maxTierAllowed, ref remainingBudget, prefabUsage, uniqueUsage);
         }

         if (!changed)
         {
            break;
         }
      }
   }

   static bool TryAddExtraUnit(
      EncounterDefinition definition,
      EnemyFactionConfig faction,
      BattleEncounterType battleType,
      CompositionArchetype archetype,
      int maxTierAllowed,
      int maxUnits,
      ref int remainingBudget,
      Dictionary<GameObject, int> prefabUsage,
      Dictionary<string, int> uniqueUsage)
   {
      if (definition == null || definition.units.Count >= maxUnits)
      {
         return false;
      }

      int selectedTier = ChooseTierForSlot(faction, battleType, archetype, remainingBudget, 1, remainingBudget, maxTierAllowed, uniqueUsage);
      if (selectedTier == 0)
      {
         return false;
      }

      GameObject prefab = PickPrefabFromTierWithVariety(faction, selectedTier, prefabUsage, uniqueUsage);
      if (prefab == null)
      {
         return false;
      }

      definition.units.Add(new EncounterUnitSlot
      {
         prefab = prefab,
         tierCost = selectedTier,
         spawnAsReinforcement = false
      });
      RegistrarUsoPrefab(prefabUsage, prefab, 1);
      RegistrarUsoUnico(uniqueUsage, prefab, 1);
      remainingBudget -= selectedTier;
      return true;
   }

   static bool TryUpgradeExistingUnit(
      EncounterDefinition definition,
      EnemyFactionConfig faction,
      CompositionArchetype archetype,
      int maxTierAllowed,
      ref int remainingBudget,
      Dictionary<GameObject, int> prefabUsage,
      Dictionary<string, int> uniqueUsage)
   {
      if (definition == null || definition.units == null || definition.units.Count == 0)
      {
         return false;
      }

      int bestIndex = -1;
      int bestTier = 0;
      int bestDelta = 0;
      int bestScore = int.MinValue;

      for (int i = 0; i < definition.units.Count; i++)
      {
         var slot = definition.units[i];
         if (slot == null)
         {
            continue;
         }

         for (int tier = maxTierAllowed; tier > slot.tierCost; tier--)
         {
            string uniqueKeyActual = GetUniqueEncounterKey(slot.prefab);
            if (!HasEligibleTier(faction, tier, uniqueUsage, uniqueKeyActual))
            {
               continue;
            }

            int delta = tier - slot.tierCost;
            if (delta > remainingBudget)
            {
               continue;
            }

            int score = delta * 10 - slot.tierCost * 2;
            if (archetype == CompositionArchetype.FewStrong)
            {
               score += tier * 6;
            }
            else if (archetype == CompositionArchetype.Swarm)
            {
               score += slot.tierCost <= 2 ? 4 : 0;
            }

            if (score > bestScore)
            {
               bestScore = score;
               bestIndex = i;
               bestTier = tier;
               bestDelta = delta;
            }
         }
      }

      if (bestIndex < 0 || bestTier == 0)
      {
         return false;
      }

      string uniqueKeyAnterior = GetUniqueEncounterKey(definition.units[bestIndex].prefab);
      GameObject nuevoPrefab = PickPrefabFromTierWithVariety(faction, bestTier, prefabUsage, uniqueUsage, uniqueKeyAnterior);
      if (nuevoPrefab == null)
      {
         return false;
      }

      RegistrarUsoPrefab(prefabUsage, definition.units[bestIndex].prefab, -1);
      RegistrarUsoUnico(uniqueUsage, definition.units[bestIndex].prefab, -1);
      definition.units[bestIndex].prefab = nuevoPrefab;
      definition.units[bestIndex].tierCost = bestTier;
      RegistrarUsoPrefab(prefabUsage, nuevoPrefab, 1);
      RegistrarUsoUnico(uniqueUsage, nuevoPrefab, 1);
      remainingBudget -= bestDelta;
      return true;
   }

   static void MarkReinforcements(EncounterDefinition definition, int initialCap)
   {
      if (definition == null || definition.units == null || definition.units.Count == 0)
      {
         return;
      }

      int desiredInitialUnits = GetDesiredInitialUnits(definition.battleType, definition.fase, definition.units.Count, initialCap);
      desiredInitialUnits = Mathf.Clamp(desiredInitialUnits, 1, Mathf.Min(initialCap, definition.units.Count));

      List<int> indicesOrdenados = new List<int>();
      for (int i = 0; i < definition.units.Count; i++)
      {
         indicesOrdenados.Add(i);
      }

      indicesOrdenados.Sort((a, b) =>
      {
         var slotA = definition.units[a];
         var slotB = definition.units[b];
         int tierCompare = slotB.tierCost.CompareTo(slotA.tierCost);
         if (tierCompare != 0)
         {
            return tierCompare;
         }

         string nombreA = slotA != null && slotA.prefab != null ? slotA.prefab.name : string.Empty;
         string nombreB = slotB != null && slotB.prefab != null ? slotB.prefab.name : string.Empty;
         return string.CompareOrdinal(nombreA, nombreB);
      });

      bool[] mantenerInicial = new bool[definition.units.Count];
      for (int i = 0; i < desiredInitialUnits && i < indicesOrdenados.Count; i++)
      {
         mantenerInicial[indicesOrdenados[i]] = true;
      }

      for (int i = 0; i < definition.units.Count; i++)
      {
         if (definition.units[i] == null)
         {
            continue;
         }

         definition.units[i].spawnAsReinforcement = !mantenerInicial[i];
      }
   }

   static bool HasReinforcements(EncounterDefinition definition)
   {
      if (definition == null || definition.units == null)
      {
         return false;
      }

      foreach (var slot in definition.units)
      {
         if (slot != null && slot.spawnAsReinforcement)
         {
            return true;
         }
      }

      return false;
   }

   static int GetDesiredInitialUnits(BattleEncounterType battleType, int fase, int totalUnits, int initialCap)
   {
      int desired;
      switch (battleType)
      {
         case BattleEncounterType.Elite:
            desired = 4;
            break;
         case BattleEncounterType.AtaqueCaravana:
            desired = 4 + Mathf.Clamp(fase - 1, 0, 2);
            break;
         case BattleEncounterType.Subterraneo:
            desired = 4;
            break;
         case BattleEncounterType.Normal:
         default:
            desired = 4 + Mathf.Clamp((fase - 1) / 2, 0, 1);
            break;
      }

      return Mathf.Clamp(desired, 1, Mathf.Min(initialCap, totalUnits));
   }

   static int GetInitialUnitCap(BattleEncounterType battleType, int fase)
   {
      int cap;
      switch (battleType)
      {
         case BattleEncounterType.Elite:
            cap = 4;
            break;
         case BattleEncounterType.AtaqueCaravana:
            cap = 6;
            break;
         case BattleEncounterType.Subterraneo:
            cap = 5;
            break;
         case BattleEncounterType.Normal:
         default:
            cap = 5;
            break;
      }

      cap += Mathf.Clamp((fase - 1) / 3, 0, 1);
      return Mathf.Clamp(cap, 3, MaxInitialUnits);
   }

   static int GetSoftMaxUnits(BattleEncounterType battleType, int fase, int budget, int cheapestTier, int minUnits, int maxUnitsPossible)
   {
      int maxUnits;
      switch (battleType)
      {
         case BattleEncounterType.Elite:
            maxUnits = 4;
            break;
         case BattleEncounterType.AtaqueCaravana:
            maxUnits = 6;
            break;
         case BattleEncounterType.Subterraneo:
            maxUnits = 5;
            break;
         case BattleEncounterType.Normal:
         default:
            maxUnits = 5;
            break;
      }

      maxUnits += Mathf.Clamp((fase - 1) / 2, 0, 2);
      if (budget >= cheapestTier * (minUnits + 3))
      {
         maxUnits++;
      }

      return Mathf.Clamp(maxUnits, minUnits, Mathf.Max(minUnits, maxUnitsPossible));
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

   static List<int> GetAvailableTiers(EnemyFactionConfig faction, int remainingBudget, int maxTierAllowed, Dictionary<string, int> uniqueUsage, string ignoreUniqueKey = null)
   {
      var tiers = new List<int>();

      if (HasEligibleTier(faction, 1, uniqueUsage, ignoreUniqueKey) && remainingBudget >= 1)
      {
         tiers.Add(1);
      }
      if (maxTierAllowed >= 2 && HasEligibleTier(faction, 2, uniqueUsage, ignoreUniqueKey) && remainingBudget >= 2)
      {
         tiers.Add(2);
      }
      if (maxTierAllowed >= 3 && HasEligibleTier(faction, 3, uniqueUsage, ignoreUniqueKey) && remainingBudget >= 3)
      {
         tiers.Add(3);
      }
      if (maxTierAllowed >= 4 && HasEligibleTier(faction, 4, uniqueUsage, ignoreUniqueKey) && remainingBudget >= 4)
      {
         tiers.Add(4);
      }
      if (maxTierAllowed >= 5 && HasEligibleTier(faction, 5, uniqueUsage, ignoreUniqueKey) && remainingBudget >= 5)
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

   static bool HasEligibleTier(EnemyFactionConfig faction, int tier, Dictionary<string, int> uniqueUsage, string ignoreUniqueKey = null)
   {
      var list = GetEligibleTierList(faction, tier, uniqueUsage, ignoreUniqueKey);
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

   static int GetHighestTierAvailable(EnemyFactionConfig faction, int maxTierAllowed)
   {
      if (faction == null)
      {
         return 0;
      }

      for (int tier = Mathf.Clamp(maxTierAllowed, 1, 5); tier >= 1; tier--)
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

   static List<GameObject> GetEligibleTierList(EnemyFactionConfig faction, int tier, Dictionary<string, int> uniqueUsage, string ignoreUniqueKey = null)
   {
      var tierList = GetTierList(faction, tier);
      if (tierList == null || tierList.Count == 0)
      {
         return null;
      }

      List<GameObject> elegibles = new List<GameObject>();
      foreach (GameObject prefab in tierList)
      {
         if (PrefabEsElegibleSegunUnicos(prefab, uniqueUsage, ignoreUniqueKey))
         {
            elegibles.Add(prefab);
         }
      }

      return elegibles;
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

   static GameObject GetPrefabFromTier(EnemyFactionConfig faction, int tier, Dictionary<string, int> uniqueUsage = null, string ignoreUniqueKey = null)
   {
      var tierList = GetEligibleTierList(faction, tier, uniqueUsage, ignoreUniqueKey);
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

   static int GetMinUnitsFloor(BattleEncounterType battleType, int fase)
   {
      switch (battleType)
      {
         case BattleEncounterType.Elite:
            return 3;
         case BattleEncounterType.AtaqueCaravana:
            return 4;
         case BattleEncounterType.Subterraneo:
            return fase >= 2 ? 3 : 2;
         case BattleEncounterType.Normal:
         default:
            return fase >= 3 ? 4 : 3;
      }
   }

   static int GetMinBudgetFloor(BattleEncounterType battleType, int fase, int cheapestTier, int minUnits, int highestTierAvailable)
   {
      int safeCheapestTier = Mathf.Max(1, cheapestTier);
      int safeMinUnits = Mathf.Max(1, minUnits);
      int safeHighestTier = Mathf.Max(safeCheapestTier, highestTierAvailable);
      int baseFloor = safeCheapestTier * safeMinUnits;
      int strongFloor = safeCheapestTier * Mathf.Max(0, safeMinUnits - 1) + safeHighestTier;

      switch (battleType)
      {
         case BattleEncounterType.Elite:
            return Mathf.Max(baseFloor + 2 + Mathf.Max(1, fase), strongFloor + 2);
         case BattleEncounterType.AtaqueCaravana:
            return Mathf.Max(baseFloor + 3 + Mathf.Clamp(fase, 0, 3), strongFloor + 1);
         case BattleEncounterType.Subterraneo:
            return Mathf.Max(baseFloor + 1 + Mathf.Clamp((fase - 1) / 2, 0, 2), strongFloor);
         case BattleEncounterType.Normal:
         default:
            return Mathf.Max(baseFloor + 1 + Mathf.Clamp((fase - 1) / 2, 0, 2), strongFloor);
      }
   }

   static void EnforceMinimumComposition(
      EncounterDefinition definition,
      EnemyFactionConfig faction,
      BattleEncounterType battleType,
      int maxTierAllowed,
      int minUnits)
   {
      if (definition == null || faction == null)
      {
         return;
      }

      int cheapestTier = GetCheapestTierAvailable(faction, maxTierAllowed);
      if (cheapestTier == 0)
      {
         return;
      }

      Dictionary<GameObject, int> prefabUsage = BuildPrefabUsage(definition);
      Dictionary<string, int> uniqueUsage = BuildUniqueUsage(definition);

      int allowedInitial = Mathf.Min(definition.initialCap, MaxInitialUnits);
      int initialCount = 0;
      foreach (var slot in definition.units)
      {
         if (slot != null && !slot.spawnAsReinforcement)
         {
            initialCount++;
         }
      }

      while (definition.units.Count < minUnits)
      {
         var prefab = GetPrefabFromTier(faction, cheapestTier, uniqueUsage);
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
         RegistrarUsoPrefab(prefabUsage, prefab, 1);
         RegistrarUsoUnico(uniqueUsage, prefab, 1);

         if (!spawnAsReinforcement)
         {
            initialCount++;
         }
      }

      if (battleType != BattleEncounterType.Elite)
      {
         return;
      }

      int eliteTier = GetLowestAvailableTierFrom(faction, 2, maxTierAllowed, uniqueUsage);
      if (eliteTier == 0)
      {
         return;
      }

      bool hasEliteUnit = false;
      foreach (var slot in definition.units)
      {
         if (slot != null && slot.tierCost >= 2)
         {
            hasEliteUnit = true;
            break;
         }
      }

      if (hasEliteUnit)
      {
         return;
      }

      int replaceIndex = -1;
      int minTier = int.MaxValue;
      for (int i = 0; i < definition.units.Count; i++)
      {
         var slot = definition.units[i];
         if (slot == null)
         {
            continue;
         }

         if (slot.tierCost < minTier)
         {
            minTier = slot.tierCost;
            replaceIndex = i;
         }
      }

      string uniqueKeyReemplazo = replaceIndex >= 0 ? GetUniqueEncounterKey(definition.units[replaceIndex].prefab) : null;
      var elitePrefab = GetPrefabFromTier(faction, eliteTier, uniqueUsage, uniqueKeyReemplazo);
      if (elitePrefab == null)
      {
         return;
      }

      if (replaceIndex >= 0)
      {
         RegistrarUsoPrefab(prefabUsage, definition.units[replaceIndex].prefab, -1);
         RegistrarUsoUnico(uniqueUsage, definition.units[replaceIndex].prefab, -1);
         definition.units[replaceIndex].prefab = elitePrefab;
         definition.units[replaceIndex].tierCost = eliteTier;
         RegistrarUsoPrefab(prefabUsage, elitePrefab, 1);
         RegistrarUsoUnico(uniqueUsage, elitePrefab, 1);
      }
      else
      {
         bool spawnAsReinforcement = initialCount >= allowedInitial;
         definition.units.Add(new EncounterUnitSlot
         {
            prefab = elitePrefab,
            tierCost = eliteTier,
            spawnAsReinforcement = spawnAsReinforcement
         });
         RegistrarUsoPrefab(prefabUsage, elitePrefab, 1);
         RegistrarUsoUnico(uniqueUsage, elitePrefab, 1);
      }
   }

   static void EnsureMandatoryReinforcements(EncounterDefinition definition, EnemyFactionConfig faction, int maxTierAllowed)
   {
      if (definition == null || faction == null || definition.battleType != BattleEncounterType.AtaqueCaravana)
      {
         return;
      }

      int cheapestTier = GetCheapestTierAvailable(faction, maxTierAllowed);
      if (cheapestTier == 0)
      {
         return;
      }

      Dictionary<GameObject, int> prefabUsage = BuildPrefabUsage(definition);
      Dictionary<string, int> uniqueUsage = BuildUniqueUsage(definition);

      int desiredInitialUnits = GetDesiredInitialUnits(
         definition.battleType,
         definition.fase,
         Mathf.Max(1, definition.units.Count),
         definition.initialCap);
      int requiredTotalUnits = Mathf.Max(5, desiredInitialUnits + 1);
      int safetyCounter = 0;

      while (definition.units.Count < requiredTotalUnits && safetyCounter < 8)
      {
         safetyCounter++;
         var prefab = GetPrefabFromTier(faction, cheapestTier, uniqueUsage);
         if (prefab == null)
         {
            break;
         }

         definition.units.Add(new EncounterUnitSlot
         {
            prefab = prefab,
            tierCost = cheapestTier,
            spawnAsReinforcement = false
         });
         RegistrarUsoPrefab(prefabUsage, prefab, 1);
         RegistrarUsoUnico(uniqueUsage, prefab, 1);
      }
   }

   static int GetLowestAvailableTierFrom(EnemyFactionConfig faction, int minTier, int maxTierAllowed, Dictionary<string, int> uniqueUsage = null, string ignoreUniqueKey = null)
   {
      int start = Mathf.Clamp(minTier, 1, 5);
      int end = Mathf.Clamp(maxTierAllowed, 1, 5);
      for (int tier = start; tier <= end; tier++)
      {
         if (HasEligibleTier(faction, tier, uniqueUsage, ignoreUniqueKey))
         {
            return tier;
         }
      }

      return 0;
   }

   static void EnforceUniqueUnits(EncounterDefinition definition, EnemyFactionConfig faction, BattleEncounterType battleType, int maxTierAllowed, int minUnits)
   {
      if (definition == null || definition.units == null || faction == null)
      {
         return;
      }

      Dictionary<GameObject, int> prefabUsage = BuildPrefabUsage(definition);
      Dictionary<string, int> uniqueUsage = BuildUniqueUsage(definition);

      for (int i = 0; i < definition.units.Count; i++)
      {
        var slot = definition.units[i];
        if (slot == null || slot.prefab == null)
        {
           continue;
        }

        string uniqueKey = GetUniqueEncounterKey(slot.prefab);
        if (string.IsNullOrEmpty(uniqueKey))
        {
           continue;
        }

        int usos = 0;
        uniqueUsage.TryGetValue(uniqueKey, out usos);
        if (usos <= 1)
        {
           continue;
        }

        RegistrarUsoPrefab(prefabUsage, slot.prefab, -1);
        RegistrarUsoUnico(uniqueUsage, slot.prefab, -1);

        GameObject reemplazo = BuscarReemplazoParaDuplicadoUnico(faction, slot.tierCost, maxTierAllowed, prefabUsage, uniqueUsage);
        if (reemplazo != null)
        {
           slot.prefab = reemplazo;
           slot.tierCost = ObtenerTierDelPrefab(faction, reemplazo);
           RegistrarUsoPrefab(prefabUsage, reemplazo, 1);
           RegistrarUsoUnico(uniqueUsage, reemplazo, 1);
           continue;
        }

        definition.units.RemoveAt(i);
        i--;
      }

      EnforceMinimumComposition(definition, faction, battleType, maxTierAllowed, minUnits);
      EnsureMandatoryReinforcements(definition, faction, maxTierAllowed);
   }

   static GameObject BuscarReemplazoParaDuplicadoUnico(
      EnemyFactionConfig faction,
      int tierActual,
      int maxTierAllowed,
      Dictionary<GameObject, int> prefabUsage,
      Dictionary<string, int> uniqueUsage)
   {
      if (faction == null)
      {
         return null;
      }

      List<int> ordenTiers = new List<int>();
      int tierBase = Mathf.Clamp(tierActual, 1, 5);
      if (tierBase <= maxTierAllowed)
      {
         ordenTiers.Add(tierBase);
      }

      for (int delta = 1; delta <= 4; delta++)
      {
         int abajo = tierBase - delta;
         int arriba = tierBase + delta;
         if (abajo >= 1 && !ordenTiers.Contains(abajo))
         {
            ordenTiers.Add(abajo);
         }

         if (arriba <= maxTierAllowed && !ordenTiers.Contains(arriba))
         {
            ordenTiers.Add(arriba);
         }
      }

      foreach (int tier in ordenTiers)
      {
         GameObject candidato = PickPrefabFromTierWithVariety(faction, tier, prefabUsage, uniqueUsage);
         if (candidato != null)
         {
            return candidato;
         }
      }

      return null;
   }

   static int ObtenerTierDelPrefab(EnemyFactionConfig faction, GameObject prefab)
   {
      if (faction == null || prefab == null)
      {
         return 0;
      }

      for (int tier = 1; tier <= 5; tier++)
      {
         var tierList = GetTierList(faction, tier);
         if (tierList != null && tierList.Contains(prefab))
         {
            return tier;
         }
      }

      return 0;
   }

   static Dictionary<GameObject, int> BuildPrefabUsage(EncounterDefinition definition)
   {
      Dictionary<GameObject, int> usage = new Dictionary<GameObject, int>();
      if (definition == null || definition.units == null)
      {
         return usage;
      }

      foreach (var slot in definition.units)
      {
         if (slot == null || slot.prefab == null)
         {
            continue;
         }

         RegistrarUsoPrefab(usage, slot.prefab, 1);
      }

      return usage;
   }

   static Dictionary<string, int> BuildUniqueUsage(EncounterDefinition definition)
   {
      Dictionary<string, int> usage = new Dictionary<string, int>();
      if (definition == null || definition.units == null)
      {
         return usage;
      }

      foreach (var slot in definition.units)
      {
         if (slot == null || slot.prefab == null)
         {
            continue;
         }

         RegistrarUsoUnico(usage, slot.prefab, 1);
      }

      return usage;
   }

   static bool PrefabEsElegibleSegunUnicos(GameObject prefab, Dictionary<string, int> uniqueUsage, string ignoreUniqueKey = null)
   {
      if (prefab == null)
      {
         return false;
      }

      string uniqueKey = GetUniqueEncounterKey(prefab);
      if (string.IsNullOrEmpty(uniqueKey))
      {
         return true;
      }

      if (!string.IsNullOrEmpty(ignoreUniqueKey) && uniqueKey == ignoreUniqueKey)
      {
         return true;
      }

      if (uniqueUsage == null)
      {
         return true;
      }

      int usos = 0;
      uniqueUsage.TryGetValue(uniqueKey, out usos);
      return usos < 1;
   }

   static string GetUniqueEncounterKey(GameObject prefab)
   {
      if (prefab == null)
      {
         return string.Empty;
      }

      IAUnidad iaUnidad = prefab.GetComponent<IAUnidad>();
      if (iaUnidad == null || !iaUnidad.unicoEnCombate)
      {
         return string.Empty;
      }

      return prefab.name.Trim().ToLowerInvariant();
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

   static int GetReinforcementDelay(BattleEncounterType battleType, int fase)
   {
      int minDelay;
      int maxDelay;

      switch (battleType)
      {
         case BattleEncounterType.Elite:
            minDelay = 2;
            maxDelay = 3;
            break;
         case BattleEncounterType.AtaqueCaravana:
            minDelay = 1;
            maxDelay = fase >= 3 ? 2 : 3;
            break;
         case BattleEncounterType.Subterraneo:
            minDelay = 1;
            maxDelay = 2;
            break;
         case BattleEncounterType.Normal:
         default:
            minDelay = 1;
            maxDelay = 3;
            break;
      }

      return UnityEngine.Random.Range(minDelay, maxDelay + 1);
   }

   static int RollBudgetBonus(BattleEncounterType battleType, int fase)
   {
      int negativeSwing;
      int positiveSwing;

      switch (battleType)
      {
         case BattleEncounterType.AtaqueCaravana:
            negativeSwing = 2;
            positiveSwing = 4;
            break;
         case BattleEncounterType.Elite:
            negativeSwing = 1;
            positiveSwing = 3;
            break;
         case BattleEncounterType.Subterraneo:
            negativeSwing = 2;
            positiveSwing = 2;
            break;
         case BattleEncounterType.Normal:
         default:
            negativeSwing = 2;
            positiveSwing = 2;
            break;
      }

      int faseBias = Mathf.Clamp((fase - 1) / 2, 0, 2);
      int rollA = UnityEngine.Random.Range(-negativeSwing, positiveSwing + 1);
      int rollB = UnityEngine.Random.Range(-1, 2);
      return rollA + rollB + faseBias;
   }
}



