using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Guardia : Actividad
{
    void Awake()
    {
      IDActividad = 3; //Guardia

      desc = "<color=#0cca74><b>Guardia: </b></color><color=#d3d3d3><i>El personaje se mantendrá alerta y custodiará la caravana.</color></i>\\n\\nSi se produce una emboscada, podrá participar de la defensa sin penalización. +3% Exploración al descansar.";



    }
}
