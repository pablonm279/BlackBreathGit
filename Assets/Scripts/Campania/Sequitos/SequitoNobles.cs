using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoNobles : MonoBehaviour
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
        txtDesc.text = TRADU.i.Traducir("Un grupo de nobles que se vieron obligados a abandonar la comodidad de sus tierras, ahora viajan junto a la caravana. Si bien son quejosos y no son de gran utilidad, al menos donan periódicamente parte de su riqueza para asegurarse de que no serán abandonados.\n\n");
        txtMecanica.text = TRADU.i.Traducir($"EFECTOS PASIVOS:\n\n-Cada día donan Oro equivalente a 1/3 de la Esperanza.\n\n-Se pierde 2 de Esperanza al viajar con fatiga 4 o mayor.");

    }



    public void EcharSequito()
    {
        //--- Destruye el séquito de nobles
        CampaignManager.Instance.scMenuSequito.RemoverSequito(9);
        Destroy(gameObject);
    }

    
}