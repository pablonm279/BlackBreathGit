using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaEspadaCorta : Arma
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
      if (sNombreItem == "Espada Corta")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Espada Corta " + nmejora;
            itemDescripcion = "Una espada de hoja corta, versátil.\n\nHabilidad de ataque: Corte de Espada Corta.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Shortsword " + nmejora;
            itemDescripcion = "A short-bladed sword, very versatile.\n\nAttack ability: Shortsword Slash.";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Espada Curta " + nmejora;
            itemDescripcion = "Uma espada de lamina curta, muito versatil.\n\nHabilidade de ataque: Corte de Espada Curta.";
         }
      }


      //Si hay variantes especiales, se ponen aca abajo
      if (sNombreItem == "Espada Corta Arcana")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Espada Corta Arcana " + nmejora;
            itemDescripcion = "Una espada de hoja corta, versátil.\n\nHabilidad de ataque: Corte de Espada Corta Arcana.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Arcane Shortsword " + nmejora;
            itemDescripcion = "A short-bladed sword, very versatile.\n\nAttack ability: Arcane Shortsword Slash.";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Espada Curta Arcana " + nmejora;
            itemDescripcion = "Uma espada de lamina curta, muito versatil.\n\nHabilidade de ataque: Corte de Espada Curta Arcana.";
         }
      }
      //---
      if (sNombreItem == "Espada Corta Filonegro")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Espada Corta Filonegro " + nmejora;
            itemDescripcion = "Una espada de hoja corta, versátil.\n\nHabilidad de ataque: Corte de Espada Corta Filonegro.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Blacksteel Shortsword " + nmejora;
            itemDescripcion = "A short-bladed sword, very versatile.\n\nAttack ability: Blacksteel Shortsword Slash.";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Espada Curta Fio Negro " + nmejora;
            itemDescripcion = "Uma espada de lamina curta, muito versatil.\n\nHabilidade de ataque: Corte de Espada Curta Fio Negro.";
         }
      }
      //---
      if (sNombreItem == "Espada Corta Consumevida")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Espada Corta Consumevida " + nmejora;
            itemDescripcion = "Una espada de hoja corta, versátil.\n\nHabilidad de ataque: Corte de Espada Corta Consumevida.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Life-consuming Shortsword " + nmejora;
            itemDescripcion = "A short-bladed sword, very versatile.\n\nAttack ability: Life-consuming Shortsword Slash.";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Espada Curta Consomevida " + nmejora;
            itemDescripcion = "Uma espada de lamina curta, muito versatil.\n\nHabilidade de ataque: Corte de Espada Curta Consomevida.";
         }
      }



      //------
      AgregarStatsArmaaDescripcion();

   }
   
}


