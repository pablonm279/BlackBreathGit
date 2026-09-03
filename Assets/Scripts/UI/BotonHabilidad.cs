using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Net;

public class BotonHabilidad : MonoBehaviour, IPointerClickHandler
{
    private int costoMovimientoPreview = -1;
    private const string TooltipEsforzarId = "combate_esforzar";

    public Habilidad HabilidadRepresentada;
    public Image HabilidadCooldownMuestra;
    public TextMeshProUGUI turnosCooldown;

    [SerializeField] private bool BotonActivo = false;
    [SerializeField] private GameObject goDesc;
    [SerializeField] private TextMeshProUGUI txtDescHab;
    [SerializeField] private TMP_SpriteAsset spriteAssetIconosCombate;
    [SerializeField] private float hoverDelay = 0.35f;
    [SerializeField] private GameObject pinHab;

    [SerializeField] private GameObject prefabCirculoAccion;
    [SerializeField] private GameObject seleccionada;

    [SerializeField] private GameObject contenedorCirculosAccion;

    [SerializeField] private TextMeshProUGUI txtHotkey;

    public UIBotonesHabilidades scUiBotonesHabilidades;


    TextMeshProUGUI nombreHabilidad;
    private Coroutine hoverDescripcionRoutine;
    private bool hoverDescripcionActiva;
    // En campaña (fuera de batalla), un click deja la descripción fijada para que el jugador
    // pueda mover el mouse hacia el tooltip y hacer hover sobre los términos resaltados.
    private bool descripcionFijada;
    // Único botón (fuera de batalla) con su descripción abierta a la vez: al abrir una nueva,
    // se cierra la anterior (aplica tanto a hover como a fijado por click).
    private static BotonHabilidad botonConDescripcionAbierta;
    private RectTransform rectBoton;
    private Image imagenBoton;
    private Image realceInteraccion;
    private RectTransform rectRealceInteraccion;
    private RectTransform rectPinHab;
    private Image imagenPinHab;
    private Vector2 tamanoBaseBoton;
    private Vector2 posicionBasePinHab;
    private Vector3 escalaBasePinHab;
    private Color colorBasePinHab;
    private bool pinHabEscalaInicializada;
    private Coroutine animacionInteraccionRoutine;
    private Vector3 escalaBaseBoton;
    private Quaternion rotacionBaseBoton;
    private int cooldownAnterior = -1;
    private Sprite spriteCirculoAccionNormal;
    private Sprite spriteCirculoEsfuerzo;
    private static readonly Color colorCirculoDisponible = Color.white;
    private static readonly Color colorCirculoFaltante = new Color(0.5f, 0.5f, 0.5f, 1f);
    private static readonly Color colorRealceNormal = new Color(1f, 0.84f, 0.38f, 1f);
    private static readonly Color colorRealceRechazo = new Color(1f, 0.18f, 0.12f, 1f);
    private static readonly Color colorRealceDisponible = new Color(0.55f, 1f, 0.72f, 1f);
    private const float EscalaPinHabFijado = 1.16f;
    private const float AlphaPinHabHover = 0.6f;
    private void Awake()
    {
        scUiBotonesHabilidades = transform.parent.GetComponent<UIBotonesHabilidades>();
        nombreHabilidad = transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        rectBoton = GetComponent<RectTransform>();
        imagenBoton = GetComponent<Image>();
        escalaBaseBoton = rectBoton.localScale;
        rotacionBaseBoton = rectBoton.localRotation;
        if (seleccionada != null)
        {
            seleccionada.SetActive(false);
        }
        AsegurarPinHab();
        CachearAjustePinHab();
        ActualizarIndicadorPinCampania();
    }

    private void OnRectTransformDimensionsChange()
    {
        AjustarPinHabAlTamanoBoton();
    }

    void Start()
    {

        imagenBoton.sprite = HabilidadRepresentada.ObtenerIconoUI();
        CrearRealceInteraccion();
        cooldownAnterior = HabilidadRepresentada.cooldownActual;
        CachearSpritesCirculosAccion();



        // Instanciar tantos prefabCirculoAccion en contenedorCirculosAccion como el costo de AP de la habilidad
        if (contenedorCirculosAccion != null && prefabCirculoAccion != null && HabilidadRepresentada != null)
        {
            // Limpiar los círculos existentes primero
            foreach (Transform child in contenedorCirculosAccion.transform)
            {
                Destroy(child.gameObject);
            }

            int cantidad = Mathf.Max(0, (int)HabilidadRepresentada.costoAP);
            for (int i = 0; i < cantidad; i++)
            {
                Instantiate(prefabCirculoAccion, contenedorCirculosAccion.transform);
            }
        }

        ActualizarVisualCirculosAccion();
    }

    private void OnDisable()
    {
        if (hoverDescripcionRoutine != null)
        {
            StopCoroutine(hoverDescripcionRoutine);
        }
        if (animacionInteraccionRoutine != null)
        {
            StopCoroutine(animacionInteraccionRoutine);
        }

        hoverDescripcionActiva = false;
        hoverDescripcionRoutine = null;
        animacionInteraccionRoutine = null;
        descripcionFijada = false;
        ActualizarIndicadorPinCampania();
        if (botonConDescripcionAbierta == this)
        {
            botonConDescripcionAbierta = null;
        }

        if (rectBoton != null)
        {
            rectBoton.localScale = ObtenerEscalaInteraccion(BotonActivo);
            rectBoton.localRotation = rotacionBaseBoton;
        }
        RestaurarRealceInteraccion();
        AplicarAlphaRealce(ObtenerAlphaRealce(BotonActivo));
        if (goDesc != null)
        {
            goDesc.SetActive(false);
        }
        BattleManager.Instance?.OcultarPanelDescripcionHabilidad(HabilidadRepresentada, true);
    }

    // Cierra la descripción de este botón sin afectar a botonConDescripcionAbierta (lo hace
    // el llamador, que ya está por asignarse a sí mismo como el nuevo abierto).
    private void CerrarDescripcionPropia()
    {
        descripcionFijada = false;
        hoverDescripcionActiva = false;
        ActualizarIndicadorPinCampania();
        if (hoverDescripcionRoutine != null)
        {
            StopCoroutine(hoverDescripcionRoutine);
            hoverDescripcionRoutine = null;
        }
        if (goDesc != null)
        {
            goDesc.SetActive(false);
        }
    }

    // En campaña, hace click-para-fijar la descripción (no interfiere con el uso de la
    // habilidad en batalla, que se activa por otro camino).
    public void OnPointerClick(PointerEventData eventData)
    {
        if (EstaEnBatallaActiva())
        {
            return;
        }

        descripcionFijada = !descripcionFijada;
        ActualizarIndicadorPinCampania();
        if (descripcionFijada)
        {
            MostrarDescripcion();
        }
        else
        {
            if (botonConDescripcionAbierta == this)
            {
                botonConDescripcionAbierta = null;
            }
            if (!hoverDescripcionActiva && goDesc != null)
            {
                goDesc.SetActive(false);
            }
            ActualizarIndicadorPinCampania();
        }
    }

    private void ActualizarIndicadorPinCampania()
    {
        AsegurarPinHab();

        bool esCampania = !EstaEnBatallaActiva();
        if (pinHab != null)
        {
            bool mostrarIndicador = esCampania && (hoverDescripcionActiva || descripcionFijada);
            pinHab.SetActive(mostrarIndicador);

            if (imagenPinHab != null)
            {
                Color color = colorBasePinHab;
                color.a *= descripcionFijada ? 1f : AlphaPinHabHover;
                imagenPinHab.color = color;
                imagenPinHab.raycastTarget = false;
            }
        }

        AjustarPinHabAlTamanoBoton();
    }

    private void AsegurarPinHab()
    {
        if (pinHab != null)
        {
            return;
        }

        Transform encontrado = BuscarHijoPorNombre(transform, "Pinhab");
        if (encontrado != null)
        {
            pinHab = encontrado.gameObject;
        }
    }

    private void CachearAjustePinHab()
    {
        if (rectBoton == null || pinHab == null)
        {
            return;
        }

        rectPinHab = pinHab.GetComponent<RectTransform>();
        if (rectPinHab == null)
        {
            return;
        }

        imagenPinHab = pinHab.GetComponent<Image>();
        if (imagenPinHab != null)
        {
            colorBasePinHab = imagenPinHab.color;
        }

        tamanoBaseBoton = rectBoton.rect.size;
        posicionBasePinHab = rectPinHab.anchoredPosition;
        escalaBasePinHab = rectPinHab.localScale;
        pinHabEscalaInicializada = tamanoBaseBoton.x > 0f && tamanoBaseBoton.y > 0f;
        AjustarPinHabAlTamanoBoton();
    }

    private void AjustarPinHabAlTamanoBoton()
    {
        if (!pinHabEscalaInicializada || rectBoton == null || rectPinHab == null)
        {
            return;
        }

        Vector2 tamanoActual = rectBoton.rect.size;
        if (tamanoActual.x <= 0f || tamanoActual.y <= 0f)
        {
            return;
        }

        Vector2 proporcion = new Vector2(
            tamanoActual.x / tamanoBaseBoton.x,
            tamanoActual.y / tamanoBaseBoton.y);

        rectPinHab.anchoredPosition = Vector2.Scale(posicionBasePinHab, proporcion);
        float multiplicadorFijado = descripcionFijada ? EscalaPinHabFijado : 1f;
        rectPinHab.localScale = new Vector3(
            escalaBasePinHab.x * proporcion.x * multiplicadorFijado,
            escalaBasePinHab.y * proporcion.y * multiplicadorFijado,
            escalaBasePinHab.z);
    }

    private Transform BuscarHijoPorNombre(Transform raiz, string nombre)
    {
        if (raiz == null)
        {
            return null;
        }

        foreach (Transform hijo in raiz)
        {
            if (hijo.name == nombre)
            {
                return hijo;
            }

            Transform encontrado = BuscarHijoPorNombre(hijo, nombre);
            if (encontrado != null)
            {
                return encontrado;
            }
        }

        return null;
    }

    private bool EstaEnBatallaActiva()
    {
        return BattleManager.Instance != null
            && CampaignManager.Instance != null
            && CampaignManager.Instance.scAdministradorEscenas != null
            && CampaignManager.Instance.scAdministradorEscenas.escenaActual == 1;
    }

    public void hoverDescripcion(int n)
    {
        if (n == 1)
        {
            hoverDescripcionActiva = true;
            ActualizarIndicadorPinCampania();
            TransicionarInteraccion(ObtenerEscalaInteraccion(BotonActivo), ObtenerAlphaRealce(), 0.1f);
            if (hoverDescripcionRoutine != null)
            {
                StopCoroutine(hoverDescripcionRoutine);
                hoverDescripcionRoutine = null;
            }

            if (hoverDelay <= 0f)
            {
                MostrarDescripcion();
            }
            else
            {
                hoverDescripcionRoutine = StartCoroutine(HoverDescripcionConDelay());
            }
        }
        else
        {
            hoverDescripcionActiva = false;
            ActualizarIndicadorPinCampania();
            TransicionarInteraccion(ObtenerEscalaInteraccion(BotonActivo), ObtenerAlphaRealce(), 0.1f);
            if (hoverDescripcionRoutine != null)
            {
                StopCoroutine(hoverDescripcionRoutine);
                hoverDescripcionRoutine = null;
            }

            if (descripcionFijada)
            {
                // La descripción quedó fijada por click: se mantiene visible para poder
                // hacer hover sobre los términos resaltados dentro del tooltip.
                return;
            }

            if (EstaEnBatallaActiva())
            {
                BattleManager.Instance.OcultarPanelDescripcionHabilidad(HabilidadRepresentada, false);
            }
            if (goDesc != null)
            {
                goDesc.SetActive(false);
            }
        }
    }

    private IEnumerator HoverDescripcionConDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        if (hoverDescripcionActiva)
        {
            MostrarDescripcion();
        }
        hoverDescripcionRoutine = null;
    }

    private void MostrarDescripcion()
    {
        if (HabilidadRepresentada == null)
        {
            return;
        }

        if (EstaEnBatallaActiva())
        {
            BattleManager.Instance.TryMostrarPanelDescripcionHabilidad(HabilidadRepresentada, false);
            return;
        }

        if (botonConDescripcionAbierta != null && botonConDescripcionAbierta != this)
        {
            botonConDescripcionAbierta.CerrarDescripcionPropia();
        }
        botonConDescripcionAbierta = this;

        HabilidadRepresentada.ActualizarDescripcion();
        TMP_SpriteAsset spriteAsset = ObtenerSpriteAssetIconosCombate();
        if (txtDescHab != null && spriteAsset != null)
        {
            TextoIconosCombate.NormalizarSpriteAsset(spriteAsset);
            txtDescHab.spriteAsset = spriteAsset;
        }

        txtDescHab.text = ConstruirDescripcionFormateada(HabilidadRepresentada, spriteAsset, DebeMostrarEtiquetaIntrinsecaCampania());
        TerminoHoverDetector.AsegurarEn(txtDescHab);
        goDesc.SetActive(true);

        // Asegurarnos de que el goDesc (RectTransform) no salga de los margenes de la pantalla
        RectTransform descRect = goDesc.GetComponent<RectTransform>();

        // Obtener las dimensiones de la pantalla
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        // Obtener las coordenadas de la imagen en pantalla (anclada a su pivote)
        Vector3[] corners = new Vector3[4];
        descRect.GetWorldCorners(corners);

        // Comprobar si alguna esquina esta fuera de los margenes de la pantalla
        for (int i = 0; i < 4; i++)
        {
            Vector3 corner = corners[i];

            // Si alguna parte de la descripcion esta fuera del lado izquierdo de la pantalla
            if (corner.x < 0)
            {
                descRect.position += new Vector3(-corner.x, 0, 0); // Ajustar al margen izquierdo
            }

            // Si alguna parte de la descripcion esta fuera del lado derecho de la pantalla
            if (corner.x > screenSize.x)
            {
                descRect.position += new Vector3(screenSize.x - corner.x, 0, 0); // Ajustar al margen derecho
            }

            // Si alguna parte de la descripcion esta fuera de la parte inferior de la pantalla
            if (corner.y < 0)
            {
                descRect.position += new Vector3(0, -corner.y, 0); // Ajustar al margen inferior
            }

            // Si alguna parte de la descripcion esta fuera de la parte superior de la pantalla
            if (corner.y > screenSize.y)
            {
                descRect.position += new Vector3(0, screenSize.y - corner.y, 0); // Ajustar al margen superior
            }
        }
    }

    private TMP_SpriteAsset ObtenerSpriteAssetIconosCombate()
    {
        if (spriteAssetIconosCombate != null)
        {
            return spriteAssetIconosCombate;
        }

        return EstaEnBatallaActiva() ? BattleManager.Instance.SpriteAssetCombate : null;
    }

    public static string ConstruirDescripcionFormateada(Habilidad habilidad, TMP_SpriteAsset spriteAsset, bool incluirEtiquetaIntrinseca = false)
    {
        if (habilidad == null)
        {
            return string.Empty;
        }

        string descripcion = Habilidad.LimpiarCostoValentiaDescripcion(habilidad.txtDescripcion);
        if (incluirEtiquetaIntrinseca)
        {
            descripcion = InsertarEtiquetaIntrinseca(descripcion);
        }

        bool incluirIconos = spriteAsset != null;
        string descripcionFormateada;
        if (habilidad.GetType().Name.Contains("REPRESENTACION"))
        {
            descripcionFormateada = TextoIconosCombate.LimitarRepeticionIconos(
                TextoIconosCombate.FormatearIconosDespuesDelTitulo(descripcion, incluirIconos),
                2);
        }
        else
        {
            descripcionFormateada = TextoIconosCombate.LimitarRepeticionIconos(
                TextoIconosCombate.FormatearIconosDesdeBloqueMecanico(descripcion, incluirIconos),
                2);
        }

        return TextoIconosCombate.NormalizarIconosInline(descripcionFormateada);
    }

    private bool DebeMostrarEtiquetaIntrinsecaCampania()
    {
        Habilidad habilidad = HabilidadRepresentada;
        if (habilidad == null || EstaEnBatallaActiva())
        {
            return false;
        }

        if (habilidad.NIVEL < 0)
        {
            return true;
        }

        Personaje personaje = scMenuPersonajes != null ? scMenuPersonajes.pSel : null;
        if (personaje == null)
        {
            return false;
        }

        if (personaje.itemArma != null && habilidad.agregaDesdeArmaUI == personaje.itemArma)
        {
            return true;
        }

        switch (personaje.IDClase)
        {
            case 1:
                return habilidad is REPRESENTACIONCorajeInquebrantable;
            case 2:
                return habilidad is REPRESENTACIONPasoCauteloso
                    || habilidad is ImprovisarFlechas
                    || habilidad is CorteDaga;
            case 3:
                return habilidad is REPRESENTACIONAlmaEndeble
                    || habilidad is REPRESENTACIONFervorConjunto;
            case 4:
                return habilidad is REPRESENTACIONSueldo
                    || habilidad is REPRESENTACIONSigiloso
                    || habilidad is TiroBallestaDeMano;
            case 5:
                return habilidad is REPRESENTACIONSobrecarga
                    || habilidad is AcumularEnergia
                    || habilidad is DescargaArcana;
            case 6:
                return habilidad is REPRESENTACIONPasoLigero
                    || habilidad is REPRESENTACIONPosturaDemandante;
            default:
                return false;
        }
    }

    private static string InsertarEtiquetaIntrinseca(string descripcion)
    {
        if (string.IsNullOrEmpty(descripcion))
        {
            return descripcion;
        }

        const string idTermino = "intrinsic";
        string etiqueta = ObtenerTextoIntrinseca();
        string lineaIntrinseca = $"<link=\"skill-term:{idTermino}\"><color=#8f8f8f><i>{etiqueta}</i></color></link>";
        int indicePrimerBloque = descripcion.IndexOf("\n\n", System.StringComparison.Ordinal);
        if (indicePrimerBloque < 0)
        {
            return descripcion + "\n" + lineaIntrinseca;
        }

        return descripcion.Insert(indicePrimerBloque, "\n" + lineaIntrinseca);
    }

    private static string ObtenerTextoIntrinseca()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
        if (idioma == TRADU.IdiomaIngles)
        {
            return "Intrinsic";
        }

        if (idioma == TRADU.IdiomaPortugues)
        {
            return "Intrínseca";
        }

        return "Intrínseca";
    }

    private void CachearSpritesCirculosAccion()
    {
        if (spriteCirculoAccionNormal == null && prefabCirculoAccion != null)
        {
            Image imagenPrefab = prefabCirculoAccion.GetComponent<Image>();
            if (imagenPrefab != null)
            {
                spriteCirculoAccionNormal = imagenPrefab.sprite;
            }
        }

        if (spriteCirculoEsfuerzo == null)
        {
            spriteCirculoEsfuerzo = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/esforzar");
        }
    }

    private void ActualizarVisualCirculosAccion()
    {
        if (contenedorCirculosAccion == null || HabilidadRepresentada == null || BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
        {
            return;
        }

        CachearSpritesCirculosAccion();

        int apActual = Mathf.Max(0, Mathf.FloorToInt(BattleManager.Instance.unidadActiva.ObtenerAPActual()));
        int apNecesarios = Mathf.Max(0, (int)HabilidadRepresentada.costoAP);
        int faltantes = Mathf.Max(0, apNecesarios - apActual);
        int faltantesEsforzables = Mathf.Min(faltantes, Mathf.Max(0, HabilidadRepresentada.esforzable));
        int primerIndiceEsfuerzo = apNecesarios - faltantesEsforzables;

        for (int i = 0; i < contenedorCirculosAccion.transform.childCount; i++)
        {
            Image img = contenedorCirculosAccion.transform.GetChild(i).GetComponent<Image>();
            if (img == null)
            {
                continue;
            }

            bool circuloFaltante = i >= apActual && i < apNecesarios;
            bool mostrarEsfuerzo = circuloFaltante && faltantesEsforzables > 0 && i >= primerIndiceEsfuerzo;

            if (mostrarEsfuerzo && spriteCirculoEsfuerzo != null)
            {
                img.sprite = spriteCirculoEsfuerzo;
            }
            else if (spriteCirculoAccionNormal != null)
            {
                img.sprite = spriteCirculoAccionNormal;
            }

            img.color = circuloFaltante ? colorCirculoFaltante : colorCirculoDisponible;
        }
    }

    public void ActivarHabilidad(bool yaVienedeCargando)
    {
        if (!EstaEnBatallaActiva())
        {
            return;
        }

        TutorialCombate tutorialCombate = BattleManager.Instance.scTutorialCombate;
        bool habilidadPermitidaPorTutorial = tutorialCombate == null
            || tutorialCombate.PermiteHabilidadEnPasoActual(HabilidadRepresentada);
        if (!habilidadPermitidaPorTutorial)
        {
            return;
        }

        if (!yaVienedeCargando
            && BattleManager.Instance.EntradaBatallaBloqueadaPorUI
            && (tutorialCombate == null || !tutorialCombate.EsPasoSeleccionBallesta()))
        {
            return;
        }

        if (HabilidadRepresentada is REPRESENTACIONEvasionMaestra evasionMaestra)
        {
            evasionMaestra.AlternarEnCombate();
            BattleManager.Instance.TryMostrarPanelDescripcionHabilidad(evasionMaestra, false);
            ReproducirConfirmacionInteraccion();
            return;
        }

        if (HabilidadRepresentada != null)
        {
            TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.BattleAbilityClicked, gameObject)
                .Add("abilityName", HabilidadRepresentada.nombre)
                .Add("abilityType", HabilidadRepresentada.GetType().Name));
        }

        if(tutorialCombate != null && tutorialCombate.tutorialCombateActivo)
        {
            if(tutorialCombate.EstaAntesDelPasoAtaqueBallesta())
            {
                tutorialCombate.SiguientePasoCombate();
            }
        }

        if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
        { return; } // Sale del método si la escena no es "ES-Batallas"
        BattleManager.Instance.bOcupado = false;
        if (HabilidadRepresentada.GetType().Name.Contains("REPRESENTACION"))
        { return; } //Si se clickea el boton de una pasiva, no pasa nada

        if (HabilidadRepresentada.cooldownActual > 0)
        { ReproducirRechazoInteraccion(); return; } //Control extra para que no se puedan activar habilidades en cooldown

        if (HabilidadRepresentada is CargaDeEstoque cargaDeEstoque && !cargaDeEstoque.PuedeActivarseDesdePosicionActual(out string motivoCarga))
        {
            BattleManager.Instance.unidadActiva?.GenerarTextoFlotante(TRADU.i.Traducir(motivoCarga), Color.gray, FloatingTextContext.Generic);
            ReproducirRechazoInteraccion();
            return;
        }

        if (HabilidadRepresentada is RecuperarAire recuperarAire && !recuperarAire.PuedeActivarseDesdePosicionActual(out string motivoRecuperarAire))
        {
            BattleManager.Instance.unidadActiva?.GenerarTextoFlotante(TRADU.i.Traducir(motivoRecuperarAire), Color.gray, FloatingTextContext.Generic);
            ReproducirRechazoInteraccion();
            return;
        }

        if (HabilidadRepresentada.esMelee && !PuedeUsarHabilidadMelee())
        {
            Unidad unidadActiva = BattleManager.Instance.unidadActiva;
            if (unidadActiva != null && unidadActiva.estado_inmovil > 0)
            {
                unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("Inmóvil, Melee solo adyacente."), Color.gray, FloatingTextContext.Generic);
            }
            else
            {
                unidadActiva?.GenerarTextoFlotante(TRADU.i.Traducir("Adelántate para usarla"), Color.gray, FloatingTextContext.Generic);
            }
            ReproducirRechazoInteraccion();
            return;
        }

        if (HabilidadRepresentada.requiereRecurso > 0) //de las habilidades que requieran un recurso, se fija una por una si lo cumplen
        {
            if (HabilidadRepresentada.nombre == "Tiro con Arco")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes flechas"), Color.gray, FloatingTextContext.Resist); ReproducirRechazoInteraccion(); return; }
            }
            if (HabilidadRepresentada.nombre == "Tiro Potente")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes flechas"), Color.gray, FloatingTextContext.Resist); ReproducirRechazoInteraccion(); return; }
            }
            if (HabilidadRepresentada.nombre == "Vigilancia")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseExplorador>().ObtenerCantidadFlechas() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes flechas"), Color.gray, FloatingTextContext.Resist); ReproducirRechazoInteraccion(); return; }
            }
            if (HabilidadRepresentada.nombre == "Enmendar" || HabilidadRepresentada.nombre == "Salmo Purificador")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClasePurificadora>().ObtenerFervor() < HabilidadRepresentada.requiereRecurso)
                {
                    BattleManager.Instance.unidadActiva.GenerarTextoFlotante(ObtenerTextoSinFervor(), Color.gray, FloatingTextContext.Resist);
                    ReproducirRechazoInteraccion();
                    return;
                }
            }
            if (HabilidadRepresentada.nombre == "Asesinar")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseAcechador>().ObtenerEstaEscondido() < HabilidadRepresentada.requiereRecurso)
                {
                    BattleManager.Instance.unidadActiva.GenerarTextoFlotante(ObtenerTextoRequiereEstarEscondido(), Color.gray, FloatingTextContext.Resist);
                    ReproducirRechazoInteraccion();
                    return;
                }
            }
            if (HabilidadRepresentada.nombre == "Descarga Desintegradora")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseCanalizador>().ObtenerEnergia() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes energía"), Color.gray, FloatingTextContext.Resist); ReproducirRechazoInteraccion(); return; }
            }
            if (HabilidadRepresentada.nombre == "Manifestacion Arcana")
            {
                if (BattleManager.Instance.unidadActiva.gameObject.GetComponent<ClaseCanalizador>().ObtenerEnergia() < HabilidadRepresentada.requiereRecurso)
                { BattleManager.Instance.unidadActiva.GenerarTextoFlotante(TRADU.i.Traducir("No hay suficientes energía"), Color.gray, FloatingTextContext.Resist); ReproducirRechazoInteraccion(); return; }
            }

        }

        if (!yaVienedeCargando)
        {
            if (ChequearCargaHabilidad())
            { return; }
        }


        BattleManager.Instance.LimpiarCapasCasillas();
        BattleManager.Instance.scUIContadorAP.ResetearCirculos();


        int esfuerzo;
        if (yaVienedeCargando)
        {
            ActivarBoton(0);


        }
        else if (BotonActivo == false && HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
            ActivarBoton(esfuerzo);
        }
        else if (HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
             if(BattleManager.Instance.scTutorialCombate.tutorialCombateActivo)
           {
            if(BattleManager.Instance.scTutorialCombate.EstaAntesDelPasoAtaqueBallesta())
            {
                    return;
            }
           }
            DesactivarBoton();
        }
        else
        {
            BattleManager.Instance.unidadActiva?.GenerarTextoFlotante(ObtenerTextoPAInsuficientes(), Color.gray, FloatingTextContext.Resist);
            ReproducirRechazoInteraccion();
        }


    }


    public void UpdateCooldownMuestra()
    {
        if (HabilidadCooldownMuestra == null)
        {
            return;
        }

        bool mostrarMuestraCooldown = DebeMostrarMuestraCooldown();
        if (HabilidadCooldownMuestra.gameObject.activeSelf != mostrarMuestraCooldown)
        {
            HabilidadCooldownMuestra.gameObject.SetActive(mostrarMuestraCooldown);
        }

        if (!mostrarMuestraCooldown)
        {
            return;
        }

        if (HabilidadRepresentada != null)
        {

            if (HabilidadRepresentada.cooldownMax > 0 && HabilidadRepresentada.cooldownActual > 0)
            {
                float nMax = HabilidadRepresentada.cooldownMax;
                float nCurr = HabilidadRepresentada.cooldownActual;
                float fillRes = nCurr / nMax;


                HabilidadCooldownMuestra.fillAmount = fillRes;

            }
            else
            {
                HabilidadCooldownMuestra.fillAmount = 0;
            }



        }
    }

    private bool DebeMostrarMuestraCooldown()
    {
        if (!(HabilidadRepresentada is AcumulacionInestable))
        {
            return true;
        }

        return CampaignManager.Instance != null
            && CampaignManager.Instance.scAdministradorEscenas != null
            && CampaignManager.Instance.scAdministradorEscenas.escenaActual == 1;
    }

    void ActivarBoton(int iEsfuerzo)
    {

        int esfuerzo = iEsfuerzo;
        if (esfuerzo > 0)
        {
            TutorialTooltipManager.TryShow(TooltipEsforzarId);
        }

        scUiBotonesHabilidades.UIDesactivarHabilidades(HabilidadRepresentada.esHostil);
        BattleManager.Instance.LimpiarSeleccionHabilidadActual();
        HabilidadRepresentada.Activar();
        RuntimeAnalytics.TrackDesign("combat", "ability_selected", RuntimeAnalytics.AbilityToken(HabilidadRepresentada));
        VisualBotonActivo(1);
        // BattleManager.Instance.OpacarCasillasMelee();
        BotonActivo = true;
        BattleManager.Instance.TryMostrarPanelDescripcionHabilidad(HabilidadRepresentada, true);
        ReproducirConfirmacionInteraccion();
        if (BattleManager.Instance.unidadActiva.valorCargando > 0)
        {
            BattleManager.Instance.scUIContadorAP.MarcarCirculos(BattleManager.Instance.unidadActiva.valorCargando);
        }
        else
        {
            BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)HabilidadRepresentada.costoAP);
        }
        BattleManager.Instance.scUIContadorAP.SeEsforzaria(esfuerzo);



    }

    public void DesactivarBoton()
    {
        if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
        { return; } // Sale del método si la escena no es "ES-Batallas"

        int esfuerzo = 0;
        if (HabilidadRepresentada.tieneAPSuficientes(out esfuerzo) && HabilidadRepresentada.cooldownActual == 0)
        {
            VisualBotonActivo(0);
            BotonActivo = false;
            BattleManager.Instance.LimpiarCapasCasillas();
            BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
            BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
            BattleManager.Instance.SeleccionandoObjetivo = false;
            BattleManager.Instance.HabilidadActiva = null;
            BattleManager.Instance.OcultarPanelDescripcionHabilidad(HabilidadRepresentada, true);

            BattleManager.Instance.scUIContadorAP.ResetearCirculos();


        }
    }





    public void DesactivarHabilidad(bool omitirTilteo = false)
    {
        BattleManager.Instance.DesmarcarTodasLasUnidades();
        if (BotonActivo == true)
        { VisualBotonActivo(0, omitirTilteo); }
        BotonActivo = false;
        BattleManager.Instance?.OcultarPanelDescripcionHabilidad(HabilidadRepresentada, true);
    }

    public GameObject Oscurecedor;
    public GameObject Melee;

    private void Update()
    {
        if (HabilidadRepresentada != null)
        {
            nombreHabilidad.text = HabilidadRepresentada.nombre;

            if (turnosCooldown != null)
            {
                turnosCooldown.text = DebeMostrarMuestraCooldown() && HabilidadRepresentada.cooldownActual > 0
                    ? HabilidadRepresentada.cooldownActual.ToString()
                    : string.Empty;
            }

            // Actualiza el fill de cooldown en tiempo real para reflejar
            // inmediatamente los cambios al usar la habilidad o al cambiar de turno.
            UpdateCooldownMuestra();

            int cooldownActual = HabilidadRepresentada.cooldownActual;
            if (cooldownAnterior > 0 && cooldownActual <= 0)
            {
                ReproducirDisponibleInteraccion();
            }
            cooldownAnterior = cooldownActual;
        }
        else if (turnosCooldown != null)
        {
            turnosCooldown.text = string.Empty;
        }

        if (contenedorCirculosAccion != null && BattleManager.Instance != null && BattleManager.Instance.unidadActiva != null)
        {
            ActualizarVisualCirculosAccion();
        }

        // Mostrar/ocultar oscurecedor según si hay AP + esforzable suficientes
        if (Oscurecedor != null && HabilidadRepresentada != null && BattleManager.Instance != null && BattleManager.Instance.unidadActiva != null)
        {
            int apActualInt = Mathf.Max(0, (int)BattleManager.Instance.unidadActiva.ObtenerAPActual());
            if (costoMovimientoPreview >= 0)
            {
                apActualInt = Mathf.Max(0, apActualInt - costoMovimientoPreview);
            }
            int apNecesarios = Mathf.Max(0, (int)HabilidadRepresentada.costoAP);
            int disponible = apActualInt + HabilidadRepresentada.esforzable;
            bool bloqueoMelee = HabilidadRepresentada.esMelee && !PuedeUsarHabilidadMelee();
            bool sinAPParaEsforzarse = apActualInt < apNecesarios && apActualInt < 1;

            Oscurecedor.SetActive(disponible < apNecesarios || sinAPParaEsforzarse || bloqueoMelee);

            if (HabilidadRepresentada.esMelee)
            {
                Melee.SetActive(true);
            }
            else
            {
                Melee.SetActive(false);
            }
        }

    }


    private void VisualBotonActivo(int estado, bool omitirTilteo = false)
    {
        if (CampaignManager.Instance.gameObject.transform.parent.parent.GetComponent<AdministradorEscenas>().escenaActual != 1)
        { return; } // Sale del método si la escena no es "ES-Batallas"


        if (estado == 1)
        {
            if (BotonActivo == false)
            {
                if (HabilidadRepresentada.esHostil)
                {
                    BattleManager.Instance.TiltearCamaraLadoEnemigo(true);
                }
                TransicionarInteraccion(ObtenerEscalaInteraccion(true), ObtenerAlphaRealce(true), 0.1f);
            }

            if (seleccionada != null)
            {
                seleccionada.SetActive(true);
            }

        }
        else if (estado == 0)
        {

            if (BotonActivo == true)
            {
                BattleManager.Instance.DesmarcarTodasLasUnidades();
                if (HabilidadRepresentada.esHostil && !omitirTilteo)
                {
                    BattleManager.Instance.TiltearCamaraLadoEnemigo(false);
                }
                TransicionarInteraccion(ObtenerEscalaInteraccion(false), ObtenerAlphaRealce(false), 0.1f);
            }

            if (seleccionada != null)
            {
                seleccionada.SetActive(false);
            }
        }
    }

    public void MostrarDisponibilidadTrasMovimiento(int costoMovimiento)
    {
        costoMovimientoPreview = Mathf.Max(0, costoMovimiento);
    }

    public void LimpiarDisponibilidadTrasMovimiento()
    {
        costoMovimientoPreview = -1;
    }

    private void CrearRealceInteraccion()
    {
        if (imagenBoton == null || realceInteraccion != null)
        {
            return;
        }

        GameObject realce = new GameObject("RealceInteraccion", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        realce.layer = gameObject.layer;
        RectTransform rectRealce = realce.GetComponent<RectTransform>();
        rectRealce.SetParent(transform, false);
        rectRealce.anchorMin = Vector2.zero;
        rectRealce.anchorMax = Vector2.one;
        rectRealce.offsetMin = Vector2.zero;
        rectRealce.offsetMax = Vector2.zero;
        rectRealce.SetSiblingIndex(0);
        rectRealceInteraccion = rectRealce;

        realceInteraccion = realce.GetComponent<Image>();
        realceInteraccion.sprite = imagenBoton.sprite;
        realceInteraccion.type = imagenBoton.type;
        realceInteraccion.preserveAspect = imagenBoton.preserveAspect;
        realceInteraccion.raycastTarget = false;
        realceInteraccion.color = new Color(colorRealceNormal.r, colorRealceNormal.g, colorRealceNormal.b, 0f);
    }

    private Vector3 ObtenerEscalaInteraccion(bool seleccionado)
    {
        Vector3 escala = escalaBaseBoton + (seleccionado ? Vector3.one * 0.2f : Vector3.zero);
        // El agrandado por hover solo aplica en batalla; en campaña el botón no debe cambiar de tamaño al pasar el mouse.
        if (hoverDescripcionActiva && EstaEnBatallaActiva())
        {
            escala.x *= 1.035f;
            escala.y *= 1.035f;
        }
        return escala;
    }

    private float ObtenerAlphaRealce()
    {
        return ObtenerAlphaRealce(BotonActivo);
    }

    private float ObtenerAlphaRealce(bool seleccionado)
    {
        if (hoverDescripcionActiva)
        {
            return seleccionado ? 0.18f : 0.24f;
        }
        return seleccionado ? 0.08f : 0f;
    }

    private void TransicionarInteraccion(Vector3 escalaObjetivo, float alphaObjetivo, float duracion)
    {
        if (animacionInteraccionRoutine != null)
        {
            StopCoroutine(animacionInteraccionRoutine);
        }
        RestaurarRealceInteraccion();
        rectBoton.localRotation = rotacionBaseBoton;
        animacionInteraccionRoutine = StartCoroutine(TransicionInteraccion(escalaObjetivo, alphaObjetivo, duracion));
    }

    private IEnumerator TransicionInteraccion(Vector3 escalaObjetivo, float alphaObjetivo, float duracion)
    {
        yield return AnimarInteraccion(escalaObjetivo, alphaObjetivo, duracion);
        animacionInteraccionRoutine = null;
    }

    private IEnumerator AnimarInteraccion(Vector3 escalaObjetivo, float alphaObjetivo, float duracion)
    {
        Vector3 escalaInicial = rectBoton.localScale;
        float alphaInicial = realceInteraccion != null ? realceInteraccion.color.a : 0f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / duracion));
            rectBoton.localScale = Vector3.LerpUnclamped(escalaInicial, escalaObjetivo, t);
            AplicarAlphaRealce(Mathf.Lerp(alphaInicial, alphaObjetivo, t));
            yield return null;
        }

        rectBoton.localScale = escalaObjetivo;
        AplicarAlphaRealce(alphaObjetivo);
    }

    private void ReproducirConfirmacionInteraccion()
    {
        if (animacionInteraccionRoutine != null)
        {
            StopCoroutine(animacionInteraccionRoutine);
        }
        RestaurarRealceInteraccion();
        rectBoton.localRotation = rotacionBaseBoton;
        animacionInteraccionRoutine = StartCoroutine(ConfirmarInteraccion());
    }

    private IEnumerator ConfirmarInteraccion()
    {
        Vector3 escalaFinal = ObtenerEscalaInteraccion(true);
        yield return AnimarInteraccion(escalaFinal * 0.95f, 0.32f, 0.045f);
        yield return AnimarInteraccion(escalaFinal * 1.025f, 0.4f, 0.065f);
        yield return AnimarInteraccion(escalaFinal, ObtenerAlphaRealce(true), 0.09f);
        animacionInteraccionRoutine = null;
    }

    private void ReproducirRechazoInteraccion()
    {
        if (animacionInteraccionRoutine != null)
        {
            StopCoroutine(animacionInteraccionRoutine);
        }
        RestaurarRealceInteraccion();
        animacionInteraccionRoutine = StartCoroutine(RechazarInteraccion());
    }

    private IEnumerator RechazarInteraccion()
    {
        Vector3 escalaFinal = ObtenerEscalaInteraccion(BotonActivo);
        float alphaFinal = ObtenerAlphaRealce();
        const float duracion = 0.18f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float intensidad = 1f - t;
            float golpe = Mathf.Sin(t * Mathf.PI);
            float angulo = Mathf.Sin(t * Mathf.PI * 6f) * intensidad * 1.8f;

            rectBoton.localRotation = rotacionBaseBoton * Quaternion.Euler(0f, 0f, angulo);
            rectBoton.localScale = escalaFinal * (1f - golpe * 0.025f);
            if (rectRealceInteraccion != null)
            {
                rectRealceInteraccion.localScale = Vector3.one * (1f + golpe * 0.06f);
            }
            AplicarColorRealce(colorRealceRechazo, Mathf.Lerp(alphaFinal, 0.38f, golpe));
            yield return null;
        }

        rectBoton.localRotation = rotacionBaseBoton;
        rectBoton.localScale = escalaFinal;
        RestaurarRealceInteraccion();
        AplicarAlphaRealce(alphaFinal);
        animacionInteraccionRoutine = null;
    }

    private void ReproducirDisponibleInteraccion()
    {
        if (animacionInteraccionRoutine != null)
        {
            StopCoroutine(animacionInteraccionRoutine);
        }
        RestaurarRealceInteraccion();
        rectBoton.localRotation = rotacionBaseBoton;
        animacionInteraccionRoutine = StartCoroutine(MostrarDisponibleInteraccion());
    }

    private IEnumerator MostrarDisponibleInteraccion()
    {
        Vector3 escalaFinal = ObtenerEscalaInteraccion(BotonActivo);
        float alphaFinal = ObtenerAlphaRealce();
        const float duracion = 0.3f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float pulso = Mathf.Sin(t * Mathf.PI);

            rectBoton.localScale = escalaFinal * (1f + pulso * 0.035f);
            if (rectRealceInteraccion != null)
            {
                rectRealceInteraccion.localScale = Vector3.one * Mathf.Lerp(0.92f, 1.2f, t);
            }
            AplicarColorRealce(colorRealceDisponible, Mathf.Lerp(alphaFinal, 0.42f, pulso));
            yield return null;
        }

        rectBoton.localScale = escalaFinal;
        RestaurarRealceInteraccion();
        AplicarAlphaRealce(alphaFinal);
        animacionInteraccionRoutine = null;
    }

    private void RestaurarRealceInteraccion()
    {
        if (realceInteraccion == null)
        {
            return;
        }

        if (rectRealceInteraccion != null)
        {
            rectRealceInteraccion.localScale = Vector3.one;
        }
        AplicarColorRealce(colorRealceNormal, realceInteraccion.color.a);
    }

    private void AplicarColorRealce(Color color, float alpha)
    {
        if (realceInteraccion == null)
        {
            return;
        }

        color.a = alpha;
        realceInteraccion.color = color;
    }

    private void AplicarAlphaRealce(float alpha)
    {
        if (realceInteraccion == null)
        {
            return;
        }

        Color color = realceInteraccion.color;
        color.a = alpha;
        realceInteraccion.color = color;
    }

    bool ChequearCargaHabilidad()
    {
        Unidad uActiva = BattleManager.Instance.unidadActiva;

        if (uActiva.ObtenerAPActual() < HabilidadRepresentada.costoAP && HabilidadRepresentada.esCargable)
        {
            uActiva.estaCargando = HabilidadRepresentada;
            uActiva.valorCargando = (int)(HabilidadRepresentada.costoAP - uActiva.ObtenerAPActual());

            BattleManager.Instance.TerminarTurno();



            return true;
        }
        else
        {
            return false;
        }
    }

    string ObtenerTextoSinFervor()
    {
        if (TRADU.i == null)
        {
            return "Fervor insuficiente";
        }

        switch (TRADU.i.nIdioma)
        {
            case TRADU.IdiomaIngles:
                return "Insufficient Fervor";
            case TRADU.IdiomaPortugues:
                return "Fervor insuficiente";
            default:
                return "Fervor insuficiente";
        }
    }

    string ObtenerTextoRequiereEstarEscondido()
    {
        if (TRADU.i == null)
        {
            return "Requiere estar escondido";
        }

        switch (TRADU.i.nIdioma)
        {
            case TRADU.IdiomaIngles:
                return "Requires being hidden";
            case TRADU.IdiomaPortugues:
                return "Requer estar escondido";
            default:
                return "Requiere estar escondido";
        }
    }

    string ObtenerTextoPAInsuficientes()
    {
        if (TRADU.i == null)
        {
            return "PA insuficientes";
        }

        switch (TRADU.i.nIdioma)
        {
            case TRADU.IdiomaIngles:
                return "Insufficient AP";
            case TRADU.IdiomaPortugues:
                return "PA insuficientes";
            default:
                return "PA insuficientes";
        }
    }

    bool PuedeUsarHabilidadMelee()
    {
        if (HabilidadRepresentada == null || !HabilidadRepresentada.esMelee)
        {
            return true;
        }

        if (HabilidadRepresentada is CargaDeEstoque cargaDeEstoque)
        {
            return cargaDeEstoque.PuedeActivarseDesdePosicionActual(out _);
        }

        if (HabilidadRepresentada.nombre.Contains("Destruir Obstaculo"))
        {
            return true;

        }

        if (BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
        {
            return false;
        }

        Unidad unidad = BattleManager.Instance.unidadActiva;
        if (unidad.CasillaPosicion == null)
        {
            return false;
        }

        int posX = unidad.CasillaPosicion.posX;
        if (unidad.estado_inmovil > 0)
        {
            return posX == 3;
        }

        if (posX == 3)
        {
            return true;
        }

        if (posX == 2)
        {
            LadoManager lado = unidad.CasillaPosicion.ladoGO != null ? unidad.CasillaPosicion.ladoGO.GetComponent<LadoManager>() : null;
            if (lado != null)
            {
                Casilla casillaDelantera = lado.ObtenerCasillaPorIndex(3, unidad.CasillaPosicion.posY);
                if (casillaDelantera != null && casillaDelantera.Presente != null)
                {
                    Unidad aliado = casillaDelantera.Presente.GetComponent<Unidad>();
                    if (aliado != null && aliado.CasillaPosicion != null && aliado.CasillaPosicion.lado == unidad.CasillaPosicion.lado)
                    {
                        return true;
                    }

                    Obstaculo obstaculo = casillaDelantera.Presente.GetComponent<Obstaculo>();
                    if (obstaculo != null && obstaculo.bPermiteAtacarDetras)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    public MenuPersonajes scMenuPersonajes;
    public void SubirHabDeNivel(int n)
    {
        scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
        bool nivelado = false;
        if (n == 0) //0 - Subida Normal
        {
            scMenuPersonajes.pSel.NivelPuntoHabilidad--;
            HabilidadRepresentada.NIVEL++;
            scMenuPersonajes.ActualizarInfo();
            nivelado = true;
        }
        if (n == 4) //4 - Subida a 4a
        {
            scMenuPersonajes.pSel.NivelPuntoHabilidad--;
            HabilidadRepresentada.NIVEL = 4;
            scMenuPersonajes.ActualizarInfo();
            nivelado = true;
        }
        if (n == 5) //5 - Subida a 4b
        {
            scMenuPersonajes.pSel.NivelPuntoHabilidad--;
            HabilidadRepresentada.NIVEL = 5;
            scMenuPersonajes.ActualizarInfo();
            nivelado = true;
        }

        if (nivelado)
        {
            RuntimeAnalytics.TrackDesign(
                "characters",
                "skill_up",
                RuntimeAnalytics.AbilityToken(HabilidadRepresentada) + "_tier_" + HabilidadRepresentada.NIVEL);
        }
    }


    public void AgregarComoHabilidadNueva()
    {
        scMenuPersonajes = CampaignManager.Instance.scMenuPersonajes;
        // Verificamos si la habilidad representada no es nula
        if (HabilidadRepresentada != null)
        {
            // Obtenemos el tipo de la habilidad representada
            System.Type tipoHabilidad = HabilidadRepresentada.GetType();

            // Agregamos el componente del tipo de la habilidad al objeto scMenuPersonajes.pSel
            Habilidad nuevaHabilidad = (Habilidad)scMenuPersonajes.pSel.gameObject.AddComponent(tipoHabilidad);


            switch (nuevaHabilidad.IDenClase)
            {
                case 1: scMenuPersonajes.pSel.Habilidad_1 = 1; break;
                case 2: scMenuPersonajes.pSel.Habilidad_2 = 1; break;
                case 3: scMenuPersonajes.pSel.Habilidad_3 = 1; break;
                case 4: scMenuPersonajes.pSel.Habilidad_4 = 1; break;
                case 5: scMenuPersonajes.pSel.Habilidad_5 = 1; break;
                case 6: scMenuPersonajes.pSel.Habilidad_6 = 1; break;
                case 7: scMenuPersonajes.pSel.Habilidad_7 = 1; break;
                case 8: scMenuPersonajes.pSel.Habilidad_8 = 1; break;
                case 9: scMenuPersonajes.pSel.Habilidad_9 = 1; break;
                case 10: scMenuPersonajes.pSel.Habilidad_10 = 1; break;
            }
            nuevaHabilidad.NIVEL = 1;

            // Actualizamos la información y reducimos el nivel de la nueva habilidad base
            scMenuPersonajes.pSel.NivelNuevaHabilidadBase--;
            scMenuPersonajes.yaTiroHabRand = false;
            scMenuPersonajes.ActualizarInfo();
            scMenuPersonajes.LimpiarComponentesHab();
            RuntimeAnalytics.TrackDesign("characters", "new_skill", RuntimeAnalytics.AbilityToken(nuevaHabilidad));
        }
    }


    public void AsignarHotkey()
    {
        Invoke("ActualizarkotkeyDelay", 0.1f);
    }

    public void ActualizarkotkeyDelay()
    {
          if (HabilidadRepresentada.GetType().Name.Contains("REPRESENTACION"))
        { txtHotkey.text = "";  return; } //Si se clickea el boton de una pasiva, no pasa nada

        if (txtHotkey == null) return;
        int pos = transform.GetSiblingIndex();
        pos++; // Convertir de índice base 0 a base 1
        txtHotkey.text = pos.ToString();

    }
    
}



