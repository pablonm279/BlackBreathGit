using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_ConcentracionArcana: Actividad
{
    void Awake()
    {
      IDActividad = 16; //

      desc = TRADU.i.Traducir("<color=#0cca74><b>Concentración Arcana: </b></color><color=#d3d3d3><i>El Canalizador se concentra y mantiene su poder preparado para cualquier combate que surja.</color></i>\\n\\n+1 Nivel de Energía al iniciar combates.");

    }
}
