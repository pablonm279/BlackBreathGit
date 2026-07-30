using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BattleAmbientLife : MonoBehaviour
{
  // Usar material existente en Resources evita diferencias Editor/Build por shader stripping.
  private const string RutaMaterialMotas = "Imagenes/polvilloruina";
  private const string RutaMaterialBruma = "Imagenes/polvilloruina";
  private const int OrdenRenderParticulasFrente = 4500;

  [Header("Fondo")]
  [SerializeField] private MeshRenderer fondoRenderer;
  [SerializeField] private bool animarUVFondo = true;
  [SerializeField] private float amplitudUVx = 0.00125f;
  [SerializeField] private float amplitudUVy = 0.00000f;
  [SerializeField] private float frecuenciaUV = 0.105f;
  [SerializeField] private float amplitudPulsoColor = 0.045f;
  [SerializeField] private float velocidadPulsoColor = 0.22f;

  [Header("Particulas Ambiente")]
  [SerializeField] private bool habilitarParticulas = true;
  [SerializeField] private int maxParticulasMotas = 81;
  [SerializeField] private int maxParticulasBruma = 20;
  [Header("Luces")]
  [SerializeField] private bool animarLucesPuntuales = true;
  [SerializeField] private float variacionIntensidadLuces = 0.075f;
  [SerializeField] private float velocidadLuces = 1.15f;

  private Material materialFondoInstancia;
  private Color colorBaseFondo = Color.white;
  private Vector2 uvBaseFondo;
  private Vector2 uvInicialFondo;
  private float fasePulso;

  private GameObject goAmbientRoot;
  private ParticleSystem psMotas;
  private ParticleSystem psBruma;
  private Material materialFallbackParticulas;
  private Texture2D texturaCirculoSuave;

  private EncounterZoneType zonaActual = EncounterZoneType.BosqueAngustiante;
  private bool subterraneoActual;
  private Bounds boundsCampo;
  private bool boundsValido;
  private readonly Dictionary<Light, float> intensidadBaseLuces = new Dictionary<Light, float>();
  private readonly Dictionary<Light, float> faseLuces = new Dictionary<Light, float>();

  public void Configurar(MeshRenderer fondo, EncounterZoneType zona, bool subterraneo)
  {
    fondoRenderer = fondo;
    zonaActual = zona;
    subterraneoActual = subterraneo;
    fasePulso = Random.Range(0f, Mathf.PI * 2f);

    InicializarFondo();
    RecalcularBoundsCampo();
    InicializarParticulas();
    InicializarLuces();
    AplicarPresetVisual();
  }

  void Update()
  {
    if (!EstaBatallaActiva())
    {
      RestaurarLuces();
      if (goAmbientRoot != null && goAmbientRoot.activeSelf)
      {
        goAmbientRoot.SetActive(false);
      }

      return;
    }

    if (goAmbientRoot != null && !goAmbientRoot.activeSelf)
    {
      goAmbientRoot.SetActive(true);
    }

    AnimarFondo();
    AnimarLuces();
    SincronizarAreaParticulas();
  }

  void OnDestroy()
  {
    RestaurarLuces();

    if (materialFallbackParticulas != null)
    {
      Destroy(materialFallbackParticulas);
      materialFallbackParticulas = null;
    }

    if (texturaCirculoSuave != null)
    {
      Destroy(texturaCirculoSuave);
      texturaCirculoSuave = null;
    }
  }

  private void InicializarLuces()
  {
    RestaurarLuces();
    intensidadBaseLuces.Clear();
    faseLuces.Clear();

    Light[] luces = GetComponentsInChildren<Light>(true);
    for (int i = 0; i < luces.Length; i++)
    {
      Light luz = luces[i];
      if (luz == null || luz.type != LightType.Point || luz.intensity <= 0f)
      {
        continue;
      }

      intensidadBaseLuces[luz] = luz.intensity;
      faseLuces[luz] = Random.Range(0f, Mathf.PI * 2f);
    }
  }

  private void AnimarLuces()
  {
    if (!animarLucesPuntuales || !BattleVisualJuice.Enabled)
    {
      RestaurarLuces();
      return;
    }

    float tiempo = Time.unscaledTime * Mathf.Max(0.01f, velocidadLuces);
    foreach (KeyValuePair<Light, float> par in intensidadBaseLuces)
    {
      Light luz = par.Key;
      if (luz == null)
      {
        continue;
      }

      float faseLuz = faseLuces.TryGetValue(luz, out float faseGuardada) ? faseGuardada : 0f;
      float ruidoLento = Mathf.Sin(tiempo + faseLuz);
      float ruidoRapido = Mathf.Sin(tiempo * 2.37f + faseLuz * 0.61f) * 0.32f;
      float variacion = (ruidoLento + ruidoRapido) * variacionIntensidadLuces;
      luz.intensity = par.Value * Mathf.Max(0.72f, 1f + variacion);
    }
  }

  private void RestaurarLuces()
  {
    foreach (KeyValuePair<Light, float> par in intensidadBaseLuces)
    {
      if (par.Key != null)
      {
        par.Key.intensity = par.Value;
      }
    }
  }

  private bool EstaBatallaActiva()
  {
    if (BattleManager.Instance == null)
    {
      return false;
    }

    if (CampaignManager.Instance == null || CampaignManager.Instance.scAdministradorEscenas == null)
    {
      return true;
    }

    return CampaignManager.Instance.scAdministradorEscenas.escenaActual == 1;
  }

  private void InicializarFondo()
  {
    materialFondoInstancia = null;
    colorBaseFondo = Color.white;
    uvBaseFondo = Vector2.zero;
    uvInicialFondo = Vector2.zero;

    if (fondoRenderer == null)
    {
      return;
    }

    materialFondoInstancia = fondoRenderer.material;
    if (materialFondoInstancia == null)
    {
      return;
    }

    if (materialFondoInstancia.HasProperty("_MainTex"))
    {
      uvInicialFondo = materialFondoInstancia.mainTextureOffset;
      uvBaseFondo = uvInicialFondo;
    }

    if (materialFondoInstancia.HasProperty("_Color"))
    {
      colorBaseFondo = materialFondoInstancia.color;
    }
  }

  private void AnimarFondo()
  {
    if (materialFondoInstancia == null)
    {
      return;
    }

    float t = Time.time;

    if (animarUVFondo && materialFondoInstancia.HasProperty("_MainTex"))
    {
      // Oscilacion minima y acotada para evitar desplazamiento visible hacia margenes.
      float fx = Mathf.Max(0.01f, frecuenciaUV);
      uvBaseFondo.x = uvInicialFondo.x + Mathf.Sin(t * fx + fasePulso) * amplitudUVx;
      uvBaseFondo.y = uvInicialFondo.y + Mathf.Sin(t * fx * 0.79f + fasePulso * 0.73f) * amplitudUVy;
      materialFondoInstancia.mainTextureOffset = uvBaseFondo;
    }

    if (materialFondoInstancia.HasProperty("_Color"))
    {
      float pulso = 1f + Mathf.Sin(t * velocidadPulsoColor + fasePulso) * amplitudPulsoColor;
      Color c = colorBaseFondo * pulso;
      c.a = colorBaseFondo.a;
      materialFondoInstancia.color = c;
    }
  }

  private void RecalcularBoundsCampo()
  {
    boundsValido = false;
    List<Casilla> casillas = BattleManager.Instance != null ? BattleManager.Instance.lCasillasTotal : null;
    if (casillas == null || casillas.Count < 1)
    {
      return;
    }

    bool inicio = false;
    Bounds b = new Bounds(Vector3.zero, Vector3.zero);
    for (int i = 0; i < casillas.Count; i++)
    {
      Casilla c = casillas[i];
      if (c == null) { continue; }

      if (!inicio)
      {
        b = new Bounds(c.transform.position, Vector3.zero);
        inicio = true;
      }
      else
      {
        b.Encapsulate(c.transform.position);
      }
    }

    if (!inicio)
    {
      return;
    }

    b.Expand(new Vector3(1.8f, 1.4f, 1.8f));
    boundsCampo = b;
    boundsValido = true;
  }

  private void InicializarParticulas()
  {
    if (!habilitarParticulas)
    {
      return;
    }

    if (goAmbientRoot == null)
    {
      goAmbientRoot = new GameObject("BattleAmbientLifeRoot");
      goAmbientRoot.transform.SetParent(transform, false);
    }

    if (psMotas == null)
    {
      psMotas = CrearSistemaParticulas("AmbientMotas", RutaMaterialMotas);
    }
    if (psBruma == null)
    {
      psBruma = CrearSistemaParticulas("AmbientBruma", RutaMaterialBruma);
    }

    if (psMotas == null || psBruma == null)
    {
      return;
    }

    Color colorPrincipal = ObtenerColorZona();
    ConfigurarSistemaMotas(psMotas, colorPrincipal);
    ConfigurarSistemaBruma(psBruma, colorPrincipal);
    SincronizarAreaParticulas();
  }

  private ParticleSystem CrearSistemaParticulas(string nombre, string rutaMaterialPreferida)
  {
    GameObject go = new GameObject(nombre);
    go.transform.SetParent(goAmbientRoot.transform, false);
    ParticleSystem ps = go.AddComponent<ParticleSystem>();
    var renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    renderer.receiveShadows = false;
    renderer.sortMode = ParticleSystemSortMode.Distance;
    renderer.renderMode = ParticleSystemRenderMode.Billboard;
    renderer.alignment = ParticleSystemRenderSpace.View;
    renderer.minParticleSize = 0.000765f;
    renderer.maxParticleSize = 0.026775f;
    renderer.sortingLayerName = "Default";
    renderer.sortingOrder = OrdenRenderParticulasFrente;
    renderer.sortingFudge = -80f;
    Material materialParticulas = ObtenerMaterialParticulas(rutaMaterialPreferida);
    if (materialParticulas != null)
    {
      renderer.sharedMaterial = materialParticulas;
    }
    return ps;
  }

  private Material ObtenerMaterialParticulas(string rutaMaterialPreferida)
  {
    if (!string.IsNullOrWhiteSpace(rutaMaterialPreferida))
    {
      Material matDesdeResources = Resources.Load<Material>(rutaMaterialPreferida);
      if (matDesdeResources != null)
      {
        if (materialFallbackParticulas == null
          || materialFallbackParticulas.shader != matDesdeResources.shader
          || materialFallbackParticulas.mainTexture != matDesdeResources.mainTexture)
        {
          if (materialFallbackParticulas != null)
          {
            Destroy(materialFallbackParticulas);
          }

          materialFallbackParticulas = new Material(matDesdeResources);
          materialFallbackParticulas.name = "BattleAmbientLife_RuntimeParticles";
        }

        if (materialFallbackParticulas.HasProperty("_Color"))
        {
          materialFallbackParticulas.color = Color.white;
        }
        if (materialFallbackParticulas.HasProperty("_ZWrite"))
        {
          materialFallbackParticulas.SetInt("_ZWrite", 0);
        }
        if (materialFallbackParticulas.HasProperty("_ZTest"))
        {
          materialFallbackParticulas.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
        materialFallbackParticulas.renderQueue = 3900;
        return materialFallbackParticulas;
      }
    }

    Shader shaderFallback = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
    if (shaderFallback == null)
    {
      shaderFallback = Shader.Find("Particles/Standard Unlit");
    }
    if (shaderFallback == null)
    {
      shaderFallback = Shader.Find("Sprites/Default");
    }
    if (shaderFallback == null)
    {
      return null;
    }

    if (materialFallbackParticulas == null || materialFallbackParticulas.shader != shaderFallback)
    {
      if (materialFallbackParticulas != null)
      {
        Destroy(materialFallbackParticulas);
      }

      materialFallbackParticulas = new Material(shaderFallback);
      materialFallbackParticulas.name = "BattleAmbientLife_FallbackParticles";

      if (materialFallbackParticulas.HasProperty("_MainTex"))
      {
        materialFallbackParticulas.mainTexture = ObtenerTexturaCirculoSuave();
      }

      if (materialFallbackParticulas.HasProperty("_Color"))
      {
        materialFallbackParticulas.color = Color.white;
      }
      if (materialFallbackParticulas.HasProperty("_ZWrite"))
      {
        materialFallbackParticulas.SetInt("_ZWrite", 0);
      }
      if (materialFallbackParticulas.HasProperty("_ZTest"))
      {
        materialFallbackParticulas.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
      }
      materialFallbackParticulas.renderQueue = 3900;
    }

    return materialFallbackParticulas;
  }

  private Texture2D ObtenerTexturaCirculoSuave()
  {
    if (texturaCirculoSuave != null)
    {
      return texturaCirculoSuave;
    }

    const int size = 64;
    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
    tex.name = "BattleAmbientLife_SoftCircle";
    tex.wrapMode = TextureWrapMode.Clamp;
    tex.filterMode = FilterMode.Bilinear;

    float half = (size - 1) * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float nx = (x - half) / half;
        float ny = (y - half) / half;
        float r = Mathf.Sqrt(nx * nx + ny * ny);
        float alpha = Mathf.Clamp01(1f - r);
        alpha = alpha * alpha * (3f - 2f * alpha); // smoothstep
        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
      }
    }

    tex.Apply(false, false);
    texturaCirculoSuave = tex;
    return texturaCirculoSuave;
  }

  private void ConfigurarSistemaMotas(ParticleSystem ps, Color color)
  {
    var main = ps.main;
    main.loop = true;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.maxParticles = Mathf.Max(8, maxParticulasMotas);
    main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.008f, 0.03f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.01008f, 0.028575f);
    main.startColor = new Color(color.r, color.g, color.b, 0.047f);
    main.gravityModifier = 0f;

    var emission = ps.emission;
    emission.rateOverTime = subterraneoActual ? 4.86f : 3.42f;

    var shape = ps.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Box;

    var noise = ps.noise;
    noise.enabled = true;
    noise.strength = 0.045f;
    noise.frequency = 0.08f;
    noise.scrollSpeed = 0.03f;

    var velocity = ps.velocityOverLifetime;
    velocity.enabled = true;
    velocity.space = ParticleSystemSimulationSpace.World;
    velocity.x = new ParticleSystem.MinMaxCurve(-0.006f, 0.006f);
    velocity.y = new ParticleSystem.MinMaxCurve(0.004f, 0.012f);
    velocity.z = new ParticleSystem.MinMaxCurve(-0.006f, 0.006f);

    ReiniciarSistema(ps);
  }

  private void ConfigurarSistemaBruma(ParticleSystem ps, Color color)
  {
    var main = ps.main;
    main.loop = true;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.maxParticles = Mathf.Max(6, maxParticulasBruma);
    main.startLifetime = new ParticleSystem.MinMaxCurve(9f, 14f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.005f, 0.018f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.027675f, 0.075375f);
    main.startColor = new Color(color.r, color.g, color.b, 0.025f);
    main.gravityModifier = 0f;

    var emission = ps.emission;
    emission.rateOverTime = subterraneoActual ? 2.16f : 1.35f;

    var shape = ps.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Box;

    var noise = ps.noise;
    noise.enabled = true;
    noise.strength = 0.08f;
    noise.frequency = 0.04f;
    noise.scrollSpeed = 0.02f;

    var velocity = ps.velocityOverLifetime;
    velocity.enabled = true;
    velocity.space = ParticleSystemSimulationSpace.World;
    velocity.x = new ParticleSystem.MinMaxCurve(-0.004f, 0.004f);
    velocity.y = new ParticleSystem.MinMaxCurve(0.002f, 0.007f);
    velocity.z = new ParticleSystem.MinMaxCurve(-0.004f, 0.004f);

    ReiniciarSistema(ps);
  }

  private void ReiniciarSistema(ParticleSystem ps)
  {
    if (ps == null)
    {
      return;
    }

    ps.Clear(true);
    ps.Play(true);
  }

  private void SincronizarAreaParticulas()
  {
    if (!habilitarParticulas || psMotas == null || psBruma == null)
    {
      return;
    }

    if (!boundsValido)
    {
      RecalcularBoundsCampo();
      if (!boundsValido)
      {
        return;
      }
    }

    Vector3 centro = boundsCampo.center + new Vector3(0f, 0.2f, 0f);
    Vector3 tam = new Vector3(
      Mathf.Max(3f, boundsCampo.size.x + 1.5f),
      Mathf.Max(1.2f, boundsCampo.size.y + 0.8f),
      Mathf.Max(3f, boundsCampo.size.z + 1.5f));

    if (goAmbientRoot != null)
    {
      goAmbientRoot.transform.position = centro;
    }

    var shapeMotas = psMotas.shape;
    shapeMotas.scale = tam;

    var shapeBruma = psBruma.shape;
    shapeBruma.scale = Vector3.Scale(tam, new Vector3(1f, 0.8f, 1f));
  }

  private Color ObtenerColorZona()
  {
    switch (zonaActual)
    {
      case EncounterZoneType.PasoVientoHelado:
        return new Color(0.78f, 0.9f, 1f, 1f);
      case EncounterZoneType.Nedukazal:
        return new Color(0.98f, 0.84f, 0.67f, 1f);
      case EncounterZoneType.Subterraneo:
        return new Color(0.72f, 0.88f, 0.9f, 1f);
      case EncounterZoneType.BosqueAngustiante:
      case EncounterZoneType.Generico:
      default:
        return new Color(0.79f, 0.93f, 0.78f, 1f);
    }
  }

  private void AplicarPresetVisual()
  {
    Color colorZona = ObtenerColorZona();
    if (subterraneoActual)
    {
      colorZona = Color.Lerp(colorZona, new Color(0.72f, 0.86f, 0.9f, 1f), 0.6f);
      amplitudUVx = 0.00018f;
      amplitudUVy = 0.00012f;
      frecuenciaUV = 0.075f;
      amplitudPulsoColor = 0.004f;
      velocidadPulsoColor = 0.16f;
    }
    else
    {
      amplitudUVx = 0.00022f;
      amplitudUVy = 0.00014f;
      frecuenciaUV = 0.085f;
      amplitudPulsoColor = 0.006f;
      velocidadPulsoColor = 0.18f;
    }

    if (materialFondoInstancia != null && materialFondoInstancia.HasProperty("_Color"))
    {
      Color actual = materialFondoInstancia.color;
      colorBaseFondo = Color.Lerp(actual, colorZona, 0.08f);
      colorBaseFondo.a = actual.a;
    }
  }
}
