using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class BombaDeHumo : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
     public override void  Awake()
    {
      nombre = "Bomba de Humo";
      IDenClase = 5;
      costoAP = 2;
      if(NIVEL > 2){costoAP--;}
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 5;
      if(NIVEL > 1){cooldownMax--;}
      bAfectaObstaculos = false;

      poneTrampas = true;
      


      imHab = Resources.Load<Sprite>("imHab/Acechador_BombaDeHumo");


    }
    public override void ActualizarDescripcion()
    {
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Bomba de Humo I</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja una Bomba de Humo que esconde a los aliados en la casilla y adyacentes y les otorga bonificaciones de ataque.</i>\n";
        txtDescripcion += "<i>el Humo creado dura 2 Turnos. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 Cooldown.</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
       txtDescripcion = "<color=#5dade2><b>Bomba de Humo II</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja una Bomba de Humo que esconde a los aliados en la casilla y adyacentes y les otorga bonificaciones de ataque.</i>\n";
        txtDescripcion += "<i>el Humo creado dura 2 Turnos. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";
    if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 AP</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
        txtDescripcion = "<color=#5dade2><b>Bomba de Humo III</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja una Bomba de Humo que esconde a los aliados en la casilla y adyacentes y les otorga bonificaciones de ataque.</i>\n";
        txtDescripcion += "<i>el Humo creado dura 2 Turnos. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP-1} \n- Costo Val: {costoPM} \n</color>\n\n";
 
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Turno Duración. </color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Agranda la zona.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
          txtDescripcion = "<color=#5dade2><b>Bomba de Humo IVa</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja una Bomba de Humo que esconde a los aliados en la casilla y adyacentes y les otorga bonificaciones de ataque.</i>\n";
        txtDescripcion += "<i>el Humo creado dura 3 Turnos. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP-1} \n- Costo Val: {costoPM} \n</color>\n\n";
       }
       if(NIVEL==5)
       {
          txtDescripcion = "<color=#5dade2><b>Bomba de Humo IVb</b></color>\n\n"; 

        txtDescripcion += "<i>Arroja una Bomba de Humo que esconde a los aliados en la casilla y alrededor y les otorga bonificaciones de ataque.</i>\n";
        txtDescripcion += "<i>el Humo creado dura 2 Turnos. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax-1} \n- Costo AP: {costoAP-1} \n- Costo Val: {costoPM} \n</color>\n\n";
       }



    }
    void Start()
    {
       

    }

    Casilla Origen;
    public override void Activar()
    {
      
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

    BattleManager.Instance.scUIContadorAP.MarcarCirculos((int)scEstaUnidad.ObtenerAPActual());       
        
    }



  public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
  {
    if (cas == null)
    {
      if (obj is Unidad) //Acá van los efectos a Unidades.
      {
        Unidad objetivo = (Unidad)obj;
        
        cas = objetivo.GetComponent<Unidad>().CasillaPosicion; //Si no se pasa una casilla, se usa la del origen
      }
    }
      List<Casilla> casillasAlrededor = new List<Casilla>();
      int alre = 1;
      if(NIVEL == 5){alre = 2;} //Aumenta el alcance de las casillas alrededor a 2 si es nivel 5
      casillasAlrededor = cas.ObtenerCasillasAlrededor(alre);
      casillasAlrededor.Add(cas); //Agrega la casilla origen


      foreach (Casilla c in casillasAlrededor)
      {
        c.AddComponent<TrampaBombaHumo>();

        c.GetComponent<TrampaBombaHumo>().Inicializar(NIVEL);
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
      
      lObjetivosPosibles.Clear();
      lCasillasafectadas.Clear();
     
      
      //Casillas Alrededor al origen
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4); //alcance
      lCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in lCasillasafectadas)
      {
         c.ActivarCapaColorAzul();
        if(c.Presente == null)
        {
            continue;
        }
        
      
        if(c.Presente.GetComponent<Unidad>() == null)
        {
        continue;
        }
          if(c.Presente.GetComponent<Unidad>() != null)
        {
          lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    
        



      
         
    }

   
    

 
}
