using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoCronistas : MonoBehaviour
{
    //---
    [SerializeField] TextMeshProUGUI txtDesc;

    [SerializeField] TextMeshProUGUI txtMecanica;

    [SerializeField] GameObject btnVender;


    public int progresoCronica = 150;
    public int valorCambiosCronicas = 0;
    public bool yaVendioCronica = false;


    void OnEnable()
    {
        Actualizar();
    }

    void Actualizar()
    {

        progresoCronica = 150 + (1 * CampaignManager.Instance.GetEsperanzaActual()) + valorCambiosCronicas;

        txtDesc.text = $"Un grupo de eruditos unidos que se dedican a registrar los sucesos del viaje de la caravana hacia el puerto. Sus escrituras pueden ser una fuenta de ingresos y moral, pero también puede ser contraproducente en los peores momentos.\n\n";
        txtMecanica.text = $"EFECTOS PASIVOS:\n\n-Otorgan +5 de Esperanza por batallas ganadas (-3 Derrotas). ";
        if (yaVendioCronica)
        {
            txtMecanica.text += "\n\n-Ya se ha vendido la crónica de este viaje.";
        }
        else
        {
            txtMecanica.text += 
                "\n\n- Crónica: Acumula valor de la siguiente manera:" +
                "\n   • Base: 150 Oro" +
                "\n   • +1 Oro por cada punto de Esperanza" +
                "\n   • +20 Oro por cada nodo viajado" +
                "\n   • +50 Oro por cada batalla ganada / -50 Oro por cada batalla perdida" +
                "\n\nSe puede vender en Asentamientos o Puestos Comerciales.";
            txtMecanica.text += $"\n\n\n\n-Valor Crónica: {progresoCronica} Oro";
        }

       

        //Si en asentamiento o puesto comercial
        if (((CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 4) || (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 6)) && !yaVendioCronica)
        {
            btnVender.SetActive(true);
        }
        else
        { 
            btnVender.SetActive(false);
        }


       
    }



    public void EcharSequito()
    {
        //--- Destruye el séquito de desertores
        CampaignManager.Instance.scMenuSequito.RemoverSequito(7);
        Destroy(gameObject);
    }
    public void VenderCronica()
    {
        //--- Vende la crónica
        if (!yaVendioCronica)
        {
            CampaignManager.Instance.CambiarOroActual(progresoCronica);
            CampaignManager.Instance.EscribirLog($"-Se ha vendido la crónica del viaje por {progresoCronica} de Oro.");
            yaVendioCronica = true;
            Actualizar();
        }
    }
    
}