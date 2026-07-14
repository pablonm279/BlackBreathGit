using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FloatingTextBackground : MonoBehaviour
{
    private const int TextureWidth = 64;
    private const int TextureHeight = 16;
    private const float DefaultAlpha = 0.8f;
    private static readonly Vector2 DefaultHorizontalPadding = new Vector2(2f, 4f);
    private const float DefaultVerticalPadding = 6f;

    private static Sprite sharedSprite;

    private TextMeshProUGUI targetText;
    private RectTransform targetRect;
    private RectTransform canvasRect;
    private RectTransform backgroundRect;
    private Image backgroundImage;
    private Vector2 horizontalPadding = DefaultHorizontalPadding;
    private float verticalPadding = DefaultVerticalPadding;
    private float alpha = DefaultAlpha;
    private float externalAlpha = 1f;

    public static FloatingTextBackground Attach(TextMeshProUGUI target)
    {
        if (target == null)
        {
            return null;
        }

        FloatingTextBackground background = target.GetComponent<FloatingTextBackground>();
        if (background == null)
        {
            background = target.gameObject.AddComponent<FloatingTextBackground>();
        }

        background.Initialize(target, DefaultHorizontalPadding, DefaultVerticalPadding, DefaultAlpha);
        return background;
    }

    private void Initialize(TextMeshProUGUI target, Vector2 horizontalPaddingValue, float verticalPaddingValue, float alphaValue)
    {
        targetText = target;
        targetRect = target.GetComponent<RectTransform>();
        Canvas canvas = target.GetComponentInParent<Canvas>();
        canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
        horizontalPadding = horizontalPaddingValue;
        verticalPadding = verticalPaddingValue;
        alpha = Mathf.Clamp01(alphaValue);
        externalAlpha = 1f;

        RectTransform parentRect = canvasRect != null ? canvasRect : targetRect != null ? targetRect.parent as RectTransform : null;
        if (targetRect == null || parentRect == null)
        {
            return;
        }

        if (backgroundRect == null)
        {
            GameObject backgroundGo = new GameObject("FondoTextoFlotanteCampania", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            backgroundRect = backgroundGo.GetComponent<RectTransform>();
            backgroundImage = backgroundGo.GetComponent<Image>();
            backgroundRect.SetParent(parentRect, false);
            backgroundRect.SetAsFirstSibling();
        }

        if (backgroundImage == null)
        {
            backgroundImage = backgroundRect.GetComponent<Image>();
        }

        backgroundImage.sprite = GetSharedSprite();
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.raycastTarget = false;
    }

    public void SetExternalAlpha(float value)
    {
        externalAlpha = Mathf.Clamp01(value);
    }

    private void LateUpdate()
    {
        RectTransform parentRect = canvasRect != null ? canvasRect : backgroundRect != null ? backgroundRect.parent as RectTransform : null;
        if (targetText == null || targetRect == null || parentRect == null || backgroundRect == null || backgroundImage == null)
        {
            return;
        }

        targetText.ForceMeshUpdate();
        Bounds textBounds = targetText.textBounds;
        Vector3 rightWorld = targetRect.TransformPoint(new Vector3(textBounds.max.x + horizontalPadding.y, textBounds.center.y, 0f));
        Vector3 bottomWorld = targetRect.TransformPoint(new Vector3(textBounds.center.x, textBounds.min.y - verticalPadding, 0f));
        Vector3 topWorld = targetRect.TransformPoint(new Vector3(textBounds.center.x, textBounds.max.y + verticalPadding, 0f));

        Vector2 rightLocal = parentRect.InverseTransformPoint(rightWorld);
        Vector2 bottomLocal = parentRect.InverseTransformPoint(bottomWorld);
        Vector2 topLocal = parentRect.InverseTransformPoint(topWorld);
        float leftX = parentRect.rect.xMin;
        float rightX = rightLocal.x;
        float centerY = (bottomLocal.y + topLocal.y) * 0.5f;
        float width = Mathf.Max(1f, rightX - leftX);
        float height = Mathf.Max(1f, Mathf.Abs(topLocal.y - bottomLocal.y));

        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(leftX, centerY);
        backgroundRect.localRotation = Quaternion.identity;
        backgroundRect.localScale = Vector3.one;
        backgroundRect.sizeDelta = new Vector2(width, height);

        Color backgroundColor = Color.black;
        backgroundColor.a = alpha * targetText.color.a * externalAlpha;
        backgroundImage.color = backgroundColor;
    }

    private void OnDestroy()
    {
        if (backgroundRect != null)
        {
            Destroy(backgroundRect.gameObject);
        }
    }

    private static Sprite GetSharedSprite()
    {
        if (sharedSprite != null)
        {
            return sharedSprite;
        }

        Texture2D texture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false)
        {
            name = "FloatingTextBackgroundGradient",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        for (int y = 0; y < TextureHeight; y++)
        {
            float vertical = Mathf.Clamp01(Mathf.Min(y + 1, TextureHeight - y) / 4f);
            vertical = Mathf.SmoothStep(0f, 1f, vertical);

            for (int x = 0; x < TextureWidth; x++)
            {
                float normalizedX = x / (float)(TextureWidth - 1);
                float rightFade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((normalizedX - 0.97f) / 0.03f));
                float alphaValue = vertical * rightFade;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alphaValue));
            }
        }

        texture.Apply(false, true);
        sharedSprite = Sprite.Create(texture, new Rect(0f, 0f, TextureWidth, TextureHeight), new Vector2(0.5f, 0.5f), 100f);
        return sharedSprite;
    }
}
