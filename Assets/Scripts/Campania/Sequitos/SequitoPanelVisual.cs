using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class SequitoPanelVisual
{
    static readonly Vector2 TamanioPanel = new Vector2(585.9432f, 806.8621f);
    static readonly Color ColorDescripcion = new Color(0.74f, 0.72f, 0.67f, 1f);
    static readonly Color ColorMecanicas = new Color(0.82f, 0.81f, 0.77f, 1f);
    static readonly Color ColorFondoMecanicas = new Color(0.015f, 0.022f, 0.024f, 0.9f);
    static readonly Color ColorBorde = new Color(0.39f, 0.28f, 0.14f, 0.65f);

    public static void Aplicar(Sequito sequito, RectTransform panel)
    {
        if (sequito == null || panel == null)
        {
            return;
        }

        bool usaLayoutComun = sequito.ID >= 1;
        ConfigurarPanel(panel, usaLayoutComun);

        if (!usaLayoutComun)
        {
            return;
        }

        OcultarSplash(sequito.imSplashart);
        ConfigurarDescripcion(sequito.txtdesc);
        ConfigurarMecanicas(sequito.ID, sequito.txtmecanicas, panel);
        ConfigurarContenidoHerreros(sequito.ID, panel);
        ConfigurarAccionesEspecializadas(sequito.ID, panel);
        ConfigurarAcciones(sequito.ID, panel);
    }

    public static void ReiniciarScroll(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        ScrollRect[] scrolls = panel.GetComponentsInChildren<ScrollRect>(true);
        for (int i = 0; i < scrolls.Length; i++)
        {
            ScrollRect scroll = scrolls[i];
            if (scroll != null && scroll.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
                scroll.verticalNormalizedPosition = 1f;
            }
        }
    }

    static void ConfigurarPanel(RectTransform panel, bool normalizarTamanio)
    {
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = new Vector2(0f, 4f);
        if (normalizarTamanio)
        {
            panel.sizeDelta = TamanioPanel;
        }
        panel.localScale = Vector3.one;
        panel.localRotation = Quaternion.identity;
    }

    static void OcultarSplash(Image splash)
    {
        if (splash == null)
        {
            return;
        }

        splash.gameObject.SetActive(false);
    }

    static void ConfigurarDescripcion(TextMeshProUGUI descripcion)
    {
        if (descripcion == null)
        {
            return;
        }

        RectTransform rect = descripcion.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -42f);
        rect.sizeDelta = new Vector2(450f, 190f);
        rect.localScale = Vector3.one;

        descripcion.color = ColorDescripcion;
        descripcion.alignment = TextAlignmentOptions.TopLeft;
        descripcion.enableAutoSizing = true;
        descripcion.fontSizeMin = 15f;
        descripcion.fontSizeMax = 20f;
        descripcion.fontSize = 20f;
        descripcion.lineSpacing = 3f;
        descripcion.paragraphSpacing = 0f;
        descripcion.margin = new Vector4(6f, 4f, 6f, 4f);
        descripcion.textWrappingMode = TextWrappingModes.Normal;
        descripcion.overflowMode = TextOverflowModes.Ellipsis;
        descripcion.raycastTarget = false;
    }

    static void ConfigurarMecanicas(int idSequito, TextMeshProUGUI mecanicas, RectTransform panel)
    {
        if (mecanicas == null)
        {
            return;
        }

        RectTransform fondo = mecanicas.transform.parent as RectTransform;
        if (fondo == null || !fondo.IsChildOf(panel))
        {
            return;
        }

        float altura = 300f;
        if (idSequito == 5)
        {
            altura = 215f;
        }
        else if (idSequito == 10)
        {
            altura = 210f;
        }

        fondo.anchorMin = new Vector2(0.5f, 1f);
        fondo.anchorMax = new Vector2(0.5f, 1f);
        fondo.pivot = new Vector2(0.5f, 1f);
        fondo.anchoredPosition = new Vector2(0f, -252f);
        fondo.sizeDelta = new Vector2(450f, altura);
        fondo.localScale = Vector3.one;

        Image fondoImagen = fondo.GetComponent<Image>();
        if (fondoImagen != null)
        {
            fondoImagen.color = ColorFondoMecanicas;
            fondoImagen.raycastTarget = true;
        }

        Outline borde = fondo.GetComponent<Outline>();
        if (borde == null)
        {
            borde = fondo.gameObject.AddComponent<Outline>();
        }

        borde.effectColor = ColorBorde;
        borde.effectDistance = new Vector2(1f, -1f);
        borde.useGraphicAlpha = true;

        if (fondo.GetComponent<RectMask2D>() == null)
        {
            fondo.gameObject.AddComponent<RectMask2D>();
        }

        RectTransform textoRect = mecanicas.rectTransform;
        textoRect.anchorMin = new Vector2(0f, 1f);
        textoRect.anchorMax = new Vector2(1f, 1f);
        textoRect.pivot = new Vector2(0.5f, 1f);
        textoRect.anchoredPosition = new Vector2(0f, -16f);
        textoRect.sizeDelta = new Vector2(-32f, 0f);
        textoRect.localScale = Vector3.one;

        mecanicas.color = ColorMecanicas;
        mecanicas.alignment = TextAlignmentOptions.TopLeft;
        mecanicas.enableAutoSizing = false;
        mecanicas.fontSize = 16f;
        mecanicas.lineSpacing = 3f;
        mecanicas.paragraphSpacing = 0f;
        mecanicas.margin = Vector4.zero;
        mecanicas.textWrappingMode = TextWrappingModes.Normal;
        mecanicas.overflowMode = TextOverflowModes.Overflow;
        mecanicas.raycastTarget = false;

        ContentSizeFitter fitter = mecanicas.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = mecanicas.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = fondo.GetComponent<ScrollRect>();
        if (scroll == null)
        {
            scroll = fondo.gameObject.AddComponent<ScrollRect>();
        }

        scroll.content = textoRect;
        scroll.viewport = fondo;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;
        scroll.scrollSensitivity = 24f;
    }

    static void ConfigurarAcciones(int idSequito, RectTransform panel)
    {
        RectTransform botonEchar = BuscarHijoDirecto(panel, "BtnEchar");
        RectTransform botonVender = BuscarHijoDirecto(panel, "VenderCronica");

        if (botonEchar != null)
        {
            float posicionX = botonVender != null ? 105f : 0f;
            ConfigurarBoton(botonEchar, new Vector2(posicionX, 135f), new Vector2(150f, 46f), idSequito != 11);
        }

        if (botonVender != null)
        {
            ConfigurarBoton(botonVender, new Vector2(-95f, 135f), new Vector2(210f, 46f), false);
        }

        RectTransform plegaria = BuscarHijoDirecto(panel, "Plegaria");
        if (plegaria != null)
        {
            ConfigurarBoton(plegaria, new Vector2(0f, 213f), new Vector2(180f, 46f), false);
        }
    }

    static void ConfigurarAccionesEspecializadas(int idSequito, RectTransform panel)
    {
        if (idSequito < 1 || idSequito > 3)
        {
            return;
        }

        string[] nombresBotones = idSequito == 1
            ? new[] { "btnMejorar", "btnMantArmas", "btnMantArmaduras" }
            : new[] { "btnMejorar" };

        for (int i = 0; i < nombresBotones.Length; i++)
        {
            RectTransform boton = BuscarDescendiente(panel, nombresBotones[i]);
            if (boton != null)
            {
                boton.anchoredPosition += new Vector2(0f, 70f);
            }
        }
    }

    static void ConfigurarContenidoHerreros(int idSequito, RectTransform panel)
    {
        if (idSequito != 1)
        {
            return;
        }

        RectTransform extras = BuscarHijoDirecto(panel, "Extras");
        if (extras == null)
        {
            return;
        }

        extras.anchorMin = new Vector2(0.5f, 0.5f);
        extras.anchorMax = new Vector2(0.5f, 0.5f);
        extras.pivot = new Vector2(0.5f, 0.5f);
        extras.anchoredPosition = new Vector2(0f, -130f);
        extras.sizeDelta = new Vector2(567.94f, 545.3f);
        extras.localScale = Vector3.one;
    }

    static void ConfigurarBoton(RectTransform rect, Vector2 posicion, Vector2 tamanio, bool destructivo)
    {
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamanio;
        rect.localScale = Vector3.one;

        Button boton = rect.GetComponent<Button>();
        if (boton == null)
        {
            return;
        }

        ColorBlock colores = boton.colors;
        colores.normalColor = destructivo
            ? new Color(0.56f, 0.22f, 0.17f, 1f)
            : new Color(0.46f, 0.34f, 0.18f, 1f);
        colores.highlightedColor = destructivo
            ? new Color(0.72f, 0.3f, 0.21f, 1f)
            : new Color(0.64f, 0.47f, 0.22f, 1f);
        colores.pressedColor = destructivo
            ? new Color(0.38f, 0.13f, 0.11f, 1f)
            : new Color(0.3f, 0.21f, 0.11f, 1f);
        colores.selectedColor = colores.highlightedColor;
        colores.disabledColor = new Color(0.24f, 0.24f, 0.22f, 0.55f);
        colores.colorMultiplier = 1f;
        colores.fadeDuration = 0.12f;
        boton.colors = colores;
    }

    static RectTransform BuscarHijoDirecto(RectTransform padre, string nombre)
    {
        for (int i = 0; i < padre.childCount; i++)
        {
            Transform hijo = padre.GetChild(i);
            if (hijo.name == nombre)
            {
                return hijo as RectTransform;
            }
        }

        return null;
    }

    static RectTransform BuscarDescendiente(RectTransform padre, string nombre)
    {
        Transform encontrado = padre.Find(nombre);
        if (encontrado != null)
        {
            return encontrado as RectTransform;
        }

        RectTransform[] descendientes = padre.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < descendientes.Length; i++)
        {
            if (descendientes[i].name == nombre)
            {
                return descendientes[i];
            }
        }

        return null;
    }
}
