using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Entrenar : Actividad
{
    void Awake()
    {
      IDActividad = 2; //Entrenar

      desc = TRADU.i.Traducir("<color=#0cca74><b>Entrenar: </b></color><color=#d3d3d3><i>El personaje utilizará su tiempo libre para entrenar y mantenerse en forma.</color></i>\\n\\nCada día que pase ganará 15 Experiencia.\\nSi se produce un combate, lo arrancará Fatigado.");


    }
}
