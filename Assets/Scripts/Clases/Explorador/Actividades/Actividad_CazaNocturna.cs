using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_CazaNocturna: Actividad
{
    void Awake()
    {
      IDActividad = 7; //

      desc = "<color=#0cca74><b>Caza: </b></color><color=#d3d3d3><i>El personaje cazará en las inmediaciones para conseguir comida para la caravana.</color></i>\\n\\n+10-15 Suministros cada 24 h activas. +3% probabilidad de Emboscada Enemiga.";

    }
}


