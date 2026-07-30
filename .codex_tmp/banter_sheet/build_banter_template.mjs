import fs from "node:fs/promises";
import { SpreadsheetFile, Workbook } from "@oai/artifact-tool";

const outputDir =
  "C:/UnityProyectosNOborrar/GDD - Untitled/outputs/019fb2c5-2a3b-79a1-8bf8-8a14edb77265";
await fs.mkdir(outputDir, { recursive: true });

const workbook = Workbook.create();
const sheet = workbook.worksheets.add("Banters ES");
const instructions = workbook.worksheets.add("Cómo completar");

sheet.showGridLines = false;
sheet.getRange("A1:I1").merge();
sheet.getRange("A1").values = [["Banters de batalla — propuesta en español para revisión"]];
sheet.getRange("A2:I2").merge();
sheet.getRange("A2").values = [[
  "Escribí una línea por renglón dentro de cada celda. Podés cargar varias variantes o dejar la celda vacía."
]];

const headers = [
  "Situación",
  "Chance",
  "Quién habla",
  "Caballero",
  "Explorador",
  "Purificadora",
  "Acechador",
  "Canalizador",
  "Duelista",
];

const situations = [
  ["Comenzar batalla", 1, "1 personaje al azar"],
  ["Idle: el turno supera 30 segundos", 0.6, "1 personaje al azar"],
  ["Un aliado mata a un enemigo", 0.6, "Un compañero de quien mata"],
  ["Muere un aliado", 1, "Un compañero al azar"],
  ["Llegan refuerzos enemigos", 0.5, "1 personaje al azar"],
  ["Recibe un golpe crítico", 0.3, "El receptor"],
  ["Realiza un golpe crítico", 0.5, "Quien realiza el crítico"],
  ["La moral grupal aumenta una etapa", 1, "1 personaje al azar"],
  ["La moral grupal disminuye una etapa", 1, "1 personaje al azar"],
  ["Recibe una habilidad de un aliado", 0.4, "El receptor"],
  ["Un aliado pifia", 0.5, "Un compañero de quien pifia"],
];

sheet.getRange("A4:I4").values = [headers];
const rows = situations.map((item) => [item[0], item[1], item[2], "", "", "", "", "", ""]);
sheet.getRange("A5:I15").values = rows;

const banterLines = [
  [
    "¡Formen filas! Hoy ganaremos con honor.\nAcero en mano. Que comience la batalla.",
    "No me gusta este lugar... manténganse cerca.\nEntremos, salgamos y procuremos seguir respirando.",
    "La luz camina con nosotros.\nFirmes. Ninguna sombra quebrará nuestra fe.",
    "Dejen alguno vivo... por un momento.\nTerminaré esto antes de que puedan suplicar.",
    "Las variables convergen. Es hora.\nSiento el poder arder. No lo desperdicien.",
    "Bueno, equipo: intentemos no morir con estilo.\n¿Listos? Yo nací lista. Más o menos.",
  ],
  [
    "¿Esperamos una invitación? ¡Al ataque!\nMi espada se oxida mientras dudamos.",
    "Cuanto más esperamos, más cosas pueden salir mal.\n¿Terminamos de pensarlo antes de que nos encuentren?",
    "La prudencia es virtud; la parálisis, no.\nRespiren. Luego decidamos antes de echar raíces.",
    "Si siguen dudando, decidiré por ustedes.\nEl enemigo envejece mejor que este plan.",
    "He calculado seis opciones. Elijan una.\nMi paciencia es menos estable que mi magia.",
    "¿Este turno incluye una siesta?\nAvísenme cuando termine la reunión estratégica.",
  ],
  [
    "¡Buen golpe! Una victoria digna.\nAsí se combate. Me honra luchar a tu lado.",
    "Bien hecho. Uno menos que puede perseguirnos.\nEso fue... sorprendentemente eficaz.",
    "Golpe certero. La luz guio tu mano.\nHas protegido muchas vidas con ese acto.",
    "Limpio. Casi pareció que sabías hacerlo.\nNo estuvo mal. El siguiente, más despacio.",
    "Ejecución precisa. Resultado óptimo.\nInteresante. Superaste mis cálculos.",
    "¡Eso! Recuérdame no hacerte enfadar.\nQué elegancia. Casi me robas el protagonismo.",
  ],
  [
    "¡No! Su sacrificio será vengado.\nCayó con honor. Nosotros seguiremos.",
    "No... no tenía que terminar así.\nManténganse juntos. No quiero perder a nadie más.",
    "Que la luz reciba su alma.\nTu memoria será mi plegaria... y mi furia.",
    "Los muertos no necesitan lágrimas. Necesitan venganza.\nAlguien pagará por esto. Lentamente.",
    "Una variable perdida... una deuda adquirida.\nNo pude preverlo. No volverá a ocurrir.",
    "Oye... todavía no terminamos nuestra charla.\nMalditos sean. Ya no estoy bromeando.",
  ],
  [
    "¡Más enemigos! Mejor para la gloria.\nQue vengan. Mi brazo aún tiene fuerza.",
    "Claro. Porque no había suficientes.\nRefuerzos... deberíamos reconsiderar el terreno.",
    "Más sombras para purificar.\nQue vengan. La luz no retrocede.",
    "Más carne caminando hacia el matadero.\nQué considerados: trajeron reemplazos.",
    "Nuevas variables. Ajustaré la destrucción.\nPerfecto. Necesitaba más objetivos.",
    "Llegaron tarde. Qué falta de educación.\n¿Más invitados? Nadie me avisó de la fiesta.",
  ],
  [
    "¡Buen golpe! Ahora recibe el mío.\nLa sangre no quiebra mi juramento.",
    "¡Eso dolió más de lo razonable!\nNota práctica: no volver a recibir eso.",
    "Mi fe es más fuerte que este dolor.\nPuedes herir mi cuerpo, no mi propósito.",
    "Te arrancaré la mano por eso.\nDisfruta el momento. Será el último.",
    "Dolor registrado. Represalia inminente.\n¡Error de cálculo! Ahora estoy furioso.",
    "¡Ay! Eso definitivamente cuenta como trampa.\nBuen golpe. El mío será más gracioso.",
  ],
  [
    "¡Por el honor! ¡Golpe perfecto!\nAsí canta el acero en manos dignas.",
    "¿Vieron eso? Yo tampoco me lo esperaba.\nPerfecto. Hagámoslo otra vez... desde lejos.",
    "¡Que la luz juzgue a los impíos!\nLa justicia encontró su objetivo.",
    "Sentí cómo se quebraba. Satisfactorio.\nPerfecto. El miedo hará el resto.",
    "Impacto crítico. Exactamente como calculé.\n¡Sí! ¡El poder exige ser liberado!",
    "Directo al orgullo. Y quizá a algún órgano.\n¿Eso dolió? Mi técnica dice que sí.",
  ],
  [
    "¡Eso es! Nuestros corazones son uno.\nLa victoria ya siente nuestro paso.",
    "Bien... quizá sí salgamos de esta.\nEso mejora nuestras probabilidades. Un poco.",
    "La esperanza prende incluso en la oscuridad.\nNuestra fe crece. Mantengamos su llama.",
    "Al fin muestran algo parecido a determinación.\nBien. Conserven esa rabia.",
    "La cohesión aumenta. También nuestro potencial.\nLa energía del grupo está ascendiendo.",
    "Miren esas caras. Casi parecen valientes.\nEso, equipo. La confianza combina con todo.",
  ],
  [
    "¡No cedan! El miedo deshonra a los vivos.\nAlcen la cabeza. Aún sostenemos nuestras armas.",
    "Esto empieza a parecer una muy mala idea.\nRespiren... entrar en pánico no ayuda. Creo.",
    "No permitan que la oscuridad entre en sus corazones.\nLa fe tambalea, pero no se extingue.",
    "El miedo apesta. Contrólenlo.\nSi van a quebrarse, háganlo después de matar.",
    "La moral cae. Irracional, pero peligroso.\nControlen sus emociones antes de que yo pierda las mías.",
    "Eh, esas caras largas atraen flechas.\nVamos, todavía no llegó la parte realmente terrible.",
  ],
  [
    "Mi gratitud. Haré valer tu ayuda.\nBuen apoyo. Ahora déjame abrir camino.",
    "Eso ayuda. Mucho más de lo que admitiré.\nGracias. Prefiero las sorpresas que no duelen.",
    "Tu bondad fortalece mi espíritu.\nRecibo tu don con gratitud.",
    "Útil. No esperes que lo repita.\nBien. Ahora apártate y observa.",
    "Sinergia aceptable. Continúa.\nInteresante... tu energía armoniza con la mía.",
    "Gracias. Sabía que era tu favorita.\nBuen trabajo. Te debo una... pequeña.",
  ],
  [
    "Recobra la postura. Aún puedes luchar con honor.\nEl suelo no era tu enemigo. Concéntrate.",
    "Y por eso siempre llevo un plan de escape.\nTranquilo, fingiremos que fue una maniobra.",
    "Todos erramos. Levántate y vuelve a intentarlo.\nLa luz perdona... procura que el enemigo también.",
    "Patético. Intenta apuntar al enemigo.\nOtra así y te usaré de distracción.",
    "Desviación catastrófica. Fascinante.\nTus probabilidades acaban de insultarme.",
    "¡Magnífico! Ahora prueba con el filo correcto.\nDiez puntos por estilo. Cero por eficacia.",
  ],
];
sheet.getRange("D5:I15").values = banterLines;

sheet.getRange("A1:I1").format = {
  fill: "#111827",
  font: { bold: true, color: "#F8FAFC", size: 18 },
  horizontalAlignment: "center",
  verticalAlignment: "center",
};
sheet.getRange("A2:I2").format = {
  fill: "#1F2937",
  font: { italic: true, color: "#CBD5E1", size: 10 },
  horizontalAlignment: "left",
  verticalAlignment: "center",
  wrapText: true,
};
sheet.getRange("A4:I4").format = {
  fill: "#374151",
  font: { bold: true, color: "#FFFFFF", size: 11 },
  horizontalAlignment: "center",
  verticalAlignment: "center",
  wrapText: true,
  borders: { preset: "outside", style: "medium", color: "#111827" },
};
sheet.getRange("A5:C15").format = {
  fill: "#E5E7EB",
  font: { color: "#111827", size: 10 },
  verticalAlignment: "center",
  wrapText: true,
  borders: {
    insideHorizontal: { style: "thin", color: "#CBD5E1" },
    insideVertical: { style: "thin", color: "#CBD5E1" },
    left: { style: "medium", color: "#6B7280" },
    right: { style: "medium", color: "#6B7280" },
  },
};
sheet.getRange("D5:I15").format = {
  fill: "#FFF7D6",
  font: { color: "#111827", size: 10 },
  horizontalAlignment: "left",
  verticalAlignment: "top",
  wrapText: true,
  borders: {
    insideHorizontal: { style: "thin", color: "#E5D49C" },
    insideVertical: { style: "thin", color: "#E5D49C" },
    right: { style: "medium", color: "#A78B3B" },
  },
};
sheet.getRange("B5:B15").format.numberFormat = "0%";

sheet.getRange("A1:I1").format.rowHeight = 34;
sheet.getRange("A2:I2").format.rowHeight = 34;
sheet.getRange("A4:I4").format.rowHeight = 42;
sheet.getRange("A5:I15").format.rowHeight = 94;
sheet.getRange("A:A").format.columnWidth = 31;
sheet.getRange("B:B").format.columnWidth = 10;
sheet.getRange("C:C").format.columnWidth = 28;
sheet.getRange("D:I").format.columnWidth = 28;
sheet.freezePanes.freezeRows(4);
sheet.freezePanes.freezeColumns(3);

instructions.showGridLines = false;
instructions.getRange("A1:F1").merge();
instructions.getRange("A1").values = [["Cómo completar la plantilla"]];
instructions.getRange("A3:A8").values = [
  ["1. Escribí solamente en las celdas amarillas."],
  ["2. Cada celda cruza una clase con una situación."],
  ["3. Si querés varias variantes, escribí una línea por renglón usando Alt+Enter."],
  ["4. No hace falta completar todas las combinaciones."],
  ["5. Escribí todo en español; después se traducirá a inglés y portugués."],
  ["6. Cada línea resultante podrá aparecer una sola vez por batalla."],
];
instructions.getRange("A10:F10").merge();
instructions.getRange("A10").values = [[
  "Ejemplo dentro de una celda: «¡Buen golpe!» [Alt+Enter] «Así se hace.»"
]];
instructions.getRange("A1:F1").format = {
  fill: "#111827",
  font: { bold: true, color: "#FFFFFF", size: 18 },
  horizontalAlignment: "center",
  verticalAlignment: "center",
};
instructions.getRange("A3:F8").merge(true);
instructions.getRange("A3:F8").format = {
  fill: "#F3F4F6",
  font: { color: "#1F2937", size: 11 },
  verticalAlignment: "center",
  wrapText: true,
  borders: { preset: "outside", style: "thin", color: "#D1D5DB" },
};
instructions.getRange("A10:F10").format = {
  fill: "#FFF7D6",
  font: { italic: true, color: "#713F12", size: 11 },
  verticalAlignment: "center",
  wrapText: true,
  borders: { preset: "outside", style: "thin", color: "#D6B656" },
};
instructions.getRange("A:F").format.columnWidth = 18;
instructions.getRange("A1:F1").format.rowHeight = 36;
instructions.getRange("A3:F8").format.rowHeight = 28;
instructions.getRange("A10:F10").format.rowHeight = 42;

const matrixInspection = await workbook.inspect({
  kind: "table",
  range: "Banters ES!A1:I15",
  include: "values,formulas",
  tableMaxRows: 15,
  tableMaxCols: 9,
});
console.log(matrixInspection.ndjson);

const errors = await workbook.inspect({
  kind: "match",
  searchTerm: "#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A",
  options: { useRegex: true, maxResults: 50 },
  summary: "formula error scan",
});
console.log(errors.ndjson);

const previewMatrix = await workbook.render({
  sheetName: "Banters ES",
  range: "A1:I15",
  scale: 1,
  format: "png",
});
await fs.writeFile(
  `${outputDir}/preview_banters_es_revision.png`,
  new Uint8Array(await previewMatrix.arrayBuffer()),
);

const previewInstructions = await workbook.render({
  sheetName: "Cómo completar",
  range: "A1:F10",
  scale: 1,
  format: "png",
});
await fs.writeFile(
  `${outputDir}/preview_instrucciones.png`,
  new Uint8Array(await previewInstructions.arrayBuffer()),
);

const output = await SpreadsheetFile.exportXlsx(workbook);
await output.save(`${outputDir}/Banters_ES_Propuesta_Revision.xlsx`);
