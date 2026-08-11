using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipItems: MonoBehaviour
{
    public static TooltipItems Instance;

    public GameObject tooltipObject;
    public TextMeshProUGUI tooltipText;
    private UIFadeSlide tooltipAnim;
    private GameObject tooltipEquipadoObject;
    private TextMeshProUGUI tooltipEquipadoText;
    private readonly List<Transform> elementosVisualesBase = new List<Transform>();
    private readonly List<GameObject> tooltipsHabilidad = new List<GameObject>();
    private readonly List<Vector2> tamanosFondosHabilidad = new List<Vector2>();
    private readonly List<Vector2> tamanosTextosHabilidad = new List<Vector2>();
    private Bounds boundsVisualesBase;
    private TMP_SpriteAsset iconosCombateSpriteAsset;
    private const float EscalaTooltipHabilidad = 0.8f;
    private const float SeparacionTooltips = 12f;
    private const float PaddingVerticalTooltipHabilidad = 56f;
    private const float AltoMinimoTooltipHabilidad = 220f;

    void Awake()
    {
        Instance = this;
        if (tooltipObject == null) { return; }

        iconosCombateSpriteAsset = Resources.Load<TMP_SpriteAsset>(
            "Imagenes/RecursosSprites/IconosTextoCombate/ICONOS_COMBATE");
        RegistrarElementosVisualesBase();
        CrearTooltipEquipado();

        tooltipAnim = UIFadeSlideUtility.Ensure(tooltipObject);
        if (tooltipAnim != null)
        {
            tooltipAnim.SetDurations(0.14f, 0.14f);
            tooltipAnim.SetOffsets(new Vector2(0f, -10f), new Vector2(0f, -8f));
            tooltipAnim.SetFollowMouse(true, new Vector2(14f, -18f));
            tooltipAnim.HideImmediate();
        }
        else
        {
            tooltipObject.SetActive(false);
        }
    }

    public void ShowTooltip(string content, Vector3 position, Item item = null)
    {
        if (tooltipObject == null) { return; }

        OcultarComparacion();
        tooltipText.text = TRADU.i.Traducir(content);
        MostrarTooltipsHabilidades(item);
        UIFadeSlideUtility.ShowAt(tooltipObject, position);
    }

    public void ShowTooltipComparado(
        string content,
        string contentEquipado,
        Vector3 position,
        Item item = null,
        Item itemEquipado = null)
    {
        if (tooltipObject == null) { return; }

        tooltipText.text = TRADU.i.Traducir(content);
        if (tooltipEquipadoObject != null && tooltipEquipadoText != null)
        {
            tooltipEquipadoText.text = TRADU.i.Traducir(contentEquipado);
            tooltipEquipadoObject.SetActive(true);
        }

        MostrarTooltipsHabilidades(item, itemEquipado);
        UIFadeSlideUtility.ShowAt(tooltipObject, position);
    }

    public void HideTooltip()
    {
        UIFadeSlideUtility.Hide(tooltipObject);
    }

    private void RegistrarElementosVisualesBase()
    {
        elementosVisualesBase.Clear();
        for (int i = 0; i < tooltipObject.transform.childCount; i++)
        {
            elementosVisualesBase.Add(tooltipObject.transform.GetChild(i));
        }

        boundsVisualesBase = RectTransformUtility.CalculateRelativeRectTransformBounds(tooltipObject.transform);
    }

    private void CrearTooltipEquipado()
    {
        if (tooltipText == null || tooltipObject.transform.childCount == 0)
        {
            return;
        }

        RectTransform tooltipRect = tooltipObject.GetComponent<RectTransform>();
        if (tooltipRect == null)
        {
            return;
        }

        Bounds boundsVisuales = RectTransformUtility.CalculateRelativeRectTransformBounds(tooltipObject.transform);
        GameObject comparacion = new GameObject("TooltipEquipped", typeof(RectTransform));
        RectTransform comparacionRect = comparacion.GetComponent<RectTransform>();
        comparacionRect.SetParent(tooltipObject.transform, false);
        comparacionRect.anchorMin = tooltipRect.pivot;
        comparacionRect.anchorMax = tooltipRect.pivot;
        comparacionRect.pivot = tooltipRect.pivot;
        comparacionRect.sizeDelta = tooltipRect.rect.size;
        comparacionRect.anchoredPosition = new Vector2(boundsVisuales.size.x + 12f, 0f);

        for (int i = 0; i < elementosVisualesBase.Count; i++)
        {
            Transform original = elementosVisualesBase[i];
            GameObject copia = Instantiate(original.gameObject, comparacionRect);
            if (original == tooltipText.transform)
            {
                tooltipEquipadoText = copia.GetComponent<TextMeshProUGUI>();
            }
        }

        tooltipEquipadoObject = comparacion;
        tooltipEquipadoObject.SetActive(false);
    }

    private void MostrarTooltipsHabilidades(Item item, Item itemSecundario = null)
    {
        OcultarTooltipsHabilidades();
        int indicePanel = 0;
        MostrarTooltipsHabilidadesDeItem(item, boundsVisualesBase.center.x, ref indicePanel);
        if (itemSecundario != null)
        {
            float centroSecundario = boundsVisualesBase.center.x + boundsVisualesBase.size.x + SeparacionTooltips;
            MostrarTooltipsHabilidadesDeItem(itemSecundario, centroSecundario, ref indicePanel);
        }
    }

    private void MostrarTooltipsHabilidadesDeItem(Item item, float centroX, ref int indicePanel)
    {
        if (item == null)
        {
            return;
        }

        List<string> contenidos = ItemTooltipFormatter.ConstruirTooltipsHabilidades(item);
        float bordeInferior = boundsVisualesBase.max.y + SeparacionTooltips;
        for (int i = 0; i < contenidos.Count; i++)
        {
            GameObject panel = indicePanel < tooltipsHabilidad.Count
                ? tooltipsHabilidad[indicePanel]
                : CrearTooltipHabilidad();
            if (panel == null)
            {
                continue;
            }

            TextMeshProUGUI texto = panel.GetComponentInChildren<TextMeshProUGUI>(true);
            Image fondo = panel.GetComponentInChildren<Image>(true);
            if (texto == null || fondo == null)
            {
                continue;
            }

            texto.text = TRADU.i != null ? TRADU.i.Traducir(contenidos[i]) : contenidos[i];
            RectTransform textoRect = texto.rectTransform;
            RectTransform fondoRect = fondo.rectTransform;
            Vector2 tamanoFondoBase = tamanosFondosHabilidad[indicePanel];
            Vector2 tamanoTextoBase = tamanosTextosHabilidad[indicePanel];
            fondoRect.sizeDelta = tamanoFondoBase;
            textoRect.sizeDelta = tamanoTextoBase;
            texto.lineSpacing = -15f;
            texto.paragraphSpacing = 0f;
            texto.ForceMeshUpdate();
            float anchoTexto = Mathf.Max(1f, textoRect.rect.width);
            float altoPreferido = texto.GetPreferredValues(texto.text, anchoTexto, Mathf.Infinity).y;
            float altoTextoVisual = altoPreferido * Mathf.Abs(textoRect.localScale.y);
            float altoPanel = Mathf.Max(
                AltoMinimoTooltipHabilidad,
                altoTextoVisual + PaddingVerticalTooltipHabilidad);

            fondoRect.sizeDelta = new Vector2(tamanoFondoBase.x, altoPanel);
            textoRect.sizeDelta = new Vector2(tamanoTextoBase.x, altoPreferido);

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(fondoRect.rect.width, altoPanel);
            float altoEscalado = altoPanel * EscalaTooltipHabilidad;
            panelRect.anchoredPosition = new Vector2(
                centroX,
                bordeInferior + (altoEscalado * 0.5f));
            bordeInferior += altoEscalado + SeparacionTooltips;
            panel.SetActive(true);
            indicePanel++;
        }
    }

    private GameObject CrearTooltipHabilidad()
    {
        RectTransform tooltipRect = tooltipObject.GetComponent<RectTransform>();
        if (tooltipRect == null)
        {
            return null;
        }

        GameObject panel = new GameObject("TooltipAbility", typeof(RectTransform));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(tooltipObject.transform, false);
        panelRect.anchorMin = tooltipRect.pivot;
        panelRect.anchorMax = tooltipRect.pivot;
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.localScale = Vector3.one * EscalaTooltipHabilidad;

        for (int i = 0; i < elementosVisualesBase.Count; i++)
        {
            GameObject copia = Instantiate(elementosVisualesBase[i].gameObject, panelRect);
            RectTransform copiaRect = copia.GetComponent<RectTransform>();
            if (copiaRect == null)
            {
                continue;
            }

            copiaRect.anchorMin = new Vector2(0.5f, 0.5f);
            copiaRect.anchorMax = new Vector2(0.5f, 0.5f);
            copiaRect.pivot = new Vector2(0.5f, 0.5f);
            copiaRect.anchoredPosition = Vector2.zero;
        }

        tooltipsHabilidad.Add(panel);
        Image fondo = panel.GetComponentInChildren<Image>(true);
        TextMeshProUGUI texto = panel.GetComponentInChildren<TextMeshProUGUI>(true);
        if (texto != null && iconosCombateSpriteAsset != null)
        {
            texto.spriteAsset = iconosCombateSpriteAsset;
        }
        tamanosFondosHabilidad.Add(fondo != null ? fondo.rectTransform.sizeDelta : Vector2.zero);
        tamanosTextosHabilidad.Add(texto != null ? texto.rectTransform.sizeDelta : Vector2.zero);
        panel.SetActive(false);
        return panel;
    }

    private void OcultarTooltipsHabilidades()
    {
        for (int i = 0; i < tooltipsHabilidad.Count; i++)
        {
            tooltipsHabilidad[i].SetActive(false);
        }
    }

    private void OcultarComparacion()
    {
        if (tooltipEquipadoObject != null)
        {
            tooltipEquipadoObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (tooltipObject != null && tooltipObject.activeSelf && tooltipAnim == null)
        {
            tooltipObject.transform.position = Input.mousePosition;
        }
    }
 }

