using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RotuloHabilidadEnemigaUI : MonoBehaviour
{
    private const string RutaFuente = "Fuentes/Fondamento/Fondamento-Regular SDF";
    private const float DuracionPredeterminada = 1.8f;
    private const float AnchoMinimo = 210f;
    private const float AnchoMaximo = 460f;
    private const float EscalaVisual = 0.5f;
    private const float OffsetVertical = 18f;

    private static RotuloHabilidadEnemigaUI instance;

    private RectTransform canvasRect;
    private RectTransform rotuloRect;
    private TextMeshProUGUI textoRotulo;
    private CanvasGroup canvasGroup;
    private Unidad unidadObjetivo;
    private RectTransform anclajeObjetivo;
    private Coroutine animacionActual;

    public static bool Mostrar(
        Unidad unidad,
        RectTransform anclaje,
        string texto,
        Color color,
        float duracion = DuracionPredeterminada)
    {
        if (unidad == null || string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        if (instance == null)
        {
            instance = CrearInstancia();
        }

        if (instance == null)
        {
            return false;
        }

        instance.MostrarInterno(unidad, anclaje, texto, color, duracion);
        return true;
    }

    private static RotuloHabilidadEnemigaUI CrearInstancia()
    {
        GameObject root = new GameObject(
            "RotuloHabilidadEnemigaCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(RotuloHabilidadEnemigaUI));

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32000;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RotuloHabilidadEnemigaUI controlador = root.GetComponent<RotuloHabilidadEnemigaUI>();
        controlador.ConstruirRotulo();
        return controlador;
    }

    private void ConstruirRotulo()
    {
        canvasRect = transform as RectTransform;

        GameObject rotulo = new GameObject(
            "RotuloHabilidadEnemiga",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(Image),
            typeof(Outline));
        rotulo.transform.SetParent(transform, false);

        rotuloRect = rotulo.GetComponent<RectTransform>();
        rotuloRect.anchorMin = new Vector2(0.5f, 0.5f);
        rotuloRect.anchorMax = new Vector2(0.5f, 0.5f);
        rotuloRect.pivot = new Vector2(0.5f, 0.5f);
        rotuloRect.sizeDelta = new Vector2(AnchoMinimo, 56f);

        canvasGroup = rotulo.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Image fondo = rotulo.GetComponent<Image>();
        fondo.color = new Color(0.012f, 0.006f, 0.005f, 0.98f);
        fondo.raycastTarget = false;

        Outline borde = rotulo.GetComponent<Outline>();
        borde.effectColor = new Color(0.64f, 0.25f, 0.12f, 0.95f);
        borde.effectDistance = new Vector2(1.5f, -1.5f);
        borde.useGraphicAlpha = true;

        CrearLineaDecorativa(rotulo.transform);
        CrearRomboDecorativo(rotulo.transform, false);
        CrearRomboDecorativo(rotulo.transform, true);

        GameObject texto = new GameObject("NombreHabilidad", typeof(RectTransform), typeof(TextMeshProUGUI));
        texto.transform.SetParent(rotulo.transform, false);

        RectTransform textoRect = texto.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(32f, 8f);
        textoRect.offsetMax = new Vector2(-32f, -8f);

        textoRotulo = texto.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset fuente = Resources.Load<TMP_FontAsset>(RutaFuente);
        if (fuente != null)
        {
            textoRotulo.font = fuente;
            textoRotulo.fontSharedMaterial = fuente.material;
        }

        textoRotulo.fontSize = 29f;
        textoRotulo.enableAutoSizing = true;
        textoRotulo.fontSizeMin = 21f;
        textoRotulo.fontSizeMax = 29f;
        textoRotulo.fontStyle = FontStyles.Bold;
        textoRotulo.alignment = TextAlignmentOptions.Center;
        textoRotulo.textWrappingMode = TextWrappingModes.Normal;
        textoRotulo.overflowMode = TextOverflowModes.Overflow;
        textoRotulo.characterSpacing = 1.5f;
        textoRotulo.raycastTarget = false;
        textoRotulo.richText = false;

        rotulo.SetActive(false);
    }

    private static void CrearLineaDecorativa(Transform parent)
    {
        GameObject linea = new GameObject("LineaSuperior", typeof(RectTransform), typeof(Image));
        linea.transform.SetParent(parent, false);

        RectTransform rect = linea.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(18f, -2f);
        rect.offsetMax = new Vector2(-18f, 0f);

        Image imagen = linea.GetComponent<Image>();
        imagen.color = new Color(0.9f, 0.48f, 0.2f, 0.9f);
        imagen.raycastTarget = false;
    }

    private static void CrearRomboDecorativo(Transform parent, bool derecha)
    {
        GameObject rombo = new GameObject(derecha ? "RomboDerecho" : "RomboIzquierdo", typeof(RectTransform), typeof(Image));
        rombo.transform.SetParent(parent, false);

        RectTransform rect = rombo.GetComponent<RectTransform>();
        float anchorX = derecha ? 1f : 0f;
        rect.anchorMin = new Vector2(anchorX, 0.5f);
        rect.anchorMax = new Vector2(anchorX, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(derecha ? -15f : 15f, 0f);
        rect.sizeDelta = new Vector2(9f, 9f);
        rect.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Image imagen = rombo.GetComponent<Image>();
        imagen.color = new Color(0.78f, 0.34f, 0.14f, 0.95f);
        imagen.raycastTarget = false;
    }

    private void MostrarInterno(Unidad unidad, RectTransform anclaje, string texto, Color color, float duracion)
    {
        if (animacionActual != null)
        {
            StopCoroutine(animacionActual);
        }

        unidadObjetivo = unidad;
        anclajeObjetivo = anclaje;

        color.a = 1f;
        textoRotulo.color = color;
        textoRotulo.text = texto.Trim().ToUpperInvariant();
        AjustarTamanio();

        rotuloRect.localScale = Vector3.one * EscalaVisual;
        canvasGroup.alpha = 0f;
        rotuloRect.gameObject.SetActive(true);
        ActualizarPosicion(0f);

        float duracionFinal = Mathf.Clamp(duracion > 0f ? duracion : DuracionPredeterminada, 1.5f, 2.2f);
        animacionActual = StartCoroutine(Animar(duracionFinal));
    }

    private void AjustarTamanio()
    {
        const float anchoInteriorMaximo = AnchoMaximo - 64f;
        Vector2 preferido = textoRotulo.GetPreferredValues(textoRotulo.text, anchoInteriorMaximo, 0f);
        float ancho = Mathf.Clamp(preferido.x + 64f, AnchoMinimo, AnchoMaximo);
        float alto = Mathf.Clamp(preferido.y + 20f, 56f, 92f);
        rotuloRect.sizeDelta = new Vector2(ancho, alto);
    }

    private IEnumerator Animar(float duracion)
    {
        const float tiempoEntrada = 0.14f;
        const float tiempoSalida = 0.26f;
        float transcurrido = 0f;

        while (transcurrido < duracion && unidadObjetivo != null)
        {
            float alpha;
            float escala;

            if (transcurrido < tiempoEntrada)
            {
                float t = Mathf.SmoothStep(0f, 1f, transcurrido / tiempoEntrada);
                alpha = t;
                escala = Mathf.Lerp(EscalaVisual * 0.88f, EscalaVisual * 1.03f, t);
            }
            else if (transcurrido > duracion - tiempoSalida)
            {
                float t = Mathf.Clamp01((transcurrido - (duracion - tiempoSalida)) / tiempoSalida);
                alpha = Mathf.SmoothStep(1f, 0f, t);
                escala = Mathf.Lerp(EscalaVisual, EscalaVisual * 0.98f, t);
            }
            else
            {
                alpha = 1f;
                escala = Mathf.Lerp(rotuloRect.localScale.x, EscalaVisual, 0.24f);
            }

            float progreso = Mathf.Clamp01(transcurrido / duracion);
            canvasGroup.alpha = alpha;
            rotuloRect.localScale = Vector3.one * escala;
            ActualizarPosicion(Mathf.Lerp(0f, 10f, progreso));

            transcurrido += Time.unscaledDeltaTime;
            yield return null;
        }

        Ocultar();
    }

    private void ActualizarPosicion(float elevacionAnimada)
    {
        if (canvasRect == null || unidadObjetivo == null)
        {
            return;
        }

        Vector2 puntoPantalla;
        if (anclajeObjetivo != null)
        {
            Canvas canvasAnclaje = anclajeObjetivo.GetComponentInParent<Canvas>();
            Camera camara = canvasAnclaje != null && canvasAnclaje.renderMode != RenderMode.ScreenSpaceOverlay
                ? (canvasAnclaje.worldCamera != null ? canvasAnclaje.worldCamera : Camera.main)
                : null;
            Vector3 bordeSuperior = anclajeObjetivo.TransformPoint(Vector3.up * anclajeObjetivo.rect.height * 0.5f);
            puntoPantalla = RectTransformUtility.WorldToScreenPoint(camara, bordeSuperior);
        }
        else
        {
            Camera camara = Camera.main;
            if (camara == null)
            {
                return;
            }

            puntoPantalla = camara.WorldToScreenPoint(unidadObjetivo.transform.position + Vector3.up * 1.25f);
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, puntoPantalla, null, out Vector2 puntoLocal))
        {
            rotuloRect.anchoredPosition = puntoLocal + Vector2.up * (OffsetVertical + elevacionAnimada);
        }
    }

    private void Ocultar()
    {
        canvasGroup.alpha = 0f;
        rotuloRect.localScale = Vector3.one * EscalaVisual;
        rotuloRect.gameObject.SetActive(false);
        unidadObjetivo = null;
        anclajeObjetivo = null;
        animacionActual = null;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
