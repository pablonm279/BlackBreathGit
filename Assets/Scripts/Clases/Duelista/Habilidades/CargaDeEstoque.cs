using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CargaDeEstoque : Habilidad
{
    [SerializeField] private GameObject VFXenObjetivo;
    [SerializeField] private int bonusAtaqueBase;
    [SerializeField] private int bonusAtaquePorCasilla;
    [SerializeField] private int XdDanio;
    [SerializeField] private int daniodX;
    [SerializeField] private int bonusDanioBase;
    [SerializeField] private int bonusDanioNivel2;
    [SerializeField] private int bonusDanioPorCasilla;
    [SerializeField] private int bonusDanioPorCasillaNivel5;
    [SerializeField] private int criticoRangoHab;
    [SerializeField] private int tipoDanio;

    private Casilla origen;
    private int casillasAvanzadasUltimoUso;
    private readonly List<Unidad> lObjetivosPosibles = new List<Unidad>();


    public override void Awake()
    {
        nombre = "Carga de Estoque";
        IDenClase = 3;
        costoAP = 4;
        if (NIVEL == 4)
        {
            costoAP -= 1;
        }
        costoPM = 0;
        Usuario = gameObject;
        scEstaUnidad = Usuario.GetComponent<Unidad>();
        esZonal = true;
        enArea = 0;
        esforzable = 1;
        esCargable = false;
        esMelee = true;
        esHostil = true;
        cooldownMax = 2; //2
        bAfectaObstaculos = false;
        targetEspecial = 1;
        fuerzaPoseAtaque = true;

        bonusAtaqueBase = 0;
        bonusAtaquePorCasilla = 2;
        XdDanio = 1;
        daniodX = 8;
        bonusDanioBase = 5;
        bonusDanioNivel2 = NIVEL > 1 ? 3 : 0;
        bonusDanioPorCasilla = 3;
        bonusDanioPorCasillaNivel5 = NIVEL == 5 ? 2 : 0;
        criticoRangoHab = 0;
        tipoDanio = 2;

        imHab = Resources.Load<Sprite>("imHab/Duelista_CargaDeEstoque");
        if (imHab == null)
        {
            imHab = Resources.Load<Sprite>("imHab/Duelista_habilidad");
        }

        ActualizarDescripcion();
        tipoPorcentaje = 2;
    }


    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;
        var statsUI = ObtenerStatsDescripcionUI();

        int agilidadActual = statsUI.Agilidad;
        int ataqueActual = statsUI.Ataque;
        int criticoBaseMin = Mathf.Clamp(19 - (statsUI.CriticoRango + criticoRangoHab), 2, 20);
        int criticoPorcentaje = Mathf.Clamp(21 - criticoBaseMin, 0, 20) * 5;
        int danioPorCasillaActual = bonusDanioPorCasilla + bonusDanioPorCasillaNivel5;
        string rangoDanio = FormatearRangoDados(1, 8, bonusDanioBase + bonusDanioNivel2);

        string tituloEs = "Carga de Estoque I";
        string tituloEn = "Estoc Charge I";
        string tituloPt = "Carga de Estoque I";
        if (NIVEL == 2) { tituloEs = "Carga de Estoque II"; tituloEn = "Estoc Charge II"; tituloPt = "Carga de Estoque II"; }
        if (NIVEL == 3) { tituloEs = "Carga de Estoque III"; tituloEn = "Estoc Charge III"; tituloPt = "Carga de Estoque III"; }
        if (NIVEL == 4) { tituloEs = "Carga de Estoque IV a"; tituloEn = "Estoc Charge IV a"; tituloPt = "Carga de Estoque IV a"; }
        if (NIVEL == 5) { tituloEs = "Carga de Estoque IV b"; tituloEn = "Estoc Charge IV b"; tituloPt = "Carga de Estoque IV b"; }

        string colorTitulo = "#5dade2";
        string colorEncabezado = "#44d3ec";
        string colorAgilidad = "#7fa35a";
        string iconoAP = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"ap\"></voffset></size><space=-0.35em>";
        string iconoCooldown = "<space=0.55em><size=150%><voffset=0.34em><sprite name=\"cooldown\"></voffset></size><space=-0.35em>";
        string costoSuperior = $"{costoAP} {iconoAP}  {cooldownMax} {iconoCooldown}";
        string atributo = esIngles
            ? $"<color={colorAgilidad}>Agility ({agilidadActual})</color>"
            : esPortugues
                ? $"<color={colorAgilidad}>Agilidade ({agilidadActual})</color>"
                : $"<color={colorAgilidad}>Agilidad ({agilidadActual})</color>";
        string bonusTirada = TextoModificadorDescripcion(ataqueActual) + TextoModificadorDescripcion(bonusAtaqueBase);

        string reglaCruceEn = NIVEL > 2 ? "cannot cross obstacles; can cross allies" : "cannot cross obstacles or allies";
        string reglaCrucePt = NIVEL > 2 ? "nao pode atravessar obstáculos; pode atravessar aliados" : "nao pode atravessar obstáculos nem aliados";
        string reglaCruceEs = NIVEL > 2 ? "no puede atravesar obstáculos; puede atravesar aliados" : "no puede atravesar obstáculos ni aliados";

        if (esIngles)
        {
            string agilidad = TerminoDescripcion(TerminoDescripcionId.Agilidad, $"Agility ({agilidadActual})");
            string defensa = TerminoDescripcion(TerminoDescripcionId.Defensa, "Defense", "IconoDefensa");
            string critico = TerminoDescripcion(TerminoDescripcionId.Critico, "Crit", "critico");
            string danioPerforante = TerminoDescripcion(TerminoDescripcionId.DanioPerforante, "Piercing damage", "dano_perforante");
            string proximaMejora = null;
            if (DebeMostrarProximaMejoraDescripcion())
            {
                if (NIVEL < 2) proximaMejora = "+3 damage.";
                else if (NIVEL == 2) proximaMejora = "Can cross allies.";
                else if (NIVEL == 3) proximaMejora = "Option A: -1 AP cost. Option B: +2 damage per tile advanced.";
            }

            txtDescripcion = ConstruirDescripcionNormalizadaIngles(
                tituloEn,
                "Charges through enemies in the same row.",
                new List<LineaDescripcionNormalizada>
                {
                    LineaDescripcion("Target", "All enemies in 1 row"),
                    LineaDescripcion("Effect", $"Moves to the front column and attacks each target for {rangoDanio} + {agilidad} + {danioPorCasillaActual} per tile advanced as {danioPerforante}."),
                    LineaDescripcion("Path", NIVEL > 2 ? "Can cross allies, but not obstacles." : "Cannot cross allies or obstacles."),
                    LineaDescripcion("Attack Roll", $"1d20 + {agilidad}{bonusTirada} + {bonusAtaquePorCasilla} per tile advanced vs {defensa}. Fumble: 5%. {critico}: {criticoPorcentaje}%."),
                    LineaDescripcion("Effort", $"Up to {esforzable} AP.")
                },
                proximaMejora,
                mostrarIconoMelee: true);
            return;
        }
        if(esPortugues){string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,$"Agilidade ({agilidadActual})");string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defesa","IconoDefensa");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string dano=TerminoDescripcion(TerminoDescripcionId.DanioPerforante,"dano Perfurante","dano_perforante");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nível: +3 de dano.":NIVEL==2?"Próximo nível: pode atravessar aliados.":NIVEL==3?"Opção A: -1 de custo de AP. Opção B: +2 de dano por casa avançada.":null;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloPt,"Avança através de inimigos na mesma fileira.",new List<LineaDescripcionNormalizada>{LineaDescripcion("Alvo","Todos os inimigos em 1 fileira"),LineaDescripcion("Efeito",$"Move-se para a coluna frontal e ataca cada alvo, causando {rangoDanio} + {agi} + {danioPorCasillaActual} por casa avançada como {dano}."),LineaDescripcion("Trajeto",NIVEL>2?"Pode atravessar aliados, mas não obstáculos.":"Não pode atravessar aliados nem obstáculos."),LineaDescripcion("Rolagem de Ataque",$"1d20 + {agi}{bonusTirada} + {bonusAtaquePorCasilla} por casa avançada vs {def}. Falha crítica: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion("Esforço",$"Até {esforzable} AP.")},prox,mostrarIconoMelee:true);return;}
        {string agi=TerminoDescripcion(TerminoDescripcionId.Agilidad,$"Agilidad ({agilidadActual})");string def=TerminoDescripcion(TerminoDescripcionId.Defensa,"Defensa","IconoDefensa");string crit=TerminoDescripcion(TerminoDescripcionId.Critico,"Crítico","critico");string dano=TerminoDescripcion(TerminoDescripcionId.DanioPerforante,"daño Perforante","dano_perforante");string prox=!DebeMostrarProximaMejoraDescripcion()?null:NIVEL<2?"Próximo nivel: +3 de daño.":NIVEL==2?"Próximo nivel: puede atravesar aliados.":NIVEL==3?"Opción A: -1 de costo de AP. Opción B: +2 de daño por casilla avanzada.":null;txtDescripcion=ConstruirDescripcionNormalizadaLocalizada(tituloEs,"Carga a través de los enemigos de la misma fila.",new List<LineaDescripcionNormalizada>{LineaDescripcion("Objetivo","Todos los enemigos en 1 fila"),LineaDescripcion("Efecto",$"Se mueve a la columna frontal y ataca a cada objetivo, infligiendo {rangoDanio} + {agi} + {danioPorCasillaActual} por casilla avanzada como {dano}."),LineaDescripcion("Trayecto",NIVEL>2?"Puede atravesar aliados, pero no obstáculos.":"No puede atravesar aliados ni obstáculos."),LineaDescripcion("Tirada de Ataque",$"1d20 + {agi}{bonusTirada} + {bonusAtaquePorCasilla} por casilla avanzada vs {def}. Pifia: 5%. {crit}: {criticoPorcentaje}%."),LineaDescripcion("Esfuerzo",$"Hasta {esforzable} AP.")},prox,mostrarIconoMelee:true);return;}

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += $"<color={colorEncabezado}><b>Type:</b></color> Melee row attack\n";
            cuerpo += $"<color={colorEncabezado}><b>Target:</b></color> enemies on the same row\n";
            cuerpo += $"<color={colorEncabezado}><b>Cast rule:</b></color> {reglaCruceEn}\n";
            cuerpo += $"<color={colorEncabezado}><b>Roll:</b></color> 1d20 + {atributo}{bonusTirada} + {bonusAtaquePorCasilla} per tile advanced vs Defense\n";
            cuerpo += $"<color={colorEncabezado}><b>Fumble:</b></color> 5%   <color={colorEncabezado}><b>Crit:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Damage:</b></color> {rangoDanio} + {atributo} + {danioPorCasillaActual} per tile advanced. <color={colorEncabezado}><b>Type:</b></color> Piercing\n";
            cuerpo += $"<color={colorEncabezado}><b>Effortable:</b></color> yes ({esforzable})";
        }
        else if (esPortugues)
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee em linha\n";
            cuerpo += $"<color={colorEncabezado}><b>Alvo:</b></color> inimigos na mesma linha\n";
            cuerpo += $"<color={colorEncabezado}><b>Regra de uso:</b></color> {reglaCrucePt}\n";
            cuerpo += $"<color={colorEncabezado}><b>Rolagem:</b></color> 1d20 + {atributo}{bonusTirada} + {bonusAtaquePorCasilla} por casa avancada vs Defesa\n";
            cuerpo += $"<color={colorEncabezado}><b>Falha critica:</b></color> 5%   <color={colorEncabezado}><b>Critico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Dano:</b></color> {rangoDanio} + {atributo} + {danioPorCasillaActual} por casa avancada. <color={colorEncabezado}><b>Tipo:</b></color> Perfurante\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforcavel:</b></color> sim ({esforzable})";
        }
        else
        {
            cuerpo += $"<color={colorEncabezado}><b>Tipo:</b></color> Ataque melee en fila\n";
            cuerpo += $"<color={colorEncabezado}><b>Objetivo:</b></color> enemigos en la misma fila\n";
            cuerpo += $"<color={colorEncabezado}><b>Regla de uso:</b></color> {reglaCruceEs}\n";
            cuerpo += $"<color={colorEncabezado}><b>Tirada:</b></color> 1d20 + {atributo}{bonusTirada} + {bonusAtaquePorCasilla} por casilla avanzada vs Defensa\n";
            cuerpo += $"<color={colorEncabezado}><b>Pifia:</b></color> 5%   <color={colorEncabezado}><b>Crítico:</b></color> {criticoPorcentaje}%\n";
            cuerpo += $"<color={colorEncabezado}><b>Daño:</b></color> {rangoDanio} + {atributo} + {danioPorCasillaActual} por casilla avanzada. <color={colorEncabezado}><b>Tipo:</b></color> Perforante\n";
            cuerpo += $"<color={colorEncabezado}><b>Esforzable:</b></color> si ({esforzable})";
        }

        string titulo = esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs;
        string subtitulo = esIngles
            ? "Charges to the front tile and pierce the enemies in the row."
            : esPortugues
                ? "Avanca para a casa da frente e perfura os inimigos na linha."
                : "Carga hacia la casilla del frente y atraviesa los enemigos en la fila.";

        txtDescripcion = $"<size=115%><color={colorTitulo}><b>{titulo}</b></color></size><pos=74%><color=#c8c8c8>{costoSuperior}</color>\n\n";
        txtDescripcion += $"<color=#8f8f8f><i>{subtitulo}</i></color>\n\n";
        txtDescripcion += "<color=#3f4744><size=85%>--------------------------------</size></color>\n\n";
        txtDescripcion += cuerpo;

        bool mostrarProximoNivel = CampaignManager.Instance != null && CampaignManager.Instance.scMenuPersonajes != null && CampaignManager.Instance.scMenuPersonajes.pSel != null && CampaignManager.Instance.scMenuPersonajes.pSel.NivelPuntoHabilidad > 0;
        if (!mostrarProximoNivel) { return; }

        if (esIngles)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +3 damage.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: can cross allies.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 AP Cost) or Option B (+2 damage per tile advanced).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +3 de dano.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: pode atravessar aliados.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de AP) ou Opcao B (+2 de dano por casa avancada).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: +3 de daño.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: puede atravesar aliados.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Próximo Nivel: Opción A (-1 costo AP) u Opción B (+2 de daño por casilla avanzada).</color>"; }
        }
    }

    private string TextoModificadorDescripcion(int valor)
    {
        if (valor > 0) { return $" + {valor}"; }
        if (valor < 0) { return $" - {Mathf.Abs(valor)}"; }
        return "";
    }
    public bool PuedeActivarseDesdePosicionActual(out string motivo)
    {
        motivo = string.Empty;

        if (BattleManager.Instance == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return true;
        }

        if (!HayEnemigosEnFilaActual())
        {
            motivo = "No hay enemigos en la fila";
            return false;
        }

        if (!TryResolverMovimiento(out _, out _, out _, out motivo))
        {
            return false;
        }

        return true;
    }


    public override void Activar()
    {
        origen = scEstaUnidad.CasillaPosicion;
        if (!PuedeActivarseDesdePosicionActual(out string motivo))
        {
            CancelarSeleccion();
            _ = scEstaUnidad?.GenerarTextoFlotante(TRADU.i.Traducir(motivo), Color.gray, FloatingTextContext.Generic);
            return;
        }

        ObtenerObjetivos();
        if (lObjetivosPosibles.Count == 0)
        {
            CancelarSeleccion();
            _ = scEstaUnidad?.GenerarTextoFlotante(TRADU.i.Traducir("No hay enemigos en la fila"), Color.gray, FloatingTextContext.Generic);
            return;
        }

        BattleManager.Instance.SeleccionandoObjetivo = true;
        BattleManager.Instance.HabilidadActiva = this;
    }


    public override async Task Resolver(List<object> Objetivos, Casilla casillaOrigenTrampas = null)
    {
        BattleManager.Instance?.OcultarPanelDescripcionHabilidad(this, true);

        if (BattleManager.Instance == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            CancelarSeleccion();
            return;
        }

        if (!TryResolverMovimiento(out Casilla destinoDuelista, out Casilla destinoAliado, out Unidad aliadoADesplazar, out string motivo))
        {
            CancelarSeleccion();
            _ = scEstaUnidad.GenerarTextoFlotante(TRADU.i.Traducir(motivo), Color.gray, FloatingTextContext.Generic);
            return;
        }

        BattleManager.Instance.bOcupado = true;

        try
        {
            Casilla origenMovimiento = scEstaUnidad.CasillaPosicion;
            casillasAvanzadasUltimoUso = Mathf.Max(0, destinoDuelista != null ? destinoDuelista.posX - origenMovimiento.posX : 0);

            if (aliadoADesplazar != null && destinoAliado != null)
            {
                aliadoADesplazar.CasillaForzadoaMover = destinoAliado;
                aliadoADesplazar.CasillaDeseadaMov = null;
            }

            if (destinoDuelista != null && destinoDuelista != scEstaUnidad.CasillaPosicion)
            {
                scEstaUnidad.CasillaForzadoaMover = destinoDuelista;
                scEstaUnidad.CasillaDeseadaMov = null;
                scEstaUnidad.ForzarSiguienteMovimientoForzadoInmediato();
                await EsperarMovimientoForzadoAsync(scEstaUnidad, aliadoADesplazar);
            }

            List<object> objetivosFila = RecolectarObjetivosFilaActual();
            if (objetivosFila.Count == 0)
            {
                CancelarSeleccion();
                return;
            }

            bool esMeleeOriginal = esMelee;
            esMelee = false;
            try
            {
                await base.Resolver(objetivosFila, casillaOrigenTrampas);
            }
            finally
            {
                esMelee = esMeleeOriginal;
            }
        }
        finally
        {
            casillasAvanzadasUltimoUso = 0;
            if (BattleManager.Instance != null && !BattleManager.Instance.bOcupado)
            {
                BattleManager.Instance.bOcupado = false;
            }
        }
    }

    protected override Task EsperarPreImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        var pose = scEstaUnidad != null ? scEstaUnidad.GetComponent<UnidadPoseController>() : null;
        int ms = MeleeTimingUtility.CalcularPreImpactoMs(pose);
        return ms > 0 ? BattleManager.DelayCombateAsync(ms) : Task.CompletedTask;
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        var pose = scEstaUnidad != null ? scEstaUnidad.GetComponent<UnidadPoseController>() : null;
        int ms = MeleeTimingUtility.CalcularPostImpactoMs(pose);
        return ms > 0 ? BattleManager.DelayCombateAsync(ms) : Task.CompletedTask;
    }

     void VFXAplicar(GameObject objetivo)
    {
        VFXenObjetivo = Resources.Load<GameObject>("VFX/VFX_CargaDeEstoque");
        if (VFXenObjetivo == null)
        {
            return;
        }

        GameObject vfx = Instantiate(VFXenObjetivo, objetivo.transform.position, Quaternion.identity);
        vfx.transform.parent = objetivo.transform;

        Canvas canvasObjeto = vfx.GetComponentInChildren<Canvas>();
        RenderOrderHelper.OrdenarCanvasEncima(canvasObjeto, vfx.transform.parent, 5);
    }

    public override void AplicarEfectosHabilidad(object obj, int tirada, Casilla nada)
    {
        if (obj is not Unidad objetivo)
        {
            return;
        }

        float defensaObjetivo = objetivo.ObtenerdefensaActual();
        float criticoRango = scEstaUnidad.mod_CriticoRangoDado + criticoRangoHab;
        int bonusAtaqueTotal = bonusAtaqueBase + bonusAtaquePorCasilla * casillasAvanzadasUltimoUso;
        int resultadoTirada = TiradaAtaque(tirada, defensaObjetivo, scEstaUnidad.mod_CarAgilidad, bonusAtaqueTotal, criticoRango, objetivo, 0);

        if (resultadoTirada == -1)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, true);
            scEstaUnidad.EstablecerAPActualA(0);
        }
        else if (resultadoTirada == 0)
        {
            objetivo.FalloAtaqueRecibido(scEstaUnidad, true);
        }
        else
        {
            ClaseDuelista duelista = scEstaUnidad as ClaseDuelista;
            int bonusDanioPorcentajeDanza = duelista != null ? duelista.ObtenerBonusDanioPorcentajeDanzaDelEstoque(objetivo) : 0;
            float danio = TiradaDeDados.TirarDados(XdDanio, daniodX)
              + bonusDanioBase
              + bonusDanioNivel2
              + scEstaUnidad.mod_CarAgilidad
              + (bonusDanioPorCasilla + bonusDanioPorCasillaNivel5) * casillasAvanzadasUltimoUso;

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
        }

        objetivo.AplicarDebuffPorAtaquesreiterados(1);
    }

    private void ObtenerObjetivos()
    {
        lObjetivosPosibles.Clear();
        lCasillasafectadas.Clear();

        if (origen == null)
        {
            return;
        }

        List<Casilla> casillasFila = origen.ObtenerCasillasRango(6, 0);
        foreach (Casilla casilla in casillasFila)
        {
            lCasillasafectadas.Add(casilla);
            casilla.ActivarCapaColorRojo();

            if (casilla.Presente == null)
            {
                continue;
            }

            Unidad unidadObjetivo = casilla.Presente.GetComponent<Unidad>();
            if (unidadObjetivo != null)
            {
                lObjetivosPosibles.Add(unidadObjetivo);
            }
        }

        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva = new List<Unidad>(lObjetivosPosibles);
        BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
    }

    private List<object> RecolectarObjetivosFilaActual()
    {
        List<object> objetivos = new List<object>();
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return objetivos;
        }

        foreach (Casilla casilla in scEstaUnidad.CasillaPosicion.ObtenerCasillasRango(6, 0))
        {
            if (casilla.Presente == null)
            {
                continue;
            }

            Unidad unidadObjetivo = casilla.Presente.GetComponent<Unidad>();
            if (unidadObjetivo != null)
            {
                objetivos.Add(unidadObjetivo);
            }
        }

        return objetivos;
    }

    private bool HayEnemigosEnFilaActual()
    {
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return false;
        }

        foreach (Casilla casilla in scEstaUnidad.CasillaPosicion.ObtenerCasillasRango(6, 0))
        {
            if (casilla.Presente != null && casilla.Presente.GetComponent<Unidad>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryResolverMovimiento(out Casilla destinoDuelista, out Casilla destinoAliado, out Unidad aliadoADesplazar, out string motivo)
    {
        destinoDuelista = null;
        destinoAliado = null;
        aliadoADesplazar = null;
        motivo = string.Empty;

        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null || scEstaUnidad.CasillaPosicion.ladoGO == null)
        {
            motivo = "No puede lanzar esta habilidad";
            return false;
        }

        LadoManager ladoPropio = scEstaUnidad.CasillaPosicion.ladoGO.GetComponent<LadoManager>();
        if (ladoPropio == null)
        {
            motivo = "No puede lanzar esta habilidad";
            return false;
        }

        destinoDuelista = ladoPropio.ObtenerCasillaPorIndex(3, scEstaUnidad.CasillaPosicion.posY);
        if (destinoDuelista == null)
        {
            motivo = "No hay casilla frontal disponible";
            return false;
        }

        int origenX = scEstaUnidad.CasillaPosicion.posX;
        if (origenX >= 3)
        {
            destinoDuelista = scEstaUnidad.CasillaPosicion;
            return true;
        }

        for (int posX = origenX + 1; posX <= 3; posX++)
        {
            Casilla casillaPaso = ladoPropio.ObtenerCasillaPorIndex(posX, scEstaUnidad.CasillaPosicion.posY);
            if (casillaPaso == null)
            {
                motivo = "No hay trayecto valido";
                return false;
            }

            if (casillaPaso.Presente == null || casillaPaso.Presente == scEstaUnidad.gameObject)
            {
                continue;
            }

            Obstaculo obstaculo = casillaPaso.Presente.GetComponent<Obstaculo>();
            if (obstaculo != null)
            {
                motivo = "El trayecto esta bloqueado";
                return false;
            }

            Unidad unidadBloqueando = casillaPaso.Presente.GetComponent<Unidad>();
            if (unidadBloqueando == null || unidadBloqueando.CasillaPosicion == null || unidadBloqueando.CasillaPosicion.lado != scEstaUnidad.CasillaPosicion.lado)
            {
                motivo = "El trayecto esta bloqueado";
                return false;
            }

            bool esDestinoFinal = posX == 3;
            if (!esDestinoFinal)
            {
                if (NIVEL < 3)
                {
                    motivo = "El trayecto esta bloqueado";
                    return false;
                }

                continue;
            }

            aliadoADesplazar = unidadBloqueando;
            destinoAliado = ladoPropio.ObtenerCasillaPorIndex(2, scEstaUnidad.CasillaPosicion.posY);
            if (destinoAliado == null)
            {
                motivo = "No hay espacio para intercambiar";
                return false;
            }

            if (destinoAliado.Presente != null && destinoAliado.Presente != scEstaUnidad.gameObject)
            {
                motivo = "No hay espacio para intercambiar";
                return false;
            }
        }

        return true;
    }

    private async Task EsperarMovimientoForzadoAsync(Unidad duelista, Unidad aliado)
    {
        float timeout = Time.time + 2.5f;
        while (Time.time < timeout)
        {
            bool duelistaLista = duelista == null || (duelista.CasillaForzadoaMover == null && !duelista.movimientoEnCurso);
            bool aliadoListo = aliado == null || (aliado.CasillaForzadoaMover == null && !aliado.movimientoEnCurso);
            if (duelistaLista && aliadoListo)
            {
                break;
            }

            await Task.Yield();
        }
    }

    private void CancelarSeleccion()
    {
        if (BattleManager.Instance == null)
        {
            return;
        }

        BattleManager.Instance.bOcupado = false;
        BattleManager.Instance.HabilidadActiva = null;
        BattleManager.Instance.SeleccionandoObjetivo = false;
        BattleManager.Instance.lUnidadesPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.lObstaculosPosiblesHabilidadActiva.Clear();
        BattleManager.Instance.LimpiarCapasCasillas();
        if (BattleManager.Instance.scUIContadorAP != null)
        {
            BattleManager.Instance.scUIContadorAP.ResetearCirculos();
        }
        BattleManager.Instance.scUIBotonesHab?.DeseleccionarTodas();
    }
}
