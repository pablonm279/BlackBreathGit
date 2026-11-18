using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class armaEstoque : Arma
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
      if (sNombreItem == "Estoque")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Estoque " + nmejora;
            itemDescripcion = "Un estoque ligero y preciso, ideal para duelos.\n\nHabilidad de ataque: Estocada.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Estoc " + nmejora;
            itemDescripcion = "A light, precise estoc, ideal for duels.\n\nAttack ability: Thrust.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "刺剑 " + nmejora;
            itemDescripcion = "一把轻巧而精准的刺剑，非常适合决斗。\n\n攻击技能：突刺。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Estoque " + nmejora;
            itemDescripcion = "Um estoque leve e preciso, ideal para duelos.\n\nHabilidade de ataque: Estocada.";
         }
      }

      if (sNombreItem == "Estoque Astral")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Estoque Astral " + nmejora;
            itemDescripcion = "Un estoque grabado con runas astrales, ideal para marcar a los rivales.\n\nHabilidad de ataque: Estocada Astral.\nEfecto: Marca Astral al impactar.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Astral Estoc " + nmejora;
            itemDescripcion = "An estoc engraved with astral sigils, ideal for marking foes.\n\nAttack ability: Astral Thrust.\nEffect: applies Astral Mark on hit.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "星界刺剑 " + nmejora;
            itemDescripcion = "一把刻有星界符文的刺剑，适合标记对手。\n\n攻击技能：星界突刺。\n效果：命中时施加星界印记。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Estoque Astral " + nmejora;
            itemDescripcion = "Um estoque gravado com runas astrais, ideal para marcar os rivais.\n\nHabilidade de ataque: Estocada Astral.\nEfeito: aplica Marca Astral ao acertar.";
         }
      }

      if (sNombreItem == "Estoque del Primer Sangre")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Estoque del Primer Sangre " + nmejora;
            itemDescripcion = "Un estoque pensado para abrir el duelo con una ventaja brutal.\n\nHabilidad de ataque: Estocada del Primer Sangre.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "First Blood Estoc " + nmejora;
            itemDescripcion = "An estoc built to open a duel with brutal advantage.\n\nAttack ability: First Blood Thrust.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "首滴血刺剑 " + nmejora;
            itemDescripcion = "一把专为在决斗开局占据压倒性优势而设计的刺剑。\n\n攻击技能：首滴血突刺。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Estoque do Primeiro Sangue " + nmejora;
            itemDescripcion = "Um estoque pensado para abrir o duelo com vantagem brutal.\n\nHabilidade de ataque: Estocada do Primeiro Sangue.";
         }
      }

      if (sNombreItem == "Estoque de Veloz Replica")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Estoque de Veloz Replica " + nmejora;
            itemDescripcion = "Un estoque agil, pensado para presionar con ritmo y precision.\n\nHabilidad de ataque: Estocada.\nEfecto: aplica Blanco Medido al impactar.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Swift Riposte Estoc " + nmejora;
            itemDescripcion = "A nimble estoc built to pressure with tempo and precision.\n\nAttack ability: Thrust.\nEffect: applies Measured Target on hit.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "疾速还击刺剑 " + nmejora;
            itemDescripcion = "一把灵巧的刺剑，专为以节奏与精准施压而设计。\n\n攻击技能：突刺。\n效果：命中时施加精准目标。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Estoque de Replica Veloz " + nmejora;
            itemDescripcion = "Um estoque agil, pensado para pressionar com ritmo e precisao.\n\nHabilidade de ataque: Estocada.\nEfeito: aplica Alvo Medido ao acertar.";
         }
      }

      if (sNombreItem == "Estoque de la Rosa Negra")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Estoque de la Rosa Negra " + nmejora;
            itemDescripcion = "Un estoque elegante y cruel, hecho para dejar una herida que no cierra.\n\nHabilidad de ataque: Estocada de la Rosa Negra.\nEfecto: aplica Rosa Negra al impactar.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Black Rose Estoc " + nmejora;
            itemDescripcion = "An elegant, cruel estoc made to leave wounds that refuse to close.\n\nAttack ability: Black Rose Thrust.\nEffect: applies Black Rose on hit.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "黑玫瑰刺剑 " + nmejora;
            itemDescripcion = "一把优雅而残酷的刺剑，专为留下无法愈合的伤口而打造。\n\n攻击技能：黑玫瑰突刺。\n效果：命中时施加黑玫瑰。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Estoque da Rosa Negra " + nmejora;
            itemDescripcion = "Um estoque elegante e cruel, feito para deixar uma ferida que nao fecha.\n\nHabilidade de ataque: Estocada da Rosa Negra.\nEfeito: aplica Rosa Negra ao acertar.";
         }
      }



      //------
      AgregarStatsArmaaDescripcion();

   }
   
}


