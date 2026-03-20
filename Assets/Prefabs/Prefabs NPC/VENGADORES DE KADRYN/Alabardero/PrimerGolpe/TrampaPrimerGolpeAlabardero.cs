using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TrampaPrimerGolpeAlabardero : Trampa
{
  
  IAGolpeAlabarda tiroArco;
  bool destruidaPorMuerteCreador;
  bool creadorAsignado;
  public void InicializarCreador(Unidad creadora)
  {
    AsignarCreador(creadora);
    print(111);
     Invoke("Activar", 0.5f);
    tiroArco = unidadCreadora.GetComponent<IAGolpeAlabarda>();
    Inicializar();
    creadorAsignado = true;
    
  
  }

  public void Inicializar()
  {
    nombre = "Primer Golpe";
    intDificultadVer = 0;
    intUsos = 1;
    intDuracionTurnos = 1;
    esPersistente = false;
    tiroArco = unidadCreadora.GetComponent<IAGolpeAlabarda>();
    ActivarVFXModeloTrampa();
   

  }

  bool activa = false;

  void Activar()
  { 
    activa = true;
  
  }

  void Update()
  {
    VerificarCreadorSigueActivo();
  }

  public override void AplicarEfectosTrampa(Unidad objetivo)
  {
    if (activa != true)
    {
      return;
    }
    if (unidadCreadora != null && tiroArco != null && objetivo != null)
    {

      unidadCreadora.ReproducirAnimacionAtaque();

      tiroArco.AplicarEfectosHabilidad(objetivo);

      if (BattleManager.Instance != null && TRADU.i != null)
      {
        BattleManager.Instance.EscribirLog(unidadCreadora.uNombre + TRADU.i.Traducir(" reacciona con Primer Golpe."));
      }

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

  void VerificarCreadorSigueActivo()
  {
    if (destruidaPorMuerteCreador)
    {
      return;
    }

    if (!creadorAsignado)
    {
      return;
    }

    if (unidadCreadora == null)
    {
      DestruirTrampaPorMuerteCreador();
      return;
    }

    if (unidadCreadora.HP_actual <= 0 || !unidadCreadora.gameObject.activeInHierarchy)
    {
      DestruirTrampaPorMuerteCreador();
    }
  }

  void DestruirTrampaPorMuerteCreador()
  {
    if (destruidaPorMuerteCreador)
    {
      return;
    }

    destruidaPorMuerteCreador = true;
    DestruirTrampa();
  }

}


