using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextoFlotanteManager : MonoBehaviour
{
    public static TextoFlotanteManager Instance { get; private set; }

    [Header("Prefab de texto flotante")]
    public GameObject prefabTexto; // Asignar en inspector
    public Transform contenedor; // Asignar el Canvas padre

    [Header("Parámetros de animación")]
    public float duracion = 1.5f;
    public float desplazamientoY = 50f;
    public float retrasoEntreTextos = 0.15f;

    private Queue<IEnumerator> colaTextos = new Queue<IEnumerator>();
    private bool procesandoCola = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Genera un texto flotante en la UI
    /// </summary>
    public void GenerarTextoFlotante(string texto, Color color)
    {
        colaTextos.Enqueue(SpawnTexto(texto, color));

        if (!procesandoCola)
            StartCoroutine(ProcesarCola());
    }

    private IEnumerator ProcesarCola()
    {
        procesandoCola = true;

        while (colaTextos.Count > 0)
        {
            yield return StartCoroutine(colaTextos.Dequeue());
            yield return new WaitForSeconds(retrasoEntreTextos);
        }

        procesandoCola = false;
    }

   private IEnumerator SpawnTexto(string texto, Color color)
    {
        GameObject goTexto = Instantiate(prefabTexto, contenedor, false);
        TextMeshProUGUI tmp = goTexto.GetComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.color = color;

        Vector3 startPos = goTexto.transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, desplazamientoY, 0);

        float t = 0f;
        Color startColor = tmp.color;
        Color endColor = new Color(color.r, color.g, color.b, 0);

        while (t < duracion)
        {
            t += Time.deltaTime;
            float p = t / duracion;

            goTexto.transform.localPosition = Vector3.Lerp(startPos, endPos, p);
            tmp.color = Color.Lerp(startColor, endColor, p);

            yield return null;
        }

        Destroy(goTexto);
    }
}
