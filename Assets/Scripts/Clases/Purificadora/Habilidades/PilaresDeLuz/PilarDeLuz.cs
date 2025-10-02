using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public  class PilarDeLuz : Obstaculo
{

   
public int NIVEL = 1;
public ClasePurificadora scCreador;
 public override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante)
 {
   base.RecibirDanio(danio,tipoDanio,esCritico,uCausante);

    if (uCausante != null)
    {
      int dam =UnityEngine.Random.Range(1, 9) + (int)scCreador.mod_CarPoder;
      if(NIVEL > 2){ dam+=3;}
      if(uCausante.TieneTag("Nomuerto") || uCausante.TieneTag("Etereo"))
      {
          dam = dam*2;
      }

      uCausante.RecibirDanio(dam, 11, false, null);
    }
   


 }

 
 


 
 

 
}
