using System.Threading.Tasks;
using UnityEngine;

public class ClaseDuelista : Unidad
{
    public int PASIVA_AtaquesReveladores;
    public int PASIVA_EvasionMaestra;
    private int usosEvasionMaestraEsteTurno;

    public override void ComienzoBatallaClase()
    {
        base.ComienzoBatallaClase();
        ResetearPasoLigero();
        ResetearPosturaDemandante();
        ResetearEvasionMaestra();
    }

    public override void ActualizarClaseComienzoTurno()
    {
        base.ActualizarClaseComienzoTurno();
        ResetearPasoLigero();
        ResetearPosturaDemandante();
        ResetearEvasionMaestra();
    }

    public override async void RecibirDanio(float danio, int tipoDanio, bool esCritico, Unidad uCausante, int delayEfectos = 0)
    {
        float maxHpAlRecibir = mod_maxHP;
        bool eraSuTurnoAlRecibir = BattleManager.Instance != null && BattleManager.Instance.unidadActiva == this;
        bool activaPosturaDemandante = DebeActivarsePosturaDemandante(danio, tipoDanio, uCausante);
        if (activaPosturaDemandante)
        {
            ConsumirPosturaDemandante();
        }

        float hpAntes = HP_actual;
        base.RecibirDanio(danio, tipoDanio, esCritico, uCausante, delayEfectos);
        ProcesarRiposteAlRecibirDanio(hpAntes);

        if (!activaPosturaDemandante)
        {
            return;
        }

        await BattleManager.DelayCombateAsync(Mathf.Max(20, delayEfectos + 20));

        if (HP_actual <= 0)
        {
            return;
        }

        float danioRecibido = hpAntes - HP_actual;
        if (danioRecibido <= maxHpAlRecibir * 0.2f)
        {
            return;
        }

        AplicarTambaleando(eraSuTurnoAlRecibir);
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
        if (nivel > 1)
        {
            Estados.Aplicar_MovimientoAbaratado(this, 1, this);
        }

        if (nivel > 2)
        {
            estado_evasion += 1;
            if (BattleManager.Instance != null && BattleManager.Instance.scUIInfoChar != null)
            {
                BattleManager.Instance.scUIInfoChar.ActualizarInfoChar(this);
            }
        }

        if (nivel == 5 && atacante != null && atacante.HP_actual > 0f)
        {
            const int dcReflejos = 10;
            bool fallaTSReflejos = atacante.TiradaSalvacion(atacante.mod_TSReflejos, dcReflejos);
            if (fallaTSReflejos)
            {
                AplicarTambaleandoEvasionMaestra(atacante);
            }
        }
    }

    private bool DebeActivarsePosturaDemandante(float danio, int tipoDanio, Unidad uCausante)
    {
        if (tipoDanio < 1 || tipoDanio > 3)
        {
            return false;
        }

        if (!PuedeUsarPosturaDemandante())
        {
            return false;
        }

        if (uCausante == null || BattleManager.Instance == null || BattleManager.Instance.HabilidadActiva == null)
        {
            return false;
        }

        return BattleManager.Instance.HabilidadActiva.scEstaUnidad == uCausante && BattleManager.Instance.HabilidadActiva.esMelee && danio > mod_maxHP * 0.2f;
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
}
