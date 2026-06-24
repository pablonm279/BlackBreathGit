using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IACondenaRaices : IAHabilidad
{
  const int TipoDanioNecrotico = 9;
  const int DadosCantidad = 1;
  const int DadosCaras = 10;
  const int DificultadSalvacion = 14;
  const int TurnosCondena = 2;
  const int CantidadObjetivos = 2;

  const float DuracionAnillo = 2.1f;
  const float AnchoAnillo = 0.014f;
  const int SegmentosAnillo = 56;
  const float RadioAnillo = 0.22f;
  const float AlturaAnillo = 0.28f;

  static Material materialAnillo;

  void Awake()
  {
    nombre = "Condena del bosque";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 5;
    esMelee = false;
    hAlcance = 8; //Se ignora en ListaHayObjetivosAlAlcance, se usa alcance global
    hCooldownMax = 0;
    esHostil = true;
    prioridad = 8;
    costoAP = 4;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 8;
  }

  public async override Task ActivarHabilidad()
  {
    scEstaUnidad = scEstaUnidad ?? GetComponent<Unidad>();

    List<Unidad> objetivos = ListaHayObjetivosAlAlcance()
      .OfType<Unidad>()
      .OrderBy(_ => UnityEngine.Random.value)
      .Take(CantidadObjetivos)
      .ToList();

    if (scEstaUnidad == null || objetivos.Count == 0)
    {
      return;
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    PrepararInicioAnimacion(null, objetivos[0]);
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();

    await BattleManager.DelayCombateAsync(450);

    hActualCooldown = hCooldownMax;

    foreach (Unidad objetivo in objetivos)
    {
      AplicarEfectosHabilidad(objetivo);
      await BattleManager.DelayCombateAsync(120);
    }
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad objetivo) || scEstaUnidad == null)
    {
      return;
    }

    DibujarAnillo(objetivo);

    float danio = TiradaDeDados.TirarDados(DadosCantidad, DadosCaras);
    danio = danio / 100f * (100 + scEstaUnidad.mod_DanioPorcentaje);

    bool noSeSalva = objetivo.TiradaSalvacion(3, DificultadSalvacion);
    if (noSeSalva)
    {    objetivo.RecibirDanio(danio, TipoDanioNecrotico, false, scEstaUnidad);

      objetivo.estado_Condenado = Mathf.Min(3, objetivo.estado_Condenado + Mathf.Max(objetivo.estado_Condenado, TurnosCondena));
      objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Condenado"), new Color(0.4f, 0.24f, 0.5f));
      BattleManager.Instance?.EscribirLog($"{objetivo.uNombre} {TRADU.i.Traducir("es condenado por")} {TurnosCondena} {TRADU.i.Traducir("turnos.")}");
    }
    else
    {
      BattleManager.Instance?.EscribirLog($"{objetivo.uNombre} {TRADU.i.Traducir("resiste la condena, pero sufre el latido necrotico.")}");
    }
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return objPosibles.OfType<Unidad>().OrderBy(_ => UnityEngine.Random.value).FirstOrDefault();
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    scEstaUnidad = scEstaUnidad ?? GetComponent<Unidad>();
    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return objPosibles;
    }

    LadoManager ladoEnemigo = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();
    if (ladoEnemigo == null)
    {
      return objPosibles;
    }

    ladoEnemigo.ActualizarListaDeUnidadesEnLado();

    foreach (Unidad u in ladoEnemigo.unidadesLado)
    {
      if (u == null)
      {
        continue;
      }

      if (u.HP_actual < 1)
      {
        continue;
      }

      if (u.ObtenerEstaEscondido() > 0 && !Usuario.GetComponent<IAUnidad>().bPuedeVerEscondidos)
      {
        continue;
      }

      objPosibles.Add(u);
    }

    return objPosibles;
  }

  void DibujarAnillo(Unidad objetivo)
  {
    Vector3 destino = objetivo.puntoEntrante != null
      ? objetivo.puntoEntrante.position
      : objetivo.transform.position + Vector3.up * AlturaAnillo;

    GameObject anilloGO = new GameObject("VFX_CondenaRaices");
    if (BattleManager.Instance != null)
    {
      anilloGO.transform.SetParent(BattleManager.Instance.transform, true);
    }

    LineRenderer anillo = anilloGO.AddComponent<LineRenderer>();
    anillo.useWorldSpace = true;
    anillo.alignment = LineAlignment.View;
    anillo.material = ObtenerMaterial();
    anillo.widthMultiplier = AnchoAnillo;
    anillo.loop = true;
    anillo.textureMode = LineTextureMode.Stretch;
    anillo.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    anillo.receiveShadows = false;
    anillo.numCapVertices = 0;
    anillo.numCornerVertices = 4;

    Vector3 normal = Vector3.up;
    Vector3 binormal = Vector3.right;
    Vector3 tangent = Vector3.forward;

    Vector3[] puntos = new Vector3[SegmentosAnillo];
    for (int i = 0; i < SegmentosAnillo; i++)
    {
      float angulo = (i / (float)SegmentosAnillo) * Mathf.PI * 2f;
      float ruido = Mathf.Sin(angulo * 3.5f) * 0.055f + Mathf.PerlinNoise(angulo, Time.time * 0.35f) * 0.02f;
      Vector3 offset = (Mathf.Cos(angulo) * binormal + Mathf.Sin(angulo) * tangent) * (RadioAnillo + ruido);
      offset += normal * (Mathf.Sin(angulo * 2.5f) * 0.07f + AlturaAnillo);
      puntos[i] = destino + offset;
    }

    anillo.positionCount = SegmentosAnillo;
    anillo.SetPositions(puntos);

    anillo.startColor = new Color(0.12f, 0.95f, 0.48f, 0.85f);
    anillo.endColor = new Color(0.18f, 0.35f, 0.12f, 0.18f);

    StartCoroutine(DesvanecerAnillo(anillo));
    StartCoroutine(GenerarEsporas(destino, binormal, tangent, normal));
  }

  IEnumerator DesvanecerAnillo(LineRenderer anillo)
  {
    float tiempo = 0f;
    while (tiempo < DuracionAnillo)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / DuracionAnillo);
      if (anillo != null)
      {
        Color cInicio = anillo.startColor;
        Color cFin = anillo.endColor;
        anillo.startColor = new Color(cInicio.r, cInicio.g, cInicio.b, Mathf.Lerp(0.85f, 0f, factor));
        anillo.endColor = new Color(cFin.r, cFin.g, cFin.b, Mathf.Lerp(0.28f, 0f, factor));
        anillo.widthMultiplier = Mathf.Lerp(AnchoAnillo, AnchoAnillo * 0.15f, factor);
      }
      yield return null;
    }

    if (anillo != null)
    {
      Destroy(anillo.gameObject);
    }
  }

  IEnumerator GenerarEsporas(Vector3 centro, Vector3 binormal, Vector3 tangent, Vector3 normal)
  {
    int particulas = 7;
    for (int i = 0; i < particulas; i++)
    {
      GameObject esporaGO = new GameObject("VFX_CondenaRaices_Espora");
      LineRenderer espora = esporaGO.AddComponent<LineRenderer>();
      espora.useWorldSpace = true;
      espora.alignment = LineAlignment.View;
      espora.material = ObtenerMaterial();
      espora.widthMultiplier = 0.05f;
      espora.positionCount = 3;
      espora.textureMode = LineTextureMode.Stretch;
      espora.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
      espora.receiveShadows = false;
      espora.numCapVertices = 2;
      espora.startColor = new Color(0.42f, 1.2f, 0.7f, 0.8f);
      espora.endColor = new Color(0.12f, 0.3f, 0.14f, 0f);

      float angulo = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
      Vector3 direccion = (Mathf.Cos(angulo) * binormal + Mathf.Sin(angulo) * tangent).normalized;
      float alturaBase = AlturaAnillo + UnityEngine.Random.Range(0.02f, 0.16f);
      Vector3 inicio = centro + direccion * (RadioAnillo * 0.45f) + normal * alturaBase;
      Vector3 medio = inicio + direccion * UnityEngine.Random.Range(0.08f, 0.18f) + normal * UnityEngine.Random.Range(0.04f, 0.12f);
      Vector3 fin = inicio + direccion * UnityEngine.Random.Range(0.12f, 0.25f) + normal * UnityEngine.Random.Range(-0.05f, 0.02f);

      espora.SetPosition(0, inicio);
      espora.SetPosition(1, medio);
      espora.SetPosition(2, fin);

      StartCoroutine(DesvanecerEspora(espora, UnityEngine.Random.Range(0.15f, 0.22f)));
      yield return new WaitForSeconds(0.028f);
    }
  }

  IEnumerator DesvanecerEspora(LineRenderer espora, float duracion)
  {
    float tiempo = 0f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / duracion);
      if (espora != null)
      {
        Color inicio = espora.startColor;
        Color fin = espora.endColor;
        espora.startColor = new Color(inicio.r, inicio.g, inicio.b, Mathf.Lerp(0.7f, 0f, factor));
        espora.endColor = new Color(fin.r, fin.g, fin.b, Mathf.Lerp(0.15f, 0f, factor));
        espora.widthMultiplier = Mathf.Lerp(0.05f, 0f, factor);
      }
      yield return null;
    }

    if (espora != null)
    {
      Destroy(espora.gameObject);
    }
  }

  static Material ObtenerMaterial()
  {
    if (materialAnillo == null)
    {
      Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }

      materialAnillo = new Material(shader);
      materialAnillo.name = "Mat_CondenaRaices";

      if (materialAnillo.HasProperty("_TintColor"))
      {
        materialAnillo.SetColor("_TintColor", new Color(0.18f, 0.8f, 0.4f, 0.8f));
      }

      if (materialAnillo.HasProperty("_EmissionColor"))
      {
        materialAnillo.EnableKeyword("_EMISSION");
        materialAnillo.SetColor("_EmissionColor", new Color(0.1f, 0.45f, 0.28f) * 2.2f);
      }
    }

    return materialAnillo;
  }
}
