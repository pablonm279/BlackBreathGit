using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_MantenerArmadura : Actividad
{
    void Awake()
    {
      IDActividad = 5; //

      desc = TRADU.i.Traducir("<color=#0cca74><b>Mantenimiento de Armadura: </b></color><color=#d3d3d3><i>El personaje se ocupará de hacer mantenimiento a su armadura.</color></i>\\n\\nSi se produce un combate comenzará con +3 Armadura.");

    }
}
