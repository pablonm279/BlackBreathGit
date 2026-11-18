using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using UnityEngine;

try
{
string root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
Resources.root = root;
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var current = new TRADU();
var legacy = new LegacyTRADU();
int checks = 0;
void Equal<T>(T expected, T actual, string context)
{
    checks++;
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception(context + ": expected " + expected + ", got " + actual);
}

using var inventory = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "outputs/localization/code-inventory.json")));
string[] keys = inventory.RootElement.GetProperty("strings").EnumerateArray().Select(e => e.GetProperty("key").GetString()).ToArray();
foreach (int language in new[] { 1, 2, 3 })
{
    current.nIdioma = legacy.nIdioma = language;
    foreach (string key in keys)
        Equal(legacy.Traducir(key), current.Traducir(key), "Legacy " + language + " / " + key);
}

current.nIdioma = TRADU.IdiomaChinoSimplificado;
legacy.nIdioma = TRADU.IdiomaIngles;
foreach (string key in keys)
{
    string expected = ChineseLocalization.TryTranslate(key, out string chinese) ? chinese : legacy.Traducir(key);
    Equal(expected, current.Traducir(key), "Chinese/fallback " + key);
}
var catalog = JsonSerializer.Deserialize<ChineseLocalization.Catalog>(File.ReadAllText(Path.Combine(root,
    "Assets/Resources/Localization/zh-Hans.json")), new JsonSerializerOptions { IncludeFields = true });
foreach (var entry in catalog.entries)
    Equal(entry.value, current.Traducir(entry.key), "Catalog " + entry.key);
Equal("设置", current.Traducir("Opciones"), "Menu translation");
Equal("新闻", current.Traducir("Noticias"), "News translation");
Equal(current.Traducir("Gráficos"), current.Traducir("Gráficos".Normalize(NormalizationForm.FormD)), "NFC");
Equal(null, current.Traducir(null), "Null Chinese input");
Equal("", current.Traducir(""), "Empty Chinese input");
Equal("unchanged-key", current.Traducir("unchanged-key"), "Unknown key");
Equal("胜利！获得12经验、30金币、4材料。 希望+2。", ChineseBattleText.Victory(12, 30, 4, 2), "All battle rewards");
Equal("胜利！获得30金币。", ChineseBattleText.Victory(0, 30, 0, 0), "Gold-only victory");
Equal("胜利，本次没有奖励。", ChineseBattleText.Victory(0, 0, 0, 0), "Empty victory");
Equal("胜利！希望+2。", ChineseBattleText.Victory(0, 0, 0, 2), "Hope-only victory");
Equal("战败，损失：-3平民、-2希望、-1驮牛。", ChineseBattleText.Defeat(2, 3, 1), "All battle losses");
Equal("战败，损失：-1驮牛。", ChineseBattleText.Defeat(0, 0, 1), "Oxen-only defeat");
Equal("战败，但未造成严重损失。", ChineseBattleText.Defeat(0, 0, 0), "Empty defeat");

var bootstrap = typeof(InitialLanguageBootstrap).GetMethod("DetectWindowsLanguage", BindingFlags.NonPublic | BindingFlags.Static);
void Detect(SystemLanguage system, int? saved, int expected)
{
    PlayerPrefs.values.Clear();
    Application.systemLanguage = system;
    if (saved.HasValue) PlayerPrefs.SetInt("nIdioma", saved.Value);
    bootstrap.Invoke(null, null);
    Equal(expected, PlayerPrefs.GetInt("nIdioma"), "Bootstrap " + system + " saved " + saved);
}
Detect(SystemLanguage.Spanish, null, 1);
Detect(SystemLanguage.English, null, 2);
Detect(SystemLanguage.Portuguese, null, 3);
Detect(SystemLanguage.ChineseSimplified, null, 4);
Detect(SystemLanguage.ChineseTraditional, null, 2);
Detect(SystemLanguage.Chinese, null, 2);
Detect(SystemLanguage.Unknown, null, 2);
foreach (int saved in new[] { 1, 2, 3, 4 }) Detect(SystemLanguage.ChineseSimplified, saved, saved);
Detect(SystemLanguage.Spanish, 99, 2);
Application.isEditor = false;
// ChineseLocalization.ReleaseReady = true (traducción + LQA completadas): el chino queda
// disponible en cualquier build, no solo en el Editor o en Development Build.
Detect(SystemLanguage.ChineseSimplified, null, 4);
Detect(SystemLanguage.ChineseSimplified, 4, 4);
Debug.isDebugBuild = true;
Detect(SystemLanguage.ChineseSimplified, null, 4);
Application.isEditor = true;

var tutorial = new TutorialLocalizationTable();
tutorial.texts.Add(new TutorialLocalizedText { key = "tutorial.intro.title", es = "Introducción", en = "Introduction", pt = "Introdução" });
tutorial.texts.Add(new TutorialLocalizedText { key = "test.body", es = "Texto original", en = "English fallback", pt = "Texto português" });
foreach (var (language, expected) in new[] { (1, "Introducción"), (2, "Introduction"), (3, "Introdução"), (4, "简介") })
{
    PlayerPrefs.SetInt("nIdioma", language);
    Equal(expected, tutorial.Get("tutorial.intro.title"), "Tutorial language " + language);
}
Equal("English fallback", tutorial.Get("test.body"), "Tutorial English fallback");
Equal("missing.key", tutorial.Get("missing.key"), "Tutorial unknown key");
Equal("", tutorial.Get(null), "Tutorial null key");
tutorial.texts[1].en = "";
Equal("Texto original", tutorial.Get("test.body"), "Tutorial last fallback");

using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "outputs/localization/asset-inventory.json")));
var realTutorial = new TutorialLocalizationTable();
foreach (var entry in assets.RootElement.GetProperty("entries").EnumerateArray())
{
    if (entry.GetProperty("path").GetString() != "Assets/Resources/Tutoriales/TutorialVerticalSlice_Textos.asset") continue;
    realTutorial.texts.Add(new TutorialLocalizedText
    {
        key = entry.GetProperty("key").GetString(), es = entry.GetProperty("es").GetString(),
        en = entry.GetProperty("en").GetString(), pt = entry.GetProperty("pt").GetString()
    });
}
Equal(105, realTutorial.texts.Count, "Tutorial inventory");
foreach (int language in new[] { 1, 2, 3, 4 })
{
    PlayerPrefs.SetInt("nIdioma", language);
    foreach (var entry in realTutorial.texts)
    {
        string fallback = (language == 2 || language == 4) && !string.IsNullOrEmpty(entry.en) ? entry.en
            : (language == 3 && !string.IsNullOrEmpty(entry.pt) ? entry.pt
                : (!string.IsNullOrEmpty(entry.es) ? entry.es : entry.key));
        string expected = language == 4 ? ChineseLocalization.Get(entry.key, fallback) : fallback;
        Equal(expected, realTutorial.Get(entry.key), "Real tutorial " + language + " / " + entry.key);
    }
}
typeof(TRADU).GetProperty("i").SetValue(null, current);
PlayerPrefs.SetInt("nIdioma", 1);
Equal("简介", realTutorial.Get("tutorial.intro.title"), "Active instance overrides saved language");
typeof(TRADU).GetProperty("i").SetValue(null, null);
int quickComponents = 0;
int pregameTooltips = 0;
const BindingFlags privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
foreach (var entry in assets.RootElement.GetProperty("entries").EnumerateArray())
{
    if (entry.GetProperty("path").GetString().StartsWith("Assets/_Recovery/")) continue;
    if (entry.GetProperty("field").GetString() != "textoEspanol") continue;
    string es = entry.GetProperty("es").GetString();
    string en = entry.GetProperty("en").GetString();
    string pt = entry.GetProperty("pt").GetString();
    Equal(true, ChineseLocalization.TryTranslate(es, out string zh), "Serialized component coverage " + es);
    if (entry.GetProperty("scriptGuid").GetString() == "5a910377adbd87f4b8ff8436cb4f5107")
    {
        quickComponents++;
        var label = new TMPro.TextMeshProUGUI();
        var quick = new TraduccionRapida { associatedComponent = label };
        typeof(TraduccionRapida).GetField("textoEspanol", privateInstance).SetValue(quick, es);
        typeof(TraduccionRapida).GetField("textoIngles", privateInstance).SetValue(quick, en);
        typeof(TraduccionRapida).GetField("textoPortugues", privateInstance).SetValue(quick, pt);
        PlayerPrefs.SetInt("nIdioma", 4);
        typeof(TraduccionRapida).GetMethod("OnEnable", privateInstance).Invoke(quick, null);
        Equal(zh, label.text, "Quick translation on enable / saved Chinese");
        foreach (int language in new[] { 1, 2, 3, 4, 1 })
        {
            current.nIdioma = language;
            current.TraducirTodosTextosSegunIdioma();
            Equal(language == 4 ? zh : language == 2 ? en : language == 3 ? pt : es,
                label.text, "Quick translation language event " + language);
        }
        typeof(TraduccionRapida).GetMethod("OnDisable", privateInstance).Invoke(quick, null);
        current.nIdioma = 4;
        current.TraducirTodosTextosSegunIdioma();
        Equal(es, label.text, "Disabled quick component unsubscribes");
        typeof(TraduccionRapida).GetMethod("OnEnable", privateInstance).Invoke(quick, null);
        Equal(zh, label.text, "Reopened quick component uses current language");
        if (es.Contains("VERSIÓN DEMO"))
        {
            foreach (string variant in new[] { new string(' ', 66) + es.TrimStart(),
                new string(' ', 66) + es.Trim().Replace("\r\n", "\n"),
                es.Trim().Replace("\r\n", "\n").Replace("\n", "\r\n") })
            {
                typeof(TraduccionRapida).GetField("textoEspanol", privateInstance).SetValue(quick, variant);
                current.nIdioma = 4;
                current.TraducirTodosTextosSegunIdioma();
                Equal(zh, label.text, "Demo notice with Inspector padding / line endings");
                current.nIdioma = 1;
                current.TraducirTodosTextosSegunIdioma();
                Equal(variant, label.text, "Spanish keeps original layout whitespace");
            }
        }
        typeof(TraduccionRapida).GetMethod("OnDisable", privateInstance).Invoke(quick, null);
    }
    else if (entry.GetProperty("scriptGuid").GetString() == "8b9c0d1e2f3a4567890abcdef1234567")
    {
        pregameTooltips++;
        var tooltip = new PrePartidaTooltip();
        tooltip.ConfigurarTexto(es, en, pt);
        foreach (int language in new[] { 1, 2, 3, 4 })
        {
            PlayerPrefs.SetInt("nIdioma", language);
            Equal(language == 4 ? zh : language == 2 ? en : language == 3 ? pt : es,
                tooltip.ObtenerTextoActual(), "Pregame tooltip language " + language);
        }
    }
}
Equal(42, quickComponents, "Real TraduccionRapida components");
Equal(6, pregameTooltips, "Real PrePartidaTooltip components");
var fallbackTooltip = new PrePartidaTooltip();
fallbackTooltip.ConfigurarTexto("missing-tooltip-test", "English tooltip fallback", "Portuguese tooltip");
PlayerPrefs.SetInt("nIdioma", 4);
Equal("English tooltip fallback", fallbackTooltip.ObtenerTextoActual(), "Pregame tooltip Chinese fallback");
Equal(0, Debug.errors.Count, "Runtime catalog errors");
Console.WriteLine($"PASS: {checks} assertions; {keys.Length} keys compared with original TRADU in ES/EN/PT; Chinese lookup/fallback; bootstrap; tutorial text selection; {quickComponents} quick components; {pregameTooltips} pregame tooltips.");
return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
