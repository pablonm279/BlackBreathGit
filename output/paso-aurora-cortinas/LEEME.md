# Aurora: cortinas y presencia diurna

Se conserva la ubicación y el sistema de partículas existente. Sus materiales de runtime usan ahora un shader propio con silueta curva, extremos difusos, filamentos internos, variación verde/celeste y oscilaciones lentas con fases diferentes. Se mantiene la mezcla original y el contacto suave con profundidad. No se modifican los materiales fuente.

La opacidad diurna pasa de factor 0,28 a 0,46. La máscara nueva compensa la pérdida de luminosidad de sus bordes suaves. El reflejo de nieve diurno pasa de 0,010 a 0,026; la intensidad nocturna del reflejo se conserva en 0,035. Las luces originales mantienen el ajuste nocturno y ganan un poco de presencia diurna.

Validado en Unity mediante MCP: shader compilado, capturas reales de día/noche y dos instantes del movimiento. Hora y clima reales conservados; resultados en validacion.txt. Consola sin errores al finalizar. Vistas previas retiradas y Play detenido, sin guardar escenas ni partidas. Revisión de diff sin errores de espacios.

Alcance limitado al controlador del Paso (ID 2) y su shader de terreno. No se modifican visión, nodos, rutas ni secuencias del tutorial; el tutorial completo no se recorrió en esta prueba visual.
