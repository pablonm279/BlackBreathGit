using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ImprovisarFlechas : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
     public override void  Awake()
    {
      nombre = "Improvisar Flechas";
      IDenClase = 3;
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
      cooldownMax = 4;
      bAfectaObstaculos = false;
      
     
      usosBatalla = 2;

      imHab = Resources.Load<Sprite>("imHab/Explorador_ImprovisarFlechas");
      ActualizarDescripcion();
    }
        public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

      int flechasFijas = (NIVEL > 1 ? 1 : 0) + (NIVEL == 4 ? 1 : 0);
      int buffCrit = 1 + (NIVEL > 2 ? 1 : 0);
      int buffPenetracion = 1;
      int duracionBuff = 2;
      bool sumaDanioNivel5 = NIVEL == 5;

      string tituloEs = "Improvisar Flechas I";
      string tituloEn = "Improvise Arrows I";
      if (NIVEL == 2) { tituloEs = "Improvisar Flechas II"; tituloEn = "Improvise Arrows II"; }
      if (NIVEL == 3) { tituloEs = "Improvisar Flechas III"; tituloEn = "Improvise Arrows III"; }
      if (NIVEL == 4) { tituloEs = "Improvisar Flechas IV a"; tituloEn = "Improvise Arrows IV a"; }
      if (NIVEL == 5) { tituloEs = "Improvisar Flechas IV b"; tituloEn = "Improvise Arrows IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Utility\n";
        cuerpo += "<b>Target:</b> Self\n";
        if (flechasFijas > 0)
        {
          cuerpo += $"<b>Arrows gained:</b> current AP + {flechasFijas}\n";
        }
        else
        {
          cuerpo += "<b>Arrows gained:</b> current AP\n";
        }
        cuerpo += "<b>On cast:</b> sets current AP to 0\n";
        cuerpo += $"<b>Buff ({duracionBuff} turns):</b> +{buffCrit} crit range, +{buffPenetracion} Armor Penetration";
        if (sumaDanioNivel5)
        {
          cuerpo += ", +15% Damage";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Utilidad\n";
        cuerpo += "<b>Objetivo:</b> Uno mismo\n";
        if (flechasFijas > 0)
        {
          cuerpo += $"<b>Flechas ganadas:</b> AP actuales + {flechasFijas}\n";
        }
        else
        {
          cuerpo += "<b>Flechas ganadas:</b> AP actuales\n";
        }
        cuerpo += "<b>Al lanzarla:</b> deja los AP actuales en 0\n";
        cuerpo += $"<b>Buff ({duracionBuff} turnos):</b> +{buffCrit} rango critico, +{buffPenetracion} Penetracion de armadura";
        if (sumaDanioNivel5)
        {
          cuerpo += ", +15% Danio";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "Converts tempo into ammo and primes your next attacks."
          : "Convierte tempo en municion y prepara tus siguientes ataques.",
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 fixed arrow.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 crit range in the buff.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 fixed arrow) or Option B (+15% damage in the buff).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 flecha fija.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 rango critico en el buff.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 flecha fija) u Opcion B (+15% danio en el buff).</color>"; }
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
        
        //AplicarEfectosHabilidad(scEstaUnidad, 0);
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
      cooldownActual = cooldownMax;
      
      
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");

       int APusados = (int)scEstaUnidad.ObtenerAPActual();
       int flechasCreadas = 0;
      
        for (int veces = 0; veces < APusados; veces++)
        {
           flechasCreadas++;
        }
        if( NIVEL > 1)
        {
         flechasCreadas++;
        }
        if( NIVEL == 4)
        {
         flechasCreadas++;
        }
        
        Usuario.GetComponent<ClaseExplorador>().CambiarCantidadFlechas(flechasCreadas);

        VFXAplicar(Usuario);
       scEstaUnidad.EstablecerAPActualA(0);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Flechas Preparadas";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantCritDado += 1;
       buff.cantPenetracionArmadura  += 1;
       if (NIVEL > 2)
    {
      buff.cantCritDado += 1;
    }
        if( NIVEL == 5)
       {
         buff.cantDanioPorcentaje += 15;
       }
       buff.AplicarBuff(scEstaUnidad);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);
       

      

    }
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_ImprovisarFlechas");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);  

    }

    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private void ObtenerObjetivos()
    {
      lObjetivosPosibles.Add(scEstaUnidad);

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
            uni.Marcar(1);
      }
     
    }
      
         
   

   
    

 
}





