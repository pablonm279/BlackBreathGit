using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBotonesConsumibles : MonoBehaviour
{
  public GameObject BotonConsumibleA;

  public GameObject BotonConsumibleB;

  private void Awake()
  {
    ConfigurarTooltipConsumible(BotonConsumibleA, 1);
    ConfigurarTooltipConsumible(BotonConsumibleB, 2);
  }

  private void ConfigurarTooltipConsumible(GameObject boton, int slot)
  {
    if (boton == null)
    {
      return;
    }

    TooltipConsumibleBatalla tooltip = boton.GetComponent<TooltipConsumibleBatalla>();
    if (tooltip == null)
    {
      tooltip = boton.AddComponent<TooltipConsumibleBatalla>();
    }

    tooltip.ConfigurarSlot(slot);
  }

  public void UsarConsumible(int num)
  {
      
     if(num == 1 &&  BattleManager.Instance.unidadActiva.ObtenerAPActual() > 1)
     {

       Unidad unidad = BattleManager.Instance.unidadActiva;
       if (unidad == null || unidad.ConsumibleA == null)
       {
         return;
       }

       Consumible consumible = unidad.ConsumibleA;
       if (!consumible.TieneUsoConfigurado())
       {
         Debug.LogWarning("Consumible sin efecto configurado: " + consumible.sNombreItem);
         return;
       }

       if (!consumible.UsarConsumibleConRegistro(unidad))
       {
         Debug.LogWarning("Consumible no aplico efecto: " + consumible.sNombreItem);
         return;
       }

       unidad.ConsumibleA = null; //Saca consumible
       
       TooltipBatalla.Instance?.HideTooltipSinAnim();
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

       Consumible consumible = unidad.ConsumibleB;
       if (!consumible.TieneUsoConfigurado())
       {
         Debug.LogWarning("Consumible sin efecto configurado: " + consumible.sNombreItem);
         return;
       }

       if (!consumible.UsarConsumibleConRegistro(unidad))
       {
         Debug.LogWarning("Consumible no aplico efecto: " + consumible.sNombreItem);
         return;
       }

       unidad.ConsumibleB = null; //Saca consumible

       TooltipBatalla.Instance?.HideTooltipSinAnim();
       BotonConsumibleB.SetActive(false);

      BattleManager.Instance.unidadActiva.CambiarAPActual(-1);

     }


  }

    
}
