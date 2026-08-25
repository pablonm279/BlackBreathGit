# AGENTS.md

## Proyecto

- Motor: Unity `6000.3.20f1`
- Solucion principal: `GDD - Untitled.sln`
- Scripts de juego: `Assets/Scripts`
- Ensamblados: no hay `asmdef` propios en `Assets`; el codigo compila en `Assembly-CSharp`

## Escenas principales

- `Assets/Scenes/ES-MenuPrincipal.unity`
- `Assets/Scenes/ES-Campaña.unity`
- `Assets/Scenes/ES-Batallas.unity` (deshabilitada en `EditorBuildSettings.asset`)

## Mapa rapido

- `Assets/Scripts/Campania`: flujo y UI de campaña
- `Assets/Scripts/Clases`: clases jugables y contenido asociado
- `Assets/Scripts/Habilidades`: habilidades compartidas, IA y trampas
- `Assets/Scripts/UI`: widgets y efectos de interfaz de batalla
- `Assets/Prefabs`: prefabs principales
- `Assets/Scenes`: escenas del proyecto
- `Assets/Resources`: assets cargados por ruta
- `Assets/Editor`: tooling de editor
- `tools`: scripts utilitarios para chequeos locales

## Alertas del repo

- El worktree puede estar sucio con frecuencia. Revisar `git status --short` antes de editar.
- Existe una carpeta anidada `GDD - Untitled/` con una copia parcial vieja del proyecto. Evitar editar ahi salvo pedido explicito.
- Hay mucho contenido serializado de Unity (`.prefab`, `.unity`, `.asset`). No tocarlo si la tarea puede resolverse solo en C#.
- El proyecto usa nombres y textos en espanol. Mantener UTF-8 y revisar acentos al editar.

## Chequeos utiles

- Resumen del proyecto: `powershell -ExecutionPolicy Bypass -File tools/project-summary.ps1`
- Mojibake/encoding (solo al preparar o realizar un commit): `powershell -ExecutionPolicy Bypass -File tools/check-mojibake.ps1 -Root Assets`
- Dependencias prohibidas en runtime: `powershell -ExecutionPolicy Bypass -File tools/check-runtime-deps.ps1`
- Archivos modificados: `git status --short`
- Buscar scripts: `rg --files Assets/Scripts`
- Buscar referencias: `rg -n "NombreSimbolo" Assets/Scripts Assets/Prefabs Assets/Scenes`

## Git hooks y encoding

- Hay un hook en `.githooks/pre-commit` que bloquea commits con UTF-8 invalido o mojibake.
- El chequeo manual de mojibake/encoding queda suspendido durante tareas normales. Ejecutarlo unicamente al preparar o realizar un commit.
- `.editorconfig` exige `charset = utf-8`, `insert_final_newline = true` y trim de whitespace para la mayoria de archivos.

## Convenciones practicas para automatizacion

- Priorizar cambios en C# antes que ediciones manuales sobre prefabs o escenas.
- Cuando un cambio afecte UI o contenido serializado, validar primero el script fuente relacionado.
- Si aparece una referencia duplicada entre `Assets/...` y `GDD - Untitled/Assets/...`, tomar `Assets/...` como fuente primaria salvo evidencia en contra.
- `Assets/TRADU.cs` queda off-limits salvo pedido explicito del usuario. Si una solucion requiere tocar traducciones, avisarlo primero en chat con clave original + propuesta en ingles + propuesta en portugues BR, y esperar confirmacion antes de editar.

## Flujo para cambios chicos

- Trabajar en modo clinico: tocar lo minimo indispensable para resolver el pedido.
- Para pedidos puntuales y chicos, aplicar parche minimo sobre el bloque exacto.
- No hacer refactors, limpiezas, renombres, reordenamientos ni mover codigo salvo pedido explicito.
- Si el archivo ya tiene cambios previos, tocar solo las lineas necesarias para la tarea. Evitar reescrituras amplias.
- Si la tarea queda resuelta quitando una llamada, condicion o valor puntual, preferir eso antes que una limpieza estructural.
- Despues de editar, revisar el cambio puntual con `git diff -- <archivo>`.
- Si existe una validacion rapida aplicable, correr solo una verificacion acotada antes de cerrar.

## Proteccion del tutorial

- El tutorial actualmente funciona y debe tratarse como un flujo protegido del proyecto.
- Tener siempre presente su integridad al analizar, implementar y validar cualquier cambio.
- Ante cada cambio grande, revisar explicitamente que no rompa el tutorial ni sus dependencias, estados, secuencias o UI asociada.
- Si no es posible validar el tutorial de forma completa, indicar con claridad que aspecto quedo sin comprobar y el riesgo potencial antes de cerrar la tarea.
