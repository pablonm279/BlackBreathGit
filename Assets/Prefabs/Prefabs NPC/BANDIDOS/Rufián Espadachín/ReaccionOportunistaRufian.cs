using UnityEngine;

public class ReaccionOportunistaRufian : Reaccion
{
    private const int PausaContraataqueMs = 360;
    private IAEspadaLargaRufian espada;

    private void Awake()
    {
        TipoTrigger = 1;
        permanente = true;
        nombre = "Oportunista";
        descripcion = "Reacción: cuando un enemigo falla un ataque melee contra esta unidad, contraataca con su espada larga. Un uso por ronda.";
        scEstaUnidad = GetComponent<Unidad>();
        espada = GetComponent<IAEspadaLargaRufian>();
        RestaurarUso();
    }

    public void RestaurarUso()
    {
        usos = 1;
    }

    public override async void AplicarEfectos(Unidad atacante, bool melee, float variableFlexible1 = 0, float variableFlexible2 = 0)
    {
        if (!melee || usos <= 0 || atacante == null || atacante.HP_actual <= 0 || scEstaUnidad == null || scEstaUnidad.HP_actual <= 0)
        {
            return;
        }

        usos--;
        scEstaUnidad.ReproducirAnimacionAtaque();
        await BattleManager.DelayCombateAsync(PausaContraataqueMs);

        if (espada == null) espada = GetComponent<IAEspadaLargaRufian>();
        if (espada == null || atacante == null || atacante.HP_actual <= 0) return;

        espada.EjecutarContraataque(atacante, Random.Range(1, 21));

        string nombreUnidad = TRADU.i != null ? TRADU.i.Traducir(scEstaUnidad.uNombre) : scEstaUnidad.uNombre;
        string nombreReaccion = TRADU.i != null ? TRADU.i.Traducir(nombre) : nombre;
        BattleManager.Instance.EscribirLog(nombreUnidad + " " + (TRADU.i != null ? TRADU.i.Traducir("reacciona con ") : "reacciona con ") + nombreReaccion + ".");
    }
}
