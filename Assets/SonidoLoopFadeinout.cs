using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SonidoLoopFadeinout : MonoBehaviour
{
    [Range(0f, 1f)] public float volumenObjetivo = 1f;
    public float duracionFadeIn = 0.35f;
    public float duracionFadeOut = 0.35f;
    public bool autoPlay = true;

    AudioSource source;
    Coroutine rutina;
    bool loopOriginal;
    float volumenInicial;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        loopOriginal = source.loop;
        volumenInicial = source.volume;
    }

    void OnEnable()
    {
        if (autoPlay && source.clip != null)
            IniciarLoop();
    }

    void OnDisable()
    {
        DetenerLoop();
        if (source != null) source.loop = loopOriginal;
    }

    public void IniciarLoop()
    {
        if (source == null) source = GetComponent<AudioSource>();
        if (source.clip == null) return;

        volumenInicial = source.volume;
        if (rutina != null) StopCoroutine(rutina);
        rutina = StartCoroutine(CoLoopConFade());
    }

    public void DetenerLoop()
    {
        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }

        if (source != null)
        {
            source.Stop();
            source.volume = volumenInicial;
        }
    }

    IEnumerator CoLoopConFade()
    {
        source.loop = false; // se controla manualmente para poder hacer fade en cada vuelta
        float volBase = Mathf.Clamp01(volumenObjetivo > 0f ? volumenObjetivo : volumenInicial);

        while (enabled)
        {
            AudioClip clip = source.clip;
            if (clip == null) yield break;

            float fIn = Mathf.Clamp(duracionFadeIn, 0f, clip.length);
            float fOut = Mathf.Clamp(duracionFadeOut, 0f, clip.length);
            float tramoPlano = Mathf.Max(0f, clip.length - fIn - fOut);

            source.volume = 0f;
            source.time = 0f;
            source.Play();

            if (fIn > 0f) yield return Fade(0f, volBase, fIn);
            else source.volume = volBase;

            if (tramoPlano > 0f) yield return new WaitForSeconds(tramoPlano);

            if (fOut > 0f) yield return Fade(source.volume, 0f, fOut);
            else source.volume = 0f;

            source.Stop();
        }
    }

    IEnumerator Fade(float desde, float hasta, float tiempo)
    {
        if (tiempo <= 0f) { source.volume = hasta; yield break; }

        float t = 0f;
        while (t < tiempo)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / tiempo);
            source.volume = Mathf.Lerp(desde, hasta, k);
            yield return null;
        }
        source.volume = hasta;
    }
}
