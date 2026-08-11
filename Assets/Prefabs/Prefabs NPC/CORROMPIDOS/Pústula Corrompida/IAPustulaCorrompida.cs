using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class IAPustulaCorrompida : IAHabilidad
{
    private const int ColumnaFrontal = 3;
    private const int DificultadReflejos = 13;
    private const int DificultadFortaleza = 11;
    private const float DuracionEnfermoCampaniaHoras = 72f;
    private const int DadosDanio = 4;
    private const int CarasDanio = 10;
    private const int DanioAcidoBase = 5;
    private const int TipoDanioAcido = 7;

    private bool llegoAlFrente;
    private bool explosionHabilitada;
    private BattleManager battleManagerSuscrito;
    private Color colorImagenOriginal;
    private bool colorImagenOriginalCapturado;
    private float fasePulso;
    private Casilla ultimaCasillaRegistrada;
    private bool explosionVoluntariaEnCurso;
    private bool explosionPostumaEnCurso;

    private void Awake()
    {
        nombre = "Explosión Ácida";
        Usuario = gameObject;
        scEstaUnidad = GetComponent<Unidad>();
        IAUnidad iaUnidad = GetComponent<IAUnidad>();
        if (iaUnidad != null)
        {
            iaUnidad.siempreEnRetaguardiaInicial = true;
        }
        if (GetComponent<ReaccionMuertePustulaCorrompida>() == null)
        {
            gameObject.AddComponent<ReaccionMuertePustulaCorrompida>();
        }
        hAncho = 1;
        hAlcance = 6;
        hCooldownMax = 0;
        hActualCooldown = 0;
        esMelee = false;
        esHostil = false;
        ignoraProvocacion = true;
        afectaObstaculos = true;
        fuerzaPoseAtaque = true;
        prioridad = 10;
        costoAP = 3;
        fasePulso = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        IntentarSuscribirAlCombate();
    }

    private void Update()
    {
        IntentarSuscribirAlCombate();
        DetectarMovimientoExterno();
        DetectarLlegadaAlFrente();
        ActualizarPulsoRojo();
    }

    private void OnDisable()
    {
        DesuscribirDelCombate();
        RestaurarColorImagen();
    }

    private void IntentarSuscribirAlCombate()
    {
        BattleManager actual = BattleManager.Instance;
        if (actual == battleManagerSuscrito)
        {
            return;
        }

        DesuscribirDelCombate();
        battleManagerSuscrito = actual;
        if (battleManagerSuscrito != null)
        {
            battleManagerSuscrito.OnTurnoNuevo += BattleManager_OnTurnoNuevo;
        }
    }

    private void DesuscribirDelCombate()
    {
        if (battleManagerSuscrito != null)
        {
            battleManagerSuscrito.OnTurnoNuevo -= BattleManager_OnTurnoNuevo;
            battleManagerSuscrito = null;
        }
    }

    private void DetectarLlegadaAlFrente()
    {
        if (!llegoAlFrente && scEstaUnidad != null && scEstaUnidad.CasillaPosicion != null
            && scEstaUnidad.CasillaPosicion.posX == ColumnaFrontal)
        {
            llegoAlFrente = true;
        }
    }

    private void BattleManager_OnTurnoNuevo(object sender, EventArgs args)
    {
        DetectarLlegadaAlFrente();
        if (battleManagerSuscrito != null && battleManagerSuscrito.unidadActiva == scEstaUnidad
            && llegoAlFrente && scEstaUnidad.CasillaPosicion != null
            && scEstaUnidad.CasillaPosicion.posX == ColumnaFrontal)
        {
            explosionHabilitada = true;
        }
    }

    public override List<object> ListaHayObjetivosAlAlcance()
    {
        objPosibles.Clear();
        if (scEstaUnidad == null)
        {
            scEstaUnidad = GetComponent<Unidad>();
        }

        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return objPosibles;
        }

        DetectarLlegadaAlFrente();
        if (scEstaUnidad.CasillaPosicion.posX < ColumnaFrontal)
        {
            esHostil = false;
            if (BuscarSiguienteCasillaHaciaElFrente() != null)
            {
                objPosibles.Add(scEstaUnidad);
            }
            return objPosibles;
        }

        if (!explosionHabilitada)
        {
            esHostil = false;
            objPosibles.Add(scEstaUnidad);
            return objPosibles;
        }

        esHostil = true;
        List<Unidad> enemigos = ObtenerEnemigosEnColumnaOcupadaMasCercana();
        objPosibles.AddRange(enemigos.Cast<object>());
        return objPosibles;
    }

    public override object EstablecerObjetivoPrioritario()
    {
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return null;
        }

        return objPosibles
            .OfType<Unidad>()
            .Where(unidad => unidad != null && unidad.HP_actual > 0 && unidad.CasillaPosicion != null)
            .OrderBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - scEstaUnidad.CasillaPosicion.posY))
            .FirstOrDefault();
    }

    public override async Task ActivarHabilidad()
    {
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return;
        }

        if (scEstaUnidad.CasillaPosicion.posX < ColumnaFrontal)
        {
            await AvanzarUnPaso();
            return;
        }

        if (!explosionHabilitada)
        {
            scEstaUnidad.EstablecerAPActualA(0);
            return;
        }

        if (scEstaUnidad.ObtenerAPActual() < costoAP)
        {
            scEstaUnidad.EstablecerAPActualA(0);
            return;
        }

        Casilla centroExplosion = ObtenerCasillaCentroExplosion();
        if (centroExplosion == null)
        {
            scEstaUnidad.EstablecerAPActualA(0);
            return;
        }

        List<Casilla> casillasAfectadas = ObtenerCasillasAfectadas(centroExplosion);
        List<Casilla> casillasDanio = ObtenerCasillasDanio(centroExplosion);
        List<Unidad> enemigosAfectados = casillasDanio
            .Where(casilla => casilla != null && casilla.Presente != null)
            .Select(casilla => casilla.Presente.GetComponent<Unidad>())
            .Where(unidad => unidad != null && unidad.HP_actual > 0)
            .Distinct()
            .ToList();
        List<Obstaculo> obstaculosAfectados = casillasDanio
            .Where(casilla => casilla != null && casilla.Presente != null)
            .Select(casilla => casilla.Presente.GetComponent<Obstaculo>())
            .Where(obstaculo => obstaculo != null && obstaculo.hpCurr > 0)
            .Distinct()
            .ToList();
        List<Unidad> aliadosAdyacentes = ObtenerAliadosAdyacentes();
        List<Unidad> unidadesAfectadas = enemigosAfectados
            .Concat(aliadosAdyacentes)
            .Distinct()
            .ToList();

        scEstaUnidad.CambiarAPActual(-costoAP);
        Unidad objetivoVisual = unidadesAfectadas
            .OrderBy(unidad => Mathf.Abs(unidad.CasillaPosicion.posY - scEstaUnidad.CasillaPosicion.posY))
            .FirstOrDefault();
        PrepararInicioAnimacion(unidadesAfectadas.Cast<object>().ToList(), objetivoVisual);
        MeleeApproachMover aproximacion = MeleeApproachMover.ObtenerOCrear(scEstaUnidad);
        if (aproximacion != null)
        {
            bool seAproximo = await aproximacion.PrepararAproximacionIAAsync(true, 0, centroExplosion, true);
            if (seAproximo)
            {
                aproximacion.ConfirmarPosicionActual();
            }
        }
        explosionVoluntariaEnCurso = true;
        await BattleManager.DelayCombateAsync(450);
        PustulaExplosionVFX.Crear(scEstaUnidad);

        foreach (Unidad unidad in enemigosAfectados)
        {
            if (unidad != null && unidad.HP_actual > 0)
            {
                AplicarEfectosHabilidad(unidad);
            }
        }

        foreach (Obstaculo obstaculo in obstaculosAfectados)
        {
            if (obstaculo != null && obstaculo.hpCurr > 0)
            {
                AplicarEfectosHabilidad(obstaculo);
            }
        }

        foreach (Unidad aliado in aliadosAdyacentes)
        {
            if (aliado != null && aliado.HP_actual > 0)
            {
                AplicarDanioAliado(aliado);
            }
        }

        await BattleManager.DelayCombateAsync(450);
        CrearMasaContaminada(casillasAfectadas);
        await BattleManager.DelayCombateAsync(150);

        if (scEstaUnidad != null && scEstaUnidad.HP_actual > 0)
        {
            scEstaUnidad.HP_actual = 0;
            scEstaUnidad.UnidadMuere();
        }
    }

    public override void AplicarEfectosHabilidad(object obj)
    {
        if (obj is Obstaculo obstaculo)
        {
            float danioObstaculo = TiradaDeDados.TirarDados(DadosDanio, CarasDanio) + DanioAcidoBase;
            danioObstaculo = danioObstaculo / 100f * (100f + scEstaUnidad.mod_DanioPorcentaje);
            obstaculo.RecibirDanio(danioObstaculo, TipoDanioAcido, false, scEstaUnidad);
            return;
        }

        Unidad objetivo = obj as Unidad;
        if (objetivo == null)
        {
            return;
        }

        float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDanio) + DanioAcidoBase;
        danio = danio / 100f * (100f + scEstaUnidad.mod_DanioPorcentaje);

        bool fallaReflejos = objetivo.TiradaSalvacion(2, DificultadReflejos);
        if (!fallaReflejos)
        {
            danio *= 0.5f;
        }

        objetivo.RecibirDanioSinFlashPantalla(danio, TipoDanioAcido, false, scEstaUnidad);

        bool fallaFortaleza = objetivo.TiradaSalvacion(1, DificultadFortaleza);
        if (fallaFortaleza)
        {
            AplicarEnfermo(objetivo);
        }
    }

    private void AplicarEnfermo(Unidad objetivo)
    {
        if (objetivo == null)
        {
            return;
        }

        if (!objetivo.TieneBuffNombre("Enfermo"))
        {
            Buff enfermo = new Buff();
            enfermo.buffNombre = "Enfermo";
            enfermo.boolfDebufftBuff = false;
            enfermo.DuracionBuffRondas = -1;
            enfermo.esStackeable = false;
            enfermo.cantTsFortaleza -= 3;
            enfermo.cantDanioPorcentaje -= 15;
            enfermo.cantAPMax -= 1;
            enfermo.AplicarBuff(objetivo, scEstaUnidad);
            ComponentCopier.CopyComponent(enfermo, objetivo.gameObject);
        }

        CampaignManager campaignManager = CampaignManager.Instance;
        AdministradorEscenas administrador = campaignManager != null
            ? campaignManager.scAdministradorEscenas
            : null;
        Personaje personaje = administrador != null
            ? administrador.ObtenerPersonajeDesdeUnidad(objetivo)
            : null;
        if (personaje != null)
        {
            personaje.AplicarEnfermoHoras(DuracionEnfermoCampaniaHoras);
        }
    }

    private void AplicarDanioAliado(Unidad aliado)
    {
        float danio = TiradaDeDados.TirarDados(DadosDanio, CarasDanio) + DanioAcidoBase;
        danio = danio / 100f * (100f + scEstaUnidad.mod_DanioPorcentaje);
        aliado.RecibirDanioSinFlashPantalla(danio * 0.5f, TipoDanioAcido, false, scEstaUnidad);
    }

    public async Task ExplotarAlMorirAsync()
    {
        if (explosionVoluntariaEnCurso || explosionPostumaEnCurso || scEstaUnidad == null)
        {
            return;
        }

        explosionPostumaEnCurso = true;
        Unidad atacante = scEstaUnidad.ultimaUnidadQueLeHizoDanio;
        MeleeApproachMover aproximacionAtacante = atacante != null
            ? atacante.GetComponent<MeleeApproachMover>()
            : null;
        bool atacanteUsoMelee = AtacanteUsoHabilidadMelee(atacante, aproximacionAtacante);
        bool bloquearRetorno = atacanteUsoMelee && aproximacionAtacante != null
            && aproximacionAtacante.TieneAproximacionActiva();
        List<Unidad> aliadosAdyacentes = ObtenerAliadosAdyacentes();
        if (atacanteUsoMelee)
        {
            aliadosAdyacentes.Remove(atacante);
        }

        if (bloquearRetorno)
        {
            aproximacionAtacante.BloquearRetornoTemporal();
        }

        try
        {
            await BattleManager.DelayCombateAsync(280);
            PustulaExplosionVFX.Crear(scEstaUnidad);

            foreach (Unidad aliado in aliadosAdyacentes)
            {
                if (aliado != null && aliado.HP_actual > 0)
                {
                    AplicarDanioAliado(aliado);
                }
            }

            if (atacanteUsoMelee && atacante != null && atacante.HP_actual > 0)
            {
                AplicarEfectosHabilidad(atacante);
            }

            await BattleManager.DelayCombateAsync(480);
        }
        finally
        {
            if (bloquearRetorno && aproximacionAtacante != null)
            {
                if (atacante == null || atacante.HP_actual <= 0)
                {
                    aproximacionAtacante.ConfirmarPosicionActual();
                }
                aproximacionAtacante.LiberarRetornoTemporal();
            }
        }
    }

    private static bool AtacanteUsoHabilidadMelee(Unidad atacante, MeleeApproachMover aproximacion)
    {
        if (atacante == null || atacante.HP_actual <= 0)
        {
            return false;
        }

        if (aproximacion != null && aproximacion.TieneAproximacionActiva())
        {
            return true;
        }

        BattleManager battleManager = BattleManager.Instance;
        return battleManager != null && battleManager.unidadActiva == atacante
            && battleManager.HabilidadActiva != null && battleManager.HabilidadActiva.esMelee;
    }

    private async Task AvanzarUnPaso()
    {
        IAUnidad iaUnidad = GetComponent<IAUnidad>();
        Casilla origen = scEstaUnidad.CasillaPosicion;
        Casilla destino = BuscarSiguienteCasillaHaciaElFrente();
        bool seMovio = iaUnidad != null && destino != null && await iaUnidad.MoverACasilla(destino);
        if (seMovio)
        {
            CrearMasaEnCasillaAbandonada(origen);
            ultimaCasillaRegistrada = scEstaUnidad.CasillaPosicion;
        }
        else if (scEstaUnidad != null)
        {
            scEstaUnidad.EstablecerAPActualA(0);
        }
    }

    private Casilla BuscarSiguienteCasillaHaciaElFrente()
    {
        Casilla origen = scEstaUnidad.CasillaPosicion;
        LadoManager lado = origen.ladoGO != null ? origen.ladoGO.GetComponent<LadoManager>() : null;
        if (lado == null || origen.posX >= ColumnaFrontal)
        {
            return null;
        }

        List<Unidad> enemigos = ObtenerEnemigosVivos();
        List<Casilla> laterales = ObtenerLateralesLibres(lado, origen);
        Casilla frontal = lado.ObtenerCasillaPorIndex(origen.posX + 1, origen.posY);
        bool hayObjetivoEnCorredor = HayBloqueoEnCorredorActual();

        if (hayObjetivoEnCorredor && frontal != null && frontal.Presente == null)
        {
            return frontal;
        }

        if (!hayObjetivoEnCorredor && enemigos.Count > 0)
        {
            int distanciaActual = enemigos.Min(enemigo =>
                Mathf.Abs(enemigo.CasillaPosicion.posY - origen.posY));
            Casilla acercamientoLateral = laterales
                .Select(casilla => new
                {
                    Casilla = casilla,
                    Distancia = enemigos.Min(enemigo =>
                        Mathf.Abs(enemigo.CasillaPosicion.posY - casilla.posY))
                })
                .Where(opcion => opcion.Distancia < distanciaActual)
                .OrderBy(opcion => opcion.Distancia)
                .ThenBy(opcion => Mathf.Abs(opcion.Casilla.posY - 3))
                .Select(opcion => opcion.Casilla)
                .FirstOrDefault();

            if (acercamientoLateral != null)
            {
                return acercamientoLateral;
            }
        }

        if (frontal != null && frontal.Presente == null)
        {
            return frontal;
        }

        return laterales
            .OrderByDescending(casilla =>
            {
                Casilla siguiente = lado.ObtenerCasillaPorIndex(casilla.posX + 1, casilla.posY);
                return siguiente != null && siguiente.Presente == null;
            })
            .ThenBy(casilla => Mathf.Abs(casilla.posY - 3))
            .FirstOrDefault();
    }

    private static List<Casilla> ObtenerLateralesLibres(LadoManager lado, Casilla origen)
    {
        List<Casilla> laterales = new List<Casilla>();
        AgregarSiLibre(laterales, lado.ObtenerCasillaPorIndex(origen.posX, origen.posY - 1));
        AgregarSiLibre(laterales, lado.ObtenerCasillaPorIndex(origen.posX, origen.posY + 1));
        return laterales;
    }

    private static void AgregarSiLibre(List<Casilla> casillas, Casilla candidata)
    {
        if (candidata != null && candidata.Presente == null)
        {
            casillas.Add(candidata);
        }
    }

    private List<Unidad> ObtenerEnemigosEnColumnaOcupadaMasCercana()
    {
        List<Unidad> enemigos = ObtenerEnemigosVivos();

        if (enemigos.Count == 0)
        {
            return enemigos;
        }

        int columnaMasCercana = enemigos.Max(unidad => unidad.CasillaPosicion.posX);
        return enemigos.Where(unidad => unidad.CasillaPosicion.posX == columnaMasCercana).ToList();
    }

    private List<Unidad> ObtenerEnemigosVivos()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.lUnidadesTotal == null
            || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return new List<Unidad>();
        }

        return BattleManager.Instance.lUnidadesTotal
            .Where(unidad => unidad != null && unidad.HP_actual > 0 && unidad.CasillaPosicion != null
                && unidad.CasillaPosicion.lado != scEstaUnidad.CasillaPosicion.lado)
            .ToList();
    }

    private bool HayBloqueoEnCorredorActual()
    {
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null
            || scEstaUnidad.CasillaPosicion.ladoOpuesto == null)
        {
            return false;
        }

        LadoManager ladoEnemigo = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();
        if (ladoEnemigo == null)
        {
            return false;
        }

        int filaOrigen = scEstaUnidad.CasillaPosicion.posY;
        for (int columna = ColumnaFrontal; columna >= 1; columna--)
        {
            Casilla centro = ladoEnemigo.ObtenerCasillaPorIndex(columna, filaOrigen);
            if (centro != null && ObtenerCasillasAfectadas(centro)
                .Any(casilla => BloqueaSeleccionExplosion(casilla, filaOrigen)))
            {
                return true;
            }
        }

        return false;
    }

    private Casilla ObtenerCasillaCentroExplosion()
    {
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null
            || scEstaUnidad.CasillaPosicion.ladoOpuesto == null)
        {
            return null;
        }

        LadoManager ladoEnemigo = scEstaUnidad.CasillaPosicion.ladoOpuesto.GetComponent<LadoManager>();
        if (ladoEnemigo == null)
        {
            return null;
        }

        int filaOrigen = scEstaUnidad.CasillaPosicion.posY;
        for (int columna = ColumnaFrontal; columna >= 1; columna--)
        {
            Casilla centro = ladoEnemigo.ObtenerCasillaPorIndex(columna, filaOrigen);
            if (centro == null)
            {
                continue;
            }

            bool hayBloqueoEnFrente = ObtenerCasillasAfectadas(centro)
                .Any(casilla => BloqueaSeleccionExplosion(casilla, filaOrigen));
            if (hayBloqueoEnFrente)
            {
                return centro;
            }
        }

        return null;
    }

    private static List<Casilla> ObtenerCasillasAfectadas(Casilla centro)
    {
        List<Casilla> casillas = new List<Casilla> { centro };
        casillas.AddRange(centro.ObtenerCasillasAdyacentesEnColumna());
        return casillas.Where(casilla => casilla != null).Distinct().ToList();
    }

    private bool BloqueaSeleccionExplosion(Casilla casilla, int filaOrigen)
    {
        if (casilla == null || casilla.Presente == null)
        {
            return false;
        }

        Unidad unidad = casilla.Presente.GetComponent<Unidad>();
        if (unidad != null)
        {
            return unidad.HP_actual > 0 && unidad.CasillaPosicion != null
                && unidad.CasillaPosicion.lado != scEstaUnidad.CasillaPosicion.lado;
        }

        return casilla.posY == filaOrigen && casilla.Presente.GetComponent<Obstaculo>() != null;
    }

    private static List<Casilla> ObtenerCasillasDanio(Casilla centro)
    {
        List<Casilla> casillas = new List<Casilla>();
        LadoManager lado = centro != null && centro.ladoGO != null
            ? centro.ladoGO.GetComponent<LadoManager>()
            : null;
        if (lado == null)
        {
            return casillas;
        }

        int ultimaColumna = Mathf.Max(1, centro.posX - 1);
        for (int columna = centro.posX; columna >= ultimaColumna; columna--)
        {
            Casilla centroColumna = lado.ObtenerCasillaPorIndex(columna, centro.posY);
            if (centroColumna != null)
            {
                casillas.AddRange(ObtenerCasillasAfectadas(centroColumna));
            }
        }

        return casillas.Distinct().ToList();
    }

    private List<Unidad> ObtenerAliadosAdyacentes()
    {
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return new List<Unidad>();
        }

        int ladoPropio = scEstaUnidad.CasillaPosicion.lado;
        return scEstaUnidad.CasillaPosicion.ObtenerCasillasAlrededor(1)
            .Where(casilla => casilla != null && casilla.Presente != null)
            .Select(casilla => casilla.Presente.GetComponent<Unidad>())
            .Where(unidad => unidad != null && unidad != scEstaUnidad && unidad.HP_actual > 0
                && unidad.CasillaPosicion != null && unidad.CasillaPosicion.lado == ladoPropio)
            .Distinct()
            .ToList();
    }

    private void CrearMasaContaminada(IEnumerable<Casilla> casillas)
    {
        foreach (Casilla casilla in casillas)
        {
            if (casilla == null || casilla.Presente != null || casilla.GetComponent<Trampa>() != null)
            {
                continue;
            }

            TrampaMasaContaminada trampa = casilla.gameObject.AddComponent<TrampaMasaContaminada>();
            trampa.Inicializar();
            trampa.AsignarCreador(scEstaUnidad);
        }
    }

    private void CrearMasaEnCasillaAbandonada(Casilla origen)
    {
        if (origen == null || scEstaUnidad == null || scEstaUnidad.CasillaPosicion == origen)
        {
            return;
        }

        if (origen.Presente == scEstaUnidad.gameObject)
        {
            origen.Presente = null;
        }

        CrearMasaContaminada(new[] { origen });
    }

    private void DetectarMovimientoExterno()
    {
        if (scEstaUnidad == null || scEstaUnidad.CasillaPosicion == null)
        {
            return;
        }

        if (ultimaCasillaRegistrada == null)
        {
            ultimaCasillaRegistrada = scEstaUnidad.CasillaPosicion;
            return;
        }

        if (ultimaCasillaRegistrada != scEstaUnidad.CasillaPosicion)
        {
            Casilla casillaAbandonada = ultimaCasillaRegistrada;
            ultimaCasillaRegistrada = scEstaUnidad.CasillaPosicion;
            CrearMasaEnCasillaAbandonada(casillaAbandonada);
        }
    }

    private void ActualizarPulsoRojo()
    {
        if (scEstaUnidad == null || scEstaUnidad.uImage == null)
        {
            return;
        }

        if (!colorImagenOriginalCapturado)
        {
            colorImagenOriginal = scEstaUnidad.ObtenerColorBaseImagenUnidad();
            colorImagenOriginalCapturado = true;
        }

        int columna = scEstaUnidad.CasillaPosicion != null
            ? Mathf.Clamp(scEstaUnidad.CasillaPosicion.posX, 1, ColumnaFrontal)
            : 1;
        float cercania = Mathf.InverseLerp(1f, ColumnaFrontal, columna);
        float intensidadBase = Mathf.Lerp(0.04f, 0.12f, cercania);
        float amplitud = Mathf.Lerp(0.04f, 0.11f, cercania);
        float velocidad = Mathf.Lerp(2.2f, 3.4f, cercania);
        float onda = 0.5f + 0.5f * Mathf.Sin(Time.time * velocidad + fasePulso);
        float intensidad = intensidadBase + amplitud * onda;

        Color rojoSuave = new Color(1f, 0.2f, 0.2f, colorImagenOriginal.a);
        Color colorPulso = Color.Lerp(colorImagenOriginal, rojoSuave, intensidad);
        colorPulso.a = colorImagenOriginal.a;
        scEstaUnidad.EstablecerColorBaseImagenUnidad(colorPulso);
    }

    private void RestaurarColorImagen()
    {
        if (colorImagenOriginalCapturado && scEstaUnidad != null && scEstaUnidad.uImage != null)
        {
            scEstaUnidad.EstablecerColorBaseImagenUnidad(colorImagenOriginal);
        }
    }
}

public sealed class ReaccionMuertePustulaCorrompida : Reaccion
{
    private void Awake()
    {
        TipoTrigger = 3;
        usos = 1;
        permanente = true;
        nombre = "Explosión póstuma";
        descripcion = "Reacción: Al morir explota infligiendo daño a unidades adyacentes y a su atacante (si fue cuerpo a cuerpo).";
        if (TRADU.i != null && TRADU.i.nIdioma == 2)
        {
            descripcion = "Reaction: Upon dying, it explodes, dealing damage to adjacent units and its attacker (if the attack was melee).";
        }
        else if (TRADU.i != null && TRADU.i.nIdioma == 3)
        {
            descripcion = "Reação: Ao morrer, explode, causando dano às unidades adjacentes e ao atacante (se o ataque foi corpo a corpo).";
        }
        scEstaUnidad = GetComponent<Unidad>();
    }

    public async override void AplicarEfectos(
        Unidad uTriggerer,
        bool melee,
        float variableFlexible1 = 0,
        float variableFlexible2 = 0)
    {
        if (usos <= 0)
        {
            return;
        }

        usos--;
        IAPustulaCorrompida pustula = GetComponent<IAPustulaCorrompida>();
        if (pustula != null)
        {
            await pustula.ExplotarAlMorirAsync();
        }
    }
}

public sealed class PustulaExplosionVFX : MonoBehaviour
{
    private const float Duracion = 1.15f;
    private const int SegmentosAnillo = 52;

    private static readonly Color[] ColoresGotas =
    {
        new Color(0.54f, 0.95f, 0.08f, 0.95f),
        new Color(0.78f, 0.9f, 0.12f, 0.92f),
        new Color(0.28f, 0.68f, 0.06f, 0.94f),
        new Color(0.62f, 0.55f, 0.04f, 0.9f)
    };

    private static readonly Color[] ColoresNube =
    {
        new Color(0.7f, 1f, 0.25f, 0.42f),
        new Color(0.42f, 0.76f, 0.08f, 0.38f),
        new Color(0.72f, 0.68f, 0.1f, 0.34f)
    };

    private static Material materialParticulas;
    private static Material materialAnillo;
    private static Texture2D texturaGotaSuave;
    private static Sprite spriteGotaSuave;

    private SpriteRenderer nucleo;
    private SpriteRenderer halo;
    private LineRenderer anillo;
    private float tiempo;
    private float faseIrregularidad;

    public static void Crear(Unidad origen)
    {
        if (origen == null)
        {
            return;
        }

        Vector3 posicion = origen.puntoEntrante != null
            ? origen.puntoEntrante.position
            : origen.transform.position + Vector3.up * 0.34f;

        GameObject root = new GameObject("VFX_ExplosionPustulaCorrompida");
        root.transform.position = posicion;
        if (BattleManager.Instance != null)
        {
            root.transform.SetParent(BattleManager.Instance.transform, true);
        }

        PustulaExplosionVFX fx = root.AddComponent<PustulaExplosionVFX>();
        fx.Inicializar(origen);
    }

    private void Inicializar(Unidad origen)
    {
        ObtenerOrden(origen, out int sortingLayerId, out int sortingOrder);
        faseIrregularidad = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        ParticleSystem nube = CrearSistemaParticulas(
            "NubeDePus", 0.5f, 0.95f, 0.12f, 0.28f, -0.03f, sortingLayerId, sortingOrder);
        ParticleSystem gotas = CrearSistemaParticulas(
            "GotasDePus", 0.42f, 0.82f, 0.035f, 0.095f, 0.62f, sortingLayerId, sortingOrder + 3);
        ParticleSystem microgotas = CrearSistemaParticulas(
            "Microgotas", 0.24f, 0.54f, 0.014f, 0.045f, 0.3f, sortingLayerId, sortingOrder + 4);

        EmitirRadial(nube, 20, 0.12f, 0.48f, ColoresNube, 0.72f, 0.32f);
        EmitirRadial(gotas, 38, 0.58f, 1.42f, ColoresGotas, 0.82f, 0.2f);
        EmitirRadial(microgotas, 54, 1.05f, 2.15f, ColoresGotas, 0.9f, 0.32f);
    }

    private void Update()
    {
        tiempo += Time.deltaTime;
        float t = Mathf.Clamp01(tiempo / Duracion);
        float golpe = 1f - Mathf.SmoothStep(0f, 1f, t);
        float salidaHalo = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.08f) / 0.92f));

        if (nucleo != null)
        {
            float escalaNucleo = Mathf.Lerp(0.16f, 0.74f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.42f)));
            nucleo.transform.localScale = Vector3.one * escalaNucleo;
            Color color = Color.Lerp(
                new Color(0.92f, 1f, 0.28f, 0.86f),
                new Color(0.3f, 0.62f, 0.03f, 0f),
                t);
            nucleo.color = color;
        }

        if (halo != null)
        {
            float pulso = 0.94f + Mathf.Sin(tiempo * 17f) * 0.06f;
            halo.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.28f, Mathf.SmoothStep(0f, 1f, t)) * pulso;
            halo.color = new Color(0.5f, 0.82f, 0.06f, 0.3f * salidaHalo);
        }

        ActualizarAnillo(t, golpe);
        if (tiempo >= Duracion)
        {
            Destroy(gameObject);
        }
    }

    private SpriteRenderer CrearMancha(string nombre, Color color, int sortingLayerId, int sortingOrder)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(transform, false);
        if (Camera.main != null)
        {
            go.transform.rotation = Camera.main.transform.rotation;
        }

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = ObtenerSpriteGotaSuave();
        renderer.color = color;
        renderer.sortingLayerID = sortingLayerId;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private LineRenderer CrearAnillo(int sortingLayerId, int sortingOrder)
    {
        GameObject go = new GameObject("OndaDePus");
        go.transform.SetParent(transform, false);
        if (Camera.main != null)
        {
            go.transform.rotation = Camera.main.transform.rotation;
        }

        LineRenderer linea = go.AddComponent<LineRenderer>();
        linea.useWorldSpace = false;
        linea.loop = true;
        linea.positionCount = SegmentosAnillo;
        linea.sharedMaterial = ObtenerMaterialAnillo();
        linea.sortingLayerID = sortingLayerId;
        linea.sortingOrder = sortingOrder;
        linea.numCornerVertices = 3;
        linea.numCapVertices = 3;
        return linea;
    }

    private void ActualizarAnillo(float t, float intensidad)
    {
        if (anillo == null)
        {
            return;
        }

        float radio = Mathf.Lerp(0.08f, 1.05f, Mathf.SmoothStep(0f, 1f, t));
        for (int i = 0; i < SegmentosAnillo; i++)
        {
            float angulo = i / (float)SegmentosAnillo * Mathf.PI * 2f;
            float irregularidad = 1f + Mathf.Sin(angulo * 7f + faseIrregularidad) * 0.035f;
            anillo.SetPosition(i, new Vector3(
                Mathf.Cos(angulo) * radio * irregularidad,
                Mathf.Sin(angulo) * radio * irregularidad,
                0f));
        }

        Color color = Color.Lerp(
            new Color(0.9f, 1f, 0.22f, 0.82f),
            new Color(0.34f, 0.62f, 0.03f, 0f),
            t);
        color.a *= intensidad;
        anillo.startColor = color;
        anillo.endColor = color;
        anillo.startWidth = Mathf.Lerp(0.075f, 0.008f, t);
        anillo.endWidth = anillo.startWidth;
    }

    private static ParticleSystem CrearSistemaParticulas(
        string nombre,
        float vidaMin,
        float vidaMax,
        float tamanoMin,
        float tamanoMax,
        float gravedad,
        int sortingLayerId,
        int sortingOrder)
    {
        GameObject go = new GameObject(nombre);
        ParticleSystem particulas = go.AddComponent<ParticleSystem>();
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = ObtenerMaterialParticulas();
        renderer.sortingLayerID = sortingLayerId;
        renderer.sortingOrder = sortingOrder;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;

        var main = particulas.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.16f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(vidaMin, vidaMax);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoMin, tamanoMax);
        main.startColor = Color.white;
        main.gravityModifier = gravedad;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 72;

        var emission = particulas.emission;
        emission.enabled = false;

        var shape = particulas.shape;
        shape.enabled = false;

        var colorOverLifetime = particulas.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.9f, 1f, 0.76f), 0.32f),
                new GradientColorKey(new Color(0.48f, 0.58f, 0.18f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.94f, 0f),
                new GradientAlphaKey(0.82f, 0.34f),
                new GradientAlphaKey(0.24f, 0.78f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradiente);

        var sizeOverLifetime = particulas.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curvaTamano = new AnimationCurve();
        curvaTamano.AddKey(0f, 0.58f);
        curvaTamano.AddKey(0.16f, 1f);
        curvaTamano.AddKey(0.72f, 0.82f);
        curvaTamano.AddKey(1f, 0.12f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curvaTamano);

        go.transform.SetParent(null, true);
        return particulas;
    }

    private void EmitirRadial(
        ParticleSystem particulas,
        int cantidad,
        float velocidadMin,
        float velocidadMax,
        Color[] paleta,
        float escalaVertical,
        float profundidad)
    {
        if (particulas == null)
        {
            return;
        }

        particulas.transform.position = transform.position;
        particulas.transform.SetParent(transform, true);
        particulas.Play();

        for (int i = 0; i < cantidad; i++)
        {
            float angulo = (i + UnityEngine.Random.Range(-0.42f, 0.42f)) / cantidad * Mathf.PI * 2f;
            float velocidad = UnityEngine.Random.Range(velocidadMin, velocidadMax);
            Vector3 direccion = new Vector3(
                Mathf.Cos(angulo),
                Mathf.Sin(angulo) * escalaVertical,
                UnityEngine.Random.Range(-profundidad, profundidad));
            direccion.Normalize();

            ParticleSystem.EmitParams parametros = new ParticleSystem.EmitParams
            {
                velocity = direccion * velocidad,
                startColor = paleta[UnityEngine.Random.Range(0, paleta.Length)]
            };
            particulas.Emit(parametros, 1);
        }
    }

    private static void ObtenerOrden(Unidad origen, out int sortingLayerId, out int sortingOrder)
    {
        sortingLayerId = 0;
        sortingOrder = 85;
        Canvas canvas = origen != null ? origen.GetComponentInChildren<Canvas>(true) : null;
        if (canvas != null)
        {
            sortingLayerId = canvas.sortingLayerID;
            sortingOrder = canvas.sortingOrder + 8;
        }
    }

    private static Material ObtenerMaterialParticulas()
    {
        if (materialParticulas != null)
        {
            return materialParticulas;
        }

        Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }
        if (shader == null)
        {
            return null;
        }

        materialParticulas = new Material(shader)
        {
            name = "PustulaExplosion_Particulas",
            hideFlags = HideFlags.HideAndDontSave
        };
        if (materialParticulas.HasProperty("_MainTex"))
        {
            materialParticulas.mainTexture = ObtenerTexturaGotaSuave();
        }
        if (materialParticulas.HasProperty("_Color"))
        {
            materialParticulas.color = Color.white;
        }
        return materialParticulas;
    }

    private static Material ObtenerMaterialAnillo()
    {
        if (materialAnillo != null)
        {
            return materialAnillo;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            return ObtenerMaterialParticulas();
        }

        materialAnillo = new Material(shader)
        {
            name = "PustulaExplosion_Anillo",
            hideFlags = HideFlags.HideAndDontSave
        };
        return materialAnillo;
    }

    private static Sprite ObtenerSpriteGotaSuave()
    {
        if (spriteGotaSuave == null)
        {
            Texture2D textura = ObtenerTexturaGotaSuave();
            spriteGotaSuave = Sprite.Create(
                textura,
                new Rect(0f, 0f, textura.width, textura.height),
                new Vector2(0.5f, 0.5f),
                64f);
            spriteGotaSuave.name = "PustulaExplosion_GotaSuave";
            spriteGotaSuave.hideFlags = HideFlags.HideAndDontSave;
        }
        return spriteGotaSuave;
    }

    private static Texture2D ObtenerTexturaGotaSuave()
    {
        if (texturaGotaSuave != null)
        {
            return texturaGotaSuave;
        }

        const int tamano = 64;
        texturaGotaSuave = new Texture2D(tamano, tamano, TextureFormat.RGBA32, false, true)
        {
            name = "PustulaExplosion_TexturaGota",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        float centro = (tamano - 1) * 0.5f;
        for (int y = 0; y < tamano; y++)
        {
            for (int x = 0; x < tamano; x++)
            {
                float nx = (x - centro) / centro;
                float ny = (y - centro) / centro;
                float distancia = Mathf.Sqrt(nx * nx + ny * ny);
                float alpha = Mathf.Clamp01(1f - distancia);
                alpha = alpha * alpha * (3f - 2f * alpha);
                texturaGotaSuave.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texturaGotaSuave.Apply(false, false);
        return texturaGotaSuave;
    }
}
