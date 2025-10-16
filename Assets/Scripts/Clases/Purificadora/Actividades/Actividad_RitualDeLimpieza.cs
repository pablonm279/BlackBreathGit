using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_RitualDeLimpieza: Actividad
{
    void Awake()
    {
      IDActividad = 10; //

      desc = TRADU.i.Traducir("<color=#0cca74><b>Ritual de Limpieza: </b></color><color=#d3d3d3><i>La Purificadora realizará rituales de protección para combatir el Aliento Negro.</color></i>\\n\\nProbabilidad de evitar avance del Aliento Negro: 25% al descansar, 15% por día.");

    }
}
