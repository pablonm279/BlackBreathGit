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
        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";

        string titulo = esIngles ? "Light Step" : esPortugues ? "Passo Leve" : "Paso Ligero";
        string subtitulo = esIngles
            ? "Discount one diagonal move or ally swap each turn."
            : esPortugues
                ? "Reduz o custo de um movimento diagonal ou troca por turno."
                : "Reduce el costo de un movimiento diagonal o intercambio por turno.";

        if (esIngles)
        {
            string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                titulo,
                "Passive: Reduces the cost of one agile movement each turn.",
                new[]
                {
                    LineaDescripcion("Trigger", "Moves diagonally or swaps positions with an ally."),
                    LineaDescripcion("Effect", $"Movement costs 1 less {ap}."),
                    LineaDescripcion("Limit", "Once per turn.")
                });
            return;
        }
        if (esPortugues){string ap=TerminoDescripcion(TerminoDescripcionId.PuntosAccion,"AP","ap");txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Passiva: reduz o custo de um movimento ágil por turno.",new[]{LineaDescripcion("Ativação","Move-se diagonalmente ou troca de posição com um aliado."),LineaDescripcion("Efeito",$"O movimento custa 1 {ap} a menos."),LineaDescripcion("Limite","Uma vez por turno.")},costoSuperior:string.Empty);return;}
        {string ap=TerminoDescripcion(TerminoDescripcionId.PuntosAccion,"AP","ap");txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(titulo,"Pasiva: reduce el costo de un movimiento ágil por turno.",new[]{LineaDescripcion("Activación","Se mueve en diagonal o intercambia posiciones con un aliado."),LineaDescripcion("Efecto",$"El movimiento cuesta 1 {ap} menos."),LineaDescripcion("Límite","Una vez por turno.")},costoSuperior:string.Empty);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Passive\n";
            cuerpo += $"<color={colorEncabezado}><b>Uses:</b></color> once per turn\n";
            cuerpo += $"<color={colorEncabezado}><b>Effect:</b></color> diagonal movement or swapping position with an ally costs -1 AP";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos:</b></color> uma vez por turno\n";
            cuerpo += $"<color={colorEncabezado}><b>Efeito:</b></color> mover-se na diagonal ou trocar posicao com um aliado custa -1 AP";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Usos:</b></color> una vez por turno\n";
            cuerpo += $"<color={colorEncabezado}><b>Efecto:</b></color> moverse en diagonal o intercambiar posicion con un aliado cuesta -1 AP";
        }

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
