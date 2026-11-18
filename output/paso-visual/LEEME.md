# Paso Viento Helado: actualización visual

Implementado y comprobado en Unity 6000.3.20f1 mediante el MCP `unity_black_breath`.

## Cambios

- Acantilado delantero: pizarra nueva, malla visual de 3225 vértices, estratos irregulares y cornisa redondeada apoyada sobre el terreno. Normales orientadas hacia la cámara. El collider original permanece intacto.
- Fondo: panorama de cordillera con capas de montañas y bruma; adaptación cromática a la hora y desplazamiento horizontal limitado al mover la cámara.
- Nieve: textura de ventisqueros, proyección en tres planos para las pendientes y detalle de relieve moderado. Pizarra expuesta en laderas.
- Hielo del lago: material propio con transición gradual hacia la bruma del fondo.
- Luz: perfil continuo de amanecer, mañana, mediodía, tarde, crepúsculo y noche. Menor relleno para conservar volumen y colores de puesta de sol.
- Ventisca: ráfagas variables, partículas deterministas y densidad reducida según calidad gráfica.
- Bruma de la extensión: transición de color con el panorama fuera del terreno jugable. La máscara de visión y el historial conservan sus cálculos originales.

## Verificación

- Compilación de C# y cinco shaders sin errores.
- Capturas en seis franjas: 07:00, 09:00, 13:00, 18:30, 20:00 y 23:00.
- El reloj de campaña permaneció a las 09:00. La hora que indica el HUD no cambia durante las previsualizaciones de iluminación.
- Perfil horario muestreado cada 0,01 horas: valores finitos, sin saltos de intensidad; no consume UnityEngine.Random.
- Preparar el ambiente dos veces no duplica materiales ni malla. No cambia reloj, RNG ni cantidad de colliders.
- Destruir el componente restaura la malla original de 121 vértices; volver a prepararlo recrea el relieve visual.
- Comprobado el retorno temprano para zona ID 1 (tutorial): no agrega el componente.
- `tools/check-runtime-deps.ps1`: sin dependencias de editor/test en runtime.
- Consola final sin errores ni excepciones.
- No se hizo build ni recorrido completo del tutorial. Tampoco un benchmark de rendimiento en hardware de destino.

## Incidente del editor

Durante la primera importación se bloqueó la recarga. La recuperación provocó un cierre inesperado de Unity. Se reabrió el proyecto, se preservaron las copias de recuperación que ofreció Unity y se terminó la validación en una sesión limpia. La ejecución original de Play Mode se perdió. No se sobrescribió `campaign_save.json` (seguía fechado 15/11/2026). Se terminó fuera de Play Mode, sin guardar modificaciones en las escenas.

No se debe usar `LockReloadAssemblies`/`DisallowAutoRefresh` para agrupar cambios por este MCP: RunCommand refresca y espera al editor antes de ejecutar el código. Modificar C# fuera de Play Mode y dejar que Unity complete la recarga.

## Capturas

- [Antes](antes.png)
- [Amanecer](final-amanecer.png)
- [Mañana](final-manana.png)
- [Mediodía](final-mediodia.png)
- [Atardecer](final-atardecer.png)
- [Crepúsculo](final-crepusculo.png)
- [Noche](final-noche.png)

## Assets generados

Generados con la herramienta integrada `image_gen`; se copiaron al proyecto y Unity importó sus metadatos. No se sustituyeron las imágenes de unidades ni la interfaz.

- `Assets/Resources/PasoVientoHeladoCordillera.png`
- `Assets/Resources/PasoVientoHeladoPizarra.png`
- `Assets/Resources/PasoVientoHeladoNieve.png`

Prompts utilizados (especificaciones de generación):

**Cordillera:** Game environment background matte painting texture, ultrawide horizontal panorama around 4:1. For a somber dark fantasy snowy mountain pass strategy game. No UI, text, characters, buildings, roads, foreground trees, water or lake. An expansive alpine valley with layered jagged rocky snow mountains: medium distance dark slate escarpments on far left and far right, open central valley receding into atmospheric pale blue distant sharp peaks. Snow collected in gullies and on ledges, realistic geological faces, thin mist between each range. Upper third pale overcast steel blue gray morning sky with soft cloud veils, no sun disc, no orange illumination, neutral soft diffuse daylight so the texture can be tinted for dawn dusk and night. Bottom third consists entirely of blue-gray ground mist fading into a smooth uniform desaturated pale slate blue at the very bottom, no hard horizon line. Highest peaks distributed across the upper middle, natural asymmetric varied silhouettes, no round puffy fantasy shapes. Detailed restrained painterly realistic game environment art matching a grounded medieval grimdark world. No black border, no labels, no vignette. This is a background texture only, depth and coherent mountain geology are essential.

**Pizarra:** A seamless horizontally tileable rock cliff albedo texture for a dark fantasy snowy alpine strategy game. Wide landscape 3:1. Orthographic front view of a dark slate cliff face, entire canvas filled with rock and a little snow, no sky, no landscape background, no floor. The upper edge 15 percent is wind-packed off-white snow cornice feathering down onto irregular dark charcoal blue gray layered slate. Lower 85 percent geological craggy broken rock strata, angular fractures, narrow vertical crevices, subtle frost and small snow deposits on horizontal shelves. Large readable rock blocks with fine cracks, varied sizes, realistic but slightly painterly grounded medieval game art. Diffuse even overcast neutral lighting, no cast shadows from outside frame, no strong directional illumination, no shiny metallic look, subdued palette slate gray and desaturated pale blue white. Match believable snowy mountain geology. The rock must remain detailed and dark toward bottom. No icicle crystals, no objects, no text, no borders.

**Nieve:** Seamless tileable square game ground ALBEDO texture, top-down orthographic view straight down at wind-carved alpine snow. Entire frame filled with natural snow surface, no horizon or other objects. Broad softly sculpted wind-packed drifts with shallow irregular sastrugi ridges oriented diagonally, quiet smooth compact snow between ridges, extremely subtle ice-blue shadow pockets and fine granular frost. Natural asymmetrical variation, restrained realistic painterly dark fantasy strategy game environment art. Neutral diffuse overcast daylight, soft medium low contrast, muted silver white and desaturated blue gray, no strong baked cast shadows. No rocks, footprints, grass, trees, holes, text, vignette, frame, checkerboard. Avoid uniform corrugated lines, rings, swirls, repetitive waves or glossy plastic. Snow forms should be readable at strategy game zoom, large gentle ridges mixed with small detail, not extreme close-up grains.
