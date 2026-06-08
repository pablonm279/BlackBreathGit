using System.Collections;
using UnityEngine;

public class silverlandlogomanager : MonoBehaviour
{
    [SerializeField] float duracionLogo = 4f;
    [SerializeField] KeyCode teclaSkip = KeyCode.Escape;

    CanvasGroup canvasGroup;

    public bool IntroTerminada { get; private set; }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        IntroTerminada = false;

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
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        IntroTerminada = true;
        gameObject.SetActive(false);
    }
}
