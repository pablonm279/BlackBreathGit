using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Fogata : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
      public override void  Awake()
    {
      nombre = "Fogata";
      IDenClase = 8;
      costoAP = 5;
      if(NIVEL > 2){costoAP--;}
      if(NIVEL == 5){costoAP--;}
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0; 
      esforzable = 2;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 7;
      bAfectaObstaculos = false;
      poneTrampas = true;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Explorador_Fogata");
      ActualizarDescripcion();
    }
  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    int trampaUsos = NIVEL > 1 ? 4 : 3;
    string danoTrampaEs = FormatearRangoDados(1, 3);
    int duracionTrampaTurnos = NIVEL > 1 ? 5 : 4;
    string dadoFuego = NIVEL == 4 ? "1d9" : "1d6";
    string dadoFuegoEs = NIVEL == 4 ? FormatearRangoDados(1, 9) : FormatearRangoDados(1, 6);

    string tituloEs = "Fogata I";
    string tituloEn = "Campfire I";
    string tituloPt = "Fogueira I";
    if (NIVEL == 2) { tituloEs = "Fogata II"; tituloEn = "Campfire II"; }
    if (NIVEL == 3) { tituloEs = "Fogata III"; tituloEn = "Campfire III"; }
    if (NIVEL == 4) { tituloEs = "Fogata IV a"; tituloEn = "Campfire IV a"; }
    if (NIVEL == 5) { tituloEs = "Fogata IV b"; tituloEn = "Campfire IV b"; }
    if (NIVEL == 2) { tituloPt = "Fogueira II"; }
    if (NIVEL == 3) { tituloPt = "Fogueira III"; }
    if (NIVEL == 4) { tituloPt = "Fogueira IV a"; }
    if (NIVEL == 5) { tituloPt = "Fogueira IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Utility Trap\n";
      cuerpo += "<b>Target:</b> Adjacent tile (including own tile)\n";
      cuerpo += $"<b>Campfire trap:</b> {duracionTrampaTurnos} turns, {trampaUsos} uses\n";
      cuerpo += "<b>Trap trigger:</b> 1d3 fire damage (persistent)\n";
      cuerpo += $"<b>Adjacency buff:</b> while adjacent to a campfire, gains fire arrows (+{dadoFuego} fire damage on attacks)";
    }
    else if (esPortugues)
    {
      cuerpo += "<b>Tipo:</b> Armadilha de Utilidade\n";
      cuerpo += "<b>Alvo:</b> Casa adjacente (inclui a propria casa)\n";
      cuerpo += $"<b>Armadilha fogueira:</b> {duracionTrampaTurnos} turnos, {trampaUsos} usos\n";
      cuerpo += "<b>Ativacao da armadilha:</b> 1d3 dano de fogo (persistente)\n";
      cuerpo += $"<b>Buff por adjacencia:</b> enquanto estiver adjacente a uma fogueira, ganha flechas de fogo (+{dadoFuego} dano de fogo em ataques)";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Trampa de Utilidad\n";
      cuerpo += "<b>Objetivo:</b> Casilla adyacente (incluye tu propia casilla)\n";
      cuerpo += $"<b>Trampa fogata:</b> {duracionTrampaTurnos} turnos, {trampaUsos} usos\n";
      cuerpo += $"<b>Activacion de trampa:</b> {danoTrampaEs} danio de fuego (persistente)\n";
      cuerpo += $"<b>Buff por adyacencia:</b> mientras estas adyacente a una fogata, ganas flechas de fuego (+{dadoFuegoEs} danio de fuego en ataques)";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "Sets battlefield control and empowers shots near the fire."
        : esPortugues
          ? "Planta controle no terreno e fortalece disparos perto do fogo."
        : "Planta control en el terreno y potencia disparos cerca del fuego.",
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
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 trap use and +1 trap duration turn.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 AP cost.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1d3 fire-arrow bonus) or Option B (-1 AP cost).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 uso de armadilha e +1 turno de duracao da armadilha.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 custo AP.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1d3 no bonus de flechas de fogo) ou Opcao B (-1 custo AP).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 uso de trampa y +1 turno de duracion de trampa.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 costo AP.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1-3 al bono de flechas de fuego) u Opcion B (-1 costo AP).</color>"; }
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
        ClaseExplorador clas = (ClaseExplorador)scEstaUnidad;
        clas.ChequeartieneFogataCerca();
        
    }
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
    {
       TrampaFogata trampa = cas.AddComponent<TrampaFogata>();
       trampa.Inicializar(NIVEL);
       trampa.AsignarCreador(scEstaUnidad);
     
   
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
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(1);
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
    
         
    }

   
    

 
}




