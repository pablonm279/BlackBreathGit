using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Habilidad pasiva otorgada por "Estoque de la Medianoche" (Duelista, Epico).
// Sigue el mismo patron que las habilidades "REPRESENTACION" del proyecto:
// es un componente pasivo (no se activa desde la UI), que al equiparse aplica
// un Buff permanente usando el mismo mecanismo que ya usan ClaseDuelista/ClaseCanalizador
// (new Buff(); ...; buff.AplicarBuff(unidad); ComponentCopier.CopyComponent(...)).
public class MedianocheFilo : Habilidad
{
    private bool buffAplicado;

    public override void Awake()
    {
        nombre = "Filo de la Medianoche";
        esHostil = false;
        esDiscreta = true;

        Usuario = gameObject;
        scEstaUnidad = Usuario != null ? Usuario.GetComponent<Unidad>() : null;

        ActualizarDescripcion();
        AplicarBuffPasivoSiCorresponde();
    }

    private void AplicarBuffPasivoSiCorresponde()
    {
        // Defensivo: si por algun motivo este componente se instancia sobre un
        // GameObject que todavia no tiene Unidad (por ejemplo, al equipar desde
        // una pantalla que no sea de batalla), no hace nada en vez de romper.
        if (buffAplicado || scEstaUnidad == null)
        {
            return;
        }

        Buff buff = new Buff();
        buff.buffNombre = "Filo de la Medianoche";
        buff.buffDescr = "El Estoque de la Medianoche afila cada estocada.";
        buff.boolfDebufftBuff = true;
        buff.esStackeable = false;
        buff.DuracionBuffRondas = -1; // permanente mientras el item este equipado
        buff.cantCritDado = 1;
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
            txtDescripcion = "<color=#5dade2><b>Midnight Edge</b></color>\n\n" +
                "<i>(Passive) The blade remembers every duel it has ended. +1 Critical die, +5% damage.</i>";
            return;
        }

        if (esPortugues)
        {
            txtDescripcion = "<color=#5dade2><b>Fio da Meia-Noite</b></color>\n\n" +
                "<i>(Passiva) A lamina lembra cada duelo que encerrou. +1 dado de critico, +5% de dano.</i>";
            return;
        }

        txtDescripcion = "<color=#5dade2><b>Filo de la Medianoche</b></color>\n\n" +
            "<i>(Pasiva) La hoja recuerda cada duelo que termino. +1 al dado de critico, +5% de daño.</i>";
    }

    public override void Activar()
    {
        // Pasiva: no tiene activacion manual.
    }

    public override void AplicarEfectosHabilidad(object unidad, int tirada, Casilla casillaOrigenTrampa)
    {
        // Pasiva: no aplica efectos al impactar.
    }
}
