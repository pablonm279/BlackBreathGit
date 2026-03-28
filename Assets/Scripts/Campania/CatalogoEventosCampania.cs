using System.Collections.Generic;
using UnityEngine;

public enum TipoOrigenEventoCampania
{
    Nodo,
    Descanso
}

public enum TipoResultadoEventoCampania
{
    Malo,
    Bueno
}

public static class IdsZonaCampania
{
    public const int Generica = 0;
    public const int BosqueAngustiante = 1;
    public const int PasoVientoHelado = 2;
    public const int Nedukazal = 3;
}

public static class IdsEventoCampania
{
    // Eventos de nodo malos genericos
    public const int RetrasoNocturno = 1;
    public const int DesaparicionesMisteriosas = 2;
    public const int BueyesEnfermos = 3;
    public const int PeajeCriminal = 4;
    public const int PersonajeEnfermo = 5;
    public const int ArcasRobadas = 6;
    public const int CarroDeteriorado = 7;
    public const int RioContaminado = 8;
    public const int Rina = 9;
    public const int LiderazgoCuestionado = 10;
    public const int PasoPrecario = 11;
    public const int AireEnrarecido = 12;
    public const int RumorDeDesbande = 13;
    public const int VadoTraicionero = 14;
    public const int CarroEncajado = 15;
    public const int SiluetasEntreLosArboles = 16;
    public const int BarroQueRetiene = 17;
    public const int PromesaIncumplida = 18;
    public const int RutinaFloja = 19;

    // Eventos de descanso malos genericos
    public const int HumoEnElCampamento = 101;
    public const int GuardiaSomnolienta = 102;
    public const int RacionesMojadas = 103;
    public const int DiscusionEnLaFogata = 104;
    public const int HerramientasPerdidas = 105;
    public const int EscalofriosNocturnos = 106;
    public const int NocheEnVela = 107;
    public const int PracticaImprudente = 108;
    public const int BrasasErrantes = 109;
    public const int TroncoReavivado = 110;
    public const int CanticosDeMadrugada = 111;
    public const int HuellasAlrededorDelCampamento = 112;
    public const int VigiliaHelada = 113;
    public const int SimboloEnLaNieve = 114;
    public const int LuzAhogada = 115;
    public const int TechoInestable = 116;
    public const int RuidosEnLaBodega = 117;
    public const int ListaEnLaPared = 118;
    public const int PesadillasCompartidas = 119;
    public const int DescansoIncompleto = 120;
    public const int QuejasEnVozBaja = 121;
    public const int FogatasDemasiadoLejos = 122;

    // Eventos de nodo buenos genericos
    public const int DestelloEsperanzador = 201;
    public const int RisotadasEnLaCaravana = 202;
    public const int CaravanaPerdida = 203;
    public const int AserraderoAbandonado = 204;
    public const int ManadaDeBueyes = 205;
    public const int CivilesEnApuros = 206;
    public const int Tranquilidad = 207;
    public const int VotoDeConfianza = 208;
    public const int LugarenoAnciano = 209;
    public const int SuenoInspirador = 210;
    public const int MarcasDelCorreo = 211;
    public const int PulsoDeMando = 212;
    public const int HombrosFirmes = 213;
    public const int ManoCierta = 214;
    public const int DosMiradas = 215;
    public const int ArengaEnLaLluvia = 216;
    public const int CaminoAFavor = 217;
    public const int JuramentoDeLaEscolta = 218;
    public const int RastroSospechoso = 219;

    // Eventos de descanso buenos genericos
    public const int NocheSerena = 301;
    public const int FogonCompartido = 302;
    public const int ManosVoluntariosas = 303;
    public const int SuenoReparador = 304;
    public const int HallazgoEntreLosCarros = 305;
    public const int BolsaOlvidada = 306;
    public const int LeccionJuntoAlFuego = 307;
    public const int PalabrasNecesarias = 308;
    public const int CalorDeLasCenizas = 309;
    public const int HongosDelCarbon = 310;
    public const int ComercianteVisitante = 311;
    public const int RefuerzoEnElCamino = 312;
    public const int MensajeDesdeSerria = 313;
    public const int ParedonContraElViento = 314;
    public const int PasoEnSilencio = 315;
    public const int RastroDelRebano = 316;
    public const int AuroraDelPaso = 317;
    public const int VentanaVigilante = 318;
    public const int FogonEntreEscombros = 319;
    public const int PatioDeEvacuacion = 320;
    public const int CartaSinEnviar = 321;
    public const int CirculoDeHistorias = 322;
    public const int CampamentoLigero = 323;
    public const int RepasoDeManiobras = 324;
    public const int GuardiasRelevados = 325;

    // Eventos manuales / especificos de nodo
    public const int CenizasEnElCamino = 81;
    public const int BestiasAterradas = 82;
    public const int FuegoEnLaRetaguardia = 83;
    public const int TamboresEnLaNiebla = 84;
    public const int HieloQuebradizo = 85;
    public const int EfigiesDelPaso = 86;
    public const int PartidaDeCaza = 87;
    public const int FrioHastaLosHuesos = 88;
    public const int TotemDeGuerra = 89;
    public const int GolpesBajoElEmpedrado = 90;
    public const int BrechaEnLaCalzada = 91;
    public const int EcosEnElPozo = 92;
    public const int CampanasSinTorre = 93;
    public const int AcechoEnLosTejados = 94;
    public const int PuertaAstillada = 95;
    public const int HumoInquieto = 96;
    public const int CuervosDelPaso = 97;
    public const int EcoBajoLosPies = 98;
    public const int BroteEntreLasBrasas = 281;
    public const int MaderaMedioQuemada = 282;
    public const int RefugioDePiedra = 283;
    public const int SenderoDelCarnero = 284;
    public const int CieloAbierto = 285;
    public const int EfigieDerribada = 286;
    public const int JuramentoDelPaso = 287;
    public const int VientoAFavor = 288;
    public const int FarolesPrestados = 289;
    public const int BarricadaTodaviaEnPie = 290;
    public const int SotanoConVida = 291;
    public const int SenalesDeTiza = 292;
    public const int ValorEnLaPlaza = 293;
    public const int CampamentoEntreColumnas = 294;
    public const int VetaDeResina = 295;
    public const int VigiaDelHielo = 296;
    public const int SenalDeLosResistentes = 297;
    public const int Claro = 401;
    public const int Asentamiento = 402;
    public const int Recursos = 403;
    public const int MisionSalvamento = 404;
    public const int EncuentroEsperado = 405;
}

internal sealed class DefinicionEventoAleatorioCampania
{
    readonly int[] zonasBloqueadas;

    private DefinicionEventoAleatorioCampania(
        int id,
        string nombreDebug,
        TipoOrigenEventoCampania origen,
        TipoResultadoEventoCampania resultado,
        int zonaEspecifica,
        int[] zonasBloqueadas)
    {
        Id = id;
        NombreDebug = nombreDebug;
        Origen = origen;
        Resultado = resultado;
        ZonaEspecifica = zonaEspecifica;
        this.zonasBloqueadas = zonasBloqueadas ?? new int[0];
    }

    public int Id { get; private set; }
    public string NombreDebug { get; private set; }
    public TipoOrigenEventoCampania Origen { get; private set; }
    public TipoResultadoEventoCampania Resultado { get; private set; }
    public int ZonaEspecifica { get; private set; }
    public bool EsZonal => ZonaEspecifica != IdsZonaCampania.Generica;

    public static DefinicionEventoAleatorioCampania CrearGenerico(
        int id,
        string nombreDebug,
        TipoOrigenEventoCampania origen,
        TipoResultadoEventoCampania resultado,
        params int[] zonasBloqueadas)
    {
        return new DefinicionEventoAleatorioCampania(
            id,
            nombreDebug,
            origen,
            resultado,
            IdsZonaCampania.Generica,
            zonasBloqueadas);
    }

    public static DefinicionEventoAleatorioCampania CrearZonal(
        int id,
        string nombreDebug,
        TipoOrigenEventoCampania origen,
        TipoResultadoEventoCampania resultado,
        int zonaEspecifica)
    {
        return new DefinicionEventoAleatorioCampania(
            id,
            nombreDebug,
            origen,
            resultado,
            zonaEspecifica,
            null);
    }

    public bool EstaDisponibleEnZona(int zonaId)
    {
        if (EsZonal && ZonaEspecifica != zonaId)
        {
            return false;
        }

        for (int i = 0; i < zonasBloqueadas.Length; i++)
        {
            if (zonasBloqueadas[i] == zonaId)
            {
                return false;
            }
        }

        return true;
    }
}

public static class CatalogoEventosCampania
{
    const float ChanceEventoZonalNodo = 25f;
    const float ChanceEventoZonalDescanso = 25f;

    static readonly List<DefinicionEventoAleatorioCampania> definiciones = new List<DefinicionEventoAleatorioCampania>
    {
        // Eventos de nodo malos genericos
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RetrasoNocturno, "Retraso Nocturno", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.DesaparicionesMisteriosas, "Desapariciones Misteriosas", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.BueyesEnfermos, "Bueyes Enfermos", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PeajeCriminal, "Peaje Criminal", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PersonajeEnfermo, "Personaje Enfermo", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.ArcasRobadas, "Arcas Robadas", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.CarroDeteriorado, "Carro Deteriorado", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RioContaminado, "Rio Contaminado", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.Rina, "Rina", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.LiderazgoCuestionado, "Liderazgo Cuestionado", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PasoPrecario, "Paso Precario", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.AireEnrarecido, "Aire Enrarecido", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RumorDeDesbande, "Rumor de Desbande", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.VadoTraicionero, "Vado Traicionero", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.CarroEncajado, "Carro Encajado", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.SiluetasEntreLosArboles, "Siluetas entre los Arboles", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.BarroQueRetiene, "Barro que Retiene", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PromesaIncumplida, "Promesa Incumplida", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RutinaFloja, "Rutina Floja", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo),

        // Eventos de descanso malos genericos
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.HumoEnElCampamento, "Humo en el Campamento", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.GuardiaSomnolienta, "Guardia Somnolienta", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RacionesMojadas, "Raciones Mojadas", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.DiscusionEnLaFogata, "Discusion en la Fogata", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.HerramientasPerdidas, "Herramientas Perdidas", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.EscalofriosNocturnos, "Escalofrios Nocturnos", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.NocheEnVela, "Noche en Vela", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PracticaImprudente, "Practica Imprudente", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PesadillasCompartidas, "Pesadillas Compartidas", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.DescansoIncompleto, "Descanso Incompleto", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.QuejasEnVozBaja, "Quejas en Voz Baja", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.FogatasDemasiadoLejos, "Fogatas Demasiado Lejos", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo),

        // Eventos de nodo buenos genericos
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.DestelloEsperanzador, "Destello Esperanzador", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RisotadasEnLaCaravana, "Risotadas en la Caravana", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.CaravanaPerdida, "Caravana Perdida", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.AserraderoAbandonado, "Aserradero Abandonado", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.ManadaDeBueyes, "Manada de Bueyes", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.CivilesEnApuros, "Civiles en Apuros", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.Tranquilidad, "Tranquilidad", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.VotoDeConfianza, "Voto de Confianza", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.LugarenoAnciano, "Lugareno Anciano", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.SuenoInspirador, "Sueno Inspirador", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.MarcasDelCorreo, "Marcas del Correo", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PulsoDeMando, "Pulso de Mando", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.HombrosFirmes, "Hombros Firmes", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.ManoCierta, "Mano Cierta", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.DosMiradas, "Dos Miradas", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.ArengaEnLaLluvia, "Arenga en la Lluvia", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.CaminoAFavor, "Camino a Favor", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.JuramentoDeLaEscolta, "Juramento de la Escolta", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RastroSospechoso, "Rastro Sospechoso", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno),

        // Eventos de descanso buenos genericos
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.NocheSerena, "Noche Serena", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.FogonCompartido, "Fogon Compartido", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.ManosVoluntariosas, "Manos Voluntariosas", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.SuenoReparador, "Sueno Reparador", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.HallazgoEntreLosCarros, "Hallazgo entre los Carros", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.BolsaOlvidada, "Bolsa Olvidada", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.LeccionJuntoAlFuego, "Leccion junto al Fuego", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.PalabrasNecesarias, "Palabras Necesarias", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.ComercianteVisitante, "Comerciante Visitante", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RefuerzoEnElCamino, "Refuerzo en el Camino", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.MensajeDesdeSerria, "Mensaje desde Serria", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.CirculoDeHistorias, "Circulo de Historias", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.CampamentoLigero, "Campamento Ligero", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.RepasoDeManiobras, "Repaso de Maniobras", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),
        DefinicionEventoAleatorioCampania.CrearGenerico(IdsEventoCampania.GuardiasRelevados, "Guardias Relevados", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno),

        // Eventos de nodo zonales
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CenizasEnElCamino, "Cenizas en el Camino", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.BestiasAterradas, "Bestias Aterradas", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.FuegoEnLaRetaguardia, "Fuego en la Retaguardia", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.TamboresEnLaNiebla, "Tambores en la Niebla", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.HieloQuebradizo, "Hielo Quebradizo", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.EfigiesDelPaso, "Efigies del Paso", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.PartidaDeCaza, "Partida de Caza", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.FrioHastaLosHuesos, "Frío Hasta los Huesos", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.TotemDeGuerra, "Tótem de Guerra", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.GolpesBajoElEmpedrado, "Golpes Bajo el Empedrado", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.BrechaEnLaCalzada, "Brecha en la Calzada", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.EcosEnElPozo, "Ecos en el Pozo", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CampanasSinTorre, "Campanas sin Torre", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.AcechoEnLosTejados, "Acecho en los Tejados", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.PuertaAstillada, "Puerta Astillada", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.HumoInquieto, "Humo Inquieto", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CuervosDelPaso, "Cuervos del Paso", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.EcoBajoLosPies, "Eco Bajo los Pies", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.BroteEntreLasBrasas, "Brote entre las Brasas", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.MaderaMedioQuemada, "Madera Medio Quemada", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.RefugioDePiedra, "Refugio de Piedra", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.SenderoDelCarnero, "Sendero del Carnero", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CieloAbierto, "Cielo Abierto", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.EfigieDerribada, "Efigie Derribada", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.JuramentoDelPaso, "Juramento del Paso", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.VientoAFavor, "Viento a Favor", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.FarolesPrestados, "Faroles Prestados", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.BarricadaTodaviaEnPie, "Barricada Todavía en Pie", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.SotanoConVida, "Sótano con Vida", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.SenalesDeTiza, "Señales de Tiza", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.ValorEnLaPlaza, "Valor en la Plaza", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CampamentoEntreColumnas, "Campamento entre Columnas", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.VetaDeResina, "Veta de Resina", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.VigiaDelHielo, "Vigia del Hielo", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.SenalDeLosResistentes, "Señal de los Resistentes", TipoOrigenEventoCampania.Nodo, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),

        // Eventos de descanso zonales
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.BrasasErrantes, "Brasas Errantes", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.TroncoReavivado, "Tronco Reavivado", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CanticosDeMadrugada, "Cánticos de Madrugada", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.HuellasAlrededorDelCampamento, "Huellas alrededor del Campamento", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.VigiliaHelada, "Vigilia Helada", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.SimboloEnLaNieve, "Símbolo en la Nieve", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.LuzAhogada, "Luz Ahogada", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.TechoInestable, "Techo Inestable", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.RuidosEnLaBodega, "Ruidos en la Bodega", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.ListaEnLaPared, "Lista en la Pared", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Malo, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CalorDeLasCenizas, "Calor de las Cenizas", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.HongosDelCarbon, "Hongos del Carbon", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.BosqueAngustiante),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.ParedonContraElViento, "Paredón contra el Viento", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.PasoEnSilencio, "Paso en Silencio", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.RastroDelRebano, "Rastro del Rebaño", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.AuroraDelPaso, "Aurora del Paso", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.PasoVientoHelado),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.VentanaVigilante, "Ventana Vigilante", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.FogonEntreEscombros, "Fogón entre Escombros", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.PatioDeEvacuacion, "Patio de Evacuación", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
        DefinicionEventoAleatorioCampania.CrearZonal(IdsEventoCampania.CartaSinEnviar, "Carta sin Enviar", TipoOrigenEventoCampania.Descanso, TipoResultadoEventoCampania.Bueno, IdsZonaCampania.Nedukazal),
    };

    public static bool TryObtenerEventoAleatorio(
        TipoOrigenEventoCampania origen,
        TipoResultadoEventoCampania resultado,
        int zonaId,
        List<int> idsEventosExcluidos,
        out int idEvento)
    {
        List<int> candidatosGenericos = new List<int>();
        List<int> candidatosZonales = new List<int>();

        for (int i = 0; i < definiciones.Count; i++)
        {
            DefinicionEventoAleatorioCampania definicion = definiciones[i];
            if (definicion.Origen != origen || definicion.Resultado != resultado)
            {
                continue;
            }

            if (!definicion.EstaDisponibleEnZona(zonaId))
            {
                continue;
            }

            if (idsEventosExcluidos != null && idsEventosExcluidos.Contains(definicion.Id))
            {
                continue;
            }

            if (!CumpleDisponibilidadEspecial(definicion.Id))
            {
                continue;
            }

            if (definicion.EsZonal)
            {
                candidatosZonales.Add(definicion.Id);
            }
            else
            {
                candidatosGenericos.Add(definicion.Id);
            }
        }

        List<int> candidatos = SeleccionarPool(candidatosGenericos, candidatosZonales, origen);
        if (candidatos == null || candidatos.Count == 0)
        {
            idEvento = 0;
            return false;
        }

        idEvento = candidatos[Random.Range(0, candidatos.Count)];
        return true;
    }

    static bool CumpleDisponibilidadEspecial(int idEvento)
    {
        CampaignManager manager = CampaignManager.Instance;
        if (manager == null)
        {
            return true;
        }

        switch (idEvento)
        {
            case IdsEventoCampania.ComercianteVisitante:
                return manager.scSequitoMercaderes != null;

            case IdsEventoCampania.RefuerzoEnElCamino:
                return manager.TieneCapacidadDisponibleParaHeroe();

            case IdsEventoCampania.MensajeDesdeSerria:
                return !manager.HayMisionSalvamentoActivaEnMapa();

            default:
                return true;
        }
    }

    static List<int> SeleccionarPool(
        List<int> candidatosGenericos,
        List<int> candidatosZonales,
        TipoOrigenEventoCampania origen)
    {
        bool hayGenericos = candidatosGenericos.Count > 0;
        bool hayZonales = candidatosZonales.Count > 0;

        if (!hayGenericos && !hayZonales)
        {
            return null;
        }

        if (!hayGenericos)
        {
            return candidatosZonales;
        }

        if (!hayZonales)
        {
            return candidatosGenericos;
        }

        float chanceZonal = origen == TipoOrigenEventoCampania.Descanso
            ? ChanceEventoZonalDescanso
            : ChanceEventoZonalNodo;

        return Random.Range(0f, 100f) < chanceZonal
            ? candidatosZonales
            : candidatosGenericos;
    }
}
