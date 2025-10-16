using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_Panacea : Consumible
{

  void Awake()
  {
    sNombreItem = TRADU.i.Traducir("Panacea");
        itemDescripcion = TRADU.i.Traducir("Remueve todos los debuffs de la unidad.");
    }

  public override void UsarConsumible(Unidad unidad)
  {
         foreach (Buff buff in unidad.GetComponents<Buff>())
          {
            if(buff.esRemovible && !buff.boolfDebufftBuff)
            {
              // Remueve el buff de la unidad
              if(buff != null)
              {
                buff.RemoverBuff(unidad);
              }
            }

          }
  }

}
