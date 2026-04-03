using UnityEngine;

public class EstocadaAstral : Estocada
{
    private const int DuracionMarcaAstral = 2;
    private const int DificultadMarcaAstral = 12;

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
        string lineaSalvacion = ConstruirLineaSalvacion(esIngles, TipoSalvacionDescripcion.Mental, DificultadMarcaAstral);

        string tituloEs = "Estocada Astral";
        string tituloEn = "Astral Thrust";
        string tituloPt = "Estocada Astral";
        nombre = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Melee\n";
            cuerpo += "<b>Target:</b> 1 enemy in front range\n";
            cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> + Attack ({ataqueActual}) vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Damage:</b> 1d8 + <color=#ea0606>Str/Agi ({atributoMixtoActual})</color> | <b>Type:</b> Piercing\n";
            cuerpo += $"<b>Armor Penetration:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Weapon effect:</b> on hit, applies Astral Mark for {DuracionMarcaAstral} turns if the target fails a Mental Save\n";
            cuerpo += $"{lineaSalvacion}\n";
            cuerpo += "<b>On failed save:</b> -1 Defense, -2 Mental Save, -10 Arcane Resistance";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Alvo:</b> 1 inimigo em alcance frontal\n";
            cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) vs Defesa. Falha critica: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Dano:</b> 1d8 + <color=#ea0606>Forca/Agilidade ({atributoMixtoActual})</color> | <b>Tipo:</b> Perfurante\n";
            cuerpo += $"<b>Penetracao de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Efeito da arma:</b> ao acertar, aplica Marca Astral por {DuracionMarcaAstral} turnos se o alvo falhar em uma resistencia Mental\n";
            cuerpo += $"{lineaSalvacion}\n";
            cuerpo += "<b>Se falhar na resistencia:</b> -1 Defesa, -2 Resistencia Mental, -10 Resistencia Arcana";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Melee\n";
            cuerpo += "<b>Objetivo:</b> 1 enemigo en alcance frontal\n";
            cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> + Ataque ({ataqueActual}) vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Danio:</b> 1d8 + <color=#ea0606>Fuerza/Agilidad ({atributoMixtoActual})</color> | <b>Tipo:</b> Perforante\n";
            cuerpo += $"<b>Penetracion de armadura:</b> {penetracionArmadura}\n";
            cuerpo += $"<b>Efecto del arma:</b> al impactar, aplica Marca Astral por {DuracionMarcaAstral} turnos si el objetivo falla una TS Mental\n";
            cuerpo += $"{lineaSalvacion}\n";
            cuerpo += "<b>Si falla TS:</b> -1 Defensa, -2 TS Mental, -10 Res Arcano";
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
            : esPortugues
                ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
                : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentia: {costoPM}\n- Esforzable: Si ({esforzable})";

        txtDescripcion = ConstruirDescripcionEstandar(
            nombre,
            esIngles
                ? "A precise thrust that punctures armor and leaves an astral seal on the target."
                : esPortugues
                    ? "Uma estocada precisa que perfura armadura e deixa um selo astral no alvo."
                    : "Una estocada precisa que perfora armadura y deja un sello astral en el objetivo.",
            cuerpo,
            costos,
            "#5dade2");
    }
}
