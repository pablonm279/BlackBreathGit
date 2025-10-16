using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class REPRESENTACIONSigiloso : Habilidad
{
   

    
    public override void  Awake()
    {
      imHab = Resources.Load<Sprite>("imHab/Acechador_Sigiloso");
      ActualizarDescripcion();
      IDenClase = 0;
      
    }

    public bool seusoEsteTurno = false;

  public override void ActualizarDescripcion()
  {
    txtDescripcion = "<color=#5dade2><b>Sigiloso</b></color>\n\n";
    txtDescripcion += "<i>(Pasiva) El Acechador comienza escondido cada combate que no sea una emboscada enemiga.</i>\n\nMientras está escondido gana 'Al Acecho' que le otorga +2 Ataque +1 Dado Crítico y 10% de daño. Además no es perjudicado en combates nocturnos.";
    if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
    {
      txtDescripcion = "<color=#5dade2><b>Stealthy</b></color>\n\n";
      txtDescripcion += "<i>(Passive) The Stalker starts hidden in every combat that is not an enemy ambush.</i>\n\nWhile hidden, gains 'On the Prowl' which grants +2 Attack, +1 Critical Die, and 10% damage. Also, is not penalized in night combats.";
    }

  }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}
