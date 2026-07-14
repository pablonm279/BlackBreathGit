using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UnidadCombatFeedbackFx : MonoBehaviour
{
  private const string OverlayRootName = "CombatFeedbackFx";
  private const float EscalaImpacto = 0.9f;
  private const int SalvacionFortaleza = 1;
  private const int SalvacionReflejos = 2;
  private const int SalvacionMental = 3;

  [Header("Impacto")]
  [SerializeField] private float duracionImpacto = 0.18f;
  [SerializeField] private float duracionImpactoCritico = 0.24f;
  [SerializeField] private float duracionMissGlint = 0.14f;
  [SerializeField] private float duracionSalvacionMental = 0.58f;
  [SerializeField] private float duracionSalvacionReflejos = 0.26f;
  [SerializeField] private float duracionSalvacionFortaleza = 0.48f;
  [SerializeField] private float duracionSalvacionFallida = 0.16f;

  private Unidad unidad;
  private RectTransform imagenUnidad;
  private RectTransform overlayRoot;
  private CanvasGroup overlayGroup;
  private Image impactoAnillo;
  private Image impactoDestello;
  private Image missGlint;
  private Image salvacionGlow;
  private Image salvacionAro;
  private Image salvacionMarcaA;
  private Image salvacionMarcaB;
  private Image salvacionMarcaC;

  private float tiempoImpactoRestante = -1f;
  private float tiempoMissGlintRestante = -1f;
  private float tiempoSalvacionRestante = -1f;
  private float duracionImpactoActual;
  private float duracionSalvacionActual;
  private Color colorImpacto = Color.white;
  private bool impactoCritico;
  private bool impactoGuardia;
  private bool impactoSalvacionFallida;
  private int tipoImpacto;
  private int tipoSalvacion;
  private float faseSalvacion;

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
    ActualizarSalvacion();
  }

  private void OnDisable()
  {
    tiempoImpactoRestante = -1f;
    tiempoMissGlintRestante = -1f;
    tiempoSalvacionRestante = -1f;
    OcultarOverlay();
  }

  private void OnDestroy()
  {
    if (overlayRoot != null)
    {
      Destroy(overlayRoot.gameObject);
    }
  }

  public void PlayDamageImpact(Color color, bool critico, int tipoDanio, bool omitirFlashPantalla = false)
  {
    colorImpacto = AjustarColorImpacto(color, tipoDanio, false);
    impactoCritico = critico;
    impactoGuardia = false;
    impactoSalvacionFallida = false;
    tipoImpacto = tipoDanio;
    duracionImpactoActual = critico ? duracionImpactoCritico : duracionImpacto;
    tiempoImpactoRestante = duracionImpactoActual;

    if (!omitirFlashPantalla && critico)
    {
      ScreenFlash.FlashImpact(colorImpacto, 0.07f, 0.01f, 0.09f, 0f);
    }
    else if (!omitirFlashPantalla && tipoDanio >= 4)
    {
      ScreenFlash.FlashImpact(colorImpacto, 0.02f, 0.008f, 0.06f, 0f);
    }
  }

  public void PlayGuardImpact(Color color, bool omitirFlashPantalla = false)
  {
    colorImpacto = AjustarColorImpacto(color, 0, true);
    impactoCritico = false;
    impactoGuardia = true;
    impactoSalvacionFallida = false;
    tipoImpacto = 0;
    duracionImpactoActual = duracionImpacto * 0.85f;
    tiempoImpactoRestante = duracionImpactoActual;
    if (!omitirFlashPantalla)
    {
      ScreenFlash.FlashImpact(colorImpacto, 0.012f, 0.008f, 0.05f, 0f);
    }
  }

  public void PlaySaveSuccess(int tipo)
  {
    tipoSalvacion = Mathf.Clamp(tipo, SalvacionFortaleza, SalvacionMental);
    faseSalvacion = Random.Range(0f, Mathf.PI * 2f);

    switch (tipoSalvacion)
    {
      case SalvacionReflejos:
        duracionSalvacionActual = Mathf.Max(0.01f, duracionSalvacionReflejos);
        break;
      case SalvacionMental:
        duracionSalvacionActual = Mathf.Max(0.01f, duracionSalvacionMental);
        break;
      case SalvacionFortaleza:
      default:
        duracionSalvacionActual = Mathf.Max(0.01f, duracionSalvacionFortaleza);
        break;
    }

    tiempoSalvacionRestante = duracionSalvacionActual;
  }

  public void PlaySaveFailureImpact()
  {
    colorImpacto = new Color(0.74f, 0.7f, 0.82f, 1f);
    impactoCritico = false;
    impactoGuardia = false;
    impactoSalvacionFallida = true;
    tipoImpacto = 0;
    duracionImpactoActual = Mathf.Max(0.01f, duracionSalvacionFallida);
    tiempoImpactoRestante = duracionImpactoActual;
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
    salvacionGlow = CrearImagen("SalvacionGlow", overlayRoot, spriteSuave);
    salvacionAro = CrearImagen("SalvacionAro", overlayRoot, spriteAnillo);
    salvacionMarcaA = CrearImagen("SalvacionMarcaA", overlayRoot, spriteGlint);
    salvacionMarcaB = CrearImagen("SalvacionMarcaB", overlayRoot, spriteGlint);
    salvacionMarcaC = CrearImagen("SalvacionMarcaC", overlayRoot, spriteSuave);

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

    if (overlayGroup != null)
    {
      overlayGroup.alpha = unidad != null ? unidad.ObtenerMultiplicadorAlphaVisual() : 1f;
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

    float anchoBase = tamano.x * (impactoSalvacionFallida ? 0.32f : (impactoGuardia ? 0.3536f : (impactoCritico ? 0.468f : 0.4056f))) * EscalaImpacto;
    float altoBase = tamano.y * (impactoSalvacionFallida ? 0.26f : (impactoGuardia ? 0.2808f : (impactoCritico ? 0.4368f : 0.3536f))) * EscalaImpacto;
    float expansion = impactoSalvacionFallida ? 0.055f : (impactoGuardia ? 0.08f : (impactoCritico ? 0.14f : 0.1f));
    float giro = (impactoGuardia || impactoSalvacionFallida) ? 0f : (tipoImpacto <= 3 ? -10f : 0f);
    float alphaAnillo = impactoSalvacionFallida ? 0.053f : (impactoGuardia ? 0.102f : (impactoCritico ? 0.153f : 0.095625f));
    float alphaDestello = impactoSalvacionFallida ? 0.067f : (impactoGuardia ? 0.11475f : (impactoCritico ? 0.19125f : 0.1275f));

    ConfigurarImagen(
      impactoAnillo,
      Vector2.zero,
      new Vector2(anchoBase * (1f + (n * expansion)), altoBase * (1f + (n * expansion))),
      WithAlpha(colorImpacto, fade * alphaAnillo));
    impactoAnillo.rectTransform.localEulerAngles = new Vector3(0f, 0f, giro);

    ConfigurarImagen(
      impactoDestello,
      Vector2.zero,
      new Vector2(tamano.x * (impactoSalvacionFallida ? 0.23f : (impactoGuardia ? 0.26f : (impactoCritico ? 0.3536f : 0.3016f))) * EscalaImpacto, tamano.y * (impactoSalvacionFallida ? 0.2f : (impactoGuardia ? 0.2288f : (impactoCritico ? 0.3328f : 0.2704f))) * EscalaImpacto),
      WithAlpha(Color.Lerp(colorImpacto, Color.white, impactoCritico ? 0.28f : 0.14f), fade * alphaDestello));
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

  private void ActualizarSalvacion()
  {
    if (salvacionGlow == null
      || salvacionAro == null
      || salvacionMarcaA == null
      || salvacionMarcaB == null
      || salvacionMarcaC == null)
    {
      return;
    }

    if (tiempoSalvacionRestante <= 0f)
    {
      OcultarImagen(salvacionGlow);
      OcultarImagen(salvacionAro);
      OcultarImagen(salvacionMarcaA);
      OcultarImagen(salvacionMarcaB);
      OcultarImagen(salvacionMarcaC);
      return;
    }

    tiempoSalvacionRestante = Mathf.Max(0f, tiempoSalvacionRestante - Time.unscaledDeltaTime);
    float duracion = Mathf.Max(0.01f, duracionSalvacionActual);
    float n = 1f - (tiempoSalvacionRestante / duracion);

    switch (tipoSalvacion)
    {
      case SalvacionReflejos:
        ActualizarSalvacionReflejos(n);
        break;
      case SalvacionMental:
        ActualizarSalvacionMental(n);
        break;
      case SalvacionFortaleza:
      default:
        ActualizarSalvacionFortaleza(n);
        break;
    }
  }

  private void ActualizarSalvacionMental(float n)
  {
    Vector2 tamano = ObtenerTamanoUnidad();
    Vector2 centro = ObtenerPosicionSalvacionLocal();
    float ancho = Mathf.Max(24f, tamano.x * 0.5f) * 0.8f;
    float alto = Mathf.Max(8f, tamano.y * 0.14f);
    float fade = 1f - SmoothStep01(0.08f, 1f, n);
    float disolver = 1f + (n * 0.34f);
    float t = Time.unscaledTime;
    float pulso = Mathf.Abs(Mathf.Sin((t * 8.2f) + faseSalvacion));
    float orbitaX = ancho * Mathf.Lerp(0.16f, 0.34f, n);
    float orbitaY = alto * Mathf.Lerp(0.45f, 0.82f, n);
    Color baseColor = new Color(0.65f, 0.9f, 1f, 1f);
    Color auraColor = new Color(0.58f, 0.48f, 1f, 1f);

    ConfigurarImagen(
      salvacionGlow,
      centro + new Vector2(0f, 1.4f + (n * 4f)),
      new Vector2(ancho * 1.22f, alto * 1.82f) * disolver,
      WithAlpha(auraColor, fade * Mathf.Lerp(0.12f, 0.05f, n)));

    ConfigurarImagen(
      salvacionAro,
      centro,
      new Vector2(ancho, alto) * (0.92f + (pulso * 0.05f) + (n * 0.2f)),
      WithAlpha(baseColor, fade * Mathf.Lerp(0.48f, 0.12f, n)));
    salvacionAro.rectTransform.localEulerAngles = new Vector3(0f, 0f, 0f);

    ConfigurarImagen(
      salvacionMarcaA,
      centro + new Vector2(Mathf.Cos(faseSalvacion + (n * 4.4f)) * orbitaX, Mathf.Sin(faseSalvacion + (n * 4.4f)) * orbitaY),
      new Vector2(ancho * 0.18f, alto * 0.9f),
      WithAlpha(new Color(0.82f, 0.97f, 1f, 1f), fade * 0.34f));
    salvacionMarcaA.rectTransform.localEulerAngles = new Vector3(0f, 0f, -26f);

    ConfigurarImagen(
      salvacionMarcaB,
      centro + new Vector2(Mathf.Cos(faseSalvacion + 2.2f + (n * 3.7f)) * (orbitaX * 0.84f), Mathf.Sin(faseSalvacion + 2.2f + (n * 3.7f)) * (orbitaY * 0.92f)),
      new Vector2(ancho * 0.14f, alto * 0.72f),
      WithAlpha(new Color(0.72f, 0.8f, 1f, 1f), fade * 0.28f));
    salvacionMarcaB.rectTransform.localEulerAngles = new Vector3(0f, 0f, 24f);

    ConfigurarImagen(
      salvacionMarcaC,
      centro + new Vector2(Mathf.Cos(faseSalvacion + 4.1f + (n * 3.2f)) * (orbitaX * 0.72f), Mathf.Sin(faseSalvacion + 4.1f + (n * 3.2f)) * (orbitaY * 0.82f)),
      Vector2.one * Mathf.Max(4f, tamano.x * 0.05f) * (1f + (n * 0.2f)),
      WithAlpha(new Color(0.95f, 0.98f, 1f, 1f), fade * 0.26f));
  }

  private void ActualizarSalvacionReflejos(float n)
  {
    Vector2 tamano = ObtenerTamanoUnidad();
    Vector2 centro = ObtenerPosicionSalvacionLocal();
    float fade = 1f - SmoothStep01(0.18f, 1f, n);
    float ancho = Mathf.Max(20f, tamano.x * 0.36f);
    float alto = Mathf.Max(12f, tamano.y * 0.2f);
    Color colorPrincipal = new Color(0.2f, 1f, 0.82f, 1f);

    ConfigurarImagen(
      salvacionGlow,
      centro + new Vector2(ancho * Mathf.Lerp(-0.12f, 0.16f, n), 0f),
      new Vector2(ancho * 0.72f, alto * 0.66f),
      WithAlpha(colorPrincipal, fade * 0.12f));

    OcultarImagen(salvacionAro);

    ConfigurarImagen(
      salvacionMarcaA,
      centro + new Vector2(Mathf.Lerp(-ancho * 0.32f, ancho * 0.22f, n), Mathf.Lerp(-alto * 0.08f, alto * 0.08f, n)),
      new Vector2(ancho * 0.88f, alto * 0.2f),
      WithAlpha(colorPrincipal, fade * 0.46f));
    salvacionMarcaA.rectTransform.localEulerAngles = new Vector3(0f, 0f, -24f);

    ConfigurarImagen(
      salvacionMarcaB,
      centro + new Vector2(Mathf.Lerp(ancho * 0.24f, -ancho * 0.12f, n), Mathf.Lerp(alto * 0.12f, -alto * 0.1f, n)),
      new Vector2(ancho * 0.62f, alto * 0.16f),
      WithAlpha(new Color(0.86f, 1f, 0.94f, 1f), fade * 0.34f));
    salvacionMarcaB.rectTransform.localEulerAngles = new Vector3(0f, 0f, 28f);

    ConfigurarImagen(
      salvacionMarcaC,
      centro + new Vector2(ancho * Mathf.Lerp(0.06f, 0.28f, n), alto * Mathf.Lerp(-0.06f, 0.1f, n)),
      Vector2.one * Mathf.Max(3f, tamano.x * 0.035f),
      WithAlpha(new Color(0.78f, 1f, 0.92f, 1f), fade * 0.28f));
  }

  private void ActualizarSalvacionFortaleza(float n)
  {
    Vector2 tamano = ObtenerTamanoUnidad();
    Vector2 centro = ObtenerPosicionSalvacionLocal();
    float fadeIn = SmoothStep01(0f, 0.22f, n);
    float fadeOut = 1f - SmoothStep01(0.42f, 1f, n);
    float fade = fadeIn * fadeOut;
    float ancho = Mathf.Max(15.4f, tamano.x * 0.294f) * 0.65f;
    float alto = Mathf.Max(7f, tamano.y * 0.112f) * 0.65f;
    Color colorPrincipal = new Color(0.7f, 1f, 0.58f, 1f);
    Color colorSecundario = new Color(1f, 0.88f, 0.42f, 1f);

    ConfigurarImagen(
      salvacionGlow,
      centro + new Vector2(0f, alto * 0.02f),
      new Vector2(ancho * 1.05f, alto * 1.24f) * (0.9f + (n * 0.14f)),
      WithAlpha(colorPrincipal, fade * 0.12f));

    ConfigurarImagen(
      salvacionAro,
      centro,
      new Vector2(ancho, alto) * (0.86f + (n * 0.12f)),
      WithAlpha(Color.Lerp(colorPrincipal, colorSecundario, 0.34f), fade * 0.36f));
    salvacionAro.rectTransform.localEulerAngles = new Vector3(0f, 0f, 0f);

    ConfigurarImagen(
      salvacionMarcaA,
      centro + new Vector2(-ancho * 0.18f, alto * 0.02f),
      new Vector2(ancho * 0.32f, alto * 0.12f),
      WithAlpha(colorSecundario, fade * 0.28f));
    salvacionMarcaA.rectTransform.localEulerAngles = new Vector3(0f, 0f, -42f);

    ConfigurarImagen(
      salvacionMarcaB,
      centro + new Vector2(ancho * 0.18f, alto * 0.02f),
      new Vector2(ancho * 0.32f, alto * 0.12f),
      WithAlpha(colorSecundario, fade * 0.28f));
    salvacionMarcaB.rectTransform.localEulerAngles = new Vector3(0f, 0f, 42f);

    ConfigurarImagen(
      salvacionMarcaC,
      centro,
      Vector2.one * Mathf.Max(4f, tamano.x * 0.045f) * 0.65f * (0.8f + (n * 0.12f)),
      WithAlpha(new Color(0.92f, 1f, 0.76f, 1f), fade * 0.22f));
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

  private Vector2 ObtenerPosicionSalvacionLocal()
  {
    Vector2 tamano = ObtenerTamanoUnidad();
    if (imagenUnidad == null || overlayRoot == null)
    {
      return new Vector2(0f, tamano.y * 0.58f);
    }

    Vector2 posicionCabeza = imagenUnidad.anchoredPosition + new Vector2(0f, tamano.y * 0.58f);
    return posicionCabeza - overlayRoot.anchoredPosition;
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

  private static void OcultarImagen(Image image)
  {
    if (image != null)
    {
      image.color = WithAlpha(image.color, 0f);
    }
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

  private static float SmoothStep01(float desde, float hasta, float valor)
  {
    return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(desde, hasta, valor));
  }
}
