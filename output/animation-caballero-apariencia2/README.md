# Caballero apariencia 2: integración en Unity

Procedimiento seguido: `ACTUALIZACION_VISUAL_UNIDADES.md`. Se conserva el caballero de pelo gris, armadura envejecida y capa rojiza de los sprites originales.

## Resultado

- Tres sprites RGBA en `Assets/Resources/AnimacionesIlustradas/CaballeroApariencia2/`: reposo, anticipación e impacto.
- Perfil `Assets/Resources/AnimacionesIlustradas/CaballeroApariencia2.asset`, identificado por las referencias originales de mover y atacar de la apariencia alternativa.
- Idle con respiración; atento, dash, habilidades y defensa conservan sus sprites originales. Sin caminata ni sprite nuevo de daño.
- `UnidadPoseController.ConfigurarPoses` y `RestaurarPosesBase` actualizan el perfil ilustrado al cambiar de apariencia, reutilizando el motor existente.
- Las otras unidades y las escenas no fueron modificadas en esta actualización.

## Fuentes y reproducción

Generación mediante la herramienta integrada `image_gen`. Prompts exactos en `prompt.md` y `prompts-ataque.md`.

La lámina inicial se descartó como atlas completo porque la espada del impacto solapaba otra pose. Su primer personaje está completo y es la fuente del reposo. Los dos frames de ataque se generaron por separado en `anticipacion-fuente.png` e `impacto-fuente.png`.

Las fuentes tenían cuadrícula dibujada en RGB. Se eliminó el fondo y se reconstruyó el antialias contaminado del contorno, conservando los detalles interiores y el pelo gris. El procesamiento por código está autorizado por el usuario.

Reproducción desde la raíz del proyecto, con Python, Pillow, NumPy y SciPy:

```powershell
python tools/animation-pilot/prepare_caballero_apariencia2.py
```

El script solo escribe los assets de esta apariencia y su registro de preparación. No regenera la preview ni los personajes anteriores. `registro.json` conserva escalas y coordenadas de apoyo. Las escalas se midieron para estas fuentes: no llevan el aumento del 8% usado para el idle de la apariencia 1.

## Verificación realizada

- Tres PNG de 1024 × 768, RGBA real, sin tocar los bordes del lienzo; metas y referencias válidas.
- Perfiles sin colisiones entre las dos apariencias y la dríada.
- Inspección de transparencia y escala corporal sobre fondos oscuro, claro y contrastante, comparada con idle y atento originales (`verificacion-encuadre.png`).
- Compilación de runtime y editor correcta con el compilador y referencias locales de Unity 6000.3.20f1. Logs en `outputs/animation-pilot/compilacion-apariencia2.log` y `compilacion-editor-apariencia2.log`.
- Se amplió `Assets/Editor/ValidacionAnimacionIlustrada.cs` para comprobar la activación de apariencia 2, atento, dash, habilidad, ataque, defensa, ausencia de perfil y regreso a apariencia 1. Disponible desde `Tools > Arte > Validar animaciones Caballero y Driada`.

La validación de transiciones dentro del editor no produjo un informe durante esta sesión: las comprobaciones añadidas están compiladas, pero no se declaran ejecutadas. Quedan pendientes la prueba visual en batalla y el recorrido completo del tutorial. El cambio compartido está limitado a seleccionar el perfil al configurar/restaurar poses; no modifica tiempos de combate, estados ni secuencias del tutorial.
