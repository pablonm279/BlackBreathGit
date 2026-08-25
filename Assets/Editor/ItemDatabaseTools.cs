using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ItemDatabaseTools
{
    private const string MenuRoot = "Tools/Items/";
    private const string DatabaseFolderPath = "Assets/Data";
    private const string DatabaseAssetPath = "Assets/Data/ItemDatabase.asset";

    [MenuItem(MenuRoot + "Create Or Refresh Item Database")]
    public static void CreateOrRefreshItemDatabase()
    {
        ItemDatabase database = GetOrCreateDatabase(out bool createdDatabase);
        Dictionary<string, ItemDatabaseEntry> existingByGuid = IndexExistingEntries(database);

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        List<ItemDatabaseEntry> refreshedEntries = new List<ItemDatabaseEntry>();
        HashSet<string> usedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        int scannedItems = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                continue;
            }

            Item item = prefab.GetComponent<Item>();
            if (item == null)
            {
                continue;
            }

            scannedItems++;
            ItemDatabaseEntry entry;

            if (existingByGuid.TryGetValue(guid, out ItemDatabaseEntry oldEntry))
            {
                entry = CloneEntry(oldEntry);
                entry.prefab = item;

                // One-time init for legacy entries created before full fields existed.
                if (string.IsNullOrWhiteSpace(entry.listaTitulo))
                {
                    string keepId = entry.id;
                    List<string> keepTags = entry.tags != null ? new List<string>(entry.tags) : new List<string>();
                    bool keepActivo = entry.activo;
                    bool keepExcluirDeTiendas = entry.excluirDeTiendas;

                    PopulateEntryFromItem(entry, item, path);

                    entry.id = keepId;
                    entry.tags = keepTags;
                    entry.activo = keepActivo;
                    entry.excluirDeTiendas = keepExcluirDeTiendas;
                }

                EnsureEntryDefaultsFromItem(entry, item, path);
            }
            else
            {
                entry = new ItemDatabaseEntry();
                PopulateEntryFromItem(entry, item, path);
            }

            if (string.IsNullOrWhiteSpace(entry.id))
            {
                entry.id = BuildDefaultId(path);
            }

            entry.id = EnsureUniqueId(entry.id, path, usedIds);
            entry.listaTitulo = BuildListTitle(entry, prefab.name);

            refreshedEntries.Add(entry);
        }

        refreshedEntries.Sort(CompareEntries);
        database.items = refreshedEntries;

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);

        string msg =
            $"ItemDatabase actualizado.\n\nItems escaneados: {scannedItems}\nEntradas en database: {database.items.Count}\nAsset: {DatabaseAssetPath}\nCreado ahora: {createdDatabase}";
        Debug.Log("[Items] " + msg.Replace("\n", " "));
        EditorUtility.DisplayDialog("Item Database", msg, "OK");
    }

    [MenuItem(MenuRoot + "Sync Database From Prefabs (Overwrite Fields)")]
    public static void SyncDatabaseFromPrefabsOverwriteFields()
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
        if (database == null)
        {
            EditorUtility.DisplayDialog(
                "Item Database",
                $"No existe '{DatabaseAssetPath}'.\n\nPrimero ejecuta: Tools/Items/Create Or Refresh Item Database",
                "OK");
            return;
        }

        Dictionary<string, ItemDatabaseEntry> existingByGuid = IndexExistingEntries(database);
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        List<ItemDatabaseEntry> refreshedEntries = new List<ItemDatabaseEntry>();
        HashSet<string> usedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        int scannedItems = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                continue;
            }

            Item item = prefab.GetComponent<Item>();
            if (item == null)
            {
                continue;
            }

            scannedItems++;
            ItemDatabaseEntry entry = new ItemDatabaseEntry();

            if (existingByGuid.TryGetValue(guid, out ItemDatabaseEntry oldEntry))
            {
                entry.id = oldEntry.id;
                entry.tags = oldEntry.tags != null ? new List<string>(oldEntry.tags) : new List<string>();
                entry.activo = oldEntry.activo;
                entry.excluirDeTiendas = oldEntry.excluirDeTiendas;
            }

            PopulateEntryFromItem(entry, item, path);

            if (string.IsNullOrWhiteSpace(entry.id))
            {
                entry.id = BuildDefaultId(path);
            }

            entry.id = EnsureUniqueId(entry.id, path, usedIds);
            entry.listaTitulo = BuildListTitle(entry, prefab.name);

            refreshedEntries.Add(entry);
        }

        refreshedEntries.Sort(CompareEntries);
        database.items = refreshedEntries;

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);

        string msg =
            $"Sincronizacion desde prefabs completada.\n\nItems escaneados: {scannedItems}\nEntradas en database: {database.items.Count}\nCampos sobreescritos desde prefab: SI";
        Debug.Log("[Items] " + msg.Replace("\n", " "));
        EditorUtility.DisplayDialog("Item Database", msg, "OK");
    }

    [MenuItem(MenuRoot + "Apply Database To Prefabs")]
    public static void ApplyDatabaseToPrefabs()
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
        if (database == null)
        {
            EditorUtility.DisplayDialog(
                "Item Database",
                $"No existe '{DatabaseAssetPath}'.\n\nPrimero ejecuta: Tools/Items/Create Or Refresh Item Database",
                "OK");
            return;
        }

        if (database.items == null)
        {
            EditorUtility.DisplayDialog("Item Database", "La lista de items esta vacia.", "OK");
            return;
        }

        int scanned = 0;
        int changed = 0;
        int missingPrefab = 0;
        StringBuilder missingNames = new StringBuilder();

        for (int i = 0; i < database.items.Count; i++)
        {
            ItemDatabaseEntry entry = database.items[i];
            if (entry == null)
            {
                continue;
            }

            if (entry.prefab == null)
            {
                missingPrefab++;
                missingNames.AppendLine($"- {entry.id} ({entry.listaTitulo})");
                continue;
            }

            scanned++;
            if (ApplyEntryToItem(entry, entry.prefab))
            {
                EditorUtility.SetDirty(entry.prefab);
                changed++;
            }

            // Keep list label in sync.
            entry.listaTitulo = BuildListTitle(entry, entry.prefab.name);
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string msg =
            $"Aplicacion completada.\n\nEntradas procesadas: {scanned}\nPrefabs modificados: {changed}\nEntradas sin prefab: {missingPrefab}";

        if (missingPrefab > 0)
        {
            msg += "\n\nEntradas sin prefab:\n" + missingNames;
        }

        Debug.Log("[Items] " + msg.Replace("\n", " "));
        EditorUtility.DisplayDialog("Item Database", msg, "OK");
    }

    [MenuItem(MenuRoot + "Normalize Prices And Repair Catalog")]
    public static void NormalizePricesAndRepairCatalog()
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
        if (database == null || database.items == null)
        {
            Debug.LogError("[Items] No se pudo normalizar: falta el ItemDatabase.");
            return;
        }

        int pricesAdjusted = 0;
        int metadataRepaired = 0;
        int missingPrefabsDisabled = 0;

        foreach (ItemDatabaseEntry entry in database.items)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.prefab == null)
            {
                // No se elimina la ficha: conserva sus datos para una futura restauracion del prefab,
                // pero queda fuera de cualquier listado de tienda o recompensa.
                if (entry.activo || !entry.excluirDeTiendas)
                {
                    entry.activo = false;
                    entry.excluirDeTiendas = true;
                    missingPrefabsDisabled++;
                }

                continue;
            }

            // El prefab es la fuente que usa la tienda al instanciar el item. Repara metadatos viejos
            // sin rebajar un nivel de mejora valido del contenido.
            if (entry.rareza != entry.prefab.iRareza)
            {
                entry.rareza = entry.prefab.iRareza;
                metadataRepaired++;
            }

            if (entry.nivelMejora != entry.prefab.nivelMejora)
            {
                entry.nivelMejora = entry.prefab.nivelMejora;
                metadataRepaired++;
            }

            int floor = GetPriceFloor(entry.categoria, entry.rareza, entry.nivelMejora);
            if (entry.precio < floor)
            {
                entry.precio = floor;
                pricesAdjusted++;
            }

            if (entry.prefab.iPrecio != entry.precio)
            {
                entry.prefab.iPrecio = entry.precio;
                EditorUtility.SetDirty(entry.prefab);
                metadataRepaired++;
            }
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[Items] Normalizacion completada. Precios corregidos: {pricesAdjusted}. "
            + $"Metadatos/prefabs sincronizados: {metadataRepaired}. "
            + $"Entradas sin prefab desactivadas: {missingPrefabsDisabled}.");
    }

    private static int GetPriceFloor(string categoria, int rareza, int nivelMejora)
    {
        int[] floorsByRarity;
        int[] floorsByUpgrade;

        switch (categoria)
        {
            case "Arma":
                // La progresion historica del equipo es 45 / 125 / 235 / 470.
                // Rareza y mejora no se suman: representan el mismo escalon base de poder.
                floorsByRarity = new[] { 45, 125, 235, 470, 650, 900 };
                floorsByUpgrade = new[] { 0, 125, 235, 470, 650, 900 };
                break;
            case "Armadura":
                floorsByRarity = new[] { 45, 135, 230, 310, 500, 750 };
                floorsByUpgrade = new[] { 0, 135, 230, 310, 500, 750 };
                break;
            case "Accesorio":
                floorsByRarity = new[] { 130, 225, 420, 700, 980, 1250 };
                floorsByUpgrade = new[] { 0, 0, 0, 0, 0, 0 };
                break;
            case "Consumible":
                floorsByRarity = new[] { 45, 85, 165, 260, 380, 520 };
                floorsByUpgrade = new[] { 0, 0, 0, 0, 0, 0 };
                break;
            default:
                return 0;
        }

        int rarityFloor = floorsByRarity[Mathf.Clamp(rareza, 0, floorsByRarity.Length - 1)];
        int upgradeFloor = floorsByUpgrade[Mathf.Clamp(nivelMejora, 0, floorsByUpgrade.Length - 1)];
        return Mathf.Max(rarityFloor, upgradeFloor);
    }

    [MenuItem(MenuRoot + "Open Item Database")]
    public static void OpenItemDatabase()
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
        if (database == null)
        {
            EditorUtility.DisplayDialog(
                "Item Database",
                $"No existe '{DatabaseAssetPath}'.\n\nPrimero ejecuta: Tools/Items/Create Or Refresh Item Database",
                "OK");
            return;
        }

        Selection.activeObject = database;
        EditorGUIUtility.PingObject(database);
    }

    public static void CreatePrefabFromCategoryTemplate(string category)
    {
        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
        if (database == null)
        {
            EditorUtility.DisplayDialog(
                "Item Database",
                $"No existe '{DatabaseAssetPath}'.\n\nPrimero ejecuta: Tools/Items/Create Or Refresh Item Database",
                "OK");
            return;
        }

        if (database.items == null || database.items.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Item Database",
                "La base de datos no tiene entradas para usar como plantilla.",
                "OK");
            return;
        }

        ItemDatabaseEntry templateEntry = null;
        for (int i = 0; i < database.items.Count; i++)
        {
            ItemDatabaseEntry entry = database.items[i];
            if (entry == null || entry.prefab == null)
            {
                continue;
            }

            if (string.Equals(entry.categoria, category, System.StringComparison.OrdinalIgnoreCase))
            {
                templateEntry = entry;
                break;
            }
        }

        if (templateEntry == null || templateEntry.prefab == null)
        {
            EditorUtility.DisplayDialog(
                "Crear Item",
                $"No hay plantilla disponible para la categoria '{category}'.",
                "OK");
            return;
        }

        string templatePath = AssetDatabase.GetAssetPath(templateEntry.prefab);
        if (string.IsNullOrWhiteSpace(templatePath))
        {
            EditorUtility.DisplayDialog(
                "Crear Item",
                "No se pudo resolver la ruta del prefab plantilla.",
                "OK");
            return;
        }

        string folder = Path.GetDirectoryName(templatePath)?.Replace("\\", "/");
        string defaultFileName = BuildDefaultNewPrefabName(category);

        string newPath = EditorUtility.SaveFilePanelInProject(
            "Crear Item Nuevo",
            defaultFileName,
            "prefab",
            $"Se duplicara una plantilla de categoria '{category}'.",
            string.IsNullOrWhiteSpace(folder) ? "Assets" : folder);

        if (string.IsNullOrWhiteSpace(newPath))
        {
            return;
        }

        if (!AssetDatabase.CopyAsset(templatePath, newPath))
        {
            EditorUtility.DisplayDialog(
                "Crear Item",
                $"No se pudo crear el prefab en:\n{newPath}",
                "OK");
            return;
        }

        AssetDatabase.Refresh();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
        Item item = prefab != null ? prefab.GetComponent<Item>() : null;
        if (item != null)
        {
            string nameFromPath = Path.GetFileNameWithoutExtension(newPath);
            item.sNombreItem = nameFromPath;
            item.itemDescripcion = string.Empty;

            if (item is Consumible consumible)
            {
                consumible.efectoConsumible = new ConsumibleEfectoData();
            }

            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
        }

        CreateOrRefreshItemDatabase();

        if (prefab != null)
        {
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }
    }

    private static ItemDatabase GetOrCreateDatabase(out bool createdNow)
    {
        createdNow = false;
        EnsureFolder(DatabaseFolderPath);

        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);
        if (database != null)
        {
            return database;
        }

        database = ScriptableObject.CreateInstance<ItemDatabase>();
        AssetDatabase.CreateAsset(database, DatabaseAssetPath);
        createdNow = true;
        return database;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] parts = folderPath.Split('/');
        if (parts.Length == 0)
        {
            return;
        }

        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private static Dictionary<string, ItemDatabaseEntry> IndexExistingEntries(ItemDatabase database)
    {
        Dictionary<string, ItemDatabaseEntry> map = new Dictionary<string, ItemDatabaseEntry>();
        if (database == null || database.items == null)
        {
            return map;
        }

        foreach (ItemDatabaseEntry entry in database.items)
        {
            if (entry == null || entry.prefab == null)
            {
                continue;
            }

            string path = AssetDatabase.GetAssetPath(entry.prefab);
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrWhiteSpace(guid) || map.ContainsKey(guid))
            {
                continue;
            }

            map.Add(guid, entry);
        }

        return map;
    }

    private static ItemDatabaseEntry CloneEntry(ItemDatabaseEntry source)
    {
        ItemDatabaseEntry entry = new ItemDatabaseEntry();
        if (source == null)
        {
            return entry;
        }

        entry.listaTitulo = source.listaTitulo;
        entry.id = source.id;
        entry.prefab = source.prefab;
        entry.nombre = source.nombre;
        entry.descripcion = source.descripcion;
        entry.categoria = source.categoria;
        entry.rareza = source.rareza;
        entry.icono = source.icono;
        entry.precio = source.precio;
        entry.nivelMejora = source.nivelMejora;
        entry.idEfectoEspecial = source.idEfectoEspecial;

        entry.clasesPermitidas = source.clasesPermitidas != null
            ? new List<int>(source.clasesPermitidas)
            : new List<int>();

        entry.requisitoFue = source.requisitoFue;
        entry.requisitoAgi = source.requisitoAgi;
        entry.requisitoPoder = source.requisitoPoder;

        entry.buffs = CloneBuffs(source.buffs);
        entry.debuffsImpactoArma = CloneDebuffsImpactoArma(source.debuffsImpactoArma);
        entry.efectoConsumible = CloneConsumibleEfectoData(source.efectoConsumible);

        entry.habilidadAtaque = source.habilidadAtaque;
        entry.habilidadExtra1 = source.habilidadExtra1;
        entry.habilidadExtra2 = source.habilidadExtra2;

        entry.tags = source.tags != null ? new List<string>(source.tags) : new List<string>();
        entry.activo = source.activo;
        entry.excluirDeTiendas = source.excluirDeTiendas;

        return entry;
    }

    private static ItemBuffsData CloneBuffs(ItemBuffsData source)
    {
        ItemBuffsData buffs = new ItemBuffsData();
        if (source == null)
        {
            return buffs;
        }

        buffs.buffFuerza = source.buffFuerza;
        buffs.buffAgi = source.buffAgi;
        buffs.buffPoder = source.buffPoder;
        buffs.buffIniciativa = source.buffIniciativa;
        buffs.buffApMax = source.buffApMax;
        buffs.buffValMax = source.buffValMax;
        buffs.buffhpMax = source.buffhpMax;
        buffs.buffArmadura = source.buffArmadura;
        buffs.buffDefensa = source.buffDefensa;
        buffs.buffTSReflejo = source.buffTSReflejo;
        buffs.buffTSFortaleza = source.buffTSFortaleza;
        buffs.buffTSMental = source.buffTSMental;
        buffs.buffResFuego = source.buffResFuego;
        buffs.buffResRayo = source.buffResRayo;
        buffs.buffResHielo = source.buffResHielo;
        buffs.buffResArcano = source.buffResArcano;
        buffs.buffResAcido = source.buffResAcido;
        buffs.buffResNecro = source.buffResNecro;
        buffs.buffResDivino = source.buffResDivino;
        buffs.barreraInicioCombate = source.barreraInicioCombate;
        buffs.evasionInicioCombate = source.evasionInicioCombate;
        buffs.bonusDanioFuegoInicioCombate = source.bonusDanioFuegoInicioCombate;
        buffs.bonusDanioHieloInicioCombate = source.bonusDanioHieloInicioCombate;
        buffs.bonusDanioRayoInicioCombate = source.bonusDanioRayoInicioCombate;
        buffs.bonusDanioAcidoInicioCombate = source.bonusDanioAcidoInicioCombate;
        buffs.bonusDanioArcanoInicioCombate = source.bonusDanioArcanoInicioCombate;
        buffs.bonusDanioNecroInicioCombate = source.bonusDanioNecroInicioCombate;
        buffs.bonusDanioDivinoInicioCombate = source.bonusDanioDivinoInicioCombate;
        buffs.regeneracionVidaInicioCombate = source.regeneracionVidaInicioCombate;
        buffs.regeneracionArmaduraInicioCombate = source.regeneracionArmaduraInicioCombate;
        buffs.reduccionDanioRecibidoPorcentaje = source.reduccionDanioRecibidoPorcentaje;
        buffs.reduccionDanioCriticoRecibidoPorcentaje = source.reduccionDanioCriticoRecibidoPorcentaje;
        buffs.resistenciaEstadosPorcentaje = source.resistenciaEstadosPorcentaje;
        buffs.espinasDanioPlano = source.espinasDanioPlano;
        buffs.espinasDanioPorcentaje = source.espinasDanioPorcentaje;

        return buffs;
    }

    private static void EnsureEntryDefaultsFromItem(ItemDatabaseEntry entry, Item item, string path)
    {
        if (string.IsNullOrWhiteSpace(entry.categoria))
        {
            entry.categoria = ResolverCategoria(item);
        }

        if (string.IsNullOrWhiteSpace(entry.nombre))
        {
            entry.nombre = string.IsNullOrWhiteSpace(item.sNombreItem)
                ? Path.GetFileNameWithoutExtension(path)
                : item.sNombreItem;
        }

        if (string.IsNullOrWhiteSpace(entry.descripcion))
        {
            entry.descripcion = item.itemDescripcion;
        }

        if (entry.icono == null)
        {
            entry.icono = item.imItem;
        }

        if (entry.clasesPermitidas == null)
        {
            entry.clasesPermitidas = CopyIntList(item.IDClasesQuePuedenUsarEsteItem);
        }

        if (entry.buffs == null)
        {
            entry.buffs = new ItemBuffsData();
            entry.buffs.barreraInicioCombate = item.barreraInicioCombate;
            entry.buffs.evasionInicioCombate = item.evasionInicioCombate;
            entry.buffs.bonusDanioFuegoInicioCombate = item.bonusDanioFuegoInicioCombate;
            entry.buffs.bonusDanioHieloInicioCombate = item.bonusDanioHieloInicioCombate;
            entry.buffs.bonusDanioRayoInicioCombate = item.bonusDanioRayoInicioCombate;
            entry.buffs.bonusDanioAcidoInicioCombate = item.bonusDanioAcidoInicioCombate;
            entry.buffs.bonusDanioArcanoInicioCombate = item.bonusDanioArcanoInicioCombate;
            entry.buffs.bonusDanioNecroInicioCombate = item.bonusDanioNecroInicioCombate;
            entry.buffs.bonusDanioDivinoInicioCombate = item.bonusDanioDivinoInicioCombate;
            entry.buffs.regeneracionVidaInicioCombate = item.regeneracionVidaInicioCombate;
            entry.buffs.regeneracionArmaduraInicioCombate = item.regeneracionArmaduraInicioCombate;
            entry.buffs.reduccionDanioRecibidoPorcentaje = item.reduccionDanioRecibidoPorcentaje;
            entry.buffs.reduccionDanioCriticoRecibidoPorcentaje = item.reduccionDanioCriticoRecibidoPorcentaje;
            entry.buffs.resistenciaEstadosPorcentaje = item.resistenciaEstadosPorcentaje;
            entry.buffs.espinasDanioPlano = item.espinasDanioPlano;
            entry.buffs.espinasDanioPorcentaje = item.espinasDanioPorcentaje;
        }
        if (entry.debuffsImpactoArma == null)
        {
            if (item is Arma arma)
            {
                entry.debuffsImpactoArma = CloneDebuffsImpactoArma(arma.debuffsImpactoArma);
            }
            else
            {
                entry.debuffsImpactoArma = new List<DebuffImpactoArmaData>();
            }
        }

        if (entry.efectoConsumible == null)
        {
            if (item is Consumible consumibleDefault)
            {
                entry.efectoConsumible = CloneConsumibleEfectoData(consumibleDefault.ObtenerEfectoConsumibleNormalizado());
            }
            else
            {
                entry.efectoConsumible = new ConsumibleEfectoData();
            }
        }

        if (entry.tags == null)
        {
            entry.tags = new List<string>();
        }
    }

    private static void PopulateEntryFromItem(ItemDatabaseEntry entry, Item item, string path)
    {
        if (entry == null || item == null)
        {
            return;
        }

        entry.prefab = item;
        entry.nombre = string.IsNullOrWhiteSpace(item.sNombreItem)
            ? Path.GetFileNameWithoutExtension(path)
            : item.sNombreItem;
        entry.descripcion = item.itemDescripcion;
        entry.categoria = ResolverCategoria(item);
        entry.rareza = item.iRareza;
        entry.icono = item.imItem;
        entry.precio = item.iPrecio;
        entry.nivelMejora = item.nivelMejora;
        entry.idEfectoEspecial = item.IDEfectoEspecial;
        entry.clasesPermitidas = CopyIntList(item.IDClasesQuePuedenUsarEsteItem);

        entry.requisitoFue = 0;
        entry.requisitoAgi = 0;
        entry.requisitoPoder = 0;
        entry.buffs = new ItemBuffsData();
        entry.buffs.barreraInicioCombate = item.barreraInicioCombate;
        entry.buffs.evasionInicioCombate = item.evasionInicioCombate;
        entry.buffs.bonusDanioFuegoInicioCombate = item.bonusDanioFuegoInicioCombate;
        entry.buffs.bonusDanioHieloInicioCombate = item.bonusDanioHieloInicioCombate;
        entry.buffs.bonusDanioRayoInicioCombate = item.bonusDanioRayoInicioCombate;
        entry.buffs.bonusDanioAcidoInicioCombate = item.bonusDanioAcidoInicioCombate;
        entry.buffs.bonusDanioArcanoInicioCombate = item.bonusDanioArcanoInicioCombate;
        entry.buffs.bonusDanioNecroInicioCombate = item.bonusDanioNecroInicioCombate;
        entry.buffs.bonusDanioDivinoInicioCombate = item.bonusDanioDivinoInicioCombate;
        entry.buffs.regeneracionVidaInicioCombate = item.regeneracionVidaInicioCombate;
        entry.buffs.regeneracionArmaduraInicioCombate = item.regeneracionArmaduraInicioCombate;
        entry.buffs.reduccionDanioRecibidoPorcentaje = item.reduccionDanioRecibidoPorcentaje;
        entry.buffs.reduccionDanioCriticoRecibidoPorcentaje = item.reduccionDanioCriticoRecibidoPorcentaje;
        entry.buffs.resistenciaEstadosPorcentaje = item.resistenciaEstadosPorcentaje;
        entry.buffs.espinasDanioPlano = item.espinasDanioPlano;
        entry.buffs.espinasDanioPorcentaje = item.espinasDanioPorcentaje;
        entry.debuffsImpactoArma = new List<DebuffImpactoArmaData>();
        entry.efectoConsumible = new ConsumibleEfectoData();
        entry.habilidadAtaque = null;
        entry.habilidadExtra1 = null;
        entry.habilidadExtra2 = null;

        if (item is Arma arma)
        {
            entry.requisitoFue = arma.requisitoFue;
            entry.requisitoAgi = arma.requisitoAgi;
            entry.requisitoPoder = arma.requisitoPoder;
            CopyBuffsFromArma(entry.buffs, arma);
            entry.habilidadAtaque = arma.habilidadAtaque;
            entry.habilidadExtra1 = arma.habilidadExtra1;
            entry.habilidadExtra2 = arma.habilidadExtra2;
            entry.debuffsImpactoArma = CloneDebuffsImpactoArma(arma.debuffsImpactoArma);
        }
        else if (item is Armadura armadura)
        {
            entry.requisitoFue = armadura.requisitoFue;
            entry.requisitoAgi = armadura.requisitoAgi;
            entry.requisitoPoder = armadura.requisitoPoder;
            CopyBuffsFromArmadura(entry.buffs, armadura);
            entry.habilidadExtra1 = armadura.habilidadExtra1;
            entry.habilidadExtra2 = armadura.habilidadExtra2;
        }
        else if (item is Accesorio accesorio)
        {
            entry.requisitoFue = accesorio.requisitoFue;
            entry.requisitoAgi = accesorio.requisitoAgi;
            entry.requisitoPoder = accesorio.requisitoPoder;
            CopyBuffsFromAccesorio(entry.buffs, accesorio);
            entry.habilidadExtra1 = accesorio.habilidadExtra1;
            entry.habilidadExtra2 = accesorio.habilidadExtra2;
        }
        else if (item is Consumible consumible)
        {
            entry.efectoConsumible = CloneConsumibleEfectoData(consumible.ObtenerEfectoConsumibleNormalizado());
        }

        if (entry.tags == null)
        {
            entry.tags = new List<string>();
        }
    }

    private static bool ApplyEntryToItem(ItemDatabaseEntry entry, Item item)
    {
        bool changed = false;

        changed |= SetString(ref item.sNombreItem, entry.nombre);
        changed |= SetString(ref item.itemDescripcion, entry.descripcion);
        changed |= SetInt(ref item.iRareza, entry.rareza);
        changed |= SetObject(ref item.imItem, entry.icono);
        changed |= SetInt(ref item.iPrecio, entry.precio);
        changed |= SetInt(ref item.nivelMejora, entry.nivelMejora);
        changed |= SetInt(ref item.IDEfectoEspecial, entry.idEfectoEspecial);
        ItemBuffsData dataInicioCombate = entry.buffs ?? new ItemBuffsData();
        int reduccionDanio = Mathf.Clamp(dataInicioCombate.reduccionDanioRecibidoPorcentaje, 0, 95);
        int reduccionDanioCritico = Mathf.Clamp(dataInicioCombate.reduccionDanioCriticoRecibidoPorcentaje, 0, 95);
        int resistenciaEstados = Mathf.Clamp(dataInicioCombate.resistenciaEstadosPorcentaje, 0, 100);
        int espinasPlano = Mathf.Max(0, dataInicioCombate.espinasDanioPlano);
        int espinasPorcentaje = Mathf.Max(0, dataInicioCombate.espinasDanioPorcentaje);

        changed |= SetInt(ref item.barreraInicioCombate, dataInicioCombate.barreraInicioCombate);
        changed |= SetInt(ref item.evasionInicioCombate, dataInicioCombate.evasionInicioCombate);
        changed |= SetInt(ref item.bonusDanioFuegoInicioCombate, dataInicioCombate.bonusDanioFuegoInicioCombate);
        changed |= SetInt(ref item.bonusDanioHieloInicioCombate, dataInicioCombate.bonusDanioHieloInicioCombate);
        changed |= SetInt(ref item.bonusDanioRayoInicioCombate, dataInicioCombate.bonusDanioRayoInicioCombate);
        changed |= SetInt(ref item.bonusDanioAcidoInicioCombate, dataInicioCombate.bonusDanioAcidoInicioCombate);
        changed |= SetInt(ref item.bonusDanioArcanoInicioCombate, dataInicioCombate.bonusDanioArcanoInicioCombate);
        changed |= SetInt(ref item.bonusDanioNecroInicioCombate, dataInicioCombate.bonusDanioNecroInicioCombate);
        changed |= SetInt(ref item.bonusDanioDivinoInicioCombate, dataInicioCombate.bonusDanioDivinoInicioCombate);
        changed |= SetInt(ref item.regeneracionVidaInicioCombate, dataInicioCombate.regeneracionVidaInicioCombate);
        changed |= SetInt(ref item.regeneracionArmaduraInicioCombate, dataInicioCombate.regeneracionArmaduraInicioCombate);
        changed |= SetInt(ref item.reduccionDanioRecibidoPorcentaje, reduccionDanio);
        changed |= SetInt(ref item.reduccionDanioCriticoRecibidoPorcentaje, reduccionDanioCritico);
        changed |= SetInt(ref item.resistenciaEstadosPorcentaje, resistenciaEstados);
        changed |= SetInt(ref item.espinasDanioPlano, espinasPlano);
        changed |= SetInt(ref item.espinasDanioPorcentaje, espinasPorcentaje);

        List<int> classes = entry.clasesPermitidas != null ? entry.clasesPermitidas : new List<int>();
        changed |= SetIntList(ref item.IDClasesQuePuedenUsarEsteItem, classes);

        if (item is Arma arma)
        {
            changed |= SetInt(ref arma.requisitoFue, entry.requisitoFue);
            changed |= SetInt(ref arma.requisitoAgi, entry.requisitoAgi);
            changed |= SetInt(ref arma.requisitoPoder, entry.requisitoPoder);

            changed |= ApplyBuffsToArma(arma, entry.buffs);
            changed |= SetDebuffsImpactoArmaList(ref arma.debuffsImpactoArma, entry.debuffsImpactoArma);
            changed |= SetObject(ref arma.habilidadAtaque, entry.habilidadAtaque);
            changed |= SetObject(ref arma.habilidadExtra1, entry.habilidadExtra1);
            changed |= SetObject(ref arma.habilidadExtra2, entry.habilidadExtra2);
        }
        else if (item is Armadura armadura)
        {
            changed |= SetInt(ref armadura.requisitoFue, entry.requisitoFue);
            changed |= SetInt(ref armadura.requisitoAgi, entry.requisitoAgi);
            changed |= SetInt(ref armadura.requisitoPoder, entry.requisitoPoder);

            changed |= ApplyBuffsToArmadura(armadura, entry.buffs);
            changed |= SetObject(ref armadura.habilidadExtra1, entry.habilidadExtra1);
            changed |= SetObject(ref armadura.habilidadExtra2, entry.habilidadExtra2);
        }
        else if (item is Accesorio accesorio)
        {
            changed |= SetInt(ref accesorio.requisitoFue, entry.requisitoFue);
            changed |= SetInt(ref accesorio.requisitoAgi, entry.requisitoAgi);
            changed |= SetInt(ref accesorio.requisitoPoder, entry.requisitoPoder);

            changed |= ApplyBuffsToAccesorio(accesorio, entry.buffs);
            changed |= SetObject(ref accesorio.habilidadExtra1, entry.habilidadExtra1);
            changed |= SetObject(ref accesorio.habilidadExtra2, entry.habilidadExtra2);
        }
        else if (item is Consumible consumible)
        {
            changed |= SetConsumibleEfectoData(ref consumible.efectoConsumible, entry.efectoConsumible);
        }

        return changed;
    }

    private static int CompareEntries(ItemDatabaseEntry a, ItemDatabaseEntry b)
    {
        int byCategory = string.Compare(a.categoria, b.categoria, true);
        if (byCategory != 0)
        {
            return byCategory;
        }

        return string.Compare(a.nombre, b.nombre, true);
    }

    private static string ResolverCategoria(Item item)
    {
        if (item is Arma)
        {
            return "Arma";
        }

        if (item is Armadura)
        {
            return "Armadura";
        }

        if (item is Accesorio)
        {
            return "Accesorio";
        }

        if (item is Consumible)
        {
            return "Consumible";
        }

        return "Item";
    }

    private static string BuildListTitle(ItemDatabaseEntry entry, string fallbackPrefabName)
    {
        string categoria = string.IsNullOrWhiteSpace(entry.categoria) ? "Item" : entry.categoria.Trim();
        string nombre = string.IsNullOrWhiteSpace(entry.nombre) ? fallbackPrefabName : entry.nombre.Trim();
        return categoria + " - " + nombre;
    }

    private static string EnsureUniqueId(string baseId, string path, HashSet<string> usedIds)
    {
        string candidate = string.IsNullOrWhiteSpace(baseId) ? BuildDefaultId(path) : baseId.Trim();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            candidate = "ITEM";
        }

        if (!usedIds.Contains(candidate))
        {
            usedIds.Add(candidate);
            return candidate;
        }

        int suffix = 2;
        string withSuffix = candidate + "_" + suffix;
        while (usedIds.Contains(withSuffix))
        {
            suffix++;
            withSuffix = candidate + "_" + suffix;
        }

        usedIds.Add(withSuffix);
        return withSuffix;
    }

    private static string BuildDefaultId(string path)
    {
        string noExt = Path.ChangeExtension(path, null) ?? path;
        if (noExt.StartsWith("Assets/"))
        {
            noExt = noExt.Substring("Assets/".Length);
        }

        StringBuilder sb = new StringBuilder(noExt.Length + 8);
        sb.Append("ITEM_");

        bool previousWasUnderscore = false;
        for (int i = 0; i < noExt.Length; i++)
        {
            char c = noExt[i];
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
            {
                sb.Append(char.ToUpperInvariant(c));
                previousWasUnderscore = false;
            }
            else if (!previousWasUnderscore)
            {
                sb.Append('_');
                previousWasUnderscore = true;
            }
        }

        return sb.ToString().Trim('_');
    }

    private static string BuildDefaultNewPrefabName(string category)
    {
        string safeCategory = string.IsNullOrWhiteSpace(category) ? "Item" : category.Trim();
        return "nuevo" + safeCategory;
    }

    private static List<int> CopyIntList(List<int> source)
    {
        return source != null ? new List<int>(source) : new List<int>();
    }

    private static List<DebuffImpactoArmaData> CloneDebuffsImpactoArma(List<DebuffImpactoArmaData> source)
    {
        List<DebuffImpactoArmaData> clone = new List<DebuffImpactoArmaData>();
        if (source == null)
        {
            return clone;
        }

        for (int i = 0; i < source.Count; i++)
        {
            DebuffImpactoArmaData entry = source[i];
            if (entry == null)
            {
                continue;
            }

            clone.Add(CloneDebuffImpactoArma(entry));
        }

        return clone;
    }

    private static ConsumibleEfectoData CloneConsumibleEfectoData(ConsumibleEfectoData source)
    {
        return source != null ? source.Clone() : new ConsumibleEfectoData();
    }

    private static DebuffImpactoArmaData CloneDebuffImpactoArma(DebuffImpactoArmaData source)
    {
        DebuffImpactoArmaData clone = new DebuffImpactoArmaData();
        if (source == null)
        {
            return clone;
        }

        clone.activo = source.activo;
        clone.nombreDebuff = source.nombreDebuff;
        clone.probabilidadAplicar = source.probabilidadAplicar;
        clone.duracionRondas = source.duracionRondas;
        clone.requiereTiradaSalvacion = source.requiereTiradaSalvacion;
        clone.tipoTiradaSalvacion = source.tipoTiradaSalvacion;
        clone.dificultadSalvacion = source.dificultadSalvacion;
        clone.modFuerza = source.modFuerza;
        clone.modAgilidad = source.modAgilidad;
        clone.modPoder = source.modPoder;
        clone.modIniciativa = source.modIniciativa;
        clone.modAtaque = source.modAtaque;
        clone.modDefensa = source.modDefensa;
        clone.modArmadura = source.modArmadura;
        clone.modDanioPorcentaje = source.modDanioPorcentaje;
        clone.modTSReflejos = source.modTSReflejos;
        clone.modTSFortaleza = source.modTSFortaleza;
        clone.modTSMental = source.modTSMental;
        clone.modResFuego = source.modResFuego;
        clone.modResHielo = source.modResHielo;
        clone.modResRayo = source.modResRayo;
        clone.modResAcido = source.modResAcido;
        clone.modResArcano = source.modResArcano;
        clone.modResNecro = source.modResNecro;
        clone.modResDivino = source.modResDivino;
        clone.modCritDado = source.modCritDado;
        clone.modCritDanioPorcentaje = source.modCritDanioPorcentaje;
        clone.stacksSangrado = source.stacksSangrado;
        clone.stacksArdiendo = source.stacksArdiendo;
        clone.stacksCongelado = source.stacksCongelado;
        clone.stacksAcido = source.stacksAcido;
        clone.stacksAturdido = source.stacksAturdido;
        clone.reduccionAPPorTurno = source.reduccionAPPorTurno;
        clone.reduccionResistencias = source.reduccionResistencias;
        clone.stacksCondenado = source.stacksCondenado;
        clone.ignorarArmaduraPlano = source.ignorarArmaduraPlano;
        clone.roboVidaPorcentaje = source.roboVidaPorcentaje;
        clone.empujeCasillas = source.empujeCasillas;
        clone.jalonCasillas = source.jalonCasillas;
        return clone;
    }

    private static void CopyBuffsFromArma(ItemBuffsData buffs, Arma arma)
    {
        if (buffs == null || arma == null)
        {
            return;
        }

        buffs.buffFuerza = arma.buffFuerza;
        buffs.buffAgi = arma.buffAgi;
        buffs.buffPoder = arma.buffPoder;
        buffs.buffIniciativa = arma.buffIniciativa;
        buffs.buffApMax = arma.buffApMax;
        buffs.buffValMax = arma.buffValMax;
        buffs.buffhpMax = arma.buffhpMax;
        buffs.buffArmadura = arma.buffArmadura;
        buffs.buffDefensa = arma.buffDefensa;
        buffs.buffTSReflejo = arma.buffTSReflejo;
        buffs.buffTSFortaleza = arma.buffTSFortaleza;
        buffs.buffTSMental = arma.buffTSMental;
        buffs.buffResFuego = arma.buffResFuego;
        buffs.buffResRayo = arma.buffResRayo;
        buffs.buffResHielo = arma.buffResHielo;
        buffs.buffResArcano = arma.buffResArcano;
        buffs.buffResAcido = arma.buffResAcido;
        buffs.buffResNecro = arma.buffResNecro;
        buffs.buffResDivino = arma.buffResDivino;
        buffs.barreraInicioCombate = arma.barreraInicioCombate;
        buffs.evasionInicioCombate = arma.evasionInicioCombate;
        buffs.bonusDanioFuegoInicioCombate = arma.bonusDanioFuegoInicioCombate;
        buffs.bonusDanioHieloInicioCombate = arma.bonusDanioHieloInicioCombate;
        buffs.bonusDanioRayoInicioCombate = arma.bonusDanioRayoInicioCombate;
        buffs.bonusDanioAcidoInicioCombate = arma.bonusDanioAcidoInicioCombate;
        buffs.bonusDanioArcanoInicioCombate = arma.bonusDanioArcanoInicioCombate;
        buffs.bonusDanioNecroInicioCombate = arma.bonusDanioNecroInicioCombate;
        buffs.bonusDanioDivinoInicioCombate = arma.bonusDanioDivinoInicioCombate;
        buffs.regeneracionVidaInicioCombate = arma.regeneracionVidaInicioCombate;
        buffs.regeneracionArmaduraInicioCombate = arma.regeneracionArmaduraInicioCombate;
        buffs.reduccionDanioRecibidoPorcentaje = arma.reduccionDanioRecibidoPorcentaje;
        buffs.reduccionDanioCriticoRecibidoPorcentaje = arma.reduccionDanioCriticoRecibidoPorcentaje;
        buffs.resistenciaEstadosPorcentaje = arma.resistenciaEstadosPorcentaje;
        buffs.espinasDanioPlano = arma.espinasDanioPlano;
        buffs.espinasDanioPorcentaje = arma.espinasDanioPorcentaje;
    }

    private static void CopyBuffsFromArmadura(ItemBuffsData buffs, Armadura armadura)
    {
        if (buffs == null || armadura == null)
        {
            return;
        }

        buffs.buffFuerza = armadura.buffFuerza;
        buffs.buffAgi = armadura.buffAgi;
        buffs.buffPoder = armadura.buffPoder;
        buffs.buffIniciativa = armadura.buffIniciativa;
        buffs.buffApMax = armadura.buffApMax;
        buffs.buffValMax = armadura.buffValMax;
        buffs.buffhpMax = armadura.buffhpMax;
        buffs.buffArmadura = armadura.buffArmadura;
        buffs.buffDefensa = armadura.buffDefensa;
        buffs.buffTSReflejo = armadura.buffTSReflejo;
        buffs.buffTSFortaleza = armadura.buffTSFortaleza;
        buffs.buffTSMental = armadura.buffTSMental;
        buffs.buffResFuego = armadura.buffResFuego;
        buffs.buffResRayo = armadura.buffResRayo;
        buffs.buffResHielo = armadura.buffResHielo;
        buffs.buffResArcano = armadura.buffResArcano;
        buffs.buffResAcido = armadura.buffResAcido;
        buffs.buffResNecro = armadura.buffResNecro;
        buffs.buffResDivino = armadura.buffResDivino;
        buffs.barreraInicioCombate = armadura.barreraInicioCombate;
        buffs.evasionInicioCombate = armadura.evasionInicioCombate;
        buffs.bonusDanioFuegoInicioCombate = armadura.bonusDanioFuegoInicioCombate;
        buffs.bonusDanioHieloInicioCombate = armadura.bonusDanioHieloInicioCombate;
        buffs.bonusDanioRayoInicioCombate = armadura.bonusDanioRayoInicioCombate;
        buffs.bonusDanioAcidoInicioCombate = armadura.bonusDanioAcidoInicioCombate;
        buffs.bonusDanioArcanoInicioCombate = armadura.bonusDanioArcanoInicioCombate;
        buffs.bonusDanioNecroInicioCombate = armadura.bonusDanioNecroInicioCombate;
        buffs.bonusDanioDivinoInicioCombate = armadura.bonusDanioDivinoInicioCombate;
        buffs.regeneracionVidaInicioCombate = armadura.regeneracionVidaInicioCombate;
        buffs.regeneracionArmaduraInicioCombate = armadura.regeneracionArmaduraInicioCombate;
        buffs.reduccionDanioRecibidoPorcentaje = armadura.reduccionDanioRecibidoPorcentaje;
        buffs.reduccionDanioCriticoRecibidoPorcentaje = armadura.reduccionDanioCriticoRecibidoPorcentaje;
        buffs.resistenciaEstadosPorcentaje = armadura.resistenciaEstadosPorcentaje;
        buffs.espinasDanioPlano = armadura.espinasDanioPlano;
        buffs.espinasDanioPorcentaje = armadura.espinasDanioPorcentaje;
    }

    private static void CopyBuffsFromAccesorio(ItemBuffsData buffs, Accesorio accesorio)
    {
        if (buffs == null || accesorio == null)
        {
            return;
        }

        buffs.buffFuerza = accesorio.buffFuerza;
        buffs.buffAgi = accesorio.buffAgi;
        buffs.buffPoder = accesorio.buffPoder;
        buffs.buffIniciativa = accesorio.buffIniciativa;
        buffs.buffApMax = accesorio.buffApMax;
        buffs.buffValMax = accesorio.buffValMax;
        buffs.buffhpMax = accesorio.buffhpMax;
        buffs.buffArmadura = accesorio.buffArmadura;
        buffs.buffDefensa = accesorio.buffDefensa;
        buffs.buffTSReflejo = accesorio.buffTSReflejo;
        buffs.buffTSFortaleza = accesorio.buffTSFortaleza;
        buffs.buffTSMental = accesorio.buffTSMental;
        buffs.buffResFuego = accesorio.buffResFuego;
        buffs.buffResRayo = accesorio.buffResRayo;
        buffs.buffResHielo = accesorio.buffResHielo;
        buffs.buffResArcano = accesorio.buffResArcano;
        buffs.buffResAcido = accesorio.buffResAcido;
        buffs.buffResNecro = accesorio.buffResNecro;
        buffs.buffResDivino = accesorio.buffResDivino;
        buffs.barreraInicioCombate = accesorio.barreraInicioCombate;
        buffs.evasionInicioCombate = accesorio.evasionInicioCombate;
        buffs.bonusDanioFuegoInicioCombate = accesorio.bonusDanioFuegoInicioCombate;
        buffs.bonusDanioHieloInicioCombate = accesorio.bonusDanioHieloInicioCombate;
        buffs.bonusDanioRayoInicioCombate = accesorio.bonusDanioRayoInicioCombate;
        buffs.bonusDanioAcidoInicioCombate = accesorio.bonusDanioAcidoInicioCombate;
        buffs.bonusDanioArcanoInicioCombate = accesorio.bonusDanioArcanoInicioCombate;
        buffs.bonusDanioNecroInicioCombate = accesorio.bonusDanioNecroInicioCombate;
        buffs.bonusDanioDivinoInicioCombate = accesorio.bonusDanioDivinoInicioCombate;
        buffs.regeneracionVidaInicioCombate = accesorio.regeneracionVidaInicioCombate;
        buffs.regeneracionArmaduraInicioCombate = accesorio.regeneracionArmaduraInicioCombate;
        buffs.reduccionDanioRecibidoPorcentaje = accesorio.reduccionDanioRecibidoPorcentaje;
        buffs.reduccionDanioCriticoRecibidoPorcentaje = accesorio.reduccionDanioCriticoRecibidoPorcentaje;
        buffs.resistenciaEstadosPorcentaje = accesorio.resistenciaEstadosPorcentaje;
        buffs.espinasDanioPlano = accesorio.espinasDanioPlano;
        buffs.espinasDanioPorcentaje = accesorio.espinasDanioPorcentaje;
    }

    private static bool ApplyBuffsToArma(Arma arma, ItemBuffsData buffs)
    {
        if (arma == null)
        {
            return false;
        }

        ItemBuffsData data = buffs ?? new ItemBuffsData();
        bool changed = false;

        changed |= SetInt(ref arma.buffFuerza, data.buffFuerza);
        changed |= SetInt(ref arma.buffAgi, data.buffAgi);
        changed |= SetInt(ref arma.buffPoder, data.buffPoder);
        changed |= SetInt(ref arma.buffIniciativa, data.buffIniciativa);
        changed |= SetInt(ref arma.buffApMax, data.buffApMax);
        changed |= SetInt(ref arma.buffValMax, data.buffValMax);
        changed |= SetInt(ref arma.buffhpMax, data.buffhpMax);
        changed |= SetInt(ref arma.buffArmadura, data.buffArmadura);
        changed |= SetInt(ref arma.buffDefensa, data.buffDefensa);
        changed |= SetInt(ref arma.buffTSReflejo, data.buffTSReflejo);
        changed |= SetInt(ref arma.buffTSFortaleza, data.buffTSFortaleza);
        changed |= SetInt(ref arma.buffTSMental, data.buffTSMental);
        changed |= SetInt(ref arma.buffResFuego, data.buffResFuego);
        changed |= SetInt(ref arma.buffResRayo, data.buffResRayo);
        changed |= SetInt(ref arma.buffResHielo, data.buffResHielo);
        changed |= SetInt(ref arma.buffResArcano, data.buffResArcano);
        changed |= SetInt(ref arma.buffResAcido, data.buffResAcido);
        changed |= SetInt(ref arma.buffResNecro, data.buffResNecro);
        changed |= SetInt(ref arma.buffResDivino, data.buffResDivino);

        return changed;
    }

    private static bool ApplyBuffsToArmadura(Armadura armadura, ItemBuffsData buffs)
    {
        if (armadura == null)
        {
            return false;
        }

        ItemBuffsData data = buffs ?? new ItemBuffsData();
        bool changed = false;

        changed |= SetInt(ref armadura.buffFuerza, data.buffFuerza);
        changed |= SetInt(ref armadura.buffAgi, data.buffAgi);
        changed |= SetInt(ref armadura.buffPoder, data.buffPoder);
        changed |= SetInt(ref armadura.buffIniciativa, data.buffIniciativa);
        changed |= SetInt(ref armadura.buffApMax, data.buffApMax);
        changed |= SetInt(ref armadura.buffValMax, data.buffValMax);
        changed |= SetInt(ref armadura.buffhpMax, data.buffhpMax);
        changed |= SetInt(ref armadura.buffArmadura, data.buffArmadura);
        changed |= SetInt(ref armadura.buffDefensa, data.buffDefensa);
        changed |= SetInt(ref armadura.buffTSReflejo, data.buffTSReflejo);
        changed |= SetInt(ref armadura.buffTSFortaleza, data.buffTSFortaleza);
        changed |= SetInt(ref armadura.buffTSMental, data.buffTSMental);
        changed |= SetInt(ref armadura.buffResFuego, data.buffResFuego);
        changed |= SetInt(ref armadura.buffResRayo, data.buffResRayo);
        changed |= SetInt(ref armadura.buffResHielo, data.buffResHielo);
        changed |= SetInt(ref armadura.buffResArcano, data.buffResArcano);
        changed |= SetInt(ref armadura.buffResAcido, data.buffResAcido);
        changed |= SetInt(ref armadura.buffResNecro, data.buffResNecro);
        changed |= SetInt(ref armadura.buffResDivino, data.buffResDivino);

        return changed;
    }

    private static bool ApplyBuffsToAccesorio(Accesorio accesorio, ItemBuffsData buffs)
    {
        if (accesorio == null)
        {
            return false;
        }

        ItemBuffsData data = buffs ?? new ItemBuffsData();
        bool changed = false;

        changed |= SetInt(ref accesorio.buffFuerza, data.buffFuerza);
        changed |= SetInt(ref accesorio.buffAgi, data.buffAgi);
        changed |= SetInt(ref accesorio.buffPoder, data.buffPoder);
        changed |= SetInt(ref accesorio.buffIniciativa, data.buffIniciativa);
        changed |= SetInt(ref accesorio.buffApMax, data.buffApMax);
        changed |= SetInt(ref accesorio.buffValMax, data.buffValMax);
        changed |= SetInt(ref accesorio.buffhpMax, data.buffhpMax);
        changed |= SetInt(ref accesorio.buffArmadura, data.buffArmadura);
        changed |= SetInt(ref accesorio.buffDefensa, data.buffDefensa);
        changed |= SetInt(ref accesorio.buffTSReflejo, data.buffTSReflejo);
        changed |= SetInt(ref accesorio.buffTSFortaleza, data.buffTSFortaleza);
        changed |= SetInt(ref accesorio.buffTSMental, data.buffTSMental);
        changed |= SetInt(ref accesorio.buffResFuego, data.buffResFuego);
        changed |= SetInt(ref accesorio.buffResRayo, data.buffResRayo);
        changed |= SetInt(ref accesorio.buffResHielo, data.buffResHielo);
        changed |= SetInt(ref accesorio.buffResArcano, data.buffResArcano);
        changed |= SetInt(ref accesorio.buffResAcido, data.buffResAcido);
        changed |= SetInt(ref accesorio.buffResNecro, data.buffResNecro);
        changed |= SetInt(ref accesorio.buffResDivino, data.buffResDivino);

        return changed;
    }

    private static bool SetInt(ref int current, int next)
    {
        if (current == next)
        {
            return false;
        }

        current = next;
        return true;
    }

    private static bool SetString(ref string current, string next)
    {
        string safeNext = next ?? string.Empty;
        if (current == safeNext)
        {
            return false;
        }

        current = safeNext;
        return true;
    }

    private static bool SetObject<T>(ref T current, T next) where T : UnityEngine.Object
    {
        if (current == next)
        {
            return false;
        }

        current = next;
        return true;
    }

    private static bool SetIntList(ref List<int> current, List<int> next)
    {
        List<int> target = next ?? new List<int>();
        if (current == null)
        {
            current = new List<int>(target);
            return true;
        }

        if (current.Count == target.Count)
        {
            bool same = true;
            for (int i = 0; i < current.Count; i++)
            {
                if (current[i] != target[i])
                {
                    same = false;
                    break;
                }
            }

            if (same)
            {
                return false;
            }
        }

        current = new List<int>(target);
        return true;
    }

    private static bool SetDebuffsImpactoArmaList(ref List<DebuffImpactoArmaData> current, List<DebuffImpactoArmaData> next)
    {
        List<DebuffImpactoArmaData> target = CloneDebuffsImpactoArma(next);
        if (DebuffsImpactoArmaIguales(current, target))
        {
            return false;
        }

        current = target;
        return true;
    }

    private static bool SetConsumibleEfectoData(ref ConsumibleEfectoData current, ConsumibleEfectoData next)
    {
        ConsumibleEfectoData target = CloneConsumibleEfectoData(next);
        if (ConsumibleEfectoDataIgual(current, target))
        {
            return false;
        }

        current = target;
        return true;
    }

    private static bool ConsumibleEfectoDataIgual(ConsumibleEfectoData a, ConsumibleEfectoData b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a == null || b == null)
        {
            return false;
        }

        return a.curacionBase == b.curacionBase
            && a.curacionDadosCantidad == b.curacionDadosCantidad
            && a.curacionDadosCaras == b.curacionDadosCaras
            && a.curacionPorcentajeHPMax == b.curacionPorcentajeHPMax
            && a.removerDebuffs == b.removerDebuffs
            && a.removerBuffs == b.removerBuffs
            && a.removerEstadosNegativos == b.removerEstadosNegativos
            && a.modificarRegeneracionVida == b.modificarRegeneracionVida
            && a.modificarRegeneracionArmadura == b.modificarRegeneracionArmadura
            && a.modificarEvasion == b.modificarEvasion
            && a.aplicarBuff == b.aplicarBuff
            && a.nombreBuff == b.nombreBuff
            && a.buffEsBeneficio == b.buffEsBeneficio
            && a.duracionBuffRondas == b.duracionBuffRondas
            && a.buffReferencia == b.buffReferencia
            && ConsumibleBuffDataIgual(a.buff, b.buff);
    }

    private static bool ConsumibleBuffDataIgual(ConsumibleBuffData a, ConsumibleBuffData b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a == null || b == null)
        {
            return false;
        }

        return a.cantAtFue == b.cantAtFue
            && a.cantAtAgi == b.cantAtAgi
            && a.cantAtPod == b.cantAtPod
            && a.cantIniciativa == b.cantIniciativa
            && a.cantAPMax == b.cantAPMax
            && a.cantPMMax == b.cantPMMax
            && a.cantHPMax == b.cantHPMax
            && a.cantArmadura == b.cantArmadura
            && a.cantDefensa == b.cantDefensa
            && a.cantAtaque == b.cantAtaque
            && a.cantDanioPorcentaje == b.cantDanioPorcentaje
            && a.cantCritDado == b.cantCritDado
            && a.cantCritDanio == b.cantCritDanio
            && a.cantTsReflejos == b.cantTsReflejos
            && a.cantTsFortaleza == b.cantTsFortaleza
            && a.cantTsMental == b.cantTsMental
            && a.cantResFue == b.cantResFue
            && a.cantResHie == b.cantResHie
            && a.cantResRay == b.cantResRay
            && a.cantResAci == b.cantResAci
            && a.cantResArc == b.cantResArc
            && a.cantResNec == b.cantResNec
            && a.cantResDiv == b.cantResDiv
            && a.cantBarrera == b.cantBarrera
            && a.cantDamBonusElementalFue == b.cantDamBonusElementalFue
            && a.cantDamBonusElementalHie == b.cantDamBonusElementalHie
            && a.cantDamBonusElementalRay == b.cantDamBonusElementalRay
            && a.cantDamBonusElementalAci == b.cantDamBonusElementalAci
            && a.cantDamBonusElementalArc == b.cantDamBonusElementalArc
            && a.cantDamBonusElementalNec == b.cantDamBonusElementalNec
            && a.cantDamBonusElementalDiv == b.cantDamBonusElementalDiv
            && a.cantPenetracionArmadura == b.cantPenetracionArmadura
            && a.cantReduccionDanioRecibidoPorcentaje == b.cantReduccionDanioRecibidoPorcentaje
            && a.cantReduccionDanioCriticoRecibidoPorcentaje == b.cantReduccionDanioCriticoRecibidoPorcentaje
            && a.cantResistenciaEstadosPorcentaje == b.cantResistenciaEstadosPorcentaje
            && a.cantEspinasDanioPlano == b.cantEspinasDanioPlano
            && a.cantEspinasDanioPorcentaje == b.cantEspinasDanioPorcentaje;
    }

    private static bool DebuffsImpactoArmaIguales(List<DebuffImpactoArmaData> a, List<DebuffImpactoArmaData> b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a == null || b == null)
        {
            return false;
        }

        if (a.Count != b.Count)
        {
            return false;
        }

        for (int i = 0; i < a.Count; i++)
        {
            if (!DebuffImpactoArmaIgual(a[i], b[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool DebuffImpactoArmaIgual(DebuffImpactoArmaData a, DebuffImpactoArmaData b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a == null || b == null)
        {
            return false;
        }

        return a.activo == b.activo
            && a.nombreDebuff == b.nombreDebuff
            && a.probabilidadAplicar == b.probabilidadAplicar
            && a.duracionRondas == b.duracionRondas
            && a.requiereTiradaSalvacion == b.requiereTiradaSalvacion
            && a.tipoTiradaSalvacion == b.tipoTiradaSalvacion
            && a.dificultadSalvacion == b.dificultadSalvacion
            && a.modFuerza == b.modFuerza
            && a.modAgilidad == b.modAgilidad
            && a.modPoder == b.modPoder
            && a.modIniciativa == b.modIniciativa
            && a.modAtaque == b.modAtaque
            && a.modDefensa == b.modDefensa
            && a.modArmadura == b.modArmadura
            && a.modDanioPorcentaje == b.modDanioPorcentaje
            && a.modTSReflejos == b.modTSReflejos
            && a.modTSFortaleza == b.modTSFortaleza
            && a.modTSMental == b.modTSMental
            && a.modResFuego == b.modResFuego
            && a.modResHielo == b.modResHielo
            && a.modResRayo == b.modResRayo
            && a.modResAcido == b.modResAcido
            && a.modResArcano == b.modResArcano
            && a.modResNecro == b.modResNecro
            && a.modResDivino == b.modResDivino
            && a.modCritDado == b.modCritDado
            && a.modCritDanioPorcentaje == b.modCritDanioPorcentaje
            && a.stacksSangrado == b.stacksSangrado
            && a.stacksArdiendo == b.stacksArdiendo
            && a.stacksCongelado == b.stacksCongelado
            && a.stacksAcido == b.stacksAcido
            && a.stacksAturdido == b.stacksAturdido
            && a.reduccionAPPorTurno == b.reduccionAPPorTurno
            && a.reduccionResistencias == b.reduccionResistencias
            && a.stacksCondenado == b.stacksCondenado
            && a.ignorarArmaduraPlano == b.ignorarArmaduraPlano
            && a.roboVidaPorcentaje == b.roboVidaPorcentaje
            && a.empujeCasillas == b.empujeCasillas
            && a.jalonCasillas == b.jalonCasillas;
    }
}
