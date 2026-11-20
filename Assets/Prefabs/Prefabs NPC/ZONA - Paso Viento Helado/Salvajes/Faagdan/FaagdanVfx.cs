using UnityEngine;

/// <summary>
/// Polvo, nieve y escombros procedurales para las habilidades del Faagdan (Embestida, Garra, Mordisco).
/// Reutiliza los helpers de particulas de GulekGulVfx. Tamaños en "casillas" para escalar con el tablero.
/// </summary>
public static class FaagdanVfx
{
  // Polvo helado del Paso: gris azulado claro.
  private static readonly Color PolvoPaso = new Color(0.84f, 0.88f, 0.93f);
  private static readonly Color PiedraPaso = new Color(0.5f, 0.5f, 0.52f);

  // ------------------------------------------------------------------ bloques base

  /// <summary>Nube de polvo que se levanta. empujeX sesga el movimiento (-1 atras, +1 adelante).</summary>
  private static void Nube(Vector3 posicion, float intensidad, float anchoEnCasillas, Color tinte, float empujeX, int cantidad)
  {
    intensidad = Mathf.Clamp(intensidad, 0.3f, 2.2f);
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Faagdan_Polvo", posicion + new Vector3(0f, cas * 0.05f, 0f), 1.6f);

    ParticleSystem polvo = GulekGulVfx.CrearSistema(raiz.transform, "Polvo", 0.5f, 36);
    var main = polvo.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.95f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.15f, cas * 0.55f);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.2f * intensidad, cas * 0.4f * intensidad);
    main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    main.startColor = new Color(tinte.r, tinte.g, tinte.b, 0.55f);
    main.gravityModifier = -0.02f;

    var shape = polvo.shape;
    shape.radius = cas * anchoEnCasillas * 0.5f;

    var vel = polvo.velocityOverLifetime;
    vel.enabled = true;
    vel.space = ParticleSystemSimulationSpace.World;
    float lateral = cas * 0.5f;
    vel.x = new ParticleSystem.MinMaxCurve(-lateral + empujeX * cas * 0.9f, lateral + empujeX * cas * 0.9f);
    vel.y = new ParticleSystem.MinMaxCurve(cas * 0.1f, cas * 0.45f);
    vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

    var tam = polvo.sizeOverLifetime;
    tam.enabled = true;
    tam.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.55f)));

    GulekGulVfx.AplicarGradiente(polvo, tinte, 0.5f);
    polvo.Play();
    polvo.Emit(Mathf.RoundToInt(cantidad * intensidad));

    GulekGulVfx.AlFrente(raiz);
  }

  /// <summary>Trocitos de piedra/hielo que saltan y caen.</summary>
  private static void Escombros(Vector3 posicion, float intensidad, Color tinte, float empujeX)
  {
    intensidad = Mathf.Clamp(intensidad, 0.3f, 2.2f);
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Faagdan_Escombros", posicion, 1.4f);

    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Escombros", 0.4f, 24);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.8f, cas * 2f);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.03f, cas * 0.075f);
    main.startColor = new ParticleSystem.MinMaxGradient(tinte, Color.Lerp(tinte, Color.white, 0.5f));
    main.gravityModifier = 2.4f;

    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Cone;
    shape.angle = 40f;
    shape.radius = cas * 0.15f;
    shape.rotation = new Vector3(-90f, 0f, 0f);

    var vel = ps.velocityOverLifetime;
    vel.enabled = true;
    vel.space = ParticleSystemSimulationSpace.World;
    vel.x = new ParticleSystem.MinMaxCurve(empujeX * cas * 0.4f, empujeX * cas * 1.2f);
    vel.y = new ParticleSystem.MinMaxCurve(0f, 0f);
    vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

    GulekGulVfx.AplicarGradiente(ps, tinte, 1f, 0.05f);
    ps.Play();
    ps.Emit(Mathf.RoundToInt(9 * intensidad));

    GulekGulVfx.AlFrente(raiz);
  }

  // ------------------------------------------------------------------ Embestida

  /// <summary>Patada de arranque: polvo hacia atras y un anillo leve.</summary>
  public static void Arranque(Vector3 posicion, float dirX)
  {
    Nube(posicion, 1.1f, 0.9f, PolvoPaso, -dirX, 12);
    GulekGulVfx.OndaDeChoque(posicion, 0.45f, new Color(PolvoPaso.r, PolvoPaso.g, PolvoPaso.b, 0.35f), 0.35f, 1);
  }

  /// <summary>Estela de polvo que acompaña a la unidad mientras corre (se apaga sola).</summary>
  public static void Estela(Transform unidad, float duracion, float intensidad = 1f)
  {
    if (unidad == null) { return; }

    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Faagdan_Estela", unidad.position, duracion + 1.4f);
    raiz.transform.SetParent(unidad, true);

    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Estela", Mathf.Max(0.1f, duracion), 70);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.05f, cas * 0.3f);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.14f * intensidad, cas * 0.3f * intensidad);
    main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    main.startColor = new Color(PolvoPaso.r, PolvoPaso.g, PolvoPaso.b, 0.5f);

    var shape = ps.shape;
    shape.radius = cas * 0.22f;

    var emision = ps.emission;
    emision.enabled = true;
    emision.rateOverTime = 85f;

    var vel = ps.velocityOverLifetime;
    vel.enabled = true;
    vel.space = ParticleSystemSimulationSpace.World;
    vel.x = new ParticleSystem.MinMaxCurve(-cas * 0.25f, cas * 0.25f);
    vel.y = new ParticleSystem.MinMaxCurve(cas * 0.08f, cas * 0.35f);
    vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

    var tam = ps.sizeOverLifetime;
    tam.enabled = true;
    tam.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 1.6f)));

    GulekGulVfx.AplicarGradiente(ps, PolvoPaso, 0.5f, 0.12f);
    ps.Play();

    GulekGulVfx.AlFrente(raiz);
  }

  /// <summary>Choque contra una unidad de la fila: polvo hacia adelante, escombros y un temblor corto.</summary>
  public static void ImpactoEmbestida(Vector3 posicion, float dirX)
  {
    Nube(posicion, 1.3f, 0.8f, PolvoPaso, dirX, 14);
    Escombros(posicion, 1.1f, PiedraPaso, dirX);
    GulekGulVfx.OndaDeChoque(posicion, 0.6f, new Color(PolvoPaso.r, PolvoPaso.g, PolvoPaso.b, 0.4f), 0.4f, 1);
    GulekGulVfx.SacudirCamara(0.3f, 0.14f);
  }

  /// <summary>Frenada al terminar la carrera: nube arrastrada en el sentido del movimiento.</summary>
  public static void Frenada(Vector3 posicion, float dirX)
  {
    Nube(posicion, 1.5f, 1.1f, PolvoPaso, dirX, 16);
    GulekGulVfx.OndaDeChoque(posicion, 0.7f, new Color(PolvoPaso.r, PolvoPaso.g, PolvoPaso.b, 0.3f), 0.45f, 1);
  }

  // ------------------------------------------------------------------ Garra y Mordisco

  /// <summary>Zarpazo: bocanada de polvo y esquirlas chicas en el punto del golpe.</summary>
  public static void Zarpazo(Vector3 posicion, bool critico)
  {
    Nube(posicion, critico ? 1f : 0.65f, 0.6f, PolvoPaso, 0f, 8);
    Escombros(posicion, critico ? 0.8f : 0.5f, PiedraPaso, 0f);
  }

  /// <summary>Mordisco: nube mas densa y mas esquirlas (aplasta la armadura).</summary>
  public static void Mordisco(Vector3 posicion, bool critico)
  {
    Nube(posicion, critico ? 1.4f : 0.95f, 0.7f, PolvoPaso, 0f, 10);
    Escombros(posicion, critico ? 1.3f : 0.9f, PiedraPaso, 0f);
    if (critico) { GulekGulVfx.SacudirCamara(0.3f, 0.12f); }
  }
}
