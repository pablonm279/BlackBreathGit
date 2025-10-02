using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_PocionCuracionMayor : Consumible
{

    void Awake()
    {
      sNombreItem = "Poción de Curación Mayor";
    }

    public override void UsarConsumible(Unidad unidad)
    {
       int cura = 20 + TiradaDeDados.TirarDados(2, 8);
       unidad.RecibirCuracion(cura, true);
    }

}
