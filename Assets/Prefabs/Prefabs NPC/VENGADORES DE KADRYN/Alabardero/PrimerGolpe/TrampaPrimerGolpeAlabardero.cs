using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TrampaPrimerGolpeAlabardero : Trampa
{
  
  IAGolpeAlabarda tiroArco;
  public Unidad unidadCreadora;
  public void InicializarCreador(Unidad creadora)
  {
    unidadCreadora = creadora;
    print(111);
     Invoke("Activar", 0.5f);
    tiroArco = unidadCreadora.GetComponent<IAGolpeAlabarda>();
    Inicializar();
    
  
  }

  public void Inicializar()
  {
    nombre = "Primer Golpe";
    intDificultadVer = 0;
    intUsos = 1;
    intDuracionTurnos = 2;
    esPersistente = false;
    tiroArco = unidadCreadora.GetComponent<IAGolpeAlabarda>();
    ActivarVFXModeloTrampa();
   

  }

  bool activa = false;

  void Activar()
  { 
    activa = true;
   print(222);
  }

  public async override void AplicarEfectosTrampa(Unidad objetivo)
  {
    if (activa != true)
    {
      return;
    }
    if (unidadCreadora != null)
    {

      unidadCreadora.ReproducirAnimacionAtaque();

      await Task.Delay(200);

      tiroArco.AplicarEfectosHabilidad(objetivo);

      BattleManager.Instance.EscribirLog(unidadCreadora.uNombre + TRADU.i.Traducir(" reacciona con Primer Golpe."));

      //--------------------------

      ReducirUsos();
    }

  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaPrimerGolpeAlabardero;
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
