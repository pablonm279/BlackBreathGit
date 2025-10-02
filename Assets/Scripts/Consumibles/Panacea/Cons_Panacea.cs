using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Cons_Panacea : Consumible
{

    void Awake()
    {
      sNombreItem = "Panacea";
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
