using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IADescargaMaldita : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano


  void Awake()
   {
      nombre = "Descarga Maldita";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = false;
      hAlcance = 4;
      hCooldownMax = 2;
      esHostil = true;
      prioridad = pPrioridad;
      costoAP = 3;
      afectaObstaculos = true;



      hActualCooldown = 0;

      bonusAtaque = 3;
      XdDanio = 2;
      daniodX = 6; //1d6
      tipoDanio = 8; //Arcano
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

      Objetivo = EstablecerObjetivoPrioritario();
      PrepararInicioAnimacion(null,Objetivo);//Despues de establecer objetivo

      Task impactoPendiente = Objetivo != null ? CrearProyectil(Objetivo) : Task.CompletedTask;
      await impactoPendiente;

      if (Objetivo != null)
      {
          AplicarEfectosHabilidad(Objetivo);
      }
   }

   internal Task CrearProyectil(object objetivo)
    {
        if (objetivo == null)
        {
            return Task.CompletedTask;
        }

        return LanzarProyectilAsync(objetivo);
    }

    private async Task LanzarProyectilAsync(object objetivo)
    {
        await BattleManager.DelayCombateAsync(550);

        GameObject proyectilPrefab = BattleManager.Instance.contenedorPrefabs.BrujaKaleTav_DescargaMaldita;
        if (proyectilPrefab == null)
        {
            return;
        }

        GameObject proyectil = Instantiate(proyectilPrefab);
        ArrowFlight flight = proyectil.GetComponent<ArrowFlight>();

        Transform destino = null;
        if (objetivo is Unidad unidad)
        {
            destino = unidad.transform;
        }
        else if (objetivo is Obstaculo obstaculo)
        {
            destino = obstaculo.transform;
        }

        if (flight != null && destino != null)
        {
            flight.Configure(transform, destino, 0.45f, 4.2f);
            await flight.EsperarImpactoAsync();
        }
        else
        {
            await BattleManager.DelayCombateAsync(200);
        }
    }
    public override void AplicarEfectosHabilidad(object obj)
    {

     if(obj is Unidad)
     {


        Unidad objetivo = (Unidad)obj;

        float defensaObjetivo = objetivo.ObtenerdefensaActual();

        int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo) ;



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

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
         objetivo.RecibirDanio(danio, 9, false, scEstaUnidad);


      }
      else if (resultadoTirada == 2)
      {//GOLPE
        print("Golpe");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
       
        objetivo.estado_Condenado = 4; //Aplica 3 stacks de condenado


      }
      else if (resultadoTirada == 3)
      {//CRITICO
        print("Critico");

        float danio = TiradaDeDados.TirarDados(XdDanio, daniodX);
        danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje);

        objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
      
        objetivo.estado_Condenado = 3; //Aplica 2 stacks de condenado

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


    public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
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

  
 
  

