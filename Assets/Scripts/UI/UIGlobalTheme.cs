using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class UIGlobalTheme : MonoBehaviour
{
/*  private static UIGlobalTheme instance;

  [Header("Fonts (Resources paths)")]
  [SerializeField] private string headingFontPath = "Fuentes/Cinzel/CinzelDecorative-Regular SDF";
  [SerializeField] private string bodyFontPath = "Fuentes/Cardo/Cardo-Regular SDF";

  [Header("Typography")]
  [SerializeField] private float headingMinCharacterSpacing = 1.0f;
  [SerializeField] private float bodyMinCharacterSpacing = 0.15f;
  [SerializeField] private float bodyMinLineSpacing = 1.05f;

  [Header("Colors")]
  [SerializeField] private bool normalizeNeutralColors = true;
  [SerializeField] private Color headingColor = new Color32(236, 227, 206, 255);
  [SerializeField] private Color bodyColor = new Color32(220, 215, 204, 255);

  [Header("Behavior")]
  [SerializeField] private float periodicScanSeconds = 0.9f;

  [Header("Buttons")]
  [SerializeField] private bool polishButtons = true;
  [SerializeField] [Range(0f, 0.2f)] private float buttonHoverLift = 0.08f;
  [SerializeField] [Range(0f, 0.2f)] private float buttonPressedDepth = 0.08f;
  [SerializeField] [Range(0f, 1f)] private float buttonDisabledDesaturation = 0.55f;
  [SerializeField] [Range(0.02f, 0.3f)] private float buttonFadeDuration = 0.08f;

  [Header("Panels")]
  [SerializeField] private bool polishPanels = true;
  [SerializeField] private Vector2 panelMinSize = new Vector2(180f, 80f);
  [SerializeField] [Range(0f, 0.5f)] private float panelTintStrength = 0.22f;
  [SerializeField] private Color primaryPanelTint = new Color32(80, 73, 60, 255);
  [SerializeField] private Color nestedPanelTint = new Color32(72, 66, 55, 255);
  [SerializeField] private Color rootPanelShadow = new Color(0f, 0f, 0f, 0.28f);
  [SerializeField] private Color nestedPanelShadow = new Color(0f, 0f, 0f, 0.18f);
  [SerializeField] private Color rootPanelOutline = new Color(1f, 1f, 1f, 0.05f);

  private TMP_FontAsset headingFont;
  private TMP_FontAsset bodyFont;
  private readonly HashSet<int> processed = new HashSet<int>();
  private readonly HashSet<int> processedButtons = new HashSet<int>();
  private readonly HashSet<int> processedPanels = new HashSet<int>();

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Boot()
  {
    if (instance != null) { return; }

    GameObject go = new GameObject("UIGlobalTheme");
    instance = go.AddComponent<UIGlobalTheme>();
    DontDestroyOnLoad(go);
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

    LoadFonts();
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
    ApplyThemeToOpenScene();
    StartCoroutine(PeriodicThemePass());
  }

  private void OnSceneLoaded(Scene _, LoadSceneMode __)
  {
    processed.Clear();
    processedButtons.Clear();
    processedPanels.Clear();
    StartCoroutine(ApplyAfterOneFrame());
  }

  private IEnumerator ApplyAfterOneFrame()
  {
    yield return null;
    ApplyThemeToOpenScene();
  }

  private IEnumerator PeriodicThemePass()
  {
    while (true)
    {
      ApplyThemeToOpenScene();
      yield return new WaitForSeconds(periodicScanSeconds);
    }
  }

  private void LoadFonts()
  {
    headingFont = Resources.Load<TMP_FontAsset>(headingFontPath);
    bodyFont = Resources.Load<TMP_FontAsset>(bodyFontPath);

    if (headingFont == null || bodyFont == null)
    {
      Debug.LogWarning("[UIGlobalTheme] Could not load one or more fonts from Resources.");
      return;
    }

    TMP_Settings.defaultFontAsset = bodyFont;
  }

  private void ApplyThemeToOpenScene()
  {
    if (headingFont != null && bodyFont != null)
    {
      TextMeshProUGUI[] labels = FindObjectsOfType<TextMeshProUGUI>(true);
      for (int i = 0; i < labels.Length; i++)
      {
        TextMeshProUGUI label = labels[i];
        if (!ShouldProcess(label)) { continue; }

        bool isHeading = IsHeadingLabel(label);
        label.font = isHeading ? headingFont : bodyFont;
        if (label.font != null && label.font.material != null)
        {
          label.fontSharedMaterial = label.font.material;
        }

        if (isHeading)
        {
          label.characterSpacing = Mathf.Max(label.characterSpacing, headingMinCharacterSpacing);
        }
        else
        {
          label.characterSpacing = Mathf.Max(label.characterSpacing, bodyMinCharacterSpacing);
          label.lineSpacing = Mathf.Max(label.lineSpacing, bodyMinLineSpacing);
        }

        if (normalizeNeutralColors && IsNeutral(label.color))
        {
          label.color = isHeading ? headingColor : bodyColor;
        }

        processed.Add(label.GetInstanceID());
      }
    }

    ApplyButtonPolish();
    ApplyPanelPolish();
  }

  private bool ShouldProcess(TextMeshProUGUI label)
  {
    if (label == null) { return false; }
    if (!label.gameObject.scene.IsValid()) { return false; }
    if (processed.Contains(label.GetInstanceID())) { return false; }
    if (IsFloatingTextLabel(label)) { return false; }
    if (IsHandbookOrLogLabel(label)) { return false; }
    if (label.GetComponentInParent<btnPersonaje>(true) != null) { return false; }
    if (label.GetComponentInParent<BotonHabilidad>(true) != null) { return false; }

    string hierarchyPath = GetHierarchyPathLower(label.transform);
    if (hierarchyPath.Contains("btnpersonaje") || hierarchyPath.Contains("botonpersonaje") ||
        hierarchyPath.Contains("botonhabilidad"))
    {
      return false;
    }

    return true;
  }

  private static bool IsFloatingTextLabel(TextMeshProUGUI label)
  {
    if (label == null) { return false; }

    if (label.GetComponentInParent<FloatingTextAnimator>(true) != null) { return true; }
    if (label.GetComponentInParent<TextoFlotanteManager>(true) != null) { return true; }
    if (label.GetComponentInParent<AutodestruirDelay>(true) != null) { return true; }
    if (HasTagInHierarchy(label.transform, "txtFlotante")) { return true; }

    string nodeName = label.gameObject.name.ToLowerInvariant();
    if (nodeName.Contains("floating") || nodeName.Contains("flotante") || nodeName.Contains("flota") ||
        nodeName.Contains("dano") || nodeName.Contains("da\u00f1o") || nodeName.Contains("txtda"))
    {
      return true;
    }

    string hierarchyPath = GetHierarchyPathLower(label.transform);
    if (hierarchyPath.Contains("floating") || hierarchyPath.Contains("flotante") ||
        hierarchyPath.Contains("txtflota") || hierarchyPath.Contains("txtda") ||
        hierarchyPath.Contains("puntoorigenalertas"))
    {
      return true;
    }

    return false;
  }

  private static bool IsHandbookOrLogLabel(TextMeshProUGUI label)
  {
    if (label == null) { return false; }

    if (label.GetComponentInParent<HandbookManager>(true) != null) { return true; }
    if (label.GetComponentInParent<LogDeCampania>(true) != null) { return true; }

    string nodeName = label.gameObject.name.ToLowerInvariant();
    if (nodeName == "log" || nodeName.Contains("txtlog") || nodeName.Contains("handbook"))
    {
      return true;
    }

    string hierarchyPath = GetHierarchyPathLower(label.transform);
    if (hierarchyPath.Contains("handbook") ||
        hierarchyPath.Contains("canvaslog") ||
        hierarchyPath.Contains("/log/") ||
        hierarchyPath.EndsWith("/log"))
    {
      return true;
    }

    return false;
  }

  private void ApplyButtonPolish()
  {
    if (!polishButtons) { return; }

    Button[] buttons = FindObjectsOfType<Button>(true);
    for (int i = 0; i < buttons.Length; i++)
    {
      Button button = buttons[i];
      if (!ShouldProcessButton(button)) { continue; }

      ColorBlock colors = button.colors;
      Color normal = colors.normalColor;
      if (normal.a <= 0.001f) { normal = Color.white; }

      colors.colorMultiplier = 1f;
      colors.fadeDuration = buttonFadeDuration;
      colors.highlightedColor = LiftColor(normal, buttonHoverLift);
      colors.selectedColor = LiftColor(normal, Mathf.Max(0.03f, buttonHoverLift * 0.65f));
      colors.pressedColor = DarkenColor(normal, buttonPressedDepth);

      Color disabled = DesaturateColor(normal, buttonDisabledDesaturation);
      disabled.a = Mathf.Clamp01(normal.a * 0.65f);
      colors.disabledColor = disabled;

      button.colors = colors;
      processedButtons.Add(button.GetInstanceID());
    }
  }

  private bool ShouldProcessButton(Button button)
  {
    if (button == null) { return false; }
    if (!button.gameObject.scene.IsValid()) { return false; }
    if (processedButtons.Contains(button.GetInstanceID())) { return false; }
    if (button.transition != Selectable.Transition.ColorTint) { return false; }

    Canvas canvas = button.GetComponentInParent<Canvas>(true);
    if (canvas != null && canvas.renderMode == RenderMode.WorldSpace) { return false; }

    if (button.GetComponentInParent<btnPersonaje>(true) != null) { return false; }
    if (button.GetComponentInParent<BotonHabilidad>(true) != null) { return false; }

    string path = GetHierarchyPathLower(button.transform);
    if (path.Contains("btnpersonaje") || path.Contains("botonpersonaje") ||
        path.Contains("botonhabilidad"))
    {
      return false;
    }

    return true;
  }

  private void ApplyPanelPolish()
  {
    if (!polishPanels) { return; }

    Image[] images = FindObjectsOfType<Image>(true);
    for (int i = 0; i < images.Length; i++)
    {
      Image image = images[i];
      if (!ShouldProcessPanel(image)) { continue; }

      bool hasPanelAncestor = HasPanelAncestor(image.transform);

      if (IsNeutral(image.color))
      {
        Color tint = hasPanelAncestor ? nestedPanelTint : primaryPanelTint;
        tint.a = image.color.a;
        image.color = Color.Lerp(image.color, tint, panelTintStrength);
      }

      Shadow shadow = image.GetComponent<Shadow>();
      if (shadow == null)
      {
        shadow = image.gameObject.AddComponent<Shadow>();
      }
      shadow.effectColor = hasPanelAncestor ? nestedPanelShadow : rootPanelShadow;
      shadow.effectDistance = hasPanelAncestor ? new Vector2(0f, -1.5f) : new Vector2(0f, -2.5f);
      shadow.useGraphicAlpha = true;

      if (!hasPanelAncestor)
      {
        Outline outline = image.GetComponent<Outline>();
        if (outline == null)
        {
          outline = image.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = rootPanelOutline;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;
      }

      processedPanels.Add(image.GetInstanceID());
    }
  }

  private bool ShouldProcessPanel(Image image)
  {
    if (image == null) { return false; }
    if (!image.gameObject.scene.IsValid()) { return false; }
    if (!image.enabled) { return false; }
    if (processedPanels.Contains(image.GetInstanceID())) { return false; }

    Canvas canvas = image.GetComponentInParent<Canvas>(true);
    if (canvas == null) { return false; }
    if (canvas.renderMode == RenderMode.WorldSpace) { return false; }

    if (image.GetComponentInParent<BotonHabilidad>(true) != null) { return false; }
    if (image.GetComponentInParent<Selectable>(true) != null) { return false; }

    RectTransform rt = image.rectTransform;
    if (rt == null) { return false; }
    if (rt.rect.width < panelMinSize.x || rt.rect.height < panelMinSize.y) { return false; }
    if (IsLikelyFullscreenBackground(rt)) { return false; }

    if (!LooksLikePanel(image)) { return false; }

    string path = GetHierarchyPathLower(image.transform);
    if (path.Contains("icon") || path.Contains("portrait") || path.Contains("retrato") ||
        path.Contains("avatar") || path.Contains("botonhabilidad"))
    {
      return false;
    }

    return true;
  }

  private bool LooksLikePanel(Image image)
  {
    string name = image.gameObject.name.ToLowerInvariant();
    bool panelName =
      name.Contains("panel") ||
      name.Contains("window") ||
      name.Contains("popup") ||
      name.Contains("cuadro") ||
      name.Contains("marco") ||
      name.Contains("contenedor") ||
      name.Contains("container") ||
      name.Contains("fondo") ||
      name.Contains("background") ||
      name.Contains("tooltip");

    bool hasLayout = image.GetComponent<LayoutGroup>() != null;
    bool hasMask = image.GetComponent<Mask>() != null || image.GetComponent<RectMask2D>() != null;
    bool hasScroll = image.GetComponent<ScrollRect>() != null;
    bool hasChildren = image.transform.childCount >= 2;

    return panelName || hasLayout || hasMask || hasScroll || hasChildren;
  }

  private bool HasPanelAncestor(Transform t)
  {
    if (t == null) { return false; }

    Transform current = t.parent;
    while (current != null)
    {
      Image parentImage = current.GetComponent<Image>();
      if (parentImage != null)
      {
        RectTransform parentRt = parentImage.rectTransform;
        if (parentRt != null &&
            parentRt.rect.width >= panelMinSize.x &&
            parentRt.rect.height >= panelMinSize.y &&
            LooksLikePanel(parentImage) &&
            parentImage.GetComponentInParent<Selectable>(true) == null)
        {
          return true;
        }
      }

      current = current.parent;
    }

    return false;
  }

  private static bool IsLikelyFullscreenBackground(RectTransform rt)
  {
    if (rt == null) { return false; }

    if (rt.anchorMin != Vector2.zero || rt.anchorMax != Vector2.one)
    {
      return false;
    }

    bool nearZeroOffsets =
      Mathf.Abs(rt.offsetMin.x) <= 2f &&
      Mathf.Abs(rt.offsetMin.y) <= 2f &&
      Mathf.Abs(rt.offsetMax.x) <= 2f &&
      Mathf.Abs(rt.offsetMax.y) <= 2f;

    return nearZeroOffsets;
  }

  private static Color LiftColor(Color c, float amount)
  {
    Color lifted = Color.Lerp(c, Color.white, Mathf.Clamp01(amount));
    lifted.a = c.a;
    return lifted;
  }

  private static Color DarkenColor(Color c, float amount)
  {
    Color darkened = Color.Lerp(c, Color.black, Mathf.Clamp01(amount));
    darkened.a = c.a;
    return darkened;
  }

  private static Color DesaturateColor(Color c, float amount)
  {
    float g = c.grayscale;
    Color gray = new Color(g, g, g, c.a);
    return Color.Lerp(c, gray, Mathf.Clamp01(amount));
  }

  private static string GetHierarchyPathLower(Transform t)
  {
    if (t == null) { return string.Empty; }

    System.Text.StringBuilder sb = new System.Text.StringBuilder(96);
    Transform current = t;
    while (current != null)
    {
      if (sb.Length > 0) { sb.Insert(0, '/'); }
      sb.Insert(0, current.name);
      current = current.parent;
    }
    return sb.ToString().ToLowerInvariant();
  }

  private static bool HasTagInHierarchy(Transform t, string expectedTag)
  {
    if (t == null || string.IsNullOrEmpty(expectedTag)) { return false; }

    Transform current = t;
    while (current != null)
    {
      if (current.gameObject.tag == expectedTag)
      {
        return true;
      }

      current = current.parent;
    }

    return false;
  }

  private static bool IsHeadingLabel(TextMeshProUGUI label)
  {
    string name = label.gameObject.name.ToLowerInvariant();

    if (name.Contains("titulo") || name.Contains("title") || name.Contains("header") ||
        name.Contains("ronda") || name.Contains("turno") || name.Contains("nombre"))
    {
      return true;
    }

    if (label.fontSize >= 34f) { return true; }
    return false;
  }

  private static bool IsNeutral(Color color)
  {
    if (color.a < 0.95f) { return false; }

    float rg = Mathf.Abs(color.r - color.g);
    float gb = Mathf.Abs(color.g - color.b);
    return rg < 0.08f && gb < 0.08f && color.grayscale > 0.35f;
  }
  */
}
