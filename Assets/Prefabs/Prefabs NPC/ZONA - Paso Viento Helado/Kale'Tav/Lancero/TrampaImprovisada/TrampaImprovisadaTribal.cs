using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaImprovisadaTribal : Trampa
{
  
  
  public void Inicializar()
  {
     nombre = "Trampa Improvisada";
     intDificultadVer = 3;   
     intUsos = 1;
     intDuracionTurnos = 3;
     esPersistente = false;

     ActivarVFXModeloTrampa();

  }
 
  public override void  AplicarEfectosTrampa(Unidad objetivo)
  {
    
         int danio =UnityEngine.Random.Range(1,10)+2;
         objetivo.RecibirDanio(danio,2,false, null);

    if (objetivo.TiradaSalvacion(objetivo.mod_TSReflejos, 12))
    {
    

      // BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Marcado";
      buff.boolfDebufftBuff = false;
      buff.DuracionBuffRondas = 3;
      buff.cantDefensa -= 3;
      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);    
    }

    ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaImprovisadaTribal;
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

    TrampaSpawnLightFx flashAparicion = GOvfx.GetComponent<TrampaSpawnLightFx>();
    if (flashAparicion == null)
    {
      flashAparicion = GOvfx.AddComponent<TrampaSpawnLightFx>();
    }

    flashAparicion.Reproducir();
  }

}



