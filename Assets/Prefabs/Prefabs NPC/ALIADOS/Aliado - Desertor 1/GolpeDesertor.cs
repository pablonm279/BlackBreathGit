using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class GolpeDesertor : IAHabilidad
{

    [SerializeField] public int pPrioridad;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano - 9: Necro
   
  void Awake()
   {
      nombre = "Ataque de Espada";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = true;
      hAlcance = 2;
      hCooldownMax = 0;
      esHostil = true;
      prioridad = 1;
      costoAP = 2;
      afectaObstaculos = true;
      


      hActualCooldown = hCooldownMax;

      bonusAtaque = 0;
      XdDanio = 1;
      daniodX = 10; //1d10
      tipoDanio = 2; //Cortante


   
    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }


  public async override Task ActivarHabilidad()
  {
    object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,

    PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo


    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;

    scEstaUnidad.ReproducirAnimacionAtaque();
    await Task.Delay(700);
    AplicarEfectosHabilidad(Objetivo);
      
   

     
   }

    
    public override void AplicarEfectosHabilidad(object obj)
    {
     if(obj is Unidad)
     {
          Unidad objetivo = (Unidad)obj;
          float defensaObjetivo = objetivo.ObtenerdefensaActual();

          int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;
          
        

          if(resultadoTirada == -1)
          {//PIFIA 
        //    print("Pifia");
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
            //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
            
          }
          else if (resultadoTirada == 0)
          {//FALLO
           // print("Fallo");

            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);

          }
          else if (resultadoTirada == 1)
          {//ROCE
         //   print("Roce");
            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
             danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

            danio -= danio/2; //Reduce 50% por roce

            objetivo.RecibirDanio(danio, tipoDanio, false,  scEstaUnidad);

          }
          else if (resultadoTirada == 2)
          {//GOLPE
         //   print("Golpe");

            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
             danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);


            objetivo.RecibirDanio(danio, tipoDanio, false,  scEstaUnidad);
            

          }
          else if (resultadoTirada == 3)
          {//CRITICO
          //  print("Critico");

            float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
            danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

            
            
            
            objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
            
          }
          
            objetivo.AplicarDebuffPorAtaquesreiterados(1);
     }
     else if(obj is Obstaculo)
     {
          Obstaculo objetivo = (Obstaculo)obj;
          
          float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
           danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
          objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

public override object EstablecerObjetivoPrioritario() 
{
    // Obtener la unidad dueña
    Unidad unidadDueña = gameObject.GetComponent<Unidad>();
    if (unidadDueña == null) return null;

    // Filtrar las unidades
    var unidades = objPosibles.OfType<Unidad>().ToList();
    // Filtrar los obstáculos
    var obstaculos = objPosibles.OfType<Obstaculo>().ToList();

    // Ordenar las unidades primero por posX y luego por la diferencia en posY
    var unidadesOrdenadas = unidades
        .OrderByDescending(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDueña.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la primera
    if (unidadesOrdenadas.Any())
    {
        return unidadesOrdenadas.FirstOrDefault();
    }

    // Si no hay unidades, devolver el obstáculo
    var obstaculo = obstaculos.FirstOrDefault();
    return obstaculo;
}


 public GameObject VFXEstadoPrefab;


}

  
 
  