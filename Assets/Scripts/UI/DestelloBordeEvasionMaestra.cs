using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public sealed class DestelloBordeEvasionMaestra : MonoBehaviour
{
    private const float DuracionVuelta = 2.8f;
    private const float GrosorBorde = 2.5f;
    private static readonly Color ColorBorde = new Color(0.9f, 0.95f, 1f, 0.24f);
    private static readonly Color ColorHalo = new Color(0.92f, 0.97f, 1f, 0.3f);
    private static readonly Color ColorNucleo = new Color(0.98f, 1f, 1f, 0.82f);
    private static Sprite spriteDestelloCircular;

    private RectTransform rectBoton;
    private RectTransform halo;
    private RectTransform nucleo;
    private GameObject borde;
    private GameObject veloDesactivado;
    private REPRESENTACIONEvasionMaestra habilidad;

    public void Inicializar(REPRESENTACIONEvasionMaestra evasionMaestra)
    {
        habilidad = evasionMaestra;
        ActualizarVisibilidad();
    }

    private void Awake()
    {
        rectBoton = GetComponent<RectTransform>();
        borde = CrearBordeFijo();
        veloDesactivado = CrearImagen("EvasionMaestraDesactivada", new Color(0.04f, 0.06f, 0.08f, 0.48f)).gameObject;
        halo = CrearImagen("EvasionMaestraDestelloHalo", ColorHalo);
        nucleo = CrearImagen("EvasionMaestraDestelloNucleo", ColorNucleo);
        Sprite spriteDestello = ObtenerSpriteDestelloCircular();
        halo.GetComponent<Image>().sprite = spriteDestello;
        nucleo.GetComponent<Image>().sprite = spriteDestello;
        ActualizarVisibilidad();
    }

    private void Update()
    {
        ActualizarVisibilidad();
        if (habilidad == null || !habilidad.ActivaEnCombate || rectBoton == null)
        {
            return;
        }

        float ancho = Mathf.Max(1f, rectBoton.rect.width);
        float alto = Mathf.Max(1f, rectBoton.rect.height);
        float radioEsquina = Mathf.Clamp(Mathf.Min(ancho, alto) * 0.065f, 2.5f, 5f);
        float perimetro = 2f * (ancho + alto - 4f * radioEsquina) + 2f * Mathf.PI * radioEsquina;
        float distancia = Mathf.Repeat(Time.unscaledTime / DuracionVuelta, 1f) * perimetro;
        Vector2 posicion = ObtenerPosicionEnBordeRedondeado(distancia, ancho, alto, radioEsquina);
        float pulso = 1f + Mathf.Sin(Time.unscaledTime * 4.6f) * 0.055f;
        float tamanoHalo = Mathf.Clamp(Mathf.Min(ancho, alto) * 0.105f, 6.5f, 9f) * pulso;
        float tamanoNucleo = Mathf.Clamp(Mathf.Min(ancho, alto) * 0.038f, 2.5f, 4f);

        halo.anchoredPosition = posicion;
        halo.sizeDelta = Vector2.one * tamanoHalo;
        nucleo.anchoredPosition = posicion;
        nucleo.sizeDelta = Vector2.one * tamanoNucleo;
    }

    private void ActualizarVisibilidad()
    {
        bool activa = habilidad != null && habilidad.ActivaEnCombate;
        if (borde != null) borde.SetActive(activa);
        if (halo != null) halo.gameObject.SetActive(activa);
        if (nucleo != null) nucleo.gameObject.SetActive(activa);
        if (veloDesactivado != null) veloDesactivado.SetActive(habilidad != null && !activa);
    }

    private GameObject CrearBordeFijo()
    {
        GameObject contenedor = new GameObject("EvasionMaestraBordeActivo", typeof(RectTransform));
        RectTransform rectContenedor = contenedor.GetComponent<RectTransform>();
        rectContenedor.SetParent(transform, false);
        rectContenedor.anchorMin = Vector2.zero;
        rectContenedor.anchorMax = Vector2.one;
        rectContenedor.offsetMin = Vector2.zero;
        rectContenedor.offsetMax = Vector2.zero;

        float mitadGrosor = GrosorBorde * 0.5f;
        CrearLineaBorde(rectContenedor, "Superior", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, GrosorBorde), new Vector2(0f, -mitadGrosor));
        CrearLineaBorde(rectContenedor, "Inferior", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, GrosorBorde), new Vector2(0f, mitadGrosor));
        CrearLineaBorde(rectContenedor, "Izquierdo", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(GrosorBorde, 0f), new Vector2(mitadGrosor, 0f));
        CrearLineaBorde(rectContenedor, "Derecho", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(GrosorBorde, 0f), new Vector2(-mitadGrosor, 0f));
        return contenedor;
    }

    private static void CrearLineaBorde(RectTransform padre, string nombre, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 posicion)
    {
        GameObject linea = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rectLinea = linea.GetComponent<RectTransform>();
        rectLinea.SetParent(padre, false);
        rectLinea.anchorMin = anchorMin;
        rectLinea.anchorMax = anchorMax;
        rectLinea.sizeDelta = sizeDelta;
        rectLinea.anchoredPosition = posicion;
        Image imagen = linea.GetComponent<Image>();
        imagen.color = ColorBorde;
        imagen.raycastTarget = false;
    }

    private RectTransform CrearImagen(string nombre, Color color)
    {
        GameObject imagenObjeto = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rectImagen = imagenObjeto.GetComponent<RectTransform>();
        rectImagen.SetParent(transform, false);
        rectImagen.anchorMin = new Vector2(0.5f, 0.5f);
        rectImagen.anchorMax = new Vector2(0.5f, 0.5f);
        rectImagen.pivot = new Vector2(0.5f, 0.5f);

        Image imagen = imagenObjeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;

        if (nombre == "EvasionMaestraDesactivada")
        {
            rectImagen.anchorMin = Vector2.zero;
            rectImagen.anchorMax = Vector2.one;
            rectImagen.offsetMin = Vector2.zero;
            rectImagen.offsetMax = Vector2.zero;
        }

        return rectImagen;
    }

    private static Vector2 ObtenerPosicionEnBordeRedondeado(float distancia, float ancho, float alto, float radio)
    {
        float horizontal = Mathf.Max(0f, ancho - 2f * radio);
        float vertical = Mathf.Max(0f, alto - 2f * radio);
        float arco = Mathf.PI * radio * 0.5f;

        if (distancia < horizontal)
        {
            return new Vector2(-ancho * 0.5f + radio + distancia, alto * 0.5f);
        }

        distancia -= horizontal;
        if (distancia < arco)
        {
            float angulo = Mathf.Lerp(Mathf.PI * 0.5f, 0f, distancia / arco);
            return new Vector2(ancho * 0.5f - radio, alto * 0.5f - radio)
                + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio;
        }

        distancia -= arco;
        if (distancia < vertical)
        {
            return new Vector2(ancho * 0.5f, alto * 0.5f - radio - distancia);
        }

        distancia -= vertical;
        if (distancia < arco)
        {
            float angulo = Mathf.Lerp(0f, -Mathf.PI * 0.5f, distancia / arco);
            return new Vector2(ancho * 0.5f - radio, -alto * 0.5f + radio)
                + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio;
        }

        distancia -= arco;
        if (distancia < horizontal)
        {
            return new Vector2(ancho * 0.5f - radio - distancia, -alto * 0.5f);
        }

        distancia -= horizontal;
        if (distancia < arco)
        {
            float angulo = Mathf.Lerp(-Mathf.PI * 0.5f, -Mathf.PI, distancia / arco);
            return new Vector2(-ancho * 0.5f + radio, -alto * 0.5f + radio)
                + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio;
        }

        distancia -= arco;
        if (distancia < vertical)
        {
            return new Vector2(-ancho * 0.5f, -alto * 0.5f + radio + distancia);
        }

        distancia -= vertical;
        float anguloFinal = Mathf.Lerp(Mathf.PI, Mathf.PI * 0.5f, Mathf.Clamp01(distancia / arco));
        return new Vector2(-ancho * 0.5f + radio, alto * 0.5f - radio)
            + new Vector2(Mathf.Cos(anguloFinal), Mathf.Sin(anguloFinal)) * radio;
    }

    private static Sprite ObtenerSpriteDestelloCircular()
    {
        if (spriteDestelloCircular != null)
        {
            return spriteDestelloCircular;
        }

        const int tamano = 32;
        Texture2D textura = new Texture2D(tamano, tamano, TextureFormat.RGBA32, false)
        {
            name = "EvasionMaestraDestelloSuaveRuntime",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixeles = new Color[tamano * tamano];
        for (int y = 0; y < tamano; y++)
        {
            for (int x = 0; x < tamano; x++)
            {
                float nx = ((x + 0.5f) / tamano) * 2f - 1f;
                float ny = ((y + 0.5f) / tamano) * 2f - 1f;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Sqrt(nx * nx + ny * ny)), 1.8f);
                pixeles[y * tamano + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        textura.SetPixels(pixeles);
        textura.Apply(false, true);
        spriteDestelloCircular = Sprite.Create(textura, new Rect(0f, 0f, tamano, tamano), new Vector2(0.5f, 0.5f), tamano);
        spriteDestelloCircular.name = "EvasionMaestraDestelloSuaveRuntime";
        return spriteDestelloCircular;
    }
}
