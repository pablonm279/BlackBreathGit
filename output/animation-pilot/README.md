# Piloto: Caballero apariencia 1 y Driada Quemada

Version ajustada segun la revision del usuario del 7 de septiembre de 2026.

- Reposo: sprite nuevo y respiracion sutil.
- Atento (turno activo / objetivo): sprite original propio, con respiracion.
- Movimiento: sprite original y dash existente, con inclinacion de impulso y recuperacion de 0.18 s. Sin ciclo de caminata.
- Ataque: anticipacion e impacto nuevos; utiliza el instante de impacto del combate existente. Recupera atento, reposo o postura defensiva segun el estado actual.
- Dano: sin sprite propio. Conserva la pose y el feedback de impacto existente del juego.
- Habilidad: conserva el sprite original y las reglas de pose sostenida.
- Postura defensiva del Caballero: conserva su sprite original.
- La apariencia 2 del Caballero queda fuera del perfil.

## Archivos

- `comparacion.html`: preview autocontenida; se puede abrir directamente en un navegador.
- `Assets/Resources/AnimacionesIlustradas/`: dos perfiles y seis sprites RGBA utilizados por Unity.
- `prompts.md`: prompts exactos usados con el generador integrado.
- `caballero-generado.png` y `driada-generada.png`: laminas fuente generadas.
- `descartados/`: pasos y poses de dano retirados del juego tras la revision; no se cargan como recursos.
- `tools/animation-pilot/prepare_sprites.py`: recorte y alineacion autorizados por el usuario.
- `tools/animation-pilot/build_preview.py`: reconstruccion de la preview.

## Verificacion

Compilacion de runtime y editor: correcta con el compilador y referencias locales de Unity 6000.3.20f1.
Seis sprites RGBA con transparencia real; los perfiles activos no referencian pasos ni poses de dano.
Preview abierta y revisada por el usuario.

La prueba completa en Play Mode no se pudo ejecutar en esta sesion: la instancia temporal de Unity no alcanzo la escena de prueba. Falta comprobar las transiciones durante un combate real y el tutorial completo. Los cambios no modifican recorridos, tiempos de combate, escenas ni estados del tutorial, pero su presentacion visual debe revisarse en juego.

La comprobacion de poses en editor puede ejecutarse desde `Tools > Arte > Validar animaciones Caballero y Driada`; escribe su resultado en `outputs/animation-pilot/validacion-editor.txt`. La validacion completa mediante instancia dedicada esta en `Assets/Editor/ValidacionAnimacionIlustrada.cs`.
