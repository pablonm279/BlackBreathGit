using System;
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

    [Header("Cola y superposicion")]
    public int maxTextosSimultaneos = 4;
    public float separacionVertical = 18f;
    public float separacionHorizontal = 0f;
    public float esperaMaximaSlot = 0.35f;

    [Header("Parametros fallback")]
    public float duracion = 1.5f;
    public float desplazamientoY = 50f;
    public float retrasoEntreTextos = 0.001f;

    private readonly Queue<FloatingTextRequest> colaTextos = new Queue<FloatingTextRequest>();
    private bool procesandoCola;
    private int textosActivos;

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
            yield return EsperarSlotDisponible();

            FloatingTextRequest request = colaTextos.Dequeue();
            LanzarTexto(request);
            yield return new WaitForSeconds(retrasoEntreTextos);
        }

        while (textosActivos > 0)
        {
            yield return null;
        }

        procesandoCola = false;
    }

    private IEnumerator EsperarSlotDisponible()
    {
        float waited = 0f;
        int maxSlots = Mathf.Max(1, maxTextosSimultaneos);
        while (textosActivos >= maxSlots)
        {
            waited += Time.deltaTime;
            if (waited >= esperaMaximaSlot)
            {
                break;
            }
            yield return null;
        }
    }

    private void LanzarTexto(FloatingTextRequest request)
    {
        int slotIndex = Mathf.Clamp(textosActivos, 0, Mathf.Max(0, maxTextosSimultaneos - 1));
        textosActivos = Mathf.Max(0, textosActivos + 1);
        StartCoroutine(SpawnTexto(request, slotIndex, () => { textosActivos = Mathf.Max(0, textosActivos - 1); }));
    }

    private IEnumerator SpawnTexto(FloatingTextRequest request, int slotIndex, Action onFinish)
    {
        GameObject goTexto = Instantiate(prefabTexto, contenedor, false);
        try
        {
            RectTransform rect = goTexto.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 offset = new Vector2(separacionHorizontal * slotIndex, separacionVertical * slotIndex);
                rect.anchoredPosition += offset;
            }

            FloatingTextAnimator animator = goTexto.GetComponent<FloatingTextAnimator>();

            if (animator != null)
            {
                if (rect != null)
                {
                    animator.SetBasePosition(rect.anchoredPosition);
                }

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
        }
        finally
        {
            if (goTexto != null)
            {
                Destroy(goTexto);
            }
            onFinish?.Invoke();
        }
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
