using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Habilidad pasiva otorgada por "Guantelete de Sobrecarga Controlada" (Canalizador, Raro).
public class SobrecargaControlada : Habilidad
{
    private bool buffAplicado;

    public override void Awake()
    {
        nombre = "Sobrecarga Controlada";
        esHostil = false;
        esDiscreta = true;

        Usuario = gameObject;
        scEstaUnidad = Usuario != null ? Usuario.GetComponent<Unidad>() : null;

        ActualizarDescripcion();
        AplicarBuffPasivoSiCorresponde();
    }

    private void AplicarBuffPasivoSiCorresponde()
    {
        if (buffAplicado || scEstaUnidad == null)
        {
            return;
        }

        Buff buff = new Buff();
        buff.buffNombre = "Sobrecarga Controlada";
        buff.buffDescr = "El guantelete deja escapar un poco de energía arcana en cada golpe.";
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = -1;
        buff.cantDanioPorcentaje = 5;
        buff.AplicarBuff(scEstaUnidad);
        ComponentCopier.CopyComponent(buff, scEstaUnidad.gameObject);

        buffAplicado = true;
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

        if (esIngles)
        {
            txtDescripcion = "<color=#5dade2><b>Controlled Overload</b></color>\n\n" +
                "<i>(Passive) Lets a trickle of arcane energy leak into every strike. +5% damage.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Sobrecarga Controlada</b></color>\n\n" +
                "<i>(Passiva) Libera um pouco de energia arcana em cada golpe. +5% de dano.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Sobrecarga Controlada</b></color>\n\n" +
            "<i>(Pasiva) Deja escapar un poco de energía arcana en cada golpe. +5% de daño.</i>";
    }

    public override void Activar()
    {
    }

    public override void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa)
    {
    }
}
