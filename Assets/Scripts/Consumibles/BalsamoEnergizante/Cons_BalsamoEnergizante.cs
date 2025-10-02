using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_BalsamoEnergizante : Consumible
{

    void Awake()
    {
      sNombreItem = "Bálsamo Energizante";
      itemDescripcion = "+2 TS Reflejos por todo el combate.";

    }

  public override void UsarConsumible(Unidad unidad)
  {
    /////////////////////////////////////////////
    //BUFF ---- Así se aplica un buff/debuff
    Buff buff = new Buff();
    buff.buffNombre = "Bálsamo Energizante";
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = -1; // -1 significa que dura todo el combate
    buff.cantTsReflejos += 2;
    buff.AplicarBuff(unidad);
    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
    Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);
       
     BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad); 
   }

}
