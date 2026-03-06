using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaAlientoNegro : Trampa
{
  
  
  public void Inicializar()
  {
     nombre = "Trampa Aliento Negro";
     intDificultadVer = 3;   
     intUsos = 1;
     intDuracionTurnos = 10;
     esPersistente = false;

     ActivarVFXModeloTrampa();

  }
 
  public override void  AplicarEfectosTrampa(Unidad objetivo)
  {

      objetivo.RecibirCuracion(20, true);
      // BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Extasiado por Aliento Negro";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 2;
      buff.cantTsFortaleza += 3;
      buff.cantTsMental += 3;
      buff.cantAtaque += 2;
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);    
    

    ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaAlientoNegro;
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



