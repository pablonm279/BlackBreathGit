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
    if (definition.steps == null || definition.steps.Count == 0)
    {
      definition.steps = BuildSteps();
    }
    EditorUtility.SetDirty(definition);

    TutorialLocalizationTable localization = AssetDatabase.LoadAssetAtPath<TutorialLocalizationTable>(LocalizationPath);
    if (localization == null)
    {
      localization = ScriptableObject.CreateInstance<TutorialLocalizationTable>();
      AssetDatabase.CreateAsset(localization, LocalizationPath);
    }

    if (localization.texts == null)
    {
      localization.texts = new List<TutorialLocalizedText>();
    }
    UpsertTexts(localization.texts, BuildTexts());
    UpsertTexts(localization.texts, BuildMechanicalTimeTexts());
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

  private static List<TutorialLocalizedText> BuildMechanicalTimeTexts()
  {
    return new List<TutorialLocalizedText>
    {
      Text(
        "tutorial.intro3.body",
        "El <b>Aliento Negro</b> acumula tiempo de forma continua: <b>1 h por cada hora de campaña</b>. Durante los descansos en <b>Claros</b> acumula tiempo al 50%. Cuanto más cerca se encuentre, más peligrosos serán los caminos y mayores sus efectos nocivos.\n\nLa flecha muestra su distancia actual en horas.\n",
        "The <b>Black Breath</b> accumulates time continuously: <b>1 h per campaign hour</b>. During rests in <b>Clearings</b>, it accumulates time at 50%. The closer it gets, the more dangerous the roads and its harmful effects become.\n\nThe arrow shows its current distance in hours.\n",
        "O <b>Respiro Negro</b> acumula tempo continuamente: <b>1 h por hora de campanha</b>. Durante descansos em <b>Clareiras</b>, acumula tempo a 50%. Quanto mais perto estiver, mais perigosos serão os caminhos e maiores seus efeitos nocivos.\n\nA seta mostra sua distância atual em horas.\n"),
      Text(
        "tutorial.exp2.body",
        "<Size=85%><i><color=#A94444>---Los nodos de Región Expuesta garantizan una emboscada al viajar allí. Son combates más difíciles y extensos; si eres derrotado, perderás la partida.---</color></i></Size>\n\nLa <b>Exploración Activa</b> permite enviar 5 <b>Civiles</b> a revelar un <b>Nodo Misterioso</b>. La expedición dura <b>5 h</b>, por lo que el Aliento Negro también acumula 5 h, y a veces los exploradores pueden no regresar.\n\n<color=#FFD54A><b>Haz clic con el <b>Botón Derecho</b> en el Nodo seleccionado para enviar Exploradores.</b></color>",
        "<Size=85%><i><color=#A94444>---Exposed Region nodes guarantee an ambush when traveling there. These are harder, longer fights; defeat will end the game.---</color></i></Size>\n\n<b>Active Exploration</b> lets you send 5 <b>Civilians</b> to reveal a <b>Mysterious Node</b>. The expedition lasts <b>5 h</b>, so the Black Breath also accumulates 5 h, and sometimes the scouts may not return.\n\n<color=#FFD54A><b>Right-click the selected Node to send Scouts.</b></color>",
        "<Size=85%><i><color=#A94444>---Nós de Região Exposta garantem uma emboscada ao viajar para lá. São combates mais difíceis e longos; se você for derrotado, perderá a partida.---</color></i></Size>\n\nA <b>Exploração Ativa</b> permite enviar 5 <b>Civis</b> para revelar um <b>Nó Misterioso</b>. A expedição dura <b>5 h</b>, por isso o Respiro Negro também acumula 5 h, e às vezes os exploradores podem não voltar.\n\n<color=#FFD54A><b>Clique com o <b>Botão Direito</b> no Nó selecionado para enviar Exploradores.</b></color>"),
      Text(
        "tutorial.personaje2.body",
        "Las <b>Actividades de Campaña</b> permiten asignar tareas a los Personajes mientras la caravana viaja o descansa. Cada actividad acumula horas activas y aplica su resultado al completar <b>24 h activas</b>. Cambiar de actividad conserva por separado el progreso de cada una.\n\nCada Personaje posee opciones únicas según su clase.\n\n<color=#FFD54A><b>Cierra el menú de Personajes con la tecla \"C\" para continuar.</b></color>\n",
        "<b>Campaign Activities</b> let you assign tasks to Characters while the caravan travels or rests. Each activity accumulates active hours and applies its result after <b>24 active h</b>. Switching activities preserves each activity's progress separately.\n\nEach Character has unique options based on their class.\n\n<color=#FFD54A><b>Close the Characters menu with the \"C\" key to continue.</b></color>\n",
        "As <b>Atividades de Campanha</b> permitem atribuir tarefas aos Personagens enquanto a caravana viaja ou descansa. Cada atividade acumula horas ativas e aplica seu resultado ao completar <b>24 h ativas</b>. Mudar de atividade preserva separadamente o progresso de cada uma.\n\nCada Personagem possui opções únicas conforme sua classe.\n\n<color=#FFD54A><b>Feche o menu de Personagens com a tecla \"C\" para continuar.</b></color>\n"),
      Text(
        "tutorial.fatiga1.narrator",
        "“Excelente… la caravana necesita este descanso. Han sido muchas horas de viaje casi ininterrumpido, con apenas unas pocas pausas breves.\n\nSi exigimos demasiado a la caravana sin acampar… tarde o temprano terminará quebrándose.”",
        "“Excellent... the caravan needs this rest. It has been many hours of almost nonstop travel, with only a few short pauses.\n\nIf we demand too much from the caravan without camping... sooner or later it will break.”",
        "“Excelente... a caravana precisa deste descanso. Foram muitas horas de viagem quase sem parar, com apenas algumas pausas breves.\n\nSe exigirmos demais da caravana sem acampar... cedo ou tarde ela acabará quebrando.”"),
      Text(
        "tutorial.fatiga2.body",
        "Acampar implica que la caravana se detendrá para recobrar fuerzas y reacomodarse.\n\nAl descansar:\n-La <b>Fatiga</b> se restablecerá.\n-Los <b>Personajes</b> recuperarán más salud.\n-Cada <b>Civil</b> consumirá 1 <b>Suministro</b>. (Cada Buey 2)\n-El <b>Aliento Negro</b> seguirá avanzando.\n-Se dará un <b>Evento</b> aleatorio de descanso.\n-Se podrá seleccionar una <b>Tarea Civil</b>.\n\n<color=#FFD54A>Presiona el botón de <b>Acampar</b></color>",
        "Camping means the caravan will stop to recover strength and reorganize.\n\nWhen resting:\n-<b>Fatigue</b> will reset.\n-<b>Characters</b> will recover more health.\n-Each <b>Civilian</b> will consume 1 <b>Supply</b>. (Each Ox 2)\n-The <b>Black Breath</b> will keep advancing.\n-A random rest <b>Event</b> will occur.\n-A <b>Civil Task</b> can be selected.\n\n<color=#FFD54A>Press the <b>Camp</b> button</color>",
        "Acampar implica que a caravana parará para recuperar forças e se reorganizar.\n\nAo descansar:\n-A <b>Fadiga</b> será restaurada.\n-Os <b>Personagens</b> recuperarão mais saúde.\n-Cada <b>Civil</b> consumirá 1 <b>Suprimento</b>. (Cada Boi 2)\n-O <b>Respiro Negro</b> continuará avançando.\n-Ocorrerá um <b>Evento</b> aleatório de descanso.\n-Será possível selecionar uma <b>Tarefa Civil</b>.\n\n<color=#FFD54A>Pressione o botão <b>Acampar</b></color>"),
      Text(
        "tutorial.descanso1.body",
        "Durante cada <b>Descanso</b>, podrás asignar una <b>Tarea Civil</b> a la caravana. Sus condiciones quedan decididas al comenzar y sus resultados se aplican al finalizar las 6, 8 o 10 h correspondientes.\n\nEstas tareas pueden conseguir recursos, reducir riesgos o aumentar la Esperanza.\n\n<color=#FFD54A>Elige una Tarea Civil y presiona el botón de <b>Descanso</b></color>",
        "During each <b>Rest</b>, you can assign a <b>Civil Task</b> to the caravan. Its conditions are decided when the rest begins, and its results are applied when the corresponding 6, 8, or 10 h duration ends.\n\nThese tasks can obtain resources, reduce risks, or increase Hope.\n\n<color=#FFD54A>Choose a Civil Task and press the <b>Rest</b> button</color>",
        "Durante cada <b>Descanso</b>, você pode atribuir uma <b>Tarefa Civil</b> à caravana. Suas condições são decididas ao começar, e seus resultados são aplicados ao fim das 6, 8 ou 10 h correspondentes.\n\nEssas tarefas podem obter recursos, reduzir riscos ou aumentar a Esperança.\n\n<color=#FFD54A>Escolha uma Tarefa Civil e pressione o botão de <b>Descanso</b></color>"),
      Text(
        "tutorial.descanso1.narrator",
        "\"El campamento está montado. Durante estas horas no avanzaremos: descansaremos y también podremos asignar una tarea a la caravana. Sus resultados llegarán cuando termine el descanso.\"",
        "\"The camp is set. During these hours we will not move forward: we will rest and can also assign a task to the caravan. Its results will arrive when the rest ends.\"",
        "\"O acampamento está montado. Durante estas horas não avançaremos: vamos descansar e também poderemos atribuir uma tarefa à caravana. Seus resultados chegarão quando o descanso terminar.\""),
      Text(
        "tutorial.clima.body",
        "Cada día se realiza una <b>Tirada de Clima</b>, que puede cambiar las condiciones de la campaña.\n\n<color=#FFD54A>Puedes ver el Clima actual y sus efectos en el área marcada.</color>\n\nCada región puede tener sus propios climas e incluso condiciones especiales, como las <b>Almas Danzantes</b> del <b>Bosque Ardiente</b>.",
        "Each day, a <b>Weather Roll</b> is made, which can change campaign conditions.\n\n<color=#FFD54A>You can see the current Weather and its effects in the marked area.</color>\n\nEach region can have its own weather, and even special conditions, such as the <b>Dancing Souls</b> of the <b>Burning Forest</b>.",
        "A cada dia, é feita uma <b>Rolagem de Clima</b>, que pode mudar as condições da campanha.\n\n<color=#FFD54A>Você pode ver o Clima atual e seus efeitos na área marcada.</color>\n\nCada região pode ter seus próprios climas e até condições especiais, como as <b>Almas Dançantes</b> da <b>Floresta Ardente</b>."),
      Text(
        "tutorial.sequitos3.body",
        "El <b>Séquito de Curanderos</b> suma <b>+10%</b> al multiplicador global de curación. Cada mejora suma +5%, hasta +30%. Este total se suma con las demás mejoras globales y luego multiplica tanto la tasa de <b>4% por hora</b> al descansar como la de <b>2% por hora</b> al realizar otra actividad.\n\nAdemás, los Curanderos pueden tratar a Personajes con el estado <b>Herido</b>, aunque hacerlo no será barato.\n\n<color=#A94444><size=90%><i>Un Personaje queda Herido si cae en combate. Si vuelve a caer mientras ya tiene una Herida, morirá.</i></size></color>",
        "The <b>Healer Retinue</b> adds <b>+10%</b> to the global healing multiplier. Each upgrade adds +5%, up to +30%. This total is added to the other global upgrades and then multiplies both the <b>4% per hour</b> resting rate and the <b>2% per hour</b> rate while performing another activity.\n\nHealers can also treat Characters with the <b>Wounded</b> state, though doing so will not be cheap.\n\n<color=#A94444><size=90%><i>A Character becomes Wounded if they fall in combat. If they fall again while already Wounded, they will die.</i></size></color>",
        "O <b>Séquito de Curandeiros</b> soma <b>+10%</b> ao multiplicador global de cura. Cada melhoria soma +5%, até +30%. Esse total é somado aos demais aprimoramentos globais e depois multiplica tanto a taxa de <b>4% por hora</b> ao descansar quanto a de <b>2% por hora</b> ao realizar outra atividade.\n\nAlém disso, os Curandeiros podem tratar Personagens com o estado <b>Ferido</b>, embora isso não seja barato.\n\n<color=#A94444><size=90%><i>Um Personagem fica Ferido se cair em combate. Se cair novamente enquanto já tem uma Ferida, morrerá.</i></size></color>")
    };
  }

  private static void UpsertTexts(List<TutorialLocalizedText> destino, List<TutorialLocalizedText> cambios)
  {
    foreach (TutorialLocalizedText cambio in cambios)
    {
      int indice = destino.FindIndex(texto => texto != null && texto.key == cambio.key);
      if (indice >= 0)
      {
        destino[indice] = cambio;
      }
      else
      {
        destino.Add(cambio);
      }
    }
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
