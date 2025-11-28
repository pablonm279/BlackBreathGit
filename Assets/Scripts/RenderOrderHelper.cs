using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Utilidad simple para asegurar que VFX/proyectiles queden por encima de la escena.
/// No restaura estado porque los proyectiles se destruyen al finalizar su recorrido.
/// </summary>
public static class RenderOrderHelper
{
  private const int OrdenProyectilPrioritario = 5000;

  public static void ForzarProyectilAlFrente(GameObject proyectil)
  {
    if (proyectil == null)
    {
      return;
    }

    foreach (Canvas canvas in proyectil.GetComponentsInChildren<Canvas>(true))
    {
      canvas.overrideSorting = true;
      canvas.sortingOrder = OrdenProyectilPrioritario;
    }

    foreach (SortingGroup sg in proyectil.GetComponentsInChildren<SortingGroup>(true))
    {
      sg.sortingOrder = OrdenProyectilPrioritario;
    }

    foreach (Renderer renderer in proyectil.GetComponentsInChildren<Renderer>(true))
    {
      renderer.sortingOrder = OrdenProyectilPrioritario;
    }
  }
}
