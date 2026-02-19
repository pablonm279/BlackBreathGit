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
       if (unidad == null || unidad.ConsumibleA == null)
       {
         return;
       }
       
       unidad.ConsumibleA.UsarConsumibleDesdeDatos(unidad);

       unidad.ConsumibleA = null; //Saca consumible
       
       BotonConsumibleA.SetActive(false);

      BattleManager.Instance.unidadActiva.CambiarAPActual(-1);

     }

     if(num == 2  &&  BattleManager.Instance.unidadActiva.ObtenerAPActual() > 1)
     {

       Unidad unidad = BattleManager.Instance.unidadActiva;
       if (unidad == null || unidad.ConsumibleB == null)
       {
         return;
       }
       
       unidad.ConsumibleB.UsarConsumibleDesdeDatos(unidad);

       unidad.ConsumibleB = null; //Saca consumible

       BotonConsumibleB.SetActive(false);

      BattleManager.Instance.unidadActiva.CambiarAPActual(-1);

     }


  }

    
}
