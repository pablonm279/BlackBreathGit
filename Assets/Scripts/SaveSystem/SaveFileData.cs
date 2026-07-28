using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Serialization;

[Serializable]
public class SaveFileData
{
  public const int MinimumCompatibleVersion = 1;
  public const int CurrentVersion = 23;

  public int version = CurrentVersion;
  public string savedAtUtc;
  public string displayName;
  public CampaignSaveData campaign = new CampaignSaveData();
  public MapSaveData map = new MapSaveData();
  public PartySaveData party = new PartySaveData();
  public SequitosSaveData sequitos = new SequitosSaveData();
  public MetaprogresionSaveData metaprogresion = new MetaprogresionSaveData();

  public void MarcarGuardadoAhora()
  {
    version = CurrentVersion;
    DateTime utcNow = DateTime.UtcNow;
    savedAtUtc = utcNow.ToString("o");
    displayName = ConstruirDisplayName(utcNow.ToLocalTime());
  }

  public void AsegurarDisplayName()
  {
    if (!string.IsNullOrWhiteSpace(displayName))
    {
      return;
    }

    displayName = ConstruirDisplayName(ObtenerFechaLocalGuardado());
  }

  private string ConstruirDisplayName(DateTime fechaLocal)
  {
    int fase = campaign != null ? Math.Max(1, campaign.zonaFase) : 1;
    int idioma = ObtenerIdiomaActual();
    string etapa = ObtenerEtiquetaEtapa(idioma);
    string region = ObtenerNombreRegion(campaign != null ? campaign.zonaId : 0, idioma);
    string fecha = fechaLocal.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

    return etapa + " " + ConvertirARomano(fase) + " - " + region + " - " + fecha;
  }

  private DateTime ObtenerFechaLocalGuardado()
  {
    if (DateTime.TryParse(savedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime fechaGuardada))
    {
      return fechaGuardada.Kind == DateTimeKind.Utc ? fechaGuardada.ToLocalTime() : fechaGuardada;
    }

    return DateTime.Now;
  }

  private static int ObtenerIdiomaActual()
  {
    return TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
  }

  private static string ObtenerEtiquetaEtapa(int idioma)
  {
    return idioma == TRADU.IdiomaIngles ? "Stage" : "Etapa";
  }

  private static string ObtenerNombreRegion(int zonaId, int idioma)
  {
    if (idioma == TRADU.IdiomaIngles)
    {
      return zonaId switch
      {
        IdsZonaCampania.BosqueAngustiante => "Burning Forest",
        IdsZonaCampania.PasoVientoHelado => "Frozenwind Passage",
        IdsZonaCampania.Nedukazal => "Nedukazal",
        _ => "Unknown Region"
      };
    }

    if (idioma == TRADU.IdiomaPortugues)
    {
      return zonaId switch
      {
        IdsZonaCampania.BosqueAngustiante => "Floresta Ardente",
        IdsZonaCampania.PasoVientoHelado => "Passagem do Vento Gelado",
        IdsZonaCampania.Nedukazal => "Nedukazal",
        _ => "Regiao desconhecida"
      };
    }

    return zonaId switch
    {
      IdsZonaCampania.BosqueAngustiante => "Bosque Ardiente",
      IdsZonaCampania.PasoVientoHelado => "Paso Vientohelado",
      IdsZonaCampania.Nedukazal => "Nedukazal",
      _ => "Region desconocida"
    };
  }

  private static string ConvertirARomano(int numero)
  {
    if (numero <= 0 || numero > 3999)
    {
      return numero.ToString(CultureInfo.InvariantCulture);
    }

    int[] valores = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
    string[] simbolos = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
    string resultado = string.Empty;

    for (int i = 0; i < valores.Length; i++)
    {
      while (numero >= valores[i])
      {
        resultado += simbolos[i];
        numero -= valores[i];
      }
    }

    return resultado;
  }
}

[Serializable]
public class CampaignSaveData
{
  public int zonaId;
  public int zonaFase;
  public int reliefSeed;
  public int pasoVientoHeladoFuerzaKaleTav;
  public int numeroTurno;
  public int posicionCaravana;
  public int tipoClima;
  public int presagiosRegionId;
  public List<int> presagiosActivos = new List<int>();
  public bool primeraBatallaPresagioEnemigosConsumida;

  public float alientoNegro;
  public int fatiga;
  public int esperanza;
  public int civiles;
  public int bueyes;
  public int suministros;
  public int materiales;
  public int oro;

  public int mejoraCaravanaAntorchas;
  public int mejoraCaravanaAlforjas;
  public int mejoraCaravanaTiendas;
  public int mejoraCaravanaCatalejos;
  public int mejoraCaravanaAlmacen;
  public int mejoraCaravanaDefensas;

  public int sequitoHerrerosMantArmas;
  public int sequitoHerrerosMantArmaduras;
  public int sequitoMercaderesTier;
  public float sequitoCuranderosMejoraCuracion;

  public int miliciasMejoras;
  public int peligroZonaAnterior;

  public int puestoComercialSuministrosDisp;
  public int puestoComercialMaterialesDisp;
  public int puestoComercialBueyesDisp;

  public bool tutorialActivo;
  public int tutorialPasoActual;
  public bool tutorialNuevoActivo;
  public string tutorialNuevoId;
  public string tutorialNuevoPasoId;
  public bool tutorialNuevoPendienteTrasDescripcionZona;
  public bool tutorialTooltipsSilenciados;
  public List<string> tutorialTooltipsVistos = new List<string>();
  public int estadisticaDiasViajados;
  public int estadisticaBatallasLibradas;
  public int estadisticaCivilesPerdidos;
  public int estadisticaAsentamientosVisitados;
  public List<int> zonasEstado = new List<int>();

  public NodeReferenceSaveData nodoActual = new NodeReferenceSaveData();
  public NodeReferenceSaveData nodoDestinoActual = new NodeReferenceSaveData();
  public int batallaEnCurso;
  public int emboscadaEnCurso;
  public List<int> eventosAleatoriosUsadosMapa = new List<int>();
  public EstadosCaravanaSaveData estadosCaravana = new EstadosCaravanaSaveData();
  public BitacoraSaveData bitacora = new BitacoraSaveData();
  public List<UltimaAparienciaClaseSaveData> ultimasAparienciasPorClase = new List<UltimaAparienciaClaseSaveData>();
  public bool settlementOpen;
  public int settlementActionsRemaining = 3;
}

[Serializable]
public class UltimaAparienciaClaseSaveData
{
  public int idClase;
  public int indiceAparienciaAlternativa = -1;
}

[Serializable]
public class BitacoraSaveData
{
  public int ultimoDiaRegistrado;
  public List<BitacoraEntradaSaveData> entradasCampania = new List<BitacoraEntradaSaveData>();
  public List<BitacoraDiaSaveData> dias = new List<BitacoraDiaSaveData>();
}

[Serializable]
public class BitacoraDiaSaveData
{
  public int dia;
  public int tipoClima;
  public bool tieneSnapshotRecursos;
  public int esperanzaInicial;
  public int oroInicial;
  public int materialesIniciales;
  public int suministrosIniciales;
  public List<BitacoraEntradaSaveData> entradas = new List<BitacoraEntradaSaveData>();
}

[Serializable]
public class BitacoraEntradaSaveData
{
  public string texto;
}

[Serializable]
public class MapSaveData
{
  public List<NodeSaveData> nodes = new List<NodeSaveData>();
  public List<NodeReferenceSaveData> settlementsForzados = new List<NodeReferenceSaveData>();
  public int emboscadasSubterraneasZona;
  public int viajesDesdeUltimaEmboscadaSubterranea = 99;
}

[Serializable]
public class NodeSaveData
{
  public int x;
  public int y;
  public bool activo;
  public int tipoNodo;
  public bool nodoDespejado;
  public bool revelado;
  public bool yatiroConexiones;
  public bool nodoIncendiado;
  public bool nodoRitual;
  public int tipoNodoOriginalRitual;
  public int visualCode;
  public bool esMisterioso;
  public bool atajoSubterraneoPendiente;
  public string faccionScoutReveladaId;
  public string faccionScoutReveladaNombre;
  public bool visibilidadForzadaEspecial;
  public bool reveladoPorZonaCartografiada;
  public List<CaminoConexionSaveData> conexiones = new List<CaminoConexionSaveData>();
}

[Serializable]
public class CaminoConexionSaveData
{
  public int destinoX;
  public int destinoY;
  public TipoCaminoCampania tipo;
  public int costoMovimiento = 1;
  public bool rutaHaciaAldea;
  public bool reveladoPorVision;
}

[Serializable]
public class NodeReferenceSaveData
{
  public int x = -1;
  public int y = -1;
}

[Serializable]
public class PartySaveData
{
  public List<CharacterSaveData> characters = new List<CharacterSaveData>();
  public List<string> inventoryItemIds = new List<string>();
  public List<string> selectedBattleCharacterIds = new List<string>();
}

[Serializable]
public class CharacterSaveData
{
  public string id;
  public string nombre;
  public int idClase;
  public int idRetrato;
  public int indiceAparienciaAlternativa = -1;
  public bool aparienciaAlternativaResuelta;
  public int puestoDeseado;

  public float vidaActual;
  public float vidaMaxima;
  public float experienciaActual;
  public float nivelActual;

  public int fuerza;
  public int agi;
  public int poder;
  public int iniciativa;
  public int apMax;
  public int valMax;
  public int armadura;
  public int defensa;
  public int tsReflejo;
  public int tsFortaleza;
  public int tsMental;
  public int resFuego;
  public int resRayo;
  public int resHielo;
  public int resArcano;
  public int resAcido;
  public int resNecro;
  public int resDivino;
  public float critRango;
  public float critDanio;
  public float bonusAtaque;
  public bool sinCooldownDebug;

  public int[] habilidades = new int[10];
  public int[] actividades = new int[3];
  public int actividadSeleccionada;
  public bool actividadFijada;

  public int nivelPuntoAtributo;
  public int nivelPuntoTS;
  public int nivelPuntoHabilidad;
  public int nivelNuevaHabilidadBase;

  public bool campFatigado;
  public bool campBendecidoSequitoClerigos;
  public int campBendecidoDias;
  public bool campHerido;
  public int campEnfermo;
  public int campMoral;
  public bool campAvergonzado;
  public bool campMuerto;
  public bool campCorrupto;
  public bool traitHeroeLocalCivilesOtorgados;
  public bool traitHeroeLocalPenalidadMuerteAplicada;
  public bool traitLiderCaravanaPenalidadMuerteAplicada;
  public bool traitEjemploASeguirAplicado;
  public bool traitHerenciaItemOtorgado;
  public int diasViajado;
  public int enemigosEliminados;
  public int danioHecho;
  public int danioRecibido;
  public int vecesDerribado;

  public int[] rasgos = Array.Empty<int>();
  public EquipmentSaveData equipment = new EquipmentSaveData();
}

[Serializable]
public class EquipmentSaveData
{
  public string armaItemId;
  public string armaduraItemId;
  public string accesorio1ItemId;
  public string accesorio2ItemId;
  public string consumible1ItemId;
  public string consumible2ItemId;
}

[Serializable]
public class SequitosSaveData
{
  public List<int> sequitosActivos = new List<int>();

  public int herboristasVecesEnClaro;
  public int herboristasCantBalsamoFort;
  public int herboristasCantBalsamoReflej;
  public int herboristasCantBalsamoMental;

  public int cronistasValorCambios;
  public bool cronistasYaVendio;

  public int clerigosZonaIdUltimaPlegaria = -1;

  public List<string> mercaderesItemsVendidosIds = new List<string>();
}

[Serializable]
public class MetaprogresionSaveData
{
  public int corrupcionGlobal;
  public int cantidadCiviles;
  public int valorTrabajoDisponible;
  public List<PresagioRegionPendienteSaveData> presagiosRegionesPendientes = new List<PresagioRegionPendienteSaveData>();
  public List<int> zonasVisitadas = new List<int>();
  public List<int> climasExclusivosDescubiertos = new List<int>();

  public int misionesSalvamento = -1;
  [FormerlySerializedAs("nivelPeligroBosqueArdiente")]
  public int nivelAlertaBosqueArdiente = -1;
  [FormerlySerializedAs("nivelPeligroPasoVientohelado")]
  public int nivelAlertaPasoVientohelado = -1;
  [FormerlySerializedAs("nivelPeligroNedukazal")]
  public int nivelAlertaNedukazal = -1;

  public int serriaTierBarcos = -1;
  public int serriaTierAlmenaras = -1;
  public int serriaTierPalacio = -1;
  public int serriaTierCuartel = -1;
  public int serriaTierGranjas = -1;
  public int serriaTierBarricadas = -1;
  public int serriaTierTemplo = -1;

  public int serriaPuntosAlmacenadosBarcos;
  public int serriaPuntosAlmacenadosAlmenaras;
  public int serriaPuntosAlmacenadosPalacio;
  public int serriaPuntosAlmacenadosCuartel;
  public int serriaPuntosAlmacenadosGranjas;
  public int serriaPuntosAlmacenadosBarricadas;
  public int serriaPuntosAlmacenadosTemplo;
}
