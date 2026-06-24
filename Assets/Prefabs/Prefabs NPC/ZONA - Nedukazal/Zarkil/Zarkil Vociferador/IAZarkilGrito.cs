using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class IAZarkilGrito : IAHabilidad
{

    [SerializeField] public int pPrioridad;

    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Cortante - 2: Perforante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: �cido - 8: Arcano

    [SerializeField] private int zonaX; 
    [SerializeField] private  int zonaY;

    [SerializeField] private  int iAlrededor;

   
    
  void Awake()
   {
      nombre = "Grito de batalla Zarkil";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 5;
      esMelee = false;
      hAlcance = 8;
      hCooldownMax = 5;
      esHostil = true;
      prioridad = 10;
      costoAP = 5;
      afectaObstaculos = false;
      
     
      hActualCooldown = 0;


         
      zonaX = 0;
      zonaY = 0;

      iAlrededor = 15;
    
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

    
    VFXAplicar(gameObject);
    object Objetivo = EstablecerObjetivoPrioritario(); //Esto es cuando el objetivo es uno solo,
    List<object> unidadesEnZona = ObtenerUnidadesEnZona(ObtenerAfectadosZonaObjetivo(zonaX, zonaY, Objetivo));
    PrepararInicioAnimacion(unidadesEnZona, null);//Despues de establecer objetivo

    await BattleManager.DelayCombateAsync(1000);

    AplicarEfectosEnZonaEnemiga();
    AplicarEfectosEnZonaAliada();

    scEstaUnidad.gameObject.GetComponent<IAUnidad>().esRango = false;
    

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

  public void AplicarEfectosEnZonaAliada()
  { 
    List<Casilla> casillasAliadas = new List<Casilla>();
    casillasAliadas = scEstaUnidad.CasillaPosicion.ObtenerCasillasMismoLado();


    foreach (Casilla cas in casillasAliadas)
    {
      if (cas.Presente != null)
      {
        if (cas.Presente.GetComponent<Unidad>() != null)
        {
          Unidad obj = cas.Presente.GetComponent<Unidad>();

          if (obj.TieneTag("Zarkil") && obj != scEstaUnidad)
          {

            // BUFF ---- As� se aplica un buff/debuff
            Buff buff = new Buff();
            buff.buffNombre = "Orden Recibida";
            buff.boolfDebufftBuff = true;
            buff.DuracionBuffRondas = 1;
            buff.cantAPMax += 2;
            buff.AplicarBuff(obj);
            // Agrega el componente Buff al objeto objetivo y asigna la configuraci�n del buff
            Buff buffComponent = ComponentCopier.CopyComponent(buff, obj.gameObject);
          }
          
        }
        
      }
    }




  }
 public void AplicarEfectosEnZonaEnemiga()
  { 
    List<Casilla> casillasAliadas = new List<Casilla>();
    casillasAliadas = scEstaUnidad.CasillaPosicion.ObtenerCasillasLadoOpuesto();


    foreach (Casilla cas in casillasAliadas)
    {
      if (cas.Presente != null)
      {
        if (cas.Presente.GetComponent<Unidad>() != null)
        {
          Unidad obj = cas.Presente.GetComponent<Unidad>();

          if (obj.TiradaSalvacion(3, 9))
          {

            obj.estado_aturdido += 1;
            obj.GenerarTextoFlotante("Aturdido", Color.red);

          }

        }
      }

    }


  }

  void VFXAplicar(GameObject objetivo)
  {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_VociferadorZarkilGrito");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

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
       

      if (obj.TiradaSalvacion(3, 10))
      {

        obj.estado_aturdido += 1;
        obj.GenerarTextoFlotante("Aturdido", Color.red);


      }
          



     }
   
    }

    


   public override object EstablecerObjetivoPrioritario() //Cuando hay 1 solo objetivo posible para la habilidad, determinar a cual prioritiza segun l�gica
   {
    
  

    // Obtener la unidad duea
    Unidad unidadDuea = gameObject.GetComponent<Unidad>();
    if (unidadDuea == null) return null;

    // Filtrar las unidades
    var unidades = objPosibles.OfType<Unidad>().ToList();
    // Filtrar los obstculos
    var obstaculos = objPosibles.OfType<Obstaculo>().ToList();

    // Ordenar las unidades primero por posX y luego por la diferencia en posY
    var unidadesOrdenadas = unidades
        .OrderBy(unidad => unidad.CasillaPosicion.posX)
        .ThenBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - unidadDuea.CasillaPosicion.posY))
        .ToList();

    // Si hay unidades disponibles, devolver la primera
    if (unidadesOrdenadas.Any())
    {
        return unidadesOrdenadas.FirstOrDefault();
    }

    // Si no hay unidades, devolver el obstculo
    var obstaculo = obstaculos.FirstOrDefault();
    return obstaculo;


    
   }



}

  
 
  


