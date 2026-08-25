using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialDirector : MonoBehaviour
{
  private const string TutorialCampaniaVerticalSliceId = "vertical_slice_intro";
  private const string PrimerPasoBloqueoMenus = "Intro";
  private const string UltimoPasoBloqueoMenus = "Recursos1";
  private const string PasoBloqueoCierreMenuPersonaje = "MenuPersonaje";
  private const int SaveVersionConEstadoTutorialNuevo = 16;
  private const string CompletedKeyPrefix = "TutorialNuevo_Completado_";
  private const string DefaultDefinitionPath = "Tutoriales/TutorialVerticalSlice";
  public const string DefaultTutorialId = "vertical_slice_intro";
  public const string PendingStartAfterZoneDescriptionKey = "TutorialNuevo_IniciarTrasDescripcionBosqueArdiente";
  private const string LegacyTutorialCompletedKey = "Tutorial_Terminado";
  private const string FinalStepId = "postbatfinal1";
  private const string MainMenuSceneName = "ES-MenuPrincipal";

  public static TutorialDirector Instance { get; private set; }

  [SerializeField] private TutorialDefinition activeDefinition;
  [SerializeField] private TutorialPresenter presenter;
  [SerializeField] private bool autoStartOnStart = true;
  [SerializeField] private bool persistAcrossScenes = true;
  [SerializeField] private string startFromStepId;

  private int stepIndex = -1;
  private bool running;
  private bool suspendedForLegacyCombatTutorial;
  private bool autoStartPendienteTrasIntro;
  private bool pendingStartPendienteTrasIntro;
  private Coroutine timedAdvanceCoroutine;
  private static PendingRestoreState pendingRestoreState;

  public bool IsRunning => running;
  public TutorialDefinition ActiveDefinition => activeDefinition;
  public TutorialStep CurrentStep => activeDefinition != null ? activeDefinition.GetStep(stepIndex) : null;
  public string CurrentStepId => CurrentStep != null ? CurrentStep.id : string.Empty;
  public bool BlocksOptionalCampaignMenus => BloqueaMenusOpcionalesCampania();
  public bool BlocksCharacterMenuClose => running
    && !suspendedForLegacyCombatTutorial
    && activeDefinition != null
    && activeDefinition.tutorialId == TutorialCampaniaVerticalSliceId
    && CurrentStepId == PasoBloqueoCierreMenuPersonaje;

  private struct PendingRestoreState
  {
    public bool hasValue;
    public bool running;
    public bool pendingAfterZoneDescription;
    public string tutorialId;
    public string stepId;
  }

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;

    if (persistAcrossScenes)
    {
      DontDestroyOnLoad(gameObject);
    }

    EnsurePresenter();
  }

  private void OnEnable()
  {
    TutorialEvents.EventEmitted += OnTutorialEvent;
  }

  private void OnDisable()
  {
    TutorialEvents.EventEmitted -= OnTutorialEvent;
    StopTimedAdvance();
  }

  private void Start()
  {
    if (activeDefinition == null)
    {
      activeDefinition = Resources.Load<TutorialDefinition>(DefaultDefinitionPath);
    }

    if (TryApplyPendingRestoreState())
    {
      return;
    }

    if (autoStartOnStart && activeDefinition != null && DebeAutoIniciarEnStart())
    {
      StartTutorial(activeDefinition, startFromStepId);
    }
  }

  public void StartDefaultTutorial(string stepId = "")
  {
    if (activeDefinition == null)
    {
      activeDefinition = Resources.Load<TutorialDefinition>(DefaultDefinitionPath);
    }

    StartTutorial(activeDefinition, string.IsNullOrEmpty(stepId) ? startFromStepId : stepId);
  }

  public static bool TryStartPendingAfterZoneDescription()
  {
    if (PlayerPrefs.GetInt(PendingStartAfterZoneDescriptionKey, 0) != 1)
    {
      return false;
    }

    TutorialDirector director = Instance != null ? Instance : FindObjectOfType<TutorialDirector>(true);
    if (director == null)
    {
      TutorialDirector[] directores = Resources.FindObjectsOfTypeAll<TutorialDirector>();
      for (int i = 0; i < directores.Length; i++)
      {
        if (directores[i] != null && directores[i].gameObject.scene.IsValid())
        {
          director = directores[i];
          break;
        }
      }
    }

    if (director == null)
    {
      return false;
    }

    if (!director.gameObject.activeSelf)
    {
      director.gameObject.SetActive(true);
    }

    if (!director.enabled)
    {
      director.enabled = true;
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.IntroCampaniaActivaOPendiente)
    {
      director.pendingStartPendienteTrasIntro = true;
      return false;
    }

    PlayerPrefs.DeleteKey(PendingStartAfterZoneDescriptionKey);
    PlayerPrefs.Save();
    director.StartDefaultTutorial();
    return true;
  }

  public static void ReintentarAutoarranqueTrasIntroSiCorresponde()
  {
    TutorialDirector director = Instance != null ? Instance : FindObjectOfType<TutorialDirector>(true);
    if (director == null)
    {
      return;
    }

    director.ReintentarArranqueTrasIntroSiCorresponde();
  }

  public static void PrepararRestauracionDesdeSave(CampaignSaveData data, int saveVersion)
  {
    if (saveVersion < SaveVersionConEstadoTutorialNuevo)
    {
      CancelarRestauracionPendiente();
      return;
    }

    pendingRestoreState = new PendingRestoreState
    {
      hasValue = true,
      running = data != null && data.tutorialNuevoActivo,
      pendingAfterZoneDescription = data != null && data.tutorialNuevoPendienteTrasDescripcionZona,
      tutorialId = data != null ? data.tutorialNuevoId : string.Empty,
      stepId = data != null ? data.tutorialNuevoPasoId : string.Empty
    };
  }

  public static void AplicarRestauracionPendienteSiCorresponde()
  {
    TutorialDirector director = Instance != null ? Instance : FindObjectOfType<TutorialDirector>(true);
    if (director == null)
    {
      return;
    }

    director.TryApplyPendingRestoreState();
  }

  public static void CancelarRestauracionPendiente()
  {
    pendingRestoreState = new PendingRestoreState();
  }

  public static bool HayTutorialActivoOPendiente()
  {
    if (pendingRestoreState.hasValue)
    {
      return pendingRestoreState.running || pendingRestoreState.pendingAfterZoneDescription;
    }

    return PlayerPrefs.GetInt(PendingStartAfterZoneDescriptionKey, 0) == 1
      || (Instance != null && Instance.IsRunning);
  }

  public void StartTutorial(TutorialDefinition definition, string stepId = "")
  {
    if (definition == null || definition.steps == null || definition.steps.Count == 0)
    {
      return;
    }

    if (!definition.restartIfCompleted && PlayerPrefs.GetInt(GetCompletedKey(definition.tutorialId), 0) == 1)
    {
      return;
    }

    activeDefinition = definition;
    int requestedIndex = definition.GetStepIndex(stepId);
    stepIndex = requestedIndex >= 0 ? requestedIndex : 0;
    running = true;
    suspendedForLegacyCombatTutorial = false;
    StopTimedAdvance();
    EnterCurrentStep();
  }

  public void NextStep()
  {
    if (!running)
    {
      return;
    }

    TutorialStep exitingStep = CurrentStep;
    StopTimedAdvance();
    if (presenter != null)
    {
      presenter.StopCurrentStepNarration();
    }
    ExitCurrentStep();
    stepIndex++;

    if (activeDefinition == null || stepIndex >= activeDefinition.steps.Count)
    {
      CompleteTutorial(exitingStep != null && exitingStep.id == FinalStepId);
      return;
    }

    EnterCurrentStep();
  }

  public void PreviousStep()
  {
    if (!running || stepIndex <= 0)
    {
      return;
    }

    StopTimedAdvance();
    if (presenter != null)
    {
      presenter.StopCurrentStepNarration();
    }
    ExitCurrentStep();
    stepIndex--;
    EnterCurrentStep();
  }

  public void SkipTutorial()
  {
    StopTimedAdvance();
    if (presenter != null)
    {
      presenter.StopCurrentStepNarration();
    }
    CompleteTutorial();
  }

  public bool AllowsInput(string targetId)
  {
    TutorialStep step = CurrentStep;
    if (!running || suspendedForLegacyCombatTutorial || step == null || step.inputBlockMode == TutorialInputBlockMode.None)
    {
      return true;
    }

    if (step.inputBlockMode == TutorialInputBlockMode.All)
    {
      return false;
    }

    return !string.IsNullOrEmpty(step.targetId) && step.targetId == targetId;
  }

  private bool BloqueaMenusOpcionalesCampania()
  {
    if (!running
        || suspendedForLegacyCombatTutorial
        || activeDefinition == null
        || activeDefinition.tutorialId != TutorialCampaniaVerticalSliceId)
    {
      return false;
    }

    int primerPaso = activeDefinition.GetStepIndex(PrimerPasoBloqueoMenus);
    int ultimoPaso = activeDefinition.GetStepIndex(UltimoPasoBloqueoMenus);
    return primerPaso >= 0
      && ultimoPaso >= primerPaso
      && stepIndex >= primerPaso
      && stepIndex <= ultimoPaso;
  }

  private void OnTutorialEvent(TutorialEventPayload payload)
  {
    TutorialStep step = CurrentStep;
    if (!running || suspendedForLegacyCombatTutorial || step == null || step.advanceMode != TutorialAdvanceMode.Event)
    {
      return;
    }

    for (int i = 0; i < step.advanceConditions.Count; i++)
    {
      TutorialCondition condition = step.advanceConditions[i];
      if (condition != null && condition.Matches(payload))
      {
        NextStep();
        return;
      }
    }
  }

  public void SuspendForLegacyCombatTutorial()
  {
    if (!running || suspendedForLegacyCombatTutorial)
    {
      return;
    }

    suspendedForLegacyCombatTutorial = true;
    StopTimedAdvance();

    if (presenter != null)
    {
      presenter.Hide();
    }
  }

  public void ResumeFromLegacyCombatTutorial(bool advanceStep)
  {
    if (!suspendedForLegacyCombatTutorial)
    {
      return;
    }

    suspendedForLegacyCombatTutorial = false;

    if (!running)
    {
      return;
    }

    if (advanceStep)
    {
      NextStep();
      return;
    }

    EnterCurrentStep();
  }

  private void EnterCurrentStep()
  {
    TutorialStep step = CurrentStep;
    if (step == null)
    {
      CompleteTutorial();
      return;
    }

    ExecuteActions(step.enterActions);
    PrepararUiRuntimeParaPaso(step);
    EnsurePresenter();

    if (presenter != null)
    {
      presenter.Show(step, stepIndex > 0);
    }

    if (step.advanceMode == TutorialAdvanceMode.Timed)
    {
      timedAdvanceCoroutine = StartCoroutine(AdvanceAfterDelay(step.autoAdvanceDelay));
    }
  }

  private void ExitCurrentStep()
  {
    TutorialStep step = CurrentStep;
    if (step != null)
    {
      ExecuteActions(step.exitActions);
    }
  }

  private void ExecuteActions(System.Collections.Generic.List<TutorialAction> actions)
  {
    if (actions == null)
    {
      return;
    }

    for (int i = 0; i < actions.Count; i++)
    {
      ExecuteAction(actions[i]);
    }
  }

  private void ExecuteAction(TutorialAction action)
  {
    if (action == null)
    {
      return;
    }

    switch (action.type)
    {
      case TutorialActionType.EmitEvent:
        TutorialEvents.Emit(action.eventId, gameObject);
        break;
      case TutorialActionType.SetTargetActive:
        TutorialTarget target = TutorialTarget.Find(action.targetId);
        if (target != null)
        {
          target.gameObject.SetActive(action.active);
        }
        break;
      case TutorialActionType.CompleteTutorial:
        CompleteTutorial();
        break;
    }
  }

  private void CompleteTutorial(bool volverAlMenuPrincipal = false)
  {
    StopTimedAdvance();
    CancelarRestauracionPendiente();

    if (activeDefinition != null && !string.IsNullOrEmpty(activeDefinition.tutorialId))
    {
      PlayerPrefs.SetInt(GetCompletedKey(activeDefinition.tutorialId), 1);
    }

    PlayerPrefs.DeleteKey(PendingStartAfterZoneDescriptionKey);

    if (volverAlMenuPrincipal)
    {
      PlayerPrefs.SetInt(LegacyTutorialCompletedKey, 1);
    }

    PlayerPrefs.Save();

    running = false;
    stepIndex = -1;
    suspendedForLegacyCombatTutorial = false;

    if (presenter != null)
    {
      presenter.Hide();
    }

    if (volverAlMenuPrincipal)
    {
      Time.timeScale = 1f;
      if (MusicManager.Instance != null)
      {
        MusicManager.Instance.PausarMusica(false);
        MusicManager.Instance.FadeOutYParar(0.5f);
      }

      SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
    }
  }

  public static string GetCompletedPlayerPrefsKey(string tutorialId)
  {
    return CompletedKeyPrefix + tutorialId;
  }

  private static string GetCompletedKey(string tutorialId)
  {
    return GetCompletedPlayerPrefsKey(tutorialId);
  }

  private bool DebeAutoIniciarEnStart()
  {
    if (pendingRestoreState.hasValue)
    {
      return false;
    }

    if (PlayerPrefs.GetInt(PendingStartAfterZoneDescriptionKey, 0) == 1)
    {
      return false;
    }

    if (PlayerPrefs.GetInt(LegacyTutorialCompletedKey, 0) == 1)
    {
      return false;
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.IntroCampaniaActivaOPendiente)
    {
      autoStartPendienteTrasIntro = true;
      return false;
    }

    return true;
  }

  private void ReintentarArranqueTrasIntroSiCorresponde()
  {
    if (CampaignManager.Instance != null && CampaignManager.Instance.IntroCampaniaActivaOPendiente)
    {
      return;
    }

    if (TryApplyPendingRestoreState())
    {
      return;
    }

    if (pendingStartPendienteTrasIntro)
    {
      pendingStartPendienteTrasIntro = false;
      TryStartPendingAfterZoneDescription();
      return;
    }

    if (!autoStartPendienteTrasIntro || running)
    {
      return;
    }

    autoStartPendienteTrasIntro = false;
    if (activeDefinition == null)
    {
      activeDefinition = Resources.Load<TutorialDefinition>(DefaultDefinitionPath);
    }

    if (autoStartOnStart && activeDefinition != null && DebeAutoIniciarEnStart())
    {
      StartTutorial(activeDefinition, startFromStepId);
    }
  }

  private void EnsurePresenter()
  {
    if (presenter == null)
    {
      presenter = FindObjectOfType<TutorialPresenter>(true);
    }

    if (presenter != null)
    {
      presenter.Configure(this);
    }
  }

  private System.Collections.IEnumerator AdvanceAfterDelay(float delay)
  {
    yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay));
    timedAdvanceCoroutine = null;

    if (running && CurrentStep != null && CurrentStep.advanceMode == TutorialAdvanceMode.Timed)
    {
      NextStep();
    }
  }

  private void PrepararUiRuntimeParaPaso(TutorialStep step)
  {
    if (step == null || CampaignManager.Instance == null || CampaignManager.Instance.scMenuSequito == null)
    {
      return;
    }

    CampaignManager.Instance.scMenuSequito.MostrarContenidoSequitoTutorial(step.id);
  }

  private void StopTimedAdvance()
  {
    if (timedAdvanceCoroutine == null)
    {
      return;
    }

    StopCoroutine(timedAdvanceCoroutine);
    timedAdvanceCoroutine = null;
  }

  private bool TryApplyPendingRestoreState()
  {
    if (!pendingRestoreState.hasValue)
    {
      return false;
    }

    if (CampaignManager.Instance != null && CampaignManager.Instance.IntroCampaniaActivaOPendiente)
    {
      return false;
    }

    PendingRestoreState restoreState = pendingRestoreState;
    pendingRestoreState = new PendingRestoreState();

    autoStartPendienteTrasIntro = false;
    pendingStartPendienteTrasIntro = false;
    RestablecerEstadoRuntimeSinCompletar();

    if (restoreState.pendingAfterZoneDescription)
    {
      PlayerPrefs.SetInt(PendingStartAfterZoneDescriptionKey, 1);
    }
    else
    {
      PlayerPrefs.DeleteKey(PendingStartAfterZoneDescriptionKey);
    }

    PlayerPrefs.Save();

    if (!restoreState.running)
    {
      return true;
    }

    TutorialDefinition definition = ObtenerDefinitionParaTutorial(restoreState.tutorialId);
    if (definition == null || definition.steps == null || definition.steps.Count == 0)
    {
      return true;
    }

    activeDefinition = definition;
    stepIndex = definition.GetStepIndex(restoreState.stepId);
    if (stepIndex < 0)
    {
      stepIndex = 0;
    }

    running = true;
    suspendedForLegacyCombatTutorial = false;
    EnterCurrentStep();
    return true;
  }

  private void RestablecerEstadoRuntimeSinCompletar()
  {
    StopTimedAdvance();
    running = false;
    stepIndex = -1;
    suspendedForLegacyCombatTutorial = false;

    if (presenter != null)
    {
      presenter.Hide();
    }
  }

  private TutorialDefinition ObtenerDefinitionParaTutorial(string tutorialId)
  {
    if (activeDefinition != null
      && (string.IsNullOrEmpty(tutorialId) || activeDefinition.tutorialId == tutorialId))
    {
      return activeDefinition;
    }

    if (string.IsNullOrEmpty(tutorialId) || tutorialId == DefaultTutorialId)
    {
      activeDefinition = Resources.Load<TutorialDefinition>(DefaultDefinitionPath);
      return activeDefinition;
    }

    TutorialDefinition[] definiciones = Resources.LoadAll<TutorialDefinition>(string.Empty);
    for (int i = 0; i < definiciones.Length; i++)
    {
      if (definiciones[i] != null && definiciones[i].tutorialId == tutorialId)
      {
        activeDefinition = definiciones[i];
        return activeDefinition;
      }
    }

    return null;
  }
}
