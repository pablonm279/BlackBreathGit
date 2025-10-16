using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

public class RetrasarTurno : Habilidad
{
   

    public bool yaRetraso;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     public override void  Awake()
    {
      nombre = "Retrasar";
      costoAP = 0;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      esforzable = 0;
      cooldownMax = 0;
      esHostil = false;

      bonusAtaque = 0;
      XdDanio = 0;
      daniodX = 0; 
      tipoDanio = 0; 


      txtDescripcion = "Retrasa tu acción este turno. Pasas al final de la cola."; 
      if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
      {
          txtDescripcion = "Delay your action this turn. You move to the end of the queue.";
      }

      imHab = Resources.Load<Sprite>("imHab/Retrasar_Turno");

    
    }

    
     public override void ActualizarDescripcion(){}
    Casilla Origen;
    public override void Activar()
    {
      AplicarEfectosHabilidad(scEstaUnidad, 0, null);
      
    }
    
    

    public override void AplicarEfectosHabilidad(object objetivo, int tirada, Casilla nada)
    {
      
      if(!yaRetraso)
      {
        yaRetraso = true;

        
        BattleManager.Instance.TerminarTurno();
        print("Index: "+  BattleManager.Instance.indexTurno--);
        BattleManager.Instance.lUnidadesTotal.Remove(scEstaUnidad);
        BattleManager.Instance.lUnidadesTotal.Add(scEstaUnidad);

        BattleManager.Instance.scUIBarraOrdenTurno.ActualizarBarraOrdenTurno();
        

      }



         

    }


    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();

        

   
}
