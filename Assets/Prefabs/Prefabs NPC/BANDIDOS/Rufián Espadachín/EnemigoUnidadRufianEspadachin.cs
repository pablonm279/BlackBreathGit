using UnityEngine;

public class EnemigoUnidadRufianEspadachin : Unidad
{
    public override void ComienzoBatallaEnemigo()
    {
        base.ComienzoBatallaEnemigo();
        estado_evasion = Mathf.Max(estado_evasion, 1);

        ReaccionOportunistaRufian reaccion = GetComponent<ReaccionOportunistaRufian>();
        if (reaccion == null)
        {
            reaccion = gameObject.AddComponent<ReaccionOportunistaRufian>();
        }

        reaccion.RestaurarUso();
    }

    public override void ActualizarClaseComienzoTurno()
    {
        base.ActualizarClaseComienzoTurno();

        ReaccionOportunistaRufian reaccion = GetComponent<ReaccionOportunistaRufian>();
        if (reaccion != null)
        {
            reaccion.RestaurarUso();
        }
    }
}
