using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
//using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Nodo : MonoBehaviour
{
  private ContenedorDeNodos scContenedorNodos2;

  // --- Datos del nodo ---
  public int tipoNodo;     //1-Batalla  2-Elie  3-Evento  4-Claro
  public int posXNodo;     // 1..11
  public int posYNodo;     // 1..5 (A..E)
  public bool nodoDespejado;
  public int cantidadConexiones = 0;
  public int costoMovimiento = 1;
  public List<Nodo> DestinosPosibles = new List<Nodo>();
  public bool revelado = false;
  public List<int> ObligatorioEnZona = new List<int>();
  public List<int> ProhibidoEnZona = new List<int>();

  public MapaManager scMapaManager;

  // --- Visual caminos ---
  [Header("Caminos")]
  public GameObject linePrefab;          // Debe traer LineRenderer
  public float lineWidth = 0.6f;         // Ancho de la cinta (CaminoMesh)
  public float lineHeightOffset = 0.02f; // Evitar z-fighting
  const float CaminoAnchoBaseMultiplicador = 0.75f;
  const float CaminoAnchoDificilMultiplicador = 0.62f;
  const float CaminoAlturaMinimaSobreRelieve = 0.055f;
  const float CaminoYOffsetMallaMinimo = 0.025f;
  const float ToleranciaCoincidenciaCaminoXZ = 0.18f;

  // Materiales
  public Material MaterialCaminoOriginal;
  public Material MaterialCaminoMarcado;
  public Material MaterialCaminoUsado;
  public Material MaterialAtajo;
  public Material caminoLento;
  private Material caminoLentoVisual;

  // Lógica movimiento
  public float velocidadMovimiento = 4f;

  // Internos
  public bool yatiroConexiones = false;
  Nodo vieneDeNodo;
  bool esMisterioso = false; // Nodo no revelado visualmente
  public bool nodoIncendiado = false;
  public bool nodoRitual = false;
  int numVisualActual = -1;
  const int CodigoSettlement = 4;
  const int IndiceVisualSettlement = 4;
  const float MultiplicadorEscalaSettlement = 1.18f;
  bool escalaSettlementInicializada = false;
  Vector3 escalaSettlementOriginal = Vector3.one;
  bool atajoSubterraneoPendiente = false;
  private static GameObject undergroundTravelMarker;

  bool EsSettlement()
  {
    return tipoNodo == CodigoSettlement;
  }

  bool PuedeTenerIncendioPersistente()
  {
    return !EsSettlement();
  }

  bool PuedeTenerRitualPersistente()
  {
    return !EsSettlement();
  }

  void LimpiarEstadosPersistentesNoValidos()
  {
    bool tutorialActivo = CampaignManager.Instance != null &&
                          CampaignManager.Instance.scTutorialManager != null &&
                          CampaignManager.Instance.scTutorialManager.tutorialActivo;

    if (tutorialActivo || !PuedeTenerIncendioPersistente())
    {
      nodoIncendiado = false;
    }

    if (tutorialActivo || !PuedeTenerRitualPersistente())
    {
      nodoRitual = false;
    }
  }

  private class UndergroundAudioFxState
  {
    public AudioReverbFilter reverb;
    public AudioLowPassFilter lowPass;
    public AudioEchoFilter echo;

    public bool createdReverb;
    public bool createdLowPass;
    public bool createdEcho;

    public bool reverbWasEnabled;
    public bool lowPassWasEnabled;
    public bool echoWasEnabled;

    public AudioReverbPreset reverbPresetBefore;
    public float lowPassCutoffBefore;
    public float lowPassResonanceBefore;
    public float echoWetBefore;
    public float echoDryBefore;
    public float echoDelayBefore;
    public float echoDecayBefore;
  }

  void Start()
  {
    scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;
    PrepararMaterialCaminoLento();

    int random = UnityEngine.Random.Range(0, 100);
    if (random < 20 && posXNodo > 1) //20% camino difícil
    { costoMovimiento = 2; }

    EsconderSiNedukazal();
  }

  public void LlegoCaravana()
  {
    CampaignManager.Instance.MoviendoCaravana = false;
    scMapaManager.nodoActual = this;
    if (scMapaManager != null)
    {
      scMapaManager.NotificarFinViajeCaravana();
    }

    // Apagar animaciones con un retraso aleatorio hasta 0.25s por cada follower
    if (scMapaManager != null)
    {
      IEnumerator SetWalkingFalseAfterRandomDelay(GameObject follower)
      {
      if (follower == null) yield break;
      float delay = UnityEngine.Random.Range(0f, 0.25f);
      yield return new WaitForSeconds(delay);
      if (follower.transform.childCount > 0)
      {
        var animator = follower.transform.GetChild(0).GetComponent<Animator>();
        if (animator != null) animator.SetBool("IsWalking", false);
      }
      }

      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower1));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower2));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower3));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower4));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower5));
      StartCoroutine(SetWalkingFalseAfterRandomDelay(scMapaManager.goCaravanafollower6));
    }

    string hayExploracionExplorador = "";
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers.PuedeRealizarActividades() && pers.ActividadSeleccionada == 9) hayExploracionExplorador = pers.sNombre;
      if (pers.Camp_Enfermo > 0) pers.Camp_Enfermo -= 1;
      pers.ReducirCampBendecido();
      if (pers.Camp_Moral > 0) pers.Camp_Moral -= 1;
      if (pers.Camp_Moral < 0) pers.Camp_Moral += 1;
    }

    if (hayExploracionExplorador != "")
    {
      TiradaExploracion(200, false);
      TiradaExploracion(40, true, hayExploracionExplorador);
    }
    else
    {
      TiradaExploracion(200, false);
    }

    int fatigaSuma = 1;
    int esperanzaSuma = 0;

    if (CampaignManager.Instance.SeLlevaDemasiadaCarga())
    {
      fatigaSuma += 1;
      esperanzaSuma -= 10;
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-La Caravana ha viajado con exceso de Carga. -10 Esperanza +1 Fatiga"));
    }

    int chancesAtajo = 15;
    chancesAtajo += 5 * CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9);
    if (CampaignManager.Instance.scAtributosZona.ID == 3) { chancesAtajo = 0; } // En Nedukazal no hay atajos
    if (CampaignManager.Instance.scTutorialManager.tutorialActivo ) { chancesAtajo = 0; } // En Tutorial no hay atajos
    
    if (UnityEngine.Random.Range(0, 100) < chancesAtajo && posXNodo < 9)
    {
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Se ha encontrado un atajo subterráneo."));
      EncontrarAtajo(2, 0);
    }

    CampaignManager.Instance.CambiarFatigaActual(fatigaSuma);
    CampaignManager.Instance.CambiarEsperanzaActual(esperanzaSuma);
    CampaignManager.Instance.LlegarANodo(ObtenerTipoNodoAlLlegar(), posXNodo, this);

    MarcarCaminosPosibles();
  }

  public void EncontrarAtajo(int X, int Y)
  {
    if (scContenedorNodos2 == null)
      scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;
    int zonaId = -1;
    if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
      zonaId = CampaignManager.Instance.scAtributosZona.ID;

    int nextX = posXNodo + X;
    List<Nodo> posiblesAtajos = new List<Nodo>();

    for (int dy = -Y; dy <= Y; dy++)
    {
      int y = posYNodo + dy;
      if (y < 1 || y > 5) continue;

      Nodo c = scContenedorNodos2.ObtenerNodoSegunXY(nextX, y);
      if (c == null) continue;
      if (!EstaPermitidoEnZona(c, zonaId)) continue;
      if (DestinosPosibles.Contains(c)) continue;

      bool hayRutaIntermedia = false;
      foreach (var b in DestinosPosibles)
      {
        if (b == null) continue;
        if (b.posXNodo != posXNodo + 1) continue;
        if (b.DestinosPosibles != null && b.DestinosPosibles.Contains(c))
        {
          hayRutaIntermedia = true;
          break;
        }
      }

      if (!hayRutaIntermedia) posiblesAtajos.Add(c);
    }

    if (posiblesAtajos.Count == 0)
    {
      if (Y < 2)
      {
        EncontrarAtajo(X, Y + 1);
        return;
      }

      for (int dy = -2; dy <= 2; dy++)
      {
        int y = posYNodo + dy;
        if (y < 1 || y > 5) continue;

        Nodo c = scContenedorNodos2.ObtenerNodoSegunXY(nextX, y);
        if (c != null && !DestinosPosibles.Contains(c) && EstaPermitidoEnZona(c, zonaId))
          posiblesAtajos.Add(c);
      }
    }

    if (posiblesAtajos.Count > 0)
    {
      Nodo elegido = posiblesAtajos[UnityEngine.Random.Range(0, posiblesAtajos.Count)];
      ConectarConNodo(elegido, true);
      elegido.Revelar(true);
    }
  }

  #region LOGICA CAMINOS
  public void DeterminarConexiones()
  {
    int xadelante = posXNodo + 1;
    scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;
    int zonaId = -1;
    if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
      zonaId = CampaignManager.Instance.scAtributosZona.ID;

    if (yatiroConexiones) return;
    yatiroConexiones = true;

    if ((posXNodo == 0) && (posYNodo == 0)) // Nodo origen
    {
      int random = 1;// UnityEngine.Random.Range(1, 5); // 1..4

      if (random == 1)
      {
        IntentarConectar(1, 1, zonaId);
        IntentarConectar(1, 3, zonaId);
        IntentarConectar(1, 5, zonaId);
      }
      /*  else if (random == 2)
        {
          IntentarConectar(1, 2, zonaId);
          IntentarConectar(1, 3, zonaId);
          IntentarConectar(1, 4, zonaId);
        }
        else if (random == 3)
        {
          IntentarConectar(1, 2, zonaId);
          IntentarConectar(1, 3, zonaId);
          IntentarConectar(1, 5, zonaId);
        }
        else if (random == 4)
        {
          IntentarConectar(1, 1, zonaId);
          IntentarConectar(1, 3, zonaId);
          IntentarConectar(1, 4, zonaId);
        }*/

      TiradaExploracion(300, false);
    }
    else if (posXNodo == 1)
    {
      IntentarConectar(xadelante, posYNodo - 1, zonaId);
      IntentarConectar(xadelante, posYNodo, zonaId);
    }
    else if (posYNodo == 1 && posXNodo < 10)
    {
      int random1 = UnityEngine.Random.Range(1, 5);
      if (random1 == 1) IntentarConectar(xadelante, 1, zonaId);
      else if (random1 == 2) { IntentarConectar(xadelante, 1, zonaId); IntentarConectar(xadelante, 2, zonaId); }
      else if (random1 == 3) IntentarConectar(xadelante, 2, zonaId);
      else if (random1 == 4) { IntentarConectar(xadelante, 1, zonaId); IntentarConectar(xadelante, 2, zonaId); }
    }
    else if (posYNodo == 2 && posXNodo < 10)
    {
      int random2 = UnityEngine.Random.Range(1, 6);
      if (random2 == 1) IntentarConectar(xadelante, 1, zonaId);
      else if (random2 == 2) { IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 3, zonaId); }
      else if (random2 == 3) IntentarConectar(xadelante, 2, zonaId);
      else if (random2 == 4) { IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 3, zonaId); }
      else if (random2 == 5) IntentarConectar(xadelante, 3, zonaId);
    }
    else if (posYNodo == 3 && posXNodo < 10)
    {
      int random3 = UnityEngine.Random.Range(1, 6);
      if (random3 == 1) IntentarConectar(xadelante, 2, zonaId);
      else if (random3 == 2) IntentarConectar(xadelante, 3, zonaId);
      else if (random3 == 3) { IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 4, zonaId); }
      else if (random3 == 4) { IntentarConectar(xadelante, 3, zonaId); IntentarConectar(xadelante, 4, zonaId); }
      else if (random3 == 5) { IntentarConectar(xadelante, 3, zonaId); IntentarConectar(xadelante, 2, zonaId); IntentarConectar(xadelante, 4, zonaId); }
    }
    else if (posYNodo == 4 && posXNodo < 10)
    {
      int random4 = UnityEngine.Random.Range(1, 6);
      if (random4 == 1) IntentarConectar(xadelante, 4, zonaId);
      else if (random4 == 2) { IntentarConectar(xadelante, 4, zonaId); IntentarConectar(xadelante, 5, zonaId); }
      else if (random4 == 3) IntentarConectar(xadelante, 3, zonaId);
      else if (random4 == 4) IntentarConectar(xadelante, 4, zonaId);
      else if (random4 == 5) { IntentarConectar(xadelante, 4, zonaId); IntentarConectar(xadelante, 3, zonaId); }
    }
    else if (posYNodo == 5 && posXNodo < 10)
    {
      int random5 = UnityEngine.Random.Range(1, 5);
      if (random5 == 1) IntentarConectar(xadelante, 5, zonaId);
      else if (random5 == 2) { IntentarConectar(xadelante, 5, zonaId); IntentarConectar(xadelante, 4, zonaId); }
      else if (random5 == 3) IntentarConectar(xadelante, 4, zonaId);
      else if (random5 == 4) { IntentarConectar(xadelante, 4, zonaId); IntentarConectar(xadelante, 5, zonaId); }
    }
    else if (posXNodo == 10)
    {
      IntentarConectar(11, 10, zonaId);
    }

    if (DestinosPosibles.Count == 0 && posXNodo < 10)
      ConectarFallbackSiguienteColumna(xadelante, zonaId);
  }

  public void ConectarConNodo(Nodo nodoB, bool esPorAbajo = false, bool propagar = true, bool ignorarRestricciones = false)
  {
    if (nodoB == null) return;

    int zonaId = -1;
    if (CampaignManager.Instance != null && CampaignManager.Instance.scAtributosZona != null)
      zonaId = CampaignManager.Instance.scAtributosZona.ID;
    if (!ignorarRestricciones && !EstaPermitidoEnZona(nodoB, zonaId)) return;
    if (DestinosPosibles.Contains(nodoB)) return;

    Nodo nodoA = this;
    bool esCaminoDificil = !esPorAbajo && nodoB.costoMovimiento > 1;
    nodoA.DestinosPosibles.Add(nodoB);
    cantidadConexiones++;

    // Crear línea
    GameObject lineObject = Instantiate(linePrefab, this.transform);
    lineObject.name = esPorAbajo ? "LineaCaminosSubterraneo" : "LineaCaminos";

    LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
    if (lineRenderer == null)
    {
      Debug.LogError("El prefab de línea no tiene LineRenderer.");
      return;
    }

    // Aseguramos world space para que CaminoMesh convierta bien a local
    lineRenderer.useWorldSpace = true;

    Vector3 p0 = nodoA.transform.position;
    Vector3 p3 = nodoB.transform.position;
    MapDecorator mapDecorator = ObtenerDecoradorMapa();

    // Dirección y perpendicular para "empujar" la curva
    Vector3 dir = (p3 - p0);
    dir.y = 0f;
    float dist = dir.magnitude;
    if (dist < 0.001f) dist = 0.001f;
    dir /= dist;

    Vector3 perp = Vector3.Cross(Vector3.up, dir);
    if (perp.sqrMagnitude < 0.0001f) perp = Vector3.Cross(Vector3.forward, dir);
    perp.Normalize();

    // Curvatura: más marcada si es atajo, pero SIN tocar Y
    float outward;
    if (esPorAbajo)
    {
      // atajo: curva notoria
      outward = UnityEngine.Random.Range(2.3f, 3.2f);
    }
    else if (esCaminoDificil)
    {
      outward = UnityEngine.Random.Range(0.36f, 0.67f);
    }
    else
    {
      // 30% de probabilidad de una curvatura más pronunciada también para no-atajo
      if (UnityEngine.Random.value < 0.24f && cantidadConexiones < 2 && dist > 7.5f)
        outward = UnityEngine.Random.Range(0.9f, 1.35f); // curva visible pero controlada
      else
        outward = UnityEngine.Random.Range(0.16f, 0.46f); // normal: leve
    }

    // Evitar que los primeros 2 ramos salgan muy curvos
    if (!esPorAbajo && cantidadConexiones < 2)
      outward *= esCaminoDificil ? 0.88f : 0.72f;

    float outwardMaximo = esPorAbajo
      ? Mathf.Max(2.2f, dist * 0.22f)
      : esCaminoDificil
        ? Mathf.Clamp(dist * 0.125f, 0.3f, 0.67f)
        : Mathf.Clamp(dist * 0.075f, 0.18f, 0.62f);
    outward = Mathf.Min(outward, outwardMaximo);

    float sideSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;

    // Dónde colocar puntos de control
    float t1 = UnityEngine.Random.Range(0.16f, 0.24f);
    float t2 = UnityEngine.Random.Range(0.60f, 0.76f);

    // Pequeña variación lateral
    float jitter1 = UnityEngine.Random.Range(-0.5f, 0.5f);
    float jitter2 = UnityEngine.Random.Range(-0.5f, 0.5f);

    Vector3 p1 = p0 + dir * (dist * t1) + perp * (sideSign * outward * (0.35f + 0.65f * Mathf.Abs(jitter1)));
    Vector3 p2 = p3 - dir * (dist * (1f - t2)) + perp * (sideSign * outward * (0.35f + 0.65f * Mathf.Abs(jitter2)));

    // Curva Bézier: SIEMPRE PLANA en Y (evita hundirse bajo el suelo)
    int resolutionMinima = esCaminoDificil ? 30 : 22;
    int resolutionMaxima = esCaminoDificil ? 54 : 42;
    float densidadMuestreo = esCaminoDificil ? 10.5f : 8.5f;
    int resolution = Mathf.Clamp(Mathf.RoundToInt(dist * densidadMuestreo), resolutionMinima, resolutionMaxima);
    float frecuenciaCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(1.14f, 1.34f) : 0f;
    float amplitudCaminoSinuoso = esCaminoDificil ? Mathf.Min(dist * 0.045f, UnityEngine.Random.Range(0.125f, 0.21f)) : 0f;
    float caosCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(0.011f, 0.025f) : 0f;
    float faseCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(0f, Mathf.PI * 2f) : 0f;
    float semillaCaosCaminoSinuoso = esCaminoDificil ? UnityEngine.Random.Range(0f, 100f) : 0f;
    float offsetCaminoSobreRelieve = Mathf.Max(CaminoAlturaMinimaSobreRelieve, lineHeightOffset * 3f);
    lineRenderer.positionCount = resolution;
    for (int i = 0; i < resolution; i++)
    {
      float t = i / (float)(resolution - 1);
      Vector3 point = BezierCurve.GetPoint(p0, p1, p2, p3, t);

      // Forzamos Y a la interpolación del tramo (plano) + leve offset si querés
      if (esCaminoDificil && i > 0 && i < resolution - 1)
      {
        point += perp * CalcularDesvioCaminoSinuoso(t, frecuenciaCaminoSinuoso, amplitudCaminoSinuoso, caosCaminoSinuoso, faseCaminoSinuoso, semillaCaosCaminoSinuoso);
      }

      if (mapDecorator != null && mapDecorator.TrySampleSurface(point, out var surfacePoint, out _, offsetCaminoSobreRelieve))
      {
        point.y = surfacePoint.y;
      }
      else
      {
        point.y = Mathf.Lerp(p0.y, p3.y, t);
      }

      lineRenderer.SetPosition(i, point);
    }

    // Construir malla plana del camino
    var caminoMesh = lineObject.GetComponent<CaminoMesh>();
    if (caminoMesh == null) caminoMesh = lineObject.AddComponent<CaminoMesh>();
    caminoMesh.SetWidth(ObtenerAnchoVisualCamino(esCaminoDificil));
    caminoMesh.SetYOffset(Mathf.Max(CaminoYOffsetMallaMinimo, lineHeightOffset));
    caminoMesh.RebuildFromLine();

    // Material según tipo (normal vs atajo)
    SetMaterialCamino(lineObject.transform, esPorAbajo ? MaterialAtajo : MaterialCaminoOriginal);

    // Continuar tirando conexiones
    if (propagar)
      nodoB.DeterminarConexiones();
  }
  // Resetea este nodo para reutilizarlo en una nueva zona
  public void ResetearParaNuevaZona()
  {
    tipoNodo = 0;
    nodoDespejado = false;
    cantidadConexiones = 0;
    costoMovimiento = 1;
    revelado = false;
    yatiroConexiones = false;
    DestinosPosibles.Clear();
    nodoIncendiado = false;
    nodoRitual = false;
    esMisterioso = false;
    numVisualActual = -1;
    atajoSubterraneoPendiente = false;
    vieneDeNodo = null;

    var destruir = new List<GameObject>();
    foreach (Transform child in transform)
    {
      if (child.name.Contains("LineaCaminos")) destruir.Add(child.gameObject);
      if (child.name.Contains("Nodo")) child.gameObject.SetActive(false);
    }
    foreach (var go in destruir) Destroy(go);

    AplicarEstiloVisualSettlement(false);
    transform.GetChild(0).gameObject.SetActive(true);
    transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
    SincronizarVFXPersistentes();
    Invoke("EsconderSiNedukazal", 0.15f);
    gameObject.SetActive(true);
  }

  public void RestaurarDesdeSave(NodeSaveData data)
  {
    if (data == null)
    {
      return;
    }

    gameObject.SetActive(true);
    tipoNodo = data.tipoNodo;
    nodoDespejado = data.nodoDespejado;
    cantidadConexiones = 0;
    costoMovimiento = data.costoMovimiento;
    revelado = data.revelado;
    yatiroConexiones = data.yatiroConexiones;
    nodoIncendiado = data.nodoIncendiado;
    nodoRitual = data.nodoRitual;
    atajoSubterraneoPendiente = data.atajoSubterraneoPendiente;
    DestinosPosibles.Clear();
    vieneDeNodo = null;
    LimpiarEstadosPersistentesNoValidos();
    AplicarVisualGuardado(data.visualCode, data.esMisterioso);
    SincronizarVFXPersistentes();
    gameObject.SetActive(data.activo);
  }

  public void AplicarVisualGuardado(int visualCode, bool estadoMisterioso)
  {
    DesactivarGraficosNodo();
    esMisterioso = estadoMisterioso;
    numVisualActual = visualCode;

    int codigoAAplicar = numVisualActual > 0 ? numVisualActual : tipoNodo;
    if (codigoAAplicar <= 0)
    {
      ActivarVisualBaseNoRevelado();
      return;
    }

    if (!ActivarVisualPorCodigo(codigoAAplicar))
    {
      numVisualActual = -1;
      ActivarVisualBaseNoRevelado();
    }
  }

  public void ForzarSettlement(bool mostrarVisualDesdeInicio)
  {
    tipoNodo = CodigoSettlement;
    esMisterioso = false;
    atajoSubterraneoPendiente = false;
    nodoIncendiado = false;
    nodoRitual = false;
    SincronizarVFXPersistentes();

    if (mostrarVisualDesdeInicio)
    {
      revelado = true;
      ActivarNodoVisual(CodigoSettlement, false, true);
      return;
    }

    revelado = false;
    numVisualActual = -1;
    DesactivarGraficosNodo();
    ActivarVisualBaseNoRevelado();
  }

  int ObtenerTipoNodoAlLlegar()
  {
    bool llegoPorAtajo = vieneDeNodo != null && (posXNodo - vieneDeNodo.posXNodo > 1);
    int tipoNodoLlegada = tipoNodo;

    if (llegoPorAtajo && atajoSubterraneoPendiente)
    {
      atajoSubterraneoPendiente = false;
      tipoNodoLlegada = 12;
    }

    return tipoNodoLlegada;
  }

  void ConfigurarResultadoAtajoSubterraneo()
  {
    atajoSubterraneoPendiente = false;

    if (!revelado || scMapaManager == null)
    {
      return;
    }

    if (scMapaManager.TirarEmboscadaSubterraneaAtajo(this))
    {
      atajoSubterraneoPendiente = true;
    }
  }

  public void RefrescarCaminosMarcadosDesdeEstadoActual()
  {
    MarcarCaminosPosibles();
  }

  public void PosicionarObjetoEnNodo(GameObject go)
  {
    if (go == null)
    {
      return;
    }

    go.transform.position = transform.position;
  }

  private void OnMouseDown()
  {
    if (CampaignManager.Instance != null)
    {
      AsentamientoManager asentamientoManager = CampaignManager.Instance.ObtenerAsentamientoManager();
      if (asentamientoManager != null && asentamientoManager.TieneInteraccionActiva)
      {
        return;
      }
    }

    var tm = CampaignManager.Instance != null ? CampaignManager.Instance.scTutorialManager : null;
    if (tm != null && tm.tutorialActivo)
    {
      // permitir interacción solo en los pasos 2, 11, 17, 21 durante el tutorial
      if (!(tm.pasoActual == 2 || tm.pasoActual == 11 || tm.pasoActual == 17 || tm.pasoActual == 21|| tm.pasoActual == 30))
        return;
    }

    if (EventSystem.current.IsPointerOverGameObject() && !TooltipNodos.Instance.tooltipObject.activeInHierarchy)
      return;

    if (scMapaManager.nodoActual.DestinosPosibles.Contains(this))
    {
      if (tm != null && tm.tutorialActivo)
      {
        if (tm.pasoActual == 2)
          tm.cerrarPasoEspecifico(2);

        if (tm.pasoActual == 11 || tm.pasoActual == 17 || tm.pasoActual == 21 || tm.pasoActual == 30)
          tm.SiguientePaso();
      }

      CampaignManager.Instance.MoviendoCaravana = true;
      RuntimeAnalytics.TrackDesign(
        "campaign",
        "node_selected",
        RuntimeAnalytics.SanitizeToken("type_" + tipoNodo + "_x" + posXNodo + "_y" + posYNodo));
      MoverJugadorANodo(scMapaManager.nodoActual, this);

      if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 7)
        CampaignManager.Instance.scTutorialManager.SiguientePaso();

      if (posXNodo - scMapaManager.nodoActual.posXNodo > 1)
      {
        CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Al viajar por el atajo subterráneo, la moral de la caravana disminuye. -5 Esperanza"));
        CampaignManager.Instance.CambiarEsperanzaActual(-5);
      }
    }
  }


  public void MoverJugadorANodo(Nodo nodoOrigen, Nodo nodoDestino)
  {
    if (nodoDestino == null || !nodoOrigen.DestinosPosibles.Contains(nodoDestino))
    {
      Debug.LogWarning("Nodo destino no vélido o no está en la lista de destinos posibles.");
      return;
    }

    // Buscar línea
    const float multiplicadorVelocidadMinimaFatiga = 0.6f;
    float velocidadBase = 0.75f + 0.6f / nodoDestino.costoMovimiento;
    int cansancio = CampaignManager.Instance.GetFatigaActual();
    float velocidadReducidaPorFatiga = velocidadBase - cansancio * 0.07f;
    velocidadMovimiento = Mathf.Max(velocidadBase * multiplicadorVelocidadMinimaFatiga, velocidadReducidaPorFatiga);

    Transform lineaTransform = null;
    foreach (Transform child in nodoOrigen.transform)
    {
      if (!child.name.Contains("LineaCaminos")) continue;
      LineRenderer lr = child.GetComponent<LineRenderer>();
      if (CoincideExtremoLineaConPosicion(lr, nodoDestino.transform.position))
      {
        lineaTransform = child;
        break;
      }
    }

    if (lineaTransform == null)
    {
      Debug.LogWarning("No se encontró la línea correspondiente entre los nodos.");
      return;
    }

    bool viajeSubterraneo = lineaTransform.name.Contains("Subterraneo")
      || (nodoDestino.posXNodo - nodoOrigen.posXNodo > 1);

    vieneDeNodo = nodoOrigen;
    CampaignManager.Instance.ViajeIniciado(nodoDestino, viajeSubterraneo);

    if (scMapaManager != null && scMapaManager.goCaravana != null)
    {
      var girarCaravana = scMapaManager.goCaravana.GetComponent<GirarCaravana>();
      if (girarCaravana != null)
        girarCaravana.CambiarSpriteSegunRuta(nodoOrigen, nodoDestino);
    }

   /* StartCoroutine(MoverAloLargoDeLaCurva(0.0f, true, scMapaManager.goCaravana, lineaTransform.GetComponent<LineRenderer>(), 1.5f));
    StartCoroutine(MoverAloLargoDeLaCurva(0.35f, false, scMapaManager.goCaravanafollower1, lineaTransform.GetComponent<LineRenderer>(), 1.3f));
    StartCoroutine(MoverAloLargoDeLaCurva(0.5f, false, scMapaManager.goCaravanafollower2, lineaTransform.GetComponent<LineRenderer>(), 1.15f));
    StartCoroutine(MoverAloLargoDeLaCurva(0.75f, false, scMapaManager.goCaravanafollower3, lineaTransform.GetComponent<LineRenderer>(), 1.0f));
    StartCoroutine(MoverAloLargoDeLaCurva(1.0f, false, scMapaManager.goCaravanafollower4, lineaTransform.GetComponent<LineRenderer>(), 0.95f));
    StartCoroutine(MoverAloLargoDeLaCurva(1.15f, false, scMapaManager.goCaravanafollower5, lineaTransform.GetComponent<LineRenderer>(), 0.85f));
    StartCoroutine(MoverAloLargoDeLaCurva(1.25f, false, scMapaManager.goCaravanafollower6, lineaTransform.GetComponent<LineRenderer>(), 0.8f));
*/
    StartCoroutine(MoverConvoyEnLinea(lineaTransform.GetComponent<LineRenderer>(), viajeSubterraneo));
  }

 /* private IEnumerator MoverAloLargoDeLaCurva(float delay, bool esLaLider, GameObject caravana, LineRenderer lineRenderer, float velRotacion)
  {
    if (caravana == null) yield break;

    GameObject caravanarotacion = esLaLider ? caravana.transform.GetChild(4).gameObject : caravana.transform.GetChild(0).gameObject;

    int resolution = lineRenderer.positionCount;

    Vector3 inicio = lineRenderer.GetPosition(0);
    Vector3 fin = lineRenderer.GetPosition(resolution - 1);
    Vector3 dirAvance = (fin - inicio).normalized;

    // Ajustes
    float velocidadRotacion = velRotacion;                 // más alto = gira más rápido
    float lookAhead = Mathf.Max(0.01f, 2f / Mathf.Max(2, resolution)); // cuánto "mira" hacia adelante en la curva

    // --- Rotación durante el delay (no mover aún) ---
    float elapsed = 0f;
    while (elapsed < delay)
    {
      float tFuture = Mathf.Clamp01(0f + lookAhead);
      Vector3 posFutura = CalcularPosicionEnCurva(lineRenderer, tFuture);

      Vector3 forward = (posFutura - caravanarotacion.transform.position);
      forward.y = 0f;
      if (forward.sqrMagnitude > 0.000001f)
      {
        Quaternion rotObjetivo = Quaternion.LookRotation(forward.normalized, Vector3.up);
        float k = 1f - Mathf.Exp(-velocidadRotacion * Time.deltaTime);
        caravanarotacion.transform.rotation = Quaternion.Slerp(caravanarotacion.transform.rotation, rotObjetivo, k);
      }

      elapsed += Time.deltaTime;
      yield return null;
    }

  // Comienza el movimiento
float t = 0f;
Vector3 ultima = caravana.transform.position;

// Cuánto del recorrido usás para acelerar y frenar (0.15 = 15%)
const float ramp = 0.35f;

while (t < 1f)
{
  float tNorm = Mathf.Clamp01(t);

  // Factor de velocidad: acelera al inicio y frena al final
  float inF  = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tNorm / ramp));
  float outF = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - tNorm) / ramp));

  float speedFactor = inF * outF;

  // Piso para que no se quede "cerquita" eternamente
  speedFactor = Mathf.Max(0.03f, speedFactor);

  // Avance de t (acá va el suavizado)
  t += Time.deltaTime * velocidadMovimiento / resolution * speedFactor;

  // Snap final
  if (t >= 0.999f) t = 1f;

  float tClamped = Mathf.Clamp01(t);

  // Posición SIEMPRE con tClamped (monótono)
  Vector3 nuevaPosicion = CalcularPosicionEnCurva(lineRenderer, tClamped);

  Vector3 delta = nuevaPosicion - ultima;
  if (Vector3.Dot(delta, dirAvance) < 0f)
    nuevaPosicion = ultima;

  // Rotación siguiendo la curva (lookahead también con tClamped)
  float tFuturo = Mathf.Clamp01(tClamped + lookAhead);
  Vector3 posFutura = CalcularPosicionEnCurva(lineRenderer, tFuturo);

  Vector3 forward = (posFutura - nuevaPosicion);
  forward.y = 0f;

  if (forward.sqrMagnitude > 0.000001f)
  {
    Quaternion rotObjetivo = Quaternion.LookRotation(forward.normalized, Vector3.up);
    float k = 1f - Mathf.Exp(-velocidadRotacion * Time.deltaTime);
    caravanarotacion.transform.rotation =
      Quaternion.Slerp(caravanarotacion.transform.rotation, rotObjetivo, k);
  }

  caravana.transform.position = nuevaPosicion;
  ultima = nuevaPosicion;

  yield return null;
}

caravana.transform.position = fin;
if (esLaLider)
{
  LlegoCaravana();
}
  }*/



  private Vector3 CalcularPosicionEnCurva(LineRenderer lineRenderer, float t)
  {
    t = Mathf.Clamp01(t);
    int resolution = lineRenderer.positionCount;

    int indexA = Mathf.FloorToInt(t * (resolution - 1));
    int indexB = Mathf.Clamp(indexA + 1, 0, resolution - 1);

    Vector3 posicionA = lineRenderer.GetPosition(indexA);
    Vector3 posicionB = lineRenderer.GetPosition(indexB);

    float tLocal = t * (resolution - 1) - indexA;

    return Vector3.Lerp(posicionA, posicionB, tLocal);
  }

  public void PrepararCostoMovimientoParaGeneracion()
  {
    costoMovimiento = 1;

    if (posXNodo <= 1)
    {
      return;
    }

    if (UnityEngine.Random.Range(0, 100) < 20)
    {
      costoMovimiento = 2;
    }
  }

  private MapDecorator ObtenerDecoradorMapa()
  {
    if (CampaignManager.Instance == null || CampaignManager.Instance.scAtributosZona == null)
    {
      return null;
    }

    return CampaignManager.Instance.scAtributosZona.GetComponent<MapDecorator>();
  }

  private Quaternion CalcularRotacionConvoyPorRelieve(Vector3 posActual, Vector3 posFutura, bool viajeSubterraneo)
  {
    Vector3 forward = posFutura - posActual;
    if (forward.sqrMagnitude <= 0.000001f)
    {
      return Quaternion.identity;
    }

    if (viajeSubterraneo)
    {
      forward.y = 0f;
      return forward.sqrMagnitude > 0.000001f
        ? Quaternion.LookRotation(forward.normalized, Vector3.up)
        : Quaternion.identity;
    }

    MapDecorator mapDecorator = ObtenerDecoradorMapa();
    Vector3 up = Vector3.up;
    if (mapDecorator != null && mapDecorator.TrySampleSurface(posActual, out _, out var terrainNormal, 0f))
    {
      up = Vector3.RotateTowards(Vector3.up, terrainNormal.normalized, Mathf.Deg2Rad * 10f, 0f);
    }

    Vector3 forwardPlano = Vector3.ProjectOnPlane(forward, up);
    if (forwardPlano.sqrMagnitude <= 0.000001f)
    {
      forwardPlano = new Vector3(forward.x, 0f, forward.z);
    }

    return forwardPlano.sqrMagnitude > 0.000001f
      ? Quaternion.LookRotation(forwardPlano.normalized, up)
      : Quaternion.identity;
  }

  private float ObtenerAnchoVisualCamino(bool esCaminoDificil)
  {
    return lineWidth * (esCaminoDificil ? CaminoAnchoDificilMultiplicador : CaminoAnchoBaseMultiplicador);
  }

  private static float CalcularDesvioCaminoSinuoso(float t, float frecuencia, float amplitud, float caos, float fase, float semilla)
  {
    const float inicioSinuosidad = 0.22f;
    const float finSinuosidad = 0.78f;
    const float suavizadoBorde = 0.11f;

    if (t <= inicioSinuosidad || t >= finSinuosidad)
    {
      return 0f;
    }

    float tNormalizado = Mathf.InverseLerp(inicioSinuosidad, finSinuosidad, t);
    float entrada = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - inicioSinuosidad) / suavizadoBorde));
    float salida = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((finSinuosidad - t) / suavizadoBorde));
    float envolvente = entrada * salida;
    float curvaPrincipal = Mathf.Sin(tNormalizado * Mathf.PI) * Mathf.Sin(tNormalizado * Mathf.PI * 2f) * amplitud;
    float zigzagSuave = Mathf.Sin((tNormalizado * frecuencia * Mathf.PI * 2f) + fase) * (amplitud * 0.3f);
    float ruido = (Mathf.PerlinNoise(semilla, tNormalizado * 3.3f) - 0.5f) * 2f * caos;
    return (curvaPrincipal + zigzagSuave + ruido) * envolvente;
  }

  // --- Helper materiales: aplica al LR y a la malla ---
  private void SetMaterialCamino(Transform linea, Material mat)
  {
    if (mat == caminoLento)
    {
      mat = ObtenerMaterialCaminoLentoVisual();
    }

    var lr = linea.GetComponent<LineRenderer>();
    if (lr != null) lr.sharedMaterial = mat;

    var mr = linea.GetComponent<MeshRenderer>();
    if (mr != null) mr.sharedMaterial = mat;
  }

  private void PrepararMaterialCaminoLento()
  {
    if (caminoLento == null || caminoLentoVisual != null)
    {
      return;
    }

    caminoLentoVisual = new Material(caminoLento);

    if (caminoLentoVisual.HasProperty("_Color"))
    {
      Color colorBase = caminoLento.color;
      float brilloObjetivo = Mathf.Max(Mathf.Max(colorBase.r, colorBase.g), colorBase.b);
      brilloObjetivo = Mathf.Max(brilloObjetivo, 0.66f);
      Color colorGris = new Color(brilloObjetivo, brilloObjetivo, brilloObjetivo, colorBase.a);
      caminoLentoVisual.color = Color.Lerp(colorBase, colorGris, 0.5f);
    }
  }

  private Material ObtenerMaterialCaminoLentoVisual()
  {
    PrepararMaterialCaminoLento();
    return caminoLentoVisual != null ? caminoLentoVisual : caminoLento;
  }

  private bool CoincideExtremoLineaConPosicion(LineRenderer lineRenderer, Vector3 posicionObjetivo)
  {
    if (lineRenderer == null || lineRenderer.positionCount <= 0)
    {
      return false;
    }

    return CoincidePosicionCamino(lineRenderer.GetPosition(lineRenderer.positionCount - 1), posicionObjetivo);
  }

  private bool CoincidePosicionCamino(Vector3 posicionA, Vector3 posicionB)
  {
    Vector2 aXZ = new Vector2(posicionA.x, posicionA.z);
    Vector2 bXZ = new Vector2(posicionB.x, posicionB.z);
    return (aXZ - bXZ).sqrMagnitude <= ToleranciaCoincidenciaCaminoXZ * ToleranciaCoincidenciaCaminoXZ;
  }

  private Nodo ObtenerDestinoSegunLinea(LineRenderer lineRenderer)
  {
    if (lineRenderer == null)
    {
      return null;
    }

    return DestinosPosibles.Find(n => n != null && CoincideExtremoLineaConPosicion(lineRenderer, n.transform.position));
  }

  void MarcarCaminosPosibles()
  {
    // Desde donde venís
    if (vieneDeNodo != null)
    {
      foreach (Transform child in vieneDeNodo.transform)
      {
        if (!child.name.Contains("LineaCaminos")) continue;
        var lr = child.GetComponent<LineRenderer>();
        if (lr == null) continue;

        if (CoincideExtremoLineaConPosicion(lr, transform.position))
          SetMaterialCamino(child, MaterialCaminoUsado);
        else
          SetMaterialCamino(child, MaterialCaminoOriginal);
      }
    }

    // Salientes del nodo actual
    foreach (Transform child in transform)
    {
      if (!child.name.Contains("LineaCaminos")) continue;
      var lr = child.GetComponent<LineRenderer>();
      if (lr == null) continue;

      Nodo nodoDestino = ObtenerDestinoSegunLinea(lr);
      if (nodoDestino == null)
      {
        SetMaterialCamino(child, MaterialCaminoOriginal);
        continue;
      }

      // Camino normal o lento
      SetMaterialCamino(child, nodoDestino.costoMovimiento > 1 ? caminoLento : MaterialCaminoMarcado);

      // Atajo (salto en X)
      if (nodoDestino.posXNodo - posXNodo > 1)
      {
        nodoDestino.costoMovimiento = 2;
        SetMaterialCamino(child, MaterialAtajo);
      }
    }
  }
  #endregion

  public void Revelar(bool esAtajo)
  {
    bool estabaRevelado = revelado;
    revelado = true;
    //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
   
    if (tipoNodo == 0)
    {
      int rand = UnityEngine.Random.Range(1, 8);

      if (posXNodo == 1)
      {
        switch (rand)
        {
          
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 5; break;
          case 5: tipoNodo = 8; break;
          case 6: tipoNodo = 5; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 2)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 7; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 8; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 3)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 11; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 1; break;
          case 6: tipoNodo = 11; break;
          case 7: tipoNodo = 3; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 4)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 11; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 1; break;
          case 7: tipoNodo = 6; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 5)
      {
        switch (rand)
        {
          case 1: tipoNodo = 3; break;
          case 2: tipoNodo = 3; break;
          case 3: tipoNodo = 3; break;
          case 4: tipoNodo = 14; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 7; break;
          case 7: tipoNodo = 5; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 6)
      {
        switch (rand)
        {
          case 1: tipoNodo = 11; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 8; break;
          case 6: tipoNodo = 11; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 7)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 3; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 1; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 3; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 8)
      {
        switch (rand)
        {
          case 1: tipoNodo = 14; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 14; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 7; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 9)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 8; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 8; break;
          case 5: tipoNodo = 11; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 1; break;
        }
      }
      //1 Batalla - 2 Evento - 3 Claro - 4 Asentamiento (NO) - 5 Recurso
    // 6 Comercio - 7 Sequito -8 Elite -11 Emboscada - 14 Santuario
      if (posXNodo == 10)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 3; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 14; break;
          case 5: tipoNodo = 3; break;
          case 6: tipoNodo = 3; break;
          case 7: tipoNodo = 3; break;
        }
      }
      if (posXNodo == 11) { tipoNodo = 10; }
    }

    //Correctores por zona
    //Nedukazal no tiene Santuarios
    if (CampaignManager.Instance.scAtributosZona.ID == 3) //Nedukazal
    {
      if (tipoNodo == 14) tipoNodo = 1; //Santuario a Batalla normal
    }

    ActivarNodoVisual(tipoNodo, esAtajo, estabaRevelado);

    if (esAtajo)
    {
      ConfigurarResultadoAtajoSubterraneo();
    }
  }

  public void TiradaExploracion(int chances, bool continua, string actividadExploradorON = "", bool sinLog = false)
  {
    bool yaAvisoLog = sinLog;
    int cappedChance = Mathf.Clamp(chances, 0, 100);

    foreach (Nodo nodo in DestinosPosibles)
    {
      int tirada = UnityEngine.Random.Range(0, 100);
      if (tirada >= cappedChance) continue;

      nodo.Revelar(false);
      int logChance = Mathf.Min(cappedChance, 90);

      if ((continua || !string.IsNullOrEmpty(actividadExploradorON)) && logChance > 36)
      {
        if (!yaAvisoLog)
        {
          string textoTirada = TRADU.i.Traducir("Tirada: ");
          if (!string.IsNullOrEmpty(actividadExploradorON))
            CampaignManager.Instance.EscribirLog("<color=#7ED6F7>-" + actividadExploradorON + TRADU.i.Traducir(" ha Explorado con Éxito el camino adelante.</color>") + $"({textoTirada}{tirada} < {cappedChance})");
          else
            CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("<color=#7ED6F7>-Durante el Descanso, se ha Explorado con Éxito el camino adelante.</color>") + $"({textoTirada}{tirada} < {cappedChance})");

          yaAvisoLog = true;
        }

        if (continua)
        {
          int nextChance = Mathf.Clamp(logChance - 15, 0, 90);
          if (nextChance > 0) nodo.TiradaExploracion(nextChance, true, "", true);
        }
      }
    }
  }

  public void DesactivarGraficosNodo()
  {
    foreach (Transform child in transform)
    {
      if (!child.name.Contains("Nodo")) continue;
      int idx = child.GetSiblingIndex();
      if (idx == 14 && nodoIncendiado) continue; 
      if (idx == 15 && nodoRitual) continue;  

      child.gameObject.SetActive(false);
    }
  }

  bool ActivarVisualPorCodigo(int codigo)
  {

    int indice = -1;
    AplicarEstiloVisualSettlement(codigo == CodigoSettlement);
    switch (codigo)
    {
      case 1: indice = 1; break;  // 1: Combate directo (batalla normal)
      case 2: indice = 2; break;  // 2: Evento aleatorio
      case 3: indice = 3; break;  // 3: Claro tranquilo (posible descanso / efecto benigno)
      case 4: indice = 4; break;  // 4: Asentamiento
      case 5: indice = 5; break;  // 5: Recolección de recursos
      case 6: indice = 6; break;  // 6: Puesto de comercio
      case 7: indice = 7; break;  // 7: Adquisición de personajes (reclutamiento)
      case 8: indice = 8; break;  // 8: Combate contra enemigos de élite
      case 10: indice = 8; break; // 10: Batalla final de la zona (visual similar a élite)
      case 11: indice = 9; break; // 11: Zona expuesta (emboscada)
      case 12: indice = 10; break; // 12: Nodo misterioso / posible batalla subterránea
      case 13: indice = 11; break; // 13: Salida del atajo subterráneo
      case 14: indice = 12; break; //14: Santuario
      case 15: indice = 15; break; //15: Ritual Kale'Tav
      case 16: indice = 16; break; //16: Misión de Salvamento
    }


    if (indice < 0 || indice >= transform.childCount) return false;
    transform.GetChild(indice).gameObject.SetActive(true);
    return true;
  }

  void AplicarEstiloVisualSettlement(bool destacar)
  {
    if (IndiceVisualSettlement < 0 || IndiceVisualSettlement >= transform.childCount)
    {
      return;
    }

    Transform visualSettlement = transform.GetChild(IndiceVisualSettlement);
    if (visualSettlement == null)
    {
      return;
    }

    if (!escalaSettlementInicializada)
    {
      escalaSettlementOriginal = visualSettlement.localScale;
      escalaSettlementInicializada = true;
    }

    float multiplicador = destacar ? MultiplicadorEscalaSettlement : 1f;
    visualSettlement.localScale = escalaSettlementOriginal * multiplicador;
  }

  public void ActivarNodoVisual(int num, bool esAtajo, bool estabaRevelado)
  {
    DesactivarGraficosNodo();

    esMisterioso = false;

    bool esTutorial = CampaignManager.Instance != null &&
                      CampaignManager.Instance.scTutorialManager != null &&
                      CampaignManager.Instance.scTutorialManager.tutorialActivo;

    int chancesMisterioso = 15;
    if (CampaignManager.Instance.intTipoClima == 5) chancesMisterioso += 10; // Niebla
    if (CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) > 0)
      chancesMisterioso -= CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) * 5;

    if (esTutorial)
    {
      chancesMisterioso = 0;
      esAtajo = false;
      num = tipoNodo; // en tutorial no variamos el visual
    }

    if (posXNodo == 10 || posXNodo == 1) chancesMisterioso = 0;
    if (estabaRevelado) chancesMisterioso = 0;
    if (nodoRitual) chancesMisterioso = 0;
    if (nodoIncendiado) chancesMisterioso = 0;

    if (UnityEngine.Random.Range(0, 100) < chancesMisterioso && tipoNodo != 16)
    {
      num = 12; // misterioso
      esMisterioso = true;
    }
    if (esAtajo) num = 13; // salida atajo

    numVisualActual = num;
    if (!ActivarVisualPorCodigo(numVisualActual))
      numVisualActual = -1;

    if (CampaignManager.Instance.scMapaManager.nodoActual != null && transform.childCount > 13)
    {
      int nodoenXactual = CampaignManager.Instance.scMapaManager.nodoActual.posXNodo;
      if (nodoenXactual >= posXNodo) { return; } //No activa VFx de revelado en nodos de la misma altura en X

      if (!CampaignManager.Instance.scMapaManager.nodoActual.DestinosPosibles.Contains(this) || esAtajo)
      {
        if (tipoNodo != 16) //no vfx en misión de salvamento
        { transform.GetChild(13).gameObject.SetActive(true); }
      } // vfx de revelado (no inmediatos)
    }

  }

  public void ActivarVfxDescubrimiento()
  {
    if (!gameObject.activeInHierarchy || tipoNodo == 16 || transform.childCount <= 13)
    {
      return;
    }

    transform.GetChild(13).gameObject.SetActive(true);
  }

  public string descripcion;

  void OnEnable()
  {
    // Sincronizar VFX persistentes con estado lógico al reactivar (antes de cualquier early-return)
    //SincronizarVFXPersistentes();
    SincronizarVFXPersistentes();
    Invoke("SincronizarVFXPersistentes", 0.1f);

    if (tipoNodo == 15 && nodoRitual && numVisualActual == 16)
    {
      numVisualActual = 15;
    }

    int codigoAAplicar = numVisualActual > 0 ? numVisualActual : tipoNodo;
    if (codigoAAplicar <= 0)
    {
      ActivarVisualBaseNoRevelado();
      return;
    }
    DesactivarGraficosNodo();

    bool visualActivado = ActivarVisualPorCodigo(codigoAAplicar);
    if (visualActivado)
    {
      numVisualActual = codigoAAplicar;
    }
    else if (codigoAAplicar != tipoNodo && tipoNodo > 0)
    {
      visualActivado = ActivarVisualPorCodigo(tipoNodo);
      if (visualActivado)
        numVisualActual = tipoNodo;
    }

    if (!visualActivado)
    {
      ActivarVisualBaseNoRevelado();
      return;
    }

    if (CampaignManager.Instance != null &&
        CampaignManager.Instance.scMapaManager != null &&
        CampaignManager.Instance.scMapaManager.nodoActual != null &&
        transform.childCount > 13)
    {
      bool esAtajoActivo = numVisualActual == 13;
      int nodoenXactual = CampaignManager.Instance.scMapaManager.nodoActual.posXNodo;
      if (nodoenXactual == posXNodo) { return; } //No activa VFx de revelado en nodos de la misma altura en X
      if (!CampaignManager.Instance.scMapaManager.nodoActual.DestinosPosibles.Contains(this) || esAtajoActivo)
        transform.GetChild(13).gameObject.SetActive(true);
    }
  }

  void OnMouseEnter()
  {
    if (EventSystem.current.IsPointerOverGameObject()) return;

    switch (tipoNodo)
    {
      case 1: descripcion = TRADU.i.Traducir("Combate directo."); break;
      case 2: descripcion = TRADU.i.Traducir("Evento aleatorio."); break;
      case 3: descripcion = TRADU.i.Traducir("Claro tranquilo."); break;
      case 4: descripcion = TRADU.i.Traducir("Asentamiento."); break;
      case 5: descripcion = TRADU.i.Traducir("Recolección de Recursos."); break;
      case 6: descripcion = TRADU.i.Traducir("Puesto de Comercio."); break;
      case 7: descripcion = TRADU.i.Traducir("Adquisición de Personajes."); break;
      case 8: descripcion = TRADU.i.Traducir("Combate directo contra enemigos de élite."); break;
      case 10: descripcion = TRADU.i.Traducir("Batalla final de la Zona actual."); break;
      case 11: descripcion = TRADU.i.Traducir("<b>(!)</b> Zona Expuesta, la caravana será emboscada."); break;
      case 15: descripcion = TRADU.i.Traducir("Batalla Kale'Tav"); break;
      case 16: descripcion = TRADU.i.Traducir("Ubicación de la Misión de Salvamento"); break;

      default: descripcion = TRADU.i.Traducir("Nodo Desconocido."); break;
    }
    if (esMisterioso) descripcion = TRADU.i.Traducir("Nodo Misterioso, no se ha logrado revelar.");
    if (transform.GetChild(11).gameObject.activeInHierarchy) descripcion = TRADU.i.Traducir("Salida del atajo subterraneo, no sabemos que hay del otro lado.");
    if (transform.GetChild(12).gameObject.activeInHierarchy) descripcion = TRADU.i.Traducir("Santuario de Purificadores.");

    Vector3 pos = Input.mousePosition;
    TooltipNodos.Instance.ShowTooltip(descripcion, pos, this);
  }

  void OnMouseExit()
  {
    TooltipNodos.Instance.HideTooltip();
  }


  public void ActivarIncendio()
  {
    if (!PuedeTenerIncendioPersistente())
    {
      nodoIncendiado = false;
      if (transform.childCount > 14)
        transform.GetChild(14).gameObject.SetActive(false);
      return;
    }

    print("ActivarIncendio called");
    nodoIncendiado = true;
    if (transform.childCount > 14)
      transform.GetChild(14).gameObject.SetActive(true);

  }

  public void DesactivarIncendio()
  {
    print("DesactivarIncendio called");
    nodoIncendiado = false;
    if (transform.childCount > 14)
      transform.GetChild(14).gameObject.SetActive(false);
  }

  public void ActivarRitual()
  {
    if (!PuedeTenerRitualPersistente())
    {
      nodoRitual = false;
      if (transform.childCount > 15)
        transform.GetChild(15).gameObject.SetActive(false);
      return;
    }

    print("ActivarRitual called");
    nodoRitual = true;
    if (transform.childCount > 15)
      transform.GetChild(15).gameObject.SetActive(true);
  }

  public void DesactivarRitual()
  {
    print("DesactivarRitual called");
    nodoRitual = false;
    if (transform.childCount > 15)
      transform.GetChild(15).gameObject.SetActive(false);
  }

  public void SincronizarVFXPersistentes()
  {
    // En tutorial nunca deberáan quedar incendios/rituales activos
    LimpiarEstadosPersistentesNoValidos();

    // Fallback por índice (prefab original)
    if (transform.childCount > 14)
      transform.GetChild(14).gameObject.SetActive(nodoIncendiado);
    if (transform.childCount > 15)
      transform.GetChild(15).gameObject.SetActive(nodoRitual);

    // Refuerzo por nombre (por si cambia el orden de hijos)
    for (int i = 0; i < transform.childCount; i++)
    {
      var child = transform.GetChild(i);
      string name = child.name.ToLowerInvariant();
      if (name.Contains("nodoincendiado"))
      {
        child.gameObject.SetActive(nodoIncendiado);
      }
      else if (name.Contains("nodoritual"))
      {
        child.gameObject.SetActive(nodoRitual);
      }
    }
  }

  void ActivarVisualBaseNoRevelado()
  {
    AplicarEstiloVisualSettlement(false);

    if (transform.childCount == 0)
    {
      return;
    }

    Transform visualBase = transform.GetChild(0);
    if (visualBase == null)
    {
      return;
    }

    visualBase.gameObject.SetActive(true);

    if (visualBase.childCount == 0)
    {
      return;
    }

    bool mostrarSubVisual = CampaignManager.Instance == null
      || CampaignManager.Instance.scAtributosZona == null
      || CampaignManager.Instance.scAtributosZona.ID != 3;

    visualBase.GetChild(0).gameObject.SetActive(mostrarSubVisual);
  }

  bool EstaPermitidoEnZona(Nodo nodo, int zonaId)
  {
    if (nodo == null) return false;
    if (zonaId <= 0) return true;
    return nodo.ProhibidoEnZona == null || !nodo.ProhibidoEnZona.Contains(zonaId);
  }

  Nodo ObtenerNodoPermitido(int x, int y, int zonaId)
  {
    if (scContenedorNodos2 == null) return null;
    Nodo destino = scContenedorNodos2.ObtenerNodoSegunXY(x, y);
    if (!EstaPermitidoEnZona(destino, zonaId)) return null;
    return destino;
  }

  bool IntentarConectar(int x, int y, int zonaId, bool esPorAbajo = false)
  {
    Nodo destino = ObtenerNodoPermitido(x, y, zonaId);
    if (destino == null) return false;
    ConectarConNodo(destino, esPorAbajo);
    return true;
  }

  void ConectarFallbackSiguienteColumna(int nextX, int zonaId)
  {
    if (scContenedorNodos2 == null) return;

    Nodo mejor = null;
    int mejorDistY = int.MaxValue;

    foreach (Nodo candidato in scContenedorNodos2.listTodosNodos)
    {
      if (candidato == null) continue;
      if (candidato.posXNodo != nextX) continue;
      if (!EstaPermitidoEnZona(candidato, zonaId)) continue;

      int distY = Mathf.Abs(candidato.posYNodo - posYNodo);
      if (distY < mejorDistY)
      {
        mejorDistY = distY;
        mejor = candidato;
        if (mejorDistY == 0) break;
      }
    }

    if (mejor != null)
      ConectarConNodo(mejor);
  }

  public void EsconderSiNedukazal()
  {
    if (CampaignManager.Instance.scAtributosZona.ID == 3)
    {
      transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }
    else
    { transform.GetChild(0).GetChild(0).gameObject.SetActive(true); }
  }
  
  [Header("Convoy")]
public float rotSpeed = 10f;            // suavizado de rotación
public float lookAheadDist = 0.5f;    // "mira" hacia adelante para orientar
float gapDist = 0.66f;              // distancia entre vehículos
private IEnumerator MoverConvoyEnLinea(LineRenderer lr, bool viajeSubterraneo = false)
{
    // Ajustes de suavizado (si querés tunear, subilos a fields públicos)
    const float tramoSuavizadoExtremos = 0.18f;
    float easeInTime = 0.28f;       // segundos para acelerar al inicio
    float easeOutDist = 0.6f;       // metros antes del final para frenar líder
    float easeOutTailDist = 0.08f;   // "error" de cola para frenar al final
    float minSpeedFactor = 0.10f;   // piso para que no se quede "cerquita"
    float snapEps = 0.03f;          // snap final del líder (en metros aprox)

    if (lr == null) yield break;

    // Convoy ordenado
    var convoy = new List<(GameObject go, Transform rot)>();
    void AddIf(GameObject go, bool esLider)
    {
        if (go == null) return;
        int idx = esLider ? 4 : 0; // tu setup
        Transform rotT = (go.transform.childCount > idx) ? go.transform.GetChild(idx) : null;
        convoy.Add((go, rotT));
    }

    AddIf(scMapaManager.goCaravana, true);
    AddIf(scMapaManager.goCaravanafollower1, false);
    AddIf(scMapaManager.goCaravanafollower2, false);
    AddIf(scMapaManager.goCaravanafollower3, false);
    AddIf(scMapaManager.goCaravanafollower4, false);
    AddIf(scMapaManager.goCaravanafollower5, false);
    AddIf(scMapaManager.goCaravanafollower6, false);

    scMapaManager.goCaravanafollower1.transform.GetChild(0).GetComponent<Animator>().SetBool("IsWalking", true);
    scMapaManager.goCaravanafollower2.transform.GetChild(0).GetComponent<Animator>().SetBool("IsWalking", true);
    scMapaManager.goCaravanafollower3.transform.GetChild(0).GetComponent<Animator>().SetBool("IsWalking", true);
    scMapaManager.goCaravanafollower4.transform.GetChild(0).GetComponent<Animator>().SetBool("IsWalking", true);
    scMapaManager.goCaravanafollower5.transform.GetChild(0).GetComponent<Animator>().SetBool("IsWalking", true);
    scMapaManager.goCaravanafollower6.transform.GetChild(0).GetComponent<Animator>().SetBool("IsWalking", true);

    int m = convoy.Count;
    if (m == 0) yield break;

    var undergroundRenderers = new List<Renderer>();
    var undergroundRendererInitialStates = new List<bool>();
    bool convoyOculto = false;
    GameObject markerGO = null;
    Vector3 markerBaseScale = new Vector3(0.24f, 0.24f, 0.24f);
    const float markerYOffset = 0.2f;
    const float markerPulseSpeed = 8f;
    const float markerPulseAmp = 0.1f;
    const float tramoEntradaSalidaVisible = 0.18f;
    const float profundidadSubterraneaY = 1.2f;
    const float pitchSubterraneoMax = 7f;
    const float overlayAlphaMax = 0.322f; // +15% más oscuro
    GameObject overlayGO = null;
    Image overlayImage = null;
    UndergroundAudioFxState undergroundAudioFx = null;

    // Puntos del camino en WORLD
    int n = lr.positionCount;
    if (n < 2) yield break;

    Vector3[] pts = new Vector3[n];
    for (int i = 0; i < n; i++)
    {
        Vector3 p = lr.GetPosition(i);
        pts[i] = lr.useWorldSpace ? p : lr.transform.TransformPoint(p);
    }

    AjustarExtremosTrayectoConvoy(pts, convoy[0].go.transform.position, transform.position, tramoSuavizadoExtremos);

    // Longitudes acumuladas del tramo
    float[] segLen = new float[n - 1];
    float[] cumLen = new float[n];
    cumLen[0] = 0f;
    for (int i = 0; i < n - 1; i++)
    {
        float L = Vector3.Distance(pts[i], pts[i + 1]);
        segLen[i] = L;
        cumLen[i + 1] = cumLen[i] + L;
    }

    float totalLen = cumLen[n - 1];
    if (totalLen <= 0.0001f) yield break;
    float duracionDesvanecidoSonido = Mathf.Lerp(0.08f, 0.2f, Mathf.InverseLerp(2.5f, 9f, totalLen));

    // Gap desde el Inspector (no lo pises con un local)
    float gap = Mathf.Max(0.0001f, this.gapDist);
    easeInTime = Mathf.Lerp(0.24f, 0.4f, Mathf.InverseLerp(2.5f, 9f, totalLen));
    easeOutDist = Mathf.Clamp(totalLen * 0.07f, 0.2f, 0.45f);
    easeOutTailDist = Mathf.Clamp(gap * 0.28f, 0.04f, 0.12f);
    minSpeedFactor = 0.18f;
    snapEps = 0.05f;

    if (viajeSubterraneo)
    {
      for (int i = 0; i < convoy.Count; i++)
      {
        RegistrarRenderersSubterraneos(convoy[i].rot, undergroundRenderers, undergroundRendererInitialStates);
      }
      markerGO = GetOrCreateUndergroundTravelMarker();
      overlayGO = CrearOverlayViajeSubterraneo(out overlayImage);
      undergroundAudioFx = PrepararAudioSubterraneo();
      ActualizarAudioSubterraneo(undergroundAudioFx, 0f);
    }


    // Trail del líder: seed con estado actual (cola -> ... -> líder) para cero teleports
    var trailPos = new List<Vector3>(256);
    var trailRot = new List<Quaternion>(256);
    var trailS   = new List<float>(256);

    for (int i = m - 1; i >= 0; i--)
    {
        Vector3 p = convoy[i].go.transform.position;
        Quaternion r = (convoy[i].rot != null) ? convoy[i].rot.rotation : Quaternion.identity;

        if (trailPos.Count == 0)
        {
            trailPos.Add(p);
            trailRot.Add(r);
            trailS.Add(0f);
        }
        else
        {
            float add = Vector3.Distance(trailPos[trailPos.Count - 1], p);
            trailPos.Add(p);
            trailRot.Add(r);
            trailS.Add(trailS[trailS.Count - 1] + add);
        }
    }

    // "odómetro" de cada follower sobre el TRAIL (esto es lo que permite que sigan moviéndose al final)
    float[] followerS = new float[m];
    for (int i = 0; i < m; i++)
        followerS[i] = ProjectDistanceOnTrail(trailPos, trailS, convoy[i].go.transform.position);

    // Arrancamos líder en su proyección al tramo nuevo
    float leaderS = ProjectDistanceOnPolyline(pts, cumLen, convoy[0].go.transform.position);

    const float minSampleDist = 0.02f;
    float maxTrailBack = gap * (m - 1) + 2.0f;

    float elapsed = 0f;
    bool leaderArrived = false;
    bool sonidoMovimientoDesvanecido = false;
    float speedFactorSmoothed = 0f;
    const float speedSmooth = 10f; // subí a 12-16 si aún hay tirán
    
    try
    {
      while (true)
      {
      float dt = Time.deltaTime;
      elapsed += dt;
      bool leaderArrivedPrevio = leaderArrived;

      // Factor de aceleración inicial
      float easeIn = (easeInTime <= 0.0001f) ? 1f : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / easeInTime));

      // Factor de frenado (líder) o drenado (cola)
      float easeOutFactor;

      if (!leaderArrived)
      {
        float remaining = totalLen - leaderS;

        // Snap para que no quede "cerquita"
        if (remaining <= snapEps)
        {
          leaderS = totalLen;
          leaderArrived = true;
          remaining = 0f;
        }

        // 1 lejos, 0 cerca del final
        easeOutFactor = (easeOutDist <= 0.0001f)
            ? 1f
            : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(remaining / easeOutDist));
      }
      else
      {
        // Drenado: si la cola todavía tiene error grande, seguimos a buena velocidad;
        // si el error es chico, frenamos suave.
        float headTrailS2 = trailS[trailS.Count - 1];
        float maxErr = 0f;

        for (int i = 1; i < m; i++)
        {
          float targetS = headTrailS2 - gap * i;
          if (targetS < trailS[0]) targetS = trailS[0];
          float err = Mathf.Abs(followerS[i] - targetS);
          if (err > maxErr) maxErr = err;
        }

        easeOutFactor = (easeOutTailDist <= 0.0001f)
            ? 1f
            : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(maxErr / easeOutTailDist));
      }

      float speedFactorTarget = Mathf.Max(minSpeedFactor, easeIn * easeOutFactor);
      float speedFloor = Mathf.Lerp(0f, minSpeedFactor, easeIn);
      speedFactorTarget = Mathf.Max(speedFloor, easeIn * easeOutFactor);

// suavizado exponencial independiente del FPS
float kSpeed = 1f - Mathf.Exp(-speedSmooth * dt);
speedFactorSmoothed = Mathf.Lerp(speedFactorSmoothed, speedFactorTarget, kSpeed);

float step = Mathf.Max(0f, velocidadMovimiento) * dt * speedFactorSmoothed;


      // Mover líder hasta el final (luego se queda)
      if (!leaderArrived)
      {
        leaderS = Mathf.MoveTowards(leaderS, totalLen, step);
        if (totalLen - leaderS <= snapEps)
        {
          leaderS = totalLen;
          leaderArrived = true;
        }
      }

      if (leaderArrived && !leaderArrivedPrevio && !sonidoMovimientoDesvanecido && CampaignManager.Instance != null)
      {
        CampaignManager.Instance.DesvanecerSonidoMovimientoCaravana(duracionDesvanecidoSonido);
        sonidoMovimientoDesvanecido = true;
      }

      Vector3 leaderPosCamino = PointAtDistance(pts, segLen, cumLen, leaderS);
      float offsetYSubterraneo = 0f;

      if (viajeSubterraneo)
      {
        float progreso = totalLen <= 0.0001f ? 1f : Mathf.Clamp01(leaderS / totalLen);
        bool ocultarAhora = progreso > tramoEntradaSalidaVisible && progreso < (1f - tramoEntradaSalidaVisible);

        if (ocultarAhora != convoyOculto)
        {
          SetRenderersVisible(undergroundRenderers, !ocultarAhora);
          convoyOculto = ocultarAhora;
        }

        if (markerGO != null)
        {
          markerGO.SetActive(ocultarAhora);
          if (ocultarAhora)
          {
            float pulse = 1f + markerPulseAmp * Mathf.Sin(elapsed * markerPulseSpeed);
            markerGO.transform.localScale = markerBaseScale * pulse;
            markerGO.transform.position = leaderPosCamino + Vector3.up * markerYOffset;
          }
        }

        float intensidadSubterranea = CalcularIntensidadSubterranea(progreso, tramoEntradaSalidaVisible);
        AplicarTinteSubterraneo(overlayImage, intensidadSubterranea, overlayAlphaMax);
        ActualizarAudioSubterraneo(undergroundAudioFx, intensidadSubterranea);
        offsetYSubterraneo = CalcularOffsetYSubterraneo(progreso, tramoEntradaSalidaVisible, profundidadSubterraneaY);
      }

      Vector3 leaderPos = leaderPosCamino;
      leaderPos.y += offsetYSubterraneo;
      convoy[0].go.transform.position = leaderPos;

      // Rotación del líder por tangente
      Quaternion leaderRot = Quaternion.identity;
      if (convoy[0].rot != null)
      {
        float sAtras = Mathf.Max(leaderS - lookAheadDist, 0f);
        float sF = Mathf.Min(leaderS + lookAheadDist, totalLen);
        Vector3 posAtras = PointAtDistance(pts, segLen, cumLen, sAtras);
        Vector3 posF = PointAtDistance(pts, segLen, cumLen, sF);
        float pitchSubterraneo = 0f;

        if (viajeSubterraneo)
        {
          float progresoActual = totalLen <= 0.0001f ? 1f : Mathf.Clamp01(leaderS / totalLen);
          float progresoAtras = totalLen <= 0.0001f ? 0f : Mathf.Clamp01(sAtras / totalLen);
          float progresoFuturo = totalLen <= 0.0001f ? 1f : Mathf.Clamp01(sF / totalLen);

          posAtras.y += CalcularOffsetYSubterraneo(progresoAtras, tramoEntradaSalidaVisible, profundidadSubterraneaY);
          posF.y += CalcularOffsetYSubterraneo(progresoFuturo, tramoEntradaSalidaVisible, profundidadSubterraneaY);
          pitchSubterraneo = CalcularPitchSubterraneo(progresoActual, tramoEntradaSalidaVisible, pitchSubterraneoMax);
        }

        Vector3 origenRotacion = leaderPos;
        Vector3 destinoRotacion = posF;

        if ((destinoRotacion - origenRotacion).sqrMagnitude <= 0.000001f)
        {
          origenRotacion = posAtras;
          destinoRotacion = leaderPos;
        }

        Quaternion target = CalcularRotacionConvoyPorRelieve(origenRotacion, destinoRotacion, viajeSubterraneo);
        if (viajeSubterraneo && target != Quaternion.identity)
        {
          target *= Quaternion.Euler(pitchSubterraneo, 0f, 0f);
        }

        if (target != Quaternion.identity)
        {
          float k = 1f - Mathf.Exp(-rotSpeed * dt);
          convoy[0].rot.rotation = Quaternion.Slerp(convoy[0].rot.rotation, target, k);
        }
        leaderRot = convoy[0].rot.rotation;
      }

      // Guardar sample del líder en el trail (siempre consistente)
Vector3 lastPos = trailPos[trailPos.Count - 1];
float moved = Vector3.Distance(lastPos, leaderPos);

// si el líder realmente se movió, agregamos un sample nuevo
if (moved > 0.00001f)
{
    trailPos.Add(leaderPos);
    trailRot.Add(leaderRot);
    trailS.Add(trailS[trailS.Count - 1] + moved);
}
else
{
    // si no se movió, actualizamos rot/pos por prolijidad
    trailPos[trailPos.Count - 1] = leaderPos;
    trailRot[trailRot.Count - 1] = leaderRot;
}


      float headTrailS = trailS[trailS.Count - 1];

      // Followers: avanzan por el trail con su propio odómetro (esto permite "terminar" después)
      bool allFollowersAtTarget = true;

      for (int i = 1; i < m; i++)
      {
        float targetS = headTrailS - gap * i;
        if (targetS < trailS[0]) targetS = trailS[0];

        followerS[i] = Mathf.MoveTowards(followerS[i], targetS, step);

        SampleTrail(trailPos, trailRot, trailS, followerS[i], out var p, out var r);
        p.y += offsetYSubterraneo;
        convoy[i].go.transform.position = p;

        if (convoy[i].rot != null)
        {
          float k = 1f - Mathf.Exp(-rotSpeed * dt);
          convoy[i].rot.rotation = Quaternion.Slerp(convoy[i].rot.rotation, r, k);
        }

        if (Mathf.Abs(followerS[i] - targetS) > 0.01f)
          allFollowersAtTarget = false;
      }

      // Recorte del trail
      while (trailS.Count > 2 && (headTrailS - trailS[0]) > maxTrailBack)
      {
        trailS.RemoveAt(0);
        trailPos.RemoveAt(0);
        trailRot.RemoveAt(0);

        // Clamp por si justo recortaste "debajo" de algún follower
        for (int i = 1; i < m; i++)
          if (followerS[i] < trailS[0]) followerS[i] = trailS[0];
      }

      // Salida: líder llegó y cola drenó
      if (leaderArrived && allFollowersAtTarget)
        break;

      yield return null;
      }

      // Snap final líder al último punto exacto del LR
      convoy[0].go.transform.position = pts[n - 1];
    }
    finally
    {
      if (viajeSubterraneo)
      {
        RestaurarRenderers(undergroundRenderers, undergroundRendererInitialStates);
        if (markerGO != null) markerGO.SetActive(false);
        AplicarTinteSubterraneo(overlayImage, 0f, overlayAlphaMax);
        RestaurarAudioSubterraneo(undergroundAudioFx);
        if (overlayGO != null) Destroy(overlayGO);
      }
    }

    LlegoCaravana();
}

// Helper: proyecta posición al TRAIL (polilínea) y devuelve "s"
private static void AjustarExtremosTrayectoConvoy(Vector3[] pts, Vector3 origenReal, Vector3 destinoReal, float tramoSuavizado)
{
    if (pts == null || pts.Length == 0)
    {
        return;
    }

    if (pts.Length == 1)
    {
        pts[0] = destinoReal;
        return;
    }

    Vector3 offsetInicio = origenReal - pts[0];
    Vector3 offsetFin = destinoReal - pts[pts.Length - 1];

    if (offsetInicio.sqrMagnitude <= 0.000001f && offsetFin.sqrMagnitude <= 0.000001f)
    {
        return;
    }

    float blend = Mathf.Clamp01(tramoSuavizado);
    float blendSeguro = Mathf.Max(0.0001f, blend);
    int ultimo = pts.Length - 1;

    for (int i = 0; i <= ultimo; i++)
    {
        float t = i / (float)ultimo;
        float pesoInicio = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / blendSeguro));
        float pesoFin = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - t) / blendSeguro));
        pts[i] += offsetInicio * pesoInicio + offsetFin * pesoFin;
    }

    pts[0] = origenReal;
    pts[ultimo] = destinoReal;
}

private static float ProjectDistanceOnTrail(List<Vector3> trailPos, List<float> trailS, Vector3 worldPos)
{
    Vector3 p = worldPos; p.y = 0f;
    float bestS = trailS[0];
    float bestD2 = float.PositiveInfinity;

    for (int i = 0; i < trailPos.Count - 1; i++)
    {
        Vector3 a = trailPos[i]; a.y = 0f;
        Vector3 b = trailPos[i + 1]; b.y = 0f;

        Vector3 ab = b - a;
        float ab2 = Vector3.Dot(ab, ab);
        if (ab2 <= 0.000001f) continue;

        float t = Vector3.Dot(p - a, ab) / ab2;
        t = Mathf.Clamp01(t);

        Vector3 proj = a + ab * t;
        float d2 = (p - proj).sqrMagnitude;

        if (d2 < bestD2)
        {
            bestD2 = d2;
            float segL = Mathf.Sqrt(ab2);
            bestS = trailS[i] + segL * t;
        }
    }

    return bestS;
}


// Helpers (los mismos de antes)
private static float ProjectDistanceOnPolyline(Vector3[] pts, float[] cumLen, Vector3 worldPos)
{
    Vector3 p = worldPos; p.y = 0f;

    float bestS = 0f;
    float bestD2 = float.PositiveInfinity;

    for (int i = 0; i < pts.Length - 1; i++)
    {
        Vector3 a = pts[i]; a.y = 0f;
        Vector3 b = pts[i + 1]; b.y = 0f;

        Vector3 ab = b - a;
        float ab2 = Vector3.Dot(ab, ab);
        if (ab2 <= 0.000001f) continue;

        float t = Vector3.Dot(p - a, ab) / ab2;
        t = Mathf.Clamp01(t);

        Vector3 proj = a + ab * t;
        float d2 = (p - proj).sqrMagnitude;

        if (d2 < bestD2)
        {
            bestD2 = d2;
            float segL = Mathf.Sqrt(ab2);
            bestS = cumLen[i] + segL * t;
        }
    }

    return bestS;
}

private static Vector3 PointAtDistance(Vector3[] pts, float[] segLen, float[] cumLen, float s)
{
    if (s <= 0f) return pts[0];
    float total = cumLen[cumLen.Length - 1];
    if (s >= total) return pts[pts.Length - 1];

    for (int i = 0; i < segLen.Length; i++)
    {
        float a = cumLen[i];
        float b = cumLen[i + 1];
        if (s > b) continue;

        float L = segLen[i];
        if (L <= 0.000001f) return pts[i];

        float t = (s - a) / L;
        return Vector3.Lerp(pts[i], pts[i + 1], t);
    }

    return pts[pts.Length - 1];
}

private static void SampleTrail(
    List<Vector3> pos,
    List<Quaternion> rot,
    List<float> s,
    float targetS,
    out Vector3 outPos,
    out Quaternion outRot)
{
    int last = s.Count - 1;

    if (targetS <= s[0]) { outPos = pos[0]; outRot = rot[0]; return; }
    if (targetS >= s[last]) { outPos = pos[last]; outRot = rot[last]; return; }

    int j = 1;
    while (j < s.Count && s[j] < targetS) j++;

    int a = j - 1;
    int b = j;

    float t = Mathf.InverseLerp(s[a], s[b], targetS);
    outPos = Vector3.Lerp(pos[a], pos[b], t);
    outRot = Quaternion.Slerp(rot[a], rot[b], t);
}

private static void RegistrarRenderersSubterraneos(Transform root, List<Renderer> renderers, List<bool> enabledInicial)
{
  if (root == null) return;

  Renderer[] encontrados = root.GetComponentsInChildren<Renderer>(true);
  for (int i = 0; i < encontrados.Length; i++)
  {
    Renderer r = encontrados[i];
    if (r == null) continue;

    renderers.Add(r);
    enabledInicial.Add(r.enabled);
  }
}

private static void SetRenderersVisible(List<Renderer> renderers, bool visible)
{
  if (renderers == null) return;

  for (int i = 0; i < renderers.Count; i++)
  {
    Renderer r = renderers[i];
    if (r == null) continue;
    r.enabled = visible;
  }
}

private static void RestaurarRenderers(List<Renderer> renderers, List<bool> enabledInicial)
{
  if (renderers == null || enabledInicial == null) return;
  int n = Mathf.Min(renderers.Count, enabledInicial.Count);

  for (int i = 0; i < n; i++)
  {
    Renderer r = renderers[i];
    if (r == null) continue;
    r.enabled = enabledInicial[i];
  }
}

private static float CalcularOffsetYSubterraneo(float progreso, float tramoEntradaSalida, float profundidadY)
{
  if (tramoEntradaSalida <= 0.0001f) return -Mathf.Abs(profundidadY);

  float depth = Mathf.Abs(profundidadY);
  float p = Mathf.Clamp01(progreso);

  if (p <= tramoEntradaSalida)
  {
    float t = Mathf.Clamp01(p / tramoEntradaSalida);
    return -Mathf.SmoothStep(0f, depth, t);
  }

  if (p >= 1f - tramoEntradaSalida)
  {
    float t = Mathf.Clamp01((p - (1f - tramoEntradaSalida)) / tramoEntradaSalida);
    return -Mathf.SmoothStep(depth, 0f, t);
  }

  return -depth;
}

private static float CalcularPitchSubterraneo(float progreso, float tramoEntradaSalida, float pitchMax)
{
  if (tramoEntradaSalida <= 0.0001f || Mathf.Abs(pitchMax) <= 0.0001f) return 0f;

  float maxPitch = Mathf.Abs(pitchMax);
  float p = Mathf.Clamp01(progreso);

  if (p <= tramoEntradaSalida)
  {
    float t = Mathf.Clamp01(p / tramoEntradaSalida);
    return Mathf.Lerp(0f, maxPitch, t);
  }

  if (p >= 1f - tramoEntradaSalida)
  {
    float t = Mathf.Clamp01((p - (1f - tramoEntradaSalida)) / tramoEntradaSalida);
    return Mathf.Lerp(-maxPitch, 0f, t);
  }

  return 0f;
}

public int ObtenerVisualCodeActual()
{
  return numVisualActual;
}

public bool ObtenerEstadoMisterioso()
{
  return esMisterioso;
}

public bool ObtenerAtajoSubterraneoPendiente()
{
  return atajoSubterraneoPendiente;
}

private static float CalcularIntensidadSubterranea(float progreso, float tramoEntradaSalida)
{
  if (tramoEntradaSalida <= 0.0001f) return 1f;

  float p = Mathf.Clamp01(progreso);
  if (p <= tramoEntradaSalida)
  {
    float t = Mathf.Clamp01(p / tramoEntradaSalida);
    return Mathf.SmoothStep(0f, 1f, t);
  }

  if (p >= 1f - tramoEntradaSalida)
  {
    float t = Mathf.Clamp01((p - (1f - tramoEntradaSalida)) / tramoEntradaSalida);
    return Mathf.SmoothStep(1f, 0f, t);
  }

  return 1f;
}

private static GameObject CrearOverlayViajeSubterraneo(out Image overlayImage)
{
  overlayImage = null;

  GameObject canvasGO = new GameObject("UndergroundTravelOverlay", typeof(Canvas));
  Canvas canvas = canvasGO.GetComponent<Canvas>();
  canvas.renderMode = RenderMode.ScreenSpaceOverlay;
  canvas.sortingOrder = 8000;

  GameObject tintGO = new GameObject("Tint", typeof(RectTransform), typeof(Image));
  tintGO.transform.SetParent(canvasGO.transform, false);

  RectTransform rt = tintGO.GetComponent<RectTransform>();
  rt.anchorMin = Vector2.zero;
  rt.anchorMax = Vector2.one;
  rt.offsetMin = Vector2.zero;
  rt.offsetMax = Vector2.zero;

  overlayImage = tintGO.GetComponent<Image>();
  overlayImage.raycastTarget = false;
  overlayImage.color = new Color(0.2f, 0.12f, 0.06f, 0f);

  return canvasGO;
}

private static void AplicarTinteSubterraneo(Image overlayImage, float intensidad, float alphaMax)
{
  if (overlayImage == null) return;

  Color c = overlayImage.color;
  c.a = Mathf.Clamp01(intensidad) * Mathf.Clamp01(alphaMax);
  overlayImage.color = c;
}

private static AudioListener ObtenerAudioListenerActivo()
{
  if (Camera.main != null)
  {
    AudioListener mainListener = Camera.main.GetComponent<AudioListener>();
    if (mainListener != null) return mainListener;
  }

  return Object.FindObjectOfType<AudioListener>();
}

private static UndergroundAudioFxState PrepararAudioSubterraneo()
{
  AudioListener listener = ObtenerAudioListenerActivo();
  if (listener == null) return null;

  GameObject go = listener.gameObject;
  var estado = new UndergroundAudioFxState();

  estado.reverb = go.GetComponent<AudioReverbFilter>();
  if (estado.reverb == null)
  {
    estado.reverb = go.AddComponent<AudioReverbFilter>();
    estado.createdReverb = true;
  }
  estado.reverbWasEnabled = estado.reverb.enabled;
  estado.reverbPresetBefore = estado.reverb.reverbPreset;
  estado.reverb.enabled = true;

  estado.lowPass = go.GetComponent<AudioLowPassFilter>();
  if (estado.lowPass == null)
  {
    estado.lowPass = go.AddComponent<AudioLowPassFilter>();
    estado.createdLowPass = true;
  }
  estado.lowPassWasEnabled = estado.lowPass.enabled;
  estado.lowPassCutoffBefore = estado.lowPass.cutoffFrequency;
  estado.lowPassResonanceBefore = estado.lowPass.lowpassResonanceQ;
  estado.lowPass.enabled = true;

  estado.echo = go.GetComponent<AudioEchoFilter>();
  if (estado.echo == null)
  {
    estado.echo = go.AddComponent<AudioEchoFilter>();
    estado.createdEcho = true;
  }
  estado.echoWasEnabled = estado.echo.enabled;
  estado.echoWetBefore = estado.echo.wetMix;
  estado.echoDryBefore = estado.echo.dryMix;
  estado.echoDelayBefore = estado.echo.delay;
  estado.echoDecayBefore = estado.echo.decayRatio;
  estado.echo.enabled = true;

  return estado;
}

private static void ActualizarAudioSubterraneo(UndergroundAudioFxState estado, float intensidad)
{
  if (estado == null) return;

  float blend = Mathf.Clamp01(intensidad);

  if (estado.reverb != null)
  {
    estado.reverb.enabled = blend > 0.001f;
    estado.reverb.reverbPreset = AudioReverbPreset.Cave;
  }

  if (estado.lowPass != null)
  {
    estado.lowPass.enabled = blend > 0.001f;
    estado.lowPass.cutoffFrequency = Mathf.Lerp(22000f, 1200f, blend);
    estado.lowPass.lowpassResonanceQ = Mathf.Lerp(1f, 1.15f, blend);
  }

  if (estado.echo != null)
  {
    estado.echo.enabled = blend > 0.001f;
    estado.echo.wetMix = Mathf.Lerp(0f, 0.25f, blend);
    estado.echo.dryMix = 1f;
    estado.echo.delay = Mathf.Lerp(30f, 140f, blend);
    estado.echo.decayRatio = Mathf.Lerp(0f, 0.18f, blend);
  }
}

private static void RestaurarAudioSubterraneo(UndergroundAudioFxState estado)
{
  if (estado == null) return;

  if (estado.reverb != null)
  {
    if (estado.createdReverb)
      Object.Destroy(estado.reverb);
    else
    {
      estado.reverb.reverbPreset = estado.reverbPresetBefore;
      estado.reverb.enabled = estado.reverbWasEnabled;
    }
  }

  if (estado.lowPass != null)
  {
    if (estado.createdLowPass)
      Object.Destroy(estado.lowPass);
    else
    {
      estado.lowPass.cutoffFrequency = estado.lowPassCutoffBefore;
      estado.lowPass.lowpassResonanceQ = estado.lowPassResonanceBefore;
      estado.lowPass.enabled = estado.lowPassWasEnabled;
    }
  }

  if (estado.echo != null)
  {
    if (estado.createdEcho)
      Object.Destroy(estado.echo);
    else
    {
      estado.echo.wetMix = estado.echoWetBefore;
      estado.echo.dryMix = estado.echoDryBefore;
      estado.echo.delay = estado.echoDelayBefore;
      estado.echo.decayRatio = estado.echoDecayBefore;
      estado.echo.enabled = estado.echoWasEnabled;
    }
  }
}

private static GameObject GetOrCreateUndergroundTravelMarker()
{
  if (undergroundTravelMarker != null) return undergroundTravelMarker;

  undergroundTravelMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
  undergroundTravelMarker.name = "UndergroundTravelMarker";
  undergroundTravelMarker.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);

  Collider col = undergroundTravelMarker.GetComponent<Collider>();
  if (col != null) Object.Destroy(col);

  Renderer renderer = undergroundTravelMarker.GetComponent<Renderer>();
  if (renderer != null)
  {
    Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
    if (shader == null) shader = Shader.Find("Unlit/Color");
    if (shader == null) shader = Shader.Find("Standard");

    Material markerMaterial = new Material(shader);
    markerMaterial.color = new Color(0.62f, 0.98f, 0.76f, 0.85f);
    renderer.sharedMaterial = markerMaterial;
    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    renderer.receiveShadows = false;
  }

  undergroundTravelMarker.SetActive(false);
  return undergroundTravelMarker;
}




}



