using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrampaEscudoFe : Trampa
{
  
  public int NIVEL;
  public int Fervor;
  public void Inicializar(int Nivel, int fervor)
  {
    nombre = "Escudo de Fe";
    NIVEL = Nivel;
    intDificultadVer = 0;
    intUsos = 30;
    intDuracionTurnos = 3;
    if (Nivel == 4) { intDuracionTurnos += 1; }
    esPersistente = true;
    Fervor = fervor;

    ActivarVFXModeloTrampa();

  }

  public override void AplicarEfectosTrampa(Unidad objetivo)
  {
     
    bool yaTieneBuff = false;
    foreach (Buff buff in objetivo.GetComponents<Buff>())
    {
      if (buff.buffNombre == "Escudado por Fe")
      {
        yaTieneBuff = true;
        break;
      }
    }

    if (!yaTieneBuff)
    {

      /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
      buff.buffNombre = "Escudado por Fe";
      buff.boolfDebufftBuff = true;
      buff.DuracionBuffRondas = 1;
      buff.cantTsReflejos += 1 * Fervor;
      buff.cantTsMental += 1 * Fervor;
      buff.cantTsFortaleza += 1 * Fervor;
      if (NIVEL > 2) { buff.cantDefensa += 1; }
      buff.cantBarrera += 3 * Fervor;

      buff.AplicarBuff(objetivo);
      // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
      Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
      int randomNum = UnityEngine.Random.Range(2, 13);
      
      if (NIVEL == 5)
      {
        objetivo.RecibirCuracion(randomNum, true);
      }
      



    }

   ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaEscudoFe;
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