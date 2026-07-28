using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private const string TutorialTerminadoKey = "Tutorial_Terminado";
    public const string TutorialIniciadoKey = "Tutorial_Iniciado";
    private static readonly Color ColorBotonDeshabilitado = new Color(0.37f, 0.37f, 0.37f, 1f);
    private static readonly Color ColorTextoDeshabilitado = new Color(0.78f, 0.78f, 0.78f, 1f);
    [Header("Escenas")]
    [SerializeField] string escenaJuego = "ES-Campaña";

    [Header("UI")]
    [SerializeField] CanvasGroup fader;
    [SerializeField] float fadeTime = 0.4f;
    [SerializeField] GameObject panelOpciones;
    [SerializeField] PrePartidaManager prePartidaManager;

    public GameObject logoIngles;
    public GameObject logoEspaniol;
    public GameObject logoPortugues;
    public GameObject disclaimerIngles;
    public GameObject disclaimerEspaniol;
    public GameObject disclaimerPortugues;
    public GameObject Opciones;

    private readonly List<Button> botonesCargarPartida = new List<Button>();
    private readonly List<Button> botonesNuevaPartida = new List<Button>();
    private readonly Dictionary<TMP_Text, Color> coloresTextoOriginales = new Dictionary<TMP_Text, Color>();

    void Awake()
    {
        if (prePartidaManager == null)
        {
            prePartidaManager = FindFirstObjectByType<PrePartidaManager>(FindObjectsInactive.Include);
        }

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

        if (fader != null)
        {
            fader.alpha = 1f;
            fader.blocksRaycasts = true;
            fader.interactable = false;
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (TRADU.i != null)
        {
            TRADU.i.ActualizarIdioma();
        }

        RecolectarBotonesCargarPartida();
        RecolectarBotonesNuevaPartida();
        RefrescarBotonesCargarPartida();
        RefrescarBotonesNuevaPartida();

        AplicarVersionesIdioma();

        if (Opciones != null)
        {
            Opciones.GetComponent<OpcionesCargarPlayerPrefsUI>().AplicarEfectosEnUI();
        }
    }

    void OnEnable()
    {
        RefrescarBotonesCargarPartida();
        RefrescarBotonesNuevaPartida();
    }

    IEnumerator Start()
    {
        silverlandlogomanager logoManager = FindFirstObjectByType<silverlandlogomanager>(FindObjectsInactive.Include);
        if (logoManager != null && logoManager.isActiveAndEnabled)
        {
            yield return logoManager.EsperarFinIntro();
        }

        yield return null;
        AsegurarMenuListoAntesDeMostrar();
        yield return null;
        yield return FadeTo(0f, fadeTime);

        if (fader != null)
        {
            fader.blocksRaycasts = false;
        }
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

    public void AplicarVersionesIdioma()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
        bool usarIngles = idioma == TRADU.IdiomaIngles;
        bool usarPortugues = idioma == TRADU.IdiomaPortugues;

        GameObject logoActivo = logoEspaniol;
        if (usarIngles && logoIngles != null) { logoActivo = logoIngles; }
        else if (usarPortugues && logoPortugues != null) { logoActivo = logoPortugues; }

        GameObject disclaimerActivo = disclaimerEspaniol;
        if (usarIngles && disclaimerIngles != null) { disclaimerActivo = disclaimerIngles; }
        else if (usarPortugues && disclaimerPortugues != null) { disclaimerActivo = disclaimerPortugues; }

        ReiniciarVersionesIdioma(logoActivo, logoEspaniol, logoIngles, logoPortugues);
        ReiniciarVersionesIdioma(disclaimerActivo, disclaimerEspaniol, disclaimerIngles, disclaimerPortugues);
    }

    private static void ReiniciarVersionesIdioma(GameObject activo, params GameObject[] versiones)
    {
        for (int i = 0; i < versiones.Length; i++)
        {
            if (versiones[i] != null)
            {
                versiones[i].SetActive(false);
            }
        }

        if (activo != null)
        {
            activo.SetActive(true);
        }
    }

    public void OnNuevaPartida()
    {
        if (!TutorialFueIniciado())
        {
            RefrescarBotonesNuevaPartida();
            return;
        }

        if (prePartidaManager != null)
        {
            RuntimeAnalytics.TrackDesign("ui", "main_menu", "new_game_preparation_open");
            prePartidaManager.Abrir();
            return;
        }

        IniciarNuevaPartida();
    }

    public void IniciarNuevaPartidaDesdePrePartida(int zonaId)
    {
        PrePartidaManager.EstablecerZonaInicialPendiente(zonaId);
        IniciarNuevaPartida();
    }

    private void IniciarNuevaPartida()
    {
        RuntimeAnalytics.TrackDesign("ui", "main_menu", "new_game");
        RuntimeAnalytics.TrackProgressionStart("campaign", "new_game");
        MarcarTutorialComoCompletado();
        SaveGameService.ClearPendingLoad();
        StartCoroutine(CargarJuego());
    }

    public void OnNuevaPartidaTutorial()
    {
        PrePartidaManager.LimpiarZonaInicialPendiente();
        PrePartidaManager.LimpiarClaseLiderPendiente();
        PrePartidaManager.LimpiarPresagiosInicialesPendientes();
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

    void RecolectarBotonesNuevaPartida()
    {
        botonesNuevaPartida.Clear();
        Button[] botones = GetComponentsInChildren<Button>(true);
        foreach (Button boton in botones)
        {
            if (boton != null && boton.name == "bt_NuevaPartida")
            {
                botonesNuevaPartida.Add(boton);
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
            AplicarEstadoVisualDeshabilitado(boton, !haySave);
        }
    }

    void RefrescarBotonesNuevaPartida()
    {
        bool tutorialIniciado = TutorialFueIniciado();
        if (botonesNuevaPartida.Count == 0)
        {
            RecolectarBotonesNuevaPartida();
        }

        foreach (Button boton in botonesNuevaPartida)
        {
            if (boton == null)
            {
                continue;
            }

            boton.interactable = tutorialIniciado;
            AplicarEstadoVisualDeshabilitado(boton, !tutorialIniciado);
        }
    }

    IEnumerator CargarJuego()
    {
        OcultarInterfazExceptoFader();

        if (fader != null)
        {
            fader.blocksRaycasts = true;
            fader.interactable = false;
        }

        yield return FadeTo(1f, fadeTime);
        AdministradorEscenas.SolicitarFaderNegroEnProximaCargaCampania();
        SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
    }

    void OcultarInterfazExceptoFader()
    {
        if (fader == null)
        {
            return;
        }

        Transform faderTransform = fader.transform;
        Transform contenedorUI = faderTransform.parent;
        if (contenedorUI == null)
        {
            faderTransform.gameObject.SetActive(true);
            return;
        }

        faderTransform.gameObject.SetActive(true);
        faderTransform.SetAsLastSibling();

        for (int i = 0; i < contenedorUI.childCount; i++)
        {
            Transform hijo = contenedorUI.GetChild(i);
            if (hijo == null || hijo == faderTransform)
            {
                continue;
            }

            if (OcultarPrePartidaExceptoFondo(hijo))
            {
                continue;
            }

            hijo.gameObject.SetActive(false);
        }
    }

    bool OcultarPrePartidaExceptoFondo(Transform candidato)
    {
        if (prePartidaManager == null
            || candidato != prePartidaManager.transform
            || !candidato.gameObject.activeSelf)
        {
            return false;
        }

        candidato.gameObject.SetActive(true);

        Transform fondoPrePartida = candidato.Find("FondoPrepartida");
        if (fondoPrePartida != null)
        {
            fondoPrePartida.gameObject.SetActive(true);
        }

        Transform panelFondo = candidato.Find("Panelfondo");
        if (panelFondo != null)
        {
            panelFondo.gameObject.SetActive(false);
        }

        return true;
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

        if (target >= 1f)
        {
            fader.blocksRaycasts = true;
        }
        else if (target <= 0f)
        {
            fader.blocksRaycasts = false;
        }
    }

    void AsegurarMenuListoAntesDeMostrar()
    {
        AplicarIdiomaActualSinFadeVisible();
        AplicarVersionesIdioma();
        RefrescarBotonesCargarPartida();
        RefrescarBotonesNuevaPartida();

        OpcionesCargarPlayerPrefsUI opcionesUi = Opciones != null ? Opciones.GetComponent<OpcionesCargarPlayerPrefsUI>() : null;
        if (opcionesUi != null)
        {
            opcionesUi.AplicarEfectosEnUI();
        }
    }

    void AplicarIdiomaActualSinFadeVisible()
    {
        if (TRADU.i == null)
        {
            return;
        }

        int idioma = PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaIngles);
        if (idioma != TRADU.IdiomaEspanol && idioma != TRADU.IdiomaIngles && idioma != TRADU.IdiomaPortugues)
        {
            idioma = TRADU.IdiomaIngles;
        }

        TRADU.i.nIdioma = idioma;
        TRADU.i.CancelInvoke(nameof(TRADU.TraducirTodosTextosSegunIdioma));
        TRADU.i.TraducirTodosTextosSegunIdioma();
    }

    void MarcarTutorialComoCompletado()
    {
        PlayerPrefs.SetInt(TutorialIniciadoKey, 1);
        PlayerPrefs.SetInt(TutorialTerminadoKey, 1);
        PlayerPrefs.SetInt(TutorialDirector.GetCompletedPlayerPrefsKey(TutorialDirector.DefaultTutorialId), 1);
        PlayerPrefs.DeleteKey(TutorialDirector.PendingStartAfterZoneDescriptionKey);
        PlayerPrefs.Save();
    }

    void ReiniciarTutorial()
    {
        PlayerPrefs.SetInt(TutorialIniciadoKey, 1);
        PlayerPrefs.DeleteKey(TutorialTerminadoKey);
        PlayerPrefs.DeleteKey(TutorialDirector.GetCompletedPlayerPrefsKey(TutorialDirector.DefaultTutorialId));
        PlayerPrefs.SetInt(TutorialDirector.PendingStartAfterZoneDescriptionKey, 1);
        PlayerPrefs.Save();
    }

    public static bool TutorialFueIniciado()
    {
        return PlayerPrefs.GetInt(TutorialIniciadoKey, 0) == 1
            || PlayerPrefs.GetInt(TutorialTerminadoKey, 0) == 1
            || PlayerPrefs.GetInt(TutorialDirector.PendingStartAfterZoneDescriptionKey, 0) == 1
            || PlayerPrefs.GetInt(TutorialDirector.GetCompletedPlayerPrefsKey(TutorialDirector.DefaultTutorialId), 0) == 1;
    }

    private void AplicarEstadoVisualDeshabilitado(Button boton, bool deshabilitado)
    {
        if (boton == null)
        {
            return;
        }

        ColorBlock colores = boton.colors;
        colores.disabledColor = ColorBotonDeshabilitado;
        boton.colors = colores;

        TMP_Text texto = boton.GetComponentInChildren<TMP_Text>(true);
        if (texto == null)
        {
            return;
        }

        if (!coloresTextoOriginales.ContainsKey(texto))
        {
            coloresTextoOriginales[texto] = texto.color;
        }

        texto.color = deshabilitado ? ColorTextoDeshabilitado : coloresTextoOriginales[texto];
    }
}
