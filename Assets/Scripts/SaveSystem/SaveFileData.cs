using System;
using System.Collections.Generic;

[Serializable]
public class SaveFileData
{
  public const int CurrentVersion = 7;

  public int version = CurrentVersion;
  public string savedAtUtc;
  public CampaignSaveData campaign = new CampaignSaveData();
  public MapSaveData map = new MapSaveData();
  public PartySaveData party = new PartySaveData();
  public SequitosSaveData sequitos = new SequitosSaveData();
  public MetaprogresionSaveData metaprogresion = new MetaprogresionSaveData();

  public void MarcarGuardadoAhora()
  {
    version = CurrentVersion;
    savedAtUtc = DateTime.UtcNow.ToString("o");
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
  public List<int> zonasEstado = new List<int>();

  public NodeReferenceSaveData nodoActual = new NodeReferenceSaveData();
  public NodeReferenceSaveData nodoDestinoActual = new NodeReferenceSaveData();
  public bool settlementOpen;
  public int settlementActionsRemaining = 3;
}

[Serializable]
public class MapSaveData
{
  public List<NodeSaveData> nodes = new List<NodeSaveData>();
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
  public int costoMovimiento;
  public bool revelado;
  public bool yatiroConexiones;
  public bool nodoIncendiado;
  public bool nodoRitual;
  public int visualCode;
  public bool esMisterioso;
  public bool atajoSubterraneoPendiente;
  public List<NodeReferenceSaveData> destinos = new List<NodeReferenceSaveData>();
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

  public int[] habilidades = new int[10];
  public int[] actividades = new int[3];
  public int actividadSeleccionada;

  public int nivelPuntoAtributo;
  public int nivelPuntoTS;
  public int nivelPuntoHabilidad;
  public int nivelNuevaHabilidadBase;

  public bool campFatigado;
  public bool campBendecidoSequitoClerigos;
  public bool campHerido;
  public int campEnfermo;
  public int campMoral;
  public bool campAvergonzado;
  public bool campMuerto;
  public bool campCorrupto;

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

  public List<string> mercaderesItemsVendidosIds = new List<string>();
}

[Serializable]
public class MetaprogresionSaveData
{
  public int corrupcionGlobal;
  public int cantidadCiviles;
  public int valorTrabajoDisponible;

  public int misionesSalvamento = -1;
  public int nivelPeligroBosqueArdiente = -1;
  public int nivelPeligroPasoVientohelado = -1;
  public int nivelPeligroNedukazal = -1;

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
