using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBotonesConsumibles : MonoBehaviour
{
  public GameObject BotonConsumibleA;

  public GameObject BotonConsumibleB;
  public void UsarConsumible(int num)
  {
      
     if(num == 1 &&  BattleManager.Instance.unidadActiva.ObtenerAPActual() > 1)
     {

       Unidad unidad = BattleManager.Instance.unidadActiva;
       
       unidad.ConsumibleA.UsarConsumible(unidad);

       unidad.ConsumibleA = null; //Saca consumible
       
       BotonConsumibleA.SetActive(false);

      BattleManager.Instance.unidadActiva.CambiarAPActual(-1);

     }

     if(num == 2  &&  BattleManager.Instance.unidadActiva.ObtenerAPActual() > 1)
     {

       Unidad unidad = BattleManager.Instance.unidadActiva;
       
       unidad.ConsumibleB.UsarConsumible(unidad);

       unidad.ConsumibleB = null; //Saca consumible

       BotonConsumibleB.SetActive(false);

      BattleManager.Instance.unidadActiva.CambiarAPActual(-1);

     }


  }

    
}
