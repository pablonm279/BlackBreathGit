using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAOracionKadryn : IAHabilidad
{
  [SerializeField] public int pPrioridad;

  private const int CantidadTrampas = 5;

  void Awake()
  {
    nombre = "Oración de Kadryn";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 5;
    esHostil = false;
    prioridad = 12;
    costoAP = 3;
    afectaObstaculos = false;

    hActualCooldown = UnityEngine.Random.Range(0, 5);
  }

  void Start()
  {
    prioridad = 12;
  }

  public async override Task ActivarHabilidad()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = Usuario.GetComponent<Unidad>();
    }

    if (ObtenerCasillasDisponibles().Count < CantidadTrampas)
    {
      return;
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();

    PrepararInicioAnimacion(null, scEstaUnidad);

    await Task.Delay(600);

    hActualCooldown = hCooldownMax;

    AplicarEfectosHabilidad(scEstaUnidad);

    await Task.Delay(600);
  }

  public override void AplicarEfectosHabilidad(object objetivo)
  {
    if (BattleManager.Instance == null || BattleManager.Instance.ladoA == null)
    {
      hActualCooldown = 3;
      return;
    }

    List<Casilla> casillasDisponibles = ObtenerCasillasDisponibles();

    if (casillasDisponibles.Count < CantidadTrampas)
    {
      return;
    }

    List<Casilla> seleccionadas = casillasDisponibles
      .OrderBy(_ => UnityEngine.Random.value)
      .Take(CantidadTrampas)
      .ToList();

    foreach (Casilla casilla in seleccionadas)
    {
      TrampaAlientoNegro trampa = casilla.gameObject.AddComponent<TrampaAlientoNegro>();
      trampa.Inicializar();
    }

    if (seleccionadas.Count > 0)
    {
      BattleManager.Instance.EscribirLog(TRADU.i.Traducir("El Aliento Negro se expande por el campo enemigo."));
    }
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return scEstaUnidad;
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.ladoA == null || scEstaUnidad == null)
    {
      return new List<object>();
    }

    int casillasDisponibles = ObtenerCasillasDisponibles().Count;

    return casillasDisponibles >= CantidadTrampas ? new List<object> { scEstaUnidad } : new List<object>();
  }

  List<Casilla> ObtenerCasillasDisponibles()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.ladoA == null)
    {
      return new List<Casilla>();
    }

    LadoManager ladoEnemigo = BattleManager.Instance.ladoA;
    ladoEnemigo.ActualizarListaDeCasillasEnLado();

    return ladoEnemigo.casillasLado
      .Where(c => c != null && c.Presente == null && c.GetComponent<Trampa>() == null)
      .ToList();
  }
}
