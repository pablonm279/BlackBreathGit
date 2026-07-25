using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoEsclavos : MonoBehaviour
{
    //---
    [SerializeField] TextMeshProUGUI txtDesc;
    [SerializeField] TextMeshProUGUI txtBotonLiberar;
    [SerializeField] TextMeshProUGUI txtConsec;

    [SerializeField] TextMeshProUGUI txtMecanica;

    void OnEnable()
    {
        Actualizar();
    }


    void Actualizar()
    {
        if (txtBotonLiberar != null)
        {
            txtBotonLiberar.text = TraducirBotonLiberar();
        }

        if (txtConsec != null)
        {
            txtConsec.text = TraducirConsec();
        }


        txtDesc.text = TRADU.i.Traducir("Han sido esclavos toda su vida, e incluso en estas circunstancias se comportan como tal. La situación amerita aprovecharse de su condición para obtener ventajas de mano de obra, ¿o quizás llegó el momento de liberarlos?\n\n");
        txtMecanica.text = TRADU.i.Traducir("EFECTOS PASIVOS:\n\n-Otorgan +50 Capacidad de Carga\n\n-Cada descanso juntan 10-15 Materiales.\n\n-Cada Viaje se pierden 2 de Esperanza.\n\n-Al ser liberados, se convierten en Civiles comunes y otorgan +25 Esperanza.");

      
    }
    private string TraducirBotonLiberar()
    {
        if (TRADU.i == null || TRADU.i.nIdioma == TRADU.IdiomaEspanol)
        {
            return "Liberar";
        }

        return TRADU.i.nIdioma == TRADU.IdiomaPortugues ? "Liberar" : "Release";
    }

    private string TraducirConsec()
    {
        if (TRADU.i == null || TRADU.i.nIdioma == TRADU.IdiomaEspanol)
        {
            return "Se perderá el séquito, pero se sumarán como civiles comunes a la caravana. +25 Esperanza";
        }

        return TRADU.i.nIdioma == TRADU.IdiomaPortugues
            ? "O séquito será perdido, mas eles se juntarão à caravana como civis comuns. +25 Esperança"
            : "The retinue will be lost, but they will join the caravan as common civilians. +25 Hope";
    }

    public void EcharSequito()
    {
        //Se remueven asi cuando son liberados, el metodo Removersequito() de scMenuSequitos.cs está para cuando son asesinados
        CampaignManager.Instance.scMenuSequito.lstSequitos.Remove(11);
        CampaignManager.Instance.scSequitoEsclavos = null;
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Esclavos han sido liberados y ahora son Civiles comunes. +25 Esperanza"));
        CampaignManager.Instance.CambiarEsperanzaActual(+25);
        CampaignManager.Instance.CambiarBueyesActuales(0); // Actualiza la capacidad de carga al liberar esclavos, 0 porque no cambia nada
       


        Destroy(gameObject);



    }
}


