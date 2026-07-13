using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
// using TMPro; // removido (no se usa)

public class AdministradorEscenas : MonoBehaviour
{
  private static bool mantenerFaderNegroEnProximaCargaCampania;

  public GameObject EscenaCampaign;
  public GameObject EscenaBatalla;

  public ContenedorPrefabs ContenedorPrefabsBatalla;

  public GameObject HandbookCampania;

  public int escenaActual; //0 campaña 1 batalla


  public Personaje Personaje1;
  public Personaje Personaje2;
  public Personaje Personaje3;
  public Personaje Personaje4;

  public Unidad unidadPers1;
  public Unidad unidadPers2;
  public Unidad unidadPers3;
  public Unidad unidadPers4;

  // Personajes forzados a desplegar en Ataque a Caravana (se les aplica "Sorprendido" al inicio)
  public List<Personaje> PersonajesSorprendidosInicioCaravana = new List<Personaje>();
  public List<RefuerzoAliadoCaravanaOrdenItem> OrdenRefuerzosAliadosCaravana = new List<RefuerzoAliadoCaravanaOrdenItem>();


  public MeshRenderer mrFondoBatalla;

  public List<Material> listaFondosBosqueLamentos; //Para agregar fondos simplemente hay que agregarlos a la lista
  public List<Material> listaFondosPasoVientoHelado; //Para agregar fondos simplemente hay que agregarlos a la lista

  public List<Material> listaFondosNedukazal; //Para agregar fondos simplemente hay que agregarlos a la lista

  public List<Material> listaFondosSubterraneos; //Para agregar fondos simplemente hay que agregarlos a la lista
  private BattleAmbientLife battleAmbientLife;
  private EncounterZoneType zonaVisualBatallaActual = EncounterZoneType.BosqueAngustiante;
  private bool fondoBatallaSubterraneoActual;

  [Header("UI")]
  [SerializeField] public CanvasGroup fader;        // Imagen negra con CanvasGroup
  [SerializeField] float fadeTime = 0.4f;
  [Header("Ritmo Demo")]
  [SerializeField] float fadeEntradaBatalla = 0.45f;
  [SerializeField] float esperaEntradaBatalla = 1.15f;
  [SerializeField] float fadeSalidaBatalla = 0.55f;
  [SerializeField] float esperaSalidaBatalla = 1.05f;
  [SerializeField] int delayPostFinBatallaMs = 120;
  // Bloqueo para mantener el fader negro (evita fade-outs concurrentes)
  bool faderHold = false;
  bool cargandoBatalla = false;
  bool cerrandoBatalla = false;
  float CalcularEsperaCambioEscena(float fadeDuration, float holdDuration)
  {
    return Mathf.Max(fadeDuration + 0.05f, holdDuration);
  }

  async Task EsperarSegundosRealtime(float segundos)
  {
    int delayMs = Mathf.Max(0, Mathf.RoundToInt(segundos * 1000f));
    if (delayMs <= 0) { return; }
    await BattleManager.DelayCombateAsync(delayMs);
  }
  public GameObject MenuOpciones;
  private Canvas canvasLogHandbook;
  private GameObject goLogCompartido;
  private int sortingOrderCanvasLogHandbookOriginal = int.MinValue;

  public void abrirOpciones()
  {
    MenuOpciones.SetActive(!MenuOpciones.activeInHierarchy);
  }

  Canvas ObtenerCanvasLogHandbook()
  {
    if (canvasLogHandbook != null)
    {
      return canvasLogHandbook;
    }

    Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
    for (int i = 0; i < canvases.Length; i++)
    {
      if (canvases[i] != null && canvases[i].name == "CanvasLog/Handbook")
      {
        canvasLogHandbook = canvases[i];
        sortingOrderCanvasLogHandbookOriginal = canvasLogHandbook.sortingOrder;
        break;
      }
    }

    return canvasLogHandbook;
  }

  GameObject ObtenerLogCompartido()
  {
    if (goLogCompartido != null)
    {
      return goLogCompartido;
    }

    Canvas canvasCompartido = ObtenerCanvasLogHandbook();
    if (canvasCompartido == null)
    {
      return null;
    }

    Transform logTransform = canvasCompartido.transform.Find("Log");
    if (logTransform != null)
    {
      goLogCompartido = logTransform.gameObject;
    }

    return goLogCompartido;
  }

  public void RefrescarUICompartidaSegunEscena()
  {
    Canvas canvasCompartido = ObtenerCanvasLogHandbook();
    if (canvasCompartido == null)
    {
      return;
    }

    if (!canvasCompartido.gameObject.activeSelf)
    {
      canvasCompartido.gameObject.SetActive(true);
    }

    int sortingOrderBase = sortingOrderCanvasLogHandbookOriginal != int.MinValue
      ? sortingOrderCanvasLogHandbookOriginal
      : canvasCompartido.sortingOrder;

    canvasCompartido.sortingOrder = escenaActual == 1
      ? Mathf.Max(sortingOrderBase, 50)
      : sortingOrderBase;

    GameObject logCompartido = ObtenerLogCompartido();
    if (logCompartido != null && logCompartido.activeSelf != (escenaActual == 1))
    {
      logCompartido.SetActive(escenaActual == 1);
    }
  }
  async void EjecutarOperacionSegura(Task tarea, string contexto)
  {
    try
    {
      await tarea;
    }
    catch (System.Exception ex)
    {
      Debug.LogError($"[AdministradorEscenas] Error en {contexto}.");
      Debug.LogException(ex, this);
    }
  }

  void SetActiveSiExiste(GameObject go, bool activo)
  {
    if (go != null)
    {
      go.SetActive(activo);
    }
  }

  void ResolverReferenciasVfxAlientoNegroBatalla()
  {
    if (EscenaBatalla == null) { return; }
    Transform raizClimas = EscenaBatalla.transform.Find("Climas");
    if (raizClimas == null) { return; }

    if (VFXAlientoLeve == null)
    {
      Transform alientoLeve = raizClimas.Find("Clima_AlientoLeve");
      if (alientoLeve != null) { VFXAlientoLeve = alientoLeve.gameObject; }
    }

    if (VFXAlientoMedio == null)
    {
      Transform alientoMedio = raizClimas.Find("Clima_AlientoMedio");
      if (alientoMedio != null) { VFXAlientoMedio = alientoMedio.gameObject; }
    }

    if (VFXAlientoGrande == null)
    {
      Transform alientoGrande = raizClimas.Find("Clima_AlientoGrande");
      if (alientoGrande != null) { VFXAlientoGrande = alientoGrande.gameObject; }
    }
  }

  void RefrescarVfxAlientoNegroBatalla(bool desactivarTodo = false)
  {
    ResolverReferenciasVfxAlientoNegroBatalla();

    SetActiveSiExiste(VFXAlientoLeve, false);
    SetActiveSiExiste(VFXAlientoMedio, false);
    SetActiveSiExiste(VFXAlientoGrande, false);

    if (desactivarTodo || CampaignManager.Instance == null)
    {
      return;
    }

    int tierAlientoNegro = (int)CampaignManager.Instance.GetTierAlientoNegro();
    GameObject vfxActivo = null;
    if (tierAlientoNegro == 2)
    {
      vfxActivo = VFXAlientoLeve;
    }
    else if (tierAlientoNegro == 3)
    {
      vfxActivo = VFXAlientoMedio;
    }
    else if (tierAlientoNegro >= 4)
    {
      vfxActivo = VFXAlientoGrande;
    }

    if (vfxActivo != null)
    {
      VisualPolishRuntime.ApplyBlackBreathParticleQualityScale(vfxActivo);
      SetActiveSiExiste(vfxActivo, true);
    }
  }

  bool ValidarDependenciasCambioABatalla(out CampaignManager campaignManager)
  {
    campaignManager = CampaignManager.Instance;
    if (campaignManager == null)
    {
      Debug.LogError("[AdministradorEscenas] No se puede cargar batalla: CampaignManager no inicializado.");
      return false;
    }

    if (campaignManager.scAtributosZona == null)
    {
      Debug.LogError("[AdministradorEscenas] No se puede cargar batalla: falta AtributosZona.");
      return false;
    }

    if (EscenaCampaign == null || EscenaBatalla == null)
    {
      Debug.LogError("[AdministradorEscenas] No se puede cargar batalla: faltan referencias a EscenaCampaign/EscenaBatalla.");
      return false;
    }

    return true;
  }

  bool ValidarDependenciasFinDeBatalla(out CampaignManager campaignManager, out BattleManager battleManager)
  {
    campaignManager = CampaignManager.Instance;
    battleManager = BattleManager.Instance;

    if (campaignManager == null)
    {
      Debug.LogError("[AdministradorEscenas] No se puede cerrar batalla: CampaignManager no inicializado.");
      return false;
    }

    if (battleManager == null)
    {
      Debug.LogError("[AdministradorEscenas] No se puede cerrar batalla: BattleManager no inicializado.");
      return false;
    }

    if (EscenaCampaign == null || EscenaBatalla == null)
    {
      Debug.LogError("[AdministradorEscenas] No se puede cerrar batalla: faltan referencias a EscenaCampaign/EscenaBatalla.");
      return false;
    }

    return true;
  }

  bool EsActividadGuardiaBaseOCaballero(Personaje personaje)
  {
    if (personaje == null || !personaje.PuedeRealizarActividades()) { return false; }
    return personaje.ActividadSeleccionada == 3 || personaje.ActividadSeleccionada == 6;
  }

  bool EsActividadGuardiaParaCaravana(Personaje personaje)
  {
    if (personaje == null || !personaje.PuedeRealizarActividades()) { return false; }
    return EsActividadGuardiaBaseOCaballero(personaje) || personaje.ActividadSeleccionada == 14;
  }

  void AplicarSigiloInicialCaravana(Personaje personaje, Unidad unidad)
  {
    if (personaje == null || unidad == null) { return; }
    if (!personaje.PuedeRealizarActividades()) { return; }
    if (personaje.ActividadSeleccionada != 14) { return; }
    unidad.GanarEscondido(1);
  }

  bool TieneTrait(Personaje personaje, int traitId)
  {
    return personaje != null && personaje.TieneRasgo(traitId);
  }

  bool PuedeSerSorprendidoPorEmboscada(Personaje personaje)
  {
    return !TieneTrait(personaje, PersonajeTraitCatalog.TraitAvispado);
  }

  bool EsDefensaCaravanaActual()
  {
    return tipoEmboscadaActual == 3
      || (encuentroGeneradoActual != null && encuentroGeneradoActual.battleType == BattleEncounterType.AtaqueCaravana);
  }

  public Personaje ObtenerPersonajeAliadoSeleccionadoPorUnidad(Unidad unidad)
  {
    if (unidad == null)
    {
      return null;
    }

    if (unidad == unidadPers1) return Personaje1;
    if (unidad == unidadPers2) return Personaje2;
    if (unidad == unidadPers3) return Personaje3;
    if (unidad == unidadPers4) return Personaje4;
    return null;
  }

  public Unidad ObtenerUnidadAliadaSeleccionadaPorPersonaje(Personaje personaje)
  {
    if (personaje == null)
    {
      return null;
    }

    if (personaje == Personaje1) return unidadPers1;
    if (personaje == Personaje2) return unidadPers2;
    if (personaje == Personaje3) return unidadPers3;
    if (personaje == Personaje4) return unidadPers4;
    return null;
  }

  void ResetearEstadoTraitsCombateParticipantes()
  {
    Personaje1?.ResetearEstadoTraitsCombate();
    Personaje2?.ResetearEstadoTraitsCombate();
    Personaje3?.ResetearEstadoTraitsCombate();
    Personaje4?.ResetearEstadoTraitsCombate();
  }

  void AplicarBuffSiNoExiste(
    Unidad unidad,
    string nombreBuff,
    bool esBuff,
    int duracion,
    float cantAtaque = 0f,
    float cantDefensa = 0f,
    float cantDanioPorcentaje = 0f,
    float cantCritDado = 0f,
    float cantIniciativa = 0f,
    float cantAPMax = 0f,
    float cantTsMental = 0f, bool seVeBarraVida = true)
  {
    if (unidad == null || string.IsNullOrWhiteSpace(nombreBuff) || unidad.TieneBuffNombre(nombreBuff))
    {
      return;
    }

    Buff buff = new Buff();
    buff.buffNombre = nombreBuff;
    buff.boolfDebufftBuff = esBuff;
    buff.DuracionBuffRondas = duracion;
    buff.esStackeable = false;
    buff.cantAtaque += cantAtaque;
    buff.cantDefensa += cantDefensa;
    buff.cantDanioPorcentaje += cantDanioPorcentaje;
    buff.cantCritDado += cantCritDado;
    buff.cantIniciativa += cantIniciativa;
    buff.cantAPMax += cantAPMax;
    buff.cantTsMental += cantTsMental;
    buff.ocultarEnBarraVida = !seVeBarraVida;
    buff.AplicarBuff(unidad);
    
    ComponentCopier.CopyComponent(buff, unidad.gameObject);
  }

  int ContarAliadosSeleccionadosNoIAVivos()
  {
    int cantidad = 0;
    Unidad[] aliados = { unidadPers1, unidadPers2, unidadPers3, unidadPers4 };
    for (int i = 0; i < aliados.Length; i++)
    {
      Unidad aliado = aliados[i];
      if (aliado == null || aliado.HP_actual <= 0f || aliado.GetComponent<IAUnidad>() != null)
      {
        continue;
      }

      cantidad++;
    }

    return cantidad;
  }

  void RemoverBuffsValorGrupal(Unidad unidad)
  {
    if (unidad == null)
    {
      return;
    }

    unidad.RemoverBuffNombre("Valentía Global Alta");
    unidad.RemoverBuffNombre("Valentia Global Alta");
    unidad.RemoverBuffNombre("ValentÃ­a Global Alta");
    unidad.RemoverBuffNombre("Valour Global Alto");
    unidad.RemoverBuffNombre("Valentía Global Muy Alta");
    unidad.RemoverBuffNombre("Valentia Global Muy Alta");
    unidad.RemoverBuffNombre("ValentÃ­a Global Muy Alta");
    unidad.RemoverBuffNombre("Valour Global Muy Alto");
  }

  public bool DebeIgnorarValorGrupalPorLoboSolitario(Unidad unidad)
  {
    Personaje pers = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidad);
    return pers != null && pers.TieneRasgo(PersonajeTraitCatalog.TraitLoboSolitario);
  }

  public void RefrescarTraitsComposicionAliada()
  {
    int aliadosVivos = ContarAliadosSeleccionadosNoIAVivos();
    Personaje[] personajes = { Personaje1, Personaje2, Personaje3, Personaje4 };
    Unidad[] unidades = { unidadPers1, unidadPers2, unidadPers3, unidadPers4 };

    for (int i = 0; i < personajes.Length; i++)
    {
      Personaje pers = personajes[i];
      Unidad unidad = unidades[i];
      if (pers == null || unidad == null)
      {
        continue;
      }

      unidad.RemoverBuffNombre("Lobo Solitario");
      unidad.RemoverBuffNombre("Claridad");

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitLoboSolitario))
      {
        RemoverBuffsValorGrupal(unidad);

        if (aliadosVivos == 1 && unidad.HP_actual > 0f)
        {
          AplicarBuffSiNoExiste(unidad, "Lobo Solitario", true, -1, cantAtaque: 2f, cantDefensa: 1f, cantDanioPorcentaje: 15f, cantAPMax: 2f, cantTsMental: 1f);
        }
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitTrabajoEnEquipo)
        && unidad.HP_actual > 0f)
      {
        int companerosVivos = Mathf.Max(0, aliadosVivos - 1);
        if (companerosVivos > 0)
        {
          AplicarBuffSiNoExiste(unidad, "Claridad", true, -1, cantTsMental: companerosVivos);
        }
      }
    }
  }

  public int AjustarDanioRecibidoPorTraits(Unidad unidad, int danioBase)
  {
    if (unidad == null || danioBase <= 0)
    {
      return danioBase;
    }

    Personaje pers = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidad);
    if (pers != null && pers.TieneRasgo(PersonajeTraitCatalog.TraitFragil))
    {
      return Mathf.Max(0, Mathf.RoundToInt(danioBase * 1.05f));
    }

    return danioBase;
  }

  public bool ProcesarTraitPuertasDeLaMuerteSiCorresponde(Unidad unidad)
  {
    if (unidad == null || unidad.HP_actual >= 1f || unidad.mod_maxHP <= 0f)
    {
      return false;
    }

    Personaje pers = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidad);
    if (pers == null
      || !pers.TieneRasgo(PersonajeTraitCatalog.TraitPuertasDeLaMuerte)
      || pers.TraitPuertasDeLaMuerteActivadoEnCombate)
    {
      return false;
    }

    pers.TraitPuertasDeLaMuerteActivadoEnCombate = true;
    unidad.HP_actual = Mathf.Max(1f, unidad.mod_maxHP * 0.05f);
    unidad.estado_invulnerable = Mathf.Max(unidad.estado_invulnerable, 1);
    AplicarBuffSiNoExiste(unidad, "Invulnerable", true, 1);
    return true;
  }

  void AplicarAltaOMoralBajaContagiada(Unidad unidadObjetivo, bool altaMoral)
  {
    if (unidadObjetivo == null || unidadObjetivo.HP_actual <= 0f)
    {
      return;
    }

    if (unidadObjetivo.TieneBuffNombre("Alta Moral") || unidadObjetivo.TieneBuffNombre("Baja Moral"))
    {
      return;
    }

    if (altaMoral)
    {
      AplicarBuffSiNoExiste(unidadObjetivo, "Alta Moral", true, -1, cantAtaque: 1f, cantTsMental: 2f);
      unidadObjetivo.SumarValentia(2, mostrarTextoFlotante: false);
      return;
    }

    AplicarBuffSiNoExiste(unidadObjetivo, "Baja Moral", false, -1, cantAtaque: -1f, cantDefensa: -1f, cantTsMental: -3f);
    unidadObjetivo.SumarValentia(-2, mostrarTextoFlotante: false);
  }

  void PropagarEspirituSiCorresponde(Personaje pers, Unidad unidad)
  {
    if (pers == null || unidad == null)
    {
      return;
    }

    bool propagaAlta = TieneTrait(pers, PersonajeTraitCatalog.TraitEspirituAlegre) && pers.Camp_Moral > 0;
    bool propagaBaja = TieneTrait(pers, PersonajeTraitCatalog.TraitEspirituNegativo) && pers.Camp_Moral < 0;
    if (!propagaAlta && !propagaBaja)
    {
      return;
    }

    Unidad[] aliados = { unidadPers1, unidadPers2, unidadPers3, unidadPers4 };
    for (int i = 0; i < aliados.Length; i++)
    {
      Unidad aliado = aliados[i];
      if (aliado == null || aliado == unidad || aliado.GetComponent<IAUnidad>() != null)
      {
        continue;
      }

      AplicarAltaOMoralBajaContagiada(aliado, propagaAlta);
    }
  }

  public void ProcesarTraitDuroDeMatarSiCorresponde(Unidad unidad)
  {
    if (unidad == null || unidad.HP_actual <= 0f || unidad.mod_maxHP <= 0f)
    {
      return;
    }

    Personaje pers = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidad);
    if (pers == null || pers.TraitDuroDeMatarActivadoEnCombate || !pers.TieneRasgo(PersonajeTraitCatalog.TraitDuroDeMatar))
    {
      return;
    }

    if (unidad.HP_actual >= unidad.mod_maxHP * 0.20f)
    {
      return;
    }

    pers.TraitDuroDeMatarActivadoEnCombate = true;
    AplicarBuffSiNoExiste(unidad, "Duro de Matar", true, -1, cantDefensa: 2f);
  }

  public void ProcesarTraitVengativoPorAliadoCaido(Unidad unidadCaida)
  {
    if (unidadCaida == null || unidadCaida.GetComponent<IAUnidad>() != null)
    {
      return;
    }

    Personaje persCaido = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidadCaida);
    if (persCaido == null)
    {
      return;
    }

    Personaje[] personajes = { Personaje1, Personaje2, Personaje3, Personaje4 };
    Unidad[] unidades = { unidadPers1, unidadPers2, unidadPers3, unidadPers4 };
    for (int i = 0; i < personajes.Length; i++)
    {
      Personaje pers = personajes[i];
      Unidad unidad = unidades[i];
      if (pers == null || unidad == null || pers == persCaido || unidad.HP_actual <= 0f)
      {
        continue;
      }

      if (!pers.TieneRasgo(PersonajeTraitCatalog.TraitVengativo) || pers.TraitVengativoActivadoEnCombate)
      {
        continue;
      }

      pers.TraitVengativoActivadoEnCombate = true;
      AplicarBuffSiNoExiste(unidad, "Furia", true, -1, cantDanioPorcentaje: 15f, cantDefensa: -1f);
    }

  }

  public void ProcesarTraitsInicioRonda()
  {
    if (BattleManager.Instance == null)
    {
      return;
    }

    Personaje[] personajes = { Personaje1, Personaje2, Personaje3, Personaje4 };
    Unidad[] unidades = { unidadPers1, unidadPers2, unidadPers3, unidadPers4 };
    for (int i = 0; i < personajes.Length; i++)
    {
      Personaje pers = personajes[i];
      Unidad unidad = unidades[i];
      if (pers == null || unidad == null || unidad.HP_actual <= 0f)
      {
        continue;
      }

      if (BattleManager.Instance.RondaNro < 3)
      {
        continue;
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitPaciente) && !pers.TraitPacienteAplicadoEnCombate)
      {
        pers.TraitPacienteAplicadoEnCombate = true;
        AplicarBuffSiNoExiste(unidad, "Paciente", true, -1, cantAtaque: 1f, cantDefensa: 1f, cantDanioPorcentaje: 10f);
      }

      if (pers.TieneRasgo(PersonajeTraitCatalog.TraitImpulsivo) && !pers.TraitImpulsivoCansadoAplicadoEnCombate)
      {
        pers.TraitImpulsivoCansadoAplicadoEnCombate = true;
        AplicarBuffSiNoExiste(unidad, "Cansado", false, -1, cantAtaque: -1f, cantDanioPorcentaje: -10f);
      }
    }
  }

  public bool DebeBloquearIntercambioPorIndividualista(Unidad unidadActiva, Unidad unidadObjetivo, out Unidad unidadTexto)
  {
    unidadTexto = null;

    Personaje persActiva = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidadActiva);
    if (persActiva != null && persActiva.TieneRasgo(PersonajeTraitCatalog.TraitIndividualista))
    {
      unidadTexto = unidadActiva;
      return true;
    }

    Personaje persObjetivo = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidadObjetivo);
    if (persObjetivo != null && persObjetivo.TieneRasgo(PersonajeTraitCatalog.TraitIndividualista))
    {
      unidadTexto = unidadActiva != null ? unidadActiva : unidadObjetivo;
      return true;
    }

    return false;
  }

  public void MostrarTextoIntercambioBloqueadoIndividualista(Unidad unidadTexto)
  {
    if (unidadTexto == null)
    {
      return;
    }

    _ = unidadTexto.GenerarTextoFlotante(TRADU.i != null ? TRADU.i.Traducir("Prohibido: Individualista") : "Prohibido: Individualista", Color.gray, FloatingTextContext.Resist);
  }

  public bool TryConsumirIntercambioGratisColaborativo(Unidad unidadActiva, Unidad unidadObjetivo)
  {
    Personaje persActiva = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidadActiva);
    if (persActiva != null && persActiva.TieneRasgo(PersonajeTraitCatalog.TraitColaborativo) && !persActiva.TraitColaborativoUsadoEnCombate)
    {
      persActiva.TraitColaborativoUsadoEnCombate = true;
      return true;
    }

    Personaje persObjetivo = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidadObjetivo);
    if (persObjetivo != null && persObjetivo.TieneRasgo(PersonajeTraitCatalog.TraitColaborativo) && !persObjetivo.TraitColaborativoUsadoEnCombate)
    {
      persObjetivo.TraitColaborativoUsadoEnCombate = true;
      return true;
    }

    return false;
  }

  public bool TieneIntercambioGratisColaborativoDisponible(Unidad unidadActiva, Unidad unidadObjetivo)
  {
    Personaje persActiva = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidadActiva);
    if (persActiva != null && persActiva.TieneRasgo(PersonajeTraitCatalog.TraitColaborativo) && !persActiva.TraitColaborativoUsadoEnCombate)
    {
      return true;
    }

    Personaje persObjetivo = ObtenerPersonajeAliadoSeleccionadoPorUnidad(unidadObjetivo);
    return persObjetivo != null && persObjetivo.TieneRasgo(PersonajeTraitCatalog.TraitColaborativo) && !persObjetivo.TraitColaborativoUsadoEnCombate;
  }

  bool RosterEnemigoTieneTag(string tagBuscado)
  {
    if (string.IsNullOrWhiteSpace(tagBuscado) || BattleManager.Instance == null)
    {
      return false;
    }

    if (BattleManager.Instance.ladoA != null && BattleManager.Instance.ladoA.unidadesLado != null)
    {
      foreach (Unidad enemigo in BattleManager.Instance.ladoA.unidadesLado)
      {
        if (enemigo != null && enemigo.TieneTag(tagBuscado))
        {
          return true;
        }
      }
    }

    if (BattleManager.Instance.enemigosRefuerzos != null)
    {
      foreach (GameObject refuerzo in BattleManager.Instance.enemigosRefuerzos)
      {
        Unidad unidadRefuerzo = refuerzo != null ? refuerzo.GetComponent<Unidad>() : null;
        if (unidadRefuerzo != null && unidadRefuerzo.TieneTag(tagBuscado))
        {
          return true;
        }
      }
    }

    return false;
  }

  string ObtenerNombreCategoriaTagTrait(string tagBuscado, int idioma)
  {
    return tagBuscado switch
    {
      "Corrupto" => idioma switch
      {
        TRADU.IdiomaIngles => "Corrupted",
        TRADU.IdiomaPortugues => "Corrompidos",
        _ => "Corruptos"
      },
      "Nomuerto" => idioma switch
      {
        TRADU.IdiomaIngles => "Undead",
        TRADU.IdiomaPortugues => "Mortos-vivos",
        _ => "Nomuertos"
      },
      "Bestia" => idioma switch
      {
        TRADU.IdiomaIngles => "Beasts",
        TRADU.IdiomaPortugues => "Bestas",
        _ => "Bestias"
      },
      "Criatura" => idioma switch
      {
        TRADU.IdiomaIngles => "Creatures",
        TRADU.IdiomaPortugues => "Criaturas",
        _ => "Criaturas"
      },
      "Demonio" => idioma switch
      {
        TRADU.IdiomaIngles => "Demons",
        TRADU.IdiomaPortugues => "Demônios",
        _ => "Demonios"
      },
      _ => tagBuscado
    };
  }

  string ObtenerNombreBuffOdioTag(string tagBuscado, int idioma)
  {
    return tagBuscado switch
    {
      "Corrupto" => idioma switch
      {
        TRADU.IdiomaIngles => "Hatred of the Corrupted",
        TRADU.IdiomaPortugues => "Ódio aos Corrompidos",
        _ => "Odia a Corruptos"
      },
      "Nomuerto" => idioma switch
      {
        TRADU.IdiomaIngles => "Hatred of the Undead",
        TRADU.IdiomaPortugues => "Ódio aos Mortos-vivos",
        _ => "Odia a Nomuertos"
      },
      "Bestia" => idioma switch
      {
        TRADU.IdiomaIngles => "Hatred of Beasts",
        TRADU.IdiomaPortugues => "Ódio às Bestas",
        _ => "Odia a Bestias"
      },
      "Criatura" => idioma switch
      {
        TRADU.IdiomaIngles => "Hatred of Creatures",
        TRADU.IdiomaPortugues => "Ódio às Criaturas",
        _ => "Odia a Criaturas"
      },
      "Demonio" => idioma switch
      {
        TRADU.IdiomaIngles => "Hatred of Demons",
        TRADU.IdiomaPortugues => "Ódio aos Demônios",
        _ => "Odia a Demonios"
      },
      _ => tagBuscado
    };
  }

  void AplicarTraitMiedoTag(Personaje pers, Unidad unidad, int traitId, string tagBuscado)
  {
    if (!TieneTrait(pers, traitId) || !RosterEnemigoTieneTag(tagBuscado))
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    string categoria = ObtenerNombreCategoriaTagTrait(tagBuscado, idiomaTrait);
    string motivo = idiomaTrait switch
    {
      TRADU.IdiomaIngles => pers.sNombre + " is shaken by the sight of " + categoria + ".",
      TRADU.IdiomaPortugues => pers.sNombre + " se abala ao encarar " + categoria + ".",
      _ => pers.sNombre + " se estremece ante " + categoria + "."
    };

    unidad.SumarValentia(-2, motivo);
  }

  void AplicarTraitOdioTag(Personaje pers, Unidad unidad, int traitId, string tagBuscado)
  {
    if (!TieneTrait(pers, traitId) || !RosterEnemigoTieneTag(tagBuscado))
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    string nombreBuff = ObtenerNombreBuffOdioTag(tagBuscado, idiomaTrait);
    if (unidad.TieneBuffNombre(nombreBuff))
    {
      return;
    }

    Buff buff = new Buff();
    buff.buffNombre = nombreBuff;
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = -1;
    buff.esStackeable = false;
    buff.cantDanioPorcentaje += 10;
    buff.cantAtaque += 1;
    buff.AplicarBuff(unidad);
    ComponentCopier.CopyComponent(buff, unidad.gameObject);
  }

  void AplicarSorprendidoInicioCombate(Unidad unidad, int penalizacionIniciativa = 3, int duracionRondas = 1)
  {
    if (unidad == null || unidad.TieneBuffNombre("Sorprendido"))
    {
      return;
    }

    unidad.PerderEscondido();

    Buff buff = new Buff();
    buff.buffNombre = "Sorprendido";
    buff.forzarTextoFlotanteInicioCombate = true;
    buff.ocultarEnBarraVida = false;
    buff.boolfDebufftBuff = false;
    buff.DuracionBuffRondas = duracionRondas;
    buff.cantIniciativa -= penalizacionIniciativa;
    buff.cantAPMax -= 1;
    buff.cantDefensa -= 2;
    buff.AplicarBuff(unidad);
    ComponentCopier.CopyComponent(buff, unidad.gameObject);
  }

  void AplicarRasgosInicioCombate(Personaje pers, Unidad unidad)
  {
    if (pers == null || unidad == null)
    {
      return;
    }

    if (TieneTrait(pers, PersonajeTraitCatalog.TraitValiente))
    {
      int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
      string motivoValiente = idiomaTrait switch
      {
        TRADU.IdiomaIngles => pers.sNombre + " enters battle emboldened.",
        TRADU.IdiomaPortugues => pers.sNombre + " entra no combate encorajado.",
        _ => pers.sNombre + " entra al combate envalentonado."
      };
      unidad.SumarValentia(2, motivoValiente);
    }

    if (TieneTrait(pers, PersonajeTraitCatalog.TraitCobarde))
    {
      bool fallaSalvacion = unidad.TiradaSalvacion(3, 12f);
      if (fallaSalvacion)
      {
        int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
        string motivoCobarde = idiomaTrait switch
        {
          TRADU.IdiomaIngles => pers.sNombre + " falters at the start of battle.",
          TRADU.IdiomaPortugues => pers.sNombre + " vacila no começo do combate.",
          _ => pers.sNombre + " vacila al comenzar el combate."
        };
        unidad.SumarValentia(-2, motivoCobarde);
      }
    }

    if (TieneTrait(pers, PersonajeTraitCatalog.TraitDistraido))
    {
      bool fallaSalvacion = unidad.TiradaSalvacion(3, 11f);
      if (fallaSalvacion)
      {
        AplicarSorprendidoInicioCombate(unidad);
      }
    }

    if (TieneTrait(pers, PersonajeTraitCatalog.TraitAlmaFuerte)
      && CampaignManager.Instance != null
      && CampaignManager.Instance.HayAlientoNegroIntenso()
      && !unidad.TieneBuffNombre("Alma Fuerte"))
    {
      Buff buff = new Buff();
      buff.buffNombre = "Alma Fuerte";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = -1;
      buff.cantDanioPorcentaje += 15;
      buff.cantTsMental += 2;
      buff.AplicarBuff(unidad);
      ComponentCopier.CopyComponent(buff, unidad.gameObject);
    }

    if (TieneTrait(pers, PersonajeTraitCatalog.TraitImpulsivo))
    {
      AplicarBuffSiNoExiste(unidad, "Impulsivo", true, 2, cantAtaque: 2f, cantDanioPorcentaje: 15f, cantIniciativa: 3f, cantAPMax: 1f);
    }

    if (EsDefensaCaravanaActual())
    {
      if (TieneTrait(pers, PersonajeTraitCatalog.TraitProtector))
      {
        AplicarBuffSiNoExiste(unidad, "Protector de la caravana", true, -1, cantDefensa: 1f, cantDanioPorcentaje: 10f, cantAPMax: 1f, cantTsMental: 2f);
        unidad.SumarValentia(2);
      }

      if (TieneTrait(pers, PersonajeTraitCatalog.TraitBajoPresion))
      {
        unidad.SumarValentia(-2);
      }
    }

    if (TieneTrait(pers, PersonajeTraitCatalog.TraitBrutal))
    {
      AplicarBuffSiNoExiste(unidad, "Brutal", true, -1, cantAtaque: -1f, cantCritDado: 2f, seVeBarraVida: false);
    }

    if (TieneTrait(pers, PersonajeTraitCatalog.TraitClinico))
    {
      AplicarBuffSiNoExiste(unidad, "Clínico", true, -1, cantAtaque: 2f, cantDanioPorcentaje: -10f, seVeBarraVida: false);
    }

    AplicarTraitMiedoTag(pers, unidad, PersonajeTraitCatalog.TraitTemeCorruptos, "Corrupto");
    AplicarTraitMiedoTag(pers, unidad, PersonajeTraitCatalog.TraitTemeNomuertos, "Nomuerto");
    AplicarTraitMiedoTag(pers, unidad, PersonajeTraitCatalog.TraitTemeBestias, "Bestia");
    AplicarTraitMiedoTag(pers, unidad, PersonajeTraitCatalog.TraitTemeCriaturas, "Criatura");
    AplicarTraitMiedoTag(pers, unidad, PersonajeTraitCatalog.TraitTemeDemonios, "Demonio");

    AplicarTraitOdioTag(pers, unidad, PersonajeTraitCatalog.TraitOdiaCorruptos, "Corrupto");
    AplicarTraitOdioTag(pers, unidad, PersonajeTraitCatalog.TraitOdiaNomuertos, "Nomuerto");
    AplicarTraitOdioTag(pers, unidad, PersonajeTraitCatalog.TraitOdiaBestias, "Bestia");
    AplicarTraitOdioTag(pers, unidad, PersonajeTraitCatalog.TraitOdiaCriaturas, "Criatura");
    AplicarTraitOdioTag(pers, unidad, PersonajeTraitCatalog.TraitOdiaDemonios, "Demonio");

    PropagarEspirituSiCorresponde(pers, unidad);
  }
  // (removido) UI de carga sutil

  IEnumerator Start()
  {
    RefrescarUICompartidaSegunEscena();

    if (mantenerFaderNegroEnProximaCargaCampania)
    {
      mantenerFaderNegroEnProximaCargaCampania = false;
      SetFaderHold(true);
      yield break;
    }

    // Fade-in al entrar al menú
    yield return FadeTo(0f, fadeTime);
  }

  public static void SolicitarFaderNegroEnProximaCargaCampania()
  {
    mantenerFaderNegroEnProximaCargaCampania = true;
  }

  public void PlayFadeInOut(float fadeDuration, float holdDuration)
  {
    StartCoroutine(FadeInOut(fadeDuration, holdDuration));
  }

  private IEnumerator FadeInOut(float fadeDuration, float holdDuration)
  {
    // Se oscurece (fade in)
    yield return StartCoroutine(FadeTo(1f, fadeDuration));

    // Mantiene la pantalla negra
    if (holdDuration > 0f)
      yield return new WaitForSecondsRealtime(holdDuration);

    // Se aclara (fade out)
    yield return StartCoroutine(FadeTo(0f, fadeDuration));
  }

  IEnumerator FadeTo(float target, float time)
  {
    if (fader == null) yield break;
    // Si intentan hacer fade-out mientras está bloqueado, esperar hasta liberar
    if (faderHold && target < 1f)
    {
      while (faderHold) { yield return null; }
    }
    float start = fader.alpha;
    float t = 0f;
    while (t < time)
    {
      // Si durante la animación se activa un hold y el destino es fade-out, pausar hasta liberar y reanudar suave
      if (faderHold && target < 1f)
      {
        while (faderHold) { yield return null; }
        start = fader.alpha;
        t = 0f;
      }
      t += Time.unscaledDeltaTime;
      fader.alpha = Mathf.Lerp(start, target, t / time);
      yield return null;
    }
    fader.alpha = target;
  }

  // Expuestos para poder hacer yield desde otras clases (p.ej. AtributosZona)
  public IEnumerator FadeIn(float duration)
  {
    yield return FadeTo(1f, duration);
  }

  public IEnumerator FadeOut(float duration)
  {
    yield return FadeTo(0f, duration);
  }

  // Forzar mantener pantalla negra y bloquear fade-outs hasta liberar
  public void SetFaderHold(bool hold)
  {
    faderHold = hold;
    if (fader != null && hold)
    {
      fader.alpha = 1f;
      fader.blocksRaycasts = true;
      fader.interactable = false;
    }
    else if (fader != null && !hold)
    {
      fader.blocksRaycasts = false;
    }
  }

  // (removido) Loading label y animación de puntos
  public GameObject VFXLluvia;
  public GameObject VFXNieve;
  public GameObject VFXNiebla;
  public GameObject VFXAurora;
  [Header("VFX Aliento Negro (Batalla)")]
  public GameObject VFXAlientoLeve;
  public GameObject VFXAlientoMedio;
  public GameObject VFXAlientoGrande;
  EncounterDefinition encuentroGeneradoActual;
  int ultimoIDEncuentro;
  int tipoEmboscadaActual = 0;
  private bool filtrandoUnicosEnComposicionInicial;
  private readonly HashSet<int> filasAliadasInicialesReservadas = new HashSet<int>();
  private readonly List<int> filasAliadasInicialesSeleccionadas = new List<int>();
  private int totalAliadosInicialesAColocar;

  public void CargarBatalla(int IDEncuentro, int esEmboscada = 0, EncounterDefinition encuentro = null, bool usarRefuerzosAliadosCaravana = false)
  {
    EjecutarOperacionSegura(CargarBatallaAsync(IDEncuentro, esEmboscada, encuentro, usarRefuerzosAliadosCaravana), nameof(CargarBatalla));
  }

  MusicManager.VarianteBatalla DeterminarVarianteMusicalBatalla(int idEncuentro, int esEmboscada, EncounterDefinition encuentro, bool usarRefuerzosAliadosCaravana)
  {
    if (EsCombateEspecialMusical(idEncuentro, encuentro, usarRefuerzosAliadosCaravana))
    {
      return MusicManager.VarianteBatalla.Especial;
    }

    return MusicManager.VarianteBatalla.Normal;
  }

  bool EsCombateEspecialMusical(int idEncuentro, EncounterDefinition encuentro, bool usarRefuerzosAliadosCaravana)
  {
    if (EsCombateFinalODeCaravana(idEncuentro, encuentro, usarRefuerzosAliadosCaravana))
    {
      return true;
    }

    if (idEncuentro < 0 || idEncuentro == 700)
    {
      return true;
    }

    if (encuentro != null && encuentro.battleType == BattleEncounterType.Subterraneo)
    {
      return true;
    }

    if (CampaignManager.Instance != null
      && CampaignManager.Instance.scMapaManager != null
      && CampaignManager.Instance.scMapaManager.nodoActual != null
      && CampaignManager.Instance.scMapaManager.nodoActual.tipoNodo == 15
      && CampaignManager.Instance.scMapaManager.nodoActual.nodoRitual)
    {
      return true;
    }

    return false;
  }

  bool EsCombateFinalODeCaravana(int idEncuentro, EncounterDefinition encuentro, bool usarRefuerzosAliadosCaravana)
  {
    if (usarRefuerzosAliadosCaravana || idEncuentro == 701)
    {
      return true;
    }

    if (encuentro != null && encuentro.battleType == BattleEncounterType.AtaqueCaravana)
    {
      return true;
    }

    return false;
  }

  private async Task CargarBatallaAsync(int IDEncuentro, int esEmboscada = 0, EncounterDefinition encuentro = null, bool usarRefuerzosAliadosCaravana = false)
  {
    if (cargandoBatalla || cerrandoBatalla)
    {
      return;
    }

    cargandoBatalla = true;
    try
    {
    if (!ValidarDependenciasCambioABatalla(out var campaignManager))
    {
      return;
    }
    ultimoIDEncuentro = IDEncuentro;
    encuentroGeneradoActual = encuentro;
    tipoEmboscadaActual = esEmboscada;
    ResetearEstadoTraitsCombateParticipantes();

    SetActiveSiExiste(VFXLluvia, false);
    SetActiveSiExiste(VFXNieve, false);
    SetActiveSiExiste(VFXNiebla, false);
    SetActiveSiExiste(VFXAurora, false);
    RefrescarVfxAlientoNegroBatalla(desactivarTodo: true);

    escenaActual = 1; //1 Batalla 0 Campaña
    campaignManager.CancelarHighlightsNodosCampania();
    campaignManager.logDeCampania?.LimpiarDesdeCampania();
    int idZona = campaignManager.scAtributosZona.ID;
    MusicManager.VarianteBatalla varianteMusical = DeterminarVarianteMusicalBatalla(IDEncuentro, esEmboscada, encuentro, usarRefuerzosAliadosCaravana);
    float fadeTransicion = Mathf.Max(0.05f, fadeEntradaBatalla);
    float holdTransicion = Mathf.Max(0.1f, esperaEntradaBatalla);
    float esperaCambioEscena = CalcularEsperaCambioEscena(fadeTransicion, holdTransicion);
    if (MusicManager.Instance != null)
    {
      MusicManager.Instance.PausarMusica(false);
      MusicManager.Instance.PlayBatalla(idZona, varianteMusical);
    }
    campaignManager.scAdministradorEscenas.PlayFadeInOut(fadeTransicion, holdTransicion);
    await EsperarSegundosRealtime(esperaCambioEscena);
    EscenaCampaign.SetActive(false);
    EscenaBatalla.SetActive(true);
    RefrescarUICompartidaSegunEscena();
    BattleManager battleManager = BattleManager.Instance;
    if (battleManager == null)
    {
      Debug.LogError("[AdministradorEscenas] No se pudo cargar batalla: BattleManager no quedó disponible tras activar la escena.");
      escenaActual = 0;
      SetActiveSiExiste(EscenaBatalla, false);
      SetActiveSiExiste(EscenaCampaign, true);
      MusicManager.Instance?.VolverACampania();
      return;
    }
    battleManager.RefrescarVfxClimaCalor(false);
    battleManager.ConfigurarOrdenIniciativaPorEmboscada(esEmboscada);
    battleManager.SetLogCombateActivoPorEscena(true);
    // Silenciar logs de combate durante la preparación (buffs/estados iniciales)
    battleManager.silenciarLogCombate = true;
    battleManager.ReiniciarEstadoRefuerzos();
    filasAliadasInicialesReservadas.Clear();
    filasAliadasInicialesSeleccionadas.Clear();
    totalAliadosInicialesAColocar = 0;
    if (Personaje1 != null) { totalAliadosInicialesAColocar++; }
    if (Personaje2 != null) { totalAliadosInicialesAColocar++; }
    if (Personaje3 != null) { totalAliadosInicialesAColocar++; }
    if (Personaje4 != null) { totalAliadosInicialesAColocar++; }
    CampaignManager campaignManagerTutorial = CampaignManager.Instance;
    bool tutorialViejoActivo = campaignManagerTutorial != null
      && campaignManagerTutorial.scTutorialManager != null
      && campaignManagerTutorial.scTutorialManager.tutorialActivo;
    bool tutorialLinealForzado = campaignManagerTutorial != null && campaignManagerTutorial.DebeForzarMapaLinealTutorial();
    bool esPrimerCombateTutorial = IDEncuentro == 700 && (tutorialViejoActivo || tutorialLinealForzado);
    bool esCombateFinalTutorial = IDEncuentro == 701 && (tutorialViejoActivo || tutorialLinealForzado);
    bool esEncuentroJefeGulekGul = IDEncuentro == 60;
    if (esPrimerCombateTutorial || esCombateFinalTutorial)
    {
      TutorialDirector.Instance?.SuspendForLegacyCombatTutorial();
    }
    if (esEmboscada == 3) // Debe ocurrir antes de colocar unidades y objetos aleatorios para reservar esas casillas.
    {
      CrearDefensasCaravana();
    }

    if (esPrimerCombateTutorial)
    {
      GenerarObstaculosPrimerCombateTutorial();
    }
    if (esCombateFinalTutorial)
    {
      GenerarObstaculosCombateFinalTutorial();
    }

    AdministrarFondos(IDEncuentro, encuentroGeneradoActual);
    ConfigurarAmbienteVidaBatalla();

    if (Personaje1 != null)
    {
      ColocarPersonajecomoUnidad(Personaje1);
    }
    if (Personaje2 != null)
    {
      ColocarPersonajecomoUnidad(Personaje2);
    }
    if (Personaje3 != null)
    {
      ColocarPersonajecomoUnidad(Personaje3);
    }
    if (Personaje4 != null)
    {
      ColocarPersonajecomoUnidad(Personaje4);
    }

    //Inicializa ComienzoBatallaClase una vez asignados ya todos los aliados
    if (unidadPers1 != null)
    {
      unidadPers1.ComienzoBatallaClase();
    }
    if (unidadPers2 != null)
    {
      unidadPers2.ComienzoBatallaClase();
    }
    if (unidadPers3 != null)
    {
      unidadPers3.ComienzoBatallaClase();
    }
    if (unidadPers4 != null)
    {
      unidadPers4.ComienzoBatallaClase();
    }

    // Solo en la primera pelea del tutorial, mover al Explorador a (x1,y3) y aplicar su bonificador inicial
    if (esPrimerCombateTutorial)
    {
      Unidad explorador = null;
      if (unidadPers1 != null && unidadPers1.GetComponent<ClaseAcechador>() != null) explorador = unidadPers1;
      else if (unidadPers2 != null && unidadPers2.GetComponent<ClaseAcechador>() != null) explorador = unidadPers2;
      else if (unidadPers3 != null && unidadPers3.GetComponent<ClaseAcechador>() != null) explorador = unidadPers3;
      else if (unidadPers4 != null && unidadPers4.GetComponent<ClaseAcechador>() != null) explorador = unidadPers4;

      if (explorador != null)
      {
        // Limpiar casilla original antes de reubicar para evitar que quede marcado en dos casillas
        if (explorador.CasillaPosicion != null)
        {
          explorador.CasillaPosicion.Presente = null;
        }

        explorador.mod_iniciativa += 20; explorador.iniciativa_actual += 20;
        explorador.mod_Ataque += 11;
        explorador.mod_DanioPorcentaje += 51;
        explorador.mod_CriticoDañoBonus += 18;
        explorador.mod_CriticoRangoDado += 20;
        ColocarEnCasillaEspecifica(1, explorador.gameObject, 2, 2); // x1,y3 para la primera pelea del tutorial
      }
    }

    // Solo en la pelea final del tutorial, dar +20 iniciativa al Caballero
    if (esCombateFinalTutorial)
    {
      Unidad caballero = null;
      if (unidadPers1 != null && unidadPers1.GetComponent<ClaseCaballero>() != null) caballero = unidadPers1;
      else if (unidadPers2 != null && unidadPers2.GetComponent<ClaseCaballero>() != null) caballero = unidadPers2;
      else if (unidadPers3 != null && unidadPers3.GetComponent<ClaseCaballero>() != null) caballero = unidadPers3;
      else if (unidadPers4 != null && unidadPers4.GetComponent<ClaseCaballero>() != null) caballero = unidadPers4;

      if (caballero != null)
      {
        caballero.mod_iniciativa += 20;
        caballero.iniciativa_actual += 20;
      }
    }

    CrearEncuentroEnemigos(IDEncuentro, encuentroGeneradoActual);

    // Actualizar listas de unidades tras colocar aliados y enemigos
    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.ladoA.ActualizarListaDeUnidadesEnLado();
      BattleManager.Instance.ladoB.ActualizarListaDeUnidadesEnLado();
      AplicarAjusteValentiaInicialPorEsperanza();
    }




    //Si es emboscada, se le da la iniciativa a los enemigos
    if (esEmboscada == 1)
    {
      LadoManager ladoEnemigo2 = BattleManager.Instance.ladoA; //Enemigos
      foreach (Unidad u in ladoEnemigo2.unidadesLado)
      {
        if (u == null) { continue; }
        u.mod_iniciativa += 3; //Aumenta la iniciativa de los enemigos
      }


      Personaje ObtenerPersonajeAliado(Unidad unidad)
      {
        if (unidad == unidadPers1) return Personaje1;
        if (unidad == unidadPers2) return Personaje2;
        if (unidad == unidadPers3) return Personaje3;
        if (unidad == unidadPers4) return Personaje4;
        return null;
      }

      bool EstaEnGuardiaOVigilanciaCaballero(Personaje personaje)
      {
        return EsActividadGuardiaBaseOCaballero(personaje);
      }

      LadoManager ladoBueno = BattleManager.Instance.ladoB; //Buenos
      foreach (Unidad u in ladoBueno.unidadesLado)
      {
        if (u == null) { continue; }
        //Saca Escondido para los que arrancan combate escondidos
        u.PerderEscondido(); //Remueve el estado escondido de la unidad

        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Personaje personajeAliado = ObtenerPersonajeAliado(u);
        if (EstaEnGuardiaOVigilanciaCaballero(personajeAliado))
        {
          continue;
        }

        if (!PuedeSerSorprendidoPorEmboscada(personajeAliado))
        {
          continue;
        }

        Buff buff = new Buff();
        buff.buffNombre = "Sorprendido";
        buff.forzarTextoFlotanteInicioCombate = true;
        buff.ocultarEnBarraVida = false;
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 1;
        buff.cantIniciativa -= 3;
        buff.cantAPMax -= 2;
        buff.cantDefensa -= 2;
        buff.AplicarBuff(u);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }

    }
    else if (esEmboscada == 2)
    {
      // Emboscada a favor de la caravana: los enemigos inician sorprendidos
      LadoManager ladoEnemigoEmboscada = BattleManager.Instance.ladoA;
      foreach (Unidad u in ladoEnemigoEmboscada.unidadesLado)
      {
        if (u == null) { continue; }
        AplicarSorprendidoInicioCombate(u);
      }
    }

    if (esEmboscada == 3)//Ataque a Caravana
    {
      // Evitar que los personajes forzados aparezcan tambin como refuerzos: marcar temporalmente como "Guardia"
      List<System.Tuple<Personaje, int>> actividadesPrevias = new List<System.Tuple<Personaje, int>>();
      if (PersonajesSorprendidosInicioCaravana != null && PersonajesSorprendidosInicioCaravana.Count > 0)
      {
        foreach (var p in PersonajesSorprendidosInicioCaravana)
        {
          if (p == null) continue;
          actividadesPrevias.Add(new System.Tuple<Personaje, int>(p, p.ActividadSeleccionada));
          p.ActividadSeleccionada = 3; // Guardia
        }
      }

      AsignarRefuerzosAliados();

      // Restaurar actividades previas
      if (actividadesPrevias.Count > 0)
      {
        foreach (var parAct in actividadesPrevias)
        {
          if (parAct.Item1 != null) parAct.Item1.ActividadSeleccionada = parAct.Item2;
        }
      }

      // Acechador en "Vigilar Desde las Sombras": inicia oculto en Ataque a Caravana.
      AplicarSigiloInicialCaravana(Personaje1, unidadPers1);
      AplicarSigiloInicialCaravana(Personaje2, unidadPers2);
      AplicarSigiloInicialCaravana(Personaje3, unidadPers3);
      AplicarSigiloInicialCaravana(Personaje4, unidadPers4);

      // Aplicar "Sorprendido" solo a los que fueron forzados a desplegar
      if (PersonajesSorprendidosInicioCaravana != null && PersonajesSorprendidosInicioCaravana.Count > 0)
      {
        // Aplicar a cada uno de los 4 posibles participantes si corresponde
        void AplicarSorprendidoSiCorresponde(Personaje pers, Unidad uni)
        {
          if (pers == null || uni == null) return;
          if (EsActividadGuardiaParaCaravana(pers)) return;
          if (!PuedeSerSorprendidoPorEmboscada(pers)) return;
          if (!PersonajesSorprendidosInicioCaravana.Contains(pers)) return;

          // Remover ocultamiento si lo tuviera
          uni.PerderEscondido();

          // Debuff Sorprendido: mismos efectos que emboscada
          Buff buff = new Buff();
          buff.buffNombre = "Sorprendido";
          buff.forzarTextoFlotanteInicioCombate = true;
          buff.ocultarEnBarraVida = false;
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = 1;
          buff.cantIniciativa -= 5;
          buff.cantAPMax -= 1;
          buff.cantDefensa -= 1;
          buff.AplicarBuff(uni);
          Buff buffComponent = ComponentCopier.CopyComponent(buff, uni.gameObject);
        }

        AplicarSorprendidoSiCorresponde(Personaje1, unidadPers1);
        AplicarSorprendidoSiCorresponde(Personaje2, unidadPers2);
        AplicarSorprendidoSiCorresponde(Personaje3, unidadPers3);
        AplicarSorprendidoSiCorresponde(Personaje4, unidadPers4);

        // Limpiar la lista para evitar reutilizar en próximos combates
        PersonajesSorprendidosInicioCaravana.Clear();
      }
    }
    else if (usarRefuerzosAliadosCaravana)
    {
      AsignarRefuerzosAliados(filtrarPorActividadGuardia: false);
    }

    if (esEmboscada == 0 && !usarRefuerzosAliadosCaravana)
    {
      int tierCuartel = MetaprogresionManager.Instance.SerriaTierCuartel;
      if (UnityEngine.Random.Range(0,100) < tierCuartel * 10) 
      {
        AsignarRefuerzosCuartel();

      }

    }

    AplicarRasgosInicioCombate(Personaje1, unidadPers1);
    AplicarRasgosInicioCombate(Personaje2, unidadPers2);
    AplicarRasgosInicioCombate(Personaje3, unidadPers3);
    AplicarRasgosInicioCombate(Personaje4, unidadPers4);
    RefrescarTraitsComposicionAliada();
    //Efectos ambientales de Batalla
      #region Clima en Batalla

      // Refrescar listas de unidades antes de aplicar clima
      if (BattleManager.Instance != null)
      {
        BattleManager.Instance.ladoA.ActualizarListaDeUnidadesEnLado();
        BattleManager.Instance.ladoB.ActualizarListaDeUnidadesEnLado();
      }

    int climaCampaña = CampaignManager.Instance.intTipoClima;
    if (EsEncuentroSubterraneo(IDEncuentro)) { climaCampaña = 0; }//Si es encuentro subterraneo no hay clima

    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.SetClimaUIVisible(climaCampaña >= 1 && climaCampaña <= 5);
    }

    if (climaCampaña == 1)//Normal (soleado)
    {
      //--- 
      BattleManager.Instance.RefrescarVfxClimaCalor(false);
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_sol;
    }
    if (climaCampaña == 2)//Calor
    {
      BattleManager.Instance.RefrescarVfxClimaCalor(true);
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      listaUnidades.AddRange(BattleManager.Instance.ladoA.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (u == null) { continue; }
        //---
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Acalorado";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.suprimeTextoFlotante = true; // no mostrar texto flotante al iniciar combate
        buff.cantResHie += 2;
        buff.percResFue -= 3;
        buff.AplicarBuff(u, null, false); //OJO, el false es para que NO sea removible por habilidades
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }

      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_calor;

    }
    if (climaCampaña == 3)//Lluvia
    {
      BattleManager.Instance.RefrescarVfxClimaCalor(false);
      VFXLluvia.SetActive(true);
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      listaUnidades.AddRange(BattleManager.Instance.ladoA.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (u == null) { continue; }
        //---
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Mojado";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.suprimeTextoFlotante = true; // no mostrar texto flotante al iniciar combate
        buff.cantResHie -= 2;
        buff.cantResRay -= 4;
        buff.cantResFue += 3;
        buff.AplicarBuff(u, null, false);//OJO, el false es para que NO sea removible por habilidades
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }
      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_lluvia;
    }
    if (climaCampaña == 4)//Nieve
    {
      BattleManager.Instance.RefrescarVfxClimaCalor(false);
      VFXNieve.SetActive(true);
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      listaUnidades.AddRange(BattleManager.Instance.ladoA.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (u == null) { continue; }
        //---
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Frío";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = -1;
        buff.suprimeTextoFlotante = true; // no mostrar texto flotante al iniciar combate
        buff.cantResHie -= 3;
        buff.cantAPMax -= 1;
        buff.cantResFue += 2;
        buff.AplicarBuff(u, null, false);//OJO, el false es para que NO sea removible por habilidades
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }

      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_nieve;
    }
    if (climaCampaña == 5)//Niebla
    {
      BattleManager.Instance.RefrescarVfxClimaCalor(false);
      VFXNiebla.SetActive(true);
      //---
      BattleManager.Instance.widgetClima.sprite = CampaignManager.Instance.clima_niebla;
    }
    if (climaCampaña == 7)//Aurora Boreal
    {
      BattleManager.Instance.RefrescarVfxClimaCalor(false);
      if (CampaignManager.Instance.scAtributosZona.ID == 2) //Paso Viento Helado
      { VFXAurora.SetActive(true); }

    }
    RefrescarVfxAlientoNegroBatalla();
    #endregion
    #region Corrupcion
    LadoManager ladodeEnemigo = BattleManager.Instance.ladoA; //Enemigos
    foreach (Unidad u in ladodeEnemigo.unidadesLado)
    {
      if (u == null) { continue; }
      int tierAlientoNegro = (int)CampaignManager.Instance.GetTierAlientoNegro();

      if (u.TieneTag("Corrupto"))
      {
        if (tierAlientoNegro < 2)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Débil";
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = -1;
          buff.cantDanioPorcentaje -= 5;
          buff.cantAtaque -= 1;
          buff.percTsMental -= 1;
          buff.percTsFortaleza -= 1;
          buff.cantResNec -= 5;
          buff.AplicarBuff(u);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
        else if (tierAlientoNegro == 2)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Presente";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDamBonusElementalNec += 4;
          buff.percTsMental += 1;
          buff.percHPMax += 5;
          buff.percTsFortaleza += 1;
          u.estado_regeneravida = 1;
          buff.AplicarBuff(u);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
        else if (tierAlientoNegro == 3)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Fuerte";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDamBonusElementalNec += 6;
          buff.cantAtaque += 1;
          buff.percTsMental += 2;
          buff.percHPMax += 10;
          buff.percTsFortaleza += 2;
          u.estado_regeneravida = 3;
          buff.AplicarBuff(u);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
        else if (tierAlientoNegro > 3)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Aliento Negro: Empoderante";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDamBonusElementalNec += 8;
          buff.cantAtaque += 2;
          buff.percTsMental += 3;
          buff.percHPMax += 15;
          buff.percTsFortaleza += 3;
          buff.AplicarBuff(u);
          u.estado_regeneravida = 5;
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
      }
    }
    #endregion
    #region Noche
    int randomNoche = UnityEngine.Random.Range(1, 101); //1-100
    if (EsEncuentroSubterraneo(IDEncuentro)) { randomNoche = 0; }//Si es encuentro subterraneo siempre es "noche"

    if (randomNoche <= 25) //probabilidad noche 
    {
      BattleManager.Instance.nocheLienzo.SetActive(true);
      List<Unidad> listaUnidades = new List<Unidad>(BattleManager.Instance.ladoB.unidadesLado);
      foreach (Unidad u in listaUnidades)
      {
        if (!(u is ClaseAcechador) && !u.inmunidad_Oscuridad) //El acechador y criaturas inmunes no reciben debuff
        {
          //---
          //BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Oscuridad";
          buff.boolfDebufftBuff = false;
          buff.DuracionBuffRondas = -1;
          buff.cantTsMental -= 2;
          buff.cantAtaque -= 1;
          buff.cantResNec -= 2;
          buff.AplicarBuff(u, null, false);//OJO, el false es para que NO sea removible por habilidades
                                           // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
      }

    }
    else
    {
      BattleManager.Instance.nocheLienzo.SetActive(false);
    }
    #endregion
    #region Ritual Kale'Tav
    LadoManager ladodeEnemigos = BattleManager.Instance.ladoA; //Enemigos
    foreach (Unidad u in ladodeEnemigos.unidadesLado)
    {
      int repeticiones = CampaignManager.Instance.scAtributosZona.PasoVientoHelado_FuerzaKaleTav;
      for (int i = 0; i < repeticiones; i++)
      {
        if (u.TieneTag("Kale'Tav") && CampaignManager.Instance.scAtributosZona.ID == 2)
        {
          // BUFF ---- Así se aplica un buff/debuff
          Buff buff = new Buff();
          buff.buffNombre = "Fuerza Kale'Tav";
          buff.boolfDebufftBuff = true;
          buff.DuracionBuffRondas = -1;
          buff.cantDanioPorcentaje += 10;
          buff.cantHPMax += 10;
          buff.cantTsMental += 1;
          buff.cantTsFortaleza += 1;
          buff.AplicarBuff(u);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
        }
      }
    }
    #endregion
    #region Zarkil Masacre
    LadoManager ladodeEnemigos2 = BattleManager.Instance.ladoA; //Enemigos
    foreach (Unidad u in ladodeEnemigos2.unidadesLado)
    {
      if (u.TieneTag("Zarkil") && CampaignManager.Instance.scAtributosZona.ID == 3 && CampaignManager.Instance.intTipoClima == 9)
      {
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Masacre Zarkil";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = -1;
        buff.cantCritDado += 2;
        buff.percCritDaño += 20;
        buff.AplicarBuff(u);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }
    }
    #endregion
    #region Objetos en Batalla
    bool nodoIncendiadoActual = CampaignManager.Instance.scMapaManager.nodoActual.nodoIncendiado;

    //Fuego de Bosque Ardiente
    if (nodoIncendiadoActual)
    {
      LadoManager ladodeEnemigos3 = BattleManager.Instance.ladoA; //Enemigos
      foreach (Unidad u in ladodeEnemigos3.unidadesLado)
      {
        // BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Enfurecido por el Fuego";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = -1;
        buff.cantDanioPorcentaje += 15;
        buff.cantAtaque += 2;
        buff.cantDefensa -= 2;
        buff.AplicarBuff(u);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, u.gameObject);
      }
    }

    //Obstaculos genericos
    if (ProbabilidadPorcentual(70))
    {
      if (!esPrimerCombateTutorial && !esCombateFinalTutorial && !esEncuentroJefeGulekGul) //Si no es tutorial
      {
        GenerarObstaculosGenericos();
      }

    }

    if (nodoIncendiadoActual)
    {
      GenerarFuegoNodoIncendiado();
    }

    #endregion



    //Forzar enemigos a tirar iniciativa
    LadoManager ladoEnemigo = BattleManager.Instance.ladoA; //Enemigos
    foreach (Unidad u in ladoEnemigo.unidadesLado)
    {
      u.HP_actual = u.mod_maxHP;
      u.TirarIniciativa();
      u.ComienzoBatallaEnemigo();
    }
    // El inicio de la primera ronda y el primer turno ya los gestiona BattleManager.RondaNueva()
    // Asegurar que la primera ronda comience en 1.
    // RondaNueva() incrementa el contador, por eso se inicializa en 0 aquí.
    BattleManager.Instance.RondaNro = 0;
    if (esCombateFinalTutorial && BattleManager.Instance.scTutorialCombate != null)
    {
      BattleManager.Instance.scTutorialCombate.IniciarCombateDesdePasoNombre("Paso11", 12, true);
    }

    BattleManager.Instance.RondaNueva();
    TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.BattleStarted, gameObject)
      .Add("encounterId", IDEncuentro)
      .Add("isFirstTutorialBattle", esPrimerCombateTutorial ? 1 : 0)
      .Add("isFinalTutorialBattle", esCombateFinalTutorial ? 1 : 0));

    if (BattleManager.Instance != null && BattleManager.Instance.scTutorialCombate != null)
    {
      if (esPrimerCombateTutorial)
      {
        BattleManager.Instance.scTutorialCombate.IniciarCombateDesdePaso(0, true);
      }
    }
    BattleManager.Instance.SetLogCombateActivoPorEscena(true);

    Invoke("ColocarunidadesEnCanvasUnidades", 0.3f);

    bool modorapido = PlayerPrefs.GetInt("modoRapido", 0) == 1;
    if (modorapido)
    {
      BattleManager.Instance.ActivarModoRapido(true);
    }
    else
    {
      BattleManager.Instance.ActivarModoRapido(false);
    }
    }
    finally
    {
      cargandoBatalla = false;
    }
  }

  void GenerarObstaculosGenericos()
  {
    BattleManager battleManager = BattleManager.Instance;
    if (battleManager == null)
    {
      Debug.LogWarning("[AdministradorEscenas] No se pudo generar obstaculos: BattleManager no inicializado.");
      return;
    }

    LadoManager ladoEnemigo = battleManager.ladoA;
    LadoManager ladoAliado = battleManager.ladoB;

    if (ladoEnemigo == null || ladoAliado == null)
    {
      Debug.LogWarning("[AdministradorEscenas] No se pudo generar obstaculos: lados sin configurar.");
      return;
    }

    if (ContenedorPrefabsBatalla == null)
    {
      Debug.LogWarning("[AdministradorEscenas] No se pudo generar obstaculos: contenedor de prefabs nulo.");
      return;
    }

    if (ProbabilidadPorcentual(500))
    {
      int cantidadrocamenor = 2 + UnityEngine.Random.Range(0, 3);
      ColocarObstaculos(ContenedorPrefabsBatalla.Roca1, cantidadrocamenor, ladoAliado, ladoEnemigo);
    }

    if (ProbabilidadPorcentual(500))
    {
      int cantidadRocaMayor = 2 + UnityEngine.Random.Range(0, 3);
      ColocarObstaculos(ContenedorPrefabsBatalla.Roca2, cantidadRocaMayor, ladoAliado, ladoEnemigo);
    }

    int chancesBarro = 30;
    bool climaLluvioso = CampaignManager.Instance != null && CampaignManager.Instance.intTipoClima == 3;
    if (climaLluvioso) { chancesBarro = Mathf.Min(chancesBarro + 50, 100); }

    if (ProbabilidadPorcentual(chancesBarro))
    {
      int cantidadBarro = UnityEngine.Random.Range(0, 3);
      if (climaLluvioso) { cantidadBarro += 4; }
      ColocarTrampasDeBarro(cantidadBarro, ladoAliado, ladoEnemigo);
    }
  }

  void GenerarObstaculosPrimerCombateTutorial()
  {
    if (ContenedorPrefabsBatalla == null)
    {
      Debug.LogWarning("[AdministradorEscenas] No se pudieron generar obstaculos del tutorial: contenedor de prefabs nulo.");
      return;
    }

    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca1, 1, 3, 1);
    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca1, 1, 1, 5);
    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 1, 2, 4);
    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 2, 2, 5);
    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 2, 3, 1);
  }

  void GenerarObstaculosCombateFinalTutorial()
  {
    if (ContenedorPrefabsBatalla == null)
    {
      Debug.LogWarning("[AdministradorEscenas] No se pudieron generar obstaculos del tutorial final: contenedor de prefabs nulo.");
      return;
    }

    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca1, 2, 3, 5);
    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca1, 2, 2, 4);
    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 2, 3, 1);
    ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 1, 2, 4);
  }

  void ColocarObstaculoFijoTutorial(GameObject prefab, int iLado, int x, int y)
  {
    if (prefab == null || BattleManager.Instance == null)
    {
      Debug.LogWarning("[AdministradorEscenas] No se pudo colocar obstaculo fijo del tutorial: prefab o BattleManager nulo.");
      return;
    }

    LadoManager lado = iLado == 1 ? BattleManager.Instance.ladoB : BattleManager.Instance.ladoA;
    Casilla casillaObjetivo = lado != null ? lado.ObtenerCasillaPorIndex(x, y) : null;
    if (!CasillaEstaDisponibleParaSpawn(casillaObjetivo))
    {
      Debug.LogWarning("[AdministradorEscenas] No se pudo colocar obstaculo fijo del tutorial en x" + x + " y" + y + ".");
      return;
    }

    GameObject obstaculo = Instantiate(prefab);
    if (!lado.ColocarEnCasilla(obstaculo, x, y))
    {
      Destroy(obstaculo);
    }
  }

  void GenerarFuegoNodoIncendiado()
  {
    if (BattleManager.Instance == null)
    {
      return;
    }

    int cantidadTrampasPorLado = UnityEngine.Random.Range(1, 5) + 1;
    ColocarTrampasDeFuegoNodoIncendiado(cantidadTrampasPorLado, BattleManager.Instance.ladoA);
    ColocarTrampasDeFuegoNodoIncendiado(cantidadTrampasPorLado, BattleManager.Instance.ladoB);
  }

  void ColocarTrampasDeFuegoNodoIncendiado(int cantidad, LadoManager lado)
  {
    if (cantidad <= 0 || lado == null)
    {
      return;
    }

    lado.ActualizarListaDeCasillasEnLado();
    List<Casilla> casillasDisponibles = new List<Casilla>();
    foreach (Casilla casilla in lado.casillasLado)
    {
      if (!CasillaEstaDisponibleParaSpawn(casilla))
      {
        continue;
      }

      casillasDisponibles.Add(casilla);
    }

    int cantidadAColocar = Mathf.Min(cantidad, casillasDisponibles.Count);
    for (int i = 0; i < cantidadAColocar; i++)
    {
      int indiceCasilla = UnityEngine.Random.Range(0, casillasDisponibles.Count);
      Casilla casilla = casillasDisponibles[indiceCasilla];
      casillasDisponibles.RemoveAt(indiceCasilla);

      TrampaFuego trampa = casilla.gameObject.AddComponent<TrampaFuego>();
      trampa.Inicializar();
    }
  }

  void ColocarObstaculos(GameObject prefab, int cantidad, LadoManager ladoAliado, LadoManager ladoEnemigo)
  {
    const int maxRocasPorLado = 6;

    if (prefab == null || cantidad <= 0)
    {
      return;
    }

    for (int i = 0; i < cantidad; i++)
    {
      int rocasAliadas = ContarRocasEnLado(ladoAliado);
      int rocasEnemigas = ContarRocasEnLado(ladoEnemigo);
      bool aliadoConCupo = rocasAliadas < maxRocasPorLado;
      bool enemigoConCupo = rocasEnemigas < maxRocasPorLado;

      if (!aliadoConCupo && !enemigoConCupo)
      {
        break;
      }

      Casilla casilla = null;
      if (aliadoConCupo && enemigoConCupo)
      {
        bool aliadosPrimero = UnityEngine.Random.Range(0, 2) == 0;
        LadoManager preferido = aliadosPrimero ? ladoAliado : ladoEnemigo;
        LadoManager alternativo = aliadosPrimero ? ladoEnemigo : ladoAliado;
        casilla = ObtenerCasillaAleatoriaDisponibleEnLado(preferido);
        if (casilla == null)
        {
          casilla = ObtenerCasillaAleatoriaDisponibleEnLado(alternativo);
        }
      }
      else if (aliadoConCupo)
      {
        casilla = ObtenerCasillaAleatoriaDisponibleEnLado(ladoAliado);
      }
      else
      {
        casilla = ObtenerCasillaAleatoriaDisponibleEnLado(ladoEnemigo);
      }

      if (casilla == null)
      {
        Debug.LogWarning("[AdministradorEscenas] No hay casillas disponibles para colocar obstaculos.");
        break;
      }

      GameObject obst5 = Instantiate(prefab);
      casilla.PonerObjetoEnCasilla(obst5);
    }
  }

  int ContarRocasEnLado(LadoManager lado)
  {
    if (lado == null)
    {
      return 0;
    }

    lado.ActualizarListaDeCasillasEnLado();
    int cantidad = 0;
    foreach (Casilla casilla in lado.casillasLado)
    {
      if (casilla == null || casilla.Presente == null)
      {
        continue;
      }

      Obstaculo obstaculo = casilla.Presente.GetComponent<Obstaculo>();
      if (obstaculo == null)
      {
        continue;
      }

      if (EsObstaculoRoca(obstaculo))
      {
        cantidad++;
      }
    }

    return cantidad;
  }

  bool EsObstaculoRoca(Obstaculo obstaculo)
  {
    if (obstaculo == null)
    {
      return false;
    }

    if (!string.IsNullOrEmpty(obstaculo.oName) && obstaculo.oName.ToLowerInvariant().Contains("roca"))
    {
      return true;
    }

    string nombreGO = obstaculo.gameObject != null ? obstaculo.gameObject.name : string.Empty;
    return !string.IsNullOrEmpty(nombreGO) && nombreGO.ToLowerInvariant().Contains("roca");
  }

  void ColocarPiedrasVariadasEnLadoEnemigo(int cantidad)
  {
    const int maxRocasPorLado = 6;

    if (cantidad <= 0 || BattleManager.Instance == null || BattleManager.Instance.ladoA == null || ContenedorPrefabsBatalla == null)
    {
      return;
    }

    List<GameObject> prefabsPiedra = new List<GameObject>();
    if (ContenedorPrefabsBatalla.Roca1 != null) { prefabsPiedra.Add(ContenedorPrefabsBatalla.Roca1); }
    if (ContenedorPrefabsBatalla.Roca2 != null) { prefabsPiedra.Add(ContenedorPrefabsBatalla.Roca2); }
    if (prefabsPiedra.Count < 1)
    {
      return;
    }

    for (int i = 0; i < prefabsPiedra.Count; i++)
    {
      int randomIndex = UnityEngine.Random.Range(i, prefabsPiedra.Count);
      GameObject tmp = prefabsPiedra[i];
      prefabsPiedra[i] = prefabsPiedra[randomIndex];
      prefabsPiedra[randomIndex] = tmp;
    }

    LadoManager ladoEnemigo = BattleManager.Instance.ladoA;
    ladoEnemigo.ActualizarListaDeCasillasEnLado();

    List<Casilla> casillasLibres = new List<Casilla>();
    foreach (Casilla casilla in ladoEnemigo.casillasLado)
    {
      if (!CasillaEstaDisponibleParaSpawn(casilla))
      {
        continue;
      }

      casillasLibres.Add(casilla);
    }

    int cupoDisponible = Mathf.Max(0, maxRocasPorLado - ContarRocasEnLado(ladoEnemigo));
    int cantidadAColocar = Mathf.Min(cantidad, Mathf.Min(casillasLibres.Count, cupoDisponible));
    int indicePrefab = 0;
    for (int i = 0; i < cantidadAColocar; i++)
    {
      int indiceCasilla = UnityEngine.Random.Range(0, casillasLibres.Count);
      Casilla casillaElegida = casillasLibres[indiceCasilla];
      casillasLibres.RemoveAt(indiceCasilla);

      GameObject prefabPiedra = prefabsPiedra[indicePrefab];
      indicePrefab = (indicePrefab + 1) % prefabsPiedra.Count;

      GameObject piedra = Instantiate(prefabPiedra);
      casillaElegida.PonerObjetoEnCasilla(piedra);
    }
  }

  void ColocarTrampasDeBarro(int cantidad, LadoManager ladoAliado, LadoManager ladoEnemigo)
  {
    if (cantidad <= 0)
    {
      return;
    }

    for (int i = 0; i < cantidad; i++)
    {
      Casilla casilla = ObtenerCasillaAleatoriaDisponible(ladoAliado, ladoEnemigo);
      if (casilla == null)
      {
        Debug.LogWarning("[AdministradorEscenas] No hay casillas disponibles para colocar Trampa de Barro.");
        break;
      }

      TrampaBarro trampa = casilla.gameObject.AddComponent<TrampaBarro>();
      trampa.Inicializar();
    }
  }

  bool CasillaEstaDisponibleParaSpawn(Casilla casilla)
  {
    // Las trampas de caravana y ambientales viven en la Casilla, no en Presente.
    return casilla != null && casilla.Presente == null && casilla.GetComponent<Trampa>() == null;
  }

  Casilla ObtenerCasillaAleatoriaDisponibleEnLado(LadoManager lado)
  {
    if (lado == null)
    {
      return null;
    }

    lado.ActualizarListaDeCasillasEnLado();
    List<Casilla> casillasDisponibles = new List<Casilla>();

    foreach (Casilla casilla in lado.casillasLado)
    {
      if (CasillaEstaDisponibleParaSpawn(casilla))
      {
        casillasDisponibles.Add(casilla);
      }
    }

    if (casillasDisponibles.Count < 1)
    {
      return null;
    }

    return casillasDisponibles[UnityEngine.Random.Range(0, casillasDisponibles.Count)];
  }

  Casilla ObtenerCasillaAleatoriaDisponible(LadoManager ladoAliado, LadoManager ladoEnemigo)
  {
    bool aliadosPrimero = UnityEngine.Random.Range(0, 2) == 0;
    LadoManager preferido = aliadosPrimero ? ladoAliado : ladoEnemigo;
    LadoManager alternativo = aliadosPrimero ? ladoEnemigo : ladoAliado;

    Casilla casilla = ObtenerCasillaAleatoriaDisponibleEnLado(preferido);
    if (casilla == null && alternativo != null)
    {
      casilla = ObtenerCasillaAleatoriaDisponibleEnLado(alternativo);
    }

    return casilla;
  }

  bool ProbabilidadPorcentual(int porcentaje)
  {
    if (porcentaje <= 0) { return false; }
    if (porcentaje >= 100) { return true; }
    return UnityEngine.Random.Range(0, 100) < porcentaje;
  }

  void AdministrarFondos(int idEncuentro, EncounterDefinition encounterDefinition = null)
  {
    bool esSubterraneo = (encounterDefinition != null && (encounterDefinition.battleType == BattleEncounterType.Subterraneo || encounterDefinition.zoneType == EncounterZoneType.Subterraneo))
       || (idEncuentro > 399 && idEncuentro < 450);
    fondoBatallaSubterraneoActual = esSubterraneo;

    if (esSubterraneo) // Encuentro subterraneo
    {
      zonaVisualBatallaActual = EncounterZoneType.Subterraneo;
      if (listaFondosSubterraneos != null && listaFondosSubterraneos.Count > 0)
      {
        mrFondoBatalla.material = listaFondosSubterraneos[UnityEngine.Random.Range(0, listaFondosSubterraneos.Count)];
      }
      return;
    }

    EncounterZoneType zonaEncuentro;
    if (encounterDefinition != null)
    {
      zonaEncuentro = encounterDefinition.zoneType;
    }
    else
    {
      var atributosZona = CampaignManager.Instance.scAtributosZona;
      zonaEncuentro = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
    }
    zonaVisualBatallaActual = zonaEncuentro;

    switch (zonaEncuentro)
    {
      case EncounterZoneType.PasoVientoHelado:
        zonaVisualBatallaActual = EncounterZoneType.PasoVientoHelado;
        if (listaFondosPasoVientoHelado != null && listaFondosPasoVientoHelado.Count > 0)
        {
          mrFondoBatalla.material = listaFondosPasoVientoHelado[UnityEngine.Random.Range(0, listaFondosPasoVientoHelado.Count)];
        }
        break;
      case EncounterZoneType.Nedukazal:
        zonaVisualBatallaActual = EncounterZoneType.Nedukazal;
        if (listaFondosNedukazal != null && listaFondosNedukazal.Count > 0)
        {
          mrFondoBatalla.material = listaFondosNedukazal[UnityEngine.Random.Range(0, listaFondosNedukazal.Count)];
        }
        break;
      case EncounterZoneType.Generico:
        // Usa la zona activa de campaña si existe, si no, cae al bosque
        if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
        {
          var zonaActiva = CampaignManager.Instance.scAtributosZona.GetZoneTypeById(CampaignManager.Instance.scAtributosZona.ID);
          if (zonaActiva == EncounterZoneType.PasoVientoHelado)
          {
            zonaVisualBatallaActual = EncounterZoneType.PasoVientoHelado;
            if (listaFondosPasoVientoHelado != null && listaFondosPasoVientoHelado.Count > 0)
            {
              mrFondoBatalla.material = listaFondosPasoVientoHelado[UnityEngine.Random.Range(0, listaFondosPasoVientoHelado.Count)];
              break;
            }
          }
          if (zonaActiva == EncounterZoneType.Nedukazal)
          {
            zonaVisualBatallaActual = EncounterZoneType.Nedukazal;
            if (listaFondosNedukazal != null && listaFondosNedukazal.Count > 0)
            {
              mrFondoBatalla.material = listaFondosNedukazal[UnityEngine.Random.Range(0, listaFondosNedukazal.Count)];
              break;
            }
          }

        }
        goto case EncounterZoneType.BosqueAngustiante;
      case EncounterZoneType.BosqueAngustiante:
      default:
        zonaVisualBatallaActual = EncounterZoneType.BosqueAngustiante;
        if (listaFondosBosqueLamentos != null && listaFondosBosqueLamentos.Count > 0)
        {
          mrFondoBatalla.material = listaFondosBosqueLamentos[UnityEngine.Random.Range(0, listaFondosBosqueLamentos.Count)];
        }
        break;
    }

  }

  void ConfigurarAmbienteVidaBatalla()
  {
    if (EscenaBatalla == null)
    {
      return;
    }

    if (battleAmbientLife == null)
    {
      battleAmbientLife = EscenaBatalla.GetComponent<BattleAmbientLife>();
      if (battleAmbientLife == null)
      {
        battleAmbientLife = EscenaBatalla.AddComponent<BattleAmbientLife>();
      }
    }

    battleAmbientLife.Configurar(mrFondoBatalla, zonaVisualBatallaActual, fondoBatallaSubterraneoActual);
  }
  public void ColocarPersonajecomoUnidad(Personaje pers, int indexRefuezo = 0)
  {

    GameObject prefabClase = null;
    switch (pers.IDClase)
    {
      case 1: prefabClase = BattleManager.Instance.prefabUnidadCaballero; break;
      case 2: prefabClase = BattleManager.Instance.prefabUnidadExplorador; break;
      case 3: prefabClase = BattleManager.Instance.prefabUnidadPurificadora; break;
      case 4: prefabClase = BattleManager.Instance.prefabUnidadAcechador; break;
      case 5: prefabClase = BattleManager.Instance.prefabUnidadCanalizador; break;
      case 6: prefabClase = BattleManager.Instance.prefabUnidadDuelista; break;
        //-----
    }

    if (prefabClase == null)
    {
      prefabClase = BattleManager.Instance.prefabUnidad;
      if (prefabClase == null)
      {
        Debug.LogError($"[AdministradorEscenas] No hay prefab de batalla configurado para la clase {pers.IDClase}.");
        return;
      }

      Debug.LogWarning($"[AdministradorEscenas] Falta prefab específico para la clase {pers.IDClase}. Se usará el prefab genérico.");
    }

   
    GameObject persUnidad = Instantiate(prefabClase);
    Unidad scUnidadPrefabClase = persUnidad.GetComponent<Unidad>();
    if (CampaignManager.Instance != null)
    {
      CampaignManager.Instance.SincronizarAparienciaVisualPersonaje(pers);
    }
    if (scUnidadPrefabClase != null)
    {
      scUnidadPrefabClase.AplicarAparienciaAlternativaPorIndice(pers.indiceAparienciaAlternativa);
    }



    CampaignManager.Instance.scMenuPersonajes.scEquipo.ActualizarEquipo(pers);
    Equipo sEquipo = CampaignManager.Instance.scMenuPersonajes.scEquipo;
    sEquipo.ActualizarEquipo(pers); //Actualiza el equipo del personaje, para que tenga los buffs de la campaña

    int indexRetrato = pers.idRetrato;
    string stNombre = pers.sNombre;
    float maxHP = pers.fVidaMaxima + sEquipo.BuffTOTALEQUIPOhpMax;
    float fIniciativa = pers.iIniciativa + sEquipo.BuffTOTALEQUIPOIniciativa;
    float fApMax = pers.iApMax + sEquipo.BuffTOTALEQUIPOApMax;
    float fValMax = pers.iValMax + sEquipo.BuffTOTALEQUIPOValMax;
    float fFuerza = pers.iFuerza + sEquipo.BuffTOTALEQUIPOFuerza;
    float fAgilidad = pers.iAgi + sEquipo.BuffTOTALEQUIPOAgi;
    float fPoder = pers.iPoder + sEquipo.BuffTOTALEQUIPOPoder;
    float fArmadura = pers.iArmadura + sEquipo.BuffTOTALEQUIPOArmadura;
    int bonusResElementalPorPoder = pers.ObtenerBonusResElementalPorPoder(sEquipo.BuffTOTALEQUIPOPoder);
    float resNecro = pers.iResNecro + sEquipo.BuffTOTALEQUIPOResNecro;
    float ResDivino = pers.iResDivino + sEquipo.BuffTOTALEQUIPOResDivino;
    float resArcano = pers.iResArcano + sEquipo.BuffTOTALEQUIPOResArcano + bonusResElementalPorPoder;
    float resFuego = pers.iResFuego + sEquipo.BuffTOTALEQUIPOResFuego + bonusResElementalPorPoder;
    float resHielo = pers.iResHielo + sEquipo.BuffTOTALEQUIPOResHielo + bonusResElementalPorPoder;
    float resRayo = pers.iResRayo + sEquipo.BuffTOTALEQUIPOResRayo + bonusResElementalPorPoder;
    float resAcido = pers.iResAcido + sEquipo.BuffTOTALEQUIPOResAcido + bonusResElementalPorPoder;
    float fDefensa = pers.ObtenerDefensaTotalConAgilidad(sEquipo.BuffTOTALEQUIPODefensa, sEquipo.BuffTOTALEQUIPOAgi);
    float fCritRango = pers.fCritRango;
    float fCritDanio = pers.fCritDanio;
    float fAtaque = pers.fBonusAtaque;
    float fTSReflejos = pers.iTSReflejo + sEquipo.BuffTOTALEQUIPOTSReflejo;
    float fTSFortaleza = pers.iTSFortaleza + sEquipo.BuffTOTALEQUIPOTSFortaleza;
    float fTSMental = pers.iTSMental + sEquipo.BuffTOTALEQUIPOTSMental;


    persUnidad.GetComponent<Unidad>().CrearUnidad(indexRetrato, stNombre, maxHP, fIniciativa, fApMax, fValMax, fFuerza, fAgilidad, fPoder, fArmadura, resNecro, resArcano, resFuego, resHielo, resRayo, fDefensa, fCritRango, fCritDanio, fAtaque, fTSReflejos, fTSFortaleza, fTSMental, ResDivino); //Asi se determinan atributos
    if (pers.spRetrato != null)
    {
      persUnidad.GetComponent<Unidad>().uRetrato = pers.spRetrato;
    }



    //Habilidades
    CopiarHabilidades(pers.gameObject, persUnidad);

    //Vida
    var scUnidad = persUnidad.GetComponent<Unidad>();
    float bonusVidaPorFuerza = scUnidad.ObtenerBonusVidaPorFuerzaActual();
    scUnidad.HP_actual = Mathf.Clamp(pers.fVidaActual + bonusVidaPorFuerza, 0f, scUnidad.mod_maxHP);
    scUnidad.EstablecerResistenciaAcidoBase(resAcido);
    scUnidad.ConfigurarDebuffsImpactoArma(pers.itemArma);
    // Sincroniza barra de vida de la unidad con la vida actual al inicio de la batalla
    scUnidad.ActualizarBarraVidaPropia();

    //Consumibles
    persUnidad.GetComponent<Unidad>().ConsumibleA = pers.Consumible1;
    persUnidad.GetComponent<Unidad>().ConsumibleB = pers.Consumible2;


    //Ubicación
    if (indexRefuezo == 0) //Si no es refuerzo, se coloca en una casilla aleatoria
    { ColocarEnCasillaAleatoriaEnColumna(1, pers.iPuestoDeseado, persUnidad); }

    //Estados-Buffs
    AplicarEstadosCampaña(pers, persUnidad);

    //Aplicar efectos de Items específicos
    AplicarEfectosItemsEspecificos(pers, persUnidad);

    if (indexRefuezo > 0) //Si es refuerzo, se coloca en lista
    {
      BattleManager.Instance.aliadosRefuerzos.Add(persUnidad);
    }

    if (Personaje1 == pers)
    {
      unidadPers1 = persUnidad.GetComponent<Unidad>();
    }
    if (Personaje2 == pers)
    {
      unidadPers2 = persUnidad.GetComponent<Unidad>();
    }
    if (Personaje3 == pers)
    {
      unidadPers3 = persUnidad.GetComponent<Unidad>();
    }
    if (Personaje4 == pers)
    {
      unidadPers4 = persUnidad.GetComponent<Unidad>();
    }




  }

  void AplicarAjusteValentiaInicialPorEsperanza()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.ladoB == null || CampaignManager.Instance == null)
    {
      return;
    }

    int esperanzaActual = CampaignManager.Instance.GetEsperanzaActual();
    int ajusteValentia = (esperanzaActual - 50) / 20;
    ajusteValentia += CampaignManager.Instance.ObtenerModificadorValentiaEstadosCaravanaCombateActual();
    if (ajusteValentia == 0)
    {
      return;
    }

    foreach (Unidad aliado in BattleManager.Instance.ladoB.unidadesLado)
    {
      if (aliado == null || aliado.GetComponent<IAUnidad>() != null)
      {
        continue;
      }

      aliado.AjustarValentiaInicialSinLog(ajusteValentia);
    }
  }

  public Personaje ObtenerPersonajeDesdeUnidad(Unidad unidad)
  {
    if (unidad == null)
    {
      return null;
    }

    if (unidad == unidadPers1) { return Personaje1; }
    if (unidad == unidadPers2) { return Personaje2; }
    if (unidad == unidadPers3) { return Personaje3; }
    if (unidad == unidadPers4) { return Personaje4; }

    if (CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null)
    {
      foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
        if (pers == null) { continue; }
        if (pers.sNombre == unidad.uNombre)
        {
          return pers;
        }
      }
    }

    return null;
  }

  public bool MarcarAvergonzadoDesdeUnidad(Unidad unidad)
  {
    Personaje pers = ObtenerPersonajeDesdeUnidad(unidad);
    if (pers == null)
    {
      return false;
    }

    pers.Camp_Avergonzado = true;
    pers.Camp_Moral = Mathf.Min(pers.Camp_Moral, -2);
    return true;
  }

  public void LimpiarAvergonzadoPorCambioZona()
  {
    if (CampaignManager.Instance == null || CampaignManager.Instance.scMenuPersonajes == null)
    {
      return;
    }

    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers == null)
      {
        continue;
      }

      pers.Camp_Avergonzado = false;
    }
  }

  void CrearDefensasCaravana()
  {
    if (CampaignManager.Instance.mejoraCaravanaDefensas >= 5)
    {
      GameObject barricada = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada, 3, 1);
      GameObject barricada2 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada2, 3, 2);
      GameObject barricada3 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada3, 3, 4);
      GameObject barricada4 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada4, 3, 5);

      //Pinchos
      TrampaPinchos tr1 = BattleManager.Instance.ladoA.c3x2.gameObject.AddComponent<TrampaPinchos>();
      tr1.Inicializar();
      TrampaPinchos tr3 = BattleManager.Instance.ladoA.c3x4.gameObject.AddComponent<TrampaPinchos>();
      tr3.Inicializar();
      TrampaPinchos tr4 = BattleManager.Instance.ladoA.c2x1.gameObject.AddComponent<TrampaPinchos>();
      tr4.Inicializar();
      TrampaPinchos tr5 = BattleManager.Instance.ladoA.c2x5.gameObject.AddComponent<TrampaPinchos>();
      tr5.Inicializar();
      TrampaPinchos tr6 = BattleManager.Instance.ladoA.c3x3.gameObject.AddComponent<TrampaPinchos>();
      tr6.Inicializar();


      //Nido
      TrampaNidoDefensivo tr7 = BattleManager.Instance.ladoB.c1x2.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr7.Inicializar();
      TrampaNidoDefensivo tr8 = BattleManager.Instance.ladoB.c1x4.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr8.Inicializar();

    }
    else if (CampaignManager.Instance.mejoraCaravanaDefensas >= 3)
    {
      GameObject barricada = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada, 3, 1);
      GameObject barricada2 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada2, 3, 3);
      GameObject barricada3 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada3, 3, 5);

      //Pinchos
      TrampaPinchos tr1 = BattleManager.Instance.ladoA.c3x2.gameObject.AddComponent<TrampaPinchos>();
      tr1.Inicializar();
      TrampaPinchos tr3 = BattleManager.Instance.ladoA.c3x4.gameObject.AddComponent<TrampaPinchos>();
      tr3.Inicializar();
      TrampaPinchos tr4 = BattleManager.Instance.ladoA.c2x1.gameObject.AddComponent<TrampaPinchos>();
      tr4.Inicializar();
      TrampaPinchos tr5 = BattleManager.Instance.ladoA.c2x5.gameObject.AddComponent<TrampaPinchos>();
      tr5.Inicializar();

      //Nido
      TrampaNidoDefensivo tr6 = BattleManager.Instance.ladoB.c1x3.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr6.Inicializar();

    }
    else if (CampaignManager.Instance.mejoraCaravanaDefensas >= 2)
    {
      GameObject barricada = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada, 3, 1);
      GameObject barricada3 = Instantiate(ContenedorPrefabsBatalla.Barricada);
      ColocarEnCasillaEspecifica(1, barricada3, 3, 5);

      //Pinchos
      TrampaPinchos tr1 = BattleManager.Instance.ladoA.c3x2.gameObject.AddComponent<TrampaPinchos>();
      tr1.Inicializar();
      TrampaPinchos tr3 = BattleManager.Instance.ladoA.c3x4.gameObject.AddComponent<TrampaPinchos>();
      tr3.Inicializar();

      //Nido
      TrampaNidoDefensivo tr5 = BattleManager.Instance.ladoB.c1x3.gameObject.AddComponent<TrampaNidoDefensivo>();
      tr5.Inicializar();


    }
  }

  void CrearEncuentroDinamico(EncounterDefinition encounterDefinition)
  {
    if (encounterDefinition == null)
    {
      return;
    }

    var battleManager = BattleManager.Instance;
    if (battleManager == null)
    {
      return;
    }

    battleManager.enemigosRefuerzos.Clear();
    battleManager.delayRefuerzo = encounterDefinition.reinforcementDelay > 0
      ? encounterDefinition.reinforcementDelay
      : ObtenerDelayRefuerzosAleatorio();

    foreach (var slot in encounterDefinition.units)
    {
      if (slot == null || slot.prefab == null)
      {
        continue;
      }

      GameObject enemigo = Instantiate(slot.prefab);
      if (slot.spawnAsReinforcement)
      {
        enemigo.SetActive(false);
        battleManager.enemigosRefuerzos.Add(enemigo);
      }
      else
      {
        ColocarEnCasillaAleatoria(2, enemigo);
      }
    }

    battleManager.ActualizarRefuerzosUI();
  }
  bool EsEncuentroSubterraneo(int idEncuentro)
  {
    if (encuentroGeneradoActual != null)
    {
      return encuentroGeneradoActual.battleType == BattleEncounterType.Subterraneo
        || encuentroGeneradoActual.zoneType == EncounterZoneType.Subterraneo;
    }

    return idEncuentro > 399 && idEncuentro < 450;
  }
  public GameObject CanvasUnidades;
  int ObtenerDelayRefuerzosAleatorio()
  {
    return UnityEngine.Random.Range(1, 4);
  }

  void CrearEncuentroEnemigos(int IDEncuentro, EncounterDefinition encounterDefinition = null)  //Agregar en MenuBatallas "EfectosDeBatallaEnCampaña" las recompensas/penalizaciones por ganar/perder de cada encuentro nuevo
  {
    BattleManager.Instance.RondaNro = 1;
    filtrandoUnicosEnComposicionInicial = true;
    try
    {
    if (encounterDefinition != null)
    {
      CrearEncuentroDinamico(encounterDefinition);
      return;
    }
    //OBSOLETOO
    #region Encuentros Bosque Ardiendo ID Entre 1 y 50
    ///FASE 1 - BOSQUE ARDIENDO
    ///
    if (IDEncuentro == 1) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();


    }
    if (IDEncuentro == 2) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);

    }
    if (IDEncuentro == 3) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 4) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo3);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 5) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 6) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 7) // FASE 1 - BOSQUE ARDIENDO
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 8) // FASE 1 - BOSQUE ARDIENDO - ELITE
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboAlfaEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo5);
    }
    if (IDEncuentro == 9) // FASE 1 - BOSQUE ARDIENDO - ELITE
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo5);
    }
    if (IDEncuentro == 10) // FASE 1 - BOSQUE ARDIENDO - ELITE
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 11) // FASE 1 - BOSQUE ARDIENDO - Jefe - Arbol Lamentos
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.JefeArbolLamentos);
      ColocarEnCasillaEspecifica(2, enemigo1, 2, 3);

      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo2, 3, 1);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo3, 3, 2);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo4, 3, 3);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo5, 3, 4);
      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.Raizmaldita);
      ColocarEnCasillaEspecifica(2, enemigo6, 3, 5);

      ColocarPiedrasVariadasEnLadoEnemigo(4);
    }
    if (IDEncuentro == 12) // FASE 1 - BOSQUE ARDIENDO - Jefe - PENDIENTE
    {
      //PENDIENTE
    }
    if (IDEncuentro == 13) // FASE 1 - BOSQUE ARDIENDO - Ataque caravana 1
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.LoboAlfaEspectral);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 14) // FASE 1 - BOSQUE ARDIENDO - Ataque caravana 2
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      ColocarEnCasillaAleatoria(2, enemigo3);


      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.FuegoFatuo);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.DriadaQuemada);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.EspectodelBosque);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    #endregion


    #region Encuentros Paso Viento Helado ID Entre 50 y 100
    ///FASE 1 - Paso Viento Helado
    if (IDEncuentro == 50) // FASE 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo4);

    }
    if (IDEncuentro == 51) // FASE 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 52) // FASE 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 53) // FASE 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 54) // FASE 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Faagdan);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 55) // FASE 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      ColocarEnCasillaAleatoria(2, enemigo4);
      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.PajaroRompehielos);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 56) // FASE 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      ColocarEnCasillaAleatoria(2, enemigo5);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 57) // FASE 1 - Paso Vientohelado - ELITE
    {

      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Faagdan);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Faagdan);
      ColocarEnCasillaAleatoria(2, enemigo2);

    }
    if (IDEncuentro == 58) // FASE 1 - Paso Vientohelado - ELITE
    {

      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo5);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 59) // FASE 1 - Paso Vientohelado - ELITE
    {

      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo5);


      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 60) // FASE 1 - Jefe 1 - Paso Vientohelado
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GulekGul);
      ColocarEnCasillaEspecifica(2, enemigo1, 2, 3);

      ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca1, 2, 3, 1);
      ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 2, 3, 2);
      ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 2, 3, 3);
      ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca2, 2, 3, 4);
      ColocarObstaculoFijoTutorial(ContenedorPrefabsBatalla.Roca1, 2, 3, 5);

    }
    if (IDEncuentro == 61) // FASE 1 - Jefe 2 - Paso Vientohelado - PENDIENTE
    {

      //PENDIENTE

    }
    if (IDEncuentro == 62) // FASE 1 - Paso Vientohelado - Ataque caravana 1
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();


    }
    if (IDEncuentro == 63) //FASE 1 - Paso Vientohelado - Ataque caravana 2
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.GuerreroKaleTav);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.LanceroKaleTav);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.BrujaKaleTav);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.EfigieAnimada);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.CanibalKaleTav);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();


    }
    #endregion



    #region Encuentros Nedukazal ID Entre 100 y 150
    if (IDEncuentro == 100) // FASE 1 - Nedukazal - Jefe Comandante Zarkil
    {
      GameObject jefe = Instantiate(ContenedorPrefabsBatalla.ComandanteZarkil);
      ColocarEnCasillaEspecifica(2, jefe, 1, 3);

      GameObject acechador1 = Instantiate(ContenedorPrefabsBatalla.ZarkilAcechador);
      ColocarEnCasillaAleatoria(2, acechador1);
      GameObject acechador2 = Instantiate(ContenedorPrefabsBatalla.ZarkilAcechador);
      ColocarEnCasillaAleatoria(2, acechador2);
      GameObject guerrero = Instantiate(ContenedorPrefabsBatalla.ZarkilGuerrero);
      ColocarEnCasillaAleatoria(2, guerrero);
    }
    #endregion





    #region //Batallas Subterraneas ID Entre 400 y 450
    if (IDEncuentro == 400) // FASE 1 - Batalla Subterranea  
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 401) // FASE 1 - Batalla Subterranea
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Vagranilo);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.VagraniloMayor);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 402) // FASE 1 - Batalla Subterranea
    {

      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.VagraniloMayor);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.VagraniloMayor);
      ColocarEnCasillaAleatoria(2, enemigo2);

    }

    /*Vacio*/
    if (IDEncuentro == 403) // FASE 2 - Batalla Subterranea
    {

    }
    /*Vacio*/
    if (IDEncuentro == 404) // FASE 2 - Batalla Subterranea
    {

    }
    /*Vacio*/
    if (IDEncuentro == 405) // FASE 2 - Batalla Subterranea
    {

    }

    /*Vacio*/
    if (IDEncuentro == 406) // FASE 3 - Batalla Subterranea
    {

    }
    /*Vacio*/
    if (IDEncuentro == 407) // FASE 3 - Batalla Subterranea
    {

    }
    /*Vacio*/
    if (IDEncuentro == 408) // FASE 3 - Batalla Subterranea
    {

    }
    #endregion

    #region //Batallas Genericas ID Entre 500 y 599
    if (IDEncuentro == 500) // FASE 1 - Batalla Bandidos I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo4);
      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo5);
    }
    if (IDEncuentro == 501) // FASE 1 - Batalla Bandidos II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 502) // FASE 1 - Batalla Bandidos III
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 503) // FASE 1 - Batalla Bandidos IV
    {

      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 504) // FASE 1 - Batalla Bandidos - Ataque a Caravana I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      GameObject enemigo10 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      enemigo10.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo10);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 505) // FASE 1 - Batalla Bandidos - Ataque a Caravana II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.Ladron);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.RufianConMazo);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.PerroAdiestrado);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);


      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 506) // FASE 1 - Batalla Vengadores I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo4);


    }
    if (IDEncuentro == 507) // FASE 1 - Batalla Vengadores II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.AlabarderoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PredicadorDelAlientoKadryn);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 508) // FASE 1 - Batalla Vengadores III
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.AlabarderoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PredicadorDelAlientoKadryn);
      ColocarEnCasillaAleatoria(2, enemigo4);

    }
    if (IDEncuentro == 509) // FASE 1 - Batalla Vengadores IV
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.AlabarderoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.PredicadorDelAlientoKadryn);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
      BattleManager.Instance.ActualizarRefuerzosUI();

    }
    if (IDEncuentro == 510) // FASE 1 - Batalla Bandidos - Ataque a Caravana I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.AlabarderoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.PredicadorDelAlientoKadryn);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.AlabarderoVengadorKadryn);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);




      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 511) // FASE 1 - Batalla Bandidos - Ataque a Caravana I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.PredicadorDelAlientoKadryn);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlabarderoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.AlabarderoVengadorKadryn);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.ArqueroVengadorKadryn);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.SoldadoVengadorKadryn);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.PredicadorDelAlientoKadryn);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);




      BattleManager.Instance.ActualizarRefuerzosUI();
    }



    #endregion

    #region //Batallas Curruptas ID Entre 600 y 700
    if (IDEncuentro == 600) // FASE 1 - Batalla Currupta I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 601) // FASE 1 - Batalla Currupta II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);
    }
    if (IDEncuentro == 602) // FASE 1 - Batalla Currupta III
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo3);
    }
    if (IDEncuentro == 603) // FASE 1 - Batalla Currupta IV
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo4);
    }
    if (IDEncuentro == 604) // FASE 1 - Batalla Currupta V
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo3);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 605) // FASE 1 - Batalla Currupta - Ataque a la Caravana I
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    if (IDEncuentro == 606) // FASE 1 - Batalla Currupta - Ataque a la Caravana II
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo1);
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      ColocarEnCasillaAleatoria(2, enemigo2);
      GameObject enemigo3 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo3);
      GameObject enemigo4 = Instantiate(ContenedorPrefabsBatalla.DevoradorCorrompido);
      ColocarEnCasillaAleatoria(2, enemigo4);

      //Refuerzos
      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = ObtenerDelayRefuerzosAleatorio(); //aca poner el delay a gusto para el evento

      GameObject enemigo5 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo5.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo5);

      GameObject enemigo6 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo6.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo6);

      GameObject enemigo7 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo7.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo7);

      GameObject enemigo8 = Instantiate(ContenedorPrefabsBatalla.GuerreroCorrompido);
      enemigo8.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo8);

      GameObject enemigo9 = Instantiate(ContenedorPrefabsBatalla.AlimaniaCorrompida);
      enemigo9.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(enemigo9);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }


    if (IDEncuentro == 700) // TUTORIAL Batalla 
    {
      /*GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.DEBUGPRUEBA);
      ColocarEnCasillaEspecifica(2,enemigo1.gameObject,1,4); 
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.DEBUGPRUEBA);
      ColocarEnCasillaEspecifica(2,enemigo2.gameObject,3,2);*/
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.RufianConBallesta);
      ColocarEnCasillaEspecifica(2,enemigo1.gameObject,1,4); 
      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.MastinTuto);
      ColocarEnCasillaEspecifica(2,enemigo2.gameObject,3,2); 
     
    }
    if (IDEncuentro == 701) // TUTORIAL Batalla final
    {
      GameObject enemigo1 = Instantiate(ContenedorPrefabsBatalla.BrujaQuemada);
      ColocarEnCasillaEspecifica(2, enemigo1.gameObject, 1, 3);

      GameObject enemigo2 = Instantiate(ContenedorPrefabsBatalla.TreantEspectral);
      ColocarEnCasillaEspecifica(2, enemigo2.gameObject, 2, 2);

     

      BattleManager.Instance.enemigosRefuerzos.Clear();
      BattleManager.Instance.delayRefuerzo = 3;
      BattleManager.Instance.enviarUnRefuerzoEnemigoPorRonda = true;
      BattleManager.Instance.ignorarModificadoresDelayRefuerzosEnemigos = true;

      GameObject refuerzo1 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      refuerzo1.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(refuerzo1);

      GameObject refuerzo2 = Instantiate(ContenedorPrefabsBatalla.LoboEspectral);
      refuerzo2.SetActive(false);
      BattleManager.Instance.enemigosRefuerzos.Add(refuerzo2);

      BattleManager.Instance.ActualizarRefuerzosUI();
    }
    #endregion



    Invoke("ColocarunidadesEnCanvasUnidades", 0.3f);
    }
    finally
    {
      FiltrarRefuerzosUnicosEnCombate();
      if (BattleManager.Instance != null)
      {
        BattleManager.Instance.ActualizarRefuerzosUI();
      }
      filtrandoUnicosEnComposicionInicial = false;
    }
  }

  void ColocarunidadesEnCanvasUnidades()
  {
    GameObject[] unidades = GameObject.FindGameObjectsWithTag("Unidad");
    foreach (GameObject unidad in unidades)
    {
      print("colocando unidad en canvas unidades: " + unidad.name);
      unidad.transform.SetParent(CanvasUnidades.transform, true); // Mantiene la escala
                                                                  // unidad.transform.localPosition = Vector3.zero; // O la posición relativa correcta
                                                                  // unidad.transform.localScale = Vector3.one;     // Asegura escala normal
    }
    GameObject[] obstaculos = GameObject.FindGameObjectsWithTag("Obstaculo");
    foreach (GameObject obstaculo in obstaculos)
    {
      obstaculo.transform.SetParent(CanvasUnidades.transform, true); // Mantiene la escala
                                                                     // obstaculo.transform.localPosition = Vector3.zero; // O la posición relativa correcta
                                                                     // obstaculo.transform.localScale = Vector3.one;     // Asegura escala normal
    }

    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.RefrescarOrdenVisualBatalla();
    }
  }
  void ColocarEnCasillaAleatoria(int iLado, GameObject GO)
  {
    bool IntentarColocar(GameObject unidad, LadoManager ladoObjetivo, List<int> columnasPreferidas)
    {
      int intentos = 0;
      while (intentos < 100)
      {
        int columna = columnasPreferidas == null
          ? UnityEngine.Random.Range(1, 4)
          : columnasPreferidas[UnityEngine.Random.Range(0, columnasPreferidas.Count)];
        int fila = UnityEngine.Random.Range(1, 6);
        Casilla casillaObjetivo = ladoObjetivo.ObtenerCasillaPorIndex(columna, fila);

        if (!CasillaEstaDisponibleParaSpawn(casillaObjetivo))
        {
          intentos++;
          continue;
        }

        if (ladoObjetivo.ColocarEnCasilla(unidad, columna, fila))
        {
          return true;
        }

        intentos++;
      }

      return false;
    }

    if (DebeOmitirPorUnicoEnCombate(iLado, GO))
    {
      Destroy(GO);
      return;
    }

    LadoManager lado = iLado == 1 ? BattleManager.Instance.ladoB : BattleManager.Instance.ladoA;

    List<int> columnasPreferidas = null;
    if (iLado != 1)
    {
      IAUnidad iaUnidad = GO.GetComponent<IAUnidad>();
      if (iaUnidad != null)
      {
        columnasPreferidas = iaUnidad.esRango ? new List<int> { 1 } : new List<int> { 2, 3 };
      }
    }

    bool colocado = IntentarColocar(GO, lado, columnasPreferidas);

    if (!colocado && columnasPreferidas != null)
    {
      colocado = IntentarColocar(GO, lado, new List<int> { 1, 2, 3 });
    }

    if (!colocado)
    {
      Debug.LogError("No se pudo colocar el objeto en ninguna casilla despues de 100 intentos.");
    }
  }
  void ColocarEnCasillaAleatoriaEnColumna(int iLado, int columna, GameObject GO)
  {
    LadoManager lado = null;
    if (iLado == 1)
    {
      lado = BattleManager.Instance.ladoB; //Player
    }
    else
    {
      lado = BattleManager.Instance.ladoA; //Enemigos
    }

    List<int> filasCandidatas = new List<int>();
    for (int fila = 1; fila <= 5; fila++)
    {
      if (iLado == 1 && !filasAliadasInicialesReservadas.Contains(fila))
      {
        if (FilaAliadaMantieneDistribucionValida(fila))
        {
          filasCandidatas.Add(fila);
        }
      }
    }

    if (filasCandidatas.Count < 1)
    {
      for (int fila = 1; fila <= 5; fila++)
      {
        if (iLado != 1 || !filasAliadasInicialesReservadas.Contains(fila))
        {
          filasCandidatas.Add(fila);
        }
      }
    }

    if (filasCandidatas.Count < 1)
    {
      for (int fila = 1; fila <= 5; fila++)
      {
        filasCandidatas.Add(fila);
      }
    }

    int intentos = 0; // Para limitar los intentos de colocar la unidad
    bool colocado = false;

    // columna representa la posX preferida del personaje.
    while (!colocado && intentos < 100) // Limitar los intentos para evitar bucles infinitos
    {
      int indiceFila = UnityEngine.Random.Range(0, filasCandidatas.Count);
      int fila = filasCandidatas[indiceFila];
      int posX = columna;
      int posY = fila;
      Casilla casillaObjetivo = lado.ObtenerCasillaPorIndex(posX, posY);

      if (!CasillaEstaDisponibleParaSpawn(casillaObjetivo))
      {
        filasCandidatas.RemoveAt(indiceFila);
        if (filasCandidatas.Count < 1)
        {
          break;
        }
        intentos++;
        continue;
      }

      if (lado.ColocarEnCasilla(GO, posX, posY))
      {
        if (iLado == 1)
        {
          filasAliadasInicialesReservadas.Add(posY);
          filasAliadasInicialesSeleccionadas.Add(posY);
        }
        colocado = true; // Si fue colocado con Éxito, salir del bucle
      }
      else
      {
        filasCandidatas.RemoveAt(indiceFila);
        if (filasCandidatas.Count < 1)
        {
          break;
        }
      }

      intentos++;
    }

    if (!colocado)
    {
      Debug.LogError("No se pudo colocar el objeto en la columna " + columna + " después de 100 intentos.");
    }
  }

  bool FilaAliadaMantieneDistribucionValida(int filaCandidata)
  {
    if (totalAliadosInicialesAColocar <= 2)
    {
      return true;
    }

    List<int> filasActuales = new List<int>(filasAliadasInicialesSeleccionadas);
    if (filasActuales.Contains(filaCandidata))
    {
      return false;
    }

    filasActuales.Add(filaCandidata);
    int faltantes = totalAliadosInicialesAColocar - filasActuales.Count;
    if (faltantes <= 0)
    {
      return ExisteFilaNoAdyacenteATodas(filasActuales);
    }

    List<int> disponibles = new List<int>();
    for (int fila = 1; fila <= 5; fila++)
    {
      if (!filasActuales.Contains(fila))
      {
        disponibles.Add(fila);
      }
    }

    return ExisteCombinacionFilasValida(filasActuales, disponibles, faltantes);
  }

  bool ExisteCombinacionFilasValida(List<int> elegidas, List<int> disponibles, int faltantes)
  {
    if (faltantes <= 0)
    {
      return ExisteFilaNoAdyacenteATodas(elegidas);
    }

    if (disponibles == null || disponibles.Count < faltantes)
    {
      return false;
    }

    for (int i = 0; i < disponibles.Count; i++)
    {
      int fila = disponibles[i];
      List<int> nuevasElegidas = new List<int>(elegidas) { fila };
      List<int> nuevosDisponibles = new List<int>(disponibles);
      nuevosDisponibles.RemoveAt(i);

      if (ExisteCombinacionFilasValida(nuevasElegidas, nuevosDisponibles, faltantes - 1))
      {
        return true;
      }
    }

    return false;
  }

  bool ExisteFilaNoAdyacenteATodas(List<int> filas)
  {
    if (filas == null || filas.Count < 1)
    {
      return false;
    }

    foreach (int filaBase in filas)
    {
      bool esNoAdyacenteATodas = true;
      foreach (int otraFila in filas)
      {
        if (filaBase == otraFila)
        {
          continue;
        }

        if (Mathf.Abs(filaBase - otraFila) <= 1)
        {
          esNoAdyacenteATodas = false;
          break;
        }
      }

      if (esNoAdyacenteATodas)
      {
        return true;
      }
    }

    return false;
  }

  void ColocarEnCasillaEspecifica(int iLado, GameObject GO, int X, int Y)
  {
    if (DebeOmitirPorUnicoEnCombate(iLado, GO))
    {
      Destroy(GO);
      return;
    }

    LadoManager lado = null;
    if (iLado == 1)
    {
      lado = BattleManager.Instance.ladoB; //Player
    }
    else { lado = BattleManager.Instance.ladoA; } //Enemigos

    Casilla casillaObjetivo = lado.ObtenerCasillaPorIndex(X, Y);
    if (!CasillaEstaDisponibleParaSpawn(casillaObjetivo))
    {
      if (GO != null && GO.GetComponent<IAUnidad>() != null)
      {
        Debug.LogWarning("[AdministradorEscenas] Casilla reservada para defensa u ocupada. Se recoloca enemigo en una casilla valida.");
        ColocarEnCasillaAleatoria(iLado, GO);
        return;
      }

      Debug.LogWarning("[AdministradorEscenas] Casilla reservada para defensa u ocupada. No se coloca el objeto en esa posicion.");
      return;
    }

    lado.ColocarEnCasilla(GO, X, Y);

  }

  private bool DebeOmitirPorUnicoEnCombate(int iLado, GameObject go)
  {
    if (!filtrandoUnicosEnComposicionInicial || iLado == 1 || go == null || BattleManager.Instance == null || BattleManager.Instance.ladoA == null)
    {
      return false;
    }

    IAUnidad iaUnidad = go.GetComponent<IAUnidad>();
    if (iaUnidad == null || !iaUnidad.unicoEnCombate)
    {
      return false;
    }

    string clave = ObtenerClaveUnicoEnCombate(go);
    if (string.IsNullOrEmpty(clave))
    {
      return false;
    }

    BattleManager.Instance.ladoA.ActualizarListaDeUnidadesEnLado();

    foreach (Unidad unidadExistente in BattleManager.Instance.ladoA.unidadesLado)
    {
      if (unidadExistente == null)
      {
        continue;
      }

      if (ObtenerClaveUnicoEnCombate(unidadExistente.gameObject) == clave)
      {
        return true;
      }
    }

    return false;
  }

  private void FiltrarRefuerzosUnicosEnCombate()
  {
    if (BattleManager.Instance == null || BattleManager.Instance.enemigosRefuerzos == null)
    {
      return;
    }

    HashSet<string> clavesRegistradas = new HashSet<string>();

    if (BattleManager.Instance.ladoA != null)
    {
      BattleManager.Instance.ladoA.ActualizarListaDeUnidadesEnLado();
      foreach (Unidad unidadExistente in BattleManager.Instance.ladoA.unidadesLado)
      {
        if (unidadExistente == null)
        {
          continue;
        }

        IAUnidad iaUnidadExistente = unidadExistente.GetComponent<IAUnidad>();
        if (iaUnidadExistente == null || !iaUnidadExistente.unicoEnCombate)
        {
          continue;
        }

        string claveExistente = ObtenerClaveUnicoEnCombate(unidadExistente.gameObject);
        if (!string.IsNullOrEmpty(claveExistente))
        {
          clavesRegistradas.Add(claveExistente);
        }
      }
    }

    for (int i = 0; i < BattleManager.Instance.enemigosRefuerzos.Count;)
    {
      GameObject refuerzo = BattleManager.Instance.enemigosRefuerzos[i];
      if (refuerzo == null)
      {
        BattleManager.Instance.enemigosRefuerzos.RemoveAt(i);
        continue;
      }

      IAUnidad iaRefuerzo = refuerzo.GetComponent<IAUnidad>();
      if (iaRefuerzo == null || !iaRefuerzo.unicoEnCombate)
      {
        i++;
        continue;
      }

      string claveRefuerzo = ObtenerClaveUnicoEnCombate(refuerzo);
      if (string.IsNullOrEmpty(claveRefuerzo))
      {
        i++;
        continue;
      }

      if (!clavesRegistradas.Add(claveRefuerzo))
      {
        Destroy(refuerzo);
        BattleManager.Instance.enemigosRefuerzos.RemoveAt(i);
        continue;
      }

      i++;
    }
  }

  private static string ObtenerClaveUnicoEnCombate(GameObject go)
  {
    if (go == null)
    {
      return string.Empty;
    }

    return go.name.Replace("(Clone)", string.Empty).Trim().ToLowerInvariant();
  }

  void AplicarEstadosCampaña(Personaje pers, GameObject GO)
  {
    Unidad unidad = GO.GetComponent<Unidad>();

    if (pers.Camp_Fatigado || (pers.PuedeRealizarActividades() && pers.ActividadSeleccionada == 2)) //FATIGADO
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Fatigado = new Buff();
      Fatigado.buffNombre = "Fatigado";
      Fatigado.boolfDebufftBuff = false;
      Fatigado.DuracionBuffRondas = -1;
      Fatigado.cantAtFue -= 1;
      Fatigado.cantAtAgi -= 1;
      Fatigado.cantAtPod -= 1;
      Fatigado.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Fatigado, GO.gameObject);
      //--------------------------------------

    }
    if (pers.TieneCampBendecido()) //BENDECIDO 
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Bendecido = new Buff();
      Bendecido.buffNombre = "Bendecido";
      Bendecido.boolfDebufftBuff = true;
      Bendecido.cantTsMental += 3;
      Bendecido.cantTsReflejos += 3;
      Bendecido.cantTsFortaleza += 3;
      Bendecido.cantResNec += 5;
      Bendecido.DuracionBuffRondas = -1;
      Bendecido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Bendecido, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Herido) //Herido
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Herido = new Buff();
      Herido.buffNombre = "Herido";
      Herido.boolfDebufftBuff = false;
      Herido.DuracionBuffRondas = -1;
      Herido.ocultarEnBarraVida = true;
      Herido.cantAtFue -= 1;
      Herido.cantAtAgi -= 1;
      Herido.cantAtPod -= 1;
      Herido.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Enfermo > 0) //Enfermo
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Enfermo = new Buff();
      Enfermo.buffNombre = "Enfermo";
      Enfermo.boolfDebufftBuff = false;
      Enfermo.DuracionBuffRondas = -1;
      Enfermo.cantTsFortaleza -= 3;
      Enfermo.cantDanioPorcentaje -= 15;
      Enfermo.cantAPMax -= 1;
      Enfermo.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Enfermo, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Moral < 0) //Baja Moral
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff Bajamoral = new Buff();
      Bajamoral.buffNombre = "Baja Moral";
      Bajamoral.boolfDebufftBuff = false;
      Bajamoral.DuracionBuffRondas = -1;
      Bajamoral.cantTsMental -= 3;
      Bajamoral.cantDefensa -= 1;
      Bajamoral.cantAtaque -= 1;
      unidad.SumarValentia(-2);
      Bajamoral.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(Bajamoral, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Moral > 0) //Alta Moral
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff AltaMoral = new Buff();
      AltaMoral.buffNombre = "Alta Moral";
      AltaMoral.boolfDebufftBuff = true;
      AltaMoral.DuracionBuffRondas = -1;
      AltaMoral.cantTsMental += 2;
      AltaMoral.cantAtaque += 1;
      unidad.SumarValentia(2);
      AltaMoral.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(AltaMoral, GO.gameObject);
      //--------------------------------------

    }
    if (pers.Camp_Avergonzado) //Avergonzado
    {
      //BUFF ---- Así se aplica un buff/debuff
      Buff avergonzado = new Buff();
      avergonzado.buffNombre = "Avergonzado";
      avergonzado.boolfDebufftBuff = false;
      avergonzado.DuracionBuffRondas = -1;
      avergonzado.cantTsMental -= 2;
      //avergonzado.percHPMax -= 10;
      avergonzado.AplicarBuff(unidad);
      unidad.SumarValentia(-2);
      
      Buff buffComponent = ComponentCopier.CopyComponent(avergonzado, GO.gameObject);

      if (unidad.HP_actual > unidad.mod_maxHP)
      {
        unidad.HP_actual = unidad.mod_maxHP;
      }
    }
    if (pers.Camp_Corrupto) //Corrupto
    {
      unidad.estado_Corrupto = true;

    }
    if (pers.PuedeRealizarActividades())
    {
      if (pers.ActividadSeleccionada == 5) //Caballero: Mantener Armadura
      {
        Buff Herido = new Buff();
        Herido.buffNombre = "Armadura Cuidada";
        Herido.boolfDebufftBuff = true;
        Herido.DuracionBuffRondas = -1;
        Herido.cantArmadura += 2;
        Herido.AplicarBuff(unidad);
        Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      }
      if (pers.ActividadSeleccionada == 1 && tipoEmboscadaActual == 0) //Base: Descansar (no aplica en emboscadas)
      {
        Buff Herido = new Buff();
        Herido.buffNombre = "Fresco";
        Herido.forzarTextoFlotanteInicioCombate = true;
        Herido.boolfDebufftBuff = true;
        Herido.DuracionBuffRondas = 2;
        Herido.cantIniciativa += 3;
        Herido.cantAPMax += 1;
        Herido.AplicarBuff(unidad);
        Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      }
      if (pers.ActividadSeleccionada == 8) //Explorador: Preparar Flechas
      {
        Buff Herido = new Buff();
        Herido.buffNombre = "Flechas Preparadas";
        Herido.boolfDebufftBuff = true;
        Herido.cantDanioPorcentaje += 5;
        unidad.GetComponent<ClaseExplorador>().PrepararFlechasDelay();
        Herido.AplicarBuff(unidad);
        Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      }
      if (pers.ActividadSeleccionada == 9) //Explorador: Exploración
      {
        Buff Fatigado = new Buff();
        Fatigado.buffNombre = "Fatigado por Explorar";
        Fatigado.boolfDebufftBuff = false;
        Fatigado.DuracionBuffRondas = -1;
        Fatigado.cantAPMax -= 1;
        Fatigado.AplicarBuff(unidad);
        Buff buffComponent = ComponentCopier.CopyComponent(Fatigado, GO.gameObject);
      }
      if (pers.ActividadSeleccionada == 13) //Acechador: Afilar Armas
      {
        Buff Herido = new Buff();
        Herido.buffNombre = "Arma Afilada";
        Herido.boolfDebufftBuff = true;
        Herido.DuracionBuffRondas = -1;
        Herido.cantDanioPorcentaje += 10;
        Herido.AplicarBuff(unidad);
        Buff buffComponent = ComponentCopier.CopyComponent(Herido, GO.gameObject);
      }
      if (pers.ActividadSeleccionada == 11) //Purificadora: Ayudar a los Desamparados
      {
        ClasePurificadora clasePurificadora = unidad.GetComponent<ClasePurificadora>();
        clasePurificadora.CambiarFervor(1); //Aumenta fervor
      }
      if (pers.ActividadSeleccionada == 16) //Canalizador: Concentración Arcana
      {
        if (unidad is ClaseCanalizador cana)
        {
          cana.CambiarEnergia(1);
        }
      }

      if (pers.ActividadSeleccionada == 19) //Duelista: Siempre Alerta
      {
        Buff siempreAlerta = new Buff();
        siempreAlerta.buffNombre = "Siempre Alerta";
        siempreAlerta.forzarTextoFlotanteInicioCombate = true;
        siempreAlerta.boolfDebufftBuff = true;
        siempreAlerta.DuracionBuffRondas = -1;
        siempreAlerta.ocultarEnBarraVida = true;
        siempreAlerta.cantIniciativa += 5;
        siempreAlerta.AplicarBuff(unidad);
        ComponentCopier.CopyComponent(siempreAlerta, GO.gameObject);

        if (tipoEmboscadaActual == 0)
        {
          Estados.Aplicar_MovimientoAbaratado(unidad, 2, unidad);
        }
      }
    }

    //Aplicar efectos de items equipados
    if (pers.itemArma != null)  //ARMA
    {
      if (pers.itemArma.nivelMejora > 0)   //Cada +1 da +1Ataque y +5% daño.
      {
        Buff buffArma = new Buff();
        buffArma.buffNombre = "Arma";
        buffArma.boolfDebufftBuff = true;
        buffArma.cantAtaque += pers.itemArma.nivelMejora; //+1 Ataque por cada nivel de mejora
        buffArma.cantDanioPorcentaje += 5 * pers.itemArma.nivelMejora; //5% por cada nivel de mejora
        buffArma.esBuffVisibleUI = false; //No se muestra en la UI
        buffArma.DuracionBuffRondas = -1; //Duración indefinida
        buffArma.AplicarBuff(unidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buffArma, GO.gameObject);
      }
    }
    /* if (pers.itemArmadura != null)  //EN ARMADURAS SE PUEDE CAMBIAR DIRECTAMENTE EN EL ITEM, MEJOR ASI SE MUESTRA EN LA UI
     ------------- NO SE APLICA ARMADURA POR MEJORA
     {
       if (pers.itemArmadura.nivelMejora > 0)   //Cada +1 da +2 Armadura.
       {
         Buff buffArmadura = new Buff();
         buffArmadura.buffNombre = "Armadura";
         buffArmadura.boolfDebufftBuff = true;
         buffArmadura.cantArmadura += 2*pers.itemArmadura.nivelMejora; // +2 Armadura por nivel de mejora
         buffArmadura.esBuffVisibleUI = false; //No se muestra en la UI
         buffArmadura.DuracionBuffRondas = -1; //Duración indefinida
         buffArmadura.AplicarBuff(unidad);
         // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
         Buff buffComponent = ComponentCopier.CopyComponent(buffArmadura, GO.gameObject);
       }
     }*/









    //Barrera/Evasion inicial de items equipados
    AplicarAtributosInicioCombateDeItems(pers, unidad);

    //---
    BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(unidad);

  }
  void AplicarAtributosInicioCombateDeItems(Personaje pers, Unidad unidad)
  {
    if (pers == null || unidad == null)
    {
      return;
    }

    int barreraTotal = 0;
    int evasionTotal = 0;
    int bonusFuegoTotal = 0;
    int bonusHieloTotal = 0;
    int bonusRayoTotal = 0;
    int bonusAcidoTotal = 0;
    int bonusArcanoTotal = 0;
    int bonusNecroTotal = 0;
    int bonusDivinoTotal = 0;
    int regenVidaTotal = 0;
    int regenArmaduraTotal = 0;
    int reduccionDanioTotal = 0;
    int reduccionDanioCriticoTotal = 0;
    int resistenciaEstadosTotal = 0;
    int espinasPlanoTotal = 0;
    int espinasPorcentajeTotal = 0;

    SumarAtributosInicioCombateDeItem(
      pers.itemArma,
      ref barreraTotal, ref evasionTotal,
      ref bonusFuegoTotal, ref bonusHieloTotal, ref bonusRayoTotal,
      ref bonusAcidoTotal, ref bonusArcanoTotal, ref bonusNecroTotal, ref bonusDivinoTotal,
      ref regenVidaTotal, ref regenArmaduraTotal,
      ref reduccionDanioTotal, ref reduccionDanioCriticoTotal, ref resistenciaEstadosTotal,
      ref espinasPlanoTotal, ref espinasPorcentajeTotal);
    SumarAtributosInicioCombateDeItem(
      pers.itemArmadura,
      ref barreraTotal, ref evasionTotal,
      ref bonusFuegoTotal, ref bonusHieloTotal, ref bonusRayoTotal,
      ref bonusAcidoTotal, ref bonusArcanoTotal, ref bonusNecroTotal, ref bonusDivinoTotal,
      ref regenVidaTotal, ref regenArmaduraTotal,
      ref reduccionDanioTotal, ref reduccionDanioCriticoTotal, ref resistenciaEstadosTotal,
      ref espinasPlanoTotal, ref espinasPorcentajeTotal);
    SumarAtributosInicioCombateDeItem(
      pers.Accesorio1,
      ref barreraTotal, ref evasionTotal,
      ref bonusFuegoTotal, ref bonusHieloTotal, ref bonusRayoTotal,
      ref bonusAcidoTotal, ref bonusArcanoTotal, ref bonusNecroTotal, ref bonusDivinoTotal,
      ref regenVidaTotal, ref regenArmaduraTotal,
      ref reduccionDanioTotal, ref reduccionDanioCriticoTotal, ref resistenciaEstadosTotal,
      ref espinasPlanoTotal, ref espinasPorcentajeTotal);
    SumarAtributosInicioCombateDeItem(
      pers.Accesorio2,
      ref barreraTotal, ref evasionTotal,
      ref bonusFuegoTotal, ref bonusHieloTotal, ref bonusRayoTotal,
      ref bonusAcidoTotal, ref bonusArcanoTotal, ref bonusNecroTotal, ref bonusDivinoTotal,
      ref regenVidaTotal, ref regenArmaduraTotal,
      ref reduccionDanioTotal, ref reduccionDanioCriticoTotal, ref resistenciaEstadosTotal,
      ref espinasPlanoTotal, ref espinasPorcentajeTotal);

    if (barreraTotal != 0)
    {
      unidad.barreraDeDanio += barreraTotal;
      if (unidad.barreraDeDanio < 0)
      {
        unidad.barreraDeDanio = 0;
      }
    }

    if (evasionTotal != 0)
    {
      unidad.estado_evasion += evasionTotal;
      if (unidad.estado_evasion < 0)
      {
        unidad.estado_evasion = 0;
      }
    }

    if (bonusFuegoTotal != 0) { unidad.bonusdam_fuego += bonusFuegoTotal; }
    if (bonusHieloTotal != 0) { unidad.bonusdam_hielo += bonusHieloTotal; }
    if (bonusRayoTotal != 0) { unidad.bonusdam_rayo += bonusRayoTotal; }
    if (bonusAcidoTotal != 0) { unidad.bonusdam_acido += bonusAcidoTotal; }
    if (bonusArcanoTotal != 0) { unidad.bonusdam_arcano += bonusArcanoTotal; }
    if (bonusNecroTotal != 0) { unidad.bonusdam_necro += bonusNecroTotal; }
    if (bonusDivinoTotal != 0) { unidad.bonusdam_divino += bonusDivinoTotal; }

    if (regenVidaTotal != 0)
    {
      unidad.estado_regeneravida += regenVidaTotal;
      if (unidad.estado_regeneravida < 0)
      {
        unidad.estado_regeneravida = 0;
      }
    }

    if (regenArmaduraTotal != 0)
    {
      unidad.estado_regeneraarmadura += regenArmaduraTotal;
      if (unidad.estado_regeneraarmadura < 0)
      {
        unidad.estado_regeneraarmadura = 0;
      }
    }

    if (reduccionDanioTotal != 0)
    {
      unidad.reduccionDanioRecibidoPorcentaje += reduccionDanioTotal;
      unidad.reduccionDanioRecibidoPorcentaje = Mathf.Clamp(unidad.reduccionDanioRecibidoPorcentaje, 0, 95);
    }

    if (reduccionDanioCriticoTotal != 0)
    {
      unidad.reduccionDanioCriticoRecibidoPorcentaje += reduccionDanioCriticoTotal;
      unidad.reduccionDanioCriticoRecibidoPorcentaje = Mathf.Clamp(unidad.reduccionDanioCriticoRecibidoPorcentaje, 0, 95);
    }

    if (resistenciaEstadosTotal != 0)
    {
      unidad.resistenciaEstadosPorcentaje += resistenciaEstadosTotal;
      unidad.resistenciaEstadosPorcentaje = Mathf.Clamp(unidad.resistenciaEstadosPorcentaje, 0, 100);
    }

    if (espinasPlanoTotal != 0)
    {
      unidad.espinasDanioPlano += espinasPlanoTotal;
      if (unidad.espinasDanioPlano < 0)
      {
        unidad.espinasDanioPlano = 0;
      }
    }

    if (espinasPorcentajeTotal != 0)
    {
      unidad.espinasDanioPorcentaje += espinasPorcentajeTotal;
      if (unidad.espinasDanioPorcentaje < 0)
      {
        unidad.espinasDanioPorcentaje = 0;
      }
    }
  }

  void SumarAtributosInicioCombateDeItem(
    Item item,
    ref int barreraTotal,
    ref int evasionTotal,
    ref int bonusFuegoTotal,
    ref int bonusHieloTotal,
    ref int bonusRayoTotal,
    ref int bonusAcidoTotal,
    ref int bonusArcanoTotal,
    ref int bonusNecroTotal,
    ref int bonusDivinoTotal,
    ref int regenVidaTotal,
    ref int regenArmaduraTotal,
    ref int reduccionDanioTotal,
    ref int reduccionDanioCriticoTotal,
    ref int resistenciaEstadosTotal,
    ref int espinasPlanoTotal,
    ref int espinasPorcentajeTotal)
  {
    if (item == null)
    {
      return;
    }

    barreraTotal += item.barreraInicioCombate;
    evasionTotal += item.evasionInicioCombate;
    bonusFuegoTotal += item.bonusDanioFuegoInicioCombate;
    bonusHieloTotal += item.bonusDanioHieloInicioCombate;
    bonusRayoTotal += item.bonusDanioRayoInicioCombate;
    bonusAcidoTotal += item.bonusDanioAcidoInicioCombate;
    bonusArcanoTotal += item.bonusDanioArcanoInicioCombate;
    bonusNecroTotal += item.bonusDanioNecroInicioCombate;
    bonusDivinoTotal += item.bonusDanioDivinoInicioCombate;
    regenVidaTotal += item.regeneracionVidaInicioCombate;
    regenArmaduraTotal += item.regeneracionArmaduraInicioCombate;
    reduccionDanioTotal += item.reduccionDanioRecibidoPorcentaje;
    reduccionDanioCriticoTotal += item.reduccionDanioCriticoRecibidoPorcentaje;
    resistenciaEstadosTotal += item.resistenciaEstadosPorcentaje;
    espinasPlanoTotal += item.espinasDanioPlano;
    espinasPorcentajeTotal += item.espinasDanioPorcentaje;
  }

  void AplicarEfectosItemsEspecificos(Personaje pers, GameObject GO)
  {
    Unidad unidad = GO.GetComponent<Unidad>();

    //Armaduras
    if (pers.itemArmadura != null)
    {
      //Armadura de Cuero Reforzado de Ligereza
      if (pers.itemArmadura.IDEfectoEspecial == 1)
      {
        unidad.estado_evasion += 2; // Da +2 de evasión al comenzar el combate
      }
      //Armadura de Cuero Reforzado de Velo
      if (pers.itemArmadura.IDEfectoEspecial == 2 && unidad is ClaseAcechador unidadAcechador)
      {
        unidadAcechador.tieneArmaduradeVelo = true; // Activa la armadura de velo
      }
      //Armadura de Coraza de Llamas +1
      if (pers.itemArmadura.IDEfectoEspecial == 3 && unidad is ClaseCaballero unidadCaballero)
      {
        unidadCaballero.tieneCorazaDeLlamas = true; // Activa la armadura de coraza de llamas
      }
      if (pers.itemArmadura.IDEfectoEspecial == 4) // Activa la armadura de cuero borrosa
      {
        unidad.estado_evasion += 3; // Da +3 de evasión al comenzar el combate
      }
    }

    AplicarEfectoEspecialAccesorio(pers.Accesorio1, unidad, GO);
    AplicarEfectoEspecialAccesorio(pers.Accesorio2, unidad, GO);
  }


  void AplicarEfectoEspecialAccesorio(Accesorio accesorio, Unidad unidad, GameObject GO)
  {
    if (accesorio == null || unidad == null || GO == null)
    {
      return;
    }

    if (accesorio.IDEfectoEspecial == 5)
    {
      Buff buffAnillo = new Buff();
      buffAnillo.buffNombre = "Anillo de Destruccion";
      buffAnillo.boolfDebufftBuff = true;
      buffAnillo.DuracionBuffRondas = -1;
      buffAnillo.esBuffVisibleUI = false;
      buffAnillo.cantDanioPorcentaje += 10;
      buffAnillo.AplicarBuff(unidad);
      ComponentCopier.CopyComponent(buffAnillo, GO.gameObject);
    }
  }

  void CopiarHabilidades(GameObject pers, GameObject persUnidad)
  {
    Habilidad[] habilidades = pers.GetComponents<Habilidad>();


    foreach (Habilidad habilidad in habilidades)
    {

      if (habilidad.GetType().Name.Contains("REPRESENTACION")) //Si es una pasiva
      {
        //Aca se le dice a la Unidad creada qué pasivas tiene el personaje, y de que nivel
        //------------------------------------------------------------------
        //Pasivas de CABALLERO 
        if (persUnidad.GetComponent<ClaseCaballero>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONArmaduraLimitante"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONCorajeInquebrantable"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAcorazado"))
          {
            persUnidad.GetComponent<ClaseCaballero>().PASIVA_Acorazado = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONDeterminacion"))
          {
            persUnidad.GetComponent<ClaseCaballero>().PASIVA_Determinacion = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONImplacable"))
          {
            persUnidad.GetComponent<ClaseCaballero>().PASIVA_Implacable = habilidad.NIVEL;
          }
          else
          {
            print("ATENCIÓN: " + pers.name + " tiene una pasiva (" + habilidad.GetType().Name + ") que no ha sido agregada al control (arriba en codigo), revisar.");
          }
        }
        if (persUnidad.GetComponent<ClaseExplorador>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONPasoCauteloso"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONVistaLejana"))
          {
            persUnidad.GetComponent<ClaseExplorador>().PASIVA_VistaLejana = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAcrobatico"))
          {
            persUnidad.GetComponent<ClaseExplorador>().PASIVA_Acrobatico = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONReconocimiento"))
          {
            persUnidad.GetComponent<ClaseExplorador>().PASIVA_Reconocimiento = habilidad.NIVEL;
          }


        }
        if (persUnidad.GetComponent<ClasePurificadora>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONFervorConjunto"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAuraSagrada"))
          {
            persUnidad.GetComponent<ClasePurificadora>().PASIVA_AuraSagrada = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONEcosDivinos"))
          {
            persUnidad.GetComponent<ClasePurificadora>().PASIVA_EcosDivinos = habilidad.NIVEL;
          }
        }
        if (persUnidad.GetComponent<ClaseAcechador>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONSigiloso"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONMaestriaBallesta"))
          {
            persUnidad.GetComponent<ClaseAcechador>().PASIVA_MaestriaConBallestaMano = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONMaestriaEspadaCorta"))
          {
            persUnidad.GetComponent<ClaseAcechador>().PASIVA_MaestriaConEspadacorta = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONMasacre"))
          {
            persUnidad.GetComponent<ClaseAcechador>().PASIVA_Masacre = habilidad.NIVEL;
          }

        }
        if (persUnidad.GetComponent<ClaseCanalizador>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONSobrecarga"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAcumulacionProtegida"))
          {
            persUnidad.GetComponent<ClaseCanalizador>().PASIVA_AcumulacionProtegida = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONExcesoDePoder"))
          {
            persUnidad.GetComponent<ClaseCanalizador>().PASIVA_ExcesoDePoder = habilidad.NIVEL;
          }

        }
        if (persUnidad.GetComponent<ClaseDuelista>() != null)
        {
          if (habilidad.GetType().Name.Contains("REPRESENTACIONPasoLigero") || habilidad.GetType().Name.Contains("REPRESENTACIONPosturaDemandante"))
          {
            //Intrinseca (ya programada en el código de la clase)
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONAtaquesReveladores"))
          {
            persUnidad.GetComponent<ClaseDuelista>().PASIVA_AtaquesReveladores = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONEvasionMaestra"))
          {
            persUnidad.GetComponent<ClaseDuelista>().PASIVA_EvasionMaestra = habilidad.NIVEL;
          }
          else if (habilidad.GetType().Name.Contains("REPRESENTACIONDanzaDelEstoque"))
          {
            persUnidad.GetComponent<ClaseDuelista>().PASIVA_DanzaDelEstoque = habilidad.NIVEL;
          }
        }
      }

      // Obtener el tipo de la habilidad
      System.Type tipoHabilidad = habilidad.GetType();

      // Agregar un nuevo componente del mismo tipo al objeto persUnidad
      Habilidad nuevaHabilidad = (Habilidad)persUnidad.AddComponent(tipoHabilidad);


      nuevaHabilidad.NIVEL = habilidad.NIVEL;
      nuevaHabilidad.ActualizarNivel(); //Esto es para que se llame awake de nuevo, luego de establecer el nivel, asi se aplican los efectos del nivel

    }
  }



  public void FinDeBatalla(int resultado)
  {
    EjecutarOperacionSegura(FinDeBatallaAsync(resultado), nameof(FinDeBatalla));
  }

  private async Task FinDeBatallaAsync(int resultado)
  {
    if (cerrandoBatalla)
    {
      return;
    }

    cerrandoBatalla = true;
    try
    {
    if (!ValidarDependenciasFinDeBatalla(out var campaignManager, out var battleManager))
    {
      return;
    }

    bool debeReanudarTutorialNuevoPostCombate =
      (ultimoIDEncuentro == 700 || ultimoIDEncuentro == 701)
      && campaignManager.DebeUsarConfiguracionTutorial();

    PlasmarEfectosBatallaEnPersonajes();
    campaignManager.logDeCampania?.LimpiarDesdeBatalla();
    if (campaignManager.scMenuBatallas != null)
    {
      campaignManager.scMenuBatallas.EfectosDeBatallaEnCampaña(resultado);
      campaignManager.logDeCampania?.RegistrarResumenBatalla(resultado, campaignManager.scMenuPersonajes != null ? campaignManager.scMenuPersonajes.listaPersonajes : null);
      campaignManager.scMenuBatallas.DejanEnListaParticipantesSolo();
    }
    campaignManager.RefrescarBarraPersonajesCampania(true);
    if (campaignManager.scMapaManager != null && campaignManager.scMapaManager.nodoActual != null)
    {
      campaignManager.scMapaManager.nodoActual.nodoDespejado = true;
    }

    if (resultado == 1 && ultimoIDEncuentro == 700 && campaignManager.DebeUsarConfiguracionTutorial())
    {
      campaignManager.AplicarRecompensaPrimerCombateTutorial();
    }

    campaignManager.BATALLA_EnCurso = 0;
    campaignManager.EMBOSCADA_EnCurso = 0;

   

    battleManager.ReiniciarEstadoRefuerzos();

    if (unidadPers1 != null)
    { Destroy(unidadPers1); }
    if (unidadPers2 != null)
    { Destroy(unidadPers2); }
    if (unidadPers3 != null)
    { Destroy(unidadPers3); }
    if (unidadPers4 != null)
    { Destroy(unidadPers4); }

    EliminarTodasLasUnidades();
    EliminarTodasLasTrampas();
    EliminarTodosLosPresentes();

    battleManager.RondaNro = 1;

    battleManager.unidadActiva = null;
    battleManager.indexTurno = 0;


    float fadeTransicion = Mathf.Max(0.05f, fadeSalidaBatalla);
    float holdTransicion = Mathf.Max(0.1f, esperaSalidaBatalla);
    float esperaCambioEscena = CalcularEsperaCambioEscena(fadeTransicion, holdTransicion);
    campaignManager.scAdministradorEscenas.PlayFadeInOut(fadeTransicion, holdTransicion);
    await EsperarSegundosRealtime(esperaCambioEscena);

    EscenaCampaign.SetActive(true);
    // Al volver a campaña, asegurar que los VFX de nodos reflejen su estado lógico
    var contNodos = campaignManager.scMapaManager != null
      ? campaignManager.scMapaManager.scContenedordeNodos
      : null;
    if (contNodos != null && contNodos.listTodosNodos != null)
    {
      foreach (var nodo in contNodos.listTodosNodos)
      {
        if (nodo == null) continue;

      

        nodo.SincronizarVFXPersistentes();
      }
    }

    if (campaignManager.scMapaManager != null)
    {
      campaignManager.scMapaManager.RefrescarVisibilidadExploracion();
    }

    if (MusicManager.Instance != null)
    {
      MusicManager.Instance.PausarMusica(false);
      MusicManager.Instance.VolverACampania();
    }
    escenaActual = 0;
    RefrescarUICompartidaSegunEscena();
    battleManager.SetLogCombateActivoPorEscena(false);
    EscenaBatalla.SetActive(false);

    Time.timeScale = 1; //Vuelve el tiempo al normal(por si habia modo rapido), pero no cambia el estado de Modorapido

    int delayFinal = Mathf.Max(0, delayPostFinBatallaMs);
    if (delayFinal > 0)
    {
      await BattleManager.DelayCombateAsync(delayFinal);
    }

    TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.BattleEnded, gameObject)
      .Add("encounterId", ultimoIDEncuentro)
      .Add("result", resultado)
      .Add("isFirstTutorialBattle", ultimoIDEncuentro == 700 ? 1 : 0)
      .Add("isFinalTutorialBattle", ultimoIDEncuentro == 701 ? 1 : 0));

    if (debeReanudarTutorialNuevoPostCombate)
    {
      TutorialDirector.Instance?.ResumeFromLegacyCombatTutorial(ultimoIDEncuentro != 701);
    }

    if (campaignManager.scTutorialManager != null
      && campaignManager.scTutorialManager.tutorialActivo
      && campaignManager.scTutorialManager.pasoActual == 3)
    {
      campaignManager.scTutorialManager.SiguientePaso();
      //Personaje1.RecibirExperiencia(100);
    }
    //Limpieza
    Personaje1 = null;
    Personaje2 = null;
    Personaje3 = null;
    Personaje4 = null;
    }
    finally
    {
      cerrandoBatalla = false;
    }
  }
  public void EliminarTodosLosPresentes()
  {
    foreach (var casilla in BattleManager.Instance.ladoA.casillasLado)
    {
      if (casilla.Presente != null)
      {
        Destroy(casilla.Presente);
        casilla.Presente = null;
      }

    }
    foreach (var casilla in BattleManager.Instance.ladoB.casillasLado)
    {
      if (casilla.Presente != null)
      {
        Destroy(casilla.Presente);
        casilla.Presente = null;
      }
    }
  }
  public void EliminarTodasLasUnidades()
  {
    // Buscar todos los objetos con el tag "Unidad"
    GameObject[] unidades = GameObject.FindGameObjectsWithTag("Unidad");

    // Recorrer el array y destruir cada objeto
    foreach (GameObject unidad in unidades)
    {
      Destroy(unidad);
    }
  }
  public void EliminarTodasLasTrampas()
  {

    // Elimina todos los componentes de tipo Trampa en cada casilla de ambos lados
    foreach (var casilla in BattleManager.Instance.ladoA.casillasLado)
    {
      var trampas = casilla.GetComponents<Trampa>();
      foreach (var trampa in trampas)
      {
        trampa.DestruirTrampa();
      }
    }
    foreach (var casilla in BattleManager.Instance.ladoB.casillasLado)
    {
      var trampas = casilla.GetComponents<Trampa>();
      foreach (var trampa in trampas)
      {
        trampa.DestruirTrampa();
      }
    }


  }
  void PlasmarEfectosBatallaEnPersonajes()
  {
    int longitudBatallaFatiga = 7;

    float VidaCampaniaDesdeUnidad(Personaje pers, Unidad unidad)
    {
      if (pers == null || unidad == null)
      {
        return 0f;
      }

      float vidaSinBonusFuerza = unidad.HP_actual - unidad.ObtenerBonusVidaPorFuerzaActual();
      float vidaCampania = Mathf.Clamp(vidaSinBonusFuerza, 0f, pers.fVidaMaxima);
      return unidad.HP_actual > 0f && vidaCampania < 1f ? 1f : vidaCampania;
    }

    void AplicarFatigaPorBatallaLargaSiCorresponde(Personaje pers)
    {
      if (pers == null || BattleManager.Instance.RondaNro <= longitudBatallaFatiga || TieneTrait(pers, PersonajeTraitCatalog.TraitIncansable))
      {
        return;
      }

      bool yaEstabaFatigado = pers.Camp_Fatigado;
      pers.SetCampFatigado(true);

      if (!yaEstabaFatigado && CampaignManager.Instance != null && CampaignManager.Instance.scMenuBatallas != null)
      {
        CampaignManager.Instance.scMenuBatallas.RegistrarTooltipFatigadoPorBatallaLarga();
      }
    }


    if (Personaje1 != null)
    {
      float vidaFinalCamp = VidaCampaniaDesdeUnidad(Personaje1, unidadPers1);
      if (vidaFinalCamp < Personaje1.fVidaActual)
      { Personaje1.fVidaActual = vidaFinalCamp; } //No puede terminar con mas vida de la que empezo

      CanalizadorPasivaSobrecarga(Personaje1, unidadPers1);

      AplicarFatigaPorBatallaLargaSiCorresponde(Personaje1);
    }
    if (Personaje2 != null)
    {
      float vidaFinalCamp = VidaCampaniaDesdeUnidad(Personaje2, unidadPers2);
      if (vidaFinalCamp < Personaje2.fVidaActual)
      { Personaje2.fVidaActual = vidaFinalCamp; } //No puede terminar con mas vida de la que empezo

      CanalizadorPasivaSobrecarga(Personaje2, unidadPers2);


      AplicarFatigaPorBatallaLargaSiCorresponde(Personaje2);
    }
    if (Personaje3 != null)
    {
      float vidaFinalCamp = VidaCampaniaDesdeUnidad(Personaje3, unidadPers3);
      if (vidaFinalCamp < Personaje3.fVidaActual)
      { Personaje3.fVidaActual = vidaFinalCamp; } //No puede terminar con mas vida de la que empezo

      CanalizadorPasivaSobrecarga(Personaje3, unidadPers3);


      AplicarFatigaPorBatallaLargaSiCorresponde(Personaje3);
    }
    if (Personaje4 != null)
    {
      float vidaFinalCamp = VidaCampaniaDesdeUnidad(Personaje4, unidadPers4);
      if (vidaFinalCamp < Personaje4.fVidaActual)
      { Personaje4.fVidaActual = vidaFinalCamp; } //No puede terminar con mas vida de la que empezo

      CanalizadorPasivaSobrecarga(Personaje4, unidadPers4);

      AplicarFatigaPorBatallaLargaSiCorresponde(Personaje4);
    }

    // Plasmar efectos de los refuerzos aliados (no milicianos) 
    //EN TEORIA ESTO NO ES NECESARIO YA QUE SI SE PIERDEN BATALLAS DE REFUERZOS SE PIERDE EL JUEGO
    List<Unidad> aliadosQueEntraron = new List<Unidad>();
    foreach (Unidad unidadRefuerzo in BattleManager.Instance.ladoB.unidadesLado)
    {
      if (unidadRefuerzo == null) continue;
      if (unidadRefuerzo.entroComoAliado)
      {
        aliadosQueEntraron.Add(unidadRefuerzo);
      }


    }
    foreach (Unidad unidadRefuerzo in aliadosQueEntraron)
    {

      // Buscar el personaje correspondiente al refuerzo (por nombre)
      bool encontrado = false;
      foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
        if (pers.sNombre == unidadRefuerzo.uNombre)
        {
          // Plasmar vida actual (no puede terminar con más vida de la que empezó)
          float vidaFinalCamp = VidaCampaniaDesdeUnidad(pers, unidadRefuerzo);
          if (vidaFinalCamp < pers.fVidaActual)
          {
            pers.fVidaActual = vidaFinalCamp;
          }

          CanalizadorPasivaSobrecarga(pers, unidadRefuerzo);
          CampaignManager.Instance.scMenuBatallas.AdministrarHeridas(pers, unidadRefuerzo);

          AplicarFatigaPorBatallaLargaSiCorresponde(pers);
          encontrado = true;
          break;
        }
      }
    }


  }

  void CanalizadorPasivaSobrecarga(Personaje pers, Unidad unidad)
  {
    if (unidad is ClaseCanalizador scCanalizador && pers.fVidaActual > 0)
    {

      float maxHP = pers.fVidaMaxima;
      float damPorEnergia = maxHP * 0.15f;
      int nEnergiaCombate = scCanalizador.ObtenerEnergia();

      pers.fVidaActual -= damPorEnergia * nEnergiaCombate;
      if (pers.fVidaActual < 1) { pers.fVidaActual = 1; }
      pers.fVidaMaxima -= 1 * nEnergiaCombate;

      //     print($"Al tener {nEnergiaCombate} Energia, {scCanalizador.uNombre} perdió {damPorEnergia * nEnergiaCombate} vida y {1 * nEnergiaCombate} mas hp.");

    }



  }

  void AsignarRefuerzosCuartel()
  {
    AdministradorEscenas scAdminEscenas = CampaignManager.Instance.scMenuBatallas.scAdministradorEscenas;
    var prefabs = scAdminEscenas.ContenedorPrefabsBatalla;

    if (prefabs != null && BattleManager.Instance != null)
    {
      GameObject mil1 = Instantiate(prefabs.Miliciano1);
      mil1.SetActive(false);
      BattleManager.Instance.aliadosRefuerzos.Add(mil1);

      GameObject mil2 = Instantiate(prefabs.Miliciano2);
      mil2.SetActive(false);
      BattleManager.Instance.aliadosRefuerzos.Add(mil2);

      BattleManager.Instance.ActualizarAliadosRefUI();
    }

    BattleManager.Instance.EscribirLog(TRADU.i.Traducir("<b>Una patrulla de milicianos de Serria se une a la batalla como refuerzos.</b>"));
    CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("<b>Una patrulla de milicianos de Serria se une a la batalla como refuerzos.</b>"));
  }
  void AsignarRefuerzosAliados(bool filtrarPorActividadGuardia = true)
  {
    AdministradorEscenas scAdminEscenas = CampaignManager.Instance.scMenuBatallas.scAdministradorEscenas;
    bool usarOrdenPersonalizadoCaravana = OrdenRefuerzosAliadosCaravana != null && OrdenRefuerzosAliadosCaravana.Count > 0;
    List<Personaje> personajesOrdenados = new List<Personaje>();
    Dictionary<Personaje, GameObject> refuerzosHeroesPorPersonaje = new Dictionary<Personaje, GameObject>();
    Dictionary<string, Queue<GameObject>> refuerzosMilicianosPorPrefab = new Dictionary<string, Queue<GameObject>>();

    if (usarOrdenPersonalizadoCaravana)
    {
      foreach (RefuerzoAliadoCaravanaOrdenItem refuerzo in OrdenRefuerzosAliadosCaravana)
      {
        if (refuerzo != null && refuerzo.EsPersonaje && refuerzo.personaje != null && !personajesOrdenados.Contains(refuerzo.personaje))
        {
          personajesOrdenados.Add(refuerzo.personaje);
        }
      }
    }

    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers != null && !personajesOrdenados.Contains(pers))
      {
        personajesOrdenados.Add(pers);
      }
    }

    int cantRefuerzoAliadoHeroe = 0;
    foreach (Personaje pers in personajesOrdenados)
    {
      if (pers == Personaje1 || pers == Personaje2 || pers == Personaje3 || pers == Personaje4)
      {
        continue; // Ya está desplegado al inicio, no duplicar como refuerzo.
      }
      if (!pers.Camp_Muerto && pers.fVidaActual > 1) //Si no está muerto y tiene vida
      {
        if (!filtrarPorActividadGuardia || !EsActividadGuardiaParaCaravana(pers))
        {
          cantRefuerzoAliadoHeroe++;
          //Si no está haciendo guardia, lo agrega como refuerzo, pero no lo muestra
          scAdminEscenas.ColocarPersonajecomoUnidad(pers, cantRefuerzoAliadoHeroe);
          if (!refuerzosHeroesPorPersonaje.ContainsKey(pers) && BattleManager.Instance.aliadosRefuerzos.Count > 0)
          {
            refuerzosHeroesPorPersonaje.Add(pers, BattleManager.Instance.aliadosRefuerzos[BattleManager.Instance.aliadosRefuerzos.Count - 1]);
          }
        }
      }

    }
    //Agrega milicianos como refuerzos
    int cantidadMilicianos = (int)CampaignManager.Instance.GetMiliciasActual() / 10; //Cada 10 milicianos, uno se une como refuerzo
    if (cantidadMilicianos > 0)
    {
      bool tieneDesertores = CampaignManager.Instance.scMenuSequito.TieneSequito(6);
      var prefabs = scAdminEscenas.ContenedorPrefabsBatalla;

      for (int i = 0; i < cantidadMilicianos; i++)
      {
        GameObject aliado;
        GameObject prefabOrigen;

        if (tieneDesertores)
        {
          prefabOrigen = i % 2 == 0 ? prefabs.Desertor2 : prefabs.Desertor1;
          aliado = Instantiate(prefabOrigen);
        }
        else
        {
          prefabOrigen = i % 2 == 0 ? prefabs.Miliciano2 : prefabs.Miliciano1;
          aliado = Instantiate(prefabOrigen);
        }

        aliado.SetActive(false);
        BattleManager.Instance.aliadosRefuerzos.Add(aliado);
        if (prefabOrigen != null)
        {
          string clavePrefab = prefabOrigen.name;
          if (!refuerzosMilicianosPorPrefab.TryGetValue(clavePrefab, out Queue<GameObject> colaPrefab))
          {
            colaPrefab = new Queue<GameObject>();
            refuerzosMilicianosPorPrefab.Add(clavePrefab, colaPrefab);
          }

          colaPrefab.Enqueue(aliado);
        }
      }
    }

    if (usarOrdenPersonalizadoCaravana)
    {
      List<GameObject> refuerzosOrdenados = new List<GameObject>(BattleManager.Instance.aliadosRefuerzos.Count);
      HashSet<GameObject> refuerzosYaOrdenados = new HashSet<GameObject>();

      foreach (RefuerzoAliadoCaravanaOrdenItem refuerzo in OrdenRefuerzosAliadosCaravana)
      {
        if (refuerzo == null)
        {
          continue;
        }

        if (refuerzo.EsPersonaje && refuerzo.personaje != null && refuerzosHeroesPorPersonaje.TryGetValue(refuerzo.personaje, out GameObject refuerzoHeroe) && refuerzoHeroe != null && !refuerzosYaOrdenados.Contains(refuerzoHeroe))
        {
          refuerzosOrdenados.Add(refuerzoHeroe);
          refuerzosYaOrdenados.Add(refuerzoHeroe);
          continue;
        }

        if (refuerzo.EsUnidadPrefab && refuerzo.prefabRefuerzo != null && refuerzosMilicianosPorPrefab.TryGetValue(refuerzo.prefabRefuerzo.name, out Queue<GameObject> colaPrefab))
        {
          while (colaPrefab.Count > 0)
          {
            GameObject refuerzoMilicia = colaPrefab.Dequeue();
            if (refuerzoMilicia == null || refuerzosYaOrdenados.Contains(refuerzoMilicia))
            {
              continue;
            }

            refuerzosOrdenados.Add(refuerzoMilicia);
            refuerzosYaOrdenados.Add(refuerzoMilicia);
            break;
          }
        }
      }

      foreach (GameObject refuerzoPendiente in BattleManager.Instance.aliadosRefuerzos)
      {
        if (refuerzoPendiente != null && !refuerzosYaOrdenados.Contains(refuerzoPendiente))
        {
          refuerzosOrdenados.Add(refuerzoPendiente);
        }
      }

      BattleManager.Instance.aliadosRefuerzos.Clear();
      BattleManager.Instance.aliadosRefuerzos.AddRange(refuerzosOrdenados);
    }
    else
    {
      // Da vuelta la lista de refuerzos aliados
      BattleManager.Instance.aliadosRefuerzos.Reverse();
    }

    if (usarOrdenPersonalizadoCaravana)
    {
      OrdenRefuerzosAliadosCaravana.Clear();
    }

    BattleManager.Instance.ActualizarAliadosRefUI();

  }

  public void AbrirHandbook()
  {
    if (HandbookCampania == null)
    {
      return;
    }

    bool abrir = !HandbookCampania.activeSelf;
    HandbookCampania.SetActive(abrir);

    if (abrir)
    {
      bool abrirHandbookBatalla = BattleManager.Instance != null && BattleManager.Instance.LogCombateActivoPorEscena;
      ActivarHandbookInicial(abrirHandbookBatalla);
    }
  }

  void ActivarHandbookInicial(bool batalla)
  {
    HandbookManager[] handbooks = HandbookCampania.GetComponentsInChildren<HandbookManager>(true);
    foreach (HandbookManager handbook in handbooks)
    {
      if (handbook == null)
      {
        continue;
      }

      bool activo = handbook.esdeBatalla == batalla;
      handbook.gameObject.SetActive(activo);
      if (activo)
      {
        handbook.AbrirSolapa(1);
      }
    }
  }
  void Update()
  {
    // Detecta cuando se presiona la tecla H una sola vez
    if (Input.GetKeyDown(KeyCode.H))
    {
      AbrirHandbook();
    }
  }



}
