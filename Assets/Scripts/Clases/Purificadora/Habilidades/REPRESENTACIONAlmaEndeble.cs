using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAlmaEndeble : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_AlmaEndeble");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
        string titulo = "Alma Endeble";
        string subtitulo = "<color=#4f5552>Debilidad: recibe Aflicciones segun el Aliento Negro.</color>";
        string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Debilidad</color>\n" +
                        "<color=#44d3ec><b>Fuente:</b></color> <color=#ffffff>Aliento Negro.</color>\n" +
                        "<color=#44d3ec><b>Efecto:</b></color> <color=#ffffff>Las Aflicciones de combate escalan con su intensidad.</color>";

        if (TRADU.i.nIdioma == 2)
        {
            titulo = "Fragile Soul";
            subtitulo = "<color=#4f5552>Weakness: suffers Afflictions based on Black Breath.</color>";
            cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Weakness</color>\n" +
                     "<color=#44d3ec><b>Source:</b></color> <color=#ffffff>Black Breath.</color>\n" +
                     "<color=#44d3ec><b>Effect:</b></color> <color=#ffffff>Combat Afflictions scale with its intensity.</color>";
        }
        else if (TRADU.i.nIdioma == 3)
        {
            titulo = "Alma Fragil";
            subtitulo = "<color=#4f5552>Fraqueza: sofre Aflicoes conforme o Respiro Negro.</color>";
            cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Fraqueza</color>\n" +
                     "<color=#44d3ec><b>Fonte:</b></color> <color=#ffffff>Respiro Negro.</color>\n" +
                     "<color=#44d3ec><b>Efeito:</b></color> <color=#ffffff>Aflicoes de combate escalam com sua intensidade.</color>";
        }

        txtDescripcion = ConstruirDescripcionEstandar($"<size=115%>{titulo}</size>", subtitulo, cuerpo, "", "#cb5000");
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}




