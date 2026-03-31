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

        int bonusCrit = 1;
        int bonusDanioCritico = 0;
        int duracion = 2;

        if (NIVEL > 1)
        {
            bonusDanioCritico += 10;
        }
        if (NIVEL > 2)
        {
            bonusCrit += 1;
        }
        if (NIVEL == 4)
        {
            bonusDanioCritico += 15;
        }
        if (NIVEL == 5)
        {
            duracion += 1;
        }

        string sufijoNivel = "I";
        if (NIVEL == 2) { sufijoNivel = "II"; }
        else if (NIVEL == 3) { sufijoNivel = "III"; }
        else if (NIVEL == 4) { sufijoNivel = "IV a"; }
        else if (NIVEL == 5) { sufijoNivel = "IV b"; }

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Passive\n";
            cuerpo += $"<b>Effect:</b> when the Duelist deals damage, applies {nombreDebuff} for {duracion} turns\n";
            cuerpo += $"<b>{nombreDebuff}:</b> attackers gain +{bonusCrit} crit range against that target";
            if (bonusDanioCritico > 0)
            {
                cuerpo += $", and critical hits deal +{bonusDanioCritico}% damage";
            }
            cuerpo += "\n";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Passiva\n";
            cuerpo += $"<b>Efeito:</b> ao causar dano, aplica {nombreDebuff} por {duracion} turnos\n";
            cuerpo += $"<b>{nombreDebuff}:</b> atacantes ganham +{bonusCrit} de faixa de critico contra esse alvo";
            if (bonusDanioCritico > 0)
            {
                cuerpo += $", e acertos criticos causam +{bonusDanioCritico}% de dano";
            }
            cuerpo += "\n";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Pasiva\n";
            cuerpo += $"<b>Efecto:</b> al danar, aplica {nombreDebuff} por {duracion} turnos\n";
            cuerpo += $"<b>{nombreDebuff}:</b> quienes ataquen a ese objetivo ganan +{bonusCrit} rango critico";
            if (bonusDanioCritico > 0)
            {
                cuerpo += $", y los criticos le causan +{bonusDanioCritico}% de danio";
            }
            cuerpo += "\n";
        }

        txtDescripcion = ConstruirDescripcionEstandar(
          esIngles ? "Revealing Attacks " + sufijoNivel : "Ataques Reveladores " + sufijoNivel,
          esIngles
            ? "Each damaging hit exposes the target and makes follow-up critical strikes easier."
            : esPortugues
              ? "Cada golpe que causa dano expoe o alvo e facilita os proximos acertos criticos."
              : "Cada golpe que hace dano expone al objetivo y facilita los siguientes golpes criticos.",
          cuerpo,
          "",
          "#5dade2");

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel)
        {
            return;
        }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +10% critical damage taken.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 extra crit range.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+15% critical damage taken) or Option B (+1 duration).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +10% de dano critico recebido.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 faixa de critico extra.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+15% de dano critico recebido) ou Opcao B (+1 duracao).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +10% dano critico recibido.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +1 rango critico extra.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (+15% dano critico recibido) u Opcion B (+1 duracion).</color>"; }
        }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
