import fs from "node:fs/promises";
import { FileBlob, SpreadsheetFile } from "@oai/artifact-tool";

const inputPath = "C:/Users/Pablo/Desktop/sadsadasd.xlsx";
const outputDir =
  "C:/UnityProyectosNOborrar/GDD - Untitled/outputs/019fb2c5-2a3b-79a1-8bf8-8a14edb77265";
await fs.mkdir(outputDir, { recursive: true });

const input = await FileBlob.load(inputPath);
const workbook = await SpreadsheetFile.importXlsx(input);
const sheet = workbook.worksheets.getItem("Banters ES");

const overview = await workbook.inspect({
  kind: "workbook,sheet,table",
  maxChars: 30000,
  tableMaxRows: 20,
  tableMaxCols: 12,
  tableMaxCellChars: 500,
});
console.log(overview.ndjson);

for (let row = 5; row <= 15; row += 1) {
  for (let col = 4; col <= 9; col += 1) {
    const cell = sheet.getCell(row - 1, col - 1);
    const value = cell.values?.[0]?.[0];
    if (typeof value === "string") {
      cell.values = [[
        value
          .split(/\r?\n/)
          .map((line) => line.trim())
          .filter((line) => line.length > 0)
          .join("\n"),
      ]];
    }
  }
}

const corrections = {
  I5: "Bueno, equipo: momento de bailar.\n¿Listos? Yo nací lista.",
  H6: "El tiempo pasa...\nMi poder se impacienta...",
  I6: "¿Este turno incluye una siesta?\nLa punta afilada hacia el enemigo. Es fácil...",
  I7: "¡Qué destreza tienes!\nÚltimo golpe, sutil...",
  G8: "Lamentable...\nTodo depende de mí...",
  G9: "¡Más sangre para desparramar!\nNo se acaban jamás...",
  D10: "Argh... ¡Buen golpe!\nLa sangre no quiebra mi juramento.",
  E10: "¡Eso dolió!\nEso estuvo demasiado cerca...",
  G10: "Argh... ya verás...\nHmpf... Ya me tocará...",
  H10: "Argh... mi energía flaquea...\nArgh... ya verás...",
  I10: "¡Ay!\nHmpf... Buen golpe.",
  D11: "¡Muere!\n¡Que prueben nuestro acero!",
  F11: "¡Purificado!\nLa luz ha dictado sentencia.",
  G11: "¡Sufre!\nAsí termina toda resistencia.",
  I12: "¡Lo tenemos, compañeros!\nEso es... con confianza...",
  D13: "¡No cedan! El miedo deshonra a los vivos.\nNo bajen la cabeza o la perderán...",
  F15: "Déjate guiar por la luz...\n¡Recupérate! Tú puedes.",
  H15: "Difícil de ver...\n¡Coordina tus ataques!",
};

for (const [address, value] of Object.entries(corrections)) {
  sheet.getRange(address).values = [[value]];
}
sheet.getRange("C:C").format.columnWidth = 28;

const incomplete = [];
for (let row = 5; row <= 15; row += 1) {
  for (let col = 4; col <= 9; col += 1) {
    const cell = sheet.getCell(row - 1, col - 1);
    const value = cell.values?.[0]?.[0];
    const count = typeof value === "string"
      ? value.split(/\r?\n/).filter((line) => line.trim().length > 0).length
      : 0;
    if (count !== 2) {
      incomplete.push(`${cell.address}:${count}`);
    }
  }
}
if (incomplete.length > 0) {
  throw new Error(`Celdas sin exactamente dos líneas: ${incomplete.join(", ")}`);
}

const correctedInspection = await workbook.inspect({
  kind: "table",
  range: "Banters ES!A1:I15",
  include: "values,formulas",
  tableMaxRows: 15,
  tableMaxCols: 9,
  tableMaxCellChars: 500,
});
console.log(correctedInspection.ndjson);

const errors = await workbook.inspect({
  kind: "match",
  searchTerm: "#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A",
  options: { useRegex: true, maxResults: 50 },
  summary: "formula error scan",
});
console.log(errors.ndjson);

const preview = await workbook.render({
  sheetName: "Banters ES",
  range: "A1:I15",
  scale: 1,
  format: "png",
});
await fs.writeFile(
  `${outputDir}/preview_sadsadasd_corregido.png`,
  new Uint8Array(await preview.arrayBuffer()),
);

const instructionsPreview = await workbook.render({
  sheetName: "Cómo completar",
  range: "A1:F10",
  scale: 1,
  format: "png",
});
await fs.writeFile(
  `${outputDir}/preview_sadsadasd_instrucciones.png`,
  new Uint8Array(await instructionsPreview.arrayBuffer()),
);

const output = await SpreadsheetFile.exportXlsx(workbook);
await output.save(`${outputDir}/sadsadasd_corregido.xlsx`);
