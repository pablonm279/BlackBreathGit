using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class btnPersonaje : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
  private const float IntervaloAutoRefrescoRetratoCampania = 0.2f;
  private const float EsperaCierreOpcionesActividad = 0.1f;
  private const float DuracionAnimacionOpcionesActividad = 0.12f;
  private const float EspaciadoOpcionesActividad = 21.5f;
  private static readonly Vector2 AnchorEstadosCampania = new Vector2(0.5f, 0f);
  private static readonly Vector2 PosicionEstadosCampania = new Vector2(-16.2f, 56.2f);
  private static readonly Vector2 AnchorEstadosMenuBatallas = new Vector2(0.5f, 0.5f);
  private static readonly Vector2 PosicionEstadosMenuBatallas = new Vector2(-16.2f, 6.3f);

  public Personaje personajeRepresentado;
  RefuerzoAliadoCaravanaOrdenItem refuerzoCaravanaRepresentado;
  SequitoCuranderos sequitoCuranderosTratamiento;

  public TextMeshProUGUI txtPersonajeRepresentado;
  [SerializeField] private Image retratoRepresenta;
  [SerializeField] private Image retratoSombraRepresenta;
  [SerializeField] private btnActividad actividadRetrato;
  private Image progresoActividadFill;
  [SerializeField] private GameObject indicadorActividadFijada;
  [SerializeField] private Transform contenedorEstadosCampania;
  [SerializeField] private GameObject prefabEstadoCampania;
  [SerializeField] private Image iconoMuerto;
  [SerializeField] private GameObject indicadorNivelPendiente;
  [SerializeField] private GameObject indicadorSeleccionado;
  bool arrastrandoRefuerzoCaravana;
  int ultimoFrameArrastre = -1000;
  bool abreMenuPersonajesAlHover;
  bool reenvioArrastreConfigurado;
  float proximoAutoRefrescoRetratoCampania;
  int firmaVisualRetratoCampania = int.MinValue;
  readonly List<btnActividad> opcionesActividadRetrato = new List<btnActividad>();
  Coroutine rutinaCierreOpcionesActividad;
  bool selectorActividadRetratoActivo;

  private void Awake()
  {
    AsegurarReferencias();
    ConfigurarReenvioArrastreDesdeEventTriggers();
  }


  public void SeleccionarPJ()
  {
    TutorialEvents.Emit("ui.pjpostbat1_presionado", gameObject);
    
    if (Time.frameCount - ultimoFrameArrastre <= 1)
    {
      return;
    }

    if (personajeRepresentado == null)
    {
      return;
    }

    if (sequitoCuranderosTratamiento != null)
    {
      sequitoCuranderosTratamiento.TratarHerida(personajeRepresentado);
      representarVida();
      return;
    }

    if (!CampaignManager.Instance.goMenuBatallas.activeInHierarchy)
    {
      if (!CampaignManager.Instance.goSequitos.activeInHierarchy)
      {
        if (CampaignManager.Instance.scMenuCaravana != null)
        {
          CampaignManager.Instance.scMenuCaravana.AbrirMenuPersonajes(personajeRepresentado);
        }
        else
        {
          MenuPersonajes menuPersonajes = GetComponentInParent<MenuPersonajes>();
          if (menuPersonajes != null)
          {
            menuPersonajes.PrepararYAbrirMenu(personajeRepresentado);
          }
        }
      }
      else //Si estan los sequitos activos, por lo tanto se asume que es el de curanderos curando.
      {
        SequitoCuranderos curanderos = CampaignManager.Instance.scMenuSequito != null
          ? CampaignManager.Instance.scMenuSequito.ObtenerSequitoCuranderosActivo()
          : null;
        if (curanderos != null)
        {
          curanderos.TratarHerida(personajeRepresentado);
        }
      }
    }
    else if (CampaignManager.Instance.scMenuBatallas.UIEmpezarBatalla.activeInHierarchy || CampaignManager.Instance.scMenuBatallas.UIEmpezarBatallaACaravana.activeInHierarchy) //Si esta en la pantalla de batalla, se selecciona el personaje para la batalla
    {
      CampaignManager.Instance.scMenuBatallas.Seleccionar(personajeRepresentado);
    }

    representarVida();

  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    MenuBatallas menuBatallas = CampaignManager.Instance != null ? CampaignManager.Instance.scMenuBatallas : null;
    if (menuBatallas == null || !menuBatallas.PuedeReordenarRefuerzoCaravana(this))
    {
      arrastrandoRefuerzoCaravana = false;
      return;
    }

    arrastrandoRefuerzoCaravana = true;
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (!arrastrandoRefuerzoCaravana)
    {
      return;
    }

    MenuBatallas menuBatallas = CampaignManager.Instance != null ? CampaignManager.Instance.scMenuBatallas : null;
    if (menuBatallas != null)
    {
      menuBatallas.ReordenarRefuerzoCaravana(this, eventData.position, eventData.pressEventCamera);
    }
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    if (!arrastrandoRefuerzoCaravana)
    {
      return;
    }

    arrastrandoRefuerzoCaravana = false;
    ultimoFrameArrastre = Time.frameCount;

    MenuBatallas menuBatallas = CampaignManager.Instance != null ? CampaignManager.Instance.scMenuBatallas : null;
    if (menuBatallas != null)
    {
      menuBatallas.ConfirmarOrdenRefuerzosCaravanaDesdeUI();
      menuBatallas.ActualizarLista();
    }
  }

  private void ConfigurarReenvioArrastreDesdeEventTriggers()
  {
    if (reenvioArrastreConfigurado)
    {
      return;
    }

    reenvioArrastreConfigurado = true;
    EventTrigger[] triggers = GetComponentsInChildren<EventTrigger>(true);
    foreach (EventTrigger trigger in triggers)
    {
      if (trigger == null || trigger.gameObject == gameObject)
      {
        continue;
      }

      AgregarReenvioArrastre(trigger, EventTriggerType.BeginDrag, OnBeginDrag);
      AgregarReenvioArrastre(trigger, EventTriggerType.Drag, OnDrag);
      AgregarReenvioArrastre(trigger, EventTriggerType.EndDrag, OnEndDrag);
    }
  }

  private static void AgregarReenvioArrastre(EventTrigger trigger, EventTriggerType tipo, System.Action<PointerEventData> accion)
  {
    EventTrigger.Entry entry = new EventTrigger.Entry { eventID = tipo };
    entry.callback.AddListener(data =>
    {
      PointerEventData pointerData = data as PointerEventData;
      if (pointerData != null)
      {
        accion(pointerData);
      }
    });
    trigger.triggers.Add(entry);
  }

  public Image vidaRepresenta;
  public void Configurar(Personaje personaje)
  {
    refuerzoCaravanaRepresentado = null;
    sequitoCuranderosTratamiento = null;
    personajeRepresentado = personaje;
    RepresentarTodo();
  }

  public void ConfigurarHoverMenuPersonajesCampania(bool habilitado)
  {
    abreMenuPersonajesAlHover = habilitado;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (selectorActividadRetratoActivo
      || !abreMenuPersonajesAlHover
      || personajeRepresentado == null
      || CampaignManager.Instance == null
      || CampaignManager.Instance.scMenuCaravana == null)
    {
      return;
    }

    CampaignManager.Instance.scMenuCaravana.AbrirMenuPersonajesPorHover(personajeRepresentado);
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (!abreMenuPersonajesAlHover || personajeRepresentado == null || CampaignManager.Instance == null || CampaignManager.Instance.scMenuCaravana == null)
    {
      return;
    }

    CampaignManager.Instance.scMenuCaravana.CerrarMenuPersonajesPorHover(personajeRepresentado);
  }

  public void ConfigurarParaCuranderos(Personaje personaje, SequitoCuranderos curanderos)
  {
    refuerzoCaravanaRepresentado = null;
    sequitoCuranderosTratamiento = curanderos;
    personajeRepresentado = personaje;
    RepresentarTodo();
  }

  public void ConfigurarRefuerzoCaravana(RefuerzoAliadoCaravanaOrdenItem refuerzo)
  {
    refuerzoCaravanaRepresentado = refuerzo;
    sequitoCuranderosTratamiento = null;
    personajeRepresentado = refuerzo != null ? refuerzo.personaje : null;
    RepresentarTodo();
  }

  public RefuerzoAliadoCaravanaOrdenItem ObtenerRefuerzoCaravanaRepresentado()
  {
    if (refuerzoCaravanaRepresentado != null)
    {
      return refuerzoCaravanaRepresentado;
    }

    if (personajeRepresentado == null)
    {
      return null;
    }

    return new RefuerzoAliadoCaravanaOrdenItem
    {
      id = "personaje:" + personajeRepresentado.GetInstanceID(),
      tipo = TipoRefuerzoAliadoCaravana.Personaje,
      personaje = personajeRepresentado,
      nombreVisible = personajeRepresentado.sNombre,
      retrato = personajeRepresentado.spRetrato
    };
  }

  public void SetSeleccionado(bool seleccionado)
  {
    AsegurarReferencias();

    if (indicadorSeleccionado != null)
    {
      indicadorSeleccionado.SetActive(seleccionado);
    }
  }

  public GameObject ObtenerPrefabEstadoCampania()
  {
    return prefabEstadoCampania;
  }

  public void representarVida()
  {
    AsegurarReferencias();

    if (vidaRepresenta != null)
    {
      vidaRepresenta.gameObject.SetActive(personajeRepresentado != null);
    }

    if (personajeRepresentado != null)
    {

      float vidaMaxEscalada = personajeRepresentado.ObtenerVidaMaximaConFuerza();
      float vidaActualEscalada = personajeRepresentado.ObtenerVidaActualConFuerza();
      float valor = vidaMaxEscalada > 0f ? 1f - (vidaActualEscalada / vidaMaxEscalada) : 1f;

      if (vidaRepresenta != null)
      {
        vidaRepresenta.fillAmount = Mathf.Clamp01(valor);
      }


      if (!CampaignManager.Instance.goMenuBatallas.activeInHierarchy) //Muestra efecto de subida pendiente
      {
        bool sube = false;
        if (personajeRepresentado != null)
        {
          if (personajeRepresentado.NivelPuntoAtributo > 0) { sube = true; }
          if (personajeRepresentado.NivelPuntoHabilidad > 0) { sube = true; }
          if (personajeRepresentado.NivelPuntoTS > 0) { sube = true; }

          if (sube)
          {
            if (indicadorNivelPendiente != null)
            {
              indicadorNivelPendiente.SetActive(true);
            }
          }
          else if (indicadorNivelPendiente != null)
          {
            indicadorNivelPendiente.SetActive(false);
          }
        }
      }
      else if (indicadorNivelPendiente != null)
      {
        indicadorNivelPendiente.SetActive(false);
      }
    }
    else if (indicadorNivelPendiente != null)
    {
      indicadorNivelPendiente.SetActive(false);
    }

  }
  public void representarinfo()
  {
    if (txtPersonajeRepresentado == null)
    {
      return;
    }

    if (EstaEnContenedorMenuBatallas())
    {
      if (personajeRepresentado != null)
      {
        txtPersonajeRepresentado.gameObject.SetActive(true);
        string nombreClase = ObtenerNombreClaseTraducido(personajeRepresentado);
        string prefijoClase = string.IsNullOrEmpty(nombreClase) ? "" : nombreClase + " ";
        if (TRADU.i.nIdioma == 1) //Español
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<size=90%><color=#B8860B>" + prefijoClase + "Nv." + ((int)personajeRepresentado.fNivelActual) + "</color></size>";
        }
        if (TRADU.i.nIdioma == 2) //Inglés
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<size=90%><color=#B8860B>" + prefijoClase + "Lv." + ((int)personajeRepresentado.fNivelActual) + "</color></size>";
        }
         if (TRADU.i.nIdioma == 3) //Poertu
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<size=90%><color=#B8860B>" + prefijoClase + "Nv." + ((int)personajeRepresentado.fNivelActual) + "</color></size>";
        }



      }
      else if (refuerzoCaravanaRepresentado != null)
      {
        txtPersonajeRepresentado.gameObject.SetActive(true);
        txtPersonajeRepresentado.text = TRADU.i != null
          ? TRADU.i.Traducir(refuerzoCaravanaRepresentado.nombreVisible)
          : refuerzoCaravanaRepresentado.nombreVisible;
      }
      else
      {
        txtPersonajeRepresentado.gameObject.SetActive(false);
      }
    }
    else
    {
      if (personajeRepresentado != null)
      {
        txtPersonajeRepresentado.gameObject.SetActive(true);
        txtPersonajeRepresentado.text = personajeRepresentado.sNombre;
      }
      else if (refuerzoCaravanaRepresentado != null)
      {
        txtPersonajeRepresentado.gameObject.SetActive(true);
        txtPersonajeRepresentado.text = TRADU.i != null
          ? TRADU.i.Traducir(refuerzoCaravanaRepresentado.nombreVisible)
          : refuerzoCaravanaRepresentado.nombreVisible;
      }
      else
      {
        txtPersonajeRepresentado.gameObject.SetActive(false);
      }
    }

  }
  private void OnEnable()
  {

    RepresentarTodo();


  }

  private void OnDisable()
  {
    CerrarOpcionesActividadRetrato(true);
  }

  private void Update()
  {
    if (!abreMenuPersonajesAlHover || personajeRepresentado == null)
    {
      return;
    }

    if (Time.unscaledTime < proximoAutoRefrescoRetratoCampania)
    {
      return;
    }

    proximoAutoRefrescoRetratoCampania = Time.unscaledTime + IntervaloAutoRefrescoRetratoCampania;
    RefrescarRetratoCampaniaSiCambio();
  }

  public void RepresentarTodo()
  {
    if (personajeRepresentado != null && CampaignManager.Instance != null)
    {
      CampaignManager.Instance.SincronizarAparienciaVisualPersonaje(personajeRepresentado);
    }

    RepresentarRetrato();
    RepresentarActividad();
    representarinfo();
    representarVida();
    AjustarLayoutEstadosCampania();
    RepresentarIconos();
    firmaVisualRetratoCampania = CalcularFirmaVisualRetratoCampania();
  }

  private void RefrescarRetratoCampaniaSiCambio()
  {
    int firmaActual = CalcularFirmaVisualRetratoCampania();
    if (firmaActual == firmaVisualRetratoCampania)
    {
      return;
    }

    RepresentarTodo();
  }

  private int CalcularFirmaVisualRetratoCampania()
  {
    if (personajeRepresentado == null)
    {
      return 0;
    }

    unchecked
    {
      int hash = 17;
      hash = hash * 31 + Mathf.RoundToInt(personajeRepresentado.fVidaActual * 100f);
      hash = hash * 31 + Mathf.RoundToInt(personajeRepresentado.fVidaMaxima * 100f);
      hash = hash * 31 + Mathf.RoundToInt(personajeRepresentado.fExperienciaActual * 100f);
      hash = hash * 31 + Mathf.RoundToInt(personajeRepresentado.fNivelActual * 100f);
      hash = hash * 31 + personajeRepresentado.NivelPuntoAtributo;
      hash = hash * 31 + personajeRepresentado.NivelPuntoHabilidad;
      hash = hash * 31 + personajeRepresentado.NivelPuntoTS;
      hash = hash * 31 + personajeRepresentado.ActividadSeleccionada;
      hash = hash * 31 + Mathf.RoundToInt(personajeRepresentado.ObtenerHorasActividad(personajeRepresentado.ActividadSeleccionada) * 60f);
      hash = hash * 31 + (personajeRepresentado.ActividadFijada ? 1 : 0);
      hash = hash * 31 + (personajeRepresentado.Camp_Muerto ? 1 : 0);
      hash = hash * 31 + (personajeRepresentado.Camp_Herido ? 1 : 0);
      hash = hash * 31 + (personajeRepresentado.Camp_Corrupto ? 1 : 0);
      hash = hash * 31 + personajeRepresentado.ObtenerHorasRestantesEnfermo().GetHashCode();
      hash = hash * 31 + personajeRepresentado.ObtenerEstadoMoralCampania().GetHashCode();
      hash = hash * 31 + personajeRepresentado.ObtenerHorasRestantesMoral().GetHashCode();
      hash = hash * 31 + (personajeRepresentado.Camp_Fatigado ? 1 : 0);
      hash = hash * 31 + personajeRepresentado.ObtenerHorasRestantesBendecido().GetHashCode();
      hash = hash * 31 + (personajeRepresentado.Camp_Avergonzado ? 1 : 0);
      hash = hash * 31 + (personajeRepresentado.spRetrato != null ? personajeRepresentado.spRetrato.GetInstanceID() : 0);
      return hash;
    }
  }
  public void RepresentarIconos()
  {
    AsegurarReferencias();

    if (iconoMuerto != null)
    {
      iconoMuerto.gameObject.SetActive(personajeRepresentado != null && personajeRepresentado.Camp_Muerto);
    }

    LimpiarEstadosCampania();

    if (personajeRepresentado != null)
    {
      if (personajeRepresentado.Camp_Muerto || contenedorEstadosCampania == null)
      {
        return;
      }

      if (personajeRepresentado.Camp_Herido)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Herido);
      }

      if (personajeRepresentado.Camp_Corrupto)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Corrupto);
      }

      if (personajeRepresentado.EstaEnfermo())
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Enfermo);
      }

      if (personajeRepresentado.TieneMoralBaja())
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.BajaMoral);
      }

      if (personajeRepresentado.TieneMoralAlta())
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.AltaMoral);
      }

      if (personajeRepresentado.Camp_Fatigado)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Fatigado);
      }

      if (personajeRepresentado.EstaBendecido())
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Bendecido);
      }

      if (personajeRepresentado.Camp_Avergonzado)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Avergonzado);
      }
    }
  }

  private void RepresentarRetrato()
  {
    AsegurarReferencias();

    Sprite spriteRetrato = personajeRepresentado != null
      ? personajeRepresentado.spRetrato
      : (refuerzoCaravanaRepresentado != null ? refuerzoCaravanaRepresentado.retrato : null);

    if (retratoRepresenta != null)
    {
      retratoRepresenta.sprite = spriteRetrato;
    }

    if (retratoSombraRepresenta != null)
    {
      retratoSombraRepresenta.sprite = spriteRetrato;
    }
  }

  private void RepresentarActividad()
  {
    AsegurarReferencias();

    if (EstaEnContenedorMenuBatallas())
    {
      if (actividadRetrato != null)
      {
        actividadRetrato.gameObject.SetActive(false);
      }

      if (indicadorActividadFijada != null)
      {
        indicadorActividadFijada.SetActive(false);
      }

      return;
    }

    if (indicadorActividadFijada != null)
    {
      indicadorActividadFijada.SetActive(personajeRepresentado != null && personajeRepresentado.ActividadFijada);
    }

    if (actividadRetrato == null)
    {
      return;
    }

    Actividades actividades = CampaignManager.Instance != null
      && CampaignManager.Instance.scMenuPersonajes != null
      ? CampaignManager.Instance.scMenuPersonajes.scActividades
      : null;

    if (personajeRepresentado == null || personajeRepresentado.Camp_Muerto || actividades == null)
    {
      actividadRetrato.gameObject.SetActive(false);
      if (indicadorActividadFijada != null)
      {
        indicadorActividadFijada.SetActive(false);
      }
      return;
    }

    Actividad actividadActual = actividades.ObtenerActividadActual(personajeRepresentado);
    if (actividadActual == null)
    {
      actividadRetrato.gameObject.SetActive(false);
      if (indicadorActividadFijada != null)
      {
        indicadorActividadFijada.SetActive(false);
      }
      return;
    }

    actividadRetrato.gameObject.SetActive(true);
    actividadRetrato.ConfigurarIndicadorRetrato(actividadActual, actividades, personajeRepresentado);
    actividadRetrato.actImage.sprite = actividades.ObtenerSpriteActividad(actividadActual.IDActividad);
    ActualizarProgresoActividadRetrato(actividadActual.IDActividad);
    if (actividadRetrato.Recuadro != null)
    {
      actividadRetrato.Recuadro.SetActive(false);
    }
  }

  private void ActualizarProgresoActividadRetrato(int actividadId)
  {
    if (actividadRetrato == null || actividadRetrato.actImage == null || personajeRepresentado == null)
    {
      return;
    }

    if (!CampaignManager.ActividadTieneResultadoCada24Horas(actividadId))
    {
      actividadRetrato.actImage.color = Color.white;
      if (progresoActividadFill != null)
      {
        progresoActividadFill.gameObject.SetActive(false);
      }
      return;
    }

    AsegurarIndicadorProgresoActividad();
    float horas = personajeRepresentado.ObtenerHorasActividad(actividadId);
    bool mostrarProgreso = horas > 0.001f;
    actividadRetrato.actImage.color = mostrarProgreso
      ? new Color(0.22f, 0.27f, 0.27f, 1f)
      : Color.white;
    progresoActividadFill.sprite = actividadRetrato.actImage.sprite;
    progresoActividadFill.fillAmount = Mathf.Clamp01(horas / 24f);
    progresoActividadFill.gameObject.SetActive(mostrarProgreso);
  }

  private void AsegurarIndicadorProgresoActividad()
  {
    if (progresoActividadFill != null)
    {
      return;
    }

    Transform padre = actividadRetrato.actImage.transform;
    Transform fillExistente = padre.Find("ProgresoActividadFill");
    if (fillExistente != null)
    {
      progresoActividadFill = fillExistente.GetComponent<Image>();
      TextMeshProUGUI textoExistente = fillExistente.GetComponentInChildren<TextMeshProUGUI>(true);
      if (textoExistente != null)
      {
        textoExistente.gameObject.SetActive(false);
      }
      if (progresoActividadFill != null)
      {
        progresoActividadFill.color = new Color(0.15f, 1f, 0.82f, 1f);
        return;
      }
    }

    GameObject fillGo = new GameObject("ProgresoActividadFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    fillGo.transform.SetParent(padre, false);
    RectTransform fillRect = fillGo.GetComponent<RectTransform>();
    fillRect.anchorMin = Vector2.zero;
    fillRect.anchorMax = Vector2.one;
    fillRect.offsetMin = Vector2.zero;
    fillRect.offsetMax = Vector2.zero;
    progresoActividadFill = fillGo.GetComponent<Image>();
    progresoActividadFill.type = Image.Type.Filled;
    progresoActividadFill.fillMethod = Image.FillMethod.Vertical;
    progresoActividadFill.fillOrigin = (int)Image.OriginVertical.Bottom;
    progresoActividadFill.fillClockwise = true;
    progresoActividadFill.preserveAspect = actividadRetrato.actImage.preserveAspect;
    progresoActividadFill.color = new Color(0.15f, 1f, 0.82f, 1f);
    progresoActividadFill.raycastTarget = false;
  }

  public void NotificarEntradaActividadRetrato(btnActividad actividad)
  {
    if (rutinaCierreOpcionesActividad != null)
    {
      StopCoroutine(rutinaCierreOpcionesActividad);
      rutinaCierreOpcionesActividad = null;
    }

    if (actividad == actividadRetrato)
    {
      MostrarOpcionesActividadRetrato();
    }
  }

  public void NotificarSalidaActividadRetrato()
  {
    if (rutinaCierreOpcionesActividad != null)
    {
      StopCoroutine(rutinaCierreOpcionesActividad);
    }

    rutinaCierreOpcionesActividad = StartCoroutine(CerrarOpcionesActividadTrasEspera());
  }

  public void CerrarOpcionesActividadRetrato(bool inmediato = false)
  {
    if (rutinaCierreOpcionesActividad != null)
    {
      StopCoroutine(rutinaCierreOpcionesActividad);
      rutinaCierreOpcionesActividad = null;
    }

    selectorActividadRetratoActivo = false;

    TooltipStats.Instance?.HideTooltip();
    TooltipItems.Instance?.HideTooltip();

    for (int i = opcionesActividadRetrato.Count - 1; i >= 0; i--)
    {
      btnActividad opcion = opcionesActividadRetrato[i];
      if (opcion == null)
      {
        continue;
      }

      if (inmediato || !gameObject.activeInHierarchy)
      {
        Destroy(opcion.gameObject);
      }
      else
      {
        StartCoroutine(AnimarSalidaOpcionActividad(opcion));
      }
    }

    opcionesActividadRetrato.Clear();
  }

  private void MostrarOpcionesActividadRetrato()
  {
    if (opcionesActividadRetrato.Count > 0
      || !abreMenuPersonajesAlHover
      || personajeRepresentado == null
      || personajeRepresentado.Camp_Muerto
      || !personajeRepresentado.PuedeRealizarActividades()
      || (CampaignManager.Instance != null && CampaignManager.Instance.HayBatallaPendiente())
      || actividadRetrato == null
      || EstaEnContenedorMenuBatallas())
    {
      return;
    }

    Actividades actividades = CampaignManager.Instance != null
      && CampaignManager.Instance.scMenuPersonajes != null
      ? CampaignManager.Instance.scMenuPersonajes.scActividades
      : null;
    if (actividades == null)
    {
      return;
    }

    int idActividadActual = actividadRetrato.actividadRepresentada != null
      ? actividadRetrato.actividadRepresentada.IDActividad
      : personajeRepresentado.ActividadSeleccionada;
    List<Actividad> actividadesDisponibles = new List<Actividad>(personajeRepresentado.GetComponents<Actividad>());
    actividadesDisponibles.RemoveAll(actividad => actividad == null || actividad.IDActividad == idActividadActual);
    actividadesDisponibles.Sort((a, b) => a.IDActividad.CompareTo(b.IDActividad));

    RectTransform rectActividadActual = actividadRetrato.transform as RectTransform;
    if (rectActividadActual == null)
    {
      return;
    }

    selectorActividadRetratoActivo = true;
    if (CampaignManager.Instance != null && CampaignManager.Instance.scMenuCaravana != null)
    {
      CampaignManager.Instance.scMenuCaravana.CerrarMenuPersonajesPorHover(personajeRepresentado);
    }

    Canvas canvasRaiz = actividadRetrato.GetComponentInParent<Canvas>();
    int ordenOpciones = canvasRaiz != null ? canvasRaiz.sortingOrder : 0;
    Canvas[] canvasesActivos = FindObjectsOfType<Canvas>();
    foreach (Canvas canvasActivo in canvasesActivos)
    {
      if (canvasActivo != null
        && canvasActivo.isActiveAndEnabled
        && (canvasRaiz == null || canvasActivo.sortingLayerID == canvasRaiz.sortingLayerID))
      {
        ordenOpciones = Mathf.Max(ordenOpciones, canvasActivo.sortingOrder);
      }
    }
    ordenOpciones++;

    int cantidadOpciones = Mathf.Min(4, actividadesDisponibles.Count);
    for (int i = 0; i < cantidadOpciones; i++)
    {
      btnActividad opcion = Instantiate(actividadRetrato, actividadRetrato.transform.parent);
      opcion.name = "OpcionActividadRetrato";
      opcion.ConfigurarOpcionRapidaRetrato(actividadesDisponibles[i], actividades, personajeRepresentado, this);
      if (opcion.actImage != null)
      {
        opcion.actImage.sprite = actividades.ObtenerSpriteActividad(actividadesDisponibles[i].IDActividad);
        opcion.actImage.color = Color.white;
        Transform progresoClonado = opcion.actImage.transform.Find("ProgresoActividadFill");
        if (progresoClonado != null)
        {
          progresoClonado.gameObject.SetActive(false);
        }
      }
      if (opcion.Recuadro != null)
      {
        opcion.Recuadro.SetActive(false);
      }

      RectTransform rectOpcion = opcion.transform as RectTransform;
      Vector2 posicionFinal = rectActividadActual.anchoredPosition + Vector2.up * EspaciadoOpcionesActividad * (i + 1);
      rectOpcion.anchoredPosition = posicionFinal + Vector2.down * 4f;
      rectOpcion.localScale = rectActividadActual.localScale * 0.82f;
      opcion.transform.SetAsLastSibling();

      Canvas canvasOpcion = opcion.GetComponent<Canvas>();
      if (canvasOpcion == null)
      {
        canvasOpcion = opcion.gameObject.AddComponent<Canvas>();
      }
      canvasOpcion.overrideSorting = true;
      canvasOpcion.sortingOrder = ordenOpciones;
      if (canvasRaiz != null)
      {
        canvasOpcion.sortingLayerID = canvasRaiz.sortingLayerID;
      }
      if (opcion.GetComponent<GraphicRaycaster>() == null)
      {
        opcion.gameObject.AddComponent<GraphicRaycaster>();
      }

      CanvasGroup canvasGroup = opcion.GetComponent<CanvasGroup>();
      if (canvasGroup == null)
      {
        canvasGroup = opcion.gameObject.AddComponent<CanvasGroup>();
      }
      canvasGroup.alpha = 0f;

      opcionesActividadRetrato.Add(opcion);
      StartCoroutine(AnimarEntradaOpcionActividad(rectOpcion, canvasGroup, posicionFinal, rectActividadActual.localScale, i * 0.025f));
    }
  }

  private IEnumerator CerrarOpcionesActividadTrasEspera()
  {
    yield return new WaitForSecondsRealtime(EsperaCierreOpcionesActividad);
    rutinaCierreOpcionesActividad = null;
    CerrarOpcionesActividadRetrato();
  }

  private IEnumerator AnimarEntradaOpcionActividad(RectTransform rectOpcion, CanvasGroup canvasGroup, Vector2 posicionFinal, Vector3 escalaFinal, float demora)
  {
    if (demora > 0f)
    {
      yield return new WaitForSecondsRealtime(demora);
    }

    if (rectOpcion == null || canvasGroup == null)
    {
      yield break;
    }

    Vector2 posicionInicial = rectOpcion.anchoredPosition;
    Vector3 escalaInicial = rectOpcion.localScale;
    float tiempo = 0f;
    while (tiempo < DuracionAnimacionOpcionesActividad && rectOpcion != null)
    {
      tiempo += Time.unscaledDeltaTime;
      float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / DuracionAnimacionOpcionesActividad));
      rectOpcion.anchoredPosition = Vector2.LerpUnclamped(posicionInicial, posicionFinal, t);
      rectOpcion.localScale = Vector3.LerpUnclamped(escalaInicial, escalaFinal, t);
      canvasGroup.alpha = t;
      yield return null;
    }

    if (rectOpcion != null)
    {
      rectOpcion.anchoredPosition = posicionFinal;
      rectOpcion.localScale = escalaFinal;
      canvasGroup.alpha = 1f;
    }
  }

  private IEnumerator AnimarSalidaOpcionActividad(btnActividad opcion)
  {
    if (opcion == null)
    {
      yield break;
    }

    CanvasGroup canvasGroup = opcion.GetComponent<CanvasGroup>();
    RectTransform rectOpcion = opcion.transform as RectTransform;
    if (canvasGroup != null)
    {
      canvasGroup.blocksRaycasts = false;
      canvasGroup.interactable = false;
    }
    float alphaInicial = canvasGroup != null ? canvasGroup.alpha : 1f;
    Vector3 escalaInicial = rectOpcion != null ? rectOpcion.localScale : Vector3.one;
    float tiempo = 0f;
    while (tiempo < DuracionAnimacionOpcionesActividad * 0.75f && opcion != null)
    {
      tiempo += Time.unscaledDeltaTime;
      float t = Mathf.Clamp01(tiempo / (DuracionAnimacionOpcionesActividad * 0.75f));
      if (canvasGroup != null)
      {
        canvasGroup.alpha = Mathf.Lerp(alphaInicial, 0f, t);
      }
      if (rectOpcion != null)
      {
        rectOpcion.localScale = Vector3.LerpUnclamped(escalaInicial, escalaInicial * 0.85f, t);
      }
      yield return null;
    }

    if (opcion != null)
    {
      Destroy(opcion.gameObject);
    }
  }

  private bool EstaEnContenedorMenuBatallas()
  {
    if (CampaignManager.Instance == null || CampaignManager.Instance.scMenuBatallas == null)
    {
      return false;
    }

    Transform parent = transform.parent;
    if (parent == null)
    {
      return false;
    }

    Transform contenedorBatalla = CampaignManager.Instance.scMenuBatallas.contenedorUIPersonajes != null
      ? CampaignManager.Instance.scMenuBatallas.contenedorUIPersonajes.transform
      : null;
    if (contenedorBatalla != null && parent == contenedorBatalla)
    {
      return true;
    }

    Transform contenedorBatallaFuera = CampaignManager.Instance.scMenuBatallas.contenedorUIPersonajesFuera != null
      ? CampaignManager.Instance.scMenuBatallas.contenedorUIPersonajesFuera.transform
      : null;
    return contenedorBatallaFuera != null && parent == contenedorBatallaFuera;
  }

  private void AjustarLayoutEstadosCampania()
  {
    RectTransform rectEstados = contenedorEstadosCampania as RectTransform;
    if (rectEstados == null)
    {
      return;
    }

    bool enMenuBatallas = EstaEnContenedorMenuBatallas();
    Vector2 anchor = enMenuBatallas ? AnchorEstadosMenuBatallas : AnchorEstadosCampania;
    rectEstados.anchorMin = anchor;
    rectEstados.anchorMax = anchor;
    rectEstados.anchoredPosition = enMenuBatallas ? PosicionEstadosMenuBatallas : PosicionEstadosCampania;
  }

  private string ObtenerNombreClaseTraducido(Personaje personaje)
  {
    if (personaje == null)
    {
      return "";
    }

    string nombreClase = ObtenerNombreClaseBase(personaje.IDClase);
    return string.IsNullOrEmpty(nombreClase) || TRADU.i == null
      ? nombreClase
      : TRADU.i.Traducir(nombreClase);
  }

  private string ObtenerNombreClaseBase(int idClase)
  {
    switch (idClase)
    {
      case 1: return "Caballero";
      case 2: return "Explorador";
      case 3: return "Purificadora";
      case 4: return "Acechador";
      case 5: return "Canalizador";
      case 6: return "Duelista";
      default: return "";
    }
  }

  private void AsegurarReferencias()
  {
    if (retratoRepresenta == null)
    {
      Transform retrato = transform.Find("RetratoMask/Retrato");
      if (retrato == null)
      {
        retrato = transform.Find("Retrato");
      }
      if (retrato != null)
      {
        retratoRepresenta = retrato.GetComponent<Image>();
      }
    }

    if (retratoSombraRepresenta == null)
    {
      Transform retratoSombra = transform.Find("RetratoMask/RetratoSombra");
      if (retratoSombra == null)
      {
        retratoSombra = transform.Find("RetratoSombra");
      }
      if (retratoSombra != null)
      {
        retratoSombraRepresenta = retratoSombra.GetComponent<Image>();
      }
    }

    if (actividadRetrato == null)
    {
      Transform actividad = transform.Find("btnActividad");
      if (actividad != null)
      {
        actividadRetrato = actividad.GetComponent<btnActividad>();
      }
    }

    if (indicadorActividadFijada == null)
    {
      Transform candadoActividad = transform.Find("imCandadoActividad");
      if (candadoActividad != null)
      {
        indicadorActividadFijada = candadoActividad.gameObject;
      }
    }

    if (contenedorEstadosCampania == null)
    {
      Transform estados = transform.Find("Estados");
      if (estados != null)
      {
        contenedorEstadosCampania = estados;
      }
    }

    if (indicadorNivelPendiente == null)
    {
      Transform nivelPendiente = transform.Find("NivelPendiente");
      if (nivelPendiente != null)
      {
        indicadorNivelPendiente = nivelPendiente.gameObject;
      }
    }

    if (indicadorSeleccionado == null)
    {
      Transform seleccionado = transform.Find("Seleccionado");
      if (seleccionado != null)
      {
        indicadorSeleccionado = seleccionado.gameObject;
      }
    }

    if (iconoMuerto == null)
    {
      Transform muerto = transform.Find("Muerto");
      if (muerto != null)
      {
        iconoMuerto = muerto.GetComponent<Image>();
      }
    }
  }

  private void LimpiarEstadosCampania()
  {
    if (contenedorEstadosCampania == null)
    {
      return;
    }

    for (int i = contenedorEstadosCampania.childCount - 1; i >= 0; i--)
    {
      Destroy(contenedorEstadosCampania.GetChild(i).gameObject);
    }
  }

  private void CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania tipoEstado)
  {
    if (contenedorEstadosCampania == null)
    {
      return;
    }

    GameObject goEstado = prefabEstadoCampania != null
      ? Instantiate(prefabEstadoCampania, contenedorEstadosCampania, false)
      : CrearEstadoCampaniaFallback();

    if (goEstado == null)
    {
      return;
    }

    goEstado.transform.localScale = Vector3.one;

    UIEstadoPersonajeCamp estadoUI = goEstado.GetComponent<UIEstadoPersonajeCamp>();
    if (estadoUI == null)
    {
      estadoUI = goEstado.AddComponent<UIEstadoPersonajeCamp>();
    }

    estadoUI.Representar(tipoEstado, personajeRepresentado);
  }

  private GameObject CrearEstadoCampaniaFallback()
  {
    GameObject goEstado = new GameObject(
      "camp_estado",
      typeof(RectTransform),
      typeof(CanvasRenderer),
      typeof(Image),
      typeof(UIEstadoPersonajeCamp));

    goEstado.transform.SetParent(contenedorEstadosCampania, false);

    RectTransform rectTransform = goEstado.GetComponent<RectTransform>();
    if (rectTransform != null)
    {
      rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
      rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
      rectTransform.pivot = new Vector2(0.5f, 0.5f);
      rectTransform.sizeDelta = new Vector2(25f, 25f);
    }

    Image image = goEstado.GetComponent<Image>();
    if (image != null)
    {
      image.preserveAspect = true;
      image.raycastTarget = true;
    }

    return goEstado;
  }
}


