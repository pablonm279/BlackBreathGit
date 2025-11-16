using System.Collections.Generic;
using UnityEngine;

public class DestruirObstaculo : Habilidad
{
  private readonly List<Obstaculo> obstaculosDisponibles = new List<Obstaculo>();

  public override void Awake()
  {
    nombre = "Destruir Obstaculo";
    costoAP = 3;
    costoPM = 0;
    cooldownMax = 0;
    esZonal = false;
    enArea = 0;
    esforzable = 1;
    esCargable = false;
    esMelee = true;
    esHostil = false;
    bAfectaObstaculos = true;
    Usuario = gameObject;
    scEstaUnidad = GetComponent<Unidad>();
    ActualizarDescripcion();

     imHab = Resources.Load<Sprite>("imHab/DestruirObstaculo");

  }

  public override void Activar()
  {
    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return;
    }

    BattleManager.Instance.LimpiarCapasCasillas();
    obstaculosDisponibles.Clear();

    List<Casilla> casillas = scEstaUnidad.CasillaPosicion.ObtenerCasillasAlrededor(1);
    foreach (Casilla casilla in casillas)
    {
      if (casilla == null || casilla.Presente == null)
      {
        continue;
      }

      if (casilla.lado != scEstaUnidad.CasillaPosicion.lado)
      {
        continue;
      }

      Obstaculo obstaculo = casilla.Presente.GetComponent<Obstaculo>();
      if (obstaculo == null || !obstaculo.destruiblePorMismoLado)
      {
        continue;
      }

      casilla.ActivarCapaColorRojo();
      obstaculosDisponibles.Add(obstaculo);
    }

    if (obstaculosDisponibles.Count == 0)
    {
      BattleManager.Instance.HabilidadActiva = null;
      BattleManager.Instance.SeleccionandoObjetivo = false;
      BattleManager.Instance.EscribirLog(Traducir("No hay obstaculos adyacentes que puedas destruir."));
      BattleManager.Instance.scUIBotonesHab?.DeseleccionarTodas();
      BattleManager.Instance.scUIContadorAP?.ResetearCirculos();
      return;
    }

    BattleManager.Instance.SeleccionandoObjetivo = true;
    BattleManager.Instance.HabilidadActiva = this;
    BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.AddRange(obstaculosDisponibles);
  }

  public override void ActualizarDescripcion()
  {
    txtDescripcion = Traducir("Gasta 3 PA, para destruir un obstaculo adyacente de tu mismo lado si lo permite.");
  }

  public override void AplicarEfectosHabilidad(object objetivo, int tirada, Casilla casillaOrigenTrampa)
  {
    if (objetivo is not Obstaculo obstaculo || scEstaUnidad == null)
    {
      return;
    }

    if (obstaculo.CasillaPosicion == null || obstaculo.CasillaPosicion.lado != scEstaUnidad.CasillaPosicion.lado)
    {
      BattleManager.Instance.EscribirLog(Traducir("Solo puedes destruir obstaculos que esten en casillas aliadas."));
      return;
    }

    if (!obstaculo.destruiblePorMismoLado)
    {
      BattleManager.Instance.EscribirLog(Traducir("Este obstaculo no puede ser destruido por tus unidades."));
      return;
    }

    if (obstaculo.CasillaPosicion.Presente == obstaculo.gameObject)
    {
      obstaculo.CasillaPosicion.Presente = null;
    }

    obstaculo.ForzarDestruccion();
    BattleManager.Instance.EscribirLog(string.Format("{0} {1}", Traducir("Destruyes"), obstaculo.oName));
    BattleManager.Instance.SincronizarHabilidadDestruirObstaculo(scEstaUnidad);
    BattleManager.Instance.CalcularCasillasAMovimiento();
  }

  private string Traducir(string texto)
  {
    if (TRADU.i != null)
    {
      return TRADU.i.Traducir(texto);
    }
    return texto;
  }
}
