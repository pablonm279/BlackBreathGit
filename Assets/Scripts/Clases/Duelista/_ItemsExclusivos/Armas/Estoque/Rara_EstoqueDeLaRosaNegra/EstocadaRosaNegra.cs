using UnityEngine;

public class EstocadaRosaNegra : Estocada
{
    private const int DificultadSangrado = 12;
    private const int DuracionSangrado = 2;

    public override void Awake()
    {
        base.Awake();
        ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaIngles;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;
        var statsUI = ObtenerStatsDescripcionUI();

        int atributoMixtoActual = Mathf.CeilToInt((statsUI.Fuerza + statsUI.Agilidad) / 2f);
        int ataqueActual = statsUI.Ataque;
        int criticoBaseMin = Mathf.Clamp(19 - statsUI.CriticoRango, 2, 20);
        string lineaSalvacion = ConstruirLineaSalvacion(esIngles, TipoSalvacionDescripcion.Fortaleza, DificultadSangrado);

        string tituloEs = "Estocada de la Rosa Negra";
        string tituloEn = "Black Rose Thrust";
        string tituloPt = "Estocada da Rosa Negra";
        nombre = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;

        if (esIngles)
        {
            string fuerza = TerminoDescripcion(TerminoDescripcionId.Fuerza, "Strength");
            string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, "Agility");
            string atributoMixto = $"{fuerza}/{agilidad} ({atributoMixtoActual})";
            string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
            string fortaleza = TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza, "Fortitude", "ic_fortaleza");
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
            string danioPerforante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "Piercing damage", "dano_perforante");
            string sangrado = TerminoDescripcion(TerminoDescripcionId.Sangrado, "Bleed", "Estado_sangrano");
            string bonusTiradaNormalizado = FormatoModificadorDescripcion(ataqueActual) + FormatoModificadorDescripcion(bonusAtaque);
            int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                tituloEn,
                "A graceful thrust that can leave the target bleeding.",
                new[]
                {
                    LineaDescripcion("Target", ObjetivoMeleeUnitarioIngles),
                    LineaDescripcion("Effect", $"On hit, deals {FormatearRangoDados(1, 8)} + {atributoMixto} as {danioPerforante}."),
                    LineaDescripcion("Attack Roll", $"1d20 + {atributoMixto}{bonusTiradaNormalizado} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%."),
                    LineaDescripcion("Penetration", $"Armor Penetration: {penetracionArmadura}."),
                    LineaDescripcion("Save", $"Target's {fortaleza} vs DC {DificultadSangrado}.", 1),
                    LineaDescripcion("Failed save", $"Gains {sangrado} ({DuracionSangrado} turns).", 1)
                },
                mostrarIconoMelee: true);
            return;
        }
        if(esPortugues){string forca=TerminoDescripcion(TerminoDescripcionId.Fuerza,"Força");string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,"Agilidade");string mix=$"{forca}/{agi} ({atributoMixtoActual})";string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defesa","IconoDefensa");string fort=TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza,"Fortitude","ic_fortaleza");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string dano=TerminoDescripcion(TerminoDescripcionId.DanioPerforante,"dano Perfurante","dano_perforante");string sang=TerminoDescripcion(TerminoDescripcionId.Sangrado,"Sangramento","Estado_sangrano");string bonus=FormatoModificadorDescripcion(ataqueActual)+FormatoModificadorDescripcion(bonusAtaque);int pct=Mathf.Clamp(21-criticoBaseMin,0,20)*5;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloPt,"Uma estocada elegante que pode deixar o alvo sangrando.",new[]{LineaDescripcion("Alvo","1 alvo ou obstáculo em alcance corpo a corpo"),LineaDescripcion("Efeito",$"Ao acertar, causa {FormatearRangoDados(1,8)} + {mix} como {dano}."),LineaDescripcion("Rolagem de Ataque",$"1d20 + {mix}{bonus} vs {def}. Falha crítica: 5%. {crit}: {pct}%."),LineaDescripcion("Penetração",$"Penetração de Armadura: {penetracionArmadura}."),LineaDescripcion("Salvaguarda",$"{fort} do alvo vs CD {DificultadSangrado}.",1),LineaDescripcion("Falha",$"Recebe {sang} ({DuracionSangrado} turnos).",1)},mostrarIconoMelee:true);return;}

        {string fuerza=TerminoDescripcion(TerminoDescripcionId.Fuerza,"Fuerza");string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,"Agilidad");string mix=$"{fuerza}/{agi} ({atributoMixtoActual})";string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defensa","IconoDefensa");string fort=TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza,"Fortaleza","ic_fortaleza");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string dano=TerminoDescripcion(TerminoDescripcionId.DanioPerforante,"daño Perforante","dano_perforante");string sang=TerminoDescripcion(TerminoDescripcionId.Sangrado,"Sangrado","Estado_sangrano");string bonus=FormatoModificadorDescripcion(ataqueActual)+FormatoModificadorDescripcion(bonusAtaque);int pct=Mathf.Clamp(21-criticoBaseMin,0,20)*5;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloEs,"Una estocada elegante que puede dejar sangrando al objetivo.",new[]{LineaDescripcion("Objetivo","1 objetivo u obstáculo en alcance cuerpo a cuerpo"),LineaDescripcion("Efecto",$"Al impactar, inflige {FormatearRangoDados(1,8)} + {mix} como {dano}."),LineaDescripcion("Tirada de Ataque",$"1d20 + {mix}{bonus} vs {def}. Pifia: 5%. {crit}: {pct}%."),LineaDescripcion("Penetración",$"Penetración de Armadura: {penetracionArmadura}."),LineaDescripcion("Salvación",$"{fort} del objetivo vs CD {DificultadSangrado}.",1),LineaDescripcion("Salvación fallida",$"Obtiene {sang} ({DuracionSangrado} turnos).",1)},mostrarIconoMelee:true);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Melee\n";
            cuerpo += "<b>Target:</b> 1 enemy in front range\n";
            cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> + Attack ({ataqueActual}) vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Damage:</b> 1d8 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> | <b>Type:</b> Piercing\n";
            cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Weapon effect:</b> on hit, applies Black Rose for {DuracionSangrado} turns if the target fails a Fortitude Save\n";
            cuerpo += $"{lineaSalvacion}\n";
            cuerpo += "<b>On failed save:</b> gains Bleed";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Alvo:</b> 1 inimigo em alcance frontal\n";
            cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) vs Defesa. Falha critica: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Dano:</b> 1d8 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> | <b>Tipo:</b> Perfurante\n";
            cuerpo += $"<b>Penetracao de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Efeito da arma:</b> ao acertar, aplica Rosa Negra por {DuracionSangrado} turnos se o alvo falhar em uma resistencia de Fortitude\n";
            cuerpo += $"{lineaSalvacion}\n";
            cuerpo += "<b>Se falhar na resistencia:</b> ganha Sangramento";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Objetivo:</b> 1 enemigo en alcance frontal\n";
            cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) vs Defensa. Pifia: 1. Crítico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Daño:</b> 1d8 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> | <b>Tipo:</b> Perforante\n";
            cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Efecto del arma:</b> al impactar, aplica Rosa Negra por {DuracionSangrado} turnos si el objetivo falla una TS Fortaleza\n";
            cuerpo += $"{lineaSalvacion}\n";
            cuerpo += "<b>Si falla TS:</b> gana Sangrado";
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
            : esPortugues
                ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
                : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

        txtDescripcion = ConstruirDescripcionEstandar(
            nombre,
            esIngles
                ? "A graceful thrust that leaves a dark bloom of blood behind."
                : esPortugues
                    ? "Uma estocada graciosa que deixa um florir sombrio de sangue para tras."
                    : "Una estocada graciosa que deja un florecer sombrio de sangre a su paso.",
            cuerpo,
            costos,
            "#5dade2");
        string colorEncabezadoNuevo = "#44d3ec";
        string colorValorNuevo = "#ffffff";
        string atributoNuevo = esIngles
            ? $"<color=#d9822b>Strength</color>/<color=#7fa35a>Agility</color> ({atributoMixtoActual})"
            : esPortugues
                ? $"<color=#d9822b>Forca</color>/<color=#7fa35a>Agilidade</color> ({atributoMixtoActual})"
                : $"<color=#d9822b>Fuerza</color>/<color=#7fa35a>Agilidad</color> ({atributoMixtoActual})";
        string rangoDanioNuevo = FormatearRangoDados(1, 8);
        string bonusTiradaNuevo = FormatoModificadorDescripcion(ataqueActual) + FormatoModificadorDescripcion(bonusAtaque);
        int criticoPorcentajeNuevo = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
        string efectoNuevo = esIngles ? "On hit: Fortitude save vs DC 12; on failed save, Bleed for 2 turns" : esPortugues ? "Ao acertar: resistencia Fortitude vs CD 12; se falhar, Sangramento por 2 turnos" : "Al impactar: TS Fortaleza DC 12; si falla, Sangrado por 2 turnos";
        string cuerpoNuevo = "";
        if (esIngles)
        {
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Type:</b></color> <color={colorValorNuevo}>Melee attack</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Target:</b></color> <color={colorValorNuevo}>1 enemy or obstacle in frontal melee range</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Roll:</b></color> <color={colorValorNuevo}>1d20 + {atributoNuevo}{bonusTiradaNuevo} vs Defense. Fumble: 5%. Crit: {criticoPorcentajeNuevo}%</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Damage:</b></color> <color={colorValorNuevo}>{rangoDanioNuevo} + {atributoNuevo}. Type: Piercing</color>\n";
            if (penetracionArmadura > 0) { cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Armor penetration:</b></color> <color={colorValorNuevo}>{penetracionArmadura}</color>\n"; }
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Weapon effect:</b></color> <color={colorValorNuevo}>{efectoNuevo}</color>";
        }
        else if (esPortugues)
        {
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Tipo:</b></color> <color={colorValorNuevo}>Ataque corpo a corpo</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Alvo:</b></color> <color={colorValorNuevo}>1 inimigo ou obstaculo no alcance frontal corpo a corpo</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Rolagem:</b></color> <color={colorValorNuevo}>1d20 + {atributoNuevo}{bonusTiradaNuevo} vs Defesa. Falha critica: 5%. Critico: {criticoPorcentajeNuevo}%</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Daño:</b></color> <color={colorValorNuevo}>{rangoDanioNuevo} + {atributoNuevo}. Tipo: Perfurante</color>\n";
            if (penetracionArmadura > 0) { cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Penetracao de armadura:</b></color> <color={colorValorNuevo}>{penetracionArmadura}</color>\n"; }
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Efeito da arma:</b></color> <color={colorValorNuevo}>{efectoNuevo}</color>";
        }
        else
        {
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Tipo:</b></color> <color={colorValorNuevo}>Ataque melee</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Objetivo:</b></color> <color={colorValorNuevo}>1 enemigo u obstáculo en alcance melee frontal</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Tirada:</b></color> <color={colorValorNuevo}>1d20 + {atributoNuevo}{bonusTiradaNuevo} vs Defensa. Pifia: 5%. Crítico: {criticoPorcentajeNuevo}%</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Daño:</b></color> <color={colorValorNuevo}>{rangoDanioNuevo} + {atributoNuevo}. Tipo: Perforante</color>\n";
            if (penetracionArmadura > 0) { cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Penetracion de armadura:</b></color> <color={colorValorNuevo}>{penetracionArmadura}</color>\n"; }
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Efecto del arma:</b></color> <color={colorValorNuevo}>{efectoNuevo}</color>";
        }

        txtDescripcion = ConstruirDescripcionTooltipNueva(
            nombre,
            esIngles ? "Item thrust with a weapon-specific effect." : esPortugues ? "Estocada de item com efeito especifico da arma." : "Estocada de item con efecto propio del arma.",
            cuerpoNuevo);
    }
}
