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
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Armadura de Couro Reforcado " + nmejora;
            itemDescripcion = "Uma armadura leve feita de couro reforcado.";
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
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Armadura de Couro Reforcado de Leveza";
            itemDescripcion = "Uma armadura extremamente leve feita de couro reforcado.\n\n+2 Evasao no inicio do combate.";
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
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Armadura de Couro Reforcado do Veu";
            itemDescripcion = "Uma armadura feita de couro reforcado.\n\nAo receber dano critico, ganha Escondido I.";
         }
      }
      //------





      //CUERO DE EXPLORADOR
      if (sNombreItem == "Armadura de Cuero")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero " + nmejora;
            itemDescripcion = "Una armadura ligera hecha de cuero.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Leather Armor " + nmejora;
            itemDescripcion = "A lightweight armor made of leather.";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Armadura de Couro " + nmejora;
            itemDescripcion = "Uma armadura leve feita de couro.";
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
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Armadura de Couro da Fortaleza";
            itemDescripcion = "Uma armadura feita de couro.\n\n+1 TS Fortaleza.";
         }
      }
      //------
      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Armadura de Cuero Necrítico")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Armadura de Cuero Necrítico";
            itemDescripcion = "Una armadura hecha de cuero necrosado.\n\n";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Necrotic-leather Armor";
            itemDescripcion = "A lightweight armor made of necrotic leather.\n\n";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Armadura de Couro Necrotico";
            itemDescripcion = "Uma armadura leve feita de couro necrotico.\n\n";
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
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Armadura de Couro Turva";
            itemDescripcion = "Uma armadura feita de couro turvo.\n\n+3 Evasao no inicio do combate.";
         }
      }
      //------
      


      //------
       AgregarStatsArmaduraaDescripcion();
   }
   
}



