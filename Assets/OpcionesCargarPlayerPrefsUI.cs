using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class OpcionesCargarPlayerPrefsUI : MonoBehaviour
{
    private const string PrefPostFx = "gfx_postfx_enabled";
    private const string PrefAA = "gfx_aa_enabled";
    private const string PrefBloom = "gfx_bloom_enabled";
    private const string PrefDoF = "gfx_dof_enabled";
    private const string PrefVsync = "gfx_vsync";
    private const string PrefFpsLimit = "gfx_fps_limit";
    private const string PrefBrightness = "gfx_brightness";
    private const float DefaultBrightness = 0.65f;
    private const float AudioSfxOffsetY = -60.7f;

    public Slider volMusicaSlider;
    [NonSerialized] private Slider volSfxSlider;
    public AudioSource musicaFondo;
    private TextMeshProUGUI etiquetaVolMusica;
    private TextMeshProUGUI etiquetaVolSfx;

    public int nIdioma;
    public Toggle EspaniolToggle;

    public Toggle EnglishToggle;
    public Toggle PortuguesToggle;
    public TextMeshProUGUI restartRequiredText;

    bool musicInBackground = true;
    public Toggle musicInBackgroundToggle;

    public GameObject PanelAudio;
    public GameObject PanelGraficos;
    public GameObject PanelControles;
    public GameObject PanelGameplay;
    public GameObject PanelIdioma;

    public TMP_Dropdown resolucionDropdown;
    public TMP_Dropdown graficosDropdown;
    public TMP_Dropdown dificultadDropdown;

    private List<Resolution> resolucionesSoportadas = new List<Resolution>();
    private int resolucionActualIndex = 0;

    public Toggle fullscreenToggle;
    public Toggle modorapidoToggle;
    public Toggle tipsToggle;

    [Header("Visual Quality")]
    public Toggle postFxToggle;
    public Toggle aaToggle;
    public Toggle bloomToggle;
    public Toggle dofToggle;
    public Toggle vSyncToggle;
    public TMP_Dropdown fpsLimitDropdown;

    [Header("Brillo")]
    public Slider brilloSlider;
    public TextMeshProUGUI brilloLabel;
    

    void Start()
    {
        InicializarCalibracionVisualPorDefecto();
        AsegurarControlesAudioSfx();
        ConfigurarToggleAyudas();
       
        LlenarDropdownResoluciones();
        AplicarEfectosEnUI();

    }
    void OnEnable()
    {
        InicializarCalibracionVisualPorDefecto();
        AsegurarControlesAudioSfx();
        ConfigurarToggleAyudas();
        
        AplicarEfectosEnUI();
    }

    private void InicializarCalibracionVisualPorDefecto()
    {
        if (PlayerPrefs.HasKey(PrefBrightness)) { return; }

        float brilloGuardado = PlayerPrefs.GetFloat("brillo", DefaultBrightness);
        PlayerPrefs.SetFloat(PrefBrightness, brilloGuardado);
        if (PlayerPrefs.HasKey("brillo"))
        {
            PlayerPrefs.DeleteKey("brillo");
        }
        PlayerPrefs.Save();
    }


    public void AplicarEfectosEnUI()
    {
        AsegurarControlesAudioSfx();

        // Volumen de la másica
        float volumenMusica = AjustesAudio.ObtenerVolumenMusica();
        if (volumenMusica > 0.9f)
        { volumenMusica = 0.9f; }
        if (volMusicaSlider != null)
        {
            volMusicaSlider.SetValueWithoutNotify(volumenMusica);
        }
        AplicarVolumenMusica(volumenMusica);

        float volumenSfx = AjustesAudio.ObtenerVolumenSfx();
        if (volSfxSlider != null)
        {
            volSfxSlider.SetValueWithoutNotify(volumenSfx);
        }
        AplicarVolumenSfx(volumenSfx);

        // Sonido en segundo plano
        musicInBackground = PlayerPrefs.GetInt("Background_Sound", 1) == 1;
        if (musicInBackgroundToggle != null)
        {
            musicInBackgroundToggle.SetIsOnWithoutNotify(musicInBackground);
        }


        //Idioma
        nIdioma = PlayerPrefs.GetInt("nIdioma", 2);


        EspaniolToggle.SetIsOnWithoutNotify(false);
        EnglishToggle.SetIsOnWithoutNotify(false);
        PortuguesToggle.SetIsOnWithoutNotify(false); 

        if (nIdioma == TRADU.IdiomaEspanol)
        { EspaniolToggle.SetIsOnWithoutNotify(true); }
        else if (nIdioma == TRADU.IdiomaIngles)
        { EnglishToggle.SetIsOnWithoutNotify(true); }
        else if (nIdioma == TRADU.IdiomaPortugues )
        { PortuguesToggle.SetIsOnWithoutNotify(true); }
        else
        { EspaniolToggle.SetIsOnWithoutNotify(true);}
        ActualizarEtiquetasAudio();
        ActualizarEtiquetaBrillo();

        if (PanelControles != null && PanelControles.activeInHierarchy)
        {
            AplicarIdiomaPanelControles();
        }

        //Pantalla y resolución
        if (resolucionDropdown != null && resolucionesSoportadas.Count > 0)
        {
            int guardado = PlayerPrefs.GetInt("res_index", resolucionActualIndex);
            guardado = Mathf.Clamp(guardado, 0, resolucionesSoportadas.Count - 1);

            resolucionActualIndex = guardado;
            resolucionDropdown.SetValueWithoutNotify(resolucionActualIndex);
            AplicarResolucion();
        }
        // Pantalla completa
        bool fs = PlayerPrefs.GetInt("fullscreen", 1) == 1;
        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(fs);
        Screen.fullScreen = fs;

        // Calidad grafica
        int calidadIndex = PlayerPrefs.GetInt("graficos_index", QualitySettings.GetQualityLevel());
        //        print($"Obteniendo calidad grafica nivel {calidadIndex}");

        // Brillo
        float brillo = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefBrightness, DefaultBrightness));
        if (brilloSlider != null)
        {
            brilloSlider.SetValueWithoutNotify(brillo);
        }
        VisualPolishRuntime.ApplyPostProcessingPrefsNow();

        QualitySettings.SetQualityLevel(calidadIndex, true);
        PlayerPrefs.SetInt("graficos_index", calidadIndex); // asegura persistencia inmediata
        PlayerPrefs.Save();
        graficosDropdown.SetValueWithoutNotify(calidadIndex);
        TraducirDropdownGraficos();
        TraducirDropdownDificultad();

        int difGuardada = PlayerPrefs.GetInt("dificultad_index", 2); // 2 = Normal por defecto (í­ndice dropdown)
        difGuardada = Mathf.Clamp(difGuardada, 0, Mathf.Max(0, dificultadDropdown.options.Count - 1));
        dificultadDropdown.SetValueWithoutNotify(difGuardada);

        // Modo rapido
        bool modorapido = PlayerPrefs.GetInt("modoRapido", 0) == 1;
        if (modorapidoToggle != null)
            modorapidoToggle.SetIsOnWithoutNotify(modorapido);

        if (tipsToggle != null)
            tipsToggle.SetIsOnWithoutNotify(TutorialTooltipProgress.MostrarAyudas);

        CargarOpcionesVisuales();
        AplicarPreferenciasSyncYFPS();
    }

    public void CambiarEfectos()
    {
        // Volumen de la másica
        if (volMusicaSlider != null)
        {
            AjustesAudio.EstablecerVolumenMusica(volMusicaSlider.value);
            AplicarVolumenMusica(volMusicaSlider.value);
        }

        if (volSfxSlider != null)
        {
            AjustesAudio.EstablecerVolumenSfx(volSfxSlider.value);
            AplicarVolumenSfx(volSfxSlider.value);
        }

        // Brillo
        if (brilloSlider != null)
        {
            PlayerPrefs.SetFloat(PrefBrightness, Mathf.Clamp01(brilloSlider.value));
            VisualPolishRuntime.ApplyPostProcessingPrefsNow();
        }
      
        // Sonido en segundo plano
        PlayerPrefs.SetInt("Background_Sound", musicInBackgroundToggle.isOn ? 1 : 0);
        Application.runInBackground = musicInBackgroundToggle.isOn;


        // Idioma
       /* nIdioma = ObtenerIdiomaSeleccionado();
        print("Obtener Idioma seleccionado: " + nIdioma);
        PlayerPrefs.SetInt("nIdioma", nIdioma);
        if (TRADU.i != null) TRADU.i.nIdioma = nIdioma;*/
        SetRestartRequiredText(string.Empty);

        // Resolución y pantalla completa

        AplicarResolucion();


        GuardarOpcionesVisuales();
        AplicarPreferenciasSyncYFPS();
        AplicarMostrarAyudas();

        //---
        PlayerPrefs.Save();
    }

    private void AplicarVolumenMusica(float volumenMusica)
    {
        MusicManager managerMusica = null;
        if (musicaFondo != null)
        {
            managerMusica = musicaFondo.GetComponent<MusicManager>();
        }

        if (managerMusica == null)
        {
            managerMusica = MusicManager.Instance;
        }

        if (managerMusica != null)
        {
            managerMusica.SetVolumen(volumenMusica);
            return;
        }

        if (musicaFondo != null)
        {
            musicaFondo.volume = volumenMusica;
        }
    }

    private void AplicarVolumenSfx(float volumenSfx)
    {
        AjustesAudio.EstablecerVolumenSfx(volumenSfx);
        AjustesAudio.AplicarVolumenSfxEnEscena(musicaFondo);
    }



    public void AbrirPanelAudio()
    {
        RuntimeAnalytics.TrackDesign("ui", "options", "audio_panel");
        PanelAudio.SetActive(true);
        PanelGraficos.SetActive(false);
        PanelControles.SetActive(false);
        PanelGameplay.SetActive(false);
        PanelIdioma.SetActive(false);
    }

    public void AbrirPanelGraficos()
    {
        RuntimeAnalytics.TrackDesign("ui", "options", "graphics_panel");
        PanelAudio.SetActive(false);
        PanelGraficos.SetActive(true);
        PanelControles.SetActive(false);
        PanelGameplay.SetActive(false);
        PanelIdioma.SetActive(false);
       
    }

    public void AbrirPanelControles()
    {
        RuntimeAnalytics.TrackDesign("ui", "options", "controls_panel");
        PanelAudio.SetActive(false);
        PanelGraficos.SetActive(false);
        PanelControles.SetActive(true);
        PanelGameplay.SetActive(false);
        PanelIdioma.SetActive(false);
        AplicarIdiomaPanelControles();
    }

    public void AbrirPanelGameplay()
    {
        RuntimeAnalytics.TrackDesign("ui", "options", "gameplay_panel");
        PanelAudio.SetActive(false);
        PanelGraficos.SetActive(false);
        PanelControles.SetActive(false);
        PanelGameplay.SetActive(true);
        PanelIdioma.SetActive(false);
    }
    public void AbrirPanelIdioma()
    {
        RuntimeAnalytics.TrackDesign("ui", "options", "language_panel");
        PanelAudio.SetActive(false);
        PanelGraficos.SetActive(false);
        PanelControles.SetActive(false);
        PanelGameplay.SetActive(false);
        PanelIdioma.SetActive(true);
    }


    public void OnSalir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void cerrarOpciones()
    {
         CambiarEfectos();
        this.gameObject.SetActive(false);

    }

    void LlenarDropdownResoluciones()
    {
        if (resolucionDropdown == null)
            return;

        resolucionesSoportadas.Clear();
        resolucionDropdown.ClearOptions();

        // Resoluciones tí­picas que queremos mostrar
        List<Vector2Int> resolucionesDeseadas = new List<Vector2Int>()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160)
    };

        Resolution[] todas = Screen.resolutions;
        List<string> opciones = new List<string>();

        int indiceActualPorDefecto = 0;

        // Filtrar: solo agregar resoluciones deseadas que existan en el monitor
        foreach (var r in todas)
        {
            foreach (var deseada in resolucionesDeseadas)
            {
                if (r.width == deseada.x && r.height == deseada.y)
                {
                    string texto = $"{r.width} x {r.height}";

                    if (!opciones.Contains(texto))
                    {
                        opciones.Add(texto);
                        resolucionesSoportadas.Add(r);

                        // Detectar resolución actual
                        if (r.width == Screen.currentResolution.width &&
                            r.height == Screen.currentResolution.height)
                        {
                            indiceActualPorDefecto = resolucionesSoportadas.Count - 1;
                        }
                    }
                }
            }
        }

        // Si no se encontró ninguna deseada, fallback: agregar la actual
        if (opciones.Count == 0)
        {
            string actual = $"{Screen.currentResolution.width} x {Screen.currentResolution.height}";
            opciones.Add(actual);
            resolucionesSoportadas.Add(Screen.currentResolution);
            indiceActualPorDefecto = 0;
        }

        resolucionDropdown.AddOptions(opciones);

        // Cargar í­ndice guardado
        int guardado = PlayerPrefs.GetInt("res_index", indiceActualPorDefecto);
        guardado = Mathf.Clamp(guardado, 0, resolucionesSoportadas.Count - 1);

        resolucionActualIndex = guardado;
        resolucionDropdown.SetValueWithoutNotify(resolucionActualIndex);

        // Aplicar resolución inicial
        AplicarResolucion();
    }

    void AplicarResolucion()
    {
        if (resolucionesSoportadas.Count == 0)
            return;

        int index = resolucionDropdown.value;
        index = Mathf.Clamp(index, 0, resolucionesSoportadas.Count - 1);

        Resolution r = resolucionesSoportadas[index];

        bool full = fullscreenToggle.isOn;

        // print($"Aplicando resolución {r.width}x{r.height} | Fullscreen = {full}");

        // Aplicar TODO junto: resolución + fullscreen
        Screen.SetResolution(r.width, r.height, full);

        // Guardamos
        PlayerPrefs.SetInt("res_index", index);
        PlayerPrefs.SetInt("fullscreen", full ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void AplicarGraficos()
    {
        int index = graficosDropdown.value;
        QualitySettings.SetQualityLevel(index, true);

        PlayerPrefs.SetInt("graficos_index", index);
        PlayerPrefs.Save();

        AplicarPreferenciasSyncYFPS();

    }

    public void AplicarDificultad()
    {
        int index = dificultadDropdown.value;
        PlayerPrefs.SetInt("dificultad_index", index);
        PlayerPrefs.Save();
        RuntimeAnalytics.TrackDesign("ui", "options", "difficulty_" + index);

        if (CampaignManager.Instance != null) { CampaignManager.Instance.AjustarDificultad(); }
    }

    void TraducirDropdownGraficos()
    {
        if (graficosDropdown == null || graficosDropdown.options.Count < 3)
            return;

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : nIdioma;

        if (idioma == 1)
        {
            graficosDropdown.options[0].text = "Baja";
            graficosDropdown.options[1].text = "Media";
            graficosDropdown.options[2].text = "Ultra";
            // graficosDropdown.options[3].text = "Ultra";
        }
        else if (idioma == 2)
        {
            graficosDropdown.options[0].text = "Low";
            graficosDropdown.options[1].text = "Medium";
            graficosDropdown.options[2].text = "Ultra";
            //   graficosDropdown.options[3].text = "Ultra";
        }
        else if (idioma == TRADU.IdiomaPortugues)
        {
            graficosDropdown.options[0].text = "Baixa";
            graficosDropdown.options[1].text = "Media";
            graficosDropdown.options[2].text = "Ultra";
        }

        graficosDropdown.RefreshShownValue();

    }

    void TraducirDropdownDificultad()
    {
        if (dificultadDropdown == null || dificultadDropdown.options.Count < 5)
            return;

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : nIdioma;

        if (idioma == 1)
        {
            dificultadDropdown.options[0].text = "Muy Facil";
            dificultadDropdown.options[1].text = "Facil";
            dificultadDropdown.options[2].text = "Normal";
            dificultadDropdown.options[3].text = "Dificil";
            dificultadDropdown.options[4].text = "Muy Dificil";
        }
        else if (idioma == 2)
        {
            dificultadDropdown.options[0].text = "Very Easy";
            dificultadDropdown.options[1].text = "Easy";
            dificultadDropdown.options[2].text = "Normal";
            dificultadDropdown.options[3].text = "Hard";
            dificultadDropdown.options[4].text = "Very Hard";
        }
        else if (idioma == TRADU.IdiomaPortugues)
        {
            dificultadDropdown.options[0].text = "Muito Facil";
            dificultadDropdown.options[1].text = "Facil";
            dificultadDropdown.options[2].text = "Normal";
            dificultadDropdown.options[3].text = "Dificil";
            dificultadDropdown.options[4].text = "Muito Dificil";
        }

        dificultadDropdown.RefreshShownValue();
    }

    public void AplicarModoRapido()
    {
        bool modorapido = modorapidoToggle.isOn;
        PlayerPrefs.SetInt("modoRapido", modorapido ? 1 : 0);
        PlayerPrefs.Save();
        RuntimeAnalytics.TrackDesign("ui", "options", "fast_mode_" + RuntimeAnalytics.BoolToken(modorapido));

    }

    public void AplicarMostrarAyudas()
    {
        if (tipsToggle == null)
        {
            return;
        }

        TutorialTooltipProgress.SetMostrarAyudas(tipsToggle.isOn);
        PersistirEstadoAyudasEnCampania();
    }

    private void ConfigurarToggleAyudas()
    {
        if (tipsToggle == null)
        {
            return;
        }

        tipsToggle.onValueChanged.RemoveListener(OnTipsToggleChanged);
        tipsToggle.onValueChanged.AddListener(OnTipsToggleChanged);
    }

    private void OnTipsToggleChanged(bool activo)
    {
        TutorialTooltipProgress.SetMostrarAyudas(activo);
        PersistirEstadoAyudasEnCampania();
        RuntimeAnalytics.TrackDesign("ui", "options", "tutorial_tips_" + RuntimeAnalytics.BoolToken(activo));
    }

    private void PersistirEstadoAyudasEnCampania()
    {
        if (CampaignManager.Instance == null)
        {
            return;
        }

        if (!CampaignManager.Instance.PuedeGuardarCampania(out _))
        {
            return;
        }

        CampaignManager.Instance.TryAutosaveCampania("opciones ayudas tutorial", out _);
    }

    public void AplicarPostFXToggle() { GuardarOpcionesVisuales(); PlayerPrefs.Save(); }
    public void AplicarAAToggle() { GuardarOpcionesVisuales(); PlayerPrefs.Save(); }
    public void AplicarBloomToggle() { GuardarOpcionesVisuales(); PlayerPrefs.Save(); }
    public void AplicarDoFToggle() { GuardarOpcionesVisuales(); PlayerPrefs.Save(); }
    public void AplicarVSyncToggle() { GuardarOpcionesVisuales(); AplicarPreferenciasSyncYFPS(); PlayerPrefs.Save(); }
    public void AplicarFPSLimit() { GuardarOpcionesVisuales(); AplicarPreferenciasSyncYFPS(); PlayerPrefs.Save(); }


    private void CargarOpcionesVisuales()
    {
        bool postFx = PlayerPrefs.GetInt(PrefPostFx, 1) == 1;
        bool aa = PlayerPrefs.GetInt(PrefAA, 1) == 1;
        bool bloom = PlayerPrefs.GetInt(PrefBloom, 1) == 1;
        bool dof = PlayerPrefs.GetInt(PrefDoF, 0) == 1; // default: no blur de movimiento
        bool vsync = PlayerPrefs.GetInt(PrefVsync, 1) == 1;
        int fpsLimit = PlayerPrefs.GetInt(PrefFpsLimit, 60);

        if (postFxToggle != null) { postFxToggle.SetIsOnWithoutNotify(postFx); }
        if (aaToggle != null) { aaToggle.SetIsOnWithoutNotify(aa); }
        if (bloomToggle != null) { bloomToggle.SetIsOnWithoutNotify(bloom); }
        if (dofToggle != null) { dofToggle.SetIsOnWithoutNotify(dof); }
        if (vSyncToggle != null) { vSyncToggle.SetIsOnWithoutNotify(vsync); }

        if (fpsLimitDropdown != null)
        {
            InicializarDropdownFPS();
            fpsLimitDropdown.SetValueWithoutNotify(MapearFPSAIndice(fpsLimit));
        }

       
    }

    private void GuardarOpcionesVisuales()
    {
        if (postFxToggle != null) { PlayerPrefs.SetInt(PrefPostFx, postFxToggle.isOn ? 1 : 0); }
        if (aaToggle != null) { PlayerPrefs.SetInt(PrefAA, aaToggle.isOn ? 1 : 0); }
        if (bloomToggle != null) { PlayerPrefs.SetInt(PrefBloom, bloomToggle.isOn ? 1 : 0); }
        if (dofToggle != null) { PlayerPrefs.SetInt(PrefDoF, dofToggle.isOn ? 1 : 0); }
        if (vSyncToggle != null) { PlayerPrefs.SetInt(PrefVsync, vSyncToggle.isOn ? 1 : 0); }

        if (fpsLimitDropdown != null)
        {
            int fps = MapearIndiceAFPS(fpsLimitDropdown.value);
            PlayerPrefs.SetInt(PrefFpsLimit, fps);
        }

      
    }

    private void AplicarPreferenciasSyncYFPS()
    {
        bool vsync = PlayerPrefs.GetInt(PrefVsync, 1) == 1;
        QualitySettings.vSyncCount = vsync ? 1 : 0;

        int fps = PlayerPrefs.GetInt(PrefFpsLimit, 60);
        Application.targetFrameRate = VisualPolishRuntime.ResolveTargetFrameRate(vsync, fps, SceneManager.GetActiveScene());
    }

    private void InicializarDropdownFPS()
    {
        if (fpsLimitDropdown == null) { return; }
        if (fpsLimitDropdown.options != null && fpsLimitDropdown.options.Count > 0) { return; }

        fpsLimitDropdown.ClearOptions();
        fpsLimitDropdown.AddOptions(new List<string>
        {
            "30 FPS",
            "60 FPS",
            "120 FPS",
            "144 FPS",
            "240 FPS",
            "Sin lí­mite"
        });
    }

    private static int MapearIndiceAFPS(int idx)
    {
        switch (idx)
        {
            case 0: return 30;
            case 1: return 60;
            case 2: return 120;
            case 3: return 144;
            case 4: return 240;
            default: return 0; // sin limite
        }
    }

    private static int MapearFPSAIndice(int fps)
    {
        if (fps <= 0) { return 5; }
        if (fps <= 30) { return 0; }
        if (fps <= 60) { return 1; }
        if (fps <= 120) { return 2; }
        if (fps <= 144) { return 3; }
        return 4;
    }

    private void AplicarIdiomaPanelControles()
    {
        if (PanelControles == null) return;
        int childCount = PanelControles.transform.childCount;
        if (childCount <= 0) return;

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : nIdioma;
        int childIdioma = ObtenerIndicePanelIdioma(idioma, childCount);
        int panelesIdioma = Mathf.Min(3, childCount);

        for (int i = 0; i < panelesIdioma; i++)
        {
            PanelControles.transform.GetChild(i).gameObject.SetActive(i == childIdioma);
        }

        if (childIdioma >= panelesIdioma && panelesIdioma > 0)
        {
            PanelControles.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    private int ObtenerIdiomaSeleccionado()
    {
        Debug.Log("PT: " + (PortuguesToggle != null && PortuguesToggle.isOn));
        Debug.Log("EN: " + (EnglishToggle != null && EnglishToggle.isOn));
        Debug.Log("ES: " + (EspaniolToggle != null && EspaniolToggle.isOn));

        if (PortuguesToggle != null && PortuguesToggle.isOn)
            return TRADU.IdiomaPortugues;

        if (EnglishToggle != null && EnglishToggle.isOn)
            return TRADU.IdiomaIngles;

        return TRADU.IdiomaEspanol;
    }

public void OnEspanolToggle(bool isOn)
{
    if (!isOn) return;
    CambiarIdioma(TRADU.IdiomaEspanol);
}

public void OnEnglishToggle(bool isOn)
{
    if (!isOn) return;
    CambiarIdioma(TRADU.IdiomaIngles);
}

    public void OnPortuguesToggle(bool isOn)
    {
        if (!isOn) return;
        CambiarIdioma(TRADU.IdiomaPortugues);
    }

public void CambiarIdioma(int idioma)
{
    nIdioma = idioma;
    PlayerPrefs.SetInt("nIdioma", nIdioma);
    PlayerPrefs.Save();

    if (TRADU.i != null)
    {
        TRADU.i.nIdioma = nIdioma;
        TRADU.i.TraducirTodosTextosSegunIdioma();
    }

    ActualizarEtiquetasAudio();
    ActualizarEtiquetaBrillo();
    TraducirDropdownGraficos();
    TraducirDropdownDificultad();
    AplicarIdiomaPanelControles();
    RefrescarObjetosMenuDependientesIdioma();
    SetRestartRequiredText(string.Empty);
}

    private void RefrescarObjetosMenuDependientesIdioma()
    {
        MenuController menu = FindFirstObjectByType<MenuController>(FindObjectsInactive.Include);
        if (menu != null)
        {
            menu.AplicarVersionesIdioma();
        }
    }

    private static int ObtenerIndicePanelIdioma(int idioma, int childCount)
    {
        if (idioma == TRADU.IdiomaIngles && childCount > 1)
        {
            return 1;
        }

        if (idioma == TRADU.IdiomaPortugues && childCount > 2)
        {
            return 2;
        }

    

        return 0;
    }

    private static string ObtenerTextoReinicio(int idioma)
    {
        if (idioma == TRADU.IdiomaIngles)
        {
            return "Restart required to apply changes.";
        }

        if (idioma == TRADU.IdiomaPortugues)
        {
            return "E preciso reiniciar para aplicar as alteracoes.";
        }

        return "Se requiere reiniciar para aplicar los cambios.";
    }

    private void SetRestartRequiredText(string text)
    {
        if (restartRequiredText != null)
        {
            restartRequiredText.text = text;
        }
    }

    private void AsegurarControlesAudioSfx()
    {
        if (PanelAudio == null || volMusicaSlider == null)
        {
            return;
        }

        if (etiquetaVolMusica == null)
        {
            etiquetaVolMusica = BuscarEtiquetaEnPanelAudio("txtMusicVolume", "music", "musica");
        }

        if (volSfxSlider == null)
        {
            Transform sliderExistente = PanelAudio.transform.Find("SliderSFX");
            if (sliderExistente != null)
            {
                volSfxSlider = sliderExistente.GetComponent<Slider>();
            }
        }

        if (etiquetaVolSfx == null)
        {
            Transform etiquetaExistente = PanelAudio.transform.Find("txtSfxVolume");
            if (etiquetaExistente != null)
            {
                etiquetaVolSfx = etiquetaExistente.GetComponent<TextMeshProUGUI>();
            }
        }

        if (etiquetaVolMusica == null)
        {
            return;
        }

        if (etiquetaVolSfx == null)
        {
            etiquetaVolSfx = CrearEtiquetaSfx();
        }

        if (volSfxSlider == null)
        {
            volSfxSlider = CrearSliderSfx();
        }

        RectTransform rectEtiquetaMusica = etiquetaVolMusica.transform as RectTransform;
        RectTransform rectSliderMusica = volMusicaSlider.transform as RectTransform;

        if (etiquetaVolSfx != null && rectEtiquetaMusica != null)
        {
            RectTransform rectEtiquetaSfx = etiquetaVolSfx.transform as RectTransform;
            if (rectEtiquetaSfx != null)
            {
                Vector2 posEtiqueta = rectEtiquetaSfx.anchoredPosition;
                posEtiqueta.y = rectEtiquetaMusica.anchoredPosition.y + AudioSfxOffsetY;
                rectEtiquetaSfx.anchoredPosition = posEtiqueta;
            }
        }

        if (volSfxSlider != null && rectSliderMusica != null)
        {
            RectTransform rectSliderSfx = volSfxSlider.transform as RectTransform;
            if (rectSliderSfx != null)
            {
                Vector2 posSlider = rectSliderSfx.anchoredPosition;
                posSlider.y = rectSliderMusica.anchoredPosition.y + AudioSfxOffsetY;
                rectSliderSfx.anchoredPosition = posSlider;
            }
        }
    }

    private TextMeshProUGUI CrearEtiquetaSfx()
    {
        if (etiquetaVolMusica == null)
        {
            return null;
        }

        GameObject clon = Instantiate(etiquetaVolMusica.gameObject, etiquetaVolMusica.transform.parent);
        clon.name = "txtSfxVolume";
        return clon.GetComponent<TextMeshProUGUI>();
    }

    private Slider CrearSliderSfx()
    {
        if (volMusicaSlider == null)
        {
            return null;
        }

        GameObject clon = Instantiate(volMusicaSlider.gameObject, volMusicaSlider.transform.parent);
        clon.name = "SliderSFX";
        return clon.GetComponent<Slider>();
    }

    private TextMeshProUGUI BuscarEtiquetaEnPanelAudio(string nombrePreferido, params string[] patrones)
    {
        if (PanelAudio == null)
        {
            return null;
        }

        Transform hijoDirecto = PanelAudio.transform.Find(nombrePreferido);
        if (hijoDirecto != null)
        {
            TextMeshProUGUI textoDirecto = hijoDirecto.GetComponent<TextMeshProUGUI>();
            if (textoDirecto != null)
            {
                return textoDirecto;
            }
        }

        TextMeshProUGUI[] textos = PanelAudio.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            TextMeshProUGUI texto = textos[i];
            if (texto == null)
            {
                continue;
            }

            string nombre = texto.name.ToLowerInvariant();
            string contenido = texto.text.ToLowerInvariant();
            for (int j = 0; j < patrones.Length; j++)
            {
                string patron = patrones[j];
                if (nombre.Contains(patron) || contenido.Contains(patron))
                {
                    return texto;
                }
            }
        }

        return null;
    }

    private void ActualizarEtiquetasAudio()
    {
        if (etiquetaVolMusica == null)
        {
            etiquetaVolMusica = BuscarEtiquetaEnPanelAudio("txtMusicVolume", "music", "musica");
        }

        if (etiquetaVolSfx == null)
        {
            etiquetaVolSfx = BuscarEtiquetaEnPanelAudio("txtSfxVolume", "sfx", "efect");
        }

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : nIdioma;
        string textoMusica = "Volumen de la Musica";
        string textoSfx = "Volumen de Efectos";

        if (idioma == TRADU.IdiomaIngles)
        {
            textoMusica = "Music Volume";
            textoSfx = "SFX Volume";
        }
        else if (idioma == TRADU.IdiomaPortugues)
        {
            textoMusica = "Volume da Musica";
            textoSfx = "Volume de Efeitos";
        }

        if (etiquetaVolMusica != null)
        {
            etiquetaVolMusica.text = textoMusica;
        }

        if (etiquetaVolSfx != null)
        {
            etiquetaVolSfx.text = textoSfx;
        }
    }

    private static void ReposicionarSliderDebajo(RectTransform sliderRect)
    {
        if (sliderRect == null) { return; }
        RectTransform parent = sliderRect.parent as RectTransform;
        if (parent == null) { return; }

        float minY = float.MaxValue;
        for (int i = 0; i < parent.childCount; i++)
        {
            RectTransform child = parent.GetChild(i) as RectTransform;
            if (child == null || child == sliderRect) { continue; }
            minY = Mathf.Min(minY, child.anchoredPosition.y);
        }

        if (minY == float.MaxValue) { minY = -140f; }

        Vector2 nuevaPos = sliderRect.anchoredPosition;
        nuevaPos.y = minY - 95f;
        sliderRect.anchoredPosition = nuevaPos;
    }

    private static TextMeshProUGUI ResolverEtiquetaSlider(Transform root)
    {
        if (root == null) { return null; }

        TextMeshProUGUI[] textos = root.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (textos == null || textos.Length == 0) { return null; }

        for (int i = 0; i < textos.Length; i++)
        {
            TextMeshProUGUI t = textos[i];
            if (t == null) { continue; }

            string n = t.name.ToLowerInvariant();
            string contenido = t.text.ToLowerInvariant();
            if (n.Contains("music") || n.Contains("vol") || contenido.Contains("volumen") || contenido.Contains("volume"))
            {
                return t;
            }
        }

        return textos[0];
    }

    

    private void ActualizarEtiquetaBrillo()
    {
        if (brilloLabel == null && brilloSlider != null)
        {
            brilloLabel = ResolverEtiquetaSlider(brilloSlider.transform);
        }

        if (brilloLabel == null) { return; }

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : nIdioma;
        if (idioma == TRADU.IdiomaIngles)
        {
            brilloLabel.text = "Brightness";
        }
        else if (idioma == TRADU.IdiomaPortugues)
        {
            brilloLabel.text = "Brilho";
        }
        else
        {
            brilloLabel.text = "Brillo";
        }
    }


    public GameObject confirmarSalir;


    public void btnSalir()
    {
        RuntimeAnalytics.TrackDesign("ui", "options", "quit_button");
        if (confirmarSalir != null)
        {
            confirmarSalir.SetActive(true);
        }
        else
        {
            OnSalir();
        }
    }

    public void noSalir()
    {

        confirmarSalir.SetActive(false);
    }    
       


}

public static class AjustesAudio
{
    public const string PrefVolMusica = "Vol_Musica";
    public const string PrefVolSfx = "Vol_SFX";
    public const float VolumenMusicaPorDefecto = 0.8f;
    public const float VolumenSfxPorDefecto = 0.8f;

    static readonly Dictionary<int, float> volumenesBaseAudioSource = new Dictionary<int, float>();
    static float? volumenSfxCache;

    public static event Action<float> VolumenSfxCambiado;

    public static float ObtenerVolumenMusica()
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolMusica, VolumenMusicaPorDefecto));
    }

    public static void EstablecerVolumenMusica(float volumen)
    {
        PlayerPrefs.SetFloat(PrefVolMusica, Mathf.Clamp01(volumen));
    }

    public static float ObtenerVolumenSfx()
    {
        if (!volumenSfxCache.HasValue)
        {
            volumenSfxCache = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolSfx, VolumenSfxPorDefecto));
        }

        return volumenSfxCache.Value;
    }

    public static void EstablecerVolumenSfx(float volumen)
    {
        float volumenClamped = Mathf.Clamp01(volumen);
        bool cambio = !volumenSfxCache.HasValue || !Mathf.Approximately(volumenSfxCache.Value, volumenClamped);
        volumenSfxCache = volumenClamped;
        PlayerPrefs.SetFloat(PrefVolSfx, volumenClamped);

        if (cambio)
        {
            VolumenSfxCambiado?.Invoke(volumenClamped);
        }
    }

    public static float EscalarVolumenSfx(float volumenBase = 1f)
    {
        return Mathf.Max(0f, volumenBase) * ObtenerVolumenSfx();
    }

    public static void RegistrarAudioSource(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return;
        }

        int id = audioSource.GetInstanceID();
        if (!volumenesBaseAudioSource.ContainsKey(id))
        {
            volumenesBaseAudioSource[id] = Mathf.Max(0f, audioSource.volume);
        }
    }

    public static void EstablecerVolumenBase(AudioSource audioSource, float volumenBase)
    {
        if (audioSource == null)
        {
            return;
        }

        volumenesBaseAudioSource[audioSource.GetInstanceID()] = Mathf.Max(0f, volumenBase);
    }

    public static float ObtenerVolumenBase(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return 1f;
        }

        RegistrarAudioSource(audioSource);
        return volumenesBaseAudioSource[audioSource.GetInstanceID()];
    }

    public static void AplicarVolumenSfx(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return;
        }

        float volumenBase = ObtenerVolumenBase(audioSource);
        audioSource.volume = Mathf.Clamp01(EscalarVolumenSfx(volumenBase));
    }

    public static void AplicarVolumenSfx(AudioSource audioSource, float volumenBase)
    {
        if (audioSource == null)
        {
            return;
        }

        EstablecerVolumenBase(audioSource, volumenBase);
        audioSource.volume = Mathf.Clamp01(EscalarVolumenSfx(volumenBase));
    }

    public static AudioSource ObtenerOAgregarAudioSource(GameObject owner, ref AudioSource audioSource)
    {
        if (audioSource == null && owner != null)
        {
            audioSource = owner.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = owner.AddComponent<AudioSource>();
            }
        }

        RegistrarAudioSource(audioSource);
        return audioSource;
    }

    public static void ReproducirClipEnPunto(AudioClip clip, Vector3 posicion, float volumenBase = 1f)
    {
        if (clip == null)
        {
            return;
        }

        GameObject tempAudio = new GameObject("TempSFX");
        tempAudio.transform.position = posicion;

        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.clip = clip;
        AplicarVolumenSfx(audioSource, volumenBase);
        audioSource.Play();

        UnityEngine.Object.Destroy(tempAudio, clip.length + 0.1f);
    }

    public static void AplicarVolumenSfxEnEscena(params AudioSource[] audiosExcluidos)
    {
        HashSet<int> idsExcluidos = new HashSet<int>();
        if (audiosExcluidos != null)
        {
            for (int i = 0; i < audiosExcluidos.Length; i++)
            {
                AudioSource audioExcluido = audiosExcluidos[i];
                if (audioExcluido != null)
                {
                    idsExcluidos.Add(audioExcluido.GetInstanceID());
                }
            }
        }

        if (MusicManager.Instance != null)
        {
            AudioSource[] audioSourcesMusica = MusicManager.Instance.GetComponents<AudioSource>();
            for (int i = 0; i < audioSourcesMusica.Length; i++)
            {
                AudioSource audioMusica = audioSourcesMusica[i];
                if (audioMusica != null)
                {
                    idsExcluidos.Add(audioMusica.GetInstanceID());
                }
            }
        }

        AudioSource[] audioSources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource audioSource = audioSources[i];
            if (audioSource == null || idsExcluidos.Contains(audioSource.GetInstanceID()))
            {
                continue;
            }

            AplicarVolumenSfx(audioSource);
        }
    }
}






