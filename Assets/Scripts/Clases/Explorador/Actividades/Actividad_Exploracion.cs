using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actividad_Exploracion: Actividad
{
    void Awake()
    {
      IDActividad = 9; //
 
      desc = "<color=#0cca74><b>Exploración: </b></color><color=#d3d3d3><i>El personaje explora los destinos posibles adelante de la caravana.</color></i>\\n\\nTiene 40% chances de revelar Nodos futuros al viajar a un Nodo nuevo. -5% Chances de Nodo Misterioso. +5% Chances de Atajo Subterráneo\\nSi se da un combate, lo arranca Fatigado.";

    }
}
