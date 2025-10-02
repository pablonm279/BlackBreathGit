using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IABolaDeFuego : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   [SerializeField] private int zonaX; 
   [SerializeField] private  int zonaY;

   [SerializeField] private  int iAlrededor;


  void Awake()
   {
      nombre = "IA Bola de Fuego";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 3;
      esMelee = false;
      hAlcance = 5;
      hCooldownMax = 1;
      esHostil = true;
      prioridad = pPrioridad;
      costoAP = 2;
      afectaObstaculos = true;
      


      hActualCooldown = hCooldownMax;

      bonusAtaque = 50; //pega si o si
      XdDanio = 2;
      daniodX = 10; //1d8
      tipoDanio = 4; //Contundente

      zonaX = 0;
      zonaY = 0;

      iAlrededor = 2;

    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }


   public async override Task ActivarHabilidad()
   {
   
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
    
    scEstaUnidad.ReproducirAnimacionHabilidadNoHostil();
    await Task.Delay(2000);
    object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
    
    AplicarEfectosEnZona(ObtenerAfectadosZonaObjetivo(zonaX, zonaY, Objetivo)); //aca se determina la zona del Ataque relativo al objetivo
     
   }


    public void AplicarEfectosEnZona(List<Casilla> casillas)
    {
     
     foreach(Casilla cas in casillas)
     {
        //-----
        //Acá aplicar efectos visuales a la casilla si corresponde
        //-----
       

      if(cas.Presente != null)
      {
        if(cas.Presente.GetComponent<Unidad>() != null)
        {
            Unidad obj = cas.Presente.GetComponent<Unidad>();
            AplicarEfectosHabilidad(obj);
        }
        if(afectaObstaculos && cas.Presente.GetComponent<Obstaculo>() != null)
        {
            Obstaculo obj = cas.Presente.GetComponent<Obstaculo>();
            AplicarEfectosHabilidad(obj);
        }
     }
       
    }

    }

   
    private List<Casilla> ObtenerAfectadosZonaObjetivo(int zonax, int zonay, object Objetivo) //Obtiene las casillas a una determinada distancia del objetivo en X o/y en Y
    {
          
        
          Casilla casOrigen = null;
         List<Casilla> todasCasillas = BattleManager.Instance.lCasillasTotal;
          if(Objetivo is Unidad)
          {
              Unidad obj = (Unidad)Objetivo;
              casOrigen = obj.GetComponent<Unidad>().CasillaPosicion;
          }
          if(Objetivo is Obstaculo)
          {
              Obstaculo obj = (Obstaculo)Objetivo;
              casOrigen = obj.GetComponent<Obstaculo>().CasillaPosicion;
          }

          List<Casilla> casillasEnZona = new List<Casilla>();
          casillasEnZona.Add(casOrigen);

         foreach(Casilla cas in todasCasillas)
         {
            if(esHostil)
            { 
              if(cas.lado != casOrigen.lado){continue;}
            }
            else
            {
              if(cas.lado == casOrigen.lado){continue;}
            }
         
            
            //ESTE ES PARA DISTANCIA alrededor de la casilla del objetivo
            casOrigen.CalcularDistanciaACasilla(cas, out int yVert, out int xHor, out bool f);
           
            if(( Math.Abs(yVert)+Math.Abs(xHor) ) <= iAlrededor) //distancia 1: en cruz, etc.
            {
             
               casillasEnZona.Add(cas); 
            }
           

         }

         return casillasEnZona;
    }




     public override void AplicarEfectosHabilidad(object objetivo)
    {
     if(objetivo is Unidad)
     {
       Unidad obj = (Unidad)objetivo;
          float defensaObjetivo = obj.ObtenerdefensaActual();

          int resultadoTirada = TiradaAtaque(defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, obj) ;
     
         if (resultadoTirada < 3 ) //Se subscriben alternativas menores (fallo, pifia) ya que es en zona pega si o si, puede ser crítico
         {
           float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
            danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

           obj.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

         }
         else if (resultadoTirada >= 3)
         {
           float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
            danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
           obj.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
         }
     }
     else if(objetivo is Obstaculo)
     {
          Obstaculo obj = (Obstaculo)objetivo;
          
          float danio = TiradaDeDados.TirarDados(XdDanio,daniodX);
           danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         
           obj.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

    


   public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun lógica
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
        .OrderBy(unidad => unidad.CasillaPosicion.posX)
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



}

  
 
  
