using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Socializar : Actividad
{
    void Awake()
    {
      IDActividad = 20;

      desc = TRADU.i.Traducir("<color=#0cca74><b>Socializar: </b></color><color=#d3d3d3><i>La Duelista dedica tiempo a conversar, bromear y sostener el ánimo de la caravana.</color></i>\\n\\nCada 24 h activas, sus compañeros realizan una TS Mental DC 16. Quienes la superan obtienen Moral Alta durante 24 h.");
    }
}
