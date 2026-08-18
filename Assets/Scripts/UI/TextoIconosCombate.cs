using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.TextCore;

public static class TextoIconosCombate
{
    private const string escalaIcono = "121%";
    private const float proporcionBearingVertical = 0.78f;

    private static readonly HashSet<int> spriteAssetsNormalizados = new HashSet<int>();

    private static readonly Regex regexTagsTmp =
        new Regex(@"(<[^>]+>)", RegexOptions.Compiled);

    private static readonly Regex regexBloqueIconoInline =
        new Regex(
            @"(?:<space=[^>]+>)?(?:<size=[^>]+>)?(?:<voffset=[^>]+>)?<sprite name=""(?<name>[^""]+)""(?:\s*/)?>(?:</voffset>)?(?:</size>)?(?:<space=[^>]+>)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex regexEspaciadoIconoNormalizado =
        new Regex(
            @"(?<antes>[ \t]*)(?<icono><size=" + escalaIcono + @"><sprite name=""[^""]+""(?:\s*/)?></size>)(?<despues>[ \t]*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex regexInicioBloqueMecanico =
        new Regex(
            @"(?im)(^|\n)[^\n]*(?:Type|Tipo|Target|Objetivo|Roll/Save|Roll|Tirada|Save|TS|Damage|Da(?:n|ñ)o|Armor|Armadura|Defense|Defensa|Cooldown|Recarga|AP\s*Cost|PA\s*Cost|Valour\s*Cost|Valor\s*Cost|Resource|Recurso|Reaction|Reacci(?:o|ó)n|Crit|Critico|Crítico|Scaling|Escalado|Turn\s*flow|Flujo\s*de\s*turno|Watch\s*setup)\s*:",
            RegexOptions.Compiled);

    private static readonly IconoAlias[] iconos =
    {
        new IconoAlias("ic_fortaleza", @"TS\s*Fortaleza|Fortitude\s+Save|Save\s+Fortitude|Teste\s+de\s+Fortaleza|TS\s*Fortitude|Fortaleza|Fortitude"),
        new IconoAlias("ic_Reflejos", @"TS\s*Reflejos?|Reflex\s+Save|Save\s+Reflex|Teste\s+de\s+Reflexos?|TS\s*Reflexos?|Reflejos?|Reflex(?:es)?"),
        new IconoAlias("ic_mental", @"TS\s*Mental|Mental\s+Save|Save\s+Mental|Teste\s+Mental|Mental"),

        new IconoAlias("dano_contundente", @"Contundente|Bludgeoning|Concussive|Contus(?:a|ao|\u00e3o)"),
        new IconoAlias("dano_perforante", @"Perforante|Piercing|Perfurante"),
        new IconoAlias("dano_cortante", @"Cortante|Slashing|Cutting|Cortante"),
        new IconoAlias("dano_necro", @"Necr(?:o|\u00f3)tico|Necrotic|Necr(?:o|\u00f3)tico"),
        new IconoAlias("dano_divino", @"Divino|Divine"),
        new IconoAlias("dano_fuego", @"Fuego|Fire|Fogo"),
        new IconoAlias("dano_hielo", @"Hielo|Ice|Frost|Gelo"),
        new IconoAlias("dano_rayo", @"Rayo|Lightning|Shock|Raio"),
        new IconoAlias("dano_arcano", @"Arcano|Arcane"),
        new IconoAlias("dano_acido", @"(?:A|\u00c1)cido|Acid|(?:A|\u00c1)cido"),

        new IconoAlias("IconoArmadura", @"Armadura|Armor|Armour"),
        new IconoAlias("IconoDefensa", @"Defensa|Defense|Defence|Defesa"),
        new IconoAlias("Estado_barrera", @"Barrera|Barrier|Barreira"),
        new IconoAlias("Valentía", @"Valent(?:i|\u00ed)a|Valor|Valour"),
        new IconoAlias("Estado_buff", @"Buff|Mejora|Beneficio|Bonus|Bono"),
        new IconoAlias("Estado_reaccion", @"Reacci(?:o|\u00f3)n|Reaction|Rea(?:c|\u00e7)(?:a|\u00e3)o"),
        new IconoAlias("cooldown", @"Cooldown|Recarga|Enfriamiento|Tempo\s+de\s+Recarga"),
        new IconoAlias("ap", @"AP|PA|Action\s+Points?|Puntos?\s+de\s+Acci(?:o|\u00f3)n|Pontos?\s+de\s+A(?:c|\u00e7)(?:a|\u00e3)o"),
        new IconoAlias("esforzar", @"Esfuerzo|Esforzable|Esforzado|Effort|Effortable|Effortful|Esfor(?:c|\u00e7)o|Esfor(?:c|\u00e7)(?:a|\u00e1)vel"),
        new IconoAlias("critico", @"Cr(?:i|\u00ed)t(?:ico|ica|ical)?|Critical"),

        new IconoAlias("Estado_sangrano", @"Sangrado|Bleeding|Bleed|Sangramento"),
        new IconoAlias("Estado_acido", @"Estado\s+(?:A|\u00c1)cido|Acidificado|Corrosi(?:o|\u00f3)n|Corrosion"),
        new IconoAlias("Estado_ardiendo", @"Ardiendo|Quemando|Burning|Burn|Em\s+Chamas"),
        new IconoAlias("Estado_congelado", @"Congelado|Frozen|Congelado"),
        new IconoAlias("Estado_aturdido", @"Aturdido|Stunned|Stun|Atordoado"),
        new IconoAlias("Estado_veneno", @"Veneno|Envenenado|Poison|Poisoned|Veneno|Envenenado"),
        new IconoAlias("Estado_condena", @"Condenado|Condena|Condemned|Condenado"),
        new IconoAlias("Estado_atrapado", @"Inm(?:o|\u00f3)vil|Inmovilizado|Atrapado|Immobilized|Immobilised|Trapped|Im(?:o|\u00f3)vel"),
        new IconoAlias("Estado_evasion", @"Evasi(?:o|\u00f3)n|Evasion|Evas(?:a|\u00e3)o"),
        new IconoAlias("Estado_oculto", @"Escondido|Al\s+Acecho|Oculto|Hidden|Stealthed"),
        new IconoAlias("Estado_regeneravida", @"Regenera\s+Vida|Regeneraci(?:o|\u00f3)n\s+de\s+Vida|Regeneration|Regenera(?:c|\u00e7)(?:a|\u00e3)o\s+de\s+Vida"),
        new IconoAlias("Estado_regeneraArmadura", @"Regenera\s+Armadura|Regeneraci(?:o|\u00f3)n\s+de\s+Armadura|Armor\s+Regeneration|Regenera(?:c|\u00e7)(?:a|\u00e3)o\s+de\s+Armadura"),
        new IconoAlias("Estado_resreducidas", @"Resistencias\s+Reducidas|Reduced\s+Resistances|Resist(?:e|\u00ea)ncias\s+Reduzidas"),
        new IconoAlias("Estado_armadurareducida", @"Armadura\s+Reducida|Reduced\s+Armor|Armadura\s+Reduzida"),
        new IconoAlias("Estado_escudado", @"Escudado|Shielded|Protegido"),
        new IconoAlias("Estado_movimientoabaratado", @"Movimiento\s+Abaratado|Movimiento\s+Reducido|Cheap\s+Movement|Movimento\s+Reduzido"),
        new IconoAlias("Estado_volando", @"Volando|Flying|Voando"),
        new IconoAlias("Estado_provocado", @"Provocado|Taunted|Provoked|Provocado"),
        new IconoAlias("Estado_marcado", @"Marcado|Marked|Marcado"),
        new IconoAlias("Estado_valentia", @"Valent(?:i|\u00ed)a\s+Global|Valour\s+Global|Valor\s+Global"),
        new IconoAlias("Estado_acumularenergia", @"Acumular\s+Energ(?:i|\u00ed)a|Accumulating\s+Energy|Acumular\s+Energia"),
        new IconoAlias("Estado_residuocurativo", @"Residuo\s+Curativo|Healing\s+Residue|Res(?:i|\u00ed)duo\s+Curativo")
    };

    public static string FormatearIconos(string texto, bool incluirIconos)
    {
        if (!incluirIconos || string.IsNullOrEmpty(texto))
        {
            return texto;
        }

        string[] partes = regexTagsTmp.Split(texto);
        var sb = new StringBuilder(texto.Length + 64);
        bool dentroTerminoSemantico = false;

        for (int i = 0; i < partes.Length; i++)
        {
            string parte = partes[i];
            if (string.IsNullOrEmpty(parte))
            {
                continue;
            }

            if (EsTagTmp(parte))
            {
                if (EsInicioTerminoSemantico(parte))
                {
                    dentroTerminoSemantico = true;
                }
                else if (EsFinTerminoSemantico(parte))
                {
                    dentroTerminoSemantico = false;
                }

                sb.Append(parte);
                continue;
            }

            sb.Append(dentroTerminoSemantico ? parte : FormatearTextoVisible(parte));
        }

        return sb.ToString();
    }

    public static string FormatearIconosDesdeBloqueMecanico(string texto, bool incluirIconos)
    {
        if (!incluirIconos || string.IsNullOrEmpty(texto))
        {
            return texto;
        }

        Match match = regexInicioBloqueMecanico.Match(texto);
        if (!match.Success)
        {
            return texto;
        }

        int inicio = match.Index;
        if (match.Value.StartsWith("\n"))
        {
            inicio++;
        }

        return texto.Substring(0, inicio) + FormatearIconos(texto.Substring(inicio), true);
    }

    public static string FormatearIconosDespuesDelTitulo(string texto, bool incluirIconos)
    {
        if (!incluirIconos || string.IsNullOrEmpty(texto))
        {
            return texto;
        }

        int inicioCuerpo = texto.IndexOf("\n\n");
        if (inicioCuerpo >= 0)
        {
            inicioCuerpo += 2;
            return texto.Substring(0, inicioCuerpo) + FormatearIconos(texto.Substring(inicioCuerpo), true);
        }

        inicioCuerpo = texto.IndexOf('\n');
        if (inicioCuerpo >= 0)
        {
            inicioCuerpo++;
            return texto.Substring(0, inicioCuerpo) + FormatearIconos(texto.Substring(inicioCuerpo), true);
        }

        return FormatearIconos(texto, true);
    }

    public static string FormatearIconoDanioInline(int tipoDanio, bool incluirIcono)
    {
        if (!incluirIcono)
        {
            return string.Empty;
        }

        string spriteName = ObtenerSpriteDanio(tipoDanio);
        return string.IsNullOrEmpty(spriteName) ? string.Empty : " " + CrearSpriteTag(spriteName);
    }

    public static string LimitarRepeticionIconos(string texto, int maxRepeticionesPorIcono = 2)
    {
        if (string.IsNullOrEmpty(texto) || maxRepeticionesPorIcono < 1)
        {
            return texto;
        }

        var conteoPorSprite = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        return regexBloqueIconoInline.Replace(texto, match =>
        {
            string spriteName = match.Groups["name"].Value;
            if (string.IsNullOrEmpty(spriteName))
            {
                return match.Value;
            }

            conteoPorSprite.TryGetValue(spriteName, out int repeticionesActuales);
            if (repeticionesActuales >= maxRepeticionesPorIcono)
            {
                return string.Empty;
            }

            conteoPorSprite[spriteName] = repeticionesActuales + 1;
            return match.Value;
        });
    }

    public static string NormalizarIconosInline(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return texto;
        }

        string normalizado = regexBloqueIconoInline.Replace(
            texto,
            match => CrearSpriteTag(match.Groups["name"].Value));

        return regexEspaciadoIconoNormalizado.Replace(normalizado, match =>
        {
            int indiceAnterior = match.Index - 1;
            int indiceSiguiente = match.Index + match.Length;
            bool conservarEspacioAntes = match.Groups["antes"].Length > 0 &&
                                         indiceAnterior >= 0 &&
                                         normalizado[indiceAnterior] != '\n' &&
                                         normalizado[indiceAnterior] != '\r';
            bool conservarEspacioDespues = match.Groups["despues"].Length > 0 &&
                                           indiceSiguiente < normalizado.Length &&
                                           normalizado[indiceSiguiente] != '\n' &&
                                           normalizado[indiceSiguiente] != '\r';

            return (conservarEspacioAntes ? " " : string.Empty) +
                   match.Groups["icono"].Value +
                   (conservarEspacioDespues ? " " : string.Empty);
        });
    }

    public static void NormalizarSpriteAsset(TMP_SpriteAsset spriteAsset)
    {
        if (spriteAsset == null)
        {
            return;
        }

        NormalizarSpriteAssetRecursivo(spriteAsset);
    }

    private static string FormatearTextoVisible(string texto)
    {
        for (int i = 0; i < iconos.Length; i++)
        {
            IconoAlias icono = iconos[i];
            texto = icono.Regex.Replace(texto, match => match.Value + " " + icono.SpriteTag);
        }

        return texto;
    }

    private static bool EsTagTmp(string texto)
    {
        return texto.Length > 1 && texto[0] == '<' && texto[texto.Length - 1] == '>';
    }

    private static bool EsInicioTerminoSemantico(string texto)
    {
        return texto.StartsWith("<link=\"skill-term:", System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool EsFinTerminoSemantico(string texto)
    {
        return texto.Equals("</link>", System.StringComparison.OrdinalIgnoreCase);
    }

    private static string ObtenerSpriteDanio(int tipoDanio)
    {
        switch (tipoDanio)
        {
            case 1: return "dano_cortante";
            case 2: return "dano_perforante";
            case 3: return "dano_contundente";
            case 4: return "dano_fuego";
            case 5: return "dano_hielo";
            case 6: return "dano_rayo";
            case 7: return "dano_acido";
            case 8: return "dano_arcano";
            case 9: return "dano_necro";
            case 11: return "dano_divino";
            default: return string.Empty;
        }
    }

    private static string CrearSpriteTag(string spriteName)
    {
        return "<size=" + escalaIcono + "><sprite name=\"" + spriteName + "\"></size>";
    }

    private static void NormalizarSpriteAssetRecursivo(TMP_SpriteAsset spriteAsset)
    {
        if (spriteAsset == null || !spriteAssetsNormalizados.Add(spriteAsset.GetInstanceID()))
        {
            return;
        }

        List<TMP_SpriteCharacter> caracteres = spriteAsset.spriteCharacterTable;
        for (int i = 0; i < caracteres.Count; i++)
        {
            TMP_SpriteCharacter caracter = caracteres[i];
            if (caracter != null)
            {
                caracter.scale = 1f;
            }
        }

        List<TMP_SpriteGlyph> glifos = spriteAsset.spriteGlyphTable;
        for (int i = 0; i < glifos.Count; i++)
        {
            TMP_SpriteGlyph glifo = glifos[i];
            if (glifo == null)
            {
                continue;
            }

            GlyphMetrics metricas = glifo.metrics;
            if (glifo.sprite != null)
            {
                var rectImportado = glifo.sprite.textureRect;
                metricas.width = rectImportado.width;
                metricas.height = rectImportado.height;
                glifo.glyphRect = new GlyphRect(rectImportado);
            }

            metricas.horizontalBearingX = 0f;
            metricas.horizontalBearingY = metricas.height * proporcionBearingVertical;
            metricas.horizontalAdvance = metricas.width;
            glifo.metrics = metricas;
            glifo.scale = 1f;
        }

        List<TMP_SpriteAsset> fallbacks = spriteAsset.fallbackSpriteAssets;
        if (fallbacks == null)
        {
            return;
        }

        for (int i = 0; i < fallbacks.Count; i++)
        {
            NormalizarSpriteAssetRecursivo(fallbacks[i]);
        }
    }

    private readonly struct IconoAlias
    {
        public readonly Regex Regex;
        public readonly string SpriteTag;

        public IconoAlias(string spriteName, string patronAliases)
        {
            Regex = new Regex(
                @"(?<![\p{L}\p{N}_])(?:" + patronAliases + @")(?![\p{L}\p{N}_])",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            SpriteTag = CrearSpriteTag(spriteName);
        }
    }
}
