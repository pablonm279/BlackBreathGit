using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Utilidad simple para asegurar que VFX/proyectiles queden por encima de la escena.
/// No restaura estado porque los proyectiles se destruyen al finalizar su recorrido.
/// </summary>
public static class RenderOrderHelper
{
  private const int OrdenProyectilPrioritario = 5000;
  private const int OrdenBaseMax = 60;
  private const int OrdenBasePaso = 10;
  private static readonly Dictionary<Renderer, int> RendererOffsets = new Dictionary<Renderer, int>();
  private static readonly Dictionary<SortingGroup, int> SortingGroupOffsets = new Dictionary<SortingGroup, int>();

  public static int CalcularOrdenPorY(int posY)
  {
    return OrdenBaseMax - posY * OrdenBasePaso;
  }

  public static int CalcularOrdenPorY(float y)
  {
    return OrdenBaseMax - Mathf.RoundToInt(y) * OrdenBasePaso;
  }

  public static void AplicarOrdenPorY(GameObject objetivo, int posY, string sortingLayerCanvas = null)
  {
    int ordenBase = CalcularOrdenPorY(posY);
    AplicarOrdenBase(objetivo, ordenBase, sortingLayerCanvas);
  }

  public static void AplicarOrdenBase(GameObject objetivo, int ordenBase, string sortingLayerCanvas = null)
  {
    if (objetivo == null)
    {
      return;
    }

    int? canvasLayerId = null;
    if (!string.IsNullOrEmpty(sortingLayerCanvas))
    {
      int layerId = SortingLayer.NameToID(sortingLayerCanvas);
      if (layerId != 0 || sortingLayerCanvas == "Default")
      {
        canvasLayerId = layerId;
      }
    }

    foreach (Canvas canvas in objetivo.GetComponentsInChildren<Canvas>(true))
    {
      canvas.overrideSorting = true;
      if (canvasLayerId.HasValue)
      {
        canvas.sortingLayerID = canvasLayerId.Value;
      }
      int offset = 0;
      CanvasSortingOffset offsetMarker = canvas.GetComponent<CanvasSortingOffset>();
      if (offsetMarker != null)
      {
        offset = offsetMarker.offset;
      }
      canvas.sortingOrder = ordenBase + offset;
    }

    foreach (SortingGroup sg in objetivo.GetComponentsInChildren<SortingGroup>(true))
    {
      if (!SortingGroupOffsets.ContainsKey(sg))
      {
        SortingGroupOffsets[sg] = sg.sortingOrder;
      }
      sg.sortingOrder = ordenBase + SortingGroupOffsets[sg];
    }

    foreach (Renderer renderer in objetivo.GetComponentsInChildren<Renderer>(true))
    {
      if (!RendererOffsets.ContainsKey(renderer))
      {
        RendererOffsets[renderer] = renderer.sortingOrder;
      }
      renderer.sortingOrder = ordenBase + RendererOffsets[renderer];
    }
  }

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

  public static void OrdenarCanvasEncima(Canvas canvas, Transform objetivo, int offset = 5)
  {
    if (canvas == null || objetivo == null)
    {
      return;
    }

    int ordenBase = ObtenerOrdenActual(objetivo, canvas);
    int? layerId = ObtenerSortingLayerId(objetivo, canvas);

    canvas.overrideSorting = true;
    if (layerId.HasValue)
    {
      canvas.sortingLayerID = layerId.Value;
    }
    canvas.sortingOrder = ordenBase + offset;

    CanvasSortingOffset offsetMarker = canvas.GetComponent<CanvasSortingOffset>();
    if (offsetMarker == null)
    {
      offsetMarker = canvas.gameObject.AddComponent<CanvasSortingOffset>();
    }
    offsetMarker.offset = offset;
  }

  public static void OrdenarCanvasEncima(Canvas canvas, GameObject objetivo, int offset = 5)
  {
    if (objetivo == null)
    {
      return;
    }

    OrdenarCanvasEncima(canvas, objetivo.transform, offset);
  }

  public static void OrdenarCanvasEncima(Canvas canvas, Component objetivo, int offset = 5)
  {
    if (objetivo == null)
    {
      return;
    }

    OrdenarCanvasEncima(canvas, objetivo.transform, offset);
  }

  private static int ObtenerOrdenActual(Transform objetivo, Canvas canvasIgnorar = null)
  {
    if (objetivo == null)
    {
      return 0;
    }

    foreach (Canvas canvas in objetivo.GetComponentsInChildren<Canvas>(true))
    {
      if (canvasIgnorar != null && canvas == canvasIgnorar)
      {
        continue;
      }
      return canvas.sortingOrder;
    }

    SortingGroup sg = objetivo.GetComponentInChildren<SortingGroup>(true);
    if (sg != null)
    {
      return sg.sortingOrder;
    }

    Renderer rend = objetivo.GetComponentInChildren<Renderer>(true);
    if (rend != null)
    {
      return rend.sortingOrder;
    }

    return 0;
  }

  private static int? ObtenerSortingLayerId(Transform objetivo, Canvas canvasIgnorar = null)
  {
    if (objetivo == null)
    {
      return null;
    }

    foreach (Canvas canvas in objetivo.GetComponentsInChildren<Canvas>(true))
    {
      if (canvasIgnorar != null && canvas == canvasIgnorar)
      {
        continue;
      }
      return canvas.sortingLayerID;
    }

    SortingGroup sg = objetivo.GetComponentInChildren<SortingGroup>(true);
    if (sg != null)
    {
      return sg.sortingLayerID;
    }

    Renderer rend = objetivo.GetComponentInChildren<Renderer>(true);
    if (rend != null)
    {
      return rend.sortingLayerID;
    }

    return null;
  }
}
