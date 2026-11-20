using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;

/// <summary>
/// Gulek-Gul arranca una roca de su propio lado del tablero y la lanza contra la retaguardia enemiga.
/// Consume la roca (abre cobertura) y solo esta disponible si le queda alguna.
/// </summary>
public class IALanzarRoca : IAHabilidad
{
  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int tipoDanio; //3: Contundente
  [SerializeField] private Sprite poseRocaAgacha;
  [SerializeField] private Sprite poseRocaLanza;

  void Awake()
  {
    nombre = "Lanzar Roca";
    Usuario = this.gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 3;
    hCooldownMax = 3;
    esHostil = true;
    prioridad = 11;
    costoAP = 2;
    afectaObstaculos = false;
    omitirAnimacionDeUso = true;

    hActualCooldown = 2; // no la usa en el primer turno

    bonusAtaque = 2;
    XdDanio = 3;
    daniodX = 10;
    tipoDanio = 3;
  }

  void Start()
  {
    prioridad = 11;
  }

  private static bool EsRoca(Obstaculo o)
  {
    return o != null && !string.IsNullOrEmpty(o.oName) && o.oName.ToLowerInvariant().Contains("roca");
  }

  private List<Obstaculo> ObtenerRocasPropias()
  {
    List<Obstaculo> rocas = new List<Obstaculo>();
    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return rocas;
    }

    foreach (GameObject go in GameObject.FindGameObjectsWithTag("Obstaculo"))
    {
      Obstaculo o = go != null ? go.GetComponent<Obstaculo>() : null;
      if (!EsRoca(o) || !go.activeInHierarchy || o.hpCurr <= 0f)
      {
        continue;
      }

      if (o.CasillaPosicion == null || o.CasillaPosicion.Presente != go)
      {
        continue;
      }

      if (o.CasillaPosicion.lado != scEstaUnidad.CasillaPosicion.lado)
      {
        continue;
      }

      rocas.Add(o);
    }

    return rocas;
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    if (BattleManager.Instance == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return objPosibles;
    }

    if (ObtenerRocasPropias().Count == 0)
    {
      return objPosibles; // sin municion no hay habilidad
    }

    IAUnidad ia = GetComponent<IAUnidad>();
    List<Unidad> enemigos = new List<Unidad>();
    foreach (Unidad u in BattleManager.Instance.lUnidadesTotal)
    {
      if (u == null || u.HP_actual <= 0f || u.CasillaPosicion == null || u.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
      {
        continue;
      }

      if (u.ObtenerEstaEscondido() > 0 && (ia == null || !ia.bPuedeVerEscondidos))
      {
        continue;
      }

      enemigos.Add(u);
    }

    // Prefiere la retaguardia (donde sus golpes de martillo no llegan).
    List<Unidad> retaguardia = enemigos.Where(u => u.CasillaPosicion.posX <= 2).ToList();
    objPosibles.AddRange((retaguardia.Count > 0 ? retaguardia : enemigos).Cast<object>());

    return objPosibles;
  }

  public override object EstablecerObjetivoPrioritario()
  {
    List<Unidad> unidades = objPosibles.OfType<Unidad>().Where(u => u != null && u.HP_actual > 0f).ToList();
    if (unidades.Count == 0)
    {
      return null;
    }

    // 50% al mas herido, 50% al azar: castiga sin ser predecible.
    if (UnityEngine.Random.value < 0.5f)
    {
      return unidades.OrderBy(u => u.HP_actual).First();
    }

    return unidades[UnityEngine.Random.Range(0, unidades.Count)];
  }

  public async override Task ActivarHabilidad()
  {
    List<Obstaculo> rocas = ObtenerRocasPropias();
    if (rocas.Count == 0)
    {
      hActualCooldown = 1;
      return;
    }

    ListaHayObjetivosAlAlcance();
    Unidad objetivo = EstablecerObjetivoPrioritario() as Unidad;
    if (objetivo == null)
    {
      hActualCooldown = 1;
      return;
    }

    // Cualquier roca de su lado sirve ("rocas alrededor").
    Obstaculo roca = rocas[UnityEngine.Random.Range(0, rocas.Count)];

    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    PrepararInicioAnimacion(null, objetivo);

    Sprite spriteRoca = ObtenerSpriteRoca(roca);
    Vector3 origen = roca.transform.position;

    float factorTiempo = EnemigoGulekGul.FactorTiempoHabilidad();
    EnemigoGulekGul gulek = scEstaUnidad as EnemigoGulekGul;
    int versionPose = gulek != null ? gulek.MostrarPoseTemporal(poseRocaAgacha) : 0;
    Vector3 destino;
    bool llegoAlImpacto;
    try
    {
      await BattleManager.DelayCombateAsync(Mathf.RoundToInt(450 * factorTiempo)); // se agacha para arrancarla
      if (!PuedeResolverAccionIA() || roca == null || roca.hpCurr <= 0f
        || roca.CasillaPosicion == null || roca.CasillaPosicion.Presente != roca.gameObject
        || objetivo == null || objetivo.HP_actual <= 0f)
      {
        return;
      }

      GulekGulVfx.ArrancarRoca(origen);
      roca.ForzarDestruccion(true);
      BattleManager.Instance.CalcularCasillasAMovimiento();

      destino = objetivo.transform.position;
      Vector3 manos = ObtenerPuntoManos(destino);

      // 1) La roca arrancada sube hasta sus manos (mientras sigue agachado).
      bool llegoManos = await GulekGulVfx.LanzarRoca(spriteRoca, origen, manos, 0.2f * factorTiempo, 0.5f);
      if (!llegoManos || !PuedeResolverAccionIA() || objetivo == null || objetivo.HP_actual <= 0f)
      {
        return;
      }

      // 2) Pose de lanzamiento y la roca sale desde sus manos (no desde el suelo).
      if (gulek != null && poseRocaLanza != null)
      {
        int versionLanzamiento = gulek.MostrarPoseTemporal(poseRocaLanza);
        if (versionLanzamiento != 0) { versionPose = versionLanzamiento; }
      }
      llegoAlImpacto = await GulekGulVfx.LanzarRoca(spriteRoca, manos, destino, 0.55f * factorTiempo, 1.9f);
    }
    finally
    {
      if (gulek != null) { gulek.RestaurarPoseTemporal(versionPose); }
    }

    if (!llegoAlImpacto || !PuedeResolverAccionIA())
    {
      return;
    }

    if (objetivo == null || objetivo.HP_actual <= 0f)
    {
      GulekGulVfx.ImpactoRoca(destino, false);
      return;
    }

    AplicarEfectosHabilidad(objetivo);
    await BattleManager.DelayCombateAsync(Mathf.RoundToInt(350 * factorTiempo));
  }

  // Punto aproximado de las manos de Gulek: centro de su sprite, adelantado hacia el objetivo.
  private Vector3 ObtenerPuntoManos(Vector3 destino)
  {
    Unidad u = scEstaUnidad;
    Vector3 centro = u.uImage != null ? u.uImage.transform.position : u.transform.position;
    float dir = Mathf.Sign(destino.x - u.transform.position.x);
    if (Mathf.Approximately(dir, 0f)) { dir = -1f; }
    float cas = GulekGulVfx.TamanoCasilla();
    return centro + new Vector3(dir * cas * 0.45f, cas * 0.1f, 0f);
  }

  private static Sprite ObtenerSpriteRoca(Obstaculo roca)
  {
    if (roca == null)
    {
      return null;
    }

    Image[] imagenes = roca.GetComponentsInChildren<Image>(true);
    foreach (Image img in imagenes)
    {
      if (img != null && img.sprite != null && img.gameObject.name == "Image")
      {
        return img.sprite;
      }
    }

    SpriteRenderer sr = roca.GetComponentInChildren<SpriteRenderer>(true);
    return sr != null ? sr.sprite : null;
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    Unidad objetivo = obj as Unidad;
    if (objetivo == null)
    {
      return;
    }

    float defensaObjetivo = objetivo.ObtenerdefensaActual();
    int resultado = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo);

    Vector3 puntoImpacto = objetivo.transform.position;

    if (resultado <= 0)
    {
      // Pifia o fallo: la roca se estrella contra el suelo cerca del objetivo.
      objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
      GulekGulVfx.PolvoYEscombros(puntoImpacto, 0.9f, 0.7f);
      return;
    }

    float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
    danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
    bool critico = resultado == 3;

    if (resultado == 1)
    {
      danio -= danio / 2; //Roce
    }

    objetivo.RecibirDanio(danio + (critico ? 4 : 2), tipoDanio, critico, scEstaUnidad);
    GulekGulVfx.ImpactoRoca(puntoImpacto, critico);
    objetivo.AplicarDebuffPorAtaquesreiterados(1);
  }
}
