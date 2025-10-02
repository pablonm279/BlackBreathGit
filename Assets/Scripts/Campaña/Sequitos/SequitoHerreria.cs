using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoHerreria : MonoBehaviour
{
   
   
   //Mejoras Milicia
   [SerializeField] TextMeshProUGUI txtDescrMejoraMilicia;
   [SerializeField] TextMeshProUGUI txtTierMilicia;
   [SerializeField] TextMeshProUGUI txtCostoMejoraMilicia;
   [SerializeField] GameObject btnMejorarMilicias;

   //Mantenimiento Armas
   [SerializeField] TextMeshProUGUI txtDescrMantArma;
   [SerializeField] TextMeshProUGUI txtCostoMantArma;

   //Mantenimiento Armaduras
   [SerializeField] TextMeshProUGUI txtDescrMantArmaduras;
   [SerializeField] TextMeshProUGUI txtCostoMantArmaduras;
   

  

   void OnEnable()
   {
    Actualizar();
   }

   int valormejora = 0;

  
  
   void Actualizar()
   {
   if(txtDescrMejoraMilicia!=null)
   {
    txtDescrMantArma.text = "Mantenimiento Armas: El Herrero se encargará de hacer un mantenimiento general de las armas de los personajes. Aumentando su Ataque en 1 y su daño en 2. Este efecto Dura 3 días.";
    txtDescrMantArmaduras.text = "Mantenimiento Armaduras: El Herrero se encargará de hacer un mantenimiento general de las armaduras de los personajes. Aumentando su Defensa en 1 y su Armadura en 2. Este efecto dura 3 días.";
    //Mantenimiento Armas
    if( CampaignManager.Instance.sequitoHerrerosMantArmas == 0)
    {txtCostoMantArma.text = "Realizar: 200 Oro"; txtCostoMantArma.color = Color.yellow;}
    else{txtCostoMantArma.text = $"Activo por { CampaignManager.Instance.sequitoHerrerosMantArmas} Días";txtCostoMantArma.color = Color.green;}

    //Mantenimiento Armaduras
    if( CampaignManager.Instance.sequitoHerrerosMantArmaduras == 0)
    {txtCostoMantArmaduras.text = "Realizar: 200 Oro"; txtCostoMantArmaduras.color = Color.yellow;}
    else{txtCostoMantArmaduras.text = $"Activo por { CampaignManager.Instance.sequitoHerrerosMantArmaduras} Días";txtCostoMantArmaduras.color = Color.green;}




    //Mejora Milicias
    txtDescrMejoraMilicia.text = "Armas Civiles: El herrero se dedica a mejorar las armas rudimentarias de los civiles, mejorando las posibilidades de defensa de las Milicias. \nCada Tier aumenta en 10% los Civiles que suman fuerza para la Milicia.";
    txtTierMilicia.text = "Tier "+CampaignManager.Instance.miliciasMejoras/10;
    valormejora = 20+(CampaignManager.Instance.miliciasMejoras*2);
    txtCostoMejoraMilicia.text = valormejora+" Materiales";


    if(CampaignManager.Instance.miliciasMejoras > 20)
    {
        btnMejorarMilicias.SetActive(false);
    }
    else{btnMejorarMilicias.SetActive(true);}

    if(CampaignManager.Instance.GetMaterialesActuales() < valormejora)
    {
        txtCostoMejoraMilicia.color = Color.red;
    }
    else
    {
        txtCostoMejoraMilicia.color = new Color(40,40,0);
    }
    }
   }
   
   public void MantenerArmas()
   {
     if( CampaignManager.Instance.sequitoHerrerosMantArmas == 0 && CampaignManager.Instance.GetOroActuales() > 199)
     {
         CampaignManager.Instance.sequitoHerrerosMantArmas = 4;
        CampaignManager.Instance.CambiarOroActual(-200);
     }
      Actualizar();
   }
   public void MantenerArmaduras()
   {
     if( CampaignManager.Instance.sequitoHerrerosMantArmaduras == 0 && CampaignManager.Instance.GetOroActuales() > 199)
     {
         CampaignManager.Instance.sequitoHerrerosMantArmaduras = 4;
        CampaignManager.Instance.CambiarOroActual(-200);
     }
      Actualizar();
   }


   public void MejorarMilicias()
   {
         if(CampaignManager.Instance.GetMaterialesActuales() >= valormejora && CampaignManager.Instance.miliciasMejoras/10 < 30)
         {
           CampaignManager.Instance.CambiarMaterialesActuales(-valormejora);
           CampaignManager.Instance.miliciasMejoras +=10;
           CampaignManager.Instance.GetMiliciasActual();
           Actualizar();
         }
   }
   

}
