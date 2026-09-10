# Campo vivo: rework visual del combate

Implementado el 9 de septiembre de 2026. Está activado por defecto y entra automáticamente al cargar un encuentro.

## Qué cambia

- Atmósfera en dos planos de profundidad, con ráfagas y remolinos suaves. El bosque quemado tiene ceniza y brasas; el bosque normal, motas vegetales; el paso helado, nieve; Nedukazal, polvo cálido; el subsuelo, partículas minerales y bruma fría.
- Bancos de bruma baja que cruzan el campo y variaciones lentas de luz y sombra sobre el suelo. La noche enfría la paleta.
- El dash levanta polvo al arrancar, deja una estela breve y se asienta al llegar. Se aplica desde un observador común a aliados y enemigos, sin agregar componentes a cada unidad.
- El polvo del movimiento es un 40% más pequeño y más oscuro que la versión de arranque reforzada. Al iniciar la aproximación melee salen dos bocanadas aproximadamente un 15% mayores y un 30% más rápidas que en el dash normal. El regreso del melee no genera ese impulso.
- Recibir golpes no genera polvo ni humo. Se conservan la onda breve sobre el terreno y las pequeñas esquirlas de críticos y muertes, sin marcas permanentes.
- El plano y la escala se calculan con las casillas del encuentro, incluida la inclinación del tablero. No hay coordenadas de una zona concreta codificadas en los efectos.

## Comparar y desactivar

En Unity: **Tools > Combate > Campo vivo > Seleccionar ajustes (reversible)**.

Se selecciona `Assets/Resources/CampoVivo/AjustesCampoVivo.asset`. Desmarcar **Activo** y guardar el proyecto desactiva todo el rework, también en el próximo build. Vuelven automáticamente las partículas ambientales anteriores. Los efectos de combate que ya existían antes de este trabajo se conservan.

Para comparar durante una batalla, sin cambiar el valor guardado:

- **Tools > Combate > Campo vivo > Comparar - aspecto anterior (Play)**.
- **Tools > Combate > Campo vivo > Comparar - campo vivo (Play)**.
- **Usar ajustes del asset (Play)** elimina la comparación temporal. Entrar nuevamente en Play también la reinicia.

El asset permite activar por separado atmósfera, suelo, desplazamientos e impactos; además tiene intensidad y densidad. La densidad se reduce en las calidades gráficas bajas. El interruptor preexistente `BattleVisualJuice.Enabled` también desactiva esta capa.

## Alcance y protección del tutorial

Los archivos existentes editados son `Assets/Scripts/Visual/BattleAmbientLife.cs`, `Assets/Scripts/Visual/BattleVisualJuice.cs` y `Assets/Scripts/Habilidades/MeleeApproachMover.cs`: llamadas de integración para configurar el ambiente, recuperar el anterior al apagar la capa y emitir polvo al inicio de la aproximación melee. No cambian los tiempos de movimiento o de impacto.

El resto vive en `Assets/Scripts/Visual/CampoVivo/`, `Assets/Resources/CampoVivo/` y `Assets/Editor/ValidacionCampoVivo.cs`. No se editaron escenas, prefabs, sprites, poses, habilidades, balance, UI ni traducciones.

El sistema no mueve las unidades ni la cámara, no escribe HP/AP, no consume `UnityEngine.Random` y no modifica `Time.timeScale`. Respeta la pausa de los tooltips del tutorial. Omite rastros para muertos, unidades ocultas, inmóviles, etéreas y voladoras. Los efectos nuevos no contienen colliders ni elementos de UI que puedan interceptar clics.

La simulación usa cinco sistemas de partículas con límites fijos (394 partículas como techo conjunto) y doce ondas reutilizables. El presupuesto limita la emisión a 44 partículas por fotograma, compartido entre ambiente e impactos. No se crean objetos por cada golpe.

## Validación

`ValidacionCampoVivo.Renderizar` abre la escena real de campaña en memoria, monta las unidades de prueba y utiliza el shader y los componentes reales de Unity. No guarda la escena ni ejecuta una partida. Genera comparaciones en `output/campo-vivo/capturas/` y comprobaciones en `output/campo-vivo/validacion.txt`.

La comprobación cubre las cinco variantes del enum de zona, emisión real, conservación del RNG/HP/AP/cámara/posiciones, pausa del tutorial, reanudación, arranque y llegada del dash, voladores, enemigos ocultos, críticos, caducidad y límites de efectos, noche/día, reconfiguración y restauración del ambiente anterior. La limpieza se verifica invocando el método real de destrucción, porque fuera de Play Unity no ejecuta ese callback del componente.

**Pendiente de comprobación manual:** recorrer el tutorial completo y una partida larga en Play Mode. Los renders y las pruebas de componentes no certifican sus secuencias interactivas completas. El principal aspecto visual pendiente es valorar densidad y legibilidad junto a todos los efectos de habilidades y UI de una partida real.

Para repetir en una instancia cerrada del proyecto:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe' -batchmode -projectPath 'C:/UnityProyectosNOborrar/GDD - Untitled' -executeMethod ValidacionCampoVivo.Renderizar -quit -logFile 'output/campo-vivo/validacion-unity.log'
```

Agregar `-campoSoloValidacion` omite los renders. Las capturas se hacen en Editor con el estado escénico de prueba; no son capturas de una partida completa ni del tutorial.

## Copia previa y reversión del código

`output/campo-vivo/antes/` conserva ambos scripts tal como estaban al iniciar el trabajo. `output/campo-vivo/integracion.patch` contiene exclusivamente las llamadas de integración nuevas. `output/campo-vivo/cambios-previos.patch` y `estado-inicial.txt` registran los cambios que ya había antes; no deben aplicarse para desactivar este rework.

Para volver visualmente atrás basta con desmarcar **Activo**. Si se decide retirar también el código, revertir sólo `integracion.patch` y luego retirar las dos carpetas nuevas `CampoVivo` con sus metas y el editor `ValidacionCampoVivo.cs` con su meta. Revisar primero que no tengan modificaciones posteriores. No usar una restauración general del repositorio: había cambios previos de arte, animaciones y combate que se preservaron.
