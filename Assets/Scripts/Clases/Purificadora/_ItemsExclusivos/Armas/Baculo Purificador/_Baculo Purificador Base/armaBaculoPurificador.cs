using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaBaculoPurificador : Arma
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
      if (sNombreItem == "Baculo Purificador")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Báculo Purificador " + nmejora;
            itemDescripcion = "Un báculo que irradia energía purificadora.\n\nHabilidad de ataque: Golpe bastón.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Purifying Staff " + nmejora;
            itemDescripcion = "A staff that radiates purifying energy.\n\nAttack ability: Staff Strike.";
         }
      }
      //Si hay variantes especiales, se ponen aca abajo





       //------
      AgregarStatsArmaaDescripcion();
   }
   
}
