using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_ColaborarConLosCuranderos: Actividad
{
    void Awake()
    {
      IDActividad = 12; //

      desc =  TRADU.i.Traducir("<color=#0cca74><b>Colaborar con los Curanderos: </b></color><color=#d3d3d3><i>Ayuda al <b>Séquito de Curanderos</b> en sus tareas, aumentando su eficacia.</color></i>\\n\\nAumenta 5% la curación diaria del Séquito de Curanderos.");

    }
}
