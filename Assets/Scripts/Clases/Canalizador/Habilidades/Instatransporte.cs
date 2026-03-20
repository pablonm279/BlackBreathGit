using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class Instatransporte : Habilidad
{


  [SerializeField] private GameObject VFXenObjetivo;

    public override void  Awake()
    {
    nombre = "Instatransporte";
    IDenClase = 3;
    costoAP = 1;
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
    if (NIVEL > 1) { cooldownMax--; }
    bAfectaObstaculos = false;
    poneTrampas = true;

    imHab = Resources.Load<Sprite>("imHab/Canalizador_Instatransporte");




  }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    string tituloEs = "Instatransporte I";
    string tituloEn = "Instatransport I";
    string tituloPt = "Instatransporte I";
    if (NIVEL == 2) { tituloEs = "Instatransporte II"; tituloEn = "Instatransport II"; }
    if (NIVEL == 3) { tituloEs = "Instatransporte III"; tituloEn = "Instatransport III"; }
    if (NIVEL == 4) { tituloEs = "Instatransporte IV a"; tituloEn = "Instatransport IV a"; }
    if (NIVEL == 5) { tituloEs = "Instatransporte IV b"; tituloEn = "Instatransport IV b"; }
    if (NIVEL == 2) { tituloPt = "Instatransporte II"; }
    if (NIVEL == 3) { tituloPt = "Instatransporte III"; }
    if (NIVEL == 4) { tituloPt = "Instatransporte IV a"; }
    if (NIVEL == 5) { tituloPt = "Instatransporte IV b"; }

    int alcance = NIVEL > 2 ? 4 : 3;
    int bonusEvasion = NIVEL == 5 ? 2 : 1;
    string residuosEs = NIVEL == 4
      ? "Genera Residuos Energeticos en todo alrededor del destino."
      : "Genera Residuos Energeticos en cruz adyacente al destino.";
    string residuosEn = NIVEL == 4
      ? "Generates Energy Residues all around the destination."
      : "Generates Energy Residues in an adjacent cross at destination.";
    string residuosPt = NIVEL == 4
      ? "Gera Residuos Energeticos em volta de todo o destino."
      : "Gera Residuos Energeticos em cruz adjacente ao destino.";

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<b>Type:</b> Ranged ({alcance} range)\n";
      cuerpo += "<b>Target:</b> 1 empty tile in range\n";
      cuerpo += "<b>Effect:</b> Instant teleport to target tile\n";
      cuerpo += "<b>On arrival:</b> Destroys traps on destination tile\n";
      cuerpo += $"<b>Extra:</b> {residuosEn}\n";
      cuerpo += $"<b>Self buff:</b> +{bonusEvasion} Evasion";
    }
    else if (esPortugues)
    {
      cuerpo += $"<b>Tipo:</b> Distancia ({alcance} alcance)\n";
      cuerpo += "<b>Alvo:</b> 1 casa vazia em alcance\n";
      cuerpo += "<b>Efeito:</b> Teletransporte instantaneo para a casa alvo\n";
      cuerpo += "<b>Ao chegar:</b> Destroi armadilhas na casa de destino\n";
      cuerpo += $"<b>Extra:</b> {residuosPt}\n";
      cuerpo += $"<b>Buff proprio:</b> +{bonusEvasion} Evasao";
    }
    else
    {
      cuerpo += $"<b>Tipo:</b> Rango ({alcance} alcance)\n";
      cuerpo += "<b>Objetivo:</b> 1 casilla vacia en rango\n";
      cuerpo += "<b>Efecto:</b> Teletransporte instantaneo a la casilla objetivo\n";
      cuerpo += "<b>Al llegar:</b> Destruye trampas en la casilla destino\n";
      cuerpo += $"<b>Extra:</b> {residuosEs}\n";
      cuerpo += $"<b>Buff propio:</b> +{bonusEvasion} Evasion";
    }

    string costos = esIngles
      ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
      : esPortugues
        ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

    txtDescripcion = ConstruirDescripcionEstandar(
      esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
      esIngles
        ? "The Channeler blinks through arcane space and leaves unstable residue behind."
        : esPortugues
          ? "O Canalizador se desloca pelo espaco arcano e deixa residuos instaveis para tras."
        : "El Canalizador se desplaza por el espacio arcano y deja residuo inestable atras.",
      cuerpo,
      costos,
      "#5dade2");

    bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 range.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (residues all around) or Option B (+1 Evasion).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 alcance.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (residuos em toda volta) ou Opcao B (+1 Evasao).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 enfriamiento.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 alcance.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (residuos en todo alrededor) u Opcion B (+1 Evasion).</color>"; }
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



  public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla cas)
  {
    scEstaUnidad.estado_evasion = 1;
    if (NIVEL == 5) { scEstaUnidad.estado_evasion += 1; }
  
    VFXAplicar(scEstaUnidad.gameObject);
    

    Trampa[] trampas = cas.transform.GetComponentsInChildren<Trampa>();
    foreach (Trampa trmp in trampas)
    {
      trmp.DestruirTrampa();

    }

    scEstaUnidad.TeletransportarACasilla(cas);
    
    int alre = 1;
    if (NIVEL == 4) { alre = 2; }
    foreach (Casilla ady in cas.ObtenerCasillasAlrededor(alre))
    {
      ady.AddComponent<ResiduoEnergetico>();
      ady.GetComponent<ResiduoEnergetico>().InicializarCreador(scEstaUnidad, NIVEL);

    }




  }
      void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Instatransporte");

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
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
    lCasillasafectadas.Clear();

    List<Casilla> alCasillasafectadas = new List<Casilla>();
    //Casillas Alrededor al origen
    int alre = 3;
    if (NIVEL > 2) { alre++; }
    alCasillasafectadas = Origen.ObtenerCasillasAlrededor(alre);
    alCasillasafectadas.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear

    foreach (Casilla c in alCasillasafectadas)
    {
      c.ActivarCapaColorAzul();
      if (c.Presente != null)
      {
        continue;
      }

      lCasillasafectadas.Add(c);


    }


  }

}
    

 

   /*  private void ObtenerObjetivos()
    {
      
      lObjetivosPosibles.Clear();

     int alcance = 3;
     if (NIVEL > 2) { alcance++; }
      //Casillas Alrededor al origen
     List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasAlrededor(alcance);
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


   
    

 
}*/




