using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class CrecimientoEspinoso : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Crecimiento Espinoso";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 5;
      esMelee = false;
      hAlcance = 6;
      hCooldownMax = 3;
      esHostil = true;
      prioridad = pPrioridad;
      costoAP = 3;
      afectaObstaculos = false;
      


      hActualCooldown = 0;


    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }

  object Objetivo;
   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
     
      scEstaUnidad.ReproducirAnimacionAtaque();

     
    
     hActualCooldown = hCooldownMax;
     
    
      await Task.Delay(1300);
      //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(scEstaUnidad);
     
   }

  public GameObject VFXEstadoPrefab;
    public override void AplicarEfectosHabilidad(object obj)
    {
   
     LadoManager ladoPC = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();
     for (int i = 0; i < 2; i++)
     {
        int filaAfectada = UnityEngine.Random.Range(1,5);
        
        if(filaAfectada == 1)
        {
        if(ladoPC.c1x1.GetComponent<TrampaEnredaderaEspinosa>() == null)
        { 
          TrampaEnredaderaEspinosa tr1 = ladoPC.c1x1.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
          tr1.Inicializar();
          TrampaEnredaderaEspinosa tr2 = ladoPC.c2x1.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
          tr2.Inicializar();
          TrampaEnredaderaEspinosa tr3 = ladoPC.c3x1.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
          tr3.Inicializar();
        }
        else{ filaAfectada--;}
        }
        if(filaAfectada == 2)
        {
        if(ladoPC.c1x2.GetComponent<TrampaEnredaderaEspinosa>() == null)
        {
          TrampaEnredaderaEspinosa tr1 = ladoPC.c1x2.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
          tr1.Inicializar();
          TrampaEnredaderaEspinosa tr2 = ladoPC.c2x2.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
          tr2.Inicializar();
          TrampaEnredaderaEspinosa tr3 = ladoPC.c3x2.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
          tr3.Inicializar();
        }else{ filaAfectada++;}
        }
      if(filaAfectada == 3)
        {
          if(ladoPC.c1x3.GetComponent<TrampaEnredaderaEspinosa>() == null)
          {
            TrampaEnredaderaEspinosa tr1 = ladoPC.c1x3.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr1.Inicializar();
            TrampaEnredaderaEspinosa tr2 = ladoPC.c2x3.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr2.Inicializar();
            TrampaEnredaderaEspinosa tr3 = ladoPC.c3x3.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr3.Inicializar();
          }else{ filaAfectada++;}
        }
        if(filaAfectada == 4)
        {
          if(ladoPC.c1x4.GetComponent<TrampaEnredaderaEspinosa>() == null)
          {
            TrampaEnredaderaEspinosa tr1 = ladoPC.c1x4.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr1.Inicializar();
            TrampaEnredaderaEspinosa tr2 = ladoPC.c2x4.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr2.Inicializar();
            TrampaEnredaderaEspinosa tr3 = ladoPC.c3x4.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr3.Inicializar();
          }else{ filaAfectada++;}
        }
        if(filaAfectada == 5)
        {
          if(ladoPC.c1x5.GetComponent<TrampaEnredaderaEspinosa>() == null)
          {
            TrampaEnredaderaEspinosa tr1 = ladoPC.c1x5.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr1.Inicializar();
            TrampaEnredaderaEspinosa tr2 = ladoPC.c2x5.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr2.Inicializar();
            TrampaEnredaderaEspinosa tr3 = ladoPC.c3x5.gameObject.AddComponent<TrampaEnredaderaEspinosa>();
            tr3.Inicializar();
          }
        }
     }
    }

   
    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
   {
     return null;
   }



}

  
 
  