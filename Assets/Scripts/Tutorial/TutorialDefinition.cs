using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialAdvanceMode
{
  Manual,
  Event,
  Timed,
  None
}

public enum TutorialInputBlockMode
{
  None,
  All,
  AllExceptTarget
}

public enum TutorialActionType
{
  None,
  EmitEvent,
  SetTargetActive,
  CompleteTutorial
}

public enum TutorialPointerDirection
{
  Right,
  Left,
  Up,
  Down
}

public enum TutorialPresentationMode
{
  Full,
  Compact,
  HintOnly
}

public enum TutorialPanelAnchor
{
  Center,
  TopLeft,
  TopRight,
  BottomLeft,
  BottomRight,
  CenterLeft,
  CenterRight
}

[Serializable]
public class TutorialConditionValue
{
  public string key;
  public string value;
}

[Serializable]
public class TutorialCondition
{
  public string eventId;
  public List<TutorialConditionValue> requiredValues = new List<TutorialConditionValue>();

  public bool Matches(TutorialEventPayload payload)
  {
    if (payload == null || payload.eventId != eventId)
    {
      return false;
    }

    for (int i = 0; i < requiredValues.Count; i++)
    {
      TutorialConditionValue required = requiredValues[i];
      if (required == null || string.IsNullOrEmpty(required.key))
      {
        continue;
      }

      string requiredKey = required.key.Trim();
      string expectedValue = (required.value ?? string.Empty).Trim();
      if (!payload.TryGetString(requiredKey, out string actual)
          || !string.Equals((actual ?? string.Empty).Trim(), expectedValue, StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }
    }

    return true;
  }
}

[Serializable]
public class TutorialAction
{
  public TutorialActionType type;
  public string eventId;
  public string targetId;
  public bool active;
}

[Serializable]
public class TutorialStep
{
  public string id;
  [Tooltip("Si está desactivado, el tutorial saltea este paso.")]
  public bool activo = true;
  public string titleKey;
  [TextArea(2, 6)] public string narratorKey;
  [TextArea(2, 6)] public string bodyKey;
  [Header("Audio por idioma")]
  [Tooltip("Audio del narrador en español.")]
  public AudioClip narratorAudio;
  [Tooltip("Audio del narrador en inglés.")]
  public AudioClip narratorAudioIngles;
  [Tooltip("Audio del narrador en portugués.")]
  public AudioClip narratorAudioPortugues;
  public string targetId;
  public TutorialPresentationMode presentationMode = TutorialPresentationMode.Full;
  public TutorialPanelAnchor panelAnchor = TutorialPanelAnchor.Center;
  public Vector2 panelOffset = Vector2.zero;
  public Vector2 compactPanelSize = new Vector2(440f, 190f);
  public bool showPointer = true;
  public bool showHighlight = true;
  public TutorialPointerDirection pointerDirection = TutorialPointerDirection.Right;
  public Vector2 pointerOffset = new Vector2(-80f, 0f);
  public float pointerScale = 1f;
  public float highlightScale = 1f;
  public Vector2 highlightScaleXY = Vector2.one;
  public TutorialAdvanceMode advanceMode = TutorialAdvanceMode.Manual;
  public TutorialInputBlockMode inputBlockMode = TutorialInputBlockMode.None;
  public bool showNextButton = true;
  public bool canGoBack = true;
  public bool canSkip = true;
  [Min(0f)] public float autoAdvanceDelay = 1f;
  public List<TutorialCondition> advanceConditions = new List<TutorialCondition>();
  public List<TutorialAction> enterActions = new List<TutorialAction>();
  public List<TutorialAction> exitActions = new List<TutorialAction>();
}

[CreateAssetMenu(menuName = "GDD/Tutorial/Tutorial Definition", fileName = "TutorialDefinition")]
public class TutorialDefinition : ScriptableObject
{
  public string tutorialId = "tutorial";
  public bool restartIfCompleted;
  public List<TutorialStep> steps = new List<TutorialStep>();

  public int GetStepIndex(string stepId)
  {
    if (string.IsNullOrEmpty(stepId))
    {
      return -1;
    }

    for (int i = 0; i < steps.Count; i++)
    {
      if (steps[i] != null && steps[i].id == stepId)
      {
        return i;
      }
    }

    return -1;
  }

  public TutorialStep GetStep(int index)
  {
    return index >= 0 && index < steps.Count ? steps[index] : null;
  }

  public int GetNextActiveStepIndex(int startIndex)
  {
    if (steps == null)
    {
      return -1;
    }

    for (int i = Mathf.Max(0, startIndex); i < steps.Count; i++)
    {
      if (steps[i] != null && steps[i].activo)
      {
        return i;
      }
    }

    return -1;
  }

  public int GetPreviousActiveStepIndex(int startIndex)
  {
    if (steps == null)
    {
      return -1;
    }

    for (int i = Mathf.Min(startIndex, steps.Count - 1); i >= 0; i--)
    {
      if (steps[i] != null && steps[i].activo)
      {
        return i;
      }
    }

    return -1;
  }
}
