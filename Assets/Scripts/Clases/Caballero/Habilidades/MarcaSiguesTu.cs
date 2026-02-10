using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarcaSiguesTu : Marca
{
    public int NIVEL;
    
    void Start()
   {
   

    descripcion = TRADU.i.Traducir("Marca: ") + TRADU.i.Traducir(quienMarco.uNombre) + TRADU.i.Traducir(" posee bonificaciones de daño y ataque con ataques individuales contra este enemigo.");

   }

}
