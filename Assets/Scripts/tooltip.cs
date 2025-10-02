using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;
    public RectTransform backgroundRectTransform;
    public Vector2 padding;

    void Start()
    {
        UpdateBackgroundSize();
    }

    void Update()
    {
        UpdateBackgroundSize();
    }

    void UpdateBackgroundSize()
    {
        Vector2 textSize = tooltipText.GetPreferredValues(tooltipText.text);
        backgroundRectTransform.sizeDelta = textSize + padding;
    }

    public void SetText(string text)
    {
        tooltipText.text = text;
        UpdateBackgroundSize();
    }
}
