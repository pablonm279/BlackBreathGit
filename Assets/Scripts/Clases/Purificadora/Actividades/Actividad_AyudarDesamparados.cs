using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_AyudarDesamparados: Actividad
{
    void Awake()
    {
      IDActividad = 11; //

      desc = TRADU.i.Traducir("<color=#0cca74><b>Ayudar a los Desamparados: </b></color><color=#d3d3d3><i>La Purificadora usará su tiempo para ayudar a los rezagados y más débiles de la caravana.</color></i>\\n\\n+1d3 Esperanza diaria. +1 Fervor en combate.");

    }
}
