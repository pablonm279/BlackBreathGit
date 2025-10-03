using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class FlechaPotenteVuelo : MonoBehaviour
{
    [Header("Par�metros de vuelo")]
    public float velocidad = 5f;     // unidades por segundo
    public float duracion = 2f;      // tiempo antes de destruirse

    Vector3 direccionMovimiento = Vector3.right;
    TaskCompletionSource<bool> completadoTcs;

    private void OnEnable()
    {
        completadoTcs = new TaskCompletionSource<bool>();
        direccionMovimiento = transform.right;
        StartCoroutine(MoverYDestruir());
    }

    public void Configure(Vector3 direccion)
    {
        if (direccion.sqrMagnitude > 0.0001f)
        {
            direccionMovimiento = direccion.normalized;
        }
        else
        {
            direccionMovimiento = transform.right;
        }
    }

    public Task EsperarFinalAsync()
    {
        return completadoTcs != null ? completadoTcs.Task : Task.CompletedTask;
    }

    private IEnumerator MoverYDestruir()
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            transform.position += direccionMovimiento * velocidad * Time.deltaTime;
            tiempo += Time.deltaTime;
            yield return null;
        }

        completadoTcs?.TrySetResult(true);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        completadoTcs?.TrySetResult(true);
    }
}
