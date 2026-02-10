using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class ItemPrefabTools
{
    private const string MenuRoot = "Tools/Items/";
    private const string ReportAssetPath = "Assets/ItemPrefabValidationReport.txt";

    // Matches legacy serialized key: itemDescrpicion
    private static readonly Regex LegacyDescriptionKeyRegex =
        new Regex(@"^(\s*)itemDescrpicion:(.*)$", RegexOptions.Multiline);

    // Matches one-line bilingual descriptions saved as: "espanol / english"
    private static readonly Regex BilingualDescriptionRegex =
        new Regex(@"^(\s*itemDescripcion:\s*)""([^""]+?) / [^""]+""\s*$", RegexOptions.Multiline);

    [MenuItem(MenuRoot + "Migrate Legacy Description Field")]
    public static void MigrateLegacyDescriptionField()
    {
        string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);
        int scanned = 0;
        int changed = 0;

        foreach (string path in prefabPaths)
        {
            string text = File.ReadAllText(path);
            if (!text.Contains("sNombreItem:"))
            {
                continue;
            }

            scanned++;
            string migrated = LegacyDescriptionKeyRegex.Replace(text, "$1itemDescripcion:$2");
            migrated = BilingualDescriptionRegex.Replace(migrated, "$1\"$2\"");

            if (!string.Equals(text, migrated))
            {
                File.WriteAllText(path, migrated);
                changed++;
            }
        }

        AssetDatabase.Refresh();
        string msg = $"Migration complete.\n\nScanned: {scanned}\nChanged: {changed}";
        Debug.Log($"[Items] {msg.Replace("\n", " ")}");
        EditorUtility.DisplayDialog("Item Prefabs Migration", msg, "OK");
    }

    [MenuItem(MenuRoot + "Validate Item Prefabs")]
    public static void ValidateItemPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        int itemCount = 0;
        int issuesCount = 0;
        StringBuilder report = new StringBuilder();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                continue;
            }

            Item item = prefab.GetComponent<Item>();
            if (item == null)
            {
                continue;
            }

            itemCount++;
            List<string> issues = new List<string>();

            if (item.imItem == null)
            {
                issues.Add("missing icon");
            }

            if (string.IsNullOrWhiteSpace(item.sNombreItem))
            {
                issues.Add("missing name");
            }

            if (string.IsNullOrWhiteSpace(item.itemDescripcion))
            {
                issues.Add("missing description");
            }

            if (item.iPrecio < 0)
            {
                issues.Add("negative price");
            }

            if (item.reduccionDanioRecibidoPorcentaje < 0 || item.reduccionDanioRecibidoPorcentaje > 95)
            {
                issues.Add("damage reduction % out of range (0-95)");
            }

            if (item.reduccionDanioCriticoRecibidoPorcentaje < 0 || item.reduccionDanioCriticoRecibidoPorcentaje > 95)
            {
                issues.Add("critical damage reduction % out of range (0-95)");
            }

            if (item.resistenciaEstadosPorcentaje < 0 || item.resistenciaEstadosPorcentaje > 100)
            {
                issues.Add("status resistance % out of range (0-100)");
            }

            if (item.espinasDanioPlano < 0 || item.espinasDanioPorcentaje < 0)
            {
                issues.Add("thorns values must be >= 0");
            }

            if (item.IDClasesQuePuedenUsarEsteItem != null)
            {
                foreach (int idClase in item.IDClasesQuePuedenUsarEsteItem)
                {
                    if (idClase == -1)
                    {
                        continue;
                    }

                    if (idClase < 1 || idClase > 5)
                    {
                        issues.Add($"invalid allowed class id ({idClase})");
                    }
                }
            }

            if (item is Arma arma)
            {
                if (arma.habilidadAtaque == null)
                {
                    issues.Add("weapon without attack ability");
                }

                if (arma.debuffsImpactoArma != null)
                {
                    for (int i = 0; i < arma.debuffsImpactoArma.Count; i++)
                    {
                        DebuffImpactoArmaData debuff = arma.debuffsImpactoArma[i];
                        if (debuff == null)
                        {
                            issues.Add($"on-hit debuff[{i}] is null");
                            continue;
                        }

                        if (debuff.probabilidadAplicar < 0 || debuff.probabilidadAplicar > 100)
                        {
                            issues.Add($"on-hit debuff[{i}] chance out of range (0-100)");
                        }

                        if (debuff.duracionRondas < 1)
                        {
                            issues.Add($"on-hit debuff[{i}] duration must be >= 1");
                        }

                        if (debuff.requiereTiradaSalvacion
                            && (debuff.tipoTiradaSalvacion < 1 || debuff.tipoTiradaSalvacion > 3))
                        {
                            issues.Add($"on-hit debuff[{i}] invalid save type (1-3)");
                        }

                        if (!debuff.TieneEfectos())
                        {
                            issues.Add($"on-hit debuff[{i}] has no effects configured");
                        }

                        if (debuff.stacksSangrado < 0 || debuff.stacksArdiendo < 0 || debuff.stacksCongelado < 0
                            || debuff.stacksAcido < 0 || debuff.stacksAturdido < 0 || debuff.reduccionAPPorTurno < 0
                            || debuff.reduccionResistencias < 0 || debuff.stacksCondenado < 0
                            || debuff.ignorarArmaduraPlano < 0 || debuff.roboVidaPorcentaje < 0
                            || debuff.empujeCasillas < 0 || debuff.jalonCasillas < 0)
                        {
                            issues.Add($"on-hit debuff[{i}] status stacks/reductions must be >= 0");
                        }

                        if (debuff.empujeCasillas > 0 && debuff.jalonCasillas > 0)
                        {
                            issues.Add($"on-hit debuff[{i}] has both push and pull configured");
                        }
                    }
                }
            }

            if (issues.Count > 0)
            {
                issuesCount++;
                report.AppendLine($"{path}: {string.Join(", ", issues)}");
            }
        }

        if (issuesCount == 0)
        {
            WriteAndSelectReport($"Validation OK.\nChecked item prefabs: {itemCount}\nIssues found: 0\n");
            string okMsg = $"Validation OK.\n\nChecked item prefabs: {itemCount}\nIssues found: 0";
            Debug.Log($"[Items] {okMsg.Replace("\n", " ")}");
            EditorUtility.DisplayDialog("Item Prefabs Validation", okMsg, "OK");
            return;
        }

        WriteAndSelectReport(
            $"Validation found issues in {issuesCount}/{itemCount} item prefabs.\n\n{report}");

        string warnMsg =
            $"Validation found issues.\n\nChecked item prefabs: {itemCount}\nPrefabs with issues: {issuesCount}\n\nSee '{ReportAssetPath}' for full report.";
        Debug.LogWarning($"[Items] Validation found issues in {issuesCount}/{itemCount} item prefabs.\n{report}");
        EditorUtility.DisplayDialog("Item Prefabs Validation", warnMsg, "OK");
    }

    private static void WriteAndSelectReport(string content)
    {
        File.WriteAllText(ReportAssetPath, content);
        AssetDatabase.Refresh();

        TextAsset reportAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(ReportAssetPath);
        if (reportAsset != null)
        {
            Selection.activeObject = reportAsset;
            EditorGUIUtility.PingObject(reportAsset);
        }
    }
}
