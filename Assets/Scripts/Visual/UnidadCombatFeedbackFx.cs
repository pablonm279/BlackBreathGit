using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnidadCombatFeedbackFx : MonoBehaviour
{
  private const string OverlayRootName = "CombatFeedbackFx";
  private const float EscalaImpacto = 0.9f;

  [Header("Impacto")]
  [SerializeField] private float duracionImpacto = 0.18f;
  [SerializeField] private float duracionImpactoCritico = 0.24f;
  [SerializeField] private float duracionMissGlint = 0.14f;

  private Unidad unidad;
  private RectTransform imagenUnidad;
  private RectTransform overlayRoot;
  private CanvasGroup overlayGroup;
  private Image impactoAnillo;
  private Image impactoDestello;
  private Image missGlint;

  private float tiempoImpactoRestante = -1f;
  private float tiempoMissGlintRestante = -1f;
  private float duracionImpactoActual;
  private Color colorImpacto = Color.white;
  private bool impactoCritico;
  private bool impactoGuardia;
  private int tipoImpacto;

  private static Texture2D texturaSuave;
  private static Sprite spriteSuave;
  private static Texture2D texturaAnillo;
  private static Sprite spriteAnillo;
  private static Texture2D texturaGlint;
  private static Sprite spriteGlint;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
  }

  private void LateUpdate()
  {
    if (!VincularImagenUnidad())
    {
      OcultarOverlay();
      return;
    }

    AsegurarOverlay();

    bool mostrar = unidad != null && unidad.gameObject.activeInHierarchy && unidad.HP_actual > 0f;
    if (!mostrar)
    {
      OcultarOverlay();
      return;
    }

    if (!overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(true);
    }

    ActualizarOverlayBase();
    ActualizarImpacto();
    ActualizarMissGlint();
  }

  private void OnDisable()
  {
    tiempoImpactoRestante = -1f;
    tiempoMissGlintRestante = -1f;
    OcultarOverlay();
  }

  private void OnDestroy()
  {
    if (overlayRoot != null)
    {
      Destroy(overlayRoot.gameObject);
    }
  }

  public void PlayDamageImpact(Color color, bool critico, int tipoDanio)
  {
    colorImpacto = AjustarColorImpacto(color, tipoDanio, false);
    impactoCritico = critico;
    impactoGuardia = false;
    tipoImpacto = tipoDanio;
    duracionImpactoActual = critico ? duracionImpactoCritico : duracionImpacto;
    tiempoImpactoRestante = duracionImpactoActual;

    if (critico)
    {
      ScreenFlash.FlashImpact(colorImpacto, 0.07f, 0.01f, 0.09f, 0f);
    }
    else if (tipoDanio >= 4)
    {
      ScreenFlash.FlashImpact(colorImpacto, 0.02f, 0.008f, 0.06f, 0f);
    }
  }

  public void PlayGuardImpact(Color color)
  {
    colorImpacto = AjustarColorImpacto(color, 0, true);
    impactoCritico = false;
    impactoGuardia = true;
    tipoImpacto = 0;
    duracionImpactoActual = duracionImpacto * 0.85f;
    tiempoImpactoRestante = duracionImpactoActual;
    ScreenFlash.FlashImpact(colorImpacto, 0.012f, 0.008f, 0.05f, 0f);
  }

  public void PlayMissGlint()
  {
    tiempoMissGlintRestante = Mathf.Max(0.01f, duracionMissGlint);
  }

  private bool VincularImagenUnidad()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
      if (unidad == null || unidad.uImage == null)
      {
        return false;
      }
    }

    if (unidad.uImage == null)
    {
      return false;
    }

    RectTransform rect = unidad.uImage.rectTransform;
    if (rect == null || rect.parent == null)
    {
      return false;
    }

    if (imagenUnidad != rect)
    {
      imagenUnidad = rect;
      if (overlayRoot != null && overlayRoot.parent != imagenUnidad.parent)
      {
        Destroy(overlayRoot.gameObject);
        overlayRoot = null;
        overlayGroup = null;
        impactoAnillo = null;
        impactoDestello = null;
        missGlint = null;
      }
    }

    return imagenUnidad != null;
  }

  private void AsegurarOverlay()
  {
    if (imagenUnidad == null || overlayRoot != null)
    {
      return;
    }

    AsegurarSprites();

    GameObject rootGo = new GameObject(OverlayRootName, typeof(RectTransform), typeof(CanvasGroup));
    overlayRoot = rootGo.GetComponent<RectTransform>();
    overlayGroup = rootGo.GetComponent<CanvasGroup>();
    overlayRoot.SetParent(imagenUnidad.parent, false);
    overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayGroup.interactable = false;
    overlayGroup.blocksRaycasts = false;
    overlayGroup.alpha = 1f;

    impactoAnillo = CrearImagen("ImpactAnillo", overlayRoot, spriteAnillo);
    impactoDestello = CrearImagen("ImpactDestello", overlayRoot, spriteSuave);
    missGlint = CrearImagen("MissGlint", overlayRoot, spriteGlint);

    overlayRoot.gameObject.SetActive(false);
  }

  private void ActualizarOverlayBase()
  {
    if (overlayRoot == null || imagenUnidad == null)
    {
      return;
    }

    Vector2 tamano = ObtenerTamanoUnidad();
    overlayRoot.anchoredPosition = ObtenerPosicionImpactoLocal();
    overlayRoot.localScale = imagenUnidad.localScale;
    overlayRoot.localEulerAngles = Vector3.zero;
    overlayRoot.sizeDelta = new Vector2(tamano.x * 0.6656f * EscalaImpacto, tamano.y * 0.7176f * EscalaImpacto);

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Clamp(sibling + 1, 0, imagenUnidad.parent.childCount - 1);
    if (overlayRoot.GetSiblingIndex() != targetSibling)
    {
      overlayRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void ActualizarImpacto()
  {
    if (impactoAnillo == null || impactoDestello == null)
    {
      return;
    }

    if (tiempoImpactoRestante <= 0f)
    {
      impactoAnillo.color = WithAlpha(impactoAnillo.color, 0f);
      impactoDestello.color = WithAlpha(impactoDestello.color, 0f);
      return;
    }

    tiempoImpactoRestante = Mathf.Max(0f, tiempoImpactoRestante - Time.unscaledDeltaTime);
    float duracion = Mathf.Max(0.01f, duracionImpactoActual);
    float n = 1f - (tiempoImpactoRestante / duracion);
    float fade = 1f - Mathf.SmoothStep(0f, 1f, n);
    Vector2 tamano = ObtenerTamanoUnidad();

    float anchoBase = tamano.x * (impactoGuardia ? 0.3536f : (impactoCritico ? 0.468f : 0.4056f)) * EscalaImpacto;
    float altoBase = tamano.y * (impactoGuardia ? 0.2808f : (impactoCritico ? 0.4368f : 0.3536f)) * EscalaImpacto;
    float expansion = impactoGuardia ? 0.08f : (impactoCritico ? 0.14f : 0.1f);
    float giro = impactoGuardia ? 0f : (tipoImpacto <= 3 ? -10f : 0f);

    ConfigurarImagen(
      impactoAnillo,
      Vector2.zero,
      new Vector2(anchoBase * (1f + (n * expansion)), altoBase * (1f + (n * expansion))),
      WithAlpha(colorImpacto, fade * (impactoGuardia ? 0.102f : (impactoCritico ? 0.153f : 0.095625f))));
    impactoAnillo.rectTransform.localEulerAngles = new Vector3(0f, 0f, giro);

    ConfigurarImagen(
      impactoDestello,
      Vector2.zero,
      new Vector2(tamano.x * (impactoGuardia ? 0.26f : (impactoCritico ? 0.3536f : 0.3016f)) * EscalaImpacto, tamano.y * (impactoGuardia ? 0.2288f : (impactoCritico ? 0.3328f : 0.2704f)) * EscalaImpacto),
      WithAlpha(Color.Lerp(colorImpacto, Color.white, impactoCritico ? 0.28f : 0.14f), fade * (impactoGuardia ? 0.11475f : (impactoCritico ? 0.19125f : 0.1275f))));
  }

  private void ActualizarMissGlint()
  {
    if (missGlint == null)
    {
      return;
    }

    if (tiempoMissGlintRestante <= 0f)
    {
      missGlint.color = WithAlpha(missGlint.color, 0f);
      return;
    }

    tiempoMissGlintRestante = Mathf.Max(0f, tiempoMissGlintRestante - Time.unscaledDeltaTime);
    float duracion = Mathf.Max(0.01f, duracionMissGlint);
    float n = 1f - (tiempoMissGlintRestante / duracion);
    float fade = 1f - Mathf.SmoothStep(0f, 1f, n);
    Vector2 tamano = ObtenerTamanoUnidad();

    ConfigurarImagen(
      missGlint,
      Vector2.zero,
      new Vector2(tamano.x * 0.24f, tamano.y * 0.4f),
      WithAlpha(new Color(0.97f, 0.98f, 1f, 1f), fade * 0.34f));
    missGlint.rectTransform.localEulerAngles = new Vector3(0f, 0f, -32f);
  }

  private void OcultarOverlay()
  {
    if (overlayRoot != null && overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(false);
    }
  }

  private Vector2 ObtenerTamanoUnidad()
  {
    if (imagenUnidad == null)
    {
      return new Vector2(72f, 96f);
    }

    Vector2 tamano = imagenUnidad.rect.size;
    if (tamano.x < 1f || tamano.y < 1f)
    {
      tamano = imagenUnidad.sizeDelta;
    }
    if (tamano.x < 1f || tamano.y < 1f)
    {
      tamano = new Vector2(72f, 96f);
    }

    return tamano;
  }

  private Vector2 ObtenerPosicionImpactoLocal()
  {
    if (imagenUnidad == null)
    {
      return Vector2.zero;
    }

    RectTransform parent = overlayRoot != null ? overlayRoot.parent as RectTransform : imagenUnidad.parent as RectTransform;
    if (parent == null)
    {
      return imagenUnidad.anchoredPosition;
    }

    Transform puntoBase = unidad != null ? unidad.puntoEntrante : null;
    if (puntoBase == null)
    {
      return imagenUnidad.anchoredPosition;
    }

    Canvas canvas = parent.GetComponentInParent<Canvas>();
    Camera eventCamera = ObtenerCanvasCamera(canvas);
    Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, puntoBase.position);
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, eventCamera, out Vector2 localPoint))
    {
      return localPoint;
    }

    return imagenUnidad.anchoredPosition;
  }

  private static Camera ObtenerCanvasCamera(Canvas canvas)
  {
    if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
    {
      return null;
    }

    if (canvas.worldCamera != null)
    {
      return canvas.worldCamera;
    }

    return Camera.main;
  }

  private static Color AjustarColorImpacto(Color color, int tipoDanio, bool guardia)
  {
    if (guardia)
    {
      return Color.Lerp(color, Color.white, 0.18f);
    }

    switch (tipoDanio)
    {
      case 4: return Color.Lerp(color, new Color(1f, 0.88f, 0.56f, 1f), 0.16f);
      case 5: return Color.Lerp(color, new Color(0.82f, 0.96f, 1f, 1f), 0.18f);
      case 6: return Color.Lerp(color, Color.white, 0.2f);
      case 8: return Color.Lerp(color, new Color(0.78f, 0.9f, 1f, 1f), 0.12f);
      case 9: return Color.Lerp(color, new Color(0.9f, 1f, 0.78f, 1f), 0.1f);
      case 11: return Color.Lerp(color, new Color(1f, 0.97f, 0.72f, 1f), 0.22f);
      default: return color;
    }
  }

  private static void AsegurarSprites()
  {
    if (spriteSuave == null)
    {
      texturaSuave = CrearTexturaRadial(128, 0f, 0.58f, false);
      spriteSuave = CrearSprite(texturaSuave);
    }

    if (spriteAnillo == null)
    {
      texturaAnillo = CrearTexturaAnillo(128, 0.46f, 0.78f);
      spriteAnillo = CrearSprite(texturaAnillo);
    }

    if (spriteGlint == null)
    {
      texturaGlint = CrearTexturaGlint(96, 48);
      spriteGlint = CrearSprite(texturaGlint);
    }
  }

  private static Image CrearImagen(string nombre, RectTransform parent, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(parent, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.preserveAspect = false;
    image.color = new Color(1f, 1f, 1f, 0f);
    return image;
  }

  private static void ConfigurarImagen(Image image, Vector2 posicion, Vector2 tamano, Color color)
  {
    if (image == null)
    {
      return;
    }

    RectTransform rect = image.rectTransform;
    rect.anchoredPosition = posicion;
    rect.sizeDelta = tamano;
    image.color = color;
  }

  private static Texture2D CrearTexturaRadial(int size, float innerRadius, float outerRadius, bool invert)
  {
    Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;

    Color[] pixels = new Color[size * size];
    Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
    float maxRadius = size * 0.5f;

    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float distance = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
        float normalized = Mathf.InverseLerp(innerRadius, outerRadius, distance);
        float alpha = invert ? normalized : 1f - normalized;
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
      }
    }

    texture.SetPixels(pixels);
    texture.Apply();
    return texture;
  }

  private static Texture2D CrearTexturaAnillo(int size, float innerRadius, float outerRadius)
  {
    Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;

    Color[] pixels = new Color[size * size];
    Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
    float maxRadius = size * 0.5f;

    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float distance = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
        float outer = 1f - Mathf.InverseLerp(innerRadius, outerRadius, distance);
        float inner = 1f - Mathf.InverseLerp(0f, innerRadius, distance);
        float alpha = Mathf.Clamp01(outer - inner);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texture.SetPixels(pixels);
    texture.Apply();
    return texture;
  }

  private static Texture2D CrearTexturaGlint(int width, int height)
  {
    Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.filterMode = FilterMode.Bilinear;

    Color[] pixels = new Color[width * height];
    float centerX = (width - 1) * 0.5f;
    float centerY = (height - 1) * 0.5f;

    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float nx = Mathf.Abs((x - centerX) / Mathf.Max(1f, centerX));
        float ny = Mathf.Abs((y - centerY) / Mathf.Max(1f, centerY));
        float horizontal = 1f - Mathf.Clamp01(nx);
        float vertical = 1f - Mathf.Clamp01(ny);
        float alpha = Mathf.Pow(horizontal, 3.4f) * Mathf.Pow(vertical, 1.1f);
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texture.SetPixels(pixels);
    texture.Apply();
    return texture;
  }

  private static Sprite CrearSprite(Texture2D texture)
  {
    return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
  }

  private static Color WithAlpha(Color color, float alpha)
  {
    color.a = Mathf.Clamp01(alpha);
    return color;
  }
}
