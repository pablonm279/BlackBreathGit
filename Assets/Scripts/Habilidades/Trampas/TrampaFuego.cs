using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaFuego : Trampa
{
  
   public void Inicializar()
  {
     nombre = "Llama";
     intDificultadVer = 0;   
     intUsos = 10;
     intDuracionTurnos = 500;
     esPersistente = true;

     ActivarVFXModeloTrampa();

  }
 
  public override void  AplicarEfectosTrampa(Unidad objetivo)
  {
    
         int danio =UnityEngine.Random.Range(5,14)+2;
         objetivo.RecibirDanio(danio,4,false, null);

        if (objetivo.TiradaSalvacion(objetivo.mod_TSReflejos, 12))
        {
          objetivo.estado_ardiendo = +2;
        }

          ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaFuego;
    GOvfx = Instantiate(prefabModelo, transform.position, transform.rotation) as GameObject;
    // Ajusta la posición en el eje Y
    Vector3 newPosition = GOvfx.transform.position;
    newPosition.y += 0.035f;
    GOvfx.transform.position = newPosition;

     Canvas canvas = GOvfx.GetComponentInChildren<Canvas>();
    if (canvas != null)
    {
      canvas.overrideSorting = true;
      float posY = gameObject.GetComponent<Casilla>().posY;
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) + 2;
    }
  }

}
