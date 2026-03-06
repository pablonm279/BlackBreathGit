using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaBarro : Trampa
{
  
  
  public void Inicializar()
  {
     nombre = "Trampa Barro";
     intDificultadVer = 0;   
     intUsos = 100;
     esPersistente = false;
     intDuracionTurnos = 100;
     esTrampaFavorable = false;

     ActivarVFXModeloTrampa();

    

  }

  public override void AplicarEfectosTrampa(Unidad unidad)
  {

    unidad.CambiarAPActual(-2);

    ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaBarro;
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
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) - 2;
    }
  }

}


