using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PrimerosAuxilios : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
     public override void  Awake()
    {
      nombre = "Primeros Auxilios";
      IDenClase = 4;
      costoAP = 0;
      costoPM = 0;
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
      
     
      usosBatalla = 2;

      imHab = Resources.Load<Sprite>("imHab/Caballero_PrimerosAuxilios");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

      string dadoCuracion = NIVEL == 1 ? "1d4" : "1d6";
      int usos = NIVEL > 2 ? 3 : 2;
      int bonoResguardo = NIVEL == 4 ? 40 : 30;
      bool trasladaCampania = NIVEL == 5;

      string tituloEs = "Primeros Auxilios I";
      string tituloEn = "First Aid I";
      if (NIVEL == 2) { tituloEs = "Primeros Auxilios II"; tituloEn = "First Aid II"; }
      if (NIVEL == 3) { tituloEs = "Primeros Auxilios III"; tituloEn = "First Aid III"; }
      if (NIVEL == 4) { tituloEs = "Primeros Auxilios IV a"; tituloEn = "First Aid IV a"; }
      if (NIVEL == 5) { tituloEs = "Primeros Auxilios IV b"; tituloEn = "First Aid IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Heal\n";
        cuerpo += "<b>Target:</b> Self or ally at range 1\n";
        cuerpo += $"<b>Healing:</b> 1 + ({dadoCuracion} x current AP)\n";
        cuerpo += "<b>Additional:</b> Removes Bleed and Poison\n";
        cuerpo += $"<b>In cover bonus:</b> +{bonoResguardo}% healing if there is another ally in a more frontal column\n";
        cuerpo += "<b>On cast:</b> spends all current AP";
        if (trasladaCampania)
        {
          cuerpo += "\n<b>Campaign:</b> this upgrade enables campaign transfer effect";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Curacion\n";
        cuerpo += "<b>Objetivo:</b> Uno mismo o aliado a rango 1\n";
        cuerpo += $"<b>Curacion:</b> 1 + ({dadoCuracion} x AP actuales)\n";
        cuerpo += "<b>Adicional:</b> remueve Sangrado y Veneno\n";
        cuerpo += $"<b>Bono en resguardo:</b> +{bonoResguardo}% curacion si hay otro aliado en una columna mas frontal\n";
        cuerpo += "<b>Al lanzarla:</b> consume todos los AP actuales";
        if (trasladaCampania)
        {
          cuerpo += "\n<b>Campania:</b> esta mejora habilita el efecto de traslado a campania";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Val Cost: {costoPM}\n- Uses per battle: {usos}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Val: {costoPM}\n- Usos por batalla: {usos}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "Field treatment that scales with the AP you are willing to spend right now."
          : "Atencion de campo que escala segun los AP que decidas gastar en ese momento.",
        cuerpo,
        costos,
        "#5dade2");

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: healing die changes from 1d4 to 1d6 per AP.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 use per battle.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+40% cover bonus) or Option B (campaign transfer upgrade).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: el dado de curacion pasa de 1d4 a 1d6 por AP.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 uso por batalla.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+40% en resguardo) u Opcion B (mejora de traslado a campania).</color>"; }
      }
    }

    void Start()
    {
        if(NIVEL > 2)
        {
          usosBatalla++;
        }

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
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
    
     if(obj is Unidad) //AcÃ¡ van los efectos a Unidades.
     {
        
       Unidad objetivo = (Unidad)obj;
        VFXAplicar(objetivo.gameObject);
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");

       int APusados = (int)scEstaUnidad.ObtenerAPActual();

       float curacion = 1;

      if(NIVEL == 1) //Cura 1d4 por AP
      {
        for (int veces = 0; veces < APusados; veces++)
        {
            curacion += UnityEngine.Random.Range(1,5);

        }
      }
      else //Cura 1d6 por AP
      {
        for (int veces = 0; veces < APusados; veces++)
        {
            curacion += UnityEngine.Random.Range(1,7);

        }

      }

       if(ChequearSiHayAliadoAdelantado(objetivo))
       {
        
        if(NIVEL == 4)
        {
          curacion = curacion*1.4f;
        }
        else{ curacion = curacion*1.3f;}

       }

      scEstaUnidad.EstablecerAPActualA(0);
       
     
       if(objetivo.estado_sangrado > 0)
       {
         objetivo.estado_sangrado = 0;
         await objetivo.GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Sangrado") + "</s>", Color.red);
       }
       if(objetivo.estado_veneno > 0)
       {
         objetivo.estado_veneno = 0;
         await objetivo.GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Veneno") + "</s>", Color.green);
       }
        objetivo.RecibirCuracion(curacion, false);

       objetivo.Marcar(0);

       usosBatalla--;
       if(usosBatalla == 0)//Al gastarse los usos, se borra la habilidad
       {
        Destroy(this); 
        BattleManager.Instance.scUIBotonesHab.UIDesactivarHabilidades();
        BattleManager.Instance.scUIBotonesHab.ActualizarBotonesHabilidad();
       } 
      
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
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PrimerosAuxilios");

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
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(1);
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



