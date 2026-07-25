using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Perdicion : IAHabilidad
{
  [SerializeField] public int pPrioridad;

  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

  public GameObject VFXEstadoPrefab;
  object Objetivo;

  void Awake()
  {
    nombre = "Perdición";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 3;
    hCooldownMax = 4;
    esHostil = true;
    prioridad = pPrioridad;
    costoAP = 2;
    afectaObstaculos = true;

    hActualCooldown = UnityEngine.Random.Range(0, hCooldownMax + 1);

    bonusAtaque = 0;
    XdDanio = 0;
    daniodX = 0;
    tipoDanio = 0;
  }

  void Start()
  {
    hActualCooldown = UnityEngine.Random.Range(0, 4);
    prioridad = 55;
  }

  public async override Task ActivarHabilidad()
  {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);

    Objetivo = EstablecerObjetivoPrioritario();
    PrepararInicioAnimacion(null, Objetivo);

    hActualCooldown = hCooldownMax;

    await BattleManager.DelayCombateAsync(1300);
    AplicarEfectosHabilidad(Objetivo);
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad objetivo))
    {
      return;
    }

    if (!objetivo.TiradaSalvacion(3, 13))
    {
      return;
    }

    Buff buff = new Buff();
    buff.buffNombre = "Perdición";
    buff.boolfDebufftBuff = false;
    buff.DuracionBuffRondas = 3;
    buff.cantAPMax -= 1;
    buff.cantAtaque -= 2;
    buff.cantTsMental -= 2;
    buff.cantResNec = -10;
    buff.AplicarBuff(objetivo);

    GameObject goVFX = CrearVfxEstadoPerdicion(objetivo);
    buff.goVFX = goVFX;

    ComponentCopier.CopyComponent(buff, objetivo.gameObject);
  }

  GameObject CrearVfxEstadoPerdicion(Unidad objetivo)
  {
    if (objetivo == null)
    {
      return null;
    }

    GameObject contenedor = new GameObject("PerdicionEstadoVFX");
    contenedor.transform.SetParent(objetivo.transform, false);
    contenedor.transform.localPosition = Vector3.zero;
    contenedor.transform.localRotation = Quaternion.identity;
    contenedor.transform.localScale = Vector3.one;

    if (VFXEstadoPrefab != null)
    {
      GameObject sonidoVfx = Instantiate(VFXEstadoPrefab, contenedor.transform);
      sonidoVfx.transform.localPosition = Vector3.zero;
      sonidoVfx.transform.localRotation = Quaternion.identity;
      sonidoVfx.transform.localScale = Vector3.one;
      VFXSoloSonido.OcultarVisuales(sonidoVfx);
    }

    Canvas canvasVisual = PerdicionEspectroObjetivoFx.Crear(contenedor.transform, objetivo);
    RenderOrderHelper.OrdenarCanvasEncima(canvasVisual, objetivo.transform, 3);
    return contenedor;
  }

  public override object EstablecerObjetivoPrioritario()
  {
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null)
    {
      return null;
    }

    var unidades = objPosibles.OfType<Unidad>().ToList();
    for (int i = unidades.Count - 1; i >= 0; i--)
    {
      if (unidades[i].estado_inmovil > 0)
      {
        unidades.RemoveAt(i);
      }
    }

    var unidadesOrdenadas = unidades
      .OrderBy(unidad => unidad.CasillaPosicion.posX)
      .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDueña.CasillaPosicion.posY))
      .ToList();

    return unidadesOrdenadas.Any() ? unidadesOrdenadas.LastOrDefault() : null;
  }
}

public class PerdicionEspectroObjetivoFx : MonoBehaviour
{
  private const float DuracionEntrada = 0.24f;
  private const float OpacidadBase = 0.9f;
  private const float VelocidadVisual = 0.68f;
  private const int CantidadFragmentos = 6;
  private const int CantidadMotas = 4;

  private RectTransform root;
  private CanvasGroup canvasGroup;
  private Image haloExterior;
  private Image nucleoSombra;
  private Image selloExterior;
  private Image selloInterior;
  private Image anilloSuperior;
  private Image anilloInferior;
  private readonly Image[] fragmentos = new Image[CantidadFragmentos];
  private readonly float[] faseFragmento = new float[CantidadFragmentos];
  private readonly float[] velocidadFragmento = new float[CantidadFragmentos];
  private readonly float[] radioFragmento = new float[CantidadFragmentos];
  private readonly Image[] motas = new Image[CantidadMotas];
  private readonly float[] faseMota = new float[CantidadMotas];
  private Vector2 tamanoBase;
  private float tiempo;

  private static Sprite spriteSuave;
  private static Sprite spriteAnillo;
  private static Sprite spriteRombo;
  private static Sprite spriteFragmento;
  private static Texture2D texturaSuave;
  private static Texture2D texturaAnillo;
  private static Texture2D texturaRombo;
  private static Texture2D texturaFragmento;

  public static Canvas Crear(Transform padre, Unidad unidad)
  {
    if (padre == null || unidad == null)
    {
      return null;
    }

    GameObject go = new GameObject(
      "PerdicionEspectroObjetivoFx",
      typeof(RectTransform),
      typeof(Canvas),
      typeof(CanvasGroup),
      typeof(PerdicionEspectroObjetivoFx));

    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(padre, false);

    Canvas canvas = go.GetComponent<Canvas>();
    canvas.renderMode = RenderMode.WorldSpace;
    canvas.overrideSorting = true;

    PerdicionEspectroObjetivoFx fx = go.GetComponent<PerdicionEspectroObjetivoFx>();
    fx.Inicializar(unidad, rect, go.GetComponent<CanvasGroup>());
    return canvas;
  }

  private void Inicializar(Unidad unidad, RectTransform rootTransform, CanvasGroup group)
  {
    root = rootTransform;
    canvasGroup = group;
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;

    tamanoBase = unidad.uImage != null ? unidad.uImage.rectTransform.rect.size : Vector2.zero;
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = unidad.uImage != null ? NormalizarTamano(unidad.uImage.rectTransform.sizeDelta) : Vector2.zero;
    }
    if (tamanoBase.x <= 0.01f || tamanoBase.y <= 0.01f)
    {
      tamanoBase = new Vector2(36f, 42f);
    }

    root.anchorMin = new Vector2(0.5f, 0.5f);
    root.anchorMax = new Vector2(0.5f, 0.5f);
    root.pivot = new Vector2(0.5f, 0.5f);
    root.localPosition = new Vector3(0f, 1.42f, -0.04f);
    root.localRotation = Quaternion.identity;
    root.localScale = new Vector3(0.05f, 0.05f, 1f);
    root.sizeDelta = new Vector2(tamanoBase.x * 1.7f, tamanoBase.y * 1.95f);

    haloExterior = CrearImagen("HaloExterior", ObtenerSpriteSuave(), root);
    nucleoSombra = CrearImagen("NucleoSombra", ObtenerSpriteSuave(), root);
    selloExterior = CrearImagen("SelloExterior", ObtenerSpriteRombo(), root);
    selloInterior = CrearImagen("SelloInterior", ObtenerSpriteRombo(), root);
    anilloSuperior = CrearImagen("AnilloSuperior", ObtenerSpriteAnillo(), root);
    anilloInferior = CrearImagen("AnilloInferior", ObtenerSpriteAnillo(), root);

    for (int i = 0; i < fragmentos.Length; i++)
    {
      fragmentos[i] = CrearImagen("Fragmento" + i, ObtenerSpriteFragmento(), root);
      faseFragmento[i] = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
      velocidadFragmento[i] = UnityEngine.Random.Range(0.72f, 1.18f);
      radioFragmento[i] = UnityEngine.Random.Range(0.84f, 1.16f);
    }

    for (int i = 0; i < motas.Length; i++)
    {
      motas[i] = CrearImagen("Mota" + i, ObtenerSpriteSuave(), root);
      faseMota[i] = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
    }

    ActualizarVisual(0f);
  }

  private static Vector2 NormalizarTamano(Vector2 valor)
  {
    return new Vector2(Mathf.Abs(valor.x), Mathf.Abs(valor.y));
  }

  private Image CrearImagen(string nombre, Sprite sprite, RectTransform padre)
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
    image.maskable = false;
    image.preserveAspect = false;
    return image;
  }

  private void Update()
  {
    tiempo += Time.deltaTime;
    float intensidad = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / DuracionEntrada));
    ActualizarVisual(intensidad * OpacidadBase);
  }

  private void ActualizarVisual(float intensidad)
  {
    if (root == null || canvasGroup == null)
    {
      return;
    }

    float t = Time.time * VelocidadVisual;
    float pulso = 1f + (Mathf.Sin(t * 6.2f) * 0.04f) + (Mathf.Sin(t * 13.4f + 0.8f) * 0.025f);
    float respiracion = 0.94f + (Mathf.Sin(t * 4.1f + 0.35f) * 0.05f);
    float derivaX = Mathf.Sin(t * 2.6f) * tamanoBase.x * 0.025f;
    float derivaY = Mathf.Cos(t * 2.1f) * tamanoBase.y * 0.012f;

    canvasGroup.alpha = intensidad;
    root.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(t * 2.3f) * 2.8f);
    root.localPosition = new Vector3(0f, 1.42f, -0.04f);

    Configurar(
      haloExterior,
      new Vector2(derivaX, derivaY),
      new Vector2(tamanoBase.x * 0.96f * pulso, tamanoBase.y * 1.34f * respiracion),
      Mathf.Sin(t * 3.1f) * 4f,
      new Color(0.32f, 0.07f, 0.42f, 0.18f * intensidad));

    Configurar(
      nucleoSombra,
      new Vector2(derivaX * 0.65f, 0f),
      new Vector2(tamanoBase.x * 0.24f, tamanoBase.y * 1.1f * (0.96f + (Mathf.Sin(t * 5.2f) * 0.04f))),
      Mathf.Sin(t * 2f) * 1.5f,
      new Color(0.03f, 0.01f, 0.05f, 0.38f * intensidad));

    Configurar(
      selloExterior,
      new Vector2(0f, tamanoBase.y * 0.02f),
      new Vector2(tamanoBase.x * 0.5f * pulso, tamanoBase.x * 0.5f * respiracion),
      45f + (Mathf.Sin(t * 4.2f) * 7f),
      new Color(0.45f, 0.14f, 0.56f, 0.19f * intensidad));

    Configurar(
      selloInterior,
      new Vector2(0f, tamanoBase.y * 0.02f),
      new Vector2(tamanoBase.x * 0.24f * respiracion, tamanoBase.x * 0.24f * pulso),
      -45f + (Mathf.Sin(t * 5.4f + 0.5f) * 10f),
      new Color(0.06f, 0.03f, 0.08f, 0.28f * intensidad));

    Configurar(
      anilloSuperior,
      new Vector2(Mathf.Sin(t * 3.6f) * tamanoBase.x * 0.03f, tamanoBase.y * 0.37f),
      new Vector2(tamanoBase.x * 0.72f, tamanoBase.y * 0.16f),
      Mathf.Sin(t * 2.8f) * 10f,
      new Color(0.52f, 0.2f, 0.66f, 0.16f * intensidad));

    Configurar(
      anilloInferior,
      new Vector2(Mathf.Sin(t * 3.1f + 0.8f) * tamanoBase.x * 0.025f, -tamanoBase.y * 0.34f),
      new Vector2(tamanoBase.x * 0.64f, tamanoBase.y * 0.13f),
      -Mathf.Sin(t * 3f + 0.6f) * 12f,
      new Color(0.07f, 0.03f, 0.08f, 0.24f * intensidad));

    for (int i = 0; i < fragmentos.Length; i++)
    {
      float angulo = faseFragmento[i] + (t * velocidadFragmento[i]);
      float radioX = tamanoBase.x * 0.34f * radioFragmento[i];
      float radioY = tamanoBase.y * 0.46f * (0.88f + (0.05f * Mathf.Sin(t * (2.2f + i))));
      Vector2 posicion = new Vector2(
        Mathf.Cos(angulo) * radioX,
        Mathf.Sin(angulo * 1.08f) * radioY);
      float alpha = (0.1f + (0.1f * Mathf.Sin((t * 4.6f) + i))) * intensidad;
      bool tonoOscuro = (i % 2) == 0;

      Configurar(
        fragmentos[i],
        posicion,
        new Vector2(tamanoBase.x * 0.22f, tamanoBase.y * 0.08f),
        (angulo * Mathf.Rad2Deg) + 90f,
        tonoOscuro
          ? new Color(0.08f, 0.03f, 0.1f, alpha * 1.15f)
          : new Color(0.52f, 0.17f, 0.68f, alpha));
    }

    for (int i = 0; i < motas.Length; i++)
    {
      float angulo = faseMota[i] + (t * (0.45f + (i * 0.08f)));
      Vector2 posicion = new Vector2(
        Mathf.Sin(angulo) * tamanoBase.x * 0.16f,
        Mathf.Cos(angulo * 1.35f) * tamanoBase.y * 0.24f);
      float escala = 0.16f + (0.04f * Mathf.Sin((t * 4.2f) + i));
      float alpha = (0.05f + (0.035f * Mathf.Sin((t * 5.4f) + i))) * intensidad;

      Configurar(
        motas[i],
        posicion,
        new Vector2(tamanoBase.x * escala, tamanoBase.x * escala),
        0f,
        new Color(0.4f, 0.16f, 0.54f, alpha));
    }
  }

  private static void Configurar(Image image, Vector2 posicion, Vector2 tamano, float rotacionZ, Color color)
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
    texturaSuave.name = "PerdicionSoftRuntime";
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
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 2.2f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaSuave.SetPixels(pixels);
    texturaSuave.Apply(false, true);
    spriteSuave = Sprite.Create(texturaSuave, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteSuave.name = "PerdicionSoftRuntime";
    return spriteSuave;
  }

  private static Sprite ObtenerSpriteAnillo()
  {
    if (spriteAnillo != null)
    {
      return spriteAnillo;
    }

    const int size = 64;
    texturaAnillo = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaAnillo.name = "PerdicionRingRuntime";
    texturaAnillo.wrapMode = TextureWrapMode.Clamp;
    texturaAnillo.filterMode = FilterMode.Bilinear;
    texturaAnillo.hideFlags = HideFlags.HideAndDontSave;

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
        float borde = Mathf.Abs(distancia - 0.58f);
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - (borde / 0.16f)), 1.75f) * Mathf.Clamp01(1f - (Mathf.Abs(dy) * 0.6f));
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAnillo.SetPixels(pixels);
    texturaAnillo.Apply(false, true);
    spriteAnillo = Sprite.Create(texturaAnillo, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteAnillo.name = "PerdicionRingRuntime";
    return spriteAnillo;
  }

  private static Sprite ObtenerSpriteRombo()
  {
    if (spriteRombo != null)
    {
      return spriteRombo;
    }

    const int size = 64;
    texturaRombo = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaRombo.name = "PerdicionDiamondRuntime";
    texturaRombo.wrapMode = TextureWrapMode.Clamp;
    texturaRombo.filterMode = FilterMode.Bilinear;
    texturaRombo.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = Mathf.Abs(x - centro) / radio;
        float dy = Mathf.Abs(y - centro) / radio;
        float distancia = dx + dy;
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distancia), 1.75f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaRombo.SetPixels(pixels);
    texturaRombo.Apply(false, true);
    spriteRombo = Sprite.Create(texturaRombo, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteRombo.name = "PerdicionDiamondRuntime";
    return spriteRombo;
  }

  private static Sprite ObtenerSpriteFragmento()
  {
    if (spriteFragmento != null)
    {
      return spriteFragmento;
    }

    const int width = 64;
    const int height = 18;
    texturaFragmento = new Texture2D(width, height, TextureFormat.ARGB32, false);
    texturaFragmento.name = "PerdicionShardRuntime";
    texturaFragmento.wrapMode = TextureWrapMode.Clamp;
    texturaFragmento.filterMode = FilterMode.Bilinear;
    texturaFragmento.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[width * height];
    float centroY = (height - 1) * 0.5f;
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        float nx = x / (width - 1f);
        float distanciaY = Mathf.Abs(y - centroY) / centroY;
        float taper = 1f - Mathf.Abs((nx * 2f) - 1f);
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - distanciaY), 1.8f) * Mathf.Pow(taper, 1.25f);
        pixels[(y * width) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaFragmento.SetPixels(pixels);
    texturaFragmento.Apply(false, true);
    spriteFragmento = Sprite.Create(texturaFragmento, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    spriteFragmento.name = "PerdicionShardRuntime";
    return spriteFragmento;
  }
}
