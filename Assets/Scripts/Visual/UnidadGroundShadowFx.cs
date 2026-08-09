using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Agrega una sombra de contacto simple y uniforme debajo de cada unidad.
/// </summary>
public sealed class UnidadGroundShadowFx : MonoBehaviour
{
  private const string NombreSombra = "SombraSueloUnidad";
  private const float AnchoSombra = 14f;
  private const float AltoSombra = 3f;
  private const float AlphaSombra = 0.22f;
  private const float OffsetHaciaHuella = 0.8f;
  private const float OffsetVerticalUnidadVoladora = 13f;

  private static Sprite spriteSombraCompartido;

  private Unidad unidad;
  private Image imagenSombra;

  private void Start()
  {
    unidad = GetComponent<Unidad>();
    CrearSombra();
  }

  private void CrearSombra()
  {
    if (unidad == null || unidad.uImage == null)
    {
      return;
    }

    RectTransform imagenUnidad = unidad.uImage.rectTransform;
    RectTransform padre = imagenUnidad.parent as RectTransform;
    if (padre == null)
    {
      return;
    }

    Transform sombraExistente = padre.Find(NombreSombra);
    if (sombraExistente != null)
    {
      imagenSombra = sombraExistente.GetComponent<Image>();
    }

    if (imagenSombra == null)
    {
      GameObject sombraGO = new GameObject(
        NombreSombra,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image));

      imagenSombra = sombraGO.GetComponent<Image>();
      imagenSombra.rectTransform.SetParent(padre, false);
    }

    imagenSombra.sprite = ObtenerSpriteSombra();
    imagenSombra.color = new Color(0f, 0f, 0f, AlphaSombra);
    imagenSombra.raycastTarget = false;
    imagenSombra.maskable = false;

    RectTransform rectSombra = imagenSombra.rectTransform;
    rectSombra.anchorMin = new Vector2(0.5f, 0.5f);
    rectSombra.anchorMax = new Vector2(0.5f, 0.5f);
    rectSombra.pivot = new Vector2(0.5f, 0.5f);
    rectSombra.sizeDelta = new Vector2(AnchoSombra, AltoSombra);
    rectSombra.localRotation = Quaternion.identity;
    rectSombra.localScale = Vector3.one;

    Vector3[] esquinas = new Vector3[4];
    imagenUnidad.GetWorldCorners(esquinas);
    Vector3 centroInferiorMundo = (esquinas[0] + esquinas[3]) * 0.5f;
    Vector3 centroInferiorLocal = padre.InverseTransformPoint(centroInferiorMundo);
    if (unidad.unidadVoladora)
    {
      centroInferiorLocal.y -= OffsetVerticalUnidadVoladora;
    }

    rectSombra.localPosition = new Vector3(
      centroInferiorLocal.x,
      centroInferiorLocal.y + OffsetHaciaHuella,
      0f);
    rectSombra.SetAsFirstSibling();
  }

  private static Sprite ObtenerSpriteSombra()
  {
    if (spriteSombraCompartido != null)
    {
      return spriteSombraCompartido;
    }

    const int anchoTextura = 64;
    const int altoTextura = 32;
    Texture2D textura = new Texture2D(anchoTextura, altoTextura, TextureFormat.RGBA32, false)
    {
      name = "SombraSueloUnidadTexture",
      filterMode = FilterMode.Bilinear,
      wrapMode = TextureWrapMode.Clamp,
      hideFlags = HideFlags.HideAndDontSave
    };

    Color[] pixeles = new Color[anchoTextura * altoTextura];
    for (int y = 0; y < altoTextura; y++)
    {
      float ny = (((y + 0.5f) / altoTextura) * 2f) - 1f;
      for (int x = 0; x < anchoTextura; x++)
      {
        float nx = (((x + 0.5f) / anchoTextura) * 2f) - 1f;
        float distancia = Mathf.Sqrt((nx * nx) + (ny * ny));
        float alpha = 1f - Mathf.SmoothStep(0.45f, 1f, distancia);
        pixeles[(y * anchoTextura) + x] = new Color(1f, 1f, 1f, alpha);
      }
    }

    textura.SetPixels(pixeles);
    textura.Apply(false, true);

    spriteSombraCompartido = Sprite.Create(
      textura,
      new Rect(0f, 0f, anchoTextura, altoTextura),
      new Vector2(0.5f, 0.5f),
      100f);
    spriteSombraCompartido.name = "SombraSueloUnidadSprite";
    spriteSombraCompartido.hideFlags = HideFlags.HideAndDontSave;
    return spriteSombraCompartido;
  }
}
