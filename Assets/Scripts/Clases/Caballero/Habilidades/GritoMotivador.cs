using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class GritoMotivador : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
     public override void  Awake()
    {
      nombre = "Grito Motivador";
      IDenClase = 2;
      costoAP = 2;
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = true;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 4;
      bAfectaObstaculos = false;

      imHab = Resources.Load<Sprite>("imHab/Caballero_GritoMotivador");

       
      ActualizarDescripcion();
    
    }

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int buffDanio = NIVEL > 1 ? 15 : 10;
      int valorAliados = 1;
      bool afectaEnemigos = NIVEL == 5;
      int duracionDebuffEnemigos = 1;

      string tituloEs = "Grito Motivador I";
      string tituloEn = "War Cry I";
      string tituloPt = "Grito Motivador I";
      if (NIVEL == 2) { tituloEs = "Grito Motivador II"; tituloEn = "War Cry II"; }
      if (NIVEL == 3) { tituloEs = "Grito Motivador III"; tituloEn = "War Cry III"; }
      if (NIVEL == 4) { tituloEs = "Grito Motivador IV a"; tituloEn = "War Cry IV a"; }
      if (NIVEL == 5) { tituloEs = "Grito Motivador IV b"; tituloEn = "War Cry IV b"; }
      if (NIVEL == 2) { tituloPt = "Grito Motivador II"; }
      if (NIVEL == 3) { tituloPt = "Grito Motivador III"; }
      if (NIVEL == 4) { tituloPt = "Grito Motivador IV a"; }
      if (NIVEL == 5) { tituloPt = "Grito Motivador IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Support\n";
        cuerpo += "<b>Target:</b> All allied units on your side\n";
        cuerpo += $"<b>Allied buff:</b> +{buffDanio}% Damage for 3 turns\n";
        if (valorAliados > 0)
        {
          cuerpo += $"<b>Allied bonus:</b> +{valorAliados} Valour to other allies\n";
        }
        if (NIVEL == 4)
        {
          cuerpo += "<b>Self bonus:</b> +2 Valour per affected ally\n";
        }
        if (afectaEnemigos)
        {
          cuerpo += "<b>Enemy effect:</b> -10% Damage for 1 turn (no save)";
        }
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Suporte\n";
        cuerpo += "<b>Alvo:</b> Todas as unidades aliadas do seu lado\n";
        cuerpo += $"<b>Buff em aliados:</b> +{buffDanio}% Dano por 3 turnos\n";
        if (valorAliados > 0)
        {
          cuerpo += $"<b>Bonus em aliados:</b> +{valorAliados} Valentia para os outros aliados\n";
        }
        if (NIVEL == 4)
        {
          cuerpo += "<b>Bonus proprio:</b> +2 Valentia por aliado afetado\n";
        }
        if (afectaEnemigos)
        {
          cuerpo += $"<b>Efeito em inimigos:</b> -10% Dano por {duracionDebuffEnemigos} turno (sem resistencia)";
        }
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Soporte\n";
        cuerpo += "<b>Objetivo:</b> Todas las unidades aliadas de tu lado\n";
        cuerpo += $"<b>Buff aliados:</b> +{buffDanio}% Danio por 3 turnos\n";
        if (valorAliados > 0)
        {
          cuerpo += $"<b>Bono aliados:</b> +{valorAliados} Valentía a los demás aliados\n";
        }
        if (NIVEL == 4)
        {
          cuerpo += "<b>Bono propio:</b> +2 Valentía por cada aliado afectado\n";
        }
        if (afectaEnemigos)
        {
          cuerpo += $"<b>Efecto enemigos:</b> -10% Danio por {duracionDebuffEnemigos} turno (sin TS)";
        }
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "A commanding shout that empowers allies and, at mastery, weakens enemies."
          : esPortugues
            ? "Um grito de comando que fortalece aliados e, no dominio total, enfraquece inimigos."
          : "Un grito de mando que potencia aliados y, al dominarlo, debilita enemigos.",
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% allied damage buff.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: improved progression toward IV specialization.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Valour per ally to self) or Option B (enemy damage debuff).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% no buff de dano aliado.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: melhora a progressao para a especializacao de nivel IV.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+2 Valentia por aliado para o Cavaleiro) ou Opcao B (debuff de dano em inimigos).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% al buff de danio aliado.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: mejora la progresión hacia la especialización de nivel IV.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+2 Valentía por aliado para el Caballero) u Opcion B (debuff de danio a enemigos).</color>"; }
      }
    }

  public override async Task Resolver(List<object> Objetivos, Casilla cas) //Esto esta hecho para que anuncie el uso de la habilidad en el Log
  {
    // El log de uso ahora está centralizado en Habilidad.Resolver
    VFXAplicarPropio(Usuario.gameObject);
   await base.Resolver(Objetivos);
    
  }


    void VFXAplicarPropio(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorOrigen");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---

  }
  void VFXAplicarAliado(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorEfectoAliado");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---
  }
   void VFXAplicarEnemigo(GameObject objetivo)
  {
    VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_GritoMotivadorEfectoEnemigo");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, objetivo.transform.rotation);
    vfx.transform.parent = objetivo.transform;
     
   //Esto pone en la capa del canvas de la unidad afectada +1, para que se vea encima
   Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
   RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5); 
            //---
  }
    Casilla Origen;
    public override void Activar()
    {
        Origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

      
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;

        
    }
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla casillaObjetivo)
    {

    
    if(obj is Unidad) //Acá van los efectos a Unidades.
     {

      Unidad objetivo = (Unidad)obj;
      if(objetivo.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado) //Chequea si son aliados para buffearlos o enemigos para debuffearlos (si nv5)
      { 

       if (objetivo != scEstaUnidad)
       {
        bool enIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        string nombreObjetivo = TRADU.i != null ? TRADU.i.Traducir(objetivo.uNombre) : objetivo.uNombre;
        string motivoValentia = enIngles
          ? nombreObjetivo + " is emboldened by War Cry"
          : (TRADU.i != null && TRADU.i.nIdioma == 3)
            ? nombreObjetivo + " se encoraja com Grito Motivador"
          : nombreObjetivo + " se envalentona por Grito Motivador";
        objetivo.SumarValentia(1, motivoValentia, mostrarTextoFlotante: false);
       }

       if(NIVEL == 4)
       {
        scEstaUnidad.SumarValentia(2);
       }

        if (objetivo != scEstaUnidad)
        {
            VFXAplicarAliado(objetivo.gameObject);
        }
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Grito Motivador";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 3;
       buff.cantDanioPorcentaje += 10;
       if( NIVEL > 1)
       {
        buff.cantDanioPorcentaje += 5;
       }
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        objetivo.Marcar(0);
     }
     else if(NIVEL == 5)
     {
        VFXAplicarEnemigo(objetivo.gameObject);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Grito Desmotivador";
       buff.boolfDebufftBuff = false;
       buff.DuracionBuffRondas = 1;
       buff.cantDanioPorcentaje -= 10;
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       
       objetivo.Marcar(0);
     }
    }
    }
    
 

    //Provisorio
    private List<Unidad> lObjetivosPosibles = new List<Unidad>();
    private List<Obstaculo> lObstaculosPosibles = new List<Obstaculo>();

    private void ObtenerObjetivos()
    {
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
      if(NIVEL == 5)
      {
         List<Casilla> lCasillop = Origen.ObtenerCasillasLadoOpuesto();
        lCasillasafectadas.AddRange(lCasillop);

      }
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
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
             c.ActivarCapaColorAzul();
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









