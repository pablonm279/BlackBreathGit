using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;

public class ReaccionAtaqueCuervo : Reaccion
{
   IAAtaqueCuervo ataqueCuervo;
   void Start()
   {

    TipoTrigger =2;
    usos = 100;
  
    permanente = true;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    ataqueCuervo =  gameObject.GetComponent<IAAtaqueCuervo>();
    

    if (TRADU.i.nIdioma == 1)
    {
      descripcion = $"Reacción: El cuervo atacará los ojos de quien lastime a su maestra.";
    }
    if (TRADU.i.nIdioma == 2)
    {
      descripcion = $"Reaction: The crow will attack the eyes of anyone who harms its master.";
    }

   }

     public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)
     {
     

        //---------------Ataque Cuervo Ojos----------------
        float delay = 0.6f;
       

        int ms = Mathf.RoundToInt(Mathf.Max(0.2f, delay * 0.5f) * 1000f);
        await BattleManager.DelayCombateAsync(ms);
         EnemigoUnidadBrujaKaleTav scEstaBruja = scEstaUnidad.GetComponent<EnemigoUnidadBrujaKaleTav>();
         scEstaBruja.MostrarImagenSinCuervoPorTresSegundos();
       
        ataqueCuervo.AplicarEfectosHabilidad(uTriggerer);

        if (TRADU.i.nIdioma == 1)
        {
            BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} reacciona con Ataque Cuervo.");
        }
        if (TRADU.i.nIdioma == 2)
        {
            BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} reacts with Crow Attack.");
        }
        //--------------------------
        usos--;
        if(usos == 0)
        {
          Destroy(this);
        }
      
    }


}



