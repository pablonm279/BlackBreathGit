using System;
using UnityEngine;

// Fuente unica de verdad de la calidad grafica.
//
// Antes, cada script leia "graficos_index" y comparaba contra 0 / 1 / 2. Ahora hay 5 calidades
// (Low, Medium, High, VeryHigh, Ultra) y TODA la logica por calidad sale de la tabla de Presets de
// esta clase: que tecnologias de postproceso se usan, cuantas particulas, que luces proyectan sombra, etc.
//
// Los valores de QualitySettings (sombras, texturas, LOD, anisotropico...) viven en QualitySettings.asset
// (uno por nivel) y se aplican con ApplyEngine().
//
// Matriz de tecnologias por preset:
//
//                    Low      Medium   High     VeryHigh  Ultra
//   Camara HDR       no       si       si       si        si
//   Bloom            no       rapido   si       si        si (calidad plena)
//   Anti-aliasing    ninguno  FXAA     SMAA med SMAA alto SMAA alto
//   Oclusion ambient no       no       batalla  batalla+  batalla+campaña
//                                               campaña*
//   Sombras motor    no       si       si       si        si          (ver QualitySettings.asset)
//   Sombras luces    no       no       no       duras     suaves      (puntos grandes de campaña)
//   Particulas       reduc.   reduc.    ~60%     ~30%      100%        (ver ParticleSteps)
//   Brillo/contraste siempre (ColorGrading)
//
//   (*) VeryHigh: AO tambien en campaña con intensidad completa.
//
// Ademas el jugador puede sobreescribir por separado AA, sombras, texturas, oclusion ambiental, bloom,
// profundidad de campo y densidad de particulas. Si hay algun override el preset se muestra como "Personalizado".
public static class GraphicsQuality
{
  public const int Low = 0;
  public const int Medium = 1;
  public const int High = 2;
  public const int VeryHigh = 3;
  public const int Ultra = 4;
  public const int LevelCount = 5;

  public const string PrefIndex = "graficos_index";
  public const string PrefPostFx = "gfx_postfx_enabled";

  private const string PrefSchema = "graficos_schema";
  private const int SchemaVersion = 2; // 2 = cinco calidades (antes: Low / Medium / Ultra)

  private const string PrefOvAA = "gfx_ov_aa";
  private const string PrefOvShadows = "gfx_ov_shadows";
  private const string PrefOvTextures = "gfx_ov_textures";
  private const string PrefOvAO = "gfx_ov_ao";
  private const string PrefOvBloom = "gfx_ov_bloom";
  private const string PrefOvDoF = "gfx_ov_dof";
  private const string PrefOvParticles = "gfx_ov_particles";

  public enum AntiAliasing { Off = 0, FXAA = 1, SMAA = 2 }
  public enum ShadowOption { Off = 0, Low = 1, Medium = 2, High = 3 }
  public enum TextureOption { Low = 0, Medium = 1, High = 2 }
  public enum ParticleOption { Low = 0, Medium = 1, High = 2 }

  private struct Preset
  {
    public bool hdr;
    public bool bloom;
    public bool bloomFast;
    public AntiAliasing aa;
    public bool smaaHigh;
    public bool aoBattle;
    public bool aoCampaign;
    public float particleSteps;   // "niveles por debajo de Ultra" (en unidades del esquema original de 3 calidades)
    public int campaignLightShadows; // 0 sin sombra, 1 dura, 2 suave
    public float vfxLodHeight;    // altura minima en pantalla para el LOD de VFX generados
    public int vfxLightEvery;     // conserva 1 de cada N luces de llama (0 = ninguna)
    public bool menuParticles;
    public int dofKernel;         // 0 Small, 1 Medium (solo si el jugador activa DoF)
  }

  private static readonly Preset[] Presets =
  {
    // Low: lo minimo real. Solo ColorGrading (brillo/contraste) sobre camara LDR.
    new Preset { hdr = false, bloom = false, bloomFast = true,  aa = AntiAliasing.Off,  smaaHigh = false, aoBattle = false, aoCampaign = false, particleSteps = 2f,   campaignLightShadows = 0, vfxLodHeight = 0.018f, vfxLightEvery = 0, menuParticles = false, dofKernel = 0 },
    // Medium
    new Preset { hdr = true,  bloom = true,  bloomFast = true,  aa = AntiAliasing.FXAA, smaaHigh = false, aoBattle = false, aoCampaign = false, particleSteps = 1f,   campaignLightShadows = 0, vfxLodHeight = 0.010f, vfxLightEvery = 5, menuParticles = true,  dofKernel = 0 },
    // High
    new Preset { hdr = true,  bloom = true,  bloomFast = false, aa = AntiAliasing.SMAA, smaaHigh = false, aoBattle = true,  aoCampaign = false, particleSteps = 0.6f, campaignLightShadows = 0, vfxLodHeight = 0.008f, vfxLightEvery = 4, menuParticles = true,  dofKernel = 0 },
    // VeryHigh
    new Preset { hdr = true,  bloom = true,  bloomFast = false, aa = AntiAliasing.SMAA, smaaHigh = true,  aoBattle = true,  aoCampaign = true,  particleSteps = 0.3f, campaignLightShadows = 1, vfxLodHeight = 0.007f, vfxLightEvery = 3, menuParticles = true,  dofKernel = 0 },
    // Ultra
    new Preset { hdr = true,  bloom = true,  bloomFast = false, aa = AntiAliasing.SMAA, smaaHigh = true,  aoBattle = true,  aoCampaign = true,  particleSteps = 0f,   campaignLightShadows = 2, vfxLodHeight = 0.006f, vfxLightEvery = 3, menuParticles = true,  dofKernel = 1 },
  };

  // Resultado de combinar el preset con los overrides del jugador.
  public struct Settings
  {
    public int level;
    public bool customized;
    public bool postProcessing;
    public bool hdr;
    public bool bloom;
    public bool bloomFast;
    public AntiAliasing aa;
    public bool smaaHigh;
    public bool aoBattle;
    public bool aoCampaign;
    public bool dof;
    public int dofKernel;
    public float particleSteps;
    public int campaignLightShadows;
    public float vfxLodHeight;
    public int vfxLightEvery;
    public bool menuParticles;
    public bool IsLow { get { return level <= Low; } }
  }

  // Valores de QualitySettings que los overrides del jugador pueden pisar.
  private struct EngineSnapshot
  {
    public ShadowQuality shadows;
    public ShadowResolution shadowResolution;
    public float shadowDistance;
    public int shadowCascades;
    public int textureMipLimit;

    public static EngineSnapshot Capture()
    {
      return new EngineSnapshot
      {
        shadows = QualitySettings.shadows,
        shadowResolution = QualitySettings.shadowResolution,
        shadowDistance = QualitySettings.shadowDistance,
        shadowCascades = QualitySettings.shadowCascades,
        textureMipLimit = QualitySettings.globalTextureMipmapLimit
      };
    }

    public void Restore()
    {
      QualitySettings.shadows = shadows;
      QualitySettings.shadowResolution = shadowResolution;
      QualitySettings.shadowDistance = shadowDistance;
      QualitySettings.shadowCascades = shadowCascades;
      QualitySettings.globalTextureMipmapLimit = textureMipLimit;
    }
  }

  private static readonly System.Collections.Generic.Dictionary<int, EngineSnapshot> pristineByLevel =
    new System.Collections.Generic.Dictionary<int, EngineSnapshot>();

  private static bool migrated;
  private static string appliedSignature;
  private static bool cacheValid;
  private static Settings cache;

  public static event Action Changed;

  // ---------------------------------------------------------------- nivel

  public static int Level
  {
    get
    {
      EnsureMigrated();
      return Mathf.Clamp(PlayerPrefs.GetInt(PrefIndex, QualitySettings.GetQualityLevel()), 0, LevelCount - 1);
    }
  }

  public static bool IsLow { get { return Level <= Low; } }

  public static Settings Current
  {
    get
    {
      if (!cacheValid)
      {
        cache = Resolve();
        cacheValid = true;
      }
      return cache;
    }
  }

  // Interpola un valor entre Low / Medium / Ultra segun el nivel actual (High y VeryHigh quedan entre Medium y Ultra).
  // Reemplaza a los viejos "calidad == 0 ? a : calidad == 1 ? b : c".
  public static float Pick(float low, float medium, float ultra)
  {
    switch (Level)
    {
      case Low: return low;
      case Medium: return medium;
      case High: return Mathf.Lerp(medium, ultra, 0.5f);
      case VeryHigh: return Mathf.Lerp(medium, ultra, 0.8f);
      default: return ultra;
    }
  }

  // Escalon de 3 niveles (0 Low, 1 Medium, 2 Alta o superior) para codigo que decide por pasos y no por interpolacion.
  public static int LegacyTier
  {
    get
    {
      int level = Level;
      return level <= Low ? 0 : level == Medium ? 1 : 2;
    }
  }

  public static int PickInt(int low, int medium, int ultra)
  {
    return Mathf.RoundToInt(Pick(low, medium, ultra));
  }

  public static string LevelName(int level, int idioma)
  {
    level = Mathf.Clamp(level, 0, LevelCount - 1);
    if (idioma == TRADU.IdiomaIngles)
    {
      return new[] { "Low", "Medium", "High", "Very High", "Ultra" }[level];
    }
    if (idioma == TRADU.IdiomaPortugues)
    {
      return new[] { "Baixa", "Média", "Alta", "Muito alta", "Ultra" }[level];
    }
    if (idioma == TRADU.IdiomaChinoSimplificado)
    {
      return ChineseLocalization.Get(new[] { "Baja", "Media", "Alta", "Muy alta", "Ultra" }[level],
        new[] { "Low", "Medium", "High", "Very High", "Ultra" }[level]);
    }
    return new[] { "Baja", "Media", "Alta", "Muy alta", "Ultra" }[level];
  }

  // ---------------------------------------------------------------- escritura

  public static void SetLevel(int level, bool clearOverrides = true)
  {
    EnsureMigrated();
    level = Mathf.Clamp(level, 0, LevelCount - 1);
    PlayerPrefs.SetInt(PrefIndex, level);
    if (clearOverrides)
    {
      ClearOverridesInternal();
    }
    PlayerPrefs.Save();
    NotifyChanged();
  }

  public static void ClearOverrides()
  {
    ClearOverridesInternal();
    PlayerPrefs.Save();
    NotifyChanged();
  }

  public static bool HasOverrides
  {
    get
    {
      return PlayerPrefs.HasKey(PrefOvAA) || PlayerPrefs.HasKey(PrefOvShadows) || PlayerPrefs.HasKey(PrefOvTextures)
        || PlayerPrefs.HasKey(PrefOvAO) || PlayerPrefs.HasKey(PrefOvBloom) || PlayerPrefs.HasKey(PrefOvDoF)
        || PlayerPrefs.HasKey(PrefOvParticles);
    }
  }

  public static void SetAntiAliasing(AntiAliasing value) { SetOverride(PrefOvAA, (int)value); }
  public static void SetShadows(ShadowOption value) { SetOverride(PrefOvShadows, (int)value); }
  public static void SetTextures(TextureOption value) { SetOverride(PrefOvTextures, (int)value); }
  public static void SetAmbientOcclusion(bool value) { SetOverride(PrefOvAO, value ? 1 : 0); }
  public static void SetBloom(bool value) { SetOverride(PrefOvBloom, value ? 1 : 0); }
  public static void SetDepthOfField(bool value) { SetOverride(PrefOvDoF, value ? 1 : 0); }
  public static void SetParticles(ParticleOption value) { SetOverride(PrefOvParticles, (int)value); }

  public static bool PostProcessingEnabled
  {
    get { return PlayerPrefs.GetInt(PrefPostFx, 1) == 1; }
    set
    {
      PlayerPrefs.SetInt(PrefPostFx, value ? 1 : 0);
      PlayerPrefs.Save();
      NotifyChanged();
    }
  }

  // Valores que el jugador ve en la UI (override si existe, si no el del preset).
  public static AntiAliasing GetAntiAliasing() { return Current.aa; }
  public static ShadowOption GetShadows()
  {
    if (PlayerPrefs.HasKey(PrefOvShadows)) { return (ShadowOption)Mathf.Clamp(PlayerPrefs.GetInt(PrefOvShadows), 0, 3); }
    switch (Level)
    {
      case Low: return ShadowOption.Off;
      case Medium: return ShadowOption.Medium;
      case High: return ShadowOption.High;
      default: return ShadowOption.High;
    }
  }
  public static TextureOption GetTextures()
  {
    if (PlayerPrefs.HasKey(PrefOvTextures)) { return (TextureOption)Mathf.Clamp(PlayerPrefs.GetInt(PrefOvTextures), 0, 2); }
    switch (Level)
    {
      case Low: return TextureOption.Low;
      case Medium: return TextureOption.Medium;
      default: return TextureOption.High;
    }
  }
  public static ParticleOption GetParticles()
  {
    if (PlayerPrefs.HasKey(PrefOvParticles)) { return (ParticleOption)Mathf.Clamp(PlayerPrefs.GetInt(PrefOvParticles), 0, 2); }
    float steps = Presets[Level].particleSteps;
    return steps >= 1.5f ? ParticleOption.Low : steps >= 0.5f ? ParticleOption.Medium : ParticleOption.High;
  }

  // ---------------------------------------------------------------- motor

  // Aplica a QualitySettings el nivel guardado y los overrides de sombras / texturas.
  // Es idempotente y se llama al cargar cada escena (antes la calidad solo se aplicaba al abrir el menu de opciones).
  public static void ApplyEngine()
  {
    int level = Level;
    int shadowOv = PlayerPrefs.GetInt(PrefOvShadows, -1);
    int textureOv = PlayerPrefs.GetInt(PrefOvTextures, -1);
    string signature = level + "|" + shadowOv + "|" + textureOv;

    // SetQualityLevel recarga TODOS los valores del nivel desde QualitySettings.asset (tambien el vSync),
    // por eso solo se reaplica cuando cambia el nivel o un override, y quien lo llame debe reaplicar vSync / FPS despues.
    if (QualitySettings.GetQualityLevel() != level || signature != appliedSignature)
    {
      if (QualitySettings.GetQualityLevel() != level)
      {
        QualitySettings.SetQualityLevel(level, true);
      }

      appliedSignature = signature;

      // Los setters de QualitySettings escriben sobre el nivel ACTUAL y quedan guardados en ese nivel durante la sesion
      // (volver a elegirlo no recarga los valores del asset). Por eso se guardan los valores originales antes de pisarlos
      // y se restauran cuando el jugador vuelve a un preset limpio.
      EngineSnapshot original;
      if (pristineByLevel.TryGetValue(level, out original))
      {
        original.Restore();
      }

      if (shadowOv >= 0 || textureOv >= 0)
      {
        if (!pristineByLevel.ContainsKey(level))
        {
          pristineByLevel[level] = EngineSnapshot.Capture();
        }

        if (shadowOv >= 0)
        {
          ApplyShadowOverride((ShadowOption)Mathf.Clamp(shadowOv, 0, 3));
        }

        if (textureOv >= 0)
        {
          QualitySettings.globalTextureMipmapLimit = 2 - Mathf.Clamp(textureOv, 0, 2);
        }
      }
    }

    // Se evita apilar MSAA con el AA del postproceso.
    QualitySettings.antiAliasing = 0;
  }

  private static void ApplyShadowOverride(ShadowOption option)
  {
    switch (option)
    {
      case ShadowOption.Off:
        QualitySettings.shadows = ShadowQuality.Disable;
        break;
      case ShadowOption.Low:
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 40f;
        QualitySettings.shadowCascades = 1;
        break;
      case ShadowOption.Medium:
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.shadowDistance = 75f;
        QualitySettings.shadowCascades = 2;
        break;
      default:
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.shadowDistance = 125f;
        QualitySettings.shadowCascades = 4;
        break;
    }
  }

  // ---------------------------------------------------------------- internos

  private static Settings Resolve()
  {
    int level = Level;
    Preset p = Presets[level];
    Settings s = new Settings
    {
      level = level,
      customized = HasOverrides,
      postProcessing = PostProcessingEnabled,
      hdr = p.hdr,
      bloom = p.bloom,
      bloomFast = p.bloomFast,
      aa = p.aa,
      smaaHigh = p.smaaHigh,
      aoBattle = p.aoBattle,
      aoCampaign = p.aoCampaign,
      dof = false,
      dofKernel = p.dofKernel,
      particleSteps = p.particleSteps,
      campaignLightShadows = p.campaignLightShadows,
      vfxLodHeight = p.vfxLodHeight,
      vfxLightEvery = p.vfxLightEvery,
      menuParticles = p.menuParticles
    };

    if (PlayerPrefs.HasKey(PrefOvAA))
    {
      s.aa = (AntiAliasing)Mathf.Clamp(PlayerPrefs.GetInt(PrefOvAA), 0, 2);
      s.smaaHigh = level >= VeryHigh;
    }
    if (PlayerPrefs.HasKey(PrefOvAO))
    {
      bool ao = PlayerPrefs.GetInt(PrefOvAO) == 1;
      s.aoBattle = ao;
      s.aoCampaign = ao;
    }
    if (PlayerPrefs.HasKey(PrefOvBloom))
    {
      s.bloom = PlayerPrefs.GetInt(PrefOvBloom) == 1;
    }
    if (PlayerPrefs.HasKey(PrefOvDoF))
    {
      s.dof = PlayerPrefs.GetInt(PrefOvDoF) == 1;
    }
    if (PlayerPrefs.HasKey(PrefOvParticles))
    {
      ParticleOption option = (ParticleOption)Mathf.Clamp(PlayerPrefs.GetInt(PrefOvParticles), 0, 2);
      s.particleSteps = option == ParticleOption.Low ? 2f : option == ParticleOption.Medium ? 1f : 0f;
    }

    // Bloom o AO/DoF activados a mano necesitan camara HDR aunque el preset sea Low.
    if (s.bloom || s.dof)
    {
      s.hdr = true;
    }

    return s;
  }

  private static void SetOverride(string key, int value)
  {
    PlayerPrefs.SetInt(key, value);
    PlayerPrefs.Save();
    NotifyChanged();
  }

  private static void ClearOverridesInternal()
  {
    PlayerPrefs.DeleteKey(PrefOvAA);
    PlayerPrefs.DeleteKey(PrefOvShadows);
    PlayerPrefs.DeleteKey(PrefOvTextures);
    PlayerPrefs.DeleteKey(PrefOvAO);
    PlayerPrefs.DeleteKey(PrefOvBloom);
    PlayerPrefs.DeleteKey(PrefOvDoF);
    PlayerPrefs.DeleteKey(PrefOvParticles);
  }

  private static void NotifyChanged()
  {
    cacheValid = false;
    Action handler = Changed;
    if (handler != null) { handler(); }
  }

  // Las partidas/ajustes anteriores guardaban 0 = Low, 1 = Medium, 2 = Ultra. Con 5 calidades el 2 pasa a ser High,
  // asi que se reubica en Ultra una unica vez.
  private static void EnsureMigrated()
  {
    if (migrated) { return; }
    migrated = true;

    if (PlayerPrefs.GetInt(PrefSchema, 0) >= SchemaVersion) { return; }

    if (PlayerPrefs.HasKey(PrefIndex) && PlayerPrefs.GetInt(PrefIndex) >= 2)
    {
      PlayerPrefs.SetInt(PrefIndex, Ultra);
    }

    PlayerPrefs.SetInt(PrefSchema, SchemaVersion);
    PlayerPrefs.Save();
  }
}
