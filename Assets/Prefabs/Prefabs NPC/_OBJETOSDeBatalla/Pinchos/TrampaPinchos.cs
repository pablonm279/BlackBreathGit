using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaPinchos : Trampa
{
  
  
  public void Inicializar()
  {
     nombre = "Trampa de Pinchos";
     intDificultadVer = 0;   
     intUsos = 2;
     intDuracionTurnos = 500;
     esPersistente = true;

     ActivarVFXModeloTrampa();

  }
 
  public override void  AplicarEfectosTrampa(Unidad objetivo)
  {
    
         int danio =UnityEngine.Random.Range(1,5)+2;
         objetivo.RecibirDanio(danio,1,false, null);

          ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaPinchos;
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
