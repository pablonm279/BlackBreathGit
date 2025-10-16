using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_ElixirResistenciaFrio : Consumible
{

  void Awake()
  {
    sNombreItem = TRADU.i.Traducir("Elixir de Resistencia al Frío");
    itemDescripcion = TRADU.i.Traducir("Aumenta la resistencia al frío en 5 por el combate.");
    }

    public override void UsarConsumible(Unidad unidad)
    {
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Elixir de Resistencia al Frío";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = -1;
       buff.cantResHie += 5; // Aumenta la resistencia al frío en 5
       buff.AplicarBuff(unidad);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);
    }

}
