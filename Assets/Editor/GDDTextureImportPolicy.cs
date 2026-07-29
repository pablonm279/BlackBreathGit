using System;
using UnityEditor;

public sealed class GDDTextureImportPolicy : AssetPostprocessor
{
    private static readonly string[] PlatformNames =
    {
        "Standalone",
        "Android",
        "iPhone",
        "WebGL",
        "Windows Store Apps",
        "Server"
    };

    private const string ItemIconsPath = "Assets/Generated/ItemIcons/";
    private const string LegacyItemIconsPath = "Assets/Generated/Iconositems/";
    private const string AbilityIconsPath = "Assets/Resources/imHab/";
    private const string CombatIconsPath = "Assets/Resources/Imagenes/RecursosSprites/IconosTextoCombate/";
    private const string StatusIconsPath = "Assets/Resources/Imagenes/";
    private const string NpcSpritesPath = "Assets/Prefabs/Prefabs NPC/";
    private const string ClassSpritesPath = "Assets/Scripts/Clases/";

    private void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;
        if (importer == null)
        {
            return;
        }

        int maxTextureSize = ObtenerMaxTextureSize(assetPath, importer);
        if (maxTextureSize <= 0)
        {
            return;
        }

        importer.maxTextureSize = maxTextureSize;
        importer.isReadable = false;
        AplicarLimiteAOverrides(importer, maxTextureSize);
    }

    public override uint GetVersion()
    {
        return 1;
    }

    [MenuItem("Tools/Optimization/Aplicar política de texturas")]
    public static void AplicarPoliticaTexturasExistentes()
    {
        string[] carpetas =
        {
            "Assets/Generated",
            "Assets/Resources/imHab",
            "Assets/Resources/Imagenes",
            "Assets/Prefabs/Prefabs NPC",
            "Assets/Scripts/Clases"
        };

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", carpetas);
        int cantidad = guids.Length;

        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < cantidad; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ObtenerMaxTextureSize(path, importer) <= 0)
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(
                    "Optimizando texturas",
                    path,
                    cantidad > 0 ? (float)i / cantidad : 1f);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }
    }

    private static int ObtenerMaxTextureSize(string path, TextureImporter importer)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }

        string normalizedPath = path.Replace('\\', '/');
        if (normalizedPath.StartsWith(ItemIconsPath, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(LegacyItemIconsPath, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(AbilityIconsPath, StringComparison.OrdinalIgnoreCase))
        {
            return 512;
        }

        if (normalizedPath.StartsWith(CombatIconsPath, StringComparison.OrdinalIgnoreCase)
            || EsIconoEstado(normalizedPath))
        {
            return 256;
        }

        if (importer != null
            && importer.textureType == TextureImporterType.Sprite
            && (normalizedPath.StartsWith(NpcSpritesPath, StringComparison.OrdinalIgnoreCase)
                || normalizedPath.StartsWith(ClassSpritesPath, StringComparison.OrdinalIgnoreCase)))
        {
            return 1024;
        }

        return 0;
    }

    private static bool EsIconoEstado(string path)
    {
        if (!path.StartsWith(StatusIconsPath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        int separatorIndex = path.LastIndexOf('/');
        string fileName = separatorIndex >= 0 ? path.Substring(separatorIndex + 1) : path;
        return fileName.StartsWith("est_", StringComparison.OrdinalIgnoreCase);
    }

    private static void AplicarLimiteAOverrides(TextureImporter importer, int maxTextureSize)
    {
        for (int i = 0; i < PlatformNames.Length; i++)
        {
            TextureImporterPlatformSettings settings =
                importer.GetPlatformTextureSettings(PlatformNames[i]);
            if (!settings.overridden || settings.maxTextureSize <= maxTextureSize)
            {
                continue;
            }

            settings.maxTextureSize = maxTextureSize;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
