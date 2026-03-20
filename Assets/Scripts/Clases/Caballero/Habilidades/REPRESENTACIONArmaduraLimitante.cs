using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONArmaduraLimitante : Habilidad
{
   


   public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_ArmaduraLimitante");
      if (TRADU.i.nIdioma == 1)
      {
          txtDescripcion = "<color=#cb5000><b>Armadura Limitante</b></color>\n\n";
          txtDescripcion += "<i>(Debilidad) El caballero carga con una armadura extremadamente pesada en todo el cuerpo, por lo tanto es más propenso a perder el balance al actuar. +1 Rango pifias.</i>\n\n";
      }
      if (TRADU.i.nIdioma == 2)
      {
          txtDescripcion = "<color=#cb5000><b>Limiting Armor</b></color>\n\n";
          txtDescripcion += "<i>(Weakness) The knight wears extremely heavy armor all over the body, so is more likely to lose balance when acting. +1 Fumble range.</i>\n\n";
      }
      if (TRADU.i.nIdioma == 3)
      {
          txtDescripcion = "<color=#cb5000><b>Armadura Limitante</b></color>\n\n";
          txtDescripcion += "<i>(Fraqueza) O cavaleiro usa uma armadura extremamente pesada em todo o corpo, ficando mais propenso a perder o equilibrio ao agir. +1 faixa de falha critica.</i>\n\n";
      }


    }



    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    public override void ActualizarDescripcion(){}




}


