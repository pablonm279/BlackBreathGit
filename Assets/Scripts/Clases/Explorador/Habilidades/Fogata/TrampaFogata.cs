using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaFogata : Trampa
{
  
  public int NIVEL;
  public void Inicializar(int Nivel)
  {
     nombre = "Fogata";
     NIVEL = Nivel;
     intDificultadVer = 0;   
     intUsos = 3;
     intDuracionTurnos = 3;
     if(Nivel > 1){intUsos++;intDuracionTurnos++;}
     esPersistente = true;

     ActivarVFXModeloTrampa();

  }

  public override void AplicarEfectosTrampa(Unidad objetivo)
  {

    int danio =UnityEngine.Random.Range(1, 4);
    objetivo.RecibirDanio(danio, 4, false, null);
         
         ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaFogata;
    GOvfx = Instantiate(prefabModelo, transform.position, transform.rotation) as GameObject;
    // Ajusta la posición en el eje Y
    /*Vector3 newPosition = GOvfx.transform.position;
    newPosition.y += 0.035f;
    GOvfx.transform.position = newPosition;*/

     Canvas canvas = GOvfx.GetComponentInChildren<Canvas>();
    if (canvas != null)
    {
      canvas.overrideSorting = true;
      float posY = gameObject.GetComponent<Casilla>().posY;
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) + 2;
    }
  }
}