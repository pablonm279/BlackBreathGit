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
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

    int debuffsPorUnidad = 1;
    if (NIVEL > 1) { debuffsPorUnidad += 1; }
    if (NIVEL == 4) { debuffsPorUnidad += 1; }
    bool daValentia = NIVEL == 5;

    string tituloEs = "Salmo Purificador I";
    string tituloEn = "Purifying Psalm I";
    if (NIVEL == 2) { tituloEs = "Salmo Purificador II"; tituloEn = "Purifying Psalm II"; }
    if (NIVEL == 3) { tituloEs = "Salmo Purificador III"; tituloEn = "Purifying Psalm III"; }
    if (NIVEL == 4) { tituloEs = "Salmo Purificador IV a"; tituloEn = "Purifying Psalm IV a"; }
    if (NIVEL == 5) { tituloEs = "Salmo Purificador IV b"; tituloEn = "Purifying Psalm IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Ranged (4 range)\n";
      cuerpo += "<b>Target:</b> 1 unit in range\n";
      cuerpo += "<b>Area:</b> Target + adjacent units\n";
      cuerpo += $"<b>Effect:</b> Removes up to {debuffsPorUnidad} removable Debuffs from each affected unit\n";
      if (daValentia)
      {
        cuerpo += "<b>IV b Extra:</b> +1 Valour to each affected unit per removed Debuff\n";
      }
      cuerpo += "<b>Requirement:</b> Needs at least 1 Fervor to activate\n";
      cuerpo += "<b>On cast:</b> Does not consume Fervor";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Rango (4 alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 unidad en rango\n";
      cuerpo += "<b>Area:</b> Objetivo + unidades adyacentes\n";
      cuerpo += $"<b>Efecto:</b> Remueve hasta {debuffsPorUnidad} Debuffs removibles de cada unidad afectada\n";
      if (daValentia)
      {
        cuerpo += "<b>Extra IV b:</b> +1 Valentía a cada unidad afectada por cada Debuff removido\n";
      }
      cuerpo += "<b>Requisito:</b> Necesita al menos 1 Fervor para activarse\n";
      cuerpo += "<b>Al lanzar:</b> No consume Fervor";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "A cleansing chant that removes hostile effects from a small cluster."
        : "Un canto de limpieza que remueve efectos negativos en un pequeno grupo.",
      cuerpo,
      costos,
      "#5dade2");

    bool mostrarProximoNivel = EsEscenaCampaña()
      && CampaignManager.Instance != null
      && CampaignManager.Instance.scMenuPersonajes != null
      && CampaignManager.Instance.scMenuPersonajes.pSel != null
      && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: removes +1 Debuff per unit.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 removed Debuff) or Option B (+1 Valour per removed Debuff).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: remueve +1 Debuff por unidad.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 Debuff removido) u Opcion B (+1 Valentía por Debuff removido).</color>"; }
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
                string nombreLanzador = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;
                string nombreBuff = TRADU.i != null ? TRADU.i.Traducir(buff.buffNombre) : buff.buffNombre;
                string nombreAliado = TRADU.i != null ? TRADU.i.Traducir(aliado.uNombre) : aliado.uNombre;
                string verboRemueve = TRADU.i != null ? TRADU.i.Traducir(" remueve ") : " remueve ";
                string conector = (TRADU.i != null && TRADU.i.nIdioma == 2) ? " from " : " de ";
                BattleManager.Instance.EscribirLog(nombreLanzador + verboRemueve + nombreBuff + conector + nombreAliado + ".");
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
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

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





