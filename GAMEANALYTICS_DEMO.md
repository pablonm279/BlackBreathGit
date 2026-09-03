# GameAnalytics para la demo

Esta guía corresponde a la integración actual del proyecto con GameAnalytics SDK 8.0.1 y Windows Standalone.

## 1. Cuenta y juego en GameAnalytics

1. Entrar en <https://tool.gameanalytics.com/> y crear una organización, un estudio y un juego.
2. Añadir la plataforma **Windows** al juego.
3. En la configuración del juego, copiar `Game Key` y `Secret Key` de Windows.
4. En Unity abrir **Tools > Metrics > Open GameAnalytics Settings** y comprobar la plataforma Windows y sus dos claves.
5. Abrir **Tools > Metrics > Validate Setup**. La consola debe indicar que Windows está listo.
6. Mantener `Player Settings > Version` sincronizada con cada build público. El tooling copia esa versión al campo Build de GameAnalytics.

Las claves ya están cargadas en este checkout. No pegarlas en documentación pública, capturas ni formularios de feedback.

## 2. Qué se envía

GameAnalytics sólo se inicia después de que la persona acepta telemetría. Si elige continuar sin telemetría, no se envían eventos.

Dimensiones disponibles para filtrar todos los informes:

| Dimensión de GameAnalytics | Significado | Valores |
| --- | --- | --- |
| `custom_01` | Idioma | `es`, `en`, `pt` |
| `custom_02` | Dificultad | `difficulty_0` a `difficulty_4` |
| `custom_03` | Ruta de la demo | `standard`, `tutorial`, `continue`, `unknown` |

Eventos principales:

| Objetivo | Tipo | Jerarquía o ejemplo |
| --- | --- | --- |
| Inicio de campaña | Progression | `campaign:new_game` |
| Tutorial iniciado realmente | Design | `tutorial:started:vertical_slice_intro` |
| Tutorial completo | Progression | `tutorial:campaign:intro` |
| Tutorial finalizado realmente | Design | `tutorial:completed:vertical_slice_intro` |
| Paso alcanzado del tutorial | Design | `tutorial:step_reached:vertical_slice_intro:<paso>` |
| Tutorial omitido | Design | `tutorial:skipped:vertical_slice_intro:<paso>` |
| Batalla iniciada/ganada/perdida | Progression | `battle:<tipo>:<encuentro>` |
| Jefe de zona | Progression | `boss:<zona>:<fase>` |
| Resumen de batalla | Design con valor | `battle:summary:<resultado>:<métrica>:<tipo>` |
| Uso de habilidades | Design con valor | `combat:ability_used:<bando>:<habilidad>` |
| Economía | Resource | monedas `gold` y `materials` |
| Rendimiento gratuito | Design con valor | `performance:fps:average` y `performance:memory:*` |
| Errores | Error automático del SDK | máximo 10 por sesión |

Los golpes, estados, buffs y usos de habilidades se registran individualmente en el JSONL local para diagnóstico. A GameAnalytics se envía un total por combinación y por batalla. Para esos eventos hay que consultar **Sum of amount/value**, no sólo el número de eventos.

Los archivos locales quedan en:

`Application.persistentDataPath/Metrics/events-AAAAMMDD.jsonl`

## 3. Validación antes de entregar el build

GameAnalytics no recibe eventos desde el Editor en esta integración. La prueba debe hacerse con un build Windows.

1. Crear un build de prueba con una versión reconocible, por ejemplo `0.75-demo1`.
2. Ejecutarlo, aceptar telemetría y empezar una partida tutorial.
3. Llegar al menos hasta dos pasos del tutorial y comenzar una batalla.
4. Cerrar el juego normalmente.
5. En la web abrir **Realtime > Live Events**. Los eventos suelen aparecer en aproximadamente un minuto.
6. Confirmar que los eventos muestran Build, `custom_01`, `custom_02` y `custom_03` correctos.
7. Confirmar en **Game Overview / Integrations** que no existan avisos de eventos rechazados o límites.
8. Repetir el inicio eligiendo continuar sin telemetría y comprobar que esa ejecución no aparece.

## 4. Dashboard recomendado: `Demo - Crucial`

En **Dashboards > Create**, crear un dashboard para este juego y añadir estos widgets:

1. **Jugadores de la demo**: métricas predefinidas `New Users`, `DAU`, `Sessions`, `Playtime per user` y `Playtime per session`.
2. **Retención**: D1 y D7. D1 es la señal útil durante festivales o playtests de varios días.
3. **Ruta elegida**: Design Events, `Unique Users`, filtro `ID1 = ui`, `ID2 = main_menu`; separar por `ID3`. Los valores esperados son `new_game`, `new_game_tutorial` y `continue_game`.
4. **Resultado de batallas**: Progression Events, filtrar `progression01 = battle`; separar por `status` y agrupar por `progression02` o `progression03`.
5. **Duración de batalla**: Design Events, `Mean amount per event`, filtros `ID1 = battle`, `ID2 = summary`, `ID4 = duration_seconds`; separar por `ID3` para victoria/derrota/abandono.
6. **Rondas por batalla**: igual al anterior, usando `ID4 = rounds`.
7. **Habilidades más usadas**: Design Events, `Sum of amount`, filtros `ID1 = combat`, `ID2 = ability_used`, `ID3 = ally`; agrupar por `ID4`.
8. **Economía**: usar el dashboard predefinido **Resource** y revisar fuentes/sumideros de `gold` y `materials`.
9. **FPS medio**: Design Events, `Mean amount per event`, filtros `ID1 = performance`, `ID2 = fps`, `ID3 = average`; separar por Build.
10. **Errores**: dashboard predefinido **Health**, con `Error count`, `Affected users` y separación por Build.

En todos los widgets conviene añadir el filtro de Build de la demo pública. Usar las dimensiones personalizadas para comparar idioma, dificultad y tutorial frente a partida estándar.

## 5. Funnels recomendados

### Tutorial

Crear un **Standard Funnel** con `Unique Users` y estos hitos Design, en orden:

1. `ui:main_menu:new_game_tutorial`
2. `tutorial:started:vertical_slice_intro`
3. `tutorial:step_reached:vertical_slice_intro:intro`
4. `tutorial:step_reached:vertical_slice_intro:prebatalla1`
5. `tutorial:step_reached:vertical_slice_intro:postbatalla1`
6. `tutorial:step_reached:vertical_slice_intro:abrirmejoras`
7. `tutorial:step_reached:vertical_slice_intro:exploracion1`
8. `tutorial:step_reached:vertical_slice_intro:prebatallafinal`
9. `tutorial:step_reached:vertical_slice_intro:postbatfinal1`
10. `tutorial:completed:vertical_slice_intro`

Filtrar `custom_03 = tutorial`. Si hay caída entre dos hitos, crear temporalmente otro funnel con los pasos intermedios de esa sección.

### Campaña y combate

- Crear un **Progression Funnel** para `campaign:new_game` y revisar Start/Complete/Fail.
- En el dashboard Progression, comparar Start/Complete/Fail de `battle:*` por encuentro. Una tasa alta de Fail o abandono, junto con más rondas y duración, señala un pico de dificultad.

## 6. Qué ofrece el plan gratuito

El plan gratuito actual no tiene límite de MAU e incluye dashboards predefinidos y personalizados, Design Events, Funnels, Realtime, Explore, Cohorts, Retention y Health Errors.

Las funciones Pro que no hacen falta para esta demo incluyen granularidad horaria, informes programados, dashboards compartidos entre varios juegos, distribuciones y el panel avanzado de Health para FPS/memoria/boot. Este proyecto envía FPS y memoria como Design Events para que sigan siendo consultables en el plan gratis.

Límites operativos relevantes:

- 500 eventos por jugador activo y día.
- Cardinalidad diaria: 15.000 Design, 8.000 Progression y 4.000 Resource.
- Realtime gratuito muestra las últimas 24 horas y los últimos 50 eventos en Live Events.

No incluir nombres, correos, texto libre del jugador, IDs de Steam ni ningún otro dato personal dentro de los IDs de eventos.

## Referencias oficiales

- Unity SDK: <https://docs.gameanalytics.com/event-tracking-and-integrations/sdks-and-collection-api/game-engine-sdks/unity/>
- Eventos y límites: <https://docs.gameanalytics.com/event-tracking-and-integrations/data-retention-and-limits/event-tracking-and-cardinality-limits/>
- Dashboards: <https://docs.gameanalytics.com/products-and-features/analytics-iq/dashboards/overview/>
- Funnels: <https://docs.gameanalytics.com/products-and-features/analytics-iq/funnels/>
- Precios y funciones: <https://www.gameanalytics.com/pricing>
