using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class ItemTooltipFormatter
{
    private const string ColorTituloItem = "#8AA1AF";
    private const string ColorMetaSecundaria = "#7A8790";
    private const string ColorSeparador = "#3E4C52";
    private const string ColorRarezaComun = "#5C5C5C";
    private const string ColorRarezaInfrecuente = "#B0B0B0";
    private const string ColorRarezaRaro = "#B08D3B";
    private const string ColorRarezaEpico = "#8B2E6E";
    private const string ColorRarezaLegendario = "#C9A227";
    private const string ColorRarezaArtefacto = "#00C8D7";
    private const string ColorEncabezadoEfectos = "#4F664B";
    private const string ColorStats = "#7F9B79";
    private const string ColorHabilidades = "#C7B27A";
    private const string ColorDebuff = "#D28E6A";
    private const string ColorConsumible = "#9BC88A";

    public static string ConstruirTooltip(Item item, bool incluirNombre = true, string sufijoNombre = null)
    {
        if (item == null)
        {
            return string.Empty;
        }

        StringBuilder texto = new StringBuilder();
        if (incluirNombre && !string.IsNullOrWhiteSpace(item.sNombreItem))
        {
            string nombreVisible = TraducirNombreVisibleItem(item);
            if (!string.IsNullOrWhiteSpace(sufijoNombre))
            {
                nombreVisible += " " + sufijoNombre;
            }

            texto.Append(FormatearTituloItem(nombreVisible));
        }

        string lineaRareza = ConstruirLineaRareza(item);
        if (!string.IsNullOrEmpty(lineaRareza))
        {
            if (texto.Length > 0)
            {
                texto.Append("\n");
            }

            texto.Append(lineaRareza);
        }

        string lineaTipo = ConstruirLineaTipo(item);
        if (!string.IsNullOrEmpty(lineaTipo))
        {
            if (texto.Length > 0)
            {
                texto.Append("\n");
            }

            texto.Append(lineaTipo);
        }

        string bloqueEfectos = ConstruirBloqueEfectos(item);
        if (!string.IsNullOrEmpty(bloqueEfectos))
        {
            if (texto.Length > 0)
            {
                texto.Append("\n");
                texto.Append(ConstruirSeparador());
                texto.Append("\n");
            }

            texto.Append(bloqueEfectos);
        }

        return texto.ToString();
    }

    public static string ConstruirTooltipSoloEfectos(Item item)
    {
        return ConstruirBloqueEfectos(item);
    }

    public static List<string> ConstruirTooltipsHabilidades(Item item)
    {
        List<Habilidad> habilidades = new List<Habilidad>();
        if (item is Arma arma)
        {
            habilidades.Add(arma.habilidadAtaque);
            habilidades.Add(arma.habilidadExtra1);
            habilidades.Add(arma.habilidadExtra2);
        }
        else if (item is Armadura armadura)
        {
            habilidades.Add(armadura.habilidadExtra1);
            habilidades.Add(armadura.habilidadExtra2);
        }
        else if (item is Accesorio accesorio)
        {
            habilidades.Add(accesorio.habilidadExtra1);
            habilidades.Add(accesorio.habilidadExtra2);
        }

        List<string> tooltips = new List<string>();
        HashSet<string> nombresUnicos = new HashSet<string>();
        for (int i = 0; i < habilidades.Count; i++)
        {
            Habilidad habilidad = habilidades[i];
            if (habilidad == null)
            {
                continue;
            }

            string descripcionAnterior = habilidad.txtDescripcion;
            try
            {
                habilidad.ActualizarDescripcion();
            }
            catch
            {
                habilidad.txtDescripcion = descripcionAnterior;
            }

            string descripcion = QuitarDescripcionCursiva(habilidad.txtDescripcion);
            string nombre = ObtenerNombreHabilidad(habilidad);
            if (string.IsNullOrWhiteSpace(descripcion) || !nombresUnicos.Add(nombre))
            {
                continue;
            }

            tooltips.Add(descripcion);
        }

        return tooltips;
    }

    private static string QuitarDescripcionCursiva(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return string.Empty;
        }

        string resultado = Regex.Replace(
            descripcion,
            @"<color(?:=[^>]*)?>\s*<i>.*?</i>\s*</color>\s*",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        resultado = Regex.Replace(
            resultado,
            @"<i>.*?</i>\s*",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        resultado = Regex.Replace(resultado, @"[ \t]*(?:\r?\n[ \t]*){2,}", "\n");
        return resultado.Trim();
    }

    private static string FormatearTituloItem(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return string.Empty;
        }

        return $"<size=112%><color={ColorTituloItem}><b>{nombre}</b></color></size>";
    }

    private static string ObtenerNombreVisibleItem(Item item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.sNombreItem))
        {
            return string.Empty;
        }

        string nombre = item.sNombreItem.Trim();
        if (item.nivelMejora <= 0)
        {
            return nombre;
        }

        string sufijo = " +" + item.nivelMejora;
        if (nombre.EndsWith(sufijo))
        {
            return nombre;
        }

        return nombre + sufijo;
    }

    private static string TraducirNombreVisibleItem(Item item)
    {
        string nombreVisible = ObtenerNombreVisibleItem(item);
        if (string.IsNullOrEmpty(nombreVisible) || TRADU.i == null)
        {
            return nombreVisible;
        }

        if (item != null && item.nivelMejora > 0)
        {
            string sufijo = " +" + item.nivelMejora;
            if (nombreVisible.EndsWith(sufijo))
            {
                string nombreBase = nombreVisible.Substring(0, nombreVisible.Length - sufijo.Length);
                return Traducir(nombreBase) + sufijo;
            }
        }

        return Traducir(nombreVisible);
    }

    private static string ConstruirSeparador()
    {
        return $"<size=82%><color={ColorSeparador}>----------------------</color></size>";
    }

    private static string ConstruirLineaRareza(Item item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        string rareza = ObtenerNombreRareza(item.iRareza);
        if (string.IsNullOrEmpty(rareza))
        {
            return string.Empty;
        }

        string colorRareza = ObtenerColorRareza(item.iRareza);
        return $"<size=92%><b>{Traducir("Rareza: ")}</b><color={colorRareza}>{Traducir(rareza)}</color></size>";
    }

    private static string ConstruirLineaTipo(Item item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        string tipo = ObtenerTipoItem(item);
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return string.Empty;
        }

        StringBuilder valorTipo = new StringBuilder();
        valorTipo.Append(Traducir(tipo));

        int claseForzada = ObtenerClaseForzada(item);
        if (claseForzada > 0)
        {
            string claseNombre = ObtenerNombreClase(claseForzada);
            if (!string.IsNullOrWhiteSpace(claseNombre))
            {
                valorTipo.Append(" - ");
                valorTipo.Append(Traducir(claseNombre));
            }
        }

        return $"<size=90%><i><color={ColorMetaSecundaria}><b>{Traducir("Tipo de item: ")}</b>{valorTipo}</color></i></size>";
    }

    private static string ObtenerTipoItem(Item item)
    {
        if (item == null) { return string.Empty; }
        if (item is Arma) { return ObtenerTipoArma(item); }
        if (item is Armadura) { return ObtenerTipoArmadura(item); }
        if (item is Accesorio) { return "Accesorio"; }
        if (item is Consumible) { return "Consumible"; }
        return "Item";
    }

    private static string ObtenerTipoArma(Item item)
    {
        int clase = ObtenerClasePrincipal(item);
        switch (clase)
        {
            case 1: return "Mandoble";
            case 2: return "Arco";
            case 3: return "Baculo";
            case 4: return "Espada Corta";
            case 5: return "Guantelete";
            case 6: return "Estoque";
        }

        string nombre = item != null && !string.IsNullOrWhiteSpace(item.sNombreItem)
            ? item.sNombreItem.ToLowerInvariant()
            : string.Empty;

        if (nombre.Contains("mandoble")) { return "Mandoble"; }
        if (nombre.Contains("guantelete")) { return "Guantelete"; }
        if (nombre.Contains("bast") || nombre.Contains("baculo")) { return "Baculo"; }
        if (nombre.Contains("arco")) { return "Arco"; }
        if (nombre.Contains("estoque")) { return "Estoque"; }
        if (nombre.Contains("espada")) { return "Espada Corta"; }
        return "Arma";
    }

    private static string ObtenerTipoArmadura(Item item)
    {
        string nombre = item != null && !string.IsNullOrWhiteSpace(item.sNombreItem)
            ? item.sNombreItem.ToLowerInvariant()
            : string.Empty;

        if (nombre.Contains("coraza"))
        {
            return "Coraza";
        }

        if (nombre.Contains("gamb"))
        {
            return "Gambeson";
        }

        if (nombre.Contains("vestidura"))
        {
            return "Vestidura";
        }

        int clase = ObtenerClasePrincipal(item);
        if (clase == 1) { return "Coraza"; }
        if (clase == 6) { return "Gambeson"; }
        if (clase == 3) { return "Vestidura"; }
        return "Armadura";
    }

    private static int ObtenerClasePrincipal(Item item)
    {
        if (item == null || item.IDClasesQuePuedenUsarEsteItem == null)
        {
            return -1;
        }

        for (int i = 0; i < item.IDClasesQuePuedenUsarEsteItem.Count; i++)
        {
            int id = item.IDClasesQuePuedenUsarEsteItem[i];
            if (id >= 1 && id <= 6)
            {
                return id;
            }
        }

        return -1;
    }

    private static int ObtenerClaseForzada(Item item)
    {
        if (item == null || item.UsaTodasLasClases() || item.IDClasesQuePuedenUsarEsteItem == null)
        {
            return -1;
        }

        int clase = -1;
        for (int i = 0; i < item.IDClasesQuePuedenUsarEsteItem.Count; i++)
        {
            int id = item.IDClasesQuePuedenUsarEsteItem[i];
            if (id < 1 || id > 6)
            {
                continue;
            }

            if (clase == -1)
            {
                clase = id;
                continue;
            }

            if (clase != id)
            {
                return -1;
            }
        }

        return clase;
    }

    private static string ObtenerNombreClase(int idClase)
    {
        switch (idClase)
        {
            case 1: return "Caballero";
            case 2: return "Explorador";
            case 3: return "Purificadora";
            case 4: return "Acechador";
            case 5: return "Canalizador";
            case 6: return "Duelista";
            default: return string.Empty;
        }
    }

    private static string ObtenerNombreRareza(int iRareza)
    {
        switch (iRareza)
        {
            case 0: return "Común";
            case 1: return "Infrecuente";
            case 2: return "Raro";
            case 3: return "Épico";
            case 4: return "Legendario";
            case 5: return "Artefacto";
            default: return "Desconocida";
        }
    }

    private static string ObtenerColorRareza(int iRareza)
    {
        switch (iRareza)
        {
            case 0: return ColorRarezaComun;
            case 1: return ColorRarezaInfrecuente;
            case 2: return ColorRarezaRaro;
            case 3: return ColorRarezaEpico;
            case 4: return ColorRarezaLegendario;
            case 5: return ColorRarezaArtefacto;
            default: return ColorMetaSecundaria;
        }
    }

    private static string ConstruirBloqueEfectos(Item item)
    {
        List<string> lineasStats = new List<string>();
        List<string> lineasHabilidades = new List<string>();
        List<string> lineasDebuffImpacto = new List<string>();
        List<string> lineasConsumible = new List<string>();
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
        else if (item is Consumible consumible)
        {
            AgregarLineasConsumible(lineasConsumible, consumible.ObtenerEfectoConsumibleNormalizado());
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
        if (item.IDEfectoEspecial == 5)
        {
            AgregarLineaStat(lineasStats, "Danio %: ", 10, "%");
        }

        if (lineasStats.Count == 0 && lineasHabilidades.Count == 0 && lineasDebuffImpacto.Count == 0 && lineasConsumible.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder bloque = new StringBuilder();
        bloque.Append("<color=");
        bloque.Append(ColorEncabezadoEfectos);
        bloque.Append("><size=93%><b>");
        bloque.Append(Traducir("Efectos del item:"));
        bloque.Append("</b></size></color>");

        for (int i = 0; i < lineasStats.Count; i++)
        {
            bloque.Append("\n<color=");
            bloque.Append(ColorStats);
            bloque.Append(">");
            bloque.Append(FormatearLineaDetalle(lineasStats[i]));
            bloque.Append("</color>");
        }

        for (int i = 0; i < lineasDebuffImpacto.Count; i++)
        {
            bloque.Append("\n<color=");
            bloque.Append(ColorDebuff);
            bloque.Append(">");
            bloque.Append(FormatearLineaDetalle(lineasDebuffImpacto[i]));
            bloque.Append("</color>");
        }

        for (int i = 0; i < lineasConsumible.Count; i++)
        {
            bloque.Append("\n<color=");
            bloque.Append(ColorConsumible);
            bloque.Append(">");
            bloque.Append(FormatearLineaDetalle(lineasConsumible[i]));
            bloque.Append("</color>");
        }

        for (int i = 0; i < lineasHabilidades.Count; i++)
        {
            bloque.Append("\n<color=");
            bloque.Append(ColorHabilidades);
            bloque.Append(">");
            bloque.Append(FormatearLineaDetalle(lineasHabilidades[i]));
            bloque.Append("</color>");
        }

        return bloque.ToString();
    }

    private static string FormatearLineaDetalle(string linea)
    {
        if (string.IsNullOrWhiteSpace(linea))
        {
            return string.Empty;
        }

        return "<size=94%>- " + linea + "</size>";
    }

    private static void AgregarLineasConsumible(List<string> lineas, ConsumibleEfectoData data)
    {
        if (lineas == null || data == null)
        {
            return;
        }

        List<string> partesCuracion = new List<string>();
        if (data.curacionBase > 0)
        {
            partesCuracion.Add("+" + data.curacionBase);
        }

        if (data.curacionDadosCantidad > 0 && data.curacionDadosCaras > 0)
        {
            partesCuracion.Add(data.curacionDadosCantidad + "d" + data.curacionDadosCaras);
        }

        if (data.curacionPorcentajeHPMax > 0)
        {
            partesCuracion.Add(data.curacionPorcentajeHPMax + "% " + Traducir("HP Max"));
        }

        if (partesCuracion.Count > 0)
        {
            lineas.Add($"{Traducir("Cura: ")}<b>{string.Join(" + ", partesCuracion)}</b>");
        }

        if (data.removerDebuffs)
        {
            lineas.Add("<b>" + Traducir("Remueve debuffs") + "</b>");
        }

        if (data.removerBuffs)
        {
            lineas.Add("<b>" + Traducir("Remueve buffs") + "</b>");
        }

        if (data.removerEstadosNegativos)
        {
            lineas.Add("<b>" + Traducir("Limpia estados negativos") + "</b>");
        }

        AgregarLineaStat(lineas, "Regeneracion vida (directo): ", data.modificarRegeneracionVida);
        AgregarLineaStat(lineas, "Regeneracion armadura (directo): ", data.modificarRegeneracionArmadura);
        AgregarLineaStat(lineas, "Evasion (directo): ", data.modificarEvasion);

        ConsumibleBuffData buff = data.buff;
        bool tieneBuffStats = buff != null && buff.TieneCambios();
        bool tieneBuffReferencia = data.buffReferencia != null;

        if (!data.aplicarBuff || (!tieneBuffStats && !tieneBuffReferencia))
        {
            return;
        }

        string nombreBuff = data.nombreBuff;
        if (string.IsNullOrWhiteSpace(nombreBuff) || nombreBuff == "Efecto de consumible")
        {
            nombreBuff = tieneBuffReferencia && !string.IsNullOrWhiteSpace(data.buffReferencia.buffNombre)
                ? data.buffReferencia.buffNombre
                : "Efecto de consumible";
        }

        nombreBuff = Traducir(nombreBuff);

        int duracionRondas = data.duracionBuffRondas;
        if (duracionRondas == 0 && tieneBuffReferencia)
        {
            duracionRondas = data.buffReferencia.DuracionBuffRondas;
        }

        string duracion = duracionRondas == -1
            ? Traducir("combate")
            : duracionRondas + "T";

        lineas.Add($"<b>{Traducir("Aplica buff: ")}</b>{nombreBuff} ({Traducir("Duracion: ")}{duracion})");

        if (!tieneBuffStats)
        {
            return;
        }

        AgregarLineaStat(lineas, "Fuerza: ", buff.cantAtFue);
        AgregarLineaStat(lineas, "Agilidad: ", buff.cantAtAgi);
        AgregarLineaStat(lineas, "Poder: ", buff.cantAtPod);
        AgregarLineaStat(lineas, "Iniciativa: ", buff.cantIniciativa);
        AgregarLineaStat(lineas, "PA: ", buff.cantAPMax);
        AgregarLineaStat(lineas, "Valentía: ", buff.cantPMMax);
        AgregarLineaStat(lineas, "HP Maximo: ", buff.cantHPMax);
        AgregarLineaStat(lineas, "Armadura: ", buff.cantArmadura);
        AgregarLineaStat(lineas, "Defensa: ", buff.cantDefensa);
        AgregarLineaStat(lineas, "Ataque: ", buff.cantAtaque);
        AgregarLineaStat(lineas, "Danio %: ", buff.cantDanioPorcentaje, "%");
        AgregarLineaStat(lineas, "Crit dado: ", buff.cantCritDado);
        AgregarLineaStat(lineas, "Danio crit %: ", buff.cantCritDanio, "%");
        AgregarLineaStat(lineas, "TS Reflejos: ", buff.cantTsReflejos);
        AgregarLineaStat(lineas, "TS Fortaleza: ", buff.cantTsFortaleza);
        AgregarLineaStat(lineas, "TS Mental: ", buff.cantTsMental);
        AgregarLineaStat(lineas, "Resistencia Fuego: ", buff.cantResFue);
        AgregarLineaStat(lineas, "Resistencia Hielo: ", buff.cantResHie);
        AgregarLineaStat(lineas, "Resistencia Rayo: ", buff.cantResRay);
        AgregarLineaStat(lineas, "Resistencia Acido: ", buff.cantResAci);
        AgregarLineaStat(lineas, "Resistencia Arcano: ", buff.cantResArc);
        AgregarLineaStat(lineas, "Resistencia Necrotica: ", buff.cantResNec);
        AgregarLineaStat(lineas, "Resistencia Divina: ", buff.cantResDiv);
        AgregarLineaStat(lineas, "Barrera inicial: ", buff.cantBarrera);
        AgregarLineaStat(lineas, "Bonus dano fuego: ", buff.cantDamBonusElementalFue);
        AgregarLineaStat(lineas, "Bonus dano hielo: ", buff.cantDamBonusElementalHie);
        AgregarLineaStat(lineas, "Bonus dano rayo: ", buff.cantDamBonusElementalRay);
        AgregarLineaStat(lineas, "Bonus dano acido: ", buff.cantDamBonusElementalAci);
        AgregarLineaStat(lineas, "Bonus dano arcano: ", buff.cantDamBonusElementalArc);
        AgregarLineaStat(lineas, "Bonus dano necro: ", buff.cantDamBonusElementalNec);
        AgregarLineaStat(lineas, "Bonus dano divino: ", buff.cantDamBonusElementalDiv);
        AgregarLineaStat(lineas, "Penetracion armadura: ", buff.cantPenetracionArmadura);
        AgregarLineaStat(lineas, "Reduccion dano recibido: ", buff.cantReduccionDanioRecibidoPorcentaje, "%");
        AgregarLineaStat(lineas, "Reduccion dano critico recibido: ", buff.cantReduccionDanioCriticoRecibidoPorcentaje, "%");
        AgregarLineaStat(lineas, "Resistencia estados: ", buff.cantResistenciaEstadosPorcentaje, "%");
        AgregarLineaStat(lineas, "Espinas dano plano: ", buff.cantEspinasDanioPlano);
        AgregarLineaStat(lineas, "Espinas dano %: ", buff.cantEspinasDanioPorcentaje, "%");
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
        AgregarLineaStat(lineas, "Valentía: ", buffValMax);
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
        string tituloDescripcion = ObtenerTituloDescripcionHabilidad(habilidad);
        if (!string.IsNullOrWhiteSpace(tituloDescripcion))
        {
            return tituloDescripcion;
        }

        if (!string.IsNullOrWhiteSpace(habilidad.nombre))
        {
            return habilidad.nombre;
        }

        string tipo = habilidad.GetType().Name;
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return string.Empty;
        }

        if (tipo.StartsWith("REPRESENTACION"))
        {
            tipo = tipo.Substring("REPRESENTACION".Length);
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

    private static string ObtenerTituloDescripcionHabilidad(Habilidad habilidad)
    {
        string descripcion = habilidad.txtDescripcion;
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            try
            {
                habilidad.ActualizarDescripcion();
                descripcion = habilidad.txtDescripcion;
            }
            catch
            {
                return string.Empty;
            }
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return string.Empty;
        }

        int inicioTitulo = descripcion.IndexOf("<b>");
        if (inicioTitulo < 0)
        {
            return string.Empty;
        }

        inicioTitulo += 3;
        int finTitulo = descripcion.IndexOf("</b>", inicioTitulo);
        if (finTitulo <= inicioTitulo)
        {
            return string.Empty;
        }

        return descripcion.Substring(inicioTitulo, finTitulo - inicioTitulo).Trim();
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
        if (TRADU.i.nIdioma == 1 && traducido == txt && txt == "Defensa") { return "Defesa"; }
        if (TRADU.i.nIdioma == 1 && traducido == txt && txt == "TS Mental") { return "TR Mental"; }
        if (TRADU.i.nIdioma == 2 && traducido == txt)
        {
            if (txt == "Tipo de item: ") { return "Item type: "; }
            if (txt == "Arma") { return "Weapon"; }
            if (txt == "Armadura") { return "Armor"; }
            if (txt == "Accesorio") { return "Accessory"; }
            if (txt == "Consumible") { return "Consumable"; }
            if (txt == "Mandoble") { return "Greatsword"; }
            if (txt == "Guantelete") { return "Gauntlet"; }
            if (txt == "Baculo") { return "Staff"; }
            if (txt == "Báculo") { return "Staff"; }
            if (txt == "Arco") { return "Bow"; }
            if (txt == "Estoque") { return "Estoc"; }
            if (txt == "Espada Corta") { return "Short Sword"; }
            if (txt == "Coraza") { return "Cuirass"; }
            if (txt == "Gambeson") { return "Gambeson"; }
            if (txt == "Vestidura") { return "Vestment"; }
            if (txt == "Caballero") { return "Knight"; }
            if (txt == "Explorador") { return "Explorer"; }
            if (txt == "Purificadora") { return "Purifier"; }
            if (txt == "Acechador") { return "Stalker"; }
            if (txt == "Canalizador") { return "Channeler"; }
            if (txt == "Duelista") { return "Duelist"; }
            if (txt == "Rareza: ") { return "Rarity: "; }
            if (txt == "Común") { return "Common"; }
            if (txt == "Infrecuente") { return "Uncommon"; }
            if (txt == "Raro") { return "Rare"; }
            if (txt == "Épico") { return "Epic"; }
            if (txt == "Legendario") { return "Legendary"; }
            if (txt == "Artefacto") { return "Artifact"; }
            if (txt == "Desconocida") { return "Unknown"; }
            if (txt == "Barrera inicial: ") { return "Starting Barrier: "; }
            if (txt == "Evasion inicial: ") { return "Starting Evasion: "; }
            if (txt == "Cura: ") { return "Heals: "; }
            if (txt == "Remueve debuffs") { return "Removes debuffs"; }
            if (txt == "Remueve buffs") { return "Removes buffs"; }
            if (txt == "Limpia estados negativos") { return "Cleanses negative states"; }
            if (txt == "Regeneracion vida (directo): ") { return "Life Regeneration (direct): "; }
            if (txt == "Regeneracion armadura (directo): ") { return "Armor Regeneration (direct): "; }
            if (txt == "Evasion (directo): ") { return "Evasion (direct): "; }
            if (txt == "Aplica buff: ") { return "Applies buff: "; }
            if (txt == "Efecto de consumible") { return "Consumable Effect"; }
            if (txt == "combate") { return "combat"; }
            if (txt == "HP Max") { return "Max HP"; }
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
            if (txt == "Ataque: ") { return "Attack: "; }
            if (txt == "Crit dado: ") { return "Critical Die: "; }
            if (txt == "Danio crit %: ") { return "Critical Damage %: "; }
            if (txt == "Penetracion armadura: ") { return "Armor Penetration: "; }
            if (txt == "Debuff por impacto: ") { return "On-hit debuff: "; }
            if (txt == "Debuff de impacto") { return "On-hit Debuff"; }
            if (txt == "Duracion: ") { return "Duration: "; }
            if (txt == "Afecta: ") { return "Affects: "; }
            if (txt == "Defensa") { return "Defense"; }
            if (txt == "TS Mental") { return "Mental Save"; }
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
            if (txt == "Valentía: " || txt == "Valentia: ") { return "Valour: "; }
            if (txt == "HP Maximo: ") { return "Max HP: "; }
            if (txt == "Resistencia Acido: ") { return "Acid Resistance: "; }
            if (txt == "Resistencia Necrotica: ") { return "Necrotic Resistance: "; }
        }

        return traducido;
    }
}


