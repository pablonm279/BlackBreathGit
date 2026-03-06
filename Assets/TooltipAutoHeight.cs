using System.Collections;
using TMPro;
using UnityEngine;

public class TooltipAutoHeight : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform background;   // Background
    [SerializeField] private RectTransform paddingRect;  // Padding (si no usas, asigna Background)
    [SerializeField] private TextMeshProUGUI text;

    [Header("Size")]
    [SerializeField] private float fixedWidth = 420f;
    [SerializeField] private float minHeight = 60f;
    [SerializeField] private float maxHeight = 600f; // opcional: evita tooltips infinitos
    [Header("Fade")]
    [SerializeField] private bool fadeInOnEnable = true;
    [SerializeField] private float fadeInDuration = 0.12f;
    [SerializeField] private float fadeInDelay = 0f;
    private bool needsRefresh = true;
    private string lastText = null;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    public void SetText(string value)
    {
        text.text = value;
        needsRefresh = true;
        RefreshSizeIfNeeded();
    }

    private void OnEnable()
    {
        needsRefresh = true;
        RefreshSizeIfNeeded();
        StartFadeIn();
    }

    private void OnDisable()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void Update()
    {
        if (!background || !paddingRect || !text) return;
        if (!string.Equals(text.text, lastText))
        {
            needsRefresh = true;
        }
        RefreshSizeIfNeeded();
    }

    private void RefreshSizeIfNeeded()
    {
        if (!needsRefresh) return;
        RefreshSize();
        needsRefresh = false;
    }

    private void RefreshSize()
    {
        if (!background || !paddingRect || !text) return;

        // Asegura ancho fijo del fondo
        var bgSize = background.sizeDelta;
        bgSize.x = fixedWidth;
        background.sizeDelta = bgSize;

        // Calcula el ancho real disponible para el texto (restando padding)
        float availableWidth = GetAvailableTextWidth();
        if (availableWidth <= 1f) availableWidth = fixedWidth;

        // Forzar TMP a recalcular
        text.ForceMeshUpdate();

        // preferred height del texto con wrapping para ese ancho
        Vector2 pref = text.GetPreferredValues(text.text, availableWidth, 0);

        float targetHeight = Mathf.Clamp(pref.y + GetVerticalPadding(), minHeight, maxHeight);

        // Aplicar altura (Pivot abajo => crece hacia arriba)
        background.sizeDelta = new Vector2(fixedWidth, targetHeight);

        // Forzar rebuild por si hay layouts arriba
        Canvas.ForceUpdateCanvases();

        lastText = text.text;
    }

    private void StartFadeIn()
    {
        if (!fadeInOnEnable) return;
        EnsureCanvasGroup();
        if (canvasGroup == null) return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (fadeInDuration <= 0f)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        canvasGroup.alpha = 0f;
        fadeRoutine = StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        if (fadeInDelay > 0f)
        {
            yield return new WaitForSeconds(fadeInDelay);
        }

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        fadeRoutine = null;
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null) return;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    float GetAvailableTextWidth()
    {
        // Usar fixedWidth y padding para evitar depender del layout en el primer frame
        float widthByPadding = fixedWidth - GetHorizontalPadding();
        if (widthByPadding > 1f) return widthByPadding;

        float rectWidth = paddingRect.rect.width;
        if (rectWidth > 1f) return rectWidth;

        return fixedWidth;
    }
    float GetHorizontalPadding()
    {
        float left = paddingRect.offsetMin.x;
        float right = paddingRect.offsetMax.x;
        return Mathf.Abs(left) + Mathf.Abs(right);
    }

    float GetVerticalPadding()
    {
        // Si usas Padding como rect que "encierra" al texto, el padding real está en Background - Padding
        // Calculamos diferencia de alturas por offsets:
        float top = paddingRect.offsetMax.y;      // negativo
        float bottom = paddingRect.offsetMin.y;   // positivo
        // offsetMax.y suele ser negativo (ej -15), offsetMin.y positivo (ej 15)
        return Mathf.Abs(top) + Mathf.Abs(bottom);
    }
}


