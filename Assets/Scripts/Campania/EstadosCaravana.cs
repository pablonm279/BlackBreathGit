using System;
using UnityEngine;

public enum TipoEstadoCaravana
{
    Inspiracion,
    Presteza,
    Compromiso,
    Vigilante,
    Acobardados,
    Aletargados,
    Desmotivacion,
    Descuidados
}

[Serializable]
public sealed class EstadosCaravanaSaveData
{
    public int inspiracion;
    public int presteza;
    public int compromiso;
    public int vigilante;
    public int acobardados;
    public int aletargados;
    public int desmotivacion;
    public int descuidados;
    public bool viajeActualVigilante;
    public bool viajeActualDescuidados;
}

public struct EfectosViajeCaravana
{
    public bool previeneAvanceAliento;
    public int avanceAlientoExtra;
    public float multiplicadorVelocidadVisual;
}

[Serializable]
public sealed class EstadosCaravana
{
    private static readonly TipoEstadoCaravana[] EstadosPositivosAleatorios =
    {
        TipoEstadoCaravana.Inspiracion,
        TipoEstadoCaravana.Presteza,
        TipoEstadoCaravana.Compromiso,
        TipoEstadoCaravana.Vigilante
    };

    public const int BonusValInspiracion = 2;
    public const int PenaltyValAcobardados = -2;
    public const float BonusExpCompromiso = 0.20f;
    public const float PenaltyExpDesmotivacion = -0.20f;
    public const int ModExploracionVigilante = 10;
    public const int ModEmboscadaVigilante = -10;
    public const int ModExploracionDescuidados = -10;
    public const int ModEmboscadaDescuidados = 10;
    public const float MultiplicadorVelocidadVisualPresteza = 1.20f;
    public const float MultiplicadorVelocidadVisualAletargados = 0.80f;

    [SerializeField] private int inspiracion;
    [SerializeField] private int presteza;
    [SerializeField] private int compromiso;
    [SerializeField] private int vigilante;
    [SerializeField] private int acobardados;
    [SerializeField] private int aletargados;
    [SerializeField] private int desmotivacion;
    [SerializeField] private int descuidados;

    [NonSerialized] private bool combateActualInspiracion;
    [NonSerialized] private bool combateActualCompromiso;
    [NonSerialized] private bool combateActualAcobardados;
    [NonSerialized] private bool combateActualDesmotivacion;
    [NonSerialized] private bool viajeActualVigilante;
    [NonSerialized] private bool viajeActualDescuidados;

    public int InspiracionStacks => inspiracion;
    public int PrestezaStacks => presteza;
    public int CompromisoStacks => compromiso;
    public int VigilanteStacks => vigilante;
    public int AcobardadosStacks => acobardados;
    public int AletargadosStacks => aletargados;
    public int DesmotivacionStacks => desmotivacion;
    public int DescuidadosStacks => descuidados;

    public static TipoEstadoCaravana ObtenerEstadoPositivoAleatorio()
    {
        return EstadosPositivosAleatorios[UnityEngine.Random.Range(0, EstadosPositivosAleatorios.Length)];
    }

    public static string ObtenerNombreVisible(TipoEstadoCaravana tipo)
    {
        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion: return "Inspiración";
            case TipoEstadoCaravana.Presteza: return "Presteza";
            case TipoEstadoCaravana.Compromiso: return "Compromiso";
            case TipoEstadoCaravana.Vigilante: return "Vigilante";
            case TipoEstadoCaravana.Acobardados: return "Acobardados";
            case TipoEstadoCaravana.Aletargados: return "Aletargados";
            case TipoEstadoCaravana.Desmotivacion: return "Desmotivación";
            case TipoEstadoCaravana.Descuidados: return "Descuidados";
            default: return string.Empty;
        }
    }

    public void AgregarEstado(TipoEstadoCaravana tipo, int stacks = 1)
    {
        if (stacks <= 0)
        {
            return;
        }

        TipoEstadoCaravana? tipoOpuesto = ObtenerTipoOpuesto(tipo);
        if (tipoOpuesto.HasValue)
        {
            int stacksOpuestos = ObtenerStacks(tipoOpuesto.Value);
            int stacksConsumidos = Mathf.Min(stacks, stacksOpuestos);
            if (stacksConsumidos > 0)
            {
                RestarStacks(tipoOpuesto.Value, stacksConsumidos);
                stacks -= stacksConsumidos;
            }
        }

        if (stacks <= 0)
        {
            return;
        }

        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion:
                inspiracion += stacks;
                break;
            case TipoEstadoCaravana.Presteza:
                presteza += stacks;
                break;
            case TipoEstadoCaravana.Compromiso:
                compromiso += stacks;
                break;
            case TipoEstadoCaravana.Vigilante:
                vigilante += stacks;
                break;
            case TipoEstadoCaravana.Acobardados:
                acobardados += stacks;
                break;
            case TipoEstadoCaravana.Aletargados:
                aletargados += stacks;
                break;
            case TipoEstadoCaravana.Desmotivacion:
                desmotivacion += stacks;
                break;
            case TipoEstadoCaravana.Descuidados:
                descuidados += stacks;
                break;
        }
    }

    private static TipoEstadoCaravana? ObtenerTipoOpuesto(TipoEstadoCaravana tipo)
    {
        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion: return TipoEstadoCaravana.Acobardados;
            case TipoEstadoCaravana.Acobardados: return TipoEstadoCaravana.Inspiracion;
            case TipoEstadoCaravana.Presteza: return TipoEstadoCaravana.Aletargados;
            case TipoEstadoCaravana.Aletargados: return TipoEstadoCaravana.Presteza;
            case TipoEstadoCaravana.Compromiso: return TipoEstadoCaravana.Desmotivacion;
            case TipoEstadoCaravana.Desmotivacion: return TipoEstadoCaravana.Compromiso;
            case TipoEstadoCaravana.Vigilante: return TipoEstadoCaravana.Descuidados;
            case TipoEstadoCaravana.Descuidados: return TipoEstadoCaravana.Vigilante;
            default: return null;
        }
    }

    private void RestarStacks(TipoEstadoCaravana tipo, int stacks)
    {
        if (stacks <= 0)
        {
            return;
        }

        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion:
                inspiracion = Mathf.Max(0, inspiracion - stacks);
                break;
            case TipoEstadoCaravana.Presteza:
                presteza = Mathf.Max(0, presteza - stacks);
                break;
            case TipoEstadoCaravana.Compromiso:
                compromiso = Mathf.Max(0, compromiso - stacks);
                break;
            case TipoEstadoCaravana.Vigilante:
                vigilante = Mathf.Max(0, vigilante - stacks);
                break;
            case TipoEstadoCaravana.Acobardados:
                acobardados = Mathf.Max(0, acobardados - stacks);
                break;
            case TipoEstadoCaravana.Aletargados:
                aletargados = Mathf.Max(0, aletargados - stacks);
                break;
            case TipoEstadoCaravana.Desmotivacion:
                desmotivacion = Mathf.Max(0, desmotivacion - stacks);
                break;
            case TipoEstadoCaravana.Descuidados:
                descuidados = Mathf.Max(0, descuidados - stacks);
                break;
        }
    }

    public int ObtenerStacks(TipoEstadoCaravana tipo)
    {
        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion: return inspiracion;
            case TipoEstadoCaravana.Presteza: return presteza;
            case TipoEstadoCaravana.Compromiso: return compromiso;
            case TipoEstadoCaravana.Vigilante: return vigilante;
            case TipoEstadoCaravana.Acobardados: return acobardados;
            case TipoEstadoCaravana.Aletargados: return aletargados;
            case TipoEstadoCaravana.Desmotivacion: return desmotivacion;
            case TipoEstadoCaravana.Descuidados: return descuidados;
            default: return 0;
        }
    }

    public int ObtenerModificadorExploracionPendiente()
    {
        int mod = 0;
        if (vigilante > 0) mod += ModExploracionVigilante;
        if (descuidados > 0) mod += ModExploracionDescuidados;
        return mod;
    }

    public int ObtenerModificadorEmboscadaDescansoPendiente()
    {
        int mod = 0;
        if (vigilante > 0) mod += ModEmboscadaVigilante;
        if (descuidados > 0) mod += ModEmboscadaDescuidados;
        return mod;
    }

    public int ObtenerModificadorEmboscadaDuranteViajeActual()
    {
        int mod = 0;
        if (viajeActualVigilante) mod += ModEmboscadaVigilante;
        if (viajeActualDescuidados) mod += ModEmboscadaDescuidados;
        return mod;
    }

    public EfectosViajeCaravana IniciarViajeActual()
    {
        viajeActualVigilante = vigilante > 0;
        viajeActualDescuidados = descuidados > 0;

        bool prestezaActiva = presteza > 0;
        bool aletargadosActivos = aletargados > 0;

        if (presteza > 0) presteza--;
        if (aletargados > 0) aletargados--;
        if (vigilante > 0) vigilante--;
        if (descuidados > 0) descuidados--;

        float multiplicadorVelocidadVisual = 1f;
        if (prestezaActiva) multiplicadorVelocidadVisual *= MultiplicadorVelocidadVisualPresteza;
        if (aletargadosActivos) multiplicadorVelocidadVisual *= MultiplicadorVelocidadVisualAletargados;

        return new EfectosViajeCaravana
        {
            previeneAvanceAliento = false,
            avanceAlientoExtra = 0,
            multiplicadorVelocidadVisual = multiplicadorVelocidadVisual
        };
    }

    public float ObtenerMultiplicadorVelocidadViajePendiente()
    {
        float multiplicador = 1f;
        if (presteza > 0) multiplicador *= MultiplicadorVelocidadVisualPresteza;
        if (aletargados > 0) multiplicador *= MultiplicadorVelocidadVisualAletargados;
        return multiplicador;
    }

    public void FinalizarViajeActual()
    {
        viajeActualVigilante = false;
        viajeActualDescuidados = false;
    }

    public void IniciarCombateActual()
    {
        FinalizarCombateActual();

        combateActualInspiracion = inspiracion > 0;
        combateActualCompromiso = compromiso > 0;
        combateActualAcobardados = acobardados > 0;
        combateActualDesmotivacion = desmotivacion > 0;

        if (inspiracion > 0) inspiracion--;
        if (compromiso > 0) compromiso--;
        if (acobardados > 0) acobardados--;
        if (desmotivacion > 0) desmotivacion--;
    }

    public void FinalizarCombateActual()
    {
        combateActualInspiracion = false;
        combateActualCompromiso = false;
        combateActualAcobardados = false;
        combateActualDesmotivacion = false;
    }

    public int ObtenerModificadorValentiaCombateActual()
    {
        int mod = 0;
        if (combateActualInspiracion) mod += BonusValInspiracion;
        if (combateActualAcobardados) mod += PenaltyValAcobardados;
        return mod;
    }

    public float ObtenerMultiplicadorExperienciaCombateActual()
    {
        float mult = 1f;
        if (combateActualCompromiso) mult += BonusExpCompromiso;
        if (combateActualDesmotivacion) mult += PenaltyExpDesmotivacion;
        return Mathf.Max(0f, mult);
    }

    public float AplicarMultiplicadorExperienciaCombateActual(float experienciaBase)
    {
        if (experienciaBase <= 0f)
        {
            return 0f;
        }

        return experienciaBase * ObtenerMultiplicadorExperienciaCombateActual();
    }

    public EstadosCaravanaSaveData ConstruirSaveData()
    {
        return new EstadosCaravanaSaveData
        {
            inspiracion = inspiracion,
            presteza = presteza,
            compromiso = compromiso,
            vigilante = vigilante,
            acobardados = acobardados,
            aletargados = aletargados,
            desmotivacion = desmotivacion,
            descuidados = descuidados,
            viajeActualVigilante = viajeActualVigilante,
            viajeActualDescuidados = viajeActualDescuidados
        };
    }

    public void RestaurarDesdeSave(EstadosCaravanaSaveData data)
    {
        inspiracion = Mathf.Max(0, data != null ? data.inspiracion : 0);
        presteza = Mathf.Max(0, data != null ? data.presteza : 0);
        compromiso = Mathf.Max(0, data != null ? data.compromiso : 0);
        vigilante = Mathf.Max(0, data != null ? data.vigilante : 0);
        acobardados = Mathf.Max(0, data != null ? data.acobardados : 0);
        aletargados = Mathf.Max(0, data != null ? data.aletargados : 0);
        desmotivacion = Mathf.Max(0, data != null ? data.desmotivacion : 0);
        descuidados = Mathf.Max(0, data != null ? data.descuidados : 0);

        viajeActualVigilante = data != null && data.viajeActualVigilante;
        viajeActualDescuidados = data != null && data.viajeActualDescuidados;
        FinalizarCombateActual();
    }
}
