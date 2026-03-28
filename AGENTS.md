# AGENTS.md

## Proyecto

- Motor: Unity `2023.2.22f1`
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
- Mojibake/encoding: `powershell -ExecutionPolicy Bypass -File tools/check-mojibake.ps1 -Root Assets`
- Dependencias prohibidas en runtime: `powershell -ExecutionPolicy Bypass -File tools/check-runtime-deps.ps1`
- Archivos modificados: `git status --short`
- Buscar scripts: `rg --files Assets/Scripts`
- Buscar referencias: `rg -n "NombreSimbolo" Assets/Scripts Assets/Prefabs Assets/Scenes`

## Git hooks y encoding

- Hay un hook en `.githooks/pre-commit` que bloquea commits con UTF-8 invalido o mojibake.
- `.editorconfig` exige `charset = utf-8`, `insert_final_newline = true` y trim de whitespace para la mayoria de archivos.

## Convenciones practicas para automatizacion

- Priorizar cambios en C# antes que ediciones manuales sobre prefabs o escenas.
- Cuando un cambio afecte UI o contenido serializado, validar primero el script fuente relacionado.
- Si aparece una referencia duplicada entre `Assets/...` y `GDD - Untitled/Assets/...`, tomar `Assets/...` como fuente primaria salvo evidencia en contra.
