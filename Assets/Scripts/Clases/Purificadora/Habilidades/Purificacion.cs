using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Purificacion : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

   
     public override void  Awake()
    {
      nombre = "Purificación";
      IDenClase = 9;
      costoAP = 6;
      costoPM = 0;
      
      
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 10;
      esforzable = 3;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5; 
      bAfectaObstaculos = false;

      targetEspecial = 0; 

      bonusAtaque +=0; //0
      XdDanio = 1;
      daniodX = 5; 
      tipoDanio = 11; //Divino
     

      imHab = Resources.Load<Sprite>("imHab/Purificadora_Purificacion");
      
     

      
    }

    public override void ActualizarDescripcion()
    {
      if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Purificación I</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando la fe de toda la caravana, lanza un ataque masivo contra todos los enemigos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Daño por Fervor: 4 + Poder + 1d10 - Daño Divino; <color=#ea0606>Tirada Salvación Reflejos: DC:9 + Poder - evita 1/2 daño, si no se salva Arde 2</color>\n";
        txtDescripcion += $"<i>La Purificadora se queda sin Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 Daño por Fervor</color>\n\n";
          }
          }
        }
   
      }
      if(NIVEL== 2)
      {
        txtDescripcion = "<color=#5dade2><b>Purificación II</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando la fe de toda la caravana, lanza un ataque masivo contra todos los enemigos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Daño por Fervor: 5 + Poder + 1d10 - Daño Divino; <color=#ea0606>Tirada Salvación Reflejos: DC:9 + Poder - evita 1/2 daño, si no se salva Arde 2</color>\n";
        txtDescripcion += $"<i>La Purificadora se queda sin Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

  
    
       if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 DC</color>\n\n";
          }
          }
        }
      }
      if(NIVEL== 3)
      {
        txtDescripcion = "<color=#5dade2><b>Purificación III</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando la fe de toda la caravana, lanza un ataque masivo contra todos los enemigos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Daño por Fervor: 5 + Poder + 1d10 - Daño Divino; <color=#ea0606>Tirada Salvación Reflejos: DC:10 + Poder - evita 1/2 daño, si no se salva Arde 2</color>\n";
        txtDescripcion += $"<i>La Purificadora se queda sin Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";


  
      if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +2 arder</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Hace 1/3 daño a todos los otros enemigos. </color>\n";
          }
          }
        }

      }
      if(NIVEL== 4)
      {
        txtDescripcion = "<color=#5dade2><b>Purificación IVa</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando la fe de toda la caravana, lanza un ataque masivo contra todos los enemigos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Daño por Fervor: 5 + Poder + 1d10 - Daño Divino; <color=#ea0606>Tirada Salvación Reflejos: DC:10 + Poder - evita 1/2 daño, si no se salva Arde 4</color>\n";
        txtDescripcion += $"<i>La Purificadora se queda sin Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
      }
      if(NIVEL== 5)
      {
        txtDescripcion = "<color=#5dade2><b>Purificación IVb</b></color>\n\n"; 
        txtDescripcion += "<i>Utilizando la fe de toda la caravana, lanza un ataque masivo contra todos los enemigos.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Daño por Fervor: 5 + Poder + 1d10 - Daño Divino; <color=#ea0606>Tirada Salvación Reflejos: DC:10 + Poder - evita 1/2 daño, si no se salva Arde 2</color>\n";
        txtDescripcion += $"<i>La Purificadora mantiene 1 de Fervor.</i>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
        }



    }

    Casilla Origen;
    public override void Activar()
    { 
        seTiroFlechaVFX = false;
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();
        
      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    
    bool seTiroFlechaVFX = false;
  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {

      Unidad objetivo = (Unidad)obj;
      float dificultadAtributo = 9 + scEstaUnidad.mod_CarPoder;

      VFXAplicar(objetivo.gameObject);


      float damDivino =  2 + UnityEngine.Random.Range(1, 6) + (scEstaUnidad.mod_CarPoder / 2); //Se pone la mitad, si no se salva, se duplica el daño
      ClasePurificadora pur = (ClasePurificadora)scEstaUnidad;
      int fervor = pur.ObtenerFervor();
      print("Se uso fervor: " + pur.ObtenerFervor());
      damDivino = damDivino* (1 + fervor);


      if (objetivo.TiradaSalvacion(objetivo.mod_TSReflejos, dificultadAtributo))
      {
        //Si falla la tirada de salvación, se aplica el daño completo (en vez de la mitad) y arde
        objetivo.estado_ardiendo += 2;
        damDivino = damDivino*2;


      }

      objetivo.RecibirDanio(damDivino, 11, false, scEstaUnidad);

      Invoke("BorrarFervor", 1.5f);

    }
     
  }
  void BorrarFervor()
  {
    ClasePurificadora pur = (ClasePurificadora)scEstaUnidad;
    if(NIVEL == 5)
    {
      pur.CambiarFervor(1);
    }
    else
    {
      pur.CambiarFervor(-100);
    }
    pur.CambiarFervor(-100);

  }

  public override Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
  {



    return base.Resolver(Objetivos, casillaOrigenTrampas);

  }
  
  
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Purificacion");

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
      //Cualquier objetivo
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
      
      lCasillasafectadas = Origen.ObtenerCasillasRango(10,10);
    
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
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
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
