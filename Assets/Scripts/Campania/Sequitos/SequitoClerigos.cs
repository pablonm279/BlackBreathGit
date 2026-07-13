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
     [SerializeField] TextMeshProUGUI txtPlegaria;

    [SerializeField] TextMeshProUGUI costooroplegaria;

    int zonaIdUltimaPlegaria = -1;



    void OnEnable()
    {
        Actualizar();
    }

    void Actualizar()
    {


        txtDesc.text = TRADU.i.Traducir("Los Clérigos del Sol Radiante Purificador participaron como apoyo en el combate contra el Liche. La mayoría murieron en la onda expansiva en ese momento, pero todavía quedan algunos grupos tratando de llegar al puerto y sobrevivir mientras luchan por retrasar al Aliento Negro.\n\n");
        txtMecanica.text = TRADU.i.Traducir("EFECTOS PASIVOS:\n\n-Otorgan 15 Esperanza al unirse a la Caravana, -20 Esperanza al perderse.\n\n-20% probabilidades de Retrasar el Aliento Negro en cada viaje.\n\n-Si el Aliento Negro llega a distancia menor a 0, los Clérigos mueren.");
        txtPlegaria.text = TRADU.i.Traducir("La Plegaria retrasará 1 el Aliento Negro y bendecirá a los personajes por 3 días. 1 vez por región.");

        int oroActual = CampaignManager.Instance.GetOroActuales(); // Ajusta según tu implementación real
        int costo = 250;


        if (YaSeHizoPlegariaEnRegionActual())
        {
            costooroplegaria.text = TRADU.i.Traducir("<color=red>La plegaria ya fue realizada.</color>");
        }
        else
        {
            if (oroActual < costo)
            {
                costooroplegaria.text = TRADU.i.Traducir("<color=red>No hay oro suficiente para una donación de 150 Oro.</color>")  ;
            }
            else
            {
                costooroplegaria.text = TRADU.i.Traducir("Se hará una donación de 150 Oro.");
            }
        }
    }

    public void HacerPlegaria()
    {
        int oroActual = CampaignManager.Instance.GetOroActuales();
        if (!YaSeHizoPlegariaEnRegionActual() && oroActual >= 150)
        {
            CampaignManager.Instance.CambiarOroActual(-150);
            CampaignManager.Instance.CambiarEsperanzaActual(10);
            CampaignManager.Instance.CambiarValorAlientoNegro(-1);
            List<Personaje> personajes = CampaignManager.Instance.scMenuPersonajes.listaPersonajes;

            foreach (Personaje personaje in personajes)
            {
                personaje.AgregarCampBendecido(3);
            }
             zonaIdUltimaPlegaria = ObtenerZonaActualId();
        }

       
        Actualizar();
    }

    public void EcharSequito()
    {
        //--- Destruye el séquito de artistas
        CampaignManager.Instance.scMenuSequito.RemoverSequito(10);
        Destroy(gameObject);
    }

    int ObtenerZonaActualId()
    {
        if (CampaignManager.Instance == null || CampaignManager.Instance.scAtributosZona == null)
        {
            return -1;
        }

        return CampaignManager.Instance.scAtributosZona.ID;
    }

    bool YaSeHizoPlegariaEnRegionActual()
    {
        int zonaActualId = ObtenerZonaActualId();
        return zonaActualId > 0 && zonaIdUltimaPlegaria == zonaActualId;
    }

    public int ObtenerZonaIdUltimaPlegaria()
    {
        return zonaIdUltimaPlegaria;
    }

    public void RestaurarZonaIdUltimaPlegaria(int zonaId)
    {
        zonaIdUltimaPlegaria = zonaId > 0 ? zonaId : -1;
        Actualizar();
    }
}



