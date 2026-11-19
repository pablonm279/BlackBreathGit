# Fondos de batalla — Paso Viento Helado

Generación: herramienta integrada image_gen, edición con las imágenes originales como referencias de composición. El fondo 1 rehecho sirvió de referencia de estilo para los fondos 2 y 3. La captura de batalla con interfaz se usó como referencia de integración.

Los tres originales se conservan en `originales/`. Entregables: `fondo1.png`, `fondo2.png`, `fondo3.png`, también instalados en sus rutas originales de Assets conservando sus archivos .meta y materiales.

Resolución: 1536 × 1024, RGB, proporción 3:2 en los tres casos.

## Prompt 1

Use case: style-transfer
Asset type: production painted background texture for an existing dark fantasy tactical battle scene, first of a coherent three-image set.
Input images: Image 1 (Fondopaso1.png) is the edit target and strict geometry/composition reference. Image 2 (interfaz-antes.png) is STYLE AND INTEGRATION REFERENCE ONLY: ornate blackened bronze gothic UI and finely drawn medieval character sprites; do not reproduce any UI, characters, grid, effects or writing.
Primary request: Completely repaint and improve image 1 into premium hand-painted dark fantasy environment art while preserving its exact 3:2 landscape proportions and original layout. Retain a snowy mountain pass with a distant jagged gothic fortress on the center-right, steep black slate cliffs on both sides, sparse dead conifers and broken trunks, narrow distant valley, broad low unobstructed foreground.
Composition invariants: same eye level and perspective, same vanishing point near x=53%, y=40%, skyline and major cliff silhouettes in the same places. Bottom 54% of image must remain open gently receding snow-covered dark stone ground, flat enough for a separate 3D battle board to overlay. No big rocks or new silhouettes in that central open area. Keep the terrain transitions near x=15% and x=85%; all 4 edges fully painted with natural continuation for camera motion and cropping, no frame, vignette, blur or transparency.
Style/medium: richly crafted illustrative medieval dark fantasy, traditional gouache/oil brushwork with restrained engraved contour accents, confident rock planes, convincing scale, layered atmospheric depth, more intentional and readable shapes than source, detailed without noisy stippling. Match the serious weathered steel/leather character art and the blackened metal UI. NOT photorealistic, NOT glossy 3D, NOT cartoon.
Palette: muted charcoal slate, cold steel blue, desaturated gray-white snow, subtle earthy rock undertones; preserve original overall luminance so current scene lighting remains compatible. Softer mid-distance contrast behind units, dark textured flanks, distant fortress silhouette clear but subordinate.
Lighting: cold overcast winter, diffuse sky light, credible snow accumulation on ledges and long icicles, mist BETWEEN distant ridges. Avoid frozen waterfall repeating stripes. Weather particles will be added in engine, so no large painted snowflakes or screen effects.
Constraints: environment only, no humans/creatures, no text, no logos, no borders, no grid or battle board, no icons. Preserve the composition and environment identity while substantially upgrading the painting. Output exactly one image, landscape 1536 x 1024.

## Prompt 2

Use case: style-transfer
Asset type: production background painting, second of a matching three-image set for a gothic dark fantasy tactical RPG.
Input images: Image 1 is the EDIT TARGET (original icy chasm scene); preserve its camera geometry and composition precisely. Image 2 is the ALREADY REPAINTED companion environment and authoritative STYLE/PALETTE reference. Image 3 shows actual game UI and character style ONLY; never reproduce UI or units.
Primary request: Repaint image 1 to match the professional hand-painted dark fantasy treatment of image 2. Keep the vast icy chasm across the upper middle, tall dark slate cliff at left, ledge at right, jagged mountains in upper right, distant needle-shaped rock spires and broad open foreground shelf. Preserve all landmarks, broad silhouettes, perspective, horizon, original 3:2 ratio and open battle-board space. Do not convert the chasm into a castle or forest.
Composition: original chasm lip around y=30%, far edge of open foreground around y=49%, bottom half is unobstructed gently receding cold dark stone with thin patches of windblown snow. No foreground obstacles in the central 75%; small rocks confined to outer edges. Entire image painted edge to edge and all margins naturally complete to survive battle camera zoom/tilt/yaw.
Style: skillful gouache and oil environment illustration, strongly designed rock planes, delicate controlled engraved accents, natural snow shelves and icicles with varying shapes, layered soft mountain mist, fine material detail subordinate to bold readable masses. Reduce repetitive sparkling flecks and faceted texture noise. Match image 2's quiet cold gray-blue palette, charcoal rock, restrained warm slate undertones and gray-white snow, subtle atmospheric depth. Hero units will be overlaid in engine.
Lighting: cold diffuse overcast light, similar global luminance to original and image 2. Dark flanks, softer contrast and calm midvalues behind actors. No large snow particles, engine adds weather.
Avoid: people, monsters, structures absent from the source, UI, text, borders, logos, grid, battle board, photorealism, glossy 3D, cartoon, oversaturated blue, bright white snow, vignettes, motion blur.
Deliver exactly one landscape image at 1536 x 1024.

## Prompt 3

Use case: style-transfer
Asset type: production background painting, third of a coherent three-image set for a gothic dark fantasy tactical RPG.
Input images: Image 1 is the EDIT TARGET, original snowy valley with tribal camp on right. Image 2 is the repainted companion environment and authoritative STYLE/PALETTE reference. Image 3 is actual game UI/character integration reference ONLY, do not reproduce it.
Primary request: Repaint image 1 completely in the refined hand-painted style of image 2 while locking original perspective and composition. Preserve tall jagged mountains dominating upper left, layered ridges receding toward center-right, huge brooding overcast sky, bare dead trees on side ledges, and the small existing tribal camp with hide tents, rough wooden palisade and ragged dark banners on the far right ridge, tiny restrained warm torch lights.
Geometry invariants: same 3:2 aspect, eye level, vanishing point near x=58%, y=45%, same mountain silhouettes and camp footprint, same quiet broad foreground. Bottom 52% remains empty dark slate ground with thin windblown snow, naturally receding into the valley, usable beneath a separate 3D tactical board. Small rocks only on outer edges, never add obstructions to the central 75%. Fully paint all margins naturally, no vignette or border, camera can zoom, tilt and yaw.
Style: mature illustrated medieval dark fantasy matching image 2 exactly, crafted oil/gouache brushwork, controlled subtle engraved accents, intentional readable rocky planes and snow shelves, atmospheric perspective, fine but restrained materials. Soft mist separating mountain layers, detailed flanks and calm midvalues behind game units. Remove repetitive stippled noise and screen-wide snowflakes. Engine supplies animated weather.
Palette: same muted cold gray blue, charcoal and slightly earthy slate, restrained gray-white snow; maintain original value range and image 2's color family. Camp fire only small muted amber accent. No new castles, buildings or decorative story objects.
Avoid: people, monsters, UI, text, logos, borders, grid, battle board, cartoon, glossy 3D, photorealism, bright saturated cyan, whiteout, lens flare, shallow depth-of-field.
Deliver exactly one landscape image, 1536 x 1024.

