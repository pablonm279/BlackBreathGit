using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class ReaccionEstertorMalditoBruja : Reaccion
{
  private const int DCMental = 12;
  private const int DadosCantidad = 2;
  private const int DadosCaras = 6;
  private const int TipoDanioNecrotico = 9;
  private static Material materialParticulasEstertor;

  void Start()
  {
    TipoTrigger = 3;
    usos = 1;
    permanente = true;
    nombre = "Estertor Maldito";
    scEstaUnidad = GetComponent<Unidad>();
    descripcion = TRADU.i != null
      ? TRADU.i.Traducir("Reacción: al morir, quien asesta el golpe final debe superar TS Mental 12 o recibe 2d6 daño necrótico.")
      : "Reacción: al morir, quien asesta el golpe final debe superar TS Mental 12 o recibe 2d6 daño necrótico.";
  }

  public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
  {
    if (BattleManager.Instance == null || scEstaUnidad == null)
    {
      return;
    }

    Unidad asesino = scEstaUnidad.ultimaUnidadQueLeHizoDanio != null
      ? scEstaUnidad.ultimaUnidadQueLeHizoDanio
      : BattleManager.Instance.unidadActiva;
    if (asesino == null || asesino == scEstaUnidad || asesino.HP_actual <= 0)
    {
      ConsumirUso();
      return;
    }

    await BattleManager.DelayCombateAsync(260);

    BattleManager.Instance.EscribirLog(scEstaUnidad.uNombre + TRADU.i.Traducir(" reacciona con ") + nombre + ".");

    bool fallaSalvacion = asesino.TiradaSalvacion(asesino.mod_TSMental, DCMental);
    if (fallaSalvacion)
    {
      CrearVfxEstertor(asesino);
      int danio = TiradaDeDados.TirarDados(DadosCantidad, DadosCaras);
      asesino.RecibirDanio(danio, TipoDanioNecrotico, false, scEstaUnidad);
    }
    else
    {
      BattleManager.Instance.EscribirLog(asesino.uNombre + TRADU.i.Traducir(" resiste el estertor maldito."));
    }

    ConsumirUso();
  }

  private void ConsumirUso()
  {
    usos--;
    if (usos <= 0)
    {
      Destroy(this);
    }
  }

  private void CrearVfxEstertor(Unidad objetivo)
  {
    if (objetivo == null)
    {
      return;
    }

    Vector3 posicion = objetivo.puntoEntrante != null
      ? objetivo.puntoEntrante.position
      : objetivo.transform.position + Vector3.up * 0.45f;

    GameObject vfx = new GameObject("VFX_EstertorMalditoBruja");
    if (BattleManager.Instance != null)
    {
      vfx.transform.SetParent(BattleManager.Instance.transform, true);
    }
    vfx.transform.position = posicion;

    ParticleSystem particulas = vfx.AddComponent<ParticleSystem>();
    particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    var main = particulas.main;
    main.duration = 0.45f;
    main.loop = false;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.42f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
    main.startColor = new ParticleSystem.MinMaxGradient(
      new Color(0.85f, 1f, 0.7f, 0.85f),
      new Color(0.38f, 0.78f, 0.42f, 0.78f));
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.playOnAwake = false;
    main.gravityModifier = -0.03f;
    main.maxParticles = 18;

    var emission = particulas.emission;
    emission.rateOverTime = 0f;

    var shape = particulas.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Circle;
    shape.radius = 0.1f;

    var colorOverLifetime = particulas.colorOverLifetime;
    colorOverLifetime.enabled = true;
    Gradient gradiente = new Gradient();
    gradiente.SetKeys(
      new[]
      {
        new GradientColorKey(new Color(0.88f, 1f, 0.76f), 0f),
        new GradientColorKey(new Color(0.45f, 0.9f, 0.5f), 0.55f),
        new GradientColorKey(new Color(0.08f, 0.16f, 0.09f), 1f)
      },
      new[]
      {
        new GradientAlphaKey(0.9f, 0f),
        new GradientAlphaKey(0.35f, 0.55f),
        new GradientAlphaKey(0f, 1f)
      });
    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradiente);

    var sizeOverLifetime = particulas.sizeOverLifetime;
    sizeOverLifetime.enabled = true;
    AnimationCurve curvaTam = new AnimationCurve();
    curvaTam.AddKey(0f, 0.65f);
    curvaTam.AddKey(0.35f, 1f);
    curvaTam.AddKey(1f, 0.16f);
    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curvaTam);

    ParticleSystemRenderer renderer = particulas.GetComponent<ParticleSystemRenderer>();
    if (renderer != null)
    {
      renderer.material = ObtenerMaterialParticulasEstertor();
      renderer.renderMode = ParticleSystemRenderMode.Billboard;
      renderer.alignment = ParticleSystemRenderSpace.View;
      renderer.minParticleSize = 0.0008f;
      renderer.maxParticleSize = 0.035f;
    }

    particulas.Emit(16);
    particulas.Play();
    Destroy(vfx, 1.25f);
  }

  private static Material ObtenerMaterialParticulasEstertor()
  {
    if (materialParticulasEstertor == null)
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

      materialParticulasEstertor = new Material(shader);
      materialParticulasEstertor.name = "Mat_VFX_EstertorMalditoBruja";

      Color colorBase = new Color(0.72f, 0.95f, 0.68f, 0.5f);
      if (materialParticulasEstertor.HasProperty("_Color"))
      {
        materialParticulasEstertor.SetColor("_Color", colorBase);
      }
      if (materialParticulasEstertor.HasProperty("_TintColor"))
      {
        materialParticulasEstertor.SetColor("_TintColor", colorBase);
      }
      if (materialParticulasEstertor.HasProperty("_SoftParticlesNearFadeDistance"))
      {
        materialParticulasEstertor.SetFloat("_SoftParticlesNearFadeDistance", 0.08f);
      }
      if (materialParticulasEstertor.HasProperty("_SoftParticlesFarFadeDistance"))
      {
        materialParticulasEstertor.SetFloat("_SoftParticlesFarFadeDistance", 0.55f);
      }
    }

    return materialParticulasEstertor;
  }
}
