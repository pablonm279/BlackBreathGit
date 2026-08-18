using System.Collections.Generic;
using UnityEngine;

public class EnGarde : Habilidad
{
    private const string BuffNombre = "En Garde";
    private const string ColorPosturaDemandante = "#a84444";
    private readonly List<Unidad> objetivosPosibles = new List<Unidad>();

    public override void Awake()
    {
        nombre = "En Garde";
        IDenClase = 6;
        costoAP = 1;
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 0;
        esCargable = false;
        esMelee = false;
        esHostil = false;
        cooldownMax = NIVEL == 5 ? 4 : 5;
        bAfectaObstaculos = false;

        imHab = Resources.Load<Sprite>("imHab/Duelista_EnGarde");
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

        int bonusEvasion = ObtenerBonusEvasion();
        int bonusDanio = ObtenerBonusDanio();
        int bonusAtaque = 2;
        int bonusCriticoPorcentaje = 5;
        int enfriamiento = NIVEL == 5 ? 4 : 5;
        int umbralPosturaDemandante = NIVEL == 4 ? 10 : 5;

        string sufijoNivel = "I";
        if (NIVEL == 2) { sufijoNivel = "II"; }
        else if (NIVEL == 3) { sufijoNivel = "III"; }
        else if (NIVEL == 4) { sufijoNivel = "IV a"; }
        else if (NIVEL == 5) { sufijoNivel = "IV b"; }

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}  {enfriamiento} {iconoCooldown}";

        if (esIngles)
        {
            string evasion = TerminoDescripcion(TerminoDescripcionId.Evasion, "Evasion", "Estado_evasion");
            string ataque = TerminoDescripcion(TerminoDescripcionId.Ataque, "Attack");
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) proximaMejora = "+1 Evasion.";
                else if (NIVEL == 2) proximaMejora = "+5% damage.";
                else if (NIVEL == 3) proximaMejora = "Option A: Demanding Stance triggers above 10% max HP. Option B: -1 cooldown.";
            }

            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                "En Garde " + sufijoNivel,
                "Adopts a technical stance that improves offense and evasion.",
                new[]
                {
                    LineaDescripcion("Target", "Self"),
                    LineaDescripcion("Effect", $"+{bonusEvasion} {evasion}, +{bonusDanio}% damage, +{bonusCriticoPorcentaje}% {critico} and +{bonusAtaque} {ataque} (up to 3 turns)."),
                    LineaDescripcion("Ends", "Upon taking damage."),
                    LineaDescripcion($"<color={ColorPosturaDemandante}>Demanding Stance:</color>", $"While active, triggers when a hit deals more than {umbralPosturaDemandante}% max HP.")
                },
                proximaMejora);
            return;
        }
        if(esPortugues){string eva=TerminoDescripcion(TerminoDescripcionId.Evasion,"Evasão","Estado_evasion");string atk=TerminoDescripcion(TerminoDescripcionId.Ataque,"Ataque");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: +1 Evasão.":NIVEL==2?"Próximo nível: +5% de dano.":NIVEL==3?"Opção A: Postura Demandante ativa acima de 10% do HP máximo. Opção B: -1 de recarga.":null;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("En Garde "+sufijoNivel,"Adota uma postura técnica que melhora o ataque e a evasão.",new[]{LineaDescripcion("Alvo","A própria Duelista"),LineaDescripcion("Efeito",$"+{bonusEvasion} {eva}, +{bonusDanio}% de dano, +{bonusCriticoPorcentaje}% {crit} e +{bonusAtaque} {atk} (até 3 turnos)."),LineaDescripcion("Termina","Ao sofrer dano."),LineaDescripcion($"<color={ColorPosturaDemandante}>Postura Demandante:</color>",$"Enquanto estiver ativa, dispara quando um golpe causa mais de {umbralPosturaDemandante}% do HP máximo.")},prox);return;}
        {string eva=TerminoDescripcion(TerminoDescripcionId.Evasion,"Evasión","Estado_evasion");string atk=TerminoDescripcion(TerminoDescripcionId.Ataque,"Ataque");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: +1 Evasión.":NIVEL==2?"Próximo nivel: +5% de daño.":NIVEL==3?"Opción A: Postura Demandante se activa por encima del 10% del HP máximo. Opción B: -1 de enfriamiento.":null;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada("En Garde "+sufijoNivel,"Adopta una postura técnica que mejora el ataque y la evasión.",new[]{LineaDescripcion("Objetivo","La propia Duelista"),LineaDescripcion("Efecto",$"+{bonusEvasion} {eva}, +{bonusDanio}% de daño, +{bonusCriticoPorcentaje}% {crit} y +{bonusAtaque} {atk} (hasta 3 turnos)."),LineaDescripcion("Termina","Al recibir daño."),LineaDescripcion($"<color={ColorPosturaDemandante}>Postura Demandante:</color>",$"Mientras está activa, se dispara cuando un golpe inflige más del {umbralPosturaDemandante}% del HP máximo.")},prox);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Self buff\n";
            cuerpo += $"<color={colorEncabezado}><b>Effect:</b></color> max 3 turns, +{bonusEvasion} Evasion, +{bonusDanio}% Damage, +{bonusCriticoPorcentaje}% Crit, +{bonusAtaque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Cancel:</b></color> removed when taking damage\n";
            cuerpo += $"<color={ColorPosturaDemandante}><b>Demanding Stance:</b></color> trigger threshold becomes {umbralPosturaDemandante}% max HP while active";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Auto buff\n";
            cuerpo += $"<color={colorEncabezado}><b>Efeito:</b></color> maximo 3 turnos, +{bonusEvasion} Evasao, +{bonusDanio}% Dano, +{bonusCriticoPorcentaje}% Critico, +{bonusAtaque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Cancelamento:</b></color> removido ao receber dano\n";
            cuerpo += $"<color={ColorPosturaDemandante}><b>Postura Demandante:</b></color> limiar vira {umbralPosturaDemandante}% da vida maxima enquanto ativo";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Auto buff\n";
            cuerpo += $"<color={colorEncabezado}><b>Efecto:</b></color> máximo 3 turnos, +{bonusEvasion} Evasion, +{bonusDanio}% Daño, +{bonusCriticoPorcentaje}% Crítico, +{bonusAtaque}\n";
            cuerpo += $"<color={colorEncabezado}><b>Cancelación:</b></color> se elimina al recibir daño\n";
            cuerpo += $"<color={ColorPosturaDemandante}><b>Postura Demandante:</b></color> umbral pasa a {umbralPosturaDemandante}% de HP máximo mientras esta activo";
        }

        string titulo = esIngles ? "En Garde " + sufijoNivel : esPortugues ? "Em Guarda " + sufijoNivel : "En Garde " + sufijoNivel;
        string subtitulo = esIngles
            ? "Adopt a very technical pose that enhances offense and evasion."
            : esPortugues
                ? "Adote uma postura de guarda por um curto periodo para ofensiva e evasao."
                : "Utiliza una pose muy técnica que mejora la ofensiva y evasión.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Evasion.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +5% Damage.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (Demanding Stance at 10%) or Option B (-1 cooldown).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 Evasao.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +5% Dano.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (Postura Demandante em 10%) ou Opcao B (-1 recarga).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 Evasion.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +5% Daño.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (Postura Demandante al 10%) u Opción B (-1 enfriamiento).</color>"; }
        }
    }

    public override void Activar()
    {
        ObtenerObjetivos();
        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
        if (obj is not Unidad objetivo)
        {
            return;
        }

        objetivo.RemoverBuffNombre(BuffNombre);

        Buff buff = new Buff();
        buff.buffNombre = BuffNombre;
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = 3;
        buff.cantDanioPorcentaje = ObtenerBonusDanio();
        buff.cantCritDado = 1;
        buff.cantAtaque = 2;
        buff.AplicarBuff(objetivo);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);

        if (objetivo is ClaseDuelista duelista)
        {
            duelista.NotificarInicioEnGarde();
        }

        objetivo.Marcar(0);
    }

    private void ObtenerObjetivos()
    {
        objetivosPosibles.Clear();
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();

        if (scEstaUnidad == null)
        {
            return;
        }

        objetivosPosibles.Add(scEstaUnidad);
        if (scEstaUnidad.CasillaPosicion != null)
        {
            scEstaUnidad.CasillaPosicion.ActivarCapaColorAzul();
        }
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Add(scEstaUnidad);
    }

    private int ObtenerBonusDanio()
    {
        int bonus = 15;
        if (NIVEL > 2)
        {
            bonus += 5;
        }

        return bonus;
    }

    private int ObtenerBonusEvasion()
    {
        int bonus = 2;
        if (NIVEL > 1)
        {
            bonus += 1;
        }

        return bonus;
    }
}
