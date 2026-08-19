using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Habilidad pasiva otorgada por "Manto del Nucleo Inestable" (Canalizador, Epico).
public class NucleoInestable : Habilidad
{
    private bool buffAplicado;

    public override void Awake()
    {
        nombre = "Núcleo Inestable";
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
        buff.buffNombre = "Núcleo Inestable";
        buff.buffDescr = "El núcleo del manto absorbe parte del desgaste de canalizar demasiada energía.";
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = -1;
        buff.cantDefensa = 2;
        buff.cantAPMax = 1;
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
            txtDescripcion = "<color=#5dade2><b>Unstable Core</b></color>\n\n" +
                "<i>(Passive) The mantle's core soaks up some of the strain of channeling too much power. +2 Defense, +1 Max AP.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Nucleo Instavel</b></color>\n\n" +
                "<i>(Passiva) O nucleo do manto absorve parte do desgaste de canalizar energia demais. +2 Defesa, +1 PA Max.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Núcleo Inestable</b></color>\n\n" +
            "<i>(Pasiva) El núcleo del manto absorbe parte del desgaste de canalizar demasiada energía. +2 Defensa, +1 PA Máx.</i>";
    }

    public override void Activar()
    {
    }

    public override void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa)
    {
    }
}
