# Canalizador, apariencia 1

Implementado según `ACTUALIZACION_VISUAL_UNIDADES.md`. Modo de generación: imagegen integrado; prompts exactos en `prompts.md`, referencias originales de idle, atento y ataque. Fuentes finales: `{reposo,anticipacion,impacto}-fuente.png` en esta carpeta.

## Assets

- Perfil: `Assets/Resources/AnimacionesIlustradas/CanalizadorApariencia1.asset`.
- Sprites: `Assets/Resources/AnimacionesIlustradas/CanalizadorApariencia1/{reposo,anticipacion,impacto}.png`.
- Preparación: `tools/animation-pilot/prepare_canalizador_apariencia1.py`, con el extractor y descontaminación autorizados.
- RGBA 1024 × 768; Sprite Single, bilineal, Clamp, sin compresión ni mipmaps, alfa activo.
- Escalas fuente: 0.485 / 0.595 / 0.595; centro entre pies fuente: 525 / 625 / 660. Apoyo `(0.5, 12/768)`, corrección horizontal `1024/750`, desplazamiento `0.01`, respiración `0.006`. Registro en `registro.json`.
- Perfil identificado por mover `bfa07ce1d2e7c9445a24d65a2393449b` y ataque `46f5d5375192a5541b801850d2e0d663`. Apariencia alternativa excluida.

Reposo con respiración 20% más lenta fuera del turno, atento original, movimiento original con dash, sin pasos ni sprite nuevo de daño. Preparación con ambas manos cerca del cuerpo y empuje de palmas como impacto. Magia y efectos siguen separados del sprite. AcumularEnergia conserva EnterSkillPoseHold y la salida de la pose al perder Acumulando.

## Sincronización de las descargas

Las habilidades del Canalizador no comparten un preimpacto fijo: DescargaArcana tiene 180 ms propios además de la espera general aplicable y otras descargas tienen esperas distintas. Se añadió una señal visual opcional a UnidadAnimacionIlustrada: esperar y confirmar impacto. Habilidad.Resolver la utiliza únicamente para ClaseCanalizador, después de iniciar la pose y después del await de preimpacto, respectivamente. No se modifican esperas, daño, tiradas ni efectos. El cambio de pose/reinicio limpia esa señal. Las clases anteriores mantienen su cálculo de timing y Habilidad conserva su sprite original.

## Verificación

Compilación runtime y editor: exit 0. Logs en `outputs/animation-pilot/compilacion-canalizador.log` y `compilacion-editor-canalizador.log`.

16 comprobaciones ejecutadas sobre EvaluarFrame/ResolverReposo extraídos del fuente actual, usando dobles simples de Sprite/controlador en `FrameHarness.cs`: espera de señales, instantes distintos, ventana de 0.18 s, recuperación a reposo/atento, habilidad original y timing anterior. Resultado: `validacion-timing.txt`. Esta prueba no ejecuta MonoBehaviour, Update ni el combate de Unity.

Alfa, márgenes, referencias y configuración de importación comprobados; ocho perfiles con parejas únicas. Resultado en `validacion-assets.txt`. Inspección de escala y bordes sobre fondos oscuro, claro y contrastante en las láminas PNG internas; preview HTML sin cambios.

Pendiente en Play Mode: transiciones completas, señales de descarga con efectos reales, interrupciones, acumulación, pausa/inmovilización, muerte, ambas orientaciones y tutorial. El enlace nuevo vive en Habilidad pero solo se activa para Canalizador; no altera estados ni secuencias del tutorial. Aun así falta comprobar visualmente la coincidencia entre descarga y gesto dentro de ese flujo. Entrar de nuevo en Play Mode para cargar el perfil.
