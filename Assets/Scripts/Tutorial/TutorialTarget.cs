using System.Collections.Generic;
using UnityEngine;

public class TutorialTarget : MonoBehaviour
{
  private static readonly Dictionary<string, TutorialTarget> targets = new Dictionary<string, TutorialTarget>();

  public string targetId;
  public RectTransform rectTransform;
  public Transform worldTransform;
  public Camera worldCamera;
  public Vector2 worldHighlightSize = new Vector2(120f, 80f);

  public static TutorialTarget Find(string targetId)
  {
    if (string.IsNullOrEmpty(targetId))
    {
      return null;
    }

    targets.TryGetValue(targetId, out TutorialTarget target);
    return target;
  }

  private void Reset()
  {
    rectTransform = GetComponent<RectTransform>();
    worldTransform = transform;
  }

  private void Awake()
  {
    if (rectTransform == null)
    {
      rectTransform = GetComponent<RectTransform>();
    }

    if (worldTransform == null)
    {
      worldTransform = transform;
    }
  }

  public bool TryGetScreenPosition(out Vector3 screenPosition)
  {
    if (rectTransform != null)
    {
      screenPosition = rectTransform.position;
      return true;
    }

    if (worldTransform == null)
    {
      screenPosition = Vector3.zero;
      return false;
    }

    Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;
    if (cameraToUse == null)
    {
      screenPosition = Vector3.zero;
      return false;
    }

    screenPosition = cameraToUse.WorldToScreenPoint(worldTransform.position);
    return screenPosition.z >= 0f;
  }

  public Vector2 GetHighlightSize()
  {
    return rectTransform != null ? rectTransform.rect.size : worldHighlightSize;
  }

  private void OnEnable()
  {
    if (!string.IsNullOrEmpty(targetId))
    {
      targets[targetId] = this;
    }
  }

  private void OnDisable()
  {
    if (!string.IsNullOrEmpty(targetId) && targets.TryGetValue(targetId, out TutorialTarget current) && current == this)
    {
      targets.Remove(targetId);
    }
  }
}
