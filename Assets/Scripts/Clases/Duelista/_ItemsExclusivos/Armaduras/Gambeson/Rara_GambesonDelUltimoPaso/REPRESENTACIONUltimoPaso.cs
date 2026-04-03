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

        if (esIngles)
        {
            txtDescripcion = "<color=#5dade2><b>Last Step</b></color>\n\n";
            txtDescripcion += "<i>(Passive) The first time this battle the wearer drops below 50% HP, gain +2 Evasion and 10 Barrier for 1 turn.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Ultimo Passo</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) A primeira vez nesta batalha em que a usuaria cair abaixo de 50% HP, ganha +2 Evasao e 10 Barreira por 1 turno.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Ultimo Paso</b></color>\n\n";
        txtDescripcion += "<i>(Pasivo) La primera vez en combate que la portadora quede por debajo de 50% de vida, gana +2 Evasion y 10 Barrera por 1 turno.</i>";
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
