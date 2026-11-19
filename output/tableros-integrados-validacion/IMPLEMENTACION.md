# Tableros integrados: implementación

Fecha: 2026-11-18.

## Resultado

Se implementaron Paso Viento Helado, Bosque de los Lamentos y Nedukazal siguiendo la opción de suelo continuo integrado aprobada por el usuario. La capa se configura automáticamente al preparar el ambiente de cada combate. Subterráneo conserva su presentación anterior.

- Paso: tierra congelada y grava, nieve en márgenes.
- Bosque: tierra quemada, ceniza y pequeños restos vegetales en márgenes.
- Nedukazal: piedra gastada, polvo y grabados tenues.
- Bordes finos en las mallas originales. Blanco: movimiento normal; rosado: coste mayor; violeta: intercambio; amarillo: turno aliado; rojo: hostil/turno enemigo según la capa actual; azul: habilidad amiga o área. Se mantienen círculos, marcas melee, trampas y vista táctica.
- Sombras de contacto de obstáculos reutilizan su gráfico existente, sin copiar colliders.
- Barras finas y horizontales: mallas de presentación que leen el valor normalizado del Slider original. No rotan ni reescalan la jerarquía del obstáculo. Los gráficos originales se restauran al desactivar la capa. Barras y sombras se ocultan al retirar el obstáculo.

## Código y assets

- Assets/Scripts/Visual/TableroIntegradoBatalla.cs: preparación, materiales en memoria, suelo, sombras y barras reversibles.
- Assets/Scripts/Visual/BattleAmbientLife.cs: una llamada al final de Configurar, después de que CampoVivo libere el diorama anterior.
- Assets/Scripts/Visual/CampoVivo/CampoVivoBatalla.cs y DioramaBatalla.cs: omiten la placa y borde anteriores solo para las tres zonas integradas; mantienen las capas atmosféricas existentes.
- Assets/Resources/TablerosIntegrados/Suelo.shader: superficie plana con margen irregular transparente, opaca bajo las casillas.
- Assets/Resources/TablerosIntegrados/Borde.shader: colores funcionales y compatibilidad con los bloques de propiedades de Casilla; también dibuja las barras.
- Assets/Resources/TablerosIntegrados/PasoSuelo.png
- Assets/Resources/TablerosIntegrados/BosqueSuelo.png
- Assets/Resources/TablerosIntegrados/NedukazalSuelo.png

Texturas generadas con la herramienta integrada image_gen y guardadas en Assets/Resources/TablerosIntegrados. Importación: 1024 × 1024, BC7 en el Editor actual, mipmaps, Clamp, filtro trilineal y exclusión del límite global de mipmaps. La calidad actual del proyecto limita las texturas a un cuarto de resolución; la exclusión mantiene el detalle de estos tres assets sin cambiar los ajustes globales. Los PNG fuente se conservan a su resolución de generación.

Se conservaron los cambios previos del usuario. Las copias previas de los tres scripts modificados están en fuentes-previas para revisar exclusivamente el delta de esta tarea. No se editaron escenas, prefabs, Casilla.cs, BattleManager.cs, TutorialCombate.cs, scripts del tutorial ni Assets/TRADU.cs.

## Validación

Unity 6000.3.20f1 compiló el código y los shaders. Assets/Editor/ValidacionTablerosIntegrados.cs prepara una escena de preview aislada a partir de la grilla, cámara, luces, gráficos de unidades y obstáculos reales. No abre ni guarda otra escena sobre la campaña y no ejecuta una partida. Menú: Tools > Combate > Tableros integrados > Validar y capturar sin alterar partida.

182 comprobaciones correctas, registradas en validacion.txt. Incluyen las variantes de fondo de las tres zonas, posiciones y coordenadas de las 30 casillas, costes, ocupación, colliders, activación de capas, vida y Slider de obstáculos, dimensiones proyectadas de barras, relleno al 25 %, desaparición al retirar un obstáculo, restauración de gráficos/materiales, limpieza sin acumulación, transición desde caverna, ausencia de placas duplicadas, RNG y reloj intactos. La escena original terminó con isDirty=False. El chequeo tools/check-runtime-deps.ps1 pasó sin dependencias de Editor/test en runtime.

Capturas reales del preview:

- paso-implementado.png
- bosque-implementado.png
- nedukazal-implementado.png

Estas capturas muestran el render de Unity, no imágenes generadas. Usan gráficos estáticos de comparación de los prefabs reales y no incluyen la interfaz completa, el posprocesado de la partida ni el recorrido del tutorial. Los fondos y su iluminación real pueden diferir de los mockups. No se sustituyeron los fondos ni se recrearon unidades para aparentar coincidencia con las imágenes.

## Tutorial y límites de validación

Se revisaron sus dependencias de selección y movimiento; la integración no cambia eventos, índices, objetos de casilla, costes ni colliders y no añade elementos que intercepten los clics. No se recorrió el tutorial completo en Play. Queda pendiente confirmar su señalización visual y selección durante toda la secuencia, además de una revisión interactiva de combate con trampas, unidades grandes y vista táctica. El riesgo pendiente es visual/de integración en esos recorridos, no una modificación intencional de sus reglas.

## Prompts de las texturas

### Ajuste posterior de margen y casillas

Por pedido del usuario, el margen exterior baja de 0.78 a 0.546 celdas por lado (30 % menos); se mantienen centros, escala y separación de las casillas. Los marcos se dibujan sobre planos del mismo Renderer con bordes finos desgastados, variación por casilla y un interior levemente oscurecido. Se conservan colores, círculos y espadas. Las mallas anteriores se restauran al desactivar la presentación; los engrosadores redundantes se ocultan de forma reversible.

La comprobación posterior usa la implementación real con 30 casillas inactivas en una escena aislada: confirmó la restauración y reactivación de mallas/materiales, el margen exactamente 30 % menor en ambos ejes, y que posiciones, vidas y RNG de la partida activa no cambiaron. El detalle del shader está en casillas-detalle.png y el registro en ajuste-casillas.txt. Las 182 comprobaciones y las tres capturas generales anteriores corresponden a la versión previa a este ajuste; el recorrido completo del tutorial sigue sin comprobarse en Play. Para cargar de forma completa los scripts recién compilados en la partida se requiere reiniciar Play.

### PasoSuelo.png

Referencia: C:\UnityProyectosNOborrar\GDD - Untitled\output\paso-tablero-propuestas\03-suelo-glacial.png

```text
Use case: stylized-concept. Asset type: production-ready Unity game GROUND ALBEDO texture. The supplied screenshot is ONLY a material/style reference, NOT a composition to reproduce. Reconstruct the approved tactical floor material as a full flat texture viewed EXACTLY ORTHOGRAPHIC TOP DOWN, NO perspective, NO horizon, NO scene, NO units, NO HUD, NO grid or tile outlines, NO icons, NO written labels, NO contact-sheet, NO cast shadows, NO specular highlights, NO directional illumination or vignette. A single square edge-to-edge quiet hand-painted dark fantasy ground surface with subtle detailed grain appropriate for a 6-by-5 tactical battlefield. Opaque image. Central playable region must be calm and readable. Fine material variation, medium-scale subtle structure, no huge contrast blobs, no theatrical landscape. Match the actual ground color in the reference's playable board. The texture will be placed on a simple flat plane under existing sprites and tactical outlines; it must remain faithful to the approved material. One continuous ground, no board perimeter frame or elevated slabs. Size 1536 square if possible.
MATERIAL: dark desaturated cool brown-gray compacted frozen earth, charcoal slate gravel and fine sediment. Very faint opaque worn ice patches, subtle hairline cracks. Restrained sparse tiny pebbles. Absolutely no reflective/translucent ice. Center 75% is mainly dark earth with very few tiny frost flecks, matching approved reference. Wind-scoured dull white snow and fine gravel gathers irregularly ONLY in a narrow band near all four OUTER edges, with soft interleaving earth/snow fingers. NO white snow through the center, no roads, no giant rocks. No black painted shadow.
```

### BosqueSuelo.png

Referencia: C:\UnityProyectosNOborrar\GDD - Untitled\output\tableros-integrados-zonas\bosque-suelo-integrado.png

```text
Use case: stylized-concept. Asset type: production-ready Unity game GROUND ALBEDO texture. The supplied screenshot is ONLY a material/style reference, NOT a composition to reproduce. Reconstruct the approved tactical floor material as a full flat texture viewed EXACTLY ORTHOGRAPHIC TOP DOWN, NO perspective, NO horizon, NO scene, NO units, NO HUD, NO grid or tile outlines, NO icons, NO written labels, NO contact-sheet, NO cast shadows, NO specular highlights, NO directional illumination or vignette. A single square edge-to-edge quiet hand-painted dark fantasy ground surface with subtle detailed grain appropriate for a 6-by-5 tactical battlefield. Opaque image. Central playable region must be calm and readable. Fine material variation, medium-scale subtle structure, no huge contrast blobs, no theatrical landscape. Match the actual ground color in the reference's playable board. The texture will be placed on a simple flat plane under existing sprites and tactical outlines; it must remain faithful to the approved material. One continuous ground, no board perimeter frame or elevated slabs. Size 1536 square if possible.
MATERIAL: desaturated dark umber compacted dry burned-forest soil, fine charcoal ash and subtle flattened leaf fragments. Center 80% is relatively clean flat small-grain earth with shallow hairline drying cracks and sparse tiny stones, same quiet brown as approved reference. Scattered very small copper-brown dead leaves, tiny black twigs and occasional shallow root fragments are concentrated near outer edges, NEVER huge roots or large leaves. No live greenery, no snowy white ash, no fire, no glowing embers or lava, no orange lighting baked in. Surface is dark warm-brown neutral.
```

### NedukazalSuelo.png

Referencia: C:\UnityProyectosNOborrar\GDD - Untitled\output\tableros-integrados-zonas\nedukazal-suelo-integrado.png

```text
Use case: stylized-concept. Asset type: production-ready Unity game GROUND ALBEDO texture. The supplied screenshot is ONLY a material/style reference, NOT a composition to reproduce. Reconstruct the approved tactical floor material as a full flat texture viewed EXACTLY ORTHOGRAPHIC TOP DOWN, NO perspective, NO horizon, NO scene, NO units, NO HUD, NO grid or tile outlines, NO icons, NO written labels, NO contact-sheet, NO cast shadows, NO specular highlights, NO directional illumination or vignette. A single square edge-to-edge quiet hand-painted dark fantasy ground surface with subtle detailed grain appropriate for a 6-by-5 tactical battlefield. Opaque image. Central playable region must be calm and readable. Fine material variation, medium-scale subtle structure, no huge contrast blobs, no theatrical landscape. Match the actual ground color in the reference's playable board. The texture will be placed on a simple flat plane under existing sprites and tactical outlines; it must remain faithful to the approved material. One continuous ground, no board perimeter frame or elevated slabs. Size 1536 square if possible.
MATERIAL: desaturated dark warm brown-gray ancient stone floor, worn granular basalt/sandstone, dust in hairline mortar joints, subtle chips and eroded edges. Broad irregular masonry slabs with quiet low-contrast seams, NOT a uniform square tactical grid. A few very faint shallow incised angular ancient rune marks, worn and dark, small relative to slabs, not glowing, not letters or readable writing, not on every slab. Center calm, readable, like approved reference, no thick raised bevels, no shiny marble, no ornamented border, no metals, no bright orange illumination baked in. Small grit at outermost margins.
```


