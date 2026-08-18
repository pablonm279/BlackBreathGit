using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class EscudoEnergetico : Habilidad
{
   

    [SerializeField] private GameObject VFXenObjetivo;
   
   
     public override void  Awake()
    {
      nombre = "Escudo Energético";
      IDenClase = 6;
      costoAP = 2; //Termina turno
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

      imHab = Resources.Load<Sprite>("imHab/Canalizador_EscudoEnergetico");
    }


    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

        string tituloEs = "Escudo Energetico I";
        string tituloEn = "Energy Shield I";
        string tituloPt = "Escudo Energetico I";
        if (NIVEL == 2) { tituloEs = "Escudo Energetico II"; tituloEn = "Energy Shield II"; }
        if (NIVEL == 3) { tituloEs = "Escudo Energetico III"; tituloEn = "Energy Shield III"; }
        if (NIVEL == 4) { tituloEs = "Escudo Energetico IV a"; tituloEn = "Energy Shield IV a"; }
        if (NIVEL == 5) { tituloEs = "Escudo Energetico IV b"; tituloEn = "Energy Shield IV b"; }
        if (NIVEL == 2) { tituloPt = "Escudo Energetico II"; }
        if (NIVEL == 3) { tituloPt = "Escudo Energetico III"; }
        if (NIVEL == 4) { tituloPt = "Escudo Energetico IV a"; }
        if (NIVEL == 5) { tituloPt = "Escudo Energetico IV b"; }

        int barreraBase = NIVEL > 1 ? 12 : 8;
        int bonusAtaqueReaccion = NIVEL > 2 ? 1 : 0;
        int usosReaccion = NIVEL == 5 ? 3 : 2;
        bool seCancelaConDanio = NIVEL != 4;

        if (esIngles)
        {
            string energia = TerminoDescripcion(TerminoDescripcionId.Energia, "current Energy tier", "Estado_acumularenergia");
            string barrera = TerminoDescripcion(TerminoDescripcionId.Barrera, "Barrier", "Estado_barrera");
            string descargaArcana = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Arcane Discharge");
            string residuo = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, "Energy Residue", "Estado_acumularenergia");
            string bonusAtaqueTextoNormalizado = bonusAtaqueReaccion > 0 ? $" with +{bonusAtaqueReaccion} Attack Roll" : string.Empty;
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) { proximaMejora = "+4 base Barrier."; }
                else if (NIVEL == 2) { proximaMejora = "+1 Attack Roll to the counterattack."; }
                else if (NIVEL == 3) { proximaMejora = "Option A: taking damage no longer removes the reaction.\nOption B: +1 reaction use."; }
            }

            var lineas = new List<LineaDescripcionNormalizada>
            {
                LineaDescripcion("Target", "Self"),
                LineaDescripcion("Effect", $"Gains {barreraBase} + 10 x {energia} as {barrera}."),
                LineaDescripcion("Reaction", "A ranged enemy misses the Channeler."),
                LineaDescripcion("Counterattack", $"Uses {descargaArcana}{bonusAtaqueTextoNormalizado} on enemy and creates 1 {residuo} in a random nearby empty tile.", 1),
                LineaDescripcion("Limit", $"{usosReaccion} reaction uses per cast.", 1)
            };
            if (seCancelaConDanio)
            {
                lineas.Add(LineaDescripcion("Reaction ends", "When the Channeler takes damage, or after all uses are spent.", 1));
            }
            else
            {
                lineas.Add(LineaDescripcion("Reaction ends", "After all uses are spent.", 1));
            }
            lineas.Add(LineaDescripcion("Duration", "Barrier: 1 round."));

            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                tituloEn,
                "Creates a reactive barrier that counters missed ranged attacks.",
                lineas,
                proximaMejora);
            return;
        }

        {
            bool pt = esPortugues;
            string energia = TerminoDescripcion(TerminoDescripcionId.Energia, pt ? "nível de Energia atual" : "nivel de Energía actual", "Estado_acumularenergia");
            string barrera = TerminoDescripcion(TerminoDescripcionId.Barrera, pt ? "Barreira" : "Barrera", "Estado_barrera");
            string descargaArcana = TerminoDescripcion(TerminoDescripcionId.DanioArcano, "Descarga Arcana");
            string residuo = TerminoDescripcion(TerminoDescripcionId.ResiduoEnergetico, pt ? "Resíduo Energético" : "Residuo Energético", "Estado_acumularenergia");
            string bonusAtaqueTextoLocalizado = bonusAtaqueReaccion > 0 ? (pt ? $" com +{bonusAtaqueReaccion} na Rolagem de ataque" : $" con +{bonusAtaqueReaccion} a la Tirada de ataque") : string.Empty;
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) { proximaMejora = pt ? "+4 de Barreira base." : "+4 de Barrera base."; }
                else if (NIVEL == 2) { proximaMejora = pt ? "+1 na Rolagem de ataque do contra-ataque." : "+1 a la Tirada de ataque del contraataque."; }
                else if (NIVEL == 3) { proximaMejora = pt ? "Opção A: sofrer dano não remove mais a reação.\nOpção B: +1 uso da reação." : "Opción A: recibir daño ya no elimina la reacción.\nOpción B: +1 uso de la reacción."; }
            }
            var lineas = new List<LineaDescripcionNormalizada>
            {
                LineaDescripcion(pt ? "Alvo" : "Objetivo", pt ? "Próprio" : "Propio"),
                LineaDescripcion(pt ? "Efeito" : "Efecto", $"{(pt ? "Recebe" : "Obtiene")} {barreraBase} + 10 x {energia} como {barrera}."),
                LineaDescripcion(pt ? "Reação" : "Reacción", pt ? "Um inimigo à distância erra o Canalizador." : "Un enemigo a distancia falla contra el Canalizador."),
                LineaDescripcion(pt ? "Contra-ataque" : "Contraataque", $"{(pt ? "Usa" : "Usa")} {descargaArcana}{bonusAtaqueTextoLocalizado} {(pt ? "contra o inimigo e cria" : "contra el enemigo y crea")} 1 {residuo} {(pt ? "em uma casa vazia próxima aleatória" : "en una casilla vacía cercana aleatoria")}.", 1),
                LineaDescripcion(pt ? "Limite" : "Límite", $"{usosReaccion} {(pt ? "usos da reação por conjuração" : "usos de la reacción por uso")}.", 1)
            };
            lineas.Add(LineaDescripcion(pt ? "Fim da reação" : "Fin de la reacción", seCancelaConDanio ? (pt ? "Quando o Canalizador sofre dano ou quando todos os usos são gastos." : "Cuando el Canalizador recibe daño o se gastan todos los usos.") : (pt ? "Quando todos os usos são gastos." : "Cuando se gastan todos los usos."), 1));
            lineas.Add(LineaDescripcion(pt ? "Duração" : "Duración", pt ? "Barreira: 1 rodada." : "Barrera: 1 ronda."));
            txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
                pt ? tituloPt : tituloEs,
                pt ? "Cria uma barreira reativa que contra-ataca ataques à distância que erram." : "Crea una barrera reactiva que contraataca los ataques a distancia que fallan.",
                lineas,
                proximaMejora);
            return;
        }

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Self\n";
            cuerpo += $"<b>Barrier:</b> {barreraBase} + 10 x current Energy Tier (1 round)\n";
            cuerpo += $"<b>Reaction:</b> On failed enemy projectile, counters with Arcane Discharge";
            if (bonusAtaqueReaccion > 0)
            {
                cuerpo += $" (+{bonusAtaqueReaccion} attack roll)";
            }
            cuerpo += " on enemy and creates 1 Energy Residue nearby\n";
            cuerpo += $"<b>Reaction Uses per cast:</b> {usosReaccion}\n";
            cuerpo += seCancelaConDanio
                ? "<b>Condition:</b> Shield is removed if user takes damage"
                : "<b>Condition:</b> Shield is not removed by incoming damage";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Propria\n";
            cuerpo += $"<b>Barreira:</b> {barreraBase} + 10 x Nivel de Energia atual (1 rodada)\n";
            cuerpo += $"<b>Reacao:</b> Contra projetil inimigo falho, contra-ataca com Descarga Arcana";
            if (bonusAtaqueReaccion > 0)
            {
                cuerpo += $" (+{bonusAtaqueReaccion} na rolagem de ataque)";
            }
            cuerpo += " e gera 1 Residuo Energetico próximo\n";
            cuerpo += $"<b>Usos da reacao por uso:</b> {usosReaccion}\n";
            cuerpo += seCancelaConDanio
                ? "<b>Condicao:</b> O escudo e removido ao receber dano"
                : "<b>Condicao:</b> O escudo nao e removido ao receber dano";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Propia\n";
            cuerpo += $"<b>Barrera:</b> {barreraBase} + 10 x Nivel de Energia actual (1 ronda)\n";
            cuerpo += $"<b>Reacción:</b> Ante proyectil enemigo fallido, contraataca con Descarga Arcana";
            if (bonusAtaqueReaccion > 0)
            {
                cuerpo += $" (+{bonusAtaqueReaccion} a la tirada de ataque)";
            }
            cuerpo += " y genera 1 Residuo Energetico cercano\n";
            cuerpo += $"<b>Usos de la reaccion por casteo:</b> {usosReaccion}\n";
            cuerpo += seCancelaConDanio
                ? "<b>Condición:</b> El escudo se cancela si recibe daño"
                : "<b>Condición:</b> El escudo no se cancela al recibir daño";
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}"
            : esPortugues
                ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}"
                : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentía: {costoPM}";

        txtDescripcion = ConstruirDescripcionEstandar(
            esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
            esIngles
                ? "The Channeler forms a reactive barrier that protects against ranged pressure."
                : esPortugues
                    ? "O Canalizador forma uma barreira reativa que protege contra a pressao a distancia."
                : "El Canalizador forma una barrera reactiva que protege contra la presión a distancia.",
            cuerpo,
            costos,
            "#5dade2");

        string colorEncabezado = "#44d3ec";
        string colorValor = "#ffffff";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string iconoReaccion = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_reaccion\"></voffset></size><space=-0.35em>";
        string iconoEnergia = "<space=0.35em><size=150%><voffset=0.34em><sprite name=\"Estado_acumularenergia\"></voffset></size><space=-0.35em>";
        string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
        string subtituloFormato = esIngles
            ? "Gains Barrier and counters failed enemy projectiles."
            : esPortugues
                ? "Ganha Barreira e contra-ataca projeteis inimigos falhos."
                : "Gana Barrera y contraataca proyectiles enemigos fallidos.";
        string bonusAtaqueTexto = bonusAtaqueReaccion > 0 ? $", +{bonusAtaqueReaccion}" : "";
        string cuerpoFormato = "";
        if (esIngles)
        {
            cuerpoFormato += $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Self buff</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Barrier:</b></color> <color={colorValor}>+{barreraBase} + 10 x current Energy Tier for 1 round</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Reaction:</b></color> <color={colorValor}>{iconoReaccion} On failed enemy projectile: Arcane Discharge{bonusAtaqueTexto} on enemy and creates {iconoEnergia} 1 Energy Residue nearby</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Uses:</b></color> <color={colorValor}>{usosReaccion} reactions per cast</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Condition:</b></color> <color={colorValor}>{(seCancelaConDanio ? "Removed when user takes damage" : "Not removed by incoming damage")}</color>";
        }
        else if (esPortugues)
        {
            cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Barreira:</b></color> <color={colorValor}>+{barreraBase} + 10 x Nivel de Energia atual por 1 rodada</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Reacao:</b></color> <color={colorValor}>{iconoReaccion} Se projetil inimigo falha: Descarga Arcana{bonusAtaqueTexto}, cria {iconoEnergia} 1 Residuo Energetico proximo</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Usos:</b></color> <color={colorValor}>{usosReaccion} reacoes por uso</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Condicao:</b></color> <color={colorValor}>{(seCancelaConDanio ? "Remove ao receber dano" : "Nao remove ao receber dano")}</color>";
        }
        else
        {
            cuerpoFormato += $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Auto buff</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Barrera:</b></color> <color={colorValor}>+{barreraBase} + 10 x Nivel de Energia actual por 1 ronda</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Reacción:</b></color> <color={colorValor}>{iconoReaccion} Si proyectil enemigo falla: Descarga Arcana{bonusAtaqueTexto}, crea {iconoEnergia} 1 Residuo Energetico cercano</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Usos:</b></color> <color={colorValor}>{usosReaccion} reacciones por uso</color>\n";
            cuerpoFormato += $"<color={colorEncabezado}><b>Condición:</b></color> <color={colorValor}>{(seCancelaConDanio ? "Se remueve al recibir daño" : "No se remueve al recibir daño")}</color>";
        }

        txtDescripcion =
            $"<size=115%><color=#5dade2><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoAP} {iconoAP}</color>\n\n" +
            $"<color=#8f8f8f><i>{subtituloFormato}</i></color>\n\n" +
            "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n" +
            cuerpoFormato;

        bool mostrarProximoNivel = EsEscenaCampaña() && CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel)
        {
            return;
        }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +4 base Barrier.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 attack roll on counter discharge.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no cancel on damage) or Option B (+1 reaction use).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +4 Barreira base.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 na rolagem de ataque da descarga de reacao.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (nao remove com dano) ou Opcao B (+1 uso de reacao).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +4 Barrera base.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 a la tirada de ataque de la descarga de reacción.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (no se cancela por daño) u Opción B (+1 uso de reacción).</color>"; }
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
    
    

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {

    if(obj is Unidad) //Acá van los efectos a Unidades.
     {

       Unidad objetivo = (Unidad)obj;
         VFXAplicar(objetivo.gameObject);
      ClaseCanalizador scCana = (ClaseCanalizador)objetivo;
      int barreraBase = NIVEL > 1 ? 12 : 8;
      int barreraEnergia = 10 * scCana.ObtenerEnergia();
       /////////////////////////////////////////////
       //BUFF ---- Así se aplica un buff/debuff
       Buff buff = new Buff();
       buff.buffNombre = "Escudo Energético";
       buff.boolfDebufftBuff = true;
       buff.DuracionBuffRondas = 2;
       buff.cantBarrera += barreraBase + barreraEnergia;
       buff.AplicarBuff(objetivo);
       // Agrega el componente Buff al objeto objetivo y asigna la configuración del buff
       Buff buffComponent = ComponentCopier.CopyComponent(buff, objetivo.gameObject);
       objetivo.Marcar(0);

       //Agrega la reacción 
       ReaccionEscudoEnergetico reaccion = new ReaccionEscudoEnergetico();
       reaccion.NIVEL = NIVEL;
       reaccion.permanente = false;
       reaccion.nombre = "Escudo Energético";
       ReaccionEscudoEnergetico reaccionPosturaDefensiva = ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

       //Usarla termina el turno
      // BattleManager.Instance.TerminarTurno();
     }
    
    
    }
    
    void VFXAplicar(GameObject objetivo)
    {
      VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_EscudoEnergetico");
      if (VFXenObjetivo == null || objetivo == null) { return; }

    GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity /*objetivo.transform.rotation*/);
    vfx.transform.parent = objetivo.transform;
    VFXSoloSonido.OcultarVisuales(vfx);
     
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





