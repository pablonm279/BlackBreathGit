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

          if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
          {
              txtDescripcion = "<color=#5dade2><b>Overload</b></color>\n\n";
              txtDescripcion += "<i>(Passive) At the end of each combat, the character receives 15% of their Maximum Health as damage for each accumulated Energy Level. This damage cannot be fatal, but will increase the character's recovery time in Campaign.</i>\n\n";
          }
          else if (TRADU.i.nIdioma == 3)
          {
              txtDescripcion = "<color=#5dade2><b>Sobrecarga</b></color>\n\n";
              txtDescripcion += "<i>(Passiva) No fim de cada combate, o personagem recebe 15% da Vida Maxima como dano para cada Nivel de Energia acumulado. Esse dano nao pode ser fatal, mas aumenta o tempo de recuperacao do personagem na Campanha.</i>\n\n";
          }

    }


    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada){}
    public override void Activar()
    {
       

      
       
        
    }
    




}



