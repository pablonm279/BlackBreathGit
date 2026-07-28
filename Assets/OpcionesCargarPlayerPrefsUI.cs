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
    private const string EscenaMenuPrincipal = "ES-MenuPrincipal";
    private const float DefaultBrightness = 0.65f;
    private const float AudioSfxOffsetY = -60.7f;
    private const float FpsLimitOffsetY = -85f;
    private const float EscalaTextoSliderOffsetY = -80f;
    private const float EscalaTextoSeparacionEtiquetaY = -19f;

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
    [NonSerialized] private Slider escalaTextoSlider;
    private TextMeshProUGUI etiquetaEscalaTexto;

    [Header("Visual Quality")]
    public Toggle postFxToggle;
    public Toggle aaToggle;
    public Toggle bloomToggle;
    public Toggle dofToggle;
    public Toggle vSyncToggle;
    public TMP_Dropdown fpsLimitDropdown;
    private TextMeshProUGUI fpsLimitLabel;

    [Header("Brillo")]
    public Slider brilloSlider;
    public TextMeshProUGUI brilloLabel;
    

    void Start()
    {
        InicializarCalibracionVisualPorDefecto();
        AsegurarControlesAudioSfx();
        AsegurarDropdownLimitadorFPS();
        ConfigurarToggleAyudas();
        AsegurarSliderEscalaTexto();
       
        LlenarDropdownResoluciones();
        AplicarEfectosEnUI();

    }
    void OnEnable()
    {
        InicializarCalibracionVisualPorDefecto();
        AsegurarControlesAudioSfx();
        AsegurarDropdownLimitadorFPS();
        ConfigurarToggleAyudas();
        AsegurarSliderEscalaTexto();
        
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
        AsegurarDropdownLimitadorFPS();
        AsegurarSliderEscalaTexto();

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
        QualitySettings.SetQualityLevel(calidadIndex, true);
        PlayerPrefs.SetInt("graficos_index", calidadIndex); // asegura persistencia inmediata
        PlayerPrefs.Save();
        VisualPolishRuntime.ApplyPostProcessingPrefsNow();
        graficosDropdown.SetValueWithoutNotify(calidadIndex);
        TraducirDropdownGraficos();
        TraducirDropdownDificultad();

        int difGuardada = PlayerPrefs.GetInt("dificultad_index", 2); // 2 = Normal por defecto (índice dropdown)
        difGuardada = Mathf.Clamp(difGuardada, 0, Mathf.Max(0, dificultadDropdown.options.Count - 1));
        dificultadDropdown.SetValueWithoutNotify(difGuardada);

        // Modo rapido
        bool modorapido = PlayerPrefs.GetInt("modoRapido", 0) == 1;
        if (modorapidoToggle != null)
            modorapidoToggle.SetIsOnWithoutNotify(modorapido);

        if (tipsToggle != null)
            tipsToggle.SetIsOnWithoutNotify(TutorialTooltipProgress.MostrarAyudas);

        if (escalaTextoSlider != null)
            escalaTextoSlider.SetValueWithoutNotify(EscaladoFuentesGlobal.EscalaTextoActual * 100f);

        ActualizarEtiquetaEscalaTexto();

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
        AsegurarDropdownLimitadorFPS();
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
        AsegurarSliderEscalaTexto();
        ActualizarEtiquetaEscalaTexto();
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

    public void VolverAlMenuPrincipal()
    {
        CambiarEfectos();
        Time.timeScale = 1f;
        SceneManager.LoadScene(EscenaMenuPrincipal, LoadSceneMode.Single);
    }

    void LlenarDropdownResoluciones()
    {
        if (resolucionDropdown == null)
            return;

        resolucionesSoportadas.Clear();
        resolucionDropdown.ClearOptions();

        // Resoluciones típicas que queremos mostrar
        List<Vector2Int> resolucionesDeseadas = new List<Vector2Int>()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1280, 800),
        new Vector2Int(1440, 900),
        new Vector2Int(1600, 900),
        new Vector2Int(1680, 1050),
        new Vector2Int(1920, 1080),
        new Vector2Int(1920, 1200),
        new Vector2Int(2560, 1440),
        new Vector2Int(2560, 1600),
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

        // Cargar índice guardado
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

        VisualPolishRuntime.ApplyPostProcessingPrefsNow();
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
    public void AplicarFPSLimit()
    {
        if (fpsLimitDropdown != null && MapearIndiceAFPS(fpsLimitDropdown.value) > 0)
        {
            if (vSyncToggle != null)
            {
                vSyncToggle.SetIsOnWithoutNotify(false);
            }

            PlayerPrefs.SetInt(PrefVsync, 0);
        }

        GuardarOpcionesVisuales();
        AplicarPreferenciasSyncYFPS();
        PlayerPrefs.Save();
    }

    private void AsegurarSliderEscalaTexto()
    {
        if (PanelGameplay == null)
        {
            return;
        }

        if (escalaTextoSlider == null)
        {
            Slider[] sliders = PanelGameplay.GetComponentsInChildren<Slider>(true);
            for (int i = 0; i < sliders.Length; i++)
            {
                Slider slider = sliders[i];
                if (slider != null && slider.name == "EscalaTextoSlider")
                {
                    escalaTextoSlider = slider;
                    break;
                }
            }
        }

        if (etiquetaEscalaTexto == null)
        {
            Transform etiquetaExistente = PanelGameplay.transform.Find("txtEscalaTexto");
            if (etiquetaExistente != null)
            {
                etiquetaEscalaTexto = etiquetaExistente.GetComponent<TextMeshProUGUI>();
            }
        }

        if (escalaTextoSlider == null)
        {
            Slider sliderBase = brilloSlider != null ? brilloSlider : volMusicaSlider;
            TextMeshProUGUI etiquetaBase = brilloLabel != null ? brilloLabel : etiquetaVolMusica;
            if (sliderBase == null || etiquetaBase == null)
            {
                return;
            }

            Transform parent = PanelGameplay.transform;

            GameObject clonEtiqueta = Instantiate(etiquetaBase.gameObject, parent);
            clonEtiqueta.name = "txtEscalaTexto";
            etiquetaEscalaTexto = clonEtiqueta.GetComponent<TextMeshProUGUI>();

            GameObject clonSlider = Instantiate(sliderBase.gameObject, parent);
            clonSlider.name = "EscalaTextoSlider";
            escalaTextoSlider = clonSlider.GetComponent<Slider>();
            if (escalaTextoSlider == null)
            {
                Destroy(clonEtiqueta);
                Destroy(clonSlider);
                etiquetaEscalaTexto = null;
                return;
            }

            escalaTextoSlider.onValueChanged = new Slider.SliderEvent();

            EscaladoFuentesGlobal.RegistrarClon(etiquetaBase, etiquetaEscalaTexto);
        }

        PosicionarControlEscalaTexto();
        escalaTextoSlider.minValue = EscaladoFuentesGlobal.EscalaMinima * 100f;
        escalaTextoSlider.maxValue = EscaladoFuentesGlobal.EscalaMaxima * 100f;
        escalaTextoSlider.wholeNumbers = true;
        escalaTextoSlider.onValueChanged.RemoveListener(OnEscalaTextoChanged);
        escalaTextoSlider.onValueChanged.AddListener(OnEscalaTextoChanged);
        escalaTextoSlider.SetValueWithoutNotify(EscaladoFuentesGlobal.EscalaTextoActual * 100f);
        ActualizarEtiquetaEscalaTexto();
    }

    private void PosicionarControlEscalaTexto()
    {
        if (PanelGameplay == null || escalaTextoSlider == null || etiquetaEscalaTexto == null) { return; }

        Transform parent = PanelGameplay.transform;
        float minY = ObtenerPosicionYControlMasBajo(parent, escalaTextoSlider);
        RectTransform sliderRect = escalaTextoSlider.transform as RectTransform;
        RectTransform etiquetaRect = etiquetaEscalaTexto.transform as RectTransform;
        if (sliderRect == null || etiquetaRect == null) { return; }

        Vector2 posicionSlider = sliderRect.anchoredPosition;
        posicionSlider.y = minY + EscalaTextoSliderOffsetY;
        sliderRect.anchoredPosition = posicionSlider;

        etiquetaRect.anchorMin = sliderRect.anchorMin;
        etiquetaRect.anchorMax = sliderRect.anchorMax;
        etiquetaRect.pivot = sliderRect.pivot;
        etiquetaRect.anchoredPosition = new Vector2(
            posicionSlider.x,
            posicionSlider.y + EscalaTextoSeparacionEtiquetaY);
    }

    private static float ObtenerPosicionYControlMasBajo(Transform parent, Slider sliderIgnorado)
    {
        float minY = 0f;
        bool encontroControl = false;

        for (int i = 0; i < parent.childCount; i++)
        {
            RectTransform rect = parent.GetChild(i) as RectTransform;
            if (rect == null) { continue; }
            if (sliderIgnorado != null && rect.gameObject == sliderIgnorado.gameObject) { continue; }

            bool esControl = rect.GetComponent<Toggle>() != null
                || rect.GetComponent<Slider>() != null
                || rect.GetComponent<TMP_Dropdown>() != null;
            if (!esControl) { continue; }

            minY = encontroControl ? Mathf.Min(minY, rect.anchoredPosition.y) : rect.anchoredPosition.y;
            encontroControl = true;
        }

        return minY;
    }

    private void OnEscalaTextoChanged(float porcentaje)
    {
        float escala = porcentaje / 100f;
        EscaladoFuentesGlobal.EstablecerEscalaTexto(escala);
        ActualizarEtiquetaEscalaTexto();
    }

    private void ActualizarEtiquetaEscalaTexto()
    {
        if (etiquetaEscalaTexto == null) { return; }

        int porcentaje = Mathf.RoundToInt(
            escalaTextoSlider != null
                ? escalaTextoSlider.value
                : EscaladoFuentesGlobal.EscalaTextoActual * 100f) - 5;
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : nIdioma;

        if (idioma == TRADU.IdiomaIngles)
        {
            etiquetaEscalaTexto.text = "Text size: " + porcentaje + "%";
        }
        else if (idioma == TRADU.IdiomaPortugues)
        {
            etiquetaEscalaTexto.text = "Tamanho do texto: " + porcentaje + "%";
        }
        else
        {
            etiquetaEscalaTexto.text = "Tamaño de texto: " + porcentaje + "%";
        }
    }


    private void CargarOpcionesVisuales()
    {
        AsegurarDropdownLimitadorFPS();

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

        if (fpsLimitDropdown.options == null || fpsLimitDropdown.options.Count != 6)
        {
            fpsLimitDropdown.ClearOptions();
            fpsLimitDropdown.AddOptions(new List<string>
            {
                "30 FPS",
                "60 FPS",
                "120 FPS",
                "144 FPS",
                "240 FPS",
                "Sin limite"
            });
        }

        ActualizarTextosLimitadorFPS();
    }

    private void AsegurarDropdownLimitadorFPS()
    {
        if (fpsLimitDropdown == null)
        {
            fpsLimitDropdown = BuscarDropdownLimitadorFPSExistente();
        }

        if (fpsLimitDropdown == null)
        {
            CrearDropdownLimitadorFPS();
        }

        if (fpsLimitDropdown == null) { return; }

        InicializarDropdownFPS();
        fpsLimitDropdown.onValueChanged.RemoveListener(OnFPSLimitDropdownChanged);
        fpsLimitDropdown.onValueChanged.AddListener(OnFPSLimitDropdownChanged);
    }

    private TMP_Dropdown BuscarDropdownLimitadorFPSExistente()
    {
        if (PanelGraficos == null) { return null; }

        TMP_Dropdown[] dropdowns = PanelGraficos.GetComponentsInChildren<TMP_Dropdown>(true);
        for (int i = 0; i < dropdowns.Length; i++)
        {
            TMP_Dropdown dropdown = dropdowns[i];
            if (dropdown == null) { continue; }

            string nombre = dropdown.name.ToLowerInvariant();
            if (nombre.Contains("fps") || nombre.Contains("framerate"))
            {
                return dropdown;
            }
        }

        return null;
    }

    private void CrearDropdownLimitadorFPS()
    {
        if (PanelGraficos == null || graficosDropdown == null) { return; }

        Transform parent = graficosDropdown.transform.parent;
        if (parent == null) { parent = PanelGraficos.transform; }

        RectTransform dropdownBaseRect = graficosDropdown.transform as RectTransform;
        float baseY = dropdownBaseRect != null ? dropdownBaseRect.anchoredPosition.y : 0f;
        float fpsY = CalcularPosicionYLimitadorFPS(parent, baseY);

        GameObject clonDropdown = Instantiate(graficosDropdown.gameObject, parent);
        clonDropdown.name = "FPSLimitDropdown";
        fpsLimitDropdown = clonDropdown.GetComponent<TMP_Dropdown>();
        if (fpsLimitDropdown == null) { return; }

        RectTransform fpsRect = fpsLimitDropdown.transform as RectTransform;
        if (fpsRect != null && dropdownBaseRect != null)
        {
            fpsRect.anchorMin = dropdownBaseRect.anchorMin;
            fpsRect.anchorMax = dropdownBaseRect.anchorMax;
            fpsRect.pivot = dropdownBaseRect.pivot;
            fpsRect.sizeDelta = dropdownBaseRect.sizeDelta;
            fpsRect.anchoredPosition = new Vector2(dropdownBaseRect.anchoredPosition.x, fpsY);
        }

        fpsLimitDropdown.onValueChanged.RemoveAllListeners();
        CrearEtiquetaLimitadorFPS(parent, baseY, fpsY);
    }

    private float CalcularPosicionYLimitadorFPS(Transform parent, float fallbackY)
    {
        float minY = fallbackY;
        bool encontroControl = false;

        for (int i = 0; i < parent.childCount; i++)
        {
            RectTransform child = parent.GetChild(i) as RectTransform;
            if (child == null) { continue; }

            if (child.GetComponent<TMP_Dropdown>() == null
                && child.GetComponent<Toggle>() == null
                && child.GetComponent<Slider>() == null)
            {
                continue;
            }

            minY = encontroControl ? Mathf.Min(minY, child.anchoredPosition.y) : child.anchoredPosition.y;
            encontroControl = true;
        }

        return minY + FpsLimitOffsetY;
    }

    private void CrearEtiquetaLimitadorFPS(Transform parent, float baseY, float fpsY)
    {
        TextMeshProUGUI etiquetaBase = BuscarEtiquetaGraficos();
        if (etiquetaBase == null) { return; }

        GameObject clonEtiqueta = Instantiate(etiquetaBase.gameObject, parent);
        clonEtiqueta.name = "txtLimitadorFPS";
        fpsLimitLabel = clonEtiqueta.GetComponent<TextMeshProUGUI>();

        RectTransform etiquetaRect = clonEtiqueta.transform as RectTransform;
        RectTransform etiquetaBaseRect = etiquetaBase.transform as RectTransform;
        if (etiquetaRect != null && etiquetaBaseRect != null)
        {
            etiquetaRect.anchorMin = etiquetaBaseRect.anchorMin;
            etiquetaRect.anchorMax = etiquetaBaseRect.anchorMax;
            etiquetaRect.pivot = etiquetaBaseRect.pivot;
            etiquetaRect.sizeDelta = etiquetaBaseRect.sizeDelta;

            Vector2 posicion = etiquetaBaseRect.parent == parent
                ? etiquetaBaseRect.anchoredPosition
                : new Vector2(etiquetaBaseRect.anchoredPosition.x, baseY);
            posicion.y += fpsY - baseY;
            etiquetaRect.anchoredPosition = posicion;
        }

        ActualizarTextosLimitadorFPS();
    }

    private TextMeshProUGUI BuscarEtiquetaGraficos()
    {
        if (PanelGraficos == null) { return null; }

        TextMeshProUGUI[] textos = PanelGraficos.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            TextMeshProUGUI texto = textos[i];
            if (texto == null) { continue; }

            string nombre = texto.name.ToLowerInvariant();
            string contenido = texto.text.ToLowerInvariant();
            if (nombre.Contains("calidad") || contenido.Contains("calidad") || contenido.Contains("quality"))
            {
                return texto;
            }
        }

        return textos.Length > 0 ? textos[0] : null;
    }

    private void OnFPSLimitDropdownChanged(int _)
    {
        AplicarFPSLimit();
    }

    private void ActualizarTextosLimitadorFPS()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : nIdioma;

        if (fpsLimitLabel == null && PanelGraficos != null)
        {
            Transform existente = PanelGraficos.transform.Find("txtLimitadorFPS");
            if (existente != null)
            {
                fpsLimitLabel = existente.GetComponent<TextMeshProUGUI>();
            }
        }

        if (fpsLimitLabel != null)
        {
            if (idioma == TRADU.IdiomaIngles)
            {
                fpsLimitLabel.text = "FPS Limit";
            }
            else if (idioma == TRADU.IdiomaPortugues)
            {
                fpsLimitLabel.text = "Limite de FPS";
            }
            else
            {
                fpsLimitLabel.text = "Limitador de FPS";
            }
        }

        if (fpsLimitDropdown == null || fpsLimitDropdown.options == null || fpsLimitDropdown.options.Count < 6)
        {
            return;
        }

        if (idioma == TRADU.IdiomaIngles)
        {
            fpsLimitDropdown.options[5].text = "Unlimited";
        }
        else if (idioma == TRADU.IdiomaPortugues)
        {
            fpsLimitDropdown.options[5].text = "Sem limite";
        }
        else
        {
            fpsLimitDropdown.options[5].text = "Sin limite";
        }

        fpsLimitDropdown.RefreshShownValue();
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
    ActualizarTextosLimitadorFPS();
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






