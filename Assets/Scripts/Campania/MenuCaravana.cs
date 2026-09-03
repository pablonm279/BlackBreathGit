using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MenuCaravana : MonoBehaviour
{
    private const int MaxTierMejoraCaravana = 5;
    private const int ProbabilidadBasePerderSequitoPorDerrota = 50;
    private const string ColorTextoFactoresExploracion = "#C8C8C8";
    private const string ColorIncrementoMejora = "#DFEA02";
    private enum TipoMejoraCaravana
    {
        Ninguna,
        Antorchas,
        Alforjas,
        Tiendas,
        Catalejos,
        Almacen,
        Defensas
    }

    private TipoMejoraCaravana mejoraCaravanaHover;
    private bool coloresCostoInicializados;
    private Color colorCostoAntorchasNormal;
    private Color colorCostoAlforjasNormal;
    private Color colorCostoTiendasNormal;
    private Color colorCostoCatalejosNormal;
    private Color colorCostoAlmacenNormal;
    private Color colorCostoDefensasNormal;
   
    [SerializeField] GameObject MenuMejoras;
    [SerializeField] GameObject MenuSequitos;
    [SerializeField] GameObject MenuPersonajes;
    [SerializeField] GameObject MenuBitacora;
    private bool menuPersonajesAbiertoPorHover;
    private Personaje personajeMenuPersonajesHover;

    [Header("Exploradores")]
    [SerializeField] GameObject panelResultadoExploradores;
    [SerializeField] TextMeshProUGUI txtTituloResultadoExploradores;
    [SerializeField] TextMeshProUGUI txtDescripcionResultadoExploradores;
    [SerializeField] TextMeshProUGUI txtStatsViaje;


    [Header("Botón antorchas")]
    [SerializeField] GameObject antorchaBoton;
    [SerializeField, Min(0.05f)] float duracionAnimacionBotonAntorcha = 0.45f;
    [SerializeField, Min(0f)] float margenOcultoBotonAntorcha = 40f;
    [SerializeField, Min(0f)] float delayEntreAntorchas = 0.12f;
    private RectTransform rectTransformBotonAntorcha;
    private CanvasGroup canvasGroupBotonAntorcha;
    private Image imagenBotonAntorcha;
    private Sprite spriteAntorchaApagada;
    private Sprite spriteAntorchaEncendida;
    private Vector2 posicionVisibleBotonAntorcha;
    private Vector2 posicionOcultaBotonAntorcha;
    private bool botonAntorchaInicializado;


    public bool TieneMenuAbierto =>
        (MenuMejoras != null && MenuMejoras.activeInHierarchy)
        || (MenuSequitos != null && MenuSequitos.activeInHierarchy)
        || (MenuPersonajes != null && MenuPersonajes.activeInHierarchy)
        || (MenuBitacora != null && MenuBitacora.activeInHierarchy)
        || (panelResultadoExploradores != null && panelResultadoExploradores.activeInHierarchy)
        || (CampaignManager.Instance != null && CampaignManager.Instance.LogEstaAbierto());

     //Antorchas de Caravana
    [SerializeField] TextMeshProUGUI txtTierMejoraAntorchas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAntorchas;
    [SerializeField] GameObject AntorchasDiamantes;

    [SerializeField] GameObject btMejoraAntorchas;
    [SerializeField] TextMeshProUGUI descAntorchas;
    int costoMejorarAntorchas = 0;

    //Alforjas
    [SerializeField] TextMeshProUGUI txtTierMejoraAlforjas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAlforjas;
    [SerializeField] GameObject AlforjasDiamantes;

    [SerializeField] TextMeshProUGUI descAlforjas;
    [SerializeField] GameObject btMejoraAlforjas;
    int costoMejorarAlforjas = 0;

    //Tiendas
    [SerializeField] TextMeshProUGUI txtTierMejoraTiendas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraTiendas;
    [SerializeField] GameObject TiendasDiamantes;

    [SerializeField] TextMeshProUGUI descTiendas;

    [SerializeField] GameObject btMejoraTiendas;
    int costoMejorarTiendas = 0;

    //Catalejos
    [SerializeField] TextMeshProUGUI txtTierMejoraCatalejos;
    [SerializeField] TextMeshProUGUI txtCostoMejoraCatalejos;
    [SerializeField] TextMeshProUGUI descCatalejos;
    [SerializeField] GameObject CatalejosDiamantes;

    [SerializeField] GameObject btMejoraCatalejos;
    int costoMejorarCatalejos = 0;

    //Almacen
    [SerializeField] TextMeshProUGUI txtTierMejoraAlmacen;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAlmacen;
    [SerializeField] TextMeshProUGUI descAlmacen;
    [SerializeField] GameObject AlmacenDiamantes;

    [SerializeField] GameObject btMejoraAlmacen;
    [SerializeField] TextMeshProUGUI txtExplicacionVistaLejana;
    [SerializeField] TextMeshProUGUI txtExplicacionExploracionPasiva;
    [SerializeField] TextMeshProUGUI txtExploradores;
    int costoMejorarAlmacen = 0;

     //Defensas
    [SerializeField] TextMeshProUGUI txtTierMejoraDefensas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraDefensas;
    [SerializeField] TextMeshProUGUI descDefensas;
    [SerializeField] GameObject DefensasDiamantes;
    [SerializeField] GameObject btMejoraDefensas;
    int costoMejorarDefensas = 0;

     [SerializeField] GameObject contenedorSequitos;

    public bool SeApretoESC()
    {
        if (BloqueaCierreMenuPersonajesTutorialNuevo() && MenuPersonajesEstaAbierto())
        {
            return true;
        }

        bool habiaalgoabierto = TieneMenuAbierto;

        if (MenuMejoras != null)
        {
            MenuMejoras.SetActive(false);
        }

        if (MenuSequitos != null)
        {
            MenuSequitos.SetActive(false);
        }

        if (MenuPersonajes != null)
        {
            MenuPersonajes.SetActive(false);
        }
        menuPersonajesAbiertoPorHover = false;
        personajeMenuPersonajesHover = null;

        if (MenuBitacora != null)
        {
            MenuBitacora.SetActive(false);
        }

        if (CampaignManager.Instance != null)
        {
            CampaignManager.Instance.ActivarLog(0);
        }

        if (panelResultadoExploradores != null)
        {
            panelResultadoExploradores.SetActive(false);
        }

        return habiaalgoabierto;
    }

    public void CerrarMenusExclusivos()
    {
        if (BloqueaCierreMenuPersonajesTutorialNuevo() && MenuPersonajesEstaAbierto())
        {
            return;
        }

        bool menuMejorasAbierto = MenuMejoras != null && MenuMejoras.activeInHierarchy;
        bool menuSequitosAbierto = MenuSequitos != null && MenuSequitos.activeInHierarchy;
        bool menuPersonajesAbierto = MenuPersonajes != null && MenuPersonajes.activeInHierarchy;

        MenuMejoras.SetActive(false);
        MenuSequitos.SetActive(false);
        MenuPersonajes.SetActive(false);
        MenuBitacora.SetActive(false);
        menuPersonajesAbiertoPorHover = false;
        personajeMenuPersonajesHover = null;

        if (menuMejorasAbierto)
        {
            EmitirCierreMenuMejorasTutorial(false);
        }

        if (menuSequitosAbierto)
        {
            EmitirCierreMenuSequitosTutorial(false);
        }

        if (menuPersonajesAbierto)
        {
            // Los botones de cierre son una alternativa válida a la tecla C durante el tutorial.
            EmitirCierreMenuPersonajesTutorial(true);
        }
    }

    public void MostrarResultadoExploradores(CampaignManager.ResultadoExploradoresCampania resultado)
    {
        if (resultado == null)
        {
            return;
        }

        if (panelResultadoExploradores == null)
        {
            return;
        }

        if (txtTituloResultadoExploradores != null)
        {
            txtTituloResultadoExploradores.text = resultado.titulo;
        }

        if (txtDescripcionResultadoExploradores != null)
        {
            string texto = resultado.descripcion + "\n";
            texto += TRADU.i.Traducir("Tirada: ") + resultado.tirada + " / " + resultado.chance + "%\n";

            if (resultado.materialesGanados != 0)
              texto += "+" + resultado.materialesGanados + " " + TRADU.i.Traducir("Materiales") + "\n";
            if (resultado.oroGanado != 0)
              texto += "+" + resultado.oroGanado + " " + TRADU.i.Traducir("Oro") + "\n";
            if (resultado.esperanzaCambio != 0)
              texto += (resultado.esperanzaCambio > 0 ? "+" : "") + resultado.esperanzaCambio + " " + TRADU.i.Traducir("Esperanza") + "\n";
            if (resultado.civilesMuertos > 0)
              texto += "-" + resultado.civilesMuertos + " " + TRADU.i.Traducir("Civiles") + "\n";
            if (!string.IsNullOrEmpty(resultado.faccionReveladaNombre))
              texto += TRADU.i.Traducir("Enemigos: ") + resultado.faccionReveladaNombre + "\n";

            txtDescripcionResultadoExploradores.text = texto.TrimEnd();
        }

        panelResultadoExploradores.SetActive(true);
    }

    void Awake()
    {
        InicializarColoresCostoSiHaceFalta();
        InicializarBotonAntorcha();
        InicializarHoverMejoras();
    }

    void Update()
    {
        ActualizarBotonAntorcha();
    }

    private void InicializarBotonAntorcha()
    {
        if (antorchaBoton == null)
        {
            return;
        }

        rectTransformBotonAntorcha = antorchaBoton.GetComponent<RectTransform>();
        imagenBotonAntorcha = antorchaBoton.GetComponent<Image>();
        canvasGroupBotonAntorcha = antorchaBoton.GetComponent<CanvasGroup>();
        if (canvasGroupBotonAntorcha == null)
        {
            canvasGroupBotonAntorcha = antorchaBoton.AddComponent<CanvasGroup>();
        }

        spriteAntorchaApagada = Resources.Load<Sprite>("uiANTORCHA");
        spriteAntorchaEncendida = Resources.Load<Sprite>("uiANTORCHA2");

        if (rectTransformBotonAntorcha != null)
        {
            posicionVisibleBotonAntorcha = rectTransformBotonAntorcha.anchoredPosition;
            float ancho = Mathf.Max(rectTransformBotonAntorcha.rect.width, rectTransformBotonAntorcha.sizeDelta.x);
            posicionOcultaBotonAntorcha = posicionVisibleBotonAntorcha + Vector2.right * (ancho + margenOcultoBotonAntorcha);
            rectTransformBotonAntorcha.anchoredPosition = posicionOcultaBotonAntorcha;
        }

        canvasGroupBotonAntorcha.alpha = 0f;
        canvasGroupBotonAntorcha.interactable = false;
        canvasGroupBotonAntorcha.blocksRaycasts = false;
        antorchaBoton.SetActive(false);
        botonAntorchaInicializado = true;
    }

    private void ActualizarBotonAntorcha()
    {
        if (!botonAntorchaInicializado || rectTransformBotonAntorcha == null)
        {
            return;
        }

        CampaignManager campaignManager = CampaignManager.Instance;
        bool menuMejorasAbierto = MenuMejoras != null && MenuMejoras.activeInHierarchy;
        bool mostrarDesdeMejoras = menuMejorasAbierto
            && campaignManager != null
            && !campaignManager.DebeUsarConfiguracionTutorial();
        bool debeMostrarse = campaignManager != null
            && (campaignManager.EsNocheActual() || mostrarDesdeMejoras);
        if (debeMostrarse && !antorchaBoton.activeSelf)
        {
            antorchaBoton.SetActive(true);
        }

        Vector2 posicionObjetivo = debeMostrarse ? posicionVisibleBotonAntorcha : posicionOcultaBotonAntorcha;
        float alphaObjetivo = debeMostrarse ? 1f : 0f;
        float delta = Time.unscaledDeltaTime / Mathf.Max(0.05f, duracionAnimacionBotonAntorcha);
        float distanciaAnimacion = Vector2.Distance(posicionVisibleBotonAntorcha, posicionOcultaBotonAntorcha);

        rectTransformBotonAntorcha.anchoredPosition = Vector2.MoveTowards(
            rectTransformBotonAntorcha.anchoredPosition,
            posicionObjetivo,
            distanciaAnimacion * delta);
        canvasGroupBotonAntorcha.alpha = Mathf.MoveTowards(canvasGroupBotonAntorcha.alpha, alphaObjetivo, delta);
        canvasGroupBotonAntorcha.interactable = debeMostrarse && campaignManager.PuedeCambiarAntorchas();
        canvasGroupBotonAntorcha.blocksRaycasts = debeMostrarse;

        ActualizarImagenBotonAntorcha(campaignManager);

        if (!debeMostrarse && canvasGroupBotonAntorcha.alpha <= 0f)
        {
            antorchaBoton.SetActive(false);
        }
    }

    private void ActualizarImagenBotonAntorcha(CampaignManager campaignManager)
    {
        if (imagenBotonAntorcha == null || campaignManager == null)
        {
            return;
        }

        Sprite spriteObjetivo = campaignManager.AntorchasEncendidas
            ? spriteAntorchaEncendida
            : spriteAntorchaApagada;
        if (spriteObjetivo != null && imagenBotonAntorcha.sprite != spriteObjetivo)
        {
            imagenBotonAntorcha.sprite = spriteObjetivo;
        }
    }

    public void ClickAntorcha()
    {
        CampaignManager campaignManager = CampaignManager.Instance;
        if (campaignManager == null)
        {
            return;
        }

        bool encender = !campaignManager.AntorchasEncendidas;
        if (!campaignManager.SetAntorchasEncendidas(encender))
        {
            return;
        }

        ActualizarImagenBotonAntorcha(campaignManager);

        if (campaignManager.scMapaManager == null || campaignManager.scMapaManager.goCaravana == null)
        {
            return;
        }

        CaravanTorchLight[] antorchas = campaignManager.scMapaManager.goCaravana
            .GetComponentsInChildren<CaravanTorchLight>(true);
        for (int i = 0; i < antorchas.Length; i++)
        {
            antorchas[i].ProgramarEstado(encender, i * delayEntreAntorchas);
        }
    }

    private void InicializarColoresCostoSiHaceFalta()
    {
        if (coloresCostoInicializados)
        {
            return;
        }

        colorCostoAntorchasNormal = txtCostoMejoraAntorchas != null ? txtCostoMejoraAntorchas.color : Color.white;
        colorCostoAlforjasNormal = txtCostoMejoraAlforjas != null ? txtCostoMejoraAlforjas.color : Color.white;
        colorCostoTiendasNormal = txtCostoMejoraTiendas != null ? txtCostoMejoraTiendas.color : Color.white;
        colorCostoCatalejosNormal = txtCostoMejoraCatalejos != null ? txtCostoMejoraCatalejos.color : Color.white;
        colorCostoAlmacenNormal = txtCostoMejoraAlmacen != null ? txtCostoMejoraAlmacen.color : Color.white;
        colorCostoDefensasNormal = txtCostoMejoraDefensas != null ? txtCostoMejoraDefensas.color : Color.white;
        coloresCostoInicializados = true;
    }

    private void ActualizarColorCosto(TextMeshProUGUI txtCosto, int costo, Color colorNormal)
    {
        if (txtCosto == null)
        {
            return;
        }

        bool alcanza = costo <= CampaignManager.Instance.GetMaterialesActuales();
        txtCosto.color = alcanza ? colorNormal : Color.red;
    }

    private static string ObtenerTierRomano(int tier)
    {
        return tier switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            _ => tier.ToString()
        };
    }

    private void ActualizarDiamantesMejora(GameObject diamantes, int tier)
    {
        if (diamantes == null)
        {
            return;
        }

        int diamantesActivos = Mathf.Max(0, tier - 1);
        for (int i = 0; i < diamantes.transform.childCount; i++)
        {
            diamantes.transform.GetChild(i).gameObject.SetActive(i < diamantesActivos);
        }
    }

    private void InicializarHoverMejoras()
    {
        RegistrarHoverMejora(btMejoraAntorchas, TipoMejoraCaravana.Antorchas);
        RegistrarHoverMejora(btMejoraAlforjas, TipoMejoraCaravana.Alforjas);
        RegistrarHoverMejora(btMejoraTiendas, TipoMejoraCaravana.Tiendas);
        RegistrarHoverMejora(btMejoraCatalejos, TipoMejoraCaravana.Catalejos);
        RegistrarHoverMejora(btMejoraAlmacen, TipoMejoraCaravana.Almacen);
        RegistrarHoverMejora(btMejoraDefensas, TipoMejoraCaravana.Defensas);
    }

    private void RegistrarHoverMejora(GameObject boton, TipoMejoraCaravana tipoMejora)
    {
        if (boton == null)
        {
            return;
        }

        EventTrigger trigger = boton.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = boton.AddComponent<EventTrigger>();
        }

        if (trigger.triggers == null)
        {
            trigger.triggers = new List<EventTrigger.Entry>();
        }

        EventTrigger.Entry entrada = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entrada.callback.AddListener(_ => MostrarPreviewMejora(tipoMejora));
        trigger.triggers.Add(entrada);

        EventTrigger.Entry salida = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        salida.callback.AddListener(_ => OcultarPreviewMejora(tipoMejora));
        trigger.triggers.Add(salida);
    }

    private void MostrarPreviewMejora(TipoMejoraCaravana tipoMejora)
    {
        mejoraCaravanaHover = tipoMejora;
        ActualizarDescripcionesMejoras();
    }

    private void OcultarPreviewMejora(TipoMejoraCaravana tipoMejora)
    {
        if (mejoraCaravanaHover != tipoMejora)
        {
            return;
        }

        mejoraCaravanaHover = TipoMejoraCaravana.Ninguna;
        ActualizarDescripcionesMejoras();
    }

    private int ObtenerIdiomaActual()
    {
        if (TRADU.i != null)
        {
            return TRADU.i.nIdioma;
        }

        return PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaIngles);
    }

    private int ObtenerReduccionPerdidaSequitoDefensas(int tier)
    {
        return 5 + (3 * tier);
    }

    private int ObtenerChanceFinalPerderSequitoDefensas(int tier)
    {
        return Mathf.Clamp(ProbabilidadBasePerderSequitoPorDerrota - ObtenerReduccionPerdidaSequitoDefensas(tier), 0, 100);
    }

    private int ObtenerBonusCargaPorBueyAlforjas(int tier)
    {
        return Mathf.Max(0, tier - 1);
    }

    private int ObtenerBonusEsperanzaTiendas(int tier)
    {
        return Mathf.Max(0, (tier - 1) * 2);
    }

    private int ObtenerCapacidadPersonajesTiendas(int tier)
    {
        return 4 + Mathf.Max(0, tier - 1);
    }

    private int ObtenerBonusVisionCatalejos(int tier)
    {
        return Mathf.Max(0, tier - 1);
    }

    private int ObtenerReduccionConsumoAlmacen(int tier)
    {
        return tier * 3;
    }

    private int ObtenerBonusCargaAlmacen(int tier)
    {
        return tier * 5;
    }

    private string ObtenerIncrementoPreview(bool mostrar, int tier, int valorActual, int valorSiguiente, string sufijo = "")
    {
        if (!mostrar || tier >= MaxTierMejoraCaravana || valorActual == valorSiguiente)
        {
            return string.Empty;
        }

        return " <color=" + ColorIncrementoMejora + ">(" + FormatearModificadorEntero(valorSiguiente - valorActual) + sufijo + ")</color>";
    }

    private int CalcularCostoEscaladoDesdeSistemaViejo(int tierActual, int costoTier1Viejo, int costoTier3Viejo)
    {
        if (MaxTierMejoraCaravana <= 1)
        {
            return costoTier1Viejo;
        }

        float progreso = Mathf.Clamp01((tierActual - 1f) / (MaxTierMejoraCaravana - 1f));
        int costoBase = Mathf.RoundToInt(Mathf.Lerp(costoTier1Viejo, costoTier3Viejo, progreso));
        int tierObjetivo = tierActual + 1;
        float aumentoPorTier = tierObjetivo >= 3 ? 0.10f + ((tierObjetivo - 3) * 0.02f) : 0f;
        return Mathf.RoundToInt(costoBase * (1f + aumentoPorTier));
    }

    private void ActualizarDescripcionMejora(TextMeshProUGUI txtDescripcion, string textoEs, string textoEn, string textoPt)
    {
        if (txtDescripcion == null)
        {
            return;
        }

        txtDescripcion.text = ObtenerIdiomaActual() switch
        {
            TRADU.IdiomaPortugues => textoPt,
            TRADU.IdiomaIngles => textoEn,
            _ => textoEs
        };
    }

    private void ActualizarDescripcionesMejoras()
    {
        int tierAntorchas = CampaignManager.Instance.mejoraCaravanaAntorchas;
        int tierAlforjas = CampaignManager.Instance.mejoraCaravanaAlforjas;
        int tierTiendas = CampaignManager.Instance.mejoraCaravanaTiendas;
        int tierCatalejos = CampaignManager.Instance.mejoraCaravanaCatalejos;
        int tierAlmacen = CampaignManager.Instance.mejoraCaravanaAlmacen;
        int tierDefensas = CampaignManager.Instance.mejoraCaravanaDefensas;

        ActualizarDescripcionMejora(
            descAntorchas,
            ObtenerDescripcionAntorchasEs(tierAntorchas, mejoraCaravanaHover == TipoMejoraCaravana.Antorchas),
            ObtenerDescripcionAntorchasEn(tierAntorchas, mejoraCaravanaHover == TipoMejoraCaravana.Antorchas),
            ObtenerDescripcionAntorchasPt(tierAntorchas, mejoraCaravanaHover == TipoMejoraCaravana.Antorchas));

        ActualizarDescripcionMejora(
            descAlforjas,
            ObtenerDescripcionAlforjasEs(tierAlforjas, mejoraCaravanaHover == TipoMejoraCaravana.Alforjas),
            ObtenerDescripcionAlforjasEn(tierAlforjas, mejoraCaravanaHover == TipoMejoraCaravana.Alforjas),
            ObtenerDescripcionAlforjasPt(tierAlforjas, mejoraCaravanaHover == TipoMejoraCaravana.Alforjas));

        ActualizarDescripcionMejora(
            descTiendas,
            ObtenerDescripcionTiendasEs(tierTiendas, mejoraCaravanaHover == TipoMejoraCaravana.Tiendas),
            ObtenerDescripcionTiendasEn(tierTiendas, mejoraCaravanaHover == TipoMejoraCaravana.Tiendas),
            ObtenerDescripcionTiendasPt(tierTiendas, mejoraCaravanaHover == TipoMejoraCaravana.Tiendas));

        ActualizarDescripcionMejora(
            descCatalejos,
            ObtenerDescripcionCatalejosEs(tierCatalejos, mejoraCaravanaHover == TipoMejoraCaravana.Catalejos),
            ObtenerDescripcionCatalejosEn(tierCatalejos, mejoraCaravanaHover == TipoMejoraCaravana.Catalejos),
            ObtenerDescripcionCatalejosPt(tierCatalejos, mejoraCaravanaHover == TipoMejoraCaravana.Catalejos));

        ActualizarDescripcionMejora(
            descAlmacen,
            ObtenerDescripcionAlmacenEs(tierAlmacen, mejoraCaravanaHover == TipoMejoraCaravana.Almacen),
            ObtenerDescripcionAlmacenEn(tierAlmacen, mejoraCaravanaHover == TipoMejoraCaravana.Almacen),
            ObtenerDescripcionAlmacenPt(tierAlmacen, mejoraCaravanaHover == TipoMejoraCaravana.Almacen));

        ActualizarDescripcionMejora(
            descDefensas,
            ObtenerDescripcionDefensasEs(tierDefensas, mejoraCaravanaHover == TipoMejoraCaravana.Defensas),
            ObtenerDescripcionDefensasEn(tierDefensas, mejoraCaravanaHover == TipoMejoraCaravana.Defensas),
            ObtenerDescripcionDefensasPt(tierDefensas, mejoraCaravanaHover == TipoMejoraCaravana.Defensas));

        ActualizarDescripcionMejora(
            txtExplicacionVistaLejana,
            ObtenerExplicacionVistaLejanaEs(tierCatalejos),
            ObtenerExplicacionVistaLejanaEn(tierCatalejos),
            ObtenerExplicacionVistaLejanaPt(tierCatalejos));

        ActualizarDescripcionMejora(
            txtExplicacionExploracionPasiva,
            ObtenerExplicacionExploracionPasivaEs(tierCatalejos),
            ObtenerExplicacionExploracionPasivaEn(tierCatalejos),
            ObtenerExplicacionExploracionPasivaPt(tierCatalejos));
    }

    public void AbrirMenuMejoras()
    {
        AbrirMenuMejoras(false);
    }

    public void AbrirMenuMejorasDesdeHotkey()
    {
        AbrirMenuMejoras(true);
    }

    public bool MenuMejorasEstaAbierto()
    {
        return MenuMejoras != null && MenuMejoras.activeInHierarchy;
    }

    private void AbrirMenuMejoras(bool desdeHotkey)
    {
        TutorialDirector tutorialNuevo = TutorialDirector.Instance;
        bool tutorialNuevoActivo = tutorialNuevo != null && tutorialNuevo.IsRunning;
        if (tutorialNuevoActivo
            && tutorialNuevo.BlocksOptionalCampaignMenus
            && !MenuMejorasEstaAbierto()) { return; }
         TutorialEvents.Emit("ui.abrirmejoras", gameObject);
        if (desdeHotkey && MenuMejorasEstaAbierto())
        {
            MenuMejoras.SetActive(false);
            EmitirCierreMenuMejorasTutorial(true);
            return;
        }

        if (!tutorialNuevoActivo)
        {
            if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 15) { return; }
            if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 15)
            { CampaignManager.Instance.scTutorialManager.SiguientePaso(); }
        }

        bool abrir = !MenuMejoras.activeInHierarchy;
        if (abrir)
        {
            mejoraCaravanaHover = TipoMejoraCaravana.Ninguna;
        }
        ActualizarMejoras();
        CerrarMenusExclusivos();
        CampaignManager.Instance.ActivarLog(0);
        MenuMejoras.SetActive(abrir);
        if (!abrir)
        {
            EmitirCierreMenuMejorasTutorial(desdeHotkey);
            return;
        }

        if (abrir)
        {
            RuntimeAnalytics.TrackDesign("ui", "caravan", "open_upgrades");
        }

    }
    public void AbrirMenuSequitos()
    {
        AbrirMenuSequitos(false);
    }

    public void AbrirMenuSequitosDesdeHotkey()
    {
        AbrirMenuSequitos(true);
    }

    public bool MenuSequitosEstaAbierto()
    {
        return MenuSequitos != null && MenuSequitos.activeInHierarchy;
    }

    private void AbrirMenuSequitos(bool desdeHotkey)
    {
        TutorialDirector tutorialNuevo = TutorialDirector.Instance;
        bool tutorialNuevoActivo = tutorialNuevo != null && tutorialNuevo.IsRunning;
        if (tutorialNuevoActivo
            && tutorialNuevo.BlocksOptionalCampaignMenus
            && !MenuSequitosEstaAbierto()) { return; }
        TutorialEvents.Emit("ui.menusequitosab", gameObject);
        if (desdeHotkey && MenuSequitosEstaAbierto())
        {
            MenuSequitos.SetActive(false);
            EmitirCierreMenuSequitosTutorial(true);
            return;
        }

        if (!tutorialNuevoActivo)
        {
            if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 27) { return; }
             if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 27)
            {            CampaignManager.Instance.scTutorialManager.SiguientePaso();        }
             if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 29)
            {            CampaignManager.Instance.scTutorialManager.SiguientePaso();        }
        }

        bool abrir = !MenuSequitos.activeInHierarchy;
        CerrarMenusExclusivos();
        CampaignManager.Instance.ActivarLog(0);
        MenuSequitos.SetActive(abrir);
        if (!abrir)
        {
            EmitirCierreMenuSequitosTutorial(desdeHotkey);
            return;
        }

        if (abrir)
        {
            RuntimeAnalytics.TrackDesign("ui", "caravan", "open_followers");
        }
      
        
        
       
    }
    public void AbrirMenuPersonajes()
    {
        AbrirMenuPersonajes(null, true, false, false);
    }

    public void AbrirMenuPersonajesDesdeHotkey()
    {
        AbrirMenuPersonajes(null, true, true, false);
    }

    public void AbrirMenuPersonajesDesdeHotkey(Personaje personaje)
    {
        AbrirMenuPersonajes(personaje, false, true, false);
    }

    public bool MenuPersonajesEstaAbierto()
    {
        return MenuPersonajes != null && MenuPersonajes.activeInHierarchy;
    }
    public void AbrirBitácora()
    {
       bool abrir = MenuBitacora == null || !MenuBitacora.activeInHierarchy;
       if (abrir
           && TutorialDirector.Instance != null
           && TutorialDirector.Instance.BlocksOptionalCampaignMenus) { return; }
       ActualizarStatsViaje();
       CerrarMenusExclusivos();
       CampaignManager.Instance.ActivarLog(abrir ? 1 : 0);
    }

    private void ActualizarStatsViaje()
    {
        if (txtStatsViaje == null || CampaignManager.Instance == null)
        {
            return;
        }

        CampaignManager campaignManager = CampaignManager.Instance;
        StringBuilder sb = new StringBuilder(160);
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Tiempo viajado", "Travel time", "Tempo viajado"),
            campaignManager.FormatearDuracionHoras(campaignManager.ObtenerEstadisticaHorasViajadas())));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Batallas libradas", "Battles fought", "Batalhas travadas"),
            campaignManager.ObtenerEstadisticaBatallasLibradas()));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Personajes muertos", "Characters dead", "Personagens mortos"),
            campaignManager.ObtenerEstadisticaPersonajesMuertos()));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Enemigos asesinados", "Enemies killed", "Inimigos assassinados"),
            campaignManager.ObtenerEstadisticaEnemigosAsesinados()));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Civiles perdidos", "Civilians lost", "Civis perdidos"),
            campaignManager.ObtenerEstadisticaCivilesPerdidos()));
        sb.Append(FormatearLineaStat(
            ObtenerTextoStats("Asentamientos visitados", "Settlements visited", "Assentamentos visitados"),
            campaignManager.ObtenerEstadisticaAsentamientosVisitados()));
        txtStatsViaje.text = sb.ToString();
    }

    private string ObtenerTextoStats(string textoEs, string textoEn, string textoPt)
    {
        return ObtenerIdiomaActual() switch
        {
            TRADU.IdiomaPortugues => textoPt,
            TRADU.IdiomaIngles => textoEn,
            _ => textoEs
        };
    }

    private static string FormatearLineaStat(string etiqueta, int valor)
    {
        return etiqueta + "... " + valor;
    }

    private static string FormatearLineaStat(string etiqueta, string valor)
    {
        return etiqueta + "... " + valor;
    }

    public void AbrirMenuPersonajes(Personaje personajeInicial)
    {
        AbrirMenuPersonajes(personajeInicial, false, false, false);
    }

    public void AbrirMenuPersonajesPorHover(Personaje personajeInicial)
    {
        if (personajeInicial == null || MenuPersonajes == null)
        {
            return;
        }

        if (MenuPersonajes.activeInHierarchy && !menuPersonajesAbiertoPorHover)
        {
            return;
        }

        AbrirMenuPersonajes(personajeInicial, false, false, true);
    }

    public void CerrarMenuPersonajesPorHover(Personaje personaje)
    {
        if (!menuPersonajesAbiertoPorHover || MenuPersonajes == null)
        {
            return;
        }

        if (personajeMenuPersonajesHover != null && personaje != personajeMenuPersonajesHover)
        {
            return;
        }

        if (BloqueaCierreMenuPersonajesTutorialNuevo())
        {
            return;
        }

        MenuPersonajes.SetActive(false);
        menuPersonajesAbiertoPorHover = false;
        personajeMenuPersonajesHover = null;
        EmitirCierreMenuPersonajesTutorial(false);
    }

    private void AbrirMenuPersonajes(Personaje personajeInicial, bool alternarMenu, bool desdeHotkey, bool desdeHover)
    {
        bool estabaAbiertoPorHover = menuPersonajesAbiertoPorHover;
        if (desdeHotkey && MenuPersonajesEstaAbierto())
        {
            if (BloqueaCierreMenuPersonajesTutorialNuevo())
            {
                return;
            }

            MenuPersonajes.SetActive(false);
            menuPersonajesAbiertoPorHover = false;
            personajeMenuPersonajesHover = null;
            EmitirCierreMenuPersonajesTutorial(true);
            return;
        }

        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 5) { return; }
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 5)
        {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
            CampaignManager.Instance.CrearCaballero();
            CampaignManager.Instance.CrearExplorador();
            
        }
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 10)
        {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
            
        }

        var scMenuPersonajes = MenuPersonajes.GetComponent<MenuPersonajes>();
        if (scMenuPersonajes == null) return;

        if (!alternarMenu && personajeInicial != null && MenuPersonajes.activeInHierarchy && scMenuPersonajes.pSel == personajeInicial && !estabaAbiertoPorHover)
        {
            if (BloqueaCierreMenuPersonajesTutorialNuevo())
            {
                return;
            }

            MenuPersonajes.SetActive(false);
            menuPersonajesAbiertoPorHover = false;
            personajeMenuPersonajesHover = null;
            EmitirCierreMenuPersonajesTutorial(desdeHotkey);
            return;
        }

        if (alternarMenu && MenuPersonajesEstaAbierto() && BloqueaCierreMenuPersonajesTutorialNuevo())
        {
            return;
        }

        bool cambiaPersonajeConMenuAbierto = !alternarMenu
            && personajeInicial != null
            && MenuPersonajes.activeInHierarchy;
        if (!cambiaPersonajeConMenuAbierto)
        {
            CerrarMenusExclusivos();
        }

        CampaignManager.Instance.ActivarLog(0);
        bool estabaAbierto = MenuPersonajes.activeInHierarchy;
        bool abrir = alternarMenu ? !estabaAbierto : true;
        MenuPersonajes.SetActive(abrir);
        if (!abrir)
        {
            menuPersonajesAbiertoPorHover = false;
            personajeMenuPersonajesHover = null;
            EmitirCierreMenuPersonajesTutorial(desdeHotkey);
            return;
        }

        RuntimeAnalytics.TrackDesign("ui", "caravan", "open_characters");

        if (personajeInicial == null && scMenuPersonajes.listaPersonajes != null && scMenuPersonajes.listaPersonajes.Count > 0)
        {
            personajeInicial = scMenuPersonajes.listaPersonajes.Find(p => p != null && !p.Camp_Muerto);
            if (personajeInicial == null)
            {
                personajeInicial = scMenuPersonajes.listaPersonajes[0];
            }
        }

        menuPersonajesAbiertoPorHover = desdeHover;
        personajeMenuPersonajesHover = desdeHover ? personajeInicial : null;
        scMenuPersonajes.PrepararYAbrirMenu(personajeInicial);
        if (scMenuPersonajes.itemDesc != null)
        {
            scMenuPersonajes.itemDesc.text = "";
        }

     
       
    }

    private void EmitirCierreMenuPersonajesTutorial(bool desdeHotkey)
    {
        TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignCharacterMenuClosed, MenuPersonajes)
            .Add("menu", "personajes")
            .Add("closedByHotkey", desdeHotkey ? 1 : 0));
    }

    private static bool BloqueaCierreMenuPersonajesTutorialNuevo()
    {
        TutorialDirector tutorial = TutorialDirector.Instance;
        return tutorial != null && tutorial.BlocksCharacterMenuClose;
    }

    private void EmitirCierreMenuMejorasTutorial(bool desdeHotkey)
    {
        TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignUpgradeMenuClosed, MenuMejoras)
            .Add("menu", "mejoras")
            .Add("closedByHotkey", desdeHotkey ? 1 : 0));

        if (desdeHotkey && CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 17)
        {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
        }
    }

    private void EmitirCierreMenuSequitosTutorial(bool desdeHotkey)
    {
        TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignFollowersMenuClosed, MenuSequitos)
            .Add("menu", "sequitos")
            .Add("closedByHotkey", desdeHotkey ? 1 : 0));
    }

    public void ActualizarMejoras()
    {
        InicializarColoresCostoSiHaceFalta();

        //Antorchas
        costoMejorarAntorchas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaAntorchas, 40, 60));
        txtTierMejoraAntorchas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaAntorchas);
        txtCostoMejoraAntorchas.text = "" + costoMejorarAntorchas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAntorchas, costoMejorarAntorchas, colorCostoAntorchasNormal);
        ActualizarDiamantesMejora(AntorchasDiamantes, CampaignManager.Instance.mejoraCaravanaAntorchas);
        btMejoraAntorchas.SetActive(CampaignManager.Instance.mejoraCaravanaAntorchas < MaxTierMejoraCaravana);

        //Alforjas
        costoMejorarAlforjas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaAlforjas, 34, 52));
        txtTierMejoraAlforjas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaAlforjas);
        txtCostoMejoraAlforjas.text = "" + costoMejorarAlforjas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAlforjas, costoMejorarAlforjas, colorCostoAlforjasNormal);
        ActualizarDiamantesMejora(AlforjasDiamantes, CampaignManager.Instance.mejoraCaravanaAlforjas);
        btMejoraAlforjas.SetActive(CampaignManager.Instance.mejoraCaravanaAlforjas < MaxTierMejoraCaravana);

        //Tiendas
        costoMejorarTiendas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaTiendas, 69, 104));
        txtTierMejoraTiendas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaTiendas);
        txtCostoMejoraTiendas.text = "" + costoMejorarTiendas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraTiendas, costoMejorarTiendas, colorCostoTiendasNormal);
        ActualizarDiamantesMejora(TiendasDiamantes, CampaignManager.Instance.mejoraCaravanaTiendas);
        btMejoraTiendas.SetActive(CampaignManager.Instance.mejoraCaravanaTiendas < MaxTierMejoraCaravana);

        //Catalejos
        costoMejorarCatalejos = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaCatalejos, 45, 68));
        txtTierMejoraCatalejos.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaCatalejos);
        txtCostoMejoraCatalejos.text = "" + costoMejorarCatalejos + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraCatalejos, costoMejorarCatalejos, colorCostoCatalejosNormal);
        ActualizarDiamantesMejora(CatalejosDiamantes, CampaignManager.Instance.mejoraCaravanaCatalejos);
        btMejoraCatalejos.SetActive(CampaignManager.Instance.mejoraCaravanaCatalejos < MaxTierMejoraCaravana);

        //Almacen
        costoMejorarAlmacen = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaAlmacen, 46, 68));
        txtTierMejoraAlmacen.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaAlmacen);
        txtCostoMejoraAlmacen.text = "" + costoMejorarAlmacen + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAlmacen, costoMejorarAlmacen, colorCostoAlmacenNormal);
        ActualizarDiamantesMejora(AlmacenDiamantes, CampaignManager.Instance.mejoraCaravanaAlmacen);
        btMejoraAlmacen.SetActive(CampaignManager.Instance.mejoraCaravanaAlmacen < MaxTierMejoraCaravana);
        
        //Defensas
        costoMejorarDefensas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaDefensas, 36, 58));
        txtTierMejoraDefensas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaDefensas);
        txtCostoMejoraDefensas.text = "" + costoMejorarDefensas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraDefensas, costoMejorarDefensas, colorCostoDefensasNormal);
        ActualizarDiamantesMejora(DefensasDiamantes, CampaignManager.Instance.mejoraCaravanaDefensas);
        btMejoraDefensas.SetActive(CampaignManager.Instance.mejoraCaravanaDefensas < MaxTierMejoraCaravana);

        ActualizarDescripcionesMejoras();
    }


    public void MejorarAntorchas()
    {
       if(costoMejorarAntorchas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaAntorchas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaAntorchas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAntorchas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarAntorchas, "caravan_upgrade", "antorchas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "antorchas");
       }

        ActualizarMejoras();
        CampaignManager.Instance.scAtributosZona.ActualizarLuzNedukazal();
    }
    public void MejorarAlforjas()
    {   TutorialEvents.Emit("ui.mejoraralforjas", gameObject);
       if(costoMejorarAlforjas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaAlforjas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaAlforjas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAlforjas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarAlforjas, "caravan_upgrade", "alforjas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "alforjas");
       }
       if(CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 16)
       {
           CampaignManager.Instance.scTutorialManager.SiguientePaso();
       }

        ActualizarMejoras();

    }

    public void MejorarTiendas()
    {
       if(costoMejorarTiendas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaTiendas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaTiendas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarTiendas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarTiendas, "caravan_upgrade", "tiendas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "tiendas");
       }

        ActualizarMejoras();

    }

    public void MejorarCatalejos()
    {
       if(costoMejorarCatalejos <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaCatalejos < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaCatalejos += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarCatalejos);
        if (CampaignManager.Instance.scMapaManager != null)
        {
            CampaignManager.Instance.scMapaManager.RefrescarVisibilidadExploracion();
        }
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarCatalejos, "caravan_upgrade", "catalejos");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "catalejos");
       }

        ActualizarMejoras();

    }

    public void MejorarAlmacen()
    {
       if(costoMejorarAlmacen <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaAlmacen < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaAlmacen += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAlmacen);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarAlmacen, "caravan_upgrade", "almacen");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "almacen");
       }

        ActualizarMejoras();

    }

    public void MejorarDefensas()
    {
       if(costoMejorarDefensas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaDefensas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaDefensas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarDefensas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarDefensas, "caravan_upgrade", "defensas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "defensas");
       }

        ActualizarMejoras();

    }

    private string ObtenerDescripcionAntorchasEs(int tier, bool mostrarIncremento)
    {
        int visionActual = Mathf.RoundToInt(ObtenerAlcanceVisionAntorchas(tier) * 10f);
        int visionSiguiente = Mathf.RoundToInt(ObtenerAlcanceVisionAntorchas(tier + 1) * 10f);
        return "Antorchas de Caravana:\n"
            + "De noche, si están prendidas, aumentan el rango de visión de la caravana por "
            + visionActual + ObtenerIncrementoPreview(mostrarIncremento, tier, visionActual, visionSiguiente) + ".\n"
            + "Además aumentan las chances de encontrar items tras una batalla.";
    }

    private string ObtenerDescripcionAntorchasEn(int tier, bool mostrarIncremento)
    {
        int visionActual = Mathf.RoundToInt(ObtenerAlcanceVisionAntorchas(tier) * 10f);
        int visionSiguiente = Mathf.RoundToInt(ObtenerAlcanceVisionAntorchas(tier + 1) * 10f);
        return "Caravan Torches:\n"
            + "At night, when lit, they increase the caravan's Vision Range by "
            + visionActual + ObtenerIncrementoPreview(mostrarIncremento, tier, visionActual, visionSiguiente) + ".\n"
            + "They also increase the chance to find items after a battle.";
    }

    private string ObtenerDescripcionAntorchasPt(int tier, bool mostrarIncremento)
    {
        int visionActual = Mathf.RoundToInt(ObtenerAlcanceVisionAntorchas(tier) * 10f);
        int visionSiguiente = Mathf.RoundToInt(ObtenerAlcanceVisionAntorchas(tier + 1) * 10f);
        return "Tochas da Caravana:\n"
            + "À noite, quando acesas, aumentam o Alcance de Visão da caravana em "
            + visionActual + ObtenerIncrementoPreview(mostrarIncremento, tier, visionActual, visionSiguiente) + ".\n"
            + "Além disso, aumentam as chances de encontrar itens após uma batalha.";
    }

    private float ObtenerAlcanceVisionAntorchas(int tier)
    {
        float[] alcances = { 1.5f, 1.9f, 2.5f, 2.9f, 3.5f };
        int indice = Mathf.Clamp(tier - 1, 0, alcances.Length - 1);
        return Mathf.Max(1.5f, alcances[indice] * 0.90f);
    }

    private string ObtenerDescripcionAlforjasEs(int tier, bool mostrarIncremento)
    {
        int bonusActual = ObtenerBonusCargaPorBueyAlforjas(tier);
        int bonusSiguiente = ObtenerBonusCargaPorBueyAlforjas(tier + 1);
        return "Alforjas:\n"
            + "Agregan +" + bonusActual + ObtenerIncrementoPreview(mostrarIncremento, tier, bonusActual, bonusSiguiente)
            + " de capacidad de carga a los bueyes que llevan los suministros de la caravana.";
    }

    private string ObtenerDescripcionAlforjasEn(int tier, bool mostrarIncremento)
    {
        int bonusActual = ObtenerBonusCargaPorBueyAlforjas(tier);
        int bonusSiguiente = ObtenerBonusCargaPorBueyAlforjas(tier + 1);
        return "Saddlebags:\n"
            + "Add +" + bonusActual + ObtenerIncrementoPreview(mostrarIncremento, tier, bonusActual, bonusSiguiente)
            + " carrying capacity to the oxen that haul the caravan supplies.";
    }

    private string ObtenerDescripcionAlforjasPt(int tier, bool mostrarIncremento)
    {
        int bonusActual = ObtenerBonusCargaPorBueyAlforjas(tier);
        int bonusSiguiente = ObtenerBonusCargaPorBueyAlforjas(tier + 1);
        return "Alforjes:\n"
            + "Adicionam +" + bonusActual + ObtenerIncrementoPreview(mostrarIncremento, tier, bonusActual, bonusSiguiente)
            + " de capacidade de carga aos bois que levam os suprimentos da caravana.";
    }

    private string ObtenerDescripcionTiendasEs(int tier, bool mostrarIncremento)
    {
        int esperanzaActual = ObtenerBonusEsperanzaTiendas(tier);
        int esperanzaSiguiente = ObtenerBonusEsperanzaTiendas(tier + 1);
        int capacidadActual = Mathf.Max(0, tier - 1);
        int capacidadSiguiente = Mathf.Max(0, tier);
        return "Tiendas:\n"
            + "Las tiendas proporcionan refugio y descanso a los civiles.\n"
            + "Al descansar: +" + esperanzaActual + ObtenerIncrementoPreview(mostrarIncremento, tier, esperanzaActual, esperanzaSiguiente) + " Esperanza.\n"
            + "Además aumenta en +" + capacidadActual + ObtenerIncrementoPreview(mostrarIncremento, tier, capacidadActual, capacidadSiguiente)
            + " la capacidad de Personajes.";
    }

    private string ObtenerDescripcionTiendasEn(int tier, bool mostrarIncremento)
    {
        int esperanzaActual = ObtenerBonusEsperanzaTiendas(tier);
        int esperanzaSiguiente = ObtenerBonusEsperanzaTiendas(tier + 1);
        int capacidadActual = Mathf.Max(0, tier - 1);
        int capacidadSiguiente = Mathf.Max(0, tier);
        return "Tents:\n"
            + "Provide shelter and rest quality to the civilians.\n"
            + "When resting: +" + esperanzaActual + ObtenerIncrementoPreview(mostrarIncremento, tier, esperanzaActual, esperanzaSiguiente) + " Hope.\n"
            + "Also increases character capacity by +" + capacidadActual
            + ObtenerIncrementoPreview(mostrarIncremento, tier, capacidadActual, capacidadSiguiente) + ".";
    }

    private string ObtenerDescripcionTiendasPt(int tier, bool mostrarIncremento)
    {
        int esperanzaActual = ObtenerBonusEsperanzaTiendas(tier);
        int esperanzaSiguiente = ObtenerBonusEsperanzaTiendas(tier + 1);
        int capacidadActual = Mathf.Max(0, tier - 1);
        int capacidadSiguiente = Mathf.Max(0, tier);
        return "Tendas:\n"
            + "As tendas oferecem abrigo e descanso aos civis.\n"
            + "Ao descansar: +" + esperanzaActual + ObtenerIncrementoPreview(mostrarIncremento, tier, esperanzaActual, esperanzaSiguiente) + " Esperança.\n"
            + "Além disso, aumentam em +" + capacidadActual + ObtenerIncrementoPreview(mostrarIncremento, tier, capacidadActual, capacidadSiguiente)
            + " a capacidade de Personagens.";
    }

    private string ObtenerDescripcionCatalejosEs(int tier, bool mostrarIncremento)
    {
        int visionActual = ObtenerBonusVisionCatalejos(tier) * 10;
        int visionSiguiente = ObtenerBonusVisionCatalejos(tier + 1) * 10;
        return "Catalejos:\n"
            + "Disipan la niebla y aumentan en +" + visionActual
            + ObtenerIncrementoPreview(mostrarIncremento, tier, visionActual, visionSiguiente) + " el Rango de Visión de la caravana.";
    }

    private string ObtenerDescripcionCatalejosEn(int tier, bool mostrarIncremento)
    {
        int visionActual = ObtenerBonusVisionCatalejos(tier) * 10;
        int visionSiguiente = ObtenerBonusVisionCatalejos(tier + 1) * 10;
        return "Spyglasses:\n"
            + "Dispel the fog and increase the caravan's Vision Range by +" + visionActual
            + ObtenerIncrementoPreview(mostrarIncremento, tier, visionActual, visionSiguiente) + ".";
    }

    private string ObtenerDescripcionCatalejosPt(int tier, bool mostrarIncremento)
    {
        int visionActual = ObtenerBonusVisionCatalejos(tier) * 10;
        int visionSiguiente = ObtenerBonusVisionCatalejos(tier + 1) * 10;
        return "Lunetas:\n"
            + "Dissipam a névoa e aumentam em +" + visionActual
            + ObtenerIncrementoPreview(mostrarIncremento, tier, visionActual, visionSiguiente) + " o Alcance de Visão da caravana.";
    }

    private string ObtenerDescripcionAlmacenEs(int tier, bool mostrarIncremento)
    {
        int reduccionActual = ObtenerReduccionConsumoAlmacen(tier);
        int reduccionSiguiente = ObtenerReduccionConsumoAlmacen(tier + 1);
        int cargaActual = ObtenerBonusCargaAlmacen(tier);
        int cargaSiguiente = ObtenerBonusCargaAlmacen(tier + 1);
        return "Carro Almacén:\n"
            + "Al descansar se consumen " + reduccionActual + "%"
            + ObtenerIncrementoPreview(mostrarIncremento, tier, reduccionActual, reduccionSiguiente, "%") + " menos Suministros.\n"
            + "Además aumenta en " + cargaActual + ObtenerIncrementoPreview(mostrarIncremento, tier, cargaActual, cargaSiguiente)
            + " la capacidad total de carga.";
    }

    private string ObtenerDescripcionAlmacenEn(int tier, bool mostrarIncremento)
    {
        int reduccionActual = ObtenerReduccionConsumoAlmacen(tier);
        int reduccionSiguiente = ObtenerReduccionConsumoAlmacen(tier + 1);
        int cargaActual = ObtenerBonusCargaAlmacen(tier);
        int cargaSiguiente = ObtenerBonusCargaAlmacen(tier + 1);
        return "Storage Wagon:\n"
            + "While resting, Supplies consumption is reduced by " + reduccionActual + "%"
            + ObtenerIncrementoPreview(mostrarIncremento, tier, reduccionActual, reduccionSiguiente, "%") + ".\n"
            + "It also increases total carrying capacity by " + cargaActual
            + ObtenerIncrementoPreview(mostrarIncremento, tier, cargaActual, cargaSiguiente) + ".";
    }

    private string ObtenerDescripcionAlmacenPt(int tier, bool mostrarIncremento)
    {
        int reduccionActual = ObtenerReduccionConsumoAlmacen(tier);
        int reduccionSiguiente = ObtenerReduccionConsumoAlmacen(tier + 1);
        int cargaActual = ObtenerBonusCargaAlmacen(tier);
        int cargaSiguiente = ObtenerBonusCargaAlmacen(tier + 1);
        return "Carro Armazém:\n"
            + "Ao descansar, consomem-se " + reduccionActual + "%"
            + ObtenerIncrementoPreview(mostrarIncremento, tier, reduccionActual, reduccionSiguiente, "%") + " menos Suprimentos.\n"
            + "Além disso, aumenta em " + cargaActual + ObtenerIncrementoPreview(mostrarIncremento, tier, cargaActual, cargaSiguiente)
            + " a capacidade total de carga.";
    }

    private string ObtenerExplicacionVistaLejanaEs(int tierCatalejos)
    {
        return "<u>Rango de Visión:</u>\n"
            + "Determina cuántos nodos y caminos hacia adelante quedan dentro del claro de la niebla. Permite planear mejor y prever cómo se va conformando el mapa."
            + ObtenerFactoresVistaLejanaEs(tierCatalejos);
    }

    private string ObtenerExplicacionVistaLejanaEn(int tierCatalejos)
    {
        return "<u>Vision Range:</u>\n"
            + "Determines how many nodes and roads ahead remain inside the clearing in the fog. It helps you plan ahead and predict how the map will unfold."
            + ObtenerFactoresVistaLejanaEn(tierCatalejos);
    }

    private string ObtenerExplicacionVistaLejanaPt(int tierCatalejos)
    {
        return "<u>Alcance de Visão:</u>\n"
            + "Determina quantos nós e caminhos à frente ficam dentro da clareira na névoa. Permite planejar melhor e prever como o mapa vai se formando."
            + ObtenerFactoresVistaLejanaPt(tierCatalejos);
    }

    private string ObtenerExplicacionExploracionPasivaEs(int tierCatalejos)
    {
        return "<u>Exploración Pasiva:</u>\n"
            + "Al descansar o al llegar a un nodo nuevo, la caravana realizará una tirada de Exploración que, de tener éxito, permitirá revelar los tipos de nodos adyacentes y a veces también los que estén más adelante."
            + ObtenerFactoresExploracionPasivaEs(tierCatalejos);
    }

    private string ObtenerExplicacionExploracionPasivaEn(int tierCatalejos)
    {
        return "<u>Passive Exploration:</u>\n"
            + "When resting or arriving at a new node, the caravan makes an Exploration roll. On success, it reveals the types of adjacent nodes and sometimes nodes farther ahead."
            + ObtenerFactoresExploracionPasivaEn(tierCatalejos);
    }

    private string ObtenerExplicacionExploracionPasivaPt(int tierCatalejos)
    {
        return "<u>Exploração Passiva:</u>\n"
            + "Ao descansar ou chegar a um novo nó, a caravana fará uma rolagem de Exploração. Em caso de sucesso, ela revela os tipos dos nós adjacentes e às vezes também os que estiverem mais à frente."
            + ObtenerFactoresExploracionPasivaPt(tierCatalejos);
    }

    private string ObtenerFactoresVistaLejanaEs(int tierCatalejos)
    {
        return ObtenerResumenFactoresVistaLejana(
            "Valor actual",
            "Clima",
            ObtenerNombreClimaVisionEs(),
            "Noche",
            "Antorchas encendidas",
            "Antorchas apagadas",
            tierCatalejos);
    }

    private string ObtenerFactoresVistaLejanaEn(int tierCatalejos)
    {
        return ObtenerResumenFactoresVistaLejana(
            "Current value",
            "Weather",
            ObtenerNombreClimaVisionEn(),
            "Night",
            "Torches lit",
            "Torches extinguished",
            tierCatalejos);
    }

    private string ObtenerFactoresVistaLejanaPt(int tierCatalejos)
    {
        return ObtenerResumenFactoresVistaLejana(
            "Valor atual",
            "Clima",
            ObtenerNombreClimaVisionPt(),
            "Noite",
            "Tochas acesas",
            "Tochas apagadas",
            tierCatalejos);
    }

    private string ObtenerResumenFactoresVistaLejana(
        string etiquetaActual,
        string etiquetaClima,
        string nombreClima,
        string etiquetaNoche,
        string antorchasEncendidas,
        string antorchasApagadas,
        int tierCatalejos)
    {
        CampaignManager cm = CampaignManager.Instance;
        int baseVision = cm != null ? cm.ObtenerDistanciaVisionBase() : 2;
        int bonusVisionCatalejos = cm != null ? cm.ObtenerBonusDistanciaVisionCatalejos() : ObtenerBonusVisionCatalejos(tierCatalejos);
        int penalizacionVisionClima = cm != null ? cm.ObtenerPenalizacionClimaVision() : 0;
        int minimoVision = cm != null ? cm.ObtenerDistanciaVisionMinima() : 1;
        float visionActual = cm != null
            ? cm.ObtenerAlcanceVisionEnPasos()
            : (Mathf.Max(minimoVision, baseVision + bonusVisionCatalejos) + 0.25f) * 1.10f;

        List<string> lineas = new List<string>
        {
            etiquetaActual + ": " + (visionActual * 10f).ToString("0.###")
        };

        if (penalizacionVisionClima != 0)
        {
            lineas.Add(etiquetaClima + " (" + nombreClima + "): " + FormatearModificadorPorcentaje(-penalizacionVisionClima));
        }

        if (cm != null && cm.EsNocheActual())
        {
            lineas.Add(etiquetaNoche + " (" + (cm.AntorchasEncendidas ? antorchasEncendidas : antorchasApagadas) + ")");
        }

        return ObtenerTextoFactoresGris(lineas);
    }

    private string ObtenerFactoresExploracionPasivaEs(int tierCatalejos)
    {
        return ObtenerResumenFactoresExploracionPasiva(
            "Valor actual",
            "Región",
            "Clima",
            ObtenerNombreClimaVisionEs(),
            "Presagio",
            "Noche",
            tierCatalejos);
    }

    private string ObtenerFactoresExploracionPasivaEn(int tierCatalejos)
    {
        return ObtenerResumenFactoresExploracionPasiva(
            "Current value",
            "Region",
            "Weather",
            ObtenerNombreClimaVisionEn(),
            "Omen",
            "Night",
            tierCatalejos);
    }

    private string ObtenerFactoresExploracionPasivaPt(int tierCatalejos)
    {
        return ObtenerResumenFactoresExploracionPasiva(
            "Valor atual",
            "Região",
            "Clima",
            ObtenerNombreClimaVisionPt(),
            "Presságio",
            "Noite",
            tierCatalejos);
    }

    private string ObtenerResumenFactoresExploracionPasiva(
        string etiquetaActual,
        string etiquetaZona,
        string etiquetaClima,
        string nombreClima,
        string etiquetaPresagio,
        string etiquetaNoche,
        int tierCatalejos)
    {
        CampaignManager cm = CampaignManager.Instance;
        int chancePasiva = cm != null ? cm.ObtenerChanceExploracionPasiva() : 55;
        int modZona = cm != null && cm.scAtributosZona != null ? cm.scAtributosZona.modChanceExploracion : 0;
        int modClimaExploracion = cm != null && cm.intTipoClima == 5 ? -10 : 0;
        int modPresagio = cm != null && cm.TienePresagioActivo(PresagioCatalog.ZonaDesconocida) ? -10 : 0;
        int modNoche = cm != null && cm.EsNocheActual() ? -15 : 0;

        List<string> lineas = new List<string>
        {
            etiquetaActual + ": " + chancePasiva + "%"
        };

        if (modZona != 0)
        {
            lineas.Add(etiquetaZona + ": " + FormatearModificadorPorcentaje(modZona));
        }

        if (modClimaExploracion != 0)
        {
            lineas.Add(etiquetaClima + " (" + nombreClima + "): " + FormatearModificadorPorcentaje(modClimaExploracion));
        }

        if (modPresagio != 0)
        {
            lineas.Add(etiquetaPresagio + " (" + PresagioCatalog.ObtenerNombreLocalizado(PresagioCatalog.ZonaDesconocida) + "): "
                + FormatearModificadorPorcentaje(modPresagio));
        }

        if (modNoche != 0)
        {
            lineas.Add(etiquetaNoche + ": " + FormatearModificadorPorcentaje(modNoche));
        }

        return ObtenerTextoFactoresGris(lineas);
    }

    private string ObtenerTextoFactoresGris(List<string> lineas)
    {
        return "\n\n<color=" + ColorTextoFactoresExploracion + ">" + string.Join("\n", lineas) + "</color>";
    }

    private string FormatearModificadorEntero(int valor)
    {
        if (valor > 0)
        {
            return "+" + valor;
        }

        return valor.ToString();
    }

    private string FormatearModificadorPorcentaje(int valor)
    {
        return FormatearModificadorEntero(valor) + "%";
    }

    private string ObtenerNombreClimaVisionEs()
    {
        if (CampaignManager.Instance == null)
        {
            return "Desconocido";
        }

        switch (CampaignManager.Instance.intTipoClima)
        {
            case 1: return "Soleado";
            case 2: return "Ola de Calor";
            case 3: return "Lluvia";
            case 4: return "Nieve";
            case 5: return "Niebla";
            case 6: return "Almas Danzantes";
            case 7: return "Aurora Boreal";
            case 8: return "Nedukazal a oscuras";
            case 9: return "Masacre de Nedukazal";
            default: return "Desconocido";
        }
    }

    private string ObtenerNombreClimaVisionEn()
    {
        if (CampaignManager.Instance == null)
        {
            return "Unknown";
        }

        switch (CampaignManager.Instance.intTipoClima)
        {
            case 1: return "Sunny";
            case 2: return "Heat Wave";
            case 3: return "Rain";
            case 4: return "Snow";
            case 5: return "Fog";
            case 6: return "Dancing Souls";
            case 7: return "Aurora Borealis";
            case 8: return "Nedukazal in Darkness";
            case 9: return "Nedukazal Massacre";
            default: return "Unknown";
        }
    }

    private string ObtenerNombreClimaVisionPt()
    {
        if (CampaignManager.Instance == null)
        {
            return "Desconhecido";
        }

        switch (CampaignManager.Instance.intTipoClima)
        {
            case 1: return "Ensolarado";
            case 2: return "Onda de Calor";
            case 3: return "Chuva";
            case 4: return "Neve";
            case 5: return "Névoa";
            case 6: return "Almas Dançantes";
            case 7: return "Aurora Boreal";
            case 8: return "Nedukazal na escuridão";
            case 9: return "Massacre de Nedukazal";
            default: return "Desconhecido";
        }
    }

    private string ObtenerDescripcionDefensasEs(int tier, bool mostrarIncremento)
    {
        int reduccionActual = ObtenerReduccionPerdidaSequitoDefensas(tier);
        int reduccionSiguiente = ObtenerReduccionPerdidaSequitoDefensas(tier + 1);
        return "Defensas:\n"
            + "Agrega trampas y barricadas a los combates de ataques directos a la caravana.\n"
            + "Además reduce " + reduccionActual + "%"
            + ObtenerIncrementoPreview(mostrarIncremento, tier, reduccionActual, reduccionSiguiente, "%")
            + " las chances de perder un séquito tras una derrota.";
    }

    private string ObtenerDescripcionDefensasEn(int tier, bool mostrarIncremento)
    {
        int reduccionActual = ObtenerReduccionPerdidaSequitoDefensas(tier);
        int reduccionSiguiente = ObtenerReduccionPerdidaSequitoDefensas(tier + 1);
        return "Defenses:\n"
            + "Adds traps and barricades to direct caravan defense battles.\n"
            + "Also reduces the chance to lose a Retinue after a defeat by " + reduccionActual + "%"
            + ObtenerIncrementoPreview(mostrarIncremento, tier, reduccionActual, reduccionSiguiente, "%") + ".";
    }

    private string ObtenerDescripcionDefensasPt(int tier, bool mostrarIncremento)
    {
        int reduccionActual = ObtenerReduccionPerdidaSequitoDefensas(tier);
        int reduccionSiguiente = ObtenerReduccionPerdidaSequitoDefensas(tier + 1);
        return "Defesas:\n"
            + "Adiciona armadilhas e barricadas aos combates de ataques diretos à caravana.\n"
            + "Além disso, reduz em " + reduccionActual + "%"
            + ObtenerIncrementoPreview(mostrarIncremento, tier, reduccionActual, reduccionSiguiente, "%")
            + " as chances de perder um séquito após uma derrota.";
    }
}
