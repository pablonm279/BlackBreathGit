using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Telekinesis: Actividad
{
    void Awake()
    {
      IDActividad = 17; //

      desc = TRADU.i.Traducir("<color=#0cca74><b>Telekinesis: </b></color><color=#d3d3d3><i>Con sus poderes arcanos de telequinesis, ayuda con la carga de la caravana.</color></i>\\n\\n+20 Capacidad de carga.");

    }
}
