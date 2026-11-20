using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel "Accesos rápidos" del menu de opciones: lista de acciones de campaña, batalla y generales con dos teclas
// reasignables por accion. Reemplaza al texto estatico que listaba las teclas. Se construye por codigo dentro de
// PanelControles (ver OpcionesCargarPlayerPrefsUI). Escape, el mouse y las teclas de debug no son reasignables.
public class HotkeysPanelUI : MonoBehaviour
{
  private const string NombreContenedor = "PanelRemapeoTeclas";
  private const float AltoFila = 22f;
  private const float AltoEncabezado = 22f;
  private const float AnchoTecla = 62f;
  private const float AltoNota = 28f;
  private const float AltoPie = 30f;
  private const float ConfirmarSegundos = 3f;

  private static readonly Color ColorTexto = new Color(0.86f, 0.86f, 0.86f, 1f);
  private static readonly Color ColorTenue = new Color(0.62f, 0.62f, 0.62f, 1f);
  private static readonly Color ColorModificado = new Color(1f, 0.82f, 0.45f, 1f);
  private static readonly Color ColorCaptura = new Color(1f, 0.78f, 0.35f, 1f);
  private static readonly Color ColorError = new Color(1f, 0.55f, 0.45f, 1f);
  private static readonly Color ColorFila = new Color(0.03f, 0.025f, 0.025f, 0.45f);

  private sealed class FilaTecla
  {
    public HotkeyAction accion;
    public TextMeshProUGUI etiqueta;
    public Button[] botones = new Button[2];
    public TextMeshProUGUI[] textos = new TextMeshProUGUI[2];
  }

  private sealed class Encabezado
  {
    public TextMeshProUGUI texto;
    public HotkeyContext contexto;
  }

  private OpcionesCargarPlayerPrefsUI opciones;
  private TMP_FontAsset fuente;
  private Material materialFuente;
  private Sprite spriteBoton;
  private ColorBlock coloresBoton;
  private AudioClip clipClick;
  private UnityEngine.Audio.AudioMixerGroup grupoAudio;
  private float volumenClick = 1f;

  private readonly List<FilaTecla> filas = new List<FilaTecla>();
  private readonly List<Encabezado> encabezados = new List<Encabezado>();
  private RectTransform contenido;
  private TextMeshProUGUI textoNota;
  private TextMeshProUGUI textoEstado;
  private Button botonRestaurar;
  private TextMeshProUGUI textoRestaurar;
  private ScrollRect scroll;
  private Rect areaUtil;

  private FilaTecla capturaFila;
  private int capturaSlot = -1;
  private bool restaurarArmado;
  private float restaurarHasta;

  public static HotkeysPanelUI Asegurar(OpcionesCargarPlayerPrefsUI opciones, Button plantillaBoton)
  {
    if (opciones == null || opciones.PanelControles == null || plantillaBoton == null)
    {
      return null;
    }

    Transform existente = opciones.PanelControles.transform.Find(NombreContenedor);
    if (existente != null)
    {
      return existente.GetComponent<HotkeysPanelUI>();
    }

    GameObject objeto = new GameObject(NombreContenedor, typeof(RectTransform));
    objeto.layer = opciones.PanelControles.layer;
    RectTransform rect = (RectTransform)objeto.transform;
    rect.SetParent(opciones.PanelControles.transform, false);
    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = Vector2.zero;
    rect.sizeDelta = ((RectTransform)opciones.PanelControles.transform).sizeDelta;

    HotkeysPanelUI panel = objeto.AddComponent<HotkeysPanelUI>();
    panel.Construir(opciones, plantillaBoton);
    return panel;
  }

  // ---------------------------------------------------------------- construccion

  private void Construir(OpcionesCargarPlayerPrefsUI opcionesPadre, Button plantillaBoton)
  {
    opciones = opcionesPadre;

    Image imagenPlantilla = plantillaBoton.GetComponent<Image>();
    spriteBoton = imagenPlantilla != null ? imagenPlantilla.sprite : null;
    coloresBoton = plantillaBoton.colors;
    TextMeshProUGUI textoPlantilla = plantillaBoton.GetComponentInChildren<TextMeshProUGUI>(true);
    if (textoPlantilla != null)
    {
      fuente = textoPlantilla.font;
      materialFuente = textoPlantilla.fontSharedMaterial;
    }
    AudioSource audioPlantilla = plantillaBoton.GetComponent<AudioSource>();
    if (audioPlantilla != null)
    {
      clipClick = audioPlantilla.clip;
      grupoAudio = audioPlantilla.outputAudioMixerGroup;
      volumenClick = audioPlantilla.volume;
    }

    CalcularArea();

    // Nota superior
    textoNota = CrearTexto(transform, "txtNota", 10f, TextAlignmentOptions.Center, ColorTenue);
    textoNota.raycastTarget = false;
    textoNota.textWrappingMode = TextWrappingModes.Normal;
    RectTransform rectNota = textoNota.rectTransform;
    rectNota.anchorMin = rectNota.anchorMax = new Vector2(0.5f, 0.5f);
    rectNota.pivot = new Vector2(0.5f, 1f);
    rectNota.sizeDelta = new Vector2(areaUtil.width, AltoNota);
    rectNota.anchoredPosition = new Vector2(areaUtil.center.x, areaUtil.yMax);

    // Viewport con scroll
    float altoViewport = areaUtil.height - AltoNota - AltoPie - 4f;
    GameObject objViewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
    objViewport.layer = gameObject.layer;
    RectTransform viewport = (RectTransform)objViewport.transform;
    viewport.SetParent(transform, false);
    viewport.anchorMin = viewport.anchorMax = new Vector2(0.5f, 0.5f);
    viewport.pivot = new Vector2(0.5f, 1f);
    viewport.sizeDelta = new Vector2(areaUtil.width - 10f, altoViewport);
    viewport.anchoredPosition = new Vector2(areaUtil.center.x - 5f, areaUtil.yMax - AltoNota);

    GameObject objContenido = new GameObject("Contenido", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
    objContenido.layer = gameObject.layer;
    contenido = (RectTransform)objContenido.transform;
    contenido.SetParent(viewport, false);
    contenido.anchorMin = new Vector2(0f, 1f);
    contenido.anchorMax = new Vector2(1f, 1f);
    contenido.pivot = new Vector2(0.5f, 1f);
    contenido.anchoredPosition = Vector2.zero;
    contenido.sizeDelta = Vector2.zero;
    VerticalLayoutGroup layout = objContenido.GetComponent<VerticalLayoutGroup>();
    layout.spacing = 2f;
    layout.childControlWidth = true;
    layout.childControlHeight = true;
    layout.childForceExpandWidth = true;
    layout.childForceExpandHeight = false;
    objContenido.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

    scroll = viewport.gameObject.AddComponent<ScrollRect>();
    scroll.viewport = viewport;
    scroll.content = contenido;
    scroll.horizontal = false;
    scroll.vertical = true;
    scroll.movementType = ScrollRect.MovementType.Clamped;
    scroll.scrollSensitivity = 18f;
    scroll.inertia = false;

    CrearBarra(viewport);

    // Contenido
    AgregarEncabezado(HotkeyContext.Campaign);
    foreach (HotkeyAction accion in Hotkeys.Actions)
    {
      if (Hotkeys.ContextOf(accion) == HotkeyContext.Campaign) { AgregarFila(accion); }
    }
    AgregarEncabezado(HotkeyContext.Battle);
    foreach (HotkeyAction accion in Hotkeys.Actions)
    {
      if (Hotkeys.ContextOf(accion) == HotkeyContext.Battle) { AgregarFila(accion); }
    }
    AgregarEncabezado(HotkeyContext.General);
    foreach (HotkeyAction accion in Hotkeys.Actions)
    {
      if (Hotkeys.ContextOf(accion) == HotkeyContext.General) { AgregarFila(accion); }
    }

    // Pie: restaurar + estado
    botonRestaurar = CrearBoton(transform, "bt_RestaurarTeclas", 120f, 22f, out textoRestaurar);
    RectTransform rectBoton = (RectTransform)botonRestaurar.transform;
    rectBoton.anchorMin = rectBoton.anchorMax = new Vector2(0.5f, 0.5f);
    rectBoton.pivot = new Vector2(1f, 0f);
    rectBoton.anchoredPosition = new Vector2(areaUtil.xMax - 6f, areaUtil.yMin);
    botonRestaurar.onClick.AddListener(OnRestaurar);

    textoEstado = CrearTexto(transform, "txtEstado", 10f, TextAlignmentOptions.MidlineLeft, ColorTenue);
    textoEstado.raycastTarget = false;
    RectTransform rectEstado = textoEstado.rectTransform;
    rectEstado.anchorMin = rectEstado.anchorMax = new Vector2(0.5f, 0.5f);
    rectEstado.pivot = new Vector2(0f, 0f);
    rectEstado.sizeDelta = new Vector2(areaUtil.width - 136f, 22f);
    rectEstado.anchoredPosition = new Vector2(areaUtil.xMin + 6f, areaUtil.yMin);

    Refrescar();
  }

  private void CalcularArea()
  {
    RectTransform self = (RectTransform)transform;
    Rect util = self.rect;

    RectTransform fondo = opciones.transform.Find("Fondo") as RectTransform;
    if (fondo != null)
    {
      Rect marco = RectEnLocal(self, fondo);
      float insetX = marco.width * 0.06f;
      float insetY = marco.height * 0.055f;
      util = Rect.MinMaxRect(marco.xMin + marco.width * 0.33f, marco.yMin + insetY, marco.xMax - insetX, marco.yMax - marco.height * 0.07f);
    }

    areaUtil = util;
  }

  private static Rect RectEnLocal(RectTransform destino, RectTransform origen)
  {
    Vector3[] esquinas = new Vector3[4];
    origen.GetWorldCorners(esquinas);
    Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
    Vector2 max = new Vector2(float.MinValue, float.MinValue);
    for (int i = 0; i < 4; i++)
    {
      Vector3 local = destino.InverseTransformPoint(esquinas[i]);
      min = Vector2.Min(min, local);
      max = Vector2.Max(max, local);
    }
    return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
  }

  private void CrearBarra(RectTransform viewport)
  {
    GameObject objPista = new GameObject("Barra", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
    objPista.layer = gameObject.layer;
    RectTransform pista = (RectTransform)objPista.transform;
    pista.SetParent(viewport, false);
    pista.anchorMin = new Vector2(1f, 0f);
    pista.anchorMax = new Vector2(1f, 1f);
    pista.pivot = new Vector2(1f, 0.5f);
    pista.sizeDelta = new Vector2(5f, 0f);
    pista.anchoredPosition = Vector2.zero;
    objPista.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);

    GameObject objAgarre = new GameObject("Agarre", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    objAgarre.layer = gameObject.layer;
    RectTransform agarre = (RectTransform)objAgarre.transform;
    agarre.SetParent(pista, false);
    agarre.anchorMin = Vector2.zero;
    agarre.anchorMax = Vector2.one;
    agarre.offsetMin = Vector2.zero;
    agarre.offsetMax = Vector2.zero;
    Image imagenAgarre = objAgarre.GetComponent<Image>();
    imagenAgarre.color = new Color(0.62f, 0.55f, 0.42f, 0.9f);

    Scrollbar barra = objPista.GetComponent<Scrollbar>();
    barra.handleRect = agarre;
    barra.targetGraphic = imagenAgarre;
    barra.direction = Scrollbar.Direction.BottomToTop;

    scroll.verticalScrollbar = barra;
    scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
    scroll.verticalScrollbarSpacing = 0f;
  }

  private void AgregarEncabezado(HotkeyContext contexto)
  {
    GameObject objeto = new GameObject("Encabezado", typeof(RectTransform), typeof(LayoutElement));
    objeto.layer = gameObject.layer;
    RectTransform rect = (RectTransform)objeto.transform;
    rect.SetParent(contenido, false);
    LayoutElement elemento = objeto.GetComponent<LayoutElement>();
    elemento.preferredHeight = AltoEncabezado;

    TextMeshProUGUI texto = CrearTexto(rect, "txt", 12f, TextAlignmentOptions.BottomLeft, ColorModificado);
    texto.raycastTarget = false;
    texto.fontStyle = FontStyles.Bold;
    Estirar((RectTransform)texto.transform, 6f, 2f);
    encabezados.Add(new Encabezado { texto = texto, contexto = contexto });
  }

  private void AgregarFila(HotkeyAction accion)
  {
    GameObject objeto = new GameObject("Fila_" + accion, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
    objeto.layer = gameObject.layer;
    RectTransform rect = (RectTransform)objeto.transform;
    rect.SetParent(contenido, false);
    objeto.GetComponent<LayoutElement>().preferredHeight = AltoFila;
    Image fondo = objeto.GetComponent<Image>();
    fondo.color = ColorFila;
    fondo.raycastTarget = false;

    FilaTecla fila = new FilaTecla { accion = accion };

    fila.etiqueta = CrearTexto(rect, "txtEtiqueta", 11f, TextAlignmentOptions.MidlineLeft, ColorTexto);
    fila.etiqueta.raycastTarget = false;
    RectTransform rectEtiqueta = fila.etiqueta.rectTransform;
    rectEtiqueta.anchorMin = Vector2.zero;
    rectEtiqueta.anchorMax = Vector2.one;
    rectEtiqueta.offsetMin = new Vector2(8f, 0f);
    rectEtiqueta.offsetMax = new Vector2(-(AnchoTecla * 2f + 22f), 0f);

    for (int slot = 0; slot < 2; slot++)
    {
      Button boton = CrearBoton(rect, "bt_Tecla" + (slot + 1), AnchoTecla, AltoFila - 2f, out fila.textos[slot]);
      RectTransform rectBoton = (RectTransform)boton.transform;
      rectBoton.anchorMin = rectBoton.anchorMax = new Vector2(1f, 0.5f);
      rectBoton.pivot = new Vector2(1f, 0.5f);
      // slot 0 (principal) queda a la izquierda del slot 1 (alternativa)
      rectBoton.anchoredPosition = new Vector2(-(6f + (1 - slot) * (AnchoTecla + 4f)), 0f);
      int slotLocal = slot;
      boton.onClick.AddListener(() => OnClicTecla(fila, slotLocal));
      fila.botones[slot] = boton;
    }

    filas.Add(fila);
  }

  private static void Estirar(RectTransform rect, float margenX, float margenY)
  {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = new Vector2(margenX, margenY);
    rect.offsetMax = new Vector2(-margenX, -margenY);
  }

  private TextMeshProUGUI CrearTexto(Transform padre, string nombre, float tamano, TextAlignmentOptions alineacion, Color color)
  {
    GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
    objeto.layer = padre.gameObject.layer;
    objeto.transform.SetParent(padre, false);
    TextMeshProUGUI texto = objeto.GetComponent<TextMeshProUGUI>();
    if (fuente != null) { texto.font = fuente; }
    if (materialFuente != null) { texto.fontSharedMaterial = materialFuente; }
    texto.alignment = alineacion;
    texto.color = color;
    texto.textWrappingMode = TextWrappingModes.NoWrap;
    float escala = Mathf.Max(0.5f, EscaladoFuentesGlobal.EscalaTextoActual);
    texto.enableAutoSizing = true;
    texto.fontSizeMax = tamano * escala;
    texto.fontSizeMin = Mathf.Max(5f, tamano * 0.55f);
    texto.fontSize = tamano * escala;
    return texto;
  }

  private Button CrearBoton(Transform padre, string nombre, float ancho, float alto, out TextMeshProUGUI texto)
  {
    GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
    objeto.layer = padre.gameObject.layer;
    RectTransform rect = (RectTransform)objeto.transform;
    rect.SetParent(padre, false);
    rect.sizeDelta = new Vector2(ancho, alto);

    Image imagen = objeto.GetComponent<Image>();
    imagen.sprite = spriteBoton;
    imagen.type = Image.Type.Simple;
    imagen.color = Color.white;

    Button boton = objeto.GetComponent<Button>();
    boton.targetGraphic = imagen;
    boton.transition = Selectable.Transition.ColorTint;
    boton.colors = coloresBoton;

    if (clipClick != null)
    {
      AudioSource audio = objeto.AddComponent<AudioSource>();
      audio.clip = clipClick;
      audio.playOnAwake = false;
      audio.spatialBlend = 0f;
      audio.volume = volumenClick;
      audio.outputAudioMixerGroup = grupoAudio;
      boton.onClick.AddListener(audio.Play);
    }

    texto = CrearTexto(rect, "Text (TMP)", 11f, TextAlignmentOptions.Center, ColorTexto);
    texto.raycastTarget = false;
    Estirar(texto.rectTransform, 2f, 1f);
    return boton;
  }

  // ---------------------------------------------------------------- ciclo de vida

  private void OnEnable()
  {
    CancelarCaptura();
    restaurarArmado = false;
    Refrescar();
    if (scroll != null) { scroll.verticalNormalizedPosition = 1f; }
  }

  private void OnDisable()
  {
    CancelarCaptura();
  }

  private void Update()
  {
    if (restaurarArmado && Time.unscaledTime > restaurarHasta)
    {
      restaurarArmado = false;
      ActualizarTextos();
    }

    if (capturaFila == null) { return; }

    if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
    {
      AsignarCaptura(KeyCode.None);
      return;
    }

    KeyCode tecla = Hotkeys.PollPressedKey();
    if (tecla != KeyCode.None)
    {
      AsignarCaptura(tecla);
    }
  }

  // ---------------------------------------------------------------- logica

  private void OnClicTecla(FilaTecla fila, int slot)
  {
    restaurarArmado = false;

    // Clic sobre la tecla que se esta asignando = cancelar.
    if (capturaFila == fila && capturaSlot == slot)
    {
      CancelarCaptura();
      ActualizarTextos();
      MostrarEstado(string.Empty, ColorTenue);
      return;
    }

    CancelarCaptura();
    capturaFila = fila;
    capturaSlot = slot;
    Hotkeys.BeginCapture();
    ActualizarTextos();
    MostrarEstado(T("Pulsa la nueva tecla para «", "Press the new key for “", "Pressione a nova tecla para “") + NombreAccion(fila.accion)
      + T("»  ·  Retroceso/Supr: dejar libre", "”  ·  Backspace/Del: clear", "”  ·  Backspace/Del: limpar"), ColorCaptura);
  }

  private void AsignarCaptura(KeyCode tecla)
  {
    FilaTecla fila = capturaFila;
    int slot = capturaSlot;
    CancelarCaptura();
    if (fila == null) { return; }

    HotkeyAction desplazada;
    bool hubo = Hotkeys.TryAssign(fila.accion, slot, tecla, out desplazada);
    Refrescar();

    if (tecla == KeyCode.None)
    {
      MostrarEstado(T("Tecla liberada.", "Key cleared.", "Tecla liberada."), ColorTenue);
    }
    else if (hubo)
    {
      MostrarEstado(T("«", "“", "“") + Hotkeys.KeyName(tecla) + T("» ya estaba en «", "” was used by “", "” já estava em “")
        + NombreAccion(desplazada) + T("»: se le quitó.", "”: removed from it.", "”: foi removida."), ColorError);
    }
    else
    {
      MostrarEstado(T("Asignada: ", "Assigned: ", "Atribuída: ") + Hotkeys.KeyName(tecla), ColorTenue);
    }
  }

  private void CancelarCaptura()
  {
    if (capturaFila != null)
    {
      capturaFila = null;
      capturaSlot = -1;
      Hotkeys.EndCapture();
    }
  }

  private void OnRestaurar()
  {
    CancelarCaptura();
    if (!restaurarArmado)
    {
      restaurarArmado = true;
      restaurarHasta = Time.unscaledTime + ConfirmarSegundos;
      ActualizarTextos();
      MostrarEstado(T("Pulsa de nuevo para restaurar todas las teclas.", "Press again to restore all keys.", "Pressione de novo para restaurar todas as teclas."), ColorCaptura);
      return;
    }

    restaurarArmado = false;
    Hotkeys.ResetAll();
    Refrescar();
    MostrarEstado(T("Teclas restauradas.", "Keys restored.", "Teclas restauradas."), ColorTenue);
  }

  public void Refrescar()
  {
    if (textoNota == null) { return; }

    textoNota.text = T("Clic en una tecla para cambiarla. Cada acción admite dos teclas. Esc, el mouse y las teclas de depuración son fijos.",
      "Click a key to change it. Each action supports two keys. Esc, the mouse and debug keys are fixed.",
      "Clique em uma tecla para alterá-la. Cada ação aceita duas teclas. Esc, o mouse e as teclas de depuração são fixos.");

    foreach (Encabezado encabezado in encabezados)
    {
      switch (encabezado.contexto)
      {
        case HotkeyContext.Campaign: encabezado.texto.text = T("Campaña", "Campaign", "Campanha"); break;
        case HotkeyContext.Battle: encabezado.texto.text = T("Batalla", "Battle", "Batalha"); break;
        default: encabezado.texto.text = T("General", "General", "Geral"); break;
      }
    }

    foreach (FilaTecla fila in filas)
    {
      fila.etiqueta.text = NombreAccion(fila.accion);
      fila.etiqueta.color = Hotkeys.IsDefault(fila.accion) ? ColorTexto : ColorModificado;
    }

    ActualizarTextos();
  }

  private void ActualizarTextos()
  {
    foreach (FilaTecla fila in filas)
    {
      for (int slot = 0; slot < 2; slot++)
      {
        bool esCaptura = capturaFila == fila && capturaSlot == slot;
        fila.textos[slot].text = esCaptura ? "..." : Hotkeys.KeyName(Hotkeys.GetKey(fila.accion, slot));
        fila.textos[slot].color = esCaptura ? ColorCaptura : ColorTexto;
      }
    }

    if (textoRestaurar != null)
    {
      textoRestaurar.text = restaurarArmado
        ? T("¿Seguro?", "Sure?", "Certeza?")
        : T("Restaurar todo", "Restore all", "Restaurar tudo");
      textoRestaurar.color = restaurarArmado ? ColorCaptura : ColorTexto;
    }
  }

  private void MostrarEstado(string mensaje, Color color)
  {
    if (textoEstado == null) { return; }
    textoEstado.text = mensaje;
    textoEstado.color = color;
  }

  private static string NombreAccion(HotkeyAction accion)
  {
    switch (accion)
    {
      case HotkeyAction.CampRest: return T("Descansar", "Rest", "Descansar");
      case HotkeyAction.CampCharacters: return T("Menú de personajes", "Characters menu", "Menu de personagens");
      case HotkeyAction.CampUpgrades: return T("Menú de mejoras", "Upgrades menu", "Menu de melhorias");
      case HotkeyAction.CampFollowers: return T("Menú de seguidores", "Followers menu", "Menu de seguidores");
      case HotkeyAction.CampQuickSave: return T("Guardado rápido", "Quick save", "Salvamento rápido");
      case HotkeyAction.CampQuickLoad: return T("Carga rápida", "Quick load", "Carregamento rápido");
      case HotkeyAction.BattleEndTurn: return T("Terminar turno / confirmar", "End turn / confirm", "Encerrar turno / confirmar");
      case HotkeyAction.BattleTacticalView: return T("Vista táctica", "Tactical view", "Visão tática");
      case HotkeyAction.BattleEnemyInfo: return T("Información de enemigos", "Enemy info", "Informação dos inimigos");
      case HotkeyAction.BattleFastMode: return T("Modo rápido", "Fast mode", "Modo rápido");
      case HotkeyAction.BattlePreviewHostile: return T("Ver alcance enemigo (mantener)", "Show enemy range (hold)", "Ver alcance inimigo (segurar)");
      case HotkeyAction.Handbook: return T("Manual", "Handbook", "Manual");
      case HotkeyAction.CameraLeft: return T("Cámara: izquierda", "Camera: left", "Câmera: esquerda");
      case HotkeyAction.CameraRight: return T("Cámara: derecha", "Camera: right", "Câmera: direita");
      case HotkeyAction.CameraUp: return T("Cámara: arriba", "Camera: up", "Câmera: cima");
      case HotkeyAction.CameraDown: return T("Cámara: abajo", "Camera: down", "Câmera: baixo");
    }

    int v = (int)accion;
    if (v >= (int)HotkeyAction.CampChar1 && v <= (int)HotkeyAction.CampChar7)
    {
      int n = v - (int)HotkeyAction.CampChar1 + 1;
      return T("Personaje " + n, "Character " + n, "Personagem " + n);
    }

    if (v >= (int)HotkeyAction.BattleSkill1 && v <= (int)HotkeyAction.BattleSkill10)
    {
      int n = v - (int)HotkeyAction.BattleSkill1 + 1;
      return T("Habilidad " + n, "Skill " + n, "Habilidade " + n);
    }

    return accion.ToString();
  }

  private static string T(string es, string en, string pt)
  {
    return SaveSlotsLoc.T(es, en, pt);
  }
}
