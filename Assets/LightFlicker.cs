using UnityEngine;
public class LightFlicker : MonoBehaviour {
    public Light L;
    public float baseIntensity = 1.6f;
    public float amplitude = 0.15f;   // 15%
    public float speed = 2.1f;
    void Reset(){ L = GetComponent<Light>(); }
    void Update(){
        float n = Mathf.PerlinNoise(Time.time * speed, 0f) * 2f - 1f;
        L.intensity = baseIntensity * (1f + n * amplitude);
    }
}
