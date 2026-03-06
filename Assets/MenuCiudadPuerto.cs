using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuCiudadPuerto : MonoBehaviour
{

    public TextMeshProUGUI txtresumenPartida;

    public TextMeshProUGUI txtValorValorTrabajo;
    public TextMeshProUGUI txtValorCorrupcion;
    public TextMeshProUGUI txtPuedeEsperartantascav;
    public TextMeshProUGUI txtMisionesCantidad;


    public TextMeshProUGUI txtPeligroBosque;
    public TextMeshProUGUI txtPeligroPaso;
    public TextMeshProUGUI txtPeligroNedukazal;

    public GameObject menuMejorasValorTrabajo;

    public GameObject tituloEspaniol;
    public GameObject tituloIngles;
    private bool recompensasFinalAplicadas;

    
     void OnEnable()
    {
        CrearResumenPartida();
    }


    void CrearResumenPartida()
    {
        float valortrabajo = CampaignManager.Instance.GetCivilesActual() + (CampaignManager.Instance.GetOroActuales() / 10);
        txtresumenPartida.text = TRADU.i.Traducir("El viaje ha durado ") + CampaignManager.Instance.logDeCampania.GetDiaActual() + TRADU.i.Traducir(" días enteros y han sobrevivido ") + CampaignManager.Instance.GetCivilesActual() + " " + TRADU.i.Traducir("civiles.\n\n");
        txtresumenPartida.text += TRADU.i.Traducir("Además, el oro restante (") + CampaignManager.Instance.GetOroActuales() + TRADU.i.Traducir(") se ha donado a las arcas de la ciudad para ayudar a financiar la evacuación.\n\nLos Personajes sobrevivientes también se han unido al esfuerzo de evacuación para defender la ciudad.\n\n");
        txtresumenPartida.text += TRADU.i.Traducir("<b>Valor de Trabajo obtenido: ") + valortrabajo + "</b>";

        txtMisionesCantidad.text = MetaprogresionManager.Instance.MisionesSalvamento+"";

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PausarMusica(true);
        }



        if (!recompensasFinalAplicadas)
        {
            MetaprogresionManager.Instance.ValordeTrabajoDisponible += (int)valortrabajo;
            MetaprogresionManager.Instance.CantidadCiviles += (int)CampaignManager.Instance.GetCivilesActual();
            recompensasFinalAplicadas = true;
        }

        txtValorValorTrabajo.text = "" + MetaprogresionManager.Instance.ValordeTrabajoDisponible;
        txtValorCorrupcion.text = MetaprogresionManager.Instance.CorrupcionGlobal + "/" + MetaprogresionManager.Instance.CorrupcionMax;
        txtPuedeEsperartantascav.text = TRADU.i.Traducir("La ciudad puede permitirse esperar ") + (MetaprogresionManager.Instance.CorrupcionMax - MetaprogresionManager.Instance.CorrupcionGlobal) + TRADU.i.Traducir(" caravanas más antes de tener que zarpar.");
        if (MetaprogresionManager.Instance.NivelPeligroBosqueArdiente > 0)
        {
            txtPeligroBosque.text = TRADU.i.Traducir("El <b>Nivel de Peligro</b> actual en el Bosque Ardiente es: ") + MetaprogresionManager.Instance.NivelPeligroBosqueArdiente;
            ColorearSegunIntensidad(txtPeligroBosque, MetaprogresionManager.Instance.NivelPeligroBosqueArdiente);
        }
        else { txtPeligroBosque.text = ""; }
        if (MetaprogresionManager.Instance.NivelPeligroPasoVientohelado > 0)
        {
            txtPeligroPaso.text = TRADU.i.Traducir("El <b>Nivel de Peligro</b> actual en el Paso Vientohelado es: ") + MetaprogresionManager.Instance.NivelPeligroPasoVientohelado;
            ColorearSegunIntensidad(txtPeligroPaso, MetaprogresionManager.Instance.NivelPeligroPasoVientohelado);
        }
        else { txtPeligroPaso.text = ""; }
        if (MetaprogresionManager.Instance.NivelPeligroNedukazal > 0)
        {
            txtPeligroNedukazal.text = TRADU.i.Traducir("El <b>Nivel de Peligro</b> actual en Nedukazal es: ") + MetaprogresionManager.Instance.NivelPeligroNedukazal;
            ColorearSegunIntensidad(txtPeligroNedukazal, MetaprogresionManager.Instance.NivelPeligroNedukazal);
        }
        else { txtPeligroNedukazal.text = ""; }

        ActualizarValores();
        AplicarIdiomaTitulo();

    }


    void ColorearSegunIntensidad(TextMeshProUGUI texto, int nivel)
    {
        if (nivel >= 3)
        {
            texto.color = Color.red;
        }
        else if (nivel > 1)
        {
            texto.color = new Color(1f, 0.65f, 0f); // Naranja
        }
        else
        {
            texto.color = Color.yellow;
        }

    }


    public void AbrirMenuMejorasValorTrabajo()
    {
        menuMejorasValorTrabajo.SetActive(!menuMejorasValorTrabajo.activeInHierarchy);

        ActualizarValores();
    }


    public TextMeshProUGUI txtValorCiviles;
    public TextMeshProUGUI txtValorTransporte;
    public TextMeshProUGUI txtValorTrabajoDisponible;
    public void ActualizarValores()
    {
        txtValorTrabajoDisponible.text = "" + MetaprogresionManager.Instance.ValordeTrabajoDisponible;
        txtValorCiviles.text = "" + MetaprogresionManager.Instance.CantidadCiviles;
        txtValorTransporte.text = "" + MetaprogresionManager.Instance.SerriaTierBarcos * 120;

        int valotrBaseGranja = 40 - (MetaprogresionManager.Instance.SerriaTierGranjas * 5);
        txtAlmacenMejoraBarcos.text = "" + MetaprogresionManager.Instance.SerriaPuntosAlmacenadosBarcos + "/" + (valotrBaseGranja + (MetaprogresionManager.Instance.SerriaTierBarcos * 15));
        txtAlmacenMejoraPalacio.text = "" + MetaprogresionManager.Instance.SerriaPuntosAlmacenadosPalacio + "/" + (valotrBaseGranja + (MetaprogresionManager.Instance.SerriaTierPalacio * 15));
        txtAlmacenMejoraCuartel.text = "" + MetaprogresionManager.Instance.SerriaPuntosAlmacenadosCuartel + "/" + (valotrBaseGranja + (MetaprogresionManager.Instance.SerriaTierCuartel * 15));
        txtAlmacenMejoraTemplo.text = "" + MetaprogresionManager.Instance.SerriaPuntosAlmacenadosTemplo + "/" + (valotrBaseGranja + (MetaprogresionManager.Instance.SerriaTierTemplo * 15));
        txtAlmacenMejoraGranja.text = "" + MetaprogresionManager.Instance.SerriaPuntosAlmacenadosGranjas + "/" + (40 + (MetaprogresionManager.Instance.SerriaTierGranjas * 15));
        txtAlmacenMejoraBarricada.text = "" + MetaprogresionManager.Instance.SerriaPuntosAlmacenadosBarricadas + "/" + (valotrBaseGranja + (MetaprogresionManager.Instance.SerriaTierBarricadas * 15));
        txtAlmacenMejoraAlmenara.text = "" + MetaprogresionManager.Instance.SerriaPuntosAlmacenadosAlmenaras + "/" + (valotrBaseGranja + (MetaprogresionManager.Instance.SerriaTierAlmenaras * 15));

        if (MetaprogresionManager.Instance.SerriaTierBarcos == 3) { txtAlmacenMejoraBarcos.text = "---"; }
        if (MetaprogresionManager.Instance.SerriaTierPalacio == 3) { txtAlmacenMejoraPalacio.text = "---"; }
        if (MetaprogresionManager.Instance.SerriaTierCuartel == 3) { txtAlmacenMejoraCuartel.text = "---"; }
        if (MetaprogresionManager.Instance.SerriaTierTemplo == 3) { txtAlmacenMejoraTemplo.text = "---"; }
        if (MetaprogresionManager.Instance.SerriaTierGranjas == 3) { txtAlmacenMejoraGranja.text = "---"; }
        if (MetaprogresionManager.Instance.SerriaTierBarricadas == 3) { txtAlmacenMejoraBarricada.text = "---"; }
        if (MetaprogresionManager.Instance.SerriaTierAlmenaras == 3) { txtAlmacenMejoraAlmenara.text = "---"; }

        txtTierBarcos.text = "" + Obtenernumeroromano(MetaprogresionManager.Instance.SerriaTierBarcos);
        txtTierPalacio.text = "" + Obtenernumeroromano(MetaprogresionManager.Instance.SerriaTierPalacio);
        txtTierCuartel.text = "" + Obtenernumeroromano(MetaprogresionManager.Instance.SerriaTierCuartel);
        txtTierTemplo.text = "" + Obtenernumeroromano(MetaprogresionManager.Instance.SerriaTierTemplo);
        txtTierGranja.text = "" + Obtenernumeroromano(MetaprogresionManager.Instance.SerriaTierGranjas);
        txtTierBarricada.text = "" + Obtenernumeroromano(MetaprogresionManager.Instance.SerriaTierBarricadas);
        txtTierAlmenara.text = "" + Obtenernumeroromano(MetaprogresionManager.Instance.SerriaTierAlmenaras);
        
        txtMisionesCantidad.text = MetaprogresionManager.Instance.MisionesSalvamento+"";
    }

    string Obtenernumeroromano(int numero)
    {
        switch (numero)
        {
            case 0:
                return  "I";
            case 1:
                return  "II";
            case 2:
                return  "III";
            case 3:
                return  "IV";
            case 4:
                return  "V";
            default:
                return  "I";
        }
    }   

    public GameObject tooltipMejoraBarcos;
    public GameObject tooltipMejoraPalacio;
    public GameObject tooltipMejoraCuartel;
    public GameObject tooltipMejoraTemplo;
    public GameObject tooltipMejoraGranja;
    public GameObject tooltipMejoraBarricada;
    public GameObject tooltipMejoraAlmenara;

    public TextMeshProUGUI txtAlmacenMejoraBarcos;
    public TextMeshProUGUI txtAlmacenMejoraPalacio;
    public TextMeshProUGUI txtAlmacenMejoraCuartel;
    public TextMeshProUGUI txtAlmacenMejoraTemplo;
    public TextMeshProUGUI txtAlmacenMejoraGranja;
    public TextMeshProUGUI txtAlmacenMejoraBarricada;
    public TextMeshProUGUI txtAlmacenMejoraAlmenara;

    public TextMeshProUGUI txtTierBarcos;
    public TextMeshProUGUI txtTierPalacio;
    public TextMeshProUGUI txtTierCuartel;
    public TextMeshProUGUI txtTierTemplo;
    public TextMeshProUGUI txtTierGranja;
    public TextMeshProUGUI txtTierBarricada;
    public TextMeshProUGUI txtTierAlmenara;
    public void HoverBotonMejora(int n)
    {
        ActualizarValores();
        HoverBotonMejoraSalir();

        GameObject tooltip = ObtenerTooltipMejora(n);
        if (tooltip == null) return;

        tooltip.SetActive(true);
        ActivarContenidoTooltipPorIdioma(tooltip);

    }

    public void HoverBotonMejoraSalir()
    {
        if (tooltipMejoraBarcos != null) tooltipMejoraBarcos.SetActive(false);
        if (tooltipMejoraPalacio != null) tooltipMejoraPalacio.SetActive(false);
        if (tooltipMejoraCuartel != null) tooltipMejoraCuartel.SetActive(false);
        if (tooltipMejoraTemplo != null) tooltipMejoraTemplo.SetActive(false);
        if (tooltipMejoraGranja != null) tooltipMejoraGranja.SetActive(false);
        if (tooltipMejoraBarricada != null) tooltipMejoraBarricada.SetActive(false);
        if (tooltipMejoraAlmenara != null) tooltipMejoraAlmenara.SetActive(false);


    }

    private GameObject ObtenerTooltipMejora(int n)
    {
        switch (n)
        {
            case 1: return tooltipMejoraBarcos;
            case 2: return tooltipMejoraPalacio;
            case 3: return tooltipMejoraCuartel;
            case 4: return tooltipMejoraTemplo;
            case 5: return tooltipMejoraGranja;
            case 6: return tooltipMejoraBarricada;
            case 7: return tooltipMejoraAlmenara;
            default: return null;
        }
    }

    private void ActivarContenidoTooltipPorIdioma(GameObject tooltip)
    {
        if (tooltip == null) return;
        int childCount = tooltip.transform.childCount;
        if (childCount <= 0) return;

        int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
        int childIdioma = idioma == 2 ? 1 : 0;
        int panelesIdioma = Mathf.Min(2, childCount);

        for (int i = 0; i < panelesIdioma; i++)
        {
            tooltip.transform.GetChild(i).gameObject.SetActive(i == childIdioma);
        }

        if (childIdioma >= panelesIdioma && panelesIdioma > 0)
        {
            tooltip.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    private void AplicarIdiomaTitulo()
    {
        int idioma = TRADU.i != null ? TRADU.i.nIdioma : 1;
        bool esIngles = idioma == 2;
        if (tituloEspaniol != null) tituloEspaniol.SetActive(!esIngles);
        if (tituloIngles != null) tituloIngles.SetActive(esIngles);
    }
    
     public void ClickBotonMejora(int n)
    {
      

        if (n == 1)
        {
            if (MetaprogresionManager.Instance.ValordeTrabajoDisponible > 4)
            { 
              
                MetaprogresionManager.Instance.AumentarAlmacenadosBarcos();;
            }
        }
        else if (n == 2)
        {
             if (MetaprogresionManager.Instance.ValordeTrabajoDisponible > 4)
            { 
               
                MetaprogresionManager.Instance.AumentarAlmacenadosPalacio();
            }
        }
        else if (n == 3)
        {
             if (MetaprogresionManager.Instance.ValordeTrabajoDisponible > 4)
            { 
              
                MetaprogresionManager.Instance.AumentarAlmacenadosCuartel();
            }
        }
        else if (n == 4)
        {
             if (MetaprogresionManager.Instance.ValordeTrabajoDisponible > 4)
            { 
               
                MetaprogresionManager.Instance.AumentarAlmacenadosTemplo();
            }
        }
        else if (n == 5)
        {
            if (MetaprogresionManager.Instance.ValordeTrabajoDisponible > 4)
            { 
             
                MetaprogresionManager.Instance.AumentarAlmacenadosGranjas();
            }
        }
        else if (n == 6)
        {
            if (MetaprogresionManager.Instance.ValordeTrabajoDisponible > 4)
            { 
               
                MetaprogresionManager.Instance.AumentarAlmacenadosBarricadas();
            }
        }
     else if (n == 7)
     {
         if (MetaprogresionManager.Instance.ValordeTrabajoDisponible > 4)
         { 
             
             MetaprogresionManager.Instance.AumentarAlmacenadosAlmenaras();
         }

     }


         ActualizarValores();
    }

    public void VolverAlMenuPrincipal()
    {   
        PlayerPrefs.Save();
        Time.timeScale = 1f; // asegurar que la proxima escena no quede pausada
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PausarMusica(false);
            MusicManager.Instance.FadeOutYParar(0.5f);
        }
        SceneManager.LoadScene("ES-MenuPrincipal");
    }



}



