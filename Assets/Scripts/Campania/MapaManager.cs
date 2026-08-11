using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EstadoVisibilidadNodoCampania
{
    Oculto,
    EnVision,
    Visitado,
    DescubiertoRemoto
}

public class MapaManager : MonoBehaviour
{
    const float FraccionPasoActualizacionCullingCaminos = 0.18f;
    const float MargenDecoracionCaminosEnPasos = 0.20f;
    const float MargenCullingCaminosEnPasos = 0.42f;
    static readonly int[] TiposNodoEspejismos = { 1, 2, 3, 5, 6, 7, 8, 11, 14 };
    const int MinXSettlementTemprano = 5;
    const int MaxXSettlementTemprano = 6;
    const int MinXSettlementTardio = 7;
    const int MaxXSettlementTardio = 8;
    const int MinDiferenciaYSettlements = 2;
    const int DistanciaReveladoSettlement = 4;
    const string LogSettlementDescubiertoEs = "<color=#7ED6F7>-La Caravana encuentra un camino de piedra, hay un asentamiento cerca.</color>";
    const string LogSettlementDescubiertoEn = "<color=#7ED6F7>-The caravan finds a stone road, there is a settlement nearby.</color>";
    const string LogSettlementDescubiertoPt = "<color=#7ED6F7>-A Caravana encontra um caminho de pedra, ha um assentamento por perto.</color>";
    const float TiempoMaximoEsperaCamaraIntro = 1.5f;
    const float TiempoMaximoEsperaDecoracionIntro = 60f;
    const float DelayInicioAnimacionIntroCampania = 0.45f;
    const float DuracionFadeAcomodoIntroCampania = 0.18f;
    const float ProgresoInicialVisualIntroCampania = 0.5f;
    const float AlphaVignetaIntroCampania = 0.72f;
    const float DuracionFadeVignetaIntroCampania = 0.18f;
    const int SortingOrderVignetaIntroCampania = 32000;
    const string PrefGraficosIndex = "graficos_index";
    const int CalidadGraficaBaja = 0;
    bool inicioCompletado;
    bool generacionDiferidaPendiente;
    bool omitirAutoGeneracionEnStart;
    bool introCampaniaEnPreparacion;
    const float OffsetNodoSobreRelieve = 0.08f;
    const float OffsetConvoySobreRelieve = 0.03f;
    const float EscalaDestinoNoAdyacente = 0.85f;
    const float EscalaNodoInaccesible = 0.8f;
    [SerializeField] float rangoAleatorioNodoXZ = 0.18f;

    public ContenedorDeNodos scContenedordeNodos;

    public Nodo nodoActual;
    public GameObject goCaravana;
    public GameObject goCaravanafollower1;
    public GameObject goCaravanafollower2;
    public GameObject goCaravanafollower3;
    public GameObject goCaravanafollower4;
    public GameObject goCaravanafollower5;
    public GameObject goCaravanafollower6;
    readonly List<Nodo> settlementsForzados = new List<Nodo>(2);
    readonly List<Nodo> nodosActivosVisibilidad = new List<Nodo>(64);
    readonly Dictionary<Nodo, Vector3> escalasBaseNodos = new Dictionary<Nodo, Vector3>();
    readonly Dictionary<Nodo, Vector3> posicionesBaseLocalesNodos = new Dictionary<Nodo, Vector3>();
    int emboscadasSubterraneasZona;
    int viajesDesdeUltimaEmboscadaSubterranea = 99;
    bool refrescandoVisibilidadExploracion;
    float pasoMapaCache;
    int cantidadNodosPasoMapaCache = -1;
    int ultimoAlcanceRefrescoVision = -1;
    int hashNodosDentroVisionRefresco = int.MinValue;
    bool posicionRefrescoVisionInicializada;
    Vector3 ultimaPosicionCullingCaminos;
    float ultimoRadioCullingCaminos = -1f;
    bool cullingCaminosInicializado;
    bool ultimoDebugCullingCaminos;
    [Header("Transición visual de visión")]
    [Tooltip("Segundos que tarda en cambiar entre el alcance de Catalejos y Antorchas.")]
    [SerializeField, Min(0.05f)] float duracionTransicionVision = 1.4f;
    float alcanceVisionVisualPasos;
    float velocidadTransicionVision;
    bool alcanceVisionVisualInicializado;

    void Start()
    {
       inicioCompletado = true;
       if (omitirAutoGeneracionEnStart)
       {
           generacionDiferidaPendiente = false;
           return;
       }

       if (!generacionDiferidaPendiente)
       {
           StartCoroutine(GenerarNodosDiferido());
       }
    }

    void LateUpdate()
    {
       if (!Application.isPlaying || goCaravana == null || nodoActual == null || refrescandoVisibilidadExploracion)
       {
           return;
       }

       ActualizarTransicionAlcanceVision();

       int alcance = CampaignManager.Instance != null
           ? CampaignManager.Instance.ObtenerDistanciaVisionEfectiva()
           : 1;
       bool cambioAlcance = alcance != ultimoAlcanceRefrescoVision;
       int nuevoHashNodosDentroVision = CalcularHashNodosDentroVision();
       bool cruzoBordeAlgunNodo = nuevoHashNodosDentroVision != hashNodosDentroVisionRefresco;

       // Durante el viaje la niebla recorta visualmente el mapa por pixel.
       // Regenerar polilineas cada pocos metros provocaba tirones y ya no es
       // necesario. Solo se refresca el estado lógico cuando un nodo cruza el
       // borde real o cuando cambia el alcance del catalejo.
       if (cambioAlcance || !posicionRefrescoVisionInicializada)
       {
           RefrescarVisibilidadExploracion();
       }
       else if (cruzoBordeAlgunNodo)
       {
           RefrescarSoloVisibilidadNodosDuranteViaje(nuevoHashNodosDentroVision);
       }

       ActualizarCullingCaminosSiHaceFalta();
    }

    


  public void GenerarNodos()
  {
       if (scContenedordeNodos == null) return;

       if (!inicioCompletado)
       {
           if (!generacionDiferidaPendiente)
           {
               StartCoroutine(GenerarNodosDiferido());
           }
           return;
       }

       scContenedordeNodos.RecolectarNodos();

       int zonaId = -1;
       if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
       {
           zonaId = CampaignManager.Instance.scAtributosZona.ID;
       }

       Nodo origen = scContenedordeNodos.ObtenerNodoSegunXY(0,0);
       if (origen == null) return;
       if (origen.yatiroConexiones)
       {
           if (nodoActual == null)
           {
               nodoActual = origen;
           }

           if (nodoActual != null)
           {
               AplicarMapaLinealTutorialSiCorresponde();
               RefrescarVisibilidadExploracion();
               IniciarIntroCampaniaPendienteTrasCarga();
           }
           return;
       }

       settlementsForzados.Clear();
       ReiniciarEstadoVariedadMapa();
       PrepararNodosParaGeneracion();
       origen.DeterminarConexiones();
       nodoActual = origen;
       ForzarNodosObligatorios(zonaId);
       ForzarSettlementsPorMapa(zonaId);
       DesactivarNodosSinUsar(zonaId);
       AplicarPresagiosInicialesDeNodos();
       AplicarZonaCartografiadaSiCorresponde();
       origen.PosicionarObjetoEnNodo(goCaravana);
       AlinearConvoyAlSuelo();

       if (nodoActual != null)
       {
           AplicarMapaLinealTutorialSiCorresponde();
           RefrescarVisibilidadExploracion();
           IniciarIntroCampaniaPendienteTrasCarga();
       }
  }

    public void IniciarIntroCampaniaPendienteTrasCarga(bool ignorarFaderInicial = false)
    {
        CampaignManager campaignManager = CampaignManager.Instance;
        if (campaignManager == null || introCampaniaEnPreparacion || !campaignManager.IntroCampaniaActivaOPendiente)
        {
            return;
        }

        if (!ignorarFaderInicial && !campaignManager.IntroCampaniaPuedeIniciarTrasCarga())
        {
            return;
        }

        StartCoroutine(EsperarMapaEIniciarIntroCampania(campaignManager));
    }

    IEnumerator EsperarMapaEIniciarIntroCampania(CampaignManager campaignManager)
    {
        introCampaniaEnPreparacion = true;
        yield return null;
        yield return new WaitForEndOfFrame();

        Nodo origen = null;
        float tiempoEsperandoMapa = 0f;
        while (origen == null && tiempoEsperandoMapa < TiempoMaximoEsperaCamaraIntro)
        {
            origen = ObtenerNodoIntroCampania();
            if (origen == null || goCaravana == null)
            {
                origen = null;
                tiempoEsperandoMapa += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        yield return EsperarDecoracionZonaIntroCampania(campaignManager);

        introCampaniaEnPreparacion = false;
        if (campaignManager == null || !campaignManager.IntroCampaniaActivaOPendiente)
        {
            yield break;
        }

        if (origen == null || goCaravana == null)
        {
            FinalizarIntroCampaniaConFallback(origen, campaignManager, "mapa o caravana no listos tras cargar");
            yield break;
        }

        IntentarIniciarIntroCampania(origen);
    }

    IEnumerator EsperarDecoracionZonaIntroCampania(CampaignManager campaignManager)
    {
        AtributosZona atributosZona = campaignManager != null ? campaignManager.scAtributosZona : null;
        float tiempoEsperandoDecoracion = 0f;

        while (atributosZona != null && atributosZona.DecoracionZonaEnCurso && tiempoEsperandoDecoracion < TiempoMaximoEsperaDecoracionIntro)
        {
            tiempoEsperandoDecoracion += Time.unscaledDeltaTime;
            yield return null;
        }

    }

    Nodo ObtenerNodoIntroCampania()
    {
        if (nodoActual != null)
        {
            return nodoActual;
        }

        if (scContenedordeNodos == null)
        {
            return null;
        }

        scContenedordeNodos.RecolectarNodos();
        Nodo origen = scContenedordeNodos.ObtenerNodoSegunXY(0, 0);
        return origen;
    }

    void IntentarIniciarIntroCampania(Nodo origen)
    {
        CampaignManager campaignManager = CampaignManager.Instance;
        if (campaignManager == null || origen == null || introCampaniaEnPreparacion || !campaignManager.IntroCampaniaActivaOPendiente)
        {
            return;
        }

        if (DebeOmitirIntroCampaniaPorCalidadBaja())
        {
            FinalizarIntroCampaniaSinAnimacion(origen, campaignManager);
            return;
        }

        origen.scMapaManager = this;
        LineRenderer lineaIntro = origen.CrearLineaIntroCampaniaDesdeIzquierda(Vector3.left);
        if (goCaravana == null || lineaIntro == null)
        {
            FinalizarIntroCampaniaConFallback(origen, campaignManager, "faltan caravana o linePrefab");
            return;
        }

        introCampaniaEnPreparacion = true;
        StartCoroutine(ReproducirIntroCampania(origen, lineaIntro, campaignManager));
    }

    IEnumerator ReproducirIntroCampania(Nodo origen, LineRenderer lineaIntro, CampaignManager campaignManager)
    {
        Transform camaraTransform = null;
        float tiempoEsperandoCamara = 0f;
        while (camaraTransform == null && tiempoEsperandoCamara < TiempoMaximoEsperaCamaraIntro)
        {
            Camera camaraIntro = ObtenerCamaraIntroCampania();
            camaraTransform = camaraIntro != null ? camaraIntro.transform : null;

            if (camaraTransform == null)
            {
                tiempoEsperandoCamara += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        if (campaignManager == null || goCaravana == null || lineaIntro == null || camaraTransform == null)
        {
            FinalizarIntroCampaniaConFallback(origen, campaignManager, "no hubo camara activa al iniciar");
            yield break;
        }

        if (!campaignManager.ConsumirIntroCampaniaPendiente())
        {
            introCampaniaEnPreparacion = false;
            yield break;
        }

        Transform parentCamaraOriginal = camaraTransform != null ? camaraTransform.parent : null;
        int indiceHermanoCamaraOriginal = camaraTransform != null && parentCamaraOriginal != null ? camaraTransform.GetSiblingIndex() : -1;
        Vector3 posicionLocalCamaraOriginal = camaraTransform != null ? camaraTransform.localPosition : Vector3.zero;
        Quaternion rotacionLocalCamaraOriginal = camaraTransform != null ? camaraTransform.localRotation : Quaternion.identity;
        Vector3 escalaLocalCamaraOriginal = camaraTransform != null ? camaraTransform.localScale : Vector3.one;
        Transform rotacionVisualLider = ObtenerRotacionVisualLiderIntro();
        List<(Behaviour componente, bool activo)> controlesCamaraIntro = DesactivarControlesCamaraIntro(camaraTransform);
        AdministradorEscenas administradorEscenasIntro = campaignManager.scAdministradorEscenas;

        if (camaraTransform != null)
        {
            camaraTransform.SetParent(null, true);
        }

        bool faderAcomodoActivo = ActivarFaderAcomodoIntro(administradorEscenasIntro);
        GameObject vignetaIntro = CrearVignetaIntroCampania(administradorEscenasIntro);
        Vector3 inicioIntro = ObtenerPosicionLineaIntroPorFraccion(lineaIntro, ProgresoInicialVisualIntroCampania);
        Vector3 deltaIntro = inicioIntro - goCaravana.transform.position;
        DesplazarConvoy(deltaIntro);
        AlinearConvoyAlSuelo();
        origen.PrepararConvoyIntroEnLinea(lineaIntro, rotacionVisualLider, ProgresoInicialVisualIntroCampania);
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(DelayInicioAnimacionIntroCampania);
        if (faderAcomodoActivo)
        {
            yield return DesactivarFaderAcomodoIntro(administradorEscenasIntro);
        }

        bool introFinalizada = false;
        origen.MoverConvoyIntroEnLinea(lineaIntro, () =>
        {
            introFinalizada = true;
        }, rotacionVisualLider, ProgresoInicialVisualIntroCampania);

        while (!introFinalizada)
        {
            yield return null;
        }

        if (origen != null)
        {
            Vector3 posicionCaravanaAntesDeSnap = goCaravana != null ? goCaravana.transform.position : Vector3.zero;
            origen.PosicionarObjetoEnNodo(goCaravana);
            if (goCaravana != null)
            {
                DesplazarSeguidores(goCaravana.transform.position - posicionCaravanaAntesDeSnap);
            }
            AlinearConvoyAlSuelo();
        }

        RestaurarCamaraIntroCampania(camaraTransform, parentCamaraOriginal, indiceHermanoCamaraOriginal, posicionLocalCamaraOriginal, rotacionLocalCamaraOriginal, escalaLocalCamaraOriginal);
        RestaurarControlesCamaraIntro(controlesCamaraIntro);
        yield return DesvanecerYDestruirVignetaIntroCampania(vignetaIntro);
        introCampaniaEnPreparacion = false;
        campaignManager.FinalizarIntroCampania();
    }

    Camera ObtenerCamaraIntroCampania()
    {
        Camera camaraCaravana = ObtenerCamaraHijaCaravana();
        if (camaraCaravana != null)
        {
            return camaraCaravana;
        }

        Camera camara = Camera.main;
        if (EsCamaraIntroDisponible(camara))
        {
            return camara;
        }

        Camera[] camaras = Camera.allCameras;
        for (int i = 0; i < camaras.Length; i++)
        {
            if (EsCamaraIntroDisponible(camaras[i]))
            {
                return camaras[i];
            }
        }

        Camera[] camarasEscena = Resources.FindObjectsOfTypeAll<Camera>();
        for (int i = 0; i < camarasEscena.Length; i++)
        {
            if (EsCamaraIntroDisponible(camarasEscena[i]) && camarasEscena[i].gameObject.scene.IsValid())
            {
                return camarasEscena[i];
            }
        }

        return null;
    }

    Camera ObtenerCamaraHijaCaravana()
    {
        if (goCaravana == null)
        {
            return null;
        }

        Camera[] camarasCaravana = goCaravana.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < camarasCaravana.Length; i++)
        {
            if (EsCamaraIntroDisponible(camarasCaravana[i]))
            {
                return camarasCaravana[i];
            }
        }

        return null;
    }

    bool EsCamaraIntroDisponible(Camera camara)
    {
        return camara != null && camara.enabled && camara.gameObject.activeInHierarchy;
    }

    Transform ObtenerRotacionVisualLiderIntro()
    {
        return goCaravana != null && goCaravana.transform.childCount > 4
            ? goCaravana.transform.GetChild(4)
            : null;
    }

    List<(Behaviour componente, bool activo)> DesactivarControlesCamaraIntro(Transform camaraTransform)
    {
        List<(Behaviour componente, bool activo)> controles = new List<(Behaviour componente, bool activo)>();
        if (camaraTransform == null)
        {
            return controles;
        }

        RegistrarControlCamaraIntro(camaraTransform.GetComponent<EdgePanCameraZ>(), controles);
        RegistrarControlCamaraIntro(camaraTransform.GetComponent<CameraObstaculosFader>(), controles);

        for (int i = 0; i < controles.Count; i++)
        {
            if (controles[i].componente != null)
            {
                controles[i].componente.enabled = false;
            }
        }

        return controles;
    }

    void RegistrarControlCamaraIntro(Behaviour componente, List<(Behaviour componente, bool activo)> controles)
    {
        if (componente == null || controles == null)
        {
            return;
        }

        controles.Add((componente, componente.enabled));
    }

    void RestaurarControlesCamaraIntro(List<(Behaviour componente, bool activo)> controles)
    {
        if (controles == null)
        {
            return;
        }

        for (int i = 0; i < controles.Count; i++)
        {
            if (controles[i].componente != null)
            {
                controles[i].componente.enabled = controles[i].activo;
            }
        }
    }

    static Vector3 ObtenerPosicionLineaIntroPorFraccion(LineRenderer lr, float fraccion)
    {
        if (lr == null || lr.positionCount == 0)
        {
            return Vector3.zero;
        }

        if (lr.positionCount == 1)
        {
            Vector3 unico = lr.GetPosition(0);
            return lr.useWorldSpace ? unico : lr.transform.TransformPoint(unico);
        }

        int cantidadPuntos = lr.positionCount;
        Vector3[] puntos = new Vector3[cantidadPuntos];
        float total = 0f;
        for (int i = 0; i < cantidadPuntos; i++)
        {
            Vector3 punto = lr.GetPosition(i);
            puntos[i] = lr.useWorldSpace ? punto : lr.transform.TransformPoint(punto);
            if (i > 0)
            {
                total += Vector3.Distance(puntos[i - 1], puntos[i]);
            }
        }

        float objetivo = total * Mathf.Clamp01(fraccion);
        float acumulado = 0f;
        for (int i = 0; i < cantidadPuntos - 1; i++)
        {
            float segmento = Vector3.Distance(puntos[i], puntos[i + 1]);
            if (segmento <= 0.0001f)
            {
                continue;
            }

            if (acumulado + segmento >= objetivo)
            {
                return Vector3.Lerp(puntos[i], puntos[i + 1], (objetivo - acumulado) / segmento);
            }

            acumulado += segmento;
        }

        return puntos[cantidadPuntos - 1];
    }

    GameObject CrearVignetaIntroCampania(AdministradorEscenas administradorEscenas)
    {
        GameObject vignetaRoot = new GameObject("VignetaIntroCampaniaCanvas", typeof(Canvas));
        Canvas canvas = vignetaRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = SortingOrderVignetaIntroCampania;

        GameObject imagen = new GameObject("VignetaIntroCampania", typeof(RectTransform), typeof(CanvasGroup), typeof(RawImage));
        imagen.transform.SetParent(vignetaRoot.transform, false);
        ConfigurarRectTransformPantalla(imagen.GetComponent<RectTransform>());
        ConfigurarImagenVigneta(imagen.GetComponent<RawImage>(), imagen.GetComponent<CanvasGroup>());
        return vignetaRoot;
    }

    static void ConfigurarRectTransformPantalla(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    static void ConfigurarImagenVigneta(RawImage rawImage, CanvasGroup canvasGroup)
    {
        if (rawImage != null)
        {
            rawImage.texture = CrearTexturaVignetaIntroCampania(256);
            rawImage.color = Color.white;
            rawImage.raycastTarget = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = AlphaVignetaIntroCampania;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    static Texture2D CrearTexturaVignetaIntroCampania(int size)
    {
        Texture2D textura = new Texture2D(size, size, TextureFormat.RGBA32, false);
        textura.name = "TexturaVignetaIntroCampania";
        textura.wrapMode = TextureWrapMode.Clamp;

        float mitad = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - mitad) / mitad;
                float ny = (y - mitad) / mitad;
                float distancia = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.38f, 1.08f, distancia));
                textura.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }

        textura.Apply(false, true);
        return textura;
    }

    IEnumerator DesvanecerYDestruirVignetaIntroCampania(GameObject vignetaRoot)
    {
        if (vignetaRoot == null)
        {
            yield break;
        }

        CanvasGroup canvasGroup = vignetaRoot.GetComponentInChildren<CanvasGroup>(true);
        if (canvasGroup != null)
        {
            float alphaInicial = canvasGroup.alpha;
            float tiempo = 0f;
            while (tiempo < DuracionFadeVignetaIntroCampania)
            {
                tiempo += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(alphaInicial, 0f, Mathf.Clamp01(tiempo / DuracionFadeVignetaIntroCampania));
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        RawImage rawImage = vignetaRoot.GetComponentInChildren<RawImage>(true);
        if (rawImage != null && rawImage.texture != null)
        {
            Destroy(rawImage.texture);
        }

        Destroy(vignetaRoot);
    }

    bool DebeOmitirIntroCampaniaPorCalidadBaja()
    {
        int calidadActual = PlayerPrefs.GetInt(PrefGraficosIndex, QualitySettings.GetQualityLevel());
        return calidadActual <= CalidadGraficaBaja;
    }

    void FinalizarIntroCampaniaSinAnimacion(Nodo origen, CampaignManager campaignManager)
    {
        introCampaniaEnPreparacion = false;

        if (origen != null)
        {
            origen.PosicionarObjetoEnNodo(goCaravana);
        }

        AlinearConvoyAlSuelo();
        campaignManager?.FinalizarIntroCampania();
    }

    void FinalizarIntroCampaniaConFallback(Nodo origen, CampaignManager campaignManager, string motivo)
    {
        introCampaniaEnPreparacion = false;
        Debug.LogWarning("[IntroCampania] Fallback: " + motivo, this);

        if (origen != null)
        {
            origen.PosicionarObjetoEnNodo(goCaravana);
        }

        AlinearConvoyAlSuelo();
        campaignManager?.FinalizarIntroCampania();
    }

    void RestaurarCamaraIntroCampania(Transform camaraTransform, Transform parentOriginal, int indiceHermanoOriginal, Vector3 posicionLocalOriginal, Quaternion rotacionLocalOriginal, Vector3 escalaLocalOriginal)
    {
        if (camaraTransform == null)
        {
            return;
        }

        if (parentOriginal != null)
        {
            camaraTransform.SetParent(parentOriginal, false);
            if (indiceHermanoOriginal >= 0)
            {
                camaraTransform.SetSiblingIndex(Mathf.Clamp(indiceHermanoOriginal, 0, parentOriginal.childCount - 1));
            }

            camaraTransform.localPosition = posicionLocalOriginal;
            camaraTransform.localRotation = rotacionLocalOriginal;
            camaraTransform.localScale = escalaLocalOriginal;
            return;
        }

        if (goCaravana != null)
        {
            camaraTransform.SetParent(goCaravana.transform, true);
        }
    }

    void DesplazarConvoy(Vector3 delta)
    {
        DesplazarTransform(goCaravana, delta);
        DesplazarTransform(goCaravanafollower1, delta);
        DesplazarTransform(goCaravanafollower2, delta);
        DesplazarTransform(goCaravanafollower3, delta);
        DesplazarTransform(goCaravanafollower4, delta);
        DesplazarTransform(goCaravanafollower5, delta);
        DesplazarTransform(goCaravanafollower6, delta);
    }

    static void DesplazarTransform(GameObject go, Vector3 delta)
    {
        if (go == null)
        {
            return;
        }

        go.transform.position += delta;
    }

    void DesplazarSeguidores(Vector3 delta)
    {
        DesplazarTransform(goCaravanafollower1, delta);
        DesplazarTransform(goCaravanafollower2, delta);
        DesplazarTransform(goCaravanafollower3, delta);
        DesplazarTransform(goCaravanafollower4, delta);
        DesplazarTransform(goCaravanafollower5, delta);
        DesplazarTransform(goCaravanafollower6, delta);
    }

    bool ActivarFaderAcomodoIntro(AdministradorEscenas administradorEscenas)
    {
        if (administradorEscenas == null || administradorEscenas.fader == null)
        {
            return false;
        }

        administradorEscenas.SetFaderHold(true);
        return true;
    }

    IEnumerator DesactivarFaderAcomodoIntro(AdministradorEscenas administradorEscenas)
    {
        if (administradorEscenas == null || administradorEscenas.fader == null)
        {
            yield break;
        }

        administradorEscenas.SetFaderHold(false);
        yield return administradorEscenas.FadeOut(DuracionFadeAcomodoIntroCampania);
        administradorEscenas.fader.blocksRaycasts = false;
        administradorEscenas.fader.interactable = false;
    }

  private void AplicarMapaLinealTutorialSiCorresponde()
  {
       CampaignManager campaignManager = CampaignManager.Instance;
       if (campaignManager == null || !campaignManager.DebeForzarMapaLinealTutorial())
       {
           return;
       }

       TutorialManager tutorialManager = campaignManager.scTutorialManager;
       if (tutorialManager == null)
       {
           return;
       }

       tutorialManager.ConfigurarSoloMapaLinealTutorial();
       if (nodoActual == null)
       {
           nodoActual = scContenedordeNodos != null ? scContenedordeNodos.ObtenerNodoSegunXY(0, 0) : null;
       }
  }

  public void RefrescarVisibilidadExploracion()
  {
       if (!Application.isPlaying || refrescandoVisibilidadExploracion)
       {
           return;
       }

       refrescandoVisibilidadExploracion = true;
       try
       {
       if (scContenedordeNodos == null) return;
       if (scContenedordeNodos.listTodosNodos.Count == 0)
       {
           scContenedordeNodos.RecolectarNodos();
       }

       nodosActivosVisibilidad.Clear();
       foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
       {
           if (nodo != null && nodo.gameObject.activeSelf)
           {
               nodosActivosVisibilidad.Add(nodo);
           }
       }

       foreach (Nodo nodo in nodosActivosVisibilidad)
       {
           if (nodo != null && nodo.posXNodo == 11 && (!nodo.revelado || !nodo.FueDescubiertoPorMecanicaEspecial()))
           {
               nodo.ForzarVisiblePorReveladoEspecial();
           }
       }

       Vector3 centroVision = ObtenerCentroVisionActual();
       float radioVision = ObtenerRadioVisionActual();

       foreach (Nodo nodo in nodosActivosVisibilidad)
       {
           AsegurarMisteriosoAdyacenteFueraDeVision(nodo, centroVision, radioVision);
           nodo.PrepararCaminosParaMascaraVision();
           nodo.AplicarEstadoVisibilidadCampania(ObtenerEstadoVisibilidadNodo(nodo, centroVision, radioVision));
       }

       HashSet<Nodo> nodosAlcanzables = CalcularNodosAlcanzablesDesdeActual();
       AplicarEscalaDestinosNoAdyacentes();
       foreach (Nodo nodo in nodosActivosVisibilidad)
       {
           nodo.AplicarMaterialCaminosSegunAlcance(nodosAlcanzables);
       }

       if (nodoActual != null)
       {
           nodoActual.RefrescarCaminosMarcadosDesdeEstadoActual();
       }

       if (CampaignManager.Instance != null && CampaignManager.Instance.DebeMostrarTodosLosCaminosMapaDebug())
       {
           foreach (Nodo nodo in nodosActivosVisibilidad)
           {
               nodo.MostrarTodosLosCaminosDebug();
           }
       }

       ActualizarCullingCaminosSiHaceFalta(true);

       ultimoAlcanceRefrescoVision = CampaignManager.Instance != null
           ? CampaignManager.Instance.ObtenerDistanciaVisionEfectiva()
           : 1;
       hashNodosDentroVisionRefresco = CalcularHashNodosDentroVision();
       posicionRefrescoVisionInicializada = true;
       }
       finally
       {
           refrescandoVisibilidadExploracion = false;
       }
  }

  public bool NodoDentroDeVision(Nodo nodo)
  {
       return nodo != null && PosicionDentroDeVision(nodo.transform.position);
  }

  void ActualizarCullingCaminosSiHaceFalta(bool forzar = false)
  {
       if (scContenedordeNodos == null || goCaravana == null)
       {
           return;
       }

       Vector3 centro = ObtenerCentroVisionActual();
       float pasoMapa = ObtenerPasoMapa();
       float radio = ObtenerRadioVisionActual();
       bool mostrarTodos = CampaignManager.Instance != null
           && CampaignManager.Instance.DebeMostrarTodosLosCaminosMapaDebug();
       float umbralMovimiento = Mathf.Max(
           0.12f,
           pasoMapa * FraccionPasoActualizacionCullingCaminos);
       bool seMovio = !cullingCaminosInicializado
           || DistanciaHorizontal(centro, ultimaPosicionCullingCaminos) >= umbralMovimiento;
       bool cambioRadio = Mathf.Abs(radio - ultimoRadioCullingCaminos) > 0.01f;
       bool cambioDebug = mostrarTodos != ultimoDebugCullingCaminos;
       if (!forzar && !seMovio && !cambioRadio && !cambioDebug)
       {
           return;
       }

       float radioDecoracion = radio + pasoMapa * MargenDecoracionCaminosEnPasos;
       float radioCulling = radio + pasoMapa * MargenCullingCaminosEnPasos;
       foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
       {
           if (nodo != null && nodo.gameObject.activeSelf)
           {
               nodo.ActualizarCullingCaminos(
                   centro,
                   radioDecoracion,
                   radioCulling,
                   mostrarTodos);
           }
       }

       ultimaPosicionCullingCaminos = centro;
       ultimoRadioCullingCaminos = radio;
       ultimoDebugCullingCaminos = mostrarTodos;
       cullingCaminosInicializado = true;
  }

  void RefrescarSoloVisibilidadNodosDuranteViaje(int nuevoHashNodosDentroVision)
  {
       if (scContenedordeNodos == null)
       {
           return;
       }

       Vector3 centroVision = ObtenerCentroVisionActual();
       float radioVision = ObtenerRadioVisionActual();
       foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
       {
           if (nodo == null || !nodo.gameObject.activeSelf)
           {
               continue;
           }

           AsegurarMisteriosoAdyacenteFueraDeVision(nodo, centroVision, radioVision);
           EstadoVisibilidadNodoCampania estado =
               ObtenerEstadoVisibilidadNodo(nodo, centroVision, radioVision);
           if (!nodo.TieneEstadoVisibilidadCampania(estado))
           {
               nodo.AplicarEstadoVisibilidadCampania(estado);
           }
       }

       hashNodosDentroVisionRefresco = nuevoHashNodosDentroVision;
  }

  void AsegurarMisteriosoAdyacenteFueraDeVision(Nodo nodo, Vector3 centroVision, float radioVision)
  {
       if (nodo == null
           || nodo.revelado
           || !EsNodoContiguoAlActual(nodo)
           || PosicionDentroDeVision(nodo.transform.position, centroVision, radioVision))
       {
           return;
       }

       if (CampaignManager.Instance != null && CampaignManager.Instance.DebeUsarConfiguracionTutorial())
       {
           return;
       }

       nodo.RevelarComoMisterioso();
  }

  void ActualizarHintsCaminos(Vector3 centroVision, float radioVision)
  {
       bool mostrarTodos = CampaignManager.Instance != null
           && CampaignManager.Instance.DebeMostrarTodosLosCaminosMapaDebug();

       foreach (Nodo origen in scContenedordeNodos.listTodosNodos)
       {
           if (origen == null || !origen.gameObject.activeSelf)
           {
               continue;
           }

           bool origenDentroDeVision = !mostrarTodos
               && PosicionDentroDeVision(origen.transform.position, centroVision, radioVision);
           foreach (Nodo destino in origen.DestinosPosibles)
           {
               bool mostrarHint = origenDentroDeVision
                   && destino != null
                   && destino.gameObject.activeSelf
                   && destino != nodoActual
                   && !destino.nodoDespejado
                   && !PosicionDentroDeVision(destino.transform.position, centroVision, radioVision);

               if (mostrarHint)
               {
                   origen.MostrarContinuacionCortaPorVisionHacia(destino);
               }
               else
               {
                   origen.OcultarContinuacionCortaPorVisionHacia(destino);
               }
           }
       }
  }

  public Vector3 ObtenerCentroVisionActual()
  {
       if (goCaravana != null && goCaravana.activeInHierarchy)
       {
           return goCaravana.transform.position;
       }

       return nodoActual != null ? nodoActual.transform.position : transform.position;
  }

  public float ObtenerRadioVisionActual()
  {
       return ObtenerPasoMapa() * ObtenerAlcanceVisionVisualEnPasos();
  }

  float ObtenerAlcanceVisionVisualEnPasos()
  {
       float objetivo = CampaignManager.Instance != null
           ? CampaignManager.Instance.ObtenerAlcanceVisionEnPasos()
           : 1.5f;
       if (!alcanceVisionVisualInicializado)
       {
           alcanceVisionVisualPasos = objetivo;
           alcanceVisionVisualInicializado = true;
       }
       return alcanceVisionVisualPasos;
  }

  void ActualizarTransicionAlcanceVision()
  {
       float objetivo = CampaignManager.Instance != null
           ? CampaignManager.Instance.ObtenerAlcanceVisionEnPasos()
           : 1.5f;
       if (!alcanceVisionVisualInicializado)
       {
           alcanceVisionVisualPasos = objetivo;
           alcanceVisionVisualInicializado = true;
           return;
       }

       alcanceVisionVisualPasos = Mathf.SmoothDamp(
           alcanceVisionVisualPasos,
           objetivo,
           ref velocidadTransicionVision,
           Mathf.Max(0.05f, duracionTransicionVision),
           Mathf.Infinity,
           Time.unscaledDeltaTime);
       if (Mathf.Abs(alcanceVisionVisualPasos - objetivo) < 0.001f)
       {
           alcanceVisionVisualPasos = objetivo;
           velocidadTransicionVision = 0f;
       }
  }

  public bool PosicionDentroDeVision(Vector3 posicion)
  {
       return PosicionDentroDeVision(posicion, ObtenerCentroVisionActual(), ObtenerRadioVisionActual());
  }

  public EstadoVisibilidadNodoCampania ObtenerEstadoVisibilidadNodo(Nodo nodo)
  {
       return ObtenerEstadoVisibilidadNodo(nodo, ObtenerCentroVisionActual(), ObtenerRadioVisionActual());
  }

  public float ObtenerPasoMapa()
  {
       int cantidadNodos = scContenedordeNodos != null ? scContenedordeNodos.listTodosNodos.Count : 0;
       if (pasoMapaCache > 0.01f && cantidadNodosPasoMapaCache == cantidadNodos)
       {
           return pasoMapaCache;
       }

       List<float> distancias = new List<float>(64);
       if (scContenedordeNodos != null)
       {
           if (cantidadNodos == 0)
           {
               scContenedordeNodos.RecolectarNodos();
           }

           foreach (Nodo origen in scContenedordeNodos.listTodosNodos)
           {
               if (origen == null || !origen.gameObject.activeSelf) continue;
               foreach (Nodo destino in origen.DestinosPosibles)
               {
                   if (destino == null || !destino.gameObject.activeSelf || Mathf.Abs(destino.posXNodo - origen.posXNodo) != 1) continue;
                   float distancia = DistanciaHorizontal(origen.transform.position, destino.transform.position);
                   if (distancia > 0.01f) distancias.Add(distancia);
               }
           }
       }

       if (distancias.Count == 0)
       {
           pasoMapaCache = 5f;
       }
       else
       {
           distancias.Sort();
           int medio = distancias.Count / 2;
           pasoMapaCache = distancias.Count % 2 == 0
               ? (distancias[medio - 1] + distancias[medio]) * 0.5f
               : distancias[medio];
       }

       cantidadNodosPasoMapaCache = scContenedordeNodos != null ? scContenedordeNodos.listTodosNodos.Count : 0;
       return pasoMapaCache;
  }

  static bool PosicionDentroDeVision(Vector3 posicion, Vector3 centro, float radio)
  {
       float dx = posicion.x - centro.x;
       float dz = posicion.z - centro.z;
       return dx * dx + dz * dz <= radio * radio + 0.0001f;
  }

  static float DistanciaHorizontal(Vector3 a, Vector3 b)
  {
       float dx = a.x - b.x;
       float dz = a.z - b.z;
       return Mathf.Sqrt(dx * dx + dz * dz);
  }

  int CalcularHashNodosDentroVision()
  {
       if (scContenedordeNodos == null)
       {
           return 0;
       }

       Vector3 centro = ObtenerCentroVisionActual();
       float radio = ObtenerRadioVisionActual();
       unchecked
       {
           int hash = 17;
           foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
           {
               if (nodo == null || !nodo.gameObject.activeSelf)
               {
                   continue;
               }

               hash = hash * 31 + nodo.GetInstanceID();
               hash = hash * 31
                   + (PosicionDentroDeVision(nodo.transform.position, centro, radio) ? 1 : 0);
           }

           return hash;
       }
  }

  EstadoVisibilidadNodoCampania ObtenerEstadoVisibilidadNodo(Nodo nodo, Vector3 centro, float radio)
  {
       if (nodo == null || !nodo.gameObject.activeSelf)
       {
           return EstadoVisibilidadNodoCampania.Oculto;
       }

       if (nodo == nodoActual || nodo.nodoDespejado)
       {
           return EstadoVisibilidadNodoCampania.Visitado;
       }

       if (PosicionDentroDeVision(nodo.transform.position, centro, radio)
           || EsNodoContiguoAlActual(nodo))
       {
           return EstadoVisibilidadNodoCampania.EnVision;
       }

       return nodo.FueDescubiertoPorMecanicaEspecial()
           ? EstadoVisibilidadNodoCampania.DescubiertoRemoto
           : EstadoVisibilidadNodoCampania.Oculto;
  }

  public bool EsNodoContiguoAlActual(Nodo nodo)
  {
       return nodo != null
           && nodoActual != null
           && nodoActual.DestinosPosibles.Contains(nodo);
  }

  public void RevelarAdyacentesDesdeAsentamiento()
  {
       if (nodoActual == null || nodoActual.tipoNodo != 4)
       {
           return;
       }

       foreach (Nodo destino in nodoActual.DestinosPosibles)
       {
           if (destino == null || !destino.gameObject.activeSelf)
           {
               continue;
           }

           if (NodoDentroDeVision(destino))
           {
               destino.RevelarCompletamente();
           }
           else
           {
               destino.RevelarComoMisterioso();
           }
           destino.MarcarDescubiertoPorMecanicaEspecial();
       }

       RefrescarVisibilidadExploracion();
  }

  HashSet<Nodo> CalcularNodosAlcanzablesDesdeActual()
  {
       HashSet<Nodo> alcanzables = new HashSet<Nodo>();
       Queue<Nodo> pendientes = new Queue<Nodo>();

       if (nodoActual == null || !nodoActual.gameObject.activeSelf)
       {
           return alcanzables;
       }

       alcanzables.Add(nodoActual);
       pendientes.Enqueue(nodoActual);

       while (pendientes.Count > 0)
       {
           Nodo actual = pendientes.Dequeue();
           if (actual == null || actual.DestinosPosibles == null)
           {
               continue;
           }

           foreach (Nodo destino in actual.DestinosPosibles)
           {
               if (destino == null || !destino.gameObject.activeSelf || !alcanzables.Add(destino))
               {
                   continue;
               }

               pendientes.Enqueue(destino);
           }
       }

       return alcanzables;
  }

  public int AplicarEspejismosEnNodosConocidosAlcanzables()
  {
       if (nodoActual == null || TiposNodoEspejismos.Length == 0)
       {
           return 0;
       }

       HashSet<Nodo> alcanzables = CalcularNodosAlcanzablesDesdeActual();
       int nodosAfectados = 0;
       CampaignManager campaign = CampaignManager.Instance;
       bool altarPermitido = campaign != null
           && campaign.scAtributosZona != null
           && campaign.scAtributosZona.ID != 3
           && !campaign.DebeEliminarAltaresPorPresagio();
       int cantidadTiposDisponibles = altarPermitido
           ? TiposNodoEspejismos.Length
           : TiposNodoEspejismos.Length - 1;

       foreach (Nodo nodo in alcanzables)
       {
           if (nodo == null || nodo == nodoActual || !nodo.PuedeCambiarTipoPorEspejismo())
           {
               continue;
           }

           int nuevoTipo = TiposNodoEspejismos[UnityEngine.Random.Range(0, cantidadTiposDisponibles)];
           if (nodo.CambiarTipoPorEspejismo(nuevoTipo))
           {
               nodosAfectados++;
           }
       }

       if (nodosAfectados > 0)
       {
           RefrescarVisibilidadExploracion();
       }

       return nodosAfectados;
  }

  void AplicarEscalaDestinosNoAdyacentes()
  {
       float multiplicadorEscala = CampaignManager.Instance != null
           ? Mathf.Max(0f, CampaignManager.Instance.ObtenerMultiplicadorEscalaNodos())
           : 1f;

       foreach (Nodo nodo in nodosActivosVisibilidad)
       {
           if (nodo == null) continue;
           if (!escalasBaseNodos.TryGetValue(nodo, out Vector3 escalaBase))
           {
               escalaBase = nodo.transform.localScale;
               escalasBaseNodos[nodo] = escalaBase;
           }

           bool quedoAtras = nodoActual != null && nodo.posXNodo < nodoActual.posXNodo;
           bool alternativaColumnaActual = nodoActual != null
               && nodo != nodoActual
               && nodo.posXNodo == nodoActual.posXNodo;
           bool alternativaInalcanzableColumnaSiguiente = nodoActual != null
               && nodo.posXNodo == nodoActual.posXNodo + 1
               && !nodoActual.DestinosPosibles.Contains(nodo);
           bool quedoInalcanzable = quedoAtras
               || alternativaColumnaActual
               || alternativaInalcanzableColumnaSiguiente;
           bool fueVisitadoPorCaravana = nodo.posXNodo == 0 || nodo.nodoDespejado;
           nodo.transform.localScale = escalaBase * multiplicadorEscala * (quedoInalcanzable ? EscalaNodoInaccesible : 1f);
           nodo.AplicarEstadoVisualInaccesible(
               (quedoInalcanzable && !fueVisitadoPorCaravana)
               || nodo.EsDescubiertoRemoto());
       }

       if (nodoActual == null) return;

       foreach (Nodo destino in nodoActual.DestinosPosibles)
       {
           if (destino == null || destino.posXNodo - nodoActual.posXNodo == 1) continue;
           if (!escalasBaseNodos.TryGetValue(destino, out Vector3 escalaBase)) continue;

           destino.transform.localScale = escalaBase * multiplicadorEscala * EscalaDestinoNoAdyacente;
       }
  }

    public void ResetearYGenerarSiguienteZona()
    {
        ReiniciarEstadoVariedadMapa();
        settlementsForzados.Clear();

        // 1) Reactivar todos y resetear estado por nodo
        foreach (Nodo n in scContenedordeNodos.GetComponentsInChildren<Nodo>(true))
        {
            n.gameObject.SetActive(true);
            n.ResetearParaNuevaZona();
        }

        // 2) Reconstruir la lista del contenedor (se había podado previamente)
        scContenedordeNodos.RecolectarNodos();

        //3) Eliminar adornos de la zona anterior
        GameObject[] objetosMapa = GameObject.FindGameObjectsWithTag("MapaObjeto");
        foreach (GameObject objeto in objetosMapa)
        {
            Destroy(objeto);
        }
    }

    IEnumerator GenerarNodosDiferido()
    {
        generacionDiferidaPendiente = true;
        yield return null;
        generacionDiferidaPendiente = false;
        GenerarNodos();
    }

    public void OmitirAutoGeneracionEnStart()
    {
        omitirAutoGeneracionEnStart = true;
    }

    public void RehabilitarAutoGeneracionEnStart()
    {
        omitirAutoGeneracionEnStart = false;
    }

    void PrepararNodosParaGeneracion()
    {
        MapDecorator mapDecorator = ObtenerMapDecorator();
        float multiplicadorEscala = 1f;

        if (CampaignManager.Instance != null)
        {
            multiplicadorEscala = Mathf.Max(0f, CampaignManager.Instance.ObtenerMultiplicadorEscalaNodos());
        }

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null) continue;

            if (!escalasBaseNodos.TryGetValue(nodo, out Vector3 escalaBase))
            {
                escalaBase = nodo.transform.localScale;
                escalasBaseNodos[nodo] = escalaBase;
            }

            AplicarVariacionPosicionNodo(nodo, mapDecorator);
            nodo.transform.localScale = escalaBase * multiplicadorEscala;
            if (mapDecorator != null)
            {
                mapDecorator.AlinearTransformASuelo(nodo.transform, OffsetNodoSobreRelieve);
            }
        }
    }

    public void AlinearNodosAlSueloActual()
    {
        if (scContenedordeNodos == null)
            return;

        scContenedordeNodos.RecolectarNodos();
        MapDecorator mapDecorator = ObtenerMapDecorator();
        if (mapDecorator == null)
            return;

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null)
                continue;

            AplicarVariacionPosicionNodo(nodo, mapDecorator);
            mapDecorator.AlinearTransformASuelo(nodo.transform, OffsetNodoSobreRelieve);
        }
    }

    void AplicarVariacionPosicionNodo(Nodo nodo, MapDecorator mapDecorator)
    {
        if (nodo == null)
            return;

        if (!posicionesBaseLocalesNodos.TryGetValue(nodo, out Vector3 posicionBaseLocal))
        {
            posicionBaseLocal = nodo.transform.localPosition;
            posicionesBaseLocalesNodos[nodo] = posicionBaseLocal;
        }

        nodo.transform.localPosition = ObtenerPosicionNodoConVariacion(nodo, posicionBaseLocal, mapDecorator);
    }

    Vector3 ObtenerPosicionNodoConVariacion(Nodo nodo, Vector3 posicionBaseLocal, MapDecorator mapDecorator)
    {
        float rango = Mathf.Max(0f, rangoAleatorioNodoXZ);
        if (nodo == null || rango <= 0f)
            return posicionBaseLocal;

        int reliefSeed = mapDecorator != null ? mapDecorator.GetReliefSeed() : 0;
        int seedNodo = reliefSeed ^ (nodo.posXNodo * 73856093) ^ (nodo.posYNodo * 19349663) ^ 0x4F1BBCDC;
        System.Random random = new System.Random(seedNodo);
        double angulo = random.NextDouble() * Mathf.PI * 2f;
        double radio = Math.Sqrt(random.NextDouble()) * rango;

        posicionBaseLocal.x += (float)(Math.Cos(angulo) * radio);
        posicionBaseLocal.z += (float)(Math.Sin(angulo) * radio);
        return posicionBaseLocal;
    }

    MapDecorator ObtenerMapDecorator()
    {
        if (CampaignManager.Instance == null || CampaignManager.Instance.scAtributosZona == null)
            return null;

        return CampaignManager.Instance.scAtributosZona.GetComponent<MapDecorator>();
    }

    void AlinearConvoyAlSuelo()
    {
        MapDecorator mapDecorator = ObtenerMapDecorator();
        if (mapDecorator == null)
            return;

        mapDecorator.AlinearTransformASuelo(goCaravana != null ? goCaravana.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower1 != null ? goCaravanafollower1.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower2 != null ? goCaravanafollower2.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower3 != null ? goCaravanafollower3.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower4 != null ? goCaravanafollower4.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower5 != null ? goCaravanafollower5.transform : null, OffsetConvoySobreRelieve);
        mapDecorator.AlinearTransformASuelo(goCaravanafollower6 != null ? goCaravanafollower6.transform : null, OffsetConvoySobreRelieve);
    }

    public void PosicionarCaravanaEnNodoActual()
    {
        if (nodoActual != null && goCaravana != null)
        {
            nodoActual.PosicionarObjetoEnNodo(goCaravana);
        }

        AlinearConvoyAlSuelo();
    }

    void DesactivarNodosSinUsar(int zonaId)
    {
       
        for (int i = scContenedordeNodos.listTodosNodos.Count - 1; i >= 0; i--)
        {   
            Nodo n = scContenedordeNodos.listTodosNodos[i];
            bool prohibido = n.ProhibidoEnZona != null && n.ProhibidoEnZona.Contains(zonaId);
            if (prohibido || (!n.yatiroConexiones && n.posXNodo != 11))
            {   
                n.gameObject.SetActive(false);
                scContenedordeNodos.listTodosNodos.RemoveAt(i);
            }
        }

    }

    void AplicarPresagiosInicialesDeNodos()
    {
        if (scContenedordeNodos == null)
        {
            return;
        }

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo != null && nodo.gameObject.activeSelf)
            {
                nodo.AplicarPresagiosInicialesDeNodos();
            }
        }
    }

    void AplicarZonaCartografiadaSiCorresponde()
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null
            || !campaign.TienePresagioActivo(PresagioCatalog.ZonaCartografiada)
            || scContenedordeNodos == null)
        {
            return;
        }

        Nodo origen = scContenedordeNodos.ObtenerNodoSegunXY(0, 0);
        Nodo destinoFinal = scContenedordeNodos.listTodosNodos.Find(
            nodo => nodo != null && nodo.gameObject.activeSelf && nodo.posXNodo == 11);
        if (origen == null || destinoFinal == null)
        {
            return;
        }

        List<Nodo> ruta = new List<Nodo>();
        if (!IntentarConstruirRutaCartografiada(origen, destinoFinal, new HashSet<Nodo>(), ruta))
        {
            Debug.LogWarning("[Presagios] No se pudo construir la ruta de Región Cartografiada.");
            return;
        }

        for (int i = 0; i < ruta.Count; i++)
        {
            ruta[i].RevelarPorZonaCartografiada();
        }
    }

    bool IntentarConstruirRutaCartografiada(
        Nodo actual,
        Nodo destinoFinal,
        HashSet<Nodo> visitados,
        List<Nodo> ruta)
    {
        if (actual == null || !actual.gameObject.activeSelf || !visitados.Add(actual))
        {
            return false;
        }

        ruta.Add(actual);
        if (actual == destinoFinal)
        {
            return true;
        }

        List<Nodo> candidatos = new List<Nodo>();
        foreach (Nodo destino in actual.DestinosPosibles)
        {
            if (destino != null && destino.gameObject.activeSelf && !visitados.Contains(destino))
            {
                candidatos.Add(destino);
            }
        }

        for (int i = candidatos.Count - 1; i > 0; i--)
        {
            int indiceAleatorio = UnityEngine.Random.Range(0, i + 1);
            (candidatos[i], candidatos[indiceAleatorio]) = (candidatos[indiceAleatorio], candidatos[i]);
        }

        foreach (Nodo candidato in candidatos)
        {
            if (IntentarConstruirRutaCartografiada(candidato, destinoFinal, visitados, ruta))
            {
                return true;
            }
        }

        ruta.RemoveAt(ruta.Count - 1);
        return false;
    }

    void ForzarNodosObligatorios(int zonaId)
    {
        if (scContenedordeNodos == null) return;

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null) continue;
            if (nodo.ProhibidoEnZona != null && nodo.ProhibidoEnZona.Contains(zonaId)) continue;
            if (nodo.ObligatorioEnZona == null || !nodo.ObligatorioEnZona.Contains(zonaId)) continue;
            if (nodo.yatiroConexiones) continue;

            Nodo origen = BuscarNodoConectadoMasCercano(nodo, zonaId);
            if (origen != null)
            {
                origen.ConectarConNodo(nodo);
            }
        }
    }

    Nodo BuscarNodoConectadoMasCercano(Nodo objetivo, int zonaId)
    {
        if (scContenedordeNodos == null) return null;

        Nodo candidato = null;
        int menorDistanciaX = int.MaxValue;
        int menorDistanciaY = int.MaxValue;

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null) continue;
            if (!nodo.yatiroConexiones) continue;
            if (nodo.ProhibidoEnZona != null && nodo.ProhibidoEnZona.Contains(zonaId)) continue;

            int distanciaX = objetivo.posXNodo - nodo.posXNodo;
            if (distanciaX != 1) continue;
            int distanciaY = Mathf.Abs(objetivo.posYNodo - nodo.posYNodo);

            bool esMejor = false;

            if (distanciaY < menorDistanciaY)
            {
                esMejor = true;
            }
            else if (distanciaY == menorDistanciaY && distanciaX < menorDistanciaX)
            {
                esMejor = true;
            }

            if (esMejor)
            {
                menorDistanciaX = distanciaX;
                menorDistanciaY = distanciaY;
                candidato = nodo;
                if (distanciaY == 0 && distanciaX == 1) break;
            }
        }

        return candidato;
    }

    struct SettlementPair
    {
        public Nodo temprano;
        public Nodo tardio;
        public int diferenciaY;

        public SettlementPair(Nodo settlementTemprano, Nodo settlementTardio)
        {
            temprano = settlementTemprano;
            tardio = settlementTardio;
            diferenciaY = Mathf.Abs(settlementTemprano.posYNodo - settlementTardio.posYNodo);
        }
    }

    public void NotificarFinViajeCaravana()
    {
        viajesDesdeUltimaEmboscadaSubterranea = Mathf.Min(viajesDesdeUltimaEmboscadaSubterranea + 1, 99);

        if (CampaignManager.Instance != null && CampaignManager.Instance.DebeUsarConfiguracionTutorial())
        {
            settlementsForzados.Clear();
            return;
        }

        if (nodoActual == null || settlementsForzados.Count == 0)
        {
            return;
        }

        foreach (Nodo settlement in settlementsForzados)
        {
            if (settlement == null || !settlement.gameObject.activeInHierarchy || settlement.revelado)
            {
                continue;
            }

            int distancia = CalcularDistanciaEnViajes(nodoActual, settlement, DistanciaReveladoSettlement);
            if (distancia < 0 || distancia > DistanciaReveladoSettlement)
            {
                continue;
            }

            settlement.ForzarSettlement(true);
            BanterCampaignDirector.NotificarNodoRevelado(settlement);
            MarcarRutaCaminoAAldea(nodoActual, settlement);
            settlement.ActivarVfxDescubrimiento();

            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.EscribirAdvertenciaLog(ObtenerLogSettlementDescubierto());
            }
        }
    }

    string ObtenerLogSettlementDescubierto()
    {
        if (TRADU.i == null)
        {
            return LogSettlementDescubiertoEs;
        }

        return TRADU.i.nIdioma switch
        {
            TRADU.IdiomaIngles => LogSettlementDescubiertoEn,
            TRADU.IdiomaPortugues => LogSettlementDescubiertoPt,
            _ => LogSettlementDescubiertoEs
        };
    }

    int CalcularDistanciaEnViajes(Nodo origen, Nodo objetivo, int distanciaMaxima)
    {
        if (origen == null || objetivo == null)
        {
            return -1;
        }

        if (origen == objetivo)
        {
            return 0;
        }

        HashSet<Nodo> visitados = new HashSet<Nodo>();
        Queue<Nodo> colaNodos = new Queue<Nodo>();
        Queue<int> colaDistancias = new Queue<int>();

        visitados.Add(origen);
        colaNodos.Enqueue(origen);
        colaDistancias.Enqueue(0);

        while (colaNodos.Count > 0)
        {
            Nodo actual = colaNodos.Dequeue();
            int distanciaActual = colaDistancias.Dequeue();

            if (distanciaActual >= distanciaMaxima || actual.DestinosPosibles == null)
            {
                continue;
            }

            foreach (Nodo destino in actual.DestinosPosibles)
            {
                if (destino == null || !destino.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!visitados.Add(destino))
                {
                    continue;
                }

                int distanciaDestino = distanciaActual + 1;
                if (destino == objetivo)
                {
                    return distanciaDestino;
                }

                colaNodos.Enqueue(destino);
                colaDistancias.Enqueue(distanciaDestino);
            }
        }

        return -1;
    }

    void MarcarRutaCaminoAAldea(Nodo origen, Nodo settlement)
    {
        List<Nodo> ruta = CalcularRutaEnViajes(origen, settlement, int.MaxValue);
        if (ruta == null || ruta.Count < 2)
        {
            return;
        }

        for (int i = 0; i < ruta.Count - 1; i++)
        {
            Nodo nodoRuta = ruta[i];
            Nodo siguienteNodo = ruta[i + 1];
            if (nodoRuta == null || siguienteNodo == null)
            {
                continue;
            }

            nodoRuta.MarcarCaminoAAldeaHacia(siguienteNodo);
        }
    }

    List<Nodo> CalcularRutaEnViajes(Nodo origen, Nodo objetivo, int distanciaMaxima)
    {
        if (origen == null || objetivo == null)
        {
            return null;
        }

        if (origen == objetivo)
        {
            return new List<Nodo> { origen };
        }

        HashSet<Nodo> visitados = new HashSet<Nodo>();
        Queue<Nodo> colaNodos = new Queue<Nodo>();
        Queue<int> colaDistancias = new Queue<int>();
        Dictionary<Nodo, Nodo> predecesores = new Dictionary<Nodo, Nodo>();

        visitados.Add(origen);
        colaNodos.Enqueue(origen);
        colaDistancias.Enqueue(0);

        while (colaNodos.Count > 0)
        {
            Nodo actual = colaNodos.Dequeue();
            int distanciaActual = colaDistancias.Dequeue();

            if (distanciaActual >= distanciaMaxima || actual.DestinosPosibles == null)
            {
                continue;
            }

            foreach (Nodo destino in actual.DestinosPosibles)
            {
                if (destino == null || !destino.gameObject.activeInHierarchy || !visitados.Add(destino))
                {
                    continue;
                }

                predecesores[destino] = actual;
                if (destino == objetivo)
                {
                    return ReconstruirRuta(predecesores, origen, objetivo);
                }

                colaNodos.Enqueue(destino);
                colaDistancias.Enqueue(distanciaActual + 1);
            }
        }

        return null;
    }

    static List<Nodo> ReconstruirRuta(Dictionary<Nodo, Nodo> predecesores, Nodo origen, Nodo objetivo)
    {
        List<Nodo> ruta = new List<Nodo>();
        Nodo actual = objetivo;

        ruta.Add(actual);
        while (actual != origen)
        {
            if (!predecesores.TryGetValue(actual, out Nodo anterior))
            {
                return null;
            }

            actual = anterior;
            ruta.Add(actual);
        }

        ruta.Reverse();
        return ruta;
    }

    public bool TirarEmboscadaSubterraneaAtajo(Nodo destino)
    {
        CampaignManager campaignManager = CampaignManager.Instance;
        if (campaignManager != null && campaignManager.DebeUsarConfiguracionTutorial())
        {
            return false;
        }

        if (destino == null)
        {
            return false;
        }

        if (destino.tipoNodo == 4 || destino.tipoNodo == 10 || destino.tipoNodo == 15 || destino.tipoNodo == 16)
        {
            return false;
        }

        if (viajesDesdeUltimaEmboscadaSubterranea < 3)
        {
            return false;
        }

        int chance = 20;
        if (emboscadasSubterraneasZona >= 1)
        {
            chance = Mathf.RoundToInt(chance * 0.45f);
        }

        if (destino.posXNodo >= 8)
        {
            chance += 4;
        }

        chance = Mathf.Clamp(chance, 0, 100);
        if (UnityEngine.Random.Range(0, 100) >= chance)
        {
            return false;
        }

        emboscadasSubterraneasZona++;
        viajesDesdeUltimaEmboscadaSubterranea = 0;
        return true;
    }

    public int ObtenerEmboscadasSubterraneasZona()
    {
        return emboscadasSubterraneasZona;
    }

    public int ObtenerViajesDesdeUltimaEmboscadaSubterranea()
    {
        return viajesDesdeUltimaEmboscadaSubterranea;
    }

    public IReadOnlyList<Nodo> ObtenerSettlementsForzados()
    {
        return settlementsForzados;
    }

    public void RestaurarSettlementsForzados(IEnumerable<Nodo> settlements)
    {
        settlementsForzados.Clear();
        if (settlements == null)
        {
            return;
        }

        foreach (Nodo settlement in settlements)
        {
            RegistrarSettlementForzado(settlement);
        }
    }

    public void RestaurarEstadoVariedadDesdeSave(MapSaveData data)
    {
        ReiniciarEstadoVariedadMapa();

        if (data != null)
        {
            emboscadasSubterraneasZona = Mathf.Max(0, data.emboscadasSubterraneasZona);
            viajesDesdeUltimaEmboscadaSubterranea = Mathf.Clamp(data.viajesDesdeUltimaEmboscadaSubterranea, 0, 99);
        }
    }

    void ReiniciarEstadoVariedadMapa()
    {
        emboscadasSubterraneasZona = 0;
        viajesDesdeUltimaEmboscadaSubterranea = 99;
    }

    void ForzarSettlementsPorMapa(int zonaId)
    {
        if (scContenedordeNodos == null)
        {
            return;
        }

        settlementsForzados.Clear();
        if (CampaignManager.Instance != null && CampaignManager.Instance.DebeUsarConfiguracionTutorial())
        {
            return;
        }

        List<Nodo> candidatosTardios = ObtenerCandidatosSettlement(zonaId, MinXSettlementTardio, MaxXSettlementTardio);
        if (CampaignManager.Instance != null
            && CampaignManager.Instance.TienePresagioActivo(PresagioCatalog.PobladosEscasos))
        {
            if (!IntentarAplicarSettlementUnico(candidatosTardios, zonaId))
            {
                List<Nodo> candidatosFallback = ObtenerCandidatosSettlement(
                    zonaId,
                    MinXSettlementTemprano,
                    MaxXSettlementTardio);
                IntentarAplicarSettlementUnico(candidatosFallback, zonaId);
            }
            return;
        }

        List<Nodo> candidatosTempranos = ObtenerCandidatosSettlement(zonaId, MinXSettlementTemprano, MaxXSettlementTemprano);
        List<SettlementPair> paresConDiferenciaMinima = ConstruirParesSettlement(candidatosTempranos, candidatosTardios, true);
        if (IntentarAplicarParSettlement(paresConDiferenciaMinima, zonaId))
        {
            return;
        }

        List<SettlementPair> paresFallback = ConstruirParesSettlement(candidatosTempranos, candidatosTardios, false);
        paresFallback.Sort((a, b) => b.diferenciaY.CompareTo(a.diferenciaY));
        if (IntentarAplicarParSettlement(paresFallback, zonaId))
        {
            Debug.LogWarning("No se pudo cumplir diferencia minima de Y=2 para settlements. Se uso el mejor par disponible.");
        }
    }

    bool IntentarAplicarSettlementUnico(List<Nodo> candidatos, int zonaId)
    {
        if (candidatos == null || candidatos.Count == 0)
        {
            return false;
        }

        List<Nodo> pendientes = new List<Nodo>(candidatos);
        while (pendientes.Count > 0)
        {
            int indice = UnityEngine.Random.Range(0, pendientes.Count);
            Nodo settlement = pendientes[indice];
            pendientes.RemoveAt(indice);

            if (!PuedeConectarseNodo(settlement, zonaId) || !AsegurarNodoConectado(settlement, zonaId))
            {
                continue;
            }

            settlementsForzados.Clear();
            RegistrarSettlementForzado(settlement);
            settlement.ForzarSettlement(false);
            return true;
        }

        return false;
    }

    List<Nodo> ObtenerCandidatosSettlement(int zonaId, int minX, int maxX)
    {
        List<Nodo> candidatos = new List<Nodo>();

        if (scContenedordeNodos == null)
        {
            return candidatos;
        }

        foreach (Nodo nodo in scContenedordeNodos.listTodosNodos)
        {
            if (!EsNodoValidoParaSettlement(nodo, zonaId))
            {
                continue;
            }

            if (nodo.posXNodo < minX || nodo.posXNodo > maxX)
            {
                continue;
            }

            candidatos.Add(nodo);
        }

        return candidatos;
    }

    List<SettlementPair> ConstruirParesSettlement(List<Nodo> candidatosTempranos, List<Nodo> candidatosTardios, bool exigirDiferenciaMinimaY)
    {
        List<SettlementPair> pares = new List<SettlementPair>();

        if (candidatosTempranos == null || candidatosTardios == null)
        {
            return pares;
        }

        foreach (Nodo settlementTemprano in candidatosTempranos)
        {
            foreach (Nodo settlementTardio in candidatosTardios)
            {
                if (settlementTemprano == null || settlementTardio == null || settlementTemprano == settlementTardio)
                {
                    continue;
                }

                SettlementPair par = new SettlementPair(settlementTemprano, settlementTardio);
                if (exigirDiferenciaMinimaY && par.diferenciaY < MinDiferenciaYSettlements)
                {
                    continue;
                }

                pares.Add(par);
            }
        }

        return pares;
    }

    bool IntentarAplicarParSettlement(List<SettlementPair> pares, int zonaId)
    {
        if (pares == null || pares.Count == 0)
        {
            return false;
        }

        List<SettlementPair> paresPendientes = new List<SettlementPair>(pares);
        while (paresPendientes.Count > 0)
        {
            int indicePar = UnityEngine.Random.Range(0, paresPendientes.Count);
            SettlementPair par = paresPendientes[indicePar];
            paresPendientes.RemoveAt(indicePar);

            if (!PuedeConectarseNodo(par.temprano, zonaId) || !PuedeConectarseNodo(par.tardio, zonaId))
            {
                continue;
            }

            if (!AsegurarNodoConectado(par.temprano, zonaId) || !AsegurarNodoConectado(par.tardio, zonaId))
            {
                continue;
            }

            settlementsForzados.Clear();
            RegistrarSettlementForzado(par.temprano);
            RegistrarSettlementForzado(par.tardio);

            par.temprano.ForzarSettlement(false);
            par.tardio.ForzarSettlement(false);
            return true;
        }

        return false;
    }

    bool PuedeConectarseNodo(Nodo nodo, int zonaId)
    {
        if (nodo == null)
        {
            return false;
        }

        if (nodo.yatiroConexiones)
        {
            return true;
        }

        return BuscarNodoConectadoMasCercano(nodo, zonaId) != null;
    }

    bool AsegurarNodoConectado(Nodo nodo, int zonaId)
    {
        if (nodo == null)
        {
            return false;
        }

        if (nodo.yatiroConexiones)
        {
            return true;
        }

        Nodo origen = BuscarNodoConectadoMasCercano(nodo, zonaId);
        if (origen == null)
        {
            return false;
        }

        origen.ConectarConNodo(nodo);
        return nodo.yatiroConexiones;
    }

    void RegistrarSettlementForzado(Nodo nodo)
    {
        if (nodo == null || settlementsForzados.Contains(nodo))
        {
            return;
        }

        settlementsForzados.Add(nodo);
    }

    bool EsNodoValidoParaSettlement(Nodo nodo, int zonaId)
    {
        if (nodo == null)
        {
            return false;
        }

        if (nodo.posXNodo <= 0 || nodo.posXNodo >= 11)
        {
            return false;
        }

        if (nodo.ProhibidoEnZona != null && nodo.ProhibidoEnZona.Contains(zonaId))
        {
            return false;
        }

        return true;
    }
}
