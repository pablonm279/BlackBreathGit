using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BanterBattleUI : MonoBehaviour
{
    private sealed class SolicitudBanter
    {
        public Unidad hablante;
        public bool requiereHablanteVivo;
        public Sprite retrato;
        public string texto;
        public float duracion;
        public int prioridad;
    }

    private static BanterBattleUI instance;
    private readonly List<SolicitudBanter> cola = new List<SolicitudBanter>();
    private RectTransform panel;
    private CanvasGroup canvasGroup;
    private Image imagenRetrato;
    private TextMeshProUGUI textoBanter;
    private AudioSource audioBanter;
    private float pitchAudioBase = 1f;
    private Color colorTextoBase = Color.white;
    private Coroutine procesarColaRoutine;
    private Vector2 posicionVisible;
    private Vector2 posicionOculta;
    private string textoActual;
    private BanterBattleUI segundaVista;
    private bool ocupada;
    private bool sistemaCerrado;
    private float ultimoInicioBanter = -10f;
    private float silencioHasta;
    private int bantersConsecutivos;
    private Vector3 escalaBase;

    public static void Instalar(BattleManager battleManager, GameObject prefab)
    {
        if (instance != null || battleManager == null)
        {
            return;
        }

        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("GOBanter");
        }
        if (prefab == null)
        {
            Debug.LogWarning("BanterBattleUI: no se encontró el prefab GOBanter.");
            return;
        }

        Canvas canvasReferencia = battleManager.scUIBarraOrdenTurno != null
            ? battleManager.scUIBarraOrdenTurno.GetComponentInParent<Canvas>()
            : null;
        Canvas canvas = canvasReferencia != null ? canvasReferencia.rootCanvas : null;
        if (canvas == null)
        {
            Debug.LogWarning("BanterBattleUI: no se encontró el Canvas de batalla.");
            return;
        }

        GameObject vista = Instantiate(prefab, canvas.transform);
        vista.name = "GOBanter_Runtime";
        BanterBattleUI controlador = vista.AddComponent<BanterBattleUI>();
        controlador.Inicializar(canvas.sortingOrder + 50, prefab, canvas, false);
        BanterBattleDirector.Instalar(battleManager);
    }

    public static bool Emitir(
        Sprite retrato,
        string texto,
        float duracion = 2.2f,
        int prioridad = 0,
        bool permitirDuplicado = false)
    {
        if (instance == null || string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        instance.Encolar(null, false, retrato, texto.Trim(), duracion, prioridad, permitirDuplicado);
        return true;
    }

    public static bool Emitir(
        Unidad unidad,
        string texto,
        float duracion = 2.2f,
        int prioridad = 0,
        bool permitirDuplicado = false)
    {
        Sprite retrato = null;
        if (unidad != null)
        {
            retrato = unidad.uRetrato != null
                ? unidad.uRetrato
                : (unidad.uImage != null ? unidad.uImage.sprite : null);
        }
        if (instance == null || unidad == null || string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        instance.Encolar(
            unidad,
            true,
            retrato,
            texto.Trim(),
            duracion,
            prioridad,
            permitirDuplicado);
        return true;
    }

    public static void Finalizar()
    {
        if (instance == null)
        {
            BanterBattleDirector.Finalizar();
            return;
        }

        BanterBattleUI principal = instance;
        BanterBattleUI secundaria = principal.segundaVista;
        instance = null;
        principal.DetenerSistema();
        BanterBattleDirector.Finalizar();

        if (secundaria != null)
        {
            Destroy(secundaria.gameObject);
        }
        Destroy(principal.gameObject);
    }

    public static void InvalidarHablante(Unidad unidad)
    {
        if (instance == null || unidad == null)
        {
            return;
        }

        instance.cola.RemoveAll(solicitud => solicitud.hablante == unidad);
    }

    private void Inicializar(
        int sortingOrder,
        GameObject prefab,
        Canvas canvasPadre,
        bool esSegundaVista)
    {
        if (!esSegundaVista)
        {
            instance = this;
        }
        panel = GetComponent<RectTransform>();
        escalaBase = panel.localScale;
        imagenRetrato = BuscarImagen("Retrato");
        textoBanter = BuscarTexto("Texto");
        if (textoBanter != null)
        {
            colorTextoBase = textoBanter.color;
        }
        audioBanter = GetComponentInChildren<AudioSource>(true);
        if (audioBanter != null)
        {
            pitchAudioBase = audioBanter.pitch;
            audioBanter.Stop();
        }

        Canvas canvasPropio = GetComponent<Canvas>();
        if (canvasPropio == null)
        {
            canvasPropio = gameObject.AddComponent<Canvas>();
        }
        canvasPropio.overrideSorting = true;
        canvasPropio.sortingOrder = sortingOrder;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Graphic[] graficos = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graficos.Length; i++)
        {
            graficos[i].raycastTarget = false;
        }

        panel.anchorMin = new Vector2(0f, 0.76f);
        panel.anchorMax = panel.anchorMin;
        panel.pivot = new Vector2(0.5f, 0.5f);
        float offsetY = esSegundaVista ? -(panel.rect.height + 36f) : 0f;
        posicionVisible = new Vector2((panel.rect.width * 0.5f) - 150f, offsetY);
        posicionOculta = posicionVisible + Vector2.left * (Mathf.Max(220f, panel.rect.width) + 100f);
        panel.anchoredPosition = posicionOculta;
        transform.SetAsLastSibling();

        if (!esSegundaVista && prefab != null && canvasPadre != null)
        {
            GameObject segunda = Instantiate(prefab, canvasPadre.transform);
            segunda.name = "GOBanter_Runtime_2";
            segundaVista = segunda.AddComponent<BanterBattleUI>();
            segundaVista.Inicializar(sortingOrder + 1, null, null, true);
        }
    }

    private void Encolar(
        Unidad hablante,
        bool requiereHablanteVivo,
        Sprite retrato,
        string texto,
        float duracion,
        int prioridad,
        bool permitirDuplicado)
    {
        if (sistemaCerrado)
        {
            return;
        }

        if (!permitirDuplicado
            && (texto == textoActual
                || (segundaVista != null && texto == segundaVista.textoActual)
                || ContieneTextoEnCola(texto)))
        {
            return;
        }

        SolicitudBanter solicitud = new SolicitudBanter
        {
            hablante = hablante,
            requiereHablanteVivo = requiereHablanteVivo,
            retrato = retrato,
            texto = texto,
            duracion = Mathf.Clamp(duracion, 1.2f, 6f),
            prioridad = prioridad
        };

        if (prioridad >= 2)
        {
            cola.RemoveAll(item => item.prioridad < prioridad);
        }

        int indice = cola.FindIndex(item => item.prioridad < prioridad);
        if (indice < 0)
        {
            cola.Add(solicitud);
        }
        else
        {
            cola.Insert(indice, solicitud);
        }

        const int maximoEnCola = 6;
        if (cola.Count > maximoEnCola)
        {
            cola.RemoveAt(cola.Count - 1);
        }

        if (procesarColaRoutine == null)
        {
            procesarColaRoutine = StartCoroutine(ProcesarCola());
        }
    }

    private bool ContieneTextoEnCola(string texto)
    {
        for (int i = 0; i < cola.Count; i++)
        {
            if (cola[i].texto == texto)
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator ProcesarCola()
    {
        while (cola.Count > 0)
        {
            float silencioRestante = silencioHasta - Time.unscaledTime;
            if (silencioRestante > 0f)
            {
                yield return new WaitForSecondsRealtime(silencioRestante);
                bantersConsecutivos = 0;
                silencioHasta = 0f;
                continue;
            }
            if (silencioHasta > 0f)
            {
                bantersConsecutivos = 0;
                silencioHasta = 0f;
            }

            BanterBattleUI vistaLibre = null;
            if (!ocupada)
            {
                vistaLibre = this;
            }
            else if (segundaVista != null && !segundaVista.ocupada)
            {
                vistaLibre = segundaVista;
            }

            if (vistaLibre == null)
            {
                yield return null;
                continue;
            }

            const float delayEntreBanters = 0.35f;
            float esperaRestante = delayEntreBanters - (Time.unscaledTime - ultimoInicioBanter);
            if (esperaRestante > 0f)
            {
                yield return new WaitForSecondsRealtime(esperaRestante);
                continue;
            }

            SolicitudBanter solicitud = cola[0];
            cola.RemoveAt(0);
            if (!EsSolicitudValida(solicitud))
            {
                continue;
            }

            vistaLibre.ocupada = true;
            vistaLibre.textoActual = solicitud.texto;
            vistaLibre.StartCoroutine(vistaLibre.MostrarYLiberar(solicitud));
            ultimoInicioBanter = Time.unscaledTime;
            bantersConsecutivos++;
            if (bantersConsecutivos >= 2)
            {
                silencioHasta = Time.unscaledTime
                    + solicitud.duracion
                    + Random.Range(4f, 6f);
            }
            yield return null;
        }

        procesarColaRoutine = null;
    }

    private IEnumerator MostrarYLiberar(SolicitudBanter solicitud)
    {
        yield return Mostrar(solicitud);
        textoActual = null;
        ocupada = false;
    }

    private IEnumerator Mostrar(SolicitudBanter solicitud)
    {
        Color colorHablante = ColorParaHablante(solicitud.hablante);
        if (solicitud.hablante != null)
        {
            StartCoroutine(FlashHablante(solicitud.hablante, colorHablante));
        }

        if (audioBanter != null)
        {
            audioBanter.pitch = pitchAudioBase * Random.Range(0.95f, 1.05f);
            audioBanter.Play();
        }

        if (imagenRetrato != null)
        {
            imagenRetrato.sprite = solicitud.retrato;
            imagenRetrato.enabled = solicitud.retrato != null;
        }

        int cantidadCaracteres = 0;
        if (textoBanter != null)
        {
            textoBanter.color = colorHablante;
            textoBanter.text = solicitud.texto;
            textoBanter.maxVisibleCharacters = 0;
            textoBanter.ForceMeshUpdate();
            cantidadCaracteres = textoBanter.textInfo.characterCount;
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        panel.anchoredPosition = posicionOculta;
        canvasGroup.alpha = 0f;

        const float duracionEntrada = 0.28f;
        float tiempo = 0f;
        while (tiempo < duracionEntrada)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionEntrada);
            float curva = EaseOutBack(t);
            panel.anchoredPosition = Vector2.LerpUnclamped(posicionOculta, posicionVisible, curva);
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
            if (textoBanter != null)
            {
                textoBanter.maxVisibleCharacters = Mathf.CeilToInt(cantidadCaracteres * t);
            }
            yield return null;
        }

        panel.anchoredPosition = posicionVisible;
        canvasGroup.alpha = 1f;
        if (textoBanter != null)
        {
            textoBanter.maxVisibleCharacters = cantidadCaracteres;
        }

        const float duracionPulso = 0.14f;
        tiempo = 0f;
        while (tiempo < duracionPulso)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionPulso);
            float escala = Mathf.Lerp(1.035f, 1f, Mathf.SmoothStep(0f, 1f, t));
            panel.localScale = escalaBase * escala;
            yield return null;
        }
        panel.localScale = escalaBase;

        yield return new WaitForSecondsRealtime(solicitud.duracion);

        const float duracionSalida = 0.22f;
        tiempo = 0f;
        while (tiempo < duracionSalida)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracionSalida));
            panel.anchoredPosition = Vector2.LerpUnclamped(posicionVisible, posicionOculta, t);
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        panel.anchoredPosition = posicionOculta;
        panel.localScale = escalaBase;
        canvasGroup.alpha = 0f;
    }

    private static bool EsSolicitudValida(SolicitudBanter solicitud)
    {
        if (solicitud == null || !solicitud.requiereHablanteVivo)
        {
            return solicitud != null;
        }

        return solicitud.hablante != null
            && solicitud.hablante.HP_actual > 0
            && solicitud.hablante.gameObject.activeInHierarchy;
    }

    private Color ColorParaHablante(Unidad hablante)
    {
        if (hablante is ClaseCaballero)
        {
            return ColorDesdeHex("#D3D5D7");
        }
        if (hablante is ClaseCanalizador)
        {
            return ColorDesdeHex("#80B7E8");
        }
        if (hablante is ClaseDuelista)
        {
            return ColorDesdeHex("#D98BCB");
        }
        if (hablante is ClasePurificadora)
        {
            return ColorDesdeHex("#E8D98A");
        }
        if (hablante is ClaseExplorador)
        {
            return ColorDesdeHex("#8FCB91");
        }
        if (hablante is ClaseAcechador)
        {
            return ColorDesdeHex("#B394D6");
        }
        return colorTextoBase;
    }

    private static Color ColorDesdeHex(string hexadecimal)
    {
        return ColorUtility.TryParseHtmlString(hexadecimal, out Color color)
            ? color
            : Color.white;
    }

    private static IEnumerator FlashHablante(Unidad hablante, Color color)
    {
        if (hablante == null
            || hablante.uImage == null
            || !hablante.uImage.gameObject.activeInHierarchy)
        {
            yield break;
        }

        Image imagenOrigen = hablante.uImage;
        Color colorOriginal = imagenOrigen.color;
        Color colorIluminado = Color.Lerp(Color.white, color, 0.45f);
        colorIluminado.r = Mathf.Max(colorOriginal.r, colorIluminado.r);
        colorIluminado.g = Mathf.Max(colorOriginal.g, colorIluminado.g);
        colorIluminado.b = Mathf.Max(colorOriginal.b, colorIluminado.b);
        colorIluminado.a = colorOriginal.a;

        Outline contorno = imagenOrigen.gameObject.AddComponent<Outline>();
        contorno.effectDistance = new Vector2(3f, -3f);
        contorno.useGraphicAlpha = true;

        const float duracionFlash = 0.22f;
        const float alphaContornoMaxima = 0.42f;
        float tiempo = 0f;
        Color ultimoColorAplicado = colorOriginal;
        while (tiempo < duracionFlash
            && hablante != null
            && imagenOrigen != null)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionFlash);
            float pulso = Mathf.Sin(t * Mathf.PI);
            ultimoColorAplicado = Color.Lerp(
                colorOriginal,
                colorIluminado,
                pulso * 0.45f);
            imagenOrigen.color = ultimoColorAplicado;
            contorno.effectColor = new Color(
                color.r,
                color.g,
                color.b,
                pulso * alphaContornoMaxima);
            yield return null;
        }

        if (imagenOrigen != null
            && ColoresAproximadamenteIguales(imagenOrigen.color, ultimoColorAplicado))
        {
            imagenOrigen.color = colorOriginal;
        }
        if (contorno != null)
        {
            Destroy(contorno);
        }
    }

    private static bool ColoresAproximadamenteIguales(Color a, Color b)
    {
        const float tolerancia = 0.01f;
        return Mathf.Abs(a.r - b.r) < tolerancia
            && Mathf.Abs(a.g - b.g) < tolerancia
            && Mathf.Abs(a.b - b.b) < tolerancia
            && Mathf.Abs(a.a - b.a) < tolerancia;
    }

    private void DetenerSistema()
    {
        sistemaCerrado = true;
        cola.Clear();
        StopAllCoroutines();
        procesarColaRoutine = null;
        textoActual = null;
        ocupada = false;
        panel.anchoredPosition = posicionOculta;
        panel.localScale = escalaBase;
        canvasGroup.alpha = 0f;

        if (segundaVista != null)
        {
            segundaVista.DetenerSistema();
        }
    }

    private Image BuscarImagen(string nombre)
    {
        Image[] imagenes = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < imagenes.Length; i++)
        {
            if (imagenes[i].name == nombre)
            {
                return imagenes[i];
            }
        }
        return null;
    }

    private TextMeshProUGUI BuscarTexto(string nombre)
    {
        TextMeshProUGUI[] textos = GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            if (textos[i].name == nombre)
            {
                return textos[i];
            }
        }
        return null;
    }

    private static float EaseOutBack(float t)
    {
        const float intensidad = 1.35f;
        const float ajuste = intensidad + 1f;
        float x = t - 1f;
        return 1f + ajuste * x * x * x + intensidad * x * x;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
