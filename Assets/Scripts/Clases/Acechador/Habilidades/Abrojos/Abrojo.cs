using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Abrojo : Trampa
{
  
  public int NIVEL;
  public void InicializarCreador(Unidad creadora, int Nivel)
  {
    AsignarCreador(creadora);
    Inicializar(Nivel);
  }

  public void Inicializar(int NIVEL)
  {
    nombre = "Abrojo";
    intDificultadVer = 0;
    esTrampaFavorable = false;
    intUsos = 1;
    intDuracionTurnos = 10;
    esPersistente = false;
    ActivarVFXModeloTrampa();
    this.NIVEL = NIVEL;

  }
 
 
  
  public async override void  AplicarEfectosTrampa(Unidad objetivo)
  {
     
         
      int rand =UnityEngine.Random.Range(1, 12);
      if(NIVEL > 1){rand+=1;}
     
      int DC = 11;
      if(NIVEL > 2){ DC++;}
      

    if (objetivo.TiradaSalvacion(2, DC))
    {
      rand *= 2;
       int stacksSangrado = 4 + (NIVEL == 4 ? 1 : 0);
       Estados.Aplicar_Sangrado(objetivo, stacksSangrado, unidadCreadora);
       if(NIVEL == 5){ objetivo.CambiarAPActual(-1); }

    }

      objetivo.RecibirDanio(rand, 2, false, unidadCreadora, 0, true);

      

    ReducirUsos();

   
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaAbrojo;
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
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) + 2;
    }
  }

}


