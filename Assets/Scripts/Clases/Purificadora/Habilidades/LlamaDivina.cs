using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class LlamaDivina : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int criticoRangoHab;//lo que resta al rango de critico del dado (mientras mayor, mas probable)
    [SerializeField] private int tipoDanio; //1: Perforante - 2: Cortante - 3: Contundente - 4: Fuego - 5: Hielo - 6: Rayo - 7: Ácido - 8: Arcano

    private int hAlcance = 5;
    private int hAncho = 1; //1 - Adyacentes

    private int danioFijo;
     public override void  Awake()
    {
      nombre = "Llama Divina";
      costoAP = 3; 
      costoPM = 0;
      IDenClase = 7;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 3; 
      bAfectaObstaculos = true;

      bonusAtaque = 0; //0
      XdDanio = 3;
      daniodX = 6; 
      danioFijo = 4;
      if(NIVEL > 2){danioFijo += 3;}
      tipoDanio = 11; //Divino

      criticoRangoHab = 0;

      requiereRecurso = 0; //esto es para que el boton no se active al apretar si no tiene X recursos (ej Flecha). Ver en BotonHabilidad.

      



     imHab = Resources.Load<Sprite>("imHab/Purificadora_LlamaDivina");
      
  


        
    }

   
     public override void ActualizarDescripcion()
     {
         if(NIVEL<2)
      {
        txtDescripcion = "<color=#5dade2><b>Llama Divina I</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora arroja una llama de fuego divino que daña al objetivo y puede dejarlo ardiendo.</i>\n\n";
        txtDescripcion += $"<i>Daño Divino: 3d6 + 4 +Poder.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 8+Poder: Aplica Ardiendo 3 y mata No-muertos y etéreos instantáneamente.</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

        if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +1 TS</color>\n\n";
          }
          }
        }
   
      }
      if(NIVEL== 2)
      {
        txtDescripcion = "<color=#5dade2><b>Llama Divina II</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora arroja una llama de fuego divino que daña al objetivo y puede dejarlo ardiendo.</i>\n\n";
        txtDescripcion += $"<i>Daño Divino: 3d6 + 4 +Poder.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 9+Poder: Aplica Ardiendo 3 y mata No-muertos y etéreos instantáneamente.</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

  
    
       if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: +3 Daño</color>\n\n";
          }
          }
        }
      }
      if(NIVEL== 3)
      {
        txtDescripcion = "<color=#5dade2><b>Llama Divina III</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora arroja una llama de fuego divino que daña al objetivo y puede dejarlo ardiendo.</i>\n\n";
        txtDescripcion += $"<i>Daño Divino: 3d6 + 7 +Poder.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 9+Poder: Aplica Ardiendo 3 y mata No-muertos y etéreos instantáneamente.</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";

  
      if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: Al matar: da 1 Fervor.</color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: +2 Arder. </color>\n";
          }
          }
        }

      }
      if(NIVEL== 4)
      {
        txtDescripcion = "<color=#5dade2><b>Llama Divina IV a</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora arroja una llama de fuego divino que daña al objetivo y puede dejarlo ardiendo.</i>\n\n";
        txtDescripcion += $"<i>Daño Divino: 3d6 + 7 +Poder. Al matar: da 1 Fervor.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 9+ Poder: Aplica Ardiendo 3 y mata No-muertos y etéreos instantáneamente.</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
      }
      if(NIVEL== 5)
      {
        txtDescripcion = "<color=#5dade2><b>Llama Divina IV b</b></color>\n\n"; 
        txtDescripcion += "<i>La Purificadora arroja una llama de fuego divino que daña al objetivo y puede dejarlo ardiendo.</i>\n\n";
        txtDescripcion += $"<i>Daño Divino: 3d6 + 7 +Poder.</i>\n\n";
        txtDescripcion += $"<color=#c8c8c8>Al impactar - TS Fortitud DC 9+ Poder: Aplica Ardiendo 5 y mata No-muertos y etéreos instantáneamente.</color>\n";
        txtDescripcion += $"<color=#44d3ec>-Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} Esforzable \n- Costo Val: {costoPM} </color>\n\n";
      }





     }
    Casilla Origen;
    public override void Activar()
    {
       
          Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
          ObtenerObjetivos();

        
          BattleManager.Instance.SeleccionandoObjetivo = true;
          BattleManager.Instance.HabilidadActiva = this;
    }
    
      

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaOrigenTrampas = null)
    { 
    
     if(obj is Unidad) //Acá van los efectos a Unidades.
     {
       Unidad objetivo = (Unidad)obj;
       float defensaObjetivo = objetivo.ObtenerdefensaActual();
      
        
      
       CrearProyectil(objetivo);

       await Task.Delay(1300);
       float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
       
      
       int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarPoder, bonusAtaque, criticoRango, objetivo, 0); 
            
     
       if(resultadoTirada == -1)
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
         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;

         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         EfectoAdicional(objetivo);

         danio -= danio/2; //Reduce 50% por roce

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);


       }
       else if (resultadoTirada == 2)
       {//GOLPE
         print("Golpe");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         EfectoAdicional(objetivo);
         

         objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);

       }
       else if (resultadoTirada == 3)
       {//CRITICO
         print("Critico");

         float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;
         danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
         EfectoAdicional(objetivo);
      
         objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
       }
     
        objetivo.AplicarDebuffPorAtaquesreiterados(1);

       
        objVerMurio = objetivo;

        if(NIVEL == 4)
        {Invoke("ChequearMurio", 1.5f);}



       }   
     else if (obj is Obstaculo) //Acá van los efectos a Obstaculos
     {
       Obstaculo objetivo = (Obstaculo)obj;
       //---


       float danio = TiradaDeDados.TirarDados(XdDanio,daniodX)+danioFijo+scEstaUnidad.mod_CarPoder;
       danio = danio/100*(100+scEstaUnidad.mod_DanioPorcentaje);
       
       objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
     }
    }

    Unidad objVerMurio;
    void ChequearMurio()
    {
        if(objVerMurio == null)
        {
          scEstaUnidad.gameObject.GetComponent<ClasePurificadora>().CambiarFervor(1);
          BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} gana 1 Fervor por matar con Llama Divina.");
        }
        else if(objVerMurio.HP_actual < 1)
        {
          scEstaUnidad.gameObject.GetComponent<ClasePurificadora>().CambiarFervor(1);
          BattleManager.Instance.EscribirLog($"{scEstaUnidad.uNombre} gana 1 Fervor por matar con Llama Divina.");
        }
    }
    
    async Task CrearProyectil(object Objetivo)
   {
      await Task.Delay(200);
      GameObject flechaPrefab = BattleManager.Instance.contenedorPrefabs.LlamaDivina;
      GameObject Proyectil = Instantiate(flechaPrefab);
      Proyectil.GetComponent<ArrowFlight>().startMarker = transform;
      Proyectil.GetComponent<ArrowFlight>().parabola = 0.22f;
      Proyectil.GetComponent<ArrowFlight>().velocidad = 5.7f;
    
    
      if(Objetivo != null)
      {
      
        if(Objetivo is Unidad)
        { 
          Unidad obj = (Unidad)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
        }
        else if(Objetivo is Obstaculo)
        {
          Obstaculo obj = (Obstaculo)Objetivo;
        Proyectil.GetComponent<ArrowFlight>().endMarker = obj.transform;
        }
      }
     
   }

    void EfectoAdicional(Unidad objetivo)
    { 

      int DC = (int)(8+scEstaUnidad.mod_CarPoder);

      if(NIVEL > 1){DC++;}
      if(objetivo.TiradaSalvacion(objetivo.mod_TSFortaleza, DC))
     {
       if(objetivo.TieneTag("Nomuerto") || objetivo.TieneTag("Etereo")) // Si los nomuertos no se salvan los mata.
       {
          if(objetivo.HP_actual > 0)
          {
           objetivo.RecibirDanio(objetivo.mod_maxHP+1,10, false, scEstaUnidad); //Muerte instantanea
          }
       }
       else
       {
         objetivo.estado_ardiendo += 3;
         if(NIVEL == 5)
         {
           objetivo.estado_ardiendo += 2 ;
         }
       }


     }



      
    }

    void VFXAplicar(GameObject objetivo)
    {
       //GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation); 

    }
   
    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
      
      
      
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasRango(hAlcance,hAncho);
    
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
