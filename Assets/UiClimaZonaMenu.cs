using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiClimaZonaMenu : MonoBehaviour
{
    [SerializeField] private Image iconoClima;
    [SerializeField] private TMP_Text textoChances;
    [SerializeField] private PrePartidaTooltip tooltip;

    private void Awake()
    {
        AutoVincular();
    }

    public void Configurar(Sprite icono, int chances, string textoTooltip, bool traducirTooltip)
    {
        AutoVincular();

        if (iconoClima != null)
        {
            iconoClima.sprite = icono;
            iconoClima.enabled = icono != null;
            iconoClima.preserveAspect = true;
            iconoClima.raycastTarget = true;
        }

        if (textoChances != null)
        {
            textoChances.text = Mathf.Clamp(chances, 0, 100) + "%";
        }

        if (tooltip == null)
        {
            tooltip = gameObject.AddComponent<PrePartidaTooltip>();
        }

        tooltip.ConfigurarTexto(textoTooltip, traducirTextoEspanol: traducirTooltip);
    }

    private void AutoVincular()
    {
        if (iconoClima == null)
        {
            iconoClima = GetComponent<Image>();
        }

        if (textoChances == null)
        {
            TMP_Text[] textos = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < textos.Length; i++)
            {
                TMP_Text candidato = textos[i];
                if (candidato == null)
                {
                    continue;
                }

                if (string.Equals(candidato.gameObject.name, "Chances", System.StringComparison.OrdinalIgnoreCase))
                {
                    textoChances = candidato;
                    break;
                }
            }

            if (textoChances == null && textos.Length > 0)
            {
                textoChances = textos[0];
            }
        }

        if (tooltip == null)
        {
            tooltip = GetComponent<PrePartidaTooltip>();
        }
    }
}
