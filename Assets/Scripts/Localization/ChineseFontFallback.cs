using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>Fuente CJK dinámica compartida por los textos TMP, sin modificar sus fuentes latinas.</summary>
public static class ChineseFontFallback
{
    public const string FontResourcePath = "Fuentes/NotoSansCJK/NotoSansCJKsc-Regular";
    private const string RuntimeFontName = "Noto Sans CJK SC Runtime";
    private static TMP_FontAsset fontAsset;

    public static bool IsReady => TieneAtlasValido(fontAsset);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (ChineseLocalization.IsAvailable)
        {
            EnsureRegistered();
        }
    }

    public static bool EnsureRegistered()
    {
        // El atlas dinámico pudo destruirse en un cambio de escena (Resources.UnloadUnusedAssets)
        // o al salir de Play sin domain reload; en ese caso se regenera desde cero.
        if (!TieneAtlasValido(fontAsset))
        {
            fontAsset = null;
        }

        if (fontAsset != null)
        {
            MarcarPersistente(fontAsset);
            GarantizarEnFallback(fontAsset);
            return true;
        }

        if (TMP_Settings.instance == null)
        {
            Debug.LogError("[Localization] TMP Settings no está disponible.");
            return false;
        }

        // Reutiliza un respaldo previo si sigue vivo (Play sin domain reload).
        if (TMP_Settings.fallbackFontAssets != null)
        {
            foreach (TMP_FontAsset fallback in TMP_Settings.fallbackFontAssets)
            {
                if (fallback != null && fallback.name == RuntimeFontName && TieneAtlasValido(fallback))
                {
                    fontAsset = fallback;
                    break;
                }
            }
        }

        if (fontAsset == null)
        {
            Font source = Resources.Load<Font>(FontResourcePath);
            if (source == null)
            {
                Debug.LogError("[Localization] No se pudo cargar la fuente CJK.");
                return false;
            }

            fontAsset = TMP_FontAsset.CreateFontAsset(source, 64, 8, GlyphRenderMode.SDFAA,
                2048, 2048, AtlasPopulationMode.Dynamic, true);
            if (fontAsset == null)
            {
                Debug.LogError("[Localization] No se pudo crear el atlas CJK.");
                return false;
            }

            fontAsset.name = RuntimeFontName;
        }

        MarcarPersistente(fontAsset);
        GarantizarEnFallback(fontAsset);
        return true;
    }

    /// <summary>
    /// HideFlags.DontSave incluye DontUnloadUnusedAsset: impide que un cambio de escena
    /// destruya el atlas o el material mientras el TMP_FontAsset sigue referenciado,
    /// evitando "The variable m_AtlasTextures of TMP_FontAsset doesn't exist anymore".
    /// </summary>
    private static void MarcarPersistente(TMP_FontAsset asset)
    {
        if (asset == null) return;
        asset.hideFlags = HideFlags.DontSave;
        if (asset.material != null) asset.material.hideFlags = HideFlags.DontSave;
        if (asset.atlasTexture != null) asset.atlasTexture.hideFlags = HideFlags.DontSave;
        if (asset.atlasTextures != null)
        {
            foreach (Texture2D atlas in asset.atlasTextures)
            {
                if (atlas != null) atlas.hideFlags = HideFlags.DontSave;
            }
        }
    }

    private static bool TieneAtlasValido(TMP_FontAsset asset)
    {
        if (asset == null) return false;
        Texture2D[] atlases = asset.atlasTextures;
        return atlases != null && atlases.Length > 0 && atlases[0] != null;
    }

    private static void GarantizarEnFallback(TMP_FontAsset asset)
    {
        var fallbacks = new List<TMP_FontAsset>();
        bool presente = false;
        if (TMP_Settings.fallbackFontAssets != null)
        {
            foreach (TMP_FontAsset fallback in TMP_Settings.fallbackFontAssets)
            {
                if (fallback == null) continue;                              // referencia destruida
                if (fallback.name == RuntimeFontName && fallback != asset) continue; // atlas CJK viejo
                fallbacks.Add(fallback);
                if (fallback == asset) presente = true;
            }
        }

        if (!presente) fallbacks.Add(asset);
        TMP_Settings.fallbackFontAssets = fallbacks;
    }
}
