using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;

public class BoladeFuego : Habilidad
{
   

   
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de crpitico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     public override void  Awake()
    {
      nombre = "Bola de Fuego";
      costoAP = 1;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      esHostil = true;
      //---
      enArea = 2;
      esforzable = 0;
      targetEspecial = 0;
      //---

      esCargable = true;
      cooldownMax = 0;

      bAfectaObstaculos = true;
      bonusAtaque = 0;
      XdDanio = 3;
      daniodX = 8; //3d8
      tipoDanio = 4; //Fuego
      criticoRangoHab = 0;
      

    
    }

   
    public override void ActualizarDescripcion(){}
    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada,  Casilla casillaObjetivo)
    {
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
     float defensaObjetivo = objetivo.ObtenerdefensaActual();
     print("Defensa: "+ defensaObjetivo);

     int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarFuerza, bonusAtaque, scEstaUnidad.mod_CriticoRangoDado, objetivo,0) ;
     print("Resultado tirada "+resultadoTirada);
     
    
     if(resultadoTirada == -1)
     {//PIFIA 
       print("Pifia");
       //BattleManager.Instance.TerminarTurno(); //Al ser Pifia, termina el turno.
       scEstaUnidad.EstablecerAPActualA(0);
     }
     else if (resultadoTirada == 0)
     {//FALLO
       print("Fallo");

     }
     else if (resultadoTirada == 1)
     {//ROCE
       print("Roce");
       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder;
        danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

       danio -= danio/2; //Reduce 50% por roce

       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


     }
     else if (resultadoTirada == 2)
     {//GOLPE
       print("Golpe");

       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder;
        danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);

       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

     }
     else if (resultadoTirada == 3)
     {//CRITICO
       print("Critico");

       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder;
        danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
      
       objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
     }
     
      objetivo.AplicarDebuffPorAtaquesreiterados(1);
     }
      else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+scEstaUnidad.mod_CarPoder;
        danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
        
        objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }

    }


    public List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();
   
    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(3,2);
    
       foreach(Casilla c in lCasillasafectadas)
      {
       
       c.ActivarCapaColorRojo();
        if(c.Presente == null)
        {
            continue;
        }
        
        if(!bAfectaObstaculos) //Si no afecta obstaculos se descarta la casilla si no hay unidad, si afecta obstaculo se descarta si tampoco hay obstaculo
        {
           if(c.Presente.GetComponent<Unidad>() == null)
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>())
           {
            continue;
           }

           if(c.Presente.GetComponent<Unidad>() != null)
           {
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());;
           }

           if(c.Presente.GetComponent<Obstaculo>() != null)
           {
             lObstaculosPosibles.Add(c.Presente.GetComponent<Obstaculo>());;
           }

        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

    

}
