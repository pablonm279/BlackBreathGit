# Duelista apariencia 1

Implementación según `ACTUALIZACION_VISUAL_UNIDADES.md`. Mantiene a la duelista original de pelo oscuro, sombrero con plumas, gambesón negro, detalles rojizos y estoque.

## Assets e integración

- `Assets/Resources/AnimacionesIlustradas/DuelistaApariencia1/`: `reposo.png`, `anticipacion.png` e `impacto.png` con transparencia RGBA real.
- `Assets/Resources/AnimacionesIlustradas/DuelistaApariencia1.asset`: perfil específico de la apariencia base, reconocido por sus referencias originales de mover y atacar.
- Idle con respiración; ataque con preparación y estocada sincronizados con los tiempos existentes. Atento, dash, habilidades y En Garde conservan sus sprites originales. Sin caminata ni nuevo sprite de daño.
- Utiliza el motor ilustrado existente. Esta actualización no cambia código runtime, prefabs ni escenas. La apariencia 2 queda fuera del perfil.

## Preparación reproducible

Generación con la herramienta integrada `image_gen`, una imagen por pose y referencias originales de idle, atento y ataque. Prompts exactos en `prompts.md`; fuentes en los tres archivos `*-fuente.png`.

Las fuentes RGB tenían cuadrícula dibujada. Se procesaron por código con la autorización del usuario: eliminación del fondo, reconstrucción de antialias y limpieza de residuos claros en el borde. Se preservaron plumas, cuello y puños claros, y el estoque fino.

```powershell
python tools/animation-pilot/prepare_duelista_apariencia1.py
```

Requiere Pillow, NumPy y SciPy. Reutiliza el recorte de `prepare_caballero_apariencia2.py` sin ejecutar su preparación ni sobrescribir sus assets.

El lienzo común de 1280 × 768 da margen a toda la estocada sin reducir el cuerpo. Importación con tamaño máximo 2048, sin compresión ni mipmaps. El perfil compensa el ancho (`1280/750`) y sitúa los pies al 52.5% horizontal del Image original. Las escalas específicas de las fuentes son 0.573, 0.635 y 0.745 para reposo, anticipación e impacto, respectivamente. Los pies se alinean en y=750. `registro.json` conserva los recortes y medidas.

## Verificación

- PNG, alfa, límites, importación y referencias verificados; cuatro perfiles sin colisiones y apariencia 2 excluida. Informe: `validacion-assets.txt`.
- Tamaño corporal comparado con idle y atento originales; bordes inspeccionados sobre fondos oscuro, claro y contrastante en `verificacion-escala.png` y `verificacion-contornos.png`. Estas imágenes son comprobaciones de assets; no se regeneró la preview HTML.
- Se amplió la validación del editor para incluir idle, atento, dash, habilidad sostenida, estocada, recuperación a En Garde y cambio a apariencia 2 y vuelta. Compila correctamente; log en `outputs/animation-pilot/compilacion-editor-duelista1.log`. Se ejecuta desde el menú existente `Tools > Arte > Validar animaciones Caballero y Driada`, que ahora también comprueba Duelista 1.

Las comprobaciones de transiciones del editor se dejaron preparadas y compiladas, pero no se ejecutaron en esta sesión. Falta revisar las animaciones en una batalla real y el tutorial completo. No hay cambios de lógica, tiempos ni secuencias del tutorial; queda por comprobar su presentación visual con los nuevos assets.
