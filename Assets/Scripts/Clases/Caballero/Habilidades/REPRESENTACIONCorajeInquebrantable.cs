using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONCorajeInquebrantable : Habilidad
{
   


     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_CorajeInquebrantable");
      ActualizarDescripcion();

    }



    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    
    public override void ActualizarDescripcion()
    {
      if (TRADU.i != null && TRADU.i.nIdioma == 2)
      {
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valour", "Valentía");
        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          "Unbreakable Valour",
          "Passive: Knights never waver in battle.",
          new[]
          {
            LineaDescripcion("Effect", $"{valentia} cannot drop below 0.")
          });
        return;
      }

      if (TRADU.i != null && TRADU.i.nIdioma == 3)
      {
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentia", "Valentía");
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Coragem Inquebrantável", "Passiva: os Cavaleiros nunca vacilam em batalha.", new[]
        {
          LineaDescripcion("Efeito", $"A {valentia} não pode cair abaixo de 0.")
        }, costoSuperior: string.Empty);
        return;
      }

      {
        string valentia = TerminoDescripcion(TerminoDescripcionId.Valentia, "Valentía", "Valentía");
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Coraje Inquebrantable", "Pasiva: los Caballeros nunca flaquean en batalla.", new[]
        {
          LineaDescripcion("Efecto", $"La {valentia} no puede bajar de 0.")
        }, costoSuperior: string.Empty);
        return;
      }

      string titulo = "Coraje Inquebrantable";
      string subtitulo = "<color=#4f5552>Pasiva: la Valentía no baja de 0.</color>";
      string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Pasiva</color>\n" +
                      "<color=#44d3ec><b>Efecto:</b></color> <color=#ffffff>Los puntos de Valentía nunca quedan por debajo de 0.</color>";

      if (TRADU.i.nIdioma == 2)
      {
        titulo = "Unbreakable Valour";
        subtitulo = "<color=#4f5552>Passive: Knights never waver in battle.</color>";
        cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Passive</color>\n" +
                 "<color=#44d3ec><b>Effect:</b></color> <color=#ffffff>Valour points never stay below 0.</color>";
      }
      else if (TRADU.i.nIdioma == 3)
      {
        titulo = "Coragem Inquebravel";
        subtitulo = "<color=#4f5552>Passiva: Valentía nao baixa de 0.</color>";
        cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Passiva</color>\n" +
                 "<color=#44d3ec><b>Efeito:</b></color> <color=#ffffff>Os pontos de Valentia nunca ficam abaixo de 0.</color>";
      }

      txtDescripcion = ConstruirDescripcionEstandar($"<size=115%>{titulo}</size>", subtitulo, cuerpo, "", "#5dade2");
    }



}



