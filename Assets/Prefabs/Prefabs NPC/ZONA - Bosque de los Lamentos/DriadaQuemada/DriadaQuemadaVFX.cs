using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// VFX procedural breve para Enredar y Caricia del Bosque.
/// Complementa los prefabs existentes sin modificar sus recursos compartidos.
/// </summary>
public static class DriadaQuemadaVFX
{
  public static void CrearEnredar(Unidad objetivo)
  {
    if (objetivo == null)
    {
      return;
    }

    CrearMotor("VFX_DriadaQuemada_Enredar").IniciarEnredar(
      ObtenerPuntoSuelo(objetivo),
      objetivo.bGrande ? 1.25f : 1f);
  }

  public static void CrearCuracion(Unidad objetivo)
  {
    if (objetivo == null)
    {
      return;
    }

    CrearMotor("VFX_DriadaQuemada_Curacion").IniciarCuracion(
      ObtenerPuntoSuelo(objetivo),
      objetivo.bGrande ? 1.25f : 1f);
  }

  private static MotorVFXDriadaQuemada CrearMotor(string nombre)
  {
    GameObject go = new GameObject(nombre);
    if (BattleManager.Instance != null)
    {
      go.transform.SetParent(BattleManager.Instance.transform, true);
    }
    return go.AddComponent<MotorVFXDriadaQuemada>();
  }

  private static Vector3 ObtenerPuntoSuelo(Unidad objetivo)
  {
    return objetivo.transform.position + Vector3.up * 0.04f;
  }
}

public sealed class MotorVFXDriadaQuemada : MonoBehaviour
{
  private const int OrdenVFX = 380;
  private const int SegmentosAnillo = 36;

  public void IniciarEnredar(Vector3 centro, float escala)
  {
    StartCoroutine(AnimarEnredar(centro, escala));
  }

  public void IniciarCuracion(Vector3 centro, float escala)
  {
    StartCoroutine(AnimarCuracion(centro, escala));
  }

  private IEnumerator AnimarEnredar(Vector3 centro, float escala)
  {
    Color raiz = new Color(0.22f, 0.07f, 0.018f, 1f);
    Color brasa = new Color(0.62f, 0.24f, 0.055f, 1f);
    LineRenderer anillo = CrearLinea("AnilloSuelo", SegmentosAnillo, true, 0.010f);
    LineRenderer[] raices = new LineRenderer[5];
    float[] alturas = new float[raices.Length];
    for (int i = 0; i < raices.Length; i++)
    {
      raices[i] = CrearLinea("Raiz_" + i, 5, false, 0.009f);
      alturas[i] = Random.Range(0.15f, 0.24f) * escala;
    }
    CrearMotas(centro, raiz, brasa, 7, 0.22f * escala, 0.012f, 0.027f, 0.22f);

    float tiempo = 0f;
    const float duracion = 0.88f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      float crecimiento = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.42f));
      float salida = 1f - Mathf.SmoothStep(0.50f, 1f, t);
      float radio = Mathf.Lerp(0.04f, 0.30f * escala, crecimiento);

      ActualizarAnillo(anillo, centro, radio, tiempo, 0.005f);
      AplicarColor(anillo, brasa, salida * 0.26f);

      for (int i = 0; i < raices.Length; i++)
      {
        float angulo = i / (float)raices.Length * Mathf.PI * 2f + 0.28f;
        Vector3 radial = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo));
        Vector3 lateral = Vector3.Cross(Vector3.up, radial);
        Vector3 baseRaiz = centro + radial * (0.07f + (i % 2) * 0.035f) * escala;
        for (int p = 0; p < raices[i].positionCount; p++)
        {
          float tramo = p / (float)(raices[i].positionCount - 1);
          float serpenteo = Mathf.Sin(tramo * Mathf.PI * 2f + i) * 0.018f * crecimiento;
          raices[i].SetPosition(
            p,
            baseRaiz
              + radial * tramo * 0.10f * crecimiento * escala
              + lateral * serpenteo
              + Vector3.up * Mathf.Sin(tramo * Mathf.PI * 0.5f) * alturas[i] * crecimiento);
        }
        AplicarColor(raices[i], Color.Lerp(raiz, brasa, 0.28f), salida * 0.38f);
      }
      yield return null;
    }

    Destroy(gameObject);
  }

  private IEnumerator AnimarCuracion(Vector3 centro, float escala)
  {
    Color verde = new Color(0.24f, 0.52f, 0.12f, 1f);
    Color dorado = new Color(0.72f, 0.78f, 0.24f, 1f);
    LineRenderer anilloInterior = CrearLinea("AnilloCuracionInterior", SegmentosAnillo, true, 0.009f);
    LineRenderer anilloExterior = CrearLinea("AnilloCuracionExterior", SegmentosAnillo, true, 0.012f);
    LineRenderer[] brotes = new LineRenderer[6];
    for (int i = 0; i < brotes.Length; i++)
    {
      brotes[i] = CrearLinea("BroteCuracion_" + i, 4, false, 0.008f);
    }
    CrearMotas(centro, verde, dorado, 13, 0.28f * escala, 0.014f, 0.038f, 0.34f);

    float tiempo = 0f;
    const float duracion = 1.18f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float t = Mathf.Clamp01(tiempo / duracion);
      float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.30f));
      float salida = 1f - Mathf.SmoothStep(0.62f, 1f, t);
      float pulso = 0.92f + Mathf.Sin(t * Mathf.PI) * 0.08f;

      ActualizarAnillo(anilloInterior, centro, 0.20f * escala * entrada * pulso, -tiempo, 0.003f);
      ActualizarAnillo(anilloExterior, centro, 0.34f * escala * entrada * pulso, tiempo * 0.65f, 0.006f);
      AplicarColor(anilloInterior, dorado, salida * 0.26f);
      AplicarColor(anilloExterior, verde, salida * 0.20f);

      for (int i = 0; i < brotes.Length; i++)
      {
        float angulo = i / (float)brotes.Length * Mathf.PI * 2f + tiempo * 0.22f;
        Vector3 radial = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo));
        Vector3 tangente = Vector3.Cross(Vector3.up, radial);
        Vector3 baseBrote = centro + radial * (0.11f + (i % 2) * 0.05f) * escala;
        for (int p = 0; p < brotes[i].positionCount; p++)
        {
          float tramo = p / (float)(brotes[i].positionCount - 1);
          brotes[i].SetPosition(
            p,
            baseBrote
              + tangente * Mathf.Sin(tramo * Mathf.PI) * 0.035f * entrada
              + radial * tramo * 0.025f
              + Vector3.up * tramo * (0.16f + (i % 3) * 0.035f) * entrada);
        }
        AplicarColor(brotes[i], Color.Lerp(verde, dorado, 0.55f), salida * 0.32f);
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
    linea.shadowCastingMode = ShadowCastingMode.Off;
    linea.receiveShadows = false;
    linea.sortingLayerID = SortingLayer.NameToID("UI3D");
    linea.sortingOrder = OrdenVFX;
    return linea;
  }

  private static void ActualizarAnillo(LineRenderer anillo, Vector3 centro, float radio, float fase, float ondulacion)
  {
    for (int i = 0; i < anillo.positionCount; i++)
    {
      float angulo = i / (float)anillo.positionCount * Mathf.PI * 2f;
      float variacion = Mathf.Sin(angulo * 5f + fase * 4f) * ondulacion;
      anillo.SetPosition(
        i,
        centro + new Vector3(Mathf.Cos(angulo) * (radio + variacion), variacion * 0.35f, Mathf.Sin(angulo) * (radio + variacion)));
    }
  }

  private static void AplicarColor(LineRenderer linea, Color color, float alpha)
  {
    linea.startColor = new Color(color.r, color.g, color.b, alpha);
    linea.endColor = new Color(color.r * 0.64f, color.g * 0.64f, color.b * 0.64f, alpha * 0.55f);
  }

  private void CrearMotas(
    Vector3 centro,
    Color principal,
    Color brillo,
    int cantidad,
    float radio,
    float tamanoMin,
    float tamanoMax,
    float ascenso)
  {
    GameObject go = new GameObject("Motas");
    go.transform.SetParent(transform, false);
    go.transform.position = centro;
    ParticleSystem particulas = go.AddComponent<ParticleSystem>();
    particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

    ParticleSystem.MainModule main = particulas.main;
    main.duration = 0.55f;
    main.loop = false;
    main.startLifetime = new ParticleSystem.MinMaxCurve(0.52f, 0.92f);
    main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.06f);
    main.startSize = new ParticleSystem.MinMaxCurve(tamanoMin, tamanoMax);
    main.startColor = new ParticleSystem.MinMaxGradient(principal, brillo);
    main.simulationSpace = ParticleSystemSimulationSpace.World;

    ParticleSystem.EmissionModule emision = particulas.emission;
    emision.rateOverTime = 0f;
    emision.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)cantidad) });

    ParticleSystem.ShapeModule forma = particulas.shape;
    forma.shapeType = ParticleSystemShapeType.Circle;
    forma.radius = radio;
    forma.rotation = new Vector3(90f, 0f, 0f);

    ParticleSystem.VelocityOverLifetimeModule velocidad = particulas.velocityOverLifetime;
    velocidad.enabled = true;
    velocidad.space = ParticleSystemSimulationSpace.World;
    velocidad.y = new ParticleSystem.MinMaxCurve(ascenso * 0.65f, ascenso);

    ParticleSystem.ColorOverLifetimeModule colorVida = particulas.colorOverLifetime;
    colorVida.enabled = true;
    Gradient gradiente = new Gradient();
    gradiente.SetKeys(
      new[] { new GradientColorKey(brillo, 0f), new GradientColorKey(principal, 1f) },
      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.52f, 0.18f), new GradientAlphaKey(0f, 1f) });
    colorVida.color = new ParticleSystem.MinMaxGradient(gradiente);

    ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
    renderer.renderMode = ParticleSystemRenderMode.Billboard;
    renderer.alignment = ParticleSystemRenderSpace.View;
    renderer.material = ArbolLamentosVFX.ObtenerMaterialParticula();
    renderer.sortingLayerID = SortingLayer.NameToID("UI3D");
    renderer.sortingOrder = OrdenVFX + 2;
    particulas.Play();
  }
}
