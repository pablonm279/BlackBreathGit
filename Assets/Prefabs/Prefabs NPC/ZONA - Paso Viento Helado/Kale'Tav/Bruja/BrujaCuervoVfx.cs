using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Cuervo procedural de la Bruja Kale'Tav: silueta con aleteo, vuelo en arco, picotazos y plumas.
/// Sirve para que la reaccion/ataque de cuervo se lea paso a paso (sale, vuela, pica, vuelve).
/// </summary>
public static class BrujaCuervoVfx
{
  private static Sprite[] framesCuervo;

  // ------------------------------------------------------------------ sprite procedural

  private static Sprite[] ObtenerFrames()
  {
    if (framesCuervo != null) { return framesCuervo; }

    // Punta del ala por frame: arriba, intermedio, abajo.
    Vector2[] puntasAla = { new Vector2(20f, 58f), new Vector2(10f, 42f), new Vector2(18f, 8f) };
    framesCuervo = new Sprite[puntasAla.Length];
    for (int i = 0; i < puntasAla.Length; i++)
    {
      framesCuervo[i] = CrearFrame(puntasAla[i], i);
    }

    return framesCuervo;
  }

  private static Sprite CrearFrame(Vector2 puntaAla, int indice)
  {
    const int size = 64;
    const int ss = 3; // supersampling para bordes suaves
    Color32[] px = new Color32[size * size];

    Color32 colorCuerpo = new Color32(20, 16, 28, 255);
    Color32 colorAla = new Color32(36, 28, 52, 255);

    for (int y = 0; y < size; y++)
    {
      for (int x = 0; x < size; x++)
      {
        float cubCuerpo = 0f, cubAla = 0f, cubOjo = 0f;
        for (int sy = 0; sy < ss; sy++)
        {
          for (int sx = 0; sx < ss; sx++)
          {
            Vector2 p = new Vector2(x + (sx + 0.5f) / ss, y + (sy + 0.5f) / ss);
            if (EnCuerpo(p)) { cubCuerpo += 1f; }
            if (EnAla(p, puntaAla)) { cubAla += 1f; }
            if (Vector2.Distance(p, new Vector2(44.5f, 35f)) < 1.1f) { cubOjo += 1f; }
          }
        }

        float total = ss * ss;
        float aCuerpo = cubCuerpo / total;
        float aAla = cubAla / total;
        float aOjo = cubOjo / total;
        float alpha = Mathf.Max(aCuerpo, aAla);
        if (alpha <= 0f) { continue; }

        // El cuerpo se pinta sobre el ala (el ala lejana queda detras).
        Color32 baseColor = aCuerpo >= aAla ? colorCuerpo : colorAla;
        Color c = Color.Lerp(baseColor, new Color32(235, 235, 245, 255), aOjo * 0.9f);
        px[y * size + x] = new Color(c.r, c.g, c.b, alpha);
      }
    }

    Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
    {
      name = "BrujaCuervo_" + indice,
      wrapMode = TextureWrapMode.Clamp,
      filterMode = FilterMode.Bilinear
    };
    tex.SetPixels32(px);
    tex.Apply(false, false);
    return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
  }

  private static bool EnCuerpo(Vector2 p)
  {
    // Torso: elipse; cabeza: circulo; pico y cola: triangulos.
    Vector2 d = p - new Vector2(30f, 31f);
    if ((d.x * d.x) / (12f * 12f) + (d.y * d.y) / (6.5f * 6.5f) <= 1f) { return true; }
    if (Vector2.Distance(p, new Vector2(43f, 34f)) <= 5.2f) { return true; }
    if (EnTriangulo(p, new Vector2(47f, 36f), new Vector2(47f, 31.5f), new Vector2(56f, 33f))) { return true; }   // pico
    if (EnTriangulo(p, new Vector2(21f, 33f), new Vector2(21f, 28f), new Vector2(6f, 25f))) { return true; }      // cola
    if (EnTriangulo(p, new Vector2(21f, 31f), new Vector2(21f, 27f), new Vector2(7f, 31f))) { return true; }
    return false;
  }

  private static bool EnAla(Vector2 p, Vector2 punta)
  {
    // Ala: dos triangulos que forman una hoja curva desde el hombro hasta la punta.
    Vector2 hombro = new Vector2(33f, 33f);
    Vector2 atras = new Vector2(19f, 32f);
    Vector2 medio = new Vector2(punta.x + 12f, Mathf.Lerp(33f, punta.y, 0.55f));
    return EnTriangulo(p, hombro, atras, punta) || EnTriangulo(p, hombro, medio, punta);
  }

  private static bool EnTriangulo(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
  {
    float d1 = Signo(p, a, b);
    float d2 = Signo(p, b, c);
    float d3 = Signo(p, c, a);
    bool neg = d1 < 0f || d2 < 0f || d3 < 0f;
    bool pos = d1 > 0f || d2 > 0f || d3 > 0f;
    return !(neg && pos);
  }

  private static float Signo(Vector2 p1, Vector2 p2, Vector2 p3)
  {
    return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
  }

  // ------------------------------------------------------------------ cuervo

  /// <summary>Crea el cuervo en 'posicion', con aleteo. Mirando a la derecha por defecto.</summary>
  public static BrujaCuervo Crear(Vector3 posicion)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject go = new GameObject("Bruja_Cuervo");
    go.transform.position = posicion;

    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
    Sprite[] frames = ObtenerFrames();
    sr.sprite = frames[1];
    float escala = cas * 0.75f / Mathf.Max(0.01f, frames[1].bounds.size.x);
    go.transform.localScale = new Vector3(escala, escala, 1f);

    RenderOrderHelper.ForzarProyectilAlFrente(go);

    BrujaCuervo cuervo = go.AddComponent<BrujaCuervo>();
    cuervo.Configurar(sr, frames);
    return cuervo;
  }

  // ------------------------------------------------------------------ plumas

  /// <summary>Nube de plumas oscuras que caen despacio (salida, picotazo y regreso).</summary>
  public static void Plumas(Vector3 posicion, int cantidad, float intensidad = 1f)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    GameObject raiz = GulekGulVfx.CrearRaiz("Bruja_Plumas", posicion, 2f);

    ParticleSystem ps = GulekGulVfx.CrearSistema(raiz.transform, "Plumas", 0.3f, 40);
    var main = ps.main;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(cas * 0.3f * intensidad, cas * 1.1f * intensidad);
    main.startSize = new ParticleSystem.MinMaxCurve(cas * 0.035f, cas * 0.08f);
    main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
    main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.1f, 0.08f, 0.14f, 1f), new Color(0.32f, 0.26f, 0.42f, 1f));
    main.gravityModifier = 0.35f;

    var shape = ps.shape;
    shape.radius = cas * 0.12f;

    var rot = ps.rotationOverLifetime;
    rot.enabled = true;
    rot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

    var vel = ps.velocityOverLifetime;
    vel.enabled = true;
    vel.space = ParticleSystemSimulationSpace.World;
    vel.x = new ParticleSystem.MinMaxCurve(-cas * 0.4f, cas * 0.4f);
    vel.y = new ParticleSystem.MinMaxCurve(cas * 0.1f, cas * 0.6f);
    vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

    GulekGulVfx.AplicarGradiente(ps, new Color(0.2f, 0.16f, 0.28f), 1f, 0.05f);
    ps.Play();
    ps.Emit(Mathf.Max(1, cantidad));

    GulekGulVfx.AlFrente(raiz);
  }
}

/// <summary>Cuervo en vuelo: aleteo, arco, giro segun el sentido y picotazos.</summary>
public class BrujaCuervo : MonoBehaviour
{
  private SpriteRenderer sr;
  private Sprite[] frames;
  private float tAleteo;
  private float offsetPicado;       // desplazamiento vertical extra al picar
  private float inclinacionPicado;  // grados de inclinacion al picar
  private float inclinacionVuelo;
  private Vector3 posicionBase;

  public void Configurar(SpriteRenderer renderer, Sprite[] sprites)
  {
    sr = renderer;
    frames = sprites;
    posicionBase = transform.position;
  }

  private void Update()
  {
    if (sr == null) { return; }

    // Aleteo ciclico: arriba - medio - abajo - medio.
    tAleteo += Time.deltaTime * 14f;
    int[] ciclo = { 0, 1, 2, 1 };
    sr.sprite = frames[ciclo[Mathf.FloorToInt(tAleteo) % ciclo.Length]];

    transform.rotation = Quaternion.Euler(0f, 0f, inclinacionVuelo + inclinacionPicado);
  }

  private void LateUpdate()
  {
    // El picado se suma sobre la posicion de vuelo sin alterar la trayectoria.
    if (Mathf.Abs(offsetPicado) > 0.0001f)
    {
      transform.position = posicionBase + new Vector3(0f, offsetPicado, 0f);
    }
  }

  /// <summary>Vuela en arco hasta 'destino'. Completa al llegar. Respeta la pausa del combate.</summary>
  public async Task VolarAAsync(Vector3 destino, float duracion, float alturaArco)
  {
    Vector3 origen = posicionBase;
    float dirX = destino.x - origen.x;
    if (Mathf.Abs(dirX) > 0.001f && sr != null) { sr.flipX = dirX < 0f; }

    float tiempo = 0f;
    duracion = Mathf.Max(0.05f, duracion);
    while (tiempo < duracion)
    {
      if (this == null) { return; }

      if (BattleManager.Instance != null && BattleManager.Instance.PausaCombateActiva)
      {
        await Task.Yield();
        continue;
      }

      tiempo += Time.deltaTime;
      float k = Mathf.Clamp01(tiempo / duracion);
      float suave = k * k * (3f - 2f * k);

      Vector3 pos = Vector3.Lerp(origen, destino, suave);
      pos.y += Mathf.Sin(k * Mathf.PI) * alturaArco;
      pos.y += Mathf.Sin(k * Mathf.PI * 5f) * alturaArco * 0.08f; // vaiven del aleteo

      Vector3 anterior = posicionBase;
      posicionBase = pos;
      transform.position = pos;

      // Se inclina segun suba o baje.
      float dy = pos.y - anterior.y;
      inclinacionVuelo = Mathf.Clamp(dy * 900f, -25f, 25f) * (sr != null && sr.flipX ? -1f : 1f);

      await Task.Yield();
    }

    if (this == null) { return; }
    posicionBase = destino;
    transform.position = destino;
    inclinacionVuelo = 0f;
  }

  /// <summary>Un picotazo: cabeceo rapido hacia abajo y vuelta.</summary>
  public async Task PicarAsync(float duracion = 0.14f)
  {
    float cas = GulekGulVfx.TamanoCasilla();
    float tiempo = 0f;
    duracion = Mathf.Max(0.05f, duracion);
    while (tiempo < duracion)
    {
      if (this == null) { return; }

      if (BattleManager.Instance != null && BattleManager.Instance.PausaCombateActiva)
      {
        await Task.Yield();
        continue;
      }

      tiempo += Time.deltaTime;
      float k = Mathf.Clamp01(tiempo / duracion);
      float golpe = Mathf.Sin(k * Mathf.PI);
      offsetPicado = -golpe * cas * 0.22f;
      inclinacionPicado = -golpe * 32f * (sr != null && sr.flipX ? -1f : 1f);
      await Task.Yield();
    }

    if (this == null) { return; }
    offsetPicado = 0f;
    inclinacionPicado = 0f;
    transform.position = posicionBase;
  }

  public void Quitar()
  {
    if (this != null) { Destroy(gameObject); }
  }
}
