using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaMandoble : Arma
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
      if (sNombreItem == "Mandoble")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Mandoble " + nmejora;
            itemDescripcion = "Una espada enorme que requiere gran fuerza para ser usada.\n\nHabilidad de ataque: Golpe Vertical.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Greatsword " + nmejora;
            itemDescripcion = "A huge sword that requires great strength to be used.\n\nAttack ability: Vertical Cut.";
         }
      }


      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Mandoble Congelado")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Mandoble Congelado " + nmejora;
            itemDescripcion = "Una espada enorme fabricada con acero-helado, capaz de dañar a los enemigos con su frío extremo.\n\nHabilidad de ataque: Golpe Vertical Congelado.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Frozen Greatsword " + nmejora;
            itemDescripcion = "A huge sword made of ice-steel, capable of harming enemies with its extreme cold.\n\nAttack ability: Frozen Vertical Slash.";
         }
      }
      //---
      if (sNombreItem == "Mandoble Sagrado")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Mandoble Sagrado " + nmejora;
            itemDescripcion = "Una espada enorme bendecida por los Purificadores y la luz sagrada.\n\nHabilidad de ataque: Golpe Vertical Sagrado.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Sacred Greatsword " + nmejora;
            itemDescripcion = "A huge sword blessed by the Purifiers and the sacred light.\n\nAttack ability: Sacred Vertical Slash.";
         }
      }
      //---
      if (sNombreItem == "Mandoble Sediento")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Mandoble Sediento " + nmejora;
            itemDescripcion = "Una espada enorme que es más mortífera si el enemigo está herido.\n\nHabilidad de ataque: Golpe Vertical Sediento.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Thirsty Greatsword " + nmejora;
            itemDescripcion = "A huge sword that is deadlier if the enemy is wounded.\n\nAttack ability: Thirsty Vertical Slash.";
         }
      }
      
      






       //------
      AgregarStatsArmaaDescripcion();

   }
   
}
