using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class Distraer : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano
     ClaseAcechador claseAcechador;
      public override void  Awake()
    {


      nombre = "Distraer";
      costoAP = 1; 
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      claseAcechador = scEstaUnidad as ClaseAcechador;
      IDenClase = 7;
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 2;  
      bAfectaObstaculos = false;

      esDiscreta = true; //No quita sigilo

      bonusAtaque = 0;
    
      XdDanio = 0;
      daniodX = 0; 
      tipoDanio = 0; 
      criticoRangoHab = 0;


      imHab = Resources.Load<Sprite>("imHab/Acechador_Distraer");

       
    }
    
   void Start()
   {
  
   }

   public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Distraer I</b></color>\n\n";
      txtDescripcion += "<i>Distrae con un destello arrojadizo a un enemigo, si está aislado y no se salva, el personaje se Esconde.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Distraído: -2 AP, -2 Defensa,<color=#ea0606>Tirada Salvación Mental: DC:12 </color></color>\n";
      txtDescripcion += "<i><color=#44d3ec>Si el enemigo no tiene aliados alrededor, el Acechador obtiene Escondido I.</color></i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Discreta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 DC.</color>\n\n";
          }
        }
      }

    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Distraer II</b></color>\n\n";
      txtDescripcion += "<i>Distrae con un destello arrojadizo a un enemigo, si está aislado y no se salva, el personaje se Esconde.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Distraído: -2 AP, -2 Defensa,<color=#ea0606>Tirada Salvación Mental: DC:13 </color></color>\n";
      txtDescripcion += "<i><color=#44d3ec>Si el enemigo no tiene aliados alrededor, el Acechador obtiene Escondido I.</color></i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Discreta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 AP por Distraído.</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Distraer III</b></color>\n\n";
      txtDescripcion += "<i>Distrae con un destello arrojadizo a un enemigo, si está aislado y no se salva, el personaje se Esconde.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Distraído: -3 AP, -2 Defensa,<color=#ea0606>Tirada Salvación Mental: DC:13 </color></color>\n";
      txtDescripcion += "<i><color=#44d3ec>Si el enemigo no tiene aliados alrededor, el Acechador obtiene Escondido I.</color></i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Discreta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: -1 AP, -1 Defensa por Distraído.</color>\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: El Acechador obtiene Escondido II.</color>\n";
          }
        }
      }

    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Distraer IVa</b></color>\n\n";
      txtDescripcion += "<i>Distrae con un destello arrojadizo a un enemigo, si está aislado y no se salva, el personaje se Esconde.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Distraído: -4 AP, -3 Defensa,<color=#ea0606>Tirada Salvación Mental: DC:13 </color></color>\n";
      txtDescripcion += "<i><color=#44d3ec>Si el enemigo no tiene aliados alrededor, el Acechador obtiene Escondido I.</color></i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Discreta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Distraer IVb</b></color>\n\n";
      txtDescripcion += "<i>Distrae con un destello arrojadizo a un enemigo, si está aislado y no se salva, el personaje se Esconde.</i>\n\n";
      txtDescripcion += $"<color=#c8c8c8>Distraído: -3 AP, -2 Defensa,<color=#ea0606>Tirada Salvación Mental: DC:13 </color></color>\n";
      txtDescripcion += "<i><color=#44d3ec>Si el enemigo no tiene aliados alrededor, el Acechador obtiene Escondido II.</color></i>\n\n";
      txtDescripcion += $"<color=#44d3ec>-Discreta -Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} </color>\n\n";
    }
   
   
   
 
  }

  int damExtra;
      Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
  {

    if (obj is Unidad) //Acá van los efectos a Unidades.
    {
      int DC = 12;
      if (NIVEL > 1) { DC++; }
      Unidad objetivo = (Unidad)obj;
      VFXAplicar(objetivo.gameObject);
      if (objetivo.TiradaSalvacion(objetivo.mod_TSMental, DC))
      {

        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Distraído";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 1;
        buff.cantAPMax -= 2;
        buff.cantDefensa -= 2;
        if (NIVEL > 2) { buff.cantAPMax--; }
        if (NIVEL == 4) { buff.cantAPMax--; buff.cantDefensa--; }
        

        buff.AplicarBuff(objetivo);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        if (objetivo.ChequearEstaAislado(2))
        {
          if (NIVEL < 5) { scEstaUnidad.GanarEscondido(1); }
          if (NIVEL == 5) {  scEstaUnidad.GanarEscondido(2);}
         
        }


      }


    }
  }
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Distraer");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
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
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      //Melee - Si está en columna 3 de su lado, aumenta el rango ignorando cada columna vacia del lado opuesto
      int rangoPlus = 0;
   
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(5,1);
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
       
       c.ActivarCapaColorRojo();
       if(esMelee)//Si hab es melee, activa capa roja, de columna al alcance final, no de las otras también
       {
         if(c.transform.GetChild(2).gameObject.activeInHierarchy){ c.DesactivarCapaColorRojo();}
       } 



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

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
 
}
