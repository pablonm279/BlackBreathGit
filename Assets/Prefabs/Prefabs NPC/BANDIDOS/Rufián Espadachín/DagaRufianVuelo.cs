using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DagaRufianVuelo : MonoBehaviour
{
    private const float DuracionVuelo = 0.48f;
    private const float LargoVisual = 0.82f;

    private Vector3 inicio;
    private Vector3 destino;
    private Transform visual;
    private TaskCompletionSource<bool> completado;

    public void Configurar(Vector3 posicionInicio, Vector3 posicionDestino)
    {
        inicio = posicionInicio;
        destino = posicionDestino;
        transform.position = inicio;
        completado = new TaskCompletionSource<bool>();
        CrearVisual();
        StartCoroutine(Volar());
    }

    public Task EsperarFinalAsync()
    {
        return completado != null ? completado.Task : Task.CompletedTask;
    }

    private void CrearVisual()
    {
        Sprite sprite = Resources.Load<Sprite>("VFX/daga_rufian");
        if (sprite == null)
        {
            Debug.LogWarning("No se encontró Resources/VFX/daga_rufian como Sprite.");
            return;
        }

        GameObject canvasObjeto = new GameObject("Visual Daga", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObjeto.transform.SetParent(transform, false);
        Canvas canvas = canvasObjeto.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 80;

        GameObject imagenObjeto = new GameObject("Daga", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imagenObjeto.transform.SetParent(canvasObjeto.transform, false);
        Image imagen = imagenObjeto.GetComponent<Image>();
        imagen.sprite = sprite;
        imagen.preserveAspect = true;
        imagen.raycastTarget = false;

        RectTransform rect = imagenObjeto.GetComponent<RectTransform>();
        float aspecto = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 2f;
        rect.sizeDelta = new Vector2(LargoVisual, LargoVisual / Mathf.Max(0.1f, aspecto));
        visual = canvasObjeto.transform;
        OrientarVisual();
        RenderOrderHelper.ForzarProyectilAlFrente(gameObject);
    }

    private IEnumerator Volar()
    {
        float tiempo = 0f;
        while (tiempo < DuracionVuelo)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / DuracionVuelo);
            transform.position = Vector3.Lerp(inicio, destino, Mathf.SmoothStep(0f, 1f, t));
            OrientarVisual();
            yield return null;
        }

        completado?.TrySetResult(true);
        Destroy(gameObject);
    }

    private void OrientarVisual()
    {
        if (visual == null || Camera.main == null) return;

        visual.rotation = Camera.main.transform.rotation;
        Vector3 direccionPantalla = Camera.main.WorldToScreenPoint(destino) - Camera.main.WorldToScreenPoint(inicio);
        float angulo = Mathf.Atan2(direccionPantalla.y, direccionPantalla.x) * Mathf.Rad2Deg;
        visual.Rotate(0f, 0f, angulo + 180f, Space.Self);
    }

    private void OnDisable()
    {
        completado?.TrySetResult(true);
    }
}
