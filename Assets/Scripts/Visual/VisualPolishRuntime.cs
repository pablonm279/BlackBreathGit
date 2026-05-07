using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

public sealed class VisualPolishRuntime : MonoBehaviour
{
  private static VisualPolishRuntime instance;

  private const string PrefPostFx = "gfx_postfx_enabled";
  private const string PrefAA = "gfx_aa_enabled";
  private const string PrefBloom = "gfx_bloom_enabled";
  private const string PrefDoF = "gfx_dof_enabled";
  private const string PrefVsync = "gfx_vsync";
  private const string PrefFpsLimit = "gfx_fps_limit";
  private const string PrefBrightness = "gfx_brightness";
  private const string PrefContrast = "gfx_contrast";

  [SerializeField] private bool rebalanceQualityAtRuntime = true;
  [SerializeField] private int postProcessLayerIndex = 12; // Bit 4096 in current scenes.
  [SerializeField] private bool ensureSceneGlobalPostFxVolume = true;
  [Header("UI Responsive")]
  [SerializeField] private bool normalizeScreenSpaceCanvases = true;
  [SerializeField] private Vector2 uiReferenceResolution = new Vector2(1920f, 1080f);
  [SerializeField] [Range(0f, 1f)] private float uiMatchWidthOrHeight = 0.5f;

  [Header("Defaults (if prefs are missing)")]
  [SerializeField] private bool defaultPostFxEnabled = true;
  [SerializeField] private bool defaultAAEnabled = true;
  [SerializeField] private bool defaultBloomEnabled = true;
  [SerializeField] private bool defaultDoFEnabled = false;
  [SerializeField] private bool defaultVsyncEnabled = true;
  [SerializeField] private int defaultFpsLimit = 60;
  [SerializeField] [Range(0f, 1f)] private float defaultBrightness = 0.5f;
  [SerializeField] [Range(0.25f, 2.5f)] private float brightnessExposureRange = 1.32f;
  [SerializeField] [Range(0.5f, 1.5f)] private float defaultContrast = 1f;
  [SerializeField] [Range(20f, 120f)] private float contrastOffsetRange = 85f;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Boot()
  {
    if (instance != null) { return; }

    GameObject go = new GameObject("VisualPolishRuntime");
    instance = go.AddComponent<VisualPolishRuntime>();
    DontDestroyOnLoad(go);
  }

  public static void ApplyPostProcessingPrefsNow()
  {
#if UNITY_POST_PROCESSING_STACK_V2
    if (instance == null) { return; }
    Scene scene = SceneManager.GetActiveScene();
    instance.ApplyCameraPostFxAA(scene);
    if (instance.ensureSceneGlobalPostFxVolume)
    {
      instance.EnsureSceneGlobalVolume(scene);
    }
#endif
  }

  private void Awake()
  {
    if (instance != null && instance != this)
    {
      Destroy(gameObject);
      return;
    }

    instance = this;
    DontDestroyOnLoad(gameObject);
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDestroy()
  {
    if (instance == this)
    {
      SceneManager.sceneLoaded -= OnSceneLoaded;
      instance = null;
    }
  }

  private void Start()
  {
    ApplyScenePolish(SceneManager.GetActiveScene());
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode _)
  {
    ApplyScenePolish(scene);
  }

  private void ApplyScenePolish(Scene scene)
  {
    ApplySyncAndFrameRatePrefs();

    if (rebalanceQualityAtRuntime)
    {
      ApplyRuntimeQualityRebalance();
    }

    NormalizeSceneCanvasScalers(scene);

#if UNITY_POST_PROCESSING_STACK_V2
    ApplyCameraPostFxAA(scene);
    if (ensureSceneGlobalPostFxVolume)
    {
      EnsureSceneGlobalVolume(scene);
    }
#endif
  }

  private static void ApplyRuntimeQualityRebalance()
  {
    // Avoid MSAA + post-process AA stacking. PostProcessLayer handles anti-aliasing.
    QualitySettings.antiAliasing = 0;

    int level = QualitySettings.GetQualityLevel();
    if (level <= 0) // Low
    {
      QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, 30f);
    }
    else if (level == 1) // Medium/High
    {
      QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, 90f);
    }
    else // Ultra+
    {
      QualitySettings.shadowDistance = Mathf.Min(QualitySettings.shadowDistance, 140f);
    }
  }

  private void NormalizeSceneCanvasScalers(Scene scene)
  {
    if (!normalizeScreenSpaceCanvases) { return; }

    Canvas[] canvases = FindObjectsOfType<Canvas>(true);
    float clampedMatch = Mathf.Clamp01(uiMatchWidthOrHeight);

    for (int i = 0; i < canvases.Length; i++)
    {
      Canvas canvas = canvases[i];
      if (canvas == null) { continue; }
      if (canvas.gameObject.scene != scene) { continue; }
      if (!canvas.isRootCanvas) { continue; }
      if (canvas.renderMode == RenderMode.WorldSpace) { continue; }

      CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
      if (scaler == null)
      {
        scaler = canvas.gameObject.AddComponent<CanvasScaler>();
      }

      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = uiReferenceResolution;
      scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
      scaler.matchWidthOrHeight = clampedMatch;
    }
  }

#if UNITY_POST_PROCESSING_STACK_V2
  private void ApplySyncAndFrameRatePrefs()
  {
    bool vsyncEnabled = PrefBool(PrefVsync, defaultVsyncEnabled);
    QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;

    int fpsLimit = PrefInt(PrefFpsLimit, defaultFpsLimit);
    if (vsyncEnabled)
    {
      Application.targetFrameRate = -1;
      return;
    }

    if (fpsLimit <= 0)
    {
      Application.targetFrameRate = -1;
    }
    else
    {
      Application.targetFrameRate = Mathf.Clamp(fpsLimit, 30, 240);
    }
  }

  private void ApplyCameraPostFxAA(Scene scene)
  {
    Camera[] cameras = FindObjectsOfType<Camera>(true);
    int volumeMask = 1 << postProcessLayerIndex;
    bool postFxEnabled = PrefBool(PrefPostFx, defaultPostFxEnabled);
    bool aaEnabled = PrefBool(PrefAA, defaultAAEnabled);

    for (int i = 0; i < cameras.Length; i++)
    {
      Camera cam = cameras[i];
      if (cam == null) { continue; }
      if (cam.gameObject.scene != scene) { continue; }

      PostProcessLayer layer = cam.GetComponent<PostProcessLayer>();
      if (layer == null) { continue; }

      cam.allowMSAA = false;
      layer.enabled = postFxEnabled;
      if (!postFxEnabled) { continue; }

      layer.stopNaNPropagation = true;
      layer.volumeLayer = volumeMask;

      if (aaEnabled)
      {
        // SMAA avoids temporal ghosting/motion blur feel when characters move.
        layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
        layer.subpixelMorphologicalAntialiasing.quality = SubpixelMorphologicalAntialiasing.Quality.High;
      }
      else
      {
        layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
      }
    }
  }

  private void EnsureSceneGlobalVolume(Scene scene)
  {
    string sceneName = scene.name.ToLowerInvariant();
    bool isMenu = sceneName.Contains("menu");
    bool isCampaign = sceneName.Contains("camp");
    bool isBattle = sceneName.Contains("batalla");
    if (!isMenu && !isCampaign && !isBattle) { return; }

    bool postFxEnabled = PrefBool(PrefPostFx, defaultPostFxEnabled);
    bool bloomEnabled = PrefBool(PrefBloom, defaultBloomEnabled);
    bool dofEnabled = PrefBool(PrefDoF, defaultDoFEnabled);
    float brightness = PrefFloat(PrefBrightness, defaultBrightness);
    float sceneBrightnessExposureRange = brightnessExposureRange;
    if (isMenu || isCampaign)
    {
      // Give options brightness a bit more headroom in front-end scenes without affecting battles.
      sceneBrightnessExposureRange += 0.24f;
    }
    float brightnessExposureOffset = (Mathf.Clamp01(brightness) - 0.5f) * 2f * sceneBrightnessExposureRange;
    float contrast = Mathf.Clamp(PrefFloat(PrefContrast, defaultContrast), 0.5f, 1.5f);
    float contrastDelta = contrast - 1f;
    float contrastOffset = -Mathf.Sign(contrastDelta) * Mathf.Pow(Mathf.Abs(contrastDelta) * 2f, 1.2f) * (contrastOffsetRange * 0.5f);

    PostProcessVolume[] volumes = FindObjectsOfType<PostProcessVolume>(true);
    PostProcessVolume runtimeVolume = null;
    for (int i = 0; i < volumes.Length; i++)
    {
      PostProcessVolume v = volumes[i];
      if (v == null) { continue; }
      if (v.gameObject.scene != scene) { continue; }
      if (v.gameObject.name == "PostFX Runtime Global")
      {
        runtimeVolume = v;
      }
    }

    Camera[] cameras = FindObjectsOfType<Camera>(true);
    bool sceneUsesPostFx = false;
    for (int i = 0; i < cameras.Length; i++)
    {
      Camera cam = cameras[i];
      if (cam == null) { continue; }
      if (cam.gameObject.scene != scene) { continue; }
      if (cam.GetComponent<PostProcessLayer>() != null)
      {
        sceneUsesPostFx = true;
        break;
      }
    }

    if (!sceneUsesPostFx) { return; }

    if (runtimeVolume == null)
    {
      GameObject go = new GameObject("PostFX Runtime Global");
      go.layer = postProcessLayerIndex;
      SceneManager.MoveGameObjectToScene(go, scene);
      runtimeVolume = go.AddComponent<PostProcessVolume>();
    }

    runtimeVolume.gameObject.layer = postProcessLayerIndex;
    runtimeVolume.isGlobal = true;
    runtimeVolume.weight = postFxEnabled ? 1f : 0f;
    runtimeVolume.priority = 100f;

    PostProcessProfile profile = runtimeVolume.sharedProfile;
    if (profile == null)
    {
      profile = ScriptableObject.CreateInstance<PostProcessProfile>();
      profile.name = "PostFX Runtime Profile";
      runtimeVolume.sharedProfile = profile;
    }

    BuildSceneProfile(profile, isMenu, isCampaign, isBattle, bloomEnabled, dofEnabled, brightnessExposureOffset, contrastOffset);
  }

  private static void BuildSceneProfile(PostProcessProfile profile, bool isMenu, bool isCampaign, bool isBattle, bool bloomEnabled, bool dofEnabled, float brightnessExposureOffset, float contrastOffset)
  {
    ColorGrading color = GetOrAddSetting<ColorGrading>(profile);
    color.enabled.Override(true);
    color.gradingMode.Override(GradingMode.HighDefinitionRange);

    float baseExposure = 0f;
    float baseContrast = 0f;
    if (isMenu)
    {
      // Keep menu visuals neutral to avoid a darker main menu.
      color.temperature.Override(0f);
      color.saturation.Override(0f);
      baseExposure = 0.18f;
    }
    else if (isCampaign)
    {
      color.temperature.Override(-9f);
      color.saturation.Override(-5f);
      baseExposure = 0.24f;
      baseContrast = 14f;
    }
    else if (isBattle)
    {
      color.temperature.Override(-6f);
      color.saturation.Override(-2f);
      baseExposure = 0.06f;
      baseContrast = 10f;
    }
    color.postExposure.Override(baseExposure + brightnessExposureOffset);
    color.contrast.Override(Mathf.Clamp(baseContrast + contrastOffset, -100f, 100f));

    Bloom bloom = GetOrAddSetting<Bloom>(profile);
    bloom.enabled.Override(bloomEnabled);
    bloom.intensity.Override(isBattle ? 0.14f : 0.22f);
    bloom.threshold.Override(isBattle ? 1.22f : 1.15f);
    bloom.softKnee.Override(0.25f);

    Vignette vignette = GetOrAddSetting<Vignette>(profile);
    vignette.enabled.Override(!isMenu);
    if (!isMenu)
    {
      vignette.intensity.Override(isBattle ? 0.20f : 0.24f);
      vignette.smoothness.Override(isBattle ? 0.28f : 0.32f);
    }

    AmbientOcclusion ao = GetOrAddSetting<AmbientOcclusion>(profile);
    ao.enabled.Override(!isMenu);
    if (!isMenu)
    {
      ao.intensity.Override(isBattle ? 0.26f : 0.32f);
      ao.mode.Override(AmbientOcclusionMode.MultiScaleVolumetricObscurance);
    }

    DepthOfField dof = GetOrAddSetting<DepthOfField>(profile);
    dof.enabled.Override(dofEnabled && !isBattle);
    dof.focusDistance.Override(isCampaign ? 4f : 3f);
    dof.aperture.Override(isCampaign ? 18f : 20f);
    dof.kernelSize.Override(KernelSize.Medium);
  }

  private static T GetOrAddSetting<T>(PostProcessProfile profile) where T : PostProcessEffectSettings
  {
    if (!profile.TryGetSettings(out T setting))
    {
      setting = profile.AddSettings<T>();
    }

    return setting;
  }

  private static bool PrefBool(string key, bool defaultValue)
  {
    int fallback = defaultValue ? 1 : 0;
    return PlayerPrefs.GetInt(key, fallback) == 1;
  }

  private static int PrefInt(string key, int defaultValue)
  {
    return PlayerPrefs.GetInt(key, defaultValue);
  }

  private static float PrefFloat(string key, float defaultValue)
  {
    return PlayerPrefs.GetFloat(key, defaultValue);
  }
#endif
}
