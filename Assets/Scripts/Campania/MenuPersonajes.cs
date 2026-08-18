using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEditor.SearchService;
using Unity.VisualScripting;

public class MenuPersonajes : MonoBehaviour
{
  const string ColorTituloRasgo = "#bfb7aa";

  public List<Personaje> listaPersonajes = new List<Personaje>(); //La lista que posee los personajes activos


  public GameObject prefabBtnPersonaje;
  public GameObject contenedorUIPersonajes;
  public Equipo scEquipo;

  public Actividades scActividades;


  public Sprite Male001;
  public Sprite Male002;
  public Sprite Male003; //Explorador
  public Sprite Male004; //Acechador
  public Sprite Male005; //Canalizador

  public Sprite Female001; //Purificadora
  public Sprite Female002; //Duelista

  public GameObject clasePreview;

  public GameObject clasePreviewCaballero;
  public GameObject clasePreviewExplorador;
  public GameObject clasePreviewAcechador;
  public GameObject clasePreviewCanalizador;
  public GameObject clasePreviewPurificadora;
  public GameObject clasePreviewDuelista;

  public Personaje pSel;

  //Base Stats personaje
  [SerializeField] TextMeshProUGUI txtNombre;
  [SerializeField] TextMeshProUGUI txtClase;
  [SerializeField] TextMeshProUGUI txtExperiencia;
  [SerializeField] TextMeshProUGUI txtNivel;
  [SerializeField] TextMeshProUGUI txtHP;
  [SerializeField] TextMeshProUGUI txtFuerza;
  [SerializeField] TextMeshProUGUI txtAgi;
  [SerializeField] TextMeshProUGUI txtPoder;
  [SerializeField] TextMeshProUGUI txtIniciativa;
  [SerializeField] TextMeshProUGUI txtApMax;
  [SerializeField] TextMeshProUGUI txtValMax;
  [SerializeField] TextMeshProUGUI txtArmadura;
  [SerializeField] TextMeshProUGUI txtDefensa;
  [SerializeField] TextMeshProUGUI txtTSReflejo;
  [SerializeField] TextMeshProUGUI txtTSFortaleza;
  [SerializeField] TextMeshProUGUI txtTSMental;
  [SerializeField] TextMeshProUGUI txtResFuego;
  [SerializeField] TextMeshProUGUI txtResRayo;
  [SerializeField] TextMeshProUGUI txtResHielo;
  [SerializeField] TextMeshProUGUI txtResArcano;
  [SerializeField] TextMeshProUGUI txtResAcido;
  [SerializeField] TextMeshProUGUI txtResNecro;
  [SerializeField] TextMeshProUGUI txtResDivino;
  [SerializeField] TextMeshProUGUI txtDiasViajado;
  [SerializeField] TextMeshProUGUI txtEnemigosEliminados;
  [SerializeField] TextMeshProUGUI txtDanioHecho;
  [SerializeField] TextMeshProUGUI txtDanioRecibido;
  [SerializeField] TextMeshProUGUI txtVecesDerribado;

  [SerializeField] TextMeshProUGUI txtContenedorRasgos;
  [SerializeField] TextMeshProUGUI txtCapacidadPersonajes;

  [SerializeField] Image imCorazon;
  [SerializeField] Image imMedalla;
  [SerializeField] Image RetratoGrande;
  [SerializeField] Image SombraRetratoGrande;

  [SerializeField] GameObject EstadosContainer;
  [SerializeField] GameObject EcharContainer;
  [SerializeField] TextMeshProUGUI txtEchar;

  private GameObject prefabEstadoCampania;

  private void Awake()
  {
    AsegurarTextoCapacidadPersonajes();
  }

  private void OnDisable()
  {
    if (scEquipo != null)
    {
      scEquipo.CerrarInventario();
    }

    LimpiarSeleccionVisual();
  }

  private void ActualizarPreviewClaseSeleccionada()
  {
    if (clasePreview == null)
    {
      return;
    }

    if (clasePreviewCaballero != null) clasePreviewCaballero.SetActive(false);
    if (clasePreviewExplorador != null) clasePreviewExplorador.SetActive(false);
    if (clasePreviewAcechador != null) clasePreviewAcechador.SetActive(false);
    if (clasePreviewCanalizador != null) clasePreviewCanalizador.SetActive(false);
    if (clasePreviewPurificadora != null) clasePreviewPurificadora.SetActive(false);
    if (clasePreviewDuelista != null) clasePreviewDuelista.SetActive(false);

    if (pSel == null)
    {
      return;
    }

    switch (pSel.IDClase)
    {
      case 1:
        if (clasePreviewCaballero != null) clasePreviewCaballero.SetActive(true);
        break;
      case 2:
        if (clasePreviewExplorador != null) clasePreviewExplorador.SetActive(true);
        break;
      case 3:
        if (clasePreviewPurificadora != null) clasePreviewPurificadora.SetActive(true);
        break;
      case 4:
        if (clasePreviewAcechador != null) clasePreviewAcechador.SetActive(true);
        break;
      case 5:
        if (clasePreviewCanalizador != null) clasePreviewCanalizador.SetActive(true);
        break;
      case 6:
        if (clasePreviewDuelista != null) clasePreviewDuelista.SetActive(true);
        break;
    }
  }

  public void abrirClasePreview()
  {
    if (clasePreview != null)
    {
      clasePreview.SetActive(true);
      ActualizarPreviewClaseSeleccionada();
    }
  }

  public void cerrarClasePreview()
  {
    if (clasePreview != null)
    {
      clasePreview.SetActive(false);
    }
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape) && clasePreview != null && clasePreview.activeInHierarchy)
    {
      cerrarClasePreview();
    }
  }

  public void AbrirEchar()
  {
    if (EcharContainer == null)
    {
      return;
    }

    ActualizarTextoEchar();
    EcharContainer.SetActive(!EcharContainer.activeInHierarchy);
  }

  public void PrepararYAbrirMenu(Personaje personajeInicial = null)
  {
    AsegurarTextoCapacidadPersonajes();
    ActualizarTextoCapacidadPersonajes();

    if (listaPersonajes.Count == 0)
    {
      ActualizarLista();
      ForzarRebuildInmediato();
      return;
    }

    Personaje personajeBase = personajeInicial != null && !personajeInicial.Camp_Muerto
      ? personajeInicial
      : listaPersonajes.Find(p => p != null && !p.Camp_Muerto);
    pSel = personajeBase != null ? personajeBase : listaPersonajes[0];
    if (scEquipo != null)
    {
      scEquipo.ConfigurarClickDerechoSlots(this);
    }
    if (ListaVisualPersonajesSincronizada())
    {
      RefrescarBotonesExistentes();
    }
    else
    {
      ActualizarLista();
    }
    ActualizarTextoCapacidadPersonajes();
    CancelInvoke("ActualizarInfo");
    ActualizarInfo();
    ForzarRebuildInmediato();
  }

  private bool ListaVisualPersonajesSincronizada()
  {
    if (contenedorUIPersonajes == null)
    {
      return false;
    }

    int personajesVisibles = 0;
    foreach (Personaje pers in listaPersonajes)
    {
      if (pers != null && !pers.Camp_Muerto)
      {
        personajesVisibles++;
      }
    }

    if (contenedorUIPersonajes.transform.childCount != personajesVisibles)
    {
      return false;
    }

    foreach (Transform child in contenedorUIPersonajes.transform)
    {
      btnPersonaje btn = child.GetComponent<btnPersonaje>();
      if (btn == null || btn.personajeRepresentado == null || btn.personajeRepresentado.Camp_Muerto)
      {
        return false;
      }
    }

    return true;
  }

  private void RefrescarBotonesExistentes()
  {
    if (contenedorUIPersonajes == null)
    {
      return;
    }

    bool mostrarSeleccion = DebeMostrarSeleccionVisual();

    foreach (Transform child in contenedorUIPersonajes.transform)
    {
      btnPersonaje btn = child.GetComponent<btnPersonaje>();

      if (btn != null)
      {
        btn.RepresentarTodo();
        btn.SetSeleccionado(mostrarSeleccion && pSel == btn.personajeRepresentado);
      }
    }
  }

  private void LimpiarSeleccionVisual()
  {
    if (contenedorUIPersonajes == null)
    {
      return;
    }

    foreach (Transform child in contenedorUIPersonajes.transform)
    {
      btnPersonaje btn = child.GetComponent<btnPersonaje>();
      if (btn != null)
      {
        btn.SetSeleccionado(false);
      }
    }
  }

  private bool DebeMostrarSeleccionVisual()
  {
    return isActiveAndEnabled && gameObject.activeInHierarchy;
  }

  public void RefrescarListaVisual()
  {
    if (ListaVisualPersonajesSincronizada())
    {
      RefrescarBotonesExistentes();
    }
    else
    {
      ActualizarLista();
    }
  }

  private void AsegurarTextoCapacidadPersonajes()
  {
    if (txtCapacidadPersonajes != null || txtNombre == null)
    {
      return;
    }

    Transform parent = txtNombre.transform.parent;
    if (parent == null)
    {
      return;
    }

    GameObject goCapacidad = Instantiate(txtNombre.gameObject, parent);
    goCapacidad.name = "txtCapacidadPersonajes";
    txtCapacidadPersonajes = goCapacidad.GetComponent<TextMeshProUGUI>();
    if (txtCapacidadPersonajes == null)
    {
      return;
    }

    RectTransform rt = txtCapacidadPersonajes.rectTransform;
    if (rt != null)
    {
      rt.anchorMin = new Vector2(1f, 1f);
      rt.anchorMax = new Vector2(1f, 1f);
      rt.pivot = new Vector2(1f, 1f);
      rt.anchoredPosition = new Vector2(-12f, -12f);
      rt.sizeDelta = new Vector2(120f, rt.sizeDelta.y);
    }

    txtCapacidadPersonajes.alignment = TextAlignmentOptions.TopRight;
    txtCapacidadPersonajes.fontSize = txtNombre.fontSize * 0.8f;
    txtCapacidadPersonajes.raycastTarget = false;
    txtCapacidadPersonajes.text = string.Empty;
  }

  private void ActualizarTextoCapacidadPersonajes()
  {
    if (txtCapacidadPersonajes == null)
    {
      AsegurarTextoCapacidadPersonajes();
    }

    if (txtCapacidadPersonajes == null)
    {
      return;
    }

    int actuales = CampaignManager.Instance != null ? CampaignManager.Instance.CuantosPersonajesActivos() : 0;
    int maximos = CampaignManager.Instance != null ? CampaignManager.Instance.ObtenerCapacidadMaximaPersonajes() : 4;
    txtCapacidadPersonajes.text = actuales + "/" + maximos;
  }

  public void ActualizarLista()
  {
    if (contenedorUIPersonajes == null)
    {
      ActualizarTextoCapacidadPersonajes();
      return;
    }

    foreach (Transform transform in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
      Destroy(transform.gameObject);
    }

    foreach (Personaje pers in listaPersonajes)
    {
      if (!pers.Camp_Muerto)
      {
        GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedorUIPersonajes.transform);
        btnPersonaje btn = btnPers.GetComponent<btnPersonaje>();
        if (btn != null)
        {
          btn.personajeRepresentado = pers;
          btn.ConfigurarHoverMenuPersonajesCampania(true);
        }
      }

    }

    foreach (Transform child in contenedorUIPersonajes.transform)
    {
      // Intenta obtener el componente btnPersonaje del hijo
      btnPersonaje btn = child.GetComponent<btnPersonaje>();

      if (btn != null) // Asegúrate de que el componente btnPersonaje exista
      {
        btn.RepresentarTodo();
        btn.SetSeleccionado(DebeMostrarSeleccionVisual() && pSel == btn.personajeRepresentado);
      }
    }

    ActualizarTextoCapacidadPersonajes();


  }


  public void SeleccionarPersonaje(Personaje pers, GameObject btnPers)
  {
    if (pers == null) return;

    pSel = pers;
    RuntimeAnalytics.TrackDesign("characters", "select", RuntimeAnalytics.ClassToken(pSel));
    RefrescarBotonesExistentes();
    ActualizarTextoEcharSiAbierto();

    CancelInvoke("ActualizarInfo");
    ActualizarInfo();
    if (clasePreview != null && clasePreview.activeInHierarchy)
    {
      ActualizarPreviewClaseSeleccionada();
    }
    if (scEquipo != null)
    {
      scEquipo.RefrescarInventarioSiAbierto();
    }
    ForzarRebuildInmediato();

  }

  public void EcharPersonajeSeleccionado()
  {
    if (pSel == null)
    {
      return;
    }

    Personaje personajeAEchar = pSel;
    int esperanzaPerdida = Mathf.Max(1, (int)personajeAEchar.fNivelActual) * 2;

    if (scEquipo != null)
    {
      scEquipo.CerrarInventario();
    }

    DestruirItemEquipadoSiExiste(personajeAEchar.itemArma);
    DestruirItemEquipadoSiExiste(personajeAEchar.itemArmadura);
    DestruirItemEquipadoSiExiste(personajeAEchar.Accesorio1);
    DestruirItemEquipadoSiExiste(personajeAEchar.Accesorio2);
    DestruirItemEquipadoSiExiste(personajeAEchar.Consumible1);
    DestruirItemEquipadoSiExiste(personajeAEchar.Consumible2);

    listaPersonajes.Remove(personajeAEchar);

    if (CampaignManager.Instance != null)
    {
      CampaignManager.Instance.CambiarEsperanzaActual(-esperanzaPerdida);
    }

    Destroy(personajeAEchar.gameObject);

    Personaje siguiente = listaPersonajes.Find(p => p != null && !p.Camp_Muerto);
    pSel = siguiente;

    if (pSel == null)
    {
      ActualizarLista();
      ActualizarTextoCapacidadPersonajes();
      if (EcharContainer != null)
      {
        EcharContainer.SetActive(false);
      }
      gameObject.SetActive(false);
      return;
    }
    AbrirEchar();
    PrepararYAbrirMenu(pSel);

  }

  private void DestruirItemEquipadoSiExiste(Item item)
  {
    if (item == null)
    {
      return;
    }

    if (scEquipo != null)
    {
      scEquipo.listInventario.Remove(item.gameObject);
    }

    Destroy(item.gameObject);
  }

  private void ActualizarTextoEcharSiAbierto()
  {
    if (EcharContainer != null && EcharContainer.activeInHierarchy)
    {
      ActualizarTextoEchar();
    }
  }

  private void ActualizarTextoEchar()
  {
    if (txtEchar == null)
    {
      return;
    }

    if (pSel == null)
    {
      txtEchar.text = string.Empty;
      return;
    }

    int esperanzaPerdida = Mathf.Max(1, (int)pSel.fNivelActual) * 2;
    txtEchar.text =
      TRADU.i.Traducir("Echar a ")
      + pSel.sNombre
      + TRADU.i.Traducir(" hará que se pierdan ")
      + esperanzaPerdida
      + TRADU.i.Traducir(" Esperanza. ¿Continuar?");
  }



  public void ActualizarInfo()
  {
    if (pSel == null)
    {
      return;
    }

    if (CampaignManager.Instance != null)
    {
      CampaignManager.Instance.SincronizarAparienciaVisualPersonaje(pSel);
    }

    pSel.NormalizarPuntosPendientesPorNivelActual();
    SelPos(pSel.iPuestoDeseado);
    //Clase
    switch (pSel.IDClase)
    {
      case 1: txtClase.text = TRADU.i.Traducir("Caballero"); break;
      case 2: txtClase.text = TRADU.i.Traducir("Explorador"); break;
      case 3: txtClase.text = TRADU.i.Traducir("Purificadora"); break;
      case 4: txtClase.text = TRADU.i.Traducir("Acechador"); break;
      case 5: txtClase.text = TRADU.i.Traducir("Canalizador"); break;
      case 6: txtClase.text = TRADU.i.Traducir("Duelista"); break;
        //----


    }
    if (contenedorUIPersonajes != null)
    {
      bool mostrarSeleccion = DebeMostrarSeleccionVisual();
      foreach (Transform transform in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
      {
        btnPersonaje btn = transform.gameObject.GetComponent<btnPersonaje>();
        if (btn != null)
        {
          btn.representarVida();
          btn.RepresentarIconos();
          btn.SetSeleccionado(mostrarSeleccion && pSel == btn.personajeRepresentado);
        }
      }
    }

    RepresentarRasgos();
    scEquipo.ActualizarEquipo(pSel);
    scActividades.ActualizarActividades();
    ActualizarListaHabilidades();

      

    //Info
    txtNombre.text = pSel.sNombre;
    if (RetratoGrande != null)
    {
      RetratoGrande.sprite = pSel.spRetrato;
      SombraRetratoGrande.sprite = pSel.spRetrato;
    }
    RepresentarEstadosCampaniaSeleccionado();
    float experienciaNecesaria = pSel.ObtenerExperienciaNecesariaParaProximoNivel();
    //txtExperiencia.text = $"" + pSel.fExperienciaActual + "/" + experienciaNecesaria;
    txtNivel.text = "" + pSel.fNivelActual;
    float vidaActualEscalada = pSel.ObtenerVidaActualConFuerza(scEquipo.BuffTOTALEQUIPOhpMax, scEquipo.BuffTOTALEQUIPOFuerza);
    float vidaMaxEscalada = pSel.ObtenerVidaMaximaConFuerza(scEquipo.BuffTOTALEQUIPOhpMax, scEquipo.BuffTOTALEQUIPOFuerza);
    imCorazon.fillAmount = Mathf.Clamp01(vidaActualEscalada / vidaMaxEscalada);
    txtHP.text = "" + (int)vidaActualEscalada + "/" + (int)vidaMaxEscalada;
    imMedalla.fillAmount = Mathf.Clamp01((float)pSel.fExperienciaActual / experienciaNecesaria);
    txtFuerza.text = TRADU.i.Traducir("Fuerza: ") + (pSel.iFuerza + scEquipo.BuffTOTALEQUIPOFuerza);
    txtAgi.text = TRADU.i.Traducir("Agilidad: ") + (pSel.iAgi + scEquipo.BuffTOTALEQUIPOAgi);
    txtPoder.text = TRADU.i.Traducir("Poder: ") + (pSel.iPoder + scEquipo.BuffTOTALEQUIPOPoder);
    txtIniciativa.text = TRADU.i.Traducir("Iniciativa: ") + (pSel.iIniciativa + scEquipo.BuffTOTALEQUIPOIniciativa);
    txtApMax.text = TRADU.i.Traducir("PA: ") + (pSel.iApMax + scEquipo.BuffTOTALEQUIPOApMax);
    txtValMax.text = TRADU.i.Traducir("Valentía: ") + (pSel.iValMax + scEquipo.BuffTOTALEQUIPOValMax);
    txtArmadura.text = TRADU.i.Traducir("Armadura: ") + (pSel.iArmadura + scEquipo.BuffTOTALEQUIPOArmadura);
    txtDefensa.text = TRADU.i.Traducir("Defensa: ") + Mathf.RoundToInt(pSel.ObtenerDefensaTotalConAgilidad(scEquipo.BuffTOTALEQUIPODefensa, scEquipo.BuffTOTALEQUIPOAgi));
    txtTSReflejo.text = TRADU.i.Traducir("-Reflejos: ") + (pSel.iTSReflejo + scEquipo.BuffTOTALEQUIPOTSReflejo);
    txtTSFortaleza.text = TRADU.i.Traducir("-Fortaleza: ") + (pSel.iTSFortaleza + scEquipo.BuffTOTALEQUIPOTSFortaleza);
    txtTSMental.text = TRADU.i.Traducir("-Mental: ") + (pSel.iTSMental + scEquipo.BuffTOTALEQUIPOTSMental);
    txtResFuego.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResFuego, scEquipo.BuffTOTALEQUIPOResFuego, scEquipo.BuffTOTALEQUIPOPoder);
    txtResRayo.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResRayo, scEquipo.BuffTOTALEQUIPOResRayo, scEquipo.BuffTOTALEQUIPOPoder);
    txtResHielo.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResHielo, scEquipo.BuffTOTALEQUIPOResHielo, scEquipo.BuffTOTALEQUIPOPoder);
    txtResArcano.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResArcano, scEquipo.BuffTOTALEQUIPOResArcano, scEquipo.BuffTOTALEQUIPOPoder);
    txtResAcido.text = "" + pSel.ObtenerResElementalConPoder(pSel.iResAcido, scEquipo.BuffTOTALEQUIPOResAcido, scEquipo.BuffTOTALEQUIPOPoder);
    txtResNecro.text = "" + (pSel.iResNecro + scEquipo.BuffTOTALEQUIPOResNecro);
    txtResDivino.text = "" + (pSel.iResDivino + scEquipo.BuffTOTALEQUIPOResDivino);
    if (txtDiasViajado != null)
    {
      txtDiasViajado.text = CampaignManager.Instance != null
        ? CampaignManager.Instance.FormatearDuracionHoras(pSel.HorasViajadas)
        : pSel.DiasViajado.ToString();
    }
    if (txtEnemigosEliminados != null) { txtEnemigosEliminados.text = pSel.EnemigosEliminados.ToString(); }
    if (txtDanioHecho != null) { txtDanioHecho.text = pSel.DanioHecho.ToString(); }
    if (txtDanioRecibido != null) { txtDanioRecibido.text = pSel.DanioRecibido.ToString(); }
    if (txtVecesDerribado != null) { txtVecesDerribado.text = pSel.VecesDerribado.ToString(); }

    Invoke("ActualizarInfoNivel", 0.05f);
    EmitirEstadoPuntosTutorial();
  }

  private void EmitirEstadoPuntosTutorial()
  {
    if (pSel == null)
    {
      return;
    }

    int puntosAtributo = Mathf.Max(0, pSel.NivelPuntoAtributo);
    int puntosHabilidad = Mathf.Max(0, pSel.NivelPuntoHabilidad);
    TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignCharacterPointsChanged, pSel.gameObject)
      .Add("classId", pSel.IDClase)
      .Add("isAcechador", pSel.IDClase == 4 ? 1 : 0)
      .Add("attributePoints", puntosAtributo)
      .Add("skillPoints", puntosHabilidad)
      .Add("noAttributeOrSkillPoints", puntosAtributo == 0 && puntosHabilidad == 0 ? 1 : 0));
  }

  private void RepresentarEstadosCampaniaSeleccionado()
  {
    LimpiarEstadosCampaniaSeleccionado();

    if (EstadosContainer == null || pSel == null || pSel.Camp_Muerto)
    {
      return;
    }

    if (pSel.Camp_Herido)
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.Herido);
    }

    if (pSel.Camp_Corrupto)
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.Corrupto);
    }

    if (pSel.EstaEnfermo())
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.Enfermo);
    }

    if (pSel.TieneMoralBaja())
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.BajaMoral);
    }

    if (pSel.TieneMoralAlta())
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.AltaMoral);
    }

    if (pSel.Camp_Fatigado)
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.Fatigado);
    }

    if (pSel.EstaBendecido())
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.Bendecido);
    }

    if (pSel.Camp_Avergonzado)
    {
      CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania.Avergonzado);
    }
  }

  private void LimpiarEstadosCampaniaSeleccionado()
  {
    if (EstadosContainer == null)
    {
      return;
    }

    for (int i = EstadosContainer.transform.childCount - 1; i >= 0; i--)
    {
      Destroy(EstadosContainer.transform.GetChild(i).gameObject);
    }
  }

  private void CrearEstadoCampaniaSeleccionado(UIEstadoPersonajeCamp.TipoEstadoCampania tipoEstado)
  {
    if (EstadosContainer == null)
    {
      return;
    }

    GameObject prefabEstado = ObtenerPrefabEstadoCampania();
    GameObject goEstado = prefabEstado != null
      ? Instantiate(prefabEstado, EstadosContainer.transform, false)
      : null;

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

    estadoUI.Representar(tipoEstado, pSel);
  }

  private GameObject ObtenerPrefabEstadoCampania()
  {
    if (prefabEstadoCampania != null)
    {
      return prefabEstadoCampania;
    }

    if (contenedorUIPersonajes == null)
    {
      return null;
    }

    foreach (Transform child in contenedorUIPersonajes.transform)
    {
      btnPersonaje btn = child.GetComponent<btnPersonaje>();
      if (btn == null)
      {
        continue;
      }

      prefabEstadoCampania = btn.ObtenerPrefabEstadoCampania();
      if (prefabEstadoCampania != null)
      {
        return prefabEstadoCampania;
      }
    }

    return null;
  }


  void RepresentarRasgos()
  {
    if (txtContenedorRasgos == null || pSel == null)
    {
      return;
    }

    List<string> lineas = new List<string>();
    int idioma = PersonajeTraitCatalog.ObtenerIdiomaActual();

    foreach (int rasgoId in pSel.EnumerarRasgosActivos())
    {
      if (PersonajeTraitCatalog.TryGet(rasgoId, out PersonajeTraitDefinition definicion))
      {
        string nombre = definicion.ObtenerNombre(idioma);
        if (string.IsNullOrWhiteSpace(nombre))
        {
          continue;
        }

        string descripcion = definicion.ObtenerDescripcion(idioma);
        string bloque = "<color=" + ColorTituloRasgo + ">" + nombre.ToUpperInvariant() + "</color>";

        if (!string.IsNullOrWhiteSpace(descripcion))
        {
          bloque += "\n" + descripcion;
        }
        lineas.Add(bloque);
      }
    }

    txtContenedorRasgos.text = string.Join("\n\n", lineas);
  }

  public Transform listaHab;
  public GameObject actionButtonPrefab;

  private static Transform BuscarHijoDirectoPorNombre(Transform padre, string nombre)
  {
    if (padre == null)
    {
      return null;
    }

    for (int i = 0; i < padre.childCount; i++)
    {
      Transform hijo = padre.GetChild(i);
      if (hijo != null && hijo.name == nombre)
      {
        return hijo;
      }
    }

    return null;
  }

  public void ActualizarListaHabilidades()
  {
    foreach (Transform buttonTransform in listaHab)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
      Destroy(buttonTransform.gameObject);
    }

    foreach (Habilidad habilidad in pSel.gameObject.GetComponents<Habilidad>())
    {
      //Habilidades que no se muestran
      if (habilidad is RetrasarTurno)
      {
        continue;
      }
      if (habilidad is AtaqueBasico)
      {
        continue;
      }

      habilidad.ActualizarDescripcion();

      GameObject actionButtonTransform = Instantiate(actionButtonPrefab, listaHab);
      BotonHabilidad habilidadBotonUI = actionButtonTransform.GetComponent<BotonHabilidad>();
      habilidadBotonUI.HabilidadRepresentada = habilidad;
      Transform subirNivel = BuscarHijoDirectoPorNombre(actionButtonTransform.transform, "-SubirNivel");

      if (subirNivel != null && pSel.NivelPuntoHabilidad > 0 && habilidadBotonUI.HabilidadRepresentada.NIVEL < 4 && habilidadBotonUI.HabilidadRepresentada.NIVEL > 0)
      {
        subirNivel.gameObject.SetActive(true);

        bool mostrarEspecializacion = habilidadBotonUI.HabilidadRepresentada.NIVEL == 3;
        if (subirNivel.childCount > 0)
        {
          subirNivel.GetChild(0).gameObject.SetActive(!mostrarEspecializacion);
        }
        if (subirNivel.childCount > 1)
        {
          subirNivel.GetChild(1).gameObject.SetActive(mostrarEspecializacion);
        }
      }
      else if (subirNivel != null)
      {
        subirNivel.gameObject.SetActive(false);
      }
      actionButtonTransform.GetComponent<BotonHabilidad>().scMenuPersonajes = this;





    }


  }

  public TextMeshProUGUI itemDesc;
  private bool ignorarClickIzquierdoPorClickDerechoEquipo;
  private float tiempoBloqueoClickDerechoEquipoHasta;

  public void OnClickCofre()
  {
    if (!scEquipo.goInventario.activeInHierarchy)
    {
      RuntimeAnalytics.TrackDesign("characters", "inventory_open", "backpack");
      scEquipo.MostrarInventario(5);
    }
    else { scEquipo.goInventario.SetActive(false); }


  }
  public void OnClickArma()
  {
    if (FueClickDerechoEnSlotEquipo())
    {
      OnRightClickArma();
      return;
    }

    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "weapon");
    AlternarInventarioDeEquipo(1);
  }
  public void OnClickArmadura()
  {
    if (FueClickDerechoEnSlotEquipo())
    {
      OnRightClickArmadura();
      return;
    }

    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "armor");
    AlternarInventarioDeEquipo(2);
  }


  public void OnHoverArma()
  {
    if (pSel.itemArma != null)
    {
      //itemDesc.text = pSel.itemArma.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = ItemTooltipFormatter.ConstruirTooltip(pSel.itemArma, true);
      TooltipItems.Instance.ShowTooltip(total, pos, pSel.itemArma);

    }
  }

  public void OnHoverArmadura()
  {
    if (pSel != null)
    {
      if (pSel.itemArmadura != null)
      {
        // itemDesc.text = pSel.itemArmadura.itemDescrpicion;
         Vector3 pos = Input.mousePosition;
     string total = ItemTooltipFormatter.ConstruirTooltip(pSel.itemArmadura, true);
      TooltipItems.Instance.ShowTooltip(total, pos, pSel.itemArmadura);

      }
    }
  }


  public void OnClickAccesorio1()
  {
    if (FueClickDerechoEnSlotEquipo())
    {
      OnRightClickAccesorio1();
      return;
    }

    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "accessory_1");
    AlternarInventarioDeEquipo(3, accesorioSlot: 1);
  }

  public void OnHoverAccesorio1()
  {
    if (pSel.Accesorio1 != null)
    {
      //itemDesc.text = pSel.Accesorio1.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Accesorio1, true);
      TooltipItems.Instance.ShowTooltip(total, pos, pSel.Accesorio1);

    }
  }

  public void OnClickAccesorio2()
  {
    if (FueClickDerechoEnSlotEquipo())
    {
      OnRightClickAccesorio2();
      return;
    }

    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "accessory_2");
    AlternarInventarioDeEquipo(3, accesorioSlot: 2);
  }

  public void OnHoverAccesorio2()
  {
    if (pSel.Accesorio2 != null)
    {
      //itemDesc.text = pSel.Accesorio2.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
      string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Accesorio2, true);
      TooltipItems.Instance.ShowTooltip(total, pos, pSel.Accesorio2);

    }
  }


  public void OnClickConsumible1()
  {
    if (FueClickDerechoEnSlotEquipo())
    {
      OnRightClickConsumible1();
      return;
    }

    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "consumable_1");
    AlternarInventarioDeEquipo(4, consumibleSlot: 1);
  }

  public void OnHoverConsumible1()
  {
    if (pSel.Consumible1 != null)
    {
      //itemDesc.text = pSel.Consumible1.itemDescrpicion;
        Vector3 pos = Input.mousePosition;
     string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Consumible1, true);
      TooltipItems.Instance.ShowTooltip(total, pos, pSel.Consumible1);

    }
  }

  public void OnClickConsumible2()
  {
    if (FueClickDerechoEnSlotEquipo())
    {
      OnRightClickConsumible2();
      return;
    }

    if (DebeIgnorarClickSlotPorBloqueoDerecho())
    {
      return;
    }

    TooltipItems.Instance.HideTooltip();
    RuntimeAnalytics.TrackDesign("characters", "inventory_open", "consumable_2");
    AlternarInventarioDeEquipo(4, consumibleSlot: 2);
  }

  public void OnHoverConsumible2()
  {
    if (pSel.Consumible2 != null)
    {
      //itemDesc.text = pSel.Consumible2.itemDescrpicion;
      Vector3 pos = Input.mousePosition;
     string total = ItemTooltipFormatter.ConstruirTooltip(pSel.Consumible2, true);
      TooltipItems.Instance.ShowTooltip(total, pos, pSel.Consumible2);

    }
  }

  public void RegistrarClickDerechoEnSlotEquipo()
  {
    ignorarClickIzquierdoPorClickDerechoEquipo = true;
    tiempoBloqueoClickDerechoEquipoHasta = Time.unscaledTime + 0.25f;
  }

  public void LimpiarBloqueoClickDerechoEnSlotEquipo()
  {
    StartCoroutine(LimpiarBloqueoClickDerechoEnSlotEquipoDelay());
  }

  private IEnumerator LimpiarBloqueoClickDerechoEnSlotEquipoDelay()
  {
    yield return null;
    ignorarClickIzquierdoPorClickDerechoEquipo = false;
    tiempoBloqueoClickDerechoEquipoHasta = 0f;
  }

  private bool DebeIgnorarClickSlotPorBloqueoDerecho()
  {
    if (!ignorarClickIzquierdoPorClickDerechoEquipo)
    {
      return false;
    }

    if (Time.unscaledTime > tiempoBloqueoClickDerechoEquipoHasta)
    {
      ignorarClickIzquierdoPorClickDerechoEquipo = false;
      tiempoBloqueoClickDerechoEquipoHasta = 0f;
      return false;
    }

    return true;
  }

  private static bool FueClickDerechoEnSlotEquipo()
  {
    return Input.GetMouseButton(1) || Input.GetMouseButtonUp(1);
  }

  private void AbrirInventarioDeEquipo(int tipo)
  {
    if (scEquipo == null)
    {
      return;
    }

    scEquipo.MostrarInventario(tipo);
  }

  private void AlternarInventarioDeEquipo(int tipo, int accesorioSlot = 0, int consumibleSlot = 0)
  {
    if (scEquipo == null || scEquipo.goInventario == null)
    {
      return;
    }

    bool mismoTipo = scEquipo.TipoInventarioAbierto == tipo;
    bool mismoAccesorio = tipo != 3 || scEquipo.accesorioACambiar == accesorioSlot;
    bool mismoConsumible = tipo != 4 || scEquipo.consumibleACambiar == consumibleSlot;

    if (scEquipo.goInventario.activeInHierarchy && mismoTipo && mismoAccesorio && mismoConsumible)
    {
      scEquipo.CerrarInventario();
      return;
    }

    if (tipo == 3 && accesorioSlot > 0)
    {
      scEquipo.accesorioACambiar = accesorioSlot;
    }

    if (tipo == 4 && consumibleSlot > 0)
    {
      scEquipo.consumibleACambiar = consumibleSlot;
    }

    AbrirInventarioDeEquipo(tipo);
  }

  private void RefrescarInventarioDeEquipoSiEstaAbierto(int tipo)
  {
    if (scEquipo != null && scEquipo.goInventario != null && scEquipo.goInventario.activeInHierarchy)
    {
      scEquipo.MostrarInventario(tipo);
    }
  }

  private void AgregarAlInventarioSiHaceFalta(GameObject itemGO)
  {
    if (itemGO == null || scEquipo == null)
    {
      return;
    }

    if (!scEquipo.listInventario.Contains(itemGO))
    {
      scEquipo.listInventario.Add(itemGO);
    }
  }

  public Item ObtenerItemEquipadoQueReemplazaria(Item nuevoItem)
  {
    if (nuevoItem == null || pSel == null || !nuevoItem.PuedeUsarClase(pSel.IDClase))
    {
      return null;
    }

    Arma nuevaArma = nuevoItem as Arma;
    if (nuevaArma != null)
    {
      if (nuevaArma.requisitoAgi > pSel.iAgi || nuevaArma.requisitoFue > pSel.iFuerza || nuevaArma.requisitoPoder > pSel.iPoder)
      {
        return null;
      }

      return pSel.itemArma;
    }

    Armadura nuevaArmadura = nuevoItem as Armadura;
    if (nuevaArmadura != null)
    {
      if (nuevaArmadura.requisitoAgi > pSel.iAgi || nuevaArmadura.requisitoFue > pSel.iFuerza || nuevaArmadura.requisitoPoder > pSel.iPoder)
      {
        return null;
      }

      return pSel.itemArmadura;
    }

    Accesorio nuevoAccesorio = nuevoItem as Accesorio;
    if (nuevoAccesorio != null)
    {
      if (nuevoAccesorio.requisitoAgi > pSel.iAgi || nuevoAccesorio.requisitoFue > pSel.iFuerza || nuevoAccesorio.requisitoPoder > pSel.iPoder)
      {
        return null;
      }

      bool cambiarSlot2 = scEquipo != null && scEquipo.accesorioACambiar == 2;
      return cambiarSlot2 ? pSel.Accesorio2 : pSel.Accesorio1;
    }

    if (nuevoItem is Consumible)
    {
      bool cambiarSlot2 = scEquipo != null && scEquipo.consumibleACambiar == 2;
      return cambiarSlot2 ? pSel.Consumible2 : pSel.Consumible1;
    }

    return null;
  }

  public bool EquiparArmaDesdeInventario(Arma nuevaArma)
  {
    if (nuevaArma == null || pSel == null)
    {
      return false;
    }

    if (!nuevaArma.PuedeUsarClase(pSel.IDClase)) { return false; }
    if (nuevaArma.requisitoAgi > pSel.iAgi) { return false; }
    if (nuevaArma.requisitoFue > pSel.iFuerza) { return false; }
    if (nuevaArma.requisitoPoder > pSel.iPoder) { return false; }

    Arma armaAnterior = pSel.itemArma;
    if (armaAnterior != null)
    {
      pSel.QuitarArma(armaAnterior);
      AgregarAlInventarioSiHaceFalta(armaAnterior.gameObject);
    }

    pSel.itemArma = nuevaArma;
    scEquipo.listInventario.Remove(nuevaArma.gameObject);
    return true;
  }

  public bool EquiparArmaduraDesdeInventario(Armadura nuevaArmadura)
  {
    if (nuevaArmadura == null || pSel == null)
    {
      return false;
    }

    if (!nuevaArmadura.PuedeUsarClase(pSel.IDClase)) { return false; }
    if (nuevaArmadura.requisitoAgi > pSel.iAgi) { return false; }
    if (nuevaArmadura.requisitoFue > pSel.iFuerza) { return false; }
    if (nuevaArmadura.requisitoPoder > pSel.iPoder) { return false; }

    Armadura armaduraAnterior = pSel.itemArmadura;
    if (armaduraAnterior != null)
    {
      pSel.QuitarArmadura(armaduraAnterior);
      AgregarAlInventarioSiHaceFalta(armaduraAnterior.gameObject);
    }

    pSel.itemArmadura = nuevaArmadura;
    scEquipo.listInventario.Remove(nuevaArmadura.gameObject);
    return true;
  }

  public bool EquiparAccesorioDesdeInventario(Accesorio nuevoAccesorio)
  {
    if (nuevoAccesorio == null || pSel == null)
    {
      return false;
    }

    if (!nuevoAccesorio.PuedeUsarClase(pSel.IDClase)) { return false; }
    if (nuevoAccesorio.requisitoAgi > pSel.iAgi) { return false; }
    if (nuevoAccesorio.requisitoFue > pSel.iFuerza) { return false; }
    if (nuevoAccesorio.requisitoPoder > pSel.iPoder) { return false; }

    bool cambiarSlot2 = scEquipo != null && scEquipo.accesorioACambiar == 2;
    if (cambiarSlot2)
    {
      Accesorio accesorioAnterior = pSel.Accesorio2;
      if (accesorioAnterior != null)
      {
        pSel.QuitarAccesorio2(accesorioAnterior);
        AgregarAlInventarioSiHaceFalta(accesorioAnterior.gameObject);
      }

      pSel.Accesorio2 = nuevoAccesorio;
    }
    else
    {
      Accesorio accesorioAnterior = pSel.Accesorio1;
      if (accesorioAnterior != null)
      {
        pSel.QuitarAccesorio1(accesorioAnterior);
        AgregarAlInventarioSiHaceFalta(accesorioAnterior.gameObject);
      }

      pSel.Accesorio1 = nuevoAccesorio;
    }

    scEquipo.listInventario.Remove(nuevoAccesorio.gameObject);
    return true;
  }

  public bool EquiparConsumibleDesdeInventario(Consumible nuevoConsumible)
  {
    if (nuevoConsumible == null || pSel == null)
    {
      return false;
    }

    if (!nuevoConsumible.PuedeUsarClase(pSel.IDClase)) { return false; }
    bool cambiarSlot2 = scEquipo != null && scEquipo.consumibleACambiar == 2;
    if (cambiarSlot2)
    {
      Consumible consumibleAnterior = pSel.Consumible2;
      if (consumibleAnterior != null)
      {
        pSel.QuitarConsumible2(consumibleAnterior);
        AgregarAlInventarioSiHaceFalta(consumibleAnterior.gameObject);
      }

      pSel.Consumible2 = nuevoConsumible;
    }
    else
    {
      Consumible consumibleAnterior = pSel.Consumible1;
      if (consumibleAnterior != null)
      {
        pSel.QuitarConsumible1(consumibleAnterior);
        AgregarAlInventarioSiHaceFalta(consumibleAnterior.gameObject);
      }

      pSel.Consumible1 = nuevoConsumible;
    }

    scEquipo.listInventario.Remove(nuevoConsumible.gameObject);
    return true;
  }

  public void OnRightClickArma()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.itemArma == null)
    {
      return;
    }

    Arma armaAQuitar = pSel.itemArma;
    pSel.QuitarArma(armaAQuitar);
    AgregarAlInventarioSiHaceFalta(armaAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(1);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickArmadura()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.itemArmadura == null)
    {
      return;
    }

    Armadura armaduraAQuitar = pSel.itemArmadura;
    pSel.QuitarArmadura(armaduraAQuitar);
    AgregarAlInventarioSiHaceFalta(armaduraAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(2);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickAccesorio1()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Accesorio1 == null)
    {
      return;
    }

    Accesorio accesorioAQuitar = pSel.Accesorio1;
    pSel.QuitarAccesorio1(accesorioAQuitar);
    AgregarAlInventarioSiHaceFalta(accesorioAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(3);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickAccesorio2()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Accesorio2 == null)
    {
      return;
    }

    Accesorio accesorioAQuitar = pSel.Accesorio2;
    pSel.QuitarAccesorio2(accesorioAQuitar);
    AgregarAlInventarioSiHaceFalta(accesorioAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(3);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickConsumible1()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Consumible1 == null)
    {
      return;
    }

    Consumible consumibleAQuitar = pSel.Consumible1;
    pSel.QuitarConsumible1(consumibleAQuitar);
    AgregarAlInventarioSiHaceFalta(consumibleAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(4);
    Invoke("ActualizarInfo", 0.05f);
  }

  public void OnRightClickConsumible2()
  {
    TooltipItems.Instance.HideTooltip();
    if (pSel == null || pSel.Consumible2 == null)
    {
      return;
    }

    Consumible consumibleAQuitar = pSel.Consumible2;
    pSel.QuitarConsumible2(consumibleAQuitar);
    AgregarAlInventarioSiHaceFalta(consumibleAQuitar.gameObject);
    RefrescarInventarioDeEquipoSiEstaAbierto(4);
    Invoke("ActualizarInfo", 0.05f);
  }

  [SerializeField] GameObject SubirNivelAtributo;
  [SerializeField] GameObject SubirNivelTS;
  [SerializeField] GameObject SubirNivelHabilidad;
  [SerializeField] GameObject HabilidadBaseNueva;
  [SerializeField] Transform ListaElegirHabilidad;

  public List<Habilidad> poolSortear;

  public bool yaTiroHabRand = false;

  public void LimpiarComponentesHab()
  {
    // Remover todos los componentes de tipo Habilidad de ListaElegirHabilidad
    foreach (var habilidad in ListaElegirHabilidad.GetComponents<Habilidad>())
    {
      Destroy(habilidad);
    }
    poolSortear.Clear();
  }
  public void notHoverItem()
  { 
     TooltipItems.Instance.HideTooltip();

  }
  void ActualizarInfoNivel()
  {
    if (pSel.NivelPuntoAtributo > 0)
    {
      SubirNivelAtributo.SetActive(true);

    }
    else { SubirNivelAtributo.SetActive(false); }

    if (pSel.NivelPuntoTS > 0)
    {
      SubirNivelTS.SetActive(true);

    }
    else { SubirNivelTS.SetActive(false); }

    if (pSel.NivelPuntoHabilidad > 0)
    {
      SubirNivelHabilidad.SetActive(true);

    }
    else { SubirNivelHabilidad.SetActive(false); }

    if (pSel.NivelPuntoHabilidad > 0)
    {
      HabilidadBaseNueva.SetActive(false);
      yaTiroHabRand = false;
      return;
    }

    //Habilidad Base Nueva
    if (pSel.NivelNuevaHabilidadBase > 0)
    {
      if (!yaTiroHabRand)
      {
        yaTiroHabRand = true;
        HabilidadBaseNueva.SetActive(true);
        foreach (Transform buttonTransform in ListaElegirHabilidad)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
        {
          Destroy(buttonTransform.gameObject);
        }
        LimpiarComponentesHab();


        if (pSel.IDClase == 1) //Caballero
        {

          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAcorazado>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAcorazado>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<GritoMotivador>() == null)
          {
            ListaElegirHabilidad.AddComponent<GritoMotivador>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<CorteHorizontal>() == null)
          {
            ListaElegirHabilidad.AddComponent<CorteHorizontal>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<PrimerosAuxilios>() == null)
          {
            ListaElegirHabilidad.AddComponent<PrimerosAuxilios>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<REPRESENTACIONDeterminacion>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONDeterminacion>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<Partir>() == null)
          {
            ListaElegirHabilidad.AddComponent<Partir>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<PosturaDefensiva>() == null)
          {
            ListaElegirHabilidad.AddComponent<PosturaDefensiva>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<SiguesTu>() == null)
          {
            ListaElegirHabilidad.AddComponent<SiguesTu>();
          }
        }
        if (pSel.IDClase == 2) //Explorador
        {

          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONVistaLejana>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONVistaLejana>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONAcrobatico>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAcrobatico>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<MarcarPresa>() == null)
          {
            ListaElegirHabilidad.AddComponent<MarcarPresa>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<DisparoPotente>() == null)
          {
            ListaElegirHabilidad.AddComponent<DisparoPotente>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<Vigilancia>() == null)
          {
            ListaElegirHabilidad.AddComponent<Vigilancia>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<Acechar>() == null)
          {
            ListaElegirHabilidad.AddComponent<Acechar>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<Fogata>() == null)
          {
            ListaElegirHabilidad.AddComponent<Fogata>();
          }
          //Explorador tiene una menos por la de crear flechas que es intrinseca.
        }
        if (pSel.IDClase == 3) //Purificadora
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAuraSagrada>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAuraSagrada>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONEcosDivinos>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONEcosDivinos>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<Enmendar>() == null)
          {
            ListaElegirHabilidad.AddComponent<Enmendar>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<LuzCegadora>() == null)
          {
            ListaElegirHabilidad.AddComponent<LuzCegadora>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<PilaresDeLuz>() == null)
          {
            ListaElegirHabilidad.AddComponent<PilaresDeLuz>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<SalmoPurificador>() == null)
          {
            ListaElegirHabilidad.AddComponent<SalmoPurificador>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<LlamaDivina>() == null)
          {
            ListaElegirHabilidad.AddComponent<LlamaDivina>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<CastigaraLosMalvados>() == null)
          {
            ListaElegirHabilidad.AddComponent<CastigaraLosMalvados>();
          }
        }
        if (pSel.IDClase == 4) //Acechador
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONMaestriaBallesta>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONMaestriaBallesta>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONMaestriaEspadaCorta>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONMaestriaEspadaCorta>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<DisparoEnvenenado>() == null)
          {
            ListaElegirHabilidad.AddComponent<DisparoEnvenenado>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<CorteIncapacitante>() == null)
          {
            ListaElegirHabilidad.AddComponent<CorteIncapacitante>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<BombaDeHumo>() == null)
          {
            ListaElegirHabilidad.AddComponent<BombaDeHumo>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<Asesinar>() == null)
          {
            ListaElegirHabilidad.AddComponent<Asesinar>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<Distraer>() == null)
          {
            ListaElegirHabilidad.AddComponent<Distraer>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<ArrojarAbrojos>() == null)
          {
            ListaElegirHabilidad.AddComponent<ArrojarAbrojos>();
          }
        }
        if (pSel.IDClase == 5) //Canalizador
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAcumulacionProtegida>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAcumulacionProtegida>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<DescargaDePoder>() == null)
          {
            ListaElegirHabilidad.AddComponent<DescargaDePoder>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<Instatransporte>() == null)
          {
            ListaElegirHabilidad.AddComponent<Instatransporte>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<AcumulacionInestable>() == null)
          {
            ListaElegirHabilidad.AddComponent<AcumulacionInestable>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<HojaDeEnergia>() == null)
          {
            ListaElegirHabilidad.AddComponent<HojaDeEnergia>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<EscudoEnergetico>() == null)
          {
            ListaElegirHabilidad.AddComponent<EscudoEnergetico>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<SifonArcano>() == null)
          {
            ListaElegirHabilidad.AddComponent<SifonArcano>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<REPRESENTACIONExcesoDePoder>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONExcesoDePoder>();
          }

        }
        if (pSel.IDClase == 6) //Duelista
        {
          if (pSel.Habilidad_1 == 0 && pSel.GetComponent<REPRESENTACIONAtaquesReveladores>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONAtaquesReveladores>();
          }
          if (pSel.Habilidad_2 == 0 && pSel.GetComponent<REPRESENTACIONEvasionMaestra>() == null)
          {
            ListaElegirHabilidad.AddComponent<REPRESENTACIONEvasionMaestra>();
          }
          if (pSel.Habilidad_3 == 0 && pSel.GetComponent<CargaDeEstoque>() == null)
          {
            ListaElegirHabilidad.AddComponent<CargaDeEstoque>();
          }
          if (pSel.Habilidad_4 == 0 && pSel.GetComponent<Riposte>() == null)
          {
            ListaElegirHabilidad.AddComponent<Riposte>();
          }
          if (pSel.Habilidad_5 == 0 && pSel.GetComponent<AFondo>() == null)
          {
            ListaElegirHabilidad.AddComponent<AFondo>();
          }
          if (pSel.Habilidad_6 == 0 && pSel.GetComponent<EnGarde>() == null)
          {
            ListaElegirHabilidad.AddComponent<EnGarde>();
          }
          if (pSel.Habilidad_7 == 0 && pSel.GetComponent<PuntaHiriente>() == null)
          {
            ListaElegirHabilidad.AddComponent<PuntaHiriente>();
          }
          if (pSel.Habilidad_8 == 0 && pSel.GetComponent<RecuperarAire>() == null)
          {
            ListaElegirHabilidad.AddComponent<RecuperarAire>();
          }
        }



        foreach (Habilidad habilidad in ListaElegirHabilidad.gameObject.GetComponents<Habilidad>())
        {

          poolSortear.Add(habilidad);

        }
        if (poolSortear.Count == 0)
        {
          HabilidadBaseNueva.SetActive(false);
          yaTiroHabRand = false;
          return;
        }
        // Verificar si hay más de 3 habilidades en la lista
        if (poolSortear.Count > 3)
        {
          // Crear una instancia de Random
          System.Random random = new System.Random();

          // Crear una nueva lista que contendrá las habilidades seleccionadas al azar
          List<Habilidad> habilidadesSeleccionadas = new List<Habilidad>();

          // Seleccionar 3 elementos al azar
          for (int i = 0; i < 3; i++)
          {
            int indexAleatorio =random.Next(poolSortear.Count);
            habilidadesSeleccionadas.Add(poolSortear[indexAleatorio]);
            poolSortear.RemoveAt(indexAleatorio); // Eliminar el elemento seleccionado de la lista original
          }

          // Asignar la lista de habilidades seleccionadas de vuelta a poolSortear
          poolSortear = habilidadesSeleccionadas;
        }

        foreach (Habilidad habilidad in poolSortear)
        {
          GameObject actionButtonTransform = Instantiate(actionButtonPrefab, ListaElegirHabilidad);
          BotonHabilidad habilidadBotonUI = actionButtonTransform.GetComponent<BotonHabilidad>();
          habilidadBotonUI.HabilidadRepresentada = habilidad;
          Transform seleccionarNueva = BuscarHijoDirectoPorNombre(actionButtonTransform.transform, "-SeleccionarNueva");
          if (seleccionarNueva != null)
          {
            seleccionarNueva.gameObject.SetActive(true);
          }

        }
      }
    }
    else { HabilidadBaseNueva.SetActive(false); }
  }

  public void SubirAtributo(int i)
  {
    string atributoAnalytics = null;
    if (i == 1)//1-Fuerza
    {
      pSel.iFuerza++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
      atributoAnalytics = "fuerza";
    }
    if (i == 2)//2-Agiliadd
    {
      pSel.iAgi++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
      atributoAnalytics = "agilidad";
    }
    if (i == 3)//3-Poder
    {
      pSel.iPoder++;
      pSel.NivelPuntoAtributo--;
      ActualizarInfo();
      atributoAnalytics = "poder";
    }

    if (!string.IsNullOrEmpty(atributoAnalytics))
    {
      RuntimeAnalytics.TrackDesign("characters", "stat_up", atributoAnalytics);
    }

    if (CampaignManager.Instance.scTutorialManager.tutorialActivo) { CampaignManager.Instance.scTutorialManager.SiguientePaso(); }

  }

  public void SubirTiradaSalvacion(int i)
  {
    string tsAnalytics = null;
    if (i == 1)//1-Fuerza
    {
      pSel.iTSFortaleza++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
      tsAnalytics = "fortaleza";
    }
    if (i == 2)//2-Agiliadd
    {
      pSel.iTSReflejo++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
      tsAnalytics = "reflejos";
    }
    if (i == 3)//3-Poder
    {
      pSel.iTSMental++;
      pSel.NivelPuntoTS--;
      ActualizarInfo();
      tsAnalytics = "mental";
    }

    if (!string.IsNullOrEmpty(tsAnalytics))
    {
      RuntimeAnalytics.TrackDesign("characters", "save_up", tsAnalytics);
    }

  }


  public void subirNivel()
  {
    pSel.RecibirExperiencia(100);

    ActualizarInfo();

  }

  public GameObject btnPos1;
  public GameObject btnPos2;
  public GameObject btnPos3;

  public void SelPos(int pos)
  {
    // Restablecer el tamaño de todos los botones a su escala original
    btnPos1.GetComponent<Image>().rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);
    btnPos2.GetComponent<Image>().rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);
    btnPos3.GetComponent<Image>().rectTransform.localScale = new Vector3(0.4f, 0.4f, 1f);

    // Aumentar el tamaño del botón seleccionado en un 25%
    switch (pos)
    {
      case 1:
        btnPos1.GetComponent<Image>().rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
        break;
      case 2:
        btnPos2.GetComponent<Image>().rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
        break;
      case 3:
        btnPos3.GetComponent<Image>().rectTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
        break;
    }

    pSel.iPuestoDeseado = pos;

  }

  void ForzarRebuildInmediato()
  {
    Canvas.ForceUpdateCanvases();

    RectTransform root = transform as RectTransform;
    if (root != null)
      LayoutRebuilder.ForceRebuildLayoutImmediate(root);

    if (contenedorUIPersonajes != null)
    {
      RectTransform rtLista = contenedorUIPersonajes.transform as RectTransform;
      if (rtLista != null)
        LayoutRebuilder.ForceRebuildLayoutImmediate(rtLista);
    }

    if (listaHab != null)
    {
      RectTransform rtHab = listaHab as RectTransform;
      if (rtHab != null)
        LayoutRebuilder.ForceRebuildLayoutImmediate(rtHab);
    }

    Canvas.ForceUpdateCanvases();
  }

}



