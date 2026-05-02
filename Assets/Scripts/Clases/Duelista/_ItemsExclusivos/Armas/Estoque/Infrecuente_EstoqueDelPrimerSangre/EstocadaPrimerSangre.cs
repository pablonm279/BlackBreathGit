using UnityEngine;

public class EstocadaPrimerSangre : Estocada
{
    private const int BonusAtaquePrimerSangre = 2;
    private const int BonusDanioPlanoPrimerSangre = 2;
    private const int BonusCriticoPrimerSangre = 1;

    public override void Awake()
    {
        base.Awake();
        bonusAtaque = 0;
        XdDanio = 1;
        daniodX = 8;
        criticoRangoHab = 0;
        tipoDanio = 2;
        ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaIngles;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;
        var statsUI = ObtenerStatsDescripcionUI();

        int atributoMixtoActual = Mathf.CeilToInt((statsUI.Fuerza + statsUI.Agilidad) / 2f);
        int ataqueActual = statsUI.Ataque;
        int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
        int criticoPrimerSangre = Mathf.Clamp(criticoBaseMin - BonusCriticoPrimerSangre, 2, 20);

        string tituloEs = "Estocada del Primer Sangre";
        string tituloEn = "First Blood Thrust";
        string tituloPt = "Estocada do Primeiro Sangue";
        nombre = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Melee\n";
            cuerpo += "<b>Target:</b> 1 enemy in front range\n";
            cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> + Attack ({ataqueActual}) vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Damage:</b> 1d8 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> | <b>Type:</b> Piercing\n";
            cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>If target is at full HP:</b> +{BonusAtaquePrimerSangre} Attack, +{BonusDanioPlanoPrimerSangre} flat damage, crit {criticoPrimerSangre}-20";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Alvo:</b> 1 inimigo em alcance frontal\n";
            cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) vs Defesa. Falha critica: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Dano:</b> 1d8 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> | <b>Tipo:</b> Perfurante\n";
            cuerpo += $"<b>Penetracao de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Se o alvo estiver com HP cheio:</b> +{BonusAtaquePrimerSangre} Ataque, +{BonusDanioPlanoPrimerSangre} dano plano, critico {criticoPrimerSangre}-20";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Objetivo:</b> 1 enemigo en alcance frontal\n";
            cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Danio:</b> 1d8 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> | <b>Tipo:</b> Perforante\n";
            cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Si el objetivo esta con vida completa:</b> +{BonusAtaquePrimerSangre} Ataque, +{BonusDanioPlanoPrimerSangre} de dano plano, critico {criticoPrimerSangre}-20";
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
            : esPortugues
                ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
                : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentia: {costoPM}\n- Esforzable: Si ({esforzable})";

        txtDescripcion = ConstruirDescripcionEstandar(
            nombre,
            esIngles
                ? "A refined opening thrust that punishes untouched opponents."
                : esPortugues
                    ? "Uma estocada de abertura refinada que pune adversarios intactos."
                    : "Una estocada de apertura refinada que castiga a rivales intactos.",
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
        string efectoNuevo = esIngles ? "If target is at full HP: +2 attack, +2 damage, +5% Crit" : esPortugues ? "Se o alvo esta com HP cheio: +2 ataque, +2 dano, +5% Critico" : "Si el objetivo tiene HP completo: +2 ataque, +2 dano, +5% Critico";
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
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Dano:</b></color> <color={colorValorNuevo}>{rangoDanioNuevo} + {atributoNuevo}. Tipo: Perfurante</color>\n";
            if (penetracionArmadura > 0) { cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Penetracao de armadura:</b></color> <color={colorValorNuevo}>{penetracionArmadura}</color>\n"; }
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Efeito da arma:</b></color> <color={colorValorNuevo}>{efectoNuevo}</color>";
        }
        else
        {
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Tipo:</b></color> <color={colorValorNuevo}>Ataque melee</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Objetivo:</b></color> <color={colorValorNuevo}>1 enemigo u obstaculo en alcance melee frontal</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Tirada:</b></color> <color={colorValorNuevo}>1d20 + {atributoNuevo}{bonusTiradaNuevo} vs Defensa. Pifia: 5%. Critico: {criticoPorcentajeNuevo}%</color>\n";
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Dano:</b></color> <color={colorValorNuevo}>{rangoDanioNuevo} + {atributoNuevo}. Tipo: Perforante</color>\n";
            if (penetracionArmadura > 0) { cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Penetracion de armadura:</b></color> <color={colorValorNuevo}>{penetracionArmadura}</color>\n"; }
            cuerpoNuevo += $"<color={colorEncabezadoNuevo}><b>Efecto del arma:</b></color> <color={colorValorNuevo}>{efectoNuevo}</color>";
        }

        txtDescripcion = ConstruirDescripcionTooltipNueva(
            nombre,
            esIngles ? "Item thrust with a weapon-specific effect." : esPortugues ? "Estocada de item com efeito especifico da arma." : "Estocada de item con efecto propio del arma.",
            cuerpoNuevo);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
        if (obj is Unidad objetivo)
        {
            bool objetivoIntacto = objetivo.mod_maxHP > 0f && Mathf.Approximately(objetivo.HP_actual, objetivo.mod_maxHP);
            int bonusAtaqueTotal = bonusAtaque + (objetivoIntacto ? BonusAtaquePrimerSangre : 0);
            int bonusDanioPlano = objetivoIntacto ? BonusDanioPlanoPrimerSangre : 0;
            int bonusCritico = objetivoIntacto ? BonusCriticoPrimerSangre : 0;
            EjecutarAtaquePrimerSangre(objetivo, tirada, bonusAtaqueTotal, bonusDanioPlano, bonusCritico, true);
            return;
        }

        if (obj is Obstaculo obstaculo)
        {
            int atributoMixto = Mathf.CeilToInt((scEstaUnidad.mod_CarFuerza + scEstaUnidad.mod_CarAgilidad) / 2f);
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + atributoMixto;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);
            obstaculo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
        }
    }

    private void EjecutarAtaquePrimerSangre(Unidad objetivo, int tirada, int bonusAtaqueTotal, int bonusDanioPlano, int bonusCritico, bool gastarAPPorPifia)
    {
        if (objetivo == null)
        {
            return;
        }

        ClaseDuelista duelista = scEstaUnidad as ClaseDuelista;
        int bonusDanioPorcentajeDanza = duelista != null ? duelista.ObtenerBonusDanioPorcentajeDanzaDelEstoque(objetivo) : 0;
        int atributoMixto = Mathf.CeilToInt((scEstaUnidad.mod_CarFuerza + scEstaUnidad.mod_CarAgilidad) / 2f);
        float defensaObjetivo = objetivo.ObtenerdefensaActual();
        float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab + bonusCritico;
        int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, atributoMixto, bonusAtaqueTotal, criticoRango, objetivo, 0);

        if (resultadoTirada == -1)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
            if (gastarAPPorPifia)
            {
                scEstaUnidad.EstablecerAPActualA(0);
            }
        }
        else if (resultadoTirada == 0)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        }
        else
        {
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + atributoMixto + bonusDanioPlano;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + bonusDanioPorcentajeDanza);
            if (resultadoTirada == 1)
            {
                danio -= danio / 2;
            }

            objetivo.RecibirDanio(danio, tipoDanio, resultadoTirada == 3, scEstaUnidad);
            VFXAplicar(objetivo.gameObject);
        }

        objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }

    private void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Estocada");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;

        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }
}
