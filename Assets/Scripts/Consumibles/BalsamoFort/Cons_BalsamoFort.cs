using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_BalsamoFort : Consumible
{

    void Awake()
    {
      sNombreItem = "Bálsamo Fortalecedor";
      itemDescripcion = "+2 TS Fortaleza por todo el combate.";

    }

  public override void UsarConsumible(Unidad unidad)
  {
    /////////////////////////////////////////////
    //BUFF ---- Así se aplica un buff/debuff
    Buff buff = new Buff();
    buff.buffNombre = "Bálsamo Fortalecedor";
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = -1; // -1 significa que dura todo el combate
    buff.percTsFortaleza += 2;
    buff.AplicarBuff(unidad);
    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
    Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
    }

}
