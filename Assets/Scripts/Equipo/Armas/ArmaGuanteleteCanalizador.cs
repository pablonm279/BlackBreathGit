using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Clase concreta para la linea de armas tipo Guantelete del Canalizador.
// Sigue el mismo patron que armaduraPesadaCaballero.cs / armaEstoque.cs:
// una unica clase reusada por todos los prefabs de esta linea (Comun a Epico),
// donde los datos particulares de cada item viven en el prefab, no en el codigo.
public class ArmaGuanteleteCanalizador : Arma
{
    void Awake()
    {
        string nmejora = nivelMejora > 0 ? "+" + nivelMejora : "";

        if (!string.IsNullOrEmpty(nmejora) && !string.IsNullOrEmpty(sNombreItem) && !sNombreItem.Contains(nmejora))
        {
            sNombreItem = sNombreItem + " " + nmejora;
        }

        AgregarStatsArmaaDescripcion();
    }
}
