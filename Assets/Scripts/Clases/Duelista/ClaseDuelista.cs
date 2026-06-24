using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class AparienciaAlternativaDuelista
{
    public string nombre;
    public Sprite retrato;
    public Sprite poseIdle;
    public Sprite poseMover;
    public Sprite poseAtacar;
    public Sprite poseHabilidad;
    public Sprite poseRecibirDanio;
    public Sprite poseTurnoActivo;
    public Sprite poseEnGarde;

    public bool TieneContenido()
    {
        return retrato != null || poseIdle != null || poseMover != null || poseAtacar != null || poseHabilidad != null || poseRecibirDanio != null || poseTurnoActivo != null || poseEnGarde != null;
    }
}

public class ClaseDuelista : Unidad
{
    private const string BuffNombreEnGarde = "En Garde";
    private const string BuffNombreDanzando = "Danzando";

    public int PASIVA_AtaquesReveladores;
    public int PASIVA_EvasionMaestra;
    public int PASIVA_DanzaDelEstoque;
    private int usosEvasionMaestraEsteTurno;
    private int bonusEvasionEnGardeAplicado;
    private bool poseEnGardeActiva;
    private Sprite poseIdleOriginal;
    private Sprite poseTurnoActivoOriginal;
    private Sprite poseEnGardeAlternativaActiva;
    private UnidadPoseController poseControllerDuelista;

    public Sprite Pose_Engarde;
    public System.Collections.Generic.List<AparienciaAlternativaDuelista> aparienciasAlternativas = new System.Collections.Generic.List<AparienciaAlternativaDuelista>();

    public override void AplicarAparienciaAlternativaAleatoria()
    {
        AplicarAparienciaAlternativaPorIndice(ElegirIndiceAparienciaAlternativaAleatoria());
    }

    public override void AplicarAparienciaAlternativaPorIndice(int indiceApariencia)
    {
        if (poseControllerDuelista == null)
        {
            poseControllerDuelista = GetComponent<UnidadPoseController>();
        }

        if (poseControllerDuelista == null)
        {
            return;
        }

        AparienciaAlternativaDuelista aparienciaElegida = ObtenerAparienciaAlternativaDuelista(indiceApariencia);
        poseEnGardeAlternativaActiva = aparienciaElegida != null ? aparienciaElegida.poseEnGarde : null;
        if (aparienciaElegida == null)
        {
            poseControllerDuelista.RestaurarPosesBase();
            return;
        }

        Sprite poseIdleBase = poseControllerDuelista.ObtenerPoseIdleBase() != null ? poseControllerDuelista.ObtenerPoseIdleBase() : (uImage != null ? uImage.sprite : null);
        Sprite poseMoverBase = poseControllerDuelista.ObtenerPoseMoverBase() != null ? poseControllerDuelista.ObtenerPoseMoverBase() : poseIdleBase;
        Sprite poseAtacarBase = poseControllerDuelista.ObtenerPoseAtacarBase() != null ? poseControllerDuelista.ObtenerPoseAtacarBase() : poseIdleBase;
        Sprite poseHabilidadBase = poseControllerDuelista.ObtenerPoseHabilidadBase() != null ? poseControllerDuelista.ObtenerPoseHabilidadBase() : poseIdleBase;
        Sprite poseRecibirDanioBase = poseControllerDuelista.ObtenerPoseRecibirDanioBase();
        Sprite poseTurnoActivoBase = poseControllerDuelista.ObtenerPoseTurnoActivoBase();

        Sprite poseIdle = aparienciaElegida.poseIdle != null ? aparienciaElegida.poseIdle : poseIdleBase;
        Sprite poseMover = aparienciaElegida.poseMover != null ? aparienciaElegida.poseMover : poseMoverBase;
        Sprite poseAtacar = aparienciaElegida.poseAtacar != null ? aparienciaElegida.poseAtacar : poseAtacarBase;
        Sprite poseHabilidad = aparienciaElegida.poseHabilidad != null ? aparienciaElegida.poseHabilidad : poseHabilidadBase;
        Sprite poseRecibirDanio = aparienciaElegida.poseRecibirDanio != null ? aparienciaElegida.poseRecibirDanio : poseRecibirDanioBase;
        Sprite poseTurnoActivo = aparienciaElegida.poseTurnoActivo != null ? aparienciaElegida.poseTurnoActivo : poseTurnoActivoBase;

        poseControllerDuelista.ConfigurarPoses(poseIdle, poseMover, poseAtacar, poseHabilidad, poseRecibirDanio, poseTurnoActivo);
    }

    public override int ObtenerCantidadAparienciasAlternativas()
    {
        return 1 + ObtenerAparienciasAlternativasDuelistaValidas().Count;
    }

    public override bool EsIndiceAparienciaAlternativaValido(int indiceApariencia)
    {
        return indiceApariencia == Personaje.IndiceAparienciaBase || ObtenerAparienciaAlternativaDuelista(indiceApariencia) != null;
    }

    public override Sprite ObtenerRetratoAparienciaAlternativa(int indiceApariencia)
    {
        AparienciaAlternativaDuelista apariencia = ObtenerAparienciaAlternativaDuelista(indiceApariencia);
        return apariencia != null ? apariencia.retrato : null;
    }

    public override System.Collections.Generic.List<int> ObtenerIndicesAparienciasAlternativasDisponibles()
    {
        System.Collections.Generic.List<int> indicesDisponibles = new System.Collections.Generic.List<int> { Personaje.IndiceAparienciaBase };
        if (aparienciasAlternativas == null || aparienciasAlternativas.Count == 0)
        {
            return indicesDisponibles;
        }

        for (int i = 0; i < aparienciasAlternativas.Count; i++)
        {
            AparienciaAlternativaDuelista apariencia = aparienciasAlternativas[i];
            if (apariencia != null && apariencia.TieneContenido())
            {
                indicesDisponibles.Add(i);
            }
        }

        return indicesDisponibles;
    }

    AparienciaAlternativaDuelista ObtenerAparienciaAlternativaDuelista(int indiceApariencia)
    {
        if (aparienciasAlternativas == null || indiceApariencia < 0 || indiceApariencia >= aparienciasAlternativas.Count)
        {
            return null;
        }

        AparienciaAlternativaDuelista apariencia = aparienciasAlternativas[indiceApariencia];
        return apariencia != null && apariencia.TieneContenido() ? apariencia : null;
    }

    System.Collections.Generic.List<AparienciaAlternativaDuelista> ObtenerAparienciasAlternativasDuelistaValidas()
    {
        System.Collections.Generic.List<AparienciaAlternativaDuelista> aparienciasValidas = new System.Collections.Generic.List<AparienciaAlternativaDuelista>();
        if (aparienciasAlternativas == null || aparienciasAlternativas.Count == 0)
        {
            return aparienciasValidas;
        }

        for (int i = 0; i < aparienciasAlternativas.Count; i++)
        {
            AparienciaAlternativaDuelista apariencia = aparienciasAlternativas[i];
            if (apariencia != null && apariencia.TieneContenido())
            {
                aparienciasValidas.Add(apariencia);
            }
        }

        return aparienciasValidas;
    }

    public override int ElegirIndiceAparienciaAlternativaAleatoria()
    {
        System.Collections.Generic.List<AparienciaAlternativaDuelista> aparienciasValidas = ObtenerAparienciasAlternativasDuelistaValidas();
        if (aparienciasValidas.Count == 0)
        {
            return Personaje.IndiceAparienciaBase;
        }

        int opcionElegida = UnityEngine.Random.Range(0, aparienciasValidas.Count + 1);
        if (opcionElegida == 0)
        {
            return Personaje.IndiceAparienciaBase;
        }

        AparienciaAlternativaDuelista aparienciaElegida = aparienciasValidas[opcionElegida - 1];
        return aparienciasAlternativas.IndexOf(aparienciaElegida);
    }

    public override void ComienzoBatallaClase()
    {
        base.ComienzoBatallaClase();
        ResetearPasoLigero();
        ResetearPosturaDemandante();
        ResetearEvasionMaestra();
        SincronizarEnGardeSegunBuffActual();
    }

    public override void ActualizarClaseComienzoTurno()
    {
        base.ActualizarClaseComienzoTurno();
        ResetearPasoLigero();
        ResetearPosturaDemandante();
        ResetearEvasionMaestra();
        SincronizarEnGardeSegunBuffActual();
    }

    public override async void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0, bool ignorarEscudo = false)
    {
        float maxHpAlRecibir = mod_maxHP;
        bool eraSuTurnoAlRecibir = BattleManager.Instance != null && BattleManager.Instance.unidadActiva == this;
        bool puedeDispararPosturaDemandante = DebeActivarsePosturaDemandante();
        float porcentajeUmbralPosturaDemandante = ObtenerUmbralPosturaDemandante(TieneBuffNombre(BuffNombreEnGarde));

        float hpAntes = HP_actual;
        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos, ignorarEscudo);
       
        
        await EsperarResolucionDanioAsync(hpAntes, delayEfectos);
        
        
        await Task.Delay(1500); // Asegura que cualquier cambio de HP se haya procesado antes de continuar
             float danioRecibido = hpAntes - HP_actual;
         if (danioRecibido > 0f && TieneBuffNombre(BuffNombreEnGarde))
        {
            RemoverBuffNombre(BuffNombreEnGarde);
            NotificarFinEnGarde();
        }
             ProcesarRiposteAlRecibirDanio(hpAntes);

             bool activaPosturaDemandante = false;
             if (puedeDispararPosturaDemandante)
             {    

                 float umbralPosturaDemandante = maxHpAlRecibir * porcentajeUmbralPosturaDemandante;
                 if (danioRecibido > umbralPosturaDemandante)
               {
                activaPosturaDemandante = ConsumirPosturaDemandante();
               }
             }

             if (!activaPosturaDemandante)
             {
                 return;
             }

             if (HP_actual <= 0)
             {
                 return;
             }

             AplicarTambaleando(eraSuTurnoAlRecibir);
    }

    private async Task EsperarResolucionDanioAsync(float hpAntes, int delayEfectos)
    {
        int esperaMaximaMs = Mathf.Max(80, delayEfectos + 80);
        int esperaAcumuladaMs = 0;

        while (esperaAcumuladaMs < esperaMaximaMs)
        {
            if (HP_actual < hpAntes || HP_actual <= 0f)
            {
                return;
            }

            await BattleManager.DelayCombateAsync(20);
            esperaAcumuladaMs += 20;
        }
    }

    public override void FalloAtaqueRecibido(Unidad uOrigen, bool melee)
    {
        base.FalloAtaqueRecibido(uOrigen, melee);
        IntentarActivarEvasionMaestra(uOrigen, melee);
    }

    public bool PuedeUsarPasoLigero()
    {
        REPRESENTACIONPasoLigero pasoLigero = GetComponent<REPRESENTACIONPasoLigero>();
        return pasoLigero != null && !pasoLigero.seusoEsteTurno;
    }

    public bool ConsumirPasoLigero()
    {
        REPRESENTACIONPasoLigero pasoLigero = GetComponent<REPRESENTACIONPasoLigero>();
        if (pasoLigero == null || pasoLigero.seusoEsteTurno)
        {
            return false;
        }

        pasoLigero.seusoEsteTurno = true;
        return true;
    }

    public bool PuedeUsarPosturaDemandante()
    {
        REPRESENTACIONPosturaDemandante posturaDemandante = GetComponent<REPRESENTACIONPosturaDemandante>();
        return posturaDemandante != null && !posturaDemandante.seusoEsteTurno;
    }

    public bool ConsumirPosturaDemandante()
    {
        REPRESENTACIONPosturaDemandante posturaDemandante = GetComponent<REPRESENTACIONPosturaDemandante>();
        if (posturaDemandante == null || posturaDemandante.seusoEsteTurno)
        {
            return false;
        }

        posturaDemandante.seusoEsteTurno = true;
        return true;
    }

    public override void OcasionoDanioaEnemigo(Unidad victima, int tipoDanio, bool esCritico, float danio)
    {
        base.OcasionoDanioaEnemigo(victima, tipoDanio, esCritico, danio);

        if (PASIVA_AtaquesReveladores <= 0 || victima == null || danio <= 0f || victima.HP_actual <= 0f)
        {
            return;
        }

        AplicarVulnerabilidadExpuesta(victima);
    }

    public override void AcabaDeMatarUnidad(Unidad uVictima)
    {
        base.AcabaDeMatarUnidad(uVictima);

        if (!DebeActivarDanzaDelEstoqueAlMatar(uVictima))
        {
            return;
        }

        CambiarAPActual(ObtenerApGanadosDanzaDelEstoque());
        AplicarDanzando();
    }

    private void ResetearPasoLigero()
    {
        REPRESENTACIONPasoLigero pasoLigero = GetComponent<REPRESENTACIONPasoLigero>();
        if (pasoLigero != null)
        {
            pasoLigero.seusoEsteTurno = false;
        }
    }

    private void ResetearPosturaDemandante()
    {
        REPRESENTACIONPosturaDemandante posturaDemandante = GetComponent<REPRESENTACIONPosturaDemandante>();
        if (posturaDemandante != null)
        {
            posturaDemandante.seusoEsteTurno = false;
        }
    }

    private void ResetearEvasionMaestra()
    {
        usosEvasionMaestraEsteTurno = 0;
    }

    private void IntentarActivarEvasionMaestra(Unidad atacante, bool melee)
    {
        if (GetComponent<ReaccionRiposte>() != null || TieneBuffNombre("Riposte"))
        {
            return;
        }

        int nivel = ObtenerNivelEvasionMaestra();
        if (nivel <= 0 || !EsAtaqueMeleeParaEvasionMaestra(atacante, melee))
        {
            return;
        }

        if (!PuedeUsarEvasionMaestra(nivel))
        {
            return;
        }

        Casilla destino = ObtenerDestinoEvasionMaestra(atacante);
        if (destino == null)
        {
            return;
        }

        usosEvasionMaestraEsteTurno++;
        CasillaForzadoaMover = destino;
        CasillaDeseadaMov = null;
        ForzarSiguienteMovimientoForzadoInmediato();

        if (atacante != null)
        {
            atacante.EstablecerAPActualA(0);
        }

        AplicarBonosEvasionMaestra(nivel, atacante);
    }

    private async void ProcesarRiposteAlRecibirDanio(float hpAntes)
    {
        ReaccionRiposte reaccionRiposte = GetComponent<ReaccionRiposte>();
        if (reaccionRiposte == null || HP_actual >= hpAntes)
        {
            return;
        }

        bool mostrarTextoCancelacion = reaccionRiposte.NIVEL != 4;
        reaccionRiposte.ProcesarDanioRecibido();

        if (mostrarTextoCancelacion && gameObject != null)
        {
            await GenerarTextoFlotante("<s>" + TRADU.i.Traducir("Riposte") + "</s>", Color.blue);
        }
    }

    private bool PuedeUsarEvasionMaestra(int nivel)
    {
        int usosMaximos = nivel == 4 ? 2 : 1;
        return usosEvasionMaestraEsteTurno < usosMaximos;
    }

    private int ObtenerNivelEvasionMaestra()
    {
        if (PASIVA_EvasionMaestra > 0)
        {
            return PASIVA_EvasionMaestra;
        }

        REPRESENTACIONEvasionMaestra representacion = GetComponent<REPRESENTACIONEvasionMaestra>();
        return representacion != null ? representacion.NIVEL : 0;
    }

    public int ObtenerBonusDanioPorcentajeDanzaDelEstoque(Unidad objetivo)
    {
        int nivel = ObtenerNivelDanzaDelEstoque();
        if (nivel <= 0 || !ObjetivoEstaEnUmbralDanzaDelEstoque(objetivo))
        {
            return 0;
        }

        int bonus = 25;
        if (nivel > 2)
        {
            bonus += 5;
        }

        return bonus;
    }

    private bool EsAtaqueMeleeParaEvasionMaestra(Unidad atacante, bool melee)
    {
        if (melee)
        {
            return true;
        }

        if (BattleManager.Instance == null || BattleManager.Instance.HabilidadActiva == null || atacante == null)
        {
            return false;
        }

        Habilidad habilidadActiva = BattleManager.Instance.HabilidadActiva;
        return habilidadActiva.scEstaUnidad == atacante && habilidadActiva.esMelee;
    }

    private Casilla ObtenerDestinoEvasionMaestra(Unidad atacante)
    {
        if (CasillaPosicion == null || CasillaPosicion.ladoGO == null)
        {
            return null;
        }

        LadoManager lado = CasillaPosicion.ladoGO.GetComponent<LadoManager>();
        if (lado == null)
        {
            return null;
        }

        int direccionPreferida = ObtenerDireccionAtrasEvasionMaestra(lado, atacante);
        if (direccionPreferida == 0)
        {
            direccionPreferida = -1;
        }

        Casilla destinoPreferido = ObtenerDestinoEvasionMaestraPorDireccion(lado, direccionPreferida);
        if (destinoPreferido != null)
        {
            return destinoPreferido;
        }

        return ObtenerDestinoEvasionMaestraPorDireccion(lado, -direccionPreferida);
    }

    private int ObtenerDireccionAtrasEvasionMaestra(LadoManager lado, Unidad atacante)
    {
        if (lado == null || atacante == null || atacante.CasillaPosicion == null || CasillaPosicion == null)
        {
            return 0;
        }

        Vector3 posicionActual = CasillaPosicion.transform.position;
        Vector3 vectorAtras = posicionActual - atacante.CasillaPosicion.transform.position;
        if (vectorAtras.sqrMagnitude < 0.0001f)
        {
            return 0;
        }

        vectorAtras.Normalize();

        int direccionElegida = 0;
        float mejorScore = float.NegativeInfinity;
        int[] direcciones = { -1, 1 };
        for (int i = 0; i < direcciones.Length; i++)
        {
            int direccion = direcciones[i];
            Casilla candidata = lado.ObtenerCasillaPorIndex(CasillaPosicion.posX + direccion, CasillaPosicion.posY);
            if (candidata == null)
            {
                continue;
            }

            Vector3 desplazamiento = candidata.transform.position - posicionActual;
            if (desplazamiento.sqrMagnitude < 0.0001f)
            {
                continue;
            }

            desplazamiento.Normalize();
            float score = Vector3.Dot(desplazamiento, vectorAtras);
            if (score > mejorScore)
            {
                mejorScore = score;
                direccionElegida = direccion;
            }
        }

        return direccionElegida;
    }

    private Casilla ObtenerDestinoEvasionMaestraPorDireccion(LadoManager lado, int direccionX)
    {
        if (lado == null || CasillaPosicion == null || direccionX == 0)
        {
            return null;
        }

        int destinoX = CasillaPosicion.posX + direccionX;
        Casilla casillaAtras = lado.ObtenerCasillaPorIndex(destinoX, CasillaPosicion.posY);
        if (EsCasillaValidaParaEvasionMaestra(casillaAtras))
        {
            return casillaAtras;
        }

        Casilla diagonalInferior = lado.ObtenerCasillaPorIndex(destinoX, CasillaPosicion.posY - 1);
        Casilla diagonalSuperior = lado.ObtenerCasillaPorIndex(destinoX, CasillaPosicion.posY + 1);
        bool inferiorValida = EsCasillaValidaParaEvasionMaestra(diagonalInferior);
        bool superiorValida = EsCasillaValidaParaEvasionMaestra(diagonalSuperior);

        if (inferiorValida && superiorValida)
        {
            return UnityEngine.Random.Range(0, 2) == 0 ? diagonalInferior : diagonalSuperior;
        }
        if (inferiorValida)
        {
            return diagonalInferior;
        }
        if (superiorValida)
        {
            return diagonalSuperior;
        }

        return null;
    }

    private bool EsCasillaValidaParaEvasionMaestra(Casilla casilla)
    {
        return casilla != null && casilla.Presente == null && casilla.GetComponent<Trampa>() == null;
    }

    private void AplicarBonosEvasionMaestra(int nivel, Unidad atacante)
    {
        if (nivel > 0)
        {
            Estados.Aplicar_MovimientoAbaratado(this, 1, this, false);
              if (BattleManager.Instance != null && BattleManager.Instance.scUIInfoChar != null)
            {
                BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(this);
            }
        }

        if (nivel > 1)
        {
            estado_evasion += 1;
            if (BattleManager.Instance != null && BattleManager.Instance.scUIInfoChar != null)
            {
                BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(this);
            }
        }

        if (nivel == 5 && atacante != null && atacante.HP_actual > 0f)
        {
            const int dcReflejos = 10;
            bool fallaTSReflejos = atacante.TiradaSalvacion(2, dcReflejos);
            if (fallaTSReflejos)
            {
                AplicarTambaleandoEvasionMaestra(atacante);
            }
        }
    }

    private bool DebeActivarsePosturaDemandante()
    {
        if (!PuedeUsarPosturaDemandante())
        {
            return false;
        }

        return true;
    }

    public void NotificarInicioEnGarde()
    {
        AplicarBonusEvasionEnGarde();
        ActualizarPoseEnGarde(true);
    }

    public void NotificarFinEnGarde()
    {
        RemoverBonusEvasionEnGarde();
        ActualizarPoseEnGarde(false);
    }

    private void SincronizarEnGardeSegunBuffActual()
    {
        if (TieneBuffNombre(BuffNombreEnGarde))
        {
            NotificarInicioEnGarde();
            return;
        }

        NotificarFinEnGarde();
    }

    private float ObtenerUmbralPosturaDemandante(bool teniaEnGarde)
    {
        if (!teniaEnGarde)
        {
            return 0.2f;
        }

        EnGarde enGarde = GetComponent<EnGarde>();
        if (enGarde != null && enGarde.NIVEL == 4)
        {
            return 0.10f;
        }

        return 0.05f;
    }

    private int ObtenerBonusEvasionEnGarde()
    {
        int bonus = 2;
        EnGarde enGarde = GetComponent<EnGarde>();
        if (enGarde != null && enGarde.NIVEL > 1)
        {
            bonus += 1;
        }

        return bonus;
    }

    private void AplicarBonusEvasionEnGarde()
    {
        RemoverBonusEvasionEnGarde();

        bonusEvasionEnGardeAplicado = ObtenerBonusEvasionEnGarde();
        if (bonusEvasionEnGardeAplicado <= 0)
        {
            return;
        }

        estado_evasion += bonusEvasionEnGardeAplicado;
        if (BattleManager.Instance != null && BattleManager.Instance.scUIInfoChar != null)
        {
            BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(this);
        }
    }

    private void RemoverBonusEvasionEnGarde()
    {
        if (bonusEvasionEnGardeAplicado <= 0)
        {
            return;
        }

        estado_evasion = Mathf.Max(0, estado_evasion - bonusEvasionEnGardeAplicado);
        bonusEvasionEnGardeAplicado = 0;
        if (BattleManager.Instance != null && BattleManager.Instance.scUIInfoChar != null)
        {
            BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(this);
        }
    }

    private void ActualizarPoseEnGarde(bool activar)
    {
        if (poseControllerDuelista == null)
        {
            poseControllerDuelista = GetComponent<UnidadPoseController>();
        }

        if (poseControllerDuelista == null)
        {
            return;
        }

        Sprite poseEnGarde = poseEnGardeAlternativaActiva != null ? poseEnGardeAlternativaActiva : Pose_Engarde;
        if (poseEnGarde == null)
        {
            return;
        }

        if (activar)
        {
            if (!poseEnGardeActiva)
            {
                poseIdleOriginal = poseControllerDuelista.poseIdle;
                poseTurnoActivoOriginal = poseControllerDuelista.poseTurnoActivo;
            }

            poseControllerDuelista.poseIdle = poseEnGarde;
            poseControllerDuelista.poseTurnoActivo = poseEnGarde;
            poseEnGardeActiva = true;
            poseControllerDuelista.SetIdle();
            return;
        }

        if (poseEnGardeActiva)
        {
            poseControllerDuelista.poseIdle = poseIdleOriginal;
            poseControllerDuelista.poseTurnoActivo = poseTurnoActivoOriginal;
            poseControllerDuelista.RefrescarPoseActual();
        }

        poseEnGardeActiva = false;
    }

    private void AplicarTambaleando(bool eraSuTurnoAlRecibir)
    {
        if (TieneBuffNombre("Tambaleando"))
        {
            return;
        }

        Buff buff = new Buff();
        buff.buffNombre = "Tambaleando";
        buff.boolfDebufftBuff = false;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = eraSuTurnoAlRecibir ? 2 : 1;
        buff.cantAPMax -= 1;
        buff.cantDefensa -= 2;
        buff.AplicarBuff(this);
        ComponentCopier.CopyComponent(buff, gameObject);
        RefrescarEstadoTambaleando();
    }

    private void AplicarTambaleandoEvasionMaestra(Unidad objetivo)
    {
        if (objetivo == null || objetivo.TieneBuffNombre("Tambaleando"))
        {
            return;
        }

        Buff buff = new Buff();
        buff.buffNombre = "Tambaleando";
        buff.boolfDebufftBuff = false;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = 1;
        buff.cantAPMax -= 1;
        buff.cantDefensa -= 2;
        buff.AplicarBuff(objetivo);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);
    }

    private void RefrescarEstadoTambaleando()
    {
        if (BattleManager.Instance == null)
        {
            return;
        }

        if (BattleManager.Instance.unidadActiva == this && ObtenerAPActual() > mod_maxAccionP)
        {
            EstablecerAPActualA(Mathf.FloorToInt(mod_maxAccionP));
        }

        if (BattleManager.Instance.scUIInfoChar != null)
        {
            BattleManager.Instance.scUIInfoChar.RefrescarSiVisible(this);
        }
    }

    private void AplicarVulnerabilidadExpuesta(Unidad objetivo)
    {
        Buff buff = new Buff();
        buff.buffNombre = "Vulnerabilidad Expuesta";
        buff.boolfDebufftBuff = false;
        buff.esStackeable = true;
        buff.DuracionBuffRondas = ObtenerDuracionAtaquesReveladores();
        buff.cantCritDadoRecibido = ObtenerBonusCritDadoAtaquesReveladores();
        buff.cantAumentoDanioCriticoRecibidoPorcentaje = ObtenerBonusDanioCriticoRecibidoAtaquesReveladores();
        buff.AplicarBuff(objetivo);
        ComponentCopier.CopyComponent(buff, objetivo.gameObject);
    }

    private int ObtenerDuracionAtaquesReveladores()
    {
        return PASIVA_AtaquesReveladores == 5 ? 3 : 2;
    }

    private int ObtenerBonusCritDadoAtaquesReveladores()
    {
        int bonus = 1;
        if (PASIVA_AtaquesReveladores > 2)
        {
            bonus += 1;
        }
        return bonus;
    }

    private int ObtenerBonusDanioCriticoRecibidoAtaquesReveladores()
    {
        int bonus = 0;
        if (PASIVA_AtaquesReveladores > 1)
        {
            bonus += 10;
        }
        if (PASIVA_AtaquesReveladores == 4)
        {
            bonus += 15;
        }
        return bonus;
    }

    private int ObtenerNivelDanzaDelEstoque()
    {
        if (PASIVA_DanzaDelEstoque > 0)
        {
            return PASIVA_DanzaDelEstoque;
        }

        REPRESENTACIONDanzaDelEstoque representacion = GetComponent<REPRESENTACIONDanzaDelEstoque>();
        return representacion != null ? representacion.NIVEL : 0;
    }

    private bool ObjetivoEstaEnUmbralDanzaDelEstoque(Unidad objetivo)
    {
        if (objetivo == null || objetivo.HP_actual <= 0f || objetivo.mod_maxHP <= 0f)
        {
            return false;
        }

        return objetivo.HP_actual <= objetivo.mod_maxHP * ObtenerUmbralDanzaDelEstoque();
    }

    private float ObtenerUmbralDanzaDelEstoque()
    {
        float umbral = 0.20f;
        if (ObtenerNivelDanzaDelEstoque() > 1)
        {
            umbral += 0.05f;
        }

        return umbral;
    }

    private bool DebeActivarDanzaDelEstoqueAlMatar(Unidad victima)
    {
        if (victima == null || ObtenerNivelDanzaDelEstoque() <= 0 || BattleManager.Instance == null || BattleManager.Instance.unidadActiva != this)
        {
            return false;
        }

        return CasillaPosicion != null
            && victima.CasillaPosicion != null
            && CasillaPosicion.lado != victima.CasillaPosicion.lado;
    }

    private int ObtenerApGanadosDanzaDelEstoque()
    {
        return ObtenerNivelDanzaDelEstoque() == 4 ? 4 : 3;
    }

    private void AplicarDanzando()
    {
        Buff buff = new Buff();
        buff.buffNombre = BuffNombreDanzando;
        buff.buffDescr = ObtenerDescripcionBuffDanzando();
        buff.boolfDebufftBuff = true;
        buff.esStackeable = true;
        buff.DuracionBuffRondas = 1;
        buff.cantAtaque = 1;
        buff.cantDanioPorcentaje = 15;

        if (ObtenerNivelDanzaDelEstoque() == 5)
        {
            buff.cantCritDado = 1;
        }

        buff.AplicarBuff(this);
        ComponentCopier.CopyComponent(buff, gameObject);
    }

    private string ObtenerDescripcionBuffDanzando()
    {
        if (ObtenerNivelDanzaDelEstoque() == 5)
        {
            return "Encadena bajas por este turno: +1 Ataque, +15% Daño y +5% Crítico.";
        }

        return "Encadena bajas por este turno: +1 Ataque y +15% Daño.";
    }
}
