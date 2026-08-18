using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONExcesoDePoder : Habilidad
{
    public override void Awake()
    {
        imHab = Resources.Load<Sprite>("imHab/Canalizador_ExcesoDePoder");
        ActualizarDescripcion();
        IDenClase = 8;
    }

    public override void ActualizarDescripcion()
    {
        bool esInglesFormato = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortuguesFormato = TRADU.i != null && TRADU.i.nIdioma == 3;
        string colorEncabezado = "#44d3ec";
        string colorValor = "#ffffff";
        string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
        int residuos = NIVEL == 4 ? 2 : 1;
        int criticoPorcentaje = NIVEL == 5 ? 10 : 5;
        string danioPropio = NIVEL < 2 ? "1-4" : NIVEL == 2 ? "0-3" : "0-2";

        if (esInglesFormato)
        {
            string tituloIngles = "Excess of Power I";
            if (NIVEL == 2) { tituloIngles = "Excess of Power II"; }
            else if (NIVEL == 3) { tituloIngles = "Excess of Power III"; }
            else if (NIVEL == 4) { tituloIngles = "Excess of Power IV a"; }
            else if (NIVEL == 5) { tituloIngles = "Excess of Power IV b"; }

            int duracionResiduo = NIVEL == 4 ? 3 : 2;
            int bonusDanioArcano = NIVEL > 1 ? 4 : 3;
            int apRestaurado = NIVEL > 2 ? 2 : 1;
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
            string residuo = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, "Energy Residue", "Estado_acumularenergia");
            string residuoPlural = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, "Energy Residues");
            string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Arcane damage", "dano_arcano");
            string danioArcanoSinIcono = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Arcane damage");
            string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");

            var lineas = new List<LineaDescripcionNormalizada>
            {
                LineaDescripcion("Passive bonus", $"+{criticoPorcentaje}% {critico}."),
                LineaDescripcion("Trigger", "Deals a critical hit."),
                LineaDescripcion("Effect", $"Creates {residuos} {((residuos == 1) ? residuo : residuoPlural)} in a random nearby empty tile."),
                LineaDescripcion("Backlash", $"Suffers {danioPropio} {danioArcano}."),
                LineaDescripcion("Residue", $"Lasts {duracionResiduo} turns."),
                LineaDescripcion("On contact", $"Gains +1 Attack and +{bonusDanioArcano} {danioArcanoSinIcono} ({duracionResiduo} turns); restores {apRestaurado} {ap}.", 1),
                LineaDescripcion("Channeler", "Also restores 1-8 HP.", 2),
                LineaDescripcion("Other units", $"Also suffer 1-8 {danioArcanoSinIcono}.", 2)
            };

            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) { proximaMejora = "-1 maximum backlash damage."; }
                else if (NIVEL == 2) { proximaMejora = "-1 maximum backlash damage."; }
                else if (NIVEL == 3) { proximaMejora = "Option A: +1 Energy Residue per critical hit.\nOption B: +5% Crit."; }
            }

            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                tituloIngles,
                "Passive: Critical hits create volatile Energy Residues and cause Arcane backlash.",
                lineas,
                proximaMejora);
            return;
        }

        {
            bool pt = esPortuguesFormato;
            string titulo = pt ? "Excesso de Poder I" : "Exceso de Poder I";
            if (NIVEL == 2) { titulo = pt ? "Excesso de Poder II" : "Exceso de Poder II"; }
            else if (NIVEL == 3) { titulo = pt ? "Excesso de Poder III" : "Exceso de Poder III"; }
            else if (NIVEL == 4) { titulo = pt ? "Excesso de Poder IV a" : "Exceso de Poder IV a"; }
            else if (NIVEL == 5) { titulo = pt ? "Excesso de Poder IV b" : "Exceso de Poder IV b"; }
            int duracionResiduo = NIVEL == 4 ? 3 : 2;
            int bonusDanioArcano = NIVEL > 1 ? 4 : 3;
            int apRestaurado = NIVEL > 2 ? 2 : 1;
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crítico", "critico");
            string residuo = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, pt ? "Resíduo Energético" : "Residuo Energético", "Estado_acumularenergia");
            string residuoPlural = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, pt ? "Resíduos Energéticos" : "Residuos Energéticos");
            string danioArcano = TerminoDescripcion(TerminoDescripcionId.DanioArcano, pt ? "dano Arcano" : "daño Arcano", "dano_arcano");
            string danioArcanoSinIcono = TerminoDescripcion(TerminoDescripcionId.DanioArcano, pt ? "dano Arcano" : "daño Arcano");
            string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
            var lineas = new List<LineaDescripcionNormalizada>
            {
                LineaDescripcion(pt ? "Bônus passivo" : "Bonificación pasiva", $"+{criticoPorcentaje}% {critico}."),
                LineaDescripcion(pt ? "Gatilho" : "Desencadenante", pt ? "Causa um acerto crítico." : "Inflige un golpe crítico."),
                LineaDescripcion(pt ? "Efeito" : "Efecto", $"{(pt ? "Cria" : "Crea")} {residuos} {(residuos == 1 ? residuo : residuoPlural)} {(pt ? "em uma casa vazia próxima aleatória" : "en una casilla vacía cercana aleatoria")}."),
                LineaDescripcion(pt ? "Retorno" : "Retroceso", $"{(pt ? "Sofre" : "Sufre")} {danioPropio} {danioArcano}."),
                LineaDescripcion(pt ? "Resíduo" : "Residuo", $"{(pt ? "Dura" : "Dura")} {duracionResiduo} turnos."),
                LineaDescripcion(pt ? "Ao entrar em contato" : "Al entrar en contacto", $"{(pt ? "Recebe" : "Obtiene")} +1 Ataque {(pt ? "e" : "y")} +{bonusDanioArcano} {danioArcanoSinIcono} ({duracionResiduo} turnos); {(pt ? "recupera" : "recupera")} {apRestaurado} {ap}.", 1),
                LineaDescripcion("Canalizador", pt ? "Também recupera 1-8 PV." : "También recupera 1-8 PV.", 2),
                LineaDescripcion(pt ? "Outras unidades" : "Otras unidades", $"{(pt ? "Também sofrem" : "También sufren")} 1-8 {danioArcanoSinIcono}.", 2)
            };
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2 || NIVEL == 2) { proximaMejora = pt ? "-1 ao dano máximo de retorno." : "-1 al daño máximo de retroceso."; }
                else if (NIVEL == 3) { proximaMejora = pt ? "Opção A: +1 Resíduo Energético por acerto crítico.\nOpção B: +5% Crítico." : "Opción A: +1 Residuo Energético por golpe crítico.\nOpción B: +5% Crítico."; }
            }
            txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
                titulo,
                pt ? "Passiva: os acertos críticos criam Resíduos Energéticos voláteis e causam retorno Arcano." : "Pasiva: los golpes críticos crean Residuos Energéticos volátiles y causan retroceso Arcano.",
                lineas,
                proximaMejora,
                costoSuperior: string.Empty);
            return;
        }

        string tituloFormato = esInglesFormato ? "Excess of Power" : esPortuguesFormato ? "Excesso de Poder" : "Exceso de Poder";
        if (NIVEL < 2) { tituloFormato += " I"; }
        else if (NIVEL == 2) { tituloFormato += " II"; }
        else if (NIVEL == 3) { tituloFormato += " III"; }
        else if (NIVEL == 4) { tituloFormato += " IV a"; }
        else if (NIVEL == 5) { tituloFormato += " IV b"; }

        string subtituloFormato = esInglesFormato
            ? "Critical hits create Energy Residues and deal Arcane backlash."
            : esPortuguesFormato
                ? "Criticos criam Residuos Energeticos e causam retorno Arcano."
                : "Los criticos crean Residuos Energeticos y causan retorno Arcano.";

        string cuerpoFormato;
        if (esInglesFormato)
        {
            cuerpoFormato =
                $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Passive buff</color>\n" +
                $"<color={colorEncabezado}><b>Passive bonus:</b></color> <color={colorValor}>+{criticoPorcentaje}% Critical</color>\n" +
                $"<color={colorEncabezado}><b>On critical:</b></color> <color={colorValor}>{iconoEnergia} creates {residuos} Energy Residue{(residuos > 1 ? "s" : "")}</color>\n" +
                $"<color={colorEncabezado}><b>Backlash:</b></color> <color={colorValor}>Takes {danioPropio} Arcane damage</color>";
        }
        else if (esPortuguesFormato)
        {
            cuerpoFormato =
                $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Buff passivo</color>\n" +
                $"<color={colorEncabezado}><b>Bonus passivo:</b></color> <color={colorValor}>+{criticoPorcentaje}% Crítico</color>\n" +
                $"<color={colorEncabezado}><b>Ao critar:</b></color> <color={colorValor}>{iconoEnergia} cria {residuos} Residuo{(residuos > 1 ? "s" : "")} Energetico{(residuos > 1 ? "s" : "")}</color>\n" +
                $"<color={colorEncabezado}><b>Retorno:</b></color> <color={colorValor}>Recebe {danioPropio} dano Arcano</color>";
        }
        else
        {
            cuerpoFormato =
                $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Buff pasivo</color>\n" +
                $"<color={colorEncabezado}><b>Bonus pasivo:</b></color> <color={colorValor}>+{criticoPorcentaje}% Crítico</color>\n" +
                $"<color={colorEncabezado}><b>Al crítico:</b></color> <color={colorValor}>{iconoEnergia} crea {residuos} Residuo{(residuos > 1 ? "s" : "")} Energetico{(residuos > 1 ? "s" : "")}</color>\n" +
                $"<color={colorEncabezado}><b>Retorno:</b></color> <color={colorValor}>Recibe {danioPropio} daño Arcano</color>";
        }

        txtDescripcion =
            $"<size=115%><color=#5dade2><b>{tituloFormato}</b></color></size>\n\n" +
            $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
            "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
            cuerpoFormato +
            ObtenerBloqueProximoNivelSiCorresponde(esInglesFormato, esPortuguesFormato);
    }

    string ObtenerBloqueProximoNivelSiCorresponde(bool esInglesFormato, bool esPortuguesFormato)
    {
        if (!EsEscenaCampaña()
            || CampaignManager.Instance == null
            || CampaignManager.Instance.scMenuPersonajes == null
            || CampaignManager.Instance.scMenuPersonajes.pSel == null
            || CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad <= 0)
        {
            return string.Empty;
        }

        string texto = ObtenerTextoProximoNivel(esInglesFormato, esPortuguesFormato);
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        return "\n\n" + texto;
    }

    string ObtenerTextoProximoNivel(bool esInglesFormato, bool esPortuguesFormato)
    {
        if (NIVEL < 2)
        {
            if (esInglesFormato)
            {
                return "<color=#dfea02><b>Next Level:</b></color> <color=#ffffff>-1 backlash damage on critical</color>";
            }

            if (esPortuguesFormato)
            {
                return "<color=#dfea02><b>Proximo Nivel:</b></color> <color=#ffffff>-1 dano de retorno no critico</color>";
            }

            return "<color=#dfea02><b>Próximo Nivel:</b></color> <color=#ffffff>-1 daño de retorno al crítico</color>";
        }

        if (NIVEL == 2)
        {
            if (esInglesFormato)
            {
                return "<color=#dfea02><b>Next Level:</b></color> <color=#ffffff>-1 backlash damage on critical (cumulative)</color>";
            }

            if (esPortuguesFormato)
            {
                return "<color=#dfea02><b>Proximo Nivel:</b></color> <color=#ffffff>-1 dano de retorno no critico (acumulativo)</color>";
            }

            return "<color=#dfea02><b>Próximo Nivel:</b></color> <color=#ffffff>-1 daño de retorno al crítico (acumulativo)</color>";
        }

        if (NIVEL == 3)
        {
            if (esInglesFormato)
            {
                return "<color=#dfea02><b>Next Level A:</b></color> <color=#ffffff>+1 Energy Residue per critical</color>\n"
                    + "<color=#dfea02><b>Next Level B:</b></color> <color=#ffffff>+5% additional Critical</color>";
            }

            if (esPortuguesFormato)
            {
                return "<color=#dfea02><b>Próximo Nivel A:</b></color> <color=#ffffff>+1 Residuo Energetico por crítico</color>\n"
                    + "<color=#dfea02><b>Próximo Nivel B:</b></color> <color=#ffffff>+5% Crítico adicional</color>";
            }

            return "<color=#dfea02><b>Próximo Nivel A:</b></color> <color=#ffffff>+1 Residuo Energético por crítico</color>\n"
                + "<color=#dfea02><b>Próximo Nivel B:</b></color> <color=#ffffff>+5% Crítico adicional</color>";
        }

        return string.Empty;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar()
    {
    }
}
