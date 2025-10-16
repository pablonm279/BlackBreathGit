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
      }

     
      //------
       AgregarStatsArmaduraaDescripcion();
   }
  
}
