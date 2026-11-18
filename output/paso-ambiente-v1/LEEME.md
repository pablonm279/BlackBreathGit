# Ambiente permanente del Paso Viento Helado

Se reemplazan las corrientes heredadas por dos capas: polvo de nieve rasante emitido sobre superficies válidas y cristales finos suspendidos. Un campo de ráfagas con propagación espacial coordina esas capas, la precipitación y la vegetación. La intensidad aumenta durante la nevada.

Los árboles usan deformación de vértices con base fija, sin mover sus transformaciones ni colliders. Se añaden grupos bajos y dispersos de pizarra y matas secas escarchadas; las matas responden al mismo viento. Se conservan las exclusiones de caminos, nodos y el margen de 3 m de la cornisa. Las luces puntuales de los fuegos del escenario reciben un titilar leve y adaptación diurna; sus alcances permanecen iguales.

## Validación

- C# y shader de vegetación compilados en Unity mediante MCP.
- 138 árboles encontrados con el shader de vegetación durante la prueba.
- Sistemas antiguos de viento, nieve ambiental y niebla base suspendidos mientras funciona el reemplazo.
- Prueba de desactivar/reactivar: raíz oculta y campo de viento desactivado; al reactivar vuelven los dos sistemas sin duplicación. RNG global conservado.
- 76 partículas propias observadas durante una nevada; cantidad variable con las ráfagas y calidad gráfica.
- Sin colliders nuevos en la raíz de corrientes.
- Capturas reales de día y noche: `dia.png`, `noche.png`. Vista previa de hora retirada al finalizar, sin guardar escenas ni partidas.
- Comprobación acotada de margen frontal en los detalles encontrados: sin invasiones.
- Dependencias runtime y revisión de espacios correctas.

La integración está limitada a la zona ID 2. No se modifican las reglas de visión, los costes de caminos ni las secuencias del tutorial. No se recorrió el tutorial completo; esa validación integral queda pendiente.

Las primeras pruebas se interrumpieron por recargas del editor durante Play; la comprobación final de día/noche se ejecutó completa y la consulta de errores posterior no devolvió errores.
