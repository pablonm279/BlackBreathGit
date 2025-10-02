using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoRefugiados : MonoBehaviour
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
        txtDesc.text = $"Varios civiles que estuvieron a la deriva mucho tiempo buscando sobrevivir. Compuesto de mayormente de ancianos, mujeres y niños desnutridos. Consumen menos comida de lo normal y su presencia llena de regocijo a la Caravana porque se hizo lo correcto al recibirlos. Ahora habrá que cuidar de ellos.\n\n";
        txtMecanica.text = $"EFECTOS PASIVOS:\n\n-Consumen la mitad de Suministros que los Civiles habituales. \n\n-Al aceptarlos la Esperanza aumenta en 30. \n\n-Al perderlos la Esperanza disminuye en 40.";

    }



    public void EcharSequito()
    {
        //--- Destruye el séquito de desertores
        CampaignManager.Instance.scMenuSequito.RemoverSequito(8);
        Destroy(gameObject);
    }

    
}