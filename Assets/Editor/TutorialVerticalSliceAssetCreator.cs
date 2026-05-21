#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TutorialVerticalSliceAssetCreator
{
  private const string Folder = "Assets/Resources/Tutoriales";
  private const string DefinitionPath = Folder + "/TutorialVerticalSlice.asset";
  private const string LocalizationPath = Folder + "/TutorialVerticalSlice_Textos.asset";

  [MenuItem("GDD/Tutorial/Crear vertical slice inicial")]
  public static void CreateVerticalSliceAssets()
  {
    EnsureFolder("Assets", "Resources");
    EnsureFolder("Assets/Resources", "Tutoriales");

    TutorialDefinition definition = AssetDatabase.LoadAssetAtPath<TutorialDefinition>(DefinitionPath);
    if (definition == null)
    {
      definition = ScriptableObject.CreateInstance<TutorialDefinition>();
      AssetDatabase.CreateAsset(definition, DefinitionPath);
    }

    definition.tutorialId = "vertical_slice_intro";
    definition.restartIfCompleted = true;
    definition.steps = BuildSteps();
    EditorUtility.SetDirty(definition);

    TutorialLocalizationTable localization = AssetDatabase.LoadAssetAtPath<TutorialLocalizationTable>(LocalizationPath);
    if (localization == null)
    {
      localization = ScriptableObject.CreateInstance<TutorialLocalizationTable>();
      AssetDatabase.CreateAsset(localization, LocalizationPath);
    }

    localization.texts = BuildTexts();
    EditorUtility.SetDirty(localization);

    AssetDatabase.SaveAssets();
    AssetDatabase.Refresh();
    Selection.activeObject = definition;
    Debug.Log("Tutorial vertical slice creado en " + DefinitionPath);
  }

  private static List<TutorialStep> BuildSteps()
  {
    return new List<TutorialStep>
    {
      new TutorialStep
      {
        id = "intro",
        titleKey = "tutorial.vertical.intro.title",
        narratorKey = "tutorial.vertical.intro.narrator",
        bodyKey = "tutorial.vertical.intro.body",
        advanceMode = TutorialAdvanceMode.Manual,
        inputBlockMode = TutorialInputBlockMode.None,
        showNextButton = true,
        canGoBack = false,
        canSkip = true
      },
      new TutorialStep
      {
        id = "select_node",
        titleKey = "tutorial.vertical.select_node.title",
        narratorKey = "tutorial.vertical.select_node.narrator",
        bodyKey = "tutorial.vertical.select_node.body",
        advanceMode = TutorialAdvanceMode.Event,
        inputBlockMode = TutorialInputBlockMode.None,
        showNextButton = false,
        canGoBack = true,
        canSkip = true,
        advanceConditions = Conditions(TutorialEventNames.CampaignNodeSelected)
      },
      new TutorialStep
      {
        id = "battle_loaded",
        titleKey = "tutorial.vertical.battle_loaded.title",
        narratorKey = "tutorial.vertical.battle_loaded.narrator",
        bodyKey = "tutorial.vertical.battle_loaded.body",
        advanceMode = TutorialAdvanceMode.Event,
        inputBlockMode = TutorialInputBlockMode.None,
        showNextButton = false,
        canGoBack = false,
        canSkip = true,
        advanceConditions = Conditions(TutorialEventNames.BattleStarted)
      },
      new TutorialStep
      {
        id = "use_ability",
        titleKey = "tutorial.vertical.use_ability.title",
        narratorKey = "tutorial.vertical.use_ability.narrator",
        bodyKey = "tutorial.vertical.use_ability.body",
        advanceMode = TutorialAdvanceMode.Event,
        inputBlockMode = TutorialInputBlockMode.None,
        showNextButton = false,
        canGoBack = false,
        canSkip = true,
        advanceConditions = Conditions(TutorialEventNames.BattleAbilityClicked)
      },
      new TutorialStep
      {
        id = "end_turn",
        titleKey = "tutorial.vertical.end_turn.title",
        narratorKey = "tutorial.vertical.end_turn.narrator",
        bodyKey = "tutorial.vertical.end_turn.body",
        advanceMode = TutorialAdvanceMode.Event,
        inputBlockMode = TutorialInputBlockMode.None,
        showNextButton = false,
        canGoBack = false,
        canSkip = true,
        advanceConditions = Conditions(TutorialEventNames.BattleTurnEnded)
      },
      new TutorialStep
      {
        id = "done",
        titleKey = "tutorial.vertical.done.title",
        narratorKey = "tutorial.vertical.done.narrator",
        bodyKey = "tutorial.vertical.done.body",
        advanceMode = TutorialAdvanceMode.Manual,
        inputBlockMode = TutorialInputBlockMode.None,
        showNextButton = true,
        canGoBack = false,
        canSkip = true
      }
    };
  }

  private static List<TutorialLocalizedText> BuildTexts()
  {
    return new List<TutorialLocalizedText>
    {
      Text("tutorial.vertical.intro.title", "Tutorial inicial", "Starter Tutorial", "Tutorial inicial"),
      Text("tutorial.vertical.intro.narrator", "\"La caravana despierta bajo un cielo incierto. Cada decisión marcará el pulso del viaje.\"", "\"The caravan wakes beneath an uncertain sky. Each decision will shape the rhythm of the journey.\"", "\"A caravana desperta sob um céu incerto. Cada decisão marcará o ritmo da jornada.\""),
      Text("tutorial.vertical.intro.body", "Este flujo prueba el sistema nuevo con una ruta corta: elegir un nodo, entrar en combate, usar una habilidad y terminar el turno.", "This flow tests the new tutorial system with a short route: choose a node, enter combat, use an ability, and end the turn.", "Este fluxo testa o novo sistema de tutorial com uma rota curta: escolher um nó, entrar em combate, usar uma habilidade e terminar o turno."),
      Text("tutorial.vertical.select_node.title", "Elige un nodo", "Choose a Node", "Escolha um nó"),
      Text("tutorial.vertical.select_node.narrator", "\"El camino no espera. Una senda abierta es una promesa y una amenaza.\"", "\"The road does not wait. An open path is both promise and threat.\"", "\"O caminho não espera. Uma trilha aberta é promessa e ameaça.\""),
      Text("tutorial.vertical.select_node.body", "Selecciona cualquier nodo disponible para mover la caravana.", "Select any available node to move the caravan.", "Selecione qualquer nó disponível para mover a caravana."),
      Text("tutorial.vertical.battle_loaded.title", "Combate iniciado", "Battle Started", "Combate iniciado"),
      Text("tutorial.vertical.battle_loaded.narrator", "\"El polvo se alza. Las órdenes deben ser claras antes de que el miedo decida por todos.\"", "\"Dust rises. Orders must be clear before fear decides for everyone.\"", "\"A poeira se ergue. As ordens devem ser claras antes que o medo decida por todos.\""),
      Text("tutorial.vertical.battle_loaded.body", "La batalla ya está cargada. El siguiente paso avanzará cuando pulses una habilidad.", "The battle is loaded. The next step advances when you click an ability.", "A batalha já carregou. O próximo passo avança quando você clicar em uma habilidade."),
      Text("tutorial.vertical.use_ability.title", "Usa una habilidad", "Use an Ability", "Use uma habilidade"),
      Text("tutorial.vertical.use_ability.narrator", "\"En combate, la intención pesa tanto como el acero. Elige una acción.\"", "\"In battle, intent weighs as much as steel. Choose an action.\"", "\"Em combate, a intenção pesa tanto quanto o aço. Escolha uma ação.\""),
      Text("tutorial.vertical.use_ability.body", "Pulsa una habilidad activa de la unidad actual.", "Click an active ability from the current unit.", "Clique em uma habilidade ativa da unidade atual."),
      Text("tutorial.vertical.end_turn.title", "Termina el turno", "End the Turn", "Termine o turno"),
      Text("tutorial.vertical.end_turn.narrator", "\"Nadie puede sostener la iniciativa para siempre. A veces, sobrevivir es ceder el momento.\"", "\"No one can hold the initiative forever. Sometimes, surviving means yielding the moment.\"", "\"Ninguém sustenta a iniciativa para sempre. Às vezes, sobreviver é ceder o momento.\""),
      Text("tutorial.vertical.end_turn.body", "Termina el turno para cerrar el recorrido mínimo del tutorial.", "End the turn to close the tutorial's minimum path.", "Termine o turno para fechar o percurso mínimo do tutorial."),
      Text("tutorial.vertical.done.title", "Tutorial completado", "Tutorial Complete", "Tutorial concluído"),
      Text("tutorial.vertical.done.narrator", "\"El primer aprendizaje deja huella. El resto del viaje cobrará su precio.\"", "\"The first lesson leaves a mark. The rest of the journey will claim its price.\"", "\"O primeiro aprendizado deixa marca. O restante da jornada cobrará seu preço.\""),
      Text("tutorial.vertical.done.body", "El vertical slice funciona. Desde este punto se pueden reemplazar pasos viejos por pasos data-driven.", "The vertical slice works. From here, old steps can be replaced with data-driven steps.", "O vertical slice funciona. A partir daqui, os passos antigos podem ser substituídos por passos data-driven.")
    };
  }

  private static List<TutorialCondition> Conditions(string eventId)
  {
    return new List<TutorialCondition>
    {
      new TutorialCondition { eventId = eventId }
    };
  }

  private static TutorialLocalizedText Text(string key, string es, string en, string pt)
  {
    return new TutorialLocalizedText
    {
      key = key,
      es = es,
      en = en,
      pt = pt
    };
  }

  private static void EnsureFolder(string parent, string name)
  {
    string path = parent + "/" + name;
    if (!AssetDatabase.IsValidFolder(path))
    {
      AssetDatabase.CreateFolder(parent, name);
    }
  }
}
#endif
