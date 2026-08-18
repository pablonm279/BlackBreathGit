using System.Collections.Generic;
using UnityEngine;

public class Riposte : Habilidad
{
    private const int PausaClaridadMs = 300;
    private readonly List<Unidad> objetivosPosibles = new List<Unidad>();

    public override void Awake()
    {
        nombre = "Riposte";
        IDenClase = 4;
        costoAP = 2;
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 0;
        esCargable = false;
        esMelee = false;
        esHostil = false;
        cooldownMax = 0;
        bAfectaObstaculos = false;

        imHab = Resources.Load<Sprite>("imHab/Duelista_Riposte");
        if (imHab == null)
        {
            imHab = Resources.Load<Sprite>("imHab/Duelista_habilidad");
        }

        ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
        int bonoDefensa = NIVEL > 1 ? 3 : 2;
        int usos = NIVEL == 5 ? 2 : 1;
        int bonusAtaqueReaccion = NIVEL > 2 ? 0 : -1;
        bool seCancelaConDanio = NIVEL != 4;

        string titulo = NIVEL switch
        {
            2 => "Riposte II",
            3 => "Riposte III",
            4 => "Riposte IV a",
            5 => "Riposte IV b",
            _ => "Riposte I"
        };

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}";
        string contraataque = ConstruirTextoContraataqueRiposte(bonusAtaqueReaccion, -2, esIngles, esPortugues);

        if (esIngles)
        {
            string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) proximaMejora = "+1 Defense on the intercepted hit.";
                else if (NIVEL == 2) proximaMejora = "Counterattack loses its -1 Attack Roll penalty.";
                else if (NIVEL == 3) proximaMejora = "Option A: Taking damage does not cancel Riposte. Option B: +1 use per turn.";
            }

            string contraataqueNormalizado = bonusAtaqueReaccion < 0
                ? $"Thrust ({bonusAtaqueReaccion} Attack Roll, -2 damage)"
                : "Thrust (-2 damage)";
            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                titulo,
                "Prepares to intercept melee attacks and counterattack misses.",
                new[]
                {
                    LineaDescripcion("Target", "Self"),
                    LineaDescripcion("Effect", "Prepares Riposte."),
                    LineaDescripcion("Reaction", $"When an adjacent ally is targeted by a single-target melee attack, swaps with the ally, becomes the target and gains +{bonoDefensa} {defensa} for that hit; when an enemy misses a melee attack against the Duelist, counterattacks with {contraataqueNormalizado}.", 1),
                    LineaDescripcion("Limit", $"{usos} use{(usos == 1 ? "" : "s")}.", 1),
                    LineaDescripcion("Ends", seCancelaConDanio ? "After taking damage." : "Taking damage does not end it.", 1),
                    LineaDescripcion("Use", "Ends the turn")
                },
                proximaMejora);
            return;
        }
        if(esPortugues){string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defesa","IconoDefensa");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: +1 Defesa no golpe interceptado.":NIVEL==2?"Próximo nível: o contra-ataque perde a penalidade de -1 na Rolagem de Ataque.":NIVEL==3?"Opção A: sofrer dano não cancela Riposte. Opção B: +1 uso por turno.":null;string contra=bonusAtaqueReaccion<0?$"Estocada ({bonusAtaqueReaccion} na Rolagem de Ataque, -2 de dano)":"Estocada (-2 de dano)";txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Prepara-se para interceptar ataques corpo a corpo e contra-atacar erros.",new[]{LineaDescripcion("Alvo","A própria Duelista"),LineaDescripcion("Efeito","Prepara Riposte."),LineaDescripcion("Reação",$"Quando um aliado adjacente é alvo de um ataque corpo a corpo de alvo único, troca de posição com ele, torna-se o alvo e recebe +{bonoDefensa} {def} para esse golpe; quando um inimigo erra um ataque corpo a corpo contra a Duelista, contra-ataca com {contra}.",1),LineaDescripcion("Limite",$"{usos} uso{(usos==1?"":"s")}.",1),LineaDescripcion("Termina",seCancelaConDanio?"Após sofrer dano.":"Sofrer dano não encerra o efeito.",1),LineaDescripcion("Uso","Encerra o turno")},prox);return;}
        {string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defensa","IconoDefensa");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: +1 Defensa en el golpe interceptado.":NIVEL==2?"Próximo nivel: el contraataque pierde su penalización de -1 a la Tirada de Ataque.":NIVEL==3?"Opción A: recibir daño no cancela Riposte. Opción B: +1 uso por turno.":null;string contra=bonusAtaqueReaccion<0?$"Estocada ({bonusAtaqueReaccion} a la Tirada de Ataque, -2 de daño)":"Estocada (-2 de daño)";txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Se prepara para interceptar ataques cuerpo a cuerpo y contraatacar fallos.",new[]{LineaDescripcion("Objetivo","La propia Duelista"),LineaDescripcion("Efecto","Prepara Riposte."),LineaDescripcion("Reacción",$"Cuando un aliado adyacente es objetivo de un ataque cuerpo a cuerpo de un solo objetivo, intercambia posiciones, se convierte en el objetivo y obtiene +{bonoDefensa} {def} para ese golpe; cuando un enemigo falla un ataque cuerpo a cuerpo contra la Duelista, contraataca con {contra}.",1),LineaDescripcion("Límite",$"{usos} uso{(usos==1?"":"s")}.",1),LineaDescripcion("Termina",seCancelaConDanio?"Después de recibir daño.":"Recibir daño no termina el efecto.",1),LineaDescripcion("Uso","Termina el turno")},prox);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Self reaction\n";
            cuerpo += $"<color={colorEncabezado}><b>Trigger 1:</b></color> adjacent ally is targeted by a single-target enemy melee attack\n";
            cuerpo += $"<color={colorEncabezado}><b>Intercept:</b></color> swap positions, become target, +{bonoDefensa} Defense for that hit\n";
            cuerpo += $"<color={colorEncabezado}><b>Trigger 2:</b></color> enemy misses a melee attack against her; counterattacks with Thrust{contraataque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Uses:</b></color> {usos} per turn\n";
            cuerpo += seCancelaConDanio
                ? $"<color={colorEncabezado}><b>Cancel:</b></color> removed when taking damage\n"
                : $"<color={colorEncabezado}><b>Cancel:</b></color> not removed when taking damage\n";
            cuerpo += $"<color={colorEncabezado}><b>Turn flow:</b></color> ends turn";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Reacao propria\n";
            cuerpo += $"<color={colorEncabezado}><b>Gatilho 1:</b></color> aliado adjacente vira alvo de ataque melee unitario inimigo\n";
            cuerpo += $"<color={colorEncabezado}><b>Intercepcao:</b></color> troca posicoes, vira alvo, +{bonoDefensa} Defesa nesse golpe\n";
            cuerpo += $"<color={colorEncabezado}><b>Gatilho 2:</b></color> inimigo erra ataque melee contra ela; contra-ataca com Estocada base{contraataque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos:</b></color> {usos} por turno\n";
            cuerpo += seCancelaConDanio
                ? $"<color={colorEncabezado}><b>Cancelamento:</b></color> removida ao receber dano\n"
                : $"<color={colorEncabezado}><b>Cancelamento:</b></color> nao e removida ao receber dano\n";
            cuerpo += $"<color={colorEncabezado}><b>Fluxo de turno:</b></color> termina turno";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Reaccion propia\n";
            cuerpo += $"<color={colorEncabezado}><b>Se activa cuando:</b></color> aliado adyacente es objetivo de ataque melee unitario enemigo\n";
            cuerpo += $"<color={colorEncabezado}><b>Intercepcion:</b></color> intercambia posicion, pasa a ser objetivo, +{bonoDefensa} Defensa para ese golpe\n";
            cuerpo += $"<color={colorEncabezado}><b>Al fallar enemigo:</b></color> contraataca con Estocada base{contraataque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos:</b></color> {usos} por turno\n";
            cuerpo += seCancelaConDanio
                ? $"<color={colorEncabezado}><b>Cancelación:</b></color> se elimina al recibir daño\n"
                : $"<color={colorEncabezado}><b>Cancelación:</b></color> no se elimina al recibir daño\n";
            cuerpo += $"<color={colorEncabezado}><b>Flujo de turno:</b></color> termina turno";
        }

        string subtitulo = esIngles
            ? "Intercept melee attacks and counterattacks."
            : esPortugues
                ? "Intercepte ataques melee e contra-ataque."
                : "Intercepta ataques melee y contraataca.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense on intercepted hit.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: counterattack removes its -1 roll penalty.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (not canceled by damage) or Option B (+1 use per turn).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defesa no golpe interceptado.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: contra-ataque remove penalidade de -1 na rolagem.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (nao cancela ao receber dano) ou Opcao B (+1 uso por turno).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 Defensa en el golpe interceptado.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: contraataque elimina penalidad de -1 en tirada.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (no se cancela al recibir daño) u Opción B (+1 uso por turno).</color>"; }
        }
    }

    private string ConstruirTextoContraataqueRiposte(int bonusAtaque, int bonusDanio, bool esIngles, bool esPortugues)
    {
        string texto = "";
        if (bonusAtaque != 0)
        {
            texto += $", {bonusAtaque:+#;-#;0}";
        }
        if (bonusDanio != 0)
        {
            texto += esIngles
                ? $", {bonusDanio:+#;-#;0} Damage"
                : esPortugues
                    ? $", {bonusDanio:+#;-#;0} Dano"
                    : $", {bonusDanio:+#;-#;0} Daño";
        }
        return texto;
    }

    public override void Activar()
    {
        ObtenerObjetivos();
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }

    public async override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
        if (obj is not Unidad objetivo)
        {
            return;
        }

        ReaccionRiposte reaccionExistente = objetivo.GetComponent<ReaccionRiposte>();
        if (reaccionExistente != null)
        {
            Destroy(reaccionExistente);
        }

        await BattleManager.DelayCombateAsync(PausaClaridadMs);

        BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");
        scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir("Riposte"), new Color(0.55f, 0.8f, 1f));

        ReaccionRiposte reaccion = new ReaccionRiposte();
        reaccion.NIVEL = NIVEL;
        reaccion.permanente = false;
        reaccion.nombre = nombre;
        ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

        objetivo.Marcar(0);
        await BattleManager.DelayCombateAsync(PausaClaridadMs);
        BattleManager.Instance.TerminarTurno();
    }

    private void ObtenerObjetivos()
    {
        objetivosPosibles.Clear();
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();

        if (scEstaUnidad == null)
        {
            return;
        }

        objetivosPosibles.Add(scEstaUnidad);
        if (scEstaUnidad.CasillaPosicion != null)
        {
            scEstaUnidad.CasillaPosicion.ActivarCapaColorAzul();
        }
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Add(scEstaUnidad);
    }
}
