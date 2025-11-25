using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeObstaculo : MonoBehaviour
{
    SpriteRenderer sr;
    Material materialInstance;
    static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    static readonly int ColorProperty = Shader.PropertyToID("_Color");
    int activeColorProperty;

    [Header("Alpha")]
    public float alphaNormal = 1f;
    public float alphaOculto = 0.25f;
    public float velocidadFade = 5f;

    float alphaObjetivo;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        materialInstance = sr.material;
        activeColorProperty = materialInstance.HasProperty(BaseColorProperty) ? BaseColorProperty : ColorProperty;
        Color baseColor = materialInstance.GetColor(activeColorProperty);
        alphaNormal = baseColor.a;
        alphaObjetivo = alphaNormal;
    }

    public void Tapando()
    {
        alphaObjetivo = alphaOculto;
    }

    public void DejarDeTapar()
    {
        alphaObjetivo = alphaNormal;
    }

    void Update()
    {
        Color c = materialInstance.GetColor(activeColorProperty);
        float nuevoA = Mathf.MoveTowards(c.a, alphaObjetivo, velocidadFade * Time.deltaTime);
        c.a = nuevoA;
        materialInstance.SetColor(activeColorProperty, c);
    }

    // === NUEVO ===
    private void OnMouseEnter()
    {
        Tapando();
    }

    private void OnMouseExit()
    {
        DejarDeTapar();
    }
}
