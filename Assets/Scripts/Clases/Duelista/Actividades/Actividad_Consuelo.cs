using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Consuelo : Actividad
{
    void Awake()
    {
      IDActividad = 21;

      desc = TRADU.i.Traducir("<color=#0cca74><b>Consuelo: </b></color><color=#d3d3d3><i>La Duelista contiene el desánimo de la caravana cuando llegan malas noticias o tiempos difíciles.</color></i>\\n\\nSiempre que se pierda Esperanza por cualquier motivo, se pierde 2 menos.");
    }
}
