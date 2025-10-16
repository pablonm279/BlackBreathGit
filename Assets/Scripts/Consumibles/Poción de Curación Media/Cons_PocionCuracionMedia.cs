using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_PocionCuracionMedia : Consumible
{

  void Awake()
  {
    sNombreItem = TRADU.i.Traducir("Poción de Curación Media");
    itemDescripcion = TRADU.i.Traducir("Restaura 12 + 1d8 puntos de vida.");

    }

    public override void UsarConsumible(Unidad unidad)
    {
       int cura = 12 + TiradaDeDados.TirarDados(1, 8);
       unidad.RecibirCuracion(cura, true);
    }

}
