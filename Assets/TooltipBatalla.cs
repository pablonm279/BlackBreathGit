using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipBatalla : MonoBehaviour
{
    public static TooltipBatalla Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform backgroundRectTransform;
    [SerializeField] private Vector2 backgroundPadding = new Vector2(26f, 18f);
    [SerializeField] private Vector2 backgroundMinSize = new Vector2(140f, 48f);
    [SerializeField] private float preferredTextMaxWidth = 260f;
    private UIFadeSlide tooltipAnim;

    public bool tooltipFade = false;
    void Awake()
    {
        Instance = this;
        if (tooltipObject == null) { return; }

        if (backgroundRectTransform == null)
        {
            Transform fondo = tooltipObject.transform.Find("FondoTooltip");
            if (fondo != null)
            {
                backgroundRectTransform = fondo as RectTransform;
            }
        }

        tooltipAnim = UIFadeSlideUtility.Ensure(tooltipObject);
        if (tooltipAnim != null)
        {
            tooltipAnim.SetDurations(0.14f, 0.14f);
            tooltipAnim.SetOffsets(new Vector2(0f, -10f), new Vector2(0f, -8f));
            tooltipAnim.SetFollowMouse(true, new Vector2(14f, -18f));
            tooltipAnim.HideImmediate();
        }
        else
        {
            tooltipObject.SetActive(false);
        }
    }

    public void ShowTooltip(int tipo)
    {
        if (tooltipObject == null) { return; }

        desdeBarraVida = false;

        UIFadeSlideUtility.ShowAt(tooltipObject, Input.mousePosition);
        tooltipText.text = TRADU.i.Traducir(ObtenerContenidoTooltip(tipo));
        AjustarTamanoTooltip();

        if (tooltipFade)
        {
            Invoke("HideTooltip", 3f);
        }
    }

    bool desdeBarraVida = false;

    public void ShowTooltipText(string txt)
    {
        if (tooltipObject == null) { return; }

        desdeBarraVida = true;
        UIFadeSlideUtility.ShowAt(tooltipObject, Input.mousePosition);
        tooltipText.text = TRADU.i.Traducir(txt);
        AjustarTamanoTooltip();
    }

    public void ShowTooltipTextSinAnim(string txt)
    {
        if (tooltipObject == null) { return; }

        desdeBarraVida = false;
        UIFadeSlideUtility.ShowAtImmediate(tooltipObject, Input.mousePosition);
        tooltipText.text = TRADU.i.Traducir(txt);
        AjustarTamanoTooltip();
    }

    public void ShowTooltipTextSinAnimDirecto(string txt)
    {
        if (tooltipObject == null) { return; }

        desdeBarraVida = false;
        UIFadeSlideUtility.ShowAtImmediate(tooltipObject, Input.mousePosition);
        tooltipText.text = txt;
        AjustarTamanoTooltip();
    }

    private void AjustarTamanoTooltip()
    {
        if (tooltipText == null)
        {
            return;
        }

        tooltipText.enableWordWrapping = true;
        tooltipText.ForceMeshUpdate();

        Vector2 tamanoTexto = tooltipText.GetPreferredValues(tooltipText.text, preferredTextMaxWidth, Mathf.Infinity);
        Vector2 tamanoFondo = new Vector2(
            Mathf.Max(backgroundMinSize.x, tamanoTexto.x + backgroundPadding.x),
            Mathf.Max(backgroundMinSize.y, tamanoTexto.y + backgroundPadding.y)
        );

        RectTransform rectFondo = backgroundRectTransform != null ? backgroundRectTransform : tooltipObject.GetComponent<RectTransform>();
        if (rectFondo != null)
        {
            rectFondo.sizeDelta = tamanoFondo;
        }
    }

    private string ObtenerContenidoTooltip(int tipo)
    {
        switch (tipo)
        {
            case 1:
                return "Defensa: determina capacidad para evadir ataques.";
            case 2:
                return "Armadura: reduce el daño físico recibido.";
            case 3:
                return "Reflejos: resistencia a determinados efectos de ataques.";
            case 4:
                return "Fortaleza: resistencia a efectos físicos.";
            case 5:
                return "Mental: resistencia a efectos mentales.";
            case 6:
                return "Valentía: moral general en combate.";
            case 7:
                return "Resistencia al Fuego: Cantidad de daño que previene.";
            case 8:
                return "Resistencia al Frío: Cantidad de daño que previene.";
            case 9:
                return "Resistencia al Rayo: Cantidad de daño que previene.";
            case 10:
                return "Resistencia al Ácido: Cantidad de daño que previene.";
            case 11:
                return "Resistencia Arcana: Cantidad de daño que previene.";
            case 12:
                return "Resistencia Necrótica: Cantidad de daño que previene.";
            case 13:
                return "Resistencia Divina: Cantidad de daño que previene.";
            case 14:
                return "Residuo Energético: Otorga daño arcano y hiere levemente.";
            case 15:
                return "Zona bajo Vigilancia del Explorador.";
            case 16:
                return "Añade daño fuego al Explorador si está adyacente.";
            case 17:
                return "Abrojos: Inflige daño y puede desangrar.";
            case 18:
                return "Eco Divino: Cura a aliados y daña a enemigos.";
            case 19:
                return "Humo: Esconde a los personajes dentro.";
            case 20:
                return "Escudo de Fe: Protege a los aliados dentro.";
            case 21:
                return "Masa Contaminada: Hace daño ácido. Potencia enemigos corruptos.";
            case 22:
                return "Pinchos: Daña a enemigos que los pisen.";
            case 23:
                return "Barricada: Obstáculo para enemigos. Hiere al ser atacada.";
            case 24:
                return "Puesto de Tiro: Aumenta ataque y defensa a aliados dentro.";
            case 25:
                return "Pilar de Luz: Obstáculo que daña a enemigos al ser atacado.";
            case 26:
                return "Trampa Improvisada: Daña y marca a unidades que la pisen.";
            case 27:
                return "Restos de Aliento: Potencia y cura a los Vengadores de Kadryn.";
            case 28:
                return "Primer Golpe: el Alabardero ataca a la primera unidad que entra en la casilla.";
            case 29:
                return "Refuerzos aliados disponibles, irán uniéndose a la batalla gradualmente.";
            case 30:
                return "Refuerzos enemigos disponibles, irán uniéndose a la batalla gradualmente.";
            case 31:
                return "Llamas: infligen daño fuego a unidades que entren en la casilla.";
            case 32:
                return "Barro: reduce 2 PA a unidades que entren en la casilla.";
            case 33:
                return "Modo Rápido";
            case 34:
                return "Mirada de Masacre: al moverse aquí, Tirada de salvación mental CD 13 o se pierde el turno.";
            case 35:
                return "";
            default:
                return "Tooltip desconocido";
        }
    }

    public void HideTooltip()
    {
        UIFadeSlideUtility.Hide(tooltipObject);
        desdeBarraVida = false;
    }

    public void HideTooltipSinAnim()
    {
        UIFadeSlideUtility.HideImmediate(tooltipObject);
        desdeBarraVida = false;
    }

    void LateUpdate()
    {
        if (!desdeBarraVida) { return; }
        if (tooltipObject == null) { return; }

        if (tooltipAnim == null)
        {
            tooltipObject.transform.position = Input.mousePosition;
        }
    }
}



