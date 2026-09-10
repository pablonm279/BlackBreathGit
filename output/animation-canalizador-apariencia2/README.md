# Canalizador, apariencia 2

Implementado siguiendo `ACTUALIZACION_VISUAL_UNIDADES.md`. Generación con imagegen integrado; prompts exactos en `prompts.md`. Referencias originales de idle, atento y ataque de `Assets/Scripts/Clases/Canalizador/Aparienciasalternativas` (las asignadas al prefab, no AlternativaREWORK1).

## Assets y preparación

- Perfil: `Assets/Resources/AnimacionesIlustradas/CanalizadorApariencia2.asset`.
- Sprites: `Assets/Resources/AnimacionesIlustradas/CanalizadorApariencia2/{reposo,anticipacion,impacto}.png`.
- Fuentes: `{reposo,anticipacion,impacto}-fuente.png` en esta carpeta. La anticipación inicial tenía cuadrícula oscura dibujada; se corrigió con imagegen usando `correccion-fondo.md`, conservando el descarte fuera de Assets.
- Preparación reproducible: `tools/animation-pilot/prepare_canalizador_apariencia2.py`. Extracción y descontaminación por código autorizadas.
- RGBA 1024 × 768, alfa activo, Sprite Single, bilineal, Clamp, sin mipmaps ni compresión.
- Escalas: reposo 0.55, anticipación 0.585, impacto 0.585. Centros de pies fuente: 600, 700, 690. Apoyo `(0.5, 16/768)`, corrección horizontal `1024/750`, desplazamiento `0.02`, respiración `0.006`. Detalle en `registro.json`.

El perfil coincide exclusivamente con mover `51adfd2d46dd9454a91b10920006616c` y ataque `1eab1d4a60ba82345a2149e8c4a43207`. Conserva atento `76f24cc9b0aad404288879c96fb41290`, habilidad/acumulación `a3767e83fac60cf4dad6e6bc296298ca`, daño sin sprite nuevo, movimiento original y dash. La apariencia 1 conserva su perfil.

Reutiliza la señal de impacto real de las descargas introducida para Canalizador 1 y la respiración al 80% fuera del turno propio. No fue necesario editar C#, escenas, prefabs ni lógica del tutorial para esta apariencia.

## Validación

Comprobados transparencia real, márgenes, configuración de importación, referencias originales y nuevas; nueve perfiles con parejas mover/atacar únicas. Resultado en `validacion-assets.txt`.

Inspección de anatomía, manos, equipo, proporciones, apoyo y bordes sobre fondos oscuro, claro y contrastante en las láminas PNG internas. No se modificó la preview HTML.

Pendiente: ver las transiciones en Play Mode (idle/atento, dash, descargas, acumulación y su interrupción, daño, pausa, inmovilización, muerte, ambas orientaciones y cambio de apariencia), y el tutorial. Las comprobaciones estáticas no validan la animación en batalla. El riesgo pendiente es visual de encuadre/transición; no se modificó lógica del tutorial. Entrar de nuevo en Play Mode para renovar la caché de perfiles.
