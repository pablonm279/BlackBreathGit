using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra un unico tooltip para los elementos de la pantalla de prepartida.
/// El panel puede ser cualquier objeto UI dentro del Canvas.
/// </summary>
public class PrePartidaTooltipManager : MonoBehaviour
{
    [Header("UI del tooltip")]
    [SerializeField] private GameObject panelTooltip;
    [SerializeField] private TMP_Text textoTooltip;
    [SerializeField] private RectTransform rectTooltip;
    [SerializeField] private Canvas canvasTooltip;

    [Header("Posicion")]
    [SerializeField] private Vector2 offset = new Vector2(18f, -18f);
    [SerializeField, Min(0f)] private float margenPantalla = 8f;

    private CanvasGroup canvasGroupTooltip;
    private PrePartidaTooltip tooltipActual;
    private int idiomaMostrado = -1;

    private void Reset()
    {
        AutoVincularReferencias();
    }

    private void Awake()
    {
        AutoVincularReferencias();
        Ocultar();
    }

    private void Update()
    {
        if (tooltipActual == null)
        {
            return;
        }

        if (idiomaMostrado != tooltipActual.ObtenerIdiomaActual())
        {
            ActualizarTexto(tooltipActual);
        }

        PosicionarTooltip();
    }

    public void Mostrar(PrePartidaTooltip tooltip)
    {
        if (tooltip == null)
        {
            return;
        }

        if (panelTooltip == null || textoTooltip == null || rectTooltip == null)
        {
            Debug.LogWarning("[PrePartidaTooltip] Faltan referencias del panel o del texto.", this);
            return;
        }

        tooltipActual = tooltip;
        ActualizarTexto(tooltip);
        panelTooltip.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTooltip);
        PosicionarTooltip();
    }

    public void Ocultar(PrePartidaTooltip tooltip)
    {
        if (tooltipActual == tooltip)
        {
            Ocultar();
        }
    }

    public void Ocultar()
    {
        tooltipActual = null;
        idiomaMostrado = -1;

        if (panelTooltip != null)
        {
            panelTooltip.SetActive(false);
        }
    }

    private void ActualizarTexto(PrePartidaTooltip tooltip)
    {
        if (textoTooltip == null || tooltip == null)
        {
            return;
        }

        textoTooltip.text = tooltip.ObtenerTextoActual();
        idiomaMostrado = tooltip.ObtenerIdiomaActual();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTooltip);
    }

    private void PosicionarTooltip()
    {
        if (rectTooltip == null || canvasTooltip == null || !panelTooltip.activeInHierarchy)
        {
            return;
        }

        RectTransform parentRect = rectTooltip.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        Camera eventCamera = canvasTooltip.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvasTooltip.worldCamera;

        Vector2 tamanoEnPantalla = rectTooltip.rect.size * Mathf.Max(0.01f, canvasTooltip.scaleFactor);
        Vector2 pivot = rectTooltip.pivot;
        Vector2 posicion = (Vector2)Input.mousePosition + offset;
        float margen = Mathf.Max(0f, margenPantalla);

        posicion.x = Mathf.Clamp(
            posicion.x,
            tamanoEnPantalla.x * pivot.x + margen,
            Screen.width - tamanoEnPantalla.x * (1f - pivot.x) - margen);
        posicion.y = Mathf.Clamp(
            posicion.y,
            tamanoEnPantalla.y * pivot.y + margen,
            Screen.height - tamanoEnPantalla.y * (1f - pivot.y) - margen);

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, posicion, eventCamera, out Vector3 posicionMundo))
        {
            rectTooltip.position = posicionMundo;
        }
    }

    private void AutoVincularReferencias()
    {
        if (panelTooltip != null)
        {
            if (rectTooltip == null)
            {
                rectTooltip = panelTooltip.GetComponent<RectTransform>();
            }

            if (textoTooltip == null)
            {
                textoTooltip = panelTooltip.GetComponentInChildren<TMP_Text>(true);
            }

            canvasGroupTooltip = panelTooltip.GetComponent<CanvasGroup>();
            if (canvasGroupTooltip == null)
            {
                canvasGroupTooltip = panelTooltip.AddComponent<CanvasGroup>();
            }

            canvasGroupTooltip.interactable = false;
            canvasGroupTooltip.blocksRaycasts = false;
        }

        if (canvasTooltip == null && panelTooltip != null)
        {
            canvasTooltip = panelTooltip.GetComponentInParent<Canvas>();
            if (canvasTooltip != null)
            {
                canvasTooltip = canvasTooltip.rootCanvas;
            }
        }
    }
}
