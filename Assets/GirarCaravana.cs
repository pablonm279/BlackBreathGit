using UnityEngine;

public class GirarCaravana : MonoBehaviour
{
  [Header("Quad objetivo")]
  [SerializeField] private Renderer quadRenderer;
  [SerializeField] private bool buscarRendererEnHijos = true;

  [Header("Materiales segun direccion")]
  [SerializeField] private Material materialRecto;
  [SerializeField] private Material materialArribaDerecha;
  [SerializeField] private Material materialAbajoDerecha;

  [Header("Configuracion")]
  [SerializeField, Range(0f, 89f)] private float umbralAngulo = 40f;

  private Material materialActual;

  private void Awake()
  {
    EnsureRendererReference();

    if (materialRecto != null)
      AplicarMaterial(materialRecto);
  }

#if UNITY_EDITOR
  private void OnValidate()
  {
    EnsureRendererReference();

    if (!Application.isPlaying && materialRecto != null)
      AplicarMaterial(materialRecto);
  }
#endif

  public void CambiarSpriteSegunRuta(Nodo nodoOrigen, Nodo nodoDestino)
  {
    if (nodoOrigen == null || nodoDestino == null)
      return;

    CambiarSpriteSegunPosiciones(nodoOrigen.transform.position, nodoDestino.transform.position);
  }

  public void CambiarSpriteSegunPosiciones(Vector3 origen, Vector3 destino)
  {
    float angulo = CalcularAngulo(origen, destino);
    print("Angulo calculado: " + angulo);

    Material materialElegido = materialRecto;
    if (angulo > umbralAngulo && materialArribaDerecha != null)
    {
      materialElegido = materialArribaDerecha;
    }
    else if (angulo < -umbralAngulo && materialAbajoDerecha != null)
    {
      materialElegido = materialAbajoDerecha;
    }

    if (materialElegido == null)
      materialElegido = materialRecto;

    AplicarMaterial(materialElegido);
  }

  private float CalcularAngulo(Vector3 origen, Vector3 destino)
  {
    Vector3 direccion = destino - origen;
    Vector2 planoSeleccionado = SeleccionarPlano(direccion);
    if (planoSeleccionado.sqrMagnitude <= 0.0001f)
      return 0f;

    return Mathf.Atan2(planoSeleccionado.y, planoSeleccionado.x) * Mathf.Rad2Deg;
  }

  private static Vector2 SeleccionarPlano(Vector3 direccion)
  {
    if (Mathf.Abs(direccion.y) >= Mathf.Abs(direccion.z))
      return new Vector2(direccion.x, direccion.y);

    return new Vector2(direccion.x, direccion.z);
  }

  private void AplicarMaterial(Material material)
  {
    if (quadRenderer == null || material == null || materialActual == material)
      return;

    quadRenderer.sharedMaterial = material;
    materialActual = material;
  }

  private void EnsureRendererReference()
  {
    if (quadRenderer != null)
      return;

    quadRenderer = buscarRendererEnHijos
      ? GetComponentInChildren<Renderer>()
      : GetComponent<Renderer>();
  }
}
