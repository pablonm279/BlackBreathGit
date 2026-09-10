# Explorador, apariencia 2

Integrado en `Assets/Resources/AnimacionesIlustradas/ExploradorApariencia2.asset` con cuatro sprites en la carpeta homonima: reposo, anticipacion, impacto y preparacion. Generacion mediante image_gen integrado; [prompts exactos](prompts.md), fuentes y descartes conservados fuera de Assets.

El usuario detecto un arco deformado y un agarre incorrecto en el primer reposo. Se corrigieron manos, munecas y geometria del arco usando el idle original como referencia: mano adelantada sujeta la empunadura central, otra mano sostiene una flecha y el arco tiene una curva continua. El archivo `reposo-cuadricula.png` es el descarte, no el sprite importado.

Se conserva la identidad de esta apariencia: pelo corto, barba, armadura de cuero marron, mangas burdeos y bufanda gris. Reposo con respiracion (80% fuera del turno), ataque de daga con anticipacion/impacto, y cuclillas compartida por Improvisar Flechas y Acechar. Atento, movimiento/dash, disparo de arco y otras habilidades usan sus sprites originales. Sin pose nueva de dano ni caminata.

No se cambio C# en esta tarea: el campo opcional `preparacion` reutiliza la integracion de Explorador 1. Se muestra al usar esas dos habilidades, durante el tiempo de pose configurado; no queda sostenido todo el buff. No se editaron prefabs, escenas, logica, tiempos ni preview HTML.

Preparador: `tools/animation-pilot/prepare_explorador_apariencia2.py`. RGBA 1024 x 768, apoyo (0.5, 20/768), desplazamiento 0.03, correccion horizontal 1024/750. Escalas reposo/anticipacion/impacto/preparacion: 0.58/0.59/0.62/0.46. Pies fuente x: 640/770/810/720. Cuclillas calibrada por cabeza y torso. [Registro](registro.json).

Las fuentes RGB tenian cuadricula: se sustituyo por magenta uniforme y se extrajo con recuperacion del antialias, limitando la reconstruccion de color a pixeles mezclados para conservar la cuerda fina del arco.

Validacion ejecutada con [verificar.py](verificar.py): cuatro PNG RGBA, alfa 0..255, margenes transparentes, configuracion Sprite Single FullRect sin compresion/mipmaps, bilineal/Clamp, GUID validos y trece perfiles con parejas de origen unicas. Inspeccion de escala/manos/arco/contornos sobre fondos oscuro/claro/verde: [revision.png](revision.png), [bordes.png](bordes.png).

Pendiente: prueba real en batalla y tutorial de transiciones, habilidades, orientaciones, pausa/estados/muerte. No se modifico logica compartida en esta tarea. Salir y volver a entrar en Play Mode para renovar la cache de perfiles.
