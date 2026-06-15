using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPresenter : MonoBehaviour
{
  private const string DefaultLocalizationPath = "Tutoriales/TutorialVerticalSlice_Textos";
  private const string NarratorMutedKey = "TutorialNuevo_NarradorSilenciado";
  private const float NarratorMusicDuckingMaxVolume = 0.15f;
  private const float NarratorTextAudioDurationFactor = 0.85f;

  [SerializeField] private TutorialLocalizationTable localizationTable;
  [SerializeField] private GameObject panelRoot;
  [SerializeField] private GameObject combatPanelRoot;
  [SerializeField] private TextMeshProUGUI titleText;
  [SerializeField] private TextMeshProUGUI narratorText;
  [SerializeField] private TextMeshProUGUI bodyText;
  [SerializeField] private TextMeshProUGUI combatTitleText;
  [SerializeField] private TextMeshProUGUI combatNarratorText;
  [SerializeField] private TextMeshProUGUI combatBodyText;
  [SerializeField] private TMP_SpriteAsset inlineSpriteAsset;
  [SerializeField] private bool autoUseCampaignInlineSprites = true;
  [SerializeField] private bool animateNarratorText = true;
  [SerializeField, Min(1f)] private float narratorCharactersPerSecond = 45f;
  [SerializeField] private bool showNextAfterNarration = true;
  [SerializeField] private bool hidePanelWhileCampaignTravels = true;
  [SerializeField] private bool fitPanelInsideParent = true;
  [SerializeField, Min(0f)] private float panelViewportMargin = 24f;
  [SerializeField] private GameObject nextButtonRoot;
  [SerializeField] private Button nextButton;
  [SerializeField] private Button backButton;
  [SerializeField] private Button skipButton;
  [SerializeField] private Button muteButton;
  [SerializeField] private Button replayButton;
  [SerializeField] private AudioSource narratorAudioSource;
  [SerializeField, Min(0f)] private float narratorVolumeMultiplier = 2.875f;
  [SerializeField] private Graphic muteStateGraphic;
  [SerializeField] private Color muteColor = new Color(0.55f, 0.55f, 0.55f, 1f);
  [SerializeField] private Color unmuteColor = Color.white;
  [SerializeField] private GameObject mutedIndicator;
  [SerializeField] private GameObject unmutedIndicator;
  [SerializeField] private RectTransform highlight;
  [SerializeField] private RectTransform pointer;
  [SerializeField] private GameObject inputBlocker;

  private TutorialDirector director;
  private RectTransform panelRootRect;
  private RectTransform combatPanelRootRect;
  private GameObject activePanelRoot;
  private RectTransform activePanelRootRect;
  private TextMeshProUGUI activeTitleText;
  private TextMeshProUGUI activeNarratorText;
  private TextMeshProUGUI activeBodyText;
  private AudioClip currentNarratorAudio;
  private Coroutine narratorTextCoroutine;
  private Coroutine revealNextButtonCoroutine;
  private Coroutine narratorMusicDuckingCoroutine;
  private int narratorTextTotalCharacters;
  private TutorialStep currentStep;
  private bool hiddenForCampaignTravel;
  private bool hideUntilRestRandomEvent;
  private bool narratorMuted;
  private bool panelLayoutCached;
  private TutorialInputBlockerRaycastFilter inputBlockerRaycastFilter;
  private Vector2 panelOriginalAnchorMin;
  private Vector2 panelOriginalAnchorMax;
  private Vector2 panelOriginalPivot;
  private Vector2 panelOriginalAnchoredPosition;
  private Vector2 panelOriginalSizeDelta;
  private Vector3 panelOriginalScale;
  private readonly Dictionary<Graphic, bool> gatedGraphicsOriginalRaycastTarget = new Dictionary<Graphic, bool>();

  private void Reset()
  {
    AutoBindReferences();
  }

  private void Awake()
  {
    AutoBindReferences();
    narratorMuted = false;
    PlayerPrefs.SetInt(NarratorMutedKey, 0);
  }

  private void Update()
  {
    if (narratorTextCoroutine != null && Input.GetMouseButtonDown(0))
    {
      RevealNarratorTextImmediately();
      RevealNextButtonNow();
    }
  }

  private void OnEnable()
  {
    TutorialEvents.EventEmitted += OnTutorialEvent;
  }

  private void OnDisable()
  {
    TutorialEvents.EventEmitted -= OnTutorialEvent;
    RestoreGatedGraphics();
  }

  private void LateUpdate()
  {
    if (currentStep != null && !hiddenForCampaignTravel)
    {
      PositionTargetVisuals(currentStep);
    }

    if (ShouldSuspendInputGateForCampaignTravel())
    {
      RestoreGatedGraphics();
      return;
    }

    if (currentStep != null && currentStep.inputBlockMode != TutorialInputBlockMode.None)
    {
      ApplyGraphicInputGate(currentStep);
    }
  }

  public void Configure(TutorialDirector tutorialDirector)
  {
    director = tutorialDirector;
    AutoBindReferences();
    ApplyInlineSpriteAsset();

    if (nextButton != null)
    {
      nextButton.onClick.RemoveListener(OnNextClicked);
      nextButton.onClick.AddListener(OnNextClicked);
    }

    if (backButton != null)
    {
      backButton.onClick.RemoveListener(OnBackClicked);
      backButton.onClick.AddListener(OnBackClicked);
    }

    if (skipButton != null && !UsesSameButton(skipButton, nextButton) && !UsesSameButton(skipButton, backButton))
    {
      skipButton.onClick.RemoveListener(OnSkipClicked);
      skipButton.onClick.AddListener(OnSkipClicked);
    }

    if (muteButton != null)
    {
      muteButton.onClick.RemoveListener(OnMuteClicked);
      muteButton.onClick.AddListener(OnMuteClicked);
    }

    if (replayButton != null)
    {
      replayButton.onClick.RemoveListener(OnReplayClicked);
      replayButton.onClick.AddListener(OnReplayClicked);
    }

    UpdateMuteVisualState();
  }

  public void Show(TutorialStep step, bool canGoBack)
  {
    if (step == null)
    {
      Hide();
      return;
    }

    currentStep = step;
    hiddenForCampaignTravel = false;
    RestoreGatedGraphics();
    AudioClip narratorAudio = GetNarratorAudioForCurrentLanguage(step);
    SelectActivePanel(step);
    ApplyPanelPresentation(step);
    if (step.presentationMode != TutorialPresentationMode.Full)
    {
      FitActivePanelInsideParent();
    }
    ConfigurePanelContentVisibility(step);
    SetActive(panelRoot, activePanelRoot == panelRoot && ShouldShowPanel(step));
    SetActive(combatPanelRoot, activePanelRoot == combatPanelRoot && ShouldShowPanel(step));
    SetActive(inputBlocker, step.inputBlockMode == TutorialInputBlockMode.All);
    ArrangeTutorialLayers();

    if (activeTitleText != null)
    {
      activeTitleText.text = Translate(step.titleKey);
    }

    if (activeBodyText != null)
    {
      activeBodyText.text = Translate(step.bodyKey);
    }

    if (activeNarratorText != null)
    {
      ShowNarratorText(Translate(step.narratorKey), narratorAudio);
    }

    ApplyInlineSpriteAsset();
    ConfigureNextButtonVisibility(step, narratorAudio);
    SetActive(backButton != null ? backButton.gameObject : null, step.canGoBack && canGoBack);
    if (skipButton != null && !UsesSameButton(skipButton, nextButton) && !UsesSameButton(skipButton, backButton))
    {
      SetActive(skipButton.gameObject, step.canSkip);
    }
    PositionTargetVisuals(step);
    PlayNarratorAudio(narratorAudio);
    UpdateMuteVisualState();
    ApplyGraphicInputGate(step);
    ArrangeTutorialLayers();

    if (hideUntilRestRandomEvent && CurrentStepWaitsForEvent(TutorialEventNames.CampaignRestRandomEventContinued))
    {
      hiddenForCampaignTravel = true;
      SetTutorialVisualsVisible(false);
      RestoreGatedGraphics();
    }
  }

  public void Hide()
  {
    RestoreGatedGraphics();
    currentStep = null;
    StopRevealNextButtonDelay();
    StopNarratorTextAnimation();
    StopNarratorAudio();
    SetActive(panelRoot, false);
    SetActive(combatPanelRoot, false);
    SetActive(inputBlocker, false);
    SetActive(highlight != null ? highlight.gameObject : null, false);
    SetActive(pointer != null ? pointer.gameObject : null, false);
  }

  public void StopCurrentStepNarration()
  {
    StopRevealNextButtonDelay();
    StopNarratorTextAnimation();
    StopNarratorAudio();
  }

  private string Translate(string key)
  {
    return localizationTable != null ? localizationTable.Get(key) : key;
  }

  private void AutoBindReferences()
  {
    if (localizationTable == null)
    {
      localizationTable = Resources.Load<TutorialLocalizationTable>(DefaultLocalizationPath);
    }

    if (panelRoot == null)
    {
      Transform panel = FindChild("PanelTutorial");
      panelRoot = panel != null ? panel.gameObject : gameObject;
    }

    if (panelRootRect == null && panelRoot != null)
    {
      panelRootRect = panelRoot.GetComponent<RectTransform>();
    }

    if (combatPanelRoot == null)
    {
      Transform combatPanel = FindChild("PanelTutorialCombate") ?? FindChild("CombatPanelTutorial");
      combatPanelRoot = combatPanel != null ? combatPanel.gameObject : null;
    }

    if (combatPanelRootRect == null && combatPanelRoot != null)
    {
      combatPanelRootRect = combatPanelRoot.GetComponent<RectTransform>();
    }

    if (activePanelRoot == null)
    {
      activePanelRoot = panelRoot;
      activePanelRootRect = panelRootRect;
      activeTitleText = titleText;
      activeNarratorText = narratorText;
      activeBodyText = bodyText;
    }

    if (titleText == null)
    {
      titleText = FindText("Titulo");
    }

    if (bodyText == null)
    {
      bodyText = FindText("Cuerpo");
    }

    if (narratorText == null)
    {
      narratorText = FindText("Narrador");
    }

    if (combatPanelRoot != null)
    {
      if (combatTitleText == null)
      {
        combatTitleText = FindTextIn(combatPanelRoot, "Titulo");
      }

      if (combatBodyText == null)
      {
        combatBodyText = FindTextIn(combatPanelRoot, "Cuerpo");
      }

      if (combatNarratorText == null)
      {
        combatNarratorText = FindTextIn(combatPanelRoot, "Narrador");
      }
    }

    if (nextButton == null)
    {
      nextButton = FindButton("btnSiguiente");
    }

    if (nextButtonRoot == null)
    {
      nextButtonRoot = nextButton != null ? nextButton.gameObject : FindChildGameObject("btnSiguiente");
    }

    if (nextButton == null && nextButtonRoot != null)
    {
      nextButton = nextButtonRoot.GetComponent<Button>() ?? nextButtonRoot.GetComponentInChildren<Button>(true);
    }

    if (backButton == null)
    {
      backButton = FindButton("btnAnterior");
    }

    if (skipButton == null)
    {
      skipButton = FindButton("btnOmitir");
    }

    if (muteButton == null)
    {
      muteButton = FindButton("btnMute");
    }

    if (replayButton == null)
    {
      replayButton = FindButton("btnReplayNarrador") ?? FindButton("btnReplay");
    }

    if (muteStateGraphic == null && muteButton != null)
    {
      muteStateGraphic = muteButton.GetComponent<Graphic>();
    }

    if (mutedIndicator == null)
    {
      Transform muted = FindChild("MutedIndicator") ?? FindChild("IconoMute");
      mutedIndicator = muted != null ? muted.gameObject : null;
    }

    if (unmutedIndicator == null)
    {
      Transform unmuted = FindChild("UnmutedIndicator") ?? FindChild("IconoAudio");
      unmutedIndicator = unmuted != null ? unmuted.gameObject : null;
    }

    if (narratorAudioSource == null)
    {
      narratorAudioSource = GetComponent<AudioSource>();
    }

    if (narratorAudioSource == null)
    {
      narratorAudioSource = gameObject.AddComponent<AudioSource>();
      narratorAudioSource.playOnAwake = false;
    }

    if (inputBlocker == null)
    {
      Transform blocker = FindChild("OverlayInputBlocker") ?? FindChild("InputBlocker");
      inputBlocker = blocker != null ? blocker.gameObject : null;
    }

    ConfigureInputBlockerRaycastFilter();

    if (highlight == null)
    {
      Transform highlightTransform = FindChild("Highlight");
      highlight = highlightTransform != null ? highlightTransform.GetComponent<RectTransform>() : null;
    }

    if (pointer == null)
    {
      Transform pointerTransform = FindChild("Pointer") ?? FindChild("Flecha");
      pointer = pointerTransform != null ? pointerTransform.GetComponent<RectTransform>() : null;
    }

    if (inlineSpriteAsset == null && autoUseCampaignInlineSprites)
    {
      inlineSpriteAsset = FindInlineSpriteAsset();
    }
  }

  private void ArrangeTutorialLayers()
  {
    if (inputBlocker != null)
    {
      inputBlocker.transform.SetAsFirstSibling();
    }

    if (highlight != null)
    {
      highlight.SetAsLastSibling();
    }

    if (pointer != null)
    {
      pointer.SetAsLastSibling();
    }

    if (panelRoot != null)
    {
      panelRoot.transform.SetAsLastSibling();
    }

    if (combatPanelRoot != null && combatPanelRoot.activeSelf)
    {
      combatPanelRoot.transform.SetAsLastSibling();
    }
  }

  private TMP_SpriteAsset FindInlineSpriteAsset()
  {
    LogDeCampania log = FindObjectOfType<LogDeCampania>(true);
    if (log != null && log.SpriteAssetRecursos != null)
    {
      return log.SpriteAssetRecursos;
    }

    if (BattleManager.Instance != null && BattleManager.Instance.SpriteAssetCombate != null)
    {
      return BattleManager.Instance.SpriteAssetCombate;
    }

    return null;
  }

  private void ApplyInlineSpriteAsset()
  {
    if (inlineSpriteAsset == null && autoUseCampaignInlineSprites)
    {
      inlineSpriteAsset = FindInlineSpriteAsset();
    }

    if (inlineSpriteAsset == null)
    {
      return;
    }

   // ApplyInlineSpriteAsset(activeTitleText);
    ApplyInlineSpriteAsset(activeBodyText);
  }

  private void ApplyInlineSpriteAsset(TextMeshProUGUI text)
  {
    if (text == null)
    {
      return;
    }

    text.spriteAsset = inlineSpriteAsset;
    text.richText = true;
  }

  private TextMeshProUGUI FindText(string childName)
  {
    Transform child = FindChild(childName);
    return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
  }

  private static TextMeshProUGUI FindTextIn(GameObject root, string childName)
  {
    if (root == null || string.IsNullOrEmpty(childName))
    {
      return null;
    }

    TextMeshProUGUI[] texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
    for (int i = 0; i < texts.Length; i++)
    {
      if (texts[i] != null && texts[i].name == childName)
      {
        return texts[i];
      }
    }

    return null;
  }

  private Button FindButton(string childName)
  {
    Transform child = FindChild(childName);
    if (child == null)
    {
      return null;
    }

    return child.GetComponent<Button>() ?? child.GetComponentInChildren<Button>(true);
  }

  private GameObject FindChildGameObject(string childName)
  {
    Transform child = FindChild(childName);
    return child != null ? child.gameObject : null;
  }

  private Transform FindChild(string childName)
  {
    if (string.IsNullOrEmpty(childName))
    {
      return null;
    }

    Transform[] children = GetComponentsInChildren<Transform>(true);
    for (int i = 0; i < children.Length; i++)
    {
      if (children[i] != null && children[i].name == childName)
      {
        return children[i];
      }
    }

    return null;
  }

  private void PositionTargetVisuals(TutorialStep step)
  {
    string targetId = step != null ? step.targetId : string.Empty;
    TutorialTarget target = TutorialTarget.Find(targetId);
    Vector3 targetPosition = Vector3.zero;
    bool hasTarget = target != null && target.TryGetScreenPosition(out targetPosition);

    SetActive(highlight != null ? highlight.gameObject : null, hasTarget && step != null && step.showHighlight);
    SetActive(pointer != null ? pointer.gameObject : null, hasTarget && step != null && step.showPointer);

    if (!hasTarget)
    {
      return;
    }

    if (highlight != null && step.showHighlight)
    {
      highlight.position = targetPosition;
      Vector2 highlightScaleValue = GetHighlightScale(step);
      Vector2 highlightSize = Vector2.Scale(target.GetHighlightSize(), highlightScaleValue);
      highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, highlightSize.x);
      highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, highlightSize.y);
    }

    if (pointer != null && step.showPointer)
    {
      pointer.position = targetPosition + (Vector3)step.pointerOffset;
      pointer.localScale = Vector3.one * Mathf.Max(0.01f, step.pointerScale);
      pointer.localRotation = Quaternion.Euler(0f, 0f, GetPointerRotation(step.pointerDirection));
    }
  }

  private static float GetPointerRotation(TutorialPointerDirection direction)
  {
    switch (direction)
    {
      case TutorialPointerDirection.Left:
        return 180f;
      case TutorialPointerDirection.Up:
        return 90f;
      case TutorialPointerDirection.Down:
        return -90f;
      default:
        return 0f;
    }
  }

  private static Vector2 GetHighlightScale(TutorialStep step)
  {
    if (step == null)
    {
      return Vector2.one;
    }

    Vector2 scale = step.highlightScaleXY;
    if (scale == Vector2.zero)
    {
      scale = Vector2.one * step.highlightScale;
    }

    return new Vector2(Mathf.Max(0.01f, scale.x), Mathf.Max(0.01f, scale.y));
  }

  private void OnNextClicked()
  {
    RevealNarratorTextImmediately();
    StopRevealNextButtonDelay();

    if (director != null)
    {
      director.NextStep();
    }
  }

  private void OnBackClicked()
  {
    if (director != null)
    {
      director.PreviousStep();
    }
  }

  private void OnSkipClicked()
  {
    if (director != null)
    {
      director.SkipTutorial();
    }
  }

  private void OnMuteClicked()
  {
    narratorMuted = !narratorMuted;
    PlayerPrefs.SetInt(NarratorMutedKey, narratorMuted ? 1 : 0);
    PlayerPrefs.Save();

    if (narratorMuted)
    {
      StopNarratorAudio();
      UpdateMuteVisualState();
      return;
    }

    PlayNarratorAudio(currentNarratorAudio);
    UpdateMuteVisualState();
  }

  private void OnReplayClicked()
  {
    PlayNarratorAudio(currentNarratorAudio);
  }

  private void OnTutorialEvent(TutorialEventPayload payload)
  {
    if (!hidePanelWhileCampaignTravels || payload == null)
    {
      return;
    }

    if (payload.eventId == TutorialEventNames.CampaignNodeSelected)
    {
      StopCurrentStepNarration();
      hiddenForCampaignTravel = true;
      SetTutorialVisualsVisible(false);
      if (ShouldSuspendInputGateForCampaignTravel())
      {
        RestoreGatedGraphics();
      }
      return;
    }

    if (payload.eventId == "ui.descanso_confirmado" &&
        (hideUntilRestRandomEvent || CurrentStepWaitsForEvent(TutorialEventNames.CampaignRestRandomEventContinued)))
    {
      StopCurrentStepNarration();
      hideUntilRestRandomEvent = true;
      hiddenForCampaignTravel = true;
      SetTutorialVisualsVisible(false);
      RestoreGatedGraphics();
      return;
    }

    if (payload.eventId == TutorialEventNames.CampaignRestRandomEventContinued)
    {
      hideUntilRestRandomEvent = false;
      hiddenForCampaignTravel = false;
      return;
    }

    if (payload.eventId == TutorialEventNames.CampaignNodeArrived && hiddenForCampaignTravel)
    {
      if (currentStep == null)
      {
        hiddenForCampaignTravel = false;
        return;
      }

      if (ShouldKeepTutorialHiddenUntilCampaignEvent())
      {
        RestoreGatedGraphics();
        return;
      }

      hiddenForCampaignTravel = false;
      SetTutorialVisualsVisible(true);
      ArrangeTutorialLayers();
    }
  }

  private bool CurrentStepWaitsForEvent(string eventId)
  {
    if (currentStep == null || currentStep.advanceConditions == null || string.IsNullOrEmpty(eventId))
    {
      return false;
    }

    for (int i = 0; i < currentStep.advanceConditions.Count; i++)
    {
      TutorialCondition condition = currentStep.advanceConditions[i];
      if (condition != null && condition.eventId == eventId)
      {
        return true;
      }
    }

    return false;
  }

  private bool ShouldSuspendInputGateForCampaignTravel()
  {
    return hiddenForCampaignTravel && ShouldKeepTutorialHiddenUntilCampaignEvent();
  }

  private bool ShouldKeepTutorialHiddenUntilCampaignEvent()
  {
    return CurrentStepWaitsForEvent(TutorialEventNames.CampaignResourceNodeContinued)
      || CurrentStepWaitsForEvent(TutorialEventNames.CampaignMissingPeopleEventContinued)
      || CurrentStepWaitsForEvent(TutorialEventNames.CampaignRestNodeContinued)
      || CurrentStepWaitsForEvent(TutorialEventNames.CampaignRestRandomEventContinued);
  }

  private void SetTutorialVisualsVisible(bool visible)
  {
    SetActive(panelRoot, visible && activePanelRoot == panelRoot && ShouldShowPanel(currentStep));
    SetActive(combatPanelRoot, visible && activePanelRoot == combatPanelRoot && ShouldShowPanel(currentStep));
    SetActive(highlight != null ? highlight.gameObject : null, visible && currentStep != null && currentStep.showHighlight);
    SetActive(pointer != null ? pointer.gameObject : null, visible && currentStep != null && currentStep.showPointer);
    SetActive(inputBlocker, visible && currentStep != null && currentStep.inputBlockMode == TutorialInputBlockMode.All);
  }

  private void ApplyGraphicInputGate(TutorialStep step)
  {
    if (step == null || step.inputBlockMode == TutorialInputBlockMode.None)
    {
      return;
    }

    Graphic[] graphics = UnityEngine.Object.FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    TutorialTarget target = step.inputBlockMode == TutorialInputBlockMode.AllExceptTarget
      ? TutorialTarget.Find(step.targetId)
      : null;

    for (int i = 0; i < graphics.Length; i++)
    {
      Graphic graphic = graphics[i];
      if (graphic == null)
      {
        continue;
      }

      if (IsTutorialOwnedGraphic(graphic))
      {
        continue;
      }

      if (!gatedGraphicsOriginalRaycastTarget.ContainsKey(graphic))
      {
        gatedGraphicsOriginalRaycastTarget.Add(graphic, graphic.raycastTarget);
      }

      graphic.raycastTarget = gatedGraphicsOriginalRaycastTarget[graphic] && IsGraphicAllowedForStep(graphic, step, target);
    }
  }

  private bool IsGraphicAllowedForStep(Graphic graphic, TutorialStep step, TutorialTarget target)
  {
    if (graphic == null || step == null)
    {
      return false;
    }

    if (step.inputBlockMode == TutorialInputBlockMode.None)
    {
      return true;
    }

    if (step.inputBlockMode == TutorialInputBlockMode.All)
    {
      return false;
    }

    if (target == null)
    {
      return false;
    }

    Transform graphicTransform = graphic.transform;
    Transform targetTransform = target.transform;
    return graphicTransform == targetTransform
      || graphicTransform.IsChildOf(targetTransform)
      || targetTransform.IsChildOf(graphicTransform);
  }

  private bool IsTutorialOwnedGraphic(Graphic graphic)
  {
    if (graphic == null)
    {
      return false;
    }

    Transform graphicTransform = graphic.transform;
    return graphicTransform.IsChildOf(transform)
      || IsChildOf(graphicTransform, panelRoot)
      || IsChildOf(graphicTransform, combatPanelRoot)
      || IsChildOf(graphicTransform, inputBlocker)
      || (highlight != null && graphicTransform.IsChildOf(highlight))
      || (pointer != null && graphicTransform.IsChildOf(pointer));
  }

  private static bool IsChildOf(Transform child, GameObject parent)
  {
    return child != null && parent != null && child.IsChildOf(parent.transform);
  }

  private void RestoreGatedGraphics()
  {
    foreach (KeyValuePair<Graphic, bool> entry in gatedGraphicsOriginalRaycastTarget)
    {
      if (entry.Key != null)
      {
        entry.Key.raycastTarget = entry.Value;
      }
    }

    gatedGraphicsOriginalRaycastTarget.Clear();
  }

  private void ConfigureInputBlockerRaycastFilter()
  {
    if (inputBlocker == null)
    {
      inputBlockerRaycastFilter = null;
      return;
    }

    inputBlockerRaycastFilter = inputBlocker.GetComponent<TutorialInputBlockerRaycastFilter>();
    if (inputBlockerRaycastFilter == null)
    {
      inputBlockerRaycastFilter = inputBlocker.AddComponent<TutorialInputBlockerRaycastFilter>();
    }

    inputBlockerRaycastFilter.SetPassthroughTargets(
      GetRectTransform(nextButtonRoot),
      nextButton != null ? nextButton.GetComponent<RectTransform>() : null,
      backButton != null ? backButton.GetComponent<RectTransform>() : null,
      skipButton != null ? skipButton.GetComponent<RectTransform>() : null,
      muteButton != null ? muteButton.GetComponent<RectTransform>() : null,
      replayButton != null ? replayButton.GetComponent<RectTransform>() : null);
  }

  private static RectTransform GetRectTransform(GameObject target)
  {
    return target != null ? target.GetComponent<RectTransform>() : null;
  }

  private static bool ShouldShowPanel(TutorialStep step)
  {
    return step == null || step.presentationMode != TutorialPresentationMode.HintOnly;
  }

  private void SelectActivePanel(TutorialStep step)
  {
    GameObject selectedPanel = step != null
      && step.presentationMode == TutorialPresentationMode.Compact
      && combatPanelRoot != null
        ? combatPanelRoot
        : panelRoot;

    RectTransform selectedRect = selectedPanel == combatPanelRoot ? combatPanelRootRect : panelRootRect;
    if (activePanelRoot == selectedPanel)
    {
      activePanelRootRect = selectedRect;
      RefreshActivePanelTextReferences();
      return;
    }

    activePanelRoot = selectedPanel;
    activePanelRootRect = selectedRect;
    RefreshActivePanelTextReferences();
    panelLayoutCached = false;
  }

  private void RefreshActivePanelTextReferences()
  {
    if (activePanelRoot == combatPanelRoot)
    {
      activeTitleText = combatTitleText != null ? combatTitleText : titleText;
      activeNarratorText = combatNarratorText != null ? combatNarratorText : narratorText;
      activeBodyText = combatBodyText != null ? combatBodyText : bodyText;
      return;
    }

    activeTitleText = titleText;
    activeNarratorText = narratorText;
    activeBodyText = bodyText;
  }

  private void ApplyPanelPresentation(TutorialStep step)
  {
    CachePanelLayout();
    if (activePanelRootRect == null || step == null)
    {
      return;
    }

    if (step.presentationMode == TutorialPresentationMode.Full)
    {
      RestorePanelLayout();
      ApplyFullPanelAnchor(step);
      return;
    }

    ApplyAnchoredPanelLayout(step);
  }

  private void CachePanelLayout()
  {
    if (panelLayoutCached || activePanelRootRect == null)
    {
      return;
    }

    panelOriginalAnchorMin = activePanelRootRect.anchorMin;
    panelOriginalAnchorMax = activePanelRootRect.anchorMax;
    panelOriginalPivot = activePanelRootRect.pivot;
    panelOriginalAnchoredPosition = activePanelRootRect.anchoredPosition;
    panelOriginalSizeDelta = activePanelRootRect.sizeDelta;
    panelOriginalScale = activePanelRootRect.localScale;
    panelLayoutCached = true;
  }

  private void RestorePanelLayout()
  {
    if (!panelLayoutCached || activePanelRootRect == null)
    {
      return;
    }

    activePanelRootRect.anchorMin = panelOriginalAnchorMin;
    activePanelRootRect.anchorMax = panelOriginalAnchorMax;
    activePanelRootRect.pivot = panelOriginalPivot;
    activePanelRootRect.anchoredPosition = panelOriginalAnchoredPosition;
    activePanelRootRect.sizeDelta = panelOriginalSizeDelta;
    activePanelRootRect.localScale = panelOriginalScale;
  }

  private void ApplyAnchoredPanelLayout(TutorialStep step)
  {
    Vector2 anchor = GetPanelAnchor(step.panelAnchor);
    activePanelRootRect.anchorMin = anchor;
    activePanelRootRect.anchorMax = anchor;
    activePanelRootRect.pivot = anchor;
    activePanelRootRect.localScale = Vector3.one;

    Vector2 size = step.compactPanelSize;
    if (size.x > 0f && size.y > 0f)
    {
      activePanelRootRect.sizeDelta = size;
    }

    activePanelRootRect.anchoredPosition = GetPanelOffset(step.panelAnchor, step.panelOffset);
  }

  private void ApplyFullPanelAnchor(TutorialStep step)
  {
    if (step.panelAnchor == TutorialPanelAnchor.Center)
    {
      activePanelRootRect.anchoredPosition = panelOriginalAnchoredPosition + step.panelOffset;
      return;
    }

    if (step.panelAnchor == TutorialPanelAnchor.CenterRight)
    {
      AlignFullPanelToHorizontalEdge(true, step.panelOffset);
      return;
    }

    if (step.panelAnchor == TutorialPanelAnchor.CenterLeft)
    {
      AlignFullPanelToHorizontalEdge(false, step.panelOffset);
      return;
    }

    activePanelRootRect.anchoredPosition = panelOriginalAnchoredPosition + step.panelOffset;
  }

  private void AlignFullPanelToHorizontalEdge(bool rightEdge, Vector2 offset)
  {
    RectTransform parentRect = activePanelRootRect.parent as RectTransform;
    if (parentRect == null)
    {
      activePanelRootRect.anchoredPosition = panelOriginalAnchoredPosition + offset;
      return;
    }

    Canvas.ForceUpdateCanvases();

    Vector3[] parentCorners = new Vector3[4];
    Vector3[] panelCorners = new Vector3[4];
    parentRect.GetWorldCorners(parentCorners);
    activePanelRootRect.GetWorldCorners(panelCorners);

    float margin = Mathf.Max(0f, panelViewportMargin);
    float parentEdge = parentRect.InverseTransformPoint(parentCorners[rightEdge ? 2 : 0]).x;
    float panelEdge = parentRect.InverseTransformPoint(panelCorners[rightEdge ? 2 : 0]).x;
    float targetEdge = parentEdge + (rightEdge ? -margin : margin) + offset.x;
    float deltaX = targetEdge - panelEdge;

    activePanelRootRect.anchoredPosition = panelOriginalAnchoredPosition + new Vector2(deltaX, offset.y);
  }

  private static Vector2 GetPanelAnchor(TutorialPanelAnchor anchor)
  {
    switch (anchor)
    {
      case TutorialPanelAnchor.TopLeft:
        return new Vector2(0f, 1f);
      case TutorialPanelAnchor.TopRight:
        return new Vector2(1f, 1f);
      case TutorialPanelAnchor.BottomLeft:
        return new Vector2(0f, 0f);
      case TutorialPanelAnchor.BottomRight:
        return new Vector2(1f, 0f);
      case TutorialPanelAnchor.CenterLeft:
        return new Vector2(0f, 0.5f);
      case TutorialPanelAnchor.CenterRight:
        return new Vector2(1f, 0.5f);
      default:
        return new Vector2(0.5f, 0.5f);
    }
  }

  private static Vector2 GetPanelOffset(TutorialPanelAnchor anchor, Vector2 offset)
  {
    const float defaultMargin = 28f;
    switch (anchor)
    {
      case TutorialPanelAnchor.TopLeft:
        return new Vector2(defaultMargin + offset.x, -defaultMargin + offset.y);
      case TutorialPanelAnchor.TopRight:
        return new Vector2(-defaultMargin + offset.x, -defaultMargin + offset.y);
      case TutorialPanelAnchor.BottomLeft:
        return new Vector2(defaultMargin + offset.x, defaultMargin + offset.y);
      case TutorialPanelAnchor.BottomRight:
        return new Vector2(-defaultMargin + offset.x, defaultMargin + offset.y);
      case TutorialPanelAnchor.CenterLeft:
        return new Vector2(defaultMargin + offset.x, offset.y);
      case TutorialPanelAnchor.CenterRight:
        return new Vector2(-defaultMargin + offset.x, offset.y);
      default:
        return offset;
    }
  }

  private void FitActivePanelInsideParent()
  {
    if (!fitPanelInsideParent || activePanelRootRect == null)
    {
      return;
    }

    RectTransform parentRect = activePanelRootRect.parent as RectTransform;
    if (parentRect == null)
    {
      return;
    }

    Vector2 parentSize = parentRect.rect.size;
    Vector2 panelSize = activePanelRootRect.rect.size;
    if (parentSize.x <= 0f || parentSize.y <= 0f || panelSize.x <= 0f || panelSize.y <= 0f)
    {
      return;
    }

    float margin = Mathf.Max(0f, panelViewportMargin);
    float availableWidth = Mathf.Max(1f, parentSize.x - margin * 2f);
    float availableHeight = Mathf.Max(1f, parentSize.y - margin * 2f);
    Vector3 currentScale = activePanelRootRect.localScale;
    float scaledWidth = panelSize.x * Mathf.Abs(currentScale.x);
    float scaledHeight = panelSize.y * Mathf.Abs(currentScale.y);
    float fitScale = Mathf.Min(1f, availableWidth / scaledWidth, availableHeight / scaledHeight);

    if (fitScale < 1f)
    {
      currentScale = new Vector3(currentScale.x * fitScale, currentScale.y * fitScale, currentScale.z);
      activePanelRootRect.localScale = currentScale;
      scaledWidth = panelSize.x * Mathf.Abs(currentScale.x);
      scaledHeight = panelSize.y * Mathf.Abs(currentScale.y);
    }

    if (activePanelRootRect.anchorMin != activePanelRootRect.anchorMax)
    {
      return;
    }

    Vector2 anchor = activePanelRootRect.anchorMin;
    Vector2 pivot = activePanelRootRect.pivot;
    Vector2 pos = activePanelRootRect.anchoredPosition;

    float minX = -anchor.x * parentSize.x + margin + pivot.x * scaledWidth;
    float maxX = (1f - anchor.x) * parentSize.x - margin - (1f - pivot.x) * scaledWidth;
    float minY = -anchor.y * parentSize.y + margin + pivot.y * scaledHeight;
    float maxY = (1f - anchor.y) * parentSize.y - margin - (1f - pivot.y) * scaledHeight;

    pos.x = minX <= maxX ? Mathf.Clamp(pos.x, minX, maxX) : (minX + maxX) * 0.5f;
    pos.y = minY <= maxY ? Mathf.Clamp(pos.y, minY, maxY) : (minY + maxY) * 0.5f;
    activePanelRootRect.anchoredPosition = pos;
  }

  private void ConfigurePanelContentVisibility(TutorialStep step)
  {
    bool full = step == null || step.presentationMode == TutorialPresentationMode.Full;
    bool compact = step != null && step.presentationMode == TutorialPresentationMode.Compact;

    SetActive(activeTitleText != null ? activeTitleText.gameObject : null, full || compact);
    SetActive(activeBodyText != null ? activeBodyText.gameObject : null, full || compact);
    SetActive(activeNarratorText != null ? activeNarratorText.gameObject : null, full);
    SetActive(muteButton != null ? muteButton.gameObject : null, full);
    SetActive(replayButton != null ? replayButton.gameObject : null, full);
  }

  private void ShowNarratorText(string text, AudioClip audioClip)
  {
    StopNarratorTextAnimation();

    if (activeNarratorText == null)
    {
      return;
    }

    activeNarratorText.text = text;

    if (!animateNarratorText || string.IsNullOrEmpty(text))
    {
      activeNarratorText.maxVisibleCharacters = int.MaxValue;
      return;
    }

    activeNarratorText.ForceMeshUpdate();
    narratorTextTotalCharacters = activeNarratorText.textInfo.characterCount;
    if (narratorTextTotalCharacters <= 0)
    {
      activeNarratorText.maxVisibleCharacters = int.MaxValue;
      return;
    }

    float duration = audioClip != null && audioClip.length > 0.05f
      ? audioClip.length * NarratorTextAudioDurationFactor
      : narratorTextTotalCharacters / Mathf.Max(1f, narratorCharactersPerSecond);

    narratorTextCoroutine = StartCoroutine(AnimateNarratorText(narratorTextTotalCharacters, duration));
  }

  private System.Collections.IEnumerator AnimateNarratorText(int totalCharacters, float duration)
  {
    activeNarratorText.maxVisibleCharacters = 0;
    float elapsed = 0f;
    float safeDuration = Mathf.Max(0.01f, duration);

    while (elapsed < safeDuration)
    {
      elapsed += Time.unscaledDeltaTime;
      float progress = Mathf.Clamp01(elapsed / safeDuration);
      activeNarratorText.maxVisibleCharacters = Mathf.Clamp(Mathf.CeilToInt(totalCharacters * progress), 0, totalCharacters);
      yield return null;
    }

    activeNarratorText.maxVisibleCharacters = int.MaxValue;
    narratorTextCoroutine = null;
  }

  private void StopNarratorTextAnimation()
  {
    if (narratorTextCoroutine != null)
    {
      StopCoroutine(narratorTextCoroutine);
      narratorTextCoroutine = null;
    }

    if (activeNarratorText != null)
    {
      activeNarratorText.maxVisibleCharacters = int.MaxValue;
    }
  }

  private void RevealNarratorTextImmediately()
  {
    StopNarratorTextAnimation();
    if (activeNarratorText != null)
    {
      activeNarratorText.maxVisibleCharacters = narratorTextTotalCharacters > 0 ? narratorTextTotalCharacters : int.MaxValue;
    }
  }

  private void ConfigureNextButtonVisibility(TutorialStep step, AudioClip audioClip)
  {
    StopRevealNextButtonDelay();

    if (nextButtonRoot == null)
    {
      return;
    }

    if (step == null || !step.showNextButton)
    {
      SetActive(nextButtonRoot, false);
      return;
    }

    if (step.presentationMode == TutorialPresentationMode.HintOnly)
    {
      SetActive(nextButtonRoot, false);
      return;
    }

    float delay = GetNextButtonRevealDelay(step, audioClip);
    if (!showNextAfterNarration || delay <= 0f)
    {
      SetActive(nextButtonRoot, true);
      return;
    }

    SetActive(nextButtonRoot, false);
    revealNextButtonCoroutine = StartCoroutine(RevealNextButtonAfterDelay(delay));
  }

  private float GetNextButtonRevealDelay(TutorialStep step, AudioClip audioClip)
  {
    if (step == null)
    {
      return 0f;
    }

    float delay = 0f;
    if (!narratorMuted && audioClip != null)
    {
      delay = Mathf.Max(delay, audioClip.length);
    }

    if (animateNarratorText && activeNarratorText != null)
    {
      activeNarratorText.ForceMeshUpdate();
      int totalCharacters = activeNarratorText.textInfo.characterCount;
      if (totalCharacters > 0)
      {
        float typewriterDuration = audioClip != null && audioClip.length > 0.05f
          ? audioClip.length * NarratorTextAudioDurationFactor
          : totalCharacters / Mathf.Max(1f, narratorCharactersPerSecond);
        delay = Mathf.Max(delay, typewriterDuration);
      }
    }

    return delay;
  }

  private AudioClip GetNarratorAudioForCurrentLanguage(TutorialStep step)
  {
    if (step == null)
    {
      return null;
    }

    int idioma = TRADU.i != null ? TRADU.i.nIdioma : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);
    switch (idioma)
    {
      case TRADU.IdiomaIngles:
        return step.narratorAudioIngles;
      case TRADU.IdiomaPortugues:
        return step.narratorAudioPortugues;
      default:
        return step.narratorAudio;
    }
  }

  private System.Collections.IEnumerator RevealNextButtonAfterDelay(float delay)
  {
    yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay));
    revealNextButtonCoroutine = null;
    if (currentStep != null && currentStep.showNextButton)
    {
      SetActive(nextButtonRoot, true);
    }
  }

  private void StopRevealNextButtonDelay()
  {
    if (revealNextButtonCoroutine == null)
    {
      return;
    }

    StopCoroutine(revealNextButtonCoroutine);
    revealNextButtonCoroutine = null;
  }

  private void RevealNextButtonNow()
  {
    StopRevealNextButtonDelay();
    if (currentStep != null && currentStep.showNextButton)
    {
      SetActive(nextButtonRoot, true);
    }
  }

  private void PlayNarratorAudio(AudioClip audioClip)
  {
    StopNarratorMusicDucking();
    currentNarratorAudio = audioClip;
    if (narratorAudioSource == null)
    {
      return;
    }

    narratorAudioSource.Stop();
    narratorAudioSource.clip = null;

    if (narratorMuted || audioClip == null)
    {
      return;
    }

    narratorAudioSource.clip = audioClip;
    narratorAudioSource.volume = 1f;
    narratorAudioSource.PlayOneShot(audioClip, Mathf.Max(0f, narratorVolumeMultiplier));
    StartNarratorMusicDucking(audioClip);
  }

  private void StartNarratorMusicDucking(AudioClip audioClip)
  {
    if (MusicManager.Instance == null || audioClip == null)
    {
      return;
    }

    MusicManager.Instance.SetDuckingNarradorTutorial(true, NarratorMusicDuckingMaxVolume);
    narratorMusicDuckingCoroutine = StartCoroutine(RestoreNarratorMusicWhenAudioEnds(audioClip));
  }

  private System.Collections.IEnumerator RestoreNarratorMusicWhenAudioEnds(AudioClip audioClip)
  {
    float elapsed = 0f;
    float duration = audioClip != null ? audioClip.length : 0f;
    while (elapsed < duration
      && narratorAudioSource != null
      && narratorAudioSource.clip == audioClip)
    {
      elapsed += Time.unscaledDeltaTime;
      yield return null;
    }

    narratorMusicDuckingCoroutine = null;
    RestoreNarratorMusicDucking();
  }

  private void StopNarratorMusicDucking()
  {
    if (narratorMusicDuckingCoroutine != null)
    {
      StopCoroutine(narratorMusicDuckingCoroutine);
      narratorMusicDuckingCoroutine = null;
    }

    RestoreNarratorMusicDucking();
  }

  private void RestoreNarratorMusicDucking()
  {
    if (MusicManager.Instance != null)
    {
      MusicManager.Instance.SetDuckingNarradorTutorial(false, NarratorMusicDuckingMaxVolume);
    }
  }

  private void UpdateMuteVisualState()
  {
    if (muteStateGraphic != null)
    {
      muteStateGraphic.color = narratorMuted ? muteColor : unmuteColor;
    }

    bool showAudioControls = currentStep == null || currentStep.presentationMode == TutorialPresentationMode.Full;
    SetActive(mutedIndicator, showAudioControls && narratorMuted);
    SetActive(unmutedIndicator, showAudioControls && !narratorMuted);
  }

  private void StopNarratorAudio()
  {
    StopNarratorMusicDucking();

    if (narratorAudioSource == null)
    {
      return;
    }

    narratorAudioSource.Stop();
    narratorAudioSource.clip = null;
  }

  private static void SetActive(GameObject target, bool active)
  {
    if (target != null && target.activeSelf != active)
    {
      target.SetActive(active);
    }
  }

  private static bool UsesSameButton(Button a, Button b)
  {
    return a != null && b != null && a == b;
  }
}

public class TutorialInputBlockerRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
  private RectTransform[] passthroughTargets;

  public void SetPassthroughTargets(params RectTransform[] targets)
  {
    passthroughTargets = targets;
  }

  public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
  {
    if (passthroughTargets == null)
    {
      return true;
    }

    for (int i = 0; i < passthroughTargets.Length; i++)
    {
      RectTransform target = passthroughTargets[i];
      if (target == null || !target.gameObject.activeInHierarchy)
      {
        continue;
      }

      if (RectTransformUtility.RectangleContainsScreenPoint(target, screenPoint, eventCamera))
      {
        return false;
      }
    }

    return true;
  }
}
