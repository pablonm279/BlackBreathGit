using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// VFX procedurales del Draco de Hielo: cono de aliento gelido, escarcha en el suelo, impacto de hielo,
/// aura de escamas y movimiento del picado. Reutiliza los helpers de particulas de GulekGulVfx.
/// Tamaños en "casillas" para escalar con el tablero.
/// </summary>
public static class DracoHieloVfx
{
  private static readonly Color HieloClaro = new Color(0.8f, 0.94f, 1f);
  private static readonly Color HieloIntenso = new Color(0.4f, 0.78f, 1f);
  private static readonly Color Nieve = new Color(0.97f, 0.99f, 1f);
  // Paleta del aliento: tira a celeste, con nucleo casi blanco.
  private static readonly Color Celeste = new Color(0.45f, 0.78f, 1f);
  private static readonly Color CelesteClaro = new Color(0.68f, 0.9f, 1f);
  private static readonly Color CelesteBrillo = new Color(0.82f, 0.96f, 1f);

  private static Sprite spriteEscarcha;

  // ------------------------------------------------------------------ escarcha en el suelo (sprite procedural)

  private static Sprite ObtenerSpriteEscarcha()
  {
    if (spriteEscarcha != null) { return spriteEscarcha; }

    const int size = 128;
    Color32[] px = new Color32[size * size];
    float c = (size - 1) * 0.5f;
    System.Random rng = new System.Random(4242);

    // Velo de escarcha suave que se aclara hacia el centro.
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float r = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
        float a = Mathf.Clamp01(1f - r);
        a = a * a * 0.5f;
        px[y * size + x] = new Color32(190, 228, 250, (byte)Mathf.RoundToInt(a * 255f));
      }
    }

    // Copo: 6 brazos con ramas.
    for (int i = 0; i < 6; i++)
    {
      float ang = i * Mathf.PI / 3f + 0.1f;
      Brazo(px, size, c, c, ang, 52f, 2.4f, rng, true);
    }

    // Esquirlas sueltas alrededor.
    for (int i = 0; i < 14; i++)
    {
      float ang = (float)rng.NextDouble() * Mathf.PI * 2f;
      float rad = 22f + (float)rng.NextDouble() * 38f;
      float sx = c + Mathf.Cos(ang) * rad;
      float sy = c + Mathf.Sin(ang) * rad;
      float dir = (float)rng.NextDouble() * Mathf.PI;
      float largo = 5f + (float)rng.NextDouble() * 8f;
      Segmento(px, size, sx, sy, sx + Mathf.Cos(dir) * largo, sy + Mathf.Sin(dir) * largo, 1.4f, new Color32(235, 248, 255, 255));
    }

    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
    {
      name = "DracoHielo_Escarcha",
      wrapMode = TextureWrapMode.Clamp,
      filterMode = FilterMode.Bilinear
    };
    tex.SetPixels32(px);
    tex.Apply(false, false);

    spriteEscarcha = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    return spriteEscarcha;
  }

  private static void Brazo(Color32[] px, int size, float x0, float y0, float angulo, float largo, float grosor, System.Random rng, bool conRamas)
  {
    float x1 = x0 + Mathf.Cos(angulo) * largo;
    float y1 = y0 + Mathf.Sin(angulo) * largo;

    // Halo azul translucido debajo y trazo blanco encima.
    Segmento(px, size, x0, y0, x1, y1, grosor * 2.2f, new Color32(120, 195, 245, 150));
    Segmento(px, size, x0, y0, x1, y1, grosor, new Color32(238, 249, 255, 255));

    if (!conRamas) { return; }

    foreach (float t in new[] { 0.4f, 0.68f })
    {
      float bx = Mathf.Lerp(x0, x1, t);
      float by = Mathf.Lerp(y0, y1, t);
      float largoRama = largo * (t < 0.5f ? 0.32f : 0.22f);
      Brazo(px, size, bx, by, angulo + 0.95f, largoRama, grosor * 0.7f, rng, false);
      Brazo(px, size, bx, by, angulo - 0.95f, largoRama, grosor * 0.7f, rng, false);
    }
  }

  private static void Segmento(Color32[] px, int size, float ax, float ay, float bx, float by, float grosor, Color32 color)
  {
    int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ax, bx) - grosor - 1f));
    int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(ax, bx) + grosor + 1f));
    int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(ay, by) - grosor - 1f));
    int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(ay, by) + grosor + 1f));

    float dx = bx - ax;
    float dy = by - ay;
    float largo2 = Mathf.Max(0.0001f, dx * dx + dy * dy);

    for (int y = minY; y <= maxY; y++)
    {
      for (int x = minX; x <= maxX; x++)
      {
        float t = Mathf.Clamp01(((x - ax) * dx + (y - ay) * dy) / largo2);
        float px2 = ax + dx * t;
        float py2 = ay + dy * t;
        float d = Mathf.Sqrt((x - px2) * (x - px2) + (y - py2) * (y - py2));
        float alpha = Mathf.Clamp01(1f - (d - grosor * 0.5f) / 1.1f) * (color.a / 255f);
        if (alpha <= 0f) { continue; }

        Color32 actual = px[y * size + x];
        byte a8 = (byte)Mathf.RoundToInt(alpha * 255f);
        if (a8 >= actual.a)
        {
          px[y * size + x] = new Color32(color.r, color.g, color.b, a8);
        }
      }
    }
  }

  /// <summary>Parche de escarcha sobre una casilla; dura unos segundos y se desvanece.</summary>
  public static void EscarchaEnCasilla(Vector3 posicion, float tamanoEnCasillas = 0.95f, float retraso = 0f, float duracion = 3.2f)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject go = new GameObject("DracoHielo_Escarcha");
    go.transform.position = posicion + new Vector3(0f, cas * 0.02f, 0f);

    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    sr.sprite = ObtenerSpriteEscarcha();
    sr.flipX = Random.value < 0.5f;
    sr.transform.rotation = Quaternion.identity;
    sr.sortingLayerID = 0;   // Default: sobre el tablero, bajo las unidades
    sr.sortingOrder = 5;
    sr.color = new Color(1f, 1f, 1f, 0f);

    GulekGulAnimadorGrietas anim = go.AddComponent<GulekGulAnimadorGrietas>();
    anim.Configurar(sr, cas * tamanoEnCasillas, 0.46f, retraso, duracion);
  }

  // ------------------------------------------------------------------ aliento

  private static ParticleSystem Emisor(Transform padre, string nombre, float duracion, int max, float tasa)
  {
    ParticleSystem ps = GulekGulVfx.CrearSistema(padre, nombre, duracion, max);
    var emision = ps.emission;
    emision.enabled = true;
    emision.rateOverTime = tasa;

    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Cone;
    return ps;
  }

  /// <summary>
  /// Cono de aliento gelido desde la boca hacia el centro de la zona: nucleo brillante, niebla que se expande
  /// y destellos de nieve. Dura 'duracion' segundos de emision.
  /// </summary>
  public static void AlientoCono(Vector3 origen, Vector3 centroDestino, float duracion = 0.9f)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    Vector3 dir = centroDestino - origen;
    dir.z = 0f;
    if (dir.sqrMagnitude < 0.0001f) { dir = Vector3.left; }
    float distancia = dir.magnitude;
    dir.Normalize();

    float alcance = Mathf.Max(distancia * 1.25f, cas * 4f);

    GameObject raiz = GulekGulVfx.CrearRaiz("Draco_Aliento", origen, duracion + 2f);
    raiz.transform.rotation = Quaternion.LookRotation(dir, Vector3.back); // +Z local = direccion del aliento

    // Niebla gelida: grande, suave y lenta. Es el cuerpo del aliento.
    ParticleSystem niebla = Emisor(raiz.transform, "Niebla", duracion, 220, 110f);
    var mn = niebla.main;
    float velNiebla = cas * 4.2f;
    mn.startSpeed = new ParticleSystem.MinMaxCurve(velNiebla * 0.7f, velNiebla * 1.1f);
    mn.startLifetime = new ParticleSystem.MinMaxCurve(alcance / velNiebla * 0.9f, alcance / velNiebla * 1.3f);
    mn.startSize = new ParticleSystem.MinMaxCurve(cas * 0.24f, cas * 0.5f);
    mn.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    mn.startColor = new ParticleSystem.MinMaxGradient(
      new Color(CelesteClaro.r, CelesteClaro.g, CelesteClaro.b, 0.5f),
      new Color(Celeste.r, Celeste.g, Celeste.b, 0.45f));
    var sn = niebla.shape;
    sn.angle = 20f;
    sn.radius = cas * 0.16f;
    var tn = niebla.sizeOverLifetime;
    tn.enabled = true;
    tn.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.35f), new Keyframe(1f, 2.1f)));
    GulekGulVfx.AplicarGradiente(niebla, CelesteClaro, 0.6f, 0.15f);
    niebla.Play();

    // Nucleo: chorro rapido, fino y brillante.
    ParticleSystem nucleo = Emisor(raiz.transform, "Nucleo", duracion, 160, 150f);
    var mc = nucleo.main;
    float velNucleo = cas * 7.5f;
    mc.startSpeed = new ParticleSystem.MinMaxCurve(velNucleo * 0.85f, velNucleo * 1.15f);
    mc.startLifetime = new ParticleSystem.MinMaxCurve(alcance / velNucleo * 0.7f, alcance / velNucleo * 1.0f);
    mc.startSize = new ParticleSystem.MinMaxCurve(cas * 0.08f, cas * 0.17f);
    mc.startColor = new ParticleSystem.MinMaxGradient(CelesteBrillo, CelesteClaro);
    var sc = nucleo.shape;
    sc.angle = 9f;
    sc.radius = cas * 0.08f;
    GulekGulVfx.AplicarGradiente(nucleo, CelesteBrillo, 0.95f, 0.08f);
    nucleo.Play();

    // Destellos de nieve / cristales que se dispersan por los lados y caen.
    ParticleSystem nieve = Emisor(raiz.transform, "Cristales", duracion, 140, 85f);
    var mv = nieve.main;
    float velNieve = cas * 4.8f;
    mv.startSpeed = new ParticleSystem.MinMaxCurve(velNieve * 0.4f, velNieve * 1.2f);
    mv.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
    mv.startSize = new ParticleSystem.MinMaxCurve(cas * 0.017f, cas * 0.048f);
    mv.startColor = new ParticleSystem.MinMaxGradient(CelesteBrillo, new Color(Celeste.r, Celeste.g, Celeste.b, 1f));
    mv.gravityModifier = 0.45f;
    var sv = nieve.shape;
    sv.angle = 26f;
    sv.radius = cas * 0.2f;
    GulekGulVfx.AplicarGradiente(nieve, CelesteBrillo, 1f, 0.06f);
    nieve.Play();

    // Brillo: particulas grandes, suaves y muy tenues sobre el eje del chorro (resplandor).
    ParticleSystem brillo = Emisor(raiz.transform, "Brillo", duracion, 80, 55f);
    var mbr = brillo.main;
    mbr.startSpeed = new ParticleSystem.MinMaxCurve(velNucleo * 0.45f, velNucleo * 0.75f);
    mbr.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.4f);
    mbr.startSize = new ParticleSystem.MinMaxCurve(cas * 0.4f, cas * 0.7f);
    mbr.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    mbr.startColor = new Color(CelesteBrillo.r, CelesteBrillo.g, CelesteBrillo.b, 0.3f);
    var sbr = brillo.shape;
    sbr.angle = 5f;
    sbr.radius = cas * 0.06f;
    GulekGulVfx.AplicarGradiente(brillo, CelesteBrillo, 0.32f, 0.2f);
    brillo.Play();

    // Fogonazo en la boca.
    GulekGulVfx.OndaDeChoque(origen, 0.5f, new Color(CelesteClaro.r, CelesteClaro.g, CelesteClaro.b, 0.6f), 0.35f, 1);

    GulekGulVfx.AlFrente(raiz);
  }

  /// <summary>Preparacion del aliento: nieve y aire frio que se concentran en la boca.</summary>
  public static void CargaAliento(Vector3 boca, float duracion = 0.45f)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Draco_CargaAliento", boca, duracion + 1f);

    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Carga", duracion, 40);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(-cas * 1.2f, -cas * 0.6f); // hacia adentro
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.05f, cas * 0.12f);
    main.startColor = new ParticleSystem.MinMaxGradient(Nieve, HieloIntenso);
    var emision = ps.emission;
    emision.enabled = true;
    emision.rateOverTime = 80f;
    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Sphere;
    shape.radius = cas * 0.5f;
    GulekGulVfx.AplicarGradiente(ps, HieloClaro, 1f, 0.3f);
    ps.Play();

    GulekGulVfx.AlFrente(raiz);
  }

  // ------------------------------------------------------------------ impactos y auras

  /// <summary>Estallido de esquirlas de hielo y escarcha sobre una unidad golpeada.</summary>
  public static void ImpactoHielo(Vector3 posicion, float intensidad = 1f, bool congelado = false)
  {
    intensidad = Mathf.Clamp(intensidad, 0.4f, 2f);
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Draco_ImpactoHielo", posicion, 1.6f);

    ParticleSystem esquirlas = GulekGulVfx.CrearSistema(raiz.transform, "Esquirlas", 0.4f, 40);
    var me = esquirlas.main;
    me.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.85f);
    me.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.9f, cas * 2.4f);
    me.startSize = new ParticleSystem.MinMaxCurve(cas * 0.03f, cas * 0.09f);
    me.startColor = new ParticleSystem.MinMaxGradient(Nieve, HieloIntenso);
    me.gravityModifier = 1.6f;
    var se = esquirlas.shape;
    se.shapeType = ParticleSystemShapeType.Cone;
    se.angle = 55f;
    se.radius = cas * 0.15f;
    se.rotation = new Vector3(-90f, 0f, 0f);
    GulekGulVfx.AplicarGradiente(esquirlas, Nieve, 1f, 0.05f);
    esquirlas.Play();
    esquirlas.Emit(Mathf.RoundToInt(16 * intensidad));

    ParticleSystem bruma = GulekGulVfx.CrearSistema(raiz.transform, "Bruma", 0.4f, 20);
    var mb = bruma.main;
    mb.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
    mb.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.1f, cas * 0.4f);
    mb.startSize = new ParticleSystem.MinMaxCurve(cas * 0.2f * intensidad, cas * 0.4f * intensidad);
    mb.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    mb.startColor = new Color(HieloClaro.r, HieloClaro.g, HieloClaro.b, 0.5f);
    var sb = bruma.shape;
    sb.radius = cas * 0.3f;
    var vb = bruma.velocityOverLifetime;
    vb.enabled = true;
    vb.space = ParticleSystemSimulationSpace.World;
    vb.x = new ParticleSystem.MinMaxCurve(-cas * 0.3f, cas * 0.3f);
    vb.y = new ParticleSystem.MinMaxCurve(cas * 0.1f, cas * 0.4f);
    vb.z = new ParticleSystem.MinMaxCurve(0f, 0f);
    GulekGulVfx.AplicarGradiente(bruma, HieloClaro, 0.5f);
    bruma.Play();
    bruma.Emit(Mathf.RoundToInt(8 * intensidad));

    if (congelado)
    {
      GulekGulVfx.OndaDeChoque(posicion, 0.55f, new Color(HieloIntenso.r, HieloIntenso.g, HieloIntenso.b, 0.6f), 0.4f, 2);
    }

    GulekGulVfx.AlFrente(raiz);
  }

  /// <summary>Aura de escamas de escarcha: destellos que suben alrededor del draco y un halo que pulsa.</summary>
  public static void AuraEscamas(Transform objetivo, float duracion = 2f)
  {
    if (objetivo == null) { return; }

    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Draco_AuraEscamas", objetivo.position, duracion + 1f);
    raiz.transform.SetParent(objetivo, true);

    GameObject halo = new GameObject("Halo");
    halo.transform.SetParent(raiz.transform, false);
    SpriteRenderer sr = halo.AddComponent<SpriteRenderer>();
    sr.sprite = ObtenerSpriteEscarcha();
    sr.color = new Color(1f, 1f, 1f, 0f);
    GulekGulAnimadorAnillo animHalo = halo.AddComponent<GulekGulAnimadorAnillo>();
    animHalo.Configurar(sr, cas * 2.6f, 0.9f, duracion, 0f, 0.6f);
    animHalo.pulsos = 2;

    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Destellos", duracion, 50);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.1f, cas * 0.4f);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.04f, cas * 0.1f);
    main.startColor = new ParticleSystem.MinMaxGradient(Nieve, HieloIntenso);
    main.gravityModifier = -0.15f;
    var emision = ps.emission;
    emision.enabled = true;
    emision.rateOverTime = 30f;
    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Circle;
    shape.radius = cas * 0.6f;
    GulekGulVfx.AplicarGradiente(ps, Nieve, 1f, 0.15f);
    ps.Play();

    GulekGulVfx.AlFrente(raiz);
  }

  /// <summary>Estela de aire frio que acompaña un picado.</summary>
  public static void EstelaPicado(Transform unidad, float duracion)
  {
    if (unidad == null) { return; }

    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Draco_EstelaPicado", unidad.position, duracion + 1.2f);
    raiz.transform.SetParent(unidad, true);

    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Estela", Mathf.Max(0.1f, duracion), 70);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.05f, cas * 0.3f);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.1f, cas * 0.26f);
    main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    main.startColor = new Color(HieloClaro.r, HieloClaro.g, HieloClaro.b, 0.5f);
    var emision = ps.emission;
    emision.enabled = true;
    emision.rateOverTime = 90f;
    var shape = ps.shape;
    shape.radius = cas * 0.25f;
    GulekGulVfx.AplicarGradiente(ps, HieloClaro, 0.5f, 0.12f);
    ps.Play();

    GulekGulVfx.AlFrente(raiz);
  }

  // ------------------------------------------------------------------ movimiento

  /// <summary>Factor de tiempo de las habilidades (menor que 1 en modo rapido).</summary>
  public static float FactorTiempo()
  {
    BattleManager batalla = BattleManager.Instance;
    return batalla != null && batalla.modoRapidoActivado
      ? GameSpeedSettings.CombatAiTimingFactor(batalla.NivelVelocidadCombateEfectivo)
      : 1f;
  }

  /// <summary>Mueve un transform en linea recta respetando la pausa del combate. Termina si el objeto se destruye.</summary>
  public static async Task MoverAsync(Transform t, Vector3 desde, Vector3 hasta, float duracion, bool suavizado = true)
  {
    if (t == null) { return; }

    if (duracion <= 0f)
    {
      t.position = hasta;
      return;
    }

    float tiempo = 0f;
    while (tiempo < duracion)
    {
      if (t == null) { return; }

      if (BattleManager.Instance != null && BattleManager.Instance.PausaCombateActiva)
      {
        await Task.Yield();
        continue;
      }

      tiempo += Time.deltaTime;
      float k = Mathf.Clamp01(tiempo / duracion);
      if (suavizado) { k = k * k * (3f - 2f * k); }
      t.position = Vector3.Lerp(desde, hasta, k);
      await Task.Yield();
    }

    if (t != null) { t.position = hasta; }
  }
}
