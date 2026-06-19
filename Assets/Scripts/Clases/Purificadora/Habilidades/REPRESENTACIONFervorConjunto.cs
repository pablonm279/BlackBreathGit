using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONFervorConjunto : Habilidad
{
   

     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_FervorConjunto");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
        string titulo = "Fervor Conjunto";
        string subtitulo = "<color=#4f5552>Pasiva: la Esperanza genera Fervor para la Purificadora.</color>";
        string cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Pasiva</color>\n" +
                        "<color=#44d3ec><b>Fuente:</b></color> <color=#ffffff>Esperanza de la caravana.</color>\n" +
                        "<color=#44d3ec><b>Por Fervor:</b></color> <color=#ffffff>+1 daño Divino y 1 Barrera.</color>";

        if (TRADU.i.nIdioma == 2)
        {
            titulo = "Joint Fervor";
            subtitulo = "<color=#4f5552>Passive: Hope generates Fervor for the Purifier.</color>";
            cuerpo = "<color=#44d3ec><b>Type:</b></color> <color=#ffffff>Passive</color>\n" +
                     "<color=#44d3ec><b>Source:</b></color> <color=#ffffff>Caravan Hope.</color>\n" +
                     "<color=#44d3ec><b>Per Fervor:</b></color> <color=#ffffff>+1 Divine damage and 1 Barrier.</color>";
        }
        else if (TRADU.i.nIdioma == 3)
        {
            titulo = "Fervor Conjunto";
            subtitulo = "<color=#4f5552>Passiva: Esperanca gera Fervor para a Purificadora.</color>";
            cuerpo = "<color=#44d3ec><b>Tipo:</b></color> <color=#ffffff>Passiva</color>\n" +
                     "<color=#44d3ec><b>Fonte:</b></color> <color=#ffffff>Esperanca da caravana.</color>\n" +
                     "<color=#44d3ec><b>Por Fervor:</b></color> <color=#ffffff>+1 dano Divino e 1 Barreira.</color>";
        }

        txtDescripcion = ConstruirDescripcionEstandar($"<size=115%>{titulo}</size>", subtitulo, cuerpo, "", "#5dade2");
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}


