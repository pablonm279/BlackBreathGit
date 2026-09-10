# Purificadora, apariencia 1

Implementada mediante `Assets/Resources/AnimacionesIlustradas/PurificadoraApariencia1.asset` y sus tres PNG en la carpeta homonima. Generacion con la herramienta integrada image_gen; prompts exactos en [prompts.md](prompts.md). Fuentes y descartes se conservan aqui, fuera de Assets.

Reposo nuevo con respiracion, anticipacion e impacto de baculo. Se conservan atento, movimiento/dash, habilidades originales, feedback de dano sin sprite nuevo y respiracion al 80% fuera del turno propio. La segunda apariencia queda excluida por sus referencias de mover/atacar. No se modificaron scripts C#, prefabs, escenas, tiempos de combate ni preview HTML.

Preparacion reproducible: `tools/animation-pilot/prepare_purificadora_apariencia1.py`. Lienzo RGBA 1024 x 768, apoyo (0.5, 8/768), correccion horizontal 1024/750 y desplazamiento -0.01. Escalas calibradas por cuerpo: reposo 0.583, anticipacion 0.60, impacto 0.55; registro de pies fuente x=600/670/650. Registro detallado en [registro.json](registro.json).

La anticipacion ya tenia alfa real y se preservo. Reposo e impacto tenian cuadricula dibujada: se corrigio el fondo mediante image_gen a magenta uniforme y se extrajo por cromaticidad, reconstruyendo color y cobertura del borde desde el interior cercano. No se aplico umbral de blanco sobre pelo o ropa.

Verificado: tres PNG RGBA, canal alfa 0..255, margenes transparentes, GUID de origen presentes en prefab, referencias nuevas validas, importacion Sprite Single FullRect sin mipmaps ni compresion, bilineal/Clamp, diez perfiles con parejas de origen unicas. Revision visual en [revision.png](revision.png) sobre fondos oscuro/claro/verde y detalle de contornos en [bordes.png](bordes.png).

Pendiente: comprobacion real en Play Mode de transiciones, ataque, habilidades, ambas orientaciones, pausa/estados/muerte y tutorial. No hubo cambios de logica compartida; el aspecto aun debe confirmarse en batalla. Salir y volver a entrar en Play Mode para renovar la cache de perfiles.
