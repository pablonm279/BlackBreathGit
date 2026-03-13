using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAImprovisarTrampas : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


    
  void Awake()
   {
      nombre = "Improvisar Trampas";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 5;
      esMelee = false;
      hAlcance = 5;
      hCooldownMax = 4;
      esHostil = true;
      prioridad = 7;
      costoAP = 1;
      afectaObstaculos = false;
      


      hActualCooldown = UnityEngine.Random.Range(0,hCooldownMax);


    
   }

    void Start()
    {
      prioridad = 7;
    }

  object Objetivo;
   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
     
      scEstaUnidad.ReproducirAnimacionAtaque();

     
    
     hActualCooldown = hCooldownMax;
     
    
      await BattleManager.DelayCombateAsync(1300);
      //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(scEstaUnidad);
     
   }

  public GameObject VFXEstadoPrefab;
    public override void AplicarEfectosHabilidad(object obj)
    {
   
     LadoManager ladoPC = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();


    List<Casilla> casillasDisponibles = ladoPC.casillasLado.Where(c => c.Presente == null && c.GetComponent<Trampa>() == null).OrderBy(c => Guid.NewGuid()).Take(2).ToList();

    if (casillasDisponibles.Count >= 2)
    {
      TrampaImprovisadaTribal tr1 = casillasDisponibles[0].gameObject.AddComponent<TrampaImprovisadaTribal>();
      tr1.Inicializar();
      tr1.AsignarCreador(scEstaUnidad);

      TrampaImprovisadaTribal tr2 = casillasDisponibles[1].gameObject.AddComponent<TrampaImprovisadaTribal>();
      tr2.Inicializar();
      tr2.AsignarCreador(scEstaUnidad);
    }
    
    
    
    }

   
    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
   {
     return null;
   }



}

  
 
  


