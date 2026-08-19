using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase concreta para la linea de armaduras tipo Manto del Canalizador.
// Mismo patron que armaduraPesadaCaballero.cs: una unica clase reusada
// por todos los prefabs de la linea (Comun a Epico).
public class ArmaduraMantoCanalizador : Armadura
{
    void Awake()
    {
        string nmejora = nivelMejora > 0 ? "+" + nivelMejora : "";

        if (!string.IsNullOrEmpty(nmejora) && !string.IsNullOrEmpty(sNombreItem) && !sNombreItem.Contains(nmejora))
        {
            sNombreItem = sNombreItem + " " + nmejora;
        }

        AgregarStatsArmaduraaDescripcion();
    }
}
