using System;
using UnityEngine;

public static class TutorialEvents
{
  public static event Action<TutorialEventPayload> EventEmitted;

  public static void Emit(string eventId, GameObject source = null)
  {
    Emit(new TutorialEventPayload(eventId, source));
  }

  public static void Emit(TutorialEventPayload payload)
  {
    if (payload == null || string.IsNullOrEmpty(payload.eventId))
    {
      return;
    }

    EventEmitted?.Invoke(payload);
  }
}
