using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoArtistas : MonoBehaviour
{
     [SerializeField] TextMeshProUGUI txtDesc;

    [SerializeField] TextMeshProUGUI txtMecanica;

    void OnEnable()
    {
        Actualizar();
    }


    void Actualizar()
    {


        txtDesc.text = TRADU.i.Traducir("Varios artistas y miembros de una feria ambulante se han unido a la caravana, si bien son ostentosos y despilfarran recursos, pueden ayudar a la moral de la caravana en determinadas ocasiones festivas.");
        txtMecanica.text = TRADU.i.Traducir("EFECTOS PASIVOS:\n\n-Al unirse a la Caravana se ganan 15 de Esperanza.\n\n-Cada vez que se selecciona Feria como Tarea Civil de Descanso se ganan 10 de Esperanza Extra.\n\n-Cada día hay un 30% de chances de que hagan un festín y despilfarren 1-4 Suministros.\n\n-Si abandonan la Caravana se pierden 15 de Esperanza.");

      
    }
    public void EcharSequito()
    {
        //--- Destruye el séquito de artistas
        CampaignManager.Instance.scMenuSequito.RemoverSequito(4);
        Destroy(gameObject);



    }
}