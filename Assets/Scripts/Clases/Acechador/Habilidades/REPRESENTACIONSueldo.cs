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

   public override void ActualizarDescripcion()
   {

      txtDescripcion = "<color=#5dade2><b>Sueldo</b></color>\n\n";
      txtDescripcion += "<i>(Pasiva) Sin importar lo compleja de la situación, los Acechadores igual exigen un sueldo de 20 Monedas de Oro en cada descanso.  </i>\n\n-Si la Esperanza es mayor a 70, no demandarán el pago.";

      if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
      {
         txtDescripcion = "<color=#5dade2><b>Salary</b></color>\n\n";
         txtDescripcion += "<i>(Passive) No matter how complex the situation, Stalkers still demand a salary of 20 Gold Coins at each rest.</i>\n\n-If Hope is greater than 70, they will not demand payment.";
      }
   
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
