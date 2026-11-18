# Pulido del Paso Viento Helado

Se conserva la integración del valle aprobada. Hay respaldo de los cuatro archivos modificados al iniciar este pedido en `respaldo/`, y una captura de referencia en `antes.png`.

- Acantilado delantero: material continuo con el suelo, proyección mundial de nieve y roca, borde irregular y cornisa apoyada ligeramente bajo la superficie para evitar una banda superpuesta. La malla visible es independiente del collider.
- Montañas lejanas: menor brillo y contraste, velo de nubes asimétrico y bruma. La misma función de cielo se usa en el panorama y sus transiciones para mantenerlos integrados. Se conserva el asset original.
- Tundra: manchas procedurales de tierra y vegetación apagada, bordes fragmentados y nieve remanente. La distribución es estable por coordenadas de mundo y no usa el azar de campaña.
- Ajuste final solicitado durante la revisión: tierra un 28 % más oscura y más presencia dentro de la misma máscara. Se mantienen las posiciones y el borde suave.

## Verificación

Compilación y shaders correctos; consola sin errores; `check-runtime-deps.ps1` aprobado. Se verificó preparación repetida sin duplicar material o malla; terreno principal, colliders, reloj y RNG intactos; exclusión de la zona del tutorial. No se recorrió el tutorial completo ni se hizo build o benchmark.

Se capturaron seis franjas de iluminación sin avanzar el reloj de campaña. Estas seis capturas muestran el estado anterior al último oscurecimiento leve de la tundra. El editor estaba fuera de Play Mode al ajustar el oscurecimiento final: no se volvió a iniciar la ejecución para tomar otra captura.

- [Mañana](final-manana.png)
- [Mediodía](final-mediodia.png)
- [Amanecer](final-amanecer.png)
- [Atardecer](final-atardecer.png)
- [Crepúsculo](final-crepusculo.png)
- [Noche](final-noche.png)

El editor quedó fuera de Play Mode. No se guardaron escenas ni partidas. No se generaron imágenes nuevas: las modificaciones se realizan en geometría y shaders de Unity.
