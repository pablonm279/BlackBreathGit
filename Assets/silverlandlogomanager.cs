using System.Collections;
using UnityEngine;

public class silverlandlogomanager : MonoBehaviour
{
    [SerializeField] float duracionLogo = 4f;
    [SerializeField] KeyCode teclaSkip = KeyCode.Escape;

    public static bool LogoCerradoEstaSesion { get; private set; }

    CanvasGroup canvasGroup;

    public bool IntroTerminada { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetEstadoSesion()
    {
        LogoCerradoEstaSesion = false;
    }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        IntroTerminada = LogoCerradoEstaSesion;

        if (LogoCerradoEstaSesion)
        {
            OcultarLogo();
            gameObject.SetActive(false);
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
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
        float tiempo = 0f;

        while (tiempo < duracionLogo)
        {
            if (Input.GetKeyDown(teclaSkip))
            {
                break;
            }

            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }

        if (canvasGroup != null)
        {
            OcultarLogo();
        }

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
}
