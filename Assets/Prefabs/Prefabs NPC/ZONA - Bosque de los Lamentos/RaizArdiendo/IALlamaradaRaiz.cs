using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IALlamaradaRaiz : IAHabilidad
{
  [SerializeField] Transform puntoDisparo;

  const int TipoDanioFuego = 4;
  const int TipoDanioNecrotico = 9;
  const int DadosCantidad = 2;
  const int DadosCaras = 6;
  const int BonificacionDanio = 3;
 
  const float DuracionLinea = 0.32f;
  const float AnchoLinea = 0.04f;
  const int SegmentosLinea = 6;

  static Material materialLlamarada;
  static AnimationCurve curvaAnchoLlamarada;
  static readonly GradientColorKey[] colorKeysLlamarada = new GradientColorKey[]
  {
    new GradientColorKey(new Color(1.8f, 1.08f, 0.5f), 0.12f),
    new GradientColorKey(new Color(0.78f, 1.22f, 0.62f), 0.34f),
    new GradientColorKey(new Color(1.5f, 0.36f, 0.12f), 0.58f),
    new GradientColorKey(new Color(0.56f, 1.05f, 0.48f), 0.82f),
    new GradientColorKey(new Color(0.95f, 0.24f, 0.08f), 1f)
  };
  static readonly GradientAlphaKey[] alphaKeysBaseLlamarada = new GradientAlphaKey[]
  {
    new GradientAlphaKey(1f, 0f),
    new GradientAlphaKey(0.8f, 0.35f),
    new GradientAlphaKey(0.35f, 1f)
  };

  void Awake()
  {
    nombre = "Llamarada Raiz";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 2;
    esMelee = false;
    hAlcance = 5;
    hCooldownMax = 0;
    esHostil = true;
    prioridad = 0;
    costoAP = 3;
    afectaObstaculos = false;

    hActualCooldown = ObtenerCooldownInicialReducido();
  }

  void Start()
  {
    prioridad = 0;
  }

  int ObtenerCooldownInicialReducido()
  {
    if (hCooldownMax <= 1)
    {
      return 0;
    }

    return UnityEngine.Random.Range(0, hCooldownMax);
  }

  public async override Task ActivarHabilidad()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = Usuario.GetComponent<Unidad>();
    }

    object objetivo = EstablecerObjetivoPrioritario();
    if (objetivo == null)
    {
      return;
    }
    hActualCooldown = hCooldownMax;
    scEstaUnidad.CambiarAPActual(-costoAP);
   // scEstaUnidad.ReproducirAnimacionAtaque();

    PrepararInicioAnimacion(null, objetivo);

    await BattleManager.DelayCombateAsync(450);

    AplicarEfectosHabilidad(objetivo);

    await BattleManager.DelayCombateAsync(250);
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad unidadObjetivo))
    {
      return;
    }

    DibujarRayo(unidadObjetivo);

    float danioTotal = TiradaDeDados.TirarDados(DadosCantidad, DadosCaras) + BonificacionDanio;
    danioTotal = danioTotal / 100f * (100 + scEstaUnidad.mod_DanioPorcentaje);

    float danioFuego = Mathf.Ceil(danioTotal * 0.5f);
    //float danioNecrotico = Mathf.Max(1f, danioTotal - danioFuego);

    unidadObjetivo.RecibirDanio(danioFuego, TipoDanioFuego, false, scEstaUnidad);
   

    bool noSeSalva = unidadObjetivo.TiradaSalvacion(2, 10);
    if (noSeSalva)
    {
      Estados.Aplicar_Ardiendo(unidadObjetivo, 2);
    }

    unidadObjetivo.AplicarDebuffPorAtaquesreiterados(1);

    BattleManager.Instance?.EscribirLog(
      $"{scEstaUnidad.uNombre} {TRADU.i.Traducir("desata una llamarada necrotica y ardiente sobre")} {unidadObjetivo.uNombre}.");
  }

  public override object EstablecerObjetivoPrioritario()
  {
    Unidad unidadPropia = scEstaUnidad ?? gameObject.GetComponent<Unidad>();
    if (unidadPropia == null)
    {
      return null;
    }

    List<Unidad> unidades = objPosibles.OfType<Unidad>()
      .OrderBy(u => u.CasillaPosicion.posX)
      .ThenBy(u => Mathf.Abs(u.CasillaPosicion.posY - unidadPropia.CasillaPosicion.posY))
      .ToList();

    return unidades.FirstOrDefault();
  }

  void DibujarRayo(Unidad objetivo)
  {
    Vector3 origen = scEstaUnidad.puntoSaliente.transform.position;

    Vector3 destino = objetivo.puntoEntrante.transform.position;

    GameObject lineaGO = new GameObject("VFX_LlamaradaRaiz");
    if (BattleManager.Instance != null)
    {
      lineaGO.transform.SetParent(BattleManager.Instance.transform, true);
    }

    LineRenderer lineRenderer = lineaGO.AddComponent<LineRenderer>();
    lineRenderer.useWorldSpace = true;
    lineRenderer.alignment = LineAlignment.View;
    lineRenderer.material = ObtenerMaterialLlamarada();
    lineRenderer.widthCurve = ObtenerCurvaAnchoLlamarada();
    lineRenderer.widthMultiplier = AnchoLinea;
    lineRenderer.positionCount = SegmentosLinea;
    lineRenderer.textureMode = LineTextureMode.Stretch;
    lineRenderer.numCapVertices = 6;
    lineRenderer.numCornerVertices = 4;
    lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    lineRenderer.receiveShadows = false;
    lineRenderer.colorGradient = CrearGradienteLlamarada(1f);

    StartCoroutine(AnimarLinea(lineRenderer, origen, destino));
  }

  IEnumerator AnimarLinea(LineRenderer lineRenderer, Vector3 origen, Vector3 destino)
  {
    float tiempo = 0f;

    while (tiempo < DuracionLinea)
    {
      tiempo += Time.deltaTime;
      float progreso = Mathf.Clamp01(tiempo / DuracionLinea);
      float alpha = Mathf.Lerp(1.2f, 0.08f, progreso);

      Vector3[] puntos = GenerarPuntos(origen, destino);
      lineRenderer.positionCount = puntos.Length;
      lineRenderer.SetPositions(puntos);

      lineRenderer.colorGradient = CrearGradienteLlamarada(alpha);
      lineRenderer.widthMultiplier = Mathf.Lerp(AnchoLinea, AnchoLinea * 0.35f, progreso);

      yield return null;
    }

    if (lineRenderer != null)
    {
      Destroy(lineRenderer.gameObject);
    }
  }

  static Material ObtenerMaterialLlamarada()
  {
    if (materialLlamarada == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }

      materialLlamarada = new Material(shader);
      materialLlamarada.name = "Mat_LlamaradaRaiz";

      if (materialLlamarada.HasProperty("_TintColor"))
      {
        materialLlamarada.SetColor("_TintColor", new Color(1.1f, 0.92f, 0.42f, 1f));
      }

      if (materialLlamarada.HasProperty("_Color"))
      {
        materialLlamarada.SetColor("_Color", new Color(1.1f, 0.92f, 0.42f, 1f));
      }

      if (materialLlamarada.HasProperty("_EmissionColor"))
      {
        materialLlamarada.EnableKeyword("_EMISSION");
        materialLlamarada.SetColor("_EmissionColor", new Color(0.95f, 1.15f, 0.42f) * 6.5f);
      }
    }
    return materialLlamarada;
  }

  static AnimationCurve ObtenerCurvaAnchoLlamarada()
  {
    if (curvaAnchoLlamarada == null)
    {
      curvaAnchoLlamarada = new AnimationCurve(
        new Keyframe(0f, 0.75f, 0f, 0f),
        new Keyframe(0.35f, 1.1f, 0f, 0f),
        new Keyframe(0.7f, 0.65f, 0f, 0f),
        new Keyframe(1f, 0.25f, 0f, 0f));
    }

    return curvaAnchoLlamarada;
  }

  static Gradient CrearGradienteLlamarada(float alphaFactor)
  {
    Gradient gradiente = new Gradient();
    GradientAlphaKey[] alphaKeysEscalados = new GradientAlphaKey[alphaKeysBaseLlamarada.Length];
    for (int i = 0; i < alphaKeysBaseLlamarada.Length; i++)
    {
      alphaKeysEscalados[i] = new GradientAlphaKey(alphaKeysBaseLlamarada[i].alpha * alphaFactor, alphaKeysBaseLlamarada[i].time);
    }

    gradiente.SetKeys(colorKeysLlamarada, alphaKeysEscalados);
    return gradiente;
  }

  Vector3[] GenerarPuntos(Vector3 origen, Vector3 destino)
  {
    Vector3[] puntos = new Vector3[SegmentosLinea];

    Vector3 direccion = destino - origen;
    Vector3 direccionNormalizada = direccion.normalized;
    Vector3 normal = Vector3.Cross(direccionNormalizada, Vector3.up);
    if (normal.sqrMagnitude < 0.001f)
    {
      normal = Vector3.Cross(direccionNormalizada, Vector3.right);
    }
    normal.Normalize();
    Vector3 binormal = Vector3.Cross(direccionNormalizada, normal).normalized;

    for (int i = 0; i < SegmentosLinea; i++)
    {
      float t = SegmentosLinea == 1 ? 0f : i / (float)(SegmentosLinea - 1);
      Vector3 basePoint = Vector3.Lerp(origen, destino, t);
      float intensidad = Mathf.Sin(t * Mathf.PI);
      Vector3 offset = (normal * 0.03f * intensidad) + (binormal * 0.02f * intensidad);
      offset += Vector3.up * 0.03f * intensidad;

      puntos[i] = basePoint + offset;
    }

    return puntos;
  }
}
