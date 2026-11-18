# Acantilado: línea horizontal, detalles y margen

El corte blanco provenía del borde nevado superior de la textura de pizarra, repetido verticalmente. El shader ahora muestrea el interior de la imagen con repetición reflejada continua y deformación leve, evitando ese borde. La proyección también conserva las coordenadas originales antes del empuje de profundidad de la extensión.

Se agregaron fisuras finas y discontinuas, tonos terrosos suaves y variaciones pequeñas de grano y relieve de material. Estos detalles se limitan a las caras del acantilado frontal. No se agregaron mallas ni se modificaron colliders.

El margen de decoración pasó de 2 a 3 metros; los adornos volumétricos siguen sumando su radio. Validación sobre el mapa generado: 1028 adornos comprobados, ninguno en la franja prohibida; un punto a 2.9 m fue rechazado.

Cambios finales: `Assets/MapDecorator.cs` y `Assets/Resources/PasoVientoHeladoTerrenoExtension.shader`. El script de ambiente quedó igual que antes de esta pasada. Se conservaron los caminos, la pared plana desactivada, la transparencia de visión y el brillo de montañas aprobados.

Capturas finales: `textura-sin-banda.png` y `frontal-noche-final.png`. Comparación previa: `antes.png`.

Shaders comprobados en Unity; diff sin errores de espacios. Tutorial completo no recorrido: el cambio de margen y los datos de material están acotados a zona 2, sin modificar nodos, movimiento, costes ni secuencias. Unity quedó fuera de Play.
