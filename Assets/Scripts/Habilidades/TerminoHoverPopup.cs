using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Popup flotante que muestra la descripcion de un termino de juego al hacer hover.
// Se construye por codigo (no depende de un prefab) y vive como singleton persistente.
public class TerminoHoverPopup : MonoBehaviour
{
  private const float AnchoMaximo = 260f;
  private const float AltoMaximo = 220f;
  private const float MargenPantalla = 12f;
  private const float DesplazamientoCursorX = 18f;
  private const float DesplazamientoCursorY = 26f;
  private const float PaddingHorizontal = 10f;
  private const float PaddingVertical = 7f;
  private const int OrdenRenderizado = 5000;
  private const string RutaFuente = "Fuentes/Cardo/Cardo-Regular SDF";

  private static TerminoHoverPopup _instancia;

  public static TerminoHoverPopup Instancia
  {
    get
    {
      if (_instancia == null)
      {
        _instancia = Crear();
      }

      return _instancia;
    }
  }

  private RectTransform rectPanel;
  private TextMeshProUGUI texto;

  private static TerminoHoverPopup Crear()
  {
    GameObject raiz = new GameObject("TerminoHoverPopup(Auto)");
    Object.DontDestroyOnLoad(raiz);

    Canvas canvas = raiz.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = OrdenRenderizado;

    CanvasScaler scaler = raiz.AddComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

    GameObject panelGO = new GameObject("Panel");
    panelGO.transform.SetParent(raiz.transform, false);
    RectTransform rectPanelLocal = panelGO.AddComponent<RectTransform>();
    // Pivot arriba-izquierda: la posicion que le asignamos es la esquina superior
    // izquierda del panel, y crece hacia abajo-derecha (como un tooltip de mouse comun).
    rectPanelLocal.anchorMin = new Vector2(0f, 1f);
    rectPanelLocal.anchorMax = new Vector2(0f, 1f);
    rectPanelLocal.pivot = new Vector2(0f, 1f);

    Image fondo = panelGO.AddComponent<Image>();
    fondo.color = new Color(0f, 0f, 0f, 1f);
    fondo.raycastTarget = false;

    GameObject textoGO = new GameObject("Texto");
    textoGO.transform.SetParent(panelGO.transform, false);
    RectTransform rectTexto = textoGO.AddComponent<RectTransform>();
    rectTexto.anchorMin = Vector2.zero;
    rectTexto.anchorMax = Vector2.one;
    rectTexto.offsetMin = new Vector2(PaddingHorizontal, PaddingVertical);
    rectTexto.offsetMax = new Vector2(-PaddingHorizontal, -PaddingVertical);

    TextMeshProUGUI tmp = textoGO.AddComponent<TextMeshProUGUI>();
    tmp.fontSize = 17f;
    tmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);
    tmp.raycastTarget = false;
    tmp.enableWordWrapping = true;
    tmp.richText = true;
    tmp.overflowMode = TextOverflowModes.Ellipsis;

    TMP_FontAsset fuenteCardo = Resources.Load<TMP_FontAsset>(RutaFuente);
    if (fuenteCardo != null)
    {
      tmp.font = fuenteCardo;
    }

    TerminoHoverPopup componente = raiz.AddComponent<TerminoHoverPopup>();
    componente.rectPanel = rectPanelLocal;
    componente.texto = tmp;

    panelGO.SetActive(false);

    return componente;
  }

  public void Mostrar(string textoDescripcion, Vector2 posicionPantalla)
  {
    if (string.IsNullOrEmpty(textoDescripcion))
    {
      Ocultar();
      return;
    }

    texto.text = textoDescripcion;
    rectPanel.gameObject.SetActive(true);

    Vector2 tamanoPreferido = texto.GetPreferredValues(textoDescripcion, AnchoMaximo - (PaddingHorizontal * 2f), 0f);
    float ancho = Mathf.Min(AnchoMaximo, tamanoPreferido.x + (PaddingHorizontal * 2f));
    float alto = Mathf.Min(AltoMaximo, tamanoPreferido.y + (PaddingVertical * 2f));
    rectPanel.sizeDelta = new Vector2(ancho, alto);

    PosicionarCercaDe(posicionPantalla, ancho, alto);
  }

  public void Ocultar()
  {
    if (rectPanel != null)
    {
      rectPanel.gameObject.SetActive(false);
    }
  }

  private void PosicionarCercaDe(Vector2 posicionPantalla, float ancho, float alto)
  {
    // Truco propio de canvases Screen Space Overlay: el RectTransform.position de un
    // elemento bajo un canvas Overlay se puede asignar directamente en pixeles de pantalla,
    // sin pasar por conversiones de espacio local (evita el bug de posicion fija anterior).
    float x = posicionPantalla.x + DesplazamientoCursorX;
    float y = posicionPantalla.y - DesplazamientoCursorY;

    x = Mathf.Clamp(x, MargenPantalla, Mathf.Max(MargenPantalla, Screen.width - MargenPantalla - ancho));
    y = Mathf.Clamp(y, Mathf.Min(Screen.height - MargenPantalla, MargenPantalla + alto), Screen.height - MargenPantalla);

    rectPanel.position = new Vector3(x, y, 0f);
  }
}
