using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class EscaladoFuentesGlobal : MonoBehaviour
{
    public const string PrefEscalaTexto = "ui_escala_texto";
    public const float EscalaMinima = 0.8f;
    public const float EscalaMaxima = 1.05f;
    public const float EscalaPorDefecto = 0.95f;

    private static EscaladoFuentesGlobal instance;

    private readonly Dictionary<int, EstadoFuente> fuentes = new Dictionary<int, EstadoFuente>();
    private readonly Dictionary<int, EstadoTextoLegacy> textosLegacy = new Dictionary<int, EstadoTextoLegacy>();
    private bool aplicandoEscala;

    private sealed class EstadoFuente
    {
        public TextMeshProUGUI texto;
        public float fontSize;
        public float fontSizeMin;
        public float fontSizeMax;
    }

    private sealed class EstadoTextoLegacy
    {
        public Text texto;
        public int fontSize;
        public int resizeTextMinSize;
        public int resizeTextMaxSize;
    }

    public static float EscalaTextoActual
    {
        get
        {
            return Mathf.Clamp(
                PlayerPrefs.GetFloat(PrefEscalaTexto, EscalaPorDefecto),
                EscalaMinima,
                EscalaMaxima);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarEstadoEstatico()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Inicializar()
    {
        if (instance != null) { return; }

        GameObject go = new GameObject("EscaladoFuentesGlobal");
        instance = go.AddComponent<EscaladoFuentesGlobal>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextoCambiado);
    }

    private void Start()
    {
        StartCoroutine(AplicarDespuesDeUnFrame());
    }

    private void OnDestroy()
    {
        if (instance != this) { return; }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextoCambiado);
        instance = null;
    }

    public static void EstablecerEscalaTexto(float escala)
    {
        float escalaValida = Mathf.Clamp(escala, EscalaMinima, EscalaMaxima);
        PlayerPrefs.SetFloat(PrefEscalaTexto, escalaValida);

        AsegurarInstancia();
        instance.AplicarEscalaRegistrada();
    }

    public static void RegistrarClon(TextMeshProUGUI textoOriginal, TextMeshProUGUI textoClonado)
    {
        if (textoOriginal == null || textoClonado == null) { return; }

        AsegurarInstancia();
        instance.RegistrarClonInterno(textoOriginal, textoClonado);
    }

    private static void AsegurarInstancia()
    {
        if (instance == null)
        {
            Inicializar();
        }
    }

    private void OnSceneLoaded(Scene _, LoadSceneMode __)
    {
        StartCoroutine(AplicarDespuesDeUnFrame());
    }

    private IEnumerator AplicarDespuesDeUnFrame()
    {
        yield return null;
        AplicarATodosLosTextos();
    }

    private void AplicarATodosLosTextos()
    {
        LimpiarReferenciasDestruidas();

        TextMeshProUGUI[] textos = FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        aplicandoEscala = true;
        for (int i = 0; i < textos.Length; i++)
        {
            RegistrarYAplicar(textos[i]);
        }

        Text[] textosLegacyEncontrados = FindObjectsByType<Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < textosLegacyEncontrados.Length; i++)
        {
            RegistrarYAplicar(textosLegacyEncontrados[i]);
        }
        aplicandoEscala = false;
    }

    private void AplicarEscalaRegistrada()
    {
        LimpiarReferenciasDestruidas();

        aplicandoEscala = true;
        foreach (KeyValuePair<int, EstadoFuente> entrada in fuentes)
        {
            AplicarEstado(entrada.Value);
        }

        foreach (KeyValuePair<int, EstadoTextoLegacy> entrada in textosLegacy)
        {
            AplicarEstado(entrada.Value);
        }
        aplicandoEscala = false;
    }

    private void OnTextoCambiado(Object objeto)
    {
        if (aplicandoEscala) { return; }

        TextMeshProUGUI texto = objeto as TextMeshProUGUI;
        if (texto == null || fuentes.ContainsKey(texto.GetInstanceID())) { return; }

        RegistrarYAplicar(texto);
    }

    private void RegistrarYAplicar(TextMeshProUGUI texto)
    {
        if (!DebeEscalar(texto)) { return; }

        int id = texto.GetInstanceID();
        if (!fuentes.TryGetValue(id, out EstadoFuente estado))
        {
            estado = CrearEstado(texto);
            fuentes.Add(id, estado);
        }

        AplicarEstado(estado);
    }

    private void RegistrarYAplicar(Text texto)
    {
        if (!DebeEscalar(texto)) { return; }

        int id = texto.GetInstanceID();
        if (!textosLegacy.TryGetValue(id, out EstadoTextoLegacy estado))
        {
            estado = new EstadoTextoLegacy
            {
                texto = texto,
                fontSize = texto.fontSize,
                resizeTextMinSize = texto.resizeTextMinSize,
                resizeTextMaxSize = texto.resizeTextMaxSize
            };
            textosLegacy.Add(id, estado);
        }

        AplicarEstado(estado);
    }

    private void RegistrarClonInterno(TextMeshProUGUI textoOriginal, TextMeshProUGUI textoClonado)
    {
        if (!DebeEscalar(textoClonado)) { return; }

        int idOriginal = textoOriginal.GetInstanceID();
        if (!fuentes.TryGetValue(idOriginal, out EstadoFuente estadoOriginal))
        {
            estadoOriginal = CrearEstado(textoOriginal);
            fuentes[idOriginal] = estadoOriginal;
            AplicarEstado(estadoOriginal);
        }

        EstadoFuente estadoClonado = new EstadoFuente
        {
            texto = textoClonado,
            fontSize = estadoOriginal.fontSize,
            fontSizeMin = estadoOriginal.fontSizeMin,
            fontSizeMax = estadoOriginal.fontSizeMax
        };

        fuentes[textoClonado.GetInstanceID()] = estadoClonado;
        AplicarEstado(estadoClonado);
    }

    private static EstadoFuente CrearEstado(TextMeshProUGUI texto)
    {
        return new EstadoFuente
        {
            texto = texto,
            fontSize = texto.fontSize,
            fontSizeMin = texto.fontSizeMin,
            fontSizeMax = texto.fontSizeMax
        };
    }

    private void AplicarEstado(EstadoFuente estado)
    {
        if (estado == null || estado.texto == null) { return; }

        float escala = EscalaTextoActual;
        bool estabaAplicando = aplicandoEscala;
        aplicandoEscala = true;
        estado.texto.fontSizeMin = estado.fontSizeMin * escala;
        estado.texto.fontSizeMax = estado.fontSizeMax * escala;
        estado.texto.fontSize = estado.fontSize * escala;
        aplicandoEscala = estabaAplicando;
    }

    private static void AplicarEstado(EstadoTextoLegacy estado)
    {
        if (estado == null || estado.texto == null) { return; }

        float escala = EscalaTextoActual;
        estado.texto.resizeTextMinSize = Mathf.Max(1, Mathf.RoundToInt(estado.resizeTextMinSize * escala));
        estado.texto.resizeTextMaxSize = Mathf.Max(1, Mathf.RoundToInt(estado.resizeTextMaxSize * escala));
        estado.texto.fontSize = Mathf.Max(1, Mathf.RoundToInt(estado.fontSize * escala));
    }

    private static bool DebeEscalar(TextMeshProUGUI texto)
    {
        if (texto == null || !texto.gameObject.scene.IsValid()) { return false; }

        Canvas canvas = texto.GetComponentInParent<Canvas>(true);
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace) { return false; }

        if (texto.GetComponentInParent<FloatingTextAnimator>(true) != null) { return false; }
        if (texto.GetComponentInParent<TextoFlotanteManager>(true) != null) { return false; }
        if (texto.GetComponentInParent<AutodestruirDelay>(true) != null) { return false; }
        if (texto.GetComponentInParent<UnidadCanvas>(true) != null) { return false; }

        string ruta = ObtenerRutaJerarquia(texto.transform);
        return !ruta.Contains("floating")
            && !ruta.Contains("flotante")
            && !ruta.Contains("puntoorigenalertas");
    }

    private static bool DebeEscalar(Text texto)
    {
        if (texto == null || !texto.gameObject.scene.IsValid()) { return false; }

        Canvas canvas = texto.GetComponentInParent<Canvas>(true);
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace) { return false; }

        if (texto.GetComponentInParent<FloatingTextAnimator>(true) != null) { return false; }
        if (texto.GetComponentInParent<TextoFlotanteManager>(true) != null) { return false; }
        if (texto.GetComponentInParent<AutodestruirDelay>(true) != null) { return false; }
        if (texto.GetComponentInParent<UnidadCanvas>(true) != null) { return false; }

        string ruta = ObtenerRutaJerarquia(texto.transform);
        return !ruta.Contains("floating")
            && !ruta.Contains("flotante")
            && !ruta.Contains("puntoorigenalertas");
    }

    private static string ObtenerRutaJerarquia(Transform transformActual)
    {
        System.Text.StringBuilder ruta = new System.Text.StringBuilder(96);
        while (transformActual != null)
        {
            if (ruta.Length > 0) { ruta.Insert(0, '/'); }
            ruta.Insert(0, transformActual.name);
            transformActual = transformActual.parent;
        }

        return ruta.ToString().ToLowerInvariant();
    }

    private void LimpiarReferenciasDestruidas()
    {
        List<int> idsDestruidos = new List<int>();
        foreach (KeyValuePair<int, EstadoFuente> entrada in fuentes)
        {
            if (entrada.Value != null && entrada.Value.texto != null) { continue; }
            idsDestruidos.Add(entrada.Key);
        }

        for (int i = 0; i < idsDestruidos.Count; i++)
        {
            fuentes.Remove(idsDestruidos[i]);
        }

        idsDestruidos.Clear();
        foreach (KeyValuePair<int, EstadoTextoLegacy> entrada in textosLegacy)
        {
            if (entrada.Value != null && entrada.Value.texto != null) { continue; }
            idsDestruidos.Add(entrada.Key);
        }

        for (int i = 0; i < idsDestruidos.Count; i++)
        {
            textosLegacy.Remove(idsDestruidos[i]);
        }
    }
}
