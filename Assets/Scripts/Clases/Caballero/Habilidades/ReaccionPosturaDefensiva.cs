using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;

public class ReaccionPosturaDefensiva : Reaccion
{
   Cortevertical corteVertical;
   void Start()
   {

    TipoTrigger =1;
    usos = 1;
    if(NIVEL == 5){ usos++;}
    permanente = false;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    corteVertical =  gameObject.GetComponent<Cortevertical>();
    

    descripcion = $"Reacción: El caballero atacará a cualquier enemigo que falle un ataque cuerpo a cuerpo contra él.";

   }

     public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)
     {
      if(melee == true) //solamente reacciona a ataques melee
      {
       uTriggerer.EstablecerAPActualA(0); //Cuando a una IA le reacciona un personaje, se queda sin AP, para que no haga cosas mientras el pj reacciona

        scEstaUnidad.ReproducirAnimacionAtaque();

        float delay = 0.6f;
        var pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseAtacar;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay * 0.5f) * 1000f);
        await Task.Delay(ms);

        int tirada =  UnityEngine.Random.Range(1,21);
        corteVertical.AplicarEfectosHabilidad(uTriggerer, tirada, null);

        BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} reacciona con {nombre}.");
      
        //--------------------------
        usos--;
        if(usos == 0)
        {
          Destroy(this);
        }
      }
    }


}

