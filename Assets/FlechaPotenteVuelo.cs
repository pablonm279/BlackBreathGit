using UnityEngine;

public class FlechaPotenteVuelo : MonoBehaviour
{
    [Header("Parámetros de vuelo")]
    public float velocidad = 5f;     // unidades por segundo
    public float duracion = 2f;      // tiempo antes de destruirse

    private void OnEnable()
    {
        StartCoroutine(MoverYDestruir());
    }

    private System.Collections.IEnumerator MoverYDestruir()
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            transform.Translate(Vector3.right * velocidad * Time.deltaTime);
            tiempo += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}