using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Picado Gelido: el draco se lanza en picada sobre un enemigo (prefiere la retaguardia y a los mas heridos),
/// lo golpea con las garras y vuelve a su sitio. A cambio queda en tierra hasta su proximo turno
/// (derribado: -1 AP, -3 defensa y alcanzable por ataques cuerpo a cuerpo).
/// </summary>
public class IAPicadoGelido : IAHabilidad
{
  [SerializeField] private int bonusAtaque;
  [SerializeField] private int XdDanio;
  [SerializeField] private int daniodX;
  [SerializeField] private int tipoDanio; //1: Cortante

  void Awake()
  {
    nombre = "Picado Gélido";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 4;
    hCooldownMax = 4;
    esHostil = true;
    prioridad = 20; // por debajo del Aliento (30) y por encima de la Garra (0)
    costoAP = 3;
    afectaObstaculos = false;
    fuerzaPoseAtaque = true;

    hActualCooldown = 1; // no abre el combate con un picado

    bonusAtaque = 2;
    XdDanio = 3;
    daniodX = 10;
    tipoDanio = 1;
  }

  void Start()
  {
    prioridad = 20;
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    if (BattleManager.Instance == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return objPosibles;
    }

    IAUnidad ia = GetComponent<IAUnidad>();
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

      objPosibles.Add(u);
    }

    return objPosibles;
  }

  public override object EstablecerObjetivoPrioritario()
  {
    List<Unidad> unidades = objPosibles.OfType<Unidad>().Where(u => u != null && u.HP_actual > 0f).ToList();
    if (unidades.Count == 0)
    {
      return null;
    }

    // Prefiere la retaguardia (donde sus garras normales no llegan) y, dentro de ella, al mas herido.
    List<Unidad> retaguardia = unidades.Where(u => u.CasillaPosicion.posX <= 2).ToList();
    return (retaguardia.Count > 0 ? retaguardia : unidades).OrderBy(u => u.HP_actual).First();
  }

  public override async Task ActivarHabilidad()
  {
    ListaHayObjetivosAlAlcance();
    Unidad objetivo = EstablecerObjetivoPrioritario() as Unidad;
    if (objetivo == null)
    {
      hActualCooldown = 1;
      return;
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    PrepararInicioAnimacion(null, objetivo);

    float f = DracoHieloVfx.FactorTiempo();
    Transform tr = scEstaUnidad.transform;
    Vector3 origen = tr.position;

    Vector3 haciaObjetivo = objetivo.transform.position - origen;
    haciaObjetivo.z = 0f;
    Vector3 direccion = haciaObjetivo.sqrMagnitude > 0.0001f ? haciaObjetivo.normalized : Vector3.left;

    float cas = GulekGulVfx.TamanoCasilla();
    Vector3 destino = objetivo.transform.position - direccion * cas * 0.7f;
    destino.z = origen.z;

    await BattleManager.DelayCombateAsync(Mathf.RoundToInt(250 * f)); // se eleva y apunta

    if (!PuedeResolverAccionIA() || objetivo == null || objetivo.HP_actual <= 0f)
    {
      return;
    }

    scEstaUnidad.IniciarPoseAtaqueSostenida(true);
    DracoHieloVfx.EstelaPicado(tr, 0.3f * f);
    try
    {
      await DracoHieloVfx.MoverAsync(tr, origen, destino, 0.28f * f);

      if (objetivo != null && objetivo.HP_actual > 0f)
      {
        AplicarEfectosHabilidad(objetivo);
      }

      await BattleManager.DelayCombateAsync(Mathf.RoundToInt(120 * f));

      if (tr != null)
      {
        DracoHieloVfx.EstelaPicado(tr, 0.35f * f);
        await DracoHieloVfx.MoverAsync(tr, destino, origen, 0.4f * f);
      }
    }
    finally
    {
      if (tr != null) { tr.position = origen; }
      if (scEstaUnidad != null) { scEstaUnidad.FinalizarPoseAtaqueSostenida(); }
    }

    // El precio del picado: aterriza y queda expuesto hasta que vuelva a elevarse en su proximo turno.
    if (scEstaUnidad != null && scEstaUnidad.HP_actual > 0f && scEstaUnidad.estado_Volando)
    {
      scEstaUnidad.BajarVuelo(false, 2);
    }
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    Unidad objetivo = obj as Unidad;
    if (objetivo == null)
    {
      return;
    }

    float defensaObjetivo = objetivo.ObtenerdefensaActual();
    int resultado = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo);
    Vector3 posicion = objetivo.transform.position;

    if (resultado == -1)
    {
      objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
      scEstaUnidad.EstablecerAPActualA(0);
      return;
    }

    if (resultado == 0)
    {
      objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
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
    DracoHieloVfx.ImpactoHielo(posicion, critico ? 1.5f : 1f, critico);

    // Las garras heladas congelan solo en critico.
    if (critico && objetivo.HP_actual > 0f)
    {
      Estados.Aplicar_Congelado(objetivo, 1, scEstaUnidad);
    }

    objetivo.AplicarDebuffPorAtaquesreiterados(1);
  }
}
