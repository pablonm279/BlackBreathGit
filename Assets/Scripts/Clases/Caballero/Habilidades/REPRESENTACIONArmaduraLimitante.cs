using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONArmaduraLimitante : Habilidad
{
   


   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_ArmaduraLimitante");
      txtDescripcion = "<color=#cb5000><b>Armadura Limitante</b></color>\n\n"; 
      txtDescripcion += "<i>(Debilidad) El caballero carga con una armadura extremadamente pesada en todo el cuerpo, por lo tanto es más propenso a perder el balance al actuar. +1 Rango pifias.</i>\n\n";
 



    }



    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    public override void ActualizarDescripcion(){}




}
