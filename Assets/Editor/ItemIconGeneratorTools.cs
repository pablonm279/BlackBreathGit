using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ItemIconGeneratorTools
{
    private const string MenuRoot = "Tools/Items/";
    private const string DefaultConfigPath = "Assets/Data/ItemIconGeneratorConfig.asset";
    private const string DefaultOutputFolder = "Assets/Generated/ItemIcons";
    private const string DatabaseAssetPath = "Assets/Data/ItemDatabase.asset";

    [MenuItem(MenuRoot + "Create Item Icon Generator Config")]
    public static void CreateConfig()
    {
        ItemIconGeneratorConfig existing = AssetDatabase.LoadAssetAtPath<ItemIconGeneratorConfig>(DefaultConfigPath);
        if (existing != null)
        {
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            EditorUtility.DisplayDialog("Item Icons", "La configuracion ya existe.", "OK");
            return;
        }

        EnsureFolder(Path.GetDirectoryName(DefaultConfigPath)?.Replace("\\", "/"));
        ItemIconGeneratorConfig config = ScriptableObject.CreateInstance<ItemIconGeneratorConfig>();
        config.carpetaSalida = DefaultOutputFolder;
        AssetDatabase.CreateAsset(config, DefaultConfigPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
        EditorUtility.DisplayDialog("Item Icons", "Config creada. Carga fondos y overlays en el Inspector.", "OK");
    }

    [MenuItem(MenuRoot + "Open Item Icon Generator Config")]
    public static void OpenConfig()
    {
        ItemIconGeneratorConfig config = AssetDatabase.LoadAssetAtPath<ItemIconGeneratorConfig>(DefaultConfigPath);
        if (config == null)
        {
            EditorUtility.DisplayDialog(
                "Item Icons",
                $"No existe {DefaultConfigPath}.\n\nPrimero ejecuta: Tools/Items/Create Item Icon Generator Config",
                "OK");
            return;
        }

        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }

    [MenuItem(MenuRoot + "Generate Auto Icons (From Config)")]
    public static void GenerateIconsFromConfigMissingOnly()
    {
        GenerateIconsFromConfig(onlyMissing: true);
    }

    [MenuItem(MenuRoot + "Generate Auto Icons (Rebuild All)")]
    public static void GenerateIconsFromConfigRebuildAll()
    {
        GenerateIconsFromConfig(onlyMissing: false);
    }

    private static void GenerateIconsFromConfig(bool onlyMissing)
    {
        ItemIconGeneratorConfig config = AssetDatabase.LoadAssetAtPath<ItemIconGeneratorConfig>(DefaultConfigPath);
        if (config == null)
        {
            EditorUtility.DisplayDialog(
                "Item Icons",
                $"No existe {DefaultConfigPath}.\n\nPrimero ejecuta: Tools/Items/Create Item Icon Generator Config",
                "OK");
            return;
        }

        string outputFolder = NormalizarCarpetaSalida(config.carpetaSalida);
        EnsureFolder(outputFolder);

        ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(DatabaseAssetPath);

        Dictionary<Sprite, SpritePixelsData> spriteCache = new Dictionary<Sprite, SpritePixelsData>();
        Dictionary<string, bool> readableStateCache = new Dictionary<string, bool>();

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int itemsProcesados = 0;
        int iconosGenerados = 0;
        int itemsSinOverlay = 0;
        int itemsSinFondo = 0;
        int itemsSaltadosConIcono = 0;
        int iconosExistentesReutilizados = 0;
        int cambiosPrefabs = 0;
        int cambiosDatabase = 0;

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                continue;
            }

            Item item = prefab.GetComponent<Item>();
            if (item == null)
            {
                continue;
            }

            itemsProcesados++;

            string fileName = $"{Path.GetFileNameWithoutExtension(prefabPath)}_{guid.Substring(0, 8)}.png";
            string outputPath = Path.Combine(outputFolder, fileName).Replace("\\", "/");

            if (onlyMissing && item.imItem != null)
            {
                itemsSaltadosConIcono++;
                continue;
            }

            Sprite iconoExistente = AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
            if (onlyMissing && iconoExistente != null)
            {
                iconosExistentesReutilizados++;

                if (item.imItem != iconoExistente)
                {
                    item.imItem = iconoExistente;
                    EditorUtility.SetDirty(item);
                    cambiosPrefabs++;
                }

                cambiosDatabase += ActualizarIconoEnDatabase(database, item, iconoExistente);
                continue;
            }

            Sprite fondo = ResolverFondo(config, item.iRareza);
            if (fondo == null)
            {
                itemsSinFondo++;
            }

            Sprite overlay = ResolverOverlay(config, item);
            if (overlay == null)
            {
                itemsSinOverlay++;
            }

            if (fondo == null && overlay == null)
            {
                continue;
            }

            Texture2D texturaCompuesta = ComponerIcono(
                fondo,
                overlay,
                config,
                spriteCache,
                readableStateCache);

            if (texturaCompuesta == null)
            {
                continue;
            }

            File.WriteAllBytes(outputPath, texturaCompuesta.EncodeToPNG());
            Object.DestroyImmediate(texturaCompuesta);

            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
            ConfigurarImportadorSprite(outputPath);
            Sprite iconoGenerado = AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
            if (iconoGenerado == null)
            {
                continue;
            }

            iconosGenerados++;

            if (item.imItem != iconoGenerado)
            {
                item.imItem = iconoGenerado;
                EditorUtility.SetDirty(item);
                cambiosPrefabs++;
            }

            cambiosDatabase += ActualizarIconoEnDatabase(database, item, iconoGenerado);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string modo = onlyMissing ? "Solo faltantes" : "Reconstruir todo";
        string msg =
            $"Auto-iconos completado ({modo}).\n\n" +
            $"Items procesados: {itemsProcesados}\n" +
            $"Iconos generados: {iconosGenerados}\n" +
            $"Items salteados con icono: {itemsSaltadosConIcono}\n" +
            $"Iconos existentes reutilizados: {iconosExistentesReutilizados}\n" +
            $"Prefabs modificados: {cambiosPrefabs}\n" +
            $"Entradas DB modificadas: {cambiosDatabase}\n" +
            $"Items sin overlay resuelto: {itemsSinOverlay}\n" +
            $"Items sin fondo resuelto: {itemsSinFondo}\n" +
            $"Salida: {outputFolder}";

        Debug.Log("[Items] " + msg.Replace("\n", " "));
        EditorUtility.DisplayDialog("Item Icons", msg, "OK");
    }

    private static int ActualizarIconoEnDatabase(ItemDatabase database, Item item, Sprite icono)
    {
        if (database == null || database.items == null || item == null || icono == null)
        {
            return 0;
        }

        for (int i = 0; i < database.items.Count; i++)
        {
            ItemDatabaseEntry entry = database.items[i];
            if (entry == null || entry.prefab != item)
            {
                continue;
            }

            if (entry.icono != icono)
            {
                entry.icono = icono;
                EditorUtility.SetDirty(database);
                return 1;
            }

            return 0;
        }

        return 0;
    }

    private static string NormalizarCarpetaSalida(string carpetaSalida)
    {
        string folder = string.IsNullOrWhiteSpace(carpetaSalida) ? DefaultOutputFolder : carpetaSalida.Trim();
        folder = folder.Replace("\\", "/");
        if (!folder.StartsWith("Assets/"))
        {
            folder = DefaultOutputFolder;
        }

        return folder;
    }

    private static Sprite ResolverFondo(ItemIconGeneratorConfig config, int rareza)
    {
        if (config == null)
        {
            return null;
        }

        switch (rareza)
        {
            case 0: return config.fondoComun != null ? config.fondoComun : config.fondoFallback;
            case 1: return config.fondoInfrecuente != null ? config.fondoInfrecuente : config.fondoFallback;
            case 2: return config.fondoRaro != null ? config.fondoRaro : config.fondoFallback;
            case 3: return config.fondoEpico != null ? config.fondoEpico : config.fondoFallback;
            case 4: return config.fondoLegendario != null ? config.fondoLegendario : config.fondoFallback;
            case 5: return config.fondoArtefacto != null ? config.fondoArtefacto : config.fondoFallback;
            default: return config.fondoFallback;
        }
    }

    private static Sprite ResolverOverlay(ItemIconGeneratorConfig config, Item item)
    {
        if (config == null || item == null)
        {
            return null;
        }

        int classId = ObtenerClasePrincipal(item);

        // Priorizar Armadura/Arma antes de Accesorio para evitar falsos positivos.
        if (item is Armadura)
        {
            return ResolverOverlayArmadura(config, item, classId);
        }

        if (item is Arma)
        {
            Sprite overlayArma = ResolverOverlayArmaPorClase(config, classId);
            if (overlayArma != null)
            {
                return overlayArma;
            }

            return ResolverOverlayArmaPorNombre(config, item.sNombreItem);
        }

        if (item is Accesorio)
        {
            return config.iconoAccesorioAnillo != null ? config.iconoAccesorioAnillo : config.iconoFallback;
        }

        if (item is Consumible)
        {
            return config.iconoConsumible != null ? config.iconoConsumible : config.iconoFallback;
        }

        return config.iconoFallback;
    }

    private static Sprite ResolverOverlayArmadura(ItemIconGeneratorConfig config, Item item, int classId)
    {
        Sprite overlayClase = ResolverOverlayArmaduraPorClase(config, classId, false);
        if (overlayClase != null)
        {
            return overlayClase;
        }

        int clasePorNombre = ResolverClasePorNombre(item != null ? item.sNombreItem : string.Empty, item != null ? item.name : string.Empty);
        overlayClase = ResolverOverlayArmaduraPorClase(config, clasePorNombre, false);
        if (overlayClase != null)
        {
            return overlayClase;
        }

        // Fallback seguro para armaduras: priorizar un icono de armadura antes del fallback global.
        return ResolverOverlayArmaduraPorClase(config, -1, true);
    }

    private static Sprite ResolverOverlayArmaduraPorClase(ItemIconGeneratorConfig config, int classId, bool permitirFallbackCadena)
    {
        switch (classId)
        {
            case 1: return config.iconoArmaduraCaballero;
            case 2: return config.iconoArmaduraExplorador;
            case 3: return config.iconoArmaduraPurificadora;
            case 4: return config.iconoArmaduraAcechador;
            case 5: return config.iconoArmaduraCanalizador;
            case 6: return config.iconoArmaduraDuelista;
            default:
                if (!permitirFallbackCadena)
                {
                    return null;
                }

                return config.iconoArmaduraCaballero
                    ?? config.iconoArmaduraAcechador
                    ?? config.iconoArmaduraExplorador
                    ?? config.iconoArmaduraPurificadora
                    ?? config.iconoArmaduraCanalizador
                    ?? config.iconoArmaduraDuelista
                    ?? config.iconoFallback;
        }
    }

    private static Sprite ResolverOverlayArmaPorClase(ItemIconGeneratorConfig config, int classId)
    {
        switch (classId)
        {
            case 1: return config.iconoArmaMandoble != null ? config.iconoArmaMandoble : config.iconoFallback;
            case 2: return config.iconoArmaArco != null ? config.iconoArmaArco : config.iconoFallback;
            case 3: return config.iconoArmaBaculo != null ? config.iconoArmaBaculo : config.iconoFallback;
            case 4: return config.iconoArmaEspadaCorta != null ? config.iconoArmaEspadaCorta : config.iconoFallback;
            case 5: return config.iconoArmaGuantelete != null ? config.iconoArmaGuantelete : config.iconoFallback;
            case 6: return config.iconoEstoque != null ? config.iconoEstoque : config.iconoFallback;
            default: return null;
        }
    }

    private static Sprite ResolverOverlayArmaPorNombre(ItemIconGeneratorConfig config, string nombre)
    {
        string n = string.IsNullOrWhiteSpace(nombre) ? string.Empty : nombre.ToLowerInvariant();
        if (n.Contains("mandoble")) { return config.iconoArmaMandoble != null ? config.iconoArmaMandoble : config.iconoFallback; }
        if (n.Contains("guantelete")) { return config.iconoArmaGuantelete != null ? config.iconoArmaGuantelete : config.iconoFallback; }
        if (n.Contains("bast")) { return config.iconoArmaBaculo != null ? config.iconoArmaBaculo : config.iconoFallback; }
        if (n.Contains("arco")) { return config.iconoArmaArco != null ? config.iconoArmaArco : config.iconoFallback; }
        if (n.Contains("estoque")) { return config.iconoEstoque != null ? config.iconoEstoque : config.iconoFallback; }
        if (n.Contains("espada")) { return config.iconoArmaEspadaCorta != null ? config.iconoArmaEspadaCorta : config.iconoFallback; }
        return config.iconoFallback;
    }

    private static int ResolverClasePorNombre(string nombreA, string nombreB)
    {
        string nA = string.IsNullOrWhiteSpace(nombreA) ? string.Empty : nombreA.ToLowerInvariant();
        string nB = string.IsNullOrWhiteSpace(nombreB) ? string.Empty : nombreB.ToLowerInvariant();

        if (ContieneAlguno(nA, nB, "coraza", "caballero", "baluarte", "muralla"))
        {
            return 1;
        }

        if (ContieneAlguno(nA, nB, "vestidura", "purificadora", "alba", "santuario"))
        {
            return 3;
        }

        if (ContieneAlguno(nA, nB, "acechador", "acecho", "velo", "reforzada"))
        {
            return 4;
        }

        if (ContieneAlguno(nA, nB, "explorador", "cuero", "rastreador", "horizonte", "cazador"))
        {
            return 2;
        }

        if (ContieneAlguno(nA, nB, "canalizador", "arcano", "umbral"))
        {
            return 5;
        }

        return -1;
    }

    private static bool ContieneAlguno(string a, string b, params string[] tokens)
    {
        if (tokens == null || tokens.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < tokens.Length; i++)
        {
            string t = tokens[i];
            if (string.IsNullOrWhiteSpace(t))
            {
                continue;
            }

            if ((!string.IsNullOrEmpty(a) && a.Contains(t)) || (!string.IsNullOrEmpty(b) && b.Contains(t)))
            {
                return true;
            }
        }

        return false;
    }

    private static int ObtenerClasePrincipal(Item item)
    {
        if (item == null || item.IDClasesQuePuedenUsarEsteItem == null)
        {
            return -1;
        }

        for (int i = 0; i < item.IDClasesQuePuedenUsarEsteItem.Count; i++)
        {
            int id = item.IDClasesQuePuedenUsarEsteItem[i];
            if (id >= 1 && id <= 6)
            {
                return id;
            }
        }

        return -1;
    }

    private static Texture2D ComponerIcono(
        Sprite fondo,
        Sprite overlay,
        ItemIconGeneratorConfig config,
        Dictionary<Sprite, SpritePixelsData> cache,
        Dictionary<string, bool> readableStateCache)
    {
        SpritePixelsData fondoData = ObtenerSpritePixels(fondo, cache, readableStateCache);
        SpritePixelsData overlayData = ObtenerSpritePixels(overlay, cache, readableStateCache);

        int width = fondoData.width > 0 ? fondoData.width : overlayData.width;
        int height = fondoData.height > 0 ? fondoData.height : overlayData.height;
        if (width <= 0 || height <= 0)
        {
            return null;
        }

        Color[] canvas = new Color[width * height];
        for (int i = 0; i < canvas.Length; i++)
        {
            canvas[i] = Color.clear;
        }

        if (fondoData.HasPixels)
        {
            DibujarSpriteEscalado(canvas, width, height, fondoData, 0, 0, width, height);
        }

        if (overlayData.HasPixels)
        {
            float escala = Mathf.Max(0.1f, config != null ? config.escalaOverlay : 0.72f);
            float overlayMax = Mathf.Min(width, height) * escala;
            float aspecto = overlayData.width <= 0 || overlayData.height <= 0
                ? 1f
                : (float)overlayData.width / overlayData.height;

            int drawW;
            int drawH;
            if (aspecto >= 1f)
            {
                drawW = Mathf.Max(1, Mathf.RoundToInt(overlayMax));
                drawH = Mathf.Max(1, Mathf.RoundToInt(overlayMax / aspecto));
            }
            else
            {
                drawH = Mathf.Max(1, Mathf.RoundToInt(overlayMax));
                drawW = Mathf.Max(1, Mathf.RoundToInt(overlayMax * aspecto));
            }

            float multX = config != null ? Mathf.Clamp(config.multiplicadorOverlayX, 0.5f, 2.0f) : 1f;
            drawW = Mathf.Max(1, Mathf.RoundToInt(drawW * multX));

            float offsetXNorm = config != null ? config.offsetOverlayXNormalizado : 0f;
            float offsetYNorm = config != null ? config.offsetOverlayYNormalizado : 0f;
            int centerX = width / 2 + Mathf.RoundToInt(offsetXNorm * width * 0.5f);
            int centerY = height / 2 + Mathf.RoundToInt(offsetYNorm * height * 0.5f);
            int x = centerX - drawW / 2;
            int y = centerY - drawH / 2;

            DibujarSpriteEscalado(canvas, width, height, overlayData, x, y, drawW, drawH);
        }

        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.SetPixels(canvas);
        result.Apply();
        return result;
    }

    private static void DibujarSpriteEscalado(
        Color[] canvas,
        int canvasW,
        int canvasH,
        SpritePixelsData sprite,
        int dstX,
        int dstY,
        int dstW,
        int dstH)
    {
        if (!sprite.HasPixels || canvas == null || dstW <= 0 || dstH <= 0)
        {
            return;
        }

        for (int y = 0; y < dstH; y++)
        {
            int py = dstY + y;
            if (py < 0 || py >= canvasH)
            {
                continue;
            }

            float v = (y + 0.5f) / dstH;
            for (int x = 0; x < dstW; x++)
            {
                int px = dstX + x;
                if (px < 0 || px >= canvasW)
                {
                    continue;
                }

                float u = (x + 0.5f) / dstW;
                Color src = MuestrearBilineal(sprite, u, v);
                if (src.a <= 0.001f)
                {
                    continue;
                }

                int idx = py * canvasW + px;
                canvas[idx] = AlphaBlend(canvas[idx], src);
            }
        }
    }

    private static Color MuestrearBilineal(SpritePixelsData sprite, float u, float v)
    {
        if (!sprite.HasPixels)
        {
            return Color.clear;
        }

        u = Mathf.Clamp01(u);
        v = Mathf.Clamp01(v);

        float sx = u * (sprite.width - 1);
        float sy = v * (sprite.height - 1);

        int x0 = Mathf.FloorToInt(sx);
        int y0 = Mathf.FloorToInt(sy);
        int x1 = Mathf.Min(x0 + 1, sprite.width - 1);
        int y1 = Mathf.Min(y0 + 1, sprite.height - 1);

        float tx = sx - x0;
        float ty = sy - y0;

        Color c00 = sprite.pixels[y0 * sprite.width + x0];
        Color c10 = sprite.pixels[y0 * sprite.width + x1];
        Color c01 = sprite.pixels[y1 * sprite.width + x0];
        Color c11 = sprite.pixels[y1 * sprite.width + x1];

        Color cx0 = Color.Lerp(c00, c10, tx);
        Color cx1 = Color.Lerp(c01, c11, tx);
        return Color.Lerp(cx0, cx1, ty);
    }

    private static Color AlphaBlend(Color dst, Color src)
    {
        float outA = src.a + dst.a * (1f - src.a);
        if (outA <= 0.0001f)
        {
            return Color.clear;
        }

        Vector3 outRgb =
            (new Vector3(src.r, src.g, src.b) * src.a + new Vector3(dst.r, dst.g, dst.b) * dst.a * (1f - src.a))
            / outA;

        return new Color(outRgb.x, outRgb.y, outRgb.z, outA);
    }

    private static SpritePixelsData ObtenerSpritePixels(
        Sprite sprite,
        Dictionary<Sprite, SpritePixelsData> cache,
        Dictionary<string, bool> readableStateCache)
    {
        if (sprite == null)
        {
            return default;
        }

        if (cache != null && cache.TryGetValue(sprite, out SpritePixelsData data))
        {
            return data;
        }

        Texture2D tex = sprite.texture;
        if (tex == null)
        {
            return default;
        }

        string texPath = AssetDatabase.GetAssetPath(tex);
        if (string.IsNullOrWhiteSpace(texPath))
        {
            return default;
        }

        TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        bool changedReadable = false;
        bool prevReadable = false;

        if (importer != null)
        {
            if (!readableStateCache.TryGetValue(texPath, out prevReadable))
            {
                prevReadable = importer.isReadable;
                readableStateCache[texPath] = prevReadable;
            }

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
                changedReadable = true;
            }
        }

        Rect r = sprite.rect;
        int x = Mathf.FloorToInt(r.x);
        int y = Mathf.FloorToInt(r.y);
        int w = Mathf.FloorToInt(r.width);
        int h = Mathf.FloorToInt(r.height);

        Color[] pixels;
        try
        {
            pixels = tex.GetPixels(x, y, w, h);
        }
        catch
        {
            pixels = null;
        }

        if (changedReadable && importer != null)
        {
            importer.isReadable = prevReadable;
            importer.SaveAndReimport();
        }

        data = new SpritePixelsData
        {
            width = w,
            height = h,
            pixels = pixels
        };

        cache?.Add(sprite, data);
        return data;
    }

    private static void ConfigurarImportadorSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (importer.alphaIsTransparency != true)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        folderPath = folderPath.Replace("\\", "/");
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

    private struct SpritePixelsData
    {
        public int width;
        public int height;
        public Color[] pixels;

        public bool HasPixels => pixels != null && pixels.Length > 0 && width > 0 && height > 0;
    }
}
