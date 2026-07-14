using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
/// <summary>
/// Mueve temporalmente el GameObject de la unidad hacia el objetivo melee y luego lo devuelve.
/// Solo afecta lo visual; no toca casillas ni estado lógico.
/// </summary>
public class MeleeApproachMover : MonoBehaviour
{
  [Header("Movimiento visual melee")]
  [SerializeField] private float duracionIdaBase = 0.4f;
  [SerializeField] private float duracionVueltaBase = 0.3f;
  [SerializeField] private float velocidadIda = 4.5f; // m/s aprox para mantener ritmo visual constante
  [SerializeField] private float velocidadVuelta = 5f;
  [SerializeField] private Vector2 rangoDuracionIda = new Vector2(0.18f, 0.75f);
  [SerializeField] private Vector2 rangoDuracionVuelta = new Vector2(0.15f, 0.6f);
  [SerializeField] private float offsetAtras = 0.55f; // metros detrás del objetivo
  [SerializeField] private Vector3 offsetAdicional = Vector3.zero;
  [SerializeField] private float demoraAntesDeVolver = 0.2f;
  [SerializeField] private float pausaTrasLlegar = 0.15f;

  private Unidad unidad;
  private Vector3? posicionRetorno;
  private bool mantenerAdelante;
  private bool adelantado;
  private float ultimaDuracionVuelta;
  private readonly System.Threading.SemaphoreSlim lockMovimiento = new System.Threading.SemaphoreSlim(1, 1);
  private bool lockTomado;
  private bool retornoTemporalmenteBloqueado;
  private TaskCompletionSource<bool> desbloqueoRetorno;

  // Pose
  private UnidadPoseController poseController;

  void Awake()
  {
    unidad = GetComponent<Unidad>();
    poseController = GetComponent<UnidadPoseController>();
    ultimaDuracionVuelta = duracionVueltaBase;
  }

  public static MeleeApproachMover ObtenerOCrear(Unidad unidad)
  {
    if (unidad == null) return null;
    return unidad.GetComponent<MeleeApproachMover>() ?? unidad.gameObject.AddComponent<MeleeApproachMover>();
  }

  public Task<bool> PrepararAproximacionJugadorAsync(Habilidad habilidad, List<object> objetivos)
  {
    if (habilidad == null || objetivos == null || objetivos.Count == 0) return Task.FromResult(false);
    if (!habilidad.esMelee || habilidad.esZonal || habilidad.enArea > 0) return Task.FromResult(false);

    object objetivo = ElegirObjetivoVisualParaHabilidad(habilidad, objetivos);
    if (!TryObtenerDatosObjetivo(objetivo, out Transform objetivoTransform))
    {
      return Task.FromResult(false);
    }
    return MoverHaciaObjetivoAsync(objetivoTransform, false);
  }

  object ElegirObjetivoVisualParaHabilidad(Habilidad habilidad, List<object> objetivos)
  {
    if (habilidad != null && habilidad.targetEspecial == 10 && BattleManager.Instance != null)
    {
      Casilla casillaCentro = BattleManager.Instance.casillaClickHabilidad;
      if (casillaCentro != null && casillaCentro.Presente != null)
      {
        Unidad unidadCentro = casillaCentro.Presente.GetComponent<Unidad>();
        if (unidadCentro != null && ContieneObjetivo(objetivos, unidadCentro))
        {
          return unidadCentro;
        }

        Obstaculo obstaculoCentro = casillaCentro.Presente.GetComponent<Obstaculo>();
        if (obstaculoCentro != null && ContieneObjetivo(objetivos, obstaculoCentro))
        {
          return obstaculoCentro;
        }
      }
    }

    return ElegirObjetivoVisual(objetivos);
  }

  bool ContieneObjetivo(IEnumerable<object> objetivos, object buscado)
  {
    if (objetivos == null || buscado == null)
    {
      return false;
    }

    foreach (object obj in objetivos)
    {
      if (ReferenceEquals(obj, buscado))
      {
        return true;
      }
    }

    return false;
  }

  public Task<bool> PrepararAproximacionIAAsync(bool esMelee, int ancho, object objetivo, bool mantenerAdelanteDespues)
  {
    if (!esMelee || ancho > 1) return Task.FromResult(false);

    // Si ya esta adelantado visualmente, no reaproxima; solo actualiza si debe mantenerse.
    if (adelantado && posicionRetorno.HasValue)
    {
      mantenerAdelante = mantenerAdelanteDespues;
      return Task.FromResult(true);
    }

    if (!TryObtenerDatosObjetivo(objetivo, out Transform objetivoTransform))
    {
      return Task.FromResult(false);
    }
    return MoverHaciaObjetivoAsync(objetivoTransform, mantenerAdelanteDespues);
  }

  public async Task VolverAPosicionInicialAsync(bool forzar = false)
  {
    await EsperarDesbloqueoRetornoAsync();
    if (!posicionRetorno.HasValue || unidad == null) { posicionRetorno = null; adelantado = false; mantenerAdelante = false; RestaurarPose(); LiberarLock(); return; }
    if (mantenerAdelante && !forzar) return;

    await BattleManager.DelayCombateAsync(Mathf.Max(0, Mathf.RoundToInt(demoraAntesDeVolver * 1000f)));
    await EsperarDesbloqueoRetornoAsync();

    Vector3 origen = unidad.transform.position;
    Vector3 destino = posicionRetorno.Value;
    posicionRetorno = null;

    unidad.FinalizarPoseAtaqueSostenida(false);
    AplicarPoseMovimiento();

    await AnimarWorld(unidad.transform, origen, destino, ultimaDuracionVuelta, true);

    adelantado = false;
    mantenerAdelante = false;
    RestaurarPose();
    LiberarLock();
  }

  public bool TieneAproximacionActiva()
  {
    return adelantado && posicionRetorno.HasValue;
  }

  public void BloquearRetornoTemporal()
  {
    if (retornoTemporalmenteBloqueado)
    {
      return;
    }

    retornoTemporalmenteBloqueado = true;
    desbloqueoRetorno = new TaskCompletionSource<bool>();
  }

  public void LiberarRetornoTemporal()
  {
    retornoTemporalmenteBloqueado = false;
    desbloqueoRetorno?.TrySetResult(true);
    desbloqueoRetorno = null;
  }

  public void ConfirmarPosicionActual()
  {
    posicionRetorno = null;
    adelantado = false;
    mantenerAdelante = false;
    RestaurarPose();
    LiberarLock();
  }

  async Task EsperarDesbloqueoRetornoAsync()
  {
    while (retornoTemporalmenteBloqueado)
    {
      Task espera = desbloqueoRetorno != null ? desbloqueoRetorno.Task : Task.CompletedTask;
      await espera;
    }
  }

  async Task<bool> MoverHaciaObjetivoAsync(Transform objetivoTransform, bool mantenerLuego)
  {
    if (unidad == null || objetivoTransform == null) return false;
    await lockMovimiento.WaitAsync();
    lockTomado = true;
    bool exito = false;

    Transform tOrigen = unidad.transform;
    Transform tDestino = objetivoTransform;

    Vector3 origen = tOrigen.position;
    Vector3 destinoBase = tDestino.position;
    Vector3 dir = destinoBase - origen;
    if (dir.sqrMagnitude <= 0.0001f) { LiberarLock(); return false; }

    try
    {
      Vector3 destino = destinoBase - dir.normalized * offsetAtras + offsetAdicional;
      float distancia = Vector3.Distance(origen, destino);
      float durIda = DuracionSegunDistancia(distancia, velocidadIda, rangoDuracionIda, duracionIdaBase);
      ultimaDuracionVuelta = DuracionSegunDistancia(distancia, velocidadVuelta, rangoDuracionVuelta, duracionVueltaBase);

      posicionRetorno = origen;
      mantenerAdelante = mantenerLuego;
      adelantado = true;

      AplicarPoseMovimiento();

      await AnimarWorld(tOrigen, origen, destino, durIda);

      if (pausaTrasLlegar > 0f)
      {
        int esperaMs = Mathf.RoundToInt(pausaTrasLlegar * 1000f);
        await BattleManager.DelayCombateAsync(Mathf.Max(0, esperaMs));
      }
      exito = true;
      return exito;
    }
    finally
    {
      if (!exito)
      {
        LiberarLock();
      }
    }
  }

  object ElegirObjetivoVisual(IEnumerable<object> objetivos)
  {
    foreach (object obj in objetivos)
    {
      if (obj is Unidad) return obj;
    }
    foreach (object obj in objetivos)
    {
      if (obj is Obstaculo) return obj;
    }
    return null;
  }

  bool TryObtenerDatosObjetivo(object objetivo, out Transform objetivoTransform)
  {
    objetivoTransform = null;
    if (objetivo is Casilla casillaObjetivo)
    {
      objetivoTransform = casillaObjetivo.transform;
      return true;
    }
    if (objetivo is Unidad unidadObjetivo)
    {
      objetivoTransform = unidadObjetivo.transform;
      return true;
    }
    if (objetivo is Obstaculo obstaculoObjetivo)
    {
      objetivoTransform = obstaculoObjetivo.transform;
      return true;
    }
    return false;
  }

  float DuracionSegunDistancia(float distancia, float velocidad, Vector2 rangoDuracion, float duracionFallback)
  {
    if (distancia <= 0.0001f)
    {
      return Mathf.Max(0.01f, duracionFallback);
    }

    float velocidadUsar = velocidad > 0.0001f
      ? velocidad
      : distancia / Mathf.Max(0.01f, duracionFallback);

    float minimo = Mathf.Min(rangoDuracion.x, rangoDuracion.y);
    float maximo = Mathf.Max(rangoDuracion.x, rangoDuracion.y);
    return Mathf.Clamp(distancia / velocidadUsar, minimo, maximo);
  }

  async Task AnimarWorld(Transform t, Vector3 origen, Vector3 destino, float duracion, bool respetarBloqueoRetorno = false)
  {
    if (t == null)
    {
      return;
    }

    if (duracion <= 0f)
    {
      t.position = destino;
      return;
    }

    float tiempo = 0f;
    while (tiempo < duracion)
    {
      if (respetarBloqueoRetorno)
      {
        await EsperarDesbloqueoRetornoAsync();
      }

      tiempo += Time.deltaTime;
      float tLerp = Mathf.Clamp01(tiempo / duracion);
      t.position = Vector3.Lerp(origen, destino, tLerp);
      await Task.Yield();
    }

    t.position = destino;
  }

  void AplicarPoseMovimiento()
  {
    if (poseController == null) return;
    poseController.OnStartMove();
  }

  void RestaurarPose()
  {
    if (poseController == null) return;
    poseController.OnStopMove();
  }

  void LiberarLock()
  {
    if (lockTomado)
    {
      lockTomado = false;
      lockMovimiento.Release();
    }
  }
}



