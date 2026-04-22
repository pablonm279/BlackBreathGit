using System.Collections.Generic;
using UnityEngine;

public class RecuperarAire : Habilidad
{
    [SerializeField] private GameObject VFXenObjetivo;

    private const string BuffNombre = "Recuperando Aire";
    private const string MotivoPosicion = "Solo en columna trasera.";
    private readonly List<Unidad> objetivosPosibles = new List<Unidad>();

    public override void Awake()
    {
        nombre = "Recuperar Aire";
        IDenClase = 8;
        costoAP = 3;
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 0;
        esCargable = false;
        esMelee = false;
        esHostil = false;
        cooldownMax = 5;
        bAfectaObstaculos = false;

        imHab = Resources.Load<Sprite>("imHab/Duelista_RecuperarAire");
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

        int apMax = 3;
        int dadosCuracion = ObtenerDadosCuracion();
        int defensa = ObtenerModDefensa();
        int impulso = ObtenerCantidadImpulso();
        int valentia = ObtenerCantidadValentia();

        string tituloEs = "Recuperar Aire I";
        string tituloEn = "Catch Breath I";
        string tituloPt = "Recuperar Folego I";
        if (NIVEL == 2) { tituloEs = "Recuperar Aire II"; tituloEn = "Catch Breath II"; tituloPt = "Recuperar Folego II"; }
        if (NIVEL == 3) { tituloEs = "Recuperar Aire III"; tituloEn = "Catch Breath III"; tituloPt = "Recuperar Folego III"; }
        if (NIVEL == 4) { tituloEs = "Recuperar Aire IV a"; tituloEn = "Catch Breath IV a"; tituloPt = "Recuperar Folego IV a"; }
        if (NIVEL == 5) { tituloEs = "Recuperar Aire IV b"; tituloEn = "Catch Breath IV b"; tituloPt = "Recuperar Folego IV b"; }

        string cuerpo;
        if (esIngles)
        {
            cuerpo =
                "<b>Type:</b> Self Buff\n" +
                "<b>Target:</b> Self\n" +
                "<b>Position:</b> rear column only\n" +
                "<b>Buff:</b> 2 turns (covers the next turn)\n" +
                $"<b>Buff effect:</b> +{apMax} Max AP,  <color=red>{defensa}</color> Defense\n" +
                $"<b>Immediate effect:</b> heals {dadosCuracion}d10, gains +{impulso} Impulse and +{valentia} Valour\n" +
                "<b>Turn flow:</b> using this skill ends your turn";
        }
        else if (esPortugues)
        {
            cuerpo =
                "<b>Tipo:</b> Auto Buff\n" +
                "<b>Alvo:</b> A propria Duelista\n" +
                "<b>Posicao:</b> apenas na coluna traseira\n" +
                "<b>Buff:</b> 2 turnos (cobre o proximo turno)\n" +
                $"<b>Efeito do buff:</b> +{apMax} AP max, <color=red>{defensa}</color> Defesa\n" +
                $"<b>Efeito imediato:</b> cura {dadosCuracion}d10, ganha +{impulso} Impulso e +{valentia} Valentia\n" +
                "<b>Fluxo de turno:</b> usar esta habilidade termina seu turno";
        }
        else
        {
            cuerpo =
                "<b>Tipo:</b> Auto Buff\n" +
                "<b>Objetivo:</b> La propia Duelista\n" +
                "<b>Posicion:</b> solo en columna trasera\n" +
                "<b>Buff:</b> 2 turnos (cubre el turno siguiente)\n" +
                $"<b>Efecto del buff:</b> +{apMax} AP max, <color=red>{defensa}</color> Defensa\n" +
                $"<b>Efecto inmediato:</b> cura {dadosCuracion}d10, gana +{impulso} Impulso y +{valentia} Valentia\n" +
                "<b>Flujo de turno:</b> usar esta habilidad termina tu turno";
        }

        string costos = esIngles
            ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP} (ends turn)\n- Valour Cost: {costoPM}\n- Effortable: No"
            : esPortugues
                ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP} (termina turno)\n- Custo Valentia: {costoPM}\n- Esforcavel: Nao"
                : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP} (termina turno)\n- Costo Valentia: {costoPM}\n- Esforzable: No";

        txtDescripcion = ConstruirDescripcionEstandar(
            esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
            esIngles
                ? "The Duelist steps back, recovers, and prepares an explosive next turn."
                : esPortugues
                    ? "A Duelista recua, respira e prepara um proximo turno explosivo."
                    : "La Duelista toma aire, se recompone y prepara un siguiente turno explosivo.",
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
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1d10 healing.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 Impulse) or Option B (+1 Valour).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defesa.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1d10 de cura.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 Impulso) ou Opcao B (+1 Valentia).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defensa.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1-10 curacion.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 Impulso) u Opcion B (+1 Valentia).</color>"; }
        }
    }

    public override void Activar()
    {
        if (!PuedeActivarseDesdePosicionActual(out string motivo))
        {
            scEstaUnidad?.GenerarTextoFlotante(TRADU.i.Traducir(motivo), Color.gray, FloatingTextContext.Generic);
            return;
        }

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
        buff.buffDescr = $"Descansa para el turno siguiente: +3 PA maximo, {ObtenerModDefensa()} Defensa.";
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = 2;
        buff.cantAPMax = 3;
        buff.cantDefensa = ObtenerModDefensa();
        buff.AplicarBuff(objetivo);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        objetivo.RecibirCuracion(TiradaDeDados.TirarDados(ObtenerDadosCuracion(), 10), false);
        Estados.Aplicar_MovimientoAbaratado(objetivo, ObtenerCantidadImpulso(), scEstaUnidad);
        objetivo.SumarValentia(ObtenerCantidadValentia());

        objetivo.Marcar(0);
        BattleManager.Instance.TerminarTurno();

        
                VFXAplicar(objetivo.gameObject);
    }

     void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_RecuperarAire");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;

        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }
    public bool PuedeActivarseDesdePosicionActual(out string motivo)
    {
        motivo = MotivoPosicion;
        return scEstaUnidad != null && scEstaUnidad.CasillaPosicion != null && scEstaUnidad.CasillaPosicion.posX == 1;
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

    private int ObtenerDadosCuracion()
    {
        return NIVEL > 2 ? 4 : 3;
    }

    private int ObtenerModDefensa()
    {
        return NIVEL > 1 ? -3 : -4;
    }

    private int ObtenerCantidadImpulso()
    {
        return NIVEL == 4 ? 3 : 2;
    }

    private int ObtenerCantidadValentia()
    {
        return NIVEL == 5 ? 2 : 1;
    }
}
