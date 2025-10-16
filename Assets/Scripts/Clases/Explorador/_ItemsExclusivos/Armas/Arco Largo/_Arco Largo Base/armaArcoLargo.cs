using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaArcoLargo : Arma
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
      if (sNombreItem == "Arco Largo")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Arco Largo " + nmejora;
            itemDescripcion = "Un arco de gran tamaño, ideal para disparos a larga distancia.\n\nHabilidad de ataque: Disparo con Arco.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Longbow " + nmejora;
            itemDescripcion = "A large bow, ideal for long-range shots.\n\nAttack ability: Bow Shot.";
         }
      }


      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Arco Largo Ácido")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Arco Largo Ácido " + nmejora;
            itemDescripcion = "Un arco de gran tamaño, ideal para disparos a larga distancia.\n\nHabilidad de ataque: Disparo con Arco Ácido.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Acid Longbow " + nmejora;
            itemDescripcion = "A large bow, ideal for long-range shots.\n\nAttack ability: Acid Bow Shot.";
         }
      }
      //---
      if (sNombreItem == "Arco Largo Potente")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Arco Largo Potente " + nmejora;
            itemDescripcion = "Un arco de gran tamaño, ideal para disparos a larga distancia.\n\nHabilidad de ataque: Disparo con Arco Potente.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Powerful Longbow " + nmejora;
            itemDescripcion = "A large bow, ideal for long-range shots.\n\nAttack ability: Powerful Bow Shot.";
         }
      }
      //---
      if (sNombreItem == "Arco Largo Ralentizante")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Arco Largo Ralentizante " + nmejora;
            itemDescripcion = "Un arco de gran tamaño, ideal para disparos a larga distancia.\n\nHabilidad de ataque: Disparo con Arco Ralentizante.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Slowing Longbow " + nmejora;
            itemDescripcion = "A large bow, ideal for long-range shots.\n\nAttack ability: Slowing Bow Shot.";
         }
      }

      
      



       //------
      AgregarStatsArmaaDescripcion();

   }
   
}
