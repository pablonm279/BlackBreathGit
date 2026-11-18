using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaEspadacortaAcechador : Arma
{
   

   void Awake()
   {
    if (TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
    {
        itemDescripcion = "<i>潜猎者使用短剑，能在几乎不被察觉的情况下谨慎发动攻击。</i><b>\n\n赋予：短剑斩击</b>";
    }
    else
    {
        itemDescripcion = "<i>Los Acechadores utilizan una espada corta que les permite atacar con cautela casi sin ser vistos.</i><b>\n\nOtorga: Ataque Espada Corta</b>";
    }


   }
   
}
