using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpcionesCargarPlayerPrefsUI : MonoBehaviour
{
    private const string PrefPostFx = "gfx_postfx_enabled";
    private const string PrefAA = "gfx_aa_enabled";
    private const string PrefBloom = "gfx_bloom_enabled";
    private const string PrefDoF = "gfx_dof_enabled";
    private const string PrefVsync = "gfx_vsync";
    private const string PrefFpsLimit = "gfx_fps_limit";

    public Slider volMusicaSlider;
    public AudioSource musicaFondo;

    public int nIdioma;
    public Toggle EspaniolToggle;

    public Toggle EnglishToggle;
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

    [Header("Visual Quality")]
    public Toggle postFxToggle;
    public Toggle aaToggle;
    public Toggle bloomToggle;
    public Toggle dofToggle;
    public Toggle vSyncToggle;
    public TMP_Dropdown fpsLimitDropdown;


    void Start()
    {

        LlenarDropdownResoluciones();
        AplicarEfectosEnUI();

    }
    void OnEnable()
    {
        AplicarEfectosEnUI();
    }


    public void AplicarEfectosEnUI()
    {
        // Volumen de la másica
        float volumenMusica = PlayerPrefs.GetFloat("Vol_Musica", 0.8f);
        if (volMusicaSlider.value > 0.9f)
        { volumenMusica = 0.9f; }
        volMusicaSlider.value = volumenMusica;
        musicaFondo.volume = volumenMusica;

        // Sonido en segundo plano
        musicInBackground = PlayerPrefs.GetInt("Background_Sound", 1) == 1;
        musicInBackgroundToggle.isOn = musicInBackground;


        //Idioma
        nIdioma = PlayerPrefs.GetInt("nIdioma", 1);


        EspaniolToggle.SetIsOnWithoutNotify(false);
        EnglishToggle.SetIsOnWithoutNotify(false);

        if (nIdioma == 1)
        { EspaniolToggle.SetIsOnWithoutNotify(true); }
        else if (nIdioma == 2)
        { EnglishToggle.SetIsOnWithoutNotify(true); }
        else
        { EnglishToggle.SetIsOnWithoutNotify(true); }

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

        QualitySettings.SetQualityLevel(calidadIndex, true);
        PlayerPrefs.SetInt("graficos_index", calidadIndex); // asegura persistencia inmediata
        PlayerPrefs.Save();
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

        CargarOpcionesVisuales();
        AplicarPreferenciasSyncYFPS();
    }

    public void CambiarEfectos()
    {
        // Volumen de la másica
        PlayerPrefs.SetFloat("Vol_Musica", volMusicaSlider.value);
        if (musicaFondo.GetComponent<MusicManager>() != null)
        {
            musicaFondo.GetComponent<MusicManager>().SetVolumen(volMusicaSlider.value);
        }
        else {    musicaFondo.volume = volMusicaSlider.value;}
     

        // Sonido en segundo plano
        PlayerPrefs.SetInt("Background_Sound", musicInBackgroundToggle.isOn ? 1 : 0);
        Application.runInBackground = musicInBackgroundToggle.isOn;


        // Idioma
        if (EspaniolToggle.isOn)
        {
            nIdioma = 1;
            PlayerPrefs.SetInt("nIdioma", nIdioma);
            if (TRADU.i != null) TRADU.i.nIdioma = nIdioma;
            restartRequiredText.text = "Se requiere reiniciar para aplicar los cambios.";
        }
        else if (EnglishToggle.isOn)
        {
            nIdioma = 2;
            PlayerPrefs.SetInt("nIdioma", nIdioma);
            if (TRADU.i != null) TRADU.i.nIdioma = nIdioma;
            restartRequiredText.text = "Restart required to apply changes.";

        }

        // Resolución y pantalla completa

        AplicarResolucion();


        GuardarOpcionesVisuales();
        AplicarPreferenciasSyncYFPS();

        //---
        PlayerPrefs.Save();
    }



    public void AbrirPanelAudio()
    {
        PanelAudio.SetActive(true);
        PanelGraficos.SetActive(false);
        PanelControles.SetActive(false);
        PanelGameplay.SetActive(false);
        PanelIdioma.SetActive(false);
    }

    public void AbrirPanelGraficos()
    {
        PanelAudio.SetActive(false);
        PanelGraficos.SetActive(true);
        PanelControles.SetActive(false);
        PanelGameplay.SetActive(false);
        PanelIdioma.SetActive(false);
    }

    public void AbrirPanelControles()
    {
        PanelAudio.SetActive(false);
        PanelGraficos.SetActive(false);
        PanelControles.SetActive(true);
        PanelGameplay.SetActive(false);
        PanelIdioma.SetActive(false);

        int nidioma = TRADU.i.nIdioma;
        if (nidioma == 1)
        {
            PanelControles.transform.GetChild(0).gameObject.SetActive(true); // Español
            PanelControles.transform.GetChild(1).gameObject.SetActive(false); // Inglés

        }
        else if (nidioma == 2)
        {
            PanelControles.transform.GetChild(0).gameObject.SetActive(false); // Español
            PanelControles.transform.GetChild(1).gameObject.SetActive(true); // Inglés
        }
        else
        {

        }
    }

    public void AbrirPanelGameplay()
    {
        PanelAudio.SetActive(false);
        PanelGraficos.SetActive(false);
        PanelControles.SetActive(false);
        PanelGameplay.SetActive(true);
        PanelIdioma.SetActive(false);
    }
    public void AbrirPanelIdioma()
    {
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
        this.gameObject.SetActive(false);

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
        print($"Aplicando calidad gráfica nivel {index}");

        PlayerPrefs.SetInt("graficos_index", index);
        PlayerPrefs.Save();
        print($"playerprefs {PlayerPrefs.GetInt("graficos_index")}");

        AplicarPreferenciasSyncYFPS();

    }

    public void AplicarDificultad()
    {
        int index = dificultadDropdown.value;
        PlayerPrefs.SetInt("dificultad_index", index);
        PlayerPrefs.Save();

        if (CampaignManager.Instance != null) {CampaignManager.Instance.AjustarDificultad(); }
    }

    void TraducirDropdownGraficos()
    {
        if (TRADU.i.nIdioma == 1)
        {
            graficosDropdown.options[0].text = "Baja";
            graficosDropdown.options[1].text = "Media";
            graficosDropdown.options[2].text = "Ultra";
            // graficosDropdown.options[3].text = "Ultra";
        }
        else if (TRADU.i.nIdioma == 2)
        {
            graficosDropdown.options[0].text = "Low";
            graficosDropdown.options[1].text = "Medium";
            graficosDropdown.options[2].text = "Ultra";
            //   graficosDropdown.options[3].text = "Ultra";
        }


    }

    void TraducirDropdownDificultad()
    {
        if (dificultadDropdown == null || dificultadDropdown.options.Count < 5)
            return;

        if (TRADU.i.nIdioma == 1)
        {
            dificultadDropdown.options[0].text = "Muy Facil";
            dificultadDropdown.options[1].text = "Facil";
            dificultadDropdown.options[2].text = "Normal";
            dificultadDropdown.options[3].text = "Dificil";
            dificultadDropdown.options[4].text = "Muy Dificil";
        }
        else if (TRADU.i.nIdioma == 2)
        {
            dificultadDropdown.options[0].text = "Very Easy";
            dificultadDropdown.options[1].text = "Easy";
            dificultadDropdown.options[2].text = "Normal";
            dificultadDropdown.options[3].text = "Hard";
            dificultadDropdown.options[4].text = "Very Hard";
        }
    }

    public void AplicarModoRapido()
    {
        bool modorapido = modorapidoToggle.isOn;
        PlayerPrefs.SetInt("modoRapido", modorapido ? 1 : 0);
        PlayerPrefs.Save();

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
        if (vsync)
        {
            Application.targetFrameRate = -1;
        }
        else
        {
            if (fps <= 0) { Application.targetFrameRate = -1; }
            else { Application.targetFrameRate = Mathf.Clamp(fps, 30, 240); }
        }
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
            "Sin límite"
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


}




