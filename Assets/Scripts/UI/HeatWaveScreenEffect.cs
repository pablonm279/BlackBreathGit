using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HeatWaveScreenEffect : MonoBehaviour
{
  private const string OverlayName = "HeatWaveOverlay";
  private const int TextureSize = 128;

  private static readonly Color ColorTintBase = new Color(0.8f, 0.52f, 0.14f, 0.00871f);
  private static readonly Color ColorVignetteBase = new Color(0.85f, 0.32f, 0.08f, 0.022725f);
  private static readonly Color ColorGlowBase = new Color(0.8f, 0.64f, 0.34f, 0.01266f);

  private Canvas overlayCanvas;
  private CanvasGroup canvasGroup;
  private RectTransform rectTransform;
  private Image tintImage;
  private Image vignetteImage;
  private Image glowImage;

  private Texture2D solidTexture;
  private Texture2D vignetteTexture;
  private Texture2D glowTexture;
  private Sprite solidSprite;
  private Sprite vignetteSprite;
  private Sprite glowSprite;

  private bool effectActive;
  private float animationSeed;

  public static HeatWaveScreenEffect Ensure(Canvas sourceCanvas)
  {
    if (sourceCanvas == null)
    {
      return null;
    }

    Canvas rootCanvas = sourceCanvas.rootCanvas != null ? sourceCanvas.rootCanvas : sourceCanvas;
    Transform existing = rootCanvas.transform.Find(OverlayName);
    if (existing != null)
    {
      HeatWaveScreenEffect existingEffect = existing.GetComponent<HeatWaveScreenEffect>();
      if (existingEffect != null)
      {
        existingEffect.EnsureVisualTree();
        return existingEffect;
      }
    }

    GameObject overlayGO = new GameObject(OverlayName, typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(HeatWaveScreenEffect));
    RectTransform overlayRect = overlayGO.GetComponent<RectTransform>();
    overlayRect.SetParent(rootCanvas.transform, false);
    overlayRect.anchorMin = Vector2.zero;
    overlayRect.anchorMax = Vector2.one;
    overlayRect.offsetMin = Vector2.zero;
    overlayRect.offsetMax = Vector2.zero;

    HeatWaveScreenEffect effect = overlayGO.GetComponent<HeatWaveScreenEffect>();
    effect.EnsureVisualTree();
    effect.SetEffectActive(false);
    return effect;
  }

  private void Awake()
  {
    EnsureVisualTree();
    SetEffectActive(false);
  }

  private void OnDestroy()
  {
    if (solidSprite != null) { Destroy(solidSprite); }
    if (vignetteSprite != null) { Destroy(vignetteSprite); }
    if (glowSprite != null) { Destroy(glowSprite); }

    if (solidTexture != null) { Destroy(solidTexture); }
    if (vignetteTexture != null) { Destroy(vignetteTexture); }
    if (glowTexture != null) { Destroy(glowTexture); }
  }

  private void Update()
  {
    if (!effectActive)
    {
      return;
    }

    float t = Time.unscaledTime * 0.65f + animationSeed;
    float pulse = 0.5f + 0.5f * Mathf.Sin(t);
    float secondaryPulse = 0.5f + 0.5f * Mathf.Sin((t * 1.31f) + 0.8f);

    tintImage.color = WithAlpha(ColorTintBase, ColorTintBase.a + (pulse * 0.00238f));
    vignetteImage.color = WithAlpha(ColorVignetteBase, ColorVignetteBase.a + (secondaryPulse * 0.00714f));
    glowImage.color = WithAlpha(ColorGlowBase, ColorGlowBase.a + (pulse * 0.00595f));

    glowImage.rectTransform.localScale = Vector3.one * (1.03f + (pulse * 0.055f));
    glowImage.rectTransform.anchoredPosition = new Vector2(
      Mathf.Sin(t * 0.42f) * 16f,
      Mathf.Cos(t * 0.37f) * 9f);
  }

  public void SetEffectActive(bool active)
  {
    EnsureVisualTree();

    bool cambioEstado = effectActive != active;
    effectActive = active;
    if (cambioEstado && active)
    {
      animationSeed = Random.Range(0f, 100f);
    }

    canvasGroup.alpha = active ? 1f : 0f;
    enabled = active;

    if (active)
    {
      Update();
    }
  }

  private void EnsureVisualTree()
  {
    if (canvasGroup == null)
    {
      canvasGroup = GetComponent<CanvasGroup>();
      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;
    }

    if (overlayCanvas == null)
    {
      overlayCanvas = GetComponent<Canvas>();
    }

    if (overlayCanvas != null)
    {
      Canvas parentCanvas = GetComponentInParent<Canvas>();
      overlayCanvas.overrideSorting = true;
      overlayCanvas.sortingOrder = parentCanvas != null ? parentCanvas.sortingOrder - 1 : -1;
    }

    if (rectTransform == null)
    {
      rectTransform = GetComponent<RectTransform>();
      rectTransform.anchorMin = Vector2.zero;
      rectTransform.anchorMax = Vector2.one;
      rectTransform.offsetMin = Vector2.zero;
      rectTransform.offsetMax = Vector2.zero;
    }

    EnsureSprites();

    if (tintImage == null)
    {
      tintImage = CreateLayer("WarmTint", solidSprite);
      tintImage.color = ColorTintBase;
    }

    if (vignetteImage == null)
    {
      vignetteImage = CreateLayer("Vignette", vignetteSprite);
      vignetteImage.color = ColorVignetteBase;
    }

    if (glowImage == null)
    {
      glowImage = CreateLayer("Glow", glowSprite);
      glowImage.color = ColorGlowBase;
      glowImage.rectTransform.localScale = Vector3.one * 1.03f;
    }
  }

  private void EnsureSprites()
  {
    if (solidSprite == null)
    {
      solidTexture = CreateSolidTexture();
      solidSprite = CreateSprite(solidTexture);
    }

    if (vignetteSprite == null)
    {
      vignetteTexture = CreateRadialTexture(0.64f, 1f, true);
      vignetteSprite = CreateSprite(vignetteTexture);
    }

    if (glowSprite == null)
    {
      glowTexture = CreateRadialTexture(0f, 0.6f, false);
      glowSprite = CreateSprite(glowTexture);
    }
  }

  private Image CreateLayer(string layerName, Sprite sprite)
  {
    Transform existing = transform.Find(layerName);
    Image image = existing != null ? existing.GetComponent<Image>() : null;
    if (image != null)
    {
      image.sprite = sprite;
      return image;
    }

    GameObject layerGO = new GameObject(layerName, typeof(RectTransform), typeof(Image));
    layerGO.transform.SetParent(transform, false);

    RectTransform layerRect = layerGO.GetComponent<RectTransform>();
    layerRect.anchorMin = Vector2.zero;
    layerRect.anchorMax = Vector2.one;
    layerRect.offsetMin = Vector2.zero;
    layerRect.offsetMax = Vector2.zero;

    image = layerGO.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.preserveAspect = false;
    return image;
  }

  private static Texture2D CreateSolidTexture()
  {
    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;
    texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
    texture.Apply();
    return texture;
  }

  private static Texture2D CreateRadialTexture(float innerRadius, float outerRadius, bool invert)
  {
    Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;

    Color[] pixels = new Color[TextureSize * TextureSize];
    Vector2 center = new Vector2((TextureSize - 1) * 0.5f, (TextureSize - 1) * 0.5f);
    float maxRadius = TextureSize * 0.5f;

    for (int y = 0; y < TextureSize; y++)
    {
      for (int x = 0; x < TextureSize; x++)
      {
        float distance = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
        float normalized = Mathf.InverseLerp(innerRadius, outerRadius, distance);
        float alpha = invert ? normalized : 1f - normalized;
        alpha = Mathf.Clamp01(alpha);
        pixels[(y * TextureSize) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texture.SetPixels(pixels);
    texture.Apply();
    return texture;
  }

  private static Sprite CreateSprite(Texture2D texture)
  {
    return Sprite.Create(
      texture,
      new Rect(0f, 0f, texture.width, texture.height),
      new Vector2(0.5f, 0.5f),
      100f);
  }

  private static Color WithAlpha(Color color, float alpha)
  {
    color.a = Mathf.Clamp01(alpha);
    return color;
  }
}

[DisallowMultipleComponent]
public class BurningForestScreenEffect : MonoBehaviour
{
  private const string OverlayName = "BurningForestOverlay";
  private const int TextureSize = 128;

  private static readonly Color ColorTintBase = new Color(0.26f, 0.07f, 0.03f, 0.0065f);

  private Canvas overlayCanvas;
  private CanvasGroup canvasGroup;
  private RectTransform rectTransform;
  private Image tintImage;

  private Texture2D solidTexture;
  private Sprite solidSprite;

  private bool effectActive;

  public static BurningForestScreenEffect Ensure(Canvas sourceCanvas)
  {
    if (sourceCanvas == null)
    {
      return null;
    }

    Canvas rootCanvas = sourceCanvas.rootCanvas != null ? sourceCanvas.rootCanvas : sourceCanvas;
    Transform existing = rootCanvas.transform.Find(OverlayName);
    if (existing != null)
    {
      BurningForestScreenEffect existingEffect = existing.GetComponent<BurningForestScreenEffect>();
      if (existingEffect != null)
      {
        existingEffect.EnsureVisualTree();
        return existingEffect;
      }
    }

    GameObject overlayGO = new GameObject(OverlayName, typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(BurningForestScreenEffect));
    RectTransform overlayRect = overlayGO.GetComponent<RectTransform>();
    overlayRect.SetParent(rootCanvas.transform, false);
    overlayRect.anchorMin = Vector2.zero;
    overlayRect.anchorMax = Vector2.one;
    overlayRect.offsetMin = Vector2.zero;
    overlayRect.offsetMax = Vector2.zero;

    BurningForestScreenEffect effect = overlayGO.GetComponent<BurningForestScreenEffect>();
    effect.EnsureVisualTree();
    effect.SetEffectActive(false);
    return effect;
  }

  private void Awake()
  {
    EnsureVisualTree();
    SetEffectActive(false);
  }

  private void OnDestroy()
  {
    if (solidSprite != null) { Destroy(solidSprite); }

    if (solidTexture != null) { Destroy(solidTexture); }
  }

  public void SetEffectActive(bool active)
  {
    EnsureVisualTree();

    effectActive = active;

    canvasGroup.alpha = active ? 1f : 0f;
    enabled = false;
  }

  private void EnsureVisualTree()
  {
    if (canvasGroup == null)
    {
      canvasGroup = GetComponent<CanvasGroup>();
      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;
    }

    if (overlayCanvas == null)
    {
      overlayCanvas = GetComponent<Canvas>();
    }

    if (overlayCanvas != null)
    {
      Canvas parentCanvas = GetComponentInParent<Canvas>();
      overlayCanvas.overrideSorting = true;
      overlayCanvas.sortingOrder = parentCanvas != null ? parentCanvas.sortingOrder - 2 : -2;
    }

    if (rectTransform == null)
    {
      rectTransform = GetComponent<RectTransform>();
      rectTransform.anchorMin = Vector2.zero;
      rectTransform.anchorMax = Vector2.one;
      rectTransform.offsetMin = Vector2.zero;
      rectTransform.offsetMax = Vector2.zero;
    }

    EnsureSprites();
    RemoveLegacyVignetteLayer();

    if (tintImage == null)
    {
      tintImage = CreateLayer("WarmTint", solidSprite);
      tintImage.color = ColorTintBase;
    }
  }

  private void EnsureSprites()
  {
    if (solidSprite == null)
    {
      solidTexture = CreateSolidTexture();
      solidSprite = CreateSprite(solidTexture);
    }
  }

  private void RemoveLegacyVignetteLayer()
  {
    Transform existingVignette = transform.Find("Vignette");
    if (existingVignette != null)
    {
      Destroy(existingVignette.gameObject);
    }
  }

  private Image CreateLayer(string layerName, Sprite sprite)
  {
    Transform existing = transform.Find(layerName);
    Image image = existing != null ? existing.GetComponent<Image>() : null;
    if (image != null)
    {
      image.sprite = sprite;
      return image;
    }

    GameObject layerGO = new GameObject(layerName, typeof(RectTransform), typeof(Image));
    layerGO.transform.SetParent(transform, false);

    RectTransform layerRect = layerGO.GetComponent<RectTransform>();
    layerRect.anchorMin = Vector2.zero;
    layerRect.anchorMax = Vector2.one;
    layerRect.offsetMin = Vector2.zero;
    layerRect.offsetMax = Vector2.zero;

    image = layerGO.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.preserveAspect = false;
    return image;
  }

  private static Texture2D CreateSolidTexture()
  {
    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;
    texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
    texture.Apply();
    return texture;
  }

  private static Texture2D CreateRadialTexture(float innerRadius, float outerRadius, bool invert)
  {
    Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;

    Color[] pixels = new Color[TextureSize * TextureSize];
    Vector2 center = new Vector2((TextureSize - 1) * 0.5f, (TextureSize - 1) * 0.5f);
    float maxRadius = TextureSize * 0.5f;

    for (int y = 0; y < TextureSize; y++)
    {
      for (int x = 0; x < TextureSize; x++)
      {
        float distance = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
        float normalized = Mathf.InverseLerp(innerRadius, outerRadius, distance);
        float alpha = invert ? normalized : 1f - normalized;
        alpha = Mathf.Clamp01(alpha);
        pixels[(y * TextureSize) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texture.SetPixels(pixels);
    texture.Apply();
    return texture;
  }

  private static Sprite CreateSprite(Texture2D texture)
  {
    return Sprite.Create(
      texture,
      new Rect(0f, 0f, texture.width, texture.height),
      new Vector2(0.5f, 0.5f),
      100f);
  }

  private static Color WithAlpha(Color color, float alpha)
  {
    color.a = Mathf.Clamp01(alpha);
    return color;
  }
}

[DisallowMultipleComponent]
public class BlackBreathInsideScreenEffect : MonoBehaviour
{
  private const string OverlayName = "BlackBreathInsideOverlay";
  private const int TextureSize = 128;
  private const float FadeDuration = 0.24f;

  // Cian/verde sutil para inmersion cuando la caravana esta dentro del aliento.
  private static readonly Color ColorVignettePrimaryBase = new Color(0.11f, 0.62f, 0.55f, 0.0045f);
  private static readonly Color ColorVignetteSecondaryBase = new Color(0.18f, 0.7f, 0.58f, 0.003f);

  private Canvas overlayCanvas;
  private CanvasGroup canvasGroup;
  private RectTransform rectTransform;
  private Image vignettePrimaryImage;
  private Image vignetteSecondaryImage;

  private Texture2D vignettePrimaryTexture;
  private Texture2D vignetteSecondaryTexture;
  private Sprite vignettePrimarySprite;
  private Sprite vignetteSecondarySprite;

  private bool effectActive;
  private float animationSeed;
  private float targetAlpha;

  public static BlackBreathInsideScreenEffect Ensure(Canvas sourceCanvas)
  {
    if (sourceCanvas == null)
    {
      return null;
    }

    Canvas rootCanvas = sourceCanvas.rootCanvas != null ? sourceCanvas.rootCanvas : sourceCanvas;
    Transform existing = rootCanvas.transform.Find(OverlayName);
    if (existing != null)
    {
      BlackBreathInsideScreenEffect existingEffect = existing.GetComponent<BlackBreathInsideScreenEffect>();
      if (existingEffect != null)
      {
        existingEffect.EnsureVisualTree();
        return existingEffect;
      }
    }

    GameObject overlayGO = new GameObject(OverlayName, typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(BlackBreathInsideScreenEffect));
    RectTransform overlayRect = overlayGO.GetComponent<RectTransform>();
    overlayRect.SetParent(rootCanvas.transform, false);
    overlayRect.anchorMin = Vector2.zero;
    overlayRect.anchorMax = Vector2.one;
    overlayRect.offsetMin = Vector2.zero;
    overlayRect.offsetMax = Vector2.zero;

    BlackBreathInsideScreenEffect effect = overlayGO.GetComponent<BlackBreathInsideScreenEffect>();
    effect.EnsureVisualTree();
    effect.SetEffectActive(false);
    return effect;
  }

  private void Awake()
  {
    EnsureVisualTree();
    SetEffectActive(false);
  }

  private void OnDestroy()
  {
    if (vignettePrimarySprite != null) { Destroy(vignettePrimarySprite); }
    if (vignetteSecondarySprite != null) { Destroy(vignetteSecondarySprite); }

    if (vignettePrimaryTexture != null) { Destroy(vignettePrimaryTexture); }
    if (vignetteSecondaryTexture != null) { Destroy(vignetteSecondaryTexture); }
  }

  private void Update()
  {
    float alphaActual = canvasGroup != null ? canvasGroup.alpha : 0f;
    float velocidadFade = FadeDuration > 0f ? (1f / FadeDuration) : 999f;
    float alphaNuevo = Mathf.MoveTowards(alphaActual, targetAlpha, Time.unscaledDeltaTime * velocidadFade);

    if (canvasGroup != null)
    {
      canvasGroup.alpha = alphaNuevo;
    }

    if (!effectActive)
    {
      if (Mathf.Approximately(alphaNuevo, targetAlpha))
      {
        enabled = false;
      }

      return;
    }

    float t = Time.unscaledTime * 0.52f + animationSeed;
    float pulseA = 0.5f + 0.5f * Mathf.Sin(t);
    float pulseB = 0.5f + 0.5f * Mathf.Sin((t * 1.21f) + 1.1f);

    vignettePrimaryImage.color = WithAlpha(ColorVignettePrimaryBase, ColorVignettePrimaryBase.a + (pulseA * 0.00175f));
    vignetteSecondaryImage.color = WithAlpha(ColorVignetteSecondaryBase, ColorVignetteSecondaryBase.a + (pulseB * 0.0014f));

    vignetteSecondaryImage.rectTransform.localScale = Vector3.one * (1.005f + (pulseA * 0.0125f));
    vignetteSecondaryImage.rectTransform.anchoredPosition = new Vector2(
      Mathf.Sin(t * 0.37f) * 5f,
      Mathf.Cos(t * 0.33f) * 3.5f);
  }

  public void SetEffectActive(bool active)
  {
    EnsureVisualTree();

    bool cambioEstado = effectActive != active;
    effectActive = active;
    if (cambioEstado && active)
    {
      animationSeed = Random.Range(0f, 100f);
    }

    targetAlpha = active ? 1f : 0f;
    enabled = true;

    if (active)
    {
      Update();
    }
  }

  private void EnsureVisualTree()
  {
    if (canvasGroup == null)
    {
      canvasGroup = GetComponent<CanvasGroup>();
      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;
    }

    if (overlayCanvas == null)
    {
      overlayCanvas = GetComponent<Canvas>();
    }

    if (overlayCanvas != null)
    {
      Canvas parentCanvas = GetComponentInParent<Canvas>();
      overlayCanvas.overrideSorting = true;
      overlayCanvas.sortingOrder = parentCanvas != null ? parentCanvas.sortingOrder - 3 : -3;
    }

    if (rectTransform == null)
    {
      rectTransform = GetComponent<RectTransform>();
      rectTransform.anchorMin = Vector2.zero;
      rectTransform.anchorMax = Vector2.one;
      rectTransform.offsetMin = Vector2.zero;
      rectTransform.offsetMax = Vector2.zero;
    }

    EnsureSprites();

    if (vignettePrimaryImage == null)
    {
      vignettePrimaryImage = CreateLayer("VignettePrimary", vignettePrimarySprite);
      vignettePrimaryImage.color = ColorVignettePrimaryBase;
    }

    if (vignetteSecondaryImage == null)
    {
      vignetteSecondaryImage = CreateLayer("VignetteSecondary", vignetteSecondarySprite);
      vignetteSecondaryImage.color = ColorVignetteSecondaryBase;
      vignetteSecondaryImage.rectTransform.localScale = Vector3.one * 1.005f;
    }
  }

  private void EnsureSprites()
  {
    if (vignettePrimarySprite == null)
    {
      // Anillo externo: deja el centro limpio.
      vignettePrimaryTexture = CreateRadialTexture(0.8f, 1f, true);
      vignettePrimarySprite = CreateSprite(vignettePrimaryTexture);
    }

    if (vignetteSecondarySprite == null)
    {
      // Segunda capa suave para dar vida sin tapar el centro.
      vignetteSecondaryTexture = CreateRadialTexture(0.74f, 1f, true);
      vignetteSecondarySprite = CreateSprite(vignetteSecondaryTexture);
    }
  }

  private Image CreateLayer(string layerName, Sprite sprite)
  {
    Transform existing = transform.Find(layerName);
    Image image = existing != null ? existing.GetComponent<Image>() : null;
    if (image != null)
    {
      image.sprite = sprite;
      return image;
    }

    GameObject layerGO = new GameObject(layerName, typeof(RectTransform), typeof(Image));
    layerGO.transform.SetParent(transform, false);

    RectTransform layerRect = layerGO.GetComponent<RectTransform>();
    layerRect.anchorMin = Vector2.zero;
    layerRect.anchorMax = Vector2.one;
    layerRect.offsetMin = Vector2.zero;
    layerRect.offsetMax = Vector2.zero;

    image = layerGO.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.preserveAspect = false;
    return image;
  }

  private static Texture2D CreateRadialTexture(float innerRadius, float outerRadius, bool invert)
  {
    Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;

    Color[] pixels = new Color[TextureSize * TextureSize];
    Vector2 center = new Vector2((TextureSize - 1) * 0.5f, (TextureSize - 1) * 0.5f);
    float maxRadius = TextureSize * 0.5f;

    for (int y = 0; y < TextureSize; y++)
    {
      for (int x = 0; x < TextureSize; x++)
      {
        float distance = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
        float normalized = Mathf.InverseLerp(innerRadius, outerRadius, distance);
        float alpha = invert ? normalized : 1f - normalized;
        alpha = Mathf.Clamp01(alpha);
        pixels[(y * TextureSize) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texture.SetPixels(pixels);
    texture.Apply();
    return texture;
  }

  private static Sprite CreateSprite(Texture2D texture)
  {
    return Sprite.Create(
      texture,
      new Rect(0f, 0f, texture.width, texture.height),
      new Vector2(0.5f, 0.5f),
      100f);
  }

  private static Color WithAlpha(Color color, float alpha)
  {
    color.a = Mathf.Clamp01(alpha);
    return color;
  }
}
