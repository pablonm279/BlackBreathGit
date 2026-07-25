using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialTooltipManager : MonoBehaviour
{
  private const string DefaultCatalogPath = "Tutoriales/TooltipsTutorial";

  public static TutorialTooltipManager Instance { get; private set; }
  public bool EstaMostrandoTooltip { get { return mostrando; } }

  [Header("Contenido")]
  [SerializeField] private TutorialTooltipCatalog catalog;
  [SerializeField] private List<TutorialTooltipDefinition> tooltips = new List<TutorialTooltipDefinition>();

  [Header("UI")]
  [SerializeField] private GameObject bloqueador;
  [SerializeField] private GameObject tooltipGrande;
  [SerializeField] private GameObject tooltipChico;
  [SerializeField] private RectTransform pointer;

  [Header("Animacion")]
  [SerializeField] private bool animarTexto = true;
  [SerializeField, Min(1f)] private float caracteresPorSegundo = 42f;
  [SerializeField] private bool continuarCompletaTextoPrimero = true;

  [Header("Layout")]
  [SerializeField, Min(0f)] private float margenPantalla = 28f;

  [Header("Persistencia")]
  [SerializeField] private bool autosaveAlCambiarProgreso = true;

  [Header("Debug")]
  [SerializeField] private bool debugIgnorarVistos;
  [SerializeField] private bool debugIgnorarSilencio;
  [SerializeField] private string debugTooltipId;

  private readonly Queue<TutorialTooltipRequest> colaTooltips = new Queue<TutorialTooltipRequest>();
  private readonly HashSet<string> idsEnCola = new HashSet<string>();
  private readonly List<Button> botonesContinuar = new List<Button>();
  private readonly List<Button> botonesSilenciar = new List<Button>();
  private readonly List<Toggle> togglesSilenciar = new List<Toggle>();

  private TutorialTooltipDefinition tooltipActual;
  private GameObject panelActivo;
  private RectTransform panelActivoRect;
  private TextMeshProUGUI textoActivo;
  private Coroutine textoCoroutine;
  private int totalCaracteresTexto;
  private bool mostrando;
  private bool pausaAplicada;
  private bool pausaCombateAplicada;
  private float escalaTiempoPrevia = 1f;

  private struct TutorialTooltipRequest
  {
    public TutorialTooltipDefinition definition;
    public bool force;
  }

  private void Reset()
  {
    AutoBindReferences();
  }

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(this);
      return;
    }

    Instance = this;
    AutoBindReferences();
    ConfigurarBotones();
    OcultarVisuales();
  }

  private void OnDestroy()
  {
    if (Instance == this)
    {
      Instance = null;
    }

    RestaurarPausaJuego();
  }

  public static bool TryShow(string tooltipId)
  {
    TutorialTooltipManager manager = GetOrFindInstance();
    return manager != null && manager.EnqueueTooltip(tooltipId, false);
  }

  public static bool ForceShow(string tooltipId)
  {
    TutorialTooltipManager manager = GetOrFindInstance();
    return manager != null && manager.EnqueueTooltip(tooltipId, true);
  }

  public bool EnqueueTooltip(string tooltipId, bool force = false)
  {
    TutorialTooltipDefinition definition = FindDefinition(tooltipId);
    if (definition == null)
    {
      Debug.LogWarning("[TutorialTooltip] No se encontro tooltip con id '" + tooltipId + "'.", this);
      return false;
    }

    if (!force && !DebeMostrar(definition))
    {
      return false;
    }

    if (mostrando && tooltipActual != null && tooltipActual.id == definition.id)
    {
      return false;
    }

    if (idsEnCola.Contains(definition.id))
    {
      return false;
    }

    colaTooltips.Enqueue(new TutorialTooltipRequest
    {
      definition = definition,
      force = force
    });
    idsEnCola.Add(definition.id);

    if (!mostrando)
    {
      MostrarSiguienteEnCola();
    }

    return true;
  }

  public void BotonContinuar()
  {
    if (tooltipActual == null)
    {
      CerrarActual(false);
      return;
    }

    if (continuarCompletaTextoPrimero && textoCoroutine != null)
    {
      RevelarTextoInmediato();
      return;
    }

    bool silenciarAlContinuar = DebeSilenciarAlContinuar();
    TutorialTooltipProgress.MarcarVisto(tooltipActual.id);
    if (silenciarAlContinuar)
    {
      TutorialTooltipProgress.Silenciar();
      colaTooltips.Clear();
      idsEnCola.Clear();
      UpdateSilenciarToggleVisualState();
    }

    PersistirProgresoSiCorresponde();
    CerrarActual(!silenciarAlContinuar);
  }

  public void BotonSilenciar()
  {
    TutorialTooltipProgress.Silenciar();
    colaTooltips.Clear();
    idsEnCola.Clear();
    UpdateSilenciarToggleVisualState();
    PersistirProgresoSiCorresponde();
    CerrarActual(false);
  }

  [ContextMenu("Debug Mostrar Tooltip Configurado")]
  public void DebugMostrarTooltipConfigurado()
  {
    if (!string.IsNullOrWhiteSpace(debugTooltipId))
    {
      EnqueueTooltip(debugTooltipId, true);
    }
  }

  [ContextMenu("Debug Mostrar Ultimo Tooltip")]
  public void DebugMostrarUltimoTooltip()
  {
    TutorialTooltipDefinition definition = GetLastDebugDefinition();
    if (definition != null && !HayTutorialActivo())
    {
      EnqueueTooltip(definition.id, true);
    }
  }

  [ContextMenu("Debug Resetear Tooltips Tutorial")]
  public void DebugResetearTooltipsTutorial()
  {
    TutorialTooltipProgress.ResetearParaNuevaCampania();
    UpdateSilenciarToggleVisualState();
    PersistirProgresoSiCorresponde();
  }

  public void BotonDebugMostrarTooltip()
  {
    DebugMostrarTooltipConfigurado();
  }

  public void BotonDebugMostrarUltimoTooltip()
  {
    DebugMostrarUltimoTooltip();
  }

  public void BotonDebugResetearTooltips()
  {
    DebugResetearTooltipsTutorial();
  }

  private void MostrarSiguienteEnCola()
  {
    while (colaTooltips.Count > 0)
    {
      TutorialTooltipRequest request = colaTooltips.Dequeue();
      TutorialTooltipDefinition definition = request.definition;
      idsEnCola.Remove(definition.id);

      if (definition == null)
      {
        continue;
      }

      if (!request.force && !DebeMostrar(definition))
      {
        continue;
      }

      Mostrar(definition);
      return;
    }

    mostrando = false;
  }

  private void Mostrar(TutorialTooltipDefinition definition)
  {
    tooltipActual = definition;
    mostrando = true;
    StopTextoAnimation();
    SelectPanel(definition.size);
    SetActive(bloqueador, true);
    SetActive(tooltipChico, panelActivo == tooltipChico);
    SetActive(tooltipGrande, panelActivo == tooltipGrande);
    SetActive(pointer != null ? pointer.gameObject : null, definition.showPointer);
    ArrangeLayers();
    ApplyPanelLayout(definition);
    ApplyPointerLayout(definition);
    UpdateSilenciarToggleVisualState();
    ShowText(definition.GetText());
  }

  private void CerrarActual(bool intentarMostrarSiguiente)
  {
    StopTextoAnimation();
    tooltipActual = null;
    mostrando = false;
    OcultarVisuales();
    RestaurarPausaJuego();

    if (intentarMostrarSiguiente)
    {
      MostrarSiguienteEnCola();
    }
  }

  private bool DebeMostrar(TutorialTooltipDefinition definition)
  {
    if (definition == null || string.IsNullOrWhiteSpace(definition.id))
    {
      return false;
    }

    if (HayTutorialActivo())
    {
      return false;
    }

    if (TutorialTooltipProgress.Silenciados && !debugIgnorarSilencio)
    {
      return false;
    }

    return debugIgnorarVistos || !TutorialTooltipProgress.FueVisto(definition.id);
  }

  private static bool HayTutorialActivo()
  {
    if (TutorialDirector.HayTutorialActivoOPendiente())
    {
      return true;
    }

    CampaignManager campaignManager = CampaignManager.Instance;
    return campaignManager != null
      && campaignManager.scTutorialManager != null
      && campaignManager.scTutorialManager.tutorialActivo;
  }

  private TutorialTooltipDefinition FindDefinition(string tooltipId)
  {
    if (string.IsNullOrWhiteSpace(tooltipId))
    {
      return null;
    }

    if (catalog == null)
    {
      catalog = Resources.Load<TutorialTooltipCatalog>(DefaultCatalogPath);
    }

    TutorialTooltipDefinition definition = catalog != null ? catalog.Find(tooltipId) : null;
    if (definition != null)
    {
      return definition;
    }

    for (int i = 0; i < tooltips.Count; i++)
    {
      TutorialTooltipDefinition tooltip = tooltips[i];
      if (tooltip != null && tooltip.id == tooltipId)
      {
        return tooltip;
      }
    }

    return null;
  }

  private TutorialTooltipDefinition GetLastDebugDefinition()
  {
    if (catalog == null)
    {
      catalog = Resources.Load<TutorialTooltipCatalog>(DefaultCatalogPath);
    }

    TutorialTooltipDefinition definition = GetLastValidDefinition(catalog != null ? catalog.tooltips : null);
    return definition ?? GetLastValidDefinition(tooltips);
  }

  private static TutorialTooltipDefinition GetLastValidDefinition(List<TutorialTooltipDefinition> definitions)
  {
    if (definitions == null)
    {
      return null;
    }

    for (int i = definitions.Count - 1; i >= 0; i--)
    {
      TutorialTooltipDefinition definition = definitions[i];
      if (definition != null && !string.IsNullOrWhiteSpace(definition.id))
      {
        return definition;
      }
    }

    return null;
  }

  private void AutoBindReferences()
  {
    if (catalog == null)
    {
      catalog = Resources.Load<TutorialTooltipCatalog>(DefaultCatalogPath);
    }

    if (bloqueador == null)
    {
      Transform found = FindChild("Bloqueador");
      bloqueador = found != null ? found.gameObject : null;
    }

    if (tooltipGrande == null)
    {
      Transform found = FindChild("TooltipGrande");
      tooltipGrande = found != null ? found.gameObject : null;
    }

    if (tooltipChico == null)
    {
      Transform found = FindChild("TooltipChico");
      tooltipChico = found != null ? found.gameObject : null;
    }

    if (pointer == null)
    {
      Transform found = FindChild("Pointer");
      pointer = found != null ? found.GetComponent<RectTransform>() : null;
    }

    RefreshButtons();
  }

  private void RefreshButtons()
  {
    botonesContinuar.Clear();
    botonesSilenciar.Clear();
    togglesSilenciar.Clear();
    FindButtonsByName("ButtonContinuar", botonesContinuar);
    FindButtonsByName("Continuar", botonesContinuar);
    FindButtonsByName("Silenciar", botonesSilenciar);
    FindButtonsByName("ButtonSilenciar", botonesSilenciar);
    FindTogglesByName("Silenciar", togglesSilenciar);
    FindTogglesByName("ToggleSilenciar", togglesSilenciar);
  }

  private void ConfigurarBotones()
  {
    for (int i = 0; i < botonesContinuar.Count; i++)
    {
      Button boton = botonesContinuar[i];
      if (boton == null)
      {
        continue;
      }

      boton.onClick.RemoveListener(BotonContinuar);
      boton.onClick.AddListener(BotonContinuar);
    }

    for (int i = 0; i < botonesSilenciar.Count; i++)
    {
      Button boton = botonesSilenciar[i];
      if (boton == null)
      {
        continue;
      }

      boton.onClick.RemoveListener(BotonSilenciar);
      boton.onClick.AddListener(BotonSilenciar);
    }

    for (int i = 0; i < togglesSilenciar.Count; i++)
    {
      Toggle toggle = togglesSilenciar[i];
      if (toggle == null)
      {
        continue;
      }

      toggle.onValueChanged.RemoveListener(OnSilenciarToggleChanged);
      toggle.SetIsOnWithoutNotify(TutorialTooltipProgress.Silenciados);
      toggle.onValueChanged.AddListener(OnSilenciarToggleChanged);
    }
  }

  private void OnSilenciarToggleChanged(bool activo)
  {
    // El toggle solo toma efecto cuando se presiona Continuar.
  }

  private bool DebeSilenciarAlContinuar()
  {
    if (TutorialTooltipProgress.Silenciados)
    {
      return true;
    }

    for (int i = 0; i < togglesSilenciar.Count; i++)
    {
      Toggle toggle = togglesSilenciar[i];
      if (toggle != null && toggle.isOn)
      {
        return true;
      }
    }

    return false;
  }

  private void UpdateSilenciarToggleVisualState()
  {
    for (int i = 0; i < togglesSilenciar.Count; i++)
    {
      Toggle toggle = togglesSilenciar[i];
      if (toggle != null)
      {
        toggle.SetIsOnWithoutNotify(TutorialTooltipProgress.Silenciados);
      }
    }
  }

  private void SelectPanel(TutorialTooltipSize size)
  {
    panelActivo = size == TutorialTooltipSize.Grande && tooltipGrande != null ? tooltipGrande : tooltipChico;
    if (panelActivo == null)
    {
      panelActivo = tooltipGrande != null ? tooltipGrande : gameObject;
    }

    panelActivoRect = panelActivo.GetComponent<RectTransform>();
    textoActivo = FindTextIn(panelActivo, "DescrTooltip") ?? FindTextIn(gameObject, "DescrTooltip");
  }

  private void ShowText(string text)
  {
    if (textoActivo == null)
    {
      return;
    }

    textoActivo.text = text;
    if (!animarTexto || string.IsNullOrEmpty(text))
    {
      totalCaracteresTexto = 0;
      textoActivo.maxVisibleCharacters = int.MaxValue;
      return;
    }

    textoActivo.ForceMeshUpdate();
    totalCaracteresTexto = textoActivo.textInfo.characterCount;
    if (totalCaracteresTexto <= 0)
    {
      textoActivo.maxVisibleCharacters = int.MaxValue;
      return;
    }

    float duration = totalCaracteresTexto / Mathf.Max(1f, caracteresPorSegundo);
    textoCoroutine = StartCoroutine(AnimateText(totalCaracteresTexto, duration));
  }

  private IEnumerator AnimateText(int totalCharacters, float duration)
  {
    textoActivo.maxVisibleCharacters = 0;
    float elapsed = 0f;
    float safeDuration = Mathf.Max(0.01f, duration);

    while (elapsed < safeDuration)
    {
      elapsed += Time.unscaledDeltaTime;
      float progress = Mathf.Clamp01(elapsed / safeDuration);
      textoActivo.maxVisibleCharacters = Mathf.Clamp(Mathf.CeilToInt(totalCharacters * progress), 0, totalCharacters);
      yield return null;
    }

    textoActivo.maxVisibleCharacters = int.MaxValue;
    textoCoroutine = null;
  }

  private void StopTextoAnimation()
  {
    if (textoCoroutine != null)
    {
      StopCoroutine(textoCoroutine);
      textoCoroutine = null;
    }

    if (textoActivo != null)
    {
      textoActivo.maxVisibleCharacters = int.MaxValue;
    }
  }

  private void RevelarTextoInmediato()
  {
    StopTextoAnimation();
    if (textoActivo != null)
    {
      textoActivo.maxVisibleCharacters = totalCaracteresTexto > 0 ? totalCaracteresTexto : int.MaxValue;
    }
  }

  private void ApplyPanelLayout(TutorialTooltipDefinition definition)
  {
    if (panelActivoRect == null || definition == null)
    {
      return;
    }

    Vector2 anchor = GetAnchor(definition.side);
    panelActivoRect.anchorMin = anchor;
    panelActivoRect.anchorMax = anchor;
    panelActivoRect.pivot = anchor;
    panelActivoRect.anchoredPosition = GetDefaultOffset(definition.side) + definition.panelOffset;
  }

  private void ApplyPointerLayout(TutorialTooltipDefinition definition)
  {
    if (pointer == null || definition == null || !definition.showPointer)
    {
      return;
    }

    pointer.anchoredPosition = definition.pointerOffset;
    pointer.localScale = Vector3.one * Mathf.Max(0.01f, definition.pointerScale);
    pointer.localRotation = Quaternion.Euler(0f, 0f, GetPointerRotation(definition.pointerDirection));
  }

  private void ArrangeLayers()
  {
    if (bloqueador != null)
    {
      bloqueador.transform.SetAsFirstSibling();
    }

    if (panelActivo != null)
    {
      panelActivo.transform.SetAsLastSibling();
    }

    if (pointer != null)
    {
      pointer.SetAsLastSibling();
    }
  }

  private void OcultarVisuales()
  {
    SetActive(bloqueador, false);
    SetActive(tooltipChico, false);
    SetActive(tooltipGrande, false);
    SetActive(pointer != null ? pointer.gameObject : null, false);
  }

  private void AplicarPausaJuego()
  {
    if (pausaAplicada)
    {
      return;
    }

    pausaAplicada = true;
    escalaTiempoPrevia = Time.timeScale;
    pausaCombateAplicada = BattleManager.Instance != null;
    if (pausaCombateAplicada)
    {
      BattleManager.Instance.SetPausaTooltipTutorial(true);
      return;
    }

    Time.timeScale = 0f;
  }

  private void RestaurarPausaJuego()
  {
    if (!pausaAplicada)
    {
      return;
    }

    if (pausaCombateAplicada && BattleManager.Instance != null)
    {
      BattleManager.Instance.SetPausaTooltipTutorial(false);
    }

    Time.timeScale = escalaTiempoPrevia;
    pausaAplicada = false;
    pausaCombateAplicada = false;
  }

  private void PersistirProgresoSiCorresponde()
  {
    if (!autosaveAlCambiarProgreso || CampaignManager.Instance == null)
    {
      return;
    }

    if (!CampaignManager.Instance.PuedeGuardarCampania(out _))
    {
      return;
    }

    CampaignManager.Instance.TryAutosaveCampania("tooltip tutorial", out _);
  }

  private Transform FindChild(string childName)
  {
    if (string.IsNullOrWhiteSpace(childName))
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

  private void FindButtonsByName(string childName, List<Button> results)
  {
    if (string.IsNullOrWhiteSpace(childName) || results == null)
    {
      return;
    }

    Button[] buttons = GetComponentsInChildren<Button>(true);
    for (int i = 0; i < buttons.Length; i++)
    {
      Button button = buttons[i];
      if (button == null || button.name != childName || results.Contains(button))
      {
        continue;
      }

      results.Add(button);
    }
  }

  private void FindTogglesByName(string childName, List<Toggle> results)
  {
    if (string.IsNullOrWhiteSpace(childName) || results == null)
    {
      return;
    }

    Toggle[] toggles = GetComponentsInChildren<Toggle>(true);
    for (int i = 0; i < toggles.Length; i++)
    {
      Toggle toggle = toggles[i];
      if (toggle == null || toggle.name != childName || results.Contains(toggle))
      {
        continue;
      }

      results.Add(toggle);
    }
  }

  private static TextMeshProUGUI FindTextIn(GameObject root, string childName)
  {
    if (root == null || string.IsNullOrWhiteSpace(childName))
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

  private static Vector2 GetAnchor(TutorialTooltipSide side)
  {
    switch (side)
    {
      case TutorialTooltipSide.Arriba:
        return new Vector2(0.5f, 1f);
      case TutorialTooltipSide.Abajo:
        return new Vector2(0.5f, 0f);
      case TutorialTooltipSide.Izquierda:
        return new Vector2(0f, 0.5f);
      case TutorialTooltipSide.Derecha:
        return new Vector2(1f, 0.5f);
      case TutorialTooltipSide.ArribaIzquierda:
        return new Vector2(0f, 1f);
      case TutorialTooltipSide.ArribaDerecha:
        return new Vector2(1f, 1f);
      case TutorialTooltipSide.AbajoIzquierda:
        return new Vector2(0f, 0f);
      case TutorialTooltipSide.AbajoDerecha:
        return new Vector2(1f, 0f);
      default:
        return new Vector2(0.5f, 0.5f);
    }
  }

  private Vector2 GetDefaultOffset(TutorialTooltipSide side)
  {
    float margin = Mathf.Max(0f, margenPantalla);
    switch (side)
    {
      case TutorialTooltipSide.Arriba:
        return new Vector2(0f, -margin);
      case TutorialTooltipSide.Abajo:
        return new Vector2(0f, margin);
      case TutorialTooltipSide.Izquierda:
        return new Vector2(margin, 0f);
      case TutorialTooltipSide.Derecha:
        return new Vector2(-margin, 0f);
      case TutorialTooltipSide.ArribaIzquierda:
        return new Vector2(margin, -margin);
      case TutorialTooltipSide.ArribaDerecha:
        return new Vector2(-margin, -margin);
      case TutorialTooltipSide.AbajoIzquierda:
        return new Vector2(margin, margin);
      case TutorialTooltipSide.AbajoDerecha:
        return new Vector2(-margin, margin);
      default:
        return Vector2.zero;
    }
  }

  private static float GetPointerRotation(TutorialTooltipPointerDirection direction)
  {
    switch (direction)
    {
      case TutorialTooltipPointerDirection.Izquierda:
        return 180f;
      case TutorialTooltipPointerDirection.Arriba:
        return 90f;
      case TutorialTooltipPointerDirection.Abajo:
        return -90f;
      default:
        return 0f;
    }
  }

  private static void SetActive(GameObject target, bool active)
  {
    if (target != null && target.activeSelf != active)
    {
      target.SetActive(active);
    }
  }

  private static TutorialTooltipManager GetOrFindInstance()
  {
    if (Instance != null)
    {
      return Instance;
    }

    TutorialTooltipBootstrap.TryInstallInCurrentScene();
    return Instance;
  }
}

public static class TutorialTooltipBootstrap
{
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Initialize()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
    SceneManager.sceneLoaded += OnSceneLoaded;
    TryInstallInCurrentScene();
  }

  private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    TryInstallInCurrentScene();
  }

  public static void TryInstallInCurrentScene()
  {
    if (TutorialTooltipManager.Instance != null)
    {
      return;
    }

    TutorialTooltipManager existing = FindSceneObject<TutorialTooltipManager>();
    if (existing != null)
    {
      if (!existing.enabled)
      {
        existing.enabled = true;
      }

      return;
    }

    GameObject root = FindSceneGameObject("TooltipEmergentes");
    if (root == null)
    {
      return;
    }

    if (!root.activeSelf)
    {
      root.SetActive(true);
    }

    root.AddComponent<TutorialTooltipManager>();
  }

  private static T FindSceneObject<T>() where T : UnityEngine.Object
  {
    T[] objects = Resources.FindObjectsOfTypeAll<T>();
    for (int i = 0; i < objects.Length; i++)
    {
      T candidate = objects[i];
      if (candidate == null)
      {
        continue;
      }

      Component component = candidate as Component;
      if (component != null && component.gameObject.scene.IsValid())
      {
        return candidate;
      }
    }

    return null;
  }

  private static GameObject FindSceneGameObject(string objectName)
  {
    Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
    for (int i = 0; i < transforms.Length; i++)
    {
      Transform transform = transforms[i];
      if (transform == null || transform.name != objectName || !transform.gameObject.scene.IsValid())
      {
        continue;
      }

      return transform.gameObject;
    }

    return null;
  }
}
