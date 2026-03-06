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

    IEnumerator Start()
    {
        // Fade-in al entrar al menú
        yield return FadeTo(0f, fadeTime);
    }


    public void CambiarIdioma(int n)
    { 
     //   if (n != 1 && n != 2) return;

       // TRADU.i.nIdioma = n;
      //  PlayerPrefs.SetInt("nIdioma", n);
      //  PlayerPrefs.Save();

       

    }
    public void OnNuevaPartida()
    {
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



