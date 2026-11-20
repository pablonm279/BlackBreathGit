using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Efectos visuales procedurales de Gulek-Gul (Pisoton, Lanzar Roca, Furia).
/// No dependen de prefabs: todo se construye en runtime (particulas, anillos, proyectil).
/// Todos los tamaños se expresan en "casillas" para escalar solos con el tablero.
/// </summary>
public static class GulekGulVfx
{
  private static Material materialParticulas;
  private static Texture2D texturaCirculo;
  private static Sprite spriteSombraRoca;
  private static Texture2D texturaAnillo;
  private static Sprite spriteAnillo;
  private static float tamanoCasillaCacheado;

  // ------------------------------------------------------------------ utilidades

  /// <summary>Distancia en mundo entre dos casillas contiguas (con fallback razonable).</summary>
  public static float TamanoCasilla()
  {
    if (tamanoCasillaCacheado > 0.01f)
    {
      return tamanoCasillaCacheado;
    }

    BattleManager bm = BattleManager.Instance;
    if (bm != null && bm.lCasillasTotal != null)
    {
      Casilla a = null;
      Casilla b = null;
      foreach (Casilla c in bm.lCasillasTotal)
      {
        if (c == null) { continue; }
        if (a == null && c.posX == 1 && c.posY == 1) { a = c; }
        if (b == null && c.posX == 2 && c.posY == 1 && a != null && c.lado == a.lado) { b = c; }
      }

      if (a != null && b != null)
      {
        float d = Vector3.Distance(a.transform.position, b.transform.position);
        if (d > 0.01f)
        {
          tamanoCasillaCacheado = d;
          return d;
        }
      }
    }

    return 0.5f;
  }

  public static void SacudirCamara(float intensidad, float duracion)
  {
    BattleVisualJuiceRuntime.Instance?.RequestCameraShake(intensidad, duracion);
  }

  private static Material ObtenerMaterial()
  {
    if (materialParticulas != null)
    {
      return materialParticulas;
    }

    Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
    if (shader == null) { shader = Shader.Find("Sprites/Default"); }
    if (shader == null) { return null; }

    materialParticulas = new Material(shader) { name = "GulekGul_Particulas" };
    if (materialParticulas.HasProperty("_MainTex")) { materialParticulas.mainTexture = ObtenerTexturaCirculo(); }
    if (materialParticulas.HasProperty("_Color")) { materialParticulas.color = Color.white; }
    return materialParticulas;
  }

  private static Texture2D ObtenerTexturaCirculo()
  {
    if (texturaCirculo != null) { return texturaCirculo; }

    const int size = 64;
    texturaCirculo = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
    {
      name = "GulekGul_Circulo",
      wrapMode = TextureWrapMode.Clamp,
      filterMode = FilterMode.Bilinear
    };

    float half = (size - 1) * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float r = Mathf.Sqrt(Mathf.Pow((x - half) / half, 2f) + Mathf.Pow((y - half) / half, 2f));
        float a = Mathf.Clamp01(1f - r);
        a = a * a * (3f - 2f * a);
        texturaCirculo.SetPixel(x, y, new Color(1f, 1f, 1f, a));
      }
    }

    texturaCirculo.Apply(false, false);
    return texturaCirculo;
  }

  private static Sprite ObtenerSpriteAnillo()
  {
    if (spriteAnillo != null) { return spriteAnillo; }

    const int size = 128;
    texturaAnillo = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
    {
      name = "GulekGul_Anillo",
      wrapMode = TextureWrapMode.Clamp,
      filterMode = FilterMode.Bilinear
    };

    float half = (size - 1) * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float r = Mathf.Sqrt(Mathf.Pow((x - half) / half, 2f) + Mathf.Pow((y - half) / half, 2f));
        // Anillo fino con borde exterior duro y caida suave hacia adentro.
        float interior = Mathf.Clamp01((r - 0.62f) / 0.30f);
        float exterior = 1f - Mathf.Clamp01((r - 0.92f) / 0.06f);
        float a = interior * interior * exterior;
        texturaAnillo.SetPixel(x, y, new Color(1f, 1f, 1f, a));
      }
    }

    texturaAnillo.Apply(false, false);
    spriteAnillo = Sprite.Create(texturaAnillo, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    return spriteAnillo;
  }

  internal static ParticleSystem CrearSistema(Transform padre, string nombre, float duracion, int maxParticulas)
  {
    GameObject go = new GameObject(nombre);
    go.transform.SetParent(padre, false);

    ParticleSystem ps = go.AddComponent<ParticleSystem>();
    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.sharedMaterial = ObtenerMaterial();
    renderer.renderMode = ParticleSystemRenderMode.Billboard;

    var main = ps.main;
    main.playOnAwake = false;
    main.loop = false;
    main.duration = duracion;
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.maxParticles = maxParticulas;

    var emission = ps.emission;
    emission.enabled = false;

    var shape = ps.shape;
    shape.enabled = true;
    shape.shapeType = ParticleSystemShapeType.Circle;
    shape.radius = 0.01f;

    return ps;
  }

  internal static void AplicarGradiente(ParticleSystem ps, Color color, float alphaPico, float picoEn = 0.2f)
  {
    var col = ps.colorOverLifetime;
    col.enabled = true;
    Gradient g = new Gradient();
    g.SetKeys(
      new[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.Lerp(color, Color.black, 0.25f), 1f) },
      new[]
      {
        new GradientAlphaKey(0f, 0f),
        new GradientAlphaKey(alphaPico, picoEn),
        new GradientAlphaKey(alphaPico * 0.45f, 0.65f),
        new GradientAlphaKey(0f, 1f)
      });
    col.color = new ParticleSystem.MinMaxGradient(g);
  }

  internal static GameObject CrearRaiz(string nombre, Vector3 posicion, float vida)
  {
    GameObject raiz = new GameObject(nombre);
    raiz.transform.position = posicion;
    AutodestruirDelay auto = raiz.AddComponent<AutodestruirDelay>();
    auto.SetDelay(vida);
    return raiz;
  }

  internal static void AlFrente(GameObject raiz)
  {
    RenderOrderHelper.ForzarProyectilAlFrente(raiz);
  }

  // ------------------------------------------------------------------ efectos

  /// <summary>Onda de choque achatada sobre el suelo (anillos que se expanden).</summary>
  public static void OndaDeChoque(Vector3 centro, float radioEnCasillas, Color color, float duracion = 0.55f, int anillos = 2)
  {
    float cas = TamanoCasilla();
    GameObject raiz = CrearRaiz("GulekGul_OndaChoque", centro, duracion + 0.6f);

    for (int i = 0; i < anillos; i++)
    {
      GameObject goAnillo = new GameObject("Anillo" + i);
      goAnillo.transform.SetParent(raiz.transform, false);
      SpriteRenderer sr = goAnillo.AddComponent<SpriteRenderer>();
      sr.sprite = ObtenerSpriteAnillo();
      sr.color = new Color(color.r, color.g, color.b, 0f);

      GulekGulAnimadorAnillo anim = goAnillo.AddComponent<GulekGulAnimadorAnillo>();
      anim.Configurar(sr, cas * radioEnCasillas * 2f, 0.38f, duracion * (1f + 0.25f * i), i * 0.07f, color.a * (1f - 0.35f * i));
    }

    AlFrente(raiz);
  }

  /// <summary>Polvo y escombros que saltan desde un punto del suelo.</summary>
  public static void PolvoYEscombros(Vector3 posicion, float intensidad = 1f, float anchoEnCasillas = 0.7f)
  {
    intensidad = Mathf.Clamp(intensidad, 0.3f, 2f);
    float cas = TamanoCasilla();
    GameObject raiz = CrearRaiz("GulekGul_Polvo", posicion + new Vector3(0f, cas * 0.05f, 0f), 1.6f);

    // Nube de polvo.
    ParticleSystem polvo = CrearSistema(raiz.transform, "Polvo", 0.5f, 24);
    var mp = polvo.main;
    mp.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 0.95f);
    mp.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.25f, cas * 0.7f);
    mp.startSize = new ParticleSystem.MinMaxCurve(cas * 0.22f * intensidad, cas * 0.42f * intensidad);
    mp.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    mp.startColor = new Color(0.78f, 0.74f, 0.68f, 0.55f);
    mp.gravityModifier = -0.02f;
    var sp = polvo.shape;
    sp.radius = cas * anchoEnCasillas * 0.5f;
    var vp = polvo.velocityOverLifetime;
    vp.enabled = true;
    vp.space = ParticleSystemSimulationSpace.World;
    vp.x = new ParticleSystem.MinMaxCurve(-cas * 0.6f, cas * 0.6f);
    vp.y = new ParticleSystem.MinMaxCurve(cas * 0.1f, cas * 0.45f);
    vp.z = new ParticleSystem.MinMaxCurve(0f, 0f);
    var tp = polvo.sizeOverLifetime;
    tp.enabled = true;
    tp.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.55f), new Keyframe(1f, 1.5f)));
    AplicarGradiente(polvo, new Color(0.82f, 0.78f, 0.72f), 0.5f);
    polvo.Play();
    polvo.Emit(Mathf.RoundToInt(14 * intensidad));

    // Escombros: piedritas que saltan y caen.
    ParticleSystem piedras = CrearSistema(raiz.transform, "Escombros", 0.5f, 24);
    var mr = piedras.main;
    mr.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
    mr.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.9f, cas * 2.2f);
    mr.startSize = new ParticleSystem.MinMaxCurve(cas * 0.035f, cas * 0.085f);
    mr.startColor = new ParticleSystem.MinMaxGradient(new Color(0.42f, 0.38f, 0.34f, 1f), new Color(0.62f, 0.58f, 0.52f, 1f));
    mr.gravityModifier = 2.4f;
    var sr = piedras.shape;
    sr.shapeType = ParticleSystemShapeType.Cone;
    sr.angle = 38f;
    sr.radius = cas * anchoEnCasillas * 0.3f;
    sr.rotation = new Vector3(-90f, 0f, 0f); // hacia arriba (eje Y)
    AplicarGradiente(piedras, new Color(0.55f, 0.5f, 0.45f), 1f, 0.05f);
    piedras.Play();
    piedras.Emit(Mathf.RoundToInt(10 * intensidad));

    AlFrente(raiz);
  }

  /// <summary>Gritos de furia: brasas rojas ascendentes + halo pulsante alrededor de Gulek.</summary>
  public static void AuraFuria(Transform objetivo, float duracion = 2.4f)
  {
    if (objetivo == null) { return; }

    float cas = TamanoCasilla();
    Vector3 pos = objetivo.position;
    GameObject raiz = CrearRaiz("GulekGul_Furia", pos, duracion + 1f);
    raiz.transform.SetParent(objetivo, true);

    // Halo rojo pulsante.
    GameObject halo = new GameObject("Halo");
    halo.transform.SetParent(raiz.transform, false);
    SpriteRenderer srHalo = halo.AddComponent<SpriteRenderer>();
    srHalo.sprite = ObtenerSpriteAnillo();
    srHalo.color = new Color(1f, 0.15f, 0.08f, 0f);
    GulekGulAnimadorAnillo animHalo = halo.AddComponent<GulekGulAnimadorAnillo>();
    animHalo.Configurar(srHalo, cas * 3.2f, 0.9f, duracion, 0f, 0.55f);
    animHalo.pulsos = 3;

    // Brasas.
    ParticleSystem brasas = CrearSistema(raiz.transform, "Brasas", duracion, 40);
    var main = brasas.main;
    main.loop = false;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.3f, cas * 0.8f);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.04f, cas * 0.11f);
    main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.25f, 0.1f, 1f), new Color(1f, 0.6f, 0.15f, 1f));
    main.gravityModifier = -0.25f;
    var emission = brasas.emission;
    emission.enabled = true;
    emission.rateOverTime = 22f;
    var shape = brasas.shape;
    shape.shapeType = ParticleSystemShapeType.Circle;
    shape.radius = cas * 0.55f;
    AplicarGradiente(brasas, new Color(1f, 0.4f, 0.12f), 1f, 0.12f);
    brasas.Play();

    AlFrente(raiz);
  }

  /// <summary>Polvo y particulas donde se arranca una roca para lanzarla.</summary>
  public static void ArrancarRoca(Vector3 posicion)
  {
    PolvoYEscombros(posicion, 1.15f, 0.6f);
    OndaDeChoque(posicion, 0.55f, new Color(0.8f, 0.75f, 0.68f, 0.55f), 0.4f, 1);
  }

  /// <summary>Impacto de la roca contra el objetivo.</summary>
  public static void ImpactoRoca(Vector3 posicion, bool critico)
  {
    PolvoYEscombros(posicion, critico ? 1.9f : 1.25f, 0.75f);
    OndaDeChoque(posicion, critico ? 1.4f : 0.9f, new Color(0.88f, 0.82f, 0.7f, 0.7f), 0.45f, 1);
    SacudirCamara(critico ? 0.9f : 0.55f, critico ? 0.28f : 0.18f);
  }

  // ------------------------------------------------------------------ grietas del martillo

  private static Sprite[] spritesGrietas;

  private static Sprite ObtenerSpriteGrietas(int variante)
  {
    if (spritesGrietas == null)
    {
      spritesGrietas = new Sprite[3];
    }

    variante = Mathf.Clamp(variante, 0, spritesGrietas.Length - 1);
    if (spritesGrietas[variante] != null)
    {
      return spritesGrietas[variante];
    }

    const int size = 128;
    Color32[] px = new Color32[size * size];
    System.Random rng = new System.Random(1000 + variante * 77);

    // Mancha central suave (el punto exacto del impacto).
    float c = (size - 1) * 0.5f;
    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float r = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / 15f;
        float a = Mathf.Clamp01(1f - r);
        a = a * a * 0.6f;
        px[y * size + x] = new Color32(10, 7, 5, (byte)Mathf.RoundToInt(a * 255f));
      }
    }

    int grietasPrincipales = 6 + rng.Next(0, 3);
    for (int i = 0; i < grietasPrincipales; i++)
    {
      float angulo = (i / (float)grietasPrincipales) * Mathf.PI * 2f + (float)(rng.NextDouble() - 0.5) * 0.6f;
      DibujarGrieta(px, size, c, c, angulo, 38f + (float)rng.NextDouble() * 22f, 2.1f, rng, 1);
    }

    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
    {
      name = "GulekGul_Grietas" + variante,
      wrapMode = TextureWrapMode.Clamp,
      filterMode = FilterMode.Bilinear
    };
    tex.SetPixels32(px);
    tex.Apply(false, false);

    spritesGrietas[variante] = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    return spritesGrietas[variante];
  }

  // Grieta quebrada: segmentos con giro aleatorio, que se afinan y a veces se ramifican.
  private static void DibujarGrieta(Color32[] px, int size, float x0, float y0, float angulo, float largo, float grosor, System.Random rng, int nivel)
  {
    float x = x0;
    float y = y0;
    float recorrido = 0f;
    float a = angulo;

    while (recorrido < largo)
    {
      float paso = 5f + (float)rng.NextDouble() * 4f;
      a += ((float)rng.NextDouble() - 0.5f) * 0.9f;
      float nx = x + Mathf.Cos(a) * paso;
      float ny = y + Mathf.Sin(a) * paso;

      float k = recorrido / largo;
      float g = Mathf.Lerp(grosor, grosor * 0.35f, k);
      DibujarSegmento(px, size, x, y, nx, ny, g);

      if (nivel == 1 && rng.NextDouble() < 0.3 && k > 0.15f && k < 0.8f)
      {
        float lado = rng.NextDouble() < 0.5 ? -1f : 1f;
        DibujarGrieta(px, size, nx, ny, a + lado * (0.6f + (float)rng.NextDouble() * 0.5f),
          largo * (0.25f + (float)rng.NextDouble() * 0.2f), grosor * 0.6f, rng, 2);
      }

      x = nx;
      y = ny;
      recorrido += paso;
    }
  }

  private static void DibujarSegmento(Color32[] px, int size, float ax, float ay, float bx, float by, float grosor)
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
        float alpha = Mathf.Clamp01(1f - (d - grosor * 0.5f) / 1.1f);
        if (alpha <= 0f) { continue; }

        Color32 actual = px[y * size + x];
        byte a8 = (byte)Mathf.Max(actual.a, Mathf.RoundToInt(alpha * 255f));
        px[y * size + x] = new Color32(6, 4, 3, a8);
      }
    }
  }

  /// <summary>Grietas en el suelo de una casilla; duran un par de segundos y se desvanecen.</summary>
  public static void GrietasEnCasilla(Vector3 posicion, float tamanoEnCasillas = 0.95f, float retraso = 0f, float duracion = 2.6f)
  {
    float cas = TamanoCasilla();
    GameObject go = new GameObject("GulekGul_Grietas");
    go.transform.position = posicion + new Vector3(0f, cas * 0.02f, 0f);

    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    sr.sprite = ObtenerSpriteGrietas(Random.Range(0, 3));
    sr.flipX = Random.value < 0.5f;
    sr.sortingLayerID = 0;   // Default: por encima del tablero, por debajo de las unidades
    sr.sortingOrder = 5;
    sr.color = new Color(1f, 1f, 1f, 0f);

    GulekGulAnimadorGrietas anim = go.AddComponent<GulekGulAnimadorGrietas>();
    anim.Configurar(sr, cas * tamanoEnCasillas, 0.46f, retraso, duracion);
  }

  /// <summary>
  /// Golpe de martillo sobre una casilla que recibe daño: grietas + polvo que se levanta + anillo leve.
  /// distancia = casillas de separacion con el golpe principal (retrasa la onda hacia afuera).
  /// </summary>
  public static void ImpactoMartilloEnCasilla(Vector3 posicion, bool principal, int distancia = 0)
  {
    float retraso = Mathf.Max(0, distancia) * 0.07f;
    GrietasEnCasilla(posicion, principal ? 1.05f : 0.8f, retraso, principal ? 3.3f : 2.7f);

    if (retraso <= 0.001f)
    {
      PolvoYEscombros(posicion, principal ? 1.7f : 1.0f, principal ? 0.9f : 0.7f);
    }
    else
    {
      GulekGulRetardador.Ejecutar(retraso, () => PolvoYEscombros(posicion, 1.0f, 0.7f));
    }

    if (principal)
    {
      OndaDeChoque(posicion, 0.6f, new Color(0.9f, 0.82f, 0.68f, 0.4f), 0.4f, 1);
    }
  }

  /// <summary>Crea el proyectil de roca y lo hace volar. Completa al llegar.</summary>
  public static Task<bool> LanzarRoca(Sprite sprite, Vector3 origen, Vector3 destino, float duracion, float alturaEnCasillas)
  {
    float cas = TamanoCasilla();
    GameObject go = new GameObject("GulekGul_RocaVolando");
    go.transform.position = origen;

    SpriteRenderer roca = new GameObject("Roca").AddComponent<SpriteRenderer>();
    roca.transform.SetParent(go.transform, false);
    roca.sprite = sprite;

    if (spriteSombraRoca == null)
    {
      Texture2D textura = ObtenerTexturaCirculo();
      spriteSombraRoca = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height),
        new Vector2(0.5f, 0.5f), textura.width);
    }
    SpriteRenderer sombra = new GameObject("Sombra").AddComponent<SpriteRenderer>();
    sombra.transform.SetParent(go.transform, false);
    sombra.sprite = spriteSombraRoca;

    // Estela de polvo.
    ParticleSystem estela = CrearSistema(go.transform, "Estela", 3f, 80);
    var main = estela.main;
    main.loop = true;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.38f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.12f, cas * 0.42f);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.16f, cas * 0.34f);
    var forma = estela.shape;
    forma.radius = cas * 0.16f;
    var emission = estela.emission;
    emission.enabled = true;
    emission.rateOverTime = 95f;
    AplicarGradiente(estela, new Color(0.75f, 0.7f, 0.62f), 0.65f, 0.1f);

    AlFrente(go);
    estela.GetComponent<ParticleSystemRenderer>().sortingOrder = roca.sortingOrder - 1;
    sombra.sortingOrder = 0;

    GulekGulRocaVuelo vuelo = go.AddComponent<GulekGulRocaVuelo>();
    return vuelo.Iniciar(roca, sombra, estela, origen, destino, duracion, cas, cas * alturaEnCasillas);
  }
}

/// <summary>Anillo que se expande y se desvanece (achatado para simular perspectiva de suelo).</summary>
public class GulekGulAnimadorAnillo : MonoBehaviour
{
  public int pulsos = 0;

  private SpriteRenderer sr;
  private float diametroFinal;
  private float achatado;
  private float duracion;
  private float retraso;
  private float alphaMax;
  private float t;
  private Color colorBase;

  public void Configurar(SpriteRenderer renderer, float diametro, float achatadoY, float duracionTotal, float retrasoInicial, float alpha)
  {
    sr = renderer;
    diametroFinal = diametro;
    achatado = achatadoY;
    duracion = Mathf.Max(0.05f, duracionTotal);
    retraso = retrasoInicial;
    alphaMax = alpha;
    colorBase = renderer.color;
    transform.localScale = Vector3.zero;
  }

  private void Update()
  {
    if (sr == null) { return; }

    t += Time.deltaTime;
    float k = Mathf.Clamp01((t - retraso) / duracion);
    if (t < retraso) { return; }

    float ease = 1f - Mathf.Pow(1f - k, 3f);
    float d = Mathf.Lerp(diametroFinal * 0.15f, diametroFinal, ease);
    // sprite de 1 unidad de diametro (128px a 128 ppu)
    transform.localScale = new Vector3(d, d * achatado, 1f);

    float a = (1f - k) * (1f - k);
    if (pulsos > 0)
    {
      a *= 0.55f + 0.45f * Mathf.Sin(k * Mathf.PI * 2f * pulsos);
      transform.localScale = new Vector3(d * 0.9f, d * 0.9f * achatado, 1f);
    }

    sr.color = new Color(colorBase.r, colorBase.g, colorBase.b, Mathf.Clamp01(a * alphaMax));
  }
}

/// <summary>Vuelo, sombra y fragmentos de la roca en el reloj escalado del combate.</summary>
public class GulekGulRocaVuelo : MonoBehaviour
{
  private const float DuracionFragmentos = 0.38f;

  private readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
  private SpriteRenderer roca;
  private SpriteRenderer sombra;
  private ParticleSystem estela;
  private SpriteRenderer[] fragmentos;
  private Vector3 origen;
  private Vector3 destino;
  private Vector3 direccion;
  private float duracion;
  private float altura;
  private float casilla;
  private float escalaRoca;
  private float t;
  private bool impacto;

  public Task<bool> Iniciar(SpriteRenderer proyectil, SpriteRenderer sombraSuelo, ParticleSystem polvo,
    Vector3 desde, Vector3 hasta, float duracionVuelo, float tamanoCasilla, float alturaParabola)
  {
    roca = proyectil;
    sombra = sombraSuelo;
    estela = polvo;
    origen = desde;
    destino = hasta;
    direccion = (destino - origen).normalized;
    duracion = Mathf.Max(0.1f, duracionVuelo);
    casilla = tamanoCasilla;
    altura = alturaParabola;
    escalaRoca = roca.sprite != null ? casilla * 0.85f / Mathf.Max(0.01f, roca.sprite.bounds.size.x) : 1f;
    ActualizarVuelo(0f);
    estela.Play();
    return tcs.Task;
  }

  private void Update()
  {
    Avanzar(Time.deltaTime);
  }

  private void Avanzar(float delta)
  {
    if (delta <= 0f) { return; }

    t += delta;
    if (impacto)
    {
      ActualizarFragmentos(Mathf.Clamp01(t / DuracionFragmentos));
      if (t >= DuracionFragmentos) { Destroy(gameObject); }
      return;
    }

    float k = Mathf.Clamp01(t / duracion);
    ActualizarVuelo(k);
    if (k >= 1f)
    {
      impacto = true;
      t = 0f;
      roca.enabled = false;
      sombra.enabled = false;
      estela.Stop(false, ParticleSystemStopBehavior.StopEmitting);
      CrearFragmentos();
      tcs.TrySetResult(true);
    }
  }

  private void ActualizarVuelo(float k)
  {
    float avance = k * (0.25f + 0.75f * k);
    float retroceso = Mathf.Sin(Mathf.PI * Mathf.Clamp01(k / 0.2f)) * casilla * 0.18f;
    Vector3 suelo = Vector3.Lerp(origen, destino, avance) - direccion * retroceso;

    // El arco alcanza el punto mas alto temprano y cae pesado al final.
    float faseArco = k + 0.85f * k * (1f - k);
    float elevacion = altura * 4f * faseArco * (1f - faseArco) + casilla * 0.35f * (1f - k);
    Vector3 posicion = suelo + Vector3.up * elevacion;
    roca.transform.position = posicion;
    estela.transform.position = posicion;

    float sentido = direccion.x < 0f ? -1f : 1f;
    float giro = 390f * k * (0.35f + 0.65f * k) + 22f * Mathf.Sin(k * 15f) * (1f - 0.5f * k);
    roca.transform.rotation = Quaternion.Euler(0f, 0f, sentido * giro);
    float escala = escalaRoca * Mathf.SmoothStep(0.62f, 1f, Mathf.Clamp01(k / 0.36f));
    roca.transform.localScale = new Vector3(escala, escala, 1f);

    sombra.transform.position = suelo;
    float alturaRelativa = Mathf.Clamp01(elevacion / altura);
    float anchoSombra = casilla * Mathf.Lerp(1.12f, 0.56f, alturaRelativa);
    sombra.transform.localScale = new Vector3(anchoSombra, anchoSombra * 0.32f, 1f);
    sombra.color = new Color(0.09f, 0.07f, 0.06f, Mathf.Lerp(0.58f, 0.2f, alturaRelativa));
  }

  private void CrearFragmentos()
  {
    if (roca.sprite == null) { return; }

    fragmentos = new SpriteRenderer[3];
    for (int i = 0; i < fragmentos.Length; i++)
    {
      SpriteRenderer trozo = new GameObject("TrozoRoca").AddComponent<SpriteRenderer>();
      trozo.transform.SetParent(transform, false);
      trozo.sprite = roca.sprite;
      trozo.sortingOrder = roca.sortingOrder;
      trozo.flipX = i == 1;
      float escala = escalaRoca * (0.18f + 0.045f * i);
      trozo.transform.localScale = new Vector3(escala, escala, 1f);
      fragmentos[i] = trozo;
    }
    ActualizarFragmentos(0f);
  }

  private void ActualizarFragmentos(float k)
  {
    if (fragmentos == null) { return; }

    float avance = 1f - (1f - k) * (1f - k);
    for (int i = 0; i < fragmentos.Length; i++)
    {
      SpriteRenderer trozo = fragmentos[i];
      Vector3 dispersion = direccion * (0.23f + 0.13f * i) + Vector3.right * (0.49f * (i - 1));
      float rebote = 4f * k * (1f - k) * (0.44f - 0.09f * i);
      trozo.transform.position = destino + casilla * (avance * dispersion + Vector3.up * rebote);
      trozo.transform.rotation = Quaternion.Euler(0f, 0f, 55f * i + (160f * (i - 1) + 55f) * k);
      trozo.color = new Color(1f, 1f, 1f, 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((k - 0.65f) / 0.35f)));
    }
  }

  private void OnDestroy()
  {
    tcs.TrySetResult(false);
  }
}

/// <summary>Grietas en el suelo: aparecen de golpe, se mantienen un par de segundos y se desvanecen.</summary>
public class GulekGulAnimadorGrietas : MonoBehaviour
{
  private SpriteRenderer sr;
  private float diametro;
  private float achatado;
  private float retraso;
  private float duracion;
  private float t;

  public void Configurar(SpriteRenderer renderer, float diametroFinal, float achatadoY, float retrasoInicial, float duracionTotal)
  {
    sr = renderer;
    diametro = diametroFinal;
    achatado = achatadoY;
    retraso = Mathf.Max(0f, retrasoInicial);
    duracion = Mathf.Max(0.5f, duracionTotal);
    transform.localScale = Vector3.zero;
  }

  private void Update()
  {
    if (sr == null) { Destroy(gameObject); return; }

    t += Time.deltaTime;
    float k = t - retraso;
    if (k < 0f) { return; }

    // Entrada rapida (0.07 s) con un pequeño "pop" de escala.
    float entrada = Mathf.Clamp01(k / 0.07f);
    float escala = Mathf.Lerp(0.8f, 1f, entrada);
    transform.localScale = new Vector3(diametro * escala, diametro * escala * achatado, 1f);

    // Se mantiene y se desvanece en el ultimo 40%.
    float fade = Mathf.Clamp01((duracion - k) / (duracion * 0.4f));
    sr.color = new Color(1f, 1f, 1f, entrada * fade * 0.95f);

    if (k >= duracion) { Destroy(gameObject); }
  }
}

/// <summary>Ejecuta una accion tras un retraso (para ondas que viajan por el tablero).</summary>
public class GulekGulRetardador : MonoBehaviour
{
  private System.Action accion;
  private float restante;

  public static void Ejecutar(float segundos, System.Action accionDiferida)
  {
    GameObject go = new GameObject("GulekGul_Retardador");
    GulekGulRetardador r = go.AddComponent<GulekGulRetardador>();
    r.accion = accionDiferida;
    r.restante = segundos;
  }

  private void Update()
  {
    restante -= Time.deltaTime;
    if (restante > 0f) { return; }

    accion?.Invoke();
    Destroy(gameObject);
  }
}
