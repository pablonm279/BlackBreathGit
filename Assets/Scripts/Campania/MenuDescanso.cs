using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Data;
using System;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class MenuDescanso : MonoBehaviour
{
  const int TipoNodoClaro = 3;
  const int TipoNodoAsentamiento = 4;
  const int TipoNodoRecursos = 5;
  const int TipoNodoPuestoComercial = 6;
  const int TipoNodoSantuario = 14;
  const string TooltipEmboscadaNormalId = "campania_emboscada_normal";


  public TextMeshProUGUI tareaCivilDescripcion;

  // SFX de descanso: arrastrar clip desde el Inspector
  public AudioClip sfxDescanso;
  [Range(0f, 1f)] public float sfxDescansoVolumen = 0.9f;
  private AudioSource sfxDescansoSource;

  // Maneja audio al descansar: iniciar/terminar, con control de emboscada
  void IniciarAudioDescansoSimple()
  {
    if (MusicManager.Instance != null && sfxDescanso != null)
    {
      MusicManager.Instance.PlaySFXYReanudar(sfxDescanso, sfxDescansoVolumen, 0.8f);
    }
  }

  [SerializeField] private GameObject btnRecoleccionSum;
  [SerializeField] private GameObject btnRecoleccionMat;
  [SerializeField] private GameObject btnFiesta;
  [SerializeField] private GameObject btnDiaLibre;
  [SerializeField] private GameObject btnAlerta;

  [SerializeField] private TextMeshProUGUI textExploracionChances;
  [SerializeField] private TextMeshProUGUI textEmboscadaChances;

  public int chancesAtaqueACaravana;
  private int chancesExploracion;
  private int tareaCivilSeleccionada;
  private bool descansoEnCurso;
  private bool forzarEventoBroteTutorialEnDescanso;
  private bool hoversTareasCivilesConfigurados;
  private readonly Dictionary<GameObject, Vector3> escalaOriginalBotonesTareasCiviles = new Dictionary<GameObject, Vector3>();

  float valor = 0;

  void Awake()
  {
    ConfigurarHoversTareasCiviles();
  }

  private void ConfigurarHoversTareasCiviles()
  {
    if (hoversTareasCivilesConfigurados)
    {
      return;
    }

    hoversTareasCivilesConfigurados = true;
    RegistrarHoverTareaCivil(btnRecoleccionSum, 1);
    RegistrarHoverTareaCivil(btnRecoleccionMat, 2);
    RegistrarHoverTareaCivil(btnFiesta, 3);
    RegistrarHoverTareaCivil(btnDiaLibre, 4);
    RegistrarHoverTareaCivil(btnAlerta, 5);
  }

  private void RegistrarHoverTareaCivil(GameObject boton, int tareaCivil)
  {
    if (boton == null)
    {
      return;
    }
    if (!escalaOriginalBotonesTareasCiviles.ContainsKey(boton))
    {
      escalaOriginalBotonesTareasCiviles.Add(boton, boton.transform.localScale);
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

    AgregarEventoTareaCivil(trigger, EventTriggerType.PointerEnter, () => MostrarDescripcionHoverTareaCivil(tareaCivil));
    AgregarEventoTareaCivil(trigger, EventTriggerType.PointerExit, RestaurarDescripcionTareaCivilSeleccionada);
  }

  private void AgregarEventoTareaCivil(EventTrigger trigger, EventTriggerType tipo, Action accion)
  {
    EventTrigger.Entry entrada = new EventTrigger.Entry { eventID = tipo };
    entrada.callback.AddListener(_ => accion());
    trigger.triggers.Add(entrada);
  }

  private void MostrarDescripcionHoverTareaCivil(int tareaCivil)
  {
    if (tareaCivilSeleccionada <= 0)
    {
      SeleccionarActividadCivil(1);
    }

    int tareaSeleccionadaGuardada = tareaCivilSeleccionada;
    int chancesAtaqueGuardadas = chancesAtaqueACaravana;
    int chancesExploracionGuardadas = chancesExploracion;
    float valorGuardado = valor;
    string textoEmboscadaGuardado = textEmboscadaChances != null ? textEmboscadaChances.text : null;
    string textoExploracionGuardado = textExploracionChances != null ? textExploracionChances.text : null;

    SeleccionarActividadCivil(tareaCivil);

    tareaCivilSeleccionada = tareaSeleccionadaGuardada;
    chancesAtaqueACaravana = chancesAtaqueGuardadas;
    chancesExploracion = chancesExploracionGuardadas;
    valor = valorGuardado;
    ActualizarEscalaTareaCivilSeleccionada();

    if (textEmboscadaChances != null)
    {
      textEmboscadaChances.text = textoEmboscadaGuardado;
    }

    if (textExploracionChances != null)
    {
      textExploracionChances.text = textoExploracionGuardado;
    }
  }

  private void RestaurarDescripcionTareaCivilSeleccionada()
  {
    SeleccionarActividadCivil(tareaCivilSeleccionada > 0 ? tareaCivilSeleccionada : 1);
  }

  private void ActualizarEscalaTareaCivilSeleccionada()
  {
    ActualizarEscalaBotonTareaCivil(btnRecoleccionSum, tareaCivilSeleccionada == 1);
    ActualizarEscalaBotonTareaCivil(btnRecoleccionMat, tareaCivilSeleccionada == 2);
    ActualizarEscalaBotonTareaCivil(btnFiesta, tareaCivilSeleccionada == 3);
    ActualizarEscalaBotonTareaCivil(btnDiaLibre, tareaCivilSeleccionada == 4);
    ActualizarEscalaBotonTareaCivil(btnAlerta, tareaCivilSeleccionada == 5);
  }

  private void ActualizarEscalaBotonTareaCivil(GameObject boton, bool seleccionado)
  {
    if (boton == null)
    {
      return;
    }

    if (!escalaOriginalBotonesTareasCiviles.TryGetValue(boton, out Vector3 escalaOriginal))
    {
      escalaOriginal = boton.transform.localScale;
      escalaOriginalBotonesTareasCiviles.Add(boton, escalaOriginal);
    }

    boton.transform.localScale = seleccionado ? escalaOriginal * 1.15f : escalaOriginal;
  }

  private bool FeriaBloqueadaPorLluvia()
  {
    return CampaignManager.Instance != null && CampaignManager.Instance.intTipoClima == 3;
  }

  private void ActualizarDisponibilidadFeria()
  {
    if (btnFiesta == null)
    {
      return;
    }

    bool bloqueada = FeriaBloqueadaPorLluvia();
    if (btnFiesta.transform.childCount > 0)
    {
      btnFiesta.transform.GetChild(0).gameObject.SetActive(!bloqueada);
    }

    Button boton = btnFiesta.GetComponent<Button>();
    if (boton != null)
    {
      boton.interactable = !bloqueada;
    }
  }

  public void SeleccionarActividadCivil(int n)
  {
    ActualizarDisponibilidadFeria();

    if (FeriaBloqueadaPorLluvia()) //Lluvia desactiva fiesta
    {
      if (n == 3)
      { n = 1; }
    }


    Actualizar();
    if (n == 1) //Suministros
    {
     /* btnRecoleccionSum.transform.gameObject.SetActive(false);
      btnRecoleccionMat.transform.gameObject.SetActive(true);
      btnFiesta.transform.gameObject.SetActive(true);
      btnDiaLibre.transform.gameObject.SetActive(true);
      btnAlerta.transform.gameObject.SetActive(true);*/



      tareaCivilSeleccionada = n;
      valor = (CampaignManager.Instance.GetCivilesActual() / 3) / 100 * (100 + CampaignManager.Instance.scAtributosZona.modRecoleccionSuministros);

      if (CampaignManager.Instance.intTipoClima == 3) //Lluvia
      {
        valor = valor * 0.85f; // -15% recoleccion suministors si llueve
      }
      if (CampaignManager.Instance.intTipoClima == 4) //Nieve
      {
        valor = valor * 0.85f; // -15% recoleccion suministors si neva
      }
      if (CampaignManager.Instance.intTipoClima == 5) //Niebla
      {
        valor = valor * 0.80f; // -20% recoleccion suministors si hay niebla
      }
      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 5) //Bonus recoleccion nodo recursos
      {
        valor = valor * 1.2f; // +20% recoleccion 
      }


      tareaCivilDescripcion.text = TRADU.i.Traducir("<b><u>Recolección de Suministros</b></u>\n\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("Los civiles se dedicarán a recolectar distintos suministros de las inmediaciones al campamento.\n\n");
      int suministrosMinimos = (int)CampaignManager.Instance.AplicarRecoleccionSuministrosPresagios(valor);
      int suministrosMaximos = (int)CampaignManager.Instance.AplicarRecoleccionSuministrosPresagios(valor + 10);
      tareaCivilDescripcion.text += TRADU.i.Traducir($"<color=#d8a205>Se juntarán entre ") + suministrosMinimos + TRADU.i.Traducir(" y ") + suministrosMaximos + TRADU.i.Traducir(" suministros. </color>\n\n\n");

      chancesAtaqueACaravana = 25 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
      chancesExploracion = 60 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;


    }
    else if (n == 2) //Materiales
    {
      /*btnRecoleccionSum.gameObject.SetActive(true);
      btnRecoleccionMat.gameObject.SetActive(false);
      btnFiesta.gameObject.SetActive(true);
      btnDiaLibre.gameObject.SetActive(true);
      btnAlerta.gameObject.SetActive(true);*/

      tareaCivilSeleccionada = n;
      valor = (CampaignManager.Instance.GetCivilesActual() / 5) / 100 * (100 + CampaignManager.Instance.scAtributosZona.modRecoleccionMateriales);
      if (CampaignManager.Instance.intTipoClima == 5) //Niebla
      {
        valor = valor * 0.80f; // -20% recoleccion materiales si hay niebla
      }
      if (CampaignManager.Instance.intTipoClima == 4) //Nieve
      {
        valor = valor * 0.85f; // -15% recoleccion materiales si hay Nieve
      }
      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 5) //Bonus recoleccion nodo recursos
      {
        valor = valor * 1.2f; // +20% recoleccion 
      }


      tareaCivilDescripcion.text = TRADU.i.Traducir("<b><u>Recolección de Materiales</b></u>\n\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("Los civiles se dedicarán a recolectar materiales básicos en la región.\n\n");
      int materialesMinimos = (int)CampaignManager.Instance.AplicarRecoleccionMaterialesPresagios(valor);
      int materialesMaximos = (int)CampaignManager.Instance.AplicarRecoleccionMaterialesPresagios(valor + 10);
      tareaCivilDescripcion.text += TRADU.i.Traducir("<color=#d8a205>Se juntarán entre ") + materialesMinimos + TRADU.i.Traducir(" y ") + materialesMaximos + TRADU.i.Traducir(" materiales. </color>\n\n\n");

      chancesAtaqueACaravana = 25 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
      chancesExploracion = 60 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;




    }
    else if (n == 3) //Fiesta
    {
     /* btnRecoleccionSum.transform.gameObject.SetActive(true);
      btnRecoleccionMat.transform.gameObject.SetActive(true);
      btnFiesta.transform.gameObject.SetActive(false);
      btnDiaLibre.transform.gameObject.SetActive(true);
      btnAlerta.transform.gameObject.SetActive(true);*/

      tareaCivilSeleccionada = n;

      tareaCivilDescripcion.text = TRADU.i.Traducir("<b><u>Feria</b></u>\n\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("Durante 8 h, los civiles organizarán una feria con varios juegos y celebraciones.\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("<color=#d8a205>Al finalizar las 8 h: se conseguirán entre 10 y 15 de Esperanza y se consumirán 20% más de Suministros. <color=#bb280d>+10% chances de Emboscada.</color></color>\n\n\n");

      chancesAtaqueACaravana = 30 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
      chancesExploracion = 60 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;


    }
    else if (n == 4) //Dia Libre
    {
     /* btnRecoleccionSum.transform.gameObject.SetActive(true);
      btnRecoleccionMat.transform.gameObject.SetActive(true);
      btnFiesta.transform.gameObject.SetActive(true);
      btnDiaLibre.transform.gameObject.SetActive(false);
      btnAlerta.transform.gameObject.SetActive(true);*/

      tareaCivilSeleccionada = n;

      tareaCivilDescripcion.text = TRADU.i.Traducir("<b><u>Día Libre</b></u>\n\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("Durante 10 h, los civiles descansarán para recobrar fuerzas.\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("<color=#d8a205>Al finalizar las 10 h: se conseguirán 6 de Esperanza, la Fatiga bajará a 0 y la curación de personajes tendrá +10%.</color>\n\n\n");

      chancesAtaqueACaravana = 20 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
      chancesExploracion = 50 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;



    }
    else if (n == 5) //Alerta
    {
    /*  btnRecoleccionSum.transform.gameObject.SetActive(true);
      btnRecoleccionMat.transform.gameObject.SetActive(true);
      btnFiesta.transform.gameObject.SetActive(true);
      btnDiaLibre.transform.gameObject.SetActive(true);
      btnAlerta.transform.gameObject.SetActive(false);*/

      tareaCivilSeleccionada = n;

      tareaCivilDescripcion.text = TRADU.i.Traducir("<b><u>Estado de Alerta</b></u>\n\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("Durante el descanso, se asignarán a los civiles mas aptos físicamente a la vigilancia del area circundante al campamento.\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("<color=#d8a205>Reduce chances de ataque a caravana. +20% a Exploración. -10 Esperanza.</color>\n\n\n");

      chancesAtaqueACaravana = 0 + CampaignManager.Instance.scAtributosZona.modChanceEmboscada;
      chancesExploracion = 80 + CampaignManager.Instance.scAtributosZona.modChanceExploracion;


    }

    float horasTarea = n == 5 ? 6f : n == 4 ? 10f : 8f;
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    string etiquetaDuracion = idioma == TRADU.IdiomaIngles
      ? "Duration: "
      : idioma == TRADU.IdiomaPortugues ? "Duração: " : "Duración: ";
    tareaCivilDescripcion.text += "\n<color=#B8C7D9>" + etiquetaDuracion + Mathf.RoundToInt(horasTarea) + "h</color>";

    Actualizar();
    ActualizarEscalaTareaCivilSeleccionada();

  }

  private void Actualizar()
  {
    CampaignManager campaignManager = CampaignManager.Instance;
    bool enTutorial = campaignManager != null && campaignManager.DebeUsarConfiguracionTutorial();

    if (CampaignManager.Instance.intTipoClima == 3 || CampaignManager.Instance.intTipoClima == 4 || CampaignManager.Instance.intTipoClima == 5) //Lluvia, Nieve o Niebla
    {
      chancesAtaqueACaravana -= 20;
    }
    if (CampaignManager.Instance.intTipoClima == 5) //Niebla
    {
      chancesExploracion -= 10;
    }


    chancesExploracion += MetaprogresionManager.Instance.SerriaTierAlmenaras * 3;

    
    //Aliento negro aumenta chances de ataque a caravana
    chancesAtaqueACaravana += (int)(CampaignManager.Instance.GetValorAlientoNegro() / 10f);

    //Actividades que toquen emboscada y exploracion van aca 
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || !pers.PuedeRealizarActividades())
      {
        continue;
      }

      if (pers.ActividadSeleccionada == 7) //Explorador: Caza Nocturna
      {
        chancesAtaqueACaravana += 3;

      }
      if (pers.ActividadSeleccionada == 14) //Acechador: Vigilar Desde Las Sombras
      {
        chancesAtaqueACaravana -= 5;

      }

    }



    if (CampaignManager.Instance.scMenuSequito.TieneSequito(5)) { chancesAtaqueACaravana += 2; } //Herboristas, aumentan chances 2%
    chancesAtaqueACaravana += CampaignManager.Instance.ObtenerModificadorChanceEmboscadaTraits();

    chancesExploracion += 3 + ((CampaignManager.Instance.mejoraCaravanaCatalejos - 1) * 2);

    chancesExploracion += CampaignManager.Instance.ExploracionSumadaPorActividades();
    chancesExploracion += CampaignManager.Instance.ObtenerModificadorChanceExploracionTraits();

    if (CampaignManager.Instance != null && CampaignManager.Instance.estadosCaravana != null)
    {
      chancesExploracion += CampaignManager.Instance.estadosCaravana.ObtenerModificadorExploracionPendiente();
      chancesAtaqueACaravana += CampaignManager.Instance.estadosCaravana.ObtenerModificadorEmboscadaDescansoPendiente();
    }

    float duracionDescansoHoras = tareaCivilSeleccionada == 5 ? 6f : tareaCivilSeleccionada == 4 ? 10f : 8f;
    if (campaignManager.AccionIncluyeNoche(duracionDescansoHoras))
    {
      chancesAtaqueACaravana += 5;
    }

    int modificadorDescansoExploracion = tareaCivilSeleccionada == 5 ? 20 : 0;
    chancesExploracion = CampaignManager.Instance.ObtenerChanceExploracionDescanso(modificadorDescansoExploracion);

    if (CampaignManager.Instance.intTipoClima == 6) //Almas Danzantes
    {
      chancesAtaqueACaravana = 0;
    }
    if (CampaignManager.Instance.intTipoClima == 9) //Masacre Zarkil
    {
      chancesAtaqueACaravana += 10;
    }

    if (enTutorial)
    {
      chancesAtaqueACaravana = 0;
    }
    if (CampaignManager.Instance.scMapaManager.nodoActual != null &&
        CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoAsentamiento)
    {
      chancesAtaqueACaravana = 0;
    }

    chancesAtaqueACaravana = Mathf.Clamp(chancesAtaqueACaravana, 0, 100);

    chancesExploracion = Mathf.Clamp(chancesExploracion, 0, 100);

    textEmboscadaChances.text = TRADU.i.Traducir("Las probabilidades de sufrir un ataque a la Caravana ") + chancesAtaqueACaravana + "%";
    textExploracionChances.text = TRADU.i.Traducir("Las probabilidades de exploración: ") + chancesExploracion + "%";

  }

  public void Descansar()
  {
    if (CampaignManager.Instance != null &&
        CampaignManager.Instance.scMapaManager != null &&
        CampaignManager.Instance.scMapaManager.nodoActual != null &&
        CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoAsentamiento)
    {
      CampaignManager.Instance.EscribirAdvertenciaLog(TRADU.i.Traducir("<color=#FF6666>El descanso normal no está disponible dentro de un Asentamiento.</color>"));
      gameObject.SetActive(false);
      return;
    }
    forzarEventoBroteTutorialEnDescanso = DebeForzarEventoDescansoTutorial();
    TutorialEvents.Emit("ui.descanso_confirmado", gameObject);
    EjecutarDescansoSeguro();
  }

  private async void EjecutarDescansoSeguro()
  {
    if (descansoEnCurso)
    {
      return;
    }

    descansoEnCurso = true;
    try
    {
      await DescansarAsync();
    }
    catch (Exception ex)
    {
      Debug.LogException(ex, this);
    }
    finally
    {
      descansoEnCurso = false;
    }
  }

  public async Task CompletarResultadosDescansoTrasCarga(int tareaCivil)
  {
    tareaCivilSeleccionada = tareaCivil;
    CampaignManager.Instance.ObtenerSnapshotResultadosDescanso(
      out _,
      out valor,
      out chancesExploracion,
      out chancesAtaqueACaravana);
    await DescansarAsync(true);
  }

  private void IntentarEncontrarAtajoSuperficieTrasDescanso()
  {
    Nodo nodoActual = CampaignManager.Instance != null &&
                      CampaignManager.Instance.scMapaManager != null
      ? CampaignManager.Instance.scMapaManager.nodoActual
      : null;

    if (nodoActual == null)
    {
      return;
    }

    int chanceAtajo = Mathf.Clamp(Mathf.FloorToInt(chancesExploracion * 0.5f), 0, 100);
    chanceAtajo = CampaignManager.Instance.AjustarChanceAtajoSuperficiePresagios(chanceAtajo);
    if (UnityEngine.Random.Range(0, 100) >= chanceAtajo)
    {
      return;
    }

    if (nodoActual.IntentarEncontrarAtajoSuperficie())
    {
      CampaignManager.Instance.EscribirLog(ObtenerTextoLogAtajoSuperficieEncontrado());
    }
  }

  private string ObtenerTextoLogAtajoSuperficieEncontrado()
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
      case TRADU.IdiomaIngles:
        return "-During the rest, the scouts found a hidden route.";
      case TRADU.IdiomaPortugues:
        return "-Durante o descanso, os exploradores encontraram uma rota oculta.";
      default:
        return "-Durante el descanso los exploradores han encontrado una ruta oculta.";
    }
  }

  private bool DebeForzarEventoDescansoTutorial()
  {
    TutorialDirector director = TutorialDirector.Instance;
    TutorialStep pasoActual = director != null ? director.CurrentStep : null;
    if (pasoActual != null)
    {
      if (pasoActual.id == "Descanso1")
      {
        return true;
      }

      if (pasoActual.advanceConditions != null)
      {
        for (int i = 0; i < pasoActual.advanceConditions.Count; i++)
        {
          TutorialCondition condition = pasoActual.advanceConditions[i];
          if (condition != null && condition.eventId == TutorialEventNames.CampaignRestRandomEventContinued)
          {
            return true;
          }
        }
      }
    }

    TutorialManager tutorialLegacy = CampaignManager.Instance != null ? CampaignManager.Instance.scTutorialManager : null;
    return tutorialLegacy != null
      && tutorialLegacy.tutorialActivo
      && (tutorialLegacy.pasoActual == 24 || tutorialLegacy.pasoActual == 25);
  }

  private async Task DescansarAsync(bool omitirTranscurso = false)
  {
    CampaignManager campaignManager = CampaignManager.Instance;
    bool enTutorial = campaignManager != null && campaignManager.DebeUsarConfiguracionTutorial();
    bool autosavePendienteTrasDescanso = true;
    bool forzarEventoBroteTutorial = forzarEventoBroteTutorialEnDescanso;
    forzarEventoBroteTutorialEnDescanso = false;
    int climaInicioDescanso;
    if (omitirTranscurso)
    {
      campaignManager.ObtenerSnapshotResultadosDescanso(
        out climaInicioDescanso,
        out _,
        out _,
        out _);
    }
    else
    {
      climaInicioDescanso = campaignManager.intTipoClima;
    }
    float duracionDescansoHoras = tareaCivilSeleccionada == 5 ? 6f : tareaCivilSeleccionada == 4 ? 10f : 8f;
    Nodo nodoDescanso = campaignManager != null && campaignManager.scMapaManager != null
      ? campaignManager.scMapaManager.nodoActual
      : null;
    bool esClaro = nodoDescanso != null && nodoDescanso.tipoNodo == TipoNodoClaro;
    float multiplicadorCuracion = 1f;
    if (esClaro) multiplicadorCuracion *= 1.1f;
    if (nodoDescanso != null && nodoDescanso.tipoNodo == TipoNodoRecursos) multiplicadorCuracion *= 1.2f;
    if (tareaCivilSeleccionada == 4) multiplicadorCuracion *= 1.1f;
    Personaje purificadoraRitualDescanso = omitirTranscurso
      ? campaignManager.ObtenerPurificadoraRitualDescansoPendiente()
      : campaignManager.scMenuPersonajes.listaPersonajes.Find(personaje =>
        personaje != null
        && personaje.PuedeRealizarActividades()
        && personaje.ActividadSeleccionada == 10
        && campaignManager.EsActividadPermitidaPorClimaCampania(10));

    bool emboscadaProgramada = omitirTranscurso
      && campaignManager.DescansoTuvoEmboscadaPendienteResultados();
    if (!omitirTranscurso)
    {
    float multiplicadorAliento = esClaro ? 0.5f : 1f;
    int tiradaEmboscadaDescanso = UnityEngine.Random.Range(1, 101);
    emboscadaProgramada = !enTutorial
      && !toggleMenuMisiones.isOn
      && nodoDescanso != null
      && nodoDescanso.tipoNodo != TipoNodoAsentamiento
      && tiradaEmboscadaDescanso <= chancesAtaqueACaravana;
    float horasHastaEmboscada = emboscadaProgramada
      ? UnityEngine.Random.Range(0f, duracionDescansoHoras)
      : duracionDescansoHoras;

    campaignManager.GuardarContinuacionDescanso(
      duracionDescansoHoras,
      tareaCivilSeleccionada,
      esClaro,
      campaignManager.ObtenerHoraActual(),
      purificadoraRitualDescanso,
      climaInicioDescanso,
      valor,
      chancesExploracion,
      chancesAtaqueACaravana,
      emboscadaProgramada,
      horasHastaEmboscada,
      tiradaEmboscadaDescanso);

    // Audio: al presionar Descansar, cortar másica con fade, reproducir SFX y reanudar.
    IniciarAudioDescansoSimple();
    if (CampaignManager.Instance.logDeCampania != null)
    {
      CampaignManager.Instance.logDeCampania.RegistrarDescanso();
    }

    CampaignManager.Instance.scSequitoMercaderes.GenerarItemsVendidos();

    gameObject.SetActive(false);

    BanterCampaignDirector.NotificarDescansoIniciado();
    CampaignManager.Instance.scAdministradorEscenas.PlayFadeInOut(1.2f, Mathf.Max(4f, duracionDescansoHoras - 1.2f));
    await TranscurrirHorasDescanso(horasHastaEmboscada, multiplicadorAliento, multiplicadorCuracion, true);
    if (emboscadaProgramada)
    {
      float horasRestantesTrasEmboscada = Mathf.Max(0f, duracionDescansoHoras - horasHastaEmboscada);
      campaignManager.MarcarEmboscadaDescansoConsumida(campaignManager.ObtenerHoraActual());
      campaignManager.CapturarHoraCombatePendiente(campaignManager.ObtenerHoraActual());
      campaignManager.BATALLA_EnCurso = 11;
      campaignManager.EMBOSCADA_EnCurso = 3;
      campaignManager.EscribirLog(TRADU.i.Traducir("-La caravana ha sufrido un Ataque durante el descanso. Probabilidades ") + chancesAtaqueACaravana + TRADU.i.Traducir("% - Tirada: 1d100 = ") + tiradaEmboscadaDescanso);
      campaignManager.TryAutosaveCampania("descanso_interrumpido", out _);
      TutorialTooltipManager.TryShow(TooltipEmboscadaNormalId);
      campaignManager.scMenuBatallas.EventoBatallaCaravana(0, 3);

      while (campaignManager.BATALLA_EnCurso > 0)
      {
        await Task.Yield();
      }

      await TranscurrirHorasDescanso(
        horasRestantesTrasEmboscada,
        multiplicadorAliento,
        multiplicadorCuracion,
        true);
    }
    campaignManager.FinalizarContinuacionDescanso();
    }
    IntentarEncontrarAtajoSuperficieTrasDescanso();

    if (tareaCivilSeleccionada == 1)
    {
      int random = UnityEngine.Random.Range(0, 21);
      float total = CampaignManager.Instance.AplicarRecoleccionSuministrosPresagios(valor + random);

      CampaignManager.Instance.CambiarSuministrosActuales((int)total);

    } //Recoleccion Suministros

    if (tareaCivilSeleccionada == 2)
    {
      int random = UnityEngine.Random.Range(0, 11);
      float total = CampaignManager.Instance.AplicarRecoleccionMaterialesPresagios(valor + random);
      CampaignManager.Instance.CambiarMaterialesActuales((int)total);

    } //Recoleccion Materiales

    float consumo = CampaignManager.Instance.GetCivilesActual() + CampaignManager.Instance.GetBueyesActual() * 2;
    if (tareaCivilSeleccionada == 3)
    {
      consumo = consumo * 1.2f;
      int random = UnityEngine.Random.Range(0, 6);
      CampaignManager.Instance.CambiarEsperanzaActual(10 + random);

    } //Fiesta

    consumo = consumo / 100 * (100 - CampaignManager.Instance.mejoraCaravanaAlmacen * 3);

    //Modificadores de Climas
    if (climaInicioDescanso == 2) //Calor capturado al comenzar el descanso
    {
      if (tareaCivilSeleccionada == 4) // y se descansa, da +5 esperanza
      {
        CampaignManager.Instance.CambiarEsperanzaActual(5);
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El tener un Día Libre en plena Ola de Calor, ha caído bien en los Civiles. +5 Esperanza"));
      }
      else
      {
        CampaignManager.Instance.CambiarEsperanzaActual(-3); //si se hace otra cosa, -3
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El tener que trabajar en plena Ola de Calor, ha caído mal en los Civiles. -3 Esperanza"));
      }

    }


    //Actividades Personajes Al Descansar
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      //Saca Fatiga de campaña
      pers.SetCampFatigado(false);

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitDulcesSuenos))
      {
        pers.AplicarMoralAltaHoras(48f);
        int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
        string mensajeDulcesSuenos = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " sleeps peacefully. Gains High Morale for 48h.",
          TRADU.IdiomaPortugues => pers.sNombre + " dorme em paz. Recebe Moral Alta por 48h.",
          _ => pers.sNombre + " duerme plácidamente. Obtiene Moral Alta por 48h."
        };
        CampaignManager.Instance.EscribirLog("-" + mensajeDulcesSuenos);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitPesadillasRecurrentes))
      {
        pers.AplicarMoralBajaHoras(24f);
        int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
        string mensajePesadillas = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " suffers recurring nightmares. Gains Low Morale for 1 day.",
          TRADU.IdiomaPortugues => pers.sNombre + " sofre pesadelos recorrentes. Recebe Moral Baixa por 1 dia.",
          _ => pers.sNombre + " sufre pesadillas recurrentes. Obtiene Baja Moral por 1 día."
        };
        CampaignManager.Instance.EscribirLog("-" + mensajePesadillas);
      }

    }

    //Efectos de Sequitos al descansar
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(4) && tareaCivilSeleccionada == 3) //Artistas
    {
      CampaignManager.Instance.CambiarEsperanzaActual(5);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-En la Feria, los Artistas han realizado un espectáculo que ha levantado el ánimo de los Civiles. +5 Esperanza"));
    }
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(5)) //Herboristas
    {
      CampaignManager.Instance.scSequitoHerboristas.cantBalsamoFort = 2;
      CampaignManager.Instance.scSequitoHerboristas.cantBalsamoReflej = 2;
      CampaignManager.Instance.scSequitoHerboristas.cantBalsamoMental = 2;
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Herboristas han preparado sus Bálsamos."));
    }
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(11)) //Esclavos
    {
      int random = UnityEngine.Random.Range(10, 16);

      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Esclavos han recolectado ") + random + TRADU.i.Traducir(" Materiales."));
      CampaignManager.Instance.CambiarMaterialesActuales(random);
    }

    if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoPuestoComercial)
    {
      int oroGenerado = Mathf.RoundToInt(CampaignManager.Instance.GetCivilesActual()) * 2;
      if (oroGenerado > 0)
      {
        CampaignManager.Instance.CambiarOroActual(oroGenerado);

        string mensajeComercio = TRADU.i.nIdioma switch
        {
          TRADU.IdiomaIngles => "-The civilians traded goods at the Trading Post and generated " + oroGenerado + " Gold, which they donated to the Caravan.",
          TRADU.IdiomaPortugues => "-Os civis trocaram mercadorias no Posto Comercial e geraram " + oroGenerado + " de Ouro, que doaram para a Caravana.",
          _ => "-Los civiles intercambiaron bienes en el Puesto Comercial y han generado " + oroGenerado + " Oro, que han donado a la Caravana."
        };

        CampaignManager.Instance.EscribirLog(mensajeComercio);
      }
    }

    if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoSantuario)
    {
      CampaignManager.Instance.BendecirPersonajesSantuario(96f);

      string mensajeSantuario = TRADU.i.nIdioma switch
      {
        TRADU.IdiomaIngles => "-Resting in the Sanctuary blesses all characters for 96 h.",
        TRADU.IdiomaPortugues => "-Descansar no Santuário abençoa todos os personagens por 96 h.",
        _ => "-Descansar en el Santuario bendice a todos los personajes por 96 h."
      };

      CampaignManager.Instance.EscribirLog(mensajeSantuario);
    }






    CampaignManager.Instance.BosqueArdienteMecanicaIncendio(40);
    CampaignManager.Instance.PasoVientoHeladoMecanicaRituales(30);

    int fatiga = CampaignManager.Instance.GetFatigaActual();
    if (tareaCivilSeleccionada == 4)
    { fatiga++; CampaignManager.Instance.CambiarEsperanzaActual(6); } //Día libre

    if (tareaCivilSeleccionada == 5)
    { CampaignManager.Instance.CambiarEsperanzaActual(-10); } //Alerta



    if (CampaignManager.Instance.scMenuSequito.TieneSequito(8)) { consumo -= 18; } //Refugiados, consumen menos suministros(son 35 civiles, pero consumen la mitad)

    CampaignManager.Instance.CambiarFatigaActual(-fatiga);
    int cantSum = CampaignManager.Instance.GetSuministrosActuales();
    if (cantSum < consumo)
    {
      CampaignManager.Instance.CambiarSuministrosActuales(-(int)cantSum);

      float faltaSum = consumo - cantSum;
      CampaignManager.Instance.CambiarEsperanzaActual(-(int)faltaSum);

      float mueren = faltaSum / 20;
      CampaignManager.Instance.CambiarCivilesActuales(-(int)mueren);

      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La falta de Suministros ha provocado la muerte de ") + mueren + TRADU.i.Traducir(" Civiles."));


    }
    else
    {
      CampaignManager.Instance.CambiarSuministrosActuales(-(int)consumo);
    }

    CampaignManager.Instance.AplicarPresagiosDescanso();
    gameObject.SetActive(false);

    int alcanceExploracion = Mathf.Max(1, CampaignManager.Instance.ObtenerDistanciaVisionEfectiva());
    CampaignManager.Instance.scMapaManager.nodoActual.TiradaExploracion(chancesExploracion, true, "", false, alcanceExploracion);

    //Efectos Esperanza en Descanso - Se van Civiles
    if (CampaignManager.Instance.GetEsperanzaActual() < 20 && CampaignManager.Instance.GetEsperanzaActual() > 10)
    {
      int random = UnityEngine.Random.Range(1, 5);
      CampaignManager.Instance.CambiarCivilesActuales(-random);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Por la baja Esperanza ") + random + TRADU.i.Traducir(" Civiles han abandonado la Caravana."));

    }
    else if (CampaignManager.Instance.GetEsperanzaActual() <= 10)
    {
      int random = UnityEngine.Random.Range(1, 11);
      CampaignManager.Instance.CambiarCivilesActuales(-random);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Por la muy baja Esperanza ") + random + TRADU.i.Traducir(" Civiles han abandonado la Caravana."));
    }

    //Efectos Esperanza en Descanso 
    if (CampaignManager.Instance.GetEsperanzaActual() > 79 && CampaignManager.Instance.GetEsperanzaActual() < 90)
    {
      int random = Mathf.RoundToInt(UnityEngine.Random.Range(1, 21) + CampaignManager.Instance.GetCivilesActual() / 3f);
      CampaignManager.Instance.CambiarOroActual(random);

      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Debido al optimismo que rodea la Caravana, los Civiles han donado Oro: ") + random);
    }
    else if (CampaignManager.Instance.GetEsperanzaActual() >= 90)
    {
      int random = Mathf.RoundToInt(UnityEngine.Random.Range(1, 21) + CampaignManager.Instance.GetCivilesActual() / 2f);
      CampaignManager.Instance.CambiarOroActual(random);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Debido al gran optimismo que rodea la Caravana, los Civiles han donado Oro: ") + random);

    }

    if (purificadoraRitualDescanso != null)
    {
      CampaignManager.Instance.CambiarValorAlientoNegroHoras(-5f);
      string mensajeRitualDescanso = TRADU.i.nIdioma switch
      {
        TRADU.IdiomaIngles => " keeps the protective circles burning while the Caravan sleeps. As the rest ends, the Black Breath recedes by 5 h.",
        TRADU.IdiomaPortugues => " mantém os círculos de proteção acesos enquanto a Caravana dorme. Ao fim do descanso, o Sopro Negro recua 5 h.",
        _ => " mantiene encendidos los círculos de protección mientras la Caravana duerme. Al concluir el descanso, el Aliento Negro retrocede 5 h."
      };
      CampaignManager.Instance.EscribirLog("-" + purificadoraRitualDescanso.sNombre + mensajeRitualDescanso);
    }

    CampaignManager.Instance.FinalizarAccionTemporal();
    CampaignManager.Instance.AplicarTraitsMoraleAmbientales();

    #region Acechadores Sueldo
    //Sueldo Acechadores
    int cantidadAcechadores = CampaignManager.Instance.CuantosPersonajesSonDeTalClase(4); //Acechadores
    if (cantidadAcechadores > 0 && CampaignManager.Instance.GetEsperanzaActual() < 70) //Si la esperanza es menor a 70, los Acechadores cobran su sueldo.
    {
      int sueldoAcechadores = cantidadAcechadores * 20;
      CampaignManager.Instance.CambiarOroActual(-sueldoAcechadores);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Acechadores en la Caravana se han cobrado su sueldo por Oro: ") + sueldoAcechadores);
    }
    else if (cantidadAcechadores > 0) //Si la esperanza es mayor o igual a 70, no cobran.
    {
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Debido a la alta Esperanza, los Acechadores han decidido no cobrar su sueldo esta vez."));

    }
    #endregion

    float randomEvento = UnityEngine.Random.Range(0, 100);
    float factorEventoBuenoMalo = 36 + CampaignManager.Instance.GetEsperanzaActual() / 5 + CampaignManager.Instance.ObtenerModificadorChanceEventoTraits();
    factorEventoBuenoMalo = CampaignManager.Instance.AjustarChanceEventoBuenoPresagios(factorEventoBuenoMalo);
    bool descansoEnNodoEvento = CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 2;
    bool descansoEnAsentamiento = CampaignManager.Instance.scMapaManager.nodoActual != null &&
                                  CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoAsentamiento;

    CampaignManager.Instance.CambiarEsperanzaActual(Mathf.Max(0, (CampaignManager.Instance.mejoraCaravanaTiendas - 1) * 2));



    if (forzarEventoBroteTutorial)
    {
      CampaignManager.Instance.EmpezarEvento(IdsEventoCampania.BroteEntreLasBrasas);
      autosavePendienteTrasDescanso = false;
    }
    else if (!toggleMenuMisiones.isOn) //Si no pidió rescate, todo normal
    {
      //Probabilidad emboscada
      // (Audio) Se maneja al principio de Descansar()

      if (emboscadaProgramada)
      {
        // La emboscada ya se resolvió en la hora sorteada y el descanso continuó al regresar.
        // (Audio) Ignorado: la másica de batalla se maneja en AdministradorEscenas
      }
      else //no puede haber evento y emboscada
      {
        if (!enTutorial && !descansoEnNodoEvento)
        {
          bool eventoBueno = randomEvento < factorEventoBuenoMalo
            || CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoClaro;

          bool eventoIniciado = eventoBueno
            ? CampaignManager.Instance.EmpezarEventoBueno(TipoOrigenEventoCampania.Descanso)
            : CampaignManager.Instance.EmpezarEventoMalo(TipoOrigenEventoCampania.Descanso);

          if (eventoIniciado)
          {
            autosavePendienteTrasDescanso = false;
          }
        }

        // (Audio) Ya se reanudó desde MusicManager tras terminar el SFX

      }
    }
    else
    {
      if (!enTutorial)
      {
        CampaignManager.Instance.EmpezarEvento(IdsEventoCampania.MisionSalvamento); //Evento de Mision de Rescate
        autosavePendienteTrasDescanso = false;
        MetaprogresionManager.Instance.MisionesSalvamento--;
      }
    }

    if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 26)
    { CampaignManager.Instance.scTutorialManager.SiguientePaso(); }
    goMenuMisiones.SetActive(false);
    CampaignManager.Instance.RefrescarBarraPersonajesCampania(true);

    if (autosavePendienteTrasDescanso &&
        CampaignManager.Instance.scMapaManager != null &&
        CampaignManager.Instance.scMapaManager.nodoActual != null &&
        CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoPuestoComercial)
    {
      CampaignManager.Instance.AbrirPuestoComercial(false);
    }

    if (autosavePendienteTrasDescanso)
    {
      CampaignManager.Instance.MarcarResultadosDescansoCompletados();
      CampaignManager.Instance.TryAutosaveCampania("descanso", out _);
    }
    else
    {
      CampaignManager.Instance.MarcarResultadosDescansoCompletados();
    }
  }

  private async Task TranscurrirHorasDescanso(
    float horas,
    float multiplicadorAliento,
    float multiplicadorCuracion,
    bool actualizarContinuacionPendiente)
  {
    float restantes = Mathf.Max(0f, horas);
    while (restantes > 0.0001f)
    {
      float delta = Mathf.Min(restantes, Mathf.Max(0f, Time.deltaTime));
      if (delta > 0f)
      {
        CampaignManager.Instance.AvanzarTiempoCampania(
          delta,
          TipoAvanceTiempoCampania.Descanso,
          multiplicadorAliento,
          multiplicadorCuracion,
          true);
        restantes -= delta;
        if (actualizarContinuacionPendiente)
        {
          CampaignManager.Instance.AvanzarProgresoDescansoPendiente(delta);
        }
      }
      await Task.Yield();
    }
  }

  public GameObject climaNieve;
  public GameObject climaLluvia;
  public GameObject climaNiebla;
  public GameObject climaAlmasDanzantes;
  public GameObject climaAuroraBoreal;
  public GameObject climaMasacre;

  void ResetearVisualesClima()
  {
    climaNieve.SetActive(false);
    climaLluvia.SetActive(false);
    climaNiebla.SetActive(false);
    climaAuroraBoreal.SetActive(false);
    climaMasacre.SetActive(false);
    climaAlmasDanzantes.SetActive(false);
  }

  public void SincronizarVisualesClimaDesdeEstadoActual()
  {
    ResetearVisualesClima();

    if (CampaignManager.Instance == null)
    {
      return;
    }

    switch (CampaignManager.Instance.intTipoClima)
    {
      case 3:
        if (climaLluvia != null) climaLluvia.SetActive(true);
        break;
      case 4:
        if (climaNieve != null) climaNieve.SetActive(true);
        break;
      case 5:
        if (climaNiebla != null) climaNiebla.SetActive(true);
        break;
      case 6:
        if (climaAlmasDanzantes != null) climaAlmasDanzantes.SetActive(true);
        break;
      case 7:
        if (climaAuroraBoreal != null) climaAuroraBoreal.SetActive(true);
        break;
      case 9:
        if (climaMasacre != null) climaMasacre.SetActive(true);
        break;
    }
  }

  bool IntentarAplicarMasacreNedukazalDebug()
  {
    if (CampaignManager.Instance == null
      || !CampaignManager.Instance.EstaActivoDebugForzarMasacreNedukazal()
      || CampaignManager.Instance.scAtributosZona == null
      || CampaignManager.Instance.scAtributosZona.ID != 3)
    {
      return false;
    }

    ResetearVisualesClima();
    if (climaMasacre != null)
    {
      climaMasacre.SetActive(true);
    }

    CampaignManager.Instance.AplicarClimaMasacreNedukazalForzada();
    CampaignManager.Instance.CambiarEsperanzaActual(-10);
    return true;
  }

  bool IntentarAplicarAuroraPasoVientoHeladoDebug()
  {
    if (CampaignManager.Instance == null)
    {
      return false;
    }

    return CampaignManager.Instance.IntentarAplicarClimaAuroraPasoVientoHeladoDebug(false, true);
  }

  bool DebeForzarAlmasDanzantesTutorial()
  {
    if (CampaignManager.Instance == null || !CampaignManager.Instance.DebeUsarConfiguracionTutorial())
    {
      return false;
    }

    bool tutorialLegacyActivo = CampaignManager.Instance.scTutorialManager != null
      && CampaignManager.Instance.scTutorialManager.tutorialActivo;
    int pasoTutorial = tutorialLegacyActivo ? CampaignManager.Instance.scTutorialManager.pasoActual : 5;
    return pasoTutorial > 4;
  }

  void AplicarClimaAlmasDanzantesTutorial()
  {
    ResetearVisualesClima();
    if (climaAlmasDanzantes != null)
    {
      climaAlmasDanzantes.SetActive(true);
    }

    CampaignManager.Instance.intTipoClima = 6;
    CampaignManager.Instance.RegistrarClimaExclusivoDescubierto(CampaignManager.Instance.intTipoClima);
    if (CampaignManager.Instance.widgetClima != null)
    {
      CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_almasDanzantes;
    }

    CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Las Almas Danzantes de animales inocentes guían a la caravana. Mientras permanezcan, no habrá emboscadas."));
    CampaignManager.Instance.RefrescarVfxClimaCalor();
    if (CampaignManager.Instance.scMapaManager != null)
    {
      CampaignManager.Instance.scMapaManager.RefrescarVisibilidadExploracion();
    }
  }

  public void TiradaClima()
  {
    if (IntentarAplicarAuroraPasoVientoHeladoDebug())
    {
      return;
    }

    if (IntentarAplicarMasacreNedukazalDebug())
    {
      return;
    }

    int random = UnityEngine.Random.Range(0, 100);
    if (CampaignManager.Instance.DebeUsarConfiguracionTutorial())
    {
      if (DebeForzarAlmasDanzantesTutorial())
      {
        AplicarClimaAlmasDanzantesTutorial();
        return;
      }
      else
      { random = 1; } //Siempre sol en los primeros pasos del tutorial
    }

    bool bloquearCalorPorInicioDeZona = CampaignManager.Instance != null
      && CampaignManager.Instance.ConsumirBloqueoOlaDeCalorEnSiguienteTiradaClima();
    if (bloquearCalorPorInicioDeZona)
    {
      int inicioRangoCalor = CampaignManager.Instance.scAtributosZona.Clima_chances_Sol;
      int finRangoCalor = CampaignManager.Instance.scAtributosZona.Clima_chances_Calor;
      int intentos = 0;
      while (random >= inicioRangoCalor && random < finRangoCalor && intentos < 32)
      {
        random = UnityEngine.Random.Range(0, 100);
        intentos++;
      }

      if (random >= inicioRangoCalor && random < finRangoCalor)
      {
        random = Mathf.Clamp(finRangoCalor, 1, 100);
      }
    }
   
    ResetearVisualesClima();





    if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_Sol)
    {
      CampaignManager.Instance.intTipoClima = 1;
      CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_sol;

      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Es un día hermoso. +5 Esperanza."));
      CampaignManager.Instance.CambiarEsperanzaActual(5);

    }
    else if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_Calor)
    {
      CampaignManager.Instance.intTipoClima = 2;
      CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_calor;

      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La Ola de Calor se hace insoportable. +1 Fatiga."));
      CampaignManager.Instance.CambiarFatigaActual(+1);
    }
    else if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_Lluvia)
    {
      climaLluvia.SetActive(true);
      CampaignManager.Instance.intTipoClima = 3;
      CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_lluvia;

      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La Lluvia hace el viaje más difícil. -5 Esperanza."));
      CampaignManager.Instance.CambiarEsperanzaActual(-5);

      CampaignManager.Instance.DesactivarIncendiosPorLluvia(true);

    }
    else if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_Nieve)
    {
      climaNieve.SetActive(true);
      CampaignManager.Instance.intTipoClima = 4;
      CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_nieve;
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La Nieve cubre el camino y limita las actividades de la caravana."));
      CampaignManager.Instance.NormalizarActividadesPorClimaCampania();
    }
    else if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_Niebla)
    {
      climaNiebla.SetActive(true);
      CampaignManager.Instance.intTipoClima = 5;
      CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_niebla;
    }
    else if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_EspecialZona1)
    {
      if (CampaignManager.Instance.scAtributosZona.ID == 1) //Bosque Ardiente - Almas Danzantes
      {
        climaAlmasDanzantes.SetActive(true);

        CampaignManager.Instance.intTipoClima = 6;
        CampaignManager.Instance.RegistrarClimaExclusivoDescubierto(CampaignManager.Instance.intTipoClima);
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_almasDanzantes;


        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Las Almas Danzantes de animales inocentes guían a la caravana. Mientras permanezcan, no habrá emboscadas."));
      }
      if (CampaignManager.Instance.scAtributosZona.ID == 2) //Paso Helado - Aurora Boreal
      {
        climaAuroraBoreal.SetActive(true);

        CampaignManager.Instance.intTipoClima = 7;
        CampaignManager.Instance.RegistrarClimaExclusivoDescubierto(CampaignManager.Instance.intTipoClima);
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_auroraboreal;


        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La Aurora Boreal maravilla a toda la caravana. +10 Esperanza"));
        CampaignManager.Instance.CambiarEsperanzaActual(10);
      }

      if (CampaignManager.Instance.scAtributosZona.ID == 3) //Nedukazal - Oscuridad
      {
        CampaignManager.Instance.intTipoClima = 8;
        CampaignManager.Instance.RegistrarClimaExclusivoDescubierto(CampaignManager.Instance.intTipoClima);
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_NedukazalNormal;

      }


    }
    else if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_EspecialZona2)
    {
      if (CampaignManager.Instance.scAtributosZona.ID == 3) //Nedukazal - Masacre
      {
        climaMasacre.SetActive(true);
        CampaignManager.Instance.intTipoClima = 9;
        CampaignManager.Instance.RegistrarClimaExclusivoDescubierto(CampaignManager.Instance.intTipoClima);
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_NedukazalMasacre;
        CampaignManager.Instance.CambiarEsperanzaActual(-10);
      }


    }




    if (CampaignManager.Instance != null)
    {
      CampaignManager.Instance.RefrescarVfxClimaCalor();
      if (CampaignManager.Instance.scMapaManager != null)
      {
        CampaignManager.Instance.scMapaManager.RefrescarVisibilidadExploracion();
      }
    }

  }



  public TextMeshProUGUI txtCantidadMisionesdisp;
  public TextMeshProUGUI txtResMision;
 

  public GameObject goMenuMisiones;

  public GameObject descIngles;
  public GameObject descEsp;
  
  public Toggle toggleMenuMisiones;
  public void AbrirMenuMisiones()
  {
    goMenuMisiones.SetActive(!goMenuMisiones.activeInHierarchy);
    int misionesDisponibles = MetaprogresionManager.Instance.MisionesSalvamento;
    txtCantidadMisionesdisp.text = TRADU.i.Traducir("Misiones Disponibles: ") + misionesDisponibles;
    toggleMenuMisiones.isOn = false;

    if (MetaprogresionManager.Instance.MisionesSalvamento < 1) { toggleMenuMisiones.interactable = false; }
    else { toggleMenuMisiones.interactable = true; }

    if (TRADU.i.nIdioma == 1) //Español
    {
      descIngles.SetActive(false);
      descEsp.SetActive(true);
      int suministros = 30;
      suministros += MetaprogresionManager.Instance.SerriaTierGranjas * 15;

      txtResMision.text = $"+25 Esperanza\n+{suministros} Suministros\n+20 Materiales\n+200 Oro\n+1 Explorador o Acechador";
    }
    else if (TRADU.i.nIdioma == 2)
    {
      descIngles.SetActive(true);
      descEsp.SetActive(false);
      int suministros = 30;
      suministros += MetaprogresionManager.Instance.SerriaTierGranjas * 15;
      txtResMision.text = $"+25 Hope\n+{suministros} Supplies\n+20 Materials\n+200 Gold\n+1 Explorer or Stalker";

    }




  }



}
