using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

public sealed class VisualPolishRuntime : MonoBehaviour
{
  private static VisualPolishRuntime instance;

  private const string PrefVsync = "gfx_vsync";
  private const string PrefFpsLimit = "gfx_fps_limit";
  private const string PrefBrightness = "gfx_brightness";
  private const string PrefContrast = "gfx_contrast";
  private const string EscenaMenuPrincipal = "ES-MenuPrincipal";
  private const float InfluenciaBrilloMenuPrincipal = 0.30f;
  private const float BlackBreathParticleReductionPerQualityLevel = 0.15f;
  private const float CampaignParticleReductionPerQualityLevel = 0.25f;
  private const float CampaignBlackBreathReductionPerQualityLevel = 0.30f;
  private const float CampaignParticleBaseMultiplier = 0.60f;
  private const float CampaignBlackBreathBaseMultiplier = 0.45f;
  private const float CampaignGeneratedVfxBaseMultiplier = 0.25f;
  private const float CampaignLargePointLightRange = 40f;

  private struct MenuParticleState
  {
    public bool emissionEnabled;
  }

  private struct ParticleAmountDefaults
  {
    public int maxParticles;
    public ParticleSystem.MinMaxCurve rateOverTime;
    public ParticleSystem.MinMaxCurve rateOverDistance;
    public ParticleSystem.Burst[] bursts;
  }

  private struct CampaignLightDefaults
  {
    public LightShadows shadows;
    public LightShadowResolution shadowResolution;
  }

  private static readonly Dictionary<int, ParticleAmountDefaults> particleAmountDefaultsById = new Dictionary<int, ParticleAmountDefaults>();
  private static readonly Dictionary<int, MenuParticleState> menuParticleStatesById = new Dictionary<int, MenuParticleState>();
  private static readonly Dictionary<int, CampaignLightDefaults> campaignLightDefaultsById = new Dictionary<int, CampaignLightDefaults>();
  private static int campaignGeneratedVfxRootCounter;

  [SerializeField] private bool rebalanceQualityAtRuntime = true;
  [SerializeField] private int postProcessLayerIndex = 12; // Bit 4096 in current scenes.
  [SerializeField] private bool ensureSceneGlobalPostFxVolume = true;
  [Header("UI Responsive")]
  [SerializeField] private bool normalizeScreenSpaceCanvases = true;
  [SerializeField] private Vector2 uiReferenceResolution = new Vector2(1920f, 1080f);
  [SerializeField] [Range(0f, 1f)] private float uiMatchWidthOrHeight = 0.5f;

  [Header("Defaults (if prefs are missing)")]
  [SerializeField] private bool defaultVsyncEnabled = true;
  [SerializeField] private int defaultFpsLimit = 60;
  [Header("Safety")]
  [SerializeField] private bool clampUnlimitedFpsInBattle = true;
  [SerializeField] [Range(30, 240)] private int unlimitedBattleFpsCap = 120;
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
    Scene scene = SceneManager.GetActiveScene();
    GraphicsQuality.ApplyEngine();

    if (instance != null)
    {
      instance.ApplySyncAndFrameRatePrefs(scene);
      ApplyMainMenuParticleQuality(scene);
      ApplyCampaignParticleQualityScale(scene);
      instance.ApplyAlientoNegroParticleQualityScale(scene);
      ApplyCampaignLightQuality(scene);
    }

#if UNITY_POST_PROCESSING_STACK_V2
    if (instance == null) { return; }
    instance.ApplyCameraPostFxAA(scene);
    if (instance.ensureSceneGlobalPostFxVolume)
    {
      instance.EnsureSceneGlobalVolume(scene);
    }
#endif
  }

  // Reaplica solo el volumen global (brillo, contraste, filtro de daltonismo): mas liviano que
  // ApplyPostProcessingPrefsNow, pensado para sliders que se arrastran.
  public static void RefreshColorGradingNow()
  {
#if UNITY_POST_PROCESSING_STACK_V2
    if (instance == null) { return; }
    instance.EnsureSceneGlobalVolume(SceneManager.GetActiveScene());
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
    campaignGeneratedVfxRootCounter = 0;
    ApplyScenePolish(scene);
  }

  private void ApplyScenePolish(Scene scene)
  {
    // Primero la calidad (SetQualityLevel pisa el vSync) y despues vSync / limite de FPS.
    if (rebalanceQualityAtRuntime)
    {
      ApplyRuntimeQualityRebalance();
    }

    ApplySyncAndFrameRatePrefs(scene);

    NormalizeSceneCanvasScalers(scene);
    ApplyMainMenuParticleQuality(scene);
    ApplyCampaignParticleQualityScale(scene);
    ApplyAlientoNegroParticleQualityScale(scene);
    ApplyCampaignLightQuality(scene);

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
    // Aplica la calidad guardada (y overrides de sombras / texturas) en cada escena.
    GraphicsQuality.ApplyEngine();
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

  public static int ResolveTargetFrameRate(bool vsyncEnabled, int fpsLimit, Scene scene)
  {
    if (vsyncEnabled)
    {
      return -1;
    }

    if (fpsLimit > 0)
    {
      return Mathf.Clamp(fpsLimit, 30, 240);
    }

    if (instance != null && instance.clampUnlimitedFpsInBattle && IsBattleScene(scene))
    {
      return Mathf.Clamp(instance.unlimitedBattleFpsCap, 30, 240);
    }

    return -1;
  }

  private void ApplySyncAndFrameRatePrefs(Scene scene)
  {
    bool vsyncEnabled = PrefBool(PrefVsync, defaultVsyncEnabled);
    QualitySettings.vSyncCount = LinuxDeckRuntime.ResolverVSyncCount(vsyncEnabled);

    int fpsLimit = PrefInt(PrefFpsLimit, defaultFpsLimit);
    Application.targetFrameRate = ResolveTargetFrameRate(vsyncEnabled, fpsLimit, scene);
  }

  public static float ResolveQualityAmountMultiplier(float reductionPerLevel)
  {
    // particleSteps equivale a "niveles por debajo de Ultra" del esquema original de 3 calidades
    // (Ultra 0, Medium 1, Low 2) y sale de la tabla de GraphicsQuality (incluye el override de densidad del jugador).
    float pasos = GraphicsQuality.Current.particleSteps;
    if (pasos <= 0f)
    {
      return 1f;
    }

    return Mathf.Clamp01(1f - (Mathf.Max(0f, reductionPerLevel) * pasos));
  }

  public static void ApplyBlackBreathParticleQualityScale(GameObject root)
  {
    ApplyParticleAmountQualityScale(root, BlackBreathParticleReductionPerQualityLevel);
  }

  public static void ApplyCampaignParticleQualityScaleNow()
  {
    ApplyCampaignParticleQualityScale(SceneManager.GetActiveScene());
  }

  public static void ApplyGeneratedCampaignVfxQualityScale(GameObject root)
  {
    if (root == null || !IsCampaignScene(root.scene)) { return; }

    ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
    float multiplier = CampaignGeneratedVfxBaseMultiplier
      * ResolveQualityAmountMultiplier(CampaignParticleReductionPerQualityLevel);
    for (int i = 0; i < systems.Length; i++)
    {
      ApplyParticleSystemAmountScale(systems[i], multiplier);
    }

    GraphicsQuality.Settings calidad = GraphicsQuality.Current;
    int indiceVfx = campaignGeneratedVfxRootCounter++;

    ApplyGeneratedCampaignVfxLod(root, calidad);
    ApplyGeneratedCampaignVfxLightBudget(root, indiceVfx, calidad);
  }

  private static void ApplyGeneratedCampaignVfxLod(GameObject root, GraphicsQuality.Settings calidad)
  {
    if (root.GetComponent<LODGroup>() != null) { return; }

    ParticleSystemRenderer[] particleRenderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
    if (particleRenderers.Length == 0) { return; }

    float alturaMinimaPantalla = calidad.vfxLodHeight;

    LODGroup lodGroup = root.AddComponent<LODGroup>();
    lodGroup.fadeMode = LODFadeMode.None;
    lodGroup.animateCrossFading = false;
    lodGroup.SetLODs(new[]
    {
      new LOD(alturaMinimaPantalla, particleRenderers)
    });
    lodGroup.RecalculateBounds();
  }

  private static void ApplyGeneratedCampaignVfxLightBudget(
    GameObject root,
    int indiceVfx,
    GraphicsQuality.Settings calidad)
  {
    if (root.name.IndexOf("llama", System.StringComparison.OrdinalIgnoreCase) < 0) { return; }

    bool conservarLuz = calidad.vfxLightEvery > 0 && indiceVfx % calidad.vfxLightEvery == 0;
    if (conservarLuz) { return; }

    Light[] lights = root.GetComponentsInChildren<Light>(true);
    for (int i = 0; i < lights.Length; i++)
    {
      Light light = lights[i];
      if (light != null
          && light.type == LightType.Point
          && light.range <= 1.5f
          && light.shadows == LightShadows.None)
      {
        light.enabled = false;
      }
    }
  }

  public static void ApplyParticleAmountQualityScale(GameObject root, float reductionPerLevel)
  {
    if (root == null) { return; }

    ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
    float multiplier = ResolveQualityAmountMultiplier(reductionPerLevel);
    for (int i = 0; i < systems.Length; i++)
    {
      ApplyParticleSystemAmountScale(systems[i], multiplier);
    }
  }

  private static void ApplyMainMenuParticleQuality(Scene scene)
  {
    if (!IsMainMenuScene(scene)) { return; }

    bool desactivarParticulas = IsLowGraphicsQuality();
    ParticleSystem[] systems = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    for (int i = 0; i < systems.Length; i++)
    {
      ParticleSystem ps = systems[i];
      if (ps == null) { continue; }
      if (ps.gameObject.scene != scene) { continue; }

      int id = ps.GetInstanceID();
      if (!menuParticleStatesById.TryGetValue(id, out MenuParticleState estadoOriginal))
      {
        var emissionOriginal = ps.emission;
        estadoOriginal = new MenuParticleState
        {
          emissionEnabled = emissionOriginal.enabled
        };
        menuParticleStatesById[id] = estadoOriginal;
      }

      var emission = ps.emission;
      if (desactivarParticulas)
      {
        emission.enabled = false;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        continue;
      }

      emission.enabled = estadoOriginal.emissionEnabled;
      var main = ps.main;
      if (estadoOriginal.emissionEnabled && main.playOnAwake && ps.gameObject.activeInHierarchy && !ps.isPlaying)
      {
        ps.Play(true);
      }
    }
  }

  private void ApplyAlientoNegroParticleQualityScale(Scene scene)
  {
    ParticleSystem[] systems = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    bool esCampania = IsCampaignScene(scene);
    float multiplier = ResolveQualityAmountMultiplier(
      esCampania
        ? CampaignBlackBreathReductionPerQualityLevel
        : BlackBreathParticleReductionPerQualityLevel);
    if (esCampania)
    {
      multiplier *= CampaignBlackBreathBaseMultiplier;
    }
    for (int i = 0; i < systems.Length; i++)
    {
      ParticleSystem ps = systems[i];
      if (ps == null) { continue; }
      if (ps.gameObject.scene != scene) { continue; }
      if (!EsSistemaParticulasAlientoNegro(ps.transform)) { continue; }

      ApplyParticleSystemAmountScale(ps, multiplier);
    }
  }

  private static void ApplyCampaignParticleQualityScale(Scene scene)
  {
    if (!IsCampaignScene(scene)) { return; }

    ParticleSystem[] systems = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    float multiplier = CampaignParticleBaseMultiplier
      * ResolveQualityAmountMultiplier(CampaignParticleReductionPerQualityLevel);
    for (int i = 0; i < systems.Length; i++)
    {
      ParticleSystem ps = systems[i];
      if (ps == null) { continue; }
      if (ps.gameObject.scene != scene) { continue; }

      ApplyParticleSystemAmountScale(ps, multiplier);
    }
  }

  private static void ApplyCampaignLightQuality(Scene scene)
  {
    if (!IsCampaignScene(scene)) { return; }

    int sombrasLuces = GraphicsQuality.Current.campaignLightShadows;

    Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    for (int i = 0; i < lights.Length; i++)
    {
      Light light = lights[i];
      if (light == null || light.gameObject.scene != scene) { continue; }
      if (light.type != LightType.Point || light.range < CampaignLargePointLightRange) { continue; }

      int id = light.GetInstanceID();
      if (!campaignLightDefaultsById.TryGetValue(id, out CampaignLightDefaults defaults))
      {
        defaults = new CampaignLightDefaults
        {
          shadows = light.shadows,
          shadowResolution = light.shadowResolution
        };
        campaignLightDefaultsById[id] = defaults;
      }

      if (defaults.shadows == LightShadows.None)
      {
        continue;
      }

      if (sombrasLuces <= 0)
      {
        light.shadows = LightShadows.None;
        continue;
      }

      if (sombrasLuces == 1)
      {
        light.shadows = LightShadows.Hard;
        light.shadowResolution = LightShadowResolution.Low;
        continue;
      }

      light.shadows = LightShadows.Soft;
      light.shadowResolution = LightShadowResolution.Medium;
    }
  }

  private static bool EsSistemaParticulasAlientoNegro(Transform transform)
  {
    while (transform != null)
    {
      string nombre = transform.name;
      if (!string.IsNullOrEmpty(nombre) && nombre.ToLowerInvariant().Contains("aliento"))
      {
        return true;
      }

      transform = transform.parent;
    }

    return false;
  }

  private static void ApplyParticleSystemAmountScale(ParticleSystem ps, float multiplier)
  {
    if (ps == null) { return; }

    int id = ps.GetInstanceID();
    if (!particleAmountDefaultsById.TryGetValue(id, out ParticleAmountDefaults defaults))
    {
      var mainDefaults = ps.main;
      var emissionDefaults = ps.emission;
      ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emissionDefaults.burstCount];
      if (bursts.Length > 0)
      {
        emissionDefaults.GetBursts(bursts);
      }

      defaults = new ParticleAmountDefaults
      {
        maxParticles = mainDefaults.maxParticles,
        rateOverTime = emissionDefaults.rateOverTime,
        rateOverDistance = emissionDefaults.rateOverDistance,
        bursts = bursts
      };
      particleAmountDefaultsById[id] = defaults;
    }

    var main = ps.main;
    main.maxParticles = defaults.maxParticles > 0
      ? Mathf.Max(1, Mathf.RoundToInt(defaults.maxParticles * multiplier))
      : defaults.maxParticles;

    var emission = ps.emission;
    emission.rateOverTime = ScaleMinMaxCurve(defaults.rateOverTime, multiplier);
    emission.rateOverDistance = ScaleMinMaxCurve(defaults.rateOverDistance, multiplier);

    if (defaults.bursts != null && defaults.bursts.Length > 0)
    {
      ParticleSystem.Burst[] scaledBursts = new ParticleSystem.Burst[defaults.bursts.Length];
      for (int i = 0; i < defaults.bursts.Length; i++)
      {
        scaledBursts[i] = defaults.bursts[i];
        scaledBursts[i].count = ScaleMinMaxCurve(defaults.bursts[i].count, multiplier);
      }

      emission.SetBursts(scaledBursts);
    }
  }

  private static ParticleSystem.MinMaxCurve ScaleMinMaxCurve(ParticleSystem.MinMaxCurve source, float multiplier)
  {
    ParticleSystem.MinMaxCurve scaled = source;
    switch (source.mode)
    {
      case ParticleSystemCurveMode.Constant:
        scaled.constant = source.constant * multiplier;
        break;
      case ParticleSystemCurveMode.TwoConstants:
        scaled.constantMin = source.constantMin * multiplier;
        scaled.constantMax = source.constantMax * multiplier;
        break;
      case ParticleSystemCurveMode.Curve:
      case ParticleSystemCurveMode.TwoCurves:
        scaled.curveMultiplier = source.curveMultiplier * multiplier;
        break;
    }

    return scaled;
  }

  private static bool IsBattleScene(Scene scene)
  {
    return scene.IsValid()
      && !string.IsNullOrWhiteSpace(scene.name)
      && scene.name.ToLowerInvariant().Contains("batalla");
  }

  private static bool IsMainMenuScene(Scene scene)
  {
    return scene.IsValid()
      && scene.name == EscenaMenuPrincipal;
  }

  private static bool IsLowGraphicsQuality()
  {
    return !GraphicsQuality.Current.menuParticles;
  }

  private static bool IsCampaignScene(Scene scene)
  {
    return scene.IsValid()
      && !string.IsNullOrWhiteSpace(scene.name)
      && scene.name.ToLowerInvariant().Contains("camp");
  }

#if UNITY_POST_PROCESSING_STACK_V2
  private void ApplyCameraPostFxAA(Scene scene)
  {
    Camera[] cameras = FindObjectsOfType<Camera>(true);
    int volumeMask = 1 << postProcessLayerIndex;
    GraphicsQuality.Settings calidad = GraphicsQuality.Current;
    bool postFxEnabled = calidad.postProcessing;

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

      // HDR conserva los brillos emisivos para el bloom; en Low se trabaja en LDR (solo ColorGrading).
      // PPSv2 agrega dithering de ruido azul en su pasada final.
      cam.allowHDR = calidad.hdr;
      layer.stopNaNPropagation = true;
      layer.volumeLayer = volumeMask;

      switch (calidad.aa)
      {
        case GraphicsQuality.AntiAliasing.FXAA:
          layer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
          layer.fastApproximateAntialiasing.fastMode = true;
          break;
        case GraphicsQuality.AntiAliasing.SMAA:
          // SMAA evita el ghosting temporal / sensacion de motion blur al mover personajes.
          layer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
          layer.subpixelMorphologicalAntialiasing.quality = calidad.smaaHigh && !IsCampaignScene(scene)
            ? SubpixelMorphologicalAntialiasing.Quality.High
            : SubpixelMorphologicalAntialiasing.Quality.Medium;
          break;
        default:
          layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
          break;
      }
    }
  }

  private void EnsureSceneGlobalVolume(Scene scene)
  {
    string sceneName = scene.name.ToLowerInvariant();
    bool isMenu = sceneName.Contains("menu");
    bool isMainMenu = IsMainMenuScene(scene);
    bool isCampaign = sceneName.Contains("camp");
    bool isBattle = sceneName.Contains("batalla");
    if (!isMenu && !isCampaign && !isBattle) { return; }

    GraphicsQuality.Settings calidad = GraphicsQuality.Current;
    bool postFxEnabled = calidad.postProcessing;
    float brightnessIncrease = isMainMenu ? 0.02f : 0.10f;
    float brightness = Mathf.Clamp01(PrefFloat(PrefBrightness, defaultBrightness) + brightnessIncrease);
    float sceneBrightnessExposureRange = brightnessExposureRange;
    if (isMainMenu)
    {
      // El fondo del menú ya tiene una exposición base: el slider debe corregirlo
      // sin quemar luces y detalles de la ilustración.
      sceneBrightnessExposureRange *= InfluenciaBrilloMenuPrincipal;
    }
    else if (isCampaign)
    {
      // La campaña conserva el margen extra de brillo de las escenas front-end.
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

    BuildSceneProfile(profile, isMenu, isCampaign, isBattle, calidad, brightnessExposureOffset, contrastOffset);
  }

  private static void BuildSceneProfile(PostProcessProfile profile, bool isMenu, bool isCampaign, bool isBattle, GraphicsQuality.Settings calidad, float brightnessExposureOffset, float contrastOffset)
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
      color.temperature.Override(-5f);
      color.saturation.Override(-3f);
      baseExposure = 0.18f;
      baseContrast = 6f;
    }
    else if (isBattle)
    {
      color.temperature.Override(-3f);
      color.saturation.Override(-1f);
      baseExposure = 0.04f;
      baseContrast = 5f;
    }
    // La exposicion usa pasos logaritmicos: este aumento multiplica el brillo base por 1.10.
    color.postExposure.Override(baseExposure + brightnessExposureOffset + Mathf.Log(1.10f, 2f));
    color.contrast.Override(Mathf.Clamp(baseContrast + contrastOffset, -100f, 100f));

    // Accesibilidad: filtro para daltonismo (mezclador de canales; identidad cuando esta apagado).
    float[] mezcla = AccessibilitySettings.GetColorMixer();
    color.mixerRedOutRedIn.Override(mezcla[0]);
    color.mixerRedOutGreenIn.Override(mezcla[1]);
    color.mixerRedOutBlueIn.Override(mezcla[2]);
    color.mixerGreenOutRedIn.Override(mezcla[3]);
    color.mixerGreenOutGreenIn.Override(mezcla[4]);
    color.mixerGreenOutBlueIn.Override(mezcla[5]);
    color.mixerBlueOutRedIn.Override(mezcla[6]);
    color.mixerBlueOutGreenIn.Override(mezcla[7]);
    color.mixerBlueOutBlueIn.Override(mezcla[8]);

    Bloom bloom = GetOrAddSetting<Bloom>(profile);
    bloom.enabled.Override(calidad.bloom);
    bloom.fastMode.Override(calidad.bloomFast);
    bloom.intensity.Override(isBattle ? 0.14f : 0.22f);
    bloom.threshold.Override(isBattle ? 1.22f : 1.15f);
    bloom.softKnee.Override(0.25f);

    Vignette vignette = GetOrAddSetting<Vignette>(profile);
    vignette.enabled.Override(!isMenu);
    if (!isMenu)
    {
      vignette.intensity.Override(isBattle ? 0.12f : 0.14f);
      vignette.smoothness.Override(isBattle ? 0.24f : 0.26f);
    }

    AmbientOcclusion ao = GetOrAddSetting<AmbientOcclusion>(profile);
    bool habilitarAo = !isMenu && (isCampaign ? calidad.aoCampaign : calidad.aoBattle);
    ao.enabled.Override(habilitarAo);
    if (habilitarAo)
    {
      ao.intensity.Override(isBattle ? 0.26f : isCampaign ? 0.20f : 0.32f);
      ao.mode.Override(AmbientOcclusionMode.MultiScaleVolumetricObscurance);
    }

    if (isCampaign)
    {
      ChromaticAberration chromaticAberration = GetOrAddSetting<ChromaticAberration>(profile);
      chromaticAberration.enabled.Override(false);
    }

    DepthOfField dof = GetOrAddSetting<DepthOfField>(profile);
    dof.enabled.Override(calidad.dof && !isBattle);
    dof.focusDistance.Override(isCampaign ? 4f : 3f);
    dof.aperture.Override(isCampaign ? 18f : 20f);
    dof.kernelSize.Override(calidad.dofKernel > 0 ? KernelSize.Medium : KernelSize.Small);
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
