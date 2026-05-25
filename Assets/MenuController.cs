using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private const string TutorialTerminadoKey = "Tutorial_Terminado";
    [Header("Escenas")]
    [SerializeField] string escenaJuego = "ES-Campaña";

    [Header("UI")]
    [SerializeField] CanvasGroup fader;
    [SerializeField] float fadeTime = 0.4f;
    [SerializeField] GameObject panelOpciones;

    public GameObject logoIngles;
    public GameObject logoEspaniol;
    public GameObject logoPortugues;
    public GameObject disclaimerIngles;
    public GameObject disclaimerEspaniol;
    public GameObject disclaimerPortugues;
    public GameObject Opciones;

    private readonly List<Button> botonesCargarPartida = new List<Button>();

    void Awake()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);

        if (Opciones != null)
        {
            UIFadeSlide animOpciones = UIFadeSlideUtility.Ensure(Opciones);
            if (animOpciones != null)
            {
                animOpciones.SetDurations(0.16f, 0.14f);
                animOpciones.SetOffsets(new Vector2(0f, -14f), new Vector2(0f, -8f));
                animOpciones.SetFollowMouse(false, Vector2.zero);
            }
        }

        if (fader != null) fader.alpha = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (TRADU.i != null)
        {
            TRADU.i.ActualizarIdioma();
        }

        RecolectarBotonesCargarPartida();
        RefrescarBotonesCargarPartida();

        AplicarVersionesIdioma();

        Opciones.GetComponent<OpcionesCargarPlayerPrefsUI>().AplicarEfectosEnUI();
    }

    void OnEnable()
    {
        RefrescarBotonesCargarPartida();
    }

    IEnumerator Start()
    {
        yield return FadeTo(0f, fadeTime);
    }

    public void CambiarIdioma(int n)
    {
        int idioma = (n == TRADU.IdiomaPortugues) ? TRADU.IdiomaPortugues
            : (n == TRADU.IdiomaIngles ? TRADU.IdiomaIngles : TRADU.IdiomaEspanol);
            print("cambiar idioma "+idioma);
        PlayerPrefs.SetInt("nIdioma", idioma);
        PlayerPrefs.Save();

        if (TRADU.i != null)
        {
            TRADU.i.nIdioma = idioma;
            TRADU.i.ActualizarIdioma();
        }

        AplicarVersionesIdioma();
    }

    private void AplicarVersionesIdioma()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
        bool usarIngles = idioma == TRADU.IdiomaIngles;
        bool usarPortugues = idioma == TRADU.IdiomaPortugues;

        if (logoEspaniol != null) { logoEspaniol.SetActive(!usarIngles && (!usarPortugues || logoPortugues == null)); }
        if (logoIngles != null) { logoIngles.SetActive(usarIngles); }
        if (logoPortugues != null) { logoPortugues.SetActive(usarPortugues); }

        if (disclaimerEspaniol != null) { disclaimerEspaniol.SetActive(!usarIngles && (!usarPortugues || disclaimerPortugues == null)); }
        if (disclaimerIngles != null) { disclaimerIngles.SetActive(usarIngles); }
        if (disclaimerPortugues != null) { disclaimerPortugues.SetActive(usarPortugues); }
    }

    public void OnNuevaPartida()
    {
        RuntimeAnalytics.TrackDesign("ui", "main_menu", "new_game");
        RuntimeAnalytics.TrackProgressionStart("campaign", "new_game");
        MarcarTutorialComoCompletado();
        SaveGameService.ClearPendingLoad();
        StartCoroutine(CargarJuego());
    }

    public void OnNuevaPartidaTutorial()
    {
        RuntimeAnalytics.TrackDesign("ui", "main_menu", "new_game_tutorial");
        RuntimeAnalytics.TrackProgressionStart("tutorial", "campaign", "intro");
        ReiniciarTutorial();
        SaveGameService.ClearPendingLoad();
        StartCoroutine(CargarJuego());
    }

    public void OnContinuarPartida()
    {
        if (!SaveGameService.TryQueuePendingLoadFromFile(out string error))
        {
            Debug.LogWarning("[SaveGame] No se pudo preparar la carga de la campaña. " + error);
            RefrescarBotonesCargarPartida();
            return;
        }

        RuntimeAnalytics.TrackDesign("ui", "main_menu", "continue_game");
        StartCoroutine(CargarJuego());
    }

    public void abriropciones()
    {
        if (Opciones == null) { return; }

        if (Opciones.activeInHierarchy)
        {
            UIFadeSlideUtility.Hide(Opciones);
        }
        else
        {
            RuntimeAnalytics.TrackDesign("ui", "main_menu", "options_open");
            UIFadeSlideUtility.Show(Opciones);
        }
    }

    public void OnSalir()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void OnFeedback()
    {
        RuntimeAnalytics.TrackDesign("ui", "main_menu", "feedback_open");
        Application.OpenURL("https://forms.gle/JBSKDkpa9MSfQx8k6");
    }

    void RecolectarBotonesCargarPartida()
    {
        botonesCargarPartida.Clear();
        Button[] botones = GetComponentsInChildren<Button>(true);
        foreach (Button boton in botones)
        {
            if (boton != null && boton.name == "bt_CargarPartida")
            {
                botonesCargarPartida.Add(boton);
            }
        }
    }

    void RefrescarBotonesCargarPartida()
    {
        bool haySave = SaveGameService.HasSaveFile();
        if (botonesCargarPartida.Count == 0)
        {
            RecolectarBotonesCargarPartida();
        }

        foreach (Button boton in botonesCargarPartida)
        {
            if (boton == null)
            {
                continue;
            }

            boton.interactable = haySave;
        }
    }

    IEnumerator CargarJuego()
    {
        yield return FadeTo(1f, fadeTime);
        AdministradorEscenas.SolicitarFaderNegroEnProximaCargaCampania();
        SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
    }

    IEnumerator FadeTo(float target, float time)
    {
        if (fader == null) yield break;
        float start = fader.alpha;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            fader.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        fader.alpha = target;
    }

    void MarcarTutorialComoCompletado()
    {
        PlayerPrefs.SetInt(TutorialTerminadoKey, 1);
        PlayerPrefs.SetInt(TutorialDirector.GetCompletedPlayerPrefsKey(TutorialDirector.DefaultTutorialId), 1);
        PlayerPrefs.DeleteKey(TutorialDirector.PendingStartAfterZoneDescriptionKey);
        PlayerPrefs.Save();
    }

    void ReiniciarTutorial()
    {
        PlayerPrefs.DeleteKey(TutorialTerminadoKey);
        PlayerPrefs.DeleteKey(TutorialDirector.GetCompletedPlayerPrefsKey(TutorialDirector.DefaultTutorialId));
        PlayerPrefs.SetInt(TutorialDirector.PendingStartAfterZoneDescriptionKey, 1);
        PlayerPrefs.Save();
    }
}
