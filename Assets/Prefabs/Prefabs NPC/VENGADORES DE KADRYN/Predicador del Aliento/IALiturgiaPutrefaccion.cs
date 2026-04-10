using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IALiturgiaPutrefaccion : IAHabilidad
{
  const int TipoDanioNecrotico = 9;
  const int DadosCantidad = 1;
  const int DadosCaras = 2;
  const int DificultadSalvacion = 15;
  const int ReduccionDefensa = 2;
  const int ReduccionArmadura = 2;
  const int ReduccionResistenciaNecro = 15;
  const int DuracionPutrefaccion = 2;

  const float DuracionHalo = 2.45f;
  const float AnchoHalo = 0.01f;
  const int SegmentosHalo = 48;
  const float RadioHalo = 0.18f;
  const float AlturaHalo = 0.25f;

  static Material materialHalo;

  readonly List<LineRenderer> halosActivos = new List<LineRenderer>();

  void Awake()
  {
    nombre = "Liturgia de la Putrefacci�n";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 2;
    esMelee = false;
    hAlcance = 4;
    hCooldownMax = 3;
    esHostil = true;
    prioridad = 5;
    costoAP = 3;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 5;
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
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();
    PrepararInicioAnimacion(null, objetivo);

    await BattleManager.DelayCombateAsync(600);

    hActualCooldown = hCooldownMax;
    AplicarEfectosHabilidad(objetivo);

    await BattleManager.DelayCombateAsync(250);
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad objetivo))
    {
      return;
    }

    DibujarHalo(objetivo);

    float danio = TiradaDeDados.TirarDados(DadosCantidad, DadosCaras);
    danio = danio / 100f * (100 + scEstaUnidad.mod_DanioPorcentaje);
    objetivo.RecibirDanio(danio, TipoDanioNecrotico, false, scEstaUnidad);

    bool noSeSalva = objetivo.TiradaSalvacion(objetivo.mod_TSMental, DificultadSalvacion);
    if (!noSeSalva)
    {
      return;
    }

    Buff putrefaccion = new Buff
    {
      buffNombre = "Putrefacción",
   //   buffDescr = TRADU.i.Traducir("Sus defensas se corroen por el Aliento Negro."),
      boolfDebufftBuff = false,
      cantDefensa = -ReduccionDefensa,
      cantArmadura = -ReduccionArmadura,
      percResNec = -ReduccionResistenciaNecro,
      DuracionBuffRondas = DuracionPutrefaccion,
      esBuffVisibleUI = true,
      esRemovible = true,
      esStackeable = false
    };

    putrefaccion.AplicarBuff(objetivo);
    Buff copia = ComponentCopier.CopyComponent(putrefaccion, objetivo.gameObject);
    copia.esStackeable = false;

    BattleManager.Instance?.EscribirLog($"{objetivo.uNombre} {TRADU.i.Traducir("es presa de la Putrefacci�n.")}");
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

  void DibujarHalo(Unidad objetivo)
  {
    Vector3 destino = objetivo.puntoEntrante != null
      ? objetivo.puntoEntrante.position
      : objetivo.transform.position + Vector3.up * AlturaHalo;

    GameObject haloGO = new GameObject("VFX_LiturgiaPutrefaccion");
    if (BattleManager.Instance != null)
    {
      haloGO.transform.SetParent(BattleManager.Instance.transform, true);
    }

    LineRenderer halo = haloGO.AddComponent<LineRenderer>();
    halo.useWorldSpace = true;
    halo.alignment = LineAlignment.View;
    halo.material = ObtenerMaterial();
    halo.widthMultiplier = AnchoHalo;
    halo.loop = true;
    halo.textureMode = LineTextureMode.Stretch;
    halo.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    halo.receiveShadows = false;
    halo.numCapVertices = 0;
    halo.numCornerVertices = 4;

    Vector3 centro = destino;
    Vector3 normal = Vector3.up;
    Vector3 binormal = Vector3.right;
    Vector3 tangent = Vector3.forward;

    Vector3[] puntos = new Vector3[SegmentosHalo];
    for (int i = 0; i < SegmentosHalo; i++)
    {
      float angulo = (i / (float)SegmentosHalo) * Mathf.PI * 2f;
      float ruido = Mathf.Sin(angulo * 5f) * 0.04f;
      Vector3 offset = (Mathf.Cos(angulo) * binormal + Mathf.Sin(angulo) * tangent) * (RadioHalo + ruido);
      offset += normal * (Mathf.Sin(angulo * 6f) * 0.06f + AlturaHalo);
      puntos[i] = centro + offset;
    }

    halo.positionCount = SegmentosHalo;
    halo.SetPositions(puntos);

    halo.startColor = new Color(0.32f, 1.3f, 0.95f, 0.9f);
    halo.endColor = new Color(0.12f, 0.45f, 0.22f, 0.25f);

    halosActivos.Add(halo);
    StartCoroutine(DesvanecerHalo(halo));

    StartCoroutine(GenerarChisporroteos(centro, binormal, tangent, normal));
  }

  IEnumerator DesvanecerHalo(LineRenderer halo)
  {
    float tiempo = 0f;
    while (tiempo < DuracionHalo)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / DuracionHalo);
      if (halo != null)
      {
        Color cInicio = halo.startColor;
        Color cFin = halo.endColor;
        halo.startColor = new Color(cInicio.r, cInicio.g, cInicio.b, Mathf.Lerp(0.9f, 0f, factor));
        halo.endColor = new Color(cFin.r, cFin.g, cFin.b, Mathf.Lerp(0.4f, 0f, factor));
      }
      yield return null;
    }

    if (halo != null)
    {
      halosActivos.Remove(halo);
      Destroy(halo.gameObject);
    }
  }

  IEnumerator GenerarChisporroteos(Vector3 centro, Vector3 binormal, Vector3 tangent, Vector3 normal)
  {
    int particulas = 6;
    for (int i = 0; i < particulas; i++)
    {
      GameObject chispaGO = new GameObject("VFX_LiturgiaPutrefaccion_Chispa");
      LineRenderer chispa = chispaGO.AddComponent<LineRenderer>();
      chispa.useWorldSpace = true;
      chispa.alignment = LineAlignment.View;
      chispa.material = ObtenerMaterial();
      chispa.widthMultiplier = 0.04f;
      chispa.positionCount = 2;
      chispa.textureMode = LineTextureMode.Stretch;
      chispa.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
      chispa.receiveShadows = false;
      chispa.numCapVertices = 2;
      chispa.startColor = new Color(0.4f, 1.35f, 1f, 0.9f);
      chispa.endColor = new Color(0.15f, 0.5f, 0.25f, 0f);

      float angulo = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
      Vector3 direccion = (Mathf.Cos(angulo) * binormal + Mathf.Sin(angulo) * tangent).normalized;
      float alturaBase = AlturaHalo + UnityEngine.Random.Range(0.05f, 0.18f);
      Vector3 inicio = centro + direccion * (RadioHalo * 0.35f) + normal * alturaBase;
      Vector3 fin = inicio + direccion * UnityEngine.Random.Range(0.12f, 0.25f) + normal * UnityEngine.Random.Range(-0.04f, 0.04f);

      chispa.SetPosition(0, inicio);
      chispa.SetPosition(1, fin);

      StartCoroutine(DesvanecerChispa(chispa, UnityEngine.Random.Range(0.1f, 0.18f)));
      yield return new WaitForSeconds(0.032f);
    }
  }

  IEnumerator DesvanecerChispa(LineRenderer chispa, float duracion)
  {
    float tiempo = 0f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / duracion);
      if (chispa != null)
      {
        Color inicio = chispa.startColor;
        Color fin = chispa.endColor;
        chispa.startColor = new Color(inicio.r, inicio.g, inicio.b, Mathf.Lerp(0.8f, 0f, factor));
        chispa.endColor = new Color(fin.r, fin.g, fin.b, Mathf.Lerp(0.2f, 0f, factor));
        chispa.widthMultiplier = Mathf.Lerp(0.04f, 0f, factor);
      }
      yield return null;
    }

    if (chispa != null)
    {
      Destroy(chispa.gameObject);
    }
  }

  static Material ObtenerMaterial()
  {
    if (materialHalo == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }

      materialHalo = new Material(shader);
      materialHalo.name = "Mat_LiturgiaPutrefaccion";

      if (materialHalo.HasProperty("_TintColor"))
      {
        materialHalo.SetColor("_TintColor", new Color(0.3f, 1.3f, 0.9f, 0.8f));
      }

      if (materialHalo.HasProperty("_EmissionColor"))
      {
        materialHalo.EnableKeyword("_EMISSION");
        materialHalo.SetColor("_EmissionColor", new Color(0.15f, 0.7f, 0.4f) * 2.5f);
      }
    }

    return materialHalo;
  }
}


