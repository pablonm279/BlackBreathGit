using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerContZonas : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contenedor;
    [SerializeField] private UiClimaZonaMenu prefabClimaZona;
    [SerializeField] private bool ocultarPlantillasIniciales = true;

    [Header("Sprites de clima")]
    [SerializeField] private Sprite climaSol;
    [SerializeField] private Sprite climaCalor;
    [SerializeField] private Sprite climaLluvia;
    [SerializeField] private Sprite climaNieve;
    [SerializeField] private Sprite climaNiebla;
    [SerializeField] private Sprite climaAlmasDanzantes;
    [SerializeField] private Sprite climaAuroraBoreal;
    [SerializeField] private Sprite climaNedukazalNormal;
    [SerializeField] private Sprite climaNedukazalMasacre;

    private readonly List<UiClimaZonaMenu> instancias = new List<UiClimaZonaMenu>();
    private readonly List<UiClimaZonaMenu> plantillasIniciales = new List<UiClimaZonaMenu>();
    private bool plantillasInicialesCapturadas;
    private int zonaActual = -1;
    private MetaprogresionSaveData metaprogresionActual;

    private void Awake()
    {
        AutoVincular();
        CapturarPlantillasIniciales();
    }

    private void OnDisable()
    {
        LimpiarInstancias();
    }

    public void MostrarRegion(int zonaId, MetaprogresionSaveData metaprogresion)
    {
        zonaActual = zonaId;
        metaprogresionActual = metaprogresion;

        AutoVincular();
        CapturarPlantillasIniciales();
        LimpiarInstancias();

        if (contenedor == null || zonaId <= 0)
        {
            return;
        }

        List<ClimaZonaProbabilidad> climas = ClimaZonaCatalog.ObtenerProbabilidadesZona(zonaId, ObtenerAtributosZona(zonaId));
        climas.Sort(CompararClimasPorProbabilidad);
        for (int i = 0; i < climas.Count; i++)
        {
            ClimaZonaProbabilidad clima = climas[i];
            if (clima.porcentaje <= 0)
            {
                continue;
            }

            UiClimaZonaMenu item = CrearItem();
            if (item == null)
            {
                continue;
            }

            bool descubierto = EstaDescubierto(clima.tipoClima);
            Sprite sprite = ObtenerSpriteClima(clima.tipoClima);
            string textoTooltip = descubierto
                ? ClimaZonaCatalog.ObtenerTooltipCampania(clima.tipoClima)
                : ClimaZonaCatalog.ObtenerTextoClimaExclusivoDesconocido();

            item.Configurar(
                sprite,
                clima.porcentaje,
                textoTooltip,
                traducirTooltip: descubierto);
            item.gameObject.name = descubierto
                ? "UiClimaszonaMenu_" + ClimaZonaCatalog.ObtenerNombreInterno(clima.tipoClima)
                : "UiClimaszonaMenu_ClimaExclusivoDesconocido";

            instancias.Add(item);
        }
    }

    public void Refrescar()
    {
        if (zonaActual >= 0)
        {
            MostrarRegion(zonaActual, metaprogresionActual);
        }
    }

    private void AutoVincular()
    {
        if (contenedor == null)
        {
            contenedor = transform;
        }

        if (prefabClimaZona == null && contenedor != null)
        {
            prefabClimaZona = BuscarPlantillaClima();
        }
    }

    private UiClimaZonaMenu BuscarPlantillaClima()
    {
        if (contenedor == null)
        {
            return null;
        }

        UiClimaZonaMenu[] existentes = contenedor.GetComponentsInChildren<UiClimaZonaMenu>(true);
        for (int i = 0; i < existentes.Length; i++)
        {
            UiClimaZonaMenu existente = existentes[i];
            if (existente != null && existente.transform != contenedor)
            {
                return existente;
            }
        }

        return null;
    }

    private void CapturarPlantillasIniciales()
    {
        if (plantillasInicialesCapturadas || contenedor == null)
        {
            return;
        }

        UiClimaZonaMenu[] existentes = contenedor.GetComponentsInChildren<UiClimaZonaMenu>(true);
        for (int i = 0; i < existentes.Length; i++)
        {
            UiClimaZonaMenu existente = existentes[i];
            if (existente == null || existente.transform == contenedor)
            {
                continue;
            }

            if (prefabClimaZona == null)
            {
                prefabClimaZona = existente;
            }

            plantillasIniciales.Add(existente);
            if (ocultarPlantillasIniciales)
            {
                existente.gameObject.SetActive(false);
            }
        }

        plantillasInicialesCapturadas = true;
    }

    private UiClimaZonaMenu CrearItem()
    {
        if (prefabClimaZona != null)
        {
            UiClimaZonaMenu item = Instantiate(prefabClimaZona, contenedor);
            item.gameObject.SetActive(true);
            return item;
        }

        return CrearItemFallback();
    }

    private UiClimaZonaMenu CrearItemFallback()
    {
        GameObject itemGo = new GameObject("UiClimaszonaMenu", typeof(RectTransform), typeof(Image));
        itemGo.transform.SetParent(contenedor, false);

        RectTransform rect = itemGo.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80f, 80f);

        Image imagen = itemGo.GetComponent<Image>();
        imagen.raycastTarget = true;
        imagen.preserveAspect = true;

        GameObject chancesGo = new GameObject("Chances", typeof(RectTransform), typeof(TextMeshProUGUI));
        chancesGo.transform.SetParent(itemGo.transform, false);
        RectTransform chancesRect = chancesGo.GetComponent<RectTransform>();
        chancesRect.anchorMin = new Vector2(0.5f, 0f);
        chancesRect.anchorMax = new Vector2(0.5f, 0f);
        chancesRect.anchoredPosition = new Vector2(0f, -8f);
        chancesRect.sizeDelta = new Vector2(120f, 32f);

        TextMeshProUGUI texto = chancesGo.GetComponent<TextMeshProUGUI>();
        texto.alignment = TextAlignmentOptions.Center;
        texto.fontSize = 24f;
        texto.raycastTarget = false;

        return itemGo.AddComponent<UiClimaZonaMenu>();
    }

    private void LimpiarInstancias()
    {
        for (int i = 0; i < instancias.Count; i++)
        {
            if (instancias[i] != null)
            {
                Destroy(instancias[i].gameObject);
            }
        }

        instancias.Clear();
    }

    private bool EstaDescubierto(int tipoClima)
    {
        if (!ClimaZonaCatalog.EsClimaExclusivoRegion(tipoClima))
        {
            return true;
        }

        if (metaprogresionActual != null
            && metaprogresionActual.climasExclusivosDescubiertos != null
            && metaprogresionActual.climasExclusivosDescubiertos.Contains(tipoClima))
        {
            return true;
        }

        return MetaprogresionManager.Instance != null
            && MetaprogresionManager.Instance.ClimaExclusivoDescubierto(tipoClima);
    }

    private static int CompararClimasPorProbabilidad(ClimaZonaProbabilidad a, ClimaZonaProbabilidad b)
    {
        int comparacion = b.porcentaje.CompareTo(a.porcentaje);
        return comparacion != 0
            ? comparacion
            : a.tipoClima.CompareTo(b.tipoClima);
    }

    private AtributosZona ObtenerAtributosZona(int zonaId)
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign == null || campaign.scAtributosZona == null || campaign.scAtributosZona.ID != zonaId)
        {
            return null;
        }

        return campaign.scAtributosZona;
    }

    private Sprite ObtenerSpriteClima(int tipoClima)
    {
        CampaignManager campaign = CampaignManager.Instance;
        if (campaign != null)
        {
            switch (tipoClima)
            {
                case ClimaZonaCatalog.ClimaSol: return campaign.clima_sol;
                case ClimaZonaCatalog.ClimaCalor: return campaign.clima_calor;
                case ClimaZonaCatalog.ClimaLluvia: return campaign.clima_lluvia;
                case ClimaZonaCatalog.ClimaNieve: return campaign.clima_nieve;
                case ClimaZonaCatalog.ClimaNiebla: return campaign.clima_niebla;
                case ClimaZonaCatalog.ClimaAlmasDanzantes: return campaign.clima_almasDanzantes;
                case ClimaZonaCatalog.ClimaAuroraBoreal: return campaign.clima_auroraboreal;
                case ClimaZonaCatalog.ClimaNedukazalNormal: return campaign.clima_NedukazalNormal;
                case ClimaZonaCatalog.ClimaNedukazalMasacre: return campaign.clima_NedukazalMasacre;
            }
        }

        switch (tipoClima)
        {
            case ClimaZonaCatalog.ClimaSol:
                return climaSol != null ? climaSol : Resources.Load<Sprite>("Imagenes/clima_despejado");
            case ClimaZonaCatalog.ClimaCalor:
                return climaCalor != null ? climaCalor : Resources.Load<Sprite>("Imagenes/calor");
            case ClimaZonaCatalog.ClimaLluvia:
                return climaLluvia != null ? climaLluvia : Resources.Load<Sprite>("Imagenes/clima_lluvia 1");
            case ClimaZonaCatalog.ClimaNieve:
                return climaNieve != null ? climaNieve : Resources.Load<Sprite>("Imagenes/clima_frio");
            case ClimaZonaCatalog.ClimaNiebla:
                return climaNiebla != null ? climaNiebla : Resources.Load<Sprite>("Imagenes/clima_niebla 1");
            case ClimaZonaCatalog.ClimaAlmasDanzantes:
                return climaAlmasDanzantes != null ? climaAlmasDanzantes : Resources.Load<Sprite>("Imagenes/Clima_almas");
            case ClimaZonaCatalog.ClimaAuroraBoreal:
                return climaAuroraBoreal != null ? climaAuroraBoreal : Resources.Load<Sprite>("Clima_Aurora");
            case ClimaZonaCatalog.ClimaNedukazalNormal:
                return climaNedukazalNormal != null ? climaNedukazalNormal : Resources.Load<Sprite>("Imagenes/clima_normal, nedukazal");
            case ClimaZonaCatalog.ClimaNedukazalMasacre:
                return climaNedukazalMasacre != null ? climaNedukazalMasacre : Resources.Load<Sprite>("Imagenes/Nedukazal_masacre");
            default:
                return null;
        }
    }
}

public struct ClimaZonaProbabilidad
{
    public int tipoClima;
    public int porcentaje;

    public ClimaZonaProbabilidad(int tipoClima, int porcentaje)
    {
        this.tipoClima = tipoClima;
        this.porcentaje = porcentaje;
    }
}

public static class ClimaZonaCatalog
{
    public const int ClimaSol = 1;
    public const int ClimaCalor = 2;
    public const int ClimaLluvia = 3;
    public const int ClimaNieve = 4;
    public const int ClimaNiebla = 5;
    public const int ClimaAlmasDanzantes = 6;
    public const int ClimaAuroraBoreal = 7;
    public const int ClimaNedukazalNormal = 8;
    public const int ClimaNedukazalMasacre = 9;

    public static List<ClimaZonaProbabilidad> ObtenerProbabilidadesZona(int zonaId)
    {
        return ObtenerProbabilidadesZona(zonaId, null);
    }

    public static List<ClimaZonaProbabilidad> ObtenerProbabilidadesZona(int zonaId, AtributosZona atributosZona)
    {
        List<ClimaZonaProbabilidad> climas = new List<ClimaZonaProbabilidad>();
        int tipoEspecial1;
        int tipoEspecial2;
        ObtenerTiposEspeciales(zonaId, out tipoEspecial1, out tipoEspecial2);

        if (tipoEspecial1 == 0)
        {
            return climas;
        }

        if (atributosZona != null && atributosZona.ID == zonaId)
        {
            AgregarDesdeUmbrales(
                climas,
                atributosZona.Clima_chances_Sol,
                atributosZona.Clima_chances_Calor,
                atributosZona.Clima_chances_Lluvia,
                atributosZona.Clima_chances_Nieve,
                atributosZona.Clima_chances_Niebla,
                atributosZona.Clima_chances_EspecialZona1,
                atributosZona.Clima_chances_EspecialZona2,
                tipoEspecial1,
                tipoEspecial2);
            return climas;
        }

        switch (zonaId)
        {
            case PrePartidaManager.ZonaBosqueArdiente:
                AgregarDesdeUmbrales(climas, 40, 50, 60, 60, 80, 100, 0, ClimaAlmasDanzantes, 0);
                break;
            case PrePartidaManager.ZonaPasoVientoHelado:
                AgregarDesdeUmbrales(climas, 40, 40, 43, 75, 91, 100, 0, ClimaAuroraBoreal, 0);
                break;
            case PrePartidaManager.ZonaNedukazal:
                AgregarDesdeUmbrales(climas, 0, 0, 0, 0, 0, 60, 100, ClimaNedukazalNormal, ClimaNedukazalMasacre);
                break;
        }

        return climas;
    }

    private static void ObtenerTiposEspeciales(int zonaId, out int tipoEspecial1, out int tipoEspecial2)
    {
        tipoEspecial1 = 0;
        tipoEspecial2 = 0;

        switch (zonaId)
        {
            case PrePartidaManager.ZonaBosqueArdiente:
                tipoEspecial1 = ClimaAlmasDanzantes;
                break;
            case PrePartidaManager.ZonaPasoVientoHelado:
                tipoEspecial1 = ClimaAuroraBoreal;
                break;
            case PrePartidaManager.ZonaNedukazal:
                tipoEspecial1 = ClimaNedukazalNormal;
                tipoEspecial2 = ClimaNedukazalMasacre;
                break;
        }
    }

    public static bool EsClimaExclusivoRegion(int tipoClima)
    {
        return tipoClima == ClimaAlmasDanzantes
            || tipoClima == ClimaAuroraBoreal
            || tipoClima == ClimaNedukazalNormal
            || tipoClima == ClimaNedukazalMasacre;
    }

    public static string ObtenerTooltipCampania(int tipoClima)
    {
        switch (tipoClima)
        {
            case ClimaSol:
                return "Soleado: +5 Esperanza.";
            case ClimaCalor:
                return "Ola de Calor: +1 Fatiga. Jornada Libre da +5 Esperanza, otras Tareas Civiles dan -3.";
            case ClimaLluvia:
                return "Lluvia: -5 Esperanza. -15% Recolección Suministros, -20% chances de Emboscada.";
            case ClimaNieve:
                return "Nieve: solo permite Descansar o Guardia. -15% Recolecciones, -20% Emboscada. Viajar da -3 Esperanza por día.";
            case ClimaNiebla:
                return "Niebla: -20% Recolecciones, -20% Emboscada, -20% Exploración, +10% Nodos Misteriosos.";
            case ClimaAlmasDanzantes:
                return "Almas Danzantes: +5 Esperanza, -100% chances de Emboscada.";
            case ClimaAuroraBoreal:
                return "Aurora Boreal: +10 Esperanza.";
            case ClimaNedukazalNormal:
                return "Nedukazal está a oscuras.";
            case ClimaNedukazalMasacre:
                return "Masacre: Nedukazal está siendo atacada. -10 Esperanza. +10% Emboscada. Los Zúrkil están potenciados.";
            default:
                return string.Empty;
        }
    }

    public static string ObtenerTextoClimaExclusivoDesconocido()
    {
        int idioma = TRADU.i != null
            ? TRADU.i.nIdioma
            : PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaEspanol);

        switch (idioma)
        {
            case TRADU.IdiomaIngles:
                return "Unknown exclusive weather";
            case TRADU.IdiomaPortugues:
                return "Clima exclusivo desconhecido";
            default:
                return "Clima exclusivo desconocido";
        }
    }

    public static string ObtenerNombreInterno(int tipoClima)
    {
        switch (tipoClima)
        {
            case ClimaSol: return "Sol";
            case ClimaCalor: return "Calor";
            case ClimaLluvia: return "Lluvia";
            case ClimaNieve: return "Nieve";
            case ClimaNiebla: return "Niebla";
            case ClimaAlmasDanzantes: return "AlmasDanzantes";
            case ClimaAuroraBoreal: return "AuroraBoreal";
            case ClimaNedukazalNormal: return "NedukazalOscuridad";
            case ClimaNedukazalMasacre: return "NedukazalMasacre";
            default: return "Desconocido";
        }
    }

    private static void AgregarDesdeUmbrales(
        List<ClimaZonaProbabilidad> climas,
        int sol,
        int calor,
        int lluvia,
        int nieve,
        int niebla,
        int especial1,
        int especial2,
        int tipoEspecial1,
        int tipoEspecial2)
    {
        int anterior = 0;
        anterior = AgregarTramo(climas, ClimaSol, anterior, sol);
        anterior = AgregarTramo(climas, ClimaCalor, anterior, calor);
        anterior = AgregarTramo(climas, ClimaLluvia, anterior, lluvia);
        anterior = AgregarTramo(climas, ClimaNieve, anterior, nieve);
        anterior = AgregarTramo(climas, ClimaNiebla, anterior, niebla);

        if (tipoEspecial1 > 0)
        {
            anterior = AgregarTramo(climas, tipoEspecial1, anterior, especial1);
        }

        if (tipoEspecial2 > 0)
        {
            AgregarTramo(climas, tipoEspecial2, anterior, especial2);
        }
    }

    private static int AgregarTramo(List<ClimaZonaProbabilidad> climas, int tipoClima, int anterior, int umbral)
    {
        int porcentaje = Mathf.Clamp(umbral - anterior, 0, 100);
        if (porcentaje > 0)
        {
            climas.Add(new ClimaZonaProbabilidad(tipoClima, porcentaje));
        }

        return Mathf.Max(anterior, umbral);
    }
}
