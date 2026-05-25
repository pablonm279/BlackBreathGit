using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialDirector : MonoBehaviour
{
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
  private Coroutine timedAdvanceCoroutine;

  public bool IsRunning => running;
  public TutorialDefinition ActiveDefinition => activeDefinition;
  public TutorialStep CurrentStep => activeDefinition != null ? activeDefinition.GetStep(stepIndex) : null;

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

    PlayerPrefs.DeleteKey(PendingStartAfterZoneDescriptionKey);
    PlayerPrefs.Save();
    director.StartDefaultTutorial();
    return true;
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
    ExitCurrentStep();
    stepIndex--;
    EnterCurrentStep();
  }

  public void SkipTutorial()
  {
    StopTimedAdvance();
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
    if (PlayerPrefs.GetInt(PendingStartAfterZoneDescriptionKey, 0) == 1)
    {
      return false;
    }

    return PlayerPrefs.GetInt(LegacyTutorialCompletedKey, 0) != 1;
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
}
