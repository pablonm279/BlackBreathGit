using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_BalsamoClaridad : Consumible
{

    void Awake()
    {
      sNombreItem = TRADU.i.Traducir("Bálsamo de Claridad");
      itemDescripcion = TRADU.i.Traducir("+2 TS Mental por todo el combate.");

    }

  public override void UsarConsumible(Unidad unidad)
  {
    /////////////////////////////////////////////
    //BUFF ---- Así se aplica un buff/debuff
    Buff buff = new Buff();
    buff.buffNombre = "Bálsamo de Claridad";
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = -1; // -1 significa que dura todo el combate
    buff.cantTsMental += 2;
    buff.AplicarBuff(unidad);
    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
    Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);

    // Actualiza la información del personaje en la interfaz de usuario
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
  }

}
