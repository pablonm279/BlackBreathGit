using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class AullidoDeLaManada : IAHabilidad
{
    [SerializeField] public int pPrioridad;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano - 9: Necro
    
    int bonusdadocrit;
  void Awake()
   {
      nombre = "Aullido de la Manada";
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      hAncho = 1;
      esMelee = false;
      hAlcance = 10;
      hCooldownMax = 3;
      esHostil = false;
      prioridad = 7;
      costoAP = 2;
      
      afectaObstaculos = false;
      
      
      


      hActualCooldown = 0;

      
   
    
   }

    void Start()
    {
      prioridad = pPrioridad;
    }


   public async override Task ActivarHabilidad()
   {
    gameObject.GetComponent<Unidad>().CambiarAPActual(-costoAP);
      hActualCooldown = hCooldownMax;
      
      scEstaUnidad.ReproducirAnimacionAtaque();

      List<Casilla> lCasillasafectadas2 = scEstaUnidad.CasillaPosicion.ObtenerCasillasMismoLado();
      List<object> unidadesEnZona = ObtenerUnidadesEnZona(lCasillasafectadas2);
      PrepararInicioAnimacion(unidadesEnZona,null);//Despues de establecer objetivo

      
      await Task.Delay(1500);
      ResolverHabilidad(); 

    
    
     
   }
        void VFXAplicar(GameObject objetivo)
    {
      GameObject VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_AullidoManada");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }

    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
    private void ResolverHabilidad()
    {
     
      lObjetivosPosibles.Clear();

      Casilla Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
   

    
      foreach (Casilla c in lCasillasafectadas)
    {


      if (c.Presente == null)
      {
        continue;
      }

      if (!afectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
      {
        if (c.Presente.GetComponent<Unidad>() == null)
        {
          continue;
        }

        if (c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
        }


      }
      else
      {
        if (c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
        {
          continue;
        }

        if (c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>()); ;
        }

        if (c.Presente.GetComponent<Obstaculo>() != null)
        {
          lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>()); ;
        }

      }

    }
    

     
      foreach(Unidad lobo in lObjetivosPosibles )
      {
        
        if(lobo.GetComponent<ReaccionMuerteLoboEspectral>() != null)
        {
            AplicarEfectosHabilidad(lobo);
        }
      
        

     
      }
      
         
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
    public override void AplicarEfectosHabilidad(object obj)
    {
     if(obj is Unidad)
     {
       Unidad objetivo = (Unidad)obj;
       VFXAplicar(objetivo.gameObject);
       /////////////////////////////////////////////
      //BUFF ---- Así se aplica un buff/debuff
      Buff buff = new Buff();
       buff.buffNombre = "Aullido de la Manada";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantTsMental += 2;
       buff.percCritDaño += 10;
       buff.cantDefensa += 1;
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
     }
     
    }

    public override object EstablecerObjetivoPrioritario() 
    {
               return null; //Esta porque tiene que overridead si o si, no se usa
    }


     public override List<object> ListaHayObjetivosAlAlcance() 
     {

        List<object> lista = new List<object>();
        lista.Add(scEstaUnidad); //Esto es para que siempre haya objetivo y siempre sea usable la habilidad

        return lista;




     }



}
