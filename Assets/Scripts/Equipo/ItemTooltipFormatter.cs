using System.Collections.Generic;
using System.Text;

public static class ItemTooltipFormatter
{
    private const string ColorStats = "#8FB7C9";
    private const string ColorHabilidades = "#C7B27A";
    private const string ColorDebuff = "#D28E6A";

    public static string ConstruirTooltip(Item item, bool incluirNombre = true)
    {
        if (item == null)
        {
            return string.Empty;
        }

        StringBuilder texto = new StringBuilder();
        if (incluirNombre && !string.IsNullOrWhiteSpace(item.sNombreItem))
        {
            texto.Append(Traducir(item.sNombreItem));
        }

        if (!string.IsNullOrWhiteSpace(item.itemDescripcion))
        {
            if (texto.Length > 0)
            {
                texto.Append("\n\n");
            }

            texto.Append(Traducir(item.itemDescripcion));
        }

        string bloqueEfectos = ConstruirBloqueEfectos(item);
        if (!string.IsNullOrEmpty(bloqueEfectos))
        {
            if (texto.Length > 0)
            {
                texto.Append("\n\n");
            }

            texto.Append(bloqueEfectos);
        }

        return texto.ToString();
    }

    private static string ConstruirBloqueEfectos(Item item)
    {
        List<string> lineasStats = new List<string>();
        List<string> lineasHabilidades = new List<string>();
        List<string> lineasDebuffImpacto = new List<string>();
        HashSet<string> habilidadesUnicas = new HashSet<string>();

        if (item is Arma arma)
        {
            AgregarStatsComunes(
                lineasStats,
                arma.buffFuerza, arma.buffAgi, arma.buffPoder, arma.buffIniciativa,
                arma.buffApMax, arma.buffValMax, arma.buffhpMax, arma.buffArmadura,
                arma.buffDefensa, arma.buffTSReflejo, arma.buffTSFortaleza, arma.buffTSMental,
                arma.buffResFuego, arma.buffResRayo, arma.buffResHielo, arma.buffResArcano,
                arma.buffResAcido, arma.buffResNecro, arma.buffResDivino);

            AgregarHabilidad(lineasHabilidades, habilidadesUnicas, arma.habilidadAtaque);
            AgregarHabilidad(lineasHabilidades, habilidadesUnicas, arma.habilidadExtra1);
            AgregarHabilidad(lineasHabilidades, habilidadesUnicas, arma.habilidadExtra2);
            AgregarDebuffsImpacto(lineasDebuffImpacto, arma.debuffsImpactoArma);
        }
        else if (item is Armadura armadura)
        {
            AgregarStatsComunes(
                lineasStats,
                armadura.buffFuerza, armadura.buffAgi, armadura.buffPoder, armadura.buffIniciativa,
                armadura.buffApMax, armadura.buffValMax, armadura.buffhpMax, armadura.buffArmadura,
                armadura.buffDefensa, armadura.buffTSReflejo, armadura.buffTSFortaleza, armadura.buffTSMental,
                armadura.buffResFuego, armadura.buffResRayo, armadura.buffResHielo, armadura.buffResArcano,
                armadura.buffResAcido, armadura.buffResNecro, armadura.buffResDivino);

            AgregarHabilidad(lineasHabilidades, habilidadesUnicas, armadura.habilidadExtra1);
            AgregarHabilidad(lineasHabilidades, habilidadesUnicas, armadura.habilidadExtra2);
        }
        else if (item is Accesorio accesorio)
        {
            AgregarStatsComunes(
                lineasStats,
                accesorio.buffFuerza, accesorio.buffAgi, accesorio.buffPoder, accesorio.buffIniciativa,
                accesorio.buffApMax, accesorio.buffValMax, accesorio.buffhpMax, accesorio.buffArmadura,
                accesorio.buffDefensa, accesorio.buffTSReflejo, accesorio.buffTSFortaleza, accesorio.buffTSMental,
                accesorio.buffResFuego, accesorio.buffResRayo, accesorio.buffResHielo, accesorio.buffResArcano,
                accesorio.buffResAcido, accesorio.buffResNecro, accesorio.buffResDivino);

            AgregarHabilidad(lineasHabilidades, habilidadesUnicas, accesorio.habilidadExtra1);
            AgregarHabilidad(lineasHabilidades, habilidadesUnicas, accesorio.habilidadExtra2);
        }

        AgregarLineaStat(lineasStats, "Barrera inicial: ", item.barreraInicioCombate);
        AgregarLineaStat(lineasStats, "Evasion inicial: ", item.evasionInicioCombate);
        AgregarLineaStat(lineasStats, "Bonus dano fuego: ", item.bonusDanioFuegoInicioCombate);
        AgregarLineaStat(lineasStats, "Bonus dano hielo: ", item.bonusDanioHieloInicioCombate);
        AgregarLineaStat(lineasStats, "Bonus dano rayo: ", item.bonusDanioRayoInicioCombate);
        AgregarLineaStat(lineasStats, "Bonus dano acido: ", item.bonusDanioAcidoInicioCombate);
        AgregarLineaStat(lineasStats, "Bonus dano arcano: ", item.bonusDanioArcanoInicioCombate);
        AgregarLineaStat(lineasStats, "Bonus dano necro: ", item.bonusDanioNecroInicioCombate);
        AgregarLineaStat(lineasStats, "Bonus dano divino: ", item.bonusDanioDivinoInicioCombate);
        AgregarLineaStat(lineasStats, "Regeneracion vida: ", item.regeneracionVidaInicioCombate);
        AgregarLineaStat(lineasStats, "Regeneracion armadura: ", item.regeneracionArmaduraInicioCombate);
        AgregarLineaStat(lineasStats, "Reduccion dano recibido: ", item.reduccionDanioRecibidoPorcentaje, "%");
        AgregarLineaStat(lineasStats, "Reduccion dano critico recibido: ", item.reduccionDanioCriticoRecibidoPorcentaje, "%");
        AgregarLineaStat(lineasStats, "Resistencia estados: ", item.resistenciaEstadosPorcentaje, "%");
        AgregarLineaStat(lineasStats, "Espinas dano plano: ", item.espinasDanioPlano);
        AgregarLineaStat(lineasStats, "Espinas dano %: ", item.espinasDanioPorcentaje, "%");

        if (lineasStats.Count == 0 && lineasHabilidades.Count == 0 && lineasDebuffImpacto.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder bloque = new StringBuilder();
        bloque.Append("<color=");
        bloque.Append(ColorStats);
        bloque.Append("><b>");
        bloque.Append(Traducir("Efectos del item:"));
        bloque.Append("</b></color>");

        for (int i = 0; i < lineasStats.Count; i++)
        {
            bloque.Append("\n<color=");
            bloque.Append(ColorStats);
            bloque.Append(">");
            bloque.Append(lineasStats[i]);
            bloque.Append("</color>");
        }

        for (int i = 0; i < lineasDebuffImpacto.Count; i++)
        {
            bloque.Append("\n<color=");
            bloque.Append(ColorDebuff);
            bloque.Append(">");
            bloque.Append(lineasDebuffImpacto[i]);
            bloque.Append("</color>");
        }

        for (int i = 0; i < lineasHabilidades.Count; i++)
        {
            bloque.Append("\n<color=");
            bloque.Append(ColorHabilidades);
            bloque.Append(">");
            bloque.Append(lineasHabilidades[i]);
            bloque.Append("</color>");
        }

        return bloque.ToString();
    }

    private static void AgregarStatsComunes(
        List<string> lineas,
        int buffFuerza, int buffAgi, int buffPoder, int buffIniciativa,
        int buffApMax, int buffValMax, int buffHpMax, int buffArmadura,
        int buffDefensa, int buffTSReflejo, int buffTSFortaleza, int buffTSMental,
        int buffResFuego, int buffResRayo, int buffResHielo, int buffResArcano,
        int buffResAcido, int buffResNecro, int buffResDivino)
    {
        AgregarLineaStat(lineas, "Fuerza: ", buffFuerza);
        AgregarLineaStat(lineas, "Agilidad: ", buffAgi);
        AgregarLineaStat(lineas, "Poder: ", buffPoder);
        AgregarLineaStat(lineas, "Iniciativa: ", buffIniciativa);
        AgregarLineaStat(lineas, "PA: ", buffApMax);
        AgregarLineaStat(lineas, "Valentia: ", buffValMax);
        AgregarLineaStat(lineas, "HP Maximo: ", buffHpMax);
        AgregarLineaStat(lineas, "Armadura: ", buffArmadura);
        AgregarLineaStat(lineas, "Defensa: ", buffDefensa);
        AgregarLineaStat(lineas, "TS Reflejos: ", buffTSReflejo);
        AgregarLineaStat(lineas, "TS Fortaleza: ", buffTSFortaleza);
        AgregarLineaStat(lineas, "TS Mental: ", buffTSMental);
        AgregarLineaStat(lineas, "Resistencia Fuego: ", buffResFuego);
        AgregarLineaStat(lineas, "Resistencia Rayo: ", buffResRayo);
        AgregarLineaStat(lineas, "Resistencia Hielo: ", buffResHielo);
        AgregarLineaStat(lineas, "Resistencia Arcano: ", buffResArcano);
        AgregarLineaStat(lineas, "Resistencia Acido: ", buffResAcido);
        AgregarLineaStat(lineas, "Resistencia Necrotica: ", buffResNecro);
        AgregarLineaStat(lineas, "Resistencia Divina: ", buffResDivino);
    }

    private static void AgregarLineaStat(List<string> lineas, string etiqueta, int valor, string sufijo = "")
    {
        if (valor == 0)
        {
            return;
        }

        string valorTexto = valor > 0 ? $"+{valor}" : valor.ToString();
        lineas.Add($"{Traducir(etiqueta)}<b>{valorTexto}{sufijo}</b>");
    }

    private static void AgregarDebuffsImpacto(List<string> lineas, List<DebuffImpactoArmaData> debuffs)
    {
        if (lineas == null || debuffs == null || debuffs.Count == 0)
        {
            return;
        }

        for (int i = 0; i < debuffs.Count; i++)
        {
            DebuffImpactoArmaData efecto = debuffs[i];
            if (efecto == null || !efecto.activo || !efecto.TieneEfectos())
            {
                continue;
            }

            int chance = efecto.probabilidadAplicar;
            if (chance < 0) { chance = 0; }
            if (chance > 100) { chance = 100; }

            int duracion = efecto.duracionRondas < 1 ? 1 : efecto.duracionRondas;
            string nombre = string.IsNullOrWhiteSpace(efecto.nombreDebuff)
                ? Traducir("Debuff de impacto")
                : Traducir(efecto.nombreDebuff);

            StringBuilder encabezado = new StringBuilder();
            encabezado.Append("<b>");
            encabezado.Append(Traducir("Debuff por impacto: "));
            encabezado.Append("</b>");
            encabezado.Append(nombre);
            encabezado.Append(" (");
            encabezado.Append(chance);
            encabezado.Append("%)");

            if (efecto.requiereTiradaSalvacion && efecto.tipoTiradaSalvacion > 0)
            {
                encabezado.Append(" | ");
                encabezado.Append(Traducir("TS"));
                encabezado.Append(" ");
                encabezado.Append(Traducir(NombreTipoTS(efecto.tipoTiradaSalvacion)));
                encabezado.Append(" DC ");
                encabezado.Append(efecto.dificultadSalvacion);
            }

            encabezado.Append(" | ");
            encabezado.Append(Traducir("Duracion: "));
            encabezado.Append(duracion);
            encabezado.Append("T");

            lineas.Add(encabezado.ToString());

            string resumen = ConstruirResumenDebuff(efecto);
            if (!string.IsNullOrWhiteSpace(resumen))
            {
                lineas.Add($"<size=90%>{Traducir("Afecta: ")}{resumen}</size>");
            }
        }
    }

    private static string ConstruirResumenDebuff(DebuffImpactoArmaData efecto)
    {
        List<string> mods = new List<string>();
        AgregarModDebuff(mods, "Fuerza", efecto.modFuerza);
        AgregarModDebuff(mods, "Agilidad", efecto.modAgilidad);
        AgregarModDebuff(mods, "Poder", efecto.modPoder);
        AgregarModDebuff(mods, "Iniciativa", efecto.modIniciativa);
        AgregarModDebuff(mods, "Ataque", efecto.modAtaque);
        AgregarModDebuff(mods, "Defensa", efecto.modDefensa);
        AgregarModDebuff(mods, "Armadura", efecto.modArmadura);
        AgregarModDebuff(mods, "Danio %", efecto.modDanioPorcentaje, "%");
        AgregarModDebuff(mods, "TS Reflejos", efecto.modTSReflejos);
        AgregarModDebuff(mods, "TS Fortaleza", efecto.modTSFortaleza);
        AgregarModDebuff(mods, "TS Mental", efecto.modTSMental);
        AgregarModDebuff(mods, "Res Fuego", efecto.modResFuego);
        AgregarModDebuff(mods, "Res Hielo", efecto.modResHielo);
        AgregarModDebuff(mods, "Res Rayo", efecto.modResRayo);
        AgregarModDebuff(mods, "Res Acido", efecto.modResAcido);
        AgregarModDebuff(mods, "Res Arcano", efecto.modResArcano);
        AgregarModDebuff(mods, "Res Necro", efecto.modResNecro);
        AgregarModDebuff(mods, "Res Divino", efecto.modResDivino);
        AgregarModDebuff(mods, "Crit dado", efecto.modCritDado);
        AgregarModDebuff(mods, "Danio crit %", efecto.modCritDanioPorcentaje, "%");
        AgregarModDebuff(mods, "Sangrado", efecto.stacksSangrado, "T");
        AgregarModDebuff(mods, "Ardiendo", efecto.stacksArdiendo, "T");
        AgregarModDebuff(mods, "Congelado", efecto.stacksCongelado, "T");
        AgregarModDebuff(mods, "Acido", efecto.stacksAcido, "T");
        AgregarModDebuff(mods, "Aturdido", efecto.stacksAturdido, "T");
        AgregarModDebuff(mods, "AP por turno", -efecto.reduccionAPPorTurno);
        AgregarModDebuff(mods, "Resistencias reducidas", efecto.reduccionResistencias, "T");
        AgregarModDebuff(mods, "Condenado", efecto.stacksCondenado, "T");
        AgregarModDebuff(mods, "Ignora armadura", efecto.ignorarArmaduraPlano);
        AgregarModDebuff(mods, "Robo vida", efecto.roboVidaPorcentaje, "%");
        AgregarModDebuff(mods, "Empuje", efecto.empujeCasillas);
        AgregarModDebuff(mods, "Jalon", efecto.jalonCasillas);

        return string.Join(", ", mods);
    }

    private static void AgregarModDebuff(List<string> mods, string etiqueta, int valor, string sufijo = "")
    {
        if (valor == 0)
        {
            return;
        }

        string valorTexto = valor > 0 ? $"+{valor}" : valor.ToString();
        mods.Add($"{Traducir(etiqueta)} {valorTexto}{sufijo}");
    }

    private static string NombreTipoTS(int tipoTS)
    {
        switch (tipoTS)
        {
            case 2: return "Reflejos";
            case 3: return "Mental";
            case 1:
            default:
                return "Fortaleza";
        }
    }

    private static void AgregarHabilidad(List<string> lineas, HashSet<string> dedupe, Habilidad habilidad)
    {
        if (habilidad == null)
        {
            return;
        }

        string nombre = ObtenerNombreHabilidad(habilidad);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return;
        }

        if (!dedupe.Add(nombre))
        {
            return;
        }

        lineas.Add($"<b>{Traducir("Agrega habilidad: ")}</b>{Traducir(nombre)}");
    }

    private static string ObtenerNombreHabilidad(Habilidad habilidad)
    {
        if (!string.IsNullOrWhiteSpace(habilidad.nombre))
        {
            return habilidad.nombre;
        }

        string tipo = habilidad.GetType().Name;
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder(tipo.Length + 8);
        for (int i = 0; i < tipo.Length; i++)
        {
            char c = tipo[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(tipo[i - 1]))
            {
                sb.Append(' ');
            }
            sb.Append(c);
        }

        return sb.ToString();
    }

    private static string Traducir(string txt)
    {
        if (string.IsNullOrEmpty(txt))
        {
            return string.Empty;
        }

        if (TRADU.i == null)
        {
            return txt;
        }

        string traducido = TRADU.i.Traducir(txt);
        if (TRADU.i.nIdioma == 2 && traducido == txt)
        {
            if (txt == "Barrera inicial: ") { return "Starting Barrier: "; }
            if (txt == "Evasion inicial: ") { return "Starting Evasion: "; }
            if (txt == "Bonus dano fuego: ") { return "Bonus Fire Damage: "; }
            if (txt == "Bonus dano hielo: ") { return "Bonus Ice Damage: "; }
            if (txt == "Bonus dano rayo: ") { return "Bonus Lightning Damage: "; }
            if (txt == "Bonus dano acido: ") { return "Bonus Acid Damage: "; }
            if (txt == "Bonus dano arcano: ") { return "Bonus Arcane Damage: "; }
            if (txt == "Bonus dano necro: ") { return "Bonus Necrotic Damage: "; }
            if (txt == "Bonus dano divino: ") { return "Bonus Divine Damage: "; }
            if (txt == "Regeneracion vida: ") { return "Life Regeneration: "; }
            if (txt == "Regeneracion armadura: ") { return "Armor Regeneration: "; }
            if (txt == "Reduccion dano recibido: ") { return "Damage Reduction: "; }
            if (txt == "Reduccion dano critico recibido: ") { return "Critical Damage Reduction: "; }
            if (txt == "Resistencia estados: ") { return "Status Resistance: "; }
            if (txt == "Espinas dano plano: ") { return "Thorns Flat Damage: "; }
            if (txt == "Espinas dano %: ") { return "Thorns Damage %: "; }
            if (txt == "Debuff por impacto: ") { return "On-hit debuff: "; }
            if (txt == "Debuff de impacto") { return "On-hit Debuff"; }
            if (txt == "Duracion: ") { return "Duration: "; }
            if (txt == "Afecta: ") { return "Affects: "; }
            if (txt == "Danio %") { return "Damage %"; }
            if (txt == "Danio crit %") { return "Critical Damage %"; }
            if (txt == "Crit dado") { return "Critical Die"; }
            if (txt == "TS") { return "Save"; }
            if (txt == "Res Fuego") { return "Fire Res"; }
            if (txt == "Res Hielo") { return "Ice Res"; }
            if (txt == "Res Rayo") { return "Lightning Res"; }
            if (txt == "Res Acido") { return "Acid Res"; }
            if (txt == "Res Arcano") { return "Arcane Res"; }
            if (txt == "Res Necro") { return "Necrotic Res"; }
            if (txt == "Res Divino") { return "Divine Res"; }
            if (txt == "Sangrado") { return "Bleed"; }
            if (txt == "Ardiendo") { return "Burning"; }
            if (txt == "Congelado") { return "Frozen"; }
            if (txt == "Acido") { return "Acid"; }
            if (txt == "Aturdido") { return "Stunned"; }
            if (txt == "AP por turno") { return "AP per turn"; }
            if (txt == "Resistencias reducidas") { return "Reduced Resistances"; }
            if (txt == "Condenado") { return "Condemned"; }
            if (txt == "Ignora armadura") { return "Ignores Armor"; }
            if (txt == "Robo vida") { return "Life Steal"; }
            if (txt == "Empuje") { return "Push"; }
            if (txt == "Jalon") { return "Pull"; }
            if (txt == "Valentia: ") { return "Valor: "; }
            if (txt == "HP Maximo: ") { return "Max HP: "; }
            if (txt == "Resistencia Acido: ") { return "Acid Resistance: "; }
            if (txt == "Resistencia Necrotica: ") { return "Necrotic Resistance: "; }
        }

        return traducido;
    }
}
