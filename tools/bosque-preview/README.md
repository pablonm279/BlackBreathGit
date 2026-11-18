# Validación del ambiente del Bosque Ardiente

`python tools/prepare-bosque-preview.py` prepara un proyecto aislado en `Temp/BosquePreview`.
Usa copias de los shaders, el controlador y los prefabs reales. Extrae también una llama
de `LlamasContainer` de la escena de campaña, sin modificar esa escena.

Ejecutar Unity 6000.3.20f1 sobre esa copia con `-batchmode -executeMethod BosqueRender.Run -quit`.
Las capturas y comprobaciones quedan en `output/ambiente-bosque`.

Comprueba los shaders, la preparación repetida, el RNG, la pausa del reloj y la limpieza
al desactivar el ambiente. Compara la serialización de las partículas originales, sus
renderers, transforms y LOD antes y después: solo puede cambiar la referencia al material
de las llamas. Verifica ambos orígenes del fuego, los materiales fuente intactos y conserva
el humo original sin cambios. Captura el mismo instante con los materiales nuevos y anteriores.
Comprueba los colores, la emisión HDR, el brillo y el blending contra los materiales fuente.
Con la máscara desactivada, compara los píxeles de fuego con el sombreado Standard original.
Verifica que se conservan el terreno y los tres sistemas ambientales, sin incendios grandes.
Incluye el relieve de 2,4 m, la escala no uniforme del padre de campaña y las funciones
reales de calidad/LOD extraídas del código fuente.
El contexto mínimo de esta prueba sustituye los gestores del juego: no recorre el tutorial,
no carga partidas y no representa una captura de la campaña completa.

Para revisar una campaña real en Play está el menú
`Tools > Campaña > Bosque Ardiente > Capturar ambiente actual`.
El menú `Reaplicar ambiente (Play)` permite actualizar sus materiales sin regenerar el mapa.

## Pulido integral del bosque

`python tools/prepare-bosque-preview.py --pulido` incluye las comprobaciones nuevas y una
copia visual de terrenos, cámara, luces y partículas de la escena fuente. Extrae las doce
tandas de decoración originales del método de campaña y las ejecuta con el decorador real.
El proyecto aislado usa su propia identidad para no compartir PlayerPrefs con el juego.

Ejecutar Unity con `-batchmode -executeMethod BosqueEscenarioRender.RunTodo -quit` sobre
`Temp/BosquePreview`. Los resultados quedan en `output/bosque-pulido-v1`: regresión de llamas,
exclusiones de rutas visibles y ocultas, limpieza, RNG, regeneración, límites de calidad,
compilación de shaders y capturas de día/noche, paneos y zoom. Las comparaciones usan las
fuentes conservadas en `output/bosque-pulido-v1/fuentes-antes`.

La copia visual conserva la escala y cámara serializadas de la escena, pero omite gestores,
UI y controladores de cámara. No contiene la distribución de nodos de una partida real ni
su posprocesado. No reemplaza una prueba interactiva de campaña ni del tutorial completo.
