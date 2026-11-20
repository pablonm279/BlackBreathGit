using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAAlientoHelado : IAHabilidad
{
  [SerializeField] public int pPrioridad = 30;
  [SerializeField] private GameObject prefabAliento; // en desuso: el aliento ahora es un VFX procedural (DracoHieloVfx)
  [SerializeField] private Vector3 prefabOffset = new Vector3(0.0f, 0.0f, 0f);
  [SerializeField] private int dificultadReflejos = 13;

  private const int TipoDanioHielo = 5;
  private const int DadosDanio = 3;
  private const int CarasDado = 8;
  private const int TipoSalvacionReflejos = 2;

  private void Awake()
  {
    nombre = "Aliento Helado";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 0;
    esMelee = false;
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
    List<Casilla> casillas = ObtenerCasillasPiramide().Where(c => c != null).ToList();
    List<Unidad> objetivos = ObtenerUnidadesEnPiramide().Where(PuedeVerObjetivo).ToList();
    if (objetivos.Count == 0)
    {
      hActualCooldown = 1; // nadie en la zona: no gasta AP ni el cooldown completo
      return;
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    float f = DracoHieloVfx.FactorTiempo();
    scEstaUnidad.SetBloquearAnimacionVuelo(true);
    try
    {
      // PrepararInicioAnimacion ya reproduce la pose de la habilidad (antes se llamaba dos veces).
      PrepararInicioAnimacion(objetivos.Cast<object>().ToList(), null);

      Vector3 boca = ObtenerPuntoBoca();
      DracoHieloVfx.CargaAliento(boca, 0.45f * f);
      await BattleManager.DelayCombateAsync(Mathf.RoundToInt(450 * f));

      if (!PuedeResolverAccionIA())
      {
        return;
      }

      Vector3 centro = CentroDeCasillas(casillas, objetivos);
      DracoHieloVfx.AlientoCono(boca, centro, 0.9f * f);
      GulekGulVfx.SacudirCamara(0.35f, 0.5f);

      // Escarcha en toda la zona (ocupada o no), escalonada desde la columna mas cercana al draco.
      foreach (Casilla cas in casillas)
      {
        float retraso = (3 - cas.posX) * 0.12f * f + 0.1f;
        DracoHieloVfx.EscarchaEnCasilla(cas.transform.position, 0.95f, retraso, 3.3f);
      }

      // El daño llega columna por columna, a medida que el aliento avanza.
      for (int columna = 3; columna >= 1; columna--)
      {
        await BattleManager.DelayCombateAsync(Mathf.RoundToInt(170 * f));
        foreach (Unidad objetivo in objetivos.Where(u => u != null && u.CasillaPosicion != null && u.CasillaPosicion.posX == columna))
        {
          AplicarEfectosHabilidad(objetivo);
        }
      }

      await BattleManager.DelayCombateAsync(Mathf.RoundToInt(250 * f));
    }
    finally
    {
      scEstaUnidad.SetBloquearAnimacionVuelo(false);
    }
  }

  public override void AplicarEfectosHabilidad(object objetivo)
  {
    if (objetivo is Unidad unidadObjetivo && unidadObjetivo.HP_actual > 0f)
    {
      float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDado);
      danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

      bool fallaSalvacion = unidadObjetivo.TiradaSalvacion(TipoSalvacionReflejos, dificultadReflejos);
      if (!fallaSalvacion)
      {
        danio *= 0.5f;
      }

      Vector3 posicion = unidadObjetivo.transform.position;
      unidadObjetivo.RecibirDanio(danio, TipoDanioHielo, false, scEstaUnidad);
      DracoHieloVfx.ImpactoHielo(posicion, fallaSalvacion ? 1.1f : 0.7f, fallaSalvacion);

      // Quien no resiste el aliento queda congelado (1 acumulacion: -1 AP al empezar su turno, -2 defensa).
      if (fallaSalvacion && unidadObjetivo.HP_actual > 0f)
      {
        Estados.Aplicar_Congelado(unidadObjetivo, 1, scEstaUnidad);
      }
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

    IAUnidad ia = Usuario.GetComponent<IAUnidad>();

    if (scEstaUnidad.CasillaPosicion.posX != 3)
    {
      if (ia != null)
      {
        ia.tendenciaMovX = 3 - scEstaUnidad.CasillaPosicion.posX;
        ia.tendenciaMovY = 0;
      }
      return objPosibles;
    }

    foreach (Unidad unidad in ObtenerUnidadesEnPiramide())
    {
      if (PuedeVerObjetivo(unidad))
      {
        objPosibles.Add(unidad);
      }
    }

    // Nadie en la zona del aliento (filas +-1): se alinea con la fila que mas enemigos cubre.
    if (objPosibles.Count == 0 && ia != null)
    {
      int direccion = DireccionHaciaMejorFila();
      if (direccion != 0)
      {
        ia.tendenciaMovX = 0;
        ia.tendenciaMovY = direccion;
      }
    }

    return objPosibles;
  }

  private bool PuedeVerObjetivo(Unidad unidad)
  {
    if (unidad == null || unidad.HP_actual <= 0f)
    {
      return false;
    }

    IAUnidad ia = Usuario != null ? Usuario.GetComponent<IAUnidad>() : null;
    return unidad.ObtenerEstaEscondido() <= 0 || (ia != null && ia.bPuedeVerEscondidos);
  }

  // Fila (1-5) centrada en la que el aliento alcanzaria mas enemigos; devuelve -1, 0 o +1 respecto de la fila actual.
  private int DireccionHaciaMejorFila()
  {
    if (BattleManager.Instance == null || scEstaUnidad.CasillaPosicion == null)
    {
      return 0;
    }

    int filaActual = scEstaUnidad.CasillaPosicion.posY;
    int ladoPropio = scEstaUnidad.CasillaPosicion.lado;

    List<Unidad> enemigos = BattleManager.Instance.lUnidadesTotal
      .Where(u => u != null && u.HP_actual > 0f && u.CasillaPosicion != null && u.CasillaPosicion.lado != ladoPropio && PuedeVerObjetivo(u))
      .ToList();
    if (enemigos.Count == 0)
    {
      return 0;
    }

    int mejorFila = filaActual;
    int mejorCantidad = -1;
    for (int centro = 1; centro <= 5; centro++)
    {
      int cantidad = enemigos.Count(u => Mathf.Abs(u.CasillaPosicion.posY - centro) <= 1);
      bool mejor = cantidad > mejorCantidad
        || (cantidad == mejorCantidad && Mathf.Abs(centro - filaActual) < Mathf.Abs(mejorFila - filaActual));
      if (mejor)
      {
        mejorCantidad = cantidad;
        mejorFila = centro;
      }
    }

    return (int)Mathf.Sign(mejorFila - filaActual) * (mejorFila == filaActual ? 0 : 1);
  }

  private Vector3 ObtenerPuntoBoca()
  {
    Transform origen = scEstaUnidad != null && scEstaUnidad.puntoSaliente != null
      ? scEstaUnidad.puntoSaliente
      : transform;

    float direccionX = scEstaUnidad != null && scEstaUnidad.CasillaPosicion != null && scEstaUnidad.CasillaPosicion.lado == 2 ? -1f : 1f;
    Vector3 offset = prefabOffset;
    offset.x *= direccionX;
    return origen.position + offset;
  }

  private static Vector3 CentroDeCasillas(List<Casilla> casillas, List<Unidad> respaldo)
  {
    Vector3 suma = Vector3.zero;
    int n = 0;
    foreach (Casilla c in casillas) { suma += c.transform.position; n++; }
    if (n == 0)
    {
      foreach (Unidad u in respaldo) { suma += u.transform.position; n++; }
    }

    return n > 0 ? suma / n : Vector3.zero;
  }

  private List<Unidad> ObtenerUnidadesEnPiramide()
  {
    List<Unidad> unidades = new List<Unidad>();
    foreach (Casilla cas in ObtenerCasillasPiramide())
    {
      if (cas == null || cas.Presente == null)
      {
        continue;
      }

      Unidad unidad = cas.Presente.GetComponent<Unidad>();
      if (unidad != null && !unidades.Contains(unidad))
      {
        unidades.Add(unidad);
      }
    }

    return unidades;
  }

  private IEnumerable<Casilla> ObtenerCasillasPiramide()
  {
    Casilla casillaOrigen = scEstaUnidad != null ? scEstaUnidad.CasillaPosicion : null;
    if (casillaOrigen == null || casillaOrigen.ladoOpuesto == null)
    {
      yield break;
    }

    LadoManager ladoObjetivo = casillaOrigen.ladoOpuesto.GetComponent<LadoManager>();
    if (ladoObjetivo == null)
    {
      yield break;
    }

    int filaCentro = Mathf.Clamp(casillaOrigen.posY, 1, 5);
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
}
