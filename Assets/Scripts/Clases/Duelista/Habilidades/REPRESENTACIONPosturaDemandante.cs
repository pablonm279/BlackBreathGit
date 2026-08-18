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
        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";

        string titulo = esIngles ? "Demanding Stance" : esPortugues ? "Postura Demandante" : "Postura Demandante";
        string subtitulo = esIngles
            ? "Heavy hits leave the Duelist staggered for a turn."
            : esPortugues
                ? "Golpes pesados deixam a Duelista cambaleando por um turno."
                : "Los golpes fuertes dejan a la Duelista tambaleando por un turno.";

        if (esIngles)
        {
            string tambaleando = TerminoDescripcion(TerminoDescripcionId.Tambaleando, "Staggering");
            string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
            string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                titulo,
                "Passive: Heavy hits leave the Duelist staggered.",
                new[]
                {
                    LineaDescripcion("Trigger", "A hit deals more than 20% max HP."),
                    LineaDescripcion("Effect", $"Gains {tambaleando}: -1 max {ap}, -2 {defensa} (1 turn)."),
                    LineaDescripcion("Limit", "Once per turn.")
                });
            return;
        }
        if(esPortugues){string tamb=TerminoDescripcion(TerminoDescripcionId.Tambaleando,"Cambaleante");string ap=TerminoDescripcion(TerminoDescripcionId.PuntosAccion,"AP","ap");string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defesa","IconoDefensa");txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Passiva: golpes pesados deixam a Duelista cambaleante.",new[]{LineaDescripcion("Ativação","Um golpe causa mais de 20% do HP máximo."),LineaDescripcion("Efeito",$"Recebe {tamb}: -1 {ap} máximo, -2 {def} (1 turno)."),LineaDescripcion("Limite","Uma vez por turno.")},costoSuperior:string.Empty);return;}
        {string tamb=TerminoDescripcion(TerminoDescripcionId.Tambaleando,"Tambaleante");string ap=TerminoDescripcion(TerminoDescripcionId.PuntosAccion,"AP","ap");string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defensa","IconoDefensa");txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Pasiva: los golpes fuertes dejan a la Duelista tambaleante.",new[]{LineaDescripcion("Activación","Un golpe inflige más del 20% del HP máximo."),LineaDescripcion("Efecto",$"Obtiene {tamb}: -1 {ap} máximo, -2 {def} (1 turno)."),LineaDescripcion("Límite","Una vez por turno.")},costoSuperior:string.Empty);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Passive\n";
            cuerpo += $"<color={colorEncabezado}><b>Uses:</b></color> once per turn\n";
            cuerpo += $"<color={colorEncabezado}><b>Trigger:</b></color> receives damage above 20% of max HP\n";
            cuerpo += $"<color={colorEncabezado}><b>Effect:</b></color> gains {nombreTambaleando} for 1 turn (-1 max AP, -2 Defense)";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos:</b></color> uma vez por turno\n";
            cuerpo += $"<color={colorEncabezado}><b>Gatilho:</b></color> recebe dano acima de 20% da vida maxima\n";
            cuerpo += $"<color={colorEncabezado}><b>Efeito:</b></color> ganha {nombreTambaleando} por 1 turno (-1 AP max, -2 Defesa)";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos:</b></color> una vez por turno\n";
            cuerpo += $"<color={colorEncabezado}><b>Disparo:</b></color> recibe daño mayor a 20% de su HP máximo\n";
            cuerpo += $"<color={colorEncabezado}><b>Efecto:</b></color> gana {nombreTambaleando} por 1 turno (-1 AP max, -2 Defensa)";
        }

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
