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
        cooldownMax = 0; //2
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
        int danioPorCasillaActual = bonusDanioPorCasilla + bonusDanioPorCasillaNivel5;

        string tituloEs = "Carga de Estoque I";
        string tituloEn = "Estoc Charge I";
        string tituloPt = "Carga de Estoque I";
        if (NIVEL == 2) { tituloEs = "Carga de Estoque II"; tituloEn = "Estoc Charge II"; tituloPt = "Carga de Estoque II"; }
        if (NIVEL == 3) { tituloEs = "Carga de Estoque III"; tituloEn = "Estoc Charge III"; tituloPt = "Carga de Estoque III"; }
        if (NIVEL == 4) { tituloEs = "Carga de Estoque IV a"; tituloEn = "Estoc Charge IV a"; tituloPt = "Carga de Estoque IV a"; }
        if (NIVEL == 5) { tituloEs = "Carga de Estoque IV b"; tituloEn = "Estoc Charge IV b"; tituloPt = "Carga de Estoque IV b"; }

        string cuerpo = "";
        if (esIngles)
        {
            cuerpo += "<b>Type:</b> Melee (row)\n";
            cuerpo += "<b>Target:</b> Enemies on the same row\n";
            cuerpo += "<b>Effect:</b> First moves to the front column. If the front tile has an ally, swaps it to the tile behind.\n";
            cuerpo += NIVEL > 2
              ? "<b>Cast rule:</b> cannot cross obstacles. Can cross allies.\n"
              : "<b>Cast rule:</b> cannot cross obstacles or allies.\n";
            cuerpo += $"<b>Roll:</b> 1d20 + <color=#ea0606>Agility ({agilidadActual})</color> + Attack ({ataqueActual}) + {bonusAtaqueBase} + {bonusAtaquePorCasilla} per tile advanced vs Defense. Fumble: 1. Crit: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Damage:</b> 1d8 + {bonusDanioBase + bonusDanioNivel2} + <color=#ea0606>Agility ({agilidadActual})</color> + {danioPorCasillaActual} per tile advanced | <b>Type:</b> Piercing\n";
        }
        else if (esPortugues)
        {
            cuerpo += "<b>Tipo:</b> Corpo a corpo (linha)\n";
            cuerpo += "<b>Alvo:</b> Inimigos na mesma linha\n";
            cuerpo += "<b>Efeito:</b> Primeiro avanca ate a coluna frontal. Se a casa frontal tiver um aliado, troca com ele para a casa de tras.\n";
            cuerpo += NIVEL > 2
              ? "<b>Regra de conjuro:</b> nao pode atravessar obstaculos. Pode atravessar aliados.\n"
              : "<b>Regra de conjuro:</b> nao pode atravessar obstaculos nem aliados.\n";
            cuerpo += $"<b>Rolagem:</b> 1d20 + <color=#ea0606>Agilidade ({agilidadActual})</color> + Ataque ({ataqueActual}) + {bonusAtaqueBase} + {bonusAtaquePorCasilla} por casa avancada vs Defesa. Falha critica: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Dano:</b> 1d8 + {bonusDanioBase + bonusDanioNivel2} + <color=#ea0606>Agilidade ({agilidadActual})</color> + {danioPorCasillaActual} por casa avancada | <b>Tipo:</b> Perfurante\n";
        }
        else
        {
            cuerpo += "<b>Tipo:</b> Melee (fila)\n";
            cuerpo += "<b>Objetivo:</b> Enemigos en la misma fila\n";
            cuerpo += "<b>Efecto:</b> Primero avanza a la columna frontal. Si la casilla frontal tiene un aliado, lo intercambia hacia la casilla de atras.\n";
            cuerpo += NIVEL > 2
              ? "<b>Regla de casteo:</b> no puede atravesar obstaculos. Puede atravesar aliados.\n"
              : "<b>Regla de casteo:</b> no puede atravesar obstaculos ni aliados.\n";
            cuerpo += $"<b>Tirada:</b> 1d20 + <color=#ea0606>Agilidad ({agilidadActual})</color> + Ataque ({ataqueActual}) + {bonusAtaqueBase} + {bonusAtaquePorCasilla} por casilla avanzada vs Defensa. Pifia: 1. Critico: {criticoBaseMin}-20\n";
            cuerpo += $"<b>Danio:</b> 1d8 + {bonusDanioBase + bonusDanioNivel2} + <color=#ea0606>Agilidad ({agilidadActual})</color> + {danioPorCasillaActual} por casilla avanzada | <b>Tipo:</b> Perforante\n";
        }

        string costos = esIngles
          ? $"- Cooldown: {cooldownMax}\n- AP Cost: {costoAP}\n- Valour Cost: {costoPM}\n- Effortable: Yes ({esforzable})"
          : esPortugues
            ? $"- Recarga: {cooldownMax}\n- Custo AP: {costoAP}\n- Custo Valentia: {costoPM}\n- Esforcavel: Sim ({esforzable})"
            : $"- Enfriamiento: {cooldownMax}\n- Costo AP: {costoAP}\n- Costo Valentia: {costoPM}\n- Esforzable: Si ({esforzable})";

        txtDescripcion = ConstruirDescripcionEstandar(
          esIngles ? tituloEn : esPortugues ? tituloPt : tituloEs,
          esIngles
            ? "The Duelist surges to the front and skewers the entire enemy row with a powerful strike."
            : esPortugues
              ? "A Duelista avanca ate a frente e atravessa a linha inimiga inteira com um ataque poderoso."
              : "La Duelista se lanza al frente y atraviesa toda la fila enemiga con un ataque poderoso.",
          cuerpo,
          costos,
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
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: +3 damage.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: can cross allies.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Next Level: Option A (-1 AP Cost) or Option B (+2 damage per tile advanced).</color>"; }
        }
        else if (esPortugues)
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +3 de dano.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: pode atravessar aliados.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcao A (-1 custo de AP) ou Opcao B (+2 de dano por casa avancada).</color>"; }
        }
        else
        {
            if (NIVEL < 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: +3 de danio.</color>"; }
            else if (NIVEL == 2) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: puede atravesar aliados.</color>"; }
            else if (NIVEL == 3) { txtDescripcion += "\n\n<color=#dfea02>- Proximo Nivel: Opcion A (-1 costo de AP) u Opcion B (+2 de danio por casilla avanzada).</color>"; }
        }
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
        float delay = 0.5f;
        var pose = scEstaUnidad != null ? scEstaUnidad.GetComponent<UnidadPoseController>() : null;
        if (pose != null)
        {
            delay = pose.duracionPoseAtacar;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay * 0.5f) * 1000f);
        return BattleManager.DelayCombateAsync(ms);
    }

    protected override Task EsperarPostImpactoAsync(List<object> objetivos, Casilla casillaOrigenTrampas)
    {
        return Task.CompletedTask;
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
