using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

public class ReaccionVengadorKadryn : Reaccion
{
  void Start()
  {

    TipoTrigger = 3;
    usos = 1;

    permanente = true;

    nombre = "Aliento Negro";
    scEstaUnidad = gameObject.GetComponent<Unidad>();


    descripcion = TRADU.i.Traducir("Reacción: Al morir genera restos de Aliento Negro en el campo de batalla.");

  }

  public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
  {
    List<Casilla> casillasalre = uTriggerer.CasillaPosicion.ObtenerCasillasAlrededor(1);
    casillasalre.Add(uTriggerer.CasillaPosicion);

    foreach (var casilla in casillasalre)
    {
      if (Random.value <= 0.5f) // 50% chance
      {
        TrampaAlientoNegro tr1 = casilla.gameObject.AddComponent<TrampaAlientoNegro>();
        tr1.Inicializar();
        tr1.AsignarCreador(scEstaUnidad);

      }
    }
  }

  
  


}


