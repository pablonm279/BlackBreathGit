using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;

public class ReaccionAtaqueCuervo : Reaccion
{
   IAAtaqueCuervo ataqueCuervo;
   int rondaReaccion = -1;
   readonly HashSet<Unidad> atacantesEstaRonda = new HashSet<Unidad>();
   void Start()
   {

    TipoTrigger =2;
    usos = 100;

    permanente = true;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    ataqueCuervo =  gameObject.GetComponent<IAAtaqueCuervo>();


    if (TRADU.i.nIdioma == 2)
    {
      descripcion = $"Reaction: The crow will attack the eyes of anyone who harms its master.";
    }
    else
    {
      descripcion = $"Reacción: El cuervo atacará los ojos de quien lastime a su maestra.";
    }

   }

     public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)
     {
        if (uTriggerer == null || scEstaUnidad == null || ataqueCuervo == null || scEstaUnidad.HP_actual <= 0f)
        {
          return;
        }

        EnemigoUnidadBrujaKaleTav scEstaBruja = scEstaUnidad.GetComponent<EnemigoUnidadBrujaKaleTav>();
        if (scEstaBruja == null || scEstaBruja.CuervoOcupado)
        {
          return; // el cuervo todavia esta fuera de un ataque anterior
        }

        // Una vez por atacante por ronda
        int rondaActual = BattleManager.Instance.RondaNro;
        if (rondaReaccion != rondaActual)
        {
          rondaReaccion = rondaActual;
          atacantesEstaRonda.Clear();
        }
        if (!atacantesEstaRonda.Add(uTriggerer)) return;

        // El log se escribe al empezar la secuencia, antes de que el cuervo vuele, para que se entienda la causa.
        if (TRADU.i.nIdioma == 2)
        {
            BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} reacts with Crow Attack.");
        }
        else
        {
            BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} reacciona con Ataque Cuervo.");
        }

        //---------------Ataque Cuervo Ojos----------------
        // Secuencia: rotulo -> el cuervo sale del hombro -> vuela en arco -> pica (aca cae el efecto) -> vuelve.
        await scEstaBruja.AnimarAtaqueCuervoAsync(uTriggerer, () => ataqueCuervo.AplicarEfectosHabilidad(uTriggerer));
        //--------------------------
        usos--;
        if(usos == 0)
        {
          Destroy(this);
        }

    }


}



