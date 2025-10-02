using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Nodo : MonoBehaviour
{
  private ContenedorDeNodos scContenedorNodos2;
  public int tipoNodo;//1-Batalla  2-Elie  3-Evento  4-Claro
  public int posXNodo; // de 1 a 11 que tan adelante está el nodo en el mapa,
  public int posYNodo; // de A (1) a E (5) en que camino va, A siendo arriba E abajo.

  public bool nodoDespejado;
  public int cantidadConexiones = 0;

  public int costoMovimiento = 1;

  public List<Nodo> DestinosPosibles = new List<Nodo>();
  
  public bool revelado = false;

  public MapaManager scMapaManager;
  void Start()
  {
    scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;

    int random = UnityEngine.Random.Range(0, 100);

    if (random < 20 && posXNodo > 1) //20% chances que cueste el doble llegar a este nodo - Camino dificil
    {
      costoMovimiento = 2;
    }
   
  }

  public void LlegoCaravana()
  {
    CampaignManager.Instance.MoviendoCaravana = false;
    scMapaManager.nodoActual = this;

    string hayExploracionExplorador = "";
    foreach (Personaje pers in CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if (pers.ActividadSeleccionada == 9) //Explorador: Exploración
      {
        hayExploracionExplorador = pers.sNombre;

      }
      if (pers.Camp_Enfermo > 0)
      {
        pers.Camp_Enfermo -= 1; //Cada viaje se cura un día
      }
      if (pers.Camp_Moral > 0) //Moral tiende a cero
      {
        pers.Camp_Moral -= 1;
      }
      if (pers.Camp_Moral < 0) //Moral tiende a cero
      {
        pers.Camp_Moral += 1;
      }

    }

    if (hayExploracionExplorador != "")//Explorador: Exploración
    {
      TiradaExploracion(200, false); //Explora si o si los adyacentes
      TiradaExploracion(55, true, hayExploracionExplorador); //Y tiene 35% de explorar los siguientes y -15% de explorar los siguientes etc.

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
      CampaignManager.Instance.EscribirLog("-La Caravana ha viajado con exceso de Carga. -10 Esperanza +1 Fatiga");
    }

    int chancesAtajo = 15; //15% de chances de encontrar un atajo

    chancesAtajo += 5 * CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9); //+5% de chances de encontrar un atajo si hay Exploradores explorando

    if(UnityEngine.Random.Range(0, 100) < chancesAtajo && posXNodo < 9)
    {
      CampaignManager.Instance.EscribirLog("-Se ha encontrado un atajo subterráneo.");

      EncontrarAtajo(2, 0);
    }



    CampaignManager.Instance.CambiarFatigaActual(fatigaSuma);


    CampaignManager.Instance.CambiarEsperanzaActual(esperanzaSuma);

    CampaignManager.Instance.LlegarANodo(tipoNodo); 

    MarcarCaminosPosibles();
    
  
  }

 public void EncontrarAtajo(int X, int Y)
{
    if (scContenedorNodos2 == null)
        scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;

    int nextX = posXNodo + X;
    List<Nodo> posiblesAtajos = new List<Nodo>();

    // 1) Candidatos en nextX con ventana vertical [-Y, +Y]
    for (int dy = -Y; dy <= Y; dy++)
    {
        int y = posYNodo + dy;
        if (y < 1 || y > 5) continue;

        Nodo c = scContenedorNodos2.ObtenerNodoSegunXY(nextX, y);
        if (c == null) continue;
        if (DestinosPosibles.Contains(c)) continue; // ya está conectado

        // 2) Filtro A-B-C: limitar Bs a los destinos de A que están en x+1
        bool hayRutaIntermedia = false;
        foreach (var b in DestinosPosibles)
        {
            if (b == null) continue;
            if (b.posXNodo != posXNodo + 1) continue; // SOLO el frente inmediato
            if (b.DestinosPosibles != null && b.DestinosPosibles.Contains(c))
            {
                hayRutaIntermedia = true;
                break;
            }
        }

        if (!hayRutaIntermedia)
            posiblesAtajos.Add(c);
    }

    // 3) Fallbacks si quedó vacío: ampliar ventana y, si sigue vacío, permitir atajo aunque exista A-B-C
    if (posiblesAtajos.Count == 0)
    {
        if (Y < 2)
        {
            EncontrarAtajo(X, Y + 1); // reintenta con ventana más amplia
            return;
        }

        // Último recurso: elegí alguno en nextX que no esté ya conectado (evitás duplicado A→C)
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
  public bool yatiroConexiones = false;
  Nodo vieneDeNodo;
  public void DeterminarConexiones()
  {
    int xadelante = posXNodo + 1;
    scContenedorNodos2 = CampaignManager.Instance.scMapaManager.scContenedordeNodos;
    if (!yatiroConexiones)
    {
      yatiroConexiones = true;
      if ((posXNodo == 0) && (posYNodo == 0)) //NodoOrigen
      {
        int random =UnityEngine.Random.Range(1, 2);

        if (random == 1)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 1));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 3));
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(1, 5));
        }
        else if (random == 2)
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
        }

        TiradaExploracion(300, false);
      }
      else if (posXNodo == 1)
      {
        ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, posYNodo - 1));
        ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, posYNodo));
      }
      else if (posYNodo == 1 && posXNodo < 10)
      {
        int random1 =UnityEngine.Random.Range(1, 5);

        if (random1 == 1) //A
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1)); //A
        }
        else if (random1 == 2)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1)); //A
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B

        }
        else if (random1 == 3)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
        }
        else if (random1 == 4)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1)); //A
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
        }
      }
      else if (posYNodo == 2 && posXNodo < 10)
      {
        int random2 =UnityEngine.Random.Range(1, 6);

        if (random2 == 1) //B
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 1)); //A
        }
        else if (random2 == 2)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C

        }
        else if (random2 == 3)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
        }
        else if (random2 == 4)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C
        }
        if (random2 == 5) //B
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C
        }
      }
      else if (posYNodo == 3 && posXNodo < 10)
      {
        int random3 =UnityEngine.Random.Range(1, 6);

        if (random3 == 1) //C
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
        }
        else if (random3 == 2)
        {

          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C

        }
        else if (random3 == 3)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
        }
        else if (random3 == 4)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
        }
        else if (random3 == 5)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 2)); //B
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
        }
      }
      else if (posYNodo == 4 && posXNodo < 10)
      {
        int random4 =UnityEngine.Random.Range(1, 6);

        if (random4 == 1) //D
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
        }
        else if (random4 == 2)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5)); //E

        }
        else if (random4 == 3)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C
        }
        else if (random4 == 4)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D

        }
        else if (random4 == 5)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 3)); //C
        }
      }
      else if (posYNodo == 5 && posXNodo < 10)
      {
        int random5 =UnityEngine.Random.Range(1, 5);

        if (random5 == 1) //E
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5)); //E
        }
        else if (random5 == 2)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5)); //E
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D

        }
        else if (random5 == 3)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
        }
        else if (random5 == 4)
        {
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 4)); //D
          ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(xadelante, 5)); //E
        }
      }
      else if (posXNodo == 10)
      {
        ConectarConNodo(scContenedorNodos2.ObtenerNodoSegunXY(11, 10));
      }
    }
  }
  public Material lineMaterial;
  private float lineWidth = 0.2f;

  public float lineHeightOffset = 0.01f; // Añade un pequeño offset en altura para evitar giros
  public GameObject linePrefab; // Asigna tu prefab de línea en el inspector

  public void ConectarConNodo(Nodo nodoB, bool esPorAbajo = false)
  {
    if (nodoB != null)
    {
      Nodo nodoA = this;
      nodoA.DestinosPosibles.Add(nodoB);

      cantidadConexiones++;

      // Crear un GameObject para la línea
      GameObject lineObject = Instantiate(linePrefab, this.transform);
      LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

      Vector3 p0 = nodoA.transform.position;
      Vector3 p3 = nodoB.transform.position;

      Vector3 p1, p2;

     /* if (esPorAbajo)
      {
        // Controla la profundidad y la "erraticidad"
        float profundidad =UnityEngine.Random.Range(-0.2f, -0.22f); // Más profundo
        float offsetX1 =UnityEngine.Random.Range(-1.5f, 1.5f);
        float offsetX2 =UnityEngine.Random.Range(-1.5f, 1.5f);

        // Primer punto de control: baja y se tuerce
        p1 = p0 + new Vector3(offsetX1, profundidad, 0);

        // Segundo punto de control: sigue bajo tierra, pero se acerca al destino
        p2 = p3 + new Vector3(offsetX2, profundidad, 0);

        // Opcional: añade un poco de "serpenteo" en la curva
        // (puedes ajustar los valores para más o menos curvatura)
      }
      else
      {*/
        float control1 =UnityEngine.Random.Range(-1.3f, 1.3f);
        float control2 =UnityEngine.Random.Range(-1.3f, 1.3f);
        int control =UnityEngine.Random.Range(1, 5);
        p1 = new Vector3(0, 0, 0);
        p2 = new Vector3(0, 0, 0);
        if (control == 1)
        {
          p1 = p0 + Vector3.right * control1;
          p2 = p3 + Vector3.left * control2;
        }
        if (control == 2)
        {
          p1 = p0 + Vector3.left * control1;
          p2 = p3 + Vector3.right * control2;
        }
        if (control == 3)
        {
          p1 = p0 + Vector3.left * control1;
          p2 = p3 + Vector3.left * control2;
        }
        if (control == 4)
        {
          p1 = p0 + Vector3.right * control1;
          p2 = p3 + Vector3.right * control2;
        }
     // }
      
      // Definir la cantidad de puntos en la curva
      int resolution = 20;
      lineRenderer.positionCount = resolution;

      // Calcular los puntos en la curva y asignarlos al LineRenderer
      for (int i = 0; i < resolution; i++)
      {   
        float t = i / (float)(resolution - 1);
        Vector3 point = BezierCurve.GetPoint(p0, p1, p2, p3, t);

        // Si esPorAbajo, suaviza la curva al final y reduce la profundidad máxima
        if (esPorAbajo && t > 0.1f && t < 0.9f)
        {
          // Menos erraticidad y menos profundidad
          float erraticY = (Mathf.PerlinNoise(i * 0.3f, Time.time) * 0.4f - 0.2f) * Mathf.Lerp(1f, 0.5f, Mathf.Abs(t - 0.5f) * 2f);
          // Suaviza la profundidad al acercarse al final/inicio
          float depthFactor = Mathf.Sin(Mathf.PI * t); // 0 en extremos, 1 en el medio
          point.y += erraticY;
          point.y = Mathf.Lerp(point.y, Mathf.Lerp(p0.y, p3.y, t), 0.4f + 0.5f * (1f - depthFactor));
        }

        lineRenderer.SetPosition(i, point);
      }

      lineRenderer.transform.Rotate(45, 0, 0);

      nodoB.DeterminarConexiones();
    }
  }
  public Material caminoLento;
  public void PosicionarObjetoEnNodo(GameObject go)
  {
    go.transform.position = transform.position;

  }

  private void OnMouseDown()
  {

    if (EventSystem.current.IsPointerOverGameObject() && !TooltipNodos.Instance.tooltipObject.activeInHierarchy)
    {
      return;
    }
    if (scMapaManager.nodoActual.DestinosPosibles.Contains(this))
    {
      CampaignManager.Instance.MoviendoCaravana = true;

      MoverJugadorANodo(scMapaManager.nodoActual, this);
      if (posXNodo - scMapaManager.nodoActual.posXNodo > 1) //Si se mueve a un nodo mas lejano que 1, se considera un atajo
      {
        CampaignManager.Instance.EscribirLog("-Al viajar por el atajo subterráneo, la moral de la caravana disminuye. -5 Esperanza");
        CampaignManager.Instance.CambiarEsperanzaActual(-5);
      }
     
    }
  }
  public float velocidadMovimiento = 6f;
  public void MoverJugadorANodo(Nodo nodoOrigen, Nodo nodoDestino)
  {
    velocidadMovimiento = 2 + 3 / nodoDestino.costoMovimiento;
    if (nodoDestino != null && nodoOrigen.DestinosPosibles.Contains(nodoDestino))
    {
      // Encontrar la línea correspondiente entre nodoOrigen y nodoDestino
      Transform lineaTransform = null;
      foreach (Transform child in nodoOrigen.transform)
      {
        if (child.name.Contains("LineaCaminos"))
        {
          LineRenderer lr = child.GetComponent<LineRenderer>();
          if (lr != null && lr.GetPosition(lr.positionCount - 1) == nodoDestino.transform.position)
          {
            lineaTransform = child;
            break;
          }
        }
      }

      if (lineaTransform != null)
      {
        vieneDeNodo = nodoOrigen;
        CampaignManager.Instance.ViajeIniciado(nodoDestino); // Suma 1 aliento negro cada vez que se mueve
        StartCoroutine(MoverAloLargoDeLaCurva(lineaTransform.GetComponent<LineRenderer>()));
      }
      else
      {
        Debug.LogWarning("No se encontró la línea correspondiente entre los nodos.");
      }
    }
    else
    {
      Debug.LogWarning("Nodo destino no válido o no está en la lista de destinos posibles.");
    }
  }

 private IEnumerator MoverAloLargoDeLaCurva(LineRenderer lineRenderer)
{
    GameObject caravana = scMapaManager.goCaravana;
    float t = 0f;
    int resolution = lineRenderer.positionCount;

    Vector3 inicio = lineRenderer.GetPosition(0);
    Vector3 fin    = lineRenderer.GetPosition(resolution - 1);
    Vector3 dirAvance = (fin - inicio).normalized;

    Vector3 ultima = caravana.transform.position;

    while (t < 1f)
    {
        t += Time.deltaTime * velocidadMovimiento / resolution;
        Vector3 nuevaPosicion = CalcularPosicionEnCurva(lineRenderer, t);

        // No permitir retroceso respecto a la dirección global de la línea
        Vector3 delta = nuevaPosicion - ultima;
        if (Vector3.Dot(delta, dirAvance) < 0f)
        {
            nuevaPosicion = ultima;
        }

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

  public Material MaterialCaminoOriginal;
  public Material MaterialCaminoMarcado;
  public Material MaterialCaminoUsado;
  public Material MaterialAtajo;


  void MarcarCaminosPosibles()
  {
    // Primero, verifica las conexiones desde el nodo del que viene la caravana.
    foreach (Transform child in vieneDeNodo.transform)
    {
      if (child.name.Contains("LineaCaminos"))
      {
        LineRenderer lr = child.GetComponent<LineRenderer>();
        if (lr != null && lr.GetPosition(lr.positionCount - 1) == transform.position)
        {
          lr.material = MaterialCaminoUsado;
        }
        else
        {
          lr.material = MaterialCaminoOriginal;
        }
      }
    }

    // Ahora, marca los caminos posibles desde el nodo actual.
    foreach (Transform child in transform)
    {
      if (child.name.Contains("LineaCaminos"))
      {
        LineRenderer lr = child.GetComponent<LineRenderer>();
        if (lr != null)
        {
          // Encuentra el nodo destino correspondiente para este LineRenderer.
          Nodo nodoDestino = DestinosPosibles.Find(nodo => nodo.transform.position == lr.GetPosition(lr.positionCount - 1));

          if (nodoDestino != null)
          {
            // Cambia el material basado en el costo de movimiento.
            if (nodoDestino.costoMovimiento > 1)
            {
              lr.material = caminoLento; // Material para caminos con costo de movimiento alto.
            }
            else
            {
              lr.material = MaterialCaminoMarcado; // Material para caminos normales.
            }
          }

          if (nodoDestino.posXNodo - posXNodo > 1) //Solamente los atajos se conectan a nodos mas lejanos que 1
          {
             nodoDestino.costoMovimiento = 2; 
             lr.material = MaterialAtajo; // Material para atajos
          }
        }
      }
    }
  }

  #endregion


  public void Revelar(bool esAtajo)
  {
    revelado = true;
    if (tipoNodo == 0)
    { //
      int rand = UnityEngine.Random.Range(1, 8);
      if (posXNodo == 1)
      {
        switch (rand)
        {
          case 1: tipoNodo = 1; break;   // Batalla
          case 2: tipoNodo = 1; break;   // Batalla
          case 3: tipoNodo = 2; break;   // Evento
          case 4: tipoNodo = 5; break;   // Recursos
          case 5: tipoNodo = 8; break;   // Elite
          case 6: tipoNodo = 5; break;   // Recursos
          case 7: tipoNodo = 1; break;   // Batalla


        }
      }
      if (posXNodo == 2)
      {
      switch (rand)
      {
        case 1: tipoNodo = 1; break;   // Batalla
        case 2: tipoNodo = 1; break;   // Batalla
        case 3: tipoNodo = 14; break;  // Santuario
        case 4: tipoNodo = 2; break;   // Evento
        case 5: tipoNodo = 5; break;   // Recursos
        case 6: tipoNodo = 6; break;   // Puesto Comercial
        case 7: tipoNodo = 8; break;   // Elite
      }
      }
      if (posXNodo == 3)
      {
      switch (rand)
      {
        case 1: tipoNodo = 1; break;   // Batalla
        case 2: tipoNodo = 11; break;  // Zona Expuesta
        case 3: tipoNodo = 8; break;   // Elite
        case 4: tipoNodo = 2; break;   // Evento
        case 5: tipoNodo = 1; break;   // Batalla
        case 6: tipoNodo = 11; break;  // Zona Expuesta
        case 7: tipoNodo = 3; break;   // Claro
      }
      }
      if (posXNodo == 4)
      {
      switch (rand)
      {
        case 1: tipoNodo = 1; break;   // Batalla
        case 2: tipoNodo = 1; break;   // Batalla
        case 3: tipoNodo = 8; break;   // Elite
        case 4: tipoNodo = 11; break;  // Zona Expuesta
        case 5: tipoNodo = 5; break;   // Recursos
        case 6: tipoNodo = 14; break;  // Santuario
        case 7: tipoNodo = 2; break;   // Evento
      }
      }
      if (posXNodo == 5)
      {
      switch (rand)
      {
        case 1: tipoNodo = 3; break;   // Claro
        case 2: tipoNodo = 3; break;   // Claro
        case 3: tipoNodo = 4; break;   // Asentamiento
        case 4: tipoNodo = 14; break;  // Santuario
        case 5: tipoNodo = 4; break;   // Asentamiento
        case 6: tipoNodo = 7; break;   // Personaje
        case 7: tipoNodo = 5; break;   // Recursos
      }
      }
      if (posXNodo == 6)
      {
      switch (rand)
      {
        case 1: tipoNodo = 11; break;  // Zona Expuesta
        case 2: tipoNodo = 1; break;   // Batalla
        case 3: tipoNodo = 8; break;   // Elite
        case 4: tipoNodo = 2; break;   // Evento
        case 5: tipoNodo = 5; break;   // Recursos
        case 6: tipoNodo = 11; break;  // Zona Expuesta
        case 7: tipoNodo = 1; break;   // Batalla
      }
      }
      if (posXNodo == 7)
      {
      switch (rand)
      {
        case 1: tipoNodo = 1; break;   // Batalla
        case 2: tipoNodo = 1; break;   // Batalla
        case 3: tipoNodo = 4; break;   // Asentamiento
        case 4: tipoNodo = 2; break;   // Evento
        case 5: tipoNodo = 5; break;   // Recursos
        case 6: tipoNodo = 6; break;   // Puesto Comercial
        case 7: tipoNodo = 3; break;   // Claro
      }
      }
      if (posXNodo == 8)
      {
      switch (rand)
      {
        case 1: tipoNodo = 14; break;  // Santuario
        case 2: tipoNodo = 1; break;   // Batalla
        case 3: tipoNodo = 14; break;  // Santuario
        case 4: tipoNodo = 2; break;   // Evento
        case 5: tipoNodo = 5; break;   // Recursos
        case 6: tipoNodo = 7; break;   // Personaje
        case 7: tipoNodo = 1; break;   // Batalla
      }
      }
      if (posXNodo == 9)
      {
      switch (rand)
      {
        case 1: tipoNodo = 1; break;   // Batalla
        case 2: tipoNodo = 8; break;   // Elite
        case 3: tipoNodo = 2; break;   // Evento
        case 4: tipoNodo = 8; break;   // Elite
        case 5: tipoNodo = 1; break;   // Batalla
        case 6: tipoNodo = 14; break;  // Santuario
        case 7: tipoNodo = 1; break;   // Batalla
      }
      }
      if (posXNodo == 10)
      {
      switch (rand)
      {
        case 1: tipoNodo = 1; break;   // Batalla
        case 2: tipoNodo = 4; break;   // Asentamiento
        case 3: tipoNodo = 2; break;   // Evento
        case 4: tipoNodo = 14; break;   // Santuario
        case 5: tipoNodo = 3; break;   // Claro
        case 6: tipoNodo = 4; break;   // Asentamiento
        case 7: tipoNodo = 3; break;   // Claro
      }
      }
      if (posXNodo == 11)
      {
        tipoNodo = 10;//batalla final
      }
    }

    ActivarNodoVisual(tipoNodo, esAtajo);


    int chancesAtaqueSubterraneo = 20; //20% de chances de que al llegar al nodo, la caravana sea emboscada por un ataque subterraneo
    if (esAtajo  &&UnityEngine.Random.Range(0, 100) < chancesAtaqueSubterraneo)
    {
      tipoNodo = 12; //Se convierte en un nodo de batalla subterránea
    }
 

  }
  public void TiradaExploracion(int chances, bool continua, string actividadExploradorON = "")
  {


    foreach (Nodo nodo in DestinosPosibles)
    {
      int tirada =UnityEngine.Random.Range(0, 100);
      if (tirada < chances)
      {
        nodo.Revelar(false); //Revela el nodo
        if (actividadExploradorON != "")
        {
          CampaignManager.Instance.EscribirLog($"-{actividadExploradorON} ha Explorado con éxito el camino adelante.");
        }
       

        if (continua)
        {
          nodo.TiradaExploracion(chances - 15, true);
        }


      }

    }

  }
  bool esMisterioso = false; //Nodo que no fue revelado visualmente
  void ActivarNodoVisual(int num, bool esAtajo)
  {
    foreach (Transform child in transform)
    {
      if (child.name.Contains("Nodo"))
      {
        child.gameObject.SetActive(false);
      }

    }

    //Chances NodoMisterioso 15%
    int chancesMisterioso = 15;
    if(CampaignManager.Instance.intTipoClima == 5) //Niebla
    {
        chancesMisterioso += 10;
    }
    if (CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) > 0) //Si hay un explorador con actividad Explorando
    {
        chancesMisterioso -= CampaignManager.Instance.CuantosPersonajesHacenTalActividad(9) * 5; //-5 por cada explorador
    }
   
    //Controles
    if (posXNodo == 10 || posXNodo == 1) //Si es el nodo final o inicial, no puede ser misterioso
    {
      chancesMisterioso = 0;
    }
    if (!revelado)//Si ya habia sido revelado, no puede ser misterioso
    {
      chancesMisterioso = 0;
    }
   

    if(UnityEngine.Random.Range(0, 100) < chancesMisterioso) //Si es un nodo normal, hay 20% de que sea un misterioso
    {
      num = 12; //Nodo Misterioso - No se revela visualmente
      esMisterioso = true;  print("Nodo Misterioso Activado");
    }
    if (esAtajo) //Nodo Salida subterraneo, sobreescribe cualquier otra visual
    {
      num = 13; 
    }

    switch (num)
    {
      case 1: transform.GetChild(1).gameObject.SetActive(true); break;  //Batalla
      case 2: transform.GetChild(2).gameObject.SetActive(true); break;  //Evento
      case 3: transform.GetChild(3).gameObject.SetActive(true); break;  //Claro
      case 4: transform.GetChild(4).gameObject.SetActive(true); break;  //Asentamiento
      case 5: transform.GetChild(5).gameObject.SetActive(true); break;  //Recursos
      case 6: transform.GetChild(6).gameObject.SetActive(true); break;  //Puesto Comercial
      case 7: transform.GetChild(7).gameObject.SetActive(true); break;  //Personaje
      case 8: transform.GetChild(8).gameObject.SetActive(true); break;  //Elite
      case 10: transform.GetChild(8).gameObject.SetActive(true); break;  //Elite (Batalla Final)
      case 11: transform.GetChild(9).gameObject.SetActive(true); break;  //Zona Expuesta
      case 12: transform.GetChild(10).gameObject.SetActive(true); break;  //Nodo Misterioso - No se revela visualmente
      case 13: transform.GetChild(11).gameObject.SetActive(true); break;  //Nodo Salida subterraneo
      case 14: transform.GetChild(12).gameObject.SetActive(true); break;  //Nodo Salida subterraneo
    }

  }


  public string descripcion;

    void OnMouseEnter()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) // evita conflicto con UI
        {
            switch (tipoNodo)
            {
                case 1: descripcion = "Combate directo."; break;
                case 2: descripcion = "Evento aleatorio."; break;
                case 3: descripcion = "Claro tranquilo."; break;
                case 4: descripcion = "Asentamiento."; break;
                case 5: descripcion = "Recolección de Recursos."; break;
                case 6: descripcion = "Puesto de Comercio."; break;
                case 7: descripcion = "Adquisición de Personajes."; break;
                case 8: descripcion = "Combate directo contra enemigos de Élite."; break;
                case 10: descripcion = "Batalla final de la Zona actual."; break;
                case 11: descripcion = "<b>(!)</b> Zona Expuesta, la caravana será emboscada."; break;

                default: descripcion = "Nodo Desconocido."; break;
            }
            if (esMisterioso)
            {
                descripcion = "Nodo Misterioso, no se ha logrado revelar.";
            }
            if ( transform.GetChild(11).gameObject.activeInHierarchy)
            {
                descripcion = "Salida del atajo subterraneo, no sabemos que hay del otro lado.";
            }
             if ( transform.GetChild(12).gameObject.activeInHierarchy)
            {
                descripcion = "Santuario de Purificadores.";
            }



            Vector3 pos = Input.mousePosition;
            TooltipNodos.Instance.ShowTooltip(descripcion, pos);
        }
    }

    void OnMouseExit()
    {
        TooltipNodos.Instance.HideTooltip();
    }
}