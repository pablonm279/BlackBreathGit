using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Autoridad visual de la niebla de guerra del mapa de campaña.
/// El alcance lógico y el historial se expresan en coordenadas XZ del mapa.
/// La apertura se proyecta como un óvalo isométrico estable para que conserve
/// lectura radial con la cámara inclinada.
/// </summary>
[DisallowMultipleComponent]
public sealed class NieblaGuerraCaravana : MonoBehaviour
{
    const string RutaShader = "NieblaGuerraCaravana";
    const int AnchoHistorial = 192;
    const int AltoHistorial = 128;
    const float IntervaloMinimoHuella = 1f / 15f;
    const float IntervaloComprobacionHistorial = 0.25f;
    const float FraccionPasoEntreHuellas = 0.035f;
    const float MargenBoundsEnPasos = 2f;

    const float AplastamientoVisualIsometrico = 0.62f;
    const float DuracionOndaApertura = 1.10f;

    static readonly int IdVisionPantalla = Shader.PropertyToID("_VisionScreen");
    static readonly int IdSuavizadoBorde = Shader.PropertyToID("_VisionFeather");
    static readonly int IdTexturaHistorial = Shader.PropertyToID("_HistoryTex");
    static readonly int IdBoundsHistorial = Shader.PropertyToID("_HistoryBounds");
    static readonly int IdFuerzaHistorial = Shader.PropertyToID("_HistoryStrength");
    static readonly int IdOpacidad = Shader.PropertyToID("_Opacity");
    static readonly int IdParametrosBorde = Shader.PropertyToID("_EdgeParams");
    static readonly int IdTiempoBorde = Shader.PropertyToID("_EdgeTime");
    static readonly int IdOndaBorde = Shader.PropertyToID("_EdgeWave");
    static readonly int IdCaminoVisionPantalla = Shader.PropertyToID("_CaminoVisionPantalla");
    static readonly int IdCaminoVisionSuavizado = Shader.PropertyToID("_CaminoVisionSuavizado");
    static readonly int IdCaminoVisionActiva = Shader.PropertyToID("_CaminoVisionActiva");
    static readonly int IdCaminoVisionDebug = Shader.PropertyToID("_CaminoVisionDebug");

    readonly HashSet<int> nodosIncorporados = new HashSet<int>();
    readonly HashSet<int> caminosIncorporados = new HashSet<int>();

    MapaManager mapaManager;
    Camera camaraCampania;
    NieblaGuerraCaravanaRender renderNiebla;
    Material materialNiebla;
    Texture2D texturaHistorial;
    byte[] pixelesHistorial;

    Vector2 minimoHistorial;
    Vector2 tamanoHistorial;
    Vector3 ultimaPosicionHuella;
    float ultimoRadioHuella = -1f;
    float siguienteHuella;
    float siguienteComprobacionHistorial;
    float opacidadSuavizada;
    float velocidadOpacidad;
    bool huellaInicializada;
    bool boundsInicializados;
    int cantidadNodosBounds = -1;
    int zonaBounds = int.MinValue;
    float radioAnteriorBorde = -1f;
    float inicioOndaBorde = float.NegativeInfinity;
    float intensidadOndaBorde;

    public bool PuedeRenderizarNiebla =>
        isActiveAndEnabled
        && materialNiebla != null
        && texturaHistorial != null
        && opacidadSuavizada > 0.001f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InstalarEnCargaDeEscenas()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstalarEnEscenaInicial()
    {
        AsegurarControladoresEnEscena();
    }

    static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        AsegurarControladoresEnEscena();
    }

    static void AsegurarControladoresEnEscena()
    {
        MapaManager[] mapas = UnityEngine.Object.FindObjectsOfType<MapaManager>();
        for (int i = 0; i < mapas.Length; i++)
        {
            MapaManager mapa = mapas[i];
            if (mapa != null && mapa.GetComponent<NieblaGuerraCaravana>() == null)
            {
                mapa.gameObject.AddComponent<NieblaGuerraCaravana>();
            }
        }
    }

    void Awake()
    {
        mapaManager = GetComponent<MapaManager>();
    }

    void OnEnable()
    {
        CrearRecursosSiHaceFalta();
    }

    void LateUpdate()
    {
        if (mapaManager == null)
        {
            mapaManager = GetComponent<MapaManager>();
        }

        CrearRecursosSiHaceFalta();
        Camera camara = ObtenerCamaraCampania();
        if (mapaManager == null || mapaManager.goCaravana == null
            || materialNiebla == null || camara == null)
        {
            DesactivarRenderTemporalmente();
            return;
        }

        AsegurarRenderEnCamara(camara);
        if (!AsegurarBoundsYTextura())
        {
            DesactivarRenderTemporalmente();
            return;
        }

        Vector3 centro = mapaManager.ObtenerCentroVisionActual();
        float radio = mapaManager.ObtenerRadioVisionActual();
        float pasoMapa = mapaManager.ObtenerPasoMapa();
        float suavizadoBorde = Mathf.Max(0.12f, pasoMapa * 0.16f);

        // Sin SmoothDamp: el óvalo queda centrado en la caravana y cambia en el
        // mismo frame que el alcance lógico al mejorar el catalejo.
        if (!CalcularVisionEnPantalla(camara, centro, radio, out Vector4 visionPantalla))
        {
            DesactivarRenderTemporalmente();
            return;
        }

        materialNiebla.SetVector(IdVisionPantalla, visionPantalla);
        materialNiebla.SetFloat(
            IdSuavizadoBorde,
            Mathf.Clamp(suavizadoBorde / Mathf.Max(0.001f, radio), 0.04f, 0.22f));
        ActualizarBordeVivo(radio, pasoMapa);
        ActualizarMascaraCaminos(
            visionPantalla,
            Mathf.Clamp(suavizadoBorde / Mathf.Max(0.001f, radio), 0.04f, 0.22f));

        bool introActiva = CampaignManager.Instance != null
            && CampaignManager.Instance.IntroCampaniaActivaOPendiente;
        ActualizarOpacidad(introActiva ? 0f : 1f);

        ActualizarHuellaDeMovimiento(centro, radio, pasoMapa);
        if (Time.unscaledTime >= siguienteComprobacionHistorial)
        {
            siguienteComprobacionHistorial =
                Time.unscaledTime + IntervaloComprobacionHistorial;
            if (IncorporarNodosYCaminosRecorridos(radio, pasoMapa))
            {
                SubirTexturaHistorial();
            }
        }

        if (renderNiebla != null)
        {
            renderNiebla.enabled = true;
        }
    }

    void CrearRecursosSiHaceFalta()
    {
        if (materialNiebla != null)
        {
            return;
        }

        Shader shader = Resources.Load<Shader>(RutaShader);
        if (shader == null)
        {
            Debug.LogWarning(
                "[NieblaGuerraCaravana] No se encontró el shader Resources/"
                + RutaShader + ".shader.");
            enabled = false;
            return;
        }

        materialNiebla = new Material(shader)
        {
            name = "Niebla de guerra - Instancia",
            hideFlags = HideFlags.DontSave
        };
        materialNiebla.SetFloat(IdFuerzaHistorial, 0.50f);
        materialNiebla.SetFloat(IdOpacidad, 0f);
        materialNiebla.SetVector(IdParametrosBorde, new Vector4(0.085f, 0.045f, 1f, 0f));
        materialNiebla.SetVector(IdOndaBorde, new Vector4(1f, 0f, 0f, 0f));
    }

    void ActualizarBordeVivo(float radio, float pasoMapa)
    {
        materialNiebla.SetFloat(IdTiempoBorde, Time.unscaledTime);

        if (radioAnteriorBorde < 0f)
        {
            radioAnteriorBorde = radio;
        }
        else
        {
            float aumento = radio - radioAnteriorBorde;
            if (aumento > Mathf.Max(0.01f, pasoMapa * 0.015f))
            {
                inicioOndaBorde = Time.unscaledTime;
                intensidadOndaBorde = Mathf.Clamp01(aumento / Mathf.Max(0.01f, pasoMapa * 0.75f));
            }
            radioAnteriorBorde = radio;
        }

        float progreso = Mathf.Clamp01(
            (Time.unscaledTime - inicioOndaBorde) / DuracionOndaApertura);
        float fuerza = intensidadOndaBorde * (1f - progreso) * (1f - progreso);
        materialNiebla.SetVector(
            IdOndaBorde,
            new Vector4(progreso, fuerza, 0f, 0f));
    }

    bool CalcularVisionEnPantalla(
        Camera camara,
        Vector3 centro,
        float radio,
        out Vector4 visionPantalla)
    {
        visionPantalla = Vector4.zero;
        Vector3 centroViewport = camara.WorldToViewportPoint(centro);
        if (centroViewport.z <= 0f)
        {
            return false;
        }

        Vector3 ejeHorizontalMundo = Vector3.ProjectOnPlane(
            camara.transform.right,
            Vector3.up).normalized;
        if (ejeHorizontalMundo.sqrMagnitude < 0.001f)
        {
            ejeHorizontalMundo = Vector3.right;
        }

        Vector3 extremoA = camara.WorldToViewportPoint(
            centro - ejeHorizontalMundo * radio);
        Vector3 extremoB = camara.WorldToViewportPoint(
            centro + ejeHorizontalMundo * radio);
        if (extremoA.z <= 0f || extremoB.z <= 0f)
        {
            return false;
        }

        float anchoPantalla = Mathf.Max(1f, camara.pixelWidth);
        float altoPantalla = Mathf.Max(1f, camara.pixelHeight);
        Vector2 aPixeles = new Vector2(
            extremoA.x * anchoPantalla,
            extremoA.y * altoPantalla);
        Vector2 bPixeles = new Vector2(
            extremoB.x * anchoPantalla,
            extremoB.y * altoPantalla);
        float radioPixeles = Mathf.Max(2f, Vector2.Distance(aPixeles, bPixeles) * 0.5f);

        visionPantalla = new Vector4(
            centroViewport.x,
            centroViewport.y,
            radioPixeles / anchoPantalla,
            radioPixeles * AplastamientoVisualIsometrico / altoPantalla);
        return true;
    }

    bool AsegurarBoundsYTextura()
    {
        if (mapaManager.scContenedordeNodos == null)
        {
            return false;
        }

        if (mapaManager.scContenedordeNodos.listTodosNodos.Count == 0)
        {
            mapaManager.scContenedordeNodos.RecolectarNodos();
        }

        int cantidadNodos = mapaManager.scContenedordeNodos.listTodosNodos.Count;
        if (cantidadNodos == 0)
        {
            return false;
        }

        int zonaActual = ObtenerZonaActual();
        if (boundsInicializados
            && cantidadNodos == cantidadNodosBounds
            && zonaActual == zonaBounds)
        {
            return true;
        }

        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;
        bool encontroPunto = false;

        foreach (Nodo nodo in mapaManager.scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null || !nodo.gameObject.activeSelf)
            {
                continue;
            }

            IncluirEnBounds(nodo.transform.position, ref minX, ref minZ, ref maxX, ref maxZ);
            encontroPunto = true;
        }

        if (!encontroPunto)
        {
            return false;
        }

        float paso = mapaManager.ObtenerPasoMapa();
        float margen = Mathf.Max(1f, paso * MargenBoundsEnPasos);
        minX -= margen;
        minZ -= margen;
        maxX += margen;
        maxZ += margen;

        minimoHistorial = new Vector2(minX, minZ);
        tamanoHistorial = new Vector2(
            Mathf.Max(1f, maxX - minX),
            Mathf.Max(1f, maxZ - minZ));
        materialNiebla.SetVector(
            IdBoundsHistorial,
            new Vector4(
                minimoHistorial.x,
                minimoHistorial.y,
                1f / tamanoHistorial.x,
                1f / tamanoHistorial.y));

        if (texturaHistorial == null)
        {
            texturaHistorial = new Texture2D(
                AnchoHistorial,
                AltoHistorial,
                TextureFormat.R8,
                false,
                true)
            {
                name = "Mascara mundial de niebla de guerra",
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            pixelesHistorial = new byte[AnchoHistorial * AltoHistorial];
            materialNiebla.SetTexture(IdTexturaHistorial, texturaHistorial);
        }

        Array.Clear(pixelesHistorial, 0, pixelesHistorial.Length);
        nodosIncorporados.Clear();
        caminosIncorporados.Clear();
        huellaInicializada = false;
        cantidadNodosBounds = cantidadNodos;
        zonaBounds = zonaActual;
        boundsInicializados = true;

        float radioActual = mapaManager.ObtenerRadioVisionActual();
        IncorporarNodosYCaminosRecorridos(radioActual, paso);
        EstamparCirculo(mapaManager.ObtenerCentroVisionActual(), radioActual);
        SubirTexturaHistorial();
        return true;
    }

    void ActualizarHuellaDeMovimiento(Vector3 centro, float radio, float pasoMapa)
    {
        float umbral = Mathf.Max(0.08f, pasoMapa * FraccionPasoEntreHuellas);
        bool cambioRadio = Mathf.Abs(radio - ultimoRadioHuella) > 0.01f;
        bool seMovio = !huellaInicializada
            || DistanciaHorizontal(centro, ultimaPosicionHuella) >= umbral;

        if ((!seMovio && !cambioRadio) || Time.unscaledTime < siguienteHuella)
        {
            return;
        }

        siguienteHuella = Time.unscaledTime + IntervaloMinimoHuella;
        if (EstamparCirculo(centro, radio))
        {
            SubirTexturaHistorial();
        }

        ultimaPosicionHuella = centro;
        ultimoRadioHuella = radio;
        huellaInicializada = true;
    }

    bool IncorporarNodosYCaminosRecorridos(float radioVision, float pasoMapa)
    {
        if (!boundsInicializados || mapaManager.scContenedordeNodos == null)
        {
            return false;
        }

        bool cambio = false;
        float radioReconstruccion = Mathf.Max(pasoMapa * 0.7f, radioVision * 0.92f);
        foreach (Nodo nodo in mapaManager.scContenedordeNodos.listTodosNodos)
        {
            if (nodo == null || !nodo.gameObject.activeSelf)
            {
                continue;
            }

            if ((nodo == mapaManager.nodoActual || nodo.nodoDespejado)
                && nodosIncorporados.Add(nodo.GetInstanceID()))
            {
                cambio |= EstamparCirculo(nodo.transform.position, radioReconstruccion);
            }

            foreach (CaminoConexion conexion in nodo.ConexionesSalientes)
            {
                if (conexion == null || !conexion.recorridoPorCaravana
                    || conexion.linea == null)
                {
                    continue;
                }

                int idCamino = conexion.linea.GetInstanceID();
                if (caminosIncorporados.Add(idCamino))
                {
                    cambio |= EstamparCamino(
                        conexion.linea.GetComponent<LineRenderer>(),
                        radioReconstruccion,
                        pasoMapa);
                }
            }
        }

        return cambio;
    }

    bool EstamparCamino(LineRenderer linea, float radio, float pasoMapa)
    {
        if (linea == null || linea.positionCount < 2)
        {
            return false;
        }

        bool cambio = false;
        float separacion = Mathf.Max(0.12f, pasoMapa * 0.12f);
        for (int i = 0; i < linea.positionCount - 1; i++)
        {
            Vector3 a = linea.GetPosition(i);
            Vector3 b = linea.GetPosition(i + 1);
            if (!linea.useWorldSpace)
            {
                a = linea.transform.TransformPoint(a);
                b = linea.transform.TransformPoint(b);
            }

            int pasos = Mathf.Max(1, Mathf.CeilToInt(DistanciaHorizontal(a, b) / separacion));
            for (int paso = 0; paso <= pasos; paso++)
            {
                cambio |= EstamparCirculo(Vector3.Lerp(a, b, paso / (float)pasos), radio);
            }
        }

        return cambio;
    }

    bool EstamparCirculo(Vector3 centro, float radio)
    {
        if (!boundsInicializados || pixelesHistorial == null || radio <= 0f)
        {
            return false;
        }

        float centroPixelX = (centro.x - minimoHistorial.x)
            / tamanoHistorial.x * (AnchoHistorial - 1);
        float centroPixelY = (centro.z - minimoHistorial.y)
            / tamanoHistorial.y * (AltoHistorial - 1);
        float radioPixelX = radio / tamanoHistorial.x * (AnchoHistorial - 1);
        float radioPixelY = radio / tamanoHistorial.y * (AltoHistorial - 1);

        int minX = Mathf.Max(0, Mathf.FloorToInt(centroPixelX - radioPixelX));
        int maxX = Mathf.Min(AnchoHistorial - 1, Mathf.CeilToInt(centroPixelX + radioPixelX));
        int minY = Mathf.Max(0, Mathf.FloorToInt(centroPixelY - radioPixelY));
        int maxY = Mathf.Min(AltoHistorial - 1, Mathf.CeilToInt(centroPixelY + radioPixelY));
        bool cambio = false;

        for (int y = minY; y <= maxY; y++)
        {
            float mundoZ = minimoHistorial.y
                + y / (float)(AltoHistorial - 1) * tamanoHistorial.y;
            float dz = mundoZ - centro.z;
            for (int x = minX; x <= maxX; x++)
            {
                float mundoX = minimoHistorial.x
                    + x / (float)(AnchoHistorial - 1) * tamanoHistorial.x;
                float dx = mundoX - centro.x;
                float distanciaNormalizada = Mathf.Sqrt(dx * dx + dz * dz) / radio;
                if (distanciaNormalizada >= 1f)
                {
                    continue;
                }

                float intensidad = 1f - Mathf.SmoothStep(0.82f, 1f, distanciaNormalizada);
                byte valor = (byte)Mathf.RoundToInt(intensidad * 255f);
                int indice = y * AnchoHistorial + x;
                if (valor > pixelesHistorial[indice])
                {
                    pixelesHistorial[indice] = valor;
                    cambio = true;
                }
            }
        }

        return cambio;
    }

    void SubirTexturaHistorial()
    {
        if (texturaHistorial == null || pixelesHistorial == null)
        {
            return;
        }

        texturaHistorial.SetPixelData(pixelesHistorial, 0);
        texturaHistorial.Apply(false, false);
    }

    Camera ObtenerCamaraCampania()
    {
        if (EsCamaraUtil(camaraCampania))
        {
            return camaraCampania;
        }

        Camera[] camaras = Camera.allCameras;
        for (int i = 0; i < camaras.Length; i++)
        {
            Camera candidata = camaras[i];
            if (EsCamaraUtil(candidata)
                && !NombreIndicaCamaraBatalla(candidata)
                && candidata.GetComponent<EdgePanCameraZ>() != null)
            {
                camaraCampania = candidata;
                return camaraCampania;
            }
        }

        Camera principal = Camera.main;
        if (EsCamaraUtil(principal) && !NombreIndicaCamaraBatalla(principal))
        {
            camaraCampania = principal;
            return camaraCampania;
        }

        return null;
    }

    void AsegurarRenderEnCamara(Camera camara)
    {
        if (renderNiebla != null && renderNiebla.gameObject == camara.gameObject)
        {
            renderNiebla.Configurar(this, materialNiebla);
            return;
        }

        if (renderNiebla != null)
        {
            Destroy(renderNiebla);
        }

        renderNiebla = camara.GetComponent<NieblaGuerraCaravanaRender>();
        if (renderNiebla == null)
        {
            renderNiebla = camara.gameObject.AddComponent<NieblaGuerraCaravanaRender>();
        }

        renderNiebla.hideFlags = HideFlags.DontSave;
        renderNiebla.Configurar(this, materialNiebla);
    }

    void ActualizarOpacidad(float objetivo)
    {
        opacidadSuavizada = Mathf.SmoothDamp(
            opacidadSuavizada,
            objetivo,
            ref velocidadOpacidad,
            objetivo > opacidadSuavizada ? 0.35f : 0.18f,
            Mathf.Infinity,
            Time.unscaledDeltaTime);
        materialNiebla.SetFloat(IdOpacidad, opacidadSuavizada);
    }

    void DesactivarRenderTemporalmente()
    {
        if (renderNiebla != null)
        {
            renderNiebla.enabled = false;
        }

        Shader.SetGlobalFloat(IdCaminoVisionActiva, 0f);
        Shader.SetGlobalFloat(IdCaminoVisionDebug, 0f);
    }

    void ActualizarMascaraCaminos(Vector4 visionPantalla, float suavizado)
    {
        bool mostrarTodosDebug = CampaignManager.Instance != null
            && CampaignManager.Instance.DebeMostrarTodosLosCaminosMapaDebug();
        Shader.SetGlobalVector(IdCaminoVisionPantalla, visionPantalla);
        Shader.SetGlobalFloat(IdCaminoVisionSuavizado, suavizado);
        Shader.SetGlobalFloat(IdCaminoVisionActiva, 1f);
        Shader.SetGlobalFloat(IdCaminoVisionDebug, mostrarTodosDebug ? 1f : 0f);
    }

    int ObtenerZonaActual()
    {
        return CampaignManager.Instance != null
            && CampaignManager.Instance.scAtributosZona != null
                ? CampaignManager.Instance.scAtributosZona.ID
                : -1;
    }

    static void IncluirEnBounds(
        Vector3 punto,
        ref float minX,
        ref float minZ,
        ref float maxX,
        ref float maxZ)
    {
        minX = Mathf.Min(minX, punto.x);
        minZ = Mathf.Min(minZ, punto.z);
        maxX = Mathf.Max(maxX, punto.x);
        maxZ = Mathf.Max(maxZ, punto.z);
    }

    static float DistanciaHorizontal(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static bool EsCamaraUtil(Camera camara)
    {
        return camara != null && camara.enabled && camara.gameObject.activeInHierarchy;
    }

    static bool NombreIndicaCamaraBatalla(Camera camara)
    {
        return camara != null
            && camara.name.IndexOf("batalla", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    void OnDisable()
    {
        DesactivarRenderTemporalmente();
    }

    void OnDestroy()
    {
        if (renderNiebla != null)
        {
            Destroy(renderNiebla);
        }

        if (materialNiebla != null)
        {
            Destroy(materialNiebla);
        }

        if (texturaHistorial != null)
        {
            Destroy(texturaHistorial);
        }
    }
}
