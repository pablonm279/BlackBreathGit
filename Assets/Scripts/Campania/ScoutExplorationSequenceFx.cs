using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Refuerzo visual runtime para la exploracion activa. Se crea solo durante la
/// expedicion para no requerir referencias nuevas en la escena de campania.
/// </summary>
public sealed class ScoutExplorationSequenceFx : MonoBehaviour
{
  const int CantidadExploradores = 5;
  const int OrdenCanvas = 7800;
  static readonly Vector2 ResolucionReferencia = new Vector2(1920f, 1080f);
  static readonly Color ColorExploracion = new Color(0.32f, 0.48f, 0.47f, 1f);
  static readonly Color ColorExito = new Color(0.34f, 0.52f, 0.40f, 1f);
  static readonly Color ColorCritico = new Color(0.62f, 0.51f, 0.32f, 1f);
  static readonly Color ColorFallo = new Color(0.48f, 0.28f, 0.29f, 1f);

  static ScoutExplorationSequenceFx activa;

  readonly List<MarcadorExplorador> marcadores = new List<MarcadorExplorador>();
  readonly List<Vector2> puntosRutaCanvas = new List<Vector2>();
  RectTransform canvasRect;
  CanvasGroup canvasGroup;
  Image tinte;
  ScoutExplorationRouteGraphic ruta;
  RectTransform reticula;
  ScoutExplorationRingGraphic anilloInterior;
  ScoutExplorationRingGraphic anilloExterior;
  RectTransform barrido;
  Nodo origen;
  Nodo destino;
  LineRenderer lineaCamino;
  Vector2 puntoOrigen;
  Vector2 puntoDestino;

  sealed class MarcadorExplorador
  {
    public RectTransform rect;
    public Image cuerpo;
    public Image halo;
  }

  public static IEnumerator ReproducirTrayecto(Nodo nodoOrigen, Nodo nodoDestino, float duracion)
  {
    if (activa != null)
    {
      activa.CerrarInmediato();
    }

    GameObject root = new GameObject("ScoutExplorationSequenceFx", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
    root.hideFlags = HideFlags.DontSave;
    activa = root.AddComponent<ScoutExplorationSequenceFx>();
    activa.Inicializar(nodoOrigen, nodoDestino);
    yield return activa.ReproducirTrayectoInterno(Mathf.Max(0.1f, duracion));
  }

  public static IEnumerator ReproducirResultado(CampaignManager.ResultadoExploradoresCampania resultado, float duracion)
  {
    if (resultado == null)
    {
      if (activa != null)
      {
        activa.CerrarInmediato();
      }
      yield break;
    }

    if (activa == null)
    {
      GameObject root = new GameObject("ScoutExplorationSequenceFx", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
      root.hideFlags = HideFlags.DontSave;
      activa = root.AddComponent<ScoutExplorationSequenceFx>();
      activa.Inicializar(resultado.nodoObjetivo, resultado.nodoObjetivo);
    }

    yield return activa.ReproducirResultadoInterno(resultado, Mathf.Max(0.2f, duracion));
  }

  void Inicializar(Nodo nodoOrigen, Nodo nodoDestino)
  {
    origen = nodoOrigen;
    destino = nodoDestino;
    CaminoConexion conexion = origen != null ? origen.ObtenerConexionHacia(destino) : null;
    lineaCamino = conexion != null && conexion.linea != null ? conexion.linea.GetComponent<LineRenderer>() : null;

    Canvas canvas = GetComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.overrideSorting = true;
    canvas.sortingOrder = OrdenCanvas;

    CanvasScaler scaler = GetComponent<CanvasScaler>();
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = ResolucionReferencia;
    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    scaler.matchWidthOrHeight = 0.5f;

    canvasRect = GetComponent<RectTransform>();
    canvasGroup = GetComponent<CanvasGroup>();
    canvasGroup.alpha = 0f;
    canvasGroup.blocksRaycasts = false;
    canvasGroup.interactable = false;

    tinte = CrearImagen("Tinte", transform, Vector2.zero, Vector2.one, Vector2.zero, new Color(0.015f, 0.035f, 0.04f, 0f));
    tinte.rectTransform.offsetMin = Vector2.zero;
    tinte.rectTransform.offsetMax = Vector2.zero;

    GameObject rutaGo = new GameObject("RutaExploradores", typeof(RectTransform), typeof(ScoutExplorationRouteGraphic));
    rutaGo.transform.SetParent(transform, false);
    RectTransform rutaRect = rutaGo.GetComponent<RectTransform>();
    EstirarPantalla(rutaRect);
    ruta = rutaGo.GetComponent<ScoutExplorationRouteGraphic>();
    ruta.raycastTarget = false;
    ruta.color = ColorExploracion;
    ruta.Progreso = 0f;

    CrearReticula();
    CrearMarcadores();
    ActualizarPuntosPantalla();
    AplicarPosicionReticula();
  }

  void CrearReticula()
  {
    GameObject reticulaGo = new GameObject("ReticulaDestino", typeof(RectTransform));
    reticulaGo.transform.SetParent(transform, false);
    reticula = reticulaGo.GetComponent<RectTransform>();
    reticula.sizeDelta = new Vector2(84f, 84f);

    anilloExterior = CrearAnillo("AnilloExterior", reticula, 36f, 0.9f, 56);
    anilloInterior = CrearAnillo("AnilloInterior", reticula, 24f, 0.7f, 40);

    for (int i = 0; i < 8; i++)
    {
      Image marca = CrearImagen("Marca" + i, reticula, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1.5f, i % 2 == 0 ? 8f : 5f), ConAlpha(ColorExploracion, 0.52f));
      float anguloGrados = i * 45f;
      float angulo = anguloGrados * Mathf.Deg2Rad;
      marca.rectTransform.anchoredPosition = new Vector2(-Mathf.Sin(angulo), Mathf.Cos(angulo)) * 31f;
      marca.rectTransform.localRotation = Quaternion.Euler(0f, 0f, anguloGrados);
    }

    Image centro = CrearImagen("Centro", reticula, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(5f, 5f), ConAlpha(ColorExploracion, 0.58f));
    centro.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

    Image barridoImagen = CrearImagen("Barrido", reticula, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1f, 28f), new Color(ColorExploracion.r, ColorExploracion.g, ColorExploracion.b, 0.18f));
    barrido = barridoImagen.rectTransform;
    barrido.pivot = new Vector2(0.5f, 0f);
    barrido.anchoredPosition = Vector2.zero;
    reticula.localScale = Vector3.zero;
  }

  void CrearMarcadores()
  {
    for (int i = 0; i < CantidadExploradores; i++)
    {
      GameObject go = new GameObject("Explorador" + (i + 1), typeof(RectTransform));
      go.transform.SetParent(transform, false);
      RectTransform rect = go.GetComponent<RectTransform>();
      rect.sizeDelta = new Vector2(11.4f, 11.4f);

      Image halo = CrearImagen("Halo", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10.45f, 10.45f), new Color(ColorExploracion.r * 0.62f, ColorExploracion.g * 0.62f, ColorExploracion.b * 0.62f, 0.045f));
      halo.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

      Color colorScout = new Color(ColorExploracion.r * 0.62f, ColorExploracion.g * 0.62f, ColorExploracion.b * 0.62f, 1f);
      Image cuerpo = CrearImagen("Cuerpo", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(4.75f, 4.75f), ConAlpha(colorScout, 0.62f));
      cuerpo.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

      marcadores.Add(new MarcadorExplorador { rect = rect, cuerpo = cuerpo, halo = halo });
    }
  }

  IEnumerator ReproducirTrayectoInterno(float duracion)
  {
    float tiempo = 0f;
    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float p = Mathf.Clamp01(tiempo / duracion);
      ActualizarPuntosPantalla();

      canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(p / 0.1f));
      tinte.color = new Color(0.015f, 0.025f, 0.026f, 0.025f + Mathf.Sin(p * Mathf.PI) * 0.025f);
      ruta.Progreso = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.04f, 0.57f, p));
      ruta.color = ConAlpha(ColorExploracion, 0.10f + 0.07f * Mathf.Sin(Mathf.Clamp01(p / 0.62f) * Mathf.PI));

      float aparicionReticula = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.34f, 0.58f, p));
      float pulso = 1f + Mathf.Sin(tiempo * 2.9f) * 0.025f;
      reticula.localScale = Vector3.one * aparicionReticula * pulso;
      reticula.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(tiempo * 0.4f) * 3f);
      barrido.localRotation = Quaternion.Euler(0f, 0f, -55f - p * 410f);
      AplicarColorReticula(ColorExploracion, 0.14f + aparicionReticula * 0.18f);

      for (int i = 0; i < marcadores.Count; i++)
      {
        float demora = i * 0.035f;
        float avance = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f + demora, 0.62f + demora, p));
        Vector2 posicion = PosicionRuta(avance, OffsetMarcador(i));

        if (p > 0.6f)
        {
          float busqueda = Mathf.InverseLerp(0.6f, 1f, p);
          float angulo = i * Mathf.PI * 2f / CantidadExploradores;
          float respiracion = Mathf.Sin(tiempo * 1.25f + i) * 1.2f;
          float radio = 7f + (i % 2) * 2f + respiracion;
          Vector2 posicionBusqueda = puntoDestino + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio;
          posicion = Vector2.Lerp(posicion, posicionBusqueda, Mathf.SmoothStep(0f, 1f, busqueda));
        }

        AplicarMarcador(marcadores[i], posicion, ColorExploracion, Mathf.Clamp01(avance * 1.25f) * 0.62f, 1f + Mathf.Sin(tiempo * 2.3f + i) * 0.025f);
      }

      yield return null;
    }

    ruta.Progreso = 1f;
  }

  IEnumerator ReproducirResultadoInterno(CampaignManager.ResultadoExploradoresCampania resultado, float duracion)
  {
    Color colorResultado = resultado.critico && resultado.exito ? ColorCritico : resultado.exito ? ColorExito : ColorFallo;
    int cantidadQueRegresa = resultado.exito ? CantidadExploradores : Mathf.Clamp(resultado.civilesDevueltos, 0, CantidadExploradores);
    float tiempo = 0f;

    while (tiempo < duracion)
    {
      tiempo += Time.deltaTime;
      float p = Mathf.Clamp01(tiempo / duracion);
      ActualizarPuntosPantalla();

      float salida = Mathf.SmoothStep(0f, 1f, p);
      tinte.color = ConAlpha(colorResultado, Mathf.Lerp(0.03f, 0f, salida));
      ruta.color = ConAlpha(colorResultado, Mathf.Lerp(0.18f, 0.02f, salida));
      AplicarColorReticula(colorResultado, Mathf.Lerp(0.36f, 0f, salida));
      reticula.localScale = Vector3.one * Mathf.Lerp(1f, resultado.critico ? 1.15f : 1.08f, salida);
      barrido.localRotation = Quaternion.Euler(0f, 0f, -465f - p * 190f);

      for (int i = 0; i < marcadores.Count; i++)
      {
        MarcadorExplorador marcador = marcadores[i];
        if (i < cantidadQueRegresa)
        {
          float demora = i * 0.055f;
          float regreso = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(demora, 0.86f + demora, p));
          Vector2 posicion = PosicionRuta(1f - regreso, OffsetMarcador(i));
          AplicarMarcador(marcador, posicion, colorResultado, Mathf.Clamp01((1f - regreso) * 2f) * 0.62f, 1f + Mathf.Sin(tiempo * 3.5f + i) * 0.02f);
        }
        else
        {
          float angulo = (i * 79f + 25f) * Mathf.Deg2Rad;
          Vector2 dispersion = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * Mathf.Lerp(10f, 26f, salida);
          AplicarMarcador(marcador, puntoDestino + dispersion, ColorFallo, 1f - salida, Mathf.Lerp(1f, 0.25f, salida));
        }
      }

      canvasGroup.alpha = p < 0.78f ? 1f : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.78f, 1f, p));
      yield return null;
    }

    CerrarInmediato();
  }

  void ActualizarPuntosPantalla()
  {
    puntosRutaCanvas.Clear();
    if (lineaCamino != null && lineaCamino.positionCount >= 2)
    {
      bool invertir = DebeInvertirLineaCamino();
      for (int i = 0; i < lineaCamino.positionCount; i++)
      {
        int indice = invertir ? lineaCamino.positionCount - 1 - i : i;
        Vector3 puntoLinea = lineaCamino.GetPosition(indice);
        Vector3 puntoMundo = lineaCamino.useWorldSpace ? puntoLinea : lineaCamino.transform.TransformPoint(puntoLinea);
        puntosRutaCanvas.Add(ConvertirAPuntoCanvas(puntoMundo, Vector2.zero));
      }
    }

    if (puntosRutaCanvas.Count < 2)
    {
      puntosRutaCanvas.Clear();
      puntosRutaCanvas.Add(ConvertirAPuntoCanvas(origen != null ? origen.transform.position : Vector3.zero, new Vector2(-260f, 0f)));
      puntosRutaCanvas.Add(ConvertirAPuntoCanvas(destino != null ? destino.transform.position : Vector3.zero, new Vector2(260f, 0f)));
    }

    puntoOrigen = puntosRutaCanvas[0];
    puntoDestino = puntosRutaCanvas[puntosRutaCanvas.Count - 1];
    ruta.EstablecerPuntos(puntosRutaCanvas);
    AplicarPosicionReticula();
  }

  bool DebeInvertirLineaCamino()
  {
    if (lineaCamino == null || lineaCamino.positionCount < 2 || origen == null)
    {
      return false;
    }

    Vector3 primero = lineaCamino.GetPosition(0);
    Vector3 ultimo = lineaCamino.GetPosition(lineaCamino.positionCount - 1);
    if (!lineaCamino.useWorldSpace)
    {
      primero = lineaCamino.transform.TransformPoint(primero);
      ultimo = lineaCamino.transform.TransformPoint(ultimo);
    }

    return Vector3.SqrMagnitude(ultimo - origen.transform.position) < Vector3.SqrMagnitude(primero - origen.transform.position);
  }

  Vector2 ConvertirAPuntoCanvas(Vector3 posicionMundo, Vector2 fallback)
  {
    Camera camara = Camera.main;
    if (camara == null || canvasRect == null)
    {
      return fallback;
    }

    Vector3 pantalla = camara.WorldToScreenPoint(posicionMundo);
    if (pantalla.z <= 0f)
    {
      return fallback;
    }

    return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pantalla, null, out Vector2 local) ? local : fallback;
  }

  Vector2 PosicionRuta(float progreso, float offsetLateral)
  {
    if (puntosRutaCanvas.Count < 2)
    {
      return Vector2.Lerp(puntoOrigen, puntoDestino, Mathf.Clamp01(progreso));
    }

    float distanciaTotal = 0f;
    for (int i = 1; i < puntosRutaCanvas.Count; i++)
    {
      distanciaTotal += Vector2.Distance(puntosRutaCanvas[i - 1], puntosRutaCanvas[i]);
    }

    float distanciaObjetivo = distanciaTotal * Mathf.Clamp01(progreso);
    float acumulada = 0f;
    for (int i = 1; i < puntosRutaCanvas.Count; i++)
    {
      Vector2 a = puntosRutaCanvas[i - 1];
      Vector2 b = puntosRutaCanvas[i];
      float tramo = Vector2.Distance(a, b);
      if (tramo <= 0.001f)
      {
        continue;
      }

      if (acumulada + tramo >= distanciaObjetivo)
      {
        Vector2 direccion = (b - a).normalized;
        Vector2 normal = new Vector2(-direccion.y, direccion.x);
        return Vector2.Lerp(a, b, (distanciaObjetivo - acumulada) / tramo) + normal * offsetLateral;
      }

      acumulada += tramo;
    }

    return puntosRutaCanvas[puntosRutaCanvas.Count - 1];
  }

  static float OffsetMarcador(int indice)
  {
    return (indice - (CantidadExploradores - 1) * 0.5f) * 2.2f;
  }

  void AplicarPosicionReticula()
  {
    if (reticula != null)
    {
      reticula.anchoredPosition = puntoDestino;
    }
  }

  void AplicarColorReticula(Color color, float alpha)
  {
    if (anilloInterior != null) anilloInterior.color = ConAlpha(color, alpha * 0.42f);
    if (anilloExterior != null) anilloExterior.color = ConAlpha(color, alpha * 0.22f);
    if (barrido != null)
    {
      Image imagenBarrido = barrido.GetComponent<Image>();
      if (imagenBarrido != null) imagenBarrido.color = ConAlpha(color, alpha * 0.24f);
    }
  }

  static void AplicarMarcador(MarcadorExplorador marcador, Vector2 posicion, Color color, float alpha, float escala)
  {
    Color colorScout = new Color(color.r * 0.62f, color.g * 0.62f, color.b * 0.62f, color.a);
    marcador.rect.anchoredPosition = posicion;
    marcador.rect.localScale = Vector3.one * escala;
    marcador.cuerpo.color = ConAlpha(colorScout, alpha);
    marcador.halo.color = ConAlpha(colorScout, alpha * 0.06f);
  }

  ScoutExplorationRingGraphic CrearAnillo(string nombre, Transform padre, float radio, float grosor, int segmentos)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(ScoutExplorationRingGraphic));
    go.transform.SetParent(padre, false);
    RectTransform rect = go.GetComponent<RectTransform>();
    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.sizeDelta = Vector2.one * radio * 2f;
    ScoutExplorationRingGraphic ring = go.GetComponent<ScoutExplorationRingGraphic>();
    ring.raycastTarget = false;
    ring.Grosor = grosor;
    ring.Segmentos = segmentos;
    ring.color = ColorExploracion;
    return ring;
  }

  static Image CrearImagen(string nombre, Transform padre, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Color color)
  {
    GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
    go.transform.SetParent(padre, false);
    Image image = go.GetComponent<Image>();
    image.raycastTarget = false;
    image.color = color;
    RectTransform rect = image.rectTransform;
    rect.anchorMin = anchorMin;
    rect.anchorMax = anchorMax;
    rect.anchoredPosition = Vector2.zero;
    rect.sizeDelta = size;
    return image;
  }

  static void EstirarPantalla(RectTransform rect)
  {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
  }

  static Color ConAlpha(Color color, float alpha)
  {
    color.a = Mathf.Clamp01(alpha);
    return color;
  }

  void CerrarInmediato()
  {
    if (activa == this)
    {
      activa = null;
    }
    gameObject.SetActive(false);
    Destroy(gameObject);
  }

  void OnDestroy()
  {
    if (activa == this)
    {
      activa = null;
    }
  }
}

public sealed class ScoutExplorationRouteGraphic : MaskableGraphic
{
  readonly List<Vector2> puntos = new List<Vector2>();
  float progreso;

  public float Progreso { set { progreso = Mathf.Clamp01(value); SetVerticesDirty(); } }

  public void EstablecerPuntos(IList<Vector2> nuevosPuntos)
  {
    puntos.Clear();
    if (nuevosPuntos != null)
    {
      for (int i = 0; i < nuevosPuntos.Count; i++)
      {
        puntos.Add(nuevosPuntos[i]);
      }
    }
    SetVerticesDirty();
  }

  protected override void OnPopulateMesh(VertexHelper vh)
  {
    vh.Clear();
    if (puntos.Count < 2 || progreso <= 0f)
    {
      return;
    }

    float distanciaTotal = 0f;
    for (int i = 1; i < puntos.Count; i++)
    {
      distanciaTotal += Vector2.Distance(puntos[i - 1], puntos[i]);
    }

    if (distanciaTotal <= 0.01f)
    {
      return;
    }

    float distanciaVisible = distanciaTotal * progreso;
    float acumulada = 0f;
    const float semigrosor = 0.7f;

    for (int i = 1; i < puntos.Count; i++)
    {
      Vector2 p0 = puntos[i - 1];
      Vector2 p1 = puntos[i];
      float tramo = Vector2.Distance(p0, p1);
      if (tramo <= 0.001f)
      {
        continue;
      }

      if (acumulada >= distanciaVisible)
      {
        break;
      }

      if (acumulada + tramo > distanciaVisible)
      {
        p1 = Vector2.Lerp(p0, p1, (distanciaVisible - acumulada) / tramo);
      }

      Vector2 direccion = (p1 - p0).normalized;
      Vector2 normal = new Vector2(-direccion.y, direccion.x) * semigrosor;
      AgregarQuad(vh, p0 - normal, p0 + normal, p1 + normal, p1 - normal, color);
      acumulada += tramo;
    }
  }

  static void AgregarQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
  {
    int baseVertice = vh.currentVertCount;
    UIVertex vertice = UIVertex.simpleVert;
    vertice.color = color;
    vertice.position = a; vh.AddVert(vertice);
    vertice.position = b; vh.AddVert(vertice);
    vertice.position = c; vh.AddVert(vertice);
    vertice.position = d; vh.AddVert(vertice);
    vh.AddTriangle(baseVertice, baseVertice + 1, baseVertice + 2);
    vh.AddTriangle(baseVertice, baseVertice + 2, baseVertice + 3);
  }
}

public sealed class ScoutExplorationRingGraphic : MaskableGraphic
{
  public float Grosor = 2f;
  public int Segmentos = 64;

  protected override void OnPopulateMesh(VertexHelper vh)
  {
    vh.Clear();
    int segmentos = Mathf.Clamp(Segmentos, 12, 128);
    float radioExterior = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;
    float radioInterior = Mathf.Max(0f, radioExterior - Grosor);

    for (int i = 0; i < segmentos; i++)
    {
      float a0 = i * Mathf.PI * 2f / segmentos;
      float a1 = (i + 1f) * Mathf.PI * 2f / segmentos;
      Vector2 dir0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0));
      Vector2 dir1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1));
      AgregarQuad(vh, dir0 * radioInterior, dir0 * radioExterior, dir1 * radioExterior, dir1 * radioInterior, color);
    }
  }

  static void AgregarQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
  {
    int baseVertice = vh.currentVertCount;
    UIVertex vertice = UIVertex.simpleVert;
    vertice.color = color;
    vertice.position = a; vh.AddVert(vertice);
    vertice.position = b; vh.AddVert(vertice);
    vertice.position = c; vh.AddVert(vertice);
    vertice.position = d; vh.AddVert(vertice);
    vh.AddTriangle(baseVertice, baseVertice + 1, baseVertice + 2);
    vh.AddTriangle(baseVertice, baseVertice + 2, baseVertice + 3);
  }
}
