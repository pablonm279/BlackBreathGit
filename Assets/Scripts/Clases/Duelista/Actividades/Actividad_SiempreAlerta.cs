using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_SiempreAlerta : Actividad
{
    void Awake()
    {
      IDActividad = 19;

      desc = "<color=#0cca74><b>Siempre Alerta: </b></color><color=#d3d3d3><i>La Duelista se mantiene lista para actuar con rapidez si se presenta una batalla.</color></i>\\n\\n+5 Iniciativa en combate. Si no es emboscada, gana 2 Impulso al comenzar la batalla.";
    }
}
