using UnityEngine;

public class ModoCavernaOscuro : MonoBehaviour {
    void Start() {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.03f, 0.035f, 0.045f); // #07090C aprox
        RenderSettings.reflectionIntensity = 0.1f; // menos rebote especular
      
    }
}
