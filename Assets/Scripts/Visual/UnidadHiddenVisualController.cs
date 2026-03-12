using UnityEngine;
using UnityEngine.UI;

public sealed class UnidadHiddenVisualController : MonoBehaviour
{
  private const float VelocidadAparicion = 6.4f;
  private const float VelocidadDesaparicion = 9f;

  private Unidad unidad;
  private RectTransform imagenUnidad;
  private RectTransform overlayRoot;
  private CanvasGroup overlayGroup;
  private Image overlayTint;
  private Image overlayHalo;
  private Image overlaySmokeA;
  private Image overlaySmokeB;
  private float visibilidad;
  private float faseHalo;
  private float faseSmokeA;
  private float faseSmokeB;

  private static Sprite spriteSuave;
  private static Texture2D texturaSuave;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
    faseHalo = Random.Range(0f, Mathf.PI * 2f);
    faseSmokeA = Random.Range(0f, Mathf.PI * 2f);
    faseSmokeB = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
      if (unidad == null)
      {
        return;
      }
    }

    unidad.SincronizarVisualEscondido();
    if (!VincularImagenUnidad())
    {
      return;
    }

    bool mostrarOverlay = unidad.ObtenerEstaEscondido() > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy
      && !unidad.EstaOcultoVisualmenteParaJugador();

    if (mostrarOverlay)
    {
      AsegurarOverlay();
    }

    ActualizarOverlay(mostrarOverlay);
  }

  private void OnDisable()
  {
    visibilidad = 0f;
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

  private bool VincularImagenUnidad()
  {
    if (unidad == null || unidad.uImage == null)
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
        overlayTint = null;
        overlayHalo = null;
        overlaySmokeA = null;
        overlaySmokeB = null;
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

    Sprite spriteBase = ObtenerSpriteSuave();

    GameObject rootGo = new GameObject("StatusVfx_Escondido", typeof(RectTransform), typeof(CanvasGroup));
    overlayRoot = rootGo.GetComponent<RectTransform>();
    overlayGroup = rootGo.GetComponent<CanvasGroup>();
    overlayRoot.SetParent(imagenUnidad.parent, false);
    overlayGroup.interactable = false;
    overlayGroup.blocksRaycasts = false;

    overlayTint = CrearImagen("Tint", overlayRoot, null);
    overlayHalo = CrearImagen("Halo", overlayRoot, spriteBase);
    overlaySmokeA = CrearImagen("SmokeA", overlayRoot, spriteBase);
    overlaySmokeB = CrearImagen("SmokeB", overlayRoot, spriteBase);

    overlayRoot.gameObject.SetActive(false);
  }

  private void ActualizarOverlay(bool mostrarOverlay)
  {
    if (overlayRoot == null
      || overlayGroup == null
      || overlayTint == null
      || overlayHalo == null
      || overlaySmokeA == null
      || overlaySmokeB == null)
    {
      return;
    }

    float velocidad = mostrarOverlay ? VelocidadAparicion : VelocidadDesaparicion;
    visibilidad = Mathf.MoveTowards(visibilidad, mostrarOverlay ? 1f : 0f, velocidad * Time.deltaTime);
    bool debeSeguirActivo = visibilidad > 0.001f;

    if (overlayRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo || imagenUnidad == null)
    {
      return;
    }

    overlayRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayRoot.anchoredPosition = imagenUnidad.anchoredPosition;
    overlayRoot.localEulerAngles = imagenUnidad.localEulerAngles;
    overlayRoot.localScale = imagenUnidad.localScale;
    overlayRoot.sizeDelta = ObtenerTamanoUnidad();

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayRoot.parent.childCount - 1);
    if (overlayRoot.GetSiblingIndex() != targetSibling)
    {
      overlayRoot.SetSiblingIndex(targetSibling);
    }

    overlayTint.sprite = unidad.uImage.sprite;
    overlayTint.overrideSprite = unidad.uImage.overrideSprite;
    overlayTint.type = unidad.uImage.type;
    overlayTint.preserveAspect = unidad.uImage.preserveAspect;

    float tiempo = Time.time;
    float pulsoHalo = 0.5f + (0.5f * Mathf.Sin((tiempo * 2.45f) + faseHalo));
    float derivaA = Mathf.Sin((tiempo * 1.22f) + faseSmokeA);
    float derivaB = Mathf.Sin((tiempo * 1.46f) + faseSmokeB);
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(20f, tamano.x * 0.92f);
    float alto = Mathf.Max(24f, tamano.y * 1.02f);

    overlayGroup.alpha = visibilidad;

    ConfigurarCapa(
      overlayTint,
      Vector2.zero,
      tamano * 1.03f,
      new Color(0.01f, 0.01f, 0.02f, Mathf.Lerp(0.62f, 0.8f, pulsoHalo)));

    ConfigurarCapa(
      overlayHalo,
      new Vector2(0f, -alto * 0.02f),
      new Vector2(ancho * (1.16f + (pulsoHalo * 0.08f)), alto * (1.22f + (pulsoHalo * 0.06f))),
      new Color(0f, 0f, 0f, Mathf.Lerp(0.22f, 0.38f, pulsoHalo)));

    ConfigurarCapa(
      overlaySmokeA,
      new Vector2((-ancho * 0.08f) + (derivaA * 2.4f), alto * 0.12f),
      new Vector2(ancho * 0.82f, alto * 0.58f) * (0.92f + (Mathf.Abs(derivaA) * 0.1f)),
      new Color(0.05f, 0.05f, 0.07f, 0.26f),
      derivaA * 8f);

    ConfigurarCapa(
      overlaySmokeB,
      new Vector2((ancho * 0.1f) + (derivaB * 2.1f), -alto * 0.06f),
      new Vector2(ancho * 0.9f, alto * 0.64f) * (0.9f + (Mathf.Abs(derivaB) * 0.12f)),
      new Color(0.07f, 0.07f, 0.09f, 0.22f),
      -6f + (derivaB * 7f));
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

  private static Image CrearImagen(string nombre, RectTransform padre, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.preserveAspect = true;
    return image;
  }

  private static void ConfigurarCapa(Image image, Vector2 posicion, Vector2 tamano, Color color, float rotacionZ = 0f)
  {
    if (image == null)
    {
      return;
    }

    RectTransform rect = image.rectTransform;
    rect.anchoredPosition = posicion;
    rect.sizeDelta = tamano;
    rect.localEulerAngles = new Vector3(0f, 0f, rotacionZ);
    rect.localScale = Vector3.one;
    image.color = color;
  }

  private static Sprite ObtenerSpriteSuave()
  {
    if (spriteSuave != null)
    {
      return spriteSuave;
    }

    const int size = 64;
    texturaSuave = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaSuave.name = "EscondidoParticulaRuntime";
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
        float alpha = Mathf.Pow(borde, 2.2f);
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
    spriteSuave.name = "EscondidoParticulaRuntime";
    return spriteSuave;
  }
}
