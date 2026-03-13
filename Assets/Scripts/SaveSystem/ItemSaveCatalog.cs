using System;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class ItemSaveCatalog
{
  public static ItemDatabase GetRuntimeItemDatabase(CampaignManager campaignManager)
  {
    if (campaignManager == null)
    {
      return null;
    }

    if (campaignManager.scSequitoMercaderes != null)
    {
      ItemDatabase database = campaignManager.scSequitoMercaderes.GetItemDatabase();
      if (database != null)
      {
        return database;
      }
    }

    if (campaignManager.scMenuSequito != null && campaignManager.scMenuSequito.Sequito003Mercaderes != null)
    {
      SequitoMercaderes mercaderesPrefab = campaignManager.scMenuSequito.Sequito003Mercaderes.GetComponent<SequitoMercaderes>();
      if (mercaderesPrefab != null)
      {
        return mercaderesPrefab.GetItemDatabase();
      }
    }

    return null;
  }

  public static string ResolveItemId(Item item, ItemDatabase database)
  {
    if (item == null || database == null || database.items == null)
    {
      return string.Empty;
    }

    string currentId = item.GetPersistentItemId();
    if (!string.IsNullOrWhiteSpace(currentId))
    {
      return currentId;
    }

    for (int i = 0; i < database.items.Count; i++)
    {
      ItemDatabaseEntry entry = database.items[i];
      if (entry == null || entry.prefab == null)
      {
        continue;
      }

      if (!ItemMatchesEntry(item, entry.prefab))
      {
        continue;
      }

      item.SetPersistentItemId(entry.id);
      return entry.id;
    }

    ItemDatabaseEntry relaxedEntry = FindBestRelaxedMatch(item, database);
    if (relaxedEntry != null)
    {
      item.SetPersistentItemId(relaxedEntry.id);
      return relaxedEntry.id;
    }

    return string.Empty;
  }

  public static Item InstantiateItemById(string itemId, ItemDatabase database)
  {
    if (string.IsNullOrWhiteSpace(itemId) || database == null)
    {
      return null;
    }

    ItemDatabaseEntry entry = database.BuscarPorId(itemId);
    if (entry == null || entry.prefab == null)
    {
      return null;
    }

    Item instancia = UnityEngine.Object.Instantiate(entry.prefab);
    instancia.SetPersistentItemId(entry.id);
    return instancia;
  }

  private static bool ItemMatchesEntry(Item instance, Item prefab)
  {
    if (instance == null || prefab == null)
    {
      return false;
    }

    if (instance.GetType() != prefab.GetType())
    {
      return false;
    }

    if (!string.Equals(instance.sNombreItem, prefab.sNombreItem, StringComparison.Ordinal))
    {
      return false;
    }

    if (instance.iRareza != prefab.iRareza)
    {
      return false;
    }

    if (instance.nivelMejora != prefab.nivelMejora)
    {
      return false;
    }

    if (instance.IDEfectoEspecial != prefab.IDEfectoEspecial)
    {
      return false;
    }

    return instance.iPrecio == prefab.iPrecio;
  }

  private static ItemDatabaseEntry FindBestRelaxedMatch(Item instance, ItemDatabase database)
  {
    if (instance == null || database == null || database.items == null)
    {
      return null;
    }

    ItemDatabaseEntry bestEntry = null;
    int bestScore = int.MinValue;
    string normalizedInstanceName = NormalizeItemName(instance.sNombreItem);

    for (int i = 0; i < database.items.Count; i++)
    {
      ItemDatabaseEntry entry = database.items[i];
      if (entry == null || entry.prefab == null)
      {
        continue;
      }

      int score = ScoreRelaxedMatch(instance, entry.prefab, normalizedInstanceName);
      if (score > bestScore)
      {
        bestScore = score;
        bestEntry = entry;
      }
    }

    return bestScore > 0 ? bestEntry : null;
  }

  private static int ScoreRelaxedMatch(Item instance, Item prefab, string normalizedInstanceName)
  {
    if (!IsSameItemCategory(instance, prefab))
    {
      return int.MinValue;
    }

    if (instance.iRareza != prefab.iRareza)
    {
      return int.MinValue;
    }

    if (instance.nivelMejora != prefab.nivelMejora)
    {
      return int.MinValue;
    }

    if (instance.IDEfectoEspecial != prefab.IDEfectoEspecial)
    {
      return int.MinValue;
    }

    string normalizedPrefabName = NormalizeItemName(prefab.sNombreItem);
    bool nameMatches = !string.IsNullOrEmpty(normalizedInstanceName)
      && normalizedInstanceName == normalizedPrefabName;
    bool spriteMatches = instance.imItem != null && instance.imItem == prefab.imItem;
    bool priceMatches = instance.iPrecio == prefab.iPrecio;

    if (!nameMatches && !(spriteMatches && priceMatches))
    {
      return int.MinValue;
    }

    int score = 0;
    if (nameMatches)
    {
      score += 100;
    }

    if (priceMatches)
    {
      score += 20;
    }

    if (spriteMatches)
    {
      score += 10;
    }

    if (instance.GetType() == prefab.GetType())
    {
      score += 5;
    }

    return score;
  }

  private static bool IsSameItemCategory(Item instance, Item prefab)
  {
    if (instance == null || prefab == null)
    {
      return false;
    }

    if (instance is Arma)
    {
      return prefab is Arma;
    }

    if (instance is Armadura)
    {
      return prefab is Armadura;
    }

    if (instance is Accesorio)
    {
      return prefab is Accesorio;
    }

    if (instance is Consumible)
    {
      return prefab is Consumible;
    }

    return instance.GetType() == prefab.GetType();
  }

  private static string NormalizeItemName(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return string.Empty;
    }

    string normalized = value.Trim().Normalize(NormalizationForm.FormD);
    StringBuilder builder = new StringBuilder(normalized.Length);

    for (int i = 0; i < normalized.Length; i++)
    {
      char current = normalized[i];
      UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(current);
      if (category == UnicodeCategory.NonSpacingMark)
      {
        continue;
      }

      if (char.IsLetterOrDigit(current))
      {
        builder.Append(char.ToLowerInvariant(current));
      }
    }

    return builder.ToString();
  }
}
