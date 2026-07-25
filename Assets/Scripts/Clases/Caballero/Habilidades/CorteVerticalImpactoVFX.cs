using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Impacto ligero para el ataque basico del Caballero: una hoja luminosa descendente,
/// una estela corta y pequenas esquirlas en el punto de contacto.
/// </summary>
public sealed class CorteVerticalImpactoVFX : MonoBehaviour
{
  private const float Duracion = 0.38f;
  private const int CantidadEsquirlas = 6;

  private RectTransform raiz;
  private CanvasGroup grupoCanvas;
  private Image estela;
  private Image hoja;
  private Image nucleo;
  private Image[] esquirlas;
  private Vector2[] direccionesEsquirlas;
  private Vector2 tamanoBase;
  private float tiempo;

  private static Sprite spriteSuave;
  private static Sprite spriteTrazo;
  private static Texture2D texturaSuave;
  private static Texture2D texturaTrazo;

  public static void Crear(GameObject objetivo)
  {
    if (objetivo == null)
    {
      return;
    }

    Unidad unidad = objetivo.GetComponent<Unidad>();
    RectTransform imagenUnidad = unidad != null && unidad.uImage != null ? unidad.uImage.rectTransform : null;
    Canvas canvas = imagenUnidad != null
      ? imagenUnidad.GetComponentInParent<Canvas>(true)
      : objetivo.GetComponentInChildren<Canvas>(true);

    RectTransform padre = imagenUnidad != null ? imagenUnidad.parent as RectTransform : canvas != null ? canvas.transform as RectTransform : null;
    if (padre == null)
    {
      return;
    }

    GameObject go = new GameObject("VFX_CorteVerticalModerno", typeof(RectTransform), typeof(CanvasGroup), typeof(CorteVerticalImpactoVFX));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Vector2 tamano = ObtenerTamano(imagenUnidad);
    rect.sizeDelta = new Vector2(Mathf.Max(24f, tamano.x * 0.53f), Mathf.Max(32f, tamano.y * 0.73f));
    rect.anchoredPosition = imagenUnidad != null
      ? imagenUnidad.anchoredPosition - new Vector2(0f, tamano.y * 0.04f)
      : Vector2.zero;

    if (imagenUnidad != null)
    {
      rect.SetSiblingIndex(Mathf.Min(imagenUnidad.GetSiblingIndex() + 2, padre.childCount - 1));
    }

    go.GetComponent<CorteVerticalImpactoVFX>().Inicializar(rect.sizeDelta);
  }

  private void Inicializar(Vector2 tamano)
  {
    raiz = GetComponent<RectTransform>();
    grupoCanvas = GetComponent<CanvasGroup>();
    grupoCanvas.interactable = false;
    grupoCanvas.blocksRaycasts = false;
    tamanoBase = tamano;

    Sprite suave = ObtenerSpriteSuave();
    Sprite trazo = ObtenerSpriteTrazo();
    estela = CrearCapa("Estela", trazo);
    hoja = CrearCapa("Hoja", trazo);
    nucleo = CrearCapa("Nucleo", trazo);

    esquirlas = new Image[CantidadEsquirlas];
    direccionesEsquirlas = new Vector2[CantidadEsquirlas];
    for (int i = 0; i < CantidadEsquirlas; i++)
    {
      esquirlas[i] = CrearCapa("Esquirla" + i, suave);
      float angulo = Mathf.Lerp(205f, 335f, i / (CantidadEsquirlas - 1f)) * Mathf.Deg2Rad;
      direccionesEsquirlas[i] = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo) * 0.7f);
    }
  }

  private void Update()
  {
    tiempo += Time.unscaledDeltaTime;
    float progreso = Mathf.Clamp01(tiempo / Duracion);
    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progreso / 0.16f));
    float salida = 1f - Mathf.SmoothStep(0.42f, 1f, progreso);
    grupoCanvas.alpha = Mathf.Min(1f, entrada * 1.2f) * salida;

    float ancho = tamanoBase.x;
    float alto = tamanoBase.y;
    float recorrido = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progreso / 0.58f));
    float posicionY = Mathf.Lerp(alto * 0.27f, -alto * 0.22f, recorrido);
    float golpe = 1f - Mathf.SmoothStep(0.38f, 0.76f, progreso);

    ConfigurarCapa(estela, new Vector2(-ancho * 0.035f, posicionY + alto * 0.14f), new Vector2(ancho * 0.14f, alto * 0.74f), new Color(0.12f, 0.55f, 0.8f, 0.28f * golpe), -8f);
    ConfigurarCapa(hoja, new Vector2(0f, posicionY), new Vector2(ancho * 0.075f, alto * 0.63f), new Color(0.46f, 0.88f, 1f, 0.88f * golpe), -8f);
    ConfigurarCapa(nucleo, new Vector2(ancho * 0.005f, posicionY), new Vector2(ancho * 0.026f, alto * 0.59f), new Color(0.96f, 1f, 1f, 0.96f * golpe), -8f);
    for (int i = 0; i < esquirlas.Length; i++)
    {
      float retraso = i * 0.018f;
      float local = Mathf.Clamp01((progreso - 0.38f - retraso) / 0.42f);
      float visible = Mathf.Sin(local * Mathf.PI);
      Vector2 posicion = new Vector2(0f, -alto * 0.22f) + direccionesEsquirlas[i] * (ancho * Mathf.Lerp(0.04f, 0.42f, local));
      float tamano = Mathf.Lerp(ancho * 0.07f, ancho * 0.018f, local);
      ConfigurarCapa(esquirlas[i], posicion, Vector2.one * tamano, new Color(0.68f, 0.94f, 1f, visible * 0.82f));
    }

    if (progreso >= 1f)
    {
      Destroy(gameObject);
    }
  }

  private Image CrearCapa(string nombre, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(raiz, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image imagen = go.GetComponent<Image>();
    imagen.sprite = sprite;
    imagen.raycastTarget = false;
    imagen.maskable = false;
    return imagen;
  }

  private static Vector2 ObtenerTamano(RectTransform rect)
  {
    if (rect == null)
    {
      return new Vector2(64f, 88f);
    }

    Vector2 tamano = rect.rect.size;
    return tamano.x > 0.01f && tamano.y > 0.01f ? tamano : new Vector2(64f, 88f);
  }

  private static void ConfigurarCapa(Image imagen, Vector2 posicion, Vector2 tamano, Color color, float rotacion = 0f)
  {
    RectTransform rect = imagen.rectTransform;
    rect.anchoredPosition = posicion;
    rect.sizeDelta = tamano;
    rect.localEulerAngles = new Vector3(0f, 0f, rotacion);
    imagen.color = color;
  }

  private static Sprite ObtenerSpriteSuave()
  {
    if (spriteSuave != null)
    {
      return spriteSuave;
    }

    const int tamano = 64;
    texturaSuave = new Texture2D(tamano, tamano, TextureFormat.ARGB32, false);
    texturaSuave.hideFlags = HideFlags.HideAndDontSave;
    texturaSuave.filterMode = FilterMode.Bilinear;
    Color[] pixeles = new Color[tamano * tamano];
    float centro = (tamano - 1) * 0.5f;
    for (int y = 0; y < tamano; y++)
    {
      for (int x = 0; x < tamano; x++)
      {
        float distancia = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro)) / centro;
        pixeles[y * tamano + x] = new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.4f));
      }
    }
    texturaSuave.SetPixels(pixeles);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, tamano, tamano), new Vector2(0.5f, 0.5f), 100f);
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteTrazo()
  {
    if (spriteTrazo != null)
    {
      return spriteTrazo;
    }

    const int ancho = 24;
    const int alto = 128;
    texturaTrazo = new Texture2D(ancho, alto, TextureFormat.ARGB32, false);
    texturaTrazo.hideFlags = HideFlags.HideAndDontSave;
    texturaTrazo.filterMode = FilterMode.Bilinear;
    Color[] pixeles = new Color[ancho * alto];
    float centroX = (ancho - 1) * 0.5f;
    for (int y = 0; y < alto; y++)
    {
      for (int x = 0; x < ancho; x++)
      {
        float lateral = 1f - Mathf.Abs((x - centroX) / centroX);
        float vertical = Mathf.Clamp01((y + 1f) / alto);
        pixeles[y * ancho + x] = new Color(1f, 1f, 1f, Mathf.Pow(lateral, 1.7f) * Mathf.SmoothStep(0f, 0.22f, vertical));
      }
    }
    texturaTrazo.SetPixels(pixeles);
    texturaTrazo.Apply(false, true);
    spriteTrazo = Sprite.Create(texturaTrazo, new Rect(0f, 0f, ancho, alto), new Vector2(0.5f, 0.5f), 100f);
    return spriteTrazo;
  }
}
