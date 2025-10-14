using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipBatalla : MonoBehaviour
{
    public static TooltipBatalla Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;

    void Awake()
    {
        Instance = this;
        tooltipObject.SetActive(false);
    }

    public void ShowTooltip(int tipo)
    {
        tooltipObject.SetActive(true);
        desdeBarraVida = false;
      
            tooltipObject.transform.position = Input.mousePosition;
            tooltipText.text = ObtenerContenidoTooltip(tipo);
        
    }
    bool desdeBarraVida = false;
    public void ShowTooltipText(string txt)
    {
        tooltipObject.SetActive(true);
        desdeBarraVida = true;
        tooltipObject.transform.position = Input.mousePosition;
        tooltipText.text = txt;
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
                return "Valentía: recurso para habilidades especiales.";
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
                return "";
            case 27:
                return "";
            case 28:
                return "";
            case 29:
                return "";
            case 30:
                return "";
            case 31:
                return "";
            case 32:
                return "";
            case 33:
                return "";
            case 34:
                return "";
            case 35:
                return "";
            default:
                return "Tooltip desconocido";
        }
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
        desdeBarraVida = false;
    }

    void LateUpdate()
    {
        if (!desdeBarraVida) { return; }


        tooltipObject.transform.position = Input.mousePosition;
            
     Vector2 m = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
     if (tooltipObject.activeSelf && m.sqrMagnitude > 0f) HideTooltip();

        
    }
    


}
