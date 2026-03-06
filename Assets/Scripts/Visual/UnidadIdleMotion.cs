using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UnidadIdleMotion : MonoBehaviour
{
  [Header("Idle Motion")]
  [SerializeField] private bool habilitado = true;
  [SerializeField] private bool soloEnBatalla = true;
  [SerializeField] private float amplitudX = 1.05f;
  [SerializeField] private float amplitudY = 5.7f;
  [SerializeField] private float amplitudRotacion = 0.55f;
  [SerializeField] private float velocidad = 0.93f;
  [SerializeField] private float suavizado = 7.5f;
  [SerializeField] private float factorDuranteMovimiento = 0.2f;
  [SerializeField] private float multiplicadorGlobal = 1.05f; // +5% general
  [SerializeField] private float multiplicadorUnidadActivaJugador = 1.15f; // +15% adicional si es turno del jugador

  private Unidad unidad;
  private RectTransform rectImagen;

  private Vector2 offsetAplicado;
  private float rotacionAplicada;
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

  void LateUpdate()
  {
    if (!VincularRect())
    {
      return;
    }

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

    Vector2 objetivoOffset = new Vector2(
      Mathf.Sin(t + faseX) * amplitudX,
      Mathf.Sin(t * 1.37f + faseY) * amplitudY) * factor;

    float objetivoRot = Mathf.Sin(t * 0.87f + faseRot) * amplitudRotacion * factor;

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
    if (rectImagen == null)
    {
      return;
    }

    // Deshace el offset al desactivar para no dejar drift visual acumulado.
    Vector2 basePos = rectImagen.anchoredPosition - offsetAplicado;
    float baseRot = Mathf.DeltaAngle(0f, rectImagen.localEulerAngles.z) - rotacionAplicada;
    offsetAplicado = Vector2.zero;
    rotacionAplicada = 0f;

    rectImagen.anchoredPosition = basePos;
    Vector3 e = rectImagen.localEulerAngles;
    e.z = baseRot;
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
    }

    return rectImagen != null;
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
    Vector2 basePos = rectImagen.anchoredPosition - offsetAplicado;
    float rotActual = Mathf.DeltaAngle(0f, rectImagen.localEulerAngles.z);
    float baseRot = rotActual - rotacionAplicada;

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

    rectImagen.anchoredPosition = basePos + offsetAplicado;
    Vector3 e = rectImagen.localEulerAngles;
    e.z = baseRot + rotacionAplicada;
    rectImagen.localEulerAngles = e;
  }
}
