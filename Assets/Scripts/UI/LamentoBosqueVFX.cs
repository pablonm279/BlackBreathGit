using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presagio sombrio que aparece sobre las unidades alcanzadas por Lamento del Bosque.
/// El audio original se carga por separado para no depender del antiguo prefab visual.
/// </summary>
public sealed class LamentoBosqueVFX : MonoBehaviour
{
  private const float Duracion = 1.8f;
  private const int CantidadBrumas = 7;
  private const string RutaSfx = "Sonidos/Efectos/lamentodelbosque";

  private RectTransform raiz;
  private CanvasGroup grupoCanvas;
  private Image sombra;
  private Image aro;
  private Image ojoIzquierdo;
  private Image ojoDerecho;
  private Image[] brumas;
  private Image[] espinas;
  private Vector2[] origenBrumas;
  private float tiempo;

  private static Sprite spriteSuave;
  private static Sprite spriteTrazo;
  private static Sprite spriteAro;
  private static Texture2D texturaSuave;
  private static Texture2D texturaTrazo;
  private static Texture2D texturaAro;

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

    GameObject go = new GameObject("VFX_LamentoBosque_Tenebroso", typeof(RectTransform), typeof(CanvasGroup), typeof(AudioSource), typeof(LamentoBosqueVFX));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Vector2 tamanoUnidad = ObtenerTamano(imagenUnidad);
    rect.sizeDelta = new Vector2(Mathf.Max(34f, tamanoUnidad.x * 0.62f), Mathf.Max(24f, tamanoUnidad.y * 0.32f));
    rect.anchoredPosition = imagenUnidad != null
      ? imagenUnidad.anchoredPosition + new Vector2(0f, tamanoUnidad.y * 0.55f)
      : new Vector2(0f, 38f);

    if (imagenUnidad != null)
    {
      rect.SetSiblingIndex(Mathf.Min(imagenUnidad.GetSiblingIndex() + 3, padre.childCount - 1));
    }

    go.GetComponent<LamentoBosqueVFX>().Inicializar();
  }

  private void Inicializar()
  {
    raiz = GetComponent<RectTransform>();
    grupoCanvas = GetComponent<CanvasGroup>();
    grupoCanvas.interactable = false;
    grupoCanvas.blocksRaycasts = false;

    sombra = CrearCapa("SombraEspectral", ObtenerSpriteSuave());
    aro = CrearCapa("AroDeTemor", ObtenerSpriteAro());
    ojoIzquierdo = CrearCapa("OjoIzquierdo", ObtenerSpriteTrazo());
    ojoDerecho = CrearCapa("OjoDerecho", ObtenerSpriteTrazo());

    espinas = new Image[3];
    for (int i = 0; i < espinas.Length; i++)
    {
      espinas[i] = CrearCapa("Espina" + i, ObtenerSpriteTrazo());
    }

    brumas = new Image[CantidadBrumas];
    origenBrumas = new Vector2[CantidadBrumas];
    for (int i = 0; i < CantidadBrumas; i++)
    {
      float distribuido = i / (CantidadBrumas - 1f);
      origenBrumas[i] = new Vector2(Mathf.Lerp(-0.42f, 0.42f, distribuido), Random.Range(-0.12f, 0.09f));
      brumas[i] = CrearCapa("Bruma" + i, ObtenerSpriteSuave());
    }

    AudioSource audio = GetComponent<AudioSource>();
    audio.playOnAwake = false;
    AjustesAudio.AplicarVolumenSfx(audio, 0.841f);
    AudioClip clip = Resources.Load<AudioClip>(RutaSfx);
    if (clip != null)
    {
      audio.clip = clip;
      audio.PlayDelayed(0.05f);
    }
  }

  private void Update()
  {
    tiempo += Time.unscaledDeltaTime;
    float progreso = Mathf.Clamp01(tiempo / Duracion);
    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progreso / 0.15f));
    float salida = 1f - Mathf.SmoothStep(0.58f, 1f, progreso);
    grupoCanvas.alpha = entrada * salida;

    float ancho = raiz.rect.width;
    float alto = raiz.rect.height;
    float pulso = 1f + Mathf.Sin(progreso * Mathf.PI * 3f) * 0.025f;
    raiz.localScale = Vector3.one * pulso;

    Configurar(sombra, new Vector2(0f, alto * 0.02f), new Vector2(ancho * 0.92f, alto * 0.84f), new Color(0.035f, 0.015f, 0.055f, 0.67f));
    Configurar(aro, new Vector2(0f, -alto * 0.28f), new Vector2(ancho * 0.68f, alto * 0.30f), new Color(0.30f, 0.10f, 0.42f, 0.65f), Mathf.Sin(tiempo * 1.7f) * 3f);

    float brilloOjos = 0.72f + Mathf.Sin(tiempo * 8f) * 0.12f;
    Configurar(ojoIzquierdo, new Vector2(-ancho * 0.12f, alto * 0.04f), new Vector2(ancho * 0.17f, alto * 0.075f), new Color(0.76f, 0.48f, 0.92f, brilloOjos), 76f);
    Configurar(ojoDerecho, new Vector2(ancho * 0.12f, alto * 0.04f), new Vector2(ancho * 0.17f, alto * 0.075f), new Color(0.76f, 0.48f, 0.92f, brilloOjos), 104f);

    for (int i = 0; i < espinas.Length; i++)
    {
      float x = (i - 1) * ancho * 0.17f;
      float inclinacion = (i - 1) * -18f;
      float alturaEspina = i == 1 ? alto * 0.42f : alto * 0.33f;
      Configurar(espinas[i], new Vector2(x, alto * 0.37f), new Vector2(ancho * 0.055f, alturaEspina), new Color(0.12f, 0.035f, 0.17f, 0.76f), inclinacion);
    }

    for (int i = 0; i < brumas.Length; i++)
    {
      float fase = tiempo * (0.9f + i * 0.07f) + i * 0.8f;
      Vector2 baseLocal = origenBrumas[i];
      Vector2 posicion = new Vector2(
        (baseLocal.x + Mathf.Sin(fase) * 0.045f) * ancho,
        (baseLocal.y + progreso * 0.24f + Mathf.Cos(fase * 0.7f) * 0.035f) * alto);
      float escala = Mathf.Lerp(0.24f, 0.12f, progreso) * (0.82f + (i % 3) * 0.12f);
      Configurar(brumas[i], posicion, new Vector2(ancho * escala, alto * escala * 1.25f), new Color(0.16f, 0.045f, 0.21f, 0.34f));
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

  private static void Configurar(Image imagen, Vector2 posicion, Vector2 tamano, Color color, float rotacion = 0f)
  {
    RectTransform rect = imagen.rectTransform;
    rect.anchoredPosition = posicion;
    rect.sizeDelta = tamano;
    rect.localEulerAngles = new Vector3(0f, 0f, rotacion);
    imagen.color = color;
  }

  private static Vector2 ObtenerTamano(RectTransform rect)
  {
    if (rect == null || rect.rect.width <= 0.01f || rect.rect.height <= 0.01f)
    {
      return new Vector2(64f, 88f);
    }
    return rect.rect.size;
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
        float dx = (x - centro) / centro;
        float dy = (y - centro) / centro;
        float distancia = Mathf.Sqrt(dx * dx + dy * dy);
        pixeles[y * tamano + x] = new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.2f));
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

    const int ancho = 20;
    const int alto = 96;
    texturaTrazo = new Texture2D(ancho, alto, TextureFormat.ARGB32, false);
    texturaTrazo.hideFlags = HideFlags.HideAndDontSave;
    texturaTrazo.filterMode = FilterMode.Bilinear;
    Color[] pixeles = new Color[ancho * alto];
    float centroX = (ancho - 1) * 0.5f;
    for (int y = 0; y < alto; y++)
    {
      for (int x = 0; x < ancho; x++)
      {
        float lateral = Mathf.Clamp01(1f - Mathf.Abs((x - centroX) / centroX));
        float punta = Mathf.Clamp01(1f - Mathf.Abs((y - alto * 0.52f) / (alto * 0.52f)));
        pixeles[y * ancho + x] = new Color(1f, 1f, 1f, lateral * punta);
      }
    }
    texturaTrazo.SetPixels(pixeles);
    texturaTrazo.Apply(false, true);
    spriteTrazo = Sprite.Create(texturaTrazo, new Rect(0f, 0f, ancho, alto), new Vector2(0.5f, 0.5f), 100f);
    return spriteTrazo;
  }

  private static Sprite ObtenerSpriteAro()
  {
    if (spriteAro != null)
    {
      return spriteAro;
    }

    const int tamano = 64;
    texturaAro = new Texture2D(tamano, tamano, TextureFormat.ARGB32, false);
    texturaAro.hideFlags = HideFlags.HideAndDontSave;
    texturaAro.filterMode = FilterMode.Bilinear;
    Color[] pixeles = new Color[tamano * tamano];
    float centro = (tamano - 1) * 0.5f;
    for (int y = 0; y < tamano; y++)
    {
      for (int x = 0; x < tamano; x++)
      {
        float dx = (x - centro) / centro;
        float dy = (y - centro) / centro;
        float distancia = Mathf.Sqrt(dx * dx + dy * dy);
        float borde = Mathf.Clamp01(1f - Mathf.Abs(distancia - 0.72f) / 0.14f);
        pixeles[y * tamano + x] = new Color(1f, 1f, 1f, borde * borde);
      }
    }
    texturaAro.SetPixels(pixeles);
    texturaAro.Apply(false, true);
    spriteAro = Sprite.Create(texturaAro, new Rect(0f, 0f, tamano, tamano), new Vector2(0.5f, 0.5f), 100f);
    return spriteAro;
  }
}
