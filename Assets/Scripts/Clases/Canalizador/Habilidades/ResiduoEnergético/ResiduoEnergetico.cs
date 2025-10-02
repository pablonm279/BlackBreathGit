using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ResiduoEnergetico : Trampa
{
  
 
  public Unidad unidadCreadora;
  public int NIVEL;
  public void InicializarCreador(Unidad creadora, int NIVEL)
  {
    unidadCreadora = creadora;
    this.NIVEL = NIVEL;
    Inicializar();
  }
  
  public void Inicializar()
  {
     nombre = "Residuo Energetico";
     intDificultadVer = 0;   
     esTrampaFavorable = true;
     intUsos = 1;
     intDuracionTurnos = 2;
     esPersistente = false;
     ActivarVFXModeloTrampa();

  }
 
 
  
  public async override void  AplicarEfectosTrampa(Unidad objetivo)
  {
    
      /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Residuo Energético";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 1;
       if (NIVEL == 4) {  buff.DuracionBuffRondas += 1;}     
       buff.cantDamBonusElementalArc += 3;
       if (NIVEL > 1) { buff.cantDamBonusElementalArc++; }
       buff.cantAtaque += 1;
       objetivo.CambiarAPActual(1); 
       if (NIVEL > 2) {   objetivo.CambiarAPActual(1);  }
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

    int rand =UnityEngine.Random.Range(1,9);
    if (objetivo is ClaseCanalizador scCan)
    {
      scCan.RecibirCuracion(rand, true);
    }
    else
    { 
      objetivo.RecibirDanio(rand, 8,false,unidadCreadora);
    }

    
     ReducirUsos();
  }

  void ActivarVFXModeloTrampa()
  {
    prefabModelo = scBattleManager.contenedorPrefabs.TrampaResiduoEnergetico;
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
