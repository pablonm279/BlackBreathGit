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

public enum TipoRefuerzoAliadoCaravana
{
   Personaje,
   UnidadPrefab
}

public class RefuerzoAliadoCaravanaOrdenItem
{
   public string id;
   public TipoRefuerzoAliadoCaravana tipo;
   public Personaje personaje;
   public GameObject prefabRefuerzo;
   public string nombreVisible;
   public Sprite retrato;

   public bool EsPersonaje => tipo == TipoRefuerzoAliadoCaravana.Personaje && personaje != null;
   public bool EsUnidadPrefab => tipo == TipoRefuerzoAliadoCaravana.UnidadPrefab && prefabRefuerzo != null;
}

public class MenuBatallas : MonoBehaviour
{
 const string TooltipPersonajeHeridoId = "campania_personaje_herido";
 const string TooltipPersonajeCorruptoId = "campania_personaje_corrupto";
 const string TooltipPersonajeFatigadoBatallaLargaId = "campania_personaje_fatigado_batalla_larga";
 const string ItemRewardNombre = "ItemRecomp";
 const int BonusExperienciaPrimeraBatallaRun = 60;
 const int ModificadorPoolEnemigoPrimeraBatallaRun = -3;

 GameObject itemReward;
 static readonly TipoEstadoCaravana[] EstadosNegativosDerrotaBatalla =
 {
    TipoEstadoCaravana.Acobardados,
    TipoEstadoCaravana.Aletargados,
    TipoEstadoCaravana.Desmotivacion,
    TipoEstadoCaravana.Descuidados
 };
 public GameObject prefabBtnPersonaje; 
 public GameObject contenedorUIPersonajes; 
 public GameObject contenedorUIPersonajesFuera; 

 public AdministradorEscenas scAdministradorEscenas;
 List<RefuerzoAliadoCaravanaOrdenItem> ordenRefuerzosCaravana = new List<RefuerzoAliadoCaravanaOrdenItem>();

 [Header("Generador de encuentros")]
 [SerializeField] List<string> corruptFactionIds = new List<string>() { "Corruptos" };
 [SerializeField] List<string> banditFactionIds = new List<string>() { "Bandidos" };
 [SerializeField] float corruptChancePerAliento = 4f;
 [Header("Debug de encuentros")]
 [SerializeField] bool debugRestringirEncuentrosProcedurales = false;
 [SerializeField] List<string> debugEncounterFactionIds = new List<string>() { "Corruptos", "Bandidos", "Vengadores de Kadryn" };
 [SerializeField] List<BattleRewardProfile> rewardProfiles = new List<BattleRewardProfile>();
 [SerializeField] BattleRewardTuning defaultRewardTuning = new BattleRewardTuning();
 const string KaleTavFactionId = "Kale'Tav";
 const string VengadoresKadrynFactionId = "Vengadores de Kadryn";
 const int EventoEspecialDefenderCivilesBandidos = -206;

  EncounterDefinition encuentroGeneradoActual;
  EncounterZoneType encuentroZonaActual;
  BattleEncounterType encuentroTipoActual;
  bool esBatallaFinal = false;
  bool transicionJefeZonaPendiente = false;
  bool tutorialFinalPostBatallaPendiente = false;
  bool tooltipPersonajeHeridoPendiente = false;
  bool tooltipPersonajeCorruptoPendiente = false;
  bool tooltipPersonajeFatigadoBatallaLargaPendiente = false;

 public GameObject UIEmpezarBatalla;
 public GameObject UIEmpezarBatallaACaravana;
 public GameObject UITerminarBatalla;
 public GameObject txtVictoria;
 public GameObject txtDerrota;
 public TextMeshProUGUI txtRecompensa;
 public TextMeshProUGUI faccionBatalla;
 public TextMeshProUGUI txtPoderenemigo;
 public TextMeshProUGUI txtPoderaliado;

 public TextMeshProUGUI txtMilicianosDisponibles;

 [SerializeField] GameObject btnComenzar;
 bool bloqueoComenzarBatalla = false;
 int idiomaUIBatallaCache = int.MinValue;

 static readonly Dictionary<string, string> TraduccionesFaccionEn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
 {
    { "Bandidos", "Bandits" },
    { "Corruptos", "Corrupted" },
    { "Criaturas del Bosque", "Forest Creatures" },
    { "Criaturas del Paso", "Pass Creatures" },
    { "Etereos", "Ethereals" },
    { "Kale'Tav", "Kale'Tav" },
    { "Lobos del Bosque", "Forest Wolves" },
    { "Vagranilo", "Vagranilo" },
    { "Vagranilos", "Vagranilos" },
    { "Vengadores de Kadryn", "Kadryn Avengers" },
    { "Zarkil", "Zarkil" }
 };

 static readonly Dictionary<string, string> TraduccionesFaccionPtBr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
 {
    { "Bandidos", "Bandidos" },
    { "Corruptos", "Corrompidos" },
    { "Criaturas del Bosque", "Criaturas da Floresta" },
    { "Criaturas del Paso", "Criaturas do Paso" },
    { "Etereos", "Etéreos" },
    { "Kale'Tav", "Kale'Tav" },
    { "Lobos del Bosque", "Lobos da Floresta" },
    { "Vagranilo", "Vagranilo" },
    { "Vagranilos", "Vagranilos" },
    { "Vengadores de Kadryn", "Vingadores de Kadryn" },
    { "Zarkil", "Zarkil" }
 };

 void OnEnable()
 {
    bloqueoComenzarBatalla = false;
    SetBotonComenzarInteractable(true);
    RefrescarTextosUIBatallaSegunIdioma();
 }

 void Update()
 {
    RefrescarTextosUIBatallaSegunIdioma();
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

  static bool EsGuardiaParaCaravana(Personaje personaje)
  {
    if (personaje == null || !personaje.PuedeRealizarActividades())
    {
      return false;
    }

    return personaje.ActividadSeleccionada == 3
      || personaje.ActividadSeleccionada == 6
      || personaje.ActividadSeleccionada == 14;
  }

  void AplicarTraitsVictoriaParticipantes()
  {
    if (CampaignManager.Instance == null)
    {
      return;
    }

    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
    HashSet<Personaje> participantes = new HashSet<Personaje>()
    {
      scAdministradorEscenas != null ? scAdministradorEscenas.Personaje1 : null,
      scAdministradorEscenas != null ? scAdministradorEscenas.Personaje2 : null,
      scAdministradorEscenas != null ? scAdministradorEscenas.Personaje3 : null,
      scAdministradorEscenas != null ? scAdministradorEscenas.Personaje4 : null
    };

    foreach (Personaje pers in participantes)
    {
      if (pers == null || pers.Camp_Muerto || !pers.TieneRasgo(PersonajeTraitCatalog.TraitAdmirado))
      {
        continue;
      }

      CampaignManager.Instance.CambiarEsperanzaActual(5);
      string mensajeAdmirado = idiomaTrait switch
      {
        TRADU.IdiomaIngles => pers.sNombre + " turns victory into a rallying cry. +5 Hope.",
        TRADU.IdiomaPortugues => pers.sNombre + " transforma a vitória em inspiração. +5 de Esperança.",
        _ => pers.sNombre + " convierte la victoria en inspiración. +5 Esperanza."
      };
      CampaignManager.Instance.EscribirLog("-" + mensajeAdmirado);
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

bool TryGenerarEncuentro(BattleEncounterType tipo, EncounterZoneType zona, Predicate<EnemyFactionConfig> factionFilter, out EncounterDefinition definition, int faseOverride = 0, bool aplicarAjustePrimeraBatallaRun = false)
{
   definition = null;
   var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
   if (atributosZona == null)
    {
       return false;
    }

   int fase = faseOverride > 0 ? faseOverride : Mathf.Max(1, atributosZona.FASE);
   Predicate<EnemyFactionConfig> filtroEfectivo = CombinarFiltroFacciones(factionFilter);
   int budgetModifier = aplicarAjustePrimeraBatallaRun ? ObtenerModificadorPoolEnemigoPrimeraBatallaRun() : 0;

   if (EstaActivoDebugFaccionesCombate() && tipo == BattleEncounterType.Subterraneo)
   {
      if (EncounterGenerator.TryGenerateEncounter(atributosZona, EncounterZoneType.Generico, BattleEncounterType.Normal, fase, out definition, filtroEfectivo, budgetModifier))
      {
         definition.zoneType = EncounterZoneType.Subterraneo;
         definition.battleType = BattleEncounterType.Subterraneo;
         return true;
      }
   }

  return EncounterGenerator.TryGenerateEncounter(atributosZona, zona, tipo, fase, out definition, filtroEfectivo, budgetModifier);
}

int ObtenerModificadorPoolEnemigoPrimeraBatallaRun()
{
   CampaignManager campaignManager = CampaignManager.Instance;
   if (campaignManager == null)
   {
      return 0;
   }

   return campaignManager.ObtenerEstadisticaBatallasLibradas() == 0
      ? ModificadorPoolEnemigoPrimeraBatallaRun
      : 0;
}

public bool TryGenerarFaccionScout(Nodo nodo, out string factionId, out string factionName)
{
   factionId = "";
   factionName = "";
   if (nodo == null)
   {
      return false;
   }

   var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
   EncounterZoneType zonaActual = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
   BattleEncounterType tipo = ObtenerTipoEncuentroScout(nodo);

   Predicate<EnemyFactionConfig> filtro = null;
   if (nodo.tipoNodo == 15 && nodo.nodoRitual)
   {
      filtro = faction =>
         faction != null &&
         !string.IsNullOrWhiteSpace(faction.factionId) &&
         string.Equals(faction.factionId, KaleTavFactionId, StringComparison.OrdinalIgnoreCase);
   }

   if (!TryGenerarEncuentro(tipo, zonaActual, filtro, out EncounterDefinition definition) &&
       !TryGenerarEncuentro(tipo, EncounterZoneType.Generico, filtro, out definition))
   {
      return false;
   }

   factionId = definition != null ? definition.factionId : "";
   factionName = definition != null ? ObtenerNombreFaccionTraducido(definition.factionId, definition.factionName) : "";
   return !string.IsNullOrWhiteSpace(factionId);
}

BattleEncounterType ObtenerTipoEncuentroScout(Nodo nodo)
{
   if (nodo == null)
   {
      return BattleEncounterType.Normal;
   }

   switch (nodo.tipoNodo)
   {
      case 8:
      case 15:
         return BattleEncounterType.Elite;
      case 11:
         return BattleEncounterType.AtaqueCaravana;
      default:
         return BattleEncounterType.Normal;
   }
}

Predicate<EnemyFactionConfig> ObtenerFiltroFaccionScoutNodoActual()
{
   Nodo nodoActual = CampaignManager.Instance != null && CampaignManager.Instance.scMapaManager != null
      ? CampaignManager.Instance.scMapaManager.nodoActual
      : null;
   string factionId = nodoActual != null ? nodoActual.ObtenerFaccionScoutReveladaId() : "";

   if (string.IsNullOrWhiteSpace(factionId))
   {
      return null;
   }

   return faction =>
      faction != null &&
      !string.IsNullOrWhiteSpace(faction.factionId) &&
      string.Equals(faction.factionId, factionId, StringComparison.OrdinalIgnoreCase);
}

bool TryGenerarEncuentroScoutForzado(BattleEncounterType tipo, EncounterZoneType zonaActual, out EncounterDefinition definition)
{
   definition = null;
   Predicate<EnemyFactionConfig> filtroScout = ObtenerFiltroFaccionScoutNodoActual();
   if (filtroScout == null)
   {
      return false;
   }

   if (TryGenerarEncuentro(tipo, zonaActual, filtroScout, out definition, 0, true))
   {
      encuentroZonaActual = zonaActual;
      return true;
   }

   if (TryGenerarEncuentro(tipo, EncounterZoneType.Generico, filtroScout, out definition, 0, true))
   {
      encuentroZonaActual = EncounterZoneType.Generico;
      return true;
   }

   return false;
}

bool TryGenerarEncuentroPresagioEnemigos(BattleEncounterType tipo, EncounterZoneType zonaActual, out EncounterDefinition definition)
{
   definition = null;
   CampaignManager campaignManager = CampaignManager.Instance;
   if (campaignManager == null || campaignManager.DebeUsarConfiguracionTutorial())
   {
      return false;
   }

   int presagioId = 0;
   if (campaignManager.TienePresagioActivo(PresagioCatalog.LeyDelMasFuerte))
   {
      presagioId = PresagioCatalog.LeyDelMasFuerte;
   }
   else if (campaignManager.TienePresagioActivo(PresagioCatalog.CorrompidosAlAcecho))
   {
      presagioId = PresagioCatalog.CorrompidosAlAcecho;
   }
   else if (campaignManager.TienePresagioActivo(PresagioCatalog.VenganadoresCazando))
   {
      presagioId = PresagioCatalog.VenganadoresCazando;
   }
   else if (campaignManager.TienePresagioActivo(PresagioCatalog.CentinelasLocales))
   {
      presagioId = PresagioCatalog.CentinelasLocales;
   }

   if (presagioId == 0)
   {
      return false;
   }

   bool garantizarPrimera = campaignManager.DebeGarantizarPrimeraBatallaPresagioEnemigos();
   if (presagioId == PresagioCatalog.CentinelasLocales && !garantizarPrimera)
   {
      return false;
   }

   if (!garantizarPrimera && UnityEngine.Random.Range(0f, 100f) >= 25f)
   {
      return false;
   }

   bool generado;
   if (presagioId == PresagioCatalog.CentinelasLocales)
   {
      generado = TryGenerarEncuentro(tipo, zonaActual, null, out definition, 0, true);
      if (generado)
      {
         encuentroZonaActual = zonaActual;
      }
   }
   else
   {
      string factionId = presagioId == PresagioCatalog.LeyDelMasFuerte
         ? "Bandidos"
         : presagioId == PresagioCatalog.CorrompidosAlAcecho
            ? "Corruptos"
            : VengadoresKadrynFactionId;
      Predicate<EnemyFactionConfig> filtro = faction =>
         faction != null
         && !string.IsNullOrWhiteSpace(faction.factionId)
         && string.Equals(faction.factionId, factionId, StringComparison.OrdinalIgnoreCase);

      generado = TryGenerarEncuentro(tipo, EncounterZoneType.Generico, filtro, out definition, 0, true);
      if (generado)
      {
         encuentroZonaActual = EncounterZoneType.Generico;
      }
   }

   if (generado)
   {
      Nodo nodoActual = campaignManager.scMapaManager != null ? campaignManager.scMapaManager.nodoActual : null;
      if (nodoActual != null && nodoActual.TieneFaccionScoutRevelada())
      {
         nodoActual.RegistrarFaccionScoutRevelada(
            definition.factionId,
            ObtenerNombreFaccionTraducido(definition.factionId, definition.factionName));
      }
   }

   return generado;
}

float AjustarChanceEncuentroPropioPresagioEnemigos(float chanceBase)
{
   CampaignManager campaignManager = CampaignManager.Instance;
   if (campaignManager == null
      || campaignManager.DebeUsarConfiguracionTutorial()
      || !campaignManager.TienePresagioActivo(PresagioCatalog.CentinelasLocales))
   {
      return chanceBase;
   }

   return Mathf.Clamp(chanceBase + 25f, 0f, 100f);
}

 bool EsNodoActualRitualKaleTav()
 {
    var manager = CampaignManager.Instance;
    if (manager == null || manager.scMapaManager == null || manager.scMapaManager.nodoActual == null)
    {
       return false;
    }

    return manager.scMapaManager.nodoActual.tipoNodo == 15 &&
           manager.scMapaManager.nodoActual.nodoRitual;
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

 bool EstaActivoDebugFaccionesCombate()
 {
    return debugRestringirEncuentrosProcedurales
       && debugEncounterFactionIds != null
       && debugEncounterFactionIds.Count > 0;
 }

 Predicate<EnemyFactionConfig> CombinarFiltroFacciones(Predicate<EnemyFactionConfig> factionFilter)
 {
    if (!EstaActivoDebugFaccionesCombate())
    {
       return factionFilter;
    }

    return faction =>
    {
       if (!EsFaccionDeLista(faction, debugEncounterFactionIds))
       {
          return false;
       }

       return factionFilter == null || factionFilter(faction);
    };
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

 int ObtenerEncuentroBandidoFallback()
 {
    int[] encountersBandidos = { 500, 501, 502, 503 };
    return encountersBandidos[UnityEngine.Random.Range(0, encountersBandidos.Length)];
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

 int ObtenerLimiteAliadosVisual()
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

    return limiteVisual;
 }

 void AsegurarReferenciasTextoSeleccionados()
 {
    if (txtSeleccionadosPersonajes == null && UIEmpezarBatalla != null)
    {
       txtSeleccionadosPersonajes = BuscarTextoSeleccionadosEnPanel(UIEmpezarBatalla);
    }

    if (txtSeleccionadosPersonajesCaravana == null && UIEmpezarBatallaACaravana != null)
    {
       txtSeleccionadosPersonajesCaravana = BuscarTextoSeleccionadosEnPanel(UIEmpezarBatallaACaravana);
    }
 }

 TextMeshProUGUI BuscarTextoSeleccionadosEnPanel(GameObject panel)
 {
    if (panel == null)
    {
       return null;
    }

    foreach (TextMeshProUGUI texto in panel.GetComponentsInChildren<TextMeshProUGUI>(true))
    {
       if (texto != null && texto.gameObject.name == "Cantidadtxt")
       {
          return texto;
       }
    }

    return null;
 }

 void ActualizarTextoSeleccionadosPersonajes(string texto)
 {
    AsegurarReferenciasTextoSeleccionados();

    if (txtSeleccionadosPersonajes != null)
    {
       txtSeleccionadosPersonajes.text = texto;
    }

    if (txtSeleccionadosPersonajesCaravana != null)
    {
       txtSeleccionadosPersonajesCaravana.text = texto;
    }
 }

 bool EstaSeleccionado(Personaje pers)
 {
    return pers != null
      && (scAdministradorEscenas.Personaje1 == pers
      || scAdministradorEscenas.Personaje2 == pers
      || scAdministradorEscenas.Personaje3 == pers
      || scAdministradorEscenas.Personaje4 == pers);
 }

 Personaje ObtenerPersonajeEnSlot(int indice)
 {
    switch (indice)
    {
       case 0: return scAdministradorEscenas.Personaje1;
       case 1: return scAdministradorEscenas.Personaje2;
       case 2: return scAdministradorEscenas.Personaje3;
       case 3: return scAdministradorEscenas.Personaje4;
       default: return null;
    }
 }

 void AsignarPersonajeEnSlot(int indice, Personaje pers)
 {
    switch (indice)
    {
       case 0:
          scAdministradorEscenas.Personaje1 = pers;
          break;
       case 1:
          scAdministradorEscenas.Personaje2 = pers;
          break;
       case 2:
          scAdministradorEscenas.Personaje3 = pers;
          break;
       case 3:
          scAdministradorEscenas.Personaje4 = pers;
          break;
    }
 }

 static int CompararPersonajesPorMenorHerida(Personaje a, Personaje b)
 {
    if (ReferenceEquals(a, b))
    {
       return 0;
    }

    if (a == null)
    {
       return 1;
    }

    if (b == null)
    {
       return -1;
    }

    bool aEsGuardia = EsGuardiaParaCaravana(a);
    bool bEsGuardia = EsGuardiaParaCaravana(b);
    int comparacion = bEsGuardia.CompareTo(aEsGuardia);
    if (comparacion != 0)
    {
       return comparacion;
    }

    float vidaMaximaA = Mathf.Max(1f, a.fVidaMaxima);
    float vidaMaximaB = Mathf.Max(1f, b.fVidaMaxima);
    float porcentajeVidaA = Mathf.Clamp01(a.fVidaActual / vidaMaximaA);
    float porcentajeVidaB = Mathf.Clamp01(b.fVidaActual / vidaMaximaB);

    comparacion = porcentajeVidaB.CompareTo(porcentajeVidaA);
    if (comparacion != 0)
    {
       return comparacion;
    }

    comparacion = b.fVidaActual.CompareTo(a.fVidaActual);
    if (comparacion != 0)
    {
       return comparacion;
    }

    comparacion = b.fVidaMaxima.CompareTo(a.fVidaMaxima);
    if (comparacion != 0)
    {
       return comparacion;
    }

    return string.Compare(a.sNombre, b.sNombre, StringComparison.Ordinal);
 }

 void PreseleccionarHeroesMenosHeridosParaBatallaConRefuerzos()
 {
    if (scAdministradorEscenas == null || CampaignManager.Instance == null || CampaignManager.Instance.scMenuPersonajes == null)
    {
       return;
    }

    scAdministradorEscenas.Personaje1 = null;
    scAdministradorEscenas.Personaje2 = null;
    scAdministradorEscenas.Personaje3 = null;
    scAdministradorEscenas.Personaje4 = null;

    List<Personaje> candidatos = new List<Personaje>();
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
       if (pers == null || pers.Camp_Muerto)
       {
          continue;
       }

       if (!EsGuardiaParaCaravana(pers) && pers.fVidaActual <= 1f)
       {
          continue;
       }

       candidatos.Add(pers);
    }

    candidatos.Sort(CompararPersonajesPorMenorHerida);

    int maxAliados = Mathf.Clamp(ObtenerMaxAliadosPermitidos(), 1, 4);
    for (int i = 0; i < maxAliados && i < candidatos.Count; i++)
    {
       AsignarPersonajeEnSlot(i, candidatos[i]);
    }
 }

 int ObtenerIndicePrimerSlotLibre(int maxAliados)
 {
    for (int i = 0; i < maxAliados; i++)
    {
       if (ObtenerPersonajeEnSlot(i) == null)
       {
          return i;
       }
    }

    return -1;
 }

 int ObtenerIndiceUltimoSlotOcupado(int maxAliados)
 {
    for (int i = maxAliados - 1; i >= 0; i--)
    {
       if (ObtenerPersonajeEnSlot(i) != null)
       {
          return i;
       }
    }

    return -1;
 }

 bool QuitarPersonajeSeleccionado(Personaje pers)
 {
    for (int i = 0; i < 4; i++)
    {
       if (ObtenerPersonajeEnSlot(i) == pers)
       {
          AsignarPersonajeEnSlot(i, null);
          return true;
       }
    }

    return false;
 }

 List<Personaje> ObtenerSeleccionadosEnOrden()
 {
    var seleccionados = new List<Personaje>(4);
    AgregarSiNoEsta(seleccionados, scAdministradorEscenas.Personaje1);
    AgregarSiNoEsta(seleccionados, scAdministradorEscenas.Personaje2);
    AgregarSiNoEsta(seleccionados, scAdministradorEscenas.Personaje3);
    AgregarSiNoEsta(seleccionados, scAdministradorEscenas.Personaje4);
    return seleccionados;
 }

 void LimpiarContenedor(GameObject contenedor)
 {
    if (contenedor == null)
    {
       return;
    }

    foreach (Transform child in contenedor.transform)
    {
       Destroy(child.gameObject);
    }
 }

 void ActualizarVisibilidadListasBatalla()
 {
    bool mostrarListaSecundaria = UITerminarBatalla == null || !UITerminarBatalla.activeInHierarchy;
    if (contenedorUIPersonajesFuera != null)
    {
       contenedorUIPersonajesFuera.SetActive(mostrarListaSecundaria);
    }

    RefrescarBotonesPersonaje(contenedorUIPersonajes);
    RefrescarBotonesPersonaje(contenedorUIPersonajesFuera);
 }

 void RefrescarBotonesPersonaje(GameObject contenedor)
 {
    if (contenedor == null)
    {
       return;
    }

    foreach (Transform child in contenedor.transform)
    {
       btnPersonaje btn = child.GetComponent<btnPersonaje>();
       if (btn != null)
       {
          btn.RepresentarTodo();
       }
    }
 }

 RefuerzoAliadoCaravanaOrdenItem CrearRefuerzoCaravanaDesdePersonaje(Personaje pers)
 {
    if (pers == null)
    {
       return null;
    }

    if (CampaignManager.Instance != null)
    {
       CampaignManager.Instance.SincronizarAparienciaVisualPersonaje(pers);
    }

    return new RefuerzoAliadoCaravanaOrdenItem
    {
       id = "personaje:" + pers.GetInstanceID(),
       tipo = TipoRefuerzoAliadoCaravana.Personaje,
       personaje = pers,
       nombreVisible = pers.sNombre,
       retrato = pers.spRetrato
    };
 }

 RefuerzoAliadoCaravanaOrdenItem CrearRefuerzoCaravanaDesdePrefab(GameObject prefabRefuerzo, int indice)
 {
    if (prefabRefuerzo == null)
    {
       return null;
    }

    Unidad unidad = prefabRefuerzo.GetComponent<Unidad>();
    string nombreVisible = unidad != null && !string.IsNullOrWhiteSpace(unidad.uNombre)
      ? unidad.uNombre
      : prefabRefuerzo.name;
    Sprite retrato = unidad != null ? unidad.uRetrato : null;

    return new RefuerzoAliadoCaravanaOrdenItem
    {
       id = "unidad:" + indice + ":" + prefabRefuerzo.name,
       tipo = TipoRefuerzoAliadoCaravana.UnidadPrefab,
       prefabRefuerzo = prefabRefuerzo,
       nombreVisible = nombreVisible,
       retrato = retrato
    };
 }

 List<RefuerzoAliadoCaravanaOrdenItem> ObtenerCandidatosRefuerzosCaravana()
 {
    var candidatos = new List<RefuerzoAliadoCaravanaOrdenItem>();
    if (CampaignManager.Instance == null || CampaignManager.Instance.scMenuPersonajes == null)
    {
       return candidatos;
    }

    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
       if (pers == null || pers.Camp_Muerto || pers.fVidaActual <= 1 || EstaSeleccionado(pers))
       {
          continue;
       }

       RefuerzoAliadoCaravanaOrdenItem refuerzoPersonaje = CrearRefuerzoCaravanaDesdePersonaje(pers);
       if (refuerzoPersonaje != null)
       {
          candidatos.Add(refuerzoPersonaje);
       }
    }

    if (scAdministradorEscenas == null || scAdministradorEscenas.ContenedorPrefabsBatalla == null)
    {
       return candidatos;
    }

    int cantidadMilicianos = (int)CampaignManager.Instance.GetMiliciasActual() / 10;
    if (cantidadMilicianos <= 0)
    {
       return candidatos;
    }

    bool tieneDesertores = CampaignManager.Instance.scMenuSequito != null
      && CampaignManager.Instance.scMenuSequito.TieneSequito(6);
    ContenedorPrefabs prefabs = scAdministradorEscenas.ContenedorPrefabsBatalla;

    for (int i = 0; i < cantidadMilicianos; i++)
    {
       GameObject prefabRefuerzo = tieneDesertores
         ? (i % 2 == 0 ? prefabs.Desertor2 : prefabs.Desertor1)
         : (i % 2 == 0 ? prefabs.Miliciano2 : prefabs.Miliciano1);

       RefuerzoAliadoCaravanaOrdenItem refuerzoMilicia = CrearRefuerzoCaravanaDesdePrefab(prefabRefuerzo, i);
       if (refuerzoMilicia != null)
       {
          candidatos.Add(refuerzoMilicia);
       }
    }

    return candidatos;
 }

 void SincronizarOrdenRefuerzosCaravana()
 {
    List<RefuerzoAliadoCaravanaOrdenItem> candidatos = ObtenerCandidatosRefuerzosCaravana();
    Dictionary<string, RefuerzoAliadoCaravanaOrdenItem> candidatosPorId = new Dictionary<string, RefuerzoAliadoCaravanaOrdenItem>();
    foreach (RefuerzoAliadoCaravanaOrdenItem candidato in candidatos)
    {
       if (candidato == null || string.IsNullOrWhiteSpace(candidato.id) || candidatosPorId.ContainsKey(candidato.id))
       {
          continue;
       }

       candidatosPorId.Add(candidato.id, candidato);
    }

    ordenRefuerzosCaravana.RemoveAll(refuerzo =>
      refuerzo == null
      || string.IsNullOrWhiteSpace(refuerzo.id)
      || !candidatosPorId.ContainsKey(refuerzo.id));

    for (int i = 0; i < ordenRefuerzosCaravana.Count; i++)
    {
       RefuerzoAliadoCaravanaOrdenItem refuerzo = ordenRefuerzosCaravana[i];
       ordenRefuerzosCaravana[i] = candidatosPorId[refuerzo.id];
    }

    foreach (RefuerzoAliadoCaravanaOrdenItem candidato in candidatos)
    {
       bool yaExiste = false;
       foreach (RefuerzoAliadoCaravanaOrdenItem existente in ordenRefuerzosCaravana)
       {
          if (existente != null && existente.id == candidato.id)
          {
             yaExiste = true;
             break;
          }
       }

       if (!yaExiste)
       {
          ordenRefuerzosCaravana.Add(candidato);
       }
    }
 }

 void ReiniciarOrdenRefuerzosCaravana()
 {
    ordenRefuerzosCaravana.Clear();
    SincronizarOrdenRefuerzosCaravana();
 }

 bool DebeUsarListaRefuerzosAliados()
 {
    return esBatallaFinal
      || (UIEmpezarBatallaACaravana != null && UIEmpezarBatallaACaravana.activeInHierarchy);
 }

 public bool PuedeReordenarRefuerzoCaravana(btnPersonaje btn)
 {
    return btn != null
      && DebeUsarListaRefuerzosAliados()
      && contenedorUIPersonajesFuera != null
      && btn.transform.parent == contenedorUIPersonajesFuera.transform;
 }

 public void ReordenarRefuerzoCaravana(btnPersonaje btn, Vector2 posicionPantalla, Camera uiCamera)
 {
    if (!PuedeReordenarRefuerzoCaravana(btn))
    {
       return;
    }

    RectTransform padre = contenedorUIPersonajesFuera.transform as RectTransform;
    if (padre == null)
    {
       return;
    }

    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(padre, posicionPantalla, uiCamera, out Vector2 puntoLocal))
    {
       return;
    }

    LayoutRebuilder.ForceRebuildLayoutImmediate(padre);
    bool usarEjeVertical = DebeReordenarRefuerzoCaravanaEnVertical(padre);
    float posicionComparacion = usarEjeVertical ? -puntoLocal.y : puntoLocal.x;
    int nuevoIndice = 0;
    for (int i = 0; i < padre.childCount; i++)
    {
       Transform child = padre.GetChild(i);
       if (child == btn.transform)
       {
          continue;
       }

       RectTransform childRect = child as RectTransform;
       if (childRect == null)
       {
          continue;
       }

       Vector3 centroMundo = childRect.TransformPoint(childRect.rect.center);
       Vector3 centroLocal = padre.InverseTransformPoint(centroMundo);
       float centroComparacion = usarEjeVertical ? -centroLocal.y : centroLocal.x;
       if (posicionComparacion > centroComparacion)
       {
          nuevoIndice++;
       }
    }

    int indiceClampeado = Mathf.Clamp(nuevoIndice, 0, padre.childCount - 1);
    if (btn.transform.GetSiblingIndex() != indiceClampeado)
    {
       btn.transform.SetSiblingIndex(indiceClampeado);
    }
 }

 bool DebeReordenarRefuerzoCaravanaEnVertical(RectTransform contenedor)
 {
    if (contenedor == null)
    {
       return false;
    }

    VerticalLayoutGroup verticalLayout = contenedor.GetComponent<VerticalLayoutGroup>();
    if (verticalLayout != null)
    {
       return true;
    }

    GridLayoutGroup gridLayout = contenedor.GetComponent<GridLayoutGroup>();
    return gridLayout != null && gridLayout.startAxis == GridLayoutGroup.Axis.Vertical;
 }

 public void ConfirmarOrdenRefuerzosCaravanaDesdeUI()
 {
    if (contenedorUIPersonajesFuera == null)
    {
       return;
    }

    ordenRefuerzosCaravana.Clear();
    foreach (Transform child in contenedorUIPersonajesFuera.transform)
    {
       btnPersonaje btn = child.GetComponent<btnPersonaje>();
       RefuerzoAliadoCaravanaOrdenItem refuerzo = btn != null ? btn.ObtenerRefuerzoCaravanaRepresentado() : null;
       if (refuerzo != null)
       {
          ordenRefuerzosCaravana.Add(refuerzo);
       }
    }

    SincronizarOrdenRefuerzosCaravana();
 }

 public List<RefuerzoAliadoCaravanaOrdenItem> ObtenerOrdenRefuerzosAliadosCaravana()
 {
    SincronizarOrdenRefuerzosCaravana();
    return new List<RefuerzoAliadoCaravanaOrdenItem>(ordenRefuerzosCaravana);
 }

 void CrearBotonPersonajeEnContenedor(GameObject contenedor, Personaje pers, bool seleccionado, float escala)
 {
    if (contenedor == null || prefabBtnPersonaje == null || pers == null)
    {
       return;
    }

    GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedor.transform);
    btnPersonaje btn = btnPers.GetComponent<btnPersonaje>();
    if (btn == null)
    {
       return;
    }

    btn.personajeRepresentado = pers;
    btn.RepresentarTodo();
    btn.SetSeleccionado(seleccionado);
    btnPers.transform.localScale = Vector3.one * escala;
 }

 void CrearBotonRefuerzoCaravanaEnContenedor(GameObject contenedor, RefuerzoAliadoCaravanaOrdenItem refuerzo, float escala)
 {
    if (contenedor == null || prefabBtnPersonaje == null || refuerzo == null)
    {
       return;
    }

    GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedor.transform);
    btnPersonaje btn = btnPers.GetComponent<btnPersonaje>();
    if (btn == null)
    {
       return;
    }

    btn.ConfigurarRefuerzoCaravana(refuerzo);
    btn.SetSeleccionado(false);
    btnPers.transform.localScale = Vector3.one * escala;
 }

 void PoblarListaBatallaNormal()
 {
    List<Personaje> seleccionados = ObtenerSeleccionadosEnOrden();
    HashSet<Personaje> seleccionadosSet = new HashSet<Personaje>(seleccionados);

    foreach (Personaje pers in seleccionados)
    {
       CrearBotonPersonajeEnContenedor(contenedorUIPersonajes, pers, true, 1.2f);
    }

    if (UITerminarBatalla != null && UITerminarBatalla.activeInHierarchy)
    {
       return;
    }

    if (CampaignManager.Instance == null
      || CampaignManager.Instance.scMenuPersonajes == null
      || contenedorUIPersonajesFuera == null)
    {
       return;
    }

    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
       if (pers == null || pers.Camp_Muerto || seleccionadosSet.Contains(pers))
       {
          continue;
       }

       CrearBotonPersonajeEnContenedor(contenedorUIPersonajesFuera, pers, false, 1f);
    }
 }

 void PoblarListaAtaqueCaravana()
 {
    List<Personaje> seleccionados = ObtenerSeleccionadosEnOrden();
    foreach (Personaje pers in seleccionados)
    {
       CrearBotonPersonajeEnContenedor(contenedorUIPersonajes, pers, true, 1f);
    }

    if (UITerminarBatalla != null && UITerminarBatalla.activeInHierarchy)
    {
      return;
    }

    if (contenedorUIPersonajesFuera == null)
    {
       return;
    }

    SincronizarOrdenRefuerzosCaravana();
    foreach (RefuerzoAliadoCaravanaOrdenItem refuerzo in ordenRefuerzosCaravana)
    {
       CrearBotonRefuerzoCaravanaEnContenedor(contenedorUIPersonajesFuera, refuerzo, 1f);
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
   int limiteVisual = ObtenerLimiteAliadosVisual();

   AjustarSeleccionAlMaximo(limiteVisual);

   int seleccionados = ContarAliadosSeleccionados();

        string textoSeleccionados;
        if (tutorial != null && tutorial.tutorialActivo && tutorial.pasoActual < 4)
        {
            textoSeleccionados = $"{seleccionados}/1"; // Forzar a 1 durante el tutorial
        }
        else if (tutorial != null && tutorial.tutorialActivo && tutorial.pasoActual < 40)
        {
            textoSeleccionados = $"{seleccionados}/3";
        }
        else
        {
            textoSeleccionados = $"{seleccionados}/" + limiteVisual;
        }

        ActualizarTextoSeleccionadosPersonajes(textoSeleccionados);
    // Verifica si todos los personajes son null
    bool todosVacios = scAdministradorEscenas.Personaje1 == null &&
                       scAdministradorEscenas.Personaje2 == null &&
                       scAdministradorEscenas.Personaje3 == null &&
                       scAdministradorEscenas.Personaje4 == null;

        if (DebeExigirTresParticipantesBatallaFinalTutorial()
            || (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 50))
        {
            btnComenzar.SetActive(seleccionados > 2);

        }
        else
        {
            // Configura el estado del botón basíndose en si hay personajes seleccionados o no
            btnComenzar.SetActive(!todosVacios);
        }

    LimpiarContenedor(contenedorUIPersonajes);
    LimpiarContenedor(contenedorUIPersonajesFuera);

        if (DebeUsarListaRefuerzosAliados())
        {
            PoblarListaAtaqueCaravana();
        }
        else
        {
            PoblarListaBatallaNormal();
        }

    ActualizarTextoFaccionBatalla();
    ActualizarTextoPoderesBatalla();
    ActualizarVisibilidadListasBatalla();
    return;
   

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
                    btnPersonaje btn = btnPers.GetComponent<btnPersonaje>();
                    if (btn != null)
                    {
                        btn.personajeRepresentado = pers;
                    }
                }

            }
        }
        else //Si es ataque a la caravana, solo muestra los personajes haciendo guardia
        {
            foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
            {
                if (!pers.Camp_Muerto && pers.fVidaActual > 1) //Si no está muerto y tiene vida
                {
                    if (EsGuardiaParaCaravana(pers))
                    {
                        GameObject btnPers = Instantiate(prefabBtnPersonaje, contenedorUIPersonajes.transform);
                        btnPersonaje btn = btnPers.GetComponent<btnPersonaje>();
                        if (btn != null)
                        {
                            btn.personajeRepresentado = pers;
                        }
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
                btn.RepresentarTodo();

                bool estaSeleccionado =
                    scAdministradorEscenas.Personaje1 == btn.personajeRepresentado ||
                    scAdministradorEscenas.Personaje2 == btn.personajeRepresentado ||
                    scAdministradorEscenas.Personaje3 == btn.personajeRepresentado ||
                    scAdministradorEscenas.Personaje4 == btn.personajeRepresentado;

                btn.SetSeleccionado(estaSeleccionado);

            }
        }
   

    ActualizarTextoFaccionBatalla();

 }

 void ActualizarTextoPoderesBatalla()
 {
    ActualizarTextoPoderAliado();
    ActualizarTextoPoderEnemigo();
 }

 void RefrescarTextosUIBatallaSegunIdioma()
 {
    int idiomaActual = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    if (idiomaUIBatallaCache == idiomaActual)
    {
       return;
    }

    idiomaUIBatallaCache = idiomaActual;
    ActualizarTextoFaccionBatalla();
    ActualizarTextoPoderesBatalla();
 }

 void ActualizarTextoPoderAliado()
 {
    if (txtPoderaliado == null)
    {
       return;
    }

    int poderAliado = ObtenerPoderAliadoPreview();
    txtPoderaliado.text = FormatearTextoPoderAliado(poderAliado);
    txtPoderaliado.gameObject.SetActive(true);
 }

 void ActualizarTextoPoderEnemigo()
 {
    if (txtPoderenemigo == null)
    {
       return;
    }

    if (esEmboscadaEnemiga == 1)
    {
       txtPoderenemigo.text = FormatearTextoPoderEnemigoOculto();
       txtPoderenemigo.gameObject.SetActive(true);
       return;
    }

    int poderEnemigo = ObtenerPoderEnemigoPreview();
    txtPoderenemigo.text = FormatearTextoPoderEnemigo(poderEnemigo);
    txtPoderenemigo.gameObject.SetActive(true);
 }

 int ObtenerPoderAliadoPreview()
 {
    int poderTotal = 0;
    List<Personaje> seleccionados = ObtenerSeleccionadosEnOrden();
    foreach (Personaje personaje in seleccionados)
    {
       if (personaje == null)
       {
          continue;
       }

       int poderPersonaje = 2 + Mathf.RoundToInt(personaje.fNivelActual);
       float vidaMaxima = Mathf.Max(1f, personaje.fVidaMaxima);
       float porcentajeVida = personaje.fVidaActual / vidaMaxima;

       if (porcentajeVida < 0.8f)
       {
          poderPersonaje -= 1;
       }
       if (porcentajeVida < 0.3f)
       {
          poderPersonaje -= 1;
       }

       poderTotal += Mathf.Max(0, poderPersonaje);
    }

    return poderTotal;
 }

 int ObtenerBuffPoderEquipo(Personaje personaje)
 {
    if (personaje == null)
    {
       return 0;
    }

    int buffPoder = 0;
    if (personaje.itemArma != null)
    {
       buffPoder += personaje.itemArma.buffPoder;
    }
    if (personaje.itemArmadura != null)
    {
       buffPoder += personaje.itemArmadura.buffPoder;
    }
    if (personaje.Accesorio1 != null)
    {
       buffPoder += personaje.Accesorio1.buffPoder;
    }
    if (personaje.Accesorio2 != null)
    {
       buffPoder += personaje.Accesorio2.buffPoder;
    }

    return buffPoder;
 }

 int ObtenerPoderEnemigoPreview()
 {
    if (encuentroGeneradoActual == null || encuentroGeneradoActual.units == null)
    {
       return 0;
    }

    int poderTotal = 0;
    foreach (EncounterUnitSlot slot in encuentroGeneradoActual.units)
    {
       if (slot == null)
       {
          continue;
       }

       Unidad unidad = slot.prefab != null ? slot.prefab.GetComponent<Unidad>() : null;
       if (unidad != null)
       {
          int poderUnidad = Mathf.Max(0, Mathf.RoundToInt(unidad.mod_CarPoder));
          poderTotal += Mathf.Max(poderUnidad, Mathf.Max(0, slot.tierCost));
       }
       else
       {
          poderTotal += Mathf.Max(0, slot.tierCost);
       }
    }

    return poderTotal;
 }

 string FormatearTextoPoderAliado(int poder)
 {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
       case TRADU.IdiomaIngles:
          return "Ally Power: " + poder;
       case TRADU.IdiomaPortugues:
          return "Poder aliado: " + poder;
       default:
          return "Poder aliado: " + poder;
    }
 }

 string FormatearTextoPoderEnemigo(int poder)
 {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
       case TRADU.IdiomaIngles:
          return "Enemy Power: " + poder;
       case TRADU.IdiomaPortugues:
          return "Poder enemigo: " + poder;
       default:
          return "Poder enemigo: " + poder;
    }
 }

 string FormatearTextoPoderEnemigoOculto()
 {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    switch (idioma)
    {
       case TRADU.IdiomaIngles:
          return "Enemy Power: ??";
       case TRADU.IdiomaPortugues:
          return "Poder enemigo: ??";
       default:
          return "Poder enemigo: ??";
    }
 }

 void ActualizarTextoFaccionBatalla()
 {
    if (faccionBatalla == null)
    {
       return;
    }

    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;

    if (esEmboscadaEnemiga == 1)
    {
       faccionBatalla.text = ObtenerTextoEmboscadaEnemigaPorIdioma(idioma);
       faccionBatalla.gameObject.SetActive(true);
       return;
    }

    if (encuentroGeneradoActual == null)
    {
       faccionBatalla.text = string.Empty;
       faccionBatalla.gameObject.SetActive(false);
       return;
    }

    string nombreFaccion = ObtenerNombreFaccionTraducido(encuentroGeneradoActual.factionId, encuentroGeneradoActual.factionName);
    if (string.IsNullOrWhiteSpace(nombreFaccion))
    {
       faccionBatalla.text = string.Empty;
       faccionBatalla.gameObject.SetActive(false);
       return;
    }

    if (esEmboscadaEnemiga == 2)
    {
       faccionBatalla.text = ObtenerPrefijoEmboscadaAliadaPorIdioma(idioma) + nombreFaccion;
       faccionBatalla.gameObject.SetActive(true);
       return;
    }

    faccionBatalla.text = ObtenerPrefijoFaccionPorIdioma(idioma) + nombreFaccion;
    faccionBatalla.gameObject.SetActive(true);
 }

 string ObtenerNombreFaccionTraducido(string factionId, string factionName)
 {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    string nombreBase = !string.IsNullOrWhiteSpace(factionName) ? factionName : factionId;
    if (string.IsNullOrWhiteSpace(nombreBase))
    {
       return string.Empty;
    }

    string traducido = TraducirFaccionPorIdioma(nombreBase, idioma);
    if (!string.Equals(traducido, nombreBase, StringComparison.OrdinalIgnoreCase))
    {
       return traducido;
    }

    if (!string.IsNullOrWhiteSpace(factionId) && !string.Equals(factionId, nombreBase, StringComparison.OrdinalIgnoreCase))
    {
       string traducidoPorId = TraducirFaccionPorIdioma(factionId, idioma);
       if (!string.Equals(traducidoPorId, factionId, StringComparison.OrdinalIgnoreCase))
       {
          return traducidoPorId;
       }
    }

    return nombreBase;
 }

 string TraducirFaccionPorIdioma(string nombreBase, int idioma)
 {
    if (string.IsNullOrWhiteSpace(nombreBase))
    {
       return nombreBase;
    }

    if (idioma == TRADU.IdiomaIngles && TraduccionesFaccionEn.TryGetValue(nombreBase, out string traduccionEn))
    {
       return traduccionEn;
    }

    if (idioma == TRADU.IdiomaPortugues && TraduccionesFaccionPtBr.TryGetValue(nombreBase, out string traduccionPt))
    {
       return traduccionPt;
    }

    return nombreBase;
 }

 string ObtenerPrefijoFaccionPorIdioma(int idioma)
 {
    if (idioma == TRADU.IdiomaIngles)
   {
       return "Enemy faction: ";
    }

    if (idioma == TRADU.IdiomaPortugues)
    {
       return "Facção inimiga: ";
    }

    return "Facción enemiga: ";
 }

 string ObtenerTextoEmboscadaEnemigaPorIdioma(int idioma)
 {
    if (idioma == TRADU.IdiomaIngles)
    {
       return "The caravan has been ambushed!";
    }

    if (idioma == TRADU.IdiomaPortugues)
    {
       return "A caravana foi emboscada!";
    }

    return "La caravana ha sido emboscada!";
 }

 string ObtenerPrefijoEmboscadaAliadaPorIdioma(int idioma)
 {
    if (idioma == TRADU.IdiomaIngles)
    {
       return "You ambushed the enemies: ";
    }

    if (idioma == TRADU.IdiomaPortugues)
    {
       return "Você emboscou os inimigos: ";
    }

    return "Has emboscado a los enemigos: ";
 }
 
public void DejanEnListaParticipantesSolo()
 {
  LimpiarContenedor(contenedorUIPersonajesFuera);

  if (UITerminarBatalla != null && UITerminarBatalla.activeInHierarchy)
  {
    ActualizarVisibilidadListasBatalla();
  }

  
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
    if (pers == null || scAdministradorEscenas == null)
    {
       return;
    }

    if (UIEmpezarBatallaACaravana == null || !UIEmpezarBatallaACaravana.activeInHierarchy)
    {
       if (QuitarPersonajeSeleccionado(pers))
       {
          RuntimeAnalytics.TrackDesign("battle", "party_deselect", RuntimeAnalytics.ClassToken(pers));
          ActualizarLista();
          return;
       }

       int maxAliadosNormales = ObtenerLimiteAliadosVisual();
       int indiceLibre = ObtenerIndicePrimerSlotLibre(maxAliadosNormales);
       if (indiceLibre >= 0)
       {
          AsignarPersonajeEnSlot(indiceLibre, pers);
       }
       else
       {
          int indiceReemplazo = ObtenerIndiceUltimoSlotOcupado(maxAliadosNormales);
          if (indiceReemplazo >= 0)
          {
             AsignarPersonajeEnSlot(indiceReemplazo, pers);
          }
       }

       ActualizarLista();
       RuntimeAnalytics.TrackDesign("battle", "party_select", RuntimeAnalytics.ClassToken(pers));
       return;
    }

    if (scAdministradorEscenas.Personaje1 == pers)
    {
        scAdministradorEscenas.Personaje1 = null;
        RuntimeAnalytics.TrackDesign("battle", "party_deselect", RuntimeAnalytics.ClassToken(pers));
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje2 == pers)
    {
        scAdministradorEscenas.Personaje2 = null;
        RuntimeAnalytics.TrackDesign("battle", "party_deselect", RuntimeAnalytics.ClassToken(pers));
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje3 == pers)
    {
        scAdministradorEscenas.Personaje3 = null;
        RuntimeAnalytics.TrackDesign("battle", "party_deselect", RuntimeAnalytics.ClassToken(pers));
        ActualizarLista();
        return;
    }
    if (scAdministradorEscenas.Personaje4 == pers)
    {
        scAdministradorEscenas.Personaje4 = null;
        RuntimeAnalytics.TrackDesign("battle", "party_deselect", RuntimeAnalytics.ClassToken(pers));
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
    RuntimeAnalytics.TrackDesign("battle", "party_select", RuntimeAnalytics.ClassToken(pers));

   

    
 }
 [SerializeField] TextMeshProUGUI txtSeleccionadosPersonajes;
 [SerializeField] TextMeshProUGUI txtSeleccionadosPersonajesCaravana;
 public int esEmboscadaEnemiga;
  public int EventoBatallaID = 0;
    public int cantidadAliadosComienzo;
  private bool EsNodoActualBatallaFinalTutorial()
  {
      CampaignManager campaignManager = CampaignManager.Instance;
      var tutorialManager = campaignManager != null ? campaignManager.scTutorialManager : null;
      return tutorialManager != null
          && campaignManager.DebeUsarConfiguracionTutorial()
          && campaignManager.scMapaManager != null
          && tutorialManager.EsBatallaFinalTutorial(campaignManager.scMapaManager.nodoActual);
  }

  private bool DebeExigirTresParticipantesBatallaFinalTutorial()
  {
      CampaignManager campaignManager = CampaignManager.Instance;
      return EventoBatallaID == 701
          && campaignManager != null
          && campaignManager.DebeUsarConfiguracionTutorial();
  }

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

        if (EsNodoActualBatallaFinalTutorial())
        {
            EventoBatallaID = 701;
            encuentroGeneradoActual = null;
            ActualizarLista();
            return;
        }

        bool forzarBandidos = n == EventoEspecialDefenderCivilesBandidos;
        if (n == 0 || forzarBandidos)
        {
            bool generado = false;
            float chanceEncuentroPropio = atributosZona != null ? atributosZona.GetChanceEncuentroPropio(zonaActual) : 70f;
            chanceEncuentroPropio = AjustarChanceEncuentroPropioPresagioEnemigos(chanceEncuentroPropio);
            Predicate<EnemyFactionConfig> filtroBandidos = forzarBandidos ? (f => EsFaccionDeLista(f, banditFactionIds)) : null;

            if (forzarBandidos)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.Normal, zonaActual, filtroBandidos, out encuentroGeneradoActual, 0, true);
                if (generado)
                {
                    encuentroZonaActual = zonaActual;
                }
                else if (TryGenerarEncuentro(BattleEncounterType.Normal, EncounterZoneType.Generico, filtroBandidos, out encuentroGeneradoActual, 0, true))
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                    generado = true;
                }

                if (!generado)
                {
                    EventoBatallaID = ObtenerEncuentroBandidoFallback();
                    encuentroGeneradoActual = null;
                }
            }
            else if (TryGenerarEncuentroPresagioEnemigos(BattleEncounterType.Normal, zonaActual, out encuentroGeneradoActual))
            {
                generado = true;
            }
            else if (TryGenerarEncuentroScoutForzado(BattleEncounterType.Normal, zonaActual, out encuentroGeneradoActual))
            {
                generado = true;
            }
            else if (!EstaActivoDebugFaccionesCombate() && DeberiaGenerarBatallaCorrupta())
            {
                generado = TryGenerarEncuentro(BattleEncounterType.Normal, EncounterZoneType.Generico, f => EsFaccionDeLista(f, corruptFactionIds), out encuentroGeneradoActual, 0, true);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!forzarBandidos && !generado && UnityEngine.Random.Range(0f, 100f) < chanceEncuentroPropio)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.Normal, zonaActual, null, out encuentroGeneradoActual, 0, true);
                if (generado)
                {
                    encuentroZonaActual = zonaActual;
                }
            }

            if (!forzarBandidos && !generado)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.Normal, EncounterZoneType.Generico, null, out encuentroGeneradoActual, 0, true);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!forzarBandidos && !generado)
            {
                TryGenerarEncuentro(BattleEncounterType.Normal, zonaActual, null, out encuentroGeneradoActual, 0, true);
                encuentroZonaActual = zonaActual;
            }

            EventoBatallaID = encuentroGeneradoActual != null ? 0 : EventoBatallaID;

            CampaignManager campaignManager = CampaignManager.Instance;
            var tutorialManager = campaignManager != null ? campaignManager.scTutorialManager : null;
            bool forzarPrimerCombateTutorialNuevo = campaignManager != null && campaignManager.DebeForzarPrimerCombateTutorial();
            if ((tutorialManager != null && tutorialManager.tutorialActivo && tutorialManager.pasoActual == 3)
                || forzarPrimerCombateTutorialNuevo)
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

    if (EsNodoActualBatallaFinalTutorial())
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
        chanceEncuentroPropio = AjustarChanceEncuentroPropioPresagioEnemigos(chanceEncuentroPropio);
        bool esRitualKaleTav = forzarRitualKaleTav || EsNodoActualRitualKaleTav();

            if (esRitualKaleTav)
            {
                Predicate<EnemyFactionConfig> kaleFilter = faction =>
                    faction != null &&
                    !string.IsNullOrWhiteSpace(faction.factionId) &&
                    string.Equals(faction.factionId, KaleTavFactionId, StringComparison.OrdinalIgnoreCase);

                generado = TryGenerarEncuentro(BattleEncounterType.Elite, zonaActual, kaleFilter, out encuentroGeneradoActual, 0, true);
                if (!generado && TryGenerarEncuentro(BattleEncounterType.Elite, EncounterZoneType.Generico, kaleFilter, out encuentroGeneradoActual, 0, true))
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                    generado = true;
                }
            }

            if (!generado && !esRitualKaleTav
                && TryGenerarEncuentroPresagioEnemigos(BattleEncounterType.Elite, zonaActual, out encuentroGeneradoActual))
            {
                generado = true;
            }

            if (!generado && TryGenerarEncuentroScoutForzado(BattleEncounterType.Elite, zonaActual, out encuentroGeneradoActual))
            {
                generado = true;
            }

            if (!generado)
            {
                bool intentarZonaPrimero = UnityEngine.Random.Range(0f, 100f) < chanceEncuentroPropio;
                if (intentarZonaPrimero)
                {
                    generado = TryGenerarEncuentro(BattleEncounterType.Elite, zonaActual, null, out encuentroGeneradoActual, 0, true);
                    if (generado)
                    {
                        encuentroZonaActual = zonaActual;
                    }
                    else if (TryGenerarEncuentro(BattleEncounterType.Elite, EncounterZoneType.Generico, null, out encuentroGeneradoActual, 0, true))
                    {
                        encuentroZonaActual = EncounterZoneType.Generico;
                        generado = true;
                    }
                }
                else
                {
                    generado = TryGenerarEncuentro(BattleEncounterType.Elite, EncounterZoneType.Generico, null, out encuentroGeneradoActual, 0, true);
                    if (generado)
                    {
                        encuentroZonaActual = EncounterZoneType.Generico;
                    }
                    else if (TryGenerarEncuentro(BattleEncounterType.Elite, zonaActual, null, out encuentroGeneradoActual, 0, true))
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

    public void EventoBatallaEliteCorruptos(int esEmboscada = 0)
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
        EventoBatallaID = 0;

        var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
        EncounterZoneType zonaActual = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
        encuentroZonaActual = zonaActual;

        Predicate<EnemyFactionConfig> corruptosFilter = faction => EsFaccionDeLista(faction, corruptFactionIds);
        bool generado = TryGenerarEncuentro(BattleEncounterType.Elite, zonaActual, corruptosFilter, out encuentroGeneradoActual, 0, true);
        if (!generado && TryGenerarEncuentro(BattleEncounterType.Elite, EncounterZoneType.Generico, corruptosFilter, out encuentroGeneradoActual, 0, true))
        {
            encuentroZonaActual = EncounterZoneType.Generico;
            generado = true;
        }

        if (!generado)
        {
            Debug.LogWarning("[MenuBatallas] No se pudo generar una batalla elite de Corruptos.");
        }

        ActualizarLista();
    }

    public void EventoBatallaFinal(int n, int esEmboscada = 0)
    {
        if (EsNodoActualBatallaFinalTutorial())
        {
            EventoBatallaNormal(701, esEmboscada);
            return;
        }

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

        PreseleccionarHeroesMenosHeridosParaBatallaConRefuerzos();
        ReiniciarOrdenRefuerzosCaravana();
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

        PreseleccionarHeroesMenosHeridosParaBatallaConRefuerzos();

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
            chanceEncuentroPropio = AjustarChanceEncuentroPropioPresagioEnemigos(chanceEncuentroPropio);

            if (TryGenerarEncuentroPresagioEnemigos(BattleEncounterType.AtaqueCaravana, zonaActual, out encuentroGeneradoActual))
            {
                generado = true;
            }
            else if (TryGenerarEncuentroScoutForzado(BattleEncounterType.AtaqueCaravana, zonaActual, out encuentroGeneradoActual))
            {
                generado = true;
            }

            if (!generado && !EstaActivoDebugFaccionesCombate() && DeberiaGenerarBatallaCorrupta())
            {
                generado = TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, EncounterZoneType.Generico, f => EsFaccionDeLista(f, corruptFactionIds), out encuentroGeneradoActual, 0, true);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!generado && UnityEngine.Random.Range(0f, 100f) < chanceEncuentroPropio)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, zonaActual, null, out encuentroGeneradoActual, 0, true);
                if (generado)
                {
                    encuentroZonaActual = zonaActual;
                }
            }

            if (!generado)
            {
                generado = TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, EncounterZoneType.Generico, null, out encuentroGeneradoActual, 0, true);
                if (generado)
                {
                    encuentroZonaActual = EncounterZoneType.Generico;
                }
            }

            if (!generado)
            {
                TryGenerarEncuentro(BattleEncounterType.AtaqueCaravana, zonaActual, null, out encuentroGeneradoActual, 0, true);
                encuentroZonaActual = zonaActual;
            }

            EventoBatallaID = encuentroGeneradoActual != null ? 0 : EventoBatallaID;
        }
        else
        {
            encuentroGeneradoActual = null;
        }

        ReiniciarOrdenRefuerzosCaravana();
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
    if (!TryGenerarEncuentro(BattleEncounterType.Subterraneo, EncounterZoneType.Subterraneo, null, out encuentroGeneradoActual, faseClamped, true))
    {
        var atributosZona = CampaignManager.Instance != null ? CampaignManager.Instance.scAtributosZona : null;
        EncounterZoneType zonaFallback = atributosZona != null ? atributosZona.GetZoneTypeById(atributosZona.ID) : EncounterZoneType.BosqueAngustiante;
        encuentroZonaActual = zonaFallback;
        TryGenerarEncuentro(BattleEncounterType.Subterraneo, zonaFallback, null, out encuentroGeneradoActual, faseClamped, true);
    }

    ActualizarLista();
}

bool EsResultadoVictoria(int resultado)
{
   return resultado == 1;
}

string ObtenerAnalyticsEncounterToken()
{
   if (encuentroGeneradoActual != null)
   {
      if (!string.IsNullOrWhiteSpace(encuentroGeneradoActual.factionId))
      {
         return "gen_" + RuntimeAnalytics.SanitizeToken(encuentroGeneradoActual.factionId);
      }

      return "generated";
   }

   if (EventoBatallaID > 0)
   {
      return "legacy_" + EventoBatallaID;
   }

   return "procedural";
}

bool EsResultadoDerrota(int resultado)
{
   return !EsResultadoVictoria(resultado);
}

void PrepararContextoBitacoraBatalla()
{
   if (CampaignManager.Instance == null || CampaignManager.Instance.logDeCampania == null)
   {
      return;
   }

   List<Personaje> participantes = new List<Personaje>(4);
   AgregarParticipanteBitacora(participantes, scAdministradorEscenas != null ? scAdministradorEscenas.Personaje1 : null);
   AgregarParticipanteBitacora(participantes, scAdministradorEscenas != null ? scAdministradorEscenas.Personaje2 : null);
   AgregarParticipanteBitacora(participantes, scAdministradorEscenas != null ? scAdministradorEscenas.Personaje3 : null);
   AgregarParticipanteBitacora(participantes, scAdministradorEscenas != null ? scAdministradorEscenas.Personaje4 : null);

   string factionId = encuentroGeneradoActual != null ? encuentroGeneradoActual.factionId : string.Empty;
   string factionName = ObtenerNombreFaccionBitacoraActual();

   CampaignManager.Instance.logDeCampania.PrepararContextoBatalla(
      factionId,
      factionName,
      EventoBatallaID,
      esEmboscadaEnemiga,
      participantes);
}

void AgregarParticipanteBitacora(List<Personaje> participantes, Personaje personaje)
{
   if (participantes == null || personaje == null || participantes.Contains(personaje))
   {
      return;
   }

   participantes.Add(personaje);
}

string ObtenerNombreFaccionBitacoraActual()
{
   if (encuentroGeneradoActual != null)
   {
      return ObtenerNombreFaccionTraducido(encuentroGeneradoActual.factionId, encuentroGeneradoActual.factionName);
   }

   if (EventoBatallaID >= 500 && EventoBatallaID <= 599)
   {
      return ObtenerNombreFaccionTraducido("Bandidos", "Bandidos");
   }

   if (EventoBatallaID >= 600 && EventoBatallaID <= 699)
   {
      return ObtenerNombreFaccionTraducido("Corruptos", "Corruptos");
   }

   if (EventoBatallaID == 700)
   {
      return ObtenerNombreFaccionTraducido("Bandidos", "Bandidos");
   }

   if (EventoBatallaID == 701)
   {
      return ObtenerNombreFaccionTraducido("Criaturas del Bosque", "Criaturas del Bosque");
   }

   if (EventoBatallaID == 100)
   {
      return ObtenerNombreFaccionTraducido("Zarkil", "Zarkil");
   }

   if (encuentroTipoActual == BattleEncounterType.Subterraneo)
   {
      return ObtenerNombreAmenazaSubterraneaPorIdioma();
   }

   if (EsNodoActualRitualKaleTav())
   {
      return ObtenerNombreFaccionTraducido(KaleTavFactionId, KaleTavFactionId);
   }

   return string.Empty;
}

string ObtenerNombreAmenazaSubterraneaPorIdioma()
{
   return Bitacora.ObtenerNombreAmenazaSubterraneaBitacora();
}

bool DebeOtorgarEstadoNegativoPorDerrota()
{
   return encuentroTipoActual == BattleEncounterType.Normal || encuentroTipoActual == BattleEncounterType.Elite;
}

void OtorgarEstadoNegativoPorDerrota()
{
   if (CampaignManager.Instance == null)
   {
      return;
   }

   TipoEstadoCaravana estado = EstadosNegativosDerrotaBatalla[UnityEngine.Random.Range(0, EstadosNegativosDerrotaBatalla.Length)];
   CampaignManager.Instance.AgregarEstadoCaravana(estado, 1);

   string log = estado switch
   {
      TipoEstadoCaravana.Acobardados => "-La derrota dejó a la Caravana con Acobardados.",
      TipoEstadoCaravana.Aletargados => "-La derrota dejó a la Caravana con Aletargados.",
      TipoEstadoCaravana.Desmotivacion => "-La derrota dejó a la Caravana con Desmotivación.",
      TipoEstadoCaravana.Descuidados => "-La derrota dejó a la Caravana con Descuidados.",
      _ => string.Empty
   };

   if (!string.IsNullOrEmpty(log))
   {
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir(log));
   }
}

public void EfectosDeBatallaEnCampaña(int resultado)
{
   UIEmpezarBatalla.SetActive(false);
   UITerminarBatalla.SetActive(true);
   LimpiarItemReward();
   ActualizarVisibilidadListasBatalla();
   transicionJefeZonaPendiente = false;
   tooltipPersonajeHeridoPendiente = false;
   tooltipPersonajeCorruptoPendiente = false;
   tooltipPersonajeFatigadoBatallaLargaPendiente = false;
   bool fueBatallaFinalActual = esBatallaFinal;
   bool fueDefensaCaravana = encuentroTipoActual == BattleEncounterType.AtaqueCaravana || esEmboscadaEnemiga == 3;
   string battleTypeToken = RuntimeAnalytics.SanitizeToken(encuentroTipoActual.ToString());
   string encounterToken = ObtenerAnalyticsEncounterToken();

    if (EsResultadoVictoria(resultado))
    {
        RuntimeAnalytics.TrackProgressionComplete("battle", battleTypeToken, encounterToken);
    }
    else
    {
        RuntimeAnalytics.TrackProgressionFail("battle", battleTypeToken, encounterToken);
    }

    if (EsResultadoVictoria(resultado))
    {
        txtVictoria.SetActive(true);
        txtDerrota.SetActive(false);
    }
    else
    {
        txtVictoria.SetActive(false);
        txtDerrota.SetActive(true);
    }

    if (CampaignManager.Instance != null)
    {
        CampaignManager.Instance.RegistrarBatallaLibrada();
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

   if (EsResultadoVictoria(resultado))
   {
      AplicarTraitsVictoriaParticipantes();
      AplicarVictoriaRitualKaleTav();
      if (esBatallaFinal && CampaignManager.Instance != null)
      {
         transicionJefeZonaPendiente = true;
      }
      else if (CampaignManager.Instance.scTutorialManager.tutorialActivo && EventoBatallaID == 701)
      {
         RuntimeAnalytics.TrackProgressionComplete("tutorial", "campaign", "intro");
         tutorialFinalPostBatallaPendiente = true;
      }
   }
    else if (EsResultadoDerrota(resultado) && CampaignManager.Instance != null)
    {
        CampaignManager.Instance.EvaluarDerrotaPorResultadoBatalla(fueDefensaCaravana, fueBatallaFinalActual);
        if (DebeOtorgarEstadoNegativoPorDerrota())
        {
            OtorgarEstadoNegativoPorDerrota();
        }
    }
    //Al perder se tiran chances de eliminar sequito. 50% - (5% + 3% por tier) mejora Defensa
    if (EsResultadoDerrota(resultado)) //Al perder se tiran chances de eliminar sequito. 50% - (5% + 3% por tier) mejora Defensa
    {
        int rand =UnityEngine.Random.Range(1, 101);
        int prob = Mathf.Clamp(50 - (5 + CampaignManager.Instance.mejoraCaravanaDefensas * 3), 0, 100);
        if (rand < prob) // chances de perder un sequito al perder una pelea
            {
                if (CampaignManager.Instance.scMenuSequito.SequitoAlAzarPerdido(out string nombre))
                {
                    txtRecompensa.text += TRADU.i.Traducir("\n\n-Los enemigos han eliminado al ") + nombre + TRADU.i.Traducir(" luego de la Batalla.");
                }
            }
        
    }
    
    //Al ganar puede tocar un item de la lista total del sequito de mercaderes al azar
        if (EsResultadoVictoria(resultado)) //Al ganar puede tocar un item de la lista total del sequito de mercaderes al azar
        {
            int rand =UnityEngine.Random.Range(1, 101);
            aumentochancesitem += CampaignManager.Instance.ObtenerBonusObjetosPostBatallaAntorchas();
            int prob = 30+aumentochancesitem; //!! 30%
            if (rand < prob) // chances de perder un sequito al perder una pelea
            {
                Item recompensa = CampaignManager.Instance.scMenuSequito.Sequito003Mercaderes.GetComponent<SequitoMercaderes>().ObtenerItemAlAzar();
                if (recompensa != null)
                {
                    CampaignManager.Instance.scMenuPersonajes.scEquipo.listInventario.Add(recompensa.gameObject);
                    MostrarItemReward(recompensa);
                    txtRecompensa.text += TRADU.i.Traducir("\n\n- Has encontrado un objeto de recompensa: ") + TraducirNombreVisibleRecompensa(recompensa) + ".";
                }

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

 void LimpiarItemReward()
 {
    GameObject itemRewardGO = ObtenerItemReward();
    if (itemRewardGO == null)
    {
       return;
    }

    foreach (Transform hijo in itemRewardGO.transform)
    {
       Destroy(hijo.gameObject);
    }

    itemRewardGO.SetActive(false);
 }

 void MostrarItemReward(Item item)
 {
    GameObject itemRewardGO = ObtenerItemReward();
    if (item == null || itemRewardGO == null)
    {
       return;
    }

    MenuPersonajes menuPersonajes = CampaignManager.Instance != null ? CampaignManager.Instance.scMenuPersonajes : null;
    Equipo equipo = menuPersonajes != null ? menuPersonajes.scEquipo : null;
    if (equipo == null || equipo.prefabNtnInventario == null)
    {
       return;
    }

    itemRewardGO.SetActive(true);
    GameObject btnItem = Instantiate(equipo.prefabNtnInventario, itemRewardGO.transform);
    btnItemInventario scBtnItem = btnItem.GetComponent<btnItemInventario>();
    if (scBtnItem == null)
    {
       return;
    }

    scBtnItem.imageMuestraItem.sprite = item.imItem;
    scBtnItem.itemRepresentado = item;
    scBtnItem.scMenuPersonajes = menuPersonajes;
    scBtnItem.SetOscurecido(false);
 }

 string ObtenerNombreVisibleRecompensa(Item item)
 {
    if (item == null || string.IsNullOrWhiteSpace(item.sNombreItem))
    {
       return string.Empty;
    }

    string nombre = item.sNombreItem.Trim();
    if (item.nivelMejora <= 0)
    {
       return nombre;
    }

    string sufijo = " +" + item.nivelMejora;
    if (nombre.EndsWith(sufijo))
    {
       return nombre;
    }

    return nombre + sufijo;
 }

 string TraducirNombreVisibleRecompensa(Item item)
 {
    string nombreVisible = ObtenerNombreVisibleRecompensa(item);
    if (string.IsNullOrEmpty(nombreVisible) || TRADU.i == null)
    {
       return nombreVisible;
    }

    if (item != null && item.nivelMejora > 0)
    {
       string sufijo = " +" + item.nivelMejora;
       if (nombreVisible.EndsWith(sufijo))
       {
          string nombreBase = nombreVisible.Substring(0, nombreVisible.Length - sufijo.Length);
          return TRADU.i.Traducir(nombreBase) + sufijo;
       }
    }

    return TRADU.i.Traducir(nombreVisible);
 }

 GameObject ObtenerItemReward()
 {
    if (itemReward != null)
    {
       return itemReward;
    }

    if (UITerminarBatalla == null)
    {
       return null;
    }

    Transform itemRewardTransform = BuscarHijoPorNombre(UITerminarBatalla.transform, ItemRewardNombre);
    itemReward = itemRewardTransform != null ? itemRewardTransform.gameObject : null;
    return itemReward;
 }

 Transform BuscarHijoPorNombre(Transform raiz, string nombre)
 {
    foreach (Transform hijo in raiz)
    {
       if (hijo.name == nombre)
       {
          return hijo;
       }

       Transform encontrado = BuscarHijoPorNombre(hijo, nombre);
       if (encontrado != null)
       {
          return encontrado;
       }
    }

    return null;
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

 int AplicarBonusExperienciaPrimeraBatallaRun(int experienciaBase)
 {
    CampaignManager campaignManager = CampaignManager.Instance;
    if (campaignManager == null)
    {
       return experienciaBase;
    }

    if (campaignManager.ObtenerEstadisticaBatallasLibradas() != 1)
    {
       return experienciaBase;
    }

    return experienciaBase + BonusExperienciaPrimeraBatallaRun;
 }

    public void AdministrarHeridas(Personaje pers, Unidad uni)
    {
       
        if (pers != null)
        {
            bool seHirioEnEstaBatalla = false;
            bool seCorrompioEnEstaBatalla = false;
            bool cayoEnCombate = uni != null ? uni.HP_actual < 1f : pers.fVidaActual < 1f;
            if (cayoEnCombate)
            {
                if (pers.TieneRasgo(PersonajeTraitCatalog.TraitEndeble))
                {
                    bool fallaFortitud = uni != null
                        ? uni.TiradaSalvacion(1, 11f)
                        : pers.FalloTiradaSalvacionFortalezaCampania(11);

                    if (fallaFortitud)
                    {
                        pers.Camp_Muerto = true;
                        CampaignManager.Instance.AplicarTraitHeroeLocalMuerteSiCorresponde(pers);

                        int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
                        string mensajeEndeble = idiomaTrait switch
                        {
                            TRADU.IdiomaIngles => pers.sNombre + " fails to endure the fall and dies permanently.",
                            TRADU.IdiomaPortugues => pers.sNombre + " nao resiste à queda e morre permanentemente.",
                            _ => pers.sNombre + " no resiste la caída y muere permanentemente."
                        };
                        CampaignManager.Instance.EscribirLog("-" + mensajeEndeble);
                    }
                }

                if (!pers.Camp_Muerto && pers.Camp_Herido) //Si ya estaba herido
                {
                    //muere
                    pers.Camp_Muerto = true;
                    CampaignManager.Instance.AplicarTraitHeroeLocalMuerteSiCorresponde(pers);
                }
                else if (!pers.Camp_Muerto) //Si no estaba herido, hiere
                {
                    pers.Camp_Herido = true;
                    pers.fVidaActual = 5;
                    seHirioEnEstaBatalla = true;
                }

                if (!pers.Camp_Muerto && uni != null)
                {
                if (uni.loMatoCorrompido) //Si lo mató un corrupto, se marca como corrupto
                {

                    if (pers.Camp_Corrupto)
                    {
                         //muere
                         pers.Camp_Muerto = true;
                         CampaignManager.Instance.AplicarTraitHeroeLocalMuerteSiCorresponde(pers);

                    }
                    else
                    {
                        pers.Camp_Corrupto = true;
                        seCorrompioEnEstaBatalla = true;
                        CampaignManager.Instance.EscribirLog("-" + uni.uNombre + TRADU.i.Traducir(" ha sido corrompido."));


                    }
                   
                    
                }
                }

            }

            if (!pers.Camp_Muerto && pers.Camp_Herido && pers.TieneRasgo(PersonajeTraitCatalog.TraitResistente))
            {
                bool fallaFortitud = uni != null
                    ? uni.TiradaSalvacion(1, 13f)
                    : pers.FalloTiradaSalvacionFortalezaCampania(13);

                if (!fallaFortitud)
                {
                    pers.Camp_Herido = false;

                    int idiomaTrait = PersonajeTraitCatalog.ObtenerIdiomaActual();
                    string mensajeResistente = idiomaTrait switch
                    {
                        TRADU.IdiomaIngles => pers.sNombre + " withstands the aftermath of battle and removes Wound.",
                        TRADU.IdiomaPortugues => pers.sNombre + " aguenta as sequelas da batalha e remove Ferida.",
                        _ => pers.sNombre + " resiste las secuelas de la pelea y se cura Herida."
                    };
                    CampaignManager.Instance.EscribirLog("-" + mensajeResistente);
                }
            }

            if (seHirioEnEstaBatalla && pers.Camp_Herido && !pers.Camp_Muerto)
            {
                tooltipPersonajeHeridoPendiente = true;
            }

            if (seCorrompioEnEstaBatalla && pers.Camp_Corrupto && !pers.Camp_Muerto)
            {
                tooltipPersonajeCorruptoPendiente = true;
            }

        
           

            //Actualiza estados visuales herida y muerte
            foreach (Transform boton in contenedorUIPersonajes.transform)
            {
                btnPersonaje btn = boton.gameObject.GetComponent<btnPersonaje>();
                if (btn != null)
                {
                    btn.RepresentarTodo();
                }

            }

        }
    }

 public void CerrarMenuBatalla()
    {
      bool debeMostrarPostBatallaFinalTutorial = tutorialFinalPostBatallaPendiente;
      TutorialEvents.Emit("ui.batsalir1_presionado", gameObject);
      if (!debeMostrarPostBatallaFinalTutorial
          && CampaignManager.Instance.scTutorialManager.tutorialActivo
          && CampaignManager.Instance.scTutorialManager.pasoActual == 4)
      {
         CampaignManager.Instance.scTutorialManager.SiguientePaso();
      }
        bool debeResolverTransicionJefeZona = transicionJefeZonaPendiente;
        transicionJefeZonaPendiente = false;
        tutorialFinalPostBatallaPendiente = false;
        gameObject.SetActive(false);

        if (debeMostrarPostBatallaFinalTutorial && CampaignManager.Instance != null)
        {
          if (CampaignManager.Instance.scTutorialManager != null
              && CampaignManager.Instance.scTutorialManager.tutorialActivo)
          {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
          }

          TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.BattleFinalTutorialPostBattleContinued, gameObject)
            .Add("encounterId", 701));
          CampaignManager.Instance.AbrirCiudadPuerto(); // fin tutorial
        }

        if (!debeResolverTransicionJefeZona && CampaignManager.Instance != null)
        {
          CampaignManager.Instance.TryAutosaveCampania("post-batalla", out _);
        }

    if (debeResolverTransicionJefeZona && CampaignManager.Instance != null)
    {
      CampaignManager.Instance.OnDerrotadoJefeZona();
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.estadosCaravana != null)
    {
      CampaignManager.Instance.estadosCaravana.FinalizarCombateActual();
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null)
    {
      CampaignManager.Instance.scMenuPersonajes.RefrescarListaVisual();
    }

    MostrarTooltipsPostBatallaSiCorresponde();
 }

 public void RegistrarTooltipFatigadoPorBatallaLarga()
 {
    tooltipPersonajeFatigadoBatallaLargaPendiente = true;
 }

 void MostrarTooltipsPostBatallaSiCorresponde()
 {
    if (tooltipPersonajeCorruptoPendiente)
    {
      tooltipPersonajeCorruptoPendiente = false;
      TutorialTooltipManager.TryShow(TooltipPersonajeCorruptoId);
    }

    if (tooltipPersonajeHeridoPendiente)
    {
      tooltipPersonajeHeridoPendiente = false;
      TutorialTooltipManager.TryShow(TooltipPersonajeHeridoId);
    }

    if (tooltipPersonajeFatigadoBatallaLargaPendiente)
    {
      tooltipPersonajeFatigadoBatallaLargaPendiente = false;
      TutorialTooltipManager.TryShow(TooltipPersonajeFatigadoBatallaLargaId);
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
    if (DebeExigirTresParticipantesBatallaFinalTutorial() && ContarAliadosSeleccionados() < 3)
    {
      bloqueoComenzarBatalla = false;
      SetBotonComenzarInteractable(true);
      ActualizarLista();
      return;
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.estadosCaravana != null)
    {
      CampaignManager.Instance.estadosCaravana.IniciarCombateActual();
    }
    // Autorelleno para Ataque a Caravana: prioriza personajes con m1s vida
    if (UIEmpezarBatallaACaravana != null && UIEmpezarBatallaACaravana.activeInHierarchy)
    {
        esEmboscadaEnemiga = 3; // Marcar tipo Ataque a Caravana
        if (scAdministradorEscenas.PersonajesSorprendidosInicioCaravana == null)
        {
            scAdministradorEscenas.PersonajesSorprendidosInicioCaravana = new List<Personaje>();
        }
        else
        {
            scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Clear();
        }

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

            void AgregarSorprendidoSiCorresponde(Personaje personaje)
            {
                if (personaje == null) return;
                if (EsGuardiaParaCaravana(personaje)) return;
                if (!scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Contains(personaje))
                {
                    scAdministradorEscenas.PersonajesSorprendidosInicioCaravana.Add(personaje);
                }
            }

            // Rellenar huecos en orden 1..4 respetando el límite
            if (maxAliadosPermitidos >= 1 && scAdministradorEscenas.Personaje1 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje1 = p;
                AgregarSorprendidoSiCorresponde(p);
            }
            if (maxAliadosPermitidos >= 2 && scAdministradorEscenas.Personaje2 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje2 = p;
                AgregarSorprendidoSiCorresponde(p);
            }
            if (maxAliadosPermitidos >= 3 && scAdministradorEscenas.Personaje3 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje3 = p;
                AgregarSorprendidoSiCorresponde(p);
            }
            if (maxAliadosPermitidos >= 4 && scAdministradorEscenas.Personaje4 == null && candidatos.Count > 0)
            {
                var p = candidatos[0]; candidatos.RemoveAt(0);
                scAdministradorEscenas.Personaje4 = p;
                AgregarSorprendidoSiCorresponde(p);
            }
        }

        ConfirmarOrdenRefuerzosCaravanaDesdeUI();
        scAdministradorEscenas.OrdenRefuerzosAliadosCaravana = ObtenerOrdenRefuerzosAliadosCaravana();
    }
    else if (esBatallaFinal)
    {
        ConfirmarOrdenRefuerzosCaravanaDesdeUI();
        scAdministradorEscenas.OrdenRefuerzosAliadosCaravana = ObtenerOrdenRefuerzosAliadosCaravana();
    }

    bool usarRefuerzosAliadosCaravana = esBatallaFinal;
    CampaignManager.Instance?.RegistrarInicioBatallaPresagioEnemigos();
    RuntimeAnalytics.TrackProgressionStart(
      "battle",
      RuntimeAnalytics.SanitizeToken(encuentroTipoActual.ToString()),
      ObtenerAnalyticsEncounterToken());
    PrepararContextoBitacoraBatalla();
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
        int exp = AplicarBonusExperienciaPrimeraBatallaRun(Mathf.RoundToInt(totalPoints * tuning.expPerPoint));
        int oro = Mathf.RoundToInt(totalPoints * tuning.goldPerPoint);
        int materiales = Mathf.RoundToInt(totalPoints * tuning.materialsPerPoint);
        int hopeBonus = Mathf.RoundToInt(tuning.victoryHopeBonus);

        if (oro != 0) CampaignManager.Instance.CambiarOroActual(oro);
        if (materiales != 0) CampaignManager.Instance.CambiarMaterialesActuales(materiales);
        if (exp > 0) DarExperiencia(exp);
        if (hopeBonus != 0) CampaignManager.Instance.CambiarEsperanzaActual(hopeBonus);
        if (oro > 0) RuntimeAnalytics.TrackResourceSource("gold", oro, "battle_reward", RuntimeAnalytics.SanitizeToken(encuentroTipoActual.ToString()));
        if (materiales > 0) RuntimeAnalytics.TrackResourceSource("materials", materiales, "battle_reward", RuntimeAnalytics.SanitizeToken(encuentroTipoActual.ToString()));

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
            int exp = AplicarBonusExperienciaPrimeraBatallaRun(180);
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

    int deltaEsperanza = resultado == 1 ? +5 : -5;
    int expVictoria = resultado == 1 ? AplicarBonusExperienciaPrimeraBatallaRun(0) : 0;

    if (expVictoria > 0)
    {
        DarExperiencia(expVictoria);
    }

    if (txtRecompensa != null)
    {
        if (resultado == 1)
        {
            if (expVictoria > 0)
            {
                txtRecompensa.text = FormatearTextoVictoria(expVictoria, 0, 0, deltaEsperanza);
            }
            else
            {
                txtRecompensa.text = TRADU.i.Traducir("Victoria sin recompensas definidas para este encuentro clásico.");
            }
        }
        else
        {
            txtRecompensa.text = TRADU.i.Traducir("Derrota en un encuentro clásico. Los efectos específicos aún no están configurados.");
        }
    }

    CampaignManager.Instance.CambiarEsperanzaActual(deltaEsperanza);
 }

 public BattleEncounterType ObtenerTipoEncuentroActual()
 {
    return encuentroTipoActual;
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
        partesEs.Add("-" + civilLoss + " Civiles");
        partesEn.Add("-" + civilLoss + " Civilians");
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
    if (nodoActual == null || nodoActual.tipoNodo != 15 || !nodoActual.nodoRitual)
    {
        return;
    }

    nodoActual.DesactivarRitual();

    manager.CambiarEsperanzaActual(10);
    manager.EscribirLog(TRADU.i.Traducir("-El ritual Kale'Tav ha sido detenido. +10 Esperanza."));
 }
}



