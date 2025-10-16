using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class SalmoPurificador : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
    public override void  Awake()
    {
      nombre = "Salmo Purificador";
      IDenClase = 6;
      costoAP = 3;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 1;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_SalmoPurificador");
     

    }
  public override void ActualizarDescripcion()
  {
    if (NIVEL < 2)
    {
      txtDescripcion = "<color=#5dade2><b>Salmo Purificador I</b></color>\n\n";

      txtDescripcion += "<i>Remueve 1 debuff no ambiental al azar del objetivo y de los aliados en casillas adyacentes (no en diagonal).</i>\n";
      txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: Remueve 1 debuff extra.</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 2)
    {
      txtDescripcion = "<color=#5dade2><b>Salmo Purificador II</b></color>\n\n";

      txtDescripcion += "<i>Remueve 2 debuff no ambientales al azar del objetivo y de los aliados en casillas adyacentes (no en diagonal).</i>\n";
      txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";
      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Próximo Nivel: -1 Cooldown</color>\n\n";
          }
        }
      }
    }
    if (NIVEL == 3)
    {
      txtDescripcion = "<color=#5dade2><b>Salmo Purificador III</b></color>\n\n";

      txtDescripcion += "<i>Remueve 2 debuff no ambientales al azar del objetivo y de los aliados en casillas adyacentes (no en diagonal).</i>\n";
      txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax - 1} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";

      if (EsEscenaCampaña())
      {
        if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
        {
          if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
          {
            txtDescripcion += $"<color=#dfea02>-Opción A: Remueve 1 debuff extra. </color>\n";
            txtDescripcion += $"<color=#dfea02>-Opción B: Da +1 Valentía por debuff removido.</color>\n";
          }
        }
      }
    }
    if (NIVEL == 4)
    {
      txtDescripcion = "<color=#5dade2><b>Salmo Purificador IV a</b></color>\n\n";

      txtDescripcion += "<i>Remueve 3 debuff no ambientales al azar del objetivo y de los aliados en casillas adyacentes (no en diagonal).</i>\n";
      txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax - 1} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";
    }
    if (NIVEL == 5)
    {
      txtDescripcion = "<color=#5dade2><b>Salmo Purificador IV b</b></color>\n\n";

      txtDescripcion += "<i>Remueve 2 debuff no ambientales al azar del objetivo y de los aliados en casillas adyacentes (no en diagonal).</i>\n";
      txtDescripcion += "<i>Otorga 1 Punto de Valentía por cada debuff removido.</i>\n";
      txtDescripcion += $"<color=#44d3ec>- Enfriamiento: {cooldownMax - 1} \n- Costo AP: {costoAP} \n- Costo Val: {costoPM} \n</color>\n\n";
    }
         
        if (TRADU.i.nIdioma == 2) //agrega la traduccion a ingles
        {
          if (NIVEL < 2)
          {
            txtDescripcion = "<color=#5dade2><b>Purifying Psalm I</b></color>\n\n";
            txtDescripcion += "<i>Removes 1 random non-environmental debuff from the target and adjacent allies (not diagonally).</i>\n";
            txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} \n</color>\n\n";
            if (EsEscenaCampaña())
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
              {
                if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                {
                  txtDescripcion += $"<color=#dfea02>-Next Level: Removes 1 extra debuff.</color>\n\n";
                }
              }
            }
          }
          if (NIVEL == 2)
          {
            txtDescripcion = "<color=#5dade2><b>Purifying Psalm II</b></color>\n\n";
            txtDescripcion += "<i>Removes 2 random non-environmental debuffs from the target and adjacent allies (not diagonally).</i>\n";
            txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} \n</color>\n\n";
            if (EsEscenaCampaña())
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
              {
                if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                {
                  txtDescripcion += $"<color=#dfea02>-Next Level: -1 Cooldown</color>\n\n";
                }
              }
            }
          }
          if (NIVEL == 3)
          {
            txtDescripcion = "<color=#5dade2><b>Purifying Psalm III</b></color>\n\n";
            txtDescripcion += "<i>Removes 2 random non-environmental debuffs from the target and adjacent allies (not diagonally).</i>\n";
            txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax - 1} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} \n</color>\n\n";
            if (EsEscenaCampaña())
            {
              if (CampaignManager.Instance.scMenuPersonajes.pSel != null)
              {
                if (CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0)
                {
                  txtDescripcion += $"<color=#dfea02>-Option A: Removes 1 extra debuff. </color>\n";
                  txtDescripcion += $"<color=#dfea02>-Option B: Grants +1 Valor for each debuff removed.</color>\n";
                }
              }
            }
          }
          if (NIVEL == 4)
          {
            txtDescripcion = "<color=#5dade2><b>Purifying Psalm IV a</b></color>\n\n";
            txtDescripcion += "<i>Removes 3 random non-environmental debuffs from the target and adjacent allies (not diagonally).</i>\n";
            txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax - 1} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} \n</color>\n\n";
          }
          if (NIVEL == 5)
          {
            txtDescripcion = "<color=#5dade2><b>Purifying Psalm IV b</b></color>\n\n";
            txtDescripcion += "<i>Removes 2 random non-environmental debuffs from the target and adjacent allies (not diagonally).</i>\n";
            txtDescripcion += "<i>Grants 1 Valor Point for each debuff removed.</i>\n";
            txtDescripcion += $"<color=#44d3ec>- Cooldown: {cooldownMax - 1} \n- AP Cost: {costoAP} \n- Valor Cost: {costoPM} \n</color>\n\n";
          }
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

       List<Unidad> aliadosAdyacentes = new List<Unidad>(); 
       aliadosAdyacentes.Add(objetivo);
      
        foreach(Casilla c in objetivo.CasillaPosicion.ObtenerCasillasAlrededor(1))
        {
          if(c.Presente != null)
          {
            if(c.Presente.GetComponent<Unidad>() != null)
            {
                aliadosAdyacentes.Add(c.Presente.GetComponent<Unidad>());
                print("ADD "+c.Presente.GetComponent<Unidad>().uNombre);
            }
           
          }
        }
        
        foreach(Unidad aliado in aliadosAdyacentes)
        {
          int buffsremover = 1;
          if(NIVEL > 1){buffsremover++;}
          if(NIVEL == 4){buffsremover++;}
          foreach (Buff buff in aliado.GetComponents<Buff>())
          {
            if(buff.esRemovible && !buff.boolfDebufftBuff)
            {
              
              if(buffsremover <= 0)
              {
                break;
              }
               if(NIVEL == 5){aliado.SumarValentia(1);}
              buffsremover--;

              if(buff != null)
              {
                BattleManager.Instance.EscribirLog(scEstaUnidad.uNombre+TRADU.i.Traducir(" remueve ")+buff.buffNombre+TRADU.i.Traducir(" de ")+aliado.uNombre+".");
                buff.RemoverBuff(aliado);
               
              }


            }

          }

           
        }
      











     
  
       
     


       objetivo.Marcar(0);

      
      
     }   
   
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_SalmoPurificador");

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
