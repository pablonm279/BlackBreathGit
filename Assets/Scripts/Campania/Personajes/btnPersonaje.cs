using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class btnPersonaje : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
  public Personaje personajeRepresentado;
  RefuerzoAliadoCaravanaOrdenItem refuerzoCaravanaRepresentado;
  SequitoCuranderos sequitoCuranderosTratamiento;

  public TextMeshProUGUI txtPersonajeRepresentado;
  [SerializeField] private Image retratoRepresenta;
  [SerializeField] private Image retratoSombraRepresenta;
  [SerializeField] private btnActividad actividadRetrato;
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
    if (!abreMenuPersonajesAlHover || personajeRepresentado == null || CampaignManager.Instance == null || CampaignManager.Instance.scMenuCaravana == null)
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
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<i><size=75%><color=#B8860B>" + prefijoClase + "Nv." + ((int)personajeRepresentado.fNivelActual) + "</color></size></i>";
        }
        if (TRADU.i.nIdioma == 2) //Inglés
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<i><size=75%><color=#B8860B>" + prefijoClase + "Lv." + ((int)personajeRepresentado.fNivelActual) + "</color></size></i>";
        }
         if (TRADU.i.nIdioma == 3) //Poertu
        {
          txtPersonajeRepresentado.text = personajeRepresentado.sNombre + "\n<i><size=75%><color=#B8860B>" + prefijoClase + "Nv." + ((int)personajeRepresentado.fNivelActual) + "</color></size></i>";
        }



      }
      else if (refuerzoCaravanaRepresentado != null)
      {
        txtPersonajeRepresentado.gameObject.SetActive(true);
        txtPersonajeRepresentado.text = refuerzoCaravanaRepresentado.nombreVisible;
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
        txtPersonajeRepresentado.text = refuerzoCaravanaRepresentado.nombreVisible;
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
    RepresentarIconos();
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

      if (personajeRepresentado.Camp_Enfermo > 0)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Enfermo);
      }

      if (personajeRepresentado.Camp_Moral < 0)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.BajaMoral);
      }

      if (personajeRepresentado.Camp_Moral > 0)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.AltaMoral);
      }

      if (personajeRepresentado.Camp_Fatigado)
      {
        CrearEstadoCampania(UIEstadoPersonajeCamp.TipoEstadoCampania.Fatigado);
      }

      if (personajeRepresentado.TieneCampBendecido())
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
    if (actividadRetrato.Recuadro != null)
    {
      actividadRetrato.Recuadro.SetActive(false);
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


