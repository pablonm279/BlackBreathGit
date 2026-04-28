using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TrampaSpawnLightFx : MonoBehaviour
{
  [SerializeField] private float duracionTotal = 2.82f;
  [SerializeField] private float intensidad = 1.5f;
  [SerializeField] private float factorTamano = 0.98f;
  [SerializeField] private float desplazamientoY = 8f;
  [SerializeField] private Color colorHazExterior = new Color(1f, 0.28f, 0.22f, 0.5f);
  [SerializeField] private Color colorHazInterior = new Color(1f, 0.42f, 0.34f, 0.58f);
  [SerializeField] private Color colorResplandorBase = new Color(0.96f, 0.2f, 0.18f, 0.16f);
  [SerializeField] private Color colorHaloCentral = new Color(1f, 0.58f, 0.46f, 0.14f);

  private RectTransform raizCanvas;
  private RectTransform referenciaVisual;
  private CanvasGroup overlayGroup;
  private RectTransform overlayRoot;
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
    fasePulso = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (!VincularReferencias())
    {
      return;
    }

    bool activo = tiempoActivo > 0f && gameObject.activeInHierarchy;
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
    if (!VincularReferencias())
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

  private bool VincularReferencias()
  {
    if (raizCanvas == null)
    {
      Canvas canvas = GetComponent<Canvas>();
      if (canvas == null)
      {
        canvas = GetComponentInChildren<Canvas>();
      }

      raizCanvas = canvas != null ? canvas.GetComponent<RectTransform>() : GetComponent<RectTransform>();
    }

    if (raizCanvas == null)
    {
      return false;
    }

    if (referenciaVisual == null || referenciaVisual == overlayRoot)
    {
      Graphic[] graficos = GetComponentsInChildren<Graphic>(true);
      for (int i = 0; i < graficos.Length; i++)
      {
        RectTransform candidato = graficos[i] != null ? graficos[i].rectTransform : null;
        if (candidato == null || candidato == overlayRoot)
        {
          continue;
        }

        referenciaVisual = candidato;
        break;
      }
    }

    if (referenciaVisual == null)
    {
      referenciaVisual = raizCanvas;
    }

    return referenciaVisual != null;
  }

  private void AsegurarOverlay()
  {
    if (raizCanvas == null || overlayRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("TrapSpawnLightFx", typeof(RectTransform), typeof(CanvasGroup));
    overlayRoot = rootGo.GetComponent<RectTransform>();
    overlayGroup = rootGo.GetComponent<CanvasGroup>();
    overlayRoot.SetParent(raizCanvas, false);
    overlayRoot.SetAsLastSibling();
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
    if (overlayRoot == null || overlayGroup == null || referenciaVisual == null)
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
    float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progreso / 0.18f));
    float fadeOut = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((progreso - 0.56f) / 0.44f));
    float envolvente = fadeIn * fadeOut * Mathf.Clamp01(intensidad);
    float pulso = 0.92f + (0.08f * Mathf.Sin((Time.time * 4f) + fasePulso));

    Vector2 tamanoBase = ObtenerTamanoReferencia();
    float ancho = Mathf.Max(14f, tamanoBase.x * factorTamano);
    float alto = Mathf.Max(34f, tamanoBase.y * factorTamano);
    float derivaY = Mathf.Lerp(0f, alto * 0.08f, progreso);

    overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayRoot.anchoredPosition = referenciaVisual.anchoredPosition + new Vector2(0f, (tamanoBase.y * 0.2f) + desplazamientoY);
    overlayRoot.localEulerAngles = Vector3.zero;
    overlayRoot.localScale = Vector3.one;
    overlayRoot.sizeDelta = new Vector2(ancho * 1.95f, alto * 1.9f);

    overlayGroup.alpha = envolvente;

    ConfigurarCapa(
      hazExterior,
      new Vector2(0f, (-alto * 0.32f) + derivaY),
      new Vector2(ancho * 0.98f * pulso, alto * 2.18f),
      EscalarAlpha(colorHazExterior, 0.94f + ((1f - progreso) * 0.08f)));

    ConfigurarCapa(
      hazInterior,
      new Vector2(0f, (-alto * 0.28f) + (derivaY * 1.1f)),
      new Vector2(ancho * 0.46f * pulso, alto * 1.82f),
      EscalarAlpha(colorHazInterior, 1f + (0.06f * pulso)));

    ConfigurarCapa(
      resplandorBase,
      new Vector2(0f, -alto * 0.18f),
      new Vector2(ancho * 0.9f * (1f + (0.06f * pulso)), alto * 0.42f),
      EscalarAlpha(colorResplandorBase, 0.88f + (0.1f * pulso)));

    ConfigurarCapa(
      haloCentral,
      new Vector2(0f, (alto * 0.14f) + (derivaY * 0.7f)),
      new Vector2(ancho * 0.74f, alto * 0.74f),
      EscalarAlpha(colorHaloCentral, 0.9f + (0.08f * pulso)));

    if (tiempoActivo <= 0f)
    {
      tiempoActivo = -1f;
    }
  }

  private Vector2 ObtenerTamanoReferencia()
  {
    if (referenciaVisual == null)
    {
      return new Vector2(24f, 64f);
    }

    Vector2 tamano = referenciaVisual.rect.size;
    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = referenciaVisual.sizeDelta;
    }

    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = new Vector2(24f, 64f);
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
    texturaSuave.name = "TrapSpawnLightSoftRuntime";
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
    spriteSuave.name = "TrapSpawnLightSoftRuntime";
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
    texturaHaz.name = "TrapSpawnLightBeamRuntime";
    texturaHaz.wrapMode = TextureWrapMode.Clamp;
    texturaHaz.filterMode = FilterMode.Bilinear;
    texturaHaz.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroX = (width - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      float ny = y / (height - 1f);
      float anchoHaz = Mathf.Lerp(1.02f, 0.16f, ny);
      float vertical = Mathf.SmoothStep(0f, 1f, ny) * (0.84f + (0.16f * (1f - ny)));
      float desvanecimientoSuperior = 1f - Mathf.SmoothStep(0.74f, 1f, ny);
      vertical *= desvanecimientoSuperior;
      float redondeoPunta = 1f - Mathf.SmoothStep(0.8f, 1f, ny);

      for (int x = 0; x < width; x++)
      {
        float nx = Mathf.Abs((x - centroX) / (centroX * anchoHaz));
        float lateral = Mathf.Clamp01(1f - nx);
        lateral = Mathf.Pow(lateral, ny > 0.8f ? 2.1f : 3.3f);
        float alpha = lateral * vertical;
        if (ny > 0.76f)
        {
          float cierreSuperior = Mathf.Clamp01(1f - (nx * Mathf.Lerp(0.24f, 1.26f, 1f - redondeoPunta)));
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
    spriteHaz.name = "TrapSpawnLightBeamRuntime";
    return spriteHaz;
  }
}
