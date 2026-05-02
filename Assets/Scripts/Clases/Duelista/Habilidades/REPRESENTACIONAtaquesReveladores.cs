using UnityEngine;

public class REPRESENTACIONAtaquesReveladores : Habilidad
{
    public override void Awake()
    {
        nombre = "Ataques Reveladores";
        IDenClase = 1;
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

        imHab = Resources.Load<Sprite>("imHab/Duelista_AtaquesReveladores");
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
        string nombreDebuff = TRADU.i != null ? TRADU.i.Traducir("Vulnerabilidad Expuesta") : "Vulnerabilidad Expuesta";

        int bonusCritPorcentaje = 5;
        int bonusDanioCritico = 0;
        int duracion = 2;

        if (NIVEL > 1) { bonusDanioCritico += 10; }
        if (NIVEL > 2) { bonusCritPorcentaje += 5; }
        if (NIVEL == 4) { bonusDanioCritico += 15; }
        if (NIVEL == 5) { duracion += 1; }

        string sufijoNivel = "I";
        if (NIVEL == 2) { sufijoNivel = "II"; }
        else if (NIVEL == 3) { sufijoNivel = "III"; }
        else if (NIVEL == 4) { sufijoNivel = "IV a"; }
        else if (NIVEL == 5) { sufijoNivel = "IV b"; }

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Passive\n";
            cuerpo += $"<color={colorEncabezado}><b>Trigger:</b></color> when the Duelist deals damage\n";
            cuerpo += $"<color={colorEncabezado}><b>Effect:</b></color> applies {nombreDebuff} for {duracion} turns\n";
            cuerpo += $"<color={colorEncabezado}><b>{nombreDebuff}:</b></color> attacks against that target gain +{bonusCritPorcentaje}% Crit";
            if (bonusDanioCritico > 0) { cuerpo += $", critical hits deal +{bonusDanioCritico}% damage"; }
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Gatilho:</b></color> ao causar dano\n";
            cuerpo += $"<color={colorEncabezado}><b>Efeito:</b></color> aplica {nombreDebuff} por {duracion} turnos\n";
            cuerpo += $"<color={colorEncabezado}><b>{nombreDebuff}:</b></color> ataques contra esse alvo ganham +{bonusCritPorcentaje}% Critico";
            if (bonusDanioCritico > 0) { cuerpo += $", criticos causam +{bonusDanioCritico}% de dano"; }
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Disparo:</b></color> al causar danio\n";
            cuerpo += $"<color={colorEncabezado}><b>Efecto:</b></color> aplica {nombreDebuff} por {duracion} turnos\n";
            cuerpo += $"<color={colorEncabezado}><b>{nombreDebuff}:</b></color> ataques contra ese objetivo ganan +{bonusCritPorcentaje}% Critico";
            if (bonusDanioCritico > 0) { cuerpo += $", criticos causan +{bonusDanioCritico}% de danio"; }
        }

        string titulo = esIngles ? "Revealing Attacks " + sufijoNivel : esPortugues ? "Ataques Reveladores " + sufijoNivel : "Ataques Reveladores " + sufijoNivel;
        string subtitulo = esIngles
            ? "Damaging hits expose the target to critical follow-up."
            : esPortugues
                ? "Golpes com dano expoem o alvo a criticos posteriores."
                : "Los golpes con danio exponen al objetivo a criticos posteriores.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +10% critical damage taken.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% Crit against exposed targets.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+15% critical damage taken) or Option B (+1 duration).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +10% de dano critico recebido.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Critico contra alvos expostos.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+15% de dano critico recebido) ou Opcao B (+1 duracao).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +10% danio critico recibido.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Critico contra objetivos expuestos.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+15% danio critico recibido) u Opcion B (+1 duracion).</color>"; }
        }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
