using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BandadasBosqueArdiente : MonoBehaviour
{
    const string RutaHojaSprites = "ObjetosMapa/Bandadas/CuervoBandada_SpriteStrip";
    const string RutaHojaSpritesCenital = "ObjetosMapa/Bandadas/CuervoBandada_CenitalSpriteStrip";
    const string RutaAudioEscape = "Cuervos_escape";
    const int ColumnasHoja = 4;
    const int FilasHoja = 1;

    [Header("Aparicion")]
    [SerializeField] int cantidadMinima = 4;
    [SerializeField] int cantidadMaxima = 7;
    [SerializeField] Vector2 retrasoInicialSegundos = new Vector2(6f, 12f);
    [SerializeField] Vector2 intervaloSegundos = new Vector2(18f, 34f);

    [Header("Direcciones habilitadas")]
    [SerializeField] bool desdeIzquierda = true;
    [SerializeField] bool desdeDerecha = true;
    [SerializeField] bool desdeArriba = true;
    [SerializeField] bool desdeAbajo = true;

    [Header("Vuelo en pantalla")]
    [SerializeField] Vector2 profundidadCamara = new Vector2(7.5f, 10.5f);
    [SerializeField] Vector2 duracionCruceSegundos = new Vector2(6.5f, 9.5f);
    [SerializeField] float margenFueraViewport = 0.08f;
    [SerializeField] float separacionFormacion = 0.30f;
    [SerializeField] float irregularidadFormacion = 0.08f;

    [Header("Ondulacion del vuelo")]
    [SerializeField] float amplitudOndulacionViewport = 0.014f;
    [SerializeField] Vector2 ondulacionesPorCruce = new Vector2(2.5f, 4f);
    [SerializeField] float amplitudOndulacionSecundariaViewport = 0.004f;
    [SerializeField] Vector2 ondulacionesSecundariasPorCruce = new Vector2(5f, 7f);
    [SerializeField] float amplitudOndulacionIndividual = 0.035f;
    [SerializeField, Range(0f, 0.4f)] float variacionVelocidad = 0.18f;

    [Header("Visual")]
    [SerializeField] float pixelesPorUnidad = 180f;
    [SerializeField] Vector2 escalaPajaros = new Vector2(0.22f, 0.34f);
    [SerializeField] Vector2 velocidadAleteo = new Vector2(7f, 10f);
    [SerializeField] Color tintePajaros = new Color(0.52f, 0.58f, 0.56f, 0.92f);
    [SerializeField] int ordenRender = 30;

    [Header("Bandada de escape")]
    [SerializeField] Vector2Int cantidadEscape = new Vector2Int(5, 9);
    [SerializeField] Vector2 duracionEscapeSegundos = new Vector2(3.5f, 5f);
    [SerializeField, Range(0.25f, 2f)] float velocidadEscape = 0.65f;
    [SerializeField, Range(0f, 45f)] float dispersionEscapeGrados = 18f;
    [SerializeField, Range(0.1f, 1f)] float formacionInicialEscape = 0.45f;
    [SerializeField, Range(1f, 2f)] float formacionFinalEscape = 1.35f;

    [Header("Audio de escape")]
    [SerializeField, Range(0f, 1f)] float volumenAudioEscape = 0.18f;
    [SerializeField] Vector2 pitchAudioEscape = new Vector2(0.9f, 1.1f);

    sealed class PajaroActivo
    {
        public Transform visual;
        public SpriteRenderer renderer;
        public Vector2 offsetFormacion;
        public float faseOndulacion;
        public float velocidadOndulacion;
        public float escala;
        public int offsetFrame;
    }

    sealed class BandadaActiva
    {
        public GameObject raiz;
        public readonly List<PajaroActivo> pajaros = new List<PajaroActivo>(7);
        public Vector3 inicioMundo;
        public Vector3 finMundo;
        public Vector3 perpendicularMundo;
        public Quaternion rotacionMundo;
        public Quaternion rotacionLocalPajaros;
        public List<Sprite> framesAleteo;
        public float duracion;
        public float progreso;
        public float tiempo;
        public float faseVelocidad;
        public float semillaRuido;
        public float amplitudOndulacion;
        public float ondulaciones;
        public float amplitudOndulacionSecundaria;
        public float ondulacionesSecundarias;
        public float faseOndulacionSecundaria;
        public float fpsAleteo;
        public float multiplicadorFormacionInicial;
        public float multiplicadorFormacionFinal;
    }

    readonly List<Sprite> framesAleteo = new List<Sprite>(ColumnasHoja * FilasHoja);
    readonly List<Sprite> framesAleteoCenital = new List<Sprite>(ColumnasHoja * FilasHoja);
    readonly List<BandadaActiva> bandadas = new List<BandadaActiva>(1);

    AtributosZona atributosZona;
    Camera camaraCampania;
    AudioClip audioEscape;
    Coroutine cicloAparicion;
    static BandadasBosqueArdiente instanciaActiva;

    void Awake()
    {
        atributosZona = GetComponentInParent<AtributosZona>();
        CargarFramesAleteo(RutaHojaSprites, framesAleteo, "CuervoBandada");
        CargarFramesAleteo(RutaHojaSpritesCenital, framesAleteoCenital, "CuervoBandadaCenital");
        audioEscape = Resources.Load<AudioClip>(RutaAudioEscape);
    }

    void OnEnable()
    {
        instanciaActiva = this;

        if (Application.isPlaying && cicloAparicion == null)
        {
            cicloAparicion = StartCoroutine(CicloAparicion());
        }
    }

    void Update()
    {
        if (bandadas.Count == 0)
        {
            return;
        }

        if (!ContextoDisponible())
        {
            LimpiarBandadas();
            return;
        }

        float deltaTime = Time.deltaTime;
        for (int i = bandadas.Count - 1; i >= 0; i--)
        {
            if (!ActualizarBandada(bandadas[i], deltaTime))
            {
                DestruirBandada(bandadas[i]);
                bandadas.RemoveAt(i);
            }
        }
    }

    void OnDisable()
    {
        if (instanciaActiva == this)
        {
            instanciaActiva = null;
        }

        if (cicloAparicion != null)
        {
            StopCoroutine(cicloAparicion);
            cicloAparicion = null;
        }

        LimpiarBandadas();
    }

    void OnDestroy()
    {
        for (int i = 0; i < framesAleteo.Count; i++)
        {
            if (framesAleteo[i] != null)
            {
                Destroy(framesAleteo[i]);
            }
        }

        framesAleteo.Clear();

        for (int i = 0; i < framesAleteoCenital.Count; i++)
        {
            if (framesAleteoCenital[i] != null)
            {
                Destroy(framesAleteoCenital[i]);
            }
        }

        framesAleteoCenital.Clear();
    }

    IEnumerator CicloAparicion()
    {
        float espera = RangoOrdenado(retrasoInicialSegundos);

        while (isActiveAndEnabled)
        {
            if (PuedeGenerarBandada())
            {
                espera -= Time.deltaTime;
                if (espera <= 0f)
                {
                    if (bandadas.Count == 0)
                    {
                        CrearBandada();
                    }

                    espera = RangoOrdenado(intervaloSegundos);
                }
            }

            yield return null;
        }
    }

    bool ContextoDisponible()
    {
        if (atributosZona == null)
        {
            atributosZona = GetComponentInParent<AtributosZona>();
        }

        if (atributosZona == null || atributosZona.ID != 1 || atributosZona.DecoracionZonaEnCurso)
        {
            return false;
        }

        CampaignManager campaignManager = CampaignManager.Instance;
        if (campaignManager == null || campaignManager.IntroCampaniaActivaOPendiente)
        {
            return false;
        }

        return campaignManager.scTutorialManager == null
            || !campaignManager.scTutorialManager.tutorialActivo;
    }

    bool PuedeGenerarBandada()
    {
        return (framesAleteo.Count > 0 || framesAleteoCenital.Count > 0)
            && HayDireccionHabilitada()
            && ContextoDisponible()
            && ObtenerCamaraCampania() != null;
    }

    Camera ObtenerCamaraCampania()
    {
        if (camaraCampania != null
            && camaraCampania.enabled
            && camaraCampania.gameObject.activeInHierarchy)
        {
            return camaraCampania;
        }

        camaraCampania = Camera.main;
        return camaraCampania != null
            && camaraCampania.enabled
            && camaraCampania.gameObject.activeInHierarchy
            ? camaraCampania
            : null;
    }

    void CargarFramesAleteo(string rutaHoja, List<Sprite> destino, string prefijoNombre)
    {
        Texture2D hoja = Resources.Load<Texture2D>(rutaHoja);
        if (hoja == null)
        {
            Debug.LogWarning($"[BandadasBosqueArdiente] No se encontro la hoja '{rutaHoja}'.", this);
            return;
        }

        int anchoFrame = hoja.width / ColumnasHoja;
        int altoFrame = hoja.height / FilasHoja;
        if (anchoFrame <= 0 || altoFrame <= 0)
        {
            Debug.LogWarning("[BandadasBosqueArdiente] La hoja de cuervos no tiene dimensiones validas.", this);
            return;
        }

        for (int filaVisual = 0; filaVisual < FilasHoja; filaVisual++)
        {
            int filaTextura = FilasHoja - 1 - filaVisual;
            for (int columna = 0; columna < ColumnasHoja; columna++)
            {
                Rect rect = new Rect(
                    columna * anchoFrame,
                    filaTextura * altoFrame,
                    anchoFrame,
                    altoFrame);

                Sprite frame = Sprite.Create(
                    hoja,
                    rect,
                    new Vector2(0.5f, 0.5f),
                    Mathf.Max(1f, pixelesPorUnidad),
                    0,
                    SpriteMeshType.FullRect);
                frame.name = $"{prefijoNombre}_{filaVisual}_{columna}";
                destino.Add(frame);
            }
        }
    }

    void CrearBandada()
    {
        Camera camara = ObtenerCamaraCampania();
        if (camara == null || (framesAleteo.Count == 0 && framesAleteoCenital.Count == 0))
        {
            return;
        }

        if (!ElegirTrayectoriaViewport(out Vector2 inicioViewport, out Vector2 finViewport))
        {
            return;
        }

        BandadaActiva bandada = new BandadaActiva
        {
            raiz = new GameObject("Bandada Bosque Ardiente (Runtime)"),
            duracion = Mathf.Max(0.1f, RangoOrdenado(duracionCruceSegundos)),
            faseVelocidad = Random.Range(0f, Mathf.PI * 2f),
            semillaRuido = Random.Range(0f, 1000f),
            amplitudOndulacion = amplitudOndulacionViewport * Random.Range(0.65f, 1f),
            ondulaciones = Mathf.Max(0f, RangoOrdenado(ondulacionesPorCruce)),
            amplitudOndulacionSecundaria = amplitudOndulacionSecundariaViewport * Random.Range(0.65f, 1f),
            ondulacionesSecundarias = Mathf.Max(0f, RangoOrdenado(ondulacionesSecundariasPorCruce)),
            faseOndulacionSecundaria = Random.Range(0f, Mathf.PI * 2f),
            fpsAleteo = RangoOrdenado(velocidadAleteo),
            multiplicadorFormacionInicial = 1f,
            multiplicadorFormacionFinal = 1f
        };

        float profundidad = RangoOrdenado(profundidadCamara);
        if (!ConfigurarTrayectoriaBandada(bandada, camara, inicioViewport, finViewport, profundidad))
        {
            Destroy(bandada.raiz);
            return;
        }

        int minimo = Mathf.Max(1, Mathf.Min(cantidadMinima, cantidadMaxima));
        int maximo = Mathf.Max(minimo, Mathf.Max(cantidadMinima, cantidadMaxima));
        int cantidad = Random.Range(minimo, maximo + 1);
        CrearPajaros(bandada, cantidad, profundidad);

        bandadas.Add(bandada);
        ActualizarBandada(bandada, 0f);
    }

    public static bool IntentarLanzarBandadaEscape(Vector3 posicionNodo, Vector3 posicionCaravana)
    {
        return instanciaActiva != null
            && instanciaActiva.isActiveAndEnabled
            && instanciaActiva.CrearBandadaEscape(posicionNodo, posicionCaravana);
    }

    public static bool SistemaEscapeActivo()
    {
        return instanciaActiva != null
            && instanciaActiva.isActiveAndEnabled
            && instanciaActiva.ContextoDisponible();
    }

    bool CrearBandadaEscape(Vector3 posicionNodo, Vector3 posicionCaravana)
    {
        Camera camara = ObtenerCamaraCampania();
        if (camara == null
            || !ContextoDisponible()
            || (framesAleteo.Count == 0 && framesAleteoCenital.Count == 0))
        {
            return false;
        }

        Vector3 nodoViewport3 = camara.WorldToViewportPoint(posicionNodo);
        Vector3 caravanaViewport3 = camara.WorldToViewportPoint(posicionCaravana);
        const float toleranciaVisibilidad = 0.08f;
        if (nodoViewport3.z <= 0f
            || caravanaViewport3.z <= 0f
            || nodoViewport3.x < -toleranciaVisibilidad
            || nodoViewport3.x > 1f + toleranciaVisibilidad
            || nodoViewport3.y < -toleranciaVisibilidad
            || nodoViewport3.y > 1f + toleranciaVisibilidad)
        {
            return false;
        }

        Vector2 inicioViewport = new Vector2(nodoViewport3.x, nodoViewport3.y);
        Vector2 direccionEscape = inicioViewport - new Vector2(caravanaViewport3.x, caravanaViewport3.y);
        if (direccionEscape.sqrMagnitude <= 0.0001f)
        {
            direccionEscape = Random.insideUnitCircle;
        }

        direccionEscape.Normalize();
        float anguloDispersion = Random.Range(-dispersionEscapeGrados, dispersionEscapeGrados) * Mathf.Deg2Rad;
        float coseno = Mathf.Cos(anguloDispersion);
        float seno = Mathf.Sin(anguloDispersion);
        direccionEscape = new Vector2(
            direccionEscape.x * coseno - direccionEscape.y * seno,
            direccionEscape.x * seno + direccionEscape.y * coseno).normalized;
        Vector2 finViewport = CalcularSalidaViewport(inicioViewport, direccionEscape);
        float factorDistancia = Mathf.Clamp(Vector2.Distance(inicioViewport, finViewport) / 0.75f, 0.55f, 1.15f);

        BandadaActiva bandada = new BandadaActiva
        {
            raiz = new GameObject("Bandada Escape Bosque Ardiente (Runtime)"),
            duracion = Mathf.Max(
                0.1f,
                RangoOrdenado(duracionEscapeSegundos) * factorDistancia / Mathf.Max(0.1f, velocidadEscape)),
            faseVelocidad = Random.Range(0f, Mathf.PI * 2f),
            semillaRuido = Random.Range(0f, 1000f),
            amplitudOndulacion = amplitudOndulacionViewport * Random.Range(0.8f, 1.15f),
            ondulaciones = Mathf.Max(0f, RangoOrdenado(ondulacionesPorCruce) * 1.25f),
            amplitudOndulacionSecundaria = amplitudOndulacionSecundariaViewport * Random.Range(0.9f, 1.25f),
            ondulacionesSecundarias = Mathf.Max(0f, RangoOrdenado(ondulacionesSecundariasPorCruce) * 1.2f),
            faseOndulacionSecundaria = Random.Range(0f, Mathf.PI * 2f),
            fpsAleteo = RangoOrdenado(velocidadAleteo) * Random.Range(1.05f, 1.2f),
            multiplicadorFormacionInicial = formacionInicialEscape,
            multiplicadorFormacionFinal = formacionFinalEscape
        };

        float profundidad = RangoOrdenado(profundidadCamara);
        if (!ConfigurarTrayectoriaBandada(bandada, camara, inicioViewport, finViewport, profundidad))
        {
            Destroy(bandada.raiz);
            return false;
        }

        int minimo = Mathf.Max(1, Mathf.Min(cantidadEscape.x, cantidadEscape.y));
        int maximo = Mathf.Max(minimo, Mathf.Max(cantidadEscape.x, cantidadEscape.y));
        CrearPajaros(bandada, Random.Range(minimo, maximo + 1), profundidad);
        bandadas.Add(bandada);
        ActualizarBandada(bandada, 0f);
        ReproducirAudioEscape(bandada);
        return true;
    }

    void ReproducirAudioEscape(BandadaActiva bandada)
    {
        if (audioEscape == null || bandada == null || bandada.raiz == null)
        {
            return;
        }

        AudioSource audio = bandada.raiz.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.loop = false;
        audio.clip = audioEscape;
        audio.pitch = Mathf.Clamp(RangoOrdenado(pitchAudioEscape), 0.1f, 3f);
        audio.spatialBlend = 0.25f;
        audio.dopplerLevel = 0f;
        audio.minDistance = 5f;
        audio.maxDistance = 30f;
        AjustesAudio.AplicarVolumenSfx(audio, volumenAudioEscape);
        audio.Play();
    }

    bool ConfigurarTrayectoriaBandada(
        BandadaActiva bandada,
        Camera camara,
        Vector2 inicioViewport,
        Vector2 finViewport,
        float profundidad)
    {
        Vector2 deltaViewport = finViewport - inicioViewport;
        if (deltaViewport.sqrMagnitude <= 0.0001f)
        {
            return false;
        }

        bool vueloVertical = Mathf.Abs(deltaViewport.y) > Mathf.Abs(deltaViewport.x);
        bool usarSpritesCenitales = vueloVertical && framesAleteoCenital.Count > 0;
        bandada.framesAleteo = usarSpritesCenitales ? framesAleteoCenital : framesAleteo;
        if (bandada.framesAleteo.Count == 0)
        {
            bandada.framesAleteo = framesAleteoCenital;
            usarSpritesCenitales = true;
        }

        Vector2 direccionViewport = deltaViewport.normalized;
        Vector2 perpendicularViewport = new Vector2(-direccionViewport.y, direccionViewport.x);
        bandada.inicioMundo = camara.ViewportToWorldPoint(
            new Vector3(inicioViewport.x, inicioViewport.y, profundidad));
        bandada.finMundo = camara.ViewportToWorldPoint(
            new Vector3(finViewport.x, finViewport.y, profundidad));

        Vector2 centroViewport = (inicioViewport + finViewport) * 0.5f;
        Vector3 centroMundo = camara.ViewportToWorldPoint(
            new Vector3(centroViewport.x, centroViewport.y, profundidad));
        bandada.perpendicularMundo = camara.ViewportToWorldPoint(
            new Vector3(
                centroViewport.x + perpendicularViewport.x,
                centroViewport.y + perpendicularViewport.y,
                profundidad)) - centroMundo;

        Vector2 direccionPixeles = Vector2.Scale(
            deltaViewport,
            new Vector2(Mathf.Max(1, camara.pixelWidth), Mathf.Max(1, camara.pixelHeight)));
        float anguloPantalla = Mathf.Atan2(direccionPixeles.y, direccionPixeles.x) * Mathf.Rad2Deg;
        bandada.rotacionLocalPajaros = usarSpritesCenitales
            ? Quaternion.Euler(0f, 0f, -90f)
            : Quaternion.identity;
        bandada.rotacionMundo = camara.transform.rotation * Quaternion.Euler(0f, 0f, anguloPantalla);
        bandada.raiz.transform.rotation = bandada.rotacionMundo;
        return true;
    }

    void CrearPajaros(BandadaActiva bandada, int cantidad, float profundidad)
    {
        float profundidadMedia = Mathf.Max(0.1f, (profundidadCamara.x + profundidadCamara.y) * 0.5f);
        float compensacionProfundidad = profundidad / profundidadMedia;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject goPajaro = new GameObject($"Cuervo {i + 1}");
            goPajaro.transform.SetParent(bandada.raiz.transform, false);

            SpriteRenderer renderer = goPajaro.AddComponent<SpriteRenderer>();
            renderer.sprite = bandada.framesAleteo[Random.Range(0, bandada.framesAleteo.Count)];
            renderer.color = tintePajaros;
            renderer.sortingOrder = ordenRender;

            bandada.pajaros.Add(new PajaroActivo
            {
                visual = goPajaro.transform,
                renderer = renderer,
                offsetFormacion = ObtenerOffsetFormacion(i),
                faseOndulacion = Random.Range(0f, Mathf.PI * 2f),
                velocidadOndulacion = Random.Range(1.6f, 2.5f),
                escala = RangoOrdenado(escalaPajaros) * compensacionProfundidad,
                offsetFrame = Random.Range(0, bandada.framesAleteo.Count)
            });
        }
    }

    Vector2 CalcularSalidaViewport(Vector2 inicio, Vector2 direccion)
    {
        float margen = Mathf.Max(0.01f, margenFueraViewport);
        float distancia = float.PositiveInfinity;

        if (Mathf.Abs(direccion.x) > 0.0001f)
        {
            float bordeX = direccion.x > 0f ? 1f + margen : -margen;
            float distanciaX = (bordeX - inicio.x) / direccion.x;
            if (distanciaX > 0f)
            {
                distancia = Mathf.Min(distancia, distanciaX);
            }
        }

        if (Mathf.Abs(direccion.y) > 0.0001f)
        {
            float bordeY = direccion.y > 0f ? 1f + margen : -margen;
            float distanciaY = (bordeY - inicio.y) / direccion.y;
            if (distanciaY > 0f)
            {
                distancia = Mathf.Min(distancia, distanciaY);
            }
        }

        if (float.IsInfinity(distancia))
        {
            distancia = 1.5f;
        }

        return inicio + direccion * (distancia + 0.04f);
    }

    bool ActualizarBandada(BandadaActiva bandada, float deltaTime)
    {
        if (bandada == null || bandada.raiz == null)
        {
            return false;
        }

        bandada.tiempo += deltaTime;
        float ruidoVelocidad = (Mathf.PerlinNoise(bandada.semillaRuido, bandada.tiempo * 0.32f) - 0.5f) * 2f;
        float pulsoVelocidad = Mathf.Sin(bandada.tiempo * 0.75f + bandada.faseVelocidad);
        float factorVelocidad = 1f + variacionVelocidad * ((pulsoVelocidad * 0.65f) + (ruidoVelocidad * 0.35f));
        bandada.progreso += (deltaTime / bandada.duracion) * Mathf.Clamp(factorVelocidad, 0.65f, 1.35f);

        if (bandada.progreso > 1.04f)
        {
            return false;
        }

        Vector3 posicionMundo = Vector3.LerpUnclamped(
            bandada.inicioMundo,
            bandada.finMundo,
            bandada.progreso);
        float onda = Mathf.Sin(
            (bandada.progreso * Mathf.PI * 2f * bandada.ondulaciones) + bandada.faseVelocidad);
        float ondaSecundaria = Mathf.Sin(
            (bandada.progreso * Mathf.PI * 2f * bandada.ondulacionesSecundarias)
            + bandada.faseOndulacionSecundaria);
        float deriva = (Mathf.PerlinNoise(bandada.semillaRuido + 31.7f, bandada.tiempo * 0.22f) - 0.5f) * 0.7f;
        float desplazamientoOndulacion = ((onda + deriva) * bandada.amplitudOndulacion)
            + (ondaSecundaria * bandada.amplitudOndulacionSecundaria);
        posicionMundo += bandada.perpendicularMundo * desplazamientoOndulacion;

        bandada.raiz.transform.SetPositionAndRotation(posicionMundo, bandada.rotacionMundo);

        float aperturaFormacion = Mathf.Lerp(
            bandada.multiplicadorFormacionInicial,
            bandada.multiplicadorFormacionFinal,
            Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(bandada.progreso / 0.45f)));

        for (int i = 0; i < bandada.pajaros.Count; i++)
        {
            PajaroActivo pajaro = bandada.pajaros[i];
            if (pajaro.visual == null || pajaro.renderer == null)
            {
                continue;
            }

            float ondulacionIndividual = Mathf.Sin(
                bandada.tiempo * pajaro.velocidadOndulacion + pajaro.faseOndulacion)
                * amplitudOndulacionIndividual;
            Vector2 offsetAbierto = pajaro.offsetFormacion * aperturaFormacion;
            pajaro.visual.localPosition = new Vector3(
                offsetAbierto.x,
                offsetAbierto.y + ondulacionIndividual,
                i * 0.002f);
            pajaro.visual.localRotation = bandada.rotacionLocalPajaros;
            pajaro.visual.localScale = Vector3.one * pajaro.escala;

            int frame = ((int)(bandada.tiempo * bandada.fpsAleteo) + pajaro.offsetFrame) % bandada.framesAleteo.Count;
            pajaro.renderer.sprite = bandada.framesAleteo[frame];
        }

        return true;
    }

    Vector2 ObtenerOffsetFormacion(int indice)
    {
        if (indice == 0)
        {
            return Random.insideUnitCircle * (irregularidadFormacion * 0.35f);
        }

        int fila = (indice + 1) / 2;
        float lado = indice % 2 == 1 ? -1f : 1f;
        Vector2 baseFormacion = new Vector2(
            -fila * separacionFormacion,
            lado * fila * separacionFormacion);
        return baseFormacion + Random.insideUnitCircle * irregularidadFormacion;
    }

    bool HayDireccionHabilitada()
    {
        return desdeIzquierda || desdeDerecha || desdeArriba || desdeAbajo;
    }

    bool ElegirTrayectoriaViewport(out Vector2 inicio, out Vector2 fin)
    {
        float margen = Mathf.Max(0.01f, margenFueraViewport);
        float pesoTotal = (desdeIzquierda ? 4f : 0f)
            + (desdeDerecha ? 4f : 0f)
            + (desdeArriba ? 1f : 0f)
            + (desdeAbajo ? 1f : 0f);
        float eleccion = Random.value * pesoTotal;

        if (desdeIzquierda)
        {
            if (eleccion <= 4f)
            {
                float y = Random.Range(0.28f, 0.82f);
                inicio = new Vector2(-margen, y);
                fin = new Vector2(1f + margen, Mathf.Clamp(y + Random.Range(-0.18f, 0.18f), 0.24f, 0.84f));
                return true;
            }

            eleccion -= 4f;
        }

        if (desdeDerecha)
        {
            if (eleccion <= 4f)
            {
                float y = Random.Range(0.28f, 0.82f);
                inicio = new Vector2(1f + margen, y);
                fin = new Vector2(-margen, Mathf.Clamp(y + Random.Range(-0.18f, 0.18f), 0.24f, 0.84f));
                return true;
            }

            eleccion -= 4f;
        }

        if (desdeArriba)
        {
            if (eleccion <= 1f)
            {
                float x = Random.Range(0.14f, 0.86f);
                inicio = new Vector2(x, 1f + margen);
                fin = new Vector2(Mathf.Clamp(x + Random.Range(-0.24f, 0.24f), 0.10f, 0.90f), -margen);
                return true;
            }

            eleccion -= 1f;
        }

        if (desdeAbajo)
        {
            float x = Random.Range(0.14f, 0.86f);
            inicio = new Vector2(x, -margen);
            fin = new Vector2(Mathf.Clamp(x + Random.Range(-0.24f, 0.24f), 0.10f, 0.90f), 1f + margen);
            return true;
        }

        inicio = Vector2.zero;
        fin = Vector2.zero;
        return false;
    }

    void LimpiarBandadas()
    {
        for (int i = bandadas.Count - 1; i >= 0; i--)
        {
            DestruirBandada(bandadas[i]);
        }

        bandadas.Clear();
    }

    static void DestruirBandada(BandadaActiva bandada)
    {
        if (bandada != null && bandada.raiz != null)
        {
            Destroy(bandada.raiz);
        }
    }

    static float RangoOrdenado(Vector2 rango)
    {
        return Random.Range(Mathf.Min(rango.x, rango.y), Mathf.Max(rango.x, rango.y));
    }
}
