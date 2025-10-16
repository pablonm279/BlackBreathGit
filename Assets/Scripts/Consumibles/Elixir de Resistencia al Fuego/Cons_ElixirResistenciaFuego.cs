using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_ElixirResistenciaFuego : Consumible
{

  void Awake()
  {
    sNombreItem = TRADU.i.Traducir("Elixir de Resistencia al Fuego");
        itemDescripcion = TRADU.i.Traducir("Aumenta la resistencia al fuego en 5 por el combate.");

    }

    public override void UsarConsumible(Unidad unidad)
    {
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Elixir de Resistencia al Fuego";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = -1;
       buff.cantResFue += 5; // Aumenta la resistencia al fuego en 5
       buff.AplicarBuff(unidad);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);
    }

}
