using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class ReaccionSifonArcano : Reaccion
{

  void Start()
  {
    int NIVEL = variableUnidad.GetComponent<SifonArcano>().NIVEL;
    TipoTrigger = 5;
    usos = 3;
    if (NIVEL == 5) { usos++; }

    permanente = true;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    nombre = "Sifón Arcano";



    descripcion = $"Reacción: Al final de cada turno recibirá daño arcano, incrementado por la cantidad de Residuos de Energía presentes.";
    if (TRADU.i.nIdioma == 2) // agrega la traducción a inglés
    {
      descripcion = "Reaction: At the end of each turn, receives arcane damage increased by the number of Energy Residues present.";
    }

  }

  public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
  {

    if (variableUnidad is ClaseCanalizador cana)
    {


      int cantidadResiduos = 0;
      // Buscar en todas las casillas de ambos lados
      List<Casilla> todasLasCasillas = BattleManager.Instance.lCasillasTotal;

      foreach (var casilla in todasLasCasillas)
      {
        Trampa trampa = casilla.GetComponent<Trampa>();
        if (trampa != null && trampa.nombre == "Residuo Energetico")
        {
          cantidadResiduos++;
        }
      }
      // Sumar el daño base por la cantidad de residuos encontrados
      float danioBase =UnityEngine.Random.Range(1, 7);
      if (NIVEL > 1) { danioBase += 1; }
      float danioExtra = 1 + cantidadResiduos;

      float daniofinal = danioBase * danioExtra;


      if (scEstaUnidad.HP_actual > 0)
      {

        scEstaUnidad.RecibirDanio(daniofinal, 8, false, variableUnidad);
        Invoke("ChequearMurio", 1.15f);
      }




      usos--;
      if (usos == 0)
      {
        Destroy(this);
      }
    }
  }

  void ChequearMurio()
  { 
    

        if (scEstaUnidad.HP_actual < 1)
        {
           /////////////////////////////////////////////
          //BUFF ---- Así se aplica un buff/debuff
          Buff buff2 = new Buff();
          buff2.buffNombre = "Energía Absorbida";
          buff2.boolfDebufftBuff = true;
          buff2.DuracionBuffRondas = -1;
          buff2.cantDanioPorcentaje += 10;
          buff2.cantAPMax += 1;
          buff2.AplicarBuff(variableUnidad);
          // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
          Buff buffComponent2 = ComponentCopier.CopyComponent(buff2, variableUnidad.gameObject);

          if (variableUnidad is ClaseCanalizador cana)
          { cana.CambiarEnergia(1); }         
  
         }
      



  }
   


}
