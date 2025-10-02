using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipTrampa : MonoBehaviour
{
    public void MostrarTooltip(int n)
    {
        TooltipBatalla.Instance.ShowTooltip(n);

        Invoke("EliminarTooltip", 5f); // Elimina el tooltip después de 5 segundos
    }

     public void EliminarTooltip()
    {
        TooltipBatalla.Instance.HideTooltip();
    }
}
