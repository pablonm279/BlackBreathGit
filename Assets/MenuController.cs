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
    public GameObject disclaimerIngles;
    public GameObject disclaimerEspaniol;
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

        if (TRADU.i.nIdioma == 1)
        {
            logoEspaniol.SetActive(true);
            logoIngles.SetActive(false);
            disclaimerIngles.SetActive(false);
            disclaimerEspaniol.SetActive(true);
        }
        else if (TRADU.i.nIdioma == 2)
        {
            logoEspaniol.SetActive(false);
            logoIngles.SetActive(true);
            disclaimerIngles.SetActive(true);
            disclaimerEspaniol.SetActive(false);
        }

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
    }

    public void OnNuevaPartida()
    {
        MarcarTutorialComoCompletado();
        SaveGameService.ClearPendingLoad();
        StartCoroutine(CargarJuego());
    }

    public void OnNuevaPartidaTutorial()
    {
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
        PlayerPrefs.Save();
    }

    void ReiniciarTutorial()
    {
        PlayerPrefs.DeleteKey(TutorialTerminadoKey);
        PlayerPrefs.Save();
    }
}
