using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PilaresDeLuz : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] int usosBatalla;
  
    
    public override void  Awake()
    {
      nombre = "Pilares De Luz";
      IDenClase = 5;
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
      poneTrampas = false;
      poneObstaculo = true;
      
     
      
      imHab = Resources.Load<Sprite>("imHab/Purificadora_PilaresDeLuz");
      ActualizarDescripcion();
    }
    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
      var statsUI = ObtenerStatsDescripcionUI();

      int poderActual = statsUI.Poder;
      int vidaPilar = NIVEL > 1 ? 20 : 14;
      int resistenciaDanio = NIVEL == 4 ? 3 : 0;
      int cantidadPilares = NIVEL == 5 ? 3 : 2;
      int duracionTurnos = NIVEL == 5 ? 4 : 3;
      int bonusDanio = NIVEL > 2 ? 2 : 0;
      string rangoDanioPilarEs = FormatearRangoDados(1, 4, bonusDanio);

      string tituloEs = "Pilares de Luz I";
      string tituloEn = "Pillars of Light I";
      string tituloPt = "Pilares de Luz I";
      if (NIVEL == 2) { tituloEs = "Pilares de Luz II"; tituloEn = "Pillars of Light II"; }
      if (NIVEL == 3) { tituloEs = "Pilares de Luz III"; tituloEn = "Pillars of Light III"; }
      if (NIVEL == 4) { tituloEs = "Pilares de Luz IV a"; tituloEn = "Pillars of Light IV a"; }
      if (NIVEL == 5) { tituloEs = "Pilares de Luz IV b"; tituloEn = "Pillars of Light IV b"; }
      if (NIVEL == 2) { tituloPt = "Pilares de Luz II"; }
      if (NIVEL == 3) { tituloPt = "Pilares de Luz III"; }
      if (NIVEL == 4) { tituloPt = "Pilares de Luz IV a"; }
      if (NIVEL == 5) { tituloPt = "Pilares de Luz IV b"; }

      string danioPilarEs = bonusDanio > 0
        ? $"{rangoDanioPilarEs} + <color=#ea0606>Pod ({poderActual})</color>"
        : $"{rangoDanioPilarEs} + <color=#ea0606>Pod ({poderActual})</color>";
      string danioPilarEn = bonusDanio > 0
        ? $"1d4 + {bonusDanio} + <color=#ea0606>Power ({poderActual})</color>"
        : $"1d4 + <color=#ea0606>Power ({poderActual})</color>";
      string danioPilarPt = bonusDanio > 0
        ? $"1d4 + {bonusDanio} + <color=#ea0606>Poder ({poderActual})</color>"
        : $"1d4 + <color=#ea0606>Poder ({poderActual})</color>";

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Ranged (3 range)\n";
        cuerpo += "<b>Target:</b> 1 tile in range\n";
        cuerpo += $"<b>Summon:</b> {cantidadPilares} pillars (selected tile and adjacent tiles in the same column if free)\n";
        cuerpo += $"<b>Pillar Stats:</b> HP {vidaPilar}";
        if (resistenciaDanio > 0)
        {
          cuerpo += $", Damage Resistance {resistenciaDanio}";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Retaliation:</b> {danioPilarEn} | <b>Type:</b> Divine (x2 vs Undead/Ethereal)\n";
        cuerpo += $"<b>Duration:</b> {duracionTurnos} turns";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Alcance (3 de alcance)\n";
        cuerpo += "<b>Alvo:</b> 1 celula no alcance\n";
        cuerpo += $"<b>Invocacao:</b> {cantidadPilares} pilares (celula selecionada e celulas adjacentes na mesma coluna se estiverem livres)\n";
        cuerpo += $"<b>Status do pilar:</b> Vida {vidaPilar}";
        if (resistenciaDanio > 0)
        {
          cuerpo += $", Resistencia a dano {resistenciaDanio}";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Contra-ataque:</b> {danioPilarPt} | <b>Tipo:</b> Divino (x2 vs Morto-vivo/Etereo)\n";
        cuerpo += $"<b>Duracao:</b> {duracionTurnos} turnos";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Rango (3 alcance)\n";
        cuerpo += "<b>Objetivo:</b> 1 casilla en rango\n";
        cuerpo += $"<b>Invocación:</b> {cantidadPilares} pilares (casilla seleccionada y casillas adyacentes en la misma columna si estan libres)\n";
        cuerpo += $"<b>Stats del pilar:</b> Vida {vidaPilar}";
        if (resistenciaDanio > 0)
        {
          cuerpo += $", Resistencia al daño {resistenciaDanio}";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Contraataque:</b> {danioPilarEs} | <b>Tipo:</b> Divino (x2 vs Nomuerto/Etéreo)\n";
        cuerpo += $"<b>Duración:</b> {duracionTurnos} turnos";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "Creates divine obstacles that damages enemies when attacked."
          : esPortugues
            ? "Cria obstáculos divinos que causam dano aos inimigos quando atacados."
          : "Crea obstáculos divinos que dañan a quienes los ataquen.",
        cuerpo,
        costos,
        "#5dade2");

      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string colorPoder = "#2aa6c8";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
      string costoSuperior = cooldownMax > 0
        ? $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}"
        : $"{costoAP} {iconoAP}";
      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Creates holy pillars that damages enemies when attacked."
        : esPortugues
          ? "Cria pilares sagrados que causam dano quando atacados."
          : "Crea pilares sagrados que dañan al ser atacados.";
      string danioPilar = $"{rangoDanioPilarEs} + <color={colorPoder}>{(esIngles ? "Power" : esPortugues ? "Poder" : "Poder")} ({poderActual})</color>";
      string cuerpoNuevo = "";
      if (esIngles)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Ranged obstacle (3 range)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>1 free tile in range</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Summon:</b></color> <color={colorValor}>{cantidadPilares} pillars in the same column if free</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Pillar:</b></color> <color={colorValor}>{vidaPilar} HP";
        if (resistenciaDanio > 0) { cuerpoNuevo += $", {resistenciaDanio} damage resistance"; }
        cuerpoNuevo += $", {duracionTurnos} turns</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>On hit:</b></color> <color={colorValor}>{danioPilar}. Type: Divine (x2 vs Undead/Ethereal)</color>";
      }
      else if (esPortugues)
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Obstáculo a alcance (3 de alcance)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>1 celula livre no alcance</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Invocacao:</b></color> <color={colorValor}>{cantidadPilares} pilares na mesma coluna se livres</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Pilar:</b></color> <color={colorValor}>{vidaPilar} Vida";
        if (resistenciaDanio > 0) { cuerpoNuevo += $", {resistenciaDanio} resistencia a dano"; }
        cuerpoNuevo += $", {duracionTurnos} turnos</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Ao ser atingido:</b></color> <color={colorValor}>{danioPilar}. Tipo: Divino (x2 vs Morto-vivo/Etereo)</color>";
      }
      else
      {
        cuerpoNuevo += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Obstáculo a rango (3 alcance)</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>1 casilla libre en rango</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Invocación:</b></color> <color={colorValor}>{cantidadPilares} pilares en la misma columna si estan libres</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Pilar:</b></color> <color={colorValor}>{vidaPilar} Vida";
        if (resistenciaDanio > 0) { cuerpoNuevo += $", {resistenciaDanio} resistencia al daño"; }
        cuerpoNuevo += $", {duracionTurnos} turnos</color>\n";
        cuerpoNuevo += $"<color={colorEncabezado}><b>Al recibir golpe:</b></color> <color={colorValor}>{danioPilar}. Tipo: Divino (x2 vs Nomuerto/Etéreo)</color>";
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoNuevo;

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
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +6 pillar HP.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 pillar retaliation damage.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+3 Damage Resistance) or Option B (+1 pillar).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +6 Vida do pilar.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano no contra-ataque do pilar.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+3 Resistencia a dano) ou Opcao B (+1 pilar).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +6 Vida de pilar.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 daño de contraataque del pilar.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+3 Resistencia al daño) u Opción B (+1 pilar).</color>"; }
      }
    }
    void Start()
    {
      
    }

    Casilla Origen;
    private Casilla casillaPreviewActual;
    private readonly List<Casilla> casillasPreviewPilares = new List<Casilla>();

    public override void ActualizarPreviewCasilla(Casilla casilla)
    {
      if (casilla == casillaPreviewActual
        || casilla == null
        || casilla.Presente != null
        || !lCasillasafectadas.Contains(casilla))
      {
        if (casilla != casillaPreviewActual)
        {
          LimpiarPreviewCasilla();
        }
        return;
      }

      LimpiarPreviewCasilla();
      casillaPreviewActual = casilla;
      casillasPreviewPilares.Add(casilla);

      int cantidadQuedan = NIVEL == 5 ? 2 : 1;
      foreach (Casilla adyacente in casilla.ObtenerCasillasAdyacentesEnColumna())
      {
        if (adyacente.Presente == null && cantidadQuedan > 0)
        {
          casillasPreviewPilares.Add(adyacente);
          cantidadQuedan--;
        }
      }

      foreach (Casilla casillaPreview in casillasPreviewPilares)
      {
        casillaPreview.MostrarPreviewIconoHabilidad(imHab);
      }
    }

    public override void LimpiarPreviewCasilla()
    {
      foreach (Casilla casillaPreview in casillasPreviewPilares)
      {
        if (casillaPreview != null)
        {
          casillaPreview.OcultarPreviewIconoHabilidad();
        }
      }

      casillasPreviewPilares.Clear();
      casillaPreviewActual = null;
    }

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
      
       
     GameObject obst1 = Instantiate(BattleManager.Instance.contenedorPrefabs.PurificadoraPilarDeLuz);
     obst1.GetComponent<PilarDeLuz>().oName = "Pilar De Luz";
     obst1.GetComponent<PilarDeLuz>().hpMax = 14.0f;
     if(NIVEL > 1){ obst1.GetComponent<PilarDeLuz>().hpMax += 6;}
     obst1.GetComponent<PilarDeLuz>().iDureza = 0.0f;
     if(NIVEL == 4){ obst1.GetComponent<PilarDeLuz>().iDureza += 3;}
     obst1.GetComponent<PilarDeLuz>().hpCurr =  obst1.GetComponent<PilarDeLuz>().hpMax;
     obst1.GetComponent<PilarDeLuz>().bPermiteAtacarDetras = true;
     obst1.GetComponent<PilarDeLuz>().NIVEL = NIVEL;
     obst1.GetComponent<PilarDeLuz>().scCreador = Usuario.GetComponent<ClasePurificadora>();
     obst1.GetComponent<PilarDeLuz>().intDuracionTurnos = 3;
     if(NIVEL == 5){ obst1.GetComponent<PilarDeLuz>().intDuracionTurnos += 1;}


     cas.PonerObjetoEnCasilla(obst1);
     int cantidadQuedan = 1;
     if(NIVEL == 5){ cantidadQuedan += 1;}
     foreach(Casilla ady in cas.ObtenerCasillasAdyacentesEnColumna())
     {
      if(ady.Presente == null && cantidadQuedan > 0)
      { 
        cantidadQuedan--;
        GameObject obst2 = Instantiate(BattleManager.Instance.contenedorPrefabs.PurificadoraPilarDeLuz);
        obst2.GetComponent<PilarDeLuz>().oName = "Pilar De Luz";
        obst2.GetComponent<PilarDeLuz>().hpMax = 14.0f;
        if(NIVEL > 1){ obst2.GetComponent<PilarDeLuz>().hpMax += 6;}
        obst2.GetComponent<PilarDeLuz>().iDureza = 0.0f;
        obst2.GetComponent<PilarDeLuz>().hpCurr =  obst2.GetComponent<PilarDeLuz>().hpMax;
        if(NIVEL == 4){ obst2.GetComponent<PilarDeLuz>().iDureza += 3;}
        obst2.GetComponent<PilarDeLuz>().bPermiteAtacarDetras = true;
        obst2.GetComponent<PilarDeLuz>().NIVEL = NIVEL;
        obst2.GetComponent<PilarDeLuz>().scCreador = Usuario.GetComponent<ClasePurificadora>();
        obst2.GetComponent<PilarDeLuz>().intDuracionTurnos = 3;
        if(NIVEL == 5){ obst2.GetComponent<PilarDeLuz>().intDuracionTurnos += 1;}
        ady.PonerObjetoEnCasilla(obst2);
      }
     }
     

     
       BattleManager.Instance.HabilidadActiva = null;// desactiva la habilidad activa, para que no se pueda usar de nuevo
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
      lCasillasafectadas = Origen.ObtenerCasillasAlrededor(3);
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
