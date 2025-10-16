using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_SimboloArcanoProteccion : Consumible
{

    void Awake()
    {
      sNombreItem = TRADU.i.Traducir("Símbolo de Protección Arcano");
      itemDescripcion = TRADU.i.Traducir("Otorga 3 de Resistencia contra todos los elementos. Dura 4 turnos.");

    }

  public override void UsarConsumible(Unidad unidad)
  {
    /////////////////////////////////////////////
    //BUFF ---- Así se aplica un buff/debuff
    Buff buff = new Buff();
    buff.buffNombre = "Protección Arcana";
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = 3;
    buff.cantResAci += 3;
    buff.cantResFue += 3;
    buff.cantResHie += 3;
    buff.cantResRay += 3;
    buff.percResArc += 3;
    buff.AplicarBuff(unidad);
    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
    Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);

    // Actualiza la información del personaje en la interfaz de usuario
    BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(unidad);
    }

}
