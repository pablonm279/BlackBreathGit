using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class SaveGameService
{
  private const string DefaultFileName = "campaign_save.json";
  private const string TempFileSuffix = ".tmp";
  private const string BackupFileSuffix = ".bak";
  private const string RecoveryFileSuffix = ".recovery";

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
    return TryReadSaveFile(out _, out _, path);
  }

  public static bool TryWriteSaveFile(SaveFileData saveFileData, out string error, string path = null)
  {
    error = string.Empty;
    if (saveFileData == null)
    {
      error = "SaveFileData nulo.";
      return false;
    }

    string tempPath = string.Empty;
    try
    {
      saveFileData.MarcarGuardadoAhora();
      string resolvedPath = ResolvePath(path);
      tempPath = GetTempPath(resolvedPath);
      string directory = Path.GetDirectoryName(resolvedPath);
      if (!string.IsNullOrWhiteSpace(directory))
      {
        Directory.CreateDirectory(directory);
      }

      string json = JsonUtility.ToJson(saveFileData, true);
      File.WriteAllText(tempPath, json, new UTF8Encoding(false));
      CommitTempFile(tempPath, resolvedPath);
      return true;
    }
    catch (Exception ex)
    {
      error = ex.Message;
      return false;
    }
    finally
    {
      TryDeleteTemporaryFile(tempPath);
    }
  }

  public static bool TryReadSaveFile(out SaveFileData saveFileData, out string error, string path = null)
  {
    saveFileData = null;
    error = string.Empty;

    string resolvedPath = ResolvePath(path);
    if (TryReadSaveFileAtPath(resolvedPath, out saveFileData, out string primaryError))
    {
      return true;
    }

    string backupPath = GetBackupPath(resolvedPath);
    if (TryReadSaveFileAtPath(backupPath, out saveFileData, out string backupError))
    {
      RestorePrimaryFromRecoveryFile(backupPath, resolvedPath);
      Debug.LogWarning("[SaveGame] El guardado principal no era valido. Se recupero la copia de seguridad.");
      return true;
    }

    string tempPath = GetTempPath(resolvedPath);
    if (TryReadSaveFileAtPath(tempPath, out saveFileData, out string tempError))
    {
      RestorePrimaryFromRecoveryFile(tempPath, resolvedPath);
      TryDeleteTemporaryFile(tempPath);
      Debug.LogWarning("[SaveGame] Se recupero un guardado temporal que habia quedado sin confirmar.");
      return true;
    }

    error = primaryError;
    if (File.Exists(backupPath))
    {
      error += " Copia de seguridad: " + backupError;
    }
    if (File.Exists(tempPath))
    {
      error += " Archivo temporal: " + tempError;
    }
    return false;
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
      string[] paths =
      {
        resolvedPath,
        GetTempPath(resolvedPath),
        GetBackupPath(resolvedPath),
        GetRecoveryPath(resolvedPath)
      };

      foreach (string filePath in paths)
      {
        DeleteFileIfExists(filePath);
      }
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

  private static string GetTempPath(string resolvedPath)
  {
    return resolvedPath + TempFileSuffix;
  }

  private static string GetBackupPath(string resolvedPath)
  {
    return resolvedPath + BackupFileSuffix;
  }

  private static string GetRecoveryPath(string resolvedPath)
  {
    return resolvedPath + RecoveryFileSuffix;
  }

  private static void CommitTempFile(string tempPath, string resolvedPath)
  {
    string backupPath = GetBackupPath(resolvedPath);
    if (!File.Exists(resolvedPath))
    {
      File.Copy(tempPath, backupPath, true);
      File.Move(tempPath, resolvedPath);
      return;
    }

    try
    {
      File.Replace(tempPath, resolvedPath, backupPath, true);
    }
    catch (PlatformNotSupportedException)
    {
      CommitTempFileFallback(tempPath, resolvedPath, backupPath);
    }
    catch (NotSupportedException)
    {
      CommitTempFileFallback(tempPath, resolvedPath, backupPath);
    }
  }

  private static void CommitTempFileFallback(string tempPath, string resolvedPath, string backupPath)
  {
    File.Copy(resolvedPath, backupPath, true);
    File.Copy(tempPath, resolvedPath, true);
  }

  private static bool TryReadSaveFileAtPath(string filePath, out SaveFileData saveFileData, out string error)
  {
    saveFileData = null;
    error = string.Empty;

    try
    {
      if (!File.Exists(filePath))
      {
        error = "No existe el archivo de guardado.";
        return false;
      }

      string json = File.ReadAllText(filePath);
      if (string.IsNullOrWhiteSpace(json))
      {
        error = "El archivo de guardado esta vacio.";
        return false;
      }
      if (!HasRequiredSaveStructure(json))
      {
        error = "El archivo de guardado no contiene todos los bloques requeridos.";
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
      saveFileData = null;
      return false;
    }
  }

  private static bool HasRequiredSaveStructure(string json)
  {
    return json.IndexOf("\"version\"", StringComparison.Ordinal) >= 0
      && json.IndexOf("\"campaign\"", StringComparison.Ordinal) >= 0
      && json.IndexOf("\"map\"", StringComparison.Ordinal) >= 0
      && json.IndexOf("\"party\"", StringComparison.Ordinal) >= 0
      && json.IndexOf("\"sequitos\"", StringComparison.Ordinal) >= 0;
  }

  private static void RestorePrimaryFromRecoveryFile(string recoverySourcePath, string resolvedPath)
  {
    string recoveryPath = GetRecoveryPath(resolvedPath);
    try
    {
      File.Copy(recoverySourcePath, recoveryPath, true);
      DeleteFileIfExists(resolvedPath);
      File.Move(recoveryPath, resolvedPath);
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[SaveGame] Se cargo la copia de seguridad, pero no se pudo restaurar el archivo principal. " + ex.Message);
    }
    finally
    {
      TryDeleteTemporaryFile(recoveryPath);
    }
  }

  private static void DeleteFileIfExists(string filePath)
  {
    if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
    {
      File.Delete(filePath);
    }
  }

  private static void TryDeleteTemporaryFile(string filePath)
  {
    try
    {
      DeleteFileIfExists(filePath);
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[SaveGame] No se pudo limpiar un archivo temporal. " + ex.Message);
    }
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
