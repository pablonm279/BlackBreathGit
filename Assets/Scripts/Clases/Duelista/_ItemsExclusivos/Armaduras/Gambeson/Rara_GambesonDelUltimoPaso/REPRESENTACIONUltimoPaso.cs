using UnityEngine;

public class REPRESENTACIONUltimoPaso : Habilidad
{
    private const string NombreBuffUltimoPaso = "Ultimo Paso";
    private const int BonusEvasion = 2;
    private const int BarreraOtorgada = 10;

    private bool activadoEsteCombate;
    private bool bonusEvasionAplicado;

    public override void Awake()
    {
        nombre = NombreBuffUltimoPaso;
        IDenClase = 0;
        costoAP = 0;
        costoPM = 0;
        cooldownMax = 0;
        cooldownActual = 0;
        requiereRecurso = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario != null ? Usuario.GetComponent<Unidad>() : null;
        esZonal = false;
        enArea = 0;
        esforzable = 0;
        esCargable = false;
        esMelee = false;
        bAfectaObstaculos = false;
        poneTrampas = false;
        poneObstaculo = false;
        esHostil = false;
        esDiscreta = true;
        NIVEL = -1;

        imHab = Resources.Load<Sprite>("imHab/Duelista_habilidad");
        ActualizarDescripcion();
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaIngles;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;
        string colorEncabezado = "#44d3ec";
        string colorValor = "#ffffff";
        string titulo = esIngles ? "Last Step" : esPortugues ? "Ultimo Passo" : "Ultimo Paso";
        string subtitulo = esIngles
            ? "Passive: triggers once per battle below half HP."
            : esPortugues
                ? "Passiva: ativa uma vez por batalha abaixo de metade do HP."
                : "Pasiva: se activa una vez por combate bajo mitad de HP.";
        string cuerpo = esIngles
            ? $"<color={colorEncabezado}><b>Type:</b></color> <color={colorValor}>Passive</color>\n<color={colorEncabezado}><b>Trigger:</b></color> <color={colorValor}>First time below 50% HP each battle</color>\n<color={colorEncabezado}><b>Effect:</b></color> <color={colorValor}>+{BonusEvasion} Evasion, +{BarreraOtorgada} Barrier for 1 turn</color>"
            : esPortugues
                ? $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Passiva</color>\n<color={colorEncabezado}><b>Ativacao:</b></color> <color={colorValor}>Primeira vez abaixo de 50% HP por batalha</color>\n<color={colorEncabezado}><b>Efeito:</b></color> <color={colorValor}>+{BonusEvasion} Evasao, +{BarreraOtorgada} Barreira por 1 turno</color>"
                : $"<color={colorEncabezado}><b>Tipo:</b></color> <color={colorValor}>Pasiva</color>\n<color={colorEncabezado}><b>Activacion:</b></color> <color={colorValor}>Primera vez bajo 50% HP por combate</color>\n<color={colorEncabezado}><b>Efecto:</b></color> <color={colorValor}>+{BonusEvasion} Evasion, +{BarreraOtorgada} Barrera por 1 turno</color>";
        txtDescripcion = ConstruirDescripcionTooltipNueva(titulo, subtitulo, cuerpo, "");
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }

    private void Update()
    {
        if (scEstaUnidad == null)
        {
            scEstaUnidad = GetComponent<Unidad>();
            if (scEstaUnidad == null)
            {
                return;
            }
        }

        if (EsEscenaCampana())
        {
            return;
        }

        if (bonusEvasionAplicado && !scEstaUnidad.TieneBuffNombre(NombreBuffUltimoPaso))
        {
            scEstaUnidad.estado_evasion = Mathf.Max(0, scEstaUnidad.estado_evasion - BonusEvasion);
            bonusEvasionAplicado = false;
        }

        if (activadoEsteCombate || scEstaUnidad.mod_maxHP <= 0f || scEstaUnidad.HP_actual <= 0f)
        {
            return;
        }

        if (scEstaUnidad.HP_actual > scEstaUnidad.mod_maxHP * 0.5f)
        {
            return;
        }

        Buff buff = new Buff();
        buff.buffNombre = NombreBuffUltimoPaso;
        buff.buffDescr = "Ultimo Paso: +2 Evasion y 10 Barrera por 1 turno.";
        buff.boolfDebufftBuff = true;
        buff.DuracionBuffRondas = 1;
        buff.cantBarrera = BarreraOtorgada;
        buff.AplicarBuff(scEstaUnidad);
        ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);

        scEstaUnidad.estado_evasion += BonusEvasion;
        activadoEsteCombate = true;
        bonusEvasionAplicado = true;
    }

    private bool EsEscenaCampana()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "ES-Campaña";
    }
}
