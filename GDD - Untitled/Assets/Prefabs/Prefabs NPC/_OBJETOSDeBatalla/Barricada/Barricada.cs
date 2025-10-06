using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public  class Barricada : Obstaculo
{

  void Start()
  {
    oName = "Barricada";
    hpMax = 24;
    hpCurr = 24;
    iDureza = 2;
    intDuracionTurnos = 100;

    bPermiteAtacarDetras = true; // permite atacar a traves de ella

  } 
    public override void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante)
 {
   base.RecibirDanio(danio,tipoDanio,esCritico,uCausante);

    if (uCausante != null)
    {
      int dam =UnityEngine.Random.Range(1, 4);

      uCausante.RecibirDanio(dam, 1, false, null);
    }
   


 }

 
 


 
 

 
}
