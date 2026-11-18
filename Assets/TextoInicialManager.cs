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
        bool activarEspaniol = !usarIngles && (!usarPortugues || versionPortugues == null);

        if (versionEspañol != null) { versionEspañol.SetActive(activarEspaniol); }
        if (versionIngles != null) { versionIngles.SetActive(usarIngles); }
        if (versionPortugues != null) { versionPortugues.SetActive(usarPortugues); }

        // Para chino no existe un hijo propio: se muestra el hijo en español (activarEspaniol)
        // y se depende de que su texto ya haya sido traducido. El barrido global
        // (TraducirTodosTextosSegunIdioma) corre una sola vez al iniciar la campaña y esta
        // pantalla puede activarse recién al llegar a la zona, mucho después — así que, en vez
        // de depender de ese barrido (y de si ya cacheó o no el texto original de este
        // componente en particular), traducimos directamente el texto del hijo en español,
        // que en este punto siempre contiene el español real: nada más lo modifica.
        if (idioma == TRADU.IdiomaChinoSimplificado && activarEspaniol && TRADU.i != null && versionEspañol != null)
        {
            TextMeshProUGUI[] textos = versionEspañol.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < textos.Length; i++)
            {
                TextMeshProUGUI texto = textos[i];
                if (texto == null || string.IsNullOrEmpty(texto.text))
                {
                    continue;
                }

                texto.text = TRADU.i.Traducir(texto.text);
            }
        }
    }

}


