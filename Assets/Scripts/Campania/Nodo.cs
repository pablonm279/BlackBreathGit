using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

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

  public MapaManager scMapaManager;

  // --- Visual caminos ---
  [Header("Caminos")]
  public GameObject linePrefab;          // Debe traer LineRenderer
  public float lineWidth = 0.6f;         // Ancho de la cinta (CaminoMesh)
  public float lineHeightOffset = 0.02f; // Evitar z-fighting

  // Materiales
  public Material MaterialCaminoOriginal;
  public Material MaterialCaminoMarcado;
  public Material MaterialCaminoUsado;
  public Material MaterialAtajo;
  public Material caminoLento;

  // Lógica movimiento
  public float velocidadMovimiento = 6f;

  // Internos
  public bool yatiroConexiones = false;
  Nodo vieneDeNodo;
  bool esMisterioso = false; // Nodo no revelado visualmente
  public bool nodoIncendiado = false;
  public bool nodoRitual = false;
  int numVisualActual = -1;

  void Start()
  {
    scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;

    int random = UnityEngine.Random.Range(0, 100);
    if (random < 20 && posXNodo > 1) //20% camino difícil
      costoMovimiento = 2;
  }

  public void LlegoCaravana()
  {
    CampaignManager.Instance.MoviendoCaravana = false;
    scMapaManager.nodoActual = this;

    string hayExploracionExplorador = "";
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers.ActividadSeleccionada == 9) hayExploracionExplorador = pers.sNombre;
      if (pers.Camp_Enfermo > 0) pers.Camp_Enfermo -= 1;
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

    if (UnityEngine.Random.Range(0, 100) < chancesAtajo && posXNodo < 9)
    {
      CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("-Se ha encontrado un atajo subterráneo."));
      EncontrarAtajo(2, 0);
    }

    CampaignManager.Instance.CambiarFatigaActual(fatigaSuma);
    CampaignManager.Instance.CambiarEsperanzaActual(esperanzaSuma);
    CampaignManager.Instance.LlegarANodo(tipoNodo, posXNodo, this);

    MarcarCaminosPosibles();
  }

  public void EncontrarAtajo(int X, int Y)
  {
    if (scContenedorNodos2 == null)
      scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;

    int nextX = posXNodo + X;
    List<Nodo> posiblesAtajos = new List<Nodo>();

    for (int dy = -Y; dy <= Y; dy++)
    {
      int y = posYNodo + dy;
      if (y < 1 || y > 5) continue;

      Nodo c = scContenedorNodos2.ObtenerNodoSegunXY(nextX, y);
      if (c == null) continue;
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
        if (c != null && !DestinosPosibles.Contains(c))
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

    if (yatiroConexiones) return;
    yatiroConexiones = true;

    if ((posXNodo == 0) && (posYNodo == 0)) // Nodo origen
    {
      int random = 1;// UnityEngine.Random.Range(1, 5); // 1..4

      if (random == 1)
      {
        ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 1));
        ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 3));
        ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 5));
      }
      /*  else if (random == 2)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 2));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 3));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 4));
        }
        else if (random == 3)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 2));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 3));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 5));
        }
        else if (random == 4)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 1));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 3));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 4));
        }*/

      TiradaExploracion(300, false);
    }
    else if (posXNodo == 1)
    {
      ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, posYNodo - 1));
      ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, posYNodo));
    }
    else if (posYNodo == 1 && posXNodo < 10)
    {
      int random1 = UnityEngine.Random.Range(1, 5);
      if (random1 == 1) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1));
      else if (random1 == 2) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); }
      else if (random1 == 3) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2));
      else if (random1 == 4) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); }
    }
    else if (posYNodo == 2 && posXNodo < 10)
    {
      int random2 = UnityEngine.Random.Range(1, 6);
      if (random2 == 1) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1));
      else if (random2 == 2) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); }
      else if (random2 == 3) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2));
      else if (random2 == 4) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); }
      else if (random2 == 5) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3));
    }
    else if (posYNodo == 3 && posXNodo < 10)
    {
      int random3 = UnityEngine.Random.Range(1, 6);
      if (random3 == 1) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2));
      else if (random3 == 2) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3));
      else if (random3 == 3) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); }
      else if (random3 == 4) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); }
      else if (random3 == 5) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); }
    }
    else if (posYNodo == 4 && posXNodo < 10)
    {
      int random4 = UnityEngine.Random.Range(1, 6);
      if (random4 == 1) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4));
      else if (random4 == 2) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5)); }
      else if (random4 == 3) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3));
      else if (random4 == 4) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4));
      else if (random4 == 5) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); }
    }
    else if (posYNodo == 5 && posXNodo < 10)
    {
      int random5 = UnityEngine.Random.Range(1, 5);
      if (random5 == 1) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5));
      else if (random5 == 2) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); }
      else if (random5 == 3) ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4));
      else if (random5 == 4) { ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5)); }
    }
    else if (posXNodo == 10)
    {
      ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(11, 10));
    }
  }

  public void ConectarConNodo(Nodo nodoB, bool esPorAbajo = false)
  {
    if (nodoB == null) return;

    Nodo nodoA = this;
    nodoA.DestinosPosibles.Add(nodoB);
    cantidadConexiones++;

    // Crear línea
    GameObject lineObject = Instantiate(linePrefab, this.transform);
    lineObject.name = "LineaCaminos";

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

    // Dirección y perpendicular para “empujar” la curva
    Vector3 dir = (p3 - p0);
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
    else
    {
      // 30% de probabilidad de una curvatura más pronunciada también para no-atajo
      if (UnityEngine.Random.value < 0.3f && cantidadConexiones < 2)
        outward = UnityEngine.Random.Range(1.8f, 2.4f); // curvatura notable pero menor que un atajo
      else
        outward = UnityEngine.Random.Range(0.25f, 0.8f); // normal: leve
    }

    // Evitar que los primeros 2 ramos salgan muy curvos
    if (!esPorAbajo && cantidadConexiones < 2)
      outward *= 0.75f;

    float sideSign = UnityEngine.Random.value < 0.5f ? -1f : 1f;

    // Dónde colocar puntos de control
    float t1 = UnityEngine.Random.Range(0.14f, 0.22f);
    float t2 = UnityEngine.Random.Range(0.62f, 0.78f);

    // Pequeña variación lateral
    float jitter1 = UnityEngine.Random.Range(-0.5f, 0.5f);
    float jitter2 = UnityEngine.Random.Range(-0.5f, 0.5f);

    Vector3 p1 = p0 + dir * (dist * t1) + perp * (sideSign * outward * (0.35f + 0.65f * Mathf.Abs(jitter1)));
    Vector3 p2 = p3 - dir * (dist * (1f - t2)) + perp * (sideSign * outward * (0.35f + 0.65f * Mathf.Abs(jitter2)));

    // Curva Bézier → SIEMPRE PLANA en Y (evita hundirse bajo el suelo)
    int resolution = 20;
    lineRenderer.positionCount = resolution;
    for (int i = 0; i < resolution; i++)
    {
      float t = i / (float)(resolution - 1);
      Vector3 point = BezierCurve.GetPoint(p0, p1, p2, p3, t);

      // Forzamos Y a la interpolación del tramo (plano) + leve offset si querés
      float yPlano = Mathf.Lerp(p0.y, p3.y, t);
      point.y = yPlano; // <- clave: no hundimos ni subimos secciones

      lineRenderer.SetPosition(i, point);
    }

    // Construir malla plana del camino
    var caminoMesh = lineObject.GetComponent<CaminoMesh>();
    if (caminoMesh == null) caminoMesh = lineObject.AddComponent<CaminoMesh>();
    //  caminoMesh.width   = lineWidth;
    // caminoMesh.yOffset = lineHeightOffset;                // 0.02 aprox
    caminoMesh.RebuildFromLine();

    // Material según tipo (normal vs atajo)
    SetMaterialCamino(lineObject.transform, esPorAbajo ? MaterialAtajo : MaterialCaminoOriginal);

    // Continuar tirando conexiones
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

    var destruir = new List<GameObject>();
    foreach (Transform child in transform)
    {
      if (child.name.Contains("LineaCaminos")) destruir.Add(child.gameObject);
      if (child.name.Contains("Nodo")) child.gameObject.SetActive(false);
    }
    foreach (var go in destruir) Destroy(go);

    gameObject.SetActive(true);
  }

  public void PosicionarObjetoEnNodo(GameObject go)
  {
    go.transform.position = transform.position;
  }

  private void OnMouseDown()
  {
    if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 6)
      return;

    if (EventSystem.current.IsPointerOverGameObject() && !TooltipNodos.Instance.tooltipObject.activeInHierarchy)
      return;

    if (scMapaManager.nodoActual.DestinosPosibles.Contains(this))
    {
      CampaignManager.Instance.MoviendoCaravana = true;
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
    velocidadMovimiento = 2f + 3f / nodoDestino.costoMovimiento;

    if (nodoDestino == null || !nodoOrigen.DestinosPosibles.Contains(nodoDestino))
    {
      Debug.LogWarning("Nodo destino no válido o no está en la lista de destinos posibles.");
      return;
    }

    // Buscar línea
    Transform lineaTransform = null;
    foreach (Transform child in nodoOrigen.transform)
    {
      if (!child.name.Contains("LineaCaminos")) continue;
      LineRenderer lr = child.GetComponent<LineRenderer>();
      if (lr != null && lr.GetPosition(lr.positionCount - 1) == nodoDestino.transform.position)
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

    vieneDeNodo = nodoOrigen;
    CampaignManager.Instance.ViajeIniciado(nodoDestino);

    if (scMapaManager != null && scMapaManager.goCaravana != null)
    {
      var girarCaravana = scMapaManager.goCaravana.GetComponent<GirarCaravana>();
      if (girarCaravana != null)
        girarCaravana.CambiarSpriteSegunRuta(nodoOrigen, nodoDestino);
    }

    StartCoroutine(MoverAloLargoDeLaCurva(lineaTransform.GetComponent<LineRenderer>()));
  }

  private IEnumerator MoverAloLargoDeLaCurva(LineRenderer lineRenderer)
  {
    GameObject caravana = scMapaManager.goCaravana;
    float t = 0f;
    int resolution = lineRenderer.positionCount;

    Vector3 inicio = lineRenderer.GetPosition(0);
    Vector3 fin = lineRenderer.GetPosition(resolution - 1);
    Vector3 dirAvance = (fin - inicio).normalized;

    Vector3 ultima = caravana.transform.position;

    while (t < 1f)
    {
      t += Time.deltaTime * velocidadMovimiento / resolution;
      Vector3 nuevaPosicion = CalcularPosicionEnCurva(lineRenderer, t);

      Vector3 delta = nuevaPosicion - ultima;
      if (Vector3.Dot(delta, dirAvance) < 0f) nuevaPosicion = ultima;

      caravana.transform.position = nuevaPosicion;
      ultima = nuevaPosicion;

      yield return null;
    }

    caravana.transform.position = fin;
    LlegoCaravana();
  }

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

  // --- Helper materiales: aplica al LR y a la malla ---
  private void SetMaterialCamino(Transform linea, Material mat)
  {
    var lr = linea.GetComponent<LineRenderer>();
    if (lr != null) lr.sharedMaterial = mat;

    var mr = linea.GetComponent<MeshRenderer>();
    if (mr != null) mr.sharedMaterial = mat;
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

        if (lr.GetPosition(lr.positionCount - 1) == transform.position)
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

      Nodo nodoDestino = DestinosPosibles.Find(n => n.transform.position == lr.GetPosition(lr.positionCount - 1));
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
      if (posXNodo == 2)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 14; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 8; break;
        }
      }
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
      if (posXNodo == 4)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 11; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 14; break;
          case 7: tipoNodo = 2; break;
        }
      }
      if (posXNodo == 5)
      {
        switch (rand)
        {
          case 1: tipoNodo = 3; break;
          case 2: tipoNodo = 3; break;
          case 3: tipoNodo = 4; break;
          case 4: tipoNodo = 14; break;
          case 5: tipoNodo = 4; break;
          case 6: tipoNodo = 7; break;
          case 7: tipoNodo = 5; break;
        }
      }
      if (posXNodo == 6)
      {
        switch (rand)
        {
          case 1: tipoNodo = 11; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 8; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 11; break;
          case 7: tipoNodo = 1; break;
        }
      }
      if (posXNodo == 7)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 1; break;
          case 3: tipoNodo = 4; break;
          case 4: tipoNodo = 2; break;
          case 5: tipoNodo = 5; break;
          case 6: tipoNodo = 6; break;
          case 7: tipoNodo = 3; break;
        }
      }
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
      if (posXNodo == 9)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 8; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 8; break;
          case 5: tipoNodo = 1; break;
          case 6: tipoNodo = 14; break;
          case 7: tipoNodo = 1; break;
        }
      }
      if (posXNodo == 10)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;
          case 2: tipoNodo = 4; break;
          case 3: tipoNodo = 2; break;
          case 4: tipoNodo = 14; break;
          case 5: tipoNodo = 3; break;
          case 6: tipoNodo = 4; break;
          case 7: tipoNodo = 3; break;
        }
      }
      if (posXNodo == 11) { tipoNodo = 10; }
    }

    ActivarNodoVisual(tipoNodo, esAtajo, estabaRevelado);

    int chancesAtaqueSubterraneo = 20;
    if (esAtajo && UnityEngine.Random.Range(0, 100) < chancesAtaqueSubterraneo)
    {
      tipoNodo = 12; // batalla subterránea
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
          if (!string.IsNullOrEmpty(actividadExploradorON))
            CampaignManager.Instance.EscribirLog("<color=#7ED6F7>-" + actividadExploradorON + TRADU.i.Traducir(" ha Explorado con éxito el camino adelante.</color>") + $"(Tirada: {tirada} < {cappedChance})");
          else
            CampaignManager.Instance.EscribirLog(TRADU.i.Traducir("<color=#7ED6F7>-Durante el Descanso, se ha Explorado con éxito el camino adelante.</color>") + $"(Tirada: {tirada} < {cappedChance})");

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

  void DesactivarGraficosNodo()
  {
    foreach (Transform child in transform)
    {
      if (!child.name.Contains("Nodo")) continue;
      int idx = child.GetSiblingIndex();
      if (idx == 14 || idx == 15) continue; // no desactivar child 14 o 15
      child.gameObject.SetActive(false);
    }
  }

   bool ActivarVisualPorCodigo(int codigo)
  {
   
    int indice = -1;
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

    }


    if (indice < 0 || indice >= transform.childCount) return false;
    transform.GetChild(indice).gameObject.SetActive(true);
    return true;
  }

  public void ActivarNodoVisual(int num, bool esAtajo, bool estabaRevelado)
  {
    DesactivarGraficosNodo();

    esMisterioso = false;

    int chancesMisterioso = 15;
    if (CampaignManager.Instance.intTipoClima == 5) chancesMisterioso += 10; // Niebla
    if (CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) > 0)
      chancesMisterioso -= CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) * 5;

    if (posXNodo == 10 || posXNodo == 1) chancesMisterioso = 0;
    if (estabaRevelado) chancesMisterioso = 0;
    if (nodoRitual) chancesMisterioso = 0;
    if (nodoIncendiado) chancesMisterioso = 0;

    if (UnityEngine.Random.Range(0, 100) < chancesMisterioso)
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
        transform.GetChild(13).gameObject.SetActive(true); // vfx de revelado (no inmediatos)
    }

  }

  public string descripcion;

  void OnEnable()
  {
    int codigoAAplicar = numVisualActual > 0 ? numVisualActual : tipoNodo;
    if (codigoAAplicar <= 0) return;

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

    if (!visualActivado) return;

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
      case 8: descripcion = TRADU.i.Traducir("Combate directo contra enemigos de Élite."); break;
      case 10: descripcion = TRADU.i.Traducir("Batalla final de la Zona actual."); break;
      case 11: descripcion = TRADU.i.Traducir("<b>(!)</b> Zona Expuesta, la caravana será emboscada."); break;
      case 15: descripcion = TRADU.i.Traducir("Batalla Kale'Tav"); break;

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
    print("ActivarIncendio called");
    nodoIncendiado = true;
    transform.GetChild(14).gameObject.SetActive(true);
    
  }

  public void DesactivarIncendio()
  {print("DesactivarIncendio called");
    nodoIncendiado = false;
    transform.GetChild(14).gameObject.SetActive(false);
  }

  public void ActivarRitual()
  {
    print("ActivarRitual called");
    nodoRitual = true;
    transform.GetChild(15).gameObject.SetActive(true);
  } 
  
  public void DesactivarRitual()
  {
    print("DesactivarRitual called");
    nodoRitual = false;
    transform.GetChild(15).gameObject.SetActive(false);
  } 
}
