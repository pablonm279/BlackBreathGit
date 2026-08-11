using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(GridLayoutGroup))]
public class ManagerListaEstadosCaravana : MonoBehaviour
{
    private static readonly TipoEstadoCaravana[] OrdenEstados =
    {
        TipoEstadoCaravana.Inspiracion,
        TipoEstadoCaravana.Presteza,
        TipoEstadoCaravana.Compromiso,
        TipoEstadoCaravana.Vigilante,
        TipoEstadoCaravana.Acobardados,
        TipoEstadoCaravana.Aletargados,
        TipoEstadoCaravana.Desmotivacion,
        TipoEstadoCaravana.Descuidados
    };

    [Header("Referencias")]
    [SerializeField] private CampaignManager campaignManager;
    [SerializeField] private RectTransform contenedorIconos;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("Visual")]
    [SerializeField] private TMP_FontAsset fuenteContador;
    [SerializeField] private int tamanioContador = 10;
    [SerializeField] private Color colorContador = Color.white;
    [SerializeField] private bool ocultarContadorSiEsUno = true;
    [SerializeField] private float intervaloRefresco = 0.20f;

    [Header("Sprites EstadosCaravana")]
    public Sprite spriteInspiracion;
    public Sprite spritePresteza;
    public Sprite spriteCompromiso;
    public Sprite spriteVigilante;
    public Sprite spriteAcobardados;
    public Sprite spriteAletargados;
    public Sprite spriteDesmotivacion;
    public Sprite spriteDescuidados;

    private readonly Dictionary<TipoEstadoCaravana, IconoEstadoCaravanaUI> iconos = new Dictionary<TipoEstadoCaravana, IconoEstadoCaravanaUI>();
    private float proximoRefresco;
    private int ultimaFirmaEstados = int.MinValue;

    private void Awake()
    {
        AsegurarReferencias();
    }

    private void OnEnable()
    {
        RefrescarForzado();
    }

    private void OnDisable()
    {
        if (TooltipStats.Instance != null)
        {
            TooltipStats.Instance.HideTooltip();
        }
    }

    private void Update()
    {
        if (Time.unscaledTime < proximoRefresco)
        {
            return;
        }

        proximoRefresco = Time.unscaledTime + Mathf.Max(0.05f, intervaloRefresco);
        RefrescarLista();
    }

    [ContextMenu("Refrescar Lista")]
    public void RefrescarForzado()
    {
        ultimaFirmaEstados = int.MinValue;
        RefrescarLista();
    }

    public void RefrescarLista()
    {
        AsegurarReferencias();

        EstadosCaravana estados = ObtenerEstadosCaravana();
        if (estados == null)
        {
            OcultarTodosLosIconos();
            return;
        }

        int firmaActual = CalcularFirma(estados);
        if (firmaActual == ultimaFirmaEstados)
        {
            return;
        }

        ultimaFirmaEstados = firmaActual;

        int indiceVisible = 0;
        for (int i = 0; i < OrdenEstados.Length; i++)
        {
            TipoEstadoCaravana tipo = OrdenEstados[i];
            int stacks = estados.ObtenerStacks(tipo);

            if (stacks <= 0)
            {
                if (iconos.TryGetValue(tipo, out IconoEstadoCaravanaUI iconoOculto) && iconoOculto != null)
                {
                    iconoOculto.gameObject.SetActive(false);
                }

                continue;
            }

            IconoEstadoCaravanaUI icono = ObtenerOCrearIcono(tipo);
            icono.transform.SetSiblingIndex(indiceVisible);
            icono.gameObject.SetActive(true);
            icono.Representar(
                ObtenerSpriteEstado(tipo),
                stacks,
                ConstruirTooltip(tipo, stacks),
                ocultarContadorSiEsUno,
                colorContador,
                fuenteContador != null ? fuenteContador : TMP_Settings.defaultFontAsset,
                tamanioContador);

            indiceVisible++;
        }

        if (contenedorIconos != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild(contenedorIconos);
        }
    }

    private void AsegurarReferencias()
    {
        if (campaignManager == null)
        {
            campaignManager = CampaignManager.Instance;
        }

        if (contenedorIconos == null)
        {
            contenedorIconos = transform as RectTransform;
        }

        if (gridLayoutGroup == null)
        {
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
        }
    }

    private EstadosCaravana ObtenerEstadosCaravana()
    {
        if (campaignManager == null)
        {
            campaignManager = CampaignManager.Instance;
        }

        return campaignManager != null ? campaignManager.estadosCaravana : null;
    }

    private IconoEstadoCaravanaUI ObtenerOCrearIcono(TipoEstadoCaravana tipo)
    {
        if (iconos.TryGetValue(tipo, out IconoEstadoCaravanaUI existente) && existente != null)
        {
            return existente;
        }

        GameObject goIcono = new GameObject(
            "EstadoCaravana_" + tipo,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(IconoEstadoCaravanaUI));

        goIcono.transform.SetParent(contenedorIconos != null ? contenedorIconos : transform, false);

        Image imagen = goIcono.GetComponent<Image>();
        imagen.preserveAspect = true;
        imagen.raycastTarget = true;

        GameObject goContador = new GameObject(
            "Contador",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        goContador.transform.SetParent(goIcono.transform, false);

        TextMeshProUGUI txtContador = goContador.GetComponent<TextMeshProUGUI>();
        txtContador.alignment = TextAlignmentOptions.BottomRight;
        txtContador.raycastTarget = false;
        txtContador.textWrappingMode = TextWrappingModes.NoWrap;
        txtContador.outlineWidth = 0.18f;
        txtContador.outlineColor = Color.black;

        RectTransform rtContador = goContador.GetComponent<RectTransform>();
        rtContador.anchorMin = Vector2.zero;
        rtContador.anchorMax = Vector2.one;
        rtContador.offsetMin = new Vector2(2f, 1f);
        rtContador.offsetMax = new Vector2(-3f, -1f);

        IconoEstadoCaravanaUI icono = goIcono.GetComponent<IconoEstadoCaravanaUI>();
        icono.Configurar(imagen, txtContador);
        iconos[tipo] = icono;
        return icono;
    }

    private void OcultarTodosLosIconos()
    {
        foreach (KeyValuePair<TipoEstadoCaravana, IconoEstadoCaravanaUI> kvp in iconos)
        {
            if (kvp.Value != null)
            {
                kvp.Value.gameObject.SetActive(false);
            }
        }
    }

    private int CalcularFirma(EstadosCaravana estados)
    {
        unchecked
        {
            int firma = 17;
            for (int i = 0; i < OrdenEstados.Length; i++)
            {
                firma = (firma * 31) + estados.ObtenerStacks(OrdenEstados[i]);
            }

            firma = (firma * 31) + (TRADU.i != null ? TRADU.i.nIdioma : TRADU.IdiomaEspanol);
            return firma;
        }
    }

    private Sprite ObtenerSpriteEstado(TipoEstadoCaravana tipo)
    {
        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion: return spriteInspiracion;
            case TipoEstadoCaravana.Presteza: return spritePresteza;
            case TipoEstadoCaravana.Compromiso: return spriteCompromiso;
            case TipoEstadoCaravana.Vigilante: return spriteVigilante;
            case TipoEstadoCaravana.Acobardados: return spriteAcobardados;
            case TipoEstadoCaravana.Aletargados: return spriteAletargados;
            case TipoEstadoCaravana.Desmotivacion: return spriteDesmotivacion;
            case TipoEstadoCaravana.Descuidados: return spriteDescuidados;
            default: return null;
        }
    }

    private string ConstruirTooltip(TipoEstadoCaravana tipo, int stacks)
    {
        string nombre = Traducir(ObtenerNombreEstado(tipo));
        string descripcion = Traducir(ObtenerDescripcionEstado(tipo));
        return "<b>" + nombre + "</b>\n" + descripcion;
    }

    private static string Traducir(string texto)
    {
        return TRADU.i != null ? TRADU.i.Traducir(texto) : texto;
    }

    private static string ObtenerNombreEstado(TipoEstadoCaravana tipo)
    {
        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion: return "Inspiración";
            case TipoEstadoCaravana.Presteza: return "Presteza";
            case TipoEstadoCaravana.Compromiso: return "Compromiso";
            case TipoEstadoCaravana.Vigilante: return "Vigilante";
            case TipoEstadoCaravana.Acobardados: return "Acobardados";
            case TipoEstadoCaravana.Aletargados: return "Aletargados";
            case TipoEstadoCaravana.Desmotivacion: return "Desmotivación";
            case TipoEstadoCaravana.Descuidados: return "Descuidados";
            default: return tipo.ToString();
        }
    }

    private static string ObtenerDescripcionEstado(TipoEstadoCaravana tipo)
    {
        switch (tipo)
        {
            case TipoEstadoCaravana.Inspiracion:
                return "+2 VAL a toda la Caravana en el próximo combate.";
            case TipoEstadoCaravana.Presteza:
                return "+20% velocidad de caravana en el próximo viaje.";
            case TipoEstadoCaravana.Compromiso:
                return "+20% Experiencia en el próximo combate.";
            case TipoEstadoCaravana.Vigilante:
                return "+10% Exploración y -10% emboscadas durante 1 viaje.";
            case TipoEstadoCaravana.Acobardados:
                return "-2 VAL a toda la Caravana en el próximo combate.";
            case TipoEstadoCaravana.Aletargados:
                return "-20% velocidad de caravana en el próximo viaje.";
            case TipoEstadoCaravana.Desmotivacion:
                return "-20% Experiencia en el próximo combate.";
            case TipoEstadoCaravana.Descuidados:
                return "-10% Exploración y +10% emboscadas durante 1 viaje.";
            default:
                return string.Empty;
        }
    }
}

[DisallowMultipleComponent]
public class IconoEstadoCaravanaUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const float MultiplicadorVisualContador = 0.65f;

    [SerializeField] private Image imagenEstado;
    [SerializeField] private TextMeshProUGUI txtStacks;
    private string textoTooltip;

    public void Configurar(Image imagen, TextMeshProUGUI texto)
    {
        imagenEstado = imagen;
        txtStacks = texto;
    }

    public void Representar(
        Sprite sprite,
        int stacks,
        string tooltip,
        bool ocultarSiEsUno,
        Color colorTexto,
        TMP_FontAsset fuente,
        int tamanioFuente)
    {
        if (imagenEstado != null)
        {
            imagenEstado.sprite = sprite;
            imagenEstado.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0.18f);
        }

        if (txtStacks != null)
        {
            txtStacks.text = (ocultarSiEsUno && stacks <= 1) ? string.Empty : "x" + stacks;
            txtStacks.color = colorTexto;
            txtStacks.fontSize = Mathf.Max(8f, tamanioFuente * MultiplicadorVisualContador);
            txtStacks.fontStyle = FontStyles.Bold;
            txtStacks.font = fuente != null ? fuente : TMP_Settings.defaultFontAsset;
        }

        textoTooltip = tooltip;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipStats.Instance == null || string.IsNullOrWhiteSpace(textoTooltip))
        {
            return;
        }

        TooltipStats.Instance.ShowTooltipRaw(textoTooltip, Input.mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipStats.Instance != null)
        {
            TooltipStats.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        if (TooltipStats.Instance != null)
        {
            TooltipStats.Instance.HideTooltip();
        }
    }
}
