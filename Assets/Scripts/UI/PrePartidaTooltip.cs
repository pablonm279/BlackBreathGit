using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tooltip que se agrega a cualquier elemento UI de la pantalla de prepartida.
/// </summary>
public class PrePartidaTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Texto del tooltip")]
    [TextArea(2, 6)]
    [SerializeField] private string textoEspanol;
    [TextArea(2, 6)]
    [SerializeField] private string textoIngles;
    [TextArea(2, 6)]
    [SerializeField] private string textoPortugues;

    [Header("Referencia opcional")]
    [SerializeField] private PrePartidaTooltipManager manager;

    private void Awake()
    {
        BuscarManager();
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.Ocultar(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BuscarManager();
        if (manager != null)
        {
            manager.Mostrar(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.Ocultar(this);
        }
    }

    public string ObtenerTextoActual()
    {
        string textoSeleccionado;
        switch (ObtenerIdiomaActual())
        {
            case TRADU.IdiomaIngles:
                textoSeleccionado = textoIngles;
                break;
            case TRADU.IdiomaPortugues:
                textoSeleccionado = textoPortugues;
                break;
            default:
                textoSeleccionado = textoEspanol;
                break;
        }

        if (!string.IsNullOrWhiteSpace(textoSeleccionado))
        {
            return textoSeleccionado;
        }

        return !string.IsNullOrWhiteSpace(textoEspanol) ? textoEspanol : textoSeleccionado;
    }

    public int ObtenerIdiomaActual()
    {
        int idioma = TRADU.i != null
            ? TRADU.i.nIdioma
            : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);

        if (idioma != TRADU.IdiomaIngles && idioma != TRADU.IdiomaPortugues)
        {
            return TRADU.IdiomaEspanol;
        }

        return idioma;
    }

    private void BuscarManager()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<PrePartidaTooltipManager>(FindObjectsInactive.Include);
        }
    }
}
