using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Vigilar : Actividad
{
    void Awake()
    {
      IDActividad = 6; //
 
      desc = "<color=#0cca74><b>Vigilar: </b></color><color=#d3d3d3><i>El personaje permanecerá vigilante ante cualquier peligro.</color></i>\\n\\nSi se produce una emboscada podrá participar activamente de la defensa y obtiene +2 AP, +5 Iniciativa y +20% daño los primeros 2 turnos.";

    }
}
