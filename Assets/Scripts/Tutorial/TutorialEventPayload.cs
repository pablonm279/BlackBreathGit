using System.Collections.Generic;
using UnityEngine;

public class TutorialEventPayload
{
  public readonly string eventId;
  public readonly GameObject source;
  private readonly Dictionary<string, string> values = new Dictionary<string, string>();

  public TutorialEventPayload(string eventId, GameObject source = null)
  {
    this.eventId = eventId;
    this.source = source;
  }

  public TutorialEventPayload Add(string key, string value)
  {
    if (!string.IsNullOrEmpty(key))
    {
      values[key] = value ?? string.Empty;
    }

    return this;
  }

  public TutorialEventPayload Add(string key, int value)
  {
    return Add(key, value.ToString());
  }

  public bool TryGetString(string key, out string value)
  {
    return values.TryGetValue(key, out value);
  }

  public string GetString(string key, string fallback = "")
  {
    return values.TryGetValue(key, out string value) ? value : fallback;
  }

  public int GetInt(string key, int fallback = 0)
  {
    return values.TryGetValue(key, out string value) && int.TryParse(value, out int result)
      ? result
      : fallback;
  }
}
