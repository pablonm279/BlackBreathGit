using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] string escenaMenuPrincipal = "ES-MenuPrincipal";

    [Header("Audio")]
    [SerializeField] float fadeOutMusica = 0.5f;

    // Alias para enganchar directo al boton "Continuar".
    public void Continuar()
    {
        VolverAlMenuPrincipal();
    }

    public void VolverAlMenuPrincipal()
    {
        ResetearEstadoDeCampania();
        SceneManager.LoadScene(escenaMenuPrincipal, LoadSceneMode.Single);
    }

    void ResetearEstadoDeCampania()
    {
        Time.timeScale = 1f;
        PlayerPrefs.Save();

        if (CampaignManager.Instance != null)
        {
            CampaignManager.Instance.MoviendoCaravana = false;
            CampaignManager.Instance.nodoDestinoActual = null;
            CampaignManager.Instance.BorrarLog();
            CampaignManager.Instance.ResetearAlientoNegro();
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.BorrarLog();
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PausarMusica(false);
            MusicManager.Instance.FadeOutYParar(fadeOutMusica);
        }
    }
}
