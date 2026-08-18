using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONArmaduraLimitante : Habilidad
{
   


   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_ArmaduraLimitante");
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
        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          "Limiting Armor",
          "Weakness: Heavy armor makes attacks less precise.",
          new[]
          {
            LineaDescripcion("Effect", "+5% Fumble.")
          },
          colorTitulo: "#cb5000");
        return;
      }

      if (TRADU.i != null && TRADU.i.nIdioma == 3)
      {
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Armadura Limitante", "Fraqueza: a armadura pesada reduz a precisão dos ataques.", new[]
        {
          LineaDescripcion("Efeito", "+5% de Falha Crítica.")
        }, colorTitulo: "#cb5000", costoSuperior: string.Empty);
        return;
      }

      txtDescripcion = ConstruirDescripcionNormalizadaLocalizada("Armadura Limitante", "Debilidad: la armadura pesada reduce la precisión de los ataques.", new[]
      {
        LineaDescripcion("Efecto", "+5% de Pifia.")
      }, colorTitulo: "#cb5000", costoSuperior: string.Empty);
      return;

      string iconoDebuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_debuff\"></voffset></size><space=-0.35em>";
      string titulo = "Armadura Limitante";
      string subtitulo = "<color=#4f5552>Debilidad: +5% Pifia.</color>";
      string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Debilidad " + iconoDebuff + "</color>\n" +
                      "<color=#44d3ec><b>Efecto:</b></color> <color=#ffffff>+5% Pifia.</color>";

      if (TRADU.i.nIdioma == 2)
      {
        titulo = "Limiting Armor";
        subtitulo = "<color=#4f5552>Weakness: +5% Fumble.</color>";
        cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Weakness " + iconoDebuff + "</color>\n" +
                 "<color=#44d3ec><b>Effect:</b></color> <color=#ffffff>+5% Fumble.</color>";
      }
      else if (TRADU.i.nIdioma == 3)
      {
        titulo = "Armadura Limitante";
        subtitulo = "<color=#4f5552>Fraqueza: +5% Falha critica.</color>";
        cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Fraqueza " + iconoDebuff + "</color>\n" +
                 "<color=#44d3ec><b>Efeito:</b></color> <color=#ffffff>+5% Falha critica.</color>";
      }

      txtDescripcion = ConstruirDescripcionEstandar($"<size=115%>{titulo}</size>", subtitulo, cuerpo, "", "#cb5000");
    }




}


