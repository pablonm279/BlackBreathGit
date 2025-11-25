using System.Collections;
using System.Collections.Generic;
//using UnityEditor.SearchService;
using UnityEngine;
using System.Threading.Tasks;

public class ReaccionEscurridizo : Reaccion
{
  
   void Start()
   {

    TipoTrigger =1;
    usos = 10;
    permanente = false;
    scEstaUnidad = gameObject.GetComponent<Unidad>();
    

    if (TRADU.i.nIdioma == 1)
    {
      descripcion = $"Reacción: Se moverá a una casilla adyacente al esquivar un ataque.";
    }
    if (TRADU.i.nIdioma == 2)
    {
      descripcion = $"Reaction: Will move to an adjacent square when dodging an attack.";
    }

   }

  public async override void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
  {

    List<Casilla> casillasAdyacentesLibres = new List<Casilla>();
    Casilla casillaActual = scEstaUnidad.CasillaPosicion;
    foreach (Casilla c in casillaActual.ObtenerCasillasAlrededor(1))
    {
      if (c.Presente == null)
      {
        casillasAdyacentesLibres.Add(c);
      }
    }
    if (casillasAdyacentesLibres.Count > 0)
    {
      int indiceAleatorio = Random.Range(0, casillasAdyacentesLibres.Count);
      Casilla casillaDestino = casillasAdyacentesLibres[indiceAleatorio];
      await Task.Delay(200); //pequeña espera para que no se vea tan brusco el movimiento
      scEstaUnidad.CasillaForzadoaMover = casillaDestino;
    }
        
    // BUFF ---- Así se aplica un buff/debuff
    Buff buff = new Buff();
    buff.buffNombre = "Agazapado";
    buff.boolfDebufftBuff = true;
    buff.DuracionBuffRondas = 1;
    buff.cantAtaque += 2;
    buff.cantDanioPorcentaje += 10;
    buff.esStackeable = false;
    buff.AplicarBuff(scEstaUnidad);
    // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
    Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);

      
    }


}

