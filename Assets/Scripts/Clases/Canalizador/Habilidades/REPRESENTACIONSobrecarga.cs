using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONSobrecarga : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Canalizador_Sobrecarga");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

  
    public override void  ActualizarDescripcion()
    {

      
         txtDescripcion = "<color=#5dade2><b>Sobrecarga</b></color>\n\n"; 
         txtDescripcion += "<i>(Pasiva) Al final de cada combate, el personaje recibe un 15% de su Vida Máxima como daño por cada Nivel de Energía acumulada. Este daño no puede ser Mortal, pero incrementará el tiempo de recuperación del Personaje en Campaña.</i>\n\n";

      

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
