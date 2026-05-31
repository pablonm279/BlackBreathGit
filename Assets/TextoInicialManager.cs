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
            if (CampaignManager.Instance.IntroCampaniaActivaOPendiente)
            {
                CampaignManager.Instance.EjecutarTrasIntroCampania(Continuar);
                CampaignManager.Instance.SolicitarInicioIntroCampaniaTrasCarga(true);
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(false);
            Invoke("Continuar", 0.5f);
        }
      
        AplicarVersionPorIdioma();
    }



    public void Continuar()
    {
        if (CampaignManager.Instance != null && CampaignManager.Instance.IntroCampaniaActivaOPendiente)
        {
            gameObject.SetActive(false);
            CampaignManager.Instance.EjecutarTrasIntroCampania(Continuar);
            CampaignManager.Instance.SolicitarInicioIntroCampaniaTrasCarga(true);
            return;
        }
      
        gameObject.SetActive(false);
        CampaignManager.Instance.scTutorialManager.ComenzarTutorial();
        TutorialDirector.TryStartPendingAfterZoneDescription();
    }

    public void ContinuarZonaNueva()
    {
        gameObject.SetActive(false);
        CampaignManager.Instance.ContinuarASiguienteZona();
        TutorialDirector.TryStartPendingAfterZoneDescription();
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


