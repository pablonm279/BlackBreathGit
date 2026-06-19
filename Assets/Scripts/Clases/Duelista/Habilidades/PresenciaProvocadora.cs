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

        string tituloEs = "Presencia Provocadora I";
        string tituloEn = "Provoking Presence I";
        string tituloPt = "Presenca Provocadora I";
        if (NIVEL == 2) { tituloEs = "Presencia Provocadora II"; tituloEn = "Provoking Presence II"; tituloPt = "Presenca Provocadora II"; }
        if (NIVEL == 3) { tituloEs = "Presencia Provocadora III"; tituloEn = "Provoking Presence III"; tituloPt = "Presenca Provocadora III"; }
        if (NIVEL == 4) { tituloEs = "Presencia Provocadora IV a"; tituloEn = "Provoking Presence IV a"; tituloPt = "Presenca Provocadora IV a"; }
        if (NIVEL == 5) { tituloEs = "Presencia Provocadora IV b"; tituloEn = "Provoking Presence IV b"; tituloPt = "Presenca Provocadora IV b"; }

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
        string contraataque = ConstruirTextoContraataque(bonusAtaqueContra, bonusDanioContra, esIngles, esPortugues);

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Self aura + reaction\n";
            cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> Self\n";
            cuerpo += $"<color={colorEncabezado}><b>On cast:</b></color> affects all enemies on the opposite side; ends turn\n";
            cuerpo += $"<color={colorEncabezado}><b>Save:</b></color> Mental vs DC {dcMental}\n";
            cuerpo += $"<color={colorEncabezado}><b>On failed save:</b></color> {nombreProvocado} and {nombreDistraido} for 2 turns: {reduccionDefensa} Defense, {reduccionArmadura} Armor\n";
            cuerpo += $"<color={colorEncabezado}><b>Reaction (1 turn):</b></color> when an enemy misses a melee attack against her, counterattacks with base Thrust{contraataque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Reaction uses:</b></color> unlimited until next turn\n";
            cuerpo += $"<color={colorEncabezado}><b>Effortable:</b></color> yes ({esforzable})";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Aura propria + reacao\n";
            cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> Si mesmo\n";
            cuerpo += $"<color={colorEncabezado}><b>Ao usar:</b></color> afeta todos os inimigos do lado oposto; termina turno\n";
            cuerpo += $"<color={colorEncabezado}><b>Resistencia:</b></color> Mental vs CD {dcMental}\n";
            cuerpo += $"<color={colorEncabezado}><b>Se falhar:</b></color> {nombreProvocado} e {nombreDistraido} por 2 turnos: {reduccionDefensa} Defesa, {reduccionArmadura} Armadura\n";
            cuerpo += $"<color={colorEncabezado}><b>Reacao (1 turno):</b></color> quando um inimigo erra ataque melee contra ela, contra-ataca com Estocada base{contraataque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos da reacao:</b></color> ilimitados ate o próximo turno\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforcavel:</b></color> sim ({esforzable})";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Aura propia + reaccion\n";
            cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> Uno mismo\n";
            cuerpo += $"<color={colorEncabezado}><b>Al usar:</b></color> afecta a todos los enemigos del lado opuesto; termina turno\n";
            cuerpo += $"<color={colorEncabezado}><b>TS:</b></color> Mental vs DC {dcMental}\n";
            cuerpo += $"<color={colorEncabezado}><b>Si falla:</b></color> {nombreProvocado} y {nombreDistraido} por 2 turnos: {reduccionDefensa} Defensa, {reduccionArmadura} Armadura\n";
            cuerpo += $"<color={colorEncabezado}><b>Reacción (1 turno):</b></color> cuando un enemigo falla ataque melee contra ella, contraataca con Estocada base{contraataque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos de reacción:</b></color> ilimitados hasta el próximo turno\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforzable:</b></color> si ({esforzable})";
        }

        string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
        string subtitulo = esIngles
            ? "Provokes enemies and punishes missed melee attacks."
            : esPortugues
                ? "Provoca inimigos e pune ataques melee errados."
                : "Provoca enemigos y castiga ataques melee fallidos.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 save DC.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Distracted applies -1 extra Armor.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (counterattack loses penalties) or Option B (-1 AP cost).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 na CD da resistencia.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Distraido aplica -1 Armadura extra.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (contra-ataque perde penalidades) ou Opcao B (-1 custo AP).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 al DC de la TS.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Distraido aplica -1 Armadura extra.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (contraataque pierde penalidades) u Opción B (-1 costo AP).</color>"; }
        }
    }

    private string ConstruirTextoContraataque(int bonusAtaqueContra, int bonusDanioContra, bool esIngles, bool esPortugues)
    {
        string texto = "";
        if (bonusAtaqueContra != 0)
        {
            texto += $", {bonusAtaqueContra:+#;-#;0}";
        }
        if (bonusDanioContra != 0)
        {
            texto += esIngles
                ? $", {bonusDanioContra:+#;-#;0} Damage"
                : esPortugues
                    ? $", {bonusDanioContra:+#;-#;0} Dano"
                    : $", {bonusDanioContra:+#;-#;0} Daño";
        }
        return texto;
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
        if (scEstaUnidad.CasillaPosicion != null)
        {
            scEstaUnidad.CasillaPosicion.ActivarCapaColorAzul();
        }
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Add(scEstaUnidad);
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
