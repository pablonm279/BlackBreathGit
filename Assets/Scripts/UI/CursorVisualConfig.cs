using UnityEngine;

[CreateAssetMenu(fileName = "CursorVisualConfig", menuName = "Configuracion/Cursor visual")]
public class CursorVisualConfig : ScriptableObject
{
  [Header("Texturas")]
  [Tooltip("Cursor usado normalmente. Si queda vacio, se usa el cursor del sistema operativo.")]
  [SerializeField] private Texture2D cursorNormal;
  [Tooltip("Cursor mostrado sobre botones y otros elementos interactuables.")]
  [SerializeField] private Texture2D cursorInteractivo;
  [Tooltip("Cursor mostrado mientras se mantiene presionado el boton izquierdo.")]
  [SerializeField] private Texture2D cursorPresionado;
  [Tooltip("Cursor especial para alertas de campania.")]
  [SerializeField] private Texture2D cursorAlerta;

  [Header("Hotspots")]
  [SerializeField] private Vector2 hotspotNormal = Vector2.zero;
  [SerializeField] private Vector2 hotspotInteractivo = Vector2.zero;
  [SerializeField] private Vector2 hotspotPresionado = Vector2.zero;
  [SerializeField] private Vector2 hotspotAlerta = Vector2.zero;

  [Header("Renderizado")]
  [SerializeField] private CursorMode modoCursor = CursorMode.Auto;

  public CursorMode ModoCursor => modoCursor;

  public Texture2D ObtenerTextura(EstadoCursorVisual estado)
  {
    switch (estado)
    {
      case EstadoCursorVisual.Interactivo:
        return cursorInteractivo != null ? cursorInteractivo : cursorNormal;
      case EstadoCursorVisual.Presionado:
        return cursorPresionado != null
          ? cursorPresionado
          : cursorInteractivo != null ? cursorInteractivo : cursorNormal;
      case EstadoCursorVisual.Alerta:
        return cursorAlerta != null ? cursorAlerta : cursorNormal;
      default:
        return cursorNormal;
    }
  }

  public Vector2 ObtenerHotspot(EstadoCursorVisual estado)
  {
    switch (estado)
    {
      case EstadoCursorVisual.Interactivo:
        return cursorInteractivo != null ? hotspotInteractivo : hotspotNormal;
      case EstadoCursorVisual.Presionado:
        if (cursorPresionado != null) return hotspotPresionado;
        return cursorInteractivo != null ? hotspotInteractivo : hotspotNormal;
      case EstadoCursorVisual.Alerta:
        return cursorAlerta != null ? hotspotAlerta : hotspotNormal;
      default:
        return hotspotNormal;
    }
  }
}

public enum EstadoCursorVisual
{
  Normal,
  Interactivo,
  Presionado,
  Alerta
}
