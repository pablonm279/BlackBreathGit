using UnityEngine;

[ExecuteAlways]
public class Auroramov : MonoBehaviour
{
    public float scrollSpeedX = 0.02f;
    public float scrollSpeedY = 0.015f;
    public float rotationSpeed = 3f;
    private Material cookieMat;
    private Light auroraLight;
    private float offsetX, offsetY;

    void Start()
    {
        auroraLight = GetComponent<Light>();
      /*  if (auroraLight.cookie != null)
        {
            cookieMat = new Material(Shader.Find("Hidden/Internal-Cookie"));
            cookieMat.SetTexture("_CookieTex", auroraLight.cookie);
            auroraLight.cookie = cookieMat.GetTexture("_CookieTex");
        }*/
    }

    void Update()
    {
        // Pequeño movimiento
        offsetX += scrollSpeedX * Time.deltaTime;
        offsetY += scrollSpeedY * Time.deltaTime;

        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // Movimiento lateral para simular “viento de aurora”
        Vector3 pos = transform.position;
        pos.x = Mathf.Sin(Time.time * 0.2f) * 10f;
        transform.position = pos;
    }
}

