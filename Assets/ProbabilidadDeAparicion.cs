using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbabilidadDeAparicion : MonoBehaviour
{
    [Range(0, 100)]
    public int probabilidad; // Probabilidad en porcentaje (0-100)

    void Start()
    {
        int dado = Random.Range(1, 101); // Genera un número aleatorio entre 1 y 100
        if (dado > probabilidad)
        {
            Destroy(gameObject); // Elimina el objeto si no cumple la probabilidad
        }
    }
}
