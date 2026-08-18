using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class HaciaLasSombras : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
      
      public override void  Awake()
    {
      nombre = "Hacia Las Sombras";
      IDenClase = 9; // Termina turno
      costoAP = 1;
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 7;
      if(NIVEL > 2){cooldownMax--;}
      bAfectaObstaculos = false;
      poneTrampas = true;

      imHab = Resources.Load<Sprite>("imHab/Acechador_HaciaLasSombras");

       
      ActualizarDescripcion();
    
    }

  public override void ActualizarDescripcion()
  {
    bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
    bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

    int evasionGanada = NIVEL > 2 ? 3 : 2;
    bool terminaTurno = NIVEL != 4;
    string colorTitulo = "#5dade2";
    string colorEncabezado = "#44d3ec";
    string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
    string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
    string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

    string tituloEs = "Hacia Las Sombras I";
    string tituloEn = "Into the Shadows I";
    string tituloPt = "Para as Sombras I";
    if (NIVEL == 2) { tituloEs = "Hacia Las Sombras II"; tituloEn = "Into the Shadows II"; }
    if (NIVEL == 3) { tituloEs = "Hacia Las Sombras III"; tituloEn = "Into the Shadows III"; }
    if (NIVEL == 4) { tituloEs = "Hacia Las Sombras IV a"; tituloEn = "Into the Shadows IV a"; }
    if (NIVEL == 5) { tituloEs = "Hacia Las Sombras IV b"; tituloEn = "Into the Shadows IV b"; }
    if (NIVEL == 2) { tituloPt = "Para as Sombras II"; }
    if (NIVEL == 3) { tituloPt = "Para as Sombras III"; }
    if (NIVEL == 4) { tituloPt = "Para as Sombras IV a"; }
    if (NIVEL == 5) { tituloPt = "Para as Sombras IV b"; }

    if (esIngles)
    {
      string oculto = TerminoDescripcion(TerminoDescripcionId.Oculto, "Hidden (2)", "Estado_oculto");
      string evasion = TerminoDescripcion(TerminoDescripcionId.Evasion, $"Evasion ({evasionGanada})", "Estado_evasion");
      string proximaMejora = null;
      if (DebeMostrarProximaMejoraDescripcion())
      {
        if (NIVEL < 2) proximaMejora = "-1 cooldown.";
        else if (NIVEL == 2) proximaMejora = "+1 Evasion.";
        else if (NIVEL == 3) proximaMejora = "Option A: does not end the turn. Option B: no Valour cost.";
      }
      txtDescripcion = ConstruirDescripcionNormalizadaIngles(
        tituloEn,
        "Teleports, removes debuffs and returns the Stalker to stealth.",
        new[]
        {
          LineaDescripcion("Target", "1 empty tile on the Stalker's side"),
          LineaDescripcion("Effect", $"Teleports to the target tile, removes debuffs and gains {oculto} and {evasion}."),
          LineaDescripcion("Use", terminaTurno ? "Ends the turn." : "Does not end the turn.")
        },
        proximaMejora);
      return;
    }

    if (esPortugues)
    {
      string oculto=TerminoDescripcion(TerminoDescripcionId.Oculto,"Oculto (2)","Estado_oculto"); string evasao=TerminoDescripcion(TerminoDescripcionId.Evasion,$"Evasão ({evasionGanada})","Estado_evasion"); string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: -1 de recarga.":NIVEL==2?"Próximo nível: +1 Evasão.":NIVEL==3?"Opção A: não encerra o turno. Opção B: sem custo de Valentia.":null;
      txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloPt,"Teleporta, remove efeitos negativos e devolve o Espreitador à furtividade.",new[]{LineaDescripcion("Alvo","1 casa vazia no lado do Espreitador"),LineaDescripcion("Efeito",$"Teleporta para a casa alvo, remove efeitos negativos e recebe {oculto} e {evasao}."),LineaDescripcion("Uso",terminaTurno?"Encerra o turno.":"Não encerra o turno.")},prox); return;
    }
    {
      string oculto=TerminoDescripcion(TerminoDescripcionId.Oculto,"Oculto (2)","Estado_oculto"); string evasion=TerminoDescripcion(TerminoDescripcionId.Evasion,$"Evasión ({evasionGanada})","Estado_evasion"); string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: -1 de enfriamiento.":NIVEL==2?"Próximo nivel: +1 Evasión.":NIVEL==3?"Opción A: no termina el turno. Opción B: sin costo de Valentía.":null;
      txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloEs,"Teletransporta, elimina penalizaciones y devuelve al Acechador al sigilo.",new[]{LineaDescripcion("Objetivo","1 casilla vacía del lado del Acechador"),LineaDescripcion("Efecto",$"Se teletransporta a la casilla objetivo, elimina penalizaciones y obtiene {oculto} y {evasion}."),LineaDescripcion("Uso",terminaTurno?"Termina el turno.":"No termina el turno.")},prox); return;
    }

    string cuerpo = "";
    if (esIngles)
    {
      cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Mobility utility\n";
      cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> Any empty tile on own side\n";
      cuerpo += $"<color={colorEncabezado}><b>Valour cost:</b></color> {costoPM}\n";
      cuerpo += $"<color={colorEncabezado}><b>On cast:</b></color> teleports to target tile\n";
      cuerpo += $"<color={colorEncabezado}><b>Self:</b></color> gains Hidden (2), Evasion ({evasionGanada}), removes debuffs\n";
      cuerpo += terminaTurno
        ? $"<color={colorEncabezado}><b>Turn flow:</b></color> ends turn"
        : $"<color={colorEncabezado}><b>Turn flow:</b></color> does not end turn";
    }
    else if (esPortugues)
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Utilidade de mobilidade\n";
      cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> Qualquer celula vazia do proprio lado\n";
      cuerpo += $"<color={colorEncabezado}><b>Custo Valentia:</b></color> {costoPM}\n";
      cuerpo += $"<color={colorEncabezado}><b>Ao usar:</b></color> teleporta para a celula alvo\n";
      cuerpo += $"<color={colorEncabezado}><b>Proprio:</b></color> ganha Escondido (2), Evasao ({evasionGanada}), remove debuffs\n";
      cuerpo += terminaTurno
        ? $"<color={colorEncabezado}><b>Fluxo de turno:</b></color> termina o turno"
        : $"<color={colorEncabezado}><b>Fluxo de turno:</b></color> nao termina o turno";
    }
    else
    {
      cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Utilidad de movilidad\n";
      cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> Cualquier casilla vacia de tu lado\n";
      cuerpo += $"<color={colorEncabezado}><b>Costo Valentía:</b></color> {costoPM}\n";
      cuerpo += $"<color={colorEncabezado}><b>Al lanzarla:</b></color> se teletransporta a la casilla objetivo\n";
      cuerpo += $"<color={colorEncabezado}><b>Propio:</b></color> gana Escondido (2), Evasion ({evasionGanada}), remueve debuffs\n";
      cuerpo += terminaTurno
        ? $"<color={colorEncabezado}><b>Flujo de turno:</b></color> termina turno"
        : $"<color={colorEncabezado}><b>Flujo de turno:</b></color> no termina turno";
    }

    string subtitulo = esIngles
      ? "Moves to a cell, removes debuffs and returns to stealth."
      : esPortugues
        ? "Move para uma celula, remove debuffs e volta a furtividade."
        : "Se mueve a una casilla, remueve debuffs y vuelve al sigilo.";
    string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
    txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
    txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
    txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
    txtDescripcion += cuerpo;

    bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
    if (!mostrarProximoNivel)
    {
      return;
    }

    if (esIngles)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: -1 cooldown.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Evasion gained.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (does not end turn) or Option B (-1 Valour Cost).</color>"; }
    }
    else if (esPortugues)
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: -1 recarga.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Evasao ganha.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (nao termina o turno) ou Opcao B (-1 custo de Valentia).</color>"; }
    }
    else
    {
      if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: -1 enfriamiento.</color>"; }
      else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 Evasion ganada.</color>"; }
      else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (no termina turno) u Opción B (-1 costo de Valentía).</color>"; }
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
    scEstaUnidad.GanarEscondido(2);
    scEstaUnidad.estado_evasion = 2;
    if (NIVEL > 2) { scEstaUnidad.estado_evasion++; }
    scEstaUnidad.RemoverfDebuffstBuffs(false); //Remueve todos los debuffs, si los tuviera.

    VFXAplicar(scEstaUnidad.gameObject);    
    scEstaUnidad.TeletransportarACasilla(cas);
    if (NIVEL != 4) { BattleManager.Instance.TerminarTurno(); }
    
   
  }
        void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_HaciaLasSombras");

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
     
      List<Casilla> alCasillasafectadas2 = new List<Casilla>();
      //Casillas Alrededor al origen
      alCasillasafectadas2 = Origen.ObtenerCasillasMismoLado();
      alCasillasafectadas2.Add(Origen); //Agrega la casilla del caster, para que se pueda targetear
    
      foreach(Casilla c in alCasillasafectadas2)
      {
         c.ActivarCapaColorAzul();
        if(c.Presente != null)
        {
            continue;
        }
        
        lCasillasafectadas.Add(c);
       

      }
    
         
    }

   
    

 
}




