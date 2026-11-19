using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Textos propios de los slots de guardado (ES / EN / PT; el chino usa ingles como respaldo).
// Se resuelven aca para no depender de claves nuevas en TRADU.
public static class SaveSlotsLoc
{
  public static string T(string es, string en, string pt)
  {
    int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
    if (idioma == TRADU.IdiomaPortugues) { return pt; }
    if (idioma == TRADU.IdiomaIngles || idioma == TRADU.IdiomaChinoSimplificado) { return en; }
    return es;
  }
}

// Panel "Partidas" del menu de opciones: lista de slots con vista previa (captura de la campaña),
// fecha, progreso y acciones Guardar / Cargar / Borrar. Se construye por codigo dentro de
// OpcionesCargarPlayerPrefsUI (igual que los controles de Banter y escala de texto), sin tocar escenas.
// En el menu principal funciona en modo "solo cargar".
public class SaveSlotsPanelUI : MonoBehaviour
{
  private const float RowGap = 4f;
  private const float StatusHeight = 20f;
  private const float RowPadding = 5f;
  private const float ButtonWidth = 64f;
  private const float ButtonHeight = 24f;
  private const float DeleteSize = 24f;
  private const float ButtonGap = 4f;
  private const float ConfirmSeconds = 3f;
  private const float PreviewAspect = 16f / 9f;

  private static readonly Color RowColor = new Color(0.03f, 0.025f, 0.025f, 0.62f);
  private static readonly Color RowActiveColor = new Color(0.16f, 0.11f, 0.05f, 0.6f);
  private static readonly Color TextColor = new Color(0.86f, 0.86f, 0.86f, 1f);
  private static readonly Color DimTextHex = new Color(0.62f, 0.62f, 0.62f, 1f);
  private static readonly Color ConfirmColor = new Color(1f, 0.78f, 0.35f, 1f);
  private static readonly Color ErrorColor = new Color(1f, 0.45f, 0.4f, 1f);
  private static readonly Color OkColor = new Color(0.55f, 0.9f, 0.7f, 1f);

  private enum Accion { Guardar, Cargar, Borrar }

  private sealed class Fila
  {
    public int slot;
    public Image fondo;
    public RawImage preview;
    public TextMeshProUGUI textoSinPreview;
    public TextMeshProUGUI info;
    public Button botonGuardar;
    public TextMeshProUGUI textoGuardar;
    public Button botonCargar;
    public TextMeshProUGUI textoCargar;
    public Button botonBorrar;
    public TextMeshProUGUI textoBorrar;
    public Texture2D textura;
  }

  private OpcionesCargarPlayerPrefsUI opciones;
  private TMP_FontAsset fuente;
  private Material materialFuente;
  private Sprite spriteBoton;
  private ColorBlock coloresBoton;
  private AudioClip clipClick;
  private UnityEngine.Audio.AudioMixerGroup grupoAudio;
  private float volumenClick = 1f;
  private readonly List<Fila> filas = new List<Fila>();
  private TextMeshProUGUI textoEstado;

  // Geometria de las filas: se calcula a partir del marco real del menu (ver CalcularGeometria).
  private float rowHeight = 80f;
  private float rowWidth = 380f;
  private float rowCenterX;
  private float rowTopY;
  private float previewWidth = 112f;
  private float previewHeight = 63f;

  private bool soloCargar;
  private bool puedeGuardar;
  private string motivoNoGuardar = string.Empty;
  private bool puedeCargar;
  private string motivoNoCargar = string.Empty;

  private int armadoSlot = -1;
  private Accion armadoAccion;
  private float armadoHasta;

  public static SaveSlotsPanelUI Crear(RectTransform referencia, OpcionesCargarPlayerPrefsUI opciones, Button plantillaBoton)
  {
    if (referencia == null || referencia.parent == null || plantillaBoton == null)
    {
      return null;
    }

    GameObject objeto = new GameObject("ContenidoPartidas", typeof(RectTransform));
    objeto.layer = referencia.gameObject.layer;
    RectTransform rect = (RectTransform)objeto.transform;
    rect.SetParent(referencia.parent, false);
    rect.anchorMin = referencia.anchorMin;
    rect.anchorMax = referencia.anchorMax;
    rect.pivot = referencia.pivot;
    rect.anchoredPosition = referencia.anchoredPosition;
    rect.sizeDelta = referencia.sizeDelta;
    rect.localScale = referencia.localScale;
    rect.SetSiblingIndex(referencia.GetSiblingIndex() + 1);
    objeto.SetActive(false);

    SaveSlotsPanelUI panel = objeto.AddComponent<SaveSlotsPanelUI>();
    panel.Construir(opciones, plantillaBoton);
    return panel;
  }

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

    CalcularGeometria();

    for (int slot = 0; slot < SaveSlots.TotalSlots; slot++)
    {
      filas.Add(CrearFila(slot));
    }

    textoEstado = CrearTexto(transform, "txtEstado", 11f, TextAlignmentOptions.Center, TextColor);
    textoEstado.raycastTarget = false;
    RectTransform rectEstado = textoEstado.rectTransform;
    rectEstado.anchorMin = rectEstado.anchorMax = new Vector2(0.5f, 1f);
    rectEstado.pivot = new Vector2(0.5f, 1f);
    rectEstado.sizeDelta = new Vector2(rowWidth, StatusHeight);
    rectEstado.anchoredPosition = new Vector2(rowCenterX, rowTopY - SaveSlots.TotalSlots * (rowHeight + RowGap));
  }

  // El contenido del menu de opciones es mas ancho que el marco visible (la botonera ocupa la izquierda),
  // asi que el area util se mide a partir del rectangulo real de "Fondo" (el marco).
  private void CalcularGeometria()
  {
    RectTransform self = (RectTransform)transform;
    Rect util = self.rect;

    Transform raiz = opciones != null ? opciones.transform : null;
    RectTransform fondo = raiz != null ? raiz.Find("Fondo") as RectTransform : null;
    if (fondo != null)
    {
      Rect marco = RectEnLocal(self, fondo);
      float insetX = marco.width * 0.065f;
      float insetY = marco.height * 0.055f;
      // El rectangulo de la botonera es mas ancho que su arte visible: la columna de pestañas
      // ocupa aproximadamente el tercio izquierdo del marco.
      float izquierda = marco.xMin + marco.width * 0.335f;
      util = Rect.MinMaxRect(izquierda, marco.yMin + insetY, marco.xMax - insetX, marco.yMax - insetY);
    }

    rowWidth = Mathf.Max(260f, util.width);
    rowCenterX = util.center.x;
    rowTopY = util.yMax - self.rect.yMax;
    float altoDisponible = util.height - StatusHeight - 4f;
    rowHeight = Mathf.Clamp((altoDisponible - (SaveSlots.TotalSlots - 1) * RowGap) / SaveSlots.TotalSlots, 56f, 92f);
    previewHeight = rowHeight - RowPadding * 2f;
    previewWidth = previewHeight * PreviewAspect;
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

  private Fila CrearFila(int slot)
  {
    Fila fila = new Fila { slot = slot };

    GameObject objetoFila = new GameObject("Slot" + slot, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    objetoFila.layer = gameObject.layer;
    RectTransform rectFila = (RectTransform)objetoFila.transform;
    rectFila.SetParent(transform, false);
    rectFila.anchorMin = new Vector2(0.5f, 1f);
    rectFila.anchorMax = new Vector2(0.5f, 1f);
    rectFila.pivot = new Vector2(0.5f, 1f);
    rectFila.sizeDelta = new Vector2(rowWidth, rowHeight);
    rectFila.anchoredPosition = new Vector2(rowCenterX, rowTopY - slot * (rowHeight + RowGap));
    fila.fondo = objetoFila.GetComponent<Image>();
    fila.fondo.color = RowColor;
    fila.fondo.raycastTarget = false;

    // Vista previa (marco negro + RawImage recortada a 16:9)
    GameObject marco = new GameObject("Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    marco.layer = gameObject.layer;
    RectTransform rectMarco = (RectTransform)marco.transform;
    rectMarco.SetParent(rectFila, false);
    rectMarco.anchorMin = rectMarco.anchorMax = new Vector2(0f, 0.5f);
    rectMarco.pivot = new Vector2(0f, 0.5f);
    rectMarco.anchoredPosition = new Vector2(RowPadding, 0f);
    rectMarco.sizeDelta = new Vector2(previewWidth, previewHeight);
    Image imagenMarco = marco.GetComponent<Image>();
    imagenMarco.color = new Color(0f, 0f, 0f, 0.75f);
    imagenMarco.raycastTarget = false;

    GameObject objetoRaw = new GameObject("Imagen", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
    objetoRaw.layer = gameObject.layer;
    RectTransform rectRaw = (RectTransform)objetoRaw.transform;
    rectRaw.SetParent(rectMarco, false);
    rectRaw.anchorMin = Vector2.zero;
    rectRaw.anchorMax = Vector2.one;
    rectRaw.offsetMin = new Vector2(1f, 1f);
    rectRaw.offsetMax = new Vector2(-1f, -1f);
    fila.preview = objetoRaw.GetComponent<RawImage>();
    fila.preview.raycastTarget = false;
    fila.preview.enabled = false;

    fila.textoSinPreview = CrearTexto(rectMarco, "txtSinPreview", 10f, TextAlignmentOptions.Center, DimTextHex);
    fila.textoSinPreview.raycastTarget = false;
    Estirar(fila.textoSinPreview.rectTransform, 4f);

    // Texto informativo (columna derecha, sobre los botones)
    fila.info = CrearTexto(rectFila, "txtInfo", 12f, TextAlignmentOptions.TopLeft, TextColor);
    fila.info.raycastTarget = false;
    fila.info.richText = true;
    RectTransform rectInfo = fila.info.rectTransform;
    rectInfo.anchorMin = Vector2.zero;
    rectInfo.anchorMax = Vector2.one;
    rectInfo.offsetMin = new Vector2(RowPadding + previewWidth + 8f, RowPadding + ButtonHeight + 3f);
    rectInfo.offsetMax = new Vector2(-RowPadding, -3f);

    // Botones alineados a la derecha, en la base de la fila: [Guardar] [Cargar] [X]
    fila.botonBorrar = CrearBoton(rectFila, "bt_Borrar", DeleteSize, ButtonHeight, out fila.textoBorrar);
    PosicionarBoton(fila.botonBorrar, RowPadding, DeleteSize);
    fila.botonCargar = CrearBoton(rectFila, "bt_Cargar", ButtonWidth, ButtonHeight, out fila.textoCargar);
    PosicionarBoton(fila.botonCargar, RowPadding + DeleteSize + ButtonGap, ButtonWidth);
    fila.botonGuardar = CrearBoton(rectFila, "bt_Guardar", ButtonWidth, ButtonHeight, out fila.textoGuardar);
    PosicionarBoton(fila.botonGuardar, RowPadding + DeleteSize + ButtonGap + ButtonWidth + ButtonGap, ButtonWidth);

    int slotLocal = slot;
    fila.botonGuardar.onClick.AddListener(() => OnAccion(slotLocal, Accion.Guardar));
    fila.botonCargar.onClick.AddListener(() => OnAccion(slotLocal, Accion.Cargar));
    fila.botonBorrar.onClick.AddListener(() => OnAccion(slotLocal, Accion.Borrar));
    return fila;
  }

  private static void PosicionarBoton(Button boton, float margenDerecho, float ancho)
  {
    RectTransform rect = (RectTransform)boton.transform;
    rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
    rect.pivot = new Vector2(1f, 0f);
    rect.anchoredPosition = new Vector2(-margenDerecho, RowPadding);
    rect.sizeDelta = new Vector2(ancho, ButtonHeight);
  }

  private static void Estirar(RectTransform rect, float margen)
  {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = new Vector2(margen, margen);
    rect.offsetMax = new Vector2(-margen, -margen);
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
    texto.textWrappingMode = TextWrappingModes.Normal;
    texto.overflowMode = TextOverflowModes.Overflow;
    // Autoajuste: respeta la escala de texto global del jugador sin desbordar la fila.
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

    texto = CrearTexto(rect, "Text (TMP)", 12f, TextAlignmentOptions.Center, TextColor);
    texto.raycastTarget = false;
    texto.textWrappingMode = TextWrappingModes.NoWrap;
    Estirar(texto.rectTransform, 2f);
    return boton;
  }

  private void OnEnable()
  {
    Desarmar();
    Refrescar();
  }

  private void OnDisable()
  {
    Desarmar();
    LiberarTexturas();
  }

  private void OnDestroy()
  {
    LiberarTexturas();
  }

  private void Update()
  {
    if (armadoSlot >= 0 && Time.unscaledTime > armadoHasta)
    {
      Desarmar();
      ActualizarEtiquetasBotones();
    }
  }

  private void LiberarTexturas()
  {
    for (int i = 0; i < filas.Count; i++)
    {
      Fila fila = filas[i];
      if (fila.preview != null) { fila.preview.texture = null; fila.preview.enabled = false; }
      if (fila.textura != null)
      {
        Destroy(fila.textura);
        fila.textura = null;
      }
    }
  }

  public void Refrescar()
  {
    soloCargar = CampaignManager.Instance == null;

    puedeGuardar = false;
    motivoNoGuardar = string.Empty;
    puedeCargar = true;
    motivoNoCargar = string.Empty;
    if (!soloCargar)
    {
      puedeGuardar = CampaignManager.Instance.PuedeGuardarCampania(out string motivoG);
      motivoNoGuardar = TraducirMotivo(motivoG);
      puedeCargar = CampaignManager.Instance.PuedeCargarCampania(out motivoNoCargar);
    }

    int activo = SaveSlots.ActiveManualSlot;
    for (int i = 0; i < filas.Count; i++)
    {
      Fila fila = filas[i];
      SaveSlots.SlotInfo info = SaveSlots.GetInfo(fila.slot);
      ActualizarVistaPrevia(fila, info);
      fila.info.text = ConstruirTextoInfo(info, !soloCargar && fila.slot == activo);
      fila.fondo.color = (!soloCargar && fila.slot == activo) ? RowActiveColor : RowColor;

      bool esAuto = info.IsAuto;
      fila.botonGuardar.gameObject.SetActive(!soloCargar && !esAuto);
      fila.botonGuardar.interactable = puedeGuardar;
      fila.botonCargar.interactable = info.Loadable && puedeCargar;
      fila.botonBorrar.interactable = info.Exists;
    }

    ActualizarEtiquetasBotones();

    if (!soloCargar && !puedeGuardar && !string.IsNullOrEmpty(motivoNoGuardar))
    {
      MostrarEstado(motivoNoGuardar, DimTextHex);
    }
    else if (soloCargar)
    {
      MostrarEstado(string.Empty, DimTextHex);
    }
    else
    {
      MostrarEstado(SaveSlotsLoc.T("Guardado rápido: F5 (slot activo)  ·  Cargar último: F9",
        "Quick save: F5 (active slot)  ·  Load latest: F9",
        "Salvar rápido: F5 (slot ativo)  ·  Carregar último: F9"), DimTextHex);
    }
  }

  private static string TraducirMotivo(string motivo)
  {
    return string.IsNullOrEmpty(motivo) ? string.Empty : (TRADU.i != null ? TRADU.i.Traducir(motivo) : motivo);
  }

  private void ActualizarVistaPrevia(Fila fila, SaveSlots.SlotInfo info)
  {
    if (fila.textura != null)
    {
      fila.preview.texture = null;
      Destroy(fila.textura);
      fila.textura = null;
    }

    fila.preview.enabled = false;
    if (info.Loadable && info.HasPreview)
    {
      Texture2D textura = SaveSlots.LoadPreviewTexture(fila.slot);
      if (textura != null)
      {
        fila.textura = textura;
        fila.preview.texture = textura;
        fila.preview.enabled = true;
        fila.preview.uvRect = CalcularRecorteUv(textura.width, textura.height);
        fila.textoSinPreview.text = string.Empty;
        return;
      }
    }

    if (!info.Exists)
    {
      fila.textoSinPreview.text = string.Empty;
    }
    else if (info.Corrupt)
    {
      fila.textoSinPreview.text = "!";
    }
    else
    {
      fila.textoSinPreview.text = SaveSlotsLoc.T("Sin vista previa", "No preview", "Sem pré-visualização");
    }
  }

  private static Rect CalcularRecorteUv(int ancho, int alto)
  {
    float aspecto = ancho / (float)Mathf.Max(1, alto);
    if (aspecto > PreviewAspect)
    {
      float w = PreviewAspect / aspecto;
      return new Rect((1f - w) * 0.5f, 0f, w, 1f);
    }

    float h = aspecto / PreviewAspect;
    return new Rect(0f, (1f - h) * 0.5f, 1f, h);
  }

  private static string NombreSlot(SaveSlots.SlotInfo info)
  {
    return info.IsAuto
      ? SaveSlotsLoc.T("Autoguardado", "Autosave", "Salvamento automático")
      : SaveSlotsLoc.T("Slot ", "Slot ", "Slot ") + info.Slot;
  }

  private string ConstruirTextoInfo(SaveSlots.SlotInfo info, bool activo)
  {
    string nombre = NombreSlot(info);
    if (activo)
    {
      nombre += "  <color=#E8C070>●</color>";
    }

    if (!info.Exists)
    {
      return "<b>" + nombre + "</b>\n<color=#8A8A8A>" + SaveSlotsLoc.T("Vacío", "Empty", "Vazio") + "</color>";
    }

    if (info.Corrupt)
    {
      return "<b>" + nombre + "</b>\n<color=#FF7366>"
        + SaveSlotsLoc.T("Archivo dañado: no se puede cargar.", "Damaged file: cannot be loaded.", "Arquivo danificado: não pode ser carregado.")
        + "</color>";
    }

    int dia = (int)Math.Floor(Math.Max(0d, info.HorasTotales) / 24d) + 1;
    string fecha = info.SavedLocal.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    string detalle = fecha
      + "  ·  " + SaveSlotsLoc.T("Día ", "Day ", "Dia ") + dia
      + "  ·  " + info.PartySize + " " + SaveSlotsLoc.T("personajes", "characters", "personagens");

    return "<b>" + nombre + "</b>\n" + info.DisplayName + "\n<color=#9A9A9A>" + detalle + "</color>";
  }

  private void ActualizarEtiquetasBotones()
  {
    for (int i = 0; i < filas.Count; i++)
    {
      Fila fila = filas[i];
      PonerEtiqueta(fila, fila.textoGuardar, Accion.Guardar, SaveSlotsLoc.T("Guardar", "Save", "Salvar"));
      PonerEtiqueta(fila, fila.textoCargar, Accion.Cargar, SaveSlotsLoc.T("Cargar", "Load", "Carregar"));
      PonerEtiqueta(fila, fila.textoBorrar, Accion.Borrar, "X");
    }
  }

  private void PonerEtiqueta(Fila fila, TextMeshProUGUI texto, Accion accion, string normal)
  {
    bool armado = armadoSlot == fila.slot && armadoAccion == accion;
    if (armado)
    {
      texto.text = accion == Accion.Borrar ? "?" : SaveSlotsLoc.T("¿Seguro?", "Sure?", "Certeza?");
      texto.color = ConfirmColor;
    }
    else
    {
      texto.text = normal;
      texto.color = TextColor;
    }
  }

  private bool NecesitaConfirmacion(int slot, Accion accion)
  {
    switch (accion)
    {
      case Accion.Guardar:
        return SaveSlots.GetInfo(slot).Exists;
      case Accion.Cargar:
        return !soloCargar; // en campaña reemplaza el progreso actual no guardado
      default:
        return true;
    }
  }

  private void OnAccion(int slot, Accion accion)
  {
    if (NecesitaConfirmacion(slot, accion))
    {
      bool yaArmado = armadoSlot == slot && armadoAccion == accion && Time.unscaledTime <= armadoHasta;
      if (!yaArmado)
      {
        armadoSlot = slot;
        armadoAccion = accion;
        armadoHasta = Time.unscaledTime + ConfirmSeconds;
        ActualizarEtiquetasBotones();
        MostrarEstado(MensajeConfirmacion(slot, accion), ConfirmColor);
        return;
      }
    }

    Desarmar();
    Ejecutar(slot, accion);
  }

  private static string MensajeConfirmacion(int slot, Accion accion)
  {
    switch (accion)
    {
      case Accion.Guardar:
        return SaveSlotsLoc.T("El slot ya tiene una partida: pulsa de nuevo para sobreescribirla.",
          "This slot already has a save: press again to overwrite it.",
          "Este slot já tem uma partida: pressione de novo para sobrescrevê-la.");
      case Accion.Cargar:
        return SaveSlotsLoc.T("Se perderá el progreso no guardado: pulsa de nuevo para cargar.",
          "Unsaved progress will be lost: press again to load.",
          "O progresso não salvo será perdido: pressione de novo para carregar.");
      default:
        return SaveSlotsLoc.T("Pulsa de nuevo para borrar la partida de forma permanente.",
          "Press again to permanently delete this save.",
          "Pressione de novo para apagar esta partida permanentemente.");
    }
  }

  private void Ejecutar(int slot, Accion accion)
  {
    switch (accion)
    {
      case Accion.Guardar:
        EjecutarGuardar(slot);
        break;
      case Accion.Cargar:
        EjecutarCargar(slot);
        break;
      default:
        EjecutarBorrar(slot);
        break;
    }
  }

  private void EjecutarGuardar(int slot)
  {
    if (CampaignManager.Instance == null) { return; }

    if (CampaignManager.Instance.TryGuardarCampaniaEnSlot(slot, out string error))
    {
      Refrescar();
      MostrarEstado(SaveSlotsLoc.T("Partida guardada en el slot ", "Game saved to slot ", "Partida salva no slot ") + slot + ".", OkColor);
      // La captura de pantalla se escribe un par de frames despues del guardado.
      StopAllCoroutines();
      StartCoroutine(RefrescarVistaPreviaTrasGuardar());
    }
    else
    {
      MostrarEstado(SaveSlotsLoc.T("No se pudo guardar: ", "Could not save: ", "Não foi possível salvar: ") + TraducirMotivo(error), ErrorColor);
    }
  }

  private IEnumerator RefrescarVistaPreviaTrasGuardar()
  {
    string estado = textoEstado.text;
    Color color = textoEstado.color;
    yield return new WaitForSecondsRealtime(0.35f);
    Refrescar();
    MostrarEstado(estado, color);
    yield return new WaitForSecondsRealtime(0.8f);
    Refrescar();
    MostrarEstado(estado, color);
  }

  private void EjecutarCargar(int slot)
  {
    if (CampaignManager.Instance == null)
    {
      MenuController menu = FindFirstObjectByType<MenuController>(FindObjectsInactive.Include);
      string errorMenu = string.Empty;
      if (menu != null && menu.OnCargarSlot(slot, out errorMenu))
      {
        MostrarEstado(SaveSlotsLoc.T("Cargando…", "Loading…", "Carregando…"), OkColor);
      }
      else
      {
        Refrescar();
        MostrarEstado(SaveSlotsLoc.T("No se pudo cargar: ", "Could not load: ", "Não foi possível carregar: ") + (menu == null ? string.Empty : TraducirMotivo(errorMenu)), ErrorColor);
      }
      return;
    }

    if (CampaignManager.Instance.CargarCampaniaDesdeSlot(slot))
    {
      if (opciones != null) { opciones.cerrarOpciones(); }
      return;
    }

    Refrescar();
    MostrarEstado(SaveSlotsLoc.T("No se pudo cargar esa partida.", "Could not load that save.", "Não foi possível carregar essa partida."), ErrorColor);
  }

  private void EjecutarBorrar(int slot)
  {
    if (SaveSlots.TryDeleteSlot(slot, out string error))
    {
      Refrescar();
      MostrarEstado(SaveSlotsLoc.T("Partida borrada.", "Save deleted.", "Partida apagada."), OkColor);
    }
    else
    {
      MostrarEstado(SaveSlotsLoc.T("No se pudo borrar: ", "Could not delete: ", "Não foi possível apagar: ") + error, ErrorColor);
    }
  }

  private void Desarmar()
  {
    armadoSlot = -1;
  }

  private void MostrarEstado(string mensaje, Color color)
  {
    if (textoEstado == null) { return; }
    textoEstado.text = mensaje;
    textoEstado.color = color;
  }
}
