# Acechador, apariencia 2

Implementado siguiendo `ACTUALIZACION_VISUAL_UNIDADES.md`. Generación con `imagegen` integrado; prompts exactos en `prompts.md` y corrección en `correccion-impacto.md`. Referencias: idle, atento y ataque de `Assets/Scripts/Clases/Acechador/Aparienciasalternativas`. El impacto inicial cambió la espada de mano; fue corregido con el generador y el descarte queda fuera de Assets.

## Resultado

- Perfil: `Assets/Resources/AnimacionesIlustradas/AcechadorApariencia2.asset`.
- Tres sprites: `Assets/Resources/AnimacionesIlustradas/AcechadorApariencia2/{reposo,anticipacion,impacto}.png`.
- Fuentes finales: `{reposo,anticipacion,impacto}-fuente.png` en esta carpeta.
- Preparación reproducible: `tools/animation-pilot/prepare_acechador_apariencia2.py`.
- RGBA 1024 × 768; Sprite Single, alfa activo, bilineal, Clamp, sin compresión ni mipmaps. Los tres sprites finales usan extracción y descontaminación autorizadas; la fuente de anticipación inicial con alfa real fue reemplazada a pedido del usuario.
- Escalas: reposo 0.607, anticipación 0.655, impacto 0.64. Apoyo `(0.5, 8/768)`, corrección horizontal `1024/750`, desplazamiento `-0.025`, respiración `0.006`. Registro completo en `registro.json`.
- Altura de reposo 719 px frente a unos 713 px de atento a escala equivalente. Se conservan proporciones de cabeza/cuerpo; la flexión de rodillas se contempla al calibrar anticipación.

El motor existente reconoce exclusivamente mover `f6a4df1b2ba74584f908ba50054e32ce` y atacar `a490f7f9b7093904b9655a800c4ff69e`. Conserva atento, movimiento/dash y habilidad originales, sin pasos ni sprite nuevo de daño. Mantiene respiración al 80% fuera de turno y al 100% durante el turno propio, preimpacto 0.42 s y ventana de impacto 0.18 s.

No se modificaron C#, prefabs, escenas, ocultación ni lógica del tutorial. La apariencia 1 queda con su perfil anterior. No se actualizó la preview HTML.

## Verificación

Inspección de identidad, equipo, mano de espada y contornos sobre fondos oscuros, claros y contrastantes. Comparación estática de escala. La lámina de revisión inicial precede a la reducción final de anticipación de 0.588 a 0.579, que añadió margen para la espada sin alterar su dibujo.

`validacion-assets.txt`: tres PNG RGBA, márgenes transparentes, importación y referencias correctas, pareja mover/atacar única entre siete perfiles y coincidencia con Apariencia2 del prefab.

Pendiente: ejecución en Play Mode de las transiciones idle/atento, dash, ataque, habilidad, daño, ocultación, pausa, inmovilización y muerte; ambas orientaciones y tutorial. No se ejecutaron ni se dan por validadas esas pruebas. El riesgo pendiente es visual (encuadre y transiciones durante combate); no se cambió lógica del tutorial. Volver a entrar en Play Mode para renovar la caché de perfiles.

## Corrección: preparación baja

Se reemplazó la anticipación con espada en alto por una preparación compacta a la altura del torso, en la misma mano que el impacto. Prompt exacto de imagegen integrado en `correccion-anticipacion-baja.md`. Nueva fuente: `anticipacion-fuente.png`; antigua: `anticipacion-alta-descartada.png`. Escala 0.655, centro entre botas fuente x=762 y apoyo final y=760.

Se modificó únicamente `anticipacion.png` entre los assets de Unity. Verificados por SHA256 sin cambios: reposo, impacto, perfil y meta/GUID de anticipación. RGBA y márgenes comprobados, comparación sobre tres fondos en `revision-anticipacion-baja.png`. Los controles visuales anteriores con espada elevada son históricos. Pendiente comprobar la transición en Play Mode; no se modificó lógica ni tutorial.
