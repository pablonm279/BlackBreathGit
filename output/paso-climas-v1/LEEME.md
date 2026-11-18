# Climas visuales del Paso Viento Helado

Se revisaron los cinco estados disponibles en la zona: despejado, lluvia, nieve, niebla y aurora. El calor tiene probabilidad cero en la configuración del Paso; no se agregaron estados ni se alteraron probabilidades.

| Clima | Actualización visual | Partículas medidas, día / noche |
|---|---|---:|
| Despejado | Aire limpio, nieve ambiental existente y destellos aislados sobre superficies válidas | 2 / 1 |
| Lluvia | Gotas frías inclinadas, contacto suave con profundidad e impactos pequeños sobre el suelo | 421 / 418 |
| Nieve | Dos tamaños de copos, grano fino, ruido de movimiento y ráfagas de densidad | 809 / 1021 |
| Niebla | Bancos lentos con bordes orgánicos, profundidad y luminosidad adaptada a la hora | 52 / 68 |
| Aurora | Cortinas animadas verdes y cian con filamentos y un toque violeta, detrás de la geometría; tenue de día | 23 / 29, más una malla de cielo |

Los efectos antiguos se suspenden exclusivamente mientras está activo el controlador del Paso. Se restauran al desactivar el componente o salir de la zona. Los nuevos efectos se reanudan correctamente al volver. Las cantidades escalan según calidad gráfica y la aurora deja de dibujarse cuando no corresponde.

La nubosidad atenúa suavemente el sol y el ambiente del Paso. Las montañas comparten el mismo estado atmosférico para conservar la integración con el mapa. Se mantiene el ajuste de brillo base anterior.

## Archivos

- Nuevos: `ClimasPasoVientoHelado.cs`, `PasoVientoHeladoClimaParticulas.shader`, `PasoVientoHeladoAurora.shader`, con sus `.meta`.
- Cambios mínimos de integración: una llamada en `AmbientePasoVientoHelado.cs`; tres líneas en la rama del Paso de `SunController.cs`; parámetros de nubosidad en `PasoVientoHeladoCielo.cginc`.
- Los respaldos de estos archivos conservan los cambios anteriores del proyecto.

## Validación

- Diez capturas de la escena real: archivos `*-dia-real.png` y `*-noche-real.png`.
- Vista previa de los cinco climas: conserva tipo de clima real, reloj, RNG global y activación de las raíces heredadas. No se aplican efectos jugables del clima previsualizado: las imágenes comparan solo VFX.
- Sin colliders nuevos. Radio de visión conservado.
- Transición nieve → niebla gradual, sin salto instantáneo.
- Desactivar/restaurar/reactivar: visuales nuevos ocultos, renderers y emisiones heredados restaurados, reactivación sin duplicados y partículas reanudadas.
- Alcance: zonas 0, 1 y 3 rechazadas por el controlador. La integración solar solo se aplica al perfil del Paso.
- Shaders compilados; comprobación de dependencias runtime correcta; diff sin errores de espacios.

No se recorrió el tutorial completo. No se modificaron sus secuencias, traducciones, nodos ni reglas. La validación de alcance confirma que el controlador no se agrega a las otras zonas.
