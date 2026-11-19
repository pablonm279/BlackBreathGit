# Corrección de distribución, tamaño y tono de las llamas

Se eliminó el emisor que producía la fila de llamas fuera del bosque. El fondo conserva
solo las tres cortinas de humo. El fuego visible usa los focos distribuidos originalmente
en el mapa, sin mover sus emisores ni aumentar sus tasas de emisión.

Las llamas se recortan al terreno principal y al rango horizontal de sus nodos. Mantienen
los estados anteriores de visibilidad cuando están dentro del área permitida. Los efectos
espectrales y los emisores del Paso y de Nedukazal quedan fuera de esta modificación.

Respecto de la versión rechazada, la altura se redujo aproximadamente un 38 % y el ancho
un 27 %. El perfil ahora usa densidad turbulenta en lugar de un contorno de cono fijo,
un núcleo cálido menos saturado, menor opacidad y una base con desvanecimiento suave.
Se agregó lectura de profundidad para suavizar intersecciones con el suelo y los árboles.
El resplandor de apoyo tiene menos intensidad, un radio menor y una posición más baja.

La pared de humo conserva movimiento y volumen de ruido, con menos naranja en su
iluminación. Las texturas, geometría y escala de caminos tomadas del Paso se mantienen.

## Verificación

La prueba aislada de Unity comprueba shaders, posiciones y emisión originales, aislamiento
entre zonas, visibilidad dentro de los límites de los nodos, ausencia de una fila exterior,
reaplicación sin duplicados ni escalado acumulativo, pausa, calidad baja y restauración
del humo anterior. También compara materiales, texturas efectivas, escala UV, vértices y
repetición de las cinco variantes de camino entre Paso y bosque.

`validacion.txt` y `final.log` contienen los resultados; `compilacion/` contiene las
compilaciones completas con las referencias reales del proyecto. Las fuentes anteriores
a esta corrección están en `fuentes-antes/`.

Las capturas y la animación son de una copia visual aislada con cámara y terrenos de la
escena fuente. No incluyen HUD ni posprocesado de campaña. El tutorial completo y el
rendimiento de una partida real siguen sin comprobar; queda pendiente revisar la
composición junto a su UI y la secuencia interactiva en Play.
