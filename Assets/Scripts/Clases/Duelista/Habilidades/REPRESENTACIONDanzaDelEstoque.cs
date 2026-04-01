using UnityEngine;

public class REPRESENTACIONDanzaDelEstoque : Habilidad
{
    public override void Awake()
    {
        nombre = "Danza del Estoque";
        IDenClase = 10;
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

        imHab = Resources.Load<Sprite>("imHab/Duelista_DanzaDelEstoque");
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
        string nombreBuff = TRADU.i != null ? TRADU.i.Traducir("Danzando") : "Danzando";

        int umbralVida = NIVEL > 1 ? 25 : 20;
        int bonusDanio = NIVEL > 2 ? 30 : 25;
        int apAlMatar = NIVEL == 4 ? 4 : 3;
        bool buffCritico = NIVEL == 5;

        string sufijoNivel = "I";
        if (NIVEL == 2) { sufijoNivel = "II"; }
        else if (NIVEL == 3) { sufijoNivel = "III"; }
        else if (NIVEL == 4) { sufijoNivel = "IV a"; }
        else if (NIVEL == 5) { sufijoNivel = "IV b"; }

        string buffLineaEn = $"<b>{nombreBuff} (1 turn, stackable):</b> +1 Attack, +15% Damage";
        string buffLineaPt = $"<b>{nombreBuff} (1 turno, acumulavel):</b> +1 Ataque, +15% Dano";
        string buffLineaEs = $"<b>{nombreBuff} (1 turno, acumulable):</b> +1 Ataque, +15% Danio";
        if (buffCritico)
        {
            buffLineaEn += ", +1 Crit Range";
            buffLineaPt += ", +1 Faixa Critica";
            buffLineaEs += ", +1 Rango Critico";
        }

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Passive\n";
            cuerpo += $"<b>Execution:</b> +{bonusDanio}% damage against enemies at {umbralVida}% max HP or less\n";
            cuerpo += $"<b>On kill:</b> during her own turn, gains +{apAlMatar} AP immediately\n";
            cuerpo += buffLineaEn + "\n";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Passiva\n";
            cuerpo += $"<b>Execucao:</b> +{bonusDanio}% de dano contra inimigos com {umbralVida}% da vida maxima ou menos\n";
            cuerpo += $"<b>Ao matar:</b> no proprio turno, ganha +{apAlMatar} AP imediatamente\n";
            cuerpo += buffLineaPt + "\n";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Pasiva\n";
            cuerpo += $"<b>Ejecucion:</b> +{bonusDanio}% de danio contra enemigos con {umbralVida}% de hp maximo o menos\n";
            cuerpo += $"<b>Al matar:</b> en su propio turno, gana +{apAlMatar} AP inmediatamente\n";
            cuerpo += buffLineaEs + "\n";
        }

        txtDescripcion = ConstruirDescripcionEstandar(
            esIngles ? "Sword Dance " + sufijoNivel : esPortugues ? "Danca do Estoque " + sufijoNivel : "Danza del Estoque " + sufijoNivel,
            esIngles
                ? "The Duelist accelerates her finishers and keeps chaining attacks after each clean kill."
                : esPortugues
                    ? "A Duelista acelera suas execucoes e encadeia ataques depois de cada baixa limpa."
                    : "La Duelista acelera sus ejecuciones y encadena ataques despues de cada baja limpia.",
            cuerpo,
            "",
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
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% max HP threshold.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% damage against targets in threshold.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 AP on kill) or Option B (+1 Crit Range to Dancing).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% de limiar de vida maxima.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% de dano contra alvos no limiar.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 AP ao matar) ou Opcao B (+1 faixa critica em Dancando).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% al umbral de hp maximo enemigo.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% danio contra unidades en el umbral.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+1 AP al matar) u Opcion B (+1 rango critico a Danzando).</color>"; }
        }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
