using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public  class PilarDeLuz : Obstaculo
{
  [Header("Glow Permanente")]
  [SerializeField] private float intensidadGlow = 0.9f;
  [SerializeField] private float velocidadPulsoGlow = 1.35f;
  [SerializeField] private float escalaAnchoHazGlow = 1.35f;
  [SerializeField] private float escalaAlturaHazGlow = 1.0f;
  [SerializeField] private Color colorHazExteriorGlow = new Color(1f, 0.9f, 0.32f, 0.3f);
  [SerializeField] private Color colorHazInteriorGlow = new Color(1f, 0.97f, 0.72f, 0.42f);
  [SerializeField] private Color colorResplandorBaseGlow = new Color(1f, 0.86f, 0.16f, 0.32f);
  [SerializeField] private Color colorHaloCentralGlow = new Color(1f, 0.98f, 0.64f, 0.25f);
  [SerializeField] private Color colorSiluetaExteriorGlow = new Color(1f, 0.89f, 0.28f, 0.25f);
  [SerializeField] private Color colorSiluetaInteriorGlow = new Color(1f, 0.97f, 0.72f, 0.20f);

  public int NIVEL = 1;
  public ClasePurificadora scCreador;

  private Image imagenPilar;
  private RectTransform rectImagenPilar;
  private RectTransform overlayGlowRoot;
  private CanvasGroup overlayGlowGroup;
  private Image glowSiluetaExterior;
  private Image glowSiluetaInterior;
  private Image glowHazExterior;
  private Image glowHazInterior;
  private Image glowResplandorBase;
  private Image glowHaloCentral;
  private float fasePulsoGlow;
  private bool fasePulsoInicializada;

  private static Sprite spriteSuaveGlow;
  private static Texture2D texturaSuaveGlow;
  private static Sprite spriteHazGlow;
  private static Texture2D texturaHazGlow;

  public override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante)
  {
    base.RecibirDanio(danio, tipoDanio, esCritico, uCausante);

    if (uCausante != null)
    {
      int dam = UnityEngine.Random.Range(1, 7) + (int)scCreador.mod_CarPoder;
      if (NIVEL > 2) { dam += 3; }
      if (uCausante.TieneTag("Nomuerto") || uCausante.TieneTag("Etereo"))
      {
        dam = dam * 2;
      }

      uCausante.RecibirDanio(dam, 11, false, null);
    }
  }

  private void OnEnable()
  {
    if (!fasePulsoInicializada)
    {
      fasePulsoGlow = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
      fasePulsoInicializada = true;
    }

    if (overlayGlowRoot != null)
    {
      overlayGlowRoot.gameObject.SetActive(true);
    }
  }

  private void LateUpdate()
  {
    if (!VincularImagenPilar())
    {
      return;
    }

    AsegurarOverlayGlow();
    ActualizarOverlayGlow();
  }

  private void OnDisable()
  {
    if (overlayGlowRoot != null)
    {
      overlayGlowRoot.gameObject.SetActive(false);
    }
  }

  private void OnDestroy()
  {
    if (overlayGlowRoot != null)
    {
      Destroy(overlayGlowRoot.gameObject);
    }
  }

  private bool VincularImagenPilar()
  {
    if (imagenPilar == null)
    {
      Transform canvas = transform.Find("Canvas");
      Transform imagen = canvas != null ? canvas.Find("Image") : null;
      if (imagen != null)
      {
        imagenPilar = imagen.GetComponent<Image>();
      }

      if (imagenPilar == null)
      {
        foreach (Image candidata in GetComponentsInChildren<Image>(true))
        {
          if (candidata == null || candidata.sprite == null)
          {
            continue;
          }

          if (candidata.gameObject.name == "Image" || candidata.GetComponent<UIGlowPulse>() != null)
          {
            imagenPilar = candidata;
            break;
          }
        }
      }
    }

    if (imagenPilar == null)
    {
      return false;
    }

    RectTransform rect = imagenPilar.rectTransform;
    if (rect == null || rect.parent == null)
    {
      return false;
    }

    if (rectImagenPilar != rect)
    {
      rectImagenPilar = rect;
      if (overlayGlowRoot != null && overlayGlowRoot.parent != rectImagenPilar.parent)
      {
        Destroy(overlayGlowRoot.gameObject);
        overlayGlowRoot = null;
        overlayGlowGroup = null;
        glowSiluetaExterior = null;
        glowSiluetaInterior = null;
        glowHazExterior = null;
        glowHazInterior = null;
        glowResplandorBase = null;
        glowHaloCentral = null;
      }
    }

    return rectImagenPilar != null;
  }

  private void AsegurarOverlayGlow()
  {
    if (rectImagenPilar == null || overlayGlowRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("PilarDeLuzGlow", typeof(RectTransform), typeof(CanvasGroup));
    overlayGlowRoot = rootGo.GetComponent<RectTransform>();
    overlayGlowGroup = rootGo.GetComponent<CanvasGroup>();
    overlayGlowRoot.SetParent(rectImagenPilar.parent, false);
    overlayGlowGroup.interactable = false;
    overlayGlowGroup.blocksRaycasts = false;

    glowSiluetaExterior = CrearImagenGlow("SiluetaExterior", overlayGlowRoot, null, new Vector2(0.5f, 0.5f));
    glowSiluetaInterior = CrearImagenGlow("SiluetaInterior", overlayGlowRoot, null, new Vector2(0.5f, 0.5f));
    glowHazExterior = CrearImagenGlow("HazExterior", overlayGlowRoot, ObtenerSpriteHazGlow(), new Vector2(0.5f, 0f));
    glowHazInterior = CrearImagenGlow("HazInterior", overlayGlowRoot, ObtenerSpriteHazGlow(), new Vector2(0.5f, 0f));
    glowResplandorBase = CrearImagenGlow("ResplandorBase", overlayGlowRoot, ObtenerSpriteSuaveGlow(), new Vector2(0.5f, 0.5f));
    glowHaloCentral = CrearImagenGlow("HaloCentral", overlayGlowRoot, ObtenerSpriteSuaveGlow(), new Vector2(0.5f, 0.5f));

    if (glowSiluetaExterior != null) { glowSiluetaExterior.preserveAspect = true; }
    if (glowSiluetaInterior != null) { glowSiluetaInterior.preserveAspect = true; }
    if (glowHazExterior != null) { glowHazExterior.preserveAspect = false; }
    if (glowHazInterior != null) { glowHazInterior.preserveAspect = false; }
    if (glowResplandorBase != null) { glowResplandorBase.preserveAspect = true; }
    if (glowHaloCentral != null) { glowHaloCentral.preserveAspect = true; }
  }

  private void ActualizarOverlayGlow()
  {
    if (overlayGlowRoot == null || overlayGlowGroup == null || rectImagenPilar == null)
    {
      return;
    }

    if (!overlayGlowRoot.gameObject.activeSelf)
    {
      overlayGlowRoot.gameObject.SetActive(true);
    }

    float pulso = 0.92f + (0.08f * Mathf.Sin((Time.time * velocidadPulsoGlow) + fasePulsoGlow));
    float respiracion = 0.94f + (0.06f * Mathf.Sin((Time.time * (velocidadPulsoGlow * 0.63f)) + (fasePulsoGlow * 0.7f)));
    float derivaY = 0.18f * Mathf.Sin((Time.time * 0.55f) + fasePulsoGlow);

    Vector2 tamano = rectImagenPilar.rect.size;
    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = rectImagenPilar.sizeDelta;
    }
    if (tamano.x <= 0.01f || tamano.y <= 0.01f)
    {
      tamano = new Vector2(32f, 42f);
    }

    float ancho = Mathf.Max(18f, tamano.x);
    float alto = Mathf.Max(24f, tamano.y);
    float alturaHaz = alto * escalaAlturaHazGlow;

    overlayGlowRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayGlowRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayGlowRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayGlowRoot.anchoredPosition = rectImagenPilar.anchoredPosition;
    overlayGlowRoot.localEulerAngles = Vector3.zero;
    overlayGlowRoot.localScale = rectImagenPilar.localScale;
    overlayGlowRoot.sizeDelta = new Vector2(ancho * 2.05f, alturaHaz * 3.05f);

    int sibling = rectImagenPilar.GetSiblingIndex();
    int targetSibling = sibling + 1;
    if (overlayGlowRoot.GetSiblingIndex() != targetSibling)
    {
      overlayGlowRoot.SetSiblingIndex(targetSibling);
    }

    overlayGlowGroup.alpha = Mathf.Clamp01(intensidadGlow * (0.92f + (0.08f * pulso)));

    ConfigurarSiluetaGlow(
      glowSiluetaExterior,
      new Vector2(0f, (alto * 0.03f) + (derivaY * 0.35f)),
      new Vector2(ancho * 1.52f * respiracion, alto * 1.66f * (0.98f + (0.06f * pulso))),
      EscalarAlphaGlow(colorSiluetaExteriorGlow, 1.05f + (0.18f * pulso)));

    ConfigurarSiluetaGlow(
      glowSiluetaInterior,
      new Vector2(0f, (alto * 0.02f) + (derivaY * 0.24f)),
      new Vector2(ancho * 1.28f * pulso, alto * 1.42f * (0.99f + (0.05f * respiracion))),
      EscalarAlphaGlow(colorSiluetaInteriorGlow, 1.08f + (0.2f * respiracion)));

    ConfigurarCapaGlow(
      glowHazExterior,
      new Vector2(0f, (-alturaHaz * 0.61f) + derivaY),
      new Vector2(ancho * escalaAnchoHazGlow * 1.48f * respiracion, alturaHaz * 2.92f),
      EscalarAlphaGlow(colorHazExteriorGlow, 1.08f + (0.2f * pulso)));

    ConfigurarCapaGlow(
      glowHazInterior,
      new Vector2(0f, (-alturaHaz * 0.58f) + (derivaY * 1.2f)),
      new Vector2(ancho * escalaAnchoHazGlow * 1.02f * pulso, alturaHaz * 2.48f),
      EscalarAlphaGlow(colorHazInteriorGlow, 1.1f + (0.22f * respiracion)));

    ConfigurarCapaGlow(
      glowResplandorBase,
      new Vector2(0f, -alturaHaz * 0.33f),
      new Vector2(ancho * 1.48f * (1f + (0.08f * pulso)), alturaHaz * 0.72f),
      EscalarAlphaGlow(colorResplandorBaseGlow, 1.08f + (0.22f * pulso)));

    ConfigurarCapaGlow(
      glowHaloCentral,
      new Vector2(0f, (alturaHaz * 0.02f) + (derivaY * 0.85f)),
      new Vector2(ancho * 1.18f, alturaHaz * 1.42f),
      EscalarAlphaGlow(colorHaloCentralGlow, 1.06f + (0.2f * respiracion)));
  }

  private void ConfigurarSiluetaGlow(Image image, Vector2 posicion, Vector2 tamano, Color color)
  {
    if (image == null || imagenPilar == null)
    {
      return;
    }

    image.sprite = imagenPilar.sprite;
    image.overrideSprite = imagenPilar.overrideSprite;
    image.type = imagenPilar.type;
    image.preserveAspect = true;

    ConfigurarCapaGlow(image, posicion, tamano, color);
  }

  private static Image CrearImagenGlow(string nombre, RectTransform padre, Sprite sprite, Vector2 pivot)
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

  private static void ConfigurarCapaGlow(Image image, Vector2 posicion, Vector2 tamano, Color color)
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

  private static Color EscalarAlphaGlow(Color color, float multiplicador)
  {
    color.a *= multiplicador;
    return color;
  }

  private static Sprite ObtenerSpriteSuaveGlow()
  {
    if (spriteSuaveGlow != null)
    {
      return spriteSuaveGlow;
    }

    const int size = 64;
    texturaSuaveGlow = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuaveGlow.name = "PilarDeLuzSoftGlowRuntime";
    texturaSuaveGlow.wrapMode = TextureWrapMode.Clamp;
    texturaSuaveGlow.filterMode = FilterMode.Bilinear;
    texturaSuaveGlow.hideFlags = HideFlags.HideAndDontSave;

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

    texturaSuaveGlow.SetPixels(pixels);
    texturaSuaveGlow.Apply(false, true);
    spriteSuaveGlow = Sprite.Create(
      texturaSuaveGlow,
      new Rect(0f, 0f, size, size),
      new Vector2(0.5f, 0.5f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteSuaveGlow.name = "PilarDeLuzSoftGlowRuntime";
    return spriteSuaveGlow;
  }

  private static Sprite ObtenerSpriteHazGlow()
  {
    if (spriteHazGlow != null)
    {
      return spriteHazGlow;
    }

    const int width = 64;
    const int height = 128;
    texturaHazGlow = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaHazGlow.name = "PilarDeLuzBeamGlowRuntime";
    texturaHazGlow.wrapMode = TextureWrapMode.Clamp;
    texturaHazGlow.filterMode = FilterMode.Bilinear;
    texturaHazGlow.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroX = (width - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      float ny = y / (height - 1f);
      float anchoHaz = Mathf.Lerp(1.02f, 0.18f, ny);
      float vertical = Mathf.SmoothStep(0f, 1f, ny) * (0.87f + (0.13f * (1f - ny)));
      float desvanecimientoSuperior = 1f - Mathf.SmoothStep(0.8f, 1f, ny);
      vertical *= desvanecimientoSuperior;
      float redondeoPunta = 1f - Mathf.SmoothStep(0.84f, 1f, ny);

      for (int x = 0; x < width; x++)
      {
        float nx = Mathf.Abs((x - centroX) / (centroX * anchoHaz));
        float lateral = Mathf.Clamp01(1f - nx);
        lateral = Mathf.Pow(lateral, ny > 0.84f ? 2f : 3.4f);
        float alpha = lateral * vertical;
        if (ny > 0.8f)
        {
          float cierreSuperior = Mathf.Clamp01(1f - (nx * Mathf.Lerp(0.3f, 1.28f, 1f - redondeoPunta)));
          alpha *= cierreSuperior;
        }

        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaHazGlow.SetPixels(pixels);
    texturaHazGlow.Apply(false, true);
    spriteHazGlow = Sprite.Create(
      texturaHazGlow,
      new Rect(0f, 0f, width, height),
      new Vector2(0.5f, 0f),
      100f,
      0,
      SpriteMeshType.FullRect);
    spriteHazGlow.name = "PilarDeLuzBeamGlowRuntime";
    return spriteHazGlow;
  }
}
