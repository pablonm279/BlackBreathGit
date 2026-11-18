# Acantilado frontal del Paso

- Piedra más continua: cobertura calculada con altura y dos proyecciones, con variación suave y contraste de pizarra reducido. El suelo plano conserva nieve y tundra.
- Cornisa redondeada mediante una transición de 1.6 m en la caída sur existente. Conserva altura y posición general del acantilado. La pared plana permanece desactivada.
- Adornos comunes: separación de 2 m respecto de la cornisa. Los adornos volumétricos adicionales suman su radio a ese margen.
- Niebla de guerra: opacidad multiplicada por 0.91. Máscaras de visión e historial intactas.
- Montañas: brillo multiplicado por 0.90 en la función compartida del cielo, para conservar la coincidencia de las transiciones.

Validación en Unity: shaders sin errores; 1025 adornos generados comprobados, ninguno dentro del margen; ningún sprite invadiendo la cornisa. El nodo más cercano quedó a 2.35 m del borde, fuera de la franja redondeada. Comparación visual de día y noche; la captura nocturna incluye clima de niebla.

Prueba GPU de opacidad: proporción nueva/anterior 0.9100001. Dependencias runtime sin errores.

Tutorial: no se recorrió completo. Los cambios de relieve y exclusión están limitados a zona 2; no se modificaron nodos, costes, movimiento, secuencias ni textos. Unity quedó fuera de Play. Los respaldos contienen el estado anterior de cada archivo editado en esta pasada.
