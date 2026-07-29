using System.Collections;
using TMPro;
using UnityEngine;

public class silverlandlogomanager : MonoBehaviour
{
    [SerializeField] float duracionLogo = 5f;
    [SerializeField] GameObject saveDisclaimer;
    [SerializeField] GameObject tapadorFades;
    [SerializeField] float duracionSaveDisclaimer = 7f;
    [SerializeField] float duracionFade = 1f;
    [SerializeField] KeyCode teclaSkip = KeyCode.Escape;

    public static bool LogoCerradoEstaSesion { get; private set; }

    CanvasGroup canvasGroup;
    CanvasGroup saveDisclaimerCanvasGroup;
    TMP_Text saveDisclaimerTexto;
    bool skipSolicitado;

    public bool IntroTerminada { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEstadoSesion()
    {
        LogoCerradoEstaSesion = false;
    }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        if (saveDisclaimer == null || tapadorFades == null)
        {
            GameObject[] objetosEscena = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objetosEscena.Length; i++)
            {
                if (objetosEscena[i].scene != gameObject.scene)
                {
                    continue;
                }

                if (saveDisclaimer == null && objetosEscena[i].name == "saveDisclaimer")
                {
                    saveDisclaimer = objetosEscena[i];
                }
                else if (tapadorFades == null && objetosEscena[i].name == "Tapadorfades")
                {
                    tapadorFades = objetosEscena[i];
                }
            }
        }

        if (saveDisclaimer != null)
        {
            Animator[] animatorsDisclaimer = saveDisclaimer.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animatorsDisclaimer.Length; i++)
            {
                animatorsDisclaimer[i].enabled = false;
            }

            Animation[] animacionesDisclaimer = saveDisclaimer.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < animacionesDisclaimer.Length; i++)
            {
                animacionesDisclaimer[i].enabled = false;
            }

            saveDisclaimerCanvasGroup = saveDisclaimer.GetComponent<CanvasGroup>();
            if (saveDisclaimerCanvasGroup == null)
            {
                saveDisclaimerCanvasGroup = saveDisclaimer.AddComponent<CanvasGroup>();
            }

            ConfigurarCanvasGroup(saveDisclaimerCanvasGroup, 0f);

            saveDisclaimerTexto = saveDisclaimer.GetComponentInChildren<TMP_Text>(true);
            if (saveDisclaimerTexto != null)
            {
                saveDisclaimerTexto.alpha = 0f;
            }

            // Mantenerlo activo y oculto evita un frame visible mientras TMP
            // reconstruye su geometría al reactivar el GameObject.
            saveDisclaimer.SetActive(true);
        }

        if (tapadorFades != null)
        {
            tapadorFades.SetActive(true);
        }

        IntroTerminada = LogoCerradoEstaSesion;

        if (LogoCerradoEstaSesion)
        {
            DesactivarTapadorFades();
            OcultarLogo();
            gameObject.SetActive(false);
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = false;
        }
    }

    void Start()
    {
        StartCoroutine(ReproducirIntro());
    }

    public IEnumerator EsperarFinIntro()
    {
        while (!IntroTerminada)
        {
            yield return null;
        }
    }

    IEnumerator ReproducirIntro()
    {
        // Dejar que la escena y la UI terminen su inicialización bajo negro.
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        yield return ReproducirPantalla(canvasGroup, duracionLogo);
        OcultarLogo();
        yield return new WaitForSecondsRealtime(0.5f);

        skipSolicitado = false;
        if (saveDisclaimerCanvasGroup != null)
        {
            yield return ReproducirPantalla(
                saveDisclaimerCanvasGroup,
                duracionSaveDisclaimer,
                true,
                saveDisclaimerTexto);
            saveDisclaimer.SetActive(false);
        }
        else
        {
            DesactivarTapadorFades();
        }

        FinalizarIntro();
    }

    IEnumerator ReproducirPantalla(
        CanvasGroup grupo,
        float duracionPantalla,
        bool desactivarTapadorAlIniciarFadeOut = false,
        TMP_Text textoDiferido = null)
    {
        if (grupo == null)
        {
            yield break;
        }

        float duracionFadeReal = Mathf.Min(duracionFade, duracionPantalla * 0.5f);
        int cantidadFades = textoDiferido != null ? 4 : 2;
        float tiempoVisible = Mathf.Max(0f, duracionPantalla - duracionFadeReal * cantidadFades);

        yield return Fade(grupo, 0f, 1f, duracionFadeReal);
        if (textoDiferido != null)
        {
            yield return FadeTexto(textoDiferido, 0f, 1f, duracionFadeReal);
        }

        float tiempo = 0f;
        while (!skipSolicitado && tiempo < tiempoVisible)
        {
            if (Input.GetKeyDown(teclaSkip))
            {
                skipSolicitado = true;
                break;
            }

            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }

        if (textoDiferido != null)
        {
            yield return FadeTexto(textoDiferido, textoDiferido.alpha, 0f, duracionFadeReal);
        }

        yield return Fade(grupo, grupo.alpha, 0f, duracionFadeReal);

        if (desactivarTapadorAlIniciarFadeOut)
        {
            float esperaRestanteTapador = Mathf.Max(0f, 2f - duracionFadeReal);
            if (esperaRestanteTapador > 0f)
            {
                yield return new WaitForSecondsRealtime(esperaRestanteTapador);
            }

            DesactivarTapadorFades();
        }
    }

    IEnumerator Fade(CanvasGroup grupo, float desde, float hasta, float duracion)
    {
        if (duracion <= 0f)
        {
            grupo.alpha = hasta;
            yield break;
        }

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            if (Input.GetKeyDown(teclaSkip))
            {
                skipSolicitado = true;
                yield break;
            }

            tiempo += Time.unscaledDeltaTime;
            grupo.alpha = Mathf.Lerp(desde, hasta, tiempo / duracion);
            yield return null;
        }

        grupo.alpha = hasta;
    }

    IEnumerator FadeTexto(TMP_Text texto, float desde, float hasta, float duracion)
    {
        if (duracion <= 0f)
        {
            texto.alpha = hasta;
            yield break;
        }

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            if (Input.GetKeyDown(teclaSkip))
            {
                skipSolicitado = true;
                yield break;
            }

            tiempo += Time.unscaledDeltaTime;
            texto.alpha = Mathf.Lerp(desde, hasta, tiempo / duracion);
            yield return null;
        }

        texto.alpha = hasta;
    }

    void FinalizarIntro()
    {
        OcultarLogo();
        LogoCerradoEstaSesion = true;
        IntroTerminada = true;
        gameObject.SetActive(false);
    }

    void OcultarLogo()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    void ConfigurarCanvasGroup(CanvasGroup grupo, float alpha)
    {
        grupo.alpha = alpha;
        grupo.blocksRaycasts = true;
        grupo.interactable = false;
    }

    void DesactivarTapadorFades()
    {
        if (tapadorFades != null)
        {
            tapadorFades.SetActive(false);
        }
    }
}
