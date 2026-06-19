using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveGameService
{
  private const string DefaultFileName = "campaign_save.json";

  private static SaveFileData pendingLoadData;
  private static string pendingLoadFailureMessage;

  public static string GetDefaultSavePath()
  {
    return Path.Combine(Application.persistentDataPath, DefaultFileName);
  }

  public static bool HasPendingLoad()
  {
    return pendingLoadData != null;
  }

  public static void QueuePendingLoad(SaveFileData saveFileData)
  {
    pendingLoadData = saveFileData;
  }

  public static void ClearPendingLoad()
  {
    pendingLoadData = null;
  }

  public static void ReportPendingLoadFailure(string message)
  {
    pendingLoadFailureMessage = message;
  }

  public static bool TryConsumePendingLoadFailure(out string message)
  {
    message = pendingLoadFailureMessage;
    pendingLoadFailureMessage = null;
    return !string.IsNullOrWhiteSpace(message);
  }

  public static bool TryConsumePendingLoad(out SaveFileData saveFileData)
  {
    saveFileData = pendingLoadData;
    pendingLoadData = null;
    return saveFileData != null;
  }

  public static bool HasSaveFile(string path = null)
  {
    string resolvedPath = ResolvePath(path);
    return File.Exists(resolvedPath);
  }

  public static bool TryWriteSaveFile(SaveFileData saveFileData, out string error, string path = null)
  {
    error = string.Empty;
    if (saveFileData == null)
    {
      error = "SaveFileData nulo.";
      return false;
    }

    try
    {
      saveFileData.MarcarGuardadoAhora();
      string resolvedPath = ResolvePath(path);
      string directory = Path.GetDirectoryName(resolvedPath);
      if (!string.IsNullOrWhiteSpace(directory))
      {
        Directory.CreateDirectory(directory);
      }

      string json = JsonUtility.ToJson(saveFileData, true);
      File.WriteAllText(resolvedPath, json);
      return true;
    }
    catch (Exception ex)
    {
      error = ex.Message;
      return false;
    }
  }

  public static bool TryReadSaveFile(out SaveFileData saveFileData, out string error, string path = null)
  {
    saveFileData = null;
    error = string.Empty;

    try
    {
      string resolvedPath = ResolvePath(path);
      if (!File.Exists(resolvedPath))
      {
        error = "No existe el archivo de guardado.";
        return false;
      }

      string json = File.ReadAllText(resolvedPath);
      if (string.IsNullOrWhiteSpace(json))
      {
        error = "El archivo de guardado esta vacio.";
        return false;
      }

      saveFileData = JsonUtility.FromJson<SaveFileData>(json);
      if (saveFileData == null)
      {
        error = "No se pudo deserializar el archivo de guardado.";
        return false;
      }
      if (saveFileData.version < SaveFileData.MinimumCompatibleVersion)
      {
        error = "El archivo de guardado pertenece a una version anterior incompatible.";
        saveFileData = null;
        return false;
      }
      if (saveFileData.version > SaveFileData.CurrentVersion)
      {
        error = "El archivo de guardado pertenece a una version posterior incompatible.";
        saveFileData = null;
        return false;
      }

      Normalizar(saveFileData);
      return true;
    }
    catch (Exception ex)
    {
      error = ex.Message;
      return false;
    }
  }

  public static bool TryQueuePendingLoadFromFile(out string error, string path = null)
  {
    error = string.Empty;
    if (!TryReadSaveFile(out SaveFileData saveFileData, out error, path))
    {
      return false;
    }

    QueuePendingLoad(saveFileData);
    return true;
  }

  public static bool DeleteSaveFile(out string error, string path = null)
  {
    error = string.Empty;

    try
    {
      string resolvedPath = ResolvePath(path);
      if (!File.Exists(resolvedPath))
      {
        return true;
      }

      File.Delete(resolvedPath);
      return true;
    }
    catch (Exception ex)
    {
      error = ex.Message;
      return false;
    }
  }

  private static string ResolvePath(string path)
  {
    return string.IsNullOrWhiteSpace(path) ? GetDefaultSavePath() : path;
  }

  private static void Normalizar(SaveFileData saveFileData)
  {
    if (saveFileData.campaign == null)
    {
      saveFileData.campaign = new CampaignSaveData();
    }
    if (saveFileData.campaign.estadosCaravana == null)
    {
      saveFileData.campaign.estadosCaravana = new EstadosCaravanaSaveData();
    }
    if (saveFileData.campaign.tutorialTooltipsVistos == null)
    {
      saveFileData.campaign.tutorialTooltipsVistos = new List<string>();
    }
    if (saveFileData.campaign.zonasEstado == null)
    {
      saveFileData.campaign.zonasEstado = new List<int>();
    }
    if (saveFileData.campaign.nodoActual == null)
    {
      saveFileData.campaign.nodoActual = new NodeReferenceSaveData();
    }
    if (saveFileData.campaign.nodoDestinoActual == null)
    {
      saveFileData.campaign.nodoDestinoActual = new NodeReferenceSaveData();
    }
    if (saveFileData.campaign.eventosAleatoriosUsadosMapa == null)
    {
      saveFileData.campaign.eventosAleatoriosUsadosMapa = new List<int>();
    }
    if (saveFileData.campaign.bitacora == null)
    {
      saveFileData.campaign.bitacora = new BitacoraSaveData();
    }
    if (saveFileData.campaign.ultimasAparienciasPorClase == null)
    {
      saveFileData.campaign.ultimasAparienciasPorClase = new List<UltimaAparienciaClaseSaveData>();
    }
    NormalizarBitacora(saveFileData.campaign.bitacora);

    if (saveFileData.map == null)
    {
      saveFileData.map = new MapSaveData();
    }
    if (saveFileData.map.nodes == null)
    {
      saveFileData.map.nodes = new List<NodeSaveData>();
    }
    if (saveFileData.map.settlementsForzados == null)
    {
      saveFileData.map.settlementsForzados = new List<NodeReferenceSaveData>();
    }
    foreach (NodeSaveData node in saveFileData.map.nodes)
    {
      if (node != null && node.conexiones == null)
      {
        node.conexiones = new List<CaminoConexionSaveData>();
      }
    }

    if (saveFileData.party == null)
    {
      saveFileData.party = new PartySaveData();
    }
    if (saveFileData.party.characters == null)
    {
      saveFileData.party.characters = new List<CharacterSaveData>();
    }
    if (saveFileData.party.inventoryItemIds == null)
    {
      saveFileData.party.inventoryItemIds = new List<string>();
    }
    if (saveFileData.party.selectedBattleCharacterIds == null)
    {
      saveFileData.party.selectedBattleCharacterIds = new List<string>();
    }
    foreach (CharacterSaveData character in saveFileData.party.characters)
    {
      NormalizarPersonaje(character);
    }

    if (saveFileData.sequitos == null)
    {
      saveFileData.sequitos = new SequitosSaveData();
    }
    if (saveFileData.sequitos.sequitosActivos == null)
    {
      saveFileData.sequitos.sequitosActivos = new List<int>();
    }
    if (saveFileData.sequitos.mercaderesItemsVendidosIds == null)
    {
      saveFileData.sequitos.mercaderesItemsVendidosIds = new List<string>();
    }

    if (saveFileData.metaprogresion == null)
    {
      saveFileData.metaprogresion = new MetaprogresionSaveData();
    }
  }

  private static void NormalizarBitacora(BitacoraSaveData bitacora)
  {
    if (bitacora == null)
    {
      return;
    }

    if (bitacora.entradasCampania == null)
    {
      bitacora.entradasCampania = new List<BitacoraEntradaSaveData>();
    }
    if (bitacora.dias == null)
    {
      bitacora.dias = new List<BitacoraDiaSaveData>();
    }

    foreach (BitacoraDiaSaveData dia in bitacora.dias)
    {
      if (dia != null && dia.entradas == null)
      {
        dia.entradas = new List<BitacoraEntradaSaveData>();
      }
    }
  }

  private static void NormalizarPersonaje(CharacterSaveData character)
  {
    if (character == null)
    {
      return;
    }

    if (character.habilidades == null)
    {
      character.habilidades = Array.Empty<int>();
    }
    if (character.actividades == null)
    {
      character.actividades = Array.Empty<int>();
    }
    if (character.rasgos == null)
    {
      character.rasgos = Array.Empty<int>();
    }
    if (character.equipment == null)
    {
      character.equipment = new EquipmentSaveData();
    }
  }
}
