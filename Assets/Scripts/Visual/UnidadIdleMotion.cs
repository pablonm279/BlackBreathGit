using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UnidadIdleMotion : MonoBehaviour
{
  [Header("Idle Motion")]
  [SerializeField] private bool habilitado = true;
  [SerializeField] private bool soloEnBatalla = true;
  [SerializeField] private float amplitudX = 0.28f;
  [SerializeField] private float amplitudY = 3.8f;
  [SerializeField] private float amplitudRotacion = 0.16f;
  [SerializeField] private float velocidad = 0.95f;
  [SerializeField] private float suavizado = 8.5f;
  [SerializeField] private float factorDuranteMovimiento = 0.08f;
  [SerializeField] private float multiplicadorGlobal = 1f;
  [SerializeField] private float multiplicadorUnidadActivaJugador = 1.08f; // +8% adicional si es turno del jugador
  [SerializeField] private float amplitudReboteTurnoNuevoY = 5.6f;
  [SerializeField] private float duracionReboteTurnoNuevo = 0.44f;

  private Unidad unidad;
  private RectTransform rectImagen;

  private Vector2 offsetAplicado;
  private float rotacionAplicada;
  private Vector2 posicionBase;
  private float rotacionBase;
  private bool baseInicializada;
  private float faseX;
  private float faseY;
  private float faseRot;
  private float progresoReboteTurnoNuevo = -1f;

  void Awake()
  {
    unidad = GetComponent<Unidad>();
    VincularRect();

    faseX = Random.Range(0f, Mathf.PI * 2f);
    faseY = Random.Range(0f, Mathf.PI * 2f);
    faseRot = Random.Range(0f, Mathf.PI * 2f);
  }

  void OnEnable()
  {
    if (!VincularRect())
    {
      return;
    }

    CapturarBaseActual();
  }

  void LateUpdate()
  {
    if (!VincularRect())
    {
      return;
    }

    SincronizarBaseConCambiosExternos();

    if (!DebeAnimar())
    {
      AplicarOffset(Vector2.zero, 0f, true);
      return;
    }

    float t = Time.time * Mathf.Max(0.01f, velocidad);
    float factor = unidad != null && unidad.movimientoEnCurso ? factorDuranteMovimiento : 1f;
    float factorGlobal = Mathf.Max(0f, multiplicadorGlobal);
    if (EsUnidadActivaJugador())
    {
      factorGlobal *= Mathf.Max(0f, multiplicadorUnidadActivaJugador);
    }
    factor *= factorGlobal;

    float respiracionPrincipal = Mathf.Sin(t + faseY);
    float respiracionSecundaria = Mathf.Sin(t * 2f + faseRot) * 0.18f;
    float respiracionCompuesta = (respiracionPrincipal * 0.82f) + respiracionSecundaria;
    float swayNormalizado = Mathf.Sin(t * 0.5f + faseX);
    float sway = swayNormalizado * amplitudX * (0.55f + (0.45f * Mathf.Abs(respiracionPrincipal)));

    Vector2 objetivoOffset = new Vector2(
      sway,
      respiracionCompuesta * amplitudY) * factor;

    Vector2 offsetReboteTurnoNuevo = CalcularOffsetReboteTurnoNuevo();
    objetivoOffset += offsetReboteTurnoNuevo;

    float objetivoRot = ((-respiracionPrincipal * 0.9f) + (swayNormalizado * 0.1f)) * amplitudRotacion * factor;

    bool suavizar = suavizado > 0.01f;
    AplicarOffset(objetivoOffset, objetivoRot, suavizar);
  }

  private bool EsUnidadActivaJugador()
  {
    if (unidad == null || BattleManager.Instance == null)
    {
      return false;
    }

    if (BattleManager.Instance.unidadActiva != unidad)
    {
      return false;
    }

    // Si tiene IA, no es el turno activo de una unidad del jugador.
    return unidad.GetComponent<IAUnidad>() == null;
  }

  void OnDisable()
  {
    if (rectImagen == null || !baseInicializada)
    {
      return;
    }

    SincronizarBaseConCambiosExternos();
    offsetAplicado = Vector2.zero;
    rotacionAplicada = 0f;

    rectImagen.anchoredPosition = posicionBase;
    Vector3 e = rectImagen.localEulerAngles;
    e.z = rotacionBase;
    rectImagen.localEulerAngles = e;
  }

  private bool VincularRect()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
      if (unidad == null)
      {
        return false;
      }
    }

    if (rectImagen == null)
    {
      Image img = unidad.uImage;
      if (img == null)
      {
        return false;
      }

      rectImagen = img.rectTransform;
      baseInicializada = false;
    }

    return rectImagen != null;
  }

  private void CapturarBaseActual()
  {
    if (rectImagen == null)
    {
      return;
    }

    posicionBase = rectImagen.anchoredPosition;
    rotacionBase = ObtenerRotacionActual();
    offsetAplicado = Vector2.zero;
    rotacionAplicada = 0f;
    baseInicializada = true;
  }

  private void SincronizarBaseConCambiosExternos()
  {
    if (rectImagen == null)
    {
      return;
    }

    if (!baseInicializada)
    {
      CapturarBaseActual();
      return;
    }

    Vector2 posicionEsperada = posicionBase + offsetAplicado;
    Vector2 deltaPosicion = rectImagen.anchoredPosition - posicionEsperada;
    if (deltaPosicion.sqrMagnitude > 0.0001f)
    {
      posicionBase += deltaPosicion;
    }

    float rotacionEsperada = rotacionBase + rotacionAplicada;
    float deltaRotacion = Mathf.DeltaAngle(rotacionEsperada, ObtenerRotacionActual());
    if (Mathf.Abs(deltaRotacion) > 0.001f)
    {
      rotacionBase += deltaRotacion;
    }
  }

  private bool DebeAnimar()
  {
    if (!habilitado || rectImagen == null)
    {
      return false;
    }

    if (soloEnBatalla && BattleManager.Instance == null)
    {
      return false;
    }

    if (unidad == null)
    {
      return false;
    }

    if (!unidad.gameObject.activeInHierarchy || unidad.HP_actual <= 0f)
    {
      return false;
    }

    // Excluir unidades que no deben tener respiracion visual.
    if (unidad.esInmobil || unidad.esEtereo || unidad.estado_congelado > 0 || unidad.estado_aturdido > 0)
    {
      return false;
    }

    // No aplicar a voladoras (esten o no volando en este instante).
    if (unidad.unidadVoladora || unidad.estado_Volando)
    {
      return false;
    }

    return true;
  }

  public void ReproducirReboteTurnoNuevo()
  {
    if (!VincularRect())
    {
      return;
    }

    SincronizarBaseConCambiosExternos();
    progresoReboteTurnoNuevo = 0f;
  }

  private void AplicarOffset(Vector2 objetivoOffset, float objetivoRot, bool suavizarMovimiento)
  {
    if (!baseInicializada)
    {
      CapturarBaseActual();
    }

    if (suavizarMovimiento)
    {
      float lerp = Mathf.Clamp01(Time.deltaTime * suavizado);
      offsetAplicado = Vector2.Lerp(offsetAplicado, objetivoOffset, lerp);
      rotacionAplicada = Mathf.Lerp(rotacionAplicada, objetivoRot, lerp);
    }
    else
    {
      offsetAplicado = objetivoOffset;
      rotacionAplicada = objetivoRot;
    }

    rectImagen.anchoredPosition = posicionBase + offsetAplicado;
    Vector3 e = rectImagen.localEulerAngles;
    e.z = rotacionBase + rotacionAplicada;
    rectImagen.localEulerAngles = e;
  }

  private float ObtenerRotacionActual()
  {
    return Mathf.DeltaAngle(0f, rectImagen.localEulerAngles.z);
  }

  private Vector2 CalcularOffsetReboteTurnoNuevo()
  {
    if (progresoReboteTurnoNuevo < 0f)
    {
      return Vector2.zero;
    }

    float duracion = Mathf.Max(0.01f, duracionReboteTurnoNuevo);
    progresoReboteTurnoNuevo += Time.deltaTime / duracion;
    float progresoClamped = Mathf.Clamp01(progresoReboteTurnoNuevo);
    float desplazamientoY = Mathf.Sin(progresoClamped * Mathf.PI) * amplitudReboteTurnoNuevoY;

    if (progresoReboteTurnoNuevo >= 1f)
    {
      progresoReboteTurnoNuevo = -1f;
    }

    return Vector2.up * desplazamientoY;
  }
}

[DefaultExecutionOrder(500)]
[DisallowMultipleComponent]
public sealed class UnidadStatusVfxController : MonoBehaviour
{
  private const string SpriteArdiendoPrincipalPath = "VFX/llamadivina";
  private const string SpriteArdiendoSecundarioPath = "VFX/llamadivina 1";
  private const string SpriteArdiendoFallbackPath = "Imagenes/Estado_ardiendo";
  private const string SpriteVenenoFallbackPath = "Imagenes/imagen_humo";

  [Header("Ardiendo")]
  [SerializeField] private Vector2 ardiendoOffset = new Vector2(0f, -6f);
  [SerializeField] private Vector3 ardiendoOffsetMundo = new Vector3(0f, -0.158f, 0f);
  [SerializeField] private float velocidadAparicion = 6.5f;
  [SerializeField] private float velocidadDesaparicion = 9f;

  [Header("Veneno")]
  [SerializeField] private Vector2 venenoOffset = new Vector2(-4f, 16f);
  [SerializeField] private Vector3 venenoOffsetMundo = new Vector3(-0.014f, 0.055f, 0f);
  [SerializeField] private float velocidadAparicionVeneno = 5.4f;
  [SerializeField] private float velocidadDesaparicionVeneno = 7.2f;

  [Header("Acido")]
  [SerializeField] private Vector2 acidoOffset = new Vector2(-2f, 14f);
  [SerializeField] private Vector3 acidoOffsetMundo = new Vector3(-0.012f, 0.052f, 0f);
  [SerializeField] private float velocidadAparicionAcido = 5.9f;
  [SerializeField] private float velocidadDesaparicionAcido = 7.5f;

  [Header("Sangrado")]
  [SerializeField] private Vector2 sangradoOffset = new Vector2(-3.5f, 12f);
  [SerializeField] private Vector3 sangradoOffsetMundo = new Vector3(-0.026f, 0.054f, 0f);
  [SerializeField] private float velocidadAparicionSangrado = 4.8f;
  [SerializeField] private float velocidadDesaparicionSangrado = 6.6f;

  [Header("Escudado")]
  [SerializeField] private Vector2 escudadoOffset = new Vector2(-8f, 6f);
  [SerializeField] private Vector3 escudadoOffsetMundo = new Vector3(-0.02f, 0.022f, 0f);
  [SerializeField] private float velocidadAparicionEscudado = 4.7f;
  [SerializeField] private float velocidadDesaparicionEscudado = 6f;

  [Header("Barrera")]
  [SerializeField] private Vector2 barreraOffset = new Vector2(0f, 4f);
  [SerializeField] private Vector3 barreraOffsetMundo = new Vector3(0f, 0.016f, 0f);
  [SerializeField] private float velocidadAparicionBarrera = 5f;
  [SerializeField] private float velocidadDesaparicionBarrera = 6.2f;

  [Header("Condenado")]
  [SerializeField] private Vector2 condenadoOffset = new Vector2(0f, 8f);
  [SerializeField] private Vector3 condenadoOffsetMundo = new Vector3(0f, 0.024f, 0f);
  [SerializeField] private float velocidadAparicionCondenado = 5.4f;
  [SerializeField] private float velocidadDesaparicionCondenado = 6.8f;

  [Header("Aturdido")]
  [SerializeField] private Vector2 aturdidoOffset = new Vector2(0f, 110f);
  [SerializeField] private Vector3 aturdidoOffsetMundo = new Vector3(0f, 0.292f, 0f);
  [SerializeField] private float velocidadAparicionAturdido = 6.4f;
  [SerializeField] private float velocidadDesaparicionAturdido = 8.2f;

  [Header("Congelado")]
  [SerializeField] private float velocidadAparicionCongelado = 5.1f;
  [SerializeField] private float velocidadDesaparicionCongelado = 6.6f;

  private Unidad unidad;
  private RectTransform imagenUnidad;
  private RectTransform overlayArdiendoRoot;
  private CanvasGroup overlayArdiendoGroup;
  private Image overlayArdiendoGlow;
  private Image overlayArdiendoLlamaA;
  private Image overlayArdiendoLlamaB;
  private Image overlayArdiendoLlamaC;
  private Image overlayArdiendoLlamaD;
  private RectTransform overlayVenenoRoot;
  private CanvasGroup overlayVenenoGroup;
  private Image overlayVenenoNubeA;
  private Image overlayVenenoNubeB;
  private Image overlayVenenoNubeC;
  private Image overlayVenenoGotaA;
  private Image overlayVenenoGotaB;
  private RectTransform overlayAcidoRoot;
  private CanvasGroup overlayAcidoGroup;
  private Image overlayAcidoNubeA;
  private Image overlayAcidoNubeB;
  private Image overlayAcidoNubeC;
  private Image overlayAcidoGotaA;
  private Image overlayAcidoGotaB;
  private Image overlayAcidoGotaC;
  private RectTransform overlaySangradoRoot;
  private CanvasGroup overlaySangradoGroup;
  private Image overlaySangradoGotaA;
  private Image overlaySangradoGotaB;
  private Image overlaySangradoGotaC;
  private RectTransform overlayEscudadoRoot;
  private CanvasGroup overlayEscudadoGroup;
  private Image overlayEscudadoHalo;
  private Image overlayEscudadoArcoA;
  private Image overlayEscudadoArcoB;
  private Image overlayEscudadoBrillo;
  private Text overlayEscudadoPorcentaje;
  private RectTransform overlayBarreraRoot;
  private CanvasGroup overlayBarreraGroup;
  private Image overlayBarreraHalo;
  private Image overlayBarreraAro;
  private Image overlayBarreraShimmer;
  private Image overlayBarreraRunaA;
  private Image overlayBarreraRunaB;
  private RectTransform overlayCondenadoRoot;
  private CanvasGroup overlayCondenadoGroup;
  private Image overlayCondenadoAura;
  private Image overlayCondenadoSello;
  private Image overlayCondenadoMarca;
  private Image overlayCondenadoRunaA;
  private Image overlayCondenadoRunaB;
  private Image overlayCondenadoParticulaA;
  private Image overlayCondenadoParticulaB;
  private Text overlayCondenadoContador;
  private RectTransform overlayAturdidoRoot;
  private CanvasGroup overlayAturdidoGroup;
  private Image overlayAturdidoGlow;
  private Image overlayAturdidoAro;
  private Image overlayAturdidoOrbitaA;
  private Image overlayAturdidoOrbitaB;
  private Image overlayAturdidoOrbitaC;
  private RectTransform overlayCongeladoRoot;
  private CanvasGroup overlayCongeladoGroup;
  private Image overlayCongeladoTint;
  private Image overlayCongeladoGotaA;
  private Image overlayCongeladoGotaB;

  private Sprite spriteArdiendoPrincipal;
  private Sprite spriteArdiendoSecundario;
  private Sprite spriteVenenoNube;
  private Sprite spriteVenenoGota;
  private static Sprite spriteAturdidoAroGenerado;
  private static Sprite spriteVenenoParticulaGenerada;
  private static Texture2D texturaAturdidoAroGenerada;
  private static Texture2D texturaVenenoParticulaGenerada;
  private bool spritesCargados;
  private bool spritesVenenoCargados;
  private bool advirtioSpriteFaltante;
  private bool advirtioSpriteVenenoFaltante;

  private float visibilidadArdiendo;
  private float visibilidadVeneno;
  private float visibilidadAcido;
  private float visibilidadSangrado;
  private float visibilidadEscudado;
  private float visibilidadBarrera;
  private float visibilidadCondenado;
  private float visibilidadAturdido;
  private float visibilidadCongelado;
  private bool pruebaCondenadoBrutalActiva;
  private Color colorBasePruebaCondenado = Color.white;
  private float faseGlow;
  private float faseLlamaA;
  private float faseLlamaB;
  private float faseLlamaC;
  private float faseLlamaD;
  private float faseVenenoNubeA;
  private float faseVenenoNubeB;
  private float faseVenenoNubeC;
  private float faseVenenoGotaA;
  private float faseVenenoGotaB;
  private float faseAcidoNubeA;
  private float faseAcidoNubeB;
  private float faseAcidoNubeC;
  private float faseAcidoGotaA;
  private float faseAcidoGotaB;
  private float faseAcidoGotaC;
  private float faseSangradoGotaA;
  private float faseSangradoGotaB;
  private float faseSangradoGotaC;
  private float faseEscudadoHalo;
  private float faseEscudadoArcoA;
  private float faseEscudadoArcoB;
  private float faseEscudadoBrillo;
  private float faseBarreraHalo;
  private float faseBarreraAro;
  private float faseBarreraShimmer;
  private float faseBarreraRunaA;
  private float faseBarreraRunaB;
  private float faseCondenadoAura;
  private float faseCondenadoSello;
  private float faseCondenadoMarca;
  private float faseCondenadoRunaA;
  private float faseCondenadoRunaB;
  private float faseCondenadoParticulaA;
  private float faseCondenadoParticulaB;
  private float faseAturdidoBalanceo;
  private float faseAturdidoOrbitaA;
  private float faseAturdidoOrbitaB;
  private float faseAturdidoOrbitaC;
  private float faseCongeladoBrillo;
  private float faseCongeladoGotaA;
  private float faseCongeladoGotaB;

  private void Awake()
  {
    unidad = GetComponent<Unidad>();
    faseGlow = Random.Range(0f, Mathf.PI * 2f);
    faseLlamaA = Random.Range(0f, Mathf.PI * 2f);
    faseLlamaB = Random.Range(0f, Mathf.PI * 2f);
    faseLlamaC = Random.Range(0f, Mathf.PI * 2f);
    faseLlamaD = Random.Range(0f, Mathf.PI * 2f);
    faseVenenoNubeA = Random.Range(0f, Mathf.PI * 2f);
    faseVenenoNubeB = Random.Range(0f, Mathf.PI * 2f);
    faseVenenoNubeC = Random.Range(0f, Mathf.PI * 2f);
    faseVenenoGotaA = Random.Range(0f, Mathf.PI * 2f);
    faseVenenoGotaB = Random.Range(0f, Mathf.PI * 2f);
    faseAcidoNubeA = Random.Range(0f, Mathf.PI * 2f);
    faseAcidoNubeB = Random.Range(0f, Mathf.PI * 2f);
    faseAcidoNubeC = Random.Range(0f, Mathf.PI * 2f);
    faseAcidoGotaA = Random.Range(0f, Mathf.PI * 2f);
    faseAcidoGotaB = Random.Range(0f, Mathf.PI * 2f);
    faseAcidoGotaC = Random.Range(0f, Mathf.PI * 2f);
    faseSangradoGotaA = Random.Range(0f, Mathf.PI * 2f);
    faseSangradoGotaB = Random.Range(0f, Mathf.PI * 2f);
    faseSangradoGotaC = Random.Range(0f, Mathf.PI * 2f);
    faseEscudadoHalo = Random.Range(0f, Mathf.PI * 2f);
    faseEscudadoArcoA = Random.Range(0f, Mathf.PI * 2f);
    faseEscudadoArcoB = Random.Range(0f, Mathf.PI * 2f);
    faseEscudadoBrillo = Random.Range(0f, Mathf.PI * 2f);
    faseBarreraHalo = Random.Range(0f, Mathf.PI * 2f);
    faseBarreraAro = Random.Range(0f, Mathf.PI * 2f);
    faseBarreraShimmer = Random.Range(0f, Mathf.PI * 2f);
    faseBarreraRunaA = Random.Range(0f, Mathf.PI * 2f);
    faseBarreraRunaB = Random.Range(0f, Mathf.PI * 2f);
    faseCondenadoAura = Random.Range(0f, Mathf.PI * 2f);
    faseCondenadoSello = Random.Range(0f, Mathf.PI * 2f);
    faseCondenadoMarca = Random.Range(0f, Mathf.PI * 2f);
    faseCondenadoRunaA = Random.Range(0f, Mathf.PI * 2f);
    faseCondenadoRunaB = Random.Range(0f, Mathf.PI * 2f);
    faseCondenadoParticulaA = Random.Range(0f, Mathf.PI * 2f);
    faseCondenadoParticulaB = Random.Range(0f, Mathf.PI * 2f);
    faseAturdidoBalanceo = Random.Range(0f, Mathf.PI * 2f);
    faseAturdidoOrbitaA = Random.Range(0f, Mathf.PI * 2f);
    faseAturdidoOrbitaB = Random.Range(0f, Mathf.PI * 2f);
    faseAturdidoOrbitaC = Random.Range(0f, Mathf.PI * 2f);
    faseCongeladoBrillo = Random.Range(0f, Mathf.PI * 2f);
    faseCongeladoGotaA = Random.Range(0f, Mathf.PI * 2f);
    faseCongeladoGotaB = Random.Range(0f, Mathf.PI * 2f);
  }

  private void LateUpdate()
  {
    if (!VincularImagenUnidad())
    {
      return;
    }

    bool mostrarArdiendo = DebeMostrarArdiendo();
    bool mostrarVeneno = DebeMostrarVeneno();
    bool mostrarAcido = DebeMostrarAcido();
    bool mostrarSangrado = DebeMostrarSangrado();
    bool mostrarEscudado = DebeMostrarEscudado();
    bool mostrarBarrera = DebeMostrarBarrera();
    bool mostrarCondenado = DebeMostrarCondenado();
    bool mostrarAturdido = DebeMostrarAturdido();
    bool mostrarCongelado = DebeMostrarCongelado();
    if (mostrarArdiendo)
    {
      AsegurarOverlayArdiendo();
    }
    if (mostrarVeneno)
    {
      AsegurarOverlayVeneno();
    }
    if (mostrarAcido)
    {
      AsegurarOverlayAcido();
    }
    if (mostrarSangrado)
    {
      AsegurarOverlaySangrado();
    }
    if (mostrarEscudado)
    {
      AsegurarOverlayEscudado();
    }
    if (mostrarBarrera)
    {
      AsegurarOverlayBarrera();
    }
    if (mostrarCondenado)
    {
      AsegurarOverlayCondenado();
    }
    if (mostrarAturdido)
    {
      AsegurarOverlayAturdido();
    }
    if (mostrarCongelado)
    {
      AsegurarOverlayCongelado();
    }

    ActualizarOverlayArdiendo(mostrarArdiendo);
    ActualizarOverlayVeneno(mostrarVeneno);
    ActualizarOverlayAcido(mostrarAcido);
    ActualizarOverlaySangrado(mostrarSangrado);
    ActualizarOverlayEscudado(mostrarEscudado);
    ActualizarOverlayBarrera(mostrarBarrera);
    ActualizarOverlayCondenado(mostrarCondenado);
    ActualizarOverlayAturdido(mostrarAturdido);
    ActualizarOverlayCongelado(mostrarCongelado);
    ActualizarPruebaCondenadoBrutal(
      mostrarCondenado,
      mostrarCondenado ? Mathf.Max(1, unidad.estado_Condenado) : 0,
      mostrarCondenado ? Mathf.Max(1, unidad.estado_CondenadoTurnosSeguidos + 1) : 0);
  }

  private void OnDisable()
  {
    visibilidadArdiendo = 0f;
    if (overlayArdiendoRoot != null)
    {
      overlayArdiendoRoot.gameObject.SetActive(false);
    }
    visibilidadVeneno = 0f;
    if (overlayVenenoRoot != null)
    {
      overlayVenenoRoot.gameObject.SetActive(false);
    }
    visibilidadAcido = 0f;
    if (overlayAcidoRoot != null)
    {
      overlayAcidoRoot.gameObject.SetActive(false);
    }
    visibilidadSangrado = 0f;
    if (overlaySangradoRoot != null)
    {
      overlaySangradoRoot.gameObject.SetActive(false);
    }
    visibilidadEscudado = 0f;
    if (overlayEscudadoRoot != null)
    {
      overlayEscudadoRoot.gameObject.SetActive(false);
    }
    visibilidadBarrera = 0f;
    if (overlayBarreraRoot != null)
    {
      overlayBarreraRoot.gameObject.SetActive(false);
    }
    visibilidadCondenado = 0f;
    if (overlayCondenadoRoot != null)
    {
      overlayCondenadoRoot.gameObject.SetActive(false);
    }
    RestaurarPruebaCondenadoBrutal();
    visibilidadAturdido = 0f;
    if (overlayAturdidoRoot != null)
    {
      overlayAturdidoRoot.gameObject.SetActive(false);
    }
    visibilidadCongelado = 0f;
    if (overlayCongeladoRoot != null)
    {
      overlayCongeladoRoot.gameObject.SetActive(false);
    }
  }

  private void OnDestroy()
  {
    DestruirOverlayArdiendo();
    DestruirOverlayVeneno();
    DestruirOverlayAcido();
    DestruirOverlaySangrado();
    DestruirOverlayEscudado();
    DestruirOverlayBarrera();
    DestruirOverlayCondenado();
    DestruirOverlayAturdido();
    DestruirOverlayCongelado();
  }

  private bool DebeMostrarArdiendo()
  {
    return unidad != null
      && unidad.estado_ardiendo > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarVeneno()
  {
    return unidad != null
      && unidad.estado_veneno > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarAcido()
  {
    return unidad != null
      && unidad.estado_acido > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarSangrado()
  {
    return unidad != null
      && unidad.estado_sangrado > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarEscudado()
  {
    return unidad != null
      && unidad.estado_Escudado > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarBarrera()
  {
    return unidad != null
      && unidad.barreraDeDanio > 0.01f
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarCondenado()
  {
    return unidad != null
      && unidad.estado_Condenado > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarAturdido()
  {
    return unidad != null
      && unidad.estado_aturdido > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool DebeMostrarCongelado()
  {
    return unidad != null
      && unidad.estado_congelado > 0
      && unidad.HP_actual > 0f
      && gameObject.activeInHierarchy;
  }

  private bool VincularImagenUnidad()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
      if (unidad == null)
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

      if (overlayArdiendoRoot != null && overlayArdiendoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayArdiendo();
      }
      if (overlayVenenoRoot != null && overlayVenenoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayVeneno();
      }
      if (overlayAcidoRoot != null && overlayAcidoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayAcido();
      }
      if (overlaySangradoRoot != null && overlaySangradoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlaySangrado();
      }
      if (overlayEscudadoRoot != null && overlayEscudadoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayEscudado();
      }
      if (overlayBarreraRoot != null && overlayBarreraRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayBarrera();
      }
      if (overlayCondenadoRoot != null && overlayCondenadoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayCondenado();
      }
      if (overlayAturdidoRoot != null && overlayAturdidoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayAturdido();
      }
      if (overlayCongeladoRoot != null && overlayCongeladoRoot.parent != imagenUnidad.parent)
      {
        DestruirOverlayCongelado();
      }
    }

    return imagenUnidad != null;
  }

  private void ActualizarPruebaCondenadoBrutal(bool mostrarCondenado, int turnosRestantes, int turnosAcumulados)
  {
    if (unidad == null || unidad.uImage == null)
    {
      return;
    }

    if (mostrarCondenado)
    {
      if (!pruebaCondenadoBrutalActiva)
      {
        colorBasePruebaCondenado = unidad.uImage.color;
        pruebaCondenadoBrutalActiva = true;
      }

      float progresoRestante = 1f - Mathf.InverseLerp(1f, 5f, Mathf.Max(1, turnosRestantes));
      float progresoAcumulado = Mathf.InverseLerp(1f, 6f, Mathf.Max(1, turnosAcumulados));
      float velocidadPulso = Mathf.Lerp(3.36f, 5.76f, progresoRestante);
      float pulso = 0.5f + (0.5f * Mathf.Sin((Time.time * velocidadPulso) + faseCondenadoAura));
      Color colorCondenadoBase = Color.Lerp(
        new Color(0.28f, 0.04f, 0.42f, 1f),
        new Color(0.5f, 0.06f, 0.74f, 1f),
        progresoAcumulado);
      Color colorCondenadoPico = Color.Lerp(
        new Color(0.56f, 0.12f, 0.7f, 1f),
        new Color(0.82f, 0.18f, 0.9f, 1f),
        progresoAcumulado);
      Color colorCondenado = Color.Lerp(colorCondenadoBase, colorCondenadoPico, pulso);
      float fuerzaColor = Mathf.Lerp(0.5f, 0.82f, progresoAcumulado);
      unidad.uImage.color = Color.Lerp(colorBasePruebaCondenado, colorCondenado, fuerzaColor);
      return;
    }

    RestaurarPruebaCondenadoBrutal();
  }

  private void RestaurarPruebaCondenadoBrutal()
  {
    if (!pruebaCondenadoBrutalActiva)
    {
      return;
    }

    if (unidad != null && unidad.uImage != null)
    {
      unidad.uImage.color = colorBasePruebaCondenado;
    }

    pruebaCondenadoBrutalActiva = false;
  }

  private void CargarSpritesArdiendo()
  {
    if (spritesCargados)
    {
      return;
    }

    spritesCargados = true;
    spriteArdiendoPrincipal = Resources.Load<Sprite>(SpriteArdiendoPrincipalPath);
    spriteArdiendoSecundario = Resources.Load<Sprite>(SpriteArdiendoSecundarioPath);

    if (spriteArdiendoPrincipal == null)
    {
      spriteArdiendoPrincipal = Resources.Load<Sprite>(SpriteArdiendoFallbackPath);
    }

    if (spriteArdiendoSecundario == null)
    {
      spriteArdiendoSecundario = spriteArdiendoPrincipal;
    }
  }

  private void CargarSpritesVeneno()
  {
    if (spritesVenenoCargados)
    {
      return;
    }

    spritesVenenoCargados = true;
    spriteVenenoNube = ObtenerSpriteParticulaVeneno();
    spriteVenenoGota = spriteVenenoNube;

    if (spriteVenenoNube == null)
    {
      spriteVenenoNube = Resources.Load<Sprite>(SpriteVenenoFallbackPath);
    }

    if (spriteVenenoGota == null)
    {
      spriteVenenoGota = spriteVenenoNube;
    }
  }

  private static Sprite ObtenerSpriteParticulaVeneno()
  {
    if (spriteVenenoParticulaGenerada != null)
    {
      return spriteVenenoParticulaGenerada;
    }

    const int size = 64;
    texturaVenenoParticulaGenerada = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaVenenoParticulaGenerada.name = "VenenoParticulaSuaveRuntime";
    texturaVenenoParticulaGenerada.wrapMode = TextureWrapMode.Clamp;
    texturaVenenoParticulaGenerada.filterMode = FilterMode.Bilinear;
    texturaVenenoParticulaGenerada.hideFlags = HideFlags.HideAndDontSave;

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

    texturaVenenoParticulaGenerada.SetPixels(pixels);
    texturaVenenoParticulaGenerada.Apply(false, true);
    spriteVenenoParticulaGenerada = Sprite.Create(
      texturaVenenoParticulaGenerada,
      new Rect(0f, 0f, size, size),
      new Vector2(0.5f, 0.5f),
      size);
    spriteVenenoParticulaGenerada.name = "VenenoParticulaSuaveRuntime";
    return spriteVenenoParticulaGenerada;
  }

  private static Sprite ObtenerSpriteAroAturdido()
  {
    if (spriteAturdidoAroGenerado != null)
    {
      return spriteAturdidoAroGenerado;
    }

    const int size = 96;
    texturaAturdidoAroGenerada = new Texture2D(size, size, TextureFormat.ARGB32, false);
    texturaAturdidoAroGenerada.name = "AturdidoAroRuntime";
    texturaAturdidoAroGenerada.wrapMode = TextureWrapMode.Clamp;
    texturaAturdidoAroGenerada.filterMode = FilterMode.Bilinear;
    texturaAturdidoAroGenerada.hideFlags = HideFlags.HideAndDontSave;

    Color[] pixels = new Color[size * size];
    float centro = (size - 1) * 0.5f;
    float radio = size * 0.5f;
    const float radioAro = 0.67f;
    const float grosorAro = 0.17f;

    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float dx = (x - centro) / radio;
        float dy = (y - centro) / radio;
        float distancia = Mathf.Sqrt((dx * dx) + (dy * dy));
        float delta = Mathf.Abs(distancia - radioAro);
        float alpha = Mathf.Clamp01(1f - (delta / grosorAro));
        alpha = Mathf.Pow(alpha, 1.85f);
        pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    texturaAturdidoAroGenerada.SetPixels(pixels);
    texturaAturdidoAroGenerada.Apply(false, true);
    spriteAturdidoAroGenerado = Sprite.Create(
      texturaAturdidoAroGenerada,
      new Rect(0f, 0f, size, size),
      new Vector2(0.5f, 0.5f),
      size);
    spriteAturdidoAroGenerado.name = "AturdidoAroRuntime";
    return spriteAturdidoAroGenerado;
  }

  private void AsegurarOverlayArdiendo()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesArdiendo();
    if (spriteArdiendoPrincipal == null)
    {
      if (!advirtioSpriteFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontro sprite para el efecto Ardiendo.");
        advirtioSpriteFaltante = true;
      }
      return;
    }

    if (overlayArdiendoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Ardiendo", typeof(RectTransform), typeof(CanvasGroup));
    overlayArdiendoRoot = rootGo.GetComponent<RectTransform>();
    overlayArdiendoGroup = rootGo.GetComponent<CanvasGroup>();
    overlayArdiendoRoot.SetParent(imagenUnidad.parent, false);
    overlayArdiendoGroup.interactable = false;
    overlayArdiendoGroup.blocksRaycasts = false;
    overlayArdiendoGroup.alpha = 0f;

    Sprite spriteVertical = spriteArdiendoSecundario != null ? spriteArdiendoSecundario : spriteArdiendoPrincipal;
    overlayArdiendoGlow = CrearCapaArdiendo("Glow", spriteVertical);
    overlayArdiendoLlamaA = CrearCapaArdiendo("LlamaA", spriteVertical);
    overlayArdiendoLlamaB = CrearCapaArdiendo("LlamaB", spriteVertical);
    overlayArdiendoLlamaC = CrearCapaArdiendo("LlamaC", spriteVertical);
    overlayArdiendoLlamaD = CrearCapaArdiendo("LlamaD", spriteVertical);

    SincronizarRootConUnidad();
    overlayArdiendoRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlayVeneno()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    if (spriteVenenoNube == null || spriteVenenoGota == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Veneno.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlayVenenoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Veneno", typeof(RectTransform), typeof(CanvasGroup));
    overlayVenenoRoot = rootGo.GetComponent<RectTransform>();
    overlayVenenoGroup = rootGo.GetComponent<CanvasGroup>();
    overlayVenenoRoot.SetParent(imagenUnidad.parent, false);
    overlayVenenoGroup.interactable = false;
    overlayVenenoGroup.blocksRaycasts = false;
    overlayVenenoGroup.alpha = 0f;

    overlayVenenoNubeA = CrearCapaParticula(overlayVenenoRoot, "NubeA", spriteVenenoNube);
    overlayVenenoNubeB = CrearCapaParticula(overlayVenenoRoot, "NubeB", spriteVenenoNube);
    overlayVenenoNubeC = CrearCapaParticula(overlayVenenoRoot, "NubeC", spriteVenenoNube);
    overlayVenenoGotaA = CrearCapaParticula(overlayVenenoRoot, "GotaA", spriteVenenoGota);
    overlayVenenoGotaB = CrearCapaParticula(overlayVenenoRoot, "GotaB", spriteVenenoGota);

    SincronizarRootVeneno();
    overlayVenenoRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlayAcido()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    if (spriteVenenoNube == null || spriteVenenoGota == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Acido.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlayAcidoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Acido", typeof(RectTransform), typeof(CanvasGroup));
    overlayAcidoRoot = rootGo.GetComponent<RectTransform>();
    overlayAcidoGroup = rootGo.GetComponent<CanvasGroup>();
    overlayAcidoRoot.SetParent(imagenUnidad.parent, false);
    overlayAcidoGroup.interactable = false;
    overlayAcidoGroup.blocksRaycasts = false;
    overlayAcidoGroup.alpha = 0f;

    overlayAcidoNubeA = CrearCapaParticula(overlayAcidoRoot, "NubeA", spriteVenenoNube);
    overlayAcidoNubeB = CrearCapaParticula(overlayAcidoRoot, "NubeB", spriteVenenoNube);
    overlayAcidoNubeC = CrearCapaParticula(overlayAcidoRoot, "NubeC", spriteVenenoNube);
    overlayAcidoGotaA = CrearCapaParticula(overlayAcidoRoot, "GotaA", spriteVenenoGota);
    overlayAcidoGotaB = CrearCapaParticula(overlayAcidoRoot, "GotaB", spriteVenenoGota);
    overlayAcidoGotaC = CrearCapaParticula(overlayAcidoRoot, "GotaC", spriteVenenoGota);

    SincronizarRootAcido();
    overlayAcidoRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlaySangrado()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    if (spriteVenenoGota == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Sangrado.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlaySangradoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Sangrado", typeof(RectTransform), typeof(CanvasGroup));
    overlaySangradoRoot = rootGo.GetComponent<RectTransform>();
    overlaySangradoGroup = rootGo.GetComponent<CanvasGroup>();
    overlaySangradoRoot.SetParent(imagenUnidad.parent, false);
    overlaySangradoGroup.interactable = false;
    overlaySangradoGroup.blocksRaycasts = false;
    overlaySangradoGroup.alpha = 0f;

    overlaySangradoGotaA = CrearCapaParticula(overlaySangradoRoot, "GotaA", spriteVenenoGota);
    overlaySangradoGotaB = CrearCapaParticula(overlaySangradoRoot, "GotaB", spriteVenenoGota);
    overlaySangradoGotaC = CrearCapaParticula(overlaySangradoRoot, "GotaC", spriteVenenoGota);

    SincronizarRootSangrado();
    overlaySangradoRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlayEscudado()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    Sprite spriteAro = ObtenerSpriteAroAturdido();
    if (spriteVenenoNube == null || spriteAro == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Escudado.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlayEscudadoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Escudado", typeof(RectTransform), typeof(CanvasGroup));
    overlayEscudadoRoot = rootGo.GetComponent<RectTransform>();
    overlayEscudadoGroup = rootGo.GetComponent<CanvasGroup>();
    overlayEscudadoRoot.SetParent(imagenUnidad.parent, false);
    overlayEscudadoGroup.interactable = false;
    overlayEscudadoGroup.blocksRaycasts = false;
    overlayEscudadoGroup.alpha = 0f;

    overlayEscudadoHalo = CrearCapaParticula(overlayEscudadoRoot, "Halo", spriteVenenoNube);
    overlayEscudadoArcoA = CrearCapaParticula(overlayEscudadoRoot, "ArcoA", spriteAro);
    overlayEscudadoArcoB = CrearCapaParticula(overlayEscudadoRoot, "ArcoB", spriteAro);
    overlayEscudadoBrillo = CrearCapaParticula(overlayEscudadoRoot, "Brillo", spriteVenenoNube);
    overlayEscudadoHalo.preserveAspect = false;
    overlayEscudadoArcoA.preserveAspect = false;
    overlayEscudadoArcoB.preserveAspect = false;
    overlayEscudadoBrillo.preserveAspect = true;
    overlayEscudadoPorcentaje = CrearTextoEstado(overlayEscudadoRoot, "PorcentajeEscudado");
    overlayEscudadoPorcentaje.fontStyle = FontStyle.Normal;
    Outline outlinePorcentaje = overlayEscudadoPorcentaje.GetComponent<Outline>();
    if (outlinePorcentaje != null)
    {
      outlinePorcentaje.effectColor = new Color(0.08f, 0.11f, 0.16f, 0.46f);
      outlinePorcentaje.effectDistance = new Vector2(0.45f, -0.45f);
    }

    SincronizarRootEscudado();
    overlayEscudadoRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlayBarrera()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    Sprite spriteAro = ObtenerSpriteAroAturdido();
    if (spriteVenenoNube == null || spriteAro == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Barrera.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlayBarreraRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Barrera", typeof(RectTransform), typeof(CanvasGroup));
    overlayBarreraRoot = rootGo.GetComponent<RectTransform>();
    overlayBarreraGroup = rootGo.GetComponent<CanvasGroup>();
    overlayBarreraRoot.SetParent(imagenUnidad.parent, false);
    overlayBarreraGroup.interactable = false;
    overlayBarreraGroup.blocksRaycasts = false;
    overlayBarreraGroup.alpha = 0f;

    overlayBarreraHalo = CrearCapaParticula(overlayBarreraRoot, "Halo", spriteVenenoNube);
    overlayBarreraAro = CrearCapaParticula(overlayBarreraRoot, "Aro", spriteAro);
    overlayBarreraShimmer = CrearCapaParticula(overlayBarreraRoot, "Shimmer", spriteVenenoNube);
    overlayBarreraRunaA = CrearCapaParticula(overlayBarreraRoot, "RunaA", spriteAro);
    overlayBarreraRunaB = CrearCapaParticula(overlayBarreraRoot, "RunaB", spriteAro);

    overlayBarreraHalo.preserveAspect = false;
    overlayBarreraAro.preserveAspect = false;
    overlayBarreraShimmer.preserveAspect = false;
    overlayBarreraRunaA.preserveAspect = false;
    overlayBarreraRunaB.preserveAspect = false;

    SincronizarRootBarrera();
    overlayBarreraRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlayCondenado()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    Sprite spriteAro = ObtenerSpriteAroAturdido();
    if (spriteVenenoNube == null || spriteAro == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Condenado.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlayCondenadoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Condenado", typeof(RectTransform), typeof(CanvasGroup));
    overlayCondenadoRoot = rootGo.GetComponent<RectTransform>();
    overlayCondenadoGroup = rootGo.GetComponent<CanvasGroup>();
    overlayCondenadoRoot.SetParent(imagenUnidad.parent, false);
    overlayCondenadoGroup.interactable = false;
    overlayCondenadoGroup.blocksRaycasts = false;
    overlayCondenadoGroup.alpha = 0f;

    overlayCondenadoAura = CrearCapaParticula(overlayCondenadoRoot, "Aura", spriteVenenoNube);
    overlayCondenadoSello = CrearCapaParticula(overlayCondenadoRoot, "Sello", spriteAro);
    overlayCondenadoMarca = CrearCapaParticula(overlayCondenadoRoot, "Marca", spriteVenenoNube);
    overlayCondenadoRunaA = CrearCapaParticula(overlayCondenadoRoot, "RunaA", spriteVenenoNube);
    overlayCondenadoRunaB = CrearCapaParticula(overlayCondenadoRoot, "RunaB", spriteVenenoNube);
    overlayCondenadoParticulaA = CrearCapaParticula(overlayCondenadoRoot, "ParticulaA", spriteVenenoNube);
    overlayCondenadoParticulaB = CrearCapaParticula(overlayCondenadoRoot, "ParticulaB", spriteVenenoNube);
    overlayCondenadoAura.preserveAspect = true;
    overlayCondenadoSello.preserveAspect = true;
    overlayCondenadoMarca.preserveAspect = true;
    overlayCondenadoRunaA.preserveAspect = true;
    overlayCondenadoRunaB.preserveAspect = true;
    overlayCondenadoParticulaA.preserveAspect = true;
    overlayCondenadoParticulaB.preserveAspect = true;
    overlayCondenadoContador = CrearTextoEstado(overlayCondenadoRoot, "ContadorCondenado");
    overlayCondenadoContador.fontStyle = FontStyle.Normal;
    Outline outlineContadorCondenado = overlayCondenadoContador.GetComponent<Outline>();
    if (outlineContadorCondenado != null)
    {
      outlineContadorCondenado.effectColor = new Color(0.08f, 0.02f, 0.1f, 0.72f);
      outlineContadorCondenado.effectDistance = new Vector2(0.35f, -0.35f);
    }

    SincronizarRootCondenado();
    overlayCondenadoRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlayAturdido()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    Sprite spriteAro = ObtenerSpriteAroAturdido();
    if (spriteAro == null || spriteVenenoNube == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Aturdido.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlayAturdidoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Aturdido", typeof(RectTransform), typeof(CanvasGroup));
    overlayAturdidoRoot = rootGo.GetComponent<RectTransform>();
    overlayAturdidoGroup = rootGo.GetComponent<CanvasGroup>();
    overlayAturdidoRoot.SetParent(imagenUnidad.parent, false);
    overlayAturdidoGroup.interactable = false;
    overlayAturdidoGroup.blocksRaycasts = false;
    overlayAturdidoGroup.alpha = 0f;

    overlayAturdidoGlow = CrearCapaParticula(overlayAturdidoRoot, "Glow", spriteVenenoNube);
    overlayAturdidoAro = CrearCapaParticula(overlayAturdidoRoot, "Aro", spriteAro);
    overlayAturdidoOrbitaA = CrearCapaParticula(overlayAturdidoRoot, "OrbitaA", spriteVenenoNube);
    overlayAturdidoOrbitaB = CrearCapaParticula(overlayAturdidoRoot, "OrbitaB", spriteVenenoNube);
    overlayAturdidoOrbitaC = CrearCapaParticula(overlayAturdidoRoot, "OrbitaC", spriteVenenoNube);

    SincronizarRootAturdido();
    overlayAturdidoRoot.gameObject.SetActive(false);
  }

  private void AsegurarOverlayCongelado()
  {
    if (imagenUnidad == null)
    {
      return;
    }

    CargarSpritesVeneno();
    if (spriteVenenoGota == null)
    {
      if (!advirtioSpriteVenenoFaltante)
      {
        Debug.LogWarning("[UnidadStatusVfxController] No se encontraron sprites para el efecto Congelado.");
        advirtioSpriteVenenoFaltante = true;
      }
      return;
    }

    if (overlayCongeladoRoot != null)
    {
      return;
    }

    GameObject rootGo = new GameObject("StatusVfx_Congelado", typeof(RectTransform), typeof(CanvasGroup));
    overlayCongeladoRoot = rootGo.GetComponent<RectTransform>();
    overlayCongeladoGroup = rootGo.GetComponent<CanvasGroup>();
    overlayCongeladoRoot.SetParent(imagenUnidad.parent, false);
    overlayCongeladoGroup.interactable = false;
    overlayCongeladoGroup.blocksRaycasts = false;
    overlayCongeladoGroup.alpha = 0f;

    overlayCongeladoTint = CrearCapaCongeladoTint();
    overlayCongeladoGotaA = CrearCapaParticula(overlayCongeladoRoot, "GotaA", spriteVenenoGota);
    overlayCongeladoGotaB = CrearCapaParticula(overlayCongeladoRoot, "GotaB", spriteVenenoGota);

    SincronizarRootCongelado();
    overlayCongeladoRoot.gameObject.SetActive(false);
  }

  private Image CrearCapaArdiendo(string nombre, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(overlayArdiendoRoot, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    image.preserveAspect = false;
    return image;
  }

  private Image CrearCapaParticula(RectTransform root, string nombre, Sprite sprite)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(root, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.sprite = sprite;
    image.raycastTarget = false;
    image.maskable = false;
    image.preserveAspect = true;
    return image;
  }

  private Text CrearTextoEstado(RectTransform root, string nombre)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(root, false);
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);

    Text text = go.GetComponent<Text>();
    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    text.fontStyle = FontStyle.Bold;
    text.alignment = TextAnchor.MiddleCenter;
    text.horizontalOverflow = HorizontalWrapMode.Overflow;
    text.verticalOverflow = VerticalWrapMode.Overflow;
    text.supportRichText = false;
    text.raycastTarget = false;

    Outline outline = go.GetComponent<Outline>();
    outline.effectColor = new Color(0.08f, 0.01f, 0.01f, 0.9f);
    outline.effectDistance = new Vector2(1f, -1f);
    return text;
  }

  private Image CrearCapaCongeladoTint()
  {
    GameObject go = new GameObject("Tint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.SetParent(overlayCongeladoRoot, false);
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
    rect.pivot = new Vector2(0.5f, 0.5f);

    Image image = go.GetComponent<Image>();
    image.raycastTarget = false;
    image.maskable = false;
    image.preserveAspect = false;
    return image;
  }

  private void ActualizarOverlayArdiendo(bool mostrarArdiendo)
  {
    if (overlayArdiendoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarArdiendo ? velocidadAparicion : velocidadDesaparicion;
    visibilidadArdiendo = Mathf.MoveTowards(visibilidadArdiendo, mostrarArdiendo ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarArdiendo || visibilidadArdiendo > 0.001f;
    if (overlayArdiendoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayArdiendoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootConUnidad();
    ActualizarVisualArdiendo(mostrarArdiendo ? Mathf.Max(1, unidad.estado_ardiendo) : 1);
  }

  private void ActualizarOverlayVeneno(bool mostrarVeneno)
  {
    if (overlayVenenoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarVeneno ? velocidadAparicionVeneno : velocidadDesaparicionVeneno;
    visibilidadVeneno = Mathf.MoveTowards(visibilidadVeneno, mostrarVeneno ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarVeneno || visibilidadVeneno > 0.001f;
    if (overlayVenenoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayVenenoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootVeneno();
    ActualizarVisualVeneno(mostrarVeneno ? Mathf.Max(1, unidad.estado_veneno) : 1);
  }

  private void ActualizarOverlayAcido(bool mostrarAcido)
  {
    if (overlayAcidoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarAcido ? velocidadAparicionAcido : velocidadDesaparicionAcido;
    visibilidadAcido = Mathf.MoveTowards(visibilidadAcido, mostrarAcido ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarAcido || visibilidadAcido > 0.001f;
    if (overlayAcidoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayAcidoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootAcido();
    ActualizarVisualAcido(mostrarAcido ? Mathf.Max(1, unidad.estado_acido) : 1);
  }

  private void ActualizarOverlaySangrado(bool mostrarSangrado)
  {
    if (overlaySangradoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarSangrado ? velocidadAparicionSangrado : velocidadDesaparicionSangrado;
    visibilidadSangrado = Mathf.MoveTowards(visibilidadSangrado, mostrarSangrado ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarSangrado || visibilidadSangrado > 0.001f;
    if (overlaySangradoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlaySangradoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootSangrado();
    ActualizarVisualSangrado(mostrarSangrado ? Mathf.Max(1, unidad.estado_sangrado) : 1);
  }

  private void ActualizarOverlayEscudado(bool mostrarEscudado)
  {
    if (overlayEscudadoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarEscudado ? velocidadAparicionEscudado : velocidadDesaparicionEscudado;
    visibilidadEscudado = Mathf.MoveTowards(visibilidadEscudado, mostrarEscudado ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarEscudado || visibilidadEscudado > 0.001f;
    if (overlayEscudadoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayEscudadoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootEscudado();
    ActualizarVisualEscudado(mostrarEscudado ? Mathf.Max(1, unidad.estado_Escudado) : 1);
  }

  private void ActualizarOverlayBarrera(bool mostrarBarrera)
  {
    if (overlayBarreraRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarBarrera ? velocidadAparicionBarrera : velocidadDesaparicionBarrera;
    visibilidadBarrera = Mathf.MoveTowards(visibilidadBarrera, mostrarBarrera ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarBarrera || visibilidadBarrera > 0.001f;
    if (overlayBarreraRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayBarreraRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootBarrera();
    ActualizarVisualBarrera(mostrarBarrera ? Mathf.Max(1f, unidad.barreraDeDanio) : 1f);
  }

  private void ActualizarOverlayCondenado(bool mostrarCondenado)
  {
    if (overlayCondenadoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarCondenado ? velocidadAparicionCondenado : velocidadDesaparicionCondenado;
    visibilidadCondenado = Mathf.MoveTowards(visibilidadCondenado, mostrarCondenado ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarCondenado || visibilidadCondenado > 0.001f;
    if (overlayCondenadoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayCondenadoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootCondenado();
    int turnosRestantes = mostrarCondenado ? Mathf.Max(1, unidad.estado_Condenado) : 1;
    int turnosAcumulados = mostrarCondenado ? Mathf.Max(1, unidad.estado_CondenadoTurnosSeguidos + 1) : 1;
    ActualizarVisualCondenado(turnosRestantes, turnosAcumulados);
  }

  private void ActualizarOverlayAturdido(bool mostrarAturdido)
  {
    if (overlayAturdidoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarAturdido ? velocidadAparicionAturdido : velocidadDesaparicionAturdido;
    visibilidadAturdido = Mathf.MoveTowards(visibilidadAturdido, mostrarAturdido ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarAturdido || visibilidadAturdido > 0.001f;
    if (overlayAturdidoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayAturdidoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootAturdido();
    ActualizarVisualAturdido(mostrarAturdido ? Mathf.Max(1, unidad.estado_aturdido) : 1);
  }

  private void ActualizarOverlayCongelado(bool mostrarCongelado)
  {
    if (overlayCongeladoRoot == null)
    {
      return;
    }

    float dt = Mathf.Max(0.0001f, Time.deltaTime);
    float velocidad = mostrarCongelado ? velocidadAparicionCongelado : velocidadDesaparicionCongelado;
    visibilidadCongelado = Mathf.MoveTowards(visibilidadCongelado, mostrarCongelado ? 1f : 0f, dt * Mathf.Max(0.1f, velocidad));

    bool debeSeguirActivo = mostrarCongelado || visibilidadCongelado > 0.001f;
    if (overlayCongeladoRoot.gameObject.activeSelf != debeSeguirActivo)
    {
      overlayCongeladoRoot.gameObject.SetActive(debeSeguirActivo);
    }

    if (!debeSeguirActivo)
    {
      return;
    }

    SincronizarRootCongelado();
    ActualizarVisualCongelado(mostrarCongelado ? Mathf.Max(1, unidad.estado_congelado) : 1);
  }

  private void SincronizarRootConUnidad()
  {
    if (imagenUnidad == null || overlayArdiendoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayArdiendoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayArdiendoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayArdiendoRoot.pivot = new Vector2(0.5f, 0f);
    overlayArdiendoRoot.sizeDelta = new Vector2(
      Mathf.Max(14f, tamanoUnidad.x * 0.38f),
      Mathf.Max(18f, tamanoUnidad.y * 0.62f));

    Canvas canvasPadre = overlayArdiendoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace
      && unidad != null
      && unidad.puntoEntrante != null)
    {
      overlayArdiendoRoot.position = unidad.puntoEntrante.position + ardiendoOffsetMundo;
    }
    else if (TryObtenerPosicionBaseEstado(out Vector2 posicionBase))
    {
      overlayArdiendoRoot.anchoredPosition = posicionBase + ardiendoOffset;
    }
    else
    {
      overlayArdiendoRoot.anchoredPosition = imagenUnidad.anchoredPosition + ardiendoOffset;
    }

    overlayArdiendoRoot.localEulerAngles = Vector3.zero;
    overlayArdiendoRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayArdiendoRoot.parent.childCount - 1);
    if (overlayArdiendoRoot.GetSiblingIndex() != targetSibling)
    {
      overlayArdiendoRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootVeneno()
  {
    if (imagenUnidad == null || overlayVenenoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayVenenoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayVenenoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayVenenoRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayVenenoRoot.sizeDelta = new Vector2(
      Mathf.Max(20f, tamanoUnidad.x * 0.56f),
      Mathf.Max(18f, tamanoUnidad.y * 0.46f));

    Canvas canvasPadre = overlayVenenoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlayVenenoRoot.position = imagenUnidad.position + venenoOffsetMundo;
    }
    else
    {
      overlayVenenoRoot.anchoredPosition = imagenUnidad.anchoredPosition + venenoOffset;
    }

    overlayVenenoRoot.localEulerAngles = Vector3.zero;
    overlayVenenoRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayVenenoRoot.parent.childCount - 1);
    if (overlayVenenoRoot.GetSiblingIndex() != targetSibling)
    {
      overlayVenenoRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootAcido()
  {
    if (imagenUnidad == null || overlayAcidoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayAcidoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayAcidoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayAcidoRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayAcidoRoot.sizeDelta = new Vector2(
      Mathf.Max(18f, tamanoUnidad.x * 0.48f),
      Mathf.Max(16f, tamanoUnidad.y * 0.4f));

    Canvas canvasPadre = overlayAcidoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlayAcidoRoot.position = imagenUnidad.position + acidoOffsetMundo;
    }
    else
    {
      overlayAcidoRoot.anchoredPosition = imagenUnidad.anchoredPosition + acidoOffset;
    }

    overlayAcidoRoot.localEulerAngles = Vector3.zero;
    overlayAcidoRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayAcidoRoot.parent.childCount - 1);
    if (overlayAcidoRoot.GetSiblingIndex() != targetSibling)
    {
      overlayAcidoRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootSangrado()
  {
    if (imagenUnidad == null || overlaySangradoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlaySangradoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlaySangradoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlaySangradoRoot.pivot = new Vector2(0.5f, 0.5f);
    overlaySangradoRoot.sizeDelta = new Vector2(
      Mathf.Max(24f, tamanoUnidad.x * 0.46f),
      Mathf.Max(24f, tamanoUnidad.y * 0.46f));

    Canvas canvasPadre = overlaySangradoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlaySangradoRoot.position = imagenUnidad.position + sangradoOffsetMundo;
    }
    else
    {
      overlaySangradoRoot.anchoredPosition = imagenUnidad.anchoredPosition + sangradoOffset;
    }

    overlaySangradoRoot.localEulerAngles = Vector3.zero;
    overlaySangradoRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlaySangradoRoot.parent.childCount - 1);
    if (overlaySangradoRoot.GetSiblingIndex() != targetSibling)
    {
      overlaySangradoRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootEscudado()
  {
    if (imagenUnidad == null || overlayEscudadoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayEscudadoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayEscudadoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayEscudadoRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayEscudadoRoot.sizeDelta = new Vector2(
      Mathf.Max(24f, tamanoUnidad.x * 0.62f),
      Mathf.Max(26f, tamanoUnidad.y * 0.72f));

    Canvas canvasPadre = overlayEscudadoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlayEscudadoRoot.position = imagenUnidad.position + escudadoOffsetMundo;
    }
    else
    {
      overlayEscudadoRoot.anchoredPosition = imagenUnidad.anchoredPosition + escudadoOffset;
    }

    overlayEscudadoRoot.localEulerAngles = Vector3.zero;
    overlayEscudadoRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayEscudadoRoot.parent.childCount - 1);
    if (overlayEscudadoRoot.GetSiblingIndex() != targetSibling)
    {
      overlayEscudadoRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootBarrera()
  {
    if (imagenUnidad == null || overlayBarreraRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayBarreraRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayBarreraRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayBarreraRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayBarreraRoot.sizeDelta = new Vector2(
      Mathf.Max(26f, tamanoUnidad.x * 1.04f),
      Mathf.Max(30f, tamanoUnidad.y * 1.1f));

    Canvas canvasPadre = overlayBarreraRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlayBarreraRoot.position = imagenUnidad.position + barreraOffsetMundo;
    }
    else
    {
      overlayBarreraRoot.anchoredPosition = imagenUnidad.anchoredPosition + barreraOffset;
    }

    overlayBarreraRoot.localEulerAngles = Vector3.zero;
    overlayBarreraRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayBarreraRoot.parent.childCount - 1);
    if (overlayBarreraRoot.GetSiblingIndex() != targetSibling)
    {
      overlayBarreraRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootCondenado()
  {
    if (imagenUnidad == null || overlayCondenadoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayCondenadoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayCondenadoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayCondenadoRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayCondenadoRoot.sizeDelta = new Vector2(
      Mathf.Max(28f, tamanoUnidad.x * 0.82f),
      Mathf.Max(32f, tamanoUnidad.y * 0.92f));

    Canvas canvasPadre = overlayCondenadoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlayCondenadoRoot.position = imagenUnidad.position + condenadoOffsetMundo;
    }
    else
    {
      overlayCondenadoRoot.anchoredPosition = imagenUnidad.anchoredPosition + condenadoOffset;
    }

    overlayCondenadoRoot.localEulerAngles = Vector3.zero;
    overlayCondenadoRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayCondenadoRoot.parent.childCount - 1);
    if (overlayCondenadoRoot.GetSiblingIndex() != targetSibling)
    {
      overlayCondenadoRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootAturdido()
  {
    if (imagenUnidad == null || overlayAturdidoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayAturdidoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayAturdidoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayAturdidoRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayAturdidoRoot.sizeDelta = new Vector2(
      Mathf.Max(24f, tamanoUnidad.x * 0.72f),
      Mathf.Max(16f, tamanoUnidad.y * 0.34f));

    Canvas canvasPadre = overlayAturdidoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlayAturdidoRoot.position = imagenUnidad.position + aturdidoOffsetMundo;
    }
    else
    {
      overlayAturdidoRoot.anchoredPosition = imagenUnidad.anchoredPosition + aturdidoOffset;
    }

    overlayAturdidoRoot.localEulerAngles = Vector3.zero;
    overlayAturdidoRoot.localScale = Vector3.one;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayAturdidoRoot.parent.childCount - 1);
    if (overlayAturdidoRoot.GetSiblingIndex() != targetSibling)
    {
      overlayAturdidoRoot.SetSiblingIndex(targetSibling);
    }
  }

  private void SincronizarRootCongelado()
  {
    if (imagenUnidad == null || overlayCongeladoRoot == null)
    {
      return;
    }

    Vector2 tamanoUnidad = ObtenerTamanoUnidad();
    overlayCongeladoRoot.anchorMin = new Vector2(0.5f, 0.5f);
    overlayCongeladoRoot.anchorMax = new Vector2(0.5f, 0.5f);
    overlayCongeladoRoot.pivot = new Vector2(0.5f, 0.5f);
    overlayCongeladoRoot.sizeDelta = tamanoUnidad;

    Canvas canvasPadre = overlayCongeladoRoot.GetComponentInParent<Canvas>();
    if (canvasPadre != null
      && canvasPadre.renderMode == RenderMode.WorldSpace)
    {
      overlayCongeladoRoot.position = imagenUnidad.position;
    }
    else
    {
      overlayCongeladoRoot.anchoredPosition = imagenUnidad.anchoredPosition;
    }

    overlayCongeladoRoot.localEulerAngles = imagenUnidad.localEulerAngles;
    overlayCongeladoRoot.localScale = imagenUnidad.localScale;

    int sibling = imagenUnidad.GetSiblingIndex();
    int targetSibling = Mathf.Min(sibling + 1, overlayCongeladoRoot.parent.childCount - 1);
    if (overlayCongeladoRoot.GetSiblingIndex() != targetSibling)
    {
      overlayCongeladoRoot.SetSiblingIndex(targetSibling);
    }

    if (overlayCongeladoTint != null)
    {
      Image imagenBase = unidad != null ? unidad.uImage : null;
      if (imagenBase != null)
      {
        overlayCongeladoTint.sprite = imagenBase.sprite;
        overlayCongeladoTint.overrideSprite = imagenBase.overrideSprite;
        overlayCongeladoTint.type = imagenBase.type;
        overlayCongeladoTint.preserveAspect = imagenBase.preserveAspect;
      }

      overlayCongeladoTint.rectTransform.localScale = new Vector3(1.06f, 1.06f, 1f);
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

  private void ActualizarVisualArdiendo(int stacks)
  {
    if (overlayArdiendoGroup == null
      || overlayArdiendoGlow == null
      || overlayArdiendoLlamaA == null
      || overlayArdiendoLlamaB == null
      || overlayArdiendoLlamaC == null
      || overlayArdiendoLlamaD == null)
    {
      return;
    }

    float t = Time.time;
    float intensidad = Mathf.Lerp(0.88f, 1.16f, Mathf.InverseLerp(1f, 4f, stacks));
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(12f, tamano.x * 0.33f);
    float alto = Mathf.Max(18f, tamano.y * 0.62f);

    float caosGlow = OscilacionFuego(faseGlow, t, 4.8f);
    float caosA = OscilacionFuego(faseLlamaA, t, 6.1f);
    float caosB = OscilacionFuego(faseLlamaB, t, 6.9f);
    float caosC = OscilacionFuego(faseLlamaC, t, 5.7f);
    float caosD = OscilacionFuego(faseLlamaD, t, 5.35f);
    float ascensoGlow = Mathf.Abs(OscilacionFuego(faseGlow + 2.7f, t, 3.9f));
    float ascensoA = Mathf.Abs(OscilacionFuego(faseLlamaA + 4.3f, t, 5.4f));
    float ascensoB = Mathf.Abs(OscilacionFuego(faseLlamaB + 5.1f, t, 5.9f));
    float ascensoC = Mathf.Abs(OscilacionFuego(faseLlamaC + 1.6f, t, 5.1f));
    float ascensoD = Mathf.Abs(OscilacionFuego(faseLlamaD + 3.2f, t, 4.8f));

    overlayArdiendoGroup.alpha = visibilidadArdiendo * 0.88f;

    ConfigurarCapa(
      overlayArdiendoGlow,
      new Vector2(caosGlow * 0.48f, -alto * 0.31f + ascensoGlow * 0.42f),
      new Vector2(ancho * 1.62f, alto * 0.56f) * (0.94f + ascensoGlow * 0.08f) * intensidad,
      new Color(1f, 0.42f, 0.08f, Mathf.Lerp(0.08f, 0.145f, ascensoGlow)));

    ConfigurarCapa(
      overlayArdiendoLlamaA,
      new Vector2(caosA * 0.6f, -alto * 0.42f + ascensoA * 1.02f),
      new Vector2(ancho * 0.72f, alto * 0.94f) * (0.92f + ascensoA * 0.12f) * intensidad,
      new Color(1f, 0.57f, 0.18f, Mathf.Lerp(0.16f, 0.25f, ascensoA)));

    ConfigurarCapa(
      overlayArdiendoLlamaB,
      new Vector2((ancho * 0.04f) + (caosB * 0.5f), -alto * 0.37f + ascensoB * 0.9f),
      new Vector2(ancho * 0.56f, alto * 0.76f) * (0.9f + ascensoB * 0.14f) * intensidad,
      new Color(1f, 0.75f, 0.31f, Mathf.Lerp(0.13f, 0.2f, ascensoB)));

    ConfigurarCapa(
      overlayArdiendoLlamaC,
      new Vector2(-ancho * 0.28f + (caosC * 0.4f), -alto * 0.4f + ascensoC * 0.8f),
      new Vector2(ancho * 0.46f, alto * 0.67f) * (0.9f + ascensoC * 0.12f) * intensidad,
      new Color(1f, 0.61f, 0.2f, Mathf.Lerp(0.1f, 0.16f, ascensoC)));

    ConfigurarCapa(
      overlayArdiendoLlamaD,
      new Vector2(ancho * 0.19f + (caosD * 0.28f), -alto * 0.35f + ascensoD * 0.72f),
      new Vector2(ancho * 0.38f, alto * 0.58f) * (0.9f + ascensoD * 0.12f) * intensidad,
      new Color(1f, 0.66f, 0.22f, Mathf.Lerp(0.1f, 0.16f, ascensoD)));
  }

  private void ActualizarVisualVeneno(int stacks)
  {
    if (overlayVenenoGroup == null
      || overlayVenenoNubeA == null
      || overlayVenenoNubeB == null
      || overlayVenenoNubeC == null
      || overlayVenenoGotaA == null
      || overlayVenenoGotaB == null)
    {
      return;
    }

    float t = Time.time;
    float intensidad = Mathf.Lerp(0.86f, 1.04f, Mathf.InverseLerp(1f, 4f, stacks));
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(16f, tamano.x * 0.32f);
    float alto = Mathf.Max(16f, tamano.y * 0.28f);
    float diametroBase = Mathf.Max(10f, tamano.x * 0.16f);

    float nubeA = OscilacionFuego(faseVenenoNubeA, t, 1.25f);
    float nubeB = OscilacionFuego(faseVenenoNubeB, t, 1.48f);
    float nubeC = OscilacionFuego(faseVenenoNubeC, t, 1.14f);
    float pulsoA = Mathf.Abs(OscilacionFuego(faseVenenoNubeA + 2.1f, t, 1.18f));
    float pulsoB = Mathf.Abs(OscilacionFuego(faseVenenoNubeB + 3.4f, t, 1.34f));
    float pulsoC = Mathf.Abs(OscilacionFuego(faseVenenoNubeC + 4.2f, t, 1.06f));
    float derivaA = OscilacionFuego(faseVenenoGotaA + 1.7f, t, 1.55f);
    float derivaB = OscilacionFuego(faseVenenoGotaB + 3.1f, t, 1.34f);
    float bamboleoA = OscilacionFuego(faseVenenoGotaA + 5.3f, t, 2.8f);
    float bamboleoB = OscilacionFuego(faseVenenoGotaB + 6.1f, t, 2.45f);
    EvaluarGoteoEstado(t, 0.62f + (stacks * 0.02f), faseVenenoGotaA, 0.36f, out float progresoGotaA, out float visGotaA);
    EvaluarGoteoEstado(t, 0.54f + (stacks * 0.018f), faseVenenoGotaB + 0.37f, 0.32f, out float progresoGotaB, out float visGotaB);

    overlayVenenoGroup.alpha = visibilidadVeneno * 0.86f;

    ConfigurarCapa(
      overlayVenenoNubeA,
      new Vector2(nubeA * 1.1f, alto * 0.26f + (pulsoA * 0.48f)),
      Vector2.one * (diametroBase * 1.46f * (0.98f + (pulsoA * 0.1f)) * intensidad),
      new Color(0.2f, 0.72f, 0.16f, Mathf.Lerp(0.22f, 0.32f, pulsoA)),
      nubeA * 2.5f);

    ConfigurarCapa(
      overlayVenenoNubeB,
      new Vector2((-ancho * 0.18f) + (nubeB * 0.92f), alto * 0.15f + (pulsoB * 0.42f)),
      Vector2.one * (diametroBase * 1.18f * (0.96f + (pulsoB * 0.1f)) * intensidad),
      new Color(0.24f, 0.68f, 0.16f, Mathf.Lerp(0.18f, 0.28f, pulsoB)),
      -4f + (nubeB * 3.4f));

    ConfigurarCapa(
      overlayVenenoNubeC,
      new Vector2((ancho * 0.2f) + (nubeC * 0.86f), alto * 0.12f + (pulsoC * 0.38f)),
      Vector2.one * (diametroBase * 1.02f * (0.95f + (pulsoC * 0.1f)) * intensidad),
      new Color(0.14f, 0.58f, 0.12f, Mathf.Lerp(0.14f, 0.22f, pulsoC)),
      5f + (nubeC * 3f));

    ConfigurarCapa(
      overlayVenenoGotaA,
      new Vector2((-ancho * 0.07f) + (derivaA * 0.72f), Mathf.Lerp(alto * 0.12f, -alto * 0.8f, progresoGotaA) + (bamboleoA * 0.32f)),
      Vector2.one * (diametroBase * Mathf.Lerp(0.12f, 0.18f, progresoGotaA) * intensidad),
      new Color(0.38f, 0.82f, 0.28f, Mathf.Lerp(0f, 0.24f, visGotaA)));

    ConfigurarCapa(
      overlayVenenoGotaB,
      new Vector2((ancho * 0.16f) + (derivaB * 0.64f), Mathf.Lerp(alto * 0.06f, -alto * 0.74f, progresoGotaB) + (bamboleoB * 0.28f)),
      Vector2.one * (diametroBase * Mathf.Lerp(0.1f, 0.16f, progresoGotaB) * intensidad),
      new Color(0.42f, 0.78f, 0.3f, Mathf.Lerp(0f, 0.2f, visGotaB)));
  }

  private void ActualizarVisualAcido(int stacks)
  {
    if (overlayAcidoGroup == null
      || overlayAcidoNubeA == null
      || overlayAcidoNubeB == null
      || overlayAcidoNubeC == null
      || overlayAcidoGotaA == null
      || overlayAcidoGotaB == null
      || overlayAcidoGotaC == null)
    {
      return;
    }

    float t = Time.time;
    float intensidad = Mathf.Lerp(0.92f, 1.12f, Mathf.InverseLerp(1f, 4f, stacks));
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(14f, tamano.x * 0.26f);
    float alto = Mathf.Max(14f, tamano.y * 0.22f);
    float diametroBase = Mathf.Max(8f, tamano.x * 0.12f);

    float nubeA = OscilacionFuego(faseAcidoNubeA, t, 1.42f);
    float nubeB = OscilacionFuego(faseAcidoNubeB, t, 1.56f);
    float nubeC = OscilacionFuego(faseAcidoNubeC, t, 1.28f);
    float pulsoA = Mathf.Abs(OscilacionFuego(faseAcidoNubeA + 1.8f, t, 1.36f));
    float pulsoB = Mathf.Abs(OscilacionFuego(faseAcidoNubeB + 3.1f, t, 1.42f));
    float pulsoC = Mathf.Abs(OscilacionFuego(faseAcidoNubeC + 4.4f, t, 1.22f));
    float derivaA = OscilacionFuego(faseAcidoGotaA + 1.2f, t, 1.74f);
    float derivaB = OscilacionFuego(faseAcidoGotaB + 2.7f, t, 1.66f);
    float derivaC = OscilacionFuego(faseAcidoGotaC + 4.2f, t, 1.82f);
    float bamboleoA = OscilacionFuego(faseAcidoGotaA + 5.4f, t, 3.05f);
    float bamboleoB = OscilacionFuego(faseAcidoGotaB + 6.3f, t, 2.88f);
    float bamboleoC = OscilacionFuego(faseAcidoGotaC + 2.9f, t, 3.18f);
    EvaluarGoteoEstado(t, 0.82f + (stacks * 0.026f), faseAcidoGotaA, 0.44f, out float progresoGotaA, out float visGotaA);
    EvaluarGoteoEstado(t, 0.71f + (stacks * 0.022f), faseAcidoGotaB + 0.31f, 0.4f, out float progresoGotaB, out float visGotaB);
    EvaluarGoteoEstado(t, 0.94f + (stacks * 0.03f), faseAcidoGotaC + 0.58f, 0.34f, out float progresoGotaC, out float visGotaC);

    overlayAcidoGroup.alpha = visibilidadAcido * 0.92f;

    ConfigurarCapa(
      overlayAcidoNubeA,
      new Vector2(nubeA * 0.96f, alto * 0.22f + (pulsoA * 0.34f)),
      Vector2.one * (diametroBase * 1.18f * (0.98f + (pulsoA * 0.08f)) * intensidad),
      new Color(0.76f, 0.86f, 0.12f, Mathf.Lerp(0.18f, 0.28f, pulsoA)),
      nubeA * 2.4f);

    ConfigurarCapa(
      overlayAcidoNubeB,
      new Vector2((-ancho * 0.16f) + (nubeB * 0.82f), alto * 0.12f + (pulsoB * 0.28f)),
      Vector2.one * (diametroBase * 0.98f * (0.96f + (pulsoB * 0.08f)) * intensidad),
      new Color(0.64f, 0.83f, 0.1f, Mathf.Lerp(0.14f, 0.22f, pulsoB)),
      -3f + (nubeB * 3f));

    ConfigurarCapa(
      overlayAcidoNubeC,
      new Vector2((ancho * 0.18f) + (nubeC * 0.76f), alto * 0.1f + (pulsoC * 0.26f)),
      Vector2.one * (diametroBase * 0.9f * (0.95f + (pulsoC * 0.08f)) * intensidad),
      new Color(0.86f, 0.91f, 0.19f, Mathf.Lerp(0.12f, 0.2f, pulsoC)),
      4f + (nubeC * 2.8f));

    ConfigurarCapa(
      overlayAcidoGotaA,
      new Vector2((-ancho * 0.09f) + (derivaA * 0.64f), Mathf.Lerp(alto * 0.08f, -alto * 0.94f, progresoGotaA) + (bamboleoA * 0.28f)),
      Vector2.one * (diametroBase * Mathf.Lerp(0.12f, 0.18f, progresoGotaA) * intensidad),
      new Color(0.92f, 0.96f, 0.24f, Mathf.Lerp(0f, 0.28f, visGotaA)));

    ConfigurarCapa(
      overlayAcidoGotaB,
      new Vector2((ancho * 0.1f) + (derivaB * 0.58f), Mathf.Lerp(alto * 0.04f, -alto * 0.88f, progresoGotaB) + (bamboleoB * 0.24f)),
      Vector2.one * (diametroBase * Mathf.Lerp(0.11f, 0.16f, progresoGotaB) * intensidad),
      new Color(0.85f, 0.93f, 0.19f, Mathf.Lerp(0f, 0.24f, visGotaB)));

    ConfigurarCapa(
      overlayAcidoGotaC,
      new Vector2((ancho * 0.02f) + (derivaC * 0.5f), Mathf.Lerp(alto * 0.14f, -alto * 0.82f, progresoGotaC) + (bamboleoC * 0.22f)),
      Vector2.one * (diametroBase * Mathf.Lerp(0.1f, 0.15f, progresoGotaC) * intensidad),
      new Color(0.97f, 0.98f, 0.31f, Mathf.Lerp(0f, 0.2f, visGotaC)));
  }

  private void ActualizarVisualSangrado(int stacks)
  {
    if (overlaySangradoGroup == null
      || overlaySangradoGotaA == null
      || overlaySangradoGotaB == null
      || overlaySangradoGotaC == null)
    {
      return;
    }

    float t = Time.time;
    float intensidad = Mathf.Lerp(1.08f, 1.34f, Mathf.InverseLerp(1f, 4f, stacks));
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(16f, tamano.x * 0.28f);
    float alto = Mathf.Max(18f, tamano.y * 0.3f);
    float diametroBase = Mathf.Max(10f, tamano.x * 0.14f);
    float derivaA = OscilacionFuego(faseSangradoGotaA + 1.6f, t, 0.96f);
    float derivaB = OscilacionFuego(faseSangradoGotaB + 3.2f, t, 0.84f);
    float derivaC = OscilacionFuego(faseSangradoGotaC + 5.1f, t, 1.03f);
    float bamboleoA = OscilacionFuego(faseSangradoGotaA + 4.8f, t, 1.58f);
    float bamboleoB = OscilacionFuego(faseSangradoGotaB + 6.1f, t, 1.47f);
    float bamboleoC = OscilacionFuego(faseSangradoGotaC + 2.7f, t, 1.69f);
    EvaluarGoteoEstado(t, 0.74f + (stacks * 0.032f), faseSangradoGotaA, 0.38f, out float progresoGotaA, out float visGotaA);
    EvaluarGoteoEstado(t, 0.67f + (stacks * 0.03f), faseSangradoGotaB + 0.26f, 0.34f, out float progresoGotaB, out float visGotaB);
    EvaluarGoteoEstado(t, 0.81f + (stacks * 0.035f), faseSangradoGotaC + 0.49f, 0.3f, out float progresoGotaC, out float visGotaC);

    overlaySangradoGroup.alpha = visibilidadSangrado * 0.76f;

    ConfigurarCapa(
      overlaySangradoGotaA,
      new Vector2((-ancho * 0.18f) + (derivaA * 0.66f), Mathf.Lerp(alto * 0.18f, -alto * 0.92f, progresoGotaA) + (bamboleoA * 0.24f)),
      new Vector2(diametroBase * Mathf.Lerp(0.315f, 0.465f, progresoGotaA), diametroBase * Mathf.Lerp(0.39f, 0.675f, progresoGotaA)) * intensidad,
      new Color(0.6f, 0.025f, 0.025f, Mathf.Lerp(0.03f, 0.44f, visGotaA)));

    ConfigurarCapa(
      overlaySangradoGotaB,
      new Vector2((ancho * 0.12f) + (derivaB * 0.58f), Mathf.Lerp(alto * 0.08f, -alto * 0.86f, progresoGotaB) + (bamboleoB * 0.22f)),
      new Vector2(diametroBase * Mathf.Lerp(0.27f, 0.405f, progresoGotaB), diametroBase * Mathf.Lerp(0.345f, 0.585f, progresoGotaB)) * intensidad,
      new Color(0.48f, 0.015f, 0.015f, Mathf.Lerp(0.02f, 0.36f, visGotaB)));

    ConfigurarCapa(
      overlaySangradoGotaC,
      new Vector2((-ancho * 0.01f) + (derivaC * 0.52f), Mathf.Lerp(alto * 0.22f, -alto * 0.78f, progresoGotaC) + (bamboleoC * 0.18f)),
      new Vector2(diametroBase * Mathf.Lerp(0.225f, 0.345f, progresoGotaC), diametroBase * Mathf.Lerp(0.285f, 0.51f, progresoGotaC)) * intensidad,
      new Color(0.66f, 0.04f, 0.03f, Mathf.Lerp(0.015f, 0.28f, visGotaC)));
  }

  private void ActualizarVisualEscudado(int stacks)
  {
    if (overlayEscudadoGroup == null
      || overlayEscudadoHalo == null
      || overlayEscudadoArcoA == null
      || overlayEscudadoArcoB == null
      || overlayEscudadoBrillo == null
      || overlayEscudadoPorcentaje == null)
    {
      return;
    }

    float t = Time.time;
    int porcentajeParada = Mathf.Clamp(stacks * 10, 10, 100);
    float carga = Mathf.InverseLerp(10f, 60f, porcentajeParada);
    float intensidad = Mathf.Lerp(0.82f, 0.96f, carga);
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(18f, tamano.x * 0.42f);
    float alto = Mathf.Max(22f, tamano.y * 0.58f);
    float pulsoHalo = Mathf.Abs(OscilacionFuego(faseEscudadoHalo, t, 0.18f));
    float pulsoArcoA = Mathf.Abs(OscilacionFuego(faseEscudadoArcoA + 1.9f, t, 0.16f));
    float pulsoArcoB = Mathf.Abs(OscilacionFuego(faseEscudadoArcoB + 3.6f, t, 0.14f));
    float derivaX = OscilacionFuego(faseEscudadoHalo + 2.8f, t, 0.12f);
    float derivaY = OscilacionFuego(faseEscudadoArcoA + 4.1f, t, 0.1f);
    float barrido = Mathf.Repeat((t * 0.11f) + (faseEscudadoBrillo * 0.08f), 1f);
    float brilloX = Mathf.Lerp(-ancho * 0.18f, ancho * 0.02f, barrido) + (derivaX * 0.4f);
    float brilloY = Mathf.Lerp(alto * 0.22f, -alto * 0.16f, barrido) + (derivaY * 0.32f);

    overlayEscudadoGroup.alpha = visibilidadEscudado * Mathf.Lerp(0.3f, 0.42f, carga);

    ConfigurarCapa(
      overlayEscudadoHalo,
      new Vector2((-ancho * 0.08f) + (derivaX * 0.5f), derivaY * 0.44f),
      new Vector2(ancho * 0.88f, alto * 0.94f) * (0.97f + (pulsoHalo * 0.025f)) * intensidad,
      new Color(0.66f, 0.75f, 0.88f, Mathf.Lerp(0.018f, 0.05f, pulsoHalo) * (0.82f + (carga * 0.08f))));

    ConfigurarCapa(
      overlayEscudadoArcoA,
      new Vector2((-ancho * 0.08f) + (derivaX * 0.26f), alto * 0.02f + (derivaY * 0.18f)),
      new Vector2(ancho * 0.63f, alto * 0.8f) * (0.98f + (pulsoArcoA * 0.02f)) * intensidad,
      new Color(0.8f, 0.86f, 0.95f, Mathf.Lerp(0.05f, 0.12f, pulsoArcoA) * (0.82f + (carga * 0.12f))),
      -18f);

    ConfigurarCapa(
      overlayEscudadoArcoB,
      new Vector2((-ancho * 0.03f) + (derivaX * 0.18f), -alto * 0.04f + (derivaY * 0.12f)),
      new Vector2(ancho * 0.47f, alto * 0.62f) * (0.98f + (pulsoArcoB * 0.018f)) * intensidad,
      new Color(0.72f, 0.79f, 0.91f, Mathf.Lerp(0.03f, 0.09f, pulsoArcoB) * (0.8f + (carga * 0.1f))),
      14f);

    ConfigurarCapa(
      overlayEscudadoBrillo,
      new Vector2(brilloX, brilloY),
      Vector2.one * (Mathf.Max(3.4f, tamano.x * 0.041f) * (0.88f + (pulsoHalo * 0.04f)) * intensidad),
      new Color(0.9f, 0.95f, 1f, Mathf.Lerp(0.01f, 0.04f, carga)),
      -10f);

    RectTransform contadorRect = overlayEscudadoPorcentaje.rectTransform;
    contadorRect.anchoredPosition = new Vector2(0f, alto * 0.1f);
    contadorRect.sizeDelta = new Vector2(Mathf.Max(12f, ancho * 0.28f), Mathf.Max(8f, alto * 0.11f));
    contadorRect.localEulerAngles = Vector3.zero;
    contadorRect.localScale = Vector3.one;
    overlayEscudadoPorcentaje.fontSize = Mathf.Clamp(Mathf.RoundToInt(ancho * 0.085f), 4, 6);
    overlayEscudadoPorcentaje.color = new Color(0.86f, 0.91f, 0.97f, visibilidadEscudado * Mathf.Lerp(0.26f, 0.42f, carga));
    overlayEscudadoPorcentaje.text = porcentajeParada.ToString() + "%";
  }

  private void ActualizarVisualBarrera(float valorBarrera)
  {
    if (overlayBarreraGroup == null
      || overlayBarreraHalo == null
      || overlayBarreraAro == null
      || overlayBarreraShimmer == null
      || overlayBarreraRunaA == null
      || overlayBarreraRunaB == null)
    {
      return;
    }

    float t = Time.time;
    float maxReferencia = unidad != null ? Mathf.Max(8f, unidad.mod_maxHP * 0.18f) : 12f;
    float carga = Mathf.Clamp01(valorBarrera / maxReferencia);
    float cargaVisual = Mathf.Pow(carga, 0.72f);
    float intensidad = Mathf.Lerp(0.92f, 1.2f, cargaVisual);
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(22f, tamano.x * 0.84f);
    float alto = Mathf.Max(26f, tamano.y * 0.97f);
    float pulsoHalo = Mathf.Abs(OscilacionFuego(faseBarreraHalo, t, 0.34f));
    float pulsoAro = Mathf.Abs(OscilacionFuego(faseBarreraAro + 1.9f, t, 0.26f));
    float derivaX = OscilacionFuego(faseBarreraHalo + 2.8f, t, 0.22f);
    float derivaY = OscilacionFuego(faseBarreraAro + 4.1f, t, 0.18f);
    float barrido = Mathf.Repeat((t * 0.16f) + (faseBarreraShimmer * 0.09f), 1f);
    float shimmerX = Mathf.Lerp(-ancho * 0.22f, ancho * 0.18f, barrido) + (derivaX * 0.78f);
    float shimmerY = Mathf.Lerp(alto * 0.16f, -alto * 0.08f, barrido) + (derivaY * 0.62f);
    float anguloRunaA = (t * 0.24f) + faseBarreraRunaA;
    float anguloRunaB = (t * 0.2f) + faseBarreraRunaB + 2.4f;
    float brilloRunaA = Mathf.Abs(Mathf.Sin((t * 0.42f) + faseBarreraRunaA));
    float brilloRunaB = Mathf.Abs(Mathf.Sin((t * 0.38f) + faseBarreraRunaB));

    overlayBarreraGroup.alpha = visibilidadBarrera * Mathf.Lerp(0.48f, 0.8f, cargaVisual);

    ConfigurarCapa(
      overlayBarreraHalo,
      new Vector2(derivaX * 0.88f, derivaY * 0.94f),
      new Vector2(ancho * 1.1f, alto * 1.18f) * (0.98f + (pulsoHalo * 0.04f) + (cargaVisual * 0.04f)) * intensidad,
      new Color(0.54f, 0.9f, 0.98f, Mathf.Lerp(0.02f, 0.072f, pulsoHalo) * (0.82f + (cargaVisual * 0.42f))));

    ConfigurarCapa(
      overlayBarreraAro,
      new Vector2(derivaX * 0.58f, derivaY * 0.54f),
      new Vector2(ancho * 0.94f, alto * 1.02f) * (0.985f + (pulsoAro * 0.038f) + (cargaVisual * 0.05f)) * intensidad,
      new Color(0.65f, 0.92f, 0.98f, Mathf.Lerp(0.1f, 0.24f, pulsoAro) * (0.94f + (cargaVisual * 0.36f)) * intensidad),
      0f);

    ConfigurarCapa(
      overlayBarreraShimmer,
      new Vector2(shimmerX, shimmerY),
      new Vector2(ancho * 0.12f, alto * 0.36f) * (0.94f + (pulsoHalo * 0.05f) + (cargaVisual * 0.08f)) * intensidad,
      new Color(0.88f, 0.97f, 1f, Mathf.Lerp(0.025f, 0.09f, cargaVisual)),
      -18f);

    ConfigurarCapa(
      overlayBarreraRunaA,
      new Vector2(Mathf.Cos(anguloRunaA) * ancho * 0.26f, Mathf.Sin(anguloRunaA) * alto * 0.22f),
      new Vector2(ancho * 0.18f, alto * 0.1f) * (0.95f + (brilloRunaA * 0.06f) + (cargaVisual * 0.06f)) * intensidad,
      new Color(0.66f, 0.91f, 0.98f, Mathf.Lerp(0.035f, 0.13f, brilloRunaA) * (0.86f + (cargaVisual * 0.42f))),
      12f + (Mathf.Sin(anguloRunaA) * 14f));

    ConfigurarCapa(
      overlayBarreraRunaB,
      new Vector2(Mathf.Cos(anguloRunaB) * ancho * 0.22f, Mathf.Sin(anguloRunaB) * alto * 0.18f),
      new Vector2(ancho * 0.15f, alto * 0.085f) * (0.93f + (brilloRunaB * 0.05f) + (cargaVisual * 0.05f)) * intensidad,
      new Color(0.58f, 0.86f, 0.96f, Mathf.Lerp(0.03f, 0.11f, brilloRunaB) * (0.84f + (cargaVisual * 0.4f))),
      -10f + (Mathf.Sin(anguloRunaB * 0.9f) * 12f));
  }

  private void ActualizarVisualCondenado(int turnosRestantes, int turnosAcumulados)
  {
    if (overlayCondenadoGroup == null
      || overlayCondenadoAura == null
      || overlayCondenadoSello == null
      || overlayCondenadoMarca == null
      || overlayCondenadoRunaA == null
      || overlayCondenadoRunaB == null
      || overlayCondenadoParticulaA == null
      || overlayCondenadoParticulaB == null
      || overlayCondenadoContador == null)
    {
      return;
    }

    float t = Time.time;
    float acumuladoVisual = Mathf.Max(1f, turnosAcumulados);
    float progresoCondena = Mathf.InverseLerp(1f, 6f, acumuladoVisual);
    float progresoCuentaRegresiva = 1f - Mathf.InverseLerp(1f, 5f, Mathf.Max(1, turnosRestantes));
    float intensidad = Mathf.Lerp(0.82f, 1.02f, progresoCondena);
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(22f, tamano.x * 0.74f);
    float alto = Mathf.Max(26f, tamano.y * 0.82f);
    float diametroBase = Mathf.Max(12f, tamano.x * 0.2f);
    float nubeA = OscilacionFuego(faseCondenadoAura, t, 0.944f);
    float nubeB = OscilacionFuego(faseCondenadoSello + 0.6f, t, 1.088f);
    float pulsoA = Mathf.Abs(OscilacionFuego(faseCondenadoAura + 2.1f, t, 0.88f));
    float pulsoB = Mathf.Abs(OscilacionFuego(faseCondenadoSello + 3.2f, t, 1.008f));
    float chispa = Mathf.Abs(OscilacionFuego(faseCondenadoMarca + 5.1f, t, 1.184f));
    float derivaChispaX = OscilacionFuego(faseCondenadoMarca + 1.7f, t, 1.296f);
    float derivaChispaY = OscilacionFuego(faseCondenadoMarca + 2.9f, t, 1.088f);
    float velocidadOrbita = Mathf.Lerp(0.448f, 0.72f, progresoCuentaRegresiva);
    float anguloRunaA = (t * velocidadOrbita) + faseCondenadoRunaA;
    float anguloRunaB = (t * (velocidadOrbita * 0.9f)) + faseCondenadoRunaB + 2.15f;
    float orbitaX = ancho * 0.4f;
    float orbitaY = alto * 0.34f;
    float tamParticula = Mathf.Max(5.8f, tamano.x * 0.09f);
    float brilloRunaA = Mathf.Abs(Mathf.Sin((t * 0.416f) + faseCondenadoRunaA));
    float brilloRunaB = Mathf.Abs(Mathf.Sin((t * 0.384f) + faseCondenadoRunaB));
    float cicloParticulaA = Mathf.Repeat((t * Mathf.Lerp(0.18f, 0.26f, progresoCuentaRegresiva)) + (faseCondenadoParticulaA * 0.12f), 1f);
    float cicloParticulaB = Mathf.Repeat((t * Mathf.Lerp(0.15f, 0.22f, progresoCuentaRegresiva)) + (faseCondenadoParticulaB * 0.14f), 1f);
    float visParticulaA = Mathf.Sin(cicloParticulaA * Mathf.PI);
    float visParticulaB = Mathf.Sin(cicloParticulaB * Mathf.PI);
    float derivaParticulaAX = OscilacionFuego(faseCondenadoParticulaA + 1.4f, t, 0.54f);
    float derivaParticulaAY = OscilacionFuego(faseCondenadoParticulaA + 3.1f, t, 0.46f);
    float derivaParticulaBX = OscilacionFuego(faseCondenadoParticulaB + 2.2f, t, 0.5f);
    float derivaParticulaBY = OscilacionFuego(faseCondenadoParticulaB + 4.4f, t, 0.42f);

    overlayCondenadoGroup.alpha = visibilidadCondenado * Mathf.Lerp(0.42f, 0.58f, progresoCondena);

    ConfigurarCapa(
      overlayCondenadoAura,
      new Vector2(nubeA * 1.2f, alto * 0.08f + (pulsoA * 0.42f)),
      new Vector2(ancho * 0.94f, alto * 0.9f) * (0.96f + (pulsoA * 0.05f) + (progresoCondena * 0.03f)) * intensidad,
      new Color(0.18f, 0.04f, 0.3f, Mathf.Lerp(0.07f, 0.14f, pulsoA) * (0.84f + (progresoCondena * 0.12f))),
      nubeA * 2.2f);

    ConfigurarCapa(
      overlayCondenadoSello,
      new Vector2(0f, alto * 0.02f),
      new Vector2(ancho * 0.8f, alto * 0.82f) * (0.97f + (pulsoB * 0.03f) + (progresoCondena * 0.03f)) * intensidad,
      new Color(0.36f, 0.12f, 0.5f, Mathf.Lerp(0.1f, 0.2f, pulsoB) * (0.84f + (progresoCondena * 0.12f)) * intensidad),
      0f);

    ConfigurarCapa(
      overlayCondenadoMarca,
      new Vector2(derivaChispaX * 0.92f, (alto * 0.18f) + (derivaChispaY * 0.58f)),
      Vector2.one * (diametroBase * 0.5f * (0.92f + (chispa * 0.12f)) * intensidad),
      new Color(0.56f, 0.42f, 0.68f, Mathf.Lerp(0.08f, 0.16f, chispa) * (0.82f + (progresoCondena * 0.1f))),
      -4f + (derivaChispaX * 5f));

    ConfigurarCapa(
      overlayCondenadoRunaA,
      new Vector2(Mathf.Cos(anguloRunaA) * orbitaX, Mathf.Sin(anguloRunaA) * orbitaY),
      Vector2.one * (tamParticula * 0.82f * (0.92f + (brilloRunaA * 0.08f) + (progresoCondena * 0.04f)) * intensidad),
      new Color(0.48f, 0.2f, 0.62f, Mathf.Lerp(0.1f, 0.22f, brilloRunaA) * (0.82f + (progresoCondena * 0.12f))));

    ConfigurarCapa(
      overlayCondenadoRunaB,
      new Vector2(Mathf.Cos(anguloRunaB) * (orbitaX * 0.84f), Mathf.Sin(anguloRunaB) * (orbitaY * 0.88f)),
      Vector2.one * (tamParticula * 0.74f * (0.9f + (brilloRunaB * 0.08f) + (progresoCondena * 0.04f)) * intensidad),
      new Color(0.42f, 0.16f, 0.58f, Mathf.Lerp(0.08f, 0.18f, brilloRunaB) * (0.8f + (progresoCondena * 0.1f))));

    ConfigurarCapa(
      overlayCondenadoParticulaA,
      new Vector2(
        (-ancho * 0.16f) + (derivaParticulaAX * 1.3f),
        Mathf.Lerp(-alto * 0.06f, alto * 0.32f, cicloParticulaA) + (derivaParticulaAY * 0.72f)),
      Vector2.one * (tamParticula * 0.78f * Mathf.Lerp(0.52f, 0.78f, visParticulaA) * intensidad),
      new Color(0.54f, 0.22f, 0.68f, Mathf.Lerp(0f, 0.12f, visParticulaA) * (0.84f + (progresoCondena * 0.1f))));

    ConfigurarCapa(
      overlayCondenadoParticulaB,
      new Vector2(
        (ancho * 0.18f) + (derivaParticulaBX * 1.1f),
        Mathf.Lerp(alto * 0.02f, alto * 0.38f, cicloParticulaB) + (derivaParticulaBY * 0.64f)),
      Vector2.one * (tamParticula * 0.72f * Mathf.Lerp(0.44f, 0.72f, visParticulaB) * intensidad),
      new Color(0.46f, 0.18f, 0.62f, Mathf.Lerp(0f, 0.1f, visParticulaB) * (0.82f + (progresoCondena * 0.08f))));

    RectTransform contadorRect = overlayCondenadoContador.rectTransform;
    contadorRect.anchoredPosition = new Vector2(ancho * 0.44f, alto * 0.31f);
    contadorRect.sizeDelta = new Vector2(Mathf.Max(18f, ancho * 0.18f), Mathf.Max(12f, alto * 0.16f));
    contadorRect.localEulerAngles = Vector3.zero;
    contadorRect.localScale = new Vector3(-1f, 1f, 1f);
    overlayCondenadoContador.fontSize = Mathf.Clamp(Mathf.RoundToInt(ancho * 0.11f), 7, 12);
    overlayCondenadoContador.color = new Color(1f, 0.96f, 1f, visibilidadCondenado);
    overlayCondenadoContador.text = turnosAcumulados.ToString();
    overlayCondenadoContador.enabled = turnosRestantes > 0;
  }

  private void ActualizarVisualAturdido(int stacks)
  {
    if (overlayAturdidoGroup == null
      || overlayAturdidoGlow == null
      || overlayAturdidoAro == null
      || overlayAturdidoOrbitaA == null
      || overlayAturdidoOrbitaB == null
      || overlayAturdidoOrbitaC == null
      || imagenUnidad == null)
    {
      return;
    }

    float t = Time.time;
    float intensidad = Mathf.Lerp(1f, 1.14f, Mathf.InverseLerp(1f, 4f, stacks));
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(19f, tamano.x * 0.64f);
    float alto = Mathf.Max(7f, tamano.y * 0.16f);
    float pulso = Mathf.Abs(OscilacionFuego(faseAturdidoBalanceo, t, 0.48f));
    float bamboleoX = OscilacionFuego(faseAturdidoBalanceo + 2.7f, t, 0.38f);
    float bamboleoY = OscilacionFuego(faseAturdidoBalanceo + 4.9f, t, 0.44f);
    Vector2 centro = new Vector2(bamboleoX * 0.65f, bamboleoY * 0.85f);
    float orbitaX = ancho * 0.24f;
    float orbitaY = alto * 0.55f;
    float anguloA = (t * 0.68f) + faseAturdidoOrbitaA;
    float anguloB = (t * 0.58f) + faseAturdidoOrbitaB + 2.1f;
    float anguloC = (t * 0.74f) + faseAturdidoOrbitaC + 4.2f;
    float tamOrbitaBase = Mathf.Max(3.8f, tamano.x * 0.052f);

    overlayAturdidoGroup.alpha = visibilidadAturdido * 0.66f;

    ConfigurarCapa(
      overlayAturdidoGlow,
      centro + new Vector2(0f, 0.6f),
      new Vector2(ancho * 1.22f, alto * 1.62f) * (0.97f + (pulso * 0.06f)) * intensidad,
      new Color(1f, 0.94f, 0.58f, Mathf.Lerp(0.06f, 0.11f, pulso)));

    ConfigurarCapa(
      overlayAturdidoAro,
      centro,
      new Vector2(ancho, alto) * (0.98f + (pulso * 0.05f)) * intensidad,
      new Color(1f, 0.92f, 0.48f, Mathf.Lerp(0.32f, 0.45f, pulso) * intensidad),
      0f);

    ConfigurarCapa(
      overlayAturdidoOrbitaA,
      centro + new Vector2(Mathf.Cos(anguloA) * orbitaX, Mathf.Sin(anguloA) * orbitaY),
      Vector2.one * (tamOrbitaBase * (0.92f + (Mathf.Abs(Mathf.Sin(anguloA * 0.5f)) * 0.16f)) * intensidad),
      new Color(1f, 0.96f, 0.74f, 0.5f));

    ConfigurarCapa(
      overlayAturdidoOrbitaB,
      centro + new Vector2(Mathf.Cos(anguloB) * (orbitaX * 0.88f), Mathf.Sin(anguloB) * (orbitaY * 0.92f)),
      Vector2.one * (tamOrbitaBase * 0.84f * (0.9f + (Mathf.Abs(Mathf.Sin(anguloB * 0.56f)) * 0.14f)) * intensidad),
      new Color(1f, 0.93f, 0.66f, 0.42f));

    ConfigurarCapa(
      overlayAturdidoOrbitaC,
      centro + new Vector2(Mathf.Cos(anguloC) * (orbitaX * 0.8f), Mathf.Sin(anguloC) * (orbitaY * 0.82f)),
      Vector2.one * (tamOrbitaBase * 0.72f * (0.88f + (Mathf.Abs(Mathf.Sin(anguloC * 0.48f)) * 0.12f)) * intensidad),
      new Color(1f, 0.9f, 0.55f, 0.35f));
  }

  private void ActualizarVisualCongelado(int stacks)
  {
    if (overlayCongeladoGroup == null
      || overlayCongeladoTint == null
      || overlayCongeladoGotaA == null
      || overlayCongeladoGotaB == null
      || imagenUnidad == null)
    {
      return;
    }

    float t = Time.time;
    float intensidad = Mathf.Lerp(1f, 1.12f, Mathf.InverseLerp(1f, 4f, stacks));
    Vector2 tamano = ObtenerTamanoUnidad();
    float ancho = Mathf.Max(12f, tamano.x * 0.24f);
    float alto = Mathf.Max(16f, tamano.y * 0.3f);
    float brillo = Mathf.Abs(OscilacionFuego(faseCongeladoBrillo, t, 0.78f));
    float derivaA = OscilacionFuego(faseCongeladoGotaA + 2.3f, t, 0.94f);
    float derivaB = OscilacionFuego(faseCongeladoGotaB + 4.7f, t, 0.86f);
    float bamboleoA = OscilacionFuego(faseCongeladoGotaA + 5.8f, t, 1.44f);
    float bamboleoB = OscilacionFuego(faseCongeladoGotaB + 1.9f, t, 1.28f);
    float desplazamientoIzquierda = -ancho * 0.15f;
    EvaluarGoteoEstado(t, 0.34f + (stacks * 0.012f), faseCongeladoGotaA, 0.24f, out float progresoGotaA, out float visGotaA);
    EvaluarGoteoEstado(t, 0.27f + (stacks * 0.01f), faseCongeladoGotaB + 0.41f, 0.2f, out float progresoGotaB, out float visGotaB);

    overlayCongeladoGroup.alpha = visibilidadCongelado;
    overlayCongeladoTint.color = new Color(0.76f, 0.92f, 1f, Mathf.Lerp(0.58f, 0.74f, brillo) * intensidad);

    ConfigurarCapa(
      overlayCongeladoGotaA,
      new Vector2(desplazamientoIzquierda + (-ancho * 0.16f) + (derivaA * 0.42f), Mathf.Lerp(-alto * 0.22f, -alto * 0.72f, progresoGotaA) + (bamboleoA * 0.12f)),
      Vector2.one * (Mathf.Max(3f, tamano.x * 0.03f) * 0.85f * Mathf.Lerp(0.82f, 1f, progresoGotaA) * intensidad),
      new Color(0.9f, 0.97f, 1f, Mathf.Lerp(0f, 0.28f, visGotaA)));

    ConfigurarCapa(
      overlayCongeladoGotaB,
      new Vector2(desplazamientoIzquierda + (ancho * 0.12f) + (derivaB * 0.34f), Mathf.Lerp(-alto * 0.16f, -alto * 0.66f, progresoGotaB) + (bamboleoB * 0.1f)),
      Vector2.one * (Mathf.Max(2.5f, tamano.x * 0.024f) * 0.85f * Mathf.Lerp(0.8f, 0.96f, progresoGotaB) * intensidad),
      new Color(0.84f, 0.94f, 1f, Mathf.Lerp(0f, 0.22f, visGotaB)));
  }

  private bool TryObtenerPosicionBaseEstado(out Vector2 posicionLocal)
  {
    posicionLocal = imagenUnidad != null ? imagenUnidad.anchoredPosition : Vector2.zero;

    if (overlayArdiendoRoot == null)
    {
      return imagenUnidad != null;
    }

    RectTransform parent = overlayArdiendoRoot.parent as RectTransform;
    if (parent == null)
    {
      return imagenUnidad != null;
    }

    Transform puntoBase = unidad != null ? unidad.puntoEntrante : null;
    if (puntoBase == null)
    {
      return imagenUnidad != null;
    }

    Canvas canvas = parent.GetComponentInParent<Canvas>();
    Camera eventCamera = ObtenerCanvasCamera(canvas);
    Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, puntoBase.position);
    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, eventCamera, out Vector2 localPoint))
    {
      posicionLocal = localPoint;
      return true;
    }

    return imagenUnidad != null;
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

  private static float OscilacionFuego(float semilla, float tiempo, float velocidad)
  {
    float ruidoBase = (Mathf.PerlinNoise((semilla * 0.37f) + 1.13f, tiempo * velocidad) - 0.5f) * 2f;
    float ruidoRapido = (Mathf.PerlinNoise((semilla * 0.19f) + 7.41f, tiempo * (velocidad * 1.65f)) - 0.5f) * 2f;
    float aleteo = Mathf.Sin((tiempo * (velocidad * 1.42f)) + (semilla * 5.17f));
    return Mathf.Clamp((ruidoBase * 0.55f) + (ruidoRapido * 0.25f) + (aleteo * 0.2f), -1f, 1f);
  }

  private static void EvaluarGoteoEstado(float tiempo, float velocidad, float fase, float duracionVisible, out float progreso, out float visibilidad)
  {
    float ciclo = Mathf.Repeat((tiempo * velocidad) + (fase * 0.137f), 1f);
    if (ciclo >= duracionVisible)
    {
      progreso = 1f;
      visibilidad = 0f;
      return;
    }

    progreso = Mathf.Clamp01(ciclo / Mathf.Max(0.08f, duracionVisible));
    visibilidad = Mathf.Sin(progreso * Mathf.PI);
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

  private void DestruirOverlayArdiendo()
  {
    if (overlayArdiendoRoot != null)
    {
      Destroy(overlayArdiendoRoot.gameObject);
    }

    overlayArdiendoRoot = null;
    overlayArdiendoGroup = null;
    overlayArdiendoGlow = null;
    overlayArdiendoLlamaA = null;
    overlayArdiendoLlamaB = null;
    overlayArdiendoLlamaC = null;
    overlayArdiendoLlamaD = null;
    visibilidadArdiendo = 0f;
  }

  private void DestruirOverlayVeneno()
  {
    if (overlayVenenoRoot != null)
    {
      Destroy(overlayVenenoRoot.gameObject);
    }

    overlayVenenoRoot = null;
    overlayVenenoGroup = null;
    overlayVenenoNubeA = null;
    overlayVenenoNubeB = null;
    overlayVenenoNubeC = null;
    overlayVenenoGotaA = null;
    overlayVenenoGotaB = null;
    visibilidadVeneno = 0f;
  }

  private void DestruirOverlayAcido()
  {
    if (overlayAcidoRoot != null)
    {
      Destroy(overlayAcidoRoot.gameObject);
    }

    overlayAcidoRoot = null;
    overlayAcidoGroup = null;
    overlayAcidoNubeA = null;
    overlayAcidoNubeB = null;
    overlayAcidoNubeC = null;
    overlayAcidoGotaA = null;
    overlayAcidoGotaB = null;
    overlayAcidoGotaC = null;
    visibilidadAcido = 0f;
  }

  private void DestruirOverlaySangrado()
  {
    if (overlaySangradoRoot != null)
    {
      Destroy(overlaySangradoRoot.gameObject);
    }

    overlaySangradoRoot = null;
    overlaySangradoGroup = null;
    overlaySangradoGotaA = null;
    overlaySangradoGotaB = null;
    overlaySangradoGotaC = null;
    visibilidadSangrado = 0f;
  }

  private void DestruirOverlayEscudado()
  {
    if (overlayEscudadoRoot != null)
    {
      Destroy(overlayEscudadoRoot.gameObject);
    }

    overlayEscudadoRoot = null;
    overlayEscudadoGroup = null;
    overlayEscudadoHalo = null;
    overlayEscudadoArcoA = null;
    overlayEscudadoArcoB = null;
    overlayEscudadoBrillo = null;
    overlayEscudadoPorcentaje = null;
    visibilidadEscudado = 0f;
  }

  private void DestruirOverlayBarrera()
  {
    if (overlayBarreraRoot != null)
    {
      Destroy(overlayBarreraRoot.gameObject);
    }

    overlayBarreraRoot = null;
    overlayBarreraGroup = null;
    overlayBarreraHalo = null;
    overlayBarreraAro = null;
    overlayBarreraShimmer = null;
    overlayBarreraRunaA = null;
    overlayBarreraRunaB = null;
    visibilidadBarrera = 0f;
  }

  private void DestruirOverlayCondenado()
  {
    if (overlayCondenadoRoot != null)
    {
      Destroy(overlayCondenadoRoot.gameObject);
    }

    overlayCondenadoRoot = null;
    overlayCondenadoGroup = null;
    overlayCondenadoAura = null;
    overlayCondenadoSello = null;
    overlayCondenadoMarca = null;
    overlayCondenadoRunaA = null;
    overlayCondenadoRunaB = null;
    overlayCondenadoParticulaA = null;
    overlayCondenadoParticulaB = null;
    overlayCondenadoContador = null;
    visibilidadCondenado = 0f;
  }

  private void DestruirOverlayAturdido()
  {
    if (overlayAturdidoRoot != null)
    {
      Destroy(overlayAturdidoRoot.gameObject);
    }

    overlayAturdidoRoot = null;
    overlayAturdidoGroup = null;
    overlayAturdidoGlow = null;
    overlayAturdidoAro = null;
    overlayAturdidoOrbitaA = null;
    overlayAturdidoOrbitaB = null;
    overlayAturdidoOrbitaC = null;
    visibilidadAturdido = 0f;
  }

  private void DestruirOverlayCongelado()
  {
    if (overlayCongeladoRoot != null)
    {
      Destroy(overlayCongeladoRoot.gameObject);
    }

    overlayCongeladoRoot = null;
    overlayCongeladoGroup = null;
    overlayCongeladoTint = null;
    overlayCongeladoGotaA = null;
    overlayCongeladoGotaB = null;
    visibilidadCongelado = 0f;
  }
}
