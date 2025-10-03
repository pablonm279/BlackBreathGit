using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ReaccionEscudoEnergetico : Reaccion
{
   DescargaArcana descargaArcana;
   void Start()
   {

    TipoTrigger =1;
    usos = 2;
    if(NIVEL == 5){ usos++;}
    permanente = false;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    descargaArcana =  gameObject.GetComponent<DescargaArcana>();
    

    descripcion = $"Reacción: El canalizador se defiende y contraataca con Descargas Arcanas si es atacado con proyectiles sin éxito.";

   }

     public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0,  float variableFlexible2 = 0)
     {
      if(melee == false) //no reacciona a ataques melee
      {
       uTriggerer.EstablecerAPActualA(0); //Cuando a una IA le reacciona un personaje, se queda sin AP, para que no haga cosas mientras el pj reacciona

        scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();

        await Task.Delay(200);

        int tirada =  UnityEngine.Random.Range(1,21);
        if(NIVEL > 2){ tirada++;}

        descargaArcana.AplicarEfectosHabilidad(uTriggerer, tirada, null);

        BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} reacciona con {nombre}.");

       //---Genera Residuo energetico
      List<Casilla> casillasAlrededor = scEstaUnidad.CasillaPosicion.ObtenerCasillasAlrededor(2);
      List<Casilla> casillasDesocupadas = casillasAlrededor.FindAll(c => c.Presente == null);
      if (casillasDesocupadas.Count > 0)
      {
        Casilla casillaAlAzar = casillasDesocupadas[UnityEngine.Random.Range(0, casillasDesocupadas.Count)];
        // Aquí puedes usar casillaAlAzar según lo que necesites hacer
         casillaAlAzar.AddComponent<ResiduoEnergetico>();
         casillaAlAzar.GetComponent<ResiduoEnergetico>().InicializarCreador(scEstaUnidad, NIVEL);
      }
        //--------------------------
      usos--;
        if(usos == 0)
        {
          Destroy(this);
        }
      }
    }


}

