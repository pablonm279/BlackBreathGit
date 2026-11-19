# Capa de microrelieve sin placa

Pedido del usuario: mostrar el suelo del fondo y agregar solo una capa semitransparente de textura/relieve que conserve ese color.

Se modifico exclusivamente Assets/Resources/TablerosIntegrados/Suelo.shader. La textura existente se lee como microaltura por su luminancia; cuatro muestras reconstruyen una normal local. No se dibuja su albedo ni se usa el RGB del tinte ambiental. La mezcla multiplicativa modula el color ya dibujado debajo y conserva su tono. La intensidad inicial de la mezcla es 0.20 y las variaciones quedan limitadas a sombras/luces sutiles. El desvanecimiento es amplio e irregular, sin borde, nieve pintada ni marco exterior.

La misma solucion se aplica a Paso, Bosque y Nedukazal. Los tres fondos del Paso usan la misma textura de microaltura. No se crearon imagenes nuevas ni se modificaron fondos, escenas, casillas, unidades, scripts de combate o traducciones. No se cambiaron los ajustes globales de calidad.

Validacion en Unity: shader compatible sin errores y 186 comprobaciones correctas en una escena aislada. Se preservan posiciones, costes, ocupacion, colliders, vida, graficos originales al desactivar la capa, RNG y reloj. La secuencia completa del tutorial no se recorrio; no se modifico su codigo ni sus entradas.

Capturas de prueba del render real, con la camara y assets del proyecto, sin UI completa ni posprocesado de una partida:

- paso-fondo-1.png: canon nevado.
- paso-fondo-2.png: paredes de hielo.
- paso-fondo-3.png: valle nevado.
- bosque-fondo-1.png.
- nedukazal-fondo-1.png.

El cambio se importa como shader y no requiere reiniciar Play ni recompilar scripts. La intensidad se puede ajustar en _Opacidad y _FuerzaRelieve del material.
