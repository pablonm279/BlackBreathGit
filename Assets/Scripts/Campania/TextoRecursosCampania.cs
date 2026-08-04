using System.Text;
using System.Text.RegularExpressions;

public static class TextoRecursosCampania
{
    private const string SpriteTagTamanoReducido = "<size=45%><sprite name=\"{0}\"></size>";

    private static readonly Regex regexTagsTmp =
        new Regex(@"(<[^>]+>)", RegexOptions.Compiled);

    private static readonly RecursoIcono[] recursos =
    {
        new RecursoIcono("aliento_negro", @"Aliento\s+Negro|Black\s+Breath|Respiro\s+Negro|Sopro\s+Negro|H[aá]lito\s+Negro"),
        new RecursoIcono("suministros", @"Suministros?|Supplies|Supply|Suprimentos?|Provis(?:o|õ)es|Provisao|Provisões"),
        new RecursoIcono("materiales", @"Materiales?|Materials?"),
        new RecursoIcono("esperanza", @"Esperanza|Hope|Esperan(?:c|ç)a"),
        new RecursoIcono("civiles", @"Civiles|Civil|Civilians?|Civis"),
        new RecursoIcono("fatiga", @"Fatiga|Fatigue|Fadiga"),
        new RecursoIcono("bueyes", @"Bueyes|Buey|Oxen|Ox|Bois|Boi"),
        new RecursoIcono("oro", @"Oro|Gold|Ouro")
    };

    public static string FormatearRecursos(string texto, bool incluirIconos)
    {
        if (!incluirIconos || string.IsNullOrEmpty(texto))
        {
            return texto;
        }

        string[] partes = regexTagsTmp.Split(texto);
        var sb = new StringBuilder(texto.Length + 32);

        for (int i = 0; i < partes.Length; i++)
        {
            string parte = partes[i];
            if (string.IsNullOrEmpty(parte))
            {
                continue;
            }

            sb.Append(EsTagTmp(parte) ? parte : FormatearTextoVisible(parte));
        }

        return sb.ToString();
    }

    private static string FormatearTextoVisible(string texto)
    {
        for (int i = 0; i < recursos.Length; i++)
        {
            RecursoIcono recurso = recursos[i];
            texto = recurso.Regex.Replace(texto, match => match.Value + " " + recurso.SpriteTag);
        }

        return texto;
    }

    private static bool EsTagTmp(string texto)
    {
        return texto.Length > 1 && texto[0] == '<' && texto[texto.Length - 1] == '>';
    }

    private readonly struct RecursoIcono
    {
        public readonly Regex Regex;
        public readonly string SpriteTag;

        public RecursoIcono(string spriteName, string patronAliases)
        {
            Regex = new Regex(
                @"(?<![\p{L}\p{N}_])(?:" + patronAliases + @")(?![\p{L}\p{N}_])",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            SpriteTag = string.Format(SpriteTagTamanoReducido, spriteName);
        }
    }
}
