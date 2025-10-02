using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_PocionCuracionMenor : Consumible
{

    void Awake()
    {
      sNombreItem = "Poción de Curación Menor";
    }

    public override void UsarConsumible(Unidad unidad)
    {
       int cura = 5 + TiradaDeDados.TirarDados(1, 6);
       unidad.RecibirCuracion(cura, true);
    }

}
