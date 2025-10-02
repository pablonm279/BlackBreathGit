using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaBombaHumo : Trampa
{
  
  public int NIVEL;
  public void Inicializar(int Nivel)
  {
    nombre = "Bomba de Humo";
    NIVEL = Nivel;
    intDificultadVer = 0;
    intUsos = 30;
    intDuracionTurnos = 2;
    if (Nivel == 4) { intDuracionTurnos += 1; }
    esPersistente = true;
    

    ActivarVFXModeloTrampa();

  }
  Unidad objetivo;
  public override void AplicarEfectosTrampa(Unidad objetivo)
  {
    this.objetivo = objetivo;

      
    
      Invoke("EsconderHumoDelay", 0.2f);
    

   ReducirUsos();


  }

  void EsconderHumoDelay()
  {
    if (objetivo.ObtenerEstaEscondido() == 0)
    { objetivo.GanarEscondido(1); }

    if (!(objetivo is ClaseAcechador)) //Si no es acechador recibe el buff escondido por humo (el acechador tiene su propio buff de ataque en sigilo)
    {
      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Escondido Por Humo";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 2;
      buff.cantAtaque = 2;
      buff.cantCritDado = 1;

      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
    }


  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaBombaHumo;
    GOvfx = Instantiate(prefabModelo, transform.position, transform.rotation) as GameObject;
    // Ajusta la posición en el eje Y
    /*Vector3 newPosition = GOvfx.transform.position;
    newPosition.y += 0.035f;
    GOvfx.transform.position = newPosition;*/

     Canvas canvas = GOvfx.GetComponentInChildren<Canvas>();
    if (canvas != null)
    {
      canvas.overrideSorting = true;
      float posY = gameObject.GetComponent<Casilla>().posY;
      canvas.sortingOrder = 60 - Mathf.RoundToInt(10 * posY) + 2;
    }
  }
}