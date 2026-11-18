using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaduraCueroReforzadoAcechador : Armadura
{
   

   void Awake()
   {
    if (TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
    {
        itemDescripcion = "<i>潜猎者使用的强化皮甲，能获得一定防护的同时依然保持敏捷。</i><b>\n\n护甲：3。</b>";
    }
    else
    {
        itemDescripcion = "<i>Armadura de Cuero Reforzado que usan los Acechadores para obtener algo de protección y aún así poder moverse con agilidad.</i><b>\n\nArmadura: 3.</b>";
    }


   }
   
}



