using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleIntroController : MonoBehaviour
{
  private const float DuracionLlegada = 0.45f;
  private const float PausaEntreLlegadas = 0.1f;
  private const float DesfasePoseHeroes = 0.1f;
  private const float EsperaAntesDeLlegadas = 1f;
  private const float CierreIntro = 1f;
  private const float DuracionFadeSalida = 0.22f;
  private const float OffsetLlegada = 5f;
  private const float AlphaSilueta = 0.65f;
  private const float AlphaMaxVineta = 0.32f;

  private sealed class EstadoVisualEspera
  {
    [NonSerialized] public bool gameObjectActivo;
    [NonSerialized] public Graphic[] graficos;
    [NonSerialized] public bool[] graficosActivos;
    [NonSerialized] public bool[] raycastTargets;
    [NonSerialized] public Color[] colores;
    [NonSerialized] public Collider[] colliders3D;
    [NonSerialized] public bool[] colliders3DActivos;
    [NonSerialized] public Collider2D[] colliders2D;
    [NonSerialized] public bool[] colliders2DActivos;
    [NonSerialized] public ParticleSystem[] particulas;
    [NonSerialized] public bool[] particulasActivas;
  }

  private struct LlegadaPendiente
  {
    public Unidad unidad;
    public int lado;

    public LlegadaPendiente(Unidad unidad, int lado)
    {
      this.unidad = unidad;
      this.lado = lado;
    }
  }

  private BattleManager battleManager;
  private readonly Dictionary<Unidad, EstadoVisualEspera> estadosVisualesEspera =
    new Dictionary<Unidad, EstadoVisualEspera>();
  private readonly Dictionary<Unidad, Casilla> casillasOriginalesHeroes =
    new Dictionary<Unidad, Casilla>();
  private readonly List<Casilla> casillasResaltadas = new List<Casilla>();
  private readonly List<UnidadPoseController> posesAlertaActivas = new List<UnidadPoseController>();
  private readonly List<Unidad> unidadesAutomaticasPreparadas = new List<Unidad>();

  private TaskCompletionSource<Casilla> seleccionCasillaPendiente;
  private Unidad heroePendiente;
  private Casilla casillaHover;
  private bool despliegueManualActivo;
  private bool animandoDespliegue;
  private bool permiteSaltar;
  private bool saltoSolicitado;
  private bool cancelacionSolicitada;
  private bool bOcupadoPrevio;
  private RectTransform rectTextoIntro;
  private Vector2 anchorMinTextoIntroOriginal;
  private Vector2 anchorMaxTextoIntroOriginal;
  private Vector2 pivotTextoIntroOriginal;
  private Vector2 posicionTextoIntroOriginal;
  private bool posicionTextoIntroGuardada;
  private Image imagenVinetaIntro;
  private Texture2D texturaVinetaIntro;
  private Sprite spriteVinetaIntro;
  private int indiceHermanoTextoIntroOriginal;
  private bool indiceHermanoTextoIntroGuardado;
  private Image imagenSiluetaDespliegue;
  private Vector3 offsetSiluetaDesdeUnidad;
  private GameObject panelDespliegueManual;
  private TextMeshProUGUI etiquetaDespliegueManual;
  private GameObject panelMensajeEmboscada;
  private TextMeshProUGUI textoMensajeEmboscada;
  private GameObject textoComienzaBatalla;
  private bool textoComienzaBatallaMostrado;

  public bool IntroActiva { get; private set; }
  public bool DespliegueManualActivo => IntroActiva && despliegueManualActivo;

  public void Inicializar(BattleManager manager)
  {
    battleManager = manager;
  }

  private void Update()
  {
    if (IntroActiva && permiteSaltar && Input.GetKeyDown(KeyCode.Space))
    {
      saltoSolicitado = true;
    }
  }

  private void OnDisable()
  {
    cancelacionSolicitada = true;
    saltoSolicitado = true;
    seleccionCasillaPendiente?.TrySetResult(null);
  }

  public async Task EjecutarAsync(
    int tipoEmboscada,
    IReadOnlyList<Unidad> enemigosEnOrden,
    IReadOnlyList<Unidad> heroesEnOrden,
    string nombreFaccionEnemiga = null)
  {
    if (battleManager == null || IntroActiva)
    {
      return;
    }

    IntroActiva = true;
    cancelacionSolicitada = false;
    saltoSolicitado = false;
    bOcupadoPrevio = battleManager.bOcupado;
    battleManager.bOcupado = true;
    battleManager.PrepararUIParaIntroBatalla();
    PrepararTextoComienzaBatalla();
    if (tipoEmboscada != 2)
    {
      MostrarVinetaIntro();
    }

    List<Unidad> enemigos = FiltrarUnidades(enemigosEnOrden);
    List<Unidad> heroes = FiltrarUnidades(heroesEnOrden);
    AplicarEscalaInicialUnidades(enemigos);
    AplicarEscalaInicialUnidades(heroes);

    try
    {
      if (tipoEmboscada == 2)
      {
        permiteSaltar = false;
        await EjecutarDespliegueManualAsync(heroes);
      }
      else if (tipoEmboscada == 1 || tipoEmboscada == 3)
      {
        permiteSaltar = true;
        MostrarTextoSaltarIntro();
        ActivarPoseAlertaHeroes(heroes);
        PrepararLlegadasAutomaticas(enemigos, 2);
        BanterBattleDirector.NotificarIntroEmboscadaEnemiga(heroes);
        await EjecutarLlegadasAutomaticasAsync(CrearLlegadas(enemigos, 2));
        DesactivarPoseAlertaHeroes();
      }
      else
      {
        permiteSaltar = true;
        MostrarTextoSaltarIntro();
        PrepararHeroesVisiblesEnIdle(heroes);
        PrepararLlegadasAutomaticas(enemigos, 2);
        await EsperarRealtimeAsync(EsperaAntesDeLlegadas, true);
        if (!saltoSolicitado && HayEnemigosMovilesParaIntro(enemigos))
        {
          BanterBattleDirector.NotificarIntroBatallaNormal(heroes, nombreFaccionEnemiga);
        }
        Task reaccionHeroes = ActivarPoseAlertaHeroesDesfasadoAsync(heroes);
        Task llegadaEnemigos = EjecutarLlegadasAutomaticasAsync(
          CrearLlegadas(enemigos, 2),
          false);
        await Task.WhenAll(reaccionHeroes, llegadaEnemigos);
        await DesactivarPoseAlertaHeroesDesfasadoAsync();
      }

      permiteSaltar = false;
      OcultarTextoIntro();
      OcultarEtiquetaDespliegue();
      OcultarMensajeEmboscadaJugador();
      MostrarTextoComienzaBatalla();
      if (imagenVinetaIntro != null && imagenVinetaIntro.gameObject.activeSelf)
      {
        await EsperarRealtimeAsync(CierreIntro - DuracionFadeSalida, false);
        await DesvanecerVinetaAsync(DuracionFadeSalida);
      }
      else
      {
        await EsperarRealtimeAsync(CierreIntro, false);
      }
    }
    catch (Exception ex)
    {
      Debug.LogError("[BattleIntroController] La intro de batalla no pudo completarse normalmente.");
      Debug.LogException(ex, this);
    }
    finally
    {
      permiteSaltar = false;
      despliegueManualActivo = false;
      animandoDespliegue = false;
      seleccionCasillaPendiente = null;
      OcultarSiluetaActual();
      OcultarEtiquetaDespliegue();
      OcultarMensajeEmboscadaJugador();
      LimpiarResaltadoCasillas();
      DesactivarPoseAlertaHeroes();
      AsegurarPosicionesAutomaticas();
      RestaurarHeroesSinCasilla();
      RestaurarTodosLosVisualesEspera();
      OcultarTextoIntro();
      OcultarVinetaIntro();
      if (!textoComienzaBatallaMostrado)
      {
        MostrarTextoComienzaBatalla();
      }
      battleManager.bOcupado = bOcupadoPrevio;
      IntroActiva = false;
    }
  }

  public bool ProcesarClickCasilla(Casilla casilla)
  {
    if (!IntroActiva)
    {
      return false;
    }

    if (!despliegueManualActivo
      || animandoDespliegue
      || seleccionCasillaPendiente == null
      || !EsCasillaValidaDespliegue(casilla))
    {
      return true;
    }

    OcultarSiluetaActual();
    seleccionCasillaPendiente.TrySetResult(casilla);
    return true;
  }

  public bool ProcesarHoverCasilla(Casilla casilla)
  {
    if (!IntroActiva)
    {
      return false;
    }

    if (!despliegueManualActivo || animandoDespliegue || !EsCasillaValidaDespliegue(casilla))
    {
      OcultarSiluetaActual();
      return true;
    }

    if (casillaHover == casilla)
    {
      return true;
    }

    OcultarSiluetaActual();
    casillaHover = casilla;
    MostrarSiluetaHeroePendiente(casilla);
    return true;
  }

  public bool ProcesarSalidaCasilla(Casilla casilla)
  {
    if (!IntroActiva)
    {
      return false;
    }

    if (casillaHover == casilla)
    {
      OcultarSiluetaActual();
    }

    return true;
  }

  private async Task EjecutarDespliegueManualAsync(List<Unidad> heroes)
  {
    if (heroes.Count < 1 || battleManager.ladoB == null)
    {
      return;
    }

    OcultarTextoIntro();
    OcultarEtiquetaDespliegue();
    MostrarMensajeEmboscadaJugador();

    casillasOriginalesHeroes.Clear();
    estadosVisualesEspera.Clear();
    for (int i = 0; i < heroes.Count; i++)
    {
      Unidad heroe = heroes[i];
      Casilla casillaOriginal = heroe.CasillaPosicion;
      casillasOriginalesHeroes[heroe] = casillaOriginal;
      if (casillaOriginal != null && casillaOriginal.Presente == heroe.gameObject)
      {
        casillaOriginal.Presente = null;
      }

      heroe.CasillaPosicion = null;
      GuardarYOcultarVisualHeroe(heroe);
    }

    List<Casilla> casillasDisponibles = ObtenerCasillasValidasDespliegue();
    if (casillasDisponibles.Count < heroes.Count)
    {
      Debug.LogError("[BattleIntroController] No hay suficientes casillas aliadas libres para el despliegue manual. Se restauran las posiciones originales.");
      RestaurarHeroesSinCasilla();
      RestaurarTodosLosVisualesEspera();
      return;
    }

    despliegueManualActivo = true;
    for (int i = 0; i < heroes.Count && !cancelacionSolicitada; i++)
    {
      heroePendiente = heroes[i];
      RefrescarCasillasValidasResaltadas();
      MostrarTextoUbicarHeroe(heroePendiente);

      seleccionCasillaPendiente = new TaskCompletionSource<Casilla>();
      Casilla elegida = await seleccionCasillaPendiente.Task;
      seleccionCasillaPendiente = null;
      if (elegida == null || cancelacionSolicitada)
      {
        break;
      }

      animandoDespliegue = true;
      AsignarHeroeACasilla(heroePendiente, elegida);
      RestaurarVisualHeroe(heroePendiente);
      if (heroePendiente.esInmobil)
      {
        heroePendiente.transform.position = elegida.transform.position;
        heroePendiente.GetComponent<UnidadPoseController>()?.OnStopMove();
      }
      else
      {
        await AnimarLlegadaAsync(heroePendiente, 1, false);
      }
      animandoDespliegue = false;
    }

    heroePendiente = null;
    despliegueManualActivo = false;
    LimpiarResaltadoCasillas();
    OcultarEtiquetaDespliegue();
    OcultarMensajeEmboscadaJugador();
  }

  private void PrepararLlegadasAutomaticas(List<Unidad> unidades, int lado)
  {
    for (int i = 0; i < unidades.Count; i++)
    {
      Unidad unidad = unidades[i];
      if (unidad == null || unidad.esInmobil || unidad.CasillaPosicion == null)
      {
        continue;
      }

      if (!unidadesAutomaticasPreparadas.Contains(unidad))
      {
        unidadesAutomaticasPreparadas.Add(unidad);
      }

      unidad.transform.position = PosicionInicialLlegada(unidad.CasillaPosicion, lado);
      unidad.GetComponent<UnidadPoseController>()?.SetIdle();
    }
  }

  private void PrepararHeroesVisiblesEnIdle(List<Unidad> heroes)
  {
    for (int i = 0; i < heroes.Count; i++)
    {
      Unidad heroe = heroes[i];
      if (heroe == null || heroe.CasillaPosicion == null)
      {
        continue;
      }

      heroe.transform.position = heroe.CasillaPosicion.transform.position;
      heroe.GetComponent<UnidadPoseController>()?.SetIdle();
    }
  }

  private async Task EjecutarLlegadasAutomaticasAsync(
    List<LlegadaPendiente> llegadas,
    bool esperarAntesDeLlegadas = true)
  {
    List<LlegadaPendiente> animables = new List<LlegadaPendiente>();
    for (int i = 0; i < llegadas.Count; i++)
    {
      Unidad unidad = llegadas[i].unidad;
      if (unidad != null && !unidad.esInmobil && unidad.CasillaPosicion != null)
      {
        animables.Add(llegadas[i]);
      }
    }

    if (esperarAntesDeLlegadas
      && animables.Count > 0
      && !cancelacionSolicitada
      && !saltoSolicitado)
    {
      await EsperarRealtimeAsync(EsperaAntesDeLlegadas, true);
    }

    for (int i = 0; i < animables.Count && !cancelacionSolicitada; i++)
    {
      if (saltoSolicitado)
      {
        AjustarLlegadasRestantes(animables, i);
        break;
      }

      LlegadaPendiente llegada = animables[i];
      await AnimarLlegadaAsync(llegada.unidad, llegada.lado, true);
      if (saltoSolicitado)
      {
        AjustarLlegadasRestantes(animables, i + 1);
        break;
      }

      if (i < animables.Count - 1)
      {
        await EsperarRealtimeAsync(PausaEntreLlegadas, true);
      }
    }
  }

  private async Task AnimarLlegadaAsync(Unidad unidad, int lado, bool permitirSaltoActual)
  {
    if (unidad == null || unidad.CasillaPosicion == null)
    {
      return;
    }

    Vector3 final = unidad.CasillaPosicion.transform.position;
    Vector3 inicial = PosicionInicialLlegada(unidad.CasillaPosicion, lado);
    unidad.transform.position = inicial;
    UnidadPoseController pose = unidad.GetComponent<UnidadPoseController>();
    pose?.OnStartMove();
    unidad.ReproducirSonidoMovimiento();

    float transcurrido = 0f;
    while (transcurrido < DuracionLlegada && !cancelacionSolicitada)
    {
      if (permitirSaltoActual && saltoSolicitado)
      {
        break;
      }

      await Task.Yield();
      transcurrido += Time.unscaledDeltaTime;
      float t = Mathf.Clamp01(transcurrido / DuracionLlegada);
      unidad.transform.position = Vector3.Lerp(inicial, final, t);
    }

    unidad.transform.position = final;
    pose?.OnStopMove();
  }

  private async Task EsperarRealtimeAsync(float segundos, bool permitirSaltoActual)
  {
    float transcurrido = 0f;
    while (transcurrido < segundos && !cancelacionSolicitada)
    {
      if (permitirSaltoActual && saltoSolicitado)
      {
        return;
      }

      await Task.Yield();
      transcurrido += Time.unscaledDeltaTime;
    }
  }

  private void ActivarPoseAlertaHeroes(List<Unidad> heroes)
  {
    posesAlertaActivas.Clear();
    for (int i = 0; i < heroes.Count; i++)
    {
      UnidadPoseController pose = heroes[i] != null ? heroes[i].GetComponent<UnidadPoseController>() : null;
      if (pose == null)
      {
        continue;
      }

      pose.EnterPoseObjetivoHostil();
      posesAlertaActivas.Add(pose);
    }
  }

  private async Task ActivarPoseAlertaHeroesDesfasadoAsync(List<Unidad> heroes)
  {
    posesAlertaActivas.Clear();
    for (int i = 0; i < heroes.Count && !cancelacionSolicitada && !saltoSolicitado; i++)
    {
      UnidadPoseController pose = heroes[i] != null ? heroes[i].GetComponent<UnidadPoseController>() : null;
      if (pose != null)
      {
        pose.EnterPoseObjetivoHostil();
        posesAlertaActivas.Add(pose);
      }

      if (i < heroes.Count - 1)
      {
        await EsperarRealtimeAsync(DesfasePoseHeroes, true);
      }
    }
  }

  private async Task DesactivarPoseAlertaHeroesDesfasadoAsync()
  {
    if (saltoSolicitado || cancelacionSolicitada)
    {
      DesactivarPoseAlertaHeroes();
      return;
    }

    for (int i = 0; i < posesAlertaActivas.Count; i++)
    {
      UnidadPoseController pose = posesAlertaActivas[i];
      if (pose != null)
      {
        pose.ExitPoseObjetivoHostil();
        pose.SetIdle();
      }

      if (i < posesAlertaActivas.Count - 1)
      {
        await EsperarRealtimeAsync(DesfasePoseHeroes, false);
      }
    }
    posesAlertaActivas.Clear();
  }

  private void DesactivarPoseAlertaHeroes()
  {
    for (int i = 0; i < posesAlertaActivas.Count; i++)
    {
      UnidadPoseController pose = posesAlertaActivas[i];
      if (pose != null)
      {
        pose.ExitPoseObjetivoHostil();
        pose.SetIdle();
      }
    }
    posesAlertaActivas.Clear();
  }

  private void GuardarYOcultarVisualHeroe(Unidad heroe)
  {
    EstadoVisualEspera estado = new EstadoVisualEspera
    {
      gameObjectActivo = heroe.gameObject.activeSelf,
      graficos = heroe.GetComponentsInChildren<Graphic>(true),
      colliders3D = heroe.GetComponentsInChildren<Collider>(true),
      colliders2D = heroe.GetComponentsInChildren<Collider2D>(true),
      particulas = heroe.GetComponentsInChildren<ParticleSystem>(true)
    };

    estado.graficosActivos = new bool[estado.graficos.Length];
    estado.raycastTargets = new bool[estado.graficos.Length];
    estado.colores = new Color[estado.graficos.Length];
    for (int i = 0; i < estado.graficos.Length; i++)
    {
      estado.graficosActivos[i] = estado.graficos[i].enabled;
      estado.raycastTargets[i] = estado.graficos[i].raycastTarget;
      estado.colores[i] = estado.graficos[i].color;
      estado.graficos[i].enabled = false;
      estado.graficos[i].raycastTarget = false;
    }

    estado.colliders3DActivos = new bool[estado.colliders3D.Length];
    for (int i = 0; i < estado.colliders3D.Length; i++)
    {
      estado.colliders3DActivos[i] = estado.colliders3D[i].enabled;
      estado.colliders3D[i].enabled = false;
    }

    estado.colliders2DActivos = new bool[estado.colliders2D.Length];
    for (int i = 0; i < estado.colliders2D.Length; i++)
    {
      estado.colliders2DActivos[i] = estado.colliders2D[i].enabled;
      estado.colliders2D[i].enabled = false;
    }

    estado.particulasActivas = new bool[estado.particulas.Length];
    for (int i = 0; i < estado.particulas.Length; i++)
    {
      estado.particulasActivas[i] = estado.particulas[i].isPlaying;
      estado.particulas[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    TrailRenderer[] estelas = heroe.GetComponentsInChildren<TrailRenderer>(true);
    for (int i = 0; i < estelas.Length; i++)
    {
      estelas[i].Clear();
    }

    estadosVisualesEspera[heroe] = estado;
    heroe.gameObject.SetActive(false);
  }

  private void MostrarSiluetaHeroePendiente(Casilla casilla)
  {
    if (heroePendiente == null || heroePendiente.uImage == null || casilla == null)
    {
      return;
    }

    PrepararSiluetaDespliegue(heroePendiente);
    if (imagenSiluetaDespliegue == null)
    {
      return;
    }

    imagenSiluetaDespliegue.transform.position = casilla.transform.position + offsetSiluetaDesdeUnidad;
    imagenSiluetaDespliegue.gameObject.SetActive(true);
    Color color = heroePendiente.uImage.color;
    color.a = AlphaSilueta;
    imagenSiluetaDespliegue.color = color;
  }

  private void OcultarSiluetaActual()
  {
    if (imagenSiluetaDespliegue != null)
    {
      imagenSiluetaDespliegue.gameObject.SetActive(false);
    }
    casillaHover = null;
  }

  private void PrepararSiluetaDespliegue(Unidad heroe)
  {
    if (heroe == null || heroe.uImage == null || heroe.transform.parent == null)
    {
      return;
    }

    if (imagenSiluetaDespliegue == null)
    {
      GameObject goSilueta = new GameObject(
        "BattleIntroHeroSilhouette",
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image));
      imagenSiluetaDespliegue = goSilueta.GetComponent<Image>();
      imagenSiluetaDespliegue.raycastTarget = false;
    }

    Outline contornoExistente = imagenSiluetaDespliegue.GetComponent<Outline>();
    if (contornoExistente != null)
    {
      contornoExistente.enabled = false;
      Destroy(contornoExistente);
    }

    RectTransform rect = imagenSiluetaDespliegue.rectTransform;
    rect.SetParent(heroe.transform.parent, false);
    RectTransform rectFuente = heroe.uImage.rectTransform;
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = rectFuente.pivot;
    rect.sizeDelta = rectFuente.rect.size;
    Vector3 escalaFuenteMundo = rectFuente.lossyScale;
    Vector3 escalaPadreMundo = rect.parent.lossyScale;
    rect.localScale = new Vector3(
      DividirEscalaSegura(escalaFuenteMundo.x, escalaPadreMundo.x, rectFuente.localScale.x),
      DividirEscalaSegura(escalaFuenteMundo.y, escalaPadreMundo.y, rectFuente.localScale.y),
      DividirEscalaSegura(escalaFuenteMundo.z, escalaPadreMundo.z, rectFuente.localScale.z));
    rect.rotation = rectFuente.rotation;
    offsetSiluetaDesdeUnidad = rectFuente.position - heroe.transform.position;

    imagenSiluetaDespliegue.sprite = heroe.uImage.sprite;
    imagenSiluetaDespliegue.type = heroe.uImage.type;
    imagenSiluetaDespliegue.preserveAspect = heroe.uImage.preserveAspect;
    imagenSiluetaDespliegue.transform.SetAsLastSibling();
  }

  private static float DividirEscalaSegura(float escalaMundo, float escalaPadre, float fallback)
  {
    return Mathf.Abs(escalaPadre) > 0.0001f ? escalaMundo / escalaPadre : fallback;
  }

  private void RestaurarVisualHeroe(Unidad heroe)
  {
    if (heroe == null || !estadosVisualesEspera.TryGetValue(heroe, out EstadoVisualEspera estado))
    {
      return;
    }

    for (int i = 0; i < estado.graficos.Length; i++)
    {
      if (estado.graficos[i] == null) { continue; }
      estado.graficos[i].enabled = estado.graficosActivos[i];
      estado.graficos[i].raycastTarget = estado.raycastTargets[i];
      estado.graficos[i].color = estado.colores[i];
    }
    for (int i = 0; i < estado.colliders3D.Length; i++)
    {
      if (estado.colliders3D[i] != null) { estado.colliders3D[i].enabled = estado.colliders3DActivos[i]; }
    }
    for (int i = 0; i < estado.colliders2D.Length; i++)
    {
      if (estado.colliders2D[i] != null) { estado.colliders2D[i].enabled = estado.colliders2DActivos[i]; }
    }

    heroe.gameObject.SetActive(estado.gameObjectActivo);
    for (int i = 0; i < estado.particulas.Length; i++)
    {
      if (estado.particulasActivas[i] && estado.particulas[i] != null)
      {
        estado.particulas[i].Play(true);
      }
    }

    estadosVisualesEspera.Remove(heroe);
  }

  private void RestaurarTodosLosVisualesEspera()
  {
    List<Unidad> pendientes = new List<Unidad>(estadosVisualesEspera.Keys);
    for (int i = 0; i < pendientes.Count; i++)
    {
      RestaurarVisualHeroe(pendientes[i]);
    }
  }

  private void AsignarHeroeACasilla(Unidad heroe, Casilla casilla)
  {
    if (heroe == null || casilla == null || casilla.Presente != null)
    {
      return;
    }

    casilla.NuevoObjetoPresenteEnCasilla(heroe.gameObject);
    heroe.CasillaPosicion = casilla;
    battleManager.AplicarTamanioUnidadBatalla(heroe);
  }

  private void RestaurarHeroesSinCasilla()
  {
    if (casillasOriginalesHeroes.Count < 1)
    {
      return;
    }

    foreach (KeyValuePair<Unidad, Casilla> par in casillasOriginalesHeroes)
    {
      Unidad heroe = par.Key;
      if (heroe == null || heroe.CasillaPosicion != null)
      {
        continue;
      }

      Casilla destino = par.Value;
      if (!EsCasillaValidaDespliegue(destino))
      {
        List<Casilla> disponibles = ObtenerCasillasValidasDespliegue();
        destino = disponibles.Count > 0 ? disponibles[0] : null;
      }

      if (destino != null)
      {
        AsignarHeroeACasilla(heroe, destino);
        heroe.transform.position = destino.transform.position;
        heroe.GetComponent<UnidadPoseController>()?.OnStopMove();
      }
    }

    casillasOriginalesHeroes.Clear();
  }

  private void AsegurarPosicionesAutomaticas()
  {
    for (int i = 0; i < unidadesAutomaticasPreparadas.Count; i++)
    {
      Unidad unidad = unidadesAutomaticasPreparadas[i];
      if (unidad == null || unidad.CasillaPosicion == null)
      {
        continue;
      }

      unidad.transform.position = unidad.CasillaPosicion.transform.position;
      unidad.GetComponent<UnidadPoseController>()?.OnStopMove();
    }
    unidadesAutomaticasPreparadas.Clear();
  }

  private void AjustarLlegadasRestantes(List<LlegadaPendiente> llegadas, int desde)
  {
    for (int i = desde; i < llegadas.Count; i++)
    {
      Unidad unidad = llegadas[i].unidad;
      if (unidad == null || unidad.CasillaPosicion == null)
      {
        continue;
      }

      unidad.transform.position = unidad.CasillaPosicion.transform.position;
      unidad.GetComponent<UnidadPoseController>()?.OnStopMove();
    }
  }

  private void RefrescarCasillasValidasResaltadas()
  {
    LimpiarResaltadoCasillas();
    List<Casilla> validas = ObtenerCasillasValidasDespliegue();
    for (int i = 0; i < validas.Count; i++)
    {
      validas[i].ActivarCapaColorAzul();
      casillasResaltadas.Add(validas[i]);
    }
  }

  private void LimpiarResaltadoCasillas()
  {
    for (int i = 0; i < casillasResaltadas.Count; i++)
    {
      if (casillasResaltadas[i] != null)
      {
        casillasResaltadas[i].DesactivarCapaColorAzul();
      }
    }
    casillasResaltadas.Clear();
  }

  private List<Casilla> ObtenerCasillasValidasDespliegue()
  {
    List<Casilla> validas = new List<Casilla>();
    if (battleManager == null || battleManager.ladoB == null)
    {
      return validas;
    }

    battleManager.ladoB.ActualizarListaDeCasillasEnLado();
    for (int i = 0; i < battleManager.ladoB.casillasLado.Count; i++)
    {
      Casilla casilla = battleManager.ladoB.casillasLado[i];
      if (EsCasillaValidaDespliegue(casilla))
      {
        validas.Add(casilla);
      }
    }
    return validas;
  }

  private bool EsCasillaValidaDespliegue(Casilla casilla)
  {
    return casilla != null
      && battleManager != null
      && battleManager.ladoB != null
      && casilla.ladoGO == battleManager.ladoB.gameObject
      && casilla.Presente == null
      && casilla.GetComponent<Trampa>() == null;
  }

  private void MostrarTextoSaltarIntro()
  {
    PosicionarTextoIntroAbajo();
    MostrarTextoIntro(ObtenerTextoIdioma(
      "ESPACIO: Saltar intro",
      "SPACE: Skip intro",
      "ESPAÇO: Pular introdução"));
  }

  private void MostrarTextoUbicarHeroe(Unidad heroe)
  {
    OcultarTextoIntro();
    string nombre = heroe != null ? heroe.uNombre : string.Empty;
    if (TRADU.i != null && !string.IsNullOrWhiteSpace(nombre))
    {
      nombre = TRADU.i.Traducir(nombre);
    }

    MostrarEtiquetaDespliegue(nombre);
  }

  private void MostrarEtiquetaDespliegue(string nombre)
  {
    if (battleManager == null || battleManager.txtSeleccionaobj == null)
    {
      return;
    }

    if (panelDespliegueManual == null || etiquetaDespliegueManual == null)
    {
      RectTransform canvas = battleManager.txtSeleccionaobj.transform.parent as RectTransform;
      if (canvas == null)
      {
        return;
      }

      panelDespliegueManual = new GameObject(
        "BattleIntroDeploymentPanel",
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image));
      RectTransform panelRect = panelDespliegueManual.GetComponent<RectTransform>();
      panelRect.SetParent(canvas, false);
      panelRect.anchorMin = new Vector2(0.5f, 0f);
      panelRect.anchorMax = new Vector2(0.5f, 0f);
      panelRect.pivot = new Vector2(0.5f, 0f);
      panelRect.anchoredPosition = new Vector2(0f, 34f);
      panelRect.sizeDelta = new Vector2(540f, 76f);

      Image fondo = panelDespliegueManual.GetComponent<Image>();
      fondo.color = new Color(0f, 0f, 0f, 0.68f);
      fondo.raycastTarget = false;

      GameObject goEtiqueta = new GameObject(
        "BattleIntroDeploymentLabel",
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(TextMeshProUGUI));
      RectTransform rect = goEtiqueta.GetComponent<RectTransform>();
      rect.SetParent(panelRect, false);
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.offsetMin = new Vector2(22f, 8f);
      rect.offsetMax = new Vector2(-22f, -8f);

      etiquetaDespliegueManual = goEtiqueta.GetComponent<TextMeshProUGUI>();
      TMP_FontAsset cardo = Resources.Load<TMP_FontAsset>("Fuentes/Cardo/Cardo-Regular SDF");
      if (cardo != null)
      {
        etiquetaDespliegueManual.font = cardo;
      }
      else
      {
        TextMeshProUGUI referencia = battleManager.txtSeleccionaobj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (referencia != null)
        {
          etiquetaDespliegueManual.font = referencia.font;
        }
      }
      etiquetaDespliegueManual.fontSize = 32f;
      etiquetaDespliegueManual.enableAutoSizing = true;
      etiquetaDespliegueManual.fontSizeMin = 22f;
      etiquetaDespliegueManual.fontSizeMax = 34f;
      etiquetaDespliegueManual.alignment = TextAlignmentOptions.Center;
      etiquetaDespliegueManual.color = new Color(0.38f, 0.72f, 1f, 1f);
      etiquetaDespliegueManual.outlineColor = new Color(0f, 0f, 0f, 0.9f);
      etiquetaDespliegueManual.outlineWidth = 0.24f;
      etiquetaDespliegueManual.raycastTarget = false;
      etiquetaDespliegueManual.richText = true;
    }

    string instruccion = ObtenerTextoIdioma("UBICA A", "PLACE", "POSICIONE");
    etiquetaDespliegueManual.text = $"<size=76%>{instruccion}</size>  <b>{nombre}</b>";
    panelDespliegueManual.SetActive(true);
    panelDespliegueManual.transform.SetAsLastSibling();
  }

  private void OcultarEtiquetaDespliegue()
  {
    if (panelDespliegueManual != null)
    {
      panelDespliegueManual.SetActive(false);
    }
  }

  private void MostrarMensajeEmboscadaJugador()
  {
    if (battleManager == null || battleManager.txtSeleccionaobj == null)
    {
      return;
    }

    if (panelMensajeEmboscada == null || textoMensajeEmboscada == null)
    {
      RectTransform canvas = battleManager.txtSeleccionaobj.transform.parent as RectTransform;
      if (canvas == null)
      {
        return;
      }

      panelMensajeEmboscada = new GameObject(
        "BattleIntroAmbushMessagePanel",
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image));
      RectTransform panelRect = panelMensajeEmboscada.GetComponent<RectTransform>();
      panelRect.SetParent(canvas, false);
      panelRect.anchorMin = new Vector2(0.5f, 1f);
      panelRect.anchorMax = new Vector2(0.5f, 1f);
      panelRect.pivot = new Vector2(0.5f, 1f);
      panelRect.anchoredPosition = new Vector2(0f, -28f);
      panelRect.sizeDelta = new Vector2(780f, 66f);

      Image fondo = panelMensajeEmboscada.GetComponent<Image>();
      fondo.color = new Color(0f, 0f, 0f, 0.62f);
      fondo.raycastTarget = false;

      GameObject goTexto = new GameObject(
        "BattleIntroAmbushMessage",
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(TextMeshProUGUI));
      RectTransform rect = goTexto.GetComponent<RectTransform>();
      rect.SetParent(panelRect, false);
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.offsetMin = new Vector2(20f, 7f);
      rect.offsetMax = new Vector2(-20f, -7f);

      textoMensajeEmboscada = goTexto.GetComponent<TextMeshProUGUI>();
      TMP_FontAsset cinzel = Resources.Load<TMP_FontAsset>("Fuentes/Cinzel/CinzelDecorative-Regular SDF");
      if (cinzel != null)
      {
        textoMensajeEmboscada.font = cinzel;
      }
      else
      {
        TextMeshProUGUI referencia = battleManager.txtSeleccionaobj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (referencia != null)
        {
          textoMensajeEmboscada.font = referencia.font;
        }
      }
      textoMensajeEmboscada.fontSize = 29f;
      textoMensajeEmboscada.enableAutoSizing = true;
      textoMensajeEmboscada.fontSizeMin = 21f;
      textoMensajeEmboscada.fontSizeMax = 31f;
      textoMensajeEmboscada.alignment = TextAlignmentOptions.Center;
      textoMensajeEmboscada.color = new Color(1f, 0.82f, 0.32f, 1f);
      textoMensajeEmboscada.outlineColor = new Color(0f, 0f, 0f, 0.9f);
      textoMensajeEmboscada.outlineWidth = 0.18f;
      textoMensajeEmboscada.raycastTarget = false;
    }

    textoMensajeEmboscada.text = ObtenerTextoIdioma(
      "Has emboscado al enemigo",
      "You have ambushed the enemy",
      "Voc\u00EA emboscou o inimigo");
    panelMensajeEmboscada.SetActive(true);
    panelMensajeEmboscada.transform.SetAsLastSibling();
  }

  private void OcultarMensajeEmboscadaJugador()
  {
    if (panelMensajeEmboscada != null)
    {
      panelMensajeEmboscada.SetActive(false);
    }
  }

  public void PrepararTextoComienzaBatalla()
  {
    textoComienzaBatallaMostrado = false;
    textoComienzaBatalla = null;
    if (battleManager == null || battleManager.txtSeleccionaobj == null)
    {
      return;
    }

    Transform canvas = battleManager.txtSeleccionaobj.transform.parent;
    Transform encontrado = canvas != null ? canvas.Find("txtBattleStart") : null;
    if (encontrado != null)
    {
      textoComienzaBatalla = encontrado.gameObject;
      textoComienzaBatalla.SetActive(false);
    }
  }

  private void MostrarTextoComienzaBatalla()
  {
    if (textoComienzaBatalla == null)
    {
      return;
    }

    TextMeshProUGUI tmp = textoComienzaBatalla.GetComponent<TextMeshProUGUI>();
    if (tmp == null)
    {
      tmp = textoComienzaBatalla.GetComponentInChildren<TextMeshProUGUI>(true);
    }
    if (tmp != null)
    {
      tmp.text = ObtenerTextoIdioma(
        "\u00A1Comienza la batalla!",
        "The battle begins!",
        "A batalha come\u00E7a!");
    }

    textoComienzaBatalla.SetActive(false);
    textoComienzaBatalla.transform.SetAsLastSibling();
    textoComienzaBatalla.SetActive(true);
    textoComienzaBatallaMostrado = true;
  }

  private void MostrarTextoIntro(string texto)
  {
    if (battleManager == null || battleManager.txtSeleccionaobj == null)
    {
      return;
    }

    TextMeshProUGUI tmp = battleManager.txtSeleccionaobj.GetComponentInChildren<TextMeshProUGUI>(true);
    if (tmp != null)
    {
      tmp.text = texto;
    }
    battleManager.txtSeleccionaobj.SetActive(true);
  }

  private void OcultarTextoIntro()
  {
    if (battleManager != null && battleManager.txtSeleccionaobj != null)
    {
      battleManager.txtSeleccionaobj.SetActive(false);
    }
    RestaurarPosicionTextoIntro();
  }

  private void PosicionarTextoIntroAbajo()
  {
    if (battleManager == null || battleManager.txtSeleccionaobj == null)
    {
      return;
    }

    rectTextoIntro = battleManager.txtSeleccionaobj.GetComponent<RectTransform>();
    if (rectTextoIntro == null)
    {
      return;
    }

    if (!posicionTextoIntroGuardada)
    {
      anchorMinTextoIntroOriginal = rectTextoIntro.anchorMin;
      anchorMaxTextoIntroOriginal = rectTextoIntro.anchorMax;
      pivotTextoIntroOriginal = rectTextoIntro.pivot;
      posicionTextoIntroOriginal = rectTextoIntro.anchoredPosition;
      posicionTextoIntroGuardada = true;
    }

    rectTextoIntro.anchorMin = new Vector2(0.5f, 0f);
    rectTextoIntro.anchorMax = new Vector2(0.5f, 0f);
    rectTextoIntro.pivot = new Vector2(0.5f, 0.5f);
    rectTextoIntro.anchoredPosition = new Vector2(0f, 65f);
  }

  private void RestaurarPosicionTextoIntro()
  {
    if (!posicionTextoIntroGuardada || rectTextoIntro == null)
    {
      return;
    }

    rectTextoIntro.anchorMin = anchorMinTextoIntroOriginal;
    rectTextoIntro.anchorMax = anchorMaxTextoIntroOriginal;
    rectTextoIntro.pivot = pivotTextoIntroOriginal;
    rectTextoIntro.anchoredPosition = posicionTextoIntroOriginal;
    posicionTextoIntroGuardada = false;
  }

  private void MostrarVinetaIntro()
  {
    if (imagenVinetaIntro == null)
    {
      CrearVinetaIntro();
    }

    if (imagenVinetaIntro == null)
    {
      return;
    }

    if (battleManager != null
      && battleManager.txtSeleccionaobj != null
      && !indiceHermanoTextoIntroGuardado)
    {
      indiceHermanoTextoIntroOriginal = battleManager.txtSeleccionaobj.transform.GetSiblingIndex();
      indiceHermanoTextoIntroGuardado = true;
    }

    imagenVinetaIntro.color = Color.white;
    imagenVinetaIntro.gameObject.SetActive(true);
    imagenVinetaIntro.transform.SetAsLastSibling();
    if (battleManager != null && battleManager.txtSeleccionaobj != null)
    {
      battleManager.txtSeleccionaobj.transform.SetAsLastSibling();
    }
  }

  private void OcultarVinetaIntro()
  {
    if (imagenVinetaIntro != null)
    {
      imagenVinetaIntro.gameObject.SetActive(false);
      imagenVinetaIntro.color = Color.white;
    }

    if (indiceHermanoTextoIntroGuardado
      && battleManager != null
      && battleManager.txtSeleccionaobj != null
      && battleManager.txtSeleccionaobj.transform.parent != null)
    {
      int ultimoIndice = battleManager.txtSeleccionaobj.transform.parent.childCount - 1;
      battleManager.txtSeleccionaobj.transform.SetSiblingIndex(
        Mathf.Clamp(indiceHermanoTextoIntroOriginal, 0, ultimoIndice));
    }
    indiceHermanoTextoIntroGuardado = false;
  }

  private async Task DesvanecerVinetaAsync(float duracion)
  {
    if (imagenVinetaIntro == null || !imagenVinetaIntro.gameObject.activeSelf)
    {
      return;
    }

    float transcurrido = 0f;
    while (transcurrido < duracion && !cancelacionSolicitada)
    {
      await Task.Yield();
      transcurrido += Time.unscaledDeltaTime;
      float t = Mathf.Clamp01(transcurrido / duracion);
      Color color = imagenVinetaIntro.color;
      color.a = 1f - Mathf.SmoothStep(0f, 1f, t);
      imagenVinetaIntro.color = color;
    }

    OcultarVinetaIntro();
  }

  private void CrearVinetaIntro()
  {
    if (battleManager == null || battleManager.txtSeleccionaobj == null)
    {
      return;
    }

    RectTransform canvas = battleManager.txtSeleccionaobj.transform.parent as RectTransform;
    if (canvas == null)
    {
      return;
    }

    const int tamanio = 64;
    texturaVinetaIntro = new Texture2D(tamanio, tamanio, TextureFormat.RGBA32, false)
    {
      name = "BattleIntroVignetteTexture",
      filterMode = FilterMode.Bilinear,
      wrapMode = TextureWrapMode.Clamp
    };

    Color[] pixeles = new Color[tamanio * tamanio];
    for (int y = 0; y < tamanio; y++)
    {
      for (int x = 0; x < tamanio; x++)
      {
        float nx = ((float)x / (tamanio - 1) - 0.5f) * 2f;
        float ny = ((float)y / (tamanio - 1) - 0.5f) * 2f;
        float distancia = Mathf.Sqrt(nx * nx + ny * ny);
        float alpha = Mathf.SmoothStep(0f, AlphaMaxVineta, Mathf.InverseLerp(0.48f, 1.15f, distancia));
        pixeles[y * tamanio + x] = new Color(0f, 0f, 0f, alpha);
      }
    }
    texturaVinetaIntro.SetPixels(pixeles);
    texturaVinetaIntro.Apply(false, true);

    spriteVinetaIntro = Sprite.Create(
      texturaVinetaIntro,
      new Rect(0f, 0f, tamanio, tamanio),
      new Vector2(0.5f, 0.5f),
      100f);
    spriteVinetaIntro.name = "BattleIntroVignetteSprite";

    GameObject goVineta = new GameObject("BattleIntroVignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    RectTransform rect = goVineta.GetComponent<RectTransform>();
    rect.SetParent(canvas, false);
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;

    imagenVinetaIntro = goVineta.GetComponent<Image>();
    imagenVinetaIntro.sprite = spriteVinetaIntro;
    imagenVinetaIntro.color = Color.white;
    imagenVinetaIntro.raycastTarget = false;
    imagenVinetaIntro.preserveAspect = false;
  }

  private void OnDestroy()
  {
    if (imagenSiluetaDespliegue != null)
    {
      Destroy(imagenSiluetaDespliegue.gameObject);
    }
    if (panelDespliegueManual != null)
    {
      Destroy(panelDespliegueManual);
    }
    if (panelMensajeEmboscada != null)
    {
      Destroy(panelMensajeEmboscada);
    }
    if (imagenVinetaIntro != null)
    {
      Destroy(imagenVinetaIntro.gameObject);
    }
    if (spriteVinetaIntro != null)
    {
      Destroy(spriteVinetaIntro);
    }
    if (texturaVinetaIntro != null)
    {
      Destroy(texturaVinetaIntro);
    }
  }

  private static string ObtenerTextoIdioma(string espanol, string ingles, string portugues)
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
    if (idioma == 2) { return ingles; }
    if (idioma == 3) { return portugues; }
    return espanol;
  }

  private static Vector3 PosicionInicialLlegada(Casilla casilla, int lado)
  {
    float offset = lado == 2 ? OffsetLlegada : -OffsetLlegada;
    return casilla.transform.position + new Vector3(offset, 0f, 0f);
  }

  private static List<Unidad> FiltrarUnidades(IReadOnlyList<Unidad> unidades)
  {
    List<Unidad> resultado = new List<Unidad>();
    if (unidades == null)
    {
      return resultado;
    }

    for (int i = 0; i < unidades.Count; i++)
    {
      Unidad unidad = unidades[i];
      if (unidad != null && unidad.gameObject.activeInHierarchy && !resultado.Contains(unidad))
      {
        resultado.Add(unidad);
      }
    }
    return resultado;
  }

  private void AplicarEscalaInicialUnidades(List<Unidad> unidades)
  {
    if (battleManager == null)
    {
      return;
    }

    for (int i = 0; i < unidades.Count; i++)
    {
      battleManager.AplicarTamanioUnidadBatalla(unidades[i]);
    }
  }

  private static List<LlegadaPendiente> CrearLlegadas(List<Unidad> unidades, int lado)
  {
    List<LlegadaPendiente> resultado = new List<LlegadaPendiente>();
    for (int i = 0; i < unidades.Count; i++)
    {
      resultado.Add(new LlegadaPendiente(unidades[i], lado));
    }
    return resultado;
  }

  private static bool HayEnemigosMovilesParaIntro(List<Unidad> enemigos)
  {
    for (int i = 0; i < enemigos.Count; i++)
    {
      Unidad enemigo = enemigos[i];
      if (enemigo != null && !enemigo.esInmobil && enemigo.CasillaPosicion != null)
      {
        return true;
      }
    }
    return false;
  }
}
