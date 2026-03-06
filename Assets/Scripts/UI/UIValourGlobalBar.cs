using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIValourGlobalBar : MonoBehaviour
{
  [Header("Referencias")]
  [SerializeField] private Slider barra;
  [SerializeField] private Image fillBarra;
  [SerializeField] private Image fondoBarra;
  [SerializeField] private TextMeshProUGUI txtPorcentaje;
  [SerializeField] private TextMeshProUGUI txtEstado;
  [SerializeField] private GameObject goTextoBonusActual;
  [SerializeField] private TextMeshProUGUI txtBonusActual;

  [Header("Animacion")]
  [SerializeField] private bool animarCambios = true;
  [SerializeField, Range(1f, 25f)] private float velocidadAnimacion = 10f;

  [Header("Color (verde/celeste mate)")]
  [SerializeField] private Color colorMuyBajo = new Color(0.30f, 0.58f, 0.60f, 1f);
  [SerializeField] private Color colorBajo = new Color(0.35f, 0.66f, 0.68f, 1f);
  [SerializeField] private Color colorMedio = new Color(0.40f, 0.74f, 0.73f, 1f);
  [SerializeField] private Color colorAlto = new Color(0.48f, 0.82f, 0.77f, 1f);
  [SerializeField] private Color colorMuyAlto = new Color(0.58f, 0.90f, 0.82f, 1f);
  [SerializeField] private Color colorFondo = new Color(0.10f, 0.20f, 0.22f, 0.75f);

  [Header("Texto (alto contraste)")]
  [SerializeField] private bool usarColorPartidoPorFill = true;
  [SerializeField] private Color colorTexto = new Color(0.07f, 0.13f, 0.14f, 1f); // Dentro del fill
  [SerializeField] private Color colorTextoFuera = new Color(0.96f, 0.99f, 0.98f, 1f); // Fuera del fill
  [SerializeField] private Color colorContornoTexto = new Color(0.02f, 0.05f, 0.06f, 0.85f);
  [SerializeField, Range(0f, 1f)] private float anchoContornoTexto = 0.14f;
  [Header("Texto Bonus (Bueno/Malo)")]
  [SerializeField] private Color colorBonusBueno = new Color(0.27f, 0.88f, 0.47f, 1f);
  [SerializeField] private Color colorBonusMalo = new Color(0.90f, 0.30f, 0.30f, 1f);

  private BattleManager battleManager;
  private bool suscripto;
  private bool visualInicializada;
  private float valourVisualActual = 50f;
  private float valourObjetivo = 50f;
  private readonly Vector3[] cornersBuffer = new Vector3[4];

  private void OnEnable()
  {
    IntentarSuscribir();
  }

  private void OnDisable()
  {
    Desuscribir();
  }

  private void Update()
  {
    if (!suscripto)
    {
      IntentarSuscribir();
    }

    if (!animarCambios || !visualInicializada)
    {
      if (visualInicializada)
      {
        ActualizarVisual(valourObjetivo);
      }
      return;
    }

    if (Mathf.Abs(valourVisualActual - valourObjetivo) < 0.02f)
    {
      return;
    }

    float vel = Mathf.Max(1f, velocidadAnimacion);
    valourVisualActual = Mathf.Lerp(valourVisualActual, valourObjetivo, Time.unscaledDeltaTime * vel);
    if (Mathf.Abs(valourVisualActual - valourObjetivo) < 0.1f)
    {
      valourVisualActual = valourObjetivo;
    }

    ActualizarVisual(valourVisualActual);
  }

  private void IntentarSuscribir()
  {
    if (suscripto || BattleManager.Instance == null)
    {
      return;
    }

    battleManager = BattleManager.Instance;
    battleManager.OnValourGlobalAliadosCambiado += OnValourGlobalCambiado;
    suscripto = true;
    OnValourGlobalCambiado(battleManager.ObtenerValourGlobalAliadosPctActual());
  }

  private void Desuscribir()
  {
    if (!suscripto || battleManager == null)
    {
      return;
    }

    battleManager.OnValourGlobalAliadosCambiado -= OnValourGlobalCambiado;
    suscripto = false;
    battleManager = null;
  }

  private void OnValourGlobalCambiado(float pct)
  {
    valourObjetivo = Mathf.Clamp(pct, 0f, 100f);
    if (!visualInicializada)
    {
      visualInicializada = true;
      valourVisualActual = valourObjetivo;
      ActualizarVisual(valourVisualActual);
    }
  }

  private void ActualizarVisual(float pct)
  {
    float pctClamped = Mathf.Clamp(pct, 0f, 100f);
    float normalizado = pctClamped * 0.01f;
    Color colorActual = ObtenerColorValour(pctClamped);
    bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

    if (barra != null)
    {
      float min = barra.minValue;
      float max = barra.maxValue;
      if (Mathf.Abs(max - min) > 0.001f)
      {
        barra.value = Mathf.Lerp(min, max, normalizado);
      }
      else
      {
        barra.value = normalizado;
      }
    }

    if (fillBarra != null)
    {
      fillBarra.color = colorActual;
      if (barra == null)
      {
        fillBarra.fillAmount = normalizado;
      }
    }

    if (fondoBarra != null)
    {
      fondoBarra.color = colorFondo;
    }

    if (txtPorcentaje != null)
    {
      txtPorcentaje.text = Mathf.RoundToInt(pctClamped) + "%";
      AplicarEstiloTextoContraste(txtPorcentaje, normalizado);
    }

    if (txtEstado != null)
    {
      string prefijo = enIngles ? "Global Valour: " : "Valentía Global: ";
      txtEstado.text = prefijo + ObtenerTramoValour(pctClamped, enIngles);
      AplicarEstiloTextoContraste(txtEstado, normalizado);
    }

    TextMeshProUGUI textoBonus = ObtenerTextoBonusActual();
    if (textoBonus != null)
    {
      textoBonus.text = ObtenerTextoBonusActualValour(pctClamped, enIngles);

      bool esBueno = pctClamped >= 70f;
      bool esMalo = pctClamped < 40f;
      if (esBueno || esMalo)
      {
        AplicarColorPlanoTexto(textoBonus, esBueno ? colorBonusBueno : colorBonusMalo);
      }
      else
      {
        AplicarEstiloTextoContraste(textoBonus, normalizado);
      }
    }
  }

  private TextMeshProUGUI ObtenerTextoBonusActual()
  {
    if (txtBonusActual != null)
    {
      return txtBonusActual;
    }

    if (goTextoBonusActual == null)
    {
      return null;
    }

    txtBonusActual = goTextoBonusActual.GetComponent<TextMeshProUGUI>();
    if (txtBonusActual == null)
    {
      txtBonusActual = goTextoBonusActual.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    return txtBonusActual;
  }

  private string ObtenerTextoBonusActualValour(float pct, bool enIngles)
  {
    if (pct >= 90f)
    {
      return enIngles ? "Bonus: +15% Damage, +1 Max AP" : "Bonus: +15% Daño, +1 PA Max";
    }

    if (pct >= 70f)
    {
      return enIngles ? "Bonus: +1 Max AP" : "Bonus: +1 PA Max";
    }

    if (pct < 15f)
    {
      return enIngles ? "Penalty: Mental Save or flee" : "Penalidad: TS Mental o huye";
    }

    if (pct < 40f)
    {
      return enIngles ? "Penalty: Mental Save or Doubting" : "Penalidad: TS Mental o Dudando";
    }

    return enIngles ? "" : "";
  }

  private string ObtenerTramoValour(float pct, bool enIngles)
  {
    if (pct >= 90f) { return enIngles ? "Very High" : "Muy Alta"; }
    if (pct >= 70f) { return enIngles ? "High" : "Alta"; }
    if (pct < 15f) { return enIngles ? "Very Low" : "Muy Baja"; }
    if (pct < 40f) { return enIngles ? "Low" : "Baja"; }
    return enIngles ? "Mid" : "Media";
  }

  private Color ObtenerColorValour(float pct)
  {
    if (pct >= 90f) { return colorMuyAlto; }
    if (pct >= 70f) { return colorAlto; }
    if (pct < 15f) { return colorMuyBajo; }
    if (pct < 40f) { return colorBajo; }
    return colorMedio;
  }

  private void AplicarEstiloTextoContraste(TextMeshProUGUI texto, float fill01)
  {
    if (texto == null)
    {
      return;
    }

    texto.outlineColor = colorContornoTexto;
    texto.outlineWidth = anchoContornoTexto;

    if (!usarColorPartidoPorFill)
    {
      texto.color = colorTexto;
      return;
    }

    texto.color = Color.white;
    texto.ForceMeshUpdate();

    TMP_TextInfo textInfo = texto.textInfo;
    if (textInfo == null || textInfo.characterCount == 0)
    {
      return;
    }

    bool tieneLimiteMundo = TryObtenerLimiteFillMundoX(fill01, out float limiteFillMundoX);
    Bounds boundsTexto = texto.textBounds;
    float limiteFillLocalFallback = Mathf.Lerp(boundsTexto.min.x, boundsTexto.max.x, Mathf.Clamp01(fill01));
    Color32 colorDentro = colorTexto;
    Color32 colorFuera = colorTextoFuera;

    for (int i = 0; i < textInfo.characterCount; i++)
    {
      TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
      if (!charInfo.isVisible)
      {
        continue;
      }

      int materialIndex = charInfo.materialReferenceIndex;
      int vertexIndex = charInfo.vertexIndex;
      Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
      Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
      if (colors == null || vertexIndex + 3 >= colors.Length)
      {
        continue;
      }

      if (tieneLimiteMundo)
      {
        for (int v = 0; v < 4; v++)
        {
          Vector3 vMundo = texto.rectTransform.TransformPoint(vertices[vertexIndex + v]);
          colors[vertexIndex + v] = vMundo.x <= limiteFillMundoX ? colorDentro : colorFuera;
        }
      }
      else
      {
        for (int v = 0; v < 4; v++)
        {
          float vLocalX = vertices[vertexIndex + v].x;
          colors[vertexIndex + v] = vLocalX <= limiteFillLocalFallback ? colorDentro : colorFuera;
        }
      }
    }

    texto.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
  }

  private void AplicarColorPlanoTexto(TextMeshProUGUI texto, Color color)
  {
    if (texto == null)
    {
      return;
    }

    texto.outlineColor = colorContornoTexto;
    texto.outlineWidth = anchoContornoTexto;
    texto.color = color;
    texto.ForceMeshUpdate();

    TMP_TextInfo textInfo = texto.textInfo;
    if (textInfo == null || textInfo.characterCount == 0)
    {
      return;
    }

    Color32 color32 = color;
    for (int i = 0; i < textInfo.characterCount; i++)
    {
      TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
      if (!charInfo.isVisible)
      {
        continue;
      }

      int materialIndex = charInfo.materialReferenceIndex;
      int vertexIndex = charInfo.vertexIndex;
      Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
      if (colors == null || vertexIndex + 3 >= colors.Length)
      {
        continue;
      }

      colors[vertexIndex + 0] = color32;
      colors[vertexIndex + 1] = color32;
      colors[vertexIndex + 2] = color32;
      colors[vertexIndex + 3] = color32;
    }

    texto.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
  }

  private bool TryObtenerLimiteFillMundoX(float fill01, out float xMundo)
  {
    xMundo = 0f;

    if (fillBarra != null)
    {
      RectTransform fillRect = fillBarra.rectTransform;
      if (fillRect != null)
      {
        fillRect.GetWorldCorners(cornersBuffer);
        float left = cornersBuffer[0].x;
        float right = cornersBuffer[2].x;

        // Si el fill es tipo Filled, el Rect no cambia de ancho; usamos fillAmount.
        if (fillBarra.type == Image.Type.Filled)
        {
          float amount = Mathf.Clamp01(fillBarra.fillAmount);
          xMundo = Mathf.Lerp(left, right, amount);
        }
        else
        {
          // En Slider usual, el rect del fill ya representa el borde real de llenado.
          xMundo = right;
        }

        return true;
      }
    }

    RectTransform rectReferencia = null;
    if (fondoBarra != null)
    {
      rectReferencia = fondoBarra.rectTransform;
    }
    else if (barra != null)
    {
      rectReferencia = barra.GetComponent<RectTransform>();
    }
    else if (fillBarra != null)
    {
      rectReferencia = fillBarra.rectTransform;
    }

    if (rectReferencia == null)
    {
      return false;
    }

    rectReferencia.GetWorldCorners(cornersBuffer);
    float xMin = cornersBuffer[0].x;
    float xMax = cornersBuffer[2].x;
    xMundo = Mathf.Lerp(xMin, xMax, Mathf.Clamp01(fill01));
    return true;
  }
}



