using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuCaravana : MonoBehaviour
{
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

     //Antorchas de Pie
    [SerializeField] TextMeshProUGUI txtTierMejoraAntorchas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAntorchas;
    [SerializeField] GameObject btMejoraAntorchas;
    int costoMejorarAntorchas = 0;

    //Alforjas
    [SerializeField] TextMeshProUGUI txtTierMejoraAlforjas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAlforjas;
    [SerializeField] GameObject btMejoraAlforjas;
    int costoMejorarAlforjas = 0;

    //Tiendas
    [SerializeField] TextMeshProUGUI txtTierMejoraTiendas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraTiendas;
    [SerializeField] GameObject btMejoraTiendas;
    int costoMejorarTiendas = 0;

    //Catalejos
    [SerializeField] TextMeshProUGUI txtTierMejoraCatalejos;
    [SerializeField] TextMeshProUGUI txtCostoMejoraCatalejos;
    [SerializeField] GameObject btMejoraCatalejos;
    int costoMejorarCatalejos = 0;

    //Almacen
    [SerializeField] TextMeshProUGUI txtTierMejoraAlmacen;
    [SerializeField] TextMeshProUGUI txtCostoMejoraAlmacen;
    [SerializeField] GameObject btMejoraAlmacen;
    int costoMejorarAlmacen = 0;

     //Defensas
    [SerializeField] TextMeshProUGUI txtTierMejoraDefensas;
    [SerializeField] TextMeshProUGUI txtCostoMejoraDefensas;
    [SerializeField] GameObject btMejoraDefensas;
    int costoMejorarDefensas = 0;

     [SerializeField] GameObject contenedorSequitos;

    public bool SeApretoESC()
    { 
        bool habiaalgoabierto = MenuMejoras.activeInHierarchy || MenuSequitos.activeInHierarchy || MenuPersonajes.activeInHierarchy;
        
        MenuMejoras.SetActive(false);
        MenuSequitos.SetActive(false);
        MenuPersonajes.SetActive(false);

        return habiaalgoabierto;
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

    public void AbrirMenuMejoras()
    {
        print(123123);
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 15) { return; }
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 15)
        { CampaignManager.Instance.scTutorialManager.SiguientePaso(); }

        bool abrir = !MenuMejoras.activeInHierarchy;
        ActualizarMejoras();
        MenuPersonajes.SetActive(false);
        MenuSequitos.SetActive(false);
        MenuMejoras.SetActive(abrir);
        if (abrir)
        {
            RuntimeAnalytics.TrackDesign("ui", "caravan", "open_upgrades");
        }

    }
    public void AbrirMenuSequitos()
    {
        if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual < 27) { return; }
         if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 27)
        {            CampaignManager.Instance.scTutorialManager.SiguientePaso();        }
          if (CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 29)
        {            CampaignManager.Instance.scTutorialManager.SiguientePaso();        }

        MenuPersonajes.SetActive(false);
        MenuMejoras.SetActive(false);
        bool abrir = !MenuSequitos.activeInHierarchy;
        MenuSequitos.SetActive(abrir);
        if (abrir)
        {
            RuntimeAnalytics.TrackDesign("ui", "caravan", "open_followers");
        }
      
        
        
       
    }
    public void AbrirMenuPersonajes()
    {
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

        MenuMejoras.SetActive(false);
        MenuSequitos.SetActive(false);
        bool abrir = !MenuPersonajes.activeInHierarchy;
        MenuPersonajes.SetActive(abrir);
        if (!abrir) return;

        RuntimeAnalytics.TrackDesign("ui", "caravan", "open_characters");

        var scMenuPersonajes = MenuPersonajes.GetComponent<MenuPersonajes>();
        if (scMenuPersonajes == null) return;

        Personaje personajeInicial = null;
        if (scMenuPersonajes.listaPersonajes != null && scMenuPersonajes.listaPersonajes.Count > 0)
        {
            personajeInicial = scMenuPersonajes.listaPersonajes.Find(p => p != null && !p.Camp_Muerto);
            if (personajeInicial == null)
            {
                personajeInicial = scMenuPersonajes.listaPersonajes[0];
            }
        }

        scMenuPersonajes.PrepararYAbrirMenu(personajeInicial);
        scMenuPersonajes.itemDesc.text = "";

     
       
    }
    public void ActualizarMejoras()
    {
        InicializarColoresCostoSiHaceFalta();

        //Antorchas
        costoMejorarAntorchas = 30 + (10 * CampaignManager.Instance.mejoraCaravanaAntorchas);
        txtTierMejoraAntorchas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaAntorchas;
        txtCostoMejoraAntorchas.text = "" + costoMejorarAntorchas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAntorchas, costoMejorarAntorchas, colorCostoAntorchasNormal);
        if (CampaignManager.Instance.mejoraCaravanaAntorchas == 3) { btMejoraAntorchas.SetActive(false); }

        //Alforjas
        costoMejorarAlforjas = 25 + (9 * CampaignManager.Instance.mejoraCaravanaAlforjas);
        txtTierMejoraAlforjas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaAlforjas;
        txtCostoMejoraAlforjas.text = "" + costoMejorarAlforjas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAlforjas, costoMejorarAlforjas, colorCostoAlforjasNormal);
        if (CampaignManager.Instance.mejoraCaravanaAlforjas == 3) { btMejoraAlforjas.SetActive(false); }

        //Tiendas
        costoMejorarTiendas = Mathf.CeilToInt((45 + (15 * CampaignManager.Instance.mejoraCaravanaTiendas)) * 1.15f);
        txtTierMejoraTiendas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaTiendas;
        txtCostoMejoraTiendas.text = "" + costoMejorarTiendas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraTiendas, costoMejorarTiendas, colorCostoTiendasNormal);
        if (CampaignManager.Instance.mejoraCaravanaTiendas == 3) { btMejoraTiendas.SetActive(false); }

        //Catalejos
        costoMejorarCatalejos = 35 + (11 * CampaignManager.Instance.mejoraCaravanaCatalejos);
        txtTierMejoraCatalejos.text = "Tier " + CampaignManager.Instance.mejoraCaravanaCatalejos;
        txtCostoMejoraCatalejos.text = "" + costoMejorarCatalejos + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraCatalejos, costoMejorarCatalejos, colorCostoCatalejosNormal);
        if (CampaignManager.Instance.mejoraCaravanaCatalejos == 3) { btMejoraCatalejos.SetActive(false); }

        //Almacen
        costoMejorarAlmacen = 35 + (11 * CampaignManager.Instance.mejoraCaravanaAlmacen);
        txtTierMejoraAlmacen.text = "Tier " + CampaignManager.Instance.mejoraCaravanaAlmacen;
        txtCostoMejoraAlmacen.text = "" + costoMejorarAlmacen + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraAlmacen, costoMejorarAlmacen, colorCostoAlmacenNormal);
        if (CampaignManager.Instance.mejoraCaravanaAlmacen == 3) { btMejoraAlmacen.SetActive(false); }
        
        //Defensas
        costoMejorarDefensas = 25 + (11 * CampaignManager.Instance.mejoraCaravanaDefensas);
        txtTierMejoraDefensas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaDefensas;
        txtCostoMejoraDefensas.text = "" + costoMejorarDefensas + TRADU.i.Traducir(" Materiales");
        ActualizarColorCosto(txtCostoMejoraDefensas, costoMejorarDefensas, colorCostoDefensasNormal);
        if (CampaignManager.Instance.mejoraCaravanaDefensas == 3) { btMejoraDefensas.SetActive(false); }
    }


    public void MejorarAntorchas()
    {
       if(costoMejorarAntorchas <= CampaignManager.Instance.GetMaterialesActuales())
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
    {
       if(costoMejorarAlforjas <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaAlforjas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAlforjas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarAlforjas, "caravan_upgrade", "alforjas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "alforjas");
       }
       if(CampaignManager.Instance.scTutorialManager.tutorialActivo && CampaignManager.Instance.scTutorialManager.pasoActual == 16)
       {
           AbrirMenuMejoras();
           CampaignManager.Instance.scTutorialManager.SiguientePaso();
       }

        ActualizarMejoras();

    }

    public void MejorarTiendas()
    {
       if(costoMejorarTiendas <= CampaignManager.Instance.GetMaterialesActuales())
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
       if(costoMejorarCatalejos <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaCatalejos += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarCatalejos);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarCatalejos, "caravan_upgrade", "catalejos");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "catalejos");
       }

        ActualizarMejoras();

    }

    public void MejorarAlmacen()
    {
       if(costoMejorarAlmacen <= CampaignManager.Instance.GetMaterialesActuales())
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
       if(costoMejorarDefensas <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaDefensas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarDefensas);
        RuntimeAnalytics.TrackResourceSink("materials", costoMejorarDefensas, "caravan_upgrade", "defensas");
        RuntimeAnalytics.TrackDesign("campaign", "caravan_upgrade", "defensas");
       }

        ActualizarMejoras();

    }
}
