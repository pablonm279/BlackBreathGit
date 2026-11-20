using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel de Gameplay del menu de opciones. Se construye por codigo dentro de PanelGameplay:
//  * Reubica los controles que ya existian (dificultad, tamaño de texto y banter) en filas "etiqueta + control".
//  * Agrega la velocidad de combate, "acelerar solo los turnos enemigos" y la velocidad de la caravana (ver GameSpeedSettings).
// El interruptor viejo de "Modo rapido" desaparece: lo reemplaza la velocidad de combate.
public class GameplayOptionsPanel : MonoBehaviour
{
  private const string NombreContenedor = "OpcionesJuego";
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
  private readonly List<Etiqueta> etiquetas = new List<Etiqueta>();

  private TMP_Dropdown ddCombate;
  private Toggle tgSoloEnemigos;
  private TMP_Dropdown ddCaravana;
  private TextMeshProUGUI txtNota;

  private Rect areaUtil;
  private float cursorY;
  private bool refrescando;

  public static GameplayOptionsPanel Asegurar(OpcionesCargarPlayerPrefsUI opciones)
  {
    if (opciones == null || opciones.PanelGameplay == null)
    {
      return null;
    }

    Transform existente = opciones.PanelGameplay.transform.Find(NombreContenedor);
    if (existente != null)
    {
      return existente.GetComponent<GameplayOptionsPanel>();
    }

    if (opciones.dificultadDropdown == null || opciones.fullscreenToggle == null)
    {
      return null;
    }

    GameObject objeto = new GameObject(NombreContenedor, typeof(RectTransform));
    objeto.layer = opciones.PanelGameplay.layer;
    RectTransform rect = (RectTransform)objeto.transform;
    rect.SetParent(opciones.PanelGameplay.transform, false);
    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = Vector2.zero;
    rect.sizeDelta = ((RectTransform)opciones.PanelGameplay.transform).sizeDelta;

    GameplayOptionsPanel panel = objeto.AddComponent<GameplayOptionsPanel>();
    panel.Construir(opciones);
    return panel;
  }

  // ---------------------------------------------------------------- construccion

  private void Construir(OpcionesCargarPlayerPrefsUI opcionesPadre)
  {
    opciones = opcionesPadre;
    Transform panelGameplay = opciones.PanelGameplay.transform;

    TMP_Text textoBase = opciones.dificultadDropdown.GetComponentInChildren<TMP_Text>(true);
    if (textoBase != null)
    {
      fuente = textoBase.font;
      materialFuente = textoBase.fontSharedMaterial;
    }

    CalcularArea();
    cursorY = areaUtil.yMax;

    Slider sliderTexto = opciones.EscalaTextoSliderActual;
    Toggle toggleBanter = opciones.BanterToggleActual;
    TMP_Dropdown plantillaDropdown = opciones.dificultadDropdown;

    List<Transform> rescatados = new List<Transform> { opciones.dificultadDropdown.transform };
    if (sliderTexto != null) { rescatados.Add(sliderTexto.transform); }
    if (toggleBanter != null) { rescatados.Add(toggleBanter.transform); }

    // ---- filas
    RectTransform filaDificultad = CrearFila(() => T("Dificultad", "Difficulty", "Dificuldade"));
    Colocar((RectTransform)opciones.dificultadDropdown.transform, filaDificultad, AnchoControl, AltoDropdown);

    ddCombate = CrearDropdown(plantillaDropdown, () => T("Velocidad de combate", "Combat speed", "Velocidade de combate"), OnCombate);
    tgSoloEnemigos = CrearToggle(opciones.fullscreenToggle, () => T("Acelerar solo los turnos enemigos", "Speed up enemy turns only", "Acelerar só os turnos inimigos"), OnSoloEnemigos);
    ddCaravana = CrearDropdown(plantillaDropdown, () => T("Velocidad de la caravana", "Caravan speed", "Velocidade da caravana"), OnCaravana);

    if (sliderTexto != null)
    {
      RectTransform fila = CrearFila(() => opciones.TextoEtiquetaEscalaTexto);
      Colocar((RectTransform)sliderTexto.transform, fila, AnchoControl, 20f);
    }

    if (toggleBanter != null)
    {
      RectTransform fila = CrearFila(() => opciones.TextoEtiquetaBanter);
      OcultarTextosInternos(toggleBanter.transform);
      Colocar((RectTransform)toggleBanter.transform, fila, 24f, 24f);
    }

    // Nota al pie
    txtNota = CrearTexto(transform, "txtNota", 10f, TextAlignmentOptions.TopLeft, ColorTenue);
    txtNota.raycastTarget = false;
    txtNota.textWrappingMode = TextWrappingModes.Normal;
    RectTransform rectNota = txtNota.rectTransform;
    rectNota.anchorMin = rectNota.anchorMax = new Vector2(0.5f, 0.5f);
    rectNota.pivot = new Vector2(0.5f, 1f);
    rectNota.sizeDelta = new Vector2(areaUtil.width - 12f, 90f);
    rectNota.anchoredPosition = new Vector2(areaUtil.center.x, cursorY - 4f);

    OcultarSobrantes(panelGameplay, rescatados);
    GameSpeedSettings.Changed += OnAjustesCambiados;
    Refrescar();
  }

  private void OnDestroy()
  {
    GameSpeedSettings.Changed -= OnAjustesCambiados;
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

  private void OcultarSobrantes(Transform panelGameplay, List<Transform> rescatados)
  {
    for (int i = 0; i < panelGameplay.childCount; i++)
    {
      Transform hijo = panelGameplay.GetChild(i);
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

  private void OnAjustesCambiados()
  {
    if (isActiveAndEnabled) { Refrescar(); }
  }

  // Las etiquetas del tamaño de texto y del banter las actualiza el menu cuando cambia su control:
  // aqui se copian a la fila sin depender del orden de los listeners.
  private void Update()
  {
    for (int i = 0; i < etiquetas.Count; i++)
    {
      string contenido = etiquetas[i].contenido();
      if (etiquetas[i].texto.text != contenido) { etiquetas[i].texto.text = contenido; }
    }
  }

  public void Refrescar()
  {
    if (ddCombate == null) { return; }
    refrescando = true;
    try
    {
      List<string> velocidades = new List<string>
      {
        T("Normal", "Normal", "Normal"),
        T("Rápida", "Fast", "Rápida"),
        T("Muy rápida", "Very fast", "Muito rápida")
      };

      LlenarDropdown(ddCombate, velocidades, GameSpeedSettings.CombatSpeedIndex);
      LlenarDropdown(ddCaravana, velocidades, GameSpeedSettings.CaravanLevel);

      tgSoloEnemigos.SetIsOnWithoutNotify(GameSpeedSettings.CombatOnlyEnemies);
      // Sin velocidad de combate elevada no hay nada que limitar a los turnos enemigos.
      tgSoloEnemigos.interactable = GameSpeedSettings.CombatSpeedIndex > 0;

      RefrescarEtiquetas();

      txtNota.text = T(
        "El botón de velocidad de la batalla alterna entre velocidad normal y la elegida aquí. Con «solo turnos enemigos», tus turnos van a velocidad normal. "
        + "La velocidad de la caravana también acelera el tiempo durante el viaje: el trayecto cuesta las mismas horas de juego. El tutorial siempre usa velocidad normal.",
        "The battle speed button switches between normal speed and the one chosen here. With “enemy turns only”, your turns run at normal speed. "
        + "Caravan speed also speeds up in-game time while traveling: a trip costs the same game hours. The tutorial always runs at normal speed.",
        "O botão de velocidade da batalha alterna entre a velocidade normal e a escolhida aqui. Com “só turnos inimigos”, seus turnos vão em velocidade normal. "
        + "A velocidade da caravana também acelera o tempo durante a viagem: o trajeto custa as mesmas horas de jogo. O tutorial sempre usa velocidade normal.");
    }
    finally
    {
      refrescando = false;
    }
  }

  private void RefrescarEtiquetas()
  {
    for (int i = 0; i < etiquetas.Count; i++) { etiquetas[i].texto.text = etiquetas[i].contenido(); }
  }

  private static void LlenarDropdown(TMP_Dropdown dropdown, List<string> textos, int valor)
  {
    if (dropdown.options.Count != textos.Count)
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

  // ---------------------------------------------------------------- eventos

  private void OnCombate(int indice)
  {
    if (refrescando) { return; }
    GameSpeedSettings.SetCombatSpeedIndex(indice);
  }

  private void OnSoloEnemigos(bool valor)
  {
    if (refrescando) { return; }
    GameSpeedSettings.CombatOnlyEnemies = valor;
    BattleManager batalla = BattleManager.Instance;
    if (batalla != null)
    {
      batalla.RefrescarEscalaTiempoCombate();
    }
  }

  private void OnCaravana(int indice)
  {
    if (refrescando) { return; }
    GameSpeedSettings.CaravanLevel = indice;
  }

  private static string T(string es, string en, string pt)
  {
    return SaveSlotsLoc.T(es, en, pt);
  }
}
