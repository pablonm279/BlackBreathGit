# Propuesta de naturalización de textos (tanda 1: eventos y logs de campaña)

Estado: **APLICADO** (ítems A1-A23, mayúsculas puntuales y B1-B10; los patrones de la sección C y el lote
general de mayúsculas quedaron sin aplicar). Se renombraron las claves en `TRADU.cs` (EN/PT) y `zh-Hans.json`;
las traducciones chinas se conservaron tal cual. Texto original de la propuesta:
Cambiar el español cambia la clave de traducción, así que cada ítem implica renombrar la clave en
`TRADU.cs` (EN y PT-BR) y en `Assets/Resources/Localization/zh-Hans.json`. Por la regla de `AGENTS.md`
queda a la espera de confirmación. Cuando se apruebe, se aplica el español y la clave en un solo paso.

Criterio: intervención mínima, se conserva la voz, longitud casi igual (en varios casos queda más corta).
Donde dice "EN/PT sin cambios" solo se renombra la clave y la traducción actual sigue sirviendo.

## Ya aplicado (no toca TRADU)

- `EventosAdmin.cs:1776` — `tareas atrásadas` → `tareas atrasadas`. TRADU ya tenía la clave sin tilde
  (funcionaba por el respaldo "sin tildes"); ahora coincide exacta y en español se ve bien.

## A. Eventos (`EventosAdmin.cs`)

### Errores claros

| # | Línea | Hoy | Propuesta ES | EN | PT-BR |
|---|---|---|---|---|---|
| A1 | 268 | `-Luchar con los Bandidos.` | `-Luchar contra los Bandidos.` | sin cambios | sin cambios |
| A2 | 2065 | `Has llegado a un hermoso claro natural que parece no haber sido manchado por la corrupción y la pestilencia en lo mas mínimo.` | `Has llegado a un hermoso claro que la corrupción y la pestilencia parecen no haber tocado.` | `You have arrived at a beautiful clearing that corruption and pestilence seem to have left untouched.` | `Você chegou a uma bela clareira que a corrupção e a pestilência parecem não ter tocado.` |
| A3 | 1173 | `Nadie parece notarlo, o darle importancia alguna y pasan a su lado como si nada.` | `Nadie parece notarlo, o nadie le da importancia: pasan a su lado como si nada.` | `No one seems to notice, or to care, and they walk past as if nothing had happened.` | `Ninguém parece notar, ou ninguém se importa: passam ao lado como se nada tivesse acontecido.` |
| A4 | 2121 | `…atado a sus patas. En él se indica el punto exacto al que la caravana deberá dirigirse para encontrarse con el equipo de salvamento, junto con los recursos cedidos por la ciudad de Serria.` (se lee como si la nota indicara los recursos) | `El ave mensajera regresa con un mensaje atado a las patas. Indica el punto exacto donde la Caravana deberá encontrarse con el equipo de salvamento y los recursos cedidos por la ciudad de Serria.` | `The messenger bird returns with a message tied to its legs. It marks the exact spot where the Caravan must meet the rescue team and the resources granted by the city of Serria.` | `A ave mensageira volta com uma mensagem amarrada às patas. Ela indica o ponto exato onde a Caravana deverá encontrar a equipe de salvamento e os recursos cedidos pela cidade de Serria.` |
| A5 | 2142 | `Se han entregado ` + N + ` suministros. +25 Esperanza. +20 Materiales y 200 Oro y un nuevo personaje se suma a la caravana` | clave 1 sin cambios; clave 2: ` Suministros. +25 Esperanza, +20 Materiales, +200 Oro. Un nuevo personaje se suma a la Caravana.</b></color>` | **Hoy la clave 1 dice `The caravan won `** (error). Clave 1: `Delivered `; clave 2: ` Supplies. +25 Hope, +20 Materials, +200 Gold. A new character joins the Caravan.</b></color>` | clave 1 sin cambios (`Foram entregues `); clave 2: ` Suprimentos. +25 Esperança, +20 Materiais, +200 de Ouro. Um novo personagem se junta à Caravana.</b></color>` |

### Frases que dan vueltas

| # | Línea | Hoy | Propuesta ES | EN | PT-BR |
|---|---|---|---|---|---|
| A6 | 352 | `…un Río con buen caudal y agua que parece decente. Varios civiles entusiasmados comienzan a dirigirse hacia él con la intención de recrearse y refrescarse.` | `…un Río de buen caudal y agua que parece limpia. Varios Civiles, entusiasmados, se dirigen hacia él para refrescarse.` | `The Caravan finds a river with a strong current and water that looks clean. Several Civilians, excited, head toward it to cool off.` | `A Caravana encontra um Rio de bom fluxo e água que parece limpa. Vários Civis, animados, vão até lá para se refrescar.` |
| A7 | 392 | `-Debes intervenir en apoyo a uno de los dos. El otro obtendrá Baja Moral por 5 días. Apoyas a:` | `-Debes apoyar a uno de los dos. El otro obtendrá Baja Moral por 5 días. Apoyas a:` | `-You must side with one of the two. The other will gain Low Morale for 5 days. You back:` | `-Você deve apoiar um dos dois. O outro receberá Moral Baixa por 5 dias. Você apoia:` |
| A8 | 612 | `…y el viento empuja las llamas hacia la retaguardia.` (repite el título) | `…y el viento empuja las llamas hacia ella.` | `…and the wind drives the flames toward it.` | `…e o vento empurra as chamas em sua direção.` |
| A9 | 828 | `Puedes ordenar hacer una breve parada hasta que el aire se despeje o forzar la marcha para no perder tiempo.` | `Puedes ordenar una breve parada hasta que el aire se despeje o forzar la marcha para no perder tiempo.` | `You can order a short stop until the air clears, or push on to avoid losing time.` | `Você pode ordenar uma breve parada até o ar clarear ou forçar a marcha para não perder tempo.` |
| A10 | 864 | `…</b> junto con algunos Civiles comienzan a contar chistes y anécdotas divertidas, riendo y disfrutando del momento.` | `…</b>, junto con algunos Civiles, comienzan a contar chistes y anécdotas, riendo y disfrutando del momento.` | `…</b>, along with some Civilians, start telling jokes and stories, laughing and enjoying the moment.` | `…</b>, junto com alguns Civis, começam a contar piadas e histórias, rindo e aproveitando o momento.` |
| A11 | 865 | `…y por un breve instante, el peso de la situación parece desvanecerse.` | `…y por un instante el peso de la situación parece desvanecerse.` | `…and for a moment the weight of the situation seems to fade.` | `…e por um instante o peso da situação parece se dissipar.` |
| A12 | 1177 / 1181 | `…Pero puedes optar por ordenarle que guarde sus fuerzas para futuros peligros del camino.` | `…Pero puedes ordenarle que guarde sus fuerzas para los peligros del camino.` | `…But you can order them to save their strength for the dangers of the road.` | `…Mas você pode ordenar que guarde suas forças para os perigos do caminho.` |
| A13 | 1231 | `Dos miembros de la caravana parecen tener opiniones encontradas.` (segundo "parecen" seguido) | `Dos miembros de la Caravana tienen opiniones encontradas.` | `Two members of the Caravan hold opposing views.` | `Dois membros da Caravana têm opiniões opostas.` |
| A14 | 1304 | `Si decides seguir el rastro, se intentará encontrar un Atajo.` | `Si decides seguir el rastro, intentarás encontrar un Atajo.` | `If you decide to follow the trail, you will try to find a Shortcut.` | `Se decidir seguir o rastro, você tentará encontrar um Atalho.` |
| A15 | 1933 | `Si decides seguirlo, se intentará encontrar un Atajo.` | `Si decides seguirlo, intentarás encontrar un Atajo.` | `If you decide to follow it, you will try to find a Shortcut.` | `Se decidir segui-lo, você tentará encontrar um Atalho.` |
| A16 | 1486 | `Puedes extender lo salvable junto al fuego…` (los botones dicen "secarlas") | `Puedes secar lo salvable junto al fuego antes de partir o desecharlo y seguir adelante.` | `You can dry what can be saved by the fire before leaving, or throw it out and move on.` | `Você pode secar o que for aproveitável junto ao fogo antes de partir, ou descartar tudo e seguir em frente.` |
| A17 | 2082 | `…Notas que los civiles están desorganizados y necesitan liderazgo…` | `Has llegado a un pequeño asentamiento. Los Civiles están desorganizados y necesitan liderazgo para sobrevivir al Aliento Negro.` | `You have arrived at a small settlement. The Civilians are disorganized and need leadership to survive the Black Breath.` | `Você chegou a um pequeno assentamento. Os Civis estão desorganizados e precisam de liderança para sobreviver ao Respiro Negro.` |
| A18 | 2083 | `De 15-25 Civiles se unirán a la Caravana y brindarán 50-60 Suministros, 6-8 Materiales, 2-4 Bueyes y 60-70 Oro.` | `Se unirán 15-25 Civiles a la Caravana y aportarán 50-60 Suministros, 6-8 Materiales, 2-4 Bueyes y 60-70 Oro.` | `15-25 Civilians will join the Caravan and bring 50-60 Supplies, 6-8 Materials, 2-4 Oxen and 60-70 Gold.` | `15-25 Civis se juntarão à Caravana e trarão 50-60 Suprimentos, 6-8 Materiais, 2-4 Bois e 60-70 de Ouro.` |
| A19 | 2102 | `…recursos naturales, los civiles se han puesto a recolectar lo que han podido.` (coma entre dos oraciones) | `Has llegado a un lugar rico en recursos naturales. Los Civiles recolectaron lo que pudieron.` | `You have arrived at a place rich in natural resources. The Civilians gathered what they could.` | `Você chegou a um lugar rico em recursos naturais. Os Civis recolheram o que puderam.` |
| A20 | 2105 / 2107 | `Se conseguirán de 25-40 Materiales y 60-85 Suministros.` (y la variante 18-30 / 70-110) | `Se conseguirán 25-40 Materiales y 60-85 Suministros.` | `You will get 25-40 Materials and 60-85 Supplies.` | `Você conseguirá 25-40 Materiais e 60-85 Suprimentos.` |
| A21 | 2135 | `…te has encontrado con el equipo… Enseguida saludan a la caravana… que han traído… Inmediatamente los ánimos mejoran en la caravana al ver que no están solos en esta lucha.` | `Has llegado al lugar señalado por el ave mensajera y te encuentras con el equipo de salvamento enviado por la Ciudad Puerto de Serria.\nEnseguida saludan a la Caravana y comienzan a descargar los recursos que trajeron para ayudarles en su travesía.\n\nLos ánimos mejoran de inmediato: ya no están solos en esta lucha.\n` | `You have reached the place marked by the messenger bird and meet the rescue team sent by the Port City of Serria.\nThey greet the Caravan right away and begin unloading the resources they brought to aid its journey.\n\nSpirits lift at once: they are no longer alone in this fight.\n` | `Você chegou ao lugar indicado pela ave mensageira e encontra a equipe de salvamento enviada pela Cidade Portuária de Serria.\nLogo cumprimentam a Caravana e começam a descarregar os recursos que trouxeram para ajudá-los na travessia.\n\nO ânimo melhora de imediato: eles não estão mais sozinhos nesta luta.\n` |
| A22 | 2124 | `Se ha marcado en el camino adelante el nodo al cual deberás dirigirte para encontrarte con el equipo de salvamento.` | `Se marcó en el camino el nodo donde te encontrarás con el equipo de salvamento.` | `The node where you will meet the rescue team has been marked on the road ahead.` | `O nó onde você encontrará a equipe de salvamento foi marcado no caminho à frente.` |
| A23 | 2089 | `…beneficios adicionales:+20% curación recibida.` (falta espacio; también es clave fija en `TryTraducirClavesRuntime`) | `…beneficios adicionales: +20% curación recibida.` | sin cambios | sin cambios |

### Mayúsculas ("Civiles", "Caravana", "Héroes" son nombres propios del juego en el resto del texto)

`EventosAdmin.cs`: 24 apariciones de `civiles` y 5 de `la caravana` en minúscula, por ejemplo
líneas 827, 1510, 1764, 1776, 2121, 2135. En EN/PT solo hay que capitalizar igual (`Civilians`, `Civis`).
Se resuelve junto con los ítems de arriba; el resto va como lote aparte si se aprueba.

## B. Logs de campaña / bitácora

| # | Dónde | Hoy | Propuesta ES | EN | PT-BR |
|---|---|---|---|---|---|
| B1 | `CampaignManager.cs:8831` | `…La másica profana desalienta a la caravana. -5 Esperanza.` (typo) | `…La música profana desalienta a la Caravana. -5 Esperanza.` | **Hoy EN omite la segunda mitad.** Propuesta: `<color=#6A0DAD>-A Kale'Tav ritual has begun at a nearby node. The profane music discourages the Caravan. -5 Hope.</color>` | `…A música profana desencoraja a Caravana. -5 Esperança.` |
| B2 | `CampaignManager.cs:10427` y texto pasivo de Artistas | `ha tenido un festán` / `hagan un festán` (typo) | `festín` en ambos | sin cambios | sin cambios |
| B3 | `CampaignManager.cs:7914` | `-La ausencia de Aliento Negro al viajar, inspira a la Caravana. +2 Esperanza` | sin la coma tras `viajar` | sin cambios | sin cambios |
| B4 | `CampaignManager.cs:8381` | `-La caravana ha llegado a un nodo incendiado. -10 Esperanza.  ` + N + ` Civiles Muertos.` (doble espacio; EN hoy pierde el `-` inicial y el espacio, queda `…-10 Hope.12 Civilians…`) | `-La Caravana ha llegado a un nodo incendiado. -10 Esperanza. ` | `-The Caravan has arrived at a burned node. -10 Hope. ` | `-A Caravana chegou a um nó incendiado. -10 Esperança. ` |
| B5 | `CampaignManager.cs:8324` | `-Como Purificadora,` + nombre (sale `Purificadora,Nombre`) | clave `-Como Purificadora, ` (con espacio) | `-As a Purifier, ` | `-Como Purificadora, ` |
| B6 | `MenuDescanso.cs:753, 758` | `-El tener un Día Libre en plena Ola de Calor, ha caído bien en los Civiles. +5 Esperanza` / `-El tener que trabajar … ha caído mal en los Civiles. -3 Esperanza` | `-Tener un Día Libre en plena Ola de Calor cayó bien entre los Civiles. +5 Esperanza` / `-Tener que trabajar en plena Ola de Calor cayó mal entre los Civiles. -3 Esperanza` | sin cambios | sin cambios |
| B7 | `MenuDescanso.cs:904, 911` | `-Por la baja Esperanza ` + N + ` Civiles han abandonado la Caravana.` | claves `-Por la baja Esperanza, ` y `-Por la muy baja Esperanza, ` (coma) | `-Due to the low Hope, ` / `-Due to the very low Hope, ` | `-Devido à baixa Esperança, ` / `-Devido à baixíssima Esperança, ` |
| B8 | `CampaignManager.cs:10701` | `…de los Mercaderes de la Caravana, que fueron coercionados para que donen a la causa. -5 Esperanza` | `…de los Mercaderes de la Caravana, obligados a donar a la causa. -5 Esperanza` | `…Gold from the Caravan Merchants, who were forced to donate to the cause. -5 Hope` | `…de Ouro dos Mercadores da Caravana, obrigados a doar para a causa. -5 Esperança` |
| B9 | `CampaignManager.cs:9038, 9071` | `se han gastado 200 de oro` / `se han sacrificado 3 bueyes` (resto del juego: `Oro`, `Bueyes`) | `Oro`, `Bueyes` | sin cambios | sin cambios |
| B10 | `CampaignManager.cs:7484`, `MenuDescanso.cs:699`, `8317` | `-La caravana ha sufrido un Ataque…` / `-La caravana ha llegado a un Santuario…` | `Caravana` con mayúscula (32 casos en logs de campaña) | capitalizar | capitalizar |

## C. Patrones repetidos (decisión de estilo, sin aplicar)

- `Si decides X, …` aparece **62 veces** en eventos. Es la voz del juego y funciona; acortarlo a
  `Si X, …` es posible pero cambia casi todas las claves. Recomendación: dejarlo.
- `sumarás un retraso de 5 horas` aparece **8 veces**. El resto del juego usa `Retraso de 5 horas`.
  Podría unificarse como `el viaje se retrasará 5 horas`. Recomendación: unificar solo si se aprueba.

## D. Pendiente de revisar (no se llegó en esta tanda)

- **Logs de batalla**: `CombatLogFormatter.cs` solo arma etiquetas; el texto vive en `BattleManager.cs`,
  `Unidad.cs` y las habilidades. Sin revisar.
- **Mecánicas**: descripciones de estados de Caravana (`ManagerListaEstadosCaravana.cs`) leídas y están
  bien (cortas y claras). Falta Séquitos, Actividades, Presagios, rasgos y hojas de personaje.
- **Logs de campaña con ramas por idioma** (`CampaignManager.cs` ~9690-10265): el español está inline y se
  puede editar sin tocar TRADU; sin revisar.
- **Eventos con texto en `Nodo.cs`/`MenuCaravana.cs`** y el resto de eventos manuales: sin revisar.
- Observación aparte: `TryTraducirClavesRuntime` usa `Zúrkil` (1 vez) y el resto del juego `Zarkil` (50).
  No es un cambio de naturalización pero conviene unificarlo.
