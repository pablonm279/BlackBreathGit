using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaBastonPurificadora : Arma
{
   

   void Awake()
   {
    if (TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
    {
        itemDescripcion = "<i>净化者的法杖用于引导其神圣力量，也可用于近战防御。</i><b>\n\n赋予：法杖打击</b>";
    }
    else
    {
        itemDescripcion = "<i>El bastón de una purificadora es utilizado para canalizar su poder divino y defensa cuerpo a cuerpo.</i><b>\n\nOtorga: Golpe de Bastón</b>";
    }


   }
   
}


