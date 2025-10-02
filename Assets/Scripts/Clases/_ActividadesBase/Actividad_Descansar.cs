using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Descansar : Actividad
{
    void Awake()
    {
      IDActividad = 1; //Descansar
 
      desc = "<color=#0cca74><b>Descanso: </b></color><color=#d3d3d3><i>El personaje se centrará en descansar y recuperar su salud.</color></i>\\n\\nCada día que pase recuperará un 15% de salud.\\nSi se produce un combate, lo arrancará Fresco.";

    }
}
