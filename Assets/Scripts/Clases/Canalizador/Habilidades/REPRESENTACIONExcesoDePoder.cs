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
                $"<color={colorEncabezado}><b>Bonus passivo:</b></color> <color={colorValor}>+{criticoPorcentaje}% Critico</color>\n" +
                $"<color={colorEncabezado}><b>Ao critar:</b></color> <color={colorValor}>{iconoEnergia} cria {residuos} Residuo{(residuos > 1 ? "s" : "")} Energetico{(residuos > 1 ? "s" : "")}</color>\n" +
                $"<color={colorEncabezado}><b>Retorno:</b></color> <color={colorValor}>Recebe {danioPropio} dano Arcano</color>";
        }
        else
        {
            cuerpoFormato =
                $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Buff pasivo</color>\n" +
                $"<color={colorEncabezado}><b>Bonus pasivo:</b></color> <color={colorValor}>+{criticoPorcentaje}% Critico</color>\n" +
                $"<color={colorEncabezado}><b>Al critico:</b></color> <color={colorValor}>{iconoEnergia} crea {residuos} Residuo{(residuos > 1 ? "s" : "")} Energetico{(residuos > 1 ? "s" : "")}</color>\n" +
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
                return "<color=#dfea02><b>Proximo Nivel A:</b></color> <color=#ffffff>+1 Residuo Energetico por critico</color>\n"
                    + "<color=#dfea02><b>Proximo Nivel B:</b></color> <color=#ffffff>+5% Critico adicional</color>";
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
