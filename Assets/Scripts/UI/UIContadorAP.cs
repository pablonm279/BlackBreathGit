using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIContadorAP : MonoBehaviour
{

    public GameObject circuloAPprefab;
    public GameObject esfuerzoAPprefab;
    private Sprite spriteAPDisponible;
    private Sprite spriteAPUsado;
    private Sprite spriteEsforzar;
    [SerializeField] private float duracionVaciadoAP = 0.16f;
    [SerializeField] private float demoraEntreVaciadosAP = 0.035f;
    private Unidad unidadRepresentada;
    private int apRepresentado = -1;
    private Coroutine corrutinaVaciadoAP;
    private RectTransform indicadorAPCasilla;
    private GridLayoutGroup layoutIndicadorAPCasilla;
    private Canvas canvasIndicadorAPCasilla;
    private readonly List<Image> imagenesIndicadorAPCasilla = new List<Image>();
    private Unidad unidadIndicadorAPCasilla;
    private int apIndicadorAPCasilla = -1;
    private int apMarcadosIndicadorAPCasilla;
    private int esfuerzoIndicadorAPCasilla;
 
   
   
    private void Start()
    {
       Image imagenAPDisponible = circuloAPprefab != null ? circuloAPprefab.GetComponent<Image>() : null;
       spriteAPDisponible = imagenAPDisponible != null ? imagenAPDisponible.sprite : null;
       spriteAPUsado = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/ap_usado");
       spriteEsforzar = Resources.Load<Sprite>("Imagenes/RecursosSprites/IconosTextoCombate/Iconos/esforzar");
       ActualizarAPCirculos();

    }

    private void OnEnable()
    {
      Canvas.willRenderCanvases -= ActualizarIndicadorAPCasilla;
      Canvas.willRenderCanvases += ActualizarIndicadorAPCasilla;
    }

    private void OnDisable()
    {
      Canvas.willRenderCanvases -= ActualizarIndicadorAPCasilla;
      if (indicadorAPCasilla != null)
      {
        indicadorAPCasilla.gameObject.SetActive(false);
      }
    }

    private void OnDestroy()
    {
      Canvas.willRenderCanvases -= ActualizarIndicadorAPCasilla;
      if (indicadorAPCasilla != null)
      {
        Destroy(indicadorAPCasilla.gameObject);
      }
    }

 
    public void ActualizarAPCirculos()
    {
      if (!isActiveAndEnabled || !gameObject.scene.isLoaded)
      {
        return;
      }

      BattleManager battleManager = BattleManager.Instance;
      if (battleManager == null || !battleManager.isActiveAndEnabled)
      {
        return;
      }

      Unidad unidadSeleccionada = battleManager.unidadActiva;
      int apObjetivo = unidadSeleccionada != null
        ? Mathf.Max(0, (int)unidadSeleccionada.ObtenerAPActual())
        : 0;

      bool animarGasto = unidadSeleccionada != null
        && unidadRepresentada == unidadSeleccionada
        && apRepresentado > apObjetivo
        && transform.childCount >= apRepresentado;

      if (unidadIndicadorAPCasilla != unidadSeleccionada || apIndicadorAPCasilla != apObjetivo)
      {
        apMarcadosIndicadorAPCasilla = 0;
        esfuerzoIndicadorAPCasilla = 0;
      }

      if (animarGasto)
      {
        if (corrutinaVaciadoAP == null)
        {
          corrutinaVaciadoAP = StartCoroutine(
            AnimarVaciadoAP(apObjetivo, apRepresentado - apObjetivo, unidadSeleccionada));
        }
      }
      else if (corrutinaVaciadoAP == null || unidadRepresentada != unidadSeleccionada || apRepresentado != apObjetivo)
      {
        CancelarVaciadoAP();
        ReconstruirCirculos(apObjetivo);
      }

      unidadRepresentada = unidadSeleccionada;
      apRepresentado = apObjetivo;
      SincronizarIndicadorAPCasilla(unidadSeleccionada, apObjetivo);

    // Luego de actualizar la UI, revisar si debe indicarse pasar turno
     battleManager.RevisarAPUnidadActiva();
     battleManager.ActualizarCasillasMelee();
    }

  private IEnumerator AnimarVaciadoAP(int apObjetivo, int cantidadGastada, Unidad unidadAlIniciar)
  {
    int primerIndice = Mathf.Clamp(apObjetivo, 0, transform.childCount);
    int cantidadAnimable = Mathf.Min(cantidadGastada, transform.childCount - primerIndice);
    List<Image> imagenes = new List<Image>();
    List<RectTransform> rects = new List<RectTransform>();
    List<Vector3> escalasBase = new List<Vector3>();

    for (int i = 0; i < cantidadAnimable; i++)
    {
      Transform circulo = transform.GetChild(primerIndice + i);
      Image imagen = circulo.GetComponent<Image>();
      RectTransform rect = circulo as RectTransform;
      if (imagen == null || rect == null)
      {
        continue;
      }

      imagen.type = Image.Type.Filled;
      imagen.fillMethod = Image.FillMethod.Radial360;
      imagen.fillOrigin = (int)Image.Origin360.Top;
      imagen.fillClockwise = false;
      imagen.fillAmount = 1f;
      imagenes.Add(imagen);
      rects.Add(rect);
      escalasBase.Add(rect.localScale);
    }

    float duracion = Mathf.Max(0.01f, duracionVaciadoAP);
    float demora = Mathf.Max(0f, demoraEntreVaciadosAP);
    float duracionTotal = duracion + demora * Mathf.Max(0, imagenes.Count - 1);
    float tiempo = 0f;

    while (tiempo < duracionTotal)
    {
      tiempo += Time.unscaledDeltaTime;
      for (int i = 0; i < imagenes.Count; i++)
      {
        if (imagenes[i] == null || rects[i] == null)
        {
          continue;
        }

        float progreso = Mathf.Clamp01((tiempo - demora * i) / duracion);
        float suavizado = Mathf.SmoothStep(0f, 1f, progreso);
        imagenes[i].fillAmount = 1f - suavizado;
        imagenes[i].color = new Color(1f, 1f, 1f, 1f - suavizado * 0.65f);
        float escala = Mathf.Lerp(1f, 0.72f, suavizado);
        rects[i].localScale = escalasBase[i] * escala;
      }
      yield return null;
    }

    corrutinaVaciadoAP = null;
    if (BattleManager.Instance != null && BattleManager.Instance.unidadActiva == unidadAlIniciar)
    {
      ReconstruirCirculos(apObjetivo);
      apRepresentado = apObjetivo;

      int apActual = Mathf.Max(0, (int)unidadAlIniciar.ObtenerAPActual());
      if (apActual != apObjetivo)
      {
        ActualizarAPCirculos();
      }
    }
  }

  private void CancelarVaciadoAP()
  {
    if (corrutinaVaciadoAP == null)
    {
      return;
    }

    StopCoroutine(corrutinaVaciadoAP);
    corrutinaVaciadoAP = null;
  }

  private void ReconstruirCirculos(int cantidad)
  {
    while (transform.childCount > 0)
    {
      Transform circulo = transform.GetChild(transform.childCount - 1);
      circulo.gameObject.SetActive(false);
      circulo.SetParent(null, false);
      Destroy(circulo.gameObject);
    }

    for (int i = 0; i < cantidad; i++)
    {
      Instantiate(circuloAPprefab, transform);
    }
  }

  public void MarcarCirculos(int n)
  {
    if (!isActiveAndEnabled || !gameObject.scene.isLoaded || BattleManager.Instance == null || BattleManager.Instance.unidadActiva == null)
    {
      return;
    }

    // Obtén la cantidad total de elementos en el GridLayoutGroup
    int totalCirculos = transform.childCount;

    Unidad unidadSeleccionada = BattleManager.Instance.unidadActiva.gameObject.GetComponent<Unidad>();
    int apSinEsfuerzo = (int)unidadSeleccionada.ObtenerAPActual();

    // Asegúrate de que n está dentro de los límites y no sea mayor que apSinEsfuerzo
    n = Mathf.Clamp(n, 0, Mathf.Min(totalCirculos, apSinEsfuerzo));
    apMarcadosIndicadorAPCasilla = n;
    SincronizarIndicadorAPCasilla(unidadSeleccionada, apSinEsfuerzo);

    // Itera sobre los últimos N elementos y cambia su color a azul
    for (int i = totalCirculos - n; i < totalCirculos; i++)
    {
      Transform circuloTransform = transform.GetChild(i);
      Image circuloImage = circuloTransform.GetComponent<Image>();

      if (circuloImage == null)
      {
        continue;
      }

      circuloImage.color = Color.white;
      if (spriteAPUsado != null)
      {
        circuloImage.sprite = spriteAPUsado;
      }
    }
   
  }


  public void ResetearCirculos()
  {
    ActualizarAPCirculos();
    apMarcadosIndicadorAPCasilla = 0;
    esfuerzoIndicadorAPCasilla = 0;
    Unidad unidadSeleccionada = BattleManager.Instance != null ? BattleManager.Instance.unidadActiva : null;
    int apActual = unidadSeleccionada != null ? Mathf.Max(0, (int)unidadSeleccionada.ObtenerAPActual()) : 0;
    CancelarVaciadoAP();
    ReconstruirCirculos(apActual);
    unidadRepresentada = unidadSeleccionada;
    apRepresentado = apActual;
    SincronizarIndicadorAPCasilla(unidadSeleccionada, apActual);
  }

public void SeEsforzaria(int n)
{
    esfuerzoIndicadorAPCasilla = Mathf.Max(0, n);
    Unidad unidadSeleccionada = BattleManager.Instance != null ? BattleManager.Instance.unidadActiva : null;
    int apActual = unidadSeleccionada != null ? Mathf.Max(0, (int)unidadSeleccionada.ObtenerAPActual()) : 0;
    SincronizarIndicadorAPCasilla(unidadSeleccionada, apActual);

    if(n > 0)
    {
      for (int i = 0; i < n; i++)
      {
        GameObject nuevoCirculo = Instantiate(esfuerzoAPprefab, transform);
        if (spriteEsforzar != null)
        {
          Image img = nuevoCirculo.GetComponent<Image>();
          if (img != null)
          {
            img.sprite = spriteEsforzar;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = Color.white;
          }
        }
      }
    }
}

  private void SincronizarIndicadorAPCasilla(Unidad unidad, int apDisponible)
  {
    unidadIndicadorAPCasilla = unidad;
    apIndicadorAPCasilla = Mathf.Max(0, apDisponible);

    if (!CrearIndicadorAPCasillaSiHaceFalta())
    {
      return;
    }

    int esfuerzo = Mathf.Max(0, esfuerzoIndicadorAPCasilla);
    int cantidadTotal = apIndicadorAPCasilla + esfuerzo;
    AjustarCantidadImagenesIndicador(cantidadTotal);

    int marcados = Mathf.Clamp(apMarcadosIndicadorAPCasilla, 0, apIndicadorAPCasilla);
    int primerAPMarcado = apIndicadorAPCasilla - marcados;

    for (int i = 0; i < imagenesIndicadorAPCasilla.Count; i++)
    {
      Image imagen = imagenesIndicadorAPCasilla[i];
      if (imagen == null)
      {
        continue;
      }

      bool esEsfuerzo = i >= apIndicadorAPCasilla;
      bool estaMarcado = !esEsfuerzo && i >= primerAPMarcado;
      imagen.sprite = esEsfuerzo ? spriteEsforzar : estaMarcado ? spriteAPUsado : spriteAPDisponible;
      imagen.type = Image.Type.Simple;
      imagen.fillAmount = 1f;
      imagen.preserveAspect = false;
      imagen.color = Color.white;
      imagen.raycastTarget = false;
    }
  }

  private bool CrearIndicadorAPCasillaSiHaceFalta()
  {
    if (indicadorAPCasilla != null)
    {
      return true;
    }

    Canvas canvas = GetComponentInParent<Canvas>();
    canvasIndicadorAPCasilla = canvas != null ? canvas.rootCanvas : null;
    RectTransform canvasRect = canvasIndicadorAPCasilla != null
      ? canvasIndicadorAPCasilla.transform as RectTransform
      : null;
    if (canvasRect == null || circuloAPprefab == null)
    {
      return false;
    }

    GameObject indicador = new GameObject(
      "Indicador AP Casilla Activa",
      typeof(RectTransform),
      typeof(CanvasGroup),
      typeof(GridLayoutGroup));
    indicador.layer = canvasIndicadorAPCasilla.gameObject.layer;
    indicador.transform.SetParent(canvasRect, false);
    indicador.transform.SetAsLastSibling();

    indicadorAPCasilla = indicador.GetComponent<RectTransform>();
    indicadorAPCasilla.anchorMin = new Vector2(0.5f, 0.5f);
    indicadorAPCasilla.anchorMax = new Vector2(0.5f, 0.5f);
    indicadorAPCasilla.pivot = new Vector2(0.5f, 0.5f);

    CanvasGroup canvasGroup = indicador.GetComponent<CanvasGroup>();
    canvasGroup.interactable = false;
    canvasGroup.blocksRaycasts = false;

    layoutIndicadorAPCasilla = indicador.GetComponent<GridLayoutGroup>();
    layoutIndicadorAPCasilla.startCorner = GridLayoutGroup.Corner.LowerLeft;
    layoutIndicadorAPCasilla.startAxis = GridLayoutGroup.Axis.Horizontal;
    layoutIndicadorAPCasilla.childAlignment = TextAnchor.MiddleCenter;
    layoutIndicadorAPCasilla.constraint = GridLayoutGroup.Constraint.FixedRowCount;
    layoutIndicadorAPCasilla.constraintCount = 1;
    return true;
  }

  private void AjustarCantidadImagenesIndicador(int cantidad)
  {
    while (imagenesIndicadorAPCasilla.Count < cantidad)
    {
      GameObject circulo = Instantiate(circuloAPprefab, indicadorAPCasilla);
      RectTransform rectCirculo = circulo.transform as RectTransform;
      if (rectCirculo != null)
      {
        rectCirculo.localScale = Vector3.one;
      }

      Image imagen = circulo.GetComponent<Image>();
      if (imagen == null)
      {
        Destroy(circulo);
        break;
      }

      imagen.raycastTarget = false;
      imagenesIndicadorAPCasilla.Add(imagen);
    }

    while (imagenesIndicadorAPCasilla.Count > cantidad)
    {
      int ultimoIndice = imagenesIndicadorAPCasilla.Count - 1;
      Image imagen = imagenesIndicadorAPCasilla[ultimoIndice];
      imagenesIndicadorAPCasilla.RemoveAt(ultimoIndice);
      if (imagen != null)
      {
        imagen.gameObject.SetActive(false);
        imagen.transform.SetParent(null, false);
        Destroy(imagen.gameObject);
      }
    }
  }

  private void ActualizarIndicadorAPCasilla()
  {
    BattleManager battleManager = BattleManager.Instance;
    Unidad unidadActiva = battleManager != null ? battleManager.unidadActiva : null;
    Casilla casilla = unidadActiva != null ? unidadActiva.CasillaPosicion : null;
    bool debeMostrarse = unidadActiva != null
      && !battleManager.bOcupado
      && unidadActiva.GetComponent<IAUnidad>() == null
      && casilla != null
      && casilla.Presente == unidadActiva.gameObject;

    if (!debeMostrarse)
    {
      if (indicadorAPCasilla != null)
      {
        indicadorAPCasilla.gameObject.SetActive(false);
      }
      return;
    }

    int apActual = Mathf.Max(0, (int)unidadActiva.ObtenerAPActual());
    if (unidadIndicadorAPCasilla != unidadActiva || apIndicadorAPCasilla != apActual)
    {
      apMarcadosIndicadorAPCasilla = 0;
      esfuerzoIndicadorAPCasilla = 0;
      SincronizarIndicadorAPCasilla(unidadActiva, apActual);
    }

    int cantidadTotal = apActual + Mathf.Max(0, esfuerzoIndicadorAPCasilla);
    if (indicadorAPCasilla == null || cantidadTotal <= 0)
    {
      if (indicadorAPCasilla != null)
      {
        indicadorAPCasilla.gameObject.SetActive(false);
      }
      return;
    }

    if (!ObtenerLimitesPantallaCasilla(casilla, out Vector2 minimoPantalla, out Vector2 maximoPantalla))
    {
      indicadorAPCasilla.gameObject.SetActive(false);
      return;
    }

    RectTransform canvasRect = canvasIndicadorAPCasilla != null
      ? canvasIndicadorAPCasilla.transform as RectTransform
      : null;
    if (canvasRect == null)
    {
      indicadorAPCasilla.gameObject.SetActive(false);
      return;
    }

    Camera camaraCanvas = canvasIndicadorAPCasilla.renderMode == RenderMode.ScreenSpaceOverlay
      ? null
      : canvasIndicadorAPCasilla.worldCamera;
    Vector2 centroInferiorPantalla = new Vector2(
      (minimoPantalla.x + maximoPantalla.x) * 0.5f,
      minimoPantalla.y);
    if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
      canvasRect,
      centroInferiorPantalla,
      camaraCanvas,
      out Vector2 posicionLocal))
    {
      indicadorAPCasilla.gameObject.SetActive(false);
      return;
    }

    RectTransformUtility.ScreenPointToLocalPointInRectangle(
      canvasRect,
      new Vector2(minimoPantalla.x, minimoPantalla.y),
      camaraCanvas,
      out Vector2 minimoLocal);
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
      canvasRect,
      new Vector2(maximoPantalla.x, minimoPantalla.y),
      camaraCanvas,
      out Vector2 maximoLocal);

    float anchoCasillaLocal = Mathf.Abs(maximoLocal.x - minimoLocal.x);
    float unidadesAncho = cantidadTotal - Mathf.Max(0, cantidadTotal - 1) * 0.1f;
    float diametro = Mathf.Clamp(anchoCasillaLocal * 0.78f / Mathf.Max(1f, unidadesAncho), 9f, 18f) * 1.5848217f;
    diametro = Mathf.Round(diametro * 2f) * 0.5f;
    float separacion = -diametro * 0.1f;

    layoutIndicadorAPCasilla.cellSize = new Vector2(diametro, diametro);
    layoutIndicadorAPCasilla.spacing = new Vector2(separacion, 0f);
    indicadorAPCasilla.sizeDelta = new Vector2(
      diametro * cantidadTotal + separacion * Mathf.Max(0, cantidadTotal - 1),
      diametro);
    Vector2 posicionIndicador = posicionLocal + Vector2.up * diametro * 1.55f;
    indicadorAPCasilla.anchoredPosition = new Vector2(
      Mathf.Round(posicionIndicador.x),
      Mathf.Round(posicionIndicador.y));

    indicadorAPCasilla.gameObject.SetActive(true);
  }

  private static bool ObtenerLimitesPantallaCasilla(
    Casilla casilla,
    out Vector2 minimoPantalla,
    out Vector2 maximoPantalla)
  {
    minimoPantalla = new Vector2(float.MaxValue, float.MaxValue);
    maximoPantalla = new Vector2(float.MinValue, float.MinValue);

    Camera camara = Camera.main;
    Collider colliderCasilla = casilla != null ? casilla.GetComponent<Collider>() : null;
    if (camara == null || colliderCasilla == null)
    {
      return false;
    }

    Bounds limites = colliderCasilla.bounds;
    for (int i = 0; i < 8; i++)
    {
      Vector3 esquina = new Vector3(
        (i & 1) == 0 ? limites.min.x : limites.max.x,
        (i & 2) == 0 ? limites.min.y : limites.max.y,
        (i & 4) == 0 ? limites.min.z : limites.max.z);
      Vector3 puntoPantalla = camara.WorldToScreenPoint(esquina);
      if (puntoPantalla.z <= 0f)
      {
        return false;
      }

      minimoPantalla = Vector2.Min(minimoPantalla, puntoPantalla);
      maximoPantalla = Vector2.Max(maximoPantalla, puntoPantalla);
    }

    return maximoPantalla.x >= 0f
      && minimoPantalla.x <= Screen.width
      && maximoPantalla.y >= 0f
      && minimoPantalla.y <= Screen.height;
  }


}
