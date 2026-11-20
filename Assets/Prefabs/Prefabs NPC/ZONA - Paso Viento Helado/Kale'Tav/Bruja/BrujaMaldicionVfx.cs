using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// VFX procedurales de la Descarga Maldita de la Bruja Kale'Tav: estilo ritual pagano en rojo.
/// Circulo runico, orbe sangriento con estela de brasas, impacto y marca persistente sobre el maldito.
/// </summary>
public static class BrujaMaldicionVfx
{
  // Todos los tamaños de esta habilidad se multiplican por E (30% mas chicos).
  public const float E = 0.63f; // 0,7 anterior, otro 10% mas chico

  public static readonly Color Rojo = new Color(1f, 0.16f, 0.16f);
  public static readonly Color RojoOscuro = new Color(0.42f, 0.04f, 0.07f);
  public static readonly Color Brasa = new Color(1f, 0.52f, 0.2f);
  public static readonly Color Brillo = new Color(1f, 0.78f, 0.55f);

  private static Sprite spriteRuna;
  private static Sprite spriteCirculoSuave;

  // ------------------------------------------------------------------ sprites procedurales

  public static Sprite ObtenerCirculoSuave()
  {
    if (spriteCirculoSuave != null) { return spriteCirculoSuave; }

    const int size = 64;
    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
    {
      name = "BrujaMaldicion_CirculoSuave",
      wrapMode = TextureWrapMode.Clamp,
      filterMode = FilterMode.Bilinear
    };

    float half = (size - 1) * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float r = Mathf.Sqrt((x - half) * (x - half) + (y - half) * (y - half)) / half;
        float a = Mathf.Clamp01(1f - r);
        a = a * a * (3f - 2f * a);
        tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
      }
    }

    tex.Apply(false, false);
    spriteCirculoSuave = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    return spriteCirculoSuave;
  }

  /// <summary>Circulo ritual: doble anillo, runas en la banda, y tres circulos entrelazados al centro.</summary>
  public static Sprite ObtenerSpriteRuna()
  {
    if (spriteRuna != null) { return spriteRuna; }

    const int size = 192;
    Color32[] px = new Color32[size * size];
    float c = (size - 1) * 0.5f;
    System.Random rng = new System.Random(666);
    Color32 trazo = new Color32(255, 62, 58, 255);

    // Velo rojo muy tenue dentro del circulo.
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float r = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / 88f;
        if (r > 1f) { continue; }
        float a = (1f - r) * 0.24f;
        px[y * size + x] = new Color32(220, 24, 28, (byte)Mathf.RoundToInt(a * 255f));
      }
    }

    Circulo(px, size, c, c, 90f, 2.4f, trazo);
    Circulo(px, size, c, c, 72f, 1.6f, trazo);

    // 12 runas en la banda entre los anillos: trazo radial con ramas cortas.
    for (int i = 0; i < 12; i++)
    {
      float ang = i * Mathf.PI * 2f / 12f;
      Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
      Vector2 perp = new Vector2(-dir.y, dir.x);
      Vector2 a0 = new Vector2(c, c) + dir * 76f;
      Vector2 a1 = new Vector2(c, c) + dir * 87f;
      Segmento(px, size, a0.x, a0.y, a1.x, a1.y, 1.8f, trazo);

      int ramas = 1 + rng.Next(0, 2);
      for (int r = 0; r < ramas; r++)
      {
        float t = 0.25f + (float)rng.NextDouble() * 0.5f;
        Vector2 baseP = Vector2.Lerp(a0, a1, t);
        float lado = rng.NextDouble() < 0.5 ? -1f : 1f;
        Vector2 fin = baseP + perp * lado * (4f + (float)rng.NextDouble() * 3f) + dir * (3f + (float)rng.NextDouble() * 3f);
        Segmento(px, size, baseP.x, baseP.y, fin.x, fin.y, 1.5f, trazo);
      }
    }

    // Tres circulos entrelazados (triquetra) al centro.
    for (int i = 0; i < 3; i++)
    {
      float ang = Mathf.PI * 0.5f + i * Mathf.PI * 2f / 3f;
      float cx = c + Mathf.Cos(ang) * 17f;
      float cy = c + Mathf.Sin(ang) * 17f;
      Circulo(px, size, cx, cy, 30f, 1.8f, trazo);
    }

    // Punto central.
    Circulo(px, size, c, c, 2.5f, 3f, trazo);

    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
    {
      name = "BrujaMaldicion_Runa",
      wrapMode = TextureWrapMode.Clamp,
      filterMode = FilterMode.Bilinear
    };
    tex.SetPixels32(px);
    tex.Apply(false, false);

    spriteRuna = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    return spriteRuna;
  }

  private static void Circulo(Color32[] px, int size, float cx, float cy, float radio, float grosor, Color32 color)
  {
    int pasos = Mathf.Max(24, Mathf.RoundToInt(radio * 1.2f));
    float prevX = cx + radio;
    float prevY = cy;
    for (int i = 1; i <= pasos; i++)
    {
      float a = i * Mathf.PI * 2f / pasos;
      float x = cx + Mathf.Cos(a) * radio;
      float y = cy + Mathf.Sin(a) * radio;
      Segmento(px, size, prevX, prevY, x, y, grosor, color);
      prevX = x;
      prevY = y;
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
        float qx = ax + dx * t;
        float qy = ay + dy * t;
        float d = Mathf.Sqrt((x - qx) * (x - qx) + (y - qy) * (y - qy));
        float alpha = Mathf.Clamp01(1f - (d - grosor * 0.5f) / 1.1f);
        if (alpha <= 0f) { continue; }

        byte a8 = (byte)Mathf.RoundToInt(alpha * 255f);
        Color32 actual = px[y * size + x];
        if (a8 >= actual.a)
        {
          px[y * size + x] = new Color32(color.r, color.g, color.b, a8);
        }
      }
    }
  }

  // ------------------------------------------------------------------ ritual de lanzamiento

  /// <summary>Circulo runico bajo la bruja y brasas que se concentran en sus manos mientras canaliza.</summary>
  public static void ConvocarRitual(Vector3 piesBruja, Vector3 manos, float duracion = 0.45f)
  {
    float cas = GulekGulVfx.TamanoCasilla();

    // Circulo runico en el suelo (aparece de golpe, se mantiene y se desvanece).
    GameObject go = new GameObject("Bruja_CirculoRitual");
    go.transform.position = piesBruja + new Vector3(0f, cas * 0.02f, 0f);
    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    sr.sprite = ObtenerSpriteRuna();
    sr.sortingLayerID = 0;
    sr.sortingOrder = 5;
    sr.color = new Color(1f, 1f, 1f, 0f);
    GulekGulAnimadorGrietas anim = go.AddComponent<GulekGulAnimadorGrietas>();
    anim.Configurar(sr, cas * 1.5f * E, 0.46f, 0f, 1.4f);

    // Brasas rojas que convergen en las manos.
    GameObject raiz = GulekGulVfx.CrearRaiz("Bruja_CargaRitual", manos, duracion + 1f);
    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Carga", duracion, 40);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(-cas * 1.4f * E, -cas * 0.7f * E);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.035f * E, cas * 0.09f * E);
    main.startColor = new ParticleSystem.MinMaxGradient(Brillo, Brasa);
    var emision = ps.emission;
    emision.enabled = true;
    emision.rateOverTime = 90f;
    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Sphere;
    shape.radius = cas * 0.5f * E;
    GulekGulVfx.AplicarGradiente(ps, Brasa, 1f, 0.3f);
    ps.Play();
    GulekGulVfx.AlFrente(raiz);
  }

  // ------------------------------------------------------------------ orbe

  /// <summary>Orbe sangriento con estela de brasas y humo oscuro. Completa al llegar (true).</summary>
  public static Task<bool> LanzarOrbe(Vector3 origen, Vector3 destino, float duracion)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject go = new GameObject("Bruja_OrbeMaldito");
    go.transform.position = origen;

    SpriteRenderer halo = new GameObject("Halo").AddComponent<SpriteRenderer>();
    halo.transform.SetParent(go.transform, false);
    halo.sprite = ObtenerCirculoSuave();
    halo.color = new Color(Rojo.r, Rojo.g, Rojo.b, 0.8f);
    float escHalo = cas * 0.75f * E / Mathf.Max(0.01f, halo.sprite.bounds.size.x);
    halo.transform.localScale = new Vector3(escHalo, escHalo, 1f);

    SpriteRenderer nucleo = new GameObject("Nucleo").AddComponent<SpriteRenderer>();
    nucleo.transform.SetParent(go.transform, false);
    nucleo.sprite = ObtenerCirculoSuave();
    nucleo.color = new Color(RojoOscuro.r, RojoOscuro.g, RojoOscuro.b, 0.95f);
    float escNucleo = cas * 0.3f * E / Mathf.Max(0.01f, nucleo.sprite.bounds.size.x);
    nucleo.transform.localScale = new Vector3(escNucleo, escNucleo, 1f);

    // Resplandor exterior y punto incandescente al centro del orbe.
    SpriteRenderer resplandor = new GameObject("Resplandor").AddComponent<SpriteRenderer>();
    resplandor.transform.SetParent(go.transform, false);
    resplandor.sprite = ObtenerCirculoSuave();
    resplandor.color = new Color(Brasa.r, Brasa.g, Brasa.b, 0.3f);
    float escResplandor = cas * 1.25f * E / Mathf.Max(0.01f, resplandor.sprite.bounds.size.x);
    resplandor.transform.localScale = new Vector3(escResplandor, escResplandor, 1f);
    resplandor.transform.SetAsFirstSibling();

    SpriteRenderer chispa = new GameObject("Chispa").AddComponent<SpriteRenderer>();
    chispa.transform.SetParent(go.transform, false);
    chispa.sprite = ObtenerCirculoSuave();
    chispa.color = new Color(Brillo.r, Brillo.g, Brillo.b, 0.95f);
    float escChispa = cas * 0.13f * E / Mathf.Max(0.01f, chispa.sprite.bounds.size.x);
    chispa.transform.localScale = new Vector3(escChispa, escChispa, 1f);

    // Brasas.
    ParticleSystem brasas = GulekGulVfx.CrearSistema(go.transform, "Brasas", 3f, 90);
    var mb = brasas.main;
    mb.loop = true;
    mb.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
    mb.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.1f * E, cas * 0.5f * E);
    mb.startSize = new ParticleSystem.MinMaxCurve(cas * 0.03f * E, cas * 0.08f * E);
    mb.startColor = new ParticleSystem.MinMaxGradient(Brillo, Brasa);
    var eb = brasas.emission;
    eb.enabled = true;
    eb.rateOverTime = 80f;
    var sb = brasas.shape;
    sb.radius = cas * 0.12f * E;
    GulekGulVfx.AplicarGradiente(brasas, Brasa, 1f, 0.1f);
    brasas.Play();

    // Humo oscuro rojizo.
    ParticleSystem humo = GulekGulVfx.CrearSistema(go.transform, "Humo", 3f, 60);
    var mh = humo.main;
    mh.loop = true;
    mh.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.75f);
    mh.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.05f * E, cas * 0.25f * E);
    mh.startSize = new ParticleSystem.MinMaxCurve(cas * 0.18f * E, cas * 0.34f * E);
    mh.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    mh.startColor = new Color(RojoOscuro.r, RojoOscuro.g, RojoOscuro.b, 0.5f);
    var eh = humo.emission;
    eh.enabled = true;
    eh.rateOverTime = 36f;
    var sh = humo.shape;
    sh.radius = cas * 0.16f * E;
    GulekGulVfx.AplicarGradiente(humo, RojoOscuro, 0.45f, 0.15f);
    humo.Play();

    GulekGulVfx.AlFrente(go);

    BrujaOrbeMaldito vuelo = go.AddComponent<BrujaOrbeMaldito>();
    return vuelo.Iniciar(halo, nucleo, origen, destino, duracion, cas * E);
  }

  // ------------------------------------------------------------------ impacto

  /// <summary>Estallido rojo: brasas, onda y un sello runico que destella en los pies del objetivo.</summary>
  public static void ImpactoMaldicion(Vector3 pos, Vector3 pies, bool critico)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    float k = critico ? 1.4f : 1f;

    GameObject raiz = GulekGulVfx.CrearRaiz("Bruja_ImpactoMaldicion", pos, 1.6f);
    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Brasas", 0.4f, 40);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.85f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.8f * E, cas * 2.2f * E);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.03f * E, cas * 0.085f * E);
    main.startColor = new ParticleSystem.MinMaxGradient(Brillo, Brasa);
    main.gravityModifier = 0.9f;
    var shape = ps.shape;
    shape.shapeType = ParticleSystemShapeType.Sphere;
    shape.radius = cas * 0.12f * E;
    GulekGulVfx.AplicarGradiente(ps, Brasa, 1f, 0.05f);
    ps.Play();
    ps.Emit(Mathf.RoundToInt(18 * k));
    GulekGulVfx.AlFrente(raiz);

    GulekGulVfx.OndaDeChoque(pos, 0.6f * k * E, new Color(Rojo.r, Rojo.g, Rojo.b, 0.85f), 0.4f, 2);

    // Sello runico que destella en el suelo.
    GameObject go = new GameObject("Bruja_SelloImpacto");
    go.transform.position = pies + new Vector3(0f, cas * 0.02f, 0f);
    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    sr.sprite = ObtenerSpriteRuna();
    sr.sortingLayerID = 0;
    sr.sortingOrder = 6;
    sr.color = new Color(1f, 1f, 1f, 0f);
    GulekGulAnimadorGrietas anim = go.AddComponent<GulekGulAnimadorGrietas>();
    anim.Configurar(sr, cas * 1.2f * k * E, 0.46f, 0f, 1.1f);

    GulekGulVfx.SacudirCamara(critico ? 0.35f : 0.2f, 0.12f);
  }

  /// <summary>Marca persistente: circulo runico tenue + brasas lentas mientras dure el buff.</summary>
  public static void AdjuntarMaldicion(Unidad unidad, string nombreBuff)
  {
    if (unidad == null) { return; }

    MaldicionRitualFx fx = unidad.GetComponent<MaldicionRitualFx>();
    if (fx != null)
    {
      fx.Reiniciar(nombreBuff);
      return;
    }

    unidad.gameObject.AddComponent<MaldicionRitualFx>().Iniciar(unidad, nombreBuff);
  }
}

/// <summary>Orbe maldito: vuelo con ondulacion, pulso y regreso a true al llegar.</summary>
public class BrujaOrbeMaldito : MonoBehaviour
{
  private readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
  private SpriteRenderer halo;
  private SpriteRenderer nucleo;
  private Vector3 origen;
  private Vector3 destino;
  private float duracion;
  private float cas;
  private float t;
  private float escalaHalo;

  public Task<bool> Iniciar(SpriteRenderer haloRenderer, SpriteRenderer nucleoRenderer, Vector3 desde, Vector3 hasta, float duracionVuelo, float tamanoCasilla)
  {
    halo = haloRenderer;
    nucleo = nucleoRenderer;
    origen = desde;
    destino = hasta;
    duracion = Mathf.Max(0.1f, duracionVuelo);
    cas = tamanoCasilla;
    escalaHalo = halo != null ? halo.transform.localScale.x : 1f;
    return tcs.Task;
  }

  private void Update()
  {
    if (BattleManager.Instance != null && BattleManager.Instance.PausaCombateActiva) { return; }

    t += Time.deltaTime;
    float k = Mathf.Clamp01(t / duracion);
    float suave = k * k * 0.4f + k * 0.6f;

    Vector3 eje = destino - origen;
    Vector3 perp = new Vector3(-eje.y, eje.x, 0f).normalized;
    Vector3 pos = Vector3.Lerp(origen, destino, suave);
    // Ondulacion serpenteante que se apaga al llegar.
    pos += perp * Mathf.Sin(k * Mathf.PI * 4f) * cas * 0.12f * (1f - k);
    pos.y += Mathf.Sin(k * Mathf.PI) * cas * 0.25f;
    transform.position = pos;

    // Latido del halo.
    if (halo != null)
    {
      float pulso = 1f + Mathf.Sin(t * 30f) * 0.12f;
      halo.transform.localScale = new Vector3(escalaHalo * pulso, escalaHalo * pulso, 1f);
    }

    if (k >= 1f)
    {
      tcs.TrySetResult(true);
      Destroy(gameObject);
    }
  }

  private void OnDestroy()
  {
    tcs.TrySetResult(true);
  }
}

/// <summary>
/// Marca visual persistente sobre una unidad maldita: circulo runico rojo tenue que gira despacio y brasas lentas.
/// Se apaga sola cuando el buff desaparece (o la unidad muere).
/// </summary>
public class MaldicionRitualFx : MonoBehaviour
{
  private const float AlphaMaximo = 0.48f;

  private Unidad unidad;
  private string nombreBuff;
  private GameObject contenedor;     // escala achatada (suelo)
  private Transform giro;            // rota dentro del contenedor
  private SpriteRenderer runa;
  private GameObject brasasGO;
  private ParticleSystem brasas;
  private float alpha;
  private float proximoChequeo;
  private bool terminando;
  private float tiempo;
  private float cas;

  public void Iniciar(Unidad objetivo, string buff)
  {
    unidad = objetivo;
    nombreBuff = buff;
    cas = GulekGulVfx.TamanoCasilla();

    contenedor = new GameObject("Maldicion_Runa");
    contenedor.transform.localScale = new Vector3(cas * 1.1f * BrujaMaldicionVfx.E, cas * 1.1f * BrujaMaldicionVfx.E * 0.46f, 1f);

    GameObject g = new GameObject("Giro");
    g.transform.SetParent(contenedor.transform, false);
    giro = g.transform;
    runa = g.AddComponent<SpriteRenderer>();
    runa.sprite = BrujaMaldicionVfx.ObtenerSpriteRuna();
    runa.sortingLayerID = 0;
    runa.sortingOrder = 4;
    runa.color = new Color(1f, 1f, 1f, 0f);

    brasasGO = new GameObject("Maldicion_Brasas");
    brasas = brasasGO.AddComponent<ParticleSystem>();
    brasas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    ParticleSystemRenderer rend = brasasGO.GetComponent<ParticleSystemRenderer>();
    rend.sharedMaterial = ObtenerMaterialBrasas();
    rend.renderMode = ParticleSystemRenderMode.Billboard;

    var main = brasas.main;
    main.playOnAwake = false;
    main.loop = true;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.5f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.05f * BrujaMaldicionVfx.E, cas * 0.18f * BrujaMaldicionVfx.E);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.025f * BrujaMaldicionVfx.E, cas * 0.06f * BrujaMaldicionVfx.E);
    main.startColor = new ParticleSystem.MinMaxGradient(BrujaMaldicionVfx.Brillo, BrujaMaldicionVfx.Brasa);
    main.gravityModifier = -0.12f;
    main.maxParticles = 30;

    var emision = brasas.emission;
    emision.enabled = true;
    emision.rateOverTime = 5f;

    var shape = brasas.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Circle;
    shape.radius = cas * 0.3f * BrujaMaldicionVfx.E;

    GulekGulVfx.AplicarGradiente(brasas, BrujaMaldicionVfx.Brasa, 0.7f, 0.2f);
    brasas.Play();

    RenderOrderHelper.ForzarProyectilAlFrente(brasasGO);
    Colocar();
  }

  public void Reiniciar(string buff)
  {
    nombreBuff = buff;
    terminando = false;
  }

  private Vector3 PosicionPies()
  {
    if (unidad == null) { return transform.position; }
    if (unidad.bGrande || unidad.CasillaPosicion == null) { return unidad.transform.position; }
    return unidad.CasillaPosicion.transform.position;
  }

  private void Colocar()
  {
    Vector3 pies = PosicionPies() + new Vector3(0f, cas * 0.02f, 0f);
    if (contenedor != null) { contenedor.transform.position = pies; }
    if (brasasGO != null) { brasasGO.transform.position = pies; }
  }

  private void Update()
  {
    if (unidad == null) { Destroy(this); return; }

    tiempo += Time.deltaTime;

    if (Time.time >= proximoChequeo)
    {
      proximoChequeo = Time.time + 0.2f;
      if (!unidad.TieneBuffNombre(nombreBuff) || unidad.HP_actual <= 0f)
      {
        terminando = true;
      }
    }

    float objetivoAlpha = terminando ? 0f : AlphaMaximo;
    alpha = Mathf.MoveTowards(alpha, objetivoAlpha, Time.deltaTime * 1.2f);

    float pulso = 0.85f + 0.15f * Mathf.Sin(tiempo * 2.2f);
    if (runa != null) { runa.color = new Color(1f, 1f, 1f, alpha * pulso); }
    if (giro != null) { giro.localRotation = Quaternion.Euler(0f, 0f, -tiempo * 14f); }

    if (brasas != null)
    {
      var emision = brasas.emission;
      emision.rateOverTime = terminando ? 0f : 5f;
    }

    Colocar();

    if (terminando && alpha <= 0.005f)
    {
      Destroy(this);
    }
  }

  private void OnDestroy()
  {
    if (contenedor != null) { Destroy(contenedor); }
    if (brasasGO != null) { Destroy(brasasGO, 1.5f); }
  }

  private static Material materialBrasas;

  private static Material ObtenerMaterialBrasas()
  {
    if (materialBrasas != null) { return materialBrasas; }

    Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
    if (shader == null) { shader = Shader.Find("Sprites/Default"); }
    if (shader == null) { return null; }

    materialBrasas = new Material(shader) { name = "BrujaMaldicion_Brasas" };
    if (materialBrasas.HasProperty("_MainTex")) { materialBrasas.mainTexture = BrujaMaldicionVfx.ObtenerCirculoSuave().texture; }
    if (materialBrasas.HasProperty("_Color")) { materialBrasas.color = Color.white; }
    return materialBrasas;
  }
}
