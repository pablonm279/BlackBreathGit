using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de gráficos del menu de opciones. Se construye por codigo dentro de PanelGraficos:
//  * Reubica los controles que ya existian (resolucion, pantalla completa, vSync, limite de FPS,
//    calidad y brillo) en una lista ordenada de filas "etiqueta + control".
//  * Agrega las opciones de detalle que suelen tener los juegos profesionales: postproceso, anti-aliasing,
//    sombras, texturas, particulas, bloom, oclusion ambiental y profundidad de campo.
// Elegir una calidad reinicia los detalles al preset; cambiar un detalle suelto pasa la calidad a "Personalizada".
public class GraphicsOptionsPanel : MonoBehaviour
{
  private const string NombreContenedor = "OpcionesGraficas";
  private const float AnchoControl = 150f;
  private const float AltoDropdown = 21f;
  private const float AltoHint = 20f;
  private const float SeparacionFilas = 2f;

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
  private readonly List<Etiqueta> etiquetas = new List<Etiqueta>();

  private TMP_Dropdown ddPreset;
  private TMP_Dropdown ddAA;
  private TMP_Dropdown ddSombras;
  private TMP_Dropdown ddTexturas;
  private TMP_Dropdown ddParticulas;
  private Toggle tgPostFx;
  private Toggle tgBloom;
  private Toggle tgAO;
  private Toggle tgDoF;
  private TextMeshProUGUI txtHint;

  private float alturaFila = 30f;
  private Rect areaUtil;
  private float cursorY;
  private bool refrescando;

  public static GraphicsOptionsPanel Asegurar(OpcionesCargarPlayerPrefsUI opciones)
  {
    if (opciones == null || opciones.PanelGraficos == null)
    {
      return null;
    }

    Transform existente = opciones.PanelGraficos.transform.Find(NombreContenedor);
    if (existente != null)
    {
      return existente.GetComponent<GraphicsOptionsPanel>();
    }

    if (opciones.graficosDropdown == null || opciones.fullscreenToggle == null || opciones.resolucionDropdown == null)
    {
      return null;
    }

    GameObject objeto = new GameObject(NombreContenedor, typeof(RectTransform));
    objeto.layer = opciones.PanelGraficos.layer;
    RectTransform rect = (RectTransform)objeto.transform;
    rect.SetParent(opciones.PanelGraficos.transform, false);
    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = Vector2.zero;
    rect.sizeDelta = ((RectTransform)opciones.PanelGraficos.transform).sizeDelta;

    GraphicsOptionsPanel panel = objeto.AddComponent<GraphicsOptionsPanel>();
    panel.Construir(opciones);
    return panel;
  }

  // ---------------------------------------------------------------- construccion

  private void Construir(OpcionesCargarPlayerPrefsUI opcionesPadre)
  {
    opciones = opcionesPadre;
    Transform panelGraficos = opciones.PanelGraficos.transform;

    TMP_Text textoBase = opciones.graficosDropdown.GetComponentInChildren<TMP_Text>(true);
    if (textoBase != null)
    {
      fuente = textoBase.font;
      materialFuente = textoBase.fontSharedMaterial;
    }

    Toggle vsync = opciones.vSyncToggle != null ? opciones.vSyncToggle : BuscarHijo<Toggle>(panelGraficos, "vsync");
    Slider brillo = opciones.brilloSlider;

    // Se ocultan las etiquetas y controles sueltos del diseño anterior (los que se reubican se rescatan antes).
    List<Transform> rescatados = new List<Transform>
    {
      opciones.resolucionDropdown.transform,
      opciones.fullscreenToggle.transform,
      opciones.graficosDropdown.transform
    };
    if (vsync != null) { rescatados.Add(vsync.transform); }
    if (opciones.fpsLimitDropdown != null) { rescatados.Add(opciones.fpsLimitDropdown.transform); }
    if (brillo != null) { rescatados.Add(brillo.transform); }

    CalcularArea();

    // ---- filas con los controles existentes
    TMP_Dropdown dropdownBase = opciones.graficosDropdown;
    Toggle toggleBase = opciones.fullscreenToggle;

    int filas = 14;
    alturaFila = Mathf.Clamp((areaUtil.height - AltoHint - SeparacionFilas * (filas - 1)) / filas, 22f, 34f);
    cursorY = areaUtil.yMax;

    ReubicarDropdown(opciones.resolucionDropdown, () => T("Resolución", "Resolution", "Resolução"));
    ReubicarToggle(opciones.fullscreenToggle, () => T("Pantalla completa", "Fullscreen", "Tela cheia"));
    if (vsync != null)
    {
      ReubicarToggle(vsync, () => T("Sincronización vertical (VSync)", "Vertical sync (VSync)", "Sincronização vertical (VSync)"));
    }
    if (opciones.fpsLimitDropdown != null)
    {
      ReubicarDropdown(opciones.fpsLimitDropdown, () => T("Límite de FPS", "FPS limit", "Limite de FPS"));
    }

    ddPreset = ReubicarDropdown(opciones.graficosDropdown, () => T("Calidad gráfica", "Graphics quality", "Qualidade gráfica"));
    if (brillo != null)
    {
      ReubicarSlider(brillo, () => T("Brillo", "Brightness", "Brilho"));
    }

    // ---- opciones nuevas
    tgPostFx = CrearToggle(toggleBase, () => T("Postprocesado (color y efectos)", "Post-processing (color & effects)", "Pós-processamento (cor e efeitos)"), OnPostFx);
    ddAA = CrearDropdown(dropdownBase, () => T("Anti-aliasing", "Anti-aliasing", "Anti-aliasing"), OnAA);
    ddSombras = CrearDropdown(dropdownBase, () => T("Sombras", "Shadows", "Sombras"), OnSombras);
    ddTexturas = CrearDropdown(dropdownBase, () => T("Texturas", "Textures", "Texturas"), OnTexturas);
    ddParticulas = CrearDropdown(dropdownBase, () => T("Densidad de partículas", "Particle density", "Densidade de partículas"), OnParticulas);
    tgBloom = CrearToggle(toggleBase, () => T("Bloom (resplandor)", "Bloom (glow)", "Bloom (brilho)"), OnBloom);
    tgAO = CrearToggle(toggleBase, () => T("Oclusión ambiental", "Ambient occlusion", "Oclusão ambiental"), OnAO);
    tgDoF = CrearToggle(toggleBase, () => T("Profundidad de campo", "Depth of field", "Profundidade de campo"), OnDoF);

    // Texto de ayuda al pie
    txtHint = CrearTexto(transform, "txtHint", 10f, TextAlignmentOptions.Center, ColorTenue);
    txtHint.raycastTarget = false;
    RectTransform rectHint = txtHint.rectTransform;
    rectHint.anchorMin = rectHint.anchorMax = new Vector2(0.5f, 0.5f);
    rectHint.pivot = new Vector2(0.5f, 1f);
    rectHint.sizeDelta = new Vector2(areaUtil.width, AltoHint);
    rectHint.anchoredPosition = new Vector2(areaUtil.center.x, cursorY);

    OcultarSobrantes(panelGraficos, rescatados);
    Refrescar();
  }

  private void CalcularArea()
  {
    RectTransform self = (RectTransform)transform;
    Rect util = self.rect;

    Transform raiz = opciones.transform;
    RectTransform fondo = raiz.Find("Fondo") as RectTransform;
    if (fondo != null)
    {
      Rect marco = RectEnLocal(self, fondo);
      float insetX = marco.width * 0.06f;
      float insetY = marco.height * 0.055f;
      // La columna de pestañas ocupa aproximadamente el tercio izquierdo del marco.
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

  private static T BuscarHijo<T>(Transform raiz, string fragmentoNombre) where T : Component
  {
    T[] todos = raiz.GetComponentsInChildren<T>(true);
    for (int i = 0; i < todos.Length; i++)
    {
      if (todos[i] != null && todos[i].name.ToLowerInvariant().Contains(fragmentoNombre))
      {
        return todos[i];
      }
    }
    return null;
  }

  private void OcultarSobrantes(Transform panelGraficos, List<Transform> rescatados)
  {
    for (int i = 0; i < panelGraficos.childCount; i++)
    {
      Transform hijo = panelGraficos.GetChild(i);
      if (hijo == transform) { continue; }
      if (rescatados.Contains(hijo)) { continue; }
      hijo.gameObject.SetActive(false);
    }
  }

  // ---------------------------------------------------------------- filas

  private RectTransform CrearFila(Func<string> texto)
  {
    GameObject objeto = new GameObject("Fila", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    objeto.layer = gameObject.layer;
    RectTransform fila = (RectTransform)objeto.transform;
    fila.SetParent(transform, false);
    fila.anchorMin = fila.anchorMax = new Vector2(0.5f, 0.5f);
    fila.pivot = new Vector2(0.5f, 1f);
    fila.sizeDelta = new Vector2(areaUtil.width, alturaFila);
    fila.anchoredPosition = new Vector2(areaUtil.center.x, cursorY);
    cursorY -= alturaFila + SeparacionFilas;

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

  private TMP_Dropdown ReubicarDropdown(TMP_Dropdown dropdown, Func<string> texto)
  {
    RectTransform fila = CrearFila(texto);
    Colocar((RectTransform)dropdown.transform, fila, AnchoControl, AltoDropdown);
    return dropdown;
  }

  private void ReubicarToggle(Toggle toggle, Func<string> texto)
  {
    RectTransform fila = CrearFila(texto);
    OcultarTextosInternos(toggle.transform);
    Colocar((RectTransform)toggle.transform, fila, 24f, 24f);
  }

  private void ReubicarSlider(Slider slider, Func<string> texto)
  {
    RectTransform fila = CrearFila(texto);
    Colocar((RectTransform)slider.transform, fila, AnchoControl, 20f);
  }

  private TMP_Dropdown CrearDropdown(TMP_Dropdown plantilla, Func<string> texto, UnityEngine.Events.UnityAction<int> alCambiar)
  {
    RectTransform fila = CrearFila(texto);
    GameObject clon = Instantiate(plantilla.gameObject, fila);
    clon.name = "Dropdown";
    TMP_Dropdown dropdown = clon.GetComponent<TMP_Dropdown>();
    dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent(); // descarta los eventos persistentes de la plantilla
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
    OcultarTextosInternos(clon.transform);
    Colocar((RectTransform)clon.transform, fila, 24f, 24f);
    toggle.onValueChanged.AddListener(alCambiar);
    return toggle;
  }

  private static void OcultarTextosInternos(Transform raiz)
  {
    TMP_Text[] tmp = raiz.GetComponentsInChildren<TMP_Text>(true);
    for (int i = 0; i < tmp.Length; i++) { tmp[i].gameObject.SetActive(false); }
    Text[] legacy = raiz.GetComponentsInChildren<Text>(true);
    for (int i = 0; i < legacy.Length; i++) { legacy[i].gameObject.SetActive(false); }
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

  // ---------------------------------------------------------------- sincronizacion

  private void OnEnable()
  {
    Refrescar();
  }

  public void Refrescar()
  {
    if (ddPreset == null) { return; }
    refrescando = true;
    try
    {
      for (int i = 0; i < etiquetas.Count; i++)
      {
        etiquetas[i].texto.text = etiquetas[i].contenido();
      }

      int idioma = TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol;
      List<string> niveles = new List<string>();
      for (int i = 0; i < GraphicsQuality.LevelCount; i++) { niveles.Add(GraphicsQuality.LevelName(i, idioma)); }
      niveles.Add(T("Personalizada", "Custom", "Personalizada"));
      LlenarDropdown(ddPreset, niveles, GraphicsQuality.HasOverrides ? GraphicsQuality.LevelCount : GraphicsQuality.Level);

      LlenarDropdown(ddAA, new List<string> { T("Desactivado", "Off", "Desativado"), "FXAA", "SMAA" }, (int)GraphicsQuality.GetAntiAliasing());
      LlenarDropdown(ddSombras, new List<string>
      {
        T("Desactivadas", "Off", "Desativadas"), T("Bajas", "Low", "Baixas"), T("Medias", "Medium", "Médias"), T("Altas", "High", "Altas")
      }, (int)GraphicsQuality.GetShadows());
      LlenarDropdown(ddTexturas, new List<string> { T("Bajas", "Low", "Baixas"), T("Medias", "Medium", "Médias"), T("Altas", "High", "Altas") }, (int)GraphicsQuality.GetTextures());
      LlenarDropdown(ddParticulas, new List<string> { T("Baja", "Low", "Baixa"), T("Media", "Medium", "Média"), T("Alta", "High", "Alta") }, (int)GraphicsQuality.GetParticles());

      GraphicsQuality.Settings s = GraphicsQuality.Current;
      tgPostFx.SetIsOnWithoutNotify(s.postProcessing);
      tgBloom.SetIsOnWithoutNotify(s.bloom);
      tgAO.SetIsOnWithoutNotify(s.aoBattle || s.aoCampaign);
      tgDoF.SetIsOnWithoutNotify(s.dof);

      // Sin postprocesado no hay AA ni bloom ni oclusion ni profundidad de campo.
      bool conPostFx = s.postProcessing;
      ddAA.interactable = conPostFx;
      tgBloom.interactable = conPostFx;
      tgAO.interactable = conPostFx;
      tgDoF.interactable = conPostFx;

      txtHint.text = T("Cambiar un detalle suelto pasa la calidad a «Personalizada».",
        "Changing an individual detail sets the quality to “Custom”.",
        "Alterar um detalhe isolado muda a qualidade para “Personalizada”.");
    }
    finally
    {
      refrescando = false;
    }
  }

  private static void LlenarDropdown(TMP_Dropdown dropdown, List<string> textos, int valor)
  {
    bool mismaLista = dropdown.options.Count == textos.Count;
    if (!mismaLista)
    {
      dropdown.ClearOptions();
      dropdown.AddOptions(textos);
    }
    else
    {
      for (int i = 0; i < textos.Count; i++) { dropdown.options[i].text = textos[i]; }
    }

    dropdown.SetValueWithoutNotify(Mathf.Clamp(valor, 0, textos.Count - 1));
    dropdown.RefreshShownValue();
  }

  // ---------------------------------------------------------------- eventos de opciones nuevas

  // Llamado por OpcionesCargarPlayerPrefsUI.AplicarGraficos (dropdown de calidad).
  public void AlElegirPreset(int indice)
  {
    if (refrescando) { return; }

    if (indice < GraphicsQuality.LevelCount)
    {
      GraphicsQuality.SetLevel(indice, true);
      AplicarTodo();
      return;
    }

    // "Personalizada" no es una calidad que se pueda elegir: solo refleja que hay detalles modificados.
    Refrescar();
  }

  private void OnPostFx(bool valor) { if (refrescando) { return; } GraphicsQuality.PostProcessingEnabled = valor; AplicarTodo(); }
  private void OnAA(int indice) { if (refrescando) { return; } GraphicsQuality.SetAntiAliasing((GraphicsQuality.AntiAliasing)indice); AplicarTodo(); }
  private void OnSombras(int indice) { if (refrescando) { return; } GraphicsQuality.SetShadows((GraphicsQuality.ShadowOption)indice); AplicarTodo(); }
  private void OnTexturas(int indice) { if (refrescando) { return; } GraphicsQuality.SetTextures((GraphicsQuality.TextureOption)indice); AplicarTodo(); }
  private void OnParticulas(int indice) { if (refrescando) { return; } GraphicsQuality.SetParticles((GraphicsQuality.ParticleOption)indice); AplicarTodo(); }
  private void OnBloom(bool valor) { if (refrescando) { return; } GraphicsQuality.SetBloom(valor); AplicarTodo(); }
  private void OnAO(bool valor) { if (refrescando) { return; } GraphicsQuality.SetAmbientOcclusion(valor); AplicarTodo(); }
  private void OnDoF(bool valor) { if (refrescando) { return; } GraphicsQuality.SetDepthOfField(valor); AplicarTodo(); }

  private void AplicarTodo()
  {
    VisualPolishRuntime.ApplyPostProcessingPrefsNow();
    Refrescar();
  }

  private static string T(string es, string en, string pt)
  {
    return SaveSlotsLoc.T(es, en, pt);
  }
}
