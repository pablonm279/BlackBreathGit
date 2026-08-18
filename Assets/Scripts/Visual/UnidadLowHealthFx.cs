using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnidadLowHealthFx : MonoBehaviour
{
  private const float UmbralVidaBaja = 0.25f;

  [Header("Vida baja")]
  [SerializeField] private float velocidadFade = 5.8f;
  [SerializeField] private float alphaOscurecer = 0.18f;
  [SerializeField] private float alphaFlashMin = 0.045f;
  [SerializeField] private float alphaFlashMax = 0.18f;
  [SerializeField] private float velocidadFlash = 4.2f;
  [SerializeField] private Vector2 escalaSpriteOverlay = new Vector2(0.88f, 1.08f);
  [SerializeField] private Vector2 escalaGlow = new Vector2(1.02f, 1.24f);
  [SerializeField] private Color colorFlash = new Color(1f, 0.08f, 0.045f, 1f);

  private Unidad unidad;
  private RectTransform imagenUnidad;
  private RectTransform overlayRoot;
  private CanvasGroup overlayGroup;
  private Image oscurecerSprite;
  private Image flashSprite;
  private Image flashGlow;
  private float visibilidad;
  private float faseFlash;
  private bool esControladaPorIA;

  private static Texture2D texturaGlow;
  private static Sprite spriteGlow;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
    // La presencia de IAUnidad es fija para toda la vida del objeto (no se agrega/quita en runtime),
    // asi que se cachea una vez en vez de hacer GetComponent en cada LateUpdate.
    esControladaPorIA = unidad != null && unidad.GetComponent<IAUnidad>() != null;
    faseFlash = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (!VincularImagenUnidad())
    {
      OcultarOverlay();
      return;
    }

    bool activo = DebeMostrarVidaBaja();
    if (activo)
    {
      AsegurarOverlay();
    }

    ActualizarOverlay(activo);
  }

  private void OnDisable()
  {
    visibilidad = 0f;
    OcultarOverlay();
  }

  private void OnDestroy()
  {
    if (overlayRoot != null)
    {
      Destroy(overlayRoot.gameObject);
    }
  }

  private bool DebeMostrarVidaBaja()
  {
    return unidad != null
      && unidad.gameObject.activeInHierarchy
      && unidad.HP_actual > 0f
      && unidad.mod_maxHP > 0f
      && unidad.HP_actual <= unidad.mod_maxHP * UmbralVidaBaja
      && !esControladaPorIA;
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

      esControladaPorIA = unidad.GetComponent<IAUnidad>() != null;
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
        DestruirOverlay();
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

    GameObject rootGo = new GameObject("LowHealthFx", typeof(RectTransform), typeof(CanvasGroup));
    overlayRoot = rootGo.GetComponent<RectTransform>();
    overlayGroup = rootGo.GetComponent<CanvasGroup>();
    overlayRoot.SetParent(imagenUnidad.parent, false);
    overlayGroup.interactable = false;
    overlayGroup.blocksRaycasts = false;
    overlayGroup.alpha = 0f;

    oscurecerSprite = CrearImagen("OscurecerSprite", overlayRoot, unidad.uImage.sprite);
    flashSprite = CrearImagen("FlashSprite", overlayRoot, unidad.uImage.sprite);
    flashGlow = CrearImagen("FlashGlow", overlayRoot, ObtenerSpriteGlow());
    flashGlow.preserveAspect = false;

    overlayRoot.gameObject.SetActive(false);
  }

  private void ActualizarOverlay(bool activo)
  {
    if (overlayRoot == null || overlayGroup == null || imagenUnidad == null)
    {
      return;
    }

    float dt = Time.unscaledDeltaTime;
    visibilidad = Mathf.MoveTowards(visibilidad, activo ? 1f : 0f, dt * Mathf.Max(0.1f, velocidadFade));

    bool debeSeguirActivo = activo || visibilidad > 0.001f;
    if (overlayRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      overlayGroup.alpha = 0f;
      return;
    }

    SincronizarOverlay();

    float pulso = 0.5f + (0.5f * Mathf.Sin((Time.unscaledTime * velocidadFlash) + faseFlash));
    float pulsoSuave = Mathf.SmoothStep(0f, 1f, pulso);
    float flashAlpha = Mathf.Lerp(alphaFlashMin, alphaFlashMax, pulsoSuave) * visibilidad;

    overlayGroup.alpha = visibilidad * (unidad != null ? unidad.ObtenerMultiplicadorAlphaVisual() : 1f);
    ConfigurarImagenSprite(oscurecerSprite, new Color(0f, 0f, 0f, alphaOscurecer * visibilidad), escalaSpriteOverlay);
    ConfigurarImagenSprite(flashSprite, WithAlpha(colorFlash, flashAlpha), escalaSpriteOverlay);

    Vector2 tamano = ObtenerTamanoUnidad();
    ConfigurarImagen(
      flashGlow,
      Vector2.zero,
      new Vector2(tamano.x * escalaGlow.x, tamano.y * escalaGlow.y),
      WithAlpha(colorFlash, flashAlpha * 0.54f));
  }

  private void SincronizarOverlay()
  {
    Vector2 tamano = ObtenerTamanoUnidad();
    overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayRoot.anchoredPosition = imagenUnidad.anchoredPosition;
    overlayRoot.localEulerAngles = imagenUnidad.localEulerAngles;
    overlayRoot.localScale = imagenUnidad.localScale;
    overlayRoot.sizeDelta = tamano;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Clamp(sibling + 1, 0, imagenUnidad.parent.childCount - 1);
    if (overlayRoot.GetSiblingIndex() != targetSibling)
    {
      overlayRoot.SetSiblingIndex(targetSibling);
    }

    SincronizarSpriteUnidad(oscurecerSprite);
    SincronizarSpriteUnidad(flashSprite);
  }

  private void SincronizarSpriteUnidad(Image image)
  {
    if (image == null || unidad == null || unidad.uImage == null)
    {
      return;
    }

    image.sprite = unidad.uImage.sprite;
    image.overrideSprite = unidad.uImage.overrideSprite;
    image.type = unidad.uImage.type;
    image.preserveAspect = unidad.uImage.preserveAspect;
    image.fillCenter = unidad.uImage.fillCenter;
    image.fillMethod = unidad.uImage.fillMethod;
    image.fillAmount = unidad.uImage.fillAmount;
    image.fillClockwise = unidad.uImage.fillClockwise;
    image.fillOrigin = unidad.uImage.fillOrigin;
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

  private static Image CrearImagen(string nombre, RectTransform parent, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(parent, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    image.color = new Color(1f, 1f, 1f, 0f);
    return image;
  }

  private static void ConfigurarImagenSprite(Image image, Color color, Vector2 escala)
  {
    if (image == null)
    {
      return;
    }

    RectTransform rect = image.rectTransform;
    rect.anchoredPosition = Vector2.zero;
    Vector2 tamanoBase = ((RectTransform)image.transform.parent).sizeDelta;
    rect.sizeDelta = new Vector2(tamanoBase.x * escala.x, tamanoBase.y * escala.y);
    rect.localScale = Vector3.one;
    rect.localEulerAngles = Vector3.zero;
    image.color = color;
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
    rect.localScale = Vector3.one;
    rect.localEulerAngles = Vector3.zero;
    image.color = color;
  }

  private void OcultarOverlay()
  {
    if (overlayRoot != null && overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(false);
    }
  }

  private void DestruirOverlay()
  {
    if (overlayRoot != null)
    {
      Destroy(overlayRoot.gameObject);
    }

    overlayRoot = null;
    overlayGroup = null;
    oscurecerSprite = null;
    flashSprite = null;
    flashGlow = null;
    visibilidad = 0f;
  }

  private static Sprite ObtenerSpriteGlow()
  {
    if (spriteGlow != null)
    {
      return spriteGlow;
    }

    const int size = 96;
    texturaGlow = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaGlow.name = "LowHealthGlowRuntime";
    texturaGlow.wrapMode = TextureWrapMode.Clamp;
    texturaGlow.filterMode = FilterMode.Bilinear;
    texturaGlow.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    Vector2 centro = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float distancia = Vector2.Distance(new Vector2(x, y), centro) / radio;
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.1f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaGlow.SetPixels(pixels);
    texturaGlow.Apply(false, true);
    spriteGlow = Sprite.Create(
      texturaGlow,
      new Rect(0f, 0f, size, size),
      new Vector2(0.5f, 0.5f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteGlow.name = "LowHealthGlowRuntime";
    return spriteGlow;
  }

  private static Color WithAlpha(Color color, float alpha)
  {
    color.a = Mathf.Clamp01(alpha);
    return color;
  }
}
