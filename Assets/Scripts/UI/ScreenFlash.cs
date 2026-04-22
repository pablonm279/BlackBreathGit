using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
  public static ScreenFlash Instance { get; private set; }

  [Header("Overlay")]
  [SerializeField] private Image overlay;
  [SerializeField] private int sortingOrder = 2000;
  [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

  [Header("Critical Flash")]
  [SerializeField] private Color criticalColor = new Color(0.6f, 0f, 0f, 1f);
  [SerializeField] [Range(0f, 1f)] private float criticalAlpha = 0.17f;
  [SerializeField] private float criticalDelay = 0.10f;
  [SerializeField] private float criticalFadeIn = 0.02f;
  [SerializeField] private float criticalFadeOut = 0.07f;
  [SerializeField] private float minInterval = 0.06f;
  [SerializeField] private bool useUnscaledTime = true;

  private Coroutine flashRoutine;
  private float lastFlashTime = -999f;

  public static void FlashCritical()
  {
    ScreenFlash instance = GetOrCreate();
    if (instance == null)
    {
      return;
    }

    instance.Flash(instance.criticalColor, instance.criticalAlpha, instance.criticalFadeIn, instance.criticalFadeOut, instance.criticalDelay);
  }

  public static void FlashImpact(Color color, float peakAlpha = 0.04f, float fadeIn = 0.014f, float fadeOut = 0.07f, float delay = 0f)
  {
    ScreenFlash instance = GetOrCreate();
    if (instance == null)
    {
      return;
    }

    instance.Flash(color, Mathf.Clamp01(peakAlpha), Mathf.Max(0f, fadeIn), Mathf.Max(0f, fadeOut), Mathf.Max(0f, delay));
  }

  private static ScreenFlash GetOrCreate()
  {
    if (Instance != null)
    {
      return Instance;
    }

    ScreenFlash existing = FindObjectOfType<ScreenFlash>();
    if (existing != null)
    {
      Instance = existing;
      return existing;
    }

    GameObject go = new GameObject("ScreenFlash");
    return go.AddComponent<ScreenFlash>();
  }

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    EnsureOverlay();
  }

  private void EnsureOverlay()
  {
    if (overlay != null)
    {
      ApplyColor(criticalColor, 0f);
      return;
    }

    Canvas canvas = GetComponent<Canvas>();
    if (canvas == null)
    {
      canvas = gameObject.AddComponent<Canvas>();
    }
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = sortingOrder;

    CanvasScaler scaler = GetComponent<CanvasScaler>();
    if (scaler == null)
    {
      scaler = gameObject.AddComponent<CanvasScaler>();
    }
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = referenceResolution;
    scaler.matchWidthOrHeight = 0.5f;

    GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
    if (raycaster != null)
    {
      raycaster.enabled = false;
    }

    GameObject overlayGO = new GameObject("Overlay");
    overlayGO.transform.SetParent(transform, false);
    overlay = overlayGO.AddComponent<Image>();
    overlay.raycastTarget = false;
    RectTransform rect = overlay.rectTransform;
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;

    ApplyColor(criticalColor, 0f);
  }

  private void ApplyColor(Color color, float alpha)
  {
    if (overlay == null)
    {
      return;
    }

    Color c = color;
    c.a = alpha;
    overlay.color = c;
  }

  public void Flash(Color color, float peakAlpha, float fadeIn, float fadeOut, float delay)
  {
    EnsureOverlay();

    float now = useUnscaledTime ? Time.unscaledTime : Time.time;
    if (now - lastFlashTime < minInterval)
    {
      return;
    }
    lastFlashTime = now;

    if (flashRoutine != null)
    {
      StopCoroutine(flashRoutine);
    }
    flashRoutine = StartCoroutine(FlashRoutine(color, peakAlpha, fadeIn, fadeOut, delay));
  }

  private IEnumerator FlashRoutine(Color color, float peakAlpha, float fadeIn, float fadeOut, float delay)
  {
    if (overlay == null)
    {
      yield break;
    }

    ApplyColor(color, 0f);

    if (delay > 0f)
    {
      float t = 0f;
      while (t < delay)
      {
        t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        yield return null;
      }
    }

    if (fadeIn > 0f)
    {
      float t = 0f;
      while (t < fadeIn)
      {
        t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float a = Mathf.Lerp(0f, peakAlpha, t / fadeIn);
        ApplyColor(color, a);
        yield return null;
      }
    }

    ApplyColor(color, peakAlpha);

    if (fadeOut > 0f)
    {
      float t = 0f;
      while (t < fadeOut)
      {
        t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float a = Mathf.Lerp(peakAlpha, 0f, t / fadeOut);
        ApplyColor(color, a);
        yield return null;
      }
    }

    ApplyColor(color, 0f);
    flashRoutine = null;
  }
}
