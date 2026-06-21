using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuCaravana : MonoBehaviour
{
    private const int MaxTierMejoraCaravana = 5;
    private const int ProbabilidadBasePerderSequitoPorDerrota = 50;
    private const string ColorTextoFactoresExploracion = "#C8C8C8";
    private bool coloresCostoInicializados;
    private Color colorCostoAntorchasNormal;
    private Color colorCostoAlforjasNormal;
    private Color colorCostoTiendasNormal;
    private Color colorCostoCatalejosNormal;
    private Color colorCostoAlmacenNormal;
    private Color colorCostoDefensasNormal;
   
    [SerializeField] GameObject MenuMejoras;
    [SerializeField] GameObject MenuSequitos;
    [SerializeField] GameObject MenuPersonajes;
    [SerializeField] GameObject MenuBitacora;
    private bool menuPersonajesAbiertoPorHover;
    private Personaje personajeMenuPersonajesHover;

    [Header("Exploradores")]
    [SerializeField] GameObject panelResultadoExploradores;
    [SerializeField] TextMeshProUGUI txtTituloResultadoExploradores;
    [SerializeField] TextMeshProUGUI txtDescripcionResultadoExploradores;
    [SerializeField] TextMeshProUGUI txtStatsViaje;


    public bool TieneMenuAbierto =>
        (MenuMejoras != null && MenuMejoras.activeInHierarchy)
        || (MenuSequitos != null && MenuSequitos.activeInHierarchy)
        || (MenuPersonajes != null && MenuPersonajes.activeInHierarchy)
        || (MenuBitacora != null && MenuBitacora.activeInHierarchy)
        || (panelResultadoExploradores != null && panelResultadoExploradores.activeInHierarchy)
        || (CampaignManager.Instance != null && CampaignManager.Instance.LogEstaAbierto());

     //Antorchas de Pie
    [SerializeField] TextMeshProUGUI txtTierMejoraAntorchas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAntorchas;
    [SerializeField] GameObject AntorchasDiamantes;

    [SerializeField] GameObject btMejoraAntorchas;
    [SerializeField] TextMeshProUGUI descAntorchas;
    int costoMejorarAntorchas = 0;

    //Alforjas
    [SerializeField] TextMeshProUGUI txtTierMejoraAlforjas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAlforjas;
    [SerializeField] GameObject AlforjasDiamantes;

    [SerializeField] TextMeshProUGUI descAlforjas;
    [SerializeField] GameObject btMejoraAlforjas;
    int costoMejorarAlforjas = 0;

    //Tiendas
    [SerializeField] TextMeshProUGUI txtTierMejoraTiendas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraTiendas;
    [SerializeField] GameObject TiendasDiamantes;

    [SerializeField] TextMeshProUGUI descTiendas;

    [SerializeField] GameObject btMejoraTiendas;
    int costoMejorarTiendas = 0;

    //Catalejos
    [SerializeField] TextMeshProUGUI txtTierMejoraCatalejos;
    [SerializeField] TextMeshProUGUI txtCostoMejoraCatalejos;
    [SerializeField] TextMeshProUGUI descCatalejos;
    [SerializeField] GameObject CatalejosDiamantes;

    [SerializeField] GameObject btMejoraCatalejos;
    int costoMejorarCatalejos = 0;

    //Almacen
    [SerializeField] TextMeshProUGUI txtTierMejoraAlmacen;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAlmacen;
    [SerializeField] TextMeshProUGUI descAlmacen;
    [SerializeField] GameObject AlmacenDiamantes;

    [SerializeField] GameObject btMejoraAlmacen;
    [SerializeField] TextMeshProUGUI txtExplicacionVistaLejana;
    [SerializeField] TextMeshProUGUI txtExplicacionExploracionPasiva;
    [SerializeField] TextMeshProUGUI txtExploradores;
    int costoMejorarAlmacen = 0;

     //Defensas
    [SerializeField] TextMeshProUGUI txtTierMejoraDefensas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraDefensas;
    [SerializeField] TextMeshProUGUI descDefensas;
    [SerializeField] GameObject DefensasDiamantes;
    [SerializeField] GameObject btMejoraDefensas;
    int costoMejorarDefensas = 0;

     [SerializeField] GameObject contenedorSequitos;

    public bool SeApretoESC()
    { 
        bool habiaalgoabierto = TieneMenuAbierto;

        if (MenuMejoras != null)
        {
            MenuMejoras.SetActive(false);
        }

        if (MenuSequitos != null)
        {
            MenuSequitos.SetActive(false);
        }

        if (MenuPersonajes != null)
        {
            MenuPersonajes.SetActive(false);
        }
        menuPersonajesAbiertoPorHover = false;
        personajeMenuPersonajesHover = null;

        if (MenuBitacora != null)
        {
            MenuBitacora.SetActive(false);
        }

        if (CampaignManager.Instance != null)
        {
            CampaignManager.Instance.ActivarLog(0);
        }

        if (panelResultadoExploradores != null)
        {
            panelResultadoExploradores.SetActive(false);
        }

        return habiaalgoabierto;
    }

    public void CerrarMenusExclusivos()
    {
        bool menuMejorasAbierto = MenuMejoras != null && MenuMejoras.activeInHierarchy;
        bool menuSequitosAbierto = MenuSequitos != null && MenuSequitos.activeInHierarchy;
        bool menuPersonajesAbierto = MenuPersonajes != null && MenuPersonajes.activeInHierarchy;

        MenuMejoras.SetActive(false);
        MenuSequitos.SetActive(false);
        MenuPersonajes.SetActive(false);
        MenuBitacora.SetActive(false);
        menuPersonajesAbiertoPorHover = false;
        personajeMenuPersonajesHover = null;

        if (menuMejorasAbierto)
        {
            EmitirCierreMenuMejorasTutorial(false);
        }

        if (menuSequitosAbierto)
        {
            EmitirCierreMenuSequitosTutorial(false);
        }

        if (menuPersonajesAbierto)
        {
            EmitirCierreMenuPersonajesTutorial(false);
        }
    }

    public void MostrarResultadoExploradores(CampaignManager.ResultadoExploradoresCampania resultado)
    {
        if (resultado == null)
        {
            return;
        }

        if (panelResultadoExploradores == null)
        {
            return;
        }

        if (txtTituloResultadoExploradores != null)
        {
            txtTituloResultadoExploradores.text = resultado.titulo;
        }

        if (txtDescripcionResultadoExploradores != null)
        {
            string texto = resultado.descripcion + "\n";
            texto += TRADU.i.Traducir("Tirada: ") + resultado.tirada + " / " + resultado.chance + "%\n";

            if (resultado.materialesGanados != 0)
              texto += "+" + resultado.materialesGanados + " " + TRADU.i.Traducir("Materiales") + "\n";
            if (resultado.oroGanado != 0)
              texto += "+" + resultado.oroGanado + " " + TRADU.i.Traducir("Oro") + "\n";
            if (resultado.esperanzaCambio != 0)
              texto += (resultado.esperanzaCambio > 0 ? "+" : "") + resultado.esperanzaCambio + " " + TRADU.i.Traducir("Esperanza") + "\n";
            if (resultado.civilesMuertos > 0)
              texto += "-" + resultado.civilesMuertos + " " + TRADU.i.Traducir("Civiles") + "\n";
            if (!string.IsNullOrEmpty(resultado.faccionReveladaNombre))
              texto += TRADU.i.Traducir("Enemigos: ") + resultado.faccionReveladaNombre + "\n";

            txtDescripcionResultadoExploradores.text = texto.TrimEnd();
        }

        panelResultadoExploradores.SetActive(true);
    }

    void Awake()
    {
        InicializarColoresCostoSiHaceFalta();
    }

    private void InicializarColoresCostoSiHaceFalta()
    {
        if (coloresCostoInicializados)
        {
            return;
        }

        colorCostoAntorchasNormal = txtCostoMejoraAntorchas != null ? txtCostoMejoraAntorchas.color : Color.white;
        colorCostoAlforjasNormal = txtCostoMejoraAlforjas != null ? txtCostoMejoraAlforjas.color : Color.white;
        colorCostoTiendasNormal = txtCostoMejoraTiendas != null ? txtCostoMejoraTiendas.color : Color.white;
        colorCostoCatalejosNormal = txtCostoMejoraCatalejos != null ? txtCostoMejoraCatalejos.color : Color.white;
        colorCostoAlmacenNormal = txtCostoMejoraAlmacen != null ? txtCostoMejoraAlmacen.color : Color.white;
        colorCostoDefensasNormal = txtCostoMejoraDefensas != null ? txtCostoMejoraDefensas.color : Color.white;
        coloresCostoInicializados = true;
    }

    private void ActualizarColorCosto(TextMeshProUGUI txtCosto, int costo, Color colorNormal)
    {
        if (txtCosto == null)
        {
            return;
        }

        bool alcanza = costo <= CampaignManager.Instance.GetMaterialesActuales();
        txtCosto.color = alcanza ? colorNormal : Color.red;
    }

    private static string ObtenerTierRomano(int tier)
    {
        return tier switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            _ => tier.ToString()
        };
    }

    private void ActualizarDiamantesMejora(GameObject diamantes, int tier)
    {
        if (diamantes == null)
        {
            return;
        }

        int diamantesActivos = Mathf.Max(0, tier - 1);
        for (int i = 0; i < diamantes.transform.childCount; i++)
        {
            diamantes.transform.GetChild(i).gameObject.SetActive(i < diamantesActivos);
        }
    }

    private int ObtenerIdiomaActual()
    {
        if (TRADU.i != null)
        {
            return TRADU.i.nIdioma;
        }

        return PlayerPrefs.GetInt("nIdioma", TRADU.IdiomaIngles);
    }

    private int ObtenerReduccionPerdidaSequitoDefensas(int tier)
    {
        return 5 + (3 * tier);
    }

    private int ObtenerChanceFinalPerderSequitoDefensas(int tier)
    {
        return Mathf.Clamp(ProbabilidadBasePerderSequitoPorDerrota - ObtenerReduccionPerdidaSequitoDefensas(tier), 0, 100);
    }

    private int ObtenerReduccionEmboscadaAntorchas(int tier)
    {
        return tier * 2;
    }

    private int ObtenerBonusObjetosAntorchas(int tier)
    {
        return 3 + ((tier - 1) * 2);
    }

    private int ObtenerBonusCargaPorBueyAlforjas(int tier)
    {
        return Mathf.Max(0, tier - 1);
    }

    private int ObtenerBonusEsperanzaTiendas(int tier)
    {
        return Mathf.Max(0, (tier - 1) * 2);
    }

    private int ObtenerCapacidadPersonajesTiendas(int tier)
    {
        return 4 + Mathf.Max(0, tier - 1);
    }

    private int ObtenerBonusCatalejos(int tier)
    {
        return 3 + ((tier - 1) * 2);
    }

    private int ObtenerBonusVisionCatalejos(int tier)
    {
        return Mathf.Max(0, tier - 1);
    }

    private int ObtenerBonusScoutCatalejos(int tier)
    {
        return Mathf.Max(0, tier - 1) * 5;
    }

    private int ObtenerReduccionConsumoAlmacen(int tier)
    {
        return tier * 3;
    }

    private int ObtenerBonusCargaAlmacen(int tier)
    {
        return tier * 5;
    }

    private int CalcularCostoEscaladoDesdeSistemaViejo(int tierActual, int costoTier1Viejo, int costoTier3Viejo)
    {
        if (MaxTierMejoraCaravana <= 1)
        {
            return costoTier1Viejo;
        }

        float progreso = Mathf.Clamp01((tierActual - 1f) / (MaxTierMejoraCaravana - 1f));
        return Mathf.RoundToInt(Mathf.Lerp(costoTier1Viejo, costoTier3Viejo, progreso));
    }

    private void ActualizarDescripcionMejora(TextMeshProUGUI txtDescripcion, string textoEs, string textoEn, string textoPt)
    {
        if (txtDescripcion == null)
        {
            return;
        }

        txtDescripcion.text = ObtenerIdiomaActual() switch
        {
            TRADU.IdiomaPortugues => textoPt,
            TRADU.IdiomaIngles => textoEn,
            _ => textoEs
        };
    }

    private void ActualizarDescripcionesMejoras()
    {
        int tierAntorchas = CampaignManager.Instance.mejoraCaravanaAntorchas;
        int tierAlforjas = CampaignManager.Instance.mejoraCaravanaAlforjas;
        int tierTiendas = CampaignManager.Instance.mejoraCaravanaTiendas;
        int tierCatalejos = CampaignManager.Instance.mejoraCaravanaCatalejos;
        int tierAlmacen = CampaignManager.Instance.mejoraCaravanaAlmacen;
        int tierDefensas = CampaignManager.Instance.mejoraCaravanaDefensas;

        ActualizarDescripcionMejora(
            descAntorchas,
            ObtenerDescripcionAntorchasEs(tierAntorchas),
            ObtenerDescripcionAntorchasEn(tierAntorchas),
            ObtenerDescripcionAntorchasPt(tierAntorchas));

        ActualizarDescripcionMejora(
            descAlforjas,
            ObtenerDescripcionAlforjasEs(tierAlforjas),
            ObtenerDescripcionAlforjasEn(tierAlforjas),
            ObtenerDescripcionAlforjasPt(tierAlforjas));

        ActualizarDescripcionMejora(
            descTiendas,
            ObtenerDescripcionTiendasEs(tierTiendas),
            ObtenerDescripcionTiendasEn(tierTiendas),
            ObtenerDescripcionTiendasPt(tierTiendas));

        ActualizarDescripcionMejora(
            descCatalejos,
            ObtenerDescripcionCatalejosEs(tierCatalejos),
            ObtenerDescripcionCatalejosEn(tierCatalejos),
            ObtenerDescripcionCatalejosPt(tierCatalejos));

        ActualizarDescripcionMejora(
            descAlmacen,
            ObtenerDescripcionAlmacenEs(tierAlmacen),
            ObtenerDescripcionAlmacenEn(tierAlmacen),
            ObtenerDescripcionAlmacenPt(tierAlmacen));

        ActualizarDescripcionMejora(
            descDefensas,
            ObtenerDescripcionDefensasEs(tierDefensas),
            ObtenerDescripcionDefensasEn(tierDefensas),
            ObtenerDescripcionDefensasPt(tierDefensas));

        ActualizarDescripcionMejora(
            txtExplicacionVistaLejana,
            ObtenerExplicacionVistaLejanaEs(tierCatalejos),
            ObtenerExplicacionVistaLejanaEn(tierCatalejos),
            ObtenerExplicacionVistaLejanaPt(tierCatalejos));

        ActualizarDescripcionMejora(
            txtExplicacionExploracionPasiva,
            ObtenerExplicacionExploracionPasivaEs(tierCatalejos),
            ObtenerExplicacionExploracionPasivaEn(tierCatalejos),
            ObtenerExplicacionExploracionPasivaPt(tierCatalejos));
    }

    public void AbrirMenuMejoras()
    {
        AbrirMenuMejoras(false);
    }

    public void AbrirMenuMejorasDesdeHotkey()
    {
        AbrirMenuMejoras(true);
    }

    public bool MenuMejorasEstaAbierto()
    {
        return MenuMejoras != null && MenuMejoras.activeInHierarchy;
    }

    private void AbrirMenuMejoras(bool desdeHotkey)
    {
         TutorialEvents.Emit("ui.abrirmejoras", gameObject);
        if (desdeHotkey && MenuMejorasEstaAbierto())
        {
            MenuMejoras.SetActive(false);
            EmitirCierreMenuMejorasTutorial(true);
            return;
        }

        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 15) { return; }
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 15)
        { CampaignManager.Instance.scTutorialManager.SiguientePaso(); }

        bool abrir = !MenuMejoras.activeInHierarchy;
        ActualizarMejoras();
        CerrarMenusExclusivos();
        CampaignManager.Instance.ActivarLog(0);
        MenuMejoras.SetActive(abrir);
        if (!abrir)
        {
            EmitirCierreMenuMejorasTutorial(desdeHotkey);
            return;
        }

        if (abrir)
        {
            RuntimeAnalytics.TrackDesign("ui", "caravan", "open_upgrades");
        }

    }
    public void AbrirMenuSequitos()
    {
        AbrirMenuSequitos(false);
    }

    public void AbrirMenuSequitosDesdeHotkey()
    {
        AbrirMenuSequitos(true);
    }

    public bool MenuSequitosEstaAbierto()
    {
        return MenuSequitos != null && MenuSequitos.activeInHierarchy;
    }

    private void AbrirMenuSequitos(bool desdeHotkey)
    {
        TutorialEvents.Emit("ui.menusequitosab", gameObject);
        if (desdeHotkey && MenuSequitosEstaAbierto())
        {
            MenuSequitos.SetActive(false);
            EmitirCierreMenuSequitosTutorial(true);
            return;
        }

        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 27) { return; }
         if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 27)
        {            CampaignManager.Instance.scTutorialManager.SiguientePaso();        }
         if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 29)
        {            CampaignManager.Instance.scTutorialManager.SiguientePaso();        }

        bool abrir = !MenuSequitos.activeInHierarchy;
        CerrarMenusExclusivos();
        CampaignManager.Instance.ActivarLog(0);
        MenuSequitos.SetActive(abrir);
        if (!abrir)
        {
            EmitirCierreMenuSequitosTutorial(desdeHotkey);
            return;
        }

        if (abrir)
        {
            RuntimeAnalytics.TrackDesign("ui", "caravan", "open_followers");
        }
      
        
        
       
    }
    public void AbrirMenuPersonajes()
    {
        AbrirMenuPersonajes(null, true, false, false);
    }

    public void AbrirMenuPersonajesDesdeHotkey()
    {
        AbrirMenuPersonajes(null, true, true, false);
    }

    public bool MenuPersonajesEstaAbierto()
    {
        return MenuPersonajes != null && MenuPersonajes.activeInHierarchy;
    }
    public void AbrirBitácora()
    {
       ActualizarStatsViaje();
       bool abrir = MenuBitacora == null || !MenuBitacora.activeInHierarchy;
       CerrarMenusExclusivos();
       CampaignManager.Instance.ActivarLog(abrir ? 1 : 0);
    }

    private void ActualizarStatsViaje()
    {
        if (txtStatsViaje == null || CampaignManager.Instance == null)
        {
            return;
        }

        CampaignManager campaignManager = CampaignManager.Instance;
        StringBuilder sb = new StringBuilder(160);
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Días viajados", "Days traveled", "Dias viajados"),
            campaignManager.ObtenerEstadisticaDiasViajados()));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Batallas libradas", "Battles fought", "Batalhas travadas"),
            campaignManager.ObtenerEstadisticaBatallasLibradas()));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Personajes muertos", "Characters dead", "Personagens mortos"),
            campaignManager.ObtenerEstadisticaPersonajesMuertos()));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Enemigos asesinados", "Enemies killed", "Inimigos assassinados"),
            campaignManager.ObtenerEstadisticaEnemigosAsesinados()));
        sb.AppendLine(FormatearLineaStat(
            ObtenerTextoStats("Civiles perdidos", "Civilians lost", "Civis perdidos"),
            campaignManager.ObtenerEstadisticaCivilesPerdidos()));
        sb.Append(FormatearLineaStat(
            ObtenerTextoStats("Asentamientos visitados", "Settlements visited", "Assentamentos visitados"),
            campaignManager.ObtenerEstadisticaAsentamientosVisitados()));
        txtStatsViaje.text = sb.ToString();
    }

    private string ObtenerTextoStats(string textoEs, string textoEn, string textoPt)
    {
        return ObtenerIdiomaActual() switch
        {
            TRADU.IdiomaPortugues => textoPt,
            TRADU.IdiomaIngles => textoEn,
            _ => textoEs
        };
    }

    private static string FormatearLineaStat(string etiqueta, int valor)
    {
        return etiqueta + "... " + valor;
    }

    public void AbrirMenuPersonajes(Personaje personajeInicial)
    {
        AbrirMenuPersonajes(personajeInicial, false, false, false);
    }

    public void AbrirMenuPersonajesPorHover(Personaje personajeInicial)
    {
        if (personajeInicial == null || MenuPersonajes == null)
        {
            return;
        }

        if (MenuPersonajes.activeInHierarchy && !menuPersonajesAbiertoPorHover)
        {
            return;
        }

        AbrirMenuPersonajes(personajeInicial, false, false, true);
    }

    public void CerrarMenuPersonajesPorHover(Personaje personaje)
    {
        if (!menuPersonajesAbiertoPorHover || MenuPersonajes == null)
        {
            return;
        }

        if (personajeMenuPersonajesHover != null && personaje != personajeMenuPersonajesHover)
        {
            return;
        }

        MenuPersonajes.SetActive(false);
        menuPersonajesAbiertoPorHover = false;
        personajeMenuPersonajesHover = null;
        EmitirCierreMenuPersonajesTutorial(false);
    }

    private void AbrirMenuPersonajes(Personaje personajeInicial, bool alternarMenu, bool desdeHotkey, bool desdeHover)
    {
         TutorialEvents.Emit("ui.menupersonajescerrado1", gameObject);
        bool estabaAbiertoPorHover = menuPersonajesAbiertoPorHover;
        if (desdeHotkey && MenuPersonajesEstaAbierto())
        {
            MenuPersonajes.SetActive(false);
            menuPersonajesAbiertoPorHover = false;
            personajeMenuPersonajesHover = null;
            EmitirCierreMenuPersonajesTutorial(true);
            return;
        }

        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 5) { return; }
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 5)
        {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
            CampaignManager.Instance.CrearCaballero();
            CampaignManager.Instance.CrearExplorador();
            
        }
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 10)
        {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
            
        }

        var scMenuPersonajes = MenuPersonajes.GetComponent<MenuPersonajes>();
        if (scMenuPersonajes == null) return;

        if (!alternarMenu && personajeInicial != null && MenuPersonajes.activeInHierarchy && scMenuPersonajes.pSel == personajeInicial && !estabaAbiertoPorHover)
        {
            MenuPersonajes.SetActive(false);
            menuPersonajesAbiertoPorHover = false;
            personajeMenuPersonajesHover = null;
            EmitirCierreMenuPersonajesTutorial(desdeHotkey);
            return;
        }

        CerrarMenusExclusivos();
        CampaignManager.Instance.ActivarLog(0);
        bool estabaAbierto = MenuPersonajes.activeInHierarchy;
        bool abrir = alternarMenu ? !estabaAbierto : true;
        MenuPersonajes.SetActive(abrir);
        if (!abrir)
        {
            menuPersonajesAbiertoPorHover = false;
            personajeMenuPersonajesHover = null;
            EmitirCierreMenuPersonajesTutorial(desdeHotkey);
            return;
        }

        RuntimeAnalytics.TrackDesign("ui", "caravan", "open_characters");

        if (personajeInicial == null && scMenuPersonajes.listaPersonajes != null && scMenuPersonajes.listaPersonajes.Count > 0)
        {
            personajeInicial = scMenuPersonajes.listaPersonajes.Find(p => p != null && !p.Camp_Muerto);
            if (personajeInicial == null)
            {
                personajeInicial = scMenuPersonajes.listaPersonajes[0];
            }
        }

        menuPersonajesAbiertoPorHover = desdeHover;
        personajeMenuPersonajesHover = desdeHover ? personajeInicial : null;
        scMenuPersonajes.PrepararYAbrirMenu(personajeInicial);
        if (scMenuPersonajes.itemDesc != null)
        {
            scMenuPersonajes.itemDesc.text = "";
        }

     
       
    }

    private void EmitirCierreMenuPersonajesTutorial(bool desdeHotkey)
    {
        TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignCharacterMenuClosed, MenuPersonajes)
            .Add("menu", "personajes")
            .Add("closedByHotkey", desdeHotkey ? 1 : 0));
    }

    private void EmitirCierreMenuMejorasTutorial(bool desdeHotkey)
    {
        TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignUpgradeMenuClosed, MenuMejoras)
            .Add("menu", "mejoras")
            .Add("closedByHotkey", desdeHotkey ? 1 : 0));

        if (desdeHotkey && CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 17)
        {
            CampaignManager.Instance.scTutorialManager.SiguientePaso();
        }
    }

    private void EmitirCierreMenuSequitosTutorial(bool desdeHotkey)
    {
        TutorialEvents.Emit(new TutorialEventPayload(TutorialEventNames.CampaignFollowersMenuClosed, MenuSequitos)
            .Add("menu", "sequitos")
            .Add("closedByHotkey", desdeHotkey ? 1 : 0));
    }

    public void ActualizarMejoras()
    {
        InicializarColoresCostoSiHaceFalta();

        //Antorchas
        costoMejorarAntorchas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaAntorchas, 40, 60));
        txtTierMejoraAntorchas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaAntorchas);
        txtCostoMejoraAntorchas.text = "" + costoMejorarAntorchas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAntorchas, costoMejorarAntorchas, colorCostoAntorchasNormal);
        ActualizarDiamantesMejora(AntorchasDiamantes, CampaignManager.Instance.mejoraCaravanaAntorchas);
        btMejoraAntorchas.SetActive(CampaignManager.Instance.mejoraCaravanaAntorchas < MaxTierMejoraCaravana);

        //Alforjas
        costoMejorarAlforjas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaAlforjas, 34, 52));
        txtTierMejoraAlforjas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaAlforjas);
        txtCostoMejoraAlforjas.text = "" + costoMejorarAlforjas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAlforjas, costoMejorarAlforjas, colorCostoAlforjasNormal);
        ActualizarDiamantesMejora(AlforjasDiamantes, CampaignManager.Instance.mejoraCaravanaAlforjas);
        btMejoraAlforjas.SetActive(CampaignManager.Instance.mejoraCaravanaAlforjas < MaxTierMejoraCaravana);

        //Tiendas
        costoMejorarTiendas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaTiendas, 69, 104));
        txtTierMejoraTiendas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaTiendas);
        txtCostoMejoraTiendas.text = "" + costoMejorarTiendas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraTiendas, costoMejorarTiendas, colorCostoTiendasNormal);
        ActualizarDiamantesMejora(TiendasDiamantes, CampaignManager.Instance.mejoraCaravanaTiendas);
        btMejoraTiendas.SetActive(CampaignManager.Instance.mejoraCaravanaTiendas < MaxTierMejoraCaravana);

        //Catalejos
        costoMejorarCatalejos = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaCatalejos, 46, 68));
        txtTierMejoraCatalejos.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaCatalejos);
        txtCostoMejoraCatalejos.text = "" + costoMejorarCatalejos + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraCatalejos, costoMejorarCatalejos, colorCostoCatalejosNormal);
        ActualizarDiamantesMejora(CatalejosDiamantes, CampaignManager.Instance.mejoraCaravanaCatalejos);
        btMejoraCatalejos.SetActive(CampaignManager.Instance.mejoraCaravanaCatalejos < MaxTierMejoraCaravana);

        //Almacen
        costoMejorarAlmacen = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaAlmacen, 46, 68));
        txtTierMejoraAlmacen.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaAlmacen);
        txtCostoMejoraAlmacen.text = "" + costoMejorarAlmacen + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAlmacen, costoMejorarAlmacen, colorCostoAlmacenNormal);
        ActualizarDiamantesMejora(AlmacenDiamantes, CampaignManager.Instance.mejoraCaravanaAlmacen);
        btMejoraAlmacen.SetActive(CampaignManager.Instance.mejoraCaravanaAlmacen < MaxTierMejoraCaravana);
        
        //Defensas
        costoMejorarDefensas = CampaignManager.Instance.AplicarCostoMejoraCaravanaTraits(
            CalcularCostoEscaladoDesdeSistemaViejo(CampaignManager.Instance.mejoraCaravanaDefensas, 36, 58));
        txtTierMejoraDefensas.text = ObtenerTierRomano(CampaignManager.Instance.mejoraCaravanaDefensas);
        txtCostoMejoraDefensas.text = "" + costoMejorarDefensas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraDefensas, costoMejorarDefensas, colorCostoDefensasNormal);
        ActualizarDiamantesMejora(DefensasDiamantes, CampaignManager.Instance.mejoraCaravanaDefensas);
        btMejoraDefensas.SetActive(CampaignManager.Instance.mejoraCaravanaDefensas < MaxTierMejoraCaravana);

        ActualizarDescripcionesMejoras();
    }


    public void MejorarAntorchas()
    {
       if(costoMejorarAntorchas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaAntorchas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaAntorchas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAntorchas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarAntorchas, "caravan_upgrade", "antorchas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "antorchas");
       }

        ActualizarMejoras();
        CampaignManager.Instance.scAtributosZona.ActualizarLuzNedukazal();
    }
    public void MejorarAlforjas()
    {   TutorialEvents.Emit("ui.mejoraralforjas", gameObject);
       if(costoMejorarAlforjas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaAlforjas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaAlforjas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAlforjas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarAlforjas, "caravan_upgrade", "alforjas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "alforjas");
       }
       if(CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 16)
       {
           CampaignManager.Instance.scTutorialManager.SiguientePaso();
       }

        ActualizarMejoras();

    }

    public void MejorarTiendas()
    {
       if(costoMejorarTiendas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaTiendas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaTiendas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarTiendas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarTiendas, "caravan_upgrade", "tiendas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "tiendas");
       }

        ActualizarMejoras();

    }

    public void MejorarCatalejos()
    {
       if(costoMejorarCatalejos <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaCatalejos < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaCatalejos += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarCatalejos);
        if (CampaignManager.Instance.scMapaManager != null)
        {
            CampaignManager.Instance.scMapaManager.RefrescarVisibilidadExploracion();
        }
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarCatalejos, "caravan_upgrade", "catalejos");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "catalejos");
       }

        ActualizarMejoras();

    }

    public void MejorarAlmacen()
    {
       if(costoMejorarAlmacen <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaAlmacen < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaAlmacen += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAlmacen);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarAlmacen, "caravan_upgrade", "almacen");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "almacen");
       }

        ActualizarMejoras();

    }

    public void MejorarDefensas()
    {
       if(costoMejorarDefensas <= CampaignManager.Instance.GetMaterialesActuales() && CampaignManager.Instance.mejoraCaravanaDefensas < MaxTierMejoraCaravana)
       {
        CampaignManager.Instance.mejoraCaravanaDefensas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarDefensas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarDefensas, "caravan_upgrade", "defensas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "defensas");
       }

        ActualizarMejoras();

    }

    private string ObtenerDescripcionAntorchasEs(int tier)
    {
        return "Antorchas de Pie:\n"
            + "Disminuyen en " + ObtenerReduccionEmboscadaAntorchas(tier) + "% las chances de sufrir emboscadas al viajar o descansar.\n"
            + "Además aumentan en " + ObtenerBonusObjetosAntorchas(tier) + "% las chances de encontrar objetos tras una batalla.";
    }

    private string ObtenerDescripcionAntorchasEn(int tier)
    {
        return "Standing Torches:\n"
            + "Reduce the chance of ambushes while traveling or resting by " + ObtenerReduccionEmboscadaAntorchas(tier) + "%.\n"
            + "They also increase the chance to find items after a battle by " + ObtenerBonusObjetosAntorchas(tier) + "%.";
    }

    private string ObtenerDescripcionAntorchasPt(int tier)
    {
        return "Tochas de Pé:\n"
            + "Reduzem em " + ObtenerReduccionEmboscadaAntorchas(tier) + "% as chances de sofrer emboscadas ao viajar ou descansar.\n"
            + "Além disso, aumentam em " + ObtenerBonusObjetosAntorchas(tier) + "% as chances de encontrar itens após uma batalha.";
    }

    private string ObtenerDescripcionAlforjasEs(int tier)
    {
        return "Alforjas:\n"
            + "Agregan +" + ObtenerBonusCargaPorBueyAlforjas(tier) + " de capacidad de carga a los bueyes que llevan los suministros de la caravana.";
    }

    private string ObtenerDescripcionAlforjasEn(int tier)
    {
        return "Saddlebags:\n"
            + "Add +" + ObtenerBonusCargaPorBueyAlforjas(tier) + " carrying capacity to the oxen that haul the caravan supplies.";
    }

    private string ObtenerDescripcionAlforjasPt(int tier)
    {
        return "Alforjes:\n"
            + "Adicionam +" + ObtenerBonusCargaPorBueyAlforjas(tier) + " de capacidade de carga aos bois que levam os suprimentos da caravana.";
    }

    private string ObtenerDescripcionTiendasEs(int tier)
    {
        return "Tiendas:\n"
            + "Las tiendas proporcionan refugio y descanso a los civiles.\n"
            + "Al descansar: +" + ObtenerBonusEsperanzaTiendas(tier) + " Esperanza.\n"
            + "Además aumenta en +" + Mathf.Max(0, tier - 1) + " la capacidad de Personajes.";
    }

    private string ObtenerDescripcionTiendasEn(int tier)
    {
        return "Tents:\n"
            + "Provide shelter and rest quality to the civilians.\n"
            + "When resting: +" + ObtenerBonusEsperanzaTiendas(tier) + " Hope.\n"
            + "Also increases character capacity by +" + Mathf.Max(0, tier - 1) + ".";
    }

    private string ObtenerDescripcionTiendasPt(int tier)
    {
        return "Tendas:\n"
            + "As tendas oferecem abrigo e descanso aos civis.\n"
            + "Ao descansar: +" + ObtenerBonusEsperanzaTiendas(tier) + " Esperança.\n"
            + "Além disso, aumentam em +" + Mathf.Max(0, tier - 1) + " a capacidade de Personagens.";
    }

    private string ObtenerDescripcionCatalejosEs(int tier)
    {
        return "Catalejos:\n"
            + "Aumentan en +" + ObtenerBonusVisionCatalejos(tier) + " el Rango de Visión de la caravana.\n"
            + "Además mejoran en " + ObtenerBonusCatalejos(tier) + "% la Exploración Pasiva.";
    }

    private string ObtenerDescripcionCatalejosEn(int tier)
    {
        return "Spyglasses:\n"
            + "Increase the caravan's Vision Range by +" + ObtenerBonusVisionCatalejos(tier) + ".\n"
            + "They also improve Passive Exploration by " + ObtenerBonusCatalejos(tier) + "%.";
    }

    private string ObtenerDescripcionCatalejosPt(int tier)
    {
        return "Lunetas:\n"
            + "Aumentam em +" + ObtenerBonusVisionCatalejos(tier) + " o Alcance de Visão da caravana.\n"
            + "Além disso, melhoram a Exploração Passiva em " + ObtenerBonusCatalejos(tier) + "%.";
    }

    private string ObtenerDescripcionAlmacenEs(int tier)
    {
        return "Carro Almacén:\n"
            + "Al descansar se consumen " + ObtenerReduccionConsumoAlmacen(tier) + "% menos Suministros.\n"
            + "Además aumenta en " + ObtenerBonusCargaAlmacen(tier) + " la capacidad total de carga.";
    }

    private string ObtenerDescripcionAlmacenEn(int tier)
    {
        return "Storage Wagon:\n"
            + "While resting, Supplies consumption is reduced by " + ObtenerReduccionConsumoAlmacen(tier) + "%.\n"
            + "It also increases total carrying capacity by " + ObtenerBonusCargaAlmacen(tier) + ".";
    }

    private string ObtenerDescripcionAlmacenPt(int tier)
    {
        return "Carro Armazém:\n"
            + "Ao descansar, consomem-se " + ObtenerReduccionConsumoAlmacen(tier) + "% menos Suprimentos.\n"
            + "Além disso, aumenta em " + ObtenerBonusCargaAlmacen(tier) + " a capacidade total de carga.";
    }

    private string ObtenerExplicacionVistaLejanaEs(int tierCatalejos)
    {
        return "<u>Rango de Visión:</u>\n"
            + "Determina la cantidad de nodos hacia adelante que la caravana puede divisar en el mapa. Permite planear mejor y prever cómo los caminos se van conformando."
            + ObtenerFactoresVistaLejanaEs(tierCatalejos);
    }

    private string ObtenerExplicacionVistaLejanaEn(int tierCatalejos)
    {
        return "<u>Vision Range:</u>\n"
            + "Determines how many nodes ahead the caravan can spot on the map. It helps you plan ahead and predict how roads will branch."
            + ObtenerFactoresVistaLejanaEn(tierCatalejos);
    }

    private string ObtenerExplicacionVistaLejanaPt(int tierCatalejos)
    {
        return "<u>Alcance de Visão:</u>\n"
            + "Determina quantos nós à frente a caravana consegue avistar no mapa. Permite planejar melhor e prever como os caminhos vão se formando."
            + ObtenerFactoresVistaLejanaPt(tierCatalejos);
    }

    private string ObtenerExplicacionExploracionPasivaEs(int tierCatalejos)
    {
        return "<u>Exploración Pasiva:</u>\n"
            + "Al descansar o al llegar a un nodo nuevo, la caravana realizará una tirada de Exploración que, de tener éxito, permitirá revelar los tipos de nodos adyacentes y a veces también los que estén más adelante."
            + ObtenerFactoresExploracionPasivaEs(tierCatalejos);
    }

    private string ObtenerExplicacionExploracionPasivaEn(int tierCatalejos)
    {
        return "<u>Passive Exploration:</u>\n"
            + "When resting or arriving at a new node, the caravan makes an Exploration roll. On success, it reveals the types of adjacent nodes and sometimes nodes farther ahead."
            + ObtenerFactoresExploracionPasivaEn(tierCatalejos);
    }

    private string ObtenerExplicacionExploracionPasivaPt(int tierCatalejos)
    {
        return "<u>Exploração Passiva:</u>\n"
            + "Ao descansar ou chegar a um novo nó, a caravana fará uma rolagem de Exploração. Em caso de sucesso, ela revela os tipos dos nós adjacentes e às vezes também os que estiverem mais à frente."
            + ObtenerFactoresExploracionPasivaPt(tierCatalejos);
    }

    private string ObtenerFactoresVistaLejanaEs(int tierCatalejos)
    {
        return ObtenerResumenFactoresVistaLejana(
            "Valor actual",
            "Clima",
            ObtenerNombreClimaVisionEs(),
            tierCatalejos);
    }

    private string ObtenerFactoresVistaLejanaEn(int tierCatalejos)
    {
        return ObtenerResumenFactoresVistaLejana(
            "Current value",
            "Weather",
            ObtenerNombreClimaVisionEn(),
            tierCatalejos);
    }

    private string ObtenerFactoresVistaLejanaPt(int tierCatalejos)
    {
        return ObtenerResumenFactoresVistaLejana(
            "Valor atual",
            "Clima",
            ObtenerNombreClimaVisionPt(),
            tierCatalejos);
    }

    private string ObtenerResumenFactoresVistaLejana(
        string etiquetaActual,
        string etiquetaClima,
        string nombreClima,
        int tierCatalejos)
    {
        CampaignManager cm = CampaignManager.Instance;
        int baseVision = cm != null ? cm.ObtenerDistanciaVisionBase() : 2;
        int bonusVisionCatalejos = cm != null ? cm.ObtenerBonusDistanciaVisionCatalejos() : ObtenerBonusVisionCatalejos(tierCatalejos);
        int penalizacionVisionClima = cm != null ? cm.ObtenerPenalizacionClimaVision() : 0;
        int minimoVision = cm != null ? cm.ObtenerDistanciaVisionMinima() : 1;
        int visionActual = cm != null ? cm.ObtenerDistanciaVisionEfectiva() : Mathf.Max(minimoVision, baseVision + bonusVisionCatalejos - penalizacionVisionClima);

        List<string> lineas = new List<string>
        {
            etiquetaActual + ": " + visionActual
        };

        if (penalizacionVisionClima != 0)
        {
            lineas.Add(etiquetaClima + " (" + nombreClima + "): " + FormatearModificadorEntero(-penalizacionVisionClima));
        }

        return ObtenerTextoFactoresGris(lineas);
    }

    private string ObtenerFactoresExploracionPasivaEs(int tierCatalejos)
    {
        return ObtenerResumenFactoresExploracionPasiva(
            "Valor actual",
            "Zona",
            "Clima",
            ObtenerNombreClimaVisionEs(),
            tierCatalejos);
    }

    private string ObtenerFactoresExploracionPasivaEn(int tierCatalejos)
    {
        return ObtenerResumenFactoresExploracionPasiva(
            "Current value",
            "Zone",
            "Weather",
            ObtenerNombreClimaVisionEn(),
            tierCatalejos);
    }

    private string ObtenerFactoresExploracionPasivaPt(int tierCatalejos)
    {
        return ObtenerResumenFactoresExploracionPasiva(
            "Valor atual",
            "Zona",
            "Clima",
            ObtenerNombreClimaVisionPt(),
            tierCatalejos);
    }

    private string ObtenerResumenFactoresExploracionPasiva(
        string etiquetaActual,
        string etiquetaZona,
        string etiquetaClima,
        string nombreClima,
        int tierCatalejos)
    {
        CampaignManager cm = CampaignManager.Instance;
        int chancePasiva = cm != null ? cm.ObtenerChanceExploracionPasiva() : 55 + ObtenerBonusCatalejos(tierCatalejos);
        int modZona = cm != null && cm.scAtributosZona != null ? cm.scAtributosZona.modChanceExploracion : 0;
        int modClimaExploracion = cm != null && cm.intTipoClima == 5 ? -20 : 0;

        List<string> lineas = new List<string>
        {
            etiquetaActual + ": " + chancePasiva + "%"
        };

        if (modZona != 0)
        {
            lineas.Add(etiquetaZona + ": " + FormatearModificadorPorcentaje(modZona));
        }

        if (modClimaExploracion != 0)
        {
            lineas.Add(etiquetaClima + " (" + nombreClima + "): " + FormatearModificadorPorcentaje(modClimaExploracion));
        }

        return ObtenerTextoFactoresGris(lineas);
    }

    private string ObtenerTextoFactoresGris(List<string> lineas)
    {
        return "\n\n<color=" + ColorTextoFactoresExploracion + ">" + string.Join("\n", lineas) + "</color>";
    }

    private string FormatearModificadorEntero(int valor)
    {
        if (valor > 0)
        {
            return "+" + valor;
        }

        return valor.ToString();
    }

    private string FormatearModificadorPorcentaje(int valor)
    {
        return FormatearModificadorEntero(valor) + "%";
    }

    private string ObtenerNombreClimaVisionEs()
    {
        if (CampaignManager.Instance == null)
        {
            return "Desconocido";
        }

        switch (CampaignManager.Instance.intTipoClima)
        {
            case 1: return "Soleado";
            case 2: return "Ola de Calor";
            case 3: return "Lluvia";
            case 4: return "Nieve";
            case 5: return "Niebla";
            case 6: return "Almas Danzantes";
            case 7: return "Aurora Boreal";
            case 8: return "Nedukazal a oscuras";
            case 9: return "Masacre de Nedukazal";
            default: return "Desconocido";
        }
    }

    private string ObtenerNombreClimaVisionEn()
    {
        if (CampaignManager.Instance == null)
        {
            return "Unknown";
        }

        switch (CampaignManager.Instance.intTipoClima)
        {
            case 1: return "Sunny";
            case 2: return "Heat Wave";
            case 3: return "Rain";
            case 4: return "Snow";
            case 5: return "Fog";
            case 6: return "Dancing Souls";
            case 7: return "Aurora Borealis";
            case 8: return "Nedukazal in Darkness";
            case 9: return "Nedukazal Massacre";
            default: return "Unknown";
        }
    }

    private string ObtenerNombreClimaVisionPt()
    {
        if (CampaignManager.Instance == null)
        {
            return "Desconhecido";
        }

        switch (CampaignManager.Instance.intTipoClima)
        {
            case 1: return "Ensolarado";
            case 2: return "Onda de Calor";
            case 3: return "Chuva";
            case 4: return "Neve";
            case 5: return "Névoa";
            case 6: return "Almas Dançantes";
            case 7: return "Aurora Boreal";
            case 8: return "Nedukazal na escuridão";
            case 9: return "Massacre de Nedukazal";
            default: return "Desconhecido";
        }
    }

    private string ObtenerDescripcionDefensasEs(int tier)
    {
        return "Defensas:\n"
            + "Agrega trampas y barricadas a los combates de ataques directos a la caravana.\n"
            + "Además reduce " + ObtenerReduccionPerdidaSequitoDefensas(tier) + "% las chances de perder un séquito tras una derrota.";
    }

    private string ObtenerDescripcionDefensasEn(int tier)
    {
        return "Defenses:\n"
            + "Adds traps and barricades to direct caravan defense battles.\n"
            + "Also reduces the chance to lose a Retinue after a defeat by " + ObtenerReduccionPerdidaSequitoDefensas(tier) + "%.";
    }

    private string ObtenerDescripcionDefensasPt(int tier)
    {
        return "Defesas:\n"
            + "Adiciona armadilhas e barricadas aos combates de ataques diretos à caravana.\n"
            + "Além disso, reduz em " + ObtenerReduccionPerdidaSequitoDefensas(tier) + "% as chances de perder um séquito após uma derrota.";
    }
}
