using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_PocionCuracionMenor : Consumible
{

  void Awake()
  {
    sNombreItem = TRADU.i.Traducir("Poción de Curación Menor");
    itemDescripcion = TRADU.i.Traducir("Restaura 6 + 1d6 puntos de vida.");
    }

    public override void UsarConsumible(Unidad unidad)
    {
       int cura = 5 + TiradaDeDados.TirarDados(1, 6);
       unidad.RecibirCuracion(cura, true);
    }

}
