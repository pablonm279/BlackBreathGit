using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Catalogo de slots de guardado.
//   Slot 0   = Autoguardado (archivo historico campaign_save.json: lo escriben los autosaves y
//              sigue siendo compatible con las partidas ya existentes).
//   Slot 1-N = Slots manuales (campaign_save_slotN.json).
// Cada slot tiene su propio .tmp/.bak/.recovery/.corrupt (ver SaveGameService) y una captura
// de pantalla de la campaña tomada al guardar (<slot>.preview.jpg).
public static class SaveSlots
{
  public const int AutoSlot = 0;
  public const int ManualSlotCount = 5;
  public const int TotalSlots = ManualSlotCount + 1;

  private const string ActiveSlotPrefKey = "save_slot_activo";
  private const string PreviewSuffix = ".preview.jpg";

  public sealed class SlotInfo
  {
    public int Slot;
    public string Path;
    public bool Exists;
    public bool Corrupt;
    public string Error;
    public string DisplayName;
    public DateTime SavedLocal;
    public double HorasTotales;
    public int PartySize;
    public bool HasPreview;
    public bool IsAuto { get { return Slot == AutoSlot; } }
    public bool Loadable { get { return Exists && !Corrupt; } }
  }

  private struct CacheEntry
  {
    public DateTime LastWriteUtc;
    public long Length;
    public long PreviewTicks;
    public SlotInfo Info;
  }

  private static readonly Dictionary<int, CacheEntry> cache = new Dictionary<int, CacheEntry>();

  public static bool IsValidSlot(int slot)
  {
    return slot >= 0 && slot < TotalSlots;
  }

  public static string GetSlotPath(int slot)
  {
    if (slot <= AutoSlot)
    {
      return SaveGameService.GetDefaultSavePath();
    }

    return System.IO.Path.Combine(Application.persistentDataPath, "campaign_save_slot" + slot + ".json");
  }

  public static string GetPreviewPath(int slot)
  {
    return System.IO.Path.ChangeExtension(GetSlotPath(slot), PreviewSuffix);
  }

  // Ultimo slot manual usado para guardar o cargar. Es el destino del guardado rapido (F5).
  public static int ActiveManualSlot
  {
    get
    {
      int slot = PlayerPrefs.GetInt(ActiveSlotPrefKey, 1);
      return slot >= 1 && slot <= ManualSlotCount ? slot : 1;
    }
    set
    {
      if (value < 1 || value > ManualSlotCount) { return; }
      PlayerPrefs.SetInt(ActiveSlotPrefKey, value);
      PlayerPrefs.Save();
    }
  }

  public static SlotInfo GetInfo(int slot)
  {
    string path = GetSlotPath(slot);
    string previewPath = GetPreviewPath(slot);
    bool hayArchivos = SaveGameService.TryGetLastWriteTimeUtc(out DateTime lastWrite, out long length, path);
    long previewTicks = 0;
    try
    {
      if (File.Exists(previewPath)) { previewTicks = new FileInfo(previewPath).LastWriteTimeUtc.Ticks; }
    }
    catch (Exception) { }

    if (cache.TryGetValue(slot, out CacheEntry entry)
      && hayArchivos
      && entry.LastWriteUtc == lastWrite
      && entry.Length == length
      && entry.PreviewTicks == previewTicks)
    {
      return entry.Info;
    }

    SlotInfo info = new SlotInfo { Slot = slot, Path = path, Error = string.Empty, DisplayName = string.Empty };
    info.HasPreview = previewTicks != 0;

    if (!hayArchivos)
    {
      info.Exists = false;
      cache.Remove(slot);
      return info;
    }

    info.Exists = true;
    if (SaveGameService.TryReadSaveFile(out SaveFileData save, out string error, path))
    {
      info.DisplayName = save.displayName;
      info.SavedLocal = ParseLocal(save.savedAtUtc, lastWrite);
      info.HorasTotales = save.campaign != null ? save.campaign.horasTotales : 0d;
      info.PartySize = save.party != null && save.party.characters != null ? save.party.characters.Count : 0;
    }
    else
    {
      info.Corrupt = true;
      info.Error = error;
      info.SavedLocal = lastWrite.ToLocalTime();
    }

    // TryReadSaveFile puede haber recuperado/reescrito el principal desde la copia: se vuelve a leer la marca.
    SaveGameService.TryGetLastWriteTimeUtc(out lastWrite, out length, path);
    cache[slot] = new CacheEntry { LastWriteUtc = lastWrite, Length = length, PreviewTicks = previewTicks, Info = info };
    return info;
  }

  public static void InvalidateCache(int slot)
  {
    cache.Remove(slot);
  }

  public static List<SlotInfo> GetAllInfos()
  {
    List<SlotInfo> lista = new List<SlotInfo>(TotalSlots);
    for (int i = 0; i < TotalSlots; i++)
    {
      lista.Add(GetInfo(i));
    }
    return lista;
  }

  // Slot cargable con el guardado mas reciente (para "Continuar"). -1 si no hay ninguno.
  public static int GetMostRecentLoadableSlot()
  {
    int mejor = -1;
    DateTime mejorFecha = DateTime.MinValue;
    for (int i = 0; i < TotalSlots; i++)
    {
      SlotInfo info = GetInfo(i);
      if (!info.Loadable) { continue; }
      if (mejor < 0 || info.SavedLocal > mejorFecha)
      {
        mejor = i;
        mejorFecha = info.SavedLocal;
      }
    }
    return mejor;
  }

  public static bool HasAnyLoadableSlot()
  {
    return GetMostRecentLoadableSlot() >= 0;
  }

  public static bool TryDeleteSlot(int slot, out string error)
  {
    error = string.Empty;
    if (!IsValidSlot(slot))
    {
      error = "Slot invalido.";
      return false;
    }

    if (!SaveGameService.DeleteSaveFile(out error, GetSlotPath(slot)))
    {
      return false;
    }

    try
    {
      string previewPath = GetPreviewPath(slot);
      if (File.Exists(previewPath)) { File.Delete(previewPath); }
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[SaveSlots] No se pudo borrar la vista previa del slot " + slot + ". " + ex.Message);
    }

    InvalidateCache(slot);
    return true;
  }

  // Guarda la vista previa de forma atomica (temporal + reemplazo) para no dejar jpg a medias.
  public static bool TryWritePreview(int slot, byte[] jpgBytes)
  {
    if (!IsValidSlot(slot) || jpgBytes == null || jpgBytes.Length == 0) { return false; }

    string previewPath = GetPreviewPath(slot);
    string tempPath = previewPath + ".tmp";
    try
    {
      File.WriteAllBytes(tempPath, jpgBytes);
      if (File.Exists(previewPath)) { File.Delete(previewPath); }
      File.Move(tempPath, previewPath);
      InvalidateCache(slot);
      return true;
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[SaveSlots] No se pudo guardar la vista previa del slot " + slot + ". " + ex.Message);
      try { if (File.Exists(tempPath)) { File.Delete(tempPath); } } catch (Exception) { }
      return false;
    }
  }

  // Devuelve una textura nueva (el que llama es responsable de destruirla) o null.
  public static Texture2D LoadPreviewTexture(int slot)
  {
    try
    {
      string previewPath = GetPreviewPath(slot);
      if (!File.Exists(previewPath)) { return null; }

      Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
      if (!texture.LoadImage(File.ReadAllBytes(previewPath), true))
      {
        UnityEngine.Object.Destroy(texture);
        return null;
      }

      texture.wrapMode = TextureWrapMode.Clamp;
      return texture;
    }
    catch (Exception ex)
    {
      Debug.LogWarning("[SaveSlots] No se pudo leer la vista previa del slot " + slot + ". " + ex.Message);
      return null;
    }
  }

  private static DateTime ParseLocal(string savedAtUtc, DateTime fallbackUtc)
  {
    if (DateTime.TryParse(savedAtUtc, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime fecha))
    {
      return fecha.Kind == DateTimeKind.Utc ? fecha.ToLocalTime() : fecha;
    }

    return fallbackUtc.ToLocalTime();
  }
}
