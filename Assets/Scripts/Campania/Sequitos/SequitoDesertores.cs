using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoDesertores : MonoBehaviour
{
    //---
    [SerializeField] TextMeshProUGUI txtDesc;

    [SerializeField] TextMeshProUGUI txtMecanica;


    void OnEnable()
    {
        Actualizar();
    }

    void Actualizar()
    {
        txtDesc.text = $"Estos soldados abandonaron su puesto en el ejército en pos de sobrevivir. Hambrientos y avergonzados, ofrecen protección a la Caravana pidiendo solo un lugar en ella, aunque a una parte de los civiles les desagrade la idea.\n\n";
        txtMecanica.text = $"EFECTOS PASIVOS:\n\n-Participan en la defensa de la Caravana, reemplazando a los inexpertos Milicianos. \n\n-Otorga 10 Experiencia extra a Personajes que Entrenan. \n\n-Al aceptarlos la Esperanza disminuye en 8.";

    }



    public void EcharSequito()
    {
        //--- Destruye el séquito de desertores
        CampaignManager.Instance.scMenuSequito.RemoverSequito(6);
        Destroy(gameObject);
    }

    
}