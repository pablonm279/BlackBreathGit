using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LogDeCampania : Bitacora
{
    private struct PaginaLibro
    {
        public int InicioIzquierda;
        public int FinIzquierda;
        public int InicioDerecha;
        public int FinDerecha;
    }

    private struct MedicionEntradaLibro
    {
        public float AlturaVisual;
        public float AlturaAvance;
    }

    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI txtLog;
    [SerializeField] private TMP_SpriteAsset spriteAssetRecursos;
    [SerializeField] private TMP_SpriteAsset spriteAssetCombate;
    [SerializeField] private RectTransform raizLibro;
    [SerializeField] private TextMeshProUGUI plantillaEntradaLibro;
    [SerializeField] private TextMeshProUGUI txtPaginaActual;
    [SerializeField] private GameObject botonPaginaAnterior;
    [SerializeField] private GameObject botonPaginaSiguiente;

    [Header("Comportamiento")]
    [Tooltip("Maxima cantidad de entradas de combate visibles.")]
    [SerializeField] private int maxEntradas = 90;

    [Header("Libro")]
    [SerializeField] private bool usarLayoutLibro = true;
    [SerializeField] private Vector2 paginaIzquierdaAnchorMin = new Vector2(0.215f, 0.18f);
    [SerializeField] private Vector2 paginaIzquierdaAnchorMax = new Vector2(0.49f, 0.835f);
    [SerializeField] private Vector2 paginaDerechaAnchorMin = new Vector2(0.54f, 0.18f);
    [SerializeField] private Vector2 paginaDerechaAnchorMax = new Vector2(0.815f, 0.835f);
    [SerializeField] private float espaciadoEntradasLibro = 14f;
    [SerializeField] private float fontSizeLibro = 22f;
    [SerializeField] private Color colorTextoLibro = new Color(0.17f, 0.12f, 0.08f, 1f);
    [SerializeField] private float fontSizeIndicadorPagina = 16f;
    [SerializeField] private bool mostrarRenglonesLibro = true;
    [SerializeField] private Color colorRenglonesLibro = new Color(0.22f, 0.16f, 0.1f, 0.18f);
    [SerializeField] private float grosorRenglonesLibro = 1.2f;
    [SerializeField] private float margenHorizontalRenglonesLibro = 4f;

    [Header("Debug")]
    [Tooltip("En Play Mode, al activarlo imprime una sola vez un texto flotante de campana con todos los recursos. Desactivar y reactivar para repetir.")]
    [SerializeField] private bool debugTextoFlotanteRecursos;
    [SerializeField] private string debugMensajeTextoFlotanteRecursos =
        "Recurso Materiales +100 Materiales | Esperanza | Civiles | Oro | Fatiga | Suministros | Bueyes | Aliento Negro";

    [Header("Estilos")]
    [SerializeField] private string colorDia = "#2c81b9ff";
    [SerializeField] private string colorActual = "#ffffffff";
    [SerializeField] private string colorPasado = "#d4d4d4ff";
    [SerializeField] private int sizeActualPct = 115;
    [SerializeField] private int sizePasadoPct = 80;

    private readonly List<PaginaLibro> paginasLibro = new List<PaginaLibro>();
    private readonly List<TextMeshProUGUI> entradasLibro = new List<TextMeshProUGUI>();
    private readonly List<Image> renglonesPaginaIzquierda = new List<Image>();
    private readonly List<Image> renglonesPaginaDerecha = new List<Image>();
    private RectTransform paginaIzquierdaRuntime;
    private RectTransform paginaDerechaRuntime;
    private RectTransform renglonesIzquierdaRuntime;
    private RectTransform renglonesDerechaRuntime;
    private TextMeshProUGUI medidorEntradaLibro;
    private TextMeshProUGUI indicadorPaginaLibro;
    private int cacheCantidadEntradasLibro = -1;
    private Vector2 cacheTamanoRaizLibro = new Vector2(float.NaN, float.NaN);
    private int reacomodosLibroPendientes;

    protected override TextMeshProUGUI LogText => txtLog;
    public override TMP_SpriteAsset SpriteAssetRecursos => spriteAssetRecursos;
    public override TMP_SpriteAsset SpriteAssetCombate => spriteAssetCombate;
    protected override int MaxEntradasCombate => Mathf.Max(1, maxEntradas);
    protected override string ColorDia => colorDia;
    protected override string ColorActual => colorActual;
    protected override string ColorPasado => colorPasado;
    protected override int SizeActualPct => sizeActualPct;
    protected override int SizePasadoPct => sizePasadoPct;
    protected override bool DebugTextoFlotanteRecursos
    {
        get => debugTextoFlotanteRecursos;
        set => debugTextoFlotanteRecursos = value;
    }

    protected override string DebugMensajeTextoFlotanteRecursos => debugMensajeTextoFlotanteRecursos;

    public void PaginaAnterior()
    {
        MostrarPaginaAnterior();
    }

    public void PaginaSiguiente()
    {
        MostrarPaginaSiguiente();
    }

    public void ForzarReacomodoLibro()
    {
        SolicitarReacomodoLibro();
        ProcesarReacomodoLibroPendiente();
    }

    private void OnEnable()
    {
        SolicitarReacomodoLibro();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || !usarLayoutLibro)
        {
            return;
        }

        if (!TryPrepararLibro())
        {
            return;
        }

        ActualizarRenglonesLibro();
        ProcesarReacomodoLibroPendiente();

        if (raizLibro == null || !raizLibro.gameObject.activeInHierarchy || paginasLibro.Count <= 1)
        {
            return;
        }

        float ruedaMouse = Input.mouseScrollDelta.y;
        if (ruedaMouse > 0.01f || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.PageUp))
        {
            MostrarPaginaAnterior();
        }
        else if (ruedaMouse < -0.01f || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.PageDown))
        {
            MostrarPaginaSiguiente();
        }
    }

    protected override bool TryGetCantidadPaginasCampania(out int cantidadPaginas)
    {
        cantidadPaginas = 0;
        if (!TryPrepararLibro() || CantidadEntradasCampania <= 0)
        {
            return false;
        }

        RecalcularPaginasLibroSiHaceFalta();
        cantidadPaginas = Mathf.Max(1, paginasLibro.Count);
        return true;
    }

    protected override bool TryGetRangoPaginaCampania(int pagina, out int indiceInicial, out int indiceFinal)
    {
        indiceInicial = 0;
        indiceFinal = 0;
        if (!TryPrepararLibro() || CantidadEntradasCampania <= 0)
        {
            return false;
        }

        RecalcularPaginasLibroSiHaceFalta();
        if (paginasLibro.Count == 0)
        {
            return false;
        }

        PaginaLibro paginaLibro = paginasLibro[Mathf.Clamp(pagina - 1, 0, paginasLibro.Count - 1)];
        indiceInicial = paginaLibro.InicioIzquierda;
        indiceFinal = paginaLibro.FinDerecha;
        return true;
    }

    protected override bool TryRenderizarCampaniaPersonalizada()
    {
        if (!TryPrepararLibro() || CantidadEntradasCampania <= 0)
        {
            return false;
        }

        RecalcularPaginasLibroSiHaceFalta();
        if (paginasLibro.Count == 0)
        {
            return false;
        }

        if (txtLog != null)
        {
            txtLog.enabled = false;
        }

        int pagina = Mathf.Clamp(GetPaginaVisible(), 1, paginasLibro.Count);
        RenderizarPaginaLibro(pagina);
        return true;
    }

    protected override void LimpiarRenderCampaniaPersonalizado()
    {
        if (txtLog != null)
        {
            txtLog.enabled = true;
        }

        for (int i = 0; i < entradasLibro.Count; i++)
        {
            if (entradasLibro[i] != null)
            {
                entradasLibro[i].gameObject.SetActive(false);
            }
        }

        if (indicadorPaginaLibro != null)
        {
            indicadorPaginaLibro.gameObject.SetActive(false);
        }

        ActualizarBotonesNavegacion(1, 1);
    }

    private void SolicitarReacomodoLibro()
    {
        if (!usarLayoutLibro)
        {
            return;
        }

        reacomodosLibroPendientes = Mathf.Max(reacomodosLibroPendientes, 3);
        InvalidarCachePaginasLibro();
    }

    private void ProcesarReacomodoLibroPendiente()
    {
        if (reacomodosLibroPendientes <= 0)
        {
            return;
        }

        if (!TryPrepararLibro() || raizLibro == null || !raizLibro.gameObject.activeInHierarchy)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(raizLibro);
        if (paginaIzquierdaRuntime != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(paginaIzquierdaRuntime);
        }

        if (paginaDerechaRuntime != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(paginaDerechaRuntime);
        }

        Canvas.ForceUpdateCanvases();

        if (paginaIzquierdaRuntime == null
            || paginaIzquierdaRuntime.rect.width <= 0f
            || paginaIzquierdaRuntime.rect.height <= 0f)
        {
            return;
        }

        InvalidarCachePaginasLibro();
        RecalcularPaginasLibroSiHaceFalta();

        if (txtLog != null && !txtLog.enabled && paginasLibro.Count > 0)
        {
            int pagina = Mathf.Clamp(GetPaginaVisible(), 1, paginasLibro.Count);
            RenderizarPaginaLibro(pagina);
        }

        reacomodosLibroPendientes--;
    }

    private void InvalidarCachePaginasLibro()
    {
        cacheCantidadEntradasLibro = -1;
        cacheTamanoRaizLibro = new Vector2(float.NaN, float.NaN);
    }

    private bool TryPrepararLibro()
    {
        if (!usarLayoutLibro)
        {
            return false;
        }

        if (raizLibro == null && txtLog != null)
        {
            raizLibro = txtLog.transform.parent as RectTransform;
        }

        if (raizLibro == null)
        {
            return false;
        }

        paginaIzquierdaRuntime = AsegurarContenedorPagina(
            paginaIzquierdaRuntime,
            "PaginaIzquierdaRuntime",
            paginaIzquierdaAnchorMin,
            paginaIzquierdaAnchorMax);

        paginaDerechaRuntime = AsegurarContenedorPagina(
            paginaDerechaRuntime,
            "PaginaDerechaRuntime",
            paginaDerechaAnchorMin,
            paginaDerechaAnchorMax);

        if (paginaIzquierdaRuntime == null || paginaDerechaRuntime == null)
        {
            return false;
        }

        renglonesIzquierdaRuntime = AsegurarContenedorInterno(paginaIzquierdaRuntime, renglonesIzquierdaRuntime, "RenglonesRuntime");
        renglonesDerechaRuntime = AsegurarContenedorInterno(paginaDerechaRuntime, renglonesDerechaRuntime, "RenglonesRuntime");

        if (medidorEntradaLibro == null)
        {
            medidorEntradaLibro = CrearTextoLibroRuntime("MedidorEntradaLibro", raizLibro);
            if (medidorEntradaLibro == null)
            {
                return false;
            }

            medidorEntradaLibro.color = Color.clear;
            medidorEntradaLibro.text = string.Empty;
            medidorEntradaLibro.rectTransform.anchorMin = new Vector2(0f, 0f);
            medidorEntradaLibro.rectTransform.anchorMax = new Vector2(0f, 0f);
            medidorEntradaLibro.rectTransform.pivot = new Vector2(0f, 0f);
            medidorEntradaLibro.rectTransform.anchoredPosition = new Vector2(-10000f, -10000f);
            medidorEntradaLibro.rectTransform.sizeDelta = new Vector2(32f, 32f);
        }

        if (indicadorPaginaLibro == null && txtPaginaActual != null)
        {
            indicadorPaginaLibro = txtPaginaActual;
            indicadorPaginaLibro.gameObject.SetActive(false);
        }

        if (indicadorPaginaLibro == null)
        {
            indicadorPaginaLibro = CrearTextoLibroRuntime("IndicadorPaginaLibro", raizLibro);
            if (indicadorPaginaLibro != null)
            {
                RectTransform rect = indicadorPaginaLibro.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, 96f);
                rect.sizeDelta = new Vector2(220f, 32f);
                indicadorPaginaLibro.fontSize = fontSizeIndicadorPagina;
                indicadorPaginaLibro.fontStyle = FontStyles.Italic;
                indicadorPaginaLibro.horizontalAlignment = HorizontalAlignmentOptions.Center;
                indicadorPaginaLibro.verticalAlignment = VerticalAlignmentOptions.Middle;
                indicadorPaginaLibro.color = colorTextoLibro;
                indicadorPaginaLibro.gameObject.SetActive(false);
            }
        }

        return true;
    }

    private RectTransform AsegurarContenedorPagina(
        RectTransform contenedorActual,
        string nombre,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        if (contenedorActual != null)
        {
            return contenedorActual;
        }

        Transform existente = raizLibro.Find(nombre);
        RectTransform rect = existente as RectTransform;
        if (rect == null)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform));
            rect = go.GetComponent<RectTransform>();
            rect.SetParent(raizLibro, false);
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = Vector2.zero;
        return rect;
    }

    private RectTransform AsegurarContenedorInterno(RectTransform pagina, RectTransform contenedorActual, string nombre)
    {
        if (pagina == null)
        {
            return null;
        }

        if (contenedorActual != null)
        {
            contenedorActual.SetParent(pagina, false);
            contenedorActual.SetSiblingIndex(0);
            return contenedorActual;
        }

        Transform existente = pagina.Find(nombre);
        RectTransform rect = existente as RectTransform;
        if (rect == null)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform));
            rect = go.GetComponent<RectTransform>();
            rect.SetParent(pagina, false);
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.SetSiblingIndex(0);
        return rect;
    }

    private TextMeshProUGUI CrearTextoLibroRuntime(string nombre, RectTransform padre)
    {
        if (padre == null)
        {
            return null;
        }

        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(padre, false);
        rect.localScale = Vector3.one;

        TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
        ConfigurarTextoLibro(texto);
        return texto;
    }

    private void ConfigurarTextoLibro(TextMeshProUGUI texto)
    {
        if (texto == null)
        {
            return;
        }

        TextMeshProUGUI fuente = plantillaEntradaLibro != null ? plantillaEntradaLibro : txtLog;
        if (fuente != null)
        {
            texto.font = fuente.font;
            texto.fontSharedMaterial = fuente.fontSharedMaterial;
            texto.enableKerning = fuente.enableKerning;
            texto.richText = fuente.richText;
        }

        texto.fontSize = fontSizeLibro > 0f
            ? fontSizeLibro
            : (fuente != null ? fuente.fontSize * 0.55f : 22f);
        texto.fontStyle = fuente != null ? fuente.fontStyle : FontStyles.Italic;
        texto.color = colorTextoLibro;
        texto.raycastTarget = false;
        texto.enableAutoSizing = false;
        texto.horizontalAlignment = HorizontalAlignmentOptions.Left;
        texto.verticalAlignment = VerticalAlignmentOptions.Top;
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.overflowMode = TextOverflowModes.Overflow;
        texto.characterSpacing = 0f;
        texto.lineSpacing = 0f;
        texto.paragraphSpacing = 6f;
        texto.margin = Vector4.zero;
        texto.spriteAsset = null;
    }

    private void ActualizarRenglonesLibro()
    {
        if (!mostrarRenglonesLibro || paginaIzquierdaRuntime == null || paginaDerechaRuntime == null || medidorEntradaLibro == null)
        {
            OcultarRenglonesLibro(renglonesPaginaIzquierda);
            OcultarRenglonesLibro(renglonesPaginaDerecha);
            return;
        }

        ActualizarRenglonesPagina(paginaIzquierdaRuntime, renglonesIzquierdaRuntime, renglonesPaginaIzquierda);
        ActualizarRenglonesPagina(paginaDerechaRuntime, renglonesDerechaRuntime, renglonesPaginaDerecha);
    }

    private void ActualizarRenglonesPagina(RectTransform pagina, RectTransform contenedorRenglones, List<Image> renglones)
    {
        if (pagina == null || contenedorRenglones == null)
        {
            OcultarRenglonesLibro(renglones);
            return;
        }

        float anchoPagina = pagina.rect.width;
        float altoPagina = pagina.rect.height;
        if (anchoPagina <= 0f || altoPagina <= 0f)
        {
            OcultarRenglonesLibro(renglones);
            return;
        }

        MedicionEntradaLibro medicionMuestra = MedirEntradaLibro("Ag", anchoPagina);
        float pasoRenglon = Mathf.Max(1f, medicionMuestra.AlturaAvance);
        float primerRenglonY = Mathf.Max(0f, medicionMuestra.AlturaVisual);
        int cantidadRenglones = Mathf.Max(0, Mathf.FloorToInt((altoPagina - primerRenglonY) / pasoRenglon) + 1);

        for (int i = 0; i < cantidadRenglones; i++)
        {
            Image renglon = ObtenerRenglonLibro(renglones, i, contenedorRenglones);
            RectTransform rect = renglon.rectTransform;
            rect.SetParent(contenedorRenglones, false);
            rect.SetSiblingIndex(i);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(margenHorizontalRenglonesLibro, 0f);
            rect.offsetMax = new Vector2(-margenHorizontalRenglonesLibro, 0f);
            rect.anchoredPosition = new Vector2(0f, -(primerRenglonY + (pasoRenglon * i)));
            rect.sizeDelta = new Vector2(0f, grosorRenglonesLibro);
            renglon.color = colorRenglonesLibro;
            renglon.gameObject.SetActive(true);
        }

        for (int i = cantidadRenglones; i < renglones.Count; i++)
        {
            if (renglones[i] != null)
            {
                renglones[i].gameObject.SetActive(false);
            }
        }
    }

    private Image ObtenerRenglonLibro(List<Image> renglones, int indice, RectTransform padre)
    {
        while (renglones.Count <= indice)
        {
            GameObject go = new GameObject("RenglonLibroRuntime_" + renglones.Count, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(padre, false);
            rect.localScale = Vector3.one;

            Image imagen = go.GetComponent<Image>();
            imagen.raycastTarget = false;
            imagen.maskable = true;
            renglones.Add(imagen);
        }

        return renglones[indice];
    }

    private static void OcultarRenglonesLibro(List<Image> renglones)
    {
        for (int i = 0; i < renglones.Count; i++)
        {
            if (renglones[i] != null)
            {
                renglones[i].gameObject.SetActive(false);
            }
        }
    }

    private void RecalcularPaginasLibroSiHaceFalta()
    {
        if (raizLibro == null)
        {
            paginasLibro.Clear();
            return;
        }

        Canvas.ForceUpdateCanvases();

        Vector2 tamanoRaiz = raizLibro.rect.size;
        if (cacheCantidadEntradasLibro == CantidadEntradasCampania
            && cacheTamanoRaizLibro == tamanoRaiz
            && paginasLibro.Count > 0)
        {
            return;
        }

        paginasLibro.Clear();
        cacheCantidadEntradasLibro = CantidadEntradasCampania;
        cacheTamanoRaizLibro = tamanoRaiz;

        if (CantidadEntradasCampania <= 0)
        {
            return;
        }

        float anchoPagina = paginaIzquierdaRuntime.rect.width;
        float altoPagina = paginaIzquierdaRuntime.rect.height;
        if (anchoPagina <= 0f || altoPagina <= 0f || medidorEntradaLibro == null)
        {
            return;
        }

        int indiceEntrada = 0;
        while (indiceEntrada < CantidadEntradasCampania)
        {
            PaginaLibro pagina = new PaginaLibro
            {
                InicioIzquierda = indiceEntrada
            };

            pagina.FinIzquierda = CalcularFinLadoPagina(indiceEntrada, anchoPagina, altoPagina);
            pagina.InicioDerecha = pagina.FinIzquierda;
            pagina.FinDerecha = CalcularFinLadoPagina(pagina.InicioDerecha, anchoPagina, altoPagina);
            paginasLibro.Add(pagina);

            int siguienteIndice = pagina.FinDerecha;
            if (siguienteIndice <= indiceEntrada)
            {
                siguienteIndice = indiceEntrada + 1;
            }

            indiceEntrada = siguienteIndice;
        }
    }

    private int CalcularFinLadoPagina(int indiceInicial, float anchoPagina, float altoPagina)
    {
        if (indiceInicial >= CantidadEntradasCampania)
        {
            return indiceInicial;
        }

        float alturaUsada = 0f;
        int indice = indiceInicial;
        while (indice < CantidadEntradasCampania)
        {
            string texto = ObtenerEntradaCampaniaFormateada(indice, false);
            string textoLibro = FormatearTextoEntradaLibro(texto);
            MedicionEntradaLibro medicionEntrada = MedirEntradaLibro(textoLibro, anchoPagina);
            float alturaConEspacio = medicionEntrada.AlturaAvance;
            if (indice > indiceInicial)
            {
                alturaConEspacio += espaciadoEntradasLibro;
            }

            if (indice > indiceInicial && alturaUsada + alturaConEspacio > altoPagina)
            {
                break;
            }

            alturaUsada += alturaConEspacio;
            indice++;
        }

        return indice > indiceInicial ? indice : Mathf.Min(indiceInicial + 1, CantidadEntradasCampania);
    }

    private MedicionEntradaLibro MedirEntradaLibro(string texto, float anchoPagina)
    {
        if (medidorEntradaLibro == null)
        {
            return new MedicionEntradaLibro
            {
                AlturaVisual = 1f,
                AlturaAvance = 1f
            };
        }

        string textoRender = AplicarFiltroTextoNegroBitacora(texto);
        RectTransform rect = medidorEntradaLibro.rectTransform;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, anchoPagina);
        medidorEntradaLibro.text = textoRender;
        medidorEntradaLibro.ForceMeshUpdate();

        float alturaVisual = Mathf.Max(1f, medidorEntradaLibro.preferredHeight);
        float alturaAvance = alturaVisual;
        TMP_TextInfo textInfo = medidorEntradaLibro.textInfo;
        if (textInfo != null && textInfo.lineCount > 0)
        {
            TMP_LineInfo primeraLinea = textInfo.lineInfo[0];
            float pasoLinea = primeraLinea.lineHeight;
            if (pasoLinea <= 0f && textInfo.lineCount > 1)
            {
                pasoLinea = Mathf.Abs(textInfo.lineInfo[1].ascender - primeraLinea.ascender);
            }

            if (pasoLinea <= 0f)
            {
                pasoLinea = Mathf.Max(1f, primeraLinea.ascender - primeraLinea.descender);
            }
        }

        return new MedicionEntradaLibro
        {
            AlturaVisual = alturaVisual,
            AlturaAvance = Mathf.Max(1f, alturaAvance)
        };
    }

    private static string FormatearTextoEntradaLibro(string texto)
    {
        if (!EsEncabezadoDiaLibro(texto))
        {
            return texto;
        }
       
        return "<b><size=110%>" + texto + "</size></b>";
    }

    private static bool EsEncabezadoDiaLibro(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        string recortado = texto.Trim();
        return CoincideEncabezadoDia(recortado, "Día")
            || CoincideEncabezadoDia(recortado, "Day")
            || CoincideEncabezadoDia(recortado, "Dia");
    }

    private static bool CoincideEncabezadoDia(string texto, string etiqueta)
    {
        if (!texto.StartsWith(etiqueta + " ", System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return int.TryParse(texto.Substring(etiqueta.Length).Trim(), out _);
    }

    private void RenderizarPaginaLibro(int pagina)
    {
        if (paginaIzquierdaRuntime == null || paginaDerechaRuntime == null || paginasLibro.Count == 0)
        {
            return;
        }

        PaginaLibro paginaLibro = paginasLibro[Mathf.Clamp(pagina - 1, 0, paginasLibro.Count - 1)];
        int indiceVista = 0;
        indiceVista = RenderizarEntradasEnPagina(
            paginaLibro.InicioIzquierda,
            paginaLibro.FinIzquierda,
            paginaIzquierdaRuntime,
            indiceVista);
        indiceVista = RenderizarEntradasEnPagina(
            paginaLibro.InicioDerecha,
            paginaLibro.FinDerecha,
            paginaDerechaRuntime,
            indiceVista);

        for (int i = indiceVista; i < entradasLibro.Count; i++)
        {
            if (entradasLibro[i] != null)
            {
                entradasLibro[i].gameObject.SetActive(false);
            }
        }

        if (indicadorPaginaLibro != null)
        {
            indicadorPaginaLibro.gameObject.SetActive(paginasLibro.Count > 1);
            indicadorPaginaLibro.text = pagina + " / " + paginasLibro.Count;
        }

        ActualizarBotonesNavegacion(pagina, paginasLibro.Count);
    }

    private int RenderizarEntradasEnPagina(int indiceInicial, int indiceFinal, RectTransform pagina, int indiceVista)
    {
        float anchoPagina = pagina.rect.width;
        float cursorY = 0f;
        for (int i = indiceInicial; i < indiceFinal; i++)
        {
            TextMeshProUGUI entrada = ObtenerEntradaLibroView(indiceVista, pagina);
            string texto = ObtenerEntradaCampaniaFormateada(i, false);
            string textoLibro = FormatearTextoEntradaLibro(texto);
            MedicionEntradaLibro medicionEntrada = MedirEntradaLibro(textoLibro, anchoPagina);

            RectTransform rect = entrada.rectTransform;
            rect.SetParent(pagina, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(0f, -cursorY);
            rect.sizeDelta = new Vector2(0f, medicionEntrada.AlturaVisual);

            entrada.text = AplicarFiltroTextoNegroBitacora(textoLibro);
            entrada.gameObject.SetActive(true);

            cursorY += medicionEntrada.AlturaAvance + espaciadoEntradasLibro;
            indiceVista++;
        }

        return indiceVista;
    }

    private TextMeshProUGUI ObtenerEntradaLibroView(int indice, RectTransform padre)
    {
        while (entradasLibro.Count <= indice)
        {
            TextMeshProUGUI nuevaEntrada = CrearTextoLibroRuntime(
                "EntradaLibroRuntime_" + entradasLibro.Count,
                padre);
            entradasLibro.Add(nuevaEntrada);
        }

        return entradasLibro[indice];
    }

    private void ActualizarBotonesNavegacion(int paginaActual, int totalPaginas)
    {
        bool puedeRetroceder = totalPaginas > 1 && paginaActual > 1;
        bool puedeAvanzar = totalPaginas > 1 && paginaActual < totalPaginas;

        if (botonPaginaAnterior != null)
        {
            botonPaginaAnterior.SetActive(puedeRetroceder);
        }

        if (botonPaginaSiguiente != null)
        {
            botonPaginaSiguiente.SetActive(puedeAvanzar);
        }
    }
}
