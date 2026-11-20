using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pestaña "Accesibilidad" del menu de opciones (se construye por codigo, igual que Partidas y Graficos).
// Los ajustes viven en AccessibilitySettings; cada efecto del juego los consulta.
public class AccessibilityPanelUI : MonoBehaviour
{
  private const string NombreContenedor = "ContenidoAccesibilidad";
  private const float AnchoControl = 150f;
  private const float AltoDropdown = 21f;
  private const float AltoFila = 32f;
  private const float SeparacionFilas = 3f;

  private static readonly Color ColorTexto = new Color(0.86f, 0.86f, 0.86f, 1f);
  private static readonly Color ColorTenue = new Color(0.62f, 0.62f, 0.62f, 1f);
  private static readonly Color ColorFila = new Color(0.03f, 0.025f, 0.025f, 0.45f);

  private sealed class Etiqueta
  {
    public TextMeshProUGUI texto;
    public Func<string> contenido;
  }

  private OpcionesCargarPlayerPrefsUI opciones;
  private TMP_FontAsset fuente;
  private Material materialFuente;
  private Sprite spriteBoton;
  private ColorBlock coloresBoton;
  private readonly List<Etiqueta> etiquetas = new List<Etiqueta>();

  private Slider slTexto;
  private Slider slContraste;
  private TMP_Dropdown ddDaltonismo;
  private Toggle tgSacudida;
  private Toggle tgDestellos;
  private Toggle tgVaiven;
  private Toggle tgBordes;
  private TextMeshProUGUI txtNota;
  private Button btRestaurar;
  private TextMeshProUGUI txtRestaurar;

  private Rect areaUtil;
  private float cursorY;
  private bool refrescando;
  private bool restaurarArmado;
  private float restaurarHasta;

  public static AccessibilityPanelUI Crear(RectTransform referencia, OpcionesCargarPlayerPrefsUI opciones, Button plantillaBoton)
  {
    if (referencia == null || referencia.parent == null || plantillaBoton == null || opciones == null)
    {
      return null;
    }

    // Los controles de plantilla salen de los que ya existen en el menu (se clonan sin sus eventos).
    Slider plantillaSlider = opciones.brilloSlider != null ? opciones.brilloSlider : opciones.volMusicaSlider;
    if (plantillaSlider == null || opciones.fullscreenToggle == null || opciones.graficosDropdown == null)
    {
      return null;
    }

    GameObject objeto = new GameObject(NombreContenedor, typeof(RectTransform));
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

    AccessibilityPanelUI panel = objeto.AddComponent<AccessibilityPanelUI>();
    panel.Construir(opciones, plantillaBoton, plantillaSlider);
    return panel;
  }

  // ---------------------------------------------------------------- construccion

  private void Construir(OpcionesCargarPlayerPrefsUI opcionesPadre, Button plantillaBoton, Slider plantillaSlider)
  {
    opciones = opcionesPadre;

    Image imagenPlantilla = plantillaBoton.GetComponent<Image>();
    spriteBoton = imagenPlantilla != null ? imagenPlantilla.sprite : null;
    coloresBoton = plantillaBoton.colors;
    TMP_Text textoBase = opciones.graficosDropdown.GetComponentInChildren<TMP_Text>(true);
    if (textoBase != null)
    {
      fuente = textoBase.font;
      materialFuente = textoBase.fontSharedMaterial;
    }

    CalcularArea();
    cursorY = areaUtil.yMax;

    slTexto = CrearSlider(plantillaSlider, () => T("Tamaño del texto", "Text size", "Tamanho do texto") + ": " + Mathf.RoundToInt(AccessibilitySettings.TextScale * 100f) + "%",
      EscaladoFuentesGlobal.EscalaMinima * 100f, EscaladoFuentesGlobal.EscalaMaxima * 100f, OnTexto);
    slContraste = CrearSlider(plantillaSlider, () => T("Contraste", "Contrast", "Contraste") + ": " + Mathf.RoundToInt(AccessibilitySettings.Contrast * 100f) + "%",
      AccessibilitySettings.ContrastMin * 100f, AccessibilitySettings.ContrastMax * 100f, OnContraste);
    ddDaltonismo = CrearDropdown(opciones.graficosDropdown, () => T("Filtro de daltonismo", "Colorblind filter", "Filtro de daltonismo"), OnDaltonismo);
    tgSacudida = CrearToggle(opciones.fullscreenToggle, () => T("Reducir sacudidas de cámara", "Reduce camera shake", "Reduzir tremores da câmera"), v => { if (!refrescando) { AccessibilitySettings.ReduceShake = v; } });
    tgDestellos = CrearToggle(opciones.fullscreenToggle, () => T("Reducir destellos y parpadeos", "Reduce flashes & pulsing", "Reduzir flashes e piscadas"), v => { if (!refrescando) { AccessibilitySettings.ReduceFlashes = v; } });
    tgVaiven = CrearToggle(opciones.fullscreenToggle, () => T("Reducir vaivén de cámara", "Reduce camera sway", "Reduzir balanço da câmera"), v => { if (!refrescando) { AccessibilitySettings.ReduceCameraSway = v; } });
    tgBordes = CrearToggle(opciones.fullscreenToggle, () => T("Mover cámara con el borde de pantalla", "Edge-of-screen camera scroll", "Mover câmera pela borda da tela"), v => { if (!refrescando) { AccessibilitySettings.EdgeScrolling = v; } });

    // Nota al pie
    txtNota = CrearTexto(transform, "txtNota", 10f, TextAlignmentOptions.TopLeft, ColorTenue);
    txtNota.raycastTarget = false;
    txtNota.textWrappingMode = TextWrappingModes.Normal;
    RectTransform rectNota = txtNota.rectTransform;
    rectNota.anchorMin = rectNota.anchorMax = new Vector2(0.5f, 0.5f);
    rectNota.pivot = new Vector2(0.5f, 1f);
    rectNota.sizeDelta = new Vector2(areaUtil.width - 12f, 44f);
    rectNota.anchoredPosition = new Vector2(areaUtil.center.x, cursorY - 2f);

    // Boton restaurar
    btRestaurar = CrearBoton(transform, "bt_RestaurarAccesibilidad", 130f, 22f, out txtRestaurar);
    RectTransform rectBoton = (RectTransform)btRestaurar.transform;
    rectBoton.anchorMin = rectBoton.anchorMax = new Vector2(0.5f, 0.5f);
    rectBoton.pivot = new Vector2(1f, 0f);
    rectBoton.anchoredPosition = new Vector2(areaUtil.xMax - 6f, areaUtil.yMin);
    btRestaurar.onClick.AddListener(OnRestaurar);

    AccessibilitySettings.Changed += OnAjustesCambiados;
    Refrescar();
  }

  private void OnDestroy()
  {
    AccessibilitySettings.Changed -= OnAjustesCambiados;
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

  private RectTransform CrearFila(Func<string> texto)
  {
    GameObject objeto = new GameObject("Fila", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    objeto.layer = gameObject.layer;
    RectTransform fila = (RectTransform)objeto.transform;
    fila.SetParent(transform, false);
    fila.anchorMin = fila.anchorMax = new Vector2(0.5f, 0.5f);
    fila.pivot = new Vector2(0.5f, 1f);
    fila.sizeDelta = new Vector2(areaUtil.width, AltoFila);
    fila.anchoredPosition = new Vector2(areaUtil.center.x, cursorY);
    cursorY -= AltoFila + SeparacionFilas;

    Image fondo = objeto.GetComponent<Image>();
    fondo.color = ColorFila;
    fondo.raycastTarget = false;

    TextMeshProUGUI label = CrearTexto(fila, "txtEtiqueta", 12f, TextAlignmentOptions.MidlineLeft, ColorTexto);
    label.raycastTarget = false;
    RectTransform rectLabel = label.rectTransform;
    rectLabel.anchorMin = Vector2.zero;
    rectLabel.anchorMax = Vector2.one;
    rectLabel.offsetMin = new Vector2(8f, 0f);
    rectLabel.offsetMax = new Vector2(-(AnchoControl + 14f), 0f);

    etiquetas.Add(new Etiqueta { texto = label, contenido = texto });
    return fila;
  }

  private Slider CrearSlider(Slider plantilla, Func<string> texto, float min, float max, UnityEngine.Events.UnityAction<float> alCambiar)
  {
    RectTransform fila = CrearFila(texto);
    GameObject clon = Instantiate(plantilla.gameObject, fila);
    clon.name = "Slider";
    Slider slider = clon.GetComponent<Slider>();
    slider.onValueChanged = new Slider.SliderEvent(); // descarta los eventos persistentes de la plantilla
    slider.minValue = min;
    slider.maxValue = max;
    slider.wholeNumbers = true;
    Colocar((RectTransform)clon.transform, fila, AnchoControl, 20f);
    slider.onValueChanged.AddListener(alCambiar);
    return slider;
  }

  private TMP_Dropdown CrearDropdown(TMP_Dropdown plantilla, Func<string> texto, UnityEngine.Events.UnityAction<int> alCambiar)
  {
    RectTransform fila = CrearFila(texto);
    GameObject clon = Instantiate(plantilla.gameObject, fila);
    clon.name = "Dropdown";
    TMP_Dropdown dropdown = clon.GetComponent<TMP_Dropdown>();
    dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
    if (dropdown.captionText != null)
    {
      // Evita que un nombre largo salte de linea y se salga del desplegable.
      // No usar Ellipsis: el rect del caption es mas bajo que la linea y el texto desaparece.
      dropdown.captionText.textWrappingMode = TextWrappingModes.NoWrap;
    }
    Colocar((RectTransform)clon.transform, fila, AnchoControl, AltoDropdown);
    dropdown.onValueChanged.AddListener(alCambiar);
    return dropdown;
  }

  private Toggle CrearToggle(Toggle plantilla, Func<string> texto, UnityEngine.Events.UnityAction<bool> alCambiar)
  {
    RectTransform fila = CrearFila(texto);
    GameObject clon = Instantiate(plantilla.gameObject, fila);
    clon.name = "Toggle";
    Toggle toggle = clon.GetComponent<Toggle>();
    toggle.onValueChanged = new Toggle.ToggleEvent();
    TMP_Text[] tmp = clon.GetComponentsInChildren<TMP_Text>(true);
    for (int i = 0; i < tmp.Length; i++) { tmp[i].gameObject.SetActive(false); }
    Text[] legacy = clon.GetComponentsInChildren<Text>(true);
    for (int i = 0; i < legacy.Length; i++) { legacy[i].gameObject.SetActive(false); }
    Colocar((RectTransform)clon.transform, fila, 24f, 24f);
    toggle.onValueChanged.AddListener(alCambiar);
    return toggle;
  }

  private static void Colocar(RectTransform control, RectTransform fila, float ancho, float alto)
  {
    control.SetParent(fila, false);
    control.gameObject.SetActive(true);
    control.anchorMin = control.anchorMax = new Vector2(1f, 0.5f);
    control.pivot = new Vector2(1f, 0.5f);
    control.localScale = Vector3.one;
    control.anchoredPosition = new Vector2(-6f, 0f);
    control.sizeDelta = new Vector2(ancho, Mathf.Min(alto, fila.sizeDelta.y - 2f));
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

    texto = CrearTexto(rect, "Text (TMP)", 11f, TextAlignmentOptions.Center, ColorTexto);
    texto.raycastTarget = false;
    RectTransform rectTexto = texto.rectTransform;
    rectTexto.anchorMin = Vector2.zero;
    rectTexto.anchorMax = Vector2.one;
    rectTexto.offsetMin = new Vector2(2f, 1f);
    rectTexto.offsetMax = new Vector2(-2f, -1f);
    return boton;
  }

  // ---------------------------------------------------------------- sincronizacion

  private void OnEnable()
  {
    restaurarArmado = false;
    Refrescar();
  }

  private void Update()
  {
    if (restaurarArmado && Time.unscaledTime > restaurarHasta)
    {
      restaurarArmado = false;
      ActualizarBotonRestaurar();
    }
  }

  private void OnAjustesCambiados()
  {
    if (isActiveAndEnabled) { Refrescar(); }
  }

  public void Refrescar()
  {
    if (slTexto == null) { return; }
    refrescando = true;
    try
    {
      slTexto.SetValueWithoutNotify(Mathf.Round(AccessibilitySettings.TextScale * 100f));
      slContraste.SetValueWithoutNotify(Mathf.Round(AccessibilitySettings.Contrast * 100f));

      List<string> modos = new List<string>
      {
        T("Desactivado", "Off", "Desativado"),
        T("Protanopía", "Protanopia", "Protanopia"),
        T("Deuteranopía", "Deuteranopia", "Deuteranopia"),
        T("Tritanopía", "Tritanopia", "Tritanopia")
      };
      if (ddDaltonismo.options.Count != modos.Count)
      {
        ddDaltonismo.ClearOptions();
        ddDaltonismo.AddOptions(modos);
      }
      else
      {
        for (int i = 0; i < modos.Count; i++) { ddDaltonismo.options[i].text = modos[i]; }
      }
      ddDaltonismo.SetValueWithoutNotify((int)AccessibilitySettings.ColorBlind);
      ddDaltonismo.RefreshShownValue();

      tgSacudida.SetIsOnWithoutNotify(AccessibilitySettings.ReduceShake);
      tgDestellos.SetIsOnWithoutNotify(AccessibilitySettings.ReduceFlashes);
      tgVaiven.SetIsOnWithoutNotify(AccessibilitySettings.ReduceCameraSway);
      tgBordes.SetIsOnWithoutNotify(AccessibilitySettings.EdgeScrolling);

      // El filtro y el contraste dependen del postprocesado de color.
      bool conPostFx = GraphicsQuality.PostProcessingEnabled;
      ddDaltonismo.interactable = conPostFx;
      slContraste.interactable = conPostFx;

      for (int i = 0; i < etiquetas.Count; i++) { etiquetas[i].texto.text = etiquetas[i].contenido(); }

      txtNota.text = T("El filtro de daltonismo y el contraste se aplican al mundo del juego y requieren el postprocesado activo (Gráficos). La interfaz mantiene sus colores.",
        "The colorblind filter and contrast apply to the game world and need post-processing enabled (Graphics). The interface keeps its colors.",
        "O filtro de daltonismo e o contraste afetam o mundo do jogo e exigem o pós-processamento ativo (Gráficos). A interface mantém suas cores.");
      ActualizarBotonRestaurar();
    }
    finally
    {
      refrescando = false;
    }
  }

  private void ActualizarBotonRestaurar()
  {
    txtRestaurar.text = restaurarArmado ? T("¿Seguro?", "Sure?", "Certeza?") : T("Restablecer", "Reset", "Redefinir");
  }

  // ---------------------------------------------------------------- eventos

  private void OnTexto(float valor)
  {
    if (refrescando) { return; }
    AccessibilitySettings.TextScale = valor / 100f;
    etiquetas[0].texto.text = etiquetas[0].contenido();
  }

  private void OnContraste(float valor)
  {
    if (refrescando) { return; }
    AccessibilitySettings.Contrast = valor / 100f;
    VisualPolishRuntime.RefreshColorGradingNow();
    etiquetas[1].texto.text = etiquetas[1].contenido();
  }

  private void OnDaltonismo(int indice)
  {
    if (refrescando) { return; }
    AccessibilitySettings.ColorBlind = (AccessibilitySettings.ColorBlindMode)indice;
    VisualPolishRuntime.RefreshColorGradingNow();
  }

  private void OnRestaurar()
  {
    if (!restaurarArmado)
    {
      restaurarArmado = true;
      restaurarHasta = Time.unscaledTime + 3f;
      ActualizarBotonRestaurar();
      return;
    }

    restaurarArmado = false;
    AccessibilitySettings.ResetAll();
    VisualPolishRuntime.RefreshColorGradingNow();
    Refrescar();
  }

  private static string T(string es, string en, string pt)
  {
    return SaveSlotsLoc.T(es, en, pt);
  }
}
