using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoClerigos : MonoBehaviour
{
    //---
    [SerializeField] TextMeshProUGUI txtDesc;

    [SerializeField] TextMeshProUGUI txtMecanica;

    [SerializeField] TextMeshProUGUI costooroplegaria;

    bool seHizoPlegaria = false;



    void OnEnable()
    {
        Actualizar();
    }

    void Actualizar()
    {


        txtDesc.text = $"Los Clérigos del Sol Radiante Purificador participaron como apoyo en el combate contra el Liche. La mayoría murieron en la onda expansiva en ese momento, pero todavía quedan algunos grupos tratando de llegar al puerto y sobrevivir mientras luchan por retrasar al Aliento Negro.\n\n";
        txtMecanica.text = $"EFECTOS PASIVOS:\n\n-Otorgan 15 Esperanza al unirse a la Caravana, -20 Esperanza al perderse.\n\n-20% probabilidades de Retrasar el Aliento Negro en cada viaje.\n\n-Si el Aliento Negro llega a nivel superior a 16, los Clérigos mueren.";

        int oroActual = CampaignManager.Instance.GetOroActuales(); // Ajusta según tu implementación real
        int costo = 250;


        if (seHizoPlegaria)
        {
            costooroplegaria.text = "<color=red>La plegaria ya fue realizada.</color>";
        }
        else
        {
            if (oroActual < costo)
            {
                costooroplegaria.text = "<color=red>No hay oro suficiente para una donación de 250 Oro.</color>";
            }
            else
            {
                costooroplegaria.text = "Se hará una donación de 250 Oro.";
            }
        }
    }

    public void HacerPlegaria()
    {
        int oroActual = CampaignManager.Instance.GetOroActuales();
        if (!seHizoPlegaria && oroActual >= 250)
        {
            CampaignManager.Instance.CambiarOroActual(-250);
            CampaignManager.Instance.CambiarEsperanzaActual(10);
            CampaignManager.Instance.CambiarValorAlientoNegro(-1);
            List<Personaje> personajes = CampaignManager.Instance.scMenuPersonajes.listaPersonajes;

            foreach (Personaje personaje in personajes)
            {
                personaje.Camp_Bendecido_SequitoClerigos = true;
            }
             seHizoPlegaria = true;
        }

       
        Actualizar();
    }

    public void EcharSequito()
    {
        //--- Destruye el séquito de artistas
        CampaignManager.Instance.scMenuSequito.RemoverSequito(10);
        Destroy(gameObject);
    }

 
}
