using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaccionRiposte : Reaccion
{
    private bool intercepcionEnCurso;
    private Estocada estocada;
    private const float DuracionSwapVisual = 0.3f;
    private const float PausaFinalSwapVisual = 0.08f;
    private const int PausaPreviaContraReaccionMs = 420;

    void Start()
    {
        TipoTrigger = 1;
        usos = NIVEL == 5 ? 2 : 1;
        permanente = false;
        scEstaUnidad = GetComponent<Unidad>();
        estocada = GetComponent<Estocada>();
        ActualizarDescripcion();
    }

    public override async void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
    {
        if (!melee || usos <= 0 || scEstaUnidad == null || uTriggerer == null || scEstaUnidad.HP_actual <= 0)
        {
            intercepcionEnCurso = false;
            return;
        }

        uTriggerer.EstablecerAPActualA(0);
        if (PausaPreviaContraReaccionMs > 0)
        {
            await BattleManager.DelayCombateAsync(PausaPreviaContraReaccionMs);
        }
        scEstaUnidad.ReproducirAnimacionAtaque();

        float delay = 0.6f;
        UnidadPoseController pose = scEstaUnidad.GetComponent<UnidadPoseController>();
        if (pose != null)
        {
            delay = pose.duracionPoseAtacar;
        }

        int ms = Mathf.RoundToInt(Mathf.Max(0.1f, delay * 0.5f) * 1000f);
        await BattleManager.DelayCombateAsync(ms);

        if (estocada == null)
        {
            estocada = scEstaUnidad.GetComponent<Estocada>();
        }

        if (estocada != null && uTriggerer.HP_actual > 0)
        {
            int tirada = Random.Range(1, 21);
            estocada.EjecutarRiposteContra(uTriggerer, tirada, ObtenerBonusAtaqueReaccion(), -2);
        }

        string unidadNombre = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;
        string verboReacciona = TRADU.i != null ? TRADU.i.Traducir("reacciona con ") : "reacciona con ";
        string nombreHab = TRADU.i != null ? TRADU.i.Traducir(nombre) : nombre;
        BattleManager.Instance.EscribirLog(unidadNombre + " " + verboReacciona + nombreHab + ".");

        ConsumirUso();
    }

    public bool PuedeInterceptar(Unidad aliadoProtegido, Unidad atacante)
    {
        if (usos <= 0 || scEstaUnidad == null || aliadoProtegido == null || atacante == null)
        {
            return false;
        }

        if (scEstaUnidad.HP_actual <= 0 || aliadoProtegido.HP_actual <= 0)
        {
            return false;
        }

        if (scEstaUnidad.CasillaPosicion == null || aliadoProtegido.CasillaPosicion == null || atacante.CasillaPosicion == null)
        {
            return false;
        }

        if (aliadoProtegido.estado_inmovil > 0 || scEstaUnidad.estado_inmovil > 0)
        {
            return false;
        }

        if (aliadoProtegido == scEstaUnidad || aliadoProtegido.CasillaPosicion.lado != scEstaUnidad.CasillaPosicion.lado)
        {
            return false;
        }

        if (scEstaUnidad.bGrande || aliadoProtegido.bGrande)
        {
            return false;
        }

        if (atacante.CasillaPosicion.lado == scEstaUnidad.CasillaPosicion.lado)
        {
            return false;
        }

        int dx = Mathf.Abs(scEstaUnidad.CasillaPosicion.posX - aliadoProtegido.CasillaPosicion.posX);
        int dy = Mathf.Abs(scEstaUnidad.CasillaPosicion.posY - aliadoProtegido.CasillaPosicion.posY);
        return dx <= 1 && dy <= 1 && (dx != 0 || dy != 0);
    }

    public bool Interceptar(Unidad aliadoProtegido, Unidad atacante)
    {
        if (!PuedeInterceptar(aliadoProtegido, atacante))
        {
            return false;
        }

        Casilla casillaDuelista = scEstaUnidad.CasillaPosicion;
        Casilla casillaAliado = aliadoProtegido.CasillaPosicion;
        if (casillaDuelista == null || casillaAliado == null)
        {
            return false;
        }

        IntercambiarCasillas(scEstaUnidad, casillaDuelista, aliadoProtegido, casillaAliado);
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.MarcarUnidadComoParticipanteDuranteOscurecedor(scEstaUnidad);
        }

        intercepcionEnCurso = true;

        string nombreDuelista = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;
        string nombreAliado = TRADU.i != null ? TRADU.i.Traducir(aliadoProtegido.uNombre) : aliadoProtegido.uNombre;
        string texto = TRADU.i != null && TRADU.i.nIdioma == 2
            ? nombreDuelista + " intercepts the attack for " + nombreAliado + "."
            : TRADU.i != null && TRADU.i.nIdioma == 3
                ? nombreDuelista + " intercepta o ataque por " + nombreAliado + "."
                : nombreDuelista + " intercepta el ataque por " + nombreAliado + ".";
        BattleManager.Instance.EscribirLog(texto);

        return true;
    }

    private void IntercambiarCasillas(Unidad duelista, Casilla casillaDuelista, Unidad aliado, Casilla casillaAliado)
    {
        if (duelista == null || aliado == null || casillaDuelista == null || casillaAliado == null)
        {
            return;
        }

        Vector3 posicionInicialDuelista = duelista.transform.position;
        Vector3 posicionInicialAliado = aliado.transform.position;
        Vector3 posicionFinalDuelista = casillaAliado.transform.position;
        Vector3 posicionFinalAliado = casillaDuelista.transform.position;

        casillaDuelista.Presente = aliado.gameObject;
        casillaAliado.Presente = duelista.gameObject;

        aliado.CasillaForzadoaMover = null;
        aliado.CasillaDeseadaMov = null;
        duelista.CasillaForzadoaMover = null;
        duelista.CasillaDeseadaMov = null;

        aliado.CasillaPosicion = casillaDuelista;
        duelista.CasillaPosicion = casillaAliado;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ActualizarCasillasMelee();
        }

        casillaDuelista.ActualizarSenialadores();
        casillaAliado.ActualizarSenialadores();
        aliado.ChequearSeMovio();
        duelista.ChequearSeMovio();

        StartCoroutine(AnimarIntercambioVisual(duelista, aliado, posicionInicialDuelista, posicionFinalDuelista, posicionInicialAliado, posicionFinalAliado));
    }

    private IEnumerator AnimarIntercambioVisual(Unidad duelista, Unidad aliado, Vector3 origenDuelista, Vector3 destinoDuelista, Vector3 origenAliado, Vector3 destinoAliado)
    {
        UnidadPoseController poseDuelista = duelista != null ? duelista.GetComponent<UnidadPoseController>() : null;
        UnidadPoseController poseAliado = aliado != null ? aliado.GetComponent<UnidadPoseController>() : null;

        if (poseDuelista != null)
        {
            poseDuelista.OnStartMove();
        }

        if (poseAliado != null)
        {
            poseAliado.OnStartMove();
        }

        float tiempo = 0f;
        while (tiempo < DuracionSwapVisual)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / DuracionSwapVisual);

            if (duelista != null)
            {
                duelista.transform.position = Vector3.Lerp(origenDuelista, destinoDuelista, t);
            }

            if (aliado != null)
            {
                aliado.transform.position = Vector3.Lerp(origenAliado, destinoAliado, t);
            }

            yield return null;
        }

        if (duelista != null)
        {
            duelista.transform.position = destinoDuelista;
        }

        if (aliado != null)
        {
            aliado.transform.position = destinoAliado;
        }

        if (poseDuelista != null)
        {
            poseDuelista.OnStopMove();
        }

        if (poseAliado != null)
        {
            poseAliado.OnStopMove();
        }

        if (PausaFinalSwapVisual > 0f)
        {
            yield return new WaitForSeconds(PausaFinalSwapVisual);
        }
    }

    public int ObtenerBonoDefensaIntercepcion()
    {
        return NIVEL > 1 ? 3 : 2;
    }

    public void ProcesarDanioRecibido()
    {
        if (NIVEL != 4)
        {
            CancelarPorDanio();
            return;
        }

        if (intercepcionEnCurso)
        {
            ConsumirUso();
            return;
        }

        intercepcionEnCurso = false;
    }

    public void CancelarPorDanio()
    {
        intercepcionEnCurso = false;
        if (this != null)
        {
            Destroy(this);
        }
    }

    public static bool TryPrepararIntercepcion(Unidad atacante, Unidad objetivoOriginal, bool esMelee, bool objetivoUnico, out Unidad objetivoReal, out float defensaObjetivo)
    {
        objetivoReal = objetivoOriginal;
        defensaObjetivo = objetivoOriginal != null ? objetivoOriginal.ObtenerdefensaActual() : 0f;

        Unidad.LimpiarRedireccionAtaque();

        if (!esMelee || !objetivoUnico || atacante == null || objetivoOriginal == null || objetivoOriginal.CasillaPosicion == null)
        {
            return false;
        }

        List<Casilla> casillasAdyacentes = objetivoOriginal.CasillaPosicion.ObtenerCasillasAlrededor(2);
        foreach (Casilla casilla in casillasAdyacentes)
        {
            if (casilla == null || casilla.Presente == null)
            {
                continue;
            }

            Unidad duelista = casilla.Presente.GetComponent<Unidad>();
            if (duelista == null)
            {
                continue;
            }

            ReaccionRiposte reaccion = duelista.GetComponent<ReaccionRiposte>();
            if (reaccion == null || !reaccion.Interceptar(objetivoOriginal, atacante))
            {
                continue;
            }

            objetivoReal = duelista;
            defensaObjetivo = objetivoReal.ObtenerdefensaActual() + reaccion.ObtenerBonoDefensaIntercepcion();
            Unidad.RegistrarRedireccionAtaque(atacante, objetivoOriginal, objetivoReal);
            return true;
        }

        return false;
    }

    private int ObtenerBonusAtaqueReaccion()
    {
        int bonus = -1;
        if (NIVEL > 2)
        {
            bonus += 1;
        }

        return bonus;
    }

    private void ConsumirUso()
    {
        intercepcionEnCurso = false;
        usos--;
        if (usos <= 0)
        {
            Destroy(this);
        }
    }

    private void ActualizarDescripcion()
    {
        if (TRADU.i == null)
        {
            descripcion = "Riposte";
            return;
        }

        if (TRADU.i.nIdioma == 2)
        {
            descripcion = "Reaction: swaps with an adjacent ally targeted by a single-target melee attack, gains Defense for that hit and counterattacks with Thrust on melee misses.";
        }
        else if (TRADU.i.nIdioma == 3)
        {
            descripcion = "Reacao: troca com um aliado adjacente alvo de um ataque corpo a corpo unitario, ganha Defesa para esse golpe e contra-ataca com Estocada quando erram nela.";
        }
        else
        {
            descripcion = "Reacción: intercambia con un aliado adyacente objetivo de un ataque melee unitario, gana Defensa para ese golpe y contraataca con Estocada cuando fallan contra ella.";
        }
    }
}
