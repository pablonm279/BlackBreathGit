using UnityEngine;

[ExecuteAlways]  // actualiza también en el editor
public class SunRaysBillboard : MonoBehaviour
{
    public Light sun;                 // Directional Light
    public Renderer mapaRenderer;     // MeshRenderer del plane
    public MeshRenderer quadRenderer; // MeshRenderer del quad (este GO)

    [Header("Colocación")]
    public float altura = 20f;        // Y por encima del mapa
    public float sizeFactor = 1.2f;   // tamaño relativo al ancho del mapa
    public float uvScroll = 0.02f;    // vida sutil

    void OnEnable()  { ColocarYAlinear(); }
    void LateUpdate(){ ColocarYAlinear(); }

    void ColocarYAlinear()
    {
        if (mapaRenderer)
        {
            var b = mapaRenderer.bounds;
            // centro del mapa y altura
           // transform.position = new Vector3(b.center.x, b.max.y + altura, b.center.z);
            // tamaño del quad ≈ ancho del mapa
            float s = Mathf.Max(b.size.x, b.size.z) * sizeFactor;
            transform.localScale = new Vector3(s, s, 1f);
        }

        // mirar a la cámara (billboard)
        var cam = Camera.main;
        if (cam) transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);

        // girar rayos según el Y del sol (derecha -> izquierda)
        if (sun) transform.Rotate(0f, 0f, -sun.transform.eulerAngles.y, Space.Self);

        // pequeño scroll de la textura
        if (quadRenderer)
        {
            var mat = quadRenderer.sharedMaterial; // shared para editor
            var off = mat.mainTextureOffset;
            off.x += uvScroll * (Application.isPlaying ? Time.deltaTime : 0.016f);
            mat.mainTextureOffset = off;
        }
    }
}
