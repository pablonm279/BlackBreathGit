using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAAlientoHelado : IAHabilidad
{
  [SerializeField] public int pPrioridad = 30;
  [SerializeField] private GameObject prefabAliento;
  [SerializeField] private Vector3 prefabOffset = new Vector3(0.0f, 0.0f, 0f);
  [SerializeField] private int dificultadReflejos = 13;

  private const int TipoDanioHielo = 5;
  private const int DadosDanio = 3;
  private const int CarasDado = 8;

  private void Awake()
  {
    nombre = "Aliento Helado";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 0;
    esMelee = true;
    hAlcance = 0;
    hCooldownMax = 3;
    hActualCooldown = 0;
    esHostil = true;
    prioridad = pPrioridad;
    costoAP = 3;
    afectaObstaculos = false;
  }

  private void Start()
  {
    prioridad = pPrioridad;
  }

  public override async Task ActivarHabilidad()
  {
    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    List<Unidad> objetivos = ObtenerUnidadesEnPiramide();
    List<object> objetivosComoObjetos = objetivos.Cast<object>().ToList();

    PrepararInicioAnimacion(objetivosComoObjetos, null);
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();
    
    await BattleManager.DelayCombateAsync(450);
    CrearVfx();
     await BattleManager.DelayCombateAsync(450);
    foreach (Unidad objetivo in objetivos)
    {
      AplicarEfectosHabilidad(objetivo);
    }
  }

  public override void AplicarEfectosHabilidad(object objetivo)
  {
    if (objetivo is Unidad unidadObjetivo)
    {
      float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDado);
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      bool fallaSalvacion = unidadObjetivo.TiradaSalvacion(unidadObjetivo.mod_TSReflejos, dificultadReflejos);
      if (!fallaSalvacion)
      {
        danio *= 0.5f;
      }

      unidadObjetivo.RecibirDanio(danio, TipoDanioHielo, false, scEstaUnidad);
    }
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return objPosibles.OfType<Unidad>()
      .OrderByDescending(u => u.CasillaPosicion.posX)
      .ThenBy(u => Mathf.Abs(u.CasillaPosicion.posY - scEstaUnidad.CasillaPosicion.posY))
      .FirstOrDefault();
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    objPosibles.Clear();

    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return objPosibles;
    }

    if (scEstaUnidad.CasillaPosicion.posX != 3)
    {
      IAUnidad ia = Usuario.GetComponent<IAUnidad>();
      if (ia != null)
      {
        ia.tendenciaMovX = 3 - scEstaUnidad.CasillaPosicion.posX;
        ia.tendenciaMovY = 0;
      }
      return objPosibles;
    }

    foreach (Unidad unidad in ObtenerUnidadesEnPiramide())
    {
      if (unidad == null) { continue; }

      if (unidad.ObtenerEstaEscondido() > 0 && !Usuario.GetComponent<IAUnidad>().bPuedeVerEscondidos)
      {
        continue;
      }

      objPosibles.Add(unidad);
    }

    return objPosibles;
  }

  private List<Unidad> ObtenerUnidadesEnPiramide()
  {
    List<Unidad> unidades = new List<Unidad>();
    Casilla casillaOrigen = scEstaUnidad != null ? scEstaUnidad.CasillaPosicion : null;
    if (casillaOrigen == null || casillaOrigen.ladoOpuesto == null)
    {
      return unidades;
    }

    LadoManager ladoObjetivo = casillaOrigen.ladoOpuesto.GetComponent<LadoManager>();
    if (ladoObjetivo == null)
    {
      return unidades;
    }

    foreach (Casilla cas in ObtenerCasillasPiramide(ladoObjetivo))
    {
      if (cas == null || cas.Presente == null)
      {
        continue;
      }

      Unidad unidad = cas.Presente.GetComponent<Unidad>();
      if (unidad != null)
      {
        unidades.Add(unidad);
      }
    }

    return unidades;
  }

  private IEnumerable<Casilla> ObtenerCasillasPiramide(LadoManager ladoObjetivo)
  {
    int filaCentro = Mathf.Clamp(scEstaUnidad.CasillaPosicion.posY, 1, 5);
    int filaMin = Mathf.Max(1, filaCentro - 1);
    int filaMax = Mathf.Min(5, filaCentro + 1);

    for (int x = 3; x >= 1; x--)
    {
      for (int y = filaMin; y <= filaMax; y++)
      {
        yield return ladoObjetivo.ObtenerCasillaPorIndex(x, y);
      }
    }
  }

  private void CrearVfx()
  {
    if (prefabAliento == null)
    {
      return;
    }

    Transform origen = scEstaUnidad != null && scEstaUnidad.puntoSaliente != null
      ? scEstaUnidad.puntoSaliente
      : transform;

    float direccionX = 1f;
    if (scEstaUnidad != null && scEstaUnidad.CasillaPosicion != null && scEstaUnidad.CasillaPosicion.lado == 2)
    {
      direccionX = -1f;
    }

    Vector3 offset = prefabOffset;
    offset.x *= direccionX;

    Instantiate(prefabAliento, origen.position + offset, origen.rotation);
  }
}
