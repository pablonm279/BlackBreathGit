using UnityEngine;

public sealed class LinuxDeckRuntime : MonoBehaviour
{
  private const string PrefVsync = "gfx_vsync";
  private const string PrefFpsLimit = "gfx_fps_limit";
  private const string PrefCalidad = "graficos_index";
  private const string PrefSonidoSegundoPlano = "Background_Sound";

  private static LinuxDeckRuntime instancia;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void CrearAntesDeCargarEscena()
  {
    if (!PlataformaRuntime.EsLinuxPlayer && !PlataformaRuntime.EsSteamDeck)
    {
      return;
    }

    AplicarPerfilInicialSteamDeck();

    if (instancia != null)
    {
      return;
    }

    GameObject go = new GameObject("LinuxDeckRuntime");
    instancia = go.AddComponent<LinuxDeckRuntime>();
    DontDestroyOnLoad(go);
  }

  public static int ResolverVSyncCount(bool vsyncHabilitado)
  {
    if (!vsyncHabilitado)
    {
      return 0;
    }

    return PlataformaRuntime.UsaPantallaDeckDeAltaFrecuencia ? 2 : 1;
  }

  private static void AplicarPerfilInicialSteamDeck()
  {
    if (!PlataformaRuntime.EsSteamDeck)
    {
      return;
    }

    bool huboCambios = false;
    if (!PlayerPrefs.HasKey(PrefVsync))
    {
      PlayerPrefs.SetInt(PrefVsync, 1);
      huboCambios = true;
    }

    if (!PlayerPrefs.HasKey(PrefFpsLimit))
    {
      PlayerPrefs.SetInt(PrefFpsLimit, 60);
      huboCambios = true;
    }

    if (!PlayerPrefs.HasKey(PrefCalidad) && QualitySettings.names.Length > 1)
    {
      const int calidadMedia = 1;
      QualitySettings.SetQualityLevel(calidadMedia, true);
      PlayerPrefs.SetInt(PrefCalidad, calidadMedia);
      huboCambios = true;
    }

    if (!PlayerPrefs.HasKey(PrefSonidoSegundoPlano))
    {
      PlayerPrefs.SetInt(PrefSonidoSegundoPlano, 0);
      huboCambios = true;
    }

    Application.runInBackground = PlayerPrefs.GetInt(PrefSonidoSegundoPlano, 0) == 1;

    if (huboCambios)
    {
      PlayerPrefs.Save();
    }
  }

  private void Awake()
  {
    if (instancia != null && instancia != this)
    {
      Destroy(gameObject);
      return;
    }

    instancia = this;
    DontDestroyOnLoad(gameObject);
  }

  private void Start()
  {
    Resolution resolucion = Screen.currentResolution;
    Debug.Log(
      $"[Plataforma] plataforma={Application.platform}; SO={SystemInfo.operatingSystem}; "
      + $"modelo={SystemInfo.deviceModel}; SteamDeck={PlataformaRuntime.EsSteamDeck}; "
      + $"GPU={SystemInfo.graphicsDeviceName}; API={SystemInfo.graphicsDeviceType}; "
      + $"pantalla={Screen.width}x{Screen.height}@{PlataformaRuntime.FrecuenciaPantallaHz:0.##}Hz "
      + $"(nativa {resolucion.width}x{resolucion.height}); calidad={QualitySettings.GetQualityLevel()}; "
      + $"vSync={QualitySettings.vSyncCount}; FPSObjetivo={Application.targetFrameRate}; "
      + $"segundoPlano={Application.runInBackground}.");
  }

  private void OnApplicationFocus(bool tieneFoco)
  {
    if (!tieneFoco)
    {
      PlayerPrefs.Save();
    }
  }

  private void OnApplicationPause(bool pausada)
  {
    if (!pausada)
    {
      return;
    }

    PlayerPrefs.Save();

    CampaignManager campaignManager = CampaignManager.Instance;
    if (campaignManager != null && campaignManager.PuedeGuardarCampania(out _))
    {
      campaignManager.TryAutosaveCampania("suspension Linux/Steam Deck", out _);
    }
  }

  private void OnApplicationQuit()
  {
    PlayerPrefs.Save();
  }
}
