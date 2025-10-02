using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaMasaContaminada : Trampa
{
  
  
  public void Inicializar()
  {
     nombre = "Masa Contaminada";
     intDificultadVer = 0;   
     intUsos = 1;
     esPersistente = false;
     intDuracionTurnos = 2;
     esTrampaFavorable = false;

     ActivarVFXModeloTrampa();

    

  }

  public override void AplicarEfectosTrampa(Unidad unidad)
  {

    if (!unidad.TieneTag("Corrupto"))
    {
      int rand =UnityEngine.Random.Range(1, 9);
      unidad.RecibirDanio(rand, 7, true, null); //Daño acido CRITICO

      if (unidad.estado_Corrupto) //Hace daño necrótico a personajes corruptas
      {
        rand =UnityEngine.Random.Range(1, 12);
        unidad.RecibirDanio(rand, 9, false, null);
      }
    }
    else
    { 
      // BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Potenciado por Masa Contaminada";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 3;
      buff.cantAPMax += 1;
      buff.cantDamBonusElementalNec += 6;
      buff.AplicarBuff(unidad);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, unidad.gameObject);

      unidad.RecibirCuracion(5, false);


    }
   



    ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.MasaContaminada;
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
