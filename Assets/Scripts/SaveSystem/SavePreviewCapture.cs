using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Toma la captura de pantalla que se muestra como vista previa de cada slot.
// Se ejecuta como corrutina (hay que esperar al final del frame para leer el framebuffer) y
// puede ocultar temporalmente overlays (por ejemplo el menu de opciones desde el que se guarda)
// para que la imagen muestre la campaña y no el menu.
public static class SavePreviewCapture
{
  private const int TargetWidth = 480;
  private const int JpgQuality = 80;

  private static readonly HashSet<int> capturasEnCurso = new HashSet<int>();

  public static bool PuedeCapturar()
  {
    return !Application.isBatchMode && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null;
  }

  public static IEnumerator CapturarYGuardar(int slot, GameObject[] ocultar)
  {
    if (!PuedeCapturar() || !SaveSlots.IsValidSlot(slot) || !capturasEnCurso.Add(slot))
    {
      yield break;
    }

    List<CanvasGroup> grupos = new List<CanvasGroup>();
    List<float> alfasOriginales = new List<float>();
    try
    {
      if (ocultar != null)
      {
        foreach (GameObject objeto in ocultar)
        {
          if (objeto == null || !objeto.activeInHierarchy) { continue; }
          CanvasGroup grupo = objeto.GetComponent<CanvasGroup>();
          if (grupo == null) { grupo = objeto.AddComponent<CanvasGroup>(); }
          grupos.Add(grupo);
          alfasOriginales.Add(grupo.alpha);
          grupo.alpha = 0f;
        }
      }

      yield return null;
      yield return new WaitForEndOfFrame();

      byte[] jpg = null;
      try
      {
        jpg = CapturarPantallaComoJpg();
      }
      catch (Exception ex)
      {
        Debug.LogWarning("[SavePreview] No se pudo capturar la pantalla. " + ex.Message);
      }

      RestaurarOverlays(grupos, alfasOriginales);
      grupos.Clear();

      if (jpg != null)
      {
        SaveSlots.TryWritePreview(slot, jpg);
      }
    }
    finally
    {
      RestaurarOverlays(grupos, alfasOriginales);
      capturasEnCurso.Remove(slot);
    }
  }

  private static void RestaurarOverlays(List<CanvasGroup> grupos, List<float> alfas)
  {
    for (int i = 0; i < grupos.Count; i++)
    {
      if (grupos[i] != null) { grupos[i].alpha = alfas[i]; }
    }
  }

  private static byte[] CapturarPantallaComoJpg()
  {
    Texture2D pantalla = ScreenCapture.CaptureScreenshotAsTexture();
    if (pantalla == null || pantalla.width < 2 || pantalla.height < 2)
    {
      if (pantalla != null) { UnityEngine.Object.Destroy(pantalla); }
      return null;
    }

    RenderTexture activaPrevia = RenderTexture.active;
    RenderTexture actual = null;
    Texture2D reducida = null;
    try
    {
      int destinoAncho = Mathf.Min(TargetWidth, pantalla.width);
      int destinoAlto = Mathf.Max(2, Mathf.RoundToInt(destinoAncho * (pantalla.height / (float)pantalla.width)));

      // Reduccion por pasos (mitad cada vez) para promediar bien y evitar el aliasing de un unico blit 4x.
      Texture origen = pantalla;
      int ancho = pantalla.width;
      int alto = pantalla.height;
      while (ancho / 2 >= destinoAncho && alto / 2 >= destinoAlto)
      {
        ancho /= 2;
        alto /= 2;
        RenderTexture paso = RenderTexture.GetTemporary(ancho, alto, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        paso.filterMode = FilterMode.Bilinear;
        Graphics.Blit(origen, paso);
        if (actual != null) { RenderTexture.ReleaseTemporary(actual); }
        actual = paso;
        origen = paso;
      }

      RenderTexture final = RenderTexture.GetTemporary(destinoAncho, destinoAlto, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
      final.filterMode = FilterMode.Bilinear;
      Graphics.Blit(origen, final);
      if (actual != null) { RenderTexture.ReleaseTemporary(actual); }
      actual = final;

      RenderTexture.active = actual;
      reducida = new Texture2D(destinoAncho, destinoAlto, TextureFormat.RGB24, false);
      reducida.ReadPixels(new Rect(0, 0, destinoAncho, destinoAlto), 0, 0);
      reducida.Apply(false);
      return reducida.EncodeToJPG(JpgQuality);
    }
    finally
    {
      RenderTexture.active = activaPrevia;
      if (actual != null) { RenderTexture.ReleaseTemporary(actual); }
      if (reducida != null) { UnityEngine.Object.Destroy(reducida); }
      UnityEngine.Object.Destroy(pantalla);
    }
  }
}
