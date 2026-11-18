using System;
using System.Collections.Generic;

public enum BanterDisparador
{
    ComenzarBatalla,
    EmboscadaEnemiga,
    LlegadaEnemiga,
    IdleTurno,
    EnemigoDerrotado,
    AliadoDerrotado,
    RefuerzoEnemigo,
    CriticoRecibido,
    CriticoRealizado,
    MoralAumentaEtapa,
    MoralDisminuyeEtapa,
    HabilidadAliadaRecibida,
    VidaCritica,
    Pifia,
    AliadoEscapa
}

public sealed class BanterLineaLocal
{
    public string Id { get; }
    public BanterDisparador Disparador { get; }
    public string Espanol { get; }
    public string Ingles { get; }
    public string Portugues { get; }
    public string Chino { get; }

    public BanterLineaLocal(
        string id,
        BanterDisparador disparador,
        string espanol,
        string ingles,
        string portugues,
        string chino = null)
    {
        Id = id;
        Disparador = disparador;
        Espanol = espanol;
        Ingles = ingles;
        Portugues = portugues;
        Chino = chino;
    }

    public string ObtenerTextoActual()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
        if (idioma == TRADU.IdiomaChinoSimplificado)
        {
            if (!string.IsNullOrWhiteSpace(Chino))
            {
                return Chino;
            }
            return !string.IsNullOrWhiteSpace(Ingles) ? Ingles : Espanol;
        }
        if (idioma == 2 && !string.IsNullOrWhiteSpace(Ingles))
        {
            return Ingles;
        }
        if (idioma == 3 && !string.IsNullOrWhiteSpace(Portugues))
        {
            return Portugues;
        }
        return Espanol;
    }
}

public static partial class BanterContenidoLocal
{
    private static readonly Dictionary<Type, List<BanterLineaLocal>> lineasPorClase =
        new Dictionary<Type, List<BanterLineaLocal>>();

    static BanterContenidoLocal()
    {
        RegistrarContenido();
    }

    public static IReadOnlyList<BanterLineaLocal> ObtenerLineas(
        Unidad unidad,
        BanterDisparador disparador)
    {
        if (unidad == null)
        {
            return Array.Empty<BanterLineaLocal>();
        }

        Type tipo = unidad.GetType();
        while (tipo != null && typeof(Unidad).IsAssignableFrom(tipo))
        {
            if (lineasPorClase.TryGetValue(tipo, out List<BanterLineaLocal> lineas))
            {
                List<BanterLineaLocal> coincidencias = lineas.FindAll(
                    linea => linea != null && linea.Disparador == disparador);
                if (coincidencias.Count > 0)
                {
                    return coincidencias;
                }
            }
            tipo = tipo.BaseType;
        }

        return Array.Empty<BanterLineaLocal>();
    }

    private static void Registrar<TUnidad>(params BanterLineaLocal[] lineas)
        where TUnidad : Unidad
    {
        Type tipo = typeof(TUnidad);
        if (!lineasPorClase.TryGetValue(tipo, out List<BanterLineaLocal> existentes))
        {
            existentes = new List<BanterLineaLocal>();
            lineasPorClase.Add(tipo, existentes);
        }
        existentes.AddRange(lineas);
    }

    static partial void RegistrarContenido();
}
