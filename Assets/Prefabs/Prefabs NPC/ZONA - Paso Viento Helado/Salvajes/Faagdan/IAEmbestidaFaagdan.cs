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
    object objetivoPrincipal = EstablecerObjetivoPrioritario();
    if (objetivoPrincipal == null)
    {
      hActualCooldown = 1; // sin objetivo valido: no gasta AP ni el cooldown completo
      return;
    }

    scEstaUnidad.CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

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

    await EjecutarRecorridoVisualAsync(objetivoPrincipal, unidadesObjetivo);
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

    bool noSeSalva = objetivo.TiradaSalvacion(2, fortitudeDC);
    if (noSeSalva)
    {
      EmpujarVerticalAleatorioAsync(objetivo);
       // BUFF ---- Así se aplica un buff/debuff
      if (!objetivo.TieneBuffNombre("Derribado")) // no se acumula si ya esta derribado
      {
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
  }

  private static float FactorTiempo()
  {
    BattleManager batalla = BattleManager.Instance;
    return batalla != null && batalla.modoRapidoActivado
      ? GameSpeedSettings.CombatAiTimingFactor(batalla.NivelVelocidadCombateEfectivo)
      : 1f;
  }

  // Cada unidad de la fila recibe el golpe cuando el Faagdan pasa por su posicion (no todas al final).
  private void Impactar(Unidad unidad, float dirX)
  {
    if (unidad == null || unidad.HP_actual <= 0f || !unidad.gameObject.activeInHierarchy)
    {
      return;
    }

    FaagdanVfx.ImpactoEmbestida(unidad.transform.position, dirX);
    AplicarEfectosHabilidad(unidad);
  }

  private async Task EjecutarRecorridoVisualAsync(object objetivoReferencia, List<Unidad> unidades)
  {
    if (scEstaUnidad == null || scEstaUnidad.transform == null)
    {
      foreach (Unidad u in unidades) { Impactar(u, 1f); }
      return;
    }

    Vector3 origen = scEstaUnidad.transform.position;
    if (!TryObtenerDestinoRecorrido(objetivoReferencia, origen, out Vector3 destino))
    {
      foreach (Unidad u in unidades) { Impactar(u, 1f); }
      return;
    }

    float factor = FactorTiempo();
    float duracionIda = CalcularDuracionRecorrido(origen, destino, VelocidadRecorridoIda, DuracionMinRecorridoIda, DuracionMaxRecorridoIda) * factor;
    float duracionVuelta = CalcularDuracionRecorrido(destino, origen, VelocidadRecorridoVuelta, DuracionMinRecorridoVuelta, DuracionMaxRecorridoVuelta) * factor;

    float dirX = Mathf.Sign(destino.x - origen.x);
    if (Mathf.Approximately(dirX, 0f)) { dirX = -1f; }

    List<Unidad> pendientes = unidades
      .Where(u => u != null)
      .OrderBy(u => Vector3.Distance(origen, u.transform.position))
      .ToList();

    scEstaUnidad.IniciarPoseAtaqueSostenida(true);
    ReproducirVfxAcompanamiento();
    FaagdanVfx.Arranque(origen, dirX);
    FaagdanVfx.Estela(scEstaUnidad.transform, duracionIda);
    try
    {
      await AnimarRecorridoAsync(scEstaUnidad.transform, origen, destino, duracionIda, pendientes, dirX);

      // Los que no se tocaron durante el recorrido (por ejemplo, se movieron) reciben el golpe igual.
      foreach (Unidad restante in pendientes.ToList())
      {
        Impactar(restante, dirX);
      }
      pendientes.Clear();

      if (scEstaUnidad != null && scEstaUnidad.transform != null)
      {
        FaagdanVfx.Frenada(scEstaUnidad.transform.position, dirX);
      }

      if (PausaImpactoMs > 0)
      {
        await BattleManager.DelayCombateAsync(Mathf.RoundToInt(PausaImpactoMs * factor));
      }

      if (scEstaUnidad != null && scEstaUnidad.transform != null)
      {
        FaagdanVfx.Estela(scEstaUnidad.transform, duracionVuelta * 0.7f, 0.6f);
      }

      await AnimarRecorridoAsync(scEstaUnidad != null ? scEstaUnidad.transform : null, destino, origen, duracionVuelta, null, dirX);
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

  private async Task AnimarRecorridoAsync(Transform unidadTransform, Vector3 origen, Vector3 destino, float duracion, List<Unidad> pendientes, float dirX)
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

    Vector3 eje = destino - origen;
    eje.z = 0f;
    Vector3 ejeUnitario = eje.sqrMagnitude > 0.0001f ? eje.normalized : Vector3.right;
    float margen = GulekGulVfx.TamanoCasilla() * 0.3f;

    float tiempo = 0f;
    while (tiempo < duracion)
    {
      if (unidadTransform == null)
      {
        return; // la unidad desaparecio durante la carrera
      }

      // Respeta la pausa del combate.
      if (BattleManager.Instance != null && BattleManager.Instance.PausaCombateActiva)
      {
        await Task.Yield();
        continue;
      }

      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      unidadTransform.position = Vector3.Lerp(origen, destino, t);

      if (pendientes != null && pendientes.Count > 0)
      {
        float avance = Vector3.Dot(unidadTransform.position - origen, ejeUnitario);
        for (int i = 0; i < pendientes.Count; i++)
        {
          Unidad u = pendientes[i];
          if (u == null)
          {
            pendientes.RemoveAt(i);
            i--;
            continue;
          }

          float posicionUnidad = Vector3.Dot(u.transform.position - origen, ejeUnitario);
          if (avance >= posicionUnidad - margen)
          {
            pendientes.RemoveAt(i);
            i--;
            Impactar(u, dirX);
          }
        }
      }

      await Task.Yield();
    }

    if (unidadTransform != null)
    {
      unidadTransform.position = destino;
    }
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



