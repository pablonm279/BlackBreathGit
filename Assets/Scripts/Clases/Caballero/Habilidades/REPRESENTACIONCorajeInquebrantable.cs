using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONCorajeInquebrantable : Habilidad
{
   


     public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Caballero_CorajeInquebrantable");
      if (TRADU.i.nIdioma == 1)
      {
          txtDescripcion = "<color=#5dade2><b>Coraje Inquebrantable</b></color>\n\n"; 
          txtDescripcion += "<i>(Pasiva)Pelea con coraje, incluso en los momentos mas oscuros.\nSus puntos de valentía no pueden ser menores a 0.</i>\n\n";
      }
      if (TRADU.i.nIdioma == 2)
      {
          txtDescripcion = "<color=#5dade2><b>Unbreakable Valour</b></color>\n\n";
          txtDescripcion += "<i>(Passive)Fights with valour, even in the darkest moments.\nTheir Valour points cannot be less than 0.</i>\n\n";
      }
      if (TRADU.i.nIdioma == 3)
      {
          txtDescripcion = "<color=#5dade2><b>Coragem Inquebravel</b></color>\n\n";
          txtDescripcion += "<i>(Passiva)Luta com valentia, mesmo nos momentos mais sombrios.\nSeus pontos de Valentia nao podem ficar abaixo de 0.</i>\n\n";
      }

    }



    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    
    public override void ActualizarDescripcion(){}



}



