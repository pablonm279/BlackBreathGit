using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Mueve temporalmente el GameObject de la unidad hacia el objetivo melee (con ligero zoom de cámara) y luego lo devuelve.
/// Solo afecta lo visual; no toca casillas ni estado lógico.
/// </summary>
public class MeleeApproachMover : MonoBehaviour
{
  [Header("Movimiento visual melee")]
  [SerializeField] private float duracionIdaBase = 0.4f;
  [SerializeField] private float duracionVueltaBase = 0.4f;
  [SerializeField] private float offsetAtras = 0.55f; // metros detrás del objetivo
  [SerializeField] private Vector3 offsetAdicional = Vector3.zero;
  [SerializeField] private float demoraAntesDeVolver = 0.8f;
  [SerializeField] private float zoomCamaraFactor = 0.03f; // 3% por Y hacia el objetivo

  private Unidad unidad;
  private Vector3? posicionRetorno;
  private Vector3? camaraRetorno;
  private bool mantenerAdelante;
  private bool adelantado;
  private float ultimaDuracionVuelta;
  private readonly System.Threading.SemaphoreSlim lockMovimiento = new System.Threading.SemaphoreSlim(1, 1);
  private bool lockTomado;

  // Pose
  private UnidadPoseController poseController;
  private Sprite spriteOriginal;
  private Image imagenObjetivoPose;

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
    Unidad objetivo = ElegirUnidad(objetivos);
    return objetivo == null ? Task.FromResult(false) : MoverHaciaObjetivoAsync(objetivo, false);
  }

  public Task<bool> PrepararAproximacionIAAsync(bool esMelee, int ancho, object objetivo, bool mantenerAdelanteDespues)
  {
    if (!esMelee || ancho > 1) return Task.FromResult(false);
    return objetivo is Unidad u ? MoverHaciaObjetivoAsync(u, mantenerAdelanteDespues) : Task.FromResult(false);
  }

  public async Task VolverAPosicionInicialAsync(bool forzar = false)
  {
    if (!posicionRetorno.HasValue || unidad == null) { posicionRetorno = null; camaraRetorno = null; adelantado = false; RestaurarPose(); LiberarLock(); return; }
    if (mantenerAdelante && !forzar) return;

    await Task.Delay(Mathf.Max(0, Mathf.RoundToInt(demoraAntesDeVolver * 1000f)));

    Vector3 origen = unidad.transform.position;
    Vector3 destino = posicionRetorno.Value;
    posicionRetorno = null;

    Transform cam = ObtenerCamaraBatalla();
    Vector3 camOrigen = cam != null ? cam.position : Vector3.zero;
    Vector3 camDestino = camaraRetorno ?? camOrigen;
    camaraRetorno = null;

    if (cam != null)
    {
      await Task.WhenAll(
        AnimarWorld(unidad.transform, origen, destino, ultimaDuracionVuelta),
        AnimarWorld(cam, camOrigen, camDestino, ultimaDuracionVuelta)
      );
    }
    else
    {
      await AnimarWorld(unidad.transform, origen, destino, ultimaDuracionVuelta);
    }

    adelantado = false;
    mantenerAdelante = false;
    RestaurarPose();
    LiberarLock();
  }

  async Task<bool> MoverHaciaObjetivoAsync(Unidad objetivo, bool mantenerLuego)
  {
    if (unidad == null || objetivo == null) return false;
    await lockMovimiento.WaitAsync();
    lockTomado = true;
    bool exito = false;

    Transform tOrigen = unidad.transform;
    Transform tDestino = objetivo.transform;

    Vector3 origen = tOrigen.position;
    Vector3 destinoBase = tDestino.position;
    Vector3 dir = destinoBase - origen;
    if (dir.sqrMagnitude <= 0.0001f) { LiberarLock(); return false; }

    try
    {
      Vector3 destino = destinoBase - dir.normalized * offsetAtras + offsetAdicional;
      float durIda = DuracionSegunPosX(objetivo.CasillaPosicion != null ? objetivo.CasillaPosicion.posX : 2, duracionIdaBase);
      ultimaDuracionVuelta = DuracionSegunPosX(objetivo.CasillaPosicion != null ? objetivo.CasillaPosicion.posX : 2, duracionVueltaBase);

      posicionRetorno = origen;
      mantenerAdelante = mantenerLuego;
      adelantado = true;

      AplicarPoseMovimiento();

      Transform cam = ObtenerCamaraBatalla();
      if (cam != null)
      {
        camaraRetorno = cam.position;
        Vector3 camDestino = cam.position + (destinoBase - cam.position) * zoomCamaraFactor;
        await Task.WhenAll(
          AnimarWorld(tOrigen, origen, destino, durIda),
          AnimarWorld(cam, cam.position, camDestino, durIda)
        );
      }
      else
      {
        await AnimarWorld(tOrigen, origen, destino, durIda);
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

  Unidad ElegirUnidad(IEnumerable<object> objetivos)
  {
    foreach (object obj in objetivos)
    {
      if (obj is Unidad u) return u;
    }
    return null;
  }

  float DuracionSegunPosX(int posX, float baseDuracion)
  {
    switch (posX)
    {
      case 3: return 0.25f;
      case 2: return 0.5f;
      default: return 0.75f;
    }
  }

  async Task AnimarWorld(Transform t, Vector3 origen, Vector3 destino, float duracion)
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
    Image target = poseController.targetImage ?? unidad?.uImage;
    if (target == null) return;
    if (poseController.poseMover == null) return;
    spriteOriginal = target.sprite;
    imagenObjetivoPose = target;
    target.sprite = poseController.poseMover;
  }

  void RestaurarPose()
  {
    if (imagenObjetivoPose != null && spriteOriginal != null)
    {
      imagenObjetivoPose.sprite = spriteOriginal;
    }
    imagenObjetivoPose = null;
    spriteOriginal = null;
  }

  void LiberarLock()
  {
    if (lockTomado)
    {
      lockTomado = false;
      lockMovimiento.Release();
    }
  }

  Transform ObtenerCamaraBatalla()
  {
    if (BattleManager.Instance != null && BattleManager.Instance.goCamara != null)
    {
      return BattleManager.Instance.goCamara.transform;
    }
    return Camera.main != null ? Camera.main.transform : null;
  }
}
