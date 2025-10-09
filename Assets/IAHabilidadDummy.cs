using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IAHabilidadDummy : IAHabilidad
{
  [SerializeField] private string nombreHabilidad = "Pasar Turno";

  void Awake()
  {
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    nombre = nombreHabilidad;
    esHostil = false;
    esMelee = false;
    hAncho = 0;
    hAlcance = 0;
    hCooldownMax = 0;
    hActualCooldown = 0;
    prioridad = int.MinValue;
    costoAP = 0;
    afectaObstaculos = true;
  }

  public override async Task ActivarHabilidad()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
    }

    if (scEstaUnidad == null)
    {
      BattleManager.Instance.TerminarTurno();
      return;
    }

    // Consume all remaining AP to force the AI to end its turn.
    scEstaUnidad.EstablecerAPActualA(0);
    await Task.Delay(150);
    BattleManager.Instance.TerminarTurno();
  }

  public override void AplicarEfectosHabilidad(object unidad)
  {
    // Intentionally left blank: this ability only ends the turn.
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return scEstaUnidad;
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    if (scEstaUnidad == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
    }

    if (scEstaUnidad != null && scEstaUnidad.ObtenerAPActual() > 0)
    {
      objPosibles.Add(scEstaUnidad);
    }

    return objPosibles;
  }
}
