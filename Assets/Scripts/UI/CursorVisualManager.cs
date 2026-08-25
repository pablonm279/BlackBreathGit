using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorVisualManager : MonoBehaviour
{
  private const string RutaConfiguracion = "CursorVisualConfig";

  private static CursorVisualManager instancia;
  private static readonly HashSet<Object> OrigenesAlerta = new HashSet<Object>();

  private readonly List<RaycastResult> resultadosUI = new List<RaycastResult>(16);
  private PointerEventData datosPuntero;
  private EventSystem eventSystemDatosPuntero;
  private CursorVisualConfig configuracion;
  private Texture2D texturaAplicada;
  private Vector2 hotspotAplicado;
  private CursorMode modoAplicado;
  private bool cursorAplicado;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void CrearAntesDeCargarEscena()
  {
    if (instancia != null)
    {
      return;
    }

    CursorVisualManager existente = FindFirstObjectByType<CursorVisualManager>(FindObjectsInactive.Include);
    if (existente != null)
    {
      instancia = existente;
      return;
    }

    GameObject go = new GameObject("CursorVisualManager");
    instancia = go.AddComponent<CursorVisualManager>();
    DontDestroyOnLoad(go);
  }

  public static void EstablecerAlerta(Object origen, bool activa)
  {
    if (origen == null)
    {
      return;
    }

    if (activa)
    {
      OrigenesAlerta.Add(origen);
    }
    else
    {
      OrigenesAlerta.Remove(origen);
    }
  }

  private void Awake()
  {
    if (instancia != null && instancia != this)
    {
      Destroy(gameObject);
      return;
    }

    instancia = this;
    DontDestroyOnLoad(gameObject);
    configuracion = Resources.Load<CursorVisualConfig>(RutaConfiguracion);
    if (configuracion == null)
    {
      Debug.LogWarning($"[CursorVisualManager] No se encontro Resources/{RutaConfiguracion}.asset.");
    }
  }

  private void LateUpdate()
  {
    if (configuracion == null)
    {
      return;
    }

    LimpiarOrigenesAlertaDestruidos();
    EstadoCursorVisual estado = ObtenerEstadoDeseado();
    Aplicar(estado);
  }

  private EstadoCursorVisual ObtenerEstadoDeseado()
  {
    if (OrigenesAlerta.Count > 0)
    {
      return EstadoCursorVisual.Alerta;
    }

    if (Input.GetMouseButton(0))
    {
      return EstadoCursorVisual.Presionado;
    }

    return HayInteractuableBajoCursor()
      ? EstadoCursorVisual.Interactivo
      : EstadoCursorVisual.Normal;
  }

  private bool HayInteractuableBajoCursor()
  {
    if (TryEvaluarUI(out bool hayElementoUI))
    {
      return true;
    }

    if (hayElementoUI)
    {
      return false;
    }

    Camera camara = Camera.main;
    if (camara == null)
    {
      return false;
    }

    Ray rayo = camara.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(rayo, out RaycastHit hit3D, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
    {
      return EsInteractuableMundo(hit3D.transform);
    }

    RaycastHit2D hit2D = Physics2D.GetRayIntersection(rayo, Mathf.Infinity, Physics2D.DefaultRaycastLayers);
    return hit2D.collider != null && EsInteractuableMundo(hit2D.transform);
  }

  private bool TryEvaluarUI(out bool hayElementoUI)
  {
    hayElementoUI = false;
    EventSystem eventSystemActual = EventSystem.current;
    if (eventSystemActual == null)
    {
      datosPuntero = null;
      return false;
    }

    if (datosPuntero == null || eventSystemDatosPuntero != eventSystemActual)
    {
      datosPuntero = new PointerEventData(eventSystemActual);
      eventSystemDatosPuntero = eventSystemActual;
    }

    datosPuntero.Reset();
    datosPuntero.position = Input.mousePosition;
    resultadosUI.Clear();
    eventSystemActual.RaycastAll(datosPuntero, resultadosUI);

    for (int i = 0; i < resultadosUI.Count; i++)
    {
      GameObject objetivo = resultadosUI[i].gameObject;
      if (objetivo == null)
      {
        continue;
      }

      hayElementoUI = true;
      return EsInteractuableUI(objetivo);
    }

    return false;
  }

  private static bool EsInteractuableUI(GameObject objetivo)
  {
    CursorInteractuable marca = objetivo.GetComponentInParent<CursorInteractuable>();
    if (marca != null)
    {
      return marca.EstaHabilitado;
    }

    Selectable selectable = objetivo.GetComponentInParent<Selectable>();
    if (selectable != null)
    {
      return selectable.isActiveAndEnabled && selectable.IsInteractable();
    }

    return TieneHandlerActivo<IPointerClickHandler>(objetivo)
      || TieneHandlerActivo<IPointerDownHandler>(objetivo)
      || TieneHandlerActivo<IBeginDragHandler>(objetivo);
  }

  private static bool TieneHandlerActivo<T>(GameObject objetivo) where T : IEventSystemHandler
  {
    GameObject receptor = ExecuteEvents.GetEventHandler<T>(objetivo);
    if (receptor == null)
    {
      return false;
    }

    Component componente = receptor.GetComponent(typeof(T));
    Behaviour comportamiento = componente as Behaviour;
    return comportamiento == null || comportamiento.isActiveAndEnabled;
  }

  private static bool EsInteractuableMundo(Transform objetivo)
  {
    if (objetivo == null)
    {
      return false;
    }

    CursorInteractuable marca = objetivo.GetComponentInParent<CursorInteractuable>();
    if (marca != null)
    {
      return marca.EstaHabilitado;
    }

    Nodo nodo = objetivo.GetComponentInParent<Nodo>();
    if (nodo != null)
    {
      return nodo.isActiveAndEnabled && nodo.EstaVisiblePorVision();
    }

    Unidad unidad = objetivo.GetComponentInParent<Unidad>();
    if (unidad != null)
    {
      return unidad.isActiveAndEnabled
        && !unidad.EstaOcultoVisualmenteParaJugador()
        && (BattleManager.Instance == null || !BattleManager.Instance.EntradaBatallaBloqueadaPorUI);
    }

    Casilla casilla = objetivo.GetComponentInParent<Casilla>();
    return casilla != null
      && casilla.isActiveAndEnabled
      && BattleManager.Instance != null
      && !BattleManager.Instance.EntradaBatallaBloqueadaPorUI;
  }

  private void Aplicar(EstadoCursorVisual estado)
  {
    Texture2D textura = configuracion.ObtenerTextura(estado);
    Vector2 hotspot = configuracion.ObtenerHotspot(estado);
    CursorMode modo = configuracion.ModoCursor;

    if (cursorAplicado && texturaAplicada == textura && hotspotAplicado == hotspot && modoAplicado == modo)
    {
      return;
    }

    texturaAplicada = textura;
    hotspotAplicado = hotspot;
    modoAplicado = modo;
    cursorAplicado = true;
    Cursor.SetCursor(textura, hotspot, modo);
  }

  private static void LimpiarOrigenesAlertaDestruidos()
  {
    OrigenesAlerta.RemoveWhere(origen => origen == null);
  }
}
