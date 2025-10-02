using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class EscudodeFe : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
    public override void  Awake()
    {
      nombre = "Escudo de Fe";
      IDenClase = 10;
      costoAP = 3;
      costoPM = 2;
      if(NIVEL > 1){costoPM--;}
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      bAfectaObstaculos = false;

      poneTrampas = true;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_EscudodeFe");
      

    }
    public override void ActualizarDescripcion()
    {
         if(NIVEL<2)
       {
        txtDescripcion = "<color=#5dade2><b>Escudo de Fe I</b></color>\n\n"; 
       
        txtDescripcion += "<i>Crea un 'Escudo de Fe' que dura 3 turnos en la casilla objetivo y en las casillas adyacentes.</i>\n";
        txtDescripcion += "<i>'Escudo de Fe:' Dura 3 T. Otorga a la unidad presente en la casilla +3 Barrera y 1 TS por cada Fervor de la Purificadora. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 Costo Val.</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==2)
       {
        txtDescripcion = "<color=#5dade2><b>Escudo de Fe II</b></color>\n\n"; 
       
        txtDescripcion += "<i>Crea un 'Escudo de Fe' que dura 3 turnos en la casilla objetivo y en las casillas adyacentes.</i>\n";
        txtDescripcion += "<i>'Escudo de Fe:' Dura 3 T. Otorga a la unidad presente en la casilla +3 Barrera y 1 TS por cada Fervor de la Purificadora. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM-1} \n</color>\n\n";
    if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
              txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Buff: +1 Defensa</color>\n\n";
          }
          }
        }
       }
       if(NIVEL==3)
       {
         txtDescripcion = "<color=#5dade2><b>Escudo de Fe III</b></color>\n\n"; 
       
        txtDescripcion += "<i>Crea un 'Escudo de Fe' que dura 3 turnos en la casilla objetivo y en las casillas adyacentes.</i>\n";
        txtDescripcion += "<i>'Escudo de Fe:' Dura 3 T. Otorga a la unidad presente en la casilla +3 Barrera y 1 TS por cada Fervor de la Purificadora y +1 Defensa. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM-1} \n</color>\n\n";
   
         if (EsEscenaCampaña())
        {
          if(CampaignManager.Instance.scMenuPersonajes.pSel!= null)
          {
          if(CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
             txtDescripcion += $"<color=#dfea02>-Opción A: +1 Turno Duración. </color>\n";
             txtDescripcion += $"<color=#dfea02>-Opción B: Cura 2d6 cada Turno.</color>\n";
          }
          }
        }
       }
       if(NIVEL==4)
       {
        txtDescripcion = "<color=#5dade2><b>Escudo de Fe IVa</b></color>\n\n"; 
       
        txtDescripcion += "<i>Crea un 'Escudo de Fe' que dura 3 turnos en la casilla objetivo y en las casillas adyacentes.</i>\n";
        txtDescripcion += "<i>'Escudo de Fe:' Dura 4 T. Otorga a la unidad presente en la casilla +3 Barrera y 1 TS por cada Fervor de la Purificadora y +1 Defensa. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM-1} \n</color>\n\n";
    }
       if(NIVEL==5)
       {
         txtDescripcion = "<color=#5dade2><b>Escudo de Fe IVb</b></color>\n\n"; 
       
        txtDescripcion += "<i>Crea un 'Escudo de Fe' que dura 3 turnos en la casilla objetivo y en las casillas adyacentes.</i>\n";
        txtDescripcion += "<i>'Escudo de Fe:' Dura 3 T. Otorga a la unidad presente en la casilla +3 Barrera y 1 TS por cada Fervor de la Purificadora y +1 Defensa y cura 2d6. </i>\n";
        txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM-1} \n</color>\n\n";
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
      casillasAlrededor = cas.ObtenerCasillasAlrededor(1);
      casillasAlrededor.Add(cas); //Agrega la casilla origen


      foreach (Casilla c in casillasAlrededor)
      {
        c.AddComponent<TrampaEscudoFe>();

        int fervorActual = Usuario.GetComponent<ClasePurificadora>().ObtenerFervor();
        c.GetComponent<TrampaEscudoFe>().Inicializar(NIVEL, fervorActual);
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
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(8); //alcance
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
