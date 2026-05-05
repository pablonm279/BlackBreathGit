using System.Collections.Generic;
using UnityEngine;

public class Riposte : Habilidad
{
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

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Self reaction\n";
            cuerpo += $"<color={colorEncabezado}><b>Trigger 1:</b></color> adjacent ally is targeted by a single-target enemy melee attack\n";
            cuerpo += $"<color={colorEncabezado}><b>Intercept:</b></color> swap positions, become target, +{bonoDefensa} Defense for that hit\n";
            cuerpo += $"<color={colorEncabezado}><b>Trigger 2:</b></color> enemy misses a melee attack against her; counterattacks with base Thrust{contraataque}\n";
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
            cuerpo += $"<color={colorEncabezado}><b>Disparo 1:</b></color> aliado adyacente es objetivo de ataque melee unitario enemigo\n";
            cuerpo += $"<color={colorEncabezado}><b>Intercepcion:</b></color> intercambia posicion, pasa a ser objetivo, +{bonoDefensa} Defensa para ese golpe\n";
            cuerpo += $"<color={colorEncabezado}><b>Disparo 2:</b></color> enemigo falla ataque melee contra ella; contraataca con Estocada base{contraataque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos:</b></color> {usos} por turno\n";
            cuerpo += seCancelaConDanio
                ? $"<color={colorEncabezado}><b>Cancelacion:</b></color> se elimina al recibir danio\n"
                : $"<color={colorEncabezado}><b>Cancelacion:</b></color> no se elimina al recibir danio\n";
            cuerpo += $"<color={colorEncabezado}><b>Flujo de turno:</b></color> termina turno";
        }

        string subtitulo = esIngles
            ? "Intercept melee attacks and answer misses with a counterattack."
            : esPortugues
                ? "Intercepta ataques melee e responde erros com contra-ataque."
                : "Intercepta ataques melee y responde fallos con contraataque.";

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
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defensa en el golpe interceptado.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: contraataque elimina penalidad de -1 en tirada.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (no se cancela al recibir danio) u Opcion B (+1 uso por turno).</color>"; }
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
                    : $", {bonusDanio:+#;-#;0} Danio";
        }
        return texto;
    }

    public override void Activar()
    {
        ObtenerObjetivos();
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
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

        BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");
        scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir("Riposte"), new Color(0.55f, 0.8f, 1f));

        ReaccionRiposte reaccion = new ReaccionRiposte();
        reaccion.NIVEL = NIVEL;
        reaccion.permanente = false;
        reaccion.nombre = nombre;
        ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

        objetivo.Marcar(0);
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
