using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SequitoCuranderos : MonoBehaviour
{
   
   
   //Mejoras Curacion
   [SerializeField] TextMeshProUGUI txtDescMejoraCuracion;
   [SerializeField] TextMeshProUGUI txtTierCuracion;
   [SerializeField] TextMeshProUGUI txtCostoMejoraCuracion;
   [SerializeField] GameObject btnMejorarCuracion;

   [SerializeField] TextMeshProUGUI txtTratarHeridas;

   public GameObject contenedorUIPersonajes; 

  

  

   void OnEnable()
   {
    Actualizar();
   }

   int valormejora = 0;

  
  
   public void Actualizar()
   {
    if (txtDescMejoraCuracion != null)
    {


      //Mejora Curacion
      float bonusHerboristas = 0f;
      
      if (CampaignManager.Instance.scMenuSequito.TieneSequito(5))//Herboristas
      {
         int vecesEnClaro = 0;
         vecesEnClaro = CampaignManager.Instance.scSequitoHerboristas.vecesEnClaro;
     
         bonusHerboristas = 3 + 3 * vecesEnClaro;
      }
      else { bonusHerboristas = 0; }

    string bonusText = bonusHerboristas > 0 ? " +" + bonusHerboristas + TRADU.i.Traducir("% por Herboristas") : "";
    string descripcionCuracion = TRADU.i.nIdioma switch
    {
      TRADU.IdiomaIngles => "Treatment Wagons: Their initial bonus is +10%; each tier adds another +5%, up to +30%, and reduces the cost of Treat Wounds. This bonus is added to the global healing multiplier and affects both the 4%/h resting rate and the 2%/h active rate.\nCurrent global bonus: ",
      TRADU.IdiomaPortugues => "Carros de Tratamento: O bônus inicial é +10%; cada tier adiciona outros +5%, até +30%, e reduz o custo de Tratar Feridas. Este bônus é somado ao multiplicador global de cura e afeta tanto a taxa de 4%/h em descanso quanto a taxa de 2%/h em atividade.\nBônus global atual: ",
      _ => "Carros de Tratamiento: Su bonificación inicial es +10%; cada tier suma otro +5%, hasta +30%, y reduce el costo de Tratar Heridas. Esta bonificación se suma al multiplicador global de curación y afecta tanto la tasa de 4%/h al descansar como la de 2%/h en actividad.\nBonificación global actual: "
    };

    txtDescMejoraCuracion.text = descripcionCuracion + (CampaignManager.Instance.sequitoCuranderosMejoraCuracion * 100) + "%" + bonusText;
    float tier =((CampaignManager.Instance.sequitoCuranderosMejoraCuracion*100)-10)/5;
    txtTierCuracion.text = "Tier "+(int)tier;
    valormejora =(int)( 30+(CampaignManager.Instance.sequitoCuranderosMejoraCuracion*150));
    txtCostoMejoraCuracion.text = valormejora+TRADU.i.Traducir(" Materiales");

     if(tier > 2)
    {
        btnMejorarCuracion.SetActive(false);
    }
    else{btnMejorarCuracion.SetActive(true);}
    }

    if(CampaignManager.Instance.GetMaterialesActuales() < valormejora)
    {
        txtCostoMejoraCuracion.color = Color.red;
    }
    else
    {
        txtCostoMejoraCuracion.color = new Color(40,40,0);
    }
 
    float costoCurar = 500 - CampaignManager.Instance.sequitoCuranderosMejoraCuracion*1000;
     if(CampaignManager.Instance.GetOroActuales() >= costoCurar)
     {
       txtTratarHeridas.text = TRADU.i.Traducir("Tratar Heridas - Coste: <color=#A5B328>") + (int)costoCurar + "</color>";
     }
     else
     {
       txtTratarHeridas.text = TRADU.i.Traducir("Tratar Heridas - Coste: <color=#C40E0E>") + (int)costoCurar + "</color>";
     }



    //Actualiza lista personajes heridos
    foreach (Transform transform in contenedorUIPersonajes.transform)//Esto remueve los botones anteriores antes de recalcular que botones corresponden
    {
            Destroy(transform.gameObject);
    }
   
    foreach(Personaje pers in  CampaignManager.Instance.scMenuPersonajes.listaPersonajes)
    {
      if(!pers.Camp_Muerto && pers.Camp_Herido)
      {
       GameObject btnPers = Instantiate( CampaignManager.Instance.scMenuPersonajes.prefabBtnPersonaje, contenedorUIPersonajes.transform);
       btnPersonaje btn = btnPers.GetComponent<btnPersonaje>();
       if (btn != null)
       {
         btn.ConfigurarParaCuranderos(pers, this);
       }
      }

    }

   }
   
  
   public void MejorarCuracion()
   {
         if(CampaignManager.Instance.GetMaterialesActuales() >= valormejora && CampaignManager.Instance.sequitoCuranderosMejoraCuracion < 0.30f)
         {
           CampaignManager.Instance.CambiarMaterialesActuales(-valormejora);
           CampaignManager.Instance.sequitoCuranderosMejoraCuracion += 0.05f;
           Actualizar();
         }
   }
   

   public void TratarHerida(Personaje pers)
   {
    float costoCurar = 500 - CampaignManager.Instance.sequitoCuranderosMejoraCuracion*1000;
    if(CampaignManager.Instance.GetOroActuales() >= costoCurar)
    {
        pers.Camp_Herido = false;
        CampaignManager.Instance.CambiarOroActual(-(int)costoCurar);
        CampaignManager.Instance.EscribirLog("-"+pers.sNombre+TRADU.i.Traducir(" ha recibido tratamiento especial y sus heridas han sanado."));
        Actualizar();
    }
   }

}



