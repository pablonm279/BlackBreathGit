using System.Collections;
using UnityEngine;

public static class BattleVisualJuice
{
  private const string PrefEnabled = "battle_visual_juice_enabled";
  private static bool? enabledOverride;
  private static bool? cachedPreference;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void ResetRuntimeState()
  {
    enabledOverride = null;
    cachedPreference = null;
  }

  public static bool Enabled
  {
    get
    {
      if (enabledOverride.HasValue)
      {
        return enabledOverride.Value;
      }

      if (!cachedPreference.HasValue)
      {
        cachedPreference = PlayerPrefs.GetInt(PrefEnabled, 1) != 0;
      }

      return cachedPreference.Value;
    }
  }

  public static void SetEnabled(bool enabled, bool guardarPreferencia = true)
  {
    enabledOverride = enabled;
    cachedPreference = enabled;
    if (!guardarPreferencia)
    {
      return;
    }

    PlayerPrefs.SetInt(PrefEnabled, enabled ? 1 : 0);
    PlayerPrefs.Save();
  }

  public static void ClearRuntimeOverride()
  {
    enabledOverride = null;
  }

  public static void ReproducirImpacto(Unidad causante, Unidad objetivo, bool critico, bool muerte, float danio)
  {
    if (!Enabled || objetivo == null || danio <= 0f)
    {
      return;
    }

    BattleVisualJuiceRuntime runtime = BattleVisualJuiceRuntime.Instance;
    if (runtime == null)
    {
      return;
    }

    float intensidad = muerte ? 1f : (critico ? 0.78f : Mathf.Clamp01(0.34f + danio / 90f));
    float duracionShake = muerte ? 0.22f : (critico ? 0.16f : 0.1f);
    runtime.RequestCameraShake(intensidad, duracionShake);

    float duracionHitStop = muerte ? 0.075f : (critico ? 0.055f : 0.025f);
    runtime.RequestHitStop(duracionHitStop, muerte ? 0.035f : 0.075f);
  }
}

[DefaultExecutionOrder(1000)]
public sealed class BattleVisualJuiceRuntime : MonoBehaviour
{
  public static BattleVisualJuiceRuntime Instance { get; private set; }

  [SerializeField] private float amplitudShakePixeles = 3.2f;

  private Camera cameraShake;
  private Vector3 offsetShakeAplicado;
  private Vector3 posicionBaseShakeAplicado;
  private float shakeRestante;
  private float shakeDuracion;
  private float shakeIntensidad;
  private Coroutine coroutineHitStop;
  private bool hitStopActivo;
  private float escalaTiempoAnterior = 1f;
  private float escalaHitStopAplicada = 0.075f;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Boot()
  {
    if (Instance != null)
    {
      return;
    }

    GameObject go = new GameObject("BattleVisualJuiceRuntime");
    Instance = go.AddComponent<BattleVisualJuiceRuntime>();
    DontDestroyOnLoad(go);
  }

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  private void OnDisable()
  {
    QuitarOffsetShake();
    RestaurarTiempoSiCorresponde();
  }

  private void OnDestroy()
  {
    QuitarOffsetShake();
    RestaurarTiempoSiCorresponde();
    if (Instance == this)
    {
      Instance = null;
    }
  }

  private void LateUpdate()
  {
    QuitarOffsetShake();

    if (!BattleVisualJuice.Enabled || BattleManager.Instance == null || shakeRestante <= 0f)
    {
      shakeRestante = Mathf.Max(0f, shakeRestante - Time.unscaledDeltaTime);
      return;
    }

    Camera cam = ResolverCamara();
    if (cam == null)
    {
      return;
    }

    shakeRestante = Mathf.Max(0f, shakeRestante - Time.unscaledDeltaTime);
    float duracion = Mathf.Max(0.01f, shakeDuracion);
    float caida = Mathf.Clamp01(shakeRestante / duracion);
    Vector2 ruido = Random.insideUnitCircle * (amplitudShakePixeles * 0.001f * shakeIntensidad * caida);
    offsetShakeAplicado = new Vector3(ruido.x, ruido.y, 0f);
    cameraShake = cam;
    posicionBaseShakeAplicado = cameraShake.transform.localPosition;
    cameraShake.transform.localPosition += offsetShakeAplicado;
  }

  public void RequestCameraShake(float intensidad, float duracion)
  {
    if (!BattleVisualJuice.Enabled || BattleManager.Instance == null)
    {
      return;
    }

    shakeIntensidad = Mathf.Max(shakeIntensidad, Mathf.Clamp01(intensidad));
    shakeDuracion = Mathf.Max(shakeDuracion, Mathf.Max(0.01f, duracion));
    shakeRestante = Mathf.Max(shakeRestante, Mathf.Max(0.01f, duracion));
  }

  public void RequestHitStop(float duracionReal, float escalaTiempo)
  {
    if (!BattleVisualJuice.Enabled || BattleManager.Instance == null || Time.timeScale <= 0.001f)
    {
      return;
    }

    if (!hitStopActivo)
    {
      escalaTiempoAnterior = Time.timeScale;
      hitStopActivo = true;
    }

    float escalaSolicitada = Mathf.Clamp(escalaTiempo, 0.01f, 0.25f);
    Time.timeScale = Mathf.Min(Time.timeScale, escalaSolicitada);
    escalaHitStopAplicada = Time.timeScale;

    if (coroutineHitStop != null)
    {
      StopCoroutine(coroutineHitStop);
    }

    coroutineHitStop = StartCoroutine(RestaurarTiempoTras(Mathf.Max(0.01f, duracionReal)));
  }

  private IEnumerator RestaurarTiempoTras(float duracionReal)
  {
    yield return new WaitForSecondsRealtime(duracionReal);
    coroutineHitStop = null;
    RestaurarTiempoSiCorresponde();
  }

  private void RestaurarTiempoSiCorresponde()
  {
    if (!hitStopActivo)
    {
      return;
    }

    if (Mathf.Approximately(Time.timeScale, escalaHitStopAplicada))
    {
      Time.timeScale = escalaTiempoAnterior;
    }

    hitStopActivo = false;
  }

  private Camera ResolverCamara()
  {
    if (cameraShake != null && cameraShake.isActiveAndEnabled)
    {
      return cameraShake;
    }

    BattleManager battleManager = BattleManager.Instance;
    if (battleManager != null && battleManager.goCamara != null)
    {
      cameraShake = battleManager.goCamara.GetComponent<Camera>();
      if (cameraShake == null)
      {
        cameraShake = battleManager.goCamara.GetComponentInChildren<Camera>();
      }
    }

    if (cameraShake == null)
    {
      cameraShake = Camera.main;
    }

    return cameraShake;
  }

  private void QuitarOffsetShake()
  {
    if (cameraShake != null && offsetShakeAplicado.sqrMagnitude > 0f)
    {
      Vector3 posicionEsperada = posicionBaseShakeAplicado + offsetShakeAplicado;
      if ((cameraShake.transform.localPosition - posicionEsperada).sqrMagnitude <= 0.0000001f)
      {
        cameraShake.transform.localPosition = posicionBaseShakeAplicado;
      }
    }

    offsetShakeAplicado = Vector3.zero;
  }
}
