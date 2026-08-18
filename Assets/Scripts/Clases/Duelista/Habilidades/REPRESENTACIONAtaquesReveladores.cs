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
        if (esIngles)
        {
            string vulnerabilidad = TerminoDescripcion(TerminoDescripcionId.VulnerabilidadExpuesta, "Exposed Vulnerability");
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) proximaMejora = "+10% critical damage taken.";
                else if (NIVEL == 2) proximaMejora = "+5% Crit against exposed targets.";
                else if (NIVEL == 3) proximaMejora = "Option A: +15% critical damage taken. Option B: +1 turn duration.";
            }

            string efectoVulnerabilidad = $"Attacks against the target gain +{bonusCritPorcentaje}% {critico}";
            if (bonusDanioCritico > 0) efectoVulnerabilidad += $"; critical hits deal +{bonusDanioCritico}% damage";
            efectoVulnerabilidad += ".";
            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                "Revealing Attacks " + sufijoNivel,
                "Passive: Damaging enemies exposes them to critical follow-up attacks.",
                new[]
                {
                    LineaDescripcion("Trigger", "Deals damage to an enemy."),
                    LineaDescripcion("Effect", $"Applies {vulnerabilidad} ({duracion} turns; stackable)."),
                    LineaDescripcion("Exposed", efectoVulnerabilidad, 1)
                },
                proximaMejora);
            return;
        }
        if(esPortugues){string vuln=TerminoDescripcion(TerminoDescripcionId.VulnerabilidadExpuesta,"Vulnerabilidade Exposta");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: +10% de dano crítico sofrido.":NIVEL==2?"Próximo nível: +5% de Crítico contra alvos expostos.":NIVEL==3?"Opção A: +15% de dano crítico sofrido. Opção B: +1 turno de duração.":null;string ef=$"Ataques contra o alvo recebem +{bonusCritPorcentaje}% {crit}";if(bonusDanioCritico>0)ef+=$"; acertos críticos causam +{bonusDanioCritico}% de dano";ef+=".";txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("Ataques Reveladores "+sufijoNivel,"Passiva: causar dano aos inimigos os expõe a ataques críticos posteriores.",new[]{LineaDescripcion("Ativação","Causa dano a um inimigo."),LineaDescripcion("Efeito",$"Aplica {vuln} ({duracion} turnos; acumulável)."),LineaDescripcion("Exposto",ef,1)},prox,costoSuperior:string.Empty);return;}
        {string vuln=TerminoDescripcion(TerminoDescripcionId.VulnerabilidadExpuesta,"Vulnerabilidad Expuesta");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: +10% de daño crítico recibido.":NIVEL==2?"Próximo nivel: +5% de Crítico contra objetivos expuestos.":NIVEL==3?"Opción A: +15% de daño crítico recibido. Opción B: +1 turno de duración.":null;string ef=$"Los ataques contra el objetivo obtienen +{bonusCritPorcentaje}% {crit}";if(bonusDanioCritico>0)ef+=$"; los impactos críticos infligen +{bonusDanioCritico}% de daño";ef+=".";txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("Ataques Reveladores "+sufijoNivel,"Pasiva: infligir daño a los enemigos los expone a ataques críticos posteriores.",new[]{LineaDescripcion("Activación","Inflige daño a un enemigo."),LineaDescripcion("Efecto",$"Aplica {vuln} ({duracion} turnos; acumulable)."),LineaDescripcion("Expuesto",ef,1)},prox,costoSuperior:string.Empty);return;}

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
            cuerpo += $"<color={colorEncabezado}><b>Disparo:</b></color> al causar daño\n";
            cuerpo += $"<color={colorEncabezado}><b>Efecto:</b></color> aplica {nombreDebuff} por {duracion} turnos\n";
            cuerpo += $"<color={colorEncabezado}><b>{nombreDebuff}:</b></color> ataques contra ese objetivo ganan +{bonusCritPorcentaje}% Crítico";
            if (bonusDanioCritico > 0) { cuerpo += $", críticos causan +{bonusDanioCritico}% de daño"; }
        }

        string titulo = esIngles ? "Revealing Attacks " + sufijoNivel : esPortugues ? "Ataques Reveladores " + sufijoNivel : "Ataques Reveladores " + sufijoNivel;
        string subtitulo = esIngles
            ? "Damaging hits expose the target to critical follow-up."
            : esPortugues
                ? "Golpes com dano expoem o alvo a criticos posteriores."
                : "Los golpes con daño exponen al objetivo a críticos posteriores.";

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
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% Crítico contra alvos expostos.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+15% de dano critico recebido) ou Opcao B (+1 duracao).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +10% daño crítico recibido.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% Crítico contra objetivos expuestos.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+15% daño crítico recibido) u Opción B (+1 duración).</color>"; }
        }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
