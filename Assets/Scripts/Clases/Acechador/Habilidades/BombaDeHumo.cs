using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class BombaDeHumo : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    
    
     public override void  Awake()
    {
      nombre = "Bomba de Humo";
      IDenClase = 5;
      costoAP = 2;
      if(NIVEL > 2){costoAP--;}
      costoPM = 1;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 5;
      if(NIVEL > 1){cooldownMax--;}
      bAfectaObstaculos = false;

      poneTrampas = true;
      


      imHab = Resources.Load<Sprite>("imHab/Acechador_BombaDeHumo");
      ActualizarDescripcion();

    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;

      int radioZona = NIVEL == 5 ? 2 : 1;
      int duracionHumo = NIVEL == 4 ? 3 : 2;

      string tituloEs = "Bomba de Humo I";
      string tituloEn = "Smoke Bomb I";
      if (NIVEL == 2) { tituloEs = "Bomba de Humo II"; tituloEn = "Smoke Bomb II"; }
      if (NIVEL == 3) { tituloEs = "Bomba de Humo III"; tituloEn = "Smoke Bomb III"; }
      if (NIVEL == 4) { tituloEs = "Bomba de Humo IV a"; tituloEn = "Smoke Bomb IV a"; }
      if (NIVEL == 5) { tituloEs = "Bomba de Humo IV b"; tituloEn = "Smoke Bomb IV b"; }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Utility Trap (4 range)\n";
        cuerpo += "<b>Target:</b> 1 tile in range\n";
        cuerpo += "<b>Roll/Save:</b> none\n";
        cuerpo += $"<b>On cast:</b> creates smoke traps in area radius {radioZona} around target\n";
        cuerpo += $"<b>Smoke trap profile:</b> 30 uses, {duracionHumo} turns duration, persistent\n";
        cuerpo += "<b>On trap trigger (any unit):</b> grants Hidden (1) if not hidden\n";
        cuerpo += "<b>Extra buff for non-Stalker units:</b> 2 turns, +2 Attack, +1 crit range";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Trampa de Utilidad (4 alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 casilla en rango\n";
        cuerpo += "<b>Tirada/TS:</b> no tiene\n";
        cuerpo += $"<b>Al lanzarla:</b> crea trampas de humo en area de radio {radioZona} alrededor de la casilla objetivo\n";
        cuerpo += $"<b>Perfil de trampa de humo:</b> 30 usos, {duracionHumo} turnos de duracion, persistente\n";
        cuerpo += "<b>Al activar trampa (cualquier unidad):</b> otorga Escondido (1) si no estaba escondido\n";
        cuerpo += "<b>Buff extra para unidades no Acechador:</b> 2 turnos, +2 Ataque, +1 rango critico";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Val Cost: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Val: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : tituloEs,
        esIngles
          ? "Creates a smoke field that restores stealth and buffs allies moving through it."
          : "Crea un campo de humo que restaura sigilo y buffea aliados que lo atraviesan.",
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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 AP cost.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 turn smoke duration) or Option B (radius 2 area).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 costo AP.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 turno de duracion de humo) u Opcion B (area radio 2).</color>"; }
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
      int alre = 1;
      if(NIVEL == 5){alre = 2;} //Aumenta el alcance de las casillas alrededor a 2 si es nivel 5
      casillasAlrededor = cas.ObtenerCasillasAlrededor(alre);
      casillasAlrededor.Add(cas); //Agrega la casilla origen


      foreach (Casilla c in casillasAlrededor)
      {
        TrampaBombaHumo trampa = c.AddComponent<TrampaBombaHumo>();
        trampa.Inicializar(NIVEL);
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
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(4); //alcance
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

