using System.Collections.Generic;
using UnityEngine;

public class EnGarde : Habilidad
{
    private const string BuffNombre = "En Garde";
    private readonly List<Unidad> objetivosPosibles = new List<Unidad>();

    public override void Awake()
    {
        nombre = "En Garde";
        IDenClase = 6;
        costoAP = 1;
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 0;
        esCargable = false;
        esMelee = false;
        esHostil = false;
        cooldownMax = NIVEL == 5 ? 4 : 5;
        bAfectaObstaculos = false;

        imHab = Resources.Load<Sprite>("imHab/Duelista_EnGarde");
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

        int bonusEvasion = ObtenerBonusEvasion();
        int bonusDanio = ObtenerBonusDanio();
        int bonusAtaque = 2;
        int bonusCritico = 1;
        int enfriamiento = NIVEL == 5 ? 4 : 5;
        int umbralPosturaDemandante = NIVEL == 4 ? 10 : 5;

        string sufijoNivel = "I";
        if (NIVEL == 2) { sufijoNivel = "II"; }
        else if (NIVEL == 3) { sufijoNivel = "III"; }
        else if (NIVEL == 4) { sufijoNivel = "IV a"; }
        else if (NIVEL == 5) { sufijoNivel = "IV b"; }

        string cuerpo;
        if (esIngles)
        {
            cuerpo =
                "<b>Type:</b> Self Buff\n" +
                "<b>Target:</b> Self\n" +
                $"<b>Buff (max 3 turns):</b> +{bonusEvasion} Evasion, +{bonusDanio}% Damage, +{bonusCritico} Crit Range, +{bonusAtaque} Attack\n" +
                "<b>Cancel:</b> removed when taking damage\n" +
                $"<color=#ff4d4d><b>Demanding Stance:</b> trigger threshold lowered to {umbralPosturaDemandante}% max HP while active</color>";
        }
        else if (esPortugues)
        {
            cuerpo =
                "<b>Tipo:</b> Auto Buff\n" +
                "<b>Alvo:</b> O proprio usuario\n" +
                $"<b>Buff (maximo 3 turnos):</b> +{bonusEvasion} Evasao, +{bonusDanio}% Dano, +{bonusCritico} Faixa Critica, +{bonusAtaque} Ataque\n" +
                "<b>Cancelamento:</b> removido ao receber dano\n" +
                $"<color=#ff4d4d><b>Postura Exigente:</b> limiar de disparo reduzido para {umbralPosturaDemandante}% da vida maxima enquanto ativo</color>";
        }
        else
        {
            cuerpo =
                "<b>Tipo:</b> Auto Buff\n" +
                "<b>Objetivo:</b> Uno mismo\n" +
                $"<b>Buff (mÃ¡ximo 3 turnos):</b> +{bonusEvasion} EvasiÃ³n, +{bonusDanio}% DaÃ±o, +{bonusCritico} Rango CrÃ­tico, +{bonusAtaque} Ataque\n" +
                "<b>CancelaciÃ³n:</b> se elimina al recibir daÃ±oo\n" +
                $"<color=#ff4d4d><b>Postura Demandante:</b> el umbral del disparo baja a {umbralPosturaDemandante}% de la vida maxima mientras esta activo</color>";
        }

        string costos = esIngles
            ? $"- Cooldown: {enfriamiento}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: No"
            : esPortugues
                ? $"- Recarga: {enfriamiento}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Nao"
                : $"- Enfriamiento: {enfriamiento}\n- Costo AP: {costoAP}\n- Costo Valentia: {costoPM}\n- Esforzable: No";

        txtDescripcion = ConstruirDescripcionEstandar(
            esIngles ? "En Garde " + sufijoNivel : esPortugues ? "Em Guarda " + sufijoNivel : "En Garde " + sufijoNivel,
            esIngles
                ? "The Duelist enters a prepared guard stance that sharpens offense and evasive posture."
                : esPortugues
                    ? "A Duelista entra em uma guarda preparada que amplia ofensiva e evasao."
                    : "La Duelista entra en una guardia preparada que potencia su ofensiva y evasiva.",
            cuerpo,
            costos,
            "#5dade2");

        bool mostrarProximoNivel = CampaignManager.Instance != null
            && CampaignManager.Instance.scMenuPersonajes != null
            && CampaignManager.Instance.scMenuPersonajes.pSel != null
            && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel)
        {
            return;
        }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Evasion.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% Damage.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (Demanding Stance at 10%) or Option B (-1 cooldown).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Evasao.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Dano.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (Postura Exigente em 10%) ou Opcao B (-1 recarga).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Evasion.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Danio.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (Postura Demandante al 10%) u Opcion B (-1 enfriamiento).</color>"; }
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

        objetivo.RemoverBuffNombre(BuffNombre);

        Buff buff = new Buff();
        buff.buffNombre = BuffNombre;
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = 3;
        buff.cantDanioPorcentaje = ObtenerBonusDanio();
        buff.cantCritDado = 1;
        buff.cantAtaque = 2;
        buff.AplicarBuff(objetivo);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        if (objetivo is ClaseDuelista duelista)
        {
            duelista.NotificarInicioEnGarde();
        }

        objetivo.Marcar(0);
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

    private int ObtenerBonusDanio()
    {
        int bonus = 15;
        if (NIVEL > 2)
        {
            bonus += 5;
        }

        return bonus;
    }

    private int ObtenerBonusEvasion()
    {
        int bonus = 2;
        if (NIVEL > 1)
        {
            bonus += 1;
        }

        return bonus;
    }
}
