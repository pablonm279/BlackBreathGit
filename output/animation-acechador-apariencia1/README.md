# Acechador, apariencia 1

Implementación según `ACTUALIZACION_VISUAL_UNIDADES.md`. Generación con la herramienta integrada `imagegen`, usando las ilustraciones originales como referencias; sin CLI. Prompts exactos en `prompts.md` y corrección de la espada en `correccion-anticipacion.md`. Las fuentes finales son `reposo-fuente.png`, `anticipacion-fuente.png` e `impacto-fuente.png`. La primera anticipación tenía una hoja duplicada y se conserva como descarte, fuera de Assets.

## Assets y registro

- Perfil: `Assets/Resources/AnimacionesIlustradas/AcechadorApariencia1.asset`.
- Sprites: `Assets/Resources/AnimacionesIlustradas/AcechadorApariencia1/{reposo,anticipacion,impacto}.png`.
- Preparación reproducible: `tools/animation-pilot/prepare_acechador_apariencia1.py`; reutiliza el extractor con descontaminación de bordes autorizado.
- PNG RGBA 1024 × 768; importación Sprite Single, sin compresión ni mipmaps, bilineal, Clamp y alfa activo.
- Escalas de fuente: reposo 0.605, anticipación 0.585, impacto 0.615. Línea inferior común y apoyo a 16 px del borde inferior. Apoyo horizontal 0.5, corrección 1024/750, desplazamiento -0.005, respiración 0.006. Medidas completas en `registro.json`.
- Reposo: 696 px de alto frente a aproximadamente 693 px del atento original a escala equivalente. Se comparó cuerpo, cabeza y pies; el impacto se agacha naturalmente.

## Comportamiento conservado

El motor existente encuentra este perfil por mover `32f7997e200fc8b4d8c70b8c6aae214b` y atacar `f78cfe14488de1a4c83e54566331161f`. Esa segunda referencia corresponde al ataque activo del prefab, no a la variante de archivo `Acechador_atacando_espada_corta.png`.

Atento original por turno/objetivo, dash con sprite original, habilidades y sus prioridades originales. Sin sprite nuevo de daño ni ciclo de pasos. La respiración global sigue al 80% fuera del turno propio y al 100% durante él; el ataque mantiene su preimpacto de 0.42 s y la ventana de impacto de 0.18 s. La apariencia 2 no coincide con el perfil.

No fue necesario modificar C#, prefabs o escenas. Se conserva la lógica de Escondido y su protección del tutorial en `ClaseAcechador.GanarEscondido`.

## Verificación realizada y pendiente

Inspección de las ilustraciones, corrección de la hoja duplicada, comparación estática de escala y bordes sobre fondo oscuro, claro y contrastante. Las láminas PNG son controles internos; no se modificó la preview HTML.

Comprobados alfa real, margen transparente, referencias originales y nuevas, configuración de importación y parejas mover/atacar únicas entre los seis perfiles. Resultado en `validacion-assets.txt`.

Pendiente: ejecución en Unity de idle/atento, dash, ataque, habilidades, daño, ocultación, pausa, inmovilización y muerte; probar ambas orientaciones y el tutorial. No se ejecutó Play Mode ni se amplió el validador de editor para este perfil. La comprobación estática no demuestra las transiciones en batalla. Volver a entrar en Play Mode para renovar la caché de perfiles.
