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

        string cuerpo;
        if (esIngles)
        {
            cuerpo =
                "<b>Type:</b> Self Reaction\n" +
                "<b>Trigger 1:</b> if an adjacent ally is targeted by a single-target enemy melee attack, swap positions and become the new target\n" +
                $"<b>Intercept:</b> +{bonoDefensa} Defense for that intercepted hit\n" +
                $"<b>Trigger 2:</b> if an enemy misses a melee attack against her, counterattacks with base Thrust ({bonusAtaqueReaccion} Attack, -2 Damage)\n" +
                $"<b>Uses:</b> {usos} per turn\n" +
                (seCancelaConDanio ? "<b>Cancel:</b> removed when taking damage" : "<b>Cancel:</b> no longer removed when taking damage");
        }
        else if (esPortugues)
        {
            cuerpo =
                "<b>Tipo:</b> Reacao propria\n" +
                "<b>Gatilho 1:</b> se um aliado adjacente for alvo de um ataque corpo a corpo unitario inimigo, troca de lugar e vira o novo alvo\n" +
                $"<b>Intercepcao:</b> +{bonoDefensa} Defesa para esse golpe interceptado\n" +
                $"<b>Gatilho 2:</b> se um inimigo errar um ataque corpo a corpo contra ela, contra-ataca com Estocada base ({bonusAtaqueReaccion} Ataque, -2 Dano)\n" +
                $"<b>Usos:</b> {usos} por turno\n" +
                (seCancelaConDanio ? "<b>Cancelamento:</b> removida ao receber dano" : "<b>Cancelamento:</b> nao e removida ao receber dano");
        }
        else
        {
            cuerpo =
                "<b>Tipo:</b> Reaccion propia\n" +
                "<b>Disparo 1:</b> si un aliado adyacente es objetivo de un ataque melee unitario enemigo, intercambia lugar y pasa a ser el nuevo objetivo\n" +
                $"<b>Intercepcion:</b> +{bonoDefensa} Defensa para ese golpe interceptado\n" +
                $"<b>Disparo 2:</b> si un enemigo falla un ataque melee contra ella, contraataca con Estocada base ({bonusAtaqueReaccion} Ataque, -2 Danio)\n" +
                $"<b>Usos:</b> {usos} por turno\n" +
                (seCancelaConDanio ? "<b>Cancelacion:</b> se elimina al recibir danio" : "<b>Cancelacion:</b> no se elimina al recibir danio");
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}\n- Effortable: No"
            : esPortugues
                ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}\n- Esforcavel: Nao"
                : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentia: {costoPM}\n- Esforzable: No";

        txtDescripcion = ConstruirDescripcionEstandar(
            titulo,
            esIngles
                ? "Protects nearby allies by intercepting melee attacks and answering with a precise counter."
                : esPortugues
                    ? "Protege aliados proximos interceptando ataques corpo a corpo e respondendo com um contra-ataque preciso."
                    : "Protege aliados cercanos interceptando ataques melee y respondiendo con un contraataque preciso.",
            cuerpo,
            costos,
            "#5dade2");

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense on intercepted hit.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: counterattack removes its -1 Attack penalty.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (no longer canceled when taking damage) or Option B (+1 use per turn).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defesa no golpe interceptado.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: o contra-ataque remove a penalidade de -1 Ataque.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (nao e mais cancelada ao receber dano) ou Opcao B (+1 uso por turno).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defensa en el golpe interceptado.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: el contraataque elimina su penalidad de -1 Ataque.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (ya no se cancela al recibir danio) u Opcion B (+1 uso por turno).</color>"; }
        }
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
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Add(scEstaUnidad);
        scEstaUnidad.Marcar(1);
    }
}
