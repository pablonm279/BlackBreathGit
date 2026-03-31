using UnityEngine;

public class REPRESENTACIONPasoLigero : Habilidad
{
    public bool seusoEsteTurno = false;

    public override void Awake()
    {
        nombre = "Paso Ligero";
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

        imHab = Resources.Load<Sprite>("imHab/Duelista_PasoLigero");
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

        if (esIngles)
        {
            txtDescripcion = "<color=#5dade2><b>Light Step</b></color>\n\n";
            txtDescripcion += "<i>(Passive) Once per turn, moving diagonally or swapping position with an ally costs -1 AP.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Passo Leve</b></color>\n\n";
            txtDescripcion += "<i>(Passiva) Uma vez por turno, mover-se na diagonal ou trocar de posicao com aliados custa -1 AP.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Paso Ligero</b></color>\n\n";
        txtDescripcion += "<i>(Pasivo) Una vez por turno: moverse diagonalmente o intercambiar posicion con aliados consume -1 AP.</i>";
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
