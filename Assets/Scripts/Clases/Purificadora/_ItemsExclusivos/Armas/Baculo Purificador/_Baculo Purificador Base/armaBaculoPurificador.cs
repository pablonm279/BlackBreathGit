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

      //Aca se pone la variante basica de cada item, luego se traduce y se le agrega el +1,+2,+3,+4,+5 si es que tiene mejora
      if (sNombreItem == "Baculo Purificador")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Báculo Purificador " + nmejora;
            itemDescripcion = "Un báculo que irradia energía purificadora.\n\nHabilidad de ataque: Golpe de Bastón.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Purifying Staff " + nmejora;
            itemDescripcion = "A staff that radiates purifying energy.\n\nAttack ability: Staff Strike.";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Bastao Purificador " + nmejora;
            itemDescripcion = "Um bastao que irradia energia purificadora.\n\nHabilidade de ataque: Golpe de Bastao.";
         }
      }
      else if (sNombreItem == "Baculo de Llama Sacra")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Báculo de Llama Sacra " + nmejora;
            itemDescripcion = "Un báculo encendido con fuego sagrado para castigar a los impuros.\n\nHabilidad de ataque: Llama Sacra.\nAplica Ardiendo y causa daño extra a Etéreos, Nomuertos y Corruptos.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Staff of Sacred Flame " + nmejora;
            itemDescripcion = "A staff kindled with sacred fire to punish the impure.\n\nAttack ability: Sacred Flame.\nApplies Burning and deals extra damage to Ethereal, Undead, and Corrupted targets.";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Cajado da Chama Sagrada " + nmejora;
            itemDescripcion = "Um cajado aceso com fogo sagrado para punir os impuros.\n\nHabilidade de ataque: Chama Sagrada.\nAplica Ardendo e causa dano extra a Etereo, Nomuerto e Corrupto.";
         }
      }
      else if (sNombreItem == "Baculo del Ultimo Rito")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Báculo del Último Rito " + nmejora;
            itemDescripcion = "Resquebraja el alma al impactar.\n\nHabilidad de ataque: Golpe de Bastón.\nEfecto: Alma Quebrada (-TS Mental, -Res Divino, -Res Necro).";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Staff of the Final Rite " + nmejora;
            itemDescripcion = "Shatters the soul on impact.\n\nAttack ability: Staff Strike.\nEffect: Shattered Soul (-Mental Save, -Divine Res, -Necro Res).";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Cajado do Ultimo Rito " + nmejora;
            itemDescripcion = "Estilhaça a alma ao impactar.\n\nHabilidade de ataque: Golpe de Bastão.\nEfeito: Alma Estilhaçada (-TS Mental, -Res Divino, -Res Necro).";
         }
      }

      //------
      AgregarStatsArmaaDescripcion();
   }
}
