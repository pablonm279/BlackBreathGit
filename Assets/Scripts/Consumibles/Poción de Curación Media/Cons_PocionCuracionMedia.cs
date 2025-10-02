using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_PocionCuracionMedia : Consumible
{

    void Awake()
    {
      sNombreItem = "Poción de Curación Media";
    }

    public override void UsarConsumible(Unidad unidad)
    {
       int cura = 12 + TiradaDeDados.TirarDados(1, 8);
       unidad.RecibirCuracion(cura, true);
    }

}
