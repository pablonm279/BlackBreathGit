using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class delayActivar : MonoBehaviour
{
    public float tiempoDelay = 0.1f;

    void Awake()
    {
        Invoke("Activar", tiempoDelay);
        gameObject.SetActive(false);
    }

    void Activar()
    {
        gameObject.SetActive(true);
    }
}