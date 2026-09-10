# Purificadora, apariencia 2

Integrada en `Assets/Resources/AnimacionesIlustradas/PurificadoraApariencia2.asset`, con tres sprites en la carpeta homonima: reposo, anticipacion e impacto. Fuentes generadas mediante image_gen integrado; prompts exactos en [prompts.md](prompts.md). El primer intento fallo por error HTTP 500; el reintento entrego las tres poses.

Identidad conservada: pelo rubio recogido, tunica marfil con ribetes negros, frascos, cadenas, bolso y baculo de aro solar. Se conservan atento, dash/movimiento y habilidades originales, feedback de dano sin sprite nuevo y respiracion al 80% fuera del turno propio. Sin cambios de C#, prefabs, escenas, combate o preview HTML. La apariencia 1 tiene otro perfil y referencias originales distintas.

Preparacion reproducible: `tools/animation-pilot/prepare_purificadora_apariencia2.py`. Las fuentes RGB tenian cuadricula dibujada; se sustituyo el fondo por magenta mediante image_gen y se extrajo por cromaticidad con reconstruccion de color/cobertura en los bordes, conservando ropa y pelo claros. Descartes fuera de Assets.

Calibracion por cuerpo y pies: PNG RGBA 1024 x 768; apoyo (0.5, 8/768), correccion horizontal 1024/750, desplazamiento 0. Escalas reposo/anticipacion/impacto: 0.515/0.60/0.565; pies fuente x=470/640/650. Detalles en [registro.json](registro.json).

Verificado: alfa 0..255, margenes transparentes, GUID originales presentes en prefab, referencias nuevas validas, Sprite Single FullRect sin mipmaps ni compresion, bilineal y Clamp, once perfiles con parejas de origen unicas. Inspeccion visual sobre fondos oscuro/claro/verde en [revision.png](revision.png) y contornos ampliados en [bordes.png](bordes.png).

Pendiente de comprobar en Play Mode: transiciones idle/atento, dash, ataque, habilidades, orientaciones, pausa, estados, muerte y tutorial. No se cambio logica compartida; debe confirmarse el aspecto y la continuidad en batalla. Salir y volver a entrar en Play Mode para renovar la cache de perfiles.
