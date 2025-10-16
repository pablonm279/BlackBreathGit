using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_PocionCuracionMayor : Consumible
{

  void Awake()
  {
    sNombreItem = TRADU.i.Traducir("Poción de Curación Mayor");
    itemDescripcion = TRADU.i.Traducir("Restaura 20 + 2d8 puntos de vida.");

    }

    public override void UsarConsumible(Unidad unidad)
    {
       int cura = 20 + TiradaDeDados.TirarDados(2, 8);
       unidad.RecibirCuracion(cura, true);
    }

}
