using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextoInicialManager : MonoBehaviour
{

    public GameObject versionEspañol;
    public GameObject versionIngles;
    public GameObject versionPortugues;


    void Start()
    {
        if(CampaignManager.Instance.scTutorialManager.tutorialActivo)
        {   
            gameObject.SetActive(false);
            Invoke("Continuar", 0.5f);
        }
      
        AplicarVersionPorIdioma();
    }



    public void Continuar()
    {
      
        gameObject.SetActive(false);
        CampaignManager.Instance.scTutorialManager.ComenzarTutorial();
    }

    public void ContinuarZonaNueva()
    {
        gameObject.SetActive(false);
        CampaignManager.Instance.ContinuarASiguienteZona();
    }

    private void AplicarVersionPorIdioma()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
        bool usarIngles = idioma == TRADU.IdiomaIngles;
        bool usarPortugues = idioma == TRADU.IdiomaPortugues;

        if (versionEspañol != null) { versionEspañol.SetActive(!usarIngles && (!usarPortugues || versionPortugues == null)); }
        if (versionIngles != null) { versionIngles.SetActive(usarIngles); }
        if (versionPortugues != null) { versionPortugues.SetActive(usarPortugues); }
    }

}


