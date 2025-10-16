using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONAlmaEndeble : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Purificadora_AlmaEndeble");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
     
        txtDescripcion = "<color=#5dade2><b>Alma Endeble</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) La Purificadora sufre Aflicciones de combate en proporción a la intensidad del Aliento Negro.</i>\n\n";
        if (TRADU.i.nIdioma == 2) // agrega la traducción a inglés
        {
            txtDescripcion = "<color=#5dade2><b>Fragile Soul</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The Purifier suffers combat Afflictions in proportion to the intensity of the Black Breath.</i>\n\n";
        }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
