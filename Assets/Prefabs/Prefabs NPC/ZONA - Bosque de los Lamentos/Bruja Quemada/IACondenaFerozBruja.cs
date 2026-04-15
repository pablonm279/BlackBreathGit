using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IACondenaFerozBruja : IAHabilidad
{
  private const int CooldownCondenaFeroz = 3;
  private const int PrioridadCondenaFeroz = 11;
  private const int DuracionCondena = 3;
  private const int DuracionBuff = 3;
  private const string NombreBuff = "Condena Feroz";
  private const float DuracionVfxLazo = 0.55f;
  private const float DuracionVfxSello = 1.15f;
  private const int SegmentosSello = 42;

  private static Material materialVfx;
  private static Material materialParticulasSello;
  [SerializeField] private AudioClip sfxCondenaFeroz;
  [SerializeField] private float volumenSfxCondenaFeroz = 0.85f;

  void Awake()
  {
    nombre = "Condena Feroz";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = CooldownCondenaFeroz;
    esHostil = false;
    prioridad = PrioridadCondenaFeroz;
    costoAP = 3;
    afectaObstaculos = false;
    fuerzaPoseAtaque = false;
    hActualCooldown = ObtenerCooldownInicialRandom();
  }

  private int ObtenerCooldownInicialRandom()
  {
    return hCooldownMax > 0 ? UnityEngine.Random.Range(0, hCooldownMax + 1) : 0;
  }

  public override List<object> ListaHayObjetivosAlAlcance()
  {
    return ObtenerAliadosValidos()
      .Cast<object>()
      .ToList();
  }

  public override object EstablecerObjetivoPrioritario()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
    }

    if (scEstaUnidad == null)
    {
      return null;
    }

    return ObtenerAliadosValidos()
      .OrderBy(unidad => unidad.TieneBuffNombre(NombreBuff) ? 1 : 0)
      .ThenBy(unidad => unidad.estado_Condenado >= DuracionCondena ? 1 : 0)
      .ThenByDescending(unidad => unidad.CasillaPosicion != null ? unidad.CasillaPosicion.posX : 0)
      .ThenBy(unidad => unidad.CasillaPosicion != null && scEstaUnidad.CasillaPosicion != null
        ? Mathf.Abs(unidad.CasillaPosicion.posY - scEstaUnidad.CasillaPosicion.posY)
        : 99)
      .ThenByDescending(unidad => unidad.mod_Ataque + unidad.mod_CarPoder)
      .FirstOrDefault();
  }

  public async override Task ActivarHabilidad()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
    }

    Unidad objetivo = EstablecerObjetivoPrioritario() as Unidad;
    if (scEstaUnidad == null || objetivo == null)
    {
      return;
    }

    hActualCooldown = hCooldownMax;
    scEstaUnidad.CambiarAPActual(-costoAP);
    PrepararInicioAnimacion(null, objetivo);
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();
    ReproducirSfxCondenaFeroz();
    CrearVfxLazo(scEstaUnidad, objetivo);

    await BattleManager.DelayCombateAsync(525);

    AplicarEfectosHabilidad(objetivo);

    await BattleManager.DelayCombateAsync(180);
  }

  public override void AplicarEfectosHabilidad(object obj)
  {
    if (!(obj is Unidad objetivo) || objetivo.HP_actual <= 0)
    {
      return;
    }

    objetivo.estado_Condenado = Mathf.Max(objetivo.estado_Condenado, DuracionCondena);
    objetivo.GenerarTextoFlotante(TRADU.i != null ? TRADU.i.Traducir("Condenado") : "Condenado", new Color(0.78f, 0.4f, 0.95f));

    objetivo.RemoverBuffNombre(NombreBuff);

    Buff buff = new Buff
    {
      buffNombre = NombreBuff,
      buffDescr = "+15% Danio, +2 Ataque, +5 TS Mental.",
      boolfDebufftBuff = true,
      DuracionBuffRondas = DuracionBuff,
      cantDanioPorcentaje = 15f,
      cantAtaque = 2f,
      cantTsMental = 5f,
      esBuffVisibleUI = true,
      esRemovible = true,
      esStackeable = false
    };

    buff.AplicarBuff(objetivo, scEstaUnidad, true);
    ComponentCopier.CopyComponent(buff, objetivo.gameObject);

    CrearVfxSello(objetivo);
    BattleManager.Instance?.scUIInfoChar?.ActualizarInfoChar(objetivo);

    if (BattleManager.Instance != null)
    {
      BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} {ObtenerTextoAplicacion()} {objetivo.uNombre}.");
    }
  }

  private List<Unidad> ObtenerAliadosValidos()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = GetComponent<Unidad>();
    }

    if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null || scEstaUnidad.CasillaPosicion.ladoGO == null)
    {
      return new List<Unidad>();
    }

    LadoManager ladoPropio = scEstaUnidad.CasillaPosicion.ladoGO.GetComponent<LadoManager>();
    if (ladoPropio == null)
    {
      return new List<Unidad>();
    }

    ladoPropio.ActualizarListaDeUnidadesEnLado();

    return ladoPropio.unidadesLado
      .Where(unidad => unidad != null)
      .Where(unidad => unidad != scEstaUnidad)
      .Where(unidad => unidad.HP_actual > 0)
      .Where(unidad => !unidad.TieneBuffNombre(NombreBuff) || unidad.estado_Condenado < DuracionCondena)
      .ToList();
  }

  private void CrearVfxLazo(Unidad origen, Unidad objetivo)
  {
    if (origen == null || objetivo == null)
    {
      return;
    }

    GameObject lazo = new GameObject("VFX_CondenaFeroz_Lazo");
    if (BattleManager.Instance != null)
    {
      lazo.transform.SetParent(BattleManager.Instance.transform, true);
    }

    LineRenderer linea = lazo.AddComponent<LineRenderer>();
    linea.useWorldSpace = true;
    linea.alignment = LineAlignment.View;
    linea.material = ObtenerMaterial();
    linea.widthMultiplier = 0.038f;
    linea.positionCount = 5;
    linea.textureMode = LineTextureMode.Stretch;
    linea.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    linea.receiveShadows = false;
    linea.numCapVertices = 4;
    linea.numCornerVertices = 4;

    Vector3 inicio = ObtenerPosicionVisual(origen) + new Vector3(0f, 0.05f, 0f);
    Vector3 fin = ObtenerPosicionVisual(objetivo) + new Vector3(0f, 0.08f, 0f);
    Vector3 direccion = (fin - inicio).normalized;
    Vector3 lateral = Vector3.Cross(direccion, Vector3.up).normalized;

    linea.SetPosition(0, inicio);
    linea.SetPosition(1, Vector3.Lerp(inicio, fin, 0.22f) + lateral * 0.11f + Vector3.up * 0.1f);
    linea.SetPosition(2, Vector3.Lerp(inicio, fin, 0.48f) - lateral * 0.04f + Vector3.up * 0.18f);
    linea.SetPosition(3, Vector3.Lerp(inicio, fin, 0.76f) + lateral * 0.08f + Vector3.up * 0.08f);
    linea.SetPosition(4, fin);
    linea.startColor = new Color(0.98f, 0.7f, 0.28f, 0.55f);
    linea.endColor = new Color(0.7f, 0.32f, 0.9f, 0.05f);

    StartCoroutine(DesvanecerLinea(linea, DuracionVfxLazo));
  }

  private void CrearVfxSello(Unidad objetivo)
  {
    if (objetivo == null)
    {
      return;
    }

    Vector3 centro = ObtenerPosicionVisual(objetivo);
    GameObject sello = new GameObject("VFX_CondenaFeroz_Sello");
    if (BattleManager.Instance != null)
    {
      sello.transform.SetParent(BattleManager.Instance.transform, true);
    }

    LineRenderer aro = sello.AddComponent<LineRenderer>();
    aro.useWorldSpace = true;
    aro.alignment = LineAlignment.View;
    aro.material = ObtenerMaterial();
    aro.widthMultiplier = 0.024f;
    aro.loop = true;
    aro.textureMode = LineTextureMode.Stretch;
    aro.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    aro.receiveShadows = false;
    aro.numCapVertices = 4;
    aro.numCornerVertices = 4;
    aro.positionCount = SegmentosSello;

    for (int i = 0; i < SegmentosSello; i++)
    {
      float angulo = i / (float)SegmentosSello * Mathf.PI * 2f;
      float radio = 0.13f + Mathf.Sin(angulo * 4f) * 0.015f;
      Vector3 punto = centro
        + Vector3.up * (0.24f + Mathf.Sin(angulo * 2f) * 0.018f)
        + new Vector3(Mathf.Cos(angulo) * radio, 0f, Mathf.Sin(angulo) * radio);
      aro.SetPosition(i, punto);
    }

    aro.startColor = new Color(0.96f, 0.72f, 0.3f, 0.5f);
    aro.endColor = new Color(0.56f, 0.16f, 0.78f, 0.12f);

    ParticleSystem particulas = sello.AddComponent<ParticleSystem>();
    particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    var main = particulas.main;
    main.duration = 0.45f;
    main.loop = false;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.45f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.22f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.055f);
    main.startColor = new ParticleSystem.MinMaxGradient(
      new Color(1f, 0.74f, 0.32f, 0.42f),
      new Color(0.72f, 0.18f, 0.95f, 0.34f));
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.playOnAwake = false;
    main.gravityModifier = -0.02f;
    main.maxParticles = 16;

    var emission = particulas.emission;
    emission.rateOverTime = 0f;

    var shape = particulas.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Circle;
    shape.radius = 0.06f;
    shape.position = centro + Vector3.up * 0.2f;

    var colorOverLifetime = particulas.colorOverLifetime;
    colorOverLifetime.enabled = true;
    Gradient gradiente = new Gradient();
    gradiente.SetKeys(
      new[]
      {
        new GradientColorKey(new Color(1f, 0.78f, 0.35f), 0f),
        new GradientColorKey(new Color(0.55f, 0.16f, 0.8f), 0.62f),
        new GradientColorKey(new Color(0.14f, 0.05f, 0.1f), 1f)
      },
      new[]
      {
        new GradientAlphaKey(0.45f, 0f),
        new GradientAlphaKey(0.18f, 0.45f),
        new GradientAlphaKey(0f, 1f)
      });
    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradiente);

    var sizeOverLifetime = particulas.sizeOverLifetime;
    sizeOverLifetime.enabled = true;
    AnimationCurve curvaTam = new AnimationCurve();
    curvaTam.AddKey(0f, 0.55f);
    curvaTam.AddKey(0.35f, 1f);
    curvaTam.AddKey(1f, 0.12f);
    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curvaTam);

    var velocityOverLifetime = particulas.velocityOverLifetime;
    velocityOverLifetime.enabled = true;
    velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f, 0f);
    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f, 0f);

    ParticleSystemRenderer renderer = particulas.GetComponent<ParticleSystemRenderer>();
    if (renderer != null)
    {
      renderer.material = ObtenerMaterialParticulasSello();
      renderer.renderMode = ParticleSystemRenderMode.Billboard;
      renderer.alignment = ParticleSystemRenderSpace.View;
      renderer.minParticleSize = 0.0008f;
      renderer.maxParticleSize = 0.03f;
    }

    particulas.Emit(12);
    particulas.Play();

    StartCoroutine(DesvanecerLinea(aro, DuracionVfxSello));
    Destroy(sello, 1.1f);
  }

  private IEnumerator DesvanecerLinea(LineRenderer linea, float duracion)
  {
    if (linea == null)
    {
      yield break;
    }

    float anchoInicial = linea.widthMultiplier;
    Color inicio = linea.startColor;
    Color fin = linea.endColor;
    float tiempo = 0f;

    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float factor = Mathf.Clamp01(tiempo / duracion);
      if (linea != null)
      {
        linea.widthMultiplier = Mathf.Lerp(anchoInicial, anchoInicial * 0.18f, factor);
        linea.startColor = new Color(inicio.r, inicio.g, inicio.b, Mathf.Lerp(inicio.a, 0f, factor));
        linea.endColor = new Color(fin.r, fin.g, fin.b, Mathf.Lerp(fin.a, 0f, factor));
      }
      yield return null;
    }

    if (linea != null)
    {
      Destroy(linea.gameObject);
    }
  }

  private static Vector3 ObtenerPosicionVisual(Unidad unidad)
  {
    if (unidad == null)
    {
      return Vector3.zero;
    }

    if (unidad.puntoEntrante != null)
    {
      return unidad.puntoEntrante.position;
    }

    return unidad.transform.position + Vector3.up * 0.45f;
  }

  private void ReproducirSfxCondenaFeroz()
  {
    if (sfxCondenaFeroz == null)
    {
      return;
    }

    Vector3 posicion = scEstaUnidad != null
      ? ObtenerPosicionVisual(scEstaUnidad)
      : transform.position;
    AjustesAudio.ReproducirClipEnPunto(sfxCondenaFeroz, posicion, Mathf.Clamp01(volumenSfxCondenaFeroz));
  }

  private static Material ObtenerMaterial()
  {
    if (materialVfx == null)
    {
      Shader shader = Shader.Find("Sprites/Default");
      materialVfx = new Material(shader);
    }

    return materialVfx;
  }

  private static Material ObtenerMaterialParticulasSello()
  {
    if (materialParticulasSello == null)
    {
      Shader shader = Shader.Find("Particles/Standard Unlit");
      if (shader == null)
      {
        shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
      }
      if (shader == null)
      {
        shader = Shader.Find("Legacy Shaders/Particles/Additive");
      }
      if (shader == null)
      {
        shader = Shader.Find("Sprites/Default");
      }

      materialParticulasSello = new Material(shader);
      materialParticulasSello.name = "Mat_VFX_CondenaFeroz_Sello";

      if (materialParticulasSello.HasProperty("_Color"))
      {
        materialParticulasSello.SetColor("_Color", new Color(0.98f, 0.76f, 0.38f, 0.38f));
      }
      if (materialParticulasSello.HasProperty("_TintColor"))
      {
        materialParticulasSello.SetColor("_TintColor", new Color(0.98f, 0.76f, 0.38f, 0.38f));
      }
      if (materialParticulasSello.HasProperty("_SoftParticlesNearFadeDistance"))
      {
        materialParticulasSello.SetFloat("_SoftParticlesNearFadeDistance", 0.08f);
      }
      if (materialParticulasSello.HasProperty("_SoftParticlesFarFadeDistance"))
      {
        materialParticulasSello.SetFloat("_SoftParticlesFarFadeDistance", 0.55f);
      }
    }

    return materialParticulasSello;
  }

  private string ObtenerTextoAplicacion()
  {
    if (TRADU.i == null)
    {
      return "desata Condena Feroz sobre";
    }

    switch (TRADU.i.nIdioma)
    {
      case TRADU.IdiomaIngles:
        return "unleashes Fierce Condemnation on";
      case TRADU.IdiomaPortugues:
        return "libera Condenacao Feroz sobre";
      default:
        return "desata Condena Feroz sobre";
    }
  }
}
