using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarcaMarcarPresa : Marca
{
    public int NIVEL;
    
    void Start()
   {
   

    descripcion = $"Marcado: {quienMarco.uNombre} posee bonificaciones en ataques individuales contra este enemigo.";

   }

}
