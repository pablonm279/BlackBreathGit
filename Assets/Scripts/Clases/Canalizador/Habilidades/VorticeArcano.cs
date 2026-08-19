using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Habilidad pasiva otorgada por "Guantelete del Vortice Arcano" (Canalizador, Epico).
// Al equiparse, adelanta un nivel de Energia usando la misma API publica que ya
// usa ReaccionSifonArcano.cs (ClaseCanalizador.CambiarEnergia), asi que reutiliza
// una ruta ya probada en el proyecto en vez de tocar los campos privados de la clase.
public class VorticeArcano : Habilidad
{
    private bool efectoAplicado;

    public override void Awake()
    {
        nombre = "Vórtice Arcano";
        esHostil = false;
        esDiscreta = true;

        Usuario = gameObject;
        scEstaUnidad = Usuario != null ? Usuario.GetComponent<Unidad>() : null;

        ActualizarDescripcion();
        AplicarEfectoPasivoSiCorresponde();
    }

    private void AplicarEfectoPasivoSiCorresponde()
    {
        if (efectoAplicado || scEstaUnidad == null)
        {
            return;
        }

        // Defensivo: solo actua si quien lo equipa es realmente un Canalizador.
        if (scEstaUnidad is ClaseCanalizador canalizador)
        {
            canalizador.CambiarEnergia(1);
        }

        efectoAplicado = true;
    }

    public override void ActualizarDescripcion()
    {
        bool esIngles = TRADU.i != null && TRADU.i.nIdioma == 2;
        bool esPortugues = TRADU.i != null && TRADU.i.nIdioma == 3;

        if (esIngles)
        {
            txtDescripcion = "<color=#5dade2><b>Arcane Vortex</b></color>\n\n" +
                "<i>(Passive) The gauntlet starts every battle already humming with power: begins combat with 1 Energy Tier.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Vortice Arcano</b></color>\n\n" +
                "<i>(Passiva) A manopla comeca cada batalha ja carregada: inicia o combate com 1 Nivel de Energia.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Vórtice Arcano</b></color>\n\n" +
            "<i>(Pasiva) El guantelete empieza cada batalla ya cargado: comienza el combate con 1 Nivel de Energía.</i>";
    }

    public override void Activar()
    {
    }

    public override void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa)
    {
    }
}
