using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public enum TipoVFXArbolLamentos
{
  Vaina,
  Crecimiento,
  Lamento
}

/// <summary>
/// VFX procedural compartido por las habilidades del Arbol de los Lamentos.
/// No requiere referencias serializadas: genera anillos, raices, espinas y particulas en runtime.
/// </summary>
public static class ArbolLamentosVFX
{
  private static Material materialLinea;
  private static Material materialParticula;
  private static Texture2D texturaParticula;

  public static void CrearCasteo(Unidad usuario, TipoVFXArbolLamentos tipo)
  {
    if (usuario == null)
    {
      return;
    }

    CrearMotor("VFX_ArbolLamentos_Casteo").IniciarCasteo(
      ObtenerPuntoSuelo(usuario.gameObject),
      ObtenerEscala(usuario),
      tipo);
  }

  public static void CrearLatigazo(Unidad usuario, object objetivo)
  {
    GameObject objetivoGO = ObtenerGameObject(objetivo);
    if (usuario == null || objetivoGO == null)
    {
      return;
    }

    CrearMotor("VFX_ArbolLamentos_Latigazo").IniciarLatigazo(
      usuario.puntoSaliente != null ? usuario.puntoSaliente.position : usuario.transform.position + Vector3.up * 0.32f,
      objetivoGO.transform);
  }

  public static void CrearImpacto(GameObject objetivo, TipoVFXArbolLamentos tipo)
  {
    if (objetivo == null)
    {
      return;
    }

    CrearMotor("VFX_ArbolLamentos_Impacto").IniciarImpacto(ObtenerPuntoSuelo(objetivo), tipo);
  }

  public static void CrearBrote(Casilla casilla)
  {
    if (casilla == null)
    {
      return;
    }

    CrearMotor("VFX_ArbolLamentos_Brote").IniciarBrote(casilla.transform.position + Vector3.up * 0.025f);
  }

  internal static Material ObtenerMaterialLinea()
  {
    if (materialLinea != null)
    {
      return materialLinea;
    }

    Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
    if (shader == null)
    {
      shader = Shader.Find("Sprites/Default");
    }

    materialLinea = new Material(shader)
    {
      name = "Mat_ArbolLamentos_Linea",
      hideFlags = HideFlags.HideAndDontSave
    };
    return materialLinea;
  }

  internal static Material ObtenerMaterialParticula()
  {
    if (materialParticula != null)
    {
      return materialParticula;
    }

    Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
    if (shader == null)
    {
      shader = Shader.Find("Sprites/Default");
    }

    materialParticula = new Material(shader)
    {
      name = "Mat_ArbolLamentos_Particula",
      hideFlags = HideFlags.HideAndDontSave,
      mainTexture = ObtenerTexturaParticula()
    };
    return materialParticula;
  }

  internal static void ObtenerPaleta(TipoVFXArbolLamentos tipo, out Color principal, out Color brillo)
  {
    switch (tipo)
    {
      case TipoVFXArbolLamentos.Crecimiento:
        principal = new Color(0.15f, 0.48f, 0.12f, 1f);
        brillo = new Color(0.62f, 0.92f, 0.24f, 1f);
        break;
      case TipoVFXArbolLamentos.Lamento:
        principal = new Color(0.31f, 0.08f, 0.48f, 1f);
        brillo = new Color(0.72f, 0.30f, 0.95f, 1f);
        break;
      default:
        principal = new Color(0.29f, 0.12f, 0.045f, 1f);
        brillo = new Color(0.68f, 0.82f, 0.20f, 1f);
        break;
    }
  }

  private static MotorVFXArbolLamentos CrearMotor(string nombre)
  {
    GameObject go = new GameObject(nombre);
    if (BattleManager.Instance != null)
    {
      go.transform.SetParent(BattleManager.Instance.transform, true);
    }
    return go.AddComponent<MotorVFXArbolLamentos>();
  }

  private static GameObject ObtenerGameObject(object objetivo)
  {
    if (objetivo is Unidad unidad)
    {
      return unidad.gameObject;
    }
    if (objetivo is Obstaculo obstaculo)
    {
      return obstaculo.gameObject;
    }
    return objetivo as GameObject;
  }

  private static Vector3 ObtenerPuntoSuelo(GameObject objetivo)
  {
    Unidad unidad = objetivo != null ? objetivo.GetComponent<Unidad>() : null;
    if (unidad != null)
    {
      return unidad.transform.position + Vector3.up * 0.035f;
    }

    Obstaculo obstaculo = objetivo != null ? objetivo.GetComponent<Obstaculo>() : null;
    if (obstaculo != null && obstaculo.CasillaPosicion != null)
    {
      return obstaculo.CasillaPosicion.transform.position + Vector3.up * 0.035f;
    }
    return objetivo != null ? objetivo.transform.position : Vector3.zero;
  }

  private static float ObtenerEscala(Unidad unidad)
  {
    return unidad != null && unidad.bGrande ? 1.35f : 1f;
  }

  private static Texture2D ObtenerTexturaParticula()
  {
    if (texturaParticula != null)
    {
      return texturaParticula;
    }

    const int tamano = 32;
    texturaParticula = new Texture2D(tamano, tamano, TextureFormat.ARGB32, false)
    {
      name = "Tex_ArbolLamentos_Particula",
      hideFlags = HideFlags.HideAndDontSave,
      filterMode = FilterMode.Bilinear
    };
    Color[] pixeles = new Color[tamano * tamano];
    float centro = (tamano - 1) * 0.5f;
    for (int y = 0; y < tamano; y++)
    {
      for (int x = 0; x < tamano; x++)
      {
        float dx = (x - centro) / centro;
        float dy = (y - centro) / centro;
        float alpha = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy)), 2.4f);
        pixeles[y * tamano + x] = new Color(1f, 1f, 1f, alpha);
      }
    }
    texturaParticula.SetPixels(pixeles);
    texturaParticula.Apply(false, true);
    return texturaParticula;
  }
}

public sealed class MotorVFXArbolLamentos : MonoBehaviour
{
  private const int SegmentosAnillo = 40;

  public void IniciarCasteo(Vector3 centro, float escala, TipoVFXArbolLamentos tipo)
  {
    StartCoroutine(AnimarCasteo(centro, escala, tipo));
  }

  public void IniciarLatigazo(Vector3 origen, Transform objetivo)
  {
    StartCoroutine(AnimarLatigazo(origen, objetivo));
  }

  public void IniciarImpacto(Vector3 centro, TipoVFXArbolLamentos tipo)
  {
    StartCoroutine(AnimarImpacto(centro, tipo));
  }

  public void IniciarBrote(Vector3 centro)
  {
    StartCoroutine(AnimarBrote(centro));
  }

  private IEnumerator AnimarCasteo(Vector3 centro, float escala, TipoVFXArbolLamentos tipo)
  {
    ArbolLamentosVFX.ObtenerPaleta(tipo, out Color principal, out Color brillo);
    LineRenderer[] anillos = new LineRenderer[3];
    for (int i = 0; i < anillos.Length; i++)
    {
      anillos[i] = CrearLinea("AnilloCasteo_" + i, SegmentosAnillo, true, 0.012f + i * 0.0025f);
    }

    LineRenderer[] raices = new LineRenderer[7];
    float[] fases = new float[raices.Length];
    for (int i = 0; i < raices.Length; i++)
    {
      raices[i] = CrearLinea("RaizCasteo_" + i, 7, false, 0.011f);
      fases[i] = (i / (float)raices.Length) * Mathf.PI * 2f + Random.Range(-0.18f, 0.18f);
    }

    CrearParticulas(centro + Vector3.up * 0.04f, principal, brillo, 18, 0.48f, 0.025f, 0.058f);
    float tiempo = 0f;
    const float duracion = 1.28f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.16f));
      float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.7f) / 0.3f));
      float intensidad = entrada * salida;

      for (int anilloIndex = 0; anilloIndex < anillos.Length; anilloIndex++)
      {
        LineRenderer anillo = anillos[anilloIndex];
        float radio = (0.27f + anilloIndex * 0.105f) * escala * Mathf.Lerp(0.35f, 1f, entrada);
        for (int i = 0; i < SegmentosAnillo; i++)
        {
          float angulo = i / (float)SegmentosAnillo * Mathf.PI * 2f + tiempo * (0.45f + anilloIndex * 0.22f);
          float ondulacion = Mathf.Sin(angulo * (4f + anilloIndex) + tiempo * 7f) * 0.015f;
          anillo.SetPosition(i, centro + new Vector3(Mathf.Cos(angulo) * (radio + ondulacion), ondulacion * 0.7f, Mathf.Sin(angulo) * (radio + ondulacion)));
        }
        AplicarColor(anillo, Color.Lerp(principal, brillo, anilloIndex * 0.32f), intensidad * (0.46f - anilloIndex * 0.07f));
      }

      for (int raizIndex = 0; raizIndex < raices.Length; raizIndex++)
      {
        LineRenderer raiz = raices[raizIndex];
        Vector3 direccion = new Vector3(Mathf.Cos(fases[raizIndex]), 0f, Mathf.Sin(fases[raizIndex]));
        Vector3 lateral = Vector3.Cross(Vector3.up, direccion);
        for (int i = 0; i < raiz.positionCount; i++)
        {
          float tramo = i / (float)(raiz.positionCount - 1);
          float largo = tramo * 0.53f * escala * entrada;
          float serpenteo = Mathf.Sin(tramo * 8f + tiempo * 5f + raizIndex) * 0.025f * tramo;
          raiz.SetPosition(i, centro + direccion * largo + lateral * serpenteo + Vector3.up * (0.008f + Mathf.Sin(tramo * Mathf.PI) * 0.025f));
        }
        AplicarColor(raiz, Color.Lerp(principal, brillo, 0.28f), intensidad * 0.50f);
      }
      yield return null;
    }
    Destroy(gameObject);
  }

  private IEnumerator AnimarLatigazo(Vector3 origen, Transform objetivo)
  {
    LineRenderer nucleo = CrearLinea("VainaNucleo", 15, false, 0.034f);
    LineRenderer halo = CrearLinea("VainaHalo", 15, false, 0.062f);
    nucleo.sortingLayerID = SortingLayer.NameToID("UI3D");
    nucleo.sortingOrder = 382;
    halo.sortingLayerID = nucleo.sortingLayerID;
    halo.sortingOrder = 381;
    Color madera = new Color(0.14f, 0.035f, 0.012f, 1f);
    Color savia = new Color(0.48f, 0.62f, 0.10f, 1f);
    float tiempo = 0f;
    const float duracion = 1.22f;
    while (tiempo < duracion && objetivo != null)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      float revelado = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.68f));
      float salida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.76f) / 0.24f));
      Vector3 destino = ObtenerPuntoObjetivo(objetivo);
      Vector3 direccion = (destino - origen).normalized;
      Vector3 lateral = Vector3.Cross(direccion, Vector3.up).normalized;
      if (lateral.sqrMagnitude < 0.01f)
      {
        lateral = Vector3.right;
      }

      for (int i = 0; i < nucleo.positionCount; i++)
      {
        float tramo = i / (float)(nucleo.positionCount - 1);
        float tramoVisible = Mathf.Min(tramo, revelado);
        Vector3 punto = Vector3.Lerp(origen, destino, tramoVisible);
        punto += Vector3.up * Mathf.Sin(tramoVisible * Mathf.PI) * 0.2f;
        punto += lateral * Mathf.Sin(tramoVisible * Mathf.PI * 5f + tiempo * 9f) * 0.035f * Mathf.Sin(tramoVisible * Mathf.PI);
        nucleo.SetPosition(i, punto);
        halo.SetPosition(i, punto);
      }
      AplicarColor(nucleo, madera, salida * 0.96f);
      AplicarColor(halo, savia, salida * 0.24f);
      yield return null;
    }
    Destroy(gameObject);
  }

  private IEnumerator AnimarImpacto(Vector3 centro, TipoVFXArbolLamentos tipo)
  {
    ArbolLamentosVFX.ObtenerPaleta(tipo, out Color principal, out Color brillo);
    LineRenderer anillo = CrearLinea("AnilloImpacto", SegmentosAnillo, true, 0.021f);
    LineRenderer[] espinas = new LineRenderer[8];
    for (int i = 0; i < espinas.Length; i++)
    {
      espinas[i] = CrearLinea("EspinaImpacto_" + i, 4, false, 0.016f);
    }
    CrearParticulas(centro + Vector3.up * 0.05f, principal, brillo, tipo == TipoVFXArbolLamentos.Lamento ? 22 : 15, 0.42f, 0.018f, 0.048f);

    float tiempo = 0f;
    const float duracion = 0.86f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      float intensidad = 1f - Mathf.SmoothStep(0.42f, 1f, t);
      float radio = Mathf.Lerp(0.08f, tipo == TipoVFXArbolLamentos.Lamento ? 0.46f : 0.34f, Mathf.SmoothStep(0f, 1f, t));
      for (int i = 0; i < SegmentosAnillo; i++)
      {
        float angulo = i / (float)SegmentosAnillo * Mathf.PI * 2f;
        anillo.SetPosition(i, centro + new Vector3(Mathf.Cos(angulo) * radio, Mathf.Sin(angulo * 5f + tiempo * 10f) * 0.012f, Mathf.Sin(angulo) * radio));
      }
      AplicarColor(anillo, brillo, intensidad * 0.58f);

      for (int i = 0; i < espinas.Length; i++)
      {
        float angulo = i / (float)espinas.Length * Mathf.PI * 2f;
        Vector3 radial = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo));
        float crecimiento = Mathf.Sin(Mathf.Clamp01(t / 0.45f) * Mathf.PI * 0.5f);
        Vector3 baseEspina = centro + radial * (0.09f + i % 2 * 0.055f);
        for (int p = 0; p < espinas[i].positionCount; p++)
        {
          float tramo = p / (float)(espinas[i].positionCount - 1);
          espinas[i].SetPosition(p, baseEspina + radial * tramo * 0.11f * crecimiento + Vector3.up * Mathf.Sin(tramo * Mathf.PI * 0.5f) * 0.28f * crecimiento);
        }
        AplicarColor(espinas[i], Color.Lerp(principal, brillo, 0.32f), intensidad * 0.55f);
      }
      yield return null;
    }
    Destroy(gameObject);
  }

  private IEnumerator AnimarBrote(Vector3 centro)
  {
    LineRenderer anillo = CrearLinea("AnilloBrote", 28, true, 0.014f);
    LineRenderer[] espinas = new LineRenderer[6];
    float[] alturas = new float[espinas.Length];
    for (int i = 0; i < espinas.Length; i++)
    {
      espinas[i] = CrearLinea("EspinaBrote_" + i, 5, false, 0.015f);
      alturas[i] = Random.Range(0.18f, 0.32f);
    }
    CrearParticulas(centro, new Color(0.17f, 0.38f, 0.07f), new Color(0.72f, 0.75f, 0.18f), 10, 0.30f, 0.014f, 0.034f);

    float tiempo = 0f;
    const float duracion = 1.05f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      float crecimiento = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.48f));
      float salida = 1f - Mathf.SmoothStep(0.58f, 1f, t);
      float radio = Mathf.Lerp(0.04f, 0.31f, crecimiento);
      for (int i = 0; i < anillo.positionCount; i++)
      {
        float angulo = i / (float)anillo.positionCount * Mathf.PI * 2f;
        anillo.SetPosition(i, centro + new Vector3(Mathf.Cos(angulo) * radio, 0.008f, Mathf.Sin(angulo) * radio));
      }
      AplicarColor(anillo, new Color(0.52f, 0.72f, 0.15f), salida * 0.44f);

      for (int i = 0; i < espinas.Length; i++)
      {
        float angulo = i / (float)espinas.Length * Mathf.PI * 2f + 0.35f;
        Vector3 radial = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo));
        Vector3 baseEspina = centro + radial * (0.07f + (i % 3) * 0.035f);
        for (int p = 0; p < espinas[i].positionCount; p++)
        {
          float tramo = p / (float)(espinas[i].positionCount - 1);
          float curva = Mathf.Sin(tramo * Mathf.PI) * 0.035f;
          espinas[i].SetPosition(p, baseEspina + radial * (tramo * 0.09f + curva) + Vector3.up * tramo * alturas[i] * crecimiento);
        }
        AplicarColor(espinas[i], Color.Lerp(new Color(0.18f, 0.07f, 0.025f), new Color(0.65f, 0.82f, 0.16f), t), salida * 0.68f);
      }
      yield return null;
    }
    Destroy(gameObject);
  }

  private LineRenderer CrearLinea(string nombre, int puntos, bool cerrada, float ancho)
  {
    GameObject go = new GameObject(nombre);
    go.transform.SetParent(transform, false);
    LineRenderer linea = go.AddComponent<LineRenderer>();
    linea.useWorldSpace = true;
    linea.alignment = LineAlignment.View;
    linea.material = ArbolLamentosVFX.ObtenerMaterialLinea();
    linea.positionCount = puntos;
    linea.loop = cerrada;
    linea.widthMultiplier = ancho;
    linea.numCapVertices = 3;
    linea.numCornerVertices = 3;
    linea.textureMode = LineTextureMode.Stretch;
    linea.shadowCastingMode = ShadowCastingMode.Off;
    linea.receiveShadows = false;
    linea.sortingOrder = 82;
    return linea;
  }

  private static void AplicarColor(LineRenderer linea, Color color, float alpha)
  {
    if (linea == null)
    {
      return;
    }
    linea.startColor = new Color(color.r, color.g, color.b, alpha);
    linea.endColor = new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, alpha * 0.38f);
  }

  private void CrearParticulas(Vector3 centro, Color principal, Color brillo, int cantidad, float velocidad, float tamanoMin, float tamanoMax)
  {
    GameObject go = new GameObject("Particulas");
    go.transform.position = centro;
    ParticleSystem particulas = go.AddComponent<ParticleSystem>();
    particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    ParticleSystem.MainModule main = particulas.main;
    main.duration = 0.7f;
    main.loop = false;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.78f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(velocidad * 0.35f, velocidad);
    main.startSize = new ParticleSystem.MinMaxCurve(tamanoMin, tamanoMax);
    main.startColor = new ParticleSystem.MinMaxGradient(principal, brillo);
    main.simulationSpace = ParticleSystemSimulationSpace.World;
    main.stopAction = ParticleSystemStopAction.Destroy;

    ParticleSystem.EmissionModule emision = particulas.emission;
    emision.rateOverTime = 0f;
    emision.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)cantidad) });
    ParticleSystem.ShapeModule forma = particulas.shape;
    forma.shapeType = ParticleSystemShapeType.Hemisphere;
    forma.radius = 0.16f;
    ParticleSystem.ColorOverLifetimeModule colorVida = particulas.colorOverLifetime;
    colorVida.enabled = true;
    Gradient gradiente = new Gradient();
    gradiente.SetKeys(
      new[] { new GradientColorKey(brillo, 0f), new GradientColorKey(principal, 1f) },
      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.12f), new GradientAlphaKey(0f, 1f) });
    colorVida.color = new ParticleSystem.MinMaxGradient(gradiente);

    ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.renderMode = ParticleSystemRenderMode.Billboard;
    renderer.alignment = ParticleSystemRenderSpace.View;
    renderer.material = ArbolLamentosVFX.ObtenerMaterialParticula();
    renderer.sortingOrder = 84;
    particulas.Play();
  }

  private static Vector3 ObtenerPuntoObjetivo(Transform objetivo)
  {
    if (objetivo == null)
    {
      return Vector3.zero;
    }
    Unidad unidad = objetivo.GetComponent<Unidad>();
    return unidad != null && unidad.puntoEntrante != null
      ? unidad.puntoEntrante.position
      : objetivo.position + Vector3.up * 0.22f;
  }
}
