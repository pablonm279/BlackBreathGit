using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuCaravana : MonoBehaviour
{
   
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
    public void AbrirMenuMejoras()
    {
        ActualizarMejoras();
        MenuPersonajes.SetActive(false);
        MenuSequitos.SetActive(false);
        MenuMejoras.SetActive(!MenuMejoras.activeInHierarchy);

    }
    public void AbrirMenuSequitos()
    {
        
        MenuPersonajes.SetActive(false);
        MenuMejoras.SetActive(false);
        MenuSequitos.SetActive(!MenuSequitos.activeInHierarchy);
       
    }
    public void AbrirMenuPersonajes()
    {
        
       
        MenuMejoras.SetActive(false);
        MenuSequitos.SetActive(false);
        MenuPersonajes.SetActive(!MenuPersonajes.activeInHierarchy);
        MenuPersonajes.GetComponent<MenuPersonajes>().SeleccionarPersonaje(MenuPersonajes.GetComponent<MenuPersonajes>().listaPersonajes[0], null);
        MenuPersonajes.GetComponent<MenuPersonajes>().ActualizarLista();
        MenuPersonajes.GetComponent<MenuPersonajes>().itemDesc.text = "";
       
    }
    public void ActualizarMejoras()
    {
        //Antorchas
        costoMejorarAntorchas = 30 + (10 * CampaignManager.Instance.mejoraCaravanaAntorchas);
        txtTierMejoraAntorchas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaAntorchas;
        txtCostoMejoraAntorchas.text = "" + costoMejorarAntorchas + " Materiales";
        if (costoMejorarAntorchas <= CampaignManager.Instance.GetMaterialesActuales())
        { /*txtCostoMejoraAntorchas.color = Color.white;*/ }
        else { txtCostoMejoraAntorchas.color = Color.red; }
        if (CampaignManager.Instance.mejoraCaravanaAntorchas == 3) { btMejoraAntorchas.SetActive(false); }

        //Alforjas
        costoMejorarAlforjas = 25 + (9 * CampaignManager.Instance.mejoraCaravanaAlforjas);
        txtTierMejoraAlforjas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaAlforjas;
        txtCostoMejoraAlforjas.text = "" + costoMejorarAlforjas + " Materiales";
        if (costoMejorarAlforjas <= CampaignManager.Instance.GetMaterialesActuales())
        { /*txtCostoMejoraAlforjas.color = Color.white;*/ }
        else { txtCostoMejoraAlforjas.color = Color.red; }
        if (CampaignManager.Instance.mejoraCaravanaAlforjas == 3) { btMejoraAlforjas.SetActive(false); }

        //Tiendas
        costoMejorarTiendas = 45 + (15 * CampaignManager.Instance.mejoraCaravanaTiendas);
        txtTierMejoraTiendas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaTiendas;
        txtCostoMejoraTiendas.text = "" + costoMejorarTiendas + " Materiales";
        if (costoMejorarTiendas <= CampaignManager.Instance.GetMaterialesActuales())
        { /*txtCostoMejoraTiendas.color = Color.white;*/ }
        else { txtCostoMejoraTiendas.color = Color.red; }
        if (CampaignManager.Instance.mejoraCaravanaTiendas == 3) { btMejoraTiendas.SetActive(false); }

        //Catalejos
        costoMejorarCatalejos = 35 + (11 * CampaignManager.Instance.mejoraCaravanaCatalejos);
        txtTierMejoraCatalejos.text = "Tier " + CampaignManager.Instance.mejoraCaravanaCatalejos;
        txtCostoMejoraCatalejos.text = "" + costoMejorarCatalejos + " Materiales";
        if (costoMejorarCatalejos <= CampaignManager.Instance.GetMaterialesActuales())
        { /*txtCostoMejoraCatalejos.color = Color.white; */}
        else { txtCostoMejoraCatalejos.color = Color.red; }
        if (CampaignManager.Instance.mejoraCaravanaCatalejos == 3) { btMejoraCatalejos.SetActive(false); }

        //Almacen
        costoMejorarAlmacen = 35 + (11 * CampaignManager.Instance.mejoraCaravanaAlmacen);
        txtTierMejoraAlmacen.text = "Tier " + CampaignManager.Instance.mejoraCaravanaAlmacen;
        txtCostoMejoraAlmacen.text = "" + costoMejorarAlmacen + " Materiales";
        if (costoMejorarAlmacen <= CampaignManager.Instance.GetMaterialesActuales())
        { /*txtCostoMejoraAlmacen.color = Color.white;*/ }
        else { txtCostoMejoraAlmacen.color = Color.red; }
        if (CampaignManager.Instance.mejoraCaravanaAlmacen == 3) { btMejoraAlmacen.SetActive(false); }
        
        //Defensas
        costoMejorarDefensas = 25 + (11 * CampaignManager.Instance.mejoraCaravanaDefensas);
        txtTierMejoraDefensas.text = "Tier " + CampaignManager.Instance.mejoraCaravanaDefensas;
        txtCostoMejoraDefensas.text = "" + costoMejorarDefensas + " Materiales";
        if (costoMejorarDefensas <= CampaignManager.Instance.GetMaterialesActuales())
        { /*txtCostoMejoraDefensas.color = Color.white;*/ }
        else { txtCostoMejoraDefensas.color = Color.red; }
        if (CampaignManager.Instance.mejoraCaravanaDefensas == 3) { btMejoraDefensas.SetActive(false); }
    }


    public void MejorarAntorchas()
    {
       if(costoMejorarAntorchas <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaAntorchas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAntorchas);
       }

        ActualizarMejoras();

    }
    public void MejorarAlforjas()
    {
       if(costoMejorarAlforjas <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaAlforjas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAlforjas);
       }

        ActualizarMejoras();

    }

    public void MejorarTiendas()
    {
       if(costoMejorarTiendas <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaTiendas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarTiendas);
       }

        ActualizarMejoras();

    }

    public void MejorarCatalejos()
    {
       if(costoMejorarCatalejos <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaCatalejos += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarCatalejos);
       }

        ActualizarMejoras();

    }

    public void MejorarAlmacen()
    {
       if(costoMejorarAlmacen <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaAlmacen += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarAlmacen);
       }

        ActualizarMejoras();

    }

    public void MejorarDefensas()
    {
       if(costoMejorarDefensas <= CampaignManager.Instance.GetMaterialesActuales())
       {
        CampaignManager.Instance.mejoraCaravanaDefensas += 1;
        CampaignManager.Instance.CambiarMaterialesActuales(-costoMejorarDefensas);
       }

        ActualizarMejoras();

    }
}
