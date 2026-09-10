# Duelista apariencia 2 y ritmo global de respiración

Actualización según `ACTUALIZACION_VISUAL_UNIDADES.md`. La apariencia conserva pelo castaño, capa azul bordada en dorado, mangas blancas, broche turquesa, botas negras y espada de esgrima con guarda dorada. No lleva sombrero.

## Assets

- `Assets/Resources/AnimacionesIlustradas/DuelistaApariencia2/`: reposo, anticipación e impacto nuevos, en PNG RGBA.
- `Assets/Resources/AnimacionesIlustradas/DuelistaApariencia2.asset`: perfil específico de los sprites originales de la apariencia alternativa.
- Conserva atento, habilidades, En Garde y dash originales; sin ciclo de caminar ni sprite nuevo de daño.
- Lienzo de 1280 × 768 para incluir la espada completa, importado sin compresión y con máximo 2048. Escalas de las fuentes: 0.546, 0.67 y 0.725; pies en y=760. Registro completo en `registro.json`.

Generación mediante la herramienta integrada `image_gen`; prompts exactos en `prompts.md` y en los dos archivos `prompt-*.md`. Las dos primeras fuentes de reposo tenían fondo oscuro y se descartaron. La fuente final de reposo se generó sobre blanco; anticipación e impacto tenían cuadrícula RGB. El recorte y la limpieza por código, autorizados por el usuario, produjeron transparencia real conservando los detalles claros y dorados.

Reproducción, con Python, Pillow, NumPy y SciPy:

```powershell
python tools/animation-pilot/prepare_duelista_apariencia2.py
```

El script solo prepara esta apariencia. No regenera la preview ni los personajes anteriores.

## Respiración global

`UnidadAmbientMotionFx.ObtenerFactorRespiracion` aplica velocidad `1` durante el turno propio y `0.8` fuera de él, tanto a personajes como a enemigos. Seleccionar una unidad como objetivo no le concede la velocidad del turno activo. Sin batalla mantiene la velocidad normal.

`UnidadAnimacionIlustrada` y `UnidadAmbientMotionFx` acumulan un reloj independiente para la respiración; el cambio de turno modifica su velocidad sin recalcular la fase desde el tiempo absoluto. El cambio a atento tampoco reinicia ese reloj. Ataques, dash, amplitud de respiración y reglas de habilidad conservan sus valores. La pausa y los estados inmovilizantes detienen el reloj de respiración. El antiguo bloque de respiración de `UnidadIdleMotion` está comentado y no se reactivó.

La guía principal se actualizó para que esta regla también se conserve en próximas unidades.

## Verificaciones y límites

- Tres sprites RGBA con referencias válidas, márgenes transparentes y configuración de importación comprobada; cinco perfiles sin colisiones. Informe: `validacion-assets.txt`.
- Escala comparada con idle y atento originales, y contornos revisados sobre fondos oscuro, claro y contrastante: `verificacion-escala.png` y `verificacion-contornos.png`.
- Runtime y editor compilan con Unity 6000.3.20f1. Logs en `outputs/animation-pilot/compilacion-duelista2-respiracion.log` y `compilacion-editor-duelista2.log`.
- La validación de editor se amplió para ambas apariencias de la duelista, En Garde, habilidades, ataque, cambio de perfil y factores de respiración de jugadores/enemigos. También comprueba que pasar a atento no reinicia la fase. Estas comprobaciones están compiladas, pero no se ejecutaron en Unity durante esta sesión.

Quedan pendientes la prueba visual en batalla y el recorrido completo del tutorial, especialmente las transiciones de turno. El cambio global solo afecta la presentación de respiración; no modifica secuencias, estados ni tiempos de combate del tutorial.
