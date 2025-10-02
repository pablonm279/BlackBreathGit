using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class VigilanciaTrampa : Trampa
{
  
  TiroconArco tiroArco;
  public Unidad unidadCreadora;
  public void InicializarCreador(Unidad creadora)
  {
    unidadCreadora = creadora;
    Inicializar();
  }
  
  public void Inicializar()
  {
     nombre = "Vigilancia";
     intDificultadVer = 0;   
     intUsos = 1;
     intDuracionTurnos = 1;
     esPersistente = false;
     tiroArco =  unidadCreadora.GetComponent<TiroconArco>();
     ActivarVFXModeloTrampa();

  }



  public async override void AplicarEfectosTrampa(Unidad objetivo)
  {

    if (unidadCreadora.GetComponent<Vigilancia>().disparosEsteTurno > 0)
    {
      unidadCreadora.GetComponent<Vigilancia>().disparosEsteTurno--;
      //objetivo.AccionP_actual = 0; //Cuando a una IA le reacciona un personaje, se queda sin AP, para que no haga cosas mientras el pj reacciona

      unidadCreadora.ReproducirAnimacionHabilidadNoHostil();

      await Task.Delay(200);

      int tirada = UnityEngine.Random.Range(1, 21);

      if (unidadCreadora.GetComponent<Vigilancia>().NIVEL > 1)
      {
        tirada += 1;
      }
      if (unidadCreadora.GetComponent<Vigilancia>().NIVEL > 2)
      {
        tirada += 1;
      }

      tiroArco.AplicarEfectosHabilidad(objetivo, tirada, null);

      BattleManager.Instance.EscribirLog($"{unidadCreadora.uNombre} reacciona con {nombre}.");

      //--------------------------
    }
      ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaVigilancia;
    GOvfx = Instantiate(prefabModelo, transform.position, transform.rotation) as GameObject;
    // Ajusta la posición en el eje Y
    Vector3 newPosition = GOvfx.transform.position;
    newPosition.y += 0.015f;
    GOvfx.transform.position = newPosition;

     Canvas canvas = GOvfx.GetComponentInChildren<Canvas>();
    if (canvas != null)
    {
      canvas.overrideSorting = true;
      float posY = gameObject.GetComponent<Casilla>().posY;
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) -2;
    }
  }

}
