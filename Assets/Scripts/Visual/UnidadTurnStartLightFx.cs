using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnidadTurnStartLightFx : MonoBehaviour
{
  [Header("Haz de luz")]
  [SerializeField] private float duracionTotal = 3.5f;
  [SerializeField] private float intensidad = 0.95f;
  [SerializeField] private float factorAltura = 0.97f;
  [SerializeField] private Color colorHazExterior = new Color(1f, 0.9f, 0.6f, 0.19f);
  [SerializeField] private Color colorHazInterior = new Color(1f, 0.96f, 0.76f, 0.28f);
  [SerializeField] private Color colorResplandorBase = new Color(1f, 0.88f, 0.52f, 0.17f);
  [SerializeField] private Color colorHaloCentral = new Color(1f, 0.94f, 0.68f, 0.13f);

  private Unidad unidad;
  private RectTransform imagenUnidad;
  private RectTransform overlayRoot;
  private CanvasGroup overlayGroup;
  private Image hazExterior;
  private Image hazInterior;
  private Image resplandorBase;
  private Image haloCentral;
  private float tiempoActivo = -1f;
  private float fasePulso;

  private static Sprite spriteSuave;
  private static Texture2D texturaSuave;
  private static Sprite spriteHaz;
  private static Texture2D texturaHaz;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
    fasePulso = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (!VincularImagenUnidad())
    {
      return;
    }

    bool activo = tiempoActivo > 0f && unidad != null && unidad.HP_actual > 0f && gameObject.activeInHierarchy;
    if (activo)
    {
      AsegurarOverlay();
    }

    ActualizarOverlay(activo);
  }

  private void OnDisable()
  {
    tiempoActivo = -1f;
    if (overlayRoot != null)
    {
      overlayRoot.gameObject.SetActive(false);
    }
  }

  private void OnDestroy()
  {
    if (overlayRoot != null)
    {
      Destroy(overlayRoot.gameObject);
    }
  }

  public void Reproducir()
  {
    if (!VincularImagenUnidad())
    {
      return;
    }

    tiempoActivo = Mathf.Max(0.01f, duracionTotal);
    AsegurarOverlay();
    if (overlayRoot != null)
    {
      overlayRoot.gameObject.SetActive(true);
    }
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
        hazExterior = null;
        hazInterior = null;
        resplandorBase = null;
        haloCentral = null;
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

    GameObject rootGo = new GameObject("TurnStartLightFx", typeof(RectTransform), typeof(CanvasGroup));
    overlayRoot = rootGo.GetComponent<RectTransform>();
    overlayGroup = rootGo.GetComponent<CanvasGroup>();
    overlayRoot.SetParent(imagenUnidad.parent, false);
    overlayGroup.interactable = false;
    overlayGroup.blocksRaycasts = false;
    overlayGroup.alpha = 0f;

    hazExterior = CrearImagen("HazExterior", overlayRoot, ObtenerSpriteHaz(), new Vector2(0.5f, 0f));
    hazInterior = CrearImagen("HazInterior", overlayRoot, ObtenerSpriteHaz(), new Vector2(0.5f, 0f));
    resplandorBase = CrearImagen("ResplandorBase", overlayRoot, ObtenerSpriteSuave(), new Vector2(0.5f, 0.5f));
    haloCentral = CrearImagen("HaloCentral", overlayRoot, ObtenerSpriteSuave(), new Vector2(0.5f, 0.5f));

    if (hazExterior != null) { hazExterior.preserveAspect = false; }
    if (hazInterior != null) { hazInterior.preserveAspect = false; }
    if (resplandorBase != null) { resplandorBase.preserveAspect = true; }
    if (haloCentral != null) { haloCentral.preserveAspect = true; }

    overlayRoot.gameObject.SetActive(false);
  }

  private void ActualizarOverlay(bool activo)
  {
    if (overlayRoot == null || overlayGroup == null || imagenUnidad == null)
    {
      return;
    }

    if (!activo)
    {
      overlayGroup.alpha = 0f;
      if (overlayRoot.gameObject.activeSelf)
      {
        overlayRoot.gameObject.SetActive(false);
      }
      return;
    }

    if (!overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(true);
    }

    float duracion = Mathf.Max(0.01f, duracionTotal);
    tiempoActivo = Mathf.Max(0f, tiempoActivo - Time.deltaTime);
    float progreso = 1f - (tiempoActivo / duracion);
    float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progreso / 0.14f));
    float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progreso - 0.68f) / 0.32f));
    float envolvente = fadeIn * fadeOut * Mathf.Clamp01(intensidad);
    float pulso = 0.9f + (0.1f * Mathf.Sin((Time.time * 2.25f) + fasePulso));

    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(18f, tamano.x);
    float alto = Mathf.Max(24f, tamano.y);
    float alturaHaz = alto * Mathf.Max(0.2f, factorAltura);
    float derivaY = Mathf.Lerp(0f, alturaHaz * 0.12f, progreso);

    overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayRoot.anchoredPosition = imagenUnidad.anchoredPosition;
    overlayRoot.localEulerAngles = Vector3.zero;
    overlayRoot.localScale = imagenUnidad.localScale;
    overlayRoot.sizeDelta = new Vector2(ancho * 1.5f, alturaHaz * 2.4f);

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Max(0, sibling - 1);
    if (overlayRoot.GetSiblingIndex() != targetSibling)
    {
      overlayRoot.SetSiblingIndex(targetSibling);
    }

    overlayGroup.alpha = envolvente * (unidad != null ? unidad.ObtenerMultiplicadorAlphaVisual() : 1f);

    ConfigurarCapa(
      hazExterior,
      new Vector2(0f, (-alturaHaz * 0.7f) + derivaY),
      new Vector2(ancho * 1.28f * pulso, alturaHaz * 2.7f),
      EscalarAlpha(colorHazExterior, 0.9f + ((1f - progreso) * 0.08f)));

    ConfigurarCapa(
      hazInterior,
      new Vector2(0f, (-alturaHaz * 0.66f) + (derivaY * 1.15f)),
      new Vector2(ancho * 0.62f * pulso, alturaHaz * 2.15f),
      EscalarAlpha(colorHazInterior, 1f + (0.08f * pulso)));

    ConfigurarCapa(
      resplandorBase,
      new Vector2(0f, -alturaHaz * 0.42f),
      new Vector2(ancho * 1.12f * (1f + (0.06f * pulso)), alturaHaz * 0.48f),
      EscalarAlpha(colorResplandorBase, 0.9f + (0.12f * pulso)));

    ConfigurarCapa(
      haloCentral,
      new Vector2(0f, (alturaHaz * 0.04f) + (derivaY * 0.7f)),
      new Vector2(ancho * 0.86f, alturaHaz * 1.02f),
      EscalarAlpha(colorHaloCentral, 0.92f + (0.08f * pulso)));

    if (tiempoActivo <= 0f)
    {
      tiempoActivo = -1f;
    }
  }

  private Vector2 ObtenerTamanoUnidad()
  {
    if (imagenUnidad == null)
    {
      return new Vector2(32f, 32f);
    }

    Vector2 tamano = imagenUnidad.rect.size;
    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = imagenUnidad.sizeDelta;
    }

    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = new Vector2(32f, 32f);
    }

    return tamano;
  }

  private static Image CrearImagen(string nombre, RectTransform padre, Sprite sprite, Vector2 pivot)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = pivot;

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    return image;
  }

  private static void ConfigurarCapa(Image image, Vector2 posicion, Vector2 tamano, Color color)
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

  private static Color EscalarAlpha(Color color, float multiplicador)
  {
    color.a *= multiplicador;
    return color;
  }

  private static Sprite ObtenerSpriteSuave()
  {
    if (spriteSuave != null)
    {
      return spriteSuave;
    }

    const int size = 64;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "TurnStartLightSoftRuntime";
    texturaSuave.wrapMode = TextureWrapMode.Clamp;
    texturaSuave.filterMode = FilterMode.Bilinear;
    texturaSuave.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float borde = Mathf.Clamp01(1f - distancia);
        float alpha = Mathf.Pow(borde, 2.35f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(
      texturaSuave,
      new Rect(0f, 0f, size, size),
      new Vector2(0.5f, 0.5f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteSuave.name = "TurnStartLightSoftRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteHaz()
  {
    if (spriteHaz != null)
    {
      return spriteHaz;
    }

    const int width = 64;
    const int height = 128;
    texturaHaz = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaHaz.name = "TurnStartLightBeamRuntime";
    texturaHaz.wrapMode = TextureWrapMode.Clamp;
    texturaHaz.filterMode = FilterMode.Bilinear;
    texturaHaz.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroX = (width - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      float ny = y / (height - 1f);
      float anchoHaz = Mathf.Lerp(1.08f, 0.14f, ny);
      float vertical = Mathf.SmoothStep(0f, 1f, ny) * (0.85f + (0.15f * (1f - ny)));
      float desvanecimientoSuperior = 1f - Mathf.SmoothStep(0.76f, 1f, ny);
      vertical *= desvanecimientoSuperior;
      float redondeoPunta = 1f - Mathf.SmoothStep(0.82f, 1f, ny);

      for (int x = 0; x < width; x++)
      {
        float nx = Mathf.Abs((x - centroX) / (centroX * anchoHaz));
        float lateral = Mathf.Clamp01(1f - nx);
        lateral = Mathf.Pow(lateral, ny > 0.82f ? 2.2f : 3.6f);
        float alpha = lateral * vertical;
        if (ny > 0.78f)
        {
          float cierreSuperior = Mathf.Clamp01(1f - (nx * Mathf.Lerp(0.2f, 1.35f, 1f - redondeoPunta)));
          alpha *= cierreSuperior;
        }
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaHaz.SetPixels(pixels);
    texturaHaz.Apply(false, true);
    spriteHaz = Sprite.Create(
      texturaHaz,
      new Rect(0f, 0f, width, height),
      new Vector2(0.5f, 0f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteHaz.name = "TurnStartLightBeamRuntime";
    return spriteHaz;
  }
}

[DisallowMultipleComponent]
public sealed class UnidadCanalizadorAuraFx : MonoBehaviour
{
  [Header("Aura persistente")]
  [SerializeField] private float intensidadMinima = 0.38f;
  [SerializeField] private float intensidadMaxima = 0.82f;
  [SerializeField] private float factorAncho = 1.343f;
  [SerializeField] private float factorAltura = 0.833f;
  [SerializeField] private float desplazamientoY = -2f;
  [SerializeField] private Color colorHazExterior = new Color(0.18f, 0.62f, 1f, 0.15f);
  [SerializeField] private Color colorHazInterior = new Color(0.42f, 0.86f, 1f, 0.22f);
  [SerializeField] private Color colorResplandorBase = new Color(0.16f, 0.52f, 1f, 0.12f);
  [SerializeField] private Color colorHaloCentral = new Color(0.68f, 0.94f, 1f, 0.16f);

  private Unidad unidad;
  private ClaseCanalizador canalizador;
  private RectTransform imagenUnidad;
  private RectTransform overlayRoot;
  private CanvasGroup overlayGroup;
  private Image hazExterior;
  private Image hazInterior;
  private Image resplandorBase;
  private Image haloCentral;
  private float fasePulso;
  private float faseDeriva;

  private static Sprite spriteAuraSuave;
  private static Texture2D texturaAuraSuave;
  private static Sprite spriteAuraHaz;
  private static Texture2D texturaAuraHaz;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
    canalizador = GetComponent<ClaseCanalizador>();
    fasePulso = Random.Range(0f, Mathf.PI * 2f);
    faseDeriva = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (canalizador == null || !VincularImagenUnidad())
    {
      OcultarOverlay();
      return;
    }

    bool activo = unidad != null && unidad.HP_actual > 0f && gameObject.activeInHierarchy;
    if (activo)
    {
      AsegurarOverlay();
    }

    ActualizarOverlay(activo);
  }

  private void OnDisable()
  {
    OcultarOverlay();
  }

  private void OnDestroy()
  {
    if (overlayRoot != null)
    {
      Destroy(overlayRoot.gameObject);
    }
  }

  private bool VincularImagenUnidad()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
    }

    if (canalizador == null)
    {
      canalizador = GetComponent<ClaseCanalizador>();
    }

    if (unidad == null || canalizador == null || unidad.uImage == null)
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
        hazExterior = null;
        hazInterior = null;
        resplandorBase = null;
        haloCentral = null;
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

    GameObject rootGo = new GameObject("CanalizadorAuraFx", typeof(RectTransform), typeof(CanvasGroup));
    overlayRoot = rootGo.GetComponent<RectTransform>();
    overlayGroup = rootGo.GetComponent<CanvasGroup>();
    overlayRoot.SetParent(imagenUnidad.parent, false);
    overlayGroup.interactable = false;
    overlayGroup.blocksRaycasts = false;
    overlayGroup.alpha = 0f;

    hazExterior = CrearImagen("HazExterior", overlayRoot, ObtenerSpriteAuraHaz(), new Vector2(0.5f, 0f));
    hazInterior = CrearImagen("HazInterior", overlayRoot, ObtenerSpriteAuraHaz(), new Vector2(0.5f, 0f));
    resplandorBase = CrearImagen("ResplandorBase", overlayRoot, ObtenerSpriteAuraSuave(), new Vector2(0.5f, 0.5f));
    haloCentral = CrearImagen("HaloCentral", overlayRoot, ObtenerSpriteAuraSuave(), new Vector2(0.5f, 0.5f));

    if (hazExterior != null) { hazExterior.preserveAspect = false; }
    if (hazInterior != null) { hazInterior.preserveAspect = false; }
    if (resplandorBase != null) { resplandorBase.preserveAspect = true; }
    if (haloCentral != null) { haloCentral.preserveAspect = true; }
  }

  private void ActualizarOverlay(bool activo)
  {
    if (overlayRoot == null || overlayGroup == null || imagenUnidad == null)
    {
      return;
    }

    if (!activo)
    {
      OcultarOverlay();
      return;
    }

    if (!overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(true);
    }

    int energia = Mathf.Clamp(canalizador.ObtenerEnergia(), 0, 3);
    float progresoEnergia = energia / 3f;
    float empujeAcumulando = unidad.TieneBuffNombre("Acumulando") ? 0.08f : 0f;
    float intensidad = Mathf.Lerp(intensidadMinima, intensidadMaxima, progresoEnergia) + empujeAcumulando;
    float pulsoBase = 0.94f + (0.06f * Mathf.Sin((Time.time * Mathf.Lerp(1.65f, 2.3f, progresoEnergia)) + fasePulso));
    float derivaY = Mathf.Sin((Time.time * 1.08f) + faseDeriva) * Mathf.Lerp(0.8f, 1.6f, progresoEnergia);

    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(24f, tamano.x * factorAncho);
    float alto = Mathf.Max(18f, tamano.y * factorAltura);

    overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayRoot.anchoredPosition = imagenUnidad.anchoredPosition + new Vector2(0f, desplazamientoY);
    overlayRoot.localEulerAngles = Vector3.zero;
    overlayRoot.localScale = imagenUnidad.localScale;
    overlayRoot.sizeDelta = new Vector2(ancho * 1.8f, alto * 1.7f);

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Max(0, sibling - 1);
    if (overlayRoot.GetSiblingIndex() != targetSibling)
    {
      overlayRoot.SetSiblingIndex(targetSibling);
    }

    overlayGroup.alpha = intensidad * (unidad != null ? unidad.ObtenerMultiplicadorAlphaVisual() : 1f);

    ConfigurarCapa(
      hazExterior,
      new Vector2(0f, (-alto * 0.34f) + derivaY),
      new Vector2(ancho * 1.36f * pulsoBase, alto * 1.98f),
      EscalarAlpha(colorHazExterior, 0.86f + (progresoEnergia * 0.34f)));

    ConfigurarCapa(
      hazInterior,
      new Vector2(0f, (-alto * 0.28f) + (derivaY * 1.12f)),
      new Vector2(ancho * 0.78f * pulsoBase, alto * 1.72f),
      EscalarAlpha(colorHazInterior, 0.94f + (progresoEnergia * 0.42f)));

    ConfigurarCapa(
      resplandorBase,
      new Vector2(0f, -alto * 0.08f),
      new Vector2(ancho * (1.08f + (progresoEnergia * 0.14f)), alto * 0.48f * (1f + ((pulsoBase - 0.94f) * 0.45f))),
      EscalarAlpha(colorResplandorBase, 0.86f + (progresoEnergia * 0.38f)));

    ConfigurarCapa(
      haloCentral,
      new Vector2(0f, (alto * 0.12f) + (derivaY * 0.42f)),
      new Vector2(ancho * (0.96f + (progresoEnergia * 0.12f)), alto * (1.04f + (progresoEnergia * 0.14f))),
      EscalarAlpha(colorHaloCentral, 0.9f + (progresoEnergia * 0.34f)));
  }

  private Vector2 ObtenerTamanoUnidad()
  {
    if (imagenUnidad == null)
    {
      return new Vector2(32f, 32f);
    }

    Vector2 tamano = imagenUnidad.rect.size;
    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = imagenUnidad.sizeDelta;
    }

    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = new Vector2(32f, 32f);
    }

    return tamano;
  }

  private void OcultarOverlay()
  {
    if (overlayGroup != null)
    {
      overlayGroup.alpha = 0f;
    }

    if (overlayRoot != null && overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(false);
    }
  }

  private static Image CrearImagen(string nombre, RectTransform padre, Sprite sprite, Vector2 pivot)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = pivot;

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    return image;
  }

  private static void ConfigurarCapa(Image image, Vector2 posicion, Vector2 tamano, Color color)
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

  private static Color EscalarAlpha(Color color, float multiplicador)
  {
    color.a *= multiplicador;
    return color;
  }

  private static Sprite ObtenerSpriteAuraSuave()
  {
    if (spriteAuraSuave != null)
    {
      return spriteAuraSuave;
    }

    const int size = 64;
    texturaAuraSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaAuraSuave.name = "CanalizadorAuraSoftRuntime";
    texturaAuraSuave.wrapMode = TextureWrapMode.Clamp;
    texturaAuraSuave.filterMode = FilterMode.Bilinear;
    texturaAuraSuave.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float borde = Mathf.Clamp01(1f - distancia);
        float alpha = Mathf.Pow(borde, 2.35f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAuraSuave.SetPixels(pixels);
    texturaAuraSuave.Apply(false, true);
    spriteAuraSuave = Sprite.Create(
      texturaAuraSuave,
      new Rect(0f, 0f, size, size),
      new Vector2(0.5f, 0.5f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteAuraSuave.name = "CanalizadorAuraSoftRuntime";
    return spriteAuraSuave;
  }

  private static Sprite ObtenerSpriteAuraHaz()
  {
    if (spriteAuraHaz != null)
    {
      return spriteAuraHaz;
    }

    const int width = 64;
    const int height = 128;
    texturaAuraHaz = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaAuraHaz.name = "CanalizadorAuraBeamRuntime";
    texturaAuraHaz.wrapMode = TextureWrapMode.Clamp;
    texturaAuraHaz.filterMode = FilterMode.Bilinear;
    texturaAuraHaz.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroX = (width - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      float ny = y / (height - 1f);
      float anchoHaz = Mathf.Lerp(1.26f, 0.4f, ny);
      float vertical = Mathf.SmoothStep(0f, 1f, ny) * (0.92f - (0.22f * ny));
      float desvanecimientoSuperior = 1f - Mathf.SmoothStep(0.68f, 1f, ny);
      vertical *= desvanecimientoSuperior;

      for (int x = 0; x < width; x++)
      {
        float nx = Mathf.Abs((x - centroX) / (centroX * anchoHaz));
        float lateral = Mathf.Clamp01(1f - nx);
        lateral = Mathf.Pow(lateral, 2.8f);
        float alpha = lateral * vertical;
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAuraHaz.SetPixels(pixels);
    texturaAuraHaz.Apply(false, true);
    spriteAuraHaz = Sprite.Create(
      texturaAuraHaz,
      new Rect(0f, 0f, width, height),
      new Vector2(0.5f, 0f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteAuraHaz.name = "CanalizadorAuraBeamRuntime";
    return spriteAuraHaz;
  }
}

[DisallowMultipleComponent]
public sealed class UnidadPurificadoraFervorAuraFx : MonoBehaviour
{
  [Header("Aura persistente")]
  [SerializeField] private float intensidadMaxima = 0.86f;
  [SerializeField] private float factorAncho = 0.92f;
  [SerializeField] private float factorAltura = 0.9f;
  [SerializeField] private float desplazamientoY = -4f;
  [SerializeField] private Color colorHazExterior = new Color(1f, 0.84f, 0.2f, 0.22f);
  [SerializeField] private Color colorHazInterior = new Color(1f, 0.99f, 0.58f, 0.34f);
  [SerializeField] private Color colorResplandorBase = new Color(1f, 0.8f, 0.28f, 0.2f);
  [SerializeField] private Color colorHaloCentral = new Color(1f, 0.97f, 0.56f, 0.23f);

  private Unidad unidad;
  private ClasePurificadora purificadora;
  private RectTransform imagenUnidad;
  private RectTransform overlayRoot;
  private CanvasGroup overlayGroup;
  private Image hazExterior;
  private Image hazInterior;
  private Image resplandorBase;
  private Image haloCentral;
  private float fasePulso;
  private float faseDeriva;

  private static Sprite spriteAuraSuave;
  private static Texture2D texturaAuraSuave;
  private static Sprite spriteAuraHaz;
  private static Texture2D texturaAuraHaz;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
    purificadora = GetComponent<ClasePurificadora>();
    fasePulso = Random.Range(0f, Mathf.PI * 2f);
    faseDeriva = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (purificadora == null || !VincularImagenUnidad())
    {
      OcultarOverlay();
      return;
    }

    bool activo = unidad != null && unidad.HP_actual > 0f && gameObject.activeInHierarchy;
    if (activo)
    {
      AsegurarOverlay();
    }

    ActualizarOverlay(activo);
  }

  private void OnDisable()
  {
    OcultarOverlay();
  }

  private void OnDestroy()
  {
    if (overlayRoot != null)
    {
      Destroy(overlayRoot.gameObject);
    }
  }

  private bool VincularImagenUnidad()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
    }

    if (purificadora == null)
    {
      purificadora = GetComponent<ClasePurificadora>();
    }

    if (unidad == null || purificadora == null || unidad.uImage == null)
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
        hazExterior = null;
        hazInterior = null;
        resplandorBase = null;
        haloCentral = null;
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

    GameObject rootGo = new GameObject("PurificadoraFervorAuraFx", typeof(RectTransform), typeof(CanvasGroup));
    overlayRoot = rootGo.GetComponent<RectTransform>();
    overlayGroup = rootGo.GetComponent<CanvasGroup>();
    overlayRoot.SetParent(imagenUnidad.parent, false);
    overlayGroup.interactable = false;
    overlayGroup.blocksRaycasts = false;
    overlayGroup.alpha = 0f;

    hazExterior = CrearImagen("HazExterior", overlayRoot, ObtenerSpriteAuraHaz(), new Vector2(0.5f, 0f));
    hazInterior = CrearImagen("HazInterior", overlayRoot, ObtenerSpriteAuraHaz(), new Vector2(0.5f, 0f));
    resplandorBase = CrearImagen("ResplandorBase", overlayRoot, ObtenerSpriteAuraSuave(), new Vector2(0.5f, 0.5f));
    haloCentral = CrearImagen("HaloCentral", overlayRoot, ObtenerSpriteAuraSuave(), new Vector2(0.5f, 0.5f));

    if (hazExterior != null) { hazExterior.preserveAspect = false; }
    if (hazInterior != null) { hazInterior.preserveAspect = false; }
    if (resplandorBase != null) { resplandorBase.preserveAspect = true; }
    if (haloCentral != null) { haloCentral.preserveAspect = true; }
  }

  private void ActualizarOverlay(bool activo)
  {
    if (overlayRoot == null || overlayGroup == null || imagenUnidad == null)
    {
      return;
    }

    if (!activo)
    {
      OcultarOverlay();
      return;
    }

    int fervor = Mathf.Clamp(purificadora.ObtenerFervor(), 0, 5);
    if (fervor <= 0)
    {
      OcultarOverlay();
      return;
    }

    if (!overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(true);
    }

    float progresoFervor = fervor / 5f;
    float intensidad = Mathf.Lerp(0.22f, intensidadMaxima, progresoFervor);
    float pulsoBase = 0.95f + (0.05f * Mathf.Sin((Time.time * Mathf.Lerp(1.45f, 2f, progresoFervor)) + fasePulso));
    float derivaY = Mathf.Sin((Time.time * 0.95f) + faseDeriva) * Mathf.Lerp(0.6f, 1.4f, progresoFervor);

    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(24f, tamano.x * factorAncho);
    float alto = Mathf.Max(20f, tamano.y * factorAltura);

    overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayRoot.anchoredPosition = imagenUnidad.anchoredPosition + new Vector2(0f, desplazamientoY);
    overlayRoot.localEulerAngles = Vector3.zero;
    overlayRoot.localScale = imagenUnidad.localScale;
    overlayRoot.sizeDelta = new Vector2(ancho * 1.55f, alto * 1.82f);

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Max(0, sibling - 1);
    if (overlayRoot.GetSiblingIndex() != targetSibling)
    {
      overlayRoot.SetSiblingIndex(targetSibling);
    }

    overlayGroup.alpha = intensidad * (unidad != null ? unidad.ObtenerMultiplicadorAlphaVisual() : 1f);

    ConfigurarCapa(
      hazExterior,
      new Vector2(0f, (-alto * 0.42f) + derivaY),
      new Vector2(ancho * 1.02f * pulsoBase, alto * 2.08f),
      EscalarAlpha(colorHazExterior, 0.86f + (progresoFervor * 0.34f)));

    ConfigurarCapa(
      hazInterior,
      new Vector2(0f, (-alto * 0.36f) + (derivaY * 1.08f)),
      new Vector2(ancho * 0.52f * pulsoBase, alto * 1.9f),
      EscalarAlpha(colorHazInterior, 0.9f + (progresoFervor * 0.38f)));

    ConfigurarCapa(
      resplandorBase,
      new Vector2(0f, -alto * 0.16f),
      new Vector2(ancho * (1.04f + (progresoFervor * 0.12f)), alto * 0.54f * (1f + ((pulsoBase - 0.95f) * 0.42f))),
      EscalarAlpha(colorResplandorBase, 0.86f + (progresoFervor * 0.32f)));

    ConfigurarCapa(
      haloCentral,
      new Vector2(0f, (alto * 0.16f) + (derivaY * 0.38f)),
      new Vector2(ancho * (0.72f + (progresoFervor * 0.1f)), alto * (1.22f + (progresoFervor * 0.12f))),
      EscalarAlpha(colorHaloCentral, 0.88f + (progresoFervor * 0.3f)));
  }

  private Vector2 ObtenerTamanoUnidad()
  {
    if (imagenUnidad == null)
    {
      return new Vector2(32f, 32f);
    }

    Vector2 tamano = imagenUnidad.rect.size;
    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = imagenUnidad.sizeDelta;
    }

    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = new Vector2(32f, 32f);
    }

    return tamano;
  }

  private void OcultarOverlay()
  {
    if (overlayGroup != null)
    {
      overlayGroup.alpha = 0f;
    }

    if (overlayRoot != null && overlayRoot.gameObject.activeSelf)
    {
      overlayRoot.gameObject.SetActive(false);
    }
  }

  private static Image CrearImagen(string nombre, RectTransform padre, Sprite sprite, Vector2 pivot)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = pivot;

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    return image;
  }

  private static void ConfigurarCapa(Image image, Vector2 posicion, Vector2 tamano, Color color)
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

  private static Color EscalarAlpha(Color color, float multiplicador)
  {
    color.a *= multiplicador;
    return color;
  }

  private static Sprite ObtenerSpriteAuraSuave()
  {
    if (spriteAuraSuave != null)
    {
      return spriteAuraSuave;
    }

    const int size = 64;
    texturaAuraSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaAuraSuave.name = "PurificadoraFervorAuraSoftRuntime";
    texturaAuraSuave.wrapMode = TextureWrapMode.Clamp;
    texturaAuraSuave.filterMode = FilterMode.Bilinear;
    texturaAuraSuave.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float borde = Mathf.Clamp01(1f - distancia);
        float alpha = Mathf.Pow(borde, 2.2f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAuraSuave.SetPixels(pixels);
    texturaAuraSuave.Apply(false, true);
    spriteAuraSuave = Sprite.Create(
      texturaAuraSuave,
      new Rect(0f, 0f, size, size),
      new Vector2(0.5f, 0.5f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteAuraSuave.name = "PurificadoraFervorAuraSoftRuntime";
    return spriteAuraSuave;
  }

  private static Sprite ObtenerSpriteAuraHaz()
  {
    if (spriteAuraHaz != null)
    {
      return spriteAuraHaz;
    }

    const int width = 64;
    const int height = 148;
    texturaAuraHaz = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaAuraHaz.name = "PurificadoraFervorAuraBeamRuntime";
    texturaAuraHaz.wrapMode = TextureWrapMode.Clamp;
    texturaAuraHaz.filterMode = FilterMode.Bilinear;
    texturaAuraHaz.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroX = (width - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      float ny = y / (height - 1f);
      float anchoHaz = Mathf.Lerp(0.82f, 0.22f, ny);
      float vertical = Mathf.SmoothStep(0f, 1f, ny) * (0.97f - (0.18f * ny));
      float desvanecimientoSuperior = 1f - Mathf.SmoothStep(0.76f, 1f, ny);
      vertical *= desvanecimientoSuperior;

      for (int x = 0; x < width; x++)
      {
        float nx = Mathf.Abs((x - centroX) / (centroX * anchoHaz));
        float lateral = Mathf.Clamp01(1f - nx);
        lateral = Mathf.Pow(lateral, 2.35f + (ny * 0.4f));
        float alpha = lateral * vertical;
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAuraHaz.SetPixels(pixels);
    texturaAuraHaz.Apply(false, true);
    spriteAuraHaz = Sprite.Create(
      texturaAuraHaz,
      new Rect(0f, 0f, width, height),
      new Vector2(0.5f, 0f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteAuraHaz.name = "PurificadoraFervorAuraBeamRuntime";
    return spriteAuraHaz;
  }
}
