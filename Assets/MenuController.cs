using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] string escenaJuego = "ES-Campaña";

    [Header("UI")]
    [SerializeField] CanvasGroup fader;        // Imagen negra con CanvasGroup
    [SerializeField] float fadeTime = 0.4f;
    [SerializeField] GameObject panelOpciones; // Opcional

    public GameObject logoIngles;
    public GameObject logoEspaniol;
    public GameObject disclaimerIngles;
    public GameObject disclaimerEspaniol;
    public GameObject Opciones;

    void Awake()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (fader != null) fader.alpha = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;




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

    }

    IEnumerator Start()
    {
        // Fade-in al entrar al menú
        yield return FadeTo(0f, fadeTime);
    }


    public void CambiarIdioma(int n)
    { 
        if (n != 1 && n != 2) return;

       // TRADU.i.nIdioma = n;
        PlayerPrefs.SetInt("nIdioma", n);
        PlayerPrefs.Save();

       

    }
    public void OnNuevaPartida()
    {
        StartCoroutine(CargarJuego());
    }
    public void abriropciones()
    {
        Opciones.SetActive(!Opciones.activeInHierarchy);
    }
   
    public void OnSalir()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    IEnumerator CargarJuego()
    {
        yield return FadeTo(1f, fadeTime);
        // Carga directa (simple y estable)
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
}
