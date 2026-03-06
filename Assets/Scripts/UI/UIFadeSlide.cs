using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class UIFadeSlide : MonoBehaviour
{
  [Header("Timing")]
  [SerializeField] private float fadeInDuration = 0.14f;
  [SerializeField] private float fadeOutDuration = 0.14f;
  [SerializeField] private bool useUnscaledTime = true;

  [Header("Motion")]
  [SerializeField] private Vector2 enterOffset = new Vector2(0f, -14f);
  [SerializeField] private Vector2 exitOffset = new Vector2(0f, -10f);

  [Header("Follow Mouse (optional)")]
  [SerializeField] private bool followMouse = false;
  [SerializeField] private Vector2 followMouseOffset = new Vector2(14f, -18f);
  [SerializeField] [Range(1f, 40f)] private float followLerpSpeed = 20f;

  [Header("Behaviour")]
  [SerializeField] private bool animateOnEnable = false;
  [SerializeField] private bool deactivateOnHide = true;

  private CanvasGroup canvasGroup;
  private RectTransform rectTransform;
  private Coroutine routine;
  private Vector2 baseAnchoredPosition;
  private Vector2 targetAnchoredPosition;
  private bool baseCaptured;
  private bool suppressOnEnableOnce;
  private bool isHiding;

  private void Awake()
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();
  }

  private void OnEnable()
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();

    if (suppressOnEnableOnce)
    {
      suppressOnEnableOnce = false;
      return;
    }

    if (animateOnEnable)
    {
      Show();
    }
  }

  private void OnDisable()
  {
    if (routine != null)
    {
      StopCoroutine(routine);
      routine = null;
    }

    isHiding = false;
  }

  private void LateUpdate()
  {
    if (!followMouse || isHiding || !gameObject.activeSelf)
    {
      return;
    }

    if (!TryScreenToAnchored((Vector2)Input.mousePosition + followMouseOffset, out Vector2 anchored))
    {
      return;
    }

    targetAnchoredPosition = anchored;
    float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    float blend = 1f - Mathf.Exp(-followLerpSpeed * Mathf.Max(0f, dt));
    rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetAnchoredPosition, blend);
  }

  public void SetFollowMouse(bool enabled, Vector2 offset)
  {
    followMouse = enabled;
    followMouseOffset = offset;
  }

  public void SetDurations(float fadeIn, float fadeOut)
  {
    fadeInDuration = Mathf.Max(0.02f, fadeIn);
    fadeOutDuration = Mathf.Max(0.02f, fadeOut);
  }

  public void SetOffsets(Vector2 enter, Vector2 exit)
  {
    enterOffset = enter;
    exitOffset = exit;
  }

  public void Show()
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();
    targetAnchoredPosition = baseAnchoredPosition;
    BeginShowAnimation();
  }

  public void ShowAtScreenPosition(Vector3 screenPosition)
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();

    if (TryScreenToAnchored(screenPosition, out Vector2 anchored))
    {
      targetAnchoredPosition = anchored;
    }
    else
    {
      targetAnchoredPosition = baseAnchoredPosition;
    }

    BeginShowAnimation();
  }

  public void Hide()
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();

    if (!gameObject.activeSelf)
    {
      return;
    }

    if (routine != null)
    {
      StopCoroutine(routine);
    }

    routine = StartCoroutine(HideRoutine());
  }

  public void HideImmediate()
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();

    if (routine != null)
    {
      StopCoroutine(routine);
      routine = null;
    }

    isHiding = false;
    canvasGroup.alpha = 0f;
    rectTransform.anchoredPosition = targetAnchoredPosition;
    gameObject.SetActive(false);
  }

  public void ShowImmediate()
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();

    if (routine != null)
    {
      StopCoroutine(routine);
      routine = null;
    }

    isHiding = false;
    suppressOnEnableOnce = true;
    if (!gameObject.activeSelf)
    {
      gameObject.SetActive(true);
    }

    canvasGroup.alpha = 1f;
    rectTransform.anchoredPosition = targetAnchoredPosition;
  }

  public void ShowAtScreenPositionImmediate(Vector3 screenPosition)
  {
    EnsureRefs();
    CaptureBasePositionIfNeeded();

    if (TryScreenToAnchored(screenPosition, out Vector2 anchored))
    {
      targetAnchoredPosition = anchored;
    }
    else
    {
      targetAnchoredPosition = baseAnchoredPosition;
    }

    ShowImmediate();
  }

  private void BeginShowAnimation()
  {
    suppressOnEnableOnce = true;
    if (!gameObject.activeSelf)
    {
      gameObject.SetActive(true);
    }

    if (routine != null)
    {
      StopCoroutine(routine);
    }

    routine = StartCoroutine(ShowRoutine());
  }

  private IEnumerator ShowRoutine()
  {
    isHiding = false;
    canvasGroup.alpha = 0f;
    rectTransform.anchoredPosition = targetAnchoredPosition + enterOffset;

    float t = 0f;
    while (t < fadeInDuration)
    {
      float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
      t += dt;
      float k = Mathf.Clamp01(t / fadeInDuration);

      canvasGroup.alpha = k;
      rectTransform.anchoredPosition = Vector2.Lerp(targetAnchoredPosition + enterOffset, targetAnchoredPosition, k);
      yield return null;
    }

    canvasGroup.alpha = 1f;
    rectTransform.anchoredPosition = targetAnchoredPosition;
    routine = null;
  }

  private IEnumerator HideRoutine()
  {
    isHiding = true;
    float t = 0f;
    float startAlpha = canvasGroup.alpha;
    Vector2 startPos = rectTransform.anchoredPosition;
    Vector2 endPos = targetAnchoredPosition + exitOffset;

    while (t < fadeOutDuration)
    {
      float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
      t += dt;
      float k = Mathf.Clamp01(t / fadeOutDuration);

      canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, k);
      rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, k);
      yield return null;
    }

    canvasGroup.alpha = 0f;
    rectTransform.anchoredPosition = targetAnchoredPosition;
    isHiding = false;
    routine = null;

    if (deactivateOnHide)
    {
      gameObject.SetActive(false);
    }
  }

  private void EnsureRefs()
  {
    if (rectTransform == null)
    {
      rectTransform = GetComponent<RectTransform>();
    }

    if (canvasGroup == null)
    {
      canvasGroup = GetComponent<CanvasGroup>();
      if (canvasGroup == null)
      {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
      }
    }
  }

  private void CaptureBasePositionIfNeeded()
  {
    if (baseCaptured || rectTransform == null)
    {
      return;
    }

    baseAnchoredPosition = rectTransform.anchoredPosition;
    targetAnchoredPosition = baseAnchoredPosition;
    baseCaptured = true;
  }

  private bool TryScreenToAnchored(Vector2 screenPosition, out Vector2 anchoredPosition)
  {
    anchoredPosition = baseAnchoredPosition;
    if (rectTransform == null)
    {
      return false;
    }

    RectTransform parent = rectTransform.parent as RectTransform;
    if (parent == null)
    {
      return false;
    }

    Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
    Camera eventCamera = null;
    if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
    {
      eventCamera = canvas.worldCamera;
    }

    return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, eventCamera, out anchoredPosition);
  }
}

public static class UIFadeSlideUtility
{
  public static UIFadeSlide Ensure(GameObject target)
  {
    if (target == null) { return null; }

    UIFadeSlide anim = target.GetComponent<UIFadeSlide>();
    if (anim == null)
    {
      anim = target.AddComponent<UIFadeSlide>();
    }
    return anim;
  }

  public static void Show(GameObject target)
  {
    if (target == null) { return; }

    UIFadeSlide anim = target.GetComponent<UIFadeSlide>();
    if (anim != null)
    {
      anim.Show();
      return;
    }

    target.SetActive(true);
  }

  public static void ShowAt(GameObject target, Vector3 screenPosition)
  {
    if (target == null) { return; }

    UIFadeSlide anim = target.GetComponent<UIFadeSlide>();
    if (anim != null)
    {
      anim.ShowAtScreenPosition(screenPosition);
      return;
    }

    target.SetActive(true);
    target.transform.position = screenPosition;
  }

  public static void Hide(GameObject target)
  {
    if (target == null) { return; }

    UIFadeSlide anim = target.GetComponent<UIFadeSlide>();
    if (anim != null)
    {
      anim.Hide();
      return;
    }

    target.SetActive(false);
  }

  public static void ShowImmediate(GameObject target)
  {
    if (target == null) { return; }

    UIFadeSlide anim = target.GetComponent<UIFadeSlide>();
    if (anim != null)
    {
      anim.ShowImmediate();
      return;
    }

    target.SetActive(true);
  }

  public static void ShowAtImmediate(GameObject target, Vector3 screenPosition)
  {
    if (target == null) { return; }

    UIFadeSlide anim = target.GetComponent<UIFadeSlide>();
    if (anim != null)
    {
      anim.ShowAtScreenPositionImmediate(screenPosition);
      return;
    }

    target.SetActive(true);
    target.transform.position = screenPosition;
  }

  public static void HideImmediate(GameObject target)
  {
    if (target == null) { return; }

    UIFadeSlide anim = target.GetComponent<UIFadeSlide>();
    if (anim != null)
    {
      anim.HideImmediate();
      return;
    }

    target.SetActive(false);
  }
}
