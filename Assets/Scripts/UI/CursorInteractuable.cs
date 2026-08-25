using UnityEngine;

/// <summary>
/// Marca interactuables que no sean Selectable de UI, Nodo, Unidad o Casilla.
/// Puede colocarse en el objeto con el collider o en uno de sus padres.
/// </summary>
public class CursorInteractuable : MonoBehaviour
{
  [SerializeField] private bool habilitado = true;

  public bool EstaHabilitado => habilitado && isActiveAndEnabled && gameObject.activeInHierarchy;

  public void SetHabilitado(bool valor)
  {
    habilitado = valor;
  }
}
