using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controla la pantalla que aparece antes de iniciar una campaña nueva.
/// Mantiene aquí la selección de zona y la lectura de la metaprogresión
/// para que el menú principal no tenga que conocer los detalles del save.
/// </summary>
public class PrePartidaManager : MonoBehaviour
{
    public const int ZonaDesconocida = 0;
    public const int ZonaBosqueArdiente = 1;
    public const int ZonaPasoVientoHelado = 2;
    public const int ZonaNedukazal = 3;
    public const int ClaseAleatoria = 0;
    public const int ClaseCaballero = 1;
    public const int ClaseDuelista = 6;

    private static readonly int[] zonasCarrusel =
    {
        ZonaBosqueArdiente,
        ZonaPasoVientoHelado,
        ZonaNedukazal,
        ZonaDesconocida
    };
    private static int zonaInicialPendiente;
    private static int claseLiderPendiente = ClaseAleatoria;
    private static readonly List<int> presagiosInicialesPendientes = new List<int>();
    private static bool hayPresagiosInicialesPendientes;
    private static readonly string[] nombresClases =
    {
        "Caballero",
        "Explorador",
        "Purificadora",
        "Acechador",
        "Canalizador",
        "Duelista"
    };

    [Serializable]
    public class ZonaOpcion
    {
        public int id = ZonaBosqueArdiente;
        public string nombre = "Bosque Ardiente";
        [TextArea(2, 5)] public string descripcion;
        public bool disponible = true;
        public Button boton;
        public GameObject indicadorSeleccion;
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelPrePartida;
    [SerializeField] private MenuController menuController;
    [SerializeField] private bool ocultarAlIniciar = true;

    [Header("Elementos del menú principal")]
    [SerializeField] private List<GameObject> elementosMenuPrincipalAOcultar = new List<GameObject>();

    [Header("Regiones disponibles")]
    [SerializeField] private List<ZonaOpcion> zonas = new List<ZonaOpcion>
    {
        new ZonaOpcion
        {
            id = ZonaBosqueArdiente,
            nombre = "Bosque Ardiente",
            descripcion = "Una región calcinada y peligrosa. La demo comienza aquí.",
            disponible = true
        }
    };

    [Header("Región seleccionada")]
    [SerializeField] private TMP_Text textoZonaSeleccionada;
    [SerializeField] private TMP_Text textoDescripcionZona;
    [SerializeField] private GameObject infoZona;
    [SerializeField] private GameObject fondoInfoZona;
    [SerializeField] private TMP_Text textoInfoZona;

    [Header("Metaprogresión visible")]
    [SerializeField] private TMP_Text textoCorrupcion;
    [SerializeField] private TMP_Text textoMisionesSalvamento;
    [SerializeField] private Image esferaCorr;
    [SerializeField, Min(1)] private int corrupcionMaxima = 5;
    [SerializeField] private Color colorCorrupcionMinima = new Color(0.35f, 0.65f, 1f, 1f);
    [SerializeField] private Color colorCorrupcionMaxima = Color.red;

    [Header("Lider de caravana")]
    [SerializeField] private TMP_Dropdown dropdownLider;

    [Header("Confirmacion sobrescritura")]
    [SerializeField] private GameObject confirmarSobreescribir;
    [SerializeField] private TMP_Text textoConfirmarSobreescribir;
    [SerializeField] private Button botonCancelarSobreescribir;
    [SerializeField] private Button botonContinuarSobreescribir;

    [Header("Presagios")]
    [SerializeField] private TMP_Text txtPresagio1;
    [SerializeField] private TMP_Text txtPresagio2;
    [SerializeField] private TMP_Text disclaimerDemo;
    [SerializeField] private UIManagerContZonas uiClimasZona;
    [SerializeField] private GameObject flechaIzq;
    [SerializeField] private GameObject flechaDer;
    [SerializeField] private GameObject btnNuevaZona;
    [SerializeField] private Color colorDisclaimerNoDisponible = new Color(0.75f, 0.08f, 0.08f, 1f);
    [SerializeField] private Color colorDisclaimerDesconocida = new Color(0.55f, 0.2f, 0.8f, 1f);
    [SerializeField] private Color colorNombrePasoVientoHelado = new Color(0.37f, 0.62f, 0.7f, 1f);
    [SerializeField] private Color colorNombreNedukazal = new Color(0.54f, 0.19f, 0.19f, 1f);
    public AudioClip audioAliento;
    private int zonaSeleccionada;
    private int indiceCarrusel;
    private MetaprogresionSaveData metaprogresionActual = new MetaprogresionSaveData();
    private readonly Dictionary<GameObject, bool> estadosMenuPrincipal = new Dictionary<GameObject, bool>();
    private bool menuPrincipalOculto;
    private bool flechaIzquierdaConfigurada;
    private bool flechaDerechaConfigurada;
    private bool infoZonaConfigurada;
    private bool mostrandoInfoZona;
    private bool confirmacionSobreescribirConfigurada;
    private string nombrePartidaSobreescribir = string.Empty;
    private Color colorNombreZonaBase = Color.white;
    private bool colorNombreZonaBaseCapturado;

    public int ZonaSeleccionada => zonaSeleccionada;
    public MetaprogresionSaveData MetaprogresionActual => metaprogresionActual;

    private void Awake()
    {
        if (panelPrePartida == null)
        {
            panelPrePartida = gameObject;
        }

        if (menuController == null)
        {
            menuController = FindFirstObjectByType<MenuController>();
        }

        BuscarDropdownLider();
        BuscarTextosPresagios();
        BuscarControlesCarrusel();
        BuscarUIClimasZona();
        ConfigurarInfoZona();
        OcultarInfoZona();
        ConfigurarConfirmacionSobreescribir();
        OcultarConfirmacionSobreescribir();
        ConfigurarFlechasCarrusel();
        ActualizarDropdownLider();

        // Si el manager vive dentro del propio panel, su Awake puede ejecutarse
        // justo al abrirlo por primera vez. No volver a apagarlo en ese caso.
        if (ocultarAlIniciar && panelPrePartida != null && panelPrePartida != gameObject)
        {
            panelPrePartida.SetActive(false);
        }

        ConfigurarBotonesZona();
        SeleccionarPrimeraZonaDisponible();
    }

    private void OnEnable()
    {
        TRADU.IdiomaActualizado -= AlActualizarIdioma;
        TRADU.IdiomaActualizado += AlActualizarIdioma;
        ConfigurarInfoZona();
        OcultarInfoZona();
        ConfigurarConfirmacionSobreescribir();
        OcultarConfirmacionSobreescribir();
        Refrescar();
    }

    private void OnDisable()
    {
        TRADU.IdiomaActualizado -= AlActualizarIdioma;
        OcultarInfoZona();
        OcultarConfirmacionSobreescribir();
    }

    private void Update()
    {
        if (ConfirmacionSobreescribirActiva() && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelarSobreescritura();
            return;
        }

        if (menuPrincipalOculto && Input.GetKeyDown(KeyCode.Escape))
        {
            Cerrar();
        }
    }

    /// <summary>
    /// Abre la pantalla y la deja lista para una nueva partida.
    /// </summary>
    public void Abrir()
    {
        OcultarMenuPrincipal();
        SeleccionarPrimeraZonaDisponible();
        OcultarConfirmacionSobreescribir();

        if (panelPrePartida != null && !panelPrePartida.activeSelf)
        {
            panelPrePartida.SetActive(true);
        }

        Refrescar();
    }

    public void Cerrar()
    {
        OcultarInfoZona();
        OcultarConfirmacionSobreescribir();
        RestaurarMenuPrincipal();

        if (panelPrePartida != null)
        {
            panelPrePartida.SetActive(false);
        }
    }

    /// <summary>
    /// Método pensado para enlazar los botones de cada zona desde el Inspector.
    /// </summary>
    public void SeleccionarZona(int id)
    {
        for (int i = 0; i < zonasCarrusel.Length; i++)
        {
            if (zonasCarrusel[i] == id)
            {
                indiceCarrusel = i;
                zonaSeleccionada = id;
                ActualizarZonaSeleccionada();
                return;
            }
        }

        Debug.LogWarning("[PrePartidaManager] La región no pertenece al carrusel: " + id, this);
    }

    public void GirarCarruselIzquierda()
    {
        GirarCarrusel(-1);
    }

    public void GirarCarruselDerecha()
    {
        GirarCarrusel(1);
    }

    public void SeleccionarBosqueArdiente()
    {
        SeleccionarZona(ZonaBosqueArdiente);
    }

    /// <summary>
    /// Confirma la zona y devuelve el control al MenuController para cargar campaña.
    /// </summary>
    public void ConfirmarNuevaPartida()
    {
        if (!ZonaDisponibleEnDemoODebug(zonaSeleccionada))
        {
            Debug.LogWarning("[PrePartidaManager] La región seleccionada no está disponible en la demo.", this);
            return;
        }

        if (TryObtenerNombrePartidaGuardada(out string nombrePartida))
        {
            if (MostrarConfirmacionSobreescribir(nombrePartida))
            {
                return;
            }

            Debug.LogWarning("[PrePartidaManager] Hay una partida guardada, pero no se encontro el panel ConfirmarSobreescribir.", this);
            return;
        }

        IniciarNuevaPartidaConfirmada();
    }

    public void CancelarSobreescritura()
    {
        OcultarConfirmacionSobreescribir();
    }

    public void ContinuarSobreescritura()
    {
        OcultarConfirmacionSobreescribir();
        IniciarNuevaPartidaConfirmada();
    }

    private void IniciarNuevaPartidaConfirmada()
    {
        EstablecerZonaInicialPendiente(zonaSeleccionada);
        EstablecerClaseLiderPendiente(ObtenerClaseLiderSeleccionada());

        if (menuController == null)
        {
            menuController = FindFirstObjectByType<MenuController>();
        }

        if (menuController == null)
        {
            Debug.LogError("[PrePartidaManager] No se encontró un MenuController para iniciar la campaña.", this);
            return;
        }

        EstablecerPresagiosInicialesPendientes(PresagioRegionPendienteStore.Consumir(zonaSeleccionada));
        ReproducirAudioAlientoInicioPartida();
        menuController.IniciarNuevaPartidaDesdePrePartida(zonaSeleccionada);
    }

    private void ReproducirAudioAlientoInicioPartida()
    {
        if (audioAliento == null)
        {
            return;
        }

        GameObject audioTemporal = new GameObject("AudioAlientoInicioPartida");
        DontDestroyOnLoad(audioTemporal);

        AudioSource source = audioTemporal.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.clip = audioAliento;
        AjustesAudio.AplicarVolumenSfx(source);
        source.Play();
        Destroy(audioTemporal, audioAliento.length + 0.1f);
    }

    private bool TryObtenerNombrePartidaGuardada(out string nombrePartida)
    {
        nombrePartida = string.Empty;
        if (!SaveGameService.TryReadSaveFile(out SaveFileData save, out _)
            || save == null)
        {
            return false;
        }

        save.AsegurarDisplayName();
        nombrePartida = !string.IsNullOrWhiteSpace(save.displayName)
            ? save.displayName
            : ObtenerTextoPartidaGuardadaSinNombre();
        return true;
    }

    private bool MostrarConfirmacionSobreescribir(string nombrePartida)
    {
        ConfigurarConfirmacionSobreescribir();
        if (confirmarSobreescribir == null)
        {
            return false;
        }

        nombrePartidaSobreescribir = !string.IsNullOrWhiteSpace(nombrePartida)
            ? nombrePartida
            : ObtenerTextoPartidaGuardadaSinNombre();
        ActualizarTextosConfirmacionSobreescribir();
        confirmarSobreescribir.SetActive(true);
        return true;
    }

    private void OcultarConfirmacionSobreescribir()
    {
        if (confirmarSobreescribir != null)
        {
            confirmarSobreescribir.SetActive(false);
        }
    }

    private bool ConfirmacionSobreescribirActiva()
    {
        return confirmarSobreescribir != null && confirmarSobreescribir.activeInHierarchy;
    }

    private void ConfigurarConfirmacionSobreescribir()
    {
        BuscarConfirmacionSobreescribir();
        if (confirmarSobreescribir == null)
        {
            return;
        }

        if (!confirmacionSobreescribirConfigurada)
        {
            if (botonCancelarSobreescribir != null)
            {
                botonCancelarSobreescribir.onClick.AddListener(CancelarSobreescritura);
            }

            if (botonContinuarSobreescribir != null)
            {
                botonContinuarSobreescribir.onClick.AddListener(ContinuarSobreescritura);
            }

            confirmacionSobreescribirConfigurada = true;
        }

        ActualizarTextosConfirmacionSobreescribir();
    }

    private void BuscarConfirmacionSobreescribir()
    {
        if (confirmarSobreescribir == null)
        {
            Transform raiz = BuscarDescendientePorNombre(transform, "ConfirmarSobreescribir");
            confirmarSobreescribir = raiz != null ? raiz.gameObject : null;
        }

        if (confirmarSobreescribir == null)
        {
            return;
        }

        if (botonCancelarSobreescribir == null || botonContinuarSobreescribir == null)
        {
            Button[] botones = confirmarSobreescribir.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < botones.Length; i++)
            {
                Button boton = botones[i];
                if (boton == null)
                {
                    continue;
                }

                string nombre = boton.gameObject.name;
                if (botonCancelarSobreescribir == null
                    && string.Equals(nombre, "bt_no", StringComparison.OrdinalIgnoreCase))
                {
                    botonCancelarSobreescribir = boton;
                }
                else if (botonContinuarSobreescribir == null
                    && string.Equals(nombre, "bt_si", StringComparison.OrdinalIgnoreCase))
                {
                    botonContinuarSobreescribir = boton;
                }
            }
        }

        if (textoConfirmarSobreescribir == null)
        {
            textoConfirmarSobreescribir = BuscarTextoPrincipalConfirmacion(confirmarSobreescribir.transform);
        }
    }

    private static Transform BuscarDescendientePorNombre(Transform raiz, string nombre)
    {
        if (raiz == null)
        {
            return null;
        }

        Transform[] descendientes = raiz.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < descendientes.Length; i++)
        {
            Transform candidato = descendientes[i];
            if (candidato != null && string.Equals(candidato.gameObject.name, nombre, StringComparison.OrdinalIgnoreCase))
            {
                return candidato;
            }
        }

        return null;
    }

    private static TMP_Text BuscarTextoPrincipalConfirmacion(Transform raiz)
    {
        if (raiz == null)
        {
            return null;
        }

        TMP_Text[] textos = raiz.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            TMP_Text texto = textos[i];
            if (texto != null && !EstaDentroDeBoton(texto.transform, raiz))
            {
                return texto;
            }
        }

        return textos.Length > 0 ? textos[0] : null;
    }

    private static bool EstaDentroDeBoton(Transform transform, Transform raiz)
    {
        Transform actual = transform;
        while (actual != null && actual != raiz)
        {
            if (actual.GetComponent<Button>() != null)
            {
                return true;
            }

            actual = actual.parent;
        }

        return false;
    }

    private void ActualizarTextosConfirmacionSobreescribir()
    {
        int idioma = ObtenerIdiomaActual();
        if (textoConfirmarSobreescribir != null)
        {
            textoConfirmarSobreescribir.text = ObtenerMensajeConfirmacionSobreescribir(
                nombrePartidaSobreescribir,
                idioma);
        }

        AsignarTextoBoton(botonCancelarSobreescribir, ObtenerTextoCancelarConfirmacion(idioma));
        AsignarTextoBoton(botonContinuarSobreescribir, ObtenerTextoContinuarConfirmacion(idioma));
    }

    private static void AsignarTextoBoton(Button boton, string texto)
    {
        if (boton == null)
        {
            return;
        }

        TMP_Text label = boton.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = texto;
        }
    }

    private static string ObtenerMensajeConfirmacionSobreescribir(string nombrePartida, int idioma)
    {
        string nombre = !string.IsNullOrWhiteSpace(nombrePartida)
            ? nombrePartida
            : ObtenerTextoPartidaGuardadaSinNombre();

        return idioma switch
        {
            TRADU.IdiomaIngles => "If you start a new game, you will overwrite the save: \"" + nombre + "\".\n\nContinue?",
            TRADU.IdiomaPortugues => "Se você começar uma nova partida, sobrescreverá a partida: \"" + nombre + "\".\n\nContinuar?",
            _ => "Si empiezas una nueva partida, sobreescribirás la partida: \"" + nombre + "\".\n\n¿Continuar?"
        };
    }

    private static string ObtenerTextoCancelarConfirmacion(int idioma)
    {
        return idioma == TRADU.IdiomaIngles ? "Cancel" : "Cancelar";
    }

    private static string ObtenerTextoContinuarConfirmacion(int idioma)
    {
        return idioma == TRADU.IdiomaIngles ? "Continue" : "Continuar";
    }

    private static string ObtenerTextoPartidaGuardadaSinNombre()
    {
        int idioma = ObtenerIdiomaActual();
        return idioma switch
        {
            TRADU.IdiomaIngles => "Saved Game",
            TRADU.IdiomaPortugues => "Partida salva",
            _ => "Partida guardada"
        };
    }

    public void Refrescar()
    {
        ActualizarDropdownLider();
        BuscarTextosPresagios();
        BuscarControlesCarrusel();
        BuscarUIClimasZona();
        ConfigurarInfoZona();
        ConfigurarConfirmacionSobreescribir();
        ConfigurarFlechasCarrusel();
        CargarMetaprogresion();
        ActualizarTextoMetaprogresion();
        ActualizarZonaSeleccionada();
    }

    public void DebugSumarCorrupcion()
    {
        CambiarCorrupcionDebug(1);
    }

    public void DebugRestarCorrupcion()
    {
        CambiarCorrupcionDebug(-1);
    }

#if UNITY_EDITOR
    public int DebugObtenerZonaSeleccionada()
    {
        return zonaSeleccionada > 0 ? zonaSeleccionada : ZonaBosqueArdiente;
    }

    public void DebugForzarPresagios(int presagio1Id, int presagio2Id)
    {
        List<int> ids = new List<int>();
        if (presagio1Id > 0)
        {
            ids.Add(presagio1Id);
        }

        if (presagio2Id > 0 && presagio2Id != presagio1Id)
        {
            ids.Add(presagio2Id);
        }

        PresagioRegionPendienteStore.ForzarParaDebug(DebugObtenerZonaSeleccionada(), ids);
        ActualizarPresagiosZonaSeleccionada();
    }

    public void DebugSortearNuevamentePresagios()
    {
        int regionId = DebugObtenerZonaSeleccionada();
        PresagioRegionPendienteStore.ForzarParaDebug(
            regionId,
            PresagioCatalog.SortearParaRegion(regionId));
        ActualizarPresagiosZonaSeleccionada();
    }
#endif

    public static void EstablecerZonaInicialPendiente(int zonaId)
    {
        zonaInicialPendiente = zonaId > 0 ? zonaId : 0;
    }

    public static void LimpiarZonaInicialPendiente()
    {
        zonaInicialPendiente = 0;
    }

    public static int ConsumirZonaInicialPendiente()
    {
        int zona = zonaInicialPendiente;
        zonaInicialPendiente = 0;
        return zona;
    }

    public static void EstablecerClaseLiderPendiente(int claseId)
    {
        claseLiderPendiente = Mathf.Clamp(claseId, ClaseAleatoria, ClaseDuelista);
    }

    public static void LimpiarClaseLiderPendiente()
    {
        claseLiderPendiente = 0;
    }

    public static int ConsumirClaseLiderPendiente()
    {
        int claseId = claseLiderPendiente;
        claseLiderPendiente = 0;
        return claseId;
    }

    public static void LimpiarPresagiosInicialesPendientes()
    {
        presagiosInicialesPendientes.Clear();
        hayPresagiosInicialesPendientes = false;
    }

    public static bool TryConsumirPresagiosInicialesPendientes(out List<int> presagios)
    {
        presagios = new List<int>();
        if (!hayPresagiosInicialesPendientes)
        {
            return false;
        }

        presagios.AddRange(presagiosInicialesPendientes);
        LimpiarPresagiosInicialesPendientes();
        return true;
    }

    private static void EstablecerPresagiosInicialesPendientes(List<int> presagios)
    {
        presagiosInicialesPendientes.Clear();
        if (presagios != null)
        {
            presagiosInicialesPendientes.AddRange(presagios);
        }

        hayPresagiosInicialesPendientes = true;
    }

    private void SeleccionarPrimeraZonaDisponible()
    {
        indiceCarrusel = 0;
        zonaSeleccionada = ZonaBosqueArdiente;
    }

    private void GirarCarrusel(int direccion)
    {
        indiceCarrusel = (indiceCarrusel + direccion + zonasCarrusel.Length) % zonasCarrusel.Length;
        zonaSeleccionada = zonasCarrusel[indiceCarrusel];
        ActualizarZonaSeleccionada();
    }

    private void OcultarMenuPrincipal()
    {
        if (menuPrincipalOculto)
        {
            return;
        }

        estadosMenuPrincipal.Clear();
        OcultarObjeto(elementosMenuPrincipalAOcultar);

        if (menuController != null)
        {
            OcultarObjeto(menuController.logoEspaniol);
            OcultarObjeto(menuController.logoIngles);
            OcultarObjeto(menuController.logoPortugues);
        }

        menuPrincipalOculto = true;
    }

    private void OcultarObjeto(IEnumerable<GameObject> objetos)
    {
        if (objetos == null)
        {
            return;
        }

        foreach (GameObject objeto in objetos)
        {
            OcultarObjeto(objeto);
        }
    }

    private void OcultarObjeto(GameObject objeto)
    {
        if (objeto == null || estadosMenuPrincipal.ContainsKey(objeto))
        {
            return;
        }

        estadosMenuPrincipal.Add(objeto, objeto.activeSelf);
        objeto.SetActive(false);
    }

    private void RestaurarMenuPrincipal()
    {
        if (!menuPrincipalOculto)
        {
            return;
        }

        foreach (KeyValuePair<GameObject, bool> estado in estadosMenuPrincipal)
        {
            if (estado.Key != null)
            {
                estado.Key.SetActive(estado.Value);
            }
        }

        estadosMenuPrincipal.Clear();
        menuPrincipalOculto = false;
    }

    private void CargarMetaprogresion()
    {
        if (SaveGameService.TryReadSaveFile(out SaveFileData save, out _)
            && save != null
            && save.metaprogresion != null)
        {
            PresagioRegionPendienteStore.ImportarSiNoHayEstadoGlobal(save.metaprogresion.presagiosRegionesPendientes);
            metaprogresionActual = save.metaprogresion;
            NormalizarMetaprogresion();
            RegistrarZonasVisitadasInferidasDesdeSave(save);
            SincronizarZonasVisitadasConManager();
            return;
        }

        MetaprogresionManager meta = MetaprogresionManager.Instance;
        if (meta == null)
        {
            metaprogresionActual = new MetaprogresionSaveData();
            metaprogresionActual.misionesSalvamento = 1;
            NormalizarMetaprogresion();
            return;
        }

        metaprogresionActual = new MetaprogresionSaveData
        {
            corrupcionGlobal = meta.CorrupcionGlobal,
            cantidadCiviles = meta.CantidadCiviles,
            valorTrabajoDisponible = meta.ValordeTrabajoDisponible,
            misionesSalvamento = meta.MisionesSalvamento,
            nivelAlertaBosqueArdiente = meta.NivelAlertaBosqueArdiente,
            nivelAlertaPasoVientohelado = meta.NivelAlertaPasoVientohelado,
            nivelAlertaNedukazal = meta.NivelAlertaNedukazal,
            zonasVisitadas = meta.ObtenerZonasVisitadas(),
            climasExclusivosDescubiertos = meta.ObtenerClimasExclusivosDescubiertos()
        };
        NormalizarMetaprogresion();
    }

    private void NormalizarMetaprogresion()
    {
        metaprogresionActual.corrupcionGlobal = Mathf.Max(0, metaprogresionActual.corrupcionGlobal);
        metaprogresionActual.cantidadCiviles = Mathf.Max(0, metaprogresionActual.cantidadCiviles);
        metaprogresionActual.valorTrabajoDisponible = Mathf.Max(0, metaprogresionActual.valorTrabajoDisponible);
        metaprogresionActual.misionesSalvamento = metaprogresionActual.misionesSalvamento < 0
            ? 1
            : metaprogresionActual.misionesSalvamento;
        metaprogresionActual.nivelAlertaBosqueArdiente = Mathf.Max(0, metaprogresionActual.nivelAlertaBosqueArdiente);
        metaprogresionActual.nivelAlertaPasoVientohelado = Mathf.Max(0, metaprogresionActual.nivelAlertaPasoVientohelado);
        metaprogresionActual.nivelAlertaNedukazal = Mathf.Max(0, metaprogresionActual.nivelAlertaNedukazal);
        NormalizarZonasVisitadas();
        NormalizarClimasExclusivosDescubiertos();
    }

    private void NormalizarZonasVisitadas()
    {
        if (metaprogresionActual.zonasVisitadas == null)
        {
            metaprogresionActual.zonasVisitadas = new List<int>();
            return;
        }

        List<int> normalizadas = new List<int>();
        for (int i = 0; i < metaprogresionActual.zonasVisitadas.Count; i++)
        {
            int zonaId = metaprogresionActual.zonasVisitadas[i];
            if (zonaId <= 0 || normalizadas.Contains(zonaId))
            {
                continue;
            }

            normalizadas.Add(zonaId);
        }

        metaprogresionActual.zonasVisitadas = normalizadas;
    }

    private void NormalizarClimasExclusivosDescubiertos()
    {
        if (metaprogresionActual.climasExclusivosDescubiertos == null)
        {
            metaprogresionActual.climasExclusivosDescubiertos = new List<int>();
            return;
        }

        List<int> normalizados = new List<int>();
        for (int i = 0; i < metaprogresionActual.climasExclusivosDescubiertos.Count; i++)
        {
            int tipoClima = metaprogresionActual.climasExclusivosDescubiertos[i];
            if (!ClimaZonaCatalog.EsClimaExclusivoRegion(tipoClima)
                || normalizados.Contains(tipoClima))
            {
                continue;
            }

            normalizados.Add(tipoClima);
        }

        metaprogresionActual.climasExclusivosDescubiertos = normalizados;
    }

    private void RegistrarZonasVisitadasInferidasDesdeSave(SaveFileData save)
    {
        if (save == null || save.campaign == null)
        {
            return;
        }

        AgregarZonaVisitadaAMetaprogresionActual(save.campaign.zonaId);
        if (save.campaign.zonasEstado == null)
        {
            return;
        }

        for (int i = 0; i < save.campaign.zonasEstado.Count; i++)
        {
            if (save.campaign.zonasEstado[i] == 1)
            {
                AgregarZonaVisitadaAMetaprogresionActual(i + 1);
            }
        }
    }

    private void AgregarZonaVisitadaAMetaprogresionActual(int zonaId)
    {
        if (zonaId <= 0)
        {
            return;
        }

        if (metaprogresionActual.zonasVisitadas == null)
        {
            metaprogresionActual.zonasVisitadas = new List<int>();
        }

        if (!metaprogresionActual.zonasVisitadas.Contains(zonaId))
        {
            metaprogresionActual.zonasVisitadas.Add(zonaId);
        }
    }

    private bool ZonaMecanicasConocidas(int zonaId)
    {
        return zonaId > 0
            && metaprogresionActual.zonasVisitadas != null
            && metaprogresionActual.zonasVisitadas.Contains(zonaId);
    }

    private void SincronizarZonasVisitadasConManager()
    {
        if (MetaprogresionManager.Instance != null)
        {
            MetaprogresionManager.Instance.RestaurarZonasVisitadas(metaprogresionActual.zonasVisitadas);
            MetaprogresionManager.Instance.RestaurarClimasExclusivosDescubiertos(metaprogresionActual.climasExclusivosDescubiertos);
        }
    }

    private void ActualizarTextoMetaprogresion()
    {
        AsignarTexto(textoCorrupcion, metaprogresionActual.corrupcionGlobal);
        AsignarTexto(textoMisionesSalvamento, metaprogresionActual.misionesSalvamento);
        ActualizarColorEsferaCorrupcion();
    }

    private void CambiarCorrupcionDebug(int cantidad)
    {
        metaprogresionActual.corrupcionGlobal = Mathf.Clamp(
            metaprogresionActual.corrupcionGlobal + cantidad,
            0,
            ObtenerCorrupcionMaxima());

        ActualizarTextoMetaprogresion();
    }

    private void ActualizarColorEsferaCorrupcion()
    {
        if (esferaCorr == null)
        {
            return;
        }

        float progreso = Mathf.Clamp01(
            (float)metaprogresionActual.corrupcionGlobal / ObtenerCorrupcionMaxima());
        esferaCorr.color = Color.Lerp(colorCorrupcionMinima, colorCorrupcionMaxima, progreso);
    }

    private int ObtenerCorrupcionMaxima()
    {
        if (MetaprogresionManager.Instance != null)
        {
            return Mathf.Max(1, MetaprogresionManager.Instance.CorrupcionMax);
        }

        return Mathf.Max(1, corrupcionMaxima);
    }

    private void ActualizarZonaSeleccionada()
    {
        if (zonas != null)
        {
            for (int i = 0; i < zonas.Count; i++)
            {
                ZonaOpcion zona = zonas[i];
                if (zona == null)
                {
                    continue;
                }

                if (zona.boton != null)
                {
                    zona.boton.interactable = zona.disponible;
                }

                if (zona.indicadorSeleccion != null)
                {
                    zona.indicadorSeleccion.SetActive(zona.id == zonaSeleccionada);
                }
            }
        }

        AsignarTexto(textoZonaSeleccionada, ObtenerNombreZonaCarrusel(zonaSeleccionada));
        AsignarTexto(textoDescripcionZona, ObtenerDescripcionZonaCarrusel(zonaSeleccionada));
        ActualizarColorNombreZona();
        ActualizarDisclaimerDemo();
        ActualizarBotonNuevaZona();
        ActualizarClimasZonaSeleccionada();
        if (mostrandoInfoZona && !ActualizarTextoInfoZona())
        {
            OcultarInfoZona();
        }

        if (zonaSeleccionada != ZonaBosqueArdiente)
        {
            AsignarTexto(txtPresagio1, string.Empty);
            AsignarTexto(txtPresagio2, string.Empty);
            return;
        }

        ActualizarPresagiosZonaSeleccionada();
    }

    private void ActualizarPresagiosZonaSeleccionada()
    {
        List<int> presagios = PresagioRegionPendienteStore.ObtenerOCrear(zonaSeleccionada);
        List<string> textos = new List<string>();
        for (int i = 0; i < presagios.Count; i++)
        {
            string texto = PresagioCatalog.ObtenerTextoLocalizado(presagios[i]);
            if (!string.IsNullOrWhiteSpace(texto))
            {
                textos.Add(texto);
            }
        }

        if (textos.Count == 0)
        {
            AsignarTexto(txtPresagio1, PresagioCatalog.ObtenerTextoSinPresagios());
            AsignarTexto(txtPresagio2, string.Empty);
            return;
        }

        AsignarTexto(txtPresagio1, textos[0]);
        AsignarTexto(txtPresagio2, textos.Count > 1 ? textos[1] : string.Empty);
    }

    private void ConfigurarBotonesZona()
    {
        if (zonas == null)
        {
            return;
        }

        for (int i = 0; i < zonas.Count; i++)
        {
            ZonaOpcion zona = zonas[i];
            if (zona == null || zona.boton == null)
            {
                continue;
            }

            int idZona = zona.id;
            zona.boton.onClick.AddListener(() => SeleccionarZona(idZona));
        }
    }

    private void ConfigurarInfoZona()
    {
        BuscarInfoZona();

        if (infoZona == null || infoZonaConfigurada)
        {
            return;
        }

        EventTrigger trigger = infoZona.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = infoZona.AddComponent<EventTrigger>();
        }

        if (trigger.triggers == null)
        {
            trigger.triggers = new List<EventTrigger.Entry>();
        }

        AgregarEventoInfoZona(trigger, EventTriggerType.PointerEnter, _ => MostrarInfoZona());
        AgregarEventoInfoZona(trigger, EventTriggerType.PointerExit, _ => OcultarInfoZona());
        infoZonaConfigurada = true;
    }

    private void BuscarInfoZona()
    {
        if (infoZona != null && fondoInfoZona != null && textoInfoZona != null)
        {
            return;
        }

        Transform[] objetos = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < objetos.Length; i++)
        {
            Transform candidato = objetos[i];
            if (candidato == null)
            {
                continue;
            }

            string nombre = candidato.gameObject.name;
            if (infoZona == null && string.Equals(nombre, "infoZona", StringComparison.OrdinalIgnoreCase))
            {
                infoZona = candidato.gameObject;
            }
            else if (fondoInfoZona == null && string.Equals(nombre, "FondoInfoZona", StringComparison.OrdinalIgnoreCase))
            {
                fondoInfoZona = candidato.gameObject;
            }

            if (textoInfoZona == null && string.Equals(nombre, "txtDescZona (1)", StringComparison.OrdinalIgnoreCase))
            {
                textoInfoZona = candidato.GetComponent<TMP_Text>();
            }
        }

        if (infoZona != null && textoInfoZona == null)
        {
            textoInfoZona = infoZona.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private static void AgregarEventoInfoZona(
        EventTrigger trigger,
        EventTriggerType tipo,
        UnityEngine.Events.UnityAction<BaseEventData> accion)
    {
        EventTrigger.Entry entrada = new EventTrigger.Entry { eventID = tipo };
        entrada.callback.AddListener(accion);
        trigger.triggers.Add(entrada);
    }

    private void MostrarInfoZona()
    {
        BuscarInfoZona();
        if (!ActualizarTextoInfoZona())
        {
            OcultarInfoZona();
            return;
        }

        if (fondoInfoZona != null)
        {
            fondoInfoZona.SetActive(true);
        }

        if (textoInfoZona != null)
        {
            textoInfoZona.gameObject.SetActive(true);
        }

        mostrandoInfoZona = true;
    }

    private void OcultarInfoZona()
    {
        mostrandoInfoZona = false;

        if (fondoInfoZona != null)
        {
            fondoInfoZona.SetActive(false);
        }

        if (textoInfoZona != null)
        {
            textoInfoZona.gameObject.SetActive(false);
        }
    }

    private bool ActualizarTextoInfoZona()
    {
        if (textoInfoZona == null)
        {
            return false;
        }

        string descripcion = ZonaMecanicaTextos.ObtenerDescripcion(
            zonaSeleccionada,
            0,
            ZonaMecanicasConocidas(zonaSeleccionada));
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            textoInfoZona.text = string.Empty;
            return false;
        }

        textoInfoZona.text = descripcion;
        return true;
    }

    private void ActualizarClimasZonaSeleccionada()
    {
        BuscarUIClimasZona();
        if (uiClimasZona != null)
        {
            uiClimasZona.MostrarRegion(zonaSeleccionada, metaprogresionActual);
        }
    }

    private void BuscarUIClimasZona()
    {
        if (uiClimasZona != null)
        {
            return;
        }

        uiClimasZona = GetComponentInChildren<UIManagerContZonas>(true);
        if (uiClimasZona != null)
        {
            return;
        }

        UIManagerContZonas[] managersClimas = FindObjectsByType<UIManagerContZonas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < managersClimas.Length; i++)
        {
            UIManagerContZonas managerClimas = managersClimas[i];
            if (managerClimas == null)
            {
                continue;
            }

            if (string.Equals(managerClimas.gameObject.name, "contClimas", StringComparison.OrdinalIgnoreCase)
                || managersClimas.Length == 1)
            {
                uiClimasZona = managerClimas;
                return;
            }
        }

        Transform[] objetos = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < objetos.Length; i++)
        {
            Transform candidato = objetos[i];
            if (candidato == null)
            {
                continue;
            }

            if (!string.Equals(candidato.gameObject.name, "contClimas", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(candidato.gameObject.name, "contClima", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            uiClimasZona = candidato.GetComponent<UIManagerContZonas>();
            if (uiClimasZona == null)
            {
                uiClimasZona = candidato.gameObject.AddComponent<UIManagerContZonas>();
            }

            return;
        }
    }

    private void BuscarControlesCarrusel()
    {
        TMP_Text[] textos = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            TMP_Text candidato = textos[i];
            if (candidato == null)
            {
                continue;
            }

            string nombre = candidato.gameObject.name;
            if (textoZonaSeleccionada == null
                && string.Equals(nombre, "txtNombreZona", StringComparison.OrdinalIgnoreCase))
            {
                textoZonaSeleccionada = candidato;
            }
            else if (textoDescripcionZona == null
                && string.Equals(nombre, "txtDescZona", StringComparison.OrdinalIgnoreCase))
            {
                textoDescripcionZona = candidato;
            }
            else if (disclaimerDemo == null
                && (string.Equals(nombre, "DisclaimerZona", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(nombre, "disclaimerDemo", StringComparison.OrdinalIgnoreCase)))
            {
                disclaimerDemo = candidato;
            }
        }

        if (textoZonaSeleccionada != null && !colorNombreZonaBaseCapturado)
        {
            colorNombreZonaBase = textoZonaSeleccionada.color;
            colorNombreZonaBaseCapturado = true;
        }

        if (btnNuevaZona == null)
        {
            Transform[] objetos = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < objetos.Length; i++)
            {
                if (string.Equals(objetos[i].gameObject.name, "btnNuevazona", StringComparison.OrdinalIgnoreCase))
                {
                    btnNuevaZona = objetos[i].gameObject;
                    break;
                }
            }
        }

        Button[] botones = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];
            if (boton == null)
            {
                continue;
            }

            if (flechaIzq == null
                && string.Equals(boton.gameObject.name, "FlechaZonaIzq", StringComparison.OrdinalIgnoreCase))
            {
                flechaIzq = boton.gameObject;
            }
            else if (flechaDer == null
                && string.Equals(boton.gameObject.name, "FlechaZonaDer", StringComparison.OrdinalIgnoreCase))
            {
                flechaDer = boton.gameObject;
            }
            else if (btnNuevaZona == null
                && string.Equals(boton.gameObject.name, "btnNuevazona", StringComparison.OrdinalIgnoreCase))
            {
                btnNuevaZona = boton.gameObject;
            }
        }
    }

    private void ConfigurarFlechasCarrusel()
    {
        if (!flechaIzquierdaConfigurada && flechaIzq != null)
        {
            Button botonIzquierda = flechaIzq.GetComponentInChildren<Button>(true);
            if (botonIzquierda != null)
            {
                botonIzquierda.onClick.AddListener(GirarCarruselIzquierda);
                flechaIzquierdaConfigurada = true;
            }
        }

        if (!flechaDerechaConfigurada && flechaDer != null)
        {
            Button botonDerecha = flechaDer.GetComponentInChildren<Button>(true);
            if (botonDerecha != null)
            {
                botonDerecha.onClick.AddListener(GirarCarruselDerecha);
                flechaDerechaConfigurada = true;
            }
        }
    }

    private void AlActualizarIdioma(int _)
    {
        ActualizarDropdownLider();
        ActualizarZonaSeleccionada();
        ActualizarTextosConfirmacionSobreescribir();
    }

    private void BuscarDropdownLider()
    {
        if (dropdownLider != null)
        {
            return;
        }

        TMP_Dropdown[] dropdowns = GetComponentsInChildren<TMP_Dropdown>(true);
        for (int i = 0; i < dropdowns.Length; i++)
        {
            TMP_Dropdown candidato = dropdowns[i];
            if (candidato != null
                && string.Equals(candidato.gameObject.name, "dropdownLider", StringComparison.OrdinalIgnoreCase))
            {
                dropdownLider = candidato;
                return;
            }
        }
    }

    private void BuscarTextosPresagios()
    {
        if (txtPresagio1 != null && txtPresagio2 != null)
        {
            return;
        }

        TMP_Text[] textos = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < textos.Length; i++)
        {
            TMP_Text candidato = textos[i];
            if (candidato == null)
            {
                continue;
            }

            if (txtPresagio1 == null
                && string.Equals(candidato.gameObject.name, "txtPresagio1", StringComparison.OrdinalIgnoreCase))
            {
                txtPresagio1 = candidato;
            }
            else if (txtPresagio2 == null
                && string.Equals(candidato.gameObject.name, "txtPresagio2", StringComparison.OrdinalIgnoreCase))
            {
                txtPresagio2 = candidato;
            }
        }
    }

    private void ActualizarDropdownLider()
    {
        BuscarDropdownLider();
        if (dropdownLider == null)
        {
            return;
        }

        int cantidadOpciones = nombresClases.Length + 1;
        int seleccionActual = dropdownLider.options != null && dropdownLider.options.Count == cantidadOpciones
            ? dropdownLider.value
            : 0;
        List<string> opciones = new List<string>(cantidadOpciones)
        {
            ObtenerTextoClaseAleatoria()
        };
        for (int i = 0; i < nombresClases.Length; i++)
        {
            opciones.Add(Traducir(nombresClases[i]));
        }

        dropdownLider.ClearOptions();
        dropdownLider.AddOptions(opciones);
        dropdownLider.SetValueWithoutNotify(Mathf.Clamp(seleccionActual, 0, cantidadOpciones - 1));
        dropdownLider.RefreshShownValue();
    }

    private int ObtenerClaseLiderSeleccionada()
    {
        BuscarDropdownLider();
        return dropdownLider != null
            ? Mathf.Clamp(dropdownLider.value, ClaseAleatoria, ClaseDuelista)
            : ClaseAleatoria;
    }

    private static string ObtenerTextoClaseAleatoria()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
        return idioma switch
        {
            TRADU.IdiomaIngles => "Random",
            TRADU.IdiomaPortugues => "Aleatório",
            _ => "Aleatorio"
        };
    }

    private static string ObtenerNombreZonaCarrusel(int zonaId)
    {
        int idioma = ObtenerIdiomaActual();

        if (idioma == TRADU.IdiomaIngles)
        {
            return zonaId switch
            {
                ZonaBosqueArdiente => "The Burning Forest",
                ZonaPasoVientoHelado => "Frozenwind Passage",
                ZonaNedukazal => "Nedukazal",
                _ => "Unknown"
            };
        }

        if (idioma == TRADU.IdiomaPortugues)
        {
            return zonaId switch
            {
                ZonaBosqueArdiente => "A Floresta Ardente",
                ZonaPasoVientoHelado => "Passagem do Vento Gelado",
                ZonaNedukazal => "Nedukazal",
                _ => "Desconhecida"
            };
        }

        return zonaId switch
        {
            ZonaBosqueArdiente => "El Bosque Ardiente",
            ZonaPasoVientoHelado => "Paso Viento Helado",
            ZonaNedukazal => "Nedukazal",
            _ => "Desconocida"
        };
    }

    private static string ObtenerDescripcionZonaCarrusel(int zonaId)
    {
        int idioma = ObtenerIdiomaActual();
        if (idioma == TRADU.IdiomaIngles)
        {
            return zonaId switch
            {
                ZonaBosqueArdiente =>
                    "The caravan will set out from the outskirts of the Forest of Glain, now known as the <b>Burning Forest</b>, after flames consumed a vast portion of it.\n\n"
                    + "Beyond the fire, they will have to face the forest's ancient guardians, deformed by flames and empowered by the Black Breath.",
                ZonaPasoVientoHelado =>
                    "The caravan must climb the steep and dangerous Crossed Mountains until it reaches Frozenwind Passage.\n\n"
                    + "This narrow gorge is the only known route across the mountain range. The relentless cold and wild creatures will not be the only dangers: the Kale'Tav tribe will not allow any intruder to cross their territory.",
                ZonaNedukazal =>
                    "The caravan must reach the ancient entrance to <b>Nedukazal</b>, a kingdom hidden beneath an immense mountain of stone.\n\n"
                    + "Its enormous portal, worn by the centuries, is the only way into its depths, allowing passage through and exit on the other side of the mountain.\n"
                    + "However, the secluded kingdom is facing a crisis of its own: unknown creatures have invaded it.",
                _ => string.Empty
            };
        }

        if (idioma == TRADU.IdiomaPortugues)
        {
            return zonaId switch
            {
                ZonaBosqueArdiente =>
                    "A caravana partirá das imediações da Floresta de Glain, agora chamada de <b>Floresta Ardente</b>, depois que as chamas devoraram grande parte dela.\n\n"
                    + "Além do fogo, deverão enfrentar os antigos guardiões da floresta, deformados pelas queimaduras, consumidos pela fúria e fortalecidos pelo Sopro Negro.",
                ZonaPasoVientoHelado =>
                    "A caravana deverá subir pelas íngremes e perigosas Montanhas Cruzadas até alcançar a Passagem do Vento Gelado.\n\n"
                    + "Este estreito desfiladeiro é a única rota conhecida para atravessar a cordilheira. O frio implacável e as criaturas selvagens não serão os únicos perigos: a tribo Kale'Tav não permitirá que nenhum intruso atravesse seu território.",
                ZonaNedukazal =>
                    "A caravana deverá alcançar a antiga entrada de <b>Nedukazal</b>, um reino oculto sob uma imensa montanha de pedra.\n\n"
                    + "Seu enorme portal, desgastado pelos séculos, é a única via de acesso às suas profundezas, permitindo atravessar e sair pelo outro lado da montanha.\n"
                    + "No entanto, o reino recluso enfrenta sua própria crise: criaturas desconhecidas o invadiram.",
                _ => string.Empty
            };
        }

        return zonaId switch
        {
            ZonaBosqueArdiente =>
                "La caravana partirá desde las inmediaciones del Bosque de Glain, ahora llamado el <b>Bosque Ardiente</b>, después de que las llamas devoraran gran parte de él.\n\n"
                + "Más allá del fuego, deberán enfrentarse a los antiguos guardianes del bosque, deformados por las quemaduras, consumidos por la furia y potenciados por el Aliento Negro.",
            ZonaPasoVientoHelado =>
                "La caravana deberá ascender por las escarpadas y peligrosas Montañas Cruzadas hasta alcanzar el Paso Vientohelado.\n\n"
                + "Este estrecho desfiladero es la única ruta conocida para atravesar la cordillera. El frío implacable y las criaturas salvajes no serán los únicos peligros: la tribu Kale'Tav no permitirá que ningún intruso cruce su territorio.",
            ZonaNedukazal =>
                "La caravana deberá alcanzar la antigua entrada de <b>Nedukazal</b>, un reino oculto bajo una inmensa montaña de piedra.\n\n"
                + "Su enorme portal, desgastado por los siglos, es la única vía de acceso a sus profundidades, para poder cruzar y salir por la otra cara de la montaña.\n"
                + "Sin embargo, el recluido reino tiene su propia crisis en curso: criaturas desconocidas lo han invadido.",
            _ => string.Empty
        };
    }

    private void ActualizarColorNombreZona()
    {
        if (textoZonaSeleccionada == null)
        {
            return;
        }

        textoZonaSeleccionada.color = zonaSeleccionada switch
        {
            ZonaPasoVientoHelado => colorNombrePasoVientoHelado,
            ZonaNedukazal => colorNombreNedukazal,
            _ => colorNombreZonaBase
        };
    }

    private void ActualizarBotonNuevaZona()
    {
        if (btnNuevaZona != null)
        {
            btnNuevaZona.SetActive(ZonaDisponibleEnDemoODebug(zonaSeleccionada));
        }
    }

    private static bool ZonaDisponibleEnDemoODebug(int zonaId)
    {
        if (zonaId == ZonaBosqueArdiente)
        {
            return true;
        }

#if UNITY_EDITOR
        return CampaignManager.EsZonaPermitidaPorDebug(zonaId);
#else
        return false;
#endif
    }

    private static int ObtenerIdiomaActual()
    {
        return TRADU.i != null
            ? TRADU.i.nIdioma
            : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
    }

    private void ActualizarDisclaimerDemo()
    {
        if (disclaimerDemo == null)
        {
            return;
        }

        if (ZonaDisponibleEnDemoODebug(zonaSeleccionada))
        {
            disclaimerDemo.text = string.Empty;
            return;
        }

        int idioma = ObtenerIdiomaActual();

        if (zonaSeleccionada == ZonaDesconocida)
        {
            disclaimerDemo.text = idioma switch
            {
                TRADU.IdiomaIngles => "More regions coming soon",
                TRADU.IdiomaPortugues => "Mais regiões em breve",
                _ => "Próximamente más regiones"
            };
            disclaimerDemo.color = colorDisclaimerDesconocida;
            return;
        }

        disclaimerDemo.text = idioma switch
        {
            TRADU.IdiomaIngles => "Region unavailable in the Demo",
            TRADU.IdiomaPortugues => "Região indisponível na Demo",
            _ => "Región no disponible en la Demo"
        };
        disclaimerDemo.color = colorDisclaimerNoDisponible;
    }

    private ZonaOpcion ObtenerZona(int id)
    {
        if (zonas == null)
        {
            return null;
        }

        for (int i = 0; i < zonas.Count; i++)
        {
            if (zonas[i] != null && zonas[i].id == id)
            {
                return zonas[i];
            }
        }

        return null;
    }

    private static void AsignarTexto(TMP_Text texto, object valor)
    {
        if (texto != null)
        {
            texto.text = valor != null ? valor.ToString() : string.Empty;
        }
    }

    private static string Traducir(string texto)
    {
        return TRADU.i != null && !string.IsNullOrWhiteSpace(texto)
            ? TRADU.i.Traducir(texto)
            : texto ?? string.Empty;
    }
}

public static class ZonaMecanicaTextos
{
    public static string ObtenerDescripcion(int zonaId, int fuerzaKaleTav = 0, bool mecanicasConocidas = true)
    {
        if (zonaId <= 0)
        {
            return string.Empty;
        }

        int idioma = ObtenerIdiomaActual();
        if (!mecanicasConocidas)
        {
            return ObtenerTextoMecanicasDesconocidas(idioma);
        }

        if (idioma == TRADU.IdiomaIngles)
        {
            return ObtenerDescripcionIngles(zonaId, fuerzaKaleTav);
        }

        if (idioma == TRADU.IdiomaPortugues)
        {
            return ObtenerDescripcionPortugues(zonaId, fuerzaKaleTav);
        }

        return ObtenerDescripcionEspanol(zonaId, fuerzaKaleTav);
    }

    private static string ObtenerTextoMecanicasDesconocidas(int idioma)
    {
        return idioma switch
        {
            TRADU.IdiomaIngles => "Mechanics unknown for this region. Visit it to learn them.",
            TRADU.IdiomaPortugues => "Mecânicas desconhecidas para esta região. Visite-a para conhecê-las.",
            _ => "Mecánicas desconocidas para esta región. Visítala para conocerlas."
        };
    }

    private static string ObtenerDescripcionEspanol(int zonaId, int fuerzaKaleTav)
    {
        switch (zonaId)
        {
            case 1:
                return "A medida que viajas por el bosque, <color=#ff6a1a><b>las llamas</b></color> envolverán regiones del mapa de forma inesperada.\n\nSi intentas atravesar un <color=#ff6a1a><b>Nodo prendido fuego</b></color>, perderás <color=#a0e812><b>10 de Esperanza</b></color> y <color=#c918bb><b>8-15 Civiles</b></color>.\nNo se podrá <b>descansar</b> en nodos incendiados.\n\nAdemás, las batallas que tengan lugar en un <color=#ff6a1a><b>Nodo incendiado</b></color>, tendrán <color=#ff6a1a><b>llamas</b></color> en el campo de batalla.";
            case 2:
                return "La tribu <color=#77c7ff><b>Kale'Tav</b></color> está realizando <color=#b98cff><b>rituales</b></color> en el área, preparándose para el <color=#8b5cf6><b>Aliento Negro</b></color>.\n\nAl escuchar sus <b>tambores</b> a lo lejos sabrás dónde se encuentran.\nPor cada <color=#b98cff><b>Ritual completado</b></color>, sus combatientes recibirán <color=#ffcc66><b>bonificaciones en batalla</b></color>.\n\nPara interrumpir un ritual debes aproximarte a los <color=#77c7ff><b>nodos marcados</b></color> y derrotarlos.\n\n<color=#77c7ff><b>Fuerza Kale'Tav:</b></color> " + fuerzaKaleTav;
            case 3:
                return "Debido a la invasión, <color=#b44a4a><b>Nedukazal</b></color> está envuelta en <color=#8b5cf6><b>caos y oscuridad</b></color>, por lo tanto la caravana no podrá ver claramente el camino adelante.\n\nAl depender de la <color=#ffd166><b>luz propia</b></color>, será más propensa a sufrir <color=#ff4d4d><b>emboscadas (+20%)</b></color>.\n\nMejora los <color=#77c7ff><b>Catalejos</b></color> para aumentar el <color=#77c7ff><b>rango de visión</b></color>.\nLas <color=#ffd166><b>Antorchas de Caravana</b></color> seguirán reforzando la luz propia de la caravana en esta región.\n\nEl <color=#8b5cf6><b>Aliento Negro</b></color> no será una preocupación en esta región.";
            default:
                return string.Empty;
        }
    }

    private static string ObtenerDescripcionIngles(int zonaId, int fuerzaKaleTav)
    {
        switch (zonaId)
        {
            case 1:
                return "As you travel through the forest, <color=#ff6a1a><b>flames</b></color> will unexpectedly engulf regions of the map.\n\nIf you try to cross a <color=#ff6a1a><b>burning Node</b></color>, you will lose <color=#a0e812><b>10 Hope</b></color> and <color=#c918bb><b>8-15 Civilians</b></color>.\nYou cannot <b>rest</b> on burning nodes.\n\nAlso, battles that take place on a <color=#ff6a1a><b>burning Node</b></color> will have <color=#ff6a1a><b>flames</b></color> on the battlefield.";
            case 2:
                return "The <color=#77c7ff><b>Kale'Tav</b></color> tribe is performing <color=#b98cff><b>rituals</b></color> in the area, preparing for the <color=#8b5cf6><b>Black Breath</b></color>.\n\nWhen you hear their <b>drums</b> in the distance, you will know where they are.\nFor each <color=#b98cff><b>completed Ritual</b></color>, their fighters will receive <color=#ffcc66><b>battle bonuses</b></color>.\n\nTo interrupt a ritual, approach the <color=#77c7ff><b>marked nodes</b></color> and defeat them.\n\n<color=#77c7ff><b>Kale'Tav Strength:</b></color> " + fuerzaKaleTav;
            case 3:
                return "Due to the invasion, <color=#b44a4a><b>Nedukazal</b></color> is shrouded in <color=#8b5cf6><b>chaos and darkness</b></color>, so the caravan will not be able to clearly see the path ahead.\n\nBecause it relies on its <color=#ffd166><b>own light</b></color>, it will be more likely to suffer <color=#ff4d4d><b>ambushes (+20%)</b></color>.\n\nThe <color=#ffd166><b>Caravan Torches</b></color> improvement will increase the <color=#77c7ff><b>Vision Range</b></color> of the caravan.\n\nThe <color=#8b5cf6><b>Black Breath</b></color> will not be a concern in this region.";
            default:
                return string.Empty;
        }
    }

    private static string ObtenerDescripcionPortugues(int zonaId, int fuerzaKaleTav)
    {
        switch (zonaId)
        {
            case 1:
                return "À medida que viaja pela floresta, <color=#ff6a1a><b>as chamas</b></color> envolverão regiões do mapa de forma inesperada.\n\nSe tentar atravessar um <color=#ff6a1a><b>Nodo em chamas</b></color>, perderá <color=#a0e812><b>10 de Esperança</b></color> e <color=#c918bb><b>8-15 Civis</b></color>.\nNão será possível <b>descansar</b> em nodos incendiados.\n\nAlém disso, as batalhas que acontecerem em um <color=#ff6a1a><b>Nodo incendiado</b></color> terão <color=#ff6a1a><b>chamas</b></color> no campo de batalha.";
            case 2:
                return "A tribo <color=#77c7ff><b>Kale'Tav</b></color> está realizando <color=#b98cff><b>rituais</b></color> na área, preparando-se para o <color=#8b5cf6><b>Sopro Negro</b></color>.\n\nAo ouvir seus <b>tambores</b> ao longe, você saberá onde eles estão.\nPara cada <color=#b98cff><b>Ritual completado</b></color>, seus combatentes receberão <color=#ffcc66><b>bonificações em batalha</b></color>.\n\nPara interromper um ritual, aproxime-se dos <color=#77c7ff><b>nodos marcados</b></color> e derrote-os.\n\n<color=#77c7ff><b>Força Kale'Tav:</b></color> " + fuerzaKaleTav;
            case 3:
                return "Devido à invasão, <color=#b44a4a><b>Nedukazal</b></color> está envolta em <color=#8b5cf6><b>caos e escuridão</b></color>, portanto a caravana não poderá ver claramente o caminho à frente.\n\nPor depender da <color=#ffd166><b>própria luz</b></color>, será mais propensa a sofrer <color=#ff4d4d><b>emboscadas (+20%)</b></color>.\n\nMelhore as <color=#77c7ff><b>Lunetas</b></color> para aumentar o <color=#77c7ff><b>Alcance de Visão</b></color>.\nAs <color=#ffd166><b>Tochas da Caravana</b></color> continuarão reforçando a luz própria da caravana nesta região.\n\nO <color=#8b5cf6><b>Sopro Negro</b></color> não será uma preocupação nesta região.";
            default:
                return string.Empty;
        }
    }

    private static int ObtenerIdiomaActual()
    {
        return TRADU.i != null
            ? TRADU.i.nIdioma
            : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
    }
}
