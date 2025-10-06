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

    void Awake()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (fader != null) fader.alpha = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator Start()
    {
        // Fade-in al entrar al menú
        yield return FadeTo(0f, fadeTime);
    }

    public void OnNuevaPartida()
    {
        StartCoroutine(CargarJuego());
    }

    public void OnOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(!panelOpciones.activeSelf);
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
