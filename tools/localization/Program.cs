using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Inventario sintáctico: no ejecuta código del juego ni supone que una expresión dinámica es una clave.
string root = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
string output = Path.GetFullPath(args.Length > 1 ? args[1] : Path.Combine(root, "outputs/localization"));
Directory.CreateDirectory(output);
var strings = new SortedDictionary<string, CatalogString>(StringComparer.Ordinal);
var dynamicCalls = new List<object>();
var branches = new List<object>();
var candidates = new List<object>();
var parseErrors = new List<object>();
int scriptCount = 0, calls = 0;

foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "Assets"), "*.cs", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
{
    string path = Path.GetRelativePath(root, file).Replace('\\', '/');
    if (path.Contains("/Editor/") || path.Contains("/Plugins/") || path.Contains("/TextMesh Pro/")) continue;
    scriptCount++;
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
    var syntax = tree.GetRoot();
    int Line(SyntaxNode node) => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
    string Method(SyntaxNode node) => node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText ?? "";
    void Add(string key, SyntaxNode node, string kind, string? en = null, string? pt = null)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        if (!strings.TryGetValue(key, out var entry)) strings[key] = entry = new CatalogString { key = key };
        entry.origins.Add(new { path, line = Line(node), method = Method(node), kind });
        if (en != null && !entry.en.Contains(en)) entry.en.Add(en);
        if (pt != null && !entry.pt.Contains(pt)) entry.pt.Add(pt);
    }

    foreach (var error in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
        parseErrors.Add(new { path, line = error.Location.GetLineSpan().StartLinePosition.Line + 1, message = error.GetMessage() });

    foreach (var call in syntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
    {
        string name = call.Expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
            IdentifierNameSyntax id => id.Identifier.ValueText,
            _ => ""
        };
        bool chineseLookup = call.Expression is MemberAccessExpressionSyntax access
            && access.Expression.ToString() == "ChineseLocalization" && name == "Get";
        if (!chineseLookup && (name != "Traducir" || call.ArgumentList.Arguments.Count != 1)) continue;
        if (chineseLookup && call.ArgumentList.Arguments.Count != 2) continue;
        if (!chineseLookup) calls++;
        var expression = call.ArgumentList.Arguments[0].Expression;
        string? value = ConstantText(expression);
        if (value != null) Add(value, call, chineseLookup ? "ChineseLocalization.Get" : "Traducir.literal",
            chineseLookup ? ConstantText(call.ArgumentList.Arguments[1].Expression) : null);
        else dynamicCalls.Add(new { path, line = Line(call), method = Method(call), expression = expression.ToString() });
    }

    if (path == "Assets/TRADU.cs")
    {
        foreach (var section in syntax.DescendantNodes().OfType<SwitchSectionSyntax>())
        {
            string method = Method(section);
            bool runtime = method == "TryTraducirClavesRuntime";
            bool en = method.Contains("Ingles") || runtime;
            bool pt = method.Contains("Portugues") || runtime;
            foreach (var label in section.Labels.OfType<CaseSwitchLabelSyntax>())
            {
                string? key = ConstantText(label.Value);
                if (key == null) continue;
                var assignment = section.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.SimpleAssignmentExpression) && (a.Left.ToString() == "r" || a.Left.ToString() == "traduccion"));
                ExpressionSyntax? translated = assignment?.Right ?? section.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault()?.Expression;
                Add(key, label, "TRADU.case", en ? ConstantText(translated, false) : null, pt ? ConstantText(translated, true) : null);
            }
        }
    }
    else
    {
        // Candidatos amplios para revisión; incluyen IDs y rutas que NO deben traducirse automáticamente.
        foreach (var literal in syntax.DescendantNodes().OfType<LiteralExpressionSyntax>().Where(l => l.IsKind(SyntaxKind.StringLiteralExpression)))
        {
            string value = literal.Token.ValueText;
            if (value.Length > 1 && value.Any(char.IsLetter))
                candidates.Add(new { path, line = Line(literal), method = Method(literal), value });
        }
    }

    foreach (var node in syntax.DescendantNodes())
    {
        ExpressionSyntax? condition = node switch
        {
            IfStatementSyntax statement => statement.Condition,
            ConditionalExpressionSyntax expression => expression.Condition,
            SwitchStatementSyntax statement => statement.Expression,
            SwitchExpressionSyntax expression => expression.GoverningExpression,
            _ => null
        };
        if (condition == null || !Regex.IsMatch(condition.ToString(), @"idioma|language|ingles|portugues|espanol", RegexOptions.IgnoreCase)) continue;
        branches.Add(new { path, line = Line(node), method = Method(node), condition = condition.ToString(), kind = node.Kind().ToString() });
    }
}

var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
void Write(string name, object data) => File.WriteAllText(Path.Combine(output, name), JsonSerializer.Serialize(data, options) + "\n");
Write("code-inventory.json", new { schemaVersion = 1, strings = strings.Values, dynamicCalls, languageBranches = branches, parseErrors });
Write("script-candidates.json", candidates);
var summary = new { scriptCount, translationCalls = calls, uniqueCodeKeys = strings.Count, dynamicCalls = dynamicCalls.Count,
    languageBranches = branches.Count, scriptCandidates = candidates.Count, parseErrors = parseErrors.Count };
Write("code-summary.json", summary);
Console.WriteLine(JsonSerializer.Serialize(summary, options));
return parseErrors.Count == 0 ? 0 : 1;

static string? ConstantText(ExpressionSyntax? expression, bool portuguese = false)
{
    return expression switch
    {
        LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => literal.Token.ValueText,
        ParenthesizedExpressionSyntax parens => ConstantText(parens.Expression, portuguese),
        BinaryExpressionSyntax add when add.IsKind(SyntaxKind.AddExpression)
            && ConstantText(add.Left, portuguese) is string left && ConstantText(add.Right, portuguese) is string right => left + right,
        ConditionalExpressionSyntax ternary when ternary.Condition.ToString() == "portugues" =>
            ConstantText(portuguese ? ternary.WhenTrue : ternary.WhenFalse, portuguese),
        _ => null
    };
}

internal sealed class CatalogString
{
    public string key { get; set; } = "";
    public List<string> en { get; set; } = new();
    public List<string> pt { get; set; } = new();
    public List<object> origins { get; set; } = new();
}
