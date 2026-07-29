using System.Collections.Generic;
using System.Reflection;

public static class BuffUIHelper
{
    private static readonly FieldInfo[] AggregateFields =
        typeof(Buff).GetFields(BindingFlags.Public | BindingFlags.Instance);

    public struct BuffStack
    {
        public Buff AggregatedBuff;
        public Buff SourceBuff;
        public int StackCount;
    }

    public static List<BuffStack> GetVisibleBuffStacks(Unidad unidad)
    {
        return GetVisibleBuffStacks(unidad, false);
    }

    public static List<BuffStack> GetVisibleBuffStacks(Unidad unidad, bool paraBarraVida)
    {
        var result = new List<BuffStack>();
        if (unidad == null) { return result; }

        Buff[] buffs = unidad.gameObject.GetComponents<Buff>();
        var grouped = new Dictionary<string, List<Buff>>();
        var order = new List<string>();

        foreach (Buff buff in buffs)
        {
            if (buff == null) { continue; }
            if (buff.DuracionBuffRondas == 0) { continue; }
            if (!buff.esBuffVisibleUI) { continue; }
            if (paraBarraVida && buff.ocultarEnBarraVida) { continue; }

            string key = string.IsNullOrWhiteSpace(buff.buffNombre)
                ? "__buff_sin_nombre_" + buff.GetInstanceID()
                : buff.buffNombre;
            if (!grouped.TryGetValue(key, out var list))
            {
                list = new List<Buff>();
                grouped.Add(key, list);
                order.Add(key);
            }

            list.Add(buff);
        }

        foreach (string key in order)
        {
            List<Buff> stack = grouped[key];
            if (stack.Count == 0) { continue; }

            Buff aggregated = AggregateBuff(stack);
            result.Add(new BuffStack
            {
                AggregatedBuff = aggregated,
                SourceBuff = stack[0],
                StackCount = stack.Count
            });
        }

        return result;
    }

    public static int CalculateVisibleBuffSignature(
        Unidad unidad,
        bool paraBarraVida,
        bool soloRemovibles,
        List<Buff> componentBuffer)
    {
        if (componentBuffer == null)
        {
            return 0;
        }

        componentBuffer.Clear();
        if (unidad == null)
        {
            return 0;
        }

        unidad.GetComponents(componentBuffer);

        unchecked
        {
            int hash = 17;
            for (int i = 0; i < componentBuffer.Count; i++)
            {
                Buff baseBuff = componentBuffer[i];
                if (!IsVisible(baseBuff, paraBarraVida) || YaFueAgrupado(componentBuffer, i, paraBarraVida))
                {
                    continue;
                }

                if (soloRemovibles && !baseBuff.esRemovible)
                {
                    continue;
                }

                int stackCount = 0;
                int stackDuration = 0;
                for (int j = i; j < componentBuffer.Count; j++)
                {
                    Buff candidate = componentBuffer[j];
                    if (!IsVisible(candidate, paraBarraVida) || !SameStackKey(baseBuff, candidate))
                    {
                        continue;
                    }

                    stackCount++;
                    if (candidate.DuracionBuffRondas < 0)
                    {
                        stackDuration = -1;
                    }
                    else if (stackDuration >= 0 && candidate.DuracionBuffRondas > stackDuration)
                    {
                        stackDuration = candidate.DuracionBuffRondas;
                    }
                }

                hash = MixHash(hash, baseBuff.GetInstanceID());
                hash = MixHash(hash, HashText(baseBuff.buffNombre));
                hash = MixHash(hash, HashText(baseBuff.buffDescr));
                hash = MixHash(hash, baseBuff.boolfDebufftBuff ? 1 : 0);
                hash = MixHash(hash, stackDuration);
                hash = MixHash(hash, stackCount);
            }

            return hash;
        }
    }

    private static Buff AggregateBuff(List<Buff> buffs)
    {
        Buff aggregated = new Buff();
        if (buffs == null || buffs.Count == 0)
        {
            return aggregated;
        }

        Buff baseBuff = buffs[0];
        foreach (FieldInfo field in AggregateFields)
        {
            if (field.FieldType == typeof(float))
            {
                float sum = 0f;
                foreach (Buff buff in buffs)
                {
                    sum += (float)field.GetValue(buff);
                }
                field.SetValue(aggregated, sum);
            }
            else if (field.FieldType == typeof(int))
            {
                if (field.Name == nameof(Buff.DuracionBuffRondas))
                {
                    field.SetValue(aggregated, CalculateStackDuration(buffs));
                }
                else if (field.Name == nameof(Buff.CustomEffectInicioTurnoID))
                {
                    field.SetValue(aggregated, baseBuff.CustomEffectInicioTurnoID);
                }
                else
                {
                    int sum = 0;
                    foreach (Buff buff in buffs)
                    {
                        sum += (int)field.GetValue(buff);
                    }
                    field.SetValue(aggregated, sum);
                }
            }
            else
            {
                field.SetValue(aggregated, field.GetValue(baseBuff));
            }
        }

        aggregated.goVFX = null;
        aggregated.unidadOrigen = baseBuff.unidadOrigen;
        aggregated.esBuffVisibleUI = baseBuff.esBuffVisibleUI;
        aggregated.esRemovible = baseBuff.esRemovible;
        aggregated.esStackeable = baseBuff.esStackeable;
        aggregated.suprimeTextoFlotante = baseBuff.suprimeTextoFlotante;
        aggregated.buffNombre = string.IsNullOrWhiteSpace(baseBuff.buffNombre) ? "Estado" : baseBuff.buffNombre;
        aggregated.buffDescr = baseBuff.buffDescr ?? string.Empty;
        aggregated.boolfDebufftBuff = baseBuff.boolfDebufftBuff;

        return aggregated;
    }

    private static int CalculateStackDuration(List<Buff> buffs)
    {
        bool hasPermanent = false;
        int maxDuration = 0;

        foreach (Buff buff in buffs)
        {
            if (buff.DuracionBuffRondas < 0)
            {
                hasPermanent = true;
                break;
            }

            if (buff.DuracionBuffRondas > maxDuration)
            {
                maxDuration = buff.DuracionBuffRondas;
            }
        }

        return hasPermanent ? -1 : maxDuration;
    }

    private static bool IsVisible(Buff buff, bool paraBarraVida)
    {
        return buff != null
            && buff.DuracionBuffRondas != 0
            && buff.esBuffVisibleUI
            && (!paraBarraVida || !buff.ocultarEnBarraVida);
    }

    private static bool YaFueAgrupado(List<Buff> buffs, int currentIndex, bool paraBarraVida)
    {
        Buff current = buffs[currentIndex];
        for (int i = 0; i < currentIndex; i++)
        {
            Buff previous = buffs[i];
            if (IsVisible(previous, paraBarraVida) && SameStackKey(previous, current))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SameStackKey(Buff left, Buff right)
    {
        bool leftUnnamed = string.IsNullOrWhiteSpace(left.buffNombre);
        bool rightUnnamed = string.IsNullOrWhiteSpace(right.buffNombre);
        if (leftUnnamed || rightUnnamed)
        {
            return leftUnnamed && rightUnnamed && ReferenceEquals(left, right);
        }

        return string.Equals(left.buffNombre, right.buffNombre, System.StringComparison.Ordinal);
    }

    private static int MixHash(int current, int value)
    {
        unchecked
        {
            return (current * 31) + value;
        }
    }

    private static int HashText(string text)
    {
        return string.IsNullOrEmpty(text) ? 0 : text.GetHashCode();
    }
}
