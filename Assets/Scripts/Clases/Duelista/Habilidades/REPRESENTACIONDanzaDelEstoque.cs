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
        int bonusCritPorcentaje = buffCritico ? 5 : 0;

        string sufijoNivel = "I";
        if (NIVEL == 2) { sufijoNivel = "II"; }
        else if (NIVEL == 3) { sufijoNivel = "III"; }
        else if (NIVEL == 4) { sufijoNivel = "IV a"; }
        else if (NIVEL == 5) { sufijoNivel = "IV b"; }

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        if (esIngles)
        {
            string ap = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "AP", "ap");
            string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Attack");
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
            string danzando = TerminoDescripcion(TerminoDescripcionId.Danzando, "Dancing");
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) proximaMejora = "+5% max HP threshold.";
                else if (NIVEL == 2) proximaMejora = "+5% damage against targets within the threshold.";
                else if (NIVEL == 3) proximaMejora = "Option A: +1 AP on kill. Option B: Dancing grants +5% Crit.";
            }

            string buff = $"Stackable; +1 {ataque}, +15% damage";
            if (bonusCritPorcentaje > 0) buff += $", +{bonusCritPorcentaje}% {critico}";
            buff += " (1 turn).";
            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                "Sword Dance " + sufijoNivel,
                "Passive: Finishes weakened enemies and chains momentum on kills.",
                new[]
                {
                    LineaDescripcion("Effect", $"Deals +{bonusDanio}% damage to enemies at {umbralVida}% max HP or less."),
                    LineaDescripcion("On kill", $"During the Duelist's turn, immediately gains +{apAlMatar} {ap} and {danzando}."),
                    LineaDescripcion("Dancing", buff, 1)
                },
                proximaMejora);
            return;
        }
        if (TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaChinoSimplificado)
        {
            string apZh = TerminoDescripcion(TerminoDescripcionId.PuntosAccion, "行动点", "ap");
            string atkZh = TerminoDescripcion(TerminoDescripcionId.Ataque, "攻击");
            string critZh = TerminoDescripcion(TerminoDescripcionId.Critico, "暴击", "critico");
            string danzaZh = TerminoDescripcion(TerminoDescripcionId.Danzando, "舞动中");
            string proxZh = !DebeMostrarProximaMejoraDescripcion() ? null
                : NIVEL < 2 ? "生命值上限阈值 +5%。"
                : NIVEL == 2 ? "对阈值内目标的伤害 +5%。"
                : NIVEL == 3 ? "选项A：击杀时 +1 行动点。选项B：舞动中额外获得暴击率 +5%。"
                : null;
            string buffZh = $"可叠加；{atkZh} +1，伤害 +15%";
            if (bonusCritPorcentaje > 0) { buffZh += $"，{critZh} +{bonusCritPorcentaje}%"; }
            buffZh += "（持续 1 回合）。";
            txtDescripcion = ConstruirDescripcionNormalizadaLocalizada(
                "刺剑之舞 " + sufijoNivel,
                "被动：终结虚弱的敌人，并在击杀后延续攻势。",
                new[]
                {
                    LineaDescripcion("效果", $"对生命值不高于最大值 {umbralVida}% 的敌人造成的伤害 +{bonusDanio}%。"),
                    LineaDescripcion("击杀时", $"在决斗者的回合中，立即获得 +{apAlMatar} {apZh} 并进入{danzaZh}。"),
                    LineaDescripcion("舞动中", buffZh, 1)
                },
                proxZh,
                costoSuperior: string.Empty);
            return;
        }
        if(esPortugues){string ap=TerminoDescripcion(TerminoDescripcionId.PuntosAccion,"AP","ap");string atk=TerminoDescripcion(TerminoDescripcionId.Ataque,"Ataque");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string danca=TerminoDescripcion(TerminoDescripcionId.Danzando,"Dançando");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: +5% ao limiar de HP máximo.":NIVEL==2?"Próximo nível: +5% de dano contra alvos dentro do limiar.":NIVEL==3?"Opção A: +1 AP ao matar. Opção B: Dançando concede +5% de Crítico.":null;string buff=$"Acumulável; +1 {atk}, +15% de dano";if(bonusCritPorcentaje>0)buff+=$", +{bonusCritPorcentaje}% {crit}";buff+=" (1 turno).";txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("Dança da Espada "+sufijoNivel,"Passiva: finaliza inimigos enfraquecidos e encadeia impulso ao matar.",new[]{LineaDescripcion("Efeito",$"Causa +{bonusDanio}% de dano a inimigos com {umbralVida}% do HP máximo ou menos."),LineaDescripcion("Ao matar",$"Durante o turno da Duelista, recebe imediatamente +{apAlMatar} {ap} e {danca}."),LineaDescripcion("Dançando",buff,1)},prox,costoSuperior:string.Empty);return;}
        {string ap=TerminoDescripcion(TerminoDescripcionId.PuntosAccion,"AP","ap");string atk=TerminoDescripcion(TerminoDescripcionId.Ataque,"Ataque");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string danza=TerminoDescripcion(TerminoDescripcionId.Danzando,"Danzando");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: +5% al umbral de HP máximo.":NIVEL==2?"Próximo nivel: +5% de daño contra objetivos dentro del umbral.":NIVEL==3?"Opción A: +1 AP al matar. Opción B: Danzando otorga +5% de Crítico.":null;string buff=$"Acumulable; +1 {atk}, +15% de daño";if(bonusCritPorcentaje>0)buff+=$", +{bonusCritPorcentaje}% {crit}";buff+=" (1 turno).";txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("Danza de la Espada "+sufijoNivel,"Pasiva: remata enemigos debilitados y encadena impulso al matar.",new[]{LineaDescripcion("Efecto",$"Inflige +{bonusDanio}% de daño a enemigos con {umbralVida}% del HP máximo o menos."),LineaDescripcion("Al matar",$"Durante el turno de la Duelista, obtiene inmediatamente +{apAlMatar} {ap} y {danza}."),LineaDescripcion("Danzando",buff,1)},prox,costoSuperior:string.Empty);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Passive\n";
            cuerpo += $"<color={colorEncabezado}><b>Execution:</b></color> +{bonusDanio}% damage against enemies at {umbralVida}% max HP or less\n";
            cuerpo += $"<color={colorEncabezado}><b>On kill:</b></color> during own turn, gains +{apAlMatar} AP immediately\n";
            cuerpo += $"<color={colorEncabezado}><b>{nombreBuff}:</b></color> 1 turn, stackable, +1, +15% Damage";
            if (bonusCritPorcentaje > 0) { cuerpo += $", +{bonusCritPorcentaje}% Crit"; }
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Passiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Execucao:</b></color> +{bonusDanio}% de dano contra inimigos com {umbralVida}% da vida maxima ou menos\n";
            cuerpo += $"<color={colorEncabezado}><b>Ao matar:</b></color> no proprio turno, ganha +{apAlMatar} AP imediatamente\n";
            cuerpo += $"<color={colorEncabezado}><b>{nombreBuff}:</b></color> 1 turno, acumulavel, +1, +15% Dano";
            if (bonusCritPorcentaje > 0) { cuerpo += $", +{bonusCritPorcentaje}% Crítico"; }
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Pasiva\n";
            cuerpo += $"<color={colorEncabezado}><b>Ejecución:</b></color> +{bonusDanio}% de daño contra enemigos con {umbralVida}% de HP máximo o menos\n";
            cuerpo += $"<color={colorEncabezado}><b>Al matar:</b></color> en su propio turno, gana +{apAlMatar} AP inmediatamente\n";
            cuerpo += $"<color={colorEncabezado}><b>{nombreBuff}:</b></color> 1 turno, acumulable, +1, +15% Daño";
            if (bonusCritPorcentaje > 0) { cuerpo += $", +{bonusCritPorcentaje}% Crítico"; }
        }

        string titulo = esIngles ? "Sword Dance " + sufijoNivel : esPortugues ? "Danca do Estoque " + sufijoNivel : "Danza del Estoque " + sufijoNivel;
        string subtitulo = esIngles
            ? "Finish low-health enemies and chain AP on kills."
            : esPortugues
                ? "Finaliza inimigos feridos e encadeia AP ao matar."
                : "Remata enemigos heridos y encadena AP al matar.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% max HP threshold.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% damage against targets in threshold.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+1 AP on kill) or Option B (+5% Crit to Dancing).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% de limiar de vida máxima.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% de dano contra alvos no limiar.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+1 AP ao matar) ou Opcao B (+5% Critico em Dancando).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% al umbral de HP máximo enemigo.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% daño contra unidades en el umbral.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+1 AP al matar) u Opción B (+5% Crítico a Danzando).</color>"; }
        }
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada) { }

    public override void Activar() { }
}
