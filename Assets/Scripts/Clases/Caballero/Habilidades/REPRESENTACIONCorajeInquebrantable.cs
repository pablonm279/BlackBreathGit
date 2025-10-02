using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONCorajeInquebrantable : Habilidad
{
   


     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_CorajeInquebrantable");
      txtDescripcion = "<color=#5dade2><b>Coraje Inquebrantable</b></color>\n\n"; 
      txtDescripcion += "<i>(Pasiva)Pelea con coraje, incluso en los momentos mas oscuros.\nSus puntos de valentía no pueden ser menores a 0.</i>\n\n";
 

    }



    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    
    public override void ActualizarDescripcion(){}



}
