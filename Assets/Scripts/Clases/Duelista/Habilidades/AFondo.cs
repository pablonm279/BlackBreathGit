using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AFondo : Habilidad
{
    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaque;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int bonusDanioPlano;
    [SerializeField] private int criticoRangoHab;
    [SerializeField] private int tipoDanio;
    [SerializeField] private int dcFortitud;
    [SerializeField] private int sangradoAplicado;

    private Casilla origen;
    private readonly List<Unidad> lObjetivosPosibles = new List<Unidad>();

    public override void Awake()
    {
        nombre = "A Fondo";
        IDenClase = 5;
        costoAP = 3;
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = false;
        enArea = 0;
        esforzable = 1;
        esCargable = false;
        esMelee = true;
        esHostil = true;
        cooldownMax = 2;
        bAfectaObstaculos = false;
        tipoPorcentaje = 1;
        targetEspecial = 10;

        bonusAtaque = 2;
        XdDanio = 1;
        daniodX = 10;
        bonusDanioPlano = NIVEL > 1 ? 5 : 3;
        criticoRangoHab = NIVEL == 5 ? 1 : 0;
        tipoDanio = 2; // Perforante
        dcFortitud = 12;
        sangradoAplicado = NIVEL == 4 ? 5 : 3;
        penetracionArmadura = NIVEL > 2 ? 2 : 1;

        imHab = Resources.Load<Sprite>("imHab/Duelista_A_Fondo");
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
        var statsUI = ObtenerStatsDescripcionUI();

        int fuerzaActual = statsUI.Fuerza;
        int agilidadActual = statsUI.Agilidad;
        int atributoMixtoActual = ObtenerAtributoMixto(fuerzaActual, agilidadActual);
        int ataqueActual = statsUI.Ataque;
        int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
        int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
        string rangoDanio = FormatearRangoDados(1, 10, bonusDanioPlano);

        string tituloEs = "A Fondo I";
        string tituloEn = "All In I";
        string tituloPt = "A Fondo I";
        if (NIVEL == 2) { tituloEs = "A Fondo II"; tituloEn = "All In II"; tituloPt = "A Fondo II"; }
        if (NIVEL == 3) { tituloEs = "A Fondo III"; tituloEn = "All In III"; tituloPt = "A Fondo III"; }
        if (NIVEL == 4) { tituloEs = "A Fondo IV a"; tituloEn = "All In IV a"; tituloPt = "A Fondo IV a"; }
        if (NIVEL == 5) { tituloEs = "A Fondo IV b"; tituloEn = "All In IV b"; tituloPt = "A Fondo IV b"; }

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string colorFuerza = "#d9822b";
        string colorAgilidad = "#7fa35a";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
        string atributo = esIngles
            ? $"<color={colorFuerza}>Strength</color>/<color={colorAgilidad}>Agility</color> ({atributoMixtoActual})"
            : esPortugues
                ? $"<color={colorFuerza}>Forca</color>/<color={colorAgilidad}>Agilidade</color> ({atributoMixtoActual})"
                : $"<color={colorFuerza}>Fuerza</color>/<color={colorAgilidad}>Agilidad</color> ({atributoMixtoActual})";
        string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaque);

        if (esIngles)
        {
            string fuerza = TerminoDescripcion(TerminoDescripcionId.Fuerza, "Strength");
            string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, "Agility");
            string atributoMixto = $"{fuerza}/{agilidad} ({atributoMixtoActual})";
            string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
            string fortaleza = TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza, "Fortitude", "ic_fortaleza");
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
            string danioPerforante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "Piercing damage", "dano_perforante");
            string sangrado = TerminoDescripcion(TerminoDescripcionId.Sangrado, "Bleed", "Estado_sangrano");
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) proximaMejora = "+2 damage.";
                else if (NIVEL == 2) proximaMejora = "+1 Armor Penetration.";
                else if (NIVEL == 3) proximaMejora = "Option A: +2 Bleed. Option B: +5% Crit.";
            }

            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                tituloEn,
                "Pierces a target and the 2 rear diagonals, potentially causing Bleed.",
                new List<LineaDescripcionNormalizada>
                {
                    LineaDescripcion("Target", "1 enemy and the 2 rear diagonal tiles"),
                    LineaDescripcion("Effect", $"On hit, deals {rangoDanio} as {danioPerforante}."),
                    LineaDescripcion("Attack Roll", $"1d20 + {atributoMixto}{bonusTirada} vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%."),
                    LineaDescripcion("Penetration", $"Armor Penetration: {penetracionArmadura}."),
                    LineaDescripcion("Save", $"Target's {fortaleza} vs DC {dcFortitud}.", 1),
                    LineaDescripcion("Failed save", $"If damaged, gains {sangradoAplicado} {sangrado}.", 1),
                    LineaDescripcion("Effort", $"Up to {esforzable} AP.")
                },
                proximaMejora,
                mostrarIconoMelee: true);
            return;
        }
        if(esPortugues){string forca=TerminoDescripcion(TerminoDescripcionId.Fuerza,"Força");string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,"Agilidade");string mix=$"{forca}/{agi} ({atributoMixtoActual})";string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defesa","IconoDefensa");string fort=TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza,"Fortitude","ic_fortaleza");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string dano=TerminoDescripcion(TerminoDescripcionId.DanioPerforante,"dano Perfurante","dano_perforante");string sang=TerminoDescripcion(TerminoDescripcionId.Sangrado,"Sangramento","Estado_sangrano");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: +2 de dano.":NIVEL==2?"Próximo nível: +1 Penetração de Armadura.":NIVEL==3?"Opção A: +2 Sangramento. Opção B: +5% de Crítico.":null;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloPt,"Perfura um alvo e as 2 diagonais traseiras, podendo causar Sangramento.",new List<LineaDescripcionNormalizada>{LineaDescripcion("Alvo","1 inimigo e as 2 casas diagonais traseiras"),LineaDescripcion("Efeito",$"Ao acertar, causa {rangoDanio} como {dano}."),LineaDescripcion("Rolagem de Ataque",$"1d20 + {mix}{bonusTirada} vs {def}. Falha crítica: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion("Penetração",$"Penetração de Armadura: {penetracionArmadura}."),LineaDescripcion("Salvaguarda",$"{fort} do alvo vs CD {dcFortitud}.",1),LineaDescripcion("Falha",$"Se sofrer dano, recebe {sangradoAplicado} {sang}.",1),LineaDescripcion("Esforço",$"Até {esforzable} AP.")},prox,mostrarIconoMelee:true);return;}
        {string fuerza=TerminoDescripcion(TerminoDescripcionId.Fuerza,"Fuerza");string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,"Agilidad");string mix=$"{fuerza}/{agi} ({atributoMixtoActual})";string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defensa","IconoDefensa");string fort=TerminoDescripcion(TerminoDescripcionId.SalvacionFortaleza,"Fortaleza","ic_fortaleza");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string dano=TerminoDescripcion(TerminoDescripcionId.DanioPerforante,"daño Perforante","dano_perforante");string sang=TerminoDescripcion(TerminoDescripcionId.Sangrado,"Sangrado","Estado_sangrano");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: +2 de daño.":NIVEL==2?"Próximo nivel: +1 Penetración de Armadura.":NIVEL==3?"Opción A: +2 Sangrado. Opción B: +5% de Crítico.":null;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloEs,"Perfora a un objetivo y las 2 diagonales traseras, con posibilidad de causar Sangrado.",new List<LineaDescripcionNormalizada>{LineaDescripcion("Objetivo","1 enemigo y las 2 casillas diagonales traseras"),LineaDescripcion("Efecto",$"Al impactar, inflige {rangoDanio} como {dano}."),LineaDescripcion("Tirada de Ataque",$"1d20 + {mix}{bonusTirada} vs {def}. Pifia: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion("Penetración",$"Penetración de Armadura: {penetracionArmadura}."),LineaDescripcion("Salvación",$"{fort} del objetivo vs CD {dcFortitud}.",1),LineaDescripcion("Salvación fallida",$"Si recibe daño, obtiene {sangradoAplicado} {sang}.",1),LineaDescripcion("Esfuerzo",$"Hasta {esforzable} AP.")},prox,mostrarIconoMelee:true);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Melee attack\n";
            cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> selected enemy tile and the 2 rear diagonal tiles\n";
            cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} vs Defense\n";
            cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio}. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
            cuerpo += $"<color={colorEncabezado}><b>Armor Penetration:</b></color> {penetracionArmadura}\n";
            cuerpo += $"<color={colorEncabezado}><b>Save:</b></color> Fortitude vs DC {dcFortitud}\n";
            cuerpo += $"<color={colorEncabezado}><b>On failed save:</b></color> if damaged, +{sangradoAplicado} Bleed\n";
            cuerpo += $"<color={colorEncabezado}><b>Effortable:</b></color> yes ({esforzable})";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> casa inimiga escolhida e as 2 diagonais de tras\n";
            cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} vs Defesa\n";
            cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio}. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
            cuerpo += $"<color={colorEncabezado}><b>Penetracao de armadura:</b></color> {penetracionArmadura}\n";
            cuerpo += $"<color={colorEncabezado}><b>Resistencia:</b></color> Fortitude vs CD {dcFortitud}\n";
            cuerpo += $"<color={colorEncabezado}><b>Se falhar:</b></color> se causou dano, +{sangradoAplicado} Sangramento\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforcavel:</b></color> sim ({esforzable})";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee\n";
            cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> casilla enemiga elegida y las 2 diagonales de atras\n";
            cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} vs Defensa\n";
            cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio}. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
            cuerpo += $"<color={colorEncabezado}><b>Penetracion de armadura:</b></color> {penetracionArmadura}\n";
            cuerpo += $"<color={colorEncabezado}><b>TS:</b></color> Fortaleza vs DC {dcFortitud}\n";
            cuerpo += $"<color={colorEncabezado}><b>Si falla:</b></color> si recibio daño, +{sangradoAplicado} Sangrado\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforzable:</b></color> si ({esforzable})";
        }

        string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
        string subtitulo = esIngles
            ? "Thrust through the target and rear diagonals; can inflict Bleed."
            : esPortugues
                ? "Atravessa o alvo e diagonais traseiras; pode causar Sangramento."
                : "Atraviesa el objetivo y diagonales traseras; puede causar Sangrado.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +2 damage.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +1 Armor Penetration.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (+2 Bleed) or Option B (+5% Crit).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +2 de dano.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 de Penetracao de armadura.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (+2 Sangramento) ou Opcao B (+5% Critico).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +2 de daño.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +1 de Penetracion de armadura.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (+2 Sangrado) u Opción B (+5% Crítico).</color>"; }
        }
    }


    private string TextoModificadorDescripcion(int valor)
    {
        if (valor > 0) { return $" + {valor}"; }
        if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
        return "";
    }
    public override void Activar()
    {
        origen = Usuario.GetComponent<Unidad>().CasillaPosicion;
        ObtenerObjetivos();

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return base.EsperarPreImpactoAsync(objetivos, casillaOrigenTrampas);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return base.EsperarPostImpactoAsync(objetivos, casillaOrigenTrampas);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
        if (obj is not Unidad objetivo)
        {
            return;
        }

        int atributoMixto = ObtenerAtributoMixtoActual();
        float defensaObjetivo = objetivo.ObtenerdefensaActual();
        float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
        int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, atributoMixto, bonusAtaque, criticoRango, objetivo, 0);

        if (resultadoTirada == -1)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
            scEstaUnidad.EstablecerAPActualA(0);
        }
        else if (resultadoTirada == 0)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, esMelee);
        }
        else
        {
            float hpAntes = objetivo.HP_actual;
            ClaseDuelista duelista = scEstaUnidad as ClaseDuelista;
            int bonusDanioPorcentajeDanza = duelista != null ? duelista.ObtenerBonusDanioPorcentajeDanzaDelEstoque(objetivo) : 0;
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX) + bonusDanioPlano;
            danio = danio / 100 * (100 + scEstaUnidad.mod_DanioPorcentaje + bonusDanioPorcentajeDanza);

            if (resultadoTirada == 1)
            {
                danio -= danio / 2f;
                objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
            }
            else if (resultadoTirada == 2)
            {
                objetivo.RecibirDanio(danio, tipoDanio, false, scEstaUnidad);
            }
            else if (resultadoTirada == 3)
            {
                objetivo.RecibirDanio(danio, tipoDanio, true, scEstaUnidad);
            }

            VFXAplicar(objetivo.gameObject);
            if (objetivo.HP_actual < hpAntes && objetivo.TiradaSalvacion(1, dcFortitud))
            {
                Estados.Aplicar_Sangrado(objetivo, sangradoAplicado, scEstaUnidad);
            }
        }

        objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }

    private void ObtenerObjetivos()
    {
        lObjetivosPosibles.Clear();
        lCasillasafectadas.Clear();
        HashSet<Unidad> objetivosUnicos = new HashSet<Unidad>();

        int rangoPlus = 0;
        if (esMelee)
        {
            if (Usuario.GetComponent<Unidad>().CasillaPosicion.posX == 3)
            {
                rangoPlus = AumentarRangoMelee();
            }

            if (TieneObstaculooUnidadAdelanteDeSuLado() != 0)
            {
                rangoPlus++;
            }
        }

        List<Casilla> casillasPrimarias = origen.ObtenerCasillasRango(1 + rangoPlus, 0);
        foreach (Casilla c in casillasPrimarias)
        {
            c.ActivarCapaColorRojo();
            if (esMelee && c.transform.GetChild(2).gameObject.activeInHierarchy)
            {
                c.DesactivarCapaColorRojo();
            }

            lCasillasafectadas.Add(c);
            AgregarUnidadSiExiste(c, objetivosUnicos);

            int posXAtras = c.posX - 1;
            AgregarUnidadSiExiste(ObtenerCasillaPorPos(c.ladoGO, posXAtras, c.posY - 1), objetivosUnicos);
            AgregarUnidadSiExiste(ObtenerCasillaPorPos(c.ladoGO, posXAtras, c.posY + 1), objetivosUnicos);
        }

        lObjetivosPosibles.AddRange(objetivosUnicos);
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);
        BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    }

    private void AgregarUnidadSiExiste(Casilla casilla, HashSet<Unidad> objetivosUnicos)
    {
        if (casilla == null || casilla.Presente == null)
        {
            return;
        }

        Unidad unidad = casilla.Presente.GetComponent<Unidad>();
        if (unidad == null || unidad.CasillaPosicion == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return;
        }

        if (unidad.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
        {
            return;
        }

        objetivosUnicos.Add(unidad);
    }

    private Casilla ObtenerCasillaPorPos(GameObject ladoGO, int posX, int posY)
    {
        if (ladoGO == null)
        {
            return null;
        }

        foreach (Transform child in ladoGO.transform)
        {
            Casilla cas = child.GetComponent<Casilla>();
            if (cas != null && cas.posX == posX && cas.posY == posY)
            {
                return cas;
            }
        }

        return null;
    }

    private void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_Estocada");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;
        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }

    private int AumentarRangoMelee()
    {
        int posYorigen = scEstaUnidad.CasillaPosicion.posY;
        List<Casilla> casillasAdyacentesyFrenteColumna1 = new List<Casilla>();
        List<Casilla> casillasAdyacentesyFrenteColumna2 = new List<Casilla>();

        foreach (Transform child in origen.ladoOpuesto.transform)
        {
            Casilla cas = child.GetComponent<Casilla>();
            if (cas.posX == 3)
            {
                int calculo = Math.Abs(cas.posY - posYorigen);
                if (calculo < 2)
                {
                    casillasAdyacentesyFrenteColumna1.Add(cas);
                }
            }

            if (cas.posX == 2)
            {
                int calculo = Math.Abs(cas.posY - posYorigen);
                if (calculo < 2)
                {
                    casillasAdyacentesyFrenteColumna2.Add(cas);
                }
            }
        }

        foreach (Casilla cas in casillasAdyacentesyFrenteColumna1)
        {
            if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad))
            {
                return 0;
            }
        }
        foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna1)
        {
            casOsc.ActivarCapaColorNegro();
        }

        foreach (Casilla cas in casillasAdyacentesyFrenteColumna2)
        {
            if (cas.BloqueaAvanceMeleeDesdeFila(posYorigen, scEstaUnidad))
            {
                return 1;
            }
        }
        foreach (Casilla casOsc in casillasAdyacentesyFrenteColumna2)
        {
            casOsc.ActivarCapaColorNegro();
        }

        return 2;
    }

    private int TieneObstaculooUnidadAdelanteDeSuLado()
    {
        int orX = origen.posX;
        int orY = origen.posY;
        GameObject lado = origen.ladoGO;

        if (orX != 2)
        {
            return 0;
        }

        Casilla casillaRevisar = null;
        foreach (Transform child in lado.transform)
        {
            Casilla cas = child.GetComponent<Casilla>();
            if ((cas.posY == orY) && (cas.posX == orX + 1))
            {
                casillaRevisar = cas;
            }
        }

        if (casillaRevisar != null && casillaRevisar.Presente != null)
        {
            if (casillaRevisar.Presente.GetComponent<Unidad>() != null)
            {
                return 1;
            }

            Obstaculo obstaculo = casillaRevisar.Presente.GetComponent<Obstaculo>();
            if (obstaculo != null && obstaculo.bPermiteAtacarDetras)
            {
                return 2;
            }
        }

        return 0;
    }

    private int ObtenerAtributoMixtoActual()
    {
        return ObtenerAtributoMixto(scEstaUnidad.mod_CarFuerza, scEstaUnidad.mod_CarAgilidad);
    }

    private int ObtenerAtributoMixto(float fuerza, float agilidad)
    {
        return Mathf.CeilToInt((fuerza + agilidad) / 2f);
    }

}
