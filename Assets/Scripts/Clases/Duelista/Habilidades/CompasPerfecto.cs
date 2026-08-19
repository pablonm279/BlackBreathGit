using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Habilidad pasiva otorgada por "Gambeson del Compas Perfecto" (Duelista, Epico).
public class CompasPerfecto : Habilidad
{
    private bool buffAplicado;

    public override void Awake()
    {
        nombre = "Compás Perfecto";
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
        buff.buffNombre = "Compás Perfecto";
        buff.buffDescr = "El Gambeson del Compás Perfecto mantiene la guardia siempre firme.";
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = -1;
        buff.cantDefensa = 2;
        buff.cantCritDadoRecibido = -1; // mas dificil de criticar
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
            txtDescripcion = "<color=#5dade2><b>Perfect Compass</b></color>\n\n" +
                "<i>(Passive) A footwork so precise it never leaves an opening. +2 Defense, harder to be critically hit.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Compasso Perfeito</b></color>\n\n" +
                "<i>(Passiva) Um jogo de pernas tao preciso que nunca deixa brechas. +2 Defesa, mais dificil de sofrer critico.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Compás Perfecto</b></color>\n\n" +
            "<i>(Pasiva) Un juego de piernas tan preciso que nunca deja una abertura. +2 Defensa, más difícil de recibir críticos.</i>";
    }

    public override void Activar()
    {
    }

    public override void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa)
    {
    }
}
