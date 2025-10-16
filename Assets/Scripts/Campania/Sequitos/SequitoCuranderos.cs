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

  
  
   void Actualizar()
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

    txtDescMejoraCuracion.text = TRADU.i.Traducir("Carros de Tratamiento: Mejorar los carros utilizados por el Séquito de Curanderos para tratar heridos significará una mejora en los tratamientos recibidos por los heridos y su tiempo de recuperación. \nCada Tier aumenta en 5% la curación diaria de los personajes que Descansen y reduce el costo de Tratar Heridas. \nAdemás cada tier da un 10% extra a las posibilidades de reducir Enfermedades al Descansar (20% base). \nCuración proporcionada: ") + (CampaignManager.Instance.sequitoCuranderosMejoraCuracion * 100) + "%" + bonusText;
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
       btnPers.GetComponent<Image>().sprite = pers.spRetrato;
       btnPers.transform.GetChild(3).gameObject.SetActive(true);
       btnPers.GetComponent<btnPersonaje>().personajeRepresentado = pers;
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
