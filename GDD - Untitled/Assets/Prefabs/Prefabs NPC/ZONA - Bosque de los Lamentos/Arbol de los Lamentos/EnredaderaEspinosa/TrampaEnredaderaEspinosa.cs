using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaEnredaderaEspinosa : Trampa
{
  
  
  public void Inicializar()
  {
     nombre = "Enredadera Espinoza";
     intDificultadVer = 0;   
     intUsos = 10;
     intDuracionTurnos = 5;
     esPersistente = true;

     ActivarVFXModeloTrampa();

  }
 
  public override void  AplicarEfectosTrampa(Unidad objetivo)
  {
    
         int danio =UnityEngine.Random.Range(1,5)+2;
         objetivo.RecibirDanio(danio,2,false, null);

         if(objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, 14)&& objetivo.estado_inmovil < 1)
          {
            /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Enredado";
            buff.boolfDebufftBuff = false;
            buff.DuracionBuffRondas = 1;
            buff.cantAPMax -= 1;
            buff.cantDefensa -= 2;
            buff.AplicarBuff(objetivo);

            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

            objetivo.estado_inmovil = buff.DuracionBuffRondas;
          

          }
        
          ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaEnredadoraEspinosa;
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
