using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAEmbestidaFaagdan : IAHabilidad
{
  [SerializeField] public int pPrioridad = 15;
  [SerializeField] private int XdDanio = 3;
  [SerializeField] private int daniodX = 8;
  [SerializeField] private int tipoDanio = 3; // Contundente
  [SerializeField] private int fortitudeDC = 12;

  private const string VfxAcompanamientoPath = "VFX/VFX_CargaDeEstoque";
  private const float VelocidadRecorridoIda = 11f;
  private const float VelocidadRecorridoVuelta = 7f;
  private const float DuracionMinRecorridoIda = 0.458f;
  private const float DuracionMaxRecorridoIda = 0.5f;
  private const float DuracionMinRecorridoVuelta = 0.90f;
  private const float DuracionMaxRecorridoVuelta = 0.95f;
  private const float OffsetPasadaFinal = 0.35f;
  private const int PausaAntesDeRecorrerMs = 80;
  private const int PausaImpactoMs = 80;
  private GameObject vfxAcompanamientoPrefab;

  private void Awake()
  {
    nombre = "Embestida Faagdan";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 0;
    esMelee = false;
    hAlcance = 5;
    hCooldownMax = 3;
    esHostil = true;
    prioridad = 15;
    costoAP = 3;
    afectaObstaculos = true;
    fuerzaPoseAtaque = true;

    hActualCooldown = UnityEngine.Random.Range(0, hCooldownMax + 1);
    vfxAcompanamientoPrefab = Resources.Load<GameObject>(VfxAcompanamientoPath);
  }

  private void Start()
  {
    prioridad = 15;
  }

  public async override Task ActivarHabilidad()
  {
    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    object objetivoPrincipal = EstablecerObjetivoPrioritario();
    List<Unidad> unidadesObjetivo = ObtenerUnidadesEnFila(objetivoPrincipal);

    if (objetivoPrincipal is Unidad unidadObjetivo && !unidadesObjetivo.Contains(unidadObjetivo))
    {
      unidadesObjetivo.Add(unidadObjetivo);
    }

    if (unidadesObjetivo.Count > 0)
    {
      PrepararInicioAnimacion(unidadesObjetivo.Cast<object>().ToList(), null);
    }
    else
    {
      PrepararInicioAnimacion(null, objetivoPrincipal);
    }

    if (PausaAntesDeRecorrerMs > 0)
    {
      await BattleManager.DelayCombateAsync(PausaAntesDeRecorrerMs);
    }

    await EjecutarRecorridoVisualAsync(objetivoPrincipal, () => AplicarEfectosEnFila(unidadesObjetivo));
  }

  private List<Unidad> ObtenerUnidadesEnFila(object objetivoReferencia)
  {
    var resultado = new HashSet<Unidad>();
    Casilla casillaReferencia = null;

    switch (objetivoReferencia)
    {
      case Unidad unidad:
        casillaReferencia = unidad.CasillaPosicion;
        break;
      case Obstaculo obstaculo:
        casillaReferencia = obstaculo.CasillaPosicion;
        break;
    }

    if (casillaReferencia == null)
    {
      return resultado.ToList();
    }

    foreach (Casilla casilla in casillaReferencia.ObtenerCasillasenMismaFila())
    {
      if (casilla.Presente == null)
      {
        continue;
      }

      Unidad unidad = casilla.Presente.GetComponent<Unidad>();
      if (unidad == null)
      {
        continue;
      }

      if (unidad == scEstaUnidad)
      {
        continue;
      }

      if (unidad.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
      {
        continue;
      }

      resultado.Add(unidad);
    }

    return resultado.ToList();
  }

  private void AplicarEfectosEnFila(IEnumerable<Unidad> unidades)
  {
    foreach (Unidad unidad in unidades)
    {
      AplicarEfectosHabilidad(unidad);
    }
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (obj is not Unidad objetivo)
    {
      return;
    }

    float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
    danio = danio / 100f * (100 + scEstaUnidad.mod_DanioPorcentaje);
    objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
    //objetivo.AplicarDebuffPorAtaquesreiterados(1);

    bool noSeSalva = objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, fortitudeDC);
    if (noSeSalva)
    {
      EmpujarVerticalAleatorioAsync(objetivo);
       // BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Derribado";
      buff.boolfDebufftBuff = false;
      buff.DuracionBuffRondas = 1;
      buff.cantAPMax -= 2;
      buff.cantDefensa -= 2;
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

    }
  }

  private async Task EjecutarRecorridoVisualAsync(object objetivoReferencia, Action impactoAlFinal)
  {
    if (scEstaUnidad == null || scEstaUnidad.transform == null)
    {
      impactoAlFinal?.Invoke();
      return;
    }

    Vector3 origen = scEstaUnidad.transform.position;
    if (!TryObtenerDestinoRecorrido(objetivoReferencia, origen, out Vector3 destino))
    {
      impactoAlFinal?.Invoke();
      return;
    }

    float duracionIda = CalcularDuracionRecorrido(origen, destino, VelocidadRecorridoIda, DuracionMinRecorridoIda, DuracionMaxRecorridoIda);
    float duracionVuelta = CalcularDuracionRecorrido(destino, origen, VelocidadRecorridoVuelta, DuracionMinRecorridoVuelta, DuracionMaxRecorridoVuelta);

    scEstaUnidad.IniciarPoseAtaqueSostenida(true);
    ReproducirVfxAcompanamiento();
    try
    {
      await AnimarRecorridoAsync(scEstaUnidad.transform, origen, destino, duracionIda);
      impactoAlFinal?.Invoke();

      if (PausaImpactoMs > 0)
      {
        await BattleManager.DelayCombateAsync(PausaImpactoMs);
      }

      await AnimarRecorridoAsync(scEstaUnidad.transform, destino, origen, duracionVuelta);
    }
    finally
    {
      if (scEstaUnidad != null && scEstaUnidad.transform != null)
      {
        scEstaUnidad.transform.position = origen;
        scEstaUnidad.FinalizarPoseAtaqueSostenida();
      }
    }
  }

  private bool TryObtenerDestinoRecorrido(object objetivoReferencia, Vector3 origen, out Vector3 destino)
  {
    destino = origen;

    Casilla casillaReferencia = objetivoReferencia switch
    {
      Unidad unidad => unidad.CasillaPosicion,
      Obstaculo obstaculo => obstaculo.CasillaPosicion,
      _ => null
    };

    if (casillaReferencia == null)
    {
      return false;
    }

    Casilla casillaFinal = casillaReferencia.ObtenerCasillasMasAtrasEnFila();
    if (casillaFinal == null)
    {
      return false;
    }

    Vector3 destinoBase = casillaFinal.transform.position;
    Vector3 direccion = destinoBase - origen;
    direccion.z = 0f;
    if (direccion.sqrMagnitude <= 0.0001f)
    {
      return false;
    }

    destino = destinoBase + direccion.normalized * OffsetPasadaFinal;
    destino.z = origen.z;
    return true;
  }

  private float CalcularDuracionRecorrido(Vector3 origen, Vector3 destino, float velocidad, float duracionMin, float duracionMax)
  {
    float distancia = Vector3.Distance(origen, destino);
    if (distancia <= 0.0001f)
    {
      return duracionMin;
    }

    float duracion = distancia / Mathf.Max(0.01f, velocidad);
    return Mathf.Clamp(duracion, duracionMin, duracionMax);
  }

  private async Task AnimarRecorridoAsync(Transform unidadTransform, Vector3 origen, Vector3 destino, float duracion)
  {
    if (unidadTransform == null)
    {
      return;
    }

    if (duracion <= 0f)
    {
      unidadTransform.position = destino;
      return;
    }

    float tiempo = 0f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      unidadTransform.position = Vector3.Lerp(origen, destino, t);
      await Task.Yield();
    }

    unidadTransform.position = destino;
  }

  private void ReproducirVfxAcompanamiento()
  {
    if (vfxAcompanamientoPrefab == null || scEstaUnidad == null || scEstaUnidad.transform == null)
    {
      return;
    }

    GameObject vfx = Instantiate(vfxAcompanamientoPrefab, scEstaUnidad.transform.position, Quaternion.identity);
    vfx.transform.SetParent(scEstaUnidad.transform, true);
    vfx.transform.localScale *= 0.8f;

    Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
    RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, scEstaUnidad.transform, 5);
  }

  private async Task EmpujarVerticalAleatorioAsync(Unidad objetivo)
  {
    await BattleManager.DelayCombateAsync(600);

    if (objetivo == null || objetivo.HP_actual <= 0f || !objetivo.gameObject.activeInHierarchy)
    {
      return;
    }

    Casilla origen = objetivo.CasillaPosicion;
    if (origen == null || origen.ladoGO == null)
    {
      return;
    }

    LadoManager lado = origen.ladoGO.GetComponent<LadoManager>();
    if (lado == null)
    {
      return;
    }

    List<Casilla> candidatos = new List<Casilla>();
    Casilla arriba = lado.ObtenerCasillaPorIndex(origen.posX, origen.posY + 1);
    Casilla abajo = lado.ObtenerCasillaPorIndex(origen.posX, origen.posY - 1);

    if (arriba != null && arriba.Presente == null)
    {
      candidatos.Add(arriba);
    }

    if (abajo != null && abajo.Presente == null)
    {
      candidatos.Add(abajo);
    }

    if (candidatos.Count == 0)
    {
      return;
    }

    Casilla destino = candidatos.Count == 1
      ? candidatos[0]
      : candidatos[UnityEngine.Random.value < 0.5f ? 0 : 1];

    objetivo.IntentarProgramarMovimientoForzado(destino);
  }

  public override object EstablecerObjetivoPrioritario()
  {
    Unidad unidadDuena = gameObject.GetComponent<Unidad>();
    if (unidadDuena == null)
    {
      return null;
    }

    List<Unidad> unidades = objPosibles.OfType<Unidad>().ToList();
    var unidadesOrdenadas = unidades
      .OrderByDescending(unidad => unidad.CasillaPosicion.posX)
      .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDuena.CasillaPosicion.posY))
      .ToList();

    if (unidadesOrdenadas.Any())
    {
      return unidadesOrdenadas.First();
    }

    return objPosibles.OfType<Obstaculo>().FirstOrDefault();
  }
}



