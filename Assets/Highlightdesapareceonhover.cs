using UnityEngine;
using UnityEngine.EventSystems;

public class Highlightdesapareceonhover : MonoBehaviour, IPointerEnterHandler
{
  [SerializeField] private GameObject objetivoADesactivar;
  private RectTransform rectTransform;
  private Canvas canvas;

  void Awake()
  {
    rectTransform = transform as RectTransform;
    canvas = GetComponentInParent<Canvas>();
  }

  void Update()
  {
    if (rectTransform == null)
    {
      return;
    }

    if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, ObtenerCamara()))
    {
      Ocultar();
    }
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    Ocultar();
  }

  Camera ObtenerCamara()
  {
    if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
    {
      return null;
    }

    return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
  }

  void Ocultar()
  {
    GameObject objetivo = objetivoADesactivar != null ? objetivoADesactivar : gameObject;
    objetivo.SetActive(false);
  }
}
