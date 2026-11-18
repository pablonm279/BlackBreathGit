# Chino simplificado: primera fase

Inicio: 10 de septiembre de 2026. El usuario autorizó expresamente modificar `Assets/TRADU.cs`.

## Estado

Esta es una integración de desarrollo, no una localización completa ni aprobada para publicar.
`ChineseLocalization.ReleaseReady` permanece en `false`. El idioma se puede seleccionar en el
Editor y en builds de desarrollo; las builds normales siguen ofreciendo los tres idiomas existentes.
En una build normal, una preferencia china guardada durante desarrollo vuelve a inglés.

- Identificador persistido: `4`. Se conservan español `1`, inglés `2` y portugués `3`.
- Código de idioma y dimensión analítica: `zh-Hans`.
- Detección inicial: `SystemLanguage.ChineseSimplified`; respeta una preferencia válida ya guardada.
  Chino tradicional y el valor genérico `Chinese` no se convierten automáticamente a simplificado.
- Selector: `简体中文`, creado manualmente por el usuario en la escena. El código enlaza ese toggle por
  su callback persistente si el campo del Inspector no está asignado; no genera otro control.
- Catálogo: `Assets/Resources/Localization/zh-Hans.json`. Carga local, sin red ni servicios de traducción.
  Las claves son texto español exacto, incluidos espacios/etiquetas, o IDs del tutorial.
- Respaldo: chino → traducción inglesa existente → texto fuente. Los scripts con ramas propias de idioma
  todavía pueden mostrar español; están inventariados y requieren migración por dominio.
- Fuente: Noto Sans CJK SC Regular 2.004, archivo OTF de 16.437.364 bytes, licencia y atribución incluidas.
  TextMeshPro crea un atlas SDF dinámico de 2048×2048 con múltiples atlas habilitados, compartido como
  respaldo global. Las fuentes latinas permanecen asignadas a sus componentes.
- Primer lote: 179 entradas (UI, ajustes, terminología, títulos y algunas instrucciones del tutorial).
- Logo: se usa `logoChino` cuando está asignado, con respaldo en el inglés. Controles y créditos:
  reutilizan el panel español con textos chinos del catálogo.

## Tutorial protegido

Se mantienen los 34 pasos y los assets originales con sus 105 entradas, acciones, condiciones y tiempos.
La selección de texto chino consulta el catálogo por ID y utiliza inglés cuando falta esa entrada.
Las ayudas contextuales consultan `tooltip.<id>` y tienen el mismo orden de respaldo.
La narración usa los clips ingleses existentes; no hay audio chino generado.

Las pruebas de .NET verifican la selección de textos y las preferencias, con dobles mínimos de Unity.
La compilación utiliza las referencias reales del proyecto. Esto NO sustituye recorrer el tutorial en
Play Mode: faltan validar los 34 pasos completos, la aparición de botones, el texto animado, el ajuste
de líneas y la sincronización con la voz. El riesgo pendiente es de presentación y tiempos de lectura,
además de cualquier dependencia de idioma dispersa que todavía no se haya migrado.

## Glosario de trabajo inicial

Primer borrador para mantener consistencia durante las siguientes fases. No tiene revisión nativa.
Se conserva el título comercial **The Black Breath**; `黑息` nombra al fenómeno dentro del mundo.

| Español | zh-Hans | Uso |
| --- | --- | --- |
| Aliento Negro | 黑息 | Fenómeno del mundo |
| Guardasenderos | 护路人 | Tratamiento del protagonista |
| Contramaestre | 管事 | Narrador/administrador de la caravana |
| Caravana | 车队 | Convoy de civiles que escapan del Aliento Negro; no una caravana comercial |
| Esperanza | 希望 | Recurso/estado de campaña |
| Civiles | 平民 | Población protegida |
| Suministros | 补给 | Recurso consumible de campaña |
| Bueyes de Carga | 驮牛 | Capacidad de transporte |
| Séquitos | 随行队伍 | Grupos de especialistas |
| Caballero | 骑士 | Clase |
| Acechador | 潜猎者 | Clase |
| Canalizador | 导魔者 | Clase |
| PA | 行动点 | Puntos de acción |
| Ataque | 攻击 | Atributo de ataque |
| Defensa | 防御 | Atributo distinto de armadura |
| Armadura | 护甲 | Mitigación por armadura |
| Iniciativa | 先攻 | Orden de actuación |
| Ronda | 轮次 | Ronda global; reservar 回合 para el turno individual |

No traducir nombres de eventos, rutas de Resources, IDs, claves de guardado, nombres de clases C# ni
parámetros de analítica. No convertir automáticamente el candidato `Guardar` en “Save”: el traductor
actual lo usa también con el sentido de almacenar objetos. El contexto debe decidir cada ambigüedad.

## Herramientas reproducibles

Desde la raíz del proyecto (requiere .NET 10, Python 3 y PyYAML; disponibles en este entorno):

```powershell
dotnet run --project tools/localization/Inventory.csproj -- . outputs/localization
python tools/localization/audit.py --extract-assets
python tools/localization/audit.py --validate
python tools/localization/test-regression.py
python tools/localization/compile-check.py
powershell -ExecutionPolicy Bypass -File tools/check-runtime-deps.ps1 -FailOnHit
```

Los informes se escriben en `outputs/localization` (ignorado por Git):

- `code-inventory.json`: claves, traducciones ES/EN/PT recuperables, orígenes, llamadas dinámicas y ramas de idioma.
- `script-candidates.json`: literales amplios para inspección. Incluye identificadores y rutas; no es una lista para traducir ciegamente.
- `asset-inventory.json`: textos serializados y overrides, localizados por archivo y fileID. Lista explícitamente assets binarios omitidos.
- `tutorial-baseline.json`: copia de la definición y su hash para seguimiento de secuencias.
- `validation.json`: claves duplicadas/vacías, variables, etiquetas y glifos ausentes.
- `regression/results.log`: comparación de todos los textos inventariados contra `git HEAD:Assets/TRADU.cs`
  para los tres idiomas anteriores, más selección china, detección y textos reales del tutorial.
  La referencia es HEAD al ejecutar; ejecutar antes de un commit para contrastar con la versión anterior.
- `compile/*.log`: compilación de runtime y editor con los response files de Unity; no escribe en Library.

El extractor C# utiliza Roslyn, no expresiones regulares para analizar el lenguaje. No ejecuta el juego.
Los inventarios son puntos de partida de cobertura, no una prueba de que todo texto visible esté localizado:
falta revisar interpolaciones, recursos indirectos, texto en imágenes, rutas dinámicas y pantallas reales.

Resultados de esta primera fase:

- Compilación de `Assembly-CSharp` y `Assembly-CSharp-Editor`: sin errores.
- 10.555 comprobaciones aprobadas, incluidas las 2.481 claves inventariadas en los tres idiomas anteriores
  y la selección de las 105 entradas reales del tutorial para los cuatro idiomas.
- Catálogo: 179 entradas, 298 caracteres distintos, sin variables/etiquetas inválidas ni glifos ausentes.
- Cobertura de claves de código inventariadas: 92 de 2.481. Las entradas restantes del catálogo incluyen
  textos de escenas, IDs del tutorial y términos preparados para las siguientes fases.
- Inventario serializado: 3.060 entradas en 27 archivos, incluyendo 15 entradas vacías del tutorial.
- Chequeo de dependencias runtime: sin dependencias de editor/test detectadas.
- Escenas, prefabs y assets originales del tutorial: sin diferencias respecto a HEAD.

## Lotes 2-16 (Claude): traducción completa del texto inventariado

Continuación de la fase 1, solo sobre `Assets/Resources/Localization/zh-Hans.json`. No se
modificó `Assets/TRADU.cs` ni ningún script de traducción: las entradas nuevas usan el mismo
catálogo JSON y el mismo respaldo chino → inglés → fuente.

- 2.639 entradas en el catálogo. **Claves de código inventariadas: 2.481/2.481 (100%). Tutorial (90 textos `.body`/`.narrator`) y tooltips de campaña (15) también completos.**
- Lote 2: +269 estados de combate, log de batalla, atributos, tiradas de salvación,
  consumibles, equipo, clima, reacciones/trampas de enemigos y etiquetas de UI cortas.
- Lote 3: +43 descripciones largas de enemigos (bloques `<i>...</i><color>` con pros/contras).
- Lotes 4-8: campaña completa (855 claves inventariadas): títulos de eventos, botones de
  opción, estados de Fatiga, atributos, llegada/salida de séquitos, marcadores de recompensa
  (`<color><b>±N Esperanza/Suministros/Civiles</b></color>`), ramas de decisión, narrativa
  larga de eventos de descanso/viaje y descripciones de séquitos.
- Glosario nuevo: 经验 (Experiencia), 高昂士气/低落士气 (Alta/Baja Moral), 迅捷 (Presteza),
  民兵 (Milicia), 伏击 (Emboscada), 消耗品 (consumible), 尽责 (Compromiso), 鼓舞 (Inspiración),
  警戒 (Vigilante), 怯战 (Acobardados), 迟滞 (Aletargados), 超载 (Sobrecarga), 铁匠 (Herrero),
  治疗师 (Curanderos), 塞里亚 (Serria), 编年史价值 (Valor Crónica).
- Glosario aplicado: 护甲, 行动点, 先攻, 轮次(轮)/回合, 中毒, 流血, 眩晕, 定身, 隐匿, 闪避,
  护盾, 腐蚀, 黑息, 希望, 补给, 材料, 金币, 平民, 车队, 驮牛, 疲劳, 随行队伍, 净化者,
  内杜卡赞, 凯尔塔夫, 冻风隘口, 燃烧森林.
- Glifo `▸` (U+25B8) ausente en Noto Sans CJK SC: sustituido por `▶` (U+25B6) en los valores.
- Lotes 9-16 (`TRADU.cs`-only): ~1.220 entradas — nombres de habilidades, objetos (armas,
  armaduras por tier, gemas, consumibles, accesorios, reliquias, vestiduras), enemigos, estados,
  clima, botones de UI, descripciones de item y actividad (`<color><b>...</b></color><color><i>...</i></color>`),
  mecánicas de casilla, fragmentos de log de tirada (豁免检定, resultados d20), párrafos de
  eventos, banter y explicadores de mecánica de región. Alineación por clave (dict), no por
  índice, con aborto ante clave sin traducir o etiquetas que no cuadran.
- Glosario nuevo en estos lotes: 双手大剑 (Mandoble), 短剑 (Espada Corta), 刺剑 (Estoque),
  皮甲/重甲/轻甲 (armaduras), 战袄 (Gambeson), 扎奇尔 (Zarkil), 瓦格拉尼洛 (Vagranilo),
  法格丹 (Faagdan), 卡德林 (Kadryn), 湮灭 (Perdición), 尖啸 (Chirrido), 难度 (DC), 骰点/检定,
  烽火台 (Almenaras), 念动力 (Telekinesis), 决斗者 (Duelista).
- Lote 17: +71 — los 56 textos `.body`/`.narrator` restantes del tutorial (los `.title` ya
  estaban) y los 15 `tooltip.*` de campaña/combate. Se conservan `The Black Breath` como título
  comercial y 护路人/管事/格莱恩森林/净化圣咏 para el diálogo del Contramaestre. Parity de
  etiquetas verificada contra el texto ES del `asset-inventory` (no contra la clave), incluidos
  los `<color><i>…</color></i>` mal anidados y `<size=95%>`. No se tocó `TutorialVerticalSlice.asset`
  ni secuencia/acciones/tiempos: solo se agregaron entradas al `zh-Hans.json` que consume
  `TutorialLocalizationTable` / `TutorialTooltipCatalog` con el mismo fallback a inglés.
- **Todas las claves de código inventariadas (`Traducir` + `TRADU.case`), los 90 textos del
  tutorial y los 15 tooltips están traducidos.** Sin cubrir: textos de escenas/prefabs no
  localizados por `Traducir` (~1.800 literales serializados, requieren triaje de visibilidad),
  interpolaciones dinámicas, texto en imágenes, narración de audio y las 15 entradas vacías del
  tutorial.
- Validación tras el lote 17: `audit.py --validate` sin errores ni glifos ausentes (2.639
  entradas, 1.811 caracteres distintos); `test-regression.py` PASS (13.015 aserciones, ES/EN/PT
  sin cambios respecto a HEAD); `compile-check.py` sin errores en runtime ni editor.

## Lote 18 (Claude): clima de prepartida, presagios y manual de campaña/combate

Astra agregó tres sistemas nuevos (`UIManagerContZonas.cs`/`ClimaZonaCatalog`, `PresagioCatalog.cs`,
y el `HandbookManager` de `ES-Campaña.unity`) con switches ES/EN/PT hardcodeados en C#, sin rama de
chino — a diferencia del resto del proyecto, este texto no pasaba por `Traducir()`/`TRADU.case` ni
por el catálogo, así que el inventario nunca lo vio.

- **Clima de prepartida** (`Assets/UIManagerContZonas.cs`, clase `ClimaZonaCatalog`): se agregó un
  parámetro `zh` a `LocalizarTooltip` y un `case TRADU.IdiomaChinoSimplificado` en
  `ObtenerTextoClimaExclusivoDesconocido`. Los 9 tooltips de clima de la pantalla de selección de
  región (Despejado, Ola de Calor, Lluvia, Nieve, Niebla, Almas Danzantes, Aurora Boreal,
  Nedukazal normal/masacre) + el texto de clima exclusivo desconocido ahora tienen chino inline.
- **Presagios/Omens** (`Assets/Scripts/Campania/PresagioCatalog.cs`): se agregaron los campos
  `nombreZh`/`descripcionZh` a `PresagioDefinition`, un `case` de chino en
  `ObtenerTextoLocalizado()` y `ObtenerNombreLocalizado()`, y chino en `ObtenerTextoSinPresagios()`.
  Las 53 definiciones de presagio (caminos, recursos, eventos, nodos, exploración, esperanza,
  peligros, personajes, aliento negro, enemigos) tienen ahora nombre y descripción en chino.
  Término elegido para "Presagio": 预兆.
- **Manual de Campaña y Manual de Combate** (`HandbookManager.cs` + `ES-Campaña.unity`): el
  Handbook usa GameObjects hijos por idioma (hijo 1=ES, 2=EN, 3=PT) sin slot de chino;
  `ActivarContenidoPorIdioma` ya caía al hijo 1 (español) para chino por no matchear ningún case.
  Como `TRADU.TraducirTodosTextosSegunIdioma()` recorre **todos** los `TextMeshProUGUI` de la
  escena con `includeInactive: true` y ya traduce a chino cualquier texto original que esté en el
  catálogo, **no hizo falta tocar ningún script ni la escena**: alcanzó con extraer el texto
  español real de las 7 pestañas del manual de campaña (Zonas, Mapa, Civiles, Recursos, Aliento
  Negro, Personajes, Estados/Exploración — estas dos últimas comparten GameObject por una
  referencia duplicada de Astra) y las 7 pestañas con contenido del manual de combate
  (Zonas→Conceptos Básicos, Mapa→Estructura del Campo, Civiles→Turnos y Rondas,
  Recursos→Puntos de Acción, Aliento Negro→Ataque vs Defensa/Variaciones,
  Personajes→Daño/Tipos de Daño, Estados→Valentía/Estados; la pestaña Exploración del manual de
  combate está vacía) directamente de `ES-Campaña.unity` (parseo YAML de la jerarquía
  GameObject → Transform → hijos, no edición), y agregarlo como clave/valor del catálogo. El
  texto se extrajo con un script ad-hoc (no versionado); las claves largas quedaron en
  `zh-Hans.json` con sus `<b>...</b>` intactos.
- +32 entradas nuevas de catálogo (7 ya eran claves cortas ya traducidas: "Regiones", "Mapa",
  "Civiles", "Recursos", "Aliento Negro", "Personajes", "Puntos de Acción").
- Se mantiene "The Black Breath" sin traducir al referirse al juego/partida completa (no al
  fenómeno); ahí se usó `<b>The Black Breath</b>` en vez de comillas de libro para conservar la
  paridad de etiquetas contra el texto fuente.
- Validación: `audit.py --validate` sin errores ni glifos ausentes (2.755 entradas, 1.870
  caracteres distintos); `test-regression.py` PASS (13.677 aserciones — la suite de Astra ahora
  también valida "42 quick components" y "6 pregame tooltips" en chino); `compile-check.py` sin
  errores en runtime ni editor.
- Sin tocar: los ~1.800 literales serializados triados en el lote anterior, ni el bug de Astra
  donde `solapaEstados` y `solapaExploracion` del manual de campaña apuntan al mismo GameObject
  (ambas pestañas muestran el contenido de Exploración Pasiva) — no se corrigió por no ser parte
  del pedido y no ser un problema de traducción.
- Falta igual: **revisión LQA nativa de China continental** (todo el borrador la necesita, en
  especial eventos, banter, lore y la narración del tutorial) y prueba visual en Play Mode
  (recorrer los 34 pasos: saltos de línea, desbordes, texto animado, tiempos de lectura,
  alineación del selector).

## Lote 19 (Claude): barrido de switches ES/EN/PT hardcodeados restantes

Tras el Lote 18 se hizo un barrido de todo `Assets/` buscando más patrones
`switch (idioma) { case TRADU.IdiomaIngles: ...; case TRADU.IdiomaPortugues: ...; default: ... }`
o helpers locales `Loc(es, en, pt)` sin rama de chino — el mismo problema estructural que Astra dejó
en clima/presagios/handbook, repetido en otros ~11 archivos que el inventario de `Traducir()` no ve
por no pasar por el catálogo. `disclaimerManager.cs` se investigó y se descartó: referencia GameObjects
(`DisclaimerESP/ENG/POR`) que no existen en ninguna escena — parece código muerto/desconectado.

- **`Assets/AsentamientoManager.cs`**: el helper local `Loc(es, en, pt, zh = null)` (respaldo a
  inglés si falta `zh`) ya lo había agregado Astra parcialmente; se completó chino en los ~24 call
  sites restantes (acciones del asentamiento: Puesto Comercial, Taverna, Plaza Principal, Posada,
  Marcharse, títulos, descripciones con interpolación de costos/civiles, botón "Continuar").
- **`Assets/Scripts/Campania/MenuDescanso.cs`**: chino en el log de atajo de superficie encontrado
  durante el descanso.
- **`Assets/Scripts/Campania/CampaignManager.cs`**: chino en el texto de carga inicial, en el
  contador de "descansos" restantes (dos copias del mismo cálculo, una con `switch` de enteros
  crudos), en el log de viaje por atajo de superficie, en el log de viaje con nieve, y en las ~21
  llamadas del helper `LocExploradores(es, en, pt, zh = null)` (envío/resultado de exploradores:
  errores de validación, éxito/fallo crítico y normal, mensajes de facción avistada).
- **`Assets/Scripts/UI/UIEstadoCuadro.cs`**: chino en la etiqueta "Acumulado:" de Condena.
- **`Assets/Scripts/UI/BotonHabilidad.cs`**: chino en "Fervor insuficiente" (狂热不足, coherente con
  la traducción existente de "Fervor"), "Requiere estar escondido" y "PA insuficientes" (行动点不足,
  coherente con la traducción existente de "Puntos de Acción").
- **`Assets/Scripts/Campania/EventosAdmin.cs`** (el más grande, ~4.945 líneas): se agregaron
  parámetros opcionales de chino (con respaldo a inglés) a los helpers compartidos
  `RegistrarTiradaD20`, `RegistrarTiradaPorcentual`, `AgregarDescripcionTiradaD20`,
  `AgregarDescripcionTiradaPorcentual`, `SeleccionarTextoLocal` y `ObtenerNombreTipoTirada`
  (términos: 豁免检定/难度/成功/失败 para tiradas de salvación, 反射/坚韧/意志 para los tres tipos).
  Se tradujeron los 81 call sites reales de los cuatro helpers (112 strings únicas — descripciones
  de eventos aleatorios de campaña: paso precario, bodega de aire enrarecido, rumor de desbande,
  vado traicionero, carro encajado, bestias aterradas, hielo quebradizo, brecha, interrogatorio por
  arcas robadas, discurso ante el noble, cacería, marcas del correo, fila, civil agotado, cartera
  de viaje, lectura de ruta, techo inestable, arenga bajo la lluvia, rastro sospechoso, maniobras,
  augurio de cuervos, filo de hielo, riesgo del río) más 21 llamadas directas a
  `SeleccionarTextoLocal` fuera de esos helpers (textos largos de eventos: Caravana Perdida,
  Aserradero Abandonado, Civiles Perseguidos, Noche Tranquila, Voto de Confianza, Anciano del
  Lugar, Sueño Inspirador, y el log de "renunció al interrogatorio"). Se extrajeron y aplicaron con
  un script ad-hoc de emparejamiento por texto exacto (no por índice), igual que en lotes
  anteriores, para evitar desalineación.
- **`Assets/Scripts/Campania/Nodo.cs`**: chino en el tooltip de nodo misterioso con exploradores y
  en "Fuera del alcance de visión".
- **`Assets/Bitacora.cs`**: chino en el texto narrativo de subida de nivel, en "Amenazas del
  subsuelo", y se agregó `ObtenerTextoChino(TextoBitacoraId id)` (nuevo, análogo a
  `ObtenerTextoEspanol/Ingles/Portugues`) con las 28 entradas de la bitácora (etiquetas de Día/Ronda,
  recursos, descanso, resumen de victoria/derrota, caída de personajes, y los 15 textos de llegada
  a cada tipo de nodo).
- **Los dos scripts de IA de la Bruja Quemada** (`Assets/Prefabs/Prefabs NPC/ZONA - Bosque de los
  Lamentos/Bruja Quemada/IAInvocarRaizMalditaBruja.cs` e `IACondenaFerozBruja.cs`): chino en los
  fragmentos de log de invocación ("召唤出一株" + "诅咒之根") y de aplicación de Condena Feroz
  ("释放凶暴定罪于", con "于" final para que la concatenación con el nombre del objetivo funcione
  igual que en español/inglés/portugués).

**Nota técnica sobre `Assets/Bitacora.cs`:** este archivo está escrito casi en ASCII puro; todo
carácter no-ASCII (acentos ES/PT existentes, y ahora el chino agregado) se representa como escape
Unicode de C# (`\uXXXX`) en el código fuente en vez de bytes UTF-8 literales. Es una convención
preexistente del archivo (no introducida en este lote) y es C# válido; se respetó al agregar chino
para no mezclar convenciones dentro del mismo archivo.

- Validación: `audit.py --validate` sin errores ni glifos ausentes (2.755 entradas del catálogo,
  sin cambios — este lote no tocó `zh-Hans.json`, todo el texto es literal en C#);
  `test-regression.py` PASS (13.677 aserciones, sin cambios respecto al Lote 18);
  `compile-check.py` sin errores en runtime ni editor, verificado tras cada archivo.
- **Importante:** a diferencia del catálogo (`zh-Hans.json`), estos strings viven directo en código
  C# y no pasan por `audit.py --validate`, así que no se verificó automáticamente su cobertura de
  glifos contra la fuente Noto Sans CJK SC ni paridad de placeholders/etiquetas — se revisaron a
  mano. Mismo tratamiento que ya recibieron clima/presagios/handbook en el Lote 18.
- Sin tocar: `disclaimerManager.cs` (parece código muerto), el bug de `solapaEstados`/
  `solapaExploracion` del Lote 18, y los literales serializados de escenas/prefabs (ver triaje
  abajo). Falta igual la revisión LQA nativa y la prueba visual en Play Mode de todo este texto
  nuevo (especialmente los eventos de campaña, que son numerosos y con mucho detalle narrativo).

## Triaje de los 1.818 textos serializados en escenas/prefabs (Claude)

**Nota de revisión posterior:** las conclusiones de esta sección son históricas y no deben usarse
como lista vigente de exclusiones. `ES-Campaña.unity` sí está habilitada en la build. Los paneles del
manual tienen referencias a `HandbookManager` y los créditos contienen TMP traducible. El usuario
pidió expresamente NO traducir el handbook; queda fuera del trabajo actual. Ver la continuación abajo.

De los 1.818 literales ES en `.unity`/`.asset` no cubiertos por `Traducir()`, solo hay
**420 valores distintos**; el resto son repeticiones del mismo texto en múltiples objetos.

- **~350 (la mayoría) viven en `Assets/_Recovery/0*.unity`.** No están en
  `ProjectSettings/EditorBuildSettings.asset` (la build solo lista `ES-MenuPrincipal.unity`) ni
  son cargadas por nombre en runtime; incluyen basura de desarrollo (`"dfgdfgdfgdfgdg"`,
  `"Option A"`). **No se traducen: son escenas muertas, no shippean.**
- De los ~59 valores restantes en escenas/prefabs reales, se agregaron los **11 con mayor
  certeza de ser contenido real y visible**: el cartel de título `EL ALIENTO NEGRO`, el
  placeholder `(video cinemático en producción)`, el **texto de introducción/lore completo**
  (`ES-Campaña.unity`, componente `TraduccionRapida`), una línea de banter (`GOBanter.prefab`),
  y las etiquetas cortas de atributos del HUD de batalla (`ES-Batallas.unity`: Armadura,
  Defensa, Fortaleza, Mental, Nombre, Resistencias).
- **El resto (~48) queda sin traducir a propósito**, por dos motivos que necesitan una decisión
  o una revisión en Play Mode, no una traducción a ciegas:
  1. Varios objetos en `ES-Campaña.unity` tienen el mismo texto repetido en ES, EN y PT como
     literales separados (p. ej. la descripción de "Bueyes", el ejemplo de Golpe Crítico, la
     explicación del Mapa) — parecen paneles de referencia para desarrollo, no UI en vivo.
  2. Las descripciones largas en los prefabs `Sequito00X - *.prefab` coinciden con texto que
     **ya está traducido** como clave de `TRADU.cs` (agregado en lotes anteriores); lo serializado
     en el prefab es casi seguro el placeholder de diseño que el código sobreescribe en runtime
     con el mismo string ya cubierto. Traducirlo aparte arriesga duplicar entradas sin uso real.
  3. Los créditos (`ES-MenuPrincipal.unity`) siguen el mismo patrón que el logo y el panel de
     controles: hay una versión por idioma como GameObject separado, no como texto traducible;
     agregar chino ahí es un cambio de escena/código, no de catálogo.
- Nada de este lote tocó `_Recovery/`, prefabs de Séquito, ni paneles de créditos/controles.

### Fix de fuente CJK (Claude)

`Assets/Scripts/Localization/ChineseFontFallback.cs`: el atlas dinámico se destruía en los
cambios de escena (`Resources.UnloadUnusedAssets`) porque solo el `TMP_FontAsset` tenía
`HideFlags.DontSave`, no su textura ni su material → `MissingReferenceException: m_AtlasTextures`.
Ahora el flag se aplica también al material y a todas las texturas del atlas, se valida que el
atlas siga vivo y se regenera si hace falta, y la lista de respaldo de TMP se reconstruye
descartando referencias muertas. El toggle `简体中文` se maneja desde la escena
(`OpcionesCargarPlayerPrefsUI.OnChinoSimplificadoToggle`, público); se quitó el generador de
toggle en runtime.

## Siguientes fases

1. Completar el catálogo de UI y los 105 textos del tutorial, aplicar el glosario con revisión de contexto.
2. Migrar ramas de idioma y frases concatenadas por dominio: combate, habilidades/objetos, campaña, eventos y banter.
   Usar frases completas con variables conservadas, sin cambios de balance, secuencia ni lógica del tutorial.
3. Auditar las ayudas, imágenes con texto, selecciones de audio, logos y controles. Medir cobertura de glifos
   otra vez después de cada lote y probar cambios de idioma/carga de partidas.
4. Recorrer menús, campaña, combate y tutorial en Unity. Revisar memoria de los atlas, saltos de línea,
   desbordamientos, alineación del selector y legibilidad a distintas resoluciones/escalas.
5. LQA nativa de China continental. Steam, logros, tráiler, capturas y voz china siguen siendo entregables separados.
6. Habilitar `ReleaseReady` únicamente después de completar las fases y aceptar la calidad de publicación.

## Continuación: menús, resultados y componentes de texto

El usuario confirmó que la fuente y el selector ya funcionan. Se conservaron el arreglo del atlas,
el toggle manual y el logo chino. No se añadieron traducciones del handbook ni se modificó su script.

- Catálogo: 2.723 entradas, frente a las 2.650 recibidas. Las 2.513 claves de código actualmente
  inventariadas están cubiertas; esto no implica que todo el texto del juego esté localizado.
- `TraduccionRapida`: hay 42 componentes en escenas/prefabs primarios, identificados por el GUID del
  script. Todas sus claves españolas tienen traducción china. Incluye las descripciones de las seis
  clases, antorchas, avisos de demo, final de región, lore y etiquetas de menú. El componente toma el
  chino del catálogo; no necesita un cuarto campo serializado en cada GameObject.
- Hay además 6 componentes `PrePartidaTooltip` que usan campos similares. Sus textos también están
  cubiertos y se corrigió la selección que antes convertía el idioma 4 en español.
- Nueva partida: regiones y sus descripciones/mecánicas, clase aleatoria, aviso de sobrescritura con
  nombre de partida, botones y avisos de regiones no disponibles tienen selección china explícita.
- Batallas: poder aliado/enemigo, facciones y avisos de emboscada; resultados de victoria y derrota
  usan frases chinas completas con cantidades. `ChineseBattleText` solo formatea texto y no aplica
  recompensas, pérdidas, balance ni cambios de estado.
- Créditos y cuatro variantes de controles: traducciones de los textos TMP reales, conservando
  nombres de colaboradores, contacto y teclas. No se editaron escenas o prefabs para este lote.
- Auditoría: `audit.py --extract-assets` ahora registra el GUID del componente, permitiendo separar
  `TraduccionRapida` de otros scripts con campos homónimos. `triage-assets.py` genera jerarquías y
  scripts asociados en `outputs/localization/asset-triage.json`; no determina visibilidad por el
  nombre del objeto ni por estar inicialmente inactivo.

Pruebas: catálogo sin errores de variables/etiquetas ni glifos faltantes (1.853 caracteres distintos).
El arnés de .NET usa el código real de ambos componentes y comprueba sus textos serializados en
ES/EN/PT/zh, apertura, eventos de idioma, cierre/reapertura y respaldo inglés del tooltip. También
prueba resultados de batalla sin botín, solo esperanza, recompensas mixtas y pérdidas de bueyes.
Resultado: 13.645 aserciones correctas. Compilación con referencias reales de Unity: cero errores en
runtime y editor. El chequeo de dependencias runtime tampoco detectó referencias prohibidas.

Los assets, pasos, condiciones y tiempos del tutorial siguen sin cambios en este lote. No se ejecutó
Play Mode: siguen pendientes el recorrido de los 34 pasos, la prueba visual de los textos añadidos,
los saltos de línea y la sincronización con voz inglesa. `ReleaseReady` permanece en `false`.

Fuera de este lote: ramas explícitas de idioma y texto dinámico en otros dominios (incluidos banters
con tablas propias), otros textos serializados que requieren revisar sus rutas de uso y LQA nativa.

## Lote 20 (Claude): todas las habilidades de personaje, las 6 clases

Barrido completo de `Assets/Scripts/Clases/*/Habilidades/` y `_ItemsExclusivos/` para agregar la
rama china a los switches ES/EN/PT hardcodeados en `ActualizarDescripcion()` de cada `Habilidad`,
más los componentes de reacción, marcas y trampas asociados que también construyen texto por idioma
fuera del catálogo (`Traducir()`). Alcance: las 6 clases jugables completas —
Acechador (17 archivos), Caballero (16), Canalizador (17), Duelista (18, incluidas 2 reacciones y
3 variantes de arma), Explorador (16, incluidas 3 variantes de arco), Purificadora (15, incluida
1 reacción y 1 variante de báculo) — alrededor de 99 archivos con rama de chino agregada. Se excluyeron
por no necesitarla: `AtaqueBasico.cs`/`BoladeFuego.cs` (implementación vacía), `DestruirObstaculo.cs`
(ya enrutado por el catálogo), y los componentes `arma*.cs`/`armadura*.cs` de flavor text de items
(no son habilidades; se confirmó que ni siquiera las clases ya completadas de sesiones anteriores
—p. ej. Acechador— les habían agregado chino, manteniendo el mismo criterio de alcance).

- **Infraestructura compartida extendida en `Assets/Scripts/Habilidades/Habilidad.cs`** antes de
  tocar ninguna habilidad individual, para que el resaltado automático de términos y números
  funcionara igual que en ES/EN/PT:
  - `PalabrasClaveBeneficio`/`PalabrasClavePerjuicio` (coloreado verde/rojo de números según
    contexto): se agregaron palabras clave chinas (恢复, 治疗, 获得, 赋予, 提升, 增加, 再生 /
    受到, 损失, 遭受, 伤害, 降低, 减少).
  - `TerminosColoreadosDescripcion` (los ~29 patrones regex que envuelven términos reconocidos en
    `<link="skill-term:id">`): se añadió una alternativa literal china a cada patrón, sin límites
    `\b` porque el chino no tiene concepto de límite de palabra.
  - `ConstruirLineaSalvacion` extendido con `esChino`/`atributoEscalaZh` (豁免：坚韧/反射/意志).
  - `FormatearTextoProbabilidadExito` extendido con china (`"{porcentaje}% 成功率"`).
  - `TerminoHoverDetector.cs`: rama china en `GenerarDescripcionDado` (texto del tooltip al pasar
    el mouse sobre notación de dados sin catálogo, p. ej. "1d6").
  - `CatalogoTerminosJuego.cs`/`.asset`: se agregó el campo `descripcionZh` (con respaldo a inglés
    si falta) al `ScriptableObject` de 64 términos usado por los tooltips de hover sobre términos
    resaltados; se tradujeron las 64 descripciones y se editó el `.asset` con un script Python
    ad-hoc que preserva el encabezado YAML y el fin de línea LF puro del archivo (se detectó y
    corrigió un bug propio del primer intento del script, que omitía el encabezado; verificado con
    `git diff --stat` limpio de solo inserciones antes de continuar).
  - **Hallazgo técnico importante:** `\w` en regex de .NET es Unicode-aware y coincide con
    caracteres CJK, así que un número pegado sin espacio a texto chino no es detectado por
    `RegexNumeroDescripcion` (el resaltador verde/rojo). Convención adoptada en todo este lote:
    siempre insertar un espacio entre texto chino y números adyacentes en las cadenas nuevas.
- **Terminología reutilizada o acuñada** (consistente entre las 6 clases; los términos ya usados en
  lotes anteriores no se repiten aquí): 屏障 (Barrera), 心智豁免 (Salvación Mental), 坚韧 (Fortaleza),
  反射 (Reflejos), 闪避 (Evasión), 暴击 (Crítico), 护甲穿透 (Penetración de Armadura), 护甲 (Armadura),
  防御 (Defensa), 攻击 (Ataque), 力量/敏捷 (Fuerza/Agilidad), 行动点/最大行动点 (PA/PA máx.), 冲劲
  (Impulso), 勇气 (Valentía), 先攻 (Iniciativa), 隐匿 (Oculto), 踉跄 (Tambaleando), 流血 (Sangrado),
  斩击伤害/穿刺伤害 (Daño Cortante/Perforante), 奥术伤害 (Daño Arcano), 火焰伤害 (Daño de Fuego, acuñado
  en este lote — sin precedente), 神圣伤害 (Daño Divino), 酸性伤害 (Daño Ácido, acuñado en este lote),
  燃烧 (Ardiendo), 致盲 (Ciego/Cegado), 减速 (Ralentizado), 已标记/被嘲讽 (Marcado/Provocado), 狂热
  (Fervor), 能量等级/能量残留 (nivel de Energía/Residuo Energético, ambos del Canalizador), 威力（X）
  (fórmula estándar para "Poder (X)" en líneas de daño). Nombres de habilidad traducidos libremente
  al no existir aún en el catálogo (ejemplos: 潜伏=Acechar, 弓箭射击=Tiro con Arco, 光辉之柱=Pilares de
  Luz, 净化=Purificación, 神圣火焰=Llama Divina, 炫目之光=Luz Cegadora, 疗愈=Enmendar, 信仰之盾=Escudo de
  Fe, 净化圣咏=Salmo Purificador, 神圣回响=Ecos Divinos, 圣光光环=Aura Sagrada, 同心狂热=Fervor Conjunto,
  刺剑冲锋=Carga de Estoque, 戒备架势=En Garde, 强硬架势=Postura Demandante, 刺剑之舞=Danza del Estoque,
  连射=Ráfaga, 标记猎物=Marcar Presa, 篝火=Fogata, 临时造箭=Improvisar Flechas, 警戒=Vigilancia,
  法杖打击=Golpe de Bastón). "Riposte" se mantuvo como 还击 (ya existente en el catálogo para esa
  palabra), igual que en ES/EN/PT donde el nombre francés se conserva en el título pero no en el
  cuerpo del texto.
- Se corrigió en el camino un error de tipeo propio (coma china "，" en posición de sintaxis C# en
  vez de dentro del string, detectado por `compile-check.py` inmediatamente después de introducirlo).
- Se agregaron también ramas de chino a mensajes de battle-log fuera de `ActualizarDescripcion()`
  que antes solo tenían ES/EN/PT: `ReaccionRiposte.cs` (texto de intercepción y descripción de la
  reacción), `ReaccionPresenciaProvocadora.cs`, `ReaccionCastigarMalvados.cs`, `MarcaMarcarPresa.cs`
  (descripción de la marca), `VigilanciaTrampa.cs` ("no more arrows"), y `SalmoPurificador.cs` (el
  log de remoción de debuff, que en ES/EN/PT concatena con una preposición al final —" from "/" de
  "— y en chino requiere reordenar las variables para mantener gramática correcta en vez de solo
  insertar un conector).
- **Verificación posterior con un subagente:** tras terminar las 6 clases se hizo una segunda pasada
  automatizada que confirmó que los 101 archivos con `ActualizarDescripcion()` real ya tenían la
  rama china, pero encontró 3 reacciones adicionales que construyen `descripcion` en `Start()` (no
  en `ActualizarDescripcion()`, así que el patrón de búsqueda original no las cubría):
  `Assets/Scripts/Clases/Canalizador/Habilidades/ReaccionSifonArcano.cs`,
  `ReaccionEscudoEnergetico.cs` (mismo directorio) y
  `Assets/Scripts/Clases/Caballero/Habilidades/ReaccionPosturaDefensiva.cs`. Se agregó la rama
  china a las tres. El mismo subagente también señaló los archivos base `arma*.cs`/`armadura*.cs`
  de flavor text de items (Báculo Purificador, Arco Largo, Armadura de Cuero, Estoque, Gambeson,
  Mandoble, Espada Corta) y `Actividad_RitualDeLimpieza.cs`; se confirmó que quedan fuera de alcance
  a propósito (no son habilidades de personaje, y las clases ya completadas en sesiones anteriores
  tampoco les habían agregado chino).
- Validación tras cada archivo: `compile-check.py` (Assembly-CSharp y Assembly-CSharp-Editor, 0
  errores en cada paso). Al final del lote completo: `audit.py --validate` (2.755 entradas del
  catálogo — 64 más que el Lote 19 por los `descripcionZh` de `CatalogoTerminosJuego.asset`, sin
  glifos faltantes, sin errores) y `test-regression.py` (13.677 aserciones, PASS).
- **Importante, igual que en el Lote 19:** las cadenas nuevas de habilidades viven directo en C# y
  no pasan por `audit.py --validate`, así que su cobertura de glifos y paridad de placeholders se
  revisó a mano, no automáticamente.
- Sin tocar en este lote: `TRADU.cs`, el catálogo salvo el campo `descripcionZh` ya mencionado, y
  cualquier texto de Actividades/eventos de campaña específicos de clase (fuera del árbol de
  Habilidades). Falta la revisión LQA nativa y la prueba visual en Play Mode de los tooltips de
  habilidad y tarjetas de nivel para las 6 clases en chino.
## Lote 21 (Claude): flavor text de items, Actividad de Purificadora, Handbook y barrido de bugs en scripts de campaña

Trabajo hecho en respuesta a un pedido explícito del usuario de completar 4 puntos pendientes tras
un informe de estado ("¿está todo traducido? ¿qué falta?"): (1) flavor text de armas/armaduras,
(2) `Actividad_RitualDeLimpieza.cs`, (3) el Handbook, (4) los ~48 textos serializados restantes en
escenas/prefabs catalogados por `triage-assets.py`.

- **(1) Flavor text de items exclusivos** — se agregó la rama china (`else if
  (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)`) al `Awake()` de las 7 clases base de items
  que no habían sido tocadas en el Lote 20 a propósito (ver nota de esa sección): `armaBaculoPurificador.cs`,
  `armaArcoLargo.cs`, `armaduraDeCuero.cs` (compartida con Acechador), `armaEstoque.cs`,
  `armaduraGambeson.cs`, `armaMandoble.cs`, `armaEspadaCorta.cs`. Los nombres de item usados ya
  existían como claves del catálogo (verificado con `grep` antes de escribir cada uno): 净化法杖,
  长弓/酸蚀长弓/强力长弓/减速长弓, 皮甲 y variantes (强化/坚韧/死灵/模糊), 刺剑 y variantes (星界/首滴血/
  疾速还击/黑玫瑰), 战袄 y variantes (沉着/最后一步/轻剑术), 双手大剑 y variantes (冰封/神圣/渴血), 短剑
  y variantes (奥术/黑刃/噬命). El flavor text menciona la habilidad del arma por su título chino ya
  establecido en el Lote 20.
- **(2)** `Actividad_RitualDeLimpieza.cs`: faltaba el `case` de chino en el switch de
  `ObtenerDescripcion()` (las otras 17 Actividades de Purificadora ya tenían el texto en el catálogo
  vía `Traducir()`; esta es la única que construye la descripción con un switch literal en vez de
  pasar por el catálogo — confirmado por subagente de auditoría).
- **(3) Handbook**: se investigó una contradicción en este mismo documento (el Lote 18 decía que el
  handbook estaba traducido; una nota suelta al final del archivo decía que se había excluido a
  pedido del usuario). Se verificó código y catálogo directamente en vez de confiar en cualquiera de
  las dos afirmaciones: `HandbookManager.cs` usa el patrón de "hijo por idioma" (0=ES/fallback,
  1=EN, 2=PT, sin caso chino explícito — cae a hijo 0/español) y el texto español de cada sección
  del handbook SÍ tiene su clave exacta en `zh-Hans.json` (~línea 2735-2758), así que el barrido
  global `TraducirTodosTextosSegunIdioma()` lo cubre correctamente. Conclusión: el handbook está
  completo y funcional en chino; la nota de exclusión era obsoleta o se refería a alcance de trabajo
  futuro, no a deshacer lo ya hecho. No se modificó `HandbookManager.cs`.
- **(4) Bugs reales encontrados en el barrido de textos serializados** — el propio catálogo de
  triage (452 entradas crudas / 308 distintas, ya filtrando coincidencias exactas con el catálogo)
  llevó a auditar a mano los scripts que construyen esas cadenas, encontrando la misma familia de bug
  del Lote 19 (switch o helper local ES/EN/PT sin rama china) en archivos no cubiertos antes:
  - `Assets/Scripts/Campania/Sequitos/SequitoHerboristas.cs`: helper `TraducirLocal` sin 4º parámetro
    de chino; se agregó y se tradujeron los 3 pares nombre/descripción de los ungüentos (Fortaleza,
    Vitalidad, Claridad).
  - `SequitoEsclavos.cs`: `TraducirBotonLiberar()` y `TraducirConsec()` sin rama china.
  - `SequitoClerigos.cs`: dos switches (`txtMecanica`, `txtPlegaria`) sin rama china.
  - `SequitoCuranderos.cs`: switch de `descripcionCuracion` sin rama china.
  - `Nodo.cs`: switch de penalización por sobrecarga de la caravana sin rama china.
  - `Unidad.cs`: `NombreCondenaTextoFlotante()` sin rama china (定罪 = Condena).
  - `Assets/Scripts/Campania/MenuDescanso.cs`: 3 switches sin rama china (mensaje de donación del
    puesto comercial, bendición por descansar en un santuario, sufijo del ritual de purificación
    durante el descanso — este último concatena después de un nombre propio; a diferencia de ES/EN/PT
    que llevan un espacio inicial por la sintaxis occidental, la versión china no lleva espacio
    inicial).
  - `Assets/UIEstadoPersonajeCamp.cs`: helper `TextoPorIdioma(es,en,pt)` usado en 10 sitios para los
    nombres/descripciones de estados de personaje en campaña (Herido, Corrupto, Enfermo, Baja/Alta
    Moral, Fatigado, Bendecido, Avergonzado) y en el label de "Duración restante"; se le agregó un
    4º parámetro opcional `textoZh` (con fallback a inglés si no se pasa) y se tradujeron los 10
    sitios reutilizando terminología ya establecida (坚韧豁免/心智豁免, 攻击/防御, 勇气, 行动点,
    死灵抗性).
  - `Assets/Scripts/AdministradorEscenas.cs` (vía subagente): 13 switches — nombres de categoría de
    facción (Corrupted→腐化者, Undead→不死者, Beasts→野兽, Creatures→生物, Demons→恶魔), 5 nombres de
    buff "Hatred of X", y 3 líneas de flavor text de rasgos de personaje.
  - Se auditaron además, sin encontrar bugs (ya estaban completos): los 17 archivos de `desc` de
    Actividades restantes (subagente, verificación por coincidencia exacta con el catálogo vía script
    Node/`JSON.parse`), `PrePartidaManager.cs` (falso positivo de mi grep inicial — ya usa
    `ChineseLocalization.Get(...)`) y `Assets/OpcionesCargarPlayerPrefsUI.cs` (auditado línea por
    línea a mano: sus 11 switches literales de idioma ya tenían rama china completa, en su mayoría
    vía `ChineseLocalization.Get`).
- **Nota sobre el patrón "hijo por idioma"**: se reconfirmó durante este lote que este patrón
  (contenedor con hijos indexados 0=ES/1=EN/2=PT, sin caso chino explícito, cae a hijo 0) es un
  diseño deliberado y correcto — presente en `HandbookManager.cs`, `Assets/Creditos.cs` (con caso
  chino explícito y comentario aclaratorio) y `OpcionesCargarPlayerPrefsUI.cs`
  (`ObtenerIndicePanelIdioma`) — y no debe "arreglarse" agregándole un caso de chino literal.
- Validación: `compile-check.py` limpio tras cada archivo (directo y vía subagentes).

## Continuación del Lote 21: `CampaignManager.cs`, triage completo de `ES-Campaña.unity`/`ES-Batallas.unity`, y `UIEstadoPersonajeCamp.cs`

- **`Assets/Scripts/Campania/CampaignManager.cs`** (delegado a subagente): se encontraron y
  corrigieron **57 switches/if-chains** sin rama china (bastante más que la estimación inicial de
  ~19, al hacer un barrido más exhaustivo que incluyó variantes `if/else-if` y ternarias además de
  los `switch` de flecha) — bonos de Presagios, eventos de descanso, distancia del Aliento Negro,
  mensajes de asentamiento/santuario/región/muerte/contrato/fatiga según rasgo, tiempos de viaje,
  ritual de purificación, sobrecarga, tooltips de bueyes agitados y clima (Nieve/Niebla), etiquetas
  de nodo descubierto, generador de log de emboscadas, y el mensaje del faro de Serria (incluido un
  switch interno de estados Inspiración/Presteza/Compromiso/Vigilante → 鼓舞/迅捷/尽责/警戒). Los
  nombres de zona 燃烧森林 (zona 1) y 冻风隘口 (zona 2) se verificaron exactos contra el catálogo;
  "Nedukazal" (zona 3) se dejó sin traducir a propósito, como corresponde. `compile-check.py`
  limpio.
- **Auditoría completa de `Assets/UIEstadoPersonajeCamp.cs`**: el helper privado `TextoPorIdioma`
  (usado en 10 sitios para nombres/descripciones de estados de campaña: Herido, Corrupto, Enfermo,
  Baja/Alta Moral, Fatigado, Bendecido, Avergonzado, y la etiqueta "Duración restante") no tenía
  parámetro de chino. Se le agregó un 4º parámetro opcional `textoZh` (con fallback a inglés) y se
  tradujeron los 10 sitios, reutilizando terminología ya establecida (坚韧豁免/心智豁免, 攻击/防御,
  勇气, 行动点, 死灵抗性).
- **Auditoría línea por línea de `Assets/OpcionesCargarPlayerPrefsUI.cs`** (1700+ líneas, no
  revisado exhaustivamente en lotes anteriores): sus 11 switches literales de idioma ya tenían rama
  china completa (la mayoría vía `ChineseLocalization.Get(...)`, incluida la escala de texto, el
  toggle de banter, el limitador de FPS, el volumen de audio y el brillo). Sin cambios necesarios.
- **Bug de código real en `Assets/Scripts/Campania/MenuDescanso.cs` → `AbrirMenuMisiones()`**: el
  switch de idioma solo tenía casos `nIdioma==1` (ES) y `nIdioma==2` (EN); para portugués y chino no
  se ejecutaba ninguna rama, dejando el texto de recompensas (`txtResMision`) con el valor que sea
  que tuviera serializado en el editor (potencialmente incorrecto, no necesariamente español). Se
  agregó una rama explícita para `TRADU.IdiomaChinoSimplificado` que replica el comportamiento de la
  rama española (activa el panel `descEsp`, que sí es traducido por el barrido global de catálogo) y
  arma el texto de recompensas en chino: "+25 希望\n+{suministros} 补给\n+20 材料\n+200 金币\n+1 探路者或潜猎者".
  No se tocó el caso de portugués (fuera de alcance de esta tarea, que es específicamente sobre
  chino).
- **Triage completo de `ES-Campaña.unity` (355 entradas) y `ES-Batallas.unity` (4 entradas)**
  (delegado a subagente, sobre los ~452 textos serializados sin cobertura de catálogo detectados por
  `triage-assets.py`): clasificación en 4 buckets — A) 159 textos de ruido del patrón "hijo por
  idioma" (Ingles/Portu/TEXTOES (1)/etc., nunca visibles para usuarios chinos); B) 47 de boilerplate
  de Unity/placeholders de plantilla; C) 15 confirmados como sobrescritos dinámicamente en runtime
  vía `Traducir()`/`Loc()`/`ChineseLocalization.Get()` ya existentes (sin acción necesaria); D) 138
  reportados como gaps reales.
  - **Verificación posterior de los 138 de bucket D reveló que el subagente comparó varias entradas
    contra el texto crudo `m_text` de la escena (potencialmente obsoleto, reflejando el último
    idioma previsualizado en el editor) en lugar del campo real `textoEspanol` del componente
    `TraduccionRapida`.** Al extraer el valor real de `textoEspanol` directamente del YAML de la
    escena con un script ad-hoc (ver `TraduccionRapida.cs`/`ChineseLocalization.GetDisplayText`) y
    comparar contra el catálogo, se confirmó que los **10 textos de "Character-class previews + Lore
    panel"** (Dislcaimer, Caballero/Purificadora/Explorador/Acechador/Canalizador/Duelista Desc,
    Videoprod, Lore, VictoriaZona) **ya estaban traducidos** — falso positivo del subagente. Mismo
    caso para "Ronda Nueva" (ya en catálogo). Esto es un recordatorio de que `m_text` en escenas con
    el patrón `TraduccionRapida` (ES/EN/PT como campos separados en el Inspector, no como el texto
    visible del componente) no es una fuente confiable para triage — hay que extraer el campo
    `textoEspanol` real.
  - De los gaps genuinamente confirmados (verificados contra el catálogo real antes de traducir), se
    agregaron **47 entradas nuevas** al catálogo `zh-Hans.json` (2755 → 2802), cubriendo: los 12
    pasos + encabezados + banners del tutorial de combate (`TutorialCombate.cs`, patrón de selección
    por índice de hermano, cae a `TEXTOES` para chino), las 3 descripciones de región de
    `TextoInicialManager.cs` (Nedukazal, El Bosque Ardiente, Paso Vientohelado — reutilizando
    内杜卡赞/燃烧之森/冻风隘口 ya establecidos, y acuñando 格兰之森 para "Bosque de Glain", el nombre
    anterior del Bosque Ardiente), el panel de Misiones de Salvamento del menú de descanso
    (narrativa + accesos rápidos), y una decena de textos largos de alta confianza en
    `ES-Campaña.unity` (tooltips de las 6 mejoras de la Ciudad Puerto, tooltip global de Valentía,
    mecánica especial de región del Bosque Ardiente, textos de Santuario/Exploración Pasiva/Rango de
    Visión, resumen de llegada a la ciudad, y el texto de mantenimiento de armas del Herrero). Se
    excluyeron a propósito las etiquetas tipo "Etiqueta - valor" con números ya incrustados en el
    texto (`APMax`, `ValMax`, `Armadura`, `Defensa`, `Iniciativa`, atributos, TS-*, `Estado`,
    `Porcentaje`, etc. del panel `V2-PersonajesInfo` y `BarravalorUI`) porque una clave de catálogo
    con un número fijo nunca calzará con el valor real en runtime — esto requiere encontrar y
    arreglar el script que arma esas cadenas, no agregar entradas de catálogo; **delegado a un
    subagente de investigación, resultado pendiente**.
  - Validación tras las 47 entradas nuevas: 2 pares de etiquetas HTML detectadas como
    descalibradas por `audit.py --validate` (faltaba una etiqueta `<b>` en la traducción china de
    2 textos del tutorial y 1 del panel de misiones) — corregido a mano, revalidado limpio.
    `audit.py --validate` final: 2802 entradas, 0 glifos faltantes, 0 errores. `test-regression.py`:
    13.730 aserciones, PASS. `compile-check.py`: 0 errores.
- **Investigación final: etiquetas de `BarravalorUI` y `V2-PersonajesInfo`** (el subagente delegado
  para esto falló por límite de uso de la sesión; se completó la investigación directamente):
  - **`Assets/Scripts/UI/UIValourGlobalBar.cs` — bug de código real confirmado y corregido.** El
    método `ActualizarVisual()` usaba `bool enIngles = TRADU.i.nIdioma == 2;` como único chequeo de
    idioma: todo lo que no fuera inglés (incluido chino) mostraba las cadenas en español, y como
    estas cadenas se reasignan en cada frame que cambia la Valentía Global (no es texto estático
    cubierto por el barrido de catálogo), agregar entradas de catálogo no habría servido de nada —
    hacía falta tocar el código. Se agregó `bool esChino` y ramas explícitas en `ActualizarVisual`,
    `ObtenerTramoValour` y `ObtenerTextoBonusActualValour`, reutilizando terminología ya establecida
    (全局勇气 para "Valentía Global", 极高/高/中/低/极低 para los tramos, 心智豁免 para "TS Mental",
    犹疑 para "Dudando", 最大行动点 para "Max AP"). De paso se corrigió una inconsistencia terminológica
    propia: la entrada de catálogo del tooltip largo de Valentía Global (agregada en este mismo lote)
    había traducido "Dudando" como 动摇 en vez de 犹疑 (el término ya establecido en otras partes del
    catálogo para ese estado específico) — corregido para que ambos coincidan. `compile-check.py`
    limpio.
  - **`V2-PersonajesInfo` (APMax, ValMax, Armadura, Defensa, Iniciativa, AtributoPoder/Agilidad/Fuerza,
    TS-Reflejos/Fortaleza/Mental) y su hijo `imRasgos/RasgosContainer` — fuera de alcance, no es un
    problema específico de chino.** Se rastreó el controlador de `V2-PersonajesInfo` hasta
    `MenuPersonajes.cs` (mismo script que ya traduce correctamente el panel de personajes "viejo",
    vía las cadenas con dos puntos "Armadura: "/"Valentía: "/etc., todas en el catálogo), pero
    ninguna de las cadenas con guion ("Armadura - 6", "Ap Máximos - 4", etc.) aparece en NINGÚN
    archivo `.cs` del proyecto — no hay ningún script que las arme en tiempo de ejecución. Sumado a
    que el panel contiene un nombre de personaje placeholder ("Claude") y un botón de depuración
    ("DebugSubir Nivel"), todo indica que es una maqueta de UI sin terminar de conectar a la lógica
    del juego, no traducida a NINGÚN idioma (ni siquiera inglés/portugués) — no es una regresión de
    esta tarea de localización a chino, es contenido no cableado. Se deja sin tocar.
  - Con esto se da por **cerrado** el batch completo de las 4 tareas pedidas por el usuario
    ("hace 1, 2, 3 y 4"): flavor text de armas/armaduras, `Actividad_RitualDeLimpieza.cs`, Handbook, y
    el barrido de textos serializados en escenas/prefabs. Validación final: `compile-check.py` (0
    errores), `audit.py --validate` (2802 entradas, 0 glifos faltantes, 0 errores), `test-regression.py`
    (13.730 aserciones, PASS).

## Verificación final de los últimos ítems dudosos del triage (pedido: "seguí con lo que falte")

Se investigó a mano, archivo por archivo (rastreando el fileID de cada GameObject en el YAML de la
escena hasta su script controlador real), cada entrada de "gap real" del bucket D que quedaba sin
resolver o sin confirmar tras el cierre del batch anterior:

- **`ContenedorBatalla/Canvas/txtRonda`** ("Ronda 1"): falso positivo. `BattleManager.cs` línea 1116
  arma el texto con `TRADU.i.Traducir("Ronda") + " " + RondaNro`, y `"Ronda"` ya está en el catálogo
  (轮次). El texto crudo de la escena era solo la vista previa del editor.
- **`MenuBatallas/EmpezarBatalla/txtPodEnemigo` / `txtPodAliado`** ("Poder Enemigo/Aliado: 5"): falso
  positivo. `MenuBatallas.cs` (`FormatearTextoPoderAliado`/`FormatearTextoPoderEnemigo`/
  `FormatearTextoPoderEnemigoOculto`) ya tienen rama china completa vía
  `ChineseLocalization.Get("Poder aliado: {0}", "Ally Power: {0}")` y equivalentes.
- **`MenuCiudadPuerto/Corrupciónicono/Text (TMP) (1)`** ("La Ciudad aún puede esperar a 4 caravanas
  mas antes de partir."): falso positivo. El texto real en runtime (`MenuCiudadPuerto.cs` línea 61) es
  completamente distinto al de la escena — `TRADU.i.Traducir("La ciudad puede permitirse esperar ") +
  N + TRADU.i.Traducir(" caravanas más antes de tener que zarpar.")` — y ambas claves ya están en el
  catálogo (这座城市尚能等待 / 支商队，之后就必须启航。). El texto de la escena era, de nuevo, solo la
  vista previa del editor con una redacción vieja/descartada.
  - **Comentario sobre la fiabilidad del triage automático**: con este listado en tres asciende a
    varios los "gaps reales" reportados por `triage-assets.py` que en verdad ya estaban cubiertos —
    el script de triage compara el texto crudo `m_text` serializado contra el catálogo, pero no puede
    saber si ese `m_text` es el que realmente se ve en juego o solo una vista previa/versión vieja que
    quedó guardada en el editor. Para cualquier auditoría de localización futura, conviene tratar sus
    resultados como candidatos a verificar contra el código real, no como gaps confirmados.
- **`V2-PanelViaje/RelojRota/dia`** ("Día 5 - 09:00"): falso positivo. `CampaignManager.cs` línea 2648
  arma el texto con `TRADU.i.Traducir("Día")`, y `"Día"` ya está en el catálogo (天).
- **`V2-BarraSuperior/CSuministros/txtDescansos`** ("1 Rest"): no se encontró el script que lo arma en
  tiempo de ejecución (a diferencia de los anteriores, no hay ningún `.text =` con esta forma exacta
  en ningún `.cs` del proyecto). Sin embargo, "Descanso"/"Descansar" ya están en el catálogo (休息), y
  el mecanismo de fallback de `TRADU.TraducirChinoSimplificado()` (catálogo → inglés si no hay
  coincidencia) hace improbable que un jugador chino vea texto en español sin traducir. Riesgo residual
  bajo; no se encontró nada concreto para arreglar.
- **`UIAsentamientoDestruido/txtAccionesdisp`** ("3/3 Acciones Disponibles"): confirmado como
  clutter de copy-paste sin usar. `AsentamientoManager.cs` (función que maneja este panel) solo busca
  y setea `TxtDescr`/`TxtAsentamientoArrasado` dentro de `UIAsentamientoDestruido`; nunca hace
  referencia a `txtAccionesdisp` en ese panel. No se toca.
- **`V2-PersonajesInfo`/`imRasgos/RasgosContainer`**: ver hallazgo de la sección anterior (maqueta de
  UI no cableada a ningún idioma, fuera de alcance).

Con esto, **todos los ítems del bucket D del triage de `ES-Campaña.unity`/`ES-Batallas.unity` quedaron
revisados uno por uno** (traducidos cuando eran gaps reales, corregidos en código cuando eran bugs
reales, o descartados con una razón concreta y verificada cuando eran falsos positivos o contenido
fuera de alcance). No queda ningún ítem pendiente de este triage. Validación final repetida:
`compile-check.py` (0 errores), sin cambios de catálogo en esta sub-sección (todo lo encontrado ya
estaba cubierto o era código, no texto).
- **Fuera de alcance de todo este lote** (no tocado a propósito): contenido de Eventos/Presagios
  individuales (`PresagioCatalog.cs` y similares, un sistema de contenido aparte y mucho más extenso),
  y limpieza general de textos placeholder de debug ajenos a la localización.

## Lote 22 (Claude): revisión de código externa — banter, rasgos, mejoras de caravana, logs de combate, y bugs puntuales

El usuario hizo una revisión de código y pruebas manuales propias tras el cierre del Lote 21, y
reportó 5 categorías de problemas reales que el triage automatizado de escenas no había detectado
(todo el contenido de este lote vive en `.cs`, no en objetos de escena, así que quedaba fuera del
alcance de `triage-assets.py`). Se investigó y corrigió cada uno:

- **1) Bug confirmado — `Assets/Scripts/Clases/Explorador/Habilidades/TiroconArco.cs`**: la rama de
  chino de `ActualizarDescripcion()` (agregada en el Lote 20) hacía `return` sin nunca revisar si al
  personaje le quedan flechas, a diferencia de la rama de inglés que sí lo hacía antes de su propio
  `return`. Las ramas de español y portugués tampoco lo hacían (código muerto heredado más abajo en
  el archivo, nunca alcanzado por ningún idioma) — un bug preexistente más amplio, pero se corrigió
  puntualmente la rama china agregándole el mismo chequeo `EsEscenaCampaña()` +
  `ObtenerCantidadFlechas() < 1` que ya tenía inglés, reusando la clave de catálogo existente
  `"No tienes flechas para usar esta habilidad."`.
- **2) Bug confirmado — descripción de "Condena"**: la entrada de catálogo
  `"Condena: En X cantidad de turnos recibirá daño verdadero igual al 10% de su vida máxima por
  turno con el efecto."` estaba traducida de forma ambigua/incorrecta: decía "en cada turno *durante*
  el efecto" (daño recurrente), cuando el código real (`Estados.cs`, `Efecto_Condenado`) aplica **un
  solo golpe** cuando el contador de turnos llega a 0, cuya magnitud es 10% de vida máxima **por cada
  turno consecutivo acumulado** (no un golpe repetido cada turno). Se corrigió la traducción para
  reflejar correctamente que es un único impacto cuya magnitud escala con los turnos acumulados.
- **3) Banter sin chino en absoluto (ambos sistemas)** — confirmado el reporte del usuario de que el
  banter (frases cortas de personajes en batalla y en campaña) no contemplaba chino: ambas clases,
  `BanterLineaLocal` (batalla, `Assets/Scripts/UI/BanterContenidoLocal.cs`) y
  `BanterLineaCampaniaLocal` (campaña, `Assets/Scripts/UI/BanterContenidoCampania.cs`), tenían
  `ObtenerTextoActual()` con casos para inglés y portugués pero ninguno para chino — caía siempre a
  español. Se extendió el formato de datos pipe-delimited (`id|disparador|es|en|pt`) para aceptar un
  6º campo opcional de chino (retrocompatible: 5 campos siguen siendo válidos), y se agregó chino a
  **los 328 renglones existentes** (150 en `BanterContenidoClases.cs`, 6 clases; 178 en
  `BanterContenidoCampania.cs`, 7 bloques de clase + actividades) preservando la voz de personaje
  establecida por clase. Trabajo de traducción delegado a dos subagentes en paralelo con la
  terminología del glosario del proyecto; validado con `compile-check.py` limpio y una verificación
  programática de que cada línea tiene exactamente 6 campos (sin comillas ni `|` sueltos que
  rompieran el formato).
- **4) Rasgos de personaje sin chino en absoluto** — confirmado: `PersonajeTraitDefinition.ObtenerNombre/ObtenerDescripcion`
  (`Assets/Scripts/Campania/Personajes/Personaje.cs`) tenían switch de idioma con casos para inglés y
  portugués pero ninguno para chino, afectando los **86 rasgos** de personaje del juego (nombre +
  descripción cada uno). Se agregó, de forma aditiva y sin tocar los 86 call-sites del constructor
  (que usan `params int[] antagonicos` al final, lo que impedía insertar parámetros opcionales sin
  reordenar todo), un diccionario estático `PersonajeTraitCatalog.traduccionesZh` con las 86
  traducciones y un método `TryGetZh(id, ...)` que `ObtenerNombre`/`ObtenerDescripcion` consultan
  primero para chino (con fallback a inglés si faltara alguno). Se reutilizó terminología ya
  establecida en el catálogo, y notablemente se detectó que dos rasgos ("Odia a Corruptos/Nomuertos/
  Bestias/Criaturas/Demonios") son conceptualmente el mismo buff que `AdministradorEscenas.cs` ya
  traduce como "对X的憎恨" (Hatred of X) — se reusó exactamente esa traducción en vez de acuñar una
  nueva, para no repetir el problema de nombres inconsistentes que el propio usuario reportó (ver
  punto 5). Se acuñó el patrón simétrico "对X的恐惧" para los rasgos "Teme a X" que no tenían
  precedente. `compile-check.py` limpio.
- **5) Bug estructural confirmado — Mejoras de Caravana**: `Assets/Scripts/Campania/MenuCaravana.cs`
  tenía dos helpers centrales, `ActualizarDescripcionMejora` y `ObtenerTextoStats`, cuyo switch de
  idioma solo cubría portugués e inglés (`_ => textoEs`) — afectando la descripción de **las 6
  mejoras de caravana** (Antorchas, Alforjas, Tiendas, Catalejos, Almacén, Defensas), los paneles de
  explicación de Vista Lejana y Exploración Pasiva (con sus tablas de factores dependientes del clima
  /noche/región/presagio), el nombre del clima actual, y las 6 estadísticas de viaje de la bitácora
  (tiempo viajado, batallas libradas, etc.). Se agregó un 4º parámetro opcional `textoZh` a ambos
  helpers y se escribió la variante china de cada uno de los ~15 métodos `Obtener...Es/En/Pt(...)`
  afectados, replicando exactamente su lógica de formato numérico/incremento (no solo la traducción)
  para no introducir bugs de formato. `compile-check.py` limpio.
- **6) Logs de daño y moral sin chino — barrido completo de `Unidad.cs` y `BattleManager.cs`**: se
  encontraron **9 sitios en `Unidad.cs`** y **5 en `BattleManager.cs`** (de un total de 11 y 5
  respectivamente; 2 en `Unidad.cs` resultaron ser texto de depuración deliberadamente en abreviaturas
  de inglés neutro tipo `[base 10 | crit | arm -2 | final 8]`, igual en todos los idiomas, y se
  dejaron sin tocar) con el mismo patrón: un booleano `enIngles`/`esIngles` sin contraparte china,
  construyendo mensajes de log de combate en español para jugadores chinos. Cubre: motivos de ganancia
  ("termina el turno sin gastar PA", "asesta un golpe crítico", "derrota a X") y pérdida ("recibe un
  golpe crítico", "pierde ánimo al ver huir/caer a X", "se acerca a aliado de valentía baja/alta") de
  Valentía; el mensaje central de **daño recibido** (`EscribirLogDanioRecibido`, el log que se ve en
  cada instancia de daño del juego — antes con rama de inglés y español pero ninguna de chino pese a
  ser, literalmente, el log más frecuente de toda partida); ignorar armadura; reflejo de daño
  (espinas); resistencia a estados; experiencia por baja; y los 4 anuncios de Valentía Global por
  ronda (Muy Alta/Alta/Muy Baja/Baja) más los 3 mensajes asociados a fallar la salvación mental
  (Tenaz que se rehúsa a huir, huida avergonzada, Dudando aplicado). Se agregó un helper
  `IdiomaChinoActivo()` en `Unidad.cs` (análogo al `IdiomaInglesActivo()` ya existente) y ramas
  `enChino`/`esChino` puntuales en `BattleManager.cs`. También se detectó que `ObtenerTextoTipoDanioLog`
  (usado por el log de daño) intenta primero el catálogo para el tipo de daño (p. ej. "cortante") y
  solo cae a inglés si no encuentra clave — y ninguno de los 12 tipos de daño en minúscula
  ("cortante", "perforante", "fuego", "hielo", "rayo", "ácido", "arcano", "necrótico", "verdadero",
  "divino", "daño", "contundente") estaba en el catálogo, así que **todo log de daño mostraba el tipo
  de daño en inglés** mezclado con el resto del mensaje en chino; se agregaron las 12 entradas
  reutilizando la terminología ya usada en las descripciones de habilidad (斩击/穿刺/钝击/火焰/冰霜/
  闪电/酸/奥术/死灵/真实/神圣/伤害). `compile-check.py` limpio tras cada archivo.
- **7) Nombres inconsistentes / términos sin enlace a su ayuda — un caso concreto confirmado y
  corregido**: el patrón de coloreado/enlace de términos en `Habilidad.cs`
  (`TerminosColoreadosDescripcion`, la lista de expresiones regulares que detecta términos de juego en
  las descripciones para volverlos clicables con un tooltip) reconocía el daño de Frío/Cold como
  literal chino **"冰冷"**, pero el término ya establecido en el resto del catálogo (registro de daño,
  handbook, ítems) es **"冰霜"** — es decir, cualquier habilidad que mostrara "冰霜伤害" en su
  descripción (la forma correcta y mayoritaria) **no se reconocía ni se volvía clicable/con ayuda**,
  exactamente el síntoma reportado ("términos sin enlace a su ayuda"), y a la inversa, el único lugar
  que usaba el término divergente (`CorteVerticalCongelado.cs`, 4 ocurrencias, más la propia entrada
  `cold-damage` de `CatalogoTerminosJuego.asset` con su tooltip) es que introducía el "nombre
  distinto para el mismo concepto" que el usuario reportó por separado. Se estandarizó todo a "冰霜"
  (el término ya mayoritario): el patrón regex en `Habilidad.cs`, los 4 usos en
  `CorteVerticalCongelado.cs`, y la descripción del tooltip `cold-damage` en
  `CatalogoTerminosJuego.asset`. `compile-check.py` limpio.
  - **Investigación adicional sin hallazgos nuevos**: se auditaron dos sistemas más de "ayuda" para
    buscar más instancias de la categoría "ayudas e instrucciones sin chino" reportada por el
    usuario — `TutorialLocalizationTable`/`TutorialVerticalSlice_Textos.asset` (105 entradas; 90 ya
    cubiertas por el catálogo desde trabajo previo, las 15 restantes resultaron estar completamente
    vacías en español e inglés también — no son un gap de chino sino contenido de diseño sin escribir
    todavía) y `TutorialTooltipCatalog`/`TooltipsTutorial.asset` (15 tooltips de ayuda contextual,
    los 15 ya cubiertos). No se encontraron gaps adicionales en estos dos sistemas puntuales; no se
    descarta que "ayudas e instrucciones" se refiera a otro sistema no identificado — si el usuario
    puede señalar un ejemplo concreto (qué ayuda/instrucción específica vio en español), facilitaría
    encontrar el resto.
  - Una búsqueda automatizada de nombres de habilidad con más de un título chino asignado (comparando
    el `nombre` en español contra el título usado en `ConstruirDescripcionNormalizadaLocalizada`) no
    encontró más inconsistencias entre los ~17 archivos que el patrón de búsqueda pudo capturar de
    forma fiable — dado el tamaño del proyecto (100+ archivos de habilidad), esto no es una garantía
    exhaustiva; el caso de 冰冷/冰霜 se encontró por inspección dirigida del sistema de términos, no
    por este barrido automatizado, así que probablemente haya más instancias sin descubrir de este
    mismo patrón (un término o nombre traducido dos veces de forma independiente en distintos
    archivos).
- Validación final del lote completo: `compile-check.py` (0 errores, Assembly-CSharp y
  Assembly-CSharp-Editor), `audit.py --validate` (2814 entradas, 0 glifos faltantes, 0 errores),
  `test-regression.py` (13.742 aserciones, PASS).

## Continuación del Lote 22: barrido dirigido de "nombres inconsistentes" (pedido explícito: "busca mas")

Tras el hallazgo de 冰冷/冰霜, el usuario pidió específicamente seguir buscando más casos del mismo
patrón. Se abandonó el enfoque de comparar `nombre` (ES) vs título usado en
`ConstruirDescripcionNormalizadaLocalizada` (poco fiable, solo capturó 17 de 100+ archivos) por uno
mucho más preciso: **extraer programáticamente cada llamada a `TerminoDescripcion(TerminoDescripcionId.X,
"texto chino", ...)` en todo `Assets/Scripts`, agrupar por el enum `X`, y señalar cualquier `X` que
reciba más de un texto chino distinto** — ya que ese enum es, por diseño, el identificador canónico
del concepto de juego para el sistema de resaltado/enlace de términos, así que dos textos distintos
para el mismo enum son casi por definición "nombres distintos para el mismo término".

- El barrido encontró **59 `TerminoDescripcionId` usados en 100+ archivos**, de los cuales **16**
  tenían más de un texto chino asignado. Se verificó cada uno a mano contra el texto ES/EN/PT real de
  cada sitio de llamada (no alcanza con mirar el chino solo) para descartar falsos positivos, donde la
  variación es porque el propio texto fuente (ES/EN/PT) también varía según el contexto gramatical
  (p. ej. "Mental" vs "Mental Save", o "Veneno" el estado vs "envenenado" el adjetivo de un objeto) —
  en esos casos el chino sigue correctamente esa misma variación de especificidad y **no es un bug**.
  De los 16 candidatos, **15 resultaron ser falsos positivos verificados uno por uno**
  (`Agilidad`, `Ardiendo`, `Critico`, `Curacion`, `DanioArcano`, `Debuff`, `Energia`, `Evasion`,
  `Fervor`, `Fuerza`, `Oculto`, `PuntosAccion`, `SalvacionFortaleza`, `SalvacionMental`,
  `SalvacionReflejos`, `Veneno` — 16 en total, 15 falsos positivos ya que `DanioAcido` se abordó por
  otra vía, ver abajo).
- **Bug real encontrado y corregido: `acid-damage` (daño Ácido)**, mismo patrón exacto que 冰冷/冰霜.
  El patrón regex de `Habilidad.cs` (`TerminosColoreadosDescripcion`) esperaba literalmente **"酸蚀"**
  para reconocer y enlazar el término de daño Ácido, pero el término realmente usado en las
  descripciones de habilidad (`TerminoDescripcionId.DanioAcido`, p. ej. en
  `TiroconArcoAcido.cs`) es **"酸性"** ("酸性伤害" = daño ácido). Es decir: toda habilidad que
  menciona daño ácido con el término correcto y mayoritario ("酸性") **no se reconocía ni se volvía
  clicable/con ayuda** — el mismo síntoma exacto que Frío/冰冷. Se corrigió:
  - El patrón regex en `Habilidad.cs` (`"酸蚀"` → `"酸性"`).
  - La entrada `acid-damage` de `Assets/Resources/Habilidades/CatalogoTerminosJuego.asset` (el texto
    del tooltip que se muestra al hacer click en el término), que también decía "酸蚀伤害"/"酸蚀" en
    vez de "酸性伤害"/"酸性".
  - **No se tocó** `"酸蚀长弓"` — es el nombre de ítem establecido para "Arco Largo Ácido" (nombre
    propio del arma, ya usado de forma consistente en las 2 entradas del catálogo que lo mencionan);
    los nombres de ítems legítimamente usan un registro más libre/poético que el término mecánico
    puro, igual que "Mandoble Congelado" no necesita decir literalmente "冰霜" en su nombre.
  - `compile-check.py` limpio.
- **Verificaciones adicionales sin hallazgos**: nombres de categoría enemiga (腐化者/不死者/野兽/生物/
  恶魔), y 6 nombres de estado usados vía `TerminoDescripcionId` (Tambaleando=踉跄, Danzando=舞动中,
  Provocado=被嘲讽, Ralentizado=减速, Aterrorizado=惊恐, Adolorido=酸痛) — todos coinciden exactamente
  entre el catálogo JSON y las llamadas `TerminoDescripcion` en las habilidades; términos de
  `TerminosColoreadosDescripcion` para Barrera (屏障) y Debilidad (弱点) — confirmados consistentes
  contra su uso real fuera de las llamadas envueltas.
- **Conclusión honesta sobre cobertura**: este método (agrupar por `TerminoDescripcionId`) es mucho
  más preciso que el intento anterior, pero solo cubre términos que pasan por la función
  `TerminoDescripcion(...)`. Sigue sin cubrir: (a) nombres de habilidad en sí (el título de la
  habilidad, fuera de este sistema de términos), (b) menciones de un término dentro de flavor text de
  ítems/rasgos/banter que no usan `TerminoDescripcion`, y (c) cualquier inconsistencia en archivos
  fuera de `Assets/Scripts` (el propio `CatalogoTerminosJuego.asset`, `zh-Hans.json`) salvo que se
  haya cruzado manualmente como en este lote. Con dos bugs reales confirmados y corregidos (冰冷→冰霜,
  酸蚀→酸性) usando dos métodos de búsqueda distintos, es razonable esperar que puedan existir más
  casos aislados sin descubrir, pero ya no hay una forma automatizada obvia y de bajo esfuerzo para
  seguir escalando esta búsqueda sin revisar manualmente los ~2800 valores del catálogo o los 100+
  archivos de habilidad uno por uno.
- Validación final: `compile-check.py` (0 errores).

## Lote 23 (Claude): pregunta "¿qué faltaría?" — verificación de sistemas asumidos "fuera de alcance" + barrido del sistema de ítems

El usuario preguntó honestamente qué quedaría pendiente. En vez de responder de memoria, se
verificaron dos supuestos de este documento que resultaron **incorrectos**, y se encontró una
categoría de bugs nueva y de alto impacto en el sistema de ítems genérico.

- **Corrección de un supuesto propio: Presagios y Eventos SÍ están completos.** Lotes anteriores
  documentaron `PresagioCatalog.cs` y `EventosAdmin.cs` como "fuera de alcance, sistema de contenido
  aparte, sin auditar". Se verificó directamente: `PresagioCatalog.cs` ya tiene un campo `nombreZh`/
  `descripcionZh` por cada uno de sus ~56 presagios (0 vacíos) con un `switch` que ya incluye
  `TRADU.IdiomaChinoSimplificado`. `EventosAdmin.cs` (5183 líneas, 717 llamados a
  `TRADU.i.Traducir(...)`) tiene el 100% de sus cadenas distintas ya presentes en el catálogo (un
  primer intento de verificación con un script propio dio 241 "faltantes" que resultaron ser
  **corrupción de codificación en mi propio script** — `unicode_escape` mal aplicado sobre bytes UTF-8
  — no un problema real; con el script corregido, cobertura 100%). **Ambos sistemas ya estaban
  completos**, probablemente de una sesión anterior a este historial de conversación. Lección: dos
  falsos "fuera de alcance" en la documentación propia, corregidos.
- **Bug real de alto impacto — clases base de ítems (`Arma.cs`, `Armadura.cs`, `Accesorio.cs`)**:
  estas 3 clases abstractas, heredadas por absolutamente todas las armas, armaduras y accesorios del
  juego, tienen un método compartido (`AgregarStatsArmaaDescripcion`/`...Armadura...`/`...Accesorio...`)
  que arma las líneas de bonificación de estadísticas del ítem (Fuerza, Agilidad, Poder, Iniciativa,
  PA Máx, Valentía Máx, HP Máx, Armadura, Defensa, 3 Salvaciones, 7 Resistencias elementales — 19
  líneas posibles por ítem). Tenía rama ES y rama EN, **ninguna rama de chino ni de portugués**, y
  sin `else` de reserva — para chino, ninguna rama se ejecutaba, así que **las bonificaciones de
  stats desaparecían por completo** de la descripción de cualquier arma/armadura/accesorio con esos
  bonos (no mostraban texto en otro idioma: no mostraban nada). Se agregó la rama china completa a
  las 3 clases, reutilizando terminología ya establecida en todo el proyecto. `compile-check.py`
  limpio.
- **`Assets/Scripts/Equipo/ItemTooltipFormatter.cs`**: el formateador central de tooltips de ítems
  (rareza, tipo de ítem, efectos, debuffs por impacto — usado por todo el sistema de ítems) ya
  enruta todo por `TRADU.i.Traducir(...)`, así que no tenía el mismo bug estructural. Pero se
  encontraron 3 etiquetas nunca agregadas al catálogo en ningún idioma (ni siquiera el propio
  diccionario interno de inglés de `TRADU.cs`, ni la lista de +70 sobrescrituras de inglés de este
  mismo archivo): `"Guantelete"` (categoría de arma, sin traducir — se usó 护手, coherente con los ~8
  nombres de ítem "Guantelete de X" ya traducidos así), `"Resistencia Acido: "` y `"HP Maximo: "`.
  Se agregaron las 3 entradas de catálogo en chino.
- **Barrido sistemático de `Assets/Scripts/Equipo/` y `Assets/Scripts/Consumibles/`** buscando el
  patrón "clase de ítem con texto de sabor hardcodeado sin ninguna rama de idioma" (ni siquiera
  inglés) o "con rama de inglés pero sin la de chino". De 21 candidatos, la gran mayoría fueron
  falsos positivos (rutas seguras por `TRADU.i.Traducir(...)`, como todos los 10 archivos de
  Consumibles y `anillodedestruccion.cs`) o casos ya cubiertos por las clases base recién arregladas
  (`ArmaduraMantoCanalizador.cs`, sin texto de sabor propio). Se encontraron **4 bugs reales**, todos
  con el mismo patrón (texto en español hardcodeado sin ninguna rama de idioma en absoluto, ni
  siquiera inglés — afecta también a jugadores en inglés/portugués, pero se corrigió solo agregando
  la rama de chino, dejando ese problema de alcance más amplio sin tocar):
  - `armaBastonPurificadora.cs` (báculo básico de Purificadora)
  - `armaEspadacortaAcechador.cs` (espada corta básica de Acechador)
  - `armaMandobledeCaballero.cs` (mandoble básico de Caballero)
  - `armaduraCueroReforzadoAcechador.cs` (armadura de cuero reforzado básica de Acechador)
  - Se detectó también `anillodeinteligencia.cs`, cuya descripción es literalmente un placeholder de
    broma del desarrollador ("Este anillo hace piuuum piuuuuuum!") sin traducción a ningún idioma —
    **no se tocó**, ya que no es un gap de traducción sino contenido de diseño sin terminar; escribir
    una descripción "real" en su lugar sería inventar contenido, no traducirlo.
- **Bug real — `Assets/Scripts/Campania/TooltipAtributoHojaPersonaje.cs`**: el sistema de tooltips
  al pasar el mouse sobre cada atributo en la hoja de personaje (Fuerza, Agilidad, Poder, Iniciativa,
  PA, Valentía, Armadura, Defensa, 3 Salvaciones, 7 Resistencias, Vida — 19 tooltips explicativos,
  más un mecanismo de tooltip "personalizado" reutilizable) tenía ramas ES/EN/PT pero ninguna de
  chino. Se agregaron las 19 traducciones y se corrigió el mecanismo de texto personalizado para
  intentar el catálogo antes de caer en español.
- **Otros bugs puntuales encontrados y corregidos** (mismo patrón "falta rama de chino" o "falta el
  idioma por completo"):
  - `Assets/Scripts/Campania/MapaManager.cs`: mensaje de log "la Caravana encuentra un asentamiento"
    sin rama de chino.
  - `Assets/Scripts/Campania/Personajes/btnPersonaje.cs`: etiqueta "Nombre \n Clase Nv.X" sobre el
    retrato de personaje — sin chino, el texto **no se asignaba en absoluto** para chino (quedaba en
    blanco/obsoleto).
  - `Assets/Scripts/SaveSystem/SaveFileData.cs`: etiqueta "Etapa"/"Stage" y nombres de región
    (Bosque Ardiente/Paso Vientohelado/Nedukazal) en las etiquetas de las partidas guardadas del
    menú de carga — sin chino.
- Validación final del lote: `compile-check.py` (0 errores, Assembly-CSharp y
  Assembly-CSharp-Editor), `audit.py --validate` (2817 entradas, 0 glifos faltantes, 0 errores),
  `test-regression.py` (13.745 aserciones, PASS).

### Honestamente, ¿qué quedaría?

- **El sistema de ítems ahora tiene una cobertura mucho más sólida** (clases base arregladas), pero
  con 900+ archivos en el proyecto no se puede garantizar que cada ítem individual (más de 150
  armas/armaduras/accesorios/consumibles catalogados) esté libre de un texto de sabor propio
  hardcodeado sin revisar cada uno a mano — el barrido de `Assets/Scripts/Equipo/` y
  `Assets/Scripts/Consumibles/` fue razonablemente exhaustivo (cubrió los dos directorios completos),
  pero los ítems específicos por clase en `Assets/Scripts/Clases/*/_ItemsExclusivos/` no se
  re-auditaron con este método esta vez (se habían revisado manualmente, uno por uno, en el Lote 20 y
  en "hace 1", así que el riesgo ahí es menor, pero no cero).
  - **Nombres inconsistentes**: quedó pendiente desde el lote anterior — con 2 casos reales
    confirmados (冰冷/冰霜, 酸蚀/酸性) y sin una forma de bajo esfuerzo para seguir escalando la
    búsqueda automatizada, es razonable que existan más.
  - **Otros managers de UI/campaña no revisados todavía** con el mismo patrón "bool esIngles sin
    esChino" — este lote encontró y corrigió instancias en `MapaManager.cs`, `btnPersonaje.cs`,
    `SaveFileData.cs`, `TooltipAtributoHojaPersonaje.cs`, pero el proyecto tiene decenas de archivos
    de UI de campaña (`MenuSequitos.cs`, `MenuBatallas.cs` — que en el Lote 22 mostró
    ingles=16/chino=9, sin revisar en detalle todavía —, y otros) que no se auditaron exhaustivamente
    en este lote.
  - **"Ayudas e instrucciones"**: la queja original del usuario sigue sin un segundo ejemplo
    concreto más allá de lo ya corregido; los dos sistemas de tooltips de tutorial auditados en el
    Lote 22 estaban completos.
- En síntesis: la cobertura es sustancialmente mejor que al principio de esta sesión de revisión de
  código, con varios bugs de alto impacto (stats de ítems invisibles, tooltips de atributos en
  español) genuinamente encontrados y corregidos gracias a la insistencia del usuario en seguir
  buscando — pero dado el tamaño del proyecto, no es razonable afirmar "100% completo y sin ningún
  gap restante" sin una auditoría exhaustiva línea por línea de cada archivo, que no se ha hecho.

## Lote 24 (Claude): bugs encontrados recién al probar un build real de Windows

El usuario compiló un build de Windows y probó el juego de verdad — algo que ningún lote anterior
había podido hacer (todo el trabajo previo se validó con scripts estáticos, nunca jugando). Esto
encontró **3 bugs reales que ningún análisis estático había detectado**, dos de ellos de alto
impacto general (afectan a toda la traducción, no a un texto puntual).

- **1) El chino no se activaba en absoluto en un build de release — causa raíz: `ChineseLocalization.ReleaseReady = false`.**
  En `Assets/Scripts/Localization/ChineseLocalization.cs`:
  ```csharp
  // Habilitar en distribución solamente después de completar traducción y LQA.
  public const bool ReleaseReady = false;
  public static bool IsAvailable => ReleaseReady || Application.isEditor || Debug.isDebugBuild;
  ```
  `TRADU.NormalizarIdioma()` usa este flag: si el chino "no está disponible", lo reemplaza
  **silenciosamente por inglés**. En el Editor o en un Development Build esto se evalúa `true`
  (por eso todo el trabajo de esta sesión se veía bien en Play Mode), pero en un **build de release
  normal**, ninguna de las dos condiciones es cierta y el candado bloqueaba el idioma a propósito.
  El mismo flag también bloqueaba el registro de la fuente CJK dinámica (`ChineseFontFallback.cs`,
  que carga `Assets/Resources/Fuentes/NotoSansCJK/NotoSansCJKsc-Regular.otf`, ya presente en el
  proyecto) y la autodetección de idioma del sistema operativo (`InitialLanguageBootstrap.cs`) — un
  candado consistente en los 3 puntos donde se decide si mostrar chino. El usuario decidió
  explícitamente pasar `ReleaseReady` a `true` (en vez de solo usar Development Build para probar),
  aceptando que la LQA nativa formal sigue sin hacerse. Se actualizó también
  `tools/localization/tests/Program.cs` (el test de bootstrap esperaba el comportamiento viejo de
  caída a inglés fuera del Editor; ahora espera que el chino se mantenga siempre).
- **2) Bug arquitectónico: `TraducirChinoSimplificado` no toleraba variantes de tildes/mojibake
  como sí lo hacen inglés y portugués — causaba que ciertos textos en chino se mostraran en
  *inglés*.** El usuario reportó un log de evento en pantalla: *"The Caravan forced its way through
  the mud and became Sluggish."* apareciendo en **inglés** con el idioma en chino. Investigando:
  el código real (`EventosAdmin.cs`) llama a `Traducir("-La Caravana forzó el paso entre el barro y
  quedó Aletargada.")` — **con tildes** —, pero la clave que existía en el catálogo (heredada de
  cadenas antiguas de `TRADU.cs`) estaba **sin tildes** ("forzo"/"quedo"). Para inglés y portugués,
  `TraducirConCompatibilidadMojibake()` ya reintenta la búsqueda quitando tildes y corrigiendo
  mojibake antes de rendirse — por eso el inglés sí calzó. Para chino, `TraducirChinoSimplificado()`
  intentaba el catálogo **una sola vez** con el texto tal cual y, si fallaba, caía directo al mismo
  mecanismo de repliegue a inglés — sin darle al catálogo chino la misma oportunidad de calzar con
  una variante sin tildes. Se corrigió agregando el mismo reintento (sin tildes, luego mojibake)
  contra el catálogo chino *antes* de rendirse y caer a inglés. **Este es un fix de alto apalancamiento**:
  no se limita a esta cadena puntual, sino que corrige de raíz cualquier clave del catálogo que haya
  sido copiada alguna vez de una cadena vieja/con errores tipográficos de acentuación de `TRADU.cs`
  en vez del texto real y acentuado que usa el código actual — un riesgo que existía silenciosamente
  en un número desconocido de las ~2800 entradas del catálogo agregadas a lo largo de toda la sesión.
  `test-regression.py` sigue en 13.745 aserciones, PASS (el cambio es puramente aditivo: solo agrega
  intentos adicionales para texto que antes fallaba, nunca cambia un resultado que ya era correcto).
- **3) Handbook y descripciones de zona nueva: el texto largo (cuerpo) no se traduce, pero el
  título/botones/pestañas sí.** Dos capturas de pantalla del usuario mostraron el mismo patrón: en
  la pantalla de "Bosque Ardiente" el título "燃烧之森" y el botón "继续" estaban en chino, pero el
  párrafo completo seguía en español; en el Handbook, las pestañas laterales (区域/地图/探索/etc.) y
  el botón "手册" estaban en chino, pero el contenido de la página "Región" seguía en español. Se
  verificó con un script que compara el texto exacto del catálogo contra el texto real serializado
  en la escena (extraído correctamente vía YAML, no con una búsqueda de texto cruda) — **son
  idénticos, carácter por carácter**. La clave sí existe en el catálogo con la traducción correcta.
  El problema no es de traducción sino de **momento en que corre el barrido**: `TRADU.
  TraducirTodosTextosSegunIdioma()` (el barrido global que reemplaza texto de cualquier
  `TextMeshProUGUI` de la escena, incluidos los inactivos) solo se dispara una vez al iniciar sesión
  o cambiar de idioma. Tanto el Handbook (`HandbookManager.AbrirSolapa`, que activa un hijo de
  idioma bajo demanda) como la pantalla de zona nueva (`TextoInicialManager`, instanciada recién al
  llegar a la zona) muestran contenido que puede no haber existido todavía —o no haberse mostrado
  todavía— en el momento de ese barrido único, así que ese texto en español específico nunca pasó
  por él. Se corrigió agregando una llamada explícita a `TRADU.i.TraducirTodosTextosSegunIdioma()`
  (para chino) justo antes de mostrar el contenido en ambos casos — la llamada es segura de repetir
  (usa el texto original cacheado por componente, nunca vuelve a traducir un texto ya traducido).
  - **Nota para el futuro**: este mismo patrón ("hijo por idioma" + contenido mostrado bajo demanda
    después del barrido inicial) existe en otros lugares con el mismo diseño
    (`OpcionesCargarPlayerPrefsUI.cs` panel de Controles, tooltips de `MenuCiudadPuerto.cs`) que no
    se auditaron ni se les aplicó este mismo parche preventivo en este lote — si el usuario encuentra
    texto en español en alguno de esos paneles específicos, es la primera hipótesis a revisar.
- Validación: `compile-check.py` (0 errores), `audit.py --validate` (2817 entradas, 0 glifos
  faltantes, 0 errores), `test-regression.py` (13.745 aserciones, PASS).

## Continuación del Lote 24: 3 capturas más del build real (banner de batalla, texto flotante, tooltip de Valentía Global)

El usuario siguió probando el build y mandó 3 capturas más en la misma sesión de prueba. Los 3
fueron bugs reales, confirmando que probar un build real sigue siendo mucho más efectivo que
cualquier análisis estático para encontrar este tipo de problema.

- **"¡Comienza la batalla!" y otros 3 textos de `BattleIntroController.cs`**: el helper local
  `ObtenerTextoIdioma(string es, string en, string pt)` (usado para el banner de inicio de batalla,
  el texto "ESPACIO: Saltar intro", la etiqueta "UBICA A [nombre]" del despliegue manual, y el
  mensaje "Has emboscado al enemigo") no tenía ningún caso de chino — para chino caía siempre al
  español, sin pasar por el catálogo en absoluto (ni siquiera para "¡Comienza la batalla!", que ya
  tenía traducción en el catálogo — el bug es que este helper nunca lo consulta). Se agregó un 4º
  parámetro opcional `chino` y se tradujeron los 4 usos.
- **Texto flotante "+X VAL" al ganar/perder Valentía en combate**: `Unidad.SumarValentia()` tenía
  `string etiquetaValentia = " VAL";` fijo para **todos** los idiomas (ni siquiera variaba en
  inglés/portugués — es una abreviatura universal en el código original). Se agregó una rama
  específica para chino (`" 勇气"`) sin tocar el comportamiento para los demás idiomas.
- **Tooltip grande de Valentía Global (al pasar el mouse sobre la barra) — mismo bug de timing que
  el Handbook/zona nueva.** `BattleManager.AbrirTooltipValorConDelay()` selecciona entre 3
  GameObjects hijos (`tooltipValorES/EN/PO`) según idioma, sin caso de chino (cae al hijo español).
  El texto del hijo español ya tenía su traducción completa en el catálogo desde el Lote 22 (la
  descripción larga de umbrales de Valentía Global), pero el barrido global corre una sola vez al
  iniciar la partida y este tooltip se muestra bajo demanda (hover, durante el combate) mucho
  después — mismo mecanismo de falla que el Handbook y la pantalla de zona nueva. Se aplicó el mismo
  parche: forzar `TRADU.i.TraducirTodosTextosSegunIdioma()` justo antes de mostrar el tooltip
  seleccionado, cuando el idioma es chino.
- Con este hallazgo repetido (Handbook, zona nueva, y ahora este tooltip), el patrón "hijo por
  idioma + barrido único al inicio + contenido mostrado bajo demanda" se confirma como una fuente
  recurrente de bugs específicamente en chino (los otros idiomas no lo sufren porque sus textos
  vienen hardcodeados directamente en el código, no dependen del barrido). Sigue pendiente auditar
  `OpcionesCargarPlayerPrefsUI.cs` (panel de Controles) y los tooltips de `MenuCiudadPuerto.cs` con
  el mismo criterio, señalados como sospechosos desde el hallazgo anterior.
- Validación: `compile-check.py` (0 errores), `audit.py --validate` (2817 entradas, 0 glifos
  faltantes, 0 errores), `test-regression.py` (13.745 aserciones, PASS).

## Continuación del Lote 24: nombre de partida guardada congelado en el idioma de creación

Pedido del usuario: que el nombre de cada partida guardada se muestre en el idioma elegido
actualmente, no en el idioma que estaba activo cuando se guardó.

- **`SaveFileData.cs`**: `ConstruirDisplayName()` ya arma el nombre correctamente según el idioma
  actual (usa `ObtenerEtiquetaEtapa`/`ObtenerNombreRegion`, ambas ya con chino desde el hallazgo
  anterior). El problema estaba en `AsegurarDisplayName()`: solo generaba el nombre si el campo
  `displayName` venía **vacío**; si la partida ya tenía un nombre guardado (de cuando se creó, en
  cualquier idioma), lo dejaba intacto para siempre, sin importar que el jugador cambiara de idioma
  después. Como esta función se llama cada vez que se lee un archivo de guardado del disco
  (`SaveGameService.Normalizar`, usado tanto al listar partidas en el menú de carga como al cargar
  una) — se quitó la condición para que **siempre** recalcule el nombre con el idioma actual. La
  fecha original de guardado se preserva sin cambios (se lee de `savedAtUtc`, no de la hora actual).
  `displayName` es puramente un campo de visualización autogenerado, nunca editable por el jugador
  (confirmado revisando sus otros usos en `PrePartidaManager.cs`), así que no hay riesgo de pisar un
  nombre personalizado.
- Validación: `compile-check.py` (0 errores), `test-regression.py` (13.745 aserciones, PASS).

## Continuación del Lote 24: el parche de "forzar un nuevo barrido" no alcanzaba — reemplazado por traducción directa

El usuario probó de nuevo la pantalla de "Bosque Ardiente" (con una captura de la jerarquía del
Editor mostrando `DescripcionEspañol`/`Descripcioning`/`Descripcionport` como hijos de "Descripción
BosqueArdiente") y el cuerpo del texto seguía en español pese al parche del punto anterior (forzar
`TRADU.i.TraducirTodosTextosSegunIdioma()` antes de mostrar el contenido).

- **Diagnóstico**: no se pudo confirmar con certeza la causa exacta sin poder ejecutar el juego
  (podría ser que `TRADU.i.nIdioma` todavía no esté establecido correctamente en el momento en que
  corre `Start()` de este componente en particular —posible problema de orden de ejecución de
  scripts—, o que el texto "original" que cachea internamente `TraducirTodosTextosSegunIdioma()`
  para este componente específico haya quedado registrado incorrectamente en algún momento previo).
  En vez de seguir apostando a variantes del mismo mecanismo (que depende de un caché global y de
  cuándo corrió por última vez), se cambió de enfoque a algo que no depende de ese barrido en
  absoluto.
- **Nuevo enfoque, más directo y verificable**: en `TextoInicialManager.cs`, `HandbookManager.cs` y
  el tooltip de Valentía Global en `BattleManager.cs`, en vez de confiar en que el barrido global ya
  tradujo (o volver a forzarlo) el texto del hijo en español que se muestra para chino, se traduce
  **directamente** ese texto con `TRADU.i.Traducir(...)` en el momento de mostrarlo:
  - `TextoInicialManager.cs`: al activar el hijo español (porque no hay hijo chino propio), si el
    idioma es chino se recorren todos los `TextMeshProUGUI` de ese hijo y se traduce cada uno in
    situ. Como este método solo corre una vez por instancia (`Start()`), no hace falta cachear el
    original: el texto en ese momento siempre es el español real, nada más lo toca.
  - `HandbookManager.cs`: como una misma solapa puede abrirse muchas veces en la misma sesión, se
    agregó un diccionario estático (`textosOriginalesHandbook`) que guarda el texto español real de
    cada componente la primera vez que se ve, y traduce siempre a partir de ese original cacheado
    (nunca desde el texto actual, que ya podría estar en chino) — evita traducir dos veces la misma
    cadena en aperturas repetidas de la solapa.
  - `BattleManager.AbrirTooltipValorConDelay()`: mismo patrón con su propio diccionario
    (`textosOriginalesTooltipValor`), aplicado solo cuando el tooltip elegido es el de español (caso
    de reserva para chino).
  - Se quitaron las llamadas a `TraducirTodosTextosSegunIdioma()` agregadas en el intento anterior
    en estos 3 lugares — quedan reemplazadas por este mecanismo local y más directo.
- **Nota**: no se pudo verificar este fix ejecutando el juego (el entorno de esta sesión no permite
  correr el build de Windows del usuario), así que sigue siendo posible que la causa raíz real sea
  otra cosa no contemplada — si el usuario prueba de nuevo y el problema persiste en estas pantallas
  puntuales, sería la señal de que el problema no es de traducción sino de que el `Start()` de este
  componente corre antes de que `TRADU.i` esté listo (un problema de orden de ejecución de scripts
  de Unity, que se resolvería fijando el Script Execution Order de `TRADU.cs` para que corra primero).
- Validación: `compile-check.py` (0 errores), `audit.py --validate` (2817 entradas, 0 glifos
  faltantes, 0 errores), `test-regression.py` (13.745 aserciones, PASS).

## Causa raíz real del bug de "Bosque Ardiente" / Handbook: claves de catálogo con
## saltos de línea inconsistentes (no era timing ni execution order)

Después de que las dos hipótesis anteriores (barrido de traducción diferido, luego traducción
directa) fallaran ambas en un build real de Windows re-testeado por el usuario, se hizo una
extracción exacta del texto en vivo de la escena (parseando el YAML de Unity correctamente con
`yaml.safe_load`, no con substring/grep) y se comparó byte a byte contra la clave del catálogo.

Resultado: la clave del catálogo para la descripción de **Bosque Ardiente** terminaba en
`...atravesarlo.\n\n\n` (tres saltos de línea) mientras que el texto real de la escena termina en
`...atravesarlo.\n\n` (dos saltos de línea). Una única diferencia de caracter de más, arrastrada
desde el proceso de extracción/alta de esta entrada en un lote anterior, hacía que la comparación
exacta de string fallara siempre — sin importar qué mecanismo de traducción se usara (barrido
global o traducción directa), ambos dependen de un `TryTranslate` con igualdad exacta de string.

Se revisaron con el mismo método las tres descripciones de zona y se encontró el mismo defecto en
las tres:

| Zona | Clave vieja (cola) | Clave corregida (cola) |
|---|---|---|
| Bosque Ardiente | `...atravesarlo.\n\n\n` (781 chars) | `...atravesarlo.\n\n` (780 chars) |
| Nedukazal | `...oscuridad.\n\n\n` (752 chars) | `...oscuridad.\n\n` (751 chars) |
| Paso Vientohelado | `...Aliento Negro</b>.\n\n` (796 chars) | `...Aliento Negro</b>.\n` (795 chars) |

Las tres fueron corregidas directamente en `Assets/Resources/Localization/zh-Hans.json`
(preservando el `value` en chino existente, solo se ajustó el `key`). Verificado que las tres
ahora dan `IN CATALOG: True` contra el texto real extraído de `Assets/Scenes/ES-Campaña.unity`.

La página "Región" del Handbook se re-verificó con el mismo método (extracción real del hijo
`Esp_Zonas` bajo `solapaZonas`) y sus 3 textos YA estaban correctamente en el catálogo — el fix
de traducción directa aplicado antes en `Assets/HandbookManager.cs` debería resolver ese caso sin
cambios adicionales de catálogo.

También se encontró y corrigió el mismo tipo de defecto (esta vez `\r\n` vs `\n`, no cantidad de
saltos) en el botón **"Feedback"** del menú principal: el catálogo tenía `"Feedback\r\n"` pero el
texto real en `Assets/Scenes/ES-MenuPrincipal.unity` es `"Feedback\n"` (sin retorno de carro).
Corregido cambiando la clave a `"Feedback\n"`.

Se auditaron además todas las claves del catálogo que contienen un caracter `\r` literal (6 en
total). Tres de ellas (`Continuar\r\n`, `Audio\r\n`, `Vsync\r\n`) tienen un duplicado sin `\r` que
ya funciona correctamente, por lo que son entradas muertas inofensivas (no se tocaron). Las otras
dos (`VERSIÓN DEMO...` y el tooltip de `Valentía Global...`) se verificaron contra el texto real
de sus respectivas escenas y **sí calzan exactamente** — el texto en vivo también contiene `\r\n`
en esos casos, así que no son bugs.

**Lección para el futuro**: cualquier entrada de catálogo agregada a mano o por script para un
bloque de texto largo/multilínea debe verificarse por comparación exacta de string contra el
texto real de la escena (extraído con `yaml.safe_load`, nunca con grep/substring), no solo por
inspección visual del contenido — un solo caracter de espacio en blanco de más o de menos alcanza
para que el catálogo nunca calce, sin ningún error visible en ningún lado.

- Validación tras esta corrección: `compile-check.py` (0 errores), `audit.py --validate` (2817
  entradas, 0 glifos faltantes, 0 errores), `test-regression.py` (13.745 aserciones, PASS).

## Lote 25: typos ortográficos en español (sin tilde) en las etiquetas de stats de items,
## que impedían el enganche con el catálogo chino

El usuario reportó (con capturas de un ítem en tienda) etiquetas de tooltip que se mostraban en
español pese a tener el chino activo: "Danio %: +5%", "Bonus dano arcano: +3",
"Resistencia Necrotica: +1". Estas conviven con otras líneas del MISMO tooltip que sí se veían en
chino ("威力：+1", "奥术抗性：+2", "神圣抗性：+2"), lo cual apuntaba a un problema puntual de esas
etiquetas específicas, no del mecanismo de traducción en general.

**Causa**: en `Assets/Scripts/Equipo/ItemTooltipFormatter.cs`, varias de las etiquetas de stats
usadas como clave de búsqueda en `TRADU.i.Traducir(...)` tenían errores de tipeo en español (con
tildes faltantes o "dano" en vez de "daño"): `"Danio %: "`, `"Danio crit %: "`, `"Bonus dano
fuego/hielo/rayo/acido/arcano/necro/divino: "`, `"Regeneracion vida/armadura: "`, `"Reduccion dano
recibido/critico recibido: "`, `"Espinas dano plano/%: "`, `"Evasion inicial/(directo): "`,
`"Resistencia Necrotica: "`, `"Penetracion armadura: "`, `"Duracion: "`, `"Res Acido"`, `"Acido"`,
`"Jalon"`. El catálogo chino usa las claves con la ortografía correcta (con tilde), así que la
comparación exacta de `Traducir()` nunca calzaba y el texto caía a español (idioma 1, que no pasa
por el catálogo) en vez de a inglés — de ahí que se viera en español y no en inglés como el resto
de los fallbacks.

**Corrección**: se arreglaron todas las cadenas de texto de esas etiquetas en
`ItemTooltipFormatter.cs` (en los call sites de `AgregarLineaStat`/`AgregarModDebuff` y en la
tabla de overrides de inglés, que compara por igualdad exacta de string y por lo tanto también
necesitaba las claves corregidas para no romperse). Se dejó sin tocar `"Resistencia Acido: "` (sin
tilde) porque esa clave puntual YA coincidía así con una entrada existente del catálogo
(`酸性抗性：`) desde un lote anterior — corregirle la tilde la hubiera roto de nuevo.

**Hallazgo adicional al auditar "los items en general" (pedido explícito del usuario)**: al
revisar la cobertura completa de TODAS las etiquetas de este archivo contra el catálogo (no solo
las que tenían typo), aparecieron 42 etiquetas sin ninguna entrada en chino, muchas de ellas sin
ningún error de tipeo — simplemente nunca se habían agregado. La mayoría pertenece al bloque de
`AgregarModDebuff` (usado para describir "debuffs por impacto" de armas/consumibles: `Res Fuego`,
`Res Hielo`, `Res Rayo`, `Res Ácido`, `Res Arcano`, `Res Necro`, `Res Divino`, `Congelado`,
`Ácido`, `Empuje`, `Jalón`, `AP por turno`, `Resistencias reducidas`, `Ignora armadura`, `Robo
vida`, `Crit dado`, `Daño Crítico %`, `Fuerza`, `Agilidad`, `Daño %`, `TS Reflejos`, `TS
Fortaleza`, `TS Mental`), que nunca había sido cubierto en lotes anteriores (esos sí se enfocaron
en `AgregarLineaStat`, usado para buffs). Se agregaron las 42 entradas a
`Assets/Resources/Localization/zh-Hans.json`, reusando la terminología ya establecida en el resto
del proyecto para los mismos conceptos (抗性/伤害/恢复/豁免/闪避/穿透/持续时间, etc.) para mantener
consistencia entre pantallas.

- Validación: `compile-check.py` (0 errores), `audit.py --validate` (2859 entradas, 0 glifos
  faltantes, 0 errores), `test-regression.py` (13.787 aserciones, PASS).
