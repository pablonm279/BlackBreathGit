using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class IAPrimerGolpeAlabardero : IAHabilidad
{
  [SerializeField] public int pPrioridad = 0;

  private static readonly int[] OffsetsY = { 0, -1, 1 };

  void Awake()
  {
    nombre = "Primer Golpe";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 1;
    esHostil = false;
    prioridad = 0;
    costoAP = 0;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 0;
  }

  public async override Task ActivarHabilidad()
  {
    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return;
    }
    if(scEstaUnidad.CasillaPosicion.posX != 3)
    {
      hActualCooldown = 1;
      return;

    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    scEstaUnidad.ReproducirAnimacionAtaque();
    PrepararInicioAnimacion(null, scEstaUnidad);

    hActualCooldown = hCooldownMax;

    await BattleManager.DelayCombateAsync(450);

    AplicarEfectosHabilidad(scEstaUnidad);

    scEstaUnidad.EstablecerAPActualA(0);

    if (BattleManager.Instance != null && BattleManager.Instance.unidadActiva == scEstaUnidad)
    {
      BattleManager.Instance.TerminarTurno();
    }
  }

  public override async void AplicarEfectosHabilidad(object _)
  {
    if (scEstaUnidad?.CasillaPosicion == null)
    {
      return;
    }

    LadoManager ladoOpuesto = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();
    if (ladoOpuesto == null)
    {
      return;
    }

    int origenY = scEstaUnidad.CasillaPosicion.posY;

    foreach (int offset in OffsetsY)
    {
      int objetivoY = origenY + offset;
      if (objetivoY < 1 || objetivoY > 5)
      {
        continue;
      }

      Casilla casillaObjetivo = ladoOpuesto.ObtenerCasillaPorIndex(3, objetivoY);
      if (casillaObjetivo == null)
      {
        continue;
      }

      Unidad unidadEnCasilla = null;
      if (casillaObjetivo.Presente != null)
      {
        unidadEnCasilla = casillaObjetivo.Presente.GetComponent<Unidad>();
      }
      if (unidadEnCasilla == null)
      {
        unidadEnCasilla = BattleManager.Instance.lUnidadesTotal.Find(u => u != null && u.CasillaPosicion == casillaObjetivo);
      }

      bool unidadEmpujada = false;
      if (unidadEnCasilla != null)
      {
        if (unidadEnCasilla.HP_actual <= 0f || !unidadEnCasilla.gameObject.activeInHierarchy)
        {
          continue;
        }

        unidadEnCasilla.ForzarSiguienteMovimientoForzadoInmediato();
        unidadEnCasilla.EmpujarUnidad(1);
        unidadEmpujada = true;
      }

      if (casillaObjetivo.GetComponent<TrampaPrimerGolpeAlabardero>() != null)
      {
        continue;
      }

   
       hActualCooldown = 1;
      TrampaPrimerGolpeAlabardero trampa = casillaObjetivo.gameObject.AddComponent<TrampaPrimerGolpeAlabardero>();
      trampa.InicializarCreador(scEstaUnidad);
    }
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return scEstaUnidad;
  }

  /*public override List<object> ListaHayObjetivosAlAlcance()
  {
    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
      if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
      {
        return new List<object>();
      }
    }

    if (scEstaUnidad.CasillaPosicion.posX != 3)
    {
      return new List<object>();
    }

    LadoManager ladoOpuesto = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();
    if (ladoOpuesto == null)
    {
      return new List<object>();
    }

    int origenY = scEstaUnidad.CasillaPosicion.posY;
    foreach (int offset in OffsetsY)
    {
      int objetivoY = origenY + offset;
      if (objetivoY < 1 || objetivoY > 5)
      {
        continue;
      }

      Casilla casillaObjetivo = ladoOpuesto.ObtenerCasillaPorIndex(3, objetivoY);
      if (casillaObjetivo == null)
      {
        continue;
      }

      if (casillaObjetivo.GetComponent<TrampaPrimerGolpeAlabardero>() == null)
      {
        return new List<object> { scEstaUnidad };
      }
    }

    return new List<object>();
  }*/
  
   public override List<object> ListaHayObjetivosAlAlcance() //necesario para self buffearse
  {
    return new List<object> { scEstaUnidad };

  }

}
