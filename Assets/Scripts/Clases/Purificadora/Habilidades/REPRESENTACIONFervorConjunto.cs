using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONFervorConjunto : Habilidad
{
   

     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_FervorConjunto");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
     
        txtDescripcion = "<color=#5dade2><b>Fervor Conjunto</b></color>\n\n"; 
       txtDescripcion += "<i>(Pasiva) La Esperanza de la caravana le otorga Fervor a la Purificadora, recurso con varios usos para el combate.</i>\n\nCada carga de Fervor le otorga +1 Bonus Daño Divino y 1 de Barrera.";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
