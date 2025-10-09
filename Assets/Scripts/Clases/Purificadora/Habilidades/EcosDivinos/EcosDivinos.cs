using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class EcosDivinos : Trampa
{
  
 
  public Unidad unidadCreadora;
  public int NIVEL;
  public void InicializarCreador(Unidad creadora)
  {
    unidadCreadora = creadora;
    Inicializar();
  }
  
  public void Inicializar()
  {
     nombre = "Eco Divino";
     intDificultadVer = 0;   
     esTrampaFavorable = true;
     intUsos = 1;
     intDuracionTurnos = 2;
     esPersistente = true;
     ActivarVFXModeloTrampa();

  }
 
 
  
  public override void  AplicarEfectosTrampa(Unidad objetivo)
  {
    if (unidadCreadora != null && objetivo != null)
    {
      if (objetivo.CasillaPosicion.lado == unidadCreadora.CasillaPosicion.lado) //Aliados
      {

        int rand = UnityEngine.Random.Range(1, 11);
        if (NIVEL > 1) { rand += 2; }
        if (NIVEL > 2) { rand += 2; }
        if (NIVEL == 4) { rand += 5; }

        objetivo.RecibirCuracion(rand, true);
        objetivo.SumarValentia(1);

        if (objetivo == unidadCreadora) { unidadCreadora.GetComponent<ClasePurificadora>().CambiarFervor(1); }
      }
      else //Enemigos
      {
        int rand = UnityEngine.Random.Range(1, 11);
        if (NIVEL > 1) { rand += 2; }
        if (NIVEL > 2) { rand += 2; }
        if (NIVEL == 5) { rand += 5; }

        objetivo.RecibirDanio(rand, 11, false, unidadCreadora);

      }

      ReducirUsos();
    }
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaEcoDivino;
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

