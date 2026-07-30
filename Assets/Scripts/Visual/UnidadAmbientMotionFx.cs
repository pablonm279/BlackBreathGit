using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(650)]
[DisallowMultipleComponent]
public sealed class UnidadAmbientMotionFx : MonoBehaviour
{
  private const string RootName = "IdleLifeFx";

  [Header("Silueta secundaria")]
  [SerializeField] private float alphaSilueta = 0.045f;
  [SerializeField] private float amplitudVertical = 1.4f;
  [SerializeField] private float amplitudEscala = 0.008f;
  [SerializeField] private float velocidadRespiracion = 0.72f;

  [Header("Sombra de contacto")]
  [SerializeField] private float alphaSombra = 0.14f;
  [SerializeField] private Vector2 escalaSombra = new Vector2(0.48f, 0.075f);
  [SerializeField] private float offsetSombraY = -0.43f;

  [Header("Transiciones")]
  [SerializeField] private float velocidadAparicion = 4.5f;
  [SerializeField] private float velocidadDesaparicion = 8f;
  [SerializeField] private float duracionPulsoTurno = 0.42f;

  private Unidad unidad;
  private Image imagenUnidad;
  private RectTransform imagenRect;
  private RectTransform rootRect;
  private Image silueta;
  private Image sombra;
  private float fase;
  private float visibilidad;
  private float pulsoTurnoRestante = -1f;

  private static Texture2D texturaSombra;
  private static Sprite spriteSombra;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
    fase = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (!BattleVisualJuice.Enabled || !VincularImagen())
    {
      Ocultar();
      return;
    }

    AsegurarCapa();
    if (rootRect == null || silueta == null || sombra == null)
    {
      return;
    }

    bool mostrar = DebeMostrarVidaAmbiental();
    float velocidad = mostrar ? velocidadAparicion : velocidadDesaparicion;
    visibilidad = Mathf.MoveTowards(visibilidad, mostrar ? 1f : 0f, Time.unscaledDeltaTime * velocidad);

    if (visibilidad <= 0.001f)
    {
      Ocultar();
      return;
    }

    if (!rootRect.gameObject.activeSelf)
    {
      rootRect.gameObject.SetActive(true);
    }

    SincronizarConImagen();
    AnimarCapa();
  }

  private void OnDisable()
  {
    visibilidad = 0f;
    pulsoTurnoRestante = -1f;
    Ocultar();
  }

  private void OnDestroy()
  {
    if (rootRect != null)
    {
      Destroy(rootRect.gameObject);
    }
  }

  public void ReproducirReboteTurnoNuevo()
  {
    if (!BattleVisualJuice.Enabled)
    {
      return;
    }

    pulsoTurnoRestante = Mathf.Max(0.01f, duracionPulsoTurno);
  }

  private bool VincularImagen()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
    }

    Image actual = unidad != null ? unidad.uImage : null;
    RectTransform actualRect = actual != null ? actual.rectTransform : null;
    if (actual == null || actualRect == null || actualRect.parent == null)
    {
      imagenUnidad = null;
      imagenRect = null;
      return false;
    }

    if (imagenUnidad != actual || imagenRect != actualRect)
    {
      imagenUnidad = actual;
      imagenRect = actualRect;
      if (rootRect != null)
      {
        Destroy(rootRect.gameObject);
        rootRect = null;
        silueta = null;
        sombra = null;
      }
    }

    return true;
  }

  private bool DebeMostrarVidaAmbiental()
  {
    if (unidad == null || imagenUnidad == null || BattleManager.Instance == null)
    {
      return false;
    }

    if (!unidad.gameObject.activeInHierarchy || unidad.HP_actual <= 0f || unidad.movimientoEnCurso)
    {
      return false;
    }

    if (unidad.unidadVoladora || unidad.estado_Volando || unidad.esInmobil || unidad.esEtereo)
    {
      return false;
    }

    return unidad.estado_congelado <= 0 && unidad.estado_aturdido <= 0;
  }

  private void AsegurarCapa()
  {
    if (rootRect != null || imagenRect == null || imagenRect.parent == null)
    {
      return;
    }

    GameObject root = new GameObject(RootName, typeof(RectTransform));
    rootRect = root.GetComponent<RectTransform>();
    rootRect.SetParent(imagenRect.parent, false);
    rootRect.anchorMin = imagenRect.anchorMin;
    rootRect.anchorMax = imagenRect.anchorMax;
    rootRect.pivot = imagenRect.pivot;

    sombra = CrearImagen("ContactShadow", rootRect, ObtenerSpriteSombra());
    sombra.preserveAspect = false;

    silueta = CrearImagen("BreathingRim", rootRect, imagenUnidad.sprite);
    silueta.preserveAspect = imagenUnidad.preserveAspect;
    silueta.type = imagenUnidad.type;
    silueta.fillCenter = imagenUnidad.fillCenter;

    rootRect.gameObject.SetActive(false);
  }

  private static Image CrearImagen(string nombre, Transform padre, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = true;
    return image;
  }

  private void SincronizarConImagen()
  {
    rootRect.anchorMin = imagenRect.anchorMin;
    rootRect.anchorMax = imagenRect.anchorMax;
    rootRect.pivot = imagenRect.pivot;
    rootRect.anchoredPosition = imagenRect.anchoredPosition;
    rootRect.sizeDelta = imagenRect.sizeDelta;
    rootRect.localScale = imagenRect.localScale;
    rootRect.localEulerAngles = imagenRect.localEulerAngles;

    int indiceImagen = imagenRect.GetSiblingIndex();
    int indiceRoot = rootRect.GetSiblingIndex();
    if (indiceRoot != indiceImagen - 1)
    {
      int indiceObjetivo = indiceRoot < indiceImagen ? indiceImagen - 1 : indiceImagen;
      rootRect.SetSiblingIndex(Mathf.Clamp(indiceObjetivo, 0, imagenRect.parent.childCount - 1));
    }

    if (silueta.sprite != imagenUnidad.sprite)
    {
      silueta.sprite = imagenUnidad.sprite;
    }

    silueta.preserveAspect = imagenUnidad.preserveAspect;
    silueta.type = imagenUnidad.type;
    silueta.fillCenter = imagenUnidad.fillCenter;
  }

  private void AnimarCapa()
  {
    float t = Time.unscaledTime * Mathf.Max(0.01f, velocidadRespiracion) + fase;
    float respiracion = Mathf.Sin(t);
    float secundaria = Mathf.Sin((t * 0.47f) + fase * 0.31f);
    float pulsoTurno = CalcularPulsoTurno();

    RectTransform siluetaRect = silueta.rectTransform;
    siluetaRect.sizeDelta = imagenRect.rect.size;
    siluetaRect.anchoredPosition = new Vector2(
      secundaria * 0.35f,
      respiracion * amplitudVertical + pulsoTurno * 2.2f);
    float escala = 1.006f + respiracion * amplitudEscala + pulsoTurno * 0.012f;
    siluetaRect.localScale = Vector3.one * escala;
    siluetaRect.localEulerAngles = Vector3.zero;

    Color colorRim = EsUnidadActiva()
      ? new Color(0.95f, 0.78f, 0.25f, 1f)
      : new Color(0.32f, 0.55f, 0.62f, 1f);
    colorRim.a = visibilidad * (alphaSilueta + pulsoTurno * 0.075f);
    silueta.color = colorRim;

    RectTransform sombraRect = sombra.rectTransform;
    Vector2 tamano = imagenRect.rect.size;
    sombraRect.sizeDelta = new Vector2(
      Mathf.Max(8f, tamano.x * escalaSombra.x),
      Mathf.Max(3f, tamano.y * escalaSombra.y));
    sombraRect.anchoredPosition = new Vector2(0f, tamano.y * offsetSombraY);
    sombraRect.localEulerAngles = Vector3.zero;
    float contraccion = 1f - respiracion * 0.035f;
    sombraRect.localScale = new Vector3(contraccion, 1f, 1f);
    sombra.color = new Color(0.01f, 0.015f, 0.02f, visibilidad * alphaSombra);
  }

  private float CalcularPulsoTurno()
  {
    if (pulsoTurnoRestante <= 0f)
    {
      return 0f;
    }

    float duracion = Mathf.Max(0.01f, duracionPulsoTurno);
    pulsoTurnoRestante = Mathf.Max(0f, pulsoTurnoRestante - Time.unscaledDeltaTime);
    float progreso = 1f - pulsoTurnoRestante / duracion;
    return Mathf.Sin(Mathf.Clamp01(progreso) * Mathf.PI);
  }

  private bool EsUnidadActiva()
  {
    return unidad != null
      && BattleManager.Instance != null
      && BattleManager.Instance.unidadActiva == unidad;
  }

  private void Ocultar()
  {
    if (rootRect != null && rootRect.gameObject.activeSelf)
    {
      rootRect.gameObject.SetActive(false);
    }
  }

  private static Sprite ObtenerSpriteSombra()
  {
    if (spriteSombra != null)
    {
      return spriteSombra;
    }

    const int ancho = 64;
    const int alto = 32;
    texturaSombra = new Texture2D(ancho, alto, TextureFormat.RGBA32, false, true);
    texturaSombra.name = "BattleVisualJuice_ContactShadow";
    texturaSombra.wrapMode = TextureWrapMode.Clamp;
    texturaSombra.filterMode = FilterMode.Bilinear;

    for (int y = 0; y < alto; y++)
    {
      for (int x = 0; x < ancho; x++)
      {
        float nx = ((x + 0.5f) / ancho - 0.5f) * 2f;
        float ny = ((y + 0.5f) / alto - 0.5f) * 2f;
        float distancia = Mathf.Sqrt(nx * nx + ny * ny);
        float alpha = Mathf.Clamp01(1f - distancia);
        alpha = alpha * alpha * (3f - 2f * alpha);
        texturaSombra.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
      }
    }

    texturaSombra.Apply(false, false);
    spriteSombra = Sprite.Create(
      texturaSombra,
      new Rect(0f, 0f, ancho, alto),
      new Vector2(0.5f, 0.5f),
      100f);
    spriteSombra.name = "BattleVisualJuice_ContactShadow";
    return spriteSombra;
  }
}
