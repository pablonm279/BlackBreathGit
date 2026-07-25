using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TextoInicialManager : MonoBehaviour
{
    private const string EscenaMenuPrincipal = "ES-MenuPrincipal";

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
        CampaignManager campaignManager = CampaignManager.Instance;
        gameObject.SetActive(false);
        if (campaignManager != null)
        {
            campaignManager.MostrarLogsPresagiosInicioTrasContinuarDescripcionZona();
        }

        if (campaignManager != null && campaignManager.IntroCampaniaActivaOPendiente)
        {
            campaignManager.EjecutarTrasIntroCampania(Continuar);
            campaignManager.SolicitarInicioIntroCampaniaTrasCarga(true);
            return;
        }
      
        campaignManager.scTutorialManager.ComenzarTutorial();
        TutorialDirector.TryStartPendingAfterZoneDescription();
    }

    public void ContinuarZonaNueva()
    {
        gameObject.SetActive(false);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PausarMusica(false);
            MusicManager.Instance.FadeOutYParar(0.5f);
        }
        SceneManager.LoadScene(EscenaMenuPrincipal, LoadSceneMode.Single);
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


