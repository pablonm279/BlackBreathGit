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
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      ClasePurificadora scPurificadora = Usuario != null ? Usuario.GetComponent<ClasePurificadora>() : null;
      int fervorActual = scPurificadora != null ? scPurificadora.ObtenerFervor() : 0;

      int duracionTurnos = NIVEL == 4 ? 4 : 3;
      int bonusTS = fervorActual;
      int bonusBarrera = 3 * fervorActual;
      bool agregaDefensa = NIVEL > 2;
      bool agregaCuracion = NIVEL == 5;

      string tituloEs = "Escudo de Fe I";
      string tituloEn = "Shield of Faith I";
      string tituloPt = "Escudo da Fe I";
      if (NIVEL == 2) { tituloEs = "Escudo de Fe II"; tituloEn = "Shield of Faith II"; }
      if (NIVEL == 3) { tituloEs = "Escudo de Fe III"; tituloEn = "Shield of Faith III"; }
      if (NIVEL == 4) { tituloEs = "Escudo de Fe IV a"; tituloEn = "Shield of Faith IV a"; }
      if (NIVEL == 5) { tituloEs = "Escudo de Fe IV b"; tituloEn = "Shield of Faith IV b"; }
      if (NIVEL == 2) { tituloPt = "Escudo da Fe II"; }
      if (NIVEL == 3) { tituloPt = "Escudo da Fe III"; }
      if (NIVEL == 4) { tituloPt = "Escudo da Fe IV a"; }
      if (NIVEL == 5) { tituloPt = "Escudo da Fe IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (8 range)\n";
        cuerpo += "<b>Target:</b> 1 tile in range\n";
        cuerpo += "<b>Area:</b> Selected tile + adjacent tiles\n";
        cuerpo += $"<b>Trap Duration:</b> {duracionTurnos} turns\n";
        cuerpo += $"<b>On trigger:</b> Grants +{bonusBarrera} Barrier and +{bonusTS} to Fortitude/Reflex/Mental (based on Fervor {fervorActual} at cast)";
        if (agregaDefensa)
        {
          cuerpo += ", +1 Defense";
        }
        if (agregaCuracion)
        {
          cuerpo += ", heal 2d6";
        }
        cuerpo += "\n<b>Requirement:</b> Needs at least 1 Fervor to activate\n";
        cuerpo += "<b>On cast:</b> Does not consume Fervor";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Alcance (8 de alcance)\n";
        cuerpo += "<b>Alvo:</b> 1 celula no alcance\n";
        cuerpo += "<b>Area:</b> Celula selecionada + celulas adjacentes\n";
        cuerpo += $"<b>Duracao da armadilha:</b> {duracionTurnos} turnos\n";
        cuerpo += $"<b>Ao ativar:</b> Concede +{bonusBarrera} Barreira e +{bonusTS} em Fortitude/Reflexos/Mental (com base no Fervor {fervorActual} ao usar)";
        if (agregaDefensa)
        {
          cuerpo += ", +1 Defesa";
        }
        if (agregaCuracion)
        {
          cuerpo += ", cura 2d6";
        }
        cuerpo += "\n<b>Requisito:</b> Precisa de pelo menos 1 Fervor para ativar\n";
        cuerpo += "<b>Ao usar:</b> Nao consome Fervor";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (8 alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 casilla en rango\n";
        cuerpo += "<b>Area:</b> Casilla seleccionada + casillas adyacentes\n";
        cuerpo += $"<b>Duracion de trampa:</b> {duracionTurnos} turnos\n";
        cuerpo += $"<b>Al activarse:</b> Otorga +{bonusBarrera} Barrera y +{bonusTS} a Fortaleza/Reflejos/Mental (segun Fervor {fervorActual} al lanzar)";
        if (agregaDefensa)
        {
          cuerpo += ", +1 Defensa";
        }
        if (agregaCuracion)
        {
          cuerpo += ", cura 2d6";
        }
        cuerpo += "\n<b>Requisito:</b> Necesita al menos 1 Fervor para activarse\n";
        cuerpo += "<b>Al lanzar:</b> No consume Fervor";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "Places sacred ward tiles that protect allies using current Fervor."
          : esPortugues
            ? "Coloca zonas sagradas que protegem aliados usando o Fervor atual."
          : "Coloca zonas sagradas que protegen aliados usando el Fervor actual.",
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 Valour Cost.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense on trigger buff.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 turn duration) or Option B (+2d6 healing on trigger).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 custo de Valentia.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defesa no buff ao ativar.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 turno de duracao) ou Opcao B (+2d6 de cura ao ativar).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 costo de Valentía.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defensa en el buff al activarse.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 turno de duracion) u Opcion B (+2d6 curacion al activarse).</color>"; }
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



  public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
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
        TrampaEscudoFe trampa = c.AddComponent<TrampaEscudoFe>();

        int fervorActual = Usuario.GetComponent<ClasePurificadora>().ObtenerFervor();
        trampa.Inicializar(NIVEL, fervorActual);
        trampa.AsignarCreador(scEstaUnidad);
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




