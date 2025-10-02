using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Enmendar : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
    public override void  Awake()
    {
      nombre = "Enmendar";
      IDenClase = 3;
      costoAP = 3;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_Enmendar");
      ActualizarDescripcion();

      requiereRecurso = 1; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.
      if(NIVEL == 4){requiereRecurso = 0;}
    }
    public override void ActualizarDescripcion()
    {
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Enmendar I</b></color>\n\n"; 
       
        txtDescripcion += "<i>Realiza Curación de origen mágico a un aliado presente en un radio de 2 casillas.</i>\n";
        txtDescripcion += "<i>Curación: 3d6 +1 por Fervor +1 por Poder. Consume 1 Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Curación mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Curación</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Enmendar II</b></color>\n\n"; 
       
        txtDescripcion += "<i>Realiza Curación de origen mágico a un aliado presente en un radio de 2 casillas.</i>\n";
        txtDescripcion += "<i>Curación: 3d6+1, +1 por Fervor +1 por Poder. Consume 1 Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Curación mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

      
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +2 Curación</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Enmendar III</b></color>\n\n"; 
       
        txtDescripcion += "<i>Realiza Curación de origen mágico a un aliado presente en un radio de 2 casillas.</i>\n";
        txtDescripcion += "<i>Curación: 3d6+3, +1 por Fervor +1 por Poder. Consume 1 Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Curación mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: No consume Fervor </color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: El bonus de Curación por Fervor se triplica</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Enmendar IV a</b></color>\n\n"; 
       
        txtDescripcion += "<i>Realiza Curación de origen mágico a un aliado presente en un radio de 2 casillas.</i>\n";
        txtDescripcion += "<i>Curación: 3d6+3, +1 por Fervor +1 por Poder.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Curación mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";
      }
       if(NIVEL==5)
       {
       txtDescripcion = "<color=#5dade2><b>Enmendar IV b</b></color>\n\n"; 
       
        txtDescripcion += "<i>Realiza Curación de origen mágico a un aliado presente en un radio de 2 casillas..</i>\n";
        txtDescripcion += "<i>Curación: 3d6+3, +3 por Fervor +1 por Poder. Consume 1 Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Curación mágica. Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";
      }



    }
    void Start()
    {
       

    }

    Casilla Origen;
    public override void Activar()
    {
       if(Usuario.GetComponent<ClasePurificadora>().ObtenerFervor() > 0)
       {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());       }
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       
      
       Unidad objetivo = (Unidad)obj;
       VFXAplicar(objetivo.gameObject);
      
       BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} usa {nombre} en {objetivo.uNombre}");
  
       int random = UnityEngine.Random.Range(4, 19);
       float curacion = random+scEstaUnidad.mod_CarPoder+Usuario.GetComponent<ClasePurificadora>().ObtenerFervor();
       if(NIVEL > 1){curacion++;}
       if(NIVEL > 2){curacion+= 2;}
       if(NIVEL > 5){curacion+= Usuario.GetComponent<ClasePurificadora>().ObtenerFervor()*2;}
       
     
       objetivo.RecibirCuracion(curacion, true);

       if(NIVEL != 4){  Usuario.GetComponent<ClasePurificadora>().CambiarFervor(-1);}
     


       objetivo.Marcar(0);

      
      
     }   
   
    }
    bool ChequearSiHayAliadoAdelantado(Unidad obj)
    {
      int casX = Origen.posX;

      foreach(Casilla cas in BattleManager.Instance.lCasillasTotal)
      {
        if(cas.lado != Origen.lado){ continue;} //Si es del lado opuesto la descarta
        if(cas.posX <= Origen.posX){ continue;} //Si esta en la misma culomna o una mas atras la descarta

        if(cas.Presente != null)
        {
            if(cas.Presente.GetComponent<Unidad>() != null)
            {
               if(cas.Presente.GetComponent<Unidad>() != obj) //Si hay una unidad, y no es el objetivo de la habilidad, entonces devuelve SI
               {
                    return true;
               }

            }

        }
        

      }

      return false;
    }
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Enmendar");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   canvasObjeto.overrideSorting = true;
   canvasObjeto.sortingOrder =  200;  

    }

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();
     
      
      //Casillas Alrededor al origen
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4);
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
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
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
           }
          

        }
        else
        {
           if(c.Presente.GetComponent<Unidad>() == null && c.Presente.GetComponent<Obstaculo>() == null)
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

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
    

 
}
