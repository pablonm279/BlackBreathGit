using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_RelatosDeBatalla : Actividad
{
    void Awake()
    {
      IDActividad = 4; //

      desc = TRADU.i.Traducir("<color=#0cca74><b>Relatos de Batalla: </b></color><color=#d3d3d3><i>El personaje compartirá los relatos de sus hazañas con quienes quieran oírlas.</color></i>\\n\\nCada 24 h activas: +10 Experiencia a personajes de nivel inferior y +4 Esperanza.");
                            
    }
}


