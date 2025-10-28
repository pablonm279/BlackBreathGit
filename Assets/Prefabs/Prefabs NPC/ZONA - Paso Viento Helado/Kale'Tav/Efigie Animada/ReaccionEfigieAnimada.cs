using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

public class ReaccionEfigieAnimada : Reaccion
{
  void Start()
  {

    TipoTrigger = 3;
    usos = 1;

    permanente = true;

    nombre = "Condena";
    scEstaUnidad = gameObject.GetComponent<Unidad>();


    descripcion = TRADU.i.Traducir("Reacción: Al morir condena al enemigo que dió el último golpe.");

  }

  public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
  {



    //uTriggerer no es el causante en reaccion de muerte, si no el muerto jeje

    if (uTriggerer.HP_actual < 1)
    {
      Unidad enemigoCondenado = BattleManager.Instance.unidadActiva; //El que dió el golpe final

      if (enemigoCondenado != null)
      {

        if (enemigoCondenado.estado_Condenado == 0) //Si no está ya condenado
        {
          enemigoCondenado.estado_Condenado = 2; //2 turnos de condena
          enemigoCondenado.GenerarTextoFlotante( TRADU.i.Traducir("Condenado"), Color.gray);
          BattleManager.Instance.EscribirLog(enemigoCondenado.uNombre + TRADU.i.Traducir(" es condenado por 3 turnos."));

          //--------------------------
          usos--;
          if (usos == 0)
          {
            Destroy(this);
          }


        }
      }
    }

  }
  


}
