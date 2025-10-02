using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONPasoCauteloso : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Explorador_PasoCauteloso");
       ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
     
        txtDescripcion = "<color=#5dade2><b>Paso Cauteloso</b></color>\n\n"; 
        txtDescripcion += "<i>(Pasiva) Una vez por turno, al entrar en una casilla afectadas por un efecto hostil, el Explorador logra evadirlo.</i>\n\n";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
