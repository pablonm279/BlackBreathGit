using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BanterBattleUI : MonoBehaviour, IPointerEnterHandler
{
    private const string PrefBantersHabilitados = "gameplay_banters_enabled";

    private enum TipoSistema
    {
        Batalla,
        Campania
    }

    private sealed class SolicitudBanter
    {
        public Unidad hablante;
        public Personaje personajeCampania;
        public int idClaseCampania;
        public bool requiereHablanteVivo;
        public Sprite retrato;
        public string texto;
        public float duracion;
        public int prioridad;
        [System.NonSerialized]
        public float creadaEn;
    }

    private static BanterBattleUI instanciaBatalla;
    private static BanterBattleUI instanciaCampania;
    private readonly List<SolicitudBanter> cola = new List<SolicitudBanter>();
    private readonly Dictionary<UnityEngine.Object, float> ultimoInicioPorHablante =
        new Dictionary<UnityEngine.Object, float>();
    private readonly Dictionary<Image, Color> coloresOriginalesFlash =
        new Dictionary<Image, Color>();
    private RectTransform panel;
    private CanvasGroup canvasGroup;
    private Image imagenRetrato;
    private TextMeshProUGUI textoBanter;
    private AudioSource audioBanter;
    private float pitchAudioBase = 1f;
    private Color colorTextoBase = Color.white;
    private Coroutine procesarColaRoutine;
    private Coroutine mostrarRoutine;
    private readonly Dictionary<Image, Color> coloresOriginalesHover =
        new Dictionary<Image, Color>();
    private Vector2 posicionVisible;
    private Vector2 posicionOculta;
    private string textoActual;
    private UnityEngine.Object hablanteActual;
    private BanterBattleUI segundaVista;
    private bool ocupada;
    private bool sistemaCerrado;
    private float ultimoInicioBanter = -10f;
    private float silencioHasta;
    private int bantersConsecutivos;
    private Vector3 escalaBase;
    private TipoSistema tipoSistema;

    public static bool BantersHabilitados => PlayerPrefs.GetInt(PrefBantersHabilitados, 1) == 1;

    public static void EstablecerBantersHabilitados(bool habilitados)
    {
        PlayerPrefs.SetInt(PrefBantersHabilitados, habilitados ? 1 : 0);
        PlayerPrefs.Save();

        if (habilitados)
        {
            return;
        }

        instanciaBatalla?.CancelarSolicitudes(true);
        instanciaCampania?.CancelarSolicitudes(true);
    }

    public static void Instalar(BattleManager battleManager, GameObject prefab)
    {
        if (instanciaBatalla != null || battleManager == null)
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
        controlador.Inicializar(canvas.sortingOrder + 50, prefab, canvas, false, TipoSistema.Batalla);
        BanterBattleDirector.Instalar(battleManager);
    }

    public static void InstalarCampania(CampaignManager campaignManager, GameObject prefab = null)
    {
        if (instanciaCampania != null || campaignManager == null)
        {
            return;
        }

        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("GOBanter");
        }
        if (prefab == null)
        {
            Debug.LogWarning("BanterBattleUI: no se encontró el prefab GOBanter para campaña.");
            return;
        }

        Canvas canvas = campaignManager.goCanvas != null
            ? campaignManager.goCanvas.GetComponentInParent<Canvas>()
            : null;
        if (canvas == null && campaignManager.goCanvas != null)
        {
            canvas = campaignManager.goCanvas.GetComponentInChildren<Canvas>(true);
        }
        canvas = canvas != null && canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
        if (canvas == null)
        {
            Debug.LogWarning("BanterBattleUI: no se encontró el Canvas de campaña.");
            return;
        }

        GameObject vista = Instantiate(prefab, canvas.transform);
        vista.name = "GOBanter_Campania_Runtime";
        BanterBattleUI controlador = vista.AddComponent<BanterBattleUI>();
        controlador.Inicializar(150, prefab, canvas, false, TipoSistema.Campania);
        BanterCampaignDirector.Instalar(campaignManager);
    }

    public static bool Emitir(
        Sprite retrato,
        string texto,
        float duracion = 2.2f,
        int prioridad = 0,
        bool permitirDuplicado = false)
    {
        if (!BantersHabilitados
            || instanciaBatalla == null
            || PanelHabilidadIzquierdoAbierto()
            || string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        instanciaBatalla.Encolar(null, null, 0, false, retrato, texto.Trim(), duracion, prioridad, permitirDuplicado);
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
        if (!BantersHabilitados
            || instanciaBatalla == null
            || PanelHabilidadIzquierdoAbierto()
            || unidad == null
            || string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        instanciaBatalla.Encolar(
            unidad,
            null,
            0,
            true,
            retrato,
            texto.Trim(),
            duracion,
            prioridad,
            permitirDuplicado);
        return true;
    }

    public static bool EmitirCampania(
        Personaje personaje,
        string texto,
        float duracion = 3.2f,
        int prioridad = 0,
        bool permitirDuplicado = false)
    {
        if (!BantersHabilitados
            || instanciaCampania == null
            || personaje == null
            || string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        string textoLimpio = texto.Trim();
        instanciaCampania.Encolar(
            null,
            personaje,
            personaje.IDClase,
            true,
            personaje.spRetrato,
            textoLimpio,
            CalcularDuracionCampania(textoLimpio, duracion),
            prioridad,
            permitirDuplicado);
        return true;
    }

    public static bool EmitirCampaniaDoble(
        Personaje primerPersonaje,
        string primerTexto,
        Personaje segundoPersonaje,
        string segundoTexto,
        float duracion = 3.6f)
    {
        if (!BantersHabilitados
            || instanciaCampania == null
            || primerPersonaje == null
            || string.IsNullOrWhiteSpace(primerTexto))
        {
            return false;
        }

        instanciaCampania.CancelarSolicitudes(true);
        SolicitudBanter primera = CrearSolicitudCampania(primerPersonaje, primerTexto, duracion, 3);
        instanciaCampania.IniciarSolicitudInmediata(primera);
        float duracionMayor = primera.duracion;

        if (segundoPersonaje != null
            && segundoPersonaje != primerPersonaje
            && !string.IsNullOrWhiteSpace(segundoTexto)
            && instanciaCampania.segundaVista != null)
        {
            SolicitudBanter segunda = CrearSolicitudCampania(segundoPersonaje, segundoTexto, duracion, 3);
            instanciaCampania.segundaVista.IniciarSolicitudInmediata(segunda);
            duracionMayor = Mathf.Max(duracionMayor, segunda.duracion);
        }

        instanciaCampania.silencioHasta = Time.unscaledTime
            + duracionMayor
            + Random.Range(2.5f, 4f);

        return true;
    }

    public static void Finalizar()
    {
        if (instanciaBatalla == null)
        {
            BanterBattleDirector.Finalizar();
            return;
        }

        BanterBattleUI principal = instanciaBatalla;
        BanterBattleUI secundaria = principal.segundaVista;
        instanciaBatalla = null;
        principal.DetenerSistema();
        BanterBattleDirector.Finalizar();

        if (secundaria != null)
        {
            Destroy(secundaria.gameObject);
        }
        Destroy(principal.gameObject);
    }

    public static void FinalizarCampania()
    {
        BanterCampaignDirector.Finalizar();
        if (instanciaCampania == null)
        {
            return;
        }

        BanterBattleUI principal = instanciaCampania;
        BanterBattleUI secundaria = principal.segundaVista;
        instanciaCampania = null;
        principal.DetenerSistema();

        if (secundaria != null)
        {
            Destroy(secundaria.gameObject);
        }
        Destroy(principal.gameObject);
    }

    public static void CancelarCampania(bool interrumpirActuales = false)
    {
        instanciaCampania?.CancelarSolicitudes(interrumpirActuales);
    }

    public static void CancelarPorPanelHabilidad()
    {
        instanciaBatalla?.CancelarSolicitudes(true);
    }

    public static void InvalidarHablante(Unidad unidad)
    {
        if (instanciaBatalla == null || unidad == null)
        {
            return;
        }

        instanciaBatalla.cola.RemoveAll(solicitud => solicitud.hablante == unidad);
    }

    private void Inicializar(
        int sortingOrder,
        GameObject prefab,
        Canvas canvasPadre,
        bool esSegundaVista,
        TipoSistema sistema)
    {
        tipoSistema = sistema;
        if (!esSegundaVista)
        {
            if (sistema == TipoSistema.Campania)
            {
                instanciaCampania = this;
            }
            else
            {
                instanciaBatalla = this;
            }
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

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

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
            graficos[i].raycastTarget = true;
        }

        bool apareceDesdeDerecha = false;
        panel.anchorMin = new Vector2(apareceDesdeDerecha ? 1f : 0f, 0.76f);
        panel.anchorMax = panel.anchorMin;
        panel.pivot = new Vector2(0.5f, 0.5f);
        const float offsetYBatalla = 50f;
        float offsetY = sistema == TipoSistema.Batalla ? offsetYBatalla : 0f;
        if (esSegundaVista)
        {
            offsetY -= panel.rect.height + 52f;
        }
        const float margenDerechoCampania = -50f;
        float offsetX = apareceDesdeDerecha
            ? -((panel.rect.width * 0.5f) + margenDerechoCampania)
            : (panel.rect.width * 0.5f) - 150f;
        float distanciaOculta = Mathf.Max(220f, panel.rect.width) + 100f;
        if (apareceDesdeDerecha
            && IntentarObtenerRangoHorizontalGraficos(panel, graficos, out float minX, out float maxX))
        {
            float escalaX = Mathf.Abs(escalaBase.x);
            offsetX = -((maxX * escalaX) + margenDerechoCampania);
            distanciaOculta = Mathf.Max(
                distanciaOculta,
                ((maxX - minX) * escalaX) + 250f);
        }

        posicionVisible = new Vector2(offsetX, offsetY);
        Vector2 direccionOculta = apareceDesdeDerecha ? Vector2.right : Vector2.left;
        posicionOculta = posicionVisible + direccionOculta * distanciaOculta;
        panel.anchoredPosition = posicionOculta;
        transform.SetAsLastSibling();

        if (!esSegundaVista && prefab != null && canvasPadre != null)
        {
            GameObject segunda = Instantiate(prefab, canvasPadre.transform);
            segunda.name = "GOBanter_Runtime_2";
            segundaVista = segunda.AddComponent<BanterBattleUI>();
            segundaVista.Inicializar(sortingOrder + 1, null, null, true, sistema);
        }
    }

    private void Encolar(
        Unidad hablante,
        Personaje personajeCampania,
        int idClaseCampania,
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

        UnityEngine.Object claveHablante = hablante != null
            ? hablante
            : personajeCampania;
        if (claveHablante != null)
        {
            if (HablanteVisible(claveHablante) || HablanteEnCooldown(claveHablante))
            {
                return;
            }

            int prioridadPendiente = PrioridadPendienteDeHablante(claveHablante);
            if (prioridadPendiente > prioridad)
            {
                return;
            }
            cola.RemoveAll(item => ClaveHablante(item) == claveHablante);
        }

        SolicitudBanter solicitud = new SolicitudBanter
        {
            hablante = hablante,
            personajeCampania = personajeCampania,
            idClaseCampania = idClaseCampania,
            requiereHablanteVivo = requiereHablanteVivo,
            retrato = retrato,
            texto = texto,
            duracion = Mathf.Clamp(duracion, 1.2f, 6f),
            prioridad = prioridad,
            creadaEn = Time.unscaledTime
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

    private static SolicitudBanter CrearSolicitudCampania(
        Personaje personaje,
        string texto,
        float duracion,
        int prioridad)
    {
        return new SolicitudBanter
        {
            personajeCampania = personaje,
            idClaseCampania = personaje != null ? personaje.IDClase : 0,
            requiereHablanteVivo = true,
            retrato = personaje != null ? personaje.spRetrato : null,
            texto = texto != null ? texto.Trim() : string.Empty,
            duracion = CalcularDuracionCampania(texto, duracion),
            prioridad = prioridad,
            creadaEn = Time.unscaledTime
        };
    }

    private void IniciarSolicitudInmediata(SolicitudBanter solicitud)
    {
        if (!EsSolicitudValida(solicitud))
        {
            return;
        }

        ocupada = true;
        textoActual = solicitud.texto;
        hablanteActual = ClaveHablante(solicitud);
        RegistrarInicioHablante(solicitud);
        mostrarRoutine = StartCoroutine(MostrarYLiberar(solicitud));
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

    private int PrioridadPendienteDeHablante(UnityEngine.Object claveHablante)
    {
        int prioridad = -1;
        for (int i = 0; i < cola.Count; i++)
        {
            if (ClaveHablante(cola[i]) == claveHablante)
            {
                prioridad = Mathf.Max(prioridad, cola[i].prioridad);
            }
        }
        return prioridad;
    }

    private bool HablanteVisible(UnityEngine.Object claveHablante)
    {
        return hablanteActual == claveHablante
            || (segundaVista != null && segundaVista.hablanteActual == claveHablante);
    }

    private bool HablanteEnCooldown(UnityEngine.Object claveHablante)
    {
        if (!ultimoInicioPorHablante.TryGetValue(claveHablante, out float ultimoInicio))
        {
            return false;
        }

        float cooldown = tipoSistema == TipoSistema.Campania ? 10f : 5f;
        return Time.unscaledTime - ultimoInicio < cooldown;
    }

    private void RegistrarInicioHablante(SolicitudBanter solicitud)
    {
        UnityEngine.Object claveHablante = ClaveHablante(solicitud);
        BanterBattleUI principal = tipoSistema == TipoSistema.Campania
            ? instanciaCampania
            : instanciaBatalla;
        if (claveHablante != null && principal != null)
        {
            principal.ultimoInicioPorHablante[claveHablante] = Time.unscaledTime;
        }
    }

    private static UnityEngine.Object ClaveHablante(SolicitudBanter solicitud)
    {
        if (solicitud == null)
        {
            return null;
        }
        return solicitud.hablante != null
            ? solicitud.hablante
            : solicitud.personajeCampania;
    }

    private bool SolicitudCaducada(SolicitudBanter solicitud)
    {
        float permanenciaMaxima = tipoSistema == TipoSistema.Campania ? 4f : 2.5f;
        return solicitud == null
            || Time.unscaledTime - solicitud.creadaEn > permanenciaMaxima;
    }

    private IEnumerator ProcesarCola()
    {
        while (cola.Count > 0)
        {
            cola.RemoveAll(item => !EsSolicitudValida(item) || SolicitudCaducada(item));
            if (cola.Count == 0)
            {
                break;
            }

            if (tipoSistema == TipoSistema.Batalla && PanelHabilidadIzquierdoAbierto())
            {
                yield return null;
                continue;
            }

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
            UnityEngine.Object claveHablante = ClaveHablante(solicitud);
            if (!EsSolicitudValida(solicitud)
                || SolicitudCaducada(solicitud)
                || (claveHablante != null
                    && (HablanteVisible(claveHablante) || HablanteEnCooldown(claveHablante))))
            {
                continue;
            }

            vistaLibre.ocupada = true;
            vistaLibre.textoActual = solicitud.texto;
            vistaLibre.hablanteActual = claveHablante;
            vistaLibre.RegistrarInicioHablante(solicitud);
            vistaLibre.mostrarRoutine = vistaLibre.StartCoroutine(vistaLibre.MostrarYLiberar(solicitud));
            ultimoInicioBanter = Time.unscaledTime;
            bantersConsecutivos++;
            if (tipoSistema == TipoSistema.Campania)
            {
                silencioHasta = Time.unscaledTime
                    + solicitud.duracion
                    + Random.Range(2.5f, 4f);
                bantersConsecutivos = 0;
            }
            else if (bantersConsecutivos >= 2)
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
        mostrarRoutine = null;
        textoActual = null;
        hablanteActual = null;
        ocupada = false;
    }

    private IEnumerator Mostrar(SolicitudBanter solicitud)
    {
        Color colorHablante = ColorParaHablante(solicitud.hablante, solicitud.idClaseCampania);
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
        canvasGroup.blocksRaycasts = true;

        const float duracionEntrada = 0.28f;
        float tiempo = 0f;
        while (tiempo < duracionEntrada)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracionEntrada);
            float intensidadRebote = tipoSistema == TipoSistema.Campania ? 0.65f : 1.35f;
            float curva = EaseOutBack(t, intensidadRebote);
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
        canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!ocupada || mostrarRoutine == null)
        {
            return;
        }

        StopCoroutine(mostrarRoutine);
        mostrarRoutine = StartCoroutine(DescartarPorHover());
    }

    private IEnumerator DescartarPorHover()
    {
        canvasGroup.blocksRaycasts = false;
        if (audioBanter != null)
        {
            audioBanter.Stop();
        }

        Image[] imagenes = GetComponentsInChildren<Image>(true);
        for (int i = 0; i < imagenes.Length; i++)
        {
            Image imagen = imagenes[i];
            coloresOriginalesHover[imagen] = imagen.color;
            imagen.color = Color.Lerp(imagen.color, Color.white, 0.14f);
        }
        panel.localScale = escalaBase * 1.02f;

        const float duracionHighlight = 0.07f;
        float tiempo = 0f;
        while (tiempo < duracionHighlight)
        {
            tiempo += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(tiempo / duracionHighlight);
            yield return null;
        }

        RestaurarHighlightHover();
        panel.anchoredPosition = posicionOculta;
        panel.localScale = escalaBase;
        canvasGroup.alpha = 0f;
        mostrarRoutine = null;
        textoActual = null;
        hablanteActual = null;
        ocupada = false;
    }

    private void RestaurarHighlightHover()
    {
        foreach (KeyValuePair<Image, Color> par in coloresOriginalesHover)
        {
            if (par.Key != null)
            {
                par.Key.color = par.Value;
            }
        }
        coloresOriginalesHover.Clear();
    }

    private static bool EsSolicitudValida(SolicitudBanter solicitud)
    {
        if (solicitud == null || !solicitud.requiereHablanteVivo)
        {
            return solicitud != null;
        }

        if (solicitud.hablante != null)
        {
            return solicitud.hablante.HP_actual > 0
                && solicitud.hablante.gameObject.activeInHierarchy;
        }

        return solicitud.personajeCampania != null
            && !solicitud.personajeCampania.Camp_Muerto
            && solicitud.personajeCampania.gameObject.activeInHierarchy;
    }

    private Color ColorParaHablante(Unidad hablante, int idClaseCampania)
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

        switch (idClaseCampania)
        {
            case 1: return ColorDesdeHex("#D3D5D7");
            case 2: return ColorDesdeHex("#8FCB91");
            case 3: return ColorDesdeHex("#E8D98A");
            case 4: return ColorDesdeHex("#B394D6");
            case 5: return ColorDesdeHex("#80B7E8");
            case 6: return ColorDesdeHex("#D98BCB");
        }
        return colorTextoBase;
    }

    private static Color ColorDesdeHex(string hexadecimal)
    {
        return ColorUtility.TryParseHtmlString(hexadecimal, out Color color)
            ? color
            : Color.white;
    }

    private IEnumerator FlashHablante(Unidad hablante, Color color)
    {
        if (hablante == null
            || hablante.uImage == null
            || !hablante.uImage.gameObject.activeInHierarchy)
        {
            yield break;
        }

        Image imagenOrigen = hablante.uImage;
        if (coloresOriginalesFlash.ContainsKey(imagenOrigen))
        {
            yield break;
        }

        Color colorOriginal = imagenOrigen.color;
        coloresOriginalesFlash.Add(imagenOrigen, colorOriginal);
        Color colorIluminado = Color.Lerp(colorOriginal, Color.white, 0.38f);
        colorIluminado = Color.Lerp(colorIluminado, color, 0.18f);
        colorIluminado.r = Mathf.Max(colorOriginal.r, colorIluminado.r);
        colorIluminado.g = Mathf.Max(colorOriginal.g, colorIluminado.g);
        colorIluminado.b = Mathf.Max(colorOriginal.b, colorIluminado.b);
        colorIluminado.a = colorOriginal.a;

        const float duracionFlash = 0.22f;
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
                pulso * 0.65f);
            imagenOrigen.color = ultimoColorAplicado;
            yield return null;
        }

        if (imagenOrigen != null
            && ColoresAproximadamenteIguales(imagenOrigen.color, ultimoColorAplicado))
        {
            imagenOrigen.color = colorOriginal;
        }
        coloresOriginalesFlash.Remove(imagenOrigen);
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
        RestaurarFlashesActivos();
        RestaurarHighlightHover();
        procesarColaRoutine = null;
        mostrarRoutine = null;
        textoActual = null;
        hablanteActual = null;
        ocupada = false;
        panel.anchoredPosition = posicionOculta;
        panel.localScale = escalaBase;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        if (segundaVista != null)
        {
            segundaVista.DetenerSistema();
        }
    }

    private void CancelarSolicitudes(bool interrumpirActuales)
    {
        cola.Clear();
        if (procesarColaRoutine != null)
        {
            StopCoroutine(procesarColaRoutine);
            procesarColaRoutine = null;
        }

        if (!interrumpirActuales)
        {
            return;
        }

        ReiniciarVista();
        segundaVista?.ReiniciarVista();
        ultimoInicioBanter = -10f;
        silencioHasta = 0f;
        bantersConsecutivos = 0;
    }

    private void ReiniciarVista()
    {
        StopAllCoroutines();
        RestaurarFlashesActivos();
        RestaurarHighlightHover();
        if (audioBanter != null)
        {
            audioBanter.Stop();
        }
        textoActual = null;
        hablanteActual = null;
        ocupada = false;
        mostrarRoutine = null;
        panel.anchoredPosition = posicionOculta;
        panel.localScale = escalaBase;
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
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

    private static bool IntentarObtenerRangoHorizontalGraficos(
        RectTransform raiz,
        Graphic[] graficos,
        out float minX,
        out float maxX)
    {
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        Vector3[] esquinas = new Vector3[4];

        for (int i = 0; i < graficos.Length; i++)
        {
            RectTransform rectTransform = graficos[i] != null
                ? graficos[i].rectTransform
                : null;
            if (rectTransform == null)
            {
                continue;
            }

            rectTransform.GetWorldCorners(esquinas);
            for (int esquina = 0; esquina < esquinas.Length; esquina++)
            {
                float xLocal = raiz.InverseTransformPoint(esquinas[esquina]).x;
                minX = Mathf.Min(minX, xLocal);
                maxX = Mathf.Max(maxX, xLocal);
            }
        }

        return !float.IsInfinity(minX) && !float.IsInfinity(maxX);
    }

    private static float CalcularDuracionCampania(string texto, float duracionBase)
    {
        const float duracionMinima = 3.2f;
        const float duracionMaxima = 5.2f;
        const float caracteresPorSegundo = 18f;
        int caracteres = string.IsNullOrWhiteSpace(texto) ? 0 : texto.Trim().Length;
        float duracionLectura = caracteres / caracteresPorSegundo;
        return Mathf.Clamp(
            Mathf.Max(duracionMinima, duracionBase, duracionLectura),
            duracionMinima,
            duracionMaxima);
    }

    private static bool PanelHabilidadIzquierdoAbierto()
    {
        return BattleManager.Instance != null
            && BattleManager.Instance.PanelDescripcionHabilidadIzquierdoVisible;
    }

    private static float EaseOutBack(float t, float intensidad)
    {
        float ajuste = intensidad + 1f;
        float x = t - 1f;
        return 1f + ajuste * x * x * x + intensidad * x * x;
    }

    private void OnDestroy()
    {
        RestaurarFlashesActivos();
        if (instanciaBatalla == this)
        {
            instanciaBatalla = null;
        }
        if (instanciaCampania == this)
        {
            instanciaCampania = null;
        }
    }

    private void RestaurarFlashesActivos()
    {
        foreach (KeyValuePair<Image, Color> flash in coloresOriginalesFlash)
        {
            if (flash.Key != null)
            {
                flash.Key.color = flash.Value;
            }
        }
        coloresOriginalesFlash.Clear();
    }
}
