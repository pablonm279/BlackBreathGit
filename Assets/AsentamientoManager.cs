using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AsentamientoManager : MonoBehaviour
{
    private enum AccionAsentamiento
    {
        Ninguna = 0,
        PuestoComercial = 1,
        Taverna = 2,
        PlazaPrincipal = 3,
        Posada = 4,
        Marcharse = 5
    }

    private const int MaxAccionesBase = 3;
    private const int CostoTavernaBase = 100;
    private const int CostoPosadaBase = 100;
    private const float DuracionAccionHoras = 5f;
    private const float DuracionPosadaHoras = 8f;
    private const float RetrasoEntreLogsAccionSegundos = 0.9f;
    private const float MultiplicadorCuracionViajeAsentamiento = 1.1f;
    private const float DuracionFadePanelSegundos = 0.2f;
    private const string ColorSinAcciones = "#ff4d4d";
    private static readonly TipoEstadoCaravana[] EstadosCaravanaPosada =
    {
        TipoEstadoCaravana.Inspiracion,
        TipoEstadoCaravana.Presteza,
        TipoEstadoCaravana.Compromiso,
        TipoEstadoCaravana.Vigilante,
        TipoEstadoCaravana.Acobardados,
        TipoEstadoCaravana.Aletargados,
        TipoEstadoCaravana.Desmotivacion,
        TipoEstadoCaravana.Descuidados
    };

    [Header("UI Opcional")]
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject panelContenido;
    [SerializeField] private Image fondoPanel;
    [SerializeField] private Image inputBlocker;
    [SerializeField] private GameObject panelAsentamientoArrasado;

    [Header("Textos Opcionales")]
    [SerializeField] private TextMeshProUGUI txtTitulo;
    [SerializeField] private TextMeshProUGUI txtIntro;
    [SerializeField] private TextMeshProUGUI txtSubtitulo;
    [SerializeField] private TextMeshProUGUI txtDescripcionAccion;
    [SerializeField] public TextMeshProUGUI txtAccionesDisponibles;
    [SerializeField] private TextMeshProUGUI txtAsentamientoArrasado;

    [Header("Botones Opcionales")]
    [SerializeField] public Button btnPuestoComercial;
    [SerializeField] public Button btnTaverna;
    [SerializeField] public Button btnPlazaPrincipal;
    [SerializeField] public Button btnPosada;
    [SerializeField] public Button btnMarcharse;
    [SerializeField] private Button btnContinuarAsentamientoArrasado;

    private bool uiInicializada;
    private bool accionEnCurso;
    private bool asentamientoAbierto;
    private bool asentamientoArrasadoPorAliento;
    private bool volverAlAsentamientoTrasPuesto;
    private bool persistirAbiertoEnProximoSave;
    private int accionesRestantes = MaxAccionesBase;
    private AccionAsentamiento accionHoverActual = AccionAsentamiento.Ninguna;
    private readonly System.Collections.Generic.Dictionary<GameObject, bool> estadosVisibilidadDirecta = new System.Collections.Generic.Dictionary<GameObject, bool>();
    private readonly System.Collections.Generic.Dictionary<GameObject, bool> estadosCanvasCampaniaDuranteTransicion = new System.Collections.Generic.Dictionary<GameObject, bool>();
    private bool fondoPanelVisibleAntesDeTransicion = true;
    private Coroutine panelFadeCoroutine;

    private TMP_FontAsset fuenteTitulo;
    private TMP_FontAsset fuenteCuerpo;
    private TMP_FontAsset fuenteBoton;

    public bool TieneInteraccionActiva => asentamientoAbierto || accionEnCurso || volverAlAsentamientoTrasPuesto;
    public bool EstaAbierto => asentamientoAbierto;
    public bool DebeGuardarseComoAbierto => asentamientoAbierto || persistirAbiertoEnProximoSave;
    public int AccionesRestantes => accionesRestantes;

    private string MarcarSinAcciones(string texto)
    {
        return $"<color={ColorSinAcciones}>{texto}</color>";
    }

    public void AbrirAlLlegar()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null)
        {
            return;
        }

        AsegurarUi();
        if (!uiInicializada)
        {
            if (campaign.scMapaManager != null && campaign.scMapaManager.nodoActual != null)
            {
                campaign.scMapaManager.nodoActual.nodoDespejado = true;
            }

            campaign.EmpezarEvento(402);
            return;
        }
        CerrarSilencioso();

        accionesRestantes = ObtenerMaxAcciones();
        accionHoverActual = AccionAsentamiento.Ninguna;
        asentamientoArrasadoPorAliento = campaign.EstaDentroOpeorDelAlientoNegro();

        if (campaign.scMapaManager != null && campaign.scMapaManager.nodoActual != null)
        {
            campaign.scMapaManager.nodoActual.nodoDespejado = true;
        }

        campaign.ResetearPuestoComercial();
        if (campaign.scSequitoMercaderes != null)
        {
            campaign.scSequitoMercaderes.GenerarItemsVendidos(false);
        }

        campaign.CambiarEsperanzaActual(5);
        campaign.EscribirLog(Loc(
            "-La caravana llega a un asentamiento aislado. +5 Esperanza.",
            "-The caravan reaches an isolated settlement. +5 Hope.",
            "-A caravana chega a um assentamento isolado. +5 Esperanca."));
        campaign.AplicarTraitsLlegadaAsentamiento();
       /* campaign.EscribirLog(Loc(
            "-El sequito de mercaderes actualiza su oferta en el asentamiento.",
            "-The merchants entourage refreshes its stock in the settlement.",
            "-A comitiva de mercadores atualiza sua oferta no assentamento."));*/
        persistirAbiertoEnProximoSave = true;
        campaign.TryAutosaveCampania("asentamiento-llegada", out _);
        persistirAbiertoEnProximoSave = false;

        if (asentamientoArrasadoPorAliento)
        {
           
            MostrarPanelAsentamientoArrasado();
            return;
        }

        PrepararDescripcionPuestoComercial();
        MostrarPanelAsentamiento();
        ActualizarInterfaz();
    }

    public void RestaurarEstadoDesdeSave(CampaignSaveData data)
    {
        CerrarSilencioso();

        if (data == null || !data.settlementOpen)
        {
            return;
        }

        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null || campaign.scMapaManager == null || campaign.scMapaManager.nodoActual == null)
        {
            return;
        }

        if (campaign.scMapaManager.nodoActual.tipoNodo != 4)
        {
            return;
        }

        AsegurarUi();
        if (!uiInicializada)
        {
            return;
        }
        accionesRestantes = Mathf.Clamp(data.settlementActionsRemaining, 0, ObtenerMaxAcciones());
        accionHoverActual = AccionAsentamiento.Ninguna;
        asentamientoArrasadoPorAliento = campaign.EstaDentroOpeorDelAlientoNegro();

        if (asentamientoArrasadoPorAliento)
        {
           
            MostrarPanelAsentamientoArrasado();
            return;
        }

        PrepararDescripcionPuestoComercial();
        MostrarPanelAsentamiento();
        ActualizarInterfaz();
    }

    public void OnPuestoComercialCerrado()
    {
        if (!volverAlAsentamientoTrasPuesto)
        {
            return;
        }

        volverAlAsentamientoTrasPuesto = false;
        MostrarPanelAsentamiento();
        ActualizarInterfaz();
    }

    public void SeleccionarPuestoComercial() => IntentarEjecutarAccion(AccionAsentamiento.PuestoComercial);
    public void SeleccionarTaverna() => IntentarEjecutarAccion(AccionAsentamiento.Taverna);
    public void SeleccionarPlazaPrincipal() => IntentarEjecutarAccion(AccionAsentamiento.PlazaPrincipal);
    public void SeleccionarPosada() => IntentarEjecutarAccion(AccionAsentamiento.Posada);
    public void SeleccionarMarcharse() => IntentarEjecutarAccion(AccionAsentamiento.Marcharse);

    public void HoverPuestoComercial() => MostrarDescripcionAccion(AccionAsentamiento.PuestoComercial);
    public void HoverTaverna() => MostrarDescripcionAccion(AccionAsentamiento.Taverna);
    public void HoverPlazaPrincipal() => MostrarDescripcionAccion(AccionAsentamiento.PlazaPrincipal);
    public void HoverPosada() => MostrarDescripcionAccion(AccionAsentamiento.Posada);
    public void HoverMarcharse() => MostrarDescripcionAccion(AccionAsentamiento.Marcharse);

    public void LimpiarHover()
    {
        accionHoverActual = AccionAsentamiento.Ninguna;
        if (txtDescripcionAccion != null)
        {
            txtDescripcionAccion.text = ObtenerDescripcionDefault();
        }
    }

    private void IntentarEjecutarAccion(AccionAsentamiento accion)
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null || accionEnCurso)
        {
            return;
        }

        if (asentamientoArrasadoPorAliento)
        {
            return;
        }

        if (accion == AccionAsentamiento.Marcharse)
        {
            CerrarSilencioso();
            return;
        }

        if (accionesRestantes <= 0)
        {
            campaign.EscribirAdvertenciaLog(MarcarSinAcciones(Loc(
                "-No quedan acciones disponibles en este asentamiento.",
                "-No actions remain in this settlement.",
                "-Nao restam acoes disponiveis neste assentamento.")));
            MostrarDescripcionAccion(accion);
            return;
        }

        if (accion == AccionAsentamiento.Taverna && !PuedeUsarTaverna(out string motivoTaverna))
        {
            campaign.EscribirAdvertenciaLog("<color=#ff8f8f>" + motivoTaverna + "</color>", true);
            MostrarDescripcionAccion(accion);
            return;
        }

        if (accion == AccionAsentamiento.Posada && !PuedeUsarPosada(out string motivoPosada))
        {
            campaign.EscribirAdvertenciaLog("<color=#ff8f8f>" + motivoPosada + "</color>", true);
            MostrarDescripcionAccion(accion);
            return;
        }

        accionesRestantes = Mathf.Max(0, accionesRestantes - 1);
        StartCoroutine(EjecutarAccionCoroutine(accion));
    }

    private IEnumerator EjecutarAccionCoroutine(AccionAsentamiento accion)
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null)
        {
            yield break;
        }

        accionEnCurso = true;
        asentamientoAbierto = false;
        accionHoverActual = accion;
        MostrarUiEnTransicion();
        ActualizarInterfaz();
        campaign.ComenzarBufferTextosFlotantesCampania();

        bool esPosada = accion == AccionAsentamiento.Posada;
        float horasAccion = esPosada ? DuracionPosadaHoras : DuracionAccionHoras;
        yield return campaign.TranscurrirAccionCampania(
            horasAccion,
            esPosada ? TipoAvanceTiempoCampania.Posada : TipoAvanceTiempoCampania.Asentamiento,
            1f,
            esPosada ? 1f : MultiplicadorCuracionViajeAsentamiento,
            !esPosada);

        switch (accion)
        {
            case AccionAsentamiento.PuestoComercial:
                EjecutarResultadoPuestoComercial();
                break;
            case AccionAsentamiento.Taverna:
                EjecutarResultadoTaverna();
                break;
            case AccionAsentamiento.PlazaPrincipal:
                EjecutarResultadoPlazaPrincipal();
                break;
            case AccionAsentamiento.Posada:
                EjecutarResultadoPosada();
                break;
        }
        campaign.FinalizarAccionTemporal();

        System.Collections.Generic.List<(string texto, Color color)> textosBufferizados = campaign.FinalizarBufferTextosFlotantesCampania();
        accionEnCurso = false;
        RestaurarInterfazCampaniaTrasTransicion();
        yield return null;

        if (textosBufferizados.Count > 0)
        {
            yield return ReproducirTextosAccionAsentamiento(campaign, textosBufferizados);
        }

        campaign.LiberarTextosRecursosSuspendidos();

        if (accion == AccionAsentamiento.PuestoComercial)
        {
            yield break;
        }

        MostrarPanelAsentamiento();
        ActualizarInterfaz();
    }

    private IEnumerator ReproducirTextosAccionAsentamiento(CampaignManager campaign, System.Collections.Generic.List<(string texto, Color color)> textosBufferizados)
    {
        if (campaign == null || textosBufferizados == null || textosBufferizados.Count == 0)
        {
            yield break;
        }

        for (int i = 0; i < textosBufferizados.Count; i++)
        {
            (string texto, Color color) = textosBufferizados[i];
            campaign.GenerarTextoFlotanteCampaña(texto, color);

            if (i < textosBufferizados.Count - 1)
            {
                yield return new WaitForSecondsRealtime(RetrasoEntreLogsAccionSegundos);
            }
        }
    }

    private void EjecutarResultadoPuestoComercial()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null)
        {
            return;
        }

        RestaurarInterfazCampaniaTrasTransicion();
        volverAlAsentamientoTrasPuesto = true;
        PrepararDescripcionPuestoComercial();
        campaign.AplicarTraitsVisitaPuestoComercial();

        if (campaign.goUIComercioNodo != null)
        {
            campaign.goUIComercioNodo.SetActive(true);
        }
    }

    private void EjecutarResultadoTaverna()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null || !PuedeUsarTaverna(out _))
        {
            return;
        }

        int costoTaverna = ObtenerCostoServicio(CostoTavernaBase);
        campaign.CambiarOroActual(-costoTaverna);
        bool reclutado = campaign.AgregarHeroe(0);
        if (!reclutado)
        {
            campaign.CambiarOroActual(costoTaverna);
            campaign.EscribirLog("<color=#ff8f8f>" + Loc(
                "-No se pudo reclutar a nadie en la taverna.",
                "-No one could be recruited in the tavern.",
                "-Nao foi possivel recrutar alguem na taverna.") + "</color>", true);
            return;
        }

        campaign.EscribirLog(Loc(
            $"-Se pagan {costoTaverna} de Oro en la taverna para reclutar un nuevo personaje.",
            $"-{costoTaverna} Gold is paid at the tavern to recruit a new character.",
            $"-Pagam-se {costoTaverna} de Ouro na taverna para recrutar um novo personagem."));

        if (campaign.scMenuPersonajes != null && campaign.scMenuPersonajes.listaPersonajes != null)
        {
            foreach (Personaje pers in campaign.scMenuPersonajes.listaPersonajes)
            {
                if (pers == null || pers.Camp_Muerto || pers.IDClase == 3)
                {
                    continue;
                }

                pers.AplicarMoralAltaHoras(48f);
            }
        }

        campaign.EscribirLog(Loc(
            "-Los personajes comparten charlas y cerveza en la Taberna, obtienen Alta Moral durante 48 h. (No la Purificadora)",
            "-The characters share stories and beer at the Tavern, obtaining High Morale for 48 h. (Not the Purifier)",
            "-Os personagens compartilham histórias e cerveja na Taverna, obtendo Moral Alta durante 48 h. (Exceto a Purificadora)"));
    }

    private void EjecutarResultadoPlazaPrincipal()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null)
        {
            return;
        }

        int civiles = UnityEngine.Random.Range(12, 24);
        civiles = Mathf.Max(0, civiles + campaign.ObtenerModificadorCivilesPlazaAsentamiento());
        int suministros = civiles * 2;
        int materiales = civiles;

        campaign.CambiarCivilesActuales(civiles);
        campaign.CambiarSuministrosActuales(suministros);
        campaign.CambiarMaterialesActuales(materiales);
        campaign.EscribirLog(Loc(
            $"-El discurso en la plaza convence a {civiles} civiles. Se unen con {suministros} suministros y {materiales} materiales.",
            $"-The speech in the square convinces {civiles} civilians. They join with {suministros} supplies and {materiales} materials.",
            $"-O discurso na praca convence {civiles} civis. Eles se juntam com {suministros} suprimentos e {materiales} materiais."));
    }

    private void EjecutarResultadoPosada()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null || campaign.scMenuPersonajes == null || campaign.scMenuPersonajes.listaPersonajes == null)
        {
            return;
        }

        if (!PuedeUsarPosada(out _))
        {
            return;
        }

        int costoPosada = ObtenerCostoServicio(CostoPosadaBase);
        campaign.CambiarOroActual(-costoPosada);

        foreach (Personaje pers in campaign.scMenuPersonajes.listaPersonajes)
        {
            if (pers == null || pers.Camp_Muerto)
            {
                continue;
            }

            pers.SetCampFatigado(false);

        }

        int fatigaActual = campaign.GetFatigaActual();
        if (fatigaActual != 0)
        {
            campaign.CambiarFatigaActual(-fatigaActual);
        }

        campaign.CambiarEsperanzaActual(5);
        TipoEstadoCaravana estadoGanado = EstadosCaravanaPosada[UnityEngine.Random.Range(0, EstadosCaravanaPosada.Length)];
        campaign.AgregarEstadoCaravana(estadoGanado, 1);
        campaign.EscribirLog(Loc(
            $"-La caravana descansa en la posada. -{costoPosada} Oro, Fatiga a 0, +5 Esperanza.",
            $"-The caravan rests at the inn. -{costoPosada} Gold, Fatigue reset to 0, +5 Hope.",
            $"-A caravana descansa na hospedaria. -{costoPosada} Ouro, Fadiga zerada, +5 Esperanca."));
        campaign.EscribirLog(Loc(
            "-La posada deja a la Caravana con 1 estado de caravana al azar.",
            "-The inn leaves the Caravan with 1 random caravan state.",
            "-A hospedaria deixa a Caravana com 1 estado de caravana aleatorio."));
        campaign.AplicarPresagiosDescanso();

    }

    private bool PuedeUsarTaverna(out string motivo)
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null)
        {
            motivo = Loc(
                "La campania no esta disponible.",
                "The campaign is not available.",
                "A campanha nao esta disponivel.");
            return false;
        }

        if (campaign.GetOroActuales() < ObtenerCostoServicio(CostoTavernaBase))
        {
            motivo = Loc(
                "No hay suficiente Oro para pagar la taverna.",
                "There is not enough Gold to pay for the tavern.",
                "Nao ha Ouro suficiente para pagar a taverna.");
            return false;
        }

        if (campaign.CuantosPersonajesActivos() >= campaign.ObtenerCapacidadMaximaPersonajes())
        {
            motivo = Loc(
                "La caravana no tiene más tiendas para otro personaje.",
                "The caravan has no spare tents for another character.",
                "A caravana nao tem mais tendas para outro personagem.");
            return false;
        }

        motivo = string.Empty;
        return true;
    }

    private bool PuedeUsarPosada(out string motivo)
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null)
        {
            motivo = Loc(
                "La campania no esta disponible.",
                "The campaign is not available.",
                "A campanha nao esta disponivel.");
            return false;
        }

        if (campaign.GetOroActuales() < ObtenerCostoServicio(CostoPosadaBase))
        {
            motivo = Loc(
                "No hay suficiente Oro para pagar la posada.",
                "There is not enough Gold to pay for the inn.",
                "Nao ha Ouro suficiente para pagar a hospedaria.");
            return false;
        }

        motivo = string.Empty;
        return true;
    }

    private void MostrarPanelAsentamiento()
    {
        AsegurarUi();
        RestaurarInterfazCampaniaTrasTransicion();

        if (uiRoot != null)
        {
            uiRoot.SetActive(true);
        }

        RestaurarContenidoPrincipal();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (inputBlocker != null)
        {
            inputBlocker.gameObject.SetActive(false);
            inputBlocker.raycastTarget = false;
        }

        asentamientoAbierto = true;
        IniciarFadePanelAsentamiento();
    }

    private void MostrarPanelAsentamientoArrasado()
    {
        AsegurarUi();
        RestaurarInterfazCampaniaTrasTransicion();
        OcultarContenidoPrincipal();

        if (uiRoot != null && panelAsentamientoArrasado != uiRoot && (panelAsentamientoArrasado == null || !panelAsentamientoArrasado.transform.IsChildOf(uiRoot.transform)))
        {
            uiRoot.SetActive(false);
        }

        if (panelAsentamientoArrasado != null)
        {
            panelAsentamientoArrasado.SetActive(true);
        }

        asentamientoAbierto = true;
    }

    private void MostrarUiEnTransicion()
    {
        AsegurarUi();
        BanterBattleUI.CancelarCampania(true);
        OcultarInterfazCampaniaParaTransicion();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (inputBlocker != null)
        {
            inputBlocker.gameObject.SetActive(false);
            inputBlocker.raycastTarget = false;
        }
    }

    private void CerrarSilencioso()
    {
        asentamientoAbierto = false;
        accionEnCurso = false;
        volverAlAsentamientoTrasPuesto = false;
        persistirAbiertoEnProximoSave = false;
        accionHoverActual = AccionAsentamiento.Ninguna;
        asentamientoArrasadoPorAliento = false;
        RestaurarContenidoPrincipal();
        RestaurarInterfazCampaniaTrasTransicion();

        if (uiRoot != null)
        {
            uiRoot.SetActive(false);
        }

        if (inputBlocker != null)
        {
            inputBlocker.gameObject.SetActive(false);
            inputBlocker.raycastTarget = false;
        }
    }

    private void ActualizarInterfaz()
    {
        if (!uiInicializada)
        {
            return;
        }

        ActualizarTextosEstaticos();
        ActualizarTextoAcciones();
        ActualizarEtiquetasBotones();
        ActualizarDescripcionActiva();
    }

    private void ActualizarTextosEstaticos()
    {
        if (txtTitulo != null)
        {
            txtTitulo.text = Loc("ASENTAMIENTO", "SETTLEMENT", "ASSENTAMENTO");
        }

        if (txtIntro != null)
        {
            txtIntro.text = Loc(
                "La caravana ha llegado a un pequeno asentamiento aislado en el camino.\nParece que sus habitantes no estan preparados para lo que se aproxima, y el tiempo apremia.",
                "The caravan has reached a small isolated settlement on the road.\nIts people are not prepared for what is coming, and time is running short.",
                "A caravana chegou a um pequeno assentamento isolado na estrada.\nSeus habitantes nao estao preparados para o que se aproxima, e o tempo esta se esgotando.");
        }

        if (txtSubtitulo != null)
        {
            txtSubtitulo.text = Loc(
                "Elige como emplear el tiempo disponible.",
                "Choose how to spend the remaining time.",
                "Escolha como empregar o tempo restante.");
        }
    }

    private void ActualizarTextoAcciones()
    {
        if (txtAccionesDisponibles == null)
        {
            return;
        }

        int maxAcciones = ObtenerMaxAcciones();
        string textoAcciones = Loc(
            $"{accionesRestantes}/{maxAcciones} Acciones disponibles",
            $"{accionesRestantes}/{maxAcciones} Actions available",
            $"{accionesRestantes}/{maxAcciones} Acoes disponiveis");

        txtAccionesDisponibles.text = accionesRestantes <= 0 ? MarcarSinAcciones(textoAcciones) : textoAcciones;
    }

    private void ActualizarEtiquetasBotones()
    {
        ConfigurarEtiquetaBoton(btnPuestoComercial, Loc("Puesto Comercial", "Trading Post", "Posto Comercial"));
        ConfigurarEtiquetaBoton(btnTaverna, Loc("Taverna", "Tavern", "Taverna"));
        ConfigurarEtiquetaBoton(btnPlazaPrincipal, Loc("Plaza Principal", "Main Square", "Praca Principal"));
        ConfigurarEtiquetaBoton(btnPosada, Loc("Posada", "Inn", "Hospedaria"));
        ConfigurarEtiquetaBoton(btnMarcharse, Loc("Marcharse", "Leave", "Partir"));
    }

    private void ActualizarDescripcionActiva()
    {
        if (txtDescripcionAccion == null)
        {
            return;
        }

        txtDescripcionAccion.text = accionHoverActual == AccionAsentamiento.Ninguna
            ? ObtenerDescripcionDefault()
            : ObtenerDescripcionAccion(accionHoverActual);
    }

    private void MostrarDescripcionAccion(AccionAsentamiento accion)
    {
        accionHoverActual = accion;
        if (txtDescripcionAccion != null)
        {
            txtDescripcionAccion.text = ObtenerDescripcionAccion(accion);
        }
    }

    private string ObtenerDescripcionDefault()
    {
        if (accionesRestantes > 0)
        {
            return Loc(
                "Pasa el cursor por una acción para ver su efecto. Las acciones duran 5h; la Posada, 8h.",
                "Hover an action to inspect its effect. Actions take 5h; the Inn takes 8h.",
                "Passe o cursor sobre uma ação para ver seu efeito. As ações duram 5h; a Hospedaria, 8h.");
        }

        return MarcarSinAcciones(Loc(
            "No quedan acciones disponibles. Puedes marcharte cuando quieras.",
            "No actions remain. You can leave whenever you want.",
            "Nao restam acoes disponiveis. Voce pode partir quando quiser."));
    }

    private string ObtenerDescripcionAccion(AccionAsentamiento accion)
    {
        int modificadorCiviles = CampaignManager.Instance != null
            ? CampaignManager.Instance.ObtenerModificadorCivilesPlazaAsentamiento()
            : 0;
        int civilesMinimos = Mathf.Max(0, 12 + modificadorCiviles);
        int civilesMaximos = Mathf.Max(civilesMinimos, 23 + modificadorCiviles);
        int duracionHoras = accion == AccionAsentamiento.Posada ? 8 : 5;
        string estadoAcciones = accionesRestantes > 0
            ? Loc(
                $"Consume 1 acción · Duración: {duracionHoras}h.",
                $"Consumes 1 action · Duration: {duracionHoras}h.",
                $"Consome 1 ação · Duração: {duracionHoras}h.")
            : MarcarSinAcciones(Loc("No quedan acciones disponibles.", "No actions remain.", "Nao restam acoes disponiveis."));

        switch (accion)
        {
            case AccionAsentamiento.PuestoComercial:
                return Loc(
                    "Accederás al puesto comerical del asentamiento.\n\n" + estadoAcciones,
                    "Access the settlement's trading post.\n\n" + estadoAcciones,
                    "Acesse o posto comercial do assentamento.\n\n" + estadoAcciones);

            case AccionAsentamiento.Taverna:
                string estadoTaverna = PuedeUsarTaverna(out string motivo)
                    ? Loc(
                        $"Costo: {ObtenerCostoServicio(CostoTavernaBase)} Oro. Recluta un personaje aleatorio nivel 1.",
                        $"Cost: {ObtenerCostoServicio(CostoTavernaBase)} Gold. Recruits a random level 1character.",
                        $"Custo: {ObtenerCostoServicio(CostoTavernaBase)} Ouro. Recruta um personagem aleatorio nível 1.")
                    : motivo;
                return estadoTaverna + "\n\n" + estadoAcciones;

            case AccionAsentamiento.PlazaPrincipal:
                return Loc(
                    $"Das un discurso en la plaza para concientizar a los pobladores sobre la situación, y que deben unirse a la caravana si quieren sobrevivir. Convences entre {civilesMinimos} y {civilesMaximos} civiles para unirse. Cada civil aporta 2 suministros y 1 material.\n\n" + estadoAcciones,
                    $"You give a speech in the square to raise awareness among the settlers about the inminent danger, and try to convince them to join the caravan if they want to survive. You convince between {civilesMinimos} and {civilesMaximos} civilians to join. Each civilian brings 2 supplies and 1 material.\n\n" + estadoAcciones,
                    $"Voce da um discurso na praca para conscientizar os moradores sobre a situacao, e que devem se juntar a caravana se quiserem sobreviver. Voce convence entre {civilesMinimos} e {civilesMaximos} civis para se juntar. Cada civil traz 2 suprimentos e 1 material.\n\n" + estadoAcciones);

            case AccionAsentamiento.Posada:
                return Loc(
                    $"Costo: {ObtenerCostoServicio(CostoPosadaBase)} Oro. Durante 8 h todos descansan, recuperan 4% de su Vida máxima por hora y no progresan actividades. Al finalizar, la Fatiga vuelve a 0, la caravana gana 5 Esperanza y obtiene 1 estado de caravana al azar.\n\n" + estadoAcciones,
                    $"Cost: {ObtenerCostoServicio(CostoPosadaBase)} Gold. For 8 h everyone rests, recovers 4% maximum Health per hour, and activities do not progress. At the end, Fatigue is reset to 0, the caravan gains 5 Hope, and it obtains 1 random caravan state.\n\n" + estadoAcciones,
                    $"Custo: {ObtenerCostoServicio(CostoPosadaBase)} Ouro. Durante 8 h todos descansam, recuperam 4% da Vida máxima por hora e as atividades não progridem. Ao final, a Fadiga volta a 0, a caravana ganha 5 Esperança e obtém 1 estado de caravana aleatório.\n\n" + estadoAcciones);

            case AccionAsentamiento.Marcharse:
                return Loc(
                    "La caravaná abandonará el asentamiento inmediatamente.",
                    "The caravan will leave the settlement immediately.",
                    "A caravana abandonará o assentamento imediatamente.");
        }

        return ObtenerDescripcionDefault();
    }

    private void ConfigurarEtiquetaBoton(Button boton, string etiqueta)
    {
        if (boton == null)
        {
            return;
        }

        TextMeshProUGUI label = boton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = etiqueta;
        }
    }

    private void PrepararDescripcionPuestoComercial()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null || campaign.txtDescripcionPuestoComercial == null)
        {
            return;
        }

        campaign.txtDescripcionPuestoComercial.text = Loc(
            "El sequito de mercaderes abre su puesto dentro del asentamiento. Al cerrar el comercio volveras a estas opciones.",
            "The merchants entourage opens its stall inside the settlement. Closing the shop will return you to these options.",
            "A comitiva de mercadores abre sua banca dentro do assentamento. Ao fechar a loja voce retornara a estas opcoes.");
    }

    private string Loc(string es, string en, string pt)
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
        switch (idioma)
        {
            case TRADU.IdiomaIngles:
                return en;
            case TRADU.IdiomaPortugues:
                return pt;
            default:
                return es;
        }
    }

    private int ObtenerMaxAcciones()
    {
        return CampaignManager.Instance != null
            ? CampaignManager.Instance.ObtenerMaxAccionesAsentamiento()
            : MaxAccionesBase;
    }

    private int ObtenerCostoServicio(int costoBase)
    {
        return CampaignManager.Instance != null
            ? CampaignManager.Instance.ObtenerCostoServicioAsentamientoConPresagios(costoBase)
            : costoBase;
    }

    private void AsegurarUi()
    {
        if (uiInicializada)
        {
            return;
        }

        CargarFuentes();
        AutovincularUiExistente();

        if (uiRoot == null)
        {
            ConstruirUiRuntime();
        }

        if (panelRoot == null && uiRoot != null)
        {
            panelRoot = uiRoot.GetComponent<RectTransform>();
        }

        if (panelContenido == null && uiRoot != null)
        {
            panelContenido = uiRoot;
        }

        if (panelContenido == null && panelRoot != null)
        {
            panelContenido = panelRoot.gameObject;
        }

        if (canvasGroup == null && uiRoot != null)
        {
            canvasGroup = uiRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = uiRoot.AddComponent<CanvasGroup>();
            }
        }

        if (inputBlocker == null && uiRoot != null)
        {
            inputBlocker = ObtenerOCrearInputBlocker();
        }

        if (panelAsentamientoArrasado == null)
        {
            panelAsentamientoArrasado = BuscarGameObjectEnJerarquia(
                uiRoot != null ? uiRoot.transform : null,
                "UIAsentamientoDestruido",
                "SettlementCorruptionOverlay",
                "PanelAsentamientoArrasado");

            if (panelAsentamientoArrasado == null)
            {
                panelAsentamientoArrasado = BuscarGameObjectEnEscenaPorNombre("UIAsentamientoDestruido");
            }
        }

        Transform raizAsentamientoArrasado = panelAsentamientoArrasado != null ? panelAsentamientoArrasado.transform : (uiRoot != null ? uiRoot.transform : null);

        if (txtAsentamientoArrasado == null)
        {
            txtAsentamientoArrasado = BuscarComponenteEnJerarquia<TextMeshProUGUI>(
                raizAsentamientoArrasado,
                "TxtDescr",
                "TxtAsentamientoArrasado",
                "txtAsentamientoArrasado");
        }

        if (btnContinuarAsentamientoArrasado == null)
        {
            btnContinuarAsentamientoArrasado = BuscarComponenteEnJerarquia<Button>(
                raizAsentamientoArrasado,
                "BtnContinuar",
                "btnContinuar",
                "BtnContinuarAsentamientoArrasado",
                "ContinuarAsentamientoArrasado");
        }

       

        ConfigurarBotones();
        ConfigurarOverlayAsentamientoArrasado();
        ActualizarTextosEstaticos();

        if (uiRoot != null)
        {
            uiRoot.SetActive(false);
        }

        uiInicializada = uiRoot != null;
    }

    private void CargarFuentes()
    {
        fuenteTitulo = Resources.Load<TMP_FontAsset>("Fuentes/Cinzel/CinzelDecorative-Regular SDF");
        fuenteCuerpo = Resources.Load<TMP_FontAsset>("Fuentes/Cardo/Cardo-Regular SDF");
        fuenteBoton = Resources.Load<TMP_FontAsset>("Fuentes/SpectralSC/SpectralSC-Regular SDF");

        TMP_FontAsset fuenteDefault = TMP_Settings.defaultFontAsset;
        if (fuenteTitulo == null)
        {
            fuenteTitulo = fuenteDefault;
        }
        if (fuenteCuerpo == null)
        {
            fuenteCuerpo = fuenteDefault;
        }
        if (fuenteBoton == null)
        {
            fuenteBoton = fuenteDefault;
        }
    }

    private void AutovincularUiExistente()
    {
        if (uiRoot == null)
        {
            if (string.Equals(gameObject.name, "UIAsentamiento", StringComparison.OrdinalIgnoreCase))
            {
                uiRoot = gameObject;
            }
            else
            {
                uiRoot = BuscarGameObjectEnEscenaPorNombre("UIAsentamiento");
            }
        }

        if (uiRoot == null)
        {
            return;
        }

        if (panelRoot == null)
        {
            panelRoot = uiRoot.GetComponent<RectTransform>();
        }

        if (panelContenido == null)
        {
            panelContenido = uiRoot;
        }

        if (canvasGroup == null)
        {
            canvasGroup = uiRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = uiRoot.AddComponent<CanvasGroup>();
            }
        }

        if (fondoPanel == null)
        {
            fondoPanel = uiRoot.GetComponent<Image>();
        }

        if (txtTitulo == null)
        {
            txtTitulo = BuscarComponenteEnJerarquia<TextMeshProUGUI>(uiRoot.transform, "Titulotxt", "TxtTitulo", "TituloTxt", "Titulo");
        }

        if (txtIntro == null)
        {
            txtIntro = BuscarComponenteEnJerarquia<TextMeshProUGUI>(uiRoot.transform, "TxtDescr", "TxtDesc", "TxtIntro", "Descripcion");
        }

        if (txtSubtitulo == null)
        {
            txtSubtitulo = BuscarComponenteEnJerarquia<TextMeshProUGUI>(uiRoot.transform, "TxtDescr2", "TxtDesc2", "TxtSubtitulo", "Subtitulo");
        }

        if (txtDescripcionAccion == null)
        {
            txtDescripcionAccion = BuscarComponenteEnJerarquia<TextMeshProUGUI>(uiRoot.transform, "txtMecanica", "TxtMecanica", "TxtDescripcionAccion");
        }

        if (btnPuestoComercial == null)
        {
            btnPuestoComercial = BuscarComponenteEnJerarquia<Button>(uiRoot.transform, "Mercado", "PuestoComercial", "BtnPuestoComercial");
        }

        if (btnTaverna == null)
        {
            btnTaverna = BuscarComponenteEnJerarquia<Button>(uiRoot.transform, "Taverna", "BtnTaverna");
        }

        if (btnPlazaPrincipal == null)
        {
            btnPlazaPrincipal = BuscarComponenteEnJerarquia<Button>(uiRoot.transform, "Plaza", "PlazaPrincipal", "BtnPlazaPrincipal");
        }

        if (btnPosada == null)
        {
            btnPosada = BuscarComponenteEnJerarquia<Button>(uiRoot.transform, "Posada", "BtnPosada");
        }

        if (btnMarcharse == null)
        {
            btnMarcharse = BuscarComponenteEnJerarquia<Button>(uiRoot.transform, "Marcharse", "BtnMarcharse");
        }

        if (txtAccionesDisponibles == null)
        {
            txtAccionesDisponibles = BuscarComponenteEnJerarquia<TextMeshProUGUI>(uiRoot.transform, "TxtAccionesDisponibles", "txtAccionesDisponibles", "TxtAcciones", "AccionesDisponibles");
        }

        if (txtAccionesDisponibles == null)
        {
            txtAccionesDisponibles = CrearTextoAccionesDisponiblesEnUiExistente();
        }

        if (inputBlocker == null)
        {
            inputBlocker = ObtenerOCrearInputBlocker();
        }
    }

    private GameObject BuscarGameObjectEnEscenaPorNombre(string nombre)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidato = transforms[i];
            if (candidato == null || !candidato.gameObject.scene.IsValid())
            {
                continue;
            }

            if (string.Equals(candidato.name, nombre, StringComparison.OrdinalIgnoreCase))
            {
                return candidato.gameObject;
            }
        }

        return null;
    }

    private GameObject BuscarGameObjectEnJerarquia(Transform raiz, params string[] nombres)
    {
        if (raiz == null || nombres == null)
        {
            return null;
        }

        for (int i = 0; i < nombres.Length; i++)
        {
            Transform encontrado = BuscarHijoProfundoPorNombre(raiz, nombres[i]);
            if (encontrado != null)
            {
                return encontrado.gameObject;
            }
        }

        return null;
    }

    private T BuscarComponenteEnJerarquia<T>(Transform raiz, params string[] nombres) where T : Component
    {
        if (raiz == null || nombres == null)
        {
            return null;
        }

        for (int i = 0; i < nombres.Length; i++)
        {
            Transform encontrado = BuscarHijoProfundoPorNombre(raiz, nombres[i]);
            if (encontrado == null)
            {
                continue;
            }

            T componente = encontrado.GetComponent<T>();
            if (componente == null)
            {
                componente = encontrado.GetComponentInChildren<T>(true);
            }

            if (componente != null)
            {
                return componente;
            }
        }

        return null;
    }

    private Transform BuscarHijoProfundoPorNombre(Transform raiz, string nombre)
    {
        if (raiz == null || string.IsNullOrEmpty(nombre))
        {
            return null;
        }

        if (string.Equals(raiz.name, nombre, StringComparison.OrdinalIgnoreCase))
        {
            return raiz;
        }

        for (int i = 0; i < raiz.childCount; i++)
        {
            Transform encontrado = BuscarHijoProfundoPorNombre(raiz.GetChild(i), nombre);
            if (encontrado != null)
            {
                return encontrado;
            }
        }

        return null;
    }

    private Image ObtenerOCrearInputBlocker()
    {
        if (uiRoot == null)
        {
            return null;
        }

        Image blocker = BuscarComponenteEnJerarquia<Image>(uiRoot.transform, "SettlementBlocker", "InputBlocker");
        if (blocker == null)
        {
            Transform padre = panelRoot != null ? (Transform)panelRoot : uiRoot.transform;
            blocker = CrearPanelSimple("SettlementBlocker", padre, new Color(0f, 0f, 0f, 0.22f));
            RectTransform blockerRect = blocker.rectTransform;
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;
        }

        blocker.transform.SetAsLastSibling();
        blocker.color = new Color(0f, 0f, 0f, 0.22f);
        blocker.raycastTarget = false;
        blocker.gameObject.SetActive(false);
        return blocker;
    }

    private void OcultarContenidoPrincipal()
    {
        if (panelContenido != null && panelContenido != uiRoot)
        {
            panelContenido.SetActive(false);
            return;
        }

        if (uiRoot == null)
        {
            return;
        }

        estadosVisibilidadDirecta.Clear();
        for (int i = 0; i < uiRoot.transform.childCount; i++)
        {
            Transform hijo = uiRoot.transform.GetChild(i);
            if (hijo == null || hijo.gameObject == inputBlocker?.gameObject)
            {
                continue;
            }

            estadosVisibilidadDirecta[hijo.gameObject] = hijo.gameObject.activeSelf;
            hijo.gameObject.SetActive(false);
        }

        if (fondoPanel != null)
        {
            fondoPanelVisibleAntesDeTransicion = fondoPanel.enabled;
            fondoPanel.enabled = false;
        }
    }

    private void RestaurarContenidoPrincipal()
    {
        if (panelContenido != null && panelContenido != uiRoot)
        {
            panelContenido.SetActive(true);
        }

        if (panelAsentamientoArrasado != null)
        {
            panelAsentamientoArrasado.SetActive(false);
        }

        if (fondoPanel != null)
        {
            fondoPanel.enabled = fondoPanelVisibleAntesDeTransicion;
        }

        if (estadosVisibilidadDirecta.Count == 0)
        {
            return;
        }

        foreach (System.Collections.Generic.KeyValuePair<GameObject, bool> estado in estadosVisibilidadDirecta)
        {
            if (estado.Key != null)
            {
                estado.Key.SetActive(estado.Value);
            }
        }

        estadosVisibilidadDirecta.Clear();
    }

    private void OcultarInterfazCampaniaParaTransicion()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null)
        {
            return;
        }

        OcultarElementosCanvasCampaniaParaTransicion(campaign);
    }

    private void RestaurarInterfazCampaniaTrasTransicion()
    {
        CampaignManager campaign = CampaignManager.Instance;
        RestaurarElementosCanvasCampaniaTrasTransicion(campaign);
    }

    private void OcultarElementosCanvasCampaniaParaTransicion(CampaignManager campaign)
    {
        if (campaign == null || campaign.goCanvas == null)
        {
            return;
        }

        estadosCanvasCampaniaDuranteTransicion.Clear();
        Transform canvasTransform = campaign.goCanvas.transform;
        for (int i = 0; i < canvasTransform.childCount; i++)
        {
            Transform hijo = canvasTransform.GetChild(i);
            if (hijo == null || DebeMantenerseActivoDuranteTransicion(hijo))
            {
                continue;
            }

            estadosCanvasCampaniaDuranteTransicion[hijo.gameObject] = hijo.gameObject.activeSelf;
            hijo.gameObject.SetActive(false);
        }
    }

    private void RestaurarElementosCanvasCampaniaTrasTransicion(CampaignManager campaign)
    {
        if (campaign == null || campaign.goCanvas == null || estadosCanvasCampaniaDuranteTransicion.Count == 0)
        {
            return;
        }

        foreach (System.Collections.Generic.KeyValuePair<GameObject, bool> estado in estadosCanvasCampaniaDuranteTransicion)
        {
            if (estado.Key != null)
            {
                estado.Key.SetActive(estado.Value);
            }
        }

        estadosCanvasCampaniaDuranteTransicion.Clear();
    }

    private bool DebeMantenerseActivoDuranteTransicion(Transform candidato)
    {
        if (candidato == null || uiRoot == null)
        {
            return false;
        }

        Transform uiTransform = uiRoot.transform;
        return candidato == uiTransform || uiTransform.IsChildOf(candidato);
    }

    private void IniciarFadePanelAsentamiento()
    {
        if (canvasGroup == null)
        {
            return;
        }

        if (panelFadeCoroutine != null)
        {
            StopCoroutine(panelFadeCoroutine);
        }

        panelFadeCoroutine = StartCoroutine(FadePanelAsentamiento());
    }

    private IEnumerator FadePanelAsentamiento()
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float alphaInicial = canvasGroup.alpha;
        float tiempo = 0f;

        while (tiempo < DuracionFadePanelSegundos)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / DuracionFadePanelSegundos);
            canvasGroup.alpha = Mathf.Lerp(alphaInicial, 1f, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        panelFadeCoroutine = null;
    }

    private void ConfigurarOverlayAsentamientoArrasado()
    {
        if (panelAsentamientoArrasado != null)
        {
            panelAsentamientoArrasado.SetActive(false);
        }


        if (btnContinuarAsentamientoArrasado != null)
        {
            btnContinuarAsentamientoArrasado.onClick.RemoveAllListeners();
            btnContinuarAsentamientoArrasado.onClick.AddListener(ContinuarAsentamientoArrasado);
            ConfigurarEtiquetaBoton(btnContinuarAsentamientoArrasado, Loc("Continuar", "Continue", "Continuar"));
        }
    }

    
    public void ContinuarAsentamientoArrasado()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null || campaign.scMenuBatallas == null || campaign.goMenuBatallas == null)
        {
            return;
        }

        CerrarSilencioso();
        campaign.goMenuBatallas.SetActive(true);
        campaign.scMenuBatallas.EventoBatallaEliteCorruptos();
    }

    private TextMeshProUGUI CrearTextoAccionesDisponiblesEnUiExistente()
    {
        Transform padre = panelRoot != null ? (Transform)panelRoot : (uiRoot != null ? uiRoot.transform : null);
        if (padre == null)
        {
            return null;
        }

        TextMeshProUGUI texto = CrearTexto(
            "TxtAccionesDisponibles",
            padre,
            fuenteBoton,
            24,
            FontStyles.Bold,
            new Color(0.91f, 0.84f, 0.55f),
            TextAlignmentOptions.Right);

        AjustarRect(
            texto.rectTransform,
            new Vector2(0.62f, 0.86f),
            new Vector2(0.88f, 0.93f),
            Vector2.zero,
            Vector2.zero);

        return texto;
    }

    private void ConstruirUiRuntime()
    {
        Canvas canvas = ObtenerCanvasRaiz();
        if (canvas == null)
        {
            return;
        }

        uiRoot = new GameObject("UIAsentamiento", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        uiRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = uiRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image oscurecedor = uiRoot.GetComponent<Image>();
        oscurecedor.color = new Color(0f, 0f, 0f, 0.72f);
        oscurecedor.raycastTarget = true;
        canvasGroup = uiRoot.GetComponent<CanvasGroup>();

        inputBlocker = CrearPanelSimple("SettlementBlocker", rootRect, new Color(0f, 0f, 0f, 0.22f));
        RectTransform blockerRect = inputBlocker.rectTransform;
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = Vector2.zero;
        blockerRect.offsetMax = Vector2.zero;
        inputBlocker.raycastTarget = false;
        inputBlocker.gameObject.SetActive(false);

        panelRoot = new GameObject("PanelAsentamiento", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
        panelRoot.SetParent(rootRect, false);
        panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
        panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.sizeDelta = new Vector2(1220f, 760f);
        panelRoot.anchoredPosition = Vector2.zero;
        panelContenido = panelRoot.gameObject;

        fondoPanel = panelRoot.GetComponent<Image>();
        Sprite fondo = Resources.Load<Sprite>("Imagenes/Fondo_Asentamiento");
        if (fondo == null && CampaignManager.Instance != null && CampaignManager.Instance.goUIComercioNodo != null)
        {
            Image fondoComercio = CampaignManager.Instance.goUIComercioNodo.GetComponent<Image>();
            if (fondoComercio != null)
            {
                fondo = fondoComercio.sprite;
            }
        }
        fondoPanel.sprite = fondo;
        fondoPanel.type = Image.Type.Sliced;
        fondoPanel.color = fondo != null ? Color.white : new Color(0.08f, 0.07f, 0.06f, 0.96f);

        Image descripcionBox = CrearPanelSimple("DescripcionBox", panelRoot, new Color(0f, 0f, 0f, 0.58f));
        RectTransform descripcionRect = descripcionBox.rectTransform;
        descripcionRect.anchorMin = new Vector2(0.49f, 0.22f);
        descripcionRect.anchorMax = new Vector2(0.91f, 0.52f);
        descripcionRect.offsetMin = Vector2.zero;
        descripcionRect.offsetMax = Vector2.zero;

        txtTitulo = CrearTexto("TituloAsentamiento", panelRoot, fuenteTitulo, 42, FontStyles.Bold, new Color(0.58f, 0.68f, 0.84f), TextAlignmentOptions.Center);
        AjustarRect(txtTitulo.rectTransform, new Vector2(0.2f, 0.88f), new Vector2(0.8f, 0.98f), Vector2.zero, Vector2.zero);

        txtIntro = CrearTexto("IntroAsentamiento", panelRoot, fuenteCuerpo, 27, FontStyles.Italic, new Color(0.86f, 0.86f, 0.84f, 0.96f), TextAlignmentOptions.TopLeft);
        AjustarRect(txtIntro.rectTransform, new Vector2(0.14f, 0.58f), new Vector2(0.84f, 0.85f), Vector2.zero, Vector2.zero);
        txtIntro.lineSpacing = -6f;

        txtSubtitulo = CrearTexto("SubtituloAsentamiento", panelRoot, fuenteBoton, 26, FontStyles.Bold, new Color(0.82f, 0.77f, 0.44f), TextAlignmentOptions.Left);
        AjustarRect(txtSubtitulo.rectTransform, new Vector2(0.14f, 0.5f), new Vector2(0.82f, 0.57f), Vector2.zero, Vector2.zero);

        txtAccionesDisponibles = CrearTexto("TxtAccionesDisponibles", panelRoot, fuenteBoton, 24, FontStyles.Bold, new Color(0.91f, 0.84f, 0.55f), TextAlignmentOptions.Right);
        AjustarRect(txtAccionesDisponibles.rectTransform, new Vector2(0.63f, 0.88f), new Vector2(0.88f, 0.94f), Vector2.zero, Vector2.zero);

        txtDescripcionAccion = CrearTexto("TxtDescripcionAccion", descripcionRect, fuenteCuerpo, 24, FontStyles.Bold, new Color(0.92f, 0.91f, 0.88f), TextAlignmentOptions.TopLeft);
        AjustarRect(txtDescripcionAccion.rectTransform, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.92f), Vector2.zero, Vector2.zero);
        txtDescripcionAccion.lineSpacing = -4f;

        float xMin = 0.16f;
        float xMax = 0.38f;
        btnPuestoComercial = CrearBotonAccion("BtnPuestoComercial", panelRoot, new Vector2(xMin, 0.39f), new Vector2(xMax, 0.45f));
        btnTaverna = CrearBotonAccion("BtnTaverna", panelRoot, new Vector2(xMin, 0.32f), new Vector2(xMax, 0.38f));
        btnPlazaPrincipal = CrearBotonAccion("BtnPlazaPrincipal", panelRoot, new Vector2(xMin, 0.25f), new Vector2(xMax, 0.31f));
        btnPosada = CrearBotonAccion("BtnPosada", panelRoot, new Vector2(xMin, 0.18f), new Vector2(xMax, 0.24f));
        btnMarcharse = CrearBotonAccion("BtnMarcharse", panelRoot, new Vector2(xMin, 0.11f), new Vector2(xMax, 0.17f));
    }

    private Canvas ObtenerCanvasRaiz()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign != null && campaign.goCanvas != null)
        {
            Canvas canvasEnGoCanvas = campaign.goCanvas.GetComponentInChildren<Canvas>(true);
            if (canvasEnGoCanvas != null)
            {
                return canvasEnGoCanvas;
            }
        }

        return FindObjectOfType<Canvas>();
    }

    private void ConfigurarBotones()
    {
        ConfigurarBoton(btnPuestoComercial, SeleccionarPuestoComercial, () => MostrarDescripcionAccion(AccionAsentamiento.PuestoComercial));
        ConfigurarBoton(btnTaverna, SeleccionarTaverna, () => MostrarDescripcionAccion(AccionAsentamiento.Taverna));
        ConfigurarBoton(btnPlazaPrincipal, SeleccionarPlazaPrincipal, () => MostrarDescripcionAccion(AccionAsentamiento.PlazaPrincipal));
        ConfigurarBoton(btnPosada, SeleccionarPosada, () => MostrarDescripcionAccion(AccionAsentamiento.Posada));
        ConfigurarBoton(btnMarcharse, SeleccionarMarcharse, () => MostrarDescripcionAccion(AccionAsentamiento.Marcharse));
    }

    private void ConfigurarBoton(Button boton, UnityEngine.Events.UnityAction onClick, UnityEngine.Events.UnityAction onHover)
    {
        if (boton == null)
        {
            return;
        }

        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(onClick);

        EventTrigger trigger = boton.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = boton.gameObject.AddComponent<EventTrigger>();
        }

        trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
        AgregarEventoHover(trigger, EventTriggerType.PointerEnter, onHover);
        AgregarEventoHover(trigger, EventTriggerType.PointerExit, LimpiarHover);
    }

    private void AgregarEventoHover(EventTrigger trigger, EventTriggerType tipo, UnityEngine.Events.UnityAction callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = tipo };
        entry.callback.AddListener(_ => callback());
        trigger.triggers.Add(entry);
    }

    private Image CrearPanelSimple(string nombre, Transform padre, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private TextMeshProUGUI CrearTexto(string nombre, Transform padre, TMP_FontAsset fuente, float size, FontStyles estilo, Color color, TextAlignmentOptions alineacion)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.font = fuente;
        tmp.fontSize = size;
        tmp.fontStyle = estilo;
        tmp.color = color;
        tmp.alignment = alineacion;
        tmp.raycastTarget = false;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    private Button CrearBotonAccion(string nombre, Transform padre, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = go.GetComponent<Image>();
        image.color = new Color(0.21f, 0.16f, 0.11f, 0.92f);

        Button boton = go.GetComponent<Button>();
        ColorBlock colors = boton.colors;
        colors.normalColor = new Color(0.34f, 0.25f, 0.16f, 1f);
        colors.highlightedColor = new Color(0.48f, 0.36f, 0.22f, 1f);
        colors.pressedColor = new Color(0.25f, 0.18f, 0.12f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.22f, 0.22f, 0.22f, 0.75f);
        boton.colors = colors;

        TextMeshProUGUI label = CrearTexto("Label", rect, fuenteBoton, 24, FontStyles.Bold, new Color(0.93f, 0.88f, 0.73f), TextAlignmentOptions.Center);
        AjustarRect(label.rectTransform, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.94f), Vector2.zero, Vector2.zero);
        label.enableAutoSizing = true;
        label.fontSizeMin = 14;
        label.fontSizeMax = 24;

        return boton;
    }

    private void AjustarRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
