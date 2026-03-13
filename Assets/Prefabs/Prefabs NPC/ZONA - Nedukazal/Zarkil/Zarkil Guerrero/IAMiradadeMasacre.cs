using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class IAMiradadeMasacre : IAHabilidad
{
  [SerializeField] public int pPrioridad = 0;

  private static readonly int[] OffsetsY = { 0, -1, 1 };

  void Awake()
  {
    nombre = "Mirada de la Masacre";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = 2;
    esHostil = false;
    prioridad = 10;
    costoAP = 1;
    afectaObstaculos = false;

    hActualCooldown = 0;
  }

  void Start()
  {
    prioridad = 10;
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
    //scEstaUnidad.ReproducirAnimacionAtaque();
    PrepararInicioAnimacion(null, scEstaUnidad);

    hActualCooldown = 2;

    await BattleManager.DelayCombateAsync(450);

    AplicarEfectosHabilidad(scEstaUnidad);


  
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

      

      if (casillaObjetivo.GetComponent<TrampaMiradamasacre>() != null)
      {
        continue;
      }

   
       hActualCooldown = 1;
      TrampaMiradamasacre trampa = casillaObjetivo.gameObject.AddComponent<TrampaMiradamasacre>();
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
