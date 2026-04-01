using UnityEngine;

public class REPRESENTACIONPosturaDemandante : Habilidad
{
    public bool seusoEsteTurno = false;

    public override void Awake()
    {
        nombre = "Postura Demandante";
        IDenClase = 0;
        costoAP = 0;
        costoPM = 0;
        cooldownMax = 0;
        cooldownActual = 0;
        requiereRecurso = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
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

        imHab = Resources.Load<Sprite>("imHab/Duelista_PosturaDemandante");
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
        string nombreTambaleando = TRADU.i != null ? TRADU.i.Traducir("Tambaleando") : "Tambaleando";

        if (esIngles)
        {
            txtDescripcion = "<color=#5dade2><b>Demanding Stance</b></color>\n\n";
            txtDescripcion += "<i>(Passive) Once per turn, when the Duelist receives damage above 20% of max HP, gain " + nombreTambaleando + " for 1 turn: -1 max AP, -2 Defense.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Postura Demandante</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) Uma vez por turno, ao receber dano acima de 20% da vida maxima, ganha " + nombreTambaleando + " por 1 turno: -1 AP max, -2 Defesa.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Postura Demandante</b></color>\n\n";
        txtDescripcion += "<i>(Pasivo) Una vez por turno: al recibir dano de mas de 20% de su vida maxima, obtiene " + nombreTambaleando + " por 1 turno: -1 AP maximo, -2 Defensa.</i>";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
