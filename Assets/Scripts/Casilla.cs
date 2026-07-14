using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;


public class Casilla : MonoBehaviour
{
  public int lado; //1 A Enemigo  -   2 B PC
  public int posX;
  public int posY;

  public int costoMovimiento = 1;
  public GameObject ladoOpuesto;
  public GameObject ladoGO;

  public GameObject Presente; // El GameObject presente en la casilla

  public bool TEST = false;
  public int TESTINT;
  public int TESTINT2;

  public GameObject TESTGO;

  public GameObject MarcaMovX0Y1;
  public GameObject MarcaMovX1Y1;
  public GameObject MarcaMovX1Y0;
  public GameObject MarcaMovXv1Y1;
  public GameObject MarcaMovXv1Y0;
  public GameObject MarcaMovX1Yv1;
  public GameObject MarcaMovX0Yv1;
  public GameObject MarcaMovXv1Yv1;
  public GameObject MarcaMeleeAtraviesa;
  [SerializeField] private float duracionFadeMarcaMovimiento = 0.1f;
  [SerializeField] private float intensidadPulsoObjetivoHabilidad = 0.065f;
  [SerializeField] private float velocidadPulsoObjetivoHabilidad = 5.8f;
  private readonly Dictionary<Transform, Vector3> escalasBaseGlowMovimiento = new Dictionary<Transform, Vector3>();
  private readonly Dictionary<GameObject, EstadoMarcaMovimiento> estadosMarcaMovimiento = new Dictionary<GameObject, EstadoMarcaMovimiento>();
  private MaterialPropertyBlock bloqueGlowMovimiento;
  private MaterialPropertyBlock bloqueTurnoActualEnemigo;
  private bool hoverMovimientoValido;
  private Vector3? escalaBaseCapaObjetivoHabilidad;
  private Vector3? escalaBaseBordeActualObjetivoHabilidad;
  private bool autoActivoCapaObjetivoHabilidad;
  private Transform graficoBordeHabilidadAzul;
  private Transform graficoBordeHabilidadRojo;
  private Transform graficoBordeActual;
  private Transform circuloBordeHabilidadAzul;
  private Transform circuloBordeHabilidadRojo;
  private Renderer[] renderersBordeActual = Array.Empty<Renderer>();
  private Transform contenedorCostoMovimiento;
  private readonly List<SpriteRenderer> iconosCostoMovimiento = new List<SpriteRenderer>();
  private Transform transformVistaTactica;
  private SpriteRenderer spriteVistaTactica;
  private SpriteRenderer rellenoDanioVistaTactica;
  private Sprite spriteBaseVistaTactica;
  private Vector3 escalaBaseVistaTactica = Vector3.one;
  private Vector3 escalaActualVistaTactica = Vector3.one;
  private Vector2 tamanoSpriteBaseVistaTactica = Vector2.one;
  private Sprite spriteCostoMovimiento;
  private const int OrdenVistaTacticaOffset = 6;
  private const float AlphaRellenoDanioVistaTactica = 0.72f;
  private const float IntensidadPulsoVistaTactica = 0.055f;
  private const float VelocidadPulsoVistaTactica = 7.5f;
  private static readonly int ShaderColorId = Shader.PropertyToID("_Color");
  private static readonly int ShaderBaseColorId = Shader.PropertyToID("_BaseColor");
  private static readonly int ShaderEmissionColorId = Shader.PropertyToID("_EmissionColor");
  private bool estaEnDestruccion;
  private bool hoverVistaTactica;
  private Unidad unidadHoverVistaTactica;

  private sealed class EstadoMarcaMovimiento
  {
    public GameObject objeto;
    public Renderer[] renderers;
    public Material[] materialesInstancia;
    public Color[] coloresBase;
    public Color[] emisionesBase;
    public float alphaActual;
    public float alphaObjetivo;
  }
  
  void Awake()
  {
    bloqueGlowMovimiento = new MaterialPropertyBlock();
    bloqueTurnoActualEnemigo = new MaterialPropertyBlock();
    CachearReferenciasVisuales();
    InicializarPreviewCostoMovimiento();
    InicializarEstadosMarcaMovimiento();
  }

  private void CachearReferenciasVisuales()
  {
    graficoBordeHabilidadAzul = BuscarTransformRecursivo(transform, "GraficoBorde Habilidad amiga");
    graficoBordeHabilidadRojo = BuscarTransformRecursivo(transform, "GraficoBorde Habilidad");
    graficoBordeActual = BuscarTransformRecursivo(transform, "GraficoBorde Actual");
    circuloBordeHabilidadAzul = BuscarTransformRecursivo(graficoBordeHabilidadAzul, "Circulo");
    circuloBordeHabilidadRojo = BuscarTransformRecursivo(graficoBordeHabilidadRojo, "Circulo");
    renderersBordeActual = graficoBordeActual != null ? graficoBordeActual.GetComponentsInChildren<Renderer>(true) : Array.Empty<Renderer>();

    Transform vistaTactica = BuscarTransformRecursivo(transform, "VistaTactica");
    transformVistaTactica = vistaTactica;
    spriteVistaTactica = vistaTactica != null ? vistaTactica.GetComponent<SpriteRenderer>() : null;
    if (spriteVistaTactica != null)
    {
      spriteBaseVistaTactica = spriteVistaTactica.sprite;
      escalaBaseVistaTactica = spriteVistaTactica.transform.localScale;
      escalaActualVistaTactica = escalaBaseVistaTactica;
      tamanoSpriteBaseVistaTactica = ObtenerTamanoSpriteVistaTactica(spriteVistaTactica.sprite);
      CrearRellenoDanioVistaTactica();
    }
    ActualizarVistaTactica(false);
  }

  private static Transform BuscarTransformRecursivo(Transform raiz, string nombre)
  {
    if (raiz == null)
    {
      return null;
    }

    foreach (Transform hijo in raiz.GetComponentsInChildren<Transform>(true))
    {
      if (hijo != null && hijo.name == nombre)
      {
        return hijo;
      }
    }

    return null;
  }

  public void ActualizarVistaTactica(bool activa)
  {
    if (spriteVistaTactica == null)
    {
      return;
    }

    Unidad unidadPresente = Presente != null ? Presente.GetComponent<Unidad>() : null;
    bool mostrarRetrato = activa && unidadPresente != null && unidadPresente.uRetrato != null;
    Sprite retrato = mostrarRetrato ? unidadPresente.uRetrato : null;

    spriteVistaTactica.sprite = retrato;
    AplicarEscalaVistaTactica(retrato);
    if (!mostrarRetrato)
    {
      LimpiarHoverVistaTactica();
    }
    ActualizarPulsoVistaTactica(mostrarRetrato);
    spriteVistaTactica.sortingOrder = RenderOrderHelper.CalcularOrdenPorY(posY) + OrdenVistaTacticaOffset;
    ActualizarRellenoDanioVistaTactica(unidadPresente, mostrarRetrato);
    SetActiveIfChanged(spriteVistaTactica.gameObject, mostrarRetrato);
  }

  private void CrearRellenoDanioVistaTactica()
  {
    if (transformVistaTactica == null || spriteBaseVistaTactica == null || rellenoDanioVistaTactica != null)
    {
      return;
    }

    GameObject rellenoGO = new GameObject("VistaTacticaDanio");
    rellenoGO.transform.SetParent(transformVistaTactica, false);
    rellenoGO.transform.localPosition = Vector3.zero;
    rellenoGO.transform.localRotation = Quaternion.identity;
    rellenoGO.transform.localScale = Vector3.one;

    rellenoDanioVistaTactica = rellenoGO.AddComponent<SpriteRenderer>();
    rellenoDanioVistaTactica.sprite = spriteBaseVistaTactica;
    rellenoDanioVistaTactica.color = new Color(0.28f, 0f, 0f, AlphaRellenoDanioVistaTactica);
    rellenoDanioVistaTactica.sortingOrder = RenderOrderHelper.CalcularOrdenPorY(posY) + OrdenVistaTacticaOffset + 1;
    rellenoDanioVistaTactica.gameObject.SetActive(false);
  }

  private void ActualizarRellenoDanioVistaTactica(Unidad unidadPresente, bool mostrarRetrato)
  {
    if (rellenoDanioVistaTactica == null)
    {
      return;
    }

    float proporcionDanio = 0f;
    if (mostrarRetrato && unidadPresente != null && unidadPresente.mod_maxHP > 0f)
    {
      proporcionDanio = 1f - Mathf.Clamp01(unidadPresente.HP_actual / unidadPresente.mod_maxHP);
    }

    bool mostrarRelleno = mostrarRetrato && proporcionDanio > 0.001f;
    if (mostrarRelleno)
    {
      Sprite spriteActual = spriteVistaTactica != null ? spriteVistaTactica.sprite : null;
      Vector2 tamanoSpriteActual = ObtenerTamanoSpriteVistaTactica(spriteActual);

      rellenoDanioVistaTactica.sprite = spriteBaseVistaTactica;
      rellenoDanioVistaTactica.sortingOrder = RenderOrderHelper.CalcularOrdenPorY(posY) + OrdenVistaTacticaOffset + 1;
      rellenoDanioVistaTactica.transform.localScale = new Vector3(
        tamanoSpriteActual.x / tamanoSpriteBaseVistaTactica.x,
        (tamanoSpriteActual.y / tamanoSpriteBaseVistaTactica.y) * proporcionDanio,
        1f);
      rellenoDanioVistaTactica.transform.localPosition = new Vector3(
        0f,
        -(tamanoSpriteActual.y * (1f - proporcionDanio)) * 0.5f,
        0f);
    }

    SetActiveIfChanged(rellenoDanioVistaTactica.gameObject, mostrarRelleno);
  }

  private void AplicarEscalaVistaTactica(Sprite spriteActual)
  {
    if (transformVistaTactica == null)
    {
      return;
    }

    if (spriteActual == null)
    {
      escalaActualVistaTactica = escalaBaseVistaTactica;
      transformVistaTactica.localScale = escalaActualVistaTactica;
      return;
    }

    Vector2 tamanoSpriteActual = ObtenerTamanoSpriteVistaTactica(spriteActual);
    escalaActualVistaTactica = new Vector3(
      escalaBaseVistaTactica.x * tamanoSpriteBaseVistaTactica.x / tamanoSpriteActual.x,
      escalaBaseVistaTactica.y * tamanoSpriteBaseVistaTactica.y / tamanoSpriteActual.y,
      escalaBaseVistaTactica.z);
    transformVistaTactica.localScale = escalaActualVistaTactica;
  }

  private void ActualizarPulsoVistaTactica(bool mostrarRetrato)
  {
    if (transformVistaTactica == null)
    {
      return;
    }

    if (!mostrarRetrato || !hoverVistaTactica)
    {
      transformVistaTactica.localScale = escalaActualVistaTactica;
      return;
    }

    float pulso = 1f + (Mathf.Sin(Time.time * VelocidadPulsoVistaTactica) * 0.5f + 0.5f) * IntensidadPulsoVistaTactica;
    transformVistaTactica.localScale = escalaActualVistaTactica * pulso;
  }

  private void SetHoverVistaTactica(bool hover)
  {
    Unidad unidadPresente = Presente != null ? Presente.GetComponent<Unidad>() : null;
    bool mostrarPulso = hover
      && BattleManager.Instance != null
      && BattleManager.Instance.VistaTacticaActiva
      && spriteVistaTactica != null
      && spriteVistaTactica.gameObject.activeSelf
      && MouseSobreImagenVistaTactica()
      && unidadPresente != null;

    hoverVistaTactica = mostrarPulso;
    ActualizarPulsoVistaTactica(mostrarPulso);
    ActualizarInfoHoverVistaTactica(mostrarPulso ? unidadPresente : null);
  }

  private void ActualizarInfoHoverVistaTactica(Unidad unidad)
  {
    UIInfoChar infoChar = BattleManager.Instance != null ? BattleManager.Instance.scUIInfoChar : null;
    if (infoChar == null)
    {
      unidadHoverVistaTactica = unidad;
      return;
    }

    if (unidadHoverVistaTactica == unidad)
    {
      if (unidad != null)
      {
        infoChar.ReaplicarMarcadoPrioritario();
      }
      return;
    }

    if (unidadHoverVistaTactica != null)
    {
      infoChar.LimpiarHover(unidadHoverVistaTactica);
    }

    unidadHoverVistaTactica = unidad;

    if (unidadHoverVistaTactica != null)
    {
      infoChar.MostrarHover(unidadHoverVistaTactica);
    }
  }

  private void LimpiarHoverVistaTactica()
  {
    hoverVistaTactica = false;
    ActualizarInfoHoverVistaTactica(null);
  }

  private bool DebeIgnorarInputUnidadVistaTactica()
  {
    return BattleManager.Instance != null
      && BattleManager.Instance.VistaTacticaActiva
      && Presente != null
      && Presente.GetComponent<Unidad>() != null
      && !MouseSobreImagenVistaTactica();
  }

  private bool MouseSobreImagenVistaTactica()
  {
    if (spriteVistaTactica == null || !spriteVistaTactica.gameObject.activeInHierarchy || spriteVistaTactica.sprite == null)
    {
      return false;
    }

    Camera camara = Camera.main;
    if (camara == null)
    {
      return false;
    }

    Bounds bounds = spriteVistaTactica.bounds;
    Vector3 centro = bounds.center;
    Vector3 ext = bounds.extents;
    Vector3[] esquinas =
    {
      centro + new Vector3(-ext.x, -ext.y, -ext.z),
      centro + new Vector3(-ext.x, -ext.y, ext.z),
      centro + new Vector3(-ext.x, ext.y, -ext.z),
      centro + new Vector3(-ext.x, ext.y, ext.z),
      centro + new Vector3(ext.x, -ext.y, -ext.z),
      centro + new Vector3(ext.x, -ext.y, ext.z),
      centro + new Vector3(ext.x, ext.y, -ext.z),
      centro + new Vector3(ext.x, ext.y, ext.z)
    };

    Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
    Vector2 max = new Vector2(float.MinValue, float.MinValue);
    bool algunaEsquinaValida = false;

    foreach (Vector3 esquina in esquinas)
    {
      Vector3 pantalla = camara.WorldToScreenPoint(esquina);
      if (pantalla.z < 0f)
      {
        continue;
      }

      algunaEsquinaValida = true;
      min = Vector2.Min(min, pantalla);
      max = Vector2.Max(max, pantalla);
    }

    if (!algunaEsquinaValida)
    {
      return false;
    }

    return new Rect(min, max - min).Contains(Input.mousePosition);
  }

  private static Vector2 ObtenerTamanoSpriteVistaTactica(Sprite sprite)
  {
    if (sprite == null)
    {
      return Vector2.one;
    }

    Vector3 tamano = sprite.bounds.size;
    float ancho = Mathf.Abs(tamano.x) > 0.0001f ? Mathf.Abs(tamano.x) : 1f;
    float alto = Mathf.Abs(tamano.y) > 0.0001f ? Mathf.Abs(tamano.y) : 1f;
    return new Vector2(ancho, alto);
  }

  private void InicializarPreviewCostoMovimiento()
  {
    spriteCostoMovimiento = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/ap_usado");
    if (spriteCostoMovimiento == null)
    {
      return;
    }

    GameObject contenedorGO = new GameObject("CostoMovimientoPreview");
    contenedorGO.transform.SetParent(transform, false);
    contenedorGO.transform.localPosition = new Vector3(0.55f, 0.01f, -0.18f);
    contenedorGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    contenedorGO.transform.localScale = Vector3.one;
    contenedorCostoMovimiento = contenedorGO.transform;
    contenedorCostoMovimiento.gameObject.SetActive(false);
  }

  private void MostrarPreviewCostoMovimiento(int costo, bool alcanzable)
  {
    if (contenedorCostoMovimiento == null || spriteCostoMovimiento == null || costo <= 0)
    {
      OcultarPreviewCostoMovimiento();
      return;
    }

    if (!contenedorCostoMovimiento.gameObject.activeSelf)
    {
      contenedorCostoMovimiento.gameObject.SetActive(true);
    }

    while (iconosCostoMovimiento.Count < costo)
    {
      GameObject iconoGO = new GameObject("APCosto");
      iconoGO.transform.SetParent(contenedorCostoMovimiento, false);
      SpriteRenderer spriteRenderer = iconoGO.AddComponent<SpriteRenderer>();
      spriteRenderer.sprite = spriteCostoMovimiento;
      spriteRenderer.sortingOrder = 400;
      iconosCostoMovimiento.Add(spriteRenderer);
    }

    float separacion = 0.25f;
    float offsetInicial = -((costo - 1) * separacion) * 0.5f;
    Color colorIcono = alcanzable ? Color.white : new Color(1f, 0.35f, 0.35f, 1f);

    for (int i = 0; i < iconosCostoMovimiento.Count; i++)
    {
      bool activo = i < costo;
      iconosCostoMovimiento[i].gameObject.SetActive(activo);
      if (!activo)
      {
        continue;
      }

      iconosCostoMovimiento[i].color = colorIcono;
      iconosCostoMovimiento[i].transform.localPosition = new Vector3(offsetInicial + (i * separacion), 0f, 0f);
      iconosCostoMovimiento[i].transform.localScale = Vector3.one * 0.030f;
    }
  }

  private void OcultarPreviewCostoMovimiento()
  {
    if (contenedorCostoMovimiento != null && contenedorCostoMovimiento.gameObject.activeSelf)
    {
      contenedorCostoMovimiento.gameObject.SetActive(false);
    }

    TooltipBatalla.InstanceCostoMovimiento?.HideTooltipSinAnim();
  }

  private void ResetearPreviewCostoMovimientoUI()
  {
    if (estaEnDestruccion || !isActiveAndEnabled || !gameObject.scene.isLoaded)
    {
      return;
    }

    if (BattleManager.Instance == null || !BattleManager.Instance.isActiveAndEnabled)
    {
      return;
    }

    if (BattleManager.Instance != null
      && BattleManager.Instance.scUIContadorAP != null
      && BattleManager.Instance.scUIContadorAP.isActiveAndEnabled
      && !BattleManager.Instance.SeleccionandoObjetivo
      && BattleManager.Instance.HabilidadActiva == null)
    {
      BattleManager.Instance.scUIContadorAP.ResetearCirculos();
    }
  }

  private bool TryObtenerCostoMovimientoHover(out int costoMovimientoTotal, out bool alcanzable)
  {
    costoMovimientoTotal = 0;
    alcanzable = false;

    if (lado == 1 || BattleManager.Instance == null || BattleManager.Instance.bOcupado)
    {
      return false;
    }

    if (!TryGetUnidadActiva(out Unidad unidad)
      || EsTurnoIA(unidad)
      || unidad.movimientoEnCurso
      || BattleManager.Instance.SeleccionandoObjetivo
      || unidad.estado_inmovil >= 1
      || !BattleManager.Instance.lCasillasMovimiento.Contains(this))
    {
      return false;
    }

    if (Presente == null)
    {
      costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad);
      alcanzable = unidad.ObtenerAPActual() >= costoMovimientoTotal;
      return true;
    }

    if (!PuedeIntercambiarConUnidadActiva())
    {
      return false;
    }

    costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad, true);
    AdministradorEscenas adminEscenas = FindObjectOfType<AdministradorEscenas>();
    if (adminEscenas != null && adminEscenas.TieneIntercambioGratisColaborativoDisponible(unidad, Presente.GetComponent<Unidad>()))
    {
      costoMovimientoTotal = 0;
    }

    alcanzable = unidad.ObtenerAPActual() >= costoMovimientoTotal;
    return true;
  }

  void Start()
  {
    BattleManager.Instance.OnRondaNueva += BattleManager_OnRondaNueva;
    if (MarcaMeleeAtraviesa != null)
    {
      MarcaMeleeAtraviesa.SetActive(false);
    }
  }

  void OnEnable()
  {
    estaEnDestruccion = false;
  }


  private void BattleManager_OnRondaNueva(object sender, EventArgs empty)
  {

    //---

  }

  private bool TryGetUnidadActiva(out Unidad unidad)
  {
    unidad = BattleManager.Instance != null ? BattleManager.Instance.unidadActiva : null;
    return unidad != null && unidad.CasillaPosicion != null;
  }

  private static bool EsTurnoIA(Unidad unidad)
  {
    return unidad != null && unidad.GetComponent<IAUnidad>() != null;
  }

  private bool PuedeMoverOMutarDesdeUnidadActiva(Casilla destino, out Unidad unidadActiva, out bool esIntercambio)
  {
    unidadActiva = null;
    esIntercambio = false;

    if (destino == null || BattleManager.Instance == null || BattleManager.Instance.bOcupado)
    {
      return false;
    }

    if (!BattleManager.Instance.lCasillasMovimiento.Contains(destino))
    {
      return false;
    }

    if (!TryGetUnidadActiva(out unidadActiva))
    {
      return false;
    }

    if (EsTurnoIA(unidadActiva))
    {
      return false;
    }

    if (unidadActiva.movimientoEnCurso || BattleManager.Instance.SeleccionandoObjetivo || unidadActiva.estado_inmovil > 0)
    {
      return false;
    }

    if (destino.Presente == null)
    {
      return unidadActiva.ObtenerAPActual() >= destino.ObtenerCostoMovimientoTotal(unidadActiva);
    }

    esIntercambio = destino.PuedeIntercambiarConUnidadActiva();
    return esIntercambio;
  }

  private bool TryObtenerDireccionMovimientoHover(out Casilla origen, out int deltaX, out int deltaY)
  {
    origen = null;
    deltaX = 0;
    deltaY = 0;

    if (!PuedeMoverOMutarDesdeUnidadActiva(this, out Unidad unidadActiva, out _))
    {
      return false;
    }

    origen = unidadActiva.CasillaPosicion;
    if (origen == null || origen == this)
    {
      return false;
    }

    deltaX = posX - origen.posX;
    deltaY = posY - origen.posY;

    return Mathf.Abs(deltaX) <= 1
      && Mathf.Abs(deltaY) <= 1
      && (deltaX != 0 || deltaY != 0);
  }

  private GameObject ObtenerMarcaMovimientoPorDireccion(int deltaX, int deltaY)
  {
    if (deltaX == 0 && deltaY == 1) { return MarcaMovX0Y1; }
    if (deltaX == 1 && deltaY == 1) { return MarcaMovX1Y1; }
    if (deltaX == 1 && deltaY == 0) { return MarcaMovX1Y0; }
    if (deltaX == -1 && deltaY == 1) { return MarcaMovXv1Y1; }
    if (deltaX == -1 && deltaY == 0) { return MarcaMovXv1Y0; }
    if (deltaX == 1 && deltaY == -1) { return MarcaMovX1Yv1; }
    if (deltaX == 0 && deltaY == -1) { return MarcaMovX0Yv1; }
    if (deltaX == -1 && deltaY == -1) { return MarcaMovXv1Yv1; }
    return null;
  }

  private void MostrarSenialadorSoloEnDireccion(int deltaX, int deltaY)
  {
    DesactivarSenialadores();

    GameObject marca = ObtenerMarcaMovimientoPorDireccion(deltaX, deltaY);
    if (marca != null)
    {
      ConfigurarVisibilidadMarcaMovimiento(marca, true);
    }
  }

  private IEnumerable<GameObject> ObtenerMarcasMovimiento()
  {
    if (MarcaMovX0Y1 != null) { yield return MarcaMovX0Y1; }
    if (MarcaMovX1Y1 != null) { yield return MarcaMovX1Y1; }
    if (MarcaMovX1Y0 != null) { yield return MarcaMovX1Y0; }
    if (MarcaMovXv1Y1 != null) { yield return MarcaMovXv1Y1; }
    if (MarcaMovXv1Y0 != null) { yield return MarcaMovXv1Y0; }
    if (MarcaMovX1Yv1 != null) { yield return MarcaMovX1Yv1; }
    if (MarcaMovX0Yv1 != null) { yield return MarcaMovX0Yv1; }
    if (MarcaMovXv1Yv1 != null) { yield return MarcaMovXv1Yv1; }
  }

  private void InicializarEstadosMarcaMovimiento()
  {
    estadosMarcaMovimiento.Clear();

    foreach (GameObject marca in ObtenerMarcasMovimiento())
    {
      if (marca == null)
      {
        continue;
      }

      Renderer[] renderers = marca.GetComponentsInChildren<Renderer>(true);
      Material[] materialesInstancia = new Material[renderers.Length];
      Color[] coloresBase = new Color[renderers.Length];
      Color[] emisionesBase = new Color[renderers.Length];

      for (int i = 0; i < renderers.Length; i++)
      {
        Material materialInstancia = new Material(renderers[i].sharedMaterial);
        ConfigurarMaterialFadeMarcaMovimiento(materialInstancia);
        renderers[i].material = materialInstancia;
        materialesInstancia[i] = materialInstancia;
        coloresBase[i] = materialInstancia.HasProperty(ShaderColorId) ? materialInstancia.GetColor(ShaderColorId) : Color.white;
        emisionesBase[i] = materialInstancia.HasProperty(ShaderEmissionColorId) ? materialInstancia.GetColor(ShaderEmissionColorId) : Color.black;
      }

      EstadoMarcaMovimiento estado = new EstadoMarcaMovimiento
      {
        objeto = marca,
        renderers = renderers,
        materialesInstancia = materialesInstancia,
        coloresBase = coloresBase,
        emisionesBase = emisionesBase,
        alphaActual = 0f,
        alphaObjetivo = 0f
      };

      estadosMarcaMovimiento[marca] = estado;
      AplicarAlphaMarcaMovimiento(estado, 0f);
      marca.SetActive(false);
    }
  }

  private static void ConfigurarMaterialFadeMarcaMovimiento(Material materialInstancia)
  {
    if (materialInstancia == null)
    {
      return;
    }

    materialInstancia.SetFloat("_Mode", 2f);
    materialInstancia.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
    materialInstancia.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    materialInstancia.SetInt("_ZWrite", 0);
    materialInstancia.DisableKeyword("_ALPHATEST_ON");
    materialInstancia.EnableKeyword("_ALPHABLEND_ON");
    materialInstancia.DisableKeyword("_ALPHAPREMULTIPLY_ON");
    materialInstancia.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
  }

  private void ConfigurarVisibilidadMarcaMovimiento(GameObject marca, bool visible)
  {
    if (marca == null)
    {
      return;
    }

    if (!estadosMarcaMovimiento.TryGetValue(marca, out EstadoMarcaMovimiento estado))
    {
      marca.SetActive(visible);
      return;
    }

    estado.alphaObjetivo = visible ? 1f : 0f;
    if (visible && !marca.activeSelf)
    {
      marca.SetActive(true);
    }
  }

  private void AplicarAlphaMarcaMovimiento(EstadoMarcaMovimiento estado, float alpha)
  {
    if (estado == null)
    {
      return;
    }

    for (int i = 0; i < estado.materialesInstancia.Length; i++)
    {
      Material materialInstancia = estado.materialesInstancia[i];
      if (materialInstancia == null)
      {
        continue;
      }

      if (materialInstancia.HasProperty(ShaderColorId))
      {
        Color color = estado.coloresBase[i];
        color.a *= alpha;
        materialInstancia.SetColor(ShaderColorId, color);
      }

      if (materialInstancia.HasProperty(ShaderEmissionColorId))
      {
        materialInstancia.SetColor(ShaderEmissionColorId, estado.emisionesBase[i] * Mathf.Lerp(0.2f, 1f, alpha));
      }
    }
  }

  private void ActualizarFadeMarcasMovimiento()
  {
    if (estadosMarcaMovimiento.Count == 0)
    {
      return;
    }

    float velocidad = duracionFadeMarcaMovimiento > 0.001f ? Time.deltaTime / duracionFadeMarcaMovimiento : 1f;
    foreach (EstadoMarcaMovimiento estado in estadosMarcaMovimiento.Values)
    {
      float nuevoAlpha = Mathf.MoveTowards(estado.alphaActual, estado.alphaObjetivo, velocidad);
      if (!Mathf.Approximately(nuevoAlpha, estado.alphaActual))
      {
        estado.alphaActual = nuevoAlpha;
        AplicarAlphaMarcaMovimiento(estado, estado.alphaActual);
      }

      if (estado.alphaObjetivo <= 0f && estado.alphaActual <= 0.001f && estado.objeto.activeSelf)
      {
        estado.objeto.SetActive(false);
      }
    }
  }

  public bool PuedeIntercambiarConUnidadActiva()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.bOcupado)
    {
      return false;
    }

    if (!BattleManager.Instance.lCasillasMovimiento.Contains(this))
    {
      return false;
    }

    if (!TryGetUnidadActiva(out Unidad unidadActiva))
    {
      return false;
    }

    if (EsTurnoIA(unidadActiva))
    {
      return false;
    }

    if (unidadActiva.movimientoEnCurso || BattleManager.Instance.SeleccionandoObjetivo || unidadActiva.estado_inmovil > 0)
    {
      return false;
    }

    if (Presente == null)
    {
      return false;
    }

    Unidad unidadPresente = Presente.GetComponent<Unidad>();
    if (unidadPresente == null || unidadPresente.CasillaPosicion == null)
    {
      return false;
    }

    if (unidadPresente.CasillaPosicion.lado != unidadActiva.CasillaPosicion.lado)
    {
      return false;
    }

    if (unidadPresente.estado_inmovil > 0 || unidadPresente.TieneBuffNombre("Desplazado"))
    {
      return false;
    }

    AdministradorEscenas admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
    if (admin != null && admin.DebeBloquearIntercambioPorIndividualista(unidadActiva, unidadPresente, out _))
    {
      return false;
    }

    if (admin != null && admin.TieneIntercambioGratisColaborativoDisponible(unidadActiva, unidadPresente))
    {
      return true;
    }

    return unidadActiva.ObtenerAPActual() >= ObtenerCostoMovimientoTotal(unidadActiva, true);
  }

  public void ActualizarSenialadores()
  {
    Invoke("ActualizarSenialadoresmetod", 0.05f);
  }

  public void ActualizarSenialadoresmetod()
  { 
     if (!TryGetUnidadActiva(out Unidad unidad))
    {
      DesactivarSenialadores();
      return;
    }

     if (unidad.CasillaPosicion != this || unidad.GetComponent<IAUnidad>() != null)
    {
      DesactivarSenialadores();
      return;
    }

    DesactivarSenialadores();
  }

  public void DesactivarSenialadores()
  {
    if (MarcaMovX0Y1 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovX0Y1, false);
    if (MarcaMovX1Y1 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovX1Y1, false);
    if (MarcaMovX1Y0 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovX1Y0, false);
    if (MarcaMovXv1Y1 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovXv1Y1, false);
    if (MarcaMovXv1Y0 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovXv1Y0, false);
    if (MarcaMovX1Yv1 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovX1Yv1, false);
    if (MarcaMovX0Yv1 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovX0Yv1, false);
    if (MarcaMovXv1Yv1 != null) ConfigurarVisibilidadMarcaMovimiento(MarcaMovXv1Yv1, false);






  }

  private void ActualizarSenialadorDireccion(int deltaX, int deltaY, GameObject marca)
  {
    if (marca == null) { return; }
    if (!TryGetUnidadActiva(out Unidad unidad))
    {
      ConfigurarVisibilidadMarcaMovimiento(marca, false);
      return;
    }

    Casilla destino = EncontrarCasillaEnPosicion(posX + deltaX, posY + deltaY);
    bool puedeMover = false;

    if (destino != null)
    {
      if (destino.Presente == null)
      {
        puedeMover = true;
      }
      else
      {
        if (destino.Presente.GetComponent<Obstaculo>() != null)
        {
          puedeMover = false;
        }
        else
        {
          Unidad unidadDestino = destino.Presente.GetComponent<Unidad>();
          if (unidadDestino != null && !unidadDestino.TieneBuffNombre("Desplazado"))
          {
            bool esIntercambioAliado = unidadDestino.CasillaPosicion != null
              && unidad.CasillaPosicion != null
              && unidadDestino.CasillaPosicion.lado == unidad.CasillaPosicion.lado
              && unidadDestino.estado_inmovil < 1;
            if (esIntercambioAliado)
            {
              puedeMover = unidad.ObtenerAPActual() >= destino.ObtenerCostoMovimientoTotal(unidad, true);
            }
          }
        }
      }
      if (destino.Presente == null)
      {
        puedeMover = unidad.ObtenerAPActual() >= destino.ObtenerCostoMovimientoTotal(unidad);
      }
    }

    ConfigurarVisibilidadMarcaMovimiento(marca, puedeMover);
  }

  public TooltipBatalla scTooltipBatalla;

  private TooltipBatalla ObtenerTooltipBatallaGeneral()
  {
    if (TooltipBatalla.Instance != null)
    {
      return TooltipBatalla.Instance;
    }

    return scTooltipBatalla != null && !scTooltipBatalla.usoSoloCostoMov ? scTooltipBatalla : null;
  }

  private bool EsMovimientoDiagonal(Unidad unidad)
  {
    if (unidad == null || unidad.CasillaPosicion == null)
    {
      return false;
    }

    int diferenciaX = Mathf.Abs(unidad.CasillaPosicion.posX - posX);
    int diferenciaY = Mathf.Abs(unidad.CasillaPosicion.posY - posY);
    return diferenciaX == 1 && diferenciaY == 1;
  }

  private bool DebeSumarCostoBarroAlEntrar(Unidad unidad)
  {
    if (unidad == null)
    {
      return false;
    }

    Trampa trampa = GetComponent<Trampa>();
    if (trampa is not TrampaBarro trampaBarro)
    {
      return false;
    }

    if (unidad.inmunidad_Trampas && !trampaBarro.esTrampaFavorable)
    {
      return false;
    }

    REPRESENTACIONPasoCauteloso pasoCauteloso = unidad.GetComponent<REPRESENTACIONPasoCauteloso>();
    if (pasoCauteloso != null && !pasoCauteloso.seusoEsteTurno)
    {
      return false;
    }

    return true;
  }

  private int ObtenerCostoMovimientoTotal(Unidad unidad, bool esIntercambioConAliado = false)
  {
    int costoMovimientoTotal = costoMovimiento;
    if (EsMovimientoDiagonal(unidad))
    {
      costoMovimientoTotal++;
    }
    if (DebeSumarCostoBarroAlEntrar(unidad))
    {
      costoMovimientoTotal += 2;
    }
    if (DebeAplicarsePasoLigero(unidad, esIntercambioConAliado))
    {
      costoMovimientoTotal = Mathf.Max(0, costoMovimientoTotal - 1);
    }
    if (DebeAplicarseMovimientoAbaratado(unidad))
    {
      costoMovimientoTotal = Mathf.Max(0, costoMovimientoTotal - 1);
    }
    return costoMovimientoTotal;
  }

  private bool DebeAplicarseMovimientoAbaratado(Unidad unidad)
  {
    return unidad != null && unidad.estado_MovimientoAbaratado > 0;
  }

  private bool DebeAplicarsePasoLigero(Unidad unidad, bool esIntercambioConAliado)
  {
    if (unidad == null)
    {
      return false;
    }

    bool esMovimientoElegible = esIntercambioConAliado || EsMovimientoDiagonal(unidad);
    if (!esMovimientoElegible)
    {
      return false;
    }

    ClaseDuelista duelista = unidad.GetComponent<ClaseDuelista>();
    return duelista != null && duelista.PuedeUsarPasoLigero();
  }

  private void ConsumirPasoLigeroSiCorresponde(Unidad unidad, bool esIntercambioConAliado)
  {
    if (unidad == null)
    {
      return;
    }

    bool esMovimientoElegible = esIntercambioConAliado || EsMovimientoDiagonal(unidad);
    if (!esMovimientoElegible)
    {
      return;
    }

    ClaseDuelista duelista = unidad.GetComponent<ClaseDuelista>();
    duelista?.ConsumirPasoLigero();
  }

  private void ConsumirMovimientoAbaratadoSiCorresponde(Unidad unidad)
  {
    if (!DebeAplicarseMovimientoAbaratado(unidad))
    {
      return;
    }

    unidad.estado_MovimientoAbaratado--;
    if (unidad.estado_MovimientoAbaratado < 0)
    {
      unidad.estado_MovimientoAbaratado = 0;
    }

    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);
  }

  private string ObtenerTextoCostoMovimiento(int costoMovimientoTotal)
  {
    if (TRADU.i == null)
    {
      return "Coste: " + costoMovimientoTotal + " PA";
    }

    return TRADU.i.Traducir("Coste: ") + costoMovimientoTotal + " " + TRADU.i.Traducir("PA");
  }

  private void MostrarTooltipCostoMovimiento(int costoMovimientoTotal)
  {
    TooltipBatalla tooltipCostoMovimiento = TooltipBatalla.InstanceCostoMovimiento;
    if (tooltipCostoMovimiento == null) { return; }

    tooltipCostoMovimiento.ShowTooltipCostoMovimiento(ObtenerTextoCostoMovimiento(costoMovimientoTotal));
  }

  public void MostrarAPparaMovimiento()
  {
    if (!TryObtenerCostoMovimientoHover(out int costoMovimientoTotal, out bool alcanzable))
    {
      OcultarPreviewCostoMovimiento();
      ResetearPreviewCostoMovimientoUI();
      return;
    }

    MostrarPreviewCostoMovimiento(costoMovimientoTotal, alcanzable);
    MostrarTooltipCostoMovimiento(costoMovimientoTotal);

    if (BattleManager.Instance != null && BattleManager.Instance.scUIContadorAP != null)
    {
      BattleManager.Instance.scUIContadorAP.ResetearCirculos();
      if (costoMovimientoTotal > 0)
      {
        BattleManager.Instance.scUIContadorAP.MarcarCirculos(costoMovimientoTotal);
      }
    }
  }

  public void MostrarTooltipIntercambiar()
  {
    TooltipBatalla tooltipBatalla = ObtenerTooltipBatallaGeneral();
    if (tooltipBatalla == null) { return; }

    string texto = TRADU.i != null ? TRADU.i.Traducir("Intercambiar") : "Intercambiar";
    tooltipBatalla.ShowTooltipTextSinAnimDirecto(texto);
  }

  public async void OnMouseDown()
  {
    if (BattleManager.Instance == null)
    {
      return;
    }

    //----
    if(BattleManager.Instance.scTutorialCombate.tutorialCombateActivo && BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() < 6)
    {

      return;
    }
    if (BattleManager.Instance.scTutorialCombate.tutorialCombateActivo && BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() == 6 && (posX != 3 || posY != 3))
    {

      return;
    }
    else if (BattleManager.Instance.scTutorialCombate.tutorialCombateActivo && BattleManager.Instance.scTutorialCombate.ObtenerPasoActual() == 6)
    {
      BattleManager.Instance.scTutorialCombate.SiguientePasoCombate();
    }

    if (!TryGetUnidadActiva(out Unidad unidad))
    {
      return;
    }

    if (EsTurnoIA(unidad))
    {
      return;
    }

    if (DebeIgnorarInputUnidadVistaTactica())
    {
      return;
    }

    if (await TryResolverObjetivoUnidadVistaTactica())
    {
      return;
    }

    // --- Cancelar habilidad activa si se hace clic en el campo ---
    if (BattleManager.Instance.HabilidadActiva != null)
    {
      if (BattleManager.Instance.HabilidadActiva.esHostil && unidad.CasillaPosicion.lado == this.lado)
      {
        // Cancela la selección de la habilidad al clikear casilla, solo si es hostil y es una casilla del mismo lado
        BattleManager.Instance.HabilidadActiva = null;
        BattleManager.Instance.SeleccionandoObjetivo = false;
        BattleManager.Instance.LimpiarCapasCasillas();
        BattleManager.Instance.scUIContadorAP.ResetearCirculos();
        BattleManager.Instance.scUIBotonesHab.DeseleccionarTodas();
        // return; // Sale sin ejecutar nada más
      }
    }





    //Unidad seleccionada - Movimiento
    //!!!
    unidad.CasillaPosicion.CalcularDistanciaACasilla(this, out int x, out int y, out bool lado);
    //!!!
    if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente == null && !BattleManager.Instance.bOcupado && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      int costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad);
      if (unidad.ObtenerAPActual() >= costoMovimientoTotal)
      {

        ConsumirPasoLigeroSiCorresponde(unidad, false);
        ConsumirMovimientoAbaratadoSiCorresponde(unidad);
        unidad.CambiarAPActual(-costoMovimientoTotal);
        BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();
        unidad.CasillaDeseadaMov = this;
        RuntimeAnalytics.TrackDesign("combat", "move_confirmed", "step");
        await unidad.GenerarTextoFlotante("<size=70%>-" + costoMovimientoTotal + " " + TRADU.i.Traducir(" PA") + "</size>", new Color(1.0f, 0.5f, 0.0f)); // Naranja
      }
    }
    // Intercambio con aliado: mover a casilla ocupada por aliado y que el aliado vaya a la casilla original
    if (BattleManager.Instance.lCasillasMovimiento.Contains(this) && Presente != null && !BattleManager.Instance.bOcupado && !unidad.movimientoEnCurso && !BattleManager.Instance.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      Unidad aliado = Presente != null ? Presente.GetComponent<Unidad>() : null;
      if (aliado != null)
      {
        if (aliado.CasillaPosicion.lado != unidad.CasillaPosicion.lado)
        {
          BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con enemigos."));
          return;
        }
        else if (aliado.estado_inmovil > 0)
        {
          BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con una unidad inmovilizada."));
          return;
        }
        else if (aliado.TieneBuffNombre("Desplazado"))
        {
          BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con una unidad que ya está Desplazada."));
          return;
        }
        else
        {
          int costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad, true);
          AdministradorEscenas admin = CampaignManager.Instance != null ? CampaignManager.Instance.scAdministradorEscenas : null;
          if (admin != null && admin.DebeBloquearIntercambioPorIndividualista(unidad, aliado, out Unidad unidadTextoBloqueo))
          {
            admin.MostrarTextoIntercambioBloqueadoIndividualista(unidadTextoBloqueo);
            return;
          }

          bool intercambioGratisColaborativo = admin != null && admin.TieneIntercambioGratisColaborativoDisponible(unidad, aliado);
          if (intercambioGratisColaborativo)
          {
            costoMovimientoTotal = 0;
          }

          if (unidad.ObtenerAPActual() >= costoMovimientoTotal)
          {
            if (!intercambioGratisColaborativo)
            {
              ConsumirPasoLigeroSiCorresponde(unidad, true);
              ConsumirMovimientoAbaratadoSiCorresponde(unidad);
            }
            unidad.CambiarAPActual(-costoMovimientoTotal);
            if (intercambioGratisColaborativo)
            {
              admin?.TryConsumirIntercambioGratisColaborativo(unidad, aliado);
            }
            BattleManager.Instance.scUIContadorAP.ActualizarAPCirculos();

            Casilla origen = unidad.CasillaPosicion;

            // Forzar movimiento del aliado hacia el origen del activo
            aliado.CasillaForzadoaMover = origen;

            // Aplicar Debuff Desplazado (-1 AP max por 1 turno)
            Buff desplazado = new Buff();
            desplazado.buffNombre = "Desplazado";
            desplazado.buffDescr = "AP máx -1 por 1 turno";
            desplazado.boolfDebufftBuff = false;
            desplazado.cantAPMax = -1;
            desplazado.DuracionBuffRondas = 1;
            desplazado.AplicarBuff(aliado);
            Buff buffComponent = ComponentCopier.CopyComponent(desplazado, aliado.gameObject);

            // Mover la unidad activa a esta casilla
            unidad.CasillaDeseadaMov = this;
            RuntimeAnalytics.TrackDesign("combat", "move_confirmed", "swap");
          }
          else
          {
            BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No tienes PA suficientes para intercambiar."));
          }
        }
      }
      else
      {
        BattleManager.Instance.EscribirLog(TRADU.i.Traducir("No puedes intercambiar con obstáculos."));
      }

    }

    //Para habilidades en área
    if (BattleManager.Instance.HabilidadActiva != null && !BattleManager.Instance.bOcupado)
    {

      if (BattleManager.Instance.HabilidadActiva.enArea > 0 || BattleManager.Instance.HabilidadActiva.targetEspecial > 0 && BattleManager.Instance.SeleccionandoObjetivo)
      {

        if (!BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this)) { return; } //Si no está en el área, no hace nada


        List<Unidad> lUnidadesEnArea = new List<Unidad>();
        List<Obstaculo> lObstaculosEnArea = new List<Obstaculo>();

        foreach (Unidad enAzul in unidadesEnCasAzul)
        {
          if (lUnidadesEnArea.Contains(enAzul) == false) //Que no se duplique
          { lUnidadesEnArea.Add(enAzul); }
        }
        foreach (Obstaculo enAzul in obstaculosEnCasAzul)
        {
          if (lObstaculosEnArea.Contains(enAzul) == false) //Que no se duplique
          { lObstaculosEnArea.Add(enAzul); }
        }

        foreach (Casilla u in BattleManager.Instance.HabilidadActiva.lCasillasafectadas)
        {
          if (u.Presente != null)
          {
            if (u.Presente.GetComponent<Unidad>() != null)
            {
              if (unidadesEnCasAzul.Contains(u.Presente.GetComponent<Unidad>()) && lUnidadesEnArea.Contains(u.Presente.GetComponent<Unidad>()) == false)
              {
                lUnidadesEnArea.Add(u.Presente.GetComponent<Unidad>());
              }

            }
            if (u.Presente.GetComponent<Obstaculo>() != null)
            {
              if (obstaculosEnCasAzul.Contains(u.Presente.GetComponent<Obstaculo>()) && lObstaculosEnArea.Contains(u.Presente.GetComponent<Obstaculo>()) == false)
              {
                lObstaculosEnArea.Add(u.Presente.GetComponent<Obstaculo>());
              }
            }

          }


        }

        if (BattleManager.Instance.HabilidadActiva.poneTrampas) //Habilidad que pone "trampas" en casillas
        {
          BattleManager.Instance.casillaClickHabilidad = this;
          await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
          BattleManager.Instance.casillaClickHabilidad = null;
        }
        else if (BattleManager.Instance.HabilidadActiva.poneObstaculo)
        {
          if (Presente == null) // Si es habilidad que pone obstaculo, debe estar vacia la casilla
          {
            BattleManager.Instance.casillaClickHabilidad = this;
            await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
            BattleManager.Instance.casillaClickHabilidad = null;
          }
        }
        else //Habilidades Normales
        {
          // List<object> listResolverUnidades = new List<object>();
          // listResolverUnidades.AddRange(lUnidadesEnArea); print(11);
          // await BattleManager.Instance.HabilidadActiva.Resolver(listResolverUnidades);

          //---La idea es que resolver se llame 1 sola vez para evitar efectos de algunas habilidades duplicados.
          List<object> listResolverUnidades = new List<object>();
          listResolverUnidades.AddRange(lUnidadesEnArea);
          BattleManager.Instance.casillaClickHabilidad = this;
          if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
          {
            await BattleManager.Instance.HabilidadActiva.Resolver(listResolverUnidades);
          }
          else
          {
            List<object> listResolverObstaculos = new List<object>();
            listResolverObstaculos.AddRange(lObstaculosEnArea);
            List<object> combinedList = new List<object>();
            combinedList.AddRange(listResolverObstaculos);
            combinedList.AddRange(listResolverUnidades);
            await BattleManager.Instance.HabilidadActiva.Resolver(combinedList);

          }

          BattleManager.Instance.casillaClickHabilidad = null;
        }


        BattleManager.Instance.scUIBotonesHab.UIDesactivarBotones();


      }
      else if (BattleManager.Instance.SeleccionandoObjetivo && BattleManager.Instance.HabilidadActiva.poneTrampas && BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this))
      {
        BattleManager.Instance.casillaClickHabilidad = this;
        await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
        BattleManager.Instance.casillaClickHabilidad = null;
      }
      else if (BattleManager.Instance.SeleccionandoObjetivo && BattleManager.Instance.HabilidadActiva.esZonal && BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this))
      {
        List<object> objetos = BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Cast<object>().ToList();
        BattleManager.Instance.casillaClickHabilidad = this;
        await BattleManager.Instance.HabilidadActiva.Resolver(objetos, this);
        BattleManager.Instance.casillaClickHabilidad = null;
      }
      else if (BattleManager.Instance.HabilidadActiva.poneObstaculo)
      {
        if (Presente == null && BattleManager.Instance.HabilidadActiva.lCasillasafectadas.Contains(this)) // Si es habilidad que pone obstaculo, debe estar vacia la casilla
        {
          BattleManager.Instance.casillaClickHabilidad = this;
          await BattleManager.Instance.HabilidadActiva.Resolver(null, this);
          BattleManager.Instance.casillaClickHabilidad = null;
        }
      }
    }

    TryToggleFijadoVistaTactica();



  }

  private bool TryToggleFijadoVistaTactica()
  {
    BattleManager battleManager = BattleManager.Instance;
    if (battleManager == null
      || !battleManager.VistaTacticaActiva
      || battleManager.SeleccionandoObjetivo
      || battleManager.HabilidadActiva != null
      || battleManager.scUIInfoChar == null
      || Presente == null
      || !MouseSobreImagenVistaTactica())
    {
      return false;
    }

    if (battleManager.lCasillasMovimiento.Contains(this))
    {
      return false;
    }

    Unidad unidad = Presente.GetComponent<Unidad>();
    if (unidad == null)
    {
      return false;
    }

    battleManager.scUIInfoChar.ToggleFijado(unidad);
    return true;
  }

  private async Task<bool> TryResolverObjetivoUnidadVistaTactica()
  {
    BattleManager battleManager = BattleManager.Instance;
    if (battleManager == null
      || !battleManager.VistaTacticaActiva
      || !battleManager.SeleccionandoObjetivo
      || battleManager.HabilidadActiva == null
      || battleManager.bOcupado
      || Presente == null
      || !MouseSobreImagenVistaTactica())
    {
      return false;
    }

    Habilidad habilidad = battleManager.HabilidadActiva;
    if (habilidad.esZonal || habilidad.enArea > 0 || habilidad.targetEspecial > 0 || habilidad.poneTrampas || habilidad.poneObstaculo)
    {
      return false;
    }

    Unidad objetivo = Presente.GetComponent<Unidad>();
    if (objetivo == null || !battleManager.lUnidadesPosiblesHabilidadActiva.Contains(objetivo))
    {
      return false;
    }

    if (habilidad.esMelee && objetivo.estado_Volando)
    {
      await objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Inalcanzable: unidad volando"), Color.gray, FloatingTextContext.Resist);
      return true;
    }

    if (habilidad.esHostil && objetivo.ObtenerEstaEscondido() > 0)
    {
      await objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Inalcanzable: unidad escondida"), Color.gray, FloatingTextContext.Resist);
      return true;
    }

    await habilidad.Resolver(new List<object> { objetivo });
    battleManager.scUIInfoChar.RefrescarSegunEstadoActual();
    return true;
  }




  public bool PonerObjetoEnCasilla(GameObject GO)
  {

    if (Presente != null)
    {
      return false;
    }
    GO.transform.position = transform.position;
    NuevoObjetoPresenteEnCasilla(GO);


    if (GO.GetComponent<Unidad>() != null)
    {
      GO.GetComponent<Unidad>().CasillaPosicion = this;
    }
    if (GO.GetComponent<Obstaculo>() != null)
    {
      GO.GetComponent<Obstaculo>().CasillaPosicion = this;
    }

    //---
    // Reduce the scale of the object to 87% of its original size
    GO.transform.localScale = new Vector3(GO.transform.localScale.x * 0.9f, GO.transform.localScale.y * 0.9f, GO.transform.localScale.z * 0.9f);
    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.AplicarTamanioUnidadBatalla(GO.GetComponent<Unidad>());
    }
    return true;

  }

  public void AplicarEscalaPerspectivaUnidad(GameObject GO)
  {
    if (GO == null)
    {
      return;
    }

    Unidad unidad = GO.GetComponent<Unidad>();
    if (unidad == null)
    {
      return;
    }

    float multiplicadorPerspectiva = BattleManager.Instance != null
      ? BattleManager.Instance.ObtenerMultiplicadorEscalaPerspectivaUnidad(posY)
      : 1f;
    GO.transform.localScale *= multiplicadorPerspectiva;
  }

  public void PonerObjetoEnCasillaAnimado(GameObject GO, int lado)
  {

    if (Presente != null)
    {
      print("Casilla Ocupada, no se puede colocar objeto");
      return;
    }

    // Iniciar la corrutina para mover el objeto de forma animada
    StartCoroutine(MoverObjetoAnimado(GO, lado));

    // Asignar el objeto como presente en la casilla
    NuevoObjetoPresenteEnCasilla(GO);

    // Si es una Unidad u Obstáculo, actualizar su casilla de posición
    if (GO.GetComponent<Unidad>() != null)
    {
      GO.GetComponent<Unidad>().CasillaPosicion = this;
    }
    if (GO.GetComponent<Obstaculo>() != null)
    {
      GO.GetComponent<Obstaculo>().CasillaPosicion = this;
    }
    // Reduce the scale of the object to 87% of its original size
    GO.transform.localScale = new Vector3(GO.transform.localScale.x * 0.9f, GO.transform.localScale.y * 0.9f, GO.transform.localScale.z * 0.9f);
    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.AplicarTamanioUnidadBatalla(GO.GetComponent<Unidad>());
    }
  }

  IEnumerator MoverObjetoAnimado(GameObject GO, int lado)
  {
    if (lado == 2)//Enemigos
    {
      Vector3 posicionFinal = transform.position; // Posición de la casilla
      Vector3 posicionInicial = posicionFinal + new Vector3(3f, 0, 0); // Posición inicial (desplazada a la derecha)

      float duracion = 0.7f; // Duración del movimiento
      float tiempo = 0;

      while (tiempo < duracion)
      {
        tiempo += Time.deltaTime;
        float t = Mathf.Clamp01(tiempo / duracion); // Normalizar el tiempo (0 a 1)

        // Interpolar la posición entre la inicial y la final
        GO.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);

        yield return null; // Esperar al siguiente frame
      }

      // Asegurarse de que el objeto está exactamente en la posición final
      GO.transform.position = posicionFinal;
    }
    else if (lado == 1) //Aliados
    {
      Vector3 posicionFinal = transform.position; // Posición de la casilla
      Vector3 posicionInicial = posicionFinal + new Vector3(-3f, 0, 0); // Posición inicial (desplazada a la izquierda)

      float duracion = 0.7f; // Duración del movimiento
      float tiempo = 0;

      while (tiempo < duracion)
      {
        tiempo += Time.deltaTime;
        float t = Mathf.Clamp01(tiempo / duracion); // Normalizar el tiempo (0 a 1)

        // Interpolar la posición entre la inicial y la final
        GO.transform.position = Vector3.Lerp(posicionInicial, posicionFinal, t);

        yield return null; // Esperar al siguiente frame
      }

      // Asegurarse de que el objeto está exactamente en la posición final
      GO.transform.position = posicionFinal;
    }
  }




  public List<Casilla> ObtenerCasillasAlrededor(int x)
  {
    List<Casilla> lCasillas = new List<Casilla>();

    // Obtén la casilla actual
    int posXActual = this.posX;
    int posYActual = this.posY;

    // Recorre las casillas en el rango especificado (x)
    for (int i = -x; i <= x; i++)
    {
      for (int j = -x; j <= x; j++)
      {
        // Calcula las coordenadas de la casilla vecina
        int xVecina = posXActual + i;
        int yVecina = posYActual + j;

        // Verifica si la casilla vecina está dentro del rango especificado (distancia x)
        if (Mathf.Abs(xVecina - posXActual) + Mathf.Abs(yVecina - posYActual) <= x)
        {
          // Asegúrate de no agregar la casilla actual a la lista
          if (xVecina == posXActual && yVecina == posYActual)
            continue;

          // Si x es 1, agrega las casillas adyacentes (sin diagonales)
          if (x == 1 && (Mathf.Abs(i) == 1 || Mathf.Abs(j) == 1) && !(Mathf.Abs(i) == 1 && Mathf.Abs(j) == 1))
          {
            Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
            if (casillaVecina != null)
            {
              lCasillas.Add(casillaVecina);
            }
          }
          // Si x es 2, agrega las casillas adyacentes y las diagonales inmediatas
          else if (x == 2 && Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1)
          {
            Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
            if (casillaVecina != null)
            {
              lCasillas.Add(casillaVecina);
            }
          }
          // Si x es mayor que 2, agrega las casillas no diagonales a distancia 2
          else if (x > 2 && ((Mathf.Abs(i) == 2 && Mathf.Abs(j) != 2 || Mathf.Abs(j) == 2 && Mathf.Abs(i) != 2) || (Mathf.Abs(i) <= 1 && Mathf.Abs(j) <= 1)))
          {
            Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
            if (casillaVecina != null)
            {
              lCasillas.Add(casillaVecina);
            }
          }
        }
      }
    }

    return lCasillas;
  }

  public List<Casilla> ObtenerCasillasenMismaFila()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posY == posY && cas.lado == lado)
      {
        lCasillas.Add(cas);
      }

    }

    return lCasillas;
  }

  public Casilla ObtenerCasillasMasAtrasEnFila()
  {
    Casilla lCas = this;
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posY == posY && cas.lado == lado)
      {
        if (cas.posX < posX)
        {
          lCas = cas;

        }

      }

    }

    return lCas;
  }

  public List<Casilla> ObtenerCasillasMismoLado()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.lado == lado)
      {
        lCasillas.Add(cas);
      }

    }

    return lCasillas;
  }

  public List<Casilla> ObtenerCasillasLadoOpuesto()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.lado != lado)
      {
        lCasillas.Add(cas);

      }

    }

    return lCasillas;
  }

  public List<Casilla> ObtenerCasillasenMismaColumna()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posX == posX && cas.lado == lado)
      {
        lCasillas.Add(cas);
      }

    }

    return lCasillas;
  }
  public List<Casilla> ObtenerCasillasAdyacentesEnColumna()
  {
    List<Casilla> lCasillas = new List<Casilla>();
    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas.posX == posX && cas.lado == lado)
      {
        // Comprueba si la casilla es adyacente en la columna (diferencia de 1 en posY)
        if (Mathf.Abs(cas.posY - posY) == 1)
        {
          lCasillas.Add(cas);
        }
      }

    }

    return lCasillas;

  }
  public List<Casilla> ObtenerCasillasAlrededorParaMovimiento()
  {
    List<Casilla> lCasillas = new List<Casilla>();

    // Obtén la casilla actual
    int posXActual = this.posX;
    int posYActual = this.posY;

    // Recorre las casillas en el rango especificado (x)
    for (int i = -1; i <= 1; i++)
    {
      for (int j = -1; j <= 1; j++)
      {
        // Asegúrate de no agregar la casilla actual a la lista
        if (i == 0 && j == 0)
          continue;

        // Calcula las coordenadas de la casilla vecina
        int xVecina = posXActual + i;
        int yVecina = posYActual + j;

        // Agrega la casilla vecina a la lista si está dentro del rango especificado (distancia x)
        if (Mathf.Abs(xVecina - posXActual) <= 1 && Mathf.Abs(yVecina - posYActual) <= 1)
        {
          // Encuentra la casilla vecina en la posición (xVecina, yVecina) y agrégala a la lista
          Casilla casillaVecina = EncontrarCasillaEnPosicion(xVecina, yVecina);
          if (casillaVecina != null)
          {
            lCasillas.Add(casillaVecina);
          }
        }
      }
    }

    return lCasillas;
  }
  public List<Casilla> ObtenerCasillasRango(int alcance, int ancho/*0 es en la misma fila, 1 tmb en adyacentes*/) //Segun la posicion en su lado obtiene hasta que casillas del lado opuesto llega la habilidad según el alcance
  {

    List<Casilla> lCasillas = new List<Casilla>();

    // Obtén la casilla actual
    int posXActual = this.posX;
    int posYActual = this.posY;

    int RangoEnOtroLado = alcance - (3 - posX);
    lCasillas = ladoOpuesto.GetComponent<LadoManager>().filaCasillasSegunRango(posY, RangoEnOtroLado, ancho);



    return lCasillas;
  }

  private Casilla EncontrarCasillaEnPosicion(int posX, int posY)
  {
    // Obtenemos el transform del padre de las casillas (supongamos que todas las casillas están en el mismo padre)
    Transform padreDeCasillas = transform.parent;

    // Recorremos todos los objetos hijos del padre
    foreach (Transform hijo in padreDeCasillas)
    {
      // Comprobamos si el hijo tiene un componente Casilla
      Casilla casilla = hijo.GetComponent<Casilla>();
      if (casilla != null)
      {
        // Comparamos las coordenadas
        if (casilla.posX == posX && casilla.posY == posY)
        {
          // Devolvemos la casilla encontrada
          return casilla;
        }
      }
    }

    // Si no se encontró ninguna casilla en la posición dada, devolvemos null
    return null;
  }

  public bool bTieneUnidad()
  {
    if (Presente == null)
    {
      return false;
    }

    if (Presente.GetComponent<Unidad>() != null)
    {
      return true;
    }
    else { return false; }

  }

  public bool bTieneObstaculo()
  {
    if (Presente == null)
    {
      return false;
    }

    if (Presente.GetComponent<Obstaculo>() != null)
    {
      return true;
    }
    else { return false; }

  }

  public bool bTieneUnidadoObstaculoParaMelee()
  {
    // Compatibilidad: conserva el comportamiento previo (obstaculo siempre bloquea)
    // evaluando contra su propia fila.
    return BloqueaAvanceMeleeDesdeFila(posY);
  }

  // Regla central de avance melee:
  // - Las unidades visibles/no volando siempre bloquean, salvo espectros etereos.
  // - Los obstaculos solo bloquean si estan en la misma fila (posY) que el origen.
  public bool BloqueaAvanceMeleeDesdeFila(int posYorigen, Unidad atacante = null)
  {
    if (Presente == null)
    {
      return false;
    }

    Unidad unidad = Presente.GetComponent<Unidad>();
    if (unidad != null)
    {
      if (unidad.ObtenerEstaEscondido() != 0 || unidad.estado_Volando)
      {
        return false;
      }

      IAUnidadEspectroBosque espectroBosque = unidad.GetComponent<IAUnidadEspectroBosque>();
      if (espectroBosque != null && espectroBosque.EstaEnPlanoEtereo())
      {
        return false;
      }

      if (atacante != null && atacante.CasillaPosicion != null && unidad.CasillaPosicion != null)
      {
        Unidad provocador = atacante.ObtenerProvocadorVigente();
        bool esUnidadEnemiga = unidad.CasillaPosicion.lado != atacante.CasillaPosicion.lado;
        if (provocador != null && esUnidadEnemiga && unidad != provocador)
        {
          return false;
        }
      }

      return true;
    }

    if (Presente.GetComponent<Obstaculo>() != null)
    {
      return posY == posYorigen;
    }

    return false;
  }

  public void ActivarCapaColorRojo()
  {
    transform.GetChild(1).gameObject.SetActive(true);
    ActualizarCirculoObjetivoHabilidad();
  }

  public void DesactivarCapaColorRojo()
  {
    transform.GetChild(1).gameObject.SetActive(false);
  }
  public void ActivarCapaColorNegro()
  {
    transform.GetChild(1).gameObject.SetActive(false); //desactiva la capa roja también
    GetComponent<MeshRenderer>().enabled = false; //desactiva la casilla en si
    transform.GetChild(2).gameObject.SetActive(true);
  
    if (MarcaMeleeAtraviesa != null)
    {
      MarcaMeleeAtraviesa.SetActive(true);
    }
  }
  public void ActivarCapaColorAzul()
  {
    transform.GetChild(0).gameObject.SetActive(true);
    ActualizarCirculoObjetivoHabilidad();
  }
  public void DesactivarCapaColorAzul()
  {
    transform.GetChild(0).gameObject.SetActive(false);
  }



  public void DesactivarCapas()
  {
    transform.GetChild(0).gameObject.SetActive(false);
    transform.GetChild(1).gameObject.SetActive(false);
    transform.GetChild(2).gameObject.SetActive(false);
    transform.GetChild(2).gameObject.SetActive(false);
    transform.GetChild(9).gameObject.SetActive(false);
    transform.GetChild(11).gameObject.SetActive(false);
    GetComponent<MeshRenderer>().enabled = true;
    if (MarcaMeleeAtraviesa != null)
    {
      MarcaMeleeAtraviesa.SetActive(false);
    }
    //Agregar mas
  }

  List<Casilla> casAlre = new List<Casilla>();

  [SerializeField] public List<Unidad> unidadesEnCasAzul = new List<Unidad>();
  [SerializeField] public List<Obstaculo> obstaculosEnCasAzul = new List<Obstaculo>();
  public void OnMouseOver()
  {
    SetHoverVistaTactica(true);
    if (DebeIgnorarInputUnidadVistaTactica())
    {
      return;
    }

    ActualizarHoverMovimientoVisual();
    ActualizarPulsoObjetivoHabilidad();

    MostrarAPparaMovimiento();
    unidadesEnCasAzul.Clear();
    obstaculosEnCasAzul.Clear();
    casAlre.Clear();


    string text = "";
   
    if (MarcaMelee != null && MarcaMelee.activeInHierarchy)
    {
       text += "" + TRADU.i.Traducir("Melee disponible \n(Ver: Mantén Shift)");
      TooltipBatalla.InstanceCostoMovimiento?.ShowTooltipCostoMovimiento(text);
    }
    else if (BattleManager.Instance != null
      && !BattleManager.Instance.SeleccionandoObjetivo
      && BattleManager.Instance.HabilidadActiva == null
      && PuedeIntercambiarConUnidadActiva()
      && !TryObtenerCostoMovimientoHover(out _, out _))
    {
      MostrarTooltipIntercambiar();
    }

    if (BattleManager.Instance != null && BattleManager.Instance.MostrarPreviewHoverMeleeGenericoDesdeCasilla(this))
    {
      return;
    }

    if (BattleManager.Instance != null && BattleManager.Instance.MostrarPreviewHoverHostilDesdeCasilla(this))
    {
      ActualizarFadeHoverObjetivosHabilidad();
      return;
    }

    //Controlar se esta haciendo hablidad en Area, marca las casillas en la zona de alcance y en el area
    if (BattleManager.Instance.HabilidadActiva != null)
    {
      if (BattleManager.Instance.SeleccionandoObjetivo
        && (BattleManager.Instance.HabilidadActiva.enArea > 0 || BattleManager.Instance.HabilidadActiva.targetEspecial > 0))
      {
        DesmarcarTodasLasCasillasAzules();
      }

      if (BattleManager.Instance.HabilidadActiva.enArea > 0 && BattleManager.Instance.SeleccionandoObjetivo)
      {

        casAlre = ObtenerCasillasAlrededor(BattleManager.Instance.HabilidadActiva.enArea);
        foreach (Casilla cas in casAlre)
        {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

        }
        MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

      }
      else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 1)  //Target Especial 1: misma fila (horizontal)
        {
          casAlre = ObtenerCasillasenMismaFila();
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }

        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 2)  //Target Especial 2: misma columna (Vertical)
        {
          casAlre = ObtenerCasillasenMismaColumna();
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 3) //Target Especial 3: Dos Casillas (Vertical)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (cas.posY == posY + 1 && cas.posX == posX && cas.lado == lado)
            {
              casAlre.Add(cas);
            }

          }

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 4) //Target Especial 4: Tres Casillas (Vertical)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (((cas.posY == posY + 1 && cas.posX == posX) || (cas.posY == posY - 1 && cas.posX == posX)) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 5) //Target Especial 5: Dos Casillas (Atrás)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if ((cas.posY == posY && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 6) //Target Especial 6: Tres Casillas y las de atras (Vertical)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (((cas.posY == posY + 1 && cas.posX == posX) || (cas.posY == posY - 1 && cas.posX == posX)) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if (((cas.posY == posY + 1 && cas.posX == posX - 1) || (cas.posY == posY - 1 && cas.posX == posX - 1)) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if (cas.posY == posY && cas.posX == posX - 1 && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 7) //Target Especial 7: La del origen y diagonales adyacentes X
        {
          casAlre.Add(this); //Agrega la casilla de origen
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {

            if ((cas.posY == posY + 1 && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY + 1 && cas.posX == posX + 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY - 1 && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY - 1 && cas.posX == posX + 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }

          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 8) //Target Especial 8: T horizontal
        {

          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if ((cas.posY == posY && cas.posX == posX - 1) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY && cas.posX == posX - 2) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY + 1 && cas.posX == posX - 2) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
            if ((cas.posY == posY - 1 && cas.posX == posX - 2) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
          }
          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }

              }

            }

          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);

        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 9) //Target Especial 9: Pirámide invertida
        {
          // Casilla de origen (punta de la pirámide)
          casAlre.Add(this);

          // 3 casillas en la columna siguiente (posX + 1)
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (cas.lado == lado && cas.posX == posX - 1 &&
              (cas.posY == posY - 1 || cas.posY == posY || cas.posY == posY + 1))
            {
              casAlre.Add(cas);
            }
          }

          // 5 casillas en la éltima columna (posX + 2)
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (cas.lado == lado && cas.posX == posX - 2 &&
              (cas.posY == posY - 2 || cas.posY == posY - 1 || cas.posY == posY || cas.posY == posY + 1 || cas.posY == posY + 2))
            {
              casAlre.Add(cas);
            }
          }

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
            }
          }
          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }
        else if (BattleManager.Instance.HabilidadActiva.targetEspecial == 10) //Target Especial 10: V atras (dos diagonales)
        {
          foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
          {
            if (((cas.posY == posY + 1 && cas.posX == posX - 1) || (cas.posY == posY - 1 && cas.posX == posX - 1)) && (cas.lado == lado))
            {
              casAlre.Add(cas);
            }
          }

          foreach (Casilla cas in casAlre)
          {
            if (cas.Presente != null)
            {
              if (!BattleManager.Instance.HabilidadActiva.bAfectaObstaculos)
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  continue;
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
              else
              {
                if (cas.Presente.GetComponent<Obstaculo>() != null)
                {
                  obstaculosEnCasAzul.Add(cas.Presente.GetComponent<Obstaculo>());
                }
                if (cas.Presente.GetComponent<Unidad>() != null)
                {
                  unidadesEnCasAzul.Add(cas.Presente.GetComponent<Unidad>());
                }
              }
            }
          }

          MarcarCasillasAzul(BattleManager.Instance.HabilidadActiva.lCasillasafectadas);
        }

        //---
        if (Presente != null)
        {
          if (Presente.GetComponent<Unidad>() != null)
          {
            unidadesEnCasAzul.Add(Presente.GetComponent<Unidad>());
          }
          if (Presente.GetComponent<Obstaculo>() != null)
          {
            obstaculosEnCasAzul.Add(Presente.GetComponent<Obstaculo>());
          }
        }



      }

    ActualizarFadeHoverObjetivosHabilidad();
  }

  public void OnMouseExit()
  {
    SetHoverVistaTactica(false);
    hoverMovimientoValido = false;
    RestablecerGlowMovimientoHover();
    RestablecerPulsoObjetivoHabilidad();
    OcultarPreviewCostoMovimiento();
    ResetearPreviewCostoMovimientoUI();
    if (TryGetUnidadActiva(out Unidad unidadActiva) && unidadActiva.CasillaPosicion != null)
    {
      unidadActiva.CasillaPosicion.DesactivarSenialadores();
    }

    ObtenerTooltipBatallaGeneral()?.HideTooltipSinAnim();
    TooltipBatalla.InstanceCostoMovimiento?.HideTooltipSinAnim();
    BattleManager.Instance?.LimpiarFadeHoverObjetivoHabilidad();
    BattleManager.Instance?.LimpiarPreviewHoverHostil();
    if (BattleManager.Instance.HabilidadActiva != null)
    {
      if (BattleManager.Instance.HabilidadActiva.enArea > 0 && BattleManager.Instance.SeleccionandoObjetivo)
      { DesmarcarCasillasAlreAzul(); DesactivarCapaColorAzul(); }
      else if (BattleManager.Instance.HabilidadActiva.targetEspecial > 0)
      {
        DesmarcarCasillasAlreAzul(); DesactivarCapaColorAzul();
      }

    }
  }

  private void ActualizarFadeHoverObjetivosHabilidad()
  {
    if (BattleManager.Instance == null
      || !BattleManager.Instance.SeleccionandoObjetivo
      || BattleManager.Instance.HabilidadActiva == null)
    {
      BattleManager.Instance?.LimpiarFadeHoverObjetivoHabilidad();
      return;
    }

    if (!EsObjetivoPosibleHabilidadActiva())
    {
      BattleManager.Instance.LimpiarFadeHoverObjetivoHabilidad();
      return;
    }

    HashSet<Unidad> unidadesMantenerVisibles = new HashSet<Unidad>();
    if (Presente != null)
    {
      Unidad unidadCentral = Presente.GetComponent<Unidad>();
      if (unidadCentral != null)
      {
        unidadesMantenerVisibles.Add(unidadCentral);
      }
    }

    foreach (Unidad unidad in unidadesEnCasAzul)
    {
      if (unidad != null)
      {
        unidadesMantenerVisibles.Add(unidad);
      }
    }

    BattleManager.Instance.AplicarFadeHoverObjetivoHabilidad(unidadesMantenerVisibles);
  }

  private void MarcarCasillasAzul(List<Casilla> casillasZonahab)
  {
    if (casillasZonahab.Contains(this)) //Marca casilla actual si esta en la zona de la habilidad
    {
      ActivarCapaColorAzul();
    }


    foreach (Casilla cas in casAlre) //Marca casillas alrededor si la central está en la zona de la habilidad
    {
      if (casillasZonahab.Contains(this))
      {
        cas.ActivarCapaColorAzul();
      }
    }
  }

  private void DesmarcarCasillasAlreAzul()
  {
    foreach (Casilla cas in casAlre)
    {
      cas.DesactivarCapaColorAzul();
    }
  }

  private void DesmarcarTodasLasCasillasAzules()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.lCasillasTotal == null)
    {
      return;
    }

    foreach (Casilla cas in BattleManager.Instance.lCasillasTotal)
    {
      if (cas != null)
      {
        cas.DesactivarCapaColorAzul();
      }
    }
  }

  public void CalcularDistanciaACasilla(Casilla casObjetivo, out int yVert, out int xHor, out bool mismoLado)
  {


    if (lado == casObjetivo.lado) //Casillas del mismo lado
    {
      mismoLado = true;
      xHor = posX - casObjetivo.posX; //Si es positiva la diferencia, quiere decir que queda a esa distancia hacia afuera, negativo, hacia centro.
    }
    else
    {
      mismoLado = false;
      xHor = 7 - posX - casObjetivo.posX; //La diferencia siempre va a dar positiva al estar del otro lado

    }


    yVert = (posY - casObjetivo.posY); //Si es positiva la diferencia, quiere decir que queda a esa distancia hacia arriba, negativo, hacia abajo.

    if (Presente != null)
    {

    }
  }

  public void NuevoObjetoPresenteEnCasilla(GameObject obj)
  {
    Unidad unidad = obj.GetComponent<Unidad>();

    if (unidad != null)
    {

      Presente = obj;
      unidad.CasillaPosicion = this;

      // Aplicar handicap de dificultad (invisible en UI) al aparecer una unidad en esta casilla
      try
      {
        var hd = Sistema.HandicapDificultad.Instance;
        if (hd != null)
        {
          hd.AplicarSiCorresponde(unidad, lado);
        }
      }
      catch { }

      //------------- TRIGGER DE TRAMPAS
      Trampa[] trampas = gameObject.GetComponents<Trampa>();
      foreach (Trampa scTramp in trampas)
      {
        if (scTramp == null)
        {
          continue;
        }

        Unidad scUnidad = unidad;

        // Si la unidad es inmune y la trampa no es favorable, no aplica efectos.
        if (scUnidad != null && scUnidad.inmunidad_Trampas && !scTramp.esTrampaFavorable)
        {
          continue;
        }

        bool seEvadeEfecto = false;

        if (obj.GetComponent<REPRESENTACIONPasoCauteloso>() != null)
        {
          if (!obj.GetComponent<REPRESENTACIONPasoCauteloso>().seusoEsteTurno && !scTramp.esTrampaFavorable )
          {
            obj.GetComponent<REPRESENTACIONPasoCauteloso>().seusoEsteTurno = true;
            seEvadeEfecto = true;
          }
        }

        if ((!seEvadeEfecto) /*|| (scTramp.esTrampaFavorable)*/)
        {
          scTramp.AplicarEfectosTrampa(scUnidad);
        }
      }


    }
    else if (obj.GetComponent<Obstaculo>() != null)
    {

      Presente = obj;
      //------------



    }

    string sortingLayerCanvas = unidad != null ? "UI3D" : null;
    RenderOrderHelper.AplicarOrdenPorY(obj, posY, sortingLayerCanvas);


  }

  public GameObject resaltadorBordeActivo;
  public void ResaltarCasillaActiva(bool res)
  {
    resaltadorBordeActivo.SetActive(res);
  }

  public GameObject Borde;
  public GameObject Sombra;
  public GameObject Actual;
  public GameObject Mover;
  public GameObject MoverCostoso;
  public GameObject Desplazable;
  public GameObject MarcaMelee;
  public GameObject OcupadoNegro;

  private static void SetActiveIfChanged(GameObject objeto, bool activo)
  {
    if (objeto != null && objeto.activeSelf != activo)
    {
      objeto.SetActive(activo);
    }
  }

  private bool DebeMostrarBordeActual()
  {
    Unidad unidadPresente = Presente != null ? Presente.GetComponent<Unidad>() : null;
    return DebeMostrarBordeActual(unidadPresente);
  }

  private bool DebeMostrarBordeActual(Unidad unidadPresente)
  {
    if (Actual == null || Presente == null || BattleManager.Instance == null)
    {
      return false;
    }

    Unidad unidadEnTurno = BattleManager.Instance.unidadActiva;
    return unidadPresente != null
      && unidadEnTurno != null
      && unidadEnTurno == unidadPresente
      && unidadEnTurno.CasillaPosicion == this
      && unidadEnTurno.gameObject.activeInHierarchy
      && unidadEnTurno.HP_actual > 0f;
  }

  void Update()
  {
    ActualizarFadeMarcasMovimiento();

    if (Borde != null)
    {
      Unidad unidadPresente = null;
      Obstaculo obstaculoPresente = null;
      int movible = 0;

      if (Presente != null)
      {
        obstaculoPresente = Presente.GetComponent<Obstaculo>();
        if (obstaculoPresente == null)
        {
          unidadPresente = Presente.GetComponent<Unidad>();
          if (unidadPresente != null)
          {
            movible = esMovible(unidadPresente);
          }
        }

        if (obstaculoPresente != null)
        {
          SetActiveIfChanged(Borde, false);
          SetActiveIfChanged(Actual, false);
          SetActiveIfChanged(Sombra, false);
          SetActiveIfChanged(Desplazable, false);
          SetActiveIfChanged(OcupadoNegro, true);

        }
        else if (unidadPresente != null && movible >= 10)
        {
          SetActiveIfChanged(Mover, false);
          SetActiveIfChanged(MoverCostoso, false);
          SetActiveIfChanged(Borde, false);
          SetActiveIfChanged(Actual, false);
          SetActiveIfChanged(OcupadoNegro, true);
          SetActiveIfChanged(Desplazable, true);
        }
        else
        {
          SetActiveIfChanged(Borde, false);
         
          if (Sombra != null)
          {
            SetActiveIfChanged(Sombra, true);
            SetActiveIfChanged(Desplazable, false);
            if (unidadPresente != null)
            { SetActiveIfChanged(OcupadoNegro, true); }
            else
            { SetActiveIfChanged(OcupadoNegro, false); }
            SetActiveIfChanged(Actual, DebeMostrarBordeActual(unidadPresente));

          }
        }
      }
      else
      {
        movible = esMovible(null);

        SetActiveIfChanged(Actual, false);
        if (Sombra != null)
        {
          SetActiveIfChanged(Sombra, false);
        }
        SetActiveIfChanged(Mover, false);
        SetActiveIfChanged(MoverCostoso, false);
        SetActiveIfChanged(Borde, false);
        SetActiveIfChanged(Desplazable, false);
        SetActiveIfChanged(OcupadoNegro, false);



        if (movible == 1)
        {
          SetActiveIfChanged(Mover, true);
        }
        else if (movible > 1 && movible < 10)
        {
          SetActiveIfChanged(MoverCostoso, true);
        }
        else if (movible >= 10)
        {

          SetActiveIfChanged(Desplazable, true);
        }
        else
        {
          if (gameObject.GetComponent<Trampa>() == null)
          { SetActiveIfChanged(Borde, true); SetActiveIfChanged(Desplazable, false); }
        }
      }
    }

    ActualizarGlowMovimientoHover();
    ActualizarPulsoObjetivoHabilidad();
    ActualizarVisualTurnoActualEnemigo();
  }

  int esMovible()
  {
    Unidad unidadPresente = Presente != null ? Presente.GetComponent<Unidad>() : null;
    return esMovible(unidadPresente);
  }

  int esMovible(Unidad unidadPresente)
  {
    int res = 0;
    if (lado == 1) { return 0; } //Solo para aliados
    BattleManager battleManager = BattleManager.Instance;
    if (battleManager == null || battleManager.bOcupado) { return 0; }

    //Unidad seleccionada - Movimiento
    Unidad unidad = battleManager.unidadActiva;

    if (unidad == null)
    {
      return 0;
    }
    if (EsTurnoIA(unidad))
    {
      return 0;
    }

    bool casillaEnRangoMovimiento = battleManager.lCasillasMovimiento.Contains(this);
    if (casillaEnRangoMovimiento && Presente == null && !battleManager.bOcupado && !unidad.movimientoEnCurso && !battleManager.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      int costoMovimientoTotal = ObtenerCostoMovimientoTotal(unidad);
      if (unidad.ObtenerAPActual() < costoMovimientoTotal)
      {
        res = 0;
      }
      else
      {
        res = Mathf.Max(1, costoMovimientoTotal);

      }
    }
    else if (casillaEnRangoMovimiento && unidadPresente != null && !unidad.movimientoEnCurso && !battleManager.SeleccionandoObjetivo && unidad.estado_inmovil < 1)
    {
      if (unidadPresente != null)
      {
        if (!unidadPresente.TieneBuffNombre("Desplazado"))
        {

          res = 10; //Desplazable
        }
        else
        {
          res = 0;
        }


      }

    }
    else
    {
      res = 0;
    }


    return res;
  }


  public void activarCapaMelee(bool activar)
  {

    MarcaMelee.SetActive(activar);

  }

  private void ActualizarHoverMovimientoVisual()
  {
    hoverMovimientoValido = false;

    if (TryObtenerDireccionMovimientoHover(out Casilla origen, out int deltaX, out int deltaY))
    {
      origen.MostrarSenialadorSoloEnDireccion(deltaX, deltaY);
      hoverMovimientoValido = true;
      return;
    }

    if (TryGetUnidadActiva(out Unidad unidadActiva) && unidadActiva.CasillaPosicion != null)
    {
      unidadActiva.CasillaPosicion.DesactivarSenialadores();
    }
  }

  private void ActualizarGlowMovimientoHover()
  {
    GameObject objetivoGlow = ObtenerObjetivoGlowMovimientoHover();
    if (!hoverMovimientoValido || objetivoGlow == null)
    {
      RestablecerGlowMovimientoHover();
      return;
    }

    RestablecerGlowMovimientoHover(objetivoGlow);

    Transform objetivoTransform = objetivoGlow.transform;
    if (!escalasBaseGlowMovimiento.TryGetValue(objetivoTransform, out Vector3 escalaBase))
    {
      escalaBase = objetivoTransform.localScale;
      escalasBaseGlowMovimiento[objetivoTransform] = escalaBase;
    }

    float pulso = 0.5f + (0.5f * Mathf.Sin(Time.time * 6.8f));
    float multiplicadorEscala = Mathf.Lerp(1.015f, 1.075f, pulso);
    objetivoTransform.localScale = escalaBase * multiplicadorEscala;

    Color colorPulso = Color.Lerp(new Color(1f, 1f, 1f, 0.82f), Color.white, pulso);
    Color emissionPulso = Color.white * Mathf.Lerp(0.55f, 1.35f, pulso);

    foreach (Renderer renderer in objetivoGlow.GetComponentsInChildren<Renderer>(true))
    {
      bloqueGlowMovimiento.Clear();
      if (renderer.sharedMaterial != null)
      {
        if (renderer.sharedMaterial.HasProperty(ShaderColorId))
        {
          bloqueGlowMovimiento.SetColor(ShaderColorId, colorPulso);
        }

        if (renderer.sharedMaterial.HasProperty(ShaderBaseColorId))
        {
          bloqueGlowMovimiento.SetColor(ShaderBaseColorId, colorPulso);
        }

        if (renderer.sharedMaterial.HasProperty(ShaderEmissionColorId))
        {
          bloqueGlowMovimiento.SetColor(ShaderEmissionColorId, emissionPulso);
        }
      }

      renderer.SetPropertyBlock(bloqueGlowMovimiento);
    }
  }

  private GameObject ObtenerObjetivoGlowMovimientoHover()
  {
    if (!hoverMovimientoValido)
    {
      return null;
    }

    if (Presente == null)
    {
      if (Mover != null && Mover.activeInHierarchy)
      {
        return Mover;
      }

      if (MoverCostoso != null && MoverCostoso.activeInHierarchy)
      {
        return MoverCostoso;
      }
    }
    else if (PuedeIntercambiarConUnidadActiva() && Desplazable != null && Desplazable.activeInHierarchy)
    {
      return Desplazable;
    }

    return null;
  }

  private IEnumerable<GameObject> ObtenerRaicesGlowMovimiento()
  {
    if (Mover != null) { yield return Mover; }
    if (MoverCostoso != null) { yield return MoverCostoso; }
    if (Desplazable != null) { yield return Desplazable; }
  }

  private void RestablecerGlowMovimientoHover(GameObject excepcion = null)
  {
    foreach (GameObject raiz in ObtenerRaicesGlowMovimiento())
    {
      if (raiz == null || raiz == excepcion)
      {
        continue;
      }

      Transform raizTransform = raiz.transform;
      if (escalasBaseGlowMovimiento.TryGetValue(raizTransform, out Vector3 escalaBase))
      {
        raizTransform.localScale = escalaBase;
      }

      foreach (Renderer renderer in raiz.GetComponentsInChildren<Renderer>(true))
      {
        renderer.SetPropertyBlock(null);
      }
    }
  }

  private bool EsObjetivoPosibleHabilidadActiva()
  {
    if (BattleManager.Instance == null
      || !BattleManager.Instance.SeleccionandoObjetivo
      || BattleManager.Instance.HabilidadActiva == null
      || Presente == null)
    {
      return false;
    }

    Unidad unidadObjetivo = Presente.GetComponent<Unidad>();
    if (unidadObjetivo != null)
    {
      return BattleManager.Instance.EsUnidadObjetivoVisualHabilidadActiva(unidadObjetivo);
    }

    Obstaculo obstaculoObjetivo = Presente.GetComponent<Obstaculo>();
    if (obstaculoObjetivo != null)
    {
      return BattleManager.Instance.EsObstaculoObjetivoVisualHabilidadActiva(obstaculoObjetivo);
    }

    return false;
  }

  private bool EsObjetivoObstaculoHabilidadActiva()
  {
    if (BattleManager.Instance == null
      || !BattleManager.Instance.SeleccionandoObjetivo
      || BattleManager.Instance.HabilidadActiva == null
      || Presente == null)
    {
      return false;
    }

    Obstaculo obstaculoObjetivo = Presente.GetComponent<Obstaculo>();
    return obstaculoObjetivo != null
      && BattleManager.Instance.EsObstaculoObjetivoVisualHabilidadActiva(obstaculoObjetivo);
  }

  private void ActualizarCirculoObjetivoHabilidad()
  {
    bool seleccionandoObjetivo = BattleManager.Instance != null
      && BattleManager.Instance.SeleccionandoObjetivo
      && BattleManager.Instance.HabilidadActiva != null;

    bool mostrarCirculo = seleccionandoObjetivo && EsObjetivoPosibleHabilidadActiva();

    if (circuloBordeHabilidadRojo != null)
    {
      circuloBordeHabilidadRojo.gameObject.SetActive(!seleccionandoObjetivo || mostrarCirculo);
    }

    if (circuloBordeHabilidadAzul != null)
    {
      circuloBordeHabilidadAzul.gameObject.SetActive(!seleccionandoObjetivo || mostrarCirculo);
    }
  }

  private bool DebePulsarBordeActual()
  {
    if (BattleManager.Instance == null
      || BattleManager.Instance.HabilidadActiva == null
      || BattleManager.Instance.unidadActiva == null
      || BattleManager.Instance.unidadActiva.CasillaPosicion != this)
    {
      return false;
    }

    return EsObjetivoPosibleHabilidadActiva()
      && !BattleManager.Instance.HabilidadActiva.esHostil
      && graficoBordeActual != null
      && graficoBordeActual.gameObject.activeInHierarchy;
  }

  private Transform ObtenerCapaObjetivoHabilidadTransform()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.HabilidadActiva == null)
    {
      return null;
    }

    Transform capaAzul = graficoBordeHabilidadAzul != null ? graficoBordeHabilidadAzul : (transform.childCount > 0 ? transform.GetChild(0) : null);
    Transform capaRoja = graficoBordeHabilidadRojo != null ? graficoBordeHabilidadRojo : (transform.childCount > 1 ? transform.GetChild(1) : null);

    if (capaRoja != null && capaRoja.gameObject.activeInHierarchy)
    {
      autoActivoCapaObjetivoHabilidad = false;
      return capaRoja;
    }

    if (capaAzul != null && capaAzul.gameObject.activeInHierarchy)
    {
      autoActivoCapaObjetivoHabilidad = false;
      return capaAzul;
    }

    Transform capaPreferida = BattleManager.Instance.HabilidadActiva.esHostil ? capaRoja : capaAzul;
    if (capaPreferida != null && EsObjetivoPosibleHabilidadActiva())
    {
      capaPreferida.gameObject.SetActive(true);
      autoActivoCapaObjetivoHabilidad = true;
      return capaPreferida;
    }

    autoActivoCapaObjetivoHabilidad = false;
    return capaPreferida;
  }

  private void ActualizarPulsoObjetivoHabilidad()
  {
    ActualizarCirculoObjetivoHabilidad();

    Transform capaObjetivo = ObtenerCapaObjetivoHabilidadTransform();
    if (capaObjetivo == null)
    {
      return;
    }

    if (!capaObjetivo.gameObject.activeInHierarchy || !EsObjetivoPosibleHabilidadActiva())
    {
      RestablecerPulsoObjetivoHabilidad();
      return;
    }

    if (!escalaBaseCapaObjetivoHabilidad.HasValue)
    {
      escalaBaseCapaObjetivoHabilidad = capaObjetivo.localScale;
    }

    if (EsObjetivoObstaculoHabilidadActiva())
    {
      capaObjetivo.localScale = escalaBaseCapaObjetivoHabilidad.Value;

      if (graficoBordeActual != null)
      {
        if (!escalaBaseBordeActualObjetivoHabilidad.HasValue)
        {
          escalaBaseBordeActualObjetivoHabilidad = graficoBordeActual.localScale;
        }

        graficoBordeActual.localScale = escalaBaseBordeActualObjetivoHabilidad.Value;
      }

      return;
    }

    float pulso = 0.5f + (0.5f * Mathf.Sin(Time.time * velocidadPulsoObjetivoHabilidad));
    float multiplicadorEscala = Mathf.Lerp(1f, 1f + intensidadPulsoObjetivoHabilidad, pulso);
    capaObjetivo.localScale = escalaBaseCapaObjetivoHabilidad.Value * multiplicadorEscala;

    if (DebePulsarBordeActual())
    {
      if (!escalaBaseBordeActualObjetivoHabilidad.HasValue)
      {
        escalaBaseBordeActualObjetivoHabilidad = graficoBordeActual.localScale;
      }

      graficoBordeActual.localScale = escalaBaseBordeActualObjetivoHabilidad.Value * multiplicadorEscala;
    }
    else if (graficoBordeActual != null && escalaBaseBordeActualObjetivoHabilidad.HasValue)
    {
      graficoBordeActual.localScale = escalaBaseBordeActualObjetivoHabilidad.Value;
    }
  }

  private void RestablecerPulsoObjetivoHabilidad()
  {
    Transform capaAzul = graficoBordeHabilidadAzul != null ? graficoBordeHabilidadAzul : (transform.childCount > 0 ? transform.GetChild(0) : null);
    Transform capaRoja = graficoBordeHabilidadRojo != null ? graficoBordeHabilidadRojo : (transform.childCount > 1 ? transform.GetChild(1) : null);
    Transform capaObjetivo = null;
    if (capaRoja != null && capaRoja.gameObject.activeInHierarchy)
    {
      capaObjetivo = capaRoja;
    }
    else if (capaAzul != null && capaAzul.gameObject.activeInHierarchy)
    {
      capaObjetivo = capaAzul;
    }

    if (capaObjetivo != null && escalaBaseCapaObjetivoHabilidad.HasValue)
    {
      capaObjetivo.localScale = escalaBaseCapaObjetivoHabilidad.Value;
    }

    if (graficoBordeActual != null && escalaBaseBordeActualObjetivoHabilidad.HasValue)
    {
      graficoBordeActual.localScale = escalaBaseBordeActualObjetivoHabilidad.Value;
    }

    if (autoActivoCapaObjetivoHabilidad)
    {
      Transform capaAuto = (BattleManager.Instance != null && BattleManager.Instance.HabilidadActiva != null && BattleManager.Instance.HabilidadActiva.esHostil)
        ? capaRoja
        : capaAzul;
      if (capaAuto != null)
      {
        capaAuto.gameObject.SetActive(false);
      }
    }

    autoActivoCapaObjetivoHabilidad = false;
    ActualizarCirculoObjetivoHabilidad();
  }

  private void ActualizarVisualTurnoActualEnemigo()
  {
    if (renderersBordeActual == null || renderersBordeActual.Length == 0)
    {
      return;
    }

    bool esTurnoEnemigoActual = Actual != null
      && Actual.activeInHierarchy
      && BattleManager.Instance != null
      && BattleManager.Instance.unidadActiva != null
      && BattleManager.Instance.unidadActiva.CasillaPosicion == this
      && BattleManager.Instance.unidadActiva.CasillaPosicion != null
      && BattleManager.Instance.unidadActiva.CasillaPosicion.lado == 1;

    if (!esTurnoEnemigoActual)
    {
      foreach (Renderer renderer in renderersBordeActual)
      {
        if (renderer != null)
        {
          renderer.SetPropertyBlock(null);
        }
      }

      return;
    }

    float pulso = 0.8f + (0.8f * Mathf.Sin(Time.time * 5.4f));
    Color colorPulso = Color.Lerp(new Color(0.92f, 0.04f, 0.04f, 1f), new Color(0.95f, 0.05f, 0.05f, 1f), pulso);
    Color emissionPulso = Color.Lerp(new Color(1.1f, 0.02f, 0.02f, 1f), new Color(1.4f, 0.06f, 0.06f, 1f), pulso);

    foreach (Renderer renderer in renderersBordeActual)
    {
      if (renderer == null || renderer.sharedMaterial == null)
      {
        continue;
      }

      bloqueTurnoActualEnemigo.Clear();
      if (renderer.sharedMaterial.HasProperty(ShaderColorId))
      {
        bloqueTurnoActualEnemigo.SetColor(ShaderColorId, colorPulso);
      }

      if (renderer.sharedMaterial.HasProperty(ShaderBaseColorId))
      {
        bloqueTurnoActualEnemigo.SetColor(ShaderBaseColorId, colorPulso);
      }

      if (renderer.sharedMaterial.HasProperty(ShaderEmissionColorId))
      {
        bloqueTurnoActualEnemigo.SetColor(ShaderEmissionColorId, emissionPulso);
      }

      renderer.SetPropertyBlock(bloqueTurnoActualEnemigo);
    }
  }

  void OnDisable()
  {
    estaEnDestruccion = true;
    hoverMovimientoValido = false;
    RestablecerGlowMovimientoHover();
    RestablecerPulsoObjetivoHabilidad();
    OcultarPreviewCostoMovimiento();
    ResetearPreviewCostoMovimientoUI();
    ActualizarVisualTurnoActualEnemigo();
  }

  void OnDestroy()
  {
    estaEnDestruccion = true;
    hoverMovimientoValido = false;
    RestablecerGlowMovimientoHover();
    RestablecerPulsoObjetivoHabilidad();
    OcultarPreviewCostoMovimiento();
    ResetearPreviewCostoMovimientoUI();

    foreach (EstadoMarcaMovimiento estado in estadosMarcaMovimiento.Values)
    {
      if (estado?.materialesInstancia == null)
      {
        continue;
      }

      foreach (Material materialInstancia in estado.materialesInstancia)
      {
        if (materialInstancia != null)
        {
          Destroy(materialInstancia);
        }
      }
    }
  }

}
