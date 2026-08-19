using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Habilidad pasiva otorgada por "Manto de Disipacion" (Canalizador, Raro).
public class DisipacionArcana : Habilidad
{
    private bool buffAplicado;

    public override void Awake()
    {
        nombre = "Disipación Arcana";
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
        buff.buffNombre = "Disipación Arcana";
        buff.buffDescr = "El manto dispersa parte de la energía arcana antes de que desestabilice al Canalizador.";
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = -1;
        buff.cantDefensa = 1;
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
            txtDescripcion = "<color=#5dade2><b>Arcane Dispersal</b></color>\n\n" +
                "<i>(Passive) The mantle bleeds off excess arcane energy before it destabilizes the Channeler. +1 Defense.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Dissipacao Arcana</b></color>\n\n" +
                "<i>(Passiva) O manto dispersa energia arcana antes que desestabilize o Canalizador. +1 Defesa.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Disipación Arcana</b></color>\n\n" +
            "<i>(Pasiva) El manto dispersa energía arcana antes de que desestabilice al Canalizador. +1 Defensa.</i>";
    }

    public override void Activar()
    {
    }

    public override void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa)
    {
    }
}
