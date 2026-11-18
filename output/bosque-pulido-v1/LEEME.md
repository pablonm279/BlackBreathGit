# Pulido del Bosque Ardiente

Se conserva la composición y el contenido existente. Los cambios son visuales y exclusivos de la zona 1.

- Suelo: variaciones amplias de ceniza, astillas pequeñas filtradas a distancia y microrrelieve más definido. Mantiene la opción de textura procedural del inspector.
- Ambiente: humo con más detalle, turbulencia tenue y variación lenta de emisión. Los emisores originales, colores HDR y LOD de las llamas se conservan.
- Iluminación: sombras algo más suaves y ajuste discreto del tinte nocturno. Relleno cálido con un máximo de 1/2/4 luces sin sombras según la calidad.
- Adornos: troncos caídos, tocones con raíces y grupos de ramas carbonizadas. Entre 24 y 72 candidatos según superficie, reducidos en calidad baja/media; la cantidad final depende de los espacios disponibles.
- Fondo: tres capas de siluetas con el arbolado existente y velos de humo. Cuatro renderers adicionales, sin colliders. Se adapta la altura a la inclinación de cámara.

Los adornos respetan las huellas de caminos y nodos, incluidos los ocultos. Si aparece un camino nuevo, se retiran los detalles que invaden su huella. No se agregan colliders ni se modifica el relieve jugable.

## Aplicación

Se aplica automáticamente al generar el Bosque Ardiente. En una campaña abierta en Play se puede usar `Tools > Campaña > Bosque Ardiente > Reaplicar ambiente (Play)`.

No se guardaron cambios en la escena original, ni se editaron prefabs, traducciones o scripts del tutorial. Se conservaron los cambios previos del worktree. Las fuentes anteriores a esta intervención y los diffs propios están en esta carpeta.

## Verificaciones

- Compilación completa con las referencias reales de Unity 6000.3.20f1: `Assembly-CSharp` y `Assembly-CSharp-Editor`, sin errores.
- Chequeo de dependencias runtime: aprobado.
- Regresión aislada: materiales originales, emisores, posiciones, LOD, RNG, preparación repetida, pausa y limpieza.
- Integración aislada: shaders, exclusiones de caminos visibles/ocultos y nodos, regeneración determinista, ausencia de colisiones nuevas, retiro ante rutas nuevas, presupuesto de luces y reducción de calidad.
- Capturas de día/noche, paneos y zoom con cámara, escala, terrenos, luces y las doce tandas de decoración extraídas de la escena fuente. En este escenario se colocaron 33 grupos nuevos.

`comparacion.png` reúne las vistas antes/después. `escenario/` contiene las capturas completas; `validacion-pulido.txt`, `regresion/validacion.txt` y `final.log` contienen las comprobaciones. Las compilaciones están en `compilacion/`.

## Límites de la validación

La captura de la ventana del editor falló por timeout. Las capturas entregadas pertenecen a una copia visual aislada, sin HUD, distribución de nodos de partida, controladores de cámara ni posprocesado de campaña.

No se recorrió el tutorial completo en Play ni se midió el rendimiento en una partida real. La integración conserva la espera de decoración y el fader, y las pruebas verifican que no se altera el RNG, las rutas ni los nodos. Sigue pendiente comprobar de forma interactiva la secuencia del tutorial y la composición junto a su UI.
