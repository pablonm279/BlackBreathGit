using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZonaMusical
{
    public int idZona;
    public string nombreZona;
    [Header("Lista de campaña (exploración)")]
    public List<AudioClip> temasCampania = new List<AudioClip>();
    [Header("Lista de batalla (combate)")]
    public List<AudioClip> temasBatalla = new List<AudioClip>();
    [Header("Opcional: stinger al entrar en batalla")]
    public AudioClip stingerBatalla;

}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public enum ModoMusica { Campania, Batalla }

    [Header("Zonas")]
    public List<ZonaMusical> zonas = new List<ZonaMusical>();

    [Header("Audio")]
    [Range(0f, 1f)] public float volumenBase = 0.6f;
    [Tooltip("Segundos de fundido al cambiar de tema o modo")]
    public float fadeTime = 1.5f;

    [Header("Aliento Negro")]
    [Tooltip("Lista reproducida cuando el Aliento Negro alcanza el tier 3 o superior")]
    public List<AudioClip> temasAlientoNegro = new List<AudioClip>();
    [Tooltip("Duración del fade al entrar o salir de la música del Aliento Negro")]
    public float fadeAlientoNegro = 0.75f;

    [Header("Random")]
    public bool aleatorio = true;
    [Tooltip("Evita repetir el último tema al azar")]
    public bool evitarRepeticionInmediata = true;

    // Doble Source para crossfade
    AudioSource a, b;
    AudioSource activo, pasivo;
    AudioSource sfx; // para one-shots de UI/SFX simples

    // Estado
    ZonaMusical zonaActual;
    ModoMusica modoActual = ModoMusica.Campania;
    int ultimoIndexCampania = -1;
    int ultimoIndexBatalla = -1;
    int ultimoIndexAlientoNegro = -1;
    Coroutine rutinaCiclo;
    bool pausado = false;
    bool usandoListaAlientoNegro = false;

    void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        a = gameObject.AddComponent<AudioSource>();
        b = gameObject.AddComponent<AudioSource>();
        sfx = gameObject.AddComponent<AudioSource>();
        a.loop = false; b.loop = false;
        a.playOnAwake = false; b.playOnAwake = false;
        a.volume = 0f; b.volume = 0f;
        sfx.loop = false; sfx.playOnAwake = false; sfx.spatialBlend = 0f; sfx.volume = 1f;

        activo = a; pasivo = b;
    }

    // ----------------- API PÚBLICA -----------------

    /// Cambia a la zona y modo indicados (con crossfade).
    public void SetZonaYModo(int idZona, ModoMusica modo)
    {
        var z = zonas.Find(x => x.idZona == idZona);
        if (z == null)
        {
            Debug.LogWarning($"[MusicManager] Zona {idZona} no encontrada o sin listas.");
            return;
        }
        zonaActual = z;
        modoActual = modo;

        // Reinicia ciclo
        if (rutinaCiclo != null) StopCoroutine(rutinaCiclo);
        rutinaCiclo = StartCoroutine(Ciclo());
    }

    /// Atajo: campaña en zona
    public void PlayCampania(int idZona) => SetZonaYModo(idZona, ModoMusica.Campania);

    /// Atajo: batalla en zona (dispara stinger si existe)
    public void PlayBatalla(int idZona)
    {
        var z = zonas.Find(x => x.idZona == idZona);
        if (z == null) { Debug.LogWarning($"[MusicManager] Zona {idZona} no encontrada."); return; }

        zonaActual = z;
        modoActual = ModoMusica.Batalla;

        if (rutinaCiclo != null) StopCoroutine(rutinaCiclo);
        rutinaCiclo = StartCoroutine(Ciclo(true)); // true = intenta stinger
    }

    /// Vuelve a campaña manteniendo la misma zona
    public void VolverACampania()
    {
        if (zonaActual == null) return;
        SetZonaYModo(zonaActual.idZona, ModoMusica.Campania);
    }

    /// Pausar/Reanudar
    public void PausarMusica(bool estado)
    {
        pausado = estado;
        if (estado) { a.Pause(); b.Pause(); }
        else { a.UnPause(); b.UnPause(); }
    }

    /// Volumen global (con ajuste del activo)
    public void SetVolumen(float v)
    {
        volumenBase = Mathf.Clamp01(v);
        if (activo != null) activo.volume = volumenBase;
    }

    /// Forzar siguiente tema dentro del modo actual
    public void SiguienteTema()
    {
        if (rutinaCiclo != null) StopCoroutine(rutinaCiclo);
        rutinaCiclo = StartCoroutine(Ciclo());
    }

    // --- Utilidades para pausar con fade y reanudar ---
    public void FadeOutYParar(float tiempo)
    {
        StartCoroutine(FadeOutYPararCo(tiempo));
    }

    IEnumerator FadeOutYPararCo(float tiempo)
    {
        // Detener el ciclo para que no cambie de tema durante el fade
        if (rutinaCiclo != null)
        {
            StopCoroutine(rutinaCiclo);
            rutinaCiclo = null;
        }

        // Hacer fade out de ambas fuentes por seguridad
        if (a.isPlaying) yield return StartCoroutine(FadeOut(a, Mathf.Max(0.01f, tiempo)));
        if (b.isPlaying) yield return StartCoroutine(FadeOut(b, Mathf.Max(0.01f, tiempo)));

        a.volume = 0f; b.volume = 0f;
        a.Stop(); b.Stop();
    }

    public void ReanudarComoEstaba()
    {
        // Si ya había una zona/mode establecidos, reinicia el ciclo normal
        if (zonaActual != null)
        {
            if (rutinaCiclo != null) StopCoroutine(rutinaCiclo);
            rutinaCiclo = StartCoroutine(Ciclo());
        }
    }

    // Reproduce un SFX (2D), pausando la música con fade y reanudando el mismo tema/posición.
    public void PlaySFXYReanudar(AudioClip clip, float volumen = 1f, float fade = 0.6f)
    {
        if (clip == null) return;
        StartCoroutine(CoPlaySFXYReanudar_PauseResume(clip, volumen, Mathf.Max(0.01f, fade)));
    }

    IEnumerator CoPlaySFXYReanudar_PauseResume(AudioClip clip, float volumen, float fade)
    {
        // Guardar volúmenes actuales y cuáles suenan
        float a0 = a.volume, b0 = b.volume;
        bool aWas = a.isPlaying, bWas = b.isPlaying;

        // Fade down sin detener
        float t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fade);
            if (aWas) a.volume = Mathf.Lerp(a0, 0f, k);
            if (bWas) b.volume = Mathf.Lerp(b0, 0f, k);
            yield return null;
        }
        if (aWas) a.volume = 0f; if (bWas) b.volume = 0f;

        // Pausar para preservar time positions y congelar el ciclo
        PausarMusica(true);

        // Play SFX
        sfx.volume = Mathf.Clamp01(volumen);
        sfx.PlayOneShot(clip, sfx.volume);

        // Esperar duración del clip
        t = 0f;
        while (t < clip.length)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Reanudar música y subir al volumen previo
        PausarMusica(false);
        t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fade);
            if (aWas) a.volume = Mathf.Lerp(0f, a0, k);
            if (bWas) b.volume = Mathf.Lerp(0f, b0, k);
            yield return null;
        }
        if (aWas) a.volume = a0; if (bWas) b.volume = b0;
    }

    // ----------------- LÓGICA INTERNA -----------------

    bool DebeUsarMusicaAlientoNegro()
    {
        if (temasAlientoNegro == null || temasAlientoNegro.Count == 0) return false;
        if (CampaignManager.Instance == null) return false;
        return CampaignManager.Instance.GetTierAlientoNegro() >= 3f;
    }

    IEnumerator Ciclo(bool conStinger = false)
    {
        while (zonaActual != null)
        {
            bool usarAliento = modoActual == ModoMusica.Campania && DebeUsarMusicaAlientoNegro();
            if (usarAliento && (temasAlientoNegro == null || temasAlientoNegro.Count == 0))
            {
                usarAliento = false;
            }

            List<AudioClip> lista = usarAliento
                ? temasAlientoNegro
                : (modoActual == ModoMusica.Campania ? zonaActual.temasCampania : zonaActual.temasBatalla);

            if (usarAliento && (lista == null || lista.Count == 0))
            {
                Debug.LogWarning("[MusicManager] Lista de Aliento Negro vacía. Se usará la lista de zona.");
                usarAliento = false;
                lista = (modoActual == ModoMusica.Campania) ? zonaActual.temasCampania : zonaActual.temasBatalla;
            }

            if (lista == null || lista.Count == 0)
            {
                Debug.LogWarning($"[MusicManager] Lista vacía en {modoActual} de {zonaActual.nombreZona}");
                yield break;
            }

            int idx = SiguienteIndice(lista.Count, usarAliento);
            AudioClip clip = lista[idx];

            // Stinger de entrada a batalla
            if (!usarAliento && conStinger && modoActual == ModoMusica.Batalla && zonaActual.stingerBatalla != null)
            {
                pasivo.clip = zonaActual.stingerBatalla;
                pasivo.time = 0f;
                pasivo.volume = 0f;
                pasivo.Play();

                yield return StartCoroutine(CrossFade(pasivo, 0.85f * volumenBase, 0.25f));
                yield return new WaitForSeconds(pasivo.clip.length * 0.85f);
                yield return StartCoroutine(FadeOut(pasivo, 0.25f));
            }

            float fadeCorto = Mathf.Max(0.05f, fadeAlientoNegro);
            float fadeLargo = Mathf.Max(0.05f, fadeTime);
            bool playlistCambio = usarAliento != usandoListaAlientoNegro;

            // Crossfade al tema elegido
            pasivo.clip = clip;
            pasivo.time = 0f;
            pasivo.volume = 0f;
            pasivo.Play();

            float crossFade = (usarAliento || playlistCambio) ? fadeCorto : fadeLargo;
            yield return StartCoroutine(CrossFade(pasivo, volumenBase, crossFade));
            SwapFuentes();
            usandoListaAlientoNegro = usarAliento;

            // Esperar el tema menos el tiempo de fade
            float restante = Mathf.Max(0f, clip.length - crossFade);
            float t = 0f;
            bool forzarCambio = false;
            while (t < restante)
            {
                if (!pausado) t += Time.unscaledDeltaTime;
                bool deseaAlientoNegro = modoActual == ModoMusica.Campania && DebeUsarMusicaAlientoNegro();
                if (deseaAlientoNegro != usarAliento)
                {
                    forzarCambio = true;
                    break;
                }
                yield return null;
            }

            if (forzarCambio)
            {
                float salida = Mathf.Max(0.05f, fadeCorto * 0.5f);
                yield return StartCoroutine(FadeOut(activo, salida));
                continue;
            }

            // Fade out suave antes del siguiente
            float fadeSalida = (usarAliento || playlistCambio) ? fadeCorto * 0.8f : fadeLargo * 0.8f;
            yield return StartCoroutine(FadeOut(pasivo, Mathf.Max(0.05f, fadeSalida)));
        }
    }

    int SiguienteIndice(int count, bool usarListaAliento)
    {
        if (!aleatorio)
        {
            if (usarListaAliento)
            {
                ultimoIndexAlientoNegro = (ultimoIndexAlientoNegro + 1) % count;
                return ultimoIndexAlientoNegro;
            }
            if (modoActual == ModoMusica.Campania)
            {
                ultimoIndexCampania = (ultimoIndexCampania + 1) % count;
                return ultimoIndexCampania;
            }
            else
            {
                ultimoIndexBatalla = (ultimoIndexBatalla + 1) % count;
                return ultimoIndexBatalla;
            }
        }

        // Aleatorio con anti-repetición inmediata
        int last = usarListaAliento
            ? ultimoIndexAlientoNegro
            : (modoActual == ModoMusica.Campania ? ultimoIndexCampania : ultimoIndexBatalla);
        int idx = UnityEngine.Random.Range(0, count);
        if (evitarRepeticionInmediata && count > 1)
        {
            int safety = 0;
            while (idx == last && safety++ < 10) idx = UnityEngine.Random.Range(0, count);
        }
        if (usarListaAliento)
        {
            ultimoIndexAlientoNegro = idx;
        }
        else if (modoActual == ModoMusica.Campania)
        {
            ultimoIndexCampania = idx;
        }
        else
        {
            ultimoIndexBatalla = idx;
        }
        return idx;
    }

    IEnumerator CrossFade(AudioSource inSrc, float volObjetivo, float tiempo)
    {
        // sube inSrc, baja activo
        float t = 0f;
        float vIn0 = inSrc.volume;
        float vOut0 = activo.volume;

        while (t < tiempo)
        {
            t += Time.unscaledDeltaTime;
            float k = t / tiempo;
            inSrc.volume = Mathf.Lerp(vIn0, volObjetivo, k);
            activo.volume = Mathf.Lerp(vOut0, 0f, k);
            yield return null;
        }
        inSrc.volume = volObjetivo;
        activo.volume = 0f;
        activo.Stop();
    }

    IEnumerator FadeOut(AudioSource src, float tiempo)
    {
        float t = 0f;
        float v0 = src.volume;
        while (t < tiempo)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(v0, 0f, t / tiempo);
            yield return null;
        }
        src.volume = 0f;
        src.Stop();
    }

    void SwapFuentes()
    {
        // La que estaba entrando pasa a ser la activa
        var tmp = activo;
        activo = pasivo;
        pasivo = tmp;
    }
    
    // Barajar lista al azar
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
