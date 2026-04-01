using System.Collections.Generic;
using UnityEngine;

public class PresenciaProvocadora : Habilidad
{
    private const string BuffNombreDistraido = "Distra\u00EDdo";
    private readonly List<Unidad> objetivosPosibles = new List<Unidad>();
        [SerializeField] private GameObject VFXenObjetivo;


    public override void Awake()
    {
        nombre = "Presencia Provocadora";
        IDenClase = 9;
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 1;
        esCargable = false;
        esMelee = false;
        esHostil = false;
        cooldownMax = 7;
        bAfectaObstaculos = false;

        RefrescarCostoAP();

        imHab = Resources.Load<Sprite>("imHab/Duelista_PresenciaProvocadora");
        if (imHab == null)
        {
            imHab = Resources.Load<Sprite>("imHab/Duelista_habilidad");
        }

        ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
        RefrescarCostoAP();

        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaIngles;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;
        string nombreProvocado = TRADU.i != null ? TRADU.i.Traducir(Unidad.BuffNombreProvocado) : Unidad.BuffNombreProvocado;
        string nombreDistraido = TRADU.i != null ? TRADU.i.Traducir(BuffNombreDistraido) : BuffNombreDistraido;

        int dcMental = ObtenerDificultadSalvacion();
        int reduccionDefensa = -2;
        int reduccionArmadura = ObtenerPenalidadArmadura();
        int bonusAtaqueContra = ObtenerBonusAtaqueContraataque();
        int bonusDanioContra = ObtenerBonusDanioContraataque();
        string lineaSalvacion = ConstruirLineaSalvacion(esIngles, TipoSalvacionDescripcion.Mental, dcMental);

        string tituloEs = "Presencia Provocadora I";
        string tituloEn = "Provoking Presence I";
        string tituloPt = "Presenca Provocadora I";
        if (NIVEL == 2) { tituloEs = "Presencia Provocadora II"; tituloEn = "Provoking Presence II"; tituloPt = "Presenca Provocadora II"; }
        if (NIVEL == 3) { tituloEs = "Presencia Provocadora III"; tituloEn = "Provoking Presence III"; tituloPt = "Presenca Provocadora III"; }
        if (NIVEL == 4) { tituloEs = "Presencia Provocadora IV a"; tituloEn = "Provoking Presence IV a"; tituloPt = "Presenca Provocadora IV a"; }
        if (NIVEL == 5) { tituloEs = "Presencia Provocadora IV b"; tituloEn = "Provoking Presence IV b"; tituloPt = "Presenca Provocadora IV b"; }

        string cuerpo;
        if (esIngles)
        {
            cuerpo =
                "<b>Type:</b> Self Debuff Aura + Reaction\n" +
                "<b>Target:</b> Self\n" +
                "<b>On cast:</b> affects all enemies on the opposite side\n" +
                $"{lineaSalvacion}\n" +
                $"<b>On failed save:</b> {nombreProvocado} (2 turns) and {nombreDistraido} (2 turns): {reduccionDefensa} Defense, {reduccionArmadura} Armor\n" +
                $"<b>Reaction (1 turn):</b> when an enemy misses a melee attack against her, counterattacks with base Thrust ({bonusAtaqueContra:+#;-#;0} Attack, {bonusDanioContra:+#;-#;0} Damage)\n" +
                "<b>Reaction uses:</b> unlimited until your next turn\n" +
                "<b>Turn flow:</b> using this skill ends your turn";
        }
        else if (esPortugues)
        {
            cuerpo =
                "<b>Tipo:</b> Auto Debuff em area + Reacao\n" +
                "<b>Alvo:</b> A propria Duelista\n" +
                "<b>Ao usar:</b> afeta todos os inimigos do lado oposto\n" +
                $"{lineaSalvacion}\n" +
                $"<b>Se falhar na resistencia:</b> {nombreProvocado} (2 turnos) e {nombreDistraido} (2 turnos): {reduccionDefensa} Defesa, {reduccionArmadura} Armadura\n" +
                $"<b>Reacao (1 turno):</b> quando um inimigo erra um ataque corpo a corpo contra ela, contra-ataca com Estocada base ({bonusAtaqueContra:+#;-#;0} Ataque, {bonusDanioContra:+#;-#;0} Dano)\n" +
                "<b>Usos da reacao:</b> ilimitados ate o proximo turno\n" +
                "<b>Fluxo de turno:</b> usar esta habilidade termina seu turno";
        }
        else
        {
            cuerpo =
                "<b>Tipo:</b> Auto Debuff en area + Reaccion\n" +
                "<b>Objetivo:</b> La propia Duelista\n" +
                "<b>Al usar:</b> afecta a todos los enemigos del lado opuesto\n" +
                $"{lineaSalvacion}\n" +
                $"<b>Si falla TS:</b> {nombreProvocado} (2 turnos) y {nombreDistraido} (2 turnos): {reduccionDefensa} Defensa, {reduccionArmadura} Armadura\n" +
                $"<b>Reaccion (1 turno):</b> cuando un enemigo falla un ataque melee contra ella, contraataca con Estocada base ({bonusAtaqueContra:+#;-#;0} Ataque, {bonusDanioContra:+#;-#;0} Danio)\n" +
                "<b>Usos de reaccion:</b> ilimitados hasta tu proximo turno\n" +
                "<b>Flujo de turno:</b> usar esta habilidad termina tu turno";
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
            : esPortugues
                ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
                : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentia: {costoPM}\n- Esforzable: Si ({esforzable})";

        txtDescripcion = ConstruirDescripcionEstandar(
            esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
            esIngles
                ? "The Duelist drags enemy attention to herself and punishes every failed melee attack with a precise reply."
                : esPortugues
                    ? "A Duelista puxa a atencao dos inimigos para si e pune cada erro corpo a corpo com uma resposta precisa."
                    : "La Duelista arrastra la atencion enemiga hacia si y castiga cada fallo melee con una respuesta precisa.",
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
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Distracted applies -1 extra Armor.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (counterattack loses its penalties) or Option B (-1 AP cost).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 na CD da resistencia.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Distraido aplica -1 Armadura extra.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (o contra-ataque perde as penalidades) ou Opcao B (-1 custo AP).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 al DC de la TS.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Distraido aplica -1 Armadura extra.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (el contraataque pierde sus penalidades) u Opcion B (-1 costo AP).</color>"; }
        }
    }

    public override void Activar()
    {
        RefrescarCostoAP();
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

        ReaccionPresenciaProvocadora reaccionExistente = objetivo.GetComponent<ReaccionPresenciaProvocadora>();
        if (reaccionExistente != null)
        {
            Destroy(reaccionExistente);
        }

        BattleManager.Instance.EscribirLog(TRADU.i.Traducir(scEstaUnidad.uNombre) + " " + TRADU.i.Traducir("usa ") + TRADU.i.Traducir(nombre) + ".");
        _ = scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir(nombre), new Color(0.55f, 0.8f, 1f));

        ReaccionPresenciaProvocadora reaccion = new ReaccionPresenciaProvocadora();
        reaccion.NIVEL = NIVEL;
        reaccion.permanente = false;
        reaccion.nombre = nombre;
        ComponentCopier.CopyComponent(reaccion, objetivo.gameObject);

        VFXAplicar(objetivo.gameObject);

        

        foreach (Unidad enemigo in objetivo.ObtenerTodosEnemigos())
        {
            if (enemigo == null || enemigo.HP_actual <= 0f || enemigo.CasillaPosicion == null)
            {
                continue;
            }

            if (enemigo.TiradaSalvacion(enemigo.mod_TSMental, ObtenerDificultadSalvacion()))
            {
                continue;
            }

            AplicarProvocado(enemigo);
            AplicarDistraido(enemigo);
        }

        objetivo.Marcar(0);
        BattleManager.Instance.TerminarTurno();
    }

     void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_PresenciaProvocadora");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;

        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }
    private void AplicarProvocado(Unidad objetivo)
    {
        objetivo.RemoverBuffNombre(Unidad.BuffNombreProvocado);

        Buff buff = new Buff();
        buff.buffNombre = Unidad.BuffNombreProvocado;
        buff.buffDescr = string.Empty;
        buff.boolfDebufftBuff = false;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = 2;
        buff.AplicarBuff(objetivo, scEstaUnidad);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);
    }

    private void AplicarDistraido(Unidad objetivo)
    {
        Buff buff = new Buff();
        buff.buffNombre = BuffNombreDistraido;
        buff.buffDescr = $"Pierde foco: -2 Defensa y {ObtenerPenalidadArmadura()} Armadura.";
        buff.boolfDebufftBuff = false;
        buff.DuracionBuffRondas = 2;
        buff.cantDefensa = -2;
        buff.suprimeTextoFlotante = true;
        buff.cantArmadura = ObtenerPenalidadArmadura();
        buff.AplicarBuff(objetivo, scEstaUnidad);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);
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

    private int ObtenerDificultadSalvacion()
    {
        return 13 + (NIVEL > 1 ? 1 : 0);
    }

    private int ObtenerPenalidadArmadura()
    {
        return NIVEL > 2 ? -4 : -3;
    }

    private int ObtenerBonusAtaqueContraataque()
    {
        return NIVEL == 4 ? 0 : -1;
    }

    private int ObtenerBonusDanioContraataque()
    {
        return NIVEL == 4 ? 0 : -2;
    }

    private void RefrescarCostoAP()
    {
        costoAP = NIVEL == 5 ? 2 : 3;
    }
}
