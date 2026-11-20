using System;
using System.Collections.Generic;
using UnityEngine;

// Acciones de teclado que el jugador puede reasignar. Escape (menu / cancelar), el mouse y las teclas de debug
// no forman parte de este sistema y son fijas.
public enum HotkeyAction
{
  // ---- Campaña
  CampRest = 0,
  CampCharacters,
  CampUpgrades,
  CampFollowers,
  CampQuickSave,
  CampQuickLoad,
  CampChar1,
  CampChar2,
  CampChar3,
  CampChar4,
  CampChar5,
  CampChar6,
  CampChar7,

  // ---- Batalla
  BattleEndTurn = 20,
  BattleTacticalView,
  BattleEnemyInfo,
  BattleFastMode,
  BattlePreviewHostile,
  BattleSkill1,
  BattleSkill2,
  BattleSkill3,
  BattleSkill4,
  BattleSkill5,
  BattleSkill6,
  BattleSkill7,
  BattleSkill8,
  BattleSkill9,
  BattleSkill10,

  // ---- General (campaña y batalla)
  Handbook = 50,
  CameraLeft,
  CameraRight,
  CameraUp,
  CameraDown
}

public enum HotkeyContext { Campaign, Battle, General }

// Fuente unica de las teclas del juego. Cada accion admite dos teclas (principal y alternativa).
// Los bindings se guardan en PlayerPrefs y los defaults reproducen las teclas originales del juego.
public static class Hotkeys
{
  private const string PrefKey = "hotkeys_v1";

  [Serializable]
  private class Entry
  {
    public int a;
    public int k1;
    public int k2;
  }

  [Serializable]
  private class Store
  {
    public List<Entry> e = new List<Entry>();
  }

  private static readonly Dictionary<HotkeyAction, KeyCode[]> defaults = new Dictionary<HotkeyAction, KeyCode[]>();
  private static readonly Dictionary<HotkeyAction, KeyCode[]> current = new Dictionary<HotkeyAction, KeyCode[]>();
  private static readonly List<HotkeyAction> allActions = new List<HotkeyAction>();
  private static KeyCode[] capturableKeys;
  private static bool loaded;
  private static int captureEndFrame = -1;

  public static event Action Changed;

  // El tutorial se diseño con las teclas originales: mientras esta activo se ignoran los bindings del jugador.
  public static bool TutorialCampaign;
  public static bool TutorialBattle;

  // Mientras el jugador asigna una tecla no se debe disparar ninguna accion del juego.
  public static bool Capturing;

  public static IList<HotkeyAction> Actions
  {
    get { EnsureLoaded(); return allActions; }
  }

  // ---------------------------------------------------------------- consulta

  public static bool Down(HotkeyAction action)
  {
    if (Suppressed()) { return false; }
    KeyCode[] keys = Resolve(action);
    return IsKeyDown(keys[0]) || IsKeyDown(keys[1]);
  }

  public static bool Held(HotkeyAction action)
  {
    if (Suppressed()) { return false; }
    KeyCode[] keys = Resolve(action);
    return IsKeyHeld(keys[0]) || IsKeyHeld(keys[1]);
  }

  public static KeyCode GetKey(HotkeyAction action, int slot)
  {
    EnsureLoaded();
    return current[action][Mathf.Clamp(slot, 0, 1)];
  }

  public static KeyCode GetDefaultKey(HotkeyAction action, int slot)
  {
    EnsureLoaded();
    return defaults[action][Mathf.Clamp(slot, 0, 1)];
  }

  public static HotkeyContext ContextOf(HotkeyAction action)
  {
    int v = (int)action;
    if (v >= (int)HotkeyAction.Handbook) { return HotkeyContext.General; }
    if (v >= (int)HotkeyAction.BattleEndTurn) { return HotkeyContext.Battle; }
    return HotkeyContext.Campaign;
  }

  // Nombre corto de la tecla principal (o la alternativa si la principal esta libre), para mostrar en botones.
  public static string ShortName(HotkeyAction action)
  {
    EnsureLoaded();
    KeyCode[] keys = current[action];
    KeyCode key = keys[0] != KeyCode.None ? keys[0] : keys[1];
    return KeyName(key);
  }

  // ---------------------------------------------------------------- escritura

  // Asigna una tecla. Si otra accion del mismo contexto (o general) ya la usaba, se le quita y se devuelve cual fue.
  public static bool TryAssign(HotkeyAction action, int slot, KeyCode key, out HotkeyAction displaced)
  {
    EnsureLoaded();
    displaced = action;
    bool hadConflict = false;

    if (key != KeyCode.None)
    {
      HotkeyContext context = ContextOf(action);
      foreach (HotkeyAction other in allActions)
      {
        if (other == action)
        {
          // la misma accion: si ya tenia la tecla en el otro slot, se libera ese slot
          KeyCode[] same = current[other];
          int otherSlot = 1 - Mathf.Clamp(slot, 0, 1);
          if (same[otherSlot] == key) { same[otherSlot] = KeyCode.None; }
          continue;
        }

        if (!ContextsOverlap(context, ContextOf(other))) { continue; }

        KeyCode[] keys = current[other];
        for (int s = 0; s < 2; s++)
        {
          if (keys[s] == key)
          {
            keys[s] = KeyCode.None;
            displaced = other;
            hadConflict = true;
          }
        }
      }
    }

    current[action][Mathf.Clamp(slot, 0, 1)] = key;
    Save();
    RaiseChanged();
    return hadConflict;
  }

  public static void ResetAction(HotkeyAction action)
  {
    EnsureLoaded();
    // Restaurar una accion puede pisar la tecla de otra: se resuelve igual que una asignacion manual.
    KeyCode[] def = defaults[action];
    current[action] = new[] { KeyCode.None, KeyCode.None };
    TryAssign(action, 0, def[0], out _);
    if (def[1] != KeyCode.None) { TryAssign(action, 1, def[1], out _); }
  }

  public static void ResetAll()
  {
    EnsureLoaded();
    foreach (HotkeyAction a in allActions)
    {
      current[a] = new[] { defaults[a][0], defaults[a][1] };
    }
    Save();
    RaiseChanged();
  }

  public static bool IsDefault(HotkeyAction action)
  {
    EnsureLoaded();
    return current[action][0] == defaults[action][0] && current[action][1] == defaults[action][1];
  }

  // ---------------------------------------------------------------- captura de teclas (UI)

  public static void BeginCapture() { Capturing = true; }

  public static void EndCapture()
  {
    Capturing = false;
    // La tecla que confirmo la asignacion no debe disparar tambien la accion en ese mismo frame.
    captureEndFrame = Time.frameCount;
  }

  // Devuelve la primera tecla asignable que se presiono en este frame, o None.
  public static KeyCode PollPressedKey()
  {
    if (capturableKeys == null)
    {
      List<KeyCode> list = new List<KeyCode>();
      foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
      {
        if (IsAssignable(k) && !list.Contains(k)) { list.Add(k); }
      }
      capturableKeys = list.ToArray();
    }

    for (int i = 0; i < capturableKeys.Length; i++)
    {
      if (Input.GetKeyDown(capturableKeys[i])) { return capturableKeys[i]; }
    }
    return KeyCode.None;
  }

  public static bool IsAssignable(KeyCode key)
  {
    if (key == KeyCode.None || key == KeyCode.Escape) { return false; }
    if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6) { return false; }
    if (key >= KeyCode.JoystickButton0) { return false; }
    // Teclas de debug del juego y de edicion fijas.
    if (key == KeyCode.F10 || key == KeyCode.F6 || key == KeyCode.Backspace || key == KeyCode.Delete
        || key == KeyCode.Return || key == KeyCode.KeypadEnter)
    {
      return false;
    }
    return true;
  }

  public static string KeyName(KeyCode key)
  {
    switch (key)
    {
      case KeyCode.None: return "-";
      case KeyCode.Alpha0: return "0";
      case KeyCode.Alpha1: return "1";
      case KeyCode.Alpha2: return "2";
      case KeyCode.Alpha3: return "3";
      case KeyCode.Alpha4: return "4";
      case KeyCode.Alpha5: return "5";
      case KeyCode.Alpha6: return "6";
      case KeyCode.Alpha7: return "7";
      case KeyCode.Alpha8: return "8";
      case KeyCode.Alpha9: return "9";
      case KeyCode.LeftShift: return "L-Shift";
      case KeyCode.RightShift: return "R-Shift";
      case KeyCode.LeftControl: return "L-Ctrl";
      case KeyCode.RightControl: return "R-Ctrl";
      case KeyCode.LeftAlt: return "L-Alt";
      case KeyCode.RightAlt: return "R-Alt";
      case KeyCode.LeftArrow: return "Left";
      case KeyCode.RightArrow: return "Right";
      case KeyCode.UpArrow: return "Up";
      case KeyCode.DownArrow: return "Down";
      case KeyCode.Space: return "Space";
      case KeyCode.Tab: return "Tab";
      case KeyCode.BackQuote: return "`";
      case KeyCode.Minus: return "-";
      case KeyCode.Equals: return "=";
      case KeyCode.LeftBracket: return "[";
      case KeyCode.RightBracket: return "]";
      case KeyCode.Semicolon: return ";";
      case KeyCode.Quote: return "'";
      case KeyCode.Comma: return ",";
      case KeyCode.Period: return ".";
      case KeyCode.Slash: return "/";
      case KeyCode.Backslash: return "\\";
      case KeyCode.PageUp: return "PgUp";
      case KeyCode.PageDown: return "PgDn";
      case KeyCode.Insert: return "Ins";
      case KeyCode.Home: return "Home";
      case KeyCode.End: return "End";
      case KeyCode.CapsLock: return "Caps";
    }

    string name = key.ToString();
    if (name.StartsWith("Keypad", StringComparison.Ordinal)) { return "Num " + name.Substring(6); }
    return name;
  }

  // ---------------------------------------------------------------- internos

  private static bool Suppressed()
  {
    return Capturing || Time.frameCount <= captureEndFrame;
  }

  private static bool IsKeyDown(KeyCode key) { return key != KeyCode.None && Input.GetKeyDown(key); }
  private static bool IsKeyHeld(KeyCode key) { return key != KeyCode.None && Input.GetKey(key); }

  private static KeyCode[] Resolve(HotkeyAction action)
  {
    EnsureLoaded();
    HotkeyContext context = ContextOf(action);
    if ((context == HotkeyContext.Campaign && TutorialCampaign) || (context == HotkeyContext.Battle && TutorialBattle))
    {
      return defaults[action];
    }
    return current[action];
  }

  private static bool ContextsOverlap(HotkeyContext a, HotkeyContext b)
  {
    return a == b || a == HotkeyContext.General || b == HotkeyContext.General;
  }

  private static void RaiseChanged()
  {
    Action handler = Changed;
    if (handler != null) { handler(); }
  }

  private static void Def(HotkeyAction action, KeyCode primary, KeyCode secondary = KeyCode.None)
  {
    defaults[action] = new[] { primary, secondary };
    allActions.Add(action);
  }

  private static void EnsureLoaded()
  {
    if (loaded) { return; }
    loaded = true;

    Def(HotkeyAction.CampRest, KeyCode.R);
    Def(HotkeyAction.CampCharacters, KeyCode.C);
    Def(HotkeyAction.CampUpgrades, KeyCode.I);
    Def(HotkeyAction.CampFollowers, KeyCode.M);
    Def(HotkeyAction.CampQuickSave, KeyCode.F5);
    Def(HotkeyAction.CampQuickLoad, KeyCode.F9);
    for (int i = 0; i < 7; i++) { Def(HotkeyAction.CampChar1 + i, KeyCode.Alpha1 + i); }

    Def(HotkeyAction.BattleEndTurn, KeyCode.Space);
    Def(HotkeyAction.BattleTacticalView, KeyCode.Tab);
    Def(HotkeyAction.BattleEnemyInfo, KeyCode.I);
    Def(HotkeyAction.BattleFastMode, KeyCode.F);
    Def(HotkeyAction.BattlePreviewHostile, KeyCode.LeftShift, KeyCode.RightShift);
    for (int i = 0; i < 10; i++)
    {
      Def(HotkeyAction.BattleSkill1 + i, i < 9 ? KeyCode.Alpha1 + i : KeyCode.Alpha0);
    }

    Def(HotkeyAction.Handbook, KeyCode.H);
    Def(HotkeyAction.CameraLeft, KeyCode.A, KeyCode.LeftArrow);
    Def(HotkeyAction.CameraRight, KeyCode.D, KeyCode.RightArrow);
    Def(HotkeyAction.CameraUp, KeyCode.W, KeyCode.UpArrow);
    Def(HotkeyAction.CameraDown, KeyCode.S, KeyCode.DownArrow);

    foreach (HotkeyAction a in allActions)
    {
      current[a] = new[] { defaults[a][0], defaults[a][1] };
    }

    Load();
  }

  private static void Load()
  {
    string json = PlayerPrefs.GetString(PrefKey, string.Empty);
    if (string.IsNullOrEmpty(json)) { return; }

    try
    {
      Store store = JsonUtility.FromJson<Store>(json);
      if (store == null || store.e == null) { return; }

      foreach (Entry entry in store.e)
      {
        HotkeyAction action = (HotkeyAction)entry.a;
        if (!current.ContainsKey(action)) { continue; }
        current[action] = new[] { SanitizeKey(entry.k1), SanitizeKey(entry.k2) };
      }
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[Hotkeys] No se pudieron leer las teclas guardadas. Se usan las de por defecto. " + ex.Message);
    }
  }

  private static KeyCode SanitizeKey(int value)
  {
    KeyCode key = (KeyCode)value;
    return Enum.IsDefined(typeof(KeyCode), key) && (key == KeyCode.None || IsAssignable(key)) ? key : KeyCode.None;
  }

  private static void Save()
  {
    Store store = new Store();
    foreach (HotkeyAction a in allActions)
    {
      if (IsDefault(a)) { continue; }
      store.e.Add(new Entry { a = (int)a, k1 = (int)current[a][0], k2 = (int)current[a][1] });
    }

    if (store.e.Count == 0) { PlayerPrefs.DeleteKey(PrefKey); }
    else { PlayerPrefs.SetString(PrefKey, JsonUtility.ToJson(store)); }
    PlayerPrefs.Save();
  }
}
