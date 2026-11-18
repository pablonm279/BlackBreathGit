using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaMandobledeCaballero : Arma
{
   

   void Awake()
   {
    if (TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
    {
        itemDescripcion = "<i>骑士总是配备着一把沉重而强大的双手大剑。它与其重甲一起，构成了这些勇敢战士装备的核心。</i><b>\n\n赋予：纵斩</b>";
    }
    else
    {
        itemDescripcion = "<i>Los Caballeros siempre andan equipados con un mandoble muy pesado y poderoso. Junto con su armadura pesada, hacen el núcleo del equipo de estos valientes guerreros.</i><b>\n\nOtorga: Corte Vertical</b>";
    }


   }
   
}

