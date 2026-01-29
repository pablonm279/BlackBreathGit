using UnityEngine;

public class FlareMenu : MonoBehaviour
{
    public float alphaMin = 0.15f;
    public float alphaMax = 0.25f;
    public float speed = 0.5f;

    private UnityEngine.UI.Image img;

    void Awake()
    {
        img = GetComponent<UnityEngine.UI.Image>();
    }

    void Update()
    {
        float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;
        Color c = img.color;
        c.a = Mathf.Lerp(alphaMin, alphaMax, t);
        img.color = c;
    }
}
