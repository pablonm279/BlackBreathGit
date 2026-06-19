using UnityEngine;

public class ReaccionPresenciaProvocadora : Reaccion
{
    private const int PausaPreviaContraReaccionMs = 420;
    private Estocada estocada;

    void Start()
    {
        TipoTrigger = 1;
        usos = -1;
        permanente = false;
        scEstaUnidad = GetComponent<Unidad>();
        estocada = GetComponent<Estocada>();
        if (string.IsNullOrEmpty(nombre))
        {
            nombre = "Presencia Provocadora";
        }

        ActualizarDescripcion();
    }

    public override async void AplicarEfectos(Unidad uTriggerer, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
    {
        if (!melee || scEstaUnidad == null || uTriggerer == null || scEstaUnidad.HP_actual <= 0f || uTriggerer.HP_actual <= 0f)
        {
            return;
        }

        if (scEstaUnidad.CasillaPosicion == null || uTriggerer.CasillaPosicion == null || scEstaUnidad.CasillaPosicion.lado == uTriggerer.CasillaPosicion.lado)
        {
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

        if (estocada != null && uTriggerer.HP_actual > 0f)
        {
            int tirada = Random.Range(1, 21);
            estocada.EjecutarRiposteContra(uTriggerer, tirada, ObtenerBonusAtaqueReaccion(), ObtenerBonusDanioPlanoReaccion());
        }

        string unidadNombre = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;
        string verboReacciona = TRADU.i != null ? TRADU.i.Traducir("reacciona con ") : "reacciona con ";
        string nombreHab = TRADU.i != null ? TRADU.i.Traducir(nombre) : nombre;
        BattleManager.Instance.EscribirLog(unidadNombre + " " + verboReacciona + nombreHab + ".");
    }

    private void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaIngles;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == TRADU.IdiomaPortugues;
        int bonusAtaque = ObtenerBonusAtaqueReaccion();
        int bonusDanio = ObtenerBonusDanioPlanoReaccion();

        if (esIngles)
        {
            descripcion = $"Reaction: if an enemy misses a melee attack against her, counterattacks with base Thrust ({bonusAtaque:+#;-#;0} Attack, {bonusDanio:+#;-#;0} Damage). Unlimited uses until next turn.";
        }
        else if (esPortugues)
        {
            descripcion = $"Reacao: se um inimigo errar um ataque corpo a corpo contra ela, contra-ataca com Estocada base ({bonusAtaque:+#;-#;0} Ataque, {bonusDanio:+#;-#;0} Dano). Usos ilimitados ate o proximo turno.";
        }
        else
        {
            descripcion = $"Reacción: si un enemigo falla un ataque melee contra ella, contraataca con Estocada base ({bonusAtaque:+#;-#;0} Ataque, {bonusDanio:+#;-#;0} Daño). Usos ilimitados hasta el próximo turno.";
        }
    }

    private int ObtenerBonusAtaqueReaccion()
    {
        return NIVEL == 4 ? 0 : -1;
    }

    private int ObtenerBonusDanioPlanoReaccion()
    {
        return NIVEL == 4 ? 0 : -2;
    }
}
