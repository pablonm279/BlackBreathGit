using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaNidoDefensivo : Trampa
{
  
  
  public void Inicializar()
  {
     nombre = "Trampa Nido Defensivo";
     intDificultadVer = 0;   
     intUsos = 100;
     intDuracionTurnos = 500;
     esPersistente = true;

     ActivarVFXModeloTrampa();

  }

  public override void AplicarEfectosTrampa(Unidad objetivo)
  {

    if (!objetivo.TieneBuffNombre("Nido Defensivo"))
    {

      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Nido Defensivo";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 1;
      buff.cantAtaque += 1 ;
      buff.cantDefensa += 1;
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
     
    }

    
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaNidoDefensivo;
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
