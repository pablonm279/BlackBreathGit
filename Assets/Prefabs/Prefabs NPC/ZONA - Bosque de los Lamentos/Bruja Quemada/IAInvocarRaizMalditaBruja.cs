using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IAInvocarRaizMalditaBruja : IAHabilidad
{
  private const string RutaSfxInvocarRaizMaldita = "Sonidos/Efectos/Hurt/ArbolLamentos_dolor1";
  private const int CooldownInvocacion = 5;
  private const int PrioridadInvocacion = 12;
  private static readonly int[] ColumnasPrioridad = { 3, 2, 1 };
  private static readonly int[] DesplazamientosFila = { 0, -1, 1 };
  private static Material materialParticulasInvocacion;
  [SerializeField] private AudioClip sfxInvocarRaizMaldita;
  [SerializeField] private float volumenSfxInvocarRaizMaldita = 0.85f;

  void Awake()
  {
    nombre = "Invocar Raíz Maldita";
    Usuario = gameObject;
    scEstaUnidad = Usuario.GetComponent<Unidad>();
    hAncho = 1;
    esMelee = false;
    hAlcance = 1;
    hCooldownMax = CooldownInvocacion;
    esHostil = false;
    prioridad = PrioridadInvocacion;
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
    return ObtenerCasillaInvocacion() != null
      ? new List<object> { scEstaUnidad }
      : new List<object>();
  }

  public override object EstablecerObjetivoPrioritario()
  {
    return scEstaUnidad;
  }

  public async override Task ActivarHabilidad()
  {
    if (scEstaUnidad == null)
    {
      scEstaUnidad = Usuario.GetComponent<Unidad>();
    }

    if (scEstaUnidad == null || ObtenerCasillaInvocacion() == null)
    {
      return;
    }

    hActualCooldown = hCooldownMax;
    scEstaUnidad.CambiarAPActual(-costoAP);
    PrepararInicioAnimacion(null, scEstaUnidad);
    ReproducirSfxInvocacion();

    await BattleManager.DelayCombateAsync(500);

    AplicarEfectosHabilidad(scEstaUnidad);

    scEstaUnidad.EstablecerAPActualA(0);

    await BattleManager.DelayCombateAsync(250);
  }

  public override void AplicarEfectosHabilidad(object objetivo)
  {
    BattleManager battleManager = BattleManager.Instance;
    if (battleManager == null || battleManager.contenedorPrefabs == null || scEstaUnidad == null)
    {
      return;
    }

    Casilla casillaInvocacion = ObtenerCasillaInvocacion();
    if (casillaInvocacion == null)
    {
      return;
    }

    GameObject prefabRaizMaldita = battleManager.contenedorPrefabs.Raizmaldita;
    if (prefabRaizMaldita == null)
    {
      Debug.LogWarning("[IAInvocarRaizMalditaBruja] Falta la referencia a Raizmaldita en ContenedorPrefabs.");
      return;
    }

    GameObject raizMaldita = Instantiate(prefabRaizMaldita);
    if (!casillaInvocacion.PonerObjetoEnCasilla(raizMaldita))
    {
      Destroy(raizMaldita);
      return;
    }

    Unidad unidadInvocada = raizMaldita.GetComponent<Unidad>();
    if (unidadInvocada != null)
    {
      unidadInvocada.EstablecerAPActualA(0);
    }

    CrearVfxInvocacion(casillaInvocacion.transform.position);
    RegistrarUnidadInvocada(unidadInvocada);
    battleManager.scUIBarraOrdenTurno?.ActualizarBarraOrdenTurno();
    battleManager.scUIInfoChar?.ActualizarInfoChar(scEstaUnidad);
    battleManager.CalcularCasillasAMovimiento();

    string nombreInvocacion = TRADU.i != null ? TRADU.i.Traducir("Raiz Maldita") : "Raiz Maldita";
    battleManager.EscribirLog($"{scEstaUnidad.uNombre} {ObtenerTextoInvocacion()} {nombreInvocacion}.");
  }

  private Casilla ObtenerCasillaInvocacion()
  {
    if (BattleManager.Instance == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
    {
      return null;
    }

    LadoManager ladoPropio = scEstaUnidad.CasillaPosicion.ladoGO != null
      ? scEstaUnidad.CasillaPosicion.ladoGO.GetComponent<LadoManager>()
      : null;

    if (ladoPropio == null)
    {
      return null;
    }

    int filaBase = scEstaUnidad.CasillaPosicion.posY;
    for (int i = 0; i < DesplazamientosFila.Length; i++)
    {
      int filaObjetivo = filaBase + DesplazamientosFila[i];
      if (filaObjetivo < 1 || filaObjetivo > 5)
      {
        continue;
      }

      for (int j = 0; j < ColumnasPrioridad.Length; j++)
      {
        Casilla casilla = ladoPropio.ObtenerCasillaPorIndex(ColumnasPrioridad[j], filaObjetivo);
        if (EsCasillaValidaParaInvocacion(casilla))
        {
          return casilla;
        }
      }
    }

    return null;
  }

  private static bool EsCasillaValidaParaInvocacion(Casilla casilla)
  {
    return casilla != null && casilla.Presente == null && casilla.GetComponent<Trampa>() == null;
  }

  private void ReproducirSfxInvocacion()
  {
    if (sfxInvocarRaizMaldita == null)
    {
      sfxInvocarRaizMaldita = Resources.Load<AudioClip>(RutaSfxInvocarRaizMaldita);
    }

    if (sfxInvocarRaizMaldita == null)
    {
      return;
    }

    Vector3 posicion = scEstaUnidad != null && scEstaUnidad.puntoEntrante != null
      ? scEstaUnidad.puntoEntrante.position
      : transform.position;
    AjustesAudio.ReproducirClipEnPunto(sfxInvocarRaizMaldita, posicion, Mathf.Clamp01(volumenSfxInvocarRaizMaldita));
  }

  private void RegistrarUnidadInvocada(Unidad unidadInvocada)
  {
    if (BattleManager.Instance == null || unidadInvocada == null)
    {
      return;
    }

    LadoManager ladoPropio = unidadInvocada.CasillaPosicion != null && unidadInvocada.CasillaPosicion.ladoGO != null
      ? unidadInvocada.CasillaPosicion.ladoGO.GetComponent<LadoManager>()
      : null;

    ladoPropio?.ActualizarListaDeUnidadesEnLado();

    if (!BattleManager.Instance.lUnidadesTotal.Contains(unidadInvocada))
    {
      BattleManager.Instance.lUnidadesTotal.Add(unidadInvocada);
    }
  }

  private void CrearVfxInvocacion(Vector3 posicion)
  {
    GameObject vfx = new GameObject("VFX_InvocarRaizMaldita");
    vfx.transform.position = posicion + Vector3.up * 0.2f;

    ParticleSystem particulas = vfx.AddComponent<ParticleSystem>();
    particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    var main = particulas.main;
    main.duration = 0.38f;
    main.loop = false;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.24f, 0.42f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.06f, 0.34f);
    main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.085f);
    main.startColor = new ParticleSystem.MinMaxGradient(
      new Color(0.28f, 0.18f, 0.07f, 0.6f),
      new Color(0.62f, 0.95f, 0.5f, 0.48f));
    main.gravityModifier = -0.02f;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.playOnAwake = false;
    main.maxParticles = 16;

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
        new GradientColorKey(new Color(0.72f, 0.98f, 0.62f), 0f),
        new GradientColorKey(new Color(0.4f, 0.68f, 0.33f), 0.45f),
        new GradientColorKey(new Color(0.15f, 0.11f, 0.06f), 1f)
      },
      new[]
      {
        new GradientAlphaKey(0.55f, 0f),
        new GradientAlphaKey(0.22f, 0.45f),
        new GradientAlphaKey(0f, 1f)
      });
    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradiente);

    var sizeOverLifetime = particulas.sizeOverLifetime;
    sizeOverLifetime.enabled = true;
    AnimationCurve curvaTam = new AnimationCurve();
    curvaTam.AddKey(0f, 0.55f);
    curvaTam.AddKey(0.35f, 1f);
    curvaTam.AddKey(1f, 0.18f);
    sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curvaTam);

    ParticleSystemRenderer renderer = particulas.GetComponent<ParticleSystemRenderer>();
    if (renderer != null)
    {
      renderer.material = ObtenerMaterialParticulasInvocacion();
      renderer.renderMode = ParticleSystemRenderMode.Billboard;
      renderer.alignment = ParticleSystemRenderSpace.View;
      renderer.minParticleSize = 0.0008f;
      renderer.maxParticleSize = 0.04f;
    }

    particulas.Emit(10);
    particulas.Play();
    Destroy(vfx, 1.1f);
  }

  private static Material ObtenerMaterialParticulasInvocacion()
  {
    if (materialParticulasInvocacion == null)
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

      materialParticulasInvocacion = new Material(shader);
      materialParticulasInvocacion.name = "Mat_VFX_InvocarRaizMaldita";

      if (materialParticulasInvocacion.HasProperty("_Color"))
      {
        materialParticulasInvocacion.SetColor("_Color", new Color(0.72f, 0.98f, 0.62f, 0.52f));
      }
      if (materialParticulasInvocacion.HasProperty("_TintColor"))
      {
        materialParticulasInvocacion.SetColor("_TintColor", new Color(0.72f, 0.98f, 0.62f, 0.52f));
      }
      if (materialParticulasInvocacion.HasProperty("_SoftParticlesNearFadeDistance"))
      {
        materialParticulasInvocacion.SetFloat("_SoftParticlesNearFadeDistance", 0.1f);
      }
      if (materialParticulasInvocacion.HasProperty("_SoftParticlesFarFadeDistance"))
      {
        materialParticulasInvocacion.SetFloat("_SoftParticlesFarFadeDistance", 0.65f);
      }
    }

    return materialParticulasInvocacion;
  }

  private string ObtenerTextoInvocacion()
  {
    if (TRADU.i == null)
    {
      return "invoca una";
    }

    switch (TRADU.i.nIdioma)
    {
      case TRADU.IdiomaIngles:
        return "summons a";
      case TRADU.IdiomaPortugues:
        return "invoca uma";
      default:
        return "invoca una";
    }
  }
}
