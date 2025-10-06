using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAChirridoMayor : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

    [SerializeField] private int zonaX; 
    [SerializeField] private  int zonaY;

    [SerializeField] private  int iAlrededor;

   
    
  void Awake()
   {
      nombre = "Chirrido Mayor";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 5;
      esMelee = false;
      hAlcance = 8;
      hCooldownMax = 4;
      esHostil = true;
      prioridad = 4;
      costoAP = 2;
      afectaObstaculos = true;
      
     
      hActualCooldown = UnityEngine.Random.Range(0, 4);


         
      zonaX = 0;
      zonaY = 0;

      iAlrededor = 10;
    
   }

    void Start()
    {
      prioridad = 4;
    }


   public async override Task ActivarHabilidad()
   {
   
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
    hActualCooldown = hCooldownMax;
    scEstaUnidad.ReproducirAnimacionAtaque();


     object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
     List<object> unidadesEnZona = ObtenerUnidadesEnZona(ObtenerAfectadosZonaObjetivo(zonaX, zonaY, Objetivo));
     PrepararInicioAnimacion(unidadesEnZona,null);//Despues de establecer objetivo

    await Task.Delay(2000);
    
    AplicarEfectosEnZona(ObtenerAfectadosZonaObjetivo(zonaX, zonaY, Objetivo)); //aca se determina la zona del Ataque relativo al objetivo
     
   }
    public List<object> ObtenerUnidadesEnZona(List<Casilla> casillas)
    {
      List<object> unidades = new List<object>();
      foreach (Casilla cas in casillas)
      {
        if (cas.Presente != null)
        {
          Unidad unidad = cas.Presente.GetComponent<Unidad>();
          if (unidad != null)
          {
            unidades.Add(unidad);
          }
        }
      }
      return unidades;
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
             
         if(obj.TiradaSalvacion(obj.mod_TSMental, 12))
          {
            /////////////////////////////////////////////
            //BUFF ---- Así se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Aturdido por Chirrido";
            buff.boolfDebufftBuff = false;
            buff.DuracionBuffRondas = 1;
            buff.cantAPMax -= 1;
            buff.cantTsMental -= 2;
            obj.ValentiaP_actual -= 1;
            
            buff.AplicarBuff(obj);

           
            // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, obj.gameObject);

          

          }
          



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

  
 
  