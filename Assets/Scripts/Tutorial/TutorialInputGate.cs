using UnityEngine;
using UnityEngine.UI;

public class TutorialInputGate : MonoBehaviour
{
  [SerializeField] private string targetId;
  [SerializeField] private Selectable selectable;

  private bool originalInteractable = true;
  private bool hasOriginalState;

  private void Reset()
  {
    selectable = GetComponent<Selectable>();
    TutorialTarget target = GetComponent<TutorialTarget>();
    if (target != null)
    {
      targetId = target.targetId;
    }
  }

  private void Awake()
  {
    if (selectable == null)
    {
      selectable = GetComponent<Selectable>();
    }
  }

  private void OnEnable()
  {
    CacheOriginalState();
  }

  private void OnDisable()
  {
    RestoreOriginalState();
  }

  private void Update()
  {
    if (selectable == null)
    {
      return;
    }

    CacheOriginalState();
    TutorialDirector director = TutorialDirector.Instance;
    bool allowed = director == null || director.AllowsInput(ResolveTargetId());
    selectable.interactable = originalInteractable && allowed;
  }

  private string ResolveTargetId()
  {
    if (!string.IsNullOrEmpty(targetId))
    {
      return targetId;
    }

    TutorialTarget target = GetComponent<TutorialTarget>();
    return target != null ? target.targetId : string.Empty;
  }

  private void CacheOriginalState()
  {
    if (hasOriginalState || selectable == null)
    {
      return;
    }

    originalInteractable = selectable.interactable;
    hasOriginalState = true;
  }

  private void RestoreOriginalState()
  {
    if (selectable != null && hasOriginalState)
    {
      selectable.interactable = originalInteractable;
    }
  }
}
