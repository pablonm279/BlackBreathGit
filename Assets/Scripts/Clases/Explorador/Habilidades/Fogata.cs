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

    int trampaUsos = NIVEL > 1 ? 4 : 3;
    int duracionTrampaTurnos = NIVEL > 1 ? 5 : 4;
    string dadoFuego = NIVEL == 4 ? "1d9" : "1d6";

    string tituloEs = "Fogata I";
    string tituloEn = "Campfire I";
    if (NIVEL == 2) { tituloEs = "Fogata II"; tituloEn = "Campfire II"; }
    if (NIVEL == 3) { tituloEs = "Fogata III"; tituloEn = "Campfire III"; }
    if (NIVEL == 4) { tituloEs = "Fogata IV a"; tituloEn = "Campfire IV a"; }
    if (NIVEL == 5) { tituloEs = "Fogata IV b"; tituloEn = "Campfire IV b"; }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += "<b>Type:</b> Utility Trap\n";
      cuerpo += "<b>Target:</b> Adjacent tile (including own tile)\n";
      cuerpo += $"<b>Campfire trap:</b> {duracionTrampaTurnos} turns, {trampaUsos} uses\n";
      cuerpo += "<b>Trap trigger:</b> 1d3 fire damage (persistent)\n";
      cuerpo += $"<b>Adjacency buff:</b> while adjacent to a campfire, gains fire arrows (+{dadoFuego} fire damage on attacks)";
    }
    else
    {
      cuerpo += "<b>Tipo:</b> Trampa de Utilidad\n";
      cuerpo += "<b>Objetivo:</b> Casilla adyacente (incluye tu propia casilla)\n";
      cuerpo += $"<b>Trampa fogata:</b> {duracionTrampaTurnos} turnos, {trampaUsos} usos\n";
      cuerpo += "<b>Activacion de trampa:</b> 1d3 danio de fuego (persistente)\n";
      cuerpo += $"<b>Buff por adyacencia:</b> mientras estas adyacente a una fogata, ganas flechas de fuego (+{dadoFuego} danio de fuego en ataques)";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
      : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}\n- Esforzable: Si ({esforzable})";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : tituloEs,
      esIngles
        ? "Sets battlefield control and empowers shots near the fire."
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
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 uso de trampa y +1 turno de duracion de trampa.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 costo AP.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1d3 al bono de flechas de fuego) u Opcion B (-1 costo AP).</color>"; }
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




