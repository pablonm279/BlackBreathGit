using System.Collections;
using UnityEngine;
using TMPro;

public class Alcambiarvalorflash : MonoBehaviour
{
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float flashDuration = 0.2f;

    private TMP_Text tmp;
    private Color originalColor;
    private string lastText;
    private Coroutine flashRoutine;

    private void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        originalColor = tmp.color;
        lastText = tmp.text;
    }

    /*private void LateUpdate()
    {
        if (tmp.text == lastText)
        {
            return;
        }

        lastText = tmp.text;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(Flash());
    }*/
    public void Flash()
    {
        Flash(flashColor);
    }

    public void Flash(Color color)
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashStart(color));
    }

    private IEnumerator FlashStart(Color color)
    {
        tmp.color = color;
        float t = 0f;

        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float lerp = flashDuration <= 0f ? 1f : (t / flashDuration);
            tmp.color = Color.Lerp(color, originalColor, lerp);
            yield return null;
        }

        tmp.color = originalColor;
        flashRoutine = null;
    }
}
