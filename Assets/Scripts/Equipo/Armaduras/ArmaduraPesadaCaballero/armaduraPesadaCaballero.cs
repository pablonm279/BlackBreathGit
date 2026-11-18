using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaduraPesadaCaballero : Armadura
{


   void Awake()
   {
      string nmejora = "";
      if (nivelMejora > 0)
      {
         nmejora = "+" + nivelMejora;
      }
      else
      {
         nmejora = "";
      }

      //Aca se pone la variante basica de cada item, luego se traduce y se le agrega el +1,+2,+3,+4,+5 si es que tiene mejora
      if (sNombreItem == "Coraza")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Coraza " + nmejora;
            itemDescripcion = "Una armadura pesada muy resistente pero limita el movimiento.\n\nAgrega: Armadura Limitante.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Heavy Armor " + nmejora;
            itemDescripcion = "A heavy armor that is very resistant but limits movement.\n\nAdds: Limiting Armor.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "重甲 " + nmejora;
            itemDescripcion = "一件非常坚固的重甲，但会限制移动。\n\n附加：笨重护甲。";
         }
      }
      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Coraza de Llamas")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Coraza de Llamas";
            itemDescripcion = "Una armadura pesada muy resistente pero limita el movimiento.\n\nAgrega: Armadura Limitante.\nPuede hacer arder a atacantes.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Flame Armor";
            itemDescripcion = "A heavy armor that is very resistant but limits movement.\n\nAdds: Limiting Armor.\nCan set attackers on fire.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "烈焰胸甲";
            itemDescripcion = "一件非常坚固的重甲，但会限制移动。\n\n附加：笨重护甲。\n可使攻击者燃烧。";
         }
      }
      //------
      if (sNombreItem == "Coraza Liviana")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Coraza Liviana";
            itemDescripcion = "Una armadura ligera que permite mayor movilidad al Caballero.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Light Armor";
            itemDescripcion = "A lightweight armor that allows for greater mobility.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "轻甲";
            itemDescripcion = "一件轻便的护甲，能让骑士获得更强的机动性。";
         }
      }
      //------
      if (sNombreItem == "Coraza de Fuerza de Gigante")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Coraza de Fuerza de Gigante";
            itemDescripcion = "Una armadura pesada que otorga una gran fuerza al Caballero.\n\nAgrega: Armadura Limitante.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Giant Strength Armor";
            itemDescripcion = "A heavy armor that grants great strength to the Knight.\n\nAdds: Limiting Armor.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "巨力胸甲";
            itemDescripcion = "一件能赋予骑士强大力量的重甲。\n\n附加：笨重护甲。";
         }
      }

     
      //------
       AgregarStatsArmaduraaDescripcion();
   }
  
}
