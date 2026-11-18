using UnityEngine;

public class armaduraGambeson : Armadura
{
   void Awake()
   {
      string nmejora = nivelMejora > 0 ? "+" + nivelMejora : "";

      if (sNombreItem == "Gambeson")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Gambeson " + nmejora;
            itemDescripcion = "Un gambeson acolchado y liviano, pensado para mantener la movilidad de la duelista.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Gambeson " + nmejora;
            itemDescripcion = "A light padded gambeson built to preserve the duelist's mobility.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "战袄 " + nmejora;
            itemDescripcion = "一件轻盈的夹棉战袄，用以保持决斗者的机动性。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Gambesao " + nmejora;
            itemDescripcion = "Um gambesao acolchoado e leve, feito para preservar a mobilidade da duelista.";
         }
      }

      if (sNombreItem == "Gambeson de Esgrima Ligera")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Gambeson de Esgrima Ligera " + nmejora;
            itemDescripcion = "Un gambeson flexible para duelistas que prefieren entrar al combate ya perfiladas.\nEfecto: +2 Evasion al inicio del combate.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Light Fencing Gambeson " + nmejora;
            itemDescripcion = "A flexible gambeson for duelists who prefer to start a fight already in motion.\nEffect: +2 Evasion at the start of combat.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "轻剑术战袄 " + nmejora;
            itemDescripcion = "一件灵活的战袄，适合喜欢一开战便已进入战斗节奏的决斗者。\n效果：战斗开始时闪避 +2。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Gambeson de Esgrima Leve " + nmejora;
            itemDescripcion = "Um gambeson flexivel para duelistas que preferem entrar no combate ja perfiladas.\nEfeito: +2 Evasao no inicio do combate.";
         }
      }

      if (sNombreItem == "Gambeson del Temple")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Gambeson del Temple " + nmejora;
            itemDescripcion = "Un gambeson sobrio, reforzado para mantener la mente y el cuerpo firmes bajo presión.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Gambeson of Composure " + nmejora;
            itemDescripcion = "A sober gambeson reinforced to keep body and mind steady under pressure.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "沉着战袄 " + nmejora;
            itemDescripcion = "一件朴素的战袄，经过加固以在重压之下保持身心的稳定。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Gambeson do Temple " + nmejora;
            itemDescripcion = "Um gambeson austero, reforcado para manter corpo e mente firmes sob pressao.";
         }
      }

      if (sNombreItem == "Gambeson del Ultimo Paso")
      {
         if (TRADU.i.nIdioma == 1)
         {
            sNombreItem = "Gambeson del Ultimo Paso " + nmejora;
            itemDescripcion = "Un gambeson cosido para sobrevivir al momento mas peligroso del duelo.\nEfecto: Ultimo Paso.";
         }
         else if (TRADU.i.nIdioma == 2)
         {
            sNombreItem = "Last Step Gambeson " + nmejora;
            itemDescripcion = "A gambeson stitched to survive the most dangerous moment of a duel.\nEffect: Last Step.";
         }
         else if (TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
         {
            sNombreItem = "最后一步战袄 " + nmejora;
            itemDescripcion = "一件缝制而成的战袄，用以在决斗最危险的一刻中幸存下来。\n效果：最后一步。";
         }
         else if (TRADU.i.nIdioma == 3)
         {
            sNombreItem = "Gambeson do Ultimo Passo " + nmejora;
            itemDescripcion = "Um gambeson costurado para sobreviver ao momento mais perigoso do duelo.\nEfeito: Ultimo Passo.";
         }
      }

      AgregarStatsArmaduraaDescripcion();
   }
}
