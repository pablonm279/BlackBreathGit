using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BattleRewardTuning
{
   public float expPerPoint = 25f;
   public float goldPerPoint = 18f;
   public float materialsPerPoint = 2f;
   public float defeatHopeLossPerPoint = 2f;
   public float defeatCivilLossPerPoint = 1f;
   public int defeatOxLoss = 0;
   public float victoryHopeBonus = 10f;
   public float itemChancePerPoint = 0f;
}

[System.Serializable]
public class BattleRewardProfile
{
   public BattleEncounterType battleType;
   public BattleRewardTuning tuning = new BattleRewardTuning();
}

public class MenuBatallas : MonoBehaviour
{
  
 
 public GameObject prefabBtnPersonaje; 
 public GameObject contenedorUIPersonajes; 
 public AdministradorEscenas scAdministradorEscenas;

 [Header("Generador de encuentros")]
 [SerializeField] List<string> corruptFactionIds = new List<string>() { "Corruptos" };
 [SerializeField] float corruptChancePerAliento = 4f;
 [SerializeField] List<BattleRewardProfile> rewardProfiles = new List<BattleRewardProfile>();
 [SerializeField] BattleRewardTuning defaultRewardTuning = new BattleRewardTuning();
 const string KaleTavFactionId = "Kale'Tav";

  EncounterDefinition encuentroGeneradoActual;
  EncounterZoneType encuentroZonaActual;
  BattleEncounterType encuentroTipoActual;
  bool esBatallaFinal = false;
  bool transicionJefeZonaPendiente = false;

 public GameObject UIEmpezarBatalla;
 public GameObject UIEmpezarBatallaACaravana;
 public GameObject UITerminarBatalla;
 public GameObject txtVictoria;
 public GameObject txtDerrota;
 public TextMeshProUGUI txtRecompensa;
 public TextMeshProUGUI txtMilicianosDisponibles;

 [SerializeField] GameObject btnComenzar;
 bool bloqueoComenzarBatalla = false;

 void OnEnable()
 {
    bloqueoComenzarBatalla = false;
    SetBotonComenzarInteractable(true);
 }

 void SetBotonComenzarInteractable(bool interactable)
 {
    if (btnComenzar == null)
    {
      return;
    }

    Button btn = btnComenzar.GetComponent<Button>();
    if (btn != null)
    {
      btn.interactable = interactable;
    }
 }

 void Awake()
 {
    EnsureRewardProfiles();
 }

 void OnValidate()
 {
    EnsureRewardProfiles();
 }

 void EnsureRewardProfiles()
 {
    if (defaultRewardTuning == null)
    {
       defaultRewardTuning = new BattleRewardTuning();
    }

    if (rewardProfiles == null)
    {
       rewardProfiles = new List<BattleRewardProfile>();
    }

    foreach (BattleEncounterType tipo in Enum.GetValues(typeof(BattleEncounterType)))
    {
       var profile = rewardProfiles.Find(p => p != null && p.battleType == tipo);
       if (profile == null)
       {
          profile = new BattleRewardProfile { battleType = tipo, tuning = new BattleRewardTuning() };
          rewardProfiles.Add(profile);
       }
       else if (profile.tuning == null)
       {
          profile.tuning = new BattleRewardTuning();
       }

       switch (tipo)
       {
          case BattleEncounterType.Elite:
             profile.tuning.expPerPoint = defaultRewardTuning.expPerPoint * 1.35f;
             profile.tuning.goldPerPoint = defaultRewardTuning.goldPerPoint * 1.2f;
             profile.tuning.materialsPerPoint = defaultRewardTuning.materialsPerPoint * 1.6f;
             profile.tuning.defeatHopeLossPerPoint = defaultRewardTuning.defeatHopeLossPerPoint * 1.1f;
             profile.tuning.defeatCivilLossPerPoint = Mathf.Max(defaultRewardTuning.defeatCivilLossPerPoint, 1.2f);
             profile.tuning.victoryHopeBonus = Mathf.Max(defaultRewardTuning.victoryHopeBonus, 12f);
             profile.tuning.itemChancePerPoint = Mathf.Max(defaultRewardTuning.itemChancePerPoint, 6f);
             break;
          case BattleEncounterType.AtaqueCaravana:
             profile.tuning.expPerPoint = defaultRewardTuning.expPerPoint * 1.1f;
             profile.tuning.goldPerPoint = defaultRewardTuning.goldPerPoint * 1.6f;
             profile.tuning.materialsPerPoint = defaultRewardTuning.materialsPerPoint * 1.4f;
             profile.tuning.defeatHopeLossPerPoint = defaultRewardTuning.defeatHopeLossPerPoint * 1.5f;
             profile.tuning.defeatCivilLossPerPoint = Mathf.Max(defaultRewardTuning.defeatCivilLossPerPoint, 2f);
             profile.tuning.defeatOxLoss = Mathf.Max(profile.tuning.defeatOxLoss, 1);
             profile.tuning.victoryHopeBonus = Mathf.Max(defaultRewardTuning.victoryHopeBonus, 15f);
             profile.tuning.itemChancePerPoint = Mathf.Max(defaultRewardTuning.itemChancePerPoint, 6f);
             break;
          case BattleEncounterType.Subterraneo:
             profile.tuning.expPerPoint = defaultRewardTuning.expPerPoint * 1.2f;
             profile.tuning.goldPerPoint = defaultRewardTuning.goldPerPoint * 0.9f;
             profile.tuning.materialsPerPoint = defaultRewardTuning.materialsPerPoint * 2.0f;
             profile.tuning.defeatHopeLossPerPoint = defaultRewardTuning.defeatHopeLossPerPoint * 1.2f;
             profile.tuning.defeatCivilLossPerPoint = defaultRewardTuning.defeatCivilLossPerPoint * 0.5f;
             profile.tuning.victoryHopeBonus = Mathf.Max(defaultRewardTuning.victoryHopeBonus * 0.8f, 6f);
             profile.tuning.itemChancePerPoint = Mathf.Max(defaultRewardTuning.itemChancePerPoint, 4f);
             break;
          case BattleEncounterType.Normal:
          default:
             profile.tuning.expPerPoint = defaultRewardTuning.expPerPoint;
             profile.tuning.goldPerPoint = defaultRewardTuning.goldPerPoint;
             profile.tuning.materialsPerPoint = defaultRewardTuning.materialsPerPoint;
             profile.tuning.defeatHopeLossPerPoint = defaultRewardTuning.defeatHopeLossPerPoint;
             profile.tuning.defeatCivilLossPerPoint = defaultRewardTuning.defeatCivilLossPerPoint;
             profile.tuning.defeatOxLoss = defaultRewardTuning.defeatOxLoss;
             profile.tuning.victoryHopeBonus = defaultRewardTuning.victoryHopeBonus;
             profile.tuning.itemChancePerPoint = defaultRewardTuning.itemChancePerPoint;
             break;
       }
    }
 }

bool TryGenerarEncuentro(BattleEncounterType tipo, EncounterZoneType zona, Predicate<EnemyFactionConfig> factionFilter, out EncounterDefinition definition, int faseOverride = 0)
{
   definition = null;
   var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
   if (atributosZona == null)
    {
       return false;
    }

   int fase = faseOverride > 0 ? faseOverride : Mathf.Max(1, atributosZona.FASE);
  return EncounterGenerator.TryGenerateEncounter(atributosZona, zona, tipo, fase, out definition, factionFilter);
}

 bool EsNodoActualRitualKaleTav()
 {
    var manager = CampaignManager.Instance;
    if (manager == null || manager.scMapaManager == null || manager.scMapaManager.nodoActual == null)
    {
       return false;
    }

    return manager.scMapaManager.nodoActual.tipoNodo == 15;
 }

 bool EsFaccionCorrupta(string factionId)
 {
    if (string.IsNullOrWhiteSpace(factionId) || corruptFactionIds == null || corruptFactionIds.Count == 0)
    {
       return false;
    }

    foreach (var id in corruptFactionIds)
    {
       if (!string.IsNullOrWhiteSpace(id) && string.Equals(id, factionId, StringComparison.OrdinalIgnoreCase))
       {
          return true;
       }
    }
    return false;
 }

 bool EsFaccionDeLista(EnemyFactionConfig faction, List<string> ids)
 {
    if (faction == null || ids == null || ids.Count == 0)
    {
       return false;
    }

    foreach (var id in ids)
    {
       if (!string.IsNullOrWhiteSpace(id) && string.Equals(id, faction.factionId, StringComparison.OrdinalIgnoreCase))
       {
          return true;
       }
    }
    return false;
 }

 bool DeberiaGenerarBatallaCorrupta()
 {
    if (corruptFactionIds == null || corruptFactionIds.Count == 0)
    {
       return false;
    }

    float aliento = CampaignManager.Instance != null ? CampaignManager.Instance.GetValorAlientoNegro() : 0f;
    float chance = Mathf.Clamp(aliento * corruptChancePerAliento, 0f, 100f);
    return UnityEngine.Random.Range(0f, 100f) < chance;
 }

 int ObtenerJefeIdAleatorio(EncounterZoneType zona, int fase)
 {
    List<int> ids = ObtenerIdsJefePorZonaYFase(zona, fase);
    if ((ids == null || ids.Count == 0) && fase != 1)
    {
        // Fallback: si la fase actual no tiene jefe configurado, usar el de fase 1 de la zona.
        ids = ObtenerIdsJefePorZonaYFase(zona, 1);
    }

    if (ids != null && ids.Count > 0)
    {
       return ids[UnityEngine.Random.Range(0, ids.Count)];
    }

    return 0;
 }

 List<int> ObtenerIdsJefePorZonaYFase(EncounterZoneType zona, int fase)
 {
    switch (zona)
    {
        case EncounterZoneType.PasoVientoHelado:
            if (fase == 1) return new List<int> { 60 };
            break;
        case EncounterZoneType.Nedukazal:
            if (fase == 1) return new List<int> { 100 };
            break;
        case EncounterZoneType.BosqueAngustiante:
        default:
            if (fase == 1) return new List<int> { 11 };
            break;
    }

    return null;
 }

 BattleRewardTuning GetRewardTuning(BattleEncounterType tipo)
 {
    if (rewardProfiles != null)
    {
       foreach (var profile in rewardProfiles)
       {
          if (profile != null && profile.battleType == tipo && profile.tuning != null)
          {
             return profile.tuning;
          }
       }
    }

    return defaultRewardTuning ?? new BattleRewardTuning();
 }

 int ObtenerMaxAliadosPermitidos()
 {
    // Si no se configuró aún, por defecto permitir todos los slots disponibles (máx. 4).
    int max = cantidadAliadosComienzo > 0 ? cantidadAliadosComienzo : 4;
    return Mathf.Clamp(max, 1, 4);
 }

 int ContarAliadosSeleccionados()
 {
    int count = 0;
    if (scAdministradorEscenas.Personaje1 != null) count++;
    if (scAdministradorEscenas.Personaje2 != null) count++;
    if (scAdministradorEscenas.Personaje3 != null) count++;
    if (scAdministradorEscenas.Personaje4 != null) count++;
    return count;
 }

 void AgregarSiNoEsta(List<Personaje> lista, Personaje pers)
 {
    if (pers != null && !lista.Contains(pers))
    {
       lista.Add(pers);
    }
 }

 void AjustarSeleccionAlMaximo()
 {
    AjustarSeleccionAlMaximo(ObtenerMaxAliadosPermitidos());
 }

 void AjustarSeleccionAlMaximo(int maxPermitidos)
 {
    int max = Mathf.Clamp(maxPermitidos, 1, 4);

    var seleccion = new List<Personaje>(4);
    AgregarSiNoEsta(seleccion, scAdministradorEscenas.Personaje1);
    AgregarSiNoEsta(seleccion, scAdministradorEscenas.Personaje2);
    AgregarSiNoEsta(seleccion, scAdministradorEscenas.Personaje3);
    AgregarSiNoEsta(seleccion, scAdministradorEscenas.Personaje4);

    if (seleccion.Count > max)
    {
       seleccion.RemoveRange(max, seleccion.Count - max);
    }

    scAdministradorEscenas.Personaje1 = seleccion.Count > 0 ? seleccion[0] : null;
    scAdministradorEscenas.Personaje2 = seleccion.Count > 1 ? seleccion[1] : null;
    scAdministradorEscenas.Personaje3 = seleccion.Count > 2 ? seleccion[2] : null;
    scAdministradorEscenas.Personaje4 = seleccion.Count > 3 ? seleccion[3] : null;
 }


 public void ActualizarLista()
{
   var tutorial = CampaignManager.Instance != null ? CampaignManager.Instance.scTutorialManager : null;
   int limiteVisual = ObtenerMaxAliadosPermitidos();
   if (tutorial != null && tutorial.tutorialActivo)
   {
       if (tutorial.pasoActual < 4)
       {
           limiteVisual = Mathf.Min(limiteVisual, 1);
       }
       else if (tutorial.pasoActual < 40)
       {
           limiteVisual = Mathf.Min(limiteVisual, 3);
       }
   }

   AjustarSeleccionAlMaximo(limiteVisual);

   int seleccionados = ContarAliadosSeleccionados();

        // Asumiendo que tienes un TextMeshProUGUI llamado txtSeleccionadosPersonajes
        if (txtSeleccionadosPersonajes != null)
        {
            if (tutorial != null && tutorial.tutorialActivo && tutorial.pasoActual < 4)
            {
                txtSeleccionadosPersonajes.text = $"{seleccionados}/1"; // Forzar a 1 durante el tutorial
            }
            else if (tutorial != null && tutorial.tutorialActivo && tutorial.pasoActual < 40)
            {
                txtSeleccionadosPersonajes.text = $"{seleccionados}/3";
            }
            else
            { 
                 txtSeleccionadosPersonajes.text = $"{seleccionados}/"+limiteVisual;

            }
    }
    // Verifica si todos los personajes son null
    bool todosVacios = scAdministradorEscenas.Personaje1 == null &&
                       scAdministradorEscenas.Personaje2 == null &&
                       scAdministradorEscenas.Personaje3 == null &&
                       scAdministradorEscenas.Personaje4 == null;

        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 50)
        {
            if (seleccionados > 2)
            { 
                  btnComenzar.SetActive(true);

            }else
            {
                btnComenzar.SetActive(false);
            }

        }
        else
        {
            // Configura el estado del botón basíndose en si hay personajes seleccionados o no
            btnComenzar.SetActive(!todosVacios);
        }
   

    foreach (Transform transform in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
            {
                Destroy(transform.gameObject);
            }

        if (!UIEmpezarBatallaACaravana.activeInHierarchy)
        {
            foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
            {
                if (!pers.Camp_Muerto)
                {
                    GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedorUIPersonajes.transform);
                    btnPers.GetComponent<Image>().sprite = pers.spRetrato;
                    btnPers.GetComponent<btnPersonaje>().personajeRepresentado = pers;
                }

            }
        }
        else //Si es ataque a la caravana, solo muestra los personajes haciendo guardia
        {
            foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
            {
                if (!pers.Camp_Muerto && pers.fVidaActual > 1) //Si no está muerto y tiene vida
                {
                    if ((/*Base: Guardia*/pers.ActividadSeleccionada == 3) || (/*Caballero: Vigilar*/pers.ActividadSeleccionada == 6))
                    {
                        GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedorUIPersonajes.transform);
                        btnPers.GetComponent<Image>().sprite = pers.spRetrato;
                        btnPers.GetComponent<btnPersonaje>().personajeRepresentado = pers;
                    }
                   
                }
            }
        }

        foreach (Transform child in contenedorUIPersonajes.transform)
        {
            // Intenta obtener el componente btnPersonaje del hijo
            btnPersonaje btn = child.GetComponent<btnPersonaje>();

            if (btn != null) // Asegúrate de que el componente btnPersonaje exista
            {
                btn.representarVida();

                //Marca los retratos de los personajes seleccionados
                if (scAdministradorEscenas.Personaje1 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(12).gameObject.SetActive(true);

                }
                else if (scAdministradorEscenas.Personaje2 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(12).gameObject.SetActive(true);
                }
                else if (scAdministradorEscenas.Personaje3 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(12).gameObject.SetActive(true);
                }
                else if (scAdministradorEscenas.Personaje4 == btn.personajeRepresentado)
                {
                    btn.transform.GetChild(12).gameObject.SetActive(true);
                }
                else { btn.transform.GetChild(12).gameObject.SetActive(false); }

            }


           
            btn.RepresentarTodo(); 


        }
   
   


 }
 
public void DejanEnListaParticipantesSolo()
 {
   
  foreach (Transform boton in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
   {
           btnPersonaje btn = boton.gameObject.GetComponent<btnPersonaje>();
           
            bool participo = false;

            if(btn.personajeRepresentado == scAdministradorEscenas.Personaje1)
            {participo = true;}
            if(btn.personajeRepresentado == scAdministradorEscenas.Personaje2)
            {participo = true;}
             if(btn.personajeRepresentado == scAdministradorEscenas.Personaje3)
            {participo = true;}
             if(btn.personajeRepresentado == scAdministradorEscenas.Personaje4)
            {participo = true;}
            
            if(!participo)
            {
             Destroy(boton.gameObject);
            }else{btn.representarVida();}
    }

  }

 public void Seleccionar(Personaje pers)
 {
     // Verificar si el personaje ya está seleccionado en alguna posición
    if (scAdministradorEscenas.Personaje1 == pers)
    {
        scAdministradorEscenas.Personaje1 = null;
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje2 == pers)
    {
        scAdministradorEscenas.Personaje2 = null;
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje3 == pers)
    {
        scAdministradorEscenas.Personaje3 = null;
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje4 == pers)
    {
        scAdministradorEscenas.Personaje4 = null;
        ActualizarLista();
        return;
    }
    int maxAliados = ObtenerMaxAliadosPermitidos();
    int seleccionadosActuales = ContarAliadosSeleccionados();
    if (seleccionadosActuales >= maxAliados)
    {
        return;
    }
    // Verificar si la primera posición está disponible
    if (scAdministradorEscenas.Personaje1 == null)
    {
        scAdministradorEscenas.Personaje1 = pers;
    }
    // Verificar si la segunda posición está disponible
    else if (scAdministradorEscenas.Personaje2 == null)
    {
        scAdministradorEscenas.Personaje2 = pers;
    }
    // Verificar si la tercera posición está disponible
    else if (scAdministradorEscenas.Personaje3 == null)
    {
        scAdministradorEscenas.Personaje3 = pers;
    }
    // Verificar si la cuarta posición está disponible
    else if (scAdministradorEscenas.Personaje4 == null)
    {
        scAdministradorEscenas.Personaje4 = pers;
    }
    // Si todas están ocupadas, reemplazar al cuarto scAdministradorEscenas.Personaje
    else
    {
        scAdministradorEscenas.Personaje4 = pers;
    }

    ActualizarLista();

   

    
 }
 [SerializeField] TextMeshProUGUI txtSeleccionadosPersonajes;
 public int esEmboscadaEnemiga;
  public int EventoBatallaID = 0;
    public int cantidadAliadosComienzo;
  public void EventoBatallaNormal(int n, int esEmboscada = 0)
    {
        esBatallaFinal = false;
        esEmboscadaEnemiga = esEmboscada;

        gameObject.SetActive(true);
        UIEmpezarBatalla.SetActive(true);
        UIEmpezarBatallaACaravana.SetActive(false);
        UITerminarBatalla.SetActive(false);

        cantidadAliadosComienzo = 3;
        AjustarSeleccionAlMaximo();

        encuentroGeneradoActual = null;
        encuentroTipoActual = BattleEncounterType.Normal;
        EventoBatallaID = n;

        var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
        EncounterZoneType zonaActual = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
        encuentroZonaActual = zonaActual;

        if (n == 0)
        {
            bool generado = false;
            float chanceEncuentroPropio = atributosZona != null ? atributosZona.GetChanceEncuentroPropio(zonaActual) : 70f;

            if (DeberiaGenerarBatallaCorrupta())
            {
                generado = TryGenerarEncuentro(BattleEncounterType.Normal, EncounterZoneType.Generico, f => EsFaccionDeLista(f, corruptFactionIds), out encuentroGeneradoActual);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!generado && UnityEngine.Random.Range(0f, 100f) < chanceEncuentroPropio)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.Normal, zonaActual, null, out encuentroGeneradoActual);
                if (generado)
                {
                    encuentroZonaActual = zonaActual;
                }
            }

            if (!generado)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.Normal, EncounterZoneType.Generico, null, out encuentroGeneradoActual);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!generado)
            {
                TryGenerarEncuentro(BattleEncounterType.Normal, zonaActual, null, out encuentroGeneradoActual);
                encuentroZonaActual = zonaActual;
            }

            EventoBatallaID = encuentroGeneradoActual != null ? 0 : EventoBatallaID;

            var tutorialManager = CampaignManager.Instance != null ? CampaignManager.Instance.scTutorialManager : null;
            if (tutorialManager != null && tutorialManager.tutorialActivo && tutorialManager.pasoActual == 3)
            {
                EventoBatallaID = 700; //Fuerza el encuentro del tutorial
                encuentroGeneradoActual = null; // No usar el encuentro procedural durante el tutorial
            }

        }
        else
        {
            encuentroGeneradoActual = null;
        }

        ActualizarLista();
    }
    public void EventoBatallaElite(int n, int esEmboscada = 0, bool forzarRitualKaleTav = false)
    {
        esBatallaFinal = false;
        esEmboscadaEnemiga = esEmboscada;
        cantidadAliadosComienzo = 3;
        AjustarSeleccionAlMaximo();
        gameObject.SetActive(true); 
        UIEmpezarBatalla.SetActive(true);
        UIEmpezarBatallaACaravana.SetActive(false);
        UITerminarBatalla.SetActive(false);

        encuentroGeneradoActual = null;
    encuentroTipoActual = BattleEncounterType.Elite;
    EventoBatallaID = n;

    var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
    EncounterZoneType zonaActual = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
    encuentroZonaActual = zonaActual;

    var tutorialManager = CampaignManager.Instance != null ? CampaignManager.Instance.scTutorialManager : null;
    bool esUltimoNodoTutorial = tutorialManager != null &&
                                tutorialManager.tutorialActivo &&
                                CampaignManager.Instance.scMapaManager != null &&
                                CampaignManager.Instance.scMapaManager.nodoActual == tutorialManager.Nodotut6;
    if (esUltimoNodoTutorial)
    {
        EventoBatallaID = 701; // Fuerza el encuentro final del tutorial
        encuentroGeneradoActual = null;
        ActualizarLista();
        return;
    }

    if (n == 0)
    {
        bool generado = false;
        float chanceEncuentroPropio = atributosZona != null ? atributosZona.GetChanceEncuentroPropio(zonaActual) : 70f;
        bool esRitualKaleTav = forzarRitualKaleTav || EsNodoActualRitualKaleTav();

            if (esRitualKaleTav)
            {
                Predicate<EnemyFactionConfig> kaleFilter = faction =>
                    faction != null &&
                    !string.IsNullOrWhiteSpace(faction.factionId) &&
                    string.Equals(faction.factionId, KaleTavFactionId, StringComparison.OrdinalIgnoreCase);

                generado = TryGenerarEncuentro(BattleEncounterType.Elite, zonaActual, kaleFilter, out encuentroGeneradoActual);
                if (!generado && TryGenerarEncuentro(BattleEncounterType.Elite, EncounterZoneType.Generico, kaleFilter, out encuentroGeneradoActual))
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                    generado = true;
                }
            }

            if (!generado)
            {
                bool intentarZonaPrimero = UnityEngine.Random.Range(0f, 100f) < chanceEncuentroPropio;
                if (intentarZonaPrimero)
                {
                    generado = TryGenerarEncuentro(BattleEncounterType.Elite, zonaActual, null, out encuentroGeneradoActual);
                    if (generado)
                    {
                        encuentroZonaActual = zonaActual;
                    }
                    else if (TryGenerarEncuentro(BattleEncounterType.Elite, EncounterZoneType.Generico, null, out encuentroGeneradoActual))
                    {
                        encuentroZonaActual = EncounterZoneType.Generico;
                        generado = true;
                    }
                }
                else
                {
                    generado = TryGenerarEncuentro(BattleEncounterType.Elite, EncounterZoneType.Generico, null, out encuentroGeneradoActual);
                    if (generado)
                    {
                        encuentroZonaActual = EncounterZoneType.Generico;
                    }
                    else if (TryGenerarEncuentro(BattleEncounterType.Elite, zonaActual, null, out encuentroGeneradoActual))
                    {
                        encuentroZonaActual = zonaActual;
                        generado = true;
                    }
                }
            }

            EventoBatallaID = generado && encuentroGeneradoActual != null ? 0 : EventoBatallaID;
        }
        else
        {
            encuentroGeneradoActual = null;
        }

        ActualizarLista();
    }
    public void EventoBatallaFinal(int n, int esEmboscada = 0)
    {
        esBatallaFinal = true;
        esEmboscadaEnemiga = esEmboscada;
        cantidadAliadosComienzo = 4;
        AjustarSeleccionAlMaximo();
        gameObject.SetActive(true);
        UIEmpezarBatalla.SetActive(true);
        UIEmpezarBatallaACaravana.SetActive(false);
        UITerminarBatalla.SetActive(false);
        if (txtMilicianosDisponibles != null)
        {
            txtMilicianosDisponibles.text = TRADU.i.Traducir("Milicianos disponibles: ") + (int)CampaignManager.Instance.GetMiliciasActual() / 10;
        }

        encuentroGeneradoActual = null;
        encuentroTipoActual = BattleEncounterType.Normal;
        var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
        encuentroZonaActual = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;

        if (n == 0)
        {
            int faseActual = atributosZona != null ? Mathf.Max(1, atributosZona.FASE) : 1;
            int jefeId = ObtenerJefeIdAleatorio(encuentroZonaActual, faseActual);
            if (jefeId != 0)
            {
                EventoBatallaID = jefeId;
            }
        }
        else
        {
            EventoBatallaID = n;
        }

        ActualizarLista();
    }
    public void EventoBatallaCaravana(int n, int esEmboscada = 3/*3 es Ataque a Caravana*/)
    {
        esBatallaFinal = false;
        esEmboscadaEnemiga = esEmboscada;
        cantidadAliadosComienzo = 4;
        AjustarSeleccionAlMaximo();

        gameObject.SetActive(true);
        UIEmpezarBatallaACaravana.SetActive(true);
        UIEmpezarBatalla.SetActive(false);
        UITerminarBatalla.SetActive(false);

        if (txtMilicianosDisponibles != null)
        {
            txtMilicianosDisponibles.text = TRADU.i.Traducir("Milicianos disponibles: ") + (int)CampaignManager.Instance.GetMiliciasActual() / 10;
        }

        scAdministradorEscenas.Personaje1 = null;
        scAdministradorEscenas.Personaje2 = null;
        scAdministradorEscenas.Personaje3 = null;
        scAdministradorEscenas.Personaje4 = null;

        int maxAliados = ObtenerMaxAliadosPermitidos();
        int asignados = 0;
        foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
        {
            if (asignados >= maxAliados) break;
            if (pers == null) continue;
            if (pers.Camp_Muerto) continue;
            if (pers.fVidaActual <= 1) continue;
            if (!((pers.ActividadSeleccionada == 3) || (pers.ActividadSeleccionada == 6))) continue;

            asignados++;
            if (asignados == 1) scAdministradorEscenas.Personaje1 = pers;
            else if (asignados == 2) scAdministradorEscenas.Personaje2 = pers;
            else if (asignados == 3) scAdministradorEscenas.Personaje3 = pers;
            else if (asignados == 4) scAdministradorEscenas.Personaje4 = pers;
        }

        encuentroGeneradoActual = null;
        encuentroTipoActual = BattleEncounterType.AtaqueCaravana;
        EventoBatallaID = n;

        var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
        EncounterZoneType zonaActual = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
        encuentroZonaActual = zonaActual;

        if (n == 0)
        {
            bool generado = false;
            float chanceEncuentroPropio = atributosZona != null ? atributosZona.GetChanceEncuentroPropio(zonaActual) : 70f;

            if (DeberiaGenerarBatallaCorrupta())
            {
                generado = TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, EncounterZoneType.Generico, f => EsFaccionDeLista(f, corruptFactionIds), out encuentroGeneradoActual);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!generado && UnityEngine.Random.Range(0f, 100f) < chanceEncuentroPropio)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, zonaActual, null, out encuentroGeneradoActual);
                if (generado)
                {
                    encuentroZonaActual = zonaActual;
                }
            }

            if (!generado)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, EncounterZoneType.Generico, null, out encuentroGeneradoActual);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!generado)
            {
                TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, zonaActual, null, out encuentroGeneradoActual);
                encuentroZonaActual = zonaActual;
            }

            EventoBatallaID = encuentroGeneradoActual != null ? 0 : EventoBatallaID;
        }
        else
        {
            encuentroGeneradoActual = null;
        }

        ActualizarLista();
    } public void EventoBatallaSubterranea(int fase)
  {
    esBatallaFinal = false; // batalla especial pero no es la final de jefe
    esEmboscadaEnemiga = 1; // Ataque subterraneo siempre emboscada enemiga
    cantidadAliadosComienzo = 4;
    AjustarSeleccionAlMaximo();
    gameObject.SetActive(true);
    UIEmpezarBatalla.SetActive(true);
    UIEmpezarBatallaACaravana.SetActive(false);
    UITerminarBatalla.SetActive(false);

    if (txtMilicianosDisponibles != null)
    {
        txtMilicianosDisponibles.text = TRADU.i.Traducir("Milicianos disponibles: ")  + (int)CampaignManager.Instance.GetMiliciasActual() / 10;
    }

    encuentroGeneradoActual = null;
    encuentroTipoActual = BattleEncounterType.Subterraneo;
    encuentroZonaActual = EncounterZoneType.Subterraneo;
    EventoBatallaID = 0;

    int faseClamped = Mathf.Max(1, fase);
    if (!TryGenerarEncuentro(BattleEncounterType.Subterraneo, EncounterZoneType.Subterraneo, null, out encuentroGeneradoActual, faseClamped))
    {
        var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
        EncounterZoneType zonaFallback = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
        encuentroZonaActual = zonaFallback;
        TryGenerarEncuentro(BattleEncounterType.Subterraneo, zonaFallback, null, out encuentroGeneradoActual, faseClamped);
    }

    ActualizarLista();
} public void EfectosDeBatallaEnCampaña(int resultado)
{
   UIEmpezarBatalla.SetActive(false);
   UITerminarBatalla.SetActive(true);
   transicionJefeZonaPendiente = false;
   bool fueBatallaFinalActual = esBatallaFinal;
   bool fueDefensaCaravana = encuentroTipoActual == BattleEncounterType.AtaqueCaravana || esEmboscadaEnemiga == 3;

    if (resultado == 1)
    {
        txtVictoria.SetActive(true);
        txtDerrota.SetActive(false);
    }
    else
    {
        txtVictoria.SetActive(false);
        txtDerrota.SetActive(true);
    }

    int aumentochancesitem = 0;

   if (encuentroGeneradoActual != null)
   {
        ProcesarEncuentroGenerado(resultado, ref aumentochancesitem);
   }
   else
    {
        ProcesarEncuentroLegacy(resultado, ref aumentochancesitem);
    }

   if (resultado == 1)
   {
      AplicarVictoriaRitualKaleTav();
      if (esBatallaFinal && CampaignManager.Instance != null)
      {
         transicionJefeZonaPendiente = true;
      }
      else if (CampaignManager.Instance.scTutorialManager.tutorialActivo && EventoBatallaID == 701)
      {
         CampaignManager.Instance.scTutorialManager.SiguientePaso();
         CampaignManager.Instance.AbrirCiudadPuerto(); //fin tutorial
      }
   }
    else if (resultado == 2 && CampaignManager.Instance != null)
    {
        CampaignManager.Instance.EvaluarDerrotaPorResultadoBatalla(fueDefensaCaravana, fueBatallaFinalActual);
    }
    //Al perder se tiran chances de eliminar sequito. 50% -10% por tier mejora Defensa
    if (resultado == 2) //Al perder se tiran chances de eliminar sequito. 50% -10% por tier mejora Defensa
    {
        int rand =UnityEngine.Random.Range(1, 101);
        int prob = 50 - CampaignManager.Instance.mejoraCaravanaDefensas*10;
        if (rand < prob) // chances de perder un sequito al perder una pelea
            {
                if (CampaignManager.Instance.scMenuSequito.SequitoAlAzarPerdido(out string nombre))
                {
                    txtRecompensa.text += TRADU.i.Traducir("\n\n-Los enemigos han eliminado al ") + nombre + TRADU.i.Traducir(" luego de la Batalla.");
                }
            }
        
    }
    
    //Al ganar puede tocar un item de la lista total del sequito de mercaderes al azar
        if (resultado == 1) //Al ganar puede tocar un item de la lista total del sequito de mercaderes al azar
        {
            int rand =UnityEngine.Random.Range(1, 101);
            aumentochancesitem += CampaignManager.Instance.mejoraCaravanaCatalejos*5;
            int prob = 30+aumentochancesitem; //!! 30%
            if (rand < prob) // chances de perder un sequito al perder una pelea
            {
                Item recompensa = CampaignManager.Instance.scMenuSequito.Sequito003Mercaderes.GetComponent<SequitoMercaderes>().ObtenerItemAlAzar();
                CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(recompensa.gameObject);
                txtRecompensa.text += TRADU.i.Traducir("\n\n- Has encontrado un objeto de recompensa: ") + TRADU.i.Traducir(recompensa.sNombreItem) + ".";

            }

        }

    AdministrarHeridas(scAdministradorEscenas.Personaje1, scAdministradorEscenas.unidadPers1);
    AdministrarHeridas(scAdministradorEscenas.Personaje2, scAdministradorEscenas.unidadPers2);
    AdministrarHeridas(scAdministradorEscenas.Personaje3, scAdministradorEscenas.unidadPers3);
    AdministrarHeridas(scAdministradorEscenas.Personaje4, scAdministradorEscenas.unidadPers4);
    //Cronistas
    if (CampaignManager.Instance.scMenuSequito.TieneSequito(7)) //Cronistas -- Diferenciar cuando esten todas las batallas elite etc. para que de mas.
    {
        if (!CampaignManager.Instance.scSequitoCronistas.yaVendioCronica)
        {
            if (resultado == 1) //Victoria
            {
                CampaignManager.Instance.scSequitoCronistas.valorCambiosCronicas += 50;
                CampaignManager.Instance.CambiarEsperanzaActual(+5);
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Cronistas han registrado la victoria, +50 Valor Crónica, +5 Esperanza."));
            }
            else //Derrota
            {
                CampaignManager.Instance.scSequitoCronistas.valorCambiosCronicas -= 50;
                CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Los Cronistas han registrado la derrota, -50 Valor Crónica. -3 Esperanza."));
                CampaignManager.Instance.CambiarEsperanzaActual(-3);

            }
        }
    }
         

   //Resetear
            EventoBatallaID = 0;
            esBatallaFinal = false;
}

 void DarExperiencia(int cant)
 {
    if( scAdministradorEscenas.Personaje1 != null)
   { scAdministradorEscenas.Personaje1.RecibirExperiencia(cant);}
    if( scAdministradorEscenas.Personaje2 != null)
   { scAdministradorEscenas.Personaje2.RecibirExperiencia(cant);}
    if( scAdministradorEscenas.Personaje3 != null)
   { scAdministradorEscenas.Personaje3.RecibirExperiencia(cant);}
    if( scAdministradorEscenas.Personaje4 != null)
   { scAdministradorEscenas.Personaje4.RecibirExperiencia(cant);}
 }

    public void AdministrarHeridas(Personaje pers, Unidad uni)
    {
       
        if (pers != null)
        {
            if (pers.fVidaActual < 1)
            {
                if (pers.Camp_Herido) //Si ya estaba herido
                {
                    //muere
                    pers.Camp_Muerto = true;
                }
                else //Si no estaba herido, hiere
                {
                    pers.Camp_Herido = true;
                    pers.fVidaActual = 5;
                }

                if (uni.loMatoCorrompido) //Si lo mató un corrupto, se marca como corrupto
                {

                    if (pers.Camp_Corrupto)
                    {
                         //muere
                         pers.Camp_Muerto = true;

                    }
                    else
                    {
                        pers.Camp_Corrupto = true;
                        CampaignManager.Instance.EscribirLog("-" + uni.uNombre + TRADU.i.Traducir(" ha sido corrompido."));


                    }
                   
                    
                }

            }

        
           

            //Actualiza estados visuales herida y muerte
            foreach (Transform boton in contenedorUIPersonajes.transform)
            {
                btnPersonaje btn = boton.gameObject.GetComponent<btnPersonaje>();

                if (btn.personajeRepresentado.Camp_Herido)
                {
                    boton.transform.GetChild(3).gameObject.SetActive(true);
                }
                else { boton.transform.GetChild(3).gameObject.SetActive(false); }

                if (btn.personajeRepresentado.Camp_Muerto)
                {
                    boton.transform.GetChild(3).gameObject.SetActive(false);
                    boton.transform.GetChild(4).gameObject.SetActive(true);
                }
                else { boton.transform.GetChild(4).gameObject.SetActive(false); }


            }

        }
    }

 public void CerrarMenuBatalla()
    {
      if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 4)
      {
        CampaignManager.Instance.scTutorialManager.SiguientePaso();
      }
        bool debeResolverTransicionJefeZona = transicionJefeZonaPendiente;
        transicionJefeZonaPendiente = false;
        gameObject.SetActive(false);

        if (debeResolverTransicionJefeZona && CampaignManager.Instance != null)
        {
          CampaignManager.Instance.OnDerrotadoJefeZona();
        }
    }
 public void ComenzarBatalla()
 {
    if (bloqueoComenzarBatalla)
    {
      return;
    }

    bloqueoComenzarBatalla = true;
    SetBotonComenzarInteractable(false);

    if (scAdministradorEscenas == null)
    {
      bloqueoComenzarBatalla = false;
      SetBotonComenzarInteractable(true);
      return;
    }

    AjustarSeleccionAlMaximo();
    // Autorelleno para Ataque a Caravana: prioriza personajes con m1s vida
    if (UIEmpezarBatallaACaravana != null && UIEmpezarBatallaACaravana.activeInHierarchy)
    {
        esEmboscadaEnemiga = 3; // Marcar tipo Ataque a Caravana
        // Reunir seleccionados actuales
        HashSet<Personaje> seleccionados = new HashSet<Personaje>();
        if (scAdministradorEscenas.Personaje1 != null) seleccionados.Add(scAdministradorEscenas.Personaje1);
        if (scAdministradorEscenas.Personaje2 != null) seleccionados.Add(scAdministradorEscenas.Personaje2);
        if (scAdministradorEscenas.Personaje3 != null) seleccionados.Add(scAdministradorEscenas.Personaje3);
        if (scAdministradorEscenas.Personaje4 != null) seleccionados.Add(scAdministradorEscenas.Personaje4);

        int maxAliadosPermitidos = ObtenerMaxAliadosPermitidos();
        int countSel = seleccionados.Count;
        if (countSel < maxAliadosPermitidos)
        {
            // Candidatos: vivos, con vida > 1, no repetidos
            List<Personaje> candidatos = new List<Personaje>();
            foreach (Personaje p in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
            {
                if (p == null) continue;
                if (p.Camp_Muerto) continue;
                if (p.fVidaActual <= 1) continue;
                if (seleccionados.Contains(p)) continue;
                candidatos.Add(p);
            }
            // Ordenar por vida actual desc, luego vida mbxima desc
            candidatos.Sort((a, b) => {
                int cmp = b.fVidaActual.CompareTo(a.fVidaActual);
                if (cmp == 0) cmp = b.fVidaMaxima.CompareTo(a.fVidaMaxima);
                return cmp;
            });

            if (scAdministradorEscenas.PersonajesSorprendidosInicioCaravana == null)
            {
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana = new List<Personaje>();
            }

            // Rellenar huecos en orden 1..4 respetando el límite
            if (maxAliadosPermitidos >= 1 && scAdministradorEscenas.Personaje1 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje1 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
            if (maxAliadosPermitidos >= 2 && scAdministradorEscenas.Personaje2 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje2 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
            if (maxAliadosPermitidos >= 3 && scAdministradorEscenas.Personaje3 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje3 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
            if (maxAliadosPermitidos >= 4 && scAdministradorEscenas.Personaje4 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje4 = p;
                scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(p);
            }
        }
    }

    bool usarRefuerzosAliadosCaravana = esBatallaFinal;
    if (encuentroGeneradoActual != null)
    {
        scAdministradorEscenas.CargarBatalla(EventoBatallaID, esEmboscadaEnemiga, encuentroGeneradoActual, usarRefuerzosAliadosCaravana);
    }
    else
    {
        scAdministradorEscenas.CargarBatalla(EventoBatallaID, esEmboscadaEnemiga, null, usarRefuerzosAliadosCaravana);
    }
    
 }


 void ProcesarEncuentroGenerado(int resultado, ref int aumentochancesitem)
 {
    if (encuentroGeneradoActual == null)
    {
        return;
    }

    var tuning = GetRewardTuning(encuentroTipoActual);
    int totalPoints = Mathf.Max(1, encuentroGeneradoActual.totalBudget);

    if (resultado == 1)
    {
        int exp = Mathf.RoundToInt(totalPoints * tuning.expPerPoint);
        int oro = Mathf.RoundToInt(totalPoints * tuning.goldPerPoint);
        int materiales = Mathf.RoundToInt(totalPoints * tuning.materialsPerPoint);
        int hopeBonus = Mathf.RoundToInt(tuning.victoryHopeBonus);

        if (oro != 0) CampaignManager.Instance.CambiarOroActual(oro);
        if (materiales != 0) CampaignManager.Instance.CambiarMaterialesActuales(materiales);
        if (exp > 0) DarExperiencia(exp);
        if (hopeBonus != 0) CampaignManager.Instance.CambiarEsperanzaActual(hopeBonus);

        if (txtRecompensa != null)
        {
            txtRecompensa.text = FormatearTextoVictoria(exp, oro, materiales, hopeBonus);
        }

        aumentochancesitem += Mathf.RoundToInt(totalPoints * tuning.itemChancePerPoint);
        RegistrarLogEncuentro(resultado, exp, oro, materiales);
    }
    else
    {
        int hopeLoss = Mathf.RoundToInt(totalPoints * tuning.defeatHopeLossPerPoint);
        int civiliansLoss = Mathf.RoundToInt(totalPoints * tuning.defeatCivilLossPerPoint);
        int oxLoss = encuentroTipoActual == BattleEncounterType.AtaqueCaravana ? tuning.defeatOxLoss : 0;

        if (hopeLoss > 0) CampaignManager.Instance.CambiarEsperanzaActual(-hopeLoss);
        if (civiliansLoss > 0) CampaignManager.Instance.CambiarCivilesActuales(-civiliansLoss);
        if (oxLoss > 0) CampaignManager.Instance.CambiarBueyesActuales(-oxLoss);

        if (txtRecompensa != null)
        {
            txtRecompensa.text = FormatearTextoDerrota(hopeLoss, civiliansLoss, oxLoss);
        }

        RegistrarLogEncuentro(resultado, 0, 0, 0);

        if (EsFaccionCorrupta(encuentroGeneradoActual.factionId))
        {
            CampaignManager.Instance.CambiarValorAlientoNegro(2);
        }
    }
 }

void ProcesarEncuentroLegacy(int resultado, ref int aumentochancesitem)
{
   if (EventoBatallaID == 700 || EventoBatallaID == 701)
   {
       int deltaEsperanzaTutorial = resultado == 1 ? +5 : -5;

       if (resultado == 1)
       {
            const int exp = 180;
            const int oro = 150;

            CampaignManager.Instance.CambiarOroActual(oro);
            DarExperiencia(exp);

            if (txtRecompensa != null)
            {
                txtRecompensa.text = FormatearTextoVictoria(exp, oro, 0, deltaEsperanzaTutorial);
            }
        }
        else
        {
            if (txtRecompensa != null)
            {
                txtRecompensa.text = FormatearTextoDerrota(-deltaEsperanzaTutorial, 0, 0);
            }
        }

        CampaignManager.Instance.CambiarEsperanzaActual(deltaEsperanzaTutorial);
        return;
    }

    if (txtRecompensa != null)
    {
        txtRecompensa.text = resultado == 1
            ? TRADU.i.Traducir("Victoria sin recompensas definidas para este encuentro clásico.")
            : TRADU.i.Traducir("Derrota en un encuentro clásico. Los efectos específicos aún no están configurados.");
    }

    int deltaEsperanza = resultado == 1 ? +5 : -5;
    CampaignManager.Instance.CambiarEsperanzaActual(deltaEsperanza);
 }

 string FormatearTextoVictoria(int exp, int oro, int materiales, int hopeBonus)
 {
    List<string> partes = new List<string>();

    if (exp > 0) partes.Add(exp + (TRADU.i.nIdioma == 2 ? " Exp" : " Exp"));
    if (oro > 0) partes.Add(oro + (TRADU.i.nIdioma == 2 ? " Gold" : " Oro"));
    if (materiales > 0) partes.Add(materiales + (TRADU.i.nIdioma == 2 ? " Materials" : " Materiales"));

    string contenido = partes.Count > 0 ? string.Join(", ", partes) : TRADU.i.Traducir("sin botón");
    string texto = TRADU.i.nIdioma == 2
        ? contenido + " obtained."
        : TRADU.i.Traducir("Se han obtenido ") + contenido + ".";

    if (hopeBonus > 0)
    {
        texto += TRADU.i.nIdioma == 2 ? $" +{hopeBonus} Hope." : $" +{hopeBonus} " + TRADU.i.Traducir("Esperanza.");
    }

    return texto;
 }

 string FormatearTextoDerrota(int hopeLoss, int civilLoss, int oxLoss)
 {
    List<string> partesEs = new List<string>();
    List<string> partesEn = new List<string>();

    if (civilLoss > 0)
    {
        partesEs.Add(civilLoss + " Civiles");
        partesEn.Add(civilLoss + " Civilians");
    }
    if (hopeLoss > 0)
    {
        partesEs.Add("-" + hopeLoss + " Esperanza");
        partesEn.Add("-" + hopeLoss + " Hope");
    }
    if (oxLoss > 0)
    {
        partesEs.Add(oxLoss + " Bueyes");
        partesEn.Add(oxLoss + " Oxen");
    }

    if (partesEs.Count == 0)
    {
        partesEs.Add(TRADU.i.Traducir("Sin consecuencias graves."));
        partesEn.Add("No major consequences.");
    }

    string textoEs = TRADU.i.Traducir("Derrota: ") + string.Join(", ", partesEs) + ".";
    string textoEn = "Defeat: " + string.Join(", ", partesEn) + ".";

    return TRADU.i.nIdioma == 2 ? textoEn : textoEs;
 }

 void RegistrarLogEncuentro(int resultado, int exp, int oro, int materiales)
 {
    if (CampaignManager.Instance == null || encuentroGeneradoActual == null)
    {
        return;
    }

    string faccion = !string.IsNullOrWhiteSpace(encuentroGeneradoActual.factionName)
        ? encuentroGeneradoActual.factionName
        : encuentroGeneradoActual.factionId;
    string tipo = encuentroTipoActual.ToString();

    if (resultado == 1)
    {
       // CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Victoria contra ") + faccion + $" ({tipo})");
    }
    else
    {
        //CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Derrota frente a ") + faccion + $" ({tipo})");
    }
 }

 void AplicarVictoriaRitualKaleTav()
 {
    var manager = CampaignManager.Instance;
    if (manager == null || manager.scMapaManager == null)
    {
        return;
    }

    var nodoActual = manager.scMapaManager.nodoActual;
    if (nodoActual == null || nodoActual.tipoNodo != 15)
    {
        return;
    }

    if (nodoActual.nodoRitual)
    {
        nodoActual.DesactivarRitual();
    }
    nodoActual.nodoRitual = false;

    manager.CambiarEsperanzaActual(10);
    manager.EscribirLog(TRADU.i.Traducir("-El ritual Kale'Tav ha sido detenido. +10 Esperanza."));
 }
}



