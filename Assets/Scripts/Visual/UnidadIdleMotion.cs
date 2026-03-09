using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UnidadIdleMotion : MonoBehaviour
{
  [Header("Idle Motion")]
  [SerializeField] private bool habilitado = true;
  [SerializeField] private bool soloEnBatalla = true;
  [SerializeField] private float amplitudX = 0.28f;
  [SerializeField] private float amplitudY = 3.2f;
  [SerializeField] private float amplitudRotacion = 0.16f;
  [SerializeField] private float velocidad = 0.6f;
  [SerializeField] private float suavizado = 8.5f;
  [SerializeField] private float factorDuranteMovimiento = 0.08f;
  [SerializeField] private float multiplicadorGlobal = 1f;
  [SerializeField] private float multiplicadorUnidadActivaJugador = 1.08f; // +8% adicional si es turno del jugador

  private Unidad unidad;
  private RectTransform rectImagen;

  private Vector2 offsetAplicado;
  private float rotacionAplicada;
  private Vector2 posicionBase;
  private float rotacionBase;
  private bool baseInicializada;
  private float faseX;
  private float faseY;
  private float faseRot;

  void Awake()
  {
    unidad = GetComponent<Unidad>();
    VincularRect();

    faseX = Random.Range(0f, Mathf.PI * 2f);
    faseY = Random.Range(0f, Mathf.PI * 2f);
    faseRot = Random.Range(0f, Mathf.PI * 2f);
  }

  void OnEnable()
  {
    if (!VincularRect())
    {
      return;
    }

    CapturarBaseActual();
  }

  void LateUpdate()
  {
    if (!VincularRect())
    {
      return;
    }

    SincronizarBaseConCambiosExternos();

    if (!DebeAnimar())
    {
      AplicarOffset(Vector2.zero, 0f, true);
      return;
    }

    float t = Time.time * Mathf.Max(0.01f, velocidad);
    float factor = unidad != null && unidad.movimientoEnCurso ? factorDuranteMovimiento : 1f;
    float factorGlobal = Mathf.Max(0f, multiplicadorGlobal);
    if (EsUnidadActivaJugador())
    {
      factorGlobal *= Mathf.Max(0f, multiplicadorUnidadActivaJugador);
    }
    factor *= factorGlobal;

    float respiracionPrincipal = Mathf.Sin(t + faseY);
    float respiracionSecundaria = Mathf.Sin(t * 2f + faseRot) * 0.18f;
    float respiracionCompuesta = (respiracionPrincipal * 0.82f) + respiracionSecundaria;
    float swayNormalizado = Mathf.Sin(t * 0.5f + faseX);
    float sway = swayNormalizado * amplitudX * (0.55f + (0.45f * Mathf.Abs(respiracionPrincipal)));

    Vector2 objetivoOffset = new Vector2(
      sway,
      respiracionCompuesta * amplitudY) * factor;

    float objetivoRot = ((-respiracionPrincipal * 0.9f) + (swayNormalizado * 0.1f)) * amplitudRotacion * factor;

    bool suavizar = suavizado > 0.01f;
    AplicarOffset(objetivoOffset, objetivoRot, suavizar);
  }

  private bool EsUnidadActivaJugador()
  {
    if (unidad == null || BattleManager.Instance == null)
    {
      return false;
    }

    if (BattleManager.Instance.unidadActiva != unidad)
    {
      return false;
    }

    // Si tiene IA, no es el turno activo de una unidad del jugador.
    return unidad.GetComponent<IAUnidad>() == null;
  }

  void OnDisable()
  {
    if (rectImagen == null || !baseInicializada)
    {
      return;
    }

    SincronizarBaseConCambiosExternos();
    offsetAplicado = Vector2.zero;
    rotacionAplicada = 0f;

    rectImagen.anchoredPosition = posicionBase;
    Vector3 e = rectImagen.localEulerAngles;
    e.z = rotacionBase;
    rectImagen.localEulerAngles = e;
  }

  private bool VincularRect()
  {
    if (unidad == null)
    {
      unidad = GetComponent<Unidad>();
      if (unidad == null)
      {
        return false;
      }
    }

    if (rectImagen == null)
    {
      Image img = unidad.uImage;
      if (img == null)
      {
        return false;
      }

      rectImagen = img.rectTransform;
      baseInicializada = false;
    }

    return rectImagen != null;
  }

  private void CapturarBaseActual()
  {
    if (rectImagen == null)
    {
      return;
    }

    posicionBase = rectImagen.anchoredPosition;
    rotacionBase = ObtenerRotacionActual();
    offsetAplicado = Vector2.zero;
    rotacionAplicada = 0f;
    baseInicializada = true;
  }

  private void SincronizarBaseConCambiosExternos()
  {
    if (rectImagen == null)
    {
      return;
    }

    if (!baseInicializada)
    {
      CapturarBaseActual();
      return;
    }

    Vector2 posicionEsperada = posicionBase + offsetAplicado;
    Vector2 deltaPosicion = rectImagen.anchoredPosition - posicionEsperada;
    if (deltaPosicion.sqrMagnitude > 0.0001f)
    {
      posicionBase += deltaPosicion;
    }

    float rotacionEsperada = rotacionBase + rotacionAplicada;
    float deltaRotacion = Mathf.DeltaAngle(rotacionEsperada, ObtenerRotacionActual());
    if (Mathf.Abs(deltaRotacion) > 0.001f)
    {
      rotacionBase += deltaRotacion;
    }
  }

  private bool DebeAnimar()
  {
    if (!habilitado || rectImagen == null)
    {
      return false;
    }

    if (soloEnBatalla && BattleManager.Instance == null)
    {
      return false;
    }

    if (unidad == null)
    {
      return false;
    }

    if (!unidad.gameObject.activeInHierarchy || unidad.HP_actual <= 0f)
    {
      return false;
    }

    // Excluir unidades que no deben tener respiracion visual.
    if (unidad.esInmobil || unidad.esEtereo)
    {
      return false;
    }

    // No aplicar a voladoras (esten o no volando en este instante).
    if (unidad.unidadVoladora || unidad.estado_Volando)
    {
      return false;
    }

    return true;
  }

  private void AplicarOffset(Vector2 objetivoOffset, float objetivoRot, bool suavizarMovimiento)
  {
    if (!baseInicializada)
    {
      CapturarBaseActual();
    }

    if (suavizarMovimiento)
    {
      float lerp = Mathf.Clamp01(Time.deltaTime * suavizado);
      offsetAplicado = Vector2.Lerp(offsetAplicado, objetivoOffset, lerp);
      rotacionAplicada = Mathf.Lerp(rotacionAplicada, objetivoRot, lerp);
    }
    else
    {
      offsetAplicado = objetivoOffset;
      rotacionAplicada = objetivoRot;
    }

    rectImagen.anchoredPosition = posicionBase + offsetAplicado;
    Vector3 e = rectImagen.localEulerAngles;
    e.z = rotacionBase + rotacionAplicada;
    rectImagen.localEulerAngles = e;
  }

  private float ObtenerRotacionActual()
  {
    return Mathf.DeltaAngle(0f, rectImagen.localEulerAngles.z);
  }
}
