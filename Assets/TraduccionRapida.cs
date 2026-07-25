using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TraduccionRapida : MonoBehaviour
{
    [Header("Textos por idioma")]
    [SerializeField, InspectorName("Español"), TextArea(2, 6)] private string textoEspanol;
    [SerializeField, InspectorName("Inglés"), TextArea(2, 6)] private string textoIngles;
    [SerializeField, InspectorName("Portugués (BR)"), TextArea(2, 6)] private string textoPortugues;

    private TMP_Text textoAsociado;

    private void Awake()
    {
        ObtenerTextoAsociado();
    }

    private void OnEnable()
    {
        TRADU.IdiomaActualizado += AplicarTexto;
        AplicarTexto(ObtenerIdiomaActual());
    }

    private void OnDisable()
    {
        TRADU.IdiomaActualizado -= AplicarTexto;
    }

    private void Reset()
    {
        ObtenerTextoAsociado();
        if (textoAsociado != null)
        {
            textoEspanol = textoAsociado.text;
        }
    }

    private void AplicarTexto(int idioma)
    {
        if (!ObtenerTextoAsociado())
        {
            return;
        }

        switch (idioma)
        {
            case TRADU.IdiomaEspanol:
                textoAsociado.text = textoEspanol;
                break;
            case TRADU.IdiomaPortugues:
                textoAsociado.text = textoPortugues;
                break;
            default:
                textoAsociado.text = textoIngles;
                break;
        }
    }

    private static int ObtenerIdiomaActual()
    {
        int idioma = TRADU.i != null
            ? TRADU.i.nIdioma
            : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaIngles);

        return idioma == TRADU.IdiomaEspanol
            || idioma == TRADU.IdiomaIngles
            || idioma == TRADU.IdiomaPortugues
                ? idioma
                : TRADU.IdiomaIngles;
    }

    private bool ObtenerTextoAsociado()
    {
        if (textoAsociado == null)
        {
            textoAsociado = GetComponent<TMP_Text>();
        }

        if (textoAsociado != null)
        {
            return true;
        }

        Debug.LogError(
            "TraduccionRapida necesita un componente TextMeshPro en el mismo GameObject.",
            this);
        enabled = false;
        return false;
    }
}
