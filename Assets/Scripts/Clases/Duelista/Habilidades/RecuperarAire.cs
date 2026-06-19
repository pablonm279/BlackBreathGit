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
        string rangoCuracion = FormatearRangoDados(dadosCuracion, 10);
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

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Self recovery\n";
            cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> Self\n";
            cuerpo += $"<color={colorEncabezado}><b>Position:</b></color> rear column only\n";
            cuerpo += $"<color={colorEncabezado}><b>Effect (2 turns):</b></color> +{apMax} Max AP, {defensa} Defense\n";
            cuerpo += $"<color={colorEncabezado}><b>Immediate:</b></color> heals {rangoCuracion}, +{impulso} Impulse, +{valentia} Valour\n";
            cuerpo += $"<color={colorEncabezado}><b>Turn flow:</b></color> ends turn";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Recuperacao propria\n";
            cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> Si mesmo\n";
            cuerpo += $"<color={colorEncabezado}><b>Posicao:</b></color> apenas na coluna traseira\n";
            cuerpo += $"<color={colorEncabezado}><b>Efeito (2 turnos):</b></color> +{apMax} AP max, {defensa} Defesa\n";
            cuerpo += $"<color={colorEncabezado}><b>Imediato:</b></color> cura {rangoCuracion}, +{impulso} Impulso, +{valentia} Valentía\n";
            cuerpo += $"<color={colorEncabezado}><b>Fluxo de turno:</b></color> termina turno";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Recuperacion propia\n";
            cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> Uno mismo\n";
            cuerpo += $"<color={colorEncabezado}><b>Posicion:</b></color> solo en columna trasera\n";
            cuerpo += $"<color={colorEncabezado}><b>Efecto (2 turnos):</b></color> +{apMax} AP max, {defensa} Defensa\n";
            cuerpo += $"<color={colorEncabezado}><b>Inmediato:</b></color> cura {rangoCuracion}, +{impulso} Impulso, +{valentia} Valentía\n";
            cuerpo += $"<color={colorEncabezado}><b>Flujo de turno:</b></color> termina turno";
        }

        string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
        string subtitulo = esIngles
            ? "Recover in the rear column and prepare the next turn."
            : esPortugues
                ? "Recupera na coluna traseira e prepara o próximo turno."
                : "Recupera en columna trasera y prepara el próximo turno.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Defense.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1-10 healing.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 Impulse) or Option B (+1 Valour).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 Defesa.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1-10 cura.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 Impulso) ou Opcao B (+1 Valentia).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 Defensa.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1-10 curación.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+1 Impulso) u Opción B (+1 Valentía).</color>"; }
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
        buff.buffDescr = $"Descansa para el turno siguiente: +3 PA máximo, {ObtenerModDefensa()} Defensa.";
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
        if (scEstaUnidad.CasillaPosicion != null)
        {
            scEstaUnidad.CasillaPosicion.ActivarCapaColorAzul();
        }
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Add(scEstaUnidad);
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
