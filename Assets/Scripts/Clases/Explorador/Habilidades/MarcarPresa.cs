using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class MarcarPresa : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;

   
     public override void  Awake()
    {
      nombre = "Marcar Presa";
      IDenClase = 4;
      costoAP = 1;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = true;
      cooldownMax = 5;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Explorador_MarcarPresa");
      ActualizarDescripcion();
    }

        public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

    string tituloEs = "Marcar Presa I";
    string tituloEn = "Mark Prey I";
    if (NIVEL == 2) { tituloEs = "Marcar Presa II"; tituloEn = "Mark Prey II"; }
    if (NIVEL == 3) { tituloEs = "Marcar Presa III"; tituloEn = "Mark Prey III"; }
    if (NIVEL == 4) { tituloEs = "Marcar Presa IV a"; tituloEn = "Mark Prey IV a"; }
    if (NIVEL == 5) { tituloEs = "Marcar Presa IV b"; tituloEn = "Mark Prey IV b"; }

    int bonoAtaqueMarca = NIVEL == 4 ? 2 : 4;
    int bonoCritRangoMarca = 1 + (NIVEL > 2 ? 1 : 0);
    int bonoCritDanioMarca = 15 + (NIVEL > 1 ? 5 : 0);

    int recompensaVal = NIVEL == 5 ? 2 : 1;
    int recompensaApMax = NIVEL == 5 ? 2 : 1;
    int recompensaTsMental = NIVEL == 5 ? 3 : 2;

    bool aplicaDebuffPropio = NIVEL != 4;

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Mark\n";
      cuerpo += "<b>Target:</b> 1 enemy on opposite side\n";
      cuerpo += "<b>Roll/Save:</b> none (direct application)\n";
      cuerpo += "<b>Mark duration:</b> 3 turns\n";
      cuerpo += $"<b>Bonuses vs marked target:</b> +{bonoAtaqueMarca} attack, +{bonoCritRangoMarca} crit range, +{bonoCritDanioMarca}% crit damage\n";
      cuerpo += aplicaDebuffPropio
        ? "<b>Self effect on cast:</b> -2 Attack for 2 turns (only against non-marked targets)\n"
        : "<b>Self effect on cast:</b> no attack penalty against non-marked targets\n";
      cuerpo += $"<b>On marked kill:</b> +{recompensaVal} Valour, +{recompensaApMax} max AP and +{recompensaTsMental} Mental Save for 3 turns";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Marca\n";
      cuerpo += "<b>Objetivo:</b> 1 enemigo del lado opuesto\n";
      cuerpo += "<b>Tirada/TS:</b> no tiene (aplicacion directa)\n";
      cuerpo += "<b>Duracion de marca:</b> 3 turnos\n";
      cuerpo += $"<b>Bonos contra marcado:</b> +{bonoAtaqueMarca} ataque, +{bonoCritRangoMarca} rango critico, +{bonoCritDanioMarca}% danio critico\n";
      cuerpo += aplicaDebuffPropio
        ? "<b>Efecto propio al lanzar:</b> -2 Ataque por 2 turnos (solo contra objetivos no marcados)\n"
        : "<b>Efecto propio al lanzar:</b> sin penalidad de ataque contra objetivos no marcados\n";
      cuerpo += $"<b>Al matar al marcado:</b> +{recompensaVal} Valentía, +{recompensaApMax} AP max y +{recompensaTsMental} TS Mental por 3 turnos";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "Paints a priority target and shifts your full damage profile into hunting it."
        : "Marca un objetivo prioritario y redirige tu perfil ofensivo a cazarlo.",
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% crit damage on marked target.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 crit range on marked target.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A removes self attack penalty; Option B improves kill reward (+1 Valour, +1 max AP, +1 Mental Save).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% al danio critico contra marcado.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al rango critico contra marcado.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A elimina la penalidad propia de ataque; Opcion B mejora la recompensa por muerte (+1 Valentía, +1 AP max, +1 TS Mental).</color>"; }
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
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

      if(obj is Unidad) //Acá van los efectos a Unidades.
      {
          
          Unidad objetivo = (Unidad)obj;
          VFXAplicar(objetivo.gameObject);

          BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + " -> " + TRADU.i.Traducir(objetivo.uNombre) + ".");

          MarcaMarcarPresa marca = new MarcaMarcarPresa();
          marca.nombre = "Presa Marcada";
          marca.quienMarco = scEstaUnidad;
          marca.NIVEL = NIVEL;
          marca.duracion = 3;

          MarcaMarcarPresa buffComponent = ComponentCopier.CopyComponent(marca, objetivo.gameObject);
          objetivo.Marcar(0);

          objetivo.GenerarTextoFlotante(TRADU.i.Traducir("Marcado"), Color.yellow);

                        
      }
      
      cooldownActual = cooldownMax;
    scEstaUnidad.CambiarAPActual(-costoAP); 

      if(NIVEL != 4) // a Nivel IVa, no recibe el debuff
      {
        /////////////////////////////////////////////
        //BUFF ---- Así se aplica un buff/debuff
        Buff buff = new Buff();
        buff.buffNombre = "Marcando Presa";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAtaque -= 2;
        buff.AplicarBuff(scEstaUnidad);
        // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
        Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);
      }
    }
    
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_MarcarPresa");

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
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasLadoOpuesto();
    
    
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
             c.ActivarCapaColorRojo();
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






