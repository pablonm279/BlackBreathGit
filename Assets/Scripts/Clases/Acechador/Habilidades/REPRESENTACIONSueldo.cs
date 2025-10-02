using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONSueldo : Habilidad
{
   

    
   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_Sueldo");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

    public override void  ActualizarDescripcion()
    {
     
       txtDescripcion = "<color=#5dade2><b>Sueldo</b></color>\n\n"; 
       txtDescripcion += "<i>(Pasiva) Sin importar lo compleja de la situación, los Acechadores igual exigen un sueldo de 20 Monedas de Oro en cada descanso.  </i>\n\n-Si la Esperanza es mayor a 70, no demandarán el pago.";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
