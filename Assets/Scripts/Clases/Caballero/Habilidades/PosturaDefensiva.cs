using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PosturaDefensiva : Habilidad
{
    private const int PausaClaridadMs = 300;
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
    public override void  Awake()
    {
      nombre = "Postura Defensiva";
      IDenClase = 7;
      costoAP = 1; //Termina turno
      costoPM = 0;
      Usuario = this.gameObject;
      scEstaUnidad = Usuario.GetComponent<Unidad>();
      esZonal = false;
      enArea = 0;
      esforzable = 0;
      esCargable = false;
      esMelee = false;
      esHostil = false;
      cooldownMax = 0;
      bAfectaObstaculos = false;
      omitirAnimacionDeUso = true;

      imHab = Resources.Load<Sprite>("imHab/Caballero_PosturaDefensiva");
      ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
      bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
      bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

      int bonoDefensa = 1 + (NIVEL > 1 ? 1 : 0);
      int bonoAtaque = NIVEL > 2 ? 1 : 0;
      int usosReaccion = NIVEL == 5 ? 2 : 1;
      bool seCancelaAlRecibirDanio = NIVEL != 4;
      string colorEncabezado = "#44d3ec";
      string colorValor = "#ffffff";
      string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
      string iconoBuff = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_buff\"></voffset></size><space=-0.35em>";
      string iconoReaccion = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_reaccion\"></voffset></size><space=-0.35em>";
      string costoSuperior = $"{costoAP} {iconoAP}";

      string tituloEs = "Postura Defensiva I";
      string tituloEn = "Defensive Stance I";
      string tituloPt = "Postura Defensiva I";
      if (NIVEL == 2) { tituloEs = "Postura Defensiva II"; tituloEn = "Defensive Stance II"; }
      if (NIVEL == 3) { tituloEs = "Postura Defensiva III"; tituloEn = "Defensive Stance III"; }
      if (NIVEL == 4) { tituloEs = "Postura Defensiva IV a"; tituloEn = "Defensive Stance IV a"; }
      if (NIVEL == 5) { tituloEs = "Postura Defensiva IV b"; tituloEn = "Defensive Stance IV b"; }
      if (NIVEL == 2) { tituloPt = "Postura Defensiva II"; }
      if (NIVEL == 3) { tituloPt = "Postura Defensiva III"; }
      if (NIVEL == 4) { tituloPt = "Postura Defensiva IV a"; }
      if (NIVEL == 5) { tituloPt = "Postura Defensiva IV b"; }

      if (esIngles)
      {
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
        string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Attack");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "+1 Defense."; }
          else if (NIVEL == 2) { proximaMejora = "+1 Attack while in the stance."; }
          else if (NIVEL == 3) { proximaMejora = "Option A: taking damage no longer removes the reaction.\nOption B: +1 reaction use."; }
        }

        string buff = $"+{bonoDefensa} {defensa}";
        if (bonoAtaque > 0)
        {
          buff += $", +{bonoAtaque} {ataque}";
        }

        txtDescripcion = ConstruirDescripcionNormalizadaIngles(
          tituloEn,
          "Assumes a defensive stance and prepares a melee counterattack.",
          new[]
          {
            LineaDescripcion("Target", "Self"),
            LineaDescripcion("Effect", $"Gains {buff} (2 turns)."),
            LineaDescripcion("Reaction", "Counterattacks with Vertical Cut when an enemy misses a melee attack.", 1),
            LineaDescripcion("Limit", $"{usosReaccion} use{(usosReaccion == 1 ? string.Empty : "s")}.", 1),
            LineaDescripcion("Ends", seCancelaAlRecibirDanio ? "The reaction ends after taking damage." : "Taking damage does not end the reaction.", 1),
            LineaDescripcion("Use", "Ends the turn")
          },
          proximaMejora);
        return;
      }

      if (esPortugues)
      {
        string defesa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defesa", "IconoDefensa");
        string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Ataque");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nível: +1 Defesa."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nível: +1 Ataque enquanto estiver na postura."; }
          else if (NIVEL == 3) { proximaMejora = "Opção A: sofrer dano não remove mais a reação.\nOpção B: +1 uso da reação."; }
        }
        string buff = $"+{bonoDefensa} {defesa}";
        if (bonoAtaque > 0) { buff += $", +{bonoAtaque} {ataque}"; }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloPt, "Assume uma postura defensiva e prepara um contra-ataque corpo a corpo.", new[]
        {
          LineaDescripcion("Alvo", "O próprio usuário"),
          LineaDescripcion("Efeito", $"Recebe {buff} (2 turnos)."),
          LineaDescripcion("Reação", "Contra-ataca com Corte Vertical quando um inimigo erra um ataque corpo a corpo.", 1),
          LineaDescripcion("Limite", $"{usosReaccion} uso{(usosReaccion == 1 ? string.Empty : "s")}.", 1),
          LineaDescripcion("Termina", seCancelaAlRecibirDanio ? "A reação termina após sofrer dano." : "Sofrer dano não encerra a reação.", 1),
          LineaDescripcion("Uso", "Encerra o turno")
        }, proximaMejora);
        return;
      }

      {
        string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defensa", "IconoDefensa");
        string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Ataque");
        string proximaMejora = null;
        if (DebeMostrarProximaMejoraDescripcion())
        {
          if (NIVEL < 2) { proximaMejora = "Próximo nivel: +1 Defensa."; }
          else if (NIVEL == 2) { proximaMejora = "Próximo nivel: +1 Ataque mientras está en la postura."; }
          else if (NIVEL == 3) { proximaMejora = "Opción A: recibir daño ya no elimina la reacción.\nOpción B: +1 uso de la reacción."; }
        }
        string buff = $"+{bonoDefensa} {defensa}";
        if (bonoAtaque > 0) { buff += $", +{bonoAtaque} {ataque}"; }
        txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(tituloEs, "Adopta una postura defensiva y prepara un contraataque cuerpo a cuerpo.", new[]
        {
          LineaDescripcion("Objetivo", "Uno mismo"),
          LineaDescripcion("Efecto", $"Obtiene {buff} (2 turnos)."),
          LineaDescripcion("Reacción", "Contraataca con Corte Vertical cuando un enemigo falla un ataque cuerpo a cuerpo.", 1),
          LineaDescripcion("Límite", $"{usosReaccion} uso{(usosReaccion == 1 ? string.Empty : "s")}.", 1),
          LineaDescripcion("Termina", seCancelaAlRecibirDanio ? "La reacción termina después de recibir daño." : "Recibir daño no termina la reacción.", 1),
          LineaDescripcion("Uso", "Termina el turno")
        }, proximaMejora);
        return;
      }

      string cuerpo = "";
      if (esIngles)
      {
        cuerpo += "<b>Type:</b> Self Buff + Reaction\n";
        cuerpo += $"<b>Buff (2 turns):</b> +{bonoDefensa} Defense";
        if (bonoAtaque > 0)
        {
          cuerpo += $", +{bonoAtaque} Attack";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Reaction:</b> Counterattack with Vertical Cut when an enemy misses a melee attack ({usosReaccion} use/s)\n";
        cuerpo += seCancelaAlRecibirDanio
          ? "<b>Reaction cancel:</b> removed when taking damage"
          : "<b>Reaction cancel:</b> does not get removed when taking damage";
      }
      else if (esPortugues)
      {
        cuerpo += "<b>Tipo:</b> Auto Buff + Reacao\n";
        cuerpo += $"<b>Buff (2 turnos):</b> +{bonoDefensa} Defesa";
        if (bonoAtaque > 0)
        {
          cuerpo += $", +{bonoAtaque} Ataque";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Reacao:</b> contra-ataca com Corte Vertical quando um inimigo erra um ataque corpo a corpo ({usosReaccion} uso/s)\n";
        cuerpo += seCancelaAlRecibirDanio
          ? "<b>Cancelamento da reacao:</b> removida ao receber dano"
          : "<b>Cancelamento da reacao:</b> nao e removida ao receber dano";
      }
      else
      {
        cuerpo += "<b>Tipo:</b> Auto Buff + Reaccion\n";
        cuerpo += $"<b>Buff (2 turnos):</b> +{bonoDefensa} Defensa";
        if (bonoAtaque > 0)
        {
          cuerpo += $", +{bonoAtaque} Ataque";
        }
        cuerpo += "\n";
        cuerpo += $"<b>Reacción:</b> contraataca con Corte Vertical cuando un enemigo falla un ataque melee ({usosReaccion} uso/s)\n";
        cuerpo += seCancelaAlRecibirDanio
          ? "<b>Cancelación de reacción:</b> se elimina al recibir daño"
          : "<b>Cancelación de reacción:</b> no se elimina al recibir daño";
      }

      string costos = esIngles
        ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
        : esPortugues
          ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}"
        : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentía: {costoPM}";

      txtDescripcion = ConstruirDescripcionEstandar(
        esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
        esIngles
          ? "The Knight braces for incoming melee and answers with punishing counters."
          : esPortugues
            ? "O Cavaleiro se prepara para receber ataques corpo a corpo e responde com contra-ataques."
          : "El Caballero se planta para recibir melee y responder con contraataques.",
        cuerpo,
        costos,
        "#5dade2");

      string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
      string subtitulo = esIngles
        ? "Defensive buff; ends turn and prepares a melee counter."
        : esPortugues
          ? "Buff defensivo; encerra o turno e prepara um contra-ataque melee."
          : "Buff defensivo; termina el turno y prepara un contraataque melee.";
      string ataqueBuff = bonoAtaque > 0
        ? esIngles ? $", +{bonoAtaque} Attack" : esPortugues ? $", +{bonoAtaque} Ataque" : $", +{bonoAtaque} Ataque"
        : string.Empty;

      string cuerpoFormato = "";
      if (esIngles)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Self buff + Reaction</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Target:</b></color> <color={colorValor}>Self</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Buff:</b></color> <color={colorValor}>{iconoBuff} 2 turns: +{bonoDefensa} Defense{ataqueBuff}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Reaction:</b></color> <color={colorValor}>{iconoReaccion} Counterattack with Vertical Cut when an enemy misses a melee attack. Uses: {usosReaccion}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Cancel:</b></color> <color={colorValor}>{(seCancelaAlRecibirDanio ? "Removed when taking damage" : "Not removed when taking damage")}</color>";
      }
      else if (esPortugues)
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff + Reação</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Alvo:</b></color> <color={colorValor}>O próprio usuário</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Buff:</b></color> <color={colorValor}>{iconoBuff} 2 turnos: +{bonoDefensa} Defesa{ataqueBuff}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Reação:</b></color> <color={colorValor}>{iconoReaccion} Contra-ataca com Corte Vertical quando um inimigo erra um ataque melee. Usos: {usosReaccion}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Cancelamento:</b></color> <color={colorValor}>{(seCancelaAlRecibirDanio ? "Removida ao receber dano" : "Não é removida ao receber dano")}</color>";
      }
      else
      {
        cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff + Reacción</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Objetivo:</b></color> <color={colorValor}>Uno mismo</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Buff:</b></color> <color={colorValor}>{iconoBuff} 2 turnos: +{bonoDefensa} Defensa{ataqueBuff}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Reacción:</b></color> <color={colorValor}>{iconoReaccion} Contraataca con Corte Vertical cuando un enemigo falla un ataque melee. Usos: {usosReaccion}</color>\n";
        cuerpoFormato += $"<color={colorEncabezado}><b>Cancelación:</b></color> <color={colorValor}>{(seCancelaAlRecibirDanio ? "Se elimina al recibir daño" : "No se elimina al recibir daño")}</color>";
      }

      txtDescripcion =
        $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n" +
        $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n" +
        "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
        cuerpoFormato;

      bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
      if (!mostrarProximoNivel)
      {
        return;
      }

      if (esIngles)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense buff.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Attack buff during stance.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no cancel on hit) or Option B (+1 reaction use).</color>"; }
      }
      else if (esPortugues)
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 no buff de Defesa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 no buff de Ataque durante a postura.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (nao cancela ao receber golpe) ou Opcao B (+1 uso de reacao).</color>"; }
      }
      else
      {
        if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al buff de Defensa.</color>"; }
        else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al buff de Ataque durante la postura.</color>"; }
        else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (no se cancela al recibir golpe) u Opción B (+1 uso de reacción).</color>"; }
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
    
    

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

    if(obj is Unidad) //Acá van los efectos a Unidades.
     {

       Unidad objetivo = (Unidad)obj;
       await BattleManager.DelayCombateAsync(PausaClaridadMs);
       BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");

      VFXAplicar(objetivo.gameObject);
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Postura Defensiva";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantDefensa += 1;
       if(NIVEL > 1){ buff.cantDefensa += 1;}
       if(NIVEL > 2){ buff.cantAtaque += 1;}
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       objetivo.Marcar(0);

       //Agrega la reacción 
       ReaccionPosturaDefensiva reaccion = new ReaccionPosturaDefensiva();
       reaccion.NIVEL = NIVEL;
       reaccion.permanente = false;
       reaccion.nombre = "Postura Defensiva";
       ReaccionPosturaDefensiva reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

       if (objetivo is ClaseCaballero caballero)
       {
         caballero.NotificarInicioPosturaDefensiva();
       }

       //Usarla termina el turno
       await BattleManager.DelayCombateAsync(PausaClaridadMs);
       BattleManager.Instance.TerminarTurno();
     }
    
    
    }
    
     void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PosturaDefensiva");

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
      //Cualquier objetivo en 1 de alcance 3 de ancho
      lObjetivosPosibles.Clear();
    
      List<Casilla> lCasillasafectadas = Origen.ObtenerCasillasMismoLado();
     
    
      foreach(Casilla c in lCasillasafectadas)
      {
       
        
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
            if(c.Presente.GetComponent<Unidad>() == scEstaUnidad)
            {
             c.ActivarCapaColorAzul();
             lObjetivosPosibles.Add(c.Presente.GetComponent<Unidad>());
            }
        }

      }
    

      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);

      foreach(Unidad uni in BattleManager.Instance.lUnidadesPosiblesHabilidadActiva)
      {
      }

      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
      BattleManager.Instance.lObstaculosPosiblesHabilidadActiva = new List<Obstaculo>(lObstaculosPosibles);
       
    }

   
 
}







