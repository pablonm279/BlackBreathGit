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
        txtMecanica.text = TRADU.i.nIdioma switch
        {
            TRADU.IdiomaIngles => "PASSIVE EFFECTS:\n\n-Grant 15 Hope when joining the Caravan, -20 Hope when lost.\n\n-20% chance on each journey to prevent the Black Breath from accumulating 5 h.\n\n-If the Black Breath reaches a distance below 0 h, the Clerics die.",
            TRADU.IdiomaPortugues => "EFEITOS PASSIVOS:\n\n-Concedem 15 de Esperança ao entrar na Caravana, -20 de Esperança ao serem perdidos.\n\n-20% de chance em cada viagem de impedir que o Sopro Negro acumule 5 h.\n\n-Se o Sopro Negro chegar a uma distância menor que 0 h, os Clérigos morrem.",
            _ => "EFECTOS PASIVOS:\n\n-Otorgan 15 Esperanza al unirse a la Caravana, -20 Esperanza al perderse.\n\n-20% de probabilidades en cada viaje de evitar que el Aliento Negro acumule 5 h.\n\n-Si el Aliento Negro llega a una distancia menor a 0 h, los Clérigos mueren."
        };
        txtPlegaria.text = TRADU.i.nIdioma switch
        {
            TRADU.IdiomaIngles => "The Prayer pushes the Black Breath back 5 h and blesses the characters for 72 h. Once per region.",
            TRADU.IdiomaPortugues => "A Oração afasta o Sopro Negro em 5 h e abençoa os personagens por 72 h. Uma vez por região.",
            _ => "La Plegaria retrasa 5 h el Aliento Negro y bendice a los personajes por 72 h. Una vez por región."
        };

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
            CampaignManager.Instance.CambiarValorAlientoNegroHoras(-5f);
            List<Personaje> personajes = CampaignManager.Instance.scMenuPersonajes.listaPersonajes;

            foreach (Personaje personaje in personajes)
            {
                personaje.AplicarBendecidoHoras(72f);
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



