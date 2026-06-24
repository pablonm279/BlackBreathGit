using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IARayoZarkil : IAHabilidad
{
  [SerializeField] Transform puntoDisparo;

  const int TipoDanioNecrotico = 10;
  const int DadosCantidad = 2;
  const int DadosCaras = 4;
 
  const float DuracionLinea = 0.38f;
  const float AnchoLinea = 0.045f;
  const int SegmentosLinea = 9;
  const float DesvioMaximo = 0.1f;

  static Material materialRayo;
  static AnimationCurve curvaAncho;

  public AudioClip rayosonido;

  void Awake()
  {
    nombre = "Rayo Debilitador";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 2;
    esMelee = false;
    hAlcance = 4;
    hCooldownMax = 0;
    esHostil = true;
    prioridad = 0;
    costoAP = 3;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 0;
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
    scEstaUnidad.CambiarAPActual(-costoAP);
    //scEstaUnidad.ReproducirAnimacionAtaque();

    PrepararInicioAnimacion(null, objetivo);


    await BattleManager.DelayCombateAsync(450);
   scEstaUnidad.GetComponent<AudioSource>().PlayOneShot(rayosonido);

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

    float danio = TiradaDeDados.TirarDados(DadosCantidad, DadosCaras);
    danio = danio / 100f * (100 + scEstaUnidad.mod_DanioPorcentaje);

    unidadObjetivo.RecibirDanio(danio, TipoDanioNecrotico, false, scEstaUnidad);
    unidadObjetivo.AplicarDebuffPorAtaquesreiterados(1);

    if(unidadObjetivo.TiradaSalvacion(1, 12))
    { 
      // BUFF ---- As� se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Debilitado";
      buff.boolfDebufftBuff = false;
      buff.DuracionBuffRondas = 2;
      buff.cantDanioPorcentaje -= 15;
      buff.cantAtaque -= 2;
      buff.AplicarBuff(unidadObjetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuraci�n del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, unidadObjetivo.gameObject);

    }

    BattleManager.Instance?.EscribirLog(
      $"{scEstaUnidad.uNombre} {TRADU.i.Traducir("desata un rayo debilitador sobre")} {unidadObjetivo.uNombre}.");
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

    GameObject lineaGO = new GameObject("VFX_RayoNecrotico");
    if (BattleManager.Instance != null)
    {
      lineaGO.transform.SetParent(BattleManager.Instance.transform, true);
    }

    LineRenderer lineRenderer = lineaGO.AddComponent<LineRenderer>();
    lineRenderer.useWorldSpace = true;
    lineRenderer.alignment = LineAlignment.View;
    lineRenderer.material = ObtenerMaterial();
    lineRenderer.widthCurve = ObtenerCurvaAncho();
    lineRenderer.widthMultiplier = AnchoLinea;
    lineRenderer.positionCount = SegmentosLinea;
    lineRenderer.textureMode = LineTextureMode.Stretch;
    lineRenderer.numCapVertices = 6;
    lineRenderer.numCornerVertices = 4;
    lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    lineRenderer.receiveShadows = false;

    StartCoroutine(AnimarLinea(lineRenderer, origen, destino));
  }

  IEnumerator AnimarLinea(LineRenderer lineRenderer, Vector3 origen, Vector3 destino)
  {
    float tiempo = 0f;

    while (tiempo < DuracionLinea)
    {
      tiempo += Time.deltaTime;
      float progreso = Mathf.Clamp01(tiempo / DuracionLinea);
      float alpha = Mathf.Lerp(1.25f, 0.2f, progreso);

      Vector3[] puntos = GenerarPuntos(origen, destino);
      lineRenderer.positionCount = puntos.Length;
      lineRenderer.SetPositions(puntos);

      Color inicio = new Color(1.55f, 0.25f, 0.25f, alpha);
      Color fin = new Color(0.45f, 0.2f, 0.2f, alpha * 0.8f);
      lineRenderer.startColor = inicio;
      lineRenderer.endColor = fin;

      yield return null;
    }

    if (lineRenderer != null)
    {
      Destroy(lineRenderer.gameObject);
    }
  }

  static Material ObtenerMaterial()
  {
    if (materialRayo == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }

      materialRayo = new Material(shader);
      materialRayo.name = "Mat_RayoNecrotico";

      if (materialRayo.HasProperty("_TintColor"))
      {
        materialRayo.SetColor("_TintColor", new Color(1.55f, 0.2f, 0.3f, 0.85f));
      }

      if (materialRayo.HasProperty("_EmissionColor"))
      {
        materialRayo.EnableKeyword("_EMISSION");
        materialRayo.SetColor("_EmissionColor", new Color(0.7f, 0.1f, 0.25f) * 3f);
      }
    }
    return materialRayo;
  }

  static AnimationCurve ObtenerCurvaAncho()
  {
    if (curvaAncho == null)
    {
      curvaAncho = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, 0f),
        new Keyframe(0.2f, 1.1f, 0f, 0f),
        new Keyframe(0.7f, 0.8f, 0f, 0f),
        new Keyframe(1f, 0.3f, 0f, 0f));
    }

    return curvaAncho;
  }

  Vector3[] GenerarPuntos(Vector3 origen, Vector3 destino)
  {
    Vector3[] puntos = new Vector3[SegmentosLinea];

    Vector3 direccion = destino - origen;
    Vector3 normal = Vector3.Cross(direccion.normalized, Vector3.up);
    if (normal.sqrMagnitude < 0.001f)
    {
      normal = Vector3.Cross(direccion.normalized, Vector3.right);
    }
    normal.Normalize();
    Vector3 binormal = Vector3.Cross(direccion.normalized, normal).normalized;

    for (int i = 0; i < SegmentosLinea; i++)
    {
      float t = SegmentosLinea == 1 ? 0f : i / (float)(SegmentosLinea - 1);
      Vector3 basePoint = Vector3.Lerp(origen, destino, t);
      float intensidad = Mathf.Sin(t * Mathf.PI) * DesvioMaximo;

      Vector3 offset = normal * UnityEngine.Random.Range(-intensidad, intensidad);
      offset += binormal * UnityEngine.Random.Range(-intensidad, intensidad) * 0.6f;
      offset += Vector3.up * UnityEngine.Random.Range(-intensidad, intensidad) * 0.35f;

      puntos[i] = basePoint + offset;
    }

    return puntos;
  }
}



