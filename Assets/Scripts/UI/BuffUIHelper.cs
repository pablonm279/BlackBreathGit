using System.Collections.Generic;
using System.Reflection;

public static class BuffUIHelper
{
    public struct BuffStack
    {
        public Buff AggregatedBuff;
        public int StackCount;
    }

    public static List<BuffStack> GetVisibleBuffStacks(Unidad unidad)
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
                StackCount = stack.Count
            });
        }

        return result;
    }

    private static Buff AggregateBuff(List<Buff> buffs)
    {
        Buff aggregated = new Buff();
        if (buffs == null || buffs.Count == 0)
        {
            return aggregated;
        }

        Buff baseBuff = buffs[0];
        FieldInfo[] fields = typeof(Buff).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
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
}
