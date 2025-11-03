using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextoFlotanteManager : MonoBehaviour
{
    public static TextoFlotanteManager Instance { get; private set; }

    [Header("Prefab de texto flotante")]
    public GameObject prefabTexto;
    public Transform contenedor;

    [Header("Parametros fallback")]
    public float duracion = 1.5f;
    public float desplazamientoY = 50f;
    public float retrasoEntreTextos = 0.001f;

    private readonly Queue<FloatingTextRequest> colaTextos = new Queue<FloatingTextRequest>();
    private bool procesandoCola;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Genera un texto flotante en la UI
    /// </summary>
    public void GenerarTextoFlotante(string texto, Color color)
    {
        GenerarTextoFlotante(texto, color, FloatingTextContext.Generic);
    }

    public void GenerarTextoFlotante(string texto, Color color, FloatingTextContext contexto)
    {  
         if(texto == "-0") { texto = ""; }

        colaTextos.Enqueue(new FloatingTextRequest(texto, color, contexto));

        if (!procesandoCola)
        {
            StartCoroutine(ProcesarCola());
        }
    }

    private IEnumerator ProcesarCola()
    {
        procesandoCola = true;

        while (colaTextos.Count > 0)
        {
            FloatingTextRequest request = colaTextos.Dequeue();
            yield return StartCoroutine(SpawnTexto(request));
            yield return new WaitForSeconds(retrasoEntreTextos);
        }

        procesandoCola = false;
    }

    private IEnumerator SpawnTexto(FloatingTextRequest request)
    {
        GameObject goTexto = Instantiate(prefabTexto, contenedor, false);
        FloatingTextAnimator animator = goTexto.GetComponent<FloatingTextAnimator>();

        if (animator != null)
        {
            yield return animator.PlayRoutine(request.Texto, request.Color, request.Contexto);
        }
        else
        {
            TextMeshProUGUI tmp = goTexto.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = request.Texto;
                tmp.color = request.Color;
            }

            Vector3 startPos = goTexto.transform.localPosition;
            Vector3 endPos = startPos + new Vector3(0f, desplazamientoY, 0f);

            float t = 0f;
            Color startColor = tmp != null ? tmp.color : request.Color;
            Color endColor = new Color(request.Color.r, request.Color.g, request.Color.b, 0f);

            while (t < duracion)
            {
                t += Time.deltaTime;
                float p = t / duracion;

                goTexto.transform.localPosition = Vector3.Lerp(startPos, endPos, p);
                if (tmp != null)
                {
                    tmp.color = Color.Lerp(startColor, endColor, p);
                }

                yield return null;
            }
        }

        Destroy(goTexto);
    }

    private readonly struct FloatingTextRequest
    {
        public string Texto { get; }
        public Color Color { get; }
        public FloatingTextContext Contexto { get; }

        public FloatingTextRequest(string texto, Color color, FloatingTextContext contexto)
        {
            Texto = texto;
            Color = color;
            Contexto = contexto;
        }
    }
}
