# Procedimiento obligatorio para actualizar visualmente una unidad

Referencia aprobada por el usuario el 7 de septiembre de 2026: **Caballero, apariencia 1, y Driada Quemada**, implementados en Unity. Incluye la corrección final del idle: tamaño consistente con atento y eliminación del borde claro del recorte. El usuario confirmó que quedó bien.

Leer este documento completo antes de cada actualización visual de una unidad. Seguir los pasos en orden y conservar las decisiones de esta guía salvo una indicación posterior del usuario. El objetivo es repetir el resultado visual y funcional aprobado; las medidas se calibran para cada personaje, no se copian indiscriminadamente.

## 1. Identificar la unidad y todas sus poses

1. Revisar `git status --short` y las instrucciones de `AGENTS.md`. Preservar los cambios ajenos.
2. Localizar el prefab, su `UnidadPoseController`, el `Image` que dibuja la unidad y los sprites originales de la apariencia solicitada. Trabajar en `Assets/`, nunca en la copia vieja anidada.
3. Registrar las referencias y GUID de idle, atento/turno activo, mover, atacar, habilidades, daño y cualquier postura especial. Revisar los cambios de apariencia y de postura que se hacen por código.
4. Inspeccionar las imágenes originales antes de generar. Anotar orientación, proporciones, equipo, paleta, tamaño del cuerpo, apoyo de los pies y encuadre real del `Image` en Unity.
5. Identificar las otras apariencias que deben conservarse. El sistema actual reconoce un perfil por las referencias originales de mover y atacar, no por el nombre del sprite. Comprobar que esa pareja no coincida con otra apariencia fuera del pedido.

## 2. Conservar exactamente este comportamiento

| Estado | Resultado requerido |
| --- | --- |
| Idle normal | Nuevo sprite de reposo, respiración y balanceo muy sutiles. Tamaño corporal consistente con las poses originales. Respiración al 80% de velocidad fuera del turno propio y al 100% durante su turno. |
| Atento: su turno o seleccionado como objetivo | Conservar su sprite original diferenciado, con respiración sutil. No reemplazarlo por el nuevo idle. |
| Movimiento | Conservar el sprite original de mover y el dash existente. Añadir impulso, leve inclinación y asentamiento al terminar. **Sin ciclo de caminata ni frames de pasos.** |
| Ataque | Nuevos frames de anticipación e impacto. Sincronizar con el momento de impacto que ya utiliza el combate. Al terminar, resolver la pose de reposo según el estado actual. |
| Recibir daño | **No generar ni asignar un sprite nuevo de daño.** Conservar la lógica y el feedback existentes, incluido su fallback cuando no hay pose propia. No borrar una pose original que otra unidad ya utilice. |
| Habilidades | Conservar cada sprite original y sus reglas de pose sostenida. No taparlos con idle o ataque. |
| Posturas especiales | Conservarlas; por ejemplo, la postura defensiva del Caballero. Restaurarlas correctamente después de actuar. |
| Muerte, congelación, aturdimiento y pausa | Detener el movimiento añadido según el estado, sin saltos de encuadre ni interferencias con la animación original. |

## 3. Preparar las ilustraciones

1. Usar las ilustraciones originales de la apariencia como referencia del generador de imágenes, siguiendo las instrucciones de la herramienta y su skill vigente. Mantener identidad, vestimenta, armas, materiales, paleta y estilo ilustrado del juego.
2. Preparar únicamente los tres sprites necesarios: `reposo`, `anticipacion` e `impacto`. No recrear las poses originales que se conservan en la tabla anterior.
3. Pedir cuerpo completo, separación suficiente entre poses, misma orientación, proporciones consistentes, armas completas y transparencia real. No agregar suelo, sombra de fondo, textos, cuadrículas dibujadas ni efectos luminosos pegados al personaje.
4. Guardar las fuentes y el prompt realmente enviado en una carpeta de trabajo fuera de `Assets`, identificada por unidad y apariencia. No sobrescribir las fuentes del piloto.
5. Inspeccionar el resultado antes de importarlo. Corregir inconsistencias anatómicas, cambios de equipo y elementos cortados antes de continuar.

Plantilla orientativa para el prompt, a completar con las referencias de la unidad:

> Crear tres poses de cuerpo completo del mismo personaje de las referencias: reposo natural, anticipación de su ataque e impacto del ataque. Conservar exactamente identidad, equipo, paleta, materiales, orientación y estilo ilustrado oscuro de las referencias. Mantener tamaño del cuerpo y nivel de detalle consistentes entre poses. Separar las poses, incluir todos los extremos de armas y vestimenta y usar fondo transparente real. Sin caminata, pose de daño, texto, escenario ni contorno blanco.

Esta plantilla es una guía nueva, no una transcripción del prompt histórico: `output/animation-pilot/prompts.md` contiene `undefined` en lugar de los prompts originales y no sirve para recuperarlos.

## 4. Transparencia real y contornos limpios

El usuario autorizó el procesamiento por código para quitar el fondo, recortar y alinear estos sprites. Reutilizar ese procesamiento dentro del alcance autorizado y respetando las instrucciones vigentes de las herramientas.

1. Comprobar el canal alfa: una cuadrícula pintada en una imagen RGB no es transparencia. Si el archivo ya tiene alfa correcto, preservarlo; no pasarlo por una extracción de fondo RGB innecesaria.
2. Cuando haya fondo claro, separar la figura y recuperar el antialias del contorno. Eliminar el color de fondo mezclado con los píxeles del borde, no solamente bajar su opacidad.
3. Revisar también los píxeles opacos del borde: en el piloto el umbral de recorte dejaba residuos claros que luego se veían como un halo en Unity.
4. La corrección aprobada reconstruye el color del borde a partir del interior próximo y estima su cobertura contra el fondo claro. Actúa sobre una franja estrecha; conserva el detalle interior. Está implementada en `extract()` de [prepare_sprites.py](tools/animation-pilot/prepare_sprites.py).
5. Adaptar la extracción al color y silueta de cada unidad. Los umbrales del piloto suponen fondo casi blanco y contornos oscuros: no aplicarlos ciegamente a pelo blanco, alas, transparencias, armas finas o efectos brillantes.
6. Inspeccionar las poses sobre fondos opacos oscuro, claro y de un color contrastante, al tamaño de juego y ampliadas. Revisar pelo, capa, manos, espada, ramas y huecos internos. No aceptar halos continuos, cuadrícula residual ni pérdida de detalles finos.

Si el PNG compuesto se ve limpio pero Unity muestra un halo, revisar importación, filtrado, compresión, material y capas que duplican la imagen. Corregir la causa comprobada; no desactivar efectos generales para ocultar el problema.

## 5. Igualar tamaño y registrar los pies

1. Comparar el **cuerpo** con las poses originales de atento, movimiento y habilidad: cabeza, torso y distancia de la cabeza a los pies. No normalizar por el rectángulo total, porque la espada, ramas o una capa extendida cambian sus dimensiones.
2. Usar un lienzo común por perfil. El piloto utiliza PNG RGBA de `1024 × 768`, con margen transparente y el apoyo de los pies registrado entre frames.
3. Escalar alrededor del apoyo, mantener la línea del suelo y ajustar el centro horizontal. Evitar saltos al pasar entre una pose original y una nueva.
4. Tener en cuenta que el `Image` del prefab puede estirar la imagen y que el motor ilustrado aplica una corrección horizontal solo a los sprites nuevos. No alterar el tamaño del prefab o de la unidad completa para corregir una única pose.
5. Comparar en Unity con la misma cámara y escala. La respiración no debe ocultar una diferencia permanente de tamaño.

Valores finales del piloto, como referencia verificable:

| Parámetro | Caballero apariencia 1 | Driada Quemada |
| --- | --- | --- |
| Escala del recorte fuente | `1.46` | `1.14` |
| Ajuste adicional solo del idle | `× 1.08`, escala final `1.5768` | `× 1`, sin aumento |
| Apoyo del perfil | `(0.5, 0.074)` | `(0.5, 0.012)` |
| Corrección horizontal del perfil | `1024 / 750` | `1024 / 750` |
| Desplazamiento horizontal del perfil | `0.05` | `-0.01` |
| Amplitud de respiración | `0.006` | `0.009` |

El aumento del **8%** resolvió el idle pequeño del Caballero, manteniendo los pies en su sitio. No es una regla universal para nuevas unidades. La escala depende del recorte fuente y la corrección horizontal depende del encuadre original.

## 6. Importar e integrar en Unity

1. Guardar únicamente los tres PNG utilizables en `Assets/Resources/AnimacionesIlustradas/<UnidadApariencia>/`. Mantener fuentes, pruebas y descartes fuera de `Assets/Resources`.
2. Importar como Sprite, modo Single, alfa activado y `alphaIsTransparency`, sin mipmaps, filtro bilineal y wrap Clamp. El piloto usa compresión desactivada y tamaño máximo `1024`. Revisar los overrides de plataforma si aparecen artefactos.
3. Mantener los `.meta` y GUID al corregir un PNG existente. No romper las referencias de los perfiles.
4. Crear un `PerfilAnimacionIlustrada` para la apariencia, con las referencias originales y los tres sprites nuevos; calibrar apoyo, corrección horizontal, desplazamiento y respiración.
5. Reutilizar [PerfilAnimacionIlustrada.cs](Assets/Scripts/Visual/PerfilAnimacionIlustrada.cs), [UnidadAnimacionIlustrada.cs](Assets/Scripts/Visual/UnidadAnimacionIlustrada.cs) y la integración existente en [UnidadPoseController.cs](Assets/Scripts/UnidadPoseController.cs). El motor carga los perfiles de `Resources/AnimacionesIlustradas` y se incorpora al `Image` de las unidades coincidentes. Normalmente una nueva unidad necesita assets y perfil, sin duplicar el motor ni editar escenas.
6. Mantener los tiempos de combate. El ataque usa `MeleeTimingUtility.CalcularPreImpactoMs()` y sostiene el impacto `0.18 s` antes de resolver el reposo actual. El asentamiento después del dash dura `0.18 s`.
   - Excepción implementada para el Canalizador: sus descargas tienen esperas propias. `Habilidad.Resolver` llama a `EsperarImpactoHabilidad` después de iniciar la pose y a `ConfirmarImpactoHabilidad` tras la espera real, justo antes de aplicar efectos. El motor sostiene anticipación hasta esa señal y luego muestra impacto durante `0.18 s`. No sustituir esos tiempos por el cálculo melee ni modificar el daño/esperas. Las demás clases conservan su comportamiento anterior; las poses de habilidad sostenida no usan esta señal.
7. Aplicar el movimiento añadido sobre los vértices de la ilustración alrededor del apoyo. No mover la raíz de la unidad, las casillas, puntos de impacto, barras, indicadores ni UI del tutorial.
8. Conservar los bloqueos de habilidades, la restauración de atento/posturas y el feedback original de daño. No consumir `Random` del combate para efectos visuales; el motor existente usa una fase derivada de la instancia.
9. Entrar de nuevo en Play Mode al añadir perfiles para renovar la caché estática. Verificar también las apariencias que deben quedar fuera del cambio.

10. Conservar la regla global de respiración solicitada después del piloto: `UnidadAmbientMotionFx.ObtenerFactorRespiracion` devuelve `0.8` fuera del turno de la unidad y `1` en su turno (jugadores y enemigos). Ser seleccionado como objetivo no equivale a estar en turno. Sin batalla conserva la velocidad normal. Tanto el motor ilustrado como la silueta ambiental integran un reloj de respiración independiente: no multiplicar el tiempo absoluto al cambiar de turno ni alterar los relojes de ataque/dash. La pausa y los estados que inmovilizan detienen el avance de ese reloj.

## 7. Uso correcto de las herramientas del piloto

### IA sin frames nuevos: tratamiento centralizado

- Las unidades con `IAUnidad` reutilizan `UnidadAnimacionIlustrada` aunque no tengan perfil. Conservan todos sus sprites originales. Un perfil existente tiene prioridad (incluida Driada Quemada), sin agregar un segundo motor.
- `Unidad.esInmobil` excluye el efecto de la IA. No confundir esa propiedad permanente con el estado temporal `estado_inmovil`, que impide desplazarse pero no respirar. Voladores y etereos moviles tambien reciben el efecto.
- Idle y atento usan respiracion de amplitud `0.006`, con el mismo reloj al 80% fuera del turno propio. Dash, asentamiento y deformacion del ataque reutilizan las curvas existentes. No se generan frames ni se altera la duracion de las poses o del combate.
- El apoyo de la IA sin perfil se obtiene del borde inferior de su geometria real; no se aplica correccion horizontal ni escala permanente. Solo cambia la malla de la ilustracion.
- Los prefabs antiguos sin `UnidadPoseController` se incorporan desde `UnidadAmbientMotionFx.Start`: observan `movimientoEnCurso` y los estados del Animator original. No se les agrega un controlador de poses. Los estados distintos de idle, turno nuevo y ataque quedan sin deformacion adicional para conservar su feedback.
- Las habilidades y el daño mantienen sus sprites y reglas originales. Una pose ausente conserva el sprite actual pero comunica el cambio de estado al motor. Se respeta una sustitucion externa de sprite hasta la siguiente orden del controlador.
- En la IA sin perfil se oculta exclusivamente `BreathingRim`, la copia secundaria que podria formar un borde alrededor de la figura animada. La sombra de contacto se conserva.
- Prueba acotada del motor real con dobles de Unity: `tools/animation-pilot/test_ia_motion.ps1`. No reemplaza la comprobacion visual en batalla ni la del tutorial (incluido PerroAdiestradoTutorial).

### Excepcion solicitada: Explorador y pose de preparacion

- Explorador apariencias 1 y 2 incorporan un cuarto sprite opcional `preparacion`: agachado ambiguo para **Improvisar Flechas** y **Acechar**. Es una excepcion expresa del usuario a conservar todas las poses de habilidad.
- `Habilidad.Resolver` solicita `PlayPreparationPose()` exclusivamente para esas dos habilidades. Solo se activa si el perfil de la apariencia tiene ese sprite; el resto conserva su comportamiento, incluida la omision de animacion de Improvisar Flechas sin perfil.
- La pose dura el tiempo de habilidad configurado, conserva el refresco por turno/objetivo y vuelve al reposo que corresponda. No queda sostenida durante toda la duracion del buff. No cambia AP, efectos, esperas ni final de turno.
- Mantener separado el ataque de daga (`poseAtacar`, nuevos anticipacion/impacto) del disparo con arco (`poseHabilidad`, original). No sustituir el arco por la daga al ampliar esta unidad.
- La pose opcional usa la misma correccion de encuadre que los nuevos frames. Al cambiar apariencia se descarta la sustitucion temporal; el sprite de habilidad original nunca se sobrescribe.

- [prepare_sprites.py](tools/animation-pilot/prepare_sprites.py) documenta la preparación reproducible de los dos personajes iniciales. Requiere Pillow, NumPy y SciPy.
- Su extractor actual **espera seis figuras** en una lámina con una disposición concreta; luego descarta pasos y daño. Para una nueva lámina de tres poses hay que adaptar el extractor y el mapeo de poses, no generar contenido descartado solo para satisfacer ese supuesto.
- También contiene rutas, escalas y perfiles específicos del piloto. Preparar la nueva unidad con selección explícita de destino; no ejecutar una regeneración general que sobrescriba personajes ya aprobados.
- `--idle-only` regenera los dos PNG de reposo del piloto, incluida la limpieza de bordes y el aumento del Caballero. Conserva perfiles, metas y preview. No es un selector para una unidad nueva.
- La ejecución sin ese argumento también reescribe perfiles, metas y una lámina de revisión. Revisar su alcance antes de usarla.
- [build_preview.py](tools/animation-pilot/build_preview.py) y `output/animation-pilot/comparacion.html` son auxiliares históricos. El usuario pidió implementar directamente en Unity y dejar de actualizar la preview. **No regenerarla ni abrir una ronda de preview salvo que la pida.**

## 8. Validar antes de darlo por terminado

1. Verificar PNG RGBA, transparencia real, ausencia de recortes contra los límites, referencias válidas y ausencia de frames de pasos o daño en el perfil.
2. Revisar el diff de los archivos tocados. Si se cambió C#, compilar y ejecutar una comprobación acotada de las transiciones afectadas. El menú `Tools > Arte > Validar animaciones Caballero y Driada`, definido en [ValidacionAnimacionIlustrada.cs](Assets/Editor/ValidacionAnimacionIlustrada.cs), cubre el piloto; ampliarlo si hace falta validar nuevos perfiles. No asumir que ya prueba cualquier unidad nueva.
3. Comprobar en batalla idle → atento por turno, atento por selección como objetivo y vuelta a idle. Comparar tamaño corporal y pies en esas transiciones.
4. Comprobar dash de ida y llegada, ataque y recuperación, daño durante distintos estados, cada habilidad y postura especial. Verificar ambas orientaciones cuando correspondan.
5. Comprobar pausa, inmovilización, muerte y otra apariencia fuera del pedido. Confirmar que los indicadores, la raíz y los tiempos de combate no se alteraron.
6. Revisar las partes del tutorial que usen la unidad o el controlador compartido si se cambió su comportamiento. Si no se puede ejecutar esa prueba, declarar exactamente lo pendiente; no afirmar que el tutorial fue validado.
7. Informar qué se implementó y qué se comprobó realmente. La compilación o una imagen estática no prueban por sí solas la animación en batalla.

## Criterios de aceptación

- Idle con vida sutil, contorno limpio y tamaño consistente con atento y las otras poses.
- Pies estables, sin saltos de encuadre ni cambios involuntarios de orientación o proporciones.
- Dash original con impulso y asentamiento, sin caminar.
- Ataque con anticipación e impacto sincronizados con el combate.
- Atento, habilidades, posturas y daño respetan sus reglas originales; ningún sprite nuevo de daño.
- Perfil limitado a la unidad y apariencia solicitadas, sin efectos en otras unidades ni en la lógica del tutorial.
- Resultado implementado en Unity y limitaciones de verificación expresadas con claridad.

El cierre del piloto incluye la aprobación visual del usuario en juego tras corregir el idle. Esa aprobación es la referencia de calidad; no equivale a una ejecución completa automatizada del combate o del tutorial.
