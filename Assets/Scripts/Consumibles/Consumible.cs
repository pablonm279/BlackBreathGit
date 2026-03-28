using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConsumibleBuffData
{
    public int cantAtFue;
    public int cantAtAgi;
    public int cantAtPod;
    public int cantIniciativa;
    public int cantAPMax;
    public int cantPMMax;
    public int cantHPMax;
    public int cantArmadura;
    public int cantDefensa;
    public int cantAtaque;
    public int cantDanioPorcentaje;
    public int cantCritDado;
    public int cantCritDanio;
    public int cantTsReflejos;
    public int cantTsFortaleza;
    public int cantTsMental;
    public int cantResFue;
    public int cantResHie;
    public int cantResRay;
    public int cantResAci;
    public int cantResArc;
    public int cantResNec;
    public int cantResDiv;
    public int cantBarrera;
    public int cantDamBonusElementalFue;
    public int cantDamBonusElementalHie;
    public int cantDamBonusElementalRay;
    public int cantDamBonusElementalAci;
    public int cantDamBonusElementalArc;
    public int cantDamBonusElementalNec;
    public int cantDamBonusElementalDiv;
    public int cantPenetracionArmadura;
    public int cantReduccionDanioRecibidoPorcentaje;
    public int cantReduccionDanioCriticoRecibidoPorcentaje;
    public int cantResistenciaEstadosPorcentaje;
    public int cantEspinasDanioPlano;
    public int cantEspinasDanioPorcentaje;

    public bool TieneCambios()
    {
        return cantAtFue != 0
            || cantAtAgi != 0
            || cantAtPod != 0
            || cantIniciativa != 0
            || cantAPMax != 0
            || cantPMMax != 0
            || cantHPMax != 0
            || cantArmadura != 0
            || cantDefensa != 0
            || cantAtaque != 0
            || cantDanioPorcentaje != 0
            || cantCritDado != 0
            || cantCritDanio != 0
            || cantTsReflejos != 0
            || cantTsFortaleza != 0
            || cantTsMental != 0
            || cantResFue != 0
            || cantResHie != 0
            || cantResRay != 0
            || cantResAci != 0
            || cantResArc != 0
            || cantResNec != 0
            || cantResDiv != 0
            || cantBarrera != 0
            || cantDamBonusElementalFue != 0
            || cantDamBonusElementalHie != 0
            || cantDamBonusElementalRay != 0
            || cantDamBonusElementalAci != 0
            || cantDamBonusElementalArc != 0
            || cantDamBonusElementalNec != 0
            || cantDamBonusElementalDiv != 0
            || cantPenetracionArmadura != 0
            || cantReduccionDanioRecibidoPorcentaje != 0
            || cantReduccionDanioCriticoRecibidoPorcentaje != 0
            || cantResistenciaEstadosPorcentaje != 0
            || cantEspinasDanioPlano != 0
            || cantEspinasDanioPorcentaje != 0;
    }

    public ConsumibleBuffData Clone()
    {
        return new ConsumibleBuffData
        {
            cantAtFue = cantAtFue,
            cantAtAgi = cantAtAgi,
            cantAtPod = cantAtPod,
            cantIniciativa = cantIniciativa,
            cantAPMax = cantAPMax,
            cantPMMax = cantPMMax,
            cantHPMax = cantHPMax,
            cantArmadura = cantArmadura,
            cantDefensa = cantDefensa,
            cantAtaque = cantAtaque,
            cantDanioPorcentaje = cantDanioPorcentaje,
            cantCritDado = cantCritDado,
            cantCritDanio = cantCritDanio,
            cantTsReflejos = cantTsReflejos,
            cantTsFortaleza = cantTsFortaleza,
            cantTsMental = cantTsMental,
            cantResFue = cantResFue,
            cantResHie = cantResHie,
            cantResRay = cantResRay,
            cantResAci = cantResAci,
            cantResArc = cantResArc,
            cantResNec = cantResNec,
            cantResDiv = cantResDiv,
            cantBarrera = cantBarrera,
            cantDamBonusElementalFue = cantDamBonusElementalFue,
            cantDamBonusElementalHie = cantDamBonusElementalHie,
            cantDamBonusElementalRay = cantDamBonusElementalRay,
            cantDamBonusElementalAci = cantDamBonusElementalAci,
            cantDamBonusElementalArc = cantDamBonusElementalArc,
            cantDamBonusElementalNec = cantDamBonusElementalNec,
            cantDamBonusElementalDiv = cantDamBonusElementalDiv,
            cantPenetracionArmadura = cantPenetracionArmadura,
            cantReduccionDanioRecibidoPorcentaje = cantReduccionDanioRecibidoPorcentaje,
            cantReduccionDanioCriticoRecibidoPorcentaje = cantReduccionDanioCriticoRecibidoPorcentaje,
            cantResistenciaEstadosPorcentaje = cantResistenciaEstadosPorcentaje,
            cantEspinasDanioPlano = cantEspinasDanioPlano,
            cantEspinasDanioPorcentaje = cantEspinasDanioPorcentaje
        };
    }
}

[Serializable]
public class ConsumibleEfectoData
{
    [Header("Curacion")]
    [Min(0)] public int curacionBase;
    [Min(0)] public int curacionDadosCantidad;
    [Min(0)] public int curacionDadosCaras;
    [Range(0, 100)] public int curacionPorcentajeHPMax;

    [Header("Limpieza")]
    public bool removerDebuffs;
    public bool removerBuffs;
    public bool removerEstadosNegativos;

    [Header("Estados directos")]
    public int modificarRegeneracionVida;
    public int modificarRegeneracionArmadura;
    public int modificarEvasion;

    [Header("Buff aplicado")]
    public bool aplicarBuff = true;
    public string nombreBuff = "Efecto de consumible";
    public bool buffEsBeneficio = true;
    [Tooltip("-1 = permanente durante combate")]
    public int duracionBuffRondas = -1;
    [Tooltip("Buff base opcional. Si se asigna, se copia y aplica al usar el consumible.")]
    public Buff buffReferencia;
    public ConsumibleBuffData buff = new ConsumibleBuffData();

    public bool TieneEfecto()
    {
        bool tieneCuracion = curacionBase > 0 || curacionDadosCantidad > 0 || curacionPorcentajeHPMax > 0;
        bool tieneLimpieza = removerDebuffs || removerBuffs || removerEstadosNegativos;
        bool tieneEstadosDirectos = modificarRegeneracionVida != 0 || modificarRegeneracionArmadura != 0 || modificarEvasion != 0;
        bool tieneBuff = aplicarBuff
            && ((buff != null && buff.TieneCambios()) || buffReferencia != null);
        return tieneCuracion || tieneLimpieza || tieneEstadosDirectos || tieneBuff;
    }

    public ConsumibleEfectoData Clone()
    {
        return new ConsumibleEfectoData
        {
            curacionBase = curacionBase,
            curacionDadosCantidad = curacionDadosCantidad,
            curacionDadosCaras = curacionDadosCaras,
            curacionPorcentajeHPMax = curacionPorcentajeHPMax,
            removerDebuffs = removerDebuffs,
            removerBuffs = removerBuffs,
            removerEstadosNegativos = removerEstadosNegativos,
            modificarRegeneracionVida = modificarRegeneracionVida,
            modificarRegeneracionArmadura = modificarRegeneracionArmadura,
            modificarEvasion = modificarEvasion,
            aplicarBuff = aplicarBuff,
            nombreBuff = nombreBuff,
            buffEsBeneficio = buffEsBeneficio,
            duracionBuffRondas = duracionBuffRondas,
            buffReferencia = buffReferencia,
            buff = buff != null ? buff.Clone() : new ConsumibleBuffData()
        };
    }
}

public class Consumible : Item
{
    [Header("Efecto al usar")]
    public ConsumibleEfectoData efectoConsumible = new ConsumibleEfectoData();
    private bool ejecutandoFallbackLegacy;

    public virtual void UsarConsumible(Unidad unidad)
    {
        UsarConsumibleDesdeDatos(unidad);
    }

    public void UsarConsumibleDesdeDatos(Unidad unidad)
    {
        if (ejecutandoFallbackLegacy)
        {
            return;
        }

        if (unidad == null)
        {
            return;
        }

        ConsumibleEfectoData data = ObtenerEfectoConsumibleNormalizado();
        if (!data.TieneEfecto())
        {
            // Fallback para consumibles custom que aun usan logica override.
            var metodo = GetType().GetMethod(nameof(UsarConsumible));
            if (metodo != null && metodo.DeclaringType != typeof(Consumible))
            {
                ejecutandoFallbackLegacy = true;
                try
                {
                    UsarConsumible(unidad);
                }
                finally
                {
                    ejecutandoFallbackLegacy = false;
                }
            }
            return;
        }

        bool aplicoAlgo = false;

        int curacionTotal = data.curacionBase;
        if (data.curacionDadosCantidad > 0 && data.curacionDadosCaras > 0)
        {
            curacionTotal += TiradaDeDados.TirarDados(data.curacionDadosCantidad, data.curacionDadosCaras);
        }

        if (data.curacionPorcentajeHPMax > 0)
        {
            curacionTotal += Mathf.RoundToInt(unidad.mod_maxHP * (data.curacionPorcentajeHPMax / 100f));
        }

        if (curacionTotal > 0)
        {
            unidad.RecibirCuracion(curacionTotal, true);
            aplicoAlgo = true;
        }

        if (data.removerDebuffs)
        {
            unidad.RemoverfDebuffstBuffs(false);
            aplicoAlgo = true;
        }

        if (data.removerBuffs)
        {
            unidad.RemoverfDebuffstBuffs(true);
            aplicoAlgo = true;
        }

        if (data.removerEstadosNegativos)
        {
            RemoverEstadosNegativos(unidad);
            aplicoAlgo = true;
        }

        if (data.modificarRegeneracionVida != 0)
        {
            unidad.estado_regeneravida += data.modificarRegeneracionVida;
            if (unidad.estado_regeneravida < 0) { unidad.estado_regeneravida = 0; }
            aplicoAlgo = true;
        }

        if (data.modificarRegeneracionArmadura != 0)
        {
            unidad.estado_regeneraarmadura += data.modificarRegeneracionArmadura;
            if (unidad.estado_regeneraarmadura < 0) { unidad.estado_regeneraarmadura = 0; }
            aplicoAlgo = true;
        }

        if (data.modificarEvasion != 0)
        {
            unidad.estado_evasion += data.modificarEvasion;
            if (unidad.estado_evasion < 0) { unidad.estado_evasion = 0; }
            aplicoAlgo = true;
        }

        bool tieneBuffPorReferencia = data.buffReferencia != null;
        bool tieneBuffPorStats = data.buff != null && data.buff.TieneCambios();
        if (data.aplicarBuff && (tieneBuffPorReferencia || tieneBuffPorStats))
        {
            Buff buff = new Buff();

            if (tieneBuffPorReferencia)
            {
                CopiarBuffBase(data.buffReferencia, buff);
            }

            AplicarDatosBuffConsumible(buff, data);

            if (!unidad.TieneBuffNombre(buff.buffNombre) || buff.esStackeable)
            {
                buff.AplicarBuff(unidad);
                ComponentCopier.CopyComponent(buff, unidad.gameObject);
                aplicoAlgo = true;
            }
        }

        if (aplicoAlgo && BattleManager.Instance != null && BattleManager.Instance.scUIInfoChar != null)
        {
            BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
        }
    }

    public ConsumibleEfectoData ObtenerEfectoConsumibleNormalizado()
    {
        ConsumibleEfectoData data = efectoConsumible != null ? efectoConsumible.Clone() : new ConsumibleEfectoData();
        if (data.TieneEfecto())
        {
            return data;
        }

        // Compatibilidad con consumibles legacy que tenian la logica hardcodeada en clases hijas.
        ConsumibleEfectoData legacy = ConstruirEfectoLegacyPorTipo();
        if (legacy.TieneEfecto())
        {
            return legacy;
        }

        return data;
    }

    private void RemoverEstadosNegativos(Unidad unidad)
    {
        unidad.estado_ardiendo = 0;
        unidad.estado_congelado = 0;
        unidad.estado_aturdido = 0;
        unidad.estado_inmovil = 0;
        unidad.estado_acido = 0;
        unidad.estado_sangrado = 0;
        unidad.estado_veneno = 0;
        unidad.estado_APModificador = 0;
        unidad.estado_ResistenciasReducidas = 0;
        unidad.estado_Condenado = 0;
        unidad.estado_CondenadoTurnosSeguidos = 0;
    }

    private void AplicarDatosBuffConsumible(Buff buff, ConsumibleEfectoData data)
    {
        if (buff == null || data == null)
        {
            return;
        }

        bool usarNombrePersonalizado = !string.IsNullOrWhiteSpace(data.nombreBuff)
            && data.nombreBuff != "Efecto de consumible";

        if (usarNombrePersonalizado)
        {
            buff.buffNombre = data.nombreBuff;
        }

        if (string.IsNullOrWhiteSpace(buff.buffNombre))
        {
            buff.buffNombre = sNombreItem;
        }

        buff.boolfDebufftBuff = data.buffEsBeneficio;
        buff.DuracionBuffRondas = data.duracionBuffRondas == 0 ? -1 : data.duracionBuffRondas;

        if (data.buff == null)
        {
            return;
        }

        buff.cantAtFue += data.buff.cantAtFue;
        buff.cantAtAgi += data.buff.cantAtAgi;
        buff.cantAtPod += data.buff.cantAtPod;
        buff.cantIniciativa += data.buff.cantIniciativa;
        buff.cantAPMax += data.buff.cantAPMax;
        buff.cantPMMax += data.buff.cantPMMax;
        buff.cantHPMax += data.buff.cantHPMax;
        buff.cantArmadura += data.buff.cantArmadura;
        buff.cantDefensa += data.buff.cantDefensa;
        buff.cantAtaque += data.buff.cantAtaque;
        buff.cantDanioPorcentaje += data.buff.cantDanioPorcentaje;
        buff.cantCritDado += data.buff.cantCritDado;
        buff.cantCritDaño += data.buff.cantCritDanio;
        buff.cantTsReflejos += data.buff.cantTsReflejos;
        buff.cantTsFortaleza += data.buff.cantTsFortaleza;
        buff.cantTsMental += data.buff.cantTsMental;
        buff.cantResFue += data.buff.cantResFue;
        buff.cantResHie += data.buff.cantResHie;
        buff.cantResRay += data.buff.cantResRay;
        buff.cantResAci += data.buff.cantResAci;
        buff.cantResArc += data.buff.cantResArc;
        buff.cantResNec += data.buff.cantResNec;
        buff.cantResDiv += data.buff.cantResDiv;
        buff.cantBarrera += data.buff.cantBarrera;
        buff.cantDamBonusElementalFue += data.buff.cantDamBonusElementalFue;
        buff.cantDamBonusElementalHie += data.buff.cantDamBonusElementalHie;
        buff.cantDamBonusElementalRay += data.buff.cantDamBonusElementalRay;
        buff.cantDamBonusElementalAci += data.buff.cantDamBonusElementalAci;
        buff.cantDamBonusElementalArc += data.buff.cantDamBonusElementalArc;
        buff.cantDamBonusElementalNec += data.buff.cantDamBonusElementalNec;
        buff.cantDamBonusElementalDiv += data.buff.cantDamBonusElementalDiv;
        buff.cantPenetracionArmadura += data.buff.cantPenetracionArmadura;
        buff.cantReduccionDanioRecibidoPorcentaje += data.buff.cantReduccionDanioRecibidoPorcentaje;
        buff.cantReduccionDanioCriticoRecibidoPorcentaje += data.buff.cantReduccionDanioCriticoRecibidoPorcentaje;
        buff.cantResistenciaEstadosPorcentaje += data.buff.cantResistenciaEstadosPorcentaje;
        buff.cantEspinasDanioPlano += data.buff.cantEspinasDanioPlano;
        buff.cantEspinasDanioPorcentaje += data.buff.cantEspinasDanioPorcentaje;
    }

    private static void CopiarBuffBase(Buff origen, Buff destino)
    {
        if (origen == null || destino == null)
        {
            return;
        }

        destino.buffNombre = origen.buffNombre;
        destino.buffDescr = origen.buffDescr;
        destino.suprimeTextoFlotante = origen.suprimeTextoFlotante;
        destino.boolfDebufftBuff = origen.boolfDebufftBuff;
        destino.percHPMax = origen.percHPMax;
        destino.cantHPMax = origen.cantHPMax;
        destino.percIniciativa = origen.percIniciativa;
        destino.cantIniciativa = origen.cantIniciativa;
        destino.percAPMax = origen.percAPMax;
        destino.cantAPMax = origen.cantAPMax;
        destino.percPMMax = origen.percPMMax;
        destino.cantPMMax = origen.cantPMMax;
        destino.percAtFue = origen.percAtFue;
        destino.cantAtFue = origen.cantAtFue;
        destino.percAtAgi = origen.percAtAgi;
        destino.cantAtAgi = origen.cantAtAgi;
        destino.percAtFPod = origen.percAtFPod;
        destino.cantAtPod = origen.cantAtPod;
        destino.percArmadura = origen.percArmadura;
        destino.cantArmadura = origen.cantArmadura;
        destino.percResFue = origen.percResFue;
        destino.cantResFue = origen.cantResFue;
        destino.percResHie = origen.percResHie;
        destino.cantResHie = origen.cantResHie;
        destino.percResRay = origen.percResRay;
        destino.cantResRay = origen.cantResRay;
        destino.percResAci = origen.percResAci;
        destino.cantResAci = origen.cantResAci;
        destino.percResArc = origen.percResArc;
        destino.cantResArc = origen.cantResArc;
        destino.percResNec = origen.percResNec;
        destino.cantResNec = origen.cantResNec;
        destino.percResDiv = origen.percResDiv;
        destino.cantResDiv = origen.cantResDiv;
        destino.cantBarrera = origen.cantBarrera;
        destino.cantDamBonusElementalFue = origen.cantDamBonusElementalFue;
        destino.cantDamBonusElementalHie = origen.cantDamBonusElementalHie;
        destino.cantDamBonusElementalRay = origen.cantDamBonusElementalRay;
        destino.cantDamBonusElementalAci = origen.cantDamBonusElementalAci;
        destino.cantDamBonusElementalArc = origen.cantDamBonusElementalArc;
        destino.cantDamBonusElementalNec = origen.cantDamBonusElementalNec;
        destino.cantDamBonusElementalDiv = origen.cantDamBonusElementalDiv;
        destino.cantPenetracionArmadura = origen.cantPenetracionArmadura;
        destino.cantReduccionDanioRecibidoPorcentaje = origen.cantReduccionDanioRecibidoPorcentaje;
        destino.cantReduccionDanioCriticoRecibidoPorcentaje = origen.cantReduccionDanioCriticoRecibidoPorcentaje;
        destino.cantResistenciaEstadosPorcentaje = origen.cantResistenciaEstadosPorcentaje;
        destino.cantEspinasDanioPlano = origen.cantEspinasDanioPlano;
        destino.cantEspinasDanioPorcentaje = origen.cantEspinasDanioPorcentaje;
        destino.percDefensa = origen.percDefensa;
        destino.cantDefensa = origen.cantDefensa;
        destino.percAtaque = origen.percAtaque;
        destino.cantAtaque = origen.cantAtaque;
        destino.cantDanioPorcentaje = origen.cantDanioPorcentaje;
        destino.cantCritDado = origen.cantCritDado;
        destino.percCritDaño = origen.percCritDaño;
        destino.cantCritDaño = origen.cantCritDaño;
        destino.percTsReflejos = origen.percTsReflejos;
        destino.cantTsReflejos = origen.cantTsReflejos;
        destino.percTsFortaleza = origen.percTsFortaleza;
        destino.cantTsFortaleza = origen.cantTsFortaleza;
        destino.percTsMental = origen.percTsMental;
        destino.cantTsMental = origen.cantTsMental;
        destino.DuracionBuffRondas = origen.DuracionBuffRondas;
        destino.goVFX = origen.goVFX;
        destino.esBuffVisibleUI = origen.esBuffVisibleUI;
        destino.esRemovible = origen.esRemovible;
        destino.esStackeable = origen.esStackeable;
        destino.CustomEffectInicioTurnoID = origen.CustomEffectInicioTurnoID;
        destino.seConsumeAlRecibirAtaque = origen.seConsumeAlRecibirAtaque;
        destino.unidadOrigen = origen.unidadOrigen;
    }

    private ConsumibleEfectoData ConstruirEfectoLegacyPorTipo()
    {
        string tipo = GetType().Name;
        ConsumibleEfectoData data = new ConsumibleEfectoData();

        switch (tipo)
        {
            case "Cons_PocionCuracionMenor":
                data.curacionBase = 5;
                data.curacionDadosCantidad = 1;
                data.curacionDadosCaras = 6;
                break;
            case "Cons_PocionCuracionMedia":
                data.curacionBase = 12;
                data.curacionDadosCantidad = 1;
                data.curacionDadosCaras = 8;
                break;
            case "Cons_PocionCuracionMayor":
                data.curacionBase = 20;
                data.curacionDadosCantidad = 2;
                data.curacionDadosCaras = 8;
                break;
            case "Cons_Panacea":
                data.removerDebuffs = true;
                data.removerEstadosNegativos = true;
                data.aplicarBuff = false;
                break;
            case "Cons_BalsamoFort":
                data.nombreBuff = "Balsamo Fortalecedor";
                data.buff.cantTsFortaleza = 2;
                break;
            case "Cons_BalsamoEnergizante":
                data.nombreBuff = "Balsamo Energizante";
                data.buff.cantTsReflejos = 2;
                break;
            case "Cons_BalsamoClaridad":
                data.nombreBuff = "Balsamo de Claridad";
                data.buff.cantTsMental = 2;
                break;
            case "Cons_ElixirResistenciaFuego":
                data.nombreBuff = "Elixir de Resistencia al Fuego";
                data.buff.cantResFue = 5;
                break;
            case "Cons_ElixirResistenciaFrio":
                data.nombreBuff = "Elixir de Resistencia al Frio";
                data.buff.cantResHie = 5;
                break;
            case "Cons_ElixirResistenciaRayo":
                data.nombreBuff = "Elixir de Resistencia al Rayo";
                data.buff.cantResRay = 5;
                break;
            case "Cons_ElixirResistenciaAcido":
                data.nombreBuff = "Elixir de Resistencia al Acido";
                data.buff.cantResAci = 5;
                break;
            case "Cons_SimboloArcanoProteccion":
                data.nombreBuff = "Proteccion Arcana";
                data.duracionBuffRondas = 3;
                data.buff.cantResAci = 3;
                data.buff.cantResFue = 3;
                data.buff.cantResHie = 3;
                data.buff.cantResRay = 3;
                data.buff.cantResArc = 3;
                break;
            default:
                break;
        }

        return data;
    }
}



