using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaduraDeCuero : Armadura
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

      //CUERO REFORZADO DE ACECHADOR
      //Aca se pone la variante basica de cada item, luego se traduce y se le agrega el +1,+2,+3,+4,+5 si es que tiene mejora
      if (sNombreItem == "Armadura de Cuero Reforzado")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero Reforzado " + nmejora;
            itemDescripcion = "Una armadura ligera hecha de cuero reforzado.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Reinforced Leather Armor " + nmejora;
            itemDescripcion = "A lightweight armor made of reinforced leather.";
         }
      }
      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Armadura de Cuero Reforzado de Ligereza")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero Reforzado de Ligereza";
            itemDescripcion = "Una armadura extremadamente ligera hecha de cuero reforzado.\n\n+2 Evasión al comenzar el combate.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Lightweight Reinforced Leather Armor";
            itemDescripcion = "A lightweight armor made of reinforced leather.\n\n+2 Evasion at the start of combat.";
         }
      }
      //------
      if (sNombreItem == "Armadura de Cuero Reforzado de Velo")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero Reforzado de Velo";
            itemDescripcion = "Una armadura hecha de cuero reforzado.\n\nAl recibir daño crítico recibe Escondido I.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Reinforced Leather Armor of Veil";
            itemDescripcion = "A lightweight armor made of reinforced leather.\n\nWhen receiving critical damage, gain Hidden I.";
         }
      }
      //------





      //CUERO DE EXPLORADOR
      if (sNombreItem == "Armadura de Cuero")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero" + nmejora;
            itemDescripcion = "Una armadura ligera hecha de cuero.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Leather Armor" + nmejora;
            itemDescripcion = "A lightweight armor made of leather.";
         }
      }
      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Armadura de Cuero de Fortaleza")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero de Fortaleza";
            itemDescripcion = "Una armadura hecha de cuero.\n\n+1 TS Fortaleza.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Leather Armor of Fortitude";
            itemDescripcion = "A lightweight armor made of leather.\n\n+1 Fortitude.";
         }
      }
      //------
      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Armadura de Cuero Necrótico")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero Necrótico";
            itemDescripcion = "Una armadura hecha de cuero necrosado.\n\n";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Necrotic-leather Armor";
            itemDescripcion = "A lightweight armor made of necrotic leather.\n\n";
         }
      }
      //------
      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Armadura de Cuero Borrosa")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero Borrosa";
            itemDescripcion = "Una armadura hecha de cuero borroso.\n\n+3 Evasión al comenzar el combate.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Blurred Leather Armor";
            itemDescripcion = "A lightweight armor made of blurred leather.\n\n+3 Evasion at the start of combat.";
         }
      }
      //------
      


      //------
       AgregarStatsArmaduraaDescripcion();
   }
   
}
