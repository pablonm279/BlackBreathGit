using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAArqueroVengador : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: �cido - 8: Arcano


    
  void Awake()
   {
      nombre = "Tiro con Arco";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = false;
      hAlcance = 6;
      hCooldownMax = 0;
      esHostil = true;
      prioridad = 0;
      costoAP = 3;
      afectaObstaculos = true;
      


      hActualCooldown = hCooldownMax;

      bonusAtaque = 1;
      XdDanio = 2;
      daniodX = 4; //2d8+3
      tipoDanio = 2; //Perforante


      

    
   }

    void Start()
    {
      prioridad = 0;
    }

  object Objetivo;
   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
     
      scEstaUnidad.ReproducirAnimacionAtaque();

      Objetivo = EstablecerObjetivoPrioritario();
                  PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo

    
      Invoke("CrearProyectil", 0.8f);

     
    
      await BattleManager.DelayCombateAsync(1300);
      //Esto es cuando el objetivo es uno solo,
      AplicarEfectosHabilidad(Objetivo);
     
   }

    void CrearProyectil()
   {
      GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.ViroteBallestadeManoVeneno;
      GameObject Proyectil = Instantiate(flechaPrefab);
      Proyectil.GetComponent<ArrowFlight>().startMarker = transform;
      Proyectil.GetComponent<ArrowFlight>().velocidad = 6.15f;
      Proyectil.GetComponent<ArrowFlight>().parabola = 0.55f;  
    
     
      if (Objetivo != null)
    {

      if (Objetivo is Unidad)
      {
        Unidad obj = (Unidad)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
      }
      else if (Objetivo is Obstaculo)
      {
        Obstaculo obj = (Obstaculo)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
      }
    }
     
   }
    public override void AplicarEfectosHabilidad(object obj)
    {
      if(obj is Unidad)
     {
          Unidad objetivo = (Unidad)obj;
     
     float defensaObjetivo = objetivo.ObtenerdefensaActual();
//     print("Defensa: "+ defensaObjetivo);

     int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;
      //  print("Resultado tirada "+resultadoTirada);



      if (resultadoTirada == -1)
      {//PIFIA 
        print("Pifia");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
        scEstaUnidad.EstablecerAPActualA(0);
      }
      else if (resultadoTirada == 0)
      {//FALLO
        print("Fallo");
        objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
      }
      else if (resultadoTirada == 1)
      {//ROCE
        print("Roce");
        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        danio -= danio / 2; //Reduce 50% por roce

        objetivo.RecibirDanio(danio + 1, tipoDanio, false, scEstaUnidad);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio + 3, tipoDanio, false, scEstaUnidad);

        //Aplicar efecto de veneno
        Aplicarveneno(objetivo);
      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio + 3, tipoDanio, true, scEstaUnidad);
        
        Aplicarveneno(objetivo);

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

    void Aplicarveneno(Unidad objetivo)
    {
     if (objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, 13))
     { 

      objetivo.estado_veneno += 2;

     }
    }
    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun l�gica
   {
    
      // Obtener la unidad duea
    Unidad unidadDuea = gameObject.GetComponent<Unidad>();
    if (unidadDuea == null) return null;
   
    // Filtrar las unidades
     print("Sel objPosibles "+objPosibles.Count);
   
     
    var unidades = objPosibles.OfType<Unidad>().ToList();
    print("Sel obj unidades cant: "+unidades.Count);
    // Filtrar los obstculos
    var obstaculos = objPosibles.OfType<Obstaculo>().ToList();
     print("Sel obj obstaculos cant: "+obstaculos.Count);

    // Ordenar las unidades primero por posX y luego por la diferencia en posY
   
    var unidadesOrdenadas = unidades
        .OrderBy(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDuea.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la primera
    if (unidadesOrdenadas.Any())
    {
       
        return unidadesOrdenadas.FirstOrDefault();
    }else{print("lista unidades vacia");}

    // Si no hay unidades, devolver el obstculo
     if (obstaculos.Any())
     {
       var obstaculo = obstaculos.FirstOrDefault();
      
       return obstaculo;
     }

  
     return null;
   }



}

  
 
  

