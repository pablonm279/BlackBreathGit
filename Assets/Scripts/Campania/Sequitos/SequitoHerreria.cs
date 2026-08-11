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
    txtDescrMantArma.text = TRADU.i.Traducir("Mantenimiento Armas: El Herrero realizará un mantenimiento general de las armas de los personajes. Aumenta su Ataque en 1 y su daño en 2 durante 72 h.");
    txtDescrMantArmaduras.text = TRADU.i.Traducir("Mantenimiento Armaduras: El Herrero realizará un mantenimiento general de las armaduras de los personajes. Aumenta su Defensa en 1 y su Armadura en 2 durante 72 h.");
    //Mantenimiento Armas
    if( CampaignManager.Instance.sequitoHerrerosMantArmasHoras <= 0f)
    {txtCostoMantArma.text = TRADU.i.Traducir("Realizar: 200 Oro"); txtCostoMantArma.color = Color.yellow;}
    else{txtCostoMantArma.text = TRADU.i.Traducir("Activo por ")+ CampaignManager.Instance.FormatearDuracionHoras(CampaignManager.Instance.sequitoHerrerosMantArmasHoras);txtCostoMantArma.color = Color.green;}

    //Mantenimiento Armaduras
    if( CampaignManager.Instance.sequitoHerrerosMantArmadurasHoras <= 0f)
    {txtCostoMantArmaduras.text = TRADU.i.Traducir("Realizar: 200 Oro"); txtCostoMantArmaduras.color = Color.yellow;}
    else{txtCostoMantArmaduras.text = TRADU.i.Traducir("Activo por ")+ CampaignManager.Instance.FormatearDuracionHoras(CampaignManager.Instance.sequitoHerrerosMantArmadurasHoras);txtCostoMantArmaduras.color = Color.green;}




    //Mejora Milicias
    txtDescrMejoraMilicia.text = TRADU.i.Traducir("Armas Civiles: El herrero se dedica a mejorar las armas rudimentarias de los civiles, mejorando las posibilidades de defensa de las Milicias. \nCada Tier aumenta en 10% los Civiles que suman fuerza para la Milicia.");
    txtTierMilicia.text = "Tier "+CampaignManager.Instance.miliciasMejoras/10;
    valormejora = 20+(CampaignManager.Instance.miliciasMejoras*2);
    txtCostoMejoraMilicia.text = valormejora+TRADU.i.Traducir(" Materiales");


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
     if( CampaignManager.Instance.sequitoHerrerosMantArmasHoras <= 0f && CampaignManager.Instance.GetOroActuales() > 199)
     {
         CampaignManager.Instance.sequitoHerrerosMantArmasHoras = 72f;
        CampaignManager.Instance.CambiarOroActual(-200);
     }
      Actualizar();
   }
   public void MantenerArmaduras()
   {
     if( CampaignManager.Instance.sequitoHerrerosMantArmadurasHoras <= 0f && CampaignManager.Instance.GetOroActuales() > 199)
     {
         CampaignManager.Instance.sequitoHerrerosMantArmadurasHoras = 72f;
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


