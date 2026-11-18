using System.Collections.Generic;

/// <summary>Frases completas para los resultados de combate; no calcula ni aplica recompensas.</summary>
public static class ChineseBattleText
{
    public static string Victory(int experience, int gold, int materials, int hope)
    {
        var rewards = new List<string>();
        if (experience > 0) rewards.Add(string.Format(ChineseLocalization.Get("{0} Experiencia", "{0} Experience"), experience));
        if (gold > 0) rewards.Add(string.Format(ChineseLocalization.Get("{0} Oro", "{0} Gold"), gold));
        if (materials > 0) rewards.Add(string.Format(ChineseLocalization.Get("{0} Materiales", "{0} Materials"), materials));

        if (rewards.Count == 0 && hope > 0)
        {
            return string.Format(ChineseLocalization.Get("Victoria: +{0} Esperanza.", "Victory: +{0} Hope."), hope);
        }
        string result = rewards.Count > 0
            ? string.Format(ChineseLocalization.Get("Victoria: se han obtenido {0}.", "Victory: obtained {0}."), string.Join("、", rewards))
            : ChineseLocalization.Get("Victoria sin recompensas.", "Victory without rewards.");
        if (hope > 0)
        {
            result += string.Format(ChineseLocalization.Get(" +{0} Esperanza.", " +{0} Hope."), hope);
        }
        return result;
    }

    public static string Defeat(int hope, int civilians, int oxen)
    {
        var losses = new List<string>();
        if (civilians > 0) losses.Add(string.Format(ChineseLocalization.Get("-{0} Civiles", "-{0} Civilians"), civilians));
        if (hope > 0) losses.Add(string.Format(ChineseLocalization.Get("-{0} Esperanza", "-{0} Hope"), hope));
        if (oxen > 0) losses.Add(string.Format(ChineseLocalization.Get("-{0} Bueyes", "-{0} Oxen"), oxen));
        return losses.Count > 0
            ? string.Format(ChineseLocalization.Get("Derrota: pérdidas de {0}.", "Defeat: losses of {0}."), string.Join("、", losses))
            : ChineseLocalization.Get("Derrota sin consecuencias graves.", "Defeat without major consequences.");
    }
}
