using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_PrepararFlechas: Actividad
{
    void Awake()
    {
      IDActividad = 8; //
 
      desc = "<color=#0cca74><b>Preparar Flechas: </b></color><color=#d3d3d3><i>El personaje invertirá su tiempo en crear y mejorar sus flechas.</color></i>\\n\\nSi se produce un combate tendrá +3 Flechas y +10% daño.";

    }
}
