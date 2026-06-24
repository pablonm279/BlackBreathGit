using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ReaccionCastigarMalvados : Reaccion
{
  
   void Start()
   {
    int NIVEL = variableUnidad.GetComponent<CastigaraLosMalvados>().NIVEL;
    TipoTrigger =4;
    usos = 2;
    if(NIVEL == 4){usos++;}
    
    permanente = true;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    nombre = "Castigar a los Malvados";
    
    

    descripcion = $"Reacción: Cada vez que dañe a un enemigo, esta unidad deberá superar una tirada Mental o sufrir daño y perder sus AP restantes.";
    if (TRADU.i.nIdioma == 2)
    {
      descripcion = "Reaction: Each time this unit damages an enemy, it must pass a Mental saving throw or suffer damage and lose its remaining AP.";
    }
    else if (TRADU.i.nIdioma == 3)
    {
      descripcion = "Reacao: Cada vez que esta unidade causar dano a um inimigo, ela deve passar em uma resistencia Mental ou sofrer dano e perder seu AP restante.";
    }

   }

   public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)
   {
      float DC = variableUnidad.mod_CarPoder + 10;
      if(NIVEL > 1){DC++;}

      if(scEstaUnidad.TiradaSalvacion(3, DC))
      {

      scEstaUnidad.EstablecerAPActualA(0); //Cuando a una IA le reacciona un personaje, se queda sin AP, para que no haga cosas mientras el pj reacciona
        
        float danioBase =UnityEngine.Random.Range(1, 7);
        danioBase += variableUnidad.mod_CarPoder;

        if(NIVEL == 5)
        { danioBase += variableFlexible2/2;}
        else{ danioBase += variableFlexible2/3;}
       

        scEstaUnidad.RecibirDanio(danioBase,11, false, variableUnidad);
       
        usos--;
        if(usos == 0)
        {
          Destroy(this);
        }
      }
      else  
      {
          Destroy(this);

      }
      
   }


}



