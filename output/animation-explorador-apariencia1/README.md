# Explorador, apariencia 1

Integrado en `Assets/Resources/AnimacionesIlustradas/ExploradorApariencia1.asset` con cuatro PNG en la carpeta homonima. Generacion: image_gen integrado; [prompts exactos](prompts.md), fuentes y descartes en esta carpeta.

- Reposo nuevo, respiracion sutil y velocidad al 80% fuera del turno propio.
- Daga: anticipacion e impacto nuevos, con timing melee existente. El arco usa su sprite original de habilidad; no se reemplaza por la daga.
- Cuclillas: `preparacion.png`, compartida exclusivamente por Improvisar Flechas y Acechar. Se usa al confirmar la habilidad, durante la duracion de pose configurada (1.35 s), con interrupciones normales del controlador. No se sostiene durante todo el buff.
- Atento, dash, feedback de dano y otras habilidades conservados. Sin sprite nuevo de dano o caminata.

`PerfilAnimacionIlustrada.preparacion` es opcional. `UnidadPoseController.PlayPreparationPose()` aplica una sustitucion temporal sin modificar `poseHabilidad`. Los refrescos por objetivo/turno la conservan, la reversion vuelve a atento/idle y el cambio de apariencia la descarta. Los holds existentes tienen prioridad. `Habilidad.Resolver` solicita esta pose solo para las dos habilidades indicadas; sin sprite opcional conserva el comportamiento anterior, incluso la omision de animacion de Improvisar Flechas. No se cambiaron efectos, AP, esperas, fin de turno, prefabs o escenas.

Preparador: `tools/animation-pilot/prepare_explorador_apariencia1.py`. PNG RGBA 1024 x 768, apoyo (0.5, 20/768), desplazamiento 0.03, correccion horizontal 1024/750. Escalas reposo/anticipacion/impacto/preparacion: 0.585/0.63/0.64/0.48. Pies fuente x: 640/755/800/680. Cuclillas calibrada por cabeza/torso, no por altura total. [Registro](registro.json).

Reposo, impacto y preparacion tenian cuadricula RGB: fondo corregido a magenta y extraido por cromaticidad, con descontaminacion restringida a pixeles mezclados para preservar la cuerda fina del arco. Anticipacion conserva alfa real, quitando solo ruido aislado fuera de la silueta.

Verificado:

- Compilacion runtime y editor, salida 0. Logs: `outputs/animation-pilot/compilacion-explorador.log` y `compilacion-editor-explorador.log`.
- 14 comprobaciones del controlador real con dobles de Unity: activacion, refresco por objetivo, expiracion a atento, idle, arco original, movimiento, holds, cambio de apariencia y fallback. Codigo y copia exacta bajo prueba en esta carpeta; no son pruebas en Play Mode.
- Cuatro PNG RGBA, alfa 0..255, margenes, importacion y GUID correctos; doce perfiles con parejas de origen distintas. Repetible con `verificar.py`.
- Revision sobre fondos oscuro/claro/verde y detalle: [revision.png](revision.png), [bordes.png](bordes.png). Preview HTML sin cambios.

Pendiente: probar en batalla ambas habilidades, transiciones, orientaciones, pausa/estados/muerte y tutorial. Se modifico el controlador compartido con un campo opcional y una rama limitada a estas habilidades; las pruebas aisladas cubren restauracion y fallback, pero no verifican secuencias completas del tutorial. Salir y volver a entrar en Play Mode para renovar la cache de perfiles.
