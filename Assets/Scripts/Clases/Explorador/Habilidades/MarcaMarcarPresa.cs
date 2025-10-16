using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarcaMarcarPresa : Marca
{
    public int NIVEL;
    
    void Start()
   {
   

    descripcion = $"Marcado: {quienMarco.uNombre} posee bonificaciones en ataques individuales contra este enemigo.";
    if (TRADU.i.nIdioma == 2) // agrega la traducción a inglés
    {
        descripcion = $"Marked: {quienMarco.uNombre} has bonuses on single-target attacks against this enemy.";
    }

   }

}
