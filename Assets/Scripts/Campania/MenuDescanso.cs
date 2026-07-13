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
    SincronizarVisualesClimaDesdeEstadoActual();
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
      tareaCivilDescripcion.text += TRADU.i.Traducir($"<color=#d8a205>Se juntarán entre ") + (int)valor + TRADU.i.Traducir(" y ") + ((int)valor + 10) + TRADU.i.Traducir(" suministros. </color>\n\n\n");

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
      tareaCivilDescripcion.text += TRADU.i.Traducir("Los civiles se dedicarán a recolectar materiales básicos en la zona.\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("<color=#d8a205>Se juntarán entre ") + (int)valor + TRADU.i.Traducir(" y ") + ((int)valor + 10) + TRADU.i.Traducir(" materiales. </color>\n\n\n");

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
      tareaCivilDescripcion.text += TRADU.i.Traducir("Los civiles dedicarán el día a organizar una feria con varios juegos y celebraciones.\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("<color=#d8a205>Se conseguirá entre 15 y 25 de Esperanza y se consumirán 20% más de Suministros. <color=#bb280d>+10% chances de Emboscada.</color></color>\n\n\n");

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
      tareaCivilDescripcion.text += TRADU.i.Traducir("Los civiles se tomarán el día para descansar y recobrar fuerzas.\n\n");
      tareaCivilDescripcion.text += TRADU.i.Traducir("<color=#d8a205>Se conseguirá 10 de Esperanza y el día siguiente arrancará con -1 Fatiga. +10% Curación a personajes.</color>\n\n\n");

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
      chancesExploracion -= 20;
    }


    chancesExploracion += MetaprogresionManager.Instance.SerriaTierAlmenaras * 3;

    
    //Aliento negro aumenta chances de ataque a caravana
    chancesAtaqueACaravana += (int)(CampaignManager.Instance.GetValorAlientoNegro() / 2);

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

    chancesAtaqueACaravana -= CampaignManager.Instance.mejoraCaravanaAntorchas * 2;
    chancesExploracion += 3 + ((CampaignManager.Instance.mejoraCaravanaCatalejos - 1) * 2);

    chancesExploracion += CampaignManager.Instance.ExploracionSumadaPorActividades();
    chancesExploracion += CampaignManager.Instance.ObtenerModificadorChanceExploracionTraits();

    if (CampaignManager.Instance != null && CampaignManager.Instance.estadosCaravana != null)
    {
      chancesExploracion += CampaignManager.Instance.estadosCaravana.ObtenerModificadorExploracionPendiente();
      chancesAtaqueACaravana += CampaignManager.Instance.estadosCaravana.ObtenerModificadorEmboscadaDescansoPendiente();
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

  private async Task DescansarAsync()
  {
    CampaignManager campaignManager = CampaignManager.Instance;
    bool enTutorial = campaignManager != null && campaignManager.DebeUsarConfiguracionTutorial();
    bool autosavePendienteTrasDescanso = true;
    bool forzarEventoBroteTutorial = forzarEventoBroteTutorialEnDescanso;
    forzarEventoBroteTutorialEnDescanso = false;

    // Audio: al presionar Descansar, cortar másica con fade, reproducir SFX y reanudar.
    IniciarAudioDescansoSimple();
    CampaignManager.Instance.numeroTurno++;
    if (CampaignManager.Instance.logDeCampania != null)
    {
      CampaignManager.Instance.logDeCampania.RegistrarInicioDia(
        CampaignManager.Instance.numeroTurno,
        CampaignManager.Instance.GetEsperanzaActual(),
        CampaignManager.Instance.GetOroActuales(),
        CampaignManager.Instance.GetMaterialesActuales(),
        CampaignManager.Instance.GetSuministrosActuales(),
        CampaignManager.Instance.intTipoClima);
      CampaignManager.Instance.logDeCampania.RegistrarDescanso();
    }

    CampaignManager.Instance.scSequitoMercaderes.GenerarItemsVendidos();

    gameObject.SetActive(false);

    CampaignManager.Instance.scAdministradorEscenas.PlayFadeInOut(1.2f, 4.0f);
    await BattleManager.DelayCombateAsync(TimeSpan.FromSeconds(6.0f));
    IntentarEncontrarAtajoSuperficieTrasDescanso();

    if (tareaCivilSeleccionada == 1)
    {
      int random = UnityEngine.Random.Range(0, 21);
      float total = valor + random;

      CampaignManager.Instance.CambiarSuministrosActuales((int)total);

    } //Recoleccion Suministros

    if (tareaCivilSeleccionada == 2)
    {
      int random = UnityEngine.Random.Range(0, 11);
      CampaignManager.Instance.CambiarMaterialesActuales((int)valor + random);

    } //Recoleccion Materiales

    float consumo = CampaignManager.Instance.GetCivilesActual() + CampaignManager.Instance.GetBueyesActual() * 2;
    if (tareaCivilSeleccionada == 3)
    {
      consumo = consumo * 1.2f;
      int random = UnityEngine.Random.Range(0, 11);
      CampaignManager.Instance.CambiarEsperanzaActual(15 + random);

    } //Fiesta

    consumo = consumo / 100 * (100 - CampaignManager.Instance.mejoraCaravanaAlmacen * 3);

    //Modificadores de Climas
    if (CampaignManager.Instance.intTipoClima == 2) //Calor
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
      pers.ReducirCampBendecido();

      if (pers.PuedeRealizarActividades() && pers.ActividadSeleccionada == 4) //Caballero: Relatos de Batalla
      {
        CampaignManager.Instance.CambiarEsperanzaActual(4);
        CampaignManager.Instance.EscribirLog($"-" + pers.sNombre + TRADU.i.Traducir(" comparte sus historias de batalla con los civiles. +4 Esperanza"));
      }
      if (pers.Camp_Enfermo > 0) //Disminuye Enfermedad
      {
        pers.Camp_Enfermo -= 1; //Se cura un día

        //Sequito Curanderos ayuda a disminuir enfermedad 1 extra
        int rand = UnityEngine.Random.Range(1, 100);
        float tierCuranderos = ((CampaignManager.Instance.sequitoCuranderosMejoraCuracion * 100) - 10) / 5;
        if (pers.Camp_Enfermo > 0 && rand <= 20 + (int)tierCuranderos * 10)
        {
          pers.Camp_Enfermo -= 1;
          CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-El Séquito de Curanderos ha reducido la enfermedad de") + pers.sNombre + TRADU.i.Traducir(" en 1 extra."));

        }
      }
      if (pers.Camp_Moral > 0) //Moral tiende a cero
      {
        pers.Camp_Moral -= 1;
      }
      if (pers.Camp_Moral < 0) //Moral tiende a cero
      {
        pers.Camp_Moral += 1;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitDulcesSuenos))
      {
        pers.Camp_Moral = Mathf.Max(pers.Camp_Moral, 2);
        int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
        string mensajeDulcesSuenos = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " sleeps peacefully. Gains High Morale for 2 days.",
          TRADU.IdiomaPortugues => pers.sNombre + " dorme em paz. Recebe Alta Moral por 2 dias.",
          _ => pers.sNombre + " duerme plácidamente. Obtiene Alta Moral por 2 días."
        };
        CampaignManager.Instance.EscribirLog("-" + mensajeDulcesSuenos);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitPesadillasRecurrentes))
      {
        pers.Camp_Moral = Mathf.Min(pers.Camp_Moral, -1);
        int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
        string mensajePesadillas = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " suffers recurring nightmares. Gains Low Morale for 1 day.",
          TRADU.IdiomaPortugues => pers.sNombre + " sofre pesadelos recorrentes. Recebe Moral Baixa por 1 dia.",
          _ => pers.sNombre + " sufre pesadillas recurrentes. Obtiene Baja Moral por 1 día."
        };
        CampaignManager.Instance.EscribirLog("-" + mensajePesadillas);
      }

      CampaignManager.Instance.ProcesarTraitContratoSiCorresponde(pers);



      //Curacion General por descansar


      int cantPurificadorasColaborando = CampaignManager.Instance.CuantosPersonajesHacenTalActividad(12); //Colaborar con los Curanderos

      float porcentajeVidaMax = pers.fVidaMaxima * (0.15f+CampaignManager.Instance.sequitoCuranderosMejoraCuracion + (cantPurificadorasColaborando * 0.05f)); //5% por cada Purificadora colaborando
      porcentajeVidaMax = pers.AplicarMultiplicadorCuracionCampaniaTraits(porcentajeVidaMax);



      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoClaro) //Bonus descansar en claro
      { porcentajeVidaMax = porcentajeVidaMax * 1.1f; }
      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoRecursos) //Bonus descansar en nodo de recursos
      { porcentajeVidaMax = porcentajeVidaMax * 1.2f; }
      if (tareaCivilSeleccionada == 4) //Bonus por actividad civil Día Libre
      { porcentajeVidaMax = porcentajeVidaMax * 1.1f; }

      if (pers.fVidaMaxima > pers.fVidaActual)
      {
        CampaignManager.Instance.EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" se cura ") + (int)porcentajeVidaMax + TRADU.i.Traducir(" PV tras el Descanso."));
      }
      pers.RecibirCuracion(porcentajeVidaMax);


    }

    //Efectos de Sequitos al descansar
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(4) && tareaCivilSeleccionada == 3) //Artistas
    {
      CampaignManager.Instance.CambiarEsperanzaActual(10);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-En la Feria, los Artistas han realizado un espectáculo que ha levantado el ánimo de los Civiles. +10 Esperanza"));
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
      CampaignManager.Instance.BendecirPersonajesSantuario(4);

      string mensajeSantuario = TRADU.i.nIdioma switch
      {
        TRADU.IdiomaIngles => "-Resting in the Sanctuary blesses all characters for 4 days.",
        TRADU.IdiomaPortugues => "-Descansar no Santuário abençoa todos os personagens por 4 dias.",
        _ => "-Descansar en el Santuario bendice a todos los personajes por 4 días."
      };

      CampaignManager.Instance.EscribirLog(mensajeSantuario);
    }






    CampaignManager.Instance.BosqueArdienteMecanicaIncendio(30);
    CampaignManager.Instance.BosqueArdienteMecanicaIncendio(10);
    CampaignManager.Instance.PasoVientoHeladoMecanicaRituales(30);

    int fatiga = CampaignManager.Instance.GetFatigaActual();
    if (tareaCivilSeleccionada == 4)
    { fatiga++; CampaignManager.Instance.CambiarEsperanzaActual(10); } //Día libre

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
      float random = UnityEngine.Random.Range(1, 21) + CampaignManager.Instance.GetCivilesActual() / 3;
      CampaignManager.Instance.CambiarOroActual((int)random);

      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Debido al optimismo que rodea la Caravana, los Civiles han donado Oro: ") + random);
    }
    else if (CampaignManager.Instance.GetEsperanzaActual() >= 90)
    {
      float random = UnityEngine.Random.Range(1, 21) + CampaignManager.Instance.GetCivilesActual() / 2;
      CampaignManager.Instance.CambiarOroActual((int)random);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Debido al gran optimismo que rodea la Caravana, los Civiles han donado Oro: ") + random);

    }

    TiradaClima();
    CampaignManager.Instance.AplicarTraitsMoraleAmbientales();
    CampaignManager.Instance.sunController.ResetSun();

    #region Acechadores Sueldo
    //Sueldo Acechadores
    int sueldoAcechadores = 0;
    if (CampaignManager.Instance.GetEsperanzaActual() < 70) //Si la esperanza es menor a 70, los Acechadores cobran su sueldo.
    {
      sueldoAcechadores = CampaignManager.Instance.CuantosPersonajesSonDeTalClase(4) * 20; //Acechadores
      CampaignManager.Instance.CambiarOroActual(-sueldoAcechadores);
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Acechadores en la Caravana se han cobrado su sueldo por Oro: ") + sueldoAcechadores);
    }
    else //Si la esperanza es mayor o igual a 70, no cobran.
    {
      int cantidadacechadores = CampaignManager.Instance.CuantosPersonajesSonDeTalClase(4); //Acechadores
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Debido a la alta Esperanza, los Acechadores han decidido no cobrar su sueldo esta vez."));

    }
    #endregion

    #region   Avance Aliento Negro al Descansar
    bool sePrevieneAvanceAliento = false;
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers == null || !pers.PuedeRealizarActividades())
      {
        continue;
      }

      int random = UnityEngine.Random.Range(0, 100);
      if (pers.ActividadSeleccionada == 10 && random < 25) //Purificadora: Ritual de Limpieza 
      {
        sePrevieneAvanceAliento = true;
        CampaignManager.Instance.EscribirLog("-" + pers.sNombre + TRADU.i.Traducir(" ha realizado con Éxito un Ritual de Limpieza durante el descanso, previniendo el avance del Aliento Negro."));
        break;
      }
    }

    if (!sePrevieneAvanceAliento)
    {
      if (CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == TipoNodoClaro) //Bonus descansar en claro
      {
        CampaignManager.Instance.CambiarValorAlientoNegro(1);
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Durante el descanso en el Claro, el Aliento Negro ha avanzado 1."));
      }
      else
      {
        CampaignManager.Instance.CambiarValorAlientoNegro(2);
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Durante el descanso, el Aliento Negro ha avanzado 2."));

      }
    }
    #endregion

    float randomEvento = UnityEngine.Random.Range(0, 100);
    float factorEventoBuenoMalo = 36 + CampaignManager.Instance.GetEsperanzaActual() / 3 + CampaignManager.Instance.ObtenerModificadorChanceEventoTraits();
    factorEventoBuenoMalo = Mathf.Clamp(factorEventoBuenoMalo, 0f, 100f);
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

      int randomEmboscada = UnityEngine.Random.Range(1, 101);
      if (enTutorial) { randomEmboscada += 100; } //no hay emboscada en el tutorial

      if (!enTutorial && !descansoEnAsentamiento && randomEmboscada <= chancesAtaqueACaravana)
      {
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La caravana han sufrido un Ataque durante el descanso. Probabilidades ") + chancesAtaqueACaravana + TRADU.i.Traducir("% - Tirada: 1d100 = ") + randomEmboscada);

        TutorialTooltipManager.TryShow(TooltipEmboscadaNormalId);
        CampaignManager.Instance.scMenuBatallas.EventoBatallaCaravana(0, 3);
        autosavePendienteTrasDescanso = false;
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
      CampaignManager.Instance.TryAutosaveCampania("descanso", out _);
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

    int random = UnityEngine.Random.Range(1, 101);
    if (CampaignManager.Instance.DebeUsarConfiguracionTutorial())
    {
      bool tutorialLegacyActivo = CampaignManager.Instance.scTutorialManager != null
        && CampaignManager.Instance.scTutorialManager.tutorialActivo;
      int pasoTutorial = tutorialLegacyActivo ? CampaignManager.Instance.scTutorialManager.pasoActual : 5;
      if (pasoTutorial > 4)
      {
        int almasDanzantesForzada = CampaignManager.Instance.scAtributosZona.Clima_chances_Niebla + 1;
        if (almasDanzantesForzada >= CampaignManager.Instance.scAtributosZona.Clima_chances_EspecialZona1)
        {
          almasDanzantesForzada = Mathf.Max(1, CampaignManager.Instance.scAtributosZona.Clima_chances_EspecialZona1 - 1);
        }
        random = Mathf.Clamp(almasDanzantesForzada, 1, 100);
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
        random = UnityEngine.Random.Range(1, 101);
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
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_almasDanzantes;


        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Las Almas Danzantes de animales inocentes guian a la caravana. +5 Esperanza, 0% chances de emboscada."));
        CampaignManager.Instance.CambiarEsperanzaActual(5);
      }
      if (CampaignManager.Instance.scAtributosZona.ID == 2) //Paso Helado - Aurora Boreal
      {
        climaAuroraBoreal.SetActive(true);

        CampaignManager.Instance.intTipoClima = 7;
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_auroraboreal;


        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La Aurora Boreal maravilla a toda la caravana. +10 Esperanza"));
        CampaignManager.Instance.CambiarEsperanzaActual(10);
      }

      if (CampaignManager.Instance.scAtributosZona.ID == 3) //Nedukazal - Oscuridad
      {
        CampaignManager.Instance.intTipoClima = 8;
        CampaignManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_NedukazalNormal;

      }


    }
    else if (random < CampaignManager.Instance.scAtributosZona.Clima_chances_EspecialZona2)
    {
      if (CampaignManager.Instance.scAtributosZona.ID == 3) //Nedukazal - Masacre
      {
        climaMasacre.SetActive(true);
        CampaignManager.Instance.intTipoClima = 9;
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
